namespace XXX.Application.DTOs
{
    /// <summary>
    /// 锻造职业等级规则列表项。
    /// </summary>
    public class AdminForgeProfessionLevelRuleDto
    {
        /// <summary>
        /// 锻造职业等级。
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 升到下一级所需职业经验。
        /// </summary>
        public int NextLevelExp { get; set; }

        /// <summary>
        /// 当前等级规则是否启用。
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 是否为内置种子规则。
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// 内置规则版本号。
        /// </summary>
        public string? BuiltInVersion { get; set; }

        /// <summary>
        /// 内置种子键。
        /// </summary>
        public string? SeedKey { get; set; }

        /// <summary>
        /// 最近一次编辑时间。
        /// </summary>
        public DateTime? LastUpdateTime { get; set; }
    }

    /// <summary>
    /// 锻造职业通用规则详情。
    /// </summary>
    public class AdminForgeProfessionRuleDto
    {
        /// <summary>
        /// 通用规则配置编号。
        /// </summary>
        public string ConfigId { get; set; } = string.Empty;

        /// <summary>
        /// 配方等级每超出 1 级时，额外增加的成功率。
        /// </summary>
        public int SuccessBonusPerOverLevel { get; set; }

        /// <summary>
        /// 成功率额外加成的上限。
        /// </summary>
        public int MaxSuccessBonus { get; set; }

        /// <summary>
        /// 锻造成功时获得的基础职业经验。
        /// </summary>
        public int SuccessExpBase { get; set; }

        /// <summary>
        /// 锻造成功时，每个需求等级额外增加的职业经验。
        /// </summary>
        public int SuccessExpPerRequiredLevel { get; set; }

        /// <summary>
        /// 锻造失败时获得的基础职业经验。
        /// </summary>
        public int FailureExpBase { get; set; }

        /// <summary>
        /// 锻造失败时，每个需求等级额外增加的职业经验。
        /// </summary>
        public int FailureExpPerRequiredLevel { get; set; }

        /// <summary>
        /// 当前通用规则是否启用。
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 是否为内置种子规则。
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// 内置规则版本号。
        /// </summary>
        public string? BuiltInVersion { get; set; }

        /// <summary>
        /// 内置种子键。
        /// </summary>
        public string? SeedKey { get; set; }

        /// <summary>
        /// 最近一次编辑时间。
        /// </summary>
        public DateTime? LastUpdateTime { get; set; }
    }
}
