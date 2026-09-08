namespace XXX.Application.DTOs
{
    /// <summary>
    /// 寄售行商品列表项
    /// </summary>
    public class MarketListingDto
    {
        /// <summary>商品ID</summary>
        public long Id { get; set; }

        /// <summary>卖家ID</summary>
        public string SellerId { get; set; } = string.Empty;

        /// <summary>卖家名称</summary>
        public string SellerName { get; set; } = string.Empty;

        /// <summary>物品类型: Item/Equipment</summary>
        public string ItemType { get; set; } = "Item";

        /// <summary>物品模板ID</summary>
        public string? ItemId { get; set; }

        /// <summary>装备实例ID</summary>
        public string? EquipmentInstanceId { get; set; }

        /// <summary>物品名称</summary>
        public string ItemName { get; set; } = string.Empty;

        /// <summary>品质</summary>
        public int Quality { get; set; }

        /// <summary>数量</summary>
        public int Quantity { get; set; }

        /// <summary>单价</summary>
        public long Price { get; set; }

        /// <summary>货币类型</summary>
        public string CurrencyType { get; set; } = "Gold";

        /// <summary>状态</summary>
        public string Status { get; set; } = "Listed";

        /// <summary>上架时间</summary>
        public string CreatedAt { get; set; } = string.Empty;

        /// <summary>过期时间</summary>
        public string ExpireAt { get; set; } = string.Empty;
    }

    /// <summary>
    /// 上架商品请求
    /// </summary>
    public class MarketListDto
    {
        /// <summary>物品类型: Item/Equipment</summary>
        public string ItemType { get; set; } = "Item";

        /// <summary>背包物品ID（道具交易时）</summary>
        public long? InventoryItemId { get; set; }

        /// <summary>装备实例ID（装备交易时）</summary>
        public string? EquipmentInstanceId { get; set; }

        /// <summary>数量（道具用）</summary>
        public int Quantity { get; set; } = 1;

        /// <summary>单价</summary>
        public long Price { get; set; }

        /// <summary>货币类型: Gold/SpiritStone</summary>
        public string CurrencyType { get; set; } = "Gold";
    }

    /// <summary>
    /// 交易历史记录
    /// </summary>
    public class MarketHistoryDto
    {
        /// <summary>记录ID</summary>
        public long Id { get; set; }

        /// <summary>物品名称</summary>
        public string ItemName { get; set; } = string.Empty;

        /// <summary>物品类型</summary>
        public string ItemType { get; set; } = "Item";

        /// <summary>数量</summary>
        public int Quantity { get; set; }

        /// <summary>成交价</summary>
        public long Price { get; set; }

        /// <summary>货币类型</summary>
        public string CurrencyType { get; set; } = "Gold";

        /// <summary>手续费</summary>
        public long Fee { get; set; }

        /// <summary>卖家名称</summary>
        public string SellerName { get; set; } = string.Empty;

        /// <summary>买家ID</summary>
        public string? BuyerId { get; set; }

        /// <summary>成交时间</summary>
        public string? SoldAt { get; set; }

        /// <summary>是否是自己的交易（卖家视角）</summary>
        public bool IsSeller { get; set; }
    }
}
