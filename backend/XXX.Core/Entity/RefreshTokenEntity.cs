using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 刷新令牌实体
    /// 用于存储JWT刷新令牌，支持令牌持久化和撤销管理
    /// </summary>
    [SugarTable("refresh_token")]
    public class RefreshTokenEntity
    {
        /// <summary>
        /// 令牌唯一标识符
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "令牌ID")]
        public string GID { get; set; } = string.Empty;

        /// <summary>
        /// 关联的用户ID
        /// </summary>
        [SugarColumn(ColumnDescription = "用户ID")]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// 刷新令牌值
        /// </summary>
        [SugarColumn(ColumnDescription = "刷新令牌", Length = 200)]
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// 令牌过期时间
        /// </summary>
        [SugarColumn(ColumnDescription = "过期时间")]
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// 令牌创建时间
        /// </summary>
        [SugarColumn(ColumnDescription = "创建时间")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 是否已被撤销
        /// </summary>
        [SugarColumn(ColumnDescription = "是否已撤销")]
        public bool IsRevoked { get; set; } = false;

        /// <summary>
        /// 撤销时间
        /// </summary>
        [SugarColumn(ColumnDescription = "撤销时间", IsNullable = true)]
        public DateTime? RevokedAt { get; set; }

        /// <summary>
        /// 创建令牌时的设备信息
        /// </summary>
        [SugarColumn(ColumnDescription = "设备信息", Length = 200, IsNullable = true)]
        public string? DeviceInfo { get; set; }

        /// <summary>
        /// 创建令牌时的IP地址
        /// </summary>
        [SugarColumn(ColumnDescription = "IP地址", Length = 50, IsNullable = true)]
        public string? IpAddress { get; set; }
    }

    /// <summary>
    /// 令牌黑名单实体
    /// 用于存储已失效的访问令牌，支持登出后令牌失效
    /// </summary>
    [SugarTable("token_blacklist")]
    public class TokenBlacklistEntity
    {
        /// <summary>
        /// 记录唯一标识符
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "记录ID")]
        public string GID { get; set; } = string.Empty;

        /// <summary>
        /// 被加入黑名单的访问令牌
        /// </summary>
        [SugarColumn(ColumnDescription = "访问令牌", Length = 500)]
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// 关联的用户ID
        /// </summary>
        [SugarColumn(ColumnDescription = "用户ID")]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// 令牌过期时间，过期后自动清理
        /// </summary>
        [SugarColumn(ColumnDescription = "过期时间")]
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// 加入黑名单的时间
        /// </summary>
        [SugarColumn(ColumnDescription = "加入黑名单时间")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 加入黑名单的原因
        /// </summary>
        [SugarColumn(ColumnDescription = "原因", Length = 100, IsNullable = true)]
        public string? Reason { get; set; }
    }
}
