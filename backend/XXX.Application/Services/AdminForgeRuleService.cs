#pragma warning disable CS1591
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    public class AdminForgeRuleService : IAdminForgeRuleService
    {
        private readonly IRepository<ForgeProfessionLevelConfigEntity> _levelRepository;
        private readonly IRepository<ForgeProfessionRuleConfigEntity> _ruleRepository;
        private readonly IAdminRuntimeRefreshService _runtimeRefreshService;

        public AdminForgeRuleService(
            IRepository<ForgeProfessionLevelConfigEntity> levelRepository,
            IRepository<ForgeProfessionRuleConfigEntity> ruleRepository,
            IAdminRuntimeRefreshService runtimeRefreshService)
        {
            _levelRepository = levelRepository;
            _ruleRepository = ruleRepository;
            _runtimeRefreshService = runtimeRefreshService;
        }

        public async Task<List<AdminForgeProfessionLevelRuleDto>> GetLevelRulesAsync()
        {
            var items = await _levelRepository.Db.Queryable<ForgeProfessionLevelConfigEntity>()
                .OrderBy(item => item.Level)
                .ToListAsync();
            return items.Select(MapLevelRule).ToList();
        }

        public async Task<AdminForgeProfessionLevelRuleDto?> GetLevelRuleAsync(int level)
        {
            if (level <= 0)
            {
                return null;
            }

            var entity = await _levelRepository.GetByIdAsync(level);
            return entity == null ? null : MapLevelRule(entity);
        }

        public async Task<AdminForgeProfessionLevelRuleDto> SaveLevelRuleAsync(AdminForgeProfessionLevelRuleDto request)
        {
            if (request.Level <= 0)
            {
                throw new InvalidOperationException("等级必须大于 0。");
            }

            var entity = await _levelRepository.GetByIdAsync(request.Level) ?? new ForgeProfessionLevelConfigEntity { Level = request.Level };
            entity.NextLevelExp = Math.Max(1, request.NextLevelExp);
            entity.IsEnabled = request.IsEnabled;
            entity.IsBuiltIn = false;
            entity.SeedKey = null;
            entity.BuiltInVersion = null;
            entity.LastUpdateTime = DateTime.Now;

            if (await _levelRepository.GetByIdAsync(request.Level) == null)
            {
                await _levelRepository.AddAsync(entity);
            }
            else
            {
                await _levelRepository.UpdateAsync(entity);
            }

            await _runtimeRefreshService.ReloadForgeProfessionRuleCacheAsync();
            return (await GetLevelRuleAsync(request.Level))!;
        }

        public async Task<bool> DeleteLevelRuleAsync(int level)
        {
            if (level <= 0)
            {
                return false;
            }

            var rows = await _levelRepository.DeleteAsync(level);
            if (rows > 0)
            {
                await _runtimeRefreshService.ReloadForgeProfessionRuleCacheAsync();
            }

            return rows > 0;
        }

        public async Task<AdminForgeProfessionRuleDto?> GetRuleAsync()
        {
            var entity = await _ruleRepository.Db.Queryable<ForgeProfessionRuleConfigEntity>()
                .OrderByDescending(item => item.LastUpdateTime)
                .FirstAsync();
            return entity == null ? null : MapRule(entity);
        }

        public async Task<AdminForgeProfessionRuleDto> SaveRuleAsync(AdminForgeProfessionRuleDto request)
        {
            var configId = string.IsNullOrWhiteSpace(request.ConfigId) ? "default" : request.ConfigId.Trim();
            var entity = await _ruleRepository.GetByIdAsync(configId) ?? new ForgeProfessionRuleConfigEntity { ConfigId = configId };
            entity.SuccessBonusPerOverLevel = Math.Max(0, request.SuccessBonusPerOverLevel);
            entity.MaxSuccessBonus = Math.Max(0, request.MaxSuccessBonus);
            entity.SuccessExpBase = Math.Max(0, request.SuccessExpBase);
            entity.SuccessExpPerRequiredLevel = Math.Max(0, request.SuccessExpPerRequiredLevel);
            entity.FailureExpBase = Math.Max(0, request.FailureExpBase);
            entity.FailureExpPerRequiredLevel = Math.Max(0, request.FailureExpPerRequiredLevel);
            entity.IsEnabled = request.IsEnabled;
            entity.IsBuiltIn = false;
            entity.SeedKey = null;
            entity.BuiltInVersion = null;
            entity.LastUpdateTime = DateTime.Now;

            if (await _ruleRepository.GetByIdAsync(configId) == null)
            {
                await _ruleRepository.AddAsync(entity);
            }
            else
            {
                await _ruleRepository.UpdateAsync(entity);
            }

            await _runtimeRefreshService.ReloadForgeProfessionRuleCacheAsync();
            return (await GetRuleAsync())!;
        }

        private static AdminForgeProfessionLevelRuleDto MapLevelRule(ForgeProfessionLevelConfigEntity entity)
        {
            return new AdminForgeProfessionLevelRuleDto
            {
                Level = entity.Level,
                NextLevelExp = entity.NextLevelExp,
                IsEnabled = entity.IsEnabled,
                IsBuiltIn = entity.IsBuiltIn,
                BuiltInVersion = entity.BuiltInVersion,
                SeedKey = entity.SeedKey,
                LastUpdateTime = entity.LastUpdateTime
            };
        }

        private static AdminForgeProfessionRuleDto MapRule(ForgeProfessionRuleConfigEntity entity)
        {
            return new AdminForgeProfessionRuleDto
            {
                ConfigId = entity.ConfigId,
                SuccessBonusPerOverLevel = entity.SuccessBonusPerOverLevel,
                MaxSuccessBonus = entity.MaxSuccessBonus,
                SuccessExpBase = entity.SuccessExpBase,
                SuccessExpPerRequiredLevel = entity.SuccessExpPerRequiredLevel,
                FailureExpBase = entity.FailureExpBase,
                FailureExpPerRequiredLevel = entity.FailureExpPerRequiredLevel,
                IsEnabled = entity.IsEnabled,
                IsBuiltIn = entity.IsBuiltIn,
                BuiltInVersion = entity.BuiltInVersion,
                SeedKey = entity.SeedKey,
                LastUpdateTime = entity.LastUpdateTime
            };
        }
    }
}
#pragma warning restore CS1591
