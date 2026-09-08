#pragma warning disable CS1591
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    public class AdminSpiritFieldRuleService : IAdminSpiritFieldRuleService
    {
        private readonly IRepository<SpiritFieldSystemConfigEntity> _systemConfigRepository;
        private readonly IRepository<SpiritFieldSpeedUpItemConfigEntity> _speedUpItemRepository;
        private readonly IAdminRuntimeRefreshService _runtimeRefreshService;

        public AdminSpiritFieldRuleService(
            IRepository<SpiritFieldSystemConfigEntity> systemConfigRepository,
            IRepository<SpiritFieldSpeedUpItemConfigEntity> speedUpItemRepository,
            IAdminRuntimeRefreshService runtimeRefreshService)
        {
            _systemConfigRepository = systemConfigRepository;
            _speedUpItemRepository = speedUpItemRepository;
            _runtimeRefreshService = runtimeRefreshService;
        }

        public async Task<AdminSpiritFieldSystemRuleDto?> GetSystemRuleAsync()
        {
            var entity = await _systemConfigRepository.Db.Queryable<SpiritFieldSystemConfigEntity>()
                .OrderByDescending(item => item.LastUpdateTime)
                .FirstAsync();
            return entity == null ? null : MapSystemRule(entity);
        }

        public async Task<AdminSpiritFieldSystemRuleDto> SaveSystemRuleAsync(AdminSpiritFieldSystemRuleDto request)
        {
            var configId = string.IsNullOrWhiteSpace(request.ConfigId) ? "default" : request.ConfigId.Trim();
            var entity = await _systemConfigRepository.GetByIdAsync(configId) ?? new SpiritFieldSystemConfigEntity { ConfigId = configId };
            entity.DefaultFieldLevel = Math.Max(1, request.DefaultFieldLevel);
            entity.DefaultUnlockedPlots = Math.Max(1, request.DefaultUnlockedPlots);
            entity.DefaultMaxPlots = Math.Max(entity.DefaultUnlockedPlots, request.DefaultMaxPlots);
            entity.DefaultInventoryCapacity = Math.Max(1, request.DefaultInventoryCapacity);
            entity.PlotUpgradeGoldPerLevel = Math.Max(0L, request.PlotUpgradeGoldPerLevel);
            entity.PlotUpgradeSpiritStonePerLevel = Math.Max(0L, request.PlotUpgradeSpiritStonePerLevel);
            entity.PlotUpgradeYieldBonusPerLevel = Math.Max(0, request.PlotUpgradeYieldBonusPerLevel);
            entity.IsEnabled = request.IsEnabled;
            entity.IsBuiltIn = false;
            entity.SeedKey = null;
            entity.BuiltInVersion = null;
            entity.LastUpdateTime = DateTime.Now;

            if (await _systemConfigRepository.GetByIdAsync(configId) == null)
            {
                await _systemConfigRepository.AddAsync(entity);
            }
            else
            {
                await _systemConfigRepository.UpdateAsync(entity);
            }

            await _runtimeRefreshService.ReloadSpiritFieldRuleCacheAsync();
            return (await GetSystemRuleAsync())!;
        }

        public async Task<List<AdminSpiritFieldSpeedUpItemRuleDto>> GetSpeedUpRulesAsync()
        {
            var entities = (await _speedUpItemRepository.Db.Queryable<SpiritFieldSpeedUpItemConfigEntity>()
                    .ToListAsync())
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.ItemId, StringComparer.OrdinalIgnoreCase)
                .ToList();
            return entities.Select(MapSpeedUpRule).ToList();
        }

        public async Task<AdminSpiritFieldSpeedUpItemRuleDto?> GetSpeedUpRuleAsync(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return null;
            }

            var entity = await _speedUpItemRepository.GetByIdAsync(itemId.Trim());
            return entity == null ? null : MapSpeedUpRule(entity);
        }

        public async Task<AdminSpiritFieldSpeedUpItemRuleDto> SaveSpeedUpRuleAsync(AdminSpiritFieldSpeedUpItemRuleDto request)
        {
            var itemId = (request.ItemId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(itemId))
            {
                throw new InvalidOperationException("催熟道具编号不能为空。");
            }

            var existing = await _speedUpItemRepository.GetByIdAsync(itemId) ?? new SpiritFieldSpeedUpItemConfigEntity { ItemId = itemId };
            existing.SpeedUpSeconds = Math.Max(1, request.SpeedUpSeconds);
            existing.SortOrder = request.SortOrder;
            existing.IsEnabled = request.IsEnabled;
            existing.IsBuiltIn = false;
            existing.SeedKey = null;
            existing.BuiltInVersion = null;
            existing.LastUpdateTime = DateTime.Now;

            if (await _speedUpItemRepository.GetByIdAsync(itemId) == null)
            {
                await _speedUpItemRepository.AddAsync(existing);
            }
            else
            {
                await _speedUpItemRepository.UpdateAsync(existing);
            }

            await _runtimeRefreshService.ReloadSpiritFieldRuleCacheAsync();
            return (await GetSpeedUpRuleAsync(itemId))!;
        }

        public async Task<bool> DeleteSpeedUpRuleAsync(string itemId)
        {
            var normalizedItemId = (itemId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedItemId))
            {
                return false;
            }

            var rows = await _speedUpItemRepository.DeleteAsync(normalizedItemId);
            if (rows > 0)
            {
                await _runtimeRefreshService.ReloadSpiritFieldRuleCacheAsync();
            }

            return rows > 0;
        }

        private static AdminSpiritFieldSystemRuleDto MapSystemRule(SpiritFieldSystemConfigEntity entity)
        {
            return new AdminSpiritFieldSystemRuleDto
            {
                ConfigId = entity.ConfigId,
                DefaultFieldLevel = entity.DefaultFieldLevel,
                DefaultUnlockedPlots = entity.DefaultUnlockedPlots,
                DefaultMaxPlots = entity.DefaultMaxPlots,
                DefaultInventoryCapacity = entity.DefaultInventoryCapacity,
                PlotUpgradeGoldPerLevel = entity.PlotUpgradeGoldPerLevel,
                PlotUpgradeSpiritStonePerLevel = entity.PlotUpgradeSpiritStonePerLevel,
                PlotUpgradeYieldBonusPerLevel = entity.PlotUpgradeYieldBonusPerLevel,
                IsEnabled = entity.IsEnabled,
                IsBuiltIn = entity.IsBuiltIn,
                SeedKey = entity.SeedKey,
                BuiltInVersion = entity.BuiltInVersion,
                LastUpdateTime = entity.LastUpdateTime
            };
        }

        private static AdminSpiritFieldSpeedUpItemRuleDto MapSpeedUpRule(SpiritFieldSpeedUpItemConfigEntity entity)
        {
            return new AdminSpiritFieldSpeedUpItemRuleDto
            {
                ItemId = entity.ItemId,
                SpeedUpSeconds = entity.SpeedUpSeconds,
                SortOrder = entity.SortOrder,
                IsEnabled = entity.IsEnabled,
                IsBuiltIn = entity.IsBuiltIn,
                SeedKey = entity.SeedKey,
                BuiltInVersion = entity.BuiltInVersion,
                LastUpdateTime = entity.LastUpdateTime
            };
        }
    }
}
#pragma warning restore CS1591
