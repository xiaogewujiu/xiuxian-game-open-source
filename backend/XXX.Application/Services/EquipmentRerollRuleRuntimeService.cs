using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Data;

namespace XXX.Application.Services
{
    /// <summary>
    /// 装备洗练规则运行时服务实现。
    /// </summary>
    internal class EquipmentRerollRuleRuntimeService : IEquipmentRerollRuleRuntimeService
    {
        private readonly DbContext _dbContext;

        public EquipmentRerollRuleRuntimeService(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task LoadAsync()
        {
            var systemConfig = await _dbContext.Db.Queryable<EquipmentRerollSystemConfigEntity>()
                .FirstAsync(item => item.ConfigId == "default" && item.IsEnabled);

            var enhanceRules = await _dbContext.Db.Queryable<EquipmentEnhanceRuleEntity>()
                .Where(item => item.IsEnabled)
                .ToListAsync();

            var rerollCostRules = await _dbContext.Db.Queryable<EquipmentRerollCostRuleEntity>()
                .Where(item => item.IsEnabled)
                .ToListAsync();

            var slotPoolConfigs = await _dbContext.Db.Queryable<EquipmentRerollSlotPoolConfigEntity>()
                .Where(item => item.IsEnabled)
                .ToListAsync();

            var tierConfigs = await _dbContext.Db.Queryable<EquipmentRerollTierConfigEntity>()
                .Where(item => item.IsEnabled)
                .ToListAsync();

            var attributeValueConfigs = await _dbContext.Db.Queryable<EquipmentRerollAttributeValueConfigEntity>()
                .Where(item => item.IsEnabled)
                .ToListAsync();

            EquipmentRerollRuleRuntimeCatalog.ReplaceConfigs(
                systemConfig,
                enhanceRules,
                rerollCostRules,
                slotPoolConfigs,
                tierConfigs,
                attributeValueConfigs);
        }
    }
}
