using SqlSugar;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Data;

namespace XXX.Application.Services
{
    /// <summary>
    /// 装备洗练规则管理服务实现。
    /// </summary>
    public class AdminEquipmentRerollRuleService : IAdminEquipmentRerollRuleService
    {
        private readonly DbContext _dbContext;
        private readonly IAdminRuntimeRefreshService _runtimeRefreshService;

        public AdminEquipmentRerollRuleService(DbContext dbContext, IAdminRuntimeRefreshService runtimeRefreshService)
        {
            _dbContext = dbContext;
            _runtimeRefreshService = runtimeRefreshService;
        }

        public async Task<List<AdminEquipmentEnhanceRuleDto>> GetEnhanceRulesAsync()
        {
            var entities = await _dbContext.Db.Queryable<EquipmentEnhanceRuleEntity>()
                .OrderBy(item => item.MinEquipmentLevel, OrderByType.Asc)
                .OrderBy(item => item.SortOrder, OrderByType.Asc)
                .ToListAsync();
            return entities.Select(MapToDto).ToList();
        }

        public async Task SaveEnhanceRuleAsync(AdminEquipmentEnhanceRuleDto dto)
        {
            ValidateLevelRange(dto.MinEquipmentLevel, dto.MaxEquipmentLevel);
            ValidatePositive(dto.MaterialCount, "强化材料数量");
            ValidateNonNegative(dto.GoldCost, "强化金币消耗");
            ValidatePercent(dto.SuccessRate, "强化成功率");
            ValidatePercent(dto.AttributeGrowthPercent, "强化属性成长");
            ValidatePositive(dto.MaxEnhanceLevel, "强化上限");
            await ValidateMaterialAsync(dto.MaterialItemId);
            await EnsureNoEnhanceOverlapAsync(dto);

            var now = DateTime.Now;
            if (dto.GID == 0)
            {
                await _dbContext.Db.Insertable(new EquipmentEnhanceRuleEntity
                {
                    MinEquipmentLevel = dto.MinEquipmentLevel,
                    MaxEquipmentLevel = dto.MaxEquipmentLevel,
                    MaterialItemId = dto.MaterialItemId,
                    MaterialCount = dto.MaterialCount,
                    GoldCost = dto.GoldCost,
                    SuccessRate = dto.SuccessRate,
                    AttributeGrowthPercent = dto.AttributeGrowthPercent,
                    MaxEnhanceLevel = dto.MaxEnhanceLevel,
                    SortOrder = dto.SortOrder,
                    IsEnabled = dto.IsEnabled,
                    LastUpdateTime = now
                }).ExecuteCommandAsync();
            }
            else
            {
                var existing = await _dbContext.Db.Queryable<EquipmentEnhanceRuleEntity>()
                    .FirstAsync(item => item.GID == dto.GID);
                if (existing == null) throw new InvalidOperationException("强化规则不存在。");
                existing.MinEquipmentLevel = dto.MinEquipmentLevel;
                existing.MaxEquipmentLevel = dto.MaxEquipmentLevel;
                existing.MaterialItemId = dto.MaterialItemId;
                existing.MaterialCount = dto.MaterialCount;
                existing.GoldCost = dto.GoldCost;
                existing.SuccessRate = dto.SuccessRate;
                existing.AttributeGrowthPercent = dto.AttributeGrowthPercent;
                existing.MaxEnhanceLevel = dto.MaxEnhanceLevel;
                existing.SortOrder = dto.SortOrder;
                existing.IsEnabled = dto.IsEnabled;
                existing.IsBuiltIn = false;
                existing.SeedKey = null;
                existing.BuiltInVersion = null;
                existing.LastUpdateTime = now;
                await _dbContext.Db.Updateable(existing).ExecuteCommandAsync();
            }
            await _runtimeRefreshService.ReloadEquipmentRerollRuleCacheAsync();
        }

        public async Task DeleteEnhanceRuleAsync(long gid)
        {
            await _dbContext.Db.Deleteable<EquipmentEnhanceRuleEntity>()
                .Where(item => item.GID == gid)
                .ExecuteCommandAsync();
            await _runtimeRefreshService.ReloadEquipmentRerollRuleCacheAsync();
        }

