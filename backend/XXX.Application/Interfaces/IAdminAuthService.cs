using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 管理后台认证服务接口。
    /// </summary>
    public interface IAdminAuthService
    {
        /// <summary>
        /// 管理员登录。
        /// </summary>
        Task<AdminLoginResponseDto> LoginAsync(AdminLoginRequestDto request);

        /// <summary>
        /// 刷新管理员令牌。
        /// </summary>
        Task<AdminLoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);

        /// <summary>
        /// 获取当前管理员信息。
        /// </summary>
        Task<AdminCurrentUserDto?> GetCurrentUserAsync(string adminId);

        /// <summary>
        /// 管理员登出。
        /// </summary>
        Task<bool> LogoutAsync(string adminId, string? accessToken = null, string? refreshToken = null);
    }
}
