using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.Application.Security;

namespace XXX.WebApi.Controllers.Admin
{
    /// <summary>
    /// 管理后台审计日志控制器。
    /// </summary>
    [ApiController]
    [Route("api/admin/audit-logs")]
    [Authorize(Policy = AdminPermissionCatalog.AuditReadPolicy)]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAdminAuditService _adminAuditService;

        /// <summary>
        /// 初始化审计日志控制器。
        /// </summary>
        public AuditLogsController(IAdminAuditService adminAuditService)
        {
            _adminAuditService = adminAuditService;
        }

        /// <summary>
        /// 获取审计日志列表。
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminAuditLogListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminAuditLogListItemDto>>>> GetList(
            [FromQuery] string? keyword = null,
            [FromQuery] int take = 200,
            [FromQuery] bool? success = null)
        {
            var logs = await _adminAuditService.GetListAsync(keyword, take, success);
            return Ok(ApiResponse<List<AdminAuditLogListItemDto>>.Ok(logs));
        }

        /// <summary>
        /// 获取单条审计日志详情。
        /// </summary>
        [HttpGet("{logId}")]
        [ProducesResponseType(typeof(ApiResponse<AdminAuditLogDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminAuditLogDetailDto>>> GetDetail(string logId)
        {
            var log = await _adminAuditService.GetDetailAsync(logId);
            if (log == null)
            {
                return NotFound(ApiResponse<AdminAuditLogDetailDto>.Fail("审计日志不存在。"));
            }

            return Ok(ApiResponse<AdminAuditLogDetailDto>.Ok(log));
        }
    }
}
