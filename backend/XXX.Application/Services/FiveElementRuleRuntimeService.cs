using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Data;

namespace XXX.Application.Services
{
    /// <summary>
    /// 聚灵阵规则运行时缓存加载器。
    /// </summary>
    public class FiveElementRuleRuntimeService : IFiveElementRuleRuntimeService
    {
        /// <summary>
        /// 数据库上下文。
        /// </summary>
        private readonly DbContext _dbContext;

        /// <summary>
        /// 初始化聚灵阵规则运行时缓存加载器。
        /// </summary>
        /// <param name="dbContext">数据库上下文。</param>
        public FiveElementRuleRuntimeService(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 从数据库重新加载聚灵阵主等级规则与五行分支规则。
        /// 当前要求主规则 50 条、分支规则 250 条。
        /// </summary>
        public async Task LoadAsync()
        {
            var levelConfigs = await _dbContext.Db.Queryable<FiveElementLevelConfigEntity>()
                .Where(item => item.IsEnabled)
                .OrderBy(item => item.ArrayLevel)
                .ToListAsync();
            var branchRangeConfigs = await _dbContext.Db.Queryable<FiveElementBranchRuleRangeEntity>()
                .Where(item => item.IsEnabled)
                .OrderBy(item => item.ElementType)
                .ToListAsync();

            if (levelConfigs.Count != 50)
            {
                throw new InvalidOperationException(
                    $"聚灵阵主等级规则不完整。期望 50 条，实际只有 {levelConfigs.Count} 条。请先执行启动种子同步后再加载聚灵阵规则。");
            }

            if (branchRangeConfigs.Count == 0)
            {
                throw new InvalidOperationException(
                    "聚灵阵五行区间规则不存在。请先执行启动种子同步后再加载聚灵阵规则。");
            }

            FiveElementProgressionRules.ValidateBranchRangeConfigs(branchRangeConfigs);
            FiveElementProgressionRules.ReplaceConfigs(levelConfigs, [], branchRangeConfigs);
        }
    }
}
