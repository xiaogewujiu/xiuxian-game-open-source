namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 元素克制矩阵运行时缓存服务。
    /// </summary>
    public interface IElementRelationRuleRuntimeService
    {
        /// <summary>
        /// 从数据库加载元素克制矩阵到运行时缓存。
        /// </summary>
        Task LoadAsync();
    }
}
