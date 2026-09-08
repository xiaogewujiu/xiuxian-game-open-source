namespace XXX.Application.DTOs
{
    /// <summary>
    /// 后台作物模板列表项。
    /// </summary>
    public class AdminCropListItemDto
    {
        /// <summary>
        /// 模板编号。
        /// </summary>
        public string TemplateId { get; set; } = string.Empty;

        /// <summary>
        /// 作物名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 作物类型。
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 解锁等级。
        /// </summary>
        public int UnlockLevel { get; set; }

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
    /// 后台作物模板详情。
    /// </summary>
    public class AdminCropDetailDto
    {
        /// <summary>
        /// 模板编号。
        /// </summary>
        public string TemplateId { get; set; } = string.Empty;

        /// <summary>
        /// 作物名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 描述。
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 作物类型。
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 生长周期。
        /// </summary>
        public int GrowthCycle { get; set; }

        /// <summary>
        /// 产量。
        /// </summary>
        public int Yield { get; set; }

        /// <summary>
        /// 种子编号。
        /// </summary>
        public string SeedId { get; set; } = string.Empty;

        /// <summary>
        /// 种子数量。
        /// </summary>
        public int SeedAmount { get; set; }

        /// <summary>
        /// 产出物品编号。
        /// </summary>
        public string OutputItemId { get; set; } = string.Empty;

        /// <summary>
        /// 产出数量。
        /// </summary>
        public int OutputAmount { get; set; }

        /// <summary>
        /// 最低品质。
        /// </summary>
        public int MinQuality { get; set; }

        /// <summary>
        /// 最高品质。
        /// </summary>
        public int MaxQuality { get; set; }

        /// <summary>
        /// 解锁等级。
        /// </summary>
        public int UnlockLevel { get; set; }

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
