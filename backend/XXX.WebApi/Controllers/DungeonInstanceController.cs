#pragma warning disable CS1591
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs.DungeonInstance;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;

namespace XXX.WebApi.Controllers
{
    [ApiController]
    [Route("api/dungeoninstance")]
    [Authorize]
    public class DungeonInstanceController : ControllerBase
    {
        private readonly IDungeonInstanceService _dungeonInstanceService;

        public DungeonInstanceController(IDungeonInstanceService dungeonInstanceService)
        {
            _dungeonInstanceService = dungeonInstanceService;
        }

        [HttpPost("enter")]
        [ProducesResponseType(typeof(ApiResponse<DungeonInstanceEnterResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<DungeonInstanceEnterResponseDto>>> Enter([FromBody] DungeonInstanceEnterRequestDto request)
        {
            var playerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse<DungeonInstanceEnterResponseDto>.Fail("未登录。"));
            }

            var result = await _dungeonInstanceService.EnterAsync(playerId, request.DungeonId);
            if (!result.Success)
            {
                return BadRequest(ApiResponse<DungeonInstanceEnterResponseDto>.Fail(result.Message));
            }

            return Ok(ApiResponse<DungeonInstanceEnterResponseDto>.Ok(result, result.Message));
        }

        [HttpGet("status")]
        [ProducesResponseType(typeof(ApiResponse<DungeonInstanceStatusDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<DungeonInstanceStatusDto>>> GetStatus()
        {
            var playerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse<DungeonInstanceStatusDto>.Fail("未登录。"));
            }

            var result = await _dungeonInstanceService.GetStatusAsync(playerId);
            return Ok(ApiResponse<DungeonInstanceStatusDto>.Ok(result));
        }

        [HttpGet("available")]
        [ProducesResponseType(typeof(ApiResponse<List<DungeonInstanceAvailableDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<DungeonInstanceAvailableDto>>>> GetAvailable()
        {
            var playerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse<List<DungeonInstanceAvailableDto>>.Fail("未登录。"));
            }

            var result = await _dungeonInstanceService.GetAvailableDungeonsAsync(playerId);
            return Ok(ApiResponse<List<DungeonInstanceAvailableDto>>.Ok(result));
        }

        [HttpPost("quit")]
        [ProducesResponseType(typeof(ApiResponse<DungeonInstanceSettlementDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<DungeonInstanceSettlementDto>>> Quit()
        {
            var playerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse<DungeonInstanceSettlementDto>.Fail("未登录。"));
            }

            var result = await _dungeonInstanceService.QuitAsync(playerId);
            if (!result.Success)
            {
                return BadRequest(ApiResponse<DungeonInstanceSettlementDto>.Fail(result.Reason));
            }

            return Ok(ApiResponse<DungeonInstanceSettlementDto>.Ok(result, "秘境结算完成"));
        }

        [HttpGet("history")]
        [ProducesResponseType(typeof(ApiResponse<List<DungeonInstanceHistoryDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<DungeonInstanceHistoryDto>>>> GetHistory([FromQuery] int limit = 20)
        {
            var playerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse<List<DungeonInstanceHistoryDto>>.Fail("未登录。"));
            }

            var result = await _dungeonInstanceService.GetHistoryAsync(playerId, limit);
            return Ok(ApiResponse<List<DungeonInstanceHistoryDto>>.Ok(result));
        }

        /// <summary>
        /// 获取调试状态快照（测试用，包含buff详情）。
        /// </summary>
        [HttpGet("debug/snapshot")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<string>>> DebugSnapshot()
        {
            var playerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse<string>.Fail("未登录。"));
            }

            var result = await _dungeonInstanceService.GetDebugSnapshotAsync(playerId);
            return Ok(ApiResponse<string>.Ok(result));
        }

        /// <summary>
        /// 获取战斗属性调试信息（测试用，显示buff加成后的实际战斗属性）。
        /// </summary>
        [HttpGet("debug/fighter-stats")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<string>>> DebugFighterStats()
        {
            var playerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse<string>.Fail("未登录。"));
            }

            var result = await _dungeonInstanceService.GetDebugFighterStatsAsync(playerId);
            return Ok(ApiResponse<string>.Ok(result));
        }

        /// <summary>
        /// 强制组队（测试用）。当前玩家为队长。
        /// </summary>
        [HttpPost("debug/force-party")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<string>>> ForceParty([FromBody] ForcePartyRequestDto request)
        {
            var playerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse<string>.Fail("未登录。"));
            }

            var result = await _dungeonInstanceService.ForcePartyAsync(playerId, request.TargetPlayerId);
            return Ok(ApiResponse<string>.Ok(result));
        }

        /// <summary>
        /// 获取玩家ID（测试用）。
        /// </summary>
        [HttpGet("debug/player-id")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        public ActionResult<ApiResponse<string>> GetPlayerId()
        {
            var playerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return Ok(ApiResponse<string>.Ok(playerId ?? "未登录"));
        }

        /// <summary>
        /// 强制触发指定事件（测试用）。
        /// </summary>
        [HttpPost("debug/force-event")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<string>>> ForceEvent([FromBody] ForceEventRequestDto request)
        {
            var playerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse<string>.Fail("未登录。"));
            }

            var result = await _dungeonInstanceService.ForceEventAsync(playerId, request.EventId);
            return Ok(ApiResponse<string>.Ok(result));
        }
    }

    public class ForceEventRequestDto
    {
        public string EventId { get; set; } = "";
    }

    public class ForcePartyRequestDto
    {
        public string TargetPlayerId { get; set; } = "";
    }
}
#pragma warning restore CS1591
