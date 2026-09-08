using System.Text.Json;
using XXX.Application.DTOs;
using XXX.Entity;

namespace XXX.Application.Services
{
    /// <summary>
    /// 五行聚灵阵相关的统一成长与加成规则。
    /// </summary>
    internal static class FiveElementProgressionRules
    {
        private const int MaxArrayLevel = 50;
        private const int ElementLevelPerArrayLevel = 10;
        private const int MaxElementLevel = MaxArrayLevel * ElementLevelPerArrayLevel;
        private static readonly Dictionary<int, FiveElementLevelConfigEntity> LevelConfigs = new();
        private static readonly Dictionary<string, FiveElementBranchUpgradeConfigEntity> BranchConfigs = new(StringComparer.OrdinalIgnoreCase);
        private static readonly List<FiveElementBranchRuleRangeEntity> BranchRangeConfigs = new();

        private static readonly HashSet<string> AllowedAttributeTypes =
            Enumerable.Range(1, 15)
                .Select(index => $"Type{index}")
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> ElementTypes =
            ["metal", "wood", "water", "fire", "earth"];

        /// <summary>
        /// 用数据库区间规则替换运行时缓存，并在替换前完成完整性校验。
        /// </summary>
        public static void ReplaceConfigs(
            IEnumerable<FiveElementLevelConfigEntity> levelConfigs,
            IEnumerable<FiveElementBranchUpgradeConfigEntity> branchConfigs,
            IEnumerable<FiveElementBranchRuleRangeEntity>? branchRangeConfigs = null)
        {
            var nextLevelConfigs = levelConfigs
                .Where(item => item != null && item.IsEnabled)
                .OrderBy(item => item.ArrayLevel)
                .ToList();
            var nextBranchConfigs = branchConfigs
                .Where(item => item != null && item.IsEnabled)
                .OrderBy(item => item.ElementType)
                .ThenBy(item => item.TargetLevel)
                .ToDictionary(item => BuildBranchKey(item.ElementType, item.TargetLevel), StringComparer.OrdinalIgnoreCase);
            var nextBranchRangeConfigs = (branchRangeConfigs ?? [])
                .Where(item => item != null && item.IsEnabled)
                .OrderBy(item => item.ElementType)
                .ThenBy(item => item.MinLevel)
                .ToList();

            ValidateBranchRangeConfigs(nextBranchRangeConfigs);

            LevelConfigs.Clear();
            foreach (var config in nextLevelConfigs)
            {
                LevelConfigs[config.ArrayLevel] = config;
            }

            BranchConfigs.Clear();
            foreach (var pair in nextBranchConfigs)
            {
                BranchConfigs[pair.Key] = pair.Value;
            }

            BranchRangeConfigs.Clear();
            BranchRangeConfigs.AddRange(nextBranchRangeConfigs);
        }

