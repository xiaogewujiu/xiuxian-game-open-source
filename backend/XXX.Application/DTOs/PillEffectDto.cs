namespace XXX.Application.DTOs
{
    /// <summary>
    /// 丹药效果总览。
    /// </summary>
    public class PillEffectsDto
    {
        public List<PermanentPillEffectDto> PermanentEffects { get; set; } = [];
        public List<TemporaryPillEffectDto> TemporaryEffects { get; set; } = [];
    }

    /// <summary>
    /// 永久生效丹药。
    /// </summary>
    public class PermanentPillEffectDto
    {
        public string ItemId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Rank { get; set; } = string.Empty;
        public int EffectType { get; set; }
        public string BonusType { get; set; } = string.Empty;
        public double CumulativeValue { get; set; }
        public int UsageCount { get; set; }
    }

    /// <summary>
    /// 当前限时生效丹药。
    /// </summary>
    public class TemporaryPillEffectDto
    {
        public string ItemId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Rank { get; set; } = string.Empty;
        public int EffectType { get; set; }
        public string BonusType { get; set; } = string.Empty;
        public double CurrentValue { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
