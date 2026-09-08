namespace XXX.Application.DTOs
{
    /// <summary>
    /// 后台审计日志列表项。
    /// </summary>
    public class AdminAuditLogListItemDto
    {
        /// <summary>
        /// 日志编号。
        /// </summary>
        public string LogId { get; set; } = string.Empty;

        /// <summary>
        /// 操作人名称。
        /// </summary>
        public string? OperatorName { get; set; }

        /// <summary>
        /// 操作人角色。
        /// </summary>
        public string? OperatorRole { get; set; }

        /// <summary>
        /// 请求方法。
        /// </summary>
        public string HttpMethod { get; set; } = string.Empty;

        /// <summary>
        /// 请求路径。
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// 资源键。
        /// </summary>
        public string? ResourceKey { get; set; }

        /// <summary>
        /// 目标编号。
        /// </summary>
        public string? TargetId { get; set; }

        /// <summary>
        /// 是否成功。
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 状态码。
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// 错误信息。
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 记录时间。
        /// </summary>
        public DateTime CreateTime { get; set; }
    }

    /// <summary>
    /// 后台审计日志详情。
    /// 在列表摘要基础上补充请求、响应以及前后资源快照。
    /// </summary>
    public class AdminAuditLogDetailDto : AdminAuditLogListItemDto
    {
        /// <summary>
        /// 操作人编号。
        /// </summary>
        public string? OperatorId { get; set; }

        /// <summary>
        /// 客户端 IP。
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// 请求体快照。
        /// </summary>
        public string? RequestJson { get; set; }

        /// <summary>
        /// 响应体快照。
        /// </summary>
        public string? ResponseJson { get; set; }

        /// <summary>
        /// 操作前快照。
        /// </summary>
        public string? BeforeJson { get; set; }

        /// <summary>
        /// 操作后快照。
        /// </summary>
        public string? AfterJson { get; set; }

        /// <summary>
        /// 差异快照。
        /// </summary>
        public string? DiffJson { get; set; }
    }
}
