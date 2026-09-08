using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 管理后台灵田系统服务接口。
    /// </summary>
    public interface IAdminSpiritFieldService
    {
        /// <summary>
        /// 获取灵田系统列表。
        /// </summary>
        Task<List<AdminSpiritFieldListItemDto>> GetListAsync(string? keyword = null);

        /// <summary>
        /// 获取灵田系统详情。
        /// </summary>
        Task<AdminSpiritFieldDetailDto?> GetDetailAsync(string playerId);

        /// <summary>
        /// 保存灵田系统数据。
        /// </summary>
        Task<AdminSpiritFieldDetailDto> SaveAsync(AdminSpiritFieldDetailDto request);
    }
}
