namespace XXX.Achievement
{
    /// <summary>
    /// 成就类型枚举
    /// 定义不同类型的成就
    /// </summary>
    public enum AchievementType
    {
        /// <summary>
        /// 等级成就 - 达到指定等级
        /// </summary>
        Level,

        /// <summary>
        /// 战斗成就 - 战斗相关统计
        /// </summary>
        Battle,

        /// <summary>
        /// 装备成就 - 装备强化、收集
        /// </summary>
        Equipment,

        /// <summary>
        /// 收集成就 - 收集道具/装备
        /// </summary>
        Collection,

        /// <summary>
        /// 社交成就 - 好友、公会
        /// </summary>
        Social,

        /// <summary>
        /// 活动成就 - 参与特定活动
        /// </summary>
        Activity,

        /// <summary>
        /// 特殊成就 - 隐藏成就、挑战
        /// </summary>
        Special
    }

    /// <summary>
    /// 成就难度枚举
    /// </summary>
    public enum AchievementDifficulty
    {
        /// <summary>
        /// 简单 - 10点
        /// </summary>
        Easy = 10,

        /// <summary>
        /// 普通 - 25点
        /// </summary>
        Normal = 25,

        /// <summary>
        /// 困难 - 50点
        /// </summary>
        Hard = 50,

        /// <summary>
        /// 极限 - 100点
        /// </summary>
        Extreme = 100
    }

    /// <summary>
    /// 成就状态枚举
    /// </summary>
    public enum AchievementStatus
    {
        /// <summary>
        /// 未开始
        /// </summary>
        NotStarted,

        /// <summary>
        /// 进行中
        /// </summary>
        InProgress,

        /// <summary>
        /// 已完成（可领取奖励）
        /// </summary>
        Completed,

        /// <summary>
        /// 已领取奖励
        /// </summary>
        Claimed
    }

    /// <summary>
    /// 成就要求类型枚举
    /// </summary>
    public enum AchievementRequirementType
    {
        /// <summary>
        /// 达到等级
        /// </summary>
        ReachLevel,

        /// <summary>
        /// 总击杀数
        /// </summary>
        TotalKills,

        /// <summary>
        /// 总胜利场次
        /// </summary>
        TotalWins,

        /// <summary>
        /// 最大强化等级
        /// </summary>
        MaxEnhanceLevel,

        /// <summary>
        /// 装备收集数量
        /// </summary>
        EquipmentCollection,

        /// <summary>
        /// 道具收集数量
        /// </summary>
        ItemCollection,

        /// <summary>
        /// 累计获得金币
        /// </summary>
        TotalGoldEarned,

        /// <summary>
        /// 累计消费钻石
        /// </summary>
        TotalSpiritStoneSpent,

        /// <summary>
        /// 累计登录天数
        /// </summary>
        LoginDays,

        /// <summary>
        /// 连续登录天数
        /// </summary>
        ContinuousLoginDays,

        /// <summary>
        /// 完成任务数量
        /// </summary>
        CompleteQuests,

        /// <summary>
        /// 完成副本次数
        /// </summary>
        CompleteDungeons,

        /// <summary>
        /// 无伤战斗
        /// </summary>
        BattleWithoutDamage,

        /// <summary>
        /// 最大连击数
        /// </summary>
        MaxCombo,

        /// <summary>
        /// 累计造成伤害
        /// </summary>
        TotalDamageDealt,

        /// <summary>
        /// 累计获得经验
        /// </summary>
        TotalExpEarned,

        /// <summary>
        /// 累计受到伤害
        /// </summary>
        TotalDamageTaken,

        /// <summary>
        /// 炼丹成功次数
        /// </summary>
        AlchemySuccessCount,

        /// <summary>
        /// 炼丹失败次数
        /// </summary>
        AlchemyFailureCount,

        /// <summary>
        /// 锻造成功次数
        /// </summary>
        ForgeSuccessCount,

        /// <summary>
        /// 锻造失败次数
        /// </summary>
        ForgeFailureCount,

        /// <summary>
        /// 强化成功次数
        /// </summary>
        EnhanceSuccessCount,

        /// <summary>
        /// 强化失败次数
        /// </summary>
        EnhanceFailureCount,

        /// <summary>
        /// 种植次数
        /// </summary>
        PlantCount,

