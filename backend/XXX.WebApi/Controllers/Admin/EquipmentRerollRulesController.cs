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
    [Route("api/admin/equipment-reroll-rules")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class EquipmentRerollRulesController : ControllerBase
    {
        private readonly IAdminEquipmentRerollRuleService _ruleService;

        public EquipmentRerollRulesController(IAdminEquipmentRerollRuleService ruleService)
        {
            _ruleService = ruleService;
        }

        [HttpGet("enhance-costs")]
        [ProducesResponseType(typeof(ApiResponse<List<AdminEquipmentEnhanceRuleDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminEquipmentEnhanceRuleDto>>>> GetEnhanceRules()
        {
            return Ok(ApiResponse<List<AdminEquipmentEnhanceRuleDto>>.Ok(await _ruleService.GetEnhanceRulesAsync()));
        }

        [HttpPost("enhance-costs")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        public async Task<ActionResult<ApiResponse>> SaveEnhanceRule([FromBody] AdminEquipmentEnhanceRuleDto request)
        {
            await _ruleService.SaveEnhanceRuleAsync(request);
            return Ok(ApiResponse.Ok("强化规则保存成功"));
        }

        [HttpDelete("enhance-costs/{gid:long}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        public async Task<ActionResult<ApiResponse>> DeleteEnhanceRule(long gid)
        {
            await _ruleService.DeleteEnhanceRuleAsync(gid);
            return Ok(ApiResponse.Ok("强化规则删除成功"));
        }

        [HttpGet("reroll-costs")]
        [ProducesResponseType(typeof(ApiResponse<List<AdminEquipmentRerollCostRuleDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminEquipmentRerollCostRuleDto>>>> GetRerollCostRules()
        {
            return Ok(ApiResponse<List<AdminEquipmentRerollCostRuleDto>>.Ok(await _ruleService.GetRerollCostRulesAsync()));
        }

        [HttpPost("reroll-costs")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        public async Task<ActionResult<ApiResponse>> SaveRerollCostRule([FromBody] AdminEquipmentRerollCostRuleDto request)
        {
            await _ruleService.SaveRerollCostRuleAsync(request);
            return Ok(ApiResponse.Ok("洗炼消耗规则保存成功"));
        }

        [HttpDelete("reroll-costs/{gid:long}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        public async Task<ActionResult<ApiResponse>> DeleteRerollCostRule(long gid)
        {
            await _ruleService.DeleteRerollCostRuleAsync(gid);
            return Ok(ApiResponse.Ok("洗炼消耗规则删除成功"));
        }

        [HttpGet("system")]
        [ProducesResponseType(typeof(ApiResponse<AdminEquipmentRerollSystemConfigDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<AdminEquipmentRerollSystemConfigDto>>> GetSystemConfig()
        {
            var item = await _ruleService.GetSystemConfigAsync();
            if (item == null)
            {
                return NotFound(ApiResponse<AdminEquipmentRerollSystemConfigDto>.Fail("洗练系统配置不存在。"));
            }

            return Ok(ApiResponse<AdminEquipmentRerollSystemConfigDto>.Ok(item));
        }

        [HttpPost("system")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminEquipmentRerollSystemConfigDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<AdminEquipmentRerollSystemConfigDto>>> SaveSystemConfig([FromBody] AdminEquipmentRerollSystemConfigDto request)
        {
            await _ruleService.SaveSystemConfigAsync(request);
            return Ok(ApiResponse<AdminEquipmentRerollSystemConfigDto>.Ok(request, "洗练系统配置保存成功"));
        }

        [HttpGet("pools")]
        [ProducesResponseType(typeof(ApiResponse<List<AdminEquipmentRerollSlotPoolConfigDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminEquipmentRerollSlotPoolConfigDto>>>> GetPools()
        {
            var items = await _ruleService.GetSlotPoolConfigsAsync();
            return Ok(ApiResponse<List<AdminEquipmentRerollSlotPoolConfigDto>>.Ok(items));
        }

        [HttpPost("pools")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminEquipmentRerollSlotPoolConfigDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<AdminEquipmentRerollSlotPoolConfigDto>>> SavePool([FromBody] AdminEquipmentRerollSlotPoolConfigDto request)
        {
            await _ruleService.SaveSlotPoolConfigAsync(request);
            return Ok(ApiResponse<AdminEquipmentRerollSlotPoolConfigDto>.Ok(request, "词条池配置保存成功"));
        }

        [HttpDelete("pools/{gid:long}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        public async Task<ActionResult<ApiResponse>> DeletePool(long gid)
        {
            await _ruleService.DeleteSlotPoolConfigAsync(gid);
            return Ok(ApiResponse.Ok("词条池配置删除成功"));
        }

        [HttpGet("tiers")]
        [ProducesResponseType(typeof(ApiResponse<List<AdminEquipmentRerollTierConfigDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminEquipmentRerollTierConfigDto>>>> GetTiers()
        {
            var items = await _ruleService.GetTierConfigsAsync();
            return Ok(ApiResponse<List<AdminEquipmentRerollTierConfigDto>>.Ok(items));
        }

        [HttpPost("tiers")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminEquipmentRerollTierConfigDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<AdminEquipmentRerollTierConfigDto>>> SaveTier([FromBody] AdminEquipmentRerollTierConfigDto request)
        {
            await _ruleService.SaveTierConfigAsync(request);
            return Ok(ApiResponse<AdminEquipmentRerollTierConfigDto>.Ok(request, "品阶配置保存成功"));
        }

        [HttpDelete("tiers/{gid:long}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        public async Task<ActionResult<ApiResponse>> DeleteTier(long gid)
        {
            await _ruleService.DeleteTierConfigAsync(gid);
            return Ok(ApiResponse.Ok("品阶配置删除成功"));
        }

        [HttpGet("attribute-values")]
        [ProducesResponseType(typeof(ApiResponse<List<AdminEquipmentRerollAttributeValueConfigDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminEquipmentRerollAttributeValueConfigDto>>>> GetAttributeValues()
        {
            var items = await _ruleService.GetAttributeValueConfigsAsync();
            return Ok(ApiResponse<List<AdminEquipmentRerollAttributeValueConfigDto>>.Ok(items));
        }

        [HttpPost("attribute-values")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminEquipmentRerollAttributeValueConfigDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<AdminEquipmentRerollAttributeValueConfigDto>>> SaveAttributeValue([FromBody] AdminEquipmentRerollAttributeValueConfigDto request)
        {
            await _ruleService.SaveAttributeValueConfigAsync(request);
            return Ok(ApiResponse<AdminEquipmentRerollAttributeValueConfigDto>.Ok(request, "属性值配置保存成功"));
        }

        [HttpDelete("attribute-values/{gid:long}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        public async Task<ActionResult<ApiResponse>> DeleteAttributeValue(long gid)
        {
            await _ruleService.DeleteAttributeValueConfigAsync(gid);
            return Ok(ApiResponse.Ok("属性值配置删除成功"));
        }
    }
}
#pragma warning restore CS1591
