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
    [Route("api/admin/monsters")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class MonstersController : ControllerBase
    {
        private readonly IAdminMonsterService _adminMonsterService;

        public MonstersController(IAdminMonsterService adminMonsterService)
        {
            _adminMonsterService = adminMonsterService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminMonsterListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminMonsterListItemDto>>>> GetList([FromQuery] string? keyword = null)
        {
            var monsters = await _adminMonsterService.GetListAsync(keyword);
            return Ok(ApiResponse<List<AdminMonsterListItemDto>>.Ok(monsters));
        }

        [HttpGet("{monsterId}")]
        [ProducesResponseType(typeof(ApiResponse<AdminMonsterDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminMonsterDetailDto>>> GetDetail(string monsterId)
        {
            var monster = await _adminMonsterService.GetDetailAsync(monsterId);
            if (monster == null)
            {
                return NotFound(ApiResponse<AdminMonsterDetailDto>.Fail("怪物模板不存在。"));
            }

            return Ok(ApiResponse<AdminMonsterDetailDto>.Ok(monster));
        }

        [HttpPost]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminMonsterDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminMonsterDetailDto>>> Save([FromBody] AdminSaveMonsterRequestDto request)
        {
            try
            {
                var monster = await _adminMonsterService.SaveAsync(request);
                return Ok(ApiResponse<AdminMonsterDetailDto>.Ok(monster, "怪物模板保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminMonsterDetailDto>.Fail(ex.Message));
            }
        }

        [HttpDelete("{monsterId}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> Delete(string monsterId)
        {
            try
            {
                var deleted = await _adminMonsterService.DeleteAsync(monsterId);
                if (!deleted)
                {
                    return NotFound(ApiResponse.Fail("怪物模板不存在或删除失败。"));
                }

                return Ok(ApiResponse.Ok("怪物模板删除成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }
    }
}

#pragma warning restore CS1591

