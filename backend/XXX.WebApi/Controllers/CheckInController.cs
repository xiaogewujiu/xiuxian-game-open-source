using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.WebApi.Extensions;

namespace XXX.WebApi.Controllers
{
    /// <summary>
    /// 每日签到控制器。
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CheckInController : ControllerBase
    {
        private readonly ICheckInService _checkInService;

        /// <summary>
        /// 初始化签到控制器。
        /// </summary>
        public CheckInController(ICheckInService checkInService)
        {
            _checkInService = checkInService;
        }

        private string? GetCurrentPlayerId() => User.GetCurrentPlayerId();

        /// <summary>
        /// 获取指定月份的签到状态。
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<CheckInStatusDto>>> GetStatus([FromQuery] int? year = null, [FromQuery] int? month = null)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _checkInService.GetStatusAsync(playerId, year, month);
            return Ok(ApiResponse<CheckInStatusDto>.Ok(result));
        }

        /// <summary>
        /// 领取今日签到奖励。
        /// </summary>
        [HttpPost("claim")]
        public async Task<ActionResult<ApiResponse<ClaimCheckInResultDto>>> Claim()
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _checkInService.ClaimAsync(playerId);
            return Ok(ApiResponse<ClaimCheckInResultDto>.Ok(result, result.Message));
        }
    }
}
