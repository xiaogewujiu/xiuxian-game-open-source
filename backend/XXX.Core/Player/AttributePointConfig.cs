using XXX.Entity;

namespace XXX.Player
{
    /// <summary>
    /// 单个可分配属性的定义。
    /// </summary>
    public sealed record AttributePointDefinition(
        string Profession,
        string Key,
        string Name,
        AttributeType AttributeType,
        int BonusPerPoint,
        int PointsPerBonus);

    /// <summary>
    /// 等级区间内每级可获得的属性点配置。
    /// </summary>
    public sealed record AttributePointLevelRange(
        int LevelStart,
        int LevelEnd,
        int PointsGained);

    /// <summary>
    /// 属性点配置。
    /// </summary>
    public static class AttributePointConfig
    {
        private static readonly IReadOnlyList<AttributePointDefinition> LegacyDefinitions = BuildLegacyDefinitions();

        private static readonly IReadOnlyList<AttributePointLevelRange> LegacyLevelRanges = new[]
        {
            new AttributePointLevelRange(2, 20, 2),
            new AttributePointLevelRange(21, 30, 3),
            new AttributePointLevelRange(31, LevelConfig.MaxLevel, 4)
        };

        private static IReadOnlyList<AttributePointDefinition> Definitions { get; set; } = [];

        private static IReadOnlyList<AttributePointLevelRange> LevelRanges { get; set; } = [];

        public static IReadOnlyList<AttributePointDefinition> GetLegacyDefinitions()
        {
            return LegacyDefinitions;
        }

        public static IReadOnlyList<AttributePointLevelRange> GetLegacyLevelRanges()
        {
            return LegacyLevelRanges;
        }

        /// <summary>
        /// 获取全部可分配属性定义。
        /// </summary>
        public static IReadOnlyList<AttributePointDefinition> GetDefinitions()
        {
            EnsureRuntimeEntriesLoaded();
            return Definitions;
        }

        /// <summary>
        /// 获取指定职业的可分配属性定义。
        /// </summary>
        public static IReadOnlyList<AttributePointDefinition> GetDefinitions(string? profession)
        {
            EnsureRuntimeEntriesLoaded();
            var normalizedProfession = PlayerProfessionCatalog.Normalize(profession);
            return Definitions
                .Where(item => item.Profession == normalizedProfession)
                .OrderBy(item => item.AttributeType)
                .ToList();
        }

        /// <summary>
        /// 按等级计算本级可获得的属性点。
        /// </summary>
        public static int GetPointsForLevel(int level)
        {
            if (level <= 1)
            {
                return 0;
            }

            EnsureRuntimeEntriesLoaded();
            var normalizedLevel = Math.Min(LevelConfig.MaxLevel, Math.Max(LevelConfig.StartLevel, level));
            var range = LevelRanges.FirstOrDefault(item => normalizedLevel >= item.LevelStart && normalizedLevel <= item.LevelEnd);
            if (range == null)
            {
                throw new InvalidOperationException(
                    $"Attribute point level range is missing for level {normalizedLevel}. Please run startup seed sync before using growth systems.");
            }

            return Math.Max(0, range.PointsGained);
        }

        /// <summary>
        /// 统计角色达到指定等级后累计获得的总属性点。
        /// </summary>
        public static int GetTotalEarnedPoints(int level)
        {
            var total = 0;
            for (var currentLevel = 2; currentLevel <= Math.Max(1, level); currentLevel++)
            {
                total += GetPointsForLevel(currentLevel);
            }

            return total;
        }

        /// <summary>
        /// 统一标准化属性键名。
        /// </summary>
        public static string NormalizeKey(string attributeKey)
        {
            return (attributeKey ?? string.Empty).Trim().ToLowerInvariant();
        }

        /// <summary>
        /// 根据键名查找属性定义。
        /// 默认按战职业口径查找，兼容旧调用。
        /// </summary>
        public static bool TryResolveDefinition(string key, out AttributePointDefinition? definition)
        {
            return TryResolveDefinition(PlayerProfessionCatalog.Warrior, key, out definition);
        }

        /// <summary>
        /// 根据职业和键名查找属性定义。
        /// </summary>
        public static bool TryResolveDefinition(string? profession, string key, out AttributePointDefinition? definition)
        {
            EnsureRuntimeEntriesLoaded();
            var normalizedProfession = PlayerProfessionCatalog.Normalize(profession);
            var normalizedKey = NormalizeKey(key);
            definition = Definitions.FirstOrDefault(item => item.Profession == normalizedProfession && item.Key == normalizedKey);
            return definition != null;
        }

        /// <summary>
        /// 根据底层属性类型查找属性点定义。
        /// 默认按战职业口径查找，兼容旧调用。
        /// </summary>
        public static bool TryResolveDefinition(AttributeType attributeType, out AttributePointDefinition? definition)
        {
            return TryResolveDefinition(PlayerProfessionCatalog.Warrior, attributeType, out definition);
        }

        /// <summary>
        /// 根据职业和底层属性类型查找属性点定义。
        /// </summary>
        public static bool TryResolveDefinition(string? profession, AttributeType attributeType, out AttributePointDefinition? definition)
        {
            EnsureRuntimeEntriesLoaded();
            var normalizedProfession = PlayerProfessionCatalog.Normalize(profession);
            definition = Definitions.FirstOrDefault(item => item.Profession == normalizedProfession && item.AttributeType == attributeType);
            return definition != null;
        }

