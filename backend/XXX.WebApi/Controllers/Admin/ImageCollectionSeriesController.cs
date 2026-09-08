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
    [Route("api/admin/image-collection-series")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class ImageCollectionSeriesController : ControllerBase
    {
        private readonly IAdminCollectionService _service;

        public ImageCollectionSeriesController(IAdminCollectionService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminImageCollectionSeriesListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminImageCollectionSeriesListItemDto>>>> GetList([FromQuery] string? keyword = null)
        {
            var list = await _service.GetImageSeriesListAsync(keyword);
            return Ok(ApiResponse<List<AdminImageCollectionSeriesListItemDto>>.Ok(list));
        }

        [HttpGet("{seriesId}")]
        [ProducesResponseType(typeof(ApiResponse<AdminImageCollectionSeriesDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminImageCollectionSeriesDetailDto>>> GetDetail(string seriesId)
        {
            var detail = await _service.GetImageSeriesDetailAsync(seriesId);
            if (detail == null) return NotFound(ApiResponse<AdminImageCollectionSeriesDetailDto>.Fail("图片图鉴系列不存在。"));
            return Ok(ApiResponse<AdminImageCollectionSeriesDetailDto>.Ok(detail));
        }

        [HttpPost]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminImageCollectionSeriesDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminImageCollectionSeriesDetailDto>>> Save([FromBody] AdminImageCollectionSeriesDetailDto request)
        {
            try
            {
                var result = await _service.SaveImageSeriesAsync(request);
                return Ok(ApiResponse<AdminImageCollectionSeriesDetailDto>.Ok(result, "保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminImageCollectionSeriesDetailDto>.Fail(ex.Message));
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
                var deleted = await _service.DeleteImageSeriesAsync(seriesId);
                if (!deleted) return NotFound(ApiResponse.Fail("图片图鉴系列不存在。"));
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
