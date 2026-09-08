using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Achievement;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Repositories;
using XXX.WebApi.Extensions;

namespace XXX.WebApi.Controllers
{
    /// <summary>
    /// 成就系统接口。
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AchievementController : ControllerBase
    {
        private readonly IAchievementService _achievementService;
        private readonly IRepository<UserEntity> _userRepository;
        private readonly IRepository<AlchemyRecipeEntity> _alchemyRecipeRepository;
        private readonly IRepository<ForgeRecipeEntity> _forgeRecipeRepository;
        private readonly IRepository<CropTemplateEntity> _cropTemplateRepository;

        /// <summary>
        /// 初始化成就控制器。
        /// </summary>
        public AchievementController(
            IAchievementService achievementService,
            IRepository<UserEntity> userRepository,
            IRepository<AlchemyRecipeEntity> alchemyRecipeRepository,
            IRepository<ForgeRecipeEntity> forgeRecipeRepository,
            IRepository<CropTemplateEntity> cropTemplateRepository)
        {
            _achievementService = achievementService;
            _userRepository = userRepository;
            _alchemyRecipeRepository = alchemyRecipeRepository;
            _forgeRecipeRepository = forgeRecipeRepository;
            _cropTemplateRepository = cropTemplateRepository;
        }

        /// <summary>
        /// 从当前登录上下文中读取玩家编号。
        /// </summary>
        private string? GetPlayerId() => User.GetCurrentPlayerId();

        /// <summary>
        /// 获取全部成就配置。
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<AchievementConfig>>>> GetAchievements()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var achievements = await _achievementService.GetAllAchievementsAsync();
            return Ok(ApiResponse<List<AchievementConfig>>.Ok(achievements));
        }

        /// <summary>
        /// 获取玩家视角下的成就总览。
        /// </summary>
        [HttpGet("overview")]
        public async Task<ActionResult<ApiResponse<List<AchievementDto>>>> GetOverview()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var achievements = await _achievementService.GetAllAchievementsAsync();
            var progressList = await _achievementService.GetPlayerAchievementsAsync(playerId);
            var progressLookup = progressList.ToDictionary(progress => progress.AchievementId, progress => progress);
            var displayLookup = await BuildDisplayLookupAsync(achievements);

            var overview = achievements
                .OrderBy(achievement => achievement.SortOrder)
                .Select(achievement => MapAchievementToDto(
                    achievement,
                    progressLookup.TryGetValue(achievement.AchievementId, out var progress) ? progress : null,
                    displayLookup))
                .ToList();