        /// <summary>
        /// 判断某个属性类型是否属于属性点系统管理范围。
        /// </summary>
        public static bool IsAttributePointType(AttributeType attributeType)
        {
            EnsureRuntimeEntriesLoaded();
            return Definitions.Any(item => item.AttributeType == attributeType);
        }

        /// <summary>
        /// 按整数规则把分配点数换算为属性加成值。
        /// 普通属性每点触发一次；速度等属性可以累计若干点后触发一次。
        /// </summary>
        public static int GetBonusValue(int allocatedPoints, AttributePointDefinition definition)
        {
            if (allocatedPoints <= 0 || definition.BonusPerPoint <= 0 || definition.PointsPerBonus <= 0)
            {
                return 0;
            }

            return (allocatedPoints / definition.PointsPerBonus) * definition.BonusPerPoint;
        }

        /// <summary>
        /// 从数据库配置刷新运行时属性点规则。
        /// </summary>
        public static void ReplaceEntries(IEnumerable<AttributePointConfigEntity> entries)
        {
            var items = entries?
                .Where(item => item != null && item.IsEnabled)
                .ToList();
            if (items == null || items.Count == 0)
            {
                ClearRuntimeEntries();
                return;
            }

            var definitions = items
                .Where(item =>
                    !string.IsNullOrWhiteSpace(item.Key) &&
                    Enum.IsDefined(typeof(AttributeType), item.AttributeType) &&
                    PlayerProfessionCatalog.IsPlayable(item.Profession))
                .GroupBy(item => new
                {
                    Profession = PlayerProfessionCatalog.Normalize(item.Profession),
                    Key = NormalizeKey(item.Key)
                })
                .Select(group =>
                {
                    var first = group
                        .OrderByDescending(item => item.LastUpdateTime)
                        .ThenBy(item => item.ConfigId, StringComparer.OrdinalIgnoreCase)
                        .First();
                    return new AttributePointDefinition(
                        PlayerProfessionCatalog.Normalize(first.Profession),
                        NormalizeKey(first.Key),
                        string.IsNullOrWhiteSpace(first.Name) ? NormalizeKey(first.Key) : first.Name.Trim(),
                        (AttributeType)first.AttributeType,
                        Math.Max(0, first.BonusPerPoint),
                        Math.Max(1, first.PointsPerBonus));
                })
                .OrderBy(item => item.Profession)
                .ThenBy(item => item.AttributeType)
                .ToList();

            var levelRanges = items
                .GroupBy(item => new { item.LevelStart, item.LevelEnd, item.PointsGained })
                .Select(group => new AttributePointLevelRange(
                    group.Key.LevelStart,
                    group.Key.LevelEnd,
                    group.Key.PointsGained))
                .OrderBy(item => item.LevelStart)
                .ThenBy(item => item.LevelEnd)
                .ToList();

            Definitions = definitions.Count == 0 ? [] : definitions;
            LevelRanges = levelRanges.Count == 0 ? [] : levelRanges;
        }

        /// <summary>
        /// 清空数据库运行时规则并回退到内置配置。
        /// </summary>
        public static void ClearRuntimeEntries()
        {
            Definitions = [];
            LevelRanges = [];
        }

        private static void EnsureRuntimeEntriesLoaded()
        {
            if (Definitions.Count == 0 || LevelRanges.Count == 0)
            {
                throw new InvalidOperationException(
                    "Attribute point runtime config is missing. Please run startup seed sync before using growth systems.");
            }
        }

        private static IReadOnlyList<AttributePointDefinition> BuildLegacyDefinitions()
        {
            var entries = new List<AttributePointDefinition>();

            entries.AddRange(BuildProfessionDefinitions(
                PlayerProfessionCatalog.Warrior,
                20, 6, 2, 1, 2, 1, 1));
            entries.AddRange(BuildProfessionDefinitions(
                PlayerProfessionCatalog.Mage,
                16, 10, 1, 2, 1, 2, 1));
            entries.AddRange(BuildProfessionDefinitions(
                PlayerProfessionCatalog.Body,
                30, 5, 2, 1, 3, 2, 1));

            return entries;
        }

        private static IEnumerable<AttributePointDefinition> BuildProfessionDefinitions(
            string profession,
            int hp,
            int mp,
            int physicalAttack,
            int magicAttack,
            int physicalDefense,
            int magicDefense,
            int speed)
        {
            return
            [
                new AttributePointDefinition(profession, "type1", "血量", AttributeType.Type1, hp, 1),
                new AttributePointDefinition(profession, "type2", "蓝量", AttributeType.Type2, mp, 1),
                new AttributePointDefinition(profession, "type3", "物攻", AttributeType.Type3, physicalAttack, 1),
                new AttributePointDefinition(profession, "type4", "法攻", AttributeType.Type4, magicAttack, 1),
                new AttributePointDefinition(profession, "type5", "物防", AttributeType.Type5, physicalDefense, 1),
                new AttributePointDefinition(profession, "type6", "法防", AttributeType.Type6, magicDefense, 1),
                new AttributePointDefinition(profession, "type7", "速度", AttributeType.Type7, speed, profession == PlayerProfessionCatalog.Body ? 12 : 10)
            ];
        }
    }
}
