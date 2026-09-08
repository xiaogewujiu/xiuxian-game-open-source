using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.Application.Security;

namespace XXX.WebApi.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/tower")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class AdminTowerController : ControllerBase
    {
        private readonly IAdminTowerService _adminTowerService;

        public AdminTowerController(IAdminTowerService adminTowerService)
        {
            _adminTowerService = adminTowerService;
        }

        [HttpGet("overview")]
        [ProducesResponseType(typeof(ApiResponse<AdminTowerOverviewDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<AdminTowerOverviewDto>>> GetOverview()
        {
            var result = await _adminTowerService.GetOverviewAsync();
            return Ok(ApiResponse<AdminTowerOverviewDto>.Ok(result));
        }

        [HttpGet("players")]
        [ProducesResponseType(typeof(ApiResponse<List<AdminTowerPlayerDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminTowerPlayerDto>>>> GetPlayers(
            [FromQuery] string? keyword = null,
            [FromQuery] int take = 200)
        {
            var result = await _adminTowerService.GetPlayersAsync(keyword, take);
            return Ok(ApiResponse<List<AdminTowerPlayerDto>>.Ok(result));
        }

        [HttpGet("floors")]
        [ProducesResponseType(typeof(ApiResponse<List<AdminTowerFloorConfigDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminTowerFloorConfigDto>>>> GetFloorConfigs()
        {
            var result = await _adminTowerService.GetFloorConfigsAsync();
            return Ok(ApiResponse<List<AdminTowerFloorConfigDto>>.Ok(result));
        }

        [HttpPut("floors/{id:long}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> UpdateFloorConfig(long id, [FromBody] AdminTowerFloorConfigDto dto)
        {
            var success = await _adminTowerService.UpdateFloorConfigAsync(id, dto);
            if (!success) return NotFound(ApiResponse.Fail("楼层配置不存在。"));

            return Ok(ApiResponse.Ok("楼层配置已更新。"));
        }

        [HttpPost("floors/batch-generate")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<int>>> BatchGenerate([FromBody] AdminTowerBatchGenerateDto dto)
        {
            var count = await _adminTowerService.BatchGenerateFloorsAsync(dto);
            return Ok(ApiResponse<int>.Ok(count, $"生成了 {count} 层配置。"));
        }

        [HttpGet("distribution")]
        [ProducesResponseType(typeof(ApiResponse<List<TowerFloorDistributionDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<TowerFloorDistributionDto>>>> GetFloorDistribution()
        {
            var result = await _adminTowerService.GetFloorDistributionAsync();
            return Ok(ApiResponse<List<TowerFloorDistributionDto>>.Ok(result));
        }
    }
}
