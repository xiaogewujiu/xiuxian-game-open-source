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
    [Route("api/admin/lottery-logs")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class LotteryLogsController : ControllerBase
    {
        private readonly IAdminLotteryService _service;

        public LotteryLogsController(IAdminLotteryService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<AdminLotteryLogListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PagedResult<AdminLotteryLogListItemDto>>>> GetLogs(
            [FromQuery] string? playerId = null,
            [FromQuery] string? poolId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var (items, total) = await _service.GetLogsAsync(playerId, poolId, page, pageSize);
            return Ok(ApiResponse<PagedResult<AdminLotteryLogListItemDto>>.Ok(
                new PagedResult<AdminLotteryLogListItemDto> { Items = items, Total = total, Page = page, PageSize = pageSize }));
        }
    }
}
#pragma warning restore CS1591
