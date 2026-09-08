namespace XXX.Application.DTOs
{
    /// <summary>
    /// 灵田系统规则详情。
    /// </summary>
    public class AdminSpiritFieldSystemRuleDto
    {
        /// <summary>规则配置编号。</summary>
        public string ConfigId { get; set; } = string.Empty;

        /// <summary>默认灵田等级。</summary>
        public int DefaultFieldLevel { get; set; }

        /// <summary>默认解锁地块数。</summary>
        public int DefaultUnlockedPlots { get; set; }

        /// <summary>默认最大地块数。</summary>
        public int DefaultMaxPlots { get; set; }

        /// <summary>默认仓库存储容量。</summary>
        public int DefaultInventoryCapacity { get; set; }

        /// <summary>地块每升 1 级所需金币。</summary>
        public long PlotUpgradeGoldPerLevel { get; set; }

        /// <summary>地块每升 1 级所需灵石。</summary>
        public long PlotUpgradeSpiritStonePerLevel { get; set; }

        /// <summary>地块每升 1 级增加的产量百分比。</summary>
        public int PlotUpgradeYieldBonusPerLevel { get; set; }

        /// <summary>当前规则是否启用。</summary>
        public bool IsEnabled { get; set; }

        /// <summary>是否为内置种子规则。</summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>内置种子键。</summary>
        public string? SeedKey { get; set; }

        /// <summary>内置版本号。</summary>
        public string? BuiltInVersion { get; set; }

        /// <summary>最近一次编辑时间。</summary>
        public DateTime? LastUpdateTime { get; set; }
    }

    /// <summary>
    /// 灵田催熟道具规则列表项。
    /// </summary>
    public class AdminSpiritFieldSpeedUpItemRuleDto
    {
        /// <summary>加速道具编号。</summary>
        public string ItemId { get; set; } = string.Empty;

        /// <summary>单次加速秒数。</summary>
        public int SpeedUpSeconds { get; set; }

        /// <summary>后台展示排序。</summary>
        public int SortOrder { get; set; }

        /// <summary>当前规则是否启用。</summary>
        public bool IsEnabled { get; set; }

        /// <summary>是否为内置种子规则。</summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>内置种子键。</summary>
        public string? SeedKey { get; set; }

        /// <summary>内置版本号。</summary>
        public string? BuiltInVersion { get; set; }

        /// <summary>最近一次编辑时间。</summary>
        public DateTime? LastUpdateTime { get; set; }
    }
}