        /// <summary>
        /// 收获次数
        /// </summary>
        HarvestCount,

        /// <summary>
        /// 普通副本胜利次数
        /// </summary>
        DungeonWinCount,

        /// <summary>
        /// 指定野怪击杀次数
        /// </summary>
        MonsterKillCount,

        /// <summary>
        /// 指定技能使用次数
        /// </summary>
        SkillUseCount,

        /// <summary>
        /// 累计消耗蓝量
        /// </summary>
        TotalMpConsumed,

        /// <summary>
        /// 暴击次数
        /// </summary>
        CriticalHitCount,

        /// <summary>
        /// 死亡次数
        /// </summary>
        DeathCount,

        /// <summary>
        /// 闪避次数
        /// </summary>
        DodgeCount,

        /// <summary>
        /// 指定技能累计造成伤害
        /// </summary>
        SkillDamageDealt
    }

    /// <summary>
    /// 成就配置类
    /// 定义一个成就的完整配置
    /// </summary>
    public class AchievementConfig
    {
        /// <summary>
        /// 成就唯一ID
        /// </summary>
        public string AchievementId { get; set; } = string.Empty;

        /// <summary>
        /// 成就名称
        /// </summary>
        public string AchievementName { get; set; } = string.Empty;

        /// <summary>
        /// 成就类型
        /// </summary>
        public AchievementType AchievementType { get; set; }

        /// <summary>
        /// 成就难度
        /// </summary>
        public AchievementDifficulty Difficulty { get; set; }

        /// <summary>
        /// 成就描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 成就图标
        /// </summary>
        public string Icon { get; set; } = string.Empty;

        /// <summary>
        /// 成就要求列表
        /// </summary>
        public List<AchievementRequirement> Requirements { get; set; } = [];

        /// <summary>
        /// 成就奖励
        /// </summary>
        public AchievementReward Rewards { get; set; } = new AchievementReward();

        /// <summary>
        /// 成就点数
        /// </summary>
        public int Points { get; set; }

        /// <summary>
        /// 是否隐藏成就
        /// </summary>
        public bool IsHidden { get; set; } = false;

        /// <summary>
        /// 前置成就ID列表
        /// </summary>
        public List<string> PreAchievementIds { get; set; } = [];

