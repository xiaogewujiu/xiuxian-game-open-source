namespace XXX.Application.DTOs
{
    /// <summary>
    /// 后台锻造配方列表项。
    /// </summary>
    public class AdminForgeRecipeListItemDto
    {
        /// <summary>
        /// 配方编号。
        /// </summary>
        public string RecipeId { get; set; } = string.Empty;

        /// <summary>
        /// 配方名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 对应模板编号。
        /// </summary>
        public string TemplateId { get; set; } = string.Empty;

        /// <summary>
        /// 等级。
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 是否为内置配方。
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// 内置版本号。
        /// </summary>
        public string? BuiltInVersion { get; set; }
    }

    /// <summary>
    /// 后台锻造配方详情。
    /// </summary>
    public class AdminForgeRecipeDetailDto
    {
        /// <summary>
        /// 配方编号。
        /// </summary>
        public string RecipeId { get; set; } = string.Empty;

        /// <summary>
        /// 对应模板编号。
        /// </summary>
        public string TemplateId { get; set; } = string.Empty;

        /// <summary>
        /// 配方名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 描述。
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 槽位名称。
        /// </summary>
        public string SlotName { get; set; } = string.Empty;

        /// <summary>
        /// 品质。
        /// </summary>
        public int Quality { get; set; }

        /// <summary>
        /// 等级。
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 图标。
        /// </summary>
        public string Icon { get; set; } = string.Empty;

        /// <summary>
        /// 价格。
        /// </summary>
        public long CostGold { get; set; }

        /// <summary>
        /// 成功率。
        /// </summary>
        public int SuccessRate { get; set; }

        /// <summary>
        /// 材料 JSON。
        /// </summary>
        public string MaterialsJson { get; set; } = "[]";

        /// <summary>
        /// 是否启用。
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 是否为内置配方。
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// 内置种子键。
        /// </summary>
        public string? SeedKey { get; set; }

        /// <summary>
        /// 内置版本号。
        /// </summary>
        public string? BuiltInVersion { get; set; }

        /// <summary>
        /// 最后更新时间。
        /// </summary>
        public DateTime? LastUpdateTime { get; set; }
    }
}
