using Microsoft.Extensions.Logging;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Data;
using XXX.Infrastructure.Repositories;
using XXX.Player;

namespace XXX.Application.Services
{
    /// <summary>
    /// 背包服务实现。
    /// </summary>
    public class InventoryService : IInventoryService
    {
        private const string SkillBookFragmentItemId = "skill_book_fragment";
        private const int SkillBookDecomposeFragments = 5;
        private const string AlchemyRecipeType = "Alchemy";
        private const string ForgeRecipeType = "Forge";
        private const int MaxPetCount = 10;
        private readonly DbContext _dbContext;
        private readonly IRepository<InventoryItemEntity> _inventoryRepository;
        private readonly IRepository<PetInstanceEntity> _petRepository;
        private readonly IRepository<PetTemplateEntity> _petTemplateRepository;
        private readonly IRepository<UserEntity> _userRepository;
        private readonly IRepository<SkillTemplateEntity> _skillTemplateRepository;
        private readonly IRepository<PlayerPillEffectEntity> _pillEffectRepository;
        private readonly IGameSyncService _gameSyncService;
        private readonly IPlayerAttributeService _playerAttributeService;
        private readonly IQuestService _questService;
        private readonly ILogger<InventoryService> _logger;
        private static readonly SemaphoreSlim HatchPetEggLock = new(1, 1);

        /// <summary>
        /// 初始化背包服务。
        /// </summary>
        /// <param name="dbContext">数据库上下文。</param>
        /// <param name="inventoryRepository">背包仓储。</param>
        /// <param name="petRepository">灵宠实例仓储。</param>
        /// <param name="petTemplateRepository">灵宠模板仓储。</param>
        /// <param name="userRepository">玩家仓储。</param>
        /// <param name="cacheService">缓存服务，当前实现保留注入以兼容接口。</param>
        /// <param name="gameSyncService">游戏同步服务。</param>
        /// <param name="playerAttributeService">玩家属性服务。</param>
        /// <param name="questService">任务服务。</param>
        /// <param name="mapper">对象映射器，当前实现保留注入以兼容接口。</param>
        /// <param name="logger">日志记录器。</param>
        public InventoryService(
            DbContext dbContext,
            IRepository<InventoryItemEntity> inventoryRepository,
            IRepository<PetInstanceEntity> petRepository,
            IRepository<PetTemplateEntity> petTemplateRepository,
            IRepository<UserEntity> userRepository,
            IRepository<SkillTemplateEntity> skillTemplateRepository,
            IRepository<PlayerPillEffectEntity> pillEffectRepository,
            Infrastructure.Cache.ICacheService cacheService,
            IGameSyncService gameSyncService,
            IPlayerAttributeService playerAttributeService,
            IQuestService questService,
            AutoMapper.IMapper mapper,
            ILogger<InventoryService> logger)
        {
            _dbContext = dbContext;
            _inventoryRepository = inventoryRepository;
            _petRepository = petRepository;
            _petTemplateRepository = petTemplateRepository;
            _userRepository = userRepository;
            _skillTemplateRepository = skillTemplateRepository;
            _pillEffectRepository = pillEffectRepository;
            _gameSyncService = gameSyncService;
            _playerAttributeService = playerAttributeService;
            _questService = questService;
            _logger = logger;
        }

        /// <summary>
        /// 获取玩家背包物品列表。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <returns>背包物品 DTO 列表。</returns>
        public async Task<List<InventoryItemDto>> GetInventoryItemsAsync(string playerId)
        {
            var items = await _inventoryRepository.GetListAsync(item => item.PlayerId == playerId);
            return items
                .OrderByDescending(item => item.AcquiredTime)
                .Select(item => MapToDto(item))
                .ToList();
        }

        /// <summary>
        /// 获取单个背包物品详情。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="itemInstanceId">背包物品实例编号。</param>
        /// <returns>命中的物品详情；不存在时返回空。</returns>
        public async Task<InventoryItemDto?> GetItemDetailAsync(string playerId, long itemInstanceId)
        {
            var item = await _inventoryRepository.GetFirstAsync(i => i.PlayerId == playerId && i.Id == itemInstanceId);
            if (item == null)
            {
                return null;
            }

            return MapToDto(item);
        }

        /// <summary>
        /// 向玩家背包新增物品。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="request">新增物品请求。</param>
        /// <returns>新增后的物品 DTO。</returns>
        public async Task<InventoryItemDto> AddItemAsync(string playerId, AddItemRequestDto request)
        {
            if (request.Quantity <= 0)
            {
                throw new InvalidOperationException("新增物品数量必须大于 0");
            }

            var player = await _userRepository.GetByIdAsync(playerId);
            if (player == null || player.IsDeleted)
            {
                throw new InvalidOperationException("玩家不存在");
            }

            var existingItem = await InventoryItemGrantHelper.AddOrMergeAsync(
                _dbContext.Db,
                playerId,
                request.ItemId,
                request.Quantity,
                "背包已满");

            _logger.LogInformation(
                "Added inventory item {ItemId} x{Quantity} for player {PlayerId}. locked={IsLocked}, source={Source}, totalQuantity={TotalQuantity}",
                request.ItemId,
                request.Quantity,
                playerId,
                existingItem.IsLocked,
                request.Source,
                existingItem.Quantity);

            await _questService.RecordObjectiveEventAsync(playerId, new XXX.Quest.QuestObjectiveEvent
            {
                ObjectiveType = XXX.Quest.ObjectiveType.CollectItem,
                TargetId = request.ItemId,
                Delta = request.Quantity
            });

            return MapToDto(existingItem);
        }
        /// <summary>
        /// 使用背包物品。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="request">使用物品请求。</param>
        /// <returns>使用结果。</returns>
        public async Task<ItemUseResultDto> UseItemAsync(string playerId, UseItemRequestDto request)
        {
            var item = await _inventoryRepository.GetFirstAsync(i => i.PlayerId == playerId && i.Id == request.ItemId);
            if (item == null || request.Quantity <= 0 || item.Quantity < request.Quantity)
            {
                _logger.LogWarning(
                    "Player {PlayerId} failed to use inventory item instance {ItemInstanceId}. quantity={Quantity}",
                    playerId,
                    request.ItemId,
                    request.Quantity);

                return new ItemUseResultDto
                {
                    Success = false,
                    Message = "物品不存在或数量不足"
                };
            }

            ItemUseResultDto result;
            try
            {
                if (XXX.GameData.Items.TryGetValue(item.ItemId, out var itemTemplate))
                {
                    result = await UseConfiguredItemAsync(playerId, item, itemTemplate, request.Quantity);
                }
                else
                {
                    result = item.ItemId switch
                    {
                        "item_001" => await ConsumeRestoreItemAsync(playerId, item, request.Quantity, 100, 0, "恢复 100 点生命"),
                        "item_002" => await ConsumeRestoreItemAsync(playerId, item, request.Quantity, 300, 0, "恢复 300 点生命"),
                        "pill_hp_small" => await ConsumeRestoreItemAsync(playerId, item, request.Quantity, 500, 0, "恢复 500 点生命"),
                        "pill_mp_small" => await ConsumeRestoreItemAsync(playerId, item, request.Quantity, 0, 200, "恢复 200 点法力"),
                        "pill_hp_large" => await ConsumeRestoreItemAsync(playerId, item, request.Quantity, 1000, 0, "恢复 1000 点生命"),
                        "pet_egg" => await ConsumePetEggAsync(playerId, item, request.Quantity),
                        _ => await ConsumeGenericItemAsync(item, request.Quantity, "当前物品暂无主动使用效果")
                    };
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Player {PlayerId} use inventory item {ItemId} rejected by business rule. quantity={Quantity}",
                    playerId,
                    item.ItemId,
                    request.Quantity);

                return new ItemUseResultDto
                {
                    Success = false,
                    Message = ex.Message,
                    RemainingQuantity = item.Quantity
                };
            }

            if (result.Success)
            {
                _logger.LogInformation(
                    "Player {PlayerId} used inventory item {ItemId} x{Quantity}. remaining={RemainingQuantity}, message={Message}",
                    playerId,
                    item.ItemId,
                    request.Quantity,
                    result.RemainingQuantity,
                    result.Message);
            }
            else
            {
                _logger.LogWarning(
                    "Player {PlayerId} use inventory item {ItemId} failed. quantity={Quantity}, message={Message}",
                    playerId,
                    item.ItemId,
                    request.Quantity,
                    result.Message);
            }

            return result;
        }

