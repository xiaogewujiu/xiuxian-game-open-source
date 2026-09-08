#pragma warning disable CS1591
using System.Text.Json;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    public class AdminItemService : IAdminItemService
    {
        private readonly IRepository<ItemTemplateEntity> _itemRepository;
        private readonly IRepository<ItemChestConfigEntity> _itemChestConfigRepository;
        private readonly IRepository<ItemChestRewardEntryEntity> _itemChestRewardEntryRepository;
        private readonly IRepository<ItemSkillBookConfigEntity> _itemSkillBookConfigRepository;
        private readonly IRepository<ItemRecipeUnlockConfigEntity> _itemRecipeUnlockConfigRepository;
        private readonly IRepository<ItemPetEggConfigEntity> _itemPetEggConfigRepository;
        private readonly IRepository<ItemPillConfigEntity> _itemPillConfigRepository;
        private readonly IRepository<InventoryItemEntity> _inventoryRepository;
        private readonly IRepository<ShopItemEntity> _shopItemRepository;
        private readonly IRepository<ShopDailyRecordEntity> _shopDailyRecordRepository;
        private readonly IRepository<CropTemplateEntity> _cropRepository;
        private readonly IRepository<AlchemyRecipeEntity> _alchemyRecipeRepository;
        private readonly IRepository<ForgeRecipeEntity> _forgeRecipeRepository;
        private readonly IRepository<CheckInRewardConfigEntity> _checkInRewardRepository;
        private readonly IRepository<RedeemCodeConfigEntity> _redeemCodeRepository;
        private readonly IRepository<QuestConfigEntity> _questRepository;
        private readonly IAdminRuntimeRefreshService _runtimeRefreshService;

        public AdminItemService(
            IRepository<ItemTemplateEntity> itemRepository,
            IRepository<ItemChestConfigEntity> itemChestConfigRepository,
            IRepository<ItemChestRewardEntryEntity> itemChestRewardEntryRepository,
            IRepository<ItemSkillBookConfigEntity> itemSkillBookConfigRepository,
            IRepository<ItemRecipeUnlockConfigEntity> itemRecipeUnlockConfigRepository,
            IRepository<ItemPetEggConfigEntity> itemPetEggConfigRepository,
            IRepository<ItemPillConfigEntity> itemPillConfigRepository,
            IRepository<InventoryItemEntity> inventoryRepository,
            IRepository<ShopItemEntity> shopItemRepository,
            IRepository<ShopDailyRecordEntity> shopDailyRecordRepository,
            IRepository<CropTemplateEntity> cropRepository,
            IRepository<AlchemyRecipeEntity> alchemyRecipeRepository,
            IRepository<ForgeRecipeEntity> forgeRecipeRepository,
            IRepository<CheckInRewardConfigEntity> checkInRewardRepository,
            IRepository<RedeemCodeConfigEntity> redeemCodeRepository,
            IRepository<QuestConfigEntity> questRepository,
            IAdminRuntimeRefreshService runtimeRefreshService)
        {
            _itemRepository = itemRepository;
            _itemChestConfigRepository = itemChestConfigRepository;
            _itemChestRewardEntryRepository = itemChestRewardEntryRepository;
            _itemSkillBookConfigRepository = itemSkillBookConfigRepository;
            _itemRecipeUnlockConfigRepository = itemRecipeUnlockConfigRepository;
            _itemPetEggConfigRepository = itemPetEggConfigRepository;
            _itemPillConfigRepository = itemPillConfigRepository;
            _inventoryRepository = inventoryRepository;
            _shopItemRepository = shopItemRepository;
            _shopDailyRecordRepository = shopDailyRecordRepository;
            _cropRepository = cropRepository;
            _alchemyRecipeRepository = alchemyRecipeRepository;
            _forgeRecipeRepository = forgeRecipeRepository;
            _checkInRewardRepository = checkInRewardRepository;
            _redeemCodeRepository = redeemCodeRepository;
            _questRepository = questRepository;
            _runtimeRefreshService = runtimeRefreshService;
        }

        public async Task<List<AdminItemListItemDto>> GetListAsync(string? keyword = null, int? type = null)
        {
            var normalizedKeyword = (keyword ?? string.Empty).Trim();
            var query = _itemRepository.Db.Queryable<ItemTemplateEntity>();

            if (!string.IsNullOrWhiteSpace(normalizedKeyword))
            {
                query = query.Where(item => item.ItemId.Contains(normalizedKeyword) || item.Name.Contains(normalizedKeyword));
            }

            query = query.WhereIF(type.HasValue, item => item.Type == type!.Value);

            var items = await query.OrderBy(item => item.UseLevel).OrderBy(item => item.ItemId).ToListAsync();
            return items.Select(item => new AdminItemListItemDto
            {
                ItemId = item.ItemId,
                Name = item.Name,
                UseLevel = item.UseLevel,
                Type = item.Type,
                Quality = item.Quality,
                IconPath = item.IconPath,
                IsBuiltIn = item.IsBuiltIn,
                BuiltInVersion = item.BuiltInVersion,
                IsTradeable = item.IsTradeable
            }).ToList();
        }

        public async Task<AdminItemDetailDto?> GetDetailAsync(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return null;
            }

            var item = await _itemRepository.GetByIdAsync(itemId.Trim());
            if (item == null)
            {
                return null;
            }

            return new AdminItemDetailDto
            {
                ItemId = item.ItemId,
                Name = item.Name,
                UseLevel = item.UseLevel,
                Type = item.Type,
                Description = item.Description,
                MaxStack = item.MaxStack,
                Quality = item.Quality,
                IconPath = item.IconPath,
                IsBuiltIn = item.IsBuiltIn,
                SeedKey = item.SeedKey,
                BuiltInVersion = item.BuiltInVersion,
                LastUpdateTime = item.LastUpdateTime,
                IsTradeable = item.IsTradeable,
                ChestConfig = await LoadChestConfigAsync(item),
                SkillBookConfig = await LoadSkillBookConfigAsync(item),
                PetEggConfig = await LoadPetEggConfigAsync(item),
                PillConfig = await LoadPillConfigAsync(item),
                RecipeUnlockConfig = await LoadRecipeUnlockConfigAsync(item)
            };
        }

        private async Task<AdminItemRecipeUnlockConfigDto?> LoadRecipeUnlockConfigAsync(ItemTemplateEntity item)
        {
            if (item.Type != (int)ItemType.RecipeScroll)
            {
                return null;
            }

            var config = await _itemRecipeUnlockConfigRepository.GetByIdAsync(item.ItemId);
            return config == null || !config.IsEnabled
                ? null
                : new AdminItemRecipeUnlockConfigDto
                {
                    RecipeType = config.RecipeType,
                    RecipeId = config.RecipeId
                };
        }

        public async Task<AdminItemDetailDto> SaveAsync(AdminItemDetailDto request)
        {
            var itemId = (request.ItemId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(itemId))
            {
                throw new InvalidOperationException("道具编号不能为空。");
            }

            var name = (request.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("道具名称不能为空。");
            }

            // 丹药配置校验
            var pillConfig = request.PillConfig;
            if (pillConfig != null)
            {
                if (pillConfig.DurationMinutes < 0)
                    throw new InvalidOperationException("时效分钟不允许小于 0。");

                if (pillConfig.MaxUsageCount < 0)
                    throw new InvalidOperationException("使用次数不允许小于 0。");

                if (pillConfig.EffectType == 2 && pillConfig.DurationMinutes > 0)
                    throw new InvalidOperationException("增加经验值类型的丹药不允许配置时效。");

                if (pillConfig.EffectType == 4 && pillConfig.DurationMinutes > 0)
                    throw new InvalidOperationException("恢复HP/MP类型的丹药不允许配置时效。");
            }

            var existing = await _itemRepository.GetByIdAsync(itemId) ?? new ItemTemplateEntity { ItemId = itemId };
            var isNew = await _itemRepository.GetByIdAsync(itemId) == null;

            _itemRepository.Db.Ado.BeginTran();
            try
            {
                ApplyItem(existing, request);

                if (isNew)
                {
                    await _itemRepository.AddAsync(existing);
                }
                else
                {
                    await _itemRepository.UpdateAsync(existing);
                }

                await SaveSplitConfigsAsync(existing.ItemId, request);
                _itemRepository.Db.Ado.CommitTran();
            }
            catch
            {
                _itemRepository.Db.Ado.RollbackTran();
                throw;
            }

            await _runtimeRefreshService.ReloadRuntimeTemplatesAsync();
            return (await GetDetailAsync(itemId))!;
        }

        public async Task<bool> DeleteAsync(string itemId)
        {
            var normalizedItemId = (itemId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedItemId))
            {
                return false;
            }

            if (await _inventoryRepository.Db.Queryable<InventoryItemEntity>().Where(item => item.ItemId == normalizedItemId).AnyAsync())
            {
                throw new InvalidOperationException("当前道具仍被玩家背包实例引用，不能直接删除。");
            }

            if (await _shopItemRepository.Db.Queryable<ShopItemEntity>().Where(item => item.ItemType == 0 && item.ItemId == normalizedItemId).AnyAsync())
            {
                throw new InvalidOperationException("当前道具仍被商店商品引用，不能直接删除。");
            }

            if (await _shopDailyRecordRepository.Db.Queryable<ShopDailyRecordEntity>().Where(record => record.ItemId == normalizedItemId).AnyAsync())
            {
                throw new InvalidOperationException("当前道具仍被商店购买记录引用，不能直接删除。");
            }

            if (await _cropRepository.Db.Queryable<CropTemplateEntity>().Where(crop => crop.SeedId == normalizedItemId || crop.OutputItemId == normalizedItemId).AnyAsync())
            {
                throw new InvalidOperationException("当前道具仍被灵田作物模板引用，不能直接删除。");
            }

            var alchemyRecipes = await _alchemyRecipeRepository.Db.Queryable<AlchemyRecipeEntity>()
                .Where(recipe => recipe.MaterialsJson != null && recipe.MaterialsJson.Contains(normalizedItemId))
                .ToListAsync();
            if (alchemyRecipes.Any(recipe => ContainsJsonItemId(recipe.MaterialsJson, normalizedItemId)))
            {
                throw new InvalidOperationException("当前道具仍被炼丹配方材料引用，不能直接删除。");
            }

            var forgeRecipes = await _forgeRecipeRepository.Db.Queryable<ForgeRecipeEntity>()
                .Where(recipe => recipe.MaterialsJson != null && recipe.MaterialsJson.Contains(normalizedItemId))
                .ToListAsync();
            if (forgeRecipes.Any(recipe => ContainsJsonItemId(recipe.MaterialsJson, normalizedItemId)))
            {
                throw new InvalidOperationException("当前道具仍被锻造配方材料引用，不能直接删除。");
            }

            var checkInRewards = await _checkInRewardRepository.Db.Queryable<CheckInRewardConfigEntity>()
                .Where(config => config.RewardJson.Contains(normalizedItemId))
                .ToListAsync();
            if (checkInRewards.Any(config => ContainsRewardItemId(config.RewardJson, normalizedItemId)))
            {
                throw new InvalidOperationException("当前道具仍被签到奖励引用，不能直接删除。");
            }

            var redeemRewards = await _redeemCodeRepository.Db.Queryable<RedeemCodeConfigEntity>()
                .Where(config => config.RewardJson.Contains(normalizedItemId))
                .ToListAsync();
            if (redeemRewards.Any(config => ContainsRewardItemId(config.RewardJson, normalizedItemId)))
            {
                throw new InvalidOperationException("当前道具仍被兑换码奖励引用，不能直接删除。");
            }

            var quests = await _questRepository.Db.Queryable<QuestConfigEntity>()
                .Where(quest => quest.RewardItemsJson != null && quest.RewardItemsJson.Contains(normalizedItemId))
                .ToListAsync();
            if (quests.Any(quest => ContainsQuestRewardItem(quest.RewardItemsJson, normalizedItemId)))
            {
                throw new InvalidOperationException("当前道具仍被任务奖励引用，不能直接删除。");
            }

            _itemRepository.Db.Ado.BeginTran();
            int deleteRows;
            try
            {
                await _itemSkillBookConfigRepository.DeleteAsync(normalizedItemId);
                await _itemRecipeUnlockConfigRepository.DeleteAsync(normalizedItemId);
                await _itemPetEggConfigRepository.DeleteAsync(normalizedItemId);
                await _itemPillConfigRepository.DeleteAsync(normalizedItemId);
                deleteRows = await _itemRepository.DeleteAsync(normalizedItemId);
                _itemRepository.Db.Ado.CommitTran();
            }
            catch
            {
                _itemRepository.Db.Ado.RollbackTran();
                throw;
            }

            if (deleteRows > 0)
            {
                await _runtimeRefreshService.ReloadRuntimeTemplatesAsync();
            }

            return deleteRows > 0;
        }

        private static ItemTemplateEntity ApplyItem(ItemTemplateEntity entity, AdminItemDetailDto request)
        {
            entity.Name = request.Name.Trim();
            entity.UseLevel = Math.Max(0, request.UseLevel);
            entity.Type = request.Type;
            entity.Description = (request.Description ?? string.Empty).Trim();
            entity.MaxStack = Math.Max(1, request.MaxStack);
            entity.Quality = Math.Max(1, request.Quality);
            entity.IconPath = string.IsNullOrWhiteSpace(request.IconPath) ? null : request.IconPath.Trim();
            entity.SeedKey = null;
            entity.IsBuiltIn = false;
            entity.BuiltInVersion = null;
            entity.LastUpdateTime = DateTime.Now;
            entity.IsTradeable = request.IsTradeable;
            entity.ChestConfigJson = null;
            entity.SkillBookConfigJson = null;
            entity.PetEggConfigJson = null;
            entity.PillConfigJson = null;
            return entity;
        }

        private async Task<AdminItemChestConfigDto?> LoadChestConfigAsync(ItemTemplateEntity item)
        {
            var configEntity = await _itemChestConfigRepository.GetByIdAsync(item.ItemId);
            if (configEntity != null && configEntity.IsEnabled)
            {
                var rewards = await _itemChestRewardEntryRepository.Db.Queryable<ItemChestRewardEntryEntity>()
                    .Where(entry => entry.ItemId == item.ItemId)
                    .OrderBy(entry => entry.SortOrder)
                    .ToListAsync();
                if (rewards.Count == 0)
                {
                    return null;
                }

                return new AdminItemChestConfigDto
                {
                    OpenMode = configEntity.OpenMode,
                    RollCount = configEntity.RollCount,
                    Rewards = rewards.Select(reward => new AdminItemChestRewardDto
                    {
                        RewardType = reward.RewardType,
                        TargetId = reward.TargetId,
                        MinCount = reward.MinCount,
                        MaxCount = reward.MaxCount,
                        Weight = reward.Weight,
                        Description = reward.Description
                    }).ToList()
                };
            }

            return MapChestConfig(item.ChestConfig);
        }

        private async Task<AdminItemSkillBookConfigDto?> LoadSkillBookConfigAsync(ItemTemplateEntity item)
        {
            var configEntity = await _itemSkillBookConfigRepository.GetByIdAsync(item.ItemId);
            if (configEntity != null && configEntity.IsEnabled)
            {
                return new AdminItemSkillBookConfigDto
                {
                    SkillId = configEntity.SkillId
                };
            }

            return MapSkillBookConfig(item.SkillBookConfig);
        }

        private async Task<AdminItemPetEggConfigDto?> LoadPetEggConfigAsync(ItemTemplateEntity item)
        {
            var configEntity = await _itemPetEggConfigRepository.GetByIdAsync(item.ItemId);
            if (configEntity != null && configEntity.IsEnabled)
            {
                return new AdminItemPetEggConfigDto
                {
                    PetTemplateId = configEntity.PetTemplateId
                };
            }

            return MapPetEggConfig(item.PetEggConfig);
        }

        private async Task<AdminItemPillConfigDto?> LoadPillConfigAsync(ItemTemplateEntity item)
        {
            var configEntity = await _itemPillConfigRepository.GetByIdAsync(item.ItemId);
            if (configEntity != null && configEntity.IsEnabled)
            {
                return new AdminItemPillConfigDto
                {
                    EffectType = configEntity.EffectType,
                    BreakthroughBonusPercent = configEntity.BreakthroughBonusPercent,
                    ExpGain = configEntity.ExpGain,
                    AttributeType = configEntity.AttributeType,
                    AttributeValue = configEntity.AttributeValue,
                    DurationMinutes = configEntity.DurationMinutes,
                    MaxUsageCount = configEntity.MaxUsageCount,
                    HealHpPercent = configEntity.HealHpPercent,
                    HealMpPercent = configEntity.HealMpPercent
                };
            }

            return MapPillConfig(item.PillConfig);
        }

        private async Task SaveSplitConfigsAsync(string itemId, AdminItemDetailDto request)
        {
            if (request.Type == (int)ItemType.Chest)
            {
                var config = NormalizeChestConfig(request.ChestConfig);
                if (config == null)
                {
                    await _itemChestConfigRepository.DeleteAsync(itemId);
                    await _itemChestRewardEntryRepository.Db.Deleteable<ItemChestRewardEntryEntity>()
                        .Where(entry => entry.ItemId == itemId)
                        .ExecuteCommandAsync();
                }
                else
                {
                    var entity = await _itemChestConfigRepository.GetByIdAsync(itemId) ?? new ItemChestConfigEntity { ItemId = itemId };
                    entity.OpenMode = (int)config.OpenMode;
                    entity.RollCount = Math.Max(1, config.RollCount);
                    entity.IsEnabled = true;
                    entity.LastUpdateTime = DateTime.Now;
                    if (await _itemChestConfigRepository.GetByIdAsync(itemId) == null)
                    {
                        await _itemChestConfigRepository.AddAsync(entity);
                    }
                    else
                    {
                        await _itemChestConfigRepository.UpdateAsync(entity);
                    }

                    await _itemChestRewardEntryRepository.Db.Deleteable<ItemChestRewardEntryEntity>()
                        .Where(entry => entry.ItemId == itemId)
                        .ExecuteCommandAsync();
                    var rewards = config.Rewards
                        .Select((reward, index) => new ItemChestRewardEntryEntity
                        {
                            GID = Guid.NewGuid().ToString("N"),
                            ItemId = itemId,
                            RewardType = (int)reward.RewardType,
                            TargetId = reward.TargetId,
                            MinCount = reward.MinCount,
                            MaxCount = reward.MaxCount,
                            Weight = reward.Weight,
                            Description = reward.Description,
                            SortOrder = index + 1
                        })
                        .ToList();
                    if (rewards.Count > 0)
                    {
                        await _itemChestRewardEntryRepository.Db.Insertable(rewards).ExecuteCommandAsync();
                    }
                }
            }
            else
            {
                await _itemChestConfigRepository.DeleteAsync(itemId);
                await _itemChestRewardEntryRepository.Db.Deleteable<ItemChestRewardEntryEntity>()
                    .Where(entry => entry.ItemId == itemId)
                    .ExecuteCommandAsync();
            }

            if (request.Type == (int)ItemType.SkillBook)
            {
                var config = NormalizeSkillBookConfig(request.SkillBookConfig);
                if (config == null)
                {
                    await _itemSkillBookConfigRepository.DeleteAsync(itemId);
                }
                else
                {
                    var entity = await _itemSkillBookConfigRepository.GetByIdAsync(itemId) ?? new ItemSkillBookConfigEntity { ItemId = itemId };
                    entity.SkillId = config.SkillId;
                    entity.IsEnabled = true;
                    entity.LastUpdateTime = DateTime.Now;
                    if (await _itemSkillBookConfigRepository.GetByIdAsync(itemId) == null)
                    {
                        await _itemSkillBookConfigRepository.AddAsync(entity);
                    }
                    else
                    {
                        await _itemSkillBookConfigRepository.UpdateAsync(entity);
                    }
                }
            }
            else
            {
                await _itemSkillBookConfigRepository.DeleteAsync(itemId);
            }

            if (request.Type == (int)ItemType.RecipeScroll)
            {
                var config = request.RecipeUnlockConfig;
                var recipeType = (config?.RecipeType ?? string.Empty).Trim();
                var recipeId = (config?.RecipeId ?? string.Empty).Trim();
                if (!(recipeType.Equals("Alchemy", StringComparison.OrdinalIgnoreCase) ||
                      recipeType.Equals("Forge", StringComparison.OrdinalIgnoreCase)) ||
                    string.IsNullOrWhiteSpace(recipeId))
                {
                    throw new InvalidOperationException("配方卷轴必须配置有效的炼丹或锻造配方。");
                }

                recipeType = recipeType.Equals("Alchemy", StringComparison.OrdinalIgnoreCase) ? "Alchemy" : "Forge";
                var recipeExists = recipeType == "Alchemy"
                    ? await _alchemyRecipeRepository.Db.Queryable<AlchemyRecipeEntity>().AnyAsync(recipe => recipe.RecipeId == recipeId)
                    : await _forgeRecipeRepository.Db.Queryable<ForgeRecipeEntity>().AnyAsync(recipe => recipe.RecipeId == recipeId && recipe.IsEnabled);
                if (!recipeExists)
                {
                    throw new InvalidOperationException("配方卷轴绑定的配方不存在或未启用。");
                }

                var duplicate = await _itemRecipeUnlockConfigRepository.Db.Queryable<ItemRecipeUnlockConfigEntity>()
                    .Where(configItem => configItem.IsEnabled && configItem.RecipeType == recipeType && configItem.RecipeId == recipeId && configItem.ItemId != itemId)
                    .AnyAsync();
                if (duplicate)
                {
                    throw new InvalidOperationException("该配方已经绑定其他配方卷轴。");
                }

                var unlock = await _itemRecipeUnlockConfigRepository.GetByIdAsync(itemId)
                    ?? new ItemRecipeUnlockConfigEntity { ItemId = itemId };
                unlock.RecipeType = recipeType;
                unlock.RecipeId = recipeId;
                unlock.IsEnabled = true;
                unlock.LastUpdateTime = DateTime.Now;
                if (await _itemRecipeUnlockConfigRepository.GetByIdAsync(itemId) == null)
                {
                    await _itemRecipeUnlockConfigRepository.AddAsync(unlock);
                }
                else
                {
                    await _itemRecipeUnlockConfigRepository.UpdateAsync(unlock);
                }
            }
            else
            {
                await _itemRecipeUnlockConfigRepository.DeleteAsync(itemId);
            }

            if (request.Type == (int)ItemType.PetEgg)
            {
                var config = NormalizePetEggConfig(request.PetEggConfig);
                if (config == null)
                {
                    await _itemPetEggConfigRepository.DeleteAsync(itemId);
                }
                else
                {
                    var entity = await _itemPetEggConfigRepository.GetByIdAsync(itemId) ?? new ItemPetEggConfigEntity { ItemId = itemId };
                    entity.PetTemplateId = config.PetTemplateId;
                    entity.IsEnabled = true;
                    entity.LastUpdateTime = DateTime.Now;
                    if (await _itemPetEggConfigRepository.GetByIdAsync(itemId) == null)
                    {
                        await _itemPetEggConfigRepository.AddAsync(entity);
                    }
                    else
                    {
                        await _itemPetEggConfigRepository.UpdateAsync(entity);
                    }
                }
            }
            else
            {
                await _itemPetEggConfigRepository.DeleteAsync(itemId);
            }

            if (request.Type == (int)ItemType.Pill)
            {
                var config = NormalizePillConfig(request.PillConfig);
                if (config == null)
                {
                    await _itemPillConfigRepository.DeleteAsync(itemId);
                }
                else
                {
                    var entity = await _itemPillConfigRepository.GetByIdAsync(itemId) ?? new ItemPillConfigEntity { ItemId = itemId };
                    entity.EffectType = (int)config.EffectType;
                    entity.BreakthroughBonusPercent = config.BreakthroughBonusPercent;
                    entity.ExpGain = config.ExpGain;
                    entity.AttributeType = config.AttributeType?.ToString();
                    entity.AttributeValue = config.AttributeValue;
                    entity.DurationMinutes = config.DurationMinutes;
                    entity.MaxUsageCount = config.MaxUsageCount;
                    entity.HealHpPercent = config.HealHpPercent;
                    entity.HealMpPercent = config.HealMpPercent;
                    entity.IsEnabled = true;
                    entity.LastUpdateTime = DateTime.Now;
                    if (await _itemPillConfigRepository.GetByIdAsync(itemId) == null)
                    {
                        await _itemPillConfigRepository.AddAsync(entity);
                    }
                    else
                    {
                        await _itemPillConfigRepository.UpdateAsync(entity);
                    }
                }
            }
            else
            {
                await _itemPillConfigRepository.DeleteAsync(itemId);
            }
        }

        private static AdminItemChestConfigDto? MapChestConfig(ItemChestConfig? config)
        {
            if (config == null)
            {
                return null;
            }

            return new AdminItemChestConfigDto
            {
                OpenMode = (int)config.OpenMode,
                RollCount = config.RollCount,
                Rewards = config.Rewards.Select(reward => new AdminItemChestRewardDto
                {
                    RewardType = (int)reward.RewardType,
                    TargetId = reward.TargetId,
                    MinCount = reward.MinCount,
                    MaxCount = reward.MaxCount,
                    Weight = reward.Weight,
                    Description = reward.Description
                }).ToList()
            };
        }

        private static ItemChestConfig? NormalizeChestConfig(AdminItemChestConfigDto? config)
        {
            if (config == null)
            {
                return null;
            }

            var rewards = (config.Rewards ?? [])
                .Where(reward => reward != null && reward.Weight > 0)
                .Select(reward => new ItemChestRewardEntry
                {
                    RewardType = Enum.IsDefined(typeof(ChestRewardType), reward.RewardType)
                        ? (ChestRewardType)reward.RewardType
                        : ChestRewardType.Item,
                    TargetId = string.IsNullOrWhiteSpace(reward.TargetId) ? null : reward.TargetId.Trim(),
                    MinCount = Math.Max(1, reward.MinCount),
                    MaxCount = Math.Max(Math.Max(1, reward.MinCount), reward.MaxCount),
                    Weight = Math.Max(1, reward.Weight),
                    Description = string.IsNullOrWhiteSpace(reward.Description) ? null : reward.Description.Trim()
                })
                .ToList();

            if (rewards.Count == 0)
            {
                return null;
            }

            return new ItemChestConfig
            {
                OpenMode = Enum.IsDefined(typeof(ChestOpenMode), config.OpenMode)
                    ? (ChestOpenMode)config.OpenMode
                    : ChestOpenMode.SinglePick,
                RollCount = Math.Max(1, config.RollCount),
                Rewards = rewards
            };
        }

        private static AdminItemSkillBookConfigDto? MapSkillBookConfig(ItemSkillBookConfig? config)
        {
            return config == null
                ? null
                : new AdminItemSkillBookConfigDto
                {
                    SkillId = config.SkillId
                };
        }

        private static ItemSkillBookConfig? NormalizeSkillBookConfig(AdminItemSkillBookConfigDto? config)
        {
            if (config == null || config.SkillId <= 0)
            {
                return null;
            }

            return new ItemSkillBookConfig
            {
                SkillId = config.SkillId
            };
        }

        private static AdminItemPetEggConfigDto? MapPetEggConfig(ItemPetEggConfig? config)
        {
            return config == null
                ? null
                : new AdminItemPetEggConfigDto
                {
                    PetTemplateId = config.PetTemplateId
                };
        }

        private static ItemPetEggConfig? NormalizePetEggConfig(AdminItemPetEggConfigDto? config)
        {
            if (config == null || string.IsNullOrWhiteSpace(config.PetTemplateId))
            {
                return null;
            }

            return new ItemPetEggConfig
            {
                PetTemplateId = config.PetTemplateId.Trim()
            };
        }

        private static AdminItemPillConfigDto? MapPillConfig(ItemPillConfig? config)
        {
            return config == null
                ? null
                : new AdminItemPillConfigDto
                {
                    EffectType = (int)config.EffectType,
                    BreakthroughBonusPercent = config.BreakthroughBonusPercent,
                    ExpGain = config.ExpGain,
                    AttributeType = config.AttributeType?.ToString(),
                    AttributeValue = config.AttributeValue,
                    DurationMinutes = config.DurationMinutes,
                    MaxUsageCount = config.MaxUsageCount,
                    HealHpPercent = config.HealHpPercent,
                    HealMpPercent = config.HealMpPercent
                };
        }

        private static ItemPillConfig? NormalizePillConfig(AdminItemPillConfigDto? config)
        {
            if (config == null)
            {
                return null;
            }

            AttributeType? attributeType = null;
            if (!string.IsNullOrWhiteSpace(config.AttributeType) &&
                Enum.TryParse<AttributeType>(config.AttributeType.Trim(), true, out var parsedAttributeType))
            {
                attributeType = parsedAttributeType;
            }

            return new ItemPillConfig
            {
                EffectType = Enum.IsDefined(typeof(ItemPillEffectType), config.EffectType)
                    ? (ItemPillEffectType)config.EffectType
                    : ItemPillEffectType.AddExp,
                BreakthroughBonusPercent = Math.Max(0, config.BreakthroughBonusPercent),
                ExpGain = Math.Max(0, config.ExpGain),
                AttributeType = attributeType,
                AttributeValue = Math.Max(0, config.AttributeValue),
                DurationMinutes = Math.Max(0, config.DurationMinutes),
                MaxUsageCount = Math.Max(0, config.MaxUsageCount),
                HealHpPercent = Math.Clamp(config.HealHpPercent, 0, 100),
                HealMpPercent = Math.Clamp(config.HealMpPercent, 0, 100)
            };
        }

        private static bool ContainsJsonItemId(string? json, string itemId)
        {
            if (string.IsNullOrWhiteSpace(json)) return false;
            try
            {
                using var document = JsonDocument.Parse(json);
                if (document.RootElement.ValueKind != JsonValueKind.Array) return false;
                foreach (var element in document.RootElement.EnumerateArray())
                {
                    if (TryReadStringProperty(element, "ItemId", out var currentItemId) && string.Equals(currentItemId, itemId, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        private static bool ContainsRewardItemId(string? json, string itemId)
        {
            if (string.IsNullOrWhiteSpace(json)) return false;
            try
            {
                using var document = JsonDocument.Parse(json);
                if (document.RootElement.ValueKind != JsonValueKind.Array) return false;
                foreach (var element in document.RootElement.EnumerateArray())
                {
                    if (TryReadStringProperty(element, "Type", out var rewardType) && string.Equals(rewardType, "item", StringComparison.OrdinalIgnoreCase) && TryReadStringProperty(element, "ItemId", out var rewardItemId) && string.Equals(rewardItemId, itemId, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        private static bool ContainsQuestRewardItem(string? json, string itemId)
        {
            if (string.IsNullOrWhiteSpace(json)) return false;
            try
            {
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;
                return root.ValueKind == JsonValueKind.Object && root.TryGetProperty(itemId, out _);
            }
            catch
            {
                return false;
            }
        }

        private static bool TryReadStringProperty(JsonElement element, string propertyName, out string? value)
        {
            value = null;
            foreach (var property in element.EnumerateObject())
            {
                if (!property.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase)) continue;
                value = property.Value.ValueKind == JsonValueKind.String ? property.Value.GetString() : property.Value.GetRawText();
                return !string.IsNullOrWhiteSpace(value);
            }
            return false;
        }
    }
}
#pragma warning restore CS1591
