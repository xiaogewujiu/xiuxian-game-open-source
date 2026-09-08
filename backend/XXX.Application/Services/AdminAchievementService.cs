#pragma warning disable CS1591
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    public class AdminAchievementService : IAdminAchievementService
    {
        private readonly IRepository<AchievementConfigEntity> _achievementRepository;
        private readonly IRepository<AchievementProgressEntity> _achievementProgressRepository;
        private readonly IAdminRuntimeRefreshService _runtimeRefreshService;

        public AdminAchievementService(
            IRepository<AchievementConfigEntity> achievementRepository,
            IRepository<AchievementProgressEntity> achievementProgressRepository,
            IAdminRuntimeRefreshService runtimeRefreshService)
        {
            _achievementRepository = achievementRepository;
            _achievementProgressRepository = achievementProgressRepository;
            _runtimeRefreshService = runtimeRefreshService;
        }

        public async Task<List<AdminAchievementListItemDto>> GetListAsync(string? keyword = null, int? difficulty = null)
        {
            var normalizedKeyword = (keyword ?? string.Empty).Trim();
            var query = _achievementRepository.Db.Queryable<AchievementConfigEntity>();
            if (!string.IsNullOrWhiteSpace(normalizedKeyword))
            {
                query = query.Where(achievement => achievement.AchievementId.Contains(normalizedKeyword) || achievement.AchievementName.Contains(normalizedKeyword));
            }

            query = query.WhereIF(difficulty.HasValue, achievement => achievement.Difficulty == difficulty!.Value);

            var achievements = await query.OrderBy(achievement => achievement.SortOrder).ToListAsync();
            return achievements.Select(achievement => new AdminAchievementListItemDto
            {
                AchievementId = achievement.AchievementId,
                AchievementName = achievement.AchievementName,
                AchievementType = achievement.AchievementType,
                Difficulty = achievement.Difficulty,
                IsEnabled = achievement.IsEnabled,
                IsBuiltIn = achievement.IsBuiltIn,
                BuiltInVersion = achievement.BuiltInVersion
            }).ToList();
        }

        public async Task<AdminAchievementDetailDto?> GetDetailAsync(string achievementId)
        {
            if (string.IsNullOrWhiteSpace(achievementId)) return null;
            var achievement = await _achievementRepository.GetByIdAsync(achievementId.Trim());
            if (achievement == null) return null;
            return new AdminAchievementDetailDto
            {
                AchievementId = achievement.AchievementId,
                AchievementName = achievement.AchievementName,
                AchievementType = achievement.AchievementType,
                Difficulty = achievement.Difficulty,
                Description = achievement.Description,
                Category = achievement.Category,
                Points = achievement.Points,
                IsHidden = achievement.IsHidden,
                PreAchievementIds = achievement.PreAchievementIds,
                RewardGold = achievement.RewardGold,
                RewardSpiritStone = achievement.RewardSpiritStone,
                RewardExp = achievement.RewardExp,
                RewardTitle = achievement.RewardTitle,
                RewardItemsJson = achievement.RewardItemsJson,
                RewardEquipmentIds = achievement.RewardEquipmentIds,
                RequirementType = achievement.RequirementType,
                RequirementTargetValue = achievement.RequirementTargetValue,
                RequirementDescription = achievement.RequirementDescription,
                RequirementsJson = achievement.RequirementsJson,
                SortOrder = achievement.SortOrder,
                IsEnabled = achievement.IsEnabled,
                IsBuiltIn = achievement.IsBuiltIn,
                SeedKey = achievement.SeedKey,
                BuiltInVersion = achievement.BuiltInVersion,
                LastUpdateTime = achievement.LastUpdateTime
            };
        }

        public async Task<AdminAchievementDetailDto> SaveAsync(AdminAchievementDetailDto request)
        {
            var achievementId = (request.AchievementId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(achievementId)) throw new InvalidOperationException("成就编号不能为空。");
            if (string.IsNullOrWhiteSpace(request.AchievementName)) throw new InvalidOperationException("成就名称不能为空。");

            var existing = await _achievementRepository.GetByIdAsync(achievementId);
            if (existing == null)
            {
                existing = new AchievementConfigEntity { AchievementId = achievementId };
                await _achievementRepository.AddAsync(ApplyAchievement(existing, request));
                await _runtimeRefreshService.ReloadAchievementCacheAsync();
                return request;
            }

            ApplyAchievement(existing, request);
            await _achievementRepository.UpdateAsync(existing);
            await _runtimeRefreshService.ReloadAchievementCacheAsync();
            return request;
        }

        public async Task<bool> DeleteAsync(string achievementId)
        {
            var normalizedAchievementId = (achievementId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedAchievementId)) return false;

            var allAchievements = await _achievementRepository.Db.Queryable<AchievementConfigEntity>().ToListAsync();
            var referencedAchievement = allAchievements.FirstOrDefault(achievement => !string.Equals(achievement.AchievementId, normalizedAchievementId, StringComparison.OrdinalIgnoreCase) && (achievement.PreAchievementIds ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries).Select(item => item.Trim()).Any(item => string.Equals(item, normalizedAchievementId, StringComparison.OrdinalIgnoreCase)));
            if (referencedAchievement != null) throw new InvalidOperationException($"当前成就仍被成就 {referencedAchievement.AchievementId} 作为前置成就引用，不能直接删除。");

            if (await _achievementProgressRepository.Db.Queryable<AchievementProgressEntity>().Where(progress => progress.AchievementId == normalizedAchievementId).AnyAsync())
            {
                throw new InvalidOperationException("当前成就仍被玩家成就进度引用，不能直接删除。");
            }

            var deleteRows = await _achievementRepository.DeleteAsync(normalizedAchievementId);
            if (deleteRows > 0)
            {
                await _runtimeRefreshService.ReloadAchievementCacheAsync();
            }

            return deleteRows > 0;
        }

        private static AchievementConfigEntity ApplyAchievement(AchievementConfigEntity entity, AdminAchievementDetailDto request)
        {
            entity.AchievementName = request.AchievementName.Trim();
            entity.AchievementType = request.AchievementType;
            entity.Difficulty = request.Difficulty;
            entity.Description = (request.Description ?? string.Empty).Trim();
            entity.Category = (request.Category ?? string.Empty).Trim();
            entity.Points = Math.Max(0, request.Points);
            entity.IsHidden = request.IsHidden;
            entity.PreAchievementIds = string.IsNullOrWhiteSpace(request.PreAchievementIds) ? null : request.PreAchievementIds.Trim();
            entity.RewardGold = Math.Max(0, request.RewardGold);
            entity.RewardSpiritStone = Math.Max(0L, request.RewardSpiritStone);
            entity.RewardExp = Math.Max(0, request.RewardExp);
            entity.RewardTitle = string.IsNullOrWhiteSpace(request.RewardTitle) ? null : request.RewardTitle.Trim();
            entity.RewardItemsJson = string.IsNullOrWhiteSpace(request.RewardItemsJson) ? null : request.RewardItemsJson.Trim();
            entity.RewardEquipmentIds = string.IsNullOrWhiteSpace(request.RewardEquipmentIds) ? null : request.RewardEquipmentIds.Trim();
            entity.RequirementType = request.RequirementType;
            entity.RequirementTargetValue = Math.Max(0, request.RequirementTargetValue);
            entity.RequirementDescription = string.IsNullOrWhiteSpace(request.RequirementDescription) ? null : request.RequirementDescription.Trim();
            entity.RequirementsJson = string.IsNullOrWhiteSpace(request.RequirementsJson) ? null : request.RequirementsJson.Trim();
            entity.SortOrder = request.SortOrder;
            entity.IsEnabled = request.IsEnabled;
            entity.SeedKey = null;
            entity.IsBuiltIn = false;
            entity.BuiltInVersion = null;
            entity.LastUpdateTime = DateTime.Now;
            return entity;
        }
    }
}
#pragma warning restore CS1591
