namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 装备洗练规则运行时服务。
    /// </summary>
    public interface IEquipmentRerollRuleRuntimeService
    {
        /// <summary>
        /// 从数据库加载洗练规则到运行时缓存。
        /// </summary>
        Task LoadAsync();
    }
}
