namespace XXX.Entity
{
    /// <summary>
    /// Buff目标选择模式
    /// </summary>
    public enum BuffTargetSelectionMode
    {
        /// <summary>
        /// 完全随机 - 从目标池中完全随机选择目标
        /// </summary>
        CompletelyRandom = 1,

        /// <summary>
        /// 优先技能目标 - 优先使用技能命中的目标，不足的部分再从目标池随机选择
        /// </summary>
        PrioritizeSkillTargets = 2,

        /// <summary>
        /// 仅技能目标 - buff目标数等于技能目标数，只对技能命中的目标施加buff，不随机补充
        /// </summary>
        OnlySkillTargets = 3
    }

    /// <summary>
    /// Buff 模板（配置层）
    /// 用于定义一个 Buff 的基础规则与包含的效果集合
    /// 触发时机都是 回合开始时
    /// 通常从数据库 / 配置表中读取
    /// </summary>
    public class BuffTemplate
    {
        /// <summary>
        /// Buff 全局唯一标识（配置表主键）
        /// </summary>
        public string Gid { get; set; } = string.Empty;

        /// <summary>
        /// Buff 名称（用于显示）
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Buff 描述（用于 UI 或策划查看）
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Buff 持续回合数
        /// -1 表示永久存在，直到被驱散或特殊条件移除
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Buff 最大叠加层数
        /// 1 表示不可叠加
        /// </summary>
        public int MaxStack { get; set; }

        /// <summary>
        /// Buff 的叠加规则
        /// 决定再次获得同 Buff 时的处理方式
        /// </summary>
        public StackRule StackRule { get; set; }

        /// <summary>
        /// Buff 包含的效果列表
        /// 一个 Buff 可以包含多个效果（例如：掉血 + 减防）
        /// </summary>
        public List<BuffEffect> Effects { get; set; } = [];
    }

    /// <summary>
    /// Buff 效果定义
    /// Buff 的最小效果单元
    /// 一个Buff可以包含多个效果，例如：
    /// - 狂怒：攻击提升（AttackUp）+ 防御降低（DefenseDown）
    /// - 嗜血：攻击提升（AttackUp）+ 吸血（Lifesteal）
    /// </summary>
    public class BuffEffect
    {
        /// <summary>
        /// Buff 效果类型
        /// （例如：持续伤害、增加攻击、防御降低等）
        /// </summary>
        public BuffEffectType EffectType { get; set; }

        /// <summary>
        /// 效果数值
        /// 可表示固定值或百分比数值
        /// 例如：0.50 表示 50% 的提升
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// 是否为百分比效果
        /// true：按百分比计算（例如：攻击提升50%）
        /// false：固定数值（例如：攻击提升100点）
        /// </summary>
        public bool IsPercentage { get; set; }

        /// <summary>
        /// Buff目标阵营（1=敌方，2=己方）
        /// 用于控制Buff应该施加给哪个阵营
        /// 例如：增益Buff应该给己方(2)，减益Buff应该给敌方(1)
        ///
        /// 组合效果说明：
        /// - 如果Buff包含多个效果，它们应该有相同的目标配置
        /// - 例如：狂怒buff包含攻击提升（己方）和防御降低（敌方），需要拆分为两个buff
        /// </summary>
        public int TargetCamp { get; set; } = 1;

        /// <summary>
        /// Buff目标数量
        /// 0表示全体，大于0表示指定数量的目标
        ///
        /// 组合效果说明：
        /// - 当TargetCount = 0时，对目标阵营的所有存活单位施加buff
        /// - 当TargetCount > 0时，对目标阵营的指定数量单位施加buff
        /// - 例如：TargetCount = 1 表示单体目标，TargetCount = 3 表示3个目标
        /// </summary>
        public int TargetCount { get; set; } = 0;

        /// <summary>
        /// 是否随机选择目标
        /// true：随机根据TargetCamp获取目标
        /// false：使用技能目标（技能实际命中的目标）
        ///
        /// 组合效果说明：
        /// - 当IsRandomTarget = true时，根据TargetCamp从目标阵营中随机选择目标
        /// - 当IsRandomTarget = false时，使用技能实际命中的目标（skillTargets）
        /// - 注意：如果技能目标阵营与buff目标阵营不匹配，则无法正确应用
        /// </summary>
        public bool IsRandomTarget { get; set; } = true;

        /// <summary>
        /// Buff触发概率（0-1），默认1.0表示100%触发
        ///
        /// 组合效果说明：
        /// - 当Buff包含多个效果时，每个效果的触发概率独立计算
        /// - 例如：一个buff包含两个效果，第一个效果触发概率0.8，第二个效果触发概率0.9
        ///   则第一个效果有80%概率触发，第二个效果有90%概率触发
        /// </summary>
        public double TriggerChance { get; set; } = 1.0;

