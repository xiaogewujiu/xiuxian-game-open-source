namespace XXX.Application.DTOs
{
    /// <summary>
    /// 后台灵田系统列表项。
    /// </summary>
    public class AdminSpiritFieldListItemDto
    {
        /// <summary>玩家编号。</summary>
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>玩家名称。</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>玩家账号。</summary>
        public string Account { get; set; } = string.Empty;

        /// <summary>灵田等级。</summary>
        public int FieldLevel { get; set; }

        /// <summary>已解锁地块数。</summary>
        public int UnlockedPlots { get; set; }

        /// <summary>最大地块数。</summary>
        public int MaxPlots { get; set; }

        /// <summary>全局产量加成百分比。</summary>
        public int GlobalYieldBonus { get; set; }

        /// <summary>全局生长速度加成百分比。</summary>
        public int GlobalGrowthSpeedBonus { get; set; }

        /// <summary>累计种植次数。</summary>
        public int TotalPlantCount { get; set; }

        /// <summary>累计收获次数。</summary>
        public int TotalHarvestCount { get; set; }
    }

    /// <summary>
    /// 后台灵田系统详情。
    /// </summary>
    public class AdminSpiritFieldDetailDto
    {
        /// <summary>玩家编号。</summary>
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>玩家名称。</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>玩家账号。</summary>
        public string Account { get; set; } = string.Empty;

        /// <summary>灵田等级。</summary>
        public int FieldLevel { get; set; }

        /// <summary>已解锁地块数。</summary>
        public int UnlockedPlots { get; set; }

        /// <summary>最大地块数。</summary>
        public int MaxPlots { get; set; }

        /// <summary>全局产量加成百分比。</summary>
        public int GlobalYieldBonus { get; set; }

        /// <summary>全局生长速度加成百分比。</summary>
        public int GlobalGrowthSpeedBonus { get; set; }

        /// <summary>今日种植次数。</summary>
        public int TodayPlantCount { get; set; }

        /// <summary>今日收获次数。</summary>
        public int TodayHarvestCount { get; set; }

        /// <summary>累计种植次数。</summary>
        public int TotalPlantCount { get; set; }

        /// <summary>累计收获次数。</summary>
        public int TotalHarvestCount { get; set; }

        /// <summary>地块明细列表。</summary>
        public List<AdminSpiritFieldPlotDto> Plots { get; set; } = [];
    }

    /// <summary>
    /// 后台灵田地块详情。
    /// </summary>
    public class AdminSpiritFieldPlotDto
    {
        /// <summary>地块记录主键。</summary>
        public long Id { get; set; }

        /// <summary>地块编号。</summary>
        public int PlotNumber { get; set; }

        /// <summary>地块等级。</summary>
        public int Level { get; set; }

        /// <summary>当前状态。</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>种植中的作物模板编号。</summary>
        public string? CropTemplateId { get; set; }

        /// <summary>种植中的作物名称。</summary>
        public string? CropName { get; set; }

        /// <summary>种植时间。</summary>
        public DateTime? PlantTime { get; set; }

        /// <summary>预计收获时间。</summary>
        public DateTime? ExpectedHarvestTime { get; set; }

        /// <summary>累计催熟次数。</summary>
        public int SpeedUpCount { get; set; }

        /// <summary>当前地块产量加成百分比。</summary>
        public int YieldBonusPercent { get; set; }
    }
}
