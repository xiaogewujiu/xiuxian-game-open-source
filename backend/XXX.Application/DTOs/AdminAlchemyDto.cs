namespace XXX.Application.DTOs
{
    /// <summary>
    /// 后台炼丹配方列表项。
    /// </summary>
    public class AdminAlchemyRecipeListItemDto
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
        /// 丹药模板编号。
        /// </summary>
        public string PillTemplateId { get; set; } = string.Empty;

        /// <summary>
        /// 炉等级要求。
        /// </summary>
        public int RequiredFurnaceLevel { get; set; }

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
    /// 后台炼丹配方详情。
    /// </summary>
    public class AdminAlchemyRecipeDetailDto
    {
        /// <summary>
        /// 配方编号。
        /// </summary>
        public string RecipeId { get; set; } = string.Empty;

        /// <summary>
        /// 丹药模板编号。
        /// </summary>
        public string PillTemplateId { get; set; } = string.Empty;

        /// <summary>
        /// 配方名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 描述。
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 炉等级要求。
        /// </summary>
        public int RequiredFurnaceLevel { get; set; }

        /// <summary>
        /// 基础成功率。
        /// </summary>
        public int BaseSuccessRate { get; set; }

        /// <summary>
        /// 基础炼制时长。
        /// </summary>
        public int BaseCraftTime { get; set; }

        /// <summary>
        /// 材料 JSON。
        /// </summary>
        public string MaterialsJson { get; set; } = "[]";

        /// <summary>
        /// 解锁条件。
        /// </summary>
        public string? UnlockCondition { get; set; }

        /// <summary>
        /// 是否默认已学会。
        /// </summary>
        public bool IsDefaultLearned { get; set; }

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
