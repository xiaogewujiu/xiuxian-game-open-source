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
    [Route("api/admin/quests")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class QuestsController : ControllerBase
    {
        private readonly IAdminQuestService _adminQuestService;

        public QuestsController(IAdminQuestService adminQuestService)
        {
            _adminQuestService = adminQuestService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminQuestListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminQuestListItemDto>>>> GetList([FromQuery] string? keyword = null, [FromQuery] int? questType = null, [FromQuery] int? resetCycle = null)
        {
            var quests = await _adminQuestService.GetListAsync(keyword, questType, resetCycle);
            return Ok(ApiResponse<List<AdminQuestListItemDto>>.Ok(quests));
        }

        [HttpGet("{questId}")]
        [ProducesResponseType(typeof(ApiResponse<AdminQuestDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminQuestDetailDto>>> GetDetail(string questId)
        {
            var quest = await _adminQuestService.GetDetailAsync(questId);
            if (quest == null)
            {
                return NotFound(ApiResponse<AdminQuestDetailDto>.Fail("任务配置不存在。"));
            }

            return Ok(ApiResponse<AdminQuestDetailDto>.Ok(quest));
        }

        [HttpPost]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminQuestDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminQuestDetailDto>>> Save([FromBody] AdminQuestDetailDto request)
        {
            try
            {
                var quest = await _adminQuestService.SaveAsync(request);
                return Ok(ApiResponse<AdminQuestDetailDto>.Ok(quest, "任务配置保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminQuestDetailDto>.Fail(ex.Message));
            }
        }

        [HttpDelete("{questId}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> Delete(string questId)
        {
            try
            {
                var deleted = await _adminQuestService.DeleteAsync(questId);
                if (!deleted)
                {
                    return NotFound(ApiResponse.Fail("任务配置不存在或删除失败。"));
                }

                return Ok(ApiResponse.Ok("任务配置删除成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }
    }
}

#pragma warning restore CS1591

