using XXX.Entity;

namespace XXX.Inventory
{
    /// <summary>
    /// 背包物品类型枚举
    /// 区分背包中物品的类型
    /// </summary>
    public enum InventoryItemType
    {
        /// <summary>
        /// 道具 - 消耗品、材料等
        /// </summary>
        Item,

        /// <summary>
        /// 装备 - 武器、防具等
        /// </summary>
        Equipment
    }

    /// <summary>
    /// 背包物品类
    /// 统一表示背包中的道具和装备
    ///
    /// 设计说明：
    /// 1. 使用 InventoryItemType 区分道具和装备
    /// 2. 统一的 ItemId 作为唯一标识
    /// 3. 支持数量堆叠（道具）
    /// 4. 支持单件存储（装备）
    /// </summary>
    public class InventoryItem
    {
        /// <summary>
        /// 物品唯一ID
        /// 用于标识背包中的每一个物品
        /// </summary>
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// 物品类型
        /// </summary>
        public InventoryItemType Type { get; set; }

        /// <summary>
        /// 数量
        /// 对于道具：表示堆叠数量
        /// 对于装备：固定为1
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 获取时间
        /// </summary>
        public DateTime ObtainTime { get; set; } = DateTime.Now;

        // ==================== 道具相关字段 ====================

        /// <summary>
        /// 道具模板ID
        /// 当 Type 为 Item 时使用
        /// </summary>
        public string ItemTemplateId { get; set; } = string.Empty;

        /// <summary>
        /// 获取道具模板
        /// </summary>
        public ItemTable? GetItemTemplate()
        {
            if (Type != InventoryItemType.Item) return null;
            if (string.IsNullOrEmpty(ItemTemplateId)) return null;

            return GameData.Items.ContainsKey(ItemTemplateId)
                ? GameData.Items[ItemTemplateId]
                : null;
        }

        // ==================== 装备相关字段 ====================

        /// <summary>
        /// 装备实例
        /// 当 Type 为 Equipment 时使用
        /// </summary>
        public EquipmentInstance? Equipment { get; set; }

        // ==================== 便利属性 ====================

        /// <summary>
        /// 获取显示名称
        /// </summary>
        public string GetDisplayName()
        {
            if (Type == InventoryItemType.Item)
            {
                var template = GetItemTemplate();
                return template != null ? template.Name : "未知道具";
            }
            else if (Type == InventoryItemType.Equipment && Equipment != null)
            {
                return Equipment.GetDisplayName();
            }

            return "未知物品";
        }

        /// <summary>
        /// 获取描述信息
        /// </summary>
        public string GetDescription()
        {
            if (Type == InventoryItemType.Item)
            {
                var template = GetItemTemplate();
                return template?.Description ?? "";
            }
            else if (Type == InventoryItemType.Equipment && Equipment?.Template != null)
            {
                return Equipment.Template.Description ?? "";
            }

            return "";
        }

        /// <summary>
        /// 获取图标路径
        /// </summary>
        public string GetIconPath()
        {
            if (Type == InventoryItemType.Item)
            {
                return $"Icons/Items/{ItemTemplateId}.png";
            }
            else if (Type == InventoryItemType.Equipment)
            {
                return Equipment?.Template != null
                    ? $"Icons/Equipments/{Equipment.Template.EquipmentId}.png"
                    : "Icons/default.png";
            }

            return "Icons/default.png";
        }

        /// <summary>
        /// 获取品质
        /// </summary>
        public int GetQuality()
        {
            if (Type == InventoryItemType.Item)
            {
                var template = GetItemTemplate();
                return template?.Quality ?? 1;
            }
            else if (Type == InventoryItemType.Equipment && Equipment?.Template != null)
            {
                return (int)Equipment.Template.Quality;
            }

            return 1;
        }

        /// <summary>
        /// 获取品质名称
        /// </summary>
        public string GetQualityName()
        {
            int quality = GetQuality();
            return GetQualityText(quality);
        }

        /// <summary>
        /// 获取品质文本
        /// </summary>
        /// <param name="quality">品质值</param>
        /// <returns>品质文本</returns>
        private string GetQualityText(int quality)
        {
            switch (quality)
            {
                case 1: return "普通";
                case 2: return "优秀";
                case 3: return "稀有";
                case 4: return "史诗";
                case 5: return "传说";
                case 6: return "神话";
                case 7: return "神圣";
                default: return "普通";
            }
        }

        /// <summary>
        /// 获取品质颜色代码
        /// </summary>
        public string GetQualityColor()
        {
            int quality = GetQuality();
            return GetQualityColorCode(quality);
        }

        /// <summary>
        /// 获取品质颜色代码
        /// </summary>
        private string GetQualityColorCode(int quality)
        {
            switch (quality)
            {
                case 1: return "#9E9E9E"; // 灰色
                case 2: return "#4CAF50"; // 绿色
                case 3: return "#2196F3"; // 蓝色
                case 4: return "#9C27B0"; // 紫色
                case 5: return "#FF9800"; // 橙色
                case 6: return "#F44336"; // 红色
                case 7: return "#FFD700"; // 金色
                default: return "#9E9E9E";
            }
        }

        /// <summary>
        /// 是否可堆叠
        /// </summary>
        public bool IsStackable()
        {
            if (Type == InventoryItemType.Item)
            {
                var template = GetItemTemplate();
                return template?.MaxStack > 1;
            }

            return false; // 装备不可堆叠
        }

        /// <summary>
        /// 获取最大堆叠数
        /// </summary>
        public int GetMaxStack()
        {
            if (Type == InventoryItemType.Item)
            {
                var template = GetItemTemplate();
                return template?.MaxStack ?? 1;
            }

            return 1;
        }

        /// <summary>
        /// 检查物品是否有效
        /// </summary>
        public bool IsValid()
        {
            if (string.IsNullOrEmpty(ItemId)) return false;
            if (Count <= 0) return false;

            if (Type == InventoryItemType.Item)
            {
                return GetItemTemplate() != null;
            }
            else if (Type == InventoryItemType.Equipment)
            {
                return Equipment != null && Equipment.Template != null;
            }

            return false;
        }

        /// <summary>
        /// 获取详细文本信息
        /// </summary>
        public string GetDetailText()
        {
            if (Type == InventoryItemType.Item)
            {
                var template = GetItemTemplate();
                if (template == null) return "无效道具";

                return $"[{template.Name}]\n" +
                       $"类型：{template.Type}\n" +
                       $"品质：{GetQualityName()}\n" +
                       $"数量：{Count}\n" +
                       $"描述：{template.Description}";
            }
            else if (Type == InventoryItemType.Equipment && Equipment != null)
            {
                return Equipment.GetDisplayName();
            }

            return "未知物品";
        }

        /// <summary>
        /// 创建道具类型的背包物品
        /// </summary>
        public static InventoryItem CreateItem(string templateId, int count = 1)
        {
            return new InventoryItem
            {
                ItemId = Guid.NewGuid().ToString("N"),
                Type = InventoryItemType.Item,
                ItemTemplateId = templateId,
                Count = count,
                ObtainTime = DateTime.Now
            };
        }

        /// <summary>
        /// 创建装备类型的背包物品
        /// </summary>
        public static InventoryItem CreateEquipment(EquipmentInstance equipment)
        {
            return new InventoryItem
            {
                ItemId = equipment.InstanceId,
                Type = InventoryItemType.Equipment,
                Count = 1,
                Equipment = equipment,
                ObtainTime = DateTime.Now
            };
        }
    }
}
