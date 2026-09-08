using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;
using XXX.Achievement;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Equipment;
using XXX.Infrastructure.Data;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    /// <summary>
    /// 装备服务实现。
    /// </summary>
    /// <remarks>
    /// 该实现专门负责“装备实例”这一层的真实业务闭环：
    /// 1. 查询玩家拥有的装备。
    /// 2. 穿戴、卸下后重算玩家属性。
    /// 3. 强化、出售后同步玩家战力/财富排行榜。
    /// </remarks>
    public class EquipmentService : IEquipmentService
    {
        private readonly DbContext _dbContext;
        private readonly IRepository<EquipmentInstanceEntity> _equipmentRepository;
        private readonly IRepository<UserEntity> _userRepository;
        private readonly IPlayerAttributeService _playerAttributeService;
        private readonly IGameSyncService _gameSyncService;
        private readonly IAchievementService _achievementService;
        private readonly IQuestService _questService;
        private readonly ILogger<EquipmentService> _logger;
        private readonly Random _random;

        /// <summary>
        /// 初始化装备服务。
        /// </summary>
        /// <param name="dbContext">数据库上下文。</param>
        /// <param name="equipmentRepository">装备仓储。</param>
        /// <param name="userRepository">玩家仓储。</param>
        /// <param name="playerAttributeService">玩家属性服务。</param>
        /// <param name="gameSyncService">游戏同步服务。</param>
        /// <param name="achievementService">成就服务。</param>
        /// <param name="questService">任务服务。</param>
        /// <param name="logger">日志记录器。</param>
        public EquipmentService(
            DbContext dbContext,
            IRepository<EquipmentInstanceEntity> equipmentRepository,
            IRepository<UserEntity> userRepository,
            IPlayerAttributeService playerAttributeService,
            IGameSyncService gameSyncService,
            IAchievementService achievementService,
            IQuestService questService,
            ILogger<EquipmentService> logger)
        {
            _dbContext = dbContext;
            _equipmentRepository = equipmentRepository;
            _userRepository = userRepository;
            _playerAttributeService = playerAttributeService;
            _gameSyncService = gameSyncService;
            _achievementService = achievementService;
            _questService = questService;
            _logger = logger;
            _random = new Random();
        }

        /// <summary>
        /// 获取玩家拥有的全部装备。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <returns>按穿戴状态、品质和强化等级排序后的装备列表。</returns>
        public async Task<List<EquipmentDto>> GetPlayerEquipmentsAsync(string playerId)
        {
            var equipments = await _equipmentRepository.GetListAsync(e => e.PlayerId == playerId);
            return equipments
                .OrderByDescending(e => e.IsEquipped)
                .ThenByDescending(e => e.Quality)
                .ThenByDescending(e => e.EnhanceLevel)
                .ThenByDescending(e => e.AcquiredTime)
                .Select(MapToDto)
                .ToList();
        }

        /// <summary>
        /// 获取玩家当前已穿戴的装备。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <returns>按部位排序的已穿戴装备列表。</returns>
        public async Task<List<EquipmentDto>> GetEquippedItemsAsync(string playerId)
        {
            var equipments = await _equipmentRepository.GetListAsync(e => e.PlayerId == playerId && e.IsEquipped);
            return equipments
                .OrderBy(e => e.Slot)
                .Select(MapToDto)
                .ToList();
        }

        /// <summary>
        /// 获取单件装备详情。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="equipmentId">装备实例编号。</param>
        /// <returns>命中的装备详情；不存在时返回空。</returns>
        public async Task<EquipmentDto?> GetEquipmentDetailAsync(string playerId, string equipmentId)
        {
            var equipment = await _equipmentRepository.GetFirstAsync(e => e.PlayerId == playerId && e.InstanceId == equipmentId);
            return equipment == null ? null : MapToDto(equipment);
        }

        /// <summary>
        /// 穿戴指定装备。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="request">穿戴请求。</param>
        /// <returns>穿戴成功返回真。</returns>
        public async Task<bool> EquipItemAsync(string playerId, EquipItemRequestDto request)
        {
            var equipment = await _equipmentRepository.GetFirstAsync(e => e.PlayerId == playerId && e.InstanceId == request.EquipmentId);
            if (equipment == null)
            {
                return false;
            }

            var player = await _userRepository.GetByIdAsync(playerId);
            if (player == null)
            {
                return false;
            }

            var requiredLevel = GetRequiredLevel(equipment);
            if (player.Level < requiredLevel)
            {
                _logger.LogWarning(
                    "Player {PlayerId} level too low for equipment {EquipmentId}. requiredLevel={RequiredLevel}, currentLevel={CurrentLevel}",
                    playerId,
                    request.EquipmentId,
                    requiredLevel,
                    player.Level);
                return false;
            }

            try
            {
                // 中文注释：穿戴同部位装备时，必须先把旧装备统一卸下，再把目标装备标记为已穿戴。
                // 这一步放在事务中，是为了防止中途失败后出现“同一部位同时穿两件装备”的状态。
                _dbContext.Db.Ado.BeginTran();

                await _dbContext.Db.Updateable<EquipmentInstanceEntity>()
                    .SetColumns(e => e.IsEquipped == false)
                    .SetColumns(e => e.LastUpdateTime == DateTime.Now)
                    .Where(e => e.PlayerId == playerId && e.IsEquipped && e.Slot == equipment.Slot)
                    .ExecuteCommandAsync();

                var equipRows = await _dbContext.Db.Updateable<EquipmentInstanceEntity>()
                    .SetColumns(e => e.IsEquipped == true)
                    .SetColumns(e => e.LastUpdateTime == DateTime.Now)
                    .Where(e => e.PlayerId == playerId && e.InstanceId == request.EquipmentId)
                    .ExecuteCommandAsync();

                if (equipRows == 0)
                {
                    _dbContext.Db.Ado.RollbackTran();
                    return false;
                }

                _dbContext.Db.Ado.CommitTran();
            }
            catch (Exception ex)
            {
                _dbContext.Db.Ado.RollbackTran();
                _logger.LogError(ex, "Player {PlayerId} equip equipment {EquipmentId} failed", playerId, request.EquipmentId);
                return false;
            }

            await _playerAttributeService.RecalculatePlayerAttributesAsync(playerId);
            _logger.LogInformation("Player {PlayerId} equipped item {EquipmentId}", playerId, request.EquipmentId);
            return true;
        }

        /// <summary>
        /// 卸下指定部位装备。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="request">卸下请求。</param>
        /// <returns>卸下成功返回真。</returns>
        public async Task<bool> UnequipItemAsync(string playerId, UnequipItemRequestDto request)
        {
            var stopwatch = Stopwatch.StartNew();
            var equipment = await _equipmentRepository.GetFirstAsync(e => e.PlayerId == playerId && e.IsEquipped && e.Slot == request.Slot);
            if (equipment == null)
            {
                _logger.LogInformation(
                    "UnequipItemAsync skipped. PlayerId={PlayerId}, Slot={Slot}, TotalMs={TotalMs}",
                    playerId,
                    request.Slot,
                    stopwatch.ElapsedMilliseconds);
                return false;
            }

            var loadElapsedMs = stopwatch.ElapsedMilliseconds;
            equipment.IsEquipped = false;
            equipment.LastUpdateTime = DateTime.Now;
            await _equipmentRepository.UpdateAsync(equipment);
            var updateElapsedMs = stopwatch.ElapsedMilliseconds;

            await _playerAttributeService.RecalculatePlayerAttributesAsync(playerId);
            var recalcElapsedMs = stopwatch.ElapsedMilliseconds;
            _logger.LogInformation(
                "UnequipItemAsync timing. PlayerId={PlayerId}, EquipmentId={EquipmentId}, LoadMs={LoadMs}, UpdateMs={UpdateMs}, RecalcMs={RecalcMs}, TotalMs={TotalMs}",
                playerId,
                equipment.InstanceId,
                loadElapsedMs,
                updateElapsedMs - loadElapsedMs,
                recalcElapsedMs - updateElapsedMs,
                recalcElapsedMs);
            _logger.LogInformation("Player {PlayerId} unequipped item {EquipmentId}", playerId, equipment.InstanceId);
            return true;
        }

        /// <summary>
        /// 强化装备。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="request">强化请求。</param>
        /// <returns>强化结果、消耗和新强化等级。</returns>
        public async Task<EnhanceResultDto> EnhanceEquipmentAsync(string playerId, EnhanceEquipmentRequestDto request)
        {
            var equipment = await _equipmentRepository.GetFirstAsync(e => e.PlayerId == playerId && e.InstanceId == request.EquipmentId);
            if (equipment == null)
            {
                _logger.LogWarning("Player {PlayerId} tried to enhance missing equipment {EquipmentId}", playerId, request.EquipmentId);
                return new EnhanceResultDto { Success = false, Message = "装备不存在" };
            }

            EquipmentEnhanceRuleEntity enhanceRule;
            try
            {
                enhanceRule = EquipmentRerollRuleRuntimeCatalog.GetEnhanceRule(GetRequiredLevel(equipment));
            }
            catch (InvalidOperationException ex)
            {
                return new EnhanceResultDto
                {
                    Success = false,
                    Message = ex.Message,
                    NewEnhanceLevel = equipment.EnhanceLevel
                };
            }

            if (equipment.EnhanceLevel >= enhanceRule.MaxEnhanceLevel)
            {
                _logger.LogWarning(
                    "Player {PlayerId} tried to enhance equipment {EquipmentId} beyond max level {EnhanceLevel}",
                    playerId,
                    request.EquipmentId,
                    equipment.EnhanceLevel);
                return new EnhanceResultDto
                {
                    Success = false,
                    Message = "装备强化等级已达到上限",
                    NewEnhanceLevel = equipment.EnhanceLevel,
                    CostGold = 0,
                    CostEnhanceStones = 0
                };
            }

            var oldEnhanceLevel = equipment.EnhanceLevel;
            var costGold = enhanceRule.GoldCost;
            var costEnhanceStones = enhanceRule.MaterialCount;
            var successRate = enhanceRule.SuccessRate;
            var isSuccess = _random.Next(100) < successRate;
            var now = DateTime.Now;

            try
            {
                // 中文注释：强化遵循“先扣资源，再写结果”的事务语义。
                // 只要材料、金币、强化等级三者中任一步失败，整个强化都会回滚，避免资产不一致。
                _dbContext.Db.Ado.BeginTran();

                var goldRows = await _dbContext.Db.Updateable<UserEntity>()
                    .SetColumns(u => u.Gold == u.Gold - costGold)
                    .SetColumns(u => u.TotalGoldSpent == u.TotalGoldSpent + costGold)
                    .SetColumns(u => u.LastUpdateTime == now)
                    .Where(u => u.GID == playerId && !u.IsDeleted && u.Gold >= costGold)
                    .ExecuteCommandAsync();

                if (goldRows == 0)
                {
                    _dbContext.Db.Ado.RollbackTran();
                    _logger.LogWarning(
                        "Player {PlayerId} enhance equipment {EquipmentId} failed due to insufficient gold. requiredGold={CostGold}",
                        playerId,
                        request.EquipmentId,
                        costGold);
                    return new EnhanceResultDto
                    {
                        Success = false,
                        Message = "金币不足",
                        NewEnhanceLevel = oldEnhanceLevel,
                        CostGold = costGold,
                        CostEnhanceStones = 0
                    };
                }

                var materialDeductSuccess = await DeductEnhanceMaterialInTransactionAsync(
                    playerId,
                    enhanceRule.MaterialItemId,
                    costEnhanceStones);
                if (!materialDeductSuccess)
                {
                    _dbContext.Db.Ado.RollbackTran();
                    _logger.LogWarning(
                        "Player {PlayerId} enhance equipment {EquipmentId} failed due to insufficient material {MaterialItemId}. requiredCount={MaterialCount}",
                        playerId,
                        request.EquipmentId,
                        enhanceRule.MaterialItemId,
                        costEnhanceStones);
                    var materialName = XXX.GameData.Items.TryGetValue(enhanceRule.MaterialItemId, out var matTpl) ? matTpl.Name : enhanceRule.MaterialItemId;
                    return new EnhanceResultDto
                    {
                        Success = false,
                        Message = $"强化材料不足，需要{materialName}",
                        NewEnhanceLevel = oldEnhanceLevel,
                        CostGold = 0,
                        CostEnhanceStones = 0
                    };
                }

                if (isSuccess)
                {
                    var template = EquipmentBalanceHelper.ResolveTemplate(equipment.TemplateId);
                    var newEnhanceLevel = oldEnhanceLevel + 1;
                    var newPhysicalAttack = ApplyEnhanceGrowth(EquipmentBalanceHelper.GetPhysicalAttack(equipment, template), enhanceRule.AttributeGrowthPercent);
                    var newMagicAttack = ApplyEnhanceGrowth(EquipmentBalanceHelper.GetMagicAttack(equipment, template), enhanceRule.AttributeGrowthPercent);
                    var newPhysicalDefense = ApplyEnhanceGrowth(EquipmentBalanceHelper.GetPhysicalDefense(equipment, template), enhanceRule.AttributeGrowthPercent);
                    var newMagicDefense = ApplyEnhanceGrowth(EquipmentBalanceHelper.GetMagicDefense(equipment, template), enhanceRule.AttributeGrowthPercent);
                    var newBaseHP = ApplyEnhanceGrowth(equipment.BaseHP, enhanceRule.AttributeGrowthPercent);
                    var newBaseMP = ApplyEnhanceGrowth(equipment.BaseMP, enhanceRule.AttributeGrowthPercent);
                    var equipmentRows = await _dbContext.Db.Updateable<EquipmentInstanceEntity>()
                        .SetColumns(e => e.EnhanceLevel == newEnhanceLevel)
                        .SetColumns(e => e.BasePhysicalAttack == newPhysicalAttack)
                        .SetColumns(e => e.BaseMagicAttack == newMagicAttack)
                        .SetColumns(e => e.BasePhysicalDefense == newPhysicalDefense)
                        .SetColumns(e => e.BaseMagicDefense == newMagicDefense)
                        .SetColumns(e => e.BaseHP == newBaseHP)
                        .SetColumns(e => e.BaseMP == newBaseMP)
                        .SetColumns(e => e.LastUpdateTime == now)
                        .Where(e =>
                            e.PlayerId == playerId &&
                            e.InstanceId == request.EquipmentId &&
                            e.EnhanceLevel == oldEnhanceLevel)
                        .ExecuteCommandAsync();

                    if (equipmentRows == 0)
                    {
                        throw new InvalidOperationException("强化失败：装备状态已变化，请重试");
                    }

                    equipment.EnhanceLevel = newEnhanceLevel;
                    equipment.BasePhysicalAttack = newPhysicalAttack;
                    equipment.BaseMagicAttack = newMagicAttack;
                    equipment.BasePhysicalDefense = newPhysicalDefense;
                    equipment.BaseMagicDefense = newMagicDefense;
                    equipment.BaseHP = newBaseHP;
                    equipment.BaseMP = newBaseMP;
                }

                _dbContext.Db.Ado.CommitTran();
            }
            catch (InvalidOperationException ex)
            {
                _dbContext.Db.Ado.RollbackTran();
                _logger.LogWarning(
                    ex,
                    "Player {PlayerId} enhance equipment {EquipmentId} failed. reason={Reason}",
                    playerId,
                    request.EquipmentId,
                    ex.Message);
                return new EnhanceResultDto
                {
                    Success = false,
                    Message = ex.Message,
                    NewEnhanceLevel = oldEnhanceLevel,
                    CostGold = 0,
                    CostEnhanceStones = 0
                };
            }
            catch (Exception ex)
            {
                _dbContext.Db.Ado.RollbackTran();
                _logger.LogError(ex, "Player {PlayerId} enhance equipment {EquipmentId} exception", playerId, request.EquipmentId);
                return new EnhanceResultDto
                {
                    Success = false,
                    Message = "强化失败，请稍后重试",
                    NewEnhanceLevel = oldEnhanceLevel,
                    CostGold = 0,
                    CostEnhanceStones = 0
                };
            }

            // 中文注释：强化成功且装备已穿戴时，必须重算人物属性；未穿戴时也要同步排行，确保财富/战力视图一致。
            if (equipment.IsEquipped && isSuccess)
            {
                await _playerAttributeService.RecalculatePlayerAttributesAsync(playerId);
            }
            else
            {
                await _gameSyncService.SyncPlayerAsync(playerId);
            }

            if (isSuccess)
            {
                await SyncEnhanceAchievementsAsync(playerId);
                await _achievementService.RecordRequirementEventAsync(playerId, new XXX.Achievement.AchievementRequirementEvent
                {
                    RequirementType = XXX.Achievement.AchievementRequirementType.EnhanceSuccessCount,
                    Delta = 1
                });
                await _questService.RecordObjectiveEventAsync(playerId, new XXX.Quest.QuestObjectiveEvent
                {
                    ObjectiveType = XXX.Quest.ObjectiveType.EquipEnhanceCount,
                    Delta = 1,
                    CurrentValue = equipment.EnhanceLevel
                });
                await _questService.RecordObjectiveEventAsync(playerId, new XXX.Quest.QuestObjectiveEvent
                {
                    ObjectiveType = XXX.Quest.ObjectiveType.EquipEnhance,
                    Delta = 1,
                    CurrentValue = equipment.EnhanceLevel
                });
                await _questService.SyncObjectiveStateAsync(playerId, XXX.Quest.ObjectiveType.EquipReachEnhanceLevel);
            }
            else
            {
                await _achievementService.RecordRequirementEventAsync(playerId, new XXX.Achievement.AchievementRequirementEvent
                {
                    RequirementType = XXX.Achievement.AchievementRequirementType.EnhanceFailureCount,
                    Delta = 1
                });
            }

            _logger.LogInformation(
                "Player {PlayerId} enhance equipment {EquipmentId} finished. success={Success}, newLevel={EnhanceLevel}, costGold={CostGold}, materialCount={MaterialCount}",
                playerId,
                request.EquipmentId,
                isSuccess,
                equipment.EnhanceLevel,
                costGold,
                costEnhanceStones);

            return new EnhanceResultDto
            {
                Success = isSuccess,
                Message = isSuccess ? "强化成功" : "强化失败",
                NewEnhanceLevel = equipment.EnhanceLevel,
                CostGold = costGold,
                CostEnhanceStones = costEnhanceStones
            };
        }

        private async Task SyncEnhanceAchievementsAsync(string playerId)
        {
            try
            {
                var achievements = await _achievementService.GetAllAchievementsAsync();
                var enhanceAchievements = achievements
                    .Where(achievement => achievement.Requirements.Any(requirement => requirement.RequirementType == AchievementRequirementType.MaxEnhanceLevel))
                    .ToList();
                if (enhanceAchievements.Count == 0)
                {
                    return;
                }

                var maxEnhanceLevel = await _dbContext.Db.Queryable<EquipmentInstanceEntity>()
                    .Where(equipment => equipment.PlayerId == playerId)
                    .MaxAsync(equipment => equipment.EnhanceLevel);

                foreach (var achievement in enhanceAchievements)
                {
                    await _achievementService.UpdateProgressAsync(playerId, achievement.AchievementId, maxEnhanceLevel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to sync enhance achievements for player {PlayerId}", playerId);
            }
        }

        private async Task<bool> DeductEnhanceMaterialInTransactionAsync(string playerId, string materialItemId, int requiredCount)
        {
            var materials = await _dbContext.Db.Queryable<InventoryItemEntity>()
                .Where(i => i.PlayerId == playerId && i.ItemId == materialItemId)
                .OrderBy(i => i.IsLocked)
                .OrderBy(i => i.Id)
                .ToListAsync();

            var totalCount = materials.Sum(i => i.Quantity);
            if (totalCount < requiredCount)
            {
                return false;
            }

            var remaining = requiredCount;
            foreach (var material in materials)
            {
                if (remaining <= 0)
                {
                    break;
                }

                var deductCount = Math.Min(material.Quantity, remaining);
                if (deductCount == material.Quantity)
                {
                    var deleteRows = await _dbContext.Db.Deleteable<InventoryItemEntity>()
                        .Where(i =>
                            i.Id == material.Id &&
                            i.PlayerId == playerId &&
                            i.ItemId == materialItemId &&
                            i.Quantity == material.Quantity)
                        .ExecuteCommandAsync();

                    if (deleteRows == 0)
                    {
                        return false;
                    }
                }
                else
                {
                    var updateRows = await _dbContext.Db.Updateable<InventoryItemEntity>()
                        .SetColumns(i => i.Quantity == i.Quantity - deductCount)
                        .Where(i =>
                            i.Id == material.Id &&
                            i.PlayerId == playerId &&
                            i.ItemId == materialItemId &&
                            i.Quantity >= deductCount)
                        .ExecuteCommandAsync();

                    if (updateRows == 0)
                    {
                        return false;
                    }
                }

                remaining -= deductCount;
            }

            return remaining == 0;
        }

        /// <summary>
        /// 对比目标装备与当前同部位已穿戴装备。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="newEquipmentId">待对比装备实例编号。</param>
        /// <returns>对比结果；装备不存在时返回空。</returns>
        public async Task<EquipmentCompareDto?> CompareEquipmentAsync(string playerId, string newEquipmentId)
        {
            var newEquipment = await _equipmentRepository.GetFirstAsync(e => e.PlayerId == playerId && e.InstanceId == newEquipmentId);
            if (newEquipment == null)
            {
                return null;
            }

            var currentEquipment = await _equipmentRepository.GetFirstAsync(e => e.PlayerId == playerId && e.IsEquipped && e.Slot == newEquipment.Slot);
            var differences = new List<StatDifferenceDto>();

            if (currentEquipment != null)
            {
                differences.Add(new StatDifferenceDto { StatName = "物攻", CurrentValue = EquipmentBalanceHelper.GetPhysicalAttack(currentEquipment), NewValue = EquipmentBalanceHelper.GetPhysicalAttack(newEquipment) });
                differences.Add(new StatDifferenceDto { StatName = "法攻", CurrentValue = EquipmentBalanceHelper.GetMagicAttack(currentEquipment), NewValue = EquipmentBalanceHelper.GetMagicAttack(newEquipment) });
                differences.Add(new StatDifferenceDto { StatName = "物防", CurrentValue = EquipmentBalanceHelper.GetPhysicalDefense(currentEquipment), NewValue = EquipmentBalanceHelper.GetPhysicalDefense(newEquipment) });
                differences.Add(new StatDifferenceDto { StatName = "法防", CurrentValue = EquipmentBalanceHelper.GetMagicDefense(currentEquipment), NewValue = EquipmentBalanceHelper.GetMagicDefense(newEquipment) });
                differences.Add(new StatDifferenceDto { StatName = "生命", CurrentValue = currentEquipment.BaseHP, NewValue = newEquipment.BaseHP });
                differences.Add(new StatDifferenceDto { StatName = "法力", CurrentValue = currentEquipment.BaseMP, NewValue = newEquipment.BaseMP });
            }

            return new EquipmentCompareDto
            {
                CurrentEquipment = currentEquipment == null ? null : MapToDto(currentEquipment),
                NewEquipment = MapToDto(newEquipment),
                Differences = differences
            };
        }

        /// <summary>
        /// 出售装备。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="equipmentId">装备实例编号。</param>
        /// <returns>成功出售后获得的金币数量；失败时返回 0。</returns>
        public async Task<long> SellEquipmentAsync(string playerId, string equipmentId)
        {
            var equipment = await _equipmentRepository.GetFirstAsync(e => e.PlayerId == playerId && e.InstanceId == equipmentId);
            if (equipment == null || equipment.IsEquipped || equipment.IsLocked)
            {
                return 0;
            }

            var sellPrice = 100L * equipment.Quality * (equipment.EnhanceLevel + 1);
            var now = DateTime.Now;

            try
            {
                _dbContext.Db.Ado.BeginTran();

                var deleteRows = await _dbContext.Db.Deleteable<EquipmentInstanceEntity>()
                    .Where(e =>
                        e.PlayerId == playerId &&
                        e.InstanceId == equipmentId &&
                        !e.IsEquipped &&
                        !e.IsLocked)
                    .ExecuteCommandAsync();

                if (deleteRows == 0)
                {
                    _dbContext.Db.Ado.RollbackTran();
                    return 0;
                }

                var updateRows = await _dbContext.Db.Updateable<UserEntity>()
                    .SetColumns(u => u.Gold == u.Gold + sellPrice)
                    .SetColumns(u => u.TotalGoldEarned == u.TotalGoldEarned + sellPrice)
                    .SetColumns(u => u.LastUpdateTime == now)
                    .Where(u => u.GID == playerId && !u.IsDeleted)
                    .ExecuteCommandAsync();

                if (updateRows == 0)
                {
                    _dbContext.Db.Ado.RollbackTran();
                    return 0;
                }

                _dbContext.Db.Ado.CommitTran();
            }
            catch (Exception ex)
            {
                _dbContext.Db.Ado.RollbackTran();
                _logger.LogError(ex, "Player {PlayerId} sell equipment {EquipmentId} exception", playerId, equipmentId);
                return 0;
            }

            await _gameSyncService.SyncPlayerAsync(playerId);
            _logger.LogInformation("Player {PlayerId} sold equipment {EquipmentId}, gained {Gold}", playerId, equipmentId, sellPrice);
            return sellPrice;
        }

        /// <summary>
        /// 在一个请求、一个事务内批量出售装备，避免前端逐件调用出售接口。
        /// </summary>
        public async Task<BatchSellEquipmentResultDto> SellEquipmentsAsync(string playerId, BatchSellEquipmentRequestDto request)
        {
            var result = new BatchSellEquipmentResultDto();
            var requestedIds = (request?.EquipmentIds ?? [])
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (requestedIds.Count == 0)
            {
                return result;
            }

            var equipments = await _equipmentRepository.GetListAsync(e =>
                e.PlayerId == playerId && requestedIds.Contains(e.InstanceId));
            var sellable = equipments
                .Where(e => !e.IsEquipped && !e.IsLocked)
                .ToList();

            result.FailedCount = requestedIds.Count - sellable.Count;
            if (sellable.Count == 0)
            {
                return result;
            }

            var totalGold = sellable.Sum(e => 100L * e.Quality * (e.EnhanceLevel + 1));
            var now = DateTime.Now;

            try
            {
                _dbContext.Db.Ado.BeginTran();

                var deletedRows = await _dbContext.Db.Deleteable<EquipmentInstanceEntity>()
                    .Where(e => e.PlayerId == playerId && requestedIds.Contains(e.InstanceId) && !e.IsEquipped && !e.IsLocked)
                    .ExecuteCommandAsync();
                if (deletedRows != sellable.Count)
                {
                    _dbContext.Db.Ado.RollbackTran();
                    return new BatchSellEquipmentResultDto { FailedCount = requestedIds.Count };
                }

                var updatedRows = await _dbContext.Db.Updateable<UserEntity>()
                    .SetColumns(u => u.Gold == u.Gold + totalGold)
                    .SetColumns(u => u.TotalGoldEarned == u.TotalGoldEarned + totalGold)
                    .SetColumns(u => u.LastUpdateTime == now)
                    .Where(u => u.GID == playerId && !u.IsDeleted)
                    .ExecuteCommandAsync();
                if (updatedRows == 0)
                {
                    _dbContext.Db.Ado.RollbackTran();
                    return new BatchSellEquipmentResultDto { FailedCount = requestedIds.Count };
                }

                _dbContext.Db.Ado.CommitTran();
            }
            catch (Exception ex)
            {
                _dbContext.Db.Ado.RollbackTran();
                _logger.LogError(ex, "Player {PlayerId} batch sell equipment failed", playerId);
                return new BatchSellEquipmentResultDto { FailedCount = requestedIds.Count };
            }

            result.SoldCount = sellable.Count;
            result.GoldEarned = totalGold;
            await _gameSyncService.SyncPlayerAsync(playerId);
            _logger.LogInformation("Player {PlayerId} batch sold {Count} equipment(s), gained {Gold}", playerId, result.SoldCount, totalGold);
            return result;
        }

        /// <summary>
        /// 在单个事务内分解装备并按品质规则发放材料。
        /// </summary>
        public async Task<EquipmentDecomposeResultDto> DecomposeEquipmentsAsync(
            string playerId,
            DecomposeEquipmentRequestDto request)
        {
            var requestedIds = (request?.EquipmentIds ?? [])
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (requestedIds.Count == 0)
            {
                return new EquipmentDecomposeResultDto();
            }

            var equipments = await _equipmentRepository.GetListAsync(e =>
                e.PlayerId == playerId && requestedIds.Contains(e.InstanceId));
            var rules = await _dbContext.Db.Queryable<EquipmentDecomposeRuleEntity>()
                .Where(rule => rule.IsEnabled)
                .ToListAsync();
            var ruleByQuality = rules
                .GroupBy(rule => rule.Quality)
                .ToDictionary(group => group.Key, group => group.OrderBy(rule => rule.SortOrder).First());
            var templates = await _dbContext.Db.Queryable<ItemTemplateEntity>().ToListAsync();
            var templateById = templates.ToDictionary(item => item.ItemId, StringComparer.OrdinalIgnoreCase);

            var eligible = equipments
                .Where(e => !e.IsEquipped && !e.IsLocked && ruleByQuality.ContainsKey(e.Quality))
                .ToList();
            var result = new EquipmentDecomposeResultDto
            {
                FailedCount = requestedIds.Count - eligible.Count
            };
            if (eligible.Count == 0)
            {
                return result;
            }

            var rewards = new Dictionary<string, EquipmentDecomposeRewardDto>(StringComparer.OrdinalIgnoreCase);
            foreach (var equipment in eligible)
            {
                var rule = ruleByQuality[equipment.Quality];
                if (!templateById.TryGetValue(rule.MaterialItemId, out var materialTemplate))
                {
                    throw new InvalidOperationException($"分解规则的产出道具不存在：{rule.MaterialItemId}");
                }

                var quantity = rule.MinQuantity == rule.MaxQuantity
                    ? rule.MinQuantity
                    : Random.Shared.Next(rule.MinQuantity, checked(rule.MaxQuantity + 1));
                if (rewards.TryGetValue(rule.MaterialItemId, out var reward))
                {
                    reward.Quantity = checked(reward.Quantity + quantity);
                }
                else
                {
                    rewards[rule.MaterialItemId] = new EquipmentDecomposeRewardDto
                    {
                        ItemId = rule.MaterialItemId,
                        ItemName = materialTemplate.Name,
                        Quantity = quantity
                    };
                }
            }

            try
            {
                _dbContext.Db.Ado.BeginTran();
                var deletedRows = await _dbContext.Db.Deleteable<EquipmentInstanceEntity>()
                    .Where(e => e.PlayerId == playerId && requestedIds.Contains(e.InstanceId) && !e.IsEquipped && !e.IsLocked)
                    .ExecuteCommandAsync();
                if (deletedRows != eligible.Count)
                {
                    _dbContext.Db.Ado.RollbackTran();
                    return new EquipmentDecomposeResultDto { FailedCount = requestedIds.Count };
                }

                foreach (var reward in rewards.Values)
                {
                    await InventoryItemGrantHelper.AddOrMergeAsync(
                        _dbContext.Db,
                        playerId,
                        reward.ItemId,
                        reward.Quantity,
                        "背包道具格已满，无法领取分解材料。请先整理背包。");
                }

                _dbContext.Db.Ado.CommitTran();
            }
            catch (Exception ex)
            {
                _dbContext.Db.Ado.RollbackTran();
                _logger.LogError(ex, "Player {PlayerId} decompose equipment failed", playerId);
                throw;
            }

            result.DecomposedCount = eligible.Count;
            result.Rewards = rewards.Values.OrderBy(reward => reward.ItemId).ToList();
            await _gameSyncService.SyncPlayerAsync(playerId);
            _logger.LogInformation(
                "Player {PlayerId} decomposed {Count} equipment(s), rewards={RewardCount}",
                playerId,
                result.DecomposedCount,
                result.Rewards.Count);
            return result;
        }

        /// <summary>
        /// 按玩家自动出售设置清理不符合条件的未穿戴、未绑定装备。
        /// </summary>
        public async Task<EquipmentAutoSellResultDto> AutoSellUnqualifiedEquipmentAsync(
            string playerId,
            IEnumerable<string>? equipmentInstanceIds = null,
            DateTime? acquiredAfter = null,
            string reason = "自动出售装备",
            bool joinCurrentTransaction = false)
        {
            var ids = equipmentInstanceIds?
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // 没有明确范围时禁止扫描整个背包，避免调用方误卖历史装备。
            if ((ids == null || ids.Count == 0) && !acquiredAfter.HasValue)
            {
                return new EquipmentAutoSellResultDto();
            }

            var player = await _userRepository.GetByIdAsync(playerId);
            if (player == null || player.IsDeleted)
            {
                return new EquipmentAutoSellResultDto();
            }

            var minLevel = Math.Clamp(player.EquipmentAutoSellMinLevel, 0, 100);
            var minQuality = Math.Clamp(player.EquipmentAutoSellMinQuality, 0, 5);
            if (minLevel <= 0 && minQuality <= 0)
            {
                return new EquipmentAutoSellResultDto();
            }

            // 不要把 ids == null、ids.Count == 0、acquiredAfter.HasValue 这类本地条件
            // 混进 SqlSugar 的表达式。SQLite 下 SqlSugar 可能将其错误翻译成孤立的 IS，
            // 最终产生 "near IS: syntax error"。先构建基础查询，再按实际参数追加条件。
            var equipmentQuery = _equipmentRepository.Db.Queryable<EquipmentInstanceEntity>()
                .Where(e => e.PlayerId == playerId && !e.IsEquipped && !e.IsLocked);

            if (ids is { Count: > 0 })
            {
                equipmentQuery = equipmentQuery.Where(e => ids.Contains(e.InstanceId));
            }

            if (acquiredAfter.HasValue)
            {
                var acquiredAfterValue = acquiredAfter.Value;
                equipmentQuery = equipmentQuery.Where(e => e.AcquiredTime >= acquiredAfterValue);
            }

            var candidates = await equipmentQuery.ToListAsync();

            var toSell = candidates.Where(e =>
            {
                var requiredLevel = GetRequiredLevel(e);
                var levelPass = minLevel <= 0 || requiredLevel >= minLevel;
                var qualityPass = minQuality <= 0 || e.Quality >= minQuality;
                return !(levelPass && qualityPass);
            }).ToList();

            if (toSell.Count == 0)
            {
                return new EquipmentAutoSellResultDto();
            }

            var ownsTransaction = !joinCurrentTransaction;
            try
            {
                if (ownsTransaction)
                {
                    _dbContext.Db.Ado.BeginTran();
                }

                var sellIds = toSell.Select(e => e.InstanceId).ToList();
                var deleteRows = await _dbContext.Db.Deleteable<EquipmentInstanceEntity>()
                    .Where(e => e.PlayerId == playerId && sellIds.Contains(e.InstanceId) && !e.IsEquipped && !e.IsLocked)
                    .ExecuteCommandAsync();

                if (deleteRows > 0)
                {
                    var actualGold = deleteRows == toSell.Count
                        ? toSell.Sum(e => 100L * e.Quality * (e.EnhanceLevel + 1))
                        : toSell.Take(deleteRows).Sum(e => 100L * e.Quality * (e.EnhanceLevel + 1));
                    var updateRows = await _dbContext.Db.Updateable<UserEntity>()
                        .SetColumns(u => u.Gold == u.Gold + actualGold)
                        .SetColumns(u => u.TotalGoldEarned == u.TotalGoldEarned + actualGold)
                        .SetColumns(u => u.LastUpdateTime == DateTime.Now)
                        .Where(u => u.GID == playerId && !u.IsDeleted)
                        .ExecuteCommandAsync();

                    if (updateRows == 0)
                    {
                        if (ownsTransaction) _dbContext.Db.Ado.RollbackTran();
                        return new EquipmentAutoSellResultDto();
                    }

                    if (ownsTransaction)
                    {
                        _dbContext.Db.Ado.CommitTran();
                    }

                    if (ownsTransaction)
                    {
                        await _gameSyncService.SyncPlayerAsync(playerId);
                    }
                    _logger.LogInformation(
                        "Player {PlayerId} auto-sold {Count} equipment(s), gained {Gold}, reason={Reason}",
                        playerId, deleteRows, actualGold, reason);
                    return new EquipmentAutoSellResultDto { SoldCount = deleteRows, GoldGained = actualGold };
                }

                if (ownsTransaction) _dbContext.Db.Ado.CommitTran();
            }
            catch (Exception ex)
            {
                if (ownsTransaction) _dbContext.Db.Ado.RollbackTran();
                _logger.LogError(ex, "Player {PlayerId} auto-sell equipment failed, reason={Reason}", playerId, reason);
            }

            return new EquipmentAutoSellResultDto();
        }

        /// <summary>
        /// 获取当前可强化的装备列表。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <returns>可进入强化流程的装备集合。</returns>
        public async Task<List<EquipmentDto>> GetEnhanceableEquipmentsAsync(string playerId)
        {
            var equipments = await _equipmentRepository.GetListAsync(e => e.PlayerId == playerId && e.EnhanceLevel < 15);
            return equipments
                .OrderByDescending(e => e.IsEquipped)
                .ThenByDescending(e => e.Quality)
                .ThenByDescending(e => e.EnhanceLevel)
                .Select(MapToDto)
                .ToList();
        }

        /// <summary>
        /// 计算指定装备当前强化成功率。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="equipmentId">装备实例编号。</param>
        /// <returns>强化成功率百分比。</returns>
        public async Task<int> CalculateEnhanceSuccessRateAsync(string playerId, string equipmentId)
        {
            var equipment = await _equipmentRepository.GetFirstAsync(e => e.PlayerId == playerId && e.InstanceId == equipmentId);
            if (equipment == null)
            {
                return 0;
            }

            try
            {
                return EquipmentRerollRuleRuntimeCatalog.GetEnhanceRule(GetRequiredLevel(equipment)).SuccessRate;
            }
            catch (InvalidOperationException)
            {
                return 0;
            }
        }

        /// <summary>
        /// 绑定装备。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="request">绑定请求。</param>
        /// <returns>绑定结果。</returns>
        public async Task<BindEquipmentResultDto> BindEquipmentAsync(string playerId, BindEquipmentRequestDto request)
        {
            var equipment = await _equipmentRepository.GetFirstAsync(e => e.PlayerId == playerId && e.InstanceId == request.EquipmentId);
            if (equipment == null)
            {
                _logger.LogWarning("Player {PlayerId} tried to bind missing equipment {EquipmentId}", playerId, request.EquipmentId);
                return new BindEquipmentResultDto
                {
                    Success = false,
                    Message = "装备不存在"
                };
            }

            if (equipment.IsLocked)
            {
                _logger.LogWarning("Player {PlayerId} tried to bind already bound equipment {EquipmentId}", playerId, request.EquipmentId);
                return new BindEquipmentResultDto
                {
                    Success = true,
                    Message = "该装备已经锁定"
                };
            }

            equipment.IsLocked = true;
            equipment.LastUpdateTime = DateTime.Now;
            await _equipmentRepository.UpdateAsync(equipment);
            _logger.LogInformation("Player {PlayerId} bound equipment {EquipmentId}", playerId, request.EquipmentId);

            return new BindEquipmentResultDto
            {
                Success = true,
                Message = "锁定成功"
            };
        }

        public async Task<BindEquipmentResultDto> UnlockEquipmentAsync(string playerId, BindEquipmentRequestDto request)
        {
            var equipment = await _equipmentRepository.GetFirstAsync(e => e.PlayerId == playerId && e.InstanceId == request.EquipmentId);
            if (equipment == null)
            {
                _logger.LogWarning("Player {PlayerId} tried to unlock missing equipment {EquipmentId}", playerId, request.EquipmentId);
                return new BindEquipmentResultDto
                {
                    Success = false,
                    Message = "装备不存在"
                };
            }

            if (!equipment.IsLocked)
            {
                _logger.LogWarning("Player {PlayerId} tried to unlock already unlocked equipment {EquipmentId}", playerId, request.EquipmentId);
                return new BindEquipmentResultDto
                {
                    Success = true,
                    Message = "该装备未锁定"
                };
            }

            equipment.IsLocked = false;
            equipment.LastUpdateTime = DateTime.Now;
            await _equipmentRepository.UpdateAsync(equipment);
            _logger.LogInformation("Player {PlayerId} unlocked equipment {EquipmentId}", playerId, request.EquipmentId);

            return new BindEquipmentResultDto
            {
                Success = true,
                Message = "解锁成功"
            };
        }

        /// <summary>
        /// 向宗门捐献装备。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="equipmentId">装备实例编号。</param>
        /// <returns>捐献结果与获得的贡献值。</returns>
        public async Task<DonateEquipmentResultDto> DonateEquipmentAsync(string playerId, string equipmentId)
        {
            var equipment = await _equipmentRepository.GetFirstAsync(e => e.PlayerId == playerId && e.InstanceId == equipmentId);
            if (equipment == null)
            {
                _logger.LogWarning("Player {PlayerId} tried to donate missing equipment {EquipmentId}", playerId, equipmentId);
                return new DonateEquipmentResultDto
                {
                    Success = false,
                    Message = "装备不存在"
                };
            }

            if (equipment.IsEquipped)
            {
                _logger.LogWarning("Player {PlayerId} tried to donate equipped equipment {EquipmentId}", playerId, equipmentId);
                return new DonateEquipmentResultDto
                {
                    Success = false,
                    Message = "请先卸下装备后再捐献"
                };
            }

            var gainedContribution = Math.Max(5, equipment.Quality * 5 + equipment.EnhanceLevel * 2);
            var now = DateTime.Now;

            try
            {
                _dbContext.Db.Ado.BeginTran();

                var deleteRows = await _dbContext.Db.Deleteable<EquipmentInstanceEntity>()
                    .Where(e => e.PlayerId == playerId && e.InstanceId == equipmentId && !e.IsEquipped)
                    .ExecuteCommandAsync();

                if (deleteRows == 0)
                {
                    _dbContext.Db.Ado.RollbackTran();
                    _logger.LogWarning("Player {PlayerId} donate equipment {EquipmentId} failed because state changed", playerId, equipmentId);
                    return new DonateEquipmentResultDto
                    {
                        Success = false,
                        Message = "装备状态已变化，请刷新后重试"
                    };
                }

                var updateRows = await _dbContext.Db.Updateable<UserEntity>()
                    .SetColumns(u => u.GuildContribution == u.GuildContribution + gainedContribution)
                    .SetColumns(u => u.LastUpdateTime == now)
                    .Where(u => u.GID == playerId && !u.IsDeleted)
                    .ExecuteCommandAsync();

                if (updateRows == 0)
                {
                    _dbContext.Db.Ado.RollbackTran();
                    _logger.LogWarning("Player {PlayerId} donate equipment {EquipmentId} failed because player record missing", playerId, equipmentId);
                    return new DonateEquipmentResultDto
                    {
                        Success = false,
                        Message = "玩家不存在"
                    };
                }

                _dbContext.Db.Ado.CommitTran();
            }
            catch (Exception ex)
            {
                _dbContext.Db.Ado.RollbackTran();
                _logger.LogError(ex, "Player {PlayerId} donate equipment {EquipmentId} exception", playerId, equipmentId);
                return new DonateEquipmentResultDto
                {
                    Success = false,
                    Message = "捐献失败，请稍后重试"
                };
            }

            await _gameSyncService.SyncPlayerAsync(playerId);
            _logger.LogInformation(
                "Player {PlayerId} donated equipment {EquipmentId} and gained contribution {Contribution}",
                playerId,
                equipmentId,
                gainedContribution);
            return new DonateEquipmentResultDto
            {
                Success = true,
                Message = $"捐献成功，获得 {gainedContribution} 贡献",
                ContributionGained = gainedContribution
            };
        }

        public async Task<EquipmentRerollPreviewDto?> GetEquipmentRerollPreviewAsync(string playerId, string equipmentId)
        {
            var equipment = await _equipmentRepository.GetFirstAsync(e => e.PlayerId == playerId && e.InstanceId == equipmentId);
            if (equipment == null) return null;

            var systemConfig = EquipmentRerollRuleRuntimeCatalog.GetSystemConfig();
            var equipmentLevel = GetRequiredLevel(equipment);
            var costRule = EquipmentRerollRuleRuntimeCatalog.GetRerollCostRule(equipmentLevel);
            var stoneCost = EquipmentRerollRuleRuntimeCatalog.CalculateStoneCost(costRule, 0);
            var goldCost = EquipmentRerollRuleRuntimeCatalog.CalculateGoldCost(costRule, 0);
            var materialName = GameData.Items.TryGetValue(costRule.MaterialItemId, out var materialTemplate)
                ? materialTemplate.Name
                : costRule.MaterialItemId;

            return new EquipmentRerollPreviewDto
            {
                EquipmentId = equipment.InstanceId,
                CurrentStats = DeserializeBonusStats(equipment.RerollStatsJson),
                CandidateStats = DeserializeBonusStats(equipment.RerollCandidateJson),
                Cost = new EquipmentRerollCostDto
                {
                    Gold = goldCost,
                    StoneCount = stoneCost,
                    LockedCount = 0,
                    MaterialItemId = costRule.MaterialItemId,
                    MaterialName = materialName
                },
                MaxLockedLineCount = systemConfig.MaxLockedLineCount,
                RerollStoneItemId = costRule.MaterialItemId,
                RerollCount = equipment.RerollCount,
                IsEnabled = systemConfig.IsEnabled
            };
        }

        public async Task<RerollEquipmentResultDto> RollEquipmentRerollAsync(string playerId, RerollEquipmentRollRequestDto request)
        {
            var equipment = await _equipmentRepository.GetFirstAsync(e => e.PlayerId == playerId && e.InstanceId == request.EquipmentId);
            if (equipment == null)
            {
                return new RerollEquipmentResultDto { Success = false, Message = "装备不存在" };
            }

            var systemConfig = EquipmentRerollRuleRuntimeCatalog.GetSystemConfig();
            if (!systemConfig.IsEnabled)
            {
                return new RerollEquipmentResultDto { Success = false, Message = "洗练功能暂未开放" };
            }

            // 校验锁定索引
            var currentStats = DeserializeBonusStats(equipment.RerollStatsJson);
            var lockedIndices = request.LockedIndices ?? [];
            if (lockedIndices.Count > 0 && currentStats.Count == 0)
            {
                return new RerollEquipmentResultDto { Success = false, Message = "没有当前洗练词条，无法锁定" };
            }

            if (lockedIndices.Any(i => i < 0 || i >= currentStats.Count))
            {
                return new RerollEquipmentResultDto { Success = false, Message = "锁定词条不存在" };
            }

            if (lockedIndices.Count > systemConfig.MaxLockedLineCount)
            {
                return new RerollEquipmentResultDto { Success = false, Message = "锁定词条数量超过上限" };
            }

            // 计算消耗
            var equipmentLevel = GetRequiredLevel(equipment);
            var costRule = EquipmentRerollRuleRuntimeCatalog.GetRerollCostRule(equipmentLevel);
            var stoneCost = EquipmentRerollRuleRuntimeCatalog.CalculateStoneCost(costRule, lockedIndices.Count);
            var goldCost = EquipmentRerollRuleRuntimeCatalog.CalculateGoldCost(costRule, lockedIndices.Count);
            var now = DateTime.Now;

            try
            {
                _dbContext.Db.Ado.BeginTran();

                // 扣金币
                var goldRows = await _dbContext.Db.Updateable<UserEntity>()
                    .SetColumns(u => u.Gold == u.Gold - goldCost)
                    .SetColumns(u => u.TotalGoldSpent == u.TotalGoldSpent + goldCost)
                    .SetColumns(u => u.LastUpdateTime == now)
                    .Where(u => u.GID == playerId && !u.IsDeleted && u.Gold >= goldCost)
                    .ExecuteCommandAsync();

                if (goldRows == 0)
                {
                    _dbContext.Db.Ado.RollbackTran();
                    return new RerollEquipmentResultDto { Success = false, Message = "金币不足" };
                }

                // 扣洗练石
                var stoneItemId = costRule.MaterialItemId;
                var stoneDeducted = await DeductMaterialInTransactionAsync(playerId, stoneItemId, stoneCost);
                if (!stoneDeducted)
                {
                    _dbContext.Db.Ado.RollbackTran();
                    var materialName = GameData.Items.TryGetValue(stoneItemId, out var materialTemplate)
                        ? materialTemplate.Name
                        : stoneItemId;
                    return new RerollEquipmentResultDto { Success = false, Message = $"{materialName}不足" };
                }

                // 生成候选词条
                var slotPool = EquipmentRerollRuleRuntimeCatalog.GetSlotPoolConfigs(equipment.Slot);
                var tierConfigs = EquipmentRerollRuleRuntimeCatalog.GetTierConfigs();

                if (slotPool.Count == 0)
                {
                    _dbContext.Db.Ado.RollbackTran();
                    return new RerollEquipmentResultDto { Success = false, Message = "当前装备暂无可用洗练词条" };
                }

                var targetCount = Math.Clamp(equipment.Quality, 1, 5);
                var candidateStats = GenerateCandidateStats(slotPool, tierConfigs, targetCount, lockedIndices, currentStats, equipment);

                var candidateJson = JsonSerializer.Serialize(candidateStats);

                var updateRows = await _dbContext.Db.Updateable<EquipmentInstanceEntity>()
                    .SetColumns(e => e.RerollCandidateJson == candidateJson)
                    .SetColumns(e => e.RerollCount == e.RerollCount + 1)
                    .SetColumns(e => e.RerollCandidateCreatedAt == now)
                    .SetColumns(e => e.LastUpdateTime == now)
                    .Where(e => e.PlayerId == playerId && e.InstanceId == request.EquipmentId)
                    .ExecuteCommandAsync();

                if (updateRows == 0)
                {
                    _dbContext.Db.Ado.RollbackTran();
                    return new RerollEquipmentResultDto { Success = false, Message = "装备状态已变化，请刷新后重试" };
                }

                equipment.RerollCandidateJson = candidateJson;
                equipment.RerollCount += 1;
                equipment.LastUpdateTime = now;

                _dbContext.Db.Ado.CommitTran();
            }
            catch (Exception ex)
            {
                _dbContext.Db.Ado.RollbackTran();
                _logger.LogError(ex, "Player {PlayerId} roll equipment reroll {EquipmentId} exception", playerId, request.EquipmentId);
                return new RerollEquipmentResultDto { Success = false, Message = "洗练失败，请稍后重试" };
            }

            await _gameSyncService.SyncPlayerAsync(playerId);

            return new RerollEquipmentResultDto
            {
                Success = true,
                Message = "洗练完成，请查看候选结果",
                CostGold = goldCost,
                CostMaterialCount = stoneCost,
                Equipment = MapToDto(equipment)
            };
        }

        public async Task<RerollEquipmentResultDto> AcceptEquipmentRerollAsync(string playerId, RerollEquipmentAcceptRequestDto request)
        {
            var equipment = await _equipmentRepository.GetFirstAsync(e => e.PlayerId == playerId && e.InstanceId == request.EquipmentId);
            if (equipment == null)
            {
                return new RerollEquipmentResultDto { Success = false, Message = "装备不存在" };
            }

            if (string.IsNullOrWhiteSpace(equipment.RerollCandidateJson))
            {
                return new RerollEquipmentResultDto { Success = false, Message = "没有候选结果可接受" };
            }

            var now = DateTime.Now;
            var updateRows = await _dbContext.Db.Updateable<EquipmentInstanceEntity>()
                .SetColumns(e => e.RerollStatsJson == equipment.RerollCandidateJson)
                .SetColumns(e => e.RerollCandidateJson == (string?)null)
                .SetColumns(e => e.RerollCandidateCreatedAt == (DateTime?)null)
                .SetColumns(e => e.LastUpdateTime == now)
                .Where(e => e.PlayerId == playerId && e.InstanceId == request.EquipmentId)
                .ExecuteCommandAsync();

            if (updateRows == 0)
            {
                return new RerollEquipmentResultDto { Success = false, Message = "装备状态已变化，请刷新后重试" };
            }

            if (equipment.IsEquipped)
            {
                await _playerAttributeService.RecalculatePlayerAttributesAsync(playerId);
            }
            else
            {
                await _gameSyncService.SyncPlayerAsync(playerId);
            }

            return new RerollEquipmentResultDto { Success = true, Message = "已接受洗练结果" };
        }

        public async Task<RerollEquipmentResultDto> DiscardEquipmentRerollAsync(string playerId, RerollEquipmentDiscardRequestDto request)
        {
            var equipment = await _equipmentRepository.GetFirstAsync(e => e.PlayerId == playerId && e.InstanceId == request.EquipmentId);
            if (equipment == null)
            {
                return new RerollEquipmentResultDto { Success = false, Message = "装备不存在" };
            }

            if (string.IsNullOrWhiteSpace(equipment.RerollCandidateJson))
            {
                return new RerollEquipmentResultDto { Success = false, Message = "没有候选结果可丢弃" };
            }

            var now = DateTime.Now;
            await _dbContext.Db.Updateable<EquipmentInstanceEntity>()
                .SetColumns(e => e.RerollCandidateJson == (string?)null)
                .SetColumns(e => e.RerollCandidateCreatedAt == (DateTime?)null)
                .SetColumns(e => e.LastUpdateTime == now)
                .Where(e => e.PlayerId == playerId && e.InstanceId == request.EquipmentId)
                .ExecuteCommandAsync();

            await _gameSyncService.SyncPlayerAsync(playerId);

            return new RerollEquipmentResultDto { Success = true, Message = "已丢弃候选结果" };
        }

        private List<EquipmentBonusDto> GenerateCandidateStats(
            List<EquipmentRerollSlotPoolConfigEntity> slotPool,
            List<EquipmentRerollTierConfigEntity> tierConfigs,
            int targetCount,
            List<int> lockedIndices,
            List<EquipmentBonusDto> currentStats,
            EquipmentInstanceEntity equipment)
        {
            var random = Random.Shared;
            var result = new List<EquipmentBonusDto>();
            var usedTypes = new HashSet<int>();

            // 先放锁定的词条
            foreach (var idx in lockedIndices)
            {
                if (idx >= 0 && idx < currentStats.Count)
                {
                    result.Add(currentStats[idx]);
                    usedTypes.Add(int.Parse(currentStats[idx].StatType.Replace("Type", "")));
                }
            }

            // 根据装备等级和品质限制可用品阶范围
            var equipLevel = GetRequiredLevel(equipment);
            var maxTier = Math.Clamp(1 + (equipLevel / 10) + equipment.Quality, 1, tierConfigs.Count > 0 ? tierConfigs.Max(t => t.Tier) : 5);
            var availableTiers = tierConfigs.Where(t => t.Tier <= maxTier).ToList();
            if (availableTiers.Count == 0) availableTiers = tierConfigs.Take(1).ToList();
            var tierTotalWeight = availableTiers.Sum(t => t.Weight);

            // 填充未锁定位置
            var maxAttempts = 50;
            while (result.Count < targetCount && maxAttempts-- > 0)
            {
                // 第一步：按品阶权重随机选品阶
                var tierRoll = random.Next(Math.Max(1, tierTotalWeight));
                var tierCumulative = 0;
                var selectedTier = availableTiers[0];
                foreach (var tier in availableTiers)
                {
                    tierCumulative += tier.Weight;
                    if (tierRoll < tierCumulative)
                    {
                        selectedTier = tier;
                        break;
                    }
                }

                // 第二步：从该品阶的词条池中随机选属性类型（等权重）
                var tierPool = slotPool.Where(p => p.Tier == selectedTier.Tier && !usedTypes.Contains(p.AttributeType)).ToList();
                if (tierPool.Count == 0)
                {
                    // 该品阶无可用属性，放宽到不限品阶
                    tierPool = slotPool.Where(p => !usedTypes.Contains(p.AttributeType)).ToList();
                }
                if (tierPool.Count == 0)
                {
                    // 池不足时允许重复
                    tierPool = slotPool.Where(p => p.Tier == selectedTier.Tier).ToList();
                    if (tierPool.Count == 0) tierPool = slotPool;
                }
                if (tierPool.Count == 0) break;

                var selected = tierPool[random.Next(tierPool.Count)];

                // 第三步：查属性值配置
                var attrValueConfig = EquipmentRerollRuleRuntimeCatalog.GetAttributeValueConfig(selected.AttributeType, selectedTier.Tier);
                if (attrValueConfig == null) break;

                var isPct = attrValueConfig.IsPercentage;
                double minVal = double.TryParse(attrValueConfig.MinValue, out var mn) ? mn : 0;
                double maxVal = double.TryParse(attrValueConfig.MaxValue, out var mx) ? mx : 0;
                var baseValue = minVal + random.NextDouble() * (maxVal - minVal);

                double finalValue;
                if (isPct)
                {
                    finalValue = baseValue;
                }
                else
                {
                    var levelCoeff = 1 + Math.Max(0, GetRequiredLevel(equipment) - 1) * 0.04;
                    var qualityCoeff = 1 + Math.Max(0, equipment.Quality - 1) * 0.12;
                    finalValue = Math.Round(baseValue * levelCoeff * qualityCoeff);
                }

                var displayValue = isPct
                    ? (int)Math.Round(finalValue * 100, MidpointRounding.AwayFromZero)
                    : (int)Math.Round(finalValue, MidpointRounding.AwayFromZero);

                // 数据库中属性类型从1开始，枚举从0开始，需要减1
                var attrType = (AttributeType)(selected.AttributeType - 1);
                result.Add(new EquipmentBonusDto
                {
                    StatType = attrType.ToString(),
                    Value = displayValue,
                    RawValue = finalValue,
                    IsPercentage = isPct,
                    Description = RerollSystem.GetAttributeName(attrType),
                    Index = result.Count,
                    Tier = selectedTier.Tier,
                    TierName = selectedTier.Name,
                    TierColor = selectedTier.Color
                });

                usedTypes.Add(selected.AttributeType);
            }

            return result;
        }

        private async Task<bool> DeductMaterialInTransactionAsync(string playerId, string materialItemId, int requiredCount)
        {
            var materials = await _dbContext.Db.Queryable<InventoryItemEntity>()
                .Where(i => i.PlayerId == playerId && i.ItemId == materialItemId)
                .OrderBy(i => i.IsLocked)
                .OrderBy(i => i.Id)
                .ToListAsync();

            var totalCount = materials.Sum(i => i.Quantity);
            if (totalCount < requiredCount)
            {
                return false;
            }

            var remaining = requiredCount;
            foreach (var material in materials)
            {
                if (remaining <= 0) break;

                var deductCount = Math.Min(material.Quantity, remaining);
                if (deductCount == material.Quantity)
                {
                    var deleteRows = await _dbContext.Db.Deleteable<InventoryItemEntity>()
                        .Where(i => i.Id == material.Id && i.PlayerId == playerId && i.Quantity == material.Quantity)
                        .ExecuteCommandAsync();
                    if (deleteRows == 0) return false;
                }
                else
                {
                    var updateRows = await _dbContext.Db.Updateable<InventoryItemEntity>()
                        .SetColumns(i => i.Quantity == i.Quantity - deductCount)
                        .Where(i => i.Id == material.Id && i.PlayerId == playerId && i.Quantity >= deductCount)
                        .ExecuteCommandAsync();
                    if (updateRows == 0) return false;
                }

                remaining -= deductCount;
            }

            return true;
        }

        private static int ApplyEnhanceGrowth(int currentValue, int growthPercent)
        {
            if (currentValue <= 0)
            {
                return 0;
            }

            return (int)Math.Ceiling(currentValue * (100d + Math.Max(0, growthPercent)) / 100d);
        }

        private EquipmentDto MapToDto(EquipmentInstanceEntity entity)
        {
            var description = string.Empty;
            var template = EquipmentBalanceHelper.ResolveTemplate(entity.TemplateId);
            if (template != null)
            {
                description = template.Description;
            }

            // 预加载宝石模板字典，用于解析宝石名称和属性中文名
            Dictionary<string, GemTemplateEntity> gemTemplateMap;
            try
            {
                gemTemplateMap = _dbContext.Db.Queryable<GemTemplateEntity>()
                    .ToList()
                    .ToDictionary(g => g.GemId);
            }
            catch { gemTemplateMap = []; }

            return new EquipmentDto
            {
                InstanceId = entity.InstanceId,
                TemplateId = entity.TemplateId,
                Name = entity.Name,
                Description = description,
                Slot = entity.Slot,
                Level = template?.Level ?? 0,
                Quality = entity.Quality,
                EnhanceLevel = entity.EnhanceLevel,
                IsEquipped = entity.IsEquipped,
                IsLocked = entity.IsLocked,
                BasePhysicalAttack = EquipmentBalanceHelper.GetPhysicalAttack(entity, template),
                BaseMagicAttack = EquipmentBalanceHelper.GetMagicAttack(entity, template),
                BasePhysicalDefense = EquipmentBalanceHelper.GetPhysicalDefense(entity, template),
                BaseMagicDefense = EquipmentBalanceHelper.GetMagicDefense(entity, template),
                BaseHP = entity.BaseHP,
                BaseMP = entity.BaseMP,
                CombatStyle = template?.CombatStyle ?? EquipmentBalanceHelper.ResolveCombatStyle(entity),
                WeaponCategory = template?.WeaponCategory ?? WeaponCategory.None,
                BonusStats = DeserializeBonusStats(entity.BonusStatsJson),
                RerollStats = DeserializeBonusStats(entity.RerollStatsJson),
                RerollCandidate = DeserializeBonusStats(entity.RerollCandidateJson),
                RerollCount = entity.RerollCount,
                Icon = string.IsNullOrWhiteSpace(template?.IconPath)
                    ? $"equipment-{entity.Slot.ToString().ToLowerInvariant()}"
                    : template.IconPath,
                AcquiredTime = entity.AcquiredTime,
                GemSlots = MapGemSlots(entity.GemSlotsJson, gemTemplateMap),
                MaxGemSlots = GemService.GetMaxSlots(entity.Quality)
            };
        }

        private static List<EquipmentGemSlotDto> MapGemSlots(string? json, Dictionary<string, GemTemplateEntity> gemTemplateMap)
        {
            if (string.IsNullOrWhiteSpace(json)) return [];
            try
            {
                var entries = JsonSerializer.Deserialize<List<GemSlotEntry>>(json) ?? [];
                return entries.Select(e =>
                {
                    string? gemName = null;
                    string? bonusText = null;

                    if (e.GemId != null && gemTemplateMap.TryGetValue(e.GemId, out var gemTpl))
                    {
                        gemName = gemTpl.Name;
                        var attrName = GetAttrChineseName(gemTpl.AttributeType);
                        bonusText = $"{attrName} +{gemTpl.BonusValue}{(gemTpl.BonusMode == "Percent" ? "%" : "")}";
                    }
                    else if (e.GemId != null)
                    {
                        gemName = e.GemId;
                        var attrName = GetAttrChineseName(e.AttributeType ?? "");
                        bonusText = $"{attrName} +{e.BonusValue}{(e.BonusMode == "Percent" ? "%" : "")}";
                    }

                    return new EquipmentGemSlotDto
                    {
                        SlotIndex = e.SlotIndex,
                        GemId = e.GemId,
                        GemName = gemName,
                        BonusText = bonusText
                    };
                }).ToList();
            }
            catch { return []; }
        }

        private static string GetAttrChineseName(string attrType) => attrType switch
        {
            "Type1" => "生命", "Type2" => "法力", "Type3" => "物攻", "Type4" => "法攻",
            "Type5" => "物防", "Type6" => "法防", "Type7" => "速度", "Type8" => "命中",
            "Type9" => "闪避", "Type10" => "暴击", "Type11" => "暴伤", "Type12" => "连击",
            "Type13" => "反击", "Type14" => "破甲", "Type15" => "增伤",
            _ => attrType
        };

        /// <summary>
        /// 解析装备需求等级。
        /// </summary>
        /// <remarks>
        /// 优先读装备模板里的真实等级；如果模板缺失，则回退到旧逻辑使用品质估算，
        /// 这样既兼容新数据，也能容忍开发期残留的旧装备记录。
        /// </remarks>
        private static int GetRequiredLevel(EquipmentInstanceEntity equipment)
        {
            if (int.TryParse(equipment.TemplateId, out var templateId) &&
                GameData.EquipmentTemplates.TryGetValue(templateId, out var template))
            {
                return template.Level;
            }

            return Math.Max(1, equipment.Quality * 5);
        }

        private static List<EquipmentBonusDto> DeserializeBonusStats(string? bonusStatsJson)
        {
            if (string.IsNullOrWhiteSpace(bonusStatsJson))
            {
                return [];
            }

            try
            {
                return JsonSerializer.Deserialize<List<EquipmentBonusDto>>(bonusStatsJson) ?? [];
            }
            catch
            {
                return [];
            }
        }

        private static List<EquipmentBonusDto> ConvertAttributePropertiesToBonusStats(List<AttributeProperty> attributes)
        {
            return attributes.Select(attribute =>
            {
                var isPercentage = IsPercentageAttribute(attribute.type);
                var rawValue = attribute.value;
                var displayValue = isPercentage
                    ? (int)Math.Round(rawValue * 100, MidpointRounding.AwayFromZero)
                    : (int)Math.Round(rawValue, MidpointRounding.AwayFromZero);

                return new EquipmentBonusDto
                {
                    StatType = attribute.type.ToString(),
                    Value = displayValue,
                    RawValue = rawValue,
                    IsPercentage = isPercentage,
                    Description = RerollSystem.GetAttributeName(attribute.type)
                };
            }).ToList();
        }

        private static bool IsPercentageAttribute(AttributeType attributeType)
        {
            return attributeType is AttributeType.Type8
                or AttributeType.Type9
                or AttributeType.Type10
                or AttributeType.Type11
                or AttributeType.Type12
                or AttributeType.Type13
                or AttributeType.Type14
                or AttributeType.Type15;
        }
    }
}
