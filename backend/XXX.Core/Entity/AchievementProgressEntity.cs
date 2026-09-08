using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 成就进度实体
    /// 记录玩家的成就完成进度
    /// </summary>
    [SugarTable("achievement_progress")]
    public class AchievementProgressEntity
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
        /// 成就ID
        /// </summary>
        [SugarColumn(ColumnDescription = "成就ID", Length = 50)]
        public string AchievementId { get; set; } = string.Empty;

        /// <summary>
        /// 当前进度值
        /// </summary>
        [SugarColumn(ColumnDescription = "当前进度")]
        public long CurrentProgress { get; set; } = 0;

        /// <summary>
        /// 目标进度值
        /// </summary>
        [SugarColumn(ColumnDescription = "目标进度")]
        public long TargetProgress { get; set; } = 0;

        /// <summary>
        /// 多条件进度JSON。
        /// </summary>
        [SugarColumn(ColumnDescription = "条件进度JSON", Length = 4000, IsNullable = true)]
        public string? RequirementProgressJson { get; set; }

        /// <summary>
        /// 成就状态：0-未开始，1-进行中，2-已完成，3-已领取
        /// </summary>
        [SugarColumn(ColumnDescription = "状态")]
        public int Status { get; set; } = 0;

        /// <summary>
        /// 开始追踪成就的时间
        /// </summary>
        [SugarColumn(ColumnDescription = "接取时间", IsNullable = true)]
        public DateTime? AcceptTime { get; set; }

        /// <summary>
        /// 完成成就的时间
        /// </summary>
        [SugarColumn(ColumnDescription = "完成时间", IsNullable = true)]
        public DateTime? CompleteTime { get; set; }

        /// <summary>
        /// 领取奖励的时间
        /// </summary>
        [SugarColumn(ColumnDescription = "领取时间", IsNullable = true)]
        public DateTime? ClaimTime { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        [SugarColumn(ColumnDescription = "最后更新时间")]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 成就配置实体
    /// 存储成就的配置信息，支持数据库动态配置
    /// </summary>
    [SugarTable("achievement_config")]
    public class AchievementConfigEntity
    {
        /// <summary>
        /// 成就唯一标识符
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "成就ID")]
        public string AchievementId { get; set; } = string.Empty;

        /// <summary>
        /// 内置种子键。
        /// </summary>
        [SugarColumn(ColumnDescription = "内置种子键", Length = 100, IsNullable = true)]
        public string? SeedKey { get; set; }

        /// <summary>
        /// 是否为系统内置成就。
        /// </summary>
        [SugarColumn(ColumnDescription = "是否内置")]
        public bool IsBuiltIn { get; set; } = false;

        /// <summary>
        /// 内置版本号。
        /// </summary>
        [SugarColumn(ColumnDescription = "内置版本", Length = 100, IsNullable = true)]
        public string? BuiltInVersion { get; set; }

        /// <summary>
        /// 成就名称
        /// </summary>
        [SugarColumn(ColumnDescription = "成就名称", Length = 50)]
        public string AchievementName { get; set; } = string.Empty;

        /// <summary>
        /// 成就类型：0-等级，1-战斗，2-装备，3-收集，4-社交
        /// </summary>
        [SugarColumn(ColumnDescription = "成就类型")]
        public int AchievementType { get; set; } = 0;

        /// <summary>
        /// 难度等级：1-简单，2-普通，3-困难，4-极难
        /// </summary>
        [SugarColumn(ColumnDescription = "难度")]
        public int Difficulty { get; set; } = 0;

        /// <summary>
        /// 成就描述
        /// </summary>
        [SugarColumn(ColumnDescription = "描述", Length = 200)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 成就分类名称
        /// </summary>
        [SugarColumn(ColumnDescription = "分类", Length = 50)]
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// 完成成就获得的点数
        /// </summary>
        [SugarColumn(ColumnDescription = "成就点数")]
        public int Points { get; set; } = 0;

        /// <summary>
        /// 是否为隐藏成就
        /// </summary>
        [SugarColumn(ColumnDescription = "是否隐藏")]
        public bool IsHidden { get; set; } = false;

        /// <summary>
        /// 前置成就ID列表，逗号分隔
        /// </summary>
        [SugarColumn(ColumnDescription = "前置成就ID", Length = 200, IsNullable = true)]
        public string? PreAchievementIds { get; set; }

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
        /// 奖励经验值
        /// </summary>
        [SugarColumn(ColumnDescription = "奖励经验")]
        public long RewardExp { get; set; } = 0;

        /// <summary>
        /// 奖励的称号名称
        /// </summary>
        [SugarColumn(ColumnDescription = "奖励称号", Length = 50, IsNullable = true)]
        public string? RewardTitle { get; set; }

        /// <summary>
        /// 奖励装备ID列表，逗号分隔
        /// </summary>
        [SugarColumn(ColumnDescription = "奖励装备ID列表", Length = 200, IsNullable = true)]
        public string? RewardEquipmentIds { get; set; }

        /// <summary>
        /// 奖励道具JSON。
        /// </summary>
        [SugarColumn(ColumnDescription = "奖励道具JSON", Length = 1000, IsNullable = true)]
        public string? RewardItemsJson { get; set; }

        /// <summary>
        /// 要求类型：0-等级，1-击杀，2-收集，3-胜利次数等
        /// </summary>
        [SugarColumn(ColumnDescription = "要求类型")]
        public int RequirementType { get; set; } = 0;

        /// <summary>
        /// 要求达到的目标值
        /// </summary>
        [SugarColumn(ColumnDescription = "要求目标值")]
        public long RequirementTargetValue { get; set; } = 0;

        /// <summary>
        /// 要求的描述文本
        /// </summary>
        [SugarColumn(ColumnDescription = "要求描述", Length = 100, IsNullable = true)]
        public string? RequirementDescription { get; set; }

        /// <summary>
        /// 多条件JSON。
        /// </summary>
        [SugarColumn(ColumnDescription = "条件JSON", Length = 4000, IsNullable = true)]
        public string? RequirementsJson { get; set; }

        /// <summary>
        /// 排序顺序，数值越小越靠前
        /// </summary>
        [SugarColumn(ColumnDescription = "排序顺序")]
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// 是否启用该成就
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
    /// 成就统计计数器实体。
    /// </summary>
    [SugarTable("achievement_metric_counter")]
    public class AchievementMetricCounterEntity
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        [SugarColumn(ColumnDescription = "玩家ID", Length = 50)]
        public string PlayerId { get; set; } = string.Empty;

        [SugarColumn(ColumnDescription = "要求类型")]
        public int RequirementType { get; set; }

        [SugarColumn(ColumnDescription = "目标ID", Length = 100, IsNullable = true)]
        public string? TargetId { get; set; }

        [SugarColumn(ColumnDescription = "当前统计值")]
        public long CurrentValue { get; set; }

        [SugarColumn(ColumnDescription = "最后更新时间")]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }
}
