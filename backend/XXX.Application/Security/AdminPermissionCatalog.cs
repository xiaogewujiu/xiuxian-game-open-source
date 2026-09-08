namespace XXX.Application.Security
{
    /// <summary>
    /// 管理后台权限目录。
    /// 包含权限编码、授权策略名与角色默认权限映射。
    /// </summary>
    public static class AdminPermissionCatalog
    {
        /// <summary>
        /// 配置读取权限。
        /// </summary>
        public const string ConfigRead = "admin.config.read";

        /// <summary>
        /// 配置写入权限。
        /// </summary>
        public const string ConfigWrite = "admin.config.write";

        /// <summary>
        /// 玩家读取权限策略。
        /// </summary>
        public const string PlayerReadPolicy = "AdminPlayerRead";

        /// <summary>
        /// 玩家读取授权策略。
        /// </summary>
        public const string PlayerRead = "admin.player.read";

        /// <summary>
        /// 玩家写入权限。
        /// </summary>
        public const string PlayerWrite = "admin.player.write";

        /// <summary>
        /// 玩家发放权限。
        /// </summary>
        public const string PlayerGrant = "admin.player.grant";

        /// <summary>
        /// 审计读取权限。
        /// </summary>
        public const string AuditRead = "admin.audit.read";

        /// <summary>
        /// 配置写入授权策略。
        /// </summary>
        public const string ConfigWritePolicy = "AdminConfigWrite";

        /// <summary>
        /// 玩家写入授权策略。
        /// </summary>
        public const string PlayerWritePolicy = "AdminPlayerWrite";

        /// <summary>
        /// 玩家发放授权策略。
        /// </summary>
        public const string PlayerGrantPolicy = "AdminPlayerGrant";

        /// <summary>
        /// 审计读取授权策略。
        /// </summary>
        public const string AuditReadPolicy = "AdminAuditRead";

        /// <summary>
        /// 根据角色获取默认权限集合。
        /// </summary>
        public static IReadOnlyList<string> GetPermissions(string role)
        {
            var normalizedRole = (role ?? string.Empty).Trim().ToLowerInvariant();
            return normalizedRole switch
            {
                "super_admin" or "admin" => new[]
                {
                    ConfigRead,
                    ConfigWrite,
                    PlayerRead,
                    PlayerWrite,
                    PlayerGrant,
                    AuditRead
                },
                "operator" => new[]
                {
                    ConfigRead,
                    ConfigWrite,
                    PlayerRead,
                    PlayerGrant,
                    AuditRead
                },
                "gm_support" => new[]
                {
                    PlayerRead,
                    PlayerWrite,
                    PlayerGrant,
                    AuditRead
                },
                "content_designer" => new[]
                {
                    ConfigRead,
                    ConfigWrite
                },
                "economy_designer" => new[]
                {
                    ConfigRead,
                    ConfigWrite
                },
                "audit_viewer" => new[]
                {
                    AuditRead
                },
                _ => Array.Empty<string>()
            };
        }
    }
}
