using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 管理后台首页服务接口。
    /// </summary>
    public interface IAdminDashboardService
    {
        /// <summary>
        /// 获取首页摘要数据。
        /// </summary>
        Task<AdminDashboardSummaryDto> GetSummaryAsync();
    }
}
