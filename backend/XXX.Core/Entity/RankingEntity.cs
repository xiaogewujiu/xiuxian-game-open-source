using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 排行榜条目实体
    /// 记录排行榜中的玩家排名数据
    /// </summary>
    [SugarTable("ranking_entry")]
    public class RankingEntryEntity
    {
        /// <summary>
        /// 记录唯一标识符
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "记录ID")]
        public string GID { get; set; } = string.Empty;

        /// <summary>
        /// 所属排行榜ID
        /// </summary>
        [SugarColumn(ColumnDescription = "排行榜ID", Length = 50)]
        public string RankingId { get; set; } = string.Empty;

        /// <summary>
        /// 玩家ID
        /// </summary>
        [SugarColumn(ColumnDescription = "玩家ID")]
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 玩家名称
        /// </summary>
        [SugarColumn(ColumnDescription = "玩家名称", Length = 50)]
        public string PlayerName { get; set; } = string.Empty;

        /// <summary>
        /// 玩家等级
        /// </summary>
        [SugarColumn(ColumnDescription = "玩家等级")]
        public int PlayerLevel { get; set; } = 1;

        /// <summary>
        /// 排行分数
        /// </summary>
        [SugarColumn(ColumnDescription = "分数")]
        public long Score { get; set; } = 0;

        /// <summary>
        /// 当前排名
        /// </summary>
        [SugarColumn(ColumnDescription = "当前排名")]
        public int Rank { get; set; } = 0;

        /// <summary>
        /// 上次排名，用于显示排名变化
        /// </summary>
        [SugarColumn(ColumnDescription = "上次排名")]
        public int LastRank { get; set; } = 0;

        /// <summary>
        /// 当前赛季编号
        /// </summary>
        [SugarColumn(ColumnDescription = "赛季")]
        public int Season { get; set; } = 1;

        /// <summary>
        /// 最后更新时间
        /// </summary>
        [SugarColumn(ColumnDescription = "最后更新时间")]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 排行榜配置实体
    /// 存储排行榜的基本配置信息
    /// </summary>
    [SugarTable("ranking_config")]
    public class RankingConfigEntity
    {
        /// <summary>
        /// 排行榜唯一标识符
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "排行榜ID")]
        public string RankingId { get; set; } = string.Empty;

        /// <summary>
        /// 排行榜名称
        /// </summary>
        [SugarColumn(ColumnDescription = "排行榜名称", Length = 50)]
        public string RankingName { get; set; } = string.Empty;

        /// <summary>
        /// 排行榜类型：0-等级，1-战力，2-竞技场，3-成就，4-财富
        /// </summary>
        [SugarColumn(ColumnDescription = "排行榜类型")]
        public int RankingType { get; set; } = 0;

        /// <summary>
        /// 排行榜描述
        /// </summary>
        [SugarColumn(ColumnDescription = "描述", Length = 200, IsNullable = true)]
        public string? Description { get; set; }

        /// <summary>
        /// 排行榜最大容量
        /// </summary>
        [SugarColumn(ColumnDescription = "最大容量")]
        public int MaxSize { get; set; } = 100;

        /// <summary>
        /// 自动更新间隔（分钟）
        /// </summary>
        [SugarColumn(ColumnDescription = "更新间隔(分钟)")]
        public int UpdateInterval { get; set; } = 60;

        /// <summary>
        /// 是否启用赛季机制
        /// </summary>
        [SugarColumn(ColumnDescription = "是否启用赛季")]
        public bool SeasonEnabled { get; set; } = false;

        /// <summary>
        /// 赛季时长（天）
        /// </summary>
        [SugarColumn(ColumnDescription = "赛季时长(天)")]
        public int SeasonDuration { get; set; } = 30;

        /// <summary>
        /// 当前赛季编号
        /// </summary>
        [SugarColumn(ColumnDescription = "当前赛季")]
        public int CurrentSeason { get; set; } = 1;

        /// <summary>
        /// 当前赛季开始时间
        /// </summary>
        [SugarColumn(ColumnDescription = "赛季开始时间", IsNullable = true)]
        public DateTime? SeasonStartTime { get; set; }

        /// <summary>
        /// 排序顺序，数值越小越靠前
        /// </summary>
        [SugarColumn(ColumnDescription = "排序顺序")]
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// 是否启用该排行榜
        /// </summary>
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
    /// 排行榜奖励实体
    /// 存储排行榜的排名奖励配置
    /// </summary>
    [SugarTable("ranking_reward")]
    public class RankingRewardEntity
    {
        /// <summary>
        /// 奖励记录唯一标识符
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "奖励ID")]
        public string GID { get; set; } = string.Empty;

        /// <summary>
        /// 所属排行榜ID
        /// </summary>
        [SugarColumn(ColumnDescription = "排行榜ID", Length = 50)]
        public string RankingId { get; set; } = string.Empty;

        /// <summary>
        /// 奖励适用的最小排名
        /// </summary>
        [SugarColumn(ColumnDescription = "最小排名")]
        public int MinRank { get; set; } = 1;

        /// <summary>
        /// 奖励适用的最大排名
        /// </summary>
        [SugarColumn(ColumnDescription = "最大排名")]
        public int MaxRank { get; set; } = 1;

        /// <summary>
        /// 奖励标题/名称
        /// </summary>
        [SugarColumn(ColumnDescription = "奖励标题", Length = 50)]
        public string RewardTitle { get; set; } = string.Empty;

        /// <summary>
        /// 奖励金币数量
        /// </summary>
        [SugarColumn(ColumnDescription = "奖励金币")]
        public long Gold { get; set; } = 0;

        /// <summary>
        /// 奖励灵石数量
        /// </summary>
        [SugarColumn(ColumnDescription = "奖励灵石")]
        public long SpiritStone { get; set; } = 0;

        /// <summary>
        /// 奖励的称号
        /// </summary>
        [SugarColumn(ColumnDescription = "奖励称号", Length = 50, IsNullable = true)]
        public string? Title { get; set; }

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
    /// 排行榜历史实体
    /// 存储排行榜的历史快照数据
    /// </summary>
    [SugarTable("ranking_history")]
    public class RankingHistoryEntity
    {
        /// <summary>
        /// 历史记录唯一标识符
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "历史ID")]
        public string GID { get; set; } = string.Empty;

        /// <summary>
        /// 所属排行榜ID
        /// </summary>
        [SugarColumn(ColumnDescription = "排行榜ID", Length = 50)]
        public string RankingId { get; set; } = string.Empty;

        /// <summary>
        /// 快照类型：0-手动，1-每日，2-每周，3-赛季结束
        /// </summary>
        [SugarColumn(ColumnDescription = "快照类型")]
        public int SnapshotType { get; set; } = 0;

        /// <summary>
        /// 快照所属赛季
        /// </summary>
        [SugarColumn(ColumnDescription = "赛季")]
        public int Season { get; set; } = 1;

        /// <summary>
        /// 快照描述
        /// </summary>
        [SugarColumn(ColumnDescription = "描述", Length = 100, IsNullable = true)]
        public string? Description { get; set; }

        /// <summary>
        /// 快照数据的JSON序列化
        /// </summary>
        [SugarColumn(ColumnDescription = "快照数据JSON", ColumnDataType = "nvarchar(max)")]
        public string? SnapshotData { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [SugarColumn(ColumnDescription = "创建时间")]
        public DateTime CreateTime { get; set; } = DateTime.Now;
    }
}
