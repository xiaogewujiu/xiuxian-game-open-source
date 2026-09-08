using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Data;
using XXX.Player;

namespace XXX.Application.Services
{
    /// <summary>
    /// 成长域运行时缓存加载器。
    /// </summary>
    public class GrowthConfigRuntimeService : IGrowthConfigRuntimeService
    {
        /// <summary>
        /// 数据库上下文。
        /// </summary>
        private readonly DbContext _dbContext;

        /// <summary>
        /// 初始化成长域运行时缓存加载器。
        /// </summary>
        /// <param name="dbContext">数据库上下文。</param>
        public GrowthConfigRuntimeService(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 从数据库重新加载成长域运行时缓存。
        /// 包括初始资源、等级成长、境界成长和属性点配置。
        /// </summary>
        public async Task LoadAsync()
        {
            var initialResourceConfig = await _dbContext.Db.Queryable<PlayerInitialResourceConfigEntity>()
                .InSingleAsync("default");
            var playerLevelConfigs = await _dbContext.Db.Queryable<PlayerLevelConfigEntity>()
                .OrderBy(item => item.Level)
                .ToListAsync();
            var realmLevelConfigs = await _dbContext.Db.Queryable<RealmLevelConfigEntity>()
                .OrderBy(item => item.Level)
                .ToListAsync();
            var attributePointConfigs = await _dbContext.Db.Queryable<AttributePointConfigEntity>()
                .Where(item => item.IsEnabled)
                .OrderBy(item => item.SortOrder)
                .OrderBy(item => item.LevelStart)
                .OrderBy(item => item.Key)
                .ToListAsync();

            if (playerLevelConfigs.Count != LevelConfig.MaxLevel)
            {
                LevelConfig.ClearRuntimeEntries();
                throw new InvalidOperationException(
                    $"等级成长配置不完整。期望 {LevelConfig.MaxLevel} 条，实际只有 {playerLevelConfigs.Count} 条。请先执行启动种子同步后再加载成长配置。");
            }

            if (realmLevelConfigs.Count != LevelConfig.MaxLevel)
            {
                RealmLevelCatalog.ClearRuntimeEntries();
                throw new InvalidOperationException(
                    $"境界成长配置不完整。期望 {LevelConfig.MaxLevel} 条，实际只有 {realmLevelConfigs.Count} 条。请先执行启动种子同步后再加载成长配置。");
            }

            if (initialResourceConfig == null)
            {
                PlayerInitialResourceConfig.ClearRuntimeConfig();
                throw new InvalidOperationException("玩家初始资源配置缺失。请先执行启动种子同步后再加载成长配置。");
            }

            if (attributePointConfigs.Count == 0)
            {
                AttributePointConfig.ClearRuntimeEntries();
                throw new InvalidOperationException("属性点配置缺失。请先执行启动种子同步后再加载成长配置。");
            }

            LevelConfig.ReplaceEntries(playerLevelConfigs);
            RealmLevelCatalog.ReplaceEntries(realmLevelConfigs);
            PlayerInitialResourceConfig.ReplaceConfig(initialResourceConfig);
            AttributePointConfig.ReplaceEntries(attributePointConfigs);
        }
    }
}
