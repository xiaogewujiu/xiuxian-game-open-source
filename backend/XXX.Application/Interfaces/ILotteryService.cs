using XXX.Application.DTOs;
using XXX.Entity;

namespace XXX.Application.Interfaces
{
    public interface ILotteryService
    {
        Task<List<LotteryPoolViewDto>> GetLotteryPoolsAsync(string playerId);
        Task<LotteryDrawResultDto> DrawAsync(UserEntity player, LotteryDrawRequestDto request);
    }
}
