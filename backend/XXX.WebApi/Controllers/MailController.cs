using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.WebApi.Extensions;

namespace XXX.WebApi.Controllers
{
    /// <summary>
    /// 玩家邮件控制器。
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MailController : ControllerBase
    {
        private readonly IMailService _mailService;

        public MailController(IMailService mailService)
        {
            _mailService = mailService;
        }

        private string? GetCurrentPlayerId() => User.GetCurrentPlayerId();

        /// <summary>
        /// 获取邮件列表。
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<MailPagedResultDto>>> GetMails([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _mailService.GetMailsAsync(playerId, page, pageSize);
            return Ok(ApiResponse<MailPagedResultDto>.Ok(result));
        }

        /// <summary>
        /// 获取邮件详情。
        /// </summary>
        [HttpGet("{mailId:long}")]
        public async Task<ActionResult<ApiResponse<MailDetailDto>>> GetMailDetail(long mailId)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _mailService.GetMailDetailAsync(playerId, mailId);
            if (result == null) return NotFound(ApiResponse<MailDetailDto>.Fail("邮件不存在。"));

            return Ok(ApiResponse<MailDetailDto>.Ok(result));
        }

        /// <summary>
        /// 标记已读。
        /// </summary>
        [HttpPost("{mailId:long}/read")]
        public async Task<ActionResult<ApiResponse>> MarkAsRead(long mailId)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var success = await _mailService.MarkAsReadAsync(playerId, mailId);
            if (!success) return NotFound(ApiResponse.Fail("邮件不存在。"));

            return Ok(ApiResponse.Ok("已标记为已读。"));
        }

        /// <summary>
        /// 领取附件。
        /// </summary>
        [HttpPost("{mailId:long}/claim")]
        public async Task<ActionResult<ApiResponse<MailClaimResultDto>>> ClaimAttachments(long mailId)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            try
            {
                var result = await _mailService.ClaimAttachmentsAsync(playerId, mailId);
                return Ok(ApiResponse<MailClaimResultDto>.Ok(result, result.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<MailClaimResultDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 一键领取所有附件。
        /// </summary>
        [HttpPost("claim-all")]
        public async Task<ActionResult<ApiResponse<MailClaimAllResultDto>>> ClaimAll()
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            try
            {
                var result = await _mailService.ClaimAllAsync(playerId);
                return Ok(ApiResponse<MailClaimAllResultDto>.Ok(result, $"成功领取 {result.ClaimedCount} 封邮件附件。"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<MailClaimAllResultDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 删除邮件。
        /// </summary>
        [HttpDelete("{mailId:long}")]
        public async Task<ActionResult<ApiResponse>> DeleteMail(long mailId)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var success = await _mailService.DeleteMailAsync(playerId, mailId);
            if (!success) return NotFound(ApiResponse.Fail("邮件不存在或无权删除。"));

            return Ok(ApiResponse.Ok("邮件已删除。"));
        }

        /// <summary>
        /// 全部标为已读。
        /// </summary>
        [HttpPost("read-all")]
        public async Task<ActionResult<ApiResponse<int>>> MarkAllAsRead()
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var count = await _mailService.MarkAllAsReadAsync(playerId);
            return Ok(ApiResponse<int>.Ok(count, $"已将 {count} 封邮件标记为已读。"));
        }
    }
}
