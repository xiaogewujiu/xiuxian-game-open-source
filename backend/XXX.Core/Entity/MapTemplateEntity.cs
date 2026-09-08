using SqlSugar;
using System.Text.Json;

namespace XXX.Entity
{
    /// <summary>
    /// 地图模板落库实体。
    /// </summary>
    [SugarTable("MapTemplates")]
    public class MapTemplateEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 100)]
        public string MapId { get; set; } = string.Empty;

        [SugarColumn(Length = 100, IsNullable = false)]
        public string Name { get; set; } = string.Empty;

        [SugarColumn(IsNullable = false)]
        public int Level { get; set; }

        [SugarColumn(Length = 2000, IsNullable = false)]
        public string Description { get; set; } = string.Empty;

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? NextMapId { get; set; }

        [SugarColumn(IsNullable = false)]
        public bool Carrying { get; set; }

        [SugarColumn(IsNullable = false)]
        public int MonsterCountMin { get; set; }

        [SugarColumn(IsNullable = false)]
        public int MonsterCountMax { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? SpawnRulesJson { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string? SeedKey { get; set; }

        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public bool IsBuiltIn { get; set; } = false;

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? BuiltInVersion { get; set; }

        [SugarColumn(IsNullable = false)]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;

        [SugarColumn(IsIgnore = true)]
        public List<MapMonsterSpawnRule> SpawnRules
        {
            get => string.IsNullOrWhiteSpace(SpawnRulesJson)
                ? []
                : JsonSerializer.Deserialize<List<MapMonsterSpawnRule>>(SpawnRulesJson) ?? [];
            set => SpawnRulesJson = JsonSerializer.Serialize(value ?? []);
        }
    }
}
