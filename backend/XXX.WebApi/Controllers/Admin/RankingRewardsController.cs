#pragma warning disable CS1591
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.Application.Security;

namespace XXX.WebApi.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/ranking-rewards")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class RankingRewardsController : ControllerBase
    {
        private readonly IAdminRankingService _adminRankingService;

        public RankingRewardsController(IAdminRankingService adminRankingService)
        {
            _adminRankingService = adminRankingService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminRankingRewardListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminRankingRewardListItemDto>>>> GetList([FromQuery] string? rankingId = null)
        {
            var rewards = await _adminRankingService.GetRewardsAsync(rankingId);
            return Ok(ApiResponse<List<AdminRankingRewardListItemDto>>.Ok(rewards));
        }

        [HttpGet("{gid}")]
        [ProducesResponseType(typeof(ApiResponse<AdminRankingRewardDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminRankingRewardDetailDto>>> GetDetail(string gid)
        {
            var reward = await _adminRankingService.GetRewardDetailAsync(gid);
            if (reward == null)
            {
                return NotFound(ApiResponse<AdminRankingRewardDetailDto>.Fail("排行奖励不存在。"));
            }

            return Ok(ApiResponse<AdminRankingRewardDetailDto>.Ok(reward));
        }

        [HttpPost]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminRankingRewardDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminRankingRewardDetailDto>>> Save([FromBody] AdminRankingRewardDetailDto request)
        {
            try
            {
                var reward = await _adminRankingService.SaveRewardAsync(request);
                return Ok(ApiResponse<AdminRankingRewardDetailDto>.Ok(reward, "排行奖励保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminRankingRewardDetailDto>.Fail(ex.Message));
            }
        }

        [HttpDelete("{gid}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> Delete(string gid)
        {
            try
            {
                var deleted = await _adminRankingService.DeleteRewardAsync(gid);
                if (!deleted)
                {
                    return NotFound(ApiResponse.Fail("排行奖励不存在或删除失败。"));
                }

                return Ok(ApiResponse.Ok("排行奖励删除成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }
    }
}
#pragma warning restore CS1591
