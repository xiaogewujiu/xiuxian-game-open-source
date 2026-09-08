namespace XXX.Application.DTOs
{
    /// <summary>
    /// 管理后台称号模板列表项DTO。
    /// </summary>
    public class AdminTitleListItemDto
    {
        /// <summary>
        /// 主键。
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 称号唯一标识。
        /// </summary>
        public string TitleId { get; set; } = string.Empty;

        /// <summary>
        /// 称号名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 来源类型。
        /// </summary>
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// 稀有度。
        /// </summary>
        public string Rarity { get; set; } = "Common";

        /// <summary>
        /// 是否可见。
        /// </summary>
        public bool IsVisible { get; set; }
    }

    /// <summary>
    /// 管理后台称号模板详情DTO。
    /// </summary>
    public class AdminTitleDetailDto
    {
        /// <summary>
        /// 主键。
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 称号唯一标识。
        /// </summary>
        public string TitleId { get; set; } = string.Empty;

        /// <summary>
        /// 称号名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 描述。
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 来源类型。
        /// </summary>
        public string Source { get; set; } = "Admin";

        /// <summary>
        /// 来源ID。
        /// </summary>
        public string? SourceId { get; set; }

        /// <summary>
        /// 稀有度。
        /// </summary>
        public string Rarity { get; set; } = "Common";

        /// <summary>
        /// 小图标路径。
        /// </summary>
        public string? IconPath { get; set; }

        /// <summary>
        /// 称号图片路径。
        /// </summary>
        public string? ImagePath { get; set; }

        /// <summary>
        /// 是否可见。
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// 创建时间。
        /// </summary>
        public DateTime? CreatedAt { get; set; }
    }

    /// <summary>
    /// 管理后台发放/回收称号请求DTO。
    /// </summary>
    public class AdminGrantTitleDto
    {
        /// <summary>
        /// 玩家GID。
        /// </summary>
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 称号ID。
        /// </summary>
        public string TitleId { get; set; } = string.Empty;
    }
}
