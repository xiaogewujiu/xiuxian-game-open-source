namespace XXX.Application.DTOs
{
    /// <summary>
    /// 后台寄售行配置
    /// </summary>
    public class AdminMarketConfigDto
    {
        /// <summary>手续费百分比</summary>
        public int FeePercent { get; set; } = 5;

        /// <summary>每人最大上架数</summary>
        public int MaxListingsPerPlayer { get; set; } = 10;

        /// <summary>上架有效期天数</summary>
        public int ListingDurationDays { get; set; } = 7;

        /// <summary>允许的货币类型</summary>
        public string AllowedCurrencies { get; set; } = "Gold,SpiritStone";

        /// <summary>最低价格</summary>
        public long MinPrice { get; set; } = 1;
    }

    /// <summary>
    /// 后台寄售行商品列表项
    /// </summary>
    public class AdminMarketListingDto
    {
        public long Id { get; set; }
        public string SellerId { get; set; } = string.Empty;
        public string SellerName { get; set; } = string.Empty;
        public string ItemType { get; set; } = "Item";
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public long Price { get; set; }
        public string CurrencyType { get; set; } = "Gold";
        public string Status { get; set; } = "Listed";
        public string CreatedAt { get; set; } = string.Empty;
        public string ExpireAt { get; set; } = string.Empty;
    }

    /// <summary>
    /// 后台交易日志
    /// </summary>
    public class AdminMarketTransactionDto
    {
        public long Id { get; set; }
        public string SellerName { get; set; } = string.Empty;
        public string? BuyerId { get; set; }
        public string ItemType { get; set; } = "Item";
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public long Price { get; set; }
        public string CurrencyType { get; set; } = "Gold";
        public long Fee { get; set; }
        public string? SoldAt { get; set; }
    }
}
