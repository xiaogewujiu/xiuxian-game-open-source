using XXX.Entity;

namespace XXX.Application.Services
{
    /// <summary>
    /// 锻造职业规则运行时目录。
    /// </summary>
    internal static class ForgeProfessionRuleRuntimeCatalog
    {
        private static readonly Dictionary<int, ForgeProfessionLevelConfigEntity> LevelConfigs = new();
        private static ForgeProfessionRuleConfigEntity? _ruleConfig;

        public static void ReplaceConfigs(
            IEnumerable<ForgeProfessionLevelConfigEntity> levelConfigs,
            ForgeProfessionRuleConfigEntity? ruleConfig)
        {
            LevelConfigs.Clear();
            foreach (var config in levelConfigs.Where(item => item != null && item.IsEnabled).OrderBy(item => item.Level))
            {
                LevelConfigs[config.Level] = config;
            }

            _ruleConfig = ruleConfig;
        }

        public static void Clear()
        {
            LevelConfigs.Clear();
            _ruleConfig = null;
        }

        public static int GetNextLevelExp(int currentLevel)
        {
            var safeLevel = Math.Clamp(currentLevel, 1, 50);
            if (LevelConfigs.TryGetValue(safeLevel, out var config))
            {
                return Math.Max(1, config.NextLevelExp);
            }

            throw new InvalidOperationException(
                $"Forge profession level config {safeLevel} is missing. Please refresh the forge domain first.");
        }

        public static int CalculateProfessionLevel(int totalExp, int levelCap)
        {
            var safeCap = Math.Clamp(levelCap, 1, 50);
            var remainingExp = Math.Max(0, totalExp);
            var level = 1;

            while (level < safeCap)
            {
                var nextLevelExp = GetNextLevelExp(level);
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
            var safeLevel = Math.Clamp(currentLevel, 1, 50);
            var remainingExp = Math.Max(0, totalExp);

            for (var level = 1; level < safeLevel; level++)
            {
                remainingExp -= GetNextLevelExp(level);
                if (remainingExp <= 0)
                {
                    return 0;
                }
            }

            return remainingExp;
        }

        public static int GetSuccessBonus(int professionLevel, int requiredLevel)
        {
            var config = ResolveRuleConfig();
            return Math.Min(config.MaxSuccessBonus, Math.Max(0, professionLevel - requiredLevel) * Math.Max(0, config.SuccessBonusPerOverLevel));
        }

        public static int GetProfessionExpGain(int requiredLevel, bool success, int quantity = 1)
        {
            var config = ResolveRuleConfig();
            var safeRequiredLevel = Math.Clamp(requiredLevel, 1, 50);
            var safeQuantity = Math.Max(1, quantity);
            var singleGain = success
                ? config.SuccessExpBase + safeRequiredLevel * config.SuccessExpPerRequiredLevel
                : config.FailureExpBase + safeRequiredLevel * config.FailureExpPerRequiredLevel;
            return Math.Max(0, singleGain) * safeQuantity;
        }

        private static ForgeProfessionRuleConfigEntity ResolveRuleConfig()
        {
            return _ruleConfig
                ?? throw new InvalidOperationException(
                    "Forge profession rule config has not been loaded. Please refresh the forge domain first.");
        }
    }
}
