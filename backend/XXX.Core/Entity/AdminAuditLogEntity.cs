using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 管理后台审计日志实体。
    /// 用于记录后台接口写操作，并保存请求、响应以及资源前后快照。
    /// </summary>
    [SugarTable("AdminAuditLogs")]
    public class AdminAuditLogEntity
    {
        /// <summary>
        /// 日志编号。
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, Length = 50, IsNullable = false)]
        public string LogId { get; set; } = string.Empty;

        /// <summary>
        /// 操作人编号。
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true)]
        public string? OperatorId { get; set; }

        /// <summary>
        /// 操作人名称。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string? OperatorName { get; set; }

        /// <summary>
        /// 操作人角色。
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true)]
        public string? OperatorRole { get; set; }

        /// <summary>
        /// HTTP 方法。
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false)]
        public string HttpMethod { get; set; } = string.Empty;

        /// <summary>
        /// 请求路径。
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = false)]
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// 资源键。
        /// 例如 maps、monsters、players。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string? ResourceKey { get; set; }

        /// <summary>
        /// 目标编号。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string? TargetId { get; set; }

        /// <summary>
        /// 请求体快照。
        /// 会对密码、令牌等敏感字段做脱敏。
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? RequestJson { get; set; }

        /// <summary>
        /// 响应体快照。
        /// 会对密码、令牌等敏感字段做脱敏。
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? ResponseJson { get; set; }

        /// <summary>
        /// 操作前资源快照。
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? BeforeJson { get; set; }

        /// <summary>
        /// 操作后资源快照。
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? AfterJson { get; set; }

        /// <summary>
        /// 前后差异快照。
        /// 当前记录变更字段路径以及前后值，便于快速审计。
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? DiffJson { get; set; }

        /// <summary>
        /// 响应状态码。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int StatusCode { get; set; }

        /// <summary>
        /// 是否成功。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public bool Success { get; set; }

        /// <summary>
        /// 错误信息。
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true)]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 客户端 IP。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string? IpAddress { get; set; }

        /// <summary>
        /// 记录时间。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime CreateTime { get; set; } = DateTime.Now;
    }
}
