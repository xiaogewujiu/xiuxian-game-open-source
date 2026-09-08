using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 管理后台锻造运行态服务接口。
    /// </summary>
    public interface IAdminForgeRuntimeService
    {
        /// <summary>
        /// 获取锻造系统列表。
        /// </summary>
        Task<List<AdminForgeSystemListItemDto>> GetListAsync(string? keyword = null);

        /// <summary>
        /// 获取锻造系统详情。
        /// </summary>
        Task<AdminForgeSystemDetailDto?> GetDetailAsync(string playerId);

        /// <summary>
        /// 保存锻造系统数据。
        /// </summary>
        Task<AdminForgeSystemDetailDto> SaveAsync(AdminForgeSystemDetailDto request);

        /// <summary>
        /// 清空当前锻造任务。
        /// </summary>
        Task<AdminForgeSystemDetailDto> ClearActiveTaskAsync(string playerId);
    }
}
