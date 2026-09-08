using Microsoft.Extensions.Logging;
using SqlSugar;
using System.Text.Json;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Data;
using XXX.Inventory;
using XXX.Quest;

namespace XXX.Application.Services
{
    /// <summary>
    /// 任务服务实现类
    /// 提供任务配置管理、接取、进度追踪、提交等任务系统功能的实现
    /// </summary>
    /// <remarks>
    /// 主要职责：
    /// - 加载和管理任务配置
    /// - 处理任务接取和放弃
    /// - 追踪任务目标进度
    /// - 处理任务完成和奖励发放
    /// - 管理日常/周常任务重置
    /// </remarks>
    public class QuestService : IQuestService
    {
        private readonly DbContext _dbContext;

        /// <summary>
        /// 数据库客户端，用于数据持久化操作
        /// </summary>
        private readonly ISqlSugarClient _db;

        private readonly IPlayerRewardService _playerRewardService;
        private readonly IPlayerAttributeService _playerAttributeService;
        private readonly IGameSyncService _gameSyncService;

        /// <summary>
        /// 日志记录器
        /// </summary>
        private readonly ILogger<QuestService> _logger;

        /// <summary>
        /// 任务配置缓存，键为任务ID
        /// </summary>
        private static readonly Dictionary<string, QuestConfig> SharedQuestConfigs = [];
        private static readonly SemaphoreSlim InitializationLock = new(1, 1);
        private readonly Dictionary<string, QuestConfig> _questConfigs = SharedQuestConfigs;
        private static bool _isInitialized;

        /// <summary>
        /// 上次日常任务重置时间
        /// </summary>
        private DateTime _lastDailyReset = DateTime.Today;

        /// <summary>
        /// 上次周常任务重置时间
        /// </summary>
        private DateTime _lastWeeklyReset = GetLastMonday();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dbContext">数据库上下文</param>
        /// <param name="logger">日志记录器</param>
        /// <param name="playerRewardService">玩家奖励服务</param>
        /// <param name="playerAttributeService">玩家属性服务</param>
        /// <param name="gameSyncService">玩家状态同步服务</param>
        public QuestService(
            DbContext dbContext,
            IPlayerRewardService playerRewardService,
            IPlayerAttributeService playerAttributeService,
            IGameSyncService gameSyncService,
            ILogger<QuestService> logger)
        {
            _dbContext = dbContext;
            _db = dbContext.Db;
            _playerRewardService = playerRewardService;
            _playerAttributeService = playerAttributeService;
            _gameSyncService = gameSyncService;
            _logger = logger;
        }

        private async Task EnsureInitializedAsync()
        {
            if (_isInitialized && _questConfigs.Count > 0)
            {
                return;
            }

            await InitializeAsync();
        }

        private async Task EnsureQuestResetsAsync()
        {
            await CheckDailyResetAsync();
            await CheckWeeklyResetAsync();
        }

        /// <summary>
        /// 初始化任务配置缓存。
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_isInitialized && _questConfigs.Count > 0)
            {
                return;
            }

