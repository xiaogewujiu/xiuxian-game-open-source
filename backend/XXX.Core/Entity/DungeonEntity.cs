using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 玩家副本每日挑战记录
    /// 用于统计某玩家在某副本当天已挑战次数
    /// </summary>
    [SugarTable("dungeon_daily_record")]
    public class DungeonDailyRecordEntity
    {
        /// <summary>
        /// 记录唯一标识
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, Length = 50)]
        public string GID { get; set; } = string.Empty;

        /// <summary>
        /// 玩家ID
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 副本ID
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        public string DungeonId { get; set; } = string.Empty;

        /// <summary>
        /// 当天已挑战次数
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int ChallengeCount { get; set; }

        /// <summary>
        /// 记录日期（按天）
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime RecordDate { get; set; } = DateTime.Today;

        /// <summary>
        /// 最后更新时间
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }
}
