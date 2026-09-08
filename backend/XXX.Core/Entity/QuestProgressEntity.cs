using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 任务进度实体
    /// 记录玩家的任务完成进度
    /// </summary>
    [SugarTable("quest_progress")]
    public class QuestProgressEntity
    {
        /// <summary>
        /// 记录唯一标识符
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "记录ID")]
        public string GID { get; set; } = string.Empty;

        /// <summary>
        /// 玩家ID
        /// </summary>
        [SugarColumn(ColumnDescription = "玩家ID")]
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 任务ID
        /// </summary>
        [SugarColumn(ColumnDescription = "任务ID", Length = 50)]
        public string QuestId { get; set; } = string.Empty;

        /// <summary>
        /// 当前任务阶段
        /// </summary>
        [SugarColumn(ColumnDescription = "当前阶段")]
        public int CurrentStage { get; set; } = 0;

        /// <summary>
        /// 各目标进度的JSON序列化数据
        /// </summary>
        [SugarColumn(ColumnDescription = "目标进度JSON", Length = 500, IsNullable = true)]
        public string? ObjectiveProgressJson { get; set; }

        /// <summary>
        /// 任务状态：0-未接取，1-进行中，2-已完成，3-已提交
        /// </summary>
        [SugarColumn(ColumnDescription = "状态")]
        public int Status { get; set; } = 0;

        /// <summary>
        /// 接取任务的时间
        /// </summary>
        [SugarColumn(ColumnDescription = "接取时间", IsNullable = true)]
        public DateTime? AcceptTime { get; set; }

        /// <summary>
        /// 完成任务的时间
        /// </summary>
        [SugarColumn(ColumnDescription = "完成时间", IsNullable = true)]
        public DateTime? CompleteTime { get; set; }

        /// <summary>
        /// 提交任务的时间
        /// </summary>
        [SugarColumn(ColumnDescription = "提交时间", IsNullable = true)]
        public DateTime? SubmitTime { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        [SugarColumn(ColumnDescription = "最后更新时间")]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 任务配置实体
    /// 存储任务的配置信息，支持数据库动态配置
    /// </summary>
    [SugarTable("quest_config")]
    public class QuestConfigEntity
    {
        /// <summary>
        /// 任务唯一标识符
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "任务ID")]
        public string QuestId { get; set; } = string.Empty;

        /// <summary>
        /// 内置种子键。
        /// </summary>
        [SugarColumn(ColumnDescription = "内置种子键", Length = 100, IsNullable = true)]
        public string? SeedKey { get; set; }

        /// <summary>
        /// 是否为系统内置任务。
        /// </summary>
        [SugarColumn(ColumnDescription = "是否内置")]
        public bool IsBuiltIn { get; set; } = false;

        /// <summary>
        /// 内置版本号。
        /// </summary>
        [SugarColumn(ColumnDescription = "内置版本", Length = 100, IsNullable = true)]
        public string? BuiltInVersion { get; set; }

        /// <summary>
        /// 任务名称
        /// </summary>
        [SugarColumn(ColumnDescription = "任务名称", Length = 50)]
        public string QuestName { get; set; } = string.Empty;

        /// <summary>
        /// 任务类型：0-主线，1-支线，2-日常，3-周常，4-活动
        /// </summary>
        [SugarColumn(ColumnDescription = "任务类型")]
        public int QuestType { get; set; } = 0;

        /// <summary>
        /// 任务重置周期：0-一次性，1-每日，2-每周
        /// </summary>
        [SugarColumn(ColumnDescription = "任务重置周期")]
        public int ResetCycle { get; set; } = 0;

        /// <summary>
        /// 任务描述
        /// </summary>
        [SugarColumn(ColumnDescription = "描述", Length = 500)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 接取任务所需的最低等级
        /// </summary>
        [SugarColumn(ColumnDescription = "所需等级")]
        public int RequiredLevel { get; set; } = 1;

        /// <summary>
        /// 前置任务ID列表，逗号分隔
        /// </summary>
        [SugarColumn(ColumnDescription = "前置任务ID", Length = 200, IsNullable = true)]
        public string? PreQuestIds { get; set; }

        /// <summary>
        /// 是否自动接取
        /// </summary>
        [SugarColumn(ColumnDescription = "是否自动接取")]
        public bool AutoAccept { get; set; } = false;

        /// <summary>
        /// 是否自动提交
        /// </summary>
        [SugarColumn(ColumnDescription = "是否自动提交")]
        public bool AutoSubmit { get; set; } = false;

        /// <summary>
        /// 任务时间限制（秒），0表示无限制
        /// </summary>
        [SugarColumn(ColumnDescription = "时间限制(秒)，0表示无限制")]
        public int TimeLimit { get; set; } = 0;

        /// <summary>
        /// 奖励经验值
        /// </summary>
        [SugarColumn(ColumnDescription = "奖励经验")]
        public long RewardExp { get; set; } = 0;

        /// <summary>
        /// 奖励金币数量
        /// </summary>
        [SugarColumn(ColumnDescription = "奖励金币")]
        public long RewardGold { get; set; } = 0;

        /// <summary>
        /// 奖励灵石数量
        /// </summary>
        [SugarColumn(ColumnDescription = "奖励灵石")]
        public long RewardSpiritStone { get; set; } = 0;

        /// <summary>
        /// 奖励道具的JSON序列化数据
        /// </summary>
        [SugarColumn(ColumnDescription = "奖励道具JSON", Length = 500, IsNullable = true)]
        public string? RewardItemsJson { get; set; }

        /// <summary>
        /// 奖励装备ID列表，逗号分隔
        /// </summary>
        [SugarColumn(ColumnDescription = "奖励装备ID列表", Length = 200, IsNullable = true)]
        public string? RewardEquipmentIds { get; set; }

        /// <summary>
        /// 任务目标的JSON序列化数据
        /// </summary>
        [SugarColumn(ColumnDescription = "目标JSON", Length = 1000, IsNullable = true)]
        public string? ObjectivesJson { get; set; }

        /// <summary>
        /// 排序顺序，数值越小越靠前
        /// </summary>
        [SugarColumn(ColumnDescription = "排序顺序")]
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// 是否启用该任务
        /// </summary>
        [SugarColumn(ColumnDescription = "是否启用")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 最后更新时间。
        /// </summary>
        [SugarColumn(ColumnDescription = "最后更新时间")]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 任务完成记录实体
    /// 记录玩家已完成的任务历史
    /// </summary>
    [SugarTable("quest_completed_record")]
    public class QuestCompletedRecordEntity
    {
        /// <summary>
        /// 记录唯一标识符
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "记录ID")]
        public string GID { get; set; } = string.Empty;

        /// <summary>
        /// 玩家ID
        /// </summary>
        [SugarColumn(ColumnDescription = "玩家ID")]
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 任务ID
        /// </summary>
        [SugarColumn(ColumnDescription = "任务ID", Length = 50)]
        public string QuestId { get; set; } = string.Empty;

        /// <summary>
        /// 完成任务的时间
        /// </summary>
        [SugarColumn(ColumnDescription = "完成时间")]
        public DateTime CompleteTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 提交任务的时间
        /// </summary>
        [SugarColumn(ColumnDescription = "提交时间", IsNullable = true)]
        public DateTime? SubmitTime { get; set; }
    }
}
