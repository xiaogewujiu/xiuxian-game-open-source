using XXX.Entity;

namespace XXX.Inventory
{
    /// <summary>
    /// 背包管理器
    /// 负责道具和装备的存储、管理、操作
    ///
    /// 功能说明：
    /// 1. 统一管理道具和装备
    /// 2. 支持道具堆叠
    /// 3. 背包容量管理
    /// 4. 背包扩展
    /// 5. 物品排序和整理
    /// </summary>
    public class InventoryManager
    {
        /// <summary>
        /// 所有物品列表（道具+装备）
        /// </summary>
        private List<InventoryItem> items = [];

        // ==================== 初始化 ====================

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="initialSlots">初始背包容量（默认50格）</param>
        public InventoryManager(int initialSlots = 50)
        {
            MaxSlots = initialSlots;
        }

        /// <summary>
        /// 构造函数（使用默认容量）
        /// </summary>
        public InventoryManager() : this(50)
        {
        }

        // ==================== 属性 ====================

        /// <summary>
        /// 背包容量上限
        /// </summary>
        public int MaxSlots { get; private set; }

        /// <summary>
        /// 已用格子数
        /// </summary>
        public int UsedSlots => items.Count;

        /// <summary>
        /// 空余格子数
        /// </summary>
        public int EmptySlots => MaxSlots - items.Count;

        // ==================== 添加物品 ====================

        /// <summary>
        /// 添加道具
        /// </summary>
        /// <param name="itemId">道具模板ID</param>
        /// <param name="count">数量</param>
        /// <returns>是否成功添加</returns>
        public bool AddItem(string itemId, int count = 1)
        {
            // 检查道具是否存在
            if (!GameData.Items.ContainsKey(itemId))
            {
                return false;
            }

            var template = GameData.Items[itemId];

            // 可堆叠道具
            if (template.MaxStack > 1)
            {
                // 查找背包中是否已有该道具
                var existingItem = items.FirstOrDefault(i =>
                    i.Type == InventoryItemType.Item &&
                    i.ItemTemplateId == itemId);

                if (existingItem != null)
                {
                    // 已有该道具，检查是否可以堆叠
                    int newCount = existingItem.Count + count;
                    if (newCount <= template.MaxStack)
                    {
                        // 可以堆叠
                        existingItem.Count = newCount;
                        return true;
                    }
                    else
                    {
                        // 超过堆叠上限
                        int remaining = count - (template.MaxStack - existingItem.Count);
                        existingItem.Count = template.MaxStack;

                        // 需要额外格子
                        if (EmptySlots <= 0)
                        {
                            return false;
                        }

                        // 创建新的堆叠
                        items.Add(InventoryItem.CreateItem(itemId, remaining));
                        return true;
                    }
                }
                else
                {
                    // 没有该道具，添加新格子
                    if (EmptySlots <= 0)
                    {
                        return false;
                    }

                    // 确保数量不超过堆叠上限
                    int actualCount = Math.Min(count, template.MaxStack);
                    items.Add(InventoryItem.CreateItem(itemId, actualCount));

                    // 如果添加的数量超过堆叠上限，递归添加剩余部分
                    if (count > template.MaxStack)
                    {
                        return AddItem(itemId, count - template.MaxStack);
                    }

                    return true;
                }
            }
            else
            {
                // 不可堆叠道具（每个占一个格子）
                for (int i = 0; i < count; i++)
                {
                    if (EmptySlots <= 0)
                    {
                        return false;
                    }

                    items.Add(InventoryItem.CreateItem(itemId, 1));
                }

                return true;
            }
        }

        /// <summary>
        /// 添加装备
        /// </summary>
        /// <param name="equipment">装备实例</param>
        /// <returns>是否成功添加</returns>
        public bool AddEquipment(EquipmentInstance equipment)
        {
            if (equipment == null || equipment.Template == null)
            {
                return false;
            }

            // 装备需要确保有唯一ID
            if (string.IsNullOrEmpty(equipment.InstanceId))
            {
                equipment.InstanceId = Guid.NewGuid().ToString("N");
            }

            // 检查背包是否已满
            if (EmptySlots <= 0)
            {
                return false;
            }

            // 添加到背包
            items.Add(InventoryItem.CreateEquipment(equipment));
            return true;
        }

        /// <summary>
        /// 添加背包物品（通用方法）
        /// </summary>
        /// <param name="inventoryItem">背包物品</param>
        /// <returns>是否成功添加</returns>
        public bool AddItem(InventoryItem inventoryItem)
        {
            if (inventoryItem == null || !inventoryItem.IsValid())
            {
                return false;
            }

            if (EmptySlots <= 0)
            {
                return false;
            }

            items.Add(inventoryItem);
            return true;
        }

