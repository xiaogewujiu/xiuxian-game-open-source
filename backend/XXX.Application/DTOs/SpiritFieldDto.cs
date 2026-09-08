namespace XXX.Application.DTOs
{
    /// <summary>
    /// 灵田系统总览数据。
    /// </summary>
    public class SpiritFieldDto
    {
        /// <summary>
        /// 当前启用的催熟道具规则。
        /// </summary>
        public List<SpiritFieldSpeedUpItemDto> SpeedUpItems { get; set; } = [];

        /// <summary>
        /// 灵田等级。
        /// </summary>
        public int FieldLevel { get; set; }

        /// <summary>
        /// 当前已解锁地块数。
        /// </summary>
        public int UnlockedPlots { get; set; }

        /// <summary>
        /// 地块上限。
        /// </summary>
        public int MaxPlots { get; set; }

        /// <summary>
        /// 全局产量加成。
        /// </summary>
        public int GlobalYieldBonus { get; set; }

        /// <summary>
        /// 全局生长速度加成。
        /// </summary>
        public int GlobalGrowthSpeedBonus { get; set; }

        /// <summary>
        /// 今日播种次数。
        /// </summary>
        public int TodayPlantCount { get; set; }

        /// <summary>
        /// 今日收获次数。
        /// </summary>
        public int TodayHarvestCount { get; set; }

        /// <summary>
        /// 历史总播种次数。
        /// </summary>
        public int TotalPlantCount { get; set; }

        /// <summary>
        /// 历史总收获次数。
        /// </summary>
        public int TotalHarvestCount { get; set; }

        /// <summary>
        /// 地块列表。
        /// </summary>
        public List<PlotDto> Plots { get; set; } = [];
    }

    /// <summary>
    /// 灵田催熟道具展示规则。
    /// </summary>
    public class SpiritFieldSpeedUpItemDto
    {
        /// <summary>
        /// 道具模板编号。
        /// </summary>
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// 道具名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 单次催熟缩短时间，单位为秒。
        /// </summary>
        public int SpeedUpSeconds { get; set; }
    }

    /// <summary>
    /// 单块灵田地块的展示模型。
    /// </summary>
    public class PlotDto
    {
        /// <summary>
        /// 地块记录 ID。
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 地块编号。
        /// </summary>
        public int PlotNumber { get; set; }

        /// <summary>
        /// 地块等级。
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 地块状态。
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// 当前种植的作物模板 ID。
        /// </summary>
        public string? CropTemplateId { get; set; }

        /// <summary>
        /// 当前种植的作物名称。
        /// </summary>
        public string? CropName { get; set; }

        /// <summary>
        /// 播种时间。
        /// </summary>
        public DateTime? PlantTime { get; set; }

        /// <summary>
        /// 预计收获时间。
        /// </summary>
        public DateTime? ExpectedHarvestTime { get; set; }

        /// <summary>
        /// 当前进度百分比。
        /// </summary>
        public int ProgressPercent { get; set; }

        /// <summary>
        /// 当前是否可收获。
        /// </summary>
        public bool CanHarvest { get; set; }
    }

    /// <summary>
    /// 播种请求。
    /// </summary>
    public class PlantCropRequestDto
    {
        /// <summary>
        /// 目标地块编号。
        /// </summary>
        public int PlotNumber { get; set; }

        /// <summary>
        /// 作物模板 ID。
        /// </summary>
        public string CropTemplateId { get; set; } = string.Empty;
    }

    /// <summary>
    /// 收获请求。
    /// </summary>
    public class HarvestCropRequestDto
    {
        /// <summary>
        /// 要收获的地块编号。
        /// </summary>
        public int PlotNumber { get; set; }
    }

    /// <summary>
    /// 催熟请求。
    /// </summary>
    public class SpeedUpCropRequestDto
    {
        /// <summary>
        /// 要加速的地块编号。
        /// </summary>
        public int PlotNumber { get; set; }

        /// <summary>
        /// 使用的加速道具 ID。
        /// </summary>
        public string ItemId { get; set; } = string.Empty;
    }

    /// <summary>
    /// 作物模板的前端展示结构。
    /// </summary>
    public class CropTemplateDto
    {
        /// <summary>
        /// 作物模板 ID。
        /// </summary>
        public string TemplateId { get; set; } = string.Empty;

        /// <summary>
        /// 作物名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 作物类型。
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// 生长周期。
        /// </summary>
        public int GrowthCycle { get; set; }

        /// <summary>
        /// 基础产量。
        /// </summary>
        public int Yield { get; set; }

        /// <summary>
        /// 所需种子 ID。
        /// </summary>
        public string SeedId { get; set; } = string.Empty;

        /// <summary>
        /// 所需种子名称。
        /// </summary>
        public string SeedName { get; set; } = string.Empty;

        /// <summary>
        /// 所需种子数量。
        /// </summary>
        public int SeedAmount { get; set; }

        /// <summary>
        /// 产出物品 ID。
        /// </summary>
        public string OutputItemId { get; set; } = string.Empty;

        /// <summary>
        /// 产出物品名称。
        /// </summary>
        public string OutputName { get; set; } = string.Empty;

        /// <summary>
        /// 产出数量。
        /// </summary>
        public int OutputAmount { get; set; }

        /// <summary>
        /// 解锁等级。
        /// </summary>
        public int UnlockLevel { get; set; }
    }
}