        /// <summary>
        /// Buff目标选择模式
        ///
        /// 组合效果说明：
        /// - CompletelyRandom：完全随机选择目标
        /// - PrioritizeSkillTargets：优先使用技能目标，不足的部分再随机选择
        /// - OnlySkillTargets：只对技能命中的目标施加buff，不随机补充
        /// </summary>
        public BuffTargetSelectionMode TargetSelectionMode { get; set; } = BuffTargetSelectionMode.CompletelyRandom;

        /// <summary>
        /// 是否固定选择施法者自己
        /// true：固定选择施法者自己
        /// false：根据其他配置选择目标
        ///
        /// 组合效果说明：
        /// - 当TargetSelfOnly = true时，忽略其他所有目标选择配置，固定选择施法者自己
        /// - 适用于：给自己施加增益buff（如：狂怒、嗜血、护盾等）
        /// - 优先级最高，会跳过IsRandomTarget、TargetCamp等配置的判断
        /// </summary>
        public bool TargetSelfOnly { get; set; } = false;

        /// <summary>
        /// 施加给目标的持续时间（回合数）
        /// -1：使用 BuffTemplate.Duration 的值
        /// 0：永久效果
        /// 大于0：指定回合数
        ///
        /// 使用场景：
        /// - 被动技能（被动永久 Duration=-1，但施加效果有时限 EffectDuration=2）
        /// - 永久光环（光环永久存在，但施加的效果有持续时间）
        /// </summary>
        public int EffectDuration { get; set; } = -1;
    }

    /// <summary>
    /// Buff 效果类型枚举
    /// 定义 Buff 对目标产生的具体影响
    /// </summary>
    public enum BuffEffectType
    {
        /// <summary>
        /// 持续伤害
        /// 每回合开始时对目标造成伤害（如：中毒、灼烧）
        /// </summary>
        DamageOverTime = 1,

        /// <summary>
        /// 持续治疗
        /// 每回合开始时为目标恢复生命值
        /// </summary>
        HealOverTime = 2,

        /// <summary>
        /// 持续恢复蓝量
        /// 每回合开始时为目标恢复蓝量
        /// </summary>
        ManaOverTime = 17,

        /// <summary>
        /// 眩晕
        /// 目标在眩晕期间无法行动（无法普攻、释放技能）
        /// </summary>
        Stun = 3,

        /// <summary>
        /// 防御降低
        /// 减少目标的防御属性（可为固定值或百分比）
        /// </summary>
        DefenseDown = 4,

        /// <summary>
        /// 防御提升
        /// 提升目标的防御属性（可为固定值或百分比）
        /// </summary>
        DefenseUp = 5,

        /// <summary>
        /// 攻击提升
        /// 提升目标的攻击属性（可为固定值或百分比）
        /// </summary>
        AttackUp = 6,

        /// <summary>
        /// 攻击降低
        /// 降低目标的攻击属性（可为固定值或百分比）
        /// </summary>
        AttackDown = 7,

        /// <summary>
        /// 沉默
        /// 目标无法使用技能，但仍可进行普通攻击（如有区分）
        /// </summary>
        Silence = 8,

        /// <summary>
        /// 暴击率提升
        /// 提升目标的暴击率（可为固定值或百分比）
        /// </summary>
        CritRateUp = 9,

        /// <summary>
        /// 暴击率降低
        /// 降低目标的暴击率（可为固定值或百分比）
        /// </summary>
        CritRateDown = 10,

        /// <summary>
        /// 暴击伤害提升
        /// 提升目标的暴击伤害倍率（可为固定值或百分比）
        /// </summary>
        CritDamageUp = 11,

        /// <summary>
        /// 暴击伤害降低
        /// 降低目标的暴击伤害倍率（可为固定值或百分比）
        /// </summary>
        CritDamageDown = 12,

        /// <summary>
        /// 暴击抵抗
        /// 降低被暴击的概率（可为固定值或百分比）
        /// </summary>
        CritResist = 13,

        /// <summary>
        /// 伤害增幅
        /// 提升造成的最终伤害（可为固定值或百分比）
        /// </summary>
        DamageAmplify = 14,

        /// <summary>
        /// 伤害减免
        /// 降低受到的最终伤害（可为固定值或百分比）
        /// </summary>
        DamageReduction = 15,

        /// <summary>
        /// 嘲讽
        /// 强制敌方攻击嘲讽者
        /// </summary>
        Taunt = 16,

        /// <summary>
        /// 命中率提升
        /// 提升目标的命中率
        /// </summary>
        AccuracyUp = 18,

        /// <summary>
        /// 命中率降低
        /// 降低目标的命中率
        /// </summary>
        AccuracyDown = 19,

        /// <summary>
        /// 吸血
        /// 造成伤害时恢复生命值
        /// </summary>
        Lifesteal = 20,

