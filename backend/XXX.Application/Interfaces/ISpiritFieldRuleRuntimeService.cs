namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 灵田规则运行时缓存服务。
    /// </summary>
    public interface ISpiritFieldRuleRuntimeService
    {
        /// <summary>
        /// 从数据库加载灵田总规则和加速道具规则到运行时缓存。
        /// </summary>
        Task LoadAsync();
    }
}
