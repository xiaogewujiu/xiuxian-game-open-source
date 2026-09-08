using XXX.Entity;
using XXX.Inventory;

namespace XXX.Activity
{
    /// <summary>
    /// 签到系统管理器
    /// 负责处理每日签到、连续签到、补签等功能
    ///
    /// 功能说明：
    /// 1. 每日签到：玩家每天可以签到一次
    /// 2. 连续签到奖励：连续签到天数越多，奖励越丰厚
    /// 3. 补签功能：错过签到可以使用补签卡补签
    /// 4. 月度重置：每月1号重置签到数据
    /// 5. 特殊日期：特定日期有特殊奖励
    /// </summary>
    public class CheckInSystem
    {
        /// <summary>
        /// 签到配置字典
        /// 存储所有签到相关的配置信息
        /// Key: 配置名称, Value: 配置值
        /// </summary>
        private static Dictionary<string, CheckInConfig> checkInConfigs = [];

        /// <summary>
        /// 玩家签到数据字典
        /// 存储所有玩家的签到状态和进度
        /// Key: 玩家ID, Value: 签到数据
        /// </summary>
        private static Dictionary<string, PlayerCheckInData> playerCheckInData = [];

        /// <summary>
        /// 随机数生成器
        /// 用于随机生成签到奖励等
        /// </summary>
        private static Random random = new Random();

        #region 初始化

        /// <summary>
        /// 初始化签到系统
        /// 注册默认的签到配置
        /// </summary>
        public static void Initialize()
        {
            // 注册每日签到配置
            RegisterDailyCheckInConfig();

            // 可以继续注册其他类型的签到配置
            // RegisterWeeklyCheckInConfig();
            // RegisterMonthlyCheckInConfig();
        }

        /// <summary>
        /// 注册每日签到配置
        /// 定义30天的签到奖励（一个月一个周期）
        /// </summary>
        private static void RegisterDailyCheckInConfig()
        {
            var config = new CheckInConfig
            {
                ConfigId = "daily_checkin",
                ConfigName = "每日签到",
                Description = "连续签到获得丰厚奖励！连续天数越多，奖励越丰厚！",
                CycleType = CheckInCycleType.Monthly, // 每月一个周期
                MaxContinuousDays = 30, // 最多记录30天连续签到
                CanRetroactive = true, // 允许补签
                MaxRetroactiveDays = 3, // 最多补签3天
                RetroactiveCost = 1, // 每次补签消耗1张补签卡
                Stages = []
            };

            // 配置30天的签到奖励
            for (int day = 1; day <= 30; day++)
            {
                var stage = new CheckInStage
                {
                    Day = day,
                    Description = $"第 {day} 天签到",
                    Rewards = GenerateDayRewards(day)
                };

                // 每7天设置一个里程碑，有额外奖励
                if (day % 7 == 0)
                {
                    stage.IsMilestone = true;
                    stage.Description += " - 里程碑奖励！";
                    stage.ExtraRewards = GenerateMilestoneRewards(day / 7);
                }

                config.Stages.Add(stage);
            }

            checkInConfigs[config.ConfigId] = config;
        }

        /// <summary>
        /// 生成每日签到奖励
        /// 根据签到天数生成对应奖励
        /// </summary>
        /// <param name="day">签到天数</param>
        /// <returns>奖励列表</returns>
        private static List<ActivityReward> GenerateDayRewards(int day)
        {
            var rewards = new List<ActivityReward>();

            // 基础金币奖励（随天数递增）
            int baseGold = 100 + (day * 10);
            rewards.Add(new ActivityReward { Type = RewardType.Gold, Count = baseGold });

            // 基础经验奖励（随天数递增）
            int baseExp = 200 + (day * 20);
            rewards.Add(new ActivityReward { Type = RewardType.Exp, Count = baseExp });

            // 每5天给予一个道具奖励
            if (day % 5 == 0)
            {
                string itemId = GetRandomItemId(day);
                int itemCount = day / 5;
                rewards.Add(new ActivityReward { Type = RewardType.Item, Id = itemId, Count = itemCount });
            }

            return rewards;
        }

