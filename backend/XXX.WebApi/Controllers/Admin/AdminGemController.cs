using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.Application.Security;

namespace XXX.WebApi.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/gem")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class AdminGemController : ControllerBase
    {
        private readonly IAdminGemService _adminGemService;

        public AdminGemController(IAdminGemService adminGemService)
        {
            _adminGemService = adminGemService;
        }

        [HttpGet("templates")]
        [ProducesResponseType(typeof(ApiResponse<List<AdminGemListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminGemListItemDto>>>> GetList()
        {
            var result = await _adminGemService.GetListAsync();
            return Ok(ApiResponse<List<AdminGemListItemDto>>.Ok(result));
        }

        [HttpGet("templates/{id:long}")]
        [ProducesResponseType(typeof(ApiResponse<AdminGemDetailDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<AdminGemDetailDto>>> GetDetail(long id)
        {
            var result = await _adminGemService.GetDetailAsync(id);
            if (result == null) return NotFound(ApiResponse<AdminGemDetailDto>.Fail("宝石模板不存在。"));
            return Ok(ApiResponse<AdminGemDetailDto>.Ok(result));
        }

        [HttpPost("templates")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse>> Create([FromBody] AdminGemSaveDto dto)
        {
            await _adminGemService.CreateAsync(dto);
            return Ok(ApiResponse.Ok("宝石模板已创建。"));
        }

        [HttpPut("templates/{id:long}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse>> Update(long id, [FromBody] AdminGemSaveDto dto)
        {
            try
            {
                await _adminGemService.UpdateAsync(id, dto);
                return Ok(ApiResponse.Ok("宝石模板已更新。"));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse.Fail(ex.Message));
            }
        }

        [HttpDelete("templates/{id:long}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse>> Delete(long id)
        {
            await _adminGemService.DeleteAsync(id);
            return Ok(ApiResponse.Ok("宝石模板已删除。"));
        }

        [HttpPost("batch-generate")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<int>>> BatchGenerate([FromBody] AdminGemBatchGenerateDto dto)
        {
            var count = await _adminGemService.BatchGenerateAsync(dto);
            return Ok(ApiResponse<int>.Ok(count, $"生成了 {count} 个宝石模板。"));
        }
    }
}
