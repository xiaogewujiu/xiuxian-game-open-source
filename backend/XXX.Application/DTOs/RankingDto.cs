namespace XXX.Application.DTOs
{
    /// <summary>
    /// 排行榜条目DTO
    /// </summary>
    public class RankingItemDto
    {
        /// <summary>
        /// 排名
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// 玩家ID
        /// </summary>
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 玩家名称
        /// </summary>
        public string PlayerName { get; set; } = string.Empty;

        /// <summary>
        /// 玩家等级
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 排行值（如战力值、财富值等）
        /// </summary>
        public long Score { get; set; }

        /// <summary>
        /// 排名变化（正数表示上升，负数表示下降）
        /// </summary>
        public int RankChange { get; set; }

        /// <summary>
        /// 是否是自己
        /// </summary>
        public bool IsSelf { get; set; }

        /// <summary>
        /// 当前称号
        /// </summary>
        public string? CurrentTitle { get; set; }
    }

    /// <summary>
    /// 排行榜DTO
    /// </summary>
    public class RankingDto
    {
        /// <summary>
        /// 排行榜类型
        /// </summary>
        public string RankingType { get; set; } = string.Empty;

        /// <summary>
        /// 排行榜名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 排行榜条目列表
        /// </summary>
        public List<RankingItemDto> Items { get; set; } = [];

        /// <summary>
        /// 自己的排名信息
        /// </summary>
        public RankingItemDto? MyRank { get; set; }
    }
}
