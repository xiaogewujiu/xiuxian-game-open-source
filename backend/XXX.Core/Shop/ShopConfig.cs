using XXX.Entity;

namespace XXX.Shop
{
    /// <summary>
    /// 商店类型枚举
    /// 定义不同类型的商店
    /// </summary>
    public enum ShopType
    {
        /// <summary>
        /// 杂货铺 - 出售基础道具和材料
        /// 常驻商店，无特殊限制
        /// </summary>
        General,

        /// <summary>
        /// 铁匠铺 - 出售装备强化材料
        /// 售卖强化相关道具
        /// </summary>
        Blacksmith,

        /// <summary>
        /// 魔法商店 - 出售法术相关
        /// 出售法力恢复、法术强化等
        /// </summary>
        Magic,

        /// <summary>
        /// 黑市商人 - 高级道具
        /// 价格昂贵，出售稀有道具
        /// </summary>
        BlackMarket,

        /// <summary>
        /// 限时商店 - 特殊道具
        /// 限时开放，特殊商品
        /// </summary>
        Limited
    }

    /// <summary>
    /// 商品类型
    /// </summary>
    public enum ShopItemType
    {
        /// <summary>
        /// 道具
        /// </summary>
        Item,

        /// <summary>
        /// 装备
        /// </summary>
        Equipment
    }

    /// <summary>
    /// 商店配置类
    /// 定义一个商店的完整配置
    /// </summary>
    public class ShopConfig
    {
        /// <summary>
        /// 商店唯一ID
        /// </summary>
        public string ShopId { get; set; } = string.Empty;

        /// <summary>
        /// 商店名称
        /// </summary>
        public string ShopName { get; set; } = string.Empty;

        /// <summary>
        /// 商店类型
        /// </summary>
        public ShopType ShopType { get; set; }

        /// <summary>
        /// 商店描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 商品列表
        /// </summary>
        public List<ShopItem> Items { get; set; } = [];

        /// <summary>
        /// 开放等级要求
        /// 玩家达到该等级才能访问商店
        /// </summary>
        public int RequiredLevel { get; set; } = 1;

        /// <summary>
        /// VIP等级要求
        /// </summary>
        public int RequiredVipLevel { get; set; } = 0;

        /// <summary>
        /// 商店折扣（0-1，1为无折扣）
        ///</summary>
        public float Discount { get; set; } = 1.0f;

        /// <summary>
        /// 是否开启
        /// </summary>
        public bool IsOpen { get; set; } = true;

        /// <summary>
        /// 是否自动刷新
        /// </summary>
        public bool AutoRefresh { get; set; } = false;

        /// <summary>
        /// 刷新间隔（小时）
        /// 每隔多少小时自动刷新一次
        /// </summary>
        public int RefreshIntervalHours { get; set; } = 24;

        /// <summary>
        /// 上次刷新时间
        /// </summary>
        public DateTime LastRefreshTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 商店图标
        /// </summary>
        public string Icon { get; set; } = string.Empty;

        /// <summary>
        /// 排序（用于UI显示顺序）
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 检查是否可访问
        /// </summary>
        public bool IsAccessible(UserEntity player)
        {
            if (!IsOpen) return false;
            if (player.Level < RequiredLevel) return false;

            // TODO: 检查VIP等级
            // if (player.VipLevel < RequiredVipLevel) return false;

            return true;
        }

        /// <summary>
        /// 检查是否需要刷新
        /// </summary>
        public bool NeedsRefresh()
        {
            if (!AutoRefresh) return false;

            var nextRefresh = LastRefreshTime.AddHours(RefreshIntervalHours);
            return DateTime.Now >= nextRefresh;
        }

        /// <summary>
        /// 刷新商店
        /// </summary>
        public void Refresh()
        {
            LastRefreshTime = DateTime.Now;

            // 刷新库存
            foreach (var item in Items)
            {
                if (item.Stock >= 0)
                {
                    // 重置到初始库存
                    // 这里需要保存初始库存
                    // 简化处理：如果是黑市，刷新商品列表
                    if (ShopType == ShopType.BlackMarket)
                    {
                        // TODO: 重新生成商品列表
                    }
                }
            }

            // TODO: 重新生成限时商品（限时商店）
        }
    }

