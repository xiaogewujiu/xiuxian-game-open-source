using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.Application.Security;

namespace XXX.WebApi.Controllers.Admin
{
    /// <summary>
    /// 管理后台邮件控制器。
    /// </summary>
    [ApiController]
    [Route("api/admin/mail")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class AdminMailController : ControllerBase
    {
        private readonly IAdminMailService _adminMailService;

        public AdminMailController(IAdminMailService adminMailService)
        {
            _adminMailService = adminMailService;
        }

        /// <summary>
        /// 获取邮件列表。
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminMailListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminMailListItemDto>>>> GetList(
            [FromQuery] string? keyword = null,
            [FromQuery] bool? isGlobal = null,
            [FromQuery] int take = 200)
        {
            var result = await _adminMailService.GetListAsync(keyword, isGlobal, take);
            return Ok(ApiResponse<List<AdminMailListItemDto>>.Ok(result));
        }

        /// <summary>
        /// 获取邮件详情。
        /// </summary>
        [HttpGet("{mailId:long}")]
        [ProducesResponseType(typeof(ApiResponse<AdminMailDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminMailDetailDto>>> GetDetail(long mailId)
        {
            var result = await _adminMailService.GetDetailAsync(mailId);
            if (result == null) return NotFound(ApiResponse<AdminMailDetailDto>.Fail("邮件不存在。"));

            return Ok(ApiResponse<AdminMailDetailDto>.Ok(result));
        }

        /// <summary>
        /// 发送邮件。
        /// </summary>
        [HttpPost]
        [Authorize(Policy = AdminPermissionCatalog.PlayerGrantPolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminMailDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminMailDetailDto>>> SendMail([FromBody] AdminSendMailDto dto)
        {
            try
            {
                var result = await _adminMailService.SendMailAsync(dto);
                return Ok(ApiResponse<AdminMailDetailDto>.Ok(result, "邮件发送成功。"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminMailDetailDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 删除邮件。
        /// </summary>
        [HttpDelete("{mailId:long}")]
        [Authorize(Policy = AdminPermissionCatalog.PlayerWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> DeleteMail(long mailId)
        {
            var success = await _adminMailService.DeleteMailAsync(mailId);
            if (!success) return NotFound(ApiResponse.Fail("邮件不存在。"));

            return Ok(ApiResponse.Ok("邮件已删除。"));
        }

        /// <summary>
        /// 撤回全服邮件。
        /// </summary>
        [HttpPost("{mailId:long}/recall")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> RecallGlobalMail(long mailId)
        {
            var success = await _adminMailService.RecallGlobalMailAsync(mailId);
            if (!success) return NotFound(ApiResponse.Fail("全服邮件不存在或已撤回。"));

            return Ok(ApiResponse.Ok("全服邮件已撤回。"));
        }
    }
}
