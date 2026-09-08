using Microsoft.Extensions.Logging;
using System.Text.Json;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Data;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    /// <summary>
    /// 炼丹服务实现
    /// </summary>
    public class AlchemyService : IAlchemyService
    {
        private readonly DbContext _dbContext;
        private readonly IRepository<AlchemySystemEntity> _alchemyRepository;
        private readonly IRepository<AlchemyRecipeEntity> _recipeRepository;
        private readonly IRepository<FiveElementArrayEntity> _fiveElementRepository;
        private readonly IInventoryService _inventoryService;
        private readonly IPlayerAttributeService _playerAttributeService;
        private readonly IAchievementService _achievementService;
        private readonly IQuestService _questService;
        private readonly ILogger<AlchemyService> _logger;

        /// <summary>
        /// 初始化炼丹服务。
        /// </summary>
        /// <param name="dbContext">数据库上下文。</param>
        /// <param name="alchemyRepository">炼丹系统仓储。</param>
        /// <param name="recipeRepository">丹方仓储。</param>
        /// <param name="fiveElementRepository">五行阵仓储。</param>
        /// <param name="inventoryService">背包服务。</param>
        /// <param name="playerAttributeService">玩家属性服务。</param>
        /// <param name="achievementService">成就服务。</param>
        /// <param name="questService">任务服务。</param>
        /// <param name="logger">日志记录器。</param>
        public AlchemyService(
            DbContext dbContext,
            IRepository<AlchemySystemEntity> alchemyRepository,
            IRepository<AlchemyRecipeEntity> recipeRepository,
            IRepository<FiveElementArrayEntity> fiveElementRepository,
            IInventoryService inventoryService,
            IPlayerAttributeService playerAttributeService,
            IAchievementService achievementService,
            IQuestService questService,
            ILogger<AlchemyService> logger)
        {
            _dbContext = dbContext;
            _alchemyRepository = alchemyRepository;
            _recipeRepository = recipeRepository;
            _fiveElementRepository = fiveElementRepository;
            _inventoryService = inventoryService;
            _playerAttributeService = playerAttributeService;
            _achievementService = achievementService;
            _questService = questService;
            _logger = logger;
        }

        /// <summary>
        /// 获取玩家炼丹系统总览。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <returns>炼丹炉等级、熟练度与已学丹方等信息。</returns>
        public async Task<AlchemyDto> GetAlchemyInfoAsync(string playerId)
        {
            var (system, arrayLevel) = await PrepareAlchemySystemAsync(playerId);

            return new AlchemyDto
            {
                FurnaceLevel = system.FurnaceLevel,
                AlchemistLevel = system.AlchemistLevel,
                AlchemistExp = system.AlchemistExp,
                CurrentLevelExp = AlchemyProfessionRuleRuntimeCatalog.GetCurrentLevelProgressExp(system.AlchemistExp, system.AlchemistLevel),
                NextLevelExp = AlchemyProfessionRuleRuntimeCatalog.GetNextLevelExp(system.AlchemistLevel),
                ProfessionLevelCap = FiveElementProgressionRules.GetProfessionLevelCap(arrayLevel),
                Proficiency = system.Proficiency,
                SuccessRateBonus = system.SuccessRateBonus,
                CraftTimeReduction = system.CraftTimeReduction,
                YieldBonus = system.YieldBonus,
                TodayCraftCount = system.TodayCraftCount,
                TotalCraftCount = system.TotalCraftCount,
                SuccessCraftCount = system.SuccessCraftCount,
                LearnedRecipes = system.LearnedRecipes,
                IsCrafting = !string.IsNullOrWhiteSpace(system.ActiveRecipeId),
                ActiveRecipeId = system.ActiveRecipeId,
                ActiveCraftStartedAt = system.ActiveCraftStartedAt,
                ActiveCraftCompleteAt = system.ActiveCraftCompleteAt,
                CanCollect = system.ActiveCraftCompleteAt.HasValue && system.ActiveCraftCompleteAt <= DateTime.Now
            };
        }

        /// <summary>
        /// 获取玩家已学会的丹方列表。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <returns>已解锁丹方列表。</returns>
        public async Task<List<AlchemyRecipeDto>> GetLearnedRecipesAsync(string playerId)
        {
            var (system, _) = await PrepareAlchemySystemAsync(playerId);
            var unlockMap = await LoadRecipeUnlockMapAsync("Alchemy");

            var recipes = await _recipeRepository.GetListAsync(_ => true);
            return recipes
                .Where(recipe => system.LearnedRecipes.Contains(recipe.RecipeId))
                .OrderBy(recipe => recipe.RequiredFurnaceLevel)
                .ThenBy(recipe => recipe.RecipeId)
                .Select(recipe => MapRecipeToDto(recipe, system, true, unlockMap))
                .ToList();
        }

        /// <summary>
        /// 获取玩家当前已解锁的丹方列表。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <returns>玩家已经通过配方卷轴解锁的丹方列表。</returns>
        public async Task<List<AlchemyRecipeDto>> GetAvailableRecipesAsync(string playerId)
        {
            var (system, _) = await PrepareAlchemySystemAsync(playerId);
            var unlockMap = await LoadRecipeUnlockMapAsync("Alchemy");

            var recipes = await _recipeRepository.GetListAsync(_ => true);
            return recipes
                .Where(recipe => system.LearnedRecipes.Contains(recipe.RecipeId, StringComparer.OrdinalIgnoreCase))
                .OrderBy(recipe => recipe.RequiredFurnaceLevel)
                .ThenBy(recipe => recipe.RecipeId)
                .Select(recipe => MapRecipeToDto(recipe, system, true, unlockMap))
                .ToList();
        }

        /// <summary>
        /// 学习丹方（事务一致性）。
        /// </summary>
        /// <remarks>
        /// 方法作用：完成丹方学习，并保证扣费与学习状态写入保持一致。
        /// 关键逻辑：在同一事务内执行金币扣减与 LearnedRecipes 更新，任一步失败即回滚。
        /// </remarks>
        public async Task<bool> LearnRecipeAsync(string playerId, string recipeId)
        {
            throw new InvalidOperationException("请使用对应的配方卷轴解锁丹方。");
        }

        /// <summary>
        /// 开始一次炼丹流程。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="request">炼丹请求。</param>
        /// <returns>炼丹结果。</returns>
        public async Task<AlchemyResultDto> StartAlchemyAsync(string playerId, StartAlchemyRequestDto request)
        {
            var (system, arrayLevel) = await PrepareAlchemySystemAsync(playerId);
            if (!string.IsNullOrWhiteSpace(system.ActiveRecipeId) &&
                system.ActiveCraftCompleteAt.HasValue &&
                system.ActiveCraftCompleteAt > DateTime.Now)
            {
                return new AlchemyResultDto
                {
                    Success = false,
                    Message = "当前已有炼丹任务进行中，请等待完成后领取。",
                    RequiresCollection = true,
                    StartedAt = system.ActiveCraftStartedAt,
                    CompleteAt = system.ActiveCraftCompleteAt
                };
            }

            var recipe = await _recipeRepository.GetByIdAsync(request.RecipeId);
            if (recipe == null)
            {
                return new AlchemyResultDto { Success = false, Message = "丹方不存在" };
            }

            if (!system.LearnedRecipes.Contains(request.RecipeId))
            {
                return new AlchemyResultDto { Success = false, Message = "未学习该丹方" };
            }

            if (system.AlchemistLevel < recipe.RequiredFurnaceLevel)
            {
                return new AlchemyResultDto
                {
                    Success = false,
                    Message = $"炼丹师等级不足，需要 Lv.{recipe.RequiredFurnaceLevel}。",
                    ActualSuccessRate = 0,
                    AlchemistLevel = system.AlchemistLevel
                };
            }

            foreach (var material in recipe.Materials)
            {
                var hasEnough = await _inventoryService.GetItemCountAsync(playerId, material.ItemId) >= material.Amount * request.Quantity;
                if (!hasEnough)
                {
                    return new AlchemyResultDto { Success = false, Message = $"材料不足: {material.ItemName}" };
                }
            }

            var successRate = CalculateAlchemySuccessRate(recipe, system.AlchemistLevel);
            var random = Random.Shared;
            var isSuccess = random.Next(100) < successRate;
            var professionExpGained = AlchemyProfessionRuleRuntimeCatalog.GetProfessionExpGain(recipe.RequiredFurnaceLevel, isSuccess, request.Quantity);
            var craftTimeSeconds = Math.Max(60, recipe.BaseCraftTime * 60);
            var pillQuantity = isSuccess
                ? request.Quantity * Math.Max(1, 1 + system.YieldBonus / 100)
                : 0;
            var now = DateTime.Now;
            var completeAt = now.AddSeconds(craftTimeSeconds);

            foreach (var material in recipe.Materials)
            {
                var deductSuccess = await _inventoryService.DeductItemAsync(
                    playerId,
                    material.ItemId,
                    material.Amount * request.Quantity,
                    "炼丹消耗");

                if (!deductSuccess)
                {
                    _logger.LogWarning(
                        "玩家 {PlayerId} 炼丹扣材料失败，材料 {ItemId}，数量 {Quantity}",
                        playerId,
                        material.ItemId,
                        material.Amount * request.Quantity);
                    return new AlchemyResultDto { Success = false, Message = $"材料扣除失败: {material.ItemName}" };
                }
            }

            system.ActiveRecipeId = recipe.RecipeId;
            system.ActiveCraftStartedAt = now;
            system.ActiveCraftCompleteAt = completeAt;
            system.PendingResultJson = JsonSerializer.Serialize(new PendingAlchemyResultSnapshot
            {
                RecipeId = recipe.RecipeId,
                PillItemId = recipe.PillTemplateId,
                PillName = recipe.Name,
                PillQuantity = pillQuantity,
                Success = isSuccess,
                ActualSuccessRate = successRate,
                AlchemistExpGained = professionExpGained,
                ProficiencyGained = Math.Max(1, request.Quantity)
            });
            system.LastUpdateTime = now;
            await _alchemyRepository.UpdateAsync(system);

            return new AlchemyResultDto
            {
                Success = true,
                Message = $"开始炼丹，预计 {Math.Ceiling(craftTimeSeconds / 60d)} 分钟后完成。",
                ActualSuccessRate = successRate,
                StartedAt = now,
                CompleteAt = completeAt,
                RequiresCollection = true
            };
        }

        /// <summary>
        /// 领取正在炼制中的丹药结果。
        /// </summary>
        public async Task<AlchemyResultDto?> CollectPillAsync(string playerId)
        {
            var (system, arrayLevel) = await PrepareAlchemySystemAsync(playerId);
            if (string.IsNullOrWhiteSpace(system.ActiveRecipeId) || string.IsNullOrWhiteSpace(system.PendingResultJson))
            {
                return null;
            }

            if (!system.ActiveCraftCompleteAt.HasValue || system.ActiveCraftCompleteAt > DateTime.Now)
            {
                return new AlchemyResultDto
                {
                    Success = false,
                    Message = "炼丹尚未完成。",
                    RequiresCollection = true,
                    StartedAt = system.ActiveCraftStartedAt,
                    CompleteAt = system.ActiveCraftCompleteAt
                };
            }

            var pendingResult = JsonSerializer.Deserialize<PendingAlchemyResultSnapshot>(system.PendingResultJson);
            if (pendingResult == null)
            {
                return null;
            }

            GameData.Items.TryGetValue(pendingResult.PillItemId, out var pillItem);
            system.TodayCraftCount++;
            system.TotalCraftCount++;
            if (pendingResult.Success)
            {
                system.SuccessCraftCount++;
            }

            system.Proficiency += pendingResult.ProficiencyGained;
            system.AlchemistExp += pendingResult.AlchemistExpGained;
            system.AlchemistLevel = AlchemyProfessionRuleRuntimeCatalog.CalculateProfessionLevel(system.AlchemistExp, arrayLevel);
            system.SuccessRateBonus = AlchemyProfessionRuleRuntimeCatalog.GetSuccessBonus(system.AlchemistLevel, 1);
            system.ActiveRecipeId = null;
            system.ActiveCraftStartedAt = null;
            system.ActiveCraftCompleteAt = null;
            system.PendingResultJson = null;
            system.LastUpdateTime = DateTime.Now;
            await _alchemyRepository.UpdateAsync(system);

            if (pendingResult.Success && pendingResult.PillQuantity > 0)
            {
                await _inventoryService.AddItemAsync(playerId, new AddItemRequestDto
                {
                    ItemId = pendingResult.PillItemId,
                    Quantity = pendingResult.PillQuantity,
                    Source = $"Alchemy:{pendingResult.RecipeId}"
                });
            }

            await _achievementService.RecordRequirementEventAsync(playerId, new XXX.Achievement.AchievementRequirementEvent
            {
                RequirementType = pendingResult.Success
                    ? XXX.Achievement.AchievementRequirementType.AlchemySuccessCount
                    : XXX.Achievement.AchievementRequirementType.AlchemyFailureCount,
                TargetId = pendingResult.RecipeId,
                Delta = 1
            });

            await _questService.RecordObjectiveEventAsync(playerId, new XXX.Quest.QuestObjectiveEvent
            {
                ObjectiveType = XXX.Quest.ObjectiveType.AlchemyCollect,
                Delta = 1
            });

            if (pendingResult.Success)
            {
                await _questService.RecordObjectiveEventAsync(playerId, new XXX.Quest.QuestObjectiveEvent
                {
                    ObjectiveType = XXX.Quest.ObjectiveType.AlchemySuccess,
                    TargetId = pendingResult.RecipeId,
                    Delta = 1
                });
            }

            return new AlchemyResultDto
            {
                Success = pendingResult.Success,
                Message = pendingResult.Success
                    ? $"炼丹完成（成功率 {pendingResult.ActualSuccessRate}%）"
                    : $"炼丹失败（成功率 {pendingResult.ActualSuccessRate}%）",
                PillItemId = pendingResult.Success ? pendingResult.PillItemId : null,
                PillName = pendingResult.Success ? pillItem?.Name ?? pendingResult.PillName : null,
                PillQuantity = pendingResult.PillQuantity,
                ActualSuccessRate = pendingResult.ActualSuccessRate,
                AlchemistExpGained = pendingResult.AlchemistExpGained,
                AlchemistLevel = system.AlchemistLevel,
                ProficiencyGained = pendingResult.ProficiencyGained
            };
        }

        private async Task<(AlchemySystemEntity System, int ArrayLevel)> PrepareAlchemySystemAsync(string playerId)
        {
            await EnsureAlchemySeedDataAsync();

            var system = await _alchemyRepository.GetFirstAsync(a => a.PlayerId == playerId)
                ?? await CreateDefaultSystemAsync(playerId);
            var arrayLevel = await GetArrayLevelAsync(playerId);
            var changed = NormalizeAlchemySystem(system, arrayLevel);
            if (changed)
            {
                await _alchemyRepository.UpdateAsync(system);
            }

            return (system, arrayLevel);
        }

        private static bool NormalizeAlchemySystem(AlchemySystemEntity system, int arrayLevel)
        {
            var changed = false;
            var levelCap = FiveElementProgressionRules.GetProfessionLevelCap(arrayLevel);

            if (system.AlchemistExp <= 0 && system.Proficiency > 0)
            {
                system.AlchemistExp = system.Proficiency * 20;
                changed = true;
            }

            var normalizedLevel = AlchemyProfessionRuleRuntimeCatalog.CalculateProfessionLevel(system.AlchemistExp, levelCap);
            if (system.AlchemistLevel != normalizedLevel)
            {
                system.AlchemistLevel = normalizedLevel;
                changed = true;
            }

            if (system.FurnaceLevel <= 0)
            {
                system.FurnaceLevel = 1;
                changed = true;
            }

            if (system.AlchemistLevel < 1)
            {
                system.AlchemistLevel = 1;
                changed = true;
            }

            var normalizedBonus = AlchemyProfessionRuleRuntimeCatalog.GetSuccessBonus(system.AlchemistLevel, 1);
            if (system.SuccessRateBonus != normalizedBonus)
            {
                system.SuccessRateBonus = normalizedBonus;
                changed = true;
            }

            return changed;
        }

        private async Task<int> GetArrayLevelAsync(string playerId)
        {
            var array = await _fiveElementRepository.GetFirstAsync(entity => entity.PlayerId == playerId);
            return array == null
                ? 1
                : Math.Clamp(array.ArrayLevel <= 0 ? 1 : array.ArrayLevel, 1, 50);
        }

        private static int CalculateAlchemySuccessRate(AlchemyRecipeEntity recipe, int alchemistLevel)
        {
            var recipeLevel = Math.Max(1, recipe.RequiredFurnaceLevel);
            var overLevelBonus = AlchemyProfessionRuleRuntimeCatalog.GetSuccessBonus(alchemistLevel, recipeLevel);
            return Math.Min(95, recipe.BaseSuccessRate + overLevelBonus);
        }

        /// <summary>
        /// 中文注释：
        /// 炼丹默认系统会自动继承"数据库里标记为默认学习"的基础丹方，
        /// 这样新玩家第一次打开炼丹弹窗时就能直接看到至少两张可用丹方，
        /// 不至于出现面板打开了、数据库也有数据，但玩家一个都炼不了的断层体验。
        /// </summary>
        private async Task<AlchemySystemEntity> CreateDefaultSystemAsync(string playerId)
        {
            var system = new AlchemySystemEntity
            {
                PlayerId = playerId,
                FurnaceLevel = 1,
                AlchemistLevel = 1,
                AlchemistExp = 0,
                Proficiency = 0,
                LearnedRecipes = [],
                DailyResetTime = DateTime.Now.Date,
                LastUpdateTime = DateTime.Now
            };
            try
            {
                await _alchemyRepository.AddAsync(system);
                return system;
            }
            catch (Exception ex)
            {
                var existing = await _alchemyRepository.GetFirstAsync(entity => entity.PlayerId == playerId);
                if (existing != null)
                {
                    _logger.LogInformation(ex, "Alchemy system for player {PlayerId} was created concurrently, reusing existing row.", playerId);
                    return existing;
                }

                throw;
            }
        }

        /// <summary>
        /// 确保丹方种子数据存在，缺失时抛出异常。
        /// </summary>
        private async Task EnsureAlchemySeedDataAsync()
        {
            var existingRecipeIds = (await _recipeRepository.GetAllAsync())
                .Select(recipe => recipe.RecipeId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (existingRecipeIds.Count > 0)
            {
                return;
            }

            _logger.LogError(
                "Alchemy recipe table is empty.");
            throw new InvalidOperationException("Alchemy recipe table is empty. Please import database recipes before using alchemy.");
        }

        private async Task<Dictionary<string, (string ItemId, string ItemName)>> LoadRecipeUnlockMapAsync(string recipeType)
        {
            var unlocks = await _dbContext.Db.Queryable<ItemRecipeUnlockConfigEntity>()
                .Where(item => item.IsEnabled && item.RecipeType == recipeType)
                .ToListAsync();
            var itemIds = unlocks.Select(item => item.ItemId).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var itemNames = itemIds.Count == 0
                ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                : (await _dbContext.Db.Queryable<ItemTemplateEntity>()
                    .Where(item => itemIds.Contains(item.ItemId))
                    .ToListAsync())
                    .ToDictionary(item => item.ItemId, item => item.Name, StringComparer.OrdinalIgnoreCase);

            return unlocks
                .Where(item => !string.IsNullOrWhiteSpace(item.RecipeId))
                .GroupBy(item => item.RecipeId, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group =>
                    {
                        var item = group.First();
                        return (item.ItemId, itemNames.TryGetValue(item.ItemId, out var name) ? name : item.ItemId);
                    },
                    StringComparer.OrdinalIgnoreCase);
        }

        private static AlchemyRecipeDto MapRecipeToDto(
            AlchemyRecipeEntity entity,
            AlchemySystemEntity system,
            bool isLearned,
            IReadOnlyDictionary<string, (string ItemId, string ItemName)> unlockMap)
        {
            var requiredLevel = Math.Max(1, entity.RequiredFurnaceLevel);
            var canCraft = system.AlchemistLevel >= requiredLevel;
            GameData.Items.TryGetValue(entity.PillTemplateId, out var itemTemplate);
            return new AlchemyRecipeDto
            {
                RecipeId = entity.RecipeId,
                PillTemplateId = entity.PillTemplateId,
                Name = entity.Name,
                Description = entity.Description ?? string.Empty,
                RequiredLevel = requiredLevel,
                RequiredFurnaceLevel = entity.RequiredFurnaceLevel,
                BaseSuccessRate = entity.BaseSuccessRate,
                BaseCraftTime = entity.BaseCraftTime,
                ActualCraftTimeSeconds = Math.Max(60, entity.BaseCraftTime * 60),
                ActualSuccessRate = CalculateAlchemySuccessRate(entity, system.AlchemistLevel),
                SuccessRateBonus = AlchemyProfessionRuleRuntimeCatalog.GetSuccessBonus(system.AlchemistLevel, requiredLevel),
                CanCraft = canCraft,
                UnavailableReason = canCraft
                    ? string.Empty
                    : $"需要炼丹师等级达到 Lv.{requiredLevel}",
                Materials = entity.Materials?.Select(m => new RecipeMaterialDto
                {
                    ItemId = m.ItemId,
                    ItemName = m.ItemName,
                    Amount = m.Amount,
                    IsReplaceable = m.IsReplaceable,
                    Icon = ResolveMaterialIcon(m.ItemId)
                }).ToList() ?? [],
                IsLearned = isLearned,
                UnlockItemId = unlockMap.TryGetValue(entity.RecipeId, out var unlock) ? unlock.ItemId : null,
                UnlockItemName = unlockMap.TryGetValue(entity.RecipeId, out unlock) ? unlock.ItemName : null,
                IconPath = itemTemplate?.IconPath
            };
        }

        private static string ResolveMaterialIcon(string itemId)
        {
            if (!string.IsNullOrWhiteSpace(itemId)
                && GameData.Items.TryGetValue(itemId, out var itemTemplate)
                && !string.IsNullOrWhiteSpace(itemTemplate.IconPath))
            {
                return itemTemplate.IconPath;
            }

            return string.Empty;
        }

        private sealed class PendingAlchemyResultSnapshot
        {
            public string RecipeId { get; set; } = string.Empty;

            public string PillItemId { get; set; } = string.Empty;

            public string PillName { get; set; } = string.Empty;

            public int PillQuantity { get; set; }

            public bool Success { get; set; }

            public int ActualSuccessRate { get; set; }

            public int AlchemistExpGained { get; set; }

            public int ProficiencyGained { get; set; }
        }
    }
}
