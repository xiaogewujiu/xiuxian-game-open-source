using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 装备洗练属性值配置 —— 按 (属性类型, 品阶) 定义数值区间。
    /// </summary>
    [SugarTable("EquipmentRerollAttributeValueConfigs")]
    public class EquipmentRerollAttributeValueConfigEntity
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long GID { get; set; }

        [SugarColumn]
        public int AttributeType { get; set; }

        [SugarColumn]
        public int Tier { get; set; }

        [SugarColumn(Length = 50)]
        public string MinValue { get; set; } = "0";

        [SugarColumn(Length = 50)]
        public string MaxValue { get; set; } = "0";

        [SugarColumn]
        public bool IsPercentage { get; set; } = false;

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
