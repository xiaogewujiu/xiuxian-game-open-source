using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.Application.Security;

namespace XXX.WebApi.Controllers.Admin
{
    /// <summary>
    /// 管理后台灵田系统控制器。
    /// </summary>
    [ApiController]
    [Route("api/admin/spirit-fields")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class SpiritFieldsController : ControllerBase
    {
        private readonly IAdminSpiritFieldService _adminSpiritFieldService;

        /// <summary>
        /// 初始化灵田系统控制器。
        /// </summary>
        public SpiritFieldsController(IAdminSpiritFieldService adminSpiritFieldService)
        {
            _adminSpiritFieldService = adminSpiritFieldService;
        }

        /// <summary>
        /// 获取灵田系统列表。
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminSpiritFieldListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminSpiritFieldListItemDto>>>> GetList([FromQuery] string? keyword = null)
        {
            var items = await _adminSpiritFieldService.GetListAsync(keyword);
            return Ok(ApiResponse<List<AdminSpiritFieldListItemDto>>.Ok(items));
        }

        /// <summary>
        /// 获取灵田系统详情。
        /// </summary>
        [HttpGet("{playerId}")]
        [ProducesResponseType(typeof(ApiResponse<AdminSpiritFieldDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminSpiritFieldDetailDto>>> GetDetail(string playerId)
        {
            var detail = await _adminSpiritFieldService.GetDetailAsync(playerId);
            if (detail == null)
            {
                return NotFound(ApiResponse<AdminSpiritFieldDetailDto>.Fail("玩家不存在。"));
            }

            return Ok(ApiResponse<AdminSpiritFieldDetailDto>.Ok(detail));
        }

        /// <summary>
        /// 保存灵田系统数据。
        /// </summary>
        [HttpPost]
        [Authorize(Policy = AdminPermissionCatalog.PlayerWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminSpiritFieldDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminSpiritFieldDetailDto>>> Save([FromBody] AdminSpiritFieldDetailDto request)
        {
            try
            {
                var detail = await _adminSpiritFieldService.SaveAsync(request);
                return Ok(ApiResponse<AdminSpiritFieldDetailDto>.Ok(detail, "灵田系统数据保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminSpiritFieldDetailDto>.Fail(ex.Message));
            }
        }
    }
}
