namespace XXX.Player
{
    /// <summary>
    /// 货币类型枚举
    /// 定义游戏中所有货币类型
    /// </summary>
    public enum CurrencyType
    {
        /// <summary>
        /// 金币 - 基础货币
        /// 用途：购买道具、强化装备、商店交易等
        /// 获取方式：战斗奖励、任务奖励、出售道具、活动奖励
        /// </summary>
        Gold,

        /// <summary>
        /// 灵石 - 修仙体系货币
        /// 用途：五行升级、灵田、抽奖等
        /// 获取方式：任务奖励、成就奖励、活动奖励、签到
        /// </summary>
        SpiritStone,

        /// <summary>
        /// 荣誉点 - PVP货币
        /// 用途：兑换PVP专属装备和道具
        /// 获取方式：PVP战斗、竞技场
        /// </summary>
        Honor,

        /// <summary>
        /// 公会贡献 - 公会货币
        /// 用途：兑换公会专属道具
        /// 获取方式：公会活动、公会任务
        /// </summary>
        GuildContribution
    }

    /// <summary>
    /// 货币操作类型
    /// 用于记录货币的获取和消耗原因
    /// </summary>
    public enum CurrencyOperationType
    {
        /// <summary>
        /// 战斗奖励
        /// </summary>
        BattleReward,

        /// <summary>
        /// 任务奖励
        /// </summary>
        QuestReward,

        /// <summary>
        /// 活动奖励
        /// </summary>
        ActivityReward,

        /// <summary>
        /// 签到奖励
        /// </summary>
        CheckInReward,

        /// <summary>
        /// 出售道具
        /// </summary>
        SellItem,

        /// <summary>
        /// 购买道具
        /// </summary>
        BuyItem,

        /// <summary>
        /// 装备强化
        /// </summary>
        EnhanceEquipment,

        /// <summary>
        /// 装备洗练
        /// </summary>
        RerollEquipment,

        /// <summary>
        /// 系统赠送
        /// </summary>
        SystemGift,

        /// <summary>
        /// 充值
        /// </summary>
        Recharge,

        /// <summary>
        /// 其他
        /// </summary>
        Other
    }

    /// <summary>
    /// 货币变更记录
    /// 记录每一次货币的获取和消耗
    /// </summary>
    public class CurrencyChangeRecord
    {
        /// <summary>
        /// 记录唯一ID
        /// </summary>
        public string RecordId { get; set; } = string.Empty;

        /// <summary>
        /// 玩家ID
        /// </summary>
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 货币类型
        /// </summary>
        public CurrencyType CurrencyType { get; set; }

        /// <summary>
        /// 变更金额（正数表示获得，负数表示消耗）
        /// </summary>
        public long Amount { get; set; }

        /// <summary>
        /// 变更前金额
        /// </summary>
        public long BeforeAmount { get; set; }

        /// <summary>
        /// 变更后金额
        /// </summary>
        public long AfterAmount { get; set; }

        /// <summary>
        /// 操作类型
        /// </summary>
        public CurrencyOperationType OperationType { get; set; }

        /// <summary>
        /// 原因描述
        /// </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// 记录时间
        /// </summary>
        public DateTime RecordTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 获取货币类型名称
        /// </summary>
        /// <returns>货币名称</returns>
        public string GetCurrencyTypeName()
        {
            switch (CurrencyType)
            {
                case CurrencyType.Gold: return "金币";
                case CurrencyType.SpiritStone: return "灵石";
                case CurrencyType.Honor: return "荣誉点";
                case CurrencyType.GuildContribution: return "公会贡献";
                default: return "未知货币";
            }
        }

        /// <summary>
        /// 获取操作描述
        /// </summary>
        /// <returns>操作描述</returns>
        public string GetOperationText()
        {
            string operation = Amount >= 0 ? "获得" : "消耗";
            return $"{operation}{Math.Abs(Amount)}{GetCurrencyTypeName()} - {Reason}";
        }
    }
}
