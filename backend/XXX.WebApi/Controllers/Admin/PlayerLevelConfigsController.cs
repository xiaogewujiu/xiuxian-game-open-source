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
    [Route("api/admin/player-level-configs")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class PlayerLevelConfigsController : ControllerBase
    {
        private readonly IAdminPlayerLevelConfigService _service;

        public PlayerLevelConfigsController(IAdminPlayerLevelConfigService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminPlayerLevelConfigListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminPlayerLevelConfigListItemDto>>>> GetList()
        {
            var items = await _service.GetListAsync();
            return Ok(ApiResponse<List<AdminPlayerLevelConfigListItemDto>>.Ok(items));
        }

        [HttpGet("{level:int}")]
        [ProducesResponseType(typeof(ApiResponse<AdminPlayerLevelConfigDetailDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<AdminPlayerLevelConfigDetailDto>>> GetDetail(int level)
        {
            var detail = await _service.GetDetailAsync(level);
            if (detail == null)
            {
                return NotFound(ApiResponse<AdminPlayerLevelConfigDetailDto>.Fail("等级成长配置不存在。"));
            }

            return Ok(ApiResponse<AdminPlayerLevelConfigDetailDto>.Ok(detail));
        }

        [HttpPost]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminPlayerLevelConfigDetailDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<AdminPlayerLevelConfigDetailDto>>> Save([FromBody] AdminPlayerLevelConfigDetailDto request)
        {
            try
            {
                var detail = await _service.SaveAsync(request);
                return Ok(ApiResponse<AdminPlayerLevelConfigDetailDto>.Ok(detail, "等级成长配置保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminPlayerLevelConfigDetailDto>.Fail(ex.Message));
            }
        }
    }
}
#pragma warning restore CS1591
