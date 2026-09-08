namespace XXX.Application.DTOs
{
    /// <summary>
    /// 成就DTO
    /// </summary>
    public class AchievementDto
    {
        /// <summary>
        /// 成就ID
        /// </summary>
        public string AchievementId { get; set; } = string.Empty;

        /// <summary>
        /// 成就名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 成就描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 成就类型
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// 成就类型枚举值。
        /// </summary>
        public int TypeValue { get; set; }

        /// <summary>
        /// 成就图标
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// 难度文本
        /// </summary>
        public string Difficulty { get; set; } = string.Empty;

        /// <summary>
        /// 难度数值
        /// </summary>
        public int DifficultyValue { get; set; }

        /// <summary>
        /// 分类
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// 成就点数
        /// </summary>
        public int Points { get; set; }

        /// <summary>
        /// 当前进度
        /// </summary>
        public long CurrentProgress { get; set; }

        /// <summary>
        /// 目标进度
        /// </summary>
        public long TargetProgress { get; set; }

        /// <summary>
        /// 是否已完成
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// 是否已领取奖励
        /// </summary>
        public bool IsRewardClaimed { get; set; }

        /// <summary>
        /// 状态文本
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// 状态数值
        /// </summary>
        public int StatusValue { get; set; }

        /// <summary>
        /// 是否隐藏成就
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// 排序值
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 完成时间
        /// </summary>
        public DateTime? CompleteTime { get; set; }

        /// <summary>
        /// 领取时间
        /// </summary>
        public DateTime? ClaimTime { get; set; }

        /// <summary>
        /// 条件列表
        /// </summary>
        public List<AchievementRequirementDto> Requirements { get; set; } = [];

        /// <summary>
        /// 奖励列表
        /// </summary>
        public List<AchievementRewardDto> Rewards { get; set; } = [];
    }

    /// <summary>
    /// 成就单条条件 DTO。
    /// </summary>
    public class AchievementRequirementDto
    {
        /// <summary>
        /// 条件序号。
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 条件类型名称。
        /// </summary>
        public string RequirementType { get; set; } = string.Empty;

        /// <summary>
        /// 条件类型枚举值。
        /// </summary>
        public int RequirementTypeValue { get; set; }

        /// <summary>
        /// 当前进度值。
        /// </summary>
        public long CurrentProgress { get; set; }

        /// <summary>
        /// 目标进度值。
        /// </summary>
        public long TargetProgress { get; set; }

        /// <summary>
        /// 当前完成百分比。
        /// </summary>
        public double ProgressPercent { get; set; }

        /// <summary>
        /// 该条件是否已完成。
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// 条件描述文本。
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// 成就奖励DTO
    /// </summary>
    public class AchievementRewardDto
    {
        /// <summary>
        /// 奖励类型
        /// </summary>
        public string RewardType { get; set; } = string.Empty;

        /// <summary>
        /// 奖励数量
        /// </summary>
        public long Amount { get; set; }

        /// <summary>
        /// 物品ID（如果是物品奖励）
        /// </summary>
        public string? ItemId { get; set; }

        /// <summary>
        /// 装备模板ID
        /// </summary>
        public int? EquipmentId { get; set; }

        /// <summary>
        /// 奖励名称
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}
