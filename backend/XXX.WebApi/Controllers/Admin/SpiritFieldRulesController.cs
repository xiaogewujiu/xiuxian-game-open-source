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
    [Route("api/admin/spirit-field-rules")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class SpiritFieldRulesController : ControllerBase
    {
        private readonly IAdminSpiritFieldRuleService _ruleService;

        public SpiritFieldRulesController(IAdminSpiritFieldRuleService ruleService)
        {
            _ruleService = ruleService;
        }

        [HttpGet("system")]
        [ProducesResponseType(typeof(ApiResponse<AdminSpiritFieldSystemRuleDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<AdminSpiritFieldSystemRuleDto>>> GetSystemRule()
        {
            var detail = await _ruleService.GetSystemRuleAsync();
            if (detail == null)
            {
                return NotFound(ApiResponse<AdminSpiritFieldSystemRuleDto>.Fail("灵田系统规则不存在。"));
            }

            return Ok(ApiResponse<AdminSpiritFieldSystemRuleDto>.Ok(detail));
        }

        [HttpPost("system")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminSpiritFieldSystemRuleDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<AdminSpiritFieldSystemRuleDto>>> SaveSystemRule([FromBody] AdminSpiritFieldSystemRuleDto request)
        {
            try
            {
                var detail = await _ruleService.SaveSystemRuleAsync(request);
                return Ok(ApiResponse<AdminSpiritFieldSystemRuleDto>.Ok(detail, "灵田系统规则保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminSpiritFieldSystemRuleDto>.Fail(ex.Message));
            }
        }

        [HttpGet("speedup-items")]
        [ProducesResponseType(typeof(ApiResponse<List<AdminSpiritFieldSpeedUpItemRuleDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminSpiritFieldSpeedUpItemRuleDto>>>> GetSpeedUpRules()
        {
            var items = await _ruleService.GetSpeedUpRulesAsync();
            return Ok(ApiResponse<List<AdminSpiritFieldSpeedUpItemRuleDto>>.Ok(items));
        }

        [HttpGet("speedup-items/{itemId}")]
        [ProducesResponseType(typeof(ApiResponse<AdminSpiritFieldSpeedUpItemRuleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminSpiritFieldSpeedUpItemRuleDto>>> GetSpeedUpRule(string itemId)
        {
            var detail = await _ruleService.GetSpeedUpRuleAsync(itemId);
            if (detail == null)
            {
                return NotFound(ApiResponse<AdminSpiritFieldSpeedUpItemRuleDto>.Fail("灵田催熟规则不存在。"));
            }

            return Ok(ApiResponse<AdminSpiritFieldSpeedUpItemRuleDto>.Ok(detail));
        }

        [HttpPost("speedup-items")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminSpiritFieldSpeedUpItemRuleDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<AdminSpiritFieldSpeedUpItemRuleDto>>> SaveSpeedUpRule([FromBody] AdminSpiritFieldSpeedUpItemRuleDto request)
        {
            try
            {
                var detail = await _ruleService.SaveSpeedUpRuleAsync(request);
                return Ok(ApiResponse<AdminSpiritFieldSpeedUpItemRuleDto>.Ok(detail, "灵田催熟规则保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminSpiritFieldSpeedUpItemRuleDto>.Fail(ex.Message));
            }
        }

        [HttpDelete("speedup-items/{itemId}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse>> DeleteSpeedUpRule(string itemId)
        {
            var deleted = await _ruleService.DeleteSpeedUpRuleAsync(itemId);
            if (!deleted)
            {
                return NotFound(ApiResponse.Fail("灵田催熟规则不存在或删除失败。"));
            }

            return Ok(ApiResponse.Ok("灵田催熟规则删除成功"));
        }
    }
}
#pragma warning restore CS1591