            return Ok(ApiResponse<List<AchievementDto>>.Ok(overview));
        }

        /// <summary>
        /// 获取当前玩家的成就进度明细。
        /// </summary>
        [HttpGet("progress")]
        public async Task<ActionResult<ApiResponse<List<AchievementProgress>>>> GetMyProgress()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var progress = await _achievementService.GetPlayerAchievementsAsync(playerId);
            return Ok(ApiResponse<List<AchievementProgress>>.Ok(progress));
        }

        /// <summary>
        /// 获取成就统计信息。
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<ApiResponse<AchievementStats>>> GetStats()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var stats = await _achievementService.GetAchievementStatsAsync(playerId);
            return Ok(ApiResponse<AchievementStats>.Ok(stats));
        }

        /// <summary>
        /// 领取指定成就的奖励。
        /// </summary>
        [HttpPost("{achievementId}/claim")]
        public async Task<ActionResult<ApiResponse<AchievementClaimResult>>> ClaimReward(string achievementId)
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var player = await _userRepository.GetByIdAsync(playerId);
            if (player == null)
            {
                return NotFound(ApiResponse.Fail("玩家不存在"));
            }

            var result = await _achievementService.ClaimRewardAsync(player, achievementId);
            if (!result.Success)
            {
                return BadRequest(ApiResponse<AchievementClaimResult>.Fail(result.Message));
            }

            return Ok(ApiResponse<AchievementClaimResult>.Ok(result, result.Message));
        }

        /// <summary>
        /// 将成就配置和玩家进度映射成前端总览 DTO。
        /// </summary>
        private AchievementDto MapAchievementToDto(
            AchievementConfig config,
            AchievementProgress? progress,
            AchievementDisplayLookup displayLookup)
        {
            var effectiveProgress = progress ?? new AchievementProgress
            {
                AchievementId = config.AchievementId,
                Status = AchievementStatus.NotStarted
            };

            var requirements = config.Requirements
                .Select((requirement, index) =>
                {
                    var currentProgress = Math.Max(0L, effectiveProgress.GetProgressValue(index));
                    var targetProgress = Math.Max(1L, requirement.TargetValue);
                    var progressPercent = Math.Min(100d, currentProgress * 100d / targetProgress);

                    return new AchievementRequirementDto
                    {
                        Index = index,
                        RequirementType = requirement.RequirementType.ToString(),
                        RequirementTypeValue = (int)requirement.RequirementType,
                        CurrentProgress = currentProgress,
                        TargetProgress = requirement.TargetValue,
                        ProgressPercent = progressPercent,
                        IsCompleted = currentProgress >= requirement.TargetValue,
                        Description = ResolveRequirementDescription(requirement, displayLookup)
                    };
                })
                .ToList();

            var currentProgressTotal = requirements.Sum(requirement => Math.Min(requirement.CurrentProgress, Math.Max(0L, requirement.TargetProgress)));
            var targetProgressTotal = requirements.Sum(requirement => Math.Max(0L, requirement.TargetProgress));

            return new AchievementDto
            {
                AchievementId = config.AchievementId,
                Name = config.AchievementName,
                Description = config.Description,
                Type = config.AchievementType.ToString(),
                TypeValue = (int)config.AchievementType,
                Icon = config.Icon,
                Difficulty = config.Difficulty.ToString(),
                DifficultyValue = (int)config.Difficulty,
                Category = config.Category,
                Points = config.Points,
                CurrentProgress = currentProgressTotal > int.MaxValue ? int.MaxValue : (int)currentProgressTotal,
                TargetProgress = targetProgressTotal > int.MaxValue ? int.MaxValue : (int)targetProgressTotal,
                IsCompleted = effectiveProgress.IsCompleted(),
                IsRewardClaimed = effectiveProgress.IsClaimed(),
                Status = effectiveProgress.Status.ToString(),
                StatusValue = (int)effectiveProgress.Status,
                IsHidden = config.IsHidden,
                SortOrder = config.SortOrder,
                CompleteTime = effectiveProgress.CompleteTime,
                ClaimTime = effectiveProgress.ClaimTime,
                Requirements = requirements,
                Rewards = MapRewards(config.Rewards, displayLookup)
            };
        }

        /// <summary>
        /// 将成就奖励映射成统一展示结构。
        /// </summary>
        private static List<AchievementRewardDto> MapRewards(AchievementReward reward, AchievementDisplayLookup displayLookup)
        {
            var rewards = new List<AchievementRewardDto>();

            void AddReward(string rewardType, string name, long amount)
            {
                if (amount <= 0)
                {
                    return;
                }

                rewards.Add(new AchievementRewardDto
                {
                    RewardType = rewardType,
                    Name = name,
                    Amount = amount
                });
            }

            if (!string.IsNullOrWhiteSpace(reward.Title))
            {
                rewards.Add(new AchievementRewardDto
                {
                    RewardType = "Title",
                    Name = "称号",
                    Amount = 1,
                    ItemId = reward.Title
                });
            }

            AddReward("Exp", "修为", reward.Exp);
            AddReward("Gold", "金币", reward.Gold);
            AddReward("SpiritStone", "灵石", reward.SpiritStone);
            AddReward("Honor", "荣誉", reward.Honor);
            AddReward("GuildContribution", "宗门贡献", reward.GuildContribution);

            foreach (var itemReward in reward.Items)
            {
                rewards.Add(new AchievementRewardDto
                {
                    RewardType = "Item",
                    ItemId = itemReward.Key,
                    Name = ResolveItemName(itemReward.Key, displayLookup),
                    Amount = itemReward.Value
                });
            }

            foreach (var equipmentId in reward.EquipmentIds)
            {
                rewards.Add(new AchievementRewardDto
                {
                    RewardType = "Equipment",
                    EquipmentId = equipmentId,
                    Name = ResolveEquipmentName(equipmentId, displayLookup),
                    Amount = 1
                });
            }

            foreach (var buffId in reward.Buffs)
            {
                rewards.Add(new AchievementRewardDto
                {
                    RewardType = "Buff",
                    ItemId = buffId,
                    Name = buffId,
                    Amount = 1
                });
            }

            return rewards;
        }

        private async Task<AchievementDisplayLookup> BuildDisplayLookupAsync(IEnumerable<AchievementConfig> achievements)
        {
            var lookup = new AchievementDisplayLookup();
            foreach (var item in XXX.GameData.Items)
            {
                lookup.ItemNames[item.Key] = item.Value.Name;
            }

            foreach (var item in XXX.GameData.EquipmentTemplates)
            {
                lookup.EquipmentNames[item.Key] = item.Value.Name;
            }

            foreach (var item in XXX.GameData.MonsterTemplates)
            {
                lookup.MonsterNames[item.Key] = item.Value.Name;
            }

            foreach (var item in XXX.Dungeon.DungeonCatalog.GetLegacyEntries())
            {
                lookup.DungeonNames[item.DungeonId] = item.Name;
            }

            foreach (var skill in XXX.SkillData.Skills.Values)
            {
                lookup.SkillNames[skill.Id.ToString()] = skill.Name;
            }

            var alchemyRecipeIds = achievements
                .SelectMany(achievement => achievement.Requirements)
                .Where(requirement => requirement.RequirementType == AchievementRequirementType.AlchemySuccessCount &&
                    !string.IsNullOrWhiteSpace(requirement.TargetId))
                .Select(requirement => requirement.TargetId.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (alchemyRecipeIds.Count > 0)
            {
                var recipes = await _alchemyRecipeRepository.Db.Queryable<AlchemyRecipeEntity>()
                    .Where(entity => alchemyRecipeIds.Contains(entity.RecipeId))
                    .ToListAsync();
                foreach (var recipe in recipes)
                {
                    lookup.AlchemyRecipeNames[recipe.RecipeId] = recipe.Name;
                }
            }

            var forgeRecipeIds = achievements
                .SelectMany(achievement => achievement.Requirements)
                .Where(requirement => requirement.RequirementType == AchievementRequirementType.ForgeSuccessCount &&
                    !string.IsNullOrWhiteSpace(requirement.TargetId))
                .Select(requirement => requirement.TargetId.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (forgeRecipeIds.Count > 0)
            {
                var recipes = await _forgeRecipeRepository.Db.Queryable<ForgeRecipeEntity>()
                    .Where(entity => forgeRecipeIds.Contains(entity.RecipeId))
                    .ToListAsync();
                foreach (var recipe in recipes)
                {
                    lookup.ForgeRecipeNames[recipe.RecipeId] = recipe.Name;
                }
            }

            var cropIds = achievements
                .SelectMany(achievement => achievement.Requirements)
                .Where(requirement =>
                    (requirement.RequirementType == AchievementRequirementType.PlantCount ||
                     requirement.RequirementType == AchievementRequirementType.HarvestCount) &&
                    !string.IsNullOrWhiteSpace(requirement.TargetId))
                .Select(requirement => requirement.TargetId.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (cropIds.Count > 0)
            {
                var crops = await _cropTemplateRepository.Db.Queryable<CropTemplateEntity>()
                    .Where(entity => cropIds.Contains(entity.TemplateId))
                    .ToListAsync();
                foreach (var crop in crops)
                {
                    lookup.CropNames[crop.TemplateId] = crop.Name;
                }
            }

            return lookup;
        }

        private static string ResolveRequirementDescription(AchievementRequirement requirement, AchievementDisplayLookup lookup)
        {
            if (!string.IsNullOrWhiteSpace(requirement.Description))
            {
                return requirement.Description;
            }

            var targetValue = Math.Max(0L, requirement.TargetValue);
            var targetId = requirement.TargetId?.Trim() ?? string.Empty;

            return requirement.RequirementType switch
            {
                AchievementRequirementType.ReachLevel => $"达到 {targetValue} 级",
                AchievementRequirementType.TotalKills => $"累计击杀 {targetValue} 次",
                AchievementRequirementType.TotalWins => $"累计获胜 {targetValue} 场",
                AchievementRequirementType.MaxEnhanceLevel => $"将任意装备强化到 +{targetValue}",
                AchievementRequirementType.ItemCollection => $"累计获得道具 {ResolveItemName(targetId, lookup)} x{targetValue}",
                AchievementRequirementType.TotalGoldEarned => $"累计获得 {targetValue} 金币",
                AchievementRequirementType.TotalDamageDealt => $"累计造成 {targetValue} 点伤害",
                AchievementRequirementType.TotalDamageTaken => $"累计承受 {targetValue} 点伤害",
                AchievementRequirementType.AlchemySuccessCount => string.IsNullOrWhiteSpace(targetId)
                    ? $"累计炼丹成功 {targetValue} 次"
                    : $"使用丹方 {ResolveAlchemyRecipeName(targetId, lookup)} 成功炼丹 {targetValue} 次",
                AchievementRequirementType.AlchemyFailureCount => $"累计炼丹失败 {targetValue} 次",
                AchievementRequirementType.ForgeSuccessCount => string.IsNullOrWhiteSpace(targetId)
                    ? $"累计锻造成功 {targetValue} 次"
                    : $"使用图纸 {ResolveForgeRecipeName(targetId, lookup)} 成功锻造 {targetValue} 次",
                AchievementRequirementType.ForgeFailureCount => $"累计锻造失败 {targetValue} 次",
                AchievementRequirementType.EnhanceSuccessCount => $"累计强化成功 {targetValue} 次",
                AchievementRequirementType.EnhanceFailureCount => $"累计强化失败 {targetValue} 次",
                AchievementRequirementType.PlantCount => $"种植 {ResolveCropName(targetId, lookup)} {targetValue} 次",
                AchievementRequirementType.HarvestCount => $"收获 {ResolveCropName(targetId, lookup)} {targetValue} 次",
                AchievementRequirementType.DungeonWinCount => $"在 {ResolveDungeonName(targetId, lookup)} 中获胜 {targetValue} 次",
                AchievementRequirementType.MonsterKillCount => $"击杀 {ResolveMonsterName(targetId, lookup)} {targetValue} 次",
                AchievementRequirementType.SkillUseCount => $"使用技能 {ResolveSkillName(targetId, lookup)} {targetValue} 次",
                AchievementRequirementType.TotalMpConsumed => $"累计消耗 {targetValue} 点蓝量",
                AchievementRequirementType.CriticalHitCount => $"累计触发 {targetValue} 次暴击",
                AchievementRequirementType.DeathCount => $"累计死亡 {targetValue} 次",
                AchievementRequirementType.DodgeCount => $"累计闪避 {targetValue} 次",
                AchievementRequirementType.SkillDamageDealt => $"使用技能 {ResolveSkillName(targetId, lookup)} 累计造成 {targetValue} 点伤害",
                _ => "完成对应成就条件"
            };
        }

        private static string ResolveItemName(string itemId, AchievementDisplayLookup lookup)
        {
            return lookup.ItemNames.TryGetValue(itemId, out var name) ? name : itemId;
        }

        private static string ResolveEquipmentName(int equipmentId, AchievementDisplayLookup lookup)
        {
            return lookup.EquipmentNames.TryGetValue(equipmentId, out var name) ? name : $"装备 {equipmentId}";
        }

        private static string ResolveMonsterName(string monsterId, AchievementDisplayLookup lookup)
        {
            return lookup.MonsterNames.TryGetValue(monsterId, out var name) ? name : monsterId;
        }

        private static string ResolveDungeonName(string dungeonId, AchievementDisplayLookup lookup)
        {
            return lookup.DungeonNames.TryGetValue(dungeonId, out var name) ? name : dungeonId;
        }

        private static string ResolveSkillName(string skillId, AchievementDisplayLookup lookup)
        {
            return lookup.SkillNames.TryGetValue(skillId, out var name) ? name : skillId;
        }

        private static string ResolveCropName(string cropId, AchievementDisplayLookup lookup)
        {
            return lookup.CropNames.TryGetValue(cropId, out var name) ? name : cropId;
        }

        private static string ResolveAlchemyRecipeName(string recipeId, AchievementDisplayLookup lookup)
        {
            return lookup.AlchemyRecipeNames.TryGetValue(recipeId, out var name) ? name : recipeId;
        }

        private static string ResolveForgeRecipeName(string recipeId, AchievementDisplayLookup lookup)
        {
            return lookup.ForgeRecipeNames.TryGetValue(recipeId, out var name) ? name : recipeId;
        }

        private sealed class AchievementDisplayLookup
        {
            public Dictionary<string, string> ItemNames { get; } = new(StringComparer.OrdinalIgnoreCase);

            public Dictionary<int, string> EquipmentNames { get; } = new();

            public Dictionary<string, string> MonsterNames { get; } = new(StringComparer.OrdinalIgnoreCase);

            public Dictionary<string, string> DungeonNames { get; } = new(StringComparer.OrdinalIgnoreCase);

            public Dictionary<string, string> SkillNames { get; } = new(StringComparer.OrdinalIgnoreCase);

            public Dictionary<string, string> CropNames { get; } = new(StringComparer.OrdinalIgnoreCase);

            public Dictionary<string, string> AlchemyRecipeNames { get; } = new(StringComparer.OrdinalIgnoreCase);

            public Dictionary<string, string> ForgeRecipeNames { get; } = new(StringComparer.OrdinalIgnoreCase);
        }
    }
}
