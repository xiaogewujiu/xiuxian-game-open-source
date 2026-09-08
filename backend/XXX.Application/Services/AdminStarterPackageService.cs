#pragma warning disable CS1591
using SqlSugar;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    public class AdminStarterPackageService : IAdminStarterPackageService
    {
        private readonly IRepository<StarterPackageConfigEntity> _configRepository;
        private readonly IRepository<StarterPackageGrantItemEntity> _itemGrantRepository;
        private readonly IRepository<StarterPackageGrantSkillEntity> _skillGrantRepository;
        private readonly IAdminRuntimeRefreshService _runtimeRefreshService;

        public AdminStarterPackageService(
            IRepository<StarterPackageConfigEntity> configRepository,
            IRepository<StarterPackageGrantItemEntity> itemGrantRepository,
            IRepository<StarterPackageGrantSkillEntity> skillGrantRepository,
            IAdminRuntimeRefreshService runtimeRefreshService)
        {
            _configRepository = configRepository;
            _itemGrantRepository = itemGrantRepository;
            _skillGrantRepository = skillGrantRepository;
            _runtimeRefreshService = runtimeRefreshService;
        }

        public async Task<List<AdminStarterPackageListItemDto>> GetListAsync(string? keyword = null, bool? autoGrantOnRegister = null)
        {
            var normalizedKeyword = (keyword ?? string.Empty).Trim();
            var query = _configRepository.Db.Queryable<StarterPackageConfigEntity>();
            if (!string.IsNullOrWhiteSpace(normalizedKeyword))
            {
                query = query.Where(item =>
                    item.PackageId.Contains(normalizedKeyword) ||
                    item.Name.Contains(normalizedKeyword) ||
                    (item.Description != null && item.Description.Contains(normalizedKeyword)));
            }

            query = query.WhereIF(autoGrantOnRegister.HasValue, item => item.AutoGrantOnRegister == autoGrantOnRegister!.Value);

            var configs = await query
                .OrderBy(item => item.SortOrder)
                .OrderBy(item => item.PackageId)
                .ToListAsync();
            var itemCounts = await _itemGrantRepository.Db.Queryable<StarterPackageGrantItemEntity>()
                .GroupBy(item => item.PackageId)
                .Select(item => new { item.PackageId, Count = SqlFunc.AggregateCount(item.PackageId) })
                .ToListAsync();
            var skillCounts = await _skillGrantRepository.Db.Queryable<StarterPackageGrantSkillEntity>()
                .GroupBy(item => item.PackageId)
                .Select(item => new { item.PackageId, Count = SqlFunc.AggregateCount(item.PackageId) })
                .ToListAsync();
            var itemCountMap = itemCounts.ToDictionary(item => item.PackageId, item => item.Count, StringComparer.OrdinalIgnoreCase);
            var skillCountMap = skillCounts.ToDictionary(item => item.PackageId, item => item.Count, StringComparer.OrdinalIgnoreCase);

            return configs.Select(item => new AdminStarterPackageListItemDto
            {
                PackageId = item.PackageId,
                Name = item.Name,
                IsEnabled = item.IsEnabled,
                AutoGrantOnRegister = item.AutoGrantOnRegister,
                SortOrder = item.SortOrder,
                ItemGrantCount = itemCountMap.GetValueOrDefault(item.PackageId),
                SkillGrantCount = skillCountMap.GetValueOrDefault(item.PackageId),
                ConfigVersion = item.ConfigVersion,
                IsBuiltIn = item.IsBuiltIn
            }).ToList();
        }

        public async Task<AdminStarterPackageDetailDto?> GetDetailAsync(string packageId)
        {
            var normalizedPackageId = NormalizePackageId(packageId);
            if (string.IsNullOrWhiteSpace(normalizedPackageId))
            {
                return null;
            }

            var config = await _configRepository.GetByIdAsync(normalizedPackageId);
            if (config == null)
            {
                return null;
            }

            return await MapDetailAsync(config);
        }

        public async Task<AdminStarterPackageDetailDto> SaveAsync(AdminStarterPackageDetailDto request, string? operatorName = null)
        {
            var packageId = NormalizePackageId(request.PackageId);
            if (string.IsNullOrWhiteSpace(packageId))
            {
                throw new InvalidOperationException("礼包编号不能为空。");
            }

            var name = (request.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("礼包名称不能为空。");
            }

            var existing = await _configRepository.GetByIdAsync(packageId);
            if (request.AutoGrantOnRegister)
            {
                await DisableOtherRegisterPackagesAsync(packageId);
            }

            var now = DateTime.Now;
            var normalizedOperator = string.IsNullOrWhiteSpace(operatorName) ? "system" : operatorName.Trim();
            if (existing == null)
            {
                existing = new StarterPackageConfigEntity
                {
                    PackageId = packageId,
                    CreateTime = now,
                    CreatedBy = normalizedOperator
                };
                ApplyConfig(existing, request, normalizedOperator, now);
                await _configRepository.AddAsync(existing);
            }
            else
            {
                ApplyConfig(existing, request, normalizedOperator, now);
                await _configRepository.UpdateAsync(existing);
            }

            await ReplaceItemGrantsAsync(packageId, request.ItemGrants);
            await ReplaceSkillGrantsAsync(packageId, request.SkillGrants);
            await _runtimeRefreshService.ReloadStarterPackageCacheAsync();

            return (await GetDetailAsync(packageId))!;
        }

        public async Task<bool> DeleteAsync(string packageId)
        {
            var normalizedPackageId = NormalizePackageId(packageId);
            if (string.IsNullOrWhiteSpace(normalizedPackageId))
            {
                return false;
            }

            await _itemGrantRepository.Db.Deleteable<StarterPackageGrantItemEntity>()
                .Where(item => item.PackageId == normalizedPackageId)
                .ExecuteCommandAsync();
            await _skillGrantRepository.Db.Deleteable<StarterPackageGrantSkillEntity>()
                .Where(item => item.PackageId == normalizedPackageId)
                .ExecuteCommandAsync();

            var deleted = await _configRepository.DeleteAsync(normalizedPackageId) > 0;
            if (deleted)
            {
                await _runtimeRefreshService.ReloadStarterPackageCacheAsync();
            }

            return deleted;
        }

        private async Task<AdminStarterPackageDetailDto> MapDetailAsync(StarterPackageConfigEntity config)
        {
            var itemGrants = await _itemGrantRepository.Db.Queryable<StarterPackageGrantItemEntity>()
                .Where(item => item.PackageId == config.PackageId)
                .OrderBy(item => item.SortOrder)
                .OrderBy(item => item.GID)
                .ToListAsync();
            var skillGrants = await _skillGrantRepository.Db.Queryable<StarterPackageGrantSkillEntity>()
                .Where(item => item.PackageId == config.PackageId)
                .OrderBy(item => item.SortOrder)
                .OrderBy(item => item.GID)
                .ToListAsync();

            return new AdminStarterPackageDetailDto
            {
                PackageId = config.PackageId,
                Name = config.Name,
                Description = config.Description ?? string.Empty,
                IsEnabled = config.IsEnabled,
                AutoGrantOnRegister = config.AutoGrantOnRegister,
                SortOrder = config.SortOrder,
                ConfigVersion = config.ConfigVersion,
                SeedKey = config.SeedKey,
                IsBuiltIn = config.IsBuiltIn,
                BuiltInVersion = config.BuiltInVersion,
                Remark = config.Remark,
                CreatedBy = config.CreatedBy,
                UpdatedBy = config.UpdatedBy,
                CreateTime = config.CreateTime,
                LastUpdateTime = config.LastUpdateTime,
                ItemGrants = itemGrants.Select(item => new AdminStarterPackageItemGrantDto
                {
                    GID = item.GID,
                    ItemId = item.ItemId,
                    Quantity = item.Quantity,
                    SortOrder = item.SortOrder
                }).ToList(),
                SkillGrants = skillGrants.Select(item => new AdminStarterPackageSkillGrantDto
                {
                    GID = item.GID,
                    SkillId = item.SkillId,
                    SortOrder = item.SortOrder
                }).ToList()
            };
        }

        private static void ApplyConfig(StarterPackageConfigEntity entity, AdminStarterPackageDetailDto request, string operatorName, DateTime now)
        {
            entity.Name = request.Name.Trim();
            entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            entity.IsEnabled = request.IsEnabled;
            entity.AutoGrantOnRegister = request.AutoGrantOnRegister;
            entity.SortOrder = request.SortOrder;
            entity.ConfigVersion = string.IsNullOrWhiteSpace(request.ConfigVersion) ? "manual-admin" : request.ConfigVersion.Trim();
            entity.SeedKey = string.IsNullOrWhiteSpace(request.SeedKey) ? null : request.SeedKey.Trim();
            entity.IsBuiltIn = request.IsBuiltIn;
            entity.BuiltInVersion = string.IsNullOrWhiteSpace(request.BuiltInVersion) ? null : request.BuiltInVersion.Trim();
            entity.Remark = string.IsNullOrWhiteSpace(request.Remark) ? null : request.Remark.Trim();
            entity.UpdatedBy = operatorName;
            entity.LastUpdateTime = now;
        }

        private async Task ReplaceItemGrantsAsync(string packageId, List<AdminStarterPackageItemGrantDto>? grants)
        {
            await _itemGrantRepository.Db.Deleteable<StarterPackageGrantItemEntity>()
                .Where(item => item.PackageId == packageId)
                .ExecuteCommandAsync();

            var items = (grants ?? [])
                .Where(item => !string.IsNullOrWhiteSpace(item.ItemId) && item.Quantity > 0)
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.ItemId, StringComparer.OrdinalIgnoreCase)
                .Select((item, index) => new StarterPackageGrantItemEntity
                {
                    GID = Guid.NewGuid().ToString("N"),
                    PackageId = packageId,
                    ItemId = item.ItemId.Trim(),
                    Quantity = Math.Max(1, item.Quantity),
                    SortOrder = item.SortOrder > 0 ? item.SortOrder : index + 1
                })
                .ToList();

            if (items.Count > 0)
            {
                await _itemGrantRepository.Db.Insertable(items).ExecuteCommandAsync();
            }
        }

        private async Task ReplaceSkillGrantsAsync(string packageId, List<AdminStarterPackageSkillGrantDto>? grants)
        {
            await _skillGrantRepository.Db.Deleteable<StarterPackageGrantSkillEntity>()
                .Where(item => item.PackageId == packageId)
                .ExecuteCommandAsync();

            var skills = (grants ?? [])
                .Where(item => item.SkillId > 0)
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.SkillId)
                .Select((item, index) => new StarterPackageGrantSkillEntity
                {
                    GID = Guid.NewGuid().ToString("N"),
                    PackageId = packageId,
                    SkillId = item.SkillId,
                    SortOrder = item.SortOrder > 0 ? item.SortOrder : index + 1
                })
                .ToList();

            if (skills.Count > 0)
            {
                await _skillGrantRepository.Db.Insertable(skills).ExecuteCommandAsync();
            }
        }

        private async Task DisableOtherRegisterPackagesAsync(string currentPackageId)
        {
            await _configRepository.Db.Updateable<StarterPackageConfigEntity>()
                .SetColumns(item => item.AutoGrantOnRegister == false)
                .SetColumns(item => item.LastUpdateTime == DateTime.Now)
                .Where(item => item.PackageId != currentPackageId && item.AutoGrantOnRegister)
                .ExecuteCommandAsync();
        }

        private static string NormalizePackageId(string? packageId)
        {
            return (packageId ?? string.Empty).Trim();
        }
    }
}
#pragma warning restore CS1591
