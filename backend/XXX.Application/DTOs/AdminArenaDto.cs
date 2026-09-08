namespace XXX.Application.DTOs
{
    /// <summary>
    /// 管理后台竞技场总览DTO。
    /// </summary>
    public class AdminArenaOverviewDto
    {
        public int SeasonNumber { get; set; }
        public int TotalPlayers { get; set; }
        public int TodayBattles { get; set; }
        public int ActivePlayers { get; set; }
        public int AveragePoints { get; set; }
    }

    /// <summary>
    /// 管理后台竞技场玩家列表项DTO。
    /// </summary>
    public class AdminArenaPlayerDto
    {
        public long Id { get; set; }
        public string PlayerId { get; set; } = string.Empty;
        public string PlayerName { get; set; } = string.Empty;
        public int Points { get; set; }
        public int Rank { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int WinStreak { get; set; }
        public int BestRank { get; set; }
        public DateTime? LastBattleAt { get; set; }
        public DateTime? BannedUntil { get; set; }
        public string? BanReason { get; set; }
    }

    /// <summary>
    /// 管理后台对战日志DTO。
    /// </summary>
    public class AdminArenaBattleLogDto
    {
        public long Id { get; set; }
        public string AttackerId { get; set; } = string.Empty;
        public string AttackerName { get; set; } = string.Empty;
        public string DefenderId { get; set; } = string.Empty;
        public string DefenderName { get; set; } = string.Empty;
        public int AttackerPointsBefore { get; set; }
        public int DefenderPointsBefore { get; set; }
        public int AttackerPointsAfter { get; set; }
        public int DefenderPointsAfter { get; set; }
        public string WinnerId { get; set; } = string.Empty;
        public string? BattleLogJson { get; set; }
        public int SeasonNumber { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// 管理后台调整积分请求DTO。
    /// </summary>
    public class AdminAdjustPointsDto
    {
        public string PlayerId { get; set; } = string.Empty;
        public int Points { get; set; }
    }

    /// <summary>
    /// 管理后台禁赛请求DTO。
    /// </summary>
    public class AdminBanPlayerDto
    {
        public string PlayerId { get; set; } = string.Empty;
        public int Hours { get; set; } = 24;
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// 管理后台解禁请求DTO。
    /// </summary>
    public class AdminUnbanPlayerDto
    {
        public string PlayerId { get; set; } = string.Empty;
    }

    /// <summary>
    /// 管理后台竞技场赛季信息DTO。
    /// </summary>
    public class AdminArenaSeasonDto
    {
        public int SeasonNumber { get; set; }
        public bool SeasonEnabled { get; set; }
        public int SeasonDuration { get; set; }
        public DateTime SeasonStartTime { get; set; }
        public DateTime SeasonEndTime { get; set; }
        public int DaysRemaining { get; set; }
        public int TotalPlayers { get; set; }
        public List<AdminArenaSeasonRewardDto> Rewards { get; set; } = [];
    }

    /// <summary>
    /// 管理后台竞技场赛季奖励DTO。
    /// </summary>
    public class AdminArenaSeasonRewardDto
    {
        public string GID { get; set; } = string.Empty;
        public int MinRank { get; set; }
        public int MaxRank { get; set; }
        public string RewardTitle { get; set; } = string.Empty;
        public long Gold { get; set; }
        public long SpiritStone { get; set; }
        public string? Title { get; set; }
    }

    /// <summary>
    /// 管理后台赛季结算结果DTO。
    /// </summary>
    public class AdminSettleSeasonResultDto
    {
        public int SeasonNumber { get; set; }
        public int RewardedPlayers { get; set; }
        public List<string> Messages { get; set; } = [];
    }

    /// <summary>
    /// 管理后台调整赛季时长请求DTO。
    /// </summary>
    public class AdminAdjustSeasonDto
    {
        public int SeasonDuration { get; set; } = 30;
        public bool? SeasonEnabled { get; set; }
    }

    /// <summary>
    /// 管理后台保存赛季奖励请求DTO。
    /// </summary>
    public class AdminSaveSeasonRewardDto
    {
        public string? GID { get; set; }
        public int MinRank { get; set; }
        public int MaxRank { get; set; }
        public string RewardTitle { get; set; } = string.Empty;
        public long Gold { get; set; }
        public long SpiritStone { get; set; }
        public string? Title { get; set; }
    }
}
