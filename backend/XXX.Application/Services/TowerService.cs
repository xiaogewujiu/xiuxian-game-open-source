using Microsoft.Extensions.Logging;
using System.Text.Json;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Battle;
using XXX.Entity;
using XXX.Infrastructure.Data;
using XXX.Infrastructure.Repositories;
using XXX.Ranking;

namespace XXX.Application.Services
{
    public class TowerService : ITowerService
    {
        private const string RANKING_ID = "ranking_tower";
        private const int MAX_DAILY_PURCHASES = 3;
        private const int MAX_FLOOR = 100;
        private static readonly int[] PurchaseCosts = [50, 100, 200];

        private readonly DbContext _dbContext;
        private readonly IRepository<TowerProgressEntity> _progressRepository;
        private readonly IRepository<TowerFloorConfigEntity> _floorConfigRepository;
        private readonly IRepository<TowerBattleLogEntity> _battleLogRepository;
        private readonly IRepository<UserEntity> _userRepository;
        private readonly IPlayerAttributeService _playerAttributeService;
        private readonly IRankingService _rankingService;

        private readonly ITitleService _titleService;
        private readonly ILogger<TowerService> _logger;

        public TowerService(
            DbContext dbContext,
            IRepository<TowerProgressEntity> progressRepository,
            IRepository<TowerFloorConfigEntity> floorConfigRepository,
            IRepository<TowerBattleLogEntity> battleLogRepository,
            IRepository<UserEntity> userRepository,
            IPlayerAttributeService playerAttributeService,
            IRankingService rankingService,

            ITitleService titleService,
            ILogger<TowerService> logger)
        {
            _dbContext = dbContext;
            _progressRepository = progressRepository;
            _floorConfigRepository = floorConfigRepository;
            _battleLogRepository = battleLogRepository;
            _userRepository = userRepository;
            _titleService = titleService;
            _playerAttributeService = playerAttributeService;
            _rankingService = rankingService;

            _logger = logger;
        }

        public async Task<TowerMeDto?> GetMyInfoAsync(string playerId)
        {
            var progress = await GetOrCreateProgressAsync(playerId);
            if (progress == null) return null;

            var maxDaily = 5;
            var remaining = Math.Max(0, maxDaily - progress.DailyAttemptsUsed);
            var purchasable = Math.Max(0, MAX_DAILY_PURCHASES - progress.DailyAttemptsPurchased);

            return new TowerMeDto
            {
                HighestFloor = progress.HighestFloor,
                CurrentFloor = progress.CurrentFloor,
                DailyAttemptsRemaining = remaining,
                DailyAttemptsMax = maxDaily,
                PurchasableAttempts = purchasable,
                BestClearTimeMs = progress.BestClearTimeMs,
                LastAttemptAt = progress.LastAttemptAt?.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };
        }

        public async Task<List<TowerFloorSummaryDto>> GetFloorSummariesAsync(string playerId)
        {
            var progress = await GetOrCreateProgressAsync(playerId);
            var currentFloor = progress?.CurrentFloor ?? 1;
            var maxShow = Math.Min(currentFloor + 5, MAX_FLOOR);

            var configs = await _dbContext.Db.Queryable<TowerFloorConfigEntity>()
                .Where(f => f.Floor <= maxShow)
                .OrderBy(f => f.Floor)
                .ToListAsync();

            return configs.Select(c =>
            {
                var isMilestone = c.Floor % 10 == 0;
                var monsterIds = JsonSerializer.Deserialize<List<string>>(c.MonsterTemplateIdsJson) ?? [];
                var monsterName = "未知怪物";
                var monsterLevel = 1;

                if (monsterIds.Count > 0 && GameData.MonsterTemplates.TryGetValue(monsterIds[0], out var monster))
                {
                    monsterName = monster.Name;
                    monsterLevel = int.TryParse(monster.Level, out var lv) ? lv : 1;
                }

                return new TowerFloorSummaryDto
                {
                    Floor = c.Floor,
                    MonsterName = monsterName,
                    MonsterLevel = monsterLevel,
                    IsMilestone = isMilestone,
                    IsCleared = progress != null && c.Floor <= progress.HighestFloor,
                    RewardPreview = $"金币x{c.RewardGold} 经验x{c.RewardExp}"
                };
            }).ToList();
        }

