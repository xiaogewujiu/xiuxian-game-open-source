using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 元素克制矩阵规则配置。
    /// </summary>
    [SugarTable("BattleElementRelationConfigs")]
    public class ElementRelationRuleEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 100)]
        public string GID { get; set; } = string.Empty;

        [SugarColumn]
        public int AttackerElement { get; set; }

        [SugarColumn]
        public int DefenderElement { get; set; }

        [SugarColumn]
        public double Modifier { get; set; }

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
