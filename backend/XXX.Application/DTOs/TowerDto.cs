namespace XXX.Application.DTOs
{
    public class TowerMeDto
    {
        public int HighestFloor { get; set; }
        public int CurrentFloor { get; set; }
        public int DailyAttemptsRemaining { get; set; }
        public int DailyAttemptsMax { get; set; }
        public int PurchasableAttempts { get; set; }
        public long? BestClearTimeMs { get; set; }
        public string? LastAttemptAt { get; set; }
    }

    public class TowerFloorSummaryDto
    {
        public int Floor { get; set; }
        public string MonsterName { get; set; } = string.Empty;
        public int MonsterLevel { get; set; }
        public bool IsMilestone { get; set; }
        public bool IsCleared { get; set; }
        public string RewardPreview { get; set; } = string.Empty;
    }

    public class TowerChallengeResultDto
    {
        public bool IsWin { get; set; }
        public int Floor { get; set; }
        public string? BattleLogJson { get; set; }
        public int RewardGold { get; set; }
        public int RewardExp { get; set; }
        public string? MilestoneReward { get; set; }
        public int NewHighestFloor { get; set; }
        public int DailyAttemptsRemaining { get; set; }
    }

    public class TowerBattleLogDto
    {
        public long Id { get; set; }
        public int Floor { get; set; }
        public bool IsWin { get; set; }
        public string? BattleLogJson { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
    }
}