        public async Task<TowerChallengeResultDto> ChallengeAsync(string playerId)
        {
            var progress = await GetOrCreateProgressAsync(playerId);
            if (progress == null)
                throw new InvalidOperationException("玩家数据不存在。");

            if (progress.DailyAttemptsUsed >= 5)
                throw new InvalidOperationException("今日挑战次数已用完。");

            if (progress.CurrentFloor > MAX_FLOOR)
                throw new InvalidOperationException("已通关全部楼层。");

            var floorConfig = await _dbContext.Db.Queryable<TowerFloorConfigEntity>()
                .Where(f => f.Floor == progress.CurrentFloor)
                .FirstAsync();

            if (floorConfig == null)
                throw new InvalidOperationException($"第{progress.CurrentFloor}层配置不存在。");

            var user = await _userRepository.GetByIdAsync(playerId);
            if (user == null)
                throw new InvalidOperationException("玩家数据不存在。");

            await _playerAttributeService.ApplyAllBonusesToEntityAsync(playerId, user);

            // 生成怪物
            var monsterIds = JsonSerializer.Deserialize<List<string>>(floorConfig.MonsterTemplateIdsJson) ?? [];
            var monsters = new List<MonsterEntity>();
            var multiplier = floorConfig.StatMultiplier;
            foreach (var mId in monsterIds.Take(floorConfig.MonsterCount))
            {
                if (GameData.MonsterTemplates.TryGetValue(mId, out var template))
                {
                    var lo1 = template.MinType1 ?? 0; var hi1 = template.MaxType1 ?? lo1;
                    var lo2 = template.MinType2 ?? 0; var hi2 = template.MaxType2 ?? lo2;
                    var lo3 = template.MinType3 ?? 0; var hi3 = template.MaxType3 ?? lo3;
                    var lo4 = template.MinType4 ?? 0; var hi4 = template.MaxType4 ?? lo4;
                    var lo5 = template.MinType5 ?? 0; var hi5 = template.MaxType5 ?? lo5;
                    var lo6 = template.MinType6 ?? 0; var hi6 = template.MaxType6 ?? lo6;
                    var lo7 = template.MinType7 ?? 0; var hi7 = template.MaxType7 ?? lo7;

                    var monster = new MonsterEntity
                    {
                        MonsterTempID = template.GID,
                        GID = Guid.NewGuid().ToString("N"),
                        Name = template.Name,
                        Level = template.Level,
                        SkillIds = template.SkillIds.ToList(),
                        PassiveIds = template.PassiveIds.ToList(),
                        ItemDrops = template.ItemDrops.ToList(),
                        EquipmentDrops = template.EquipmentDrops.ToList(),
                        ExpReward = template.ExpReward,
                        GoldReward = template.GoldReward,
                        Type1 = (int)(RandomRange(lo1, hi1) * multiplier),
                        Type2 = (int)(RandomRange(lo2, hi2) * multiplier),
                        Type3 = (int)(RandomRange(lo3, hi3) * multiplier),
                        Type4 = (int)(RandomRange(lo4, hi4) * multiplier),
                        Type5 = (int)(RandomRange(lo5, hi5) * multiplier),
                        Type6 = (int)(RandomRange(lo6, hi6) * multiplier),
                        Type7 = (int)(RandomRange(lo7, hi7) * multiplier),
                        Type8 = RandomRange(template.MinType8, template.MaxType8) / 100f,
                        Type9 = RandomRange(template.MinType9, template.MaxType9) / 100f,
                        Type10 = RandomRange(template.MinType10, template.MaxType10) / 100f,
                        Type11 = RandomRange(template.MinType11, template.MaxType11) / 100f,
                        Type12 = RandomRange(template.MinType12, template.MaxType12) / 100f,
                        Type13 = RandomRange(template.MinType13, template.MaxType13) / 100f,
                        Type14 = RandomRange(template.MinType14, template.MaxType14) / 100f,
                        Type15 = RandomRange(template.MinType15, template.MaxType15) / 100f,
                    };
                    monsters.Add(monster);
                }
            }

            var startTime = DateTime.Now;
            var battleResult = BattleSystem.StartBattle([user], monsters, enableElementAdvantage: true);
            var clearTimeMs = (long)(DateTime.Now - startTime).TotalMilliseconds;

            var isWin = battleResult.IsVictory;
            var battleLogJson = JsonSerializer.Serialize(battleResult.RoundLogs);

            // 写入战斗日志
            var log = new TowerBattleLogEntity
            {
                PlayerId = playerId,
                Floor = progress.CurrentFloor,
                IsWin = isWin,
                BattleLogJson = battleLogJson,
                CreatedAt = DateTime.Now
            };
            await _battleLogRepository.AddAsync(log);

            // 清理旧日志（每层保留3条）
            var oldLogs = await _dbContext.Db.Queryable<TowerBattleLogEntity>()
                .Where(l => l.PlayerId == playerId && l.Floor == progress.CurrentFloor)
                .OrderByDescending(l => l.CreatedAt)
                .Skip(3)
                .ToListAsync();

            if (oldLogs.Count > 0)
            {
                foreach (var old in oldLogs)
                {
                    await _battleLogRepository.DeleteAsync(old.Id);
                }
            }

            int rewardGold = 0, rewardExp = 0;
            string? milestoneReward = null;
            var towerRankingChanged = false;

            if (isWin)
            {
                rewardGold = floorConfig.RewardGold;
                rewardExp = floorConfig.RewardExp;

                // 发放战斗奖励
                if (rewardGold > 0)
                    await _playerAttributeService.AddGoldAsync(playerId, rewardGold, "通天塔通关奖励");
                if (rewardExp > 0)
                    await _playerAttributeService.AddExpAsync(playerId, rewardExp);

                // 里程碑楼层奖励
                if (progress.CurrentFloor % 10 == 0 && !string.IsNullOrEmpty(floorConfig.MilestoneRewardJson))
                {
                    milestoneReward = floorConfig.MilestoneRewardJson;
                }

                // 第100层终极奖励
                if (progress.CurrentFloor >= MAX_FLOOR)
                {
                    try
                    {
                        await _dbContext.Db.Updateable<UserEntity>()
                            .SetColumns(u => u.SpiritStone == u.SpiritStone + 5000)
                            .Where(u => u.GID == playerId)
                            .ExecuteCommandAsync();
                        await _titleService.GrantTitleAsync(playerId, "tower_master");
                        milestoneReward = string.IsNullOrEmpty(milestoneReward)
                            ? "{\"spiritStone\":5000,\"title\":\"tower_master\"}"
                            : milestoneReward;
                        _logger.LogInformation("玩家通关通天塔第100层。PlayerId={PlayerId}", playerId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "通天塔第100层终极奖励发放失败。PlayerId={PlayerId}", playerId);
                    }
                }

                var previousHighestFloor = progress.HighestFloor;
                var previousBestClearTimeMs = progress.BestClearTimeMs;
                progress.HighestFloor = Math.Max(progress.HighestFloor, progress.CurrentFloor);
                progress.CurrentFloor++;
                progress.BestClearTimeMs = progress.BestClearTimeMs == null
                    ? clearTimeMs
                    : Math.Min(progress.BestClearTimeMs.Value, clearTimeMs);

                // 中文注释：
                // 通天塔榜单必须使用玩家保存后的历史最佳通关时间。
                // 同层较慢成绩只能保留为战斗记录，不能覆盖榜单中已有的更快成绩。
                towerRankingChanged = progress.HighestFloor > previousHighestFloor ||
                    progress.BestClearTimeMs != previousBestClearTimeMs;


            }
            else
            {
                progress.DailyAttemptsUsed++;
            }

            progress.LastAttemptAt = DateTime.Now;
            progress.UpdatedAt = DateTime.Now;
            await _progressRepository.UpdateAsync(progress);

            if (isWin)
            {
                // 中文注释：通天塔进度保存成功后，再按实际发生的源数据变化投递 Outbox。
                var rankingIds = new List<string>();
                if (towerRankingChanged)
                {
                    rankingIds.Add("ranking_tower");
                }

                if (rewardExp > 0)
                {
                    rankingIds.Add("ranking_level");
                }

                if (rewardGold > 0 || progress.CurrentFloor > 1)
                {
                    rankingIds.Add("ranking_wealth");
                }


            }

            var maxDaily = 5;
            return new TowerChallengeResultDto
            {
                IsWin = isWin,
                Floor = progress.CurrentFloor - (isWin ? 1 : 0),
                BattleLogJson = battleLogJson,
                RewardGold = rewardGold,
                RewardExp = rewardExp,
                MilestoneReward = milestoneReward,
                NewHighestFloor = progress.HighestFloor,
                DailyAttemptsRemaining = Math.Max(0, maxDaily - progress.DailyAttemptsUsed)
            };
        }