            await ReloadCacheAsync();
        }

        /// <summary>
        /// 重新从数据库装载任务配置缓存。
        /// </summary>
        public async Task ReloadCacheAsync()
        {
            await InitializationLock.WaitAsync();
            try
            {
                var totalConfigCount = await _db.Queryable<QuestConfigEntity>().CountAsync();
                if (totalConfigCount == 0)
                {
                    _logger.LogError(
                        "Quest configs are missing. Runtime quest seeding has been disabled; database configs are required.");
                    throw new InvalidOperationException(
                        "Quest configs are missing. Please run startup seed sync before using quests.");
                }

                var configEntities = await _db.Queryable<QuestConfigEntity>()
                    .Where(c => c.IsEnabled)
                    .ToListAsync();

                _questConfigs.Clear();
                foreach (var entity in configEntities)
                {
                    var config = MapToConfig(entity);
                    _questConfigs[config.QuestId] = config;
                }

                _isInitialized = true;
                _logger.LogInformation("Quest system cache reloaded with {Count} quests", _questConfigs.Count);
            }
            finally
            {
                InitializationLock.Release();
            }
        }

        /// <summary>
        /// 获取全部任务配置。
        /// </summary>
        /// <returns>按排序字段升序返回的任务配置列表。</returns>
        public async Task<List<QuestConfig>> GetAllQuestConfigsAsync()
        {
            await EnsureInitializedAsync();
            return _questConfigs.Values.OrderBy(q => q.SortOrder).ToList();
        }

        /// <summary>
        /// 获取单条任务配置。
        /// </summary>
        /// <param name="questId">任务编号。</param>
        /// <returns>命中的任务配置；不存在时返回空。</returns>
        public async Task<QuestConfig?> GetQuestConfigAsync(string questId)
        {
            await EnsureInitializedAsync();
            return _questConfigs.TryGetValue(questId, out var config) ? config : null;
        }

        /// <summary>
        /// 获取玩家当前可接取的任务列表。
        /// </summary>
        /// <param name="player">玩家实体。</param>
        /// <returns>满足条件且尚未接取的任务集合。</returns>
        public async Task<List<QuestConfig>> GetAvailableQuestsAsync(UserEntity player)
        {
            await EnsureInitializedAsync();
            await EnsureQuestResetsAsync();
            if (player == null) return [];

            await EnsureAutoAcceptedQuestsAsync(player);

            var completedQuestIds = await GetPlayerCompletedQuestIdsAsync(player.GID);
            var inProgressQuestIds = (await GetPlayerInProgressQuestsAsync(player.GID))
                .Select(q => q.QuestId)
                .ToHashSet();

            return _questConfigs.Values
                .Where(q => !inProgressQuestIds.Contains(q.QuestId) &&
                            q.CanAccept(player, completedQuestIds))
                .OrderBy(q => q.SortOrder)
                .ToList();
        }

        /// <summary>
        /// 获取玩家进行中的任务进度。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <returns>状态为进行中的任务集合。</returns>
        public async Task<List<QuestProgress>> GetPlayerInProgressQuestsAsync(string playerId)
        {
            await EnsureInitializedAsync();
            await EnsureQuestResetsAsync();
            var player = await _db.Queryable<UserEntity>()
                .Where(user => user.GID == playerId && !user.IsDeleted)
                .FirstAsync();
            if (player != null)
            {
                await EnsureAutoAcceptedQuestsAsync(player);
            }

            var entities = await _db.Queryable<QuestProgressEntity>()
                .Where(p => p.PlayerId == playerId && p.Status == (int)XXX.Quest.QuestStatus.InProgress)
                .ToListAsync();

            return entities.Select(MapToProgress).ToList();
        }

        /// <summary>
        /// 获取玩家已完成待提交的任务进度。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <returns>状态为已完成的任务集合。</returns>
        public async Task<List<QuestProgress>> GetPlayerCompletedQuestsAsync(string playerId)
        {
            await EnsureInitializedAsync();
            await EnsureQuestResetsAsync();
            var entities = await _db.Queryable<QuestProgressEntity>()
                .Where(p => p.PlayerId == playerId && p.Status == (int)XXX.Quest.QuestStatus.Completed)
                .ToListAsync();

            return entities.Select(MapToProgress).ToList();
        }

        /// <summary>
        /// 获取单条任务进度。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="questId">任务编号。</param>
        /// <returns>命中的任务进度；不存在时返回空。</returns>
        public async Task<QuestProgress?> GetQuestProgressAsync(string playerId, string questId)
        {
            await EnsureInitializedAsync();
            await EnsureQuestResetsAsync();
            var entity = await GetQuestProgressEntityAsync(playerId, questId);

            return entity != null ? MapToProgress(entity) : null;
        }

        /// <summary>
        /// 接取任务。
        /// </summary>
        /// <param name="player">当前玩家实体。</param>
        /// <param name="questId">任务编号。</param>
        /// <returns>接取结果。</returns>
        public async Task<QuestOperationResult> AcceptQuestAsync(UserEntity player, string questId)
        {
            await EnsureInitializedAsync();
            await EnsureQuestResetsAsync();
            if (player == null)
            {
                return new QuestOperationResult { Success = false, Message = "玩家对象为空" };
            }

            var questConfig = await GetQuestConfigAsync(questId);
            if (questConfig == null)
            {
                return new QuestOperationResult { Success = false, Message = "任务不存在" };
            }

            var completedQuests = await GetPlayerCompletedQuestIdsAsync(player.GID);
            if (!questConfig.CanAccept(player, completedQuests))
            {
                return new QuestOperationResult { Success = false, Message = "不满足接取条件" };
            }

            var existingProgressEntity = await GetQuestProgressEntityAsync(player.GID, questId);
            if (existingProgressEntity != null)
            {
                return BuildExistingQuestProgressResult(questId, (XXX.Quest.QuestStatus)existingProgressEntity.Status);
            }

            var progress = new QuestProgressEntity
            {
                GID = Guid.NewGuid().ToString("N"),
                PlayerId = player.GID,
                QuestId = questId,
                CurrentStage = 0,
                ObjectiveProgressJson = JsonSerializer.Serialize(new List<int>(new int[questConfig.Objectives.Count])),
                Status = (int)XXX.Quest.QuestStatus.InProgress,
                AcceptTime = DateTime.Now,
                LastUpdateTime = DateTime.Now
            };

            try
            {
                await _db.Insertable(progress).ExecuteCommandAsync();
            }
            catch (Exception ex)
            {
                if (!IsQuestProgressDuplicate(ex))
                {
                    throw;
                }

                var duplicatedProgress = await GetQuestProgressEntityAsync(player.GID, questId);
                if (duplicatedProgress == null)
                {
                    _logger.LogWarning(
                        ex,
                        "Quest accept hit duplicate key but no quest progress row was found. PlayerId={PlayerId}, QuestId={QuestId}",
                        player.GID,
                        questId);
                    throw;
                }

                _logger.LogInformation(
                    "Player {PlayerId} already has quest {QuestId} progress, skipping duplicate accept",
                    player.GID,
                    questId);

                return BuildExistingQuestProgressResult(questId, (XXX.Quest.QuestStatus)duplicatedProgress.Status);
            }

                _logger.LogInformation("Player {PlayerId} accepted quest {QuestId}", player.GID, questId);

                await SyncQuestStateObjectivesAsync(player, questConfig);

            return new QuestOperationResult
            {
                Success = true,
                Message = $"接取任务：{questConfig.QuestName}",
                QuestId = questId
            };
        }

        private async Task<QuestProgressEntity?> GetQuestProgressEntityAsync(string playerId, string questId)
        {
            return await _db.Queryable<QuestProgressEntity>()
                .Where(p => p.PlayerId == playerId && p.QuestId == questId)
                .FirstAsync();
        }

        private static QuestOperationResult BuildExistingQuestProgressResult(string questId, XXX.Quest.QuestStatus status)
        {
            var message = status == XXX.Quest.QuestStatus.Completed
                ? "任务已完成待提交"
                : "任务已在进行中";

            return new QuestOperationResult
            {
                Success = false,
                Message = message,
                QuestId = questId
            };
        }

        private static bool IsQuestProgressDuplicate(Exception ex)
        {
            const string duplicateMessage = "UNIQUE constraint failed: quest_progress.PlayerId, quest_progress.QuestId";

            for (var current = ex; current != null; current = current.InnerException)
            {
                if (current.Message.Contains(duplicateMessage, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 放弃任务。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="questId">任务编号。</param>
        /// <returns>放弃结果。</returns>
        public async Task<QuestOperationResult> AbandonQuestAsync(string playerId, string questId)
        {
            await EnsureInitializedAsync();
            var progress = await GetQuestProgressAsync(playerId, questId);
            if (progress == null)
            {
                return new QuestOperationResult { Success = false, Message = "任务不存在" };
            }

            if (progress.Status != XXX.Quest.QuestStatus.InProgress)
            {
                return new QuestOperationResult { Success = false, Message = "只能放弃进行中的任务" };
            }

            var questConfig = await GetQuestConfigAsync(questId);
            if (questConfig != null && questConfig.QuestType == QuestType.Main)
            {
                return new QuestOperationResult { Success = false, Message = "主线任务不能放弃" };
            }

            await _db.Deleteable<QuestProgressEntity>()
                .Where(p => p.PlayerId == playerId && p.QuestId == questId)
                .ExecuteCommandAsync();
            _logger.LogInformation("Player {PlayerId} abandoned quest {QuestId}", playerId, questId);

            return new QuestOperationResult
            {
                Success = true,
                Message = "已放弃任务",
                QuestId = questId
            };
        }

        /// <summary>
        /// 提交已完成任务并发放奖励。
        /// </summary>
        /// <param name="player">当前玩家实体。</param>
        /// <param name="questId">任务编号。</param>
        /// <param name="inventory">兼容旧接口保留的背包参数，当前流程不直接使用。</param>
        /// <returns>提交结果与奖励明细。</returns>
        public async Task<QuestSubmitResult> SubmitQuestAsync(UserEntity player, string questId, InventoryManager? inventory = null)
        {
            await EnsureInitializedAsync();
            if (player == null)
            {
                return new QuestSubmitResult { Success = false, Message = "玩家对象为空" };
            }

            var questConfig = await GetQuestConfigAsync(questId);
            if (questConfig == null)
            {
                return new QuestSubmitResult { Success = false, Message = "任务不存在" };
            }

            var progress = await GetQuestProgressAsync(player.GID, questId);
            if (progress == null)
            {
                return new QuestSubmitResult { Success = false, Message = "任务进度不存在" };
            }

            if (progress.Status != XXX.Quest.QuestStatus.Completed)
            {
                return new QuestSubmitResult { Success = false, Message = "任务未完成" };
            }

            QuestRewardResult rewardResult;

            try
            {
                _dbContext.BeginTransaction();

                rewardResult = await GrantQuestRewardsAsync(player.GID, questConfig, questId);

                await _db.Deleteable<QuestProgressEntity>()
                    .Where(p => p.PlayerId == player.GID && p.QuestId == questId)
                    .ExecuteCommandAsync();

                var recordEntity = new QuestCompletedRecordEntity
                {
                    GID = Guid.NewGuid().ToString("N"),
                    PlayerId = player.GID,
                    QuestId = questId,
                    CompleteTime = progress.CompleteTime ?? DateTime.Now,
                    SubmitTime = DateTime.Now
                };
                await _db.Insertable(recordEntity).ExecuteCommandAsync();

                _dbContext.CommitTransaction();
            }
            catch (Exception ex)
            {
                _dbContext.RollbackTransaction();
                _logger.LogWarning(ex, "Quest submit failed. PlayerId={PlayerId}, QuestId={QuestId}", player.GID, questId);
                return new QuestSubmitResult
                {
                    Success = false,
                    Message = "任务奖励发放失败，请稍后重试。",
                    QuestId = questId
                };
            }

            if (questConfig.Rewards.Exp > 0)
            {
                await _playerAttributeService.RecalculatePlayerAttributesAsync(player.GID, syncLevelDrivenProgress: true);
            }
            else
            {
                await _gameSyncService.SyncPlayerAsync(player.GID);
            }

            _logger.LogInformation("Player {PlayerId} submitted quest {QuestId}", player.GID, questId);

            return new QuestSubmitResult
            {
                Success = rewardResult.Success,
                Message = rewardResult.Success
                    ? $"完成任务：{questConfig.QuestName}\n{rewardResult.GetFullMessage()}"
                    : rewardResult.GetFullMessage(),
                QuestId = questId,
                RewardResult = rewardResult
            };
        }

        /// <summary>
        /// 将任务标记为完成。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="questId">任务编号。</param>
        /// <returns>成功返回真；条件不满足时返回假。</returns>
        public async Task<bool> CompleteQuestAsync(string playerId, string questId)
        {
            await EnsureInitializedAsync();
            var progressEntity = await _db.Queryable<QuestProgressEntity>()
                .Where(p => p.PlayerId == playerId && p.QuestId == questId && p.Status == (int)XXX.Quest.QuestStatus.InProgress)
                .FirstAsync();

            if (progressEntity == null) return false;

            var questConfig = await GetQuestConfigAsync(questId);
            if (questConfig == null) return false;

            var progress = MapToProgress(progressEntity);
            if (!questConfig.IsAllObjectivesCompleted(progress)) return false;

            progressEntity.Status = (int)XXX.Quest.QuestStatus.Completed;
            progressEntity.CompleteTime = DateTime.Now;
            progressEntity.LastUpdateTime = DateTime.Now;

            await _db.Updateable(progressEntity).ExecuteCommandAsync();
            return true;
        }

        /// <summary>
        /// 增量更新任务目标进度。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="questId">任务编号。</param>
        /// <param name="objectiveIndex">目标索引。</param>
        /// <param name="delta">增加的进度值。</param>
        /// <returns>更新成功返回真；未命中任务时返回假。</returns>
        public async Task<bool> UpdateObjectiveProgressAsync(string playerId, string questId, int objectiveIndex, int delta)
        {
            await EnsureInitializedAsync();
            if (delta <= 0)
            {
                return false;
            }

            return await ApplyObjectiveProgressDeltaAsync(playerId, questId, objectiveIndex, delta);
        }

        /// <summary>
        /// 记录一次任务事件。
        /// </summary>
        public async Task RecordObjectiveEventAsync(string playerId, QuestObjectiveEvent objectiveEvent)
        {
            await EnsureInitializedAsync();
            if (string.IsNullOrWhiteSpace(playerId) || objectiveEvent == null)
            {
                return;
            }

            var progressList = await GetPlayerInProgressQuestsAsync(playerId);
            if (progressList.Count == 0)
            {
                return;
            }

            foreach (var questProgress in progressList)
            {
                var questConfig = await GetQuestConfigAsync(questProgress.QuestId);
                if (questConfig == null)
                {
                    continue;
                }

                for (var index = 0; index < questConfig.Objectives.Count; index++)
                {
                    var objective = questConfig.Objectives[index];
                    var delta = GetObjectiveEventDelta(objective, objectiveEvent);
                    if (delta <= 0)
                    {
                        continue;
                    }

                    await ApplyObjectiveProgressDeltaAsync(playerId, questConfig.QuestId, index, delta);
                }
            }
        }

        /// <summary>
        /// 同步指定类型的快照型任务目标。
        /// </summary>
        public async Task SyncObjectiveStateAsync(string playerId, ObjectiveType objectiveType)
        {
            await EnsureInitializedAsync();
            if (string.IsNullOrWhiteSpace(playerId))
            {
                return;
            }

            var progressList = await GetPlayerInProgressQuestsAsync(playerId);
            if (progressList.Count == 0)
            {
                return;
            }

            UserEntity? player = null;
            int? maxEnhanceLevel = null;

            foreach (var questProgress in progressList)
            {
                var questConfig = await GetQuestConfigAsync(questProgress.QuestId);
                if (questConfig == null)
                {
                    continue;
                }

                for (var index = 0; index < questConfig.Objectives.Count; index++)
                {
                    var objective = questConfig.Objectives[index];
                    if (objective.ObjectiveType != objectiveType)
                    {
                        continue;
                    }

                    switch (objectiveType)
                    {
                        case ObjectiveType.ReachLevel:
                            player ??= await _db.Queryable<UserEntity>()
                                .Where(user => user.GID == playerId && !user.IsDeleted)
                                .FirstAsync();
                            if (player == null)
                            {
                                continue;
                            }

                            var requiredLevel = ResolveObjectiveThreshold(objective);
                            var levelProgress = requiredLevel > 0 && player.Level >= requiredLevel
                                ? Math.Max(1, objective.TargetCount)
                                : 0;
                            await SetObjectiveProgressAsync(playerId, questConfig.QuestId, index, levelProgress);
                            break;
                        case ObjectiveType.EquipEnhance:
                            maxEnhanceLevel ??= await GetMaxEnhanceLevelAsync(playerId);
                            var requiredEnhanceLevel = ResolveObjectiveThreshold(objective);
                            var enhanceProgress = requiredEnhanceLevel > 0 && maxEnhanceLevel.Value >= requiredEnhanceLevel
                                ? Math.Max(1, objective.TargetCount)
                                : 0;
                            await SetObjectiveProgressAsync(playerId, questConfig.QuestId, index, enhanceProgress);
                            break;
                        case ObjectiveType.EquipReachEnhanceLevel:
                            var threshold = ResolveObjectiveThreshold(objective);
                            var qualifiedCount = threshold > 0
                                ? await CountEquipmentsAtOrAboveAsync(playerId, threshold)
                                : 0;
                            await SetObjectiveProgressAsync(playerId, questConfig.QuestId, index, qualifiedCount);
                            break;
                        case ObjectiveType.CollectItem:
                            var heldCount = string.IsNullOrWhiteSpace(objective.TargetId)
                                ? 0
                                : await GetInventoryItemCountAsync(playerId, objective.TargetId);
                            await SetObjectiveProgressAsync(playerId, questConfig.QuestId, index, heldCount);
                            break;
                    }
                }
            }
        }

        private async Task<bool> ApplyObjectiveProgressDeltaAsync(string playerId, string questId, int objectiveIndex, int delta)
        {
            var progressEntity = await _db.Queryable<QuestProgressEntity>()
                .Where(p => p.PlayerId == playerId && p.QuestId == questId && p.Status == (int)XXX.Quest.QuestStatus.InProgress)
                .FirstAsync();

            if (progressEntity == null) return false;

            var objectiveProgress = JsonSerializer.Deserialize<List<int>>(progressEntity.ObjectiveProgressJson ?? "[]") ?? [];
            while (objectiveProgress.Count <= objectiveIndex)
            {
                objectiveProgress.Add(0);
            }

            objectiveProgress[objectiveIndex] += delta;
            progressEntity.ObjectiveProgressJson = JsonSerializer.Serialize(objectiveProgress);
            progressEntity.LastUpdateTime = DateTime.Now;

            var questConfig = await GetQuestConfigAsync(questId);
            if (questConfig != null)
            {
                var progress = MapToProgress(progressEntity);
                for (int i = 0; i < objectiveProgress.Count && i < questConfig.Objectives.Count; i++)
                {
                    progress.SetObjectiveProgress(i, objectiveProgress[i]);
                }

                if (questConfig.IsAllObjectivesCompleted(progress))
                {
                    progressEntity.Status = (int)XXX.Quest.QuestStatus.Completed;
                    progressEntity.CompleteTime = DateTime.Now;
                }
            }

            await _db.Updateable(progressEntity).ExecuteCommandAsync();
            return true;
        }

        private async Task<bool> SetObjectiveProgressAsync(string playerId, string questId, int objectiveIndex, int progressValue)
        {
            await EnsureInitializedAsync();
            var progressEntity = await _db.Queryable<QuestProgressEntity>()
                .Where(p => p.PlayerId == playerId && p.QuestId == questId && p.Status == (int)XXX.Quest.QuestStatus.InProgress)
                .FirstAsync();

            if (progressEntity == null)
            {
                return false;
            }

            var objectiveProgress = JsonSerializer.Deserialize<List<int>>(progressEntity.ObjectiveProgressJson ?? "[]") ?? [];
            while (objectiveProgress.Count <= objectiveIndex)
            {
                objectiveProgress.Add(0);
            }

            var normalizedProgress = Math.Max(0, progressValue);
            if (objectiveProgress[objectiveIndex] == normalizedProgress)
            {
                return true;
            }

            objectiveProgress[objectiveIndex] = normalizedProgress;
            progressEntity.ObjectiveProgressJson = JsonSerializer.Serialize(objectiveProgress);
            progressEntity.LastUpdateTime = DateTime.Now;

            var questConfig = await GetQuestConfigAsync(questId);
            if (questConfig != null)
            {
                var progress = MapToProgress(progressEntity);
                for (var i = 0; i < objectiveProgress.Count && i < questConfig.Objectives.Count; i++)
                {
                    progress.SetObjectiveProgress(i, objectiveProgress[i]);
                }

                if (questConfig.IsAllObjectivesCompleted(progress))
                {
                    progressEntity.Status = (int)XXX.Quest.QuestStatus.Completed;
                    progressEntity.CompleteTime = DateTime.Now;
                }
            }

            await _db.Updateable(progressEntity).ExecuteCommandAsync();
            return true;
        }

        /// <summary>
        /// 检查并执行日常任务重置。
        /// </summary>
        public async Task CheckDailyResetAsync()
        {
            await EnsureInitializedAsync();
            if (DateTime.Today > _lastDailyReset)
            {
                var dailyQuestIds = _questConfigs.Values
                    .Where(q => q.ResetCycle == QuestResetCycle.Daily)
                    .Select(q => q.QuestId)
                    .ToList();

                await _db.Deleteable<QuestProgressEntity>()
                    .Where(p => dailyQuestIds.Contains(p.QuestId))
                    .ExecuteCommandAsync();

                await _db.Deleteable<QuestCompletedRecordEntity>()
                    .Where(p => dailyQuestIds.Contains(p.QuestId))
                    .ExecuteCommandAsync();

                _lastDailyReset = DateTime.Today;
                _logger.LogInformation("Daily quests reset completed");
            }
        }

        /// <summary>
        /// 检查并执行周常任务重置。
        /// </summary>
        public async Task CheckWeeklyResetAsync()
        {
            await EnsureInitializedAsync();
            var thisMonday = GetLastMonday();
            if (thisMonday > _lastWeeklyReset)
            {
                var weeklyQuestIds = _questConfigs.Values
                    .Where(q => q.ResetCycle == QuestResetCycle.Weekly)
                    .Select(q => q.QuestId)
                    .ToList();

                await _db.Deleteable<QuestProgressEntity>()
                    .Where(p => weeklyQuestIds.Contains(p.QuestId))
                    .ExecuteCommandAsync();

                await _db.Deleteable<QuestCompletedRecordEntity>()
                    .Where(p => weeklyQuestIds.Contains(p.QuestId))
                    .ExecuteCommandAsync();

                _lastWeeklyReset = thisMonday;
                _logger.LogInformation("Weekly quests reset completed");
            }
        }

        private async Task<HashSet<string>> GetPlayerCompletedQuestIdsAsync(string playerId)
        {
            var records = await _db.Queryable<QuestCompletedRecordEntity>()
                .Where(r => r.PlayerId == playerId)
                .ToListAsync();

            return records.Select(r => r.QuestId).ToHashSet();
        }

        private async Task<QuestRewardResult> GrantQuestRewardsAsync(
            string playerId,
            QuestConfig questConfig,
            string questId)
        {
            var result = new QuestRewardResult
            {
                Success = true,
                GrantedItems = [],
                GrantedEquipment = []
            };

            var rewardDefinitions = BuildRewardDefinitions(questConfig.Rewards);
            if (rewardDefinitions.Count > 0)
            {
                var grantedRewards = await _playerRewardService.GrantRewardsAsync(
                    playerId,
                    rewardDefinitions,
                    $"quest:{questId}");

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
            foreach (var equipmentId in questConfig.Rewards.EquipmentIds)
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

            return result;
        }

        private static List<RewardGrantItemDto> BuildRewardDefinitions(QuestReward rewards)
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
                throw new InvalidOperationException($"任务奖励装备模板不存在：{equipmentId}");
            }

            return EquipmentBalanceHelper.CreateEntity(playerId, template, false);
        }

        private QuestConfig MapToConfig(QuestConfigEntity entity)
        {
            var config = new QuestConfig
            {
                QuestId = entity.QuestId,
                QuestName = entity.QuestName,
                QuestType = (QuestType)entity.QuestType,
                ResetCycle = (QuestResetCycle)Math.Max(0, entity.ResetCycle),
                Description = entity.Description,
                RequiredLevel = entity.RequiredLevel,
                AutoAccept = entity.AutoAccept,
                AutoSubmit = entity.AutoSubmit,
                TimeLimit = entity.TimeLimit,
                SortOrder = entity.SortOrder
            };

            if (!string.IsNullOrEmpty(entity.PreQuestIds))
            {
                config.PreQuestIds = entity.PreQuestIds.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            config.Rewards.Exp = entity.RewardExp;
            config.Rewards.Gold = entity.RewardGold;
            config.Rewards.SpiritStone = entity.RewardSpiritStone;

            if (!string.IsNullOrEmpty(entity.RewardItemsJson))
            {
                try
                {
                    var items = JsonSerializer.Deserialize<Dictionary<string, int>>(entity.RewardItemsJson);
                    if (items != null)
                    {
                        if (items.TryGetValue("contribution", out var contribution))
                        {
                            config.Rewards.GuildContribution = contribution;
                            items.Remove("contribution");
                        }
                        config.Rewards.Items = items;
                    }
                }
                catch { }
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

            if (!string.IsNullOrEmpty(entity.ObjectivesJson))
            {
                try
                {
                    var objectives = JsonSerializer.Deserialize<List<QuestObjective>>(entity.ObjectivesJson);
                    if (objectives != null)
                    {
                        config.Objectives = NormalizeObjectives(objectives);
                    }
                }
                catch { }
            }

            ApplyStarterMonsterQuestCompatibility(config);

            return config;
        }

        private async Task EnsureAutoAcceptedQuestsAsync(UserEntity player)
        {
            var completedQuestIds = await GetPlayerCompletedQuestIdsAsync(player.GID);
            var inProgressQuestIds = (await _db.Queryable<QuestProgressEntity>()
                    .Where(progress => progress.PlayerId == player.GID && progress.Status == (int)XXX.Quest.QuestStatus.InProgress)
                    .ToListAsync())
                .Select(progress => progress.QuestId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var autoAcceptQuests = _questConfigs.Values
                .Where(quest =>
                    quest.AutoAccept &&
                    !inProgressQuestIds.Contains(quest.QuestId) &&
                    quest.CanAccept(player, completedQuestIds))
                .OrderBy(quest => quest.SortOrder)
                .ToList();

            foreach (var quest in autoAcceptQuests)
            {
                var acceptResult = await AcceptQuestAsync(player, quest.QuestId);
                if (!acceptResult.Success &&
                    !string.Equals(acceptResult.Message, "任务已在进行中", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                inProgressQuestIds.Add(quest.QuestId);
            }
        }

        private async Task SyncQuestStateObjectivesAsync(UserEntity player, QuestConfig quest)
        {
            for (var i = 0; i < quest.Objectives.Count; i++)
            {
                var objective = quest.Objectives[i];
                switch (objective.ObjectiveType)
                {
                    case ObjectiveType.ReachLevel:
                    {
                        var requiredLevel = ResolveObjectiveThreshold(objective);
                        if (requiredLevel > 0 && player.Level >= requiredLevel)
                        {
                            await SetObjectiveProgressAsync(player.GID, quest.QuestId, i, Math.Max(1, objective.TargetCount));
                        }
                        break;
                    }
                    case ObjectiveType.CollectItem:
                    {
                        if (string.IsNullOrWhiteSpace(objective.TargetId))
                        {
                            break;
                        }

                        var currentCount = await GetInventoryItemCountAsync(player.GID, objective.TargetId.Trim());
                        if (currentCount > 0)
                        {
                            await SetObjectiveProgressAsync(player.GID, quest.QuestId, i, currentCount);
                        }
                        break;
                    }
                    case ObjectiveType.EquipEnhance:
                    {
                        var requiredEnhanceLevel = ResolveObjectiveThreshold(objective);
                        if (requiredEnhanceLevel <= 0)
                        {
                            break;
                        }

                        var maxEnhanceLevel = await GetMaxEnhanceLevelAsync(player.GID);
                        if (maxEnhanceLevel >= requiredEnhanceLevel)
                        {
                            await SetObjectiveProgressAsync(player.GID, quest.QuestId, i, Math.Max(1, objective.TargetCount));
                        }
                        break;
                    }
                    case ObjectiveType.EquipReachEnhanceLevel:
                    {
                        var requiredEnhanceLevel = ResolveObjectiveThreshold(objective);
                        if (requiredEnhanceLevel <= 0)
                        {
                            break;
                        }

                        var qualifiedCount = await CountEquipmentsAtOrAboveAsync(player.GID, requiredEnhanceLevel);
                        if (qualifiedCount > 0)
                        {
                            await SetObjectiveProgressAsync(player.GID, quest.QuestId, i, qualifiedCount);
                        }
                        break;
                    }
                }
            }
        }

        private static int GetObjectiveEventDelta(QuestObjective objective, QuestObjectiveEvent objectiveEvent)
        {
            if (objective.ObjectiveType != objectiveEvent.ObjectiveType)
            {
                return 0;
            }

            switch (objective.ObjectiveType)
            {
                case ObjectiveType.KillMonster:
                    return MatchesMonsterTarget(objective, objectiveEvent.TargetId)
                        ? Math.Max(0, objectiveEvent.Delta)
                        : 0;
                case ObjectiveType.CollectItem:
                case ObjectiveType.CompleteDungeon:
                case ObjectiveType.MapBattleCount:
                case ObjectiveType.MapWinCount:
                case ObjectiveType.AlchemySuccess:
                case ObjectiveType.ForgeSuccess:
                case ObjectiveType.PlantCrop:
                case ObjectiveType.HarvestCrop:
                    return MatchesTargetId(objective.TargetId, objectiveEvent.TargetId)
                        ? Math.Max(0, objectiveEvent.Delta)
                        : 0;
                case ObjectiveType.AlchemyCollect:
                case ObjectiveType.ForgeCollect:
                case ObjectiveType.EquipEnhanceCount:
                case ObjectiveType.WinBattle:
                case ObjectiveType.Donate:
                case ObjectiveType.SectBossDamage:
                    return Math.Max(0, objectiveEvent.Delta);
                case ObjectiveType.BattleActivity:
                    return objective.ActivityType == objectiveEvent.ActivityType
                        ? Math.Max(0, objectiveEvent.Delta)
                        : 0;
                case ObjectiveType.ReachLevel:
                    return objectiveEvent.CurrentValue >= ResolveObjectiveThreshold(objective)
                        ? Math.Max(1, objective.TargetCount)
                        : 0;
                case ObjectiveType.EquipEnhance:
                    return objectiveEvent.CurrentValue >= ResolveObjectiveThreshold(objective)
                        ? Math.Max(1, objective.TargetCount)
                        : 0;
                default:
                    return 0;
            }
        }

        private static bool MatchesTargetId(string? objectiveTargetId, string? eventTargetId)
        {
            var normalizedObjectiveTargetId = (objectiveTargetId ?? string.Empty).Trim();
            var normalizedEventTargetId = (eventTargetId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedObjectiveTargetId))
            {
                return true;
            }
            return string.Equals(normalizedObjectiveTargetId, normalizedEventTargetId, StringComparison.OrdinalIgnoreCase);
        }

        private static bool MatchesMonsterTarget(QuestObjective objective, string? eventTargetId)
        {
            var normalizedEventTargetId = (eventTargetId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedEventTargetId))
            {
                return false;
            }

            if (MatchesTargetId(objective.TargetId, normalizedEventTargetId))
            {
                return true;
            }

            var normalizedObjectiveTargetId = (objective.TargetId ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(normalizedObjectiveTargetId) &&
                global::XXX.GameData.MonsterTemplates.TryGetValue(normalizedObjectiveTargetId, out var monsterTemplate) &&
                !string.IsNullOrWhiteSpace(monsterTemplate.Name))
            {
                return string.Equals(monsterTemplate.Name.Trim(), normalizedEventTargetId, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private static int ResolveObjectiveThreshold(QuestObjective objective)
        {
            if (objective.TargetValue > 0)
            {
                return objective.TargetValue;
            }

            return int.TryParse(objective.TargetId, out var parsedValue) && parsedValue > 0
                ? parsedValue
                : 0;
        }

        private async Task<int> GetInventoryItemCountAsync(string playerId, string itemId)
        {
            return await _db.Queryable<InventoryItemEntity>()
                .Where(item => item.PlayerId == playerId && item.ItemId == itemId)
                .SumAsync(item => item.Quantity);
        }

        private async Task<int> GetMaxEnhanceLevelAsync(string playerId)
        {
            var maxEnhanceLevel = await _db.Queryable<EquipmentInstanceEntity>()
                .Where(equipment => equipment.PlayerId == playerId)
                .MaxAsync(equipment => (int?)equipment.EnhanceLevel);
            return Math.Max(0, maxEnhanceLevel ?? 0);
        }

        private async Task<int> CountEquipmentsAtOrAboveAsync(string playerId, int requiredEnhanceLevel)
        {
            if (requiredEnhanceLevel <= 0)
            {
                return 0;
            }

            return await _db.Queryable<EquipmentInstanceEntity>()
                .Where(equipment => equipment.PlayerId == playerId && equipment.EnhanceLevel >= requiredEnhanceLevel)
                .CountAsync();
        }

        private static List<QuestObjective> NormalizeObjectives(IEnumerable<QuestObjective> objectives)
        {
            return objectives?
                .Where(objective => objective != null)
                .Select(NormalizeObjective)
                .ToList() ?? [];
        }

        private static QuestObjective NormalizeObjective(QuestObjective objective)
        {
            // 兼容旧JSON格式：Type→ObjectiveType, Count→TargetCount, Value→TargetValue
            if (objective.ExtensionData != null)
            {
                if (objective.ObjectiveType == default && objective.ExtensionData.TryGetValue("Type", out var typeElem) && typeElem.ValueKind == JsonValueKind.String)
                {
                    if (Enum.TryParse<ObjectiveType>(typeElem.GetString(), ignoreCase: true, out var parsedType))
                        objective.ObjectiveType = parsedType;
                }
                if (objective.TargetCount == 0 && objective.ExtensionData.TryGetValue("Count", out var countElem) && countElem.ValueKind == JsonValueKind.Number)
                {
                    if (countElem.TryGetInt32(out var parsedCount))
                        objective.TargetCount = parsedCount;
                }
                if (objective.TargetValue == 0 && objective.ExtensionData.TryGetValue("Value", out var valueElem) && valueElem.ValueKind == JsonValueKind.Number)
                {
                    if (valueElem.TryGetInt32(out var parsedVal))
                        objective.TargetValue = parsedVal;
                }
                objective.ExtensionData = null;
            }

            var normalizedType = objective.ObjectiveType;
            var normalizedTargetId = (objective.TargetId ?? string.Empty).Trim();
            var normalizedActivityType = normalizedType == ObjectiveType.BattleActivity
                ? objective.ActivityType
                : default;
            var normalizedTargetValue = Math.Max(0, objective.TargetValue);

            if (normalizedType == ObjectiveType.BattleActivity && string.IsNullOrWhiteSpace(normalizedTargetId))
            {
                normalizedTargetId = objective.ActivityType == BattleActivityType.WinCount
                    ? "win_count"
                    : string.Empty;
            }

            if ((normalizedType == ObjectiveType.ReachLevel || normalizedType == ObjectiveType.EquipEnhance) &&
                normalizedTargetValue <= 0 &&
                int.TryParse(normalizedTargetId, out var parsedValue) &&
                parsedValue > 0)
            {
                normalizedTargetValue = parsedValue;
            }

            return new QuestObjective
            {
                ObjectiveType = normalizedType,
                TargetId = normalizedTargetId,
                TargetCount = objective.TargetCount > 0 ? objective.TargetCount : 1,
                TargetValue = normalizedTargetValue,
                Description = (objective.Description ?? string.Empty).Trim(),
                ActivityType = normalizedActivityType
            };
        }

        private static void ApplyStarterMonsterQuestCompatibility(QuestConfig config)
        {
            if (!string.Equals(config.QuestId, "main_002", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var starterObjective = config.Objectives
                .FirstOrDefault(objective => objective.ObjectiveType == ObjectiveType.KillMonster);
            if (starterObjective == null)
            {
                return;
            }

            var starterMonsterName = ResolveMonsterDisplayName(starterObjective.TargetId, "新手村妖物");
            config.QuestName = $"击败{starterMonsterName}";
            config.Description = $"{starterMonsterName}在附近出没，击败10只{starterMonsterName}来证明你的实力！";
            starterObjective.Description = $"击败{starterMonsterName}";
        }

        private static string ResolveMonsterDisplayName(string? monsterTemplateId, string fallbackName)
        {
            if (!string.IsNullOrWhiteSpace(monsterTemplateId) &&
                global::XXX.GameData.MonsterTemplates.TryGetValue(monsterTemplateId.Trim(), out var monsterTemplate) &&
                !string.IsNullOrWhiteSpace(monsterTemplate.Name))
            {
                return monsterTemplate.Name.Trim();
            }

            return fallbackName;
        }

        private QuestProgress MapToProgress(QuestProgressEntity entity)
        {
            var progress = new QuestProgress
            {
                QuestId = entity.QuestId,
                Status = (XXX.Quest.QuestStatus)entity.Status,
                AcceptTime = entity.AcceptTime ?? DateTime.Now,
                CompleteTime = entity.CompleteTime,
                SubmitTime = entity.SubmitTime,
                CurrentStage = entity.CurrentStage
            };

            if (!string.IsNullOrEmpty(entity.ObjectiveProgressJson))
            {
                try
                {
                    var objectiveProgress = JsonSerializer.Deserialize<List<int>>(entity.ObjectiveProgressJson);
                    if (objectiveProgress != null)
                    {
                        for (int i = 0; i < objectiveProgress.Count; i++)
                        {
                            progress.SetObjectiveProgress(i, objectiveProgress[i]);
                        }
                    }
                }
                catch { }
            }

            return progress;
        }

        private static DateTime GetLastMonday()
        {
            var today = DateTime.Today;
            var daysFromMonday = (int)today.DayOfWeek - 1;
            if (daysFromMonday < 0) daysFromMonday += 7;
            return today.AddDays(-daysFromMonday);
        }
    }
}
