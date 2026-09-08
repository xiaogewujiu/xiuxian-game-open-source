using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 抽奖池配置实体
    /// </summary>
    [SugarTable("lottery_pool")]
    public class LotteryPoolEntity
    {
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "抽奖池ID")]
        public string PoolId { get; set; } = string.Empty;

        [SugarColumn(ColumnDescription = "抽奖池名称", Length = 50)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 抽奖类型：0-文字图鉴抽奖，1-图片图鉴抽奖，2-幸运抽奖
        /// </summary>
        [SugarColumn(ColumnDescription = "抽奖类型：0-文字图鉴，1-图片图鉴，2-幸运")]
        public int LotteryType { get; set; } = 0;

        /// <summary>
        /// 消耗类型：0-金币，1-灵石，2-道具
        /// </summary>
        [SugarColumn(ColumnDescription = "消耗类型：0-金币，1-灵石，2-道具")]
        public int CostType { get; set; } = 0;

        [SugarColumn(ColumnDescription = "消耗道具ID", Length = 50, IsNullable = true)]
        public string? CostItemId { get; set; }

        [SugarColumn(ColumnDescription = "消耗数量")]
        public int CostAmount { get; set; } = 0;

        [SugarColumn(ColumnDescription = "是否启用")]
        public bool IsEnabled { get; set; } = true;

        [SugarColumn(ColumnDescription = "开始时间", IsNullable = true)]
        public DateTime? StartTime { get; set; }

        [SugarColumn(ColumnDescription = "结束时间", IsNullable = true)]
        public DateTime? EndTime { get; set; }

        [SugarColumn(ColumnDescription = "是否支持单抽")]
        public bool SupportSingle { get; set; } = true;

        [SugarColumn(ColumnDescription = "是否支持十连抽")]
        public bool SupportTen { get; set; } = false;

        /// <summary>
        /// 每日抽奖次数限制，-1表示无限制
        /// </summary>
        [SugarColumn(ColumnDescription = "每日次数限制，-1为无限制")]
        public int DailyLimit { get; set; } = -1;

        /// <summary>
        /// 总抽奖次数限制，-1表示无限制
        /// </summary>
        [SugarColumn(ColumnDescription = "总次数限制，-1为无限制")]
        public int TotalLimit { get; set; } = -1;

        [SugarColumn(ColumnDescription = "排序顺序")]
        public int SortOrder { get; set; } = 0;

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? SeedKey { get; set; }

        [SugarColumn]
        public bool IsBuiltIn { get; set; } = false;

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? BuiltInVersion { get; set; }

        [SugarColumn]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 抽奖奖项配置实体
    /// </summary>
    [SugarTable("lottery_prize")]
    public class LotteryPrizeEntity
    {
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "奖项ID")]
        public string PrizeId { get; set; } = string.Empty;

        [SugarColumn(ColumnDescription = "所属抽奖池ID", Length = 50)]
        public string PoolId { get; set; } = string.Empty;

        /// <summary>
        /// 奖励类型：0-金币，1-灵石，2-道具，3-文字图鉴，4-图片图鉴，5-谢谢惠顾
        /// </summary>
        [SugarColumn(ColumnDescription = "奖励类型：0-金币，1-灵石，2-道具，3-文字图鉴，4-图片图鉴，5-谢谢惠顾")]
        public int RewardType { get; set; } = 0;

        [SugarColumn(ColumnDescription = "奖励目标ID", Length = 100, IsNullable = true)]
        public string? RewardTargetId { get; set; }

        [SugarColumn(ColumnDescription = "奖励数量")]
        public int RewardAmount { get; set; } = 0;

        /// <summary>
        /// 概率，万分比，10000=100%
        /// </summary>
        [SugarColumn(ColumnDescription = "概率(万分比，10000=100%)")]
        public int Probability { get; set; } = 0;

        [SugarColumn(ColumnDescription = "排序顺序")]
        public int SortOrder { get; set; } = 0;

        [SugarColumn(ColumnDescription = "是否启用")]
        public bool IsEnabled { get; set; } = true;

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? SeedKey { get; set; }

        [SugarColumn]
        public bool IsBuiltIn { get; set; } = false;

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? BuiltInVersion { get; set; }

        [SugarColumn]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 玩家图鉴拥有记录实体
    /// </summary>
    [SugarTable("player_collection")]
    public class PlayerCollectionEntity : BaseEntity
    {
        [SugarColumn(ColumnDescription = "玩家ID")]
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 图鉴类型：0-文字图鉴，1-图片图鉴
        /// </summary>
        [SugarColumn(ColumnDescription = "图鉴类型：0-文字，1-图片")]
        public int CollectionType { get; set; } = 0;

        [SugarColumn(ColumnDescription = "系列ID", Length = 50)]
        public string SeriesId { get; set; } = string.Empty;

        [SugarColumn(ColumnDescription = "图鉴项ID", Length = 50)]
        public string ItemId { get; set; } = string.Empty;

        [SugarColumn(ColumnDescription = "拥有数量")]
        public int OwnedCount { get; set; } = 0;

        [SugarColumn(ColumnDescription = "首次获得时间")]
        public DateTime FirstGetTime { get; set; } = DateTime.Now;

        [SugarColumn(ColumnDescription = "最后获得时间")]
        public DateTime LastGetTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 抽奖日志实体
    /// </summary>
    [SugarTable("lottery_log")]
    public class LotteryLogEntity
    {
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "日志ID")]
        public string LogId { get; set; } = string.Empty;

        [SugarColumn(ColumnDescription = "玩家ID")]
        public string PlayerId { get; set; } = string.Empty;

        [SugarColumn(ColumnDescription = "抽奖池ID", Length = 50)]
        public string PoolId { get; set; } = string.Empty;

        [SugarColumn(ColumnDescription = "抽奖类型")]
        public int LotteryType { get; set; } = 0;

        [SugarColumn(ColumnDescription = "消耗类型")]
        public int CostType { get; set; } = 0;

        [SugarColumn(ColumnDescription = "消耗道具ID", Length = 50, IsNullable = true)]
        public string? CostItemId { get; set; }

        [SugarColumn(ColumnDescription = "消耗数量")]
        public int CostAmount { get; set; } = 0;

        [SugarColumn(ColumnDescription = "中奖奖项ID", Length = 50)]
        public string PrizeId { get; set; } = string.Empty;

        [SugarColumn(ColumnDescription = "奖励类型")]
        public int RewardType { get; set; } = 0;

        [SugarColumn(ColumnDescription = "奖励目标ID", Length = 100, IsNullable = true)]
        public string? RewardTargetId { get; set; }

        [SugarColumn(ColumnDescription = "奖励名称", Length = 100)]
        public string RewardName { get; set; } = string.Empty;

        [SugarColumn(ColumnDescription = "奖励数量")]
        public int RewardAmount { get; set; } = 0;

        [SugarColumn(ColumnDescription = "是否谢谢惠顾")]
        public bool IsThanks { get; set; } = false;

        [SugarColumn(ColumnDescription = "随机数值")]
        public int RandomValue { get; set; } = 0;

        [SugarColumn(ColumnDescription = "抽奖时间")]
        public DateTime LotteryTime { get; set; } = DateTime.Now;

        [SugarColumn(ColumnDescription = "玩家IP", Length = 50, IsNullable = true)]
        public string? PlayerIp { get; set; }
    }
}
