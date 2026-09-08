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
    [Route("api/admin/element-rules")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class ElementRelationRulesController : ControllerBase
    {
        private readonly IAdminElementRelationRuleService _service;

        public ElementRelationRulesController(IAdminElementRelationRuleService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminElementRelationEntryDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminElementRelationEntryDto>>>> GetList()
        {
            var entries = await _service.GetEntriesAsync();
            return Ok(ApiResponse<List<AdminElementRelationEntryDto>>.Ok(entries));
        }

        [HttpPost]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<List<AdminElementRelationEntryDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminElementRelationEntryDto>>>> Save([FromBody] List<AdminElementRelationEntryDto> entries)
        {
            try
            {
                var result = await _service.SaveEntriesAsync(entries);
                return Ok(ApiResponse<List<AdminElementRelationEntryDto>>.Ok(result, "元素克制矩阵保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<List<AdminElementRelationEntryDto>>.Fail(ex.Message));
            }
        }
    }
}
#pragma warning restore CS1591
