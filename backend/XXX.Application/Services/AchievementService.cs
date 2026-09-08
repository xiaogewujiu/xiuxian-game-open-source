using Microsoft.Extensions.Logging;
using SqlSugar;
using System.Text.Json;
using XXX.Achievement;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Data;
using XXX.Inventory;

namespace XXX.Application.Services
{
    /// <summary>
    /// 成就服务实现类
    /// 提供成就配置管理、进度追踪、奖励领取等成就系统功能的实现
    /// </summary>
    /// <remarks>
    /// 主要职责：
    /// - 加载和管理成就配置
    /// - 追踪玩家成就进度
    /// - 处理成就完成和奖励发放
    /// - 统计成就点数
    /// </remarks>
    public class AchievementService : IAchievementService
    {
        private readonly DbContext _dbContext;

        /// <summary>
        /// 数据库客户端，用于数据持久化操作
        /// </summary>
        private readonly ISqlSugarClient _db;

        private readonly IPlayerRewardService _playerRewardService;
        private readonly IPlayerAttributeService _playerAttributeService;
        private readonly IGameSyncService _gameSyncService;

        private readonly ITitleService _titleService;

        /// <summary>
        /// 日志记录器
        /// </summary>
        private readonly ILogger<AchievementService> _logger;

        /// <summary>
        /// 成就配置缓存，键为成就ID
        /// </summary>
        private static readonly Dictionary<string, AchievementConfig> SharedAchievementConfigs = [];
        private static readonly SemaphoreSlim InitializationLock = new(1, 1);
        private readonly Dictionary<string, AchievementConfig> _achievementConfigs = SharedAchievementConfigs;
        private static bool _isInitialized;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dbContext">数据库上下文</param>
        /// <param name="playerRewardService">奖励发放服务</param>
        /// <param name="playerAttributeService">玩家属性服务</param>
        /// <param name="gameSyncService">玩家状态同步服务</param>
        /// <param name="rankingService">排行榜服务</param>
        /// <param name="logger">日志记录器</param>
        public AchievementService(
            DbContext dbContext,
            IPlayerRewardService playerRewardService,
            IPlayerAttributeService playerAttributeService,
            IGameSyncService gameSyncService,

            ITitleService titleService,
            ILogger<AchievementService> logger)
        {
            _dbContext = dbContext;
            _db = dbContext.Db;
            _playerRewardService = playerRewardService;
            _playerAttributeService = playerAttributeService;
            _gameSyncService = gameSyncService;

            _titleService = titleService;
            _logger = logger;
        }

        private async Task EnsureInitializedAsync()
        {
            if (_isInitialized && _achievementConfigs.Count > 0)
            {
                return;
            }

            await InitializeAsync();
        }

        private async Task SyncSnapshotRequirementsAsync(string playerId)
        {
            await SyncRequirementStateAsync(playerId, AchievementRequirementType.ReachLevel);
            await SyncRequirementStateAsync(playerId, AchievementRequirementType.MaxEnhanceLevel);
            await SyncRequirementStateAsync(playerId, AchievementRequirementType.TotalGoldEarned);
            await SyncRequirementStateAsync(playerId, AchievementRequirementType.TotalWins);
            await SyncRequirementStateAsync(playerId, AchievementRequirementType.TotalKills);
        }

        /// <summary>
        /// 初始化成就配置缓存。
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_isInitialized && _achievementConfigs.Count > 0)
            {
                return;
            }

