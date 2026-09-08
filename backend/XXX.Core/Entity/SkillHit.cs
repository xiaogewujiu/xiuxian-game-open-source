namespace XXX.Entity
{
    /// <summary>
    /// 技能单次伤害配置
    /// 用于定义多段伤害技能的每段伤害参数
    /// </summary>
    public class SkillHit
    {
        /// <summary>
        /// 伤害倍率（基于攻击力）
        /// </summary>
        public double DamageMultiplier { get; set; }

        /// <summary>
        /// 基础伤害（固定值）
        /// </summary>
        public int BaseDamage { get; set; }

        /// <summary>
        /// 伤害类型（可选）
        /// 用于特殊效果，如：真实伤害、治疗等
        /// </summary>
        public DamageType? HitDamageType { get; set; }

        /// <summary>
        /// 技能描述（可选）
        /// 用于显示每段伤害的说明
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }
}