        /// <summary>
        /// 在事务内分解技能书并发放通用技能书碎片。
        /// </summary>
        public async Task<SkillBookDecomposeResultDto> DecomposeSkillBookAsync(string playerId, SkillBookDecomposeRequestDto request)
        {
            if (request.ItemId <= 0 || request.Quantity <= 0)
            {
                throw new InvalidOperationException("技能书分解参数无效。");
            }

            var db = _dbContext.Db;
            var transactionStarted = false;
            try
            {
                db.Ado.BeginTran();
                transactionStarted = true;

                var source = await db.Queryable<InventoryItemEntity>()
                    .FirstAsync(item => item.Id == request.ItemId && item.PlayerId == playerId);
                if (source == null || source.Quantity < request.Quantity)
                {
                    throw new InvalidOperationException("技能书数量不足，请刷新后重试。");
                }

                var template = await db.Queryable<ItemTemplateEntity>()
                    .FirstAsync(item => item.ItemId == source.ItemId);
                if (template == null || template.Type != (int)ItemType.SkillBook)
                {
                    throw new InvalidOperationException("只能分解技能书。");
                }

                var fragmentTemplate = await db.Queryable<ItemTemplateEntity>()
                    .FirstAsync(item => item.ItemId == SkillBookFragmentItemId && item.Type == (int)ItemType.Material);
                if (fragmentTemplate == null)
                {
                    throw new InvalidOperationException("技能书碎片模板不存在，请先完成技能数据导入。");
                }

                await ReduceInventoryItemInTransactionAsync(db, source, request.Quantity);
                var fragmentAmount = checked(request.Quantity * SkillBookDecomposeFragments);
                await GrantInventoryItemInTransactionAsync(db, playerId, SkillBookFragmentItemId, fragmentAmount);

                db.Ado.CommitTran();
                transactionStarted = false;

                var balance = await db.Queryable<InventoryItemEntity>()
                    .Where(item => item.PlayerId == playerId && item.ItemId == SkillBookFragmentItemId)
                    .SumAsync(item => item.Quantity);
                var remaining = await db.Queryable<InventoryItemEntity>()
                    .Where(item => item.Id == request.ItemId && item.PlayerId == playerId)
                    .SumAsync(item => item.Quantity);
                await _gameSyncService.SyncPlayerAsync(playerId);

                return new SkillBookDecomposeResultDto
                {
                    DecomposedQuantity = request.Quantity,
                    FragmentItemQuantity = balance,
                    RemainingBookQuantity = remaining
                };
            }
            catch
            {
                if (transactionStarted)
                {
                    db.Ado.RollbackTran();
                }
                throw;
            }
        }

        private async Task<ItemUseResultDto> UseConfiguredItemAsync(
            string playerId,
            InventoryItemEntity item,
            ItemTable itemTemplate,
            int quantity)
        {
            return itemTemplate.Type switch
            {
                ItemType.Material => BuildItemNotUsableResult(item, "材料不能主动使用。"),
                ItemType.Seed => BuildItemNotUsableResult(item, "种子只能在灵田中种植。"),
                ItemType.Quest => BuildItemNotUsableResult(item, "任务道具不能主动使用。"),
                ItemType.Chest => await OpenChestAsync(playerId, item, itemTemplate, quantity),
                ItemType.SkillBook => await ConsumeSkillBookAsync(playerId, item, itemTemplate, quantity),
                ItemType.RecipeScroll => await ConsumeRecipeScrollAsync(playerId, item, itemTemplate, quantity),
                ItemType.InventoryExpansion => await ConsumeInventoryExpansionAsync(playerId, item, quantity),
                ItemType.PetEgg => await ConsumeConfiguredPetEggAsync(playerId, item, itemTemplate, quantity),
                ItemType.Pill => await ConsumeConfiguredPillAsync(playerId, item, itemTemplate, quantity),
                ItemType.Consumable => await ConsumeGenericItemAsync(item, quantity, "当前物品暂无主动使用效果"),
                _ => await ConsumeGenericItemAsync(item, quantity, "当前物品暂无主动使用效果")
            };
        }

        private static ItemUseResultDto BuildItemNotUsableResult(InventoryItemEntity item, string message)
        {
            return new ItemUseResultDto
            {
                Success = false,
                Message = message,
                RemainingQuantity = item.Quantity
            };
        }

        /// <summary>
        /// 消耗配方卷轴并永久解锁对应炼丹或锻造配方。
        /// </summary>
        private async Task<ItemUseResultDto> ConsumeRecipeScrollAsync(
            string playerId,
            InventoryItemEntity item,
            ItemTable itemTemplate,
            int quantity)
        {
            if (quantity != 1)
            {
                throw new InvalidOperationException("配方卷轴每次只能使用 1 张。");
            }

            var db = _dbContext.Db;
            var transactionStarted = false;
            try
            {
                db.Ado.BeginTran();
                transactionStarted = true;

                var player = await db.Queryable<UserEntity>()
                    .FirstAsync(entity => entity.GID == playerId && !entity.IsDeleted);
                var currentItem = await db.Queryable<InventoryItemEntity>()
                    .FirstAsync(entity => entity.Id == item.Id && entity.PlayerId == playerId);
                var unlock = await db.Queryable<ItemRecipeUnlockConfigEntity>()
                    .FirstAsync(entity => entity.ItemId == item.ItemId && entity.IsEnabled);

                if (player == null)
                {
                    throw new InvalidOperationException("玩家不存在。");
                }

                if (currentItem == null || currentItem.Quantity < 1)
                {
                    throw new InvalidOperationException("配方卷轴数量不足，请刷新后重试。");
                }

                if (unlock == null || string.IsNullOrWhiteSpace(unlock.RecipeId))
                {
                    throw new InvalidOperationException("当前配方卷轴尚未配置对应配方。");
                }

                var recipeType = (unlock.RecipeType ?? string.Empty).Trim();
                var recipeId = unlock.RecipeId.Trim();
                var recipeName = string.Empty;
                if (string.Equals(recipeType, AlchemyRecipeType, StringComparison.OrdinalIgnoreCase))
                {
                    var recipe = await db.Queryable<AlchemyRecipeEntity>()
                        .FirstAsync(entity => entity.RecipeId == recipeId);
                    if (recipe == null)
                    {
                        throw new InvalidOperationException("配方卷轴绑定的炼丹配方不存在。");
                    }

                    recipeName = recipe.Name;
                    var system = await db.Queryable<AlchemySystemEntity>()
                        .FirstAsync(entity => entity.PlayerId == playerId);
                    if (system == null)
                    {
                        system = new AlchemySystemEntity
                        {
                            PlayerId = playerId,
                            FurnaceLevel = 1,
                            AlchemistLevel = 1,
                            DailyResetTime = DateTime.Today,
                            LastUpdateTime = DateTime.Now,
                            LearnedRecipes = []
                        };
                        await db.Insertable(system).ExecuteCommandAsync();
                    }

                    var learned = system.LearnedRecipes
                        .Where(id => !string.IsNullOrWhiteSpace(id))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();
                    var alreadyLearned = learned.Contains(recipeId, StringComparer.OrdinalIgnoreCase);
                    if (!alreadyLearned)
                    {
                        learned.Add(recipeId);
                        system.LearnedRecipes = learned;
                        system.LastUpdateTime = DateTime.Now;
                        await db.Updateable(system).ExecuteCommandAsync();
                    }

                    await ReduceInventoryItemInTransactionAsync(db, currentItem, 1);
                    db.Ado.CommitTran();
                    transactionStarted = false;
                    return new ItemUseResultDto
                    {
                        Success = true,
                        Message = alreadyLearned ? $"已拥有丹方《{recipeName}》，卷轴已消耗。" : $"已解锁丹方《{recipeName}》。",
                        RemainingQuantity = Math.Max(0, currentItem.Quantity - 1),
                        Effects = [new() { EffectType = "RECIPE_UNLOCK", Value = 1, Description = alreadyLearned ? $"重复使用：{recipeName}" : $"解锁丹方：{recipeName}" }]
                    };
                }

                if (string.Equals(recipeType, ForgeRecipeType, StringComparison.OrdinalIgnoreCase))
                {
                    var recipe = await db.Queryable<ForgeRecipeEntity>()
                        .FirstAsync(entity => entity.RecipeId == recipeId);
                    if (recipe == null || !recipe.IsEnabled)
                    {
                        throw new InvalidOperationException("配方卷轴绑定的锻造图纸不存在。");
                    }

                    recipeName = recipe.Name;
                    var system = await db.Queryable<ForgeSystemEntity>()
                        .FirstAsync(entity => entity.PlayerId == playerId);
                    if (system == null)
                    {
                        system = new ForgeSystemEntity
                        {
                            PlayerId = playerId,
                            BlacksmithLevel = 1,
                            DailyResetTime = DateTime.Today,
                            LastUpdateTime = DateTime.Now,
                            LearnedRecipes = []
                        };
                        await db.Insertable(system).ExecuteCommandAsync();
                    }

                    var learned = system.LearnedRecipes
                        .Where(id => !string.IsNullOrWhiteSpace(id))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();
                    var alreadyLearned = learned.Contains(recipeId, StringComparer.OrdinalIgnoreCase);
                    if (!alreadyLearned)
                    {
                        learned.Add(recipeId);
                        system.LearnedRecipes = learned;
                        system.LastUpdateTime = DateTime.Now;
                        await db.Updateable(system).ExecuteCommandAsync();
                    }

                    await ReduceInventoryItemInTransactionAsync(db, currentItem, 1);
                    db.Ado.CommitTran();
                    transactionStarted = false;
                    return new ItemUseResultDto
                    {
                        Success = true,
                        Message = alreadyLearned ? $"已拥有图纸《{recipeName}》，图纸已消耗。" : $"已解锁图纸《{recipeName}》。",
                        RemainingQuantity = Math.Max(0, currentItem.Quantity - 1),
                        Effects = [new() { EffectType = "RECIPE_UNLOCK", Value = 1, Description = alreadyLearned ? $"重复使用：{recipeName}" : $"解锁图纸：{recipeName}" }]
                    };
                }

                throw new InvalidOperationException("配方卷轴类型配置无效。");
            }
            catch
            {
                if (transactionStarted)
                {
                    db.Ado.RollbackTran();
                }
                throw;
            }
        }

