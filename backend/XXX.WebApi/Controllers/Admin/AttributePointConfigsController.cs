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
    [Route("api/admin/attribute-point-configs")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class AttributePointConfigsController : ControllerBase
    {
        private readonly IAdminAttributePointConfigService _service;

        public AttributePointConfigsController(IAdminAttributePointConfigService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<AdminAttributePointConfigBundleDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<AdminAttributePointConfigBundleDto>>> Get()
        {
            var config = await _service.GetConfigAsync();
            return Ok(ApiResponse<AdminAttributePointConfigBundleDto>.Ok(config));
        }

        [HttpPost]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminAttributePointConfigBundleDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<AdminAttributePointConfigBundleDto>>> Save([FromBody] AdminAttributePointConfigBundleDto request)
        {
            try
            {
                var config = await _service.SaveConfigAsync(request);
                return Ok(ApiResponse<AdminAttributePointConfigBundleDto>.Ok(config, "属性点配置保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminAttributePointConfigBundleDto>.Fail(ex.Message));
            }
        }
    }
}
#pragma warning restore CS1591
