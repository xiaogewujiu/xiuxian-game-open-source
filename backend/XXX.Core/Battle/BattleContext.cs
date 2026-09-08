using XXX.Entity;

namespace XXX.Battle
{
    /// <summary>
    /// 战斗上下文
    /// </summary>
    public class BattleContext
    {
        /// <summary>
        /// 地图信息
        /// </summary>
        public Map Map { get; set; } = new Map();

        /// <summary>
        /// 我方战斗单位列表
        /// </summary>
        public List<BattleFighter> PlayerSide { get; set; } = [];

        /// <summary>
        /// 敌方战斗单位列表
        /// </summary>
        public List<BattleFighter> EnemySide { get; set; } = [];

        /// <summary>
        /// 战斗结果
        /// </summary>
        public BattleResult Result { get; set; } = new BattleResult();

        /// <summary>
        /// 是否启用属性克制（默认启用）
        /// </summary>
        public bool EnableElementAdvantage { get; set; } = true;

        /// <summary>
        /// 角色状态缓存（Key: FighterId, Value: 上次记录的状态快照）
        /// </summary>
        public Dictionary<string, FighterStateSnapshot> FighterStateCache { get; set; } = [];

        /// <summary>
        /// 当前战斗上下文的日志时间戳计数器
        /// 作用：为同一场战斗内的日志提供稳定、单调递增的时间序
        /// 关键逻辑：计数器绑定到 BattleContext，避免全局静态计数导致并发战斗互相污染
        /// </summary>
        public int LogTimestampCounter { get; set; } = 0;
    }
}
