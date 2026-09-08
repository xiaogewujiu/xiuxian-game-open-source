using XXX.Entity;

namespace XXX.Application.DTOs
{
    /// <summary>
    /// 后台技能模板列表项。
    /// </summary>
    public class AdminSkillListItemDto
    {
        public string SkillCatalog { get; set; } = SkillTemplateEntity.CurrentCatalog;

        /// <summary>
        /// 技能等级。
        /// </summary>
        public int SkillLevel { get; set; }

        /// <summary>
        /// 下一级技能编号。
        /// </summary>
        public int? NextSkillId { get; set; }

        /// <summary>
        /// 技能编号。
        /// </summary>
        public int SkillId { get; set; }

        /// <summary>
        /// 技能名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 目标类型。
        /// </summary>
        public int TargetType { get; set; }

        /// <summary>
        /// 冷却。
        /// </summary>
        public int Cooldown { get; set; }

        /// <summary>
        /// 可用职业范围。
        /// </summary>
        public string AllowedProfessionText { get; set; } = XXX.Entity.PlayerProfessionCatalog.GetDisplayName(XXX.Entity.PlayerProfessionCatalog.All);

        /// <summary>
        /// 是否为内置模板。
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// 内置版本号。
        /// </summary>
        public string? BuiltInVersion { get; set; }
    }

    /// <summary>
    /// 后台关联下一级技能请求。
    /// </summary>
    public class AdminSkillNextLinkRequestDto
    {
        /// <summary>
        /// 下一级技能编号。
        /// </summary>
        public int NextSkillId { get; set; }

        /// <summary>
        /// 当前技能升级到下一级的条件。
        /// </summary>
        public List<SkillUpgradeCondition> UpgradeConditions { get; set; } = [];
    }

    /// <summary>
    /// 后台技能后续等级列表项。
    /// </summary>
    public class AdminSkillNextLevelDto
    {
        /// <summary>当前等级技能编号；升级条件保存于此技能。</summary>
        public int PreviousSkillId { get; set; }
        /// <summary>技能编号。</summary>
        public int SkillId { get; set; }
        /// <summary>技能等级。</summary>
        public int SkillLevel { get; set; }
        /// <summary>技能名称。</summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>伤害类型。</summary>
        public int DamageType { get; set; }
        /// <summary>法力消耗。</summary>
        public int ManaCost { get; set; }
        /// <summary>冷却回合数。</summary>
        public int Cooldown { get; set; }
        /// <summary>
        /// 升级条件。
        /// </summary>
        public List<SkillUpgradeCondition> UpgradeConditions { get; set; } = [];
        /// <summary>升级条件摘要。</summary>
        public string UpgradeConditionSummary { get; set; } = string.Empty;
    }

    /// <summary>
    /// 后台技能模板详情。
    /// </summary>
    public class AdminSkillDetailDto
    {
        public string SkillCatalog { get; set; } = SkillTemplateEntity.CurrentCatalog;

        /// <summary>
        /// 技能等级。
        /// </summary>
        public int SkillLevel { get; set; }

        /// <summary>
        /// 上一级技能编号。
        /// </summary>
        public int? PreviousSkillId { get; set; }

        /// <summary>
        /// 下一级技能编号。
        /// </summary>
        public int? NextSkillId { get; set; }

        /// <summary>
        /// 下一级技能名称。
        /// </summary>
        public string? NextSkillName { get; set; }

        /// <summary>
        /// 下一级技能详情。
        /// </summary>
        public AdminSkillDetailDto? NextSkill { get; set; }

        /// <summary>
        /// 当前技能到下一级的升级条件。
        /// </summary>
        public List<SkillUpgradeCondition> UpgradeConditions { get; set; } = [];

        /// <summary>
        /// 技能编号。
        /// </summary>
        public int SkillId { get; set; }

        /// <summary>
        /// 技能名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 描述。
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 目标类型。
        /// </summary>
        public int TargetType { get; set; }

        /// <summary>
        /// 法力消耗。
        /// </summary>
        public int ManaCost { get; set; }

        /// <summary>
        /// 冷却。
        /// </summary>
        public int Cooldown { get; set; }

        /// <summary>
        /// 伤害类型。
        /// </summary>
        public int DamageType { get; set; }

        /// <summary>
        /// 段数。
        /// </summary>
        public int HitCount { get; set; }

        /// <summary>
        /// 作用范围。
        /// </summary>
        public int RangeType { get; set; }

        /// <summary>
        /// 默认伤害倍率。
        /// </summary>
        public double DamageMultiplier { get; set; }

        /// <summary>
        /// 触发概率。
        /// </summary>
        public double TriggerChance { get; set; }

        /// <summary>
        /// 多段配置。
        /// </summary>
        public List<SkillHit> Hits { get; set; } = [];

        /// <summary>
        /// Buff 编号列表。
        /// </summary>
        public List<string> BuffIds { get; set; } = [];

        /// <summary>
        /// 可用职业列表。
        /// `all` 表示全职业。
        /// </summary>
        public List<string> AllowedProfessions { get; set; } = [XXX.Entity.PlayerProfessionCatalog.All];

        /// <summary>
        /// 是否为内置模板。
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// 内置种子键。
        /// </summary>
        public string? SeedKey { get; set; }

        /// <summary>
        /// 内置版本号。
        /// </summary>
        public string? BuiltInVersion { get; set; }

        /// <summary>
        /// 最后更新时间。
        /// </summary>
        public DateTime? LastUpdateTime { get; set; }
    }
}
