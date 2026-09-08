using XXX.Entity;

namespace XXX.Application.DTOs
{
    /// <summary>
    /// 后台世界 Boss 列表项。
    /// </summary>
    public class AdminWorldBossTemplateListItemDto
    {
        /// <summary>Boss 模板编号。</summary>
        public string BossId { get; set; } = string.Empty;
        /// <summary>Boss 名称。</summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>关联怪物模板编号。</summary>
        public string MonsterTemplateId { get; set; } = string.Empty;
        /// <summary>当前模板是否启用。</summary>
        public bool IsEnabled { get; set; }
        /// <summary>随机抽取权重。</summary>
        public int Weight { get; set; }
        /// <summary>实例持续时长，单位分钟。</summary>
        public int DurationMinutes { get; set; }
        /// <summary>后台展示排序。</summary>
        public int SortOrder { get; set; }
        /// <summary>最近一次编辑时间。</summary>
        public DateTime? LastUpdateTime { get; set; }
    }

    /// <summary>
    /// 后台世界 Boss 详情。
    /// </summary>
    public class AdminWorldBossTemplateDetailDto
    {
        /// <summary>Boss 模板编号。</summary>
        public string BossId { get; set; } = string.Empty;
        /// <summary>Boss 名称。</summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>关联怪物模板编号。</summary>
        public string MonsterTemplateId { get; set; } = string.Empty;
        /// <summary>Boss 画像路径。</summary>
        public string? PortraitPath { get; set; }
        /// <summary>当前模板是否启用。</summary>
        public bool IsEnabled { get; set; }
        /// <summary>随机抽取权重。</summary>
        public int Weight { get; set; }
        /// <summary>实例持续时长，单位分钟。</summary>
        public int DurationMinutes { get; set; }
        /// <summary>Boss 生成公告文案。</summary>
        public string? NoticeText { get; set; }
        /// <summary>参与奖励最低伤害门槛。</summary>
        public long ParticipationMinDamage { get; set; }
        /// <summary>参与奖励经验。</summary>
        public long ParticipationRewardExp { get; set; }
        /// <summary>参与奖励金币。</summary>
        public long ParticipationRewardGold { get; set; }
        /// <summary>参与奖励灵石。</summary>
        public long ParticipationRewardSpiritStone { get; set; }
        /// <summary>第 1 名额外奖励经验。</summary>
        public long Rank1RewardExp { get; set; }
        /// <summary>第 1 名额外奖励金币。</summary>
        public long Rank1RewardGold { get; set; }
        /// <summary>第 1 名额外奖励灵石。</summary>
        public long Rank1RewardSpiritStone { get; set; }
        /// <summary>第 2 名额外奖励经验。</summary>
        public long Rank2RewardExp { get; set; }
        /// <summary>第 2 名额外奖励金币。</summary>
        public long Rank2RewardGold { get; set; }
        /// <summary>第 2 名额外奖励灵石。</summary>
        public long Rank2RewardSpiritStone { get; set; }
        /// <summary>第 3 名额外奖励经验。</summary>
        public long Rank3RewardExp { get; set; }
        /// <summary>第 3 名额外奖励金币。</summary>
        public long Rank3RewardGold { get; set; }
        /// <summary>第 3 名额外奖励灵石。</summary>
        public long Rank3RewardSpiritStone { get; set; }
        /// <summary>后台展示排序。</summary>
        public int SortOrder { get; set; }
        /// <summary>最近一次编辑时间。</summary>
        public DateTime? LastUpdateTime { get; set; }
    }

    /// <summary>
    /// 世界 Boss 排期配置。
    /// </summary>
    public class AdminWorldBossScheduleDto
    {
        /// <summary>排期记录编号。</summary>
        public string ScheduleId { get; set; } = "default";
        /// <summary>每日刷新时间，格式为 HH:mm。</summary>
        public string SpawnTimeText { get; set; } = "11:00";
        /// <summary>排期使用的时区标识。</summary>
        public string TimeZoneId { get; set; } = "China Standard Time";
        /// <summary>Boss 选择模式。</summary>
        public WorldBossSelectionMode SelectionMode { get; set; } = WorldBossSelectionMode.Random;
        /// <summary>当前排期是否启用。</summary>
        public bool IsEnabled { get; set; } = true;
        /// <summary>最近一次编辑时间。</summary>
        public DateTime? LastUpdateTime { get; set; }
    }

    /// <summary>
    /// 后台世界 Boss 运行时总览。
    /// </summary>
    public class AdminWorldBossRuntimeDto
    {
        /// <summary>当前是否存在活动中的实例。</summary>
        public bool HasActiveInstance { get; set; }
        /// <summary>当前实例面板总览。</summary>
        public WorldBossCurrentDto? Current { get; set; }
        /// <summary>伤害前十排行。</summary>
        public List<WorldBossRankingEntryDto> RankingTop10 { get; set; } = [];
        /// <summary>最近战斗日志。</summary>
        public List<WorldBossLogDto> RecentLogs { get; set; } = [];
    }
}
