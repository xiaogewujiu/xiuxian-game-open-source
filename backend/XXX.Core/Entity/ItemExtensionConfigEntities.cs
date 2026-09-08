using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 宝箱扩展配置。
    /// </summary>
    [SugarTable("ItemChestConfigs")]
    public class ItemChestConfigEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 100)]
        public string ItemId { get; set; } = string.Empty;

        [SugarColumn]
        public int OpenMode { get; set; }

        [SugarColumn]
        public int RollCount { get; set; } = 1;

        [SugarColumn]
        public bool IsEnabled { get; set; } = true;

        [SugarColumn]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 宝箱奖励条目扩展配置。
    /// </summary>
    [SugarTable("ItemChestRewardEntries")]
    public class ItemChestRewardEntryEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 100)]
        public string GID { get; set; } = string.Empty;

        [SugarColumn(Length = 100)]
        public string ItemId { get; set; } = string.Empty;

        [SugarColumn]
        public int RewardType { get; set; }

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? TargetId { get; set; }

        [SugarColumn]
        public int MinCount { get; set; } = 1;

        [SugarColumn]
        public int MaxCount { get; set; } = 1;

        [SugarColumn]
        public int Weight { get; set; } = 100;


        [SugarColumn(Length = 200, IsNullable = true)]
        public string? Description { get; set; }

        [SugarColumn]
        public int SortOrder { get; set; } = 0;
    }

    /// <summary>
    /// 技能书扩展配置。
    /// </summary>
    [SugarTable("ItemSkillBookConfigs")]
    public class ItemSkillBookConfigEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 100)]
        public string ItemId { get; set; } = string.Empty;

        [SugarColumn]
        public int SkillId { get; set; }

        [SugarColumn]
        public bool IsEnabled { get; set; } = true;

        [SugarColumn]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 配方卷轴扩展配置。
    /// </summary>
    [SugarTable("ItemRecipeUnlockConfigs")]
    public class ItemRecipeUnlockConfigEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 100)]
        public string ItemId { get; set; } = string.Empty;

        [SugarColumn(Length = 20, IsNullable = false)]
        public string RecipeType { get; set; } = string.Empty;

        [SugarColumn(Length = 100, IsNullable = false)]
        public string RecipeId { get; set; } = string.Empty;

        [SugarColumn]
        public bool IsEnabled { get; set; } = true;

        [SugarColumn]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 宠物蛋扩展配置。
    /// </summary>
    [SugarTable("ItemPetEggConfigs")]
    public class ItemPetEggConfigEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 100)]
        public string ItemId { get; set; } = string.Empty;

        [SugarColumn(Length = 100)]
        public string PetTemplateId { get; set; } = string.Empty;

        [SugarColumn]
        public bool IsEnabled { get; set; } = true;

        [SugarColumn]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 丹药扩展配置。
    /// </summary>
    [SugarTable("ItemPillConfigs")]
    public class ItemPillConfigEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 100)]
        public string ItemId { get; set; } = string.Empty;

        [SugarColumn]
        public int EffectType { get; set; }

        [SugarColumn]
        public int BreakthroughBonusPercent { get; set; }

        [SugarColumn]
        public int ExpGain { get; set; }

        [SugarColumn(Length = 50, IsNullable = true)]
        public string? AttributeType { get; set; }

        [SugarColumn]
        public double AttributeValue { get; set; }

        [SugarColumn]
        public int DurationMinutes { get; set; }

        [SugarColumn]
        public int MaxUsageCount { get; set; }

        [SugarColumn]
        public int HealHpPercent { get; set; }

        [SugarColumn]
        public int HealMpPercent { get; set; }

        [SugarColumn]
        public bool IsEnabled { get; set; } = true;

        [SugarColumn]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }
}
