namespace XXX.Application.DTOs
{
    /// <summary>
    /// 登录请求数据传输对象。
    /// </summary>
    public class LoginRequestDto
    {
        /// <summary>
        /// 账号
        /// </summary>
        public string Account { get; set; } = string.Empty;

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// 注册请求数据传输对象。
    /// </summary>
    public class RegisterRequestDto
    {
        /// <summary>
        /// 账号
        /// </summary>
        public string Account { get; set; } = string.Empty;

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 确认密码
        /// </summary>
        public string ConfirmPassword { get; set; } = string.Empty;

        /// <summary>
        /// 道号
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 战斗职业键。
        /// 支持：<c>warrior</c>、<c>mage</c>、<c>body</c>。
        /// </summary>
        public string Profession { get; set; } = XXX.Entity.PlayerProfessionCatalog.Warrior;
    }

    /// <summary>
    /// 刷新令牌请求数据传输对象。
    /// </summary>
    public class RefreshTokenRequestDto
    {
        /// <summary>
        /// 访问令牌
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// 刷新令牌
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// 登录响应数据传输对象。
    /// </summary>
    public class LoginResponseDto
    {
        /// <summary>
        /// 访问令牌
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// 刷新令牌
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// 令牌类型。
        /// 当前固定为 <c>Bearer</c>。
        /// </summary>
        public string TokenType { get; set; } = "Bearer";

        /// <summary>
        /// 过期时间（秒）
        /// </summary>
        public int ExpiresIn { get; set; }

        /// <summary>
        /// 玩家信息
        /// </summary>
        public PlayerDto? Player { get; set; }
    }

    /// <summary>
    /// 当前用户信息数据传输对象。
    /// </summary>
    public class CurrentUserDto
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 角色
        /// </summary>
        public string Role { get; set; } = string.Empty;
    }
}
