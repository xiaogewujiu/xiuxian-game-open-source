namespace XXX.Dungeon
{
    /// <summary>
    /// 副本目录项。
    /// </summary>
    public sealed class DungeonCatalogEntry
    {
        /// <summary>
        /// 初始化一条副本目录项。
        /// </summary>
        public DungeonCatalogEntry(
            string dungeonId,
            string name,
            string description,
            int recommendedLevel,
            int dailyLimit,
            string normalMapId,
            string fubenMapId,
            int requiredTeamSize)
        {
            DungeonId = dungeonId;
            Name = name;
            Description = description;
            RecommendedLevel = recommendedLevel;
            DailyLimit = dailyLimit;
            NormalMapId = normalMapId;
            FubenMapId = fubenMapId;
            RequiredTeamSize = Math.Max(1, requiredTeamSize);
        }

        public string DungeonId { get; }

        public string Name { get; }

        public string Description { get; }

        public int RecommendedLevel { get; }

        public int DailyLimit { get; }

        public string NormalMapId { get; }

        public string FubenMapId { get; }

        public int RequiredTeamSize { get; }

        public bool SupportsParty => RequiredTeamSize > 1;
    }

    /// <summary>
    /// 统一维护当前版本可玩的副本目录。
    /// </summary>
    public static class DungeonCatalog
    {
        private static readonly IReadOnlyList<DungeonCatalogEntry> LegacyEntries = new[]
        {
            new DungeonCatalogEntry(
                "dungeon_001",
                "新手试炼",
                "单人入门副本，按三层推进，稳定产出第一批副本令牌与宠物材料。",
                5,
                3,
                "map_002",
                "fuben_001_1",
                1),
            new DungeonCatalogEntry(
                "dungeon_002",
                "迷雾林地",
                "三人组队副本，按三层推进，产出二阶装备素材与宠物资源。",
                12,
                3,
                "map_004",
                "fuben_002_1",
                3),
            new DungeonCatalogEntry(
                "dungeon_003",
                "烈焰裂谷",
                "双人进阶副本，按三层推进，奖励显著高于同级普通图。",
                20,
                2,
                "map_006",
                "fuben_003_1",
                2),
            new DungeonCatalogEntry(
                "dungeon_004",
                "幽月禁路",
                "三人高阶副本，按三层推进，作为Lv.30前段的阶段毕业挑战。",
                28,
                2,
                "map_008",
                "fuben_004_1",
                3)
        };

        public static IReadOnlyList<DungeonCatalogEntry> Entries { get; private set; } = Array.Empty<DungeonCatalogEntry>();

        public static bool HasLoadedRuntimeEntries => Entries.Count > 0;

        /// <summary>
        /// 获取内置副本目录。
        /// </summary>
        public static IReadOnlyList<DungeonCatalogEntry> GetLegacyEntries()
        {
            return LegacyEntries;
        }

        /// <summary>
        /// 使用运行时数据替换当前副本目录。
        /// </summary>
        public static void ReplaceEntries(IEnumerable<DungeonCatalogEntry> entries)
        {
            Entries = entries?.ToList() ?? [];
        }

        /// <summary>
        /// 清空当前运行时副本目录。
        /// </summary>
        public static void ClearRuntimeEntries()
        {
            Entries = Array.Empty<DungeonCatalogEntry>();
        }

        /// <summary>
        /// 根据副本编号查找目录项。
        /// </summary>
        public static DungeonCatalogEntry? Get(string? dungeonId)
        {
            return Entries.FirstOrDefault(entry =>
                string.Equals(entry.DungeonId, dungeonId, StringComparison.OrdinalIgnoreCase));
        }
    }
}
