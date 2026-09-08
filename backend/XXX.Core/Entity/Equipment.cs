namespace XXX.Entity
{
    /// <summary>
    /// 装备品质
    /// </summary>
    public enum EquipmentQuality
    {
        Common = 1,
        Uncommon = 2,
        Rare = 3,
        Epic = 4,
        Legendary = 5
    }

    /// <summary>
    /// 装备部位
    /// </summary>
    public enum EquipmentSlot
    {
        Weapon,
        Helmet,
        Armor,
        Pants,
        Necklace,
        Ring,
        Boots,
        Treasure
    }

    /// <summary>
    /// 装备战斗流派
    /// </summary>
    public enum CombatStyle
    {
        Neutral = 0,
        Physical = 1,
        Magic = 2
    }

    /// <summary>
    /// 武器类别
    /// </summary>
    public enum WeaponCategory
    {
        None = 0,
        Sword = 1,
        Blade = 2,
        Axe = 3,
        Spear = 4,
        Qin = 5,
        Chess = 6,
        Book = 7,
        Brush = 8
    }

    /// <summary>
    /// 装备模板
    /// 仅用于配置，不直接参与战斗
    /// </summary>
    public class EquipmentTemplate : BaseAttributesTemplate
    {
        public int EquipmentId;

        public string Name = string.Empty;

        public int Level;

        public EquipmentQuality Quality;

        public EquipmentSlot Slot;

        public string Description = string.Empty;

        public CombatStyle CombatStyle;

        public WeaponCategory WeaponCategory;

        public float QualityMultiplier => 1.0f + ((int)Quality - 1) * 0.1f;

        public string? IconPath;
    }

    /// <summary>
    /// 装备实例
    /// 由模板随机生成，真实存放在玩家背包中
    /// </summary>
    public class EquipmentInstance : BaseAttributes
    {
        public EquipmentTemplate? Template;

        public string InstanceId { get; set; } = string.Empty;

        public int EnhanceLevel { get; set; }

        public int Level { get; set; } = 1;

        public int EquipmentId => Template?.EquipmentId ?? 0;

        public string Name => Template?.Name ?? string.Empty;

        public EquipmentQuality Quality => Template?.Quality ?? EquipmentQuality.Common;

        public List<AttributeProperty> RerolledAttrs { get; set; } = [];

        public int RerollCount { get; set; }

        public List<AttributeProperty> Attributes = [];

        /// <summary>
        /// 获取装备实例的总属性，包含强化与洗练加成。
        /// </summary>
        public BaseAttributes GetTotalAttributes()
        {
            var total = new BaseAttributes
            {
                Type1 = Type1,
                Type2 = Type2,
                Type3 = Type3,
                Type4 = Type4,
                Type5 = Type5,
                Type6 = Type6,
                Type7 = Type7,
                Type8 = Type8,
                Type9 = Type9,
                Type10 = Type10,
                Type11 = Type11,
                Type12 = Type12,
                Type13 = Type13,
                Type14 = Type14,
                Type15 = Type15
            };

            var enhanceBonus = 1.0f + EnhanceLevel * 0.05f;
            total.Type1 = (int)(total.Type1 * enhanceBonus);
            total.Type2 = (int)(total.Type2 * enhanceBonus);
            total.Type3 = (int)(total.Type3 * enhanceBonus);
            total.Type4 = (int)(total.Type4 * enhanceBonus);
            total.Type5 = (int)(total.Type5 * enhanceBonus);
            total.Type6 = (int)(total.Type6 * enhanceBonus);
            total.Type7 = (int)(total.Type7 * enhanceBonus);

            foreach (var attr in RerolledAttrs)
            {
                ApplyAttribute(total, attr);
            }

            return total;
        }

        private static void ApplyAttribute(BaseAttributes target, AttributeProperty attr)
        {
            switch (attr.type)
            {
                case AttributeType.Type1: target.Type1 += (int)attr.value; break;
                case AttributeType.Type2: target.Type2 += (int)attr.value; break;
                case AttributeType.Type3: target.Type3 += (int)attr.value; break;
                case AttributeType.Type4: target.Type4 += (int)attr.value; break;
                case AttributeType.Type5: target.Type5 += (int)attr.value; break;
                case AttributeType.Type6: target.Type6 += (int)attr.value; break;
                case AttributeType.Type7: target.Type7 += (int)attr.value; break;
                case AttributeType.Type8: target.Type8 += attr.value; break;
                case AttributeType.Type9: target.Type9 += attr.value; break;
                case AttributeType.Type10: target.Type10 += attr.value; break;
                case AttributeType.Type11: target.Type11 += attr.value; break;
                case AttributeType.Type12: target.Type12 += attr.value; break;
                case AttributeType.Type13: target.Type13 += attr.value; break;
                case AttributeType.Type14: target.Type14 += attr.value; break;
                case AttributeType.Type15: target.Type15 += attr.value; break;
            }
        }

        /// <summary>
        /// 获取带品质与强化等级的装备显示名。
        /// </summary>
        public string GetDisplayName()
        {
            var enhanceStr = EnhanceLevel > 0 ? $" +{EnhanceLevel}" : string.Empty;
            var qualityStr = GetQualityName(Template?.Quality ?? EquipmentQuality.Common);
            return $"[{qualityStr}] {Template?.Name ?? string.Empty}{enhanceStr}";
        }

        private static string GetQualityName(EquipmentQuality quality)
        {
            return quality switch
            {
                EquipmentQuality.Common => "普通",
                EquipmentQuality.Uncommon => "优秀",
                EquipmentQuality.Rare => "精良",
                EquipmentQuality.Epic => "史诗",
                EquipmentQuality.Legendary => "传说",
                _ => "普通"
            };
        }
    }
}
