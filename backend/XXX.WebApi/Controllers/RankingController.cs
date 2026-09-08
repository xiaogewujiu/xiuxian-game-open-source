using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.Ranking;
using XXX.WebApi.Extensions;

namespace XXX.WebApi.Controllers
{
    /// <summary>
    /// 排行榜控制器
    /// 提供排行榜系统的API接口
    /// </summary>
    /// <remarks>
    /// API列表：
    /// - GET /api/ranking - 获取所有排行榜配置
    /// - GET /api/ranking/{rankingId} - 获取排行榜排名
    /// - GET /api/ranking/{rankingId}/stats - 获取排行榜统计
    /// - GET /api/ranking/{rankingId}/my-rank - 获取我的排名
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RankingController : ControllerBase
    {
        /// <summary>
        /// 排行榜服务
        /// </summary>
        private readonly IRankingService _rankingService;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="rankingService">排行榜服务</param>
        public RankingController(IRankingService rankingService)
        {
            _rankingService = rankingService;
        }

        /// <summary>
        /// 从JWT令牌中获取玩家ID
        /// </summary>
        /// <returns>玩家ID，如果未登录则返回null</returns>
        private string? GetPlayerId() => User.GetCurrentPlayerId();

        /// <summary>
        /// 获取所有排行榜配置
        /// </summary>
        /// <returns>排行榜配置列表</returns>
        /// <response code="200">成功获取排行榜列表</response>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<RankingConfig>>>> GetAllRankings()
        {
            var rankings = await _rankingService.GetAllRankingsAsync();
            return Ok(ApiResponse<List<RankingConfig>>.Ok(rankings));
        }

        /// <summary>
        /// 获取排行榜排名
        /// </summary>
        /// <param name="rankingId">排行榜ID</param>
        /// <param name="topCount">获取前N名，默认100</param>
        /// <returns>排名条目列表</returns>
        /// <response code="200">成功获取排名</response>
        [HttpGet("{rankingId}")]
        public async Task<ActionResult<ApiResponse<List<RankingEntry>>>> GetRanking(string rankingId, [FromQuery] int topCount = 100)
        {
            try
            {
                var ranking = await _rankingService.GetTopNAsync(rankingId, topCount);
                return Ok(ApiResponse<List<RankingEntry>>.Ok(ranking));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponse<List<RankingEntry>>.Fail("排行榜不存在或未启用。"));
            }
        }

        /// <summary>
        /// 获取排行榜统计信息
        /// </summary>
        /// <param name="rankingId">排行榜ID</param>
        /// <returns>统计信息</returns>
        /// <response code="200">成功获取统计</response>
        [HttpGet("{rankingId}/stats")]
        public async Task<ActionResult<ApiResponse<RankingStats>>> GetStats(string rankingId)
        {
            try
            {
                var stats = await _rankingService.GetRankingStatsAsync(rankingId);
                return Ok(ApiResponse<RankingStats>.Ok(stats));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponse<RankingStats>.Fail("排行榜不存在或未启用。"));
            }
        }

        /// <summary>
        /// 获取当前玩家在指定排行榜的排名
        /// </summary>
        /// <param name="rankingId">排行榜ID</param>
        /// <returns>排名，如果未上榜则返回404</returns>
        /// <response code="200">成功获取排名</response>
        /// <response code="401">未登录</response>
        /// <response code="404">未上榜</response>
        [HttpGet("{rankingId}/my-rank")]
        public async Task<ActionResult<ApiResponse<int?>>> GetMyRank(string rankingId)
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            int? rank;
            try
            {
                rank = await _rankingService.GetPlayerRankAsync(playerId, rankingId);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponse<int?>.Fail("排行榜不存在或未启用。"));
            }
            if (rank == null) return NotFound(ApiResponse<int?>.Fail("未上榜"));

            return Ok(ApiResponse<int?>.Ok(rank));
        }
    }
}