        public async Task<List<TowerBattleLogDto>> GetHistoryAsync(string playerId, int take = 20)
        {
            var logs = await _dbContext.Db.Queryable<TowerBattleLogEntity>()
                .Where(l => l.PlayerId == playerId)
                .OrderByDescending(l => l.CreatedAt)
                .Take(take)
                .ToListAsync();

            return logs.Select(l => new TowerBattleLogDto
            {
                Id = l.Id,
                Floor = l.Floor,
                IsWin = l.IsWin,
                BattleLogJson = l.BattleLogJson,
                CreatedAt = l.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
            }).ToList();
        }

        public async Task<int> BuyAttemptsAsync(string playerId)
        {
            var progress = await GetOrCreateProgressAsync(playerId);
            if (progress == null)
                throw new InvalidOperationException("玩家数据不存在。");

            if (progress.DailyAttemptsPurchased >= MAX_DAILY_PURCHASES)
                throw new InvalidOperationException("今日购买次数已用完。");

            var cost = PurchaseCosts[progress.DailyAttemptsPurchased];

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

                progress.DailyAttemptsPurchased++;
                progress.DailyAttemptsUsed = Math.Max(0, progress.DailyAttemptsUsed - 1);
                progress.UpdatedAt = DateTime.Now;
                await _progressRepository.UpdateAsync(progress);

                _dbContext.CommitTransaction();
            }
            catch
            {
                _dbContext.RollbackTransaction();
                throw;
            }

            return cost;
        }

        private static int RandomRange(int min, int max)
        {
            return min >= max ? min : Random.Shared.Next(min, max + 1);
        }

        private static int RandomRange(int? min, int? max)
        {
            var minValue = min ?? 0;
            var maxValue = max ?? minValue;
            if (maxValue < minValue) maxValue = minValue;
            return Random.Shared.Next(minValue, maxValue + 1);
        }

        private async Task<TowerProgressEntity?> GetOrCreateProgressAsync(string playerId)
        {
            var existing = await _dbContext.Db.Queryable<TowerProgressEntity>()
                .Where(t => t.PlayerId == playerId)
                .FirstAsync();

            if (existing != null) return existing;

            var user = await _userRepository.GetByIdAsync(playerId);
            if (user == null) return null;

            var entity = new TowerProgressEntity
            {
                PlayerId = playerId,
                HighestFloor = 0,
                CurrentFloor = 1,
                SeasonNumber = 1,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _progressRepository.AddAsync(entity);
            return entity;
        }

        public async Task<List<RankingEntry>> GetLeaderboardAsync(int count = 50)
        {
            return await _rankingService.GetTopNAsync(RANKING_ID, Math.Max(1, count));
        }
    }
}
