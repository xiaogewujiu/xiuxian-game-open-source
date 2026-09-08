using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 秘境实例配置模板。
    /// </summary>
    [SugarTable("DungeonInstanceTemplates")]
    public class DungeonInstanceTemplateEntity : ConfigEntity
    {
        /// <summary>
        /// 是否启用。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// 推荐等级。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int RecommendedLevel { get; set; }

        /// <summary>
        /// 每日进入次数限制。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public int DailyEnterLimit { get; set; } = 1;

        /// <summary>
        /// tick 间隔秒数。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int TickIntervalSeconds { get; set; }

        /// <summary>
        /// 开放时间配置 JSON。
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? OpenScheduleJson { get; set; }

        /// <summary>
        /// 进入消耗配置 JSON。
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? EntryCostsJson { get; set; }

        /// <summary>
        /// 关联的事件组 ID。
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true)]
        public string? EventGroupId { get; set; }

        /// <summary>
        /// 自动用药配置 JSON。
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? AutoMedicineConfigJson { get; set; }

        /// <summary>
        /// 玩家偶遇配置 JSON（好感度概率阈值 + PvP 掠夺规则）。
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? EncounterConfigJson { get; set; }

        /// <summary>
        /// 种子数据标识。
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string? SeedKey { get; set; }

        /// <summary>
        /// 是否为内置种子数据。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public bool IsBuiltIn { get; set; } = false;

        /// <summary>
        /// 内置数据版本。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string? BuiltInVersion { get; set; }

        /// <summary>
        /// 最后更新时间。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 秘境运行实例。
    /// </summary>
    [SugarTable("DungeonInstances")]
    public class DungeonInstanceEntity : PlayerEntity
    {
        /// <summary>
        /// 关联的秘境模板 ID。
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        public string DungeonId { get; set; } = string.Empty;

        /// <summary>
        /// 实例状态。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int Status { get; set; }

        /// <summary>
        /// 进入时间。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime EnterTime { get; set; }

        /// <summary>
        /// 上次 tick 时间。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? LastTickTime { get; set; }

        /// <summary>
        /// 下次 tick 时间。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? NextTickTime { get; set; }

        /// <summary>
        /// 结算时间。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? SettleTime { get; set; }

        /// <summary>
        /// 结算原因。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? SettleReason { get; set; }

        /// <summary>
        /// 角色快照 JSON。
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? SnapshotJson { get; set; }

        /// <summary>
        /// 当前生命值。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public long CurrentHp { get; set; }

        /// <summary>
        /// 当前法力值。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public long CurrentMp { get; set; }

        /// <summary>
        /// 最大生命值。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public long MaxHp { get; set; }

        /// <summary>
        /// 最大法力值。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public long MaxMp { get; set; }

        /// <summary>
        /// 秘境内临时 buff 列表 JSON。
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? ActiveBuffsJson { get; set; }

        /// <summary>
        /// 探索日志 JSON。
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? ExploreLogJson { get; set; }

        /// <summary>
        /// 关联的秘境临时队伍 ID。null 表示未组队。
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true)]
        public string? PartyId { get; set; }

        /// <summary>
        /// 是否为秘境临时队伍队长。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public bool IsPartyLeader { get; set; }
    }

    /// <summary>
    /// 秘境每日进入记录。
    /// </summary>
    [SugarTable("DungeonInstanceDailyRecords")]
    public class DungeonInstanceDailyRecordEntity : PlayerEntity
    {
        /// <summary>
        /// 秘境模板 ID。
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        public string DungeonId { get; set; } = string.Empty;

        /// <summary>
        /// 进入日期（自然日）。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime EnterDate { get; set; }

        /// <summary>
        /// 当日已进入次数。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int EnterCount { get; set; }
    }

    /// <summary>
    /// 秘境临时收益池。
    /// </summary>
    [SugarTable("DungeonRewardPools")]
    public class DungeonRewardPoolEntity : PlayerEntity
    {
        /// <summary>
        /// 关联的秘境实例 ID。
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        public string InstanceId { get; set; } = string.Empty;

        /// <summary>
        /// 奖励类型（Gold/Exp/Item/Equipment/Collection）。
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        public string RewardType { get; set; } = string.Empty;

        /// <summary>
        /// 奖励 ID（道具 ID / 装备模板 ID / 空）。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string? RewardId { get; set; }

        /// <summary>
        /// 数量。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int Quantity { get; set; }

        /// <summary>
        /// 来源（Battle/Resource/Treasure/Adventure/PlayerEncounter）。
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// 死亡时是否保留。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public bool DeathKeep { get; set; }
    }

    /// <summary>
    /// 秘境临时队伍。
    /// </summary>
    [SugarTable("DungeonParties")]
    public class DungeonPartyEntity
    {
        /// <summary>
        /// 队伍 ID。
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, Length = 50)]
        public string PartyId { get; set; } = string.Empty;

        /// <summary>
        /// 所在秘境模板 ID。
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        public string DungeonId { get; set; } = string.Empty;

        /// <summary>
        /// 队长玩家 ID。
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        public string LeaderPlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 成员列表 JSON。
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = false)]
        public string MemberJson { get; set; } = "[]";

        /// <summary>
        /// 创建时间。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 最后更新时间。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 秘境事件组。
    /// </summary>
    [SugarTable("DungeonEventGroups")]
    public class DungeonEventGroupEntity : ConfigEntity
    {
        /// <summary>
        /// 组内事件列表 JSON（EventGroupItem[]）。
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? GroupItemsJson { get; set; }

        /// <summary>
        /// 种子数据标识。
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string? SeedKey { get; set; }

        /// <summary>
        /// 是否为内置种子数据。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public bool IsBuiltIn { get; set; } = false;

        /// <summary>
        /// 内置数据版本。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string? BuiltInVersion { get; set; }

        /// <summary>
        /// 最后更新时间。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }
}
