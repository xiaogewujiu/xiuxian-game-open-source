using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 运行时配置域版本与刷新状态。
    /// </summary>
    [SugarTable("SystemConfigVersions")]
    public class SystemConfigVersionEntity
    {
        /// <summary>
        /// 配置域唯一键。
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "配置域", Length = 50)]
        public string ConfigDomain { get; set; } = string.Empty;

        /// <summary>
        /// 当前版本号。
        /// </summary>
        [SugarColumn(ColumnDescription = "当前版本", Length = 100, IsNullable = true)]
        public string? CurrentVersion { get; set; }

        /// <summary>
        /// 最近一次应用时间。
        /// </summary>
        [SugarColumn(ColumnDescription = "最后应用时间", IsNullable = true)]
        public DateTime? LastAppliedAt { get; set; }

        /// <summary>
        /// 最近一次应用人。
        /// </summary>
        [SugarColumn(ColumnDescription = "最后应用人", Length = 100, IsNullable = true)]
        public string? LastAppliedBy { get; set; }

        /// <summary>
        /// 最近一次刷新状态。
        /// </summary>
        [SugarColumn(ColumnDescription = "刷新状态", Length = 50)]
        public string LastRefreshStatus { get; set; } = "未执行";

        /// <summary>
        /// 最近一次刷新消息。
        /// </summary>
        [SugarColumn(ColumnDescription = "刷新消息", Length = 500, IsNullable = true)]
        public string? LastRefreshMessage { get; set; }

        /// <summary>
        /// 累计刷新次数。
        /// </summary>
        [SugarColumn(ColumnDescription = "刷新次数")]
        public int RefreshCount { get; set; } = 0;

        /// <summary>
        /// 创建时间。
        /// </summary>
        [SugarColumn(ColumnDescription = "创建时间")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 最后更新时间。
        /// </summary>
        [SugarColumn(ColumnDescription = "最后更新时间")]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }
}
