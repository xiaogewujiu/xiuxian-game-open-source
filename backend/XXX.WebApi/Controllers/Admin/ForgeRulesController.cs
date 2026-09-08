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
    [Route("api/admin/forge-rules")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class ForgeRulesController : ControllerBase
    {
        private readonly IAdminForgeRuleService _ruleService;

        public ForgeRulesController(IAdminForgeRuleService ruleService)
        {
            _ruleService = ruleService;
        }

        [HttpGet("levels")]
        [ProducesResponseType(typeof(ApiResponse<List<AdminForgeProfessionLevelRuleDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminForgeProfessionLevelRuleDto>>>> GetLevels()
        {
            var items = await _ruleService.GetLevelRulesAsync();
            return Ok(ApiResponse<List<AdminForgeProfessionLevelRuleDto>>.Ok(items));
        }

        [HttpGet("levels/{level:int}")]
        [ProducesResponseType(typeof(ApiResponse<AdminForgeProfessionLevelRuleDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<AdminForgeProfessionLevelRuleDto>>> GetLevel(int level)
        {
            var item = await _ruleService.GetLevelRuleAsync(level);
            if (item == null)
            {
                return NotFound(ApiResponse<AdminForgeProfessionLevelRuleDto>.Fail("锻造职业等级规则不存在。"));
            }

            return Ok(ApiResponse<AdminForgeProfessionLevelRuleDto>.Ok(item));
        }

        [HttpPost("levels")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminForgeProfessionLevelRuleDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<AdminForgeProfessionLevelRuleDto>>> SaveLevel([FromBody] AdminForgeProfessionLevelRuleDto request)
        {
            try
            {
                var item = await _ruleService.SaveLevelRuleAsync(request);
                return Ok(ApiResponse<AdminForgeProfessionLevelRuleDto>.Ok(item, "锻造职业等级规则保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminForgeProfessionLevelRuleDto>.Fail(ex.Message));
            }
        }

        [HttpDelete("levels/{level:int}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        public async Task<ActionResult<ApiResponse>> DeleteLevel(int level)
        {
            var deleted = await _ruleService.DeleteLevelRuleAsync(level);
            if (!deleted)
            {
                return NotFound(ApiResponse.Fail("锻造职业等级规则不存在或删除失败。"));
            }

            return Ok(ApiResponse.Ok("锻造职业等级规则删除成功"));
        }

        [HttpGet("config")]
        [ProducesResponseType(typeof(ApiResponse<AdminForgeProfessionRuleDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<AdminForgeProfessionRuleDto>>> GetRule()
        {
            var item = await _ruleService.GetRuleAsync();
            if (item == null)
            {
                return NotFound(ApiResponse<AdminForgeProfessionRuleDto>.Fail("锻造职业通用规则不存在。"));
            }

            return Ok(ApiResponse<AdminForgeProfessionRuleDto>.Ok(item));
        }

        [HttpPost("config")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminForgeProfessionRuleDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<AdminForgeProfessionRuleDto>>> SaveRule([FromBody] AdminForgeProfessionRuleDto request)
        {
            try
            {
                var item = await _ruleService.SaveRuleAsync(request);
                return Ok(ApiResponse<AdminForgeProfessionRuleDto>.Ok(item, "锻造职业通用规则保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminForgeProfessionRuleDto>.Fail(ex.Message));
            }
        }
    }
}
#pragma warning restore CS1591
