using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 临时队伍服务接口。
    /// </summary>
    public interface ITeamService
    {
        /// <summary>
        /// 获取当前可加入的队伍列表。
        /// </summary>
        Task<List<PartySummaryDto>> GetAvailablePartiesAsync(string playerId);

        /// <summary>
        /// 获取玩家当前队伍详情。
        /// </summary>
        Task<PartyDetailDto?> GetCurrentPartyAsync(string playerId);

        /// <summary>
        /// 创建一个新的临时队伍。
        /// </summary>
        Task<PartyDetailDto> CreatePartyAsync(string playerId, CreatePartyRequestDto request);

        /// <summary>
        /// 加入指定队伍。
        /// </summary>
        Task<PartyDetailDto> JoinPartyAsync(string playerId, string partyId);

        /// <summary>
        /// 离开当前所在队伍。
        /// </summary>
        Task<PartyDetailDto?> LeaveCurrentPartyAsync(string playerId);

        /// <summary>
        /// 切换队伍招募状态。
        /// </summary>
        Task<PartyDetailDto> ToggleRecruitingAsync(string playerId, string partyId);

        /// <summary>
        /// 解散指定队伍。
        /// </summary>
        Task<bool> DismissPartyAsync(string playerId, string partyId);
    }
}