        public async Task<List<AdminEquipmentRerollCostRuleDto>> GetRerollCostRulesAsync()
        {
            var entities = await _dbContext.Db.Queryable<EquipmentRerollCostRuleEntity>()
                .OrderBy(item => item.MinEquipmentLevel, OrderByType.Asc)
                .OrderBy(item => item.SortOrder, OrderByType.Asc)
                .ToListAsync();
            return entities.Select(MapToDto).ToList();
        }

        public async Task SaveRerollCostRuleAsync(AdminEquipmentRerollCostRuleDto dto)
        {
            ValidateLevelRange(dto.MinEquipmentLevel, dto.MaxEquipmentLevel);
            ValidatePositive(dto.MaterialCount, "洗炼材料数量");
            ValidateNonNegative(dto.GoldCost, "洗炼金币消耗");
            ValidateNonNegative(dto.ExtraMaterialPerLockedLine, "锁定词条额外材料");
            ValidateNonNegative(dto.ExtraGoldPerLockedLine, "锁定词条额外金币");
            await ValidateMaterialAsync(dto.MaterialItemId);
            await EnsureNoRerollOverlapAsync(dto);

            var now = DateTime.Now;
            if (dto.GID == 0)
            {
                await _dbContext.Db.Insertable(new EquipmentRerollCostRuleEntity
                {
                    MinEquipmentLevel = dto.MinEquipmentLevel,
                    MaxEquipmentLevel = dto.MaxEquipmentLevel,
                    MaterialItemId = dto.MaterialItemId,
                    MaterialCount = dto.MaterialCount,
                    GoldCost = dto.GoldCost,
                    ExtraMaterialPerLockedLine = dto.ExtraMaterialPerLockedLine,
                    ExtraGoldPerLockedLine = dto.ExtraGoldPerLockedLine,
                    SortOrder = dto.SortOrder,
                    IsEnabled = dto.IsEnabled,
                    LastUpdateTime = now
                }).ExecuteCommandAsync();
            }
            else
            {
                var existing = await _dbContext.Db.Queryable<EquipmentRerollCostRuleEntity>()
                    .FirstAsync(item => item.GID == dto.GID);
                if (existing == null) throw new InvalidOperationException("洗炼消耗规则不存在。");
                existing.MinEquipmentLevel = dto.MinEquipmentLevel;
                existing.MaxEquipmentLevel = dto.MaxEquipmentLevel;
                existing.MaterialItemId = dto.MaterialItemId;
                existing.MaterialCount = dto.MaterialCount;
                existing.GoldCost = dto.GoldCost;
                existing.ExtraMaterialPerLockedLine = dto.ExtraMaterialPerLockedLine;
                existing.ExtraGoldPerLockedLine = dto.ExtraGoldPerLockedLine;
                existing.SortOrder = dto.SortOrder;
                existing.IsEnabled = dto.IsEnabled;
                existing.IsBuiltIn = false;
                existing.SeedKey = null;
                existing.BuiltInVersion = null;
                existing.LastUpdateTime = now;
                await _dbContext.Db.Updateable(existing).ExecuteCommandAsync();
            }
            await _runtimeRefreshService.ReloadEquipmentRerollRuleCacheAsync();
        }

        public async Task DeleteRerollCostRuleAsync(long gid)
        {
            await _dbContext.Db.Deleteable<EquipmentRerollCostRuleEntity>()
                .Where(item => item.GID == gid)
                .ExecuteCommandAsync();
            await _runtimeRefreshService.ReloadEquipmentRerollRuleCacheAsync();
        }

        private async Task ValidateMaterialAsync(string materialItemId)
        {
            if (string.IsNullOrWhiteSpace(materialItemId) ||
                !await _dbContext.Db.Queryable<ItemTemplateEntity>().AnyAsync(item => item.ItemId == materialItemId))
            {
                throw new InvalidOperationException("强化/洗炼材料道具不存在。");
            }
        }

        private async Task EnsureNoEnhanceOverlapAsync(AdminEquipmentEnhanceRuleDto dto)
        {
            var overlap = await _dbContext.Db.Queryable<EquipmentEnhanceRuleEntity>()
                .Where(item => item.IsEnabled && (dto.GID == 0 || item.GID != dto.GID) &&
                               item.MinEquipmentLevel <= dto.MaxEquipmentLevel &&
                               item.MaxEquipmentLevel >= dto.MinEquipmentLevel)
                .AnyAsync();
            if (dto.IsEnabled && overlap) throw new InvalidOperationException("强化规则等级区间与现有启用规则重叠。");
        }

