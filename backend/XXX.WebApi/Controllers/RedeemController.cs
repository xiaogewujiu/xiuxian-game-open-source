using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.WebApi.Extensions;

namespace XXX.WebApi.Controllers
{
    /// <summary>
    /// 兑换码控制器。
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RedeemController : ControllerBase
    {
        private readonly IRedeemCodeService _redeemCodeService;

        /// <summary>
        /// 初始化兑换码控制器。
        /// </summary>
        public RedeemController(IRedeemCodeService redeemCodeService)
        {
            _redeemCodeService = redeemCodeService;
        }

        private string? GetCurrentPlayerId() => User.GetCurrentPlayerId();

        /// <summary>
        /// 兑换一个奖励码。
        /// </summary>
        [HttpPost("claim")]
        public async Task<ActionResult<ApiResponse<RedeemCodeResultDto>>> Claim([FromBody] RedeemCodeRequestDto request)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _redeemCodeService.RedeemAsync(playerId, request);
            return Ok(ApiResponse<RedeemCodeResultDto>.Ok(result, result.Message));
        }
    }
}
