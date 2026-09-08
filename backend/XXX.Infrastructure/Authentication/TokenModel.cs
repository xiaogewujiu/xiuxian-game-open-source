namespace XXX.Infrastructure.Authentication
{
    /// <summary>
    /// Token模型
    /// </summary>
    public class TokenModel
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
        /// 令牌类型
        /// </summary>
        public string TokenType { get; set; } = "Bearer";

        /// <summary>
        /// 过期时间（秒）
        /// </summary>
        public int ExpiresIn { get; set; }

        /// <summary>
        /// 签发时间
        /// </summary>
        public DateTime IssuedAt { get; set; }

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime ExpiresAt { get; set; }
    }

    /// <summary>
    /// JWT Claims常量
    /// </summary>
    public static class JwtClaims
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public const string UserId = "uid";

        /// <summary>
        /// 用户名
        /// </summary>
        public const string UserName = "uname";

        /// <summary>
        /// 角色
        /// </summary>
        public const string Role = "role";

        /// <summary>
        /// 令牌ID
        /// </summary>
        public const string Jti = "jti";

        /// <summary>
        /// 权限声明。
        /// </summary>
        public const string Permission = "perm";
    }
}
