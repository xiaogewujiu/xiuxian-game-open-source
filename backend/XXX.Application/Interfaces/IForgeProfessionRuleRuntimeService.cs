namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 锻造职业规则运行时缓存服务。
    /// </summary>
    public interface IForgeProfessionRuleRuntimeService
    {
        /// <summary>
        /// 从数据库加载锻造职业等级规则和通用规则到运行时缓存。
        /// </summary>
        Task LoadAsync();
    }
}
