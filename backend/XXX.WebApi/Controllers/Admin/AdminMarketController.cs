using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.Application.Security;

namespace XXX.WebApi.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/market")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class AdminMarketController : ControllerBase
    {
        private readonly IAdminMarketService _adminMarketService;

        public AdminMarketController(IAdminMarketService adminMarketService)
        {
            _adminMarketService = adminMarketService;
        }

        [HttpGet("config")]
        [ProducesResponseType(typeof(ApiResponse<AdminMarketConfigDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<AdminMarketConfigDto>>> GetConfig()
        {
            var result = await _adminMarketService.GetConfigAsync();
            return Ok(ApiResponse<AdminMarketConfigDto>.Ok(result));
        }

        [HttpPut("config")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse>> UpdateConfig([FromBody] AdminMarketConfigDto dto)
        {
            await _adminMarketService.UpdateConfigAsync(dto);
            return Ok(ApiResponse.Ok("寄售行配置已更新。"));
        }

        [HttpGet("listings")]
        [ProducesResponseType(typeof(ApiResponse<List<AdminMarketListingDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminMarketListingDto>>>> GetListings(
            [FromQuery] string? status, [FromQuery] string? keyword, [FromQuery] int take = 200)
        {
            var result = await _adminMarketService.GetListingsAsync(status, keyword, take);
            return Ok(ApiResponse<List<AdminMarketListingDto>>.Ok(result));
        }

        [HttpPost("listings/{id:long}/force-cancel")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse>> ForceCancel(long id)
        {
            try
            {
                await _adminMarketService.ForceCancelAsync(id);
                return Ok(ApiResponse.Ok("已强制下架。"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }

        [HttpGet("transactions")]
        [ProducesResponseType(typeof(ApiResponse<List<AdminMarketTransactionDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminMarketTransactionDto>>>> GetTransactions(
            [FromQuery] string? keyword, [FromQuery] int take = 200)
        {
            var result = await _adminMarketService.GetTransactionsAsync(keyword, take);
            return Ok(ApiResponse<List<AdminMarketTransactionDto>>.Ok(result));
        }
    }
}