        // ==================== 移除物品 ====================

        /// <summary>
        /// 移除道具
        /// </summary>
        /// <param name="itemId">道具模板ID</param>
        /// <param name="count">数量</param>
        /// <returns>是否成功移除</returns>
        public bool RemoveItem(string itemId, int count = 1)
        {
            if (count <= 0)
            {
                return false;
            }

            // 查找所有匹配的道具
            var matchingItems = items
                .Where(i => i.Type == InventoryItemType.Item && i.ItemTemplateId == itemId)
                .ToList();

            // 计算总数量
            int totalCount = matchingItems.Sum(i => i.Count);
            if (totalCount < count)
            {
                return false;
            }

            // 移除逻辑
            int remainingToRemove = count;

            foreach (var item in matchingItems.ToList())
            {
                if (remainingToRemove <= 0) break;

                if (item.Count <= remainingToRemove)
                {
                    // 移除整个物品
                    remainingToRemove -= item.Count;
                    items.Remove(item);
                }
                else
                {
                    // 减少数量
                    item.Count -= remainingToRemove;
                    remainingToRemove = 0;
                }
            }

            return true;
        }

        /// <summary>
        /// 移除装备
        /// </summary>
        /// <param name="instanceId">装备实例ID</param>
        /// <returns>是否成功移除</returns>
        public bool RemoveEquipment(string instanceId)
        {
            var item = items.FirstOrDefault(i =>
                i.Type == InventoryItemType.Equipment &&
                i.ItemId == instanceId);

            if (item == null)
            {
                return false;
            }

            return items.Remove(item);
        }

        /// <summary>
        /// 移除指定ID的背包物品
        /// </summary>
        /// <param name="itemId">背包物品ID</param>
        /// <returns>是否成功移除</returns>
        public bool RemoveItemById(string itemId)
        {
            var item = items.FirstOrDefault(i => i.ItemId == itemId);
            if (item == null)
            {
                return false;
            }

            return items.Remove(item);
        }

        // ==================== 查询操作 ====================

        /// <summary>
        /// 获取道具数量
        /// </summary>
        /// <param name="itemId">道具模板ID</param>
        /// <returns>数量</returns>
        public int GetItemCount(string itemId)
        {
            return items
                .Where(i => i.Type == InventoryItemType.Item && i.ItemTemplateId == itemId)
                .Sum(i => i.Count);
        }

        /// <summary>
        /// 获取所有物品
        /// </summary>
        /// <returns>物品列表</returns>
        public List<InventoryItem> GetAllItems()
        {
            return [.. items];
        }

        /// <summary>
        /// 按类型获取物品
        /// </summary>
        /// <param name="type">物品类型</param>
        /// <returns>物品列表</returns>
        public List<InventoryItem> GetItemsByType(InventoryItemType type)
        {
            return items.Where(i => i.Type == type).ToList();
        }

        /// <summary>
        /// 获取所有道具
        /// </summary>
        /// <returns>道具列表</returns>
        public List<InventoryItem> GetItems()
        {
            return GetItemsByType(InventoryItemType.Item);
        }

        /// <summary>
        /// 获取所有装备
        /// </summary>
        /// <returns>装备列表</returns>
        public List<EquipmentInstance> GetEquipments()
        {
            return items
                .Where(i => i.Type == InventoryItemType.Equipment)
                .Select(i => i.Equipment)
                .Where(e => e != null)
                .Select(e => e!)
                .ToList();
        }

        /// <summary>
        /// 查找装备
        /// </summary>
        /// <param name="instanceId">装备实例ID</param>
        /// <returns>装备实例，不存在返回null</returns>
        public EquipmentInstance? GetEquipment(string instanceId)
        {
            var item = items.FirstOrDefault(i =>
                i.Type == InventoryItemType.Equipment &&
                i.ItemId == instanceId);

            return item?.Equipment;
        }

        /// <summary>
        /// 查找背包物品
        /// </summary>
        /// <param name="itemId">背包物品ID</param>
        /// <returns>背包物品，不存在返回null</returns>
        public InventoryItem? GetItem(string itemId)
        {
            return items.FirstOrDefault(i => i.ItemId == itemId);
        }

        /// <summary>
        /// 检查道具是否存在
        /// </summary>
        /// <param name="itemId">道具模板ID</param>
        /// <returns>是否存在</returns>
        public bool HasItem(string itemId)
        {
            return items.Any(i => i.Type == InventoryItemType.Item && i.ItemTemplateId == itemId);
        }

        /// <summary>
        /// 检查装备是否存在
        /// </summary>
        /// <param name="instanceId">装备实例ID</param>
        /// <returns>是否存在</returns>
        public bool HasEquipment(string instanceId)
        {
            return items.Any(i =>
                i.Type == InventoryItemType.Equipment &&
                i.ItemId == instanceId);
        }