        private async Task EnsureNoRerollOverlapAsync(AdminEquipmentRerollCostRuleDto dto)
        {
            var overlap = await _dbContext.Db.Queryable<EquipmentRerollCostRuleEntity>()
                .Where(item => item.IsEnabled && (dto.GID == 0 || item.GID != dto.GID) &&
                               item.MinEquipmentLevel <= dto.MaxEquipmentLevel &&
                               item.MaxEquipmentLevel >= dto.MinEquipmentLevel)
                .AnyAsync();
            if (dto.IsEnabled && overlap) throw new InvalidOperationException("洗炼规则等级区间与现有启用规则重叠。");
        }

        private static void ValidateLevelRange(int min, int max)
        {
            if (min < 1 || max < min) throw new InvalidOperationException("装备等级区间无效。");
        }

        private static void ValidatePositive(long value, string name)
        {
            if (value <= 0) throw new InvalidOperationException($"{name}必须大于0。");
        }

        private static void ValidateNonNegative(long value, string name)
        {
            if (value < 0) throw new InvalidOperationException($"{name}不能小于0。");
        }

        private static void ValidatePercent(int value, string name)
        {
            if (value < 0 || value > 100) throw new InvalidOperationException($"{name}必须在0到100之间。");
        }

        public async Task<AdminEquipmentRerollSystemConfigDto?> GetSystemConfigAsync()
        {
            var entity = await _dbContext.Db.Queryable<EquipmentRerollSystemConfigEntity>()
                .FirstAsync(item => item.ConfigId == "default");
            if (entity == null) return null;

            return MapToDto(entity);
        }

        public async Task SaveSystemConfigAsync(AdminEquipmentRerollSystemConfigDto dto)
        {
            var existing = await _dbContext.Db.Queryable<EquipmentRerollSystemConfigEntity>()
                .FirstAsync(item => item.ConfigId == dto.ConfigId);

            if (existing == null)
            {
                var entity = new EquipmentRerollSystemConfigEntity
                {
                    ConfigId = dto.ConfigId,
                    IsEnabled = dto.IsEnabled,
                    RerollStoneItemId = dto.RerollStoneItemId,
                    BaseStoneCost = dto.BaseStoneCost,
                    ExtraStoneCostPerLockedLine = dto.ExtraStoneCostPerLockedLine,
                    MaxLockedLineCount = dto.MaxLockedLineCount,
                    BaseGoldCost = dto.BaseGoldCost,
                    GoldCostPerEquipmentLevel = dto.GoldCostPerEquipmentLevel,
                    QualityGoldMultipliersJson = dto.QualityGoldMultipliersJson,
                    RerollCountGoldGrowthPercent = dto.RerollCountGoldGrowthPercent,
                    SeedKey = null,
                    IsBuiltIn = false,
                    BuiltInVersion = null,
                    LastUpdateTime = DateTime.Now
                };
                await _dbContext.Db.Insertable(entity).ExecuteCommandAsync();
            }
            else
            {
                existing.IsEnabled = dto.IsEnabled;
                existing.RerollStoneItemId = dto.RerollStoneItemId;
                existing.BaseStoneCost = dto.BaseStoneCost;
                existing.ExtraStoneCostPerLockedLine = dto.ExtraStoneCostPerLockedLine;
                existing.MaxLockedLineCount = dto.MaxLockedLineCount;
                existing.BaseGoldCost = dto.BaseGoldCost;
                existing.GoldCostPerEquipmentLevel = dto.GoldCostPerEquipmentLevel;
                existing.QualityGoldMultipliersJson = dto.QualityGoldMultipliersJson;
                existing.RerollCountGoldGrowthPercent = dto.RerollCountGoldGrowthPercent;
                existing.IsBuiltIn = false;
                existing.SeedKey = null;
                existing.BuiltInVersion = null;
                existing.LastUpdateTime = DateTime.Now;
                await _dbContext.Db.Updateable(existing).ExecuteCommandAsync();
            }

            await _runtimeRefreshService.ReloadEquipmentRerollRuleCacheAsync();
        }

