namespace XXX.Entity
{
    /// <summary>
    /// 道具/材料模板。
    /// </summary>
    public class ItemTable
    {
        public string ItemId = string.Empty;
        public string Name = string.Empty;
        public int UseLevel;
        public ItemType Type;
        public string Description = string.Empty;
        public int MaxStack;
        public int Quality;
        public string? IconPath;
        public ItemChestConfig? ChestConfig;
        public ItemSkillBookConfig? SkillBookConfig;
        public ItemPetEggConfig? PetEggConfig;
        public ItemPillConfig? PillConfig;
        public ItemFavorabilityGiftConfig? FavorabilityGiftConfig;
    }

    /// <summary>
    /// 物品类型。
    /// </summary>
    public enum ItemType
    {
        Material = 0,
        Consumable = 1,
        Quest = 2,
        Chest = 10,
        SkillBook = 11,
        Pill = 12,
        Seed = 13,
        PetEgg = 14,
        FavorabilityGift = 15,
        Gem = 16,
        RecipeScroll = 17,
        InventoryExpansion = 18
    }

    /// <summary>
    /// 道具箱开启模式。
    /// </summary>
    public enum ChestOpenMode
    {
        SinglePick = 1,
        MultiRoll = 2
    }

    /// <summary>
    /// 道具箱奖励类型。
    /// </summary>
    public enum ChestRewardType
    {
        Gold = 1,
        Item = 3,
        Equipment = 4,
        SpiritStone = 5
    }

    /// <summary>
    /// 道具箱配置。
    /// </summary>
    public class ItemChestConfig
    {
        public ChestOpenMode OpenMode { get; set; } = ChestOpenMode.SinglePick;

        public int RollCount { get; set; } = 1;

        public List<ItemChestRewardEntry> Rewards { get; set; } = [];
    }

    /// <summary>
    /// 道具箱奖励条目。
    /// </summary>
    public class ItemChestRewardEntry
    {
        public ChestRewardType RewardType { get; set; } = ChestRewardType.Item;

        public string? TargetId { get; set; }

        public int MinCount { get; set; } = 1;

        public int MaxCount { get; set; } = 1;

        public int Weight { get; set; } = 100;


        public string? Description { get; set; }
    }

    /// <summary>
    /// 技能书配置。
    /// </summary>
    public class ItemSkillBookConfig
    {
        public int SkillId { get; set; }
    }

    /// <summary>
    /// 宠物蛋配置。
    /// </summary>
    public class ItemPetEggConfig
    {
        public string PetTemplateId { get; set; } = string.Empty;
    }

    /// <summary>
    /// 丹药效果类型。
    /// </summary>
    public enum ItemPillEffectType
    {
        BreakthroughChance = 1,
        AddExp = 2,
        AddAttribute = 3,
        RestoreHPMP = 4
    }

    /// <summary>
    /// 丹药配置。
    /// </summary>
    public class ItemPillConfig
    {
        public ItemPillEffectType EffectType { get; set; } = ItemPillEffectType.AddExp;

        public int BreakthroughBonusPercent { get; set; }

        public int ExpGain { get; set; }

        public AttributeType? AttributeType { get; set; }

        public double AttributeValue { get; set; }

        /// <summary>
        /// 时效分钟。0 表示永久生效，大于 0 表示持续分钟数。
        /// 仅 BreakthroughChance 和 AddAttribute 支持，AddExp 强制为 0。
        /// </summary>
        public int DurationMinutes { get; set; }

        /// <summary>
        /// 最大使用次数。0 表示不限制，大于 0 表示最多使用次数。
        /// </summary>
        public int MaxUsageCount { get; set; }

        /// <summary>
        /// 恢复HP百分比（按最大HP的百分比恢复）。仅 RestoreHPMP 类型有效。
        /// </summary>
        public int HealHpPercent { get; set; }

        /// <summary>
        /// 恢复MP百分比（按最大MP的百分比恢复）。仅 RestoreHPMP 类型有效。
        /// </summary>
        public int HealMpPercent { get; set; }
    }
}
