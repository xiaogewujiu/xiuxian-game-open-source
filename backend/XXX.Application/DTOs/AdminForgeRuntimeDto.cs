namespace XXX.Application.DTOs
{
    /// <summary>
    /// 后台锻造系统列表项。
    /// </summary>
    public class AdminForgeSystemListItemDto
    {
        /// <summary>
        /// 玩家编号。
        /// </summary>
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 玩家名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 玩家账号。
        /// </summary>
        public string Account { get; set; } = string.Empty;

        /// <summary>
        /// 锻造职业等级。
        /// </summary>
        public int BlacksmithLevel { get; set; }

        /// <summary>
        /// 当前五行阵允许达到的职业等级上限。
        /// </summary>
        public int ProfessionLevelCap { get; set; }

        /// <summary>
        /// 当前是否正在锻造。
        /// </summary>
        public bool IsForging { get; set; }

        /// <summary>
        /// 正在锻造的配方编号。
        /// </summary>
        public string? ActiveRecipeId { get; set; }

        /// <summary>
        /// 正在锻造的配方名称。
        /// </summary>
        public string? ActiveRecipeName { get; set; }

        /// <summary>
        /// 当前锻造任务完成时间。
        /// </summary>
        public DateTime? ActiveForgeCompleteAt { get; set; }

        /// <summary>
        /// 当前任务是否已可领取。
        /// </summary>
        public bool CanCollect { get; set; }

        /// <summary>
        /// 累计锻造次数。
        /// </summary>
        public int TotalForgeCount { get; set; }
    }

    /// <summary>
    /// 后台锻造系统详情。
    /// </summary>
    public class AdminForgeSystemDetailDto
    {
        /// <summary>
        /// 玩家编号。
        /// </summary>
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 玩家名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 玩家账号。
        /// </summary>
        public string Account { get; set; } = string.Empty;

        /// <summary>
        /// 锻造职业等级。
        /// </summary>
        public int BlacksmithLevel { get; set; }

        /// <summary>
        /// 当前锻造职业经验。
        /// </summary>
        public int BlacksmithExp { get; set; }

        /// <summary>
        /// 当前五行阵允许达到的职业等级上限。
        /// </summary>
        public int ProfessionLevelCap { get; set; }

        /// <summary>
        /// 今日锻造次数。
        /// </summary>
        public int TodayForgeCount { get; set; }

        /// <summary>
        /// 累计锻造次数。
        /// </summary>
        public int TotalForgeCount { get; set; }

        /// <summary>
        /// 累计成功锻造次数。
        /// </summary>
        public int SuccessForgeCount { get; set; }

        /// <summary>
        /// 当前是否正在锻造。
        /// </summary>
        public bool IsForging { get; set; }

        /// <summary>
        /// 当前任务的配方编号。
        /// </summary>
        public string? ActiveRecipeId { get; set; }

        /// <summary>
        /// 当前任务的配方名称。
        /// </summary>
        public string? ActiveRecipeName { get; set; }

        /// <summary>
        /// 当前锻造任务开始时间。
        /// </summary>
        public DateTime? ActiveForgeStartedAt { get; set; }

        /// <summary>
        /// 当前锻造任务完成时间。
        /// </summary>
        public DateTime? ActiveForgeCompleteAt { get; set; }

        /// <summary>
        /// 当前任务是否已可领取。
        /// </summary>
        public bool CanCollect { get; set; }
    }
}
