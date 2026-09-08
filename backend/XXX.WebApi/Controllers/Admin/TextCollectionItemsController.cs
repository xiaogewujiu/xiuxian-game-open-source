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
    [Route("api/admin/text-collection-items")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class TextCollectionItemsController : ControllerBase
    {
        private readonly IAdminCollectionService _service;

        public TextCollectionItemsController(IAdminCollectionService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminTextCollectionItemListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminTextCollectionItemListItemDto>>>> GetList([FromQuery] string? seriesId = null)
        {
            var list = await _service.GetTextItemListAsync(seriesId);
            return Ok(ApiResponse<List<AdminTextCollectionItemListItemDto>>.Ok(list));
        }

        [HttpGet("{itemId}")]
        [ProducesResponseType(typeof(ApiResponse<AdminTextCollectionItemDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminTextCollectionItemDetailDto>>> GetDetail(string itemId)
        {
            var detail = await _service.GetTextItemDetailAsync(itemId);
            if (detail == null) return NotFound(ApiResponse<AdminTextCollectionItemDetailDto>.Fail("文字图鉴项不存在。"));
            return Ok(ApiResponse<AdminTextCollectionItemDetailDto>.Ok(detail));
        }

        [HttpPost]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminTextCollectionItemDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminTextCollectionItemDetailDto>>> Save([FromBody] AdminTextCollectionItemDetailDto request)
        {
            try
            {
                var result = await _service.SaveTextItemAsync(request);
                return Ok(ApiResponse<AdminTextCollectionItemDetailDto>.Ok(result, "保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminTextCollectionItemDetailDto>.Fail(ex.Message));
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
                var deleted = await _service.DeleteTextItemAsync(itemId);
                if (!deleted) return NotFound(ApiResponse.Fail("文字图鉴项不存在。"));
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
