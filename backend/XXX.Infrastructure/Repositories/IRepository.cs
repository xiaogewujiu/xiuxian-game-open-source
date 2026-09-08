using SqlSugar;
using System.Linq.Expressions;

namespace XXX.Infrastructure.Repositories
{
    /// <summary>
    /// 通用仓储接口
    /// 提供基础的CRUD操作
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public interface IRepository<T> where T : class, new()
    {
        /// <summary>
        /// SqlSugar客户端
        /// </summary>
        ISqlSugarClient Db { get; }

        /// <summary>
        /// 根据ID获取实体
        /// </summary>
        /// <param name="id">实体ID</param>
        /// <returns>实体对象</returns>
        Task<T?> GetByIdAsync(object id);

        /// <summary>
        /// 根据条件获取单个实体
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns>实体对象</returns>
        Task<T?> GetFirstAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <returns>实体列表</returns>
        Task<List<T>> GetAllAsync();

        /// <summary>
        /// 根据条件获取实体列表
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns>实体列表</returns>
        Task<List<T>> GetListAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页大小</param>
        /// <returns>分页结果</returns>
        Task<(List<T> Items, int Total)> GetPageListAsync(Expression<Func<T, bool>> predicate, int pageIndex, int pageSize);

        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <returns>影响的行数</returns>
        Task<int> AddAsync(T entity);

        /// <summary>
        /// 批量添加实体
        /// </summary>
        /// <param name="entities">实体列表</param>
        /// <returns>影响的行数</returns>
        Task<int> AddRangeAsync(List<T> entities);

        /// <summary>
        /// 更新实体
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <returns>影响的行数</returns>
        Task<int> UpdateAsync(T entity);

        /// <summary>
        /// 批量更新实体
        /// </summary>
        /// <param name="entities">实体列表</param>
        /// <returns>影响的行数</returns>
        Task<int> UpdateRangeAsync(List<T> entities);

        /// <summary>
        /// 根据ID删除实体
        /// </summary>
        /// <param name="id">实体ID</param>
        /// <returns>影响的行数</returns>
        Task<int> DeleteAsync(object id);

        /// <summary>
        /// 删除实体
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <returns>影响的行数</returns>
        Task<int> DeleteAsync(T entity);

        /// <summary>
        /// 根据条件删除实体
        /// </summary>
        /// <param name="predicate">删除条件</param>
        /// <returns>影响的行数</returns>
        Task<int> DeleteAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// 判断是否存在
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns>是否存在</returns>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// 获取记录数
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns>记录数</returns>
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);
    }
}
