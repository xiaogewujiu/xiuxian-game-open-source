using Microsoft.Extensions.Logging;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Battle;
using XXX.Entity;
using XXX.Infrastructure.Data;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    public class ArenaService : IArenaService
    {
        private const string RANKING_ID = "ranking_arena";
        private const int INITIAL_POINTS = 1000;
        private const int MAX_DAILY_PURCHASES = 3;
        private static readonly int[] PurchaseCosts = [50, 100, 200];

        private readonly DbContext _dbContext;
        private readonly IRepository<ArenaPlayerEntity> _arenaPlayerRepository;
        private readonly IRepository<ArenaBattleLogEntity> _battleLogRepository;
        private readonly IRepository<UserEntity> _userRepository;
        private readonly IPlayerAttributeService _playerAttributeService;
        private readonly IRankingService _rankingService;

        private readonly IMailService _mailService;
        private readonly ITitleService _titleService;
        private readonly ILogger<ArenaService> _logger;

        public ArenaService(
            DbContext dbContext,
            IRepository<ArenaPlayerEntity> arenaPlayerRepository,
            IRepository<ArenaBattleLogEntity> battleLogRepository,
            IRepository<UserEntity> userRepository,
            IPlayerAttributeService playerAttributeService,
            IRankingService rankingService,

            IMailService mailService,
            ITitleService titleService,
            ILogger<ArenaService> logger)
        {
            _dbContext = dbContext;
            _arenaPlayerRepository = arenaPlayerRepository;
            _battleLogRepository = battleLogRepository;
            _titleService = titleService;
            _userRepository = userRepository;
            _playerAttributeService = playerAttributeService;
            _rankingService = rankingService;

            _mailService = mailService;
            _logger = logger;
        }

        public async Task<ArenaMeDto?> GetMyInfoAsync(string playerId)
        {
            var arena = await GetOrCreateArenaPlayerAsync(playerId);
            if (arena == null) return null;

            var seasonConfig = await _rankingService.GetRankingConfigAsync(RANKING_ID);
            var maxDaily = 5;
            var remaining = Math.Max(0, maxDaily - arena.DailyBattlesUsed);
            var purchasable = Math.Max(0, MAX_DAILY_PURCHASES - arena.DailyBattlesPurchased);

            return new ArenaMeDto
            {
                Points = arena.Points,
                Rank = arena.Rank,
                Wins = arena.Wins,
                Losses = arena.Losses,
                WinStreak = arena.WinStreak,
                BestRank = arena.BestRank > 0 ? arena.BestRank : arena.Rank,
                DailyAttemptsRemaining = remaining,
                DailyAttemptsMax = maxDaily,
                PurchasableAttempts = purchasable,
                SeasonNumber = arena.SeasonNumber,
                SeasonEndAt = seasonConfig?.GetSeasonEndTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
            };
        }

        public async Task<List<ArenaOpponentDto>> GetOpponentsAsync(string playerId)
        {
            var arena = await _dbContext.Db.Queryable<ArenaPlayerEntity>()
                .Where(a => a.PlayerId == playerId)
                .FirstAsync();

            if (arena == null) return [];

            // 获取排名相邻的玩家
            var nearbyPlayers = await _dbContext.Db.Queryable<ArenaPlayerEntity>()
                .Where(a => a.PlayerId != playerId)
                .OrderBy(a => a.Rank)
                .Take(50)
                .ToListAsync();

            // 根据积分差异选择最接近的3-5个对手
            var opponents = nearbyPlayers
                .OrderBy(a => Math.Abs(a.Points - arena.Points))
                .Take(5)
                .ToList();

            if (opponents.Count == 0) return [];

            var playerIds = opponents.Select(o => o.PlayerId).ToList();
            var users = await _dbContext.Db.Queryable<UserEntity>()
                .Where(u => playerIds.Contains(u.GID))
                .ToListAsync();

            var userMap = users.ToDictionary(u => u.GID);

            return opponents.Select(o =>
            {
                userMap.TryGetValue(o.PlayerId, out var user);
                return new ArenaOpponentDto
                {
                    PlayerId = o.PlayerId,
                    PlayerName = user?.Name ?? "未知",
                    Level = user?.Level ?? 1,
                    Profession = user?.Profession ?? "",
                    Points = o.Points,
                    Rank = o.Rank,
                    GuildName = null,
                    PowerScore = 0
                };
            }).ToList();
        }

        public async Task<ArenaChallengeResultDto> ChallengeAsync(string attackerId, string defenderId)
        {
            if (attackerId == defenderId)
                throw new InvalidOperationException("不能挑战自己。");

            var attacker = await GetOrCreateArenaPlayerAsync(attackerId);
            var defender = await _dbContext.Db.Queryable<ArenaPlayerEntity>()
                .Where(a => a.PlayerId == defenderId)
                .FirstAsync();

            if (attacker == null || defender == null)
                throw new InvalidOperationException("玩家不在竞技场中。");

            // 检查禁赛状态
            if (attacker.BannedUntil.HasValue && attacker.BannedUntil.Value > DateTime.Now)
                throw new InvalidOperationException($"您已被禁赛至 {attacker.BannedUntil.Value:yyyy-MM-dd HH:mm}。原因: {attacker.BanReason}");

            // 检查每日次数
            if (attacker.DailyBattlesUsed >= 5)
                throw new InvalidOperationException("今日挑战次数已用完。");

            // 加载双方属性
            var attackerUser = await _userRepository.GetByIdAsync(attackerId);
            var defenderUser = await _userRepository.GetByIdAsync(defenderId);

            if (attackerUser == null || defenderUser == null)
                throw new InvalidOperationException("玩家数据不存在。");

            await _playerAttributeService.ApplyAllBonusesToEntityAsync(attackerId, attackerUser);
            await _playerAttributeService.ApplyAllBonusesToEntityAsync(defenderId, defenderUser);

            // 执行战斗
            var battleResult = BattleSystem.StartBattle(
                [attackerUser],
                [defenderUser],
                enableElementAdvantage: true);

            var isWin = battleResult.IsVictory;
            var pointsDiff = defender.Points - attacker.Points;
            int pointsChange;

            if (isWin)
            {
                pointsChange = Math.Max(10, 30 - pointsDiff / 50);
            }
            else
            {
                pointsChange = -Math.Max(5, 15 + pointsDiff / 100);
            }

            var oldRank = attacker.Rank;
            var attackerPointsAfter = Math.Max(0, attacker.Points + pointsChange);
            var defenderPointsAfter = Math.Max(0, defender.Points - pointsChange);

            // 事务内更新
            _dbContext.BeginTransaction();
            try
            {
                // 更新挑战方
                attacker.Points = attackerPointsAfter;
                attacker.DailyBattlesUsed++;
                attacker.LastBattleAt = DateTime.Now;
                attacker.UpdatedAt = DateTime.Now;

                if (isWin)
                {
                    attacker.Wins++;
                    attacker.WinStreak++;
                }
                else
                {
                    attacker.Losses++;
                    attacker.WinStreak = 0;
                }

                // 排名互换：胜利且积分超过对手时互换排名
                if (isWin && attackerPointsAfter > defender.Points && attacker.Rank > defender.Rank)
                {
                    var tempRank = attacker.Rank;
                    attacker.Rank = defender.Rank;
                    defender.Rank = tempRank;
                }

                if (attacker.BestRank == 0 || attacker.Rank < attacker.BestRank)
                {
                    attacker.BestRank = attacker.Rank;
                }

                await _arenaPlayerRepository.UpdateAsync(attacker);

                // 更新被挑战方
                defender.Points = defenderPointsAfter;
                defender.UpdatedAt = DateTime.Now;
                if (!isWin)
                {
                    defender.Wins++;
                    defender.WinStreak++;
                }
                else
                {
                    defender.Losses++;
                    defender.WinStreak = 0;
                }
                await _arenaPlayerRepository.UpdateAsync(defender);

                // 写入战斗日志
                var battleLog = new ArenaBattleLogEntity
                {
                    AttackerId = attackerId,
                    DefenderId = defenderId,
                    AttackerName = attackerUser.Name,
                    DefenderName = defenderUser.Name,
                    AttackerPointsBefore = attacker.Points - pointsChange,
                    DefenderPointsBefore = defender.Points + (isWin ? pointsChange : -pointsChange),
                    AttackerPointsAfter = attackerPointsAfter,
                    DefenderPointsAfter = defenderPointsAfter,
                    WinnerId = isWin ? attackerId : defenderId,
                    BattleLogJson = System.Text.Json.JsonSerializer.Serialize(battleResult.RoundLogs),
                    SeasonNumber = attacker.SeasonNumber,
                    CreatedAt = DateTime.Now
                };
                await _battleLogRepository.AddAsync(battleLog);

                _dbContext.CommitTransaction();
            }
            catch
            {
                _dbContext.RollbackTransaction();
                throw;
            }

            // 计算并发放奖励。
            var rewardGold = isWin ? 200 : 50;
            var rewardHonor = isWin ? 10 : 2;

            // 竞技场积分已在上方事务中完成；金币奖励单独写入时关闭通用排行榜同步，
            // 避免通过旧版 AddGoldAsync 再次刷新无关榜单。
            await _playerAttributeService.AddGoldAsync(
                attackerId,
                rewardGold,
                "竞技场挑战奖励",
                syncRankings: false);

            // 中文注释：
            // 重新读取玩家后只更新 Honor，避免把奖励前的旧 attackerUser 整体写回，
            // 覆盖刚刚提交的金币或其他并发变化。
            var latestAttackerUser = await _userRepository.GetByIdAsync(attackerId);
            if (latestAttackerUser != null)
            {
                latestAttackerUser.Honor += rewardHonor;
                await _userRepository.UpdateAsync(latestAttackerUser);
            }


            // 竞技场排名称号发放
            if (isWin && attacker.Rank <= 3)
            {
                var titleId = attacker.Rank switch
                {
                    1 => "arena_champion",
                    2 => "arena_runner_up",
                    3 => "arena_third",
                    _ => null
                };
                if (titleId != null)
                {
                    try { await _titleService.GrantTitleAsync(attackerId, titleId); }
                    catch (Exception ex) { _logger.LogError(ex, "发放竞技场称号失败。PlayerId={PlayerId}, Rank={Rank}", attackerId, attacker.Rank); }
                }
            }

            return new ArenaChallengeResultDto
            {
                IsWin = isWin,
                PointsChange = pointsChange,
                NewPoints = attackerPointsAfter,
                OldRank = oldRank,
                NewRank = attacker.Rank,
                RewardGold = rewardGold,
                RewardHonor = rewardHonor,
                BattleLogJson = System.Text.Json.JsonSerializer.Serialize(battleResult.RoundLogs),
                OpponentName = defenderUser.Name
            };
        }

        public async Task<List<ArenaBattleLogDto>> GetHistoryAsync(string playerId, int page = 1, int pageSize = 20)
        {
            var skip = (page - 1) * pageSize;

            var logs = await _dbContext.Db.Queryable<ArenaBattleLogEntity>()
                .Where(l => l.AttackerId == playerId || l.DefenderId == playerId)
                .OrderByDescending(l => l.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            return logs.Select(l =>
            {
                var isAttacker = l.AttackerId == playerId;
                return new ArenaBattleLogDto
                {
                    Id = l.Id,
                    OpponentName = isAttacker ? l.DefenderName : l.AttackerName,
                    IsWin = l.WinnerId == playerId,
                    PointsChange = isAttacker
                        ? l.AttackerPointsAfter - l.AttackerPointsBefore
                        : l.DefenderPointsAfter - l.DefenderPointsBefore,
                    BattleTime = l.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    IsAttacker = isAttacker,
                    BattleLogJson = l.BattleLogJson
                };
            }).ToList();
        }

        public async Task<int> BuyAttemptsAsync(string playerId)
        {
            var arena = await GetOrCreateArenaPlayerAsync(playerId);
            if (arena == null)
                throw new InvalidOperationException("玩家不在竞技场中。");

            if (arena.DailyBattlesPurchased >= MAX_DAILY_PURCHASES)
                throw new InvalidOperationException("今日购买次数已用完。");

            var cost = PurchaseCosts[arena.DailyBattlesPurchased];

            var user = await _userRepository.GetByIdAsync(playerId);
            if (user == null)
                throw new InvalidOperationException("玩家数据不存在。");

            if (user.SpiritStone < cost)
                throw new InvalidOperationException($"灵石不足，需要 {cost} 灵石。");

            _dbContext.BeginTransaction();
            try
            {
                user.SpiritStone -= cost;
                await _userRepository.UpdateAsync(user);

                arena.DailyBattlesPurchased++;
                arena.DailyBattlesUsed = Math.Max(0, arena.DailyBattlesUsed - 1);
                arena.UpdatedAt = DateTime.Now;
                await _arenaPlayerRepository.UpdateAsync(arena);

                _dbContext.CommitTransaction();
            }
            catch
            {
                _dbContext.RollbackTransaction();
                throw;
            }

            return cost;
        }

        private async Task<ArenaPlayerEntity?> GetOrCreateArenaPlayerAsync(string playerId)
        {
            var existing = await _dbContext.Db.Queryable<ArenaPlayerEntity>()
                .Where(a => a.PlayerId == playerId)
                .FirstAsync();

            if (existing != null) return existing;

            // 自动注册
            var user = await _userRepository.GetByIdAsync(playerId);
            if (user == null) return null;

            var maxRank = await _dbContext.Db.Queryable<ArenaPlayerEntity>()
                .MaxAsync(a => a.Rank);

            var entity = new ArenaPlayerEntity
            {
                PlayerId = playerId,
                Points = INITIAL_POINTS,
                Rank = maxRank + 1,
                SeasonNumber = 1,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _arenaPlayerRepository.AddAsync(entity);


            return entity;
        }

        public async Task<ArenaSeasonDto> GetSeasonInfoAsync(string playerId)
        {
            var config = await _rankingService.GetRankingConfigAsync(RANKING_ID);

            var arena = await _dbContext.Db.Queryable<ArenaPlayerEntity>()
                .Where(a => a.PlayerId == playerId)
                .FirstAsync();

            var rewards = await _dbContext.Db.Queryable<RankingRewardEntity>()
                .Where(r => r.RankingId == RANKING_ID)
                .OrderBy(r => r.MinRank)
                .ToListAsync();

            var endDate = config?.GetSeasonEndTime() ?? DateTime.MaxValue;
            var remaining = config?.GetSeasonDaysRemaining() ?? 0;

            return new ArenaSeasonDto
            {
                SeasonNumber = config?.CurrentSeason ?? 1,
                StartAt = config?.SeasonStartTime.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                EndAt = endDate != DateTime.MaxValue ? endDate.ToString("yyyy-MM-ddTHH:mm:ssZ") : null,
                TimeRemaining = remaining > 0 ? $"{remaining}天" : "赛季已结束",
                MyRank = arena?.Rank ?? 0,
                MyPoints = arena?.Points ?? 0,
                Rewards = rewards.Select(r => new ArenaSeasonRewardDto
                {
                    MinRank = r.MinRank,
                    MaxRank = r.MaxRank,
                    RewardTitle = r.RewardTitle,
                    Gold = r.Gold,
                    SpiritStone = r.SpiritStone,
                    Title = r.Title
                }).ToList()
            };
        }
    }
}
