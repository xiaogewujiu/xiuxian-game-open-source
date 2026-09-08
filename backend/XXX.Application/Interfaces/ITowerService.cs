using XXX.Application.DTOs;
using XXX.Ranking;

namespace XXX.Application.Interfaces
{
    public interface ITowerService
    {
        Task<TowerMeDto?> GetMyInfoAsync(string playerId);
        Task<List<TowerFloorSummaryDto>> GetFloorSummariesAsync(string playerId);
        Task<TowerChallengeResultDto> ChallengeAsync(string playerId);
        Task<List<TowerBattleLogDto>> GetHistoryAsync(string playerId, int take = 20);
        Task<int> BuyAttemptsAsync(string playerId);
        Task<List<RankingEntry>> GetLeaderboardAsync(int count = 50);
    }
}
