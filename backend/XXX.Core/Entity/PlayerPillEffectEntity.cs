using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 玩家丹药效果记录。
    /// </summary>
    [SugarTable("PlayerPillEffects")]
    public class PlayerPillEffectEntity
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        [SugarColumn(Length = 100)]
        public string PlayerId { get; set; } = string.Empty;

        [SugarColumn(Length = 100)]
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// 丹药效果类型，对应 ItemPillEffectType 枚举值。
        /// </summary>
        [SugarColumn]
        public int EffectType { get; set; }

        /// <summary>
        /// 加成类型名称，如"物攻"、"突破概率"等，用于展示。
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true)]
        public string? BonusType { get; set; }

        /// <summary>
        /// 单次加成数值。
        /// 限时突破概率丹药多次使用时此值会累加；限时属性丹药不累加。
        /// </summary>
        [SugarColumn]
        public double BonusValue { get; set; }

        /// <summary>
        /// 已使用次数。
        /// </summary>
        [SugarColumn]
        public int UsageCount { get; set; }

        /// <summary>
        /// 是否为限时效果。
        /// </summary>
        [SugarColumn]
        public bool IsTemporary { get; set; }

        /// <summary>
        /// 过期时间。限时效果非 null，永久效果为 null。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? ExpiresAt { get; set; }

        [SugarColumn]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [SugarColumn]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }
}
