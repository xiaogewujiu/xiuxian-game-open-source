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
    [Route("api/admin/text-collection-series")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class TextCollectionSeriesController : ControllerBase
    {
        private readonly IAdminCollectionService _service;

        public TextCollectionSeriesController(IAdminCollectionService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminTextCollectionSeriesListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminTextCollectionSeriesListItemDto>>>> GetList([FromQuery] string? keyword = null)
        {
            var list = await _service.GetTextSeriesListAsync(keyword);
            return Ok(ApiResponse<List<AdminTextCollectionSeriesListItemDto>>.Ok(list));
        }

        [HttpGet("{seriesId}")]
        [ProducesResponseType(typeof(ApiResponse<AdminTextCollectionSeriesDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminTextCollectionSeriesDetailDto>>> GetDetail(string seriesId)
        {
            var detail = await _service.GetTextSeriesDetailAsync(seriesId);
            if (detail == null) return NotFound(ApiResponse<AdminTextCollectionSeriesDetailDto>.Fail("文字图鉴系列不存在。"));
            return Ok(ApiResponse<AdminTextCollectionSeriesDetailDto>.Ok(detail));
        }

        [HttpPost]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminTextCollectionSeriesDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminTextCollectionSeriesDetailDto>>> Save([FromBody] AdminTextCollectionSeriesDetailDto request)
        {
            try
            {
                var result = await _service.SaveTextSeriesAsync(request);
                return Ok(ApiResponse<AdminTextCollectionSeriesDetailDto>.Ok(result, "保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminTextCollectionSeriesDetailDto>.Fail(ex.Message));
            }
        }

        [HttpDelete("{seriesId}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> Delete(string seriesId)
        {
            try
            {
                var deleted = await _service.DeleteTextSeriesAsync(seriesId);
                if (!deleted) return NotFound(ApiResponse.Fail("文字图鉴系列不存在。"));
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
