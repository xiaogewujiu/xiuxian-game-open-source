using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 管理后台审计日志服务接口。
    /// </summary>
    public interface IAdminAuditService
    {
        /// <summary>
        /// 获取审计日志列表。
        /// </summary>
        Task<List<AdminAuditLogListItemDto>> GetListAsync(string? keyword = null, int take = 200, bool? success = null);

        /// <summary>
        /// 获取单条审计日志详情。
        /// </summary>
        Task<AdminAuditLogDetailDto?> GetDetailAsync(string logId);
    }
}
