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
    [Route("api/admin/crops")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class CropsController : ControllerBase
    {
        private readonly IAdminCropService _adminCropService;

        public CropsController(IAdminCropService adminCropService)
        {
            _adminCropService = adminCropService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminCropListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminCropListItemDto>>>> GetList([FromQuery] string? keyword = null)
        {
            var crops = await _adminCropService.GetListAsync(keyword);
            return Ok(ApiResponse<List<AdminCropListItemDto>>.Ok(crops));
        }

        [HttpGet("{templateId}")]
        [ProducesResponseType(typeof(ApiResponse<AdminCropDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminCropDetailDto>>> GetDetail(string templateId)
        {
            var crop = await _adminCropService.GetDetailAsync(templateId);
            if (crop == null)
            {
                return NotFound(ApiResponse<AdminCropDetailDto>.Fail("作物模板不存在。"));
            }

            return Ok(ApiResponse<AdminCropDetailDto>.Ok(crop));
        }

        [HttpPost]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminCropDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminCropDetailDto>>> Save([FromBody] AdminCropDetailDto request)
        {
            try
            {
                var crop = await _adminCropService.SaveAsync(request);
                return Ok(ApiResponse<AdminCropDetailDto>.Ok(crop, "作物模板保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminCropDetailDto>.Fail(ex.Message));
            }
        }

        [HttpDelete("{templateId}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> Delete(string templateId)
        {
            try
            {
                var deleted = await _adminCropService.DeleteAsync(templateId);
                if (!deleted)
                {
                    return NotFound(ApiResponse.Fail("作物模板不存在或删除失败。"));
                }

                return Ok(ApiResponse.Ok("作物模板删除成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }
    }
}

#pragma warning restore CS1591

