using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 中文注释：
    /// 玩家签到总状态表。
    /// 这张表负责保存“连续签到天数、总签到天数、上次签到时间”这类聚合数据，
    /// 目的是避免前端每次打开签到弹窗时都临时扫全量签到记录再自己计算连续天数。
    /// </summary>
    [SugarTable("PlayerCheckInStates")]
    public class CheckInStateEntity
    {
        /// <summary>
        /// 玩家 ID。
        /// 一名玩家只保留一条签到总状态，因此直接用 PlayerId 作为主键。
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, Length = 50, IsNullable = false)]
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 当前连续签到天数。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int ContinuousDays { get; set; }

        /// <summary>
        /// 历史总签到天数。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int TotalDays { get; set; }

        /// <summary>
        /// 上次签到日期。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? LastCheckInDate { get; set; }

        /// <summary>
        /// 最近一次状态更新时间。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }
}
