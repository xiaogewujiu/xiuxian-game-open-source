using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 聚灵阵主等级规则配置。
    /// </summary>
    [SugarTable("FiveElementLevelConfigs")]
    public class FiveElementLevelConfigEntity
    {
        [SugarColumn(IsPrimaryKey = true)]
        public int ArrayLevel { get; set; }

        [SugarColumn]
        public long UpgradeGoldCost { get; set; }

        [SugarColumn]
        public long UpgradeSpiritStoneCost { get; set; }

        [SugarColumn(Length = 1000, IsNullable = true)]
        public string? UpgradeMaterialsJson { get; set; }

        [SugarColumn]
        public int SpiritFieldYieldBonusPercent { get; set; }

        [SugarColumn]
        public int BattleExpBonusPercent { get; set; }

        [SugarColumn]
        public int ProfessionLevelCap { get; set; }

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
    /// 五行分支升级规则配置。
    /// </summary>
    [SugarTable("FiveElementBranchUpgradeConfigs")]
    public class FiveElementBranchUpgradeConfigEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 50)]
        public string GID { get; set; } = string.Empty;

        [SugarColumn(Length = 20)]
        public string ElementType { get; set; } = string.Empty;

        [SugarColumn]
        public int TargetLevel { get; set; }

        [SugarColumn]
        public long GoldCost { get; set; }

        [SugarColumn]
        public long SpiritStoneCost { get; set; }

        [SugarColumn(Length = 1000, IsNullable = true)]
        public string? MaterialsJson { get; set; }

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
