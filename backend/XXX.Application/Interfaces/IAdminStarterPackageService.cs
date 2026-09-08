using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 后台新手礼包配置服务。
    /// </summary>
    public interface IAdminStarterPackageService
    {
        /// <summary>
        /// 获取新手礼包列表。
        /// </summary>
        /// <param name="keyword">可选关键字，支持按礼包编号或名称筛选。</param>
        /// <returns>新手礼包列表。</returns>
        Task<List<AdminStarterPackageListItemDto>> GetListAsync(string? keyword = null, bool? autoGrantOnRegister = null);

        /// <summary>
        /// 获取指定新手礼包详情。
        /// </summary>
        /// <param name="packageId">礼包编号。</param>
        /// <returns>礼包详情；不存在时返回空。</returns>
        Task<AdminStarterPackageDetailDto?> GetDetailAsync(string packageId);

        /// <summary>
        /// 保存新手礼包配置。
        /// </summary>
        /// <param name="request">礼包详情请求。</param>
        /// <param name="operatorName">可选操作人名称。</param>
        /// <returns>保存后的礼包详情。</returns>
        Task<AdminStarterPackageDetailDto> SaveAsync(AdminStarterPackageDetailDto request, string? operatorName = null);

        /// <summary>
        /// 删除指定新手礼包。
        /// </summary>
        /// <param name="packageId">礼包编号。</param>
        /// <returns>删除成功返回真。</returns>
        Task<bool> DeleteAsync(string packageId);
    }
}
