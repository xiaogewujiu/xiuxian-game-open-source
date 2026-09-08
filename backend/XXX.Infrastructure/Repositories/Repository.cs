using SqlSugar;
using System.Linq.Expressions;
using XXX.Infrastructure.Data;

namespace XXX.Infrastructure.Repositories
{
    /// <summary>
    /// 通用仓储实现
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public class Repository<T> : IRepository<T> where T : class, new()
    {
        private readonly DbContext _dbContext;

        /// <summary>
        /// SqlSugar客户端
        /// </summary>
        public ISqlSugarClient Db => _dbContext.Db;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dbContext">数据库上下文</param>
        public Repository(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 根据ID获取实体
        /// </summary>
        public virtual async Task<T?> GetByIdAsync(object id)
        {
            return await Db.Queryable<T>().InSingleAsync(id);
        }

        /// <summary>
        /// 根据条件获取单个实体
        /// </summary>
        public virtual async Task<T?> GetFirstAsync(Expression<Func<T, bool>> predicate)
        {
            return await Db.Queryable<T>().FirstAsync(predicate);
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        public virtual async Task<List<T>> GetAllAsync()
        {
            return await Db.Queryable<T>().ToListAsync();
        }

        /// <summary>
        /// 根据条件获取实体列表
        /// </summary>
        public virtual async Task<List<T>> GetListAsync(Expression<Func<T, bool>> predicate)
        {
            return await Db.Queryable<T>().Where(predicate).ToListAsync();
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        public virtual async Task<(List<T> Items, int Total)> GetPageListAsync(
            Expression<Func<T, bool>> predicate, int pageIndex, int pageSize)
        {
            RefAsync<int> total = 0;
            var items = await Db.Queryable<T>()
                .Where(predicate)
                .ToPageListAsync(pageIndex, pageSize, total);

            return (items, total.Value);
        }

        /// <summary>
        /// 添加实体
        /// </summary>
        public virtual async Task<int> AddAsync(T entity)
        {
            return await Db.Insertable(entity).ExecuteCommandAsync();
        }

        /// <summary>
        /// 批量添加实体
        /// </summary>
        public virtual async Task<int> AddRangeAsync(List<T> entities)
        {
            return await Db.Insertable(entities).ExecuteCommandAsync();
        }

        /// <summary>
        /// 更新实体
        /// </summary>
        public virtual async Task<int> UpdateAsync(T entity)
        {
            return await Db.Updateable(entity).ExecuteCommandAsync();
        }

        /// <summary>
        /// 批量更新实体
        /// </summary>
        public virtual async Task<int> UpdateRangeAsync(List<T> entities)
        {
            return await Db.Updateable(entities).ExecuteCommandAsync();
        }

        /// <summary>
        /// 根据ID删除实体
        /// </summary>
        public virtual async Task<int> DeleteAsync(object id)
        {
            return await Db.Deleteable<T>().In(id).ExecuteCommandAsync();
        }

        /// <summary>
        /// 删除实体
        /// </summary>
        public virtual async Task<int> DeleteAsync(T entity)
        {
            return await Db.Deleteable(entity).ExecuteCommandAsync();
        }

        /// <summary>
        /// 根据条件删除实体
        /// </summary>
        public virtual async Task<int> DeleteAsync(Expression<Func<T, bool>> predicate)
        {
            return await Db.Deleteable<T>().Where(predicate).ExecuteCommandAsync();
        }

        /// <summary>
        /// 判断是否存在
        /// </summary>
        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await Db.Queryable<T>().AnyAsync(predicate);
        }

        /// <summary>
        /// 获取记录数
        /// </summary>
        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await Db.Queryable<T>().CountAsync(predicate);
        }
    }
}
