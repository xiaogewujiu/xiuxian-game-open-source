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
    [Route("api/admin/image-collection-bonuses")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class ImageCollectionBonusesController : ControllerBase
    {
        private readonly IAdminCollectionService _service;

        public ImageCollectionBonusesController(IAdminCollectionService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminImageCollectionBonusListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminImageCollectionBonusListItemDto>>>> GetList([FromQuery] string? seriesId = null)
        {
            var list = await _service.GetImageBonusListAsync(seriesId);
            return Ok(ApiResponse<List<AdminImageCollectionBonusListItemDto>>.Ok(list));
        }

        [HttpGet("{bonusId}")]
        [ProducesResponseType(typeof(ApiResponse<AdminImageCollectionBonusDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminImageCollectionBonusDetailDto>>> GetDetail(string bonusId)
        {
            var detail = await _service.GetImageBonusDetailAsync(bonusId);
            if (detail == null) return NotFound(ApiResponse<AdminImageCollectionBonusDetailDto>.Fail("属性加成不存在。"));
            return Ok(ApiResponse<AdminImageCollectionBonusDetailDto>.Ok(detail));
        }

        [HttpPost]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminImageCollectionBonusDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminImageCollectionBonusDetailDto>>> Save([FromBody] AdminImageCollectionBonusDetailDto request)
        {
            try
            {
                var result = await _service.SaveImageBonusAsync(request);
                return Ok(ApiResponse<AdminImageCollectionBonusDetailDto>.Ok(result, "保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminImageCollectionBonusDetailDto>.Fail(ex.Message));
            }
        }

        [HttpDelete("{bonusId}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse>> Delete(string bonusId)
        {
            var deleted = await _service.DeleteImageBonusAsync(bonusId);
            if (!deleted) return NotFound(ApiResponse.Fail("属性加成不存在。"));
            return Ok(ApiResponse.Ok("删除成功"));
        }
    }
}
#pragma warning restore CS1591
