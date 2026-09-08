using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 好感度服务接口。
    /// </summary>
    public interface IFavorabilityService
    {
        Task<GiftFavorabilityResponse> GiftItemAsync(string playerId, GiftFavorabilityRequest request);
        Task<List<FavorabilityListItemDto>> GetFavorabilityListAsync(string playerId, string direction);
        Task<List<FavorabilityGiftRecordDto>> GetGiftLogAsync(string playerId, FavorabilityLogQueryRequest request);
        Task<List<FavorabilityLevelDto>> GetLevelsAsync();
        Task ResetDailyGiftLimitsAsync();
        Task AwardPartyBattleFavorabilityAsync(IList<string> playerIds);
        Task<List<FavorabilityLeaderboardDto>> GetLeaderboardAsync(int count = 20);
        Task<List<DeepFriendshipDto>> GetDeepFriendshipLeaderboardAsync(int count = 10);
        Task<FavorabilityAchievementProgressDto> GetAchievementProgressAsync(string playerId);
        Task<List<FavorabilityAchievementRewardDto>> ClaimAchievementRewardAsync(string playerId, string achievementId);

        // Admin
        Task<List<AdminFavorabilityRelationDto>> AdminGetRelationsAsync(string? keyword, int pageIndex, int pageSize);
        Task<int> AdminGetRelationsCountAsync(string? keyword);
        Task<bool> AdminUpdateValueAsync(long id, int newValue);
        Task<bool> AdminDeleteRelationAsync(long id);
        Task<List<AdminFavorabilityGiftLogDto>> AdminGetGiftLogAsync(string? playerId, string? targetPlayerId, int pageIndex, int pageSize);
        Task<int> AdminGetGiftLogCountAsync(string? playerId, string? targetPlayerId);
        Task<AdminFavorabilityStatsDto> AdminGetStatsAsync();
        Task<List<FavorabilityLevelDto>> AdminGetLevelsAsync();
        Task<FavorabilityLevelDto?> AdminSaveLevelAsync(FavorabilityLevelDto dto);
        Task<bool> AdminDeleteLevelAsync(int id);
    }
}
