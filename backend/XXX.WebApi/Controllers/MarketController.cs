using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.WebApi.Extensions;

namespace XXX.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MarketController : ControllerBase
    {
        private readonly IMarketService _marketService;

        public MarketController(IMarketService marketService)
        {
            _marketService = marketService;
        }

        private string? GetCurrentPlayerId() => User.GetCurrentPlayerId();

        [HttpGet("browse")]
        public async Task<ActionResult<ApiResponse<List<MarketListingDto>>>> Browse(
            [FromQuery] string? itemType, [FromQuery] string? currencyType,
            [FromQuery] string? keyword, [FromQuery] string? sortBy,
            [FromQuery] string? sortOrder, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _marketService.BrowseAsync(itemType, currencyType, keyword, sortBy, sortOrder, page, pageSize);
            return Ok(ApiResponse<List<MarketListingDto>>.Ok(result));
        }

        [HttpGet("me")]
        public async Task<ActionResult<ApiResponse<List<MarketListingDto>>>> GetMyListings()
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _marketService.GetMyListingsAsync(playerId);
            return Ok(ApiResponse<List<MarketListingDto>>.Ok(result));
        }

        [HttpPost("list")]
        public async Task<ActionResult<ApiResponse<long>>> List([FromBody] MarketListDto dto)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            try
            {
                var id = await _marketService.ListAsync(playerId, dto);
                return Ok(ApiResponse<long>.Ok(id, "上架成功。"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<long>.Fail(ex.Message));
            }
        }

        [HttpPost("cancel")]
        public async Task<ActionResult<ApiResponse>> Cancel([FromBody] long listingId)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            try
            {
                await _marketService.CancelAsync(playerId, listingId);
                return Ok(ApiResponse.Ok("下架成功。"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }

        [HttpPost("buy")]
        public async Task<ActionResult<ApiResponse>> Buy([FromBody] long listingId)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            try
            {
                await _marketService.BuyAsync(playerId, listingId);
                return Ok(ApiResponse.Ok("购买成功。"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }

        [HttpGet("history")]
        public async Task<ActionResult<ApiResponse<List<MarketHistoryDto>>>> GetHistory(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _marketService.GetHistoryAsync(playerId, page, pageSize);
            return Ok(ApiResponse<List<MarketHistoryDto>>.Ok(result));
        }
    }
}
