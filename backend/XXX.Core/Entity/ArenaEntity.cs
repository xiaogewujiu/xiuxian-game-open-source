using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 竞技场玩家实体。
    /// </summary>
    [SugarTable("ArenaPlayers")]
    public class ArenaPlayerEntity
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        /// <summary>
        /// 玩家GID。
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 竞技场积分。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1000")]
        public int Points { get; set; } = 1000;

        /// <summary>
        /// 当前排名。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int Rank { get; set; }

        /// <summary>
        /// 本赛季胜场。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int Wins { get; set; }

        /// <summary>
        /// 本赛季负场。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int Losses { get; set; }

        /// <summary>
        /// 当前连胜数。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int WinStreak { get; set; }

        /// <summary>
        /// 历史最高排名。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int BestRank { get; set; }

        /// <summary>
        /// 今日已用挑战次数。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int DailyBattlesUsed { get; set; }

        /// <summary>
        /// 今日已购买次数。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int DailyBattlesPurchased { get; set; }

        /// <summary>
        /// 最后一次战斗时间。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? LastBattleAt { get; set; }

        /// <summary>
        /// 所属赛季编号。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public int SeasonNumber { get; set; } = 1;

        /// <summary>
        /// 创建时间。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新时间。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 禁赛截止时间。为null表示未禁赛。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? BannedUntil { get; set; }

        /// <summary>
        /// 禁赛原因。
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string? BanReason { get; set; }
    }

    /// <summary>
    /// 竞技场战斗日志实体。
    /// </summary>
    [SugarTable("ArenaBattleLogs")]
    public class ArenaBattleLogEntity
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        /// <summary>
        /// 挑战方GID。
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        public string AttackerId { get; set; } = string.Empty;

        /// <summary>
        /// 被挑战方GID。
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        public string DefenderId { get; set; } = string.Empty;

        /// <summary>
        /// 挑战方名称。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false)]
        public string AttackerName { get; set; } = string.Empty;

        /// <summary>
        /// 被挑战方名称。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false)]
        public string DefenderName { get; set; } = string.Empty;

        /// <summary>
        /// 挑战前积分。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int AttackerPointsBefore { get; set; }

        /// <summary>
        /// 被挑战前积分。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int DefenderPointsBefore { get; set; }

        /// <summary>
        /// 挑战后积分。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int AttackerPointsAfter { get; set; }

        /// <summary>
        /// 被挑战后积分。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int DefenderPointsAfter { get; set; }

        /// <summary>
        /// 胜者GID。
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        public string WinnerId { get; set; } = string.Empty;

        /// <summary>
        /// 战斗回放日志（JSON）。
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? BattleLogJson { get; set; }

        /// <summary>
        /// 赛季编号。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int SeasonNumber { get; set; }

        /// <summary>
        /// 战斗时间。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
