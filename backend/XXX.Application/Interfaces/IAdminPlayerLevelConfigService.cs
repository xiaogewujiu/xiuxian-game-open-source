using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 等级成长配置后台服务。
    /// </summary>
    public interface IAdminPlayerLevelConfigService
    {
        /// <summary>
        /// 获取等级成长配置列表。
        /// </summary>
        /// <returns>等级成长配置列表项集合。</returns>
        Task<List<AdminPlayerLevelConfigListItemDto>> GetListAsync();

        /// <summary>
        /// 获取指定等级的成长配置详情。
        /// </summary>
        /// <param name="level">玩家等级。</param>
        /// <returns>等级成长配置详情；不存在时返回空。</returns>
        Task<AdminPlayerLevelConfigDetailDto?> GetDetailAsync(int level);

        /// <summary>
        /// 保存指定等级的成长配置。
        /// </summary>
        /// <param name="request">成长配置详情请求。</param>
        /// <returns>保存后的成长配置详情。</returns>
        Task<AdminPlayerLevelConfigDetailDto> SaveAsync(AdminPlayerLevelConfigDetailDto request);
    }
}
