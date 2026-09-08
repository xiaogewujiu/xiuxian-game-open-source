using System.Security.Claims;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Application.Security;
using XXX.Entity;
using XXX.Infrastructure.Authentication;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    /// <summary>
    /// 管理后台认证服务。
    /// 与玩家认证分离，避免后台会话与玩家会话混用。
    /// </summary>
    public class AdminAuthService : IAdminAuthService
    {
        private readonly IRepository<AdminUserEntity> _adminUserRepository;
        private readonly IJwtService _jwtService;

        /// <summary>
        /// 初始化管理后台认证服务。
        /// </summary>
        public AdminAuthService(
            IRepository<AdminUserEntity> adminUserRepository,
            IJwtService jwtService)
        {
            _adminUserRepository = adminUserRepository;
            _jwtService = jwtService;
        }

        /// <summary>
        /// 管理员登录。
        /// </summary>
        public async Task<AdminLoginResponseDto> LoginAsync(AdminLoginRequestDto request)
        {
            var normalizedAccount = NormalizeAccount(request.Account);
            if (string.IsNullOrWhiteSpace(normalizedAccount))
            {
                throw new InvalidOperationException("管理员账号不能为空");
            }

            var adminUser = await _adminUserRepository.GetFirstAsync(admin =>
                admin.Account == normalizedAccount &&
                !admin.IsDeleted);

            if (adminUser == null || !adminUser.IsActive || !VerifyPassword(request.Password, adminUser.PasswordHash))
            {
                throw new InvalidOperationException("管理员账号或密码错误");
            }

            adminUser.LastLoginTime = DateTime.Now;
            adminUser.LastUpdateTime = DateTime.Now;
            await _adminUserRepository.UpdateAsync(adminUser);

            var permissions = AdminPermissionCatalog.GetPermissions(adminUser.Role);
            var tokenModel = _jwtService.GenerateTokens(adminUser.AdminId, adminUser.DisplayName, adminUser.Role, permissions);
            return BuildLoginResponse(adminUser, tokenModel);
        }

        /// <summary>
        /// 刷新管理员令牌。
        /// </summary>
        public async Task<AdminLoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            var tokenModel = _jwtService.RefreshTokens(request.AccessToken, request.RefreshToken);
            if (tokenModel == null)
            {
                throw new InvalidOperationException("刷新令牌无效或已过期");
            }

            var principal = _jwtService.ValidateAccessToken(tokenModel.AccessToken);
            if (principal == null || !IsAdminPrincipal(principal))
            {
                throw new InvalidOperationException("当前令牌不属于管理后台会话");
            }

            var adminId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? principal.FindFirst(JwtClaims.UserId)?.Value;
            if (string.IsNullOrWhiteSpace(adminId))
            {
                throw new InvalidOperationException("管理员身份无效");
            }

            var adminUser = await _adminUserRepository.GetByIdAsync(adminId);
            if (adminUser == null || adminUser.IsDeleted || !adminUser.IsActive)
            {
                throw new InvalidOperationException("管理员账号不存在或已禁用");
            }

            return BuildLoginResponse(adminUser, tokenModel);
        }

        /// <summary>
        /// 获取当前管理员信息。
        /// </summary>
        public async Task<AdminCurrentUserDto?> GetCurrentUserAsync(string adminId)
        {
            if (string.IsNullOrWhiteSpace(adminId))
            {
                return null;
            }

            var adminUser = await _adminUserRepository.GetByIdAsync(adminId);
            if (adminUser == null || adminUser.IsDeleted || !adminUser.IsActive)
            {
                return null;
            }

            return MapCurrentUser(adminUser);
        }

        /// <summary>
        /// 管理员登出。
        /// </summary>
        public async Task<bool> LogoutAsync(string adminId, string? accessToken = null, string? refreshToken = null)
        {
            if (!string.IsNullOrEmpty(refreshToken))
            {
                await _jwtService.RevokeRefreshTokenAsync(refreshToken);
            }

            if (!string.IsNullOrEmpty(accessToken))
            {
                var expiresAt = _jwtService.GetExpirationDate(accessToken);
                if (expiresAt.HasValue && expiresAt.Value > DateTime.UtcNow)
                {
                    await _jwtService.AddToBlacklistAsync(accessToken, adminId, expiresAt.Value, "AdminLogout");
                }
            }

            return true;
        }

        private static AdminLoginResponseDto BuildLoginResponse(AdminUserEntity adminUser, TokenModel tokenModel)
        {
            return new AdminLoginResponseDto
            {
                AccessToken = tokenModel.AccessToken,
                RefreshToken = tokenModel.RefreshToken,
                TokenType = tokenModel.TokenType,
                ExpiresIn = tokenModel.ExpiresIn,
                CurrentUser = MapCurrentUser(adminUser)
            };
        }

        private static AdminCurrentUserDto MapCurrentUser(AdminUserEntity adminUser)
        {
            return new AdminCurrentUserDto
            {
                AdminId = adminUser.AdminId,
                Account = adminUser.Account,
                DisplayName = adminUser.DisplayName,
                Role = adminUser.Role,
                Permissions = AdminPermissionCatalog.GetPermissions(adminUser.Role).ToList()
            };
        }

        private static bool IsAdminPrincipal(ClaimsPrincipal principal)
        {
            var role = principal.FindFirst(ClaimTypes.Role)?.Value
                ?? principal.FindFirst(JwtClaims.Role)?.Value
                ?? string.Empty;

            return AdminRoleCatalog.AdminRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
        }

        private static bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        private static string NormalizeAccount(string? account) => account?.Trim() ?? string.Empty;
    }
}
