using XXX.Entity;

namespace XXX.Application.DTOs
{
    /// <summary>
    /// 装备分解产出规则 DTO。
    /// </summary>
    public class AdminEquipmentDecomposeRuleDto
    {
        public long GID { get; set; }
        public int Quality { get; set; } = 1;
        public string MaterialItemId { get; set; } = string.Empty;
        public int MinQuantity { get; set; } = 1;
        public int MaxQuantity { get; set; } = 1;
        public int SortOrder { get; set; }
        public bool IsEnabled { get; set; } = true;
        public bool IsBuiltIn { get; set; }
        public string? BuiltInVersion { get; set; }
        public string? SeedKey { get; set; }
        public DateTime? LastUpdateTime { get; set; }
    }

    /// <summary>
    /// 装备强化等级区间规则 DTO。
    /// </summary>
    public class AdminEquipmentEnhanceRuleDto
    {
        public long GID { get; set; }
        public int MinEquipmentLevel { get; set; } = 1;
        public int MaxEquipmentLevel { get; set; } = 10;
        public string MaterialItemId { get; set; } = string.Empty;
        public int MaterialCount { get; set; } = 1;
        public long GoldCost { get; set; }
        public int SuccessRate { get; set; } = 100;
        public int AttributeGrowthPercent { get; set; } = 10;
        public int MaxEnhanceLevel { get; set; } = 15;
        public int SortOrder { get; set; }
        public bool IsEnabled { get; set; } = true;
        public bool IsBuiltIn { get; set; }
        public string? BuiltInVersion { get; set; }
        public string? SeedKey { get; set; }
        public DateTime? LastUpdateTime { get; set; }
    }

    /// <summary>
    /// 装备洗炼消耗等级区间规则 DTO。
    /// </summary>
    public class AdminEquipmentRerollCostRuleDto
    {
        public long GID { get; set; }
        public int MinEquipmentLevel { get; set; } = 1;
        public int MaxEquipmentLevel { get; set; } = 10;
        public string MaterialItemId { get; set; } = string.Empty;
        public int MaterialCount { get; set; } = 1;
        public long GoldCost { get; set; }
        public int ExtraMaterialPerLockedLine { get; set; }
        public int ExtraGoldPerLockedLine { get; set; }
        public int SortOrder { get; set; }
        public bool IsEnabled { get; set; } = true;
        public bool IsBuiltIn { get; set; }
        public string? BuiltInVersion { get; set; }
        public string? SeedKey { get; set; }
        public DateTime? LastUpdateTime { get; set; }
    }

    /// <summary>
    /// 装备洗练系统配置 DTO。
    /// </summary>
    public class AdminEquipmentRerollSystemConfigDto
    {
        public string ConfigId { get; set; } = "default";
        public bool IsEnabled { get; set; } = true;
        public string RerollStoneItemId { get; set; } = "reroll_stone";
        public int BaseStoneCost { get; set; } = 1;
        public int ExtraStoneCostPerLockedLine { get; set; } = 1;
        public int MaxLockedLineCount { get; set; } = 3;
        public int BaseGoldCost { get; set; } = 300;
        public int GoldCostPerEquipmentLevel { get; set; } = 25;
        public string? QualityGoldMultipliersJson { get; set; }
        public int RerollCountGoldGrowthPercent { get; set; } = 3;
        public bool IsBuiltIn { get; set; }
        public string? BuiltInVersion { get; set; }
        public string? SeedKey { get; set; }
        public DateTime? LastUpdateTime { get; set; }
    }

    /// <summary>
    /// 装备洗练槽位词条池配置 DTO。
    /// </summary>
    public class AdminEquipmentRerollSlotPoolConfigDto
    {
        public long GID { get; set; }
        public EquipmentSlot Slot { get; set; }
        public int AttributeType { get; set; }
        public int Tier { get; set; } = 1;
        public int MaxDuplicateCount { get; set; } = 1;
        public int SortOrder { get; set; }
        public bool IsEnabled { get; set; } = true;
        public bool IsBuiltIn { get; set; }
        public string? BuiltInVersion { get; set; }
        public string? SeedKey { get; set; }
        public DateTime? LastUpdateTime { get; set; }
    }

    /// <summary>
    /// 装备洗练属性值配置 DTO —— 按 (属性类型, 品阶) 定义数值区间。
    /// </summary>
    public class AdminEquipmentRerollAttributeValueConfigDto
    {
        public long GID { get; set; }
        public int AttributeType { get; set; }
        public int Tier { get; set; }
        public string MinValue { get; set; } = "0";
        public string MaxValue { get; set; } = "0";
        public bool IsPercentage { get; set; }
        public int SortOrder { get; set; }
        public bool IsEnabled { get; set; } = true;
        public bool IsBuiltIn { get; set; }
        public string? BuiltInVersion { get; set; }
        public string? SeedKey { get; set; }
        public DateTime? LastUpdateTime { get; set; }
    }

    /// <summary>
    /// 装备洗练品阶配置 DTO。
    /// </summary>
    public class AdminEquipmentRerollTierConfigDto
    {
        public long GID { get; set; }
        public int Tier { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = "#9ca3af";
        public int Weight { get; set; } = 10;
        public string ValueMultiplier { get; set; } = "1.00";
        public int SortOrder { get; set; }
        public bool IsEnabled { get; set; } = true;
        public bool IsBuiltIn { get; set; }
        public string? BuiltInVersion { get; set; }
        public string? SeedKey { get; set; }
        public DateTime? LastUpdateTime { get; set; }
    }
}
