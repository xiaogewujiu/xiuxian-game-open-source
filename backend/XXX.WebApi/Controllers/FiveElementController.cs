using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.WebApi.Extensions;

namespace XXX.WebApi.Controllers
{
    /// <summary>
    /// 五行聚灵阵接口。
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FiveElementController : ControllerBase
    {
        private readonly IFiveElementService _fiveElementService;

        /// <summary>
        /// 初始化五行聚灵阵控制器。
        /// </summary>
        public FiveElementController(IFiveElementService fiveElementService) => _fiveElementService = fiveElementService;

        /// <summary>
        /// 从当前登录上下文中读取玩家编号。
        /// </summary>
        private string? GetPlayerId() => User.GetCurrentPlayerId();

        /// <summary>
        /// 获取五行阵总览。
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<FiveElementDto>>> GetInfo()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var info = await _fiveElementService.GetFiveElementInfoAsync(playerId);
            return Ok(ApiResponse<FiveElementDto>.Ok(info));
        }

        /// <summary>
        /// 升级聚灵阵主等级。
        /// </summary>
        [HttpPost("array/upgrade")]
        public async Task<ActionResult<ApiResponse<bool>>> UpgradeArray()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var result = await _fiveElementService.UpgradeArrayAsync(playerId);
            if (!result)
            {
                return BadRequest(ApiResponse<bool>.Fail("升级失败"));
            }

            return Ok(ApiResponse<bool>.Ok(true, "升级成功"));
        }

        /// <summary>
        /// 升级某个五行分支。
        /// </summary>
        [HttpPost("upgrade")]
        public async Task<ActionResult<ApiResponse<bool>>> UpgradeElement([FromBody] UpgradeElementRequestDto request)
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var result = await _fiveElementService.UpgradeElementAsync(playerId, request);
            if (!result)
            {
                return BadRequest(ApiResponse<bool>.Fail("升级失败"));
            }

            return Ok(ApiResponse<bool>.Ok(true, "升级成功"));
        }

        /// <summary>
        /// 收取当前累计灵力。
        /// </summary>
        [HttpPost("collect")]
        public async Task<ActionResult<ApiResponse<CollectSpiritPowerResultDto>>> CollectSpiritPower()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var result = await _fiveElementService.CollectSpiritPowerAsync(playerId);
            if (!result.Success)
            {
                return BadRequest(ApiResponse<CollectSpiritPowerResultDto>.Fail(result.Message));
            }

            return Ok(ApiResponse<CollectSpiritPowerResultDto>.Ok(result, result.Message));
        }

        /// <summary>
        /// 获取当前灵力产出速率。
        /// </summary>
        [HttpGet("production")]
        public async Task<ActionResult<ApiResponse<int>>> GetProductionRate()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var rate = await _fiveElementService.CalculateSpiritPowerProductionAsync(playerId);
            return Ok(ApiResponse<int>.Ok(rate));
        }
    }
}
