using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Repositories;
using XXX.Shop;
using XXX.WebApi.Extensions;

namespace XXX.WebApi.Controllers
{
    /// <summary>
    /// 商店控制器。
    /// </summary>
    /// <remarks>
    /// 该控制器对外暴露的结果全部尽量使用前端直接可消费的 DTO，
    /// 目的是减少“后端返回的是领域对象，前端还要自己拼字段”的对接成本。
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ShopController : ControllerBase
    {
        private readonly IShopService _shopService;
        private readonly IRepository<UserEntity> _userRepository;
        private readonly ILogger<ShopController> _logger;

        /// <summary>
        /// 初始化商店控制器。
        /// </summary>
        public ShopController(IShopService shopService, IRepository<UserEntity> userRepository, ILogger<ShopController> logger)
        {
            _shopService = shopService;
            _userRepository = userRepository;
            _logger = logger;
        }

        private string? GetCurrentPlayerId() => User.GetCurrentPlayerId();

        /// <summary>
        /// 获取当前玩家可访问的商店列表。
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<ShopDto>>>> GetShops()
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录或登录已过期"));
            }

            var player = await _userRepository.GetByIdAsync(playerId);
            if (player == null)
            {
                return NotFound(ApiResponse.Fail("玩家不存在"));
            }

            var shops = await _shopService.GetAccessibleShopViewsAsync(player);
            return Ok(ApiResponse<List<ShopDto>>.Ok(shops, "获取商店列表成功"));
        }

        /// <summary>
        /// 获取商店详情。
        /// </summary>
        [HttpGet("{shopId}")]
        public async Task<ActionResult<ApiResponse<ShopDto>>> GetShop(string shopId)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录或登录已过期"));
            }

            var player = await _userRepository.GetByIdAsync(playerId);
            if (player == null)
            {
                return NotFound(ApiResponse.Fail("玩家不存在"));
            }

            var shop = await _shopService.GetShopAsync(shopId);
            if (shop == null)
            {
                return NotFound(ApiResponse<ShopDto>.Fail("商店不存在"));
            }

            if (!shop.IsAccessible(player))
            {
                return BadRequest(ApiResponse<ShopDto>.Fail("当前等级不足，无法访问该商店"));
            }

            var detail = await _shopService.GetShopViewAsync(player, shopId);
            if (detail == null)
            {
                return NotFound(ApiResponse<ShopDto>.Fail("商店不存在"));
            }

            return Ok(ApiResponse<ShopDto>.Ok(detail, "获取商店详情成功"));
        }

        /// <summary>
        /// 购买商品。
        /// </summary>
        /// <remarks>
        /// 该接口会根据商店商品的真实类型自动分流为“买道具”或“买装备”，
        /// 这样前端不需要为了同一个商店页再拆两套购买接口。
        /// </remarks>
        [HttpPost("buy")]
        public async Task<ActionResult<ApiResponse<ShopBuyResult>>> BuyItem([FromBody] BuyItemRequestDto request)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录或登录已过期"));
            }

            var player = await _userRepository.GetByIdAsync(playerId);
            if (player == null)
            {
                return NotFound(ApiResponse.Fail("玩家不存在"));
            }

            var shop = await _shopService.GetShopAsync(request.ShopId);
            if (shop == null)
            {
                return NotFound(ApiResponse<ShopBuyResult>.Fail("商店不存在"));
            }

            var targetShopItem = shop.Items.FirstOrDefault(i => i.ItemId == request.ItemId);
            if (targetShopItem == null)
            {
                return NotFound(ApiResponse<ShopBuyResult>.Fail("商品不存在"));
            }

            ShopBuyResult result;
            if (targetShopItem.ItemType == ShopItemType.Equipment)
            {
                if (!int.TryParse(request.ItemId, out var templateId))
                {
                    return BadRequest(ApiResponse<ShopBuyResult>.Fail("装备模板 ID 非法"));
                }

                if (request.Count != 1)
                {
                    return BadRequest(ApiResponse<ShopBuyResult>.Fail("装备每次只能购买 1 件"));
                }

                result = await _shopService.BuyEquipmentAsync(player, request.ShopId, templateId);
            }
            else
            {
                result = await _shopService.BuyItemAsync(player, request.ShopId, request.ItemId, request.Count);
            }

            if (!result.Success)
            {
                return BadRequest(ApiResponse<ShopBuyResult>.Fail(result.Message));
            }

            return Ok(ApiResponse<ShopBuyResult>.Ok(result, result.Message));
        }

        /// <summary>
        /// 出售背包道具。
        /// </summary>
        [HttpPost("sell")]
        public async Task<ActionResult<ApiResponse<ShopSellResult>>> SellItem([FromBody] SellItemRequestDto request)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录或登录已过期"));
            }

            var player = await _userRepository.GetByIdAsync(playerId);
            if (player == null)
            {
                return NotFound(ApiResponse.Fail("玩家不存在"));
            }

            var result = await _shopService.SellItemAsync(player, request.ItemId, request.Count);
            if (!result.Success)
            {
                return BadRequest(ApiResponse<ShopSellResult>.Fail(result.Message));
            }

            return Ok(ApiResponse<ShopSellResult>.Ok(result, result.Message));
        }

        /// <summary>
        /// 批量出售背包道具。
        /// </summary>
        [HttpPost("sell-batch")]
        public async Task<ActionResult<ApiResponse<BatchSellInventoryItemsResultDto>>> SellItems([FromBody] BatchSellInventoryItemsRequestDto request)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录或登录已过期"));
            }

            var player = await _userRepository.GetByIdAsync(playerId);
            if (player == null)
            {
                return NotFound(ApiResponse.Fail("玩家不存在"));
            }

            var result = await _shopService.SellItemsAsync(player, request);
            if (result.SoldItemCount == 0)
            {
                return BadRequest(ApiResponse<BatchSellInventoryItemsResultDto>.Fail("没有可出售的道具"));
            }

            return Ok(ApiResponse<BatchSellInventoryItemsResultDto>.Ok(result, $"成功出售 {result.SoldItemCount} 种道具，共 {result.SoldQuantity} 件，获得 {result.GoldEarned} 金币"));
        }

        /// <summary>
        /// 刷新商店库存。
        /// </summary>
        [HttpPost("{shopId}/refresh")]
        public async Task<ActionResult<ApiResponse<bool>>> RefreshShop(string shopId)
        {
            var result = await _shopService.RefreshShopAsync(shopId);
            if (!result)
            {
                return BadRequest(ApiResponse<bool>.Fail("刷新失败"));
            }

            return Ok(ApiResponse<bool>.Ok(true, "刷新成功"));
        }
    }
}
