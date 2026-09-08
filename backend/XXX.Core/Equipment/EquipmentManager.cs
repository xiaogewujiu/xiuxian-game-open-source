using XXX.Entity;
using XXX.Inventory;

namespace XXX.Equipment
{
    /// <summary>
    /// 装备管理器
    /// 负责装备的穿戴、卸下和属性管理
    /// </summary>
    public class EquipmentManager
    {
        /// <summary>
        /// 玩家装备栏
        /// </summary>
        public Dictionary<EquipmentSlot, EquipmentInstance> EquippedItems { get; private set; }

        /// <summary>
        /// 背包管理器（统一管理道具和装备）
        /// </summary>
        public InventoryManager InventoryManager { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public EquipmentManager()
        {
            EquippedItems = [];
            InventoryManager = new InventoryManager();
        }

        /// <summary>
        /// 构造函数（指定背包初始容量）
        /// </summary>
        /// <param name="inventorySlots">背包初始容量</param>
        public EquipmentManager(int inventorySlots)
        {
            EquippedItems = [];
            InventoryManager = new InventoryManager(inventorySlots);
        }

        /// <summary>
        /// 穿戴装备
        /// </summary>
        /// <param name="player">玩家实体</param>
        /// <param name="equipment">装备实例</param>
        /// <returns>穿戴结果</returns>
        public EquipResult EquipItem(UserEntity player, EquipmentInstance equipment)
        {
            // 检查装备是否为空
            if (equipment == null || equipment.Template == null)
            {
                return new EquipResult
                {
                    Success = false,
                    Message = "装备无效"
                };
            }

            // 检查等级要求
            if (player.Level < equipment.Template.Level)
            {
                return new EquipResult
                {
                    Success = false,
                    Message = $"需要等级 {equipment.Template.Level}"
                };
            }

            var slot = equipment.Template.Slot;

            // 如果该槽位已有装备，先卸下
            if (EquippedItems.ContainsKey(slot))
            {
                var oldEquipment = EquippedItems[slot];
                UnequipItem(player, slot, addToInventory: false);
                InventoryManager.AddEquipment(oldEquipment);
            }

            // 从背包移除
            InventoryManager.RemoveEquipment(equipment.InstanceId);

            // 穿戴装备
            EquippedItems[slot] = equipment;

            // 应用装备属性到玩家
            ApplyEquipmentAttributes(player, equipment);

            return new EquipResult
            {
                Success = true,
                Message = $"成功穿戴 {equipment.GetDisplayName()}"
            };
        }

        /// <summary>
        /// 卸下装备
        /// </summary>
        /// <param name="player">玩家实体</param>
        /// <param name="slot">装备槽位</param>
        /// <param name="addToInventory">是否加入背包</param>
        /// <returns>卸下结果</returns>
        public EquipResult UnequipItem(UserEntity player, EquipmentSlot slot, bool addToInventory = true)
        {
            if (!EquippedItems.ContainsKey(slot))
            {
                return new EquipResult
                {
                    Success = false,
                    Message = "该槽位没有装备"
                };
            }

            var equipment = EquippedItems[slot];

            // 移除装备属性
            RemoveEquipmentAttributes(player, equipment);

            // 从装备栏移除
            EquippedItems.Remove(slot);

            // 放入背包
            if (addToInventory)
            {
                InventoryManager.AddEquipment(equipment);
            }

            return new EquipResult
            {
                Success = true,
                Message = $"成功卸下 {equipment.GetDisplayName()}"
            };
        }

        /// <summary>
        /// 添加装备到背包
        /// </summary>
        /// <param name="equipment">装备实例</param>
        /// <returns>是否成功添加</returns>
        public bool AddToInventory(EquipmentInstance equipment)
        {
            return InventoryManager.AddEquipment(equipment);
        }

        /// <summary>
        /// 从背包移除装备
        /// </summary>
        /// <param name="instanceId">装备实例ID</param>
        /// <returns>是否成功移除</returns>
        public bool RemoveFromInventory(string instanceId)
        {
            return InventoryManager.RemoveEquipment(instanceId);
        }

        /// <summary>
        /// 获取背包中的装备
        /// </summary>
        /// <param name="instanceId">装备实例ID</param>
        /// <returns>装备实例</returns>
        public EquipmentInstance? GetInventoryEquipment(string instanceId)
        {
            return InventoryManager.GetEquipment(instanceId);
        }

        /// <summary>
        /// 获取背包中的所有装备
        /// </summary>
        /// <returns>装备列表</returns>
        public List<EquipmentInstance> GetAllEquipments()
        {
            return InventoryManager.GetEquipments();
        }

        /// <summary>
        /// 添加道具到背包
        /// </summary>
        /// <param name="itemId">道具模板ID</param>
        /// <param name="count">数量</param>
        /// <returns>是否成功添加</returns>
        public bool AddItem(string itemId, int count = 1)
        {
            return InventoryManager.AddItem(itemId, count);
        }

        /// <summary>
        /// 从背包移除道具
        /// </summary>
        /// <param name="itemId">道具模板ID</param>
        /// <param name="count">数量</param>
        /// <returns>是否成功移除</returns>
        public bool RemoveItem(string itemId, int count = 1)
        {
            return InventoryManager.RemoveItem(itemId, count);
        }

        /// <summary>
        /// 获取道具数量
        /// </summary>
        /// <param name="itemId">道具模板ID</param>
        /// <returns>数量</returns>
        public int GetItemCount(string itemId)
        {
            return InventoryManager.GetItemCount(itemId);
        }

        /// <summary>
        /// 获取已穿戴的装备
        /// </summary>
        /// <param name="slot">装备槽位</param>
        /// <returns>装备实例</returns>
        public EquipmentInstance? GetEquippedItem(EquipmentSlot slot)
        {
            return EquippedItems.ContainsKey(slot) ? EquippedItems[slot] : null;
        }

        /// <summary>
        /// 应用装备属性到玩家
        /// </summary>
        private void ApplyEquipmentAttributes(UserEntity player, EquipmentInstance equipment)
        {
            var attrs = equipment.GetTotalAttributes();

            player.Type1 += attrs.Type1;
            player.Type2 += attrs.Type2;
            player.Type3 += attrs.Type3;
            player.Type4 += attrs.Type4;
            player.Type5 += attrs.Type5;
            player.Type6 += attrs.Type6;
            player.Type7 += attrs.Type7;

            player.Type8 += attrs.Type8;
            player.Type9 += attrs.Type9;
            player.Type10 += attrs.Type10;
            player.Type11 += attrs.Type11;
            player.Type12 += attrs.Type12;
            player.Type13 += attrs.Type13;
            player.Type14 += attrs.Type14;
            player.Type15 += attrs.Type15;
        }

        /// <summary>
        /// 从玩家移除装备属性
        /// </summary>
        private void RemoveEquipmentAttributes(UserEntity player, EquipmentInstance equipment)
        {
            var attrs = equipment.GetTotalAttributes();

            player.Type1 -= attrs.Type1;
            player.Type2 -= attrs.Type2;
            player.Type3 -= attrs.Type3;
            player.Type4 -= attrs.Type4;
            player.Type5 -= attrs.Type5;
            player.Type6 -= attrs.Type6;
            player.Type7 -= attrs.Type7;

            player.Type8 -= attrs.Type8;
            player.Type9 -= attrs.Type9;
            player.Type10 -= attrs.Type10;
            player.Type11 -= attrs.Type11;
            player.Type12 -= attrs.Type12;
            player.Type13 -= attrs.Type13;
            player.Type14 -= attrs.Type14;
            player.Type15 -= attrs.Type15;
        }

        /// <summary>
        /// 获取所有装备的总加成属性
        /// </summary>
        /// <returns>总属性</returns>
        public BaseAttributes GetTotalEquipmentAttributes()
        {
            var total = new BaseAttributes();

            foreach (var equipment in EquippedItems.Values)
            {
                var attrs = equipment.GetTotalAttributes();

                total.Type1 += attrs.Type1;
                total.Type2 += attrs.Type2;
                total.Type3 += attrs.Type3;
                total.Type4 += attrs.Type4;
                total.Type5 += attrs.Type5;
                total.Type6 += attrs.Type6;
                total.Type7 += attrs.Type7;

                total.Type8 += attrs.Type8;
                total.Type9 += attrs.Type9;
                total.Type10 += attrs.Type10;
                total.Type11 += attrs.Type11;
                total.Type12 += attrs.Type12;
                total.Type13 += attrs.Type13;
                total.Type14 += attrs.Type14;
                total.Type15 += attrs.Type15;
            }

            return total;
        }

        /// <summary>
        /// 重新计算并应用所有装备属性
        /// </summary>
        /// <param name="player">玩家实体</param>
        public void RefreshAllEquipmentAttributes(UserEntity player)
        {
            // 先移除所有装备属性
            var equippedList = EquippedItems.Values.ToList();
            foreach (var equipment in equippedList)
            {
                RemoveEquipmentAttributes(player, equipment);
            }

            // 重新应用所有装备属性
            foreach (var equipment in equippedList)
            {
                ApplyEquipmentAttributes(player, equipment);
            }
        }

        /// <summary>
        /// 获取背包信息
        /// </summary>
        /// <returns>背包信息</returns>
        public XXX.Inventory.InventoryInfo GetInventoryInfo()
        {
            return InventoryManager.GetInfo();
        }

        /// <summary>
        /// 扩展背包容量
        /// </summary>
        /// <param name="additionalSlots">增加的格子数</param>
        /// <returns>是否成功</returns>
        public bool ExpandInventory(int additionalSlots)
        {
            return InventoryManager.ExpandSlots(additionalSlots);
        }

        /// <summary>
        /// 整理背包
        /// </summary>
        public void OrganizeInventory()
        {
            InventoryManager.Organize();
        }

        /// <summary>
        /// 排序背包
        /// </summary>
        /// <param name="sortType">排序类型</param>
        public void SortInventory(InventorySortType sortType)
        {
            InventoryManager.Sort(sortType);
        }

        /// <summary>
        /// 获取背包中所有物品（道具+装备）
        /// </summary>
        /// <returns>物品列表</returns>
        public List<XXX.Inventory.InventoryItem> GetAllItems()
        {
            return InventoryManager.GetAllItems();
        }

        /// <summary>
        /// 检查道具是否存在
        /// </summary>
        /// <param name="itemId">道具模板ID</param>
        /// <returns>是否存在</returns>
        public bool HasItem(string itemId)
        {
            return InventoryManager.HasItem(itemId);
        }

        /// <summary>
        /// 清空背包
        /// </summary>
        public void ClearInventory()
        {
            InventoryManager.Clear();
        }
    }

    /// <summary>
    /// 装备操作结果
    /// </summary>
    public class EquipResult
    {
        /// <summary>
        /// 操作是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 结果消息，描述操作结果详情
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
