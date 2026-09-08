using XXX.Entity;

namespace XXX.Equipment
{
    /// <summary>
    /// 装备洗练系统
    /// 负责装备属性的随机重置和优化
    /// </summary>
    public class RerollSystem
    {
        private static Random Rand = Random.Shared;

        /// <summary>
        /// 洗练装备属性
        /// </summary>
        /// <param name="equipment">装备实例</param>
        /// <param name="count">洗练次数</param>
        /// <returns>洗练结果</returns>
        public static RerollResult Reroll(EquipmentInstance equipment, int count = 1)
        {
            if (equipment == null || equipment.Template == null)
            {
                return new RerollResult
                {
                    Success = false,
                    Message = "装备无效",
                    NewAttributes = []
                };
            }

            // 根据品质确定洗练属性数量
            int maxAttrs = GetMaxRerollAttributes(equipment.Template.Quality);

            // 清空旧属性
            equipment.RerolledAttrs.Clear();

            // 生成新属性
            int actualCount = 0;
            for (int i = 0; i < maxAttrs; i++)
            {
                var attr = GenerateRandomAttribute(equipment.Template);
                if (attr != null)
                {
                    equipment.RerolledAttrs.Add(attr);
                    actualCount++;
                }
            }

            // 增加洗练次数
            equipment.RerollCount += count;

            return new RerollResult
            {
                Success = true,
                Message = $"洗练完成，获得 {actualCount} 条新属性",
                NewAttributes = [.. equipment.RerolledAttrs],
                RerollCount = equipment.RerollCount
            };
        }

        /// <summary>
        /// 锁定属性洗练
        /// </summary>
        /// <param name="equipment">装备实例</param>
        /// <param name="lockedIndices">锁定的属性索引列表</param>
        /// <returns>洗练结果</returns>
        public static RerollResult RerollWithLock(EquipmentInstance equipment,
            List<int> lockedIndices)
        {
            if (equipment == null || equipment.Template == null)
            {
                return new RerollResult
                {
                    Success = false,
                    Message = "装备无效",
                    NewAttributes = []
                };
            }

            // 获取锁定的属性
            var lockedAttrs = lockedIndices
                .Where(i => i >= 0 && i < equipment.RerolledAttrs.Count)
                .Select(i => equipment.RerolledAttrs[i])
                .ToList();

            // 清空旧属性
            equipment.RerolledAttrs.Clear();

            // 添加锁定属性
            equipment.RerolledAttrs.AddRange(lockedAttrs);

            // 生成新属性填充剩余位置
            int maxAttrs = GetMaxRerollAttributes(equipment.Template.Quality);
            for (int i = equipment.RerolledAttrs.Count; i < maxAttrs; i++)
            {
                var attr = GenerateRandomAttribute(equipment.Template);
                if (attr != null)
                {
                    equipment.RerolledAttrs.Add(attr);
                }
            }

            // 增加洗练次数
            equipment.RerollCount++;

            return new RerollResult
            {
                Success = true,
                Message = $"洗练完成，保留了 {lockedAttrs.Count} 条属性",
                NewAttributes = [.. equipment.RerolledAttrs],
                LockedCount = lockedAttrs.Count,
                RerollCount = equipment.RerollCount
            };
        }

        /// <summary>
        /// 替换指定索引的属性
        /// </summary>
        /// <param name="equipment">装备实例</param>
        /// <param name="index">属性索引</param>
        /// <returns>洗练结果</returns>
        public static RerollResult ReplaceAttribute(EquipmentInstance equipment, int index)
        {
            if (equipment == null || equipment.Template == null)
            {
                return new RerollResult
                {
                    Success = false,
                    Message = "装备无效",
                    NewAttributes = []
                };
            }

            if (index < 0 || index >= equipment.RerolledAttrs.Count)
            {
                return new RerollResult
                {
                    Success = false,
                    Message = "属性索引无效",
                    NewAttributes = []
                };
            }

            // 生成新属性替换
            var newAttr = GenerateRandomAttribute(equipment.Template);
            if (newAttr == null)
            {
                return new RerollResult
                {
                    Success = false,
                    Message = "生成属性失败",
                    NewAttributes = []
                };
            }

            var oldAttr = equipment.RerolledAttrs[index];
            equipment.RerolledAttrs[index] = newAttr;

            // 增加洗练次数
            equipment.RerollCount++;

            return new RerollResult
            {
                Success = true,
                Message = $"属性替换成功",
                NewAttributes = [.. equipment.RerolledAttrs],
                ReplacedIndex = index,
                OldAttribute = oldAttr,
                RerollCount = equipment.RerollCount
            };
        }

        /// <summary>
        /// 获取最大洗练属性数量
        /// </summary>
        /// <param name="quality">装备品质</param>
        /// <returns>最大属性数量</returns>
        public static int GetMaxRerollAttributes(EquipmentQuality quality)
        {
            switch (quality)
            {
                case EquipmentQuality.Common: return 1;
                case EquipmentQuality.Uncommon: return 2;
                case EquipmentQuality.Rare: return 3;
                case EquipmentQuality.Epic: return 4;
                case EquipmentQuality.Legendary: return 5;
                default: return 1;
            }
        }

