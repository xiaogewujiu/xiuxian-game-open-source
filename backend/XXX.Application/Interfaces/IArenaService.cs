using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 竞技场服务接口。
    /// </summary>
    public interface IArenaService
    {
        /// <summary>
        /// 获取我的竞技场信息。
        /// </summary>
        Task<ArenaMeDto?> GetMyInfoAsync(string playerId);

        /// <summary>
        /// 获取推荐对手列表。
        /// </summary>
        Task<List<ArenaOpponentDto>> GetOpponentsAsync(string playerId);

        /// <summary>
        /// 发起挑战。
        /// </summary>
        Task<ArenaChallengeResultDto> ChallengeAsync(string attackerId, string defenderId);

        /// <summary>
        /// 获取对战历史。
        /// </summary>
        Task<List<ArenaBattleLogDto>> GetHistoryAsync(string playerId, int page = 1, int pageSize = 20);

        /// <summary>
        /// 购买挑战次数。
        /// </summary>
        Task<int> BuyAttemptsAsync(string playerId);

        /// <summary>
        /// 获取赛季信息。
        /// </summary>
        Task<ArenaSeasonDto> GetSeasonInfoAsync(string playerId);
    }
}
