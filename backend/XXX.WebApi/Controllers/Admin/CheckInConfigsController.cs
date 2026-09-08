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
    [Route("api/admin/checkin-configs")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class CheckInConfigsController : ControllerBase
    {
        private readonly IAdminRewardConfigService _adminRewardConfigService;

        public CheckInConfigsController(IAdminRewardConfigService adminRewardConfigService)
        {
            _adminRewardConfigService = adminRewardConfigService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminCheckInRewardConfigDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminCheckInRewardConfigDto>>>> GetList([FromQuery] bool? isMilestone = null)
        {
            var configs = await _adminRewardConfigService.GetCheckInConfigsAsync(isMilestone);
            return Ok(ApiResponse<List<AdminCheckInRewardConfigDto>>.Ok(configs));
        }

        [HttpPost]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminCheckInRewardConfigDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminCheckInRewardConfigDto>>> Save([FromBody] AdminCheckInRewardConfigDto request)
        {
            try
            {
                var config = await _adminRewardConfigService.SaveCheckInConfigAsync(request);
                return Ok(ApiResponse<AdminCheckInRewardConfigDto>.Ok(config, "签到奖励配置保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminCheckInRewardConfigDto>.Fail(ex.Message));
            }
        }

        [HttpDelete("{continuousDay:int}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> Delete(int continuousDay)
        {
            try
            {
                var deleted = await _adminRewardConfigService.DeleteCheckInConfigAsync(continuousDay);
                if (!deleted)
                {
                    return NotFound(ApiResponse.Fail("签到奖励配置不存在或删除失败。"));
                }

                return Ok(ApiResponse.Ok("签到奖励配置删除成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }
    }
}
#pragma warning restore CS1591
