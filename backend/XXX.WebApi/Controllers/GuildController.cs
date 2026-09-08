using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.WebApi.Extensions;

namespace XXX.WebApi.Controllers
{
    /// <summary>
    /// 公会控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GuildController : ControllerBase
    {
        private readonly IGuildService _guildService;

        /// <summary>
        /// 初始化宗门控制器。
        /// </summary>
        public GuildController(IGuildService guildService)
        {
            _guildService = guildService;
        }

        private string? GetPlayerId() => User.GetCurrentPlayerId();

        /// <summary>
        /// 获取公会列表
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<GuildDto>>>> GetGuilds()
        {
            var guilds = await _guildService.GetGuildsAsync();
            return Ok(ApiResponse<List<GuildDto>>.Ok(guilds));
        }

        /// <summary>
        /// 创建公会
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<GuildDto>>> CreateGuild([FromBody] CreateGuildRequestDto request)
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var guild = await _guildService.CreateGuildAsync(playerId, request);
            return Ok(ApiResponse<GuildDto>.Ok(guild, "创建成功"));
        }

        /// <summary>
        /// 加入公会
        /// </summary>
        [HttpPost("join/{guildId}")]
        public async Task<ActionResult<ApiResponse<bool>>> JoinGuild(string guildId)
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _guildService.JoinGuildAsync(playerId, guildId);
            return Ok(ApiResponse<bool>.Ok(result, result ? "加入成功" : "加入失败"));
        }

        /// <summary>
        /// 退出公会
        /// </summary>
        [HttpPost("leave")]
        public async Task<ActionResult<ApiResponse<bool>>> LeaveGuild()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _guildService.LeaveGuildAsync(playerId);
            return Ok(ApiResponse<bool>.Ok(result, result ? "退出成功" : "退出失败"));
        }
    }
}
