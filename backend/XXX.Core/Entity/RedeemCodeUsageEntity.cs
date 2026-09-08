using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 中文注释：
    /// 兑换码使用记录表。
    /// 当前项目里的兑换码配置先放在后端代码中维护，但“谁兑换过、什么时候兑换、兑换到了什么”必须真实落库，
    /// 否则页面刷新后无法判断玩家是否已经领过，也无法阻止重复兑换。
    /// </summary>
    [SugarTable("RedeemCodeUsages")]
    public class RedeemCodeUsageEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 50, IsNullable = false)]
        public string UsageId { get; set; } = string.Empty;

        [SugarColumn(Length = 50, IsNullable = false)]
        public string PlayerId { get; set; } = string.Empty;

        [SugarColumn(Length = 50, IsNullable = false)]
        public string Code { get; set; } = string.Empty;

        [SugarColumn(IsNullable = true, ColumnDataType = "nvarchar(max)")]
        public string? RewardSnapshotJson { get; set; }

        [SugarColumn(IsNullable = false)]
        public DateTime RedeemedAt { get; set; } = DateTime.Now;
    }
}
