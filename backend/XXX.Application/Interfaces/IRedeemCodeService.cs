using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 兑换码服务接口。
    /// </summary>
    public interface IRedeemCodeService
    {
        /// <summary>
        /// 兑换一个奖励码。
        /// </summary>
        Task<RedeemCodeResultDto> RedeemAsync(string playerId, RedeemCodeRequestDto request);
    }
}
