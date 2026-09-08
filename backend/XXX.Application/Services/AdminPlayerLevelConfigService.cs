#pragma warning disable CS1591
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Repositories;
using XXX.Player;

namespace XXX.Application.Services
{
    public class AdminPlayerLevelConfigService : IAdminPlayerLevelConfigService
    {
        private readonly IRepository<PlayerLevelConfigEntity> _levelRepository;
        private readonly IRepository<RealmLevelConfigEntity> _realmRepository;
        private readonly IAdminRuntimeRefreshService _runtimeRefreshService;

        public AdminPlayerLevelConfigService(
            IRepository<PlayerLevelConfigEntity> levelRepository,
            IRepository<RealmLevelConfigEntity> realmRepository,
            IAdminRuntimeRefreshService runtimeRefreshService)
        {
            _levelRepository = levelRepository;
            _realmRepository = realmRepository;
            _runtimeRefreshService = runtimeRefreshService;
        }

        public async Task<List<AdminPlayerLevelConfigListItemDto>> GetListAsync()
        {
            var entities = await _levelRepository.Db.Queryable<PlayerLevelConfigEntity>()
                .OrderBy(item => item.Level)
                .ToListAsync();

            if (entities.Count == 0)
            {
                throw new InvalidOperationException("Player level configs are missing. Please run startup seed sync before using growth admin pages.");
            }

            return entities.Select(MapListItem).ToList();
        }

        public async Task<AdminPlayerLevelConfigDetailDto?> GetDetailAsync(int level)
        {
            if (!LevelConfig.IsValidLevel(level))
            {
                return null;
            }

            var entity = await _levelRepository.GetByIdAsync(level);
            if (entity == null)
            {
                throw new InvalidOperationException($"Player level config {level} is missing. Please run startup seed sync before using growth admin pages.");
            }

            return MapDetail(entity);
        }

        public async Task<AdminPlayerLevelConfigDetailDto> SaveAsync(AdminPlayerLevelConfigDetailDto request)
        {
            ValidateRequest(request);

            var now = DateTime.Now;
            var entity = await _levelRepository.GetByIdAsync(request.Level) ?? new PlayerLevelConfigEntity { Level = request.Level };
            entity.RequiredExp = request.Level >= LevelConfig.MaxLevel ? 0 : Math.Max(0L, request.RequiredExp);
            entity.BaseHp = Math.Max(1, request.BaseHp);
            entity.BaseMp = Math.Max(0, request.BaseMp);
            entity.BasePhysicalAttack = Math.Max(0, request.BasePhysicalAttack);
            entity.BaseMagicAttack = Math.Max(0, request.BaseMagicAttack);
            entity.BasePhysicalDefense = Math.Max(0, request.BasePhysicalDefense);
            entity.BaseMagicDefense = Math.Max(0, request.BaseMagicDefense);
            entity.BaseSpeed = Math.Max(0, request.BaseSpeed);
            entity.IsBuiltIn = false;
            entity.SeedKey = null;
            entity.BuiltInVersion = null;
            entity.LastUpdateTime = now;

            try
            {
                _levelRepository.Db.Ado.BeginTran();

                if (await _levelRepository.GetByIdAsync(request.Level) == null)
                {
                    await _levelRepository.AddAsync(entity);
                }
                else
                {
                    await _levelRepository.UpdateAsync(entity);
                }

                var realmConfig = await _realmRepository.GetByIdAsync(request.Level);
                if (realmConfig != null && realmConfig.RequiredExp != entity.RequiredExp)
                {
                    realmConfig.RequiredExp = entity.RequiredExp;
                    realmConfig.IsBuiltIn = false;
                    realmConfig.SeedKey = null;
                    realmConfig.BuiltInVersion = null;
                    realmConfig.LastUpdateTime = now;
                    await _realmRepository.UpdateAsync(realmConfig);
                }

                _levelRepository.Db.Ado.CommitTran();
            }
            catch
            {
                _levelRepository.Db.Ado.RollbackTran();
                throw;
            }

            await _runtimeRefreshService.ReloadGrowthConfigCacheAsync();
            return (await GetDetailAsync(request.Level))!;
        }

        private static void ValidateRequest(AdminPlayerLevelConfigDetailDto request)
        {
            if (!LevelConfig.IsValidLevel(request.Level))
            {
                throw new InvalidOperationException("等级超出允许范围。");
            }

            if (request.Level < LevelConfig.MaxLevel && request.RequiredExp < 0)
            {
                throw new InvalidOperationException("升级经验不能小于 0。");
            }

            if (request.BaseHp <= 0)
            {
                throw new InvalidOperationException("基础血量必须大于 0。");
            }

            if (request.BaseMp < 0 ||
                request.BasePhysicalAttack < 0 ||
                request.BaseMagicAttack < 0 ||
                request.BasePhysicalDefense < 0 ||
                request.BaseMagicDefense < 0 ||
                request.BaseSpeed < 0)
            {
                throw new InvalidOperationException("基础属性不能小于 0。");
            }
        }

        private static AdminPlayerLevelConfigListItemDto MapListItem(PlayerLevelConfigEntity entity)
        {
            return new AdminPlayerLevelConfigListItemDto
            {
                Level = entity.Level,
                RequiredExp = entity.RequiredExp,
                BaseHp = entity.BaseHp,
                BaseMp = entity.BaseMp,
                BasePhysicalAttack = entity.BasePhysicalAttack,
                BaseMagicAttack = entity.BaseMagicAttack,
                BasePhysicalDefense = entity.BasePhysicalDefense,
                BaseMagicDefense = entity.BaseMagicDefense,
                BaseSpeed = entity.BaseSpeed,
                IsBuiltIn = entity.IsBuiltIn,
                BuiltInVersion = entity.BuiltInVersion
            };
        }

        private static AdminPlayerLevelConfigDetailDto MapDetail(PlayerLevelConfigEntity entity)
        {
            return new AdminPlayerLevelConfigDetailDto
            {
                Level = entity.Level,
                RequiredExp = entity.RequiredExp,
                BaseHp = entity.BaseHp,
                BaseMp = entity.BaseMp,
                BasePhysicalAttack = entity.BasePhysicalAttack,
                BaseMagicAttack = entity.BaseMagicAttack,
                BasePhysicalDefense = entity.BasePhysicalDefense,
                BaseMagicDefense = entity.BaseMagicDefense,
                BaseSpeed = entity.BaseSpeed,
                IsBuiltIn = entity.IsBuiltIn,
                SeedKey = entity.SeedKey,
                BuiltInVersion = entity.BuiltInVersion,
                LastUpdateTime = entity.LastUpdateTime
            };
        }
    }
}
#pragma warning restore CS1591
