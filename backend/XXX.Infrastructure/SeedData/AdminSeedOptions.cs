namespace XXX.Infrastructure.SeedData
{
    /// <summary>
    /// 默认管理员种子配置。
    /// 仅在后台管理员表为空时用于初始化第一位管理员。
    /// </summary>
    public sealed class AdminSeedOptions
    {
        /// <summary>
        /// 是否启用默认管理员初始化。
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// 默认管理员账号。
        /// </summary>
        public string DefaultAccount { get; set; } = "admin";

        /// <summary>
        /// 默认管理员密码。
        /// </summary>
        public string DefaultPassword { get; set; } = "Admin123456";

        /// <summary>
        /// 默认管理员显示名。
        /// </summary>
        public string DefaultDisplayName { get; set; } = "系统管理员";

        /// <summary>
        /// 默认管理员角色。
        /// </summary>
        public string DefaultRole { get; set; } = "super_admin";
    }
}
