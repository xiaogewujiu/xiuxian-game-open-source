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
    [Route("api/admin/redeem-codes")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class RedeemCodesController : ControllerBase
    {
        private readonly IAdminRewardConfigService _adminRewardConfigService;

        public RedeemCodesController(IAdminRewardConfigService adminRewardConfigService)
        {
            _adminRewardConfigService = adminRewardConfigService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminRedeemCodeConfigDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminRedeemCodeConfigDto>>>> GetList([FromQuery] string? keyword = null, [FromQuery] bool? isEnabled = null)
        {
            var configs = await _adminRewardConfigService.GetRedeemCodeConfigsAsync(keyword, isEnabled);
            return Ok(ApiResponse<List<AdminRedeemCodeConfigDto>>.Ok(configs));
        }

        [HttpPost]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminRedeemCodeConfigDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminRedeemCodeConfigDto>>> Save([FromBody] AdminRedeemCodeConfigDto request)
        {
            try
            {
                var config = await _adminRewardConfigService.SaveRedeemCodeConfigAsync(request);
                return Ok(ApiResponse<AdminRedeemCodeConfigDto>.Ok(config, "兑换码配置保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminRedeemCodeConfigDto>.Fail(ex.Message));
            }
        }

        [HttpDelete("{code}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> Delete(string code)
        {
            try
            {
                var deleted = await _adminRewardConfigService.DeleteRedeemCodeConfigAsync(code);
                if (!deleted)
                {
                    return NotFound(ApiResponse.Fail("兑换码配置不存在或删除失败。"));
                }

                return Ok(ApiResponse.Ok("兑换码配置删除成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }
    }
}
#pragma warning restore CS1591
