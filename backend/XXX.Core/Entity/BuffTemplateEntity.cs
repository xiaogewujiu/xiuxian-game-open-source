using SqlSugar;
using System.Text.Json;

namespace XXX.Entity
{
    /// <summary>
    /// Buff 模板落库实体。
    /// </summary>
    [SugarTable("BuffTemplates")]
    public class BuffTemplateEntity
    {
        public const string LegacyCatalog = "legacy";
        public const string CurrentCatalog = "current";

        /// <summary>
        /// Buff 目录：legacy 为历史 Buff，current 为当前版本 Buff。
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, DefaultValue = "legacy")]
        public string BuffCatalog { get; set; } = LegacyCatalog;

        [SugarColumn(IsPrimaryKey = true, Length = 100)]
        public string BuffId { get; set; } = string.Empty;

        [SugarColumn(Length = 100, IsNullable = false)]
        public string Name { get; set; } = string.Empty;

        [SugarColumn(Length = 2000, IsNullable = false)]
        public string Description { get; set; } = string.Empty;

        [SugarColumn(IsNullable = false)]
        public int Duration { get; set; }

        [SugarColumn(IsNullable = false)]
        public int MaxStack { get; set; }

        [SugarColumn(IsNullable = false)]
        public int StackRule { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? EffectsJson { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string? SeedKey { get; set; }

        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public bool IsBuiltIn { get; set; } = false;

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? BuiltInVersion { get; set; }

        [SugarColumn(IsNullable = false)]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;

        [SugarColumn(IsIgnore = true)]
        public List<BuffEffect> Effects
        {
            get => string.IsNullOrWhiteSpace(EffectsJson)
                ? []
                : JsonSerializer.Deserialize<List<BuffEffect>>(EffectsJson) ?? [];
            set => EffectsJson = JsonSerializer.Serialize(value ?? []);
        }
    }
}
