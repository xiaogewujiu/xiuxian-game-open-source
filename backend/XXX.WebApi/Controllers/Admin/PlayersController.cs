using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.Application.Security;

namespace XXX.WebApi.Controllers.Admin
{
    /// <summary>
    /// 管理后台玩家控制器。
    /// 当前提供列表、详情和基础资料维护能力。
    /// </summary>
    [ApiController]
    [Route("api/admin/players")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class PlayersController : ControllerBase
    {
        private readonly IAdminPlayerService _adminPlayerService;

        /// <summary>
        /// 初始化玩家控制器。
        /// </summary>
        public PlayersController(IAdminPlayerService adminPlayerService)
        {
            _adminPlayerService = adminPlayerService;
        }

        /// <summary>
        /// 获取玩家列表。
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminPlayerListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminPlayerListItemDto>>>> GetList([FromQuery] string? keyword = null, [FromQuery] bool? isOfflineBattling = null)
        {
            var players = await _adminPlayerService.GetListAsync(keyword, isOfflineBattling);
            return Ok(ApiResponse<List<AdminPlayerListItemDto>>.Ok(players));
        }

        /// <summary>
        /// 获取当前离线挂机玩家列表。
        /// </summary>
        [HttpGet("offline-battles")]
        [ProducesResponseType(typeof(ApiResponse<List<AdminOfflineBattleListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminOfflineBattleListItemDto>>>> GetOfflineBattles([FromQuery] string? keyword = null)
        {
            var players = await _adminPlayerService.GetOfflineBattlesAsync(keyword);
            return Ok(ApiResponse<List<AdminOfflineBattleListItemDto>>.Ok(players));
        }

        /// <summary>
        /// 获取玩家详情。
        /// </summary>
        [HttpGet("{playerId}")]
        [ProducesResponseType(typeof(ApiResponse<AdminPlayerDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminPlayerDetailDto>>> GetDetail(string playerId)
        {
            var player = await _adminPlayerService.GetDetailAsync(playerId);
            if (player == null)
            {
                return NotFound(ApiResponse<AdminPlayerDetailDto>.Fail("玩家不存在。"));
            }

            return Ok(ApiResponse<AdminPlayerDetailDto>.Ok(player));
        }

        /// <summary>
        /// 保存玩家基础信息。
        /// </summary>
        [HttpPost]
        [Authorize(Policy = AdminPermissionCatalog.PlayerWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminPlayerDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminPlayerDetailDto>>> Save([FromBody] AdminPlayerDetailDto request)
        {
            try
            {
                var player = await _adminPlayerService.SaveAsync(request);
                return Ok(ApiResponse<AdminPlayerDetailDto>.Ok(player, "玩家资料保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminPlayerDetailDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 发放玩家货币或经验。
        /// </summary>
        [HttpPost("{playerId}/grant-currency")]
        [Authorize(Policy = AdminPermissionCatalog.PlayerGrantPolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminPlayerDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminPlayerDetailDto>>> GrantCurrency(string playerId, [FromBody] AdminGrantCurrencyRequestDto request)
        {
            try
            {
                var player = await _adminPlayerService.GrantCurrencyAsync(playerId, request);
                return Ok(ApiResponse<AdminPlayerDetailDto>.Ok(player, "货币发放成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminPlayerDetailDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 发放玩家道具。
        /// </summary>
        [HttpPost("{playerId}/grant-item")]
        [Authorize(Policy = AdminPermissionCatalog.PlayerGrantPolicy)]
        [ProducesResponseType(typeof(ApiResponse<InventoryItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<InventoryItemDto>>> GrantItem(string playerId, [FromBody] AdminGrantItemRequestDto request)
        {
            try
            {
                var item = await _adminPlayerService.GrantItemAsync(playerId, request);
                return Ok(ApiResponse<InventoryItemDto>.Ok(item, "道具发放成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<InventoryItemDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 封禁玩家。
        /// </summary>
        [HttpPost("{playerId}/ban")]
        [Authorize(Policy = AdminPermissionCatalog.PlayerWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminPlayerDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminPlayerDetailDto>>> Ban(string playerId, [FromBody] AdminBanPlayerRequestDto request)
        {
            try
            {
                var player = await _adminPlayerService.BanAsync(playerId, request);
                return Ok(ApiResponse<AdminPlayerDetailDto>.Ok(player, "玩家封禁成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminPlayerDetailDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 解封玩家。
        /// </summary>
        [HttpPost("{playerId}/unban")]
        [Authorize(Policy = AdminPermissionCatalog.PlayerWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminPlayerDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminPlayerDetailDto>>> Unban(string playerId)
        {
            try
            {
                var player = await _adminPlayerService.UnbanAsync(playerId);
                return Ok(ApiResponse<AdminPlayerDetailDto>.Ok(player, "玩家解封成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminPlayerDetailDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 后台强制停止玩家离线挂机。
        /// </summary>
        [HttpPost("{playerId}/offline-stop")]
        [Authorize(Policy = AdminPermissionCatalog.PlayerWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminOfflineBattleSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminOfflineBattleSummaryDto>>> StopOfflineBattle(string playerId)
        {
            try
            {
                var summary = await _adminPlayerService.StopOfflineBattleAsync(playerId);
                return Ok(ApiResponse<AdminOfflineBattleSummaryDto>.Ok(summary, "离线挂机已停止"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminOfflineBattleSummaryDto>.Fail(ex.Message));
            }
        }
    }
}
