namespace XXX.Quest
{
    /// <summary>
    /// 任务类型枚举
    /// 定义不同类型的任务
    /// </summary>
    public enum QuestType
    {
        /// <summary>
        /// 主线任务 - 推进剧情，奖励最丰富
        /// </summary>
        Main,

        /// <summary>
        /// 支线任务 - 丰富游戏内容
        /// </summary>
        Side,

        /// <summary>
        /// 日常任务 - 每日可完成，每日重置
        /// </summary>
        Daily,

        /// <summary>
        /// 周常任务 - 每周可完成，每周重置
        /// </summary>
        Weekly,

        /// <summary>
        /// 成就任务 - 一次性完成，不重置
        /// </summary>
        Achievement,

        /// <summary>
        /// 宗门任务 - 宗门专属，每日可完成
        /// </summary>
        Sect = 5
    }

    /// <summary>
    /// 任务状态枚举
    /// </summary>
    public enum QuestStatus
    {
        /// <summary>
        /// 未接取
        /// </summary>
        NotAccepted,

        /// <summary>
        /// 进行中
        /// </summary>
        InProgress,

        /// <summary>
        /// 已完成（可提交）
        /// </summary>
        Completed,

        /// <summary>
        /// 已提交（已完成）
        /// </summary>
        Submitted,

        /// <summary>
        /// 已放弃
        /// </summary>
        Abandoned,

        /// <summary>
        /// 已失败（限时任务超时）
        /// </summary>
        Failed
    }

    /// <summary>
    /// 任务目标类型枚举
    /// </summary>
    public enum ObjectiveType
    {
        /// <summary>
        /// 击杀怪物
        /// </summary>
        KillMonster,

        /// <summary>
        /// 收集道具
        /// </summary>
        CollectItem,

        /// <summary>
        /// 对话NPC
        /// </summary>
        TalkToNPC,

        /// <summary>
        /// 完成副本
        /// </summary>
        CompleteDungeon,

        /// <summary>
        /// 达到等级
        /// </summary>
        ReachLevel,

        /// <summary>
        /// 装备强化
        /// </summary>
        EquipEnhance,

        /// <summary>
        /// 战斗活动（造成伤害/击杀等）
        /// </summary>
        BattleActivity,

        /// <summary>
        /// 指定地图战斗次数。
        /// </summary>
        MapBattleCount,

        /// <summary>
        /// 指定地图胜利次数。
        /// </summary>
        MapWinCount,

        /// <summary>
        /// 炼丹领取次数。
        /// </summary>
        AlchemyCollect,

        /// <summary>
        /// 锻造领取次数。
        /// </summary>
        ForgeCollect,

        /// <summary>
        /// 指定丹方炼丹成功次数。
        /// </summary>
        AlchemySuccess,

        /// <summary>
        /// 指定图纸锻造成功次数。
        /// </summary>
        ForgeSuccess,

        /// <summary>
        /// 装备强化次数。
        /// </summary>
        EquipEnhanceCount,

        /// <summary>
        /// 多少件装备达到指定强化等级。
        /// </summary>
        EquipReachEnhanceLevel,

        /// <summary>
        /// 种植指定作物次数。
        /// </summary>
        PlantCrop,

        /// <summary>
        /// 收获指定作物次数。
        /// </summary>
        HarvestCrop,

        /// <summary>
        /// 宗门捐献次数。
        /// </summary>
        Donate,

        /// <summary>
        /// 战斗胜利次数。
        /// </summary>
        WinBattle,

        /// <summary>
        /// 宗门Boss伤害。
        /// </summary>
        SectBossDamage
    }

    /// <summary>
    /// 战斗活动类型
    /// 用于战斗相关的任务目标
    /// </summary>
    public enum BattleActivityType
    {
        /// <summary>
        /// 造成伤害
        /// </summary>
        DealDamage,

        /// <summary>
        /// 承受伤害
        /// </summary>
        TakeDamage,