        private async Task<ItemUseResultDto> OpenChestAsync(
            string playerId,
            InventoryItemEntity item,
            ItemTable itemTemplate,
            int quantity)
        {
            var config = itemTemplate.ChestConfig;
            if (config == null || config.Rewards.Count == 0)
            {
                throw new InvalidOperationException("当前道具箱尚未配置奖励池。");
            }

            var db = _dbContext.Db;
            var transactionStarted = false;
            var grantedItemRewards = new List<(string ItemId, int Count)>();
            var effectDescriptions = new List<ItemEffectDto>();

            try
            {
                db.Ado.BeginTran();
                transactionStarted = true;

                var player = await db.Queryable<UserEntity>()
                    .FirstAsync(entity => entity.GID == playerId && !entity.IsDeleted);
                if (player == null)
                {
                    throw new InvalidOperationException("玩家不存在。");
                }

                var currentItem = await db.Queryable<InventoryItemEntity>()
                    .FirstAsync(entity => entity.Id == item.Id && entity.PlayerId == playerId);
                if (currentItem == null || currentItem.Quantity < quantity)
                {
                    throw new InvalidOperationException("道具箱数量不足，请刷新后重试。");
                }

                await ReduceInventoryItemInTransactionAsync(db, currentItem, quantity);

                for (var openIndex = 0; openIndex < quantity; openIndex++)
                {
                    var currentRewards = config.OpenMode == ChestOpenMode.MultiRoll
                        ? Enumerable.Range(0, Math.Max(1, config.RollCount)).Select(_ => PickChestReward(config.Rewards)).ToList()
                        : new List<ItemChestRewardEntry> { PickChestReward(config.Rewards) };

                    foreach (var reward in currentRewards)
                    {
                        var rewardCount = Random.Shared.Next(Math.Max(1, reward.MinCount), Math.Max(Math.Max(1, reward.MinCount), reward.MaxCount) + 1);
                        switch (reward.RewardType)
                        {
                            case ChestRewardType.Gold:
                                player.Gold += rewardCount;
                                player.TotalGoldEarned += rewardCount;
                                effectDescriptions.Add(BuildChestEffect("GOLD", rewardCount, reward.Description ?? $"获得金币 x{rewardCount}"));
                                break;

                            case ChestRewardType.SpiritStone:
                                player.SpiritStone += rewardCount;
                                effectDescriptions.Add(BuildChestEffect("SPIRIT_STONE", rewardCount, reward.Description ?? $"获得灵石 x{rewardCount}"));
                                break;

                            case ChestRewardType.Item:
                                if (string.IsNullOrWhiteSpace(reward.TargetId))
                                {
                                    throw new InvalidOperationException("道具箱奖励配置缺少道具编号。");
                                }

                                await GrantInventoryItemInTransactionAsync(db, playerId, reward.TargetId.Trim(), rewardCount);
                                grantedItemRewards.Add((reward.TargetId.Trim(), rewardCount));
                                effectDescriptions.Add(BuildChestEffect("ITEM", rewardCount, reward.Description ?? $"获得道具 {ResolveItemName(reward.TargetId)} x{rewardCount}"));
                                break;

                            case ChestRewardType.Equipment:
                                if (string.IsNullOrWhiteSpace(reward.TargetId) || !int.TryParse(reward.TargetId, out var equipmentId))
                                {
                                    throw new InvalidOperationException("道具箱奖励配置缺少装备模板编号。");
                                }

                                if (!XXX.GameData.EquipmentTemplates.TryGetValue(equipmentId, out var equipmentTemplate))
                                {
                                    throw new InvalidOperationException($"装备模板 {reward.TargetId} 不存在。");
                                }

                                if (!await InventoryCapacityRules.HasEquipmentSlotsAsync(db, playerId, rewardCount))
                                {
                                    throw new InvalidOperationException("装备背包已满，无法领取道具箱奖励。");
                                }

                                for (var index = 0; index < rewardCount; index++)
                                {
                                    var equipmentEntity = EquipmentBalanceHelper.CreateEntity(playerId, equipmentTemplate, false);
                                    await db.Insertable(equipmentEntity).ExecuteCommandAsync();
                                }

                                effectDescriptions.Add(BuildChestEffect("EQUIPMENT", rewardCount, reward.Description ?? $"获得装备 {equipmentTemplate.Name} x{rewardCount}"));
                                break;

                            default:
                                throw new InvalidOperationException("当前道具箱奖励类型暂不支持。");
                        }
                    }
                }

                player.LastUpdateTime = DateTime.Now;
                await db.Updateable(player).ExecuteCommandAsync();

                db.Ado.CommitTran();
                transactionStarted = false;

                foreach (var reward in grantedItemRewards)
                {
                    await _questService.RecordObjectiveEventAsync(playerId, new XXX.Quest.QuestObjectiveEvent
                    {
                        ObjectiveType = XXX.Quest.ObjectiveType.CollectItem,
                        TargetId = reward.ItemId,
                        Delta = reward.Count
                    });
                }

                await _gameSyncService.SyncPlayerAsync(playerId);

                item.Quantity = Math.Max(0, currentItem.Quantity - quantity);
                return new ItemUseResultDto
                {
                    Success = true,
                    Message = effectDescriptions.Count == 0
                        ? $"成功开启 {itemTemplate.Name}。"
                        : $"成功开启 {itemTemplate.Name}，获得 {string.Join("、", effectDescriptions.Select(effect => effect.Description).Take(4))}{(effectDescriptions.Count > 4 ? " 等奖励" : string.Empty)}。",
                    RemainingQuantity = item.Quantity,
                    Effects = effectDescriptions
                };
            }
            catch
            {
                if (transactionStarted)
                {
                    db.Ado.RollbackTran();
                }

                throw;
            }
        }

        /// <summary>
        /// 查找玩家在同一条技能链中已掌握的最高等级技能。
        /// </summary>
        private async Task<SkillTemplateEntity?> GetHighestOwnedSkillInChainAsync(UserEntity player, SkillTemplateEntity target)
        {
            var ownedIds = (player.OwnedSkillIds ?? []).ToHashSet(StringComparer.Ordinal);
            var skills = await _skillTemplateRepository.GetAllAsync();
            var current = target;
            while (current.PreviousSkillId.HasValue)
            {
                var previous = skills.FirstOrDefault(skill => skill.SkillId == current.PreviousSkillId.Value);
                if (previous == null) break;
                current = previous;
            }

            SkillTemplateEntity? highest = null;
            while (current != null)
            {
                if (ownedIds.Contains(current.SkillId.ToString()) && (highest == null || current.SkillLevel > highest.SkillLevel))
                {
                    highest = current;
                }
                current = current.NextSkillId.HasValue
                    ? skills.FirstOrDefault(skill => skill.SkillId == current.NextSkillId.Value)
                    : null;
            }

            return highest;
        }

