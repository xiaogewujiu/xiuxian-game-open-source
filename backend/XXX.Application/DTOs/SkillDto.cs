using XXX.Entity;

namespace XXX.Application.DTOs
{
    /// <summary>
    /// 中文注释：
    /// 技能总览接口返回结构。
    /// 这里把“当前角色已携带技能”和“技能总库”一次性返回，
    /// 前端技能弹窗打开后就能完整展示描述、Buff、耗蓝、冷却等信息。
    /// </summary>
    public class SkillOverviewDto
    {
        /// <summary>
        /// 可携带技能槽上限。
        /// </summary>
        public int MaxSlots { get; set; } = 6;

        /// <summary>
        /// 当前版本的技能系统提示。
        /// 用来明确告诉前端和玩家：哪些能力已接入真实后端，哪些还没有。
        /// </summary>
        public string Notice { get; set; } = string.Empty;

        /// <summary>
        /// 当前已携带技能列表。
        /// </summary>
        public List<SkillDetailDto> EquippedSkills { get; set; } = [];

        /// <summary>
        /// 当前已掌握技能库。
        /// </summary>
        public List<SkillDetailDto> LibrarySkills { get; set; } = [];
    }

    /// <summary>
    /// 技能升级条件展示 DTO。
    /// </summary>
    public class SkillUpgradeConditionDto
    {
        /// <summary>
        /// 条件类型。
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// 条件目标名称。
        /// </summary>
        public string TargetName { get; set; } = string.Empty;

        /// <summary>
        /// 需要数量或等级。
        /// </summary>
        public long Amount { get; set; }

        /// <summary>
        /// 当前数量或等级。
        /// </summary>
        public long CurrentAmount { get; set; }

        /// <summary>
        /// 当前是否满足。
        /// </summary>
        public bool IsMet { get; set; }

        /// <summary>
        /// 条件展示文本。
        /// </summary>
        public string StatusText { get; set; } = string.Empty;
    }

    /// <summary>
    /// 技能升级请求 DTO。
    /// </summary>
    public class SkillUpgradeRequestDto
    {
        /// <summary>
        /// 要升级的当前技能编号。
        /// </summary>
        public int SkillId { get; set; }
    }

    /// <summary>
    /// 技能升级结果 DTO。
    /// </summary>
    public class SkillUpgradeResultDto
    {
        /// <summary>
        /// 最新技能总览。
        /// </summary>
        public SkillOverviewDto Overview { get; set; } = new();

        /// <summary>
        /// 当前金币余额。
        /// </summary>
        public long Gold { get; set; }

        /// <summary>
        /// 当前灵石余额。
        /// </summary>
        public long SpiritStone { get; set; }

        /// <summary>
        /// 本次涉及道具最新数量。
        /// </summary>
        public List<SkillUpgradeItemBalanceDto> ItemBalances { get; set; } = [];
    }

    /// <summary>
    /// 技能升级道具余额 DTO。
    /// </summary>
    public class SkillUpgradeItemBalanceDto
    {
        /// <summary>
        /// 道具模板编号。
        /// </summary>
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// 当前总数量。
        /// </summary>
        public int Quantity { get; set; }
    }

    /// <summary>
    /// 技能详情 DTO。
    /// </summary>
    public class SkillDetailDto
    {
        /// <summary>
        /// 当前技能等级。
        /// </summary>
        public int SkillLevel { get; set; }

        /// <summary>
        /// 下一级技能编号；没有下一级时为空。
        /// </summary>
        public int? NextSkillId { get; set; }

        /// <summary>
        /// 下一级技能预览；没有下一级时为空。
        /// </summary>
        public SkillDetailDto? NextSkill { get; set; }

        /// <summary>
        /// 是否存在可升级的下一级技能。
        /// </summary>
        public bool HasNextSkill { get; set; }

        /// <summary>
        /// 当前玩家是否可以升级该技能。
        /// </summary>
        public bool CanUpgrade { get; set; }

        /// <summary>
        /// 升级条件及当前满足状态。
        /// </summary>
        public List<SkillUpgradeConditionDto> UpgradeConditions { get; set; } = [];

        /// <summary>
        /// 升级状态提示。
        /// </summary>
        public string UpgradeStatusText { get; set; } = string.Empty;

        /// <summary>
        /// 技能 ID。
        /// </summary>
        public int SkillId { get; set; }

        /// <summary>
        /// 技能名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 技能描述。
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 伤害类型文本。
        /// </summary>
        public string DamageType { get; set; } = string.Empty;

        /// <summary>
        /// 目标类型文本。
        /// </summary>
        public string TargetType { get; set; } = string.Empty;

        /// <summary>
        /// 作用范围文本。
        /// </summary>
        public string RangeText { get; set; } = string.Empty;

        /// <summary>
        /// 法力消耗。
        /// </summary>
        public int ManaCost { get; set; }

        /// <summary>
        /// 冷却回合数。
        /// </summary>
        public int Cooldown { get; set; }

        /// <summary>
        /// 命中段数。
        /// </summary>
        public int HitCount { get; set; }

        /// <summary>
        /// 伤害倍率。
        /// </summary>
        public double DamageMultiplier { get; set; }

        /// <summary>
        /// 触发概率。
        /// </summary>
        public double TriggerChance { get; set; }

        /// <summary>
        /// 当前是否已携带。
        /// </summary>
        public bool IsEquipped { get; set; }

        /// <summary>
        /// 当前是否已掌握。
        /// </summary>
        public bool IsOwned { get; set; }

        /// <summary>
        /// 当前是否允许操作。
        /// </summary>
        public bool CanOperate { get; set; }

        /// <summary>
        /// 技能拥有状态文案。
        /// </summary>
        public string OwnershipText { get; set; } = string.Empty;

        /// <summary>
        /// 技能附带 Buff 列表。
        /// </summary>
        public List<SkillBuffDto> Buffs { get; set; } = [];
    }

    /// <summary>
    /// 技能附带 Buff 的展示 DTO。
    /// </summary>
    public class SkillBuffDto
    {
        /// <summary>
        /// Buff ID。
        /// </summary>
        public string BuffId { get; set; } = string.Empty;

        /// <summary>
        /// Buff 名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Buff 描述。
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 持续回合数。
        /// </summary>
        public int Duration { get; set; }
    }

    /// <summary>
    /// 技能装备/卸下请求 DTO。
    /// </summary>
    public class SkillOperateRequestDto
    {
        /// <summary>
        /// 要操作的技能 ID。
        /// </summary>
        public int SkillId { get; set; }
    }
}
