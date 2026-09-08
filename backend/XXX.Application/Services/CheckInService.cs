using Microsoft.Extensions.Logging;
using System.Text.Json;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Balance;
using XXX.Entity;
using XXX.Infrastructure.Data;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    /// <summary>
    /// 中文注释：
    /// 数据库版签到服务。
    /// 这一层直接操作签到状态表和签到记录表，保证签到行为是“真实写库”的，
    /// 页面刷新、服务重启之后，月历勾选和领奖结果都不会丢失。
    /// </summary>
    public class CheckInService : ICheckInService
    {
        private readonly DbContext _dbContext;
        private readonly IRepository<CheckInStateEntity> _checkInStateRepository;
        private readonly IRepository<CheckInRecordEntity> _checkInRecordRepository;
        private readonly IPlayerRewardService _playerRewardService;
        private readonly IPlayerAttributeService _playerAttributeService;
        private readonly ILogger<CheckInService> _logger;

        /// <summary>
        /// 初始化签到服务。
        /// </summary>
        public CheckInService(
            DbContext dbContext,
            IRepository<CheckInStateEntity> checkInStateRepository,
            IRepository<CheckInRecordEntity> checkInRecordRepository,
            IPlayerRewardService playerRewardService,
            IPlayerAttributeService playerAttributeService,
            ILogger<CheckInService> logger)
        {
            _dbContext = dbContext;
            _checkInStateRepository = checkInStateRepository;
            _checkInRecordRepository = checkInRecordRepository;
            _playerRewardService = playerRewardService;
            _playerAttributeService = playerAttributeService;
            _logger = logger;
        }

        /// <summary>
        /// 获取指定月份的签到状态。
        /// </summary>
        public async Task<CheckInStatusDto> GetStatusAsync(string playerId, int? year = null, int? month = null)
        {
            var targetYear = year.GetValueOrDefault(DateTime.Now.Year);
            var targetMonth = month.GetValueOrDefault(DateTime.Now.Month);
            NormalizeYearMonth(ref targetYear, ref targetMonth);

            var state = await GetOrCreateStateAsync(playerId);
            var monthStart = new DateTime(targetYear, targetMonth, 1);
            var monthEnd = monthStart.AddMonths(1);
            var rewardConfigs = await LoadRewardConfigsAsync();

            var records = await _dbContext.Db.Queryable<CheckInRecordEntity>()
                .Where(record => record.PlayerId == playerId)
                .Where(record => record.CheckInDate >= monthStart && record.CheckInDate < monthEnd)
                .OrderBy(record => record.CheckInDate)
                .ToListAsync();

            var today = DateTime.Now.Date;
            var hasCheckedToday = state.LastCheckInDate?.Date == today;
            var previewDay = hasCheckedToday ? state.ContinuousDays : CalculateNextContinuousDay(state, today);

            _logger.LogInformation(
                "Loaded check-in status for player {PlayerId} at {Year}-{Month}, total days {TotalDays}, continuous days {ContinuousDays}",
                playerId,
                targetYear,
                targetMonth,
                state.TotalDays,
                state.ContinuousDays);

            return new CheckInStatusDto
            {
                Year = targetYear,
                Month = targetMonth,
                ContinuousDays = state.ContinuousDays,
                TotalDays = state.TotalDays,
                HasCheckedToday = hasCheckedToday,
                NextMilestoneDay = GetNextMilestoneDay(state.ContinuousDays, rewardConfigs),
                DaysToNextMilestone = GetDaysToNextMilestone(state.ContinuousDays, rewardConfigs),
                LastCheckInDate = state.LastCheckInDate,
                CheckedDates = records.Select(record => record.CheckInDate.ToString("yyyy-MM-dd")).ToList(),
                TodayRewards = BuildPreviewRewards(BuildRewardDefinitions(previewDay, rewardConfigs)),
                Milestones = BuildMilestoneDtos(state.ContinuousDays, rewardConfigs),
                HintMessage = hasCheckedToday
                    ? "今日奖励已经领取，明天再来继续签到。"
                    : $"本次签到将按连续第 {previewDay} 天结算奖励。"
            };
        }

        /// <summary>
        /// 领取今日签到奖励。
        /// </summary>
        public async Task<ClaimCheckInResultDto> ClaimAsync(string playerId)
        {
            var today = DateTime.Now.Date;
            var rewardConfigs = await LoadRewardConfigsAsync();
            var nextContinuousDay = 0;
            List<RewardGrantItemDto> rewardDefinitions = [];
            List<RewardItemDto> grantedRewards;

            try
            {
                _dbContext.BeginTransaction();

                var state = await GetOrCreateStateAsync(playerId);
                if (state.LastCheckInDate?.Date == today)
                {
                    _logger.LogWarning("Player {PlayerId} attempted duplicate daily check-in on {Date}", playerId, today);
                    throw new InvalidOperationException("今日已经签到，请勿重复领取。");
                }

                nextContinuousDay = CalculateNextContinuousDay(state, today);
                rewardDefinitions = BuildRewardDefinitions(nextContinuousDay, rewardConfigs);

                grantedRewards = await _playerRewardService.GrantRewardsAsync(
                    playerId,
                    rewardDefinitions,
                    $"签到第 {nextContinuousDay} 天");

                state.ContinuousDays = nextContinuousDay;
                state.TotalDays += 1;
                state.LastCheckInDate = today;
                state.LastUpdateTime = DateTime.Now;

                _logger.LogInformation("Check-in transaction updating state. PlayerId={PlayerId}, ContinuousDay={ContinuousDay}", playerId, nextContinuousDay);
                await _checkInStateRepository.UpdateAsync(state);

                _logger.LogInformation("Check-in transaction inserting record. PlayerId={PlayerId}, Date={Date}", playerId, today);
                await _checkInRecordRepository.AddAsync(new CheckInRecordEntity
                {
                    PlayerId = playerId,
                    CheckInDate = today,
                    RewardSnapshotJson = JsonSerializer.Serialize(grantedRewards),
                    CreateTime = DateTime.Now
                });

                _logger.LogInformation("Check-in transaction committing. PlayerId={PlayerId}", playerId);
                _dbContext.CommitTransaction();
            }
            catch (Exception ex)
            {
                _dbContext.RollbackTransaction();
                _logger.LogWarning(ex, "签到失败。PlayerId={PlayerId}", playerId);
                throw;
            }

            // 中文注释：
            // 签到奖励固定包含经验，必须在事务提交后再做完整重算，
            // 否则 SQLite 开发库会因为事务中的跨连接写入出现锁等待。
            await _playerAttributeService.RecalculatePlayerAttributesAsync(playerId, syncLevelDrivenProgress: true);
            _logger.LogInformation(
                "Player {PlayerId} claimed daily check-in successfully, continuous day {ContinuousDay}, reward count {RewardCount}",
                playerId,
                nextContinuousDay,
                grantedRewards.Count);

            return new ClaimCheckInResultDto
            {
                Message = $"签到成功，连续签到 {nextContinuousDay} 天。",
                Rewards = grantedRewards,
                Status = await GetStatusAsync(playerId)
            };
        }

        /// <summary>
        /// 中文注释：
        /// 连续签到规则故意保持简单直观：
        /// - 昨天签了，今天继续就是 +1
        /// - 断签后重新从 1 开始
        /// 这样对真实玩家最容易理解，也能避免前端展示和后端结算出现偏差。
        /// </summary>
        private static int CalculateNextContinuousDay(CheckInStateEntity state, DateTime today)
        {
            if (state.LastCheckInDate?.Date == today.AddDays(-1))
            {
                return state.ContinuousDays + 1;
            }

            return 1;
        }

        private async Task<CheckInStateEntity> GetOrCreateStateAsync(string playerId)
        {
            var state = await _checkInStateRepository.GetByIdAsync(playerId);
            if (state != null)
            {
                return state;
            }

            state = new CheckInStateEntity
            {
                PlayerId = playerId,
                ContinuousDays = 0,
                TotalDays = 0,
                LastUpdateTime = DateTime.Now
            };

            await _checkInStateRepository.AddAsync(state);
            return state;
        }

        private async Task<List<CheckInRewardConfigEntity>> LoadRewardConfigsAsync()
        {
            await EnsureRewardConfigsSeededAsync();
            return await _dbContext.Db.Queryable<CheckInRewardConfigEntity>()
                .OrderBy(config => config.ContinuousDay)
                .ToListAsync();
        }

        private async Task EnsureRewardConfigsSeededAsync()
        {
            var requiredDays = ActivityRewardCatalog.BuildCheckInRewards()
                .Select(seed => seed.ContinuousDay)
                .Distinct()
                .OrderBy(day => day)
                .ToList();
            var existingDays = await _dbContext.Db.Queryable<CheckInRewardConfigEntity>()
                .Select(config => config.ContinuousDay)
                .ToListAsync();
            var existingSet = existingDays.ToHashSet();
            var missingDays = requiredDays
                .Where(day => !existingSet.Contains(day))
                .ToList();

            if (missingDays.Count == 0)
            {
                return;
            }

            _logger.LogError(
                "Check-in reward configs are incomplete. Missing days: {MissingDays}. Runtime check-in seeding has been disabled; database configs are required.",
                string.Join(", ", missingDays));
            throw new InvalidOperationException($"Check-in reward configs are incomplete. Missing days: {string.Join(", ", missingDays)}. Please run startup seed sync before using check-in.");
        }

        private static List<RewardGrantItemDto> BuildRewardDefinitions(
            int continuousDay,
            IReadOnlyList<CheckInRewardConfigEntity> rewardConfigs)
        {
            var selectedConfig = rewardConfigs
                .FirstOrDefault(config => config.ContinuousDay == Math.Max(1, continuousDay))
                ?? rewardConfigs.LastOrDefault();
            if (selectedConfig == null || string.IsNullOrWhiteSpace(selectedConfig.RewardJson))
            {
                return [];
            }

            try
            {
                return JsonSerializer.Deserialize<List<RewardGrantItemDto>>(selectedConfig.RewardJson) ?? [];
            }
            catch
            {
                return [];
            }
        }

        private static List<RewardItemDto> BuildPreviewRewards(IReadOnlyList<RewardGrantItemDto> rewards)
        {
            return rewards
                .Select(MapPreviewReward)
                .ToList();
        }

        private static RewardItemDto MapPreviewReward(RewardGrantItemDto reward)
        {
            return reward.Type switch
            {
                RewardTypes.Gold => new RewardItemDto
                {
                    Type = RewardTypes.Gold,
                    Name = "金币",
                    Count = reward.Count,
                    Description = reward.Description
                },
                RewardTypes.Exp => new RewardItemDto
                {
                    Type = RewardTypes.Exp,
                    Name = "修为",
                    Count = reward.Count,
                    Description = reward.Description
                },
                RewardTypes.SpiritStone => new RewardItemDto
                {
                    Type = RewardTypes.SpiritStone,
                    Name = "灵石",
                    Count = reward.Count,
                    Description = reward.Description
                },
                RewardTypes.Item when !string.IsNullOrWhiteSpace(reward.ItemId)
                    && XXX.GameData.Items.TryGetValue(reward.ItemId, out var itemTemplate) => new RewardItemDto
                    {
                        Type = RewardTypes.Item,
                        Name = itemTemplate.Name,
                        Count = reward.Count,
                        ItemId = reward.ItemId,
                        Description = reward.Description ?? itemTemplate.Description
                    },
                _ => new RewardItemDto
                {
                    Type = reward.Type,
                    Name = reward.ItemId ?? reward.Type,
                    Count = reward.Count,
                    ItemId = reward.ItemId,
                    Description = reward.Description
                }
            };
        }

        private static List<CheckInMilestoneDto> BuildMilestoneDtos(
            int continuousDays,
            IReadOnlyList<CheckInRewardConfigEntity> rewardConfigs)
        {
            return rewardConfigs
                .Where(config => config.IsMilestone)
                .OrderBy(config => config.ContinuousDay)
                .Select(config =>
                {
                    var rewards = BuildPreviewRewards(BuildRewardDefinitions(config.ContinuousDay, rewardConfigs));
                    var summary = rewards.Count == 0
                        ? "无奖励"
                        : string.Join("、", rewards.Select(reward => $"{reward.Name} x{reward.Count}"));

                    return new CheckInMilestoneDto
                    {
                        Days = config.ContinuousDay,
                        IsReached = continuousDays >= config.ContinuousDay,
                        RewardSummary = summary
                    };
                })
                .ToList();
        }

        private static int GetNextMilestoneDay(
            int continuousDays,
            IReadOnlyList<CheckInRewardConfigEntity> rewardConfigs)
        {
            var milestoneDays = rewardConfigs
                .Where(config => config.IsMilestone)
                .Select(config => config.ContinuousDay)
                .OrderBy(day => day)
                .ToList();
            if (milestoneDays.Count == 0)
            {
                return 0;
            }

            foreach (var milestoneDay in milestoneDays)
            {
                if (continuousDays < milestoneDay)
                {
                    return milestoneDay;
                }
            }

            return milestoneDays[^1];
        }

        private static int GetDaysToNextMilestone(
            int continuousDays,
            IReadOnlyList<CheckInRewardConfigEntity> rewardConfigs)
        {
            var nextMilestoneDay = GetNextMilestoneDay(continuousDays, rewardConfigs);
            if (nextMilestoneDay <= 0)
            {
                return 0;
            }

            return Math.Max(nextMilestoneDay - continuousDays, 0);
        }

        private static void NormalizeYearMonth(ref int year, ref int month)
        {
            if (year < 2000 || year > 2100)
            {
                year = DateTime.Now.Year;
            }

            if (month < 1 || month > 12)
            {
                month = DateTime.Now.Month;
            }
        }
    }
}