        public async Task<List<AdminEquipmentRerollSlotPoolConfigDto>> GetSlotPoolConfigsAsync()
        {
            var entities = await _dbContext.Db.Queryable<EquipmentRerollSlotPoolConfigEntity>()
                .OrderBy(item => item.Slot, OrderByType.Asc)
                .OrderBy(item => item.SortOrder, OrderByType.Asc)
                .ToListAsync();

            return entities.Select(MapToDto).ToList();
        }

        public async Task SaveSlotPoolConfigAsync(AdminEquipmentRerollSlotPoolConfigDto dto)
        {
            if (dto.GID == 0)
            {
                var entity = new EquipmentRerollSlotPoolConfigEntity
                {
                    Slot = dto.Slot,
                    AttributeType = dto.AttributeType,
                    Tier = dto.Tier,
                    MaxDuplicateCount = dto.MaxDuplicateCount,
                    SortOrder = dto.SortOrder,
                    IsEnabled = dto.IsEnabled,
                    SeedKey = null,
                    IsBuiltIn = false,
                    BuiltInVersion = null,
                    LastUpdateTime = DateTime.Now
                };
                await _dbContext.Db.Insertable(entity).ExecuteCommandAsync();
            }
            else
            {
                var existing = await _dbContext.Db.Queryable<EquipmentRerollSlotPoolConfigEntity>()
                    .FirstAsync(item => item.GID == dto.GID);
                if (existing == null) return;

                existing.Slot = dto.Slot;
                existing.AttributeType = dto.AttributeType;
                existing.Tier = dto.Tier;
                existing.MaxDuplicateCount = dto.MaxDuplicateCount;
                existing.SortOrder = dto.SortOrder;
                existing.IsEnabled = dto.IsEnabled;
                existing.IsBuiltIn = false;
                existing.SeedKey = null;
                existing.BuiltInVersion = null;
                existing.LastUpdateTime = DateTime.Now;
                await _dbContext.Db.Updateable(existing).ExecuteCommandAsync();
            }

            await _runtimeRefreshService.ReloadEquipmentRerollRuleCacheAsync();
        }

        public async Task DeleteSlotPoolConfigAsync(long gid)
        {
            await _dbContext.Db.Deleteable<EquipmentRerollSlotPoolConfigEntity>()
                .Where(item => item.GID == gid)
                .ExecuteCommandAsync();
            await _runtimeRefreshService.ReloadEquipmentRerollRuleCacheAsync();
        }

        public async Task<List<AdminEquipmentRerollTierConfigDto>> GetTierConfigsAsync()
        {
            var entities = await _dbContext.Db.Queryable<EquipmentRerollTierConfigEntity>()
                .OrderBy(item => item.SortOrder)
                .ToListAsync();

            return entities.Select(MapToDto).ToList();
        }

        public async Task SaveTierConfigAsync(AdminEquipmentRerollTierConfigDto dto)
        {
            if (dto.GID == 0)
            {
                var entity = new EquipmentRerollTierConfigEntity
                {
                    Tier = dto.Tier,
                    Name = dto.Name,
                    Color = dto.Color,
                    Weight = dto.Weight,
                    ValueMultiplier = dto.ValueMultiplier,
                    SortOrder = dto.SortOrder,
                    IsEnabled = dto.IsEnabled,
                    SeedKey = null,
                    IsBuiltIn = false,
                    BuiltInVersion = null,
                    LastUpdateTime = DateTime.Now
                };
                await _dbContext.Db.Insertable(entity).ExecuteCommandAsync();
            }
            else
            {
                var existing = await _dbContext.Db.Queryable<EquipmentRerollTierConfigEntity>()
                    .FirstAsync(item => item.GID == dto.GID);
                if (existing == null) return;

                existing.Tier = dto.Tier;
                existing.Name = dto.Name;
                existing.Color = dto.Color;
                existing.Weight = dto.Weight;
                existing.ValueMultiplier = dto.ValueMultiplier;
                existing.SortOrder = dto.SortOrder;
                existing.IsEnabled = dto.IsEnabled;
                existing.IsBuiltIn = false;
                existing.SeedKey = null;
                existing.BuiltInVersion = null;
                existing.LastUpdateTime = DateTime.Now;
                await _dbContext.Db.Updateable(existing).ExecuteCommandAsync();
            }

            await _runtimeRefreshService.ReloadEquipmentRerollRuleCacheAsync();
        }

