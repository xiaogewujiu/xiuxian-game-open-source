using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.Application.Security;

namespace XXX.WebApi.Controllers.Admin
{
    /// <summary>
    /// 管理后台首页控制器。
    /// 当前先提供最小可用摘要数据，后续再扩展更多运营统计。
    /// </summary>
    [ApiController]
    [Route("api/admin/dashboard")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class DashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _adminDashboardService;

        /// <summary>
        /// 初始化管理后台首页控制器。
        /// </summary>
        public DashboardController(IAdminDashboardService adminDashboardService)
        {
            _adminDashboardService = adminDashboardService;
        }

        /// <summary>
        /// 获取管理后台首页摘要。
        /// </summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<AdminDashboardSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<AdminDashboardSummaryDto>>> GetSummary()
        {
            var summary = await _adminDashboardService.GetSummaryAsync();
            return Ok(ApiResponse<AdminDashboardSummaryDto>.Ok(summary));
        }
    }
}
