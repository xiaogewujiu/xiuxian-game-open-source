using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.Application.Security;

namespace XXX.WebApi.Controllers.Admin
{
    /// <summary>
    /// 管理后台聚灵阵控制器。
    /// </summary>
    [ApiController]
    [Route("api/admin/five-elements")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class FiveElementsController : ControllerBase
    {
        private readonly IAdminFiveElementService _adminFiveElementService;

        /// <summary>
        /// 初始化聚灵阵控制器。
        /// </summary>
        public FiveElementsController(IAdminFiveElementService adminFiveElementService)
        {
            _adminFiveElementService = adminFiveElementService;
        }

        /// <summary>
        /// 获取聚灵阵列表。
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminFiveElementListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminFiveElementListItemDto>>>> GetList([FromQuery] string? keyword = null)
        {
            var items = await _adminFiveElementService.GetListAsync(keyword);
            return Ok(ApiResponse<List<AdminFiveElementListItemDto>>.Ok(items));
        }

        /// <summary>
        /// 获取聚灵阵详情。
        /// </summary>
        [HttpGet("{playerId}")]
        [ProducesResponseType(typeof(ApiResponse<AdminFiveElementDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminFiveElementDetailDto>>> GetDetail(string playerId)
        {
            var detail = await _adminFiveElementService.GetDetailAsync(playerId);
            if (detail == null)
            {
                return NotFound(ApiResponse<AdminFiveElementDetailDto>.Fail("玩家不存在。"));
            }

            return Ok(ApiResponse<AdminFiveElementDetailDto>.Ok(detail));
        }

        /// <summary>
        /// 保存聚灵阵数据。
        /// </summary>
        [HttpPost]
        [Authorize(Policy = AdminPermissionCatalog.PlayerWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminFiveElementDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminFiveElementDetailDto>>> Save([FromBody] AdminFiveElementDetailDto request)
        {
            try
            {
                var detail = await _adminFiveElementService.SaveAsync(request);
                return Ok(ApiResponse<AdminFiveElementDetailDto>.Ok(detail, "聚灵阵数据保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminFiveElementDetailDto>.Fail(ex.Message));
            }
        }
    }
}
