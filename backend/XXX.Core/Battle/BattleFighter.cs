using XXX.Entity;

namespace XXX.Battle
{
    /// <summary>
    /// 战斗单位
    /// </summary>
    public class BattleFighter
    {
        /// <summary>
        /// 使用的模版id
        /// </summary>
        public string MonsterTempID { get; set; } = string.Empty;
        /// <summary>
        /// 唯一ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 是否为我方
        /// </summary>
        public bool IsPlayerSide { get; set; }

        /// <summary>
        /// 战斗单位类型
        /// </summary>
        public FighterType FighterType { get; set; }

        /// <summary>
        /// 当前血量
        /// </summary>
        public int CurrentHp { get; set; }

        /// <summary>
        /// 最大血量
        /// </summary>
        public int MaxHp { get; set; }

        /// <summary>
        /// 当前蓝量
        /// </summary>
        public int CurrentMp { get; set; }

        /// <summary>
        /// 最大蓝量
        /// </summary>
        public int MaxMp { get; set; }

        /// <summary>
        /// 物理攻击
        /// </summary>
        public int PhysicalAttack { get; set; }

        /// <summary>
        /// 法术攻击
        /// </summary>
        public int MagicAttack { get; set; }

        /// <summary>
        /// 物理防御
        /// </summary>
        public int PhysicalDefense { get; set; }

        /// <summary>
        /// 法术防御
        /// </summary>
        public int MagicDefense { get; set; }

        /// <summary>
        /// 速度
        /// </summary>
        public int Speed { get; set; }

        /// <summary>
        /// 命中率
        /// </summary>
        public float HitRate { get; set; }

        /// <summary>
        /// 闪避率
        /// </summary>
        public float DodgeRate { get; set; }

        /// <summary>
        /// 暴击率
        /// </summary>
        public float CritRate { get; set; }

        /// <summary>
        /// 暴击伤害倍率
        /// </summary>
        public float CritDamage { get; set; }

        /// <summary>
        /// 连击率
        /// </summary>
        public float ComboRate { get; set; }

        /// <summary>
        /// 反击率
        /// </summary>
        public float CounterRate { get; set; }

        /// <summary>
        /// 破甲率
        /// </summary>
        public float ArmorBreak { get; set; }

        /// <summary>
        /// 额外伤害倍率
        /// </summary>
        public float ExtraDamage { get; set; }

        /// <summary>
        /// 技能ID列表
        /// </summary>
        public List<string> SkillIds { get; set; } = [];

        /// <summary>
        /// 被动Buff ID列表（永久Buff，战斗开始时应用）
        /// </summary>
        public List<string> PassiveIds { get; set; } = [];

        /// <summary>
        /// 技能冷却时间
        /// </summary>
        public Dictionary<int, int> SkillCooldowns { get; set; } = [];

        /// <summary>
        /// 本回合使用的技能ID列表
        /// </summary>
        public HashSet<int> SkillsUsedThisRound { get; set; } = [];

        /// <summary>
        /// 活动Buff列表
        /// </summary>
        public List<ActiveBuff> ActiveBuffs { get; set; } = [];

        /// <summary>
        /// 本回合连击次数
        /// </summary>
        public int ComboCountThisRound { get; set; }

        /// <summary>
        /// 本回合对每个攻击者的反击次数（按攻击者ID记录）
        /// </summary>
        public Dictionary<string, int> CounterCountPerAttacker { get; set; } = [];

        /// <summary>
        /// 灵根属性
        /// </summary>
        public Element Element { get; set; } = Element.None;

        // ========== 分身相关字段 ==========

        /// <summary>
        /// 分身列表
        /// </summary>
        public List<BattleFighter> Mirrors { get; set; } = [];

        /// <summary>
        /// 是否为分身
        /// </summary>
        public bool IsMirror { get; set; } = false;

        /// <summary>
        /// 分身的主人
        /// </summary>
        public BattleFighter? MirrorOwner { get; set; }

        /// <summary>
        /// 分身持续回合数
        /// </summary>
        public int MirrorDuration { get; set; }

        /// <summary>
        /// 分身属性继承比例（攻击力）
        /// </summary>
        public float MirrorAttackRatio { get; set; } = 0.4f;

        /// <summary>
        /// 分身属性继承比例（血量）
        /// </summary>
        public float MirrorHpRatio { get; set; } = 0.5f;
    }

    /// <summary>
    /// 战斗单位类型
    /// </summary>
    public enum FighterType
    {
        Player,
        Monster,
        Pet,
        Mirror  // 分身
    }
}
