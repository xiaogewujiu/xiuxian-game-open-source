namespace XXX.Infrastructure.Authentication
{
    /// <summary>
    /// JWT配置选项
    /// </summary>
    public class JwtOptions
    {
        /// <summary>
        /// 密钥
        /// </summary>
        public string SecretKey { get; set; } = string.Empty;

        /// <summary>
        /// 签发者
        /// </summary>
        public string Issuer { get; set; } = string.Empty;

        /// <summary>
        /// 接收者
        /// </summary>
        public string Audience { get; set; } = string.Empty;

        /// <summary>
        /// 访问令牌过期时间（分钟）
        /// </summary>
        public int AccessTokenExpirationMinutes { get; set; } = 60;

        /// <summary>
        /// 刷新令牌过期时间（天）
        /// </summary>
        public int RefreshTokenExpirationDays { get; set; } = 7;

        /// <summary>
        /// 验证密钥
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(SecretKey))
                throw new ArgumentException("JWT密钥不能为空");

            if (string.IsNullOrWhiteSpace(Issuer))
                throw new ArgumentException("JWT签发者不能为空");

            if (string.IsNullOrWhiteSpace(Audience))
                throw new ArgumentException("JWT接收者不能为空");

            if (SecretKey.Length < 32)
                throw new ArgumentException("JWT密钥长度至少32位");
        }
    }
}
