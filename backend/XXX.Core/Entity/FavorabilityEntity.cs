using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 玩家好感度实体
    /// 记录玩家之间的好感度数值及今日赠送信息
    /// </summary>
    [SugarTable("PlayerFavorability")]
    public class PlayerFavorabilityEntity : PlayerEntity
    {
        /// <summary>
        /// 目标玩家ID
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        public string TargetPlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 好感度数值
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int Value { get; set; }

        /// <summary>
        /// 今日赠送记录JSON
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? TodayGiftJson { get; set; }

        /// <summary>
        /// 最后赠送时间
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? LastGiftTime { get; set; }

        /// <summary>
        /// 今日组队战斗好感度记录JSON，格式：{"2026-06-15":true}
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? TodayPartyBattleJson { get; set; }
    }

    /// <summary>
    /// 好感度赠送日志实体
    /// 记录玩家赠送礼物的历史流水，保留物品名称快照
    /// </summary>
    [SugarTable("FavorabilityGiftLog")]
    public class FavorabilityGiftLogEntity : PlayerEntity
    {
        /// <summary>
        /// 目标玩家ID
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        public string TargetPlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 赠送物品ID（组队战斗来源时为 null）
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string? ItemId { get; set; }

        /// <summary>
        /// 赠送物品名称（历史快照，即使物品改名也能保留记录）
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false)]
        public string ItemName { get; set; } = string.Empty;

        /// <summary>
        /// 好感度变化值
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int FavorabilityChange { get; set; }

        /// <summary>
        /// 赠送时间
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime GiftTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 好感度等级配置实体
    /// 定义好感度各等级的名称、数值区间、颜色和排序
    /// </summary>
    [SugarTable("FavorabilityLevelConfig")]
    public class FavorabilityLevelConfigEntity
    {
        /// <summary>
        /// 配置ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 等级
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int Level { get; set; }

        /// <summary>
        /// 等级名称
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 最低好感度值
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int MinValue { get; set; }

        /// <summary>
        /// 最高好感度值
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int MaxValue { get; set; }

        /// <summary>
        /// 等级颜色（十六进制）
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false)]
        public string Color { get; set; } = "#CCCCCC";

        /// <summary>
        /// 排序权重
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int SortOrder { get; set; }

        /// <summary>
        /// 等级奖励JSON
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? RewardJson { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新时间
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 好感度成就领取记录实体
    /// </summary>
    [SugarTable("FavorabilityAchievement")]
    public class FavorabilityAchievementEntity : PlayerEntity
    {
        /// <summary>
        /// 成就ID
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        public string AchievementId { get; set; } = string.Empty;

        /// <summary>
        /// 领取时间
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime ClaimedAt { get; set; } = DateTime.UtcNow;
    }
}
