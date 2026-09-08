using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.Application.Security;
using XXX.Infrastructure.Authentication;

namespace XXX.WebApi.Controllers.Admin
{
    /// <summary>
    /// 管理后台运行时配置域控制器。
    /// 第 1 阶段用于查看当前缓存域状态并执行后台刷新。
    /// </summary>
    [ApiController]
    [Route("api/admin/runtime-configs")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class RuntimeConfigsController : ControllerBase
    {
        /// <summary>
        /// 运行时配置刷新服务。
        /// </summary>
        private readonly IAdminRuntimeRefreshService _runtimeRefreshService;

        /// <summary>
        /// 初始化运行时配置域控制器。
        /// </summary>
        /// <param name="runtimeRefreshService">运行时配置刷新服务。</param>
        public RuntimeConfigsController(IAdminRuntimeRefreshService runtimeRefreshService)
        {
            _runtimeRefreshService = runtimeRefreshService;
        }

        /// <summary>
        /// 获取所有已接入运行时域的当前状态。
        /// </summary>
        /// <returns>运行时域状态列表。</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminRuntimeConfigDomainDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminRuntimeConfigDomainDto>>>> GetList()
        {
            var configs = await _runtimeRefreshService.GetDomainStatusesAsync();
            return Ok(ApiResponse<List<AdminRuntimeConfigDomainDto>>.Ok(configs));
        }

        /// <summary>
        /// 刷新指定的运行时配置域。
        /// </summary>
        /// <param name="domain">运行时域名称。</param>
        /// <returns>本次刷新结果。</returns>
        [HttpPost("refresh/{domain}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminRuntimeConfigRefreshResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AdminRuntimeConfigRefreshResultDto>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminRuntimeConfigRefreshResultDto>>> RefreshDomain(string domain)
        {
            try
            {
                var result = await _runtimeRefreshService.RefreshDomainAsync(domain, ResolveOperatorName());
                if (!result.Success)
                {
                    return BadRequest(ApiResponse<AdminRuntimeConfigRefreshResultDto>.Fail(result.Message));
                }

                return Ok(ApiResponse<AdminRuntimeConfigRefreshResultDto>.Ok(result, result.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminRuntimeConfigRefreshResultDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 刷新全部已接入的运行时配置域。
        /// </summary>
        /// <returns>各运行时域的刷新结果集合。</returns>
        [HttpPost("refresh-all")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<List<AdminRuntimeConfigRefreshResultDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminRuntimeConfigRefreshResultDto>>>> RefreshAll()
        {
            var results = await _runtimeRefreshService.RefreshAllAsync(ResolveOperatorName());
            return Ok(ApiResponse<List<AdminRuntimeConfigRefreshResultDto>>.Ok(results, "已刷新全部已接入的运行时配置域。"));
        }

        /// <summary>
        /// 解析当前操作人的名称。
        /// 优先使用后台登录信息，解析不到时回退为 <c>system</c>。
        /// </summary>
        /// <returns>当前操作人名称。</returns>
        private string ResolveOperatorName()
        {
            return User.FindFirst(ClaimTypes.Name)?.Value
                ?? User.FindFirst(JwtClaims.UserName)?.Value
                ?? User.FindFirst("display_name")?.Value
                ?? "system";
        }
    }
}
