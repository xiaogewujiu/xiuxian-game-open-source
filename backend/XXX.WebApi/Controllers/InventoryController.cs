using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.WebApi.Extensions;

namespace XXX.WebApi.Controllers
{
    /// <summary>
    /// 背包控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<InventoryController> _logger;

        /// <summary>
        /// 初始化背包控制器。
        /// </summary>
        public InventoryController(IInventoryService inventoryService, ILogger<InventoryController> logger)
        {
            _inventoryService = inventoryService;
            _logger = logger;
        }

        private string? GetCurrentPlayerId() => User.GetCurrentPlayerId();

        /// <summary>
        /// 获取背包物品列表
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<InventoryItemDto>>>> GetInventory()
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var items = await _inventoryService.GetInventoryItemsAsync(playerId);
            return Ok(ApiResponse<List<InventoryItemDto>>.Ok(items));
        }

        /// <summary>
        /// 获取物品详情
        /// </summary>
        [HttpGet("{itemId}")]
        public async Task<ActionResult<ApiResponse<InventoryItemDto>>> GetItemDetail(long itemId)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var item = await _inventoryService.GetItemDetailAsync(playerId, itemId);
            if (item == null) return NotFound(ApiResponse<InventoryItemDto>.Fail("物品不存在"));

            return Ok(ApiResponse<InventoryItemDto>.Ok(item));
        }

        /// <summary>
        /// 使用物品
        /// </summary>
        [HttpPost("use")]
        public async Task<ActionResult<ApiResponse<ItemUseResultDto>>> UseItem([FromBody] UseItemRequestDto request)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _inventoryService.UseItemAsync(playerId, request);
            if (!result.Success)
            {
                return BadRequest(ApiResponse<ItemUseResultDto>.Fail(result.Message));
            }

            return Ok(ApiResponse<ItemUseResultDto>.Ok(result, result.Message));
        }

        [HttpPost("skill-books/decompose")]
        public async Task<ActionResult<ApiResponse<SkillBookDecomposeResultDto>>> DecomposeSkillBook([FromBody] SkillBookDecomposeRequestDto request)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            try
            {
                var result = await _inventoryService.DecomposeSkillBookAsync(playerId, request);
                return Ok(ApiResponse<SkillBookDecomposeResultDto>.Ok(result, "技能书分解成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<SkillBookDecomposeResultDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 丢弃物品
        /// </summary>
        [HttpPost("discard")]
        public async Task<ActionResult<ApiResponse<bool>>> DiscardItem([FromBody] DiscardItemRequestDto request)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _inventoryService.DiscardItemAsync(playerId, request);
            if (!result)
            {
                return BadRequest(ApiResponse<bool>.Fail("丢弃失败"));
            }

            return Ok(ApiResponse<bool>.Ok(true, "丢弃成功"));
        }

        /// <summary>
        /// 批量丢弃背包道具。
        /// </summary>
        [HttpPost("discard-batch")]
        public async Task<ActionResult<ApiResponse<BatchDiscardItemResultDto>>> DiscardItems([FromBody] BatchDiscardItemRequestDto request)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _inventoryService.DiscardItemsAsync(playerId, request);
            if (result.DiscardedItemCount == 0)
            {
                return BadRequest(ApiResponse<BatchDiscardItemResultDto>.Fail("没有可丢弃的道具"));
            }

            return Ok(ApiResponse<BatchDiscardItemResultDto>.Ok(result, $"成功丢弃 {result.DiscardedItemCount} 种道具，共 {result.DiscardedQuantity} 件"));
        }

        /// <summary>
        /// 整理背包
        /// </summary>
        [HttpPost("organize")]
        public async Task<ActionResult<ApiResponse<InventoryOrganizeResultDto>>> Organize()
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _inventoryService.OrganizeInventoryAsync(playerId);
            return Ok(ApiResponse<InventoryOrganizeResultDto>.Ok(result, "整理完成"));
        }

        /// <summary>
        /// 锁定道具
        /// </summary>
        [HttpPost("lock")]
        public async Task<ActionResult<ApiResponse<LockItemResultDto>>> LockItem([FromBody] LockItemRequestDto request)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _inventoryService.LockItemAsync(playerId, request);
            if (!result.Success)
            {
                return BadRequest(ApiResponse<LockItemResultDto>.Fail(result.Message));
            }

            return Ok(ApiResponse<LockItemResultDto>.Ok(result, result.Message));
        }

        /// <summary>
        /// 解锁道具
        /// </summary>
        [HttpPost("unlock")]
        public async Task<ActionResult<ApiResponse<LockItemResultDto>>> UnlockItem([FromBody] LockItemRequestDto request)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _inventoryService.UnlockItemAsync(playerId, request);
            if (!result.Success)
            {
                return BadRequest(ApiResponse<LockItemResultDto>.Fail(result.Message));
            }

            return Ok(ApiResponse<LockItemResultDto>.Ok(result, result.Message));
        }
    }
}
