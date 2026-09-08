namespace XXX.Inventory
{
    /// <summary>
    /// 背包配置类
    /// 定义背包系统的全局配置
    /// </summary>
    public static class InventoryConfig
    {
        /// <summary>
        /// 默认背包容量
        /// </summary>
        public const int DefaultMaxSlots = 50;

        /// <summary>
        /// 背包容量上限
        /// </summary>
        public const int AbsoluteMaxSlots = 200;

        /// <summary>
        /// 每次扩展增加的格子数
        /// </summary>
        public const int ExpansionSlots = 10;

        /// <summary>
        /// 背包扩展所需道具ID
        /// </summary>
        public const string ExpansionItemId = "item_expansion";

        /// <summary>
        /// 不同VIP等级对应的背包容量
        /// </summary>
        public static readonly Dictionary<int, int> VipSlotBonus = new Dictionary<int, int>
        {
            { 0, 0 },   // VIP0: +0格
            { 1, 10 }, // VIP1: +10格
            { 2, 20 }, // VIP2: +20格
            { 3, 30 }, // VIP3: +30格
            { 4, 50 }, // VIP4: +50格
            { 5, 80 }  // VIP5: +80格
        };

        /// <summary>
        /// 根据VIP等级获取额外格子数
        /// </summary>
        /// <param name="vipLevel">VIP等级</param>
        /// <returns>额外格子数</returns>
        public static int GetVipBonusSlots(int vipLevel)
        {
            if (VipSlotBonus.ContainsKey(vipLevel))
            {
                return VipSlotBonus[vipLevel];
            }
            return 0;
        }

        /// <summary>
        /// 计算指定VIP等级的背包容量上限
        /// </summary>
        /// <param name="vipLevel">VIP等级</param>
        /// <returns>容量上限</returns>
        public static int GetMaxSlotsForVip(int vipLevel)
        {
            int baseSlots = DefaultMaxSlots;
            int bonusSlots = GetVipBonusSlots(vipLevel);
            int totalSlots = baseSlots + bonusSlots;

            return System.Math.Min(totalSlots, AbsoluteMaxSlots);
        }

        /// <summary>
        /// 检查是否可以扩展背包
        /// </summary>
        /// <param name="currentSlots">当前格子数</param>
        /// <returns>是否可以扩展</returns>
        public static bool CanExpand(int currentSlots)
        {
            return currentSlots < AbsoluteMaxSlots;
        }

        /// <summary>
        /// 获取扩展所需消耗
        /// </summary>
        /// <param name="currentSlots">当前格子数</param>
        /// <param name="vipLevel">VIP等级</param>
        /// <returns>需要消耗的道具数量</returns>
        public static int GetExpansionCost(int currentSlots, int vipLevel)
        {
            int baseCost = 1; // 基础消耗1张扩展券

            // 背包越大，消耗越多
            int tier = currentSlots / 50;
            int multiplier = tier + 1;

            // VIP等级可以减少消耗
            float vipDiscount = vipLevel * 0.1f; // 每级VIP减少10%消耗

            int cost = (int)(baseCost * multiplier * (1 - vipDiscount));

            return System.Math.Max(1, cost);
        }
    }
}
