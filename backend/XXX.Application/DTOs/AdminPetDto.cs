namespace XXX.Application.DTOs
{
    /// <summary>
    /// 后台灵宠模板列表项。
    /// </summary>
    public class AdminPetListItemDto
    {
        /// <summary>
        /// 模板编号。
        /// </summary>
        public string TemplateId { get; set; } = string.Empty;

        /// <summary>
        /// 灵宠名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 灵宠类型。
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 初始品质下限。
        /// </summary>
        public int InitialQualityMin { get; set; }

        /// <summary>
        /// 初始品质上限。
        /// </summary>
        public int InitialQualityMax { get; set; }

        /// <summary>
        /// 是否为内置模板。
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// 内置版本号。
        /// </summary>
        public string? BuiltInVersion { get; set; }
    }

    /// <summary>
    /// 后台灵宠模板详情。
    /// </summary>
    public class AdminPetDetailDto
    {
        /// <summary>
        /// 模板编号。
        /// </summary>
        public string TemplateId { get; set; } = string.Empty;

        /// <summary>
        /// 灵宠名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 描述。
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 灵宠类型。
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 初始品质下限。
        /// </summary>
        public int InitialQualityMin { get; set; }

        /// <summary>
        /// 初始品质上限。
        /// </summary>
        public int InitialQualityMax { get; set; }

        /// <summary>
        /// 最大品质。
        /// </summary>
        public int MaxQuality { get; set; }

        /// <summary>
        /// 成长率下限。
        /// </summary>
        public double GrowthRateMin { get; set; }

        /// <summary>
        /// 成长率上限。
        /// </summary>
        public double GrowthRateMax { get; set; }

        /// <summary>
        /// 初始技能数量。
        /// </summary>
        public int InitialSkillCount { get; set; }

        /// <summary>
        /// 属性区间模板。
        /// </summary>
        public BaseAttributesRangeDto Attributes { get; set; } = new();

        /// <summary>
        /// 技能池编号列表。
        /// </summary>
        public List<string> SkillIds { get; set; } = [];

        /// <summary>
        /// 获取方式。
        /// </summary>
        public string? ObtainMethod { get; set; }

        /// <summary>
        /// 是否可交易。
        /// </summary>
        public bool IsTradable { get; set; }

        /// <summary>
        /// 是否为内置模板。
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
