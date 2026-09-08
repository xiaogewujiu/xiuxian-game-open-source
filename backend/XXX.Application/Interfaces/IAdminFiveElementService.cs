using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 管理后台聚灵阵服务接口。
    /// </summary>
    public interface IAdminFiveElementService
    {
        /// <summary>
        /// 获取聚灵阵列表。
        /// </summary>
        Task<List<AdminFiveElementListItemDto>> GetListAsync(string? keyword = null);

        /// <summary>
        /// 获取聚灵阵详情。
        /// </summary>
        Task<AdminFiveElementDetailDto?> GetDetailAsync(string playerId);

        /// <summary>
        /// 保存聚灵阵数据。
        /// </summary>
        Task<AdminFiveElementDetailDto> SaveAsync(AdminFiveElementDetailDto request);
    }
}