        /// <summary>
        /// 校验五行区间规则必须覆盖五种元素、覆盖 1～500 且不能重叠或断档。
        /// </summary>
        public static void ValidateBranchRangeConfigs(IEnumerable<FiveElementBranchRuleRangeEntity> configs)
        {
            var enabledConfigs = configs.Where(item => item != null && item.IsEnabled).ToList();
            foreach (var item in enabledConfigs)
            {
                var rawElementType = (item.ElementType ?? string.Empty).Trim().ToLowerInvariant();
                if (!ElementTypes.Contains(rawElementType))
                {
                    throw new InvalidOperationException($"五行类型无效：{item.ElementType}。");
                }
            }

            foreach (var elementType in ElementTypes)
            {
                var elementConfigs = enabledConfigs
                    .Where(item => string.Equals(item.ElementType?.Trim(), elementType, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(item => item.MinLevel)
                    .ToList();
                if (elementConfigs.Count == 0)
                {
                    throw new InvalidOperationException($"五行区间规则缺失：{elementType}。");
                }

                var expectedMinLevel = 1;
                foreach (var config in elementConfigs)
                {
                    if (config.MinLevel < 1 || config.MaxLevel > MaxElementLevel || config.MinLevel > config.MaxLevel)
                    {
                        throw new InvalidOperationException($"五行区间边界无效：{elementType} {config.MinLevel}-{config.MaxLevel}。");
                    }

                    if (!AllowedAttributeTypes.Contains(config.AttributeType?.Trim() ?? string.Empty))
                    {
                        throw new InvalidOperationException($"五行属性类型无效：{elementType} {config.AttributeType}。");
                    }

                    if (config.BonusPerLevel < 0)
                    {
                        throw new InvalidOperationException($"五行每级加成不能为负数：{elementType}。");
                    }

                    ValidateMaterialsJson(config.MaterialsJson, elementType);
                    if (config.MinLevel < expectedMinLevel)
                    {
                        throw new InvalidOperationException($"五行区间重叠：{elementType} {config.MinLevel}-{config.MaxLevel}。");
                    }

                    if (config.MinLevel > expectedMinLevel)
                    {
                        throw new InvalidOperationException($"五行区间断档：{elementType} 缺少等级 {expectedMinLevel}。");
                    }

                    expectedMinLevel = config.MaxLevel + 1;
                }

                if (expectedMinLevel != MaxElementLevel + 1)
                {
                    throw new InvalidOperationException($"五行区间未覆盖到 500：{elementType}。");
                }
            }
        }

        public static void ClearRuntimeConfigs()
        {
            LevelConfigs.Clear();
            BranchConfigs.Clear();
            BranchRangeConfigs.Clear();
        }

        /// <summary>
        /// 获取聚灵阵等级允许的五行最高等级。
        /// </summary>
        public static int GetElementMaxLevel(int arrayLevel)
        {
            return Math.Clamp(arrayLevel, 1, MaxArrayLevel) * ElementLevelPerArrayLevel;
        }

        /// <summary>
        /// 获取指定五行的默认属性类型。
        /// </summary>
        public static string GetDefaultAttributeType(string elementType)
        {
            return NormalizeElementType(elementType) switch
            {
                "metal" => nameof(AttributeType.Type3),
                "wood" => nameof(AttributeType.Type1),
                "water" => nameof(AttributeType.Type6),
                "fire" => nameof(AttributeType.Type4),
                "earth" => nameof(AttributeType.Type5),
                _ => nameof(AttributeType.Type3)
            };
        }

        /// <summary>
        /// 获取角色属性中文名称。
        /// </summary>
        public static string GetAttributeName(string attributeType)
        {
            return attributeType.Trim() switch
            {
                nameof(AttributeType.Type1) => "最大生命",
                nameof(AttributeType.Type2) => "最大灵力",
                nameof(AttributeType.Type3) => "物理攻击",
                nameof(AttributeType.Type4) => "法术攻击",
                nameof(AttributeType.Type5) => "物理防御",
                nameof(AttributeType.Type6) => "法术防御",
                nameof(AttributeType.Type7) => "速度",
                nameof(AttributeType.Type8) => "命中率",
                nameof(AttributeType.Type9) => "闪避率",
                nameof(AttributeType.Type10) => "暴击率",
                nameof(AttributeType.Type11) => "暴击伤害",
                nameof(AttributeType.Type12) => "连击率",
                nameof(AttributeType.Type13) => "反击率",
                nameof(AttributeType.Type14) => "破甲率",
                nameof(AttributeType.Type15) => "额外伤害",
                _ => attributeType
            };
        }

        /// <summary>
        /// 查找目标等级对应的区间规则。
        /// </summary>
        public static FiveElementBranchRuleRangeEntity ResolveBranchRange(string elementType, int targetLevel)
        {
            var normalized = NormalizeElementType(elementType);
            var safeLevel = Math.Clamp(targetLevel, 1, MaxElementLevel);
            return BranchRangeConfigs
                .Where(item => string.Equals(item.ElementType, normalized, StringComparison.OrdinalIgnoreCase) && item.MinLevel <= safeLevel && item.MaxLevel >= safeLevel)
                .OrderBy(item => item.MinLevel)
                .FirstOrDefault()
                ?? throw new InvalidOperationException($"Five-element range rule is missing for {normalized}:{safeLevel}.");
        }

        /// <summary>
        /// 计算指定五行等级的累计固定属性加成。
        /// </summary>
        public static int GetElementCurrentBonus(string elementType, int currentLevel)
        {
            var normalized = NormalizeElementType(elementType);
            var safeLevel = Math.Clamp(currentLevel, 1, MaxElementLevel);
            if (safeLevel <= 1)
            {
                return 0;
            }

            var total = 0L;
            foreach (var range in BranchRangeConfigs.Where(item => string.Equals(item.ElementType, normalized, StringComparison.OrdinalIgnoreCase)).OrderBy(item => item.MinLevel))
            {
                var start = Math.Max(2, range.MinLevel);
                var end = Math.Min(safeLevel, range.MaxLevel);
                if (end >= start) total += (long)(end - start + 1) * range.BonusPerLevel;
            }
            return total > int.MaxValue ? int.MaxValue : (int)Math.Max(0, total);
        }

        public static int GetArrayUpgradeCost(int currentLevel)
        {
            var nextLevel = Math.Clamp(currentLevel + 1, 1, MaxArrayLevel);
            var config = ResolveLevelConfig(nextLevel);
            return config.UpgradeGoldCost > int.MaxValue
                ? int.MaxValue
                : (int)Math.Max(0, config.UpgradeGoldCost);
        }

        public static int GetSpiritFieldYieldBonusPercent(int arrayLevel)
        {
            var safeLevel = Math.Clamp(arrayLevel, 1, MaxArrayLevel);
            return ResolveLevelConfig(safeLevel).SpiritFieldYieldBonusPercent;
        }

        public static int GetBattleExpBonusPercent(int arrayLevel)
        {
            var safeLevel = Math.Clamp(arrayLevel, 1, MaxArrayLevel);
            return ResolveLevelConfig(safeLevel).BattleExpBonusPercent;
        }

        public static int GetProfessionLevelCap(int arrayLevel)
        {
            var safeLevel = Math.Clamp(arrayLevel, 1, MaxArrayLevel);
            return ResolveLevelConfig(safeLevel).ProfessionLevelCap;
        }

        public static int GetProfessionNextLevelExp(int currentLevel)
        {
            var safeLevel = Math.Clamp(currentLevel, 1, MaxArrayLevel);
            return 80 + (safeLevel - 1) * 40;
        }

        public static int CalculateProfessionLevel(int totalExp, int levelCap)
        {
            var safeCap = Math.Clamp(levelCap, 1, MaxArrayLevel);
            var remainingExp = Math.Max(0, totalExp);
            var level = 1;

            while (level < safeCap)
            {
                var nextLevelExp = GetProfessionNextLevelExp(level);
                if (remainingExp < nextLevelExp)
                {
                    break;
                }

                remainingExp -= nextLevelExp;
                level++;
            }

            return level;
        }

        public static int GetCurrentLevelProgressExp(int totalExp, int currentLevel)
        {
            var safeLevel = Math.Clamp(currentLevel, 1, MaxArrayLevel);
            var remainingExp = Math.Max(0, totalExp);

            for (var level = 1; level < safeLevel; level++)
            {
                remainingExp -= GetProfessionNextLevelExp(level);
                if (remainingExp <= 0)
                {
                    return 0;
                }
            }

            return remainingExp;
        }

        public static int GetProfessionSuccessBonus(int professionLevel, int requiredLevel)
        {
            return Math.Min(15, Math.Max(0, professionLevel - requiredLevel));
        }

        public static int GetProfessionExpGain(int requiredLevel, bool success, int quantity = 1)
        {
            var safeRequiredLevel = Math.Clamp(requiredLevel, 1, MaxArrayLevel);
            var safeQuantity = Math.Max(1, quantity);
            var singleGain = success
                ? 12 + safeRequiredLevel * 4
                : 6 + safeRequiredLevel * 2;

            return singleGain * safeQuantity;
        }

        public static int ApplyPercentageBonus(int baseValue, int bonusPercent)
        {
            if (baseValue <= 0 || bonusPercent <= 0)
            {
                return baseValue;
            }

            return (int)Math.Ceiling(baseValue * (1d + bonusPercent / 100d));
        }

        public static FiveElementUpgradeCostDto BuildArrayUpgradeCost(int currentArrayLevel, Func<string, string>? itemNameResolver = null)
        {
            var targetLevel = Math.Clamp(currentArrayLevel + 1, 1, MaxArrayLevel);
            var config = ResolveLevelConfig(targetLevel);

            return new FiveElementUpgradeCostDto
            {
                ElementType = "array",
                GoldCost = Math.Max(0L, config.UpgradeGoldCost),
                SpiritStoneCost = Math.Max(0L, config.UpgradeSpiritStoneCost),
                Materials = ParseMaterials(config.UpgradeMaterialsJson, itemNameResolver)
            };
        }

        public static FiveElementUpgradeCostDto BuildElementUpgradeCost(string elementType, int currentLevel, Func<string, string>? itemNameResolver = null)
        {
            var normalizedElementType = NormalizeElementType(elementType);
            var targetLevel = Math.Clamp(currentLevel + 1, 1, MaxElementLevel);
            if (BranchRangeConfigs.Count > 0)
            {
                var range = ResolveBranchRange(normalizedElementType, targetLevel);
                return new FiveElementUpgradeCostDto
                {
                    ElementType = normalizedElementType,
                    GoldCost = Math.Max(0L, range.GoldCost),
                    SpiritStoneCost = Math.Max(0L, range.SpiritStoneCost),
                    Materials = ParseMaterials(range.MaterialsJson, itemNameResolver)
                };
            }

            var key = BuildBranchKey(normalizedElementType, targetLevel);
            if (!BranchConfigs.TryGetValue(key, out var config))
            {
                throw new InvalidOperationException(
                    $"Five-element branch upgrade config is missing for {normalizedElementType}:{targetLevel}. Please run startup seed sync or refresh five-element rules.");
            }

            return new FiveElementUpgradeCostDto
            {
                ElementType = normalizedElementType,
                GoldCost = Math.Max(0L, config.GoldCost),
                SpiritStoneCost = Math.Max(0L, config.SpiritStoneCost),
                Materials = ParseMaterials(config.MaterialsJson, itemNameResolver)
            };
        }

        private static FiveElementLevelConfigEntity ResolveLevelConfig(int arrayLevel)
        {
            var safeLevel = Math.Clamp(arrayLevel, 1, MaxArrayLevel);
            if (LevelConfigs.TryGetValue(safeLevel, out var config))
            {
                return config;
            }

            throw new InvalidOperationException(
                $"Five-element array level config is missing for level {safeLevel}. Please run startup seed sync or refresh five-element rules.");
        }

        /// <summary>
        /// 校验规则材料 JSON 可解析且每个材料数量为正数。
        /// </summary>
        private static void ValidateMaterialsJson(string? json, string elementType)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidOperationException($"五行材料配置不能为空：{elementType}。");
            }

            try
            {
                var parsed = JsonSerializer.Deserialize<List<FiveElementRuleMaterialItem>>(json);
                if (parsed == null || parsed.Count == 0 || parsed.Any(item => string.IsNullOrWhiteSpace(item.ItemId) || item.Count <= 0))
                {
                    throw new InvalidOperationException($"五行材料配置无效：{elementType}。");
                }
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"五行材料 JSON 无法解析：{elementType}。", ex);
            }
        }

        private static List<FiveElementUpgradeMaterialDto> ParseMaterials(string? json, Func<string, string>? itemNameResolver)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return [];
            }

            try
            {
                var parsed = JsonSerializer.Deserialize<List<FiveElementRuleMaterialItem>>(json) ?? [];
                return parsed
                    .Where(item => !string.IsNullOrWhiteSpace(item.ItemId) && item.Count > 0)
                    .Select(item => new FiveElementUpgradeMaterialDto
                    {
                        ItemId = item.ItemId,
                        ItemName = itemNameResolver?.Invoke(item.ItemId) ?? item.ItemId,
                        Count = item.Count
                    })
                    .ToList();
            }
            catch
            {
                return [];
            }
        }

        private static string NormalizeElementType(string? elementType)
        {
            return (elementType ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "metal" => "metal",
                "wood" => "wood",
                "water" => "water",
                "fire" => "fire",
                "earth" => "earth",
                _ => "metal"
            };
        }

        private static string BuildBranchKey(string elementType, int targetLevel)
        {
            return $"{NormalizeElementType(elementType)}:{Math.Clamp(targetLevel, 1, MaxArrayLevel)}";
        }

        private sealed record FiveElementRuleMaterialItem(string ItemId, int Count);
    }
}
