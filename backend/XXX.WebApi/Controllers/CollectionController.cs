#pragma warning disable CS1591
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
    public class CollectionController : ControllerBase
    {
        private readonly ICollectionService _collectionService;

        public CollectionController(ICollectionService collectionService)
        {
            _collectionService = collectionService;
        }

        private string? GetCurrentPlayerId() => User.GetCurrentPlayerId();

        /// <summary>
        /// 获取图鉴配置（所有系列和图鉴项）。
        /// </summary>
        [HttpGet("config")]
        public async Task<ActionResult<ApiResponse<List<PlayerCollectionSeriesDto>>>> GetConfig()
        {
            var config = await _collectionService.GetCollectionConfigAsync();
            return Ok(ApiResponse<List<PlayerCollectionSeriesDto>>.Ok(config));
        }

        /// <summary>
        /// 获取当前玩家的图鉴收集情况。
        /// </summary>
        [HttpGet("player")]
        public async Task<ActionResult<ApiResponse<List<PlayerCollectionSeriesDto>>>> GetPlayerCollection()
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId))
                return Unauthorized(ApiResponse.Fail("未登录或登录已过期"));

            var collections = await _collectionService.GetPlayerCollectionAsync(playerId);
            return Ok(ApiResponse<List<PlayerCollectionSeriesDto>>.Ok(collections));
        }

        /// <summary>
        /// 获取当前玩家的图鉴属性加成汇总。
        /// </summary>
        [HttpGet("bonus")]
        public async Task<ActionResult<ApiResponse<List<CollectionBonusSummaryDto>>>> GetBonus()
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId))
                return Unauthorized(ApiResponse.Fail("未登录或登录已过期"));

            var bonuses = await _collectionService.GetPlayerBonusesAsync(playerId);
            return Ok(ApiResponse<List<CollectionBonusSummaryDto>>.Ok(bonuses));
        }
    }
}
#pragma warning restore CS1591
