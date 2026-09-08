using XXX.Entity;

namespace XXX.Battle
{
    /// <summary>
    /// 活动Buff
    /// </summary>
    public class ActiveBuff
    {
        /// <summary>
        /// Buff模板
        /// </summary>
        public BuffTemplate Template { get; set; } = new BuffTemplate();

        /// <summary>
        /// 剩余持续回合数
        /// </summary>
        public int RemainingDuration { get; set; }

        /// <summary>
        /// 当前层数
        /// </summary>
        public int CurrentStack { get; set; }

        /// <summary>
        /// Buff来源
        /// </summary>
        public BattleFighter? Source { get; set; }

        /// <summary>
        /// 是否为新施加的Buff
        /// </summary>
        public bool IsNewlyApplied { get; set; }

        /// <summary>
        /// 护盾值
        /// </summary>
        public double ShieldValue { get; set; }
    }
}
