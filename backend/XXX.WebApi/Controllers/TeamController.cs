using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.WebApi.Extensions;

namespace XXX.WebApi.Controllers
{
    /// <summary>
    /// 临时副本队伍控制器。
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TeamController : ControllerBase
    {
        private readonly ITeamService _teamService;
        private readonly ILogger<TeamController> _logger;

        /// <summary>
        /// 初始化队伍控制器。
        /// </summary>
        public TeamController(ITeamService teamService, ILogger<TeamController> logger)
        {
            _teamService = teamService;
            _logger = logger;
        }

        /// <summary>
        /// 获取当前可加入的队伍列表。
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<PartySummaryDto>>>> GetAvailableParties()
        {
            var playerId = User.GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse<List<PartySummaryDto>>.Fail("未登录"));
            }

            var parties = await _teamService.GetAvailablePartiesAsync(playerId);
            return Ok(ApiResponse<List<PartySummaryDto>>.Ok(parties));
        }

        /// <summary>
        /// 获取当前玩家所在队伍详情。
        /// </summary>
        [HttpGet("me")]
        public async Task<ActionResult<ApiResponse<PartyDetailDto>>> GetCurrentParty()
        {
            var playerId = User.GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse<PartyDetailDto>.Fail("未登录"));
            }

            var party = await _teamService.GetCurrentPartyAsync(playerId);
            if (party == null)
            {
                return Ok(ApiResponse<PartyDetailDto>.Ok(new PartyDetailDto(), "当前未加入临时队伍"));
            }

            return Ok(ApiResponse<PartyDetailDto>.Ok(party));
        }

        /// <summary>
        /// 创建一个新的临时队伍。
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<PartyDetailDto>>> CreateParty([FromBody] CreatePartyRequestDto request)
        {
            var playerId = User.GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse<PartyDetailDto>.Fail("未登录"));
            }

            try
            {
                var party = await _teamService.CreatePartyAsync(playerId, request);
                _logger.LogInformation("Player {PlayerId} created temporary party {PartyId}", playerId, party.PartyId);
                return Ok(ApiResponse<PartyDetailDto>.Ok(party, "队伍创建成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<PartyDetailDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 加入指定队伍。
        /// </summary>
        [HttpPost("{partyId}/join")]
        public async Task<ActionResult<ApiResponse<PartyDetailDto>>> JoinParty(string partyId)
        {
            var playerId = User.GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse<PartyDetailDto>.Fail("未登录"));
            }

            try
            {
                var party = await _teamService.JoinPartyAsync(playerId, partyId);
                _logger.LogInformation("Player {PlayerId} joined temporary party {PartyId}", playerId, partyId);
                return Ok(ApiResponse<PartyDetailDto>.Ok(party, "加入队伍成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<PartyDetailDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 离开当前所在队伍。
        /// </summary>
        [HttpPost("me/leave")]
        public async Task<ActionResult<ApiResponse>> LeaveCurrentParty()
        {
            var playerId = User.GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            await _teamService.LeaveCurrentPartyAsync(playerId);
            _logger.LogInformation("Player {PlayerId} left current temporary party", playerId);
            return Ok(ApiResponse.Ok("已退出当前队伍"));
        }

        /// <summary>
        /// 切换队伍招募状态。
        /// </summary>
        [HttpPost("{partyId}/recruiting")]
        public async Task<ActionResult<ApiResponse<PartyDetailDto>>> ToggleRecruiting(string partyId)
        {
            var playerId = User.GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse<PartyDetailDto>.Fail("未登录"));
            }

            try
            {
                var party = await _teamService.ToggleRecruitingAsync(playerId, partyId);
                return Ok(ApiResponse<PartyDetailDto>.Ok(
                    party,
                    party.IsRecruiting ? "已开启招募" : "已关闭招募"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<PartyDetailDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 解散指定队伍。
        /// </summary>
        [HttpDelete("{partyId}")]
        public async Task<ActionResult<ApiResponse>> DismissParty(string partyId)
        {
            var playerId = User.GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            try
            {
                var success = await _teamService.DismissPartyAsync(playerId, partyId);
                if (!success)
                {
                    return NotFound(ApiResponse.Fail("队伍不存在或已解散", 404));
                }

                return Ok(ApiResponse.Ok("队伍已解散"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }
    }
}
