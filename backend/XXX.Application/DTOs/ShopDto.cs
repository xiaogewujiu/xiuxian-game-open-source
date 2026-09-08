namespace XXX.Application.DTOs
{
    /// <summary>
    /// 商店展示 DTO。
    /// 该对象专门给前端页面使用，字段会尽量展开，避免前端还要再拼装领域对象。
    /// </summary>
    public class ShopDto
    {
        /// <summary>
        /// 商店唯一 ID。
        /// </summary>
        public string ShopId { get; set; } = string.Empty;

        /// <summary>
        /// 商店名称。
        /// </summary>
        public string ShopName { get; set; } = string.Empty;

        /// <summary>
        /// 商店类型文本，便于前端直接展示。
        /// </summary>
        public string ShopType { get; set; } = string.Empty;

        /// <summary>
        /// 商店描述。
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 商店图标。
        /// 当前后端仅返回一个稳定的字符串标识，前端可据此映射本地图标或占位图。
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// 商店是否开放。
        /// </summary>
        public bool IsOpen { get; set; }

        /// <summary>
        /// 进入商店所需的最低等级。
        /// </summary>
        public int RequiredLevel { get; set; }

        /// <summary>
        /// 商店折扣系数。
        /// 1 表示原价，0.8 表示八折。
        /// </summary>
        public float Discount { get; set; }

        /// <summary>
        /// 是否会自动刷新商品库存。
        /// </summary>
        public bool AutoRefresh { get; set; }

        /// <summary>
        /// 自动刷新间隔，单位为小时。
        /// </summary>
        public int RefreshIntervalHours { get; set; }

        /// <summary>
        /// 当前商店可见的商品列表。
        /// </summary>
        public List<ShopItemDto> Items { get; set; } = [];
    }

    /// <summary>
    /// 商店商品 DTO。
    /// 这里会把“商品元数据 + 当前库存 + 玩家当日已购数量”一次性返回给前端。
    /// </summary>
    public class ShopItemDto
    {
        /// <summary>
        /// 商品 ID。
        /// 对道具来说是道具 ID；对装备来说是装备模板 ID 的字符串形式。
        /// </summary>
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// 模板 ID。
        /// 当前与 ItemId 保持一致，单独保留字段是为了前端后续扩展时不需要改协议。
        /// </summary>
        public string TemplateId { get; set; } = string.Empty;

        /// <summary>
        /// 商品名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 商品描述。
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 商品图标标识。
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// 商品类型文本。
        /// 典型值为 Item / Equipment。
        /// </summary>
        public string ItemType { get; set; } = string.Empty;

        /// <summary>
        /// 品质数值。
        /// </summary>
        public int Quality { get; set; }

        /// <summary>
        /// 品质文本，前端可直接用于展示。
        /// </summary>
        public string QualityName { get; set; } = string.Empty;

        /// <summary>
        /// 购买价格信息。
        /// </summary>
        public ShopPriceDto Price { get; set; } = new();

        /// <summary>
        /// 出售价。
        /// 该字段主要用于前端在“商店页”上顺带提示玩家出售回收价。
        /// </summary>
        public int SellPrice { get; set; }

        /// <summary>
        /// 当前库存。
        /// -1 表示不限库存。
        /// </summary>
        public int Stock { get; set; }

        /// <summary>
        /// 每日限购数量。
        /// -1 表示不限购。
        /// </summary>
        public int DailyLimit { get; set; }

        /// <summary>
        /// 玩家今天已经购买的数量。
        /// </summary>
        public int PurchasedToday { get; set; }

        /// <summary>
        /// 玩家今天还能继续购买的数量。
        /// -1 表示不限购。
        /// </summary>
        public int RemainingDailyLimit { get; set; }

        /// <summary>
        /// 是否已售罄。
        /// 这里把不限库存视为未售罄。
        /// </summary>
        public bool IsSoldOut { get; set; }


        /// <summary>
        /// 购买所需的最低等级。
        /// 这个等级可能来自商店要求，也可能来自道具/装备自身模板要求。
        /// </summary>
        public int RequiredLevel { get; set; }

        /// <summary>
        /// 购买所需的最低 VIP 等级。
        /// 当前版本暂未启用 VIP 体系，但字段保留以避免后续协议变化。
        /// </summary>
        public int RequiredVipLevel { get; set; }

        /// <summary>
        /// 当前玩家是否允许购买。
        /// 该结果已经综合了开放状态、等级要求、库存与限购条件。
        /// </summary>
        public bool CanPurchase { get; set; }

        /// <summary>
        /// 商品属性展示列表。
        /// 装备商品会返回基础属性范围，道具通常为空。
        /// </summary>
        public List<ShopItemStatDto> Stats { get; set; } = [];
    }

    /// <summary>
    /// 商店商品属性展示 DTO。
    /// </summary>
    public class ShopItemStatDto
    {
        /// <summary>
        /// 属性名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 属性展示值。
        /// </summary>
        public string Value { get; set; } = string.Empty;
    }

    /// <summary>
    /// 商品价格 DTO。
    /// </summary>
    public class ShopPriceDto
    {
        /// <summary>
        /// 货币类型。
        /// 当前最小闭环统一使用 Gold。
        /// </summary>
        public string CurrencyType { get; set; } = string.Empty;

        /// <summary>
        /// 当前价格。
        /// </summary>
        public long Amount { get; set; }

        /// <summary>
        /// 原价。
        /// 当商店存在折扣时返回，便于前端展示划线价；无折扣时返回 null。
        /// </summary>
        public long? OriginalAmount { get; set; }

        /// <summary>
        /// 折扣百分比。
        /// 例如八折会返回 80。
        /// </summary>
        public int? DiscountPercent { get; set; }
    }

    /// <summary>
    /// 购买商品请求 DTO。
    /// </summary>
    public class BuyItemRequestDto
    {
        /// <summary>
        /// 商店 ID。
        /// </summary>
        public string ShopId { get; set; } = string.Empty;

        /// <summary>
        /// 商品 ID。
        /// </summary>
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// 购买数量。
        /// 装备购买时固定应为 1。
        /// </summary>
        public int Count { get; set; } = 1;
    }

    /// <summary>
    /// 购买商品结果 DTO。
    /// </summary>
    public class BuyItemResultDto
    {
        /// <summary>
        /// 是否购买成功。
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 结果消息。
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 购买后的背包物品。
        /// 当前主流程实际返回 ShopBuyResult，该类型保留给后续更细粒度的前端交互扩展。
        /// </summary>
        public InventoryItemDto? Item { get; set; }

        /// <summary>
        /// 消耗的货币类型。
        /// </summary>
        public string CurrencyType { get; set; } = string.Empty;

        /// <summary>
        /// 消耗的货币数量。
        /// </summary>
        public long CostAmount { get; set; }

        /// <summary>
        /// 购买后的剩余货币数量。
        /// </summary>
        public long RemainingAmount { get; set; }
    }

    /// <summary>
    /// 出售道具请求 DTO。
    /// </summary>
    public class SellItemRequestDto
    {
        /// <summary>
        /// 道具 ID。
        /// </summary>
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// 出售数量。
        /// </summary>
        public int Count { get; set; } = 1;
    }

    /// <summary>
    /// 出售道具结果 DTO。
    /// </summary>
    public class SellItemResultDto
    {
        /// <summary>
        /// 是否出售成功。
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 结果消息。
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 获得的金币数量。
        /// </summary>
        public long GoldEarned { get; set; }

        /// <summary>
        /// 当前金币总数。
        /// </summary>
        public long CurrentGold { get; set; }
    }

    /// <summary>
    /// 批量出售背包道具请求。
    /// </summary>
    public class BatchSellInventoryItemsRequestDto
    {
        public List<BatchSellInventoryItemEntryDto> Items { get; set; } = [];
    }

    public class BatchSellInventoryItemEntryDto
    {
        public string ItemId { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class BatchSellInventoryItemsResultDto
    {
        public int SoldItemCount { get; set; }
        public int SoldQuantity { get; set; }
        public int FailedItemCount { get; set; }
        public int GoldEarned { get; set; }
    }
}
