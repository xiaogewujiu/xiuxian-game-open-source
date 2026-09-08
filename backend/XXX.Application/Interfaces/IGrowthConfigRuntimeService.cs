namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 成长域运行时配置缓存服务。
    /// </summary>
    public interface IGrowthConfigRuntimeService
    {
        /// <summary>
        /// 从数据库加载等级成长、境界成长、初始资源和属性点配置到运行时缓存。
        /// </summary>
        Task LoadAsync();
    }
}
