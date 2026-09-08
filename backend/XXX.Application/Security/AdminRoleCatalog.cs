namespace XXX.Application.Security
{
    /// <summary>
    /// 管理后台角色与授权策略常量。
    /// </summary>
    public static class AdminRoleCatalog
    {
        /// <summary>
        /// 管理后台统一访问策略。
        /// </summary>
        public const string AdminOnlyPolicy = "AdminOnly";

        /// <summary>
        /// 当前允许进入后台的角色集合。
        /// </summary>
        public static readonly string[] AdminRoles =
        {
            "super_admin",
            "admin",
            "operator",
            "gm_support",
            "content_designer",
            "economy_designer",
            "audit_viewer"
        };
    }
}
