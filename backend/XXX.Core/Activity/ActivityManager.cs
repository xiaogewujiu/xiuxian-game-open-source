using XXX.Entity;
using XXX.Inventory;

namespace XXX.Activity
{
    /// <summary>
    /// 活动管理器
    /// 负责管理所有活动的配置、进度更新、奖励发放等核心功能
    /// </summary>
    public class ActivityManager
    {
        /// <summary>
        /// 活动配置字典
        /// 存储所有活动的配置信息
        /// Key: 活动ID, Value: 活动配置
        /// </summary>
        private static Dictionary<string, ActivityConfig> activities = [];

        /// <summary>
        /// 玩家活动进度字典
        /// 存储玩家在各个活动中的进度数据
        /// Key: 活动ID, Value: 玩家进度
        /// </summary>
        private static Dictionary<string, PlayerActivityProgress> playerProgress = [];

        /// <summary>
        /// 随机数生成器
        /// 用于随机生成道具等功能
        /// </summary>
        private static Random random = new Random();

        #region 初始化

        /// <summary>
        /// 初始化活动系统
        /// 注册所有活动配置
        /// </summary>
        public static void Initialize()
        {
            // 注册战斗类活动
            RegisterDamageDealtActivity();
            RegisterDamageTakenActivity();
            RegisterKillCountActivity();
            RegisterWinCountActivity();

            // 注册收集类活动
            RegisterCollectionActivity();

            // 可以在这里继续添加更多活动
        }

        #endregion

        #region 活动注册

        /// <summary>
        /// 注册活动：累计造成伤害
        /// 这是一个多阶段的战斗类活动
        /// </summary>
        private static void RegisterDamageDealtActivity()
        {
            var activity = new ActivityConfig
            {
                ActivityId = "activity_damage_dealt",
                Name = "伤害大师",
                Description = "活动期间累计造成伤害，赢取丰厚奖励！强化你的攻击力，击败更多敌人！",
                Type = ActivityType.Battle,
                BattleTask = BattleTaskType.TotalDamageDealt,
                StartTime = DateTime.Now.AddDays(-1), // 活动已开始1天
                EndTime = DateTime.Now.AddDays(7),    // 活动还剩7天
                Icon = "Icons/Activities/damage_dealt.png",
                SortOrder = 1,
                Stages =
                [
                    // 阶段1：累计造成10,000点伤害
                    new ActivityStage
                    {
                        Stage = 1,
                        TargetValue = 10000,
                        Description = "累计造成 10,000 点伤害",
                        Rewards =
                        [
                            new ActivityReward { Type = RewardType.Gold, Id = "", Count = 500 },
                            new ActivityReward { Type = RewardType.Exp, Id = "", Count = 1000 }
                        ]
                    },
                    // 阶段2：累计造成50,000点伤害
                    new ActivityStage
                    {
                        Stage = 2,
                        TargetValue = 50000,
                        Description = "累计造成 50,000 点伤害",
                        Rewards =
                        [
                            new ActivityReward { Type = RewardType.Gold, Id = "", Count = 2000 },
                            new ActivityReward { Type = RewardType.Item, Id = "item_004", Count = 2 }
                        ]
                    },
                    // 阶段3：累计造成100,000点伤害
                    new ActivityStage
                    {
                        Stage = 3,
                        TargetValue = 100000,
                        Description = "累计造成 100,000 点伤害",
                        Rewards =
                        [
                            new ActivityReward { Type = RewardType.Gold, Id = "", Count = 5000 },
                            new ActivityReward { Type = RewardType.Item, Id = "item_005", Count = 1 }
                        ]
                    },
                    // 阶段4：累计造成500,000点伤害（终极目标）
                    new ActivityStage
                    {
                        Stage = 4,
                        TargetValue = 500000,
                        Description = "累计造成 500,000 点伤害",
                        Rewards =
                        [
                            new ActivityReward { Type = RewardType.Gold, Id = "", Count = 10000 },
                            new ActivityReward { Type = RewardType.Equipment, Id = "2", Count = 1 }
                        ]
                    }
                ]
            };

            activities[activity.ActivityId] = activity;
        }

