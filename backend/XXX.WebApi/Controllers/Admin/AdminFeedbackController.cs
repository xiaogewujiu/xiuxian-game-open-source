using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.Application.Security;

namespace XXX.WebApi.Controllers.Admin
{
    /// <summary>
    /// 管理后台建议反馈控制器。
    /// </summary>
    [ApiController]
    [Route("api/admin/feedback")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class AdminFeedbackController : ControllerBase
    {
        private readonly IFeedbackService _service;

        /// <summary>初始化后台反馈控制器。</summary>
        public AdminFeedbackController(IFeedbackService service)
        {
            _service = service;
        }

        /// <summary>分页查询反馈列表。</summary>
        [HttpGet]
        [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
        public async Task<ActionResult<ApiResponse<PagedResult<FeedbackListItemDto>>>> GetList(
            [FromQuery] string? keyword = null,
            [FromQuery] string? type = null,
            [FromQuery] string? status = null,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20)
        {
            var (items, total) = await _service.AdminGetListAsync(keyword, type, status, pageIndex, pageSize);
            return Ok(ApiResponse<PagedResult<FeedbackListItemDto>>.Ok(new PagedResult<FeedbackListItemDto>
            {
                Items = items, Total = total, Page = pageIndex, PageSize = pageSize
            }));
        }

        /// <summary>获取反馈详情。</summary>
        [HttpGet("{feedbackId:long}")]
        [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
        public async Task<ActionResult<ApiResponse<FeedbackDetailDto>>> GetDetail(long feedbackId)
        {
            var result = await _service.AdminGetDetailAsync(feedbackId);
            return result == null
                ? NotFound(ApiResponse<FeedbackDetailDto>.Fail("反馈不存在。"))
                : Ok(ApiResponse<FeedbackDetailDto>.Ok(result));
        }

        /// <summary>处理反馈并记录状态历史。</summary>
        [HttpPost("{feedbackId:long}/process")]
        [Authorize(Policy = AdminPermissionCatalog.PlayerWritePolicy)]
        public async Task<ActionResult<ApiResponse<FeedbackDetailDto>>> Process(long feedbackId, [FromBody] ProcessFeedbackRequestDto request)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(adminId)) return Unauthorized(ApiResponse.Fail("管理员身份无效。"));
            try
            {
                var result = await _service.ProcessAsync(feedbackId, adminId, request);
                return result == null
                    ? NotFound(ApiResponse<FeedbackDetailDto>.Fail("反馈不存在。"))
                    : Ok(ApiResponse<FeedbackDetailDto>.Ok(result, "反馈处理成功。"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<FeedbackDetailDto>.Fail(ex.Message));
            }
        }
    }
}
