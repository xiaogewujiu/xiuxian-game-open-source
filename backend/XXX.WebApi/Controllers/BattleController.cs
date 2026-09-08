using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.WebApi.Extensions;
using XXX.WebApi.Hubs;

namespace XXX.WebApi.Controllers
{
    /// <summary>
    /// 战斗控制器。
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BattleController : ControllerBase
    {
        private readonly IBattleService _battleService;
        private readonly IHubContext<PartyHub> _partyHubContext;

        /// <summary>
        /// 初始化战斗控制器。
        /// </summary>
        public BattleController(IBattleService battleService, IHubContext<PartyHub> partyHubContext)
        {
            _battleService = battleService;
            _partyHubContext = partyHubContext;
        }

        private string? GetPlayerId() => User.GetCurrentPlayerId();

        /// <summary>
        /// 发起一场普通战斗。
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<BattleResultDto>>> StartBattle([FromBody] BattleRequestDto request)
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var result = await _battleService.StartBattleAsync(playerId, request);
            if (IsBattleRejected(result))
            {
                return BadRequest(ApiResponse<BattleResultDto>.Fail(GetBattleMessage(result), result));
            }

            return Ok(ApiResponse<BattleResultDto>.Ok(result, GetBattleMessage(result)));
        }

        /// <summary>
        /// 获取普通地图列表。
        /// </summary>
        [HttpGet("maps")]
        public async Task<ActionResult<ApiResponse<List<BattleMapDto>>>> GetMaps()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var maps = await _battleService.GetAvailableMapsAsync(playerId);
            return Ok(ApiResponse<List<BattleMapDto>>.Ok(maps));
        }

        /// <summary>
        /// 获取可挑战的副本列表。
        /// </summary>
        [HttpGet("dungeons")]
        public async Task<ActionResult<ApiResponse<List<DungeonDto>>>> GetDungeons()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var dungeons = await _battleService.GetAvailableDungeonsAsync(playerId);
            return Ok(ApiResponse<List<DungeonDto>>.Ok(dungeons));
        }

        /// <summary>
        /// 开始单人普通地图离线挂机。
        /// </summary>
        [HttpPost("offline/start")]
        public async Task<ActionResult<ApiResponse<OfflineBattleStatusDto>>> StartOfflineBattle([FromBody] StartOfflineBattleRequestDto request)
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse<OfflineBattleStatusDto>.Fail("未登录"));
            }

            try
            {
                var result = await _battleService.StartOfflineBattleAsync(playerId, request);
                return Ok(ApiResponse<OfflineBattleStatusDto>.Ok(result, "已开始离线挂机"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<OfflineBattleStatusDto>.Fail(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<OfflineBattleStatusDto>.Fail(ex.Message, 404));
            }
        }

        /// <summary>
        /// 停止当前离线挂机并返回汇总。
        /// </summary>
        [HttpPost("offline/stop")]
        public async Task<ActionResult<ApiResponse<OfflineBattleSummaryDto>>> StopOfflineBattle()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse<OfflineBattleSummaryDto>.Fail("未登录"));
            }

            try
            {
                var result = await _battleService.StopOfflineBattleAsync(playerId);
                return Ok(ApiResponse<OfflineBattleSummaryDto>.Ok(result, "已停止离线挂机"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<OfflineBattleSummaryDto>.Fail(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<OfflineBattleSummaryDto>.Fail(ex.Message, 404));
            }
        }

        /// <summary>
        /// 获取当前离线挂机状态。
        /// </summary>
        [HttpGet("offline/status")]
        public async Task<ActionResult<ApiResponse<OfflineBattleStatusDto>>> GetOfflineBattleStatus()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse<OfflineBattleStatusDto>.Fail("未登录"));
            }

            try
            {
                var result = await _battleService.GetOfflineBattleStatusAsync(playerId);
                return Ok(ApiResponse<OfflineBattleStatusDto>.Ok(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<OfflineBattleStatusDto>.Fail(ex.Message, 404));
            }
        }

        /// <summary>
        /// 发起单人副本挑战。
        /// </summary>
        [HttpPost("dungeon/{dungeonId}")]
        public async Task<ActionResult<ApiResponse<BattleResultDto>>> ChallengeDungeon(string dungeonId)
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var result = await _battleService.ChallengeDungeonAsync(playerId, dungeonId);
            if (IsBattleRejected(result))
            {
                return BadRequest(ApiResponse<BattleResultDto>.Fail(GetBattleMessage(result), result));
            }

            return Ok(ApiResponse<BattleResultDto>.Ok(result, GetBattleMessage(result)));
        }

        /// <summary>
        /// 使用临时队伍挑战副本。
        /// </summary>
        [HttpPost("dungeon/{dungeonId}/party/{partyId}")]
        public async Task<ActionResult<ApiResponse<BattleResultDto>>> ChallengeDungeonWithParty(string dungeonId, string partyId, [FromBody] PartyDungeonChallengeRequestDto? request = null)
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var result = await _battleService.ChallengeDungeonWithPartyAsync(playerId, partyId, dungeonId, request?.RequestId);
            if (IsBattleRejected(result))
            {
                return BadRequest(ApiResponse<BattleResultDto>.Fail(GetBattleMessage(result), result));
            }

            var message = GetBattleMessage(result);
            var battleId = result.BattleId;
            result.PartyId = partyId;
            result.StartedByPlayerId = playerId;
            result.Message = message;

            // 中文注释：只在真实战斗流程返回后广播，BattleId 已由业务层贯穿本场挑战。
            await _partyHubContext.Clients
                .Group(PartyHub.GetPartyGroupName(partyId))
                .SendAsync("PartyBattleResolved", new PartyBattleResolvedEvent
                {
                    BattleId = battleId,
                    PartyId = partyId,
                    DungeonId = dungeonId,
                    DungeonName = result.MapName,
                    StartedByPlayerId = playerId,
                    Result = result
                });

            return Ok(ApiResponse<BattleResultDto>.Ok(result, message));
        }

        /// <summary>
        /// 按 BattleId 查询组队副本摘要。
        /// </summary>
        [HttpGet("party-record/{battleId}")]
        public async Task<ActionResult<ApiResponse<PartyBattleRecordDto>>> GetPartyBattleRecord(string battleId)
        {
            if (string.IsNullOrWhiteSpace(GetPlayerId()))
            {
                return Unauthorized(ApiResponse<PartyBattleRecordDto>.Fail("未登录"));
            }

            var record = await _battleService.GetPartyBattleRecordAsync(battleId);
            return record == null
                ? NotFound(ApiResponse<PartyBattleRecordDto>.Fail("组队副本摘要不存在", 404))
                : Ok(ApiResponse<PartyBattleRecordDto>.Ok(record));
        }

        private static bool IsBattleRejected(BattleResultDto result)
        {
            if (!result.Success)
            {
                return true;
            }

            var hasRounds = result.TotalRounds > 0 || result.Rounds > 0;
            var hasStages = result.Stages.Count > 0;
            var hasRoundLogs = result.RoundLogs.Count > 0;
            return !hasRounds && !hasStages && !hasRoundLogs && result.BattleLog.Count > 0;
        }

        private static string GetBattleMessage(BattleResultDto result)
        {
            if (!string.IsNullOrWhiteSpace(result.Message))
            {
                return result.Message;
            }

            if (result.BattleLog.Count > 0)
            {
                return result.BattleLog[0];
            }

            return result.IsWin ? "战斗胜利" : "战斗失败";
        }
    }
}
