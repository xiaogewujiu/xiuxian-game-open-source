using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Data;

namespace XXX.Application.Services
{
    /// <summary>
    /// 灵田规则运行时缓存加载器。
    /// </summary>
    public class SpiritFieldRuleRuntimeService : ISpiritFieldRuleRuntimeService
    {
        /// <summary>
        /// 数据库上下文。
        /// </summary>
        private readonly DbContext _dbContext;

        /// <summary>
        /// 初始化灵田规则运行时缓存加载器。
        /// </summary>
        /// <param name="dbContext">数据库上下文。</param>
        public SpiritFieldRuleRuntimeService(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 从数据库重新加载灵田系统总规则与加速道具规则。
        /// </summary>
        public async Task LoadAsync()
        {
            var systemConfig = await _dbContext.Db.Queryable<SpiritFieldSystemConfigEntity>()
                .Where(item => item.IsEnabled)
                .OrderByDescending(item => item.LastUpdateTime)
                .FirstAsync();
            var speedUpItems = await _dbContext.Db.Queryable<SpiritFieldSpeedUpItemConfigEntity>()
                .Where(item => item.IsEnabled)
                .OrderBy(item => item.SortOrder)
                .ToListAsync();

            if (systemConfig == null)
            {
                throw new InvalidOperationException(
                    "灵田系统总规则缺失。请先执行启动种子同步后再加载灵田规则。");
            }

            SpiritFieldRuleRuntimeCatalog.ReplaceConfigs(systemConfig, speedUpItems);
        }
    }
}
