using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    public interface IAdminLotteryService
    {
        // 抽奖池
        Task<List<AdminLotteryPoolListItemDto>> GetPoolListAsync(string? keyword);
        Task<AdminLotteryPoolDetailDto?> GetPoolDetailAsync(string poolId);
        Task<AdminLotteryPoolDetailDto> SavePoolAsync(AdminLotteryPoolDetailDto dto);
        Task<bool> DeletePoolAsync(string poolId);

        // 抽奖奖项
        Task<List<AdminLotteryPrizeListItemDto>> GetPrizeListAsync(string? poolId);
        Task<AdminLotteryPrizeDetailDto?> GetPrizeDetailAsync(string prizeId);
        Task<AdminLotteryPrizeDetailDto> SavePrizeAsync(AdminLotteryPrizeDetailDto dto);
        Task<bool> DeletePrizeAsync(string prizeId);

        // 抽奖日志
        Task<(List<AdminLotteryLogListItemDto> Items, int Total)> GetLogsAsync(string? playerId, string? poolId, int page, int pageSize);
    }
}
