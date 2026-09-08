using SqlSugar;

namespace XXX.Entity
{
    [SugarTable("CheckInRewardConfigs")]
    /// <summary>
    /// 连续签到奖励配置实体。
    /// </summary>
    public class CheckInRewardConfigEntity
    {
        [SugarColumn(IsPrimaryKey = true)]
        /// <summary>
        /// 连续签到天数。
        /// </summary>
        public int ContinuousDay { get; set; }

        /// <summary>
        /// 内置种子键。
        /// </summary>
        public string? SeedKey { get; set; }

        /// <summary>
        /// 是否为系统内置配置。
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// 内置配置版本。
        /// </summary>
        public string? BuiltInVersion { get; set; }

        /// <summary>
        /// 当前奖励配置版本号。
        /// </summary>
        public string ConfigVersion { get; set; } = string.Empty;

        /// <summary>
        /// 是否为里程碑奖励。
        /// </summary>
        public bool IsMilestone { get; set; }

        /// <summary>
        /// 奖励内容的 JSON 序列化结果。
        /// </summary>
        public string RewardJson { get; set; } = "[]";

        /// <summary>
        /// 奖励说明。
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 最后更新时间。
        /// </summary>
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    [SugarTable("RedeemCodeConfigs")]
    /// <summary>
    /// 兑换码奖励配置实体。
    /// </summary>
    public class RedeemCodeConfigEntity
    {
        [SugarColumn(IsPrimaryKey = true)]
        /// <summary>
        /// 兑换码文本。
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 内置种子键。
        /// </summary>
        public string? SeedKey { get; set; }

        /// <summary>
        /// 是否为系统内置配置。
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// 内置配置版本。
        /// </summary>
        public string? BuiltInVersion { get; set; }

        /// <summary>
        /// 当前配置版本号。
        /// </summary>
        public string ConfigVersion { get; set; } = string.Empty;

        /// <summary>
        /// 是否启用该兑换码。
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 兑换码说明。
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 奖励内容的 JSON 序列化结果。
        /// </summary>
        public string RewardJson { get; set; } = "[]";

        /// <summary>
        /// 最后更新时间。
        /// </summary>
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }
}
