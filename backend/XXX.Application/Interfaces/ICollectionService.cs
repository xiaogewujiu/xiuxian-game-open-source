using XXX.Application.DTOs;
using XXX.Entity;

namespace XXX.Application.Interfaces
{
    public interface ICollectionService
    {
        Task<List<PlayerCollectionSeriesDto>> GetCollectionConfigAsync();
        Task<List<PlayerCollectionSeriesDto>> GetPlayerCollectionAsync(string playerId);
        Task<List<CollectionBonusSummaryDto>> GetPlayerBonusesAsync(string playerId);
        Task ApplyCollectionBonusesToEntityAsync(string playerId, UserEntity player);
        Task GrantRandomCollectionItemAsync(string playerId, string seriesId, int collectionType);
        Task InvalidatePlayerBonusCacheAsync(string playerId);
        Task InvalidateConfigCacheAsync();
    }
}
