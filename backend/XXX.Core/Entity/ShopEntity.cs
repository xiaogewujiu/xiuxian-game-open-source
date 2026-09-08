using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 商店每日购买记录实体
    /// 记录玩家每日的商品购买数量，用于限购功能
    /// </summary>
    [SugarTable("shop_daily_record")]
    public class ShopDailyRecordEntity
    {
        /// <summary>
        /// 记录唯一标识符
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "记录ID")]
        public string GID { get; set; } = string.Empty;

        /// <summary>
        /// 玩家ID
        /// </summary>
        [SugarColumn(ColumnDescription = "玩家ID")]
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 商店ID
        /// </summary>
        [SugarColumn(ColumnDescription = "商店ID", Length = 50)]
        public string ShopId { get; set; } = string.Empty;

        /// <summary>
        /// 商品ID
        /// </summary>
        [SugarColumn(ColumnDescription = "商品ID", Length = 50)]
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// 今日已购买数量
        /// </summary>
        [SugarColumn(ColumnDescription = "今日已购买数量")]
        public int PurchasedCount { get; set; } = 0;

        /// <summary>
        /// 记录日期，用于每日重置
        /// </summary>
        [SugarColumn(ColumnDescription = "记录日期")]
        public DateTime RecordDate { get; set; } = DateTime.Today;

        /// <summary>
        /// 最后更新时间
        /// </summary>
        [SugarColumn(ColumnDescription = "最后更新时间")]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 商店配置实体
    /// 存储商店的基本配置信息
    /// </summary>
    [SugarTable("shop_config")]
    public class ShopConfigEntity
    {
        /// <summary>
        /// 商店唯一标识符
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "商店ID")]
        public string ShopId { get; set; } = string.Empty;

        /// <summary>
        /// 商店名称
        /// </summary>
        [SugarColumn(ColumnDescription = "商店名称", Length = 50)]
        public string ShopName { get; set; } = string.Empty;

        /// <summary>
        /// 商店类型：0-普通商店，1-铁匠铺，2-神秘商店，3-VIP商店
        /// </summary>
        [SugarColumn(ColumnDescription = "商店类型")]
        public int ShopType { get; set; } = 0;

        /// <summary>
        /// 商店描述
        /// </summary>
        [SugarColumn(ColumnDescription = "描述", Length = 200, IsNullable = true)]
        public string? Description { get; set; }

        /// <summary>
        /// 进入商店所需的最低等级
        /// </summary>
        [SugarColumn(ColumnDescription = "所需等级")]
        public int RequiredLevel { get; set; } = 1;

        /// <summary>
        /// 进入商店所需的VIP等级
        /// </summary>
        [SugarColumn(ColumnDescription = "所需VIP等级")]
        public int RequiredVipLevel { get; set; } = 0;

        /// <summary>
        /// 商店折扣系数，1.0表示无折扣
        /// </summary>
        [SugarColumn(ColumnDescription = "折扣")]
        public float Discount { get; set; } = 1.0f;

        /// <summary>
        /// 商店是否开放
        /// </summary>
        [SugarColumn(ColumnDescription = "是否开放")]
        public bool IsOpen { get; set; } = true;

        /// <summary>
        /// 是否自动刷新商品
        /// </summary>
        [SugarColumn(ColumnDescription = "是否自动刷新")]
        public bool AutoRefresh { get; set; } = false;

        /// <summary>
        /// 自动刷新间隔（小时）
        /// </summary>
        [SugarColumn(ColumnDescription = "刷新间隔(小时)")]
        public int RefreshIntervalHours { get; set; } = 24;

        /// <summary>
        /// 商店图标资源路径
        /// </summary>
        [SugarColumn(ColumnDescription = "图标", Length = 100, IsNullable = true)]
        public string? Icon { get; set; }

        /// <summary>
        /// 排序顺序，数值越小越靠前
        /// </summary>
        [SugarColumn(ColumnDescription = "排序顺序")]
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// 是否启用该商店
        /// </summary>
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
    /// 商品实体
    /// 存储商店中销售的商品信息
    /// </summary>
    [SugarTable("shop_item")]
    public class ShopItemEntity
    {
        /// <summary>
        /// 商品记录唯一标识符
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "商品ID")]
        public string GID { get; set; } = string.Empty;

        /// <summary>
        /// 所属商店ID
        /// </summary>
        [SugarColumn(ColumnDescription = "商店ID", Length = 50)]
        public string ShopId { get; set; } = string.Empty;

        /// <summary>
        /// 商品ID（对应道具或装备模板ID）
        /// </summary>
        [SugarColumn(ColumnDescription = "商品ID", Length = 50)]
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// 商品类型：0-道具，1-装备
        /// </summary>
        [SugarColumn(ColumnDescription = "商品类型")]
        public int ItemType { get; set; } = 0;

        /// <summary>
        /// 基础价格
        /// </summary>
        [SugarColumn(ColumnDescription = "基础价格")]
        public int BasePrice { get; set; } = 0;

        /// <summary>
        /// 当前价格（可能因折扣而变化）
        /// </summary>
        [SugarColumn(ColumnDescription = "当前价格")]
        public int CurrentPrice { get; set; } = 0;

        /// <summary>
        /// 库存数量，-1表示无限库存
        /// </summary>
        [SugarColumn(ColumnDescription = "库存，-1表示无限")]
        public int Stock { get; set; } = -1;

        /// <summary>
        /// 初始库存数量
        /// </summary>
        [SugarColumn(ColumnDescription = "初始库存")]
        public int InitialStock { get; set; } = -1;

        /// <summary>
        /// 每日限购数量，-1表示无限
        /// </summary>
        [SugarColumn(ColumnDescription = "每日限购，-1表示无限")]
        public int DailyLimit { get; set; } = -1;

        /// <summary>
        /// 排序顺序，数值越小越靠前
        /// </summary>
        [SugarColumn(ColumnDescription = "排序顺序")]
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// 是否启用该商品
        /// </summary>
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
}