        /// <summary>
        /// 生成随机属性
        /// </summary>
        /// <param name="template">装备模板</param>
        /// <returns>属性</returns>
        private static AttributeProperty GenerateRandomAttribute(EquipmentTemplate template)
        {
            // 可随机生成的属性类型
            var availableTypes = new List<AttributeType>
            {
                AttributeType.Type1,  // 血量
                AttributeType.Type2,  // 蓝量
                AttributeType.Type3,  // 物攻
                AttributeType.Type4,  // 法攻
                AttributeType.Type5,  // 物防
                AttributeType.Type6,  // 法防
                AttributeType.Type7,  // 速度
                AttributeType.Type8,  // 命中
                AttributeType.Type9,  // 闪避
                AttributeType.Type10, // 暴击
                AttributeType.Type11, // 暴击伤害
                AttributeType.Type12, // 连击
                AttributeType.Type13, // 反击
                AttributeType.Type14, // 破甲
                AttributeType.Type15  // 额外伤害
            };

            // 随机选择一个属性类型
            var type = availableTypes[Rand.Next(availableTypes.Count)];

            // 根据品质生成属性值
            float value = GenerateAttributeValue(type, template.Quality);

            return new AttributeProperty
            {
                type = type,
                value = value
            };
        }

        /// <summary>
        /// 生成属性值
        /// </summary>
        /// <param name="type">属性类型</param>
        /// <param name="quality">装备品质</param>
        /// <returns>属性值</returns>
        private static float GenerateAttributeValue(AttributeType type, EquipmentQuality quality)
        {
            float baseValue = 0;

            // 根据属性类型设置基础值
            switch (type)
            {
                case AttributeType.Type1: // 血量
                    baseValue = Rand.Next(50, 150);
                    break;
                case AttributeType.Type2: // 蓝量
                    baseValue = Rand.Next(30, 100);
                    break;
                case AttributeType.Type3: // 物攻
                case AttributeType.Type4: // 法攻
                    baseValue = Rand.Next(10, 40);
                    break;
                case AttributeType.Type5: // 物防
                case AttributeType.Type6: // 法防
                    baseValue = Rand.Next(8, 30);
                    break;
                case AttributeType.Type7: // 速度
                    baseValue = Rand.Next(5, 20);
                    break;
                case AttributeType.Type8:  // 命中
                case AttributeType.Type9:  // 闪避
                case AttributeType.Type10: // 暴击
                case AttributeType.Type12: // 连击
                case AttributeType.Type13: // 反击
                case AttributeType.Type14: // 破甲
                    baseValue = Rand.Next(1, 8) / 100f; // 1%-8%
                    break;
                case AttributeType.Type11: // 暴击伤害
                    baseValue = Rand.Next(5, 20) / 100f; // 5%-20%
                    break;
                case AttributeType.Type15: // 额外伤害
                    baseValue = Rand.Next(2, 10) / 100f; // 2%-10%
                    break;
            }

            // 品质倍率（每个品质增加20%）
            float qualityMultiplier = 1.0f + ((int)quality - 1) * 0.2f;

            return baseValue * qualityMultiplier;
        }

        /// <summary>
        /// 获取属性类型名称
        /// </summary>
        /// <param name="type">属性类型</param>
        /// <returns>属性名称</returns>
        public static string GetAttributeName(AttributeType type)
        {
            switch (type)
            {
                case AttributeType.Type1: return "最大血量";
                case AttributeType.Type2: return "最大蓝量";
                case AttributeType.Type3: return "物理攻击";
                case AttributeType.Type4: return "法术攻击";
                case AttributeType.Type5: return "物理防御";
                case AttributeType.Type6: return "法术防御";
                case AttributeType.Type7: return "速度";
                case AttributeType.Type8: return "命中率";
                case AttributeType.Type9: return "闪避率";
                case AttributeType.Type10: return "暴击率";
                case AttributeType.Type11: return "暴击伤害";
                case AttributeType.Type12: return "连击率";
                case AttributeType.Type13: return "反击率";
                case AttributeType.Type14: return "破甲率";
                case AttributeType.Type15: return "额外伤害";
                default: return "未知属性";
            }
        }

        /// <summary>
        /// 格式化属性值显示
        /// </summary>
        /// <param name="type">属性类型</param>
        /// <param name="value">属性值</param>
        /// <returns>格式化后的字符串</returns>
        public static string FormatAttributeValue(AttributeType type, float value)
        {
            bool isPercentage = (int)type >= 8; // Type8及以上是百分比属性

            if (isPercentage)
            {
                return $"{value * 100:F1}%";
            }
            else
            {
                return $"{(int)value}";
            }
        }
    }

    /// <summary>
    /// 洗练结果
    /// </summary>
    public class RerollResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 结果消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 新属性列表
        /// </summary>
        public List<AttributeProperty> NewAttributes { get; set; } = [];

        /// <summary>
        /// 锁定的属性数量
        /// </summary>
        public int LockedCount { get; set; }

        /// <summary>
        /// 替换的属性索引
        /// </summary>
        public int ReplacedIndex { get; set; }

        /// <summary>
        /// 被替换的旧属性
        /// </summary>
        public AttributeProperty OldAttribute { get; set; } = new AttributeProperty();

        /// <summary>
        /// 总洗练次数
        /// </summary>
        public int RerollCount { get; set; }
    }
}