        /// <summary>
        /// 击杀怪物
        /// </summary>
        KillCount,

        /// <summary>
        /// 获得胜利
        /// </summary>
        WinCount,

        /// <summary>
        /// 使用技能
        /// </summary>
        UseSkill,

        /// <summary>
        /// 触发暴击
        /// </summary>
        CriticalHit
    }

    /// <summary>
    /// 任务重置周期。
    /// </summary>
    public enum QuestResetCycle
    {
        /// <summary>
        /// 不重置，一次性任务。
        /// </summary>
        None = 0,

        /// <summary>
        /// 每日重置。
        /// </summary>
        Daily = 1,

        /// <summary>
        /// 每周重置。
        /// </summary>
        Weekly = 2
    }

    /// <summary>
    /// 任务配置类
    /// 定义一个任务的完整配置
    /// </summary>
    public class QuestConfig
    {
        /// <summary>
        /// 任务唯一ID
        /// </summary>
        public string QuestId { get; set; } = string.Empty;

        /// <summary>
        /// 任务名称
        /// </summary>
        public string QuestName { get; set; } = string.Empty;

        /// <summary>
        /// 任务类型
        /// </summary>
        public QuestType QuestType { get; set; }

        /// <summary>
        /// 任务重置周期。
        /// </summary>
        public QuestResetCycle ResetCycle { get; set; } = QuestResetCycle.None;

        /// <summary>
        /// 任务描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 任务目标列表
        /// </summary>
        public List<QuestObjective> Objectives { get; set; } = [];

        /// <summary>
        /// 任务奖励
        /// </summary>
        public QuestReward Rewards { get; set; } = new QuestReward();

        /// <summary>
        /// 接取等级要求
        /// </summary>
        public int RequiredLevel { get; set; } = 1;

        /// <summary>
        /// 前置任务ID列表
        /// 必须完成这些任务才能接取此任务
        /// </summary>
        public List<string> PreQuestIds { get; set; } = [];

        /// <summary>
        /// 是否自动接取
        /// 主线任务可配置为自动接取
        /// </summary>
        public bool AutoAccept { get; set; } = false;

        /// <summary>
        /// 是否自动提交
        /// 完成后自动领取奖励
        /// </summary>
        public bool AutoSubmit { get; set; } = false;

        /// <summary>
        /// 时间限制（秒）
        /// 0表示无限制
        /// </summary>
        public int TimeLimit { get; set; } = 0;

        /// <summary>
        /// 任务阶段数
        /// 多阶段任务使用
        /// </summary>
        public int StageCount { get; set; } = 1;

        /// <summary>
        /// 任务图标
        /// </summary>
        public string Icon { get; set; } = string.Empty;

