using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.Application.Security;

namespace XXX.WebApi.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/arena")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class AdminArenaController : ControllerBase
    {
        private readonly IAdminArenaService _adminArenaService;

        public AdminArenaController(IAdminArenaService adminArenaService)
        {
            _adminArenaService = adminArenaService;
        }

        /// <summary>
        /// 获取竞技场总览。
        /// </summary>
        [HttpGet("overview")]
        [ProducesResponseType(typeof(ApiResponse<AdminArenaOverviewDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<AdminArenaOverviewDto>>> GetOverview()
        {
            var result = await _adminArenaService.GetOverviewAsync();
            return Ok(ApiResponse<AdminArenaOverviewDto>.Ok(result));
        }

        /// <summary>
        /// 获取竞技场玩家列表。
        /// </summary>
        [HttpGet("players")]
        [ProducesResponseType(typeof(ApiResponse<List<AdminArenaPlayerDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminArenaPlayerDto>>>> GetPlayers(
            [FromQuery] string? keyword = null,
            [FromQuery] int take = 200)
        {
            var result = await _adminArenaService.GetPlayersAsync(keyword, take);
            return Ok(ApiResponse<List<AdminArenaPlayerDto>>.Ok(result));
        }

        /// <summary>
        /// 获取对战日志。
        /// </summary>
        [HttpGet("logs")]
        [ProducesResponseType(typeof(ApiResponse<List<AdminArenaBattleLogDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminArenaBattleLogDto>>>> GetLogs(
            [FromQuery] string? playerId = null,
            [FromQuery] int take = 100)
        {
            var result = await _adminArenaService.GetBattleLogsAsync(playerId, take);
            return Ok(ApiResponse<List<AdminArenaBattleLogDto>>.Ok(result));
        }

        /// <summary>
        /// 调整玩家积分。
        /// </summary>
        [HttpPost("adjust-points")]
        [Authorize(Policy = AdminPermissionCatalog.PlayerWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> AdjustPoints([FromBody] AdminAdjustPointsDto dto)
        {
            var success = await _adminArenaService.AdjustPointsAsync(dto);
            if (!success) return BadRequest(ApiResponse.Fail("玩家不在竞技场中。"));

            return Ok(ApiResponse.Ok("积分已调整。"));
        }

        /// <summary>
        /// 禁赛玩家。
        /// </summary>
        [HttpPost("ban")]
        [Authorize(Policy = AdminPermissionCatalog.PlayerWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> BanPlayer([FromBody] AdminBanPlayerDto dto)
        {
            var success = await _adminArenaService.BanPlayerAsync(dto);
            if (!success) return BadRequest(ApiResponse.Fail("玩家不在竞技场中。"));

            return Ok(ApiResponse.Ok("已禁赛。"));
        }

        /// <summary>
        /// 解禁玩家。
        /// </summary>
        [HttpPost("unban")]
        [Authorize(Policy = AdminPermissionCatalog.PlayerWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> UnbanPlayer([FromBody] AdminUnbanPlayerDto dto)
        {
            var success = await _adminArenaService.UnbanPlayerAsync(dto);
            if (!success) return BadRequest(ApiResponse.Fail("玩家不在竞技场中。"));

            return Ok(ApiResponse.Ok("已解禁。"));
        }

        /// <summary>
        /// 获取赛季信息。
        /// </summary>
        [HttpGet("season-info")]
        [ProducesResponseType(typeof(ApiResponse<AdminArenaSeasonDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<AdminArenaSeasonDto>>> GetSeasonInfo()
        {
            var result = await _adminArenaService.GetSeasonInfoAsync();
            return Ok(ApiResponse<AdminArenaSeasonDto>.Ok(result));
        }

        /// <summary>
        /// 手动结算赛季（发放奖励并开始新赛季）。
        /// </summary>
        [HttpPost("settle-season")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminSettleSeasonResultDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<AdminSettleSeasonResultDto>>> SettleSeason()
        {
            var result = await _adminArenaService.SettleSeasonAsync();
            return Ok(ApiResponse<AdminSettleSeasonResultDto>.Ok(result, $"第{result.SeasonNumber}赛季结算完成。"));
        }

        /// <summary>
        /// 重置赛季（清空所有玩家数据，从赛季1开始）。
        /// </summary>
        [HttpPost("reset-season")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse>> ResetSeason()
        {
            await _adminArenaService.ResetSeasonAsync();
            return Ok(ApiResponse.Ok("赛季已重置。"));
        }

        /// <summary>
        /// 调整赛季配置（时长/开关）。
        /// </summary>
        [HttpPost("adjust-season")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse>> AdjustSeason([FromBody] AdminAdjustSeasonDto dto)
        {
            var success = await _adminArenaService.AdjustSeasonAsync(dto);
            if (!success) return BadRequest(ApiResponse.Fail("赛季配置不存在。"));

            return Ok(ApiResponse.Ok("赛季配置已更新。"));
        }

        /// <summary>
        /// 获取赛季奖励列表。
        /// </summary>
        [HttpGet("season-rewards")]
        [ProducesResponseType(typeof(ApiResponse<List<AdminArenaSeasonRewardDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminArenaSeasonRewardDto>>>> GetSeasonRewards()
        {
            var result = await _adminArenaService.GetSeasonRewardsAsync();
            return Ok(ApiResponse<List<AdminArenaSeasonRewardDto>>.Ok(result));
        }

        /// <summary>
        /// 保存赛季奖励（新增或更新）。
        /// </summary>
        [HttpPost("season-rewards")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> SaveSeasonReward([FromBody] AdminSaveSeasonRewardDto dto)
        {
            try
            {
                var success = await _adminArenaService.SaveSeasonRewardAsync(dto);
                if (!success) return BadRequest(ApiResponse.Fail("保存失败。"));
                return Ok(ApiResponse.Ok("赛季奖励已保存。"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 删除赛季奖励。
        /// </summary>
        [HttpDelete("season-rewards/{gid}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> DeleteSeasonReward(string gid)
        {
            var success = await _adminArenaService.DeleteSeasonRewardAsync(gid);
            if (!success) return BadRequest(ApiResponse.Fail("奖励不存在。"));

            return Ok(ApiResponse.Ok("赛季奖励已删除。"));
        }
    }
}
