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
    [Route("api/admin/maps")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class MapsController : ControllerBase
    {
        private readonly IAdminMapService _adminMapService;

        public MapsController(IAdminMapService adminMapService)
        {
            _adminMapService = adminMapService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminMapListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminMapListItemDto>>>> GetList([FromQuery] string? keyword = null, [FromQuery] string? mapKind = null)
        {
            var maps = await _adminMapService.GetListAsync(keyword, mapKind);
            return Ok(ApiResponse<List<AdminMapListItemDto>>.Ok(maps));
        }

        [HttpGet("{mapId}")]
        [ProducesResponseType(typeof(ApiResponse<AdminMapDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminMapDetailDto>>> GetDetail(string mapId)
        {
            var map = await _adminMapService.GetDetailAsync(mapId);
            if (map == null)
            {
                return NotFound(ApiResponse<AdminMapDetailDto>.Fail("地图不存在。"));
            }

            return Ok(ApiResponse<AdminMapDetailDto>.Ok(map));
        }

        [HttpPost]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminMapDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminMapDetailDto>>> Save([FromBody] AdminSaveMapRequestDto request)
        {
            try
            {
                var map = await _adminMapService.SaveAsync(request);
                return Ok(ApiResponse<AdminMapDetailDto>.Ok(map, "地图保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminMapDetailDto>.Fail(ex.Message));
            }
        }

        [HttpDelete("{mapId}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> Delete(string mapId)
        {
            try
            {
                var deleted = await _adminMapService.DeleteAsync(mapId);
                if (!deleted)
                {
                    return NotFound(ApiResponse.Fail("地图不存在或删除失败。"));
                }

                return Ok(ApiResponse.Ok("地图删除成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }
    }
}

#pragma warning restore CS1591

