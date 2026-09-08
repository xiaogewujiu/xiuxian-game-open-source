namespace XXX.Application.DTOs
{
    /// <summary>
    /// 世界 Boss 当前状态总览。
    /// </summary>
    public class WorldBossCurrentDto
    {
        /// <summary>
        /// 当前是否存在正在开放的世界 Boss。
        /// </summary>
        public bool HasActiveBoss { get; set; }

        /// <summary>
        /// 当前玩家是否存在未领取的世界 Boss 奖励。
        /// </summary>
        public bool HasPendingReward { get; set; }

        /// <summary>
        /// 待领奖励所属的实例编号。
        /// </summary>
        public string? PendingRewardInstanceId { get; set; }

        /// <summary>
        /// 当前世界 Boss 实例摘要。
        /// </summary>
        public WorldBossInstanceDto? Instance { get; set; }

        /// <summary>
        /// Boss 当前面板数据。
        /// </summary>
        public WorldBossBossDto? Boss { get; set; }

        /// <summary>
        /// 当前玩家在世界 Boss 中的个人状态。
        /// </summary>
        public WorldBossParticipantStatusDto? Self { get; set; }
    }

    /// <summary>
    /// 世界 Boss 实例信息。
    /// </summary>
    public class WorldBossInstanceDto
    {
        /// <summary>
        /// 实例编号。
        /// </summary>
        public string InstanceId { get; set; } = string.Empty;

        /// <summary>
        /// Boss 模板编号。
        /// </summary>
        public string BossId { get; set; } = string.Empty;

        /// <summary>
        /// Boss 名称。
        /// </summary>
        public string BossName { get; set; } = string.Empty;

        /// <summary>
        /// Boss 画像路径。
        /// </summary>
        public string? PortraitPath { get; set; }

        /// <summary>
        /// 当前实例状态中文文案。
        /// </summary>
        public string State { get; set; } = string.Empty;

        /// <summary>
        /// 实例生成时间（UTC）。
        /// </summary>
        public DateTime SpawnedAtUtc { get; set; }

        /// <summary>
        /// 实例结束时间（UTC）。
        /// </summary>
        public DateTime EndAtUtc { get; set; }

        /// <summary>
        /// 剩余开放秒数。
        /// </summary>
        public int RemainingSeconds { get; set; }

        /// <summary>
        /// 当前参战人数。
        /// </summary>
        public int ParticipantCount { get; set; }
    }

    /// <summary>
    /// Boss 面板数据。
    /// </summary>
    public class WorldBossBossDto
    {
        /// <summary>
        /// Boss 战斗单位编号。
        /// </summary>
        public string FighterId { get; set; } = string.Empty;

        /// <summary>
        /// Boss 名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Boss 画像路径。
        /// </summary>
        public string? PortraitPath { get; set; }

        /// <summary>
        /// 当前气血。
        /// </summary>
        public int CurrentHp { get; set; }

        /// <summary>
        /// 最大气血。
        /// </summary>
        public int MaxHp { get; set; }

        /// <summary>
        /// 当前灵力。
        /// </summary>
        public int CurrentMp { get; set; }

        /// <summary>
        /// 最大灵力。
        /// </summary>
        public int MaxMp { get; set; }

        /// <summary>
        /// Boss 战斗属性快照。
        /// </summary>
        public WorldBossCombatAttributesDto Attributes { get; set; } = new();

        /// <summary>
        /// Boss 技能列表。
        /// </summary>
        public List<WorldBossBattleSkillDto> Skills { get; set; } = [];
    }

    /// <summary>
    /// 当前玩家在世界 Boss 中的战斗状态。
    /// </summary>
    public class WorldBossParticipantStatusDto
    {
        /// <summary>
        /// 玩家编号。
        /// </summary>
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 玩家名称。
        /// </summary>
        public string PlayerName { get; set; } = string.Empty;

        /// <summary>
        /// 当前是否已加入战场。
        /// </summary>
        public bool IsJoined { get; set; }

        /// <summary>
        /// 当前是否开启自动战斗。
        /// </summary>
        public bool IsAuto { get; set; }

        /// <summary>
        /// 当前是否可执行手动出手。
        /// </summary>
        public bool CanAct { get; set; }

        /// <summary>
        /// 当前是否处于死亡状态。
        /// </summary>
        public bool IsDead { get; set; }

        /// <summary>
        /// 当前气血。
        /// </summary>
        public int CurrentHp { get; set; }

        /// <summary>
        /// 最大气血。
        /// </summary>
        public int MaxHp { get; set; }

        /// <summary>
        /// 当前灵力。
        /// </summary>
        public int CurrentMp { get; set; }

        /// <summary>
        /// 最大灵力。
        /// </summary>
        public int MaxMp { get; set; }

        /// <summary>
        /// 下次允许出手时间（UTC）。
        /// </summary>
        public DateTime ReadyAtUtc { get; set; }

        /// <summary>
        /// 当前手动窗口截止时间（UTC）。
        /// </summary>
        public DateTime DeadlineAtUtc { get; set; }

        /// <summary>
        /// 复活完成时间（UTC）。
        /// </summary>
        public DateTime? ReviveAtUtc { get; set; }

        /// <summary>
        /// 距离下次可行动还剩多少秒。
        /// </summary>
        public int SecondsToReady { get; set; }

        /// <summary>
        /// 距离手动窗口截止还剩多少秒。
        /// </summary>
        public int SecondsToDeadline { get; set; }

        /// <summary>
        /// 距离复活还剩多少秒。
        /// </summary>
        public int SecondsToRevive { get; set; }

        /// <summary>
        /// 当前累计总伤害。
        /// </summary>
        public long TotalDamage { get; set; }

        /// <summary>
        /// 当前实时排名。
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// 当前携带技能列表。
        /// </summary>
        public List<WorldBossBattleSkillDto> Skills { get; set; } = [];
    }

    /// <summary>
    /// 战斗属性快照。
    /// </summary>
    public class WorldBossCombatAttributesDto
    {
        /// <summary>物理攻击。</summary>
        public int PhysicalAttack { get; set; }
        /// <summary>法术攻击。</summary>
        public int MagicAttack { get; set; }
        /// <summary>物理防御。</summary>
        public int PhysicalDefense { get; set; }
        /// <summary>法术防御。</summary>
        public int MagicDefense { get; set; }
        /// <summary>速度。</summary>
        public int Speed { get; set; }
        /// <summary>命中率。</summary>
        public float HitRate { get; set; }
        /// <summary>闪避率。</summary>
        public float DodgeRate { get; set; }
        /// <summary>暴击率。</summary>
        public float CritRate { get; set; }
        /// <summary>暴击伤害倍率。</summary>
        public float CritDamage { get; set; }
        /// <summary>连击率。</summary>
        public float ComboRate { get; set; }
        /// <summary>反击率。</summary>
        public float CounterRate { get; set; }
        /// <summary>破甲率。</summary>
        public float ArmorBreak { get; set; }
        /// <summary>额外伤害倍率。</summary>
        public float ExtraDamage { get; set; }
        /// <summary>元素枚举值。</summary>
        public int Element { get; set; }
        /// <summary>元素中文文案。</summary>
        public string ElementText { get; set; } = string.Empty;
    }

    /// <summary>
    /// 世界 Boss 战斗技能展示数据。
    /// </summary>
    public class WorldBossBattleSkillDto
    {
        /// <summary>技能编号。</summary>
        public int SkillId { get; set; }
        /// <summary>技能名称。</summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>技能描述。</summary>
        public string Description { get; set; } = string.Empty;
        /// <summary>灵力消耗。</summary>
        public int ManaCost { get; set; }
        /// <summary>技能总冷却。</summary>
        public int Cooldown { get; set; }
        /// <summary>当前剩余冷却。</summary>
        public int CurrentCooldown { get; set; }
        /// <summary>前端展示图标。</summary>
        public string Icon { get; set; } = string.Empty;
        /// <summary>当前是否可释放。</summary>
        public bool CanUse { get; set; }
    }

    /// <summary>
    /// 世界 Boss 排行项。
    /// </summary>
    public class WorldBossRankingEntryDto
    {
        /// <summary>排名。</summary>
        public int Rank { get; set; }
        /// <summary>玩家编号。</summary>
        public string PlayerId { get; set; } = string.Empty;
        /// <summary>玩家名称。</summary>
        public string PlayerName { get; set; } = string.Empty;
        /// <summary>累计总伤害。</summary>
        public long TotalDamage { get; set; }
        /// <summary>当前项是否为自己。</summary>
        public bool IsSelf { get; set; }
    }

    /// <summary>
    /// 世界 Boss 战斗日志。
    /// </summary>
    public class WorldBossLogDto
    {
        /// <summary>日志序号。</summary>
        public long Seq { get; set; }
        /// <summary>日志时间（UTC）。</summary>
        public DateTime TimestampUtc { get; set; }
        /// <summary>动作类型标识。</summary>
        public string ActionType { get; set; } = string.Empty;
        /// <summary>日志正文。</summary>
        public string Content { get; set; } = string.Empty;
    }

    /// <summary>
    /// 手动出手请求。
    /// </summary>
    public class WorldBossActionRequestDto
    {
        /// <summary>
        /// 动作类型。
        /// 当前支持 <c>normal</c> 与 <c>skill</c>。
        /// </summary>
        public string ActionType { get; set; } = "normal";

        /// <summary>
        /// 指定释放的技能编号。
        /// 仅动作类型为 <c>skill</c> 时使用。
        /// </summary>
        public int? SkillId { get; set; }
    }

    /// <summary>
    /// 自动开关请求。
    /// </summary>
    public class WorldBossAutoToggleRequestDto
    {
        /// <summary>
        /// 是否开启自动战斗。
        /// </summary>
        public bool Enabled { get; set; }
    }

    /// <summary>
    /// 世界 Boss 手动出手结果。
    /// </summary>
    public class WorldBossActionResultDto
    {
        /// <summary>本次出手是否成功。</summary>
        public bool Success { get; set; }
        /// <summary>结果提示文案。</summary>
        public string Message { get; set; } = string.Empty;
        /// <summary>更新后的玩家自身状态。</summary>
        public WorldBossParticipantStatusDto? Self { get; set; }
        /// <summary>更新后的 Boss 状态。</summary>
        public WorldBossBossDto? Boss { get; set; }
        /// <summary>本次动作附带的新日志片段。</summary>
        public List<WorldBossLogDto> Logs { get; set; } = [];
    }

    /// <summary>
    /// 世界 Boss 领奖结果。
    /// </summary>
    public class WorldBossRewardClaimDto
    {
        /// <summary>领奖是否成功。</summary>
        public bool Success { get; set; }
        /// <summary>领奖提示文案。</summary>
        public string Message { get; set; } = string.Empty;
        /// <summary>奖励所属实例编号。</summary>
        public string InstanceId { get; set; } = string.Empty;
        /// <summary>结算排名。</summary>
        public int Rank { get; set; }
        /// <summary>结算伤害。</summary>
        public long Damage { get; set; }
        /// <summary>奖励经验。</summary>
        public long RewardExp { get; set; }
        /// <summary>奖励金币。</summary>
        public long RewardGold { get; set; }
        /// <summary>奖励灵石。</summary>
        public long RewardSpiritStone { get; set; }
    }
}
