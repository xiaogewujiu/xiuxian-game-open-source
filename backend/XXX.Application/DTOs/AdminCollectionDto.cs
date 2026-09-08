namespace XXX.Application.DTOs
{
    // ===== 文字图鉴系列 =====
    public class AdminTextCollectionSeriesListItemDto
    {
        public string SeriesId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public bool IsBuiltIn { get; set; }
        public string? BuiltInVersion { get; set; }
    }

    public class AdminTextCollectionSeriesDetailDto
    {
        public string SeriesId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public int SortOrder { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsBuiltIn { get; set; }
        public string? SeedKey { get; set; }
        public string? BuiltInVersion { get; set; }
        public DateTime? LastUpdateTime { get; set; }
    }

    // ===== 文字图鉴项 =====
    public class AdminTextCollectionItemListItemDto
    {
        public string ItemId { get; set; } = string.Empty;
        public string SeriesId { get; set; } = string.Empty;
        public string Character { get; set; } = string.Empty;
        public int SlotIndex { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsBuiltIn { get; set; }
        public string? BuiltInVersion { get; set; }
    }

    public class AdminTextCollectionItemDetailDto
    {
        public string ItemId { get; set; } = string.Empty;
        public string SeriesId { get; set; } = string.Empty;
        public string Character { get; set; } = string.Empty;
        public int SlotIndex { get; set; }
        public int SortOrder { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsBuiltIn { get; set; }
        public string? SeedKey { get; set; }
        public string? BuiltInVersion { get; set; }
        public DateTime? LastUpdateTime { get; set; }
    }

    // ===== 文字图鉴属性加成 =====
    public class AdminTextCollectionBonusListItemDto
    {
        public string BonusId { get; set; } = string.Empty;
        public string SeriesId { get; set; } = string.Empty;
        public string AttrType { get; set; } = string.Empty;
        public float AttrValue { get; set; }
        public int ValueType { get; set; }
        public bool IsBuiltIn { get; set; }
        public string? BuiltInVersion { get; set; }
    }

    public class AdminTextCollectionBonusDetailDto
    {
        public string BonusId { get; set; } = string.Empty;
        public string SeriesId { get; set; } = string.Empty;
        public string AttrType { get; set; } = string.Empty;
        public float AttrValue { get; set; }
        public int ValueType { get; set; }
        public int SortOrder { get; set; }
        public bool IsBuiltIn { get; set; }
        public string? SeedKey { get; set; }
        public string? BuiltInVersion { get; set; }
        public DateTime? LastUpdateTime { get; set; }
    }

    // ===== 图片图鉴系列 =====
    public class AdminImageCollectionSeriesListItemDto
    {
        public string SeriesId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public bool IsBuiltIn { get; set; }
        public string? BuiltInVersion { get; set; }
    }

    public class AdminImageCollectionSeriesDetailDto
    {
        public string SeriesId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public int SortOrder { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsBuiltIn { get; set; }
        public string? SeedKey { get; set; }
        public string? BuiltInVersion { get; set; }
        public DateTime? LastUpdateTime { get; set; }
    }

    // ===== 图片图鉴项 =====
    public class AdminImageCollectionItemListItemDto
    {
        public string ItemId { get; set; } = string.Empty;
        public string SeriesId { get; set; } = string.Empty;
        public string ImageName { get; set; } = string.Empty;
        public string? ThumbUrl { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsBuiltIn { get; set; }
        public string? BuiltInVersion { get; set; }
    }

    public class AdminImageCollectionItemDetailDto
    {
        public string ItemId { get; set; } = string.Empty;
        public string SeriesId { get; set; } = string.Empty;
        public string ImageName { get; set; } = string.Empty;
        public string? ThumbUrl { get; set; }
        public string? OriginalUrl { get; set; }
        public int SortOrder { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsBuiltIn { get; set; }
        public string? SeedKey { get; set; }
        public string? BuiltInVersion { get; set; }
        public DateTime? LastUpdateTime { get; set; }
    }

    // ===== 图片图鉴属性加成 =====
    public class AdminImageCollectionBonusListItemDto
    {
        public string BonusId { get; set; } = string.Empty;
        public string SeriesId { get; set; } = string.Empty;
        public string AttrType { get; set; } = string.Empty;
        public float AttrValue { get; set; }
        public int ValueType { get; set; }
        public bool IsBuiltIn { get; set; }
        public string? BuiltInVersion { get; set; }
    }

    public class AdminImageCollectionBonusDetailDto
    {
        public string BonusId { get; set; } = string.Empty;
        public string SeriesId { get; set; } = string.Empty;
        public string AttrType { get; set; } = string.Empty;
        public float AttrValue { get; set; }
        public int ValueType { get; set; }
        public int SortOrder { get; set; }
        public bool IsBuiltIn { get; set; }
        public string? SeedKey { get; set; }
        public string? BuiltInVersion { get; set; }
        public DateTime? LastUpdateTime { get; set; }
    }
}
