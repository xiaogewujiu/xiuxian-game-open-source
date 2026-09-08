using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 寄售行服务接口
    /// </summary>
    public interface IMarketService
    {
        /// <summary>浏览寄售行</summary>
        Task<List<MarketListingDto>> BrowseAsync(string? itemType, string? currencyType, string? keyword,
            string? sortBy, string? sortOrder, int page, int pageSize);

        /// <summary>查看我的上架</summary>
        Task<List<MarketListingDto>> GetMyListingsAsync(string playerId);

        /// <summary>上架商品</summary>
        Task<long> ListAsync(string playerId, MarketListDto dto);

        /// <summary>下架商品</summary>
        Task CancelAsync(string playerId, long listingId);

        /// <summary>购买商品</summary>
        Task BuyAsync(string playerId, long listingId);

        /// <summary>交易历史</summary>
        Task<List<MarketHistoryDto>> GetHistoryAsync(string playerId, int page, int pageSize);
    }

    /// <summary>
    /// 后台寄售行管理服务接口
    /// </summary>
    public interface IAdminMarketService
    {
        /// <summary>获取寄售行配置</summary>
        Task<AdminMarketConfigDto> GetConfigAsync();

        /// <summary>更新寄售行配置</summary>
        Task UpdateConfigAsync(AdminMarketConfigDto dto);

        /// <summary>浏览商品（管理视角）</summary>
        Task<List<AdminMarketListingDto>> GetListingsAsync(string? status, string? keyword, int take);

        /// <summary>强制下架</summary>
        Task ForceCancelAsync(long listingId);

        /// <summary>交易日志</summary>
        Task<List<AdminMarketTransactionDto>> GetTransactionsAsync(string? keyword, int take);
    }
}
