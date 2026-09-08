namespace XXX.Application.DTOs.DungeonInstance
{
    /// <summary>
    /// 进入秘境请求。
    /// </summary>
    public class DungeonInstanceEnterRequestDto
    {
        /// <summary>
        /// 秘境模板 ID。
        /// </summary>
        public string DungeonId { get; set; } = string.Empty;
    }

    /// <summary>
    /// 进入秘境响应。
    /// </summary>
    public class DungeonInstanceEnterResponseDto
    {
        /// <summary>
        /// 是否成功。
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 提示消息。
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 秘境实例 ID。
        /// </summary>
        public string? InstanceId { get; set; }

        /// <summary>
        /// 秘境名称。
        /// </summary>
        public string? DungeonName { get; set; }
    }

    /// <summary>
    /// 秘境状态查询响应。
    /// </summary>
    public class DungeonInstanceStatusDto
    {
        /// <summary>
        /// 是否在秘境中。
        /// </summary>
        public bool InDungeon { get; set; }

        /// <summary>
        /// 秘境实例 ID。
        /// </summary>
        public string? InstanceId { get; set; }

        /// <summary>
        /// 秘境模板 ID。
        /// </summary>
        public string? DungeonId { get; set; }

        /// <summary>
        /// 秘境名称。
        /// </summary>
        public string? DungeonName { get; set; }

        /// <summary>
        /// 当前生命值。
        /// </summary>
        public long CurrentHp { get; set; }

        /// <summary>
        /// 最大生命值。
        /// </summary>
        public long MaxHp { get; set; }

        /// <summary>
        /// 当前法力值。
        /// </summary>
        public long CurrentMp { get; set; }

        /// <summary>
        /// 最大法力值。
        /// </summary>
        public long MaxMp { get; set; }

        /// <summary>
        /// 进入时间。
        /// </summary>
        public DateTime EnterTime { get; set; }

        /// <summary>
        /// 上次 tick 时间。
        /// </summary>
        public DateTime? LastTickTime { get; set; }

        /// <summary>
        /// 下次 tick 时间。
        /// </summary>
        public DateTime? NextTickTime { get; set; }

        /// <summary>
        /// 探索日志。
        /// </summary>
        public List<string> ExploreLog { get; set; } = [];

        /// <summary>
        /// 收益池概要。
        /// </summary>
        public List<DungeonRewardSummaryDto> RewardSummary { get; set; } = [];

        /// <summary>
        /// 队伍 ID（null 表示未组队）。
        /// </summary>
        public string? PartyId { get; set; }

        /// <summary>
        /// 是否为队长。
        /// </summary>
        public bool IsPartyLeader { get; set; }
    }

    /// <summary>
    /// 收益池概要条目。
    /// </summary>
    public class DungeonRewardSummaryDto
    {
        /// <summary>
        /// 奖励类型（Gold/Exp/Item/Equipment）。
        /// </summary>
        public string RewardType { get; set; } = string.Empty;

        /// <summary>
        /// 奖励 ID。
        /// </summary>
        public string? RewardId { get; set; }

        /// <summary>
        /// 奖励名称。
        /// </summary>
        public string? RewardName { get; set; }

        /// <summary>
        /// 总数量。
        /// </summary>
        public int Quantity { get; set; }
    }

    /// <summary>
    /// 秘境结算结果。
    /// </summary>
    public class DungeonInstanceSettlementDto
    {
        /// <summary>
        /// 是否成功。
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 结算原因。
        /// </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// 发放的收益明细。
        /// </summary>
        public List<DungeonRewardGrantDto> GrantedRewards { get; set; } = [];

        /// <summary>
        /// 发放总金币。
        /// </summary>
        public long TotalGold { get; set; }

        /// <summary>
        /// 发放总经验。
        /// </summary>
        public long TotalExp { get; set; }

        /// <summary>
        /// 发放总灵石。
        /// </summary>
        public long TotalSpiritStone { get; set; }
    }

    /// <summary>
    /// 可用秘境列表条目。
    /// </summary>
    public class DungeonInstanceAvailableDto
    {
        /// <summary>
        /// 秘境模板 ID。
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 秘境名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 秘境描述。
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 推荐等级。
        /// </summary>
        public int RecommendedLevel { get; set; }

        /// <summary>
        /// 每日进入上限。
        /// </summary>
        public int DailyEnterLimit { get; set; }

        /// <summary>
        /// 今日已进入次数。
        /// </summary>
        public int TodayEnterCount { get; set; }

        /// <summary>
        /// 进入消耗 JSON。
        /// </summary>
        public string? EntryCostsJson { get; set; }
    }

    /// <summary>
    /// 秘境探索历史记录。
    /// </summary>
    public class DungeonInstanceHistoryDto
    {
        /// <summary>
        /// 实例 ID。
        /// </summary>
        public long InstanceId { get; set; }

        /// <summary>
        /// 秘境模板 ID。
        /// </summary>
        public string DungeonId { get; set; } = string.Empty;

        /// <summary>
        /// 秘境名称。
        /// </summary>
        public string DungeonName { get; set; } = string.Empty;

        /// <summary>
        /// 进入时间。
        /// </summary>
        public DateTime EnterTime { get; set; }

        /// <summary>
        /// 结算时间。
        /// </summary>
        public DateTime? SettleTime { get; set; }

        /// <summary>
        /// 结算原因。
        /// </summary>
        public string SettleReason { get; set; } = string.Empty;

        /// <summary>
        /// 探索日志。
        /// </summary>
        public List<string> ExploreLog { get; set; } = [];

        /// <summary>
        /// 收益概要。
        /// </summary>
        public List<DungeonRewardSummaryDto> RewardSummary { get; set; } = [];
    }

    /// <summary>
    /// 单条收益发放记录。
    /// </summary>
    public class DungeonRewardGrantDto
    {
        /// <summary>
        /// 奖励类型。
        /// </summary>
        public string RewardType { get; set; } = string.Empty;

        /// <summary>
        /// 奖励 ID。
        /// </summary>
        public string? RewardId { get; set; }

        /// <summary>
        /// 奖励名称（道具/装备的显示名称）。
        /// </summary>
        public string? RewardName { get; set; }

        /// <summary>
        /// 数量。
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 是否发放成功。
        /// </summary>
        public bool Granted { get; set; }

        /// <summary>
        /// 失败原因。
        /// </summary>
        public string? FailReason { get; set; }
    }
}
