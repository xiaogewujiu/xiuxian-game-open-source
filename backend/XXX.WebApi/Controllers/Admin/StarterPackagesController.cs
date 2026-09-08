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
    [Route("api/admin/starter-packages")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class StarterPackagesController : ControllerBase
    {
        private readonly IAdminStarterPackageService _adminStarterPackageService;

        public StarterPackagesController(IAdminStarterPackageService adminStarterPackageService)
        {
            _adminStarterPackageService = adminStarterPackageService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminStarterPackageListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminStarterPackageListItemDto>>>> GetList([FromQuery] string? keyword = null, [FromQuery] bool? autoGrantOnRegister = null)
        {
            var packages = await _adminStarterPackageService.GetListAsync(keyword, autoGrantOnRegister);
            return Ok(ApiResponse<List<AdminStarterPackageListItemDto>>.Ok(packages));
        }

        [HttpGet("{packageId}")]
        [ProducesResponseType(typeof(ApiResponse<AdminStarterPackageDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminStarterPackageDetailDto>>> GetDetail(string packageId)
        {
            var detail = await _adminStarterPackageService.GetDetailAsync(packageId);
            if (detail == null)
            {
                return NotFound(ApiResponse<AdminStarterPackageDetailDto>.Fail("新手礼包不存在。"));
            }

            return Ok(ApiResponse<AdminStarterPackageDetailDto>.Ok(detail));
        }

        [HttpPost]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminStarterPackageDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminStarterPackageDetailDto>>> Save([FromBody] AdminStarterPackageDetailDto request)
        {
            try
            {
                var operatorName = User.Identity?.Name;
                var detail = await _adminStarterPackageService.SaveAsync(request, operatorName);
                return Ok(ApiResponse<AdminStarterPackageDetailDto>.Ok(detail, "新手礼包保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminStarterPackageDetailDto>.Fail(ex.Message));
            }
        }

        [HttpDelete("{packageId}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> Delete(string packageId)
        {
            try
            {
                var deleted = await _adminStarterPackageService.DeleteAsync(packageId);
                if (!deleted)
                {
                    return NotFound(ApiResponse.Fail("新手礼包不存在或删除失败。"));
                }

                return Ok(ApiResponse.Ok("新手礼包删除成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }
    }
}
#pragma warning restore CS1591