        /// <summary>
        /// 注册活动：累计承受伤害
        /// 这是一个多阶段的战斗类活动，测试玩家的生存能力
        /// </summary>
        private static void RegisterDamageTakenActivity()
        {
            var activity = new ActivityConfig
            {
                ActivityId = "activity_damage_taken",
                Name = "坚韧不拔",
                Description = "活动期间累计承受伤害，证明你的实力和耐力！生存即是胜利！",
                Type = ActivityType.Battle,
                BattleTask = BattleTaskType.TotalDamageTaken,
                StartTime = DateTime.Now.AddDays(-1),
                EndTime = DateTime.Now.AddDays(7),
                Icon = "Icons/Activities/damage_taken.png",
                SortOrder = 2,
                Stages =
                [
                    // 阶段1：累计承受50,000点伤害
                    new ActivityStage
                    {
                        Stage = 1,
                        TargetValue = 50000,
                        Description = "累计承受 50,000 点伤害",
                        Rewards =
                        [
                            new ActivityReward { Type = RewardType.Gold, Id = "", Count = 1000 },
                            new ActivityReward { Type = RewardType.Item, Id = "item_001", Count = 5 }
                        ]
                    },
                    // 阶段2：累计承受200,000点伤害
                    new ActivityStage
                    {
                        Stage = 2,
                        TargetValue = 200000,
                        Description = "累计承受 200,000 点伤害",
                        Rewards =
                        [
                            new ActivityReward { Type = RewardType.Gold, Id = "", Count = 3000 },
                            new ActivityReward { Type = RewardType.Item, Id = "item_002", Count = 3 }
                        ]
                    }
                ]
            };

            activities[activity.ActivityId] = activity;
        }

        /// <summary>
        /// 注册活动：击杀数量统计
        /// 这是一个多阶段的战斗类活动
        /// </summary>
        private static void RegisterKillCountActivity()
        {
            var activity = new ActivityConfig
            {
                ActivityId = "activity_kill_count",
                Name = "狩猎大师",
                Description = "活动期间累计击杀野怪，成为狩猎之王！展示你的战斗技巧！",
                Type = ActivityType.Battle,
                BattleTask = BattleTaskType.TotalKills,
                StartTime = DateTime.Now.AddDays(-1),
                EndTime = DateTime.Now.AddDays(7),
                Icon = "Icons/Activities/kill_count.png",
                SortOrder = 3,
                Stages =
                [
                    // 阶段1：击杀10只野怪
                    new ActivityStage
                    {
                        Stage = 1,
                        TargetValue = 10,
                        Description = "累计击杀 10 只野怪",
                        Rewards =
                        [
                            new ActivityReward { Type = RewardType.Gold, Id = "", Count = 500 },
                            new ActivityReward { Type = RewardType.Exp, Id = "", Count = 2000 }
                        ]
                    },
                    // 阶段2：击杀50只野怪
                    new ActivityStage
                    {
                        Stage = 2,
                        TargetValue = 50,
                        Description = "累计击杀 50 只野怪",
                        Rewards =
                        [
                            new ActivityReward { Type = RewardType.Gold, Id = "", Count = 2000 },
                            new ActivityReward { Type = RewardType.Item, Id = "item_003", Count = 10 }
                        ]
                    },
                    // 阶段3：击杀100只野怪
                    new ActivityStage
                    {
                        Stage = 3,
                        TargetValue = 100,
                        Description = "累计击杀 100 只野怪",
                        Rewards =
                        [
                            new ActivityReward { Type = RewardType.Gold, Id = "", Count = 5000 },
                            new ActivityReward { Type = RewardType.Equipment, Id = "1", Count = 1 }
                        ]
                    }
                ]
            };

            activities[activity.ActivityId] = activity;
        }