        /// <summary>
        /// 消耗并学习技能书中的技能。
        /// </summary>
        private async Task<ItemUseResultDto> ConsumeSkillBookAsync(
            string playerId,
            InventoryItemEntity item,
            ItemTable itemTemplate,
            int quantity)
        {
            if (quantity != 1)
            {
                throw new InvalidOperationException("技能书每次只能使用 1 本。");
            }

            var config = itemTemplate.SkillBookConfig;
            if (config == null || config.SkillId <= 0)
            {
                throw new InvalidOperationException("当前技能书尚未配置对应技能。");
            }

            var db = _dbContext.Db;
            var transactionStarted = false;
            try
            {
                db.Ado.BeginTran();
                transactionStarted = true;
                var player = await db.Queryable<UserEntity>().FirstAsync(entity => entity.GID == playerId && !entity.IsDeleted);
                var currentItem = await db.Queryable<InventoryItemEntity>().FirstAsync(entity => entity.Id == item.Id && entity.PlayerId == playerId);
                var skillTemplate = await db.Queryable<SkillTemplateEntity>().FirstAsync(skill => skill.SkillId == config.SkillId);
                if (player == null) throw new InvalidOperationException("玩家不存在。");
                if (currentItem == null || currentItem.Quantity < quantity) throw new InvalidOperationException("技能书数量不足，请刷新后重试。");
                if (skillTemplate == null) throw new InvalidOperationException("技能书配置的技能不存在。");

                var highestOwnedSkill = await GetHighestOwnedSkillInChainAsync(player, skillTemplate);
                var fragmentTemplate = await db.Queryable<ItemTemplateEntity>()
                    .FirstAsync(entity => entity.ItemId == SkillBookFragmentItemId && entity.Type == (int)ItemType.Material);
                if (fragmentTemplate == null) throw new InvalidOperationException("技能书碎片模板不存在，请先完成技能数据导入。");

                await ReduceInventoryItemInTransactionAsync(db, currentItem, 1);
                if (highestOwnedSkill != null && highestOwnedSkill.SkillLevel >= skillTemplate.SkillLevel)
                {
                    await GrantInventoryItemInTransactionAsync(db, playerId, SkillBookFragmentItemId, SkillBookDecomposeFragments);
                    db.Ado.CommitTran();
                    transactionStarted = false;
                    item.Quantity = Math.Max(0, currentItem.Quantity - 1);
                    await _gameSyncService.SyncPlayerAsync(playerId);
                    return new ItemUseResultDto
                    {
                        Success = true,
                        Message = $"已拥有更高或相同等级技能：{highestOwnedSkill.Name} Lv.{highestOwnedSkill.SkillLevel}，技能书已转换为 {SkillBookDecomposeFragments} 个技能书碎片。",
                        RemainingQuantity = item.Quantity,
                        Effects = [new() { EffectType = "SKILL_BOOK_FRAGMENT", Value = SkillBookDecomposeFragments, Description = $"获得技能书碎片 ×{SkillBookDecomposeFragments}" }]
                    };
                }

                if (!PlayerProfessionCatalog.Allows(player.Profession, skillTemplate.AllowedProfessions))
                {
                    throw new InvalidOperationException($"当前职业无法学习该技能，仅 {PlayerProfessionCatalog.GetAllowedDisplayName(skillTemplate.AllowedProfessions)} 可用。");
                }

                var ownedSkillIds = (player.OwnedSkillIds ?? [])
                    .Select(skillId => skillId?.Trim())
                    .Where(skillId => !string.IsNullOrWhiteSpace(skillId))
                    .Distinct(StringComparer.Ordinal)
                    .Select(skillId => skillId!)
                    .ToList();
                var normalizedSkillId = config.SkillId.ToString();
                if (ownedSkillIds.Contains(normalizedSkillId, StringComparer.Ordinal))
                {
                    await GrantInventoryItemInTransactionAsync(db, playerId, SkillBookFragmentItemId, SkillBookDecomposeFragments);
                    db.Ado.CommitTran();
                    transactionStarted = false;
                    item.Quantity = Math.Max(0, currentItem.Quantity - 1);
                    await _gameSyncService.SyncPlayerAsync(playerId);
                    return new ItemUseResultDto
                    {
                        Success = true,
                        Message = $"你已掌握该技能，技能书已转换为 {SkillBookDecomposeFragments} 个技能书碎片。",
                        RemainingQuantity = item.Quantity,
                        Effects = [new() { EffectType = "SKILL_BOOK_FRAGMENT", Value = SkillBookDecomposeFragments, Description = $"获得技能书碎片 ×{SkillBookDecomposeFragments}" }]
                    };
                }

                ownedSkillIds.Add(normalizedSkillId);
                player.OwnedSkillIds = ownedSkillIds;
                player.LastUpdateTime = DateTime.Now;
                await db.Updateable(player).ExecuteCommandAsync();
                db.Ado.CommitTran();
                transactionStarted = false;
                item.Quantity = Math.Max(0, currentItem.Quantity - 1);
                await _gameSyncService.SyncPlayerAsync(playerId);
                return new ItemUseResultDto
                {
                    Success = true,
                    Message = $"成功学习技能：{skillTemplate.Name}。",
                    RemainingQuantity = item.Quantity,
                    Effects = [new() { EffectType = "SKILL_LEARN", Value = config.SkillId, Description = $"学会技能 {skillTemplate.Name}" }]
                };
            }
            catch
            {
                if (transactionStarted) db.Ado.RollbackTran();
                throw;
            }
        }

        private async Task<ItemUseResultDto> ConsumeConfiguredPetEggAsync(
            string playerId,
            InventoryItemEntity item,
            ItemTable itemTemplate,
            int quantity)
        {
            var config = itemTemplate.PetEggConfig;
            if (config == null || string.IsNullOrWhiteSpace(config.PetTemplateId))
            {
                return await ConsumePetEggAsync(playerId, item, quantity);
            }

            await HatchPetEggLock.WaitAsync();
            var db = _dbContext.Db;
            var transactionStarted = false;

            try
            {
                var template = await _petTemplateRepository.GetFirstAsync(entity => entity.TemplateId == config.PetTemplateId);
                if (template == null)
                {
                    throw new InvalidOperationException("宠物蛋配置的灵宠模板不存在。");
                }

                db.Ado.BeginTran();
                transactionStarted = true;

                var currentPetCount = await db.Queryable<PetInstanceEntity>()
                    .CountAsync(pet => pet.PlayerId == playerId);
                if (currentPetCount + quantity > MaxPetCount)
                {
                    throw new InvalidOperationException($"灵宠栏位不足，当前最多只能拥有 {MaxPetCount} 只灵宠。");
                }

                var currentItem = await db.Queryable<InventoryItemEntity>()
                    .FirstAsync(entity => entity.Id == item.Id && entity.PlayerId == playerId);
                if (currentItem == null || currentItem.Quantity < quantity)
                {
                    throw new InvalidOperationException("宠物蛋数量不足，请刷新后重试。");
                }

                await ReduceInventoryItemInTransactionAsync(db, currentItem, quantity);

                var acquiredPets = new List<PetInstanceEntity>(quantity);
                for (var index = 0; index < quantity; index++)
                {
                    acquiredPets.Add(PetGenerationHelper.GeneratePetInstanceFromTemplate(playerId, template, DateTime.Now));
                }

                await db.Insertable(acquiredPets).ExecuteCommandAsync();
                db.Ado.CommitTran();
                transactionStarted = false;

                item.Quantity = Math.Max(0, currentItem.Quantity - quantity);
                return new ItemUseResultDto
                {
                    Success = true,
                    Message = quantity == 1
                        ? $"成功孵化灵宠：{acquiredPets[0].Name}。"
                        : $"成功孵化 {quantity} 只灵宠：{template.Name}。",
                    RemainingQuantity = item.Quantity,
                    Effects =
                    [
                        new()
                        {
                            EffectType = "PET_ACQUIRE",
                            Value = quantity,
                            Description = quantity == 1
                                ? $"获得灵宠 {acquiredPets[0].Name}"
                                : $"获得 {quantity} 只 {template.Name}"
                        }
                    ]
                };
            }
            catch
            {
                if (transactionStarted)
                {
                    db.Ado.RollbackTran();
                }

                throw;
            }
            finally
            {
                HatchPetEggLock.Release();
            }
        }

