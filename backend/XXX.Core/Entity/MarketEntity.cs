using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 寄售行商品实体
    /// </summary>
    [SugarTable("MarketListings")]
    public class MarketListingEntity
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        [SugarColumn(Length = 50, IsNullable = false, IndexGroupNameList = new[] { "idx_seller" })]
        public string SellerId { get; set; } = string.Empty;

        [SugarColumn(Length = 100, IsNullable = false)]
        public string SellerName { get; set; } = string.Empty;

        [SugarColumn(Length = 20, IsNullable = false)]
        public string ItemType { get; set; } = "Item";

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? ItemId { get; set; }

        [SugarColumn(Length = 50, IsNullable = true)]
        public string? EquipmentInstanceId { get; set; }

        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public int Quantity { get; set; } = 1;

        [SugarColumn(IsNullable = false)]
        public long Price { get; set; }

        [SugarColumn(Length = 20, IsNullable = false)]
        public string CurrencyType { get; set; } = "Gold";

        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public bool IsBound { get; set; }

        [SugarColumn(Length = 20, IsNullable = false, DefaultValue = "'Listed'")]
        public string Status { get; set; } = "Listed";

        [SugarColumn(Length = 50, IsNullable = true)]
        public string? BuyerId { get; set; }

        [SugarColumn(IsNullable = false)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [SugarColumn(IsNullable = true)]
        public DateTime? SoldAt { get; set; }

        [SugarColumn(IsNullable = false)]
        public DateTime ExpireAt { get; set; }
    }

    /// <summary>
    /// 寄售行配置实体
    /// </summary>
    [SugarTable("MarketConfigs")]
    public class MarketConfigEntity
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        [SugarColumn(Length = 50, IsNullable = false)]
        public string ConfigKey { get; set; } = string.Empty;

        [SugarColumn(Length = 200, IsNullable = false)]
        public string ConfigValue { get; set; } = string.Empty;
    }
}
