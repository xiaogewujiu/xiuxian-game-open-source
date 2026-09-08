namespace XXX.Balance
{
    /// <summary>
    /// 活动奖励中的单条资源条目。
    /// </summary>
    public sealed record ActivityRewardSeed(
        string Type,
        long Count,
        string? ItemId = null,
        bool IsBound = false,
        string? Description = null);

    /// <summary>
    /// 连续签到奖励配置种子。
    /// </summary>
    public sealed record CheckInRewardSeed(
        int ContinuousDay,
        bool IsMilestone,
        IReadOnlyList<ActivityRewardSeed> Rewards);

    /// <summary>
    /// 兑换码配置种子。
    /// </summary>
    public sealed record RedeemCodeSeed(
        string Code,
        string Description,
        IReadOnlyList<ActivityRewardSeed> Rewards);

    /// <summary>
    /// 活动奖励目录。
    /// </summary>
    public static class ActivityRewardCatalog
    {
        public const string CheckInConfigVersion = "checkin-balance-v2-20260331";
        public const string RedeemCodeConfigVersion = "redeem-balance-v1-20260323";

        /// <summary>
        /// 生成 30 天连续签到奖励。
        /// </summary>
        public static IReadOnlyList<CheckInRewardSeed> BuildCheckInRewards()
        {
            var rewards = new List<CheckInRewardSeed>();
            for (var day = 1; day <= 30; day++)
            {
                var dailyRewards = new List<ActivityRewardSeed>
                {
                    new(
                        "gold",
                        130 + day * 28L + day / 5 * 24L,
                        Description: $"第 {day} 天基础金币奖励"),
                    new(
                        "exp",
                        40 + day * 12L + day / 6 * 10L,
                        Description: $"第 {day} 天基础修为奖励")
                };

                if (day % 3 == 0)
                {
                    dailyRewards.Add(new ActivityRewardSeed(
                        "spiritStone",
                        12 + day * 3L,
                        Description: "连续签到额外灵石奖励"));
                }

                if (day % 5 == 0)
                {
                    if (day < 15)
                    {
                        dailyRewards.Add(new ActivityRewardSeed(
                            "item",
                            2,
                            "item_001",
                            true,
                            "连续签到阶段道具奖励"));
                    }
                    else if (day < 25)
                    {
                        dailyRewards.Add(new ActivityRewardSeed(
                            "item",
                            1,
                            "item_003",
                            true,
                            "连续签到阶段材料奖励"));
                    }
                    else
                    {
                        dailyRewards.Add(new ActivityRewardSeed(
                            "item",
                            1,
                            "item_002",
                            true,
                            "连续签到阶段道具奖励"));
                    }
                }

                switch (day)
                {
                    case 7:
                        dailyRewards.Add(new ActivityRewardSeed(
                            "spiritStone",
                            18,
                            Description: "7 天里程碑奖励"));
                        break;
                    case 15:
                        dailyRewards.Add(new ActivityRewardSeed(
                            "spiritStone",
                            45,
                            Description: "15 天里程碑奖励"));
                        dailyRewards.Add(new ActivityRewardSeed(
                            "item",
                            1,
                            "item_002",
                            true,
                            "15 天里程碑道具奖励"));
                        break;
                    case 30:
                        dailyRewards.Add(new ActivityRewardSeed(
                            "spiritStone",
                            30,
                            Description: "30 天里程碑奖励"));
                        dailyRewards.Add(new ActivityRewardSeed(
                            "item",
                            1,
                            "pet_egg",
                            true,
                            "30 天里程碑灵宠奖励"));
                        dailyRewards.Add(new ActivityRewardSeed(
                            "item",
                            2,
                            "item_003",
                            true,
                            "30 天里程碑材料奖励"));
                        break;
                }

                rewards.Add(new CheckInRewardSeed(day, day is 7 or 15 or 30, dailyRewards));
            }

            return rewards;
        }

        /// <summary>
        /// 生成默认兑换码配置。
        /// </summary>
        public static IReadOnlyList<RedeemCodeSeed> BuildRedeemCodes()
        {
            return
            [
                new(
                    "WELCOME",
                    "新手欢迎奖励",
                    [
                        new("gold", 800, Description: "新手欢迎奖励"),
                        new("exp", 120, Description: "新手欢迎奖励"),
                        new("item", 2, "item_001", true, "新手欢迎补给")
                    ]),
                new(
                    "VIP666",
                    "活动兑换码奖励",
                    [
                        new("spiritStone", 88, Description: "VIP 活动奖励"),
                        new("spiritStone", 18, Description: "VIP 活动奖励"),
                        new("item", 1, "item_003", true, "VIP 活动材料奖励")
                    ]),
                new(
                    "XIUXIAN2024",
                    "纪念兑换码奖励",
                    [
                        new("gold", 2200, Description: "纪念兑换码奖励"),
                        new("spiritStone", 180, Description: "纪念兑换码奖励"),
                        new("item", 2, "item_002", true, "纪念兑换码补给")
                    ])
            ];
        }
    }
}