        /// <summary>
        /// 注册活动：胜利场次统计
        /// 这是一个多阶段的战斗类活动
        /// </summary>
        private static void RegisterWinCountActivity()
        {
            var activity = new ActivityConfig
            {
                ActivityId = "activity_win_count",
                Name = "常胜将军",
                Description = "活动期间累计获得战斗胜利，证明你的实力！",
                Type = ActivityType.Battle,
                BattleTask = BattleTaskType.TotalWins,
                StartTime = DateTime.Now.AddDays(-1),
                EndTime = DateTime.Now.AddDays(7),
                Icon = "Icons/Activities/win_count.png",
                SortOrder = 4,
                Stages =
                [
                    // 阶段1：累计获胜5场
                    new ActivityStage
                    {
                        Stage = 1,
                        TargetValue = 5,
                        Description = "累计获得 5 场胜利",
                        Rewards =
                        [
                            new ActivityReward { Type = RewardType.Gold, Id = "", Count = 1000 },
                            new ActivityReward { Type = RewardType.Item, Id = "item_004", Count = 1 }
                        ]
                    },
                    // 阶段2：累计获胜20场
                    new ActivityStage
                    {
                        Stage = 2,
                        TargetValue = 20,
                        Description = "累计获得 20 场胜利",
                        Rewards =
                        [
                            new ActivityReward { Type = RewardType.Gold, Id = "", Count = 5000 },
                            new ActivityReward { Type = RewardType.Item, Id = "item_005", Count = 2 }
                        ]
                    }
                ]
            };

            activities[activity.ActivityId] = activity;
        }

        /// <summary>
        /// 注册活动：材料收集
        /// 这是一个多阶段的收集类活动
        /// </summary>
        private static void RegisterCollectionActivity()
        {
            var activity = new ActivityConfig
            {
                ActivityId = "activity_collection",
                Name = "材料收集",
                Description = "提交狼牙材料，换取丰厚奖励！通过击败野怪获得狼牙并提交。",
                Type = ActivityType.Collection,
                TargetItemId = "item_003", // 狼牙
                StartTime = DateTime.Now.AddDays(-1),
                EndTime = DateTime.Now.AddDays(7),
                Icon = "Icons/Activities/collection.png",
                SortOrder = 5,
                Stages =
                [
                    // 阶段1：提交5个狼牙
                    new ActivityStage
                    {
                        Stage = 1,
                        TargetValue = 5,
                        Description = "提交 5 个狼牙",
                        Rewards =
                        [
                            new ActivityReward { Type = RewardType.Gold, Id = "", Count = 1000 }
                        ]
                    },
                    // 阶段2：提交20个狼牙
                    new ActivityStage
                    {
                        Stage = 2,
                        TargetValue = 20,
                        Description = "提交 20 个狼牙",
                        Rewards =
                        [
                            new ActivityReward { Type = RewardType.Gold, Id = "", Count = 5000 },
                            new ActivityReward { Type = RewardType.Item, Id = "item_004", Count = 3 }
                        ]
                    },
                    // 阶段3：提交50个狼牙
                    new ActivityStage
                    {
                        Stage = 3,
                        TargetValue = 50,
                        Description = "提交 50 个狼牙",
                        Rewards =
                        [
                            new ActivityReward { Type = RewardType.Gold, Id = "", Count = 15000 },
                            new ActivityReward { Type = RewardType.Equipment, Id = "3", Count = 1 }
                        ]
                    }
                ]
            };

            activities[activity.ActivityId] = activity;
        }

        #endregion

        #region 活动查询

        /// <summary>
        /// 获取所有活动
        /// 返回所有已注册的活动配置列表
        /// </summary>
        /// <returns>所有活动的列表</returns>
        public static List<ActivityConfig> GetAllActivities()
        {
            return activities.Values.OrderBy(a => a.SortOrder).ToList();
        }

        /// <summary>
        /// 获取活跃活动
        /// 返回当前正在进行中的活动（在开始和结束时间之间）
        /// </summary>
        /// <returns>活跃活动列表</returns>
        public static List<ActivityConfig> GetActiveActivities()
        {
            return activities.Values
                .Where(a => a.IsActive)
                .OrderBy(a => a.SortOrder)
                .ToList();
        }

        /// <summary>
        /// 根据活动ID获取活动配置
        /// </summary>
        /// <param name="activityId">活动ID</param>
        /// <returns>活动配置，不存在则返回null</returns>
        public static ActivityConfig? GetActivity(string activityId)
        {
            if (activities.ContainsKey(activityId))
            {
                return activities[activityId];
            }

            return null;
        }

        /// <summary>
        /// 根据类型获取活动
        /// 返回指定类型的所有活动
        /// </summary>
        /// <param name="type">活动类型</param>
        /// <returns>指定类型的活动列表</returns>
        public static List<ActivityConfig> GetActivitiesByType(ActivityType type)
        {
            return activities.Values
                .Where(a => a.Type == type)
                .OrderBy(a => a.SortOrder)
                .ToList();
        }

