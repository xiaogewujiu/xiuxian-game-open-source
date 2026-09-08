using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.Application.Security;

namespace XXX.WebApi.Controllers.Admin
{
    /// <summary>
    /// 管理后台用户控制器。
    /// 提供管理员账号的查询、创建、更新和密码重置能力。
    /// </summary>
    [ApiController]
    [Route("api/admin/admin-users")]
    [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
    public class AdminUsersController : ControllerBase
    {
        private readonly IAdminUserService _adminUserService;

        /// <summary>
        /// 初始化后台用户控制器。
        /// </summary>
        public AdminUsersController(IAdminUserService adminUserService)
        {
            _adminUserService = adminUserService;
        }

        /// <summary>
        /// 获取管理员列表。
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminUserListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminUserListItemDto>>>> GetList([FromQuery] string? keyword = null, [FromQuery] bool? isActive = null)
        {
            var users = await _adminUserService.GetListAsync(keyword, isActive);
            return Ok(ApiResponse<List<AdminUserListItemDto>>.Ok(users));
        }

        /// <summary>
        /// 获取管理员详情。
        /// </summary>
        [HttpGet("{adminId}")]
        [ProducesResponseType(typeof(ApiResponse<AdminUserDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminUserDetailDto>>> GetDetail(string adminId)
        {
            var user = await _adminUserService.GetDetailAsync(adminId);
            if (user == null)
            {
                return NotFound(ApiResponse<AdminUserDetailDto>.Fail("管理员不存在。"));
            }

            return Ok(ApiResponse<AdminUserDetailDto>.Ok(user));
        }

        /// <summary>
        /// 创建管理员。
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<AdminUserDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminUserDetailDto>>> Create([FromBody] AdminCreateUserRequestDto request)
        {
            try
            {
                var user = await _adminUserService.CreateAsync(request);
                return Ok(ApiResponse<AdminUserDetailDto>.Ok(user, "管理员创建成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminUserDetailDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 更新管理员。
        /// </summary>
        [HttpPut("{adminId}")]
        [ProducesResponseType(typeof(ApiResponse<AdminUserDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminUserDetailDto>>> Update(string adminId, [FromBody] AdminUpdateUserRequestDto request)
        {
            try
            {
                var user = await _adminUserService.UpdateAsync(adminId, request);
                return Ok(ApiResponse<AdminUserDetailDto>.Ok(user, "管理员更新成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminUserDetailDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 重置管理员密码。
        /// </summary>
        [HttpPost("{adminId}/reset-password")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> ResetPassword(string adminId, [FromBody] AdminResetPasswordRequestDto request)
        {
            try
            {
                await _adminUserService.ResetPasswordAsync(adminId, request);
                return Ok(ApiResponse.Ok("管理员密码重置成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }
    }
}
