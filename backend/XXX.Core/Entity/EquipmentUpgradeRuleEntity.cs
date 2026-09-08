using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 装备强化规则。按装备需求等级区间匹配。
    /// </summary>
    [SugarTable("EquipmentEnhanceRuleConfigs")]
    public class EquipmentEnhanceRuleEntity
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long GID { get; set; }

        [SugarColumn]
        public int MinEquipmentLevel { get; set; } = 1;

        [SugarColumn]
        public int MaxEquipmentLevel { get; set; } = 10;

        [SugarColumn(Length = 50)]
        public string MaterialItemId { get; set; } = string.Empty;

        [SugarColumn]
        public int MaterialCount { get; set; } = 1;

        [SugarColumn]
        public long GoldCost { get; set; }

        [SugarColumn]
        public int SuccessRate { get; set; } = 100;

        [SugarColumn]
        public int AttributeGrowthPercent { get; set; } = 10;

        [SugarColumn]
        public int MaxEnhanceLevel { get; set; } = 15;

        [SugarColumn]
        public int SortOrder { get; set; }

        [SugarColumn]
        public bool IsEnabled { get; set; } = true;

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? SeedKey { get; set; }

        [SugarColumn]
        public bool IsBuiltIn { get; set; }

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? BuiltInVersion { get; set; }

        [SugarColumn]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 装备洗炼消耗规则。按装备需求等级区间匹配。
    /// </summary>
    [SugarTable("EquipmentRerollCostRuleConfigs")]
    public class EquipmentRerollCostRuleEntity
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long GID { get; set; }

        [SugarColumn]
        public int MinEquipmentLevel { get; set; } = 1;

        [SugarColumn]
        public int MaxEquipmentLevel { get; set; } = 10;

        [SugarColumn(Length = 50)]
        public string MaterialItemId { get; set; } = string.Empty;

        [SugarColumn]
        public int MaterialCount { get; set; } = 1;

        [SugarColumn]
        public long GoldCost { get; set; }

        [SugarColumn]
        public int ExtraMaterialPerLockedLine { get; set; }

        [SugarColumn]
        public int ExtraGoldPerLockedLine { get; set; }

        [SugarColumn]
        public int SortOrder { get; set; }

        [SugarColumn]
        public bool IsEnabled { get; set; } = true;

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? SeedKey { get; set; }

        [SugarColumn]
        public bool IsBuiltIn { get; set; }

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? BuiltInVersion { get; set; }

        [SugarColumn]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }
}