        private async Task<ItemUseResultDto> ConsumeConfiguredPillAsync(
            string playerId,
            InventoryItemEntity item,
            ItemTable itemTemplate,
            int quantity)
        {
            var config = itemTemplate.PillConfig;
            if (config == null)
                throw new InvalidOperationException("当前丹药尚未配置使用效果。");

            var player = await _userRepository.GetByIdAsync(playerId);
            if (player == null || player.IsDeleted)
                throw new InvalidOperationException("玩家不存在。");

            // 增加经验值不允许配置时效，强制为 0
            if (config.EffectType == ItemPillEffectType.AddExp && config.DurationMinutes > 0)
                config.DurationMinutes = 0;

            // 查询该玩家对该丹药的使用记录
            var record = await _pillEffectRepository.GetFirstAsync(
                r => r.PlayerId == playerId && r.ItemId == itemTemplate.ItemId);

            // 使用次数检查
            if (config.MaxUsageCount > 0)
            {
                var currentUsage = record?.UsageCount ?? 0;
                if (currentUsage + quantity > config.MaxUsageCount)
                    throw new InvalidOperationException("该丹药已达到使用次数上限");
            }

            string effectDescription;
            var now = DateTime.Now;

            switch (config.EffectType)
            {
                case ItemPillEffectType.BreakthroughChance:
                {
                    var bonusValue = Math.Max(0, config.BreakthroughBonusPercent) * quantity;

                    if (config.DurationMinutes > 0)
                    {
                        var newDuration = TimeSpan.FromMinutes(config.DurationMinutes * quantity);
                        if (record != null && record.IsTemporary)
                        {
                            record.ExpiresAt = record.ExpiresAt!.Value + newDuration;
                            record.BonusValue += bonusValue;
                            record.LastUpdateTime = now;
                            await _pillEffectRepository.UpdateAsync(record);
                        }
                        else
                        {
                            await _pillEffectRepository.AddAsync(new PlayerPillEffectEntity
                            {
                                PlayerId = playerId,
                                ItemId = itemTemplate.ItemId,
                                EffectType = (int)ItemPillEffectType.BreakthroughChance,
                                BonusType = "突破概率",
                                BonusValue = bonusValue,
                                UsageCount = quantity,
                                IsTemporary = true,
                                ExpiresAt = now + newDuration,
                                CreatedAt = now,
                                LastUpdateTime = now
                            });
                        }
                        player.BreakthroughBonusPercent = Math.Max(0, player.BreakthroughBonusPercent) + bonusValue;
                    }
                    else
                    {
                        player.BreakthroughBonusPercent = Math.Max(0, player.BreakthroughBonusPercent) + bonusValue;
                    }

                    effectDescription = config.DurationMinutes > 0
                        ? $"限时突破成功率提高 {bonusValue}%，持续 {config.DurationMinutes * quantity} 分钟"
                        : $"下次突破成功率提高 {bonusValue}%";
                    break;
                }

                case ItemPillEffectType.AddExp:
                {
                    var totalExp = Math.Max(0, config.ExpGain) * quantity;
                    var addExpResult = PlayerManager.AddExp(player, totalExp);
                    if (!addExpResult.Success)
                        throw new InvalidOperationException(addExpResult.Message);
                    player.XExp = PlayerManager.GetRequiredExp(player);
                    effectDescription = $"获得修为 {totalExp}";
                    break;
                }

                case ItemPillEffectType.AddAttribute:
                {
                    if (!config.AttributeType.HasValue || config.AttributeValue <= 0)
                        throw new InvalidOperationException("当前丹药属性配置不完整。");

                    var totalAttributeValue = (float)(config.AttributeValue * quantity);

                    if (config.DurationMinutes > 0)
                    {
                        var newDuration = TimeSpan.FromMinutes(config.DurationMinutes * quantity);
                        if (record != null && record.IsTemporary)
                        {
                            record.ExpiresAt = record.ExpiresAt!.Value + newDuration;
                            record.LastUpdateTime = now;
                            await _pillEffectRepository.UpdateAsync(record);
                        }
                        else
                        {
                            var pillBonuses = player.PillBonusAttributes ?? [];
                            var targetBonus = pillBonuses.FirstOrDefault(
                                b => b.type == config.AttributeType.Value);
                            if (targetBonus == null)
                            {
                                pillBonuses.Add(new AttributeProperty
                                {
                                    type = config.AttributeType.Value,
                                    value = totalAttributeValue
                                });
                            }
                            else
                            {
                                targetBonus.value += totalAttributeValue;
                            }
                            player.PillBonusAttributes = pillBonuses;
                            _logger.LogInformation(
                                "临时属性丹药: PlayerId={PlayerId}, AttrType={AttrType}, Value={Value}, PillBonusJson={Json}",
                                playerId, config.AttributeType.Value, totalAttributeValue, player.PillBonusAttributesJson);

                            await _pillEffectRepository.AddAsync(new PlayerPillEffectEntity
                            {
                                PlayerId = playerId,
                                ItemId = itemTemplate.ItemId,
                                EffectType = (int)ItemPillEffectType.AddAttribute,
                                BonusType = ResolveAttributeName(config.AttributeType.Value),
                                BonusValue = config.AttributeValue,
                                UsageCount = quantity,
                                IsTemporary = true,
                                ExpiresAt = now + newDuration,
                                CreatedAt = now,
                                LastUpdateTime = now
                            });
                        }
                    }
                    else
                    {
                        var pillBonuses = player.PillBonusAttributes ?? [];
                        var targetBonus = pillBonuses.FirstOrDefault(
                            b => b.type == config.AttributeType.Value);
                        if (targetBonus == null)
                        {
                            pillBonuses.Add(new AttributeProperty
                            {
                                type = config.AttributeType.Value,
                                value = totalAttributeValue
                            });
                        }
                        else
                        {
                            targetBonus.value += totalAttributeValue;
                        }
                        player.PillBonusAttributes = pillBonuses;
                    }

                    var attrName = ResolveAttributeName(config.AttributeType.Value);
                    effectDescription = config.DurationMinutes > 0
                        ? $"限时提升 {attrName} {config.AttributeValue * quantity}，持续 {config.DurationMinutes * quantity} 分钟"
                        : $"永久提升 {attrName} {config.AttributeValue * quantity}";
                    break;
                }

                case ItemPillEffectType.RestoreHPMP:
                {
                    if (config.HealHpPercent <= 0 && config.HealMpPercent <= 0)
                        throw new InvalidOperationException("恢复HP/MP类型的丹药必须配置恢复百分比。");

                    var hpHeal = (int)(player.Type1 * config.HealHpPercent / 100.0) * quantity;
                    var mpHeal = (int)(player.Type2 * config.HealMpPercent / 100.0) * quantity;
                    if (hpHeal > 0) await _playerAttributeService.RestoreHPAsync(playerId, hpHeal);
                    if (mpHeal > 0) await _playerAttributeService.RestoreMPAsync(playerId, mpHeal);

                    var parts = new List<string>();
                    if (hpHeal > 0) parts.Add($"恢复HP {hpHeal}");
                    if (mpHeal > 0) parts.Add($"恢复MP {mpHeal}");
                    effectDescription = string.Join("，", parts);
                    break;
                }

                default:
                    throw new InvalidOperationException($"未知的丹药效果类型: {config.EffectType}");
            }

            // 更新使用次数
            if (record == null)
            {
                if (config.DurationMinutes == 0)
                {
                    await _pillEffectRepository.AddAsync(new PlayerPillEffectEntity
                    {
                        PlayerId = playerId,
                        ItemId = itemTemplate.ItemId,
                        EffectType = (int)config.EffectType,
                        BonusType = config.EffectType switch
                        {
                            ItemPillEffectType.BreakthroughChance => "突破概率",
                            ItemPillEffectType.AddExp => "经验值",
                            ItemPillEffectType.AddAttribute => config.AttributeType.HasValue
                                ? ResolveAttributeName(config.AttributeType.Value) : "",
                            ItemPillEffectType.RestoreHPMP => "恢复HP/MP",
                            _ => ""
                        },
                        BonusValue = config.EffectType switch
                        {
                            ItemPillEffectType.BreakthroughChance => config.BreakthroughBonusPercent,
                            ItemPillEffectType.AddExp => config.ExpGain,
                            ItemPillEffectType.AddAttribute => config.AttributeValue,
                            ItemPillEffectType.RestoreHPMP => config.HealHpPercent,
                            _ => 0
                        },
                        UsageCount = quantity,
                        IsTemporary = false,
                        ExpiresAt = null,
                        CreatedAt = now,
                        LastUpdateTime = now
                    });
                }
                else
                {
                    var newRecord = await _pillEffectRepository.GetFirstAsync(
                        r => r.PlayerId == playerId && r.ItemId == itemTemplate.ItemId);
                    if (newRecord != null)
                    {
                        newRecord.UsageCount = quantity;
                        await _pillEffectRepository.UpdateAsync(newRecord);
                    }
                }
            }
            else
            {
                record.UsageCount += quantity;
                record.LastUpdateTime = now;
                await _pillEffectRepository.UpdateAsync(record);
            }

            // 保存并同步
            player.LastUpdateTime = DateTime.Now;
            await ReduceItemQuantityAsync(item, quantity);
            await _userRepository.UpdateAsync(player);

            if (config.EffectType == ItemPillEffectType.AddExp || config.EffectType == ItemPillEffectType.AddAttribute)
            {
                await _playerAttributeService.RecalculatePlayerAttributesAsync(
                    playerId,
                    syncLevelDrivenProgress: config.EffectType == ItemPillEffectType.AddExp);
            }
            else
            {
                await _gameSyncService.SyncPlayerAsync(playerId);
            }

            return new ItemUseResultDto
            {
                Success = true,
                Message = $"成功服用 {itemTemplate.Name}，{effectDescription}。",
                RemainingQuantity = item.Quantity,
                Effects =
                [
                    new()
                    {
                        EffectType = $"PILL_{config.EffectType}",
                        Value = config.EffectType == ItemPillEffectType.AddAttribute
                            ? config.AttributeValue * quantity
                            : config.EffectType == ItemPillEffectType.AddExp
                                ? config.ExpGain * quantity
                                : config.BreakthroughBonusPercent * quantity,
                        Description = effectDescription
                    }
                ],
                UsageCount = record != null ? record.UsageCount + quantity : quantity,
                MaxUsageCount = config.MaxUsageCount
            };
        }

