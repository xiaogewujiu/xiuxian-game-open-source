using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 锻造配方实体。
    /// </summary>
    [SugarTable("ForgeRecipes")]
    public class ForgeRecipeEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 100)]
        public string RecipeId { get; set; } = string.Empty;

        [SugarColumn(Length = 50, IsNullable = false)]
        public string TemplateId { get; set; } = string.Empty;

        [SugarColumn(Length = 100, IsNullable = false)]
        public string Name { get; set; } = string.Empty;

        [SugarColumn(Length = 1000, IsNullable = false)]
        public string Description { get; set; } = string.Empty;

        [SugarColumn(Length = 50, IsNullable = false)]
        public string SlotName { get; set; } = string.Empty;

        [SugarColumn(IsNullable = false)]
        public int Quality { get; set; }

        [SugarColumn(IsNullable = false)]
        public int Level { get; set; }

        [SugarColumn(Length = 50, IsNullable = false)]
        public string Icon { get; set; } = string.Empty;

        [SugarColumn(IsNullable = false)]
        public long CostGold { get; set; }

        [SugarColumn(IsNullable = false)]
        public int SuccessRate { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = false)]
        public string MaterialsJson { get; set; } = "[]";

        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public bool IsEnabled { get; set; } = true;

        [SugarColumn(Length = 200, IsNullable = true)]
        public string? SeedKey { get; set; }

        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public bool IsBuiltIn { get; set; } = false;

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? BuiltInVersion { get; set; }

        [SugarColumn(IsNullable = false)]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 锻造配方材料快照。
    /// </summary>
    public class ForgeRecipeMaterialSnapshot
    {
        public string ItemId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Icon { get; set; } = string.Empty;

        public int Count { get; set; }
    }
}
