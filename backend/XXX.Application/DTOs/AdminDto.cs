namespace XXX.Application.DTOs
{
    /// <summary>
    /// 管理员登录请求。
    /// </summary>
    public class AdminLoginRequestDto
    {
        /// <summary>
        /// 管理员账号。
        /// </summary>
        public string Account { get; set; } = string.Empty;

        /// <summary>
        /// 管理员密码。
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// 当前管理员信息。
    /// </summary>
    public class AdminCurrentUserDto
    {
        /// <summary>
        /// 管理员编号。
        /// </summary>
        public string AdminId { get; set; } = string.Empty;

        /// <summary>
        /// 登录账号。
        /// </summary>
        public string Account { get; set; } = string.Empty;

        /// <summary>
        /// 显示名。
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 角色。
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// 当前管理员权限列表。
        /// </summary>
        public List<string> Permissions { get; set; } = [];
    }

    /// <summary>
    /// 管理员登录响应。
    /// </summary>
    public class AdminLoginResponseDto
    {
        /// <summary>
        /// 访问令牌。
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// 刷新令牌。
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// 令牌类型。
        /// </summary>
        public string TokenType { get; set; } = "Bearer";

        /// <summary>
        /// 过期时间，单位秒。
        /// </summary>
        public int ExpiresIn { get; set; }

        /// <summary>
        /// 当前管理员。
        /// </summary>
        public AdminCurrentUserDto CurrentUser { get; set; } = new();
    }

    /// <summary>
    /// 管理后台首页摘要。
    /// </summary>
    public class AdminDashboardSummaryDto
    {
        /// <summary>
        /// 玩家总数。
        /// </summary>
        public int PlayerCount { get; set; }

        /// <summary>
        /// 管理员总数。
        /// </summary>
        public int AdminCount { get; set; }

        /// <summary>
        /// 地图模板数量。
        /// </summary>
        public int MapTemplateCount { get; set; }

        /// <summary>
        /// 怪物模板数量。
        /// </summary>
        public int MonsterTemplateCount { get; set; }

        /// <summary>
        /// 道具模板数量。
        /// </summary>
        public int ItemTemplateCount { get; set; }

        /// <summary>
        /// 装备模板数量。
        /// </summary>
        public int EquipmentTemplateCount { get; set; }
    }
}
