using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    public interface IAdminTowerService
    {
        Task<AdminTowerOverviewDto> GetOverviewAsync();
        Task<List<AdminTowerPlayerDto>> GetPlayersAsync(string? keyword = null, int take = 200);
        Task<List<AdminTowerFloorConfigDto>> GetFloorConfigsAsync();
        Task<bool> UpdateFloorConfigAsync(long id, AdminTowerFloorConfigDto dto);
        Task<int> BatchGenerateFloorsAsync(AdminTowerBatchGenerateDto dto);
        Task<List<TowerFloorDistributionDto>> GetFloorDistributionAsync();
    }
}
