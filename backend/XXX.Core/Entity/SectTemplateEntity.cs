using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 宗门模板实体（预设宗门）
    /// </summary>
    [SugarTable("sect_templates")]
    public class SectTemplateEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 100)]
        public string SectId { get; set; } = string.Empty;

        [SugarColumn(Length = 100)]
        public string Name { get; set; } = string.Empty;

        [SugarColumn(Length = 2000, IsNullable = true)]
        public string? Description { get; set; }

        [SugarColumn(Length = 500, IsNullable = true)]
        public string? Icon { get; set; }

        [SugarColumn(Length = 500, IsNullable = true)]
        public string? PortraitPath { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? HeartSutraIdsJson { get; set; }

        [SugarColumn(DefaultValue = "1")]
        public bool IsEnabled { get; set; } = true;

        [SugarColumn(DefaultValue = "0")]
        public int SortOrder { get; set; } = 0;

        [SugarColumn]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 心法模板实体
    /// </summary>
    [SugarTable("heart_sutra_templates")]
    public class HeartSutraTemplateEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 100)]
        public string SutraId { get; set; } = string.Empty;

        [SugarColumn(Length = 100)]
        public string Name { get; set; } = string.Empty;

        [SugarColumn(Length = 2000, IsNullable = true)]
        public string? Description { get; set; }

        [SugarColumn(Length = 100)]
        public string SectId { get; set; } = string.Empty;

        [SugarColumn(DefaultValue = "10")]
        public int MaxLayer { get; set; } = 10;

        [SugarColumn(ColumnDataType = "TEXT")]
        public string LayersJson { get; set; } = "[]";

        [SugarColumn(DefaultValue = "0")]
        public int SortOrder { get; set; } = 0;

        [SugarColumn]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 玩家心法进度实体
    /// </summary>
    [SugarTable("player_heart_sutras")]
    public class PlayerHeartSutraEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 100)]
        public string GID { get; set; } = string.Empty;

        [SugarColumn(Length = 100)]
        public string PlayerId { get; set; } = string.Empty;

        [SugarColumn(Length = 100)]
        public string SutraId { get; set; } = string.Empty;

        [SugarColumn(DefaultValue = "0")]
        public int CurrentLayer { get; set; } = 0;

        [SugarColumn]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 宗门捐献记录实体
    /// </summary>
    [SugarTable("sect_donation_records")]
    public class SectDonationRecordEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 100)]
        public string GID { get; set; } = string.Empty;

        [SugarColumn(Length = 100)]
        public string PlayerId { get; set; } = string.Empty;

        [SugarColumn(Length = 100)]
        public string GuildId { get; set; } = string.Empty;

        [SugarColumn(DefaultValue = "0")]
        public long GoldDonated { get; set; } = 0;

        [SugarColumn(DefaultValue = "0")]
        public int ContributionEarned { get; set; } = 0;

        [SugarColumn]
        public DateTime DonateTime { get; set; } = DateTime.Now;

        [SugarColumn(Length = 10)]
        public string DonateDate { get; set; } = string.Empty;
    }

    /// <summary>
    /// 宗门大比实体
    /// </summary>
    [SugarTable("sect_tournaments")]
    public class SectTournamentEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 100)]
        public string TournamentId { get; set; } = string.Empty;

        [SugarColumn(Length = 100)]
        public string GuildId { get; set; } = string.Empty;

        [SugarColumn]
        public DateTime StartTime { get; set; }

        [SugarColumn]
        public DateTime EndTime { get; set; }

        [SugarColumn(DefaultValue = "0")]
        public int State { get; set; } = 0;

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? ResultsJson { get; set; }

        [SugarColumn]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 宗门大比对战记录实体
    /// </summary>
    [SugarTable("sect_tournament_matches")]
    public class SectTournamentMatchEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 100)]
        public string MatchId { get; set; } = string.Empty;

        [SugarColumn(Length = 100)]
        public string TournamentId { get; set; } = string.Empty;

        [SugarColumn(Length = 100)]
        public string Player1Id { get; set; } = string.Empty;

        [SugarColumn(Length = 100)]
        public string Player1Name { get; set; } = string.Empty;

        [SugarColumn(Length = 100)]
        public string Player2Id { get; set; } = string.Empty;

        [SugarColumn(Length = 100)]
        public string Player2Name { get; set; } = string.Empty;

        [SugarColumn(DefaultValue = "0")]
        public int Round { get; set; } = 0;

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? WinnerId { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? BattleLogJson { get; set; }

        [SugarColumn]
        public DateTime MatchTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 天骄赛实体
    /// </summary>
    [SugarTable("genius_tournaments")]
    public class GeniusTournamentEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 100)]
        public string TournamentId { get; set; } = string.Empty;

        [SugarColumn(DefaultValue = "1")]
        public int Season { get; set; } = 1;

        [SugarColumn]
        public DateTime StartTime { get; set; }

        [SugarColumn]
        public DateTime EndTime { get; set; }

        [SugarColumn(DefaultValue = "0")]
        public int State { get; set; } = 0;

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? ResultsJson { get; set; }

        [SugarColumn]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 天骄赛对战记录实体
    /// </summary>
    [SugarTable("genius_tournament_matches")]
    public class GeniusTournamentMatchEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 100)]
        public string MatchId { get; set; } = string.Empty;

        [SugarColumn(Length = 100)]
        public string TournamentId { get; set; } = string.Empty;

        [SugarColumn(Length = 100)]
        public string Player1Id { get; set; } = string.Empty;

        [SugarColumn(Length = 100)]
        public string Player1Name { get; set; } = string.Empty;

        [SugarColumn(Length = 100)]
        public string Player1SectName { get; set; } = string.Empty;

        [SugarColumn(Length = 100)]
        public string Player2Id { get; set; } = string.Empty;

        [SugarColumn(Length = 100)]
        public string Player2Name { get; set; } = string.Empty;

        [SugarColumn(Length = 100)]
        public string Player2SectName { get; set; } = string.Empty;

        [SugarColumn(DefaultValue = "0")]
        public int Round { get; set; } = 0;

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? WinnerId { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? BattleLogJson { get; set; }

        [SugarColumn]
        public DateTime MatchTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 宗门大比排期配置实体
    /// </summary>
    [SugarTable("sect_tournament_schedules")]
    public class SectTournamentScheduleEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 100)]
        public string ScheduleId { get; set; } = "default";

        [SugarColumn(Length = 10)]
        public string SpawnTimeText { get; set; } = "20:00";

        [SugarColumn(Length = 100)]
        public string TimeZoneId { get; set; } = "China Standard Time";

        [SugarColumn(DefaultValue = "1")]
        public bool IsEnabled { get; set; } = true;

        [SugarColumn(DefaultValue = "60")]
        public int DurationMinutes { get; set; } = 60;

        [SugarColumn]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 宗门Boss模板实体
    /// </summary>
    [SugarTable("sect_boss_templates")]
    public class SectBossTemplateEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 100)]
        public string BossId { get; set; } = string.Empty;

        [SugarColumn(Length = 100)]
        public string Name { get; set; } = string.Empty;

        [SugarColumn(Length = 100)]
        public string MonsterTemplateId { get; set; } = string.Empty;

        [SugarColumn(Length = 500, IsNullable = true)]
        public string? PortraitPath { get; set; }

        [SugarColumn(DefaultValue = "1")]
        public bool IsEnabled { get; set; } = true;

        [SugarColumn(DefaultValue = "30")]
        public int DurationMinutes { get; set; } = 30;

        [SugarColumn(DefaultValue = "0")]
        public long ParticipationRewardContribution { get; set; } = 0;

        [SugarColumn(DefaultValue = "0")]
        public long Rank1RewardContribution { get; set; } = 0;

        [SugarColumn(DefaultValue = "0")]
        public long Rank2RewardContribution { get; set; } = 0;

        [SugarColumn(DefaultValue = "0")]
        public long Rank3RewardContribution { get; set; } = 0;

        [SugarColumn(DefaultValue = "0")]
        public int SortOrder { get; set; } = 0;

        [SugarColumn]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 宗门Boss实例实体
    /// </summary>
    [SugarTable("sect_boss_instances")]
    public class SectBossInstanceEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 100)]
        public string InstanceId { get; set; } = string.Empty;

        [SugarColumn(Length = 100)]
        public string GuildId { get; set; } = string.Empty;

        [SugarColumn(Length = 100)]
        public string BossId { get; set; } = string.Empty;

        [SugarColumn(Length = 100)]
        public string BossName { get; set; } = string.Empty;

        [SugarColumn(Length = 100)]
        public string MonsterTemplateId { get; set; } = string.Empty;

        [SugarColumn(DefaultValue = "1")]
        public int State { get; set; } = 1;

        [SugarColumn]
        public DateTime SpawnedAtUtc { get; set; } = DateTime.UtcNow;

        [SugarColumn]
        public DateTime EndAtUtc { get; set; }

        [SugarColumn(DefaultValue = "0")]
        public int CurrentHp { get; set; }

        [SugarColumn(DefaultValue = "0")]
        public int MaxHp { get; set; }

        [SugarColumn(ColumnDataType = "TEXT")]
        public string CombatStateJson { get; set; } = "{}";

        [SugarColumn]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 宗门商店配置实体
    /// </summary>
    [SugarTable("sect_shop_configs")]
    public class SectShopConfigEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 100)]
        public string ShopId { get; set; } = string.Empty;

        [SugarColumn(Length = 100)]
        public string ShopName { get; set; } = string.Empty;

        [SugarColumn(Length = 500, IsNullable = true)]
        public string? Description { get; set; }

        [SugarColumn(DefaultValue = "1")]
        public bool IsOpen { get; set; } = true;

        [SugarColumn]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 宗门商店商品实体
    /// </summary>
    [SugarTable("sect_shop_items")]
    public class SectShopItemEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 100)]
        public string GID { get; set; } = string.Empty;

        [SugarColumn(Length = 100)]
        public string ShopId { get; set; } = string.Empty;

        [SugarColumn(Length = 100)]
        public string ItemId { get; set; } = string.Empty;

        [SugarColumn(DefaultValue = "0")]
        public int ItemType { get; set; } = 0;

        [SugarColumn(DefaultValue = "0")]
        public int ContributionCost { get; set; } = 0;

        [SugarColumn(DefaultValue = "-1")]
        public int Stock { get; set; } = -1;

        [SugarColumn(DefaultValue = "-1")]
        public int DailyLimit { get; set; } = -1;

        [SugarColumn(DefaultValue = "0")]
        public int SortOrder { get; set; } = 0;

        [SugarColumn]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 宗门福利配置实体
    /// </summary>
    [SugarTable("sect_blessing_configs")]
    public class SectBlessingConfigEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 100)]
        public string BlessingId { get; set; } = string.Empty;

        [SugarColumn(Length = 100)]
        public string Name { get; set; } = string.Empty;

        [SugarColumn(Length = 500, IsNullable = true)]
        public string? Description { get; set; }

        [SugarColumn(DefaultValue = "1")]
        public int RequiredGuildLevel { get; set; } = 1;

        [SugarColumn(Length = 100)]
        public string BuffId { get; set; } = string.Empty;

        [SugarColumn(DefaultValue = "0")]
        public int SortOrder { get; set; } = 0;

        [SugarColumn]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 心法层配置（非数据库实体，嵌套JSON）
    /// </summary>
    public class SutraLayerConfig
    {
        public int Layer { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ContributionCost { get; set; }
        public long GoldCost { get; set; }
        public List<SutraAttributeBonus> Bonuses { get; set; } = [];
        public string? UnlockSkillId { get; set; }
        public string? UnlockBuffId { get; set; }
    }

    /// <summary>
    /// 心法属性加成（非数据库实体，嵌套JSON）
    /// </summary>
    public class SutraAttributeBonus
    {
        public string AttributeName { get; set; } = string.Empty;
        public double Value { get; set; }
        public bool IsPercentage { get; set; } = false;
    }
}