        // ==================== 背包管理 ====================

        /// <summary>
        /// 扩展背包容量
        /// </summary>
        /// <param name="additionalSlots">增加的格子数</param>
        /// <returns>是否成功</returns>
        public bool ExpandSlots(int additionalSlots)
        {
            if (additionalSlots <= 0)
            {
                return false;
            }

            int newMaxSlots = MaxSlots + additionalSlots;

            // 检查是否超过上限
            const int MAX_SLOTS = 200;
            if (newMaxSlots > MAX_SLOTS)
            {
                newMaxSlots = MAX_SLOTS;
            }

            MaxSlots = newMaxSlots;
            return true;
        }

        /// <summary>
        /// 整理背包（自动合并同类可堆叠道具）
        /// </summary>
        public void Organize()
        {
            // 按道具模板ID分组
            var groupedItems = items
                .Where(i => i.Type == InventoryItemType.Item)
                .GroupBy(i => i.ItemTemplateId)
                .ToList();

            foreach (var group in groupedItems)
            {
                var template = GameData.Items[group.Key];
                if (template == null || template.MaxStack <= 1) continue;

                // 获取该组的所有物品
                var groupItems = group.ToList();

                // 计算总数量
                int totalCount = groupItems.Sum(i => i.Count);

                // 移除所有旧物品
                foreach (var item in groupItems)
                {
                    items.Remove(item);
                }

                // 重新添加（合并）
                int remaining = totalCount;
                while (remaining > 0)
                {
                    int stackSize = Math.Min(remaining, template.MaxStack);
                    items.Add(InventoryItem.CreateItem(group.Key, stackSize));
                    remaining -= stackSize;
                }
            }
        }

        /// <summary>
        /// 排序物品
        /// </summary>
        /// <param name="sortType">排序类型</param>
        public void Sort(InventorySortType sortType)
        {
            switch (sortType)
            {
                case InventorySortType.Type:
                    // 按类型：道具在前，装备在后
                    items = items.OrderBy(i => i.Type).ToList();
                    break;

                case InventorySortType.Quality:
                    // 按品质：高品质在前
                    items = items.OrderByDescending(i => i.GetQuality()).ToList();
                    break;

                case InventorySortType.Name:
                    // 按名称：字母顺序
                    items = items.OrderBy(i => i.GetDisplayName()).ToList();
                    break;

                case InventorySortType.ObtainTime:
                    // 按获取时间：新获取在前
                    items = items.OrderByDescending(i => i.ObtainTime).ToList();
                    break;
            }
        }

        /// <summary>
        /// 清空背包
        /// </summary>
        public void Clear()
        {
            items.Clear();
        }

        /// <summary>
        /// 获取背包信息
        /// </summary>
        /// <returns>背包信息</returns>
        public InventoryInfo GetInfo()
        {
            return new InventoryInfo
            {
                MaxSlots = MaxSlots,
                UsedSlots = UsedSlots,
                EmptySlots = EmptySlots,
                ItemCount = GetItems().Count,
                EquipmentCount = GetEquipments().Count
            };
        }

        /// <summary>
        /// 批量添加道具
        /// </summary>
        /// <param name="items">道具ID和数量的字典</param>
        /// <returns>成功添加的道具数量</returns>
        public int AddItems(Dictionary<string, int> items)
        {
            int successCount = 0;

            foreach (var kvp in items)
            {
                if (AddItem(kvp.Key, kvp.Value))
                {
                    successCount++;
                }
            }

            return successCount;
        }
    }

    /// <summary>
    /// 排序类型枚举
    /// </summary>
    public enum InventorySortType
    {
        /// <summary>
        /// 按类型排序
        /// </summary>
        Type,

        /// <summary>
        /// 按品质排序
        /// </summary>
        Quality,

        /// <summary>
        /// 按名称排序
        /// </summary>
        Name,

        /// <summary>
        /// 按获取时间排序
        /// </summary>
        ObtainTime
    }

    /// <summary>
    /// 背包信息类
    /// </summary>
    public class InventoryInfo
    {
        /// <summary>
        /// 最大格子数
        /// </summary>
        public int MaxSlots { get; set; }

        /// <summary>
        /// 已用格子数
        /// </summary>
        public int UsedSlots { get; set; }

        /// <summary>
        /// 空余格子数
        /// </summary>
        public int EmptySlots { get; set; }

        /// <summary>
        /// 道具数量
        /// </summary>
        public int ItemCount { get; set; }

        /// <summary>
        /// 装备数量
        /// </summary>
        public int EquipmentCount { get; set; }
    }
}
