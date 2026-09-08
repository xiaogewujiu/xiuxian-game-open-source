using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 通天塔进度实体。
    /// </summary>
    [SugarTable("TowerProgress")]
    public class TowerProgressEntity
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        [SugarColumn(Length = 50, IsNullable = false)]
        public string PlayerId { get; set; } = string.Empty;

        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int HighestFloor { get; set; }

        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public int CurrentFloor { get; set; } = 1;

        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int DailyAttemptsUsed { get; set; }

        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int DailyAttemptsPurchased { get; set; }

        [SugarColumn(IsNullable = true)]
        public long? BestClearTimeMs { get; set; }

        [SugarColumn(IsNullable = true)]
        public DateTime? LastAttemptAt { get; set; }

        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public int SeasonNumber { get; set; } = 1;

        [SugarColumn(IsNullable = false)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [SugarColumn(IsNullable = false)]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 通天塔楼层配置实体。
    /// </summary>
    [SugarTable("TowerFloorConfigs")]
    public class TowerFloorConfigEntity
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        [SugarColumn(IsNullable = false)]
        public int Floor { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = false)]
        public string MonsterTemplateIdsJson { get; set; } = "[]";

        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public int MonsterCount { get; set; } = 1;

        [SugarColumn(IsNullable = false, DefaultValue = "1.0")]
        public float StatMultiplier { get; set; } = 1.0f;

        [SugarColumn(IsNullable = false, DefaultValue = "100")]
        public int RewardGold { get; set; } = 100;

        [SugarColumn(IsNullable = false, DefaultValue = "50")]
        public int RewardExp { get; set; } = 50;

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? MilestoneRewardJson { get; set; }

        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public bool IsBuiltIn { get; set; } = true;

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? SeedKey { get; set; }
    }

    /// <summary>
    /// 通天塔战斗日志实体。
    /// </summary>
    [SugarTable("TowerBattleLogs")]
    public class TowerBattleLogEntity
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        [SugarColumn(Length = 50, IsNullable = false)]
        public string PlayerId { get; set; } = string.Empty;

        [SugarColumn(IsNullable = false)]
        public int Floor { get; set; }

        [SugarColumn(IsNullable = false)]
        public bool IsWin { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? BattleLogJson { get; set; }

        [SugarColumn(IsNullable = false)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
