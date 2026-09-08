using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Data;

namespace XXX.Application.Services
{
    /// <summary>
    /// 炼丹职业规则运行时缓存加载器。
    /// </summary>
    public class AlchemyProfessionRuleRuntimeService : IAlchemyProfessionRuleRuntimeService
    {
        /// <summary>
        /// 数据库上下文。
        /// </summary>
        private readonly DbContext _dbContext;

        /// <summary>
        /// 初始化炼丹职业规则运行时缓存加载器。
        /// </summary>
        /// <param name="dbContext">数据库上下文。</param>
        public AlchemyProfessionRuleRuntimeService(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 从数据库重新加载炼丹职业等级规则与通用规则。
        /// </summary>
        public async Task LoadAsync()
        {
            var levelConfigs = await _dbContext.Db.Queryable<AlchemyProfessionLevelConfigEntity>()
                .Where(item => item.IsEnabled)
                .OrderBy(item => item.Level)
                .ToListAsync();
            var ruleConfig = await _dbContext.Db.Queryable<AlchemyProfessionRuleConfigEntity>()
                .Where(item => item.IsEnabled)
                .OrderByDescending(item => item.LastUpdateTime)
                .FirstAsync();

            if (levelConfigs.Count != 50)
            {
                throw new InvalidOperationException(
                    $"炼丹职业等级规则不完整。期望 50 条，实际只有 {levelConfigs.Count} 条。请先执行启动种子同步后再加载炼丹规则。");
            }

            if (ruleConfig == null)
            {
                throw new InvalidOperationException(
                    "炼丹职业通用规则缺失。请先执行启动种子同步后再加载炼丹规则。");
            }

            AlchemyProfessionRuleRuntimeCatalog.ReplaceConfigs(levelConfigs, ruleConfig);
        }
    }
}
