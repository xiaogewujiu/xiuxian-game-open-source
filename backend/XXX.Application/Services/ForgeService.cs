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
    /// 锻造服务实现。
    /// </summary>
    public class ForgeService : IForgeService
    {
        private readonly DbContext _dbContext;
        private readonly IRepository<InventoryItemEntity> _inventoryRepository;
        private readonly IRepository<EquipmentInstanceEntity> _equipmentRepository;
        private readonly IRepository<UserEntity> _userRepository;
        private readonly IRepository<ForgeRecipeEntity> _forgeRecipeRepository;
        private readonly IRepository<ForgeSystemEntity> _forgeSystemRepository;
        private readonly IRepository<FiveElementArrayEntity> _fiveElementRepository;
        private readonly IGameSyncService _gameSyncService;
        private readonly IAchievementService _achievementService;
        private readonly IQuestService _questService;
        private readonly ILogger<ForgeService> _logger;

        /// <summary>
        /// 初始化锻造服务。
        /// </summary>
        public ForgeService(
            DbContext dbContext,
            IRepository<InventoryItemEntity> inventoryRepository,
            IRepository<EquipmentInstanceEntity> equipmentRepository,
            IRepository<UserEntity> userRepository,
            IRepository<ForgeRecipeEntity> forgeRecipeRepository,
            IRepository<ForgeSystemEntity> forgeSystemRepository,
            IRepository<FiveElementArrayEntity> fiveElementRepository,
            IGameSyncService gameSyncService,
            IAchievementService achievementService,
            IQuestService questService,
            ILogger<ForgeService> logger)
        {
            _dbContext = dbContext;
            _inventoryRepository = inventoryRepository;
            _equipmentRepository = equipmentRepository;
            _userRepository = userRepository;
            _forgeRecipeRepository = forgeRecipeRepository;
            _forgeSystemRepository = forgeSystemRepository;
            _fiveElementRepository = fiveElementRepository;
            _gameSyncService = gameSyncService;
            _achievementService = achievementService;
            _questService = questService;
            _logger = logger;
        }

        /// <summary>
        /// 获取锻造总览信息。
        /// </summary>
        public async Task<ForgeOverviewDto> GetForgeOverviewAsync(string playerId)
        {
            var (system, arrayLevel) = await PrepareForgeSystemAsync(playerId);
            var unlockMap = await LoadRecipeUnlockMapAsync();
            var recipeEntities = await LoadForgeRecipeEntitiesAsync(system);
            var recipes = recipeEntities.Select(entity => MapForgeRecipeEntity(entity, system, unlockMap)).ToList();
            var materialIds = recipes
                .SelectMany(recipe => recipe.Materials)
                .Select(material => material.ItemId)
                .Distinct()
                .ToList();

            var materials = await _inventoryRepository.GetListAsync(item => item.PlayerId == playerId && materialIds.Contains(item.ItemId));
            var materialDtos = materials
                .OrderBy(item => item.ItemId)
                .Select(MapInventoryItemToDto)
                .ToList();

            return new ForgeOverviewDto
            {
                BlacksmithLevel = system.BlacksmithLevel,
                BlacksmithExp = system.BlacksmithExp,
                CurrentLevelExp = ForgeProfessionRuleRuntimeCatalog.GetCurrentLevelProgressExp(system.BlacksmithExp, system.BlacksmithLevel),
                NextLevelExp = ForgeProfessionRuleRuntimeCatalog.GetNextLevelExp(system.BlacksmithLevel),
                ProfessionLevelCap = FiveElementProgressionRules.GetProfessionLevelCap(arrayLevel),
                IsForging = !string.IsNullOrWhiteSpace(system.ActiveRecipeId),
                ActiveRecipeId = system.ActiveRecipeId,
                ActiveForgeStartedAt = system.ActiveForgeStartedAt,
                ActiveForgeCompleteAt = system.ActiveForgeCompleteAt,
                CanCollect = system.ActiveForgeCompleteAt.HasValue && system.ActiveForgeCompleteAt <= DateTime.Now,
                Recipes = recipes,
                Materials = materialDtos
            };
        }

        /// <summary>
        /// 执行一次锻造。
        /// </summary>
        public async Task<ForgeEquipmentResultDto> ForgeAsync(string playerId, ForgeEquipmentRequestDto request)
        {
            var (system, arrayLevel) = await PrepareForgeSystemAsync(playerId);
            if (!string.IsNullOrWhiteSpace(system.ActiveRecipeId) &&
                system.ActiveForgeCompleteAt.HasValue &&
                system.ActiveForgeCompleteAt > DateTime.Now)
            {
                return new ForgeEquipmentResultDto
                {
                    Success = false,
                    Message = "当前已有锻造任务进行中，请等待完成后领取。",
                    RequiresCollection = true,
                    StartedAt = system.ActiveForgeStartedAt,
                    CompleteAt = system.ActiveForgeCompleteAt
                };
            }

            var recipeEntity = await _forgeRecipeRepository.GetByIdAsync(request.RecipeId);
            if (recipeEntity == null || !recipeEntity.IsEnabled)
            {
                return new ForgeEquipmentResultDto
                {
                    Success = false,
                    Message = "锻造图纸不存在"
                };
            }

            if (!system.LearnedRecipes.Contains(recipeEntity.RecipeId, StringComparer.OrdinalIgnoreCase))
            {
                return new ForgeEquipmentResultDto
                {
                    Success = false,
                    Message = "尚未解锁该锻造图纸，请先使用对应图纸。"
                };
            }

            var actualSuccessRate = CalculateForgeSuccessRate(recipeEntity, system.BlacksmithLevel);
            if (system.BlacksmithLevel < recipeEntity.Level)
            {
                return new ForgeEquipmentResultDto
                {
                    Success = false,
                    Message = $"锻造师等级不足，需要 Lv.{recipeEntity.Level}。",
                    ActualSuccessRate = actualSuccessRate,
                    BlacksmithLevel = system.BlacksmithLevel
                };
            }

            var player = await _userRepository.GetByIdAsync(playerId);
            if (player == null || player.IsDeleted)
            {
                return new ForgeEquipmentResultDto
                {
                    Success = false,
                    Message = "玩家不存在"
                };
            }

            var unlockMap = await LoadRecipeUnlockMapAsync();
            var recipeDto = MapForgeRecipeEntity(recipeEntity, system, unlockMap);
            if (player.Gold < recipeDto.CostGold)
            {
                return new ForgeEquipmentResultDto
                {
                    Success = false,
                    Message = "金币不足",
                    ActualSuccessRate = actualSuccessRate,
                    BlacksmithLevel = system.BlacksmithLevel
                };
            }

            if (!int.TryParse(recipeEntity.TemplateId, out var templateId) ||
                !GameData.EquipmentTemplates.TryGetValue(templateId, out var template))
            {
                return new ForgeEquipmentResultDto
                {
                    Success = false,
                    Message = "装备模板不存在",
                    ActualSuccessRate = actualSuccessRate,
                    BlacksmithLevel = system.BlacksmithLevel
                };
            }

            var isSuccess = Random.Shared.Next(100) < actualSuccessRate;
            var blacksmithExpGained = ForgeProfessionRuleRuntimeCatalog.GetProfessionExpGain(recipeEntity.Level, isSuccess);
            var craftTimeSeconds = Math.Max(60, (5 + recipeEntity.Level) * 60);
            var startedAt = DateTime.Now;
            var completeAt = startedAt.AddSeconds(craftTimeSeconds);
            var pendingResultJson = JsonSerializer.Serialize(new PendingForgeResultSnapshot
            {
                RecipeId = recipeEntity.RecipeId,
                TemplateId = recipeEntity.TemplateId,
                EquipmentName = recipeEntity.Name,
                Success = isSuccess,
                ActualSuccessRate = actualSuccessRate,
                BlacksmithExpGained = blacksmithExpGained
            });

            try
            {
                _dbContext.Db.Ado.BeginTran();

                var goldRows = await _dbContext.Db.Updateable<UserEntity>()
                    .SetColumns(u => u.Gold == u.Gold - recipeDto.CostGold)
                    .SetColumns(u => u.TotalGoldSpent == u.TotalGoldSpent + recipeDto.CostGold)
                    .SetColumns(u => u.LastUpdateTime == DateTime.Now)
                    .Where(u => u.GID == playerId && !u.IsDeleted && u.Gold >= recipeDto.CostGold)
                    .ExecuteCommandAsync();

                if (goldRows == 0)
                {
                    _dbContext.Db.Ado.RollbackTran();
                    return new ForgeEquipmentResultDto
                    {
                        Success = false,
                        Message = "金币不足",
                        ActualSuccessRate = actualSuccessRate,
                        BlacksmithLevel = system.BlacksmithLevel
                    };
                }

                foreach (var material in recipeDto.Materials)
                {
                    var deductSuccess = await DeductMaterialInTransactionAsync(playerId, material.ItemId, material.Count);
                    if (!deductSuccess)
                    {
                        _dbContext.Db.Ado.RollbackTran();
                        return new ForgeEquipmentResultDto
                        {
                            Success = false,
                            Message = $"{material.Name} 不足",
                            ActualSuccessRate = actualSuccessRate,
                            BlacksmithLevel = system.BlacksmithLevel
                        };
                    }
                }

                system.ActiveRecipeId = recipeEntity.RecipeId;
                system.ActiveForgeStartedAt = startedAt;
                system.ActiveForgeCompleteAt = completeAt;
                system.PendingResultJson = pendingResultJson;
                system.LastUpdateTime = DateTime.Now;
                var forgeRows = await _forgeSystemRepository.Db.Updateable<ForgeSystemEntity>()
                    .SetColumns(entity => entity.ActiveRecipeId == system.ActiveRecipeId)
                    .SetColumns(entity => entity.ActiveForgeStartedAt == system.ActiveForgeStartedAt)
                    .SetColumns(entity => entity.ActiveForgeCompleteAt == system.ActiveForgeCompleteAt)
                    .SetColumns(entity => entity.PendingResultJson == system.PendingResultJson)
                    .SetColumns(entity => entity.LastUpdateTime == system.LastUpdateTime)
                    .Where(entity => entity.PlayerId == playerId)
                    .ExecuteCommandAsync();

                if (forgeRows == 0)
                {
                    throw new InvalidOperationException("锻造任务创建失败：锻造系统状态未写入。");
                }

                _dbContext.Db.Ado.CommitTran();

                return new ForgeEquipmentResultDto
                {
                    Success = true,
                    Message = $"开始锻造，预计 {Math.Ceiling(craftTimeSeconds / 60d)} 分钟后完成。",
                    ActualSuccessRate = actualSuccessRate,
                    StartedAt = startedAt,
                    CompleteAt = completeAt,
                    RequiresCollection = true,
                    CostGold = recipeDto.CostGold,
                    ConsumedMaterials = recipeDto.Materials
                };
            }
            catch (Exception ex)
            {
                _dbContext.Db.Ado.RollbackTran();
                _logger.LogError(ex, "Player {PlayerId} forge recipe {RecipeId} exception", playerId, request.RecipeId);
                return new ForgeEquipmentResultDto
                {
                    Success = false,
                    Message = "锻造失败，请稍后重试",
                    ActualSuccessRate = actualSuccessRate,
                    BlacksmithLevel = system.BlacksmithLevel
                };
            }
        }

        /// <summary>
        /// 领取已经完成的锻造结果。
        /// 只有锻造任务结束且存在待领取结果时才会真正发放装备。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <returns>锻造领取结果；当前没有可领结果时返回空。</returns>
        public async Task<ForgeEquipmentResultDto?> CollectForgeResultAsync(string playerId)
        {
            var (system, arrayLevel) = await PrepareForgeSystemAsync(playerId);
            if (string.IsNullOrWhiteSpace(system.ActiveRecipeId) || string.IsNullOrWhiteSpace(system.PendingResultJson))
            {
                return null;
            }

            if (!system.ActiveForgeCompleteAt.HasValue || system.ActiveForgeCompleteAt > DateTime.Now)
            {
                return new ForgeEquipmentResultDto
                {
                    Success = false,
                    Message = "锻造尚未完成。",
                    RequiresCollection = true,
                    StartedAt = system.ActiveForgeStartedAt,
                    CompleteAt = system.ActiveForgeCompleteAt
                };
            }

            var pendingResult = JsonSerializer.Deserialize<PendingForgeResultSnapshot>(system.PendingResultJson);
            if (pendingResult == null)
            {
                return null;
            }

            EquipmentDto? equipmentDto = null;
            if (pendingResult.Success &&
                int.TryParse(pendingResult.TemplateId, out var templateId) &&
                GameData.EquipmentTemplates.TryGetValue(templateId, out var template))
            {
                if (!await InventoryCapacityRules.HasEquipmentSlotsAsync(_equipmentRepository.Db, playerId))
                {
                    return new ForgeEquipmentResultDto
                    {
                        Success = false,
                        Message = "装备背包已满，请先出售或丢弃装备后再领取锻造结果。",
                        RequiresCollection = true,
                        StartedAt = system.ActiveForgeStartedAt,
                        CompleteAt = system.ActiveForgeCompleteAt
                    };
                }

                var equipmentEntity = CreateEquipmentInstanceEntity(playerId, template);
                await _equipmentRepository.AddAsync(equipmentEntity);
                equipmentDto = MapEquipmentToDto(equipmentEntity, template.Description);
            }

            system.TodayForgeCount += 1;
            system.TotalForgeCount += 1;
            if (pendingResult.Success)
            {
                system.SuccessForgeCount += 1;
            }
            system.BlacksmithExp += pendingResult.BlacksmithExpGained;
            system.BlacksmithLevel = ForgeProfessionRuleRuntimeCatalog.CalculateProfessionLevel(system.BlacksmithExp, arrayLevel);
            system.ActiveRecipeId = null;
            system.ActiveForgeStartedAt = null;
            system.ActiveForgeCompleteAt = null;
            system.PendingResultJson = null;
            system.LastUpdateTime = DateTime.Now;
            await _forgeSystemRepository.UpdateAsync(system);
            await _gameSyncService.SyncPlayerAsync(playerId);

            await _achievementService.RecordRequirementEventAsync(playerId, new XXX.Achievement.AchievementRequirementEvent
            {
                RequirementType = pendingResult.Success
                    ? XXX.Achievement.AchievementRequirementType.ForgeSuccessCount
                    : XXX.Achievement.AchievementRequirementType.ForgeFailureCount,
                TargetId = pendingResult.RecipeId,
                Delta = 1
            });

            await _questService.RecordObjectiveEventAsync(playerId, new XXX.Quest.QuestObjectiveEvent
            {
                ObjectiveType = XXX.Quest.ObjectiveType.ForgeCollect,
                Delta = 1
            });

            if (pendingResult.Success)
            {
                await _questService.RecordObjectiveEventAsync(playerId, new XXX.Quest.QuestObjectiveEvent
                {
                    ObjectiveType = XXX.Quest.ObjectiveType.ForgeSuccess,
                    TargetId = pendingResult.RecipeId,
                    Delta = 1
                });
            }

            return new ForgeEquipmentResultDto
            {
                Success = pendingResult.Success,
                Message = pendingResult.Success
                    ? $"锻造完成（成功率 {pendingResult.ActualSuccessRate}%）"
                    : $"锻造失败（成功率 {pendingResult.ActualSuccessRate}%）",
                ActualSuccessRate = pendingResult.ActualSuccessRate,
                BlacksmithExpGained = pendingResult.BlacksmithExpGained,
                BlacksmithLevel = system.BlacksmithLevel,
                Equipment = equipmentDto
            };
        }

        private async Task<(ForgeSystemEntity System, int ArrayLevel)> PrepareForgeSystemAsync(string playerId)
        {
            var system = await _forgeSystemRepository.GetFirstAsync(entity => entity.PlayerId == playerId)
                ?? await CreateDefaultForgeSystemAsync(playerId);
            var arrayLevel = await GetArrayLevelAsync(playerId);
            var changed = NormalizeForgeSystem(system, arrayLevel);
            if (changed)
            {
                await _forgeSystemRepository.UpdateAsync(system);
            }

            return (system, arrayLevel);
        }

        private static bool NormalizeForgeSystem(ForgeSystemEntity system, int arrayLevel)
        {
            var changed = false;
            var today = DateTime.Today;
            if (system.DailyResetTime.Date < today)
            {
                system.TodayForgeCount = 0;
                system.DailyResetTime = today;
                changed = true;
            }

            var normalizedLevel = ForgeProfessionRuleRuntimeCatalog.CalculateProfessionLevel(
                Math.Max(0, system.BlacksmithExp),
                FiveElementProgressionRules.GetProfessionLevelCap(arrayLevel));
            if (system.BlacksmithLevel != normalizedLevel)
            {
                system.BlacksmithLevel = normalizedLevel;
                changed = true;
            }

            if (changed)
            {
                system.LastUpdateTime = DateTime.Now;
            }

            return changed;
        }

        private async Task<ForgeSystemEntity> CreateDefaultForgeSystemAsync(string playerId)
        {
            var system = new ForgeSystemEntity
            {
                PlayerId = playerId,
                BlacksmithLevel = 1,
                BlacksmithExp = 0,
                LearnedRecipes = [],
                DailyResetTime = DateTime.Today,
                LastUpdateTime = DateTime.Now
            };
            try
            {
                await _forgeSystemRepository.AddAsync(system);
                return await _forgeSystemRepository.GetFirstAsync(entity => entity.PlayerId == playerId) ?? system;
            }
            catch (Exception ex)
            {
                var existing = await _forgeSystemRepository.GetFirstAsync(entity => entity.PlayerId == playerId);
                if (existing != null)
                {
                    _logger.LogInformation(ex, "Forge system for player {PlayerId} was created concurrently, reusing existing row.", playerId);
                    return existing;
                }

                throw;
            }
        }

        private async Task<int> GetArrayLevelAsync(string playerId)
        {
            var array = await _fiveElementRepository.GetFirstAsync(entity => entity.PlayerId == playerId);
            return array == null
                ? 1
                : Math.Clamp(array.ArrayLevel <= 0 ? 1 : array.ArrayLevel, 1, 50);
        }

        private static int CalculateForgeSuccessRate(ForgeRecipeEntity recipe, int blacksmithLevel)
        {
            var recipeLevel = Math.Max(1, recipe.Level);
            var overLevelBonus = ForgeProfessionRuleRuntimeCatalog.GetSuccessBonus(blacksmithLevel, recipeLevel);
            return Math.Min(95, recipe.SuccessRate + overLevelBonus);
        }

        private async Task<List<ForgeRecipeEntity>> LoadForgeRecipeEntitiesAsync(ForgeSystemEntity system)
        {
            var entities = await _forgeRecipeRepository.Db.Queryable<ForgeRecipeEntity>()
                .Where(recipe => recipe.IsEnabled)
                .OrderBy(recipe => recipe.Level)
                .ToListAsync();

            var learned = system.LearnedRecipes.ToHashSet(StringComparer.OrdinalIgnoreCase);
            entities = entities
                .Where(recipe => learned.Contains(recipe.RecipeId))
                .OrderBy(recipe => recipe.Level)
                .ThenBy(recipe => recipe.RecipeId, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (entities.Count == 0)
            {
                _logger.LogWarning("Forge recipes table is empty.");
            }

            return entities;
        }

        private async Task<Dictionary<string, (string ItemId, string ItemName)>> LoadRecipeUnlockMapAsync()
        {
            var unlocks = await _dbContext.Db.Queryable<ItemRecipeUnlockConfigEntity>()
                .Where(item => item.IsEnabled && item.RecipeType == "Forge")
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

        private static ForgeRecipeDto MapForgeRecipeEntity(
            ForgeRecipeEntity entity,
            ForgeSystemEntity system,
            IReadOnlyDictionary<string, (string ItemId, string ItemName)> unlockMap)
        {
            List<ForgeRecipeMaterialSnapshot> materials;
            try
            {
                materials = JsonSerializer.Deserialize<List<ForgeRecipeMaterialSnapshot>>(entity.MaterialsJson) ?? [];
            }
            catch
            {
                materials = [];
            }

            var recipeIcon = string.Empty;
            if (int.TryParse(entity.TemplateId, out var templateId)
                && GameData.EquipmentTemplates.TryGetValue(templateId, out var equipTemplate))
            {
                recipeIcon = string.IsNullOrWhiteSpace(equipTemplate.IconPath)
                    ? $"equipment-{entity.SlotName.ToLowerInvariant()}"
                    : equipTemplate.IconPath;
            }

            var canForge = system.BlacksmithLevel >= Math.Max(1, entity.Level);
            return new ForgeRecipeDto
            {
                RecipeId = entity.RecipeId,
                TemplateId = entity.TemplateId,
                Name = entity.Name,
                Description = entity.Description,
                SlotName = entity.SlotName,
                Quality = entity.Quality,
                Level = entity.Level,
                Icon = recipeIcon,
                CostGold = entity.CostGold,
                SuccessRate = entity.SuccessRate,
                ActualSuccessRate = CalculateForgeSuccessRate(entity, system.BlacksmithLevel),
                SuccessRateBonus = ForgeProfessionRuleRuntimeCatalog.GetSuccessBonus(system.BlacksmithLevel, Math.Max(1, entity.Level)),
                CanForge = canForge,
                UnavailableReason = canForge
                    ? string.Empty
                    : $"需要锻造师等级达到 Lv.{entity.Level}",
                CraftTimeSeconds = Math.Max(60, (5 + entity.Level) * 60),
                Materials = materials.Select(material => new ForgeMaterialDto
                {
                    ItemId = material.ItemId,
                    Name = material.Name,
                    Icon = ResolveMaterialIcon(material),
                    Count = material.Count
                }).ToList(),
                UnlockItemId = unlockMap.TryGetValue(entity.RecipeId, out var unlock) ? unlock.ItemId : null,
                UnlockItemName = unlockMap.TryGetValue(entity.RecipeId, out unlock) ? unlock.ItemName : null
            };
        }

        private static string ResolveMaterialIcon(ForgeRecipeMaterialSnapshot material)
        {
            if (!string.IsNullOrWhiteSpace(material.ItemId)
                && GameData.Items.TryGetValue(material.ItemId, out var itemTemplate)
                && !string.IsNullOrWhiteSpace(itemTemplate.IconPath))
            {
                return itemTemplate.IconPath;
            }

            if (!string.IsNullOrWhiteSpace(material.Icon))
            {
                return material.Icon;
            }

            return string.Empty;
        }

        private async Task<bool> DeductMaterialInTransactionAsync(string playerId, string itemId, int requiredCount)
        {
            var materials = await _dbContext.Db.Queryable<InventoryItemEntity>()
                .Where(i => i.PlayerId == playerId && i.ItemId == itemId)
                .OrderBy(i => i.IsLocked)
                .OrderBy(i => i.Id)
                .ToListAsync();

            var totalCount = materials.Sum(i => i.Quantity);
            if (totalCount < requiredCount)
            {
                return false;
            }

            var remaining = requiredCount;
            foreach (var material in materials)
            {
                if (remaining <= 0)
                {
                    break;
                }

                var deductCount = Math.Min(material.Quantity, remaining);
                if (deductCount == material.Quantity)
                {
                    var deleteRows = await _dbContext.Db.Deleteable<InventoryItemEntity>()
                        .Where(i => i.Id == material.Id && i.PlayerId == playerId && i.Quantity == material.Quantity)
                        .ExecuteCommandAsync();

                    if (deleteRows == 0)
                    {
                        return false;
                    }
                }
                else
                {
                    var updateRows = await _dbContext.Db.Updateable<InventoryItemEntity>()
                        .SetColumns(i => i.Quantity == i.Quantity - deductCount)
                        .Where(i => i.Id == material.Id && i.PlayerId == playerId && i.Quantity >= deductCount)
                        .ExecuteCommandAsync();

                    if (updateRows == 0)
                    {
                        return false;
                    }
                }

                remaining -= deductCount;
            }

            return remaining == 0;
        }

        private static EquipmentInstanceEntity CreateEquipmentInstanceEntity(string playerId, EquipmentTemplate template)
        {
            return EquipmentBalanceHelper.CreateEntity(playerId, template, false);
        }

        private static InventoryItemDto MapInventoryItemToDto(InventoryItemEntity entity)
        {
            if (GameData.Items.TryGetValue(entity.ItemId, out var itemTemplate))
            {
                return new InventoryItemDto
                {
                    Id = entity.Id,
                    ItemId = entity.ItemId,
                    Name = itemTemplate.Name,
                    Description = itemTemplate.Description,
                    ItemType = itemTemplate.Type.ToString(),
                    Quality = itemTemplate.Quality <= 0 ? 1 : itemTemplate.Quality,
                    Icon = ResolveItemIcon(entity.ItemId, itemTemplate),
                    Quantity = entity.Quantity,
                    IsLocked = entity.IsLocked,
                    AcquiredTime = entity.AcquiredTime,
                    ExpireTime = entity.ExpireTime
                };
            }

            return new InventoryItemDto
            {
                Id = entity.Id,
                ItemId = entity.ItemId,
                Name = entity.ItemId,
                Description = "未知材料",
                ItemType = "Material",
                Quality = 1,
                Icon = "🎒",
                Quantity = entity.Quantity,
                IsLocked = entity.IsLocked,
                AcquiredTime = entity.AcquiredTime,
                ExpireTime = entity.ExpireTime
            };
        }

        private static string ResolveItemIcon(string itemId, ItemTable itemTemplate)
        {
            if (!string.IsNullOrWhiteSpace(itemTemplate.IconPath))
            {
                return itemTemplate.IconPath;
            }

            return itemId switch
            {
                "pet_egg" => "🥚",
                _ when itemTemplate.Type == ItemType.Pill || itemTemplate.Type == ItemType.Consumable => "🧪",
                _ when itemTemplate.Type == ItemType.Chest => "🎁",
                _ when itemTemplate.Type == ItemType.SkillBook => "📜",
                _ when itemTemplate.Type == ItemType.Seed => "🌱",
                _ when itemTemplate.Type == ItemType.PetEgg => "🥚",
                _ => "🎒"
            };
        }

        private static EquipmentDto MapEquipmentToDto(EquipmentInstanceEntity entity, string description)
        {
            var template = EquipmentBalanceHelper.ResolveTemplate(entity.TemplateId);
            return new EquipmentDto
            {
                InstanceId = entity.InstanceId,
                TemplateId = entity.TemplateId,
                Name = entity.Name,
                Description = description,
                Slot = entity.Slot,
                Level = template?.Level ?? 0,
                Quality = entity.Quality,
                EnhanceLevel = entity.EnhanceLevel,
                IsEquipped = entity.IsEquipped,
                IsLocked = entity.IsLocked,
                BasePhysicalAttack = EquipmentBalanceHelper.GetPhysicalAttack(entity, template),
                BaseMagicAttack = EquipmentBalanceHelper.GetMagicAttack(entity, template),
                BasePhysicalDefense = EquipmentBalanceHelper.GetPhysicalDefense(entity, template),
                BaseMagicDefense = EquipmentBalanceHelper.GetMagicDefense(entity, template),
                BaseHP = entity.BaseHP,
                BaseMP = entity.BaseMP,
                CombatStyle = template?.CombatStyle ?? EquipmentBalanceHelper.ResolveCombatStyle(entity),
                WeaponCategory = template?.WeaponCategory ?? WeaponCategory.None,
                Icon = string.IsNullOrWhiteSpace(template?.IconPath)
                    ? $"equipment-{entity.Slot.ToString().ToLowerInvariant()}"
                    : template.IconPath,
                AcquiredTime = entity.AcquiredTime
            };
        }

        private sealed class PendingForgeResultSnapshot
        {
            public string RecipeId { get; set; } = string.Empty;

            public string TemplateId { get; set; } = string.Empty;

            public string EquipmentName { get; set; } = string.Empty;

            public bool Success { get; set; }

            public int ActualSuccessRate { get; set; }

            public int BlacksmithExpGained { get; set; }
        }
    }
}
