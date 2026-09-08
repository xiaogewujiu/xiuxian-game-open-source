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
    [Route("api/admin/alchemy-systems")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class AlchemySystemsController : ControllerBase
    {
        private readonly IAdminAlchemyRuntimeService _adminAlchemyRuntimeService;

        public AlchemySystemsController(IAdminAlchemyRuntimeService adminAlchemyRuntimeService)
        {
            _adminAlchemyRuntimeService = adminAlchemyRuntimeService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminAlchemySystemListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminAlchemySystemListItemDto>>>> GetList([FromQuery] string? keyword = null)
        {
            var items = await _adminAlchemyRuntimeService.GetListAsync(keyword);
            return Ok(ApiResponse<List<AdminAlchemySystemListItemDto>>.Ok(items));
        }

        [HttpGet("{playerId}")]
        [ProducesResponseType(typeof(ApiResponse<AdminAlchemySystemDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminAlchemySystemDetailDto>>> GetDetail(string playerId)
        {
            var detail = await _adminAlchemyRuntimeService.GetDetailAsync(playerId);
            if (detail == null)
            {
                return NotFound(ApiResponse<AdminAlchemySystemDetailDto>.Fail("玩家不存在。"));
            }

            return Ok(ApiResponse<AdminAlchemySystemDetailDto>.Ok(detail));
        }

        [HttpPost]
        [Authorize(Policy = AdminPermissionCatalog.PlayerWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminAlchemySystemDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminAlchemySystemDetailDto>>> Save([FromBody] AdminAlchemySystemDetailDto request)
        {
            try
            {
                var detail = await _adminAlchemyRuntimeService.SaveAsync(request);
                return Ok(ApiResponse<AdminAlchemySystemDetailDto>.Ok(detail, "炼丹系统数据保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminAlchemySystemDetailDto>.Fail(ex.Message));
            }
        }

        [HttpPost("{playerId}/clear-task")]
        [Authorize(Policy = AdminPermissionCatalog.PlayerWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminAlchemySystemDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminAlchemySystemDetailDto>>> ClearTask(string playerId)
        {
            try
            {
                var detail = await _adminAlchemyRuntimeService.ClearActiveTaskAsync(playerId);
                return Ok(ApiResponse<AdminAlchemySystemDetailDto>.Ok(detail, "炼丹任务已清空"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminAlchemySystemDetailDto>.Fail(ex.Message));
            }
        }
    }
}
#pragma warning restore CS1591
