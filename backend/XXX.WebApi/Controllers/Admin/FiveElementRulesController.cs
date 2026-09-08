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
    [Route("api/admin/five-element-rules")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class FiveElementRulesController : ControllerBase
    {
        private readonly IAdminFiveElementRuleService _ruleService;

        public FiveElementRulesController(IAdminFiveElementRuleService ruleService)
        {
            _ruleService = ruleService;
        }

        [HttpGet("levels")]
        [ProducesResponseType(typeof(ApiResponse<List<AdminFiveElementLevelRuleListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminFiveElementLevelRuleListItemDto>>>> GetLevels()
        {
            var items = await _ruleService.GetLevelRulesAsync();
            return Ok(ApiResponse<List<AdminFiveElementLevelRuleListItemDto>>.Ok(items));
        }

        [HttpGet("levels/{arrayLevel:int}")]
        [ProducesResponseType(typeof(ApiResponse<AdminFiveElementLevelRuleDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminFiveElementLevelRuleDetailDto>>> GetLevelDetail(int arrayLevel)
        {
            var detail = await _ruleService.GetLevelRuleDetailAsync(arrayLevel);
            if (detail == null)
            {
                return NotFound(ApiResponse<AdminFiveElementLevelRuleDetailDto>.Fail("聚灵阵等级规则不存在。"));
            }

            return Ok(ApiResponse<AdminFiveElementLevelRuleDetailDto>.Ok(detail));
        }

        [HttpPost("levels")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminFiveElementLevelRuleDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminFiveElementLevelRuleDetailDto>>> SaveLevel([FromBody] AdminFiveElementLevelRuleDetailDto request)
        {
            try
            {
                var detail = await _ruleService.SaveLevelRuleAsync(request);
                return Ok(ApiResponse<AdminFiveElementLevelRuleDetailDto>.Ok(detail, "聚灵阵等级规则保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminFiveElementLevelRuleDetailDto>.Fail(ex.Message));
            }
        }

        [HttpDelete("levels/{arrayLevel:int}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> DeleteLevel(int arrayLevel)
        {
            var deleted = await _ruleService.DeleteLevelRuleAsync(arrayLevel);
            if (!deleted)
            {
                return NotFound(ApiResponse.Fail("聚灵阵等级规则不存在或删除失败。"));
            }

            return Ok(ApiResponse.Ok("聚灵阵等级规则删除成功"));
        }

        [HttpGet("ranges")]
        public async Task<ActionResult<ApiResponse<List<AdminFiveElementBranchRuleRangeListItemDto>>>> GetRanges([FromQuery] string? elementType = null)
        {
            var items = await _ruleService.GetBranchRuleRangesAsync(elementType);
            return Ok(ApiResponse<List<AdminFiveElementBranchRuleRangeListItemDto>>.Ok(items));
        }

        [HttpGet("ranges/{gid}")]
        public async Task<ActionResult<ApiResponse<AdminFiveElementBranchRuleRangeDetailDto>>> GetRangeDetail(string gid)
        {
            var detail = await _ruleService.GetBranchRuleRangeDetailAsync(gid);
            if (detail == null) return NotFound(ApiResponse<AdminFiveElementBranchRuleRangeDetailDto>.Fail("五行区间规则不存在。"));
            return Ok(ApiResponse<AdminFiveElementBranchRuleRangeDetailDto>.Ok(detail));
        }

        [HttpPost("ranges")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        public async Task<ActionResult<ApiResponse<AdminFiveElementBranchRuleRangeDetailDto>>> SaveRange([FromBody] AdminFiveElementBranchRuleRangeDetailDto request)
        {
            try
            {
                var detail = await _ruleService.SaveBranchRuleRangeAsync(request);
                return Ok(ApiResponse<AdminFiveElementBranchRuleRangeDetailDto>.Ok(detail, "五行区间规则保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminFiveElementBranchRuleRangeDetailDto>.Fail(ex.Message));
            }
        }

        [HttpDelete("ranges/{gid}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        public async Task<ActionResult<ApiResponse>> DeleteRange(string gid)
        {
            var deleted = await _ruleService.DeleteBranchRuleRangeAsync(gid);
            if (!deleted) return NotFound(ApiResponse.Fail("五行区间规则不存在或删除失败。"));
            return Ok(ApiResponse.Ok("五行区间规则删除成功"));
        }

        [HttpGet("branches")]
        [ProducesResponseType(typeof(ApiResponse<List<AdminFiveElementBranchRuleListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminFiveElementBranchRuleListItemDto>>>> GetBranches([FromQuery] string? elementType = null)
        {
            var items = await _ruleService.GetBranchRulesAsync(elementType);
            return Ok(ApiResponse<List<AdminFiveElementBranchRuleListItemDto>>.Ok(items));
        }

        [HttpGet("branches/{gid}")]
        [ProducesResponseType(typeof(ApiResponse<AdminFiveElementBranchRuleDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminFiveElementBranchRuleDetailDto>>> GetBranchDetail(string gid)
        {
            var detail = await _ruleService.GetBranchRuleDetailAsync(gid);
            if (detail == null)
            {
                return NotFound(ApiResponse<AdminFiveElementBranchRuleDetailDto>.Fail("五行分支规则不存在。"));
            }

            return Ok(ApiResponse<AdminFiveElementBranchRuleDetailDto>.Ok(detail));
        }

        [HttpPost("branches")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminFiveElementBranchRuleDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminFiveElementBranchRuleDetailDto>>> SaveBranch([FromBody] AdminFiveElementBranchRuleDetailDto request)
        {
            try
            {
                var detail = await _ruleService.SaveBranchRuleAsync(request);
                return Ok(ApiResponse<AdminFiveElementBranchRuleDetailDto>.Ok(detail, "五行分支规则保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminFiveElementBranchRuleDetailDto>.Fail(ex.Message));
            }
        }

        [HttpDelete("branches/{gid}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> DeleteBranch(string gid)
        {
            var deleted = await _ruleService.DeleteBranchRuleAsync(gid);
            if (!deleted)
            {
                return NotFound(ApiResponse.Fail("五行分支规则不存在或删除失败。"));
            }

            return Ok(ApiResponse.Ok("五行分支规则删除成功"));
        }
    }
}
#pragma warning restore CS1591