        /// <summary>
        /// 排序（用于UI显示顺序）
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 检查是否可接取
        /// </summary>
        /// <param name="player">玩家实体</param>
        /// <param name="completedQuestIds">已完成任务ID列表</param>
        /// <returns>是否可接取</returns>
        public bool CanAccept(Entity.UserEntity player, HashSet<string> completedQuestIds)
        {
            // 检查等级要求
            if (player.Level < RequiredLevel)
            {
                return false;
            }

            // 检查前置任务
            if (PreQuestIds.Count > 0)
            {
                foreach (var preQuestId in PreQuestIds)
                {
                    if (!completedQuestIds.Contains(preQuestId))
                    {
                        return false;
                    }
                }
            }

            if (ResetCycle == QuestResetCycle.None && completedQuestIds.Contains(QuestId))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 检查是否所有目标都已完成
        /// </summary>
        /// <param name="progress">任务进度</param>
        /// <returns>是否全部完成</returns>
        public bool IsAllObjectivesCompleted(QuestProgress progress)
        {
            if (Objectives.Count == 0)
            {
                return true;
            }

            for (int i = 0; i < Objectives.Count; i++)
            {
                int currentProgress = progress.GetObjectiveProgress(i);
                if (currentProgress < Objectives[i].TargetCount)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 获取任务进度文本
        /// </summary>
        /// <param name="progress">任务进度</param>
        /// <returns>进度文本</returns>
        public string GetProgressText(QuestProgress progress)
        {
            if (Objectives.Count == 0)
            {
                return "无目标";
            }

            var texts = new List<string>();
            for (int i = 0; i < Objectives.Count; i++)
            {
                int current = progress.GetObjectiveProgress(i);
                int target = Objectives[i].TargetCount;
                bool isCompleted = current >= target;

                string prefix = isCompleted ? "✓" : "○";
                texts.Add($"{prefix} {Objectives[i].Description} ({current}/{target})");
            }

            return string.Join("\n", texts);
        }

        /// <summary>
        /// 获取完成进度百分比
        /// </summary>
        /// <param name="progress">任务进度</param>
        /// <returns>百分比(0-100)</returns>
        public float GetProgressPercent(QuestProgress progress)
        {
            if (Objectives.Count == 0)
            {
                return 100f;
            }

            float totalProgress = 0f;
            for (int i = 0; i < Objectives.Count; i++)
            {
                int current = progress.GetObjectiveProgress(i);
                int target = Objectives[i].TargetCount;
                totalProgress += (float)current / target;
            }

            return totalProgress / Objectives.Count * 100f;
        }
    }

    /// <summary>
    /// 任务目标类
    /// 定义任务的单个目标
    /// </summary>
    public class QuestObjective
    {
        /// <summary>
        /// 目标类型
        /// </summary>
        public ObjectiveType ObjectiveType { get; set; }

        [System.Text.Json.Serialization.JsonExtensionData]
        public Dictionary<string, System.Text.Json.JsonElement>? ExtensionData { get; set; }

        /// <summary>
        /// 目标ID
        /// 根据目标类型不同，代表不同的含义：
        /// - KillMonster: 怪物ID
        /// - CollectItem: 道具ID
        /// - TalkToNPC: NPC ID
        /// - CompleteDungeon: 副本ID
        /// - ReachLevel: 目标等级
        /// - EquipEnhance: 兼容旧任务的目标强化等级
        /// - BattleActivity: 活动目标标识
        /// - MapBattleCount / MapWinCount: 地图ID
        /// - AlchemySuccess / ForgeSuccess: 丹方ID / 图纸ID
        /// - CollectItem: 道具ID
        /// - PlantCrop / HarvestCrop: 作物模板ID
        /// </summary>
        public string TargetId { get; set; } = string.Empty;

        /// <summary>
        /// 目标数量
        /// </summary>
        public int TargetCount { get; set; }

        /// <summary>
        /// 目标附加值。
        /// 主要用于“多少件装备强化到多少等级”等双条件任务。
        /// </summary>
        public int TargetValue { get; set; }

        /// <summary>
        /// 目标描述
        /// 例如："击杀10只史莱姆"
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 战斗活动类型（仅当ObjectiveType为BattleActivity时使用）
        /// </summary>
        public BattleActivityType ActivityType { get; set; }

        /// <summary>
        /// 检查目标是否完成
        /// </summary>
        /// <param name="currentCount">当前数量</param>
        /// <returns>是否完成</returns>
        public bool IsCompleted(int currentCount)
        {
            return currentCount >= TargetCount;
        }
    }

    /// <summary>
    /// 任务事件。
    /// 由各业务系统上报，用于驱动任务进度增长。
    /// </summary>
    public class QuestObjectiveEvent
    {
        /// <summary>
        /// 对应的任务目标类型。
        /// </summary>
        public ObjectiveType ObjectiveType { get; set; }

        /// <summary>
        /// 事件目标ID。
        /// </summary>
        public string TargetId { get; set; } = string.Empty;

        /// <summary>
        /// 本次增加的进度值。
        /// </summary>
        public int Delta { get; set; } = 1;

        /// <summary>
        /// 当前状态值。
        /// 用于等级、强化等级等快照型同步。
        /// </summary>
        public int CurrentValue { get; set; }

        /// <summary>
        /// 战斗活动子类型。
        /// </summary>
        public BattleActivityType ActivityType { get; set; }
    }

    /// <summary>
    /// 任务进度类
    /// 记录玩家某个任务的完成进度
    /// </summary>
    public class QuestProgress
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public string QuestId { get; set; } = string.Empty;

        /// <summary>
        /// 任务状态
        /// </summary>
        public QuestStatus Status { get; set; } = QuestStatus.NotAccepted;

        /// <summary>
        /// 接取时间
        /// </summary>
        public DateTime AcceptTime { get; set; }

        /// <summary>
        /// 完成时间
        /// </summary>
        public DateTime? CompleteTime { get; set; }

        /// <summary>
        /// 提交时间
        /// </summary>
        public DateTime? SubmitTime { get; set; }

        /// <summary>
        /// 各目标的进度
        /// Key: 目标索引, Value: 当前进度
        /// </summary>
        private Dictionary<int, int> objectivesProgress = [];

        /// <summary>
        /// 当前阶段（多阶段任务使用）
        /// </summary>
        public int CurrentStage { get; set; } = 0;

        /// <summary>
        /// 获取目标进度
        /// </summary>
        /// <param name="objectiveIndex">目标索引</param>
        /// <returns>当前进度</returns>
        public int GetObjectiveProgress(int objectiveIndex)
        {
            return objectivesProgress.ContainsKey(objectiveIndex) ? objectivesProgress[objectiveIndex] : 0;
        }

        /// <summary>
        /// 设置目标进度
        /// </summary>
        /// <param name="objectiveIndex">目标索引</param>
        /// <param name="progress">进度值</param>
        public void SetObjectiveProgress(int objectiveIndex, int progress)
        {
            objectivesProgress[objectiveIndex] = progress;
        }

        /// <summary>
        /// 增加目标进度
        /// </summary>
        /// <param name="objectiveIndex">目标索引</param>
        /// <param name="delta">增量</param>
        /// <returns>增加后的进度</returns>
        public int AddObjectiveProgress(int objectiveIndex, int delta)
        {
            if (!objectivesProgress.ContainsKey(objectiveIndex))
            {
                objectivesProgress[objectiveIndex] = 0;
            }

            objectivesProgress[objectiveIndex] += delta;
            return objectivesProgress[objectiveIndex];
        }

        /// <summary>
        /// 检查是否超时（限时任务）
        /// </summary>
        /// <param name="timeLimit">时间限制（秒）</param>
        /// <returns>是否超时</returns>
        public bool IsTimeOut(int timeLimit)
        {
            if (timeLimit == 0)
            {
                return false;
            }

            var elapsed = DateTime.Now - AcceptTime;
            return elapsed.TotalSeconds > timeLimit;
        }

        /// <summary>
        /// 获取剩余时间（秒）
        /// </summary>
        /// <param name="timeLimit">时间限制（秒）</param>
        /// <returns>剩余时间，-1表示无限制</returns>
        public int GetRemainingTime(int timeLimit)
        {
            if (timeLimit == 0)
            {
                return -1;
            }

            var elapsed = DateTime.Now - AcceptTime;
            int remaining = timeLimit - (int)elapsed.TotalSeconds;

            return remaining > 0 ? remaining : 0;
        }

        /// <summary>
        /// 重置任务进度
        /// 用于日常/周常任务重置
        /// </summary>
        public void Reset()
        {
            Status = QuestStatus.NotAccepted;
            AcceptTime = DateTime.Now;
            CompleteTime = null;
            SubmitTime = null;
            objectivesProgress.Clear();
            CurrentStage = 0;
        }
    }
}
