using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 管理后台作物模板服务接口。
    /// </summary>
    public interface IAdminCropService
    {
        /// <summary>
        /// 获取作物模板列表。
        /// </summary>
        Task<List<AdminCropListItemDto>> GetListAsync(string? keyword = null);

        /// <summary>
        /// 获取作物模板详情。
        /// </summary>
        Task<AdminCropDetailDto?> GetDetailAsync(string templateId);

        /// <summary>
        /// 保存作物模板。
        /// </summary>
        Task<AdminCropDetailDto> SaveAsync(AdminCropDetailDto request);

        /// <summary>
        /// 删除作物模板。
        /// </summary>
        Task<bool> DeleteAsync(string templateId);
    }
}
