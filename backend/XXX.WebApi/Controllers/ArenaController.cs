using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.WebApi.Extensions;

namespace XXX.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ArenaController : ControllerBase
    {
        private readonly IArenaService _arenaService;

        public ArenaController(IArenaService arenaService)
        {
            _arenaService = arenaService;
        }

        private string? GetCurrentPlayerId() => User.GetCurrentPlayerId();

        /// <summary>
        /// 获取我的竞技场信息。
        /// </summary>
        [HttpGet("me")]
        public async Task<ActionResult<ApiResponse<ArenaMeDto>>> GetMe()
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _arenaService.GetMyInfoAsync(playerId);
            if (result == null) return NotFound(ApiResponse<ArenaMeDto>.Fail("竞技场数据不存在。"));

            return Ok(ApiResponse<ArenaMeDto>.Ok(result));
        }

        /// <summary>
        /// 获取推荐对手。
        /// </summary>
        [HttpGet("opponents")]
        public async Task<ActionResult<ApiResponse<List<ArenaOpponentDto>>>> GetOpponents()
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _arenaService.GetOpponentsAsync(playerId);
            return Ok(ApiResponse<List<ArenaOpponentDto>>.Ok(result));
        }

        /// <summary>
        /// 发起挑战。
        /// </summary>
        [HttpPost("challenge")]
        public async Task<ActionResult<ApiResponse<ArenaChallengeResultDto>>> Challenge([FromBody] ChallengeRequest request)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            try
            {
                var result = await _arenaService.ChallengeAsync(playerId, request.DefenderId);
                return Ok(ApiResponse<ArenaChallengeResultDto>.Ok(result, result.IsWin ? "挑战胜利！" : "挑战失败。"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<ArenaChallengeResultDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 获取对战历史。
        /// </summary>
        [HttpGet("history")]
        public async Task<ActionResult<ApiResponse<List<ArenaBattleLogDto>>>> GetHistory(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _arenaService.GetHistoryAsync(playerId, page, pageSize);
            return Ok(ApiResponse<List<ArenaBattleLogDto>>.Ok(result));
        }

        /// <summary>
        /// 购买挑战次数。
        /// </summary>
        [HttpPost("buy-attempts")]
        public async Task<ActionResult<ApiResponse<int>>> BuyAttempts()
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            try
            {
                var cost = await _arenaService.BuyAttemptsAsync(playerId);
                return Ok(ApiResponse<int>.Ok(cost, $"购买成功，消耗 {cost} 灵石。"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<int>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 获取赛季信息。
        /// </summary>
        [HttpGet("season")]
        public async Task<ActionResult<ApiResponse<ArenaSeasonDto>>> GetSeason()
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _arenaService.GetSeasonInfoAsync(playerId);
            return Ok(ApiResponse<ArenaSeasonDto>.Ok(result));
        }
    }

    public class ChallengeRequest
    {
        public string DefenderId { get; set; } = string.Empty;
    }
}
