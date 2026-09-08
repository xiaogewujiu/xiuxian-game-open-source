using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.Application.Security;

namespace XXX.WebApi.Controllers.Admin
{
    /// <summary>
    /// 管理后台认证控制器。
    /// </summary>
    [ApiController]
    [Route("api/admin/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAdminAuthService _adminAuthService;
        private readonly ILogger<AuthController> _logger;

        /// <summary>
        /// 初始化管理后台认证控制器。
        /// </summary>
        public AuthController(IAdminAuthService adminAuthService, ILogger<AuthController> logger)
        {
            _adminAuthService = adminAuthService;
            _logger = logger;
        }

        /// <summary>
        /// 管理员登录。
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<AdminLoginResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminLoginResponseDto>>> Login([FromBody] AdminLoginRequestDto request)
        {
            try
            {
                var response = await _adminAuthService.LoginAsync(request);
                return Ok(ApiResponse<AdminLoginResponseDto>.Ok(response, "管理员登录成功"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Admin login failed: {Account}", request.Account);
                return BadRequest(ApiResponse<AdminLoginResponseDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 刷新管理员访问令牌。
        /// </summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<AdminLoginResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminLoginResponseDto>>> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            try
            {
                var response = await _adminAuthService.RefreshTokenAsync(request);
                return Ok(ApiResponse<AdminLoginResponseDto>.Ok(response, "管理员令牌刷新成功"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Admin refresh token failed.");
                return BadRequest(ApiResponse<AdminLoginResponseDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 获取当前登录管理员信息。
        /// </summary>
        [HttpGet("me")]
        [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminCurrentUserDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<AdminCurrentUserDto>>> Me()
        {
            var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(XXX.Infrastructure.Authentication.JwtClaims.UserId)?.Value;
            if (string.IsNullOrWhiteSpace(adminId))
            {
                return Unauthorized(ApiResponse<AdminCurrentUserDto>.Fail("未登录"));
            }

            var currentUser = await _adminAuthService.GetCurrentUserAsync(adminId);
            if (currentUser == null)
            {
                return Unauthorized(ApiResponse<AdminCurrentUserDto>.Fail("管理员账号不存在或已禁用"));
            }

            return Ok(ApiResponse<AdminCurrentUserDto>.Ok(currentUser));
        }

        /// <summary>
        /// 管理员登出。
        /// </summary>
        [HttpPost("logout")]
        [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse>> Logout([FromBody] RefreshTokenRequestDto? request = null)
        {
            var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(XXX.Infrastructure.Authentication.JwtClaims.UserId)?.Value;
            if (string.IsNullOrWhiteSpace(adminId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var bearerToken = Request.Headers.Authorization.ToString();
            var accessToken = bearerToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? bearerToken["Bearer ".Length..].Trim()
                : string.Empty;

            await _adminAuthService.LogoutAsync(adminId, accessToken, request?.RefreshToken);
            return Ok(ApiResponse.Ok("管理员登出成功"));
        }
    }
}
