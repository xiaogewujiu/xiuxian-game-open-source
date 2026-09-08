using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 管理后台 Buff 模板服务接口。
    /// </summary>
    public interface IAdminBuffService
    {
        /// <summary>
        /// 获取 Buff 模板列表。
        /// </summary>
        Task<List<AdminBuffListItemDto>> GetListAsync(string? keyword = null);

        /// <summary>
        /// 获取 Buff 模板详情。
        /// </summary>
        Task<AdminBuffDetailDto?> GetDetailAsync(string buffId);

        /// <summary>
        /// 保存 Buff 模板。
        /// </summary>
        Task<AdminBuffDetailDto> SaveAsync(AdminBuffDetailDto request);

        /// <summary>
        /// 删除 Buff 模板。
        /// </summary>
        Task<bool> DeleteAsync(string buffId);
    }
}