        /// <summary>
        /// 反伤
        /// 受到伤害时反弹给攻击者
        /// </summary>
        ReflectDamage = 21,

        /// <summary>
        /// 护盾
        /// 吸收伤害
        /// </summary>
        Shield = 22,

        /// <summary>
        /// 免疫控制
        /// 免疫眩晕、沉默、嘲讽等控制效果
        /// </summary>
        Immunity = 23,

        /// <summary>
        /// 蓝量消耗
        /// 每回合消耗蓝量
        /// </summary>
        ManaDrain = 24,

        /// <summary>
        /// 连击率提升
        /// 提升普通攻击的连击概率（可为固定值或百分比）
        /// </summary>
        ComboRateUp = 25,

        /// <summary>
        /// 连击率降低
        /// 降低普通攻击的连击概率（可为固定值或百分比）
        /// </summary>
        ComboRateDown = 26,

        /// <summary>
        /// 连击伤害提升
        /// 提升连击造成的伤害（可为固定值或百分比）
        /// </summary>
        ComboDamageUp = 27,

        /// <summary>
        /// 连击伤害降低
        /// 降低连击造成的伤害（可为固定值或百分比）
        /// </summary>
        ComboDamageDown = 28,

        /// <summary>
        /// 反击率提升
        /// 提升被攻击时的反击概率（可为固定值或百分比）
        /// </summary>
        CounterRateUp = 29,

        /// <summary>
        /// 反击率降低
        /// 降低被攻击时的反击概率（可为固定值或百分比）
        /// </summary>
        CounterRateDown = 30,

        /// <summary>
        /// 反击伤害提升
        /// 提升反击造成的伤害（可为固定值或百分比）
        /// </summary>
        CounterDamageUp = 31,

        /// <summary>
        /// 反击伤害降低
        /// 降低反击造成的伤害（可为固定值或百分比）
        /// </summary>
        CounterDamageDown = 32,

        /// <summary>
        /// 禁疗
        /// 目标无法回复生命值
        /// </summary>
        HealBlock = 33,

        /// <summary>
        /// 减速
        /// 降低目标的速度（可为固定值或百分比）
        /// </summary>
        Slow = 34,

        /// <summary>
        /// 加速
        /// 提升目标的速度（可为固定值或百分比）
        /// </summary>
        Haste = 35,

        /// <summary>
        /// 易伤
        /// 受到的伤害增加（可为固定值或百分比）
        /// </summary>
        Vulnerability = 36,

        /// <summary>
        /// 不屈
        /// 受到致死伤害时保留1点生命值（每场战斗触发一次）
        /// </summary>
        Undying = 37,

        /// <summary>
        /// 冷却缩减
        /// 技能冷却时间减少（可为固定值或百分比）
        /// </summary>
        CooldownReduction = 38,

        /// <summary>
        /// 冷却增加
        /// 技能冷却时间增加（可为固定值或百分比）
        /// </summary>
        CooldownIncrease = 39,

        /// <summary>
        /// 斩杀
        /// 对低血量目标（血量低于阈值）造成额外伤害
        /// </summary>
        Execute = 40,

        /// <summary>
        /// 镜像
        /// 创建一个分身，继承本体部分属性参与战斗
        /// </summary>
        Mirror = 41,

        /// <summary>
        /// 缴械
        /// 目标无法使用普通攻击
        /// </summary>
        Disarm = 42,

        /// <summary>
        /// 束缚
        /// 目标无法反击
        /// </summary>
        Root = 43,

        /// <summary>
        /// 破防
        /// 直接降低目标的防御值（非百分比）
        /// </summary>
        ArmorBreak = 44,

        /// <summary>
        /// 庇护
        /// 为友方分担伤害
        /// </summary>
        Sanctuary = 45,

        /// <summary>
        /// 复活
        /// 死亡时自动复活并恢复一定生命值
        /// </summary>
        AutoRevive = 46,

        /// <summary>
        /// 无敌
        /// 免疫所有伤害
        /// </summary>
        Invincible = 47,

        /// <summary>
        /// 魅惑
        /// 目标会攻击友方单位
        /// </summary>
        Charm = 48
    }

    /// <summary>
    /// Buff 叠加规则
    /// 用于控制同一 Buff 再次获得时的处理逻辑
    /// </summary>
    public enum StackRule
    {
        /// <summary>
        /// 刷新持续时间
        /// 层数不变，持续回合数重置
        /// </summary>
        RefreshDuration,

        /// <summary>
        /// 数值叠加
        /// 每层叠加效果数值，持续时间独立或统一
        /// </summary>
        StackValue,

        /// <summary>
        /// 替换 Buff
        /// 新 Buff 覆盖旧 Buff
        /// </summary>
        Replace
    }
}
