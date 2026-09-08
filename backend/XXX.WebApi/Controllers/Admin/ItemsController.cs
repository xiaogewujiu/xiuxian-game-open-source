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
    [Route("api/admin/items")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class ItemsController : ControllerBase
    {
        private readonly IAdminItemService _adminItemService;

        public ItemsController(IAdminItemService adminItemService)
        {
            _adminItemService = adminItemService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminItemListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminItemListItemDto>>>> GetList([FromQuery] string? keyword = null, [FromQuery] int? type = null)
        {
            var items = await _adminItemService.GetListAsync(keyword, type);
            return Ok(ApiResponse<List<AdminItemListItemDto>>.Ok(items));
        }

        [HttpGet("{itemId}")]
        [ProducesResponseType(typeof(ApiResponse<AdminItemDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminItemDetailDto>>> GetDetail(string itemId)
        {
            var item = await _adminItemService.GetDetailAsync(itemId);
            if (item == null)
            {
                return NotFound(ApiResponse<AdminItemDetailDto>.Fail("道具模板不存在。"));
            }

            return Ok(ApiResponse<AdminItemDetailDto>.Ok(item));
        }

        [HttpPost]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminItemDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminItemDetailDto>>> Save([FromBody] AdminItemDetailDto request)
        {
            try
            {
                var item = await _adminItemService.SaveAsync(request);
                return Ok(ApiResponse<AdminItemDetailDto>.Ok(item, "道具模板保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminItemDetailDto>.Fail(ex.Message));
            }
        }

        [HttpDelete("{itemId}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> Delete(string itemId)
        {
            try
            {
                var deleted = await _adminItemService.DeleteAsync(itemId);
                if (!deleted)
                {
                    return NotFound(ApiResponse.Fail("道具模板不存在或删除失败。"));
                }

                return Ok(ApiResponse.Ok("道具模板删除成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }
    }
}

#pragma warning restore CS1591