        /// <summary>
        /// 检查活动是否存在
        /// </summary>
        /// <param name="activityId">活动ID</param>
        /// <returns>是否存在</returns>
        public static bool ActivityExists(string activityId)
        {
            return activities.ContainsKey(activityId);
        }

        #endregion

        #region 进度管理

        /// <summary>
        /// 获取或创建玩家进度
        /// 如果玩家在该活动中还没有进度记录，则创建一个新的进度对象
        /// </summary>
        /// <param name="activityId">活动ID</param>
        /// <returns>玩家活动进度对象</returns>
        public static PlayerActivityProgress GetOrCreateProgress(string activityId)
        {
            // 如果不存在该活动的进度记录，创建一个新的
            if (!playerProgress.ContainsKey(activityId))
            {
                playerProgress[activityId] = new PlayerActivityProgress
                {
                    ActivityId = activityId,
                    CurrentProgress = 0,
                    StartTime = DateTime.Now,
                    LastUpdateTime = DateTime.Now
                };
            }

            return playerProgress[activityId];
        }

        /// <summary>
        /// 获取玩家进度
        /// 如果不存在则返回null
        /// </summary>
        /// <param name="activityId">活动ID</param>
        /// <returns>玩家活动进度对象，不存在返回null</returns>
        public static PlayerActivityProgress? GetProgress(string activityId)
        {
            if (playerProgress.ContainsKey(activityId))
            {
                return playerProgress[activityId];
            }

            return null;
        }

        /// <summary>
        /// 更新战斗进度
        /// 在战斗结束后调用，自动更新所有相关战斗活动的进度
        /// </summary>
        /// <param name="taskType">战斗任务类型</param>
        /// <param name="value">增加的数值</param>
        public static void UpdateBattleProgress(BattleTaskType taskType, long value)
        {
            // 遍历所有活跃的战斗类活动
            foreach (var activity in activities.Values
                .Where(a => a.Type == ActivityType.Battle && a.BattleTask == taskType && a.IsActive))
            {
                // 获取或创建玩家进度
                var progress = GetOrCreateProgress(activity.ActivityId);

                // 增加进度值
                progress.CurrentProgress += value;
                progress.LastUpdateTime = DateTime.Now;

                // 检查是否完成所有阶段
                var maxStage = activity.Stages.Max(s => s.Stage);
                var lastStage = activity.Stages.First(s => s.Stage == maxStage);

                if (progress.CurrentProgress >= lastStage.TargetValue)
                {
                    progress.IsCompleted = true;
                }
            }
        }

        /// <summary>
        /// 直接设置活动进度（用于特殊场景）
        /// </summary>
        /// <param name="activityId">活动ID</param>
        /// <param name="value">要设置的进度值</param>
        public static void SetProgress(string activityId, long value)
        {
            var progress = GetOrCreateProgress(activityId);
            progress.CurrentProgress = value;
            progress.LastUpdateTime = DateTime.Now;
        }

        #endregion

        #region 收集类活动

        /// <summary>
        /// 提交收集物品
        /// 用于收集类活动，玩家提交道具来增加进度
        /// </summary>
        /// <param name="activityId">活动ID</param>
        /// <param name="itemId">道具ID</param>
        /// <param name="count">提交数量</param>
        /// <param name="inventory">背包管理器</param>
        /// <returns>提交结果</returns>
        public static CollectionResult SubmitCollectionItem(string activityId, string itemId, int count, InventoryManager inventory)
        {
            // 检查活动是否存在
            var activity = GetActivity(activityId);
            if (activity == null || activity.Type != ActivityType.Collection)
            {
                return new CollectionResult
                {
                    Success = false,
                    Message = "活动不存在或不是收集类活动"
                };
            }

            // 检查提交的道具是否正确
            if (activity.TargetItemId != itemId)
            {
                return new CollectionResult
                {
                    Success = false,
                    Message = "提交的道具类型不正确"
                };
            }

            // 检查背包是否有足够道具
            int currentCount = inventory.GetItemCount(itemId);
            if (currentCount < count)
            {
                return new CollectionResult
                {
                    Success = false,
                    Message = $"道具不足，需要 {count} 个，当前 {currentCount} 个"
                };
            }

            // 消耗道具
            bool removed = inventory.RemoveItem(itemId, count);
            if (!removed)
            {
                return new CollectionResult
                {
                    Success = false,
                    Message = "扣除道具失败"
                };
            }

            // 更新进度
            var progress = GetOrCreateProgress(activityId);

            // 记录已提交的道具数量
            if (!progress.SubmittedItems.ContainsKey(itemId))
            {
                progress.SubmittedItems[itemId] = 0;
            }
            progress.SubmittedItems[itemId] += count;
            progress.CurrentProgress = progress.SubmittedItems[itemId];
            progress.LastUpdateTime = DateTime.Now;

            return new CollectionResult
            {
                Success = true,
                Message = $"成功提交 {count} 个道具",
                SubmittedCount = count,
                TotalSubmitted = progress.CurrentProgress
            };
        }

