namespace XXX.Application.DTOs
{
    /// <summary>
    /// 任务总览展示模型。
    /// </summary>
    public class QuestDto
    {
        /// <summary>
        /// 任务 ID。
        /// </summary>
        public string QuestId { get; set; } = string.Empty;

        /// <summary>
        /// 任务名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 任务描述。
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 任务类型名称。
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// 任务类型枚举值。
        /// </summary>
        public int TypeValue { get; set; }
        /// <summary>
        /// 任务图标。
        /// </summary>
        public string Icon { get; set; } = string.Empty;
        /// <summary>
        /// 接取所需等级。
        /// </summary>
        public int RequiredLevel { get; set; }
        /// <summary>
        /// 当前状态文本。
        /// </summary>
        public string Status { get; set; } = string.Empty;
        /// <summary>
        /// 当前状态枚举值。
        /// </summary>
        public int StatusValue { get; set; }
        /// <summary>
        /// 当前总进度。
        /// </summary>
        public int CurrentProgress { get; set; }
        /// <summary>
        /// 目标总进度。
        /// </summary>
        public int TargetProgress { get; set; }
        /// <summary>
        /// 当前总完成百分比。
        /// </summary>
        public double ProgressPercent { get; set; }
        /// <summary>
        /// 排序值。
        /// </summary>
        public int SortOrder { get; set; }
        /// <summary>
        /// 是否自动提交。
        /// </summary>
        public bool AutoSubmit { get; set; }
        /// <summary>
        /// 时间限制，单位秒。
        /// </summary>
        public int TimeLimit { get; set; }
        /// <summary>
        /// 目标列表。
        /// </summary>
        public List<QuestObjectiveDto> Objectives { get; set; } = [];
        /// <summary>
        /// 奖励列表。
        /// </summary>
        public List<QuestRewardDto> Rewards { get; set; } = [];
        /// <summary>
        /// 接取时间。
        /// </summary>
        public DateTime? AcceptTime { get; set; }
        /// <summary>
        /// 完成时间。
        /// </summary>
        public DateTime? CompleteTime { get; set; }
        /// <summary>
        /// 提交时间。
        /// </summary>
        public DateTime? SubmitTime { get; set; }
    }

    /// <summary>
    /// 单条任务目标的进度展示模型。
    /// </summary>
    public class QuestObjectiveDto
    {
        /// <summary>
        /// 目标序号。
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 目标类型名称。
        /// </summary>
        public string ObjectiveType { get; set; } = string.Empty;

        /// <summary>
        /// 目标类型枚举值。
        /// </summary>
        public int ObjectiveTypeValue { get; set; }

        /// <summary>
        /// 目标对象 ID。
        /// </summary>
        public string TargetId { get; set; } = string.Empty;

        /// <summary>
        /// 当前进度。
        /// </summary>
        public int CurrentProgress { get; set; }

        /// <summary>
        /// 目标进度。
        /// </summary>
        public int TargetProgress { get; set; }

        /// <summary>
        /// 完成百分比。
        /// </summary>
        public double ProgressPercent { get; set; }

        /// <summary>
        /// 是否已完成。
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// 目标描述。
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// 任务奖励的统一展示模型。
    /// </summary>
    public class QuestRewardDto
    {
        /// <summary>
        /// 奖励类型。
        /// </summary>
        public string RewardType { get; set; } = string.Empty;

        /// <summary>
        /// 物品 ID。
        /// </summary>
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// 装备模板 ID。
        /// </summary>
        public int? EquipmentId { get; set; }

        /// <summary>
        /// 奖励名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 奖励数量。
        /// </summary>
        public int Quantity { get; set; }
    }

    /// <summary>
    /// 接取任务请求。
    /// </summary>
    public class AcceptQuestRequestDto
    {
        /// <summary>
        /// 要接取的任务 ID。
        /// </summary>
        public string QuestId { get; set; } = string.Empty;
    }

    /// <summary>
    /// 提交任务请求。
    /// </summary>
    public class SubmitQuestRequestDto
    {
        /// <summary>
        /// 要提交的任务 ID。
        /// </summary>
        public string QuestId { get; set; } = string.Empty;
    }

    /// <summary>
    /// 提交任务后的结算结果。
    /// </summary>
    public class QuestSubmitResultDto
    {
        /// <summary>
        /// 本次提交是否成功。
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 结果提示文案。
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 实际发放奖励列表。
        /// </summary>
        public List<QuestRewardDto> Rewards { get; set; } = [];
    }
}
