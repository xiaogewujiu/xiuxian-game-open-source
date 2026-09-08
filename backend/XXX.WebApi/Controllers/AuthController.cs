using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.WebApi.Extensions;

namespace XXX.WebApi.Controllers
{
    /// <summary>
    /// 认证控制器。
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        /// <summary>
        /// 玩家认证服务。
        /// </summary>
        private readonly IAuthService _authService;

        /// <summary>
        /// 认证控制器日志记录器。
        /// </summary>
        private readonly ILogger<AuthController> _logger;

        /// <summary>
        /// 初始化认证控制器。
        /// </summary>
        /// <param name="authService">玩家认证服务。</param>
        /// <param name="logger">日志记录器。</param>
        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// 用户登录。
        /// </summary>
        /// <param name="request">登录请求。</param>
        /// <returns>登录响应，包含访问令牌、刷新令牌和玩家快照。</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                var response = await _authService.LoginAsync(request);
                return Ok(ApiResponse<LoginResponseDto>.Ok(response, "登录成功"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "登录失败: {Account}", request.Account);
                return BadRequest(ApiResponse<LoginResponseDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 用户注册。
        /// </summary>
        /// <param name="request">注册请求。</param>
        /// <returns>注册完成后的登录响应。</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Register([FromBody] RegisterRequestDto request)
        {
            try
            {
                var response = await _authService.RegisterAsync(request);
                return Ok(ApiResponse<LoginResponseDto>.Ok(response, "注册成功"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "注册失败: {Account}", request.Account);
                return BadRequest(ApiResponse<LoginResponseDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 刷新令牌。
        /// </summary>
        /// <param name="request">刷新令牌请求。</param>
        /// <returns>新的登录响应。</returns>
        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            try
            {
                var response = await _authService.RefreshTokenAsync(request);
                return Ok(ApiResponse<LoginResponseDto>.Ok(response, "令牌刷新成功"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "令牌刷新失败");
                return BadRequest(ApiResponse<LoginResponseDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 用户登出。
        /// </summary>
        /// <param name="request">可选的刷新令牌请求体。</param>
        /// <returns>登出结果。</returns>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse>> Logout([FromBody] RefreshTokenRequestDto? request = null)
        {
            // 中文注释：
            // 统一使用扩展方法从令牌里读取玩家 ID，
            // 避免不同控制器分别读取 Name / NameIdentifier / 自定义 uid，
            // 造成登出、鉴权口径不一致的问题。
            var userId = User.GetCurrentPlayerId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var bearerToken = Request.Headers.Authorization.ToString();
            var accessToken = bearerToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? bearerToken["Bearer ".Length..].Trim()
                : string.Empty;

            await _authService.LogoutAsync(userId, accessToken, request?.RefreshToken);
            return Ok(ApiResponse.Ok("登出成功"));
        }

        /// <summary>
        /// 检查账号是否存在。
        /// </summary>
        /// <param name="account">待检查账号。</param>
        /// <returns>账号存在性结果。</returns>
        [HttpGet("check-account")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<bool>>> CheckAccountExists([FromQuery] string account)
        {
            var exists = await _authService.IsAccountExistsAsync(account);
            return Ok(ApiResponse<bool>.Ok(exists));
        }
    }
}
