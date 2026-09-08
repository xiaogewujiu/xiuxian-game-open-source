namespace XXX.Achievement
{
    /// <summary>
    /// 成就奖励领取结果。
    /// 供应用层成就服务和 WebApi 返回统一结果模型。
    /// </summary>
    public class AchievementClaimResult
    {
        /// <summary>
        /// 是否成功。
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 结果消息。
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 成就 ID。
        /// </summary>
        public string AchievementId { get; set; } = string.Empty;

        /// <summary>
        /// 获得的成就点数。
        /// </summary>
        public int PointsEarned { get; set; }

        /// <summary>
        /// 奖励结果。
        /// </summary>
        public AchievementRewardResult RewardResult { get; set; } = new();
    }

    /// <summary>
    /// 成就统计信息。
    /// </summary>
    public class AchievementStats
    {
        /// <summary>
        /// 总成就数。
        /// </summary>
        public int TotalAchievements { get; set; }

        /// <summary>
        /// 已完成数量。
        /// </summary>
        public int CompletedCount { get; set; }

        /// <summary>
        /// 进行中数量。
        /// </summary>
        public int InProgressCount { get; set; }

        /// <summary>
        /// 完成百分比。
        /// </summary>
        public float CompletionPercent { get; set; }

        /// <summary>
        /// 总成就点数。
        /// </summary>
        public int TotalPoints { get; set; }

        /// <summary>
        /// 获取统计摘要文本。
        /// </summary>
        public string GetSummaryText()
        {
            return $"成就进度：{CompletedCount}/{TotalAchievements} ({CompletionPercent:F1}%)\n"
                + $"成就点数：{TotalPoints}\n"
                + $"进行中：{InProgressCount}";
        }
    }
}
