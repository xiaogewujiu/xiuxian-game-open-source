using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Data;

namespace XXX.Application.Services
{
    /// <summary>
    /// 锻造职业规则运行时缓存加载器。
    /// </summary>
    public class ForgeProfessionRuleRuntimeService : IForgeProfessionRuleRuntimeService
    {
        /// <summary>
        /// 数据库上下文。
        /// </summary>
        private readonly DbContext _dbContext;

        /// <summary>
        /// 初始化锻造职业规则运行时缓存加载器。
        /// </summary>
        /// <param name="dbContext">数据库上下文。</param>
        public ForgeProfessionRuleRuntimeService(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 从数据库重新加载锻造职业等级规则与通用规则。
        /// </summary>
        public async Task LoadAsync()
        {
            var levelConfigs = await _dbContext.Db.Queryable<ForgeProfessionLevelConfigEntity>()
                .Where(item => item.IsEnabled)
                .OrderBy(item => item.Level)
                .ToListAsync();
            var ruleConfig = await _dbContext.Db.Queryable<ForgeProfessionRuleConfigEntity>()
                .Where(item => item.IsEnabled)
                .OrderByDescending(item => item.LastUpdateTime)
                .FirstAsync();

            if (levelConfigs.Count != 50)
            {
                throw new InvalidOperationException(
                    $"锻造职业等级规则不完整。期望 50 条，实际只有 {levelConfigs.Count} 条。请先执行启动种子同步后再加载锻造规则。");
            }

            if (ruleConfig == null)
            {
                throw new InvalidOperationException(
                    "锻造职业通用规则缺失。请先执行启动种子同步后再加载锻造规则。");
            }

            ForgeProfessionRuleRuntimeCatalog.ReplaceConfigs(levelConfigs, ruleConfig);
        }
    }
}
