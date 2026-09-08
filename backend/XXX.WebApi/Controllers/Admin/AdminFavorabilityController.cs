#pragma warning disable CS1591
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.Application.Security;

namespace XXX.WebApi.Controllers.Admin
{
    /// <summary>
    /// 好感度后台管理控制器。
    /// </summary>
    [ApiController]
    [Route("api/admin/favorability")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class AdminFavorabilityController : ControllerBase
    {
        private readonly IFavorabilityService _service;

        public AdminFavorabilityController(IFavorabilityService service)
        {
            _service = service;
        }

        /// <summary>
        /// 获取好感度关系列表。
        /// </summary>
        [HttpGet("relations")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<AdminFavorabilityRelationDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PagedResult<AdminFavorabilityRelationDto>>>> GetRelations(
            [FromQuery] string? keyword, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 20)
        {
            var list = await _service.AdminGetRelationsAsync(keyword, pageIndex, pageSize);
            var total = await _service.AdminGetRelationsCountAsync(keyword);
            return Ok(ApiResponse<PagedResult<AdminFavorabilityRelationDto>>.Ok(
                new PagedResult<AdminFavorabilityRelationDto> { Items = list, Total = total, Page = pageIndex, PageSize = pageSize }));
        }

        /// <summary>
        /// 修改好感度数值。
        /// </summary>
        [HttpPut("relations/{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateRelation(long id, [FromBody] AdminFavorabilityUpdateRequest request)
        {
            var result = await _service.AdminUpdateValueAsync(id, request.NewValue);
            if (!result) return NotFound(ApiResponse.Fail("关系不存在"));
            return Ok(ApiResponse<bool>.Ok(true, "修改成功"));
        }

        /// <summary>
        /// 删除好感度关系。
        /// </summary>
        [HttpDelete("relations/{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteRelation(long id)
        {
            var result = await _service.AdminDeleteRelationAsync(id);
            if (!result) return NotFound(ApiResponse.Fail("关系不存在"));
            return Ok(ApiResponse<bool>.Ok(true, "删除成功"));
        }

        /// <summary>
        /// 获取赠送记录。
        /// </summary>
        [HttpGet("gift-log")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<AdminFavorabilityGiftLogDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PagedResult<AdminFavorabilityGiftLogDto>>>> GetGiftLog(
            [FromQuery] string? playerId, [FromQuery] string? targetPlayerId,
            [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 20)
        {
            var log = await _service.AdminGetGiftLogAsync(playerId, targetPlayerId, pageIndex, pageSize);
            var total = await _service.AdminGetGiftLogCountAsync(playerId, targetPlayerId);
            return Ok(ApiResponse<PagedResult<AdminFavorabilityGiftLogDto>>.Ok(
                new PagedResult<AdminFavorabilityGiftLogDto> { Items = log, Total = total, Page = pageIndex, PageSize = pageSize }));
        }

        /// <summary>
        /// 获取好感度统计。
        /// </summary>
        [HttpGet("stats")]
        [ProducesResponseType(typeof(ApiResponse<AdminFavorabilityStatsDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<AdminFavorabilityStatsDto>>> GetStats()
        {
            var stats = await _service.AdminGetStatsAsync();
            return Ok(ApiResponse<AdminFavorabilityStatsDto>.Ok(stats));
        }

        /// <summary>
        /// 获取好感度等级配置。
        /// </summary>
        [HttpGet("levels")]
        [ProducesResponseType(typeof(ApiResponse<List<FavorabilityLevelDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<FavorabilityLevelDto>>>> GetLevels()
        {
            var levels = await _service.AdminGetLevelsAsync();
            return Ok(ApiResponse<List<FavorabilityLevelDto>>.Ok(levels));
        }

        /// <summary>
        /// 保存好感度等级配置。
        /// </summary>
        [HttpPost("levels")]
        [ProducesResponseType(typeof(ApiResponse<FavorabilityLevelDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<FavorabilityLevelDto>>> SaveLevel([FromBody] FavorabilityLevelDto dto)
        {
            var result = await _service.AdminSaveLevelAsync(dto);
            if (result == null) return BadRequest(ApiResponse.Fail("保存失败"));
            return Ok(ApiResponse<FavorabilityLevelDto>.Ok(result, "保存成功"));
        }

        /// <summary>
        /// 删除好感度等级配置。
        /// </summary>
        [HttpDelete("levels/{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteLevel(int id)
        {
            var result = await _service.AdminDeleteLevelAsync(id);
            if (!result) return NotFound(ApiResponse.Fail("等级不存在"));
            return Ok(ApiResponse<bool>.Ok(true, "删除成功"));
        }

        /// <summary>
        /// 获取好感度排行榜。
        /// </summary>
        [HttpGet("leaderboard")]
        [ProducesResponseType(typeof(ApiResponse<List<FavorabilityLeaderboardDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<FavorabilityLeaderboardDto>>>> GetLeaderboard([FromQuery] int count = 20)
        {
            var list = await _service.GetLeaderboardAsync(count);
            return Ok(ApiResponse<List<FavorabilityLeaderboardDto>>.Ok(list));
        }

        /// <summary>
        /// 获取深度友谊排行榜。
        /// </summary>
        [HttpGet("deep-friendship")]
        [ProducesResponseType(typeof(ApiResponse<List<DeepFriendshipDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<DeepFriendshipDto>>>> GetDeepFriendship([FromQuery] int count = 10)
        {
            var list = await _service.GetDeepFriendshipLeaderboardAsync(count);
            return Ok(ApiResponse<List<DeepFriendshipDto>>.Ok(list));
        }
    }
}

#pragma warning restore CS1591
