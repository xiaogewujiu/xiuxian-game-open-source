using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 管理后台玩家服务接口。
    /// </summary>
    public interface IAdminPlayerService
    {
        /// <summary>
        /// 获取玩家列表。
        /// </summary>
        Task<List<AdminPlayerListItemDto>> GetListAsync(string? keyword = null, bool? isOfflineBattling = null);

        /// <summary>
        /// 获取玩家详情。
        /// </summary>
        Task<AdminPlayerDetailDto?> GetDetailAsync(string playerId);

        /// <summary>
        /// 获取当前离线挂机玩家列表。
        /// </summary>
        Task<List<AdminOfflineBattleListItemDto>> GetOfflineBattlesAsync(string? keyword = null);

        /// <summary>
        /// 保存玩家基础信息。
        /// </summary>
        Task<AdminPlayerDetailDto> SaveAsync(AdminPlayerDetailDto request);

        /// <summary>
        /// 发放玩家货币或经验。
        /// </summary>
        Task<AdminPlayerDetailDto> GrantCurrencyAsync(string playerId, AdminGrantCurrencyRequestDto request);

        /// <summary>
        /// 发放玩家道具。
        /// </summary>
        Task<InventoryItemDto> GrantItemAsync(string playerId, AdminGrantItemRequestDto request);

        /// <summary>
        /// 封禁玩家。
        /// </summary>
        Task<AdminPlayerDetailDto> BanAsync(string playerId, AdminBanPlayerRequestDto request);

        /// <summary>
        /// 解封玩家。
        /// </summary>
        Task<AdminPlayerDetailDto> UnbanAsync(string playerId);

        /// <summary>
        /// 后台强制停止某个玩家的离线挂机。
        /// </summary>
        Task<AdminOfflineBattleSummaryDto> StopOfflineBattleAsync(string playerId);
    }
}
