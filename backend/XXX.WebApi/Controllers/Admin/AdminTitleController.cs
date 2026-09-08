using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.Application.Security;

namespace XXX.WebApi.Controllers.Admin
{
    /// <summary>
    /// 管理后台称号控制器。
    /// </summary>
    [ApiController]
    [Route("api/admin/title")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class AdminTitleController : ControllerBase
    {
        private readonly IAdminTitleService _adminTitleService;

        public AdminTitleController(IAdminTitleService adminTitleService)
        {
            _adminTitleService = adminTitleService;
        }

        /// <summary>
        /// 获取称号模板列表。
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminTitleListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminTitleListItemDto>>>> GetList(
            [FromQuery] string? keyword = null,
            [FromQuery] int take = 200)
        {
            var result = await _adminTitleService.GetListAsync(keyword, take);
            return Ok(ApiResponse<List<AdminTitleListItemDto>>.Ok(result));
        }

        /// <summary>
        /// 获取称号模板详情。
        /// </summary>
        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(ApiResponse<AdminTitleDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminTitleDetailDto>>> GetDetail(long id)
        {
            var result = await _adminTitleService.GetDetailAsync(id);
            if (result == null) return NotFound(ApiResponse<AdminTitleDetailDto>.Fail("称号模板不存在。"));

            return Ok(ApiResponse<AdminTitleDetailDto>.Ok(result));
        }

        /// <summary>
        /// 创建称号模板。
        /// </summary>
        [HttpPost]
        [Authorize(Policy = AdminPermissionCatalog.PlayerWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminTitleDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminTitleDetailDto>>> Create([FromBody] AdminTitleDetailDto dto)
        {
            try
            {
                var result = await _adminTitleService.CreateAsync(dto);
                return Ok(ApiResponse<AdminTitleDetailDto>.Ok(result, "称号模板创建成功。"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminTitleDetailDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 更新称号模板。
        /// </summary>
        [HttpPut("{id:long}")]
        [Authorize(Policy = AdminPermissionCatalog.PlayerWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminTitleDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminTitleDetailDto>>> Update(long id, [FromBody] AdminTitleDetailDto dto)
        {
            var result = await _adminTitleService.UpdateAsync(id, dto);
            if (result == null) return NotFound(ApiResponse<AdminTitleDetailDto>.Fail("称号模板不存在。"));

            return Ok(ApiResponse<AdminTitleDetailDto>.Ok(result, "称号模板更新成功。"));
        }

        /// <summary>
        /// 删除称号模板。
        /// </summary>
        [HttpDelete("{id:long}")]
        [Authorize(Policy = AdminPermissionCatalog.PlayerWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> Delete(long id)
        {
            var success = await _adminTitleService.DeleteAsync(id);
            if (!success) return NotFound(ApiResponse.Fail("称号模板不存在。"));

            return Ok(ApiResponse.Ok("称号模板已删除。"));
        }

        /// <summary>
        /// 授予玩家称号。
        /// </summary>
        [HttpPost("grant")]
        [Authorize(Policy = AdminPermissionCatalog.PlayerGrantPolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> GrantTitle([FromBody] AdminGrantTitleDto dto)
        {
            try
            {
                var success = await _adminTitleService.GrantTitleAsync(dto);
                if (!success) return BadRequest(ApiResponse.Fail("授予失败，称号不存在或玩家已拥有。"));

                return Ok(ApiResponse.Ok("称号已授予。"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 回收玩家称号。
        /// </summary>
        [HttpPost("revoke")]
        [Authorize(Policy = AdminPermissionCatalog.PlayerGrantPolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> RevokeTitle([FromBody] AdminGrantTitleDto dto)
        {
            try
            {
                var success = await _adminTitleService.RevokeTitleAsync(dto);
                if (!success) return BadRequest(ApiResponse.Fail("回收失败，玩家未拥有该称号。"));

                return Ok(ApiResponse.Ok("称号已回收。"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }
    }
}
