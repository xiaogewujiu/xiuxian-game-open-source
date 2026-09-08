namespace XXX.Application.DTOs
{
    /// <summary>
    /// 后台道具列表项。
    /// </summary>
    public class AdminItemListItemDto
    {
        /// <summary>
        /// 道具编号。
        /// </summary>
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// 道具名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 使用等级。
        /// </summary>
        public int UseLevel { get; set; }

        /// <summary>
        /// 类型。
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 品质。
        /// </summary>
        public int Quality { get; set; }

        /// <summary>
        /// 图片路径。
        /// </summary>
        public string? IconPath { get; set; }

        /// <summary>
        /// 是否为内置种子道具。
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// 内置版本号。
        /// </summary>
        public string? BuiltInVersion { get; set; }

        /// <summary>
        /// 是否可交易。
        /// </summary>
        public bool IsTradeable { get; set; } = true;
    }

    /// <summary>
    /// 后台道具详情。
    /// </summary>
    public class AdminItemDetailDto
    {
        /// <summary>
        /// 道具编号。
        /// </summary>
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// 道具名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 使用等级。
        /// </summary>
        public int UseLevel { get; set; }

        /// <summary>
        /// 类型。
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 描述。
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 最大堆叠数。
        /// </summary>
        public int MaxStack { get; set; }

        /// <summary>
        /// 品质。
        /// </summary>
        public int Quality { get; set; }

        /// <summary>
        /// 图片路径。
        /// </summary>
        public string? IconPath { get; set; }

        /// <summary>
        /// 是否为内置种子道具。
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// 内置种子键。
        /// </summary>
        public string? SeedKey { get; set; }

        /// <summary>
        /// 内置版本号。
        /// </summary>
        public string? BuiltInVersion { get; set; }

        /// <summary>
        /// 最近一次编辑时间。
        /// </summary>
        public DateTime? LastUpdateTime { get; set; }

        /// <summary>
        /// 是否可交易。
        /// </summary>
        public bool IsTradeable { get; set; } = true;

        /// <summary>
        /// 道具箱配置。
        /// </summary>
        public AdminItemChestConfigDto? ChestConfig { get; set; }

        /// <summary>
        /// 技能书配置。
        /// </summary>
        public AdminItemSkillBookConfigDto? SkillBookConfig { get; set; }

        /// <summary>
        /// 宠物蛋配置。
        /// </summary>
        public AdminItemPetEggConfigDto? PetEggConfig { get; set; }

        /// <summary>
        /// 丹药配置。
        /// </summary>
        public AdminItemPillConfigDto? PillConfig { get; set; }

        /// <summary>
        /// 配方卷轴绑定配置。
        /// </summary>
        public AdminItemRecipeUnlockConfigDto? RecipeUnlockConfig { get; set; }
    }

    /// <summary>
    /// 道具箱配置。
    /// </summary>
    public class AdminItemChestConfigDto
    {
        /// <summary>
        /// 开启模式。
        /// </summary>
        public int OpenMode { get; set; } = 1;

        /// <summary>
        /// 每次开启抽取次数。
        /// </summary>
        public int RollCount { get; set; } = 1;

        /// <summary>
        /// 奖励池配置。
        /// </summary>
        public List<AdminItemChestRewardDto> Rewards { get; set; } = [];
    }

    /// <summary>
    /// 道具箱奖励配置。
    /// </summary>
    public class AdminItemChestRewardDto
    {
        /// <summary>
        /// 奖励类型。
        /// </summary>
        public int RewardType { get; set; } = 3;

        /// <summary>
        /// 奖励目标编号。
        /// </summary>
        public string? TargetId { get; set; }

        /// <summary>
        /// 最小奖励数量。
        /// </summary>
        public int MinCount { get; set; } = 1;

        /// <summary>
        /// 最大奖励数量。
        /// </summary>
        public int MaxCount { get; set; } = 1;

        /// <summary>
        /// 抽取权重。
        /// </summary>
        public int Weight { get; set; } = 100;

        /// <summary>
        /// 奖励说明。
        /// </summary>
        public string? Description { get; set; }
    }

    /// <summary>
    /// 技能书配置。
    /// </summary>
    public class AdminItemSkillBookConfigDto
    {
        /// <summary>
        /// 关联技能编号。
        /// </summary>
        public int SkillId { get; set; }
    }

    /// <summary>
    /// 配方卷轴绑定配置。
    /// </summary>
    public class AdminItemRecipeUnlockConfigDto
    {
        public string RecipeType { get; set; } = string.Empty;

        public string RecipeId { get; set; } = string.Empty;
    }

    /// <summary>
    /// 宠物蛋配置。
    /// </summary>
    public class AdminItemPetEggConfigDto
    {
        /// <summary>
        /// 关联宠物模板编号。
        /// </summary>
        public string PetTemplateId { get; set; } = string.Empty;
    }

    /// <summary>
    /// 丹药配置。
    /// </summary>
    public class AdminItemPillConfigDto
    {
        /// <summary>
        /// 丹药效果类型。
        /// </summary>
        public int EffectType { get; set; } = 2;

        /// <summary>
        /// 突破成功率加成百分比。
        /// </summary>
        public int BreakthroughBonusPercent { get; set; }

        /// <summary>
        /// 直接增加的经验值。
        /// </summary>
        public int ExpGain { get; set; }

        /// <summary>
        /// 影响的属性类型键。
        /// </summary>
        public string? AttributeType { get; set; }

        /// <summary>
        /// 属性加成数值。
        /// </summary>
        public double AttributeValue { get; set; }

        /// <summary>
        /// 时效分钟。0 表示永久生效。
        /// </summary>
        public int DurationMinutes { get; set; }

        /// <summary>
        /// 最大使用次数。0 表示不限制。
        /// </summary>
        public int MaxUsageCount { get; set; }

        /// <summary>
        /// 恢复HP百分比。仅 RestoreHPMP 类型有效。
        /// </summary>
        public int HealHpPercent { get; set; }

        /// <summary>
        /// 恢复MP百分比。仅 RestoreHPMP 类型有效。
        /// </summary>
        public int HealMpPercent { get; set; }
    }
}
