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
    [Route("api/admin/alchemy-rules")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class AlchemyRulesController : ControllerBase
    {
        private readonly IAdminAlchemyRuleService _ruleService;

        public AlchemyRulesController(IAdminAlchemyRuleService ruleService)
        {
            _ruleService = ruleService;
        }

        [HttpGet("levels")]
        [ProducesResponseType(typeof(ApiResponse<List<AdminAlchemyProfessionLevelRuleDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminAlchemyProfessionLevelRuleDto>>>> GetLevels()
        {
            var items = await _ruleService.GetLevelRulesAsync();
            return Ok(ApiResponse<List<AdminAlchemyProfessionLevelRuleDto>>.Ok(items));
        }

        [HttpGet("levels/{level:int}")]
        [ProducesResponseType(typeof(ApiResponse<AdminAlchemyProfessionLevelRuleDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<AdminAlchemyProfessionLevelRuleDto>>> GetLevel(int level)
        {
            var item = await _ruleService.GetLevelRuleAsync(level);
            if (item == null)
            {
                return NotFound(ApiResponse<AdminAlchemyProfessionLevelRuleDto>.Fail("炼丹职业等级规则不存在。"));
            }

            return Ok(ApiResponse<AdminAlchemyProfessionLevelRuleDto>.Ok(item));
        }

        [HttpPost("levels")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminAlchemyProfessionLevelRuleDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<AdminAlchemyProfessionLevelRuleDto>>> SaveLevel([FromBody] AdminAlchemyProfessionLevelRuleDto request)
        {
            try
            {
                var item = await _ruleService.SaveLevelRuleAsync(request);
                return Ok(ApiResponse<AdminAlchemyProfessionLevelRuleDto>.Ok(item, "炼丹职业等级规则保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminAlchemyProfessionLevelRuleDto>.Fail(ex.Message));
            }
        }

        [HttpDelete("levels/{level:int}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        public async Task<ActionResult<ApiResponse>> DeleteLevel(int level)
        {
            var deleted = await _ruleService.DeleteLevelRuleAsync(level);
            if (!deleted)
            {
                return NotFound(ApiResponse.Fail("炼丹职业等级规则不存在或删除失败。"));
            }

            return Ok(ApiResponse.Ok("炼丹职业等级规则删除成功"));
        }

        [HttpGet("config")]
        [ProducesResponseType(typeof(ApiResponse<AdminAlchemyProfessionRuleDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<AdminAlchemyProfessionRuleDto>>> GetRule()
        {
            var item = await _ruleService.GetRuleAsync();
            if (item == null)
            {
                return NotFound(ApiResponse<AdminAlchemyProfessionRuleDto>.Fail("炼丹职业通用规则不存在。"));
            }

            return Ok(ApiResponse<AdminAlchemyProfessionRuleDto>.Ok(item));
        }

        [HttpPost("config")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminAlchemyProfessionRuleDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<AdminAlchemyProfessionRuleDto>>> SaveRule([FromBody] AdminAlchemyProfessionRuleDto request)
        {
            try
            {
                var item = await _ruleService.SaveRuleAsync(request);
                return Ok(ApiResponse<AdminAlchemyProfessionRuleDto>.Ok(item, "炼丹职业通用规则保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminAlchemyProfessionRuleDto>.Fail(ex.Message));
            }
        }
    }
}
#pragma warning restore CS1591
