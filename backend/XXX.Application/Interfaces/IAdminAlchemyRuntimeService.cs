using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 管理后台炼丹运行态服务接口。
    /// </summary>
    public interface IAdminAlchemyRuntimeService
    {
        /// <summary>
        /// 获取炼丹系统列表。
        /// </summary>
        Task<List<AdminAlchemySystemListItemDto>> GetListAsync(string? keyword = null);

        /// <summary>
        /// 获取炼丹系统详情。
        /// </summary>
        Task<AdminAlchemySystemDetailDto?> GetDetailAsync(string playerId);

        /// <summary>
        /// 保存炼丹系统数据。
        /// </summary>
        Task<AdminAlchemySystemDetailDto> SaveAsync(AdminAlchemySystemDetailDto request);

        /// <summary>
        /// 清空当前炼丹任务。
        /// </summary>
        Task<AdminAlchemySystemDetailDto> ClearActiveTaskAsync(string playerId);
    }
}
