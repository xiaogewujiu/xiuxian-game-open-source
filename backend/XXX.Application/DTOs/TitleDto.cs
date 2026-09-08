namespace XXX.Application.DTOs
{
    /// <summary>
    /// 称号列表项DTO。
    /// </summary>
    public class TitleListItemDto
    {
        /// <summary>
        /// 称号ID。
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
        /// 解锁时间。
        /// </summary>
        public DateTime UnlockedAt { get; set; }

        /// <summary>
        /// 是否佩戴中。
        /// </summary>
        public bool IsEquipped { get; set; }
    }

    /// <summary>
    /// 当前佩戴称号DTO。
    /// </summary>
    public class CurrentTitleDto
    {
        /// <summary>
        /// 称号ID。
        /// </summary>
        public string TitleId { get; set; } = string.Empty;

        /// <summary>
        /// 称号名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

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
    }

    /// <summary>
    /// 玩家称号总览DTO。
    /// </summary>
    public class PlayerTitleOverviewDto
    {
        /// <summary>
        /// 当前佩戴称号。
        /// </summary>
        public CurrentTitleDto? CurrentTitle { get; set; }

        /// <summary>
        /// 已拥有的称号列表。
        /// </summary>
        public List<TitleListItemDto> Titles { get; set; } = new();
    }
}
