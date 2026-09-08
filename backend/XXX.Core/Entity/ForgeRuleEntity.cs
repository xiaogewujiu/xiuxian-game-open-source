using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 锻造职业等级经验配置。
    /// </summary>
    [SugarTable("ForgeProfessionLevelConfigs")]
    public class ForgeProfessionLevelConfigEntity
    {
        [SugarColumn(IsPrimaryKey = true)]
        public int Level { get; set; }

        [SugarColumn]
        public int NextLevelExp { get; set; }

        [SugarColumn]
        public bool IsEnabled { get; set; } = true;

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? SeedKey { get; set; }

        [SugarColumn]
        public bool IsBuiltIn { get; set; } = false;

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? BuiltInVersion { get; set; }

        [SugarColumn]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 锻造职业通用规则配置。
    /// </summary>
    [SugarTable("ForgeProfessionRuleConfigs")]
    public class ForgeProfessionRuleConfigEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 50)]
        public string ConfigId { get; set; } = string.Empty;

        [SugarColumn]
        public int SuccessBonusPerOverLevel { get; set; } = 1;

        [SugarColumn]
        public int MaxSuccessBonus { get; set; } = 15;

        [SugarColumn]
        public int SuccessExpBase { get; set; } = 12;

        [SugarColumn]
        public int SuccessExpPerRequiredLevel { get; set; } = 4;

        [SugarColumn]
        public int FailureExpBase { get; set; } = 6;

        [SugarColumn]
        public int FailureExpPerRequiredLevel { get; set; } = 2;

        [SugarColumn]
        public bool IsEnabled { get; set; } = true;

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? SeedKey { get; set; }

        [SugarColumn]
        public bool IsBuiltIn { get; set; } = false;

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? BuiltInVersion { get; set; }

        [SugarColumn]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }
}