            await ReloadCacheAsync();
        }

        /// <summary>
        /// 重新从数据库装载成就配置缓存。
        /// </summary>
        public async Task ReloadCacheAsync()
        {
            await InitializationLock.WaitAsync();
            try
            {
                var totalConfigCount = await _db.Queryable<AchievementConfigEntity>().CountAsync();
                if (totalConfigCount == 0)
                {
                    _logger.LogError(
                        "Achievement configs are missing. Runtime achievement seeding has been disabled; database configs are required.");
                    throw new InvalidOperationException(
                        "Achievement configs are missing. Please run startup seed sync before using achievements.");
                }

                var configEntities = await _db.Queryable<AchievementConfigEntity>()
                    .Where(c => c.IsEnabled)
                    .ToListAsync();

                _achievementConfigs.Clear();
                foreach (var entity in configEntities)
                {
                    var config = MapToConfig(entity);
                    _achievementConfigs[config.AchievementId] = config;
                }

                _isInitialized = true;
                _logger.LogInformation("Achievement system cache reloaded with {Count} achievements", _achievementConfigs.Count);
            }
            finally
            {
                InitializationLock.Release();
            }
        }

        /// <summary>
        /// 获取全部成就配置。
        /// </summary>
        /// <returns>按排序字段升序返回的成就列表。</returns>
        public async Task<List<AchievementConfig>> GetAllAchievementsAsync()
        {
            await EnsureInitializedAsync();
            return _achievementConfigs.Values.OrderBy(a => a.SortOrder).ToList();
        }

        /// <summary>
        /// 获取单个成就配置。
        /// </summary>
        /// <param name="achievementId">成就编号。</param>
        /// <returns>命中的成就配置；不存在时返回空。</returns>
        public async Task<AchievementConfig?> GetAchievementConfigAsync(string achievementId)
        {
            await EnsureInitializedAsync();
            return _achievementConfigs.TryGetValue(achievementId, out var config) ? config : null;
        }

        /// <summary>
        /// 获取玩家全部成就进度。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <returns>该玩家已有的成就进度集合。</returns>
        public async Task<List<AchievementProgress>> GetPlayerAchievementsAsync(string playerId)
        {
            await EnsureInitializedAsync();
            await SyncSnapshotRequirementsAsync(playerId);
            var entities = await _db.Queryable<AchievementProgressEntity>()
                .Where(p => p.PlayerId == playerId)
                .ToListAsync();

            return entities.Select(MapToProgress).ToList();
        }

        /// <summary>
        /// 获取玩家已完成的成就进度。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <returns>状态为已完成的成就记录。</returns>
        public async Task<List<AchievementProgress>> GetPlayerCompletedAchievementsAsync(string playerId)
        {
            await EnsureInitializedAsync();
            await SyncSnapshotRequirementsAsync(playerId);
            var entities = await _db.Queryable<AchievementProgressEntity>()
                .Where(p => p.PlayerId == playerId && p.Status == (int)AchievementStatus.Completed)
                .ToListAsync();

            return entities.Select(MapToProgress).ToList();
        }

        /// <summary>
        /// 获取玩家某一条成就的进度。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="achievementId">成就编号。</param>
        /// <returns>命中的成就进度；不存在时返回空。</returns>
        public async Task<AchievementProgress?> GetAchievementProgressAsync(string playerId, string achievementId)
        {
            await EnsureInitializedAsync();
            await SyncSnapshotRequirementsAsync(playerId);
            var entity = await _db.Queryable<AchievementProgressEntity>()
                .Where(p => p.PlayerId == playerId && p.AchievementId == achievementId)
                .FirstAsync();

            return entity != null ? MapToProgress(entity) : null;
        }

        /// <summary>
        /// 领取成就奖励。
        /// </summary>
        /// <param name="player">当前玩家实体。</param>
        /// <param name="achievementId">成就编号。</param>
        /// <param name="inventory">兼容旧接口保留的背包参数，当前流程不直接使用。</param>
        /// <returns>领取结果与奖励明细。</returns>
        public async Task<AchievementClaimResult> ClaimRewardAsync(UserEntity player, string achievementId, InventoryManager? inventory = null)
        {
            await EnsureInitializedAsync();
            await SyncSnapshotRequirementsAsync(player?.GID ?? string.Empty);

            if (player == null)
            {
                return new AchievementClaimResult { Success = false, Message = "玩家对象为空" };
            }

            var config = await GetAchievementConfigAsync(achievementId);
            if (config == null)
            {
                return new AchievementClaimResult { Success = false, Message = "成就不存在" };
            }

            var progressEntity = await GetOrCreateProgressEntityAsync(player.GID, achievementId);
            if (progressEntity.Status != (int)AchievementStatus.Completed)
            {
                return new AchievementClaimResult { Success = false, Message = "成就未完成，无法领取奖励" };
            }

            AchievementRewardResult rewardResult;

            try
            {
                _dbContext.BeginTransaction();

                rewardResult = await GrantAchievementRewardsAsync(player.GID, config, achievementId);

                progressEntity.Status = (int)AchievementStatus.Claimed;
                progressEntity.ClaimTime = DateTime.Now;
                progressEntity.LastUpdateTime = DateTime.Now;
                await _db.Updateable(progressEntity).ExecuteCommandAsync();

                _dbContext.CommitTransaction();
            }
            catch (Exception ex)
            {
                _dbContext.RollbackTransaction();
                _logger.LogWarning(ex, "Achievement reward claim failed. PlayerId={PlayerId}, AchievementId={AchievementId}", player.GID, achievementId);
                return new AchievementClaimResult
                {
                    Success = false,
                    Message = "成就奖励发放失败，请稍后重试。",
                    AchievementId = achievementId
                };
            }

            if (config.Rewards.Exp > 0)
            {
                await _playerAttributeService.RecalculatePlayerAttributesAsync(player.GID, syncLevelDrivenProgress: true);
            }
            else
            {
                await _gameSyncService.SyncPlayerAsync(player.GID);
            }

            _logger.LogInformation("Player {PlayerId} claimed achievement reward {AchievementId}", player.GID, achievementId);

            return new AchievementClaimResult
            {
                Success = rewardResult.Success,
                Message = rewardResult.Success
                    ? $"完成成就：{config.AchievementName}\n{rewardResult.GetFullMessage()}"
                    : rewardResult.GetFullMessage(),
                AchievementId = achievementId,
                PointsEarned = config.Points,
                RewardResult = rewardResult
            };
        }

        /// <summary>
        /// 将成就直接标记为完成。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="achievementId">成就编号。</param>
        /// <returns>成功返回真；当前状态不允许完成时返回假。</returns>
        public async Task<bool> CompleteAchievementAsync(string playerId, string achievementId)
        {
            await EnsureInitializedAsync();
            var progressEntity = await GetOrCreateProgressEntityAsync(playerId, achievementId);
            if (progressEntity.Status != (int)AchievementStatus.NotStarted && progressEntity.Status != (int)AchievementStatus.InProgress)
            {
                return false;
            }

            var config = await GetAchievementConfigAsync(achievementId);
            if (config == null)
            {
                return false;
            }

            progressEntity.Status = (int)AchievementStatus.Completed;
            progressEntity.CompleteTime = DateTime.Now;
            await _db.Updateable(progressEntity).ExecuteCommandAsync();


            return true;
        }

        /// <summary>
        /// 更新成就进度值。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="achievementId">成就编号。</param>
        /// <param name="progress">新的累计进度。</param>
        public async Task UpdateProgressAsync(string playerId, string achievementId, long progress)
        {
            await EnsureInitializedAsync();
            var config = await GetAchievementConfigAsync(achievementId);
            if (config == null || config.Requirements.Count == 0)
            {
                return;
            }

            var progressEntity = await GetOrCreateProgressEntityAsync(playerId, achievementId);
            var requirementProgress = DeserializeRequirementProgress(progressEntity, config.Requirements.Count);
            requirementProgress[0] = Math.Max(0L, progress);
            ApplyRequirementProgressSnapshot(progressEntity, config, requirementProgress);
            await _db.Storageable(progressEntity).ExecuteCommandAsync();
        }

        /// <summary>
        /// 记录一次成就统计事件。
        /// </summary>
        public async Task RecordRequirementEventAsync(string playerId, AchievementRequirementEvent requirementEvent)
        {
            await EnsureInitializedAsync();
            if (string.IsNullOrWhiteSpace(playerId) || requirementEvent == null)
            {
                return;
            }

            var delta = Math.Max(0L, requirementEvent.Delta);
            if (delta <= 0)
            {
                return;
            }

            var normalizedTargetId = NormalizeTargetId(requirementEvent.TargetId);
            await AddMetricValueAsync(playerId, requirementEvent.RequirementType, normalizedTargetId, delta);
            if (!string.IsNullOrWhiteSpace(normalizedTargetId))
            {
                await AddMetricValueAsync(playerId, requirementEvent.RequirementType, string.Empty, delta);
            }

            await SyncAchievementsForRequirementAsync(playerId, requirementEvent.RequirementType, normalizedTargetId);
            if (!string.IsNullOrWhiteSpace(normalizedTargetId))
            {
                await SyncAchievementsForRequirementAsync(playerId, requirementEvent.RequirementType, string.Empty);
            }
        }

        /// <summary>
        /// 同步快照型成就要求。
        /// </summary>
        public async Task SyncRequirementStateAsync(string playerId, AchievementRequirementType requirementType)
        {
            await EnsureInitializedAsync();
            if (string.IsNullOrWhiteSpace(playerId))
            {
                return;
            }

            long? snapshotValue = null;
            switch (requirementType)
            {
                case AchievementRequirementType.ReachLevel:
                case AchievementRequirementType.TotalGoldEarned:
                case AchievementRequirementType.TotalWins:
                case AchievementRequirementType.TotalKills:
                {
                    var user = await _db.Queryable<UserEntity>()
                        .Where(entity => entity.GID == playerId && !entity.IsDeleted)
                        .FirstAsync();

                    if (user != null)
                    {
                        snapshotValue = requirementType switch
                        {
                            AchievementRequirementType.ReachLevel => user.Level,
                            AchievementRequirementType.TotalGoldEarned => user.TotalGoldEarned,
                            AchievementRequirementType.TotalWins => user.WinBattles,
                            AchievementRequirementType.TotalKills => user.TotalKills,
                            _ => null
                        };
                    }

                    break;
                }
                case AchievementRequirementType.MaxEnhanceLevel:
                {
                    var enhanceLevels = await _db.Queryable<EquipmentInstanceEntity>()
                        .Where(entity => entity.PlayerId == playerId)
                        .Select(entity => entity.EnhanceLevel)
                        .ToListAsync();

                    snapshotValue = enhanceLevels.Count == 0
                        ? 0
                        : enhanceLevels.Max();
                    break;
                }
            }

            if (!snapshotValue.HasValue)
            {
                return;
            }

            await SetMetricValueAsync(playerId, requirementType, string.Empty, Math.Max(0L, snapshotValue.Value));
            await SyncAchievementsForRequirementAsync(playerId, requirementType, string.Empty);
        }

        /// <summary>
        /// 获取玩家成就统计摘要。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <returns>包含完成数、进行中数量和点数统计的结果。</returns>
        public async Task<AchievementStats> GetAchievementStatsAsync(string playerId)
        {
            await EnsureInitializedAsync();
            var completedCount = await _db.Queryable<AchievementProgressEntity>()
                .Where(p => p.PlayerId == playerId && p.Status == (int)AchievementStatus.Claimed)
                .CountAsync();

            var inProgressCount = await _db.Queryable<AchievementProgressEntity>()
                .Where(p => p.PlayerId == playerId && (p.Status == (int)AchievementStatus.InProgress || p.Status == (int)AchievementStatus.Completed))
                .CountAsync();

            var totalPoints = await GetPlayerAchievementPointsAsync(playerId);

            return new AchievementStats
            {
                TotalAchievements = _achievementConfigs.Count,
                CompletedCount = completedCount,
                InProgressCount = inProgressCount,
                CompletionPercent = _achievementConfigs.Count > 0 ? (float)completedCount / _achievementConfigs.Count * 100f : 0f,
                TotalPoints = totalPoints
            };
        }

        /// <summary>
        /// 计算玩家已领取成就奖励对应的总点数。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <returns>当前累计成就点数。</returns>
        public async Task<int> GetPlayerAchievementPointsAsync(string playerId)
        {
            await EnsureInitializedAsync();
            var claimedAchievements = await _db.Queryable<AchievementProgressEntity>()
                .Where(p => p.PlayerId == playerId && p.Status == (int)AchievementStatus.Claimed)
                .ToListAsync();

            int totalPoints = 0;
            foreach (var achievement in claimedAchievements)
            {
                if (_achievementConfigs.TryGetValue(achievement.AchievementId, out var config))
                {
                    totalPoints += config.Points;
                }
            }

            return totalPoints;
        }

        private async Task SyncAchievementsForRequirementAsync(string playerId, AchievementRequirementType requirementType, string? targetId)
        {
            var normalizedTargetId = NormalizeTargetId(targetId);
            var candidates = _achievementConfigs.Values
                .Where(config => config.Requirements.Any(requirement =>
                    requirement.RequirementType == requirementType &&
                    RequirementMatchesTarget(requirement, normalizedTargetId)))
                .ToList();

            foreach (var config in candidates)
            {
                var progressEntity = await GetOrCreateProgressEntityAsync(playerId, config.AchievementId);
                var requirementProgress = DeserializeRequirementProgress(progressEntity, config.Requirements.Count);

                for (var index = 0; index < config.Requirements.Count; index++)
                {
                    var requirement = config.Requirements[index];
                    if (requirement.RequirementType != requirementType)
                    {
                        continue;
                    }

                    if (!RequirementMatchesTarget(requirement, normalizedTargetId))
                    {
                        continue;
                    }

                    requirementProgress[index] = await GetMetricValueAsync(playerId, requirement.RequirementType, requirement.TargetId);
                }

                ApplyRequirementProgressSnapshot(progressEntity, config, requirementProgress);
                await _db.Storageable(progressEntity).ExecuteCommandAsync();
            }
        }

        private async Task<long> GetMetricValueAsync(string playerId, AchievementRequirementType requirementType, string? targetId)
        {
            var normalizedTargetId = NormalizeTargetId(targetId);
            var counter = await _db.Queryable<AchievementMetricCounterEntity>()
                .Where(entity =>
                    entity.PlayerId == playerId &&
                    entity.RequirementType == (int)requirementType &&
                    entity.TargetId == normalizedTargetId)
                .FirstAsync();

            return Math.Max(0L, counter?.CurrentValue ?? 0L);
        }

        private async Task AddMetricValueAsync(string playerId, AchievementRequirementType requirementType, string? targetId, long delta)
        {
            var normalizedTargetId = NormalizeTargetId(targetId);
            var counter = await _db.Queryable<AchievementMetricCounterEntity>()
                .Where(entity =>
                    entity.PlayerId == playerId &&
                    entity.RequirementType == (int)requirementType &&
                    entity.TargetId == normalizedTargetId)
                .FirstAsync();

            if (counter == null)
            {
                counter = new AchievementMetricCounterEntity
                {
                    PlayerId = playerId,
                    RequirementType = (int)requirementType,
                    TargetId = normalizedTargetId,
                    CurrentValue = Math.Max(0L, delta),
                    LastUpdateTime = DateTime.Now
                };
                await _db.Insertable(counter).ExecuteCommandAsync();
                return;
            }

            counter.CurrentValue += Math.Max(0L, delta);
            counter.LastUpdateTime = DateTime.Now;
            await _db.Updateable(counter).ExecuteCommandAsync();
        }

        private async Task SetMetricValueAsync(string playerId, AchievementRequirementType requirementType, string? targetId, long currentValue)
        {
            var normalizedTargetId = NormalizeTargetId(targetId);
            var counter = await _db.Queryable<AchievementMetricCounterEntity>()
                .Where(entity =>
                    entity.PlayerId == playerId &&
                    entity.RequirementType == (int)requirementType &&
                    entity.TargetId == normalizedTargetId)
                .FirstAsync();

            if (counter == null)
            {
                counter = new AchievementMetricCounterEntity
                {
                    PlayerId = playerId,
                    RequirementType = (int)requirementType,
                    TargetId = normalizedTargetId,
                    CurrentValue = Math.Max(0L, currentValue),
                    LastUpdateTime = DateTime.Now
                };
                await _db.Insertable(counter).ExecuteCommandAsync();
                return;
            }

            counter.CurrentValue = Math.Max(0L, currentValue);
            counter.LastUpdateTime = DateTime.Now;
            await _db.Updateable(counter).ExecuteCommandAsync();
        }

        private static bool RequirementMatchesTarget(AchievementRequirement requirement, string? targetId)
        {
            var normalizedRequirementTargetId = NormalizeTargetId(requirement.TargetId);
            var normalizedTargetId = NormalizeTargetId(targetId);
            if (string.IsNullOrWhiteSpace(normalizedRequirementTargetId))
            {
                return string.IsNullOrWhiteSpace(normalizedTargetId);
            }

            return string.Equals(normalizedRequirementTargetId, normalizedTargetId, StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeTargetId(string? targetId)
        {
            return (targetId ?? string.Empty).Trim();
        }

        private static List<long> DeserializeRequirementProgress(AchievementProgressEntity entity, int requirementCount)
        {
            List<long> values;
            if (!string.IsNullOrWhiteSpace(entity.RequirementProgressJson))
            {
                try
                {
                    values = JsonSerializer.Deserialize<List<long>>(entity.RequirementProgressJson) ?? [];
                }
                catch
                {
                    values = [];
                }
            }
            else
            {
                values = [];
            }

            while (values.Count < requirementCount)
            {
                values.Add(values.Count == 0 ? Math.Max(0L, entity.CurrentProgress) : 0L);
            }

            if (values.Count == 0)
            {
                values.Add(Math.Max(0L, entity.CurrentProgress));
            }

            return values;
        }

        private static void ApplyRequirementProgressSnapshot(
            AchievementProgressEntity entity,
            AchievementConfig config,
            IReadOnlyList<long> requirementProgress)
        {
            var normalizedValues = requirementProgress
                .Take(config.Requirements.Count)
                .Select(value => Math.Max(0L, value))
                .ToList();

            while (normalizedValues.Count < config.Requirements.Count)
            {
                normalizedValues.Add(0L);
            }

            entity.RequirementProgressJson = JsonSerializer.Serialize(normalizedValues);
            entity.CurrentProgress = normalizedValues.Count > 0
                ? normalizedValues.Sum(value => value)
                : 0;
            entity.TargetProgress = config.Requirements.Count > 0
                ? config.Requirements.Sum(requirement => Math.Max(0L, requirement.TargetValue))
                : 0;
            entity.LastUpdateTime = DateTime.Now;

            if (entity.Status == (int)AchievementStatus.NotStarted && normalizedValues.Any(value => value > 0))
            {
                entity.Status = (int)AchievementStatus.InProgress;
                entity.AcceptTime ??= DateTime.Now;
            }

            var progressModel = new AchievementProgress
            {
                AchievementId = entity.AchievementId,
                Status = (AchievementStatus)entity.Status,
                CompleteTime = entity.CompleteTime,
                ClaimTime = entity.ClaimTime
            };
            for (var index = 0; index < normalizedValues.Count; index++)
            {
                progressModel.SetProgressValue(index, normalizedValues[index]);
            }

            if (entity.Status != (int)AchievementStatus.Claimed && config.IsAllRequirementsCompleted(progressModel))
            {
                entity.Status = (int)AchievementStatus.Completed;
                entity.CompleteTime ??= DateTime.Now;
            }
        }

        private async Task<AchievementProgressEntity> GetOrCreateProgressEntityAsync(string playerId, string achievementId)
        {
            var entity = await _db.Queryable<AchievementProgressEntity>()
                .Where(p => p.PlayerId == playerId && p.AchievementId == achievementId)
                .FirstAsync();

            if (entity != null)
            {
                return entity;
            }

            var config = await GetAchievementConfigAsync(achievementId);
            entity = new AchievementProgressEntity
            {
                GID = Guid.NewGuid().ToString("N"),
                PlayerId = playerId,
                AchievementId = achievementId,
                CurrentProgress = 0,
                TargetProgress = config?.Requirements.Sum(requirement => Math.Max(0L, requirement.TargetValue)) ?? 0L,
                RequirementProgressJson = JsonSerializer.Serialize(Enumerable.Repeat(0L, Math.Max(1, config?.Requirements.Count ?? 1)).ToList()),
                Status = (int)AchievementStatus.NotStarted,
                LastUpdateTime = DateTime.Now
            };

            await _db.Insertable(entity).ExecuteCommandAsync();
            return entity;
        }

        private async Task<AchievementRewardResult> GrantAchievementRewardsAsync(
            string playerId,
            AchievementConfig config,
            string achievementId)
        {
            var result = new AchievementRewardResult
            {
                Success = true,
                GrantedItems = [],
                GrantedEquipment = []
            };

            var rewardDefinitions = BuildRewardDefinitions(config.Rewards);
            if (rewardDefinitions.Count > 0)
            {
                var grantedRewards = await _playerRewardService.GrantRewardsAsync(
                    playerId,
                    rewardDefinitions,
                    $"achievement:{achievementId}");

                foreach (var grantedReward in grantedRewards)
                {
                    switch (grantedReward.Type)
                    {
                        case RewardTypes.Exp:
                            result.GrantedExp += grantedReward.Count;
                            result.Messages.Add($"获得 {grantedReward.Count} 修为");
                            break;

                        case RewardTypes.Gold:
                            result.GrantedGold += grantedReward.Count;
                            result.Messages.Add($"获得 {grantedReward.Count} 金币");
                            break;

                        case RewardTypes.SpiritStone:
                            result.GrantedSpiritStone += (int)grantedReward.Count;
                            result.Messages.Add($"获得 {grantedReward.Count} 灵石");
                            break;

                        case RewardTypes.Honor:
                            result.GrantedHonor += (int)grantedReward.Count;
                            result.Messages.Add($"获得 {grantedReward.Count} 荣誉");
                            break;

                        case RewardTypes.GuildContribution:
                            result.GrantedGuildContribution += (int)grantedReward.Count;
                            result.Messages.Add($"获得 {grantedReward.Count} 公会贡献");
                            break;

                        case RewardTypes.Item:
                            if (!string.IsNullOrWhiteSpace(grantedReward.ItemId))
                            {
                                result.GrantedItems.Add(grantedReward.ItemId);
                            }

                            result.Messages.Add($"获得 {grantedReward.Name} x{grantedReward.Count}");
                            break;
                    }
                }
            }

            var usedEquipmentSlots = await _db.Queryable<EquipmentInstanceEntity>()
                .Where(equipment => equipment.PlayerId == playerId && !equipment.IsEquipped)
                .CountAsync();
            var equipmentCapacity = await InventoryCapacityRules.GetEquipmentCapacityAsync(_db, playerId);
            foreach (var equipmentId in config.Rewards.EquipmentIds)
            {
                if (usedEquipmentSlots >= equipmentCapacity)
                {
                    result.Messages.Add("装备背包已满，装备奖励未发放，请先清理背包后再领取。");
                    continue;
                }

                var equipmentEntity = CreateEquipmentRewardInstanceEntity(playerId, equipmentId);
                await _db.Insertable(equipmentEntity).ExecuteCommandAsync();
                usedEquipmentSlots++;
                result.GrantedEquipment.Add(equipmentEntity.InstanceId);
                result.Messages.Add($"获得 {equipmentEntity.Name}");
            }

            if (!string.IsNullOrWhiteSpace(config.Rewards.Title))
            {
                var grantedTitle = config.Rewards.Title.Trim();
                var granted = await _titleService.GrantTitleAsync(playerId, grantedTitle);

                if (granted)
                {
                    result.GrantedTitle = grantedTitle;
                    result.Messages.Add($"获得称号：{grantedTitle}");
                    _logger.LogInformation(
                        "Achievement {AchievementId} granted title {Title} for player {PlayerId}",
                        achievementId,
                        grantedTitle,
                        playerId);
                }
                else
                {
                    _logger.LogInformation(
                        "Achievement {AchievementId} title {Title} already owned by player {PlayerId}",
                        achievementId,
                        grantedTitle,
                        playerId);
                }
            }

            return result;
        }

        private static List<RewardGrantItemDto> BuildRewardDefinitions(AchievementReward rewards)
        {
            var rewardDefinitions = new List<RewardGrantItemDto>();

            if (rewards.Exp > 0)
            {
                rewardDefinitions.Add(new RewardGrantItemDto { Type = RewardTypes.Exp, Count = rewards.Exp });
            }

            if (rewards.Gold > 0)
            {
                rewardDefinitions.Add(new RewardGrantItemDto { Type = RewardTypes.Gold, Count = rewards.Gold });
            }

            if (rewards.SpiritStone > 0)
            {
                rewardDefinitions.Add(new RewardGrantItemDto { Type = RewardTypes.SpiritStone, Count = rewards.SpiritStone });
            }

            if (rewards.Honor > 0)
            {
                rewardDefinitions.Add(new RewardGrantItemDto { Type = RewardTypes.Honor, Count = rewards.Honor });
            }

            if (rewards.GuildContribution > 0)
            {
                rewardDefinitions.Add(new RewardGrantItemDto { Type = RewardTypes.GuildContribution, Count = rewards.GuildContribution });
            }

            foreach (var item in rewards.Items)
            {
                rewardDefinitions.Add(new RewardGrantItemDto
                {
                    Type = RewardTypes.Item,
                    ItemId = item.Key,
                    Count = item.Value,
                });
            }

            return rewardDefinitions;
        }

        private static EquipmentInstanceEntity CreateEquipmentRewardInstanceEntity(string playerId, int equipmentId)
        {
            if (!XXX.GameData.EquipmentTemplates.TryGetValue(equipmentId, out var template))
            {
                throw new InvalidOperationException($"成就奖励装备模板不存在：{equipmentId}");
            }

            return EquipmentBalanceHelper.CreateEntity(playerId, template, false);
        }

        private AchievementConfig MapToConfig(AchievementConfigEntity entity)
        {
            var config = new AchievementConfig
            {
                AchievementId = entity.AchievementId,
                AchievementName = entity.AchievementName,
                AchievementType = (AchievementType)entity.AchievementType,
                Difficulty = (AchievementDifficulty)entity.Difficulty,
                Description = entity.Description,
                Category = entity.Category,
                Points = entity.Points,
                IsHidden = entity.IsHidden,
                SortOrder = entity.SortOrder
            };

            if (!string.IsNullOrEmpty(entity.PreAchievementIds))
            {
                config.PreAchievementIds = entity.PreAchievementIds.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            config.Rewards.Gold = entity.RewardGold;
            config.Rewards.SpiritStone = entity.RewardSpiritStone;
            config.Rewards.Exp = entity.RewardExp;
            config.Rewards.Title = entity.RewardTitle ?? string.Empty;

            if (!string.IsNullOrEmpty(entity.RewardItemsJson))
            {
                try
                {
                    var items = JsonSerializer.Deserialize<Dictionary<string, int>>(entity.RewardItemsJson);
                    if (items != null)
                    {
                        config.Rewards.Items = items;
                    }
                }
                catch
                {
                }
            }

            if (!string.IsNullOrEmpty(entity.RewardEquipmentIds))
            {
                var equipIds = entity.RewardEquipmentIds.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var id in equipIds)
                {
                    if (int.TryParse(id, out int equipId))
                    {
                        config.Rewards.EquipmentIds.Add(equipId);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(entity.RequirementsJson))
            {
                try
                {
                    var requirements = JsonSerializer.Deserialize<List<AchievementRequirement>>(entity.RequirementsJson);
                    if (requirements != null)
                    {
                        config.Requirements = requirements
                            .Where(requirement => requirement != null)
                            .Select(requirement => new AchievementRequirement
                            {
                                RequirementType = requirement.RequirementType,
                                TargetId = NormalizeTargetId(requirement.TargetId),
                                TargetValue = Math.Max(0L, requirement.TargetValue),
                                Description = requirement.Description ?? string.Empty
                            })
                            .ToList();
                    }
                }
                catch
                {
                }
            }

            if (config.Requirements.Count == 0)
            {
                config.Requirements.Add(new AchievementRequirement
                {
                    RequirementType = (AchievementRequirementType)entity.RequirementType,
                    TargetValue = entity.RequirementTargetValue,
                    Description = entity.RequirementDescription ?? string.Empty
                });
            }

            return config;
        }

        private AchievementProgress MapToProgress(AchievementProgressEntity entity)
        {
            var progress = new AchievementProgress
            {
                AchievementId = entity.AchievementId,
                Status = (AchievementStatus)entity.Status,
                CompleteTime = entity.CompleteTime,
                ClaimTime = entity.ClaimTime
            };

            if (!string.IsNullOrWhiteSpace(entity.RequirementProgressJson))
            {
                try
                {
                    var values = JsonSerializer.Deserialize<List<long>>(entity.RequirementProgressJson) ?? [];
                    for (var index = 0; index < values.Count; index++)
                    {
                        progress.SetProgressValue(index, values[index]);
                    }
                }
                catch
                {
                    progress.SetProgressValue(0, entity.CurrentProgress);
                }
            }
            else
            {
                progress.SetProgressValue(0, entity.CurrentProgress);
            }
            return progress;
        }


    }
}
