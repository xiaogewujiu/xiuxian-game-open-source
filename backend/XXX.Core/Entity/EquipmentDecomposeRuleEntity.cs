using SqlSugar;

namespace XXX.Entity;

/// <summary>
/// 装备分解产出规则。每条规则按装备品质匹配一个指定道具，数量可配置为固定值或随机区间。
/// </summary>
[SugarTable("EquipmentDecomposeRuleConfigs")]
public class EquipmentDecomposeRuleEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long GID { get; set; }

    [SugarColumn]
    public int Quality { get; set; } = 1;

    [SugarColumn(Length = 50)]
    public string MaterialItemId { get; set; } = string.Empty;

    [SugarColumn]
    public int MinQuantity { get; set; } = 1;

    [SugarColumn]
    public int MaxQuantity { get; set; } = 1;

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