        /// <summary>
        /// 成就分类（用于UI分组）
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// 排序（用于UI显示顺序）
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 检查是否所有要求都已完成
        /// </summary>
        /// <param name="progress">成就进度</param>
        /// <returns>是否全部完成</returns>
        public bool IsAllRequirementsCompleted(AchievementProgress progress)
        {
            if (Requirements.Count == 0)
            {
                return true;
            }

            for (int i = 0; i < Requirements.Count; i++)
            {
                long currentProgress = progress.GetProgressValue(i);
                if (currentProgress < Requirements[i].TargetValue)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 获取成就进度文本
        /// </summary>
        /// <param name="progress">成就进度</param>
        /// <returns>进度文本</returns>
        public string GetProgressText(AchievementProgress progress)
        {
            if (Requirements.Count == 0)
            {
                return "无要求";
            }

            var texts = new List<string>();
            for (int i = 0; i < Requirements.Count; i++)
            {
                long current = progress.GetProgressValue(i);
                long target = Requirements[i].TargetValue;
                bool isCompleted = current >= target;

                string prefix = isCompleted ? "✓" : "○";
                texts.Add($"{prefix} {Requirements[i].Description} ({current}/{target})");
            }

            return string.Join("\n", texts);
        }

        /// <summary>
        /// 获取完成进度百分比
        /// </summary>
        /// <param name="progress">成就进度</param>
        /// <returns>百分比(0-100)</returns>
        public float GetProgressPercent(AchievementProgress progress)
        {
            if (Requirements.Count == 0)
            {
                return 100f;
            }

            float totalProgress = 0f;
            for (int i = 0; i < Requirements.Count; i++)
            {
                long current = progress.GetProgressValue(i);
                long target = Requirements[i].TargetValue;
                totalProgress += (float)current / target;
            }

            return Math.Min(100f, totalProgress / Requirements.Count * 100f);
        }

        /// <summary>
        /// 检查是否有前置成就
        /// </summary>
        /// <param name="completedAchievementIds">已完成成就ID集合</param>
        /// <returns>是否满足前置条件</returns>
        public bool CheckPreAchievements(HashSet<string> completedAchievementIds)
        {
            if (PreAchievementIds.Count == 0)
            {
                return true;
            }

            foreach (var preId in PreAchievementIds)
            {
                if (!completedAchievementIds.Contains(preId))
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// 成就要求类
    /// 定义成就的单个要求
    /// </summary>
    public class AchievementRequirement
    {
        /// <summary>
        /// 要求类型
        /// </summary>
        public AchievementRequirementType RequirementType { get; set; }

        /// <summary>
        /// 目标对象ID。
        /// 全局累计项留空；指定对象项可填怪物ID、技能ID、作物ID、副本ID等。
        /// </summary>
        public string TargetId { get; set; } = string.Empty;

        /// <summary>
        /// 目标值
        /// </summary>
        public long TargetValue { get; set; }

        /// <summary>
        /// 目标描述
        /// 例如："达到等级50"
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 检查要求是否完成
        /// </summary>
        /// <param name="currentValue">当前值</param>
        /// <returns>是否完成</returns>
        public bool IsCompleted(long currentValue)
        {
            return currentValue >= TargetValue;
        }
    }

    /// <summary>
    /// 成就进度类
    /// 记录玩家某个成就的完成进度
    /// </summary>
    public class AchievementProgress
    {
        /// <summary>
        /// 成就ID
        /// </summary>
        public string AchievementId { get; set; } = string.Empty;

        /// <summary>
        /// 成就状态
        /// </summary>
        public AchievementStatus Status { get; set; } = AchievementStatus.NotStarted;

        /// <summary>
        /// 各要求的进度值
        /// Key: 要求索引, Value: 当前进度值
        /// </summary>
        private Dictionary<int, long> progressValues = [];

        /// <summary>
        /// 完成时间
        /// </summary>
        public DateTime? CompleteTime { get; set; }

        /// <summary>
        /// 领取奖励时间
        /// </summary>
        public DateTime? ClaimTime { get; set; }

        /// <summary>
        /// 获取进度值
        /// </summary>
        /// <param name="requirementIndex">要求索引</param>
        /// <returns>当前进度值</returns>
        public long GetProgressValue(int requirementIndex)
        {
            return progressValues.ContainsKey(requirementIndex) ? progressValues[requirementIndex] : 0L;
        }

        /// <summary>
        /// 设置进度值
        /// </summary>
        /// <param name="requirementIndex">要求索引</param>
        /// <param name="value">进度值</param>
        public void SetProgressValue(int requirementIndex, long value)
        {
            progressValues[requirementIndex] = value;
        }

        /// <summary>
        /// 增加进度值
        /// </summary>
        /// <param name="requirementIndex">要求索引</param>
        /// <param name="delta">增量</param>
        /// <returns>增加后的进度值</returns>
        public long AddProgressValue(int requirementIndex, long delta)
        {
            if (!progressValues.ContainsKey(requirementIndex))
            {
                progressValues[requirementIndex] = 0L;
            }

            progressValues[requirementIndex] += delta;
            return progressValues[requirementIndex];
        }

        /// <summary>
        /// 检查是否已领取奖励
        /// </summary>
        /// <returns>是否已领取</returns>
        public bool IsClaimed()
        {
            return Status == AchievementStatus.Claimed;
        }

        /// <summary>
        /// 检查是否已完成
        /// </summary>
        /// <returns>是否已完成</returns>
        public bool IsCompleted()
        {
            return Status == AchievementStatus.Completed || Status == AchievementStatus.Claimed;
        }
    }

    /// <summary>
    /// 成就统计事件。
    /// </summary>
    public class AchievementRequirementEvent
    {
        /// <summary>
        /// 成就条件类型。
        /// </summary>
        public AchievementRequirementType RequirementType { get; set; }

        /// <summary>
        /// 目标对象ID。
        /// </summary>
        public string TargetId { get; set; } = string.Empty;

        /// <summary>
        /// 本次增量。
        /// </summary>
        public long Delta { get; set; }

        /// <summary>
        /// 当前快照值。
        /// </summary>
        public long CurrentValue { get; set; }
    }
}
