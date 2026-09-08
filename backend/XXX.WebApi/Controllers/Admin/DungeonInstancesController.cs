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
    [Route("api/admin/dungeon-instances")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class DungeonInstancesController : ControllerBase
    {
        private readonly IAdminDungeonInstanceService _adminService;

        public DungeonInstancesController(IAdminDungeonInstanceService adminService)
        {
            _adminService = adminService;
        }

        #region 秘境模板

        [HttpGet("templates")]
        [ProducesResponseType(typeof(ApiResponse<List<AdminDungeonInstanceTemplateListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminDungeonInstanceTemplateListItemDto>>>> GetTemplateList([FromQuery] string? keyword = null)
        {
            var list = await _adminService.GetTemplateListAsync(keyword);
            return Ok(ApiResponse<List<AdminDungeonInstanceTemplateListItemDto>>.Ok(list));
        }

        [HttpGet("templates/{dungeonId}")]
        [ProducesResponseType(typeof(ApiResponse<AdminDungeonInstanceTemplateDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminDungeonInstanceTemplateDetailDto>>> GetTemplateDetail(string dungeonId)
        {
            var detail = await _adminService.GetTemplateDetailAsync(dungeonId);
            if (detail == null)
            {
                return NotFound(ApiResponse<AdminDungeonInstanceTemplateDetailDto>.Fail("秘境模板不存在。"));
            }

            return Ok(ApiResponse<AdminDungeonInstanceTemplateDetailDto>.Ok(detail));
        }

        [HttpPost("templates")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminDungeonInstanceTemplateDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminDungeonInstanceTemplateDetailDto>>> SaveTemplate([FromBody] AdminDungeonInstanceTemplateDetailDto request)
        {
            try
            {
                var detail = await _adminService.SaveTemplateAsync(request);
                return Ok(ApiResponse<AdminDungeonInstanceTemplateDetailDto>.Ok(detail, "秘境模板保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminDungeonInstanceTemplateDetailDto>.Fail(ex.Message));
            }
        }

        [HttpDelete("templates/{dungeonId}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> DeleteTemplate(string dungeonId)
        {
            try
            {
                var deleted = await _adminService.DeleteTemplateAsync(dungeonId);
                if (!deleted)
                {
                    return NotFound(ApiResponse.Fail("秘境模板不存在或删除失败。"));
                }

                return Ok(ApiResponse.Ok("秘境模板删除成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }

        #endregion

        #region 事件配置

        [HttpGet("events")]
        [ProducesResponseType(typeof(ApiResponse<List<AdminDungeonEventConfigListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminDungeonEventConfigListItemDto>>>> GetEventList(
            [FromQuery] string? dungeonId = null,
            [FromQuery] int? eventType = null,
            [FromQuery] string? keyword = null)
        {
            var list = await _adminService.GetEventListAsync(dungeonId, eventType, keyword);
            return Ok(ApiResponse<List<AdminDungeonEventConfigListItemDto>>.Ok(list));
        }

        [HttpGet("events/{eventId}")]
        [ProducesResponseType(typeof(ApiResponse<AdminDungeonEventConfigDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminDungeonEventConfigDetailDto>>> GetEventDetail(string eventId)
        {
            var detail = await _adminService.GetEventDetailAsync(eventId);
            if (detail == null)
            {
                return NotFound(ApiResponse<AdminDungeonEventConfigDetailDto>.Fail("事件配置不存在。"));
            }

            return Ok(ApiResponse<AdminDungeonEventConfigDetailDto>.Ok(detail));
        }

        [HttpPost("events")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminDungeonEventConfigDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminDungeonEventConfigDetailDto>>> SaveEvent([FromBody] AdminDungeonEventConfigDetailDto request)
        {
            try
            {
                var detail = await _adminService.SaveEventAsync(request);
                return Ok(ApiResponse<AdminDungeonEventConfigDetailDto>.Ok(detail, "事件配置保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminDungeonEventConfigDetailDto>.Fail(ex.Message));
            }
        }

        [HttpDelete("events/{eventId}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> DeleteEvent(string eventId)
        {
            try
            {
                var deleted = await _adminService.DeleteEventAsync(eventId);
                if (!deleted)
                {
                    return NotFound(ApiResponse.Fail("事件配置不存在或删除失败。"));
                }

                return Ok(ApiResponse.Ok("事件配置删除成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }

        [HttpPatch("events/{eventId}/toggle")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> ToggleEvent(string eventId, [FromQuery] bool enabled = true)
        {
            var toggled = await _adminService.ToggleEventAsync(eventId, enabled);
            if (!toggled)
            {
                return NotFound(ApiResponse.Fail("事件配置不存在。"));
            }

            return Ok(ApiResponse.Ok(enabled ? "事件已启用" : "事件已禁用"));
        }

        #endregion

        #region 事件组

        [HttpGet("groups")]
        [ProducesResponseType(typeof(ApiResponse<List<AdminDungeonEventGroupListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminDungeonEventGroupListItemDto>>>> GetGroupList([FromQuery] string? keyword = null)
        {
            var list = await _adminService.GetGroupListAsync(keyword);
            return Ok(ApiResponse<List<AdminDungeonEventGroupListItemDto>>.Ok(list));
        }

        [HttpGet("groups/{groupId}")]
        [ProducesResponseType(typeof(ApiResponse<AdminDungeonEventGroupDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminDungeonEventGroupDetailDto>>> GetGroupDetail(string groupId)
        {
            var detail = await _adminService.GetGroupDetailAsync(groupId);
            if (detail == null)
            {
                return NotFound(ApiResponse<AdminDungeonEventGroupDetailDto>.Fail("事件组不存在。"));
            }

            return Ok(ApiResponse<AdminDungeonEventGroupDetailDto>.Ok(detail));
        }

        [HttpPost("groups")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminDungeonEventGroupDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminDungeonEventGroupDetailDto>>> SaveGroup([FromBody] AdminDungeonEventGroupDetailDto request)
        {
            try
            {
                var detail = await _adminService.SaveGroupAsync(request);
                return Ok(ApiResponse<AdminDungeonEventGroupDetailDto>.Ok(detail, "事件组保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminDungeonEventGroupDetailDto>.Fail(ex.Message));
            }
        }

        [HttpDelete("groups/{groupId}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> DeleteGroup(string groupId)
        {
            try
            {
                var deleted = await _adminService.DeleteGroupAsync(groupId);
                if (!deleted)
                {
                    return NotFound(ApiResponse.Fail("事件组不存在或删除失败。"));
                }

                return Ok(ApiResponse.Ok("事件组删除成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }

        #endregion

        [HttpPost("reload-runtime")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse>> ReloadRuntime()
        {
            await _adminService.ReloadRuntimeAsync();
            return Ok(ApiResponse.Ok("运行时缓存已刷新"));
        }
    }
}
#pragma warning restore CS1591