        #endregion

        #region 奖励领取

        /// <summary>
        /// 领取阶段奖励
        /// 玩家达到阶段目标后领取对应的奖励
        /// </summary>
        /// <param name="activityId">活动ID</param>
        /// <param name="stage">阶段编号</param>
        /// <param name="player">玩家实体</param>
        /// <param name="inventory">背包管理器</param>
        /// <returns>领取结果</returns>
        public static RewardClaimResult ClaimStageReward(string activityId, int stage, UserEntity player, InventoryManager inventory)
        {
            // 检查活动是否存在
            var activity = GetActivity(activityId);
            if (activity == null)
            {
                return new RewardClaimResult
                {
                    Success = false,
                    Message = "活动不存在"
                };
            }

            // 检查阶段是否存在
            var stageConfig = activity.Stages.FirstOrDefault(s => s.Stage == stage);
            if (stageConfig == null)
            {
                return new RewardClaimResult
                {
                    Success = false,
                    Message = "阶段不存在"
                };
            }

            // 获取玩家进度
            var progress = GetOrCreateProgress(activityId);

            // 检查是否已领取
            if (progress.ClaimedStages.Contains(stage))
            {
                return new RewardClaimResult
                {
                    Success = false,
                    Message = "该阶段奖励已领取"
                };
            }

            // 检查是否达到目标
            if (progress.CurrentProgress < stageConfig.TargetValue)
            {
                return new RewardClaimResult
                {
                    Success = false,
                    Message = "未达到该阶段目标"
                };
            }

            // 发放奖励
            var rewards = new List<string>();
            foreach (var reward in stageConfig.Rewards)
            {
                bool rewardSuccess = GrantReward(reward, inventory);
                if (rewardSuccess)
                {
                    rewards.Add(reward.Name);
                }
            }

            // 标记已领取
            progress.ClaimedStages.Add(stage);

            return new RewardClaimResult
            {
                Success = true,
                Message = $"成功领取阶段 {stage} 奖励",
                Rewards = rewards
            };
        }

