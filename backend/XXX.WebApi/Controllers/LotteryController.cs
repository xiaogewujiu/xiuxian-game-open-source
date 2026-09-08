#pragma warning disable CS1591
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Repositories;
using XXX.WebApi.Extensions;

namespace XXX.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LotteryController : ControllerBase
    {
        private readonly ILotteryService _lotteryService;
        private readonly IRepository<UserEntity> _userRepository;

        public LotteryController(ILotteryService lotteryService, IRepository<UserEntity> userRepository)
        {
            _lotteryService = lotteryService;
            _userRepository = userRepository;
        }

        private string? GetCurrentPlayerId() => User.GetCurrentPlayerId();

        /// <summary>
        /// 获取可用抽奖池列表。
        /// </summary>
        [HttpGet("list")]
        public async Task<ActionResult<ApiResponse<List<LotteryPoolViewDto>>>> GetPools()
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId))
                return Unauthorized(ApiResponse.Fail("未登录或登录已过期"));

            var pools = await _lotteryService.GetLotteryPoolsAsync(playerId);
            return Ok(ApiResponse<List<LotteryPoolViewDto>>.Ok(pools));
        }

        /// <summary>
        /// 执行抽奖。
        /// </summary>
        [HttpPost("draw")]
        public async Task<ActionResult<ApiResponse<LotteryDrawResultDto>>> Draw([FromBody] LotteryDrawRequestDto request)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId))
                return Unauthorized(ApiResponse.Fail("未登录或登录已过期"));

            var player = await _userRepository.GetByIdAsync(playerId);
            if (player == null)
                return NotFound(ApiResponse.Fail("玩家不存在"));

            try
            {
                var result = await _lotteryService.DrawAsync(player, request);
                return Ok(ApiResponse<LotteryDrawResultDto>.Ok(result, "抽奖成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<LotteryDrawResultDto>.Fail(ex.Message));
            }
        }
    }
}
#pragma warning restore CS1591
