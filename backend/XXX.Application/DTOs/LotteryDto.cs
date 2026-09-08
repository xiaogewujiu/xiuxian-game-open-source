namespace XXX.Application.DTOs
{
    /// <summary>
    /// 抽奖池展示 DTO（玩家端）。
    /// </summary>
    public class LotteryPoolViewDto
    {
        public string PoolId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int LotteryType { get; set; }
        public int CostType { get; set; }
        public string? CostItemId { get; set; }
        public int CostAmount { get; set; }
        public bool SupportSingle { get; set; }
        public bool SupportTen { get; set; }
        public int DailyRemaining { get; set; } = -1;
        public int TotalRemaining { get; set; } = -1;
        public long PlayerCurrency { get; set; }
        public int PlayerItemCount { get; set; }
        public bool CanDraw { get; set; }
    }

    /// <summary>
    /// 抽奖请求 DTO。
    /// </summary>
    public class LotteryDrawRequestDto
    {
        public string PoolId { get; set; } = string.Empty;
        public int Count { get; set; } = 1;
    }

    /// <summary>
    /// 抽奖结果 DTO。
    /// </summary>
    public class LotteryDrawResultDto
    {
        public bool Success { get; set; }
        public string PoolId { get; set; } = string.Empty;
        public int DrawCount { get; set; }
        public LotteryDrawCostDto Cost { get; set; } = new();
        public List<LotteryDrawItemDto> Rewards { get; set; } = [];
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// 抽奖消耗信息。
    /// </summary>
    public class LotteryDrawCostDto
    {
        public string Type { get; set; } = string.Empty;
        public string? ItemId { get; set; }
        public int Amount { get; set; }
    }

    /// <summary>
    /// 单次抽奖获得项。
    /// </summary>
    public class LotteryDrawItemDto
    {
        public string RewardType { get; set; } = string.Empty;
        public string? SeriesId { get; set; }
        public string? SeriesName { get; set; }
        public string? ItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
        public bool IsNew { get; set; }
        public int OwnedCount { get; set; }
    }
}
