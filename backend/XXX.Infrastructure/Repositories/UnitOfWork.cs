using SqlSugar;
using XXX.Infrastructure.Data;

namespace XXX.Infrastructure.Repositories
{
    /// <summary>
    /// 工作单元实现
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DbContext _dbContext;
        private readonly Dictionary<Type, object> _repositories;
        private bool _disposed;

        /// <summary>
        /// SqlSugar客户端
        /// </summary>
        public ISqlSugarClient Db => _dbContext.Db;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dbContext">数据库上下文</param>
        public UnitOfWork(DbContext dbContext)
        {
            _dbContext = dbContext;
            _repositories = [];
        }

        /// <summary>
        /// 获取仓储实例
        /// </summary>
        public IRepository<T> GetRepository<T>() where T : class, new()
        {
            var type = typeof(T);
            if (!_repositories.ContainsKey(type))
            {
                _repositories[type] = new Repository<T>(_dbContext);
            }
            return (IRepository<T>)_repositories[type];
        }

        /// <summary>
        /// 开始事务
        /// </summary>
        public void BeginTransaction()
        {
            _dbContext.BeginTransaction();
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        public void Commit()
        {
            _dbContext.CommitTransaction();
        }

        /// <summary>
        /// 回滚事务
        /// </summary>
        public void Rollback()
        {
            _dbContext.RollbackTransaction();
        }

        /// <summary>
        /// 异步提交事务
        /// </summary>
        public async Task CommitAsync()
        {
            await Task.Run(() => Commit());
        }

        /// <summary>
        /// 异步回滚事务
        /// </summary>
        public async Task RollbackAsync()
        {
            await Task.Run(() => Rollback());
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _repositories.Clear();
                }
                _disposed = true;
            }
        }
    }
}