        /// <summary>
        /// 生成里程碑额外奖励
        /// 每连续签到7天的额外奖励
        /// </summary>
        /// <param name="weekNum">周数（第几周）</param>
        /// <returns>额外奖励列表</returns>
        private static List<ActivityReward> GenerateMilestoneRewards(int weekNum)
        {
            var rewards = new List<ActivityReward>();

            // 大量金币
            int bonusGold = 500 * weekNum;
            rewards.Add(new ActivityReward { Type = RewardType.Gold, Count = bonusGold });

            // 补签卡
            int retroactiveCardCount = weekNum;
            rewards.Add(new ActivityReward { Type = RewardType.Item, Id = "item_retroactive_card", Count = retroactiveCardCount });

            // 第4周（28天）额外给予装备
            if (weekNum >= 4)
            {
                rewards.Add(new ActivityReward { Type = RewardType.Equipment, Id = "2", Count = 1 });
            }

            return rewards;
        }

        /// <summary>
        /// 根据天数获取随机道具ID
        /// </summary>
        /// <param name="day">签到天数</param>
        /// <returns>道具ID</returns>
        private static string GetRandomItemId(int day)
        {
            // 根据天数返回不同的道具
            if (day <= 5)
                return "item_001"; // 小型生命药水
            else if (day <= 10)
                return "item_002"; // 中型生命药水
            else if (day <= 15)
                return "item_003"; // 狼牙
            else if (day <= 20)
                return "item_004"; // 经验书
            else
                return "item_005"; // 保护石
        }

        #endregion

        #region 签到查询

        /// <summary>
        /// 获取签到配置
        /// </summary>
        /// <param name="configId">配置ID</param>
        /// <returns>签到配置，不存在返回null</returns>
        public static CheckInConfig? GetCheckInConfig(string configId = "daily_checkin")
        {
            return checkInConfigs.ContainsKey(configId) ? checkInConfigs[configId] : null;
        }

        /// <summary>
        /// 获取玩家签到数据
        /// 如果不存在则创建新数据
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <returns>玩家签到数据</returns>
        public static PlayerCheckInData GetOrCreatePlayerData(string playerId)
        {
            if (!playerCheckInData.ContainsKey(playerId))
            {
                playerCheckInData[playerId] = new PlayerCheckInData
                {
                    PlayerId = playerId,
                    ConfigId = "daily_checkin",
                    ContinuousDays = 0,
                    TotalCheckInDays = 0,
                    CheckedDates = [],
                    LastCheckInDate = null,
                    RetroactiveCount = 0,
                    Month = DateTime.Now.Month,
                    Year = DateTime.Now.Year
                };
            }

            // 检查是否需要重置（新月份）
            CheckAndResetMonth(playerId);

            return playerCheckInData[playerId];
        }

        /// <summary>
        /// 检查并重置月份
        /// 如果是新月份，重置签到数据
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        private static void CheckAndResetMonth(string playerId)
        {
            var data = playerCheckInData[playerId];
            if (data == null) return;

            DateTime now = DateTime.Now;

            // 如果是新月份，重置签到数据
            if (data.Month != now.Month || data.Year != now.Year)
            {
                // 保留总签到天数，重置其他数据
                int totalDays = data.TotalCheckInDays;

                data.Month = now.Month;
                data.Year = now.Year;
                data.ContinuousDays = 0;
                data.CheckedDates.Clear();
                data.LastCheckInDate = null;
                data.RetroactiveCount = 0;

                // 总天数不清零，累积计算
                data.TotalCheckInDays = totalDays;
            }
        }

        #endregion

        #region 签到操作

