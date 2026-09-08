namespace XXX.Application.DTOs
{
    /// <summary>
    /// 后台签到奖励配置。
    /// </summary>
    public class AdminCheckInRewardConfigDto
    {
        /// <summary>
        /// 连续签到天数。
        /// </summary>
        public int ContinuousDay { get; set; }

        /// <summary>
        /// 是否为系统内置配置。
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
        /// 配置版本号。
        /// </summary>
        public string ConfigVersion { get; set; } = string.Empty;

        /// <summary>
        /// 是否为里程碑奖励。
        /// </summary>
        public bool IsMilestone { get; set; }

        /// <summary>
        /// 奖励 JSON。
        /// </summary>
        public string RewardJson { get; set; } = "[]";

        /// <summary>
        /// 描述。
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 最后更新时间。
        /// </summary>
        public DateTime? LastUpdateTime { get; set; }
    }

    /// <summary>
    /// 后台兑换码配置。
    /// </summary>
    public class AdminRedeemCodeConfigDto
    {
        /// <summary>
        /// 兑换码。
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 是否为系统内置配置。
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
        /// 配置版本号。
        /// </summary>
        public string ConfigVersion { get; set; } = string.Empty;

        /// <summary>
        /// 是否启用。
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 描述。
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 奖励 JSON。
        /// </summary>
        public string RewardJson { get; set; } = "[]";

        /// <summary>
        /// 最后更新时间。
        /// </summary>
        public DateTime? LastUpdateTime { get; set; }
    }
}
