namespace XXX.Entity
{
    /// <summary>
    /// 结构化战斗日志条目
    /// </summary>
    public class BattleLogEntry
    {
        /// <summary>
        /// 日志类型
        /// </summary>
        public BattleLogType Type { get; set; }

        /// <summary>
        /// 时间戳（毫秒，用于排序）
        /// </summary>
        public int Timestamp { get; set; }

        /// <summary>
        /// 施法者ID（可为空）
        /// </summary>
        public string CasterId { get; set; } = string.Empty;

        /// <summary>
        /// 施法者名称
        /// </summary>
        public string CasterName { get; set; } = string.Empty;

        /// <summary>
        /// 目标ID（可为空）
        /// </summary>
        public string TargetId { get; set; } = string.Empty;

        /// <summary>
        /// 目标名称
        /// </summary>
        public string TargetName { get; set; } = string.Empty;

        /// <summary>
        /// 技能ID（如果是技能相关）
        /// </summary>
        public int? SkillId { get; set; }

        /// <summary>
        /// 技能名称
        /// </summary>
        public string SkillName { get; set; } = string.Empty;

        /// <summary>
        /// Buff ID（如果是Buff相关）
        /// </summary>
        public string BuffId { get; set; } = string.Empty;

        /// <summary>
        /// Buff名称
        /// </summary>
        public string BuffName { get; set; } = string.Empty;

        /// <summary>
        /// 数值（伤害、治疗量等）
        /// </summary>
        public int? Value { get; set; }

        /// <summary>
        /// 数值2（如暴击伤害中的基础伤害）
        /// </summary>
        public int? Value2 { get; set; }

        /// <summary>
        /// 当前生命值（目标）
        /// </summary>
        public int? TargetCurrentHp { get; set; }

        /// <summary>
        /// 最大生命值（目标）
        /// </summary>
        public int? TargetMaxHp { get; set; }

        /// <summary>
        /// 人类可读的描述文本（用于降级显示）
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 额外数据（JSON格式，用于特殊需求）
        /// </summary>
        public string ExtraData { get; set; } = string.Empty;

        /// <summary>
        /// 是否暴击
        /// </summary>
        public bool IsCrit { get; set; }

        /// <summary>
        /// 是否闪避
        /// </summary>
        public bool IsDodge { get; set; }

        /// <summary>
        /// 伤害段数（多段伤害）
        /// </summary>
        public int? HitIndex { get; set; }

        /// <summary>
        /// 总段数
        /// </summary>
        public int? TotalHits { get; set; }

        /// <summary>
        /// 该日志条目发生时所有角色的状态快照（key为FighterId）
        /// </summary>
        public Dictionary<string, FighterStateSnapshot> FighterStates { get; set; } = [];
    }
}
