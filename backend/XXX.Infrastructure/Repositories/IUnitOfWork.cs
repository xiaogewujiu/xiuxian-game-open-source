using SqlSugar;

namespace XXX.Infrastructure.Repositories
{
    /// <summary>
    /// 工作单元接口
    /// 管理事务和多个仓储
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// SqlSugar客户端
        /// </summary>
        ISqlSugarClient Db { get; }

        /// <summary>
        /// 获取仓储实例
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <returns>仓储实例</returns>
        IRepository<T> GetRepository<T>() where T : class, new();

        /// <summary>
        /// 开始事务
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// 提交事务
        /// </summary>
        void Commit();

        /// <summary>
        /// 回滚事务
        /// </summary>
        void Rollback();

        /// <summary>
        /// 异步提交事务
        /// </summary>
        Task CommitAsync();

        /// <summary>
        /// 异步回滚事务
        /// </summary>
        Task RollbackAsync();
    }
}
