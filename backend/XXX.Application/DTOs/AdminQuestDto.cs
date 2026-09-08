namespace XXX.Application.DTOs
{
    /// <summary>
    /// 后台任务配置列表项。
    /// </summary>
    public class AdminQuestListItemDto
    {
        /// <summary>
        /// 任务编号。
        /// </summary>
        public string QuestId { get; set; } = string.Empty;

        /// <summary>
        /// 任务名称。
        /// </summary>
        public string QuestName { get; set; } = string.Empty;

        /// <summary>
        /// 任务类型。
        /// </summary>
        public int QuestType { get; set; }

        /// <summary>
        /// 重置周期。
        /// </summary>
        public int ResetCycle { get; set; }

        /// <summary>
        /// 要求等级。
        /// </summary>
        public int RequiredLevel { get; set; }

        /// <summary>
        /// 是否启用。
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 是否为系统内置任务。
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// 内置版本号。
        /// </summary>
        public string? BuiltInVersion { get; set; }
    }

    /// <summary>
    /// 后台任务配置详情。
    /// </summary>
    public class AdminQuestDetailDto
    {
        /// <summary>
        /// 任务编号。
        /// </summary>
        public string QuestId { get; set; } = string.Empty;

        /// <summary>
        /// 任务名称。
        /// </summary>
        public string QuestName { get; set; } = string.Empty;

        /// <summary>
        /// 任务类型。
        /// </summary>
        public int QuestType { get; set; }

        /// <summary>
        /// 重置周期。
        /// </summary>
        public int ResetCycle { get; set; }

        /// <summary>
        /// 描述。
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 要求等级。
        /// </summary>
        public int RequiredLevel { get; set; }

        /// <summary>
        /// 前置任务编号列表。
        /// </summary>
        public string? PreQuestIds { get; set; }

        /// <summary>
        /// 是否自动接取。
        /// </summary>
        public bool AutoAccept { get; set; }

        /// <summary>
        /// 是否自动提交。
        /// </summary>
        public bool AutoSubmit { get; set; }

        /// <summary>
        /// 时间限制。
        /// </summary>
        public int TimeLimit { get; set; }

        /// <summary>
        /// 奖励经验。
        /// </summary>
        public long RewardExp { get; set; }

        /// <summary>
        /// 奖励金币。
        /// </summary>
        public long RewardGold { get; set; }

        /// <summary>
        /// 奖励灵石。
        /// </summary>
        public long RewardSpiritStone { get; set; }

        /// <summary>
        /// 奖励公会贡献。
        /// </summary>
        public int RewardGuildContribution { get; set; }

        /// <summary>
        /// 奖励道具 JSON。
        /// </summary>
        public string? RewardItemsJson { get; set; }

        /// <summary>
        /// 奖励装备编号列表。
        /// </summary>
        public string? RewardEquipmentIds { get; set; }

        /// <summary>
        /// 目标 JSON。
        /// </summary>
        public string? ObjectivesJson { get; set; }

        /// <summary>
        /// 排序值。
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 是否启用。
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 是否为系统内置任务。
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
