namespace XXX.Entity
{
    /// <summary>
    /// 战斗日志类型
    /// </summary>
    public enum BattleLogType
    {
        // 行动类
        ActionStart,         // 行动开始
        SkillUse,            // 使用技能
        NormalAttack,        // 普通攻击

        // 伤害类
        DamageDealt,         // 造成伤害
        DamageTaken,         // 受到伤害
        CritDamage,          // 暴击伤害
        Dodge,               // 闪避
        Miss,                // 未命中

        // 治疗类
        Heal,                // 治疗
        Revive,              // 复活
        Lifesteal,           // 吸血

        // Buff类
        BuffApply,           // 施加Buff
        BuffRemove,          // Buff移除
        BuffExpired,         // Buff过期
        BuffResist,          // Buff被抵抗
        Dispel,              // 驱散
        Cleanse,             // 净化

        // 控制类
        Stun,                // 眩晕
        Silence,             // 沉默
        Disarm,              // 缴械
        Root,                // 束缚

        // 特殊效果
        CounterAttack,       // 反击
        ComboAttack,         // 连击
        ReflectDamage,       // 反伤
        ShieldAbsorb,        // 护盾吸收
        SanctuaryShare,      // 庇护分担
        Charm,               // 魅惑

        // 状态变化
        Death,               // 死亡
        Kill,                // 击杀
        Execute,             // 斩杀

        // 系统
        RoundStart,          // 回合开始
        RoundEnd,            // 回合结束
        BattleStart,         // 战斗开始
        BattleEnd,           // 战斗结束
        PassiveTrigger,      // 被动触发
    }
}
