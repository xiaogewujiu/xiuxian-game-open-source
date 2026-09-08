namespace XXX.Application.DTOs
{
    // ===== 抽奖池 =====
    public class AdminLotteryPoolListItemDto
    {
        public string PoolId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int LotteryType { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsBuiltIn { get; set; }
        public string? BuiltInVersion { get; set; }
    }

    public class AdminLotteryPoolDetailDto
    {
        public string PoolId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int LotteryType { get; set; }
        public int CostType { get; set; }
        public string? CostItemId { get; set; }
        public int CostAmount { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool SupportSingle { get; set; }
        public bool SupportTen { get; set; }
        public int DailyLimit { get; set; }
        public int TotalLimit { get; set; }
        public int SortOrder { get; set; }
        public bool IsBuiltIn { get; set; }
        public string? SeedKey { get; set; }
        public string? BuiltInVersion { get; set; }
        public DateTime? LastUpdateTime { get; set; }
    }

    // ===== 抽奖奖项 =====
    public class AdminLotteryPrizeListItemDto
    {
        public string PrizeId { get; set; } = string.Empty;
        public string PoolId { get; set; } = string.Empty;
        public int RewardType { get; set; }
        public string? RewardTargetId { get; set; }
        public int RewardAmount { get; set; }
        public int Probability { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsBuiltIn { get; set; }
        public string? BuiltInVersion { get; set; }
    }

    public class AdminLotteryPrizeDetailDto
    {
        public string PrizeId { get; set; } = string.Empty;
        public string PoolId { get; set; } = string.Empty;
        public int RewardType { get; set; }
        public string? RewardTargetId { get; set; }
        public int RewardAmount { get; set; }
        public int Probability { get; set; }
        public int SortOrder { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsBuiltIn { get; set; }
        public string? SeedKey { get; set; }
        public string? BuiltInVersion { get; set; }
        public DateTime? LastUpdateTime { get; set; }
    }

    // ===== 抽奖日志 =====
    public class AdminLotteryLogListItemDto
    {
        public string LogId { get; set; } = string.Empty;
        public string PlayerId { get; set; } = string.Empty;
        public string PoolId { get; set; } = string.Empty;
        public int LotteryType { get; set; }
        public int CostType { get; set; }
        public int CostAmount { get; set; }
        public int RewardType { get; set; }
        public string RewardName { get; set; } = string.Empty;
        public int RewardAmount { get; set; }
        public bool IsThanks { get; set; }
        public DateTime LotteryTime { get; set; }
    }
}
