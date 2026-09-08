using XXX.Application.Interfaces;
using XXX.Battle;
using XXX.Entity;
using XXX.Infrastructure.Data;

namespace XXX.Application.Services
{
    /// <summary>
    /// 元素克制矩阵运行时缓存加载器。
    /// </summary>
    public class ElementRelationRuleRuntimeService : IElementRelationRuleRuntimeService
    {
        /// <summary>
        /// 数据库上下文。
        /// </summary>
        private readonly DbContext _dbContext;

        /// <summary>
        /// 初始化元素克制矩阵运行时缓存加载器。
        /// </summary>
        /// <param name="dbContext">数据库上下文。</param>
        public ElementRelationRuleRuntimeService(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 从数据库重新加载元素克制矩阵。
        /// 当前要求 8 x 8 共 64 组攻防元素组合全部存在（无属性不参与克制链）。
        /// </summary>
        public async Task LoadAsync()
        {
            var entries = await _dbContext.Db.Queryable<ElementRelationRuleEntity>()
                .Where(item => item.IsEnabled)
                .OrderBy(item => item.SortOrder)
                .ToListAsync();

            var pairCount = entries
                .Where(item => item.AttackerElement != 0 && item.DefenderElement != 0)
                .Select(item => $"{item.AttackerElement}:{item.DefenderElement}")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();
            if (pairCount != 64)
            {
                throw new InvalidOperationException(
                    $"元素克制规则不完整。期望 64 组唯一的攻防元素组合（不含无属性），实际只有 {pairCount} 组。请先执行启动种子同步后再加载战斗规则。");
            }

            ElementRelation.LoadFromDatabase(entries);
        }
    }
}
