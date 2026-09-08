using System.Text.Json;
using SqlSugar;
using XXX.Entity;
using XXX.Infrastructure.Data;
using XXX.Infrastructure.Repositories;

namespace XXX.WebApi.HostedServices
{
    /// <summary>
    /// 宗门任务每日重置后台服务。
    /// 每天 0:00 自动重置所有宗门任务：清除旧进度，为宗门成员创建新的"进行中"记录。
    /// 重置后的状态流转：进行中 → 待领取（完成后） → 已完成（领取后） → 次日重置为进行中。
    /// </summary>
    public sealed class DailySectTaskResetWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DailySectTaskResetWorker> _logger;

        /// <summary>
        /// 记录上次重置日期，避免同一天重复执行。
        /// </summary>
        private DateTime _lastResetDate = DateTime.MinValue;

        public DailySectTaskResetWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<DailySectTaskResetWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 启动后等待 10 秒，避免与其它初始化任务冲突。
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;

                    // 每天 0:00 ~ 0:05 窗口内触发一次。
                    if (now.Hour == 0 && now.Minute <= 5 && _lastResetDate.Date != now.Date)
                    {
                        await using var scope = _scopeFactory.CreateAsyncScope();
                        var db = scope.ServiceProvider.GetRequiredService<DbContext>().Db;

                        await ResetSectDailyTasksAsync(db, now);

                        _lastResetDate = now.Date;
                        _logger.LogInformation("宗门任务每日重置完成，日期：{Date}", now.ToString("yyyy-MM-dd"));
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "宗门任务每日重置 Tick 执行失败。");
                }

                if (!await timer.WaitForNextTickAsync(stoppingToken))
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 重置所有宗门任务（QuestType=5）的进度和完成记录，
        /// 并为宗门成员创建新的"进行中"进度记录。
        /// </summary>
        private async Task ResetSectDailyTasksAsync(ISqlSugarClient db, DateTime now)
        {
            // 1. 查询所有启用的宗门任务 ID。
            var sectQuestIds = await db.Queryable<QuestConfigEntity>()
                .Where(q => q.QuestType == 5 && q.IsEnabled)
                .Select(q => q.QuestId)
                .ToListAsync();

            if (sectQuestIds.Count == 0)
            {
                _logger.LogInformation("没有启用的宗门任务，跳过重置。");
                return;
            }

            // 2. 删除所有宗门任务的进度记录。
            var deletedProgress = await db.Deleteable<QuestProgressEntity>()
                .Where(p => sectQuestIds.Contains(p.QuestId))
                .ExecuteCommandAsync();

            // 3. 删除所有宗门任务的完成记录。
            var deletedCompleted = await db.Deleteable<QuestCompletedRecordEntity>()
                .Where(c => sectQuestIds.Contains(c.QuestId))
                .ExecuteCommandAsync();

            // 4. 查询所有宗门成员（有公会且公会绑定宗门的玩家）。
            var memberPlayerIds = await db.Queryable<GuildMemberEntity>()
                .InnerJoin<GuildEntity>((m, g) => m.GuildId == g.GID && !g.IsDeleted && g.SectTemplateId != null && g.SectTemplateId != "")
                .Select((m, g) => m.PlayerId)
                .Distinct()
                .ToListAsync();

            if (memberPlayerIds.Count == 0)
            {
                _logger.LogInformation("没有宗门成员，跳过进度创建。已删除 {Progress} 条进度，{Completed} 条完成记录。",
                    deletedProgress, deletedCompleted);
                return;
            }

            // 5. 为每位宗门成员创建新的"进行中"进度记录（Status=1）。
            var nowStr = now.ToString("yyyy-MM-dd HH:mm:ss");
            var newRecords = new List<QuestProgressEntity>();

            foreach (var playerId in memberPlayerIds)
            {
                foreach (var questId in sectQuestIds)
                {
                    newRecords.Add(new QuestProgressEntity
                    {
                        GID = Guid.NewGuid().ToString("N"),
                        PlayerId = playerId,
                        QuestId = questId,
                        CurrentStage = 0,
                        ObjectiveProgressJson = null,
                        Status = 1, // InProgress
                        AcceptTime = now,
                        LastUpdateTime = now
                    });
                }
            }

            // 分批插入，每批 500 条，避免 SQL 过长。
            const int batchSize = 500;
            for (var i = 0; i < newRecords.Count; i += batchSize)
            {
                var batch = newRecords.Skip(i).Take(batchSize).ToList();
                await db.Insertable(batch).ExecuteCommandAsync();
            }

            _logger.LogInformation(
                "宗门任务重置完成：删除 {Progress} 条旧进度，{Completed} 条旧完成记录；为 {PlayerCount} 名成员创建 {NewCount} 条新进度。",
                deletedProgress, deletedCompleted, memberPlayerIds.Count, newRecords.Count);
        }
    }
}
