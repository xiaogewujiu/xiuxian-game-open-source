using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.WebApi.Extensions;

namespace XXX.WebApi.Controllers
{
    /// <summary>
    /// 世界 Boss 玩家接口。
    /// </summary>
    [ApiController]
    [Route("api/world-boss")]
    [Authorize]
    public class WorldBossController : ControllerBase
    {
        /// <summary>
        /// 世界 Boss 玩家服务。
        /// </summary>
        private readonly IWorldBossService _worldBossService;

        /// <summary>
        /// 初始化世界 Boss 玩家接口。
        /// </summary>
        /// <param name="worldBossService">世界 Boss 玩家服务。</param>
        public WorldBossController(IWorldBossService worldBossService)
        {
            _worldBossService = worldBossService;
        }

        /// <summary>
        /// 从当前登录态中读取玩家编号。
        /// </summary>
        /// <returns>登录玩家编号；未登录时返回空。</returns>
        private string? GetPlayerId() => User.GetCurrentPlayerId();

        /// <summary>
        /// 获取当前世界 Boss 面板状态。
        /// </summary>
        /// <returns>当前 Boss、个人战斗状态与待领奖励信息。</returns>
        [HttpGet("current")]
        public async Task<ActionResult<ApiResponse<WorldBossCurrentDto>>> GetCurrent()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrWhiteSpace(playerId))
            {
                return Unauthorized(ApiResponse<WorldBossCurrentDto>.Fail("未登录"));
            }

            var result = await _worldBossService.GetCurrentAsync(playerId);
            return Ok(ApiResponse<WorldBossCurrentDto>.Ok(result));
        }

        /// <summary>
        /// 进入当前世界 Boss 战场。
        /// </summary>
        /// <returns>加入后的当前世界 Boss 面板数据。</returns>
        [HttpPost("join")]
        public async Task<ActionResult<ApiResponse<WorldBossCurrentDto>>> Join()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrWhiteSpace(playerId))
            {
                return Unauthorized(ApiResponse<WorldBossCurrentDto>.Fail("未登录"));
            }

            try
            {
                var result = await _worldBossService.JoinAsync(playerId);
                return Ok(ApiResponse<WorldBossCurrentDto>.Ok(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<WorldBossCurrentDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 获取世界 Boss 伤害排行榜。
        /// </summary>
        /// <param name="top">返回的排行数量上限。</param>
        /// <returns>排行榜列表。</returns>
        [HttpGet("ranking")]
        public async Task<ActionResult<ApiResponse<List<WorldBossRankingEntryDto>>>> GetRanking([FromQuery] int top = 10)
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrWhiteSpace(playerId))
            {
                return Unauthorized(ApiResponse<List<WorldBossRankingEntryDto>>.Fail("未登录"));
            }

            var result = await _worldBossService.GetRankingAsync(playerId, top);
            return Ok(ApiResponse<List<WorldBossRankingEntryDto>>.Ok(result));
        }

        /// <summary>
        /// 获取世界 Boss 战斗日志。
        /// </summary>
        /// <param name="count">返回的日志条数上限。</param>
        /// <returns>按最新记录倒序排列的日志列表。</returns>
        [HttpGet("logs")]
        public async Task<ActionResult<ApiResponse<List<WorldBossLogDto>>>> GetLogs([FromQuery] int count = 100)
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrWhiteSpace(playerId))
            {
                return Unauthorized(ApiResponse<List<WorldBossLogDto>>.Fail("未登录"));
            }

            var result = await _worldBossService.GetLogsAsync(playerId, count);
            return Ok(ApiResponse<List<WorldBossLogDto>>.Ok(result));
        }

        /// <summary>
        /// 执行一次世界 Boss 手动出手。
        /// </summary>
        /// <param name="request">本次出手请求。</param>
        /// <returns>本次出手结果与追加战斗日志。</returns>
        [HttpPost("action")]
        public async Task<ActionResult<ApiResponse<WorldBossActionResultDto>>> Action([FromBody] WorldBossActionRequestDto request)
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrWhiteSpace(playerId))
            {
                return Unauthorized(ApiResponse<WorldBossActionResultDto>.Fail("未登录"));
            }

            var result = await _worldBossService.ExecuteActionAsync(playerId, request ?? new WorldBossActionRequestDto());
            if (!result.Success)
            {
                return BadRequest(ApiResponse<WorldBossActionResultDto>.Fail(result.Message, result));
            }

            return Ok(ApiResponse<WorldBossActionResultDto>.Ok(result, result.Message));
        }

        /// <summary>
        /// 切换当前玩家的自动战斗状态。
        /// </summary>
        /// <param name="request">自动战斗切换请求。</param>
        /// <returns>切换后的玩家战斗状态。</returns>
        [HttpPost("auto")]
        public async Task<ActionResult<ApiResponse<WorldBossParticipantStatusDto>>> ToggleAuto([FromBody] WorldBossAutoToggleRequestDto request)
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrWhiteSpace(playerId))
            {
                return Unauthorized(ApiResponse<WorldBossParticipantStatusDto>.Fail("未登录"));
            }

            try
            {
                var result = await _worldBossService.ToggleAutoAsync(playerId, request?.Enabled ?? false);
                return Ok(ApiResponse<WorldBossParticipantStatusDto>.Ok(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<WorldBossParticipantStatusDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 领取当前玩家未领取的世界 Boss 奖励。
        /// </summary>
        /// <returns>奖励领取结果。</returns>
        [HttpPost("claim")]
        public async Task<ActionResult<ApiResponse<WorldBossRewardClaimDto>>> Claim()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrWhiteSpace(playerId))
            {
                return Unauthorized(ApiResponse<WorldBossRewardClaimDto>.Fail("未登录"));
            }

            var result = await _worldBossService.ClaimRewardAsync(playerId);
            if (result == null)
            {
                return NotFound(ApiResponse<WorldBossRewardClaimDto>.Fail("当前没有可领取的世界Boss奖励。"));
            }

            return Ok(ApiResponse<WorldBossRewardClaimDto>.Ok(result, result.Message));
        }
    }
}
