namespace XXX.Application.DTOs
{
    /// <summary>
    /// 后台运行时配置域状态。
    /// </summary>
    public class AdminRuntimeConfigDomainDto
    {
        /// <summary>运行时域键。</summary>
        public string Domain { get; set; } = string.Empty;

        /// <summary>运行时域名称。</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>所属分组。</summary>
        public string Group { get; set; } = string.Empty;

        /// <summary>域说明文案。</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>当前是否支持后台手动刷新。</summary>
        public bool RefreshSupported { get; set; }

        /// <summary>当前版本号。</summary>
        public string? CurrentVersion { get; set; }

        /// <summary>最近一次应用时间。</summary>
        public DateTime? LastAppliedAt { get; set; }

        /// <summary>最近一次应用人。</summary>
        public string? LastAppliedBy { get; set; }

        /// <summary>最近一次刷新状态。</summary>
        public string LastRefreshStatus { get; set; } = string.Empty;

        /// <summary>最近一次刷新消息。</summary>
        public string? LastRefreshMessage { get; set; }

        /// <summary>累计刷新次数。</summary>
        public int RefreshCount { get; set; }

        /// <summary>创建时间。</summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>最后更新时间。</summary>
        public DateTime? LastUpdateTime { get; set; }
    }

    /// <summary>
    /// 后台运行时配置刷新结果。
    /// </summary>
    public class AdminRuntimeConfigRefreshResultDto
    {
        /// <summary>运行时域键。</summary>
        public string Domain { get; set; } = string.Empty;

        /// <summary>运行时域名称。</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>刷新是否成功。</summary>
        public bool Success { get; set; }

        /// <summary>刷新结果文案。</summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>刷新后的最新域状态。</summary>
        public AdminRuntimeConfigDomainDto Status { get; set; } = new();
    }
}
