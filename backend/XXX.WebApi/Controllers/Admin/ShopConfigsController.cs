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
    [Route("api/admin/shop-configs")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class ShopConfigsController : ControllerBase
    {
        private readonly IAdminShopService _adminShopService;

        public ShopConfigsController(IAdminShopService adminShopService)
        {
            _adminShopService = adminShopService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminShopConfigListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminShopConfigListItemDto>>>> GetList([FromQuery] string? keyword = null)
        {
            var shops = await _adminShopService.GetConfigsAsync(keyword);
            return Ok(ApiResponse<List<AdminShopConfigListItemDto>>.Ok(shops));
        }

        [HttpGet("{shopId}")]
        [ProducesResponseType(typeof(ApiResponse<AdminShopConfigDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminShopConfigDetailDto>>> GetDetail(string shopId)
        {
            var shop = await _adminShopService.GetConfigDetailAsync(shopId);
            if (shop == null)
            {
                return NotFound(ApiResponse<AdminShopConfigDetailDto>.Fail("商店配置不存在。"));
            }

            return Ok(ApiResponse<AdminShopConfigDetailDto>.Ok(shop));
        }

        [HttpPost]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminShopConfigDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminShopConfigDetailDto>>> Save([FromBody] AdminShopConfigDetailDto request)
        {
            try
            {
                var shop = await _adminShopService.SaveConfigAsync(request);
                return Ok(ApiResponse<AdminShopConfigDetailDto>.Ok(shop, "商店配置保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminShopConfigDetailDto>.Fail(ex.Message));
            }
        }

        [HttpDelete("{shopId}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> Delete(string shopId)
        {
            try
            {
                var deleted = await _adminShopService.DeleteConfigAsync(shopId);
                if (!deleted)
                {
                    return NotFound(ApiResponse.Fail("商店配置不存在或删除失败。"));
                }

                return Ok(ApiResponse.Ok("商店配置删除成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }
    }
}

#pragma warning restore CS1591

