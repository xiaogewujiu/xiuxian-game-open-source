#pragma warning disable CS1591
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.Application.Security;
using XXX.Entity;

namespace XXX.WebApi.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/skills")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class SkillsController : ControllerBase
    {
        private readonly IAdminSkillService _adminSkillService;

        public SkillsController(IAdminSkillService adminSkillService)
        {
            _adminSkillService = adminSkillService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminSkillListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminSkillListItemDto>>>> GetList([FromQuery] string? keyword = null, [FromQuery] string? catalog = null)
        {
            var skills = await _adminSkillService.GetListAsync(keyword, catalog);
            return Ok(ApiResponse<List<AdminSkillListItemDto>>.Ok(skills));
        }

        [HttpGet("{skillId:int}")]
        [ProducesResponseType(typeof(ApiResponse<AdminSkillDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminSkillDetailDto>>> GetDetail(int skillId)
        {
            var skill = await _adminSkillService.GetDetailAsync(skillId);
            if (skill == null)
            {
                return NotFound(ApiResponse<AdminSkillDetailDto>.Fail("技能模板不存在。"));
            }

            return Ok(ApiResponse<AdminSkillDetailDto>.Ok(skill));
        }

        [HttpGet("{skillId:int}/next-levels")]
        public async Task<ActionResult<ApiResponse<List<AdminSkillNextLevelDto>>>> GetNextLevels(int skillId)
        {
            try
            {
                return Ok(ApiResponse<List<AdminSkillNextLevelDto>>.Ok(await _adminSkillService.GetNextLevelsAsync(skillId)));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<List<AdminSkillNextLevelDto>>.Fail(ex.Message));
            }
        }

        [HttpPost]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminSkillDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminSkillDetailDto>>> Save([FromBody] AdminSkillDetailDto request)
        {
            try
            {
                var skill = await _adminSkillService.SaveAsync(request);
                return Ok(ApiResponse<AdminSkillDetailDto>.Ok(skill, "技能模板保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminSkillDetailDto>.Fail(ex.Message));
            }
        }

        [HttpPost("{skillId:int}/next/copy")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        public async Task<ActionResult<ApiResponse<AdminSkillDetailDto>>> CopyNext(int skillId)
        {
            try
            {
                return Ok(ApiResponse<AdminSkillDetailDto>.Ok(await _adminSkillService.CopyNextAsync(skillId), "下一级技能创建成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminSkillDetailDto>.Fail(ex.Message));
            }
        }

        [HttpPost("{skillId:int}/next/link")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        public async Task<ActionResult<ApiResponse<AdminSkillDetailDto>>> LinkNext(int skillId, [FromBody] AdminSkillNextLinkRequestDto request)
        {
            try
            {
                return Ok(ApiResponse<AdminSkillDetailDto>.Ok(await _adminSkillService.LinkNextAsync(skillId, request.NextSkillId, request.UpgradeConditions), "下一级技能关联成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminSkillDetailDto>.Fail(ex.Message));
            }
        }

        [HttpPut("{skillId:int}/upgrade-conditions")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        public async Task<ActionResult<ApiResponse<AdminSkillDetailDto>>> UpdateUpgradeConditions(int skillId, [FromBody] List<SkillUpgradeCondition> conditions)
        {
            try
            {
                return Ok(ApiResponse<AdminSkillDetailDto>.Ok(await _adminSkillService.UpdateUpgradeConditionsAsync(skillId, conditions), "技能升级条件保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminSkillDetailDto>.Fail(ex.Message));
            }
        }

        [HttpDelete("{skillId:int}/next-chain")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        public async Task<ActionResult<ApiResponse>> DeleteNextChain(int skillId)
        {
            try
            {
                await _adminSkillService.DeleteNextChainAsync(skillId);
                return Ok(ApiResponse.Ok("当前技能及后续等级删除成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }

        [HttpDelete("{skillId:int}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> Delete(int skillId)
        {
            try
            {
                var deleted = await _adminSkillService.DeleteAsync(skillId);
                if (!deleted)
                {
                    return NotFound(ApiResponse.Fail("技能模板不存在或删除失败。"));
                }

                return Ok(ApiResponse.Ok("技能模板删除成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }
    }
}

#pragma warning restore CS1591

