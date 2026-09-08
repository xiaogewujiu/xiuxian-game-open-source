using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.WebApi.Extensions;

namespace XXX.WebApi.Controllers
{
    /// <summary>
    /// 玩家称号控制器。
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TitleController : ControllerBase
    {
        private readonly ITitleService _titleService;

        public TitleController(ITitleService titleService)
        {
            _titleService = titleService;
        }

        private string? GetCurrentPlayerId() => User.GetCurrentPlayerId();

        /// <summary>
        /// 获取玩家称号总览。
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PlayerTitleOverviewDto>>> GetOverview()
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _titleService.GetOverviewAsync(playerId);
            return Ok(ApiResponse<PlayerTitleOverviewDto>.Ok(result));
        }

        /// <summary>
        /// 佩戴称号。
        /// </summary>
        [HttpPost("equip")]
        public async Task<ActionResult<ApiResponse>> EquipTitle([FromBody] EquipTitleRequest request)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var success = await _titleService.EquipTitleAsync(playerId, request.TitleId);
            if (!success) return BadRequest(ApiResponse.Fail("佩戴失败，称号不存在或未拥有。"));

            return Ok(ApiResponse.Ok("称号已佩戴。"));
        }

        /// <summary>
        /// 取消佩戴称号。
        /// </summary>
        [HttpPost("unequip")]
        public async Task<ActionResult<ApiResponse>> UnequipTitle()
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            await _titleService.UnequipTitleAsync(playerId);
            return Ok(ApiResponse.Ok("已取消佩戴。"));
        }
    }

    /// <summary>
    /// 佩戴称号请求。
    /// </summary>
    public class EquipTitleRequest
    {
        public string TitleId { get; set; } = string.Empty;
    }
}
