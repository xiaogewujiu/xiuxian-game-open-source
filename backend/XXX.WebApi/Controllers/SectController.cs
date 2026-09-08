using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.WebApi.Extensions;

namespace XXX.WebApi.Controllers
{
    /// <summary>
    /// 宗门控制器
    /// 提供宗门浏览、加入、心法修炼、捐献、切磋、大比、Boss、商店、福利等宗门系统功能
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SectController : ControllerBase
    {
        private readonly ISectService _sectService;

        /// <summary>
        /// 初始化宗门控制器。
        /// </summary>
        public SectController(ISectService sectService)
        {
            _sectService = sectService;
        }

        private string? GetPlayerId() => User.GetCurrentPlayerId();

        /// <summary>
        /// 获取可用宗门列表
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<SectTemplateDto>>>> GetAvailableSects()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var sects = await _sectService.GetAvailableSectsAsync(playerId);
            return Ok(ApiResponse<List<SectTemplateDto>>.Ok(sects));
        }

        /// <summary>
        /// 获取宗门详情
        /// </summary>
        [HttpGet("{sectId}")]
        public async Task<ActionResult<ApiResponse<SectDetailDto>>> GetSectDetail(string sectId)
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var detail = await _sectService.GetSectDetailAsync(playerId, sectId);
            if (detail == null)
            {
                return NotFound(ApiResponse<SectDetailDto>.Fail("宗门不存在"));
            }