        /// <summary>
        /// 丢弃背包物品。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="request">丢弃请求。</param>
        /// <returns>丢弃成功返回真。</returns>
        public async Task<bool> DiscardItemAsync(string playerId, DiscardItemRequestDto request)
        {
            var item = await _inventoryRepository.GetFirstAsync(i => i.PlayerId == playerId && i.Id == request.ItemId);
            if (item == null || request.Quantity <= 0 || item.Quantity < request.Quantity || item.IsLocked)
            {
                _logger.LogWarning(
                    "Player {PlayerId} failed to discard inventory item instance {ItemInstanceId}. quantity={Quantity}",
                    playerId,
                    request.ItemId,
                    request.Quantity);
                return false;
            }

            item.Quantity -= request.Quantity;
            if (item.Quantity <= 0)
            {
                await _inventoryRepository.DeleteAsync(item);
            }
            else
            {
                await _inventoryRepository.UpdateAsync(item);
            }

            _logger.LogInformation(
                "Player {PlayerId} discarded inventory item {ItemId} x{Quantity}. remaining={RemainingQuantity}",
                playerId,
                item.ItemId,
                request.Quantity,
                Math.Max(item.Quantity, 0));

            return true;
        }

        /// <summary>
        /// 在一个请求、一个事务内批量丢弃背包道具，避免前端逐件请求。
        /// </summary>
        public async Task<BatchDiscardItemResultDto> DiscardItemsAsync(string playerId, BatchDiscardItemRequestDto request)
        {
            var result = new BatchDiscardItemResultDto();
            var entries = (request?.Items ?? [])
                .Where(item => item.ItemId > 0 && item.Quantity > 0)
                .GroupBy(item => item.ItemId)
                .Select(group => new BatchDiscardItemEntryDto
                {
                    ItemId = group.Key,
                    Quantity = group.Sum(item => item.Quantity)
                })
                .ToList();

            if (entries.Count == 0)
            {
                return result;
            }

            var itemIds = entries.Select(item => item.ItemId).ToList();
            var inventoryItems = await _inventoryRepository.GetListAsync(item =>
                item.PlayerId == playerId && itemIds.Contains(item.Id));

            try
            {
                _dbContext.Db.Ado.BeginTran();

                foreach (var entry in entries)
                {
                    var inventoryItem = inventoryItems.FirstOrDefault(item => item.Id == entry.ItemId);
                    if (inventoryItem == null || inventoryItem.IsLocked || inventoryItem.Quantity < entry.Quantity)
                    {
                        result.FailedItemCount++;
                        continue;
                    }

                    if (entry.Quantity == inventoryItem.Quantity)
                    {
                        var deleted = await _dbContext.Db.Deleteable<InventoryItemEntity>()
                            .Where(item => item.Id == entry.ItemId && item.PlayerId == playerId && !item.IsLocked && item.Quantity == inventoryItem.Quantity)
                            .ExecuteCommandAsync();
                        if (deleted == 0)
                        {
                            result.FailedItemCount++;
                            continue;
                        }
                    }
                    else
                    {
                        var updated = await _dbContext.Db.Updateable<InventoryItemEntity>()
                            .SetColumns(item => item.Quantity == item.Quantity - entry.Quantity)
                            .Where(item => item.Id == entry.ItemId && item.PlayerId == playerId && !item.IsLocked && item.Quantity >= entry.Quantity)
                            .ExecuteCommandAsync();
                        if (updated == 0)
                        {
                            result.FailedItemCount++;
                            continue;
                        }
                    }

                    result.DiscardedItemCount++;
                    result.DiscardedQuantity += entry.Quantity;
                }

                _dbContext.Db.Ado.CommitTran();
            }
            catch (Exception ex)
            {
                _dbContext.Db.Ado.RollbackTran();
                _logger.LogError(ex, "Player {PlayerId} batch discard failed", playerId);
                return new BatchDiscardItemResultDto { FailedItemCount = entries.Count };
            }

            return result;
        }

        public async Task<LockItemResultDto> LockItemAsync(string playerId, LockItemRequestDto request)
        {
            var item = await _inventoryRepository.GetFirstAsync(i => i.PlayerId == playerId && i.Id == request.ItemId);
            if (item == null)
            {
                return new LockItemResultDto { Success = false, Message = "物品不存在" };
            }

            if (item.IsLocked)
            {
                return new LockItemResultDto { Success = true, Message = "该物品已经锁定" };
            }

            item.IsLocked = true;
            await _inventoryRepository.UpdateAsync(item);
            _logger.LogInformation("Player {PlayerId} locked inventory item {ItemId}", playerId, item.ItemId);

            return new LockItemResultDto { Success = true, Message = "锁定成功" };
        }

        public async Task<LockItemResultDto> UnlockItemAsync(string playerId, LockItemRequestDto request)
        {
            var item = await _inventoryRepository.GetFirstAsync(i => i.PlayerId == playerId && i.Id == request.ItemId);
            if (item == null)
            {
                return new LockItemResultDto { Success = false, Message = "物品不存在" };
            }

            if (!item.IsLocked)
            {
                return new LockItemResultDto { Success = true, Message = "该物品未锁定" };
            }

            item.IsLocked = false;
            await _inventoryRepository.UpdateAsync(item);
            _logger.LogInformation("Player {PlayerId} unlocked inventory item {ItemId}", playerId, item.ItemId);

            return new LockItemResultDto { Success = true, Message = "解锁成功" };
        }

        /// <summary>
        /// 整理背包并合并可堆叠物品。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <returns>整理前后数量统计。</returns>
        public async Task<InventoryOrganizeResultDto> OrganizeInventoryAsync(string playerId)
        {
            var items = await _inventoryRepository.GetListAsync(item => item.PlayerId == playerId);
            var beforeCount = items.Count;

            var groups = items
                .GroupBy(item => item.ItemId)
                .ToList();

            foreach (var group in groups)
            {
                var keeper = group.OrderBy(item => item.Id).First();
                var totalQuantity = group.Sum(item => item.Quantity);
                keeper.Quantity = totalQuantity;
                keeper.IsLocked = group.Any(item => item.IsLocked);
                await _inventoryRepository.UpdateAsync(keeper);

                foreach (var redundant in group.Where(item => item.Id != keeper.Id))
                {
                    await _inventoryRepository.DeleteAsync(redundant);
                }
            }

            return new InventoryOrganizeResultDto
            {
                BeforeCount = beforeCount,
                AfterCount = groups.Count,
                MergedGroups = beforeCount - groups.Count
            };
        }

        /// <summary>
        /// 获取玩家某种道具的总数量。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="itemId">道具编号。</param>
        /// <returns>该道具在背包中的总数量。</returns>
        public async Task<int> GetItemCountAsync(string playerId, string itemId)
        {
            var items = await _inventoryRepository.GetListAsync(item => item.PlayerId == playerId && item.ItemId == itemId);
            return items.Sum(item => item.Quantity);
        }

