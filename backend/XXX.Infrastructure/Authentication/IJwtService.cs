using System.Security.Claims;

namespace XXX.Infrastructure.Authentication
{
    /// <summary>
    /// JWT服务接口
    /// 提供JWT令牌的生成、验证、刷新和管理功能
    /// </summary>
    /// <remarks>
    /// 主要功能：
    /// - 生成访问令牌和刷新令牌
    /// - 验证令牌有效性
    /// - 刷新令牌
    /// - 撤销令牌
    /// - 管理令牌黑名单
    /// - 清理过期令牌
    /// </remarks>
    public interface IJwtService
    {
        /// <summary>
        /// 生成访问令牌和刷新令牌
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="userName">用户名</param>
        /// <param name="role">用户角色，默认为"user"</param>
        /// <returns>令牌模型，包含访问令牌和刷新令牌</returns>
        TokenModel GenerateTokens(string userId, string userName, string role = "user", IEnumerable<string>? permissions = null);

        /// <summary>
        /// 生成访问令牌
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="userName">用户名</param>
        /// <param name="role">用户角色，默认为"user"</param>
        /// <returns>JWT访问令牌字符串</returns>
        string GenerateAccessToken(string userId, string userName, string role = "user", IEnumerable<string>? permissions = null);

        /// <summary>
        /// 生成刷新令牌
        /// </summary>
        /// <returns>随机生成的刷新令牌字符串</returns>
        string GenerateRefreshToken();

        /// <summary>
        /// 验证访问令牌
        /// </summary>
        /// <param name="token">要验证的访问令牌</param>
        /// <returns>如果验证成功返回ClaimsPrincipal，否则返回null</returns>
        ClaimsPrincipal? ValidateAccessToken(string token);

        /// <summary>
        /// 验证刷新令牌
        /// </summary>
        /// <param name="refreshToken">要验证的刷新令牌</param>
        /// <returns>如果令牌有效且未过期返回true，否则返回false</returns>
        bool ValidateRefreshToken(string refreshToken);

        /// <summary>
        /// 刷新令牌
        /// </summary>
        /// <param name="accessToken">旧的访问令牌</param>
        /// <param name="refreshToken">刷新令牌</param>
        /// <returns>新的令牌模型，如果刷新失败返回null</returns>
        TokenModel? RefreshTokens(string accessToken, string refreshToken);

        /// <summary>
        /// 从令牌中获取用户ID
        /// </summary>
        /// <param name="token">JWT令牌</param>
        /// <returns>用户ID，如果令牌无效返回null</returns>
        string? GetUserIdFromToken(string token);

        /// <summary>
        /// 从令牌中获取用户名
        /// </summary>
        /// <param name="token">JWT令牌</param>
        /// <returns>用户名，如果令牌无效返回null</returns>
        string? GetUserNameFromToken(string token);

        /// <summary>
        /// 获取令牌过期时间
        /// </summary>
        /// <param name="token">JWT令牌</param>
        /// <returns>过期时间，如果令牌无效返回null</returns>
        DateTime? GetExpirationDate(string token);

        /// <summary>
        /// 撤销刷新令牌
        /// </summary>
        /// <param name="refreshToken">要撤销的刷新令牌</param>
        Task RevokeRefreshTokenAsync(string refreshToken);

        /// <summary>
        /// 撤销用户所有刷新令牌
        /// </summary>
        /// <param name="userId">用户ID</param>
        Task RevokeAllUserTokensAsync(string userId);

        /// <summary>
        /// 将访问令牌加入黑名单
        /// </summary>
        /// <param name="accessToken">访问令牌</param>
        /// <param name="userId">用户ID</param>
        /// <param name="expiresAt">令牌过期时间</param>
        /// <param name="reason">加入黑名单的原因（可选）</param>
        Task AddToBlacklistAsync(string accessToken, string userId, DateTime expiresAt, string? reason = null);

        /// <summary>
        /// 检查令牌是否在黑名单中
        /// </summary>
        /// <param name="accessToken">访问令牌</param>
        /// <returns>如果在黑名单中返回true，否则返回false</returns>
        Task<bool> IsTokenBlacklistedAsync(string accessToken);

        /// <summary>
        /// 清理过期的令牌记录
        /// </summary>
        /// <remarks>
        /// 定期调用此方法清理数据库中过期的刷新令牌和黑名单记录
        /// </remarks>
        Task CleanupExpiredTokensAsync();
    }
}
