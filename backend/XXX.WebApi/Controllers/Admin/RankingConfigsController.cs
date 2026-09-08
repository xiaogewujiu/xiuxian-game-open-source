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
    [Route("api/admin/ranking-configs")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class RankingConfigsController : ControllerBase
    {
        private readonly IAdminRankingService _adminRankingService;

        public RankingConfigsController(IAdminRankingService adminRankingService)
        {
            _adminRankingService = adminRankingService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminRankingConfigListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminRankingConfigListItemDto>>>> GetList([FromQuery] string? keyword = null)
        {
            var configs = await _adminRankingService.GetConfigsAsync(keyword);
            return Ok(ApiResponse<List<AdminRankingConfigListItemDto>>.Ok(configs));
        }

        [HttpGet("{rankingId}")]
        [ProducesResponseType(typeof(ApiResponse<AdminRankingConfigDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminRankingConfigDetailDto>>> GetDetail(string rankingId)
        {
            var config = await _adminRankingService.GetConfigDetailAsync(rankingId);
            if (config == null)
            {
                return NotFound(ApiResponse<AdminRankingConfigDetailDto>.Fail("排行配置不存在。"));
            }

            return Ok(ApiResponse<AdminRankingConfigDetailDto>.Ok(config));
        }

        [HttpPost]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminRankingConfigDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminRankingConfigDetailDto>>> Save([FromBody] AdminRankingConfigDetailDto request)
        {
            try
            {
                var config = await _adminRankingService.SaveConfigAsync(request);
                return Ok(ApiResponse<AdminRankingConfigDetailDto>.Ok(config, "排行配置保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminRankingConfigDetailDto>.Fail(ex.Message));
            }
        }

        [HttpDelete("{rankingId}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> Delete(string rankingId)
        {
            try
            {
                var deleted = await _adminRankingService.DeleteConfigAsync(rankingId);
                if (!deleted)
                {
                    return NotFound(ApiResponse.Fail("排行配置不存在或删除失败。"));
                }

                return Ok(ApiResponse.Ok("排行配置删除成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }
    }
}

#pragma warning restore CS1591