            return Ok(ApiResponse<SectDetailDto>.Ok(detail));
        }

        /// <summary>
        /// 加入宗门
        /// </summary>
        [HttpPost("join/{sectId}")]
        public async Task<ActionResult<ApiResponse<bool>>> JoinSect(string sectId)
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            try
            {
                var result = await _sectService.JoinSectAsync(playerId, sectId);
                return Ok(ApiResponse<bool>.Ok(result, result ? "加入成功" : "加入失败"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<bool>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 获取玩家心法进度
        /// </summary>
        [HttpGet("sutras")]
        public async Task<ActionResult<ApiResponse<List<PlayerSutraProgressDto>>>> GetPlayerSutras()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var sutras = await _sectService.GetPlayerSutrasAsync(playerId);
            return Ok(ApiResponse<List<PlayerSutraProgressDto>>.Ok(sutras));
        }

        /// <summary>
        /// 升级心法层级
        /// </summary>
        [HttpPost("sutras/{sutraId}/upgrade")]
        public async Task<ActionResult<ApiResponse<HeartSutraUpgradeResultDto>>> UpgradeSutraLayer(string sutraId)
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            try
            {
                var result = await _sectService.UpgradeSutraLayerAsync(playerId, sutraId);
                return Ok(ApiResponse<HeartSutraUpgradeResultDto>.Ok(result, result.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<HeartSutraUpgradeResultDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 获取捐献状态
        /// </summary>
        [HttpGet("donation/status")]
        public async Task<ActionResult<ApiResponse<DonationStatusDto>>> GetDonationStatus()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var status = await _sectService.GetDonationStatusAsync(playerId);
            return Ok(ApiResponse<DonationStatusDto>.Ok(status));
        }

        /// <summary>
        /// 执行捐献
        /// </summary>
        [HttpPost("donation")]
        public async Task<ActionResult<ApiResponse<DonationResultDto>>> Donate([FromBody] SectDonateRequestDto? request = null)
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            try
            {
                var result = await _sectService.DonateAsync(playerId, request?.GoldAmount ?? 0);
                return Ok(ApiResponse<DonationResultDto>.Ok(result, result.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<DonationResultDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 获取宗门弟子列表
        /// </summary>
        [HttpGet("disciples")]
        public async Task<ActionResult<ApiResponse<List<SectDiscipleDto>>>> GetDisciples()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var disciples = await _sectService.GetDisciplesAsync(playerId);
            return Ok(ApiResponse<List<SectDiscipleDto>>.Ok(disciples));
        }

        /// <summary>
        /// 切磋
        /// </summary>
        [HttpPost("spar/{targetPlayerId}")]
        public async Task<ActionResult<ApiResponse<SparResultDto>>> Spar(string targetPlayerId)
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            try
            {
                var result = await _sectService.SparAsync(playerId, targetPlayerId);
                return Ok(ApiResponse<SparResultDto>.Ok(result));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<SparResultDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 获取宗门大比状态
        /// </summary>
        [HttpGet("tournament/status")]
        public async Task<ActionResult<ApiResponse<SectTournamentStatusDto?>>> GetSectTournamentStatus()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var status = await _sectService.GetSectTournamentStatusAsync(playerId);
            return Ok(ApiResponse<SectTournamentStatusDto?>.Ok(status));
        }

        /// <summary>
        /// 获取宗门大比对战记录
        /// </summary>
        [HttpGet("tournament/{tournamentId}/matches")]
        public async Task<ActionResult<ApiResponse<List<SectTournamentMatchDto>>>> GetSectTournamentMatches(string tournamentId)
        {
            var matches = await _sectService.GetSectTournamentMatchesAsync(tournamentId);
            return Ok(ApiResponse<List<SectTournamentMatchDto>>.Ok(matches));
        }

        /// <summary>
        /// 领取宗门大比奖励
        /// </summary>
        [HttpPost("tournament/{tournamentId}/claim")]
        public async Task<ActionResult<ApiResponse<SectTournamentRewardDto?>>> ClaimSectTournamentReward(string tournamentId)
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var reward = await _sectService.ClaimSectTournamentRewardAsync(playerId, tournamentId);
            if (reward == null)
            {
                return NotFound(ApiResponse<SectTournamentRewardDto?>.Fail("无可领取的奖励"));
            }

            return Ok(ApiResponse<SectTournamentRewardDto?>.Ok(reward, "奖励领取成功"));
        }

        /// <summary>
        /// 获取天骄赛状态
        /// </summary>
        [HttpGet("genius-tournament/status")]
        public async Task<ActionResult<ApiResponse<GeniusTournamentStatusDto?>>> GetGeniusTournamentStatus()
        {
            var status = await _sectService.GetGeniusTournamentStatusAsync();
            return Ok(ApiResponse<GeniusTournamentStatusDto?>.Ok(status));
        }

        /// <summary>
        /// 获取天骄赛对战记录
        /// </summary>
        [HttpGet("genius-tournament/{tournamentId}/matches")]
        public async Task<ActionResult<ApiResponse<List<GeniusTournamentMatchDto>>>> GetGeniusTournamentMatches(string tournamentId)
        {
            var matches = await _sectService.GetGeniusTournamentMatchesAsync(tournamentId);
            return Ok(ApiResponse<List<GeniusTournamentMatchDto>>.Ok(matches));
        }

        /// <summary>
        /// 领取天骄赛奖励
        /// </summary>
        [HttpPost("genius-tournament/{tournamentId}/claim")]
        public async Task<ActionResult<ApiResponse<GeniusTournamentRewardDto?>>> ClaimGeniusTournamentReward(string tournamentId)
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var reward = await _sectService.ClaimGeniusTournamentRewardAsync(playerId, tournamentId);
            if (reward == null)
            {
                return NotFound(ApiResponse<GeniusTournamentRewardDto?>.Fail("无可领取的奖励"));
            }

            return Ok(ApiResponse<GeniusTournamentRewardDto?>.Ok(reward, "奖励领取成功"));
        }

        /// <summary>
        /// 获取宗门Boss状态
        /// </summary>
        [HttpGet("boss/status")]
        public async Task<ActionResult<ApiResponse<SectBossStatusDto?>>> GetSectBossStatus()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var status = await _sectService.GetSectBossStatusAsync(playerId);
            return Ok(ApiResponse<SectBossStatusDto?>.Ok(status));
        }

        /// <summary>
        /// 攻击宗门Boss
        /// </summary>
        [HttpPost("boss/attack")]
        public async Task<ActionResult<ApiResponse<SectBossActionResultDto>>> AttackSectBoss()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            try
            {
                var result = await _sectService.AttackSectBossAsync(playerId);
                return Ok(ApiResponse<SectBossActionResultDto>.Ok(result));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<SectBossActionResultDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 领取宗门Boss奖励
        /// </summary>
        [HttpPost("boss/claim")]
        public async Task<ActionResult<ApiResponse<SectBossRewardDto?>>> ClaimSectBossReward()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var reward = await _sectService.ClaimSectBossRewardAsync(playerId);
            if (reward == null)
            {
                return NotFound(ApiResponse<SectBossRewardDto?>.Fail("无可领取的奖励"));
            }

            return Ok(ApiResponse<SectBossRewardDto?>.Ok(reward, "奖励领取成功"));
        }

        /// <summary>
        /// 获取宗门商店商品列表
        /// </summary>
        [HttpGet("shop")]
        public async Task<ActionResult<ApiResponse<List<SectShopItemDto>>>> GetSectShopItems()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var items = await _sectService.GetSectShopItemsAsync(playerId);
            return Ok(ApiResponse<List<SectShopItemDto>>.Ok(items));
        }

        /// <summary>
        /// 购买宗门商店商品
        /// </summary>
        [HttpPost("shop/{shopItemId}/purchase")]
        public async Task<ActionResult<ApiResponse<SectShopPurchaseResultDto>>> PurchaseSectShopItem(string shopItemId)
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            try
            {
                var result = await _sectService.PurchaseSectShopItemAsync(playerId, shopItemId);
                return Ok(ApiResponse<SectShopPurchaseResultDto>.Ok(result, result.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<SectShopPurchaseResultDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 获取宗门福利列表
        /// </summary>
        [HttpGet("blessings")]
        public async Task<ActionResult<ApiResponse<List<SectBlessingDto>>>> GetSectBlessings()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var blessings = await _sectService.GetSectBlessingsAsync(playerId);
            return Ok(ApiResponse<List<SectBlessingDto>>.Ok(blessings));
        }

        /// <summary>
        /// 领取宗门福利
        /// </summary>
        [HttpPost("blessings/{blessingId}/claim")]
        public async Task<ActionResult<ApiResponse<bool>>> ClaimSectBlessing(string blessingId)
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _sectService.ClaimSectBlessingAsync(playerId, blessingId);
            return Ok(ApiResponse<bool>.Ok(result, result ? "福利领取成功" : "福利领取失败"));
        }

        /// <summary>
        /// 获取宗门任务列表
        /// </summary>
        [HttpGet("tasks")]
        public async Task<ActionResult<ApiResponse<List<SectTaskDto>>>> GetSectTasks()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var tasks = await _sectService.GetSectTasksAsync(playerId);
            return Ok(ApiResponse<List<SectTaskDto>>.Ok(tasks));
        }
    }
}