        /// <summary>
        /// 发放单个奖励
        /// 根据奖励类型给予玩家对应的奖励
        /// </summary>
        /// <param name="reward">奖励对象</param>
        /// <param name="inventory">背包管理器</param>
        /// <returns>是否成功发放</returns>
        private static bool GrantReward(ActivityReward reward, InventoryManager inventory)
        {
            switch (reward.Type)
            {
                case RewardType.Gold:
                    // TODO: 增加玩家金币（需要PlayerData类）
                    // 这里暂时只记录日志
                    return true;

                case RewardType.Exp:
                    // TODO: 增加玩家经验
                    return true;

                case RewardType.Item:
                    // 添加道具到背包
                    return inventory.AddItem(reward.Id, reward.Count);

                case RewardType.Equipment:
                    // 生成装备并添加到背包
                    return GenerateAndAddEquipment(reward.Id, inventory);

                case RewardType.Title:
                    // TODO: 发放称号
                    return true;

                case RewardType.Buff:
                    // TODO: 发放临时增益效果
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 生成并添加装备到背包
        /// </summary>
        /// <param name="equipmentId">装备模板ID</param>
        /// <param name="inventory">背包管理器</param>
        /// <returns>是否成功</returns>
        private static bool GenerateAndAddEquipment(string equipmentId, InventoryManager inventory)
        {
            // 这里简化处理，实际应该调用装备生成系统
            // 暂时返回成功
            return true;
        }

        #endregion

        #region 进度显示

        /// <summary>
        /// 获取活动进度显示
        /// 生成用于UI显示的完整进度信息
        /// </summary>
        /// <param name="activityId">活动ID</param>
        /// <returns>活动进度显示对象，活动不存在返回null</returns>
        public static ActivityProgressDisplay? GetProgressDisplay(string activityId)
        {
            // 获取活动配置
            var activity = GetActivity(activityId);
            if (activity == null)
            {
                return null;
            }

            // 获取玩家进度
            var progress = GetOrCreateProgress(activityId);

            // 计算当前阶段
            int currentStage = 0;
            long targetProgress = 0;

            // 遍历所有阶段，找出当前所在的阶段
            foreach (var stage in activity.Stages.OrderBy(s => s.Stage))
            {
                if (progress.CurrentProgress >= stage.TargetValue)
                {
                    currentStage = stage.Stage;
                }
                else
                {
                    targetProgress = stage.TargetValue;
                    break;
                }
            }

            // 如果已完成所有阶段，使用最后一个阶段的目标
            if (targetProgress == 0 && activity.Stages.Count > 0)
            {
                targetProgress = activity.Stages.Max(s => s.TargetValue);
            }

            // 计算可领取奖励的阶段
            var availableStages = new List<int>();
            foreach (var stage in activity.Stages)
            {
                // 达到目标且未领取的阶段
                if (progress.CurrentProgress >= stage.TargetValue && !progress.ClaimedStages.Contains(stage.Stage))
                {
                    availableStages.Add(stage.Stage);
                }
            }

            // 计算完成百分比
            float maxTarget = activity.Stages.Max(s => s.TargetValue);
            float percent = Math.Min(100f, (progress.CurrentProgress / (float)maxTarget) * 100f);

            // 计算剩余时间
            TimeSpan remainingTime = activity.EndTime - DateTime.Now;
            if (remainingTime < TimeSpan.Zero) remainingTime = TimeSpan.Zero;

            // 构建进度显示对象
            return new ActivityProgressDisplay
            {
                Activity = activity,
                CurrentProgress = progress.CurrentProgress,
                TargetProgress = targetProgress,
                CurrentStage = currentStage,
                TotalStages = activity.Stages.Count,
                Percent = percent,
                AvailableRewardStages = availableStages,
                ClaimedStages = progress.ClaimedStages,
                RemainingTime = remainingTime
            };
        }

        /// <summary>
        /// 获取所有活动的进度显示
        /// 用于活动列表界面显示
        /// </summary>
        /// <returns>所有活动的进度显示列表</returns>
        public static List<ActivityProgressDisplay> GetAllProgressDisplays()
        {
            var displays = new List<ActivityProgressDisplay>();

            foreach (var activity in GetActiveActivities())
            {
                var display = GetProgressDisplay(activity.ActivityId);
                if (display != null)
                {
                    displays.Add(display);
                }
            }

            return displays;
        }

        #endregion

        #region 数据管理

        /// <summary>
        /// 清空所有活动进度
        /// 用于测试或重置玩家数据
        /// </summary>
        public static void ClearAllProgress()
        {
            playerProgress.Clear();
        }

        /// <summary>
        /// 清空指定活动的进度
        /// </summary>
        /// <param name="activityId">活动ID</param>
        public static void ClearProgress(string activityId)
        {
            if (playerProgress.ContainsKey(activityId))
            {
                playerProgress.Remove(activityId);
            }
        }

        /// <summary>
        /// 获取所有活动进度
        /// </summary>
        /// <returns>玩家进度字典</returns>
        public static Dictionary<string, PlayerActivityProgress> GetAllProgress()
        {
            return new Dictionary<string, PlayerActivityProgress>(playerProgress);
        }

        #endregion
    }

    #region 结果类

    /// <summary>
    /// 收集提交结果类
    /// 封装收集类活动提交道具后的返回结果
    /// </summary>
    public class CollectionResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 结果消息
        /// 用于显示给玩家的提示信息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 本次提交的数量
        /// </summary>
        public int SubmittedCount { get; set; }

        /// <summary>
        /// 总共提交的数量（历史累计）
        /// </summary>
        public long TotalSubmitted { get; set; }
    }

    /// <summary>
    /// 奖励领取结果类
    /// 封装领取奖励后的返回结果
    /// </summary>
    public class RewardClaimResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 结果消息
        /// 用于显示给玩家的提示信息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 获得的奖励列表
        /// 记录玩家实际获得的奖励名称
        /// </summary>
        public List<string> Rewards { get; set; } = [];
    }

    #endregion
}