        /// <summary>
        /// 执行签到
        /// 玩家进行每日签到
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <param name="inventory">背包管理器</param>
        /// <returns>签到结果</returns>
        public static CheckInResult CheckIn(string playerId, InventoryManager inventory)
        {
            // 获取签到配置
            var config = GetCheckInConfig();
            if (config == null)
            {
                return new CheckInResult
                {
                    Success = false,
                    Message = "签到配置不存在"
                };
            }

            // 获取玩家数据
            var data = GetOrCreatePlayerData(playerId);

            // 获取今天的日期（只取日期部分，忽略时间）
            DateTime today = DateTime.Now.Date;

            // 检查今天是否已经签到
            if (data.CheckedDates.Contains(today))
            {
                return new CheckInResult
                {
                    Success = false,
                    Message = "今天已经签到过了",
                    AlreadyChecked = true
                };
            }

            // 计算连续签到天数
            // 检查昨天是否签到，如果签到则连续，否则重新开始
            DateTime yesterday = today.AddDays(-1);
            if (data.LastCheckInDate == yesterday.Date)
            {
                // 昨天签到了，连续天数+1
                data.ContinuousDays++;
            }
            else
            {
                // 昨天没签到，重新开始
                data.ContinuousDays = 1;
            }

            // 确保连续天数不超过最大值
            if (data.ContinuousDays > config.MaxContinuousDays)
            {
                data.ContinuousDays = config.MaxContinuousDays;
            }

            // 记录签到
            data.CheckedDates.Add(today);
            data.LastCheckInDate = today;
            data.TotalCheckInDays++;

            // 获取签到奖励
            var stage = config.Stages.FirstOrDefault(s => s.Day == data.ContinuousDays);
            if (stage == null)
            {
                // 如果没有该天数的配置，使用默认奖励
                stage = config.Stages.FirstOrDefault(s => s.Day == config.MaxContinuousDays);
            }

            // 发放奖励
            var grantedRewards = new List<string>();
            if (stage != null)
            {
                // 发放基础奖励
                foreach (var reward in stage.Rewards)
                {
                    if (GrantReward(reward, inventory))
                    {
                        grantedRewards.Add(reward.Name);
                    }
                }

                // 如果是里程碑，发放额外奖励
                if (stage.IsMilestone && stage.ExtraRewards != null)
                {
                    foreach (var reward in stage.ExtraRewards)
                    {
                        if (GrantReward(reward, inventory))
                        {
                            grantedRewards.Add($"[里程碑]{reward.Name}");
                        }
                    }
                }
            }

            return new CheckInResult
            {
                Success = true,
                Message = "签到成功！",
                ContinuousDays = data.ContinuousDays,
                TotalDays = data.TotalCheckInDays,
                IsMilestone = stage != null && stage.IsMilestone,
                Rewards = grantedRewards
            };
        }

