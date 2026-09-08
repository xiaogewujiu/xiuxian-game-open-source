using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.Ranking;
using XXX.WebApi.Extensions;

namespace XXX.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TowerController : ControllerBase
    {
        private readonly ITowerService _towerService;

        public TowerController(ITowerService towerService)
        {
            _towerService = towerService;
        }

        private string? GetCurrentPlayerId() => User.GetCurrentPlayerId();

        [HttpGet("me")]
        public async Task<ActionResult<ApiResponse<TowerMeDto>>> GetMe()
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _towerService.GetMyInfoAsync(playerId);
            if (result == null) return NotFound(ApiResponse<TowerMeDto>.Fail("通天塔数据不存在。"));

            return Ok(ApiResponse<TowerMeDto>.Ok(result));
        }

        [HttpGet("floors")]
        public async Task<ActionResult<ApiResponse<List<TowerFloorSummaryDto>>>> GetFloors()
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _towerService.GetFloorSummariesAsync(playerId);
            return Ok(ApiResponse<List<TowerFloorSummaryDto>>.Ok(result));
        }

        [HttpPost("challenge")]
        public async Task<ActionResult<ApiResponse<TowerChallengeResultDto>>> Challenge()
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            try
            {
                var result = await _towerService.ChallengeAsync(playerId);
                return Ok(ApiResponse<TowerChallengeResultDto>.Ok(result, result.IsWin ? "通关成功！" : "挑战失败。"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<TowerChallengeResultDto>.Fail(ex.Message));
            }
        }

        [HttpGet("history")]
        public async Task<ActionResult<ApiResponse<List<TowerBattleLogDto>>>> GetHistory([FromQuery] int take = 20)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _towerService.GetHistoryAsync(playerId, take);
            return Ok(ApiResponse<List<TowerBattleLogDto>>.Ok(result));
        }

        [HttpPost("buy-attempts")]
        public async Task<ActionResult<ApiResponse<int>>> BuyAttempts()
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            try
            {
                var cost = await _towerService.BuyAttemptsAsync(playerId);
                return Ok(ApiResponse<int>.Ok(cost, $"购买成功，消耗 {cost} 灵石。"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<int>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 获取通天塔排行榜。
        /// </summary>
        [HttpGet("leaderboard")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<RankingEntry>>>> GetLeaderboard([FromQuery] int count = 50)
        {
            var result = await _towerService.GetLeaderboardAsync(count);
            return Ok(ApiResponse<List<RankingEntry>>.Ok(result));
        }
    }
}
