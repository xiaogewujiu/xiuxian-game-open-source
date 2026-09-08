using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 作物模板实体
    /// 对应数据库作物模板表
    /// </summary>
    [SugarTable("CropTemplates")]
    public class CropTemplateEntity
    {
        /// <summary>
        /// 模板ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, Length = 50, IsNullable = false)]
        public string TemplateId { get; set; } = string.Empty;

        /// <summary>
        /// 作物名称
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 作物描述
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string? Description { get; set; }

        /// <summary>
        /// 作物类型
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public CropType Type { get; set; }

        /// <summary>
        /// 生长周期（秒）
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "3600")]
        public int GrowthCycle { get; set; } = 3600;

        /// <summary>
        /// 产量
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public int Yield { get; set; } = 1;

        /// <summary>
        /// 种子ID
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        public string SeedId { get; set; } = string.Empty;

        /// <summary>
        /// 种子数量
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public int SeedAmount { get; set; } = 1;

        /// <summary>
        /// 产出物品ID
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        public string OutputItemId { get; set; } = string.Empty;

        /// <summary>
        /// 产出数量
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public int OutputAmount { get; set; } = 1;

        /// <summary>
        /// 最小品质
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public int MinQuality { get; set; } = 1;

        /// <summary>
        /// 最大品质
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public int MaxQuality { get; set; } = 1;

        /// <summary>
        /// 解锁等级
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public int UnlockLevel { get; set; } = 1;

        /// <summary>
        /// 内置种子键。
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string? SeedKey { get; set; }

        /// <summary>
        /// 是否为内置模板。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public bool IsBuiltIn { get; set; } = false;

        /// <summary>
        /// 内置版本号。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string? BuiltInVersion { get; set; }

        /// <summary>
        /// 最后更新时间。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 灵田地块实体
    /// 对应数据库灵田地块表
    /// </summary>
    [SugarTable("SpiritFieldPlots")]
    public class SpiritFieldPlotEntity
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        /// <summary>
        /// 玩家ID
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false, IndexGroupNameList = new[] { "idx_player" })]
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 地块编号（1-9）
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public int PlotNumber { get; set; } = 1;

        /// <summary>
        /// 地块等级
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public int Level { get; set; } = 1;

        /// <summary>
        /// 地块状态
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public PlotStatus Status { get; set; } = PlotStatus.Empty;

        /// <summary>
        /// 种植的作物模板ID
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true)]
        public string? CropTemplateId { get; set; }

        /// <summary>
        /// 种植时间
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? PlantTime { get; set; }

        /// <summary>
        /// 预计成熟时间
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? ExpectedHarvestTime { get; set; }

        /// <summary>
        /// 实际收获时间
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? ActualHarvestTime { get; set; }

        /// <summary>
        /// 加速次数
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int SpeedUpCount { get; set; } = 0;

        /// <summary>
        /// 加速总时间（秒）
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int SpeedUpDuration { get; set; } = 0;

        /// <summary>
        /// 产量加成百分比
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int YieldBonusPercent { get; set; } = 0;

        /// <summary>
        /// 最后更新时间
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 灵田系统数据实体
    /// 对应数据库灵田系统表
    /// </summary>
    [SugarTable("SpiritFieldSystems")]
    public class SpiritFieldSystemEntity
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        /// <summary>
        /// 玩家ID
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false, IndexGroupNameList = new[] { "idx_player" })]
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 灵田等级
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public int FieldLevel { get; set; } = 1;

        /// <summary>
        /// 已解锁地块数量
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public int UnlockedPlots { get; set; } = 1;

        /// <summary>
        /// 最大地块数量
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "9")]
        public int MaxPlots { get; set; } = 9;

        /// <summary>
        /// 全局产量加成百分比
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int GlobalYieldBonus { get; set; } = 0;

        /// <summary>
        /// 全局生长速度加成百分比
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int GlobalGrowthSpeedBonus { get; set; } = 0;

        /// <summary>
        /// 今日已种植次数
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int TodayPlantCount { get; set; } = 0;

        /// <summary>
        /// 今日已收获次数
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int TodayHarvestCount { get; set; } = 0;

        /// <summary>
        /// 今日统计重置时间
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime DailyResetTime { get; set; } = DateTime.Now.Date;

        /// <summary>
        /// 累计种植次数
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int TotalPlantCount { get; set; } = 0;

        /// <summary>
        /// 累计收获次数
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int TotalHarvestCount { get; set; } = 0;

        /// <summary>
        /// 最后更新时间
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }
}
