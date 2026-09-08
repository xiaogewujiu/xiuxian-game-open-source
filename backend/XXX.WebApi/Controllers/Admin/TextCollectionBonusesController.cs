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
    [Route("api/admin/text-collection-bonuses")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class TextCollectionBonusesController : ControllerBase
    {
        private readonly IAdminCollectionService _service;

        public TextCollectionBonusesController(IAdminCollectionService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminTextCollectionBonusListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminTextCollectionBonusListItemDto>>>> GetList([FromQuery] string? seriesId = null)
        {
            var list = await _service.GetTextBonusListAsync(seriesId);
            return Ok(ApiResponse<List<AdminTextCollectionBonusListItemDto>>.Ok(list));
        }

        [HttpGet("{bonusId}")]
        [ProducesResponseType(typeof(ApiResponse<AdminTextCollectionBonusDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminTextCollectionBonusDetailDto>>> GetDetail(string bonusId)
        {
            var detail = await _service.GetTextBonusDetailAsync(bonusId);
            if (detail == null) return NotFound(ApiResponse<AdminTextCollectionBonusDetailDto>.Fail("属性加成不存在。"));
            return Ok(ApiResponse<AdminTextCollectionBonusDetailDto>.Ok(detail));
        }

        [HttpPost]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminTextCollectionBonusDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminTextCollectionBonusDetailDto>>> Save([FromBody] AdminTextCollectionBonusDetailDto request)
        {
            try
            {
                var result = await _service.SaveTextBonusAsync(request);
                return Ok(ApiResponse<AdminTextCollectionBonusDetailDto>.Ok(result, "保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminTextCollectionBonusDetailDto>.Fail(ex.Message));
            }
        }

        [HttpDelete("{bonusId}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse>> Delete(string bonusId)
        {
            var deleted = await _service.DeleteTextBonusAsync(bonusId);
            if (!deleted) return NotFound(ApiResponse.Fail("属性加成不存在。"));
            return Ok(ApiResponse.Ok("删除成功"));
        }
    }
}
#pragma warning restore CS1591
