using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 副本模板实体。
    /// </summary>
    [SugarTable("DungeonTemplates")]
    public class DungeonTemplateEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 50)]
        public string DungeonId { get; set; } = string.Empty;

        [SugarColumn(Length = 100, IsNullable = false)]
        public string Name { get; set; } = string.Empty;

        [SugarColumn(Length = 1000, IsNullable = false)]
        public string Description { get; set; } = string.Empty;

        [SugarColumn(IsNullable = false)]
        public int RecommendedLevel { get; set; }

        [SugarColumn(IsNullable = false)]
        public int DailyLimit { get; set; }

        [SugarColumn(Length = 50, IsNullable = false)]
        public string NormalMapId { get; set; } = string.Empty;

        [SugarColumn(Length = 50, IsNullable = false)]
        public string FubenMapId { get; set; } = string.Empty;

        [SugarColumn(IsNullable = false)]
        public int RequiredTeamSize { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string? SeedKey { get; set; }

        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public bool IsBuiltIn { get; set; } = false;

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? BuiltInVersion { get; set; }

        [SugarColumn(IsNullable = false)]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }
}
