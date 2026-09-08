using XXX.Entity;

namespace XXX.Player
{
    /// <summary>
    /// 等级境界配置运行时目录。
    /// 默认使用内置配置，启动后若数据库里有配置会用数据库内容覆盖。
    /// </summary>
    public static class RealmLevelCatalog
    {
        private static readonly IReadOnlyList<RealmLevelConfigEntity> LegacyEntries = BuildDefaultEntries();

        public static IReadOnlyList<RealmLevelConfigEntity> Entries { get; private set; } = [];

        public static IReadOnlyList<RealmLevelConfigEntity> GetLegacyEntries()
        {
            return LegacyEntries;
        }

        public static void ReplaceEntries(IEnumerable<RealmLevelConfigEntity> entries)
        {
            var ordered = entries?
                .Where(entry => entry != null)
                .OrderBy(entry => entry.Level)
                .ToList();

            Entries = ordered == null || ordered.Count == 0 ? [] : ordered;
        }

        public static void ClearRuntimeEntries()
        {
            Entries = [];
        }

        public static RealmLevelConfigEntity Get(int level)
        {
            var normalizedLevel = Math.Clamp(level, 1, LevelConfig.MaxLevel);
            return Entries.FirstOrDefault(entry => entry.Level == normalizedLevel)
                ?? throw new InvalidOperationException(
                    $"Realm level config {normalizedLevel} is missing from runtime growth config. Please run startup seed sync before using growth systems.");
        }

        public static RealmLevelConfigEntity? GetNext(int level)
        {
            var nextLevel = level + 1;
            if (nextLevel > LevelConfig.MaxLevel)
            {
                return null;
            }

            return Get(nextLevel);
        }

        public static int GetAttributeBonusPercent(int level)
        {
            return Get(level).AttributeBonusPercent;
        }

        public static long GetRequiredExp(int level)
        {
            return Get(level).RequiredExp;
        }

        private static IReadOnlyList<RealmLevelConfigEntity> BuildDefaultEntries()
        {
            var realmNames = new[]
            {
                "练气",
                "筑基",
                "金丹",
                "元婴",
                "化神",
                "炼虚",
                "合体",
                "大乘",
                "渡劫",
                "真仙"
            };

            var layerNames = new[] { "一", "二", "三", "四", "五", "六", "七", "八", "九", "十" };
            var entries = new List<RealmLevelConfigEntity>(LevelConfig.MaxLevel);

            for (var level = 1; level <= LevelConfig.MaxLevel; level++)
            {
                var realmIndex = Math.Min(realmNames.Length - 1, (level - 1) / 10);
                var layer = ((level - 1) % 10) + 1;
                var isBreakthroughPoint = level % 10 == 0 && level < LevelConfig.MaxLevel;

                entries.Add(new RealmLevelConfigEntity
                {
                    Level = level,
                    RealmName = realmNames[realmIndex],
                    Alias = $"{realmNames[realmIndex]}{layerNames[layer - 1]}层",
                    RealmOrder = realmIndex + 1,
                    Layer = layer,
                    RequiredExp = BuildRequiredExp(level),
                    AttributeBonusPercent = realmIndex * 5,
                    IsBreakthroughPoint = isBreakthroughPoint,
                    BreakthroughSuccessRate = isBreakthroughPoint
                        ? Math.Max(45, 85 - realmIndex * 4)
                        : 100,
                    BreakthroughExpLossPercent = isBreakthroughPoint
                        ? Math.Min(45, 15 + realmIndex * 3)
                        : 0,
                    BreakthroughMaterials = isBreakthroughPoint
                        ? BuildBreakthroughMaterials(realmIndex + 1)
                        : [],
                    Description = isBreakthroughPoint
                        ? $"达到 {realmNames[realmIndex]}圆满后，需要消耗材料尝试突破至 {realmNames[Math.Min(realmIndex + 1, realmNames.Length - 1)]}。"
                        : $"{realmNames[realmIndex]}阶段修行。"
                });
            }

            return entries;
        }

        private static List<RealmBreakthroughMaterial> BuildBreakthroughMaterials(int realmOrder)
        {
            return realmOrder switch
            {
                1 =>
                [
                    new() { ItemId = "evolution_stone", Count = 2 },
                    new() { ItemId = "spirit_dust", Count = 4 }
                ],
                2 =>
                [
                    new() { ItemId = "evolution_stone", Count = 4 },
                    new() { ItemId = "beast_core", Count = 2 }
                ],
                3 =>
                [
                    new() { ItemId = "spirit_water", Count = 6 },
                    new() { ItemId = "beast_core", Count = 3 }
                ],
                4 =>
                [
                    new() { ItemId = "alchemy_herb", Count = 10 },
                    new() { ItemId = "evolution_stone", Count = 6 }
                ],
                5 =>
                [
                    new() { ItemId = "spirit_dust", Count = 12 },
                    new() { ItemId = "beast_core", Count = 5 }
                ],
                6 =>
                [
                    new() { ItemId = "spirit_water", Count = 12 },
                    new() { ItemId = "evolution_stone", Count = 8 }
                ],
                7 =>
                [
                    new() { ItemId = "alchemy_herb", Count = 18 },
                    new() { ItemId = "beast_core", Count = 7 }
                ],
                8 =>
                [
                    new() { ItemId = "spirit_dust", Count = 20 },
                    new() { ItemId = "spirit_water", Count = 15 }
                ],
                9 =>
                [
                    new() { ItemId = "evolution_stone", Count = 12 },
                    new() { ItemId = "beast_core", Count = 10 }
                ],
                _ => []
            };
        }

        private static long BuildRequiredExp(int level)
        {
            var anchors = new Dictionary<int, long>
            {
                [1] = 1_000,
                [10] = 50_000,
                [20] = 300_000,
                [30] = 1_500_000,
                [40] = 5_000_000,
                [50] = 12_000_000,
                [60] = 25_000_000,
                [70] = 45_000_000,
                [80] = 75_000_000,
                [90] = 120_000_000,
                [99] = 180_000_000,
                [100] = 0
            };

            if (anchors.TryGetValue(level, out var exactValue))
            {
                return exactValue;
            }

            var leftAnchor = anchors.Keys.Where(key => key < level).Max();
            var rightAnchor = anchors.Keys.Where(key => key > level).Min();
            var leftValue = anchors[leftAnchor];
            var rightValue = anchors[rightAnchor];
            var progress = (double)(level - leftAnchor) / (rightAnchor - leftAnchor);

            return (long)Math.Round(leftValue + (rightValue - leftValue) * progress, MidpointRounding.AwayFromZero);
        }
    }
}
