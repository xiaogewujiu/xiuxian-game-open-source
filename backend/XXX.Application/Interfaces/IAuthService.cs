using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 玩家认证服务接口。
    /// 提供登录、注册、令牌刷新、登出和账号存在性校验等能力。
    /// </summary>
    /// <remarks>
    /// 当前实现会同时处理几个真实业务规则：
    /// 1. 登录时校验封禁状态。
    /// 2. 注册时校验职业、密码强度并发放新手礼包。
    /// 3. 刷新令牌时依赖 JWT 服务统一管理刷新令牌与黑名单。
    /// </remarks>
    public interface IAuthService
    {
        /// <summary>
        /// 处理账号登录。
        /// </summary>
        /// <param name="request">登录请求。</param>
        /// <returns>登录响应，包含访问令牌、刷新令牌和玩家快照。</returns>
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);

        /// <summary>
        /// 处理账号注册。
        /// </summary>
        /// <param name="request">注册请求。</param>
        /// <returns>注册完成后的登录响应。</returns>
        Task<LoginResponseDto> RegisterAsync(RegisterRequestDto request);

        /// <summary>
        /// 刷新访问令牌与刷新令牌。
        /// </summary>
        /// <param name="request">刷新令牌请求。</param>
        /// <returns>新的登录响应。</returns>
        Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);

        /// <summary>
        /// 注销当前登录会话。
        /// </summary>
        /// <param name="userId">玩家编号。</param>
        /// <param name="accessToken">访问令牌，可选，用于加入黑名单。</param>
        /// <param name="refreshToken">刷新令牌，可选，用于撤销。</param>
        /// <returns>登出执行结果。</returns>
        Task<bool> LogoutAsync(string userId, string? accessToken = null, string? refreshToken = null);

        /// <summary>
        /// 检查账号是否已存在。
        /// </summary>
        /// <param name="account">待检查账号。</param>
        /// <returns>账号已存在返回真。</returns>
        Task<bool> IsAccountExistsAsync(string account);
    }
}
