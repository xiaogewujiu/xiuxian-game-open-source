using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 签到服务接口。
    /// </summary>
    public interface ICheckInService
    {
        /// <summary>
        /// 获取指定月份的签到状态。
        /// </summary>
        Task<CheckInStatusDto> GetStatusAsync(string playerId, int? year = null, int? month = null);

        /// <summary>
        /// 领取今日签到奖励。
        /// </summary>
        Task<ClaimCheckInResultDto> ClaimAsync(string playerId);
    }
}
