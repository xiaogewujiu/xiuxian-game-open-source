namespace XXX.Application.DTOs
{
    /// <summary>
    /// 后台炼丹系统列表项。
    /// </summary>
    public class AdminAlchemySystemListItemDto
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
        /// 丹炉等级。
        /// </summary>
        public int FurnaceLevel { get; set; }

        /// <summary>
        /// 炼丹职业等级。
        /// </summary>
        public int AlchemistLevel { get; set; }

        /// <summary>
        /// 当前五行阵允许达到的职业等级上限。
        /// </summary>
        public int ProfessionLevelCap { get; set; }

        /// <summary>
        /// 当前熟练度。
        /// </summary>
        public int Proficiency { get; set; }

        /// <summary>
        /// 当前是否正在炼制。
        /// </summary>
        public bool IsCrafting { get; set; }

        /// <summary>
        /// 正在炼制的配方编号。
        /// </summary>
        public string? ActiveRecipeId { get; set; }

        /// <summary>
        /// 正在炼制的配方名称。
        /// </summary>
        public string? ActiveRecipeName { get; set; }

        /// <summary>
        /// 当前炼制任务完成时间。
        /// </summary>
        public DateTime? ActiveCraftCompleteAt { get; set; }

        /// <summary>
        /// 当前任务是否已可领取。
        /// </summary>
        public bool CanCollect { get; set; }

        /// <summary>
        /// 累计炼制次数。
        /// </summary>
        public int TotalCraftCount { get; set; }
    }

    /// <summary>
    /// 后台炼丹系统详情。
    /// </summary>
    public class AdminAlchemySystemDetailDto
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
        /// 丹炉等级。
        /// </summary>
        public int FurnaceLevel { get; set; }

        /// <summary>
        /// 炼丹职业等级。
        /// </summary>
        public int AlchemistLevel { get; set; }

        /// <summary>
        /// 当前炼丹职业经验。
        /// </summary>
        public int AlchemistExp { get; set; }

        /// <summary>
        /// 当前五行阵允许达到的职业等级上限。
        /// </summary>
        public int ProfessionLevelCap { get; set; }

        /// <summary>
        /// 当前熟练度。
        /// </summary>
        public int Proficiency { get; set; }

        /// <summary>
        /// 当前成功率加成。
        /// </summary>
        public int SuccessRateBonus { get; set; }

        /// <summary>
        /// 当前炼制耗时缩减值。
        /// </summary>
        public int CraftTimeReduction { get; set; }

        /// <summary>
        /// 当前产量加成。
        /// </summary>
        public int YieldBonus { get; set; }

        /// <summary>
        /// 今日炼制次数。
        /// </summary>
        public int TodayCraftCount { get; set; }

        /// <summary>
        /// 累计炼制次数。
        /// </summary>
        public int TotalCraftCount { get; set; }

        /// <summary>
        /// 累计成功炼制次数。
        /// </summary>
        public int SuccessCraftCount { get; set; }

        /// <summary>
        /// 当前是否正在炼制。
        /// </summary>
        public bool IsCrafting { get; set; }

        /// <summary>
        /// 当前任务的配方编号。
        /// </summary>
        public string? ActiveRecipeId { get; set; }

        /// <summary>
        /// 当前任务的配方名称。
        /// </summary>
        public string? ActiveRecipeName { get; set; }

        /// <summary>
        /// 当前炼制任务开始时间。
        /// </summary>
        public DateTime? ActiveCraftStartedAt { get; set; }

        /// <summary>
        /// 当前炼制任务完成时间。
        /// </summary>
        public DateTime? ActiveCraftCompleteAt { get; set; }

        /// <summary>
        /// 当前任务是否已可领取。
        /// </summary>
        public bool CanCollect { get; set; }
    }
}
