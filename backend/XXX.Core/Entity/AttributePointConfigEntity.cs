using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 属性点配置。
    /// 一行代表“某属性在某等级区间”的配置快照。
    /// </summary>
    [SugarTable("AttributePointConfigs")]
    public class AttributePointConfigEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 100)]
        public string ConfigId { get; set; } = string.Empty;

        [SugarColumn(Length = 50)]
        public string Key { get; set; } = string.Empty;

        [SugarColumn(Length = 50)]
        public string Name { get; set; } = string.Empty;

        [SugarColumn(Length = 20, IsNullable = false, DefaultValue = "warrior")]
        public string Profession { get; set; } = PlayerProfessionCatalog.Warrior;

        [SugarColumn]
        public int AttributeType { get; set; }

        /// <summary>
        /// 每次触发属性收益时增加的整数属性值。
        /// </summary>
        [SugarColumn(DefaultValue = "1")]
        public int BonusPerPoint { get; set; } = 1;

        /// <summary>
        /// 累计多少个属性点触发一次收益。
        /// 普通固定属性为 1；速度可配置为 10 或 12。
        /// </summary>
        [SugarColumn(DefaultValue = "1")]
        public int PointsPerBonus { get; set; } = 1;

        [SugarColumn]
        public int LevelStart { get; set; }

        [SugarColumn]
        public int LevelEnd { get; set; }

        [SugarColumn]
        public int PointsGained { get; set; }

        [SugarColumn]
        public int SortOrder { get; set; }

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