        /// <summary>
        /// 从玩家背包扣除指定数量道具。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="itemId">道具编号。</param>
        /// <param name="quantity">扣除数量。</param>
        /// <param name="reason">扣除原因。</param>
        /// <returns>扣除成功返回真。</returns>
        public async Task<bool> DeductItemAsync(string playerId, string itemId, int quantity, string reason)
        {
            if (quantity <= 0)
            {
                return false;
            }

            var items = await _dbContext.Db.Queryable<InventoryItemEntity>()
                .Where(item => item.PlayerId == playerId && item.ItemId == itemId)
                .OrderBy(item => item.Id)
                .OrderBy(item => item.Id)
                .ToListAsync();

            var totalQuantity = items.Sum(item => item.Quantity);
            if (totalQuantity < quantity)
            {
                return false;
            }

            try
            {
                _dbContext.Db.Ado.BeginTran();

                var remaining = quantity;
                foreach (var item in items)
                {
                    if (remaining <= 0)
                    {
                        break;
                    }

                    var deductCount = Math.Min(item.Quantity, remaining);
                    if (deductCount == item.Quantity)
                    {
                        await _dbContext.Db.Deleteable<InventoryItemEntity>()
                            .Where(entity => entity.Id == item.Id && entity.Quantity == item.Quantity)
                            .ExecuteCommandAsync();
                    }
                    else
                    {
                        await _dbContext.Db.Updateable<InventoryItemEntity>()
                            .SetColumns(entity => entity.Quantity == entity.Quantity - deductCount)
                            .Where(entity => entity.Id == item.Id && entity.Quantity >= deductCount)
                            .ExecuteCommandAsync();
                    }

                    remaining -= deductCount;
                }

                _dbContext.Db.Ado.CommitTran();
                return true;
            }
            catch (Exception ex)
            {
                _dbContext.Db.Ado.RollbackTran();
                _logger.LogWarning(ex, "扣除背包物品失败：{PlayerId}-{ItemId}", playerId, itemId);
                return false;
            }
        }

        /// <summary>
        /// 消耗恢复类道具。
        /// </summary>
        private async Task<ItemUseResultDto> ConsumeRestoreItemAsync(
            string playerId,
            InventoryItemEntity item,
            int quantity,
            int hpRestore,
            int mpRestore,
            string message)
        {
            if (hpRestore > 0)
            {
                await _playerAttributeService.RestoreHPAsync(playerId, hpRestore * quantity);
            }

            if (mpRestore > 0)
            {
                await _playerAttributeService.RestoreMPAsync(playerId, mpRestore * quantity);
            }

            await ReduceItemQuantityAsync(item, quantity);

            return new ItemUseResultDto
            {
                Success = true,
                Message = message,
                RemainingQuantity = Math.Max(0, item.Quantity),
                Effects =
                [
                    new()
                    {
                        EffectType = hpRestore > 0 ? "HP_RESTORE" : "MP_RESTORE",
                        Value = hpRestore > 0 ? hpRestore * quantity : mpRestore * quantity,
                        Description = message
                    }
                ]
            };
        }

        /// <summary>
        /// 消耗普通道具。
        /// </summary>
        private async Task<ItemUseResultDto> ConsumeGenericItemAsync(
            InventoryItemEntity item,
            int quantity,
            string message)
        {
            await ReduceItemQuantityAsync(item, quantity);

            return new ItemUseResultDto
            {
                Success = true,
                Message = message,
                RemainingQuantity = Math.Max(0, item.Quantity)
            };
        }

        /// <summary>
        /// 使用纳物令，同时扩展装备背包和道具背包容量。
        /// </summary>
        private async Task<ItemUseResultDto> ConsumeInventoryExpansionAsync(
            string playerId,
            InventoryItemEntity item,
            int quantity)
        {
            var db = _dbContext.Db;
            var transactionStarted = false;
            try
            {
                db.Ado.BeginTran();
                transactionStarted = true;

                var player = await db.Queryable<UserEntity>()
                    .FirstAsync(entity => entity.GID == playerId && !entity.IsDeleted);
                var currentItem = await db.Queryable<InventoryItemEntity>()
                    .FirstAsync(entity => entity.Id == item.Id && entity.PlayerId == playerId);
                if (player == null || currentItem == null || currentItem.Quantity < quantity)
                {
                    throw new InvalidOperationException("纳物令数量不足，请刷新后重试。");
                }

                var currentEquipmentCapacity = Math.Clamp(
                    player.EquipmentInventoryCapacity,
                    InventoryCapacityRules.DefaultCapacity,
                    InventoryCapacityRules.MaximumCapacity);
                var currentItemCapacity = Math.Clamp(
                    player.ItemInventoryCapacity,
                    InventoryCapacityRules.DefaultCapacity,
                    InventoryCapacityRules.MaximumCapacity);
                var equipmentUsesAvailable = (InventoryCapacityRules.MaximumCapacity - currentEquipmentCapacity + InventoryCapacityRules.ExpansionSlots - 1)
                    / InventoryCapacityRules.ExpansionSlots;
                var itemUsesAvailable = (InventoryCapacityRules.MaximumCapacity - currentItemCapacity + InventoryCapacityRules.ExpansionSlots - 1)
                    / InventoryCapacityRules.ExpansionSlots;
                var effectiveQuantity = Math.Min(quantity, Math.Max(equipmentUsesAvailable, itemUsesAvailable));
                if (effectiveQuantity <= 0)
                {
                    throw new InvalidOperationException("装备背包和道具背包都已达到容量上限。");
                }

                var nextEquipmentCapacity = Math.Min(
                    InventoryCapacityRules.MaximumCapacity,
                    currentEquipmentCapacity + effectiveQuantity * InventoryCapacityRules.ExpansionSlots);
                var nextItemCapacity = Math.Min(
                    InventoryCapacityRules.MaximumCapacity,
                    currentItemCapacity + effectiveQuantity * InventoryCapacityRules.ExpansionSlots);

                player.EquipmentInventoryCapacity = nextEquipmentCapacity;
                player.ItemInventoryCapacity = nextItemCapacity;
                player.LastUpdateTime = DateTime.Now;
                await db.Updateable(player).ExecuteCommandAsync();
                await ReduceInventoryItemInTransactionAsync(db, currentItem, effectiveQuantity);

                db.Ado.CommitTran();
                transactionStarted = false;
                await _gameSyncService.SyncPlayerAsync(playerId);

                var equipmentIncrease = nextEquipmentCapacity - currentEquipmentCapacity;
                var itemIncrease = nextItemCapacity - currentItemCapacity;
                var message = effectiveQuantity == quantity
                    ? $"使用纳物令成功，装备背包 +{equipmentIncrease} 格，道具背包 +{itemIncrease} 格。"
                    : $"使用纳物令成功，装备背包 +{equipmentIncrease} 格，道具背包 +{itemIncrease} 格，已达到容量上限。";

                return new ItemUseResultDto
                {
                    Success = true,
                    Message = message,
                    RemainingQuantity = Math.Max(0, currentItem.Quantity),
                    Effects =
                    [
                        new() { EffectType = "EQUIPMENT_CAPACITY", Value = equipmentIncrease, Description = $"装备背包容量 +{equipmentIncrease} 格" },
                        new() { EffectType = "ITEM_CAPACITY", Value = itemIncrease, Description = $"道具背包容量 +{itemIncrease} 格" }
                    ]
                };
            }
            catch
            {
                if (transactionStarted)
                {
                    db.Ado.RollbackTran();
                }
                throw;
            }
        }

