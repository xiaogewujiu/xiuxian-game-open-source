using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 管理后台用户服务接口。
    /// </summary>
    public interface IAdminUserService
    {
        /// <summary>
        /// 获取管理员列表。
        /// </summary>
        Task<List<AdminUserListItemDto>> GetListAsync(string? keyword = null, bool? isActive = null);

        /// <summary>
        /// 获取管理员详情。
        /// </summary>
        Task<AdminUserDetailDto?> GetDetailAsync(string adminId);

        /// <summary>
        /// 创建管理员。
        /// </summary>
        Task<AdminUserDetailDto> CreateAsync(AdminCreateUserRequestDto request);

        /// <summary>
        /// 更新管理员。
        /// </summary>
        Task<AdminUserDetailDto> UpdateAsync(string adminId, AdminUpdateUserRequestDto request);

        /// <summary>
        /// 重置管理员密码。
        /// </summary>
        Task<bool> ResetPasswordAsync(string adminId, AdminResetPasswordRequestDto request);
    }
}
