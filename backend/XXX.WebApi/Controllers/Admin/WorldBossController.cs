using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.Application.Security;

namespace XXX.WebApi.Controllers.Admin
{
    /// <summary>
    /// 后台世界 Boss 管理控制器。
    /// 提供模板、排期与运行时实例的配置入口。
    /// </summary>
    [ApiController]
    [Route("api/admin/world-boss")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class WorldBossController : ControllerBase
    {
        /// <summary>
        /// 后台世界 Boss 管理服务。
        /// </summary>
        private readonly IAdminWorldBossService _adminWorldBossService;

        /// <summary>
        /// 初始化后台世界 Boss 控制器。
        /// </summary>
        /// <param name="adminWorldBossService">后台世界 Boss 管理服务。</param>
        public WorldBossController(IAdminWorldBossService adminWorldBossService)
        {
            _adminWorldBossService = adminWorldBossService;
        }

        /// <summary>
        /// 获取世界 Boss 模板列表。
        /// </summary>
        /// <returns>模板列表。</returns>
        [HttpGet("templates")]
        public async Task<ActionResult<ApiResponse<List<AdminWorldBossTemplateListItemDto>>>> GetTemplates()
        {
            var result = await _adminWorldBossService.GetTemplatesAsync();
            return Ok(ApiResponse<List<AdminWorldBossTemplateListItemDto>>.Ok(result));
        }

        /// <summary>
        /// 获取指定世界 Boss 模板详情。
        /// </summary>
        /// <param name="bossId">Boss 模板编号。</param>
        /// <returns>模板详情。</returns>
        [HttpGet("templates/{bossId}")]
        public async Task<ActionResult<ApiResponse<AdminWorldBossTemplateDetailDto>>> GetTemplate(string bossId)
        {
            var result = await _adminWorldBossService.GetTemplateAsync(bossId);
            if (result == null)
            {
                return NotFound(ApiResponse<AdminWorldBossTemplateDetailDto>.Fail("世界Boss模板不存在。"));
            }

            return Ok(ApiResponse<AdminWorldBossTemplateDetailDto>.Ok(result));
        }

        /// <summary>
        /// 保存世界 Boss 模板。
        /// </summary>
        /// <param name="request">模板详情请求。</param>
        /// <returns>保存后的模板详情。</returns>
        [HttpPost("templates")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        public async Task<ActionResult<ApiResponse<AdminWorldBossTemplateDetailDto>>> SaveTemplate([FromBody] AdminWorldBossTemplateDetailDto request)
        {
            try
            {
                var result = await _adminWorldBossService.SaveTemplateAsync(request);
                return Ok(ApiResponse<AdminWorldBossTemplateDetailDto>.Ok(result, "世界Boss模板保存成功"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<AdminWorldBossTemplateDetailDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 删除指定世界 Boss 模板。
        /// </summary>
        /// <param name="bossId">Boss 模板编号。</param>
        /// <returns>删除结果。</returns>
        [HttpDelete("templates/{bossId}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        public async Task<ActionResult<ApiResponse>> DeleteTemplate(string bossId)
        {
            var deleted = await _adminWorldBossService.DeleteTemplateAsync(bossId);
            if (!deleted)
            {
                return NotFound(ApiResponse.Fail("世界Boss模板不存在或删除失败。"));
            }

            return Ok(ApiResponse.Ok("世界Boss模板删除成功"));
        }

        /// <summary>
        /// 获取世界 Boss 每日排期配置。
        /// </summary>
        /// <returns>排期配置。</returns>
        [HttpGet("schedule")]
        public async Task<ActionResult<ApiResponse<AdminWorldBossScheduleDto>>> GetSchedule()
        {
            var result = await _adminWorldBossService.GetScheduleAsync();
            return Ok(ApiResponse<AdminWorldBossScheduleDto>.Ok(result));
        }

        /// <summary>
        /// 保存世界 Boss 每日排期配置。
        /// </summary>
        /// <param name="request">排期配置请求。</param>
        /// <returns>保存后的排期配置。</returns>
        [HttpPost("schedule")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        public async Task<ActionResult<ApiResponse<AdminWorldBossScheduleDto>>> SaveSchedule([FromBody] AdminWorldBossScheduleDto request)
        {
            var result = await _adminWorldBossService.SaveScheduleAsync(request);
            return Ok(ApiResponse<AdminWorldBossScheduleDto>.Ok(result, "世界Boss排期保存成功"));
        }

        /// <summary>
        /// 获取世界 Boss 当前运行时总览。
        /// </summary>
        /// <returns>当前实例、排行与最近日志。</returns>
        [HttpGet("runtime")]
        public async Task<ActionResult<ApiResponse<AdminWorldBossRuntimeDto>>> GetRuntime()
        {
            var result = await _adminWorldBossService.GetRuntimeAsync();
            return Ok(ApiResponse<AdminWorldBossRuntimeDto>.Ok(result));
        }

        /// <summary>
        /// 立即生成一个世界 Boss 实例。
        /// </summary>
        /// <param name="bossId">指定的 Boss 模板编号；为空时按规则挑选。</param>
        /// <returns>生成后的世界 Boss 当前状态。</returns>
        [HttpPost("runtime/spawn")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        public async Task<ActionResult<ApiResponse<WorldBossCurrentDto>>> Spawn([FromQuery] string? bossId = null)
        {
            try
            {
                var result = await _adminWorldBossService.SpawnNowAsync(bossId);
                return Ok(ApiResponse<WorldBossCurrentDto>.Ok(result, "世界Boss已生成"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<WorldBossCurrentDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 关闭当前世界 Boss 实例。
        /// </summary>
        /// <param name="reason">关闭原因。</param>
        /// <returns>关闭结果。</returns>
        [HttpPost("runtime/close")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        public async Task<ActionResult<ApiResponse>> Close([FromQuery] string? reason = null)
        {
            var closed = await _adminWorldBossService.CloseCurrentAsync(reason);
            if (!closed)
            {
                return NotFound(ApiResponse.Fail("当前没有开启中的世界Boss。"));
            }

            return Ok(ApiResponse.Ok("世界Boss已关闭"));
        }
    }
}