        /// <summary>
        /// 消耗宠物蛋并生成灵宠。
        /// </summary>
        private async Task<ItemUseResultDto> ConsumePetEggAsync(
            string playerId,
            InventoryItemEntity item,
            int quantity)
        {
            await HatchPetEggLock.WaitAsync();
            var transactionStarted = false;

            try
            {
                var templates = await _petTemplateRepository.GetListAsync(_ => true);
                if (templates.Count == 0)
                {
                    throw new InvalidOperationException("当前没有可用的灵宠模板，请先同步宠物模板数据。");
                }

                var db = _dbContext.Db;
                var now = DateTime.Now;

                db.Ado.BeginTran();
                transactionStarted = true;

                var currentPetCount = await db.Queryable<PetInstanceEntity>()
                    .CountAsync(pet => pet.PlayerId == playerId);
                if (currentPetCount + quantity > MaxPetCount)
                {
                    throw new InvalidOperationException($"灵宠栏位不足，当前最多只能拥有 {MaxPetCount} 只灵宠。");
                }

                var currentItem = await db.Queryable<InventoryItemEntity>()
                    .Where(entity => entity.Id == item.Id && entity.PlayerId == playerId)
                    .FirstAsync();
                if (currentItem == null || currentItem.Quantity < quantity)
                {
                    throw new InvalidOperationException("宠物蛋数量不足，请刷新后重试。");
                }

                var affectedRows = currentItem.Quantity == quantity
                    ? await db.Deleteable<InventoryItemEntity>()
                        .Where(entity =>
                            entity.Id == currentItem.Id &&
                            entity.PlayerId == playerId &&
                            entity.Quantity == currentItem.Quantity)
                        .ExecuteCommandAsync()
                    : await db.Updateable<InventoryItemEntity>()
                        .SetColumns(entity => entity.Quantity == entity.Quantity - quantity)
                        .Where(entity =>
                            entity.Id == currentItem.Id &&
                            entity.PlayerId == playerId &&
                            entity.Quantity >= quantity)
                        .ExecuteCommandAsync();
                if (affectedRows == 0)
                {
                    throw new InvalidOperationException("宠物蛋使用失败：背包数据已变化，请重试。");
                }

                var acquiredPets = new List<PetInstanceEntity>(quantity);
                for (var index = 0; index < quantity; index++)
                {
                    var template = templates[Random.Shared.Next(templates.Count)];
                    acquiredPets.Add(PetGenerationHelper.GeneratePetInstanceFromTemplate(playerId, template, now));
                }

                var insertRows = await db.Insertable(acquiredPets).ExecuteCommandAsync();
                if (insertRows != acquiredPets.Count)
                {
                    throw new InvalidOperationException("宠物蛋使用失败：灵宠创建不完整，请重试。");
                }

                db.Ado.CommitTran();
                transactionStarted = false;

                item.Quantity = Math.Max(0, currentItem.Quantity - quantity);
                var petNames = acquiredPets.Select(pet => pet.Name).ToList();
                var namePreview = petNames.Count <= 3
                    ? string.Join("、", petNames)
                    : $"{string.Join("、", petNames.Take(3))} 等 {petNames.Count} 只灵宠";

                return new ItemUseResultDto
                {
                    Success = true,
                    Message = quantity == 1
                        ? $"成功孵化灵宠：{petNames[0]}。"
                        : $"成功孵化 {quantity} 只灵宠：{namePreview}。",
                    RemainingQuantity = item.Quantity,
                    Effects =
                    [
                        new()
                        {
                            EffectType = "PET_ACQUIRE",
                            Value = quantity,
                            Description = quantity == 1
                                ? $"获得灵宠 {petNames[0]}"
                                : $"获得 {quantity} 只灵宠"
                        }
                    ]
                };
            }
            catch (InvalidOperationException)
            {
                if (transactionStarted)
                {
                    _dbContext.Db.Ado.RollbackTran();
                }
                throw;
            }
            catch (Exception ex)
            {
                if (transactionStarted)
                {
                    _dbContext.Db.Ado.RollbackTran();
                }
                _logger.LogError(ex, "玩家 {PlayerId} 使用宠物蛋时发生异常", playerId);
                throw new InvalidOperationException("宠物蛋使用失败，请稍后重试。");
            }
            finally
            {
                HatchPetEggLock.Release();
            }
        }

        /// <summary>
        /// 统一减少道具数量。
        /// </summary>
        private async Task ReduceItemQuantityAsync(InventoryItemEntity item, int quantity)
        {
            item.Quantity -= quantity;
            if (item.Quantity <= 0)
            {
                await _inventoryRepository.DeleteAsync(item);
                item.Quantity = 0;
            }
            else
            {
                await _inventoryRepository.UpdateAsync(item);
            }
        }

        private static async Task ReduceInventoryItemInTransactionAsync(SqlSugar.ISqlSugarClient db, InventoryItemEntity item, int quantity)
        {
            var affectedRows = item.Quantity == quantity
                ? await db.Deleteable<InventoryItemEntity>()
                    .Where(entity => entity.Id == item.Id && entity.PlayerId == item.PlayerId && entity.Quantity == item.Quantity)
                    .ExecuteCommandAsync()
                : await db.Updateable<InventoryItemEntity>()
                    .SetColumns(entity => entity.Quantity == entity.Quantity - quantity)
                    .Where(entity => entity.Id == item.Id && entity.PlayerId == item.PlayerId && entity.Quantity >= quantity)
                    .ExecuteCommandAsync();

            if (affectedRows == 0)
            {
                throw new InvalidOperationException("道具扣减失败，请刷新后重试。");
            }
        }

        private static Task<InventoryItemEntity> GrantInventoryItemInTransactionAsync(
            SqlSugar.ISqlSugarClient db,
            string playerId,
            string itemId,
            int quantity)
        {
            return InventoryItemGrantHelper.AddOrMergeAsync(
                db,
                playerId,
                itemId,
                quantity,
                "背包已满，无法领取奖励。");
        }
        /// <summary>
        /// 将数据库实体映射为前端可用 DTO。
        /// </summary>
        private static InventoryItemDto MapToDto(InventoryItemEntity entity)
        {
            if (XXX.GameData.Items.TryGetValue(entity.ItemId, out var itemTemplate))
            {
                return new InventoryItemDto
                {
                    Id = entity.Id,
                    ItemId = entity.ItemId,
                    Name = itemTemplate.Name,
                    Description = itemTemplate.Description,
                    ItemType = itemTemplate.Type == ItemType.InventoryExpansion ? "背包扩容" : itemTemplate.Type.ToString(),
                    Quality = itemTemplate.Quality <= 0 ? 1 : itemTemplate.Quality,
                    Icon = ResolveItemIcon(entity.ItemId, itemTemplate),
                    CanUse = CanUseItem(itemTemplate.Type),
                    UseActionText = ResolveUseActionText(itemTemplate.Type),
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
                Description = "未知物品",
                ItemType = "Unknown",
                Quality = 1,
                Icon = "🎒",
                CanUse = false,
                UseActionText = string.Empty,
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
                _ when itemTemplate.Type == ItemType.RecipeScroll => "📜",
                _ when itemTemplate.Type == ItemType.Seed => "🌱",
                _ when itemTemplate.Type == ItemType.PetEgg => "🥚",
                _ => "🎒"
            };
        }

        private static bool CanUseItem(ItemType itemType)
        {
            return itemType is ItemType.Chest or ItemType.SkillBook or ItemType.RecipeScroll or ItemType.InventoryExpansion or ItemType.PetEgg or ItemType.Pill or ItemType.Consumable;
        }

        private static string ResolveUseActionText(ItemType itemType)
        {
            return itemType switch
            {
                ItemType.Chest => "开启",
                ItemType.SkillBook => "学习",
                ItemType.RecipeScroll => "解锁",
                ItemType.InventoryExpansion => "扩容",
                ItemType.PetEgg => "孵化",
                ItemType.Pill => "服用",
                ItemType.Consumable => "使用",
                _ => string.Empty
            };
        }

        private static ItemChestRewardEntry PickChestReward(IReadOnlyList<ItemChestRewardEntry> rewards)
        {
            var totalWeight = rewards.Sum(reward => Math.Max(1, reward.Weight));
            var roll = Random.Shared.Next(1, totalWeight + 1);
            var cumulative = 0;
            foreach (var reward in rewards)
            {
                cumulative += Math.Max(1, reward.Weight);
                if (roll <= cumulative)
                {
                    return reward;
                }
            }

            return rewards[0];
        }

        private static ItemEffectDto BuildChestEffect(string effectType, double value, string description)
        {
            return new ItemEffectDto
            {
                EffectType = effectType,
                Value = value,
                Description = description
            };
        }

        private static string ResolveItemName(string itemId)
        {
            if (XXX.GameData.Items.TryGetValue(itemId, out var itemTemplate))
            {
                return itemTemplate.Name;
            }

            return itemId;
        }

        private static string ResolveAttributeName(AttributeType attributeType)
        {
            return attributeType switch
            {
                AttributeType.Type1 => "生命",
                AttributeType.Type2 => "法力",
                AttributeType.Type3 => "物攻",
                AttributeType.Type4 => "法攻",
                AttributeType.Type5 => "物防",
                AttributeType.Type6 => "法防",
                AttributeType.Type7 => "速度",
                AttributeType.Type8 => "命中",
                AttributeType.Type9 => "闪避",
                AttributeType.Type10 => "暴击",
                AttributeType.Type11 => "暴伤",
                AttributeType.Type12 => "连击",
                AttributeType.Type13 => "反击",
                AttributeType.Type14 => "破甲",
                AttributeType.Type15 => "增伤",
                _ => attributeType.ToString()
            };
        }
    }
}
