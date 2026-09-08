namespace XXX.Application.DTOs
{
    /// <summary>
    /// 后台商店配置列表项。
    /// </summary>
    public class AdminShopConfigListItemDto
    {
        /// <summary>
        /// 商店编号。
        /// </summary>
        public string ShopId { get; set; } = string.Empty;

        /// <summary>
        /// 商店名称。
        /// </summary>
        public string ShopName { get; set; } = string.Empty;

        /// <summary>
        /// 商店类型。
        /// </summary>
        public int ShopType { get; set; }

        /// <summary>
        /// 是否开放。
        /// </summary>
        public bool IsOpen { get; set; }

        /// <summary>
        /// 是否启用。
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 是否为内置种子配置。
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// 内置版本号。
        /// </summary>
        public string? BuiltInVersion { get; set; }
    }

    /// <summary>
    /// 后台商店配置详情。
    /// </summary>
    public class AdminShopConfigDetailDto
    {
        /// <summary>
        /// 商店编号。
        /// </summary>
        public string ShopId { get; set; } = string.Empty;

        /// <summary>
        /// 商店名称。
        /// </summary>
        public string ShopName { get; set; } = string.Empty;

        /// <summary>
        /// 商店类型。
        /// </summary>
        public int ShopType { get; set; }

        /// <summary>
        /// 描述。
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 需要等级。
        /// </summary>
        public int RequiredLevel { get; set; }

        /// <summary>
        /// 需要 VIP 等级。
        /// </summary>
        public int RequiredVipLevel { get; set; }

        /// <summary>
        /// 折扣。
        /// </summary>
        public float Discount { get; set; }

        /// <summary>
        /// 是否开放。
        /// </summary>
        public bool IsOpen { get; set; }

        /// <summary>
        /// 是否自动刷新。
        /// </summary>
        public bool AutoRefresh { get; set; }

        /// <summary>
        /// 刷新间隔小时。
        /// </summary>
        public int RefreshIntervalHours { get; set; }

        /// <summary>
        /// 图标。
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// 排序值。
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 是否启用。
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 是否为内置种子配置。
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
        /// 最近一次编辑时间。
        /// </summary>
        public DateTime? LastUpdateTime { get; set; }
    }

    /// <summary>
    /// 后台商店商品列表项。
    /// </summary>
    public class AdminShopItemListItemDto
    {
        /// <summary>
        /// 商品记录编号。
        /// </summary>
        public string GID { get; set; } = string.Empty;

        /// <summary>
        /// 商店编号。
        /// </summary>
        public string ShopId { get; set; } = string.Empty;

        /// <summary>
        /// 商店名称。
        /// </summary>
        public string ShopName { get; set; } = string.Empty;

        /// <summary>
        /// 商品编号。
        /// </summary>
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// 商品名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 商品类型。
        /// </summary>
        public int ItemType { get; set; }

        /// <summary>
        /// 当前价格。
        /// </summary>
        public int CurrentPrice { get; set; }

        /// <summary>
        /// 是否启用。
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 是否为内置种子商品。
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// 内置版本号。
        /// </summary>
        public string? BuiltInVersion { get; set; }
    }

    /// <summary>
    /// 后台商店商品详情。
    /// </summary>
    public class AdminShopItemDetailDto
    {
        /// <summary>
        /// 商品记录编号。
        /// </summary>
        public string GID { get; set; } = string.Empty;

        /// <summary>
        /// 商店编号。
        /// </summary>
        public string ShopId { get; set; } = string.Empty;

        /// <summary>
        /// 商品编号。
        /// </summary>
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// 商品类型。
        /// </summary>
        public int ItemType { get; set; }

        /// <summary>
        /// 基础价格。
        /// </summary>
        public int BasePrice { get; set; }

        /// <summary>
        /// 当前价格。
        /// </summary>
        public int CurrentPrice { get; set; }

        /// <summary>
        /// 库存。
        /// </summary>
        public int Stock { get; set; }

        /// <summary>
        /// 初始库存。
        /// </summary>
        public int InitialStock { get; set; }

        /// <summary>
        /// 每日限购。
        /// </summary>
        public int DailyLimit { get; set; }

        /// <summary>
        /// 排序值。
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 是否启用。
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 是否为内置种子商品。
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
        /// 最近一次编辑时间。
        /// </summary>
        public DateTime? LastUpdateTime { get; set; }
    }
}
