#pragma warning disable CS1591
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Repositories;
using XXX.Player;

namespace XXX.Application.Services
{
    public class AdminRealmLevelConfigService : IAdminRealmLevelConfigService
    {
        private readonly IRepository<RealmLevelConfigEntity> _repository;
        private readonly IAdminRuntimeRefreshService _runtimeRefreshService;

        public AdminRealmLevelConfigService(
            IRepository<RealmLevelConfigEntity> repository,
            IAdminRuntimeRefreshService runtimeRefreshService)
        {
            _repository = repository;
            _runtimeRefreshService = runtimeRefreshService;
        }

        public async Task<List<AdminRealmLevelConfigListItemDto>> GetListAsync()
        {
            var entities = await _repository.Db.Queryable<RealmLevelConfigEntity>()
                .OrderBy(item => item.Level)
                .ToListAsync();

            if (entities.Count == 0)
            {
                throw new InvalidOperationException("Realm level configs are missing. Please run startup seed sync before using growth admin pages.");
            }

            return entities.Select(item => new AdminRealmLevelConfigListItemDto
            {
                Level = item.Level,
                RealmName = item.RealmName,
                Alias = item.Alias,
                RealmOrder = item.RealmOrder,
                Layer = item.Layer,
                RequiredExp = item.RequiredExp,
                AttributeBonusPercent = item.AttributeBonusPercent,
                IsBreakthroughPoint = item.IsBreakthroughPoint,
                IsBuiltIn = item.IsBuiltIn,
                BuiltInVersion = item.BuiltInVersion
            }).ToList();
        }

        public async Task<AdminRealmLevelConfigDetailDto?> GetDetailAsync(int level)
        {
            if (!LevelConfig.IsValidLevel(level))
            {
                return null;
            }

            var entity = await _repository.GetByIdAsync(level);
            if (entity == null)
            {
                throw new InvalidOperationException($"Realm level config {level} is missing. Please run startup seed sync before using growth admin pages.");
            }

            return MapDetail(entity);
        }

        public async Task<AdminRealmLevelConfigDetailDto> SaveAsync(AdminRealmLevelConfigDetailDto request)
        {
            ValidateRequest(request);

            var entity = await _repository.GetByIdAsync(request.Level) ?? new RealmLevelConfigEntity { Level = request.Level };
            entity.RealmName = request.RealmName.Trim();
            entity.Alias = request.Alias.Trim();
            entity.RealmOrder = Math.Max(1, request.RealmOrder);
            entity.Layer = Math.Max(1, request.Layer);
            entity.RequiredExp = Math.Max(0L, request.RequiredExp);
            entity.AttributeBonusPercent = Math.Max(0, request.AttributeBonusPercent);
            entity.IsBreakthroughPoint = request.IsBreakthroughPoint && request.Level < LevelConfig.MaxLevel;
            entity.BreakthroughSuccessRate = entity.IsBreakthroughPoint ? Math.Clamp(request.BreakthroughSuccessRate, 1, 100) : 100;
            entity.BreakthroughExpLossPercent = entity.IsBreakthroughPoint ? Math.Clamp(request.BreakthroughExpLossPercent, 0, 100) : 0;
            entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            entity.BreakthroughMaterials = entity.IsBreakthroughPoint
                ? request.BreakthroughMaterials
                    .Where(item => !string.IsNullOrWhiteSpace(item.ItemId) && item.Count > 0)
                    .Select(item => new RealmBreakthroughMaterial
                    {
                        ItemId = item.ItemId.Trim(),
                        Count = item.Count
                    })
                    .ToList()
                : [];
            entity.IsBuiltIn = false;
            entity.SeedKey = null;
            entity.BuiltInVersion = null;
            entity.LastUpdateTime = DateTime.Now;

            if (await _repository.GetByIdAsync(request.Level) == null)
            {
                await _repository.AddAsync(entity);
            }
            else
            {
                await _repository.UpdateAsync(entity);
            }

            await _runtimeRefreshService.ReloadGrowthConfigCacheAsync();
            return (await GetDetailAsync(request.Level))!;
        }

        private static void ValidateRequest(AdminRealmLevelConfigDetailDto request)
        {
            if (!LevelConfig.IsValidLevel(request.Level))
            {
                throw new InvalidOperationException("等级超出允许范围。");
            }

            if (string.IsNullOrWhiteSpace(request.RealmName))
            {
                throw new InvalidOperationException("境界名称不能为空。");
            }

            if (string.IsNullOrWhiteSpace(request.Alias))
            {
                throw new InvalidOperationException("境界别名不能为空。");
            }

            if (request.RealmOrder <= 0)
            {
                throw new InvalidOperationException("境界排序必须大于 0。");
            }

            if (request.Layer <= 0)
            {
                throw new InvalidOperationException("境界层数必须大于 0。");
            }

            if (request.RequiredExp < 0)
            {
                throw new InvalidOperationException("升级经验不能小于 0。");
            }

            if (request.AttributeBonusPercent < 0)
            {
                throw new InvalidOperationException("属性加成不能小于 0。");
            }

            if (request.IsBreakthroughPoint)
            {
                if (request.Level >= LevelConfig.MaxLevel)
                {
                    throw new InvalidOperationException("最高等级不能配置为突破关口。");
                }

                if (request.BreakthroughSuccessRate is < 1 or > 100)
                {
                    throw new InvalidOperationException("突破成功率必须在 1 到 100 之间。");
                }

                if (request.BreakthroughExpLossPercent is < 0 or > 100)
                {
                    throw new InvalidOperationException("突破经验损失比例必须在 0 到 100 之间。");
                }

                var validMaterials = request.BreakthroughMaterials
                    .Where(item => !string.IsNullOrWhiteSpace(item.ItemId) && item.Count > 0)
                    .ToList();
                if (validMaterials.Count == 0)
                {
                    throw new InvalidOperationException("突破关口至少需要一条有效材料。");
                }

                var duplicateMaterial = validMaterials
                    .GroupBy(item => item.ItemId.Trim(), StringComparer.OrdinalIgnoreCase)
                    .FirstOrDefault(group => group.Count() > 1);
                if (duplicateMaterial != null)
                {
                    throw new InvalidOperationException($"突破材料重复：{duplicateMaterial.Key}。");
                }
            }
        }

        private static AdminRealmLevelConfigDetailDto MapDetail(RealmLevelConfigEntity entity)
        {
            return new AdminRealmLevelConfigDetailDto
            {
                Level = entity.Level,
                RealmName = entity.RealmName,
                Alias = entity.Alias,
                RealmOrder = entity.RealmOrder,
                Layer = entity.Layer,
                RequiredExp = entity.RequiredExp,
                AttributeBonusPercent = entity.AttributeBonusPercent,
                IsBreakthroughPoint = entity.IsBreakthroughPoint,
                BreakthroughSuccessRate = entity.BreakthroughSuccessRate,
                BreakthroughExpLossPercent = entity.BreakthroughExpLossPercent,
                Description = entity.Description,
                IsBuiltIn = entity.IsBuiltIn,
                SeedKey = entity.SeedKey,
                BuiltInVersion = entity.BuiltInVersion,
                LastUpdateTime = entity.LastUpdateTime,
                BreakthroughMaterials = entity.BreakthroughMaterials
                    .Select(item => new AdminRealmBreakthroughMaterialDto
                    {
                        ItemId = item.ItemId,
                        Count = item.Count
                    })
                    .ToList()
            };
        }
    }
}
#pragma warning restore CS1591
