namespace XXX.Entity
{
    /// <summary>
    /// 秘境实例状态。
    /// </summary>
    public enum DungeonInstanceStatus
    {
        /// <summary>
        /// 探索中。
        /// </summary>
        Running = 0,

        /// <summary>
        /// 已结算。
        /// </summary>
        Settled = 1
    }

    /// <summary>
    /// 秘境结算原因。
    /// </summary>
    public enum DungeonSettleReason
    {
        /// <summary>
        /// 主动退出。
        /// </summary>
        ActiveQuit = 0,

        /// <summary>
        /// 死亡。
        /// </summary>
        Death = 1,

        /// <summary>
        /// 秘境关闭强制结算。
        /// </summary>
        ForceClose = 2
    }

    /// <summary>
    /// 秘境事件大类。
    /// </summary>
    public enum DungeonEventType
    {
        /// <summary>
        /// 战斗类。
        /// </summary>
        Battle = 1,

        /// <summary>
        /// 回复类。
        /// </summary>
        Heal = 2,

        /// <summary>
        /// 增益类。
        /// </summary>
        Buff = 3,

        /// <summary>
        /// 减益/陷阱类。
        /// </summary>
        Debuff = 4,

        /// <summary>
        /// 资源获取类。
        /// </summary>
        Resource = 5,

        /// <summary>
        /// 宝箱类。
        /// </summary>
        Treasure = 6,

        /// <summary>
        /// 奇遇类。
        /// </summary>
        Adventure = 7,

        /// <summary>
        /// 商店/交易类。
        /// </summary>
        Shop = 8,

        /// <summary>
        /// 特殊事件类。
        /// </summary>
        Event = 9,

        /// <summary>
        /// 空事件。
        /// </summary>
        Empty = 10,

        /// <summary>
        /// 玩家偶遇。
        /// </summary>
        PlayerEncounter = 11
    }
}
