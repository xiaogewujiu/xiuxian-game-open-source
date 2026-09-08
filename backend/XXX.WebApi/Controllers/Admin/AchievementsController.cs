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
    [Route("api/admin/achievements")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class AchievementsController : ControllerBase
    {
        private readonly IAdminAchievementService _adminAchievementService;

        public AchievementsController(IAdminAchievementService adminAchievementService)
        {
            _adminAchievementService = adminAchievementService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminAchievementListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminAchievementListItemDto>>>> GetList([FromQuery] string? keyword = null, [FromQuery] int? difficulty = null)
        {
            var achievements = await _adminAchievementService.GetListAsync(keyword, difficulty);
            return Ok(ApiResponse<List<AdminAchievementListItemDto>>.Ok(achievements));
        }

        [HttpGet("{achievementId}")]
        [ProducesResponseType(typeof(ApiResponse<AdminAchievementDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminAchievementDetailDto>>> GetDetail(string achievementId)
        {
            var achievement = await _adminAchievementService.GetDetailAsync(achievementId);
            if (achievement == null)
            {
                return NotFound(ApiResponse<AdminAchievementDetailDto>.Fail("成就配置不存在。"));
            }

            return Ok(ApiResponse<AdminAchievementDetailDto>.Ok(achievement));
        }

        [HttpPost]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminAchievementDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminAchievementDetailDto>>> Save([FromBody] AdminAchievementDetailDto request)
        {
            try
            {
                var achievement = await _adminAchievementService.SaveAsync(request);
                return Ok(ApiResponse<AdminAchievementDetailDto>.Ok(achievement, "成就配置保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminAchievementDetailDto>.Fail(ex.Message));
            }
        }

        [HttpDelete("{achievementId}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> Delete(string achievementId)
        {
            try
            {
                var deleted = await _adminAchievementService.DeleteAsync(achievementId);
                if (!deleted)
                {
                    return NotFound(ApiResponse.Fail("成就配置不存在或删除失败。"));
                }

                return Ok(ApiResponse.Ok("成就配置删除成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }
    }
}

#pragma warning restore CS1591

