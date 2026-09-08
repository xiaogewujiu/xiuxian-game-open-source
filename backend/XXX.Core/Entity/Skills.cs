namespace XXX.Entity
{
    /// <summary>
    /// 技能
    /// </summary>
    public class Skill
    {
        /// <summary>
        /// 技能唯一ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 技能名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 技能描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 目标类型（1 敌方  2 友方）
        /// </summary>
        public int TargetType { get; set; }

        /// <summary>
        /// 消耗蓝量
        /// </summary>
        public int ManaCost { get; set; }

        /// <summary>
        /// 冷却回合数
        /// </summary>
        public int Cooldown { get; set; }

        /// <summary>
        /// 伤害类型
        /// </summary>
        public DamageType DamageType { get; set; }

        /// <summary>
        /// 伤害段数（兼容旧版本）
        /// </summary>
        public int HitCount { get; set; }

        /// <summary>
        /// 多段伤害配置（新版本）
        /// 如果不为空，则使用此配置覆盖HitCount
        /// </summary>
        public List<SkillHit> Hits { get; set; } = [];

        /// <summary>
        /// 对几个人释放 0为全敌方或者全友方
        /// </summary>
        public int RangeType { get; set; }

        /// <summary>
        /// 伤害倍率（基于攻击力）
        /// 用于兼容旧版本，当Hits为空时使用
        /// </summary>
        public double DamageMultiplier { get; set; }

        /// <summary>
        /// 携带的Buff ID列表
        /// </summary>
        public List<string> BuffIds { get; set; } = [];

        /// <summary>
        /// 技能允许使用的职业列表。
        /// `all` 表示全职业可用。
        /// </summary>
        public List<string> AllowedProfessions { get; set; } = [PlayerProfessionCatalog.All];

        /// <summary>
        /// 技能触发概率（0 ~ 1 之间）
        /// </summary>
        public double TriggerChance;

        /// <summary>
        /// 获取实际的伤害段数
        /// </summary>
        public int GetActualHitCount()
        {
            return Hits != null && Hits.Count > 0 ? Hits.Count : HitCount;
        }

        /// <summary>
        /// 获取第index段的伤害倍率
        /// </summary>
        public double GetHitDamageMultiplier(int index)
        {
            if (Hits != null && Hits.Count > 0 && index < Hits.Count)
                return Hits[index].DamageMultiplier;
            return DamageMultiplier;
        }

        /// <summary>
        /// 获取第index段的基础伤害
        /// </summary>
        public int GetHitBaseDamage(int index)
        {
            if (Hits != null && Hits.Count > 0 && index < Hits.Count)
                return Hits[index].BaseDamage;
            return 0;
        }

        /// <summary>
        /// 获取第index段的伤害类型
        /// </summary>
        public DamageType GetHitDamageType(int index)
        {
            if (Hits != null && Hits.Count > 0 && index < Hits.Count)
                return Hits[index].HitDamageType ?? DamageType;
            return DamageType;
        }
    }

    public enum DamageType
    {
        Physical,        // 物理伤害
        Magic,           // 法术伤害
        True,            // 真实伤害
        Heal,            // 治疗
        Buff,            // 增益/减益（纯Buff技能）
        ManaRestore,     // 恢复蓝量
        Revive,          // 复活术
        BloodSacrifice,  // 血祭技能
        LifeConversion,  // 生命转换技能
        PercentDamage,   // 百分比伤害
        Dispel,          // 驱散 - 移除目标的有益Buff
        Cleanse          // 净化 - 移除目标的有害Buff
    }
}
