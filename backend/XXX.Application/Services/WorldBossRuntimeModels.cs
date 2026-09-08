using System.Text.Json.Serialization;
using XXX.Battle;
using XXX.Entity;

namespace XXX.Application.Services
{
    /// <summary>
    /// 世界 Boss 持久化战斗态的根对象。
    /// 会被序列化到实例表的 <c>CombatStateJson</c> 字段中。
    /// </summary>
    internal sealed class WorldBossCombatState
    {
        /// <summary>
        /// 当前 Boss 的战斗状态快照。
        /// </summary>
        public WorldBossFighterState Boss { get; set; } = new();

        /// <summary>
        /// 当前所有参战玩家的战斗状态快照。
        /// </summary>
        public List<WorldBossFighterState> Players { get; set; } = [];
    }

    /// <summary>
    /// 世界 Boss 战斗单位快照。
    /// Boss、本体玩家和镜像分身都会落成这个结构。
    /// </summary>
    internal sealed class WorldBossFighterState
    {
        /// <summary>
        /// 战斗单位唯一编号。
        /// Boss 使用运行时生成的编号，玩家直接使用玩家编号。
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 战斗单位展示名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 下次允许自动出手的时间，使用 UTC 记录。
        /// Boss 的节奏和玩家自动战斗都会依赖它。
        /// </summary>
        public DateTime NextActionAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 是否属于玩家阵营。
        /// </summary>
        public bool IsPlayerSide { get; set; }

        /// <summary>
        /// 战斗单位类型。
        /// 用于战斗引擎识别玩家、怪物或其他扩展单位。
        /// </summary>
        public FighterType FighterType { get; set; } = FighterType.Player;

        /// <summary>
        /// 绑定的怪物模板编号。
        /// 只有 Boss 或怪物镜像会用到该字段。
        /// </summary>
        public string MonsterTemplateId { get; set; } = string.Empty;

        /// <summary>
        /// 当前气血值。
        /// </summary>
        public int CurrentHp { get; set; }

        /// <summary>
        /// 最大气血值。
        /// </summary>
        public int MaxHp { get; set; }

        /// <summary>
        /// 当前灵力值。
        /// </summary>
        public int CurrentMp { get; set; }

        /// <summary>
        /// 最大灵力值。
        /// </summary>
        public int MaxMp { get; set; }

        /// <summary>
        /// 物理攻击值。
        /// </summary>
        public int PhysicalAttack { get; set; }

        /// <summary>
        /// 法术攻击值。
        /// </summary>
        public int MagicAttack { get; set; }

        /// <summary>
        /// 物理防御值。
        /// </summary>
        public int PhysicalDefense { get; set; }

        /// <summary>
        /// 法术防御值。
        /// </summary>
        public int MagicDefense { get; set; }

        /// <summary>
        /// 速度值。
        /// </summary>
        public int Speed { get; set; }

        /// <summary>
        /// 命中率。
        /// </summary>
        public float HitRate { get; set; }

        /// <summary>
        /// 闪避率。
        /// </summary>
        public float DodgeRate { get; set; }

        /// <summary>
        /// 暴击率。
        /// </summary>
        public float CritRate { get; set; }

        /// <summary>
        /// 暴击伤害倍率。
        /// </summary>
        public float CritDamage { get; set; }

        /// <summary>
        /// 连击率。
        /// </summary>
        public float ComboRate { get; set; }

        /// <summary>
        /// 反击率。
        /// </summary>
        public float CounterRate { get; set; }

        /// <summary>
        /// 破甲率。
        /// </summary>
        public float ArmorBreak { get; set; }

        /// <summary>
        /// 额外伤害倍率。
        /// </summary>
        public float ExtraDamage { get; set; }

        /// <summary>
        /// 当前元素属性。
        /// </summary>
        public Element Element { get; set; } = Element.None;

        /// <summary>
        /// 已装备技能编号列表。
        /// </summary>
        public List<string> SkillIds { get; set; } = [];

        /// <summary>
        /// 被动 Buff 编号列表。
        /// 战斗开始时会按这个列表补挂被动效果。
        /// </summary>
        public List<string> PassiveIds { get; set; } = [];

        /// <summary>
        /// 技能冷却剩余回合表。
        /// 键为技能编号，值为剩余冷却。
        /// </summary>
        public Dictionary<int, int> SkillCooldowns { get; set; } = [];

        /// <summary>
        /// 当前正在生效的 Buff 状态列表。
        /// </summary>
        public List<WorldBossBuffState> ActiveBuffs { get; set; } = [];

        /// <summary>
        /// 是否为镜像单位。
        /// 镜像只用于战斗计算，不会单独在参战列表里出现。
        /// </summary>
        public bool IsMirror { get; set; }

        /// <summary>
        /// 镜像所属本体编号。
        /// 仅镜像单位会写入。
        /// </summary>
        public string? MirrorOwnerId { get; set; }

        /// <summary>
        /// 镜像剩余持续回合数。
        /// </summary>
        public int MirrorDuration { get; set; }

        /// <summary>
        /// 镜像攻击继承比例。
        /// </summary>
        public float MirrorAttackRatio { get; set; } = 0.4f;

        /// <summary>
        /// 镜像生命继承比例。
        /// </summary>
        public float MirrorHpRatio { get; set; } = 0.5f;

        /// <summary>
        /// 当前单位拥有的镜像分身列表。
        /// </summary>
        public List<WorldBossFighterState> Mirrors { get; set; } = [];
    }

    /// <summary>
    /// 世界 Boss 战斗 Buff 快照。
    /// </summary>
    internal sealed class WorldBossBuffState
    {
        /// <summary>
        /// Buff 模板编号。
        /// </summary>
        public string BuffId { get; set; } = string.Empty;

        /// <summary>
        /// Buff 剩余持续回合数。
        /// </summary>
        public int RemainingDuration { get; set; }

        /// <summary>
        /// 当前叠加层数。
        /// </summary>
        public int CurrentStack { get; set; }

        /// <summary>
        /// Buff 来源战斗单位编号。
        /// </summary>
        public string? SourceId { get; set; }

        /// <summary>
        /// 是否为刚刚施加、尚未结算持续回合的 Buff。
        /// </summary>
        public bool IsNewlyApplied { get; set; }

        /// <summary>
        /// 护盾剩余数值。
        /// 仅带护盾类效果的 Buff 会使用该字段。
        /// </summary>
        public double ShieldValue { get; set; }
    }
}
