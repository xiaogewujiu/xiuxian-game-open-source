namespace XXX.Application.DTOs
{
    /// <summary>
    /// 后台成就配置列表项。
    /// </summary>
    public class AdminAchievementListItemDto
    {
        /// <summary>
        /// 成就编号。
        /// </summary>
        public string AchievementId { get; set; } = string.Empty;

        /// <summary>
        /// 成就名称。
        /// </summary>
        public string AchievementName { get; set; } = string.Empty;

        /// <summary>
        /// 成就类型。
        /// </summary>
        public int AchievementType { get; set; }

        /// <summary>
        /// 难度。
        /// </summary>
        public int Difficulty { get; set; }

        /// <summary>
        /// 是否启用。
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 是否为系统内置成就。
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// 内置版本号。
        /// </summary>
        public string? BuiltInVersion { get; set; }
    }

    /// <summary>
    /// 后台成就配置详情。
    /// </summary>
    public class AdminAchievementDetailDto
    {
        /// <summary>
        /// 成就编号。
        /// </summary>
        public string AchievementId { get; set; } = string.Empty;

        /// <summary>
        /// 成就名称。
        /// </summary>
        public string AchievementName { get; set; } = string.Empty;

        /// <summary>
        /// 成就类型。
        /// </summary>
        public int AchievementType { get; set; }

        /// <summary>
        /// 难度。
        /// </summary>
        public int Difficulty { get; set; }

        /// <summary>
        /// 描述。
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 分类。
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// 积分。
        /// </summary>
        public int Points { get; set; }

        /// <summary>
        /// 是否隐藏。
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// 前置成就编号列表。
        /// </summary>
        public string? PreAchievementIds { get; set; }

        /// <summary>
        /// 奖励金币。
        /// </summary>
        public long RewardGold { get; set; }

        /// <summary>
        /// 奖励灵石。
        /// </summary>
        public long RewardSpiritStone { get; set; }

        /// <summary>
        /// 奖励经验。
        /// </summary>
        public long RewardExp { get; set; }

        /// <summary>
        /// 奖励称号。
        /// </summary>
        public string? RewardTitle { get; set; }

        /// <summary>
        /// 奖励装备编号列表。
        /// </summary>
        public string? RewardEquipmentIds { get; set; }

        /// <summary>
        /// 奖励道具 JSON。
        /// </summary>
        public string? RewardItemsJson { get; set; }

        /// <summary>
        /// 要求类型。
        /// </summary>
        public int RequirementType { get; set; }

        /// <summary>
        /// 要求目标值。
        /// </summary>
        public long RequirementTargetValue { get; set; }

        /// <summary>
        /// 要求描述。
        /// </summary>
        public string? RequirementDescription { get; set; }

        /// <summary>
        /// 条件 JSON。
        /// </summary>
        public string? RequirementsJson { get; set; }

        /// <summary>
        /// 排序值。
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 是否启用。
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 是否为系统内置成就。
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
        /// 最后更新时间。
        /// </summary>
        public DateTime? LastUpdateTime { get; set; }
    }
}
