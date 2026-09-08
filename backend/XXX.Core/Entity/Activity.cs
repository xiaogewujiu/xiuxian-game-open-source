namespace XXX.Entity
{
    /// <summary>
    /// 活动类型枚举
    /// 定义游戏中的不同活动类型
    /// </summary>
    public enum ActivityType
    {
        /// <summary>
        /// 战斗类活动 - 统计战斗相关数据（伤害、击杀等）
        /// </summary>
        Battle,

        /// <summary>
        /// 收集类活动 - 收集并提交指定道具
        /// </summary>
        Collection,

        /// <summary>
        /// 挑战类活动 - 完成特定挑战（通关副本、击败BOSS等）
        /// </summary>
        Challenge,

        /// <summary>
        /// 签到类活动 - 累计登录天数
        /// </summary>
        CheckIn,

        /// <summary>
        /// 成就类活动 - 完成特定游戏目标
        /// </summary>
        Achievement
    }

    /// <summary>
    /// 战斗任务类型枚举
    /// 定义战斗类活动的具体统计类型
    /// </summary>
    public enum BattleTaskType
    {
        /// <summary>
        /// 累计造成伤害 - 统计活动期间所有战斗造成的总伤害
        /// </summary>
        TotalDamageDealt,

        /// <summary>
        /// 累计承受伤害 - 统计活动期间所有战斗承受的总伤害
        /// </summary>
        TotalDamageTaken,

        /// <summary>
        /// 累计击杀数 - 统计活动期间击败的野怪总数
        /// </summary>
        TotalKills,

        /// <summary>
        /// 累计胜利场次 - 统计活动期间获胜的战斗场次
        /// </summary>
        TotalWins,

        /// <summary>
        /// 单次战斗造成伤害 - 统计单次战斗的最高伤害
        /// </summary>
        SingleBattleDamage,

        /// <summary>
        /// 击败BOSS次数 - 统计活动期间击败的BOSS数量
        /// </summary>
        BossKills,

        /// <summary>
        /// 使用技能次数 - 统计活动期间使用技能的总次数
        /// </summary>
        UseSkillCount,

        /// <summary>
        /// 暴击次数 - 统计活动期间触发暴击的总次数
        /// </summary>
        CriticalHits,

        /// <summary>
        /// 完美胜利 - 无伤获胜的场次
        /// </summary>
        PerfectWin
    }

    /// <summary>
    /// 奖励状态枚举
    /// 定义阶段奖励的不同状态
    /// </summary>
    public enum RewardStatus
    {
        /// <summary>
        /// 未解锁 - 未达到领取条件
        /// </summary>
        Locked,

        /// <summary>
        /// 可领取 - 已达到条件，可以领取奖励
        /// </summary>
        Available,

        /// <summary>
        /// 已领取 - 奖励已领取
        /// </summary>
        Claimed
    }

    /// <summary>
    /// 奖励类型枚举
    /// 定义游戏中可发放的奖励类型
    /// </summary>
    public enum RewardType
    {
        /// <summary>
        /// 金币 - 游戏货币
        /// </summary>
        Gold,

        /// <summary>
        /// 经验 - 角色经验值
        /// </summary>
        Exp,

        /// <summary>
        /// 道具 - 消耗品、材料等
        /// </summary>
        Item,

        /// <summary>
        /// 装备 - 武器、防具等
        /// </summary>
        Equipment,

        /// <summary>
        /// 称号 - 角色称号
        /// </summary>
        Title,

        /// <summary>
        /// Buff - 临时增益效果
        /// </summary>
        Buff
    }

    /// <summary>
    /// 活动配置类
    /// 定义一个活动的完整配置信息
    /// </summary>
    public class ActivityConfig
    {
        /// <summary>
        /// 活动唯一标识符
        /// 用于系统内部识别活动
        /// </summary>
        public string ActivityId { get; set; } = string.Empty;

        /// <summary>
        /// 活动名称
        /// 显示给玩家看的活动标题
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 活动描述
        /// 详细说明活动内容和规则
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 活动类型
        /// 决定活动的玩法和统计方式
        /// </summary>
        public ActivityType Type { get; set; }

        /// <summary>
        /// 活动开始时间
        /// 活动从何时开始生效
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 活动结束时间
        /// 活动何时失效，玩家无法再参与
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 战斗任务类型
        /// 仅当Type为Battle时有效，指定统计哪种战斗数据
        /// </summary>
        public BattleTaskType? BattleTask { get; set; }

        /// <summary>
        /// 目标道具ID
        /// 仅当Type为Collection时有效，指定需要收集的道具
        /// </summary>
        public string TargetItemId { get; set; } = string.Empty;

        /// <summary>
        /// 阶段配置列表
        /// 定义活动的各个阶段和对应奖励
        /// </summary>
        public List<ActivityStage> Stages { get; set; } = [];

        /// <summary>
        /// 活动图标路径
        /// UI显示使用的图标资源路径
        /// </summary>
        public string Icon { get; set; } = string.Empty;

        /// <summary>
        /// 活动排序值
        /// 用于控制活动列表中的显示顺序（数字越小越靠前）
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 判断活动是否处于活跃状态
        /// 根据当前时间与活动起止时间判断
        /// </summary>
        public bool IsActive => DateTime.Now >= StartTime && DateTime.Now <= EndTime;

        /// <summary>
        /// 获取活动剩余时间
        /// 返回距离活动结束还有多长时间
        /// </summary>
        public TimeSpan GetRemainingTime()
        {
            var remaining = EndTime - DateTime.Now;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        /// <summary>
        /// 获取活动已过时间
        /// 返回活动开始到现在经过了多长时间
        /// </summary>
        public TimeSpan GetElapsedTime()
        {
            var elapsed = DateTime.Now - StartTime;
            return elapsed > TimeSpan.Zero ? elapsed : TimeSpan.Zero;
        }
    }

    /// <summary>
    /// 活动阶段类
    /// 定义活动的一个阶段，包含目标和奖励
    /// </summary>
    public class ActivityStage
    {
        /// <summary>
        /// 阶段编号
        /// 从1开始的整数，表示第几阶段
        /// </summary>
        public int Stage { get; set; }

        /// <summary>
        /// 阶段目标值
        /// 完成该阶段需要达到的数值（如造成10000伤害）
        /// </summary>
        public long TargetValue { get; set; }

        /// <summary>
        /// 阶段描述
        /// 显示给玩家看的阶段说明（如"累计造成10000点伤害"）
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 奖励列表
        /// 完成该阶段后可获得的奖励
        /// </summary>
        public List<ActivityReward> Rewards { get; set; } = [];
    }

    /// <summary>
    /// 活动奖励类
    /// 定义一种奖励的详细信息
    /// </summary>
    public class ActivityReward
    {
        /// <summary>
        /// 奖励类型
        /// 决定奖励属于哪一类（金币、道具、装备等）
        /// </summary>
        public RewardType Type { get; set; }

        /// <summary>
        /// 奖励ID
        /// 当Type为Item或Equipment时，指定具体的道具或装备ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 奖励数量
        /// 奖励的数量（如500金币、3个道具）
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 获取奖励名称
        /// 返回用于显示的奖励名称字符串
        /// </summary>
        public string Name => GetRewardName();

        /// <summary>
        /// 生成奖励显示名称
        /// 根据奖励类型和ID生成人类可读的名称
        /// </summary>
        private string GetRewardName()
        {
            switch (Type)
            {
                case RewardType.Gold:
                    return $"{Count} 金币";

                case RewardType.Exp:
                    return $"{Count} 经验";

                case RewardType.Item:
                    // 从GameData获取道具名称
                    if (GameData.Items.ContainsKey(Id))
                    {
                        return $"{Count} {GameData.Items[Id].Name}";
                    }
                    return $"{Count} 未知道具";

                case RewardType.Equipment:
                    // 从GameData获取装备名称
                    if (GameData.EquipmentTemplates.ContainsKey(int.Parse(Id)))
                    {
                        return $"装备：{GameData.EquipmentTemplates[int.Parse(Id)].Name}";
                    }
                    return "装备";

                case RewardType.Title:
                    return "称号";

                case RewardType.Buff:
                    return "增益效果";

                default:
                    return "未知奖励";
            }
        }

        /// <summary>
        /// 获取奖励图标路径
        /// 返回用于UI显示的奖励图标路径
        /// </summary>
        public string GetIconPath()
        {
            switch (Type)
            {
                case RewardType.Gold:
                    return "Icons/gold.png";
                case RewardType.Exp:
                    return "Icons/exp.png";
                case RewardType.Item:
                    return GameData.Items.ContainsKey(Id)
                        ? $"Icons/Items/{Id}.png"
                        : "Icons/unknown.png";
                case RewardType.Equipment:
                    return $"Icons/Equipments/{Id}.png";
                default:
                    return "Icons/default.png";
            }
        }
    }

    /// <summary>
    /// 玩家活动进度类
    /// 记录玩家在某个活动中的进度数据
    /// </summary>
    public class PlayerActivityProgress
    {
        /// <summary>
        /// 活动ID
        /// 关联到ActivityConfig
        /// </summary>
        public string ActivityId { get; set; } = string.Empty;

        /// <summary>
        /// 当前进度值
        /// 玩家在活动中已达到的数值（如已造成5000伤害）
        /// </summary>
        public long CurrentProgress { get; set; }

        /// <summary>
        /// 已领取奖励的阶段集合
        /// 记录玩家已领取了哪些阶段的奖励
        /// </summary>
        public HashSet<int> ClaimedStages { get; set; } = [];

        /// <summary>
        /// 已提交的道具数量
        /// 仅用于收集类活动，记录每种道具已提交的数量
        /// Key: 道具ID, Value: 已提交数量
        /// </summary>
        public Dictionary<string, int> SubmittedItems { get; set; } = [];

        /// <summary>
        /// 活动开始时间
        /// 玩家首次参与该活动的时间
        /// </summary>
        public DateTime StartTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 是否已完成
        /// 标记玩家是否已完成活动的所有阶段
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// 最后更新时间
        /// 记录进度最后更新时间，用于排序和显示
        /// </summary>
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 活动进度显示类
    /// 用于UI显示的完整活动进度信息
    /// </summary>
    public class ActivityProgressDisplay
    {
        /// <summary>
        /// 活动配置
        /// 活动的基本信息
        /// </summary>
        public ActivityConfig Activity { get; set; } = new ActivityConfig();

        /// <summary>
        /// 当前进度值
        /// 玩家当前已达到的数值
        /// </summary>
        public long CurrentProgress { get; set; }

        /// <summary>
        /// 目标进度值
        /// 下一阶段或最终阶段的目标值
        /// </summary>
        public long TargetProgress { get; set; }

        /// <summary>
        /// 当前阶段
        /// 玩家已完成的最高阶段
        /// </summary>
        public int CurrentStage { get; set; }

        /// <summary>
        /// 总阶段数
        /// 该活动共有多少个阶段
        /// </summary>
        public int TotalStages { get; set; }

        /// <summary>
        /// 完成百分比
        /// 当前进度占总进度的百分比（0-100）
        /// </summary>
        public float Percent { get; set; }

        /// <summary>
        /// 可领取奖励的阶段列表
        /// 达到条件但尚未领取奖励的阶段
        /// </summary>
        public List<int> AvailableRewardStages { get; set; } = [];

        /// <summary>
        /// 已领取奖励的阶段集合
        /// 已经领取过奖励的阶段
        /// </summary>
        public HashSet<int> ClaimedStages { get; set; } = [];

        /// <summary>
        /// 剩余时间
        /// 距离活动结束还有多长时间
        /// </summary>
        public TimeSpan RemainingTime { get; set; }

        /// <summary>
        /// 获取进度条显示文本
        /// 返回格式化的进度字符串，如"5000/10000"
        /// </summary>
        public string GetProgressText()
        {
            return $"{CurrentProgress}/{TargetProgress}";
        }

        /// <summary>
        /// 获取百分比显示文本
        /// 返回格式化的百分比字符串，如"50.0%"
        /// </summary>
        public string GetPercentText()
        {
            return $"{Percent:F1}%";
        }

        /// <summary>
        /// 获取剩余时间显示文本
        /// 返回格式化的时间字符串
        /// </summary>
        public string GetRemainingTimeText()
        {
            if (RemainingTime <= TimeSpan.Zero)
            {
                return "已结束";
            }

            if (RemainingTime.TotalDays >= 1)
            {
                return $"{(int)RemainingTime.TotalDays}天";
            }
            else if (RemainingTime.TotalHours >= 1)
            {
                return $"{(int)RemainingTime.TotalHours}小时";
            }
            else
            {
                return $"{RemainingTime.Minutes}分钟";
            }
        }
    }
}
