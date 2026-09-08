using System.Text.Json;
using XXX.Entity;
using XXX.Infrastructure.Data;
using XXX.Infrastructure.Repositories;

namespace XXX.WebApi.HostedServices
{
    /// <summary>
    /// 宗门大比后台推进任务。
    /// 每 60 秒检查一次排期配置，到达预定时间后自动为每个宗门创建大比并执行淘汰赛。
    /// </summary>
    public sealed class SectTournamentWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<SectTournamentWorker> _logger;

        /// <summary>
        /// 记录今天是否已经触发过大比，避免同一分钟内重复创建。
        /// </summary>
        private DateTime _lastTriggerDate = DateTime.MinValue;

        public SectTournamentWorker(IServiceScopeFactory serviceScopeFactory, ILogger<SectTournamentWorker> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(60));
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await using var scope = _serviceScopeFactory.CreateAsyncScope();
                    await ProcessTickAsync(scope.ServiceProvider, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "宗门大比后台任务 Tick 执行失败。");
                }

                if (!await timer.WaitForNextTickAsync(stoppingToken))
                {
                    break;
                }
            }
        }

        private async Task ProcessTickAsync(IServiceProvider sp, CancellationToken ct)
        {
            var dbContext = sp.GetRequiredService<DbContext>();
            var scheduleRepo = sp.GetRequiredService<IRepository<SectTournamentScheduleEntity>>();
            var guildRepo = sp.GetRequiredService<IRepository<GuildEntity>>();
            var memberRepo = sp.GetRequiredService<IRepository<GuildMemberEntity>>();
            var userRepo = sp.GetRequiredService<IRepository<UserEntity>>();
            var tournamentRepo = sp.GetRequiredService<IRepository<SectTournamentEntity>>();
            var matchRepo = sp.GetRequiredService<IRepository<SectTournamentMatchEntity>>();

            // 读取排期配置
            var schedule = await scheduleRepo.GetFirstAsync(s => s.IsEnabled);
            if (schedule == null)
            {
                return;
            }

            var now = DateTime.Now;
            var today = now.Date;

            // 解析 SpawnTimeText（格式 "HH:mm"）
            if (!TimeOnly.TryParseExact(schedule.SpawnTimeText, "HH:mm", out var spawnTime))
            {
                _logger.LogWarning("宗门大比排期时间格式无效：{SpawnTimeText}", schedule.SpawnTimeText);
                return;
            }

            var scheduledDateTime = today.Add(spawnTime.ToTimeSpan());
            var endDateTime = scheduledDateTime.AddMinutes(schedule.DurationMinutes);

            // 检查当前是否在大比进行时间段内
            if (now < scheduledDateTime || now >= endDateTime)
            {
                // 如果不在时间段内，重置触发标记（新的一天）
                if (now.Date != _lastTriggerDate.Date)
                {
                    _lastTriggerDate = DateTime.MinValue;
                }

                return;
            }

            // 今天已经触发过了
            if (_lastTriggerDate.Date == today)
            {
                // 大比已到结束时间，结算未结算的大比
                if (now >= endDateTime)
                {
                    await SettleTournamentsAsync(tournamentRepo, matchRepo, guildRepo, memberRepo, userRepo, today);
                }

                return;
            }

            // 触发大比
            _lastTriggerDate = today;
            _logger.LogInformation("宗门大比开始触发，当前时间：{Now}", now);

            // 获取所有已绑定宗门的公会
            var guilds = await guildRepo.GetListAsync(g => !string.IsNullOrEmpty(g.SectTemplateId) && !g.IsDeleted);
            if (guilds.Count == 0)
            {
                _logger.LogInformation("没有已绑定宗门的公会，跳过大比。");
                return;
            }

            foreach (var guild in guilds)
            {
                try
                {
                    await CreateAndRunTournamentAsync(
                        dbContext, tournamentRepo, matchRepo, memberRepo, userRepo,
                        guild, scheduledDateTime, endDateTime, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "为公会 {GuildId} ({GuildName}) 创建宗门大比失败。", guild.GID, guild.Name);
                }
            }
        }

        /// <summary>
        /// 为指定公会创建并执行大比。
        /// </summary>
        private async Task CreateAndRunTournamentAsync(
            DbContext dbContext,
            IRepository<SectTournamentEntity> tournamentRepo,
            IRepository<SectTournamentMatchEntity> matchRepo,
            IRepository<GuildMemberEntity> memberRepo,
            IRepository<UserEntity> userRepo,
            GuildEntity guild,
            DateTime startTime,
            DateTime endTime,
            CancellationToken ct)
        {
            // 检查今天是否已经为该公会创建过大比
            var todayStr = startTime.ToString("yyyy-MM-dd");
            var existing = await tournamentRepo.GetFirstAsync(
                t => t.GuildId == guild.GID && t.StartTime >= startTime.Date && t.StartTime < startTime.Date.AddDays(1));
            if (existing != null)
            {
                _logger.LogInformation("公会 {GuildId} 今天已有大比记录，跳过。", guild.GID);
                return;
            }

            // 获取公会成员
            var members = await memberRepo.GetListAsync(m => m.GuildId == guild.GID);
            if (members.Count < 2)
            {
                _logger.LogInformation("公会 {GuildId} 成员不足 2 人，跳过大比。", guild.GID);
                return;
            }

            // 按等级和贡献排序，取前 N 名（最多 16 人，凑 2 的幂）
            var sortedMembers = members
                .OrderByDescending(m => m.PlayerLevel)
                .ThenByDescending(m => m.TotalContribution)
                .Take(16)
                .ToList();

            // 凑到最近的 2 的幂
            var bracketSize = GetNearestPowerOfTwo(sortedMembers.Count);
            while (sortedMembers.Count < bracketSize)
            {
                // 轮空占位
                sortedMembers.Add(new GuildMemberEntity
                {
                    GID = $"bye_{Guid.NewGuid():N}",
                    PlayerId = $"bye_{Guid.NewGuid():N}",
                    PlayerName = "（轮空）",
                    PlayerLevel = 0,
                    GuildId = guild.GID
                });
            }

            // 查询玩家属性数据
            var playerIds = sortedMembers
                .Where(m => !m.PlayerId.StartsWith("bye_"))
                .Select(m => m.PlayerId)
                .ToList();
            var users = new Dictionary<string, UserEntity>();
            if (playerIds.Count > 0)
            {
                var userList = await userRepo.GetListAsync(u => playerIds.Contains(u.GID));
                foreach (var u in userList)
                {
                    users[u.GID] = u;
                }
            }

            // 创建大比实体
            var tournamentId = $"tournament_{guild.GID}_{DateTime.Now:yyyyMMdd}";
            var tournament = new SectTournamentEntity
            {
                TournamentId = tournamentId,
                GuildId = guild.GID,
                StartTime = startTime,
                EndTime = endTime,
                State = 0, // 进行中
                LastUpdateTime = DateTime.Now
            };
            await tournamentRepo.AddAsync(tournament);

            // 执行淘汰赛
            var allMatches = new List<SectTournamentMatchEntity>();
            var currentRound = sortedMembers;
            var round = 1;

            while (currentRound.Count > 1)
            {
                var nextRound = new List<GuildMemberEntity>();
                for (var i = 0; i < currentRound.Count; i += 2)
                {
                    var p1 = currentRound[i];
                    var p2 = currentRound[i + 1];

                    var p1Power = CalculateCombatPower(p1, users);
                    var p2Power = CalculateCombatPower(p2, users);

                    var winnerId = ResolveMatchWinner(p1, p2, p1Power, p2Power);
                    var match = new SectTournamentMatchEntity
                    {
                        MatchId = $"{tournamentId}_r{round}_m{i / 2}",
                        TournamentId = tournamentId,
                        Player1Id = p1.PlayerId,
                        Player1Name = p1.PlayerName,
                        Player2Id = p2.PlayerId,
                        Player2Name = p2.PlayerName,
                        Round = round,
                        WinnerId = winnerId,
                        BattleLogJson = JsonSerializer.Serialize(new
                        {
                            Player1Power = p1Power,
                            Player2Power = p2Power,
                            WinnerId = winnerId
                        }),
                        MatchTime = DateTime.Now
                    };
                    allMatches.Add(match);

                    var winner = winnerId == p1.PlayerId ? p1 : p2;
                    nextRound.Add(winner);
                }

                currentRound = nextRound;
                round++;
            }

            // 批量插入对战记录
            if (allMatches.Count > 0)
            {
                await matchRepo.AddRangeAsync(allMatches);
            }

            // 标记为已结算
            tournament.State = 1; // Settled
            var finalWinner = currentRound.FirstOrDefault();
            tournament.ResultsJson = JsonSerializer.Serialize(new
            {
                WinnerId = finalWinner?.PlayerId,
                WinnerName = finalWinner?.PlayerName,
                TotalRounds = round - 1,
                TotalMatches = allMatches.Count
            });
            tournament.LastUpdateTime = DateTime.Now;
            await tournamentRepo.UpdateAsync(tournament);

            _logger.LogInformation(
                "宗门大比完成：公会 {GuildId}，大比 {TournamentId}，共 {MatchCount} 场对战，{RoundCount} 轮。冠军：{Winner}",
                guild.GID, tournamentId, allMatches.Count, round - 1, finalWinner?.PlayerName ?? "无");
        }

        /// <summary>
        /// 计算玩家战斗力（简化版）。
        /// </summary>
        private static long CalculateCombatPower(GuildMemberEntity member, Dictionary<string, UserEntity> users)
        {
            if (member.PlayerId.StartsWith("bye_"))
            {
                return 0;
            }

            if (!users.TryGetValue(member.PlayerId, out var user))
            {
                return member.PlayerLevel * 100L;
            }

            // 基础战力 = 等级 * 100
            var power = user.Level * 100L;

            // 累加 15 项属性
            power += (long)user.Type1;
            power += (long)user.Type2;
            power += (long)user.Type3;
            power += (long)user.Type4;
            power += (long)user.Type5;
            power += (long)user.Type6;
            power += (long)user.Type7;
            power += (long)(user.Type8 * 1000);
            power += (long)(user.Type9 * 1000);
            power += (long)(user.Type10 * 1000);
            power += (long)(user.Type11 * 1000);
            power += (long)(user.Type12 * 1000);
            power += (long)(user.Type13 * 1000);
            power += (long)(user.Type14 * 1000);
            power += (long)(user.Type15 * 1000);

            return power;
        }

        /// <summary>
        /// 简化 PvP：战力高者 80% 概率获胜，20% 概率爆冷。
        /// </summary>
        private static string ResolveMatchWinner(
            GuildMemberEntity p1, GuildMemberEntity p2,
            long p1Power, long p2Power)
        {
            // 轮空直接晋级
            if (p1.PlayerId.StartsWith("bye_"))
            {
                return p2.PlayerId;
            }

            if (p2.PlayerId.StartsWith("bye_"))
            {
                return p1.PlayerId;
            }

            // 战力完全相同时 50/50
            if (p1Power == p2Power)
            {
                return Random.Shared.Next(2) == 0 ? p1.PlayerId : p2.PlayerId;
            }

            var stronger = p1Power > p2Power ? p1 : p2;
            var weaker = p1Power > p2Power ? p2 : p1;

            // 强者 80% 胜率
            return Random.Shared.NextDouble() < 0.8 ? stronger.PlayerId : weaker.PlayerId;
        }

        /// <summary>
        /// 获取不小于 n 的最小 2 的幂。
        /// </summary>
        private static int GetNearestPowerOfTwo(int n)
        {
            if (n <= 0) return 1;
            var power = 1;
            while (power < n) power <<= 1;
            return power;
        }

        /// <summary>
        /// 结算已到期的大比。
        /// </summary>
        private async Task SettleTournamentsAsync(
            IRepository<SectTournamentEntity> tournamentRepo,
            IRepository<SectTournamentMatchEntity> matchRepo,
            IRepository<GuildEntity> guildRepo,
            IRepository<GuildMemberEntity> memberRepo,
            IRepository<UserEntity> userRepo,
            DateTime today)
        {
            var unsettled = await tournamentRepo.GetListAsync(
                t => t.StartTime >= today && t.StartTime < today.AddDays(1) && t.State == 0);

            foreach (var tournament in unsettled)
            {
                tournament.State = 1; // Settled
                tournament.LastUpdateTime = DateTime.Now;
                await tournamentRepo.UpdateAsync(tournament);
                _logger.LogInformation("宗门大比自动结算：{TournamentId}", tournament.TournamentId);
            }
        }
    }
}
