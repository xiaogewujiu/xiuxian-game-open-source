using XXX.Entity;

namespace XXX.Application.Services
{
    /// <summary>
    /// 灵田规则运行时目录。
    /// </summary>
    internal static class SpiritFieldRuleRuntimeCatalog
    {
        private static SpiritFieldSystemConfigEntity? _systemConfig;
        private static readonly Dictionary<string, SpiritFieldSpeedUpItemConfigEntity> SpeedUpItemConfigs = new(StringComparer.OrdinalIgnoreCase);

        public static void ReplaceConfigs(
            SpiritFieldSystemConfigEntity? systemConfig,
            IEnumerable<SpiritFieldSpeedUpItemConfigEntity> speedUpItems)
        {
            _systemConfig = systemConfig;
            SpeedUpItemConfigs.Clear();
            foreach (var item in speedUpItems
                         .Where(item => item != null && item.IsEnabled)
                         .OrderBy(item => item.SortOrder)
                         .ThenBy(item => item.ItemId, StringComparer.OrdinalIgnoreCase))
            {
                SpeedUpItemConfigs[item.ItemId] = item;
            }
        }

        public static void Clear()
        {
            _systemConfig = null;
            SpeedUpItemConfigs.Clear();
        }

        public static int GetDefaultFieldLevel() => ResolveSystemConfig().DefaultFieldLevel;

        public static int GetDefaultUnlockedPlots() => ResolveSystemConfig().DefaultUnlockedPlots;

        public static int GetDefaultMaxPlots() => ResolveSystemConfig().DefaultMaxPlots;

        public static int GetDefaultInventoryCapacity() => ResolveSystemConfig().DefaultInventoryCapacity;

        public static int GetPlotUpgradeYieldBonusPerLevel() => ResolveSystemConfig().PlotUpgradeYieldBonusPerLevel;

        public static long GetPlotUpgradeGoldCost(int plotLevel)
        {
            var safeLevel = Math.Max(1, plotLevel);
            return ResolveSystemConfig().PlotUpgradeGoldPerLevel * safeLevel;
        }

        public static long GetPlotUpgradeSpiritStoneCost(int plotLevel)
        {
            var safeLevel = Math.Max(1, plotLevel);
            return ResolveSystemConfig().PlotUpgradeSpiritStonePerLevel * safeLevel;
        }

        public static bool TryGetSpeedUpSeconds(string itemId, out int seconds)
        {
            if (SpeedUpItemConfigs.TryGetValue((itemId ?? string.Empty).Trim(), out var config))
            {
                seconds = Math.Max(1, config.SpeedUpSeconds);
                return true;
            }

            seconds = 0;
            return false;
        }

        public static IReadOnlyList<string> GetAllowedSpeedUpItemIds()
        {
            return SpeedUpItemConfigs.Keys.OrderBy(item => item, StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static SpiritFieldSystemConfigEntity ResolveSystemConfig()
        {
            return _systemConfig
                ?? throw new InvalidOperationException(
                    "Spirit-field runtime rules have not been loaded. Please refresh the spirit-field domain first.");
        }
    }
}
