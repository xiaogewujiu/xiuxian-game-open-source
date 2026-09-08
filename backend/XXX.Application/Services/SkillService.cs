using Microsoft.Extensions.Logging;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Data;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    /// <summary>
    /// 技能展示与携带服务。
    /// </summary>
    public class SkillService : ISkillService
    {
        private readonly IRepository<UserEntity> _userRepository;
        private readonly IRepository<SkillTemplateEntity> _skillTemplateRepository;
        private readonly IRepository<BuffTemplateEntity> _buffTemplateRepository;
        private readonly IRepository<InventoryItemEntity> _inventoryRepository;
        private readonly IRepository<ItemTemplateEntity> _itemTemplateRepository;
        private readonly DbContext _dbContext;
        private readonly ILogger<SkillService> _logger;

        /// <summary>
        /// 初始化技能服务。
        /// </summary>
        public SkillService(
            IRepository<UserEntity> userRepository,
            IRepository<SkillTemplateEntity> skillTemplateRepository,
            IRepository<BuffTemplateEntity> buffTemplateRepository,
            IRepository<InventoryItemEntity> inventoryRepository,
            IRepository<ItemTemplateEntity> itemTemplateRepository,
            DbContext dbContext,
            ILogger<SkillService> logger)
        {
            _userRepository = userRepository;
            _skillTemplateRepository = skillTemplateRepository;
            _buffTemplateRepository = buffTemplateRepository;
            _inventoryRepository = inventoryRepository;
            _itemTemplateRepository = itemTemplateRepository;
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// 获取玩家技能总览。
        /// </summary>
        public async Task<SkillOverviewDto> GetSkillOverviewAsync(string playerId)
        {
            var player = await GetExistingPlayerAsync(playerId);
            await EnsureOwnedSkillIdsAsync(player);
            return await BuildSkillOverviewAsync(player);
        }

        /// <summary>
        /// 直接升级当前已掌握技能，并在同一事务内替换技能和扣除资源。
        /// </summary>
        public async Task<SkillUpgradeResultDto> UpgradeSkillAsync(string playerId, int skillId)
        {
            if (skillId <= 0)
            {
                throw new InvalidOperationException("技能编号无效。");
            }

            return await _dbContext.ExecuteInTransactionAsync(async () =>
            {
                var player = await GetExistingPlayerAsync(playerId);
                var ownedSkillIds = NormalizeSkillIds(await EnsureOwnedSkillIdsAsync(player));
                var allSkills = await _skillTemplateRepository.GetAllAsync();
                var current = allSkills.FirstOrDefault(skill => skill.SkillId == skillId);
                if (current == null)
                {
                    throw new InvalidOperationException("技能模板不存在。");
                }


                if (!ownedSkillIds.Contains(skillId.ToString(), StringComparer.Ordinal))
                {
                    throw new InvalidOperationException("未掌握该技能，无法升级。");
                }

                var ownedInChain = allSkills
                    .Where(skill => ownedSkillIds.Contains(skill.SkillId.ToString(), StringComparer.Ordinal))
                    .Where(skill => IsSameSkillChain(skill, current, allSkills))
                    .ToList();
                if (ownedInChain.Any(skill => skill.SkillLevel > current.SkillLevel))
                {
                    throw new InvalidOperationException("只能升级当前已掌握的最高等级技能。");
                }

                if (!current.NextSkillId.HasValue)
                {
                    throw new InvalidOperationException("该技能已经是当前最高等级。");
                }

                var next = allSkills.FirstOrDefault(skill => skill.SkillId == current.NextSkillId.Value);
                if (next == null || next.SkillLevel != current.SkillLevel + 1 || next.PreviousSkillId != current.SkillId)
                {
                    throw new InvalidOperationException("技能升级关系配置无效。");
                }

                var conditions = current.UpgradeConditions
                    .Where(condition => condition.Amount > 0)
                    .ToList();
                var itemConditions = conditions
                    .Where(condition => condition.Type == SkillUpgradeConditionType.Item)
                    .GroupBy(condition => condition.ItemId?.Trim(), StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(group => group.Key ?? string.Empty, group => group.Sum(condition => condition.Amount), StringComparer.OrdinalIgnoreCase);
                if (itemConditions.Keys.Any(string.IsNullOrWhiteSpace))
                {
                    throw new InvalidOperationException("技能升级道具条件配置无效。");
                }

                foreach (var condition in conditions)
                {
                    var enough = condition.Type switch
                    {
                        SkillUpgradeConditionType.Gold => player.Gold >= condition.Amount,
                        SkillUpgradeConditionType.SpiritStone => player.SpiritStone >= condition.Amount,
                        SkillUpgradeConditionType.PlayerLevel => player.Level >= condition.Amount,
                        SkillUpgradeConditionType.Item => await GetInventoryQuantityAsync(playerId, condition.ItemId!) >= condition.Amount,
                        _ => false
                    };
                    if (!enough)
                    {
                        throw new InvalidOperationException("技能升级条件未满足或资源不足。");
                    }
                }

                var goldCost = conditions.Where(condition => condition.Type == SkillUpgradeConditionType.Gold).Sum(condition => condition.Amount);
                var spiritStoneCost = conditions.Where(condition => condition.Type == SkillUpgradeConditionType.SpiritStone).Sum(condition => condition.Amount);
                player.Gold -= goldCost;
                player.SpiritStone -= spiritStoneCost;

                foreach (var itemCondition in itemConditions)
                {
                    await DeductInventoryQuantityAsync(playerId, itemCondition.Key, checked((int)itemCondition.Value));
                }

                var normalizedCurrentId = current.SkillId.ToString();
                var normalizedNextId = next.SkillId.ToString();
                ownedSkillIds.Remove(normalizedCurrentId);
                ownedSkillIds = ownedSkillIds
                    .Where(id => !allSkills.Any(skill =>
                        skill.SkillId.ToString() == id
                        && IsSameSkillChain(skill, current, allSkills)
                        && skill.SkillLevel <= current.SkillLevel))
                    .ToList();
                ownedSkillIds.Add(normalizedNextId);
                player.OwnedSkillIds = ownedSkillIds;

                var equippedSkillIds = NormalizeSkillIds(player.SkillIds);
                var equippedIndex = equippedSkillIds.IndexOf(normalizedCurrentId);
                if (equippedIndex >= 0)
                {
                    equippedSkillIds[equippedIndex] = normalizedNextId;
                    player.SkillIds = equippedSkillIds;
                }

                player.LastUpdateTime = DateTime.Now;
                await _userRepository.UpdateAsync(player);
                var overview = await BuildSkillOverviewAsync(player);
                var itemBalances = new List<SkillUpgradeItemBalanceDto>();
                foreach (var itemId in itemConditions.Keys)
                {
                    itemBalances.Add(new SkillUpgradeItemBalanceDto
                    {
                        ItemId = itemId,
                        Quantity = await GetInventoryQuantityAsync(playerId, itemId)
                    });
                }

                return new SkillUpgradeResultDto
                {
                    Overview = overview,
                    Gold = player.Gold,
                    SpiritStone = player.SpiritStone,
                    ItemBalances = itemBalances
                };
            });
        }

        /// <summary>
        /// 判断两个技能是否属于同一条上下级链。
        /// </summary>
        private static bool IsSameSkillChain(SkillTemplateEntity left, SkillTemplateEntity right, IReadOnlyCollection<SkillTemplateEntity> allSkills)
        {
            var currentId = left.SkillId;
            var visited = new HashSet<int>();
            while (currentId > 0 && visited.Add(currentId))
            {
                if (currentId == right.SkillId) return true;
                var current = allSkills.FirstOrDefault(skill => skill.SkillId == currentId);
                if (current?.PreviousSkillId == null) break;
                currentId = current.PreviousSkillId.Value;
            }

            currentId = right.SkillId;
            visited.Clear();
            while (currentId > 0 && visited.Add(currentId))
            {
                if (currentId == left.SkillId) return true;
                var current = allSkills.FirstOrDefault(skill => skill.SkillId == currentId);
                if (current?.PreviousSkillId == null) break;
                currentId = current.PreviousSkillId.Value;
            }

            return false;
        }

        /// <summary>
        /// 获取玩家某个道具的总数量。
        /// </summary>
        private async Task<int> GetInventoryQuantityAsync(string playerId, string itemId)
        {
            return await _inventoryRepository.Db.Queryable<InventoryItemEntity>()
                .Where(item => item.PlayerId == playerId && item.ItemId == itemId)
                .SumAsync(item => item.Quantity);
        }

        /// <summary>
        /// 按绑定优先顺序扣除玩家道具。
        /// </summary>
        private async Task DeductInventoryQuantityAsync(string playerId, string itemId, int quantity)
        {
            var remaining = quantity;
            var items = await _inventoryRepository.Db.Queryable<InventoryItemEntity>()
                .Where(item => item.PlayerId == playerId && item.ItemId == itemId && item.Quantity > 0)
                .OrderByDescending(item => item.IsLocked)
                .ToListAsync();
            foreach (var item in items)
            {
                if (remaining <= 0) break;
                var deducted = Math.Min(remaining, item.Quantity);
                item.Quantity -= deducted;
                remaining -= deducted;
                await _inventoryRepository.UpdateAsync(item);
            }

            if (remaining > 0)
            {
                throw new InvalidOperationException("技能升级道具不足。");
            }
        }
        /// <summary>
        /// 携带一个技能。
        /// </summary>
        public async Task<SkillOverviewDto> EquipSkillAsync(string playerId, int skillId)
        {
            var player = await GetExistingPlayerAsync(playerId);
            var ownedSkillIds = await EnsureOwnedSkillIdsAsync(player);
            var normalizedSkillId = skillId.ToString();

            if (!ownedSkillIds.Contains(normalizedSkillId, StringComparer.Ordinal))
            {
                throw new InvalidOperationException("未掌握该技能，无法携带。");
            }

            var skillTemplate = await _skillTemplateRepository.GetFirstAsync(skill => skill.SkillId == skillId);
            if (skillTemplate == null)
            {
                throw new InvalidOperationException("技能模板不存在。");
            }

            if (!PlayerProfessionCatalog.Allows(player.Profession, skillTemplate.AllowedProfessions))
            {
                throw new InvalidOperationException($"当前职业无法携带该技能，仅 {PlayerProfessionCatalog.GetAllowedDisplayName(skillTemplate.AllowedProfessions)} 可用。");
            }

            var equippedSkillIds = NormalizeSkillIds(player.SkillIds);
            if (equippedSkillIds.Contains(normalizedSkillId, StringComparer.Ordinal))
            {
                return await BuildSkillOverviewAsync(player);
            }

            if (equippedSkillIds.Count >= 6)
            {
                throw new InvalidOperationException("当前技能槽已满，请先卸下一个技能。");
            }

            equippedSkillIds.Add(normalizedSkillId);
            player.SkillIds = equippedSkillIds;
            player.LastUpdateTime = DateTime.Now;
            await _userRepository.UpdateAsync(player);

            _logger.LogInformation("Player {PlayerId} equipped skill {SkillId}", playerId, skillId);
            return await BuildSkillOverviewAsync(player);
        }

        /// <summary>
        /// 卸下一个技能。
        /// </summary>
        public async Task<SkillOverviewDto> UnequipSkillAsync(string playerId, int skillId)
        {
            var player = await GetExistingPlayerAsync(playerId);
            await EnsureOwnedSkillIdsAsync(player);

            var normalizedSkillId = skillId.ToString();
            var equippedSkillIds = NormalizeSkillIds(player.SkillIds);
            if (!equippedSkillIds.Remove(normalizedSkillId))
            {
                throw new InvalidOperationException("当前未携带该技能。");
            }

            player.SkillIds = equippedSkillIds;
            player.LastUpdateTime = DateTime.Now;
            await _userRepository.UpdateAsync(player);

            _logger.LogInformation("Player {PlayerId} unequipped skill {SkillId}", playerId, skillId);
            return await BuildSkillOverviewAsync(player);
        }

        private async Task<UserEntity> GetExistingPlayerAsync(string playerId)
        {
            var player = await _userRepository.GetByIdAsync(playerId);
            if (player == null || player.IsDeleted)
            {
                throw new InvalidOperationException("玩家不存在，无法读取技能信息。");
            }

            return player;
        }

        private async Task<List<string>> EnsureOwnedSkillIdsAsync(UserEntity player)
        {
            // 中文注释：
            // 历史账号只有“当前携带技能”这一份数据，没有单独的“已掌握技能”集合。
            // 这里在第一次读取时把旧 SkillIds 回填到 OwnedSkillIds，
            // 这样旧号就能兼容新的“已掌握 / 已携带”双集合模型。
            var ownedSkillIds = NormalizeSkillIds(player.OwnedSkillIds);
            if (ownedSkillIds.Count > 0)
            {
                return ownedSkillIds;
            }

            var equippedSkillIds = NormalizeSkillIds(player.SkillIds);
            if (equippedSkillIds.Count == 0)
            {
                return ownedSkillIds;
            }

            player.OwnedSkillIds = equippedSkillIds;
            player.LastUpdateTime = DateTime.Now;
            await _userRepository.UpdateAsync(player);

            _logger.LogInformation(
                "Backfilled owned skills from equipped skills for player {PlayerId}: {Count}",
                player.GID,
                equippedSkillIds.Count);

            return equippedSkillIds;
        }

        private async Task<SkillOverviewDto> BuildSkillOverviewAsync(UserEntity player)
        {
            var equippedSkillIdSet = NormalizeSkillIds(player.SkillIds).ToHashSet(StringComparer.Ordinal);
            var ownedSkillIdSet = NormalizeSkillIds(player.OwnedSkillIds).ToHashSet(StringComparer.Ordinal);

            // 中文注释：
            // 技能弹窗现在完全以数据库模板为真源：
            // - 技能基础信息来自 SkillTemplates
            // - Buff 展示来自 BuffTemplates
            // 前端不再保留另一份本地写死技能表。
            var skillTemplates = await _skillTemplateRepository.Db.Queryable<SkillTemplateEntity>()
                .Where(skill => skill.SkillCatalog == SkillTemplateEntity.CurrentCatalog)
                .OrderBy(skill => skill.SkillId)
                .ToListAsync();
            var buffTemplates = await _buffTemplateRepository.Db.Queryable<BuffTemplateEntity>()
                .ToListAsync();
            var buffMap = buffTemplates.ToDictionary(buff => buff.BuffId, buff => buff, StringComparer.OrdinalIgnoreCase);
            var itemNames = (await _itemTemplateRepository.Db.Queryable<ItemTemplateEntity>().ToListAsync())
                .ToDictionary(item => item.ItemId, item => item.Name, StringComparer.OrdinalIgnoreCase);
            var inventoryQuantities = (await _inventoryRepository.Db.Queryable<InventoryItemEntity>()
                    .Where(item => item.PlayerId == player.GID)
                    .ToListAsync())
                .GroupBy(item => item.ItemId, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => (long)group.Sum(item => item.Quantity), StringComparer.OrdinalIgnoreCase);

            _logger.LogInformation(
                "Loaded {SkillCount} skill templates and {BuffCount} buff templates for player {PlayerId}",
                skillTemplates.Count,
                buffTemplates.Count,
                player.GID);

            if (skillTemplates.Count == 0)
            {
                _logger.LogWarning("Skill templates table is empty for player {PlayerId}", player.GID);
            }

            var librarySkills = skillTemplates
                .Select(skill => MapSkill(
                    skill,
                    buffMap,
                    equippedSkillIdSet.Contains(skill.SkillId.ToString()),
                    ownedSkillIdSet.Contains(skill.SkillId.ToString()),
                    PlayerProfessionCatalog.Allows(player.Profession, skill.AllowedProfessions),
                    player,
                    itemNames,
                    inventoryQuantities))
                .ToList();

            var dtoMap = librarySkills.ToDictionary(skill => skill.SkillId);
            foreach (var dto in librarySkills.Where(skill => skill.NextSkillId.HasValue))
            {
                var nextSkillTemplate = skillTemplates.FirstOrDefault(skill => skill.SkillId == dto.NextSkillId.Value);
                if (nextSkillTemplate != null && dtoMap.ContainsKey(nextSkillTemplate.SkillId))
                {
                    // 中文注释：
                    // 升级对比需要和当前技能悬浮窗完全一致的下一级技能详情，
                    // 不能只复制倍率、耗蓝等少数字段；直接复用同一套映射逻辑，
                    // 确保类型、目标、范围、段数、触发概率、描述和 Buff 全部返回。
                    dto.NextSkill = MapSkill(
                        nextSkillTemplate,
                        buffMap,
                        isEquipped: false,
                        isOwned: false,
                        professionAllowed: PlayerProfessionCatalog.Allows(player.Profession, nextSkillTemplate.AllowedProfessions),
                        player,
                        itemNames,
                        inventoryQuantities);
                }
            }

            return new SkillOverviewDto
            {
                MaxSlots = 6,
                Notice = skillTemplates.Count > 0
                    ? "当前技能与 Buff 展示读取 SQLite 模板库；技能书学习、重复转碎片和技能升级已接入。"
                    : "当前技能模板库为空，请先检查 SQLite 初始化与 Seed 日志。",
                EquippedSkills = librarySkills.Where(skill => skill.IsEquipped).Take(6).ToList(),
                LibrarySkills = librarySkills
            };
        }

        private static List<string> NormalizeSkillIds(IEnumerable<string>? skillIds)
        {
            // 中文注释：
            // 这里统一清洗账号里存的技能 ID，过滤空字符串、非法值和重复项，
            // 避免旧数据或手工改库导致技能栏出现“空槽被脏数据占住”的情况。
            return (skillIds ?? Enumerable.Empty<string>())
                .Select(skillId => skillId?.Trim())
                .Where(skillId => !string.IsNullOrWhiteSpace(skillId) && int.TryParse(skillId, out var parsedId) && parsedId > 0)
                .Distinct(StringComparer.Ordinal)
                .Select(skillId => skillId!)
                .ToList();
        }

        private static SkillDetailDto MapSkill(
            SkillTemplateEntity skill,
            IReadOnlyDictionary<string, BuffTemplateEntity> buffMap,
            bool isEquipped,
            bool isOwned,
            bool professionAllowed,
            UserEntity player,
            IReadOnlyDictionary<string, string> itemNames,
            IReadOnlyDictionary<string, long> inventoryQuantities)
        {
            // 中文注释：
            // 技能模板里只保存 BuffId 列表，真正展示时要把 Buff 模板展开成前端可直接渲染的结构。
            // 如果出现脏数据或缺模板，也保留一个兜底占位，方便直接在界面上看出是哪条配置有问题。
            var buffDtos = (skill.BuffIds ?? [])
                .Select(buffId => buffMap.TryGetValue(buffId, out var buffTemplate)
                    ? new SkillBuffDto
                    {
                        BuffId = buffId,
                        Name = buffTemplate.Name,
                        Description = buffTemplate.Description,
                        Duration = buffTemplate.Duration
                    }
                    : new SkillBuffDto
                    {
                        BuffId = buffId,
                        Name = buffId,
                        Description = "未找到对应 Buff 模板。",
                        Duration = 0
                    })
                .ToList();

            var displayMultiplier = skill.Hits != null && skill.Hits.Count > 0
                ? Math.Round(skill.Hits.Sum(hit => hit.DamageMultiplier), 2, MidpointRounding.AwayFromZero)
                : skill.DamageMultiplier;

            var upgradeConditions = skill.UpgradeConditions
                .Select(condition =>
                {
                    var itemId = condition.ItemId?.Trim() ?? string.Empty;
                    var currentAmount = condition.Type switch
                    {
                        SkillUpgradeConditionType.Gold => Math.Max(0L, player.Gold),
                        SkillUpgradeConditionType.SpiritStone => Math.Max(0L, player.SpiritStone),
                        SkillUpgradeConditionType.PlayerLevel => Math.Max(0, player.Level),
                        SkillUpgradeConditionType.Item when inventoryQuantities.TryGetValue(itemId, out var quantity) => quantity,
                        _ => 0L
                    };
                    var isMet = currentAmount >= condition.Amount;
                    var targetName = condition.Type switch
                    {
                        SkillUpgradeConditionType.Item when itemNames.TryGetValue(itemId, out var itemName)
                            && !string.IsNullOrWhiteSpace(itemName) => itemName,
                        SkillUpgradeConditionType.Item => string.IsNullOrWhiteSpace(itemId) ? "未知材料" : itemId,
                        SkillUpgradeConditionType.Gold => "金币",
                        SkillUpgradeConditionType.SpiritStone => "灵石",
                        SkillUpgradeConditionType.PlayerLevel => "玩家等级",
                        _ => "未知条件"
                    };

                    return new SkillUpgradeConditionDto
                    {
                        Type = condition.Type.ToString(),
                        TargetName = targetName,
                        Amount = condition.Amount,
                        CurrentAmount = currentAmount,
                        IsMet = isMet,
                        StatusText = isMet
                            ? "已满足"
                            : condition.Type == SkillUpgradeConditionType.PlayerLevel
                                ? $"当前等级 {currentAmount}，需要达到 {condition.Amount} 级"
                                : $"当前 {currentAmount}，还需 {Math.Max(0L, condition.Amount - currentAmount)}"
                    };
                })
                .ToList();

                return new SkillDetailDto
            {
                SkillId = skill.SkillId,
                SkillLevel = skill.SkillLevel,
                NextSkillId = skill.NextSkillId,
                HasNextSkill = skill.NextSkillId.HasValue,
                UpgradeConditions = upgradeConditions,
                Name = skill.Name,
                Description = skill.Description,
                DamageType = GetDamageTypeText((DamageType)skill.DamageType),
                TargetType = skill.TargetType == 1 ? "敌方" : "友方",
                RangeText = skill.RangeType == 0 ? "全体" : $"最多 {skill.RangeType} 个目标",
                ManaCost = skill.ManaCost,
                Cooldown = skill.Cooldown,
                HitCount = skill.Hits?.Count > 0 ? skill.Hits.Count : skill.HitCount,
                DamageMultiplier = displayMultiplier,
                TriggerChance = skill.TriggerChance,
                CanUpgrade = isOwned && skill.NextSkillId.HasValue,
                UpgradeStatusText = !isOwned ? "未掌握" : skill.NextSkillId.HasValue ? "可查看升级" : "已满级",
                IsEquipped = isEquipped,
                IsOwned = isOwned,
                CanOperate = isOwned && professionAllowed,
                OwnershipText = ResolveOwnershipText(skill, isEquipped, isOwned, professionAllowed),
                Buffs = buffDtos
            };
        }

        private static string ResolveOwnershipText(
            SkillTemplateEntity skill,
            bool isEquipped,
            bool isOwned,
            bool professionAllowed)
        {
            if (isEquipped)
            {
                return "已携带";
            }

            if (!isOwned)
            {
                return "未掌握";
            }

            if (!professionAllowed)
            {
                return $"限{PlayerProfessionCatalog.GetAllowedDisplayName(skill.AllowedProfessions)}";
            }

            return "可携带";
        }

        private static string GetDamageTypeText(DamageType damageType)
        {
            return damageType switch
            {
                DamageType.Physical => "物理",
                DamageType.Magic => "法术",
                DamageType.True => "真实",
                DamageType.Heal => "治疗",
                DamageType.Buff => "增益/减益",
                DamageType.ManaRestore => "回蓝",
                DamageType.Revive => "复活",
                DamageType.BloodSacrifice => "血祭",
                DamageType.LifeConversion => "生命转换",
                DamageType.PercentDamage => "百分比伤害",
                DamageType.Dispel => "驱散",
                DamageType.Cleanse => "净化",
                _ => "未知"
            };
        }
    }
}