        public async Task DeleteTierConfigAsync(long gid)
        {
            await _dbContext.Db.Deleteable<EquipmentRerollTierConfigEntity>()
                .Where(item => item.GID == gid)
                .ExecuteCommandAsync();
            await _runtimeRefreshService.ReloadEquipmentRerollRuleCacheAsync();
        }

        private static AdminEquipmentRerollSystemConfigDto MapToDto(EquipmentRerollSystemConfigEntity entity)
        {
            return new AdminEquipmentRerollSystemConfigDto
            {
                ConfigId = entity.ConfigId,
                IsEnabled = entity.IsEnabled,
                RerollStoneItemId = entity.RerollStoneItemId,
                BaseStoneCost = entity.BaseStoneCost,
                ExtraStoneCostPerLockedLine = entity.ExtraStoneCostPerLockedLine,
                MaxLockedLineCount = entity.MaxLockedLineCount,
                BaseGoldCost = entity.BaseGoldCost,
                GoldCostPerEquipmentLevel = entity.GoldCostPerEquipmentLevel,
                QualityGoldMultipliersJson = entity.QualityGoldMultipliersJson,
                RerollCountGoldGrowthPercent = entity.RerollCountGoldGrowthPercent,
                IsBuiltIn = entity.IsBuiltIn,
                BuiltInVersion = entity.BuiltInVersion,
                SeedKey = entity.SeedKey,
                LastUpdateTime = entity.LastUpdateTime
            };
        }

        private static AdminEquipmentEnhanceRuleDto MapToDto(EquipmentEnhanceRuleEntity entity)
        {
            return new AdminEquipmentEnhanceRuleDto
            {
                GID = entity.GID,
                MinEquipmentLevel = entity.MinEquipmentLevel,
                MaxEquipmentLevel = entity.MaxEquipmentLevel,
                MaterialItemId = entity.MaterialItemId,
                MaterialCount = entity.MaterialCount,
                GoldCost = entity.GoldCost,
                SuccessRate = entity.SuccessRate,
                AttributeGrowthPercent = entity.AttributeGrowthPercent,
                MaxEnhanceLevel = entity.MaxEnhanceLevel,
                SortOrder = entity.SortOrder,
                IsEnabled = entity.IsEnabled,
                IsBuiltIn = entity.IsBuiltIn,
                BuiltInVersion = entity.BuiltInVersion,
                SeedKey = entity.SeedKey,
                LastUpdateTime = entity.LastUpdateTime
            };
        }

        private static AdminEquipmentRerollCostRuleDto MapToDto(EquipmentRerollCostRuleEntity entity)
        {
            return new AdminEquipmentRerollCostRuleDto
            {
                GID = entity.GID,
                MinEquipmentLevel = entity.MinEquipmentLevel,
                MaxEquipmentLevel = entity.MaxEquipmentLevel,
                MaterialItemId = entity.MaterialItemId,
                MaterialCount = entity.MaterialCount,
                GoldCost = entity.GoldCost,
                ExtraMaterialPerLockedLine = entity.ExtraMaterialPerLockedLine,
                ExtraGoldPerLockedLine = entity.ExtraGoldPerLockedLine,
                SortOrder = entity.SortOrder,
                IsEnabled = entity.IsEnabled,
                IsBuiltIn = entity.IsBuiltIn,
                BuiltInVersion = entity.BuiltInVersion,
                SeedKey = entity.SeedKey,
                LastUpdateTime = entity.LastUpdateTime
            };
        }

        public async Task<List<AdminEquipmentRerollAttributeValueConfigDto>> GetAttributeValueConfigsAsync()
        {
            var entities = await _dbContext.Db.Queryable<EquipmentRerollAttributeValueConfigEntity>()
                .OrderBy(item => item.AttributeType, OrderByType.Asc)
                .OrderBy(item => item.Tier, OrderByType.Asc)
                .ToListAsync();

            return entities.Select(MapToDto).ToList();
        }

