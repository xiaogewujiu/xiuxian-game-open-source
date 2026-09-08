using XXX.Entity;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 新手礼包运行时服务。
    /// </summary>
    public interface IStarterPackageService
    {
        /// <summary>
        /// 重载新手礼包缓存。
        /// </summary>
        Task ReloadCacheAsync();

        /// <summary>
        /// 向新注册角色发放当前启用的新手礼包。
        /// </summary>
        Task ApplyOnRegisterAsync(UserEntity user);
    }
}
