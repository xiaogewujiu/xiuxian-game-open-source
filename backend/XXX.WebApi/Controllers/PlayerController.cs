using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.WebApi.Extensions;
using XXX.WebApi.Services;

namespace XXX.WebApi.Controllers
{
    /// <summary>
    /// 玩家控制器
    /// 提供玩家信息查询、更新等功能
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PlayerController : ControllerBase
    {
        /// <summary>
        /// 玩家核心服务。
        /// </summary>
        private readonly IPlayerService _playerService;

        /// <summary>
        /// 玩家控制器日志记录器。
        /// </summary>
        private readonly ILogger<PlayerController> _logger;

        /// <summary>
        /// 本地图片存储服务。
        /// 当前用于保存玩家头像。
        /// </summary>
        private readonly LocalAssetStorageService _assetStorageService;

        /// <summary>
        /// 初始化玩家控制器。
        /// </summary>
        /// <param name="playerService">玩家核心服务。</param>
        /// <param name="assetStorageService">本地图片存储服务。</param>
        /// <param name="logger">日志记录器。</param>
        public PlayerController(IPlayerService playerService, LocalAssetStorageService assetStorageService, ILogger<PlayerController> logger)
        {
            _playerService = playerService;
            _assetStorageService = assetStorageService;
            _logger = logger;
        }

        /// <summary>
        /// 获取当前登录玩家的基础信息。
        /// </summary>
        /// <returns>当前玩家基础信息。</returns>
        [HttpGet("me")]
        [ProducesResponseType(typeof(ApiResponse<PlayerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<PlayerDto>>> GetCurrentPlayer()
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse<PlayerDto>.Fail("未登录"));
            }

            var player = await _playerService.GetByIdAsync(playerId);
            if (player == null)
            {
                return NotFound(ApiResponse<PlayerDto>.Fail("玩家不存在"));
            }

            return Ok(ApiResponse<PlayerDto>.Ok(player));
        }

        /// <summary>
        /// 获取当前登录玩家的详细信息。
        /// </summary>
        /// <returns>当前玩家详细信息。</returns>
        [HttpGet("me/detail")]
        [ProducesResponseType(typeof(ApiResponse<PlayerDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<PlayerDetailDto>>> GetCurrentPlayerDetail()
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse<PlayerDetailDto>.Fail("未登录"));
            }

            var player = await _playerService.GetDetailAsync(playerId);
            if (player == null)
            {
                return NotFound(ApiResponse<PlayerDetailDto>.Fail("玩家不存在"));
            }

            return Ok(ApiResponse<PlayerDetailDto>.Ok(player));
        }

        /// <summary>
        /// 根据玩家编号获取玩家基础信息。
        /// </summary>
        /// <param name="id">玩家编号。</param>
        /// <returns>指定玩家基础信息。</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<PlayerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PlayerDto>>> GetPlayerById(string id)
        {
            var player = await _playerService.GetByIdAsync(id);
            if (player == null)
            {
                return NotFound(ApiResponse<PlayerDto>.Fail("玩家不存在"));
            }

            return Ok(ApiResponse<PlayerDto>.Ok(player));
        }

        /// <summary>
        /// 更新当前登录玩家的可编辑资料。
        /// </summary>
        /// <param name="request">更新请求。</param>
        /// <returns>更新后的玩家信息。</returns>
        [HttpPut("me")]
        [ProducesResponseType(typeof(ApiResponse<PlayerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<PlayerDto>>> UpdateCurrentPlayer([FromBody] UpdatePlayerRequestDto request)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse<PlayerDto>.Fail("未登录"));
            }

            var player = await _playerService.UpdateAsync(playerId, request);
            if (player == null)
            {
                return NotFound(ApiResponse<PlayerDto>.Fail("玩家不存在"));
            }

            return Ok(ApiResponse<PlayerDto>.Ok(player, "更新成功"));
        }

        [HttpGet("me/equipment-auto-sell")]
        public async Task<ActionResult<ApiResponse<EquipmentAutoSellSettingsDto>>> GetEquipmentAutoSellSettings()
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse<EquipmentAutoSellSettingsDto>.Fail("未登录"));
            var result = await _playerService.GetEquipmentAutoSellSettingsAsync(playerId);
            return result == null
                ? NotFound(ApiResponse<EquipmentAutoSellSettingsDto>.Fail("玩家不存在"))
                : Ok(ApiResponse<EquipmentAutoSellSettingsDto>.Ok(result));
        }

        [HttpPut("me/equipment-auto-sell")]
        public async Task<ActionResult<ApiResponse<EquipmentAutoSellSettingsDto>>> UpdateEquipmentAutoSellSettings([FromBody] UpdateEquipmentAutoSellSettingsRequestDto request)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse<EquipmentAutoSellSettingsDto>.Fail("未登录"));
            var result = await _playerService.UpdateEquipmentAutoSellSettingsAsync(playerId, request);
            return result == null
                ? NotFound(ApiResponse<EquipmentAutoSellSettingsDto>.Fail("玩家不存在"))
                : Ok(ApiResponse<EquipmentAutoSellSettingsDto>.Ok(result, "自动出售设置已保存"));
        }

        /// <summary>
        /// 上传当前玩家头像。
        /// </summary>
        /// <param name="file">头像图片文件。</param>
        /// <returns>更新头像后的玩家信息。</returns>
        [HttpPost("me/avatar")]
        [ProducesResponseType(typeof(ApiResponse<PlayerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<PlayerDto>>> UploadAvatar([FromForm] IFormFile file)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse<PlayerDto>.Fail("未登录"));
            }

            try
            {
                var uploadResult = await _assetStorageService.SaveImageAsync(file, "avatars", HttpContext.RequestAborted);
                var player = await _playerService.UpdateAvatarImageAsync(playerId, uploadResult.RelativePath);
                if (player == null)
                {
                    return NotFound(ApiResponse<PlayerDto>.Fail("玩家不存在"));
                }

                return Ok(ApiResponse<PlayerDto>.Ok(player, "头像上传成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<PlayerDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 为当前玩家增加经验。
        /// 当前主要用于测试、管理脚本和快速构造成长状态。
        /// </summary>
        /// <param name="request">经验变更请求。</param>
        /// <returns>更新后的玩家信息。</returns>
        [HttpPost("me/exp")]
        [ProducesResponseType(typeof(ApiResponse<PlayerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<PlayerDto>>> AddExp([FromBody] AddExpRequestDto request)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse<PlayerDto>.Fail("未登录"));
            }

            try
            {
                var player = await _playerService.AddExpAsync(playerId, request);
                if (player == null)
                {
                    return NotFound(ApiResponse<PlayerDto>.Fail("玩家不存在"));
                }

                return Ok(ApiResponse<PlayerDto>.Ok(player, "经验添加成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<PlayerDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 获取当前玩家属性加点总览。
        /// </summary>
        /// <returns>当前玩家属性点总览。</returns>
        [HttpGet("me/attribute-points")]
        [ProducesResponseType(typeof(ApiResponse<PlayerAttributePointOverviewDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PlayerAttributePointOverviewDto>>> GetAttributePointOverview()
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse<PlayerAttributePointOverviewDto>.Fail("未登录"));
            }

            var overview = await _playerService.GetAttributePointOverviewAsync(playerId);
            if (overview == null)
            {
                return NotFound(ApiResponse<PlayerAttributePointOverviewDto>.Fail("玩家不存在"));
            }

            return Ok(ApiResponse<PlayerAttributePointOverviewDto>.Ok(overview));
        }

        /// <summary>
        /// 为当前玩家分配 1 点属性点。
        /// </summary>
        /// <param name="request">属性点调整请求。</param>
        /// <returns>调整后的属性点总览。</returns>
        [HttpPost("me/attribute-points")]
        [ProducesResponseType(typeof(ApiResponse<PlayerAttributePointOverviewDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<PlayerAttributePointOverviewDto>>> AllocateAttributePoint([FromBody] AdjustPlayerAttributePointRequestDto request)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse<PlayerAttributePointOverviewDto>.Fail("未登录"));
            }

            try
            {
                var overview = await _playerService.AllocateAttributePointAsync(playerId, request);
                if (overview == null)
                {
                    return NotFound(ApiResponse<PlayerAttributePointOverviewDto>.Fail("玩家不存在"));
                }

                return Ok(ApiResponse<PlayerAttributePointOverviewDto>.Ok(overview, "加点成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<PlayerAttributePointOverviewDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 从当前玩家已投入的属性中返还 1 点属性点。
        /// </summary>
        /// <param name="request">属性点返还请求。</param>
        /// <returns>返还后的属性点总览。</returns>
        [HttpDelete("me/attribute-points")]
        [ProducesResponseType(typeof(ApiResponse<PlayerAttributePointOverviewDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<PlayerAttributePointOverviewDto>>> RefundAttributePoint([FromBody] AdjustPlayerAttributePointRequestDto request)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse<PlayerAttributePointOverviewDto>.Fail("未登录"));
            }

            try
            {
                var overview = await _playerService.RefundAttributePointAsync(playerId, request);
                if (overview == null)
                {
                    return NotFound(ApiResponse<PlayerAttributePointOverviewDto>.Fail("玩家不存在"));
                }

                return Ok(ApiResponse<PlayerAttributePointOverviewDto>.Ok(overview, "返还成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<PlayerAttributePointOverviewDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 执行一次真实突破。
        /// </summary>
        /// <returns>突破结果。</returns>
        [HttpPost("me/breakthrough")]
        [ProducesResponseType(typeof(ApiResponse<PlayerBreakthroughResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<PlayerBreakthroughResultDto>>> Breakthrough()
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse<PlayerBreakthroughResultDto>.Fail("未登录"));
            }

            try
            {
                var result = await _playerService.BreakthroughAsync(playerId);
                if (result == null)
                {
                    return NotFound(ApiResponse<PlayerBreakthroughResultDto>.Fail("玩家不存在"));
                }

                _logger.LogInformation("玩家 {PlayerId} 已完成突破处理，成功={Succeeded}，当前等级={Level}", playerId, result.Succeeded, result.CurrentLevel);
                return Ok(ApiResponse<PlayerBreakthroughResultDto>.Ok(result, result.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("玩家 {PlayerId} 的突破请求被拒绝：{Message}", playerId, ex.Message);
                return BadRequest(ApiResponse<PlayerBreakthroughResultDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 统一获取当前登录玩家ID。
        /// </summary>
        /// <remarks>
        /// 方法作用：统一封装令牌声明读取，避免控制器重复解析逻辑。
        /// 关键逻辑：优先读取标准NameIdentifier，不存在时回退读取历史uid。
        /// </remarks>
        private string? GetCurrentPlayerId()
        {
            return User.GetCurrentPlayerId();
        }
    }
}
