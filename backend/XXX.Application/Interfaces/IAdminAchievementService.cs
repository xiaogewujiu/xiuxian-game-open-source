using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 管理后台成就配置服务接口。
    /// </summary>
    public interface IAdminAchievementService
    {
        /// <summary>
        /// 获取成就配置列表。
        /// </summary>
        Task<List<AdminAchievementListItemDto>> GetListAsync(string? keyword = null, int? difficulty = null);

        /// <summary>
        /// 获取成就配置详情。
        /// </summary>
        Task<AdminAchievementDetailDto?> GetDetailAsync(string achievementId);

        /// <summary>
        /// 保存成就配置。
        /// </summary>
        Task<AdminAchievementDetailDto> SaveAsync(AdminAchievementDetailDto request);

        /// <summary>
        /// 删除成就配置。
        /// </summary>
        Task<bool> DeleteAsync(string achievementId);
    }
}
