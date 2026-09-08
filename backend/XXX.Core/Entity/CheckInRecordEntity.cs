using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 中文注释：
    /// 玩家每日签到明细表。
    /// 这张表按“玩家 + 日期”记录每一次实际签到结果，
    /// 主要用于两类场景：
    /// 1. 给前端月历面板提供真实勾选数据；
    /// 2. 用唯一索引硬性防止同一天重复签到。
    /// </summary>
    [SugarTable("PlayerCheckInRecords")]
    public class CheckInRecordEntity
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        [SugarColumn(Length = 50, IsNullable = false)]
        public string PlayerId { get; set; } = string.Empty;

        [SugarColumn(IsNullable = false)]
        public DateTime CheckInDate { get; set; }

        /// <summary>
        /// 本次签到奖励快照。
        /// 使用 JSON 保存，是为了让后续排查“玩家那天到底领了什么”时有据可查。
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = "nvarchar(max)")]
        public string? RewardSnapshotJson { get; set; }

        [SugarColumn(IsNullable = false)]
        public DateTime CreateTime { get; set; } = DateTime.Now;
    }
}
