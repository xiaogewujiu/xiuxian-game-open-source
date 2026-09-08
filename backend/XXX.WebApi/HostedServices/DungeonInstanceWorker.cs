using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Data;

namespace XXX.WebApi.HostedServices
{
    /// <summary>
    /// 秘境实例后台推进任务。
    /// 定时检查所有运行中的秘境实例，触发 tick 事件。
    /// </summary>
    public sealed class DungeonInstanceWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<DungeonInstanceWorker> _logger;

        public DungeonInstanceWorker(IServiceScopeFactory serviceScopeFactory, ILogger<DungeonInstanceWorker> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 启动延迟，等待其他服务初始化
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

            // 启动时执行停机补算
            await CatchUpMissedTicksAsync(stoppingToken);

            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessRunningInstancesAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Dungeon instance worker tick failed.");
                }

                if (!await timer.WaitForNextTickAsync(stoppingToken))
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 停机补算：处理所有错过 tick 的实例。
        /// </summary>
        private async Task CatchUpMissedTicksAsync(CancellationToken stoppingToken)
        {
            try
            {
                await using var scope = _serviceScopeFactory.CreateAsyncScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();
                var dungeonService = scope.ServiceProvider.GetRequiredService<IDungeonInstanceService>();

                var runningInstances = await dbContext.Db.Queryable<DungeonInstanceEntity>()
                    .Where(i => i.Status == (int)DungeonInstanceStatus.Running)
                    .ToListAsync();

                if (runningInstances.Count == 0)
                {
                    return;
                }

                _logger.LogInformation("Dungeon instance catch-up: found {Count} running instances", runningInstances.Count);

                foreach (var instance in runningInstances)
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }

                    try
                    {
                        // 检查是否超过开放时间
                        if (GameData.DungeonInstanceTemplates.TryGetValue(instance.DungeonId, out var template))
                        {
                            if (!IsInOpenSchedule(template))
                            {
                                await dungeonService.SettleAsync(instance.Id.ToString(), (int)DungeonSettleReason.ForceClose);
                                _logger.LogInformation("Force-closed dungeon instance {InstanceId} (out of schedule)", instance.Id);
                                continue;
                            }
                        }

                        // 计算错过的 tick 数
                        if (instance.LastTickTime.HasValue)
                        {
                            var tickInterval = GetTickInterval(instance.DungeonId);
                            var missedDuration = DateTime.UtcNow - instance.LastTickTime.Value;
                            var missedTicks = (int)Math.Floor(missedDuration.TotalSeconds / tickInterval);

                            // 最多补算 10 个 tick，避免长时间停机后过度处理
                            missedTicks = Math.Min(missedTicks, 10);

                            for (var i = 0; i < missedTicks; i++)
                            {
                                if (stoppingToken.IsCancellationRequested)
                                {
                                    break;
                                }

                                await dungeonService.ProcessTickAsync(instance.Id.ToString());

                                // 检查实例是否已结算
                                var current = await dbContext.Db.Queryable<DungeonInstanceEntity>()
                                    .Where(x => x.Id == instance.Id)
                                    .FirstAsync();
                                if (current == null || current.Status != (int)DungeonInstanceStatus.Running)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to catch-up dungeon instance {InstanceId}", instance.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Dungeon instance catch-up failed.");
            }
        }

        /// <summary>
        /// 处理所有到达 tick 时间的运行中实例。
        /// </summary>
        private async Task ProcessRunningInstancesAsync(CancellationToken stoppingToken)
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();
            var dungeonService = scope.ServiceProvider.GetRequiredService<IDungeonInstanceService>();

            var now = DateTime.UtcNow;
            var dueInstances = await dbContext.Db.Queryable<DungeonInstanceEntity>()
                .Where(i => i.Status == (int)DungeonInstanceStatus.Running &&
                            i.NextTickTime != null &&
                            i.NextTickTime <= now)
                .ToListAsync();

            foreach (var instance in dueInstances)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    await dungeonService.ProcessTickAsync(instance.Id.ToString());
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to process tick for dungeon instance {InstanceId}", instance.Id);
                }
            }
        }

        private static bool IsInOpenSchedule(DungeonInstanceTemplateEntity template)
        {
            if (string.IsNullOrEmpty(template.OpenScheduleJson))
            {
                return true;
            }

            try
            {
                var schedule = System.Text.Json.JsonSerializer.Deserialize<OpenScheduleConfig>(template.OpenScheduleJson);
                if (schedule == null || schedule.Schedules == null || schedule.Schedules.Count == 0)
                {
                    return true;
                }

                var dateTimeNow = DateTime.Now;
                var currentDay = (int)dateTimeNow.DayOfWeek;
                var currentMinutes = dateTimeNow.Hour * 60 + dateTimeNow.Minute;

                foreach (var s in schedule.Schedules)
                {
                    if (s.Days != null && s.Days.Count > 0 && !s.Days.Contains(currentDay))
                    {
                        continue;
                    }

                    var startMinutes = s.StartHour * 60 + s.StartMinute;
                    var endMinutes = s.EndHour * 60 + s.EndMinute;

                    if (currentMinutes >= startMinutes && currentMinutes <= endMinutes)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return true;
            }
        }

        private static int GetTickInterval(string dungeonId)
        {
            if (GameData.DungeonInstanceTemplates.TryGetValue(dungeonId, out var template))
            {
                return Math.Max(5, template.TickIntervalSeconds);
            }
            return 30;
        }

        private class OpenScheduleConfig
        {
            public List<ScheduleEntry>? Schedules { get; set; }
        }

        private class ScheduleEntry
        {
            public List<int>? Days { get; set; }
            public int StartHour { get; set; }
            public int StartMinute { get; set; }
            public int EndHour { get; set; }
            public int EndMinute { get; set; }
        }
    }
}
