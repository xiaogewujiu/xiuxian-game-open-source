using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 图片图鉴系列实体
    /// </summary>
    [SugarTable("image_collection_series")]
    public class ImageCollectionSeriesEntity
    {
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "系列ID")]
        public string SeriesId { get; set; } = string.Empty;

        [SugarColumn(ColumnDescription = "系列名称", Length = 50)]
        public string Name { get; set; } = string.Empty;

        [SugarColumn(ColumnDescription = "系列描述", Length = 200, IsNullable = true)]
        public string? Description { get; set; }

        [SugarColumn(ColumnDescription = "系列图标", Length = 200, IsNullable = true)]
        public string? Icon { get; set; }

        [SugarColumn(ColumnDescription = "排序顺序")]
        public int SortOrder { get; set; } = 0;

        [SugarColumn(ColumnDescription = "是否启用")]
        public bool IsEnabled { get; set; } = true;

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? SeedKey { get; set; }

        [SugarColumn]
        public bool IsBuiltIn { get; set; } = false;

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? BuiltInVersion { get; set; }

        [SugarColumn]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 图片图鉴项实体
    /// </summary>
    [SugarTable("image_collection_item")]
    public class ImageCollectionItemEntity
    {
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "图鉴项ID")]
        public string ItemId { get; set; } = string.Empty;

        [SugarColumn(ColumnDescription = "所属系列ID", Length = 50)]
        public string SeriesId { get; set; } = string.Empty;

        [SugarColumn(ColumnDescription = "图片名称", Length = 50)]
        public string ImageName { get; set; } = string.Empty;

        [SugarColumn(ColumnDescription = "缩略图URL", Length = 500, IsNullable = true)]
        public string? ThumbUrl { get; set; }

        [SugarColumn(ColumnDescription = "原图URL", Length = 500, IsNullable = true)]
        public string? OriginalUrl { get; set; }

        [SugarColumn(ColumnDescription = "排序顺序")]
        public int SortOrder { get; set; } = 0;

        [SugarColumn(ColumnDescription = "是否启用")]
        public bool IsEnabled { get; set; } = true;

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? SeedKey { get; set; }

        [SugarColumn]
        public bool IsBuiltIn { get; set; } = false;

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? BuiltInVersion { get; set; }

        [SugarColumn]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 图片图鉴属性加成实体
    /// </summary>
    [SugarTable("image_collection_bonus")]
    public class ImageCollectionBonusEntity
    {
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "加成ID")]
        public string BonusId { get; set; } = string.Empty;

        [SugarColumn(ColumnDescription = "所属系列ID", Length = 50)]
        public string SeriesId { get; set; } = string.Empty;

        [SugarColumn(ColumnDescription = "属性类型", Length = 50)]
        public string AttrType { get; set; } = string.Empty;

        [SugarColumn(ColumnDescription = "属性值")]
        public float AttrValue { get; set; } = 0;

        /// <summary>
        /// 属性值类型：0-固定值，1-百分比
        /// </summary>
        [SugarColumn(ColumnDescription = "属性值类型：0-固定值，1-百分比")]
        public int ValueType { get; set; } = 0;

        [SugarColumn(ColumnDescription = "排序顺序")]
        public int SortOrder { get; set; } = 0;

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? SeedKey { get; set; }

        [SugarColumn]
        public bool IsBuiltIn { get; set; } = false;

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? BuiltInVersion { get; set; }

        [SugarColumn]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }
}
