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
    [Route("api/admin/forge-systems")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class ForgeSystemsController : ControllerBase
    {
        private readonly IAdminForgeRuntimeService _adminForgeRuntimeService;

        public ForgeSystemsController(IAdminForgeRuntimeService adminForgeRuntimeService)
        {
            _adminForgeRuntimeService = adminForgeRuntimeService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminForgeSystemListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminForgeSystemListItemDto>>>> GetList([FromQuery] string? keyword = null)
        {
            var items = await _adminForgeRuntimeService.GetListAsync(keyword);
            return Ok(ApiResponse<List<AdminForgeSystemListItemDto>>.Ok(items));
        }

        [HttpGet("{playerId}")]
        [ProducesResponseType(typeof(ApiResponse<AdminForgeSystemDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminForgeSystemDetailDto>>> GetDetail(string playerId)
        {
            var detail = await _adminForgeRuntimeService.GetDetailAsync(playerId);
            if (detail == null)
            {
                return NotFound(ApiResponse<AdminForgeSystemDetailDto>.Fail("玩家不存在。"));
            }

            return Ok(ApiResponse<AdminForgeSystemDetailDto>.Ok(detail));
        }

        [HttpPost]
        [Authorize(Policy = AdminPermissionCatalog.PlayerWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminForgeSystemDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminForgeSystemDetailDto>>> Save([FromBody] AdminForgeSystemDetailDto request)
        {
            try
            {
                var detail = await _adminForgeRuntimeService.SaveAsync(request);
                return Ok(ApiResponse<AdminForgeSystemDetailDto>.Ok(detail, "锻造系统数据保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminForgeSystemDetailDto>.Fail(ex.Message));
            }
        }

        [HttpPost("{playerId}/clear-task")]
        [Authorize(Policy = AdminPermissionCatalog.PlayerWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminForgeSystemDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminForgeSystemDetailDto>>> ClearTask(string playerId)
        {
            try
            {
                var detail = await _adminForgeRuntimeService.ClearActiveTaskAsync(playerId);
                return Ok(ApiResponse<AdminForgeSystemDetailDto>.Ok(detail, "锻造任务已清空"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminForgeSystemDetailDto>.Fail(ex.Message));
            }
        }
    }
}
#pragma warning restore CS1591
