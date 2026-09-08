namespace XXX.Application.DTOs
{
    /// <summary>
    /// 玩家图鉴系列 DTO。
    /// </summary>
    public class PlayerCollectionSeriesDto
    {
        public string SeriesId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public int CollectionType { get; set; }
        public int OwnedCount { get; set; }
        public int TotalCount { get; set; }
        public bool IsComplete { get; set; }
        public List<PlayerCollectionItemDto> Items { get; set; } = [];
        public List<CollectionBonusDto> Bonuses { get; set; } = [];
    }

    /// <summary>
    /// 玩家图鉴项 DTO。
    /// </summary>
    public class PlayerCollectionItemDto
    {
        public string ItemId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? ThumbUrl { get; set; }
        public string? OriginalUrl { get; set; }
        public int SlotIndex { get; set; }
        public int OwnedCount { get; set; }
        public bool IsOwned { get; set; }
    }

    /// <summary>
    /// 图鉴属性加成 DTO。
    /// </summary>
    public class CollectionBonusDto
    {
        public string SeriesId { get; set; } = string.Empty;
        public string SeriesName { get; set; } = string.Empty;
        public string AttrType { get; set; } = string.Empty;
        public float AttrValue { get; set; }
        public int ValueType { get; set; }
    }

    /// <summary>
    /// 属性加成汇总 DTO。
    /// </summary>
    public class CollectionBonusSummaryDto
    {
        public string AttrType { get; set; } = string.Empty;
        public float FixedValue { get; set; }
        public float PercentValue { get; set; }
    }

    public class HeartSutraBonusSummaryDto
    {
        public string AttrType { get; set; } = string.Empty;
        public float FixedValue { get; set; }
        public float PercentValue { get; set; }
    }
}
