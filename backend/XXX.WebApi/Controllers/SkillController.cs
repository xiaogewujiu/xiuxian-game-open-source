using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.WebApi.Extensions;

namespace XXX.WebApi.Controllers
{
    /// <summary>
    /// 技能信息控制器。
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SkillController : ControllerBase
    {
        private readonly ISkillService _skillService;

        /// <summary>
        /// 初始化技能控制器。
        /// </summary>
        public SkillController(ISkillService skillService)
        {
            _skillService = skillService;
        }

        private string? GetCurrentPlayerId() => User.GetCurrentPlayerId();

        /// <summary>
        /// 获取当前角色技能总览。
        /// </summary>
        /// <remarks>
        /// 返回结果会同时包含：
        /// 1. 当前已携带技能。
        /// 2. 当前已掌握但未携带的技能库。
        /// </remarks>
        [HttpGet("me")]
        public async Task<ActionResult<ApiResponse<SkillOverviewDto>>> GetCurrentPlayerSkills()
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _skillService.GetSkillOverviewAsync(playerId);
            return Ok(ApiResponse<SkillOverviewDto>.Ok(result));
        }

        [HttpPost("upgrade")]
        public async Task<ActionResult<ApiResponse<SkillUpgradeResultDto>>> UpgradeSkill([FromBody] SkillUpgradeRequestDto request)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            try
            {
                var result = await _skillService.UpgradeSkillAsync(playerId, request.SkillId);
                return Ok(ApiResponse<SkillUpgradeResultDto>.Ok(result, "技能升级成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<SkillUpgradeResultDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 携带一个已掌握技能。
        /// </summary>
        [HttpPost("equip")]
        public async Task<ActionResult<ApiResponse<SkillOverviewDto>>> EquipSkill([FromBody] SkillOperateRequestDto request)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _skillService.EquipSkillAsync(playerId, request.SkillId);
            return Ok(ApiResponse<SkillOverviewDto>.Ok(result, "技能已携带"));
        }

        /// <summary>
        /// 卸下一个当前已携带技能。
        /// </summary>
        [HttpPost("unequip")]
        public async Task<ActionResult<ApiResponse<SkillOverviewDto>>> UnequipSkill([FromBody] SkillOperateRequestDto request)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _skillService.UnequipSkillAsync(playerId, request.SkillId);
            return Ok(ApiResponse<SkillOverviewDto>.Ok(result, "技能已卸下"));
        }
    }
}
