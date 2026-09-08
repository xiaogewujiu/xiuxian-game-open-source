using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 灵田系统规则配置。
    /// </summary>
    [SugarTable("SpiritFieldSystemConfigs")]
    public class SpiritFieldSystemConfigEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 50)]
        public string ConfigId { get; set; } = string.Empty;

        [SugarColumn]
        public int DefaultFieldLevel { get; set; } = 1;

        [SugarColumn]
        public int DefaultUnlockedPlots { get; set; } = 3;

        [SugarColumn]
        public int DefaultMaxPlots { get; set; } = 9;

        [SugarColumn]
        public int DefaultInventoryCapacity { get; set; } = 100;

        [SugarColumn]
        public long PlotUpgradeGoldPerLevel { get; set; } = 1000;

        [SugarColumn]
        public long PlotUpgradeSpiritStonePerLevel { get; set; } = 10;

        [SugarColumn]
        public int PlotUpgradeYieldBonusPerLevel { get; set; } = 5;

        [SugarColumn]
        public bool IsEnabled { get; set; } = true;

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? SeedKey { get; set; }

        [SugarColumn]
        public bool IsBuiltIn { get; set; } = false;

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? BuiltInVersion { get; set; }

        [SugarColumn]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 灵田催熟道具规则配置。
    /// </summary>
    [SugarTable("SpiritFieldSpeedUpItemConfigs")]
    public class SpiritFieldSpeedUpItemConfigEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 50)]
        public string ItemId { get; set; } = string.Empty;

        [SugarColumn]
        public int SpeedUpSeconds { get; set; } = 3600;

        [SugarColumn]
        public int SortOrder { get; set; } = 0;

        [SugarColumn]
        public bool IsEnabled { get; set; } = true;

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? SeedKey { get; set; }

        [SugarColumn]
        public bool IsBuiltIn { get; set; } = false;

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? BuiltInVersion { get; set; }

        [SugarColumn]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }
}