    /// <summary>
    /// 商品类
    /// 商店中出售的商品
    /// </summary>
    public class ShopItem
    {
        /// <summary>
        /// 商品ID
        /// 道具的ItemTable ID或装备的EquipmentTemplate ID
        /// </summary>
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// 商品类型
        /// </summary>
        public ShopItemType ItemType { get; set; }

        /// <summary>
        /// 原价（NPC收购价）
        /// 玩家出售给商店的价格
        /// </summary>
        public int BasePrice { get; set; }

        /// <summary>
        /// 当前售价
        /// 玩家购买价格（原价 × 商店折扣）
        /// </summary>
        public int CurrentPrice { get; set; }

        /// <summary>
        /// 库存数量
        /// -1 表示无限
        /// </summary>
        public int Stock { get; set; } = -1;

        /// <summary>
        /// 初始库存（用于刷新时恢复）
        /// </summary>
        public int InitialStock { get; set; } = -1;

        /// <summary>
        /// 每日限购数量
        /// -1 表示无限
        /// </summary>
        public int DailyLimit { get; set; } = -1;

        /// <summary>
        /// 今日已购买数量
        /// </summary>
        public int TodayPurchased { get; set; } = 0;

        /// <summary>
        /// 是否可购买
        /// </summary>
        public bool CanPurchase => Stock != 0 && (DailyLimit < 0 || TodayPurchased < DailyLimit);

        /// <summary>
        /// 获取商品名称
        /// </summary>
        public string GetName()
        {
            if (ItemType == ShopItemType.Item)
            {
                return GameData.Items.ContainsKey(ItemId)
                    ? GameData.Items[ItemId].Name
                    : "未知道具";
            }
            else if (ItemType == ShopItemType.Equipment)
            {
                return GameData.EquipmentTemplates.ContainsKey(int.Parse(ItemId))
                    ? GameData.EquipmentTemplates[int.Parse(ItemId)].Name
                    : "未知装备";
            }

            return "未知商品";
        }

        /// <summary>
        /// 获取商品图标
        /// </summary>
        public string GetIcon()
        {
            if (ItemType == ShopItemType.Item)
            {
                return $"Icons/Items/{ItemId}.png";
            }
            else if (ItemType == ShopItemType.Equipment)
            {
                return $"Icons/Equipments/{ItemId}.png";
            }

            return "Icons/default.png";
        }

        /// <summary>
        /// 获取商品品质
        /// </summary>
        public int GetQuality()
        {
            if (ItemType == ShopItemType.Item)
            {
                return GameData.Items.ContainsKey(ItemId)
                    ? GameData.Items[ItemId].Quality
                    : 1;
            }
            else if (ItemType == ShopItemType.Equipment)
            {
                return GameData.EquipmentTemplates.ContainsKey(int.Parse(ItemId))
                    ? (int)GameData.EquipmentTemplates[int.Parse(ItemId)].Quality
                    : 1;
            }

            return 1;
        }

        /// <summary>
        /// 获取购买价格（考虑折扣）
        /// </summary>
        public int GetBuyPrice(float shopDiscount = 1.0f, int vipLevel = 0)
        {
            // 计算VIP折扣
            float vipDiscount = 1.0f - Math.Min(0.25f, vipLevel * 0.05f); // 最高25%折扣

            // 总折扣 = 商店折扣 × VIP折扣
            float totalDiscount = shopDiscount * vipDiscount;

            return (int)(BasePrice * totalDiscount);
        }

        /// <summary>
        /// 获取出售价格（原价的一半）
        /// </summary>
        public int GetSellPrice()
        {
            return (int)(BasePrice * 0.5f);
        }

        /// <summary>
        /// 重置每日购买次数
        /// </summary>
        public void ResetDailyPurchase()
        {
            TodayPurchased = 0;
        }
    }
}
