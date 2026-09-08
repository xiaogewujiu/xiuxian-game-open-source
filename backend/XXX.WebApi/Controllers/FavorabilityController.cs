using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.WebApi.Extensions;

namespace XXX.WebApi.Controllers
{
    /// <summary>
    /// 好感度控制器。
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FavorabilityController : ControllerBase
    {
        private readonly IFavorabilityService _favorabilityService;

        public FavorabilityController(IFavorabilityService favorabilityService)
        {
            _favorabilityService = favorabilityService;
        }

        private string? GetCurrentPlayerId() => User.GetCurrentPlayerId();

        /// <summary>
        /// 赠送好感度道具。
        /// </summary>
        [HttpPost("gift")]
        public async Task<ActionResult<ApiResponse<GiftFavorabilityResponse>>> Gift([FromBody] GiftFavorabilityRequest request)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _favorabilityService.GiftItemAsync(playerId, request);
            if (!result.Success)
                return BadRequest(ApiResponse<GiftFavorabilityResponse>.Fail(result.Message));

            return Ok(ApiResponse<GiftFavorabilityResponse>.Ok(result, result.Message));
        }

        /// <summary>
        /// 获取好感度列表。
        /// </summary>
        [HttpGet("list")]
        public async Task<ActionResult<ApiResponse<List<FavorabilityListItemDto>>>> GetList([FromQuery] string direction = "ToOthers")
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var list = await _favorabilityService.GetFavorabilityListAsync(playerId, direction);
            return Ok(ApiResponse<List<FavorabilityListItemDto>>.Ok(list));
        }

        /// <summary>
        /// 获取赠送记录。
        /// </summary>
        [HttpGet("log")]
        public async Task<ActionResult<ApiResponse<List<FavorabilityGiftRecordDto>>>> GetLog([FromQuery] FavorabilityLogQueryRequest request)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var log = await _favorabilityService.GetGiftLogAsync(playerId, request);
            return Ok(ApiResponse<List<FavorabilityGiftRecordDto>>.Ok(log));
        }

        /// <summary>
        /// 获取好感度等级配置。
        /// </summary>
        [HttpGet("levels")]
        public async Task<ActionResult<ApiResponse<List<FavorabilityLevelDto>>>> GetLevels()
        {
            var levels = await _favorabilityService.GetLevelsAsync();
            return Ok(ApiResponse<List<FavorabilityLevelDto>>.Ok(levels));
        }

        /// <summary>
        /// 获取好感度排行榜。
        /// </summary>
        [HttpGet("leaderboard")]
        public async Task<ActionResult<ApiResponse<List<FavorabilityLeaderboardDto>>>> GetLeaderboard([FromQuery] int count = 20)
        {
            var list = await _favorabilityService.GetLeaderboardAsync(count);
            return Ok(ApiResponse<List<FavorabilityLeaderboardDto>>.Ok(list));
        }

        /// <summary>
        /// 获取深度友谊排行榜。
        /// </summary>
        [HttpGet("deep-friendship")]
        public async Task<ActionResult<ApiResponse<List<DeepFriendshipDto>>>> GetDeepFriendship([FromQuery] int count = 10)
        {
            var list = await _favorabilityService.GetDeepFriendshipLeaderboardAsync(count);
            return Ok(ApiResponse<List<DeepFriendshipDto>>.Ok(list));
        }

        /// <summary>
        /// 获取好感度成就进度。
        /// </summary>
        [HttpGet("achievements")]
        public async Task<ActionResult<ApiResponse<FavorabilityAchievementProgressDto>>> GetAchievements()
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _favorabilityService.GetAchievementProgressAsync(playerId);
            return Ok(ApiResponse<FavorabilityAchievementProgressDto>.Ok(result));
        }

        /// <summary>
        /// 领取好感度成就奖励。
        /// </summary>
        [HttpPost("achievements/{achievementId}/claim")]
        public async Task<ActionResult<ApiResponse<List<FavorabilityAchievementRewardDto>>>> ClaimAchievement(string achievementId)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _favorabilityService.ClaimAchievementRewardAsync(playerId, achievementId);
            if (!result[0].Success)
                return BadRequest(ApiResponse.Fail(result[0].Message));

            return Ok(ApiResponse<List<FavorabilityAchievementRewardDto>>.Ok(result, result[0].Message));
        }
    }
}
