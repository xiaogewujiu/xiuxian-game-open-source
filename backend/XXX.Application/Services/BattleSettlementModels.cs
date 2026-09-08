using XXX.Entity;

namespace XXX.Application.Services
{
    /// <summary>
    /// 组队副本当前场次的成员奖励资格快照。
    /// </summary>
    public sealed class PartyRewardEligibility
    {
        /// <summary>玩家编号。</summary>
        public string PlayerId { get; set; } = string.Empty;
        /// <summary>是否有本场奖励资格。</summary>
        public bool RewardEligible { get; set; }
        /// <summary>挑战前次数。</summary>
        public int AttemptsBefore { get; set; }
        /// <summary>本场是否已消耗次数。</summary>
        public bool AttemptConsumed { get; set; }
        /// <summary>关联队伍编号。</summary>
        public string PartyId { get; set; } = string.Empty;
        /// <summary>关联副本编号。</summary>
        public string DungeonId { get; set; } = string.Empty;
        /// <summary>关联战斗编号。</summary>
        public string BattleId { get; set; } = string.Empty;
    }

    /// <summary>
    /// 战斗核心结算模型（内部使用，不参与接口返回 DTO）。
    /// </summary>
    /// <remarks>
    /// 中文注释：
    /// 该模型在核心战斗计算完成后一次性聚合，供战斗结算事务消费。
    /// 经验、金币、掉落、统计和冷却字段全部来自服务端战斗计算（BattleSystem 结果），
    /// 绝不从前端输入信任任何奖励数值。
    /// </remarks>
    public sealed class BattleSettlementModel
    {
        /// <summary>
        /// 玩家编号。
        /// </summary>
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 本次战斗获得的经验（已含聚灵阵等加成）。
        /// </summary>
        public long ExpGained { get; set; }

        /// <summary>
        /// 本次战斗获得的金币。
        /// </summary>
        public long GoldGained { get; set; }

        /// <summary>
        /// 是否胜利。
        /// </summary>
        public bool IsVictory { get; set; }

        /// <summary>
        /// 本场击杀数。
        /// </summary>
        public int KillCount { get; set; }

        /// <summary>
        /// 道具掉落（按 ItemId 聚合后的数量）。
        /// </summary>
        public List<ItemDropGroup> ItemDrops { get; set; } = [];

        /// <summary>
        /// 装备掉落实例。
        /// </summary>
        public List<EquipmentInstance> EquipmentDrops { get; set; } = [];

        /// <summary>
        /// 图鉴掉落。
        /// </summary>
        public List<CollectionDropGroup> CollectionDrops { get; set; } = [];

        /// <summary>
        /// 总回合数。
        /// </summary>
        public int TotalRounds { get; set; }

        /// <summary>
        /// 冷却基准时间（UTC）。为空时由结算事务自行取当前时间。
        /// </summary>
        public DateTime? CooldownBaseUtc { get; set; }
    }

    /// <summary>
    /// 道具掉落分组（按 ItemId 合并数量）。
    /// </summary>
    public sealed class ItemDropGroup
    {
        /// <summary>
        /// 道具模板编号。
        /// </summary>
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// 掉落数量。
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 是否绑定（战斗掉落当前统一为非绑定）。
        /// </summary>
        public bool IsLocked { get; set; }
    }

    /// <summary>
    /// 图鉴掉落分组。
    /// </summary>
    public sealed class CollectionDropGroup
    {
        /// <summary>
        /// 图鉴系列编号。
        /// </summary>
        public string SeriesId { get; set; } = string.Empty;

        /// <summary>
        /// 图鉴类型。
        /// </summary>
        public int CollectionType { get; set; }
    }
}
