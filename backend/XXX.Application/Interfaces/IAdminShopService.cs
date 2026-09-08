using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 管理后台商店配置服务接口。
    /// </summary>
    public interface IAdminShopService
    {
        /// <summary>
        /// 获取商店配置列表。
        /// </summary>
        Task<List<AdminShopConfigListItemDto>> GetConfigsAsync(string? keyword = null);

        /// <summary>
        /// 获取商店配置详情。
        /// </summary>
        Task<AdminShopConfigDetailDto?> GetConfigDetailAsync(string shopId);

        /// <summary>
        /// 保存商店配置。
        /// </summary>
        Task<AdminShopConfigDetailDto> SaveConfigAsync(AdminShopConfigDetailDto request);

        /// <summary>
        /// 删除商店配置。
        /// </summary>
        Task<bool> DeleteConfigAsync(string shopId);

        /// <summary>
        /// 获取商店商品列表。
        /// </summary>
        Task<List<AdminShopItemListItemDto>> GetItemsAsync(string? shopId = null);

        /// <summary>
        /// 获取商店商品详情。
        /// </summary>
        Task<AdminShopItemDetailDto?> GetItemDetailAsync(string gid);

        /// <summary>
        /// 保存商店商品。
        /// </summary>
        Task<AdminShopItemDetailDto> SaveItemAsync(AdminShopItemDetailDto request);

        /// <summary>
        /// 删除商店商品。
        /// </summary>
        Task<bool> DeleteItemAsync(string gid);
    }
}
