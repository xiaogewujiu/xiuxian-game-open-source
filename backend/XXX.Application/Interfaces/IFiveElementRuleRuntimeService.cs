namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 聚灵阵规则运行时缓存服务。
    /// </summary>
    public interface IFiveElementRuleRuntimeService
    {
        /// <summary>
        /// 从数据库加载聚灵阵规则到运行时缓存。
        /// </summary>
        Task LoadAsync();
    }
}
