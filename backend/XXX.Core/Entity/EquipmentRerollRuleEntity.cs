using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 装备洗练系统配置。
    /// </summary>
    [SugarTable("EquipmentRerollSystemConfigs")]
    public class EquipmentRerollSystemConfigEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 50)]
        public string ConfigId { get; set; } = "default";

        [SugarColumn]
        public bool IsEnabled { get; set; } = true;

        [SugarColumn(Length = 50)]
        public string RerollStoneItemId { get; set; } = "reroll_stone";

        [SugarColumn]
        public int BaseStoneCost { get; set; } = 1;

        [SugarColumn]
        public int ExtraStoneCostPerLockedLine { get; set; } = 1;

        [SugarColumn]
        public int MaxLockedLineCount { get; set; } = 3;

        [SugarColumn]
        public int BaseGoldCost { get; set; } = 300;

        [SugarColumn]
        public int GoldCostPerEquipmentLevel { get; set; } = 25;

        [SugarColumn(Length = 500, IsNullable = true)]
        public string? QualityGoldMultipliersJson { get; set; }

        [SugarColumn]
        public int RerollCountGoldGrowthPercent { get; set; } = 3;

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
    /// 装备洗练槽位词条池配置。
    /// </summary>
    [SugarTable("EquipmentRerollSlotPoolConfigs")]
    public class EquipmentRerollSlotPoolConfigEntity
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long GID { get; set; }

        [SugarColumn]
        public EquipmentSlot Slot { get; set; }

        [SugarColumn]
        public int AttributeType { get; set; }

        [SugarColumn]
        public int Tier { get; set; } = 1;

        [SugarColumn]
        public int MaxDuplicateCount { get; set; } = 1;

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

    /// <summary>
    /// 装备洗练品阶配置。
    /// </summary>
    [SugarTable("EquipmentRerollTierConfigs")]
    public class EquipmentRerollTierConfigEntity
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long GID { get; set; }

        [SugarColumn]
        public int Tier { get; set; }

        [SugarColumn(Length = 50)]
        public string Name { get; set; } = string.Empty;

        [SugarColumn(Length = 20)]
        public string Color { get; set; } = "#9ca3af";

        [SugarColumn]
        public int Weight { get; set; } = 10;

        [SugarColumn(Length = 50)]
        public string ValueMultiplier { get; set; } = "1.00";

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
