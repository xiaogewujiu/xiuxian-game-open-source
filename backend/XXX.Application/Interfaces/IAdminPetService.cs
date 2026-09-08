using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 管理后台灵宠模板服务接口。
    /// </summary>
    public interface IAdminPetService
    {
        /// <summary>
        /// 获取灵宠模板列表。
        /// </summary>
        Task<List<AdminPetListItemDto>> GetListAsync(string? keyword = null);

        /// <summary>
        /// 获取灵宠模板详情。
        /// </summary>
        Task<AdminPetDetailDto?> GetDetailAsync(string templateId);

        /// <summary>
        /// 保存灵宠模板。
        /// </summary>
        Task<AdminPetDetailDto> SaveAsync(AdminPetDetailDto request);

        /// <summary>
        /// 删除灵宠模板。
        /// </summary>
        Task<bool> DeleteAsync(string templateId);
    }
}
