using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 世界 Boss 当前状态。
    /// </summary>
    public enum WorldBossInstanceState
    {
        /// <summary>
        /// 已创建但尚未进入有效战斗阶段。
        /// 目前常规流程基本不会停留在这个状态，主要为后续扩展预留。
        /// </summary>
        Pending = 0,

        /// <summary>
        /// 正在开放中的世界 Boss 实例。
        /// 玩家进入弹窗后可以在这个状态下参战。
        /// </summary>
        Active = 1,

        /// <summary>
        /// 已正常结算完成。
        /// 例如 Boss 被击败后进入该状态。
        /// </summary>
        Settled = 2,

        /// <summary>
        /// 被后台或系统主动关闭。
        /// </summary>
        Closed = 3,

        /// <summary>
        /// 到达结束时间后超时结束。
        /// </summary>
        Expired = 4
    }

    /// <summary>
    /// 世界 Boss 选取策略。
    /// </summary>
    public enum WorldBossSelectionMode
    {
        /// <summary>
        /// 按模板权重随机选择一个 Boss。
        /// </summary>
        Random = 0,

        /// <summary>
        /// 按模板列表顺序轮换选择 Boss。
        /// </summary>
        Sequential = 1
    }

    /// <summary>
    /// 世界 Boss 模板。
    /// 复用怪物模板的属性与技能，这里只放活动专属配置。
    /// </summary>
    [SugarTable("WorldBossTemplates")]
    public class WorldBossTemplateEntity
    {
        /// <summary>
        /// Boss 模板唯一编号。
        /// 后台新增、编辑和手动生成都会以它作为主键。
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, Length = 100)]
        public string BossId { get; set; } = string.Empty;

        /// <summary>
        /// Boss 展示名称。
        /// 前后端界面、公告和日志都会直接显示这个名称。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 关联的怪物模板编号。
        /// 世界 Boss 的基础属性、技能和被动都复用该怪物模板的数据。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false)]
        public string MonsterTemplateId { get; set; } = string.Empty;

        /// <summary>
        /// Boss 画像资源路径。
        /// 为空时前端会回退到默认图标。
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string? PortraitPath { get; set; }

        /// <summary>
        /// 是否允许该模板参与排期或手动生成。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 随机选取模式下的抽取权重。
        /// 权重越高，被随机刷新的概率越大。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public int Weight { get; set; } = 1;

        /// <summary>
        /// 单次世界 Boss 持续时长，单位为分钟。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "60")]
        public int DurationMinutes { get; set; } = 60;

        /// <summary>
        /// Boss 生成时推送到玩家日志区的公告文案。
        /// 留空时会使用系统默认公告。
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true)]
        public string? NoticeText { get; set; }

        /// <summary>
        /// 参与奖励的最低伤害门槛。
        /// 玩家总伤害达到该值后才能领取基础参与奖励。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public long ParticipationMinDamage { get; set; } = 1;

        /// <summary>
        /// 参与奖励中的经验值数量。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public long ParticipationRewardExp { get; set; } = 0;

        /// <summary>
        /// 参与奖励中的金币数量。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public long ParticipationRewardGold { get; set; } = 0;

        /// <summary>
        /// 参与奖励中的灵石数量。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public long ParticipationRewardSpiritStone { get; set; } = 0;

        /// <summary>
        /// 第 1 名额外奖励的经验值数量。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public long Rank1RewardExp { get; set; } = 0;

        /// <summary>
        /// 第 1 名额外奖励的金币数量。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public long Rank1RewardGold { get; set; } = 0;

        /// <summary>
        /// 第 1 名额外奖励的灵石数量。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public long Rank1RewardSpiritStone { get; set; } = 0;

        /// <summary>
        /// 第 2 名额外奖励的经验值数量。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public long Rank2RewardExp { get; set; } = 0;

        /// <summary>
        /// 第 2 名额外奖励的金币数量。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public long Rank2RewardGold { get; set; } = 0;

        /// <summary>
        /// 第 2 名额外奖励的灵石数量。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public long Rank2RewardSpiritStone { get; set; } = 0;

        /// <summary>
        /// 第 3 名额外奖励的经验值数量。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public long Rank3RewardExp { get; set; } = 0;

        /// <summary>
        /// 第 3 名额外奖励的金币数量。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public long Rank3RewardGold { get; set; } = 0;

        /// <summary>
        /// 第 3 名额外奖励的灵石数量。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public long Rank3RewardSpiritStone { get; set; } = 0;

        /// <summary>
        /// 后台模板列表的排序值。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// 模板最后一次编辑时间。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 世界 Boss 每日排期。
    /// 当前首版只需要单条默认排期，但实体保留可扩展能力。
    /// </summary>
    [SugarTable("WorldBossSchedules")]
    public class WorldBossScheduleEntity
    {
        /// <summary>
        /// 排期记录主键。
        /// 当前系统只使用默认值 <c>default</c> 这一条记录。
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, Length = 100)]
        public string ScheduleId { get; set; } = "default";

        /// <summary>
        /// 每日触发刷新时间，格式固定为 <c>HH:mm</c>。
        /// </summary>
        [SugarColumn(Length = 10, IsNullable = false)]
        public string SpawnTimeText { get; set; } = "11:00";

        /// <summary>
        /// 排期使用的时区标识。
        /// 由后台配置页面写入，用于把 UTC 时间换算成业务时间。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false)]
        public string TimeZoneId { get; set; } = "China Standard Time";

        /// <summary>
        /// 每日刷新时采用的 Boss 选择模式。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public WorldBossSelectionMode SelectionMode { get; set; } = WorldBossSelectionMode.Random;

        /// <summary>
        /// 是否启用该排期。
        /// 关闭后后台任务不会自动生成世界 Boss。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 排期最后一次编辑时间。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 世界 Boss 活动实例。
    /// </summary>
    [SugarTable("WorldBossInstances")]
    public class WorldBossInstanceEntity
    {
        /// <summary>
        /// 世界 Boss 实例唯一编号。
        /// 每次刷新或后台手动生成都会创建新实例。
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, Length = 100)]
        public string InstanceId { get; set; } = string.Empty;

        /// <summary>
        /// 本次实例使用的 Boss 模板编号。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false)]
        public string BossId { get; set; } = string.Empty;

        /// <summary>
        /// 本次实例的 Boss 展示名称。
        /// 生成时会把模板名称固化下来，避免后续编辑模板影响历史实例。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false)]
        public string BossName { get; set; } = string.Empty;

        /// <summary>
        /// 本次实例绑定的怪物模板编号。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false)]
        public string MonsterTemplateId { get; set; } = string.Empty;

        /// <summary>
        /// 本次实例使用的 Boss 画像路径快照。
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string? PortraitPath { get; set; }

        /// <summary>
        /// 本次实例的公告文案快照。
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true)]
        public string? NoticeText { get; set; }

        /// <summary>
        /// 当前实例状态。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public WorldBossInstanceState State { get; set; } = WorldBossInstanceState.Active;

        /// <summary>
        /// 实例实际生成时间，使用 UTC 记录。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime SpawnedAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 实例预定结束时间，使用 UTC 记录。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime EndAtUtc { get; set; } = DateTime.UtcNow.AddHours(1);

        /// <summary>
        /// 实例结算完成时间，使用 UTC 记录。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? SettledAtUtc { get; set; }

        /// <summary>
        /// Boss 当前气血值。
        /// 运行时每次保存快照都会同步到该字段。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int CurrentHp { get; set; }

        /// <summary>
        /// Boss 最大气血值。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int MaxHp { get; set; }

        /// <summary>
        /// Boss 当前灵力值。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int CurrentMp { get; set; }

        /// <summary>
        /// Boss 最大灵力值。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int MaxMp { get; set; }

        /// <summary>
        /// 世界 Boss 完整战斗态的 JSON 快照。
        /// 包含 Boss、参战玩家、镜像和 Buff 等运行时信息。
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = false)]
        public string CombatStateJson { get; set; } = "{}";

        /// <summary>
        /// 实例最后一次持久化时间。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 世界 Boss 参战者记录。
    /// </summary>
    [SugarTable("WorldBossParticipants")]
    public class WorldBossParticipantEntity
    {
        /// <summary>
        /// 参战记录唯一编号。
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, Length = 100)]
        public string ParticipantId { get; set; } = string.Empty;

        /// <summary>
        /// 所属世界 Boss 实例编号。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false)]
        public string InstanceId { get; set; } = string.Empty;

        /// <summary>
        /// 参战玩家编号。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false)]
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 玩家加入战斗时的展示名称快照。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false)]
        public string PlayerName { get; set; } = string.Empty;

        /// <summary>
        /// 当前是否处于自动战斗模式。
        /// 手动窗口超时后也会被系统强制改成真。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public bool IsAuto { get; set; } = false;

        /// <summary>
        /// 本场累计造成的总伤害。
        /// 排名和参与奖励都会基于这个值计算。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public long TotalDamage { get; set; } = 0;

        /// <summary>
        /// 本场累计治疗量。
        /// 当前主要用于记录表现，尚未进入奖励结算规则。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public long TotalHeal { get; set; } = 0;

        /// <summary>
        /// 本场累计死亡次数。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int DeathCount { get; set; } = 0;

        /// <summary>
        /// 进入世界 Boss 战场的时间，使用 UTC 记录。
        /// 相同伤害时会用这个时间作为排序兜底。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime JoinAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 最近一次实际出手时间，使用 UTC 记录。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? LastActionAtUtc { get; set; }

        /// <summary>
        /// 下次允许出手的时间，使用 UTC 记录。
        /// 当前规则里每次出手后都会进入 5 秒冷却。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime ReadyAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 当前手动操作窗口截止时间，使用 UTC 记录。
        /// 玩家在此之前不操作就会被切换到自动战斗。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime DeadlineAtUtc { get; set; } = DateTime.UtcNow.AddSeconds(5);

        /// <summary>
        /// 复活完成时间，使用 UTC 记录。
        /// 为空表示当前未处于死亡等待阶段。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? ReviveAtUtc { get; set; }

        /// <summary>
        /// 当前实例的结算奖励是否已被玩家领取。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public bool RewardClaimed { get; set; } = false;

        /// <summary>
        /// 参战记录最后一次更新时间。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 世界 Boss 奖励结算记录。
    /// </summary>
    [SugarTable("WorldBossRewardRecords")]
    public class WorldBossRewardRecordEntity
    {
        /// <summary>
        /// 奖励记录唯一编号。
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, Length = 100)]
        public string RecordId { get; set; } = string.Empty;

        /// <summary>
        /// 所属世界 Boss 实例编号。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false)]
        public string InstanceId { get; set; } = string.Empty;

        /// <summary>
        /// 奖励归属玩家编号。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false)]
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 结算时玩家名称快照。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false)]
        public string PlayerName { get; set; } = string.Empty;

        /// <summary>
        /// 结算排名。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int Rank { get; set; } = 0;

        /// <summary>
        /// 结算时记录下来的总伤害。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public long Damage { get; set; } = 0;

        /// <summary>
        /// 本条奖励中包含的经验值数量。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public long RewardExp { get; set; } = 0;

        /// <summary>
        /// 本条奖励中包含的金币数量。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public long RewardGold { get; set; } = 0;

        /// <summary>
        /// 本条奖励中包含的灵石数量。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public long RewardSpiritStone { get; set; } = 0;

        /// <summary>
        /// 该奖励是否已领取。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public bool IsClaimed { get; set; } = false;

        /// <summary>
        /// 奖励领取时间，使用 UTC 记录。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? ClaimedAtUtc { get; set; }

        /// <summary>
        /// 奖励记录创建时间，使用 UTC 记录。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime CreateTime { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// 世界 Boss 战斗日志。
    /// </summary>
    [SugarTable("WorldBossLogs")]
    public class WorldBossLogEntity
    {
        /// <summary>
        /// 战斗日志唯一编号。
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, Length = 100)]
        public string LogId { get; set; } = string.Empty;

        /// <summary>
        /// 所属世界 Boss 实例编号。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false)]
        public string InstanceId { get; set; } = string.Empty;

        /// <summary>
        /// 实例内递增的日志序号。
        /// 前端会按该值倒序展示日志。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public long Seq { get; set; } = 0;

        /// <summary>
        /// 日志产生时间，使用 UTC 记录。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 动作发起者编号。
        /// 系统日志可能为空。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string? ActorId { get; set; }

        /// <summary>
        /// 动作发起者名称。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string? ActorName { get; set; }

        /// <summary>
        /// 动作目标编号。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string? TargetId { get; set; }

        /// <summary>
        /// 动作目标名称。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string? TargetName { get; set; }

        /// <summary>
        /// 日志动作类型。
        /// 当前会使用 <c>spawn</c>、<c>auto</c>、<c>skill</c>、<c>normal</c> 等短标识。
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        public string ActionType { get; set; } = "normal";

        /// <summary>
        /// 给前端直接展示的日志正文。
        /// </summary>
        [SugarColumn(Length = 2000, IsNullable = false)]
        public string Content { get; set; } = string.Empty;
    }
}
