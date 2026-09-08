using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 管理后台保存后使用的运行时缓存刷新服务。
    /// </summary>
    public interface IAdminRuntimeRefreshService
    {
        /// <summary>
        /// 刷新运行时模板缓存。
        /// </summary>
        Task ReloadRuntimeTemplatesAsync();

        /// <summary>
        /// 刷新商店缓存。
        /// </summary>
        Task ReloadShopCacheAsync();

        /// <summary>
        /// 刷新排行榜缓存。
        /// </summary>
        Task ReloadRankingCacheAsync();

        /// <summary>
        /// 刷新任务配置缓存。
        /// </summary>
        Task ReloadQuestCacheAsync();

        /// <summary>
        /// 刷新成就配置缓存。
        /// </summary>
        Task ReloadAchievementCacheAsync();

        /// <summary>
        /// 刷新新手礼包缓存。
        /// </summary>
        Task ReloadStarterPackageCacheAsync();

        /// <summary>
        /// 刷新成长配置缓存。
        /// </summary>
        Task ReloadGrowthConfigCacheAsync();

        /// <summary>
        /// 刷新聚灵阵规则缓存。
        /// </summary>
        Task ReloadFiveElementRuleCacheAsync();

        /// <summary>
        /// 刷新灵田规则缓存。
        /// </summary>
        Task ReloadSpiritFieldRuleCacheAsync();

        /// <summary>
        /// 刷新炼丹职业规则缓存。
        /// </summary>
        Task ReloadAlchemyProfessionRuleCacheAsync();

        /// <summary>
        /// 刷新锻造职业规则缓存。
        /// </summary>
        Task ReloadForgeProfessionRuleCacheAsync();

        /// <summary>
        /// 刷新装备洗练规则缓存。
        /// </summary>
        Task ReloadEquipmentRerollRuleCacheAsync();

        /// <summary>
        /// 刷新元素克制矩阵缓存。
        /// </summary>
        Task ReloadElementRelationRuleCacheAsync();

        /// <summary>
        /// 获取所有运行时配置域状态。
        /// </summary>
        Task<List<AdminRuntimeConfigDomainDto>> GetDomainStatusesAsync();

        /// <summary>
        /// 刷新指定运行时配置域。
        /// </summary>
        Task<AdminRuntimeConfigRefreshResultDto> RefreshDomainAsync(string domain, string? operatorName = null);

        /// <summary>
        /// 刷新全部已接入的运行时配置域。
        /// </summary>
        Task<List<AdminRuntimeConfigRefreshResultDto>> RefreshAllAsync(string? operatorName = null);
    }
}
