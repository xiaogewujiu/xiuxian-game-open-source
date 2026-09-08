using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 组队副本主战斗摘要记录。
    /// 用于持久化一次队伍挑战的幂等状态、成员资格和结算摘要，防止 HTTP 重试重复扣次或发奖。
    /// </summary>
    [SugarTable("PartyBattleRecords")]
    public class PartyBattleRecordEntity
    {
        /// <summary>战斗唯一编号。</summary>
        [SugarColumn(IsPrimaryKey = true, Length = 50)]
        public string BattleId { get; set; } = string.Empty;

        /// <summary>临时队伍编号。</summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        public string PartyId { get; set; } = string.Empty;

        /// <summary>副本编号。</summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        public string DungeonId { get; set; } = string.Empty;

        /// <summary>发起挑战的玩家编号。</summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        public string InitiatorPlayerId { get; set; } = string.Empty;

        /// <summary>本场成员编号 JSON。</summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = false)]
        public string MemberIdsJson { get; set; } = "[]";

        /// <summary>请求幂等编号。</summary>
        [SugarColumn(Length = 100, IsNullable = false)]
        public string RequestId { get; set; } = string.Empty;

        /// <summary>开始时间（UTC）。</summary>
        [SugarColumn(IsNullable = false)]
        public DateTime StartedAtUtc { get; set; }

        /// <summary>完成时间（UTC）。</summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? CompletedAtUtc { get; set; }

        /// <summary>是否胜利。</summary>
        [SugarColumn(IsNullable = false)]
        public bool IsVictory { get; set; }

        /// <summary>完成的副本层数。</summary>
        [SugarColumn(IsNullable = false)]
        public int StageCount { get; set; }

        /// <summary>总回合数。</summary>
        [SugarColumn(IsNullable = false)]
        public int TotalRounds { get; set; }

        /// <summary>成员资格、次数和奖励摘要 JSON。</summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = false)]
        public string RewardSummaryJson { get; set; } = "{}";

        /// <summary>结算状态：Started、Completed、Failed、Settling。</summary>
        [SugarColumn(Length = 30, IsNullable = false)]
        public string SettlementStatus { get; set; } = "Started";

        /// <summary>完整回放 JSON；当前默认保留，后续清理任务可按完成时间删除。</summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? FullReplayJson { get; set; }
    }
}
