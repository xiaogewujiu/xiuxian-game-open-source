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
    [Route("api/admin/buffs")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class BuffsController : ControllerBase
    {
        private readonly IAdminBuffService _adminBuffService;

        public BuffsController(IAdminBuffService adminBuffService)
        {
            _adminBuffService = adminBuffService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminBuffListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminBuffListItemDto>>>> GetList([FromQuery] string? keyword = null)
        {
            var buffs = await _adminBuffService.GetListAsync(keyword);
            return Ok(ApiResponse<List<AdminBuffListItemDto>>.Ok(buffs));
        }

        [HttpGet("{buffId}")]
        [ProducesResponseType(typeof(ApiResponse<AdminBuffDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminBuffDetailDto>>> GetDetail(string buffId)
        {
            var buff = await _adminBuffService.GetDetailAsync(buffId);
            if (buff == null)
            {
                return NotFound(ApiResponse<AdminBuffDetailDto>.Fail("Buff 模板不存在。"));
            }

            return Ok(ApiResponse<AdminBuffDetailDto>.Ok(buff));
        }

        [HttpPost]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminBuffDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminBuffDetailDto>>> Save([FromBody] AdminBuffDetailDto request)
        {
            try
            {
                var buff = await _adminBuffService.SaveAsync(request);
                return Ok(ApiResponse<AdminBuffDetailDto>.Ok(buff, "Buff 模板保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminBuffDetailDto>.Fail(ex.Message));
            }
        }

        [HttpDelete("{buffId}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> Delete(string buffId)
        {
            try
            {
                var deleted = await _adminBuffService.DeleteAsync(buffId);
                if (!deleted)
                {
                    return NotFound(ApiResponse.Fail("Buff 模板不存在或删除失败。"));
                }

                return Ok(ApiResponse.Ok("Buff 模板删除成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }
    }
}

#pragma warning restore CS1591

