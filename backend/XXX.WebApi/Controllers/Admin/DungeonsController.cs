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
    [Route("api/admin/dungeons")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class DungeonsController : ControllerBase
    {
        private readonly IAdminDungeonService _adminDungeonService;

        public DungeonsController(IAdminDungeonService adminDungeonService)
        {
            _adminDungeonService = adminDungeonService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminDungeonListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminDungeonListItemDto>>>> GetList([FromQuery] string? keyword = null)
        {
            var dungeons = await _adminDungeonService.GetListAsync(keyword);
            return Ok(ApiResponse<List<AdminDungeonListItemDto>>.Ok(dungeons));
        }

        [HttpGet("{dungeonId}")]
        [ProducesResponseType(typeof(ApiResponse<AdminDungeonDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminDungeonDetailDto>>> GetDetail(string dungeonId)
        {
            var dungeon = await _adminDungeonService.GetDetailAsync(dungeonId);
            if (dungeon == null)
            {
                return NotFound(ApiResponse<AdminDungeonDetailDto>.Fail("副本模板不存在。"));
            }

            return Ok(ApiResponse<AdminDungeonDetailDto>.Ok(dungeon));
        }

        [HttpPost]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminDungeonDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminDungeonDetailDto>>> Save([FromBody] AdminDungeonDetailDto request)
        {
            try
            {
                var dungeon = await _adminDungeonService.SaveAsync(request);
                return Ok(ApiResponse<AdminDungeonDetailDto>.Ok(dungeon, "副本模板保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminDungeonDetailDto>.Fail(ex.Message));
            }
        }

        [HttpDelete("{dungeonId}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> Delete(string dungeonId)
        {
            try
            {
                var deleted = await _adminDungeonService.DeleteAsync(dungeonId);
                if (!deleted)
                {
                    return NotFound(ApiResponse.Fail("副本模板不存在或删除失败。"));
                }

                return Ok(ApiResponse.Ok("副本模板删除成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }
    }
}

#pragma warning restore CS1591

