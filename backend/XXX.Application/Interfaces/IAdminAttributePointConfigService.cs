using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 属性点配置后台服务。
    /// </summary>
    public interface IAdminAttributePointConfigService
    {
        /// <summary>
        /// 获取当前属性点配置总览。
        /// </summary>
        /// <returns>属性点配置总览。</returns>
        Task<AdminAttributePointConfigBundleDto> GetConfigAsync();

        /// <summary>
        /// 保存当前属性点配置总览。
        /// </summary>
        /// <param name="request">属性点配置请求。</param>
        /// <returns>保存后的属性点配置总览。</returns>
        Task<AdminAttributePointConfigBundleDto> SaveConfigAsync(AdminAttributePointConfigBundleDto request);
    }
}
