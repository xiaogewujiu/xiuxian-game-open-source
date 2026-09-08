namespace XXX.Application.DTOs
{
    /// <summary>
    /// 后台管理员列表项。
    /// </summary>
    public class AdminUserListItemDto
    {
        /// <summary>
        /// 管理员编号。
        /// </summary>
        public string AdminId { get; set; } = string.Empty;

        /// <summary>
        /// 账号。
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
        /// 是否启用。
        /// </summary>
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// 后台管理员详情。
    /// </summary>
    public class AdminUserDetailDto
    {
        /// <summary>
        /// 管理员编号。
        /// </summary>
        public string AdminId { get; set; } = string.Empty;

        /// <summary>
        /// 账号。
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
        /// 是否启用。
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// 最后登录时间。
        /// </summary>
        public DateTime? LastLoginTime { get; set; }
    }

    /// <summary>
    /// 新建后台管理员请求。
    /// </summary>
    public class AdminCreateUserRequestDto
    {
        /// <summary>
        /// 账号。
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
        /// 初始密码。
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// 更新后台管理员请求。
    /// </summary>
    public class AdminUpdateUserRequestDto
    {
        /// <summary>
        /// 显示名。
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 角色。
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// 是否启用。
        /// </summary>
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// 重置后台管理员密码请求。
    /// </summary>
    public class AdminResetPasswordRequestDto
    {
        /// <summary>
        /// 新密码。
        /// </summary>
        public string NewPassword { get; set; } = string.Empty;
    }
}
