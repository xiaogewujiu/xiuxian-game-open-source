using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 境界/突破配置后台服务。
    /// </summary>
    public interface IAdminRealmLevelConfigService
    {
        /// <summary>
        /// 获取境界配置列表。
        /// </summary>
        /// <returns>境界配置列表项集合。</returns>
        Task<List<AdminRealmLevelConfigListItemDto>> GetListAsync();

        /// <summary>
        /// 获取指定等级的境界配置详情。
        /// </summary>
        /// <param name="level">玩家等级。</param>
        /// <returns>境界配置详情；不存在时返回空。</returns>
        Task<AdminRealmLevelConfigDetailDto?> GetDetailAsync(int level);

        /// <summary>
        /// 保存指定等级的境界/突破配置。
        /// </summary>
        /// <param name="request">境界配置详情请求。</param>
        /// <returns>保存后的境界配置详情。</returns>
        Task<AdminRealmLevelConfigDetailDto> SaveAsync(AdminRealmLevelConfigDetailDto request);
    }
}
