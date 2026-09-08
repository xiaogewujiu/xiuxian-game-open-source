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
    [Route("api/admin/image-collection-items")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class ImageCollectionItemsController : ControllerBase
    {
        private readonly IAdminCollectionService _service;

        public ImageCollectionItemsController(IAdminCollectionService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminImageCollectionItemListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminImageCollectionItemListItemDto>>>> GetList([FromQuery] string? seriesId = null)
        {
            var list = await _service.GetImageItemListAsync(seriesId);
            return Ok(ApiResponse<List<AdminImageCollectionItemListItemDto>>.Ok(list));
        }

        [HttpGet("{itemId}")]
        [ProducesResponseType(typeof(ApiResponse<AdminImageCollectionItemDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminImageCollectionItemDetailDto>>> GetDetail(string itemId)
        {
            var detail = await _service.GetImageItemDetailAsync(itemId);
            if (detail == null) return NotFound(ApiResponse<AdminImageCollectionItemDetailDto>.Fail("图片图鉴项不存在。"));
            return Ok(ApiResponse<AdminImageCollectionItemDetailDto>.Ok(detail));
        }

        [HttpPost]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminImageCollectionItemDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminImageCollectionItemDetailDto>>> Save([FromBody] AdminImageCollectionItemDetailDto request)
        {
            try
            {
                var result = await _service.SaveImageItemAsync(request);
                return Ok(ApiResponse<AdminImageCollectionItemDetailDto>.Ok(result, "保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminImageCollectionItemDetailDto>.Fail(ex.Message));
            }
        }

        [HttpDelete("{itemId}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> Delete(string itemId)
        {
            try
            {
                var deleted = await _service.DeleteImageItemAsync(itemId);
                if (!deleted) return NotFound(ApiResponse.Fail("图片图鉴项不存在。"));
                return Ok(ApiResponse.Ok("删除成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }
    }
}
#pragma warning restore CS1591
