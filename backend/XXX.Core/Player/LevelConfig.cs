using XXX.Entity;

namespace XXX.Player
{
    /// <summary>
    /// 等级配置
    /// </summary>
    public static class LevelConfig
    {
        private static readonly IReadOnlyList<PlayerLevelConfigEntity> LegacyEntries = BuildLegacyEntries();

        public const int MaxLevel = 100;

        public const int StartLevel = 1;

        public const long StartExp = 0;

        private static IReadOnlyList<PlayerLevelConfigEntity> Entries { get; set; } = [];

        public static IReadOnlyList<PlayerLevelConfigEntity> GetLegacyEntries()
        {
            return LegacyEntries;
        }

        /// <summary>
        /// 获取指定等级升级所需经验。
        /// </summary>
        public static long GetRequiredExp(int level)
        {
            return GetEntry(level).RequiredExp;
        }

        /// <summary>
        /// 获取指定等级的基础生命值。
        /// </summary>
        public static int GetBaseHp(int level)
        {
            return GetEntry(level).BaseHp;
        }

        /// <summary>
        /// 获取指定等级的基础法力值。
        /// </summary>
        public static int GetBaseMp(int level)
        {
            return GetEntry(level).BaseMp;
        }

        /// <summary>
        /// 获取指定等级的基础物理攻击。
        /// </summary>
        public static int GetBasePhysicalAttack(int level)
        {
            return GetEntry(level).BasePhysicalAttack;
        }

        /// <summary>
        /// 获取指定等级的基础法术攻击。
        /// </summary>
        public static int GetBaseMagicAttack(int level)
        {
            return GetEntry(level).BaseMagicAttack;
        }

        /// <summary>
        /// 获取指定等级的基础物理防御。
        /// </summary>
        public static int GetBasePhysicalDefense(int level)
        {
            return GetEntry(level).BasePhysicalDefense;
        }

        /// <summary>
        /// 获取指定等级的基础法术防御。
        /// </summary>
        public static int GetBaseMagicDefense(int level)
        {
            return GetEntry(level).BaseMagicDefense;
        }

        /// <summary>
        /// 获取指定等级的基础速度。
        /// </summary>
        public static int GetBaseSpeed(int level)
        {
            return GetEntry(level).BaseSpeed;
        }

        /// <summary>
        /// 计算指定等级相对初始等级的核心成长增量。
        /// </summary>
        public static (int hpBonus, int attackBonus, int defenseBonus) GetLevelBonus(int level)
        {
            var normalizedLevel = Math.Max(StartLevel, level);
            return (
                GetBaseHp(normalizedLevel) - GetBaseHp(StartLevel),
                GetBasePhysicalAttack(normalizedLevel) - GetBasePhysicalAttack(StartLevel),
                GetBasePhysicalDefense(normalizedLevel) - GetBasePhysicalDefense(StartLevel));
        }

        /// <summary>
        /// 检查等级是否落在合法范围内。
        /// </summary>
        public static bool IsValidLevel(int level)
        {
            return level >= StartLevel && level <= MaxLevel;
        }

        /// <summary>
        /// 判断当前等级是否还能继续升级。
        /// </summary>
        public static bool CanLevelUp(int currentLevel)
        {
            return currentLevel < MaxLevel;
        }

        /// <summary>
        /// 计算本次经验增加后可提升的等级数。
        /// </summary>
        public static int CalculateLevelUps(int currentLevel, long currentExp, long addedExp)
        {
            var levelUps = 0;
            var tempLevel = currentLevel;
            var tempExp = currentExp + addedExp;

            while (tempLevel < MaxLevel)
            {
                var requiredExp = GetRequiredExp(tempLevel);
                if (requiredExp == 0)
                {
                    break;
                }

                if (tempExp < requiredExp)
                {
                    break;
                }

                tempExp -= requiredExp;
                tempLevel++;
                levelUps++;
            }

            return levelUps;
        }

        public static void ReplaceEntries(IEnumerable<PlayerLevelConfigEntity> entries)
        {
            var ordered = entries?
                .Where(item => item != null)
                .OrderBy(item => item.Level)
                .ToList();

            Entries = ordered == null || ordered.Count == 0 ? [] : ordered;
        }

        public static void ClearRuntimeEntries()
        {
            Entries = [];
        }

        private static PlayerLevelConfigEntity GetEntry(int level)
        {
            var normalizedLevel = Math.Clamp(level, StartLevel, MaxLevel);
            return Entries.FirstOrDefault(item => item.Level == normalizedLevel)
                ?? throw new InvalidOperationException(
                    $"Player level config {normalizedLevel} is missing from runtime growth config. Please run startup seed sync before using growth systems.");
        }

        private static IReadOnlyList<PlayerLevelConfigEntity> BuildLegacyEntries()
        {
            var entries = new List<PlayerLevelConfigEntity>(MaxLevel);

            for (var level = StartLevel; level <= MaxLevel; level++)
            {
                var tier = Math.Max(0, level - 1);
                entries.Add(new PlayerLevelConfigEntity
                {
                    Level = level,
                    RequiredExp = level >= MaxLevel ? 0 : 160L + level * 90L + level * level * 8L,
                    BaseHp = 420 + tier * 70 + (int)Math.Round(tier * tier * 0.90d, MidpointRounding.AwayFromZero),
                    BaseMp = 180 + tier * 24 + (int)Math.Round(tier * tier * 0.55d, MidpointRounding.AwayFromZero),
                    BasePhysicalAttack = 42 + tier * 5 + (int)Math.Round(tier * tier * 0.08d, MidpointRounding.AwayFromZero),
                    BaseMagicAttack = 42 + tier * 5 + (int)Math.Round(tier * tier * 0.08d, MidpointRounding.AwayFromZero),
                    BasePhysicalDefense = 20 + tier * 3 + (int)Math.Round(tier * tier * 0.06d, MidpointRounding.AwayFromZero),
                    BaseMagicDefense = 20 + tier * 3 + (int)Math.Round(tier * tier * 0.06d, MidpointRounding.AwayFromZero),
                    BaseSpeed = 18 + tier + tier / 7
                });
            }

            return entries;
        }
    }
}
