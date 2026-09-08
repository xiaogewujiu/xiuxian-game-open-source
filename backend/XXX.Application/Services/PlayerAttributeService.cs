using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;
using XXX.Achievement;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Data;
using XXX.Infrastructure.Repositories;
using XXX.Player;
using XXX.Quest;

namespace XXX.Application.Services
{
    /// <summary>
    /// 玩家属性服务实现。
    /// </summary>
    public class PlayerAttributeService : IPlayerAttributeService
    {
        private readonly DbContext _dbContext;
        private readonly IRepository<UserEntity> _userRepository;
        private readonly IRepository<EquipmentInstanceEntity> _equipmentRepository;
        private readonly IRepository<PetInstanceEntity> _petRepository;
        private readonly IRepository<HeartSutraTemplateEntity> _heartSutraRepository;
        private readonly IRepository<FiveElementArrayEntity> _fiveElementRepository;
        private readonly IRepository<PlayerHeartSutraEntity> _playerSutraRepository;
        private readonly IGameSyncService _gameSyncService;
        private readonly ICollectionService _collectionService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PlayerAttributeService> _logger;

        /// <summary>
        /// 初始化玩家属性服务。
        /// </summary>
        /// <param name="dbContext">数据库上下文。</param>
        /// <param name="userRepository">玩家仓储。</param>
        /// <param name="equipmentRepository">装备仓储。</param>
        /// <param name="petRepository">灵宠仓储。</param>
        /// <param name="gameSyncService">游戏同步服务。</param>
        /// <param name="serviceProvider">服务提供器。</param>
        /// <param name="logger">日志记录器。</param>
        public PlayerAttributeService(
            DbContext dbContext,
            IRepository<UserEntity> userRepository,
            IRepository<EquipmentInstanceEntity> equipmentRepository,
            IRepository<PetInstanceEntity> petRepository,
            IRepository<HeartSutraTemplateEntity> heartSutraRepository,
            IRepository<FiveElementArrayEntity> fiveElementRepository,
            IRepository<PlayerHeartSutraEntity> playerSutraRepository,
            IGameSyncService gameSyncService,
            ICollectionService collectionService,
            IServiceProvider serviceProvider,
            ILogger<PlayerAttributeService> logger)
        {
            _dbContext = dbContext;
            _userRepository = userRepository;
            _equipmentRepository = equipmentRepository;
            _petRepository = petRepository;
            _heartSutraRepository = heartSutraRepository;
            _fiveElementRepository = fiveElementRepository;
            _playerSutraRepository = playerSutraRepository;
            _gameSyncService = gameSyncService;
            _collectionService = collectionService;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// 重新计算玩家总属性。
        /// </summary>
        public async Task RecalculatePlayerAttributesAsync(string playerId, bool syncLevelDrivenProgress = false)
        {
            var stopwatch = Stopwatch.StartNew();
            var player = await _userRepository.GetByIdAsync(playerId);
            if (player == null)
            {
                _logger.LogInformation(
                    "RecalculatePlayerAttributesAsync skipped because player not found. PlayerId={PlayerId}, TotalMs={TotalMs}",
                    playerId,
                    stopwatch.ElapsedMilliseconds);
                return;
            }

            var loadPlayerElapsedMs = stopwatch.ElapsedMilliseconds;

            // 中文注释：
            // 属性计算 + 玩家字段保存统一收敛到 ApplyCombatAttributesCoreAsync，
            // 与 RecalculateCombatAttributesAsync 共用同一套计算逻辑，避免两处漂移。
            var timings = await ApplyCombatAttributesCoreAsync(player);

            await _gameSyncService.SyncPlayerAsync(playerId);
            var syncPlayerElapsedMs = stopwatch.ElapsedMilliseconds;
            if (syncLevelDrivenProgress)
            {
                await SyncLevelDrivenProgressAsync(player);
            }
            var finalElapsedMs = stopwatch.ElapsedMilliseconds;

            _logger.LogInformation(
                "RecalculatePlayerAttributesAsync timing. PlayerId={PlayerId}, SyncLevelDrivenProgress={SyncLevelDrivenProgress}, LoadPlayerMs={LoadPlayerMs}, NormalizeMs={NormalizeMs}, BaseApplyMs={BaseApplyMs}, EquippedItemsMs={EquippedItemsMs}, SutraMs={SutraMs}, PetMs={PetMs}, UpdatePlayerMs={UpdatePlayerMs}, SyncPlayerMs={SyncPlayerMs}, LevelProgressMs={LevelProgressMs}, TotalMs={TotalMs}",
                playerId,
                syncLevelDrivenProgress,
                loadPlayerElapsedMs,
                timings.NormalizeMs,
                timings.BaseApplyMs,
                timings.EquippedItemsMs,
                timings.SutraMs,
                timings.PetMs,
                timings.UpdatePlayerMs,
                syncPlayerElapsedMs - loadPlayerElapsedMs,
                finalElapsedMs - syncPlayerElapsedMs,
                finalElapsedMs);
        }

        /// <summary>
        /// 仅重新计算玩家战斗属性并保存，不触发排行榜与等级进度同步。
        /// </summary>
        public async Task RecalculateCombatAttributesAsync(string playerId)
        {
            var stopwatch = Stopwatch.StartNew();
            var player = await _userRepository.GetByIdAsync(playerId);
            if (player == null)
            {
                _logger.LogInformation(
                    "RecalculateCombatAttributesAsync skipped because player not found. PlayerId={PlayerId}, TotalMs={TotalMs}",
                    playerId,
                    stopwatch.ElapsedMilliseconds);
                return;
            }

            await ApplyCombatAttributesCoreAsync(player);

            _logger.LogInformation(
                "RecalculateCombatAttributesAsync timing. PlayerId={PlayerId}, TotalMs={TotalMs}",
                playerId,
                stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// 仅同步玩家排行榜分数（等级/战力/财富三榜）。
        /// </summary>

        /// <summary>
        /// 战斗属性计算 + 玩家字段保存（不包含排行榜与等级进度同步）。
        /// </summary>
        /// <remarks>
        /// 中文注释：
        /// 供 RecalculatePlayerAttributesAsync 与 RecalculateCombatAttributesAsync 共用。
        /// 流程：基础属性 -> 属性点 -> 丹药 -> 突破 -> 已装备 -> 保存玩家。
        /// </remarks>
        private async Task<RecalcStageTimings> ApplyCombatAttributesCoreAsync(UserEntity player)
        {
            var stopwatch = Stopwatch.StartNew();
            var allocations = await _dbContext.Db.Queryable<PlayerAttributeAllocationEntity>()
                .Where(item => item.PlayerId == player.GID)
                .ToListAsync();
            var normalizeElapsedMs = stopwatch.ElapsedMilliseconds;

            // 中文注释：
            // 这里不能再调用“创建新玩家”来重置底板，
            // 因为那样会把所有角色都按 1 级重新初始化，导致高等级角色一旦换装或洗练就属性回退。
            // 现在改为按“当前真实等级”重建基础属性，再叠加属性点、装备和宠物。
            PlayerManager.ApplyBaseAttributes(player, player.Level);
            var fiveElementArray = await _fiveElementRepository.GetFirstAsync(item => item.PlayerId == player.GID);
            if (fiveElementArray != null)
            {
                ApplyFiveElementBonuses(player, fiveElementArray);
            }
            _logger.LogInformation(
                "Recalc Step1 基础属性: PlayerId={PlayerId}, Type3={Type3}",
                player.GID, player.Type3);

            foreach (var attribute in player.Attributes.Where(item => !AttributePointConfig.IsAttributePointType(item.type)))
            {
                ApplyAttribute(player, attribute.type.ToString(), attribute.value);
            }

            ApplyAttributePointAllocations(player, allocations);
            _logger.LogInformation(
                "Recalc Step2 属性点分配后: PlayerId={PlayerId}, Type3={Type3}",
                player.GID, player.Type3);

            // 丹药属性加成：在属性点分配之后叠加，避免被覆盖
            if (player.PillBonusAttributes is { Count: > 0 } pillBonuses)
            {
                _logger.LogInformation(
                    "丹药属性加成: PlayerId={PlayerId}, Count={Count}, Json={Json}",
                    player.GID, pillBonuses.Count, player.PillBonusAttributesJson);
                foreach (var bonus in pillBonuses)
                {
                    _logger.LogInformation(
                        "  应用丹药加成: Type={Type}, Value={Value}",
                        bonus.type, bonus.value);
                    ApplyAttribute(player, bonus.type.ToString(), bonus.value);
                }
            }
            else
            {
                _logger.LogInformation(
                    "丹药属性加成: PlayerId={PlayerId}, 无加成数据, Json={Json}",
                    player.GID, player.PillBonusAttributesJson);
            }

            ApplyBreakthroughBonus(player);
            _logger.LogInformation(
                "Recalc Step3 丹药加成后: PlayerId={PlayerId}, Type3={Type3}",
                player.GID, player.Type3);
            var baseApplyElapsedMs = stopwatch.ElapsedMilliseconds;

            var equippedItems = await _equipmentRepository.GetListAsync(e => e.PlayerId == player.GID && e.IsEquipped);
            foreach (var item in equippedItems)
            {
                var template = EquipmentBalanceHelper.ResolveTemplate(item.TemplateId);
                player.Type1 += item.BaseHP;
                player.Type2 += item.BaseMP;
                player.Type3 += EquipmentBalanceHelper.GetPhysicalAttack(item, template);
                player.Type4 += EquipmentBalanceHelper.GetMagicAttack(item, template);
                player.Type5 += EquipmentBalanceHelper.GetPhysicalDefense(item, template);
                player.Type6 += EquipmentBalanceHelper.GetMagicDefense(item, template);
                ApplyEquipmentBonusStats(player, item.BonusStatsJson);
                ApplyEquipmentBonusStats(player, item.RerollStatsJson);
                ApplyEquipmentGemBonuses(player, item.GemSlotsJson);
            }
            _logger.LogInformation(
                "Recalc Step4 装备加成后: PlayerId={PlayerId}, Type3={Type3}",
                player.GID, player.Type3);
            var equipmentElapsedMs = stopwatch.ElapsedMilliseconds;

            // 心法加成已改为读取时动态计算，不再写入 DB。
            var sutraElapsedMs = stopwatch.ElapsedMilliseconds;

            // 中文注释：
            // 灵宠改成”独立战斗单位”后，不再给玩家人物面板追加属性。
            // 这里保留宠物查询仓储只是为了兼容现有服务构造，不再把出战宠物的生命/攻击/防御折算回人物。
            var petElapsedMs = stopwatch.ElapsedMilliseconds;

            player.LastUpdateTime = DateTime.Now;
            await _userRepository.UpdateAsync(player);
            var updatePlayerElapsedMs = stopwatch.ElapsedMilliseconds;

            return new RecalcStageTimings
            {
                NormalizeMs = normalizeElapsedMs,
                BaseApplyMs = baseApplyElapsedMs - normalizeElapsedMs,
                EquippedItemsMs = equipmentElapsedMs - baseApplyElapsedMs,
                SutraMs = sutraElapsedMs - equipmentElapsedMs,
                PetMs = petElapsedMs - sutraElapsedMs,
                UpdatePlayerMs = updatePlayerElapsedMs - petElapsedMs
            };
        }

        /// <summary>
        /// 属性重算阶段耗时记录（仅供日志输出）。
        /// </summary>
        private sealed class RecalcStageTimings
        {
            public long NormalizeMs { get; set; }

            public long BaseApplyMs { get; set; }

            public long EquippedItemsMs { get; set; }

            public long SutraMs { get; set; }

            public long PetMs { get; set; }

            public long UpdatePlayerMs { get; set; }
        }

        /// <summary>
        /// 把五行聚灵阵的五个固定属性加成叠加到玩家实体。
        /// </summary>
        private static void ApplyFiveElementBonuses(UserEntity player, FiveElementArrayEntity array)
        {
            var arrayLevel = Math.Clamp(array.ArrayLevel <= 0 ? 1 : array.ArrayLevel, 1, 50);
            var maxElementLevel = FiveElementProgressionRules.GetElementMaxLevel(arrayLevel);
            var levels = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["metal"] = Math.Clamp(array.MetalLevel <= 0 ? 1 : array.MetalLevel, 1, maxElementLevel),
                ["wood"] = Math.Clamp(array.WoodLevel <= 0 ? 1 : array.WoodLevel, 1, maxElementLevel),
                ["water"] = Math.Clamp(array.WaterLevel <= 0 ? 1 : array.WaterLevel, 1, maxElementLevel),
                ["fire"] = Math.Clamp(array.FireLevel <= 0 ? 1 : array.FireLevel, 1, maxElementLevel),
                ["earth"] = Math.Clamp(array.EarthLevel <= 0 ? 1 : array.EarthLevel, 1, maxElementLevel)
            };

            foreach (var entry in levels)
            {
                var range = FiveElementProgressionRules.ResolveBranchRange(entry.Key, entry.Value);
                var bonus = FiveElementProgressionRules.GetElementCurrentBonus(entry.Key, entry.Value);
                ApplyAttribute(player, range.AttributeType, bonus);
            }
        }

        /// <summary>
        /// 这些词条保存在 EquipmentInstanceEntity.BonusStatsJson 中，
        /// 如果重算时漏掉这一步，前端会看到“洗练成功但人物属性没变化”的假象。
        /// </summary>
        private static void ApplyEquipmentBonusStats(UserEntity player, string? bonusStatsJson)
        {
            if (string.IsNullOrWhiteSpace(bonusStatsJson))
            {
                return;
            }

            try
            {
                var bonusStats = JsonSerializer.Deserialize<List<EquipmentBonusStatModel>>(bonusStatsJson) ?? [];
                foreach (var stat in bonusStats)
                {
                    ApplyAttribute(player, stat.StatType, (float)stat.RawValue);
                }
            }
            catch
            {
                // 中文注释：
                // 这里故意吞掉单条坏数据，避免某件旧装备的脏 JSON 直接把整个人物属性接口打挂。
                // 开发期宁可降级忽略该条附加词条，也不能让整条角色链路报 500。
            }
        }

        /// <summary>
        /// 应用装备宝石孔的属性加成。
        /// </summary>
        private static void ApplyEquipmentGemBonuses(UserEntity player, string? gemSlotsJson)
        {
            if (string.IsNullOrWhiteSpace(gemSlotsJson)) return;
            try
            {
                var slots = JsonSerializer.Deserialize<List<GemSlotEntry>>(gemSlotsJson);
                if (slots == null) return;
                foreach (var slot in slots)
                {
                    if (string.IsNullOrEmpty(slot.GemId) || string.IsNullOrEmpty(slot.AttributeType)) continue;
                    ApplyAttribute(player, slot.AttributeType, slot.BonusValue);
                }
            }
            catch { }
        }

        /// <summary>
        /// 中文注释：
        /// 统一把属性类型和值应用到角色实体。
        /// 属性加点和装备洗练都会复用这套逻辑，避免两边分别维护一套映射后出现不一致。
        /// </summary>
        private static void ApplyAttribute(UserEntity player, string attributeType, double value)
        {
            switch (attributeType)
            {
                case nameof(AttributeType.Type1):
                    player.Type1 += (int)Math.Round(value, MidpointRounding.AwayFromZero);
                    break;
                case nameof(AttributeType.Type2):
                    player.Type2 += (int)Math.Round(value, MidpointRounding.AwayFromZero);
                    break;
                case nameof(AttributeType.Type3):
                    player.Type3 += (int)Math.Round(value, MidpointRounding.AwayFromZero);
                    break;
                case nameof(AttributeType.Type4):
                    player.Type4 += (int)Math.Round(value, MidpointRounding.AwayFromZero);
                    break;
                case nameof(AttributeType.Type5):
                    player.Type5 += (int)Math.Round(value, MidpointRounding.AwayFromZero);
                    break;
                case nameof(AttributeType.Type6):
                    player.Type6 += (int)Math.Round(value, MidpointRounding.AwayFromZero);
                    break;
                case nameof(AttributeType.Type7):
                    player.Type7 += (int)Math.Round(value, MidpointRounding.AwayFromZero);
                    break;
                case nameof(AttributeType.Type8):
                    player.Type8 += (float)value;
                    break;
                case nameof(AttributeType.Type9):
                    player.Type9 += (float)value;
                    break;
                case nameof(AttributeType.Type10):
                    player.Type10 += (float)value;
                    break;
                case nameof(AttributeType.Type11):
                    player.Type11 += (float)value;
                    break;
                case nameof(AttributeType.Type12):
                    player.Type12 += (float)value;
                    break;
                case nameof(AttributeType.Type13):
                    player.Type13 += (float)value;
                    break;
                case nameof(AttributeType.Type14):
                    player.Type14 += (float)value;
                    break;
                case nameof(AttributeType.Type15):
                    player.Type15 += (float)value;
                    break;
            }
        }

        /// <summary>
        /// 中文注释：
        /// 属性点现在独立保存为整数点数，重算人物时再按当前倍率换算成实际面板加成。
        /// 这样以后只改倍率配置即可，不需要继续把“已经换算好的加成值”反写回存档。
        /// </summary>
        private static void ApplyAttributePointAllocations(
            UserEntity player,
            IReadOnlyCollection<PlayerAttributeAllocationEntity> allocations)
        {
            foreach (var allocation in allocations)
            {
                if (!AttributePointConfig.TryResolveDefinition(player.Profession, allocation.AttributeKey, out var definition) || definition == null)
                {
                    continue;
                }

                var bonusValue = AttributePointConfig.GetBonusValue(allocation.AllocatedPoints, definition);
                if (bonusValue <= 0d)
                {
                    continue;
                }

                ApplyAttribute(player, definition.AttributeType.ToString(), bonusValue);
            }
        }

        /// <summary>
        /// 中文注释：
        /// 当前版本的突破效果先采用“基础战斗属性按阶段百分比放大”的轻量模型，
        /// 这样不用新开复杂成长表，也能让突破后的面板与战斗表现真实发生变化。
        /// </summary>
        private static void ApplyBreakthroughBonus(UserEntity player)
        {
            var bonusPercent = RealmLevelCatalog.GetAttributeBonusPercent(player.Level);
            if (bonusPercent <= 0)
            {
                return;
            }

            var multiplier = 1d + bonusPercent / 100d;
            player.Type1 = (int)Math.Round(player.Type1 * multiplier, MidpointRounding.AwayFromZero);
            player.Type2 = (int)Math.Round(player.Type2 * multiplier, MidpointRounding.AwayFromZero);
            player.Type3 = (int)Math.Round(player.Type3 * multiplier, MidpointRounding.AwayFromZero);
            player.Type4 = (int)Math.Round(player.Type4 * multiplier, MidpointRounding.AwayFromZero);
            player.Type5 = (int)Math.Round(player.Type5 * multiplier, MidpointRounding.AwayFromZero);
            player.Type6 = (int)Math.Round(player.Type6 * multiplier, MidpointRounding.AwayFromZero);
            player.Type7 = (int)Math.Round(player.Type7 * multiplier, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// 中文注释：
        /// 经验可能来自战斗、签到、兑换码、任务和成就奖励，
        /// 这些链路并不都经过同一个玩家服务入口。
        /// 这里在“重算完成后”统一补齐等级相关的任务和成就同步，
        /// 并通过按需解析服务规避 QuestService/AchievementService 与本服务的构造循环依赖。
        /// </summary>
        private async Task SyncLevelDrivenProgressAsync(UserEntity player)
        {
            await SyncLevelQuestProgressAsync(player);
            await SyncLevelAchievementProgressAsync(player);
        }

        private async Task SyncLevelQuestProgressAsync(UserEntity player)
        {
            try
            {
                var questService = _serviceProvider.GetService<IQuestService>();
                if (questService == null)
                {
                    return;
                }

                await questService.SyncObjectiveStateAsync(player.GID, ObjectiveType.ReachLevel);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to sync level quests for player {PlayerId}", player.GID);
            }
        }

        private async Task SyncLevelAchievementProgressAsync(UserEntity player)
        {
            try
            {
                var achievementService = _serviceProvider.GetService<IAchievementService>();
                if (achievementService == null)
                {
                    return;
                }

                await achievementService.SyncRequirementStateAsync(player.GID, AchievementRequirementType.ReachLevel);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to sync level achievements for player {PlayerId}", player.GID);
            }
        }

        /// <summary>
        /// 中文注释：
        /// 这个内部模型只用于解析设备附加词条 JSON。
        /// 不直接复用 DTO，是为了让领域层重算逻辑不依赖前端展示字段。
        /// </summary>
        private sealed class EquipmentBonusStatModel
        {
            public string StatType { get; set; } = string.Empty;

            public double RawValue { get; set; }
        }

        /// <summary>
        /// 中文注释：
        /// 按百分比乘算单个属性。Type1-7 是 int，Type8-15 是 float。
        /// </summary>
        private static void ApplyPercentageBonus(UserEntity player, string attributeName, double value)
        {
            switch (attributeName)
            {
                case nameof(AttributeType.Type1):
                    player.Type1 = (int)Math.Round(player.Type1 * (1 + value / 100.0), MidpointRounding.AwayFromZero);
                    break;
                case nameof(AttributeType.Type2):
                    player.Type2 = (int)Math.Round(player.Type2 * (1 + value / 100.0), MidpointRounding.AwayFromZero);
                    break;
                case nameof(AttributeType.Type3):
                    player.Type3 = (int)Math.Round(player.Type3 * (1 + value / 100.0), MidpointRounding.AwayFromZero);
                    break;
                case nameof(AttributeType.Type4):
                    player.Type4 = (int)Math.Round(player.Type4 * (1 + value / 100.0), MidpointRounding.AwayFromZero);
                    break;
                case nameof(AttributeType.Type5):
                    player.Type5 = (int)Math.Round(player.Type5 * (1 + value / 100.0), MidpointRounding.AwayFromZero);
                    break;
                case nameof(AttributeType.Type6):
                    player.Type6 = (int)Math.Round(player.Type6 * (1 + value / 100.0), MidpointRounding.AwayFromZero);
                    break;
                case nameof(AttributeType.Type7):
                    player.Type7 = (int)Math.Round(player.Type7 * (1 + value / 100.0), MidpointRounding.AwayFromZero);
                    break;
                case nameof(AttributeType.Type8):
                    player.Type8 += (float)(value / 100.0);
                    break;
                case nameof(AttributeType.Type9):
                    player.Type9 += (float)(value / 100.0);
                    break;
                case nameof(AttributeType.Type10):
                    player.Type10 += (float)(value / 100.0);
                    break;
                case nameof(AttributeType.Type11):
                    player.Type11 += (float)(value / 100.0);
                    break;
                case nameof(AttributeType.Type12):
                    player.Type12 += (float)(value / 100.0);
                    break;
                case nameof(AttributeType.Type13):
                    player.Type13 += (float)(value / 100.0);
                    break;
                case nameof(AttributeType.Type14):
                    player.Type14 += (float)(value / 100.0);
                    break;
                case nameof(AttributeType.Type15):
                    player.Type15 += (float)(value / 100.0);
                    break;
            }
        }

        /// <summary>
        /// 恢复生命值。
        /// </summary>
        public async Task<bool> RestoreHPAsync(string playerId, int amount)
        {
            var player = await _userRepository.GetByIdAsync(playerId);
            if (player == null)
            {
                return false;
            }

            player.LastUpdateTime = DateTime.Now;
            await _userRepository.UpdateAsync(player);
            return true;
        }

        /// <summary>
        /// 恢复法力值。
        /// </summary>
        public async Task<bool> RestoreMPAsync(string playerId, int amount)
        {
            var player = await _userRepository.GetByIdAsync(playerId);
            if (player == null)
            {
                return false;
            }

            player.LastUpdateTime = DateTime.Now;
            await _userRepository.UpdateAsync(player);
            return true;
        }

        /// <summary>
        /// 增加经验值。
        /// </summary>
        public async Task<bool> AddExpAsync(string playerId, long amount, bool recalculateAttributes = true)
        {
            var player = await _userRepository.GetByIdAsync(playerId);
            if (player == null)
            {
                return false;
            }

            var result = PlayerManager.AddExp(player, amount);
            if (!result.Success)
            {
                return false;
            }

            player.XExp = PlayerManager.GetRequiredExp(player);
            player.LastUpdateTime = DateTime.Now;
            await _userRepository.UpdateAsync(player);

            // 中文注释：
            // 战斗结算是经验增长的高频入口，这里也必须统一收口到完整重算，
            // 否则战斗升级后的面板和后续换装后的面板会不一致。
            if (recalculateAttributes)
            {
                await RecalculatePlayerAttributesAsync(playerId, syncLevelDrivenProgress: true);
            }
            return true;
        }

        /// <summary>
        /// 扣除金币。
        /// </summary>
        public async Task<bool> DeductGoldAsync(string playerId, long amount, string reason)
        {
            if (amount <= 0)
            {
                return false;
            }

            var now = DateTime.Now;
            var affectedRows = await _dbContext.Db.Updateable<UserEntity>()
                .SetColumns(u => u.Gold == u.Gold - amount)
                .SetColumns(u => u.TotalGoldSpent == u.TotalGoldSpent + amount)
                .SetColumns(u => u.LastUpdateTime == now)
                .Where(u => u.GID == playerId && !u.IsDeleted && u.Gold >= amount)
                .ExecuteCommandAsync();

            if (affectedRows > 0)
            {
                await _gameSyncService.SyncPlayerAsync(playerId);
            }

            return affectedRows > 0;
        }

        /// <summary>
        /// 增加金币。
        /// </summary>
        public async Task<bool> AddGoldAsync(string playerId, long amount, string reason, bool syncRankings = true)
        {
            if (amount <= 0)
            {
                return false;
            }

            var now = DateTime.Now;
            var affectedRows = await _dbContext.Db.Updateable<UserEntity>()
                .SetColumns(u => u.Gold == u.Gold + amount)
                .SetColumns(u => u.TotalGoldEarned == u.TotalGoldEarned + amount)
                .SetColumns(u => u.LastUpdateTime == now)
                .Where(u => u.GID == playerId && !u.IsDeleted)
                .ExecuteCommandAsync();

            if (affectedRows > 0 && syncRankings)
            {
                await _gameSyncService.SyncPlayerAsync(playerId);
            }

            return affectedRows > 0;
        }

        /// <summary>
        /// 检查资源是否足够。
        /// </summary>
        public async Task<bool> HasEnoughResourceAsync(string playerId, string resourceType, long amount)
        {
            var player = await _userRepository.GetByIdAsync(playerId);
            if (player == null)
            {
                return false;
            }

            return resourceType.ToLower() switch
            {
                "gold" => player.Gold >= amount,
                "spiritstone" => player.SpiritStone >= amount,
                _ => false
            };
        }

        public async Task<List<HeartSutraBonusSummaryDto>> GetHeartSutraBonusesAsync(string playerId)
        {
            var playerSutras = await _playerSutraRepository.GetListAsync(s => s.PlayerId == playerId);
            if (playerSutras.Count == 0)
                return [];

            var sutraIds = playerSutras.Select(s => s.SutraId).ToList();
            var templates = await _heartSutraRepository.GetListAsync(t => sutraIds.Contains(t.SutraId));
            var templateMap = templates.ToDictionary(t => t.SutraId);

            var bonusMap = new Dictionary<string, HeartSutraBonusSummaryDto>();
            var percentageBonuses = new List<(string AttrType, float Value)>();

            foreach (var playerSutra in playerSutras)
            {
                if (playerSutra.CurrentLayer <= 0) continue;
                if (!templateMap.TryGetValue(playerSutra.SutraId, out var template)) continue;

                List<SutraLayerConfig> layerConfigs = [];
                if (!string.IsNullOrWhiteSpace(template.LayersJson))
                {
                    try { layerConfigs = JsonSerializer.Deserialize<List<SutraLayerConfig>>(template.LayersJson) ?? []; }
                    catch { continue; }
                }

                foreach (var layerConfig in layerConfigs.Where(l => l.Layer <= playerSutra.CurrentLayer))
                {
                    foreach (var bonus in layerConfig.Bonuses)
                    {
                        if (string.IsNullOrWhiteSpace(bonus.AttributeName)) continue;

                        if (!bonusMap.TryGetValue(bonus.AttributeName, out var summary))
                        {
                            summary = new HeartSutraBonusSummaryDto { AttrType = bonus.AttributeName };
                            bonusMap[bonus.AttributeName] = summary;
                        }

                        if (bonus.IsPercentage)
                            summary.PercentValue += (float)bonus.Value;
                        else
                            summary.FixedValue += (float)bonus.Value;
                    }
                }
            }

            return bonusMap.Values.ToList();
        }

        public async Task ApplyHeartSutraBonusesToDtoAsync(string playerId, PlayerDto dto)
        {
            var bonuses = await GetHeartSutraBonusesAsync(playerId);
            if (bonuses.Count == 0) return;

            // 先叠加固定值
            foreach (var bonus in bonuses)
            {
                if (bonus.FixedValue != 0)
                    ApplySutraFixedToDto(dto, bonus.AttrType, bonus.FixedValue);
            }
            // 再叠加百分比（基于基础+装备+固定值）
            foreach (var bonus in bonuses)
            {
                if (bonus.PercentValue != 0)
                    ApplySutraPercentToDto(dto, bonus.AttrType, bonus.PercentValue);
            }
        }

        public async Task ApplyHeartSutraBonusesToEntityAsync(string playerId, UserEntity player)
        {
            var bonuses = await GetHeartSutraBonusesAsync(playerId);
            if (bonuses.Count == 0) return;

            foreach (var bonus in bonuses)
            {
                if (bonus.FixedValue != 0)
                    ApplyAttribute(player, bonus.AttrType, bonus.FixedValue);
            }
            foreach (var bonus in bonuses)
            {
                if (bonus.PercentValue != 0)
                    ApplyPercentageBonus(player, bonus.AttrType, bonus.PercentValue);
            }
        }

        /// <summary>
        /// 同时叠加心法和图鉴加成到战斗实体。
        /// 固定值先叠加，再从叠加前的基值统一计算百分比加成，避免百分比滚雪球。
        /// </summary>
        public async Task ApplyAllBonusesToEntityAsync(string playerId, UserEntity player)
        {
            var sutraBonuses = await GetHeartSutraBonusesAsync(playerId);
            var collectionBonuses = await _collectionService.GetPlayerBonusesAsync(playerId);

            if (sutraBonuses.Count == 0 && collectionBonuses.Count == 0) return;

            // 先叠加所有固定值
            foreach (var bonus in sutraBonuses)
            {
                if (bonus.FixedValue != 0)
                    ApplyAttribute(player, bonus.AttrType, bonus.FixedValue);
            }
            foreach (var bonus in collectionBonuses)
            {
                if (bonus.FixedValue != 0)
                    ApplyAttribute(player, bonus.AttrType, bonus.FixedValue);
            }

            // 快照叠加固定值后的基值
            var baseType1 = player.Type1;
            var baseType2 = player.Type2;
            var baseType3 = player.Type3;
            var baseType4 = player.Type4;
            var baseType5 = player.Type5;
            var baseType6 = player.Type6;
            var baseType7 = player.Type7;
            var baseType8 = player.Type8;
            var baseType9 = player.Type9;
            var baseType10 = player.Type10;
            var baseType11 = player.Type11;
            var baseType12 = player.Type12;
            var baseType13 = player.Type13;
            var baseType14 = player.Type14;
            var baseType15 = player.Type15;

            // 从基值统一计算百分比加成
            foreach (var bonus in sutraBonuses)
            {
                if (bonus.PercentValue != 0)
                    ApplyAdditivePercentFromBase(player, bonus.AttrType, bonus.PercentValue,
                        baseType1, baseType2, baseType3, baseType4, baseType5, baseType6, baseType7,
                        baseType8, baseType9, baseType10, baseType11, baseType12, baseType13, baseType14, baseType15);
            }
            foreach (var bonus in collectionBonuses)
            {
                if (bonus.PercentValue != 0)
                    ApplyAdditivePercentFromBase(player, bonus.AttrType, bonus.PercentValue,
                        baseType1, baseType2, baseType3, baseType4, baseType5, baseType6, baseType7,
                        baseType8, baseType9, baseType10, baseType11, baseType12, baseType13, baseType14, baseType15);
            }
        }

        /// <summary>
        /// 从快照基值计算百分比加成并叠加到角色（加算，非乘算）。
        /// </summary>
        private static void ApplyAdditivePercentFromBase(
            UserEntity player, string attrType, float value,
            int baseT1, int baseT2, int baseT3, int baseT4, int baseT5, int baseT6, int baseT7,
            float baseT8, float baseT9, float baseT10, float baseT11, float baseT12, float baseT13, float baseT14, float baseT15)
        {
            switch (attrType)
            {
                case nameof(AttributeType.Type1):
                    player.Type1 += (int)Math.Round(baseT1 * (value / 100.0), MidpointRounding.AwayFromZero);
                    break;
                case nameof(AttributeType.Type2):
                    player.Type2 += (int)Math.Round(baseT2 * (value / 100.0), MidpointRounding.AwayFromZero);
                    break;
                case nameof(AttributeType.Type3):
                    player.Type3 += (int)Math.Round(baseT3 * (value / 100.0), MidpointRounding.AwayFromZero);
                    break;
                case nameof(AttributeType.Type4):
                    player.Type4 += (int)Math.Round(baseT4 * (value / 100.0), MidpointRounding.AwayFromZero);
                    break;
                case nameof(AttributeType.Type5):
                    player.Type5 += (int)Math.Round(baseT5 * (value / 100.0), MidpointRounding.AwayFromZero);
                    break;
                case nameof(AttributeType.Type6):
                    player.Type6 += (int)Math.Round(baseT6 * (value / 100.0), MidpointRounding.AwayFromZero);
                    break;
                case nameof(AttributeType.Type7):
                    player.Type7 += (int)Math.Round(baseT7 * (value / 100.0), MidpointRounding.AwayFromZero);
                    break;
                case nameof(AttributeType.Type8):
                    player.Type8 += (float)(baseT8 * value / 100.0);
                    break;
                case nameof(AttributeType.Type9):
                    player.Type9 += (float)(baseT9 * value / 100.0);
                    break;
                case nameof(AttributeType.Type10):
                    player.Type10 += (float)(baseT10 * value / 100.0);
                    break;
                case nameof(AttributeType.Type11):
                    player.Type11 += (float)(baseT11 * value / 100.0);
                    break;
                case nameof(AttributeType.Type12):
                    player.Type12 += (float)(baseT12 * value / 100.0);
                    break;
                case nameof(AttributeType.Type13):
                    player.Type13 += (float)(baseT13 * value / 100.0);
                    break;
                case nameof(AttributeType.Type14):
                    player.Type14 += (float)(baseT14 * value / 100.0);
                    break;
                case nameof(AttributeType.Type15):
                    player.Type15 += (float)(baseT15 * value / 100.0);
                    break;
            }
        }

        /// <summary>
        /// 同时叠加心法和图鉴加成到 DTO。
        /// 固定值先叠加，再从叠加前的基值统一计算百分比加成，避免百分比滚雪球。
        /// </summary>
        public async Task ApplyAllBonusesToDtoAsync(string playerId, PlayerDto dto)
        {
            var sutraBonuses = await GetHeartSutraBonusesAsync(playerId);
            var collectionBonuses = await _collectionService.GetPlayerBonusesAsync(playerId);

            if (sutraBonuses.Count == 0 && collectionBonuses.Count == 0) return;

            // 先叠加所有固定值
            foreach (var bonus in sutraBonuses)
            {
                if (bonus.FixedValue != 0)
                    ApplySutraFixedToDto(dto, bonus.AttrType, bonus.FixedValue);
            }
            foreach (var bonus in collectionBonuses)
            {
                if (bonus.FixedValue != 0)
                    ApplyCollectionFixedBonusToDto(dto, bonus.AttrType, bonus.FixedValue);
            }

            // 快照叠加固定值后的基值
            var baseMaxHP = dto.MaxHP;
            var baseMaxMP = dto.MaxMP;
            var baseAttack = dto.Attack;
            var baseMagicAttack = dto.MagicAttack;
            var baseDefense = dto.Defense;
            var baseMagicDefense = dto.MagicDefense;
            var baseSpeed = dto.Speed;
            var baseHitRate = dto.HitRate;
            var baseDodgeRate = dto.DodgeRate;
            var baseCritRate = dto.CritRate;
            var baseCritDamage = dto.CritDamage;
            var baseComboRate = dto.ComboRate;
            var baseCounterRate = dto.CounterRate;
            var baseArmorBreak = dto.ArmorBreak;
            var baseBonusDamage = dto.BonusDamage;

            // 从基值统一计算百分比加成
            foreach (var bonus in sutraBonuses)
            {
                if (bonus.PercentValue != 0)
                    ApplyAdditivePercentToDto(dto, bonus.AttrType, bonus.PercentValue,
                        baseMaxHP, baseMaxMP, baseAttack, baseMagicAttack, baseDefense, baseMagicDefense, baseSpeed,
                        baseHitRate, baseDodgeRate, baseCritRate, baseCritDamage, baseComboRate, baseCounterRate, baseArmorBreak, baseBonusDamage);
            }
            foreach (var bonus in collectionBonuses)
            {
                if (bonus.PercentValue != 0)
                    ApplyAdditivePercentToDto(dto, bonus.AttrType, bonus.PercentValue,
                        baseMaxHP, baseMaxMP, baseAttack, baseMagicAttack, baseDefense, baseMagicDefense, baseSpeed,
                        baseHitRate, baseDodgeRate, baseCritRate, baseCritDamage, baseComboRate, baseCounterRate, baseArmorBreak, baseBonusDamage);
            }
        }

        private static void ApplyCollectionFixedBonusToDto(PlayerDto dto, string attrType, float value)
        {
            switch (attrType)
            {
                case "Type1": dto.MaxHP += (int)Math.Round(value); dto.HP = dto.MaxHP; break;
                case "Type2": dto.MaxMP += (int)Math.Round(value); dto.MP = dto.MaxMP; break;
                case "Type3": dto.Attack += (int)Math.Round(value); break;
                case "Type4": dto.MagicAttack += (int)Math.Round(value); break;
                case "Type5": dto.Defense += (int)Math.Round(value); break;
                case "Type6": dto.MagicDefense += (int)Math.Round(value); break;
                case "Type7": dto.Speed += (int)Math.Round(value); break;
                case "Type8": dto.HitRate += value; break;
                case "Type9": dto.DodgeRate += value; break;
                case "Type10": dto.CritRate += value; break;
                case "Type11": dto.CritDamage += value; break;
                case "Type12": dto.ComboRate += value; break;
                case "Type13": dto.CounterRate += value; break;
                case "Type14": dto.ArmorBreak += value; break;
                case "Type15": dto.BonusDamage += value; break;
            }
        }

        private static void ApplyAdditivePercentToDto(
            PlayerDto dto, string attrType, float value,
            int baseMaxHP, int baseMaxMP, int baseAttack, int baseMagicAttack, int baseDefense, int baseMagicDefense, int baseSpeed,
            double baseHitRate, double baseDodgeRate, double baseCritRate, double baseCritDamage,
            double baseComboRate, double baseCounterRate, double baseArmorBreak, double baseBonusDamage)
        {
            switch (attrType)
            {
                case "Type1":
                    dto.MaxHP += (int)Math.Round(baseMaxHP * (value / 100f)); dto.HP = dto.MaxHP;
                    break;
                case "Type2":
                    dto.MaxMP += (int)Math.Round(baseMaxMP * (value / 100f)); dto.MP = dto.MaxMP;
                    break;
                case "Type3": dto.Attack += (int)Math.Round(baseAttack * (value / 100f)); break;
                case "Type4": dto.MagicAttack += (int)Math.Round(baseMagicAttack * (value / 100f)); break;
                case "Type5": dto.Defense += (int)Math.Round(baseDefense * (value / 100f)); break;
                case "Type6": dto.MagicDefense += (int)Math.Round(baseMagicDefense * (value / 100f)); break;
                case "Type7": dto.Speed += (int)Math.Round(baseSpeed * (value / 100f)); break;
                case "Type8": dto.HitRate += baseHitRate * value / 100f; break;
                case "Type9": dto.DodgeRate += baseDodgeRate * value / 100f; break;
                case "Type10": dto.CritRate += baseCritRate * value / 100f; break;
                case "Type11": dto.CritDamage += baseCritDamage * value / 100f; break;
                case "Type12": dto.ComboRate += baseComboRate * value / 100f; break;
                case "Type13": dto.CounterRate += baseCounterRate * value / 100f; break;
                case "Type14": dto.ArmorBreak += baseArmorBreak * value / 100f; break;
                case "Type15": dto.BonusDamage += baseBonusDamage * value / 100f; break;
            }
        }

        private static void ApplySutraFixedToDto(PlayerDto dto, string attrType, float value)
        {
            switch (attrType)
            {
                case "Type1": dto.MaxHP += (int)Math.Round(value); break;
                case "Type2": dto.MaxMP += (int)Math.Round(value); break;
                case "Type3": dto.Attack += (int)Math.Round(value); break;
                case "Type4": dto.MagicAttack += (int)Math.Round(value); break;
                case "Type5": dto.Defense += (int)Math.Round(value); break;
                case "Type6": dto.MagicDefense += (int)Math.Round(value); break;
                case "Type7": dto.Speed += (int)Math.Round(value); break;
                case "Type8": dto.HitRate += value; break;
                case "Type9": dto.DodgeRate += value; break;
                case "Type10": dto.CritRate += value; break;
                case "Type11": dto.CritDamage += value; break;
                case "Type12": dto.ComboRate += value; break;
                case "Type13": dto.CounterRate += value; break;
                case "Type14": dto.ArmorBreak += value; break;
                case "Type15": dto.BonusDamage += value; break;
            }
        }

        private static void ApplySutraPercentToDto(PlayerDto dto, string attrType, float value)
        {
            switch (attrType)
            {
                case "Type1": dto.MaxHP = (int)Math.Round(dto.MaxHP * (1 + value / 100f)); break;
                case "Type2": dto.MaxMP = (int)Math.Round(dto.MaxMP * (1 + value / 100f)); break;
                case "Type3": dto.Attack = (int)Math.Round(dto.Attack * (1 + value / 100f)); break;
                case "Type4": dto.MagicAttack = (int)Math.Round(dto.MagicAttack * (1 + value / 100f)); break;
                case "Type5": dto.Defense = (int)Math.Round(dto.Defense * (1 + value / 100f)); break;
                case "Type6": dto.MagicDefense = (int)Math.Round(dto.MagicDefense * (1 + value / 100f)); break;
                case "Type7": dto.Speed = (int)Math.Round(dto.Speed * (1 + value / 100f)); break;
                case "Type8": dto.HitRate += value / 100f; break;
                case "Type9": dto.DodgeRate += value / 100f; break;
                case "Type10": dto.CritRate += value / 100f; break;
                case "Type11": dto.CritDamage += value / 100f; break;
                case "Type12": dto.ComboRate += value / 100f; break;
                case "Type13": dto.CounterRate += value / 100f; break;
                case "Type14": dto.ArmorBreak += value / 100f; break;
                case "Type15": dto.BonusDamage += value / 100f; break;
            }
        }
    }
}
