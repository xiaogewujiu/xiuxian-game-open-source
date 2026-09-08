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
    [Route("api/admin/realm-level-configs")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class RealmLevelConfigsController : ControllerBase
    {
        private readonly IAdminRealmLevelConfigService _service;

        public RealmLevelConfigsController(IAdminRealmLevelConfigService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminRealmLevelConfigListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminRealmLevelConfigListItemDto>>>> GetList()
        {
            var items = await _service.GetListAsync();
            return Ok(ApiResponse<List<AdminRealmLevelConfigListItemDto>>.Ok(items));
        }

        [HttpGet("{level:int}")]
        [ProducesResponseType(typeof(ApiResponse<AdminRealmLevelConfigDetailDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<AdminRealmLevelConfigDetailDto>>> GetDetail(int level)
        {
            var detail = await _service.GetDetailAsync(level);
            if (detail == null)
            {
                return NotFound(ApiResponse<AdminRealmLevelConfigDetailDto>.Fail("境界配置不存在。"));
            }

            return Ok(ApiResponse<AdminRealmLevelConfigDetailDto>.Ok(detail));
        }

        [HttpPost]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminRealmLevelConfigDetailDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<AdminRealmLevelConfigDetailDto>>> Save([FromBody] AdminRealmLevelConfigDetailDto request)
        {
            try
            {
                var detail = await _service.SaveAsync(request);
                return Ok(ApiResponse<AdminRealmLevelConfigDetailDto>.Ok(detail, "境界配置保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminRealmLevelConfigDetailDto>.Fail(ex.Message));
            }
        }
    }
}
#pragma warning restore CS1591
