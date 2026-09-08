using SqlSugar;
using System.Text.Json;

namespace XXX.Entity
{
    /// <summary>
    /// 物品模板落库实体。
    /// </summary>
    [SugarTable("ItemTemplates")]
    public class ItemTemplateEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 100)]
        public string ItemId { get; set; } = string.Empty;

        [SugarColumn(Length = 100, IsNullable = false)]
        public string Name { get; set; } = string.Empty;

        [SugarColumn(IsNullable = false)]
        public int UseLevel { get; set; }

        [SugarColumn(IsNullable = false)]
        public int Type { get; set; }

        [SugarColumn(Length = 2000, IsNullable = false)]
        public string Description { get; set; } = string.Empty;

        [SugarColumn(IsNullable = false)]
        public int MaxStack { get; set; } = 1;

        [SugarColumn(IsNullable = false)]
        public int Quality { get; set; } = 1;

        [SugarColumn(Length = 500, IsNullable = true)]
        public string? IconPath { get; set; }

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? SeedKey { get; set; }

        [SugarColumn]
        public bool IsBuiltIn { get; set; } = false;

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? BuiltInVersion { get; set; }

        [SugarColumn]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;

        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public bool IsTradeable { get; set; } = true;

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? ChestConfigJson { get; set; }

        [SugarColumn(IsIgnore = true)]
        public ItemChestConfig? ChestConfig
        {
            get => string.IsNullOrWhiteSpace(ChestConfigJson)
                ? null
                : JsonSerializer.Deserialize<ItemChestConfig>(ChestConfigJson);
            set => ChestConfigJson = value == null ? null : JsonSerializer.Serialize(value);
        }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? SkillBookConfigJson { get; set; }

        [SugarColumn(IsIgnore = true)]
        public ItemSkillBookConfig? SkillBookConfig
        {
            get => string.IsNullOrWhiteSpace(SkillBookConfigJson)
                ? null
                : JsonSerializer.Deserialize<ItemSkillBookConfig>(SkillBookConfigJson);
            set => SkillBookConfigJson = value == null ? null : JsonSerializer.Serialize(value);
        }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? PetEggConfigJson { get; set; }

        [SugarColumn(IsIgnore = true)]
        public ItemPetEggConfig? PetEggConfig
        {
            get => string.IsNullOrWhiteSpace(PetEggConfigJson)
                ? null
                : JsonSerializer.Deserialize<ItemPetEggConfig>(PetEggConfigJson);
            set => PetEggConfigJson = value == null ? null : JsonSerializer.Serialize(value);
        }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? PillConfigJson { get; set; }

        [SugarColumn(IsIgnore = true)]
        public ItemPillConfig? PillConfig
        {
            get => string.IsNullOrWhiteSpace(PillConfigJson)
                ? null
                : JsonSerializer.Deserialize<ItemPillConfig>(PillConfigJson);
            set => PillConfigJson = value == null ? null : JsonSerializer.Serialize(value);
        }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? FavorabilityGiftConfigJson { get; set; }

        [SugarColumn(IsIgnore = true)]
        public ItemFavorabilityGiftConfig? FavorabilityGiftConfig
        {
            get => string.IsNullOrWhiteSpace(FavorabilityGiftConfigJson)
                ? null
                : JsonSerializer.Deserialize<ItemFavorabilityGiftConfig>(FavorabilityGiftConfigJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            set => FavorabilityGiftConfigJson = value == null ? null : JsonSerializer.Serialize(value);
        }
    }
}
