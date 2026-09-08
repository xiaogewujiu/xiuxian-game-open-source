namespace XXX.Entity
{
    /// <summary>
    /// 任务状态
    /// </summary>
    public enum QuestStatus
    {
        /// <summary>
        /// 未开始
        /// </summary>
        NotStarted = 0,

        /// <summary>
        /// 进行中
        /// </summary>
        InProgress = 1,

        /// <summary>
        /// 已完成
        /// </summary>
        Completed = 2,

        /// <summary>
        /// 已提交
        /// </summary>
        Submitted = 3
    }

    /// <summary>
    /// 灵宠类型
    /// </summary>
    public enum PetType
    {
        /// <summary>
        /// 攻击型
        /// </summary>
        Attack = 1,

        /// <summary>
        /// 防御型
        /// </summary>
        Defense = 2,

        /// <summary>
        /// 辅助型
        /// </summary>
        Support = 3,

        /// <summary>
        /// 平衡型
        /// </summary>
        Balanced = 4
    }

    /// <summary>
    /// 作物类型
    /// </summary>
    public enum CropType
    {
        /// <summary>
        /// 普通作物
        /// </summary>
        Normal = 1,

        /// <summary>
        /// 灵草
        /// </summary>
        SpiritGrass = 2,

        /// <summary>
        /// 珍稀作物
        /// </summary>
        Rare = 3,

        /// <summary>
        /// 传说作物
        /// </summary>
        Legendary = 4
    }

    /// <summary>
    /// 地块状态
    /// </summary>
    public enum PlotStatus
    {
        /// <summary>
        /// 空闲
        /// </summary>
        Empty = 0,

        /// <summary>
        /// 种植中
        /// </summary>
        Planting = 1,

        /// <summary>
        /// 生长中
        /// </summary>
        Growing = 2,

        /// <summary>
        /// 可收获
        /// </summary>
        Harvestable = 3,

        /// <summary>
        /// 已收获
        /// </summary>
        Harvested = 4
    }

    /// <summary>
    /// 丹药品阶
    /// </summary>
    public enum PillRank
    {
        /// <summary>
        /// 一品
        /// </summary>
        Rank1 = 1,

        /// <summary>
        /// 二品
        /// </summary>
        Rank2 = 2,

        /// <summary>
        /// 三品
        /// </summary>
        Rank3 = 3,

        /// <summary>
        /// 四品
        /// </summary>
        Rank4 = 4,

        /// <summary>
        /// 五品
        /// </summary>
        Rank5 = 5,

        /// <summary>
        /// 六品
        /// </summary>
        Rank6 = 6,

        /// <summary>
        /// 七品
        /// </summary>
        Rank7 = 7,

        /// <summary>
        /// 八品
        /// </summary>
        Rank8 = 8,

        /// <summary>
        /// 九品
        /// </summary>
        Rank9 = 9
    }

    /// <summary>
    /// 丹药类型
    /// </summary>
    public enum PillType
    {
        /// <summary>
        /// 恢复类
        /// </summary>
        Recovery = 1,

        /// <summary>
        /// 增益类
        /// </summary>
        Buff = 2,

        /// <summary>
        /// 突破类
        /// </summary>
        Breakthrough = 3,

        /// <summary>
        /// 特殊类
        /// </summary>
        Special = 4
    }

    /// <summary>
    /// 丹药效果类型
    /// </summary>
    public enum PillEffectType
    {
        /// <summary>
        /// 恢复生命值
        /// </summary>
        RestoreHP = 1,

        /// <summary>
        /// 恢复法力值
        /// </summary>
        RestoreMP = 2,

        /// <summary>
        /// 增加攻击力
        /// </summary>
        IncreaseAttack = 3,

        /// <summary>
        /// 增加防御力
        /// </summary>
        IncreaseDefense = 4,

        /// <summary>
        /// 增加经验
        /// </summary>
        IncreaseExp = 5,

        /// <summary>
        /// 清除负面状态
        /// </summary>
        ClearDebuff = 6,

        /// <summary>
        /// 复活
        /// </summary>
        Resurrect = 7
    }

    /// <summary>
    /// 账号战斗模式。
    /// </summary>
    public enum BattleMode
    {
        /// <summary>
        /// 普通状态。
        /// </summary>
        Normal = 0,

        /// <summary>
        /// 单人普通地图离线挂机。
        /// </summary>
        OfflineAuto = 1
    }
}
