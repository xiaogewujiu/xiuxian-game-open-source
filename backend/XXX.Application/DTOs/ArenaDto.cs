namespace XXX.Application.DTOs
{
    /// <summary>
    /// 竞技场个人信息DTO。
    /// </summary>
    public class ArenaMeDto
    {
        public int Points { get; set; }
        public int Rank { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int WinStreak { get; set; }
        public int BestRank { get; set; }
        public int DailyAttemptsRemaining { get; set; }
        public int DailyAttemptsMax { get; set; }
        public int PurchasableAttempts { get; set; }
        public int SeasonNumber { get; set; }
        public string? SeasonEndAt { get; set; }
    }

    /// <summary>
    /// 推荐对手DTO。
    /// </summary>
    public class ArenaOpponentDto
    {
        public string PlayerId { get; set; } = string.Empty;
        public string PlayerName { get; set; } = string.Empty;
        public int Level { get; set; }
        public string Profession { get; set; } = string.Empty;
        public int Points { get; set; }
        public int Rank { get; set; }
        public string? GuildName { get; set; }
        public long PowerScore { get; set; }
    }

    /// <summary>
    /// 挑战结果DTO。
    /// </summary>
    public class ArenaChallengeResultDto
    {
        public bool IsWin { get; set; }
        public int PointsChange { get; set; }
        public int NewPoints { get; set; }
        public int OldRank { get; set; }
        public int NewRank { get; set; }
        public int RewardGold { get; set; }
        public int RewardHonor { get; set; }
        public string? BattleLogJson { get; set; }
        public string OpponentName { get; set; } = string.Empty;
    }

    /// <summary>
    /// 对战历史DTO。
    /// </summary>
    public class ArenaBattleLogDto
    {
        public long Id { get; set; }
        public string OpponentName { get; set; } = string.Empty;
        public int OpponentLevel { get; set; }
        public bool IsWin { get; set; }
        public int PointsChange { get; set; }
        public string BattleTime { get; set; } = string.Empty;
        public bool IsAttacker { get; set; }
        public string? BattleLogJson { get; set; }
    }

    /// <summary>
    /// 赛季信息DTO。
    /// </summary>
    public class ArenaSeasonDto
    {
        public int SeasonNumber { get; set; }
        public string? StartAt { get; set; }
        public string? EndAt { get; set; }
        public string TimeRemaining { get; set; } = string.Empty;
        public int MyRank { get; set; }
        public int MyPoints { get; set; }
        public List<ArenaSeasonRewardDto> Rewards { get; set; } = [];
    }

    /// <summary>
    /// 赛季奖励DTO。
    /// </summary>
    public class ArenaSeasonRewardDto
    {
        public int MinRank { get; set; }
        public int MaxRank { get; set; }
        public string RewardTitle { get; set; } = string.Empty;
        public long Gold { get; set; }
        public long SpiritStone { get; set; }
        public string? Title { get; set; }
    }
}
