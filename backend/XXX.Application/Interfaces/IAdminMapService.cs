using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 管理后台地图服务接口。
    /// </summary>
    public interface IAdminMapService
    {
        /// <summary>
        /// 获取地图列表。
        /// </summary>
        Task<List<AdminMapListItemDto>> GetListAsync(string? keyword = null, string? mapKind = null);

        /// <summary>
        /// 获取地图详情。
        /// </summary>
        Task<AdminMapDetailDto?> GetDetailAsync(string mapId);

        /// <summary>
        /// 保存地图。
        /// </summary>
        Task<AdminMapDetailDto> SaveAsync(AdminSaveMapRequestDto request);

        /// <summary>
        /// 删除地图。
        /// </summary>
        Task<bool> DeleteAsync(string mapId);
    }
}
