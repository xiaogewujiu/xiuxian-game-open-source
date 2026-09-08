#pragma warning disable CS1591
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.Application.Security;

namespace XXX.WebApi.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/shop-items")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class ShopItemsController : ControllerBase
    {
        private readonly IAdminShopService _adminShopService;

        public ShopItemsController(IAdminShopService adminShopService)
        {
            _adminShopService = adminShopService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminShopItemListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminShopItemListItemDto>>>> GetList([FromQuery] string? shopId = null)
        {
            var items = await _adminShopService.GetItemsAsync(shopId);
            return Ok(ApiResponse<List<AdminShopItemListItemDto>>.Ok(items));
        }

        [HttpGet("{gid}")]
        [ProducesResponseType(typeof(ApiResponse<AdminShopItemDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminShopItemDetailDto>>> GetDetail(string gid)
        {
            var item = await _adminShopService.GetItemDetailAsync(gid);
            if (item == null)
            {
                return NotFound(ApiResponse<AdminShopItemDetailDto>.Fail("商店商品不存在。"));
            }

            return Ok(ApiResponse<AdminShopItemDetailDto>.Ok(item));
        }

        [HttpPost]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminShopItemDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminShopItemDetailDto>>> Save([FromBody] AdminShopItemDetailDto request)
        {
            try
            {
                var item = await _adminShopService.SaveItemAsync(request);
                return Ok(ApiResponse<AdminShopItemDetailDto>.Ok(item, "商店商品保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminShopItemDetailDto>.Fail(ex.Message));
            }
        }

        [HttpDelete("{gid}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> Delete(string gid)
        {
            try
            {
                var deleted = await _adminShopService.DeleteItemAsync(gid);
                if (!deleted)
                {
                    return NotFound(ApiResponse.Fail("商店商品不存在或删除失败。"));
                }

                return Ok(ApiResponse.Ok("商店商品删除成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }
    }
}
#pragma warning restore CS1591
