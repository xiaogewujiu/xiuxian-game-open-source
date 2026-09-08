namespace XXX.Application.DTOs
{
    /// <summary>
    /// 后台排行配置列表项。
    /// </summary>
    public class AdminRankingConfigListItemDto
    {
        /// <summary>
        /// 排行编号。
        /// </summary>
        public string RankingId { get; set; } = string.Empty;

        /// <summary>
        /// 排行名称。
        /// </summary>
        public string RankingName { get; set; } = string.Empty;

        /// <summary>
        /// 排行类型。
        /// </summary>
        public int RankingType { get; set; }

        /// <summary>
        /// 是否启用。
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 是否为内置种子配置。
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// 内置版本号。
        /// </summary>
        public string? BuiltInVersion { get; set; }
    }

    /// <summary>
    /// 后台排行配置详情。
    /// </summary>
    public class AdminRankingConfigDetailDto
    {
        /// <summary>
        /// 排行编号。
        /// </summary>
        public string RankingId { get; set; } = string.Empty;

        /// <summary>
        /// 排行名称。
        /// </summary>
        public string RankingName { get; set; } = string.Empty;

        /// <summary>
        /// 排行类型。
        /// </summary>
        public int RankingType { get; set; }

        /// <summary>
        /// 描述。
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 最大容量。
        /// </summary>
        public int MaxSize { get; set; }

        /// <summary>
        /// 更新间隔。
        /// </summary>
        public int UpdateInterval { get; set; }

        /// <summary>
        /// 是否启用赛季。
        /// </summary>
        public bool SeasonEnabled { get; set; }

        /// <summary>
        /// 赛季时长。
        /// </summary>
        public int SeasonDuration { get; set; }

        /// <summary>
        /// 当前赛季。
        /// </summary>
        public int CurrentSeason { get; set; }

        /// <summary>
        /// 赛季开始时间。
        /// </summary>
        public DateTime? SeasonStartTime { get; set; }

        /// <summary>
        /// 排序值。
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 是否启用。
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 是否为内置种子配置。
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
        /// 最近一次编辑时间。
        /// </summary>
        public DateTime? LastUpdateTime { get; set; }
    }

    /// <summary>
    /// 后台排行奖励列表项。
    /// </summary>
    public class AdminRankingRewardListItemDto
    {
        /// <summary>
        /// 奖励编号。
        /// </summary>
        public string GID { get; set; } = string.Empty;

        /// <summary>
        /// 排行编号。
        /// </summary>
        public string RankingId { get; set; } = string.Empty;

        /// <summary>
        /// 奖励标题。
        /// </summary>
        public string RewardTitle { get; set; } = string.Empty;

        /// <summary>
        /// 最小名次。
        /// </summary>
        public int MinRank { get; set; }

        /// <summary>
        /// 最大名次。
        /// </summary>
        public int MaxRank { get; set; }

        /// <summary>
        /// 是否为内置种子奖励。
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// 内置版本号。
        /// </summary>
        public string? BuiltInVersion { get; set; }
    }

    /// <summary>
    /// 后台排行奖励详情。
    /// </summary>
    public class AdminRankingRewardDetailDto
    {
        /// <summary>
        /// 奖励编号。
        /// </summary>
        public string GID { get; set; } = string.Empty;

        /// <summary>
        /// 排行编号。
        /// </summary>
        public string RankingId { get; set; } = string.Empty;

        /// <summary>
        /// 最小名次。
        /// </summary>
        public int MinRank { get; set; }

        /// <summary>
        /// 最大名次。
        /// </summary>
        public int MaxRank { get; set; }

        /// <summary>
        /// 奖励标题。
        /// </summary>
        public string RewardTitle { get; set; } = string.Empty;

        /// <summary>
        /// 金币。
        /// </summary>
        public long Gold { get; set; }

        /// <summary>
        /// 灵石。
        /// </summary>
        public long SpiritStone { get; set; }

        /// <summary>
        /// 称号。
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// 是否为内置种子奖励。
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
        /// 最近一次编辑时间。
        /// </summary>
        public DateTime? LastUpdateTime { get; set; }
    }
}
