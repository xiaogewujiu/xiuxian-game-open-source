namespace XXX.Application.DTOs
{
    public class AdminTowerOverviewDto
    {
        public int TotalPlayers { get; set; }
        public int TodayChallenges { get; set; }
        public int AverageHighestFloor { get; set; }
    }

    public class AdminTowerPlayerDto
    {
        public long Id { get; set; }
        public string PlayerId { get; set; } = string.Empty;
        public string PlayerName { get; set; } = string.Empty;
        public int HighestFloor { get; set; }
        public int CurrentFloor { get; set; }
        public int DailyAttemptsUsed { get; set; }
        public DateTime? LastAttemptAt { get; set; }
    }

    public class AdminTowerFloorConfigDto
    {
        public long Id { get; set; }
        public int Floor { get; set; }
        public string MonsterTemplateIdsJson { get; set; } = "[]";
        public int MonsterCount { get; set; }
        public float StatMultiplier { get; set; }
        public int RewardGold { get; set; }
        public int RewardExp { get; set; }
        public string? MilestoneRewardJson { get; set; }
    }

    public class AdminTowerBatchGenerateDto
    {
        public string MonsterTemplateIdsJson { get; set; } = "[]";
        public int MonsterCountPerFloor { get; set; } = 2;
        public float BaseMultiplier { get; set; } = 1.0f;
        public float MultiplierGrowth { get; set; } = 0.05f;
        public int BaseRewardGold { get; set; } = 100;
        public int BaseRewardExp { get; set; } = 50;
    }

    public class TowerFloorDistributionDto
    {
        public int Floor { get; set; }
        public int PlayerCount { get; set; }
    }
}
