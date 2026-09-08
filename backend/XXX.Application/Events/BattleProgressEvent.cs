namespace XXX.Application.Events
{
    /// <summary>
    /// 战斗任务与成就进度汇总事件。
    /// </summary>
    /// <remarks>
    /// 中文注释：
    /// 事件只保存服务端战斗计算产生的汇总数据，不包含任何前端输入奖励。
    /// 唯一事件编号用于日志关联和后续幂等消费扩展。
    /// </remarks>
    public sealed record BattleProgressEvent
    {
        /// <summary>事件唯一编号。</summary>
        public string EventId { get; init; } = Guid.NewGuid().ToString("N");

        /// <summary>玩家编号。</summary>
        public string PlayerId { get; init; } = string.Empty;

        /// <summary>普通地图编号。</summary>
        public string? MapId { get; init; }

        /// <summary>副本编号。</summary>
        public string? DungeonId { get; init; }

        /// <summary>是否胜利。</summary>
        public bool IsVictory { get; init; }

        /// <summary>总输出伤害。</summary>
        public int TotalDamageDealt { get; init; }

        /// <summary>总承受伤害。</summary>
        public int TotalDamageTaken { get; init; }

        /// <summary>击杀数。</summary>
        public int Kills { get; init; }

        /// <summary>暴击次数。</summary>
        public int CritCount { get; init; }

        /// <summary>闪避次数。</summary>
        public int DodgeCount { get; init; }

        /// <summary>技能使用次数。</summary>
        public int SkillUseCount { get; init; }

        /// <summary>消耗法力值。</summary>
        public int TotalMpConsumed { get; init; }

        /// <summary>击杀的怪物模板编号。</summary>
        public List<string> KilledMonsterTemplateIds { get; init; } = [];

        /// <summary>按技能编号汇总的使用次数。</summary>
        public Dictionary<int, int> SkillUsageBySkill { get; init; } = [];

        /// <summary>按技能编号汇总的输出伤害。</summary>
        public Dictionary<int, int> SkillDamageBySkill { get; init; } = [];

        /// <summary>事件发生时间（UTC）。</summary>
        public DateTime OccurredAtUtc { get; init; } = DateTime.UtcNow;
    }
}