        public async Task SaveAttributeValueConfigAsync(AdminEquipmentRerollAttributeValueConfigDto dto)
        {
            if (dto.GID == 0)
            {
                var entity = new EquipmentRerollAttributeValueConfigEntity
                {
                    AttributeType = dto.AttributeType,
                    Tier = dto.Tier,
                    MinValue = dto.MinValue,
                    MaxValue = dto.MaxValue,
                    IsPercentage = dto.IsPercentage,
                    SortOrder = dto.SortOrder,
                    IsEnabled = dto.IsEnabled,
                    SeedKey = null,
                    IsBuiltIn = false,
                    BuiltInVersion = null,
                    LastUpdateTime = DateTime.Now
                };
                await _dbContext.Db.Insertable(entity).ExecuteCommandAsync();
            }
            else
            {
                var existing = await _dbContext.Db.Queryable<EquipmentRerollAttributeValueConfigEntity>()
                    .FirstAsync(item => item.GID == dto.GID);
                if (existing == null) return;

                existing.AttributeType = dto.AttributeType;
                existing.Tier = dto.Tier;
                existing.MinValue = dto.MinValue;
                existing.MaxValue = dto.MaxValue;
                existing.IsPercentage = dto.IsPercentage;
                existing.SortOrder = dto.SortOrder;
                existing.IsEnabled = dto.IsEnabled;
                existing.IsBuiltIn = false;
                existing.SeedKey = null;
                existing.BuiltInVersion = null;
                existing.LastUpdateTime = DateTime.Now;
                await _dbContext.Db.Updateable(existing).ExecuteCommandAsync();
            }

            await _runtimeRefreshService.ReloadEquipmentRerollRuleCacheAsync();
        }

        public async Task DeleteAttributeValueConfigAsync(long gid)
        {
            await _dbContext.Db.Deleteable<EquipmentRerollAttributeValueConfigEntity>()
                .Where(item => item.GID == gid)
                .ExecuteCommandAsync();
            await _runtimeRefreshService.ReloadEquipmentRerollRuleCacheAsync();
        }

        private static AdminEquipmentRerollSlotPoolConfigDto MapToDto(EquipmentRerollSlotPoolConfigEntity entity)
        {
            return new AdminEquipmentRerollSlotPoolConfigDto
            {
                GID = entity.GID,
                Slot = entity.Slot,
                AttributeType = entity.AttributeType,
                Tier = entity.Tier,
                MaxDuplicateCount = entity.MaxDuplicateCount,
                SortOrder = entity.SortOrder,
                IsEnabled = entity.IsEnabled,
                IsBuiltIn = entity.IsBuiltIn,
                BuiltInVersion = entity.BuiltInVersion,
                SeedKey = entity.SeedKey,
                LastUpdateTime = entity.LastUpdateTime
            };
        }

        private static AdminEquipmentRerollAttributeValueConfigDto MapToDto(EquipmentRerollAttributeValueConfigEntity entity)
        {
            return new AdminEquipmentRerollAttributeValueConfigDto
            {
                GID = entity.GID,
                AttributeType = entity.AttributeType,
                Tier = entity.Tier,
                MinValue = entity.MinValue,
                MaxValue = entity.MaxValue,
                IsPercentage = entity.IsPercentage,
                SortOrder = entity.SortOrder,
                IsEnabled = entity.IsEnabled,
                IsBuiltIn = entity.IsBuiltIn,
                BuiltInVersion = entity.BuiltInVersion,
                SeedKey = entity.SeedKey,
                LastUpdateTime = entity.LastUpdateTime
            };
        }

        private static AdminEquipmentRerollTierConfigDto MapToDto(EquipmentRerollTierConfigEntity entity)
        {
            return new AdminEquipmentRerollTierConfigDto
            {
                GID = entity.GID,
                Tier = entity.Tier,
                Name = entity.Name,
                Color = entity.Color,
                Weight = entity.Weight,
                ValueMultiplier = entity.ValueMultiplier,
                SortOrder = entity.SortOrder,
                IsEnabled = entity.IsEnabled,
                IsBuiltIn = entity.IsBuiltIn,
                BuiltInVersion = entity.BuiltInVersion,
                SeedKey = entity.SeedKey,
                LastUpdateTime = entity.LastUpdateTime
            };
        }
    }
}
