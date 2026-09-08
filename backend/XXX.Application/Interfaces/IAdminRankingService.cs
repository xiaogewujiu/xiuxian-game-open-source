using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 管理后台排行配置服务接口。
    /// </summary>
    public interface IAdminRankingService
    {
        /// <summary>
        /// 获取排行配置列表。
        /// </summary>
        Task<List<AdminRankingConfigListItemDto>> GetConfigsAsync(string? keyword = null);

        /// <summary>
        /// 获取排行配置详情。
        /// </summary>
        Task<AdminRankingConfigDetailDto?> GetConfigDetailAsync(string rankingId);

        /// <summary>
        /// 保存排行配置。
        /// </summary>
        Task<AdminRankingConfigDetailDto> SaveConfigAsync(AdminRankingConfigDetailDto request);

        /// <summary>
        /// 删除排行配置。
        /// </summary>
        Task<bool> DeleteConfigAsync(string rankingId);

        /// <summary>
        /// 获取排行奖励列表。
        /// </summary>
        Task<List<AdminRankingRewardListItemDto>> GetRewardsAsync(string? rankingId = null);

        /// <summary>
        /// 获取排行奖励详情。
        /// </summary>
        Task<AdminRankingRewardDetailDto?> GetRewardDetailAsync(string gid);

        /// <summary>
        /// 保存排行奖励。
        /// </summary>
        Task<AdminRankingRewardDetailDto> SaveRewardAsync(AdminRankingRewardDetailDto request);

        /// <summary>
        /// 删除排行奖励。
        /// </summary>
        Task<bool> DeleteRewardAsync(string gid);
    }
}
