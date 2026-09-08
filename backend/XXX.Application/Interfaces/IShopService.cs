using XXX.Application.DTOs;
using XXX.Entity;
using XXX.Shop;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 商店服务接口。
    /// </summary>
    /// <remarks>
    /// 该接口同时覆盖两层职责：
    /// 1. 商店领域能力，例如购买、出售、刷新。
    /// 2. 前后端对接能力，例如把商店信息组装成前端可直接展示的 DTO。
    /// </remarks>
    public interface IShopService
    {
        /// <summary>
        /// 初始化商店系统。
        /// </summary>
        /// <remarks>
        /// 初始化时需要确保数据库中的商店数据与当前 GameData 相匹配，
        /// 否则开发过程中很容易出现“数据库里还是旧商品 ID，但代码里已经没有对应模板”的问题。
        /// </remarks>
        Task InitializeAsync();

        /// <summary>
        /// 重新从数据库装载商店缓存。
        /// </summary>
        Task ReloadCacheAsync();

        /// <summary>
        /// 获取当前玩家可直接展示的商店列表。
        /// </summary>
        /// <param name="player">当前玩家实体。</param>
        /// <returns>已经完成前端展示字段拼装的商店列表。</returns>
        Task<List<ShopDto>> GetAccessibleShopViewsAsync(UserEntity player);

        /// <summary>
        /// 获取单个商店的前端展示详情。
        /// </summary>
        /// <param name="player">当前玩家实体。</param>
        /// <param name="shopId">商店 ID。</param>
        /// <returns>商店详情 DTO；若商店不存在或当前玩家不可访问则返回 null。</returns>
        Task<ShopDto?> GetShopViewAsync(UserEntity player, string shopId);

        /// <summary>
        /// 获取所有商店领域对象。
        /// </summary>
        /// <returns>商店领域对象列表。</returns>
        Task<List<ShopConfig>> GetAllShopsAsync();

        /// <summary>
        /// 获取指定商店领域对象。
        /// </summary>
        /// <param name="shopId">商店 ID。</param>
        /// <returns>商店领域对象；不存在时返回 null。</returns>
        Task<ShopConfig?> GetShopAsync(string shopId);

        /// <summary>
        /// 获取当前玩家可访问的商店领域对象列表。
        /// </summary>
        /// <param name="player">当前玩家实体。</param>
        /// <returns>可访问的商店领域对象列表。</returns>
        Task<List<ShopConfig>> GetAccessibleShopsAsync(UserEntity player);

        /// <summary>
        /// 购买道具。
        /// </summary>
        /// <param name="player">当前玩家实体。</param>
        /// <param name="shopId">商店 ID。</param>
        /// <param name="itemId">道具 ID。</param>
        /// <param name="count">购买数量。</param>
        /// <returns>购买结果。</returns>
        Task<ShopBuyResult> BuyItemAsync(UserEntity player, string shopId, string itemId, int count = 1);

        /// <summary>
        /// 购买装备。
        /// </summary>
        /// <param name="player">当前玩家实体。</param>
        /// <param name="shopId">商店 ID。</param>
        /// <param name="equipmentTemplateId">装备模板 ID。</param>
        /// <returns>购买结果。</returns>
        Task<ShopBuyResult> BuyEquipmentAsync(UserEntity player, string shopId, int equipmentTemplateId);

        /// <summary>
        /// 出售背包道具。
        /// </summary>
        /// <param name="player">当前玩家实体。</param>
        /// <param name="itemId">道具 ID。</param>
        /// <param name="count">出售数量。</param>
        /// <returns>出售结果。</returns>
        Task<ShopSellResult> SellItemAsync(UserEntity player, string itemId, int count = 1);

        Task<BatchSellInventoryItemsResultDto> SellItemsAsync(UserEntity player, BatchSellInventoryItemsRequestDto request);

        /// <summary>
        /// 出售装备。
        /// </summary>
        /// <param name="player">当前玩家实体。</param>
        /// <param name="equipment">装备实例。</param>
        /// <returns>出售结果。</returns>
        Task<ShopSellResult> SellEquipmentAsync(UserEntity player, EquipmentInstance equipment);

        /// <summary>
        /// 刷新指定商店库存。
        /// </summary>
        /// <param name="shopId">商店 ID。</param>
        /// <returns>是否刷新成功。</returns>
        Task<bool> RefreshShopAsync(string shopId);

        /// <summary>
        /// 刷新所有需要自动刷新的商店。
        /// </summary>
        Task RefreshAllShopsAsync();
    }
}