        /// <summary>
        /// 补签
        /// 使用补签卡补签过去的某一天
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <param name="targetDate">目标补签日期</param>
        /// <param name="inventory">背包管理器</param>
        /// <returns>补签结果</returns>
        public static CheckInResult RetroactiveCheckIn(string playerId, DateTime targetDate, InventoryManager inventory)
        {
            // 获取签到配置
            var config = GetCheckInConfig();
            if (config == null)
            {
                return new CheckInResult
                {
                    Success = false,
                    Message = "签到配置不存在"
                };
            }

            // 检查是否允许补签
            if (!config.CanRetroactive)
            {
                return new CheckInResult
                {
                    Success = false,
                    Message = "该签到类型不支持补签"
                };
            }

            // 获取玩家数据
            var data = GetOrCreatePlayerData(playerId);

            // 获取今天的日期
            DateTime today = DateTime.Now.Date;
            DateTime targetDateOnly = targetDate.Date;

            // 检查目标日期是否是今天
            if (targetDateOnly == today)
            {
                return new CheckInResult
                {
                    Success = false,
                    Message = "今天的签到请直接签到，不需要补签"
                };
            }

            // 检查目标日期是否是未来日期
            if (targetDateOnly > today)
            {
                return new CheckInResult
                {
                    Success = false,
                    Message = "不能补签未来的日期"
                };
            }

            // 检查目标日期是否已经签到
            if (data.CheckedDates.Contains(targetDateOnly))
            {
                return new CheckInResult
                {
                    Success = false,
                    Message = "该日期已经签到过了"
                };
            }

            // 检查补签天数是否超出限制
            int daysDiff = (today - targetDateOnly).Days;
            if (daysDiff > config.MaxRetroactiveDays)
            {
                return new CheckInResult
                {
                    Success = false,
                    Message = $"只能补签最近 {config.MaxRetroactiveDays} 天的签到"
                };
            }

            // 检查本月是否已使用补签次数
            if (data.RetroactiveCount >= config.MaxRetroactiveDays)
            {
                return new CheckInResult
                {
                    Success = false,
                    Message = $"本月补签次数已用完（最多{config.MaxRetroactiveDays}次）"
                };
            }

            // 检查是否有补签卡
            string retroactiveItemId = "item_retroactive_card";
            if (inventory.GetItemCount(retroactiveItemId) < config.RetroactiveCost)
            {
                return new CheckInResult
                {
                    Success = false,
                    Message = $"补签卡不足，需要 {config.RetroactiveCost} 张补签卡"
                };
            }

            // 消耗补签卡
            inventory.RemoveItem(retroactiveItemId, config.RetroactiveCost);

            // 计算补签后的连续天数
            // 找出最近一次签到的日期
            DateTime lastCheckInDate = data.CheckedDates.Count > 0
                ? data.CheckedDates.Max()
                : DateTime.MinValue;

            // 检查补签日期是否与最后一次签到连续
            bool isContinuous = (targetDateOnly - lastCheckInDate).Days == 1;

            if (isContinuous)
            {
                // 连续的，增加连续天数
                data.ContinuousDays++;
            }
            else
            {
                // 不连续，重新计算连续天数
                // 从补签日期开始数到今天，计算实际连续天数
                data.ContinuousDays = 1;
                DateTime checkDate = targetDateOnly.AddDays(1);
                while (checkDate < today)
                {
                    if (data.CheckedDates.Contains(checkDate))
                    {
                        data.ContinuousDays++;
                        checkDate = checkDate.AddDays(1);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            // 确保连续天数不超过最大值
            if (data.ContinuousDays > config.MaxContinuousDays)
            {
                data.ContinuousDays = config.MaxContinuousDays;
            }

            // 记录补签
            data.CheckedDates.Add(targetDateOnly);
            data.RetroactiveCount++;
            data.TotalCheckInDays++;

            // 获取签到奖励
            var stage = config.Stages.FirstOrDefault(s => s.Day == data.ContinuousDays);
            if (stage == null)
            {
                stage = config.Stages.FirstOrDefault(s => s.Day == config.MaxContinuousDays);
            }

            // 发放奖励
            var grantedRewards = new List<string>();
            if (stage != null)
            {
                foreach (var reward in stage.Rewards)
                {
                    if (GrantReward(reward, inventory))
                    {
                        grantedRewards.Add(reward.Name);
                    }
                }

                if (stage.IsMilestone && stage.ExtraRewards != null)
                {
                    foreach (var reward in stage.ExtraRewards)
                    {
                        if (GrantReward(reward, inventory))
                        {
                            grantedRewards.Add($"[里程碑]{reward.Name}");
                        }
                    }
                }
            }

            return new CheckInResult
            {
                Success = true,
                Message = "补签成功！",
                ContinuousDays = data.ContinuousDays,
                TotalDays = data.TotalCheckInDays,
                IsMilestone = stage != null && stage.IsMilestone,
                Rewards = grantedRewards,
                IsRetroactive = true
            };
        }

        /// <summary>
        /// 发放奖励
        /// </summary>
        /// <param name="reward">奖励对象</param>
        /// <param name="inventory">背包管理器</param>
        /// <returns>是否成功发放</returns>
        private static bool GrantReward(ActivityReward reward, InventoryManager inventory)
        {
            switch (reward.Type)
            {
                case RewardType.Gold:
                    // TODO: 增加玩家金币
                    return true;

                case RewardType.Exp:
                    // TODO: 增加玩家经验
                    return true;

                case RewardType.Item:
                    return inventory.AddItem(reward.Id, reward.Count);

                case RewardType.Equipment:
                    // TODO: 生成并添加装备
                    return true;

                default:
                    return false;
            }
        }

        #endregion

        #region 签到状态查询

        /// <summary>
        /// 获取签到状态显示
        /// 返回用于UI显示的签到信息
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <returns>签到状态显示对象</returns>
        public static CheckInDisplay GetCheckInDisplay(string playerId)
        {
            var config = GetCheckInConfig();
            if (config == null)
            {
                return new CheckInDisplay();
            }

            var data = GetOrCreatePlayerData(playerId);

            DateTime today = DateTime.Now.Date;
            bool isCheckedToday = data.CheckedDates.Contains(today);

            // 计算本月已签到天数
            int checkedDaysThisMonth = data.CheckedDates.Count;

            // 检查是否可以补签
            bool canRetroactive = config.CanRetroactive &&
                                   !isCheckedToday &&
                                   data.RetroactiveCount < config.MaxRetroactiveDays;

            // 计算下个里程碑
            int nextMilestoneDay = GetNextMilestoneDay(data.ContinuousDays, config);

            return new CheckInDisplay
            {
                Config = config,
                ContinuousDays = data.ContinuousDays,
                TotalDays = data.TotalCheckInDays,
                CheckedDaysThisMonth = checkedDaysThisMonth,
                IsCheckedToday = isCheckedToday,
                CanRetroactive = canRetroactive,
                RetroactiveCount = data.RetroactiveCount,
                MaxRetroactiveCount = config.MaxRetroactiveDays,
                NextMilestoneDay = nextMilestoneDay,
                DaysToNextMilestone = nextMilestoneDay - data.ContinuousDays,
                LastCheckInDate = data.LastCheckInDate
            };
        }

        /// <summary>
        /// 获取下一个里程碑天数
        /// </summary>
        /// <param name="currentDay">当前连续天数</param>
        /// <param name="config">签到配置</param>
        /// <returns>下一个里程碑天数</returns>
        private static int GetNextMilestoneDay(int currentDay, CheckInConfig config)
        {
            // 里程碑每7天一次
            int nextMilestone = ((currentDay / 7) + 1) * 7;

            // 确保不超过最大值
            if (nextMilestone > config.MaxContinuousDays)
            {
                nextMilestone = config.MaxContinuousDays;
            }

            return nextMilestone;
        }

        /// <summary>
        /// 获取可以补签的日期列表
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <returns>可以补签的日期列表</returns>
        public static List<DateTime> GetRetroactiveDates(string playerId)
        {
            var config = GetCheckInConfig();
            var data = GetOrCreatePlayerData(playerId);
            var dates = new List<DateTime>();

            if (config == null)
            {
                return dates;
            }

            if (!config.CanRetroactive)
            {
                return dates;
            }

            DateTime today = DateTime.Now.Date;

            // 检查最近可以补签的日期
            for (int i = 1; i <= config.MaxRetroactiveDays; i++)
            {
                DateTime date = today.AddDays(-i);

                // 只检查本月
                if (date.Month != today.Month || date.Year != today.Year)
                {
                    continue;
                }

                // 该日期未签到
                if (!data.CheckedDates.Contains(date))
                {
                    dates.Add(date);
                }
            }

            return dates;
        }

        #endregion

        #region 数据管理

        /// <summary>
        /// 清空玩家签到数据
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        public static void ClearPlayerData(string playerId)
        {
            if (playerCheckInData.ContainsKey(playerId))
            {
                playerCheckInData.Remove(playerId);
            }
        }

        /// <summary>
        /// 获取所有玩家签到数据
        /// </summary>
        /// <returns>玩家签到数据字典</returns>
        public static Dictionary<string, PlayerCheckInData> GetAllPlayerData()
        {
            return new Dictionary<string, PlayerCheckInData>(playerCheckInData);
        }

        #endregion
    }

    #region 数据类

    /// <summary>
    /// 签到周期类型枚举
    /// </summary>
    public enum CheckInCycleType
    {
        /// <summary>
        /// 每日周期
        /// </summary>
        Daily,

        /// <summary>
        /// 每周周期
        /// </summary>
        Weekly,

        /// <summary>
        /// 每月周期
        /// </summary>
        Monthly
    }

    /// <summary>
    /// 签到配置类
    /// 定义签到活动的完整配置
    /// </summary>
    public class CheckInConfig
    {
        /// <summary>
        /// 配置ID
        /// </summary>
        public string ConfigId { get; set; } = string.Empty;

        /// <summary>
        /// 配置名称
        /// </summary>
        public string ConfigName { get; set; } = string.Empty;

        /// <summary>
        /// 配置描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 周期类型
        /// </summary>
        public CheckInCycleType CycleType { get; set; }

        /// <summary>
        /// 最大连续签到天数
        /// 超过此天数后循环或重置
        /// </summary>
        public int MaxContinuousDays { get; set; }

        /// <summary>
        /// 是否允许补签
        /// </summary>
        public bool CanRetroactive { get; set; }

        /// <summary>
        /// 最大补签天数
        /// </summary>
        public int MaxRetroactiveDays { get; set; }

        /// <summary>
        /// 补签消耗
        /// 每次补签需要的补签卡数量
        /// </summary>
        public int RetroactiveCost { get; set; }

        /// <summary>
        /// 签到阶段列表
        /// 每一天对应的奖励配置
        /// </summary>
        public List<CheckInStage> Stages { get; set; } = [];
    }

    /// <summary>
    /// 签到阶段类
    /// 定义某一天的签到奖励
    /// </summary>
    public class CheckInStage
    {
        /// <summary>
        /// 签到天数（第几天）
        /// </summary>
        public int Day { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 是否是里程碑
        /// 里程碑有额外的特殊奖励
        /// </summary>
        public bool IsMilestone { get; set; }

        /// <summary>
        /// 基础奖励列表
        /// </summary>
        public List<ActivityReward> Rewards { get; set; } = [];

        /// <summary>
        /// 里程碑额外奖励
        /// </summary>
        public List<ActivityReward> ExtraRewards { get; set; } = [];
    }

    /// <summary>
    /// 玩家签到数据类
    /// 记录玩家的签到状态和进度
    /// </summary>
    public class PlayerCheckInData
    {
        /// <summary>
        /// 玩家ID
        /// </summary>
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 签到配置ID
        /// </summary>
        public string ConfigId { get; set; } = string.Empty;

        /// <summary>
        /// 当前连续签到天数
        /// </summary>
        public int ContinuousDays { get; set; }

        /// <summary>
        /// 总签到天数（历史累计）
        /// </summary>
        public int TotalCheckInDays { get; set; }

        /// <summary>
        /// 已签到的日期列表
        /// </summary>
        public HashSet<DateTime> CheckedDates { get; set; } = [];

        /// <summary>
        /// 最后签到日期
        /// </summary>
        public DateTime? LastCheckInDate { get; set; }

        /// <summary>
        /// 本月补签次数
        /// </summary>
        public int RetroactiveCount { get; set; }

        /// <summary>
        /// 当前月份
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// 当前年份
        /// </summary>
        public int Year { get; set; }
    }

    /// <summary>
    /// 签到结果类
    /// 封装签到操作的返回结果
    /// </summary>
    public class CheckInResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 结果消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 是否已经签到过
        /// </summary>
        public bool AlreadyChecked { get; set; }

        /// <summary>
        /// 是否是补签
        /// </summary>
        public bool IsRetroactive { get; set; }

        /// <summary>
        /// 当前连续签到天数
        /// </summary>
        public int ContinuousDays { get; set; }

        /// <summary>
        /// 总签到天数
        /// </summary>
        public int TotalDays { get; set; }

        /// <summary>
        /// 是否是里程碑
        /// </summary>
        public bool IsMilestone { get; set; }

        /// <summary>
        /// 获得的奖励列表
        /// </summary>
        public List<string> Rewards { get; set; } = [];
    }

    /// <summary>
    /// 签到状态显示类
    /// 用于UI显示的完整签到信息
    /// </summary>
    public class CheckInDisplay
    {
        /// <summary>
        /// 签到配置
        /// </summary>
        public CheckInConfig Config { get; set; } = new CheckInConfig();

        /// <summary>
        /// 当前连续签到天数
        /// </summary>
        public int ContinuousDays { get; set; }

        /// <summary>
        /// 总签到天数
        /// </summary>
        public int TotalDays { get; set; }

        /// <summary>
        /// 本月已签到天数
        /// </summary>
        public int CheckedDaysThisMonth { get; set; }

        /// <summary>
        /// 今天是否已签到
        /// </summary>
        public bool IsCheckedToday { get; set; }

        /// <summary>
        /// 是否可以补签
        /// </summary>
        public bool CanRetroactive { get; set; }

        /// <summary>
        /// 本月已使用补签次数
        /// </summary>
        public int RetroactiveCount { get; set; }

        /// <summary>
        /// 本月最大补签次数
        /// </summary>
        public int MaxRetroactiveCount { get; set; }

        /// <summary>
        /// 下一个里程碑天数
        /// </summary>
        public int NextMilestoneDay { get; set; }

        /// <summary>
        /// 距离下一个里程碑还有多少天
        /// </summary>
        public int DaysToNextMilestone { get; set; }

        /// <summary>
        /// 最后签到日期
        /// </summary>
        public DateTime? LastCheckInDate { get; set; }

        /// <summary>
        /// 获取签到按钮文本
        /// </summary>
        /// <returns>按钮文本</returns>
        public string GetButtonText()
        {
            if (IsCheckedToday)
            {
                return "已签到";
            }
            else
            {
                return "签到";
            }
        }

        /// <summary>
        /// 获取连续签到文本
        /// </summary>
        /// <returns>连续签到文本</returns>
        public string GetContinuousDaysText()
        {
            return $"已连续签到 {ContinuousDays} 天";
        }

        /// <summary>
        /// 获取下个里程碑文本
        /// </summary>
        /// <returns>里程碑文本</returns>
        public string GetNextMilestoneText()
        {
            if (ContinuousDays >= NextMilestoneDay)
            {
                return "已达本阶段最高里程碑！";
            }
            else
            {
                return $"距离下一里程碑还需 {DaysToNextMilestone} 天";
            }
        }
    }

    #endregion
}
