using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SqlSugar;
using XXX.Balance;
using XXX.Achievement;
using XXX.Entity;
using XXX.Infrastructure.Data;
using XXX.Player;
using XXX.Quest;
using XXX.Ranking;
using XXX.Shop;

namespace XXX.Infrastructure.SeedData
{
    /// <summary>
    /// 种子数据服务实现
    /// </summary>
    public class SeedDataService : ISeedDataService
    {
        private const string StarterPackageSeedKey = "starter-default-register";
        private const string StarterPackageVersion = "starter-package-v1-20260402";
        private const string PlayerLevelConfigVersion = "player-level-config-v1-20260403";
        private const string RealmLevelConfigVersion = "realm-level-config-v1-20260403";
        private const string AttributePointConfigVersion = "attribute-point-config-v1-20260403";
        private const string PlayerInitialResourceConfigVersion = "player-initial-resource-config-v1-20260403";
        private const string FiveElementRuleVersion = "five-element-rule-v1-20260403";
        private const string SpiritFieldRuleVersion = "spirit-field-rule-v1-20260403";
        private const string AlchemyProfessionRuleVersion = "alchemy-profession-rule-v1-20260403";
        private const string ForgeProfessionRuleVersion = "forge-profession-rule-v1-20260403";
        private const string ElementRelationRuleVersion = "element-relation-rule-v2-20260612";
        private const string CropTemplateVersion = "crop-template-built-in-v1-20260403";
        private const string SkillTemplateVersion = "skill-template-built-in-v1-20260403";
        private const string BuffTemplateVersion = "buff-template-built-in-v1-20260403";
        private const string AlchemyRecipeVersion = "alchemy-recipe-built-in-v1-20260403";
        private const string ForgeRecipeVersion = "forge-recipe-built-in-v1-20260403";
        private const string EquipmentRerollRuleVersion = "equipment-reroll-rule-v1-20260609";
        private const string EquipmentEnhanceRuleVersion = "equipment-enhance-rule-v1-20260907";
        private const string ShopBuiltInVersion = "shop-built-in-v1-20260403";
        private const string RankingBuiltInVersion = "ranking-built-in-v1-20260403";
        private const string CollectionLotteryVersion = CollectionLotterySeedData.Version;
        private static readonly int[] PureForgeEquipmentIds =
        {
            10001, 10011, 10201, 10211, 10401, 10411, 10601, 10611, 10801, 10811,
            11001, 11011, 11201, 11211, 11401, 11411, 11601, 11611, 11801, 11811
        };
        private const string GeneralShopId = "shop_general";
        private const string BlacksmithShopId = "shop_blacksmith";
        private static readonly string[] GeneralShopItemIds =
        {
            "item_001",
            "item_002",
            "item_006",
            "item_007",
            "pet_egg",
            "pet_food_basic",
            "evolution_stone",
            "seed_blood_grass",
            "seed_spirit_lotus",
            "seed_stone_moss",
            "alchemy_herb",
            "spirit_water",
            "spirit_dust",
            "ore_t1",
            "wood_t1",
            "hide_t1",
            "crystal_t1"
        };
        private static readonly string[] BlacksmithMaterialItemIds =
        {
            "item_003",
            "ore_t1",
            "wood_t1",
            "hide_t1",
            "crystal_t1"
        };

        private readonly DbContext _dbContext;
        private readonly BalanceCatalogSyncService _balanceCatalogSyncService;
        private readonly AdminSeedOptions _adminSeedOptions;
        private readonly SeedStartupOptions _startupOptions;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly string _seedDataPath;
        private readonly ILogger<SeedDataService>? _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dbContext">数据库上下文</param>
        /// <param name="seedDataPath">种子数据文件夹路径</param>
        internal SeedDataService(
            DbContext dbContext,
            BalanceCatalogSyncService balanceCatalogSyncService,
            IOptions<AdminSeedOptions> adminSeedOptions,
            IOptions<SeedStartupOptions> startupOptions,
            IHostEnvironment hostEnvironment,
            string seedDataPath = "SeedData",
            ILogger<SeedDataService>? logger = null)
        {
            _dbContext = dbContext;
            _balanceCatalogSyncService = balanceCatalogSyncService;
            _adminSeedOptions = adminSeedOptions.Value;
            _startupOptions = startupOptions.Value;
            _hostEnvironment = hostEnvironment;
            _seedDataPath = seedDataPath;
            _logger = logger;
        }

        /// <summary>
        /// 初始化所有种子数据
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_startupOptions.SkipBootstrapCheck)
            {
                _logger?.LogInformation("Seed bootstrap check skipped by configuration (SeedStartup:SkipBootstrapCheck = true).");
                return;
            }

            var bootstrapStatus = await CheckBootstrapStatusAsync();
            if (!bootstrapStatus.ShouldSeed)
            {
                _logger?.LogInformation(
                    "Skipped startup seed sync because bootstrap data is already present. {Reason}",
                    bootstrapStatus.Reason);

                // BalanceCatalog 幂等同步：补全新增的模板（如精英怪物）
                await _balanceCatalogSyncService.SyncAsync();

                // 确保宝石模板都有对应的道具条目（幂等，只补缺）
                await EnsureGemItemTemplatesAsync();

                // 这些配置表支持幂等更新，通过 SeedStartup 配置项控制是否每次启动运行
                if (_startupOptions.SyncEquipmentRerollRules)
                {
                    await SeedEquipmentRerollRulesAsync();
                }

                if (_startupOptions.SyncElementRelationRules)
                {
                    await SeedElementRelationRulesAsync();
                }

                await SeedRankingsAsync();

                return;
            }

            _logger?.LogInformation(
                "Startup seed sync is required. {Reason}",
                bootstrapStatus.Reason);

            await _balanceCatalogSyncService.SyncAsync();
            await SeedPlayerLevelConfigsAsync();
            await SeedRealmLevelConfigsAsync();
            await SeedAttributePointConfigsAsync();
            await SeedPlayerInitialResourceConfigAsync();
            await SeedItemsAsync();
            await SeedItemExtensionConfigsAsync();
            await SeedEquipmentTemplatesAsync();
            await SeedSkillsAsync();
            await SeedMonstersAsync();
            await SeedMapsAsync();
            await SeedQuestsAsync();
            await SeedCheckInRewardConfigsAsync();
            await SeedRedeemCodeConfigsAsync();
            await SeedAchievementsAsync();
            await SeedStarterPackagesAsync();
            await SeedFiveElementRulesAsync();
            await SeedFiveElementBranchRuleRangesAsync();
            await SeedSpiritFieldRulesAsync();
            await SeedAlchemyProfessionRulesAsync();
            await SeedForgeProfessionRulesAsync();
            await SeedEquipmentRerollRulesAsync();
            await SeedElementRelationRulesAsync();
            await SeedShopsAsync();
            await SeedRankingsAsync();
            await SeedPetsAsync();
            await SeedAlchemyRecipesAsync();
            await SeedSpiritFieldAsync();
            await SeedForgeRecipesAsync();
            await SeedRecipeUnlockDataAsync();
            await SeedDungeonsAsync();
            await SeedDungeonInstancesAsync();
            await SeedCollectionsAndLotteryAsync();
            await SeedAdminUsersAsync();
        }

        private async Task<SeedBootstrapStatus> CheckBootstrapStatusAsync()
        {
            var checks = new List<SeedBootstrapTableCheck>
            {
                await CheckTableCountAsync<ItemTemplateEntity>("ItemTemplates", minimumCount: 1),
                await CheckTableCountAsync<EquipmentTemplateEntity>("EquipmentTemplates", minimumCount: 1),
                await CheckTableCountAsync<MonsterTemplateEntity>("MonsterTemplates", minimumCount: 1),
                await CheckTableCountAsync<MapTemplateEntity>("MapTemplates", minimumCount: 1),
                await CheckTableCountAsync<SkillTemplateEntity>("SkillTemplates", minimumCount: 1),
                await CheckTableCountAsync<BuffTemplateEntity>("BuffTemplates", minimumCount: 1),
                await CheckTableCountAsync<DungeonTemplateEntity>("DungeonTemplates", minimumCount: 1),
                await CheckTableCountAsync<DungeonInstanceTemplateEntity>("DungeonInstanceTemplates", minimumCount: 1),
                await CheckTableCountAsync<DungeonEventConfigEntity>("DungeonEventConfigs", minimumCount: 1),
                await CheckTableCountAsync<DungeonEventGroupEntity>("DungeonEventGroups", minimumCount: 1),
                await CheckTableCountAsync<PetTemplateEntity>("PetTemplates", minimumCount: 1),
                await CheckTableCountAsync<PlayerInitialResourceConfigEntity>("PlayerInitialResourceConfigs", minimumCount: 1),
                await CheckTableCountAsync<PlayerLevelConfigEntity>("PlayerLevelConfigs", exactCount: LevelConfig.MaxLevel),
                await CheckTableCountAsync<RealmLevelConfigEntity>("RealmLevelConfigs", exactCount: LevelConfig.MaxLevel),
                await CheckTableCountAsync<AttributePointConfigEntity>(
                    "AttributePointConfigs",
                    minimumCount: AttributePointConfig.GetLegacyDefinitions().Count * AttributePointConfig.GetLegacyLevelRanges().Count),
                await CheckTableCountAsync<FiveElementLevelConfigEntity>("FiveElementLevelConfigs", minimumCount: 1),
                await CheckTableCountAsync<FiveElementBranchUpgradeConfigEntity>("FiveElementBranchUpgradeConfigs", minimumCount: 1),
                await CheckTableCountAsync<FiveElementBranchRuleRangeEntity>("FiveElementBranchRuleRanges", minimumCount: 5),
                await CheckTableCountAsync<AlchemyProfessionLevelConfigEntity>("AlchemyProfessionLevelConfigs", minimumCount: 1),
                await CheckTableCountAsync<AlchemyProfessionRuleConfigEntity>("AlchemyProfessionRuleConfigs", minimumCount: 1),
                await CheckTableCountAsync<ForgeProfessionLevelConfigEntity>("ForgeProfessionLevelConfigs", minimumCount: 1),
                await CheckTableCountAsync<ForgeProfessionRuleConfigEntity>("ForgeProfessionRuleConfigs", minimumCount: 1),
                await CheckTableCountAsync<ElementRelationRuleEntity>("BattleElementRelationConfigs", minimumCount: 1),
                await CheckTableCountAsync<SpiritFieldSystemConfigEntity>("SpiritFieldSystemConfigs", minimumCount: 1),
                await CheckTableCountAsync<SpiritFieldSpeedUpItemConfigEntity>("SpiritFieldSpeedUpItemConfigs", minimumCount: 3),
                await CheckTableCountAsync<CropTemplateEntity>("CropTemplates", minimumCount: 3),
                await CheckTableCountAsync<AlchemyRecipeEntity>("AlchemyRecipes", minimumCount: 3),
                await CheckTableCountAsync<ForgeRecipeEntity>("ForgeRecipes", minimumCount: 1),
                await CheckTableCountAsync<EquipmentRerollSystemConfigEntity>("EquipmentRerollSystemConfigs", minimumCount: 1),
                await CheckTableCountAsync<EquipmentRerollSlotPoolConfigEntity>("EquipmentRerollSlotPoolConfigs", minimumCount: 1),
                await CheckTableCountAsync<EquipmentRerollTierConfigEntity>("EquipmentRerollTierConfigs", minimumCount: 1),
                await CheckTableCountAsync<QuestConfigEntity>("quest_config", minimumCount: QuestSeedCatalog.DefaultQuestIds.Length),
                await CheckTableCountAsync<AchievementConfigEntity>("achievement_config", minimumCount: AchievementSeedCatalog.DefaultAchievementIds.Length),
                await CheckTableCountAsync<CheckInRewardConfigEntity>("CheckInRewardConfigs", minimumCount: ActivityRewardCatalog.BuildCheckInRewards().Count),
                await CheckTableCountAsync<RedeemCodeConfigEntity>("RedeemCodeConfigs", minimumCount: ActivityRewardCatalog.BuildRedeemCodes().Count),
                await CheckTableCountAsync<ShopConfigEntity>("shop_config", minimumCount: 2),
                await CheckTableCountAsync<ShopItemEntity>("shop_item", minimumCount: 1),
                await CheckTableCountAsync<RankingConfigEntity>("ranking_config", minimumCount: 1),
                await CheckTableCountAsync<RankingRewardEntity>("ranking_reward", minimumCount: 1),
                await CheckTableCountAsync<StarterPackageConfigEntity>("StarterPackageConfigs", minimumCount: 1),
                await CheckTableCountAsync<StarterPackageGrantItemEntity>("StarterPackageGrantItems", minimumCount: 1),
                await CheckTableCountAsync<StarterPackageGrantSkillEntity>("StarterPackageGrantSkills", minimumCount: 1),
                await CheckTableCountAsync<TextCollectionSeriesEntity>("TextCollectionSeries", minimumCount: 1),
                await CheckTableCountAsync<TextCollectionItemEntity>("TextCollectionItems", minimumCount: 1),
                await CheckTableCountAsync<TextCollectionBonusEntity>("TextCollectionBonuses", minimumCount: 1),
                await CheckTableCountAsync<ImageCollectionSeriesEntity>("ImageCollectionSeries", minimumCount: 1),
                await CheckTableCountAsync<ImageCollectionItemEntity>("ImageCollectionItems", minimumCount: 1),
                await CheckTableCountAsync<ImageCollectionBonusEntity>("ImageCollectionBonuses", minimumCount: 1),
                await CheckTableCountAsync<LotteryPoolEntity>("LotteryPools", minimumCount: 1),
                await CheckTableCountAsync<LotteryPrizeEntity>("LotteryPrizes", minimumCount: 1)
            };

            var failedChecks = checks
                .Where(item => !item.Passed)
                .ToList();
            if (failedChecks.Count == 0)
            {
                return new SeedBootstrapStatus(false, "All required bootstrap tables already contain seed data.");
            }

            return new SeedBootstrapStatus(
                true,
                $"Missing or incomplete bootstrap tables: {string.Join(", ", failedChecks.Select(item => item.Describe()))}");
        }

        private async Task<SeedBootstrapTableCheck> CheckTableCountAsync<T>(
            string tableName,
            int minimumCount = 0,
            int? exactCount = null)
            where T : class, new()
        {
            var count = await _dbContext.Db.Queryable<T>().CountAsync();
            var passed = exactCount.HasValue
                ? count == exactCount.Value
                : count >= Math.Max(1, minimumCount);

            return new SeedBootstrapTableCheck(tableName, count, minimumCount, exactCount, passed);
        }

        private sealed record SeedBootstrapStatus(bool ShouldSeed, string Reason);

        private sealed record SeedBootstrapTableCheck(
            string TableName,
            int ActualCount,
            int MinimumCount,
            int? ExactCount,
            bool Passed)
        {
            public string Describe()
            {
                return ExactCount.HasValue
                    ? $"{TableName}={ActualCount} (expected exactly {ExactCount.Value})"
                    : $"{TableName}={ActualCount} (expected at least {Math.Max(1, MinimumCount)})";
            }
        }

        /// <summary>
        /// 从JSON文件加载种子数据
        /// </summary>
        private async Task<List<T>> LoadSeedDataAsync<T>(string fileName) where T : class
        {
            var filePath = Path.Combine(_seedDataPath, fileName);
            if (!File.Exists(filePath))
            {
                return [];
            }

            var json = await File.ReadAllTextAsync(filePath);
            return JsonConvert.DeserializeObject<List<T>>(json) ?? [];
        }

        /// <summary>
        /// 初始化数据表（如果不存在则创建）
        /// </summary>
        private async Task SeedDataAsync<T>(string fileName, string tableName = "") where T : class, new()
        {
            var table = string.IsNullOrEmpty(tableName) ? typeof(T).Name : tableName;

            // 检查表是否为空
            var count = await _dbContext.Db.Queryable<T>().CountAsync();
            if (count > 0)
            {
                return; // 已有数据，跳过
            }

            var data = await LoadSeedDataAsync<T>(fileName);
            if (data.Any())
            {
                await _dbContext.Db.Insertable(data).ExecuteCommandAsync();
            }
        }

        /// <summary>
        /// 初始化物品数据
        /// </summary>
        public async Task SeedItemsAsync()
        {
            var existingItemCount = await _dbContext.Db.Queryable<ItemTemplateEntity>().CountAsync();
            if (existingItemCount > 0)
            {
                return;
            }

            var snapshot = BalanceCatalog.GetSeedSnapshot();
            if (snapshot.Items.Count == 0)
            {
                return;
            }

            await _dbContext.Db.Insertable(snapshot.Items.ToList()).ExecuteCommandAsync();
            _logger?.LogInformation("Seeded {Count} item templates into SQLite from balance catalog", snapshot.Items.Count);
        }

        /// <summary>
        /// 初始化等级境界配置。
        /// </summary>
        public async Task SeedRealmLevelConfigsAsync()
        {
            var now = DateTime.Now;
            var expectedConfigs = RealmLevelCatalog.GetLegacyEntries()
                .Select(item =>
                {
                    item.SeedKey = $"built-in:realm-level:{item.Level}";
                    item.IsBuiltIn = true;
                    item.BuiltInVersion = RealmLevelConfigVersion;
                    item.LastUpdateTime = now;
                    return item;
                })
                .ToList();

            foreach (var expected in expectedConfigs)
            {
                var existing = await _dbContext.Db.Queryable<RealmLevelConfigEntity>()
                    .FirstAsync(item => item.Level == expected.Level);
                if (existing == null)
                {
                    await _dbContext.Db.Insertable(expected).ExecuteCommandAsync();
                    continue;
                }

                if (!(existing.IsBuiltIn || string.Equals(existing.SeedKey, expected.SeedKey, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                existing.RealmName = expected.RealmName;
                existing.Alias = expected.Alias;
                existing.RealmOrder = expected.RealmOrder;
                existing.Layer = expected.Layer;
                existing.RequiredExp = expected.RequiredExp;
                existing.AttributeBonusPercent = expected.AttributeBonusPercent;
                existing.IsBreakthroughPoint = expected.IsBreakthroughPoint;
                existing.BreakthroughSuccessRate = expected.BreakthroughSuccessRate;
                existing.BreakthroughExpLossPercent = expected.BreakthroughExpLossPercent;
                existing.BreakthroughMaterialsJson = expected.BreakthroughMaterialsJson;
                existing.Description = expected.Description;
                existing.SeedKey = expected.SeedKey;
                existing.IsBuiltIn = true;
                existing.BuiltInVersion = expected.BuiltInVersion;
                existing.LastUpdateTime = now;
                await _dbContext.Db.Updateable(existing).ExecuteCommandAsync();
            }

            _logger?.LogInformation("Ensured built-in realm level configs exist with {Count} rows", expectedConfigs.Count);
        }

        public async Task SeedPlayerLevelConfigsAsync()
        {
            var now = DateTime.Now;
            var realmRequiredExpMap = RealmLevelCatalog.GetLegacyEntries()
                .ToDictionary(item => item.Level, item => item.RequiredExp);
            var expectedEntries = LevelConfig.GetLegacyEntries()
                .Select(item => new PlayerLevelConfigEntity
                {
                    Level = item.Level,
                    RequiredExp = realmRequiredExpMap.TryGetValue(item.Level, out var requiredExp)
                        ? requiredExp
                        : item.RequiredExp,
                    BaseHp = item.BaseHp,
                    BaseMp = item.BaseMp,
                    BasePhysicalAttack = item.BasePhysicalAttack,
                    BaseMagicAttack = item.BaseMagicAttack,
                    BasePhysicalDefense = item.BasePhysicalDefense,
                    BaseMagicDefense = item.BaseMagicDefense,
                    BaseSpeed = item.BaseSpeed,
                    SeedKey = $"built-in:player-level:{item.Level}",
                    IsBuiltIn = true,
                    BuiltInVersion = PlayerLevelConfigVersion,
                    LastUpdateTime = now
                })
                .ToList();

            foreach (var expected in expectedEntries)
            {
                var existing = await _dbContext.Db.Queryable<PlayerLevelConfigEntity>()
                    .FirstAsync(item => item.Level == expected.Level);
                if (existing == null)
                {
                    await _dbContext.Db.Insertable(expected).ExecuteCommandAsync();
                    continue;
                }

                if (!(existing.IsBuiltIn || string.Equals(existing.SeedKey, expected.SeedKey, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                existing.RequiredExp = expected.RequiredExp;
                existing.BaseHp = expected.BaseHp;
                existing.BaseMp = expected.BaseMp;
                existing.BasePhysicalAttack = expected.BasePhysicalAttack;
                existing.BaseMagicAttack = expected.BaseMagicAttack;
                existing.BasePhysicalDefense = expected.BasePhysicalDefense;
                existing.BaseMagicDefense = expected.BaseMagicDefense;
                existing.BaseSpeed = expected.BaseSpeed;
                existing.SeedKey = expected.SeedKey;
                existing.IsBuiltIn = true;
                existing.BuiltInVersion = expected.BuiltInVersion;
                existing.LastUpdateTime = now;
                await _dbContext.Db.Updateable(existing).ExecuteCommandAsync();
            }

            _logger?.LogInformation("Ensured built-in player level configs exist with {Count} rows", expectedEntries.Count);
        }

        public async Task SeedAttributePointConfigsAsync()
        {
            var now = DateTime.Now;
            var expectedEntries = BuildAttributePointConfigEntries(now);
            var existingEntries = await _dbContext.Db.Queryable<AttributePointConfigEntity>()
                .OrderBy(item => item.ConfigId)
                .ToListAsync();

            if (existingEntries.Count == 0)
            {
                await _dbContext.Db.Insertable(expectedEntries).ExecuteCommandAsync();
                _logger?.LogInformation("Seeded {Count} attribute point configs into SQLite", expectedEntries.Count);
                return;
            }

            var existingMap = existingEntries.ToDictionary(
                item => BuildAttributePointRuntimeKey(item.Profession, item.Key, item.LevelStart, item.LevelEnd),
                StringComparer.OrdinalIgnoreCase);
            foreach (var expected in expectedEntries)
            {
                var runtimeKey = BuildAttributePointRuntimeKey(expected.Profession, expected.Key, expected.LevelStart, expected.LevelEnd);
                if (!existingMap.TryGetValue(runtimeKey, out var existing))
                {
                    await _dbContext.Db.Insertable(expected).ExecuteCommandAsync();
                    continue;
                }

                if (!(existing.IsBuiltIn || string.Equals(existing.SeedKey, expected.SeedKey, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                existing.ConfigId = expected.ConfigId;
                existing.Key = expected.Key;
                existing.Name = expected.Name;
                existing.Profession = expected.Profession;
                existing.AttributeType = expected.AttributeType;
                existing.BonusPerPoint = expected.BonusPerPoint;
                existing.PointsPerBonus = expected.PointsPerBonus;
                existing.LevelStart = expected.LevelStart;
                existing.LevelEnd = expected.LevelEnd;
                existing.PointsGained = expected.PointsGained;
                existing.SortOrder = expected.SortOrder;
                existing.IsEnabled = expected.IsEnabled;
                existing.SeedKey = expected.SeedKey;
                existing.IsBuiltIn = true;
                existing.BuiltInVersion = expected.BuiltInVersion;
                existing.LastUpdateTime = now;
                await _dbContext.Db.Updateable(existing).ExecuteCommandAsync();
            }
        }

        public async Task SeedPlayerInitialResourceConfigAsync()
        {
            var now = DateTime.Now;
            var expected = new PlayerInitialResourceConfigEntity
            {
                ConfigId = "default",
                StartLevel = 1,
                StartExp = 0,
                StartGold = 1000,
                StartSpiritStone = 0,
                SeedKey = "built-in:player-initial-resource:default",
                IsBuiltIn = true,
                BuiltInVersion = PlayerInitialResourceConfigVersion,
                LastUpdateTime = now
            };

            var existing = await _dbContext.Db.Queryable<PlayerInitialResourceConfigEntity>()
                .InSingleAsync(expected.ConfigId);
            if (existing == null)
            {
                await _dbContext.Db.Insertable(expected).ExecuteCommandAsync();
                _logger?.LogInformation("Ensured built-in player initial resource config exists.");
                return;
            }

            if (!(existing.IsBuiltIn || string.Equals(existing.SeedKey, expected.SeedKey, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            existing.StartGold = expected.StartGold;
            existing.StartLevel = expected.StartLevel;
            existing.StartExp = expected.StartExp;
            existing.StartSpiritStone = expected.StartSpiritStone;
            existing.SeedKey = expected.SeedKey;
            existing.IsBuiltIn = true;
            existing.BuiltInVersion = expected.BuiltInVersion;
            existing.LastUpdateTime = now;
            await _dbContext.Db.Updateable(existing).ExecuteCommandAsync();
            _logger?.LogInformation("Ensured built-in player initial resource config exists.");
        }

        public async Task SeedItemExtensionConfigsAsync()
        {
            var now = DateTime.Now;
            var items = await _dbContext.Db.Queryable<ItemTemplateEntity>().ToListAsync();
            var builtInItemMap = BalanceCatalog.GetSeedSnapshot().Items
                .ToDictionary(item => item.ItemId, StringComparer.OrdinalIgnoreCase);

            foreach (var item in items)
            {
                builtInItemMap.TryGetValue(item.ItemId, out var builtInItem);
                var chestConfig = item.ChestConfig ?? builtInItem?.ChestConfig;
                var skillBookConfig = item.SkillBookConfig ?? builtInItem?.SkillBookConfig;
                var petEggConfig = item.PetEggConfig ?? builtInItem?.PetEggConfig;
                var pillConfig = item.PillConfig ?? builtInItem?.PillConfig;

                if (chestConfig != null && chestConfig.Rewards.Count > 0)
                {
                    var existing = await _dbContext.Db.Queryable<ItemChestConfigEntity>()
                        .FirstAsync(config => config.ItemId == item.ItemId);
                    if (existing == null)
                    {
                        await _dbContext.Db.Insertable(new ItemChestConfigEntity
                        {
                            ItemId = item.ItemId,
                            OpenMode = (int)chestConfig.OpenMode,
                            RollCount = chestConfig.RollCount,
                            IsEnabled = true,
                            LastUpdateTime = now
                        }).ExecuteCommandAsync();
                    }

                    var rewardCount = await _dbContext.Db.Queryable<ItemChestRewardEntryEntity>()
                        .CountAsync(config => config.ItemId == item.ItemId);
                    if (rewardCount == 0)
                    {
                        var rewards = chestConfig.Rewards
                            .Select((reward, index) => new ItemChestRewardEntryEntity
                            {
                                GID = Guid.NewGuid().ToString("N"),
                                ItemId = item.ItemId,
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
                            await _dbContext.Db.Insertable(rewards).ExecuteCommandAsync();
                        }
                    }
                }

                if (skillBookConfig != null && skillBookConfig.SkillId > 0)
                {
                    var existing = await _dbContext.Db.Queryable<ItemSkillBookConfigEntity>()
                        .FirstAsync(config => config.ItemId == item.ItemId);
                    if (existing == null)
                    {
                        await _dbContext.Db.Insertable(new ItemSkillBookConfigEntity
                        {
                            ItemId = item.ItemId,
                            SkillId = skillBookConfig.SkillId,
                            IsEnabled = true,
                            LastUpdateTime = now
                        }).ExecuteCommandAsync();
                    }
                    else if (existing.SkillId <= 0)
                    {
                        existing.SkillId = skillBookConfig.SkillId;
                        existing.IsEnabled = true;
                        existing.LastUpdateTime = now;
                        await _dbContext.Db.Updateable(existing).ExecuteCommandAsync();
                    }
                }

                if (petEggConfig != null && !string.IsNullOrWhiteSpace(petEggConfig.PetTemplateId))
                {
                    var existing = await _dbContext.Db.Queryable<ItemPetEggConfigEntity>()
                        .FirstAsync(config => config.ItemId == item.ItemId);
                    if (existing == null)
                    {
                        await _dbContext.Db.Insertable(new ItemPetEggConfigEntity
                        {
                            ItemId = item.ItemId,
                            PetTemplateId = petEggConfig.PetTemplateId.Trim(),
                            IsEnabled = true,
                            LastUpdateTime = now
                        }).ExecuteCommandAsync();
                    }
                    else if (string.IsNullOrWhiteSpace(existing.PetTemplateId))
                    {
                        existing.PetTemplateId = petEggConfig.PetTemplateId.Trim();
                        existing.IsEnabled = true;
                        existing.LastUpdateTime = now;
                        await _dbContext.Db.Updateable(existing).ExecuteCommandAsync();
                    }
                }

                if (pillConfig != null)
                {
                    var existing = await _dbContext.Db.Queryable<ItemPillConfigEntity>()
                        .FirstAsync(config => config.ItemId == item.ItemId);
                    if (existing == null)
                    {
                        await _dbContext.Db.Insertable(new ItemPillConfigEntity
                        {
                            ItemId = item.ItemId,
                            EffectType = (int)pillConfig.EffectType,
                            BreakthroughBonusPercent = pillConfig.BreakthroughBonusPercent,
                            ExpGain = pillConfig.ExpGain,
                            AttributeType = pillConfig.AttributeType?.ToString(),
                            AttributeValue = pillConfig.AttributeValue,
                            IsEnabled = true,
                            LastUpdateTime = now
                        }).ExecuteCommandAsync();
                    }
                    else if (existing.EffectType <= 0)
                    {
                        existing.EffectType = (int)pillConfig.EffectType;
                        existing.BreakthroughBonusPercent = pillConfig.BreakthroughBonusPercent;
                        existing.ExpGain = pillConfig.ExpGain;
                        existing.AttributeType = pillConfig.AttributeType?.ToString();
                        existing.AttributeValue = pillConfig.AttributeValue;
                        existing.IsEnabled = true;
                        existing.LastUpdateTime = now;
                        await _dbContext.Db.Updateable(existing).ExecuteCommandAsync();
                    }
                }
            }
        }

        /// <summary>
        /// 初始化装备模板数据
        /// </summary>
        public async Task SeedEquipmentTemplatesAsync()
        {
            var existingTemplateCount = await _dbContext.Db.Queryable<EquipmentTemplateEntity>().CountAsync();
            if (existingTemplateCount > 0)
            {
                return;
            }

            var snapshot = BalanceCatalog.GetSeedSnapshot();
            if (snapshot.EquipmentTemplates.Count == 0)
            {
                return;
            }

            await _dbContext.Db.Insertable(snapshot.EquipmentTemplates.ToList()).ExecuteCommandAsync();
            _logger?.LogInformation("Seeded {Count} equipment templates into SQLite from balance catalog", snapshot.EquipmentTemplates.Count);
        }

        /// <summary>
        /// 初始化技能数据
        /// </summary>
        public async Task SeedSkillsAsync()
        {
            var now = DateTime.Now;
            global::XXX.SkillData.InitializeSkills();
            global::XXX.BuffDataTemplates.InitializeBuffTemplates();

            try
            {
                var expectedSkills = global::XXX.SkillData.Skills.Values
                    .OrderBy(skill => skill.Id)
                    .Select(TemplateDataMapper.ToEntity)
                    .Select(skill => StampBuiltInSkillTemplate(skill, now))
                    .ToList();
                var existingSkills = await _dbContext.Db.Queryable<SkillTemplateEntity>()
                    .ToListAsync();
                var skillMap = existingSkills.ToDictionary(skill => skill.SkillId);

                foreach (var expected in expectedSkills)
                {
                    if (!skillMap.TryGetValue(expected.SkillId, out var existing))
                    {
                        await _dbContext.Db.Insertable(expected).ExecuteCommandAsync();
                        continue;
                    }

                    var hasBuiltInIdentity = existing.IsBuiltIn ||
                        string.Equals(existing.SeedKey, expected.SeedKey, StringComparison.OrdinalIgnoreCase);
                    if (!hasBuiltInIdentity && !MatchesSkillTemplate(existing, expected))
                    {
                        continue;
                    }

                    ApplySkillTemplateSnapshot(existing, expected);
                    await _dbContext.Db.Updateable(existing).ExecuteCommandAsync();
                }

                _logger?.LogInformation("Ensured built-in skill templates exist with {Count} rows", expectedSkills.Count);

                var expectedBuffs = global::XXX.BuffDataTemplates.BuffTemplates.Values
                    .OrderBy(buff => buff.Gid, StringComparer.OrdinalIgnoreCase)
                    .Select(TemplateDataMapper.ToEntity)
                    .Select(buff => StampBuiltInBuffTemplate(buff, now))
                    .ToList();
                var existingBuffs = await _dbContext.Db.Queryable<BuffTemplateEntity>()
                    .ToListAsync();
                var buffMap = existingBuffs.ToDictionary(buff => buff.BuffId, StringComparer.OrdinalIgnoreCase);

                foreach (var expected in expectedBuffs)
                {
                    if (!buffMap.TryGetValue(expected.BuffId, out var existing))
                    {
                        await _dbContext.Db.Insertable(expected).ExecuteCommandAsync();
                        continue;
                    }

                    var hasBuiltInIdentity = existing.IsBuiltIn ||
                        string.Equals(existing.SeedKey, expected.SeedKey, StringComparison.OrdinalIgnoreCase);
                    if (!hasBuiltInIdentity && !MatchesBuffTemplate(existing, expected))
                    {
                        continue;
                    }

                    ApplyBuffTemplateSnapshot(existing, expected);
                    await _dbContext.Db.Updateable(existing).ExecuteCommandAsync();
                }

                _logger?.LogInformation("Ensured built-in buff templates exist with {Count} rows", expectedBuffs.Count);
            }
            finally
            {
                global::XXX.SkillData.ClearRuntimeCaches();
                global::XXX.BuffDataTemplates.ClearRuntimeCaches();
            }
        }

        private static SkillTemplateEntity StampBuiltInSkillTemplate(SkillTemplateEntity skill, DateTime now)
        {
            skill.SeedKey = $"built-in:skill-template:{skill.SkillId}";
            skill.IsBuiltIn = true;
            skill.BuiltInVersion = SkillTemplateVersion;
            skill.LastUpdateTime = now;
            return skill;
        }

        private static BuffTemplateEntity StampBuiltInBuffTemplate(BuffTemplateEntity buff, DateTime now)
        {
            buff.SeedKey = $"built-in:buff-template:{buff.BuffId}";
            buff.IsBuiltIn = true;
            buff.BuiltInVersion = BuffTemplateVersion;
            buff.LastUpdateTime = now;
            return buff;
        }

        private static bool MatchesSkillTemplate(SkillTemplateEntity actual, SkillTemplateEntity expected)
        {
            return string.Equals(actual.Name, expected.Name, StringComparison.Ordinal) &&
                   string.Equals(actual.Description, expected.Description, StringComparison.Ordinal) &&
                   actual.TargetType == expected.TargetType &&
                   actual.ManaCost == expected.ManaCost &&
                   actual.Cooldown == expected.Cooldown &&
                   actual.DamageType == expected.DamageType &&
                   actual.HitCount == expected.HitCount &&
                   actual.RangeType == expected.RangeType &&
                   actual.DamageMultiplier.Equals(expected.DamageMultiplier) &&
                   actual.TriggerChance.Equals(expected.TriggerChance) &&
                   string.Equals(actual.HitsJson ?? string.Empty, expected.HitsJson ?? string.Empty, StringComparison.Ordinal) &&
                   string.Equals(actual.BuffIdsJson ?? string.Empty, expected.BuffIdsJson ?? string.Empty, StringComparison.Ordinal);
        }

        private static bool MatchesBuffTemplate(BuffTemplateEntity actual, BuffTemplateEntity expected)
        {
            return string.Equals(actual.BuffId, expected.BuffId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(actual.Name, expected.Name, StringComparison.Ordinal) &&
                   string.Equals(actual.Description, expected.Description, StringComparison.Ordinal) &&
                   actual.Duration == expected.Duration &&
                   actual.MaxStack == expected.MaxStack &&
                   actual.StackRule == expected.StackRule &&
                   string.Equals(actual.EffectsJson ?? string.Empty, expected.EffectsJson ?? string.Empty, StringComparison.Ordinal);
        }

        private static void ApplySkillTemplateSnapshot(SkillTemplateEntity target, SkillTemplateEntity snapshot)
        {
            target.Name = snapshot.Name;
            target.Description = snapshot.Description;
            target.TargetType = snapshot.TargetType;
            target.ManaCost = snapshot.ManaCost;
            target.Cooldown = snapshot.Cooldown;
            target.DamageType = snapshot.DamageType;
            target.HitCount = snapshot.HitCount;
            target.RangeType = snapshot.RangeType;
            target.DamageMultiplier = snapshot.DamageMultiplier;
            target.TriggerChance = snapshot.TriggerChance;
            target.HitsJson = snapshot.HitsJson;
            target.BuffIdsJson = snapshot.BuffIdsJson;
            target.SeedKey = snapshot.SeedKey;
            target.IsBuiltIn = true;
            target.BuiltInVersion = snapshot.BuiltInVersion;
            target.LastUpdateTime = snapshot.LastUpdateTime;
        }

        private static void ApplyBuffTemplateSnapshot(BuffTemplateEntity target, BuffTemplateEntity snapshot)
        {
            target.Name = snapshot.Name;
            target.Description = snapshot.Description;
            target.Duration = snapshot.Duration;
            target.MaxStack = snapshot.MaxStack;
            target.StackRule = snapshot.StackRule;
            target.EffectsJson = snapshot.EffectsJson;
            target.SeedKey = snapshot.SeedKey;
            target.IsBuiltIn = true;
            target.BuiltInVersion = snapshot.BuiltInVersion;
            target.LastUpdateTime = snapshot.LastUpdateTime;
        }

        /// <summary>
        /// 初始化怪物数据
        /// </summary>
        public async Task SeedMonstersAsync()
        {
            var existingMonsterCount = await _dbContext.Db.Queryable<MonsterTemplateEntity>().CountAsync();
            if (existingMonsterCount > 0)
            {
                return;
            }

            var snapshot = BalanceCatalog.GetSeedSnapshot();
            if (snapshot.MonsterTemplates.Count == 0)
            {
                return;
            }

            await _dbContext.Db.Insertable(snapshot.MonsterTemplates.ToList()).ExecuteCommandAsync();
            _logger?.LogInformation("Seeded {Count} monster templates into SQLite from balance catalog", snapshot.MonsterTemplates.Count);
        }

        /// <summary>
        /// 初始化任务数据
        /// </summary>
        public async Task SeedMapsAsync()
        {
            var existingMapCount = await _dbContext.Db.Queryable<MapTemplateEntity>().CountAsync();
            if (existingMapCount > 0)
            {
                return;
            }

            var snapshot = BalanceCatalog.GetSeedSnapshot();
            if (snapshot.MapTemplates.Count == 0)
            {
                return;
            }

            await _dbContext.Db.Insertable(snapshot.MapTemplates.ToList()).ExecuteCommandAsync();
            _logger?.LogInformation("Seeded {Count} map templates into SQLite from balance catalog", snapshot.MapTemplates.Count);
        }

        /// <summary>
        /// 初始化任务种子数据。
        /// </summary>
        public async Task SeedQuestsAsync()
        {
            var now = DateTime.Now;
            var expectedQuests = QuestSeedCatalog.BuildDefaultQuestEntities(
                await BuildQuestSeedContextFromDatabaseAsync(),
                now);
            var existingQuests = await _dbContext.Db.Queryable<QuestConfigEntity>()
                .ToListAsync();
            var existingMap = existingQuests.ToDictionary(quest => quest.QuestId, StringComparer.OrdinalIgnoreCase);

            foreach (var expected in expectedQuests)
            {
                if (!existingMap.TryGetValue(expected.QuestId, out var existing))
                {
                    await _dbContext.Db.Insertable(expected).ExecuteCommandAsync();
                    continue;
                }

                var hasBuiltInIdentity = existing.IsBuiltIn ||
                    string.Equals(existing.SeedKey, expected.SeedKey, StringComparison.OrdinalIgnoreCase);
                if (!hasBuiltInIdentity && !MatchesQuestConfig(existing, expected))
                {
                    continue;
                }

                ApplyQuestConfigSnapshot(existing, expected, now);
                await _dbContext.Db.Updateable(existing).ExecuteCommandAsync();
            }

            _logger?.LogInformation("Ensured built-in quests exist with {Count} rows", expectedQuests.Count);
        }

        /// <summary>
        /// 初始化成就数据
        /// </summary>
        public async Task SeedAchievementsAsync()
        {
            var now = DateTime.Now;
            var expectedAchievements = AchievementSeedCatalog.BuildDefaultAchievementEntities(
                await BuildAchievementSeedContextFromDatabaseAsync(),
                now);
            var existingAchievements = await _dbContext.Db.Queryable<AchievementConfigEntity>()
                .ToListAsync();
            var existingMap = existingAchievements.ToDictionary(
                achievement => achievement.AchievementId,
                StringComparer.OrdinalIgnoreCase);

            foreach (var expected in expectedAchievements)
            {
                if (!existingMap.TryGetValue(expected.AchievementId, out var existing))
                {
                    await _dbContext.Db.Insertable(expected).ExecuteCommandAsync();
                    continue;
                }

                var hasBuiltInIdentity = existing.IsBuiltIn ||
                    string.Equals(existing.SeedKey, expected.SeedKey, StringComparison.OrdinalIgnoreCase);
                if (!hasBuiltInIdentity && !MatchesAchievementConfig(existing, expected))
                {
                    continue;
                }

                ApplyAchievementConfigSnapshot(existing, expected, now);
                await _dbContext.Db.Updateable(existing).ExecuteCommandAsync();
            }

            _logger?.LogInformation("Ensured built-in achievements exist with {Count} rows", expectedAchievements.Count);
        }

        private async Task<QuestSeedContext> BuildQuestSeedContextFromDatabaseAsync()
        {
            var maps = await _dbContext.Db.Queryable<MapTemplateEntity>()
                .Where(map => map.MapId == "map_001" || map.MapId == "map_002")
                .ToListAsync();
            var starterMap = maps.FirstOrDefault(map => string.Equals(map.MapId, "map_001", StringComparison.OrdinalIgnoreCase));
            var growthMap = maps.FirstOrDefault(map => string.Equals(map.MapId, "map_002", StringComparison.OrdinalIgnoreCase));
            var starterMonsterTargetId = starterMap?.SpawnRules
                .Where(rule => rule != null && !string.IsNullOrWhiteSpace(rule.MonsterTemplateId))
                .OrderByDescending(rule => rule.Weight)
                .Select(rule => rule.MonsterTemplateId.Trim())
                .FirstOrDefault();
            if (string.IsNullOrWhiteSpace(starterMonsterTargetId))
            {
                starterMonsterTargetId = "monster_slime_1";
            }

            var starterMonsterName = await _dbContext.Db.Queryable<MonsterTemplateEntity>()
                .Where(monster => monster.MonsterId == starterMonsterTargetId)
                .Select(monster => monster.Name)
                .FirstAsync();

            return new QuestSeedContext
            {
                StarterMonsterTargetId = starterMonsterTargetId,
                StarterMonsterName = string.IsNullOrWhiteSpace(starterMonsterName) ? "新手村妖物" : starterMonsterName,
                StarterMapId = "map_001",
                StarterMapName = string.IsNullOrWhiteSpace(starterMap?.Name) ? "新手村外" : starterMap.Name,
                GrowthMapId = "map_002",
                GrowthMapName = string.IsNullOrWhiteSpace(growthMap?.Name) ? "黑风坡" : growthMap.Name
            };
        }

        private async Task<AchievementSeedContext> BuildAchievementSeedContextFromDatabaseAsync()
        {
            var questContext = await BuildQuestSeedContextFromDatabaseAsync();
            var starterDungeon = global::XXX.Dungeon.DungeonCatalog.GetLegacyEntries()
                .OrderBy(entry => entry.RecommendedLevel)
                .FirstOrDefault();
            var starterSkill = await _dbContext.Db.Queryable<SkillTemplateEntity>()
                .OrderBy(skill => skill.SkillId)
                .FirstAsync();

            return new AchievementSeedContext
            {
                StarterMonsterId = questContext.StarterMonsterTargetId,
                StarterMonsterName = questContext.StarterMonsterName,
                StarterDungeonId = starterDungeon?.DungeonId ?? "dungeon_001",
                StarterDungeonName = string.IsNullOrWhiteSpace(starterDungeon?.Name) ? "新手试炼" : starterDungeon.Name,
                StarterSkillId = starterSkill?.SkillId > 0 ? starterSkill.SkillId.ToString() : "1",
                StarterSkillName = string.IsNullOrWhiteSpace(starterSkill?.Name) ? "普通攻击" : starterSkill.Name
            };
        }

        private static void ApplyQuestConfigSnapshot(QuestConfigEntity target, QuestConfigEntity snapshot, DateTime now)
        {
            target.SeedKey = snapshot.SeedKey;
            target.IsBuiltIn = true;
            target.BuiltInVersion = snapshot.BuiltInVersion;
            target.QuestName = snapshot.QuestName;
            target.QuestType = snapshot.QuestType;
            target.ResetCycle = snapshot.ResetCycle;
            target.Description = snapshot.Description;
            target.RequiredLevel = snapshot.RequiredLevel;
            target.PreQuestIds = snapshot.PreQuestIds;
            target.AutoAccept = snapshot.AutoAccept;
            target.AutoSubmit = snapshot.AutoSubmit;
            target.TimeLimit = snapshot.TimeLimit;
            target.RewardExp = snapshot.RewardExp;
            target.RewardGold = snapshot.RewardGold;
            target.RewardSpiritStone = snapshot.RewardSpiritStone;
            target.RewardItemsJson = snapshot.RewardItemsJson;
            target.RewardEquipmentIds = snapshot.RewardEquipmentIds;
            target.ObjectivesJson = snapshot.ObjectivesJson;
            target.SortOrder = snapshot.SortOrder;
            target.IsEnabled = snapshot.IsEnabled;
            target.LastUpdateTime = now;
        }

        private static bool MatchesQuestConfig(QuestConfigEntity actual, QuestConfigEntity expected)
        {
            return string.Equals(actual.QuestName, expected.QuestName, StringComparison.Ordinal) &&
                   actual.QuestType == expected.QuestType &&
                   actual.ResetCycle == expected.ResetCycle &&
                   string.Equals(actual.Description, expected.Description, StringComparison.Ordinal) &&
                   actual.RequiredLevel == expected.RequiredLevel &&
                   string.Equals(actual.PreQuestIds ?? string.Empty, expected.PreQuestIds ?? string.Empty, StringComparison.Ordinal) &&
                   actual.AutoAccept == expected.AutoAccept &&
                   actual.AutoSubmit == expected.AutoSubmit &&
                   actual.TimeLimit == expected.TimeLimit &&
                   actual.RewardExp == expected.RewardExp &&
                   actual.RewardGold == expected.RewardGold &&
                   actual.RewardSpiritStone == expected.RewardSpiritStone &&
                   string.Equals(actual.RewardItemsJson ?? string.Empty, expected.RewardItemsJson ?? string.Empty, StringComparison.Ordinal) &&
                   string.Equals(actual.RewardEquipmentIds ?? string.Empty, expected.RewardEquipmentIds ?? string.Empty, StringComparison.Ordinal) &&
                   string.Equals(actual.ObjectivesJson ?? string.Empty, expected.ObjectivesJson ?? string.Empty, StringComparison.Ordinal) &&
                   actual.SortOrder == expected.SortOrder &&
                   actual.IsEnabled == expected.IsEnabled;
        }

        private static void ApplyAchievementConfigSnapshot(AchievementConfigEntity target, AchievementConfigEntity snapshot, DateTime now)
        {
            target.SeedKey = snapshot.SeedKey;
            target.IsBuiltIn = true;
            target.BuiltInVersion = snapshot.BuiltInVersion;
            target.AchievementName = snapshot.AchievementName;
            target.AchievementType = snapshot.AchievementType;
            target.Difficulty = snapshot.Difficulty;
            target.Description = snapshot.Description;
            target.Category = snapshot.Category;
            target.Points = snapshot.Points;
            target.IsHidden = snapshot.IsHidden;
            target.PreAchievementIds = snapshot.PreAchievementIds;
            target.RewardGold = snapshot.RewardGold;
            target.RewardSpiritStone = snapshot.RewardSpiritStone;
            target.RewardExp = snapshot.RewardExp;
            target.RewardTitle = snapshot.RewardTitle;
            target.RewardEquipmentIds = snapshot.RewardEquipmentIds;
            target.RewardItemsJson = snapshot.RewardItemsJson;
            target.RequirementType = snapshot.RequirementType;
            target.RequirementTargetValue = snapshot.RequirementTargetValue;
            target.RequirementDescription = snapshot.RequirementDescription;
            target.RequirementsJson = snapshot.RequirementsJson;
            target.SortOrder = snapshot.SortOrder;
            target.IsEnabled = snapshot.IsEnabled;
            target.LastUpdateTime = now;
        }

        private static bool MatchesAchievementConfig(AchievementConfigEntity actual, AchievementConfigEntity expected)
        {
            return string.Equals(actual.AchievementName, expected.AchievementName, StringComparison.Ordinal) &&
                   actual.AchievementType == expected.AchievementType &&
                   actual.Difficulty == expected.Difficulty &&
                   string.Equals(actual.Description, expected.Description, StringComparison.Ordinal) &&
                   string.Equals(actual.Category, expected.Category, StringComparison.Ordinal) &&
                   actual.Points == expected.Points &&
                   actual.IsHidden == expected.IsHidden &&
                   string.Equals(actual.PreAchievementIds ?? string.Empty, expected.PreAchievementIds ?? string.Empty, StringComparison.Ordinal) &&
                   actual.RewardGold == expected.RewardGold &&
                   actual.RewardSpiritStone == expected.RewardSpiritStone &&
                   actual.RewardExp == expected.RewardExp &&
                   string.Equals(actual.RewardTitle ?? string.Empty, expected.RewardTitle ?? string.Empty, StringComparison.Ordinal) &&
                   string.Equals(actual.RewardItemsJson ?? string.Empty, expected.RewardItemsJson ?? string.Empty, StringComparison.Ordinal) &&
                   string.Equals(actual.RewardEquipmentIds ?? string.Empty, expected.RewardEquipmentIds ?? string.Empty, StringComparison.Ordinal) &&
                   actual.RequirementType == expected.RequirementType &&
                   actual.RequirementTargetValue == expected.RequirementTargetValue &&
                   string.Equals(actual.RequirementDescription ?? string.Empty, expected.RequirementDescription ?? string.Empty, StringComparison.Ordinal) &&
                   string.Equals(actual.RequirementsJson ?? string.Empty, expected.RequirementsJson ?? string.Empty, StringComparison.Ordinal) &&
                   actual.SortOrder == expected.SortOrder &&
                   actual.IsEnabled == expected.IsEnabled;
        }

        private async Task SeedCheckInRewardConfigsAsync()
        {
            var expectedConfigs = ActivityRewardCatalog.BuildCheckInRewards()
                .Select(seed => new CheckInRewardConfigEntity
                {
                    ContinuousDay = seed.ContinuousDay,
                    ConfigVersion = ActivityRewardCatalog.CheckInConfigVersion,
                    IsMilestone = seed.IsMilestone,
                    RewardJson = System.Text.Json.JsonSerializer.Serialize(seed.Rewards),
                    Description = $"连续签到第 {seed.ContinuousDay} 天奖励"
                })
                .OrderBy(config => config.ContinuousDay)
                .ToList();
            var existingConfigs = await _dbContext.Db.Queryable<CheckInRewardConfigEntity>()
                .OrderBy(config => config.ContinuousDay)
                .ToListAsync();

            if (!RequiresCheckInRewardConfigReseed(existingConfigs, expectedConfigs))
            {
                return;
            }

            await _dbContext.Db.Deleteable<CheckInRewardConfigEntity>().ExecuteCommandAsync();
            await _dbContext.Db.Insertable(expectedConfigs).ExecuteCommandAsync();
            _logger?.LogInformation("Seeded {Count} check-in reward configs into SQLite", expectedConfigs.Count);
        }

        private async Task SeedRedeemCodeConfigsAsync()
        {
            var expectedConfigs = ActivityRewardCatalog.BuildRedeemCodes()
                .Select(seed => new RedeemCodeConfigEntity
                {
                    Code = seed.Code,
                    ConfigVersion = ActivityRewardCatalog.RedeemCodeConfigVersion,
                    IsEnabled = true,
                    Description = seed.Description,
                    RewardJson = System.Text.Json.JsonSerializer.Serialize(seed.Rewards)
                })
                .OrderBy(config => config.Code)
                .ToList();
            var existingConfigs = await _dbContext.Db.Queryable<RedeemCodeConfigEntity>()
                .OrderBy(config => config.Code)
                .ToListAsync();

            if (!RequiresRedeemCodeConfigReseed(existingConfigs, expectedConfigs))
            {
                return;
            }

            await _dbContext.Db.Deleteable<RedeemCodeConfigEntity>().ExecuteCommandAsync();
            await _dbContext.Db.Insertable(expectedConfigs).ExecuteCommandAsync();
            _logger?.LogInformation("Seeded {Count} redeem code configs into SQLite", expectedConfigs.Count);
        }

        /// <summary>
        /// 初始化商店数据
        /// </summary>
        public async Task SeedShopsAsync()
        {
            var now = DateTime.Now;
            var db = _dbContext.Db;
            var existingShops = await db.Queryable<ShopConfigEntity>()
                .ToListAsync();
            var existingItems = await db.Queryable<ShopItemEntity>()
                .ToListAsync();
            var itemTemplates = await db.Queryable<ItemTemplateEntity>()
                .ToListAsync();
            var equipmentTemplates = await db.Queryable<EquipmentTemplateEntity>()
                .ToListAsync();
            // 确保宝石模板都有对应的道具条目（幂等，只补缺）
            await EnsureGemItemTemplatesAsync();

            // 重新加载道具列表（可能刚插入了宝石道具）
            itemTemplates = await db.Queryable<ItemTemplateEntity>()
                .ToListAsync();

            var expectedShops = BuildDefaultShopConfigs(now);
            var expectedItems = BuildExpectedDefaultShopItems(itemTemplates, equipmentTemplates, now);

            db.Ado.BeginTran();
            try
            {
                var existingShopMap = existingShops.ToDictionary(shop => shop.ShopId, StringComparer.OrdinalIgnoreCase);
                foreach (var expected in expectedShops)
                {
                    if (!existingShopMap.TryGetValue(expected.ShopId, out var existing))
                    {
                        await db.Insertable(expected).ExecuteCommandAsync();
                        continue;
                    }

                    if (existing.IsBuiltIn || string.Equals(existing.SeedKey, expected.SeedKey, StringComparison.OrdinalIgnoreCase))
                    {
                        existing.SeedKey = expected.SeedKey;
                        existing.IsBuiltIn = true;
                        existing.BuiltInVersion = expected.BuiltInVersion;
                        existing.LastUpdateTime = now;
                        await db.Updateable(existing)
                            .UpdateColumns(item => new { item.SeedKey, item.IsBuiltIn, item.BuiltInVersion, item.LastUpdateTime })
                            .ExecuteCommandAsync();
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(existing.SeedKey) || !MatchesDefaultShopConfig(existing, expected))
                    {
                        continue;
                    }

                    existing.SeedKey = expected.SeedKey;
                    existing.IsBuiltIn = true;
                    existing.BuiltInVersion = expected.BuiltInVersion;
                    existing.LastUpdateTime = now;
                    await db.Updateable(existing)
                        .UpdateColumns(item => new { item.SeedKey, item.IsBuiltIn, item.BuiltInVersion, item.LastUpdateTime })
                        .ExecuteCommandAsync();
                }

                var existingItemMap = existingItems.ToDictionary(
                    item => $"{item.ShopId}:{item.ItemType}:{item.ItemId}",
                    StringComparer.OrdinalIgnoreCase);
                foreach (var expected in expectedItems)
                {
                    var key = $"{expected.ShopId}:{expected.ItemType}:{expected.ItemId}";
                    if (!existingItemMap.TryGetValue(key, out var existing))
                    {
                        await db.Insertable(expected).ExecuteCommandAsync();
                        continue;
                    }

                    if (existing.IsBuiltIn || string.Equals(existing.SeedKey, expected.SeedKey, StringComparison.OrdinalIgnoreCase))
                    {
                        existing.SeedKey = expected.SeedKey;
                        existing.IsBuiltIn = true;
                        existing.BuiltInVersion = expected.BuiltInVersion;
                        existing.LastUpdateTime = now;
                        await db.Updateable(existing)
                            .UpdateColumns(item => new { item.SeedKey, item.IsBuiltIn, item.BuiltInVersion, item.LastUpdateTime })
                            .ExecuteCommandAsync();
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(existing.SeedKey) || !MatchesDefaultShopItem(existing, expected))
                    {
                        continue;
                    }

                    existing.SeedKey = expected.SeedKey;
                    existing.IsBuiltIn = true;
                    existing.BuiltInVersion = expected.BuiltInVersion;
                    existing.LastUpdateTime = now;
                    await db.Updateable(existing)
                        .UpdateColumns(item => new { item.SeedKey, item.IsBuiltIn, item.BuiltInVersion, item.LastUpdateTime })
                        .ExecuteCommandAsync();
                }

                db.Ado.CommitTran();
                _logger?.LogInformation(
                    "Ensured built-in shop configs exist with {ShopCount} shops and {ItemCount} items",
                    expectedShops.Count,
                    expectedItems.Count);
            }
            catch
            {
                db.Ado.RollbackTran();
                throw;
            }
        }

        private async Task SeedRankingsAsync()
        {
            var now = DateTime.Now;
            var db = _dbContext.Db;
            var existingConfigs = await db.Queryable<RankingConfigEntity>()
                .ToListAsync();
            var existingRewards = await db.Queryable<RankingRewardEntity>()
                .ToListAsync();
            var expectedConfigs = BuildDefaultRankingSeeds(now);
            var expectedRewards = BuildDefaultRankingRewards(now);

            db.Ado.BeginTran();
            try
            {
                // 中文注释：
                // 战力榜已从产品榜单中移除。旧数据库可能仍然残留配置、榜单条目和奖励，
                // 必须在本次 Seed 事务内幂等清理，避免服务重启后 RankingService 再次加载旧榜。
                // 这里只清理战力榜的物化数据，不删除 ranking_history 历史快照，也不触碰玩家战斗属性。
                await db.Deleteable<RankingEntryEntity>()
                    .Where(entry => entry.RankingId == "ranking_combat")
                    .ExecuteCommandAsync();
                await db.Deleteable<RankingRewardEntity>()
                    .Where(reward => reward.RankingId == "ranking_combat")
                    .ExecuteCommandAsync();
                await db.Deleteable<RankingConfigEntity>()
                    .Where(config => config.RankingId == "ranking_combat")
                    .ExecuteCommandAsync();

                var existingConfigMap = existingConfigs.ToDictionary(config => config.RankingId, StringComparer.OrdinalIgnoreCase);
                foreach (var expected in expectedConfigs)
                {
                    if (!existingConfigMap.TryGetValue(expected.RankingId, out var existing))
                    {
                        await db.Insertable(expected).ExecuteCommandAsync();
                        continue;
                    }

                    if (existing.IsBuiltIn || string.Equals(existing.SeedKey, expected.SeedKey, StringComparison.OrdinalIgnoreCase))
                    {
                        existing.RankingName = expected.RankingName;
                        existing.RankingType = expected.RankingType;
                        existing.Description = expected.Description;
                        existing.MaxSize = expected.MaxSize;
                        existing.UpdateInterval = expected.UpdateInterval;
                        existing.SeasonEnabled = expected.SeasonEnabled;
                        existing.SeasonDuration = expected.SeasonDuration;
                        existing.CurrentSeason = existing.CurrentSeason > 0 ? existing.CurrentSeason : expected.CurrentSeason;
                        existing.SeasonStartTime ??= expected.SeasonStartTime;
                        existing.SortOrder = expected.SortOrder;
                        existing.IsEnabled = expected.IsEnabled;
                        existing.SeedKey = expected.SeedKey;
                        existing.IsBuiltIn = true;
                        existing.BuiltInVersion = expected.BuiltInVersion;
                        existing.LastUpdateTime = now;
                        await db.Updateable(existing).ExecuteCommandAsync();
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(existing.SeedKey) || !MatchesDefaultRankingConfig(existing, expected))
                    {
                        continue;
                    }

                    existing.SeedKey = expected.SeedKey;
                    existing.IsBuiltIn = true;
                    existing.BuiltInVersion = expected.BuiltInVersion;
                    existing.LastUpdateTime = now;
                    if (existing.CurrentSeason <= 0)
                    {
                        existing.CurrentSeason = expected.CurrentSeason;
                    }

                    existing.SeasonStartTime ??= expected.SeasonStartTime;
                    await db.Updateable(existing).ExecuteCommandAsync();
                }

                var existingRewardMap = existingRewards.ToDictionary(
                    reward => $"{reward.RankingId}:{reward.MinRank}:{reward.MaxRank}",
                    StringComparer.OrdinalIgnoreCase);
                foreach (var expected in expectedRewards)
                {
                    var key = $"{expected.RankingId}:{expected.MinRank}:{expected.MaxRank}";
                    if (!existingRewardMap.TryGetValue(key, out var existing))
                    {
                        await db.Insertable(expected).ExecuteCommandAsync();
                        continue;
                    }

                    if (existing.IsBuiltIn || string.Equals(existing.SeedKey, expected.SeedKey, StringComparison.OrdinalIgnoreCase))
                    {
                        existing.RewardTitle = expected.RewardTitle;
                        existing.Gold = expected.Gold;
                        existing.SpiritStone = expected.SpiritStone;
                        existing.Title = expected.Title;
                        existing.SeedKey = expected.SeedKey;
                        existing.IsBuiltIn = true;
                        existing.BuiltInVersion = expected.BuiltInVersion;
                        existing.LastUpdateTime = now;
                        await db.Updateable(existing).ExecuteCommandAsync();
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(existing.SeedKey) || !MatchesDefaultRankingReward(existing, expected))
                    {
                        continue;
                    }

                    existing.SeedKey = expected.SeedKey;
                    existing.IsBuiltIn = true;
                    existing.BuiltInVersion = expected.BuiltInVersion;
                    existing.LastUpdateTime = now;
                    await db.Updateable(existing)
                        .UpdateColumns(item => new { item.SeedKey, item.IsBuiltIn, item.BuiltInVersion, item.LastUpdateTime })
                        .ExecuteCommandAsync();
                }

                db.Ado.CommitTran();
                _logger?.LogInformation(
                    "Ensured built-in ranking configs exist with {ConfigCount} configs and {RewardCount} rewards",
                    expectedConfigs.Count,
                    expectedRewards.Count);
            }
            catch
            {
                db.Ado.RollbackTran();
                throw;
            }
        }

        private static List<ShopConfigEntity> BuildDefaultShopConfigs(DateTime now)
        {
            return
            [
                new ShopConfigEntity
                {
                    ShopId = GeneralShopId,
                    ShopName = "杂货铺",
                    ShopType = (int)ShopType.General,
                    Description = "出售药水、宠物与炼丹基础材料，保障前中期日常补给。",
                    RequiredLevel = 1,
                    RequiredVipLevel = 0,
                    Discount = 1.0f,
                    IsOpen = true,
                    AutoRefresh = false,
                    SortOrder = 1,
                    IsEnabled = true,
                    SeedKey = BuildShopSeedKey(GeneralShopId),
                    IsBuiltIn = true,
                    BuiltInVersion = ShopBuiltInVersion,
                    LastUpdateTime = now
                },
                new ShopConfigEntity
                {
                    ShopId = BlacksmithShopId,
                    ShopName = "铁匠铺",
                    ShopType = (int)ShopType.Blacksmith,
                    Description = "出售入门装备与低阶锻造材料，确保锻造链路始终有真实货源。",
                    RequiredLevel = 1,
                    RequiredVipLevel = 0,
                    Discount = 1.0f,
                    IsOpen = true,
                    AutoRefresh = true,
                    RefreshIntervalHours = 24,
                    SortOrder = 2,
                    IsEnabled = true,
                    SeedKey = BuildShopSeedKey(BlacksmithShopId),
                    IsBuiltIn = true,
                    BuiltInVersion = ShopBuiltInVersion,
                    LastUpdateTime = now
                }
            ];
        }

        private static List<ShopItemEntity> BuildExpectedDefaultShopItems(
            IReadOnlyCollection<ItemTemplateEntity> itemTemplates,
            IReadOnlyCollection<EquipmentTemplateEntity> equipmentTemplates,
            DateTime now)
        {
            var itemTemplateMap = itemTemplates.ToDictionary(item => item.ItemId, StringComparer.OrdinalIgnoreCase);
            var dynamicItems = new List<ShopItemEntity>();

            var sortOrder = 1;
            foreach (var itemId in GetDefaultGeneralShopItemIds(itemTemplateMap.Keys))
            {
                dynamicItems.Add(CreateDefaultItemShopEntry(GeneralShopId, itemTemplateMap[itemId], sortOrder++, now));
            }

            sortOrder = 1;
            foreach (var template in GetDefaultBlacksmithEquipmentTemplates(equipmentTemplates))
            {
                dynamicItems.Add(CreateDefaultEquipmentShopEntry(BlacksmithShopId, template, sortOrder++, now));
            }

            foreach (var itemId in GetDefaultBlacksmithMaterialItemIds(itemTemplateMap.Keys))
            {
                dynamicItems.Add(CreateDefaultItemShopEntry(BlacksmithShopId, itemTemplateMap[itemId], sortOrder++, now));
            }

            return dynamicItems;
        }

        private static ShopItemEntity CreateDefaultItemShopEntry(
            string shopId,
            ItemTemplateEntity item,
            int sortOrder,
            DateTime now)
        {
            var basePrice = GetDefaultItemPrice(item.ItemId, item);
            var stock = item.Type == (int)ItemType.Consumable ? 40 : 20;
            var dailyLimit = item.Type == (int)ItemType.Consumable ? 20 : 8;

            if (item.ItemId is "pet_egg" or "evolution_stone")
            {
                stock = 8;
                dailyLimit = 2;
            }

            return new ShopItemEntity
            {
                GID = Guid.NewGuid().ToString("N"),
                ShopId = shopId,
                ItemId = item.ItemId,
                ItemType = (int)ShopItemType.Item,
                BasePrice = basePrice,
                CurrentPrice = basePrice,
                Stock = stock,
                InitialStock = stock,
                DailyLimit = dailyLimit,
                SortOrder = sortOrder,
                IsEnabled = true,
                SeedKey = BuildShopItemSeedKey(shopId, (int)ShopItemType.Item, item.ItemId),
                IsBuiltIn = true,
                BuiltInVersion = ShopBuiltInVersion,
                LastUpdateTime = now
            };
        }

        private static ShopItemEntity CreateDefaultEquipmentShopEntry(
            string shopId,
            EquipmentTemplateEntity template,
            int sortOrder,
            DateTime now)
        {
            var price = GetEquipmentBasePrice(template);
            var stock = template.Slot == (int)EquipmentSlot.Weapon ? 4 : 2;
            return new ShopItemEntity
            {
                GID = Guid.NewGuid().ToString("N"),
                ShopId = shopId,
                ItemId = template.EquipmentId.ToString(),
                ItemType = (int)ShopItemType.Equipment,
                BasePrice = price,
                CurrentPrice = price,
                Stock = stock,
                InitialStock = stock,
                DailyLimit = 1,
                SortOrder = sortOrder,
                IsEnabled = true,
                SeedKey = BuildShopItemSeedKey(shopId, (int)ShopItemType.Equipment, template.EquipmentId.ToString()),
                IsBuiltIn = true,
                BuiltInVersion = ShopBuiltInVersion,
                LastUpdateTime = now
            };
        }

        private static List<string> GetDefaultGeneralShopItemIds(IEnumerable<string> existingItemIds)
        {
            var existingSet = existingItemIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
            return GeneralShopItemIds
                .Where(existingSet.Contains)
                .ToList();
        }

        private static List<string> GetDefaultBlacksmithMaterialItemIds(IEnumerable<string> existingItemIds)
        {
            var existingSet = existingItemIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
            return BlacksmithMaterialItemIds
                .Where(existingSet.Contains)
                .ToList();
        }

        private static List<EquipmentTemplateEntity> GetDefaultBlacksmithEquipmentTemplates(
            IEnumerable<EquipmentTemplateEntity> equipmentTemplates)
        {
            return equipmentTemplates
                .Where(template => template.EquipmentId >= 1000 && (template.Level == 1 || template.Level == 5))
                .OrderBy(template => template.Level)
                .ThenBy(template => template.Slot == (int)EquipmentSlot.Weapon ? 0 : 1)
                .ThenBy(template => template.CombatStyle == (int)CombatStyle.Physical ? 0 : 1)
                .ThenBy(template => template.EquipmentId)
                .ToList();
        }

        private static int GetEquipmentBasePrice(EquipmentTemplateEntity template)
        {
            var slotBasePrice = template.Slot switch
            {
                (int)EquipmentSlot.Weapon => 760,
                (int)EquipmentSlot.Armor => 650,
                (int)EquipmentSlot.Helmet => 560,
                (int)EquipmentSlot.Pants => 560,
                (int)EquipmentSlot.Boots => 530,
                (int)EquipmentSlot.Necklace => 520,
                (int)EquipmentSlot.Ring => 470,
                (int)EquipmentSlot.Treasure => 500,
                _ => 420
            };
            var levelPrice = template.Level * 120;
            var qualityPrice = template.Quality * 160;
            var stylePrice = template.CombatStyle == (int)CombatStyle.Magic ? 30 : 0;
            return slotBasePrice + levelPrice + qualityPrice + stylePrice;
        }

        /// <summary>
        /// 确保每个宝石模板在 ItemTemplates 中都有对应的道具条目（幂等，只补缺）。
        /// </summary>
        private async Task EnsureGemItemTemplatesAsync()
        {
            var db = _dbContext.Db;
            var now = DateTime.Now;
            var gemTemplates = await db.Queryable<GemTemplateEntity>().ToListAsync();
            if (gemTemplates.Count == 0)
            {
                return;
            }

            var existingItemIds = (await db.Queryable<ItemTemplateEntity>()
                .Select(i => i.ItemId)
                .ToListAsync())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var inserted = 0;
            foreach (var gem in gemTemplates)
            {
                if (existingItemIds.Contains(gem.GemId))
                {
                    continue;
                }

                await db.Insertable(new ItemTemplateEntity
                {
                    ItemId = gem.GemId,
                    Name = gem.Name,
                    UseLevel = 1,
                    Type = (int)ItemType.Gem,
                    Description = BuildGemDescription(gem),
                    MaxStack = 999,
                    Quality = gem.Quality,
                    IconPath = gem.IconPath,
                    IsTradeable = true,
                    SeedKey = $"built-in:gem-item:{gem.GemId}",
                    IsBuiltIn = true,
                    BuiltInVersion = ShopBuiltInVersion,
                    LastUpdateTime = now
                }).ExecuteCommandAsync();
                inserted++;
            }

            if (inserted > 0)
            {
                _logger?.LogInformation("Auto-created {Count} gem item templates from GemTemplates", inserted);
            }
        }

        private static string BuildGemDescription(GemTemplateEntity gem)
        {
            var modeText = string.Equals(gem.BonusMode, "Percent", StringComparison.OrdinalIgnoreCase) ? "百分比" : "固定值";
            var attrName = gem.AttributeType switch
            {
                "Type1" => "生命",
                "Type2" => "法力",
                "Type3" => "物攻",
                "Type4" => "法攻",
                "Type5" => "物防",
                "Type6" => "法防",
                "Type7" => "速度",
                "Type8" => "命中",
                "Type9" => "闪避",
                "Type10" => "暴击",
                "Type11" => "暴伤",
                "Type12" => "连击",
                "Type13" => "反击",
                "Type14" => "破甲",
                "Type15" => "附伤",
                _ => gem.AttributeType
            };
            var suffix = string.Equals(gem.BonusMode, "Percent", StringComparison.OrdinalIgnoreCase) ? "%" : "";
            return $"Lv.{gem.Level} {attrName}宝石，{attrName} +{gem.BonusValue}{suffix}（{modeText}）";
        }

        private static int GetDefaultItemPrice(string itemId, ItemTemplateEntity item)
        {
            return itemId switch
            {
                "item_001" => 60,
                "item_002" => 180,
                "item_006" => 70,
                "item_007" => 190,
                "pet_egg" => 420,
                "pet_food_basic" => 80,
                "evolution_stone" => 560,
                "seed_blood_grass" => 45,
                "seed_spirit_lotus" => 65,
                "seed_stone_moss" => 90,
                "alchemy_herb" => 50,
                "spirit_water" => 70,
                "spirit_dust" => 90,
                "item_003" => 120,
                "ore_t1" => 130,
                "wood_t1" => 110,
                "hide_t1" => 110,
                "crystal_t1" => 140,
                _ => Math.Max(40, 40 + item.UseLevel * 18 + item.Quality * 25)
            };
        }

        private static bool MatchesDefaultShopConfig(ShopConfigEntity actual, ShopConfigEntity expected)
        {
            return string.Equals(actual.ShopName, expected.ShopName, StringComparison.Ordinal) &&
                   actual.ShopType == expected.ShopType &&
                   string.Equals(actual.Description ?? string.Empty, expected.Description ?? string.Empty, StringComparison.Ordinal) &&
                   actual.RequiredLevel == expected.RequiredLevel &&
                   actual.RequiredVipLevel == expected.RequiredVipLevel &&
                   Math.Abs(actual.Discount - expected.Discount) < 0.0001f &&
                   actual.IsOpen == expected.IsOpen &&
                   actual.AutoRefresh == expected.AutoRefresh &&
                   actual.RefreshIntervalHours == expected.RefreshIntervalHours &&
                   string.Equals(actual.Icon ?? string.Empty, expected.Icon ?? string.Empty, StringComparison.Ordinal) &&
                   actual.SortOrder == expected.SortOrder &&
                   actual.IsEnabled == expected.IsEnabled;
        }

        private static bool MatchesDefaultShopItem(ShopItemEntity actual, ShopItemEntity expected)
        {
            return string.Equals(actual.ShopId, expected.ShopId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(actual.ItemId, expected.ItemId, StringComparison.OrdinalIgnoreCase) &&
                   actual.ItemType == expected.ItemType &&
                   actual.BasePrice == expected.BasePrice &&
                   actual.CurrentPrice == expected.CurrentPrice &&
                   actual.Stock == expected.Stock &&
                   actual.InitialStock == expected.InitialStock &&
                   actual.DailyLimit == expected.DailyLimit &&
                   actual.SortOrder == expected.SortOrder &&
                   actual.IsEnabled == expected.IsEnabled;
        }

        private static List<RankingConfigEntity> BuildDefaultRankingSeeds(DateTime now)
        {
            return
            [
                new RankingConfigEntity
                {
                    RankingId = "ranking_level",
                    RankingName = "等级排行榜",
                    RankingType = (int)RankingType.Level,
                    Description = "按玩家等级排序",
                    MaxSize = 100,
                    UpdateInterval = 60,
                    SeasonEnabled = false,
                    SortOrder = 1,
                    IsEnabled = true,
                    SeedKey = BuildRankingSeedKey("ranking_level"),
                    IsBuiltIn = true,
                    BuiltInVersion = RankingBuiltInVersion,
                    LastUpdateTime = now
                },
                new RankingConfigEntity
                {
                    RankingId = "ranking_arena",
                    RankingName = "竞技场排行榜",
                    RankingType = (int)RankingType.Arena,
                    Description = "按竞技场积分排序",
                    MaxSize = 50,
                    UpdateInterval = 30,
                    SeasonEnabled = true,
                    SeasonDuration = 30,
                    CurrentSeason = 1,
                    SeasonStartTime = now,
                    SortOrder = 3,
                    IsEnabled = true,
                    SeedKey = BuildRankingSeedKey("ranking_arena"),
                    IsBuiltIn = true,
                    BuiltInVersion = RankingBuiltInVersion,
                    LastUpdateTime = now
                },
                new RankingConfigEntity
                {
                    RankingId = "ranking_achievement",
                    RankingName = "成就排行榜",
                    RankingType = (int)RankingType.Achievement,
                    Description = "按成就点数排序",
                    MaxSize = 100,
                    UpdateInterval = 60,
                    SeasonEnabled = false,
                    SortOrder = 4,
                    IsEnabled = true,
                    SeedKey = BuildRankingSeedKey("ranking_achievement"),
                    IsBuiltIn = true,
                    BuiltInVersion = RankingBuiltInVersion,
                    LastUpdateTime = now
                },
                new RankingConfigEntity
                {
                    RankingId = "ranking_wealth",
                    RankingName = "财富排行榜",
                    RankingType = (int)RankingType.Wealth,
                    Description = "按金币数量排序",
                    MaxSize = 100,
                    UpdateInterval = 60,
                    SeasonEnabled = false,
                    SortOrder = 5,
                    IsEnabled = true,
                    SeedKey = BuildRankingSeedKey("ranking_wealth"),
                    IsBuiltIn = true,
                    BuiltInVersion = RankingBuiltInVersion,
                    LastUpdateTime = now
                },
                new RankingConfigEntity
                {
                    RankingId = "ranking_tower",
                    RankingName = "通天塔排行榜",
                    RankingType = (int)RankingType.Tower,
                    Description = "按最高通关层排名，同层按通关时间排序",
                    MaxSize = 100,
                    UpdateInterval = 60,
                    SeasonEnabled = false,
                    SortOrder = 6,
                    IsEnabled = true,
                    SeedKey = BuildRankingSeedKey("ranking_tower"),
                    IsBuiltIn = true,
                    BuiltInVersion = RankingBuiltInVersion,
                    LastUpdateTime = now
                }
            ];
        }

        private static List<RankingRewardEntity> BuildDefaultRankingRewards(DateTime now)
        {
            return
            [
                new RankingRewardEntity
                {
                    GID = Guid.NewGuid().ToString("N"),
                    RankingId = "ranking_level",
                    MinRank = 1,
                    MaxRank = 1,
                    RewardTitle = "等级之王",
                    Gold = 10000,
                    SpiritStone = 500,
                    Title = "等级王者",
                    SeedKey = BuildRankingRewardSeedKey("ranking_level", 1, 1),
                    IsBuiltIn = true,
                    BuiltInVersion = RankingBuiltInVersion,
                    LastUpdateTime = now
                },
                new RankingRewardEntity
                {
                    GID = Guid.NewGuid().ToString("N"),
                    RankingId = "ranking_level",
                    MinRank = 2,
                    MaxRank = 3,
                    RewardTitle = "等级精英",
                    Gold = 5000,
                    SpiritStone = 200,
                    SeedKey = BuildRankingRewardSeedKey("ranking_level", 2, 3),
                    IsBuiltIn = true,
                    BuiltInVersion = RankingBuiltInVersion,
                    LastUpdateTime = now
                },
                new RankingRewardEntity
                {
                    GID = Guid.NewGuid().ToString("N"),
                    RankingId = "ranking_tower",
                    MinRank = 1,
                    MaxRank = 1,
                    RewardTitle = "登天者",
                    Gold = 20000,
                    SpiritStone = 1000,
                    Title = "tower_master",
                    SeedKey = BuildRankingRewardSeedKey("ranking_tower", 1, 1),
                    IsBuiltIn = true,
                    BuiltInVersion = RankingBuiltInVersion,
                    LastUpdateTime = now
                },
                new RankingRewardEntity
                {
                    GID = Guid.NewGuid().ToString("N"),
                    RankingId = "ranking_tower",
                    MinRank = 2,
                    MaxRank = 3,
                    RewardTitle = "攀天者",
                    Gold = 10000,
                    SpiritStone = 500,
                    SeedKey = BuildRankingRewardSeedKey("ranking_tower", 2, 3),
                    IsBuiltIn = true,
                    BuiltInVersion = RankingBuiltInVersion,
                    LastUpdateTime = now
                }
            ];
        }

        private static bool MatchesDefaultRankingConfig(RankingConfigEntity actual, RankingConfigEntity expected)
        {
            return string.Equals(actual.RankingName, expected.RankingName, StringComparison.Ordinal) &&
                   actual.RankingType == expected.RankingType &&
                   string.Equals(actual.Description ?? string.Empty, expected.Description ?? string.Empty, StringComparison.Ordinal) &&
                   actual.MaxSize == expected.MaxSize &&
                   actual.UpdateInterval == expected.UpdateInterval &&
                   actual.SeasonEnabled == expected.SeasonEnabled &&
                   actual.SeasonDuration == expected.SeasonDuration &&
                   actual.SortOrder == expected.SortOrder &&
                   actual.IsEnabled == expected.IsEnabled;
        }

        private static bool MatchesDefaultRankingReward(RankingRewardEntity actual, RankingRewardEntity expected)
        {
            return string.Equals(actual.RankingId, expected.RankingId, StringComparison.OrdinalIgnoreCase) &&
                   actual.MinRank == expected.MinRank &&
                   actual.MaxRank == expected.MaxRank &&
                   string.Equals(actual.RewardTitle, expected.RewardTitle, StringComparison.Ordinal) &&
                   actual.Gold == expected.Gold &&
                   actual.SpiritStone == expected.SpiritStone &&
                   string.Equals(actual.Title ?? string.Empty, expected.Title ?? string.Empty, StringComparison.Ordinal);
        }

        private static string BuildShopSeedKey(string shopId) => $"built-in:shop:{shopId}";

        private static string BuildShopItemSeedKey(string shopId, int itemType, string itemId) => $"built-in:shop-item:{shopId}:{itemType}:{itemId}";

        private static string BuildRankingSeedKey(string rankingId) => $"built-in:ranking:{rankingId}";

        private static string BuildRankingRewardSeedKey(string rankingId, int minRank, int maxRank) => $"built-in:ranking-reward:{rankingId}:{minRank}:{maxRank}";

        public async Task SeedFiveElementRulesAsync()
        {
            var now = DateTime.Now;

            var levelConfigs = Enumerable.Range(1, 50)
                .Select(level =>
                {
                    var materialCount = 1 + (level - 1) / 5;
                    return new FiveElementLevelConfigEntity
                    {
                        ArrayLevel = level,
                        UpgradeGoldCost = 400 + level * 180L,
                        UpgradeSpiritStoneCost = 3 + level * 2L,
                        UpgradeMaterialsJson = System.Text.Json.JsonSerializer.Serialize(new[]
                        {
                            new { ItemId = "ore_t1", Count = materialCount },
                            new { ItemId = "wood_t1", Count = materialCount },
                            new { ItemId = "spirit_water", Count = materialCount },
                            new { ItemId = "crystal_t1", Count = materialCount },
                            new { ItemId = "hide_t1", Count = materialCount }
                        }),
                        SpiritFieldYieldBonusPercent = level >= 50 ? 50 : Math.Max(0, (level - 1) / 5) * 5,
                        BattleExpBonusPercent = level >= 50 ? 30 : Math.Max(0, (level - 1) / 5) * 3,
                        ProfessionLevelCap = level,
                        IsEnabled = true,
                        SeedKey = $"built-in:five-element:level:{level}",
                        IsBuiltIn = true,
                        BuiltInVersion = FiveElementRuleVersion,
                        LastUpdateTime = now
                    };
                })
                .ToList();

            var elements = new[] { "metal", "wood", "water", "fire", "earth" };
            var branchConfigs = new List<FiveElementBranchUpgradeConfigEntity>();
            foreach (var element in elements)
            {
                for (var targetLevel = 1; targetLevel <= 50; targetLevel++)
                {
                    var costLevel = Math.Clamp(targetLevel, 1, 50);
                    var itemId = element switch
                    {
                        "metal" => GetTieredItemId("ore", costLevel),
                        "wood" => GetTieredItemId("wood", costLevel),
                        "water" => "spirit_water",
                        "fire" => GetTieredItemId("crystal", costLevel),
                        "earth" => GetTieredItemId("hide", costLevel),
                        _ => "ore_t1"
                    };
                    var count = targetLevel <= 1 ? 0 : 2 + (costLevel - 1) / 4;
                    branchConfigs.Add(new FiveElementBranchUpgradeConfigEntity
                    {
                        GID = Guid.NewGuid().ToString("N"),
                        ElementType = element,
                        TargetLevel = targetLevel,
                        GoldCost = targetLevel <= 1 ? 0 : 160 + costLevel * 120L,
                        SpiritStoneCost = targetLevel <= 1 ? 0 : 2 + costLevel,
                        MaterialsJson = count <= 0
                            ? "[]"
                            : System.Text.Json.JsonSerializer.Serialize(new[]
                            {
                                new { ItemId = itemId, Count = count }
                            }),
                        SortOrder = targetLevel,
                        IsEnabled = true,
                        SeedKey = $"built-in:five-element:branch:{element}:{targetLevel}",
                        IsBuiltIn = true,
                        BuiltInVersion = FiveElementRuleVersion,
                        LastUpdateTime = now
                    });
                }
            }

            foreach (var expected in levelConfigs)
            {
                var existing = await _dbContext.Db.Queryable<FiveElementLevelConfigEntity>()
                    .FirstAsync(item => item.ArrayLevel == expected.ArrayLevel);
                if (existing == null)
                {
                    await _dbContext.Db.Insertable(expected).ExecuteCommandAsync();
                    continue;
                }

                var canOverwrite = existing.IsBuiltIn ||
                    string.Equals(existing.SeedKey, expected.SeedKey, StringComparison.OrdinalIgnoreCase);
                if (!canOverwrite)
                {
                    continue;
                }

                existing.UpgradeGoldCost = expected.UpgradeGoldCost;
                existing.UpgradeSpiritStoneCost = expected.UpgradeSpiritStoneCost;
                existing.UpgradeMaterialsJson = expected.UpgradeMaterialsJson;
                existing.SpiritFieldYieldBonusPercent = expected.SpiritFieldYieldBonusPercent;
                existing.BattleExpBonusPercent = expected.BattleExpBonusPercent;
                existing.ProfessionLevelCap = expected.ProfessionLevelCap;
                existing.IsEnabled = expected.IsEnabled;
                existing.SeedKey = expected.SeedKey;
                existing.IsBuiltIn = true;
                existing.BuiltInVersion = expected.BuiltInVersion;
                existing.LastUpdateTime = now;
                await _dbContext.Db.Updateable(existing).ExecuteCommandAsync();
            }

            foreach (var expected in branchConfigs)
            {
                var existing = await _dbContext.Db.Queryable<FiveElementBranchUpgradeConfigEntity>()
                    .FirstAsync(item => item.ElementType == expected.ElementType && item.TargetLevel == expected.TargetLevel);
                if (existing == null)
                {
                    await _dbContext.Db.Insertable(expected).ExecuteCommandAsync();
                    continue;
                }

                var canOverwrite = existing.IsBuiltIn ||
                    string.Equals(existing.SeedKey, expected.SeedKey, StringComparison.OrdinalIgnoreCase);
                if (!canOverwrite)
                {
                    continue;
                }

                existing.GoldCost = expected.GoldCost;
                existing.SpiritStoneCost = expected.SpiritStoneCost;
                existing.MaterialsJson = expected.MaterialsJson;
                existing.SortOrder = expected.SortOrder;
                existing.IsEnabled = expected.IsEnabled;
                existing.SeedKey = expected.SeedKey;
                existing.IsBuiltIn = true;
                existing.BuiltInVersion = expected.BuiltInVersion;
                existing.LastUpdateTime = now;
                await _dbContext.Db.Updateable(existing).ExecuteCommandAsync();
            }

            _logger?.LogInformation("Ensured built-in five-element rule configs exist with {LevelCount} level rows and {BranchCount} branch rows", levelConfigs.Count, branchConfigs.Count);
        }

        /// <summary>
        /// 初始化聚灵阵五行区间规则。
        /// 默认使用一条 1～500 区间，后续可由管理端拆分为多段维护。
        /// </summary>
        private async Task SeedFiveElementBranchRuleRangesAsync()
        {
            var now = DateTime.Now;
            var defaults = new[]
            {
                (ElementType: "metal", AttributeType: "Type3", ItemId: "ore_t1"),
                (ElementType: "wood", AttributeType: "Type1", ItemId: "wood_t1"),
                (ElementType: "water", AttributeType: "Type6", ItemId: "spirit_water"),
                (ElementType: "fire", AttributeType: "Type4", ItemId: "crystal_t1"),
                (ElementType: "earth", AttributeType: "Type5", ItemId: "hide_t1")
            };

            foreach (var item in defaults)
            {
                var gid = $"built-in:five-element:range:{item.ElementType}:1-500";
                var expected = new FiveElementBranchRuleRangeEntity
                {
                    GID = gid,
                    ElementType = item.ElementType,
                    MinLevel = 1,
                    MaxLevel = 500,
                    AttributeType = item.AttributeType,
                    BonusPerLevel = 10,
                    GoldCost = 160,
                    SpiritStoneCost = 3,
                    MaterialsJson = System.Text.Json.JsonSerializer.Serialize(new[] { new { ItemId = item.ItemId, Count = 2 } }),
                    SortOrder = Array.IndexOf(defaults, item),
                    IsEnabled = true,
                    SeedKey = gid,
                    IsBuiltIn = true,
                    BuiltInVersion = FiveElementRuleVersion,
                    LastUpdateTime = now
                };

                var existing = await _dbContext.Db.Queryable<FiveElementBranchRuleRangeEntity>().FirstAsync(x => x.GID == gid);
                if (existing == null)
                {
                    await _dbContext.Db.Insertable(expected).ExecuteCommandAsync();
                }
                else if (existing.IsBuiltIn || string.Equals(existing.SeedKey, gid, StringComparison.OrdinalIgnoreCase))
                {
                    existing.ElementType = expected.ElementType;
                    existing.MinLevel = expected.MinLevel;
                    existing.MaxLevel = expected.MaxLevel;
                    existing.AttributeType = expected.AttributeType;
                    existing.BonusPerLevel = expected.BonusPerLevel;
                    existing.GoldCost = expected.GoldCost;
                    existing.SpiritStoneCost = expected.SpiritStoneCost;
                    existing.MaterialsJson = expected.MaterialsJson;
                    existing.SortOrder = expected.SortOrder;
                    existing.IsEnabled = expected.IsEnabled;
                    existing.IsBuiltIn = true;
                    existing.BuiltInVersion = expected.BuiltInVersion;
                    existing.LastUpdateTime = now;
                    await _dbContext.Db.Updateable(existing).ExecuteCommandAsync();
                }
            }
        }

        public async Task SeedSpiritFieldRulesAsync()
        {
            var now = DateTime.Now;
            var systemConfig = new SpiritFieldSystemConfigEntity
            {
                ConfigId = "default",
                DefaultFieldLevel = 1,
                DefaultUnlockedPlots = 3,
                DefaultMaxPlots = 9,
                DefaultInventoryCapacity = 100,
                PlotUpgradeGoldPerLevel = 1000,
                PlotUpgradeSpiritStonePerLevel = 10,
                PlotUpgradeYieldBonusPerLevel = 5,
                IsEnabled = true,
                SeedKey = "built-in:spirit-field:system:default",
                IsBuiltIn = true,
                BuiltInVersion = SpiritFieldRuleVersion,
                LastUpdateTime = now
            };

            var existingSystem = await _dbContext.Db.Queryable<SpiritFieldSystemConfigEntity>()
                .FirstAsync(item => item.ConfigId == systemConfig.ConfigId);
            if (existingSystem == null)
            {
                await _dbContext.Db.Insertable(systemConfig).ExecuteCommandAsync();
            }
            else if (existingSystem.IsBuiltIn || string.Equals(existingSystem.SeedKey, systemConfig.SeedKey, StringComparison.OrdinalIgnoreCase))
            {
                existingSystem.DefaultFieldLevel = systemConfig.DefaultFieldLevel;
                existingSystem.DefaultUnlockedPlots = systemConfig.DefaultUnlockedPlots;
                existingSystem.DefaultMaxPlots = systemConfig.DefaultMaxPlots;
                existingSystem.DefaultInventoryCapacity = systemConfig.DefaultInventoryCapacity;
                existingSystem.PlotUpgradeGoldPerLevel = systemConfig.PlotUpgradeGoldPerLevel;
                existingSystem.PlotUpgradeSpiritStonePerLevel = systemConfig.PlotUpgradeSpiritStonePerLevel;
                existingSystem.PlotUpgradeYieldBonusPerLevel = systemConfig.PlotUpgradeYieldBonusPerLevel;
                existingSystem.IsEnabled = systemConfig.IsEnabled;
                existingSystem.SeedKey = systemConfig.SeedKey;
                existingSystem.IsBuiltIn = true;
                existingSystem.BuiltInVersion = systemConfig.BuiltInVersion;
                existingSystem.LastUpdateTime = now;
                await _dbContext.Db.Updateable(existingSystem).ExecuteCommandAsync();
            }

            var speedUpItems = new List<SpiritFieldSpeedUpItemConfigEntity>
            {
                CreateSpiritFieldSpeedUpItem("alchemy_herb", 3600, 1),
                CreateSpiritFieldSpeedUpItem("spirit_water", 3600, 2),
                CreateSpiritFieldSpeedUpItem("spirit_dust", 3600, 3)
            };

            foreach (var expected in speedUpItems)
            {
                var existing = await _dbContext.Db.Queryable<SpiritFieldSpeedUpItemConfigEntity>()
                    .FirstAsync(item => item.ItemId == expected.ItemId);
                if (existing == null)
                {
                    await _dbContext.Db.Insertable(expected).ExecuteCommandAsync();
                    continue;
                }

                if (!(existing.IsBuiltIn || string.Equals(existing.SeedKey, expected.SeedKey, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                existing.SpeedUpSeconds = expected.SpeedUpSeconds;
                existing.SortOrder = expected.SortOrder;
                existing.IsEnabled = expected.IsEnabled;
                existing.SeedKey = expected.SeedKey;
                existing.IsBuiltIn = true;
                existing.BuiltInVersion = expected.BuiltInVersion;
                existing.LastUpdateTime = now;
                await _dbContext.Db.Updateable(existing).ExecuteCommandAsync();
            }

            _logger?.LogInformation("Ensured built-in spirit field rule config exists with {SpeedUpCount} speed-up items", speedUpItems.Count);
        }

        public async Task SeedAlchemyProfessionRulesAsync()
        {
            var now = DateTime.Now;
            var levelConfigs = Enumerable.Range(1, 50)
                .Select(level => new AlchemyProfessionLevelConfigEntity
                {
                    Level = level,
                    NextLevelExp = 80 + (level - 1) * 40,
                    IsEnabled = true,
                    SeedKey = $"built-in:alchemy-profession:level:{level}",
                    IsBuiltIn = true,
                    BuiltInVersion = AlchemyProfessionRuleVersion,
                    LastUpdateTime = now
                })
                .ToList();

            foreach (var expected in levelConfigs)
            {
                var existing = await _dbContext.Db.Queryable<AlchemyProfessionLevelConfigEntity>()
                    .FirstAsync(item => item.Level == expected.Level);
                if (existing == null)
                {
                    await _dbContext.Db.Insertable(expected).ExecuteCommandAsync();
                    continue;
                }

                if (!(existing.IsBuiltIn || string.Equals(existing.SeedKey, expected.SeedKey, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                existing.NextLevelExp = expected.NextLevelExp;
                existing.IsEnabled = expected.IsEnabled;
                existing.SeedKey = expected.SeedKey;
                existing.IsBuiltIn = true;
                existing.BuiltInVersion = expected.BuiltInVersion;
                existing.LastUpdateTime = now;
                await _dbContext.Db.Updateable(existing).ExecuteCommandAsync();
            }

            var ruleConfig = new AlchemyProfessionRuleConfigEntity
            {
                ConfigId = "default",
                SuccessBonusPerOverLevel = 1,
                MaxSuccessBonus = 15,
                SuccessExpBase = 12,
                SuccessExpPerRequiredLevel = 4,
                FailureExpBase = 6,
                FailureExpPerRequiredLevel = 2,
                IsEnabled = true,
                SeedKey = "built-in:alchemy-profession:rule:default",
                IsBuiltIn = true,
                BuiltInVersion = AlchemyProfessionRuleVersion,
                LastUpdateTime = now
            };

            var existingRule = await _dbContext.Db.Queryable<AlchemyProfessionRuleConfigEntity>()
                .FirstAsync(item => item.ConfigId == ruleConfig.ConfigId);
            if (existingRule == null)
            {
                await _dbContext.Db.Insertable(ruleConfig).ExecuteCommandAsync();
            }
            else if (existingRule.IsBuiltIn || string.Equals(existingRule.SeedKey, ruleConfig.SeedKey, StringComparison.OrdinalIgnoreCase))
            {
                existingRule.SuccessBonusPerOverLevel = ruleConfig.SuccessBonusPerOverLevel;
                existingRule.MaxSuccessBonus = ruleConfig.MaxSuccessBonus;
                existingRule.SuccessExpBase = ruleConfig.SuccessExpBase;
                existingRule.SuccessExpPerRequiredLevel = ruleConfig.SuccessExpPerRequiredLevel;
                existingRule.FailureExpBase = ruleConfig.FailureExpBase;
                existingRule.FailureExpPerRequiredLevel = ruleConfig.FailureExpPerRequiredLevel;
                existingRule.IsEnabled = ruleConfig.IsEnabled;
                existingRule.SeedKey = ruleConfig.SeedKey;
                existingRule.IsBuiltIn = true;
                existingRule.BuiltInVersion = ruleConfig.BuiltInVersion;
                existingRule.LastUpdateTime = now;
                await _dbContext.Db.Updateable(existingRule).ExecuteCommandAsync();
            }

            _logger?.LogInformation("Ensured built-in alchemy profession rule config exists with {LevelCount} level rows", levelConfigs.Count);
        }

        public async Task SeedForgeProfessionRulesAsync()
        {
            var now = DateTime.Now;
            var levelConfigs = Enumerable.Range(1, 50)
                .Select(level => new ForgeProfessionLevelConfigEntity
                {
                    Level = level,
                    NextLevelExp = 80 + (level - 1) * 40,
                    IsEnabled = true,
                    SeedKey = $"built-in:forge-profession:level:{level}",
                    IsBuiltIn = true,
                    BuiltInVersion = ForgeProfessionRuleVersion,
                    LastUpdateTime = now
                })
                .ToList();

            foreach (var expected in levelConfigs)
            {
                var existing = await _dbContext.Db.Queryable<ForgeProfessionLevelConfigEntity>()
                    .FirstAsync(item => item.Level == expected.Level);
                if (existing == null)
                {
                    await _dbContext.Db.Insertable(expected).ExecuteCommandAsync();
                    continue;
                }

                if (!(existing.IsBuiltIn || string.Equals(existing.SeedKey, expected.SeedKey, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                existing.NextLevelExp = expected.NextLevelExp;
                existing.IsEnabled = expected.IsEnabled;
                existing.SeedKey = expected.SeedKey;
                existing.IsBuiltIn = true;
                existing.BuiltInVersion = expected.BuiltInVersion;
                existing.LastUpdateTime = now;
                await _dbContext.Db.Updateable(existing).ExecuteCommandAsync();
            }

            var ruleConfig = new ForgeProfessionRuleConfigEntity
            {
                ConfigId = "default",
                SuccessBonusPerOverLevel = 1,
                MaxSuccessBonus = 15,
                SuccessExpBase = 12,
                SuccessExpPerRequiredLevel = 4,
                FailureExpBase = 6,
                FailureExpPerRequiredLevel = 2,
                IsEnabled = true,
                SeedKey = "built-in:forge-profession:rule:default",
                IsBuiltIn = true,
                BuiltInVersion = ForgeProfessionRuleVersion,
                LastUpdateTime = now
            };

            var existingRule = await _dbContext.Db.Queryable<ForgeProfessionRuleConfigEntity>()
                .FirstAsync(item => item.ConfigId == ruleConfig.ConfigId);
            if (existingRule == null)
            {
                await _dbContext.Db.Insertable(ruleConfig).ExecuteCommandAsync();
            }
            else if (existingRule.IsBuiltIn || string.Equals(existingRule.SeedKey, ruleConfig.SeedKey, StringComparison.OrdinalIgnoreCase))
            {
                existingRule.SuccessBonusPerOverLevel = ruleConfig.SuccessBonusPerOverLevel;
                existingRule.MaxSuccessBonus = ruleConfig.MaxSuccessBonus;
                existingRule.SuccessExpBase = ruleConfig.SuccessExpBase;
                existingRule.SuccessExpPerRequiredLevel = ruleConfig.SuccessExpPerRequiredLevel;
                existingRule.FailureExpBase = ruleConfig.FailureExpBase;
                existingRule.FailureExpPerRequiredLevel = ruleConfig.FailureExpPerRequiredLevel;
                existingRule.IsEnabled = ruleConfig.IsEnabled;
                existingRule.SeedKey = ruleConfig.SeedKey;
                existingRule.IsBuiltIn = true;
                existingRule.BuiltInVersion = ruleConfig.BuiltInVersion;
                existingRule.LastUpdateTime = now;
                await _dbContext.Db.Updateable(existingRule).ExecuteCommandAsync();
            }

            _logger?.LogInformation("Ensured built-in forge profession rule config exists with {LevelCount} level rows", levelConfigs.Count);
        }

        public async Task SeedEquipmentRerollRulesAsync()
        {
            var now = DateTime.Now;

            // 0. 洗练石道具模板
            var rerollStoneExists = await _dbContext.Db.Queryable<ItemTemplateEntity>()
                .AnyAsync(item => item.ItemId == "reroll_stone");
            if (!rerollStoneExists)
            {
                await _dbContext.Db.Insertable(new ItemTemplateEntity
                {
                    ItemId = "reroll_stone",
                    Name = "洗练石",
                    UseLevel = 1,
                    Type = 0,
                    Description = "用于装备洗练的神秘石头，可以重置装备的附加属性。",
                    MaxStack = 999,
                    Quality = 2,
                    IconPath = null,
                    SeedKey = "built-in:item:reroll_stone",
                    IsBuiltIn = true,
                    BuiltInVersion = EquipmentRerollRuleVersion,
                    LastUpdateTime = now
                }).ExecuteCommandAsync();
            }

            // 1. 系统配置
            var systemConfig = new EquipmentRerollSystemConfigEntity
            {
                ConfigId = "default",
                IsEnabled = true,
                RerollStoneItemId = "reroll_stone",
                BaseStoneCost = 1,
                ExtraStoneCostPerLockedLine = 1,
                MaxLockedLineCount = 3,
                BaseGoldCost = 300,
                GoldCostPerEquipmentLevel = 25,
                QualityGoldMultipliersJson = "{\"1\":1.0,\"2\":1.2,\"3\":1.5,\"4\":2.0,\"5\":2.8}",
                RerollCountGoldGrowthPercent = 3,
                SeedKey = "built-in:equipment-reroll:system:default",
                IsBuiltIn = true,
                BuiltInVersion = EquipmentRerollRuleVersion,
                LastUpdateTime = now
            };

            var existingSystem = await _dbContext.Db.Queryable<EquipmentRerollSystemConfigEntity>()
                .FirstAsync(item => item.ConfigId == systemConfig.ConfigId);
            if (existingSystem == null)
            {
                await _dbContext.Db.Insertable(systemConfig).ExecuteCommandAsync();
            }
            else if (existingSystem.IsBuiltIn || string.Equals(existingSystem.SeedKey, systemConfig.SeedKey, StringComparison.OrdinalIgnoreCase))
            {
                existingSystem.IsEnabled = systemConfig.IsEnabled;
                existingSystem.RerollStoneItemId = systemConfig.RerollStoneItemId;
                existingSystem.BaseStoneCost = systemConfig.BaseStoneCost;
                existingSystem.ExtraStoneCostPerLockedLine = systemConfig.ExtraStoneCostPerLockedLine;
                existingSystem.MaxLockedLineCount = systemConfig.MaxLockedLineCount;
                existingSystem.BaseGoldCost = systemConfig.BaseGoldCost;
                existingSystem.GoldCostPerEquipmentLevel = systemConfig.GoldCostPerEquipmentLevel;
                existingSystem.QualityGoldMultipliersJson = systemConfig.QualityGoldMultipliersJson;
                existingSystem.RerollCountGoldGrowthPercent = systemConfig.RerollCountGoldGrowthPercent;
                existingSystem.SeedKey = systemConfig.SeedKey;
                existingSystem.IsBuiltIn = true;
                existingSystem.BuiltInVersion = systemConfig.BuiltInVersion;
                existingSystem.LastUpdateTime = now;
                await _dbContext.Db.Updateable(existingSystem).ExecuteCommandAsync();
            }

            // 2. 品阶配置
            var tierConfigs = new List<EquipmentRerollTierConfigEntity>
            {
                new() { Tier = 1, Name = "普通", Color = "#9ca3af", Weight = 55, ValueMultiplier = "1.00", SortOrder = 1, SeedKey = "built-in:equipment-reroll:tier:1", IsBuiltIn = true, BuiltInVersion = EquipmentRerollRuleVersion, LastUpdateTime = now },
                new() { Tier = 2, Name = "优秀", Color = "#22c55e", Weight = 25, ValueMultiplier = "1.25", SortOrder = 2, SeedKey = "built-in:equipment-reroll:tier:2", IsBuiltIn = true, BuiltInVersion = EquipmentRerollRuleVersion, LastUpdateTime = now },
                new() { Tier = 3, Name = "精良", Color = "#38bdf8", Weight = 12, ValueMultiplier = "1.55", SortOrder = 3, SeedKey = "built-in:equipment-reroll:tier:3", IsBuiltIn = true, BuiltInVersion = EquipmentRerollRuleVersion, LastUpdateTime = now },
                new() { Tier = 4, Name = "史诗", Color = "#a855f7", Weight = 6, ValueMultiplier = "1.90", SortOrder = 4, SeedKey = "built-in:equipment-reroll:tier:4", IsBuiltIn = true, BuiltInVersion = EquipmentRerollRuleVersion, LastUpdateTime = now },
                new() { Tier = 5, Name = "传说", Color = "#f97316", Weight = 2, ValueMultiplier = "2.30", SortOrder = 5, SeedKey = "built-in:equipment-reroll:tier:5", IsBuiltIn = true, BuiltInVersion = EquipmentRerollRuleVersion, LastUpdateTime = now }
            };

            foreach (var expected in tierConfigs)
            {
                var existing = await _dbContext.Db.Queryable<EquipmentRerollTierConfigEntity>()
                    .FirstAsync(item => item.SeedKey == expected.SeedKey);
                if (existing == null)
                {
                    await _dbContext.Db.Insertable(expected).ExecuteCommandAsync();
                    continue;
                }

                if (!(existing.IsBuiltIn || string.Equals(existing.SeedKey, expected.SeedKey, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                existing.Tier = expected.Tier;
                existing.Name = expected.Name;
                existing.Color = expected.Color;
                existing.Weight = expected.Weight;
                existing.ValueMultiplier = expected.ValueMultiplier;
                existing.SortOrder = expected.SortOrder;
                existing.IsEnabled = true;
                existing.SeedKey = expected.SeedKey;
                existing.IsBuiltIn = true;
                existing.BuiltInVersion = expected.BuiltInVersion;
                existing.LastUpdateTime = now;
                await _dbContext.Db.Updateable(existing).ExecuteCommandAsync();
            }

            // 3. 槽位词条池
            var poolConfigs = BuildDefaultSlotPoolConfigs(now);
            var existingPools = await _dbContext.Db.Queryable<EquipmentRerollSlotPoolConfigEntity>()
                .ToListAsync();
            var existingPoolMap = existingPools
                .Where(item => item.SeedKey != null)
                .ToDictionary(item => item.SeedKey!, item => item);

            foreach (var expected in poolConfigs)
            {
                if (expected.SeedKey != null && existingPoolMap.TryGetValue(expected.SeedKey, out var existing))
                {
                    if (!(existing.IsBuiltIn || string.Equals(existing.SeedKey, expected.SeedKey, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    existing.Slot = expected.Slot;
                    existing.AttributeType = expected.AttributeType;
                    existing.Tier = expected.Tier;
                    existing.MaxDuplicateCount = expected.MaxDuplicateCount;
                    existing.SortOrder = expected.SortOrder;
                    existing.IsEnabled = true;
                    existing.IsBuiltIn = true;
                    existing.BuiltInVersion = expected.BuiltInVersion;
                    existing.LastUpdateTime = now;
                    await _dbContext.Db.Updateable(existing).ExecuteCommandAsync();
                }
                else
                {
                    await _dbContext.Db.Insertable(expected).ExecuteCommandAsync();
                }
            }

            // 4. 属性值配置
            var attrValueConfigs = BuildDefaultAttributeValueConfigs(now);
            var existingAttrValues = await _dbContext.Db.Queryable<EquipmentRerollAttributeValueConfigEntity>()
                .ToListAsync();
            var existingAttrValueMap = existingAttrValues
                .Where(item => item.SeedKey != null)
                .ToDictionary(item => item.SeedKey!, item => item);

            foreach (var expected in attrValueConfigs)
            {
                if (expected.SeedKey != null && existingAttrValueMap.TryGetValue(expected.SeedKey, out var existing))
                {
                    if (!(existing.IsBuiltIn || string.Equals(existing.SeedKey, expected.SeedKey, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    existing.AttributeType = expected.AttributeType;
                    existing.Tier = expected.Tier;
                    existing.MinValue = expected.MinValue;
                    existing.MaxValue = expected.MaxValue;
                    existing.IsPercentage = expected.IsPercentage;
                    existing.SortOrder = expected.SortOrder;
                    existing.IsEnabled = true;
                    existing.IsBuiltIn = true;
                    existing.BuiltInVersion = expected.BuiltInVersion;
                    existing.LastUpdateTime = now;
                    await _dbContext.Db.Updateable(existing).ExecuteCommandAsync();
                }
                else
                {
                    await _dbContext.Db.Insertable(expected).ExecuteCommandAsync();
                }
            }

            _logger?.LogInformation("Ensured built-in equipment reroll rules exist with {PoolCount} pool entries, {AttrValueCount} attribute value configs", poolConfigs.Count, attrValueConfigs.Count);
            await SeedEquipmentUpgradeCostRulesAsync();
        }

        /// <summary>
        /// 初始化按装备需求等级匹配的强化、洗炼材料与金币规则。
        /// </summary>
        private async Task SeedEquipmentUpgradeCostRulesAsync()
        {
            var now = DateTime.Now;
            var enhanceData = new[]
            {
                (1, 30, "itm_enhance_s01", 1, 200L, 100, 10),
                (31, 60, "itm_enhance_s04", 2, 700L, 92, 10),
                (61, 90, "itm_enhance_s07", 4, 1600L, 80, 10),
                (91, 120, "itm_enhance_s10", 6, 3000L, 68, 10)
            };
            var enhanceSeedKeys = enhanceData
                .Select(item => $"built-in:equipment-enhance-rule:{item.Item1}-{item.Item2}")
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var obsoleteEnhanceRules = await _dbContext.Db.Queryable<EquipmentEnhanceRuleEntity>()
                .Where(rule => rule.IsBuiltIn && rule.SeedKey != null)
                .ToListAsync();
            foreach (var obsolete in obsoleteEnhanceRules.Where(rule => !enhanceSeedKeys.Contains(rule.SeedKey!)))
            {
                await _dbContext.Db.Deleteable<EquipmentEnhanceRuleEntity>()
                    .Where(rule => rule.GID == obsolete.GID)
                    .ExecuteCommandAsync();
            }

            for (var index = 0; index < enhanceData.Length; index++)
            {
                var item = enhanceData[index];
                var seedKey = $"built-in:equipment-enhance-rule:{item.Item1}-{item.Item2}";
                var expected = new EquipmentEnhanceRuleEntity
                {
                    MinEquipmentLevel = item.Item1,
                    MaxEquipmentLevel = item.Item2,
                    MaterialItemId = item.Item3,
                    MaterialCount = item.Item4,
                    GoldCost = item.Item5,
                    SuccessRate = item.Item6,
                    AttributeGrowthPercent = item.Item7,
                    MaxEnhanceLevel = 15,
                    SortOrder = index + 1,
                    IsEnabled = true,
                    SeedKey = seedKey,
                    IsBuiltIn = true,
                    BuiltInVersion = EquipmentEnhanceRuleVersion,
                    LastUpdateTime = now
                };
                var existing = await _dbContext.Db.Queryable<EquipmentEnhanceRuleEntity>()
                    .FirstAsync(rule => rule.SeedKey == seedKey);
                if (existing == null)
                {
                    await _dbContext.Db.Insertable(expected).ExecuteCommandAsync();
                }
                else if (existing.IsBuiltIn || string.Equals(existing.SeedKey, seedKey, StringComparison.OrdinalIgnoreCase))
                {
                    existing.MinEquipmentLevel = expected.MinEquipmentLevel;
                    existing.MaxEquipmentLevel = expected.MaxEquipmentLevel;
                    existing.MaterialItemId = expected.MaterialItemId;
                    existing.MaterialCount = expected.MaterialCount;
                    existing.GoldCost = expected.GoldCost;
                    existing.SuccessRate = expected.SuccessRate;
                    existing.AttributeGrowthPercent = expected.AttributeGrowthPercent;
                    existing.MaxEnhanceLevel = expected.MaxEnhanceLevel;
                    existing.SortOrder = expected.SortOrder;
                    existing.IsEnabled = true;
                    existing.IsBuiltIn = true;
                    existing.BuiltInVersion = expected.BuiltInVersion;
                    existing.LastUpdateTime = now;
                    await _dbContext.Db.Updateable(existing).ExecuteCommandAsync();
                }
            }

            var rerollData = new[]
            {
                (1, 30, "itm_reroll_s01", 1, 300L),
                (31, 60, "itm_reroll_s04", 2, 900L),
                (61, 90, "itm_reroll_s07", 3, 1800L),
                (91, 120, "itm_reroll_s10", 5, 3200L)
            };
            var rerollSeedKeys = rerollData
                .Select(item => $"built-in:equipment-reroll-cost-rule:{item.Item1}-{item.Item2}")
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var obsoleteRerollRules = await _dbContext.Db.Queryable<EquipmentRerollCostRuleEntity>()
                .Where(rule => rule.IsBuiltIn && rule.SeedKey != null)
                .ToListAsync();
            foreach (var obsolete in obsoleteRerollRules.Where(rule => !rerollSeedKeys.Contains(rule.SeedKey!)))
            {
                await _dbContext.Db.Deleteable<EquipmentRerollCostRuleEntity>()
                    .Where(rule => rule.GID == obsolete.GID)
                    .ExecuteCommandAsync();
            }

            for (var index = 0; index < rerollData.Length; index++)
            {
                var item = rerollData[index];
                var seedKey = $"built-in:equipment-reroll-cost-rule:{item.Item1}-{item.Item2}";
                var expected = new EquipmentRerollCostRuleEntity
                {
                    MinEquipmentLevel = item.Item1,
                    MaxEquipmentLevel = item.Item2,
                    MaterialItemId = item.Item3,
                    MaterialCount = item.Item4,
                    GoldCost = item.Item5,
                    ExtraMaterialPerLockedLine = 1,
                    ExtraGoldPerLockedLine = 0,
                    SortOrder = index + 1,
                    IsEnabled = true,
                    SeedKey = seedKey,
                    IsBuiltIn = true,
                    BuiltInVersion = EquipmentEnhanceRuleVersion,
                    LastUpdateTime = now
                };
                var existing = await _dbContext.Db.Queryable<EquipmentRerollCostRuleEntity>()
                    .FirstAsync(rule => rule.SeedKey == seedKey);
                if (existing == null)
                {
                    await _dbContext.Db.Insertable(expected).ExecuteCommandAsync();
                }
                else if (existing.IsBuiltIn || string.Equals(existing.SeedKey, seedKey, StringComparison.OrdinalIgnoreCase))
                {
                    existing.MinEquipmentLevel = expected.MinEquipmentLevel;
                    existing.MaxEquipmentLevel = expected.MaxEquipmentLevel;
                    existing.MaterialItemId = expected.MaterialItemId;
                    existing.MaterialCount = expected.MaterialCount;
                    existing.GoldCost = expected.GoldCost;
                    existing.ExtraMaterialPerLockedLine = expected.ExtraMaterialPerLockedLine;
                    existing.ExtraGoldPerLockedLine = expected.ExtraGoldPerLockedLine;
                    existing.SortOrder = expected.SortOrder;
                    existing.IsEnabled = true;
                    existing.IsBuiltIn = true;
                    existing.BuiltInVersion = expected.BuiltInVersion;
                    existing.LastUpdateTime = now;
                    await _dbContext.Db.Updateable(existing).ExecuteCommandAsync();
                }
            }
        }

        private static List<EquipmentRerollSlotPoolConfigEntity> BuildDefaultSlotPoolConfigs(DateTime now)
        {
            var version = EquipmentRerollRuleVersion;
            var pools = new List<EquipmentRerollSlotPoolConfigEntity>();

            // 每个部位 × 每个属性类型 × 每个品阶 生成一条词条池记录
            void AddPool(EquipmentSlot slot, int attrType, int tier)
            {
                pools.Add(new EquipmentRerollSlotPoolConfigEntity
                {
                    Slot = slot,
                    AttributeType = attrType,
                    Tier = tier,
                    MaxDuplicateCount = 1,
                    SortOrder = pools.Count + 1,
                    IsEnabled = true,
                    SeedKey = $"built-in:equipment-reroll:pool:{(int)slot}:{attrType}:{tier}",
                    IsBuiltIn = true,
                    BuiltInVersion = version,
                    LastUpdateTime = now
                });
            }

            // 为指定部位的所有属性类型生成 1~5 品阶条目
            void AddSlotPools(EquipmentSlot slot, int[] attrTypes)
            {
                foreach (var attr in attrTypes)
                {
                    for (var tier = 1; tier <= 5; tier++)
                    {
                        AddPool(slot, attr, tier);
                    }
                }
            }

            // Weapon: 物攻, 法攻, 命中, 暴击, 暴伤, 连击, 破甲, 额外伤害
            AddSlotPools(EquipmentSlot.Weapon, [3, 4, 8, 10, 11, 12, 14, 15]);

            // Helmet: 血量, 蓝量, 物防, 法防, 命中, 反击
            AddSlotPools(EquipmentSlot.Helmet, [1, 2, 5, 6, 8, 13]);

            // Armor: 血量, 物防, 法防, 闪避, 反击
            AddSlotPools(EquipmentSlot.Armor, [1, 5, 6, 9, 13]);

            // Pants: 血量, 物防, 法防, 速度, 反击
            AddSlotPools(EquipmentSlot.Pants, [1, 5, 6, 7, 13]);

            // Boots: 速度, 闪避, 命中, 反击, 血量
            AddSlotPools(EquipmentSlot.Boots, [7, 9, 8, 13, 1]);

            // Necklace: 命中, 速度, 蓝量, 物攻, 法攻, 额外伤害
            AddSlotPools(EquipmentSlot.Necklace, [3, 4, 8, 7, 2, 15]);

            // Ring: 物攻, 法攻, 暴击, 暴伤, 连击, 破甲, 额外伤害
            AddSlotPools(EquipmentSlot.Ring, [3, 4, 10, 11, 12, 14, 15]);

            return pools;
        }

        private static List<EquipmentRerollAttributeValueConfigEntity> BuildDefaultAttributeValueConfigs(DateTime now)
        {
            var version = EquipmentRerollRuleVersion;
            var configs = new List<EquipmentRerollAttributeValueConfigEntity>();

            void AddAttrValue(int attrType, int tier, string min, string max, bool isPct)
            {
                configs.Add(new EquipmentRerollAttributeValueConfigEntity
                {
                    AttributeType = attrType,
                    Tier = tier,
                    MinValue = min,
                    MaxValue = max,
                    IsPercentage = isPct,
                    SortOrder = configs.Count + 1,
                    IsEnabled = true,
                    SeedKey = $"built-in:equipment-reroll:attrval:{attrType}:{tier}",
                    IsBuiltIn = true,
                    BuiltInVersion = version,
                    LastUpdateTime = now
                });
            }

            // === 固定值属性 ===
            // 血量 (AttrType=1): 普通 25-40, 优秀 41-60, 精良 61-80, 史诗 81-100, 传说 101-120
            AddAttrValue(1, 1, "25", "40", false);
            AddAttrValue(1, 2, "41", "60", false);
            AddAttrValue(1, 3, "61", "80", false);
            AddAttrValue(1, 4, "81", "100", false);
            AddAttrValue(1, 5, "101", "120", false);

            // 蓝量 (AttrType=2): 普通 12-20, 优秀 21-30, 精良 31-40, 史诗 41-52, 传说 53-65
            AddAttrValue(2, 1, "12", "20", false);
            AddAttrValue(2, 2, "21", "30", false);
            AddAttrValue(2, 3, "31", "40", false);
            AddAttrValue(2, 4, "41", "52", false);
            AddAttrValue(2, 5, "53", "65", false);

            // 物攻 (AttrType=3): 普通 6-10, 优秀 11-15, 精良 16-20, 史诗 21-26, 传说 27-32
            AddAttrValue(3, 1, "6", "10", false);
            AddAttrValue(3, 2, "11", "15", false);
            AddAttrValue(3, 3, "16", "20", false);
            AddAttrValue(3, 4, "21", "26", false);
            AddAttrValue(3, 5, "27", "32", false);

            // 法攻 (AttrType=4): 普通 6-10, 优秀 11-15, 精良 16-20, 史诗 21-26, 传说 27-32
            AddAttrValue(4, 1, "6", "10", false);
            AddAttrValue(4, 2, "11", "15", false);
            AddAttrValue(4, 3, "16", "20", false);
            AddAttrValue(4, 4, "21", "26", false);
            AddAttrValue(4, 5, "27", "32", false);

            // 物防 (AttrType=5): 普通 5-8, 优秀 9-13, 精良 14-18, 史诗 19-24, 传说 25-30
            AddAttrValue(5, 1, "5", "8", false);
            AddAttrValue(5, 2, "9", "13", false);
            AddAttrValue(5, 3, "14", "18", false);
            AddAttrValue(5, 4, "19", "24", false);
            AddAttrValue(5, 5, "25", "30", false);

            // 法防 (AttrType=6): 普通 5-8, 优秀 9-13, 精良 14-18, 史诗 19-24, 传说 25-30
            AddAttrValue(6, 1, "5", "8", false);
            AddAttrValue(6, 2, "9", "13", false);
            AddAttrValue(6, 3, "14", "18", false);
            AddAttrValue(6, 4, "19", "24", false);
            AddAttrValue(6, 5, "25", "30", false);

            // 速度 (AttrType=7): 普通 2-4, 优秀 5-7, 精良 8-10, 史诗 11-13, 传说 14-16
            AddAttrValue(7, 1, "2", "4", false);
            AddAttrValue(7, 2, "5", "7", false);
            AddAttrValue(7, 3, "8", "10", false);
            AddAttrValue(7, 4, "11", "13", false);
            AddAttrValue(7, 5, "14", "16", false);

            // === 百分比属性 ===
            // 命中率 (AttrType=8): 普通 0.5-1%, 优秀 1.5-2%, 精良 2.5-3%, 史诗 3.5-4.5%, 传说 5-6%
            AddAttrValue(8, 1, "0.005", "0.01", true);
            AddAttrValue(8, 2, "0.015", "0.02", true);
            AddAttrValue(8, 3, "0.025", "0.03", true);
            AddAttrValue(8, 4, "0.035", "0.045", true);
            AddAttrValue(8, 5, "0.05", "0.06", true);

            // 闪避率 (AttrType=9): 普通 0.3-0.8%, 优秀 1-1.5%, 精良 1.8-2.3%, 史诗 2.5-3.2%, 传说 3.5-4.5%
            AddAttrValue(9, 1, "0.003", "0.008", true);
            AddAttrValue(9, 2, "0.01", "0.015", true);
            AddAttrValue(9, 3, "0.018", "0.023", true);
            AddAttrValue(9, 4, "0.025", "0.032", true);
            AddAttrValue(9, 5, "0.035", "0.045", true);

            // 暴击率 (AttrType=10): 普通 0.5-1%, 优秀 1.5-2%, 精良 2.5-3%, 史诗 3.5-4.5%, 传说 5-6%
            AddAttrValue(10, 1, "0.005", "0.01", true);
            AddAttrValue(10, 2, "0.015", "0.02", true);
            AddAttrValue(10, 3, "0.025", "0.03", true);
            AddAttrValue(10, 4, "0.035", "0.045", true);
            AddAttrValue(10, 5, "0.05", "0.06", true);

            // 暴击伤害 (AttrType=11): 普通 3-5%, 优秀 6-9%, 精良 10-14%, 史诗 15-20%, 传说 21-28%
            AddAttrValue(11, 1, "0.03", "0.05", true);
            AddAttrValue(11, 2, "0.06", "0.09", true);
            AddAttrValue(11, 3, "0.10", "0.14", true);
            AddAttrValue(11, 4, "0.15", "0.20", true);
            AddAttrValue(11, 5, "0.21", "0.28", true);

            // 连击率 (AttrType=12): 普通 0.3-0.8%, 优秀 1-1.5%, 精良 1.8-2.3%, 史诗 2.5-3.2%, 传说 3.5-4.5%
            AddAttrValue(12, 1, "0.003", "0.008", true);
            AddAttrValue(12, 2, "0.01", "0.015", true);
            AddAttrValue(12, 3, "0.018", "0.023", true);
            AddAttrValue(12, 4, "0.025", "0.032", true);
            AddAttrValue(12, 5, "0.035", "0.045", true);

            // 反击率 (AttrType=13): 普通 0.3-0.8%, 优秀 1-1.5%, 精良 1.8-2.3%, 史诗 2.5-3.2%, 传说 3.5-4.5%
            AddAttrValue(13, 1, "0.003", "0.008", true);
            AddAttrValue(13, 2, "0.01", "0.015", true);
            AddAttrValue(13, 3, "0.018", "0.023", true);
            AddAttrValue(13, 4, "0.025", "0.032", true);
            AddAttrValue(13, 5, "0.035", "0.045", true);

            // 破甲率 (AttrType=14): 普通 0.5-1%, 优秀 1.5-2%, 精良 2.5-3%, 史诗 3.5-4.5%, 传说 5-6%
            AddAttrValue(14, 1, "0.005", "0.01", true);
            AddAttrValue(14, 2, "0.015", "0.02", true);
            AddAttrValue(14, 3, "0.025", "0.03", true);
            AddAttrValue(14, 4, "0.035", "0.045", true);
            AddAttrValue(14, 5, "0.05", "0.06", true);

            // 额外伤害 (AttrType=15): 普通 0.5-1%, 优秀 1.5-2%, 精良 2.5-3%, 史诗 3.5-4.5%, 传说 5-6%
            AddAttrValue(15, 1, "0.005", "0.01", true);
            AddAttrValue(15, 2, "0.015", "0.02", true);
            AddAttrValue(15, 3, "0.025", "0.03", true);
            AddAttrValue(15, 4, "0.035", "0.045", true);
            AddAttrValue(15, 5, "0.05", "0.06", true);

            return configs;
        }

        public async Task SeedElementRelationRulesAsync()
        {
            var now = DateTime.Now;
            var entries = BuildElementRelationRules(now);
            var existingEntries = await _dbContext.Db.Queryable<ElementRelationRuleEntity>()
                .ToListAsync();
            var existingMap = existingEntries.ToDictionary(
                item => $"{item.AttackerElement}:{item.DefenderElement}",
                StringComparer.OrdinalIgnoreCase);

            // 清理无属性相关的旧条目（已从克制链中移除）
            var noneEntries = existingEntries
                .Where(item => item.AttackerElement == 0 || item.DefenderElement == 0)
                .ToList();
            foreach (var obsolete in noneEntries)
            {
                await _dbContext.Db.Deleteable(obsolete).ExecuteCommandAsync();
            }

            foreach (var expected in entries)
            {
                var key = $"{expected.AttackerElement}:{expected.DefenderElement}";
                existingMap.TryGetValue(key, out var existing);
                if (existing == null)
                {
                    await _dbContext.Db.Insertable(expected).ExecuteCommandAsync();
                    continue;
                }

                if (!(existing.IsBuiltIn || string.Equals(existing.SeedKey, expected.SeedKey, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                existing.Modifier = expected.Modifier;
                existing.SortOrder = expected.SortOrder;
                existing.IsEnabled = expected.IsEnabled;
                existing.SeedKey = expected.SeedKey;
                existing.IsBuiltIn = true;
                existing.BuiltInVersion = expected.BuiltInVersion;
                existing.LastUpdateTime = now;
                await _dbContext.Db.Updateable(existing).ExecuteCommandAsync();
            }

            _logger?.LogInformation("Ensured built-in element relation rule config exists with {Count} rows", entries.Count);
        }

        /// <summary>
        /// 初始化新手礼包数据。
        /// </summary>
        public async Task SeedStarterPackagesAsync()
        {
            var builtInConfig = BuildDefaultStarterPackageConfig();
            var itemGrants = BuildDefaultStarterPackageItemGrants(builtInConfig.PackageId);
            var skillGrants = BuildDefaultStarterPackageSkillGrants(builtInConfig.PackageId);

            var existingConfig = await _dbContext.Db.Queryable<StarterPackageConfigEntity>()
                .FirstAsync(item => item.SeedKey == StarterPackageSeedKey || item.PackageId == builtInConfig.PackageId);
            var hasRegisterPackage = await _dbContext.Db.Queryable<StarterPackageConfigEntity>()
                .AnyAsync(item => item.IsEnabled && item.AutoGrantOnRegister);

            if (existingConfig == null)
            {
                builtInConfig.AutoGrantOnRegister = !hasRegisterPackage;
                await _dbContext.Db.Insertable(builtInConfig).ExecuteCommandAsync();
            }
            else
            {
                existingConfig.Name = builtInConfig.Name;
                existingConfig.Description = builtInConfig.Description;
                existingConfig.ConfigVersion = StarterPackageVersion;
                existingConfig.SeedKey = StarterPackageSeedKey;
                existingConfig.IsBuiltIn = true;
                existingConfig.BuiltInVersion = StarterPackageVersion;
                existingConfig.LastUpdateTime = DateTime.Now;
                await _dbContext.Db.Updateable(existingConfig).ExecuteCommandAsync();
                builtInConfig.PackageId = existingConfig.PackageId;
            }

            await _dbContext.Db.Deleteable<StarterPackageGrantItemEntity>()
                .Where(item => item.PackageId == builtInConfig.PackageId)
                .ExecuteCommandAsync();
            await _dbContext.Db.Deleteable<StarterPackageGrantSkillEntity>()
                .Where(item => item.PackageId == builtInConfig.PackageId)
                .ExecuteCommandAsync();

            if (itemGrants.Count > 0)
            {
                foreach (var item in itemGrants)
                {
                    item.PackageId = builtInConfig.PackageId;
                }

                await _dbContext.Db.Insertable(itemGrants).ExecuteCommandAsync();
            }

            if (skillGrants.Count > 0)
            {
                foreach (var skill in skillGrants)
                {
                    skill.PackageId = builtInConfig.PackageId;
                }

                await _dbContext.Db.Insertable(skillGrants).ExecuteCommandAsync();
            }

            _logger?.LogInformation("Ensured built-in starter package {PackageId} exists in database with {ItemCount} item grants and {SkillCount} skill grants", builtInConfig.PackageId, itemGrants.Count, skillGrants.Count);
        }

        /// <summary>
        /// 初始化灵宠数据
        /// </summary>
        public async Task SeedPetsAsync()
        {
            var count = await _dbContext.Db.Queryable<PetTemplateEntity>().CountAsync();
            if (count > 0)
            {
                return;
            }

            var snapshot = BalanceCatalog.GetSeedSnapshot();
            if (snapshot.PetTemplates.Count == 0)
            {
                return;
            }

            await _dbContext.Db.Insertable(snapshot.PetTemplates.ToList()).ExecuteCommandAsync();
            _logger?.LogInformation("Seeded {Count} pet templates into SQLite from balance catalog", snapshot.PetTemplates.Count);
        }

        /// <summary>
        /// 初始化丹药配方数据
        /// </summary>
        public async Task SeedAlchemyRecipesAsync()
        {
            await SeedPureAlchemyRecipesAsync();
        }

        private async Task SeedPureAlchemyRecipesAsync()
        {
            var now = DateTime.Now;
            var itemMap = (await _dbContext.Db.Queryable<ItemTemplateEntity>().ToListAsync())
                .ToDictionary(item => item.ItemId, item => item, StringComparer.OrdinalIgnoreCase);
            var expectedRecipes = BuildPureAlchemyRecipes(itemMap, now);
            var expectedIds = expectedRecipes.Select(recipe => recipe.RecipeId).ToList();

            await _dbContext.Db.Deleteable<AlchemyRecipeEntity>()
                .Where(recipe => recipe.IsBuiltIn && !expectedIds.Contains(recipe.RecipeId))
                .ExecuteCommandAsync();

            foreach (var expected in expectedRecipes)
            {
                var existing = await _dbContext.Db.Queryable<AlchemyRecipeEntity>()
                    .FirstAsync(recipe => recipe.RecipeId == expected.RecipeId);
                if (existing == null)
                {
                    await _dbContext.Db.Insertable(expected).ExecuteCommandAsync();
                    continue;
                }

                existing.PillTemplateId = expected.PillTemplateId;
                existing.Name = expected.Name;
                existing.Description = expected.Description;
                existing.RequiredFurnaceLevel = expected.RequiredFurnaceLevel;
                existing.BaseSuccessRate = expected.BaseSuccessRate;
                existing.BaseCraftTime = expected.BaseCraftTime;
                existing.MaterialsJson = expected.MaterialsJson;
                existing.UnlockCondition = null;
                existing.IsDefaultLearned = false;
                existing.SeedKey = expected.SeedKey;
                existing.IsBuiltIn = true;
                existing.BuiltInVersion = expected.BuiltInVersion;
                existing.LastUpdateTime = now;
                await _dbContext.Db.Updateable(existing).ExecuteCommandAsync();
            }
        }

        private static List<AlchemyRecipeEntity> BuildPureAlchemyRecipes(
            IReadOnlyDictionary<string, ItemTemplateEntity> itemMap,
            DateTime now)
        {
            var recipes = new List<AlchemyRecipeEntity>();
            var baseRecipes = new[]
            {
                ("001", "itm_cons_attack", "烈阳增元", "基础攻击丹药，提升短时间内的战斗输出。", 1, 93, 12),
                ("002", "itm_cons_defense", "玄甲护脉", "基础护体丹药，提升短时间内的防御能力。", 1, 93, 12),
                ("003", "itm_cons_speed", "逐风行气", "基础身法丹药，改善短时间内的出手节奏。", 1, 93, 12),
                ("004", "itm_pill_s01_break", "青岚山境破障", "青岚山境阶段的破障丹方。", 1, 93, 12)
            };

            foreach (var (suffix, pillId, name, description, level, successRate, craftTime) in baseRecipes)
            {
                recipes.Add(CreatePureAlchemyRecipe(
                    $"alchemy_baseline_{suffix}", pillId, name, description, level, successRate, craftTime,
                    "itm_crop_s01_herb", 2, "itm_core_s01_common", itemMap, now));
            }

            for (var stage = 2; stage <= 10; stage++)
            {
                var suffix = stage.ToString("D2");
                var herbCount = stage <= 2 ? 2 : stage <= 5 ? 3 : stage <= 8 ? 4 : 5;
                var recipe = CreatePureAlchemyRecipe(
                    $"alchemy_baseline_{stage + 3:D3}",
                    $"itm_pill_s{suffix}_break",
                    $"{GetAlchemyStageName(stage)}破障",
                    $"{GetAlchemyStageName(stage)}阶段的破障丹方。",
                    stage,
                    95 - stage * 2,
                    10 + stage * 2,
                    $"itm_crop_s{suffix}_herb",
                    herbCount,
                    $"itm_core_s{suffix}_common",
                    itemMap,
                    now);
                recipes.Add(recipe);
            }

            return recipes;
        }

        private static AlchemyRecipeEntity CreatePureAlchemyRecipe(
            string recipeId,
            string pillId,
            string name,
            string description,
            int level,
            int successRate,
            int craftTime,
            string cropId,
            int cropCount,
            string coreId,
            IReadOnlyDictionary<string, ItemTemplateEntity> itemMap,
            DateTime now)
        {
            var cropName = itemMap.TryGetValue(cropId, out var crop) ? crop.Name : cropId;
            var coreName = itemMap.TryGetValue(coreId, out var core) ? core.Name : coreId;
            return new AlchemyRecipeEntity
            {
                RecipeId = recipeId,
                PillTemplateId = pillId,
                Name = name,
                Description = description,
                RequiredFurnaceLevel = level,
                BaseSuccessRate = successRate,
                BaseCraftTime = craftTime,
                IsDefaultLearned = false,
                SeedKey = $"baseline:alchemy:{recipeId}",
                IsBuiltIn = true,
                BuiltInVersion = "pure-content-v1",
                LastUpdateTime = now,
                Materials =
                [
                    new() { ItemId = cropId, ItemName = cropName, Amount = cropCount },
                    new() { ItemId = coreId, ItemName = coreName, Amount = 1 }
                ]
            };
        }

        private static string GetAlchemyStageName(int stage)
        {
            return stage switch
            {
                2 => "赤砂荒原",
                3 => "沉星沼泽",
                4 => "炎脉天关",
                5 => "寒渊古泽",
                6 => "雷陨台原",
                7 => "万木天森",
                8 => "玄霜绝域",
                9 => "太虚星海",
                10 => "天门遗境",
                _ => $"第{stage}阶段"
            };
        }

        /// <summary>
        /// 补齐灵田作物模板。
        /// </summary>
        public async Task SeedSpiritFieldAsync()
        {
            var now = DateTime.Now;
            var templates = BuildSpiritFieldCropTemplates();
            if (templates.Count == 0)
            {
                return;
            }

            foreach (var template in templates)
            {
                var existing = await _dbContext.Db.Queryable<CropTemplateEntity>()
                    .FirstAsync(entity => entity.TemplateId == template.TemplateId);

                if (existing == null)
                {
                    await _dbContext.Db.Insertable(template).ExecuteCommandAsync();
                    continue;
                }

                var hasBuiltInIdentity = existing.IsBuiltIn ||
                    string.Equals(existing.SeedKey, template.SeedKey, StringComparison.OrdinalIgnoreCase);
                if (!hasBuiltInIdentity && !MatchesCropTemplate(existing, template))
                {
                    continue;
                }

                existing.Name = template.Name;
                existing.Description = template.Description;
                existing.Type = template.Type;
                existing.GrowthCycle = template.GrowthCycle;
                existing.Yield = template.Yield;
                existing.SeedId = template.SeedId;
                existing.SeedAmount = template.SeedAmount;
                existing.OutputItemId = template.OutputItemId;
                existing.OutputAmount = template.OutputAmount;
                existing.MinQuality = template.MinQuality;
                existing.MaxQuality = template.MaxQuality;
                existing.UnlockLevel = template.UnlockLevel;
                existing.SeedKey = template.SeedKey;
                existing.IsBuiltIn = true;
                existing.BuiltInVersion = template.BuiltInVersion;
                existing.LastUpdateTime = now;
                await _dbContext.Db.Updateable(existing).ExecuteCommandAsync();
            }

            _logger?.LogInformation("Ensured {Count} spirit field crop templates are aligned with current seed configuration", templates.Count);
        }

        private static bool MatchesCropTemplate(CropTemplateEntity actual, CropTemplateEntity expected)
        {
            return string.Equals(actual.Name, expected.Name, StringComparison.Ordinal) &&
                   string.Equals(actual.Description ?? string.Empty, expected.Description ?? string.Empty, StringComparison.Ordinal) &&
                   actual.Type == expected.Type &&
                   actual.GrowthCycle == expected.GrowthCycle &&
                   actual.Yield == expected.Yield &&
                   string.Equals(actual.SeedId, expected.SeedId, StringComparison.Ordinal) &&
                   actual.SeedAmount == expected.SeedAmount &&
                   string.Equals(actual.OutputItemId, expected.OutputItemId, StringComparison.Ordinal) &&
                   actual.OutputAmount == expected.OutputAmount &&
                   actual.MinQuality == expected.MinQuality &&
                   actual.MaxQuality == expected.MaxQuality &&
                   actual.UnlockLevel == expected.UnlockLevel;
        }

        /// <summary>
        /// 初始化锻造配方数据
        /// </summary>
        public async Task SeedForgeRecipesAsync()
        {
            await SeedPureForgeRecipesAsync();
        }

        private async Task SeedPureForgeRecipesAsync()
        {
            var equipmentTemplates = await _dbContext.Db.Queryable<EquipmentTemplateEntity>()
                .Where(template => PureForgeEquipmentIds.Contains(template.EquipmentId))
                .OrderBy(template => template.Level)
                .ToListAsync();
            equipmentTemplates = equipmentTemplates
                .OrderBy(template => template.Level)
                .ThenBy(template => template.EquipmentId)
                .ToList();
            var itemMap = (await _dbContext.Db.Queryable<ItemTemplateEntity>().ToListAsync())
                .ToDictionary(item => item.ItemId, item => item, StringComparer.OrdinalIgnoreCase);
            var expectedRecipes = equipmentTemplates
                .Select(templateEntity =>
                {
                    var template = TemplateDataMapper.ToRuntime(templateEntity);
                    var materials = BuildPureForgeMaterials(template, itemMap);
                    return new SeedForgeRecipeModel
                    {
                        RecipeId = $"forge_{template.EquipmentId}",
                        TemplateId = template.EquipmentId.ToString(),
                        Name = template.Name,
                        Description = template.Description,
                        SlotName = template.Slot.ToString(),
                        Quality = (int)template.Quality,
                        Level = template.Level,
                        Icon = $"equipment-{template.Slot.ToString().ToLowerInvariant()}",
                        CostGold = GetPureForgeGoldCost(template.Level),
                        SuccessRate = GetPureForgeSuccessRate(template.Level),
                        Materials = materials
                    };
                })
                .Where(recipe => recipe.Materials.Count > 0)
                .ToList();
            var expectedIds = expectedRecipes.Select(recipe => recipe.RecipeId).ToList();

            await _dbContext.Db.Deleteable<ForgeRecipeEntity>()
                .Where(recipe => recipe.IsBuiltIn && !expectedIds.Contains(recipe.RecipeId))
                .ExecuteCommandAsync();

            foreach (var expected in expectedRecipes.Select(MapToForgeRecipeEntity))
            {
                var existing = await _dbContext.Db.Queryable<ForgeRecipeEntity>()
                    .FirstAsync(recipe => recipe.RecipeId == expected.RecipeId);
                if (existing == null)
                {
                    await _dbContext.Db.Insertable(expected).ExecuteCommandAsync();
                    continue;
                }

                existing.TemplateId = expected.TemplateId;
                existing.Name = expected.Name;
                existing.Description = expected.Description;
                existing.SlotName = expected.SlotName;
                existing.Quality = expected.Quality;
                existing.Level = expected.Level;
                existing.Icon = expected.Icon;
                existing.CostGold = expected.CostGold;
                existing.SuccessRate = expected.SuccessRate;
                existing.MaterialsJson = expected.MaterialsJson;
                existing.IsEnabled = true;
                existing.SeedKey = expected.SeedKey;
                existing.IsBuiltIn = true;
                existing.BuiltInVersion = expected.BuiltInVersion;
                existing.LastUpdateTime = DateTime.Now;
                await _dbContext.Db.Updateable(existing).ExecuteCommandAsync();
            }
        }

        private static List<SeedForgeMaterialModel> BuildPureForgeMaterials(
            EquipmentTemplate template,
            IReadOnlyDictionary<string, ItemTemplateEntity> itemMap)
        {
            var materials = new List<SeedForgeMaterialModel>();
            var stage = Math.Clamp(template.Level / 10 + 1, 1, 10);
            var suffix = stage.ToString("D2");
            var primaryCount = stage <= 2 ? 2 : stage <= 5 ? 3 : 5;
            var supportCount = stage >= 6 ? 3 : 1;

            switch (template.Slot)
            {
                case EquipmentSlot.Weapon:
                    TryAddForgeMaterial(materials, itemMap, $"itm_mat_ore_s{suffix}", primaryCount);
                    TryAddForgeMaterial(materials, itemMap, $"itm_mat_wood_s{suffix}", supportCount);
                    break;
                case EquipmentSlot.Armor:
                    TryAddForgeMaterial(materials, itemMap, $"itm_mat_ore_s{suffix}", primaryCount);
                    TryAddForgeMaterial(materials, itemMap, $"itm_mat_hide_s{suffix}", supportCount);
                    break;
            }

            TryAddForgeMaterial(materials, itemMap, $"itm_core_s{suffix}_common", 1);
            TryAddForgeMaterial(materials, itemMap, $"itm_enhance_s{suffix}", 1);
            return materials;
        }

        private static long GetPureForgeGoldCost(int level)
        {
            return level switch
            {
                1 => 180,
                10 => 500,
                20 => 850,
                30 => 1150,
                40 => 1500,
                50 => 1800,
                60 => 2150,
                70 => 2450,
                80 => 2750,
                90 => 3050,
                _ => 180 + Math.Max(0, level - 1) * 32L
            };
        }

        private static int GetPureForgeSuccessRate(int level)
        {
            return level switch
            {
                1 => 93,
                10 => 86,
                20 => 79,
                30 => 73,
                40 => 66,
                _ => 60
            };
        }

        /// <summary>
        /// 初始化配方卷轴道具及其配方绑定关系。
        /// </summary>
        private async Task SeedRecipeUnlockDataAsync()
        {
            var now = DateTime.Now;
            var alchemyRecipes = await _dbContext.Db.Queryable<AlchemyRecipeEntity>()
                .OrderBy(recipe => recipe.RecipeId)
                .ToListAsync();
            var forgeRecipes = await _dbContext.Db.Queryable<ForgeRecipeEntity>()
                .Where(recipe => recipe.IsEnabled)
                .OrderBy(recipe => recipe.Level)
                .ToListAsync();
            forgeRecipes = forgeRecipes
                .OrderBy(recipe => recipe.Level)
                .ThenBy(recipe => recipe.RecipeId, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var expected = new List<(string ItemId, string RecipeType, string RecipeId, string Name, string Description, int UseLevel, int Quality)>();
            expected.AddRange(alchemyRecipes.Select(recipe =>
                ($"recipe_scroll_alchemy_{recipe.RecipeId}", "Alchemy", recipe.RecipeId, $"{recipe.Name}丹方",
                    $"配方卷轴。使用后永久解锁炼丹配方《{recipe.Name}》。", Math.Max(1, recipe.RequiredFurnaceLevel), 1)));
            expected.AddRange(forgeRecipes.Select(recipe =>
                ($"recipe_scroll_forge_{recipe.RecipeId}", "Forge", recipe.RecipeId, $"{recipe.Name}图纸",
                    $"锻造图纸。使用后永久解锁锻造图纸《{recipe.Name}》。", Math.Max(1, recipe.Level), Math.Max(1, recipe.Quality))));

            var expectedIds = expected.Select(item => item.ItemId).ToList();
            await _dbContext.Db.Deleteable<ItemRecipeUnlockConfigEntity>()
                .Where(config => config.ItemId.StartsWith("recipe_scroll_") && !expectedIds.Contains(config.ItemId))
                .ExecuteCommandAsync();
            await _dbContext.Db.Deleteable<ItemTemplateEntity>()
                .Where(item => item.ItemId.StartsWith("recipe_scroll_") && !expectedIds.Contains(item.ItemId))
                .ExecuteCommandAsync();

            foreach (var item in expected)
            {
                var existingItem = await _dbContext.Db.Queryable<ItemTemplateEntity>()
                    .FirstAsync(entity => entity.ItemId == item.ItemId);
                if (existingItem == null)
                {
                    await _dbContext.Db.Insertable(new ItemTemplateEntity
                    {
                        ItemId = item.ItemId,
                        Name = item.Name,
                        UseLevel = item.UseLevel,
                        Type = (int)ItemType.RecipeScroll,
                        Description = item.Description,
                        MaxStack = 99,
                        Quality = item.Quality,
                        IsTradeable = true,
                        IsBuiltIn = true,
                        BuiltInVersion = "pure-content-v1",
                        SeedKey = $"baseline:recipe-scroll:{item.RecipeType.ToLowerInvariant()}:{item.RecipeId}",
                        LastUpdateTime = now
                    }).ExecuteCommandAsync();
                }
                else
                {
                    existingItem.Name = item.Name;
                    existingItem.UseLevel = item.UseLevel;
                    existingItem.Type = (int)ItemType.RecipeScroll;
                    existingItem.Description = item.Description;
                    existingItem.MaxStack = 99;
                    existingItem.Quality = item.Quality;
                    existingItem.IsBuiltIn = true;
                    existingItem.BuiltInVersion = "pure-content-v1";
                    existingItem.SeedKey = $"baseline:recipe-scroll:{item.RecipeType.ToLowerInvariant()}:{item.RecipeId}";
                    existingItem.LastUpdateTime = now;
                    await _dbContext.Db.Updateable(existingItem).ExecuteCommandAsync();
                }

                var unlock = await _dbContext.Db.Queryable<ItemRecipeUnlockConfigEntity>()
                    .FirstAsync(config => config.ItemId == item.ItemId);
                if (unlock == null)
                {
                    await _dbContext.Db.Insertable(new ItemRecipeUnlockConfigEntity
                    {
                        ItemId = item.ItemId,
                        RecipeType = item.RecipeType,
                        RecipeId = item.RecipeId,
                        IsEnabled = true,
                        LastUpdateTime = now
                    }).ExecuteCommandAsync();
                }
                else
                {
                    unlock.RecipeType = item.RecipeType;
                    unlock.RecipeId = item.RecipeId;
                    unlock.IsEnabled = true;
                    unlock.LastUpdateTime = now;
                    await _dbContext.Db.Updateable(unlock).ExecuteCommandAsync();
                }
            }

            var legacySkillBookIds = new[] { "itm_skillbook_basic" }
                .Concat(Enumerable.Range(1, 10).Select(index => $"itm_skillbook_s{index:D2}"))
                .ToList();
            await _dbContext.Db.Deleteable<InventoryItemEntity>().Where(item => legacySkillBookIds.Contains(item.ItemId)).ExecuteCommandAsync();
            await _dbContext.Db.Deleteable<ShopItemEntity>().Where(item => item.ItemType == (int)ItemType.Material && legacySkillBookIds.Contains(item.ItemId)).ExecuteCommandAsync();
            await _dbContext.Db.Deleteable<ItemSkillBookConfigEntity>().Where(item => legacySkillBookIds.Contains(item.ItemId)).ExecuteCommandAsync();
            await _dbContext.Db.Deleteable<ItemTemplateEntity>().Where(item => legacySkillBookIds.Contains(item.ItemId)).ExecuteCommandAsync();
        }

        /// <summary>
        /// 初始化副本数据
        /// </summary>
        public async Task SeedDungeonsAsync()
        {
            var existingDungeonCount = await _dbContext.Db.Queryable<DungeonTemplateEntity>().CountAsync();
            if (existingDungeonCount > 0)
            {
                return;
            }

            var snapshot = BalanceCatalog.GetSeedSnapshot();
            if (snapshot.DungeonTemplates.Count == 0)
            {
                return;
            }

            await _dbContext.Db.Insertable(snapshot.DungeonTemplates.ToList()).ExecuteCommandAsync();
            _logger?.LogInformation("Seeded {Count} dungeon templates into SQLite from balance catalog", snapshot.DungeonTemplates.Count);
        }

        /// <summary>
        /// 初始化秘境实例模板和事件配置种子数据。
        /// </summary>
        public async Task SeedDungeonInstancesAsync()
        {
            var now = DateTime.Now;
            var db = _dbContext.Db;

            // 秘境实例模板
            var expectedTemplates = DungeonInstanceSeedData.BuildTemplates();
            var existingTemplates = await db.Queryable<DungeonInstanceTemplateEntity>().ToListAsync();
            var existingTemplateMap = existingTemplates.ToDictionary(t => t.Id, StringComparer.OrdinalIgnoreCase);

            foreach (var expected in expectedTemplates)
            {
                if (!existingTemplateMap.TryGetValue(expected.Id, out var existing))
                {
                    await db.Insertable(expected).ExecuteCommandAsync();
                    continue;
                }

                if (existing.IsBuiltIn || string.Equals(existing.SeedKey, expected.SeedKey, StringComparison.OrdinalIgnoreCase))
                {
                    existing.Name = expected.Name;
                    existing.Description = expected.Description;
                    existing.Enabled = expected.Enabled;
                    existing.RecommendedLevel = expected.RecommendedLevel;
                    existing.DailyEnterLimit = expected.DailyEnterLimit;
                    existing.TickIntervalSeconds = expected.TickIntervalSeconds;
                    existing.OpenScheduleJson = expected.OpenScheduleJson;
                    existing.EntryCostsJson = expected.EntryCostsJson;
                    existing.EventGroupId = expected.EventGroupId;
                    existing.AutoMedicineConfigJson = expected.AutoMedicineConfigJson;
                    existing.EncounterConfigJson = expected.EncounterConfigJson;
                    existing.SeedKey = expected.SeedKey;
                    existing.IsBuiltIn = true;
                    existing.BuiltInVersion = expected.BuiltInVersion;
                    existing.LastUpdateTime = now;
                    await db.Updateable(existing).ExecuteCommandAsync();
                }
            }

            // 秘境事件配置
            var expectedEvents = DungeonInstanceSeedData.BuildEventConfigs();
            var existingEvents = await db.Queryable<DungeonEventConfigEntity>().ToListAsync();
            var existingEventMap = existingEvents.ToDictionary(e => e.Id, StringComparer.OrdinalIgnoreCase);

            foreach (var expected in expectedEvents)
            {
                if (!existingEventMap.TryGetValue(expected.Id, out var existing))
                {
                    await db.Insertable(expected).ExecuteCommandAsync();
                    continue;
                }

                if (existing.IsBuiltIn || string.Equals(existing.SeedKey, expected.SeedKey, StringComparison.OrdinalIgnoreCase))
                {
                    existing.Name = expected.Name;
                    existing.Description = expected.Description;
                    existing.DungeonId = expected.DungeonId;
                    existing.EventType = expected.EventType;
                    existing.Weight = expected.Weight;
                    existing.Enabled = expected.Enabled;
                    existing.EventDataJson = expected.EventDataJson;
                    existing.DeathKeep = expected.DeathKeep;
                    existing.SeedKey = expected.SeedKey;
                    existing.IsBuiltIn = true;
                    existing.BuiltInVersion = expected.BuiltInVersion;
                    existing.LastUpdateTime = now;
                    await db.Updateable(existing).ExecuteCommandAsync();
                }
            }

            // 秘境事件组
            var expectedGroups = DungeonInstanceSeedData.BuildEventGroups();
            var existingGroups = await db.Queryable<DungeonEventGroupEntity>().ToListAsync();
            var existingGroupMap = existingGroups.ToDictionary(g => g.Id, StringComparer.OrdinalIgnoreCase);

            foreach (var expected in expectedGroups)
            {
                if (!existingGroupMap.TryGetValue(expected.Id, out var existing))
                {
                    await db.Insertable(expected).ExecuteCommandAsync();
                    continue;
                }

                if (existing.IsBuiltIn || string.Equals(existing.SeedKey, expected.SeedKey, StringComparison.OrdinalIgnoreCase))
                {
                    existing.Name = expected.Name;
                    existing.Description = expected.Description;
                    existing.GroupItemsJson = expected.GroupItemsJson;
                    existing.SeedKey = expected.SeedKey;
                    existing.IsBuiltIn = true;
                    existing.BuiltInVersion = expected.BuiltInVersion;
                    existing.LastUpdateTime = now;
                    await db.Updateable(existing).ExecuteCommandAsync();
                }
            }

            _logger?.LogInformation(
                "Ensured built-in dungeon instance templates ({TemplateCount}), event configs ({EventCount}), and event groups ({GroupCount}) exist",
                expectedTemplates.Count,
                expectedEvents.Count,
                expectedGroups.Count);
        }


        /// <summary>
        /// 初始化图鉴和抽奖系统种子数据。
        /// </summary>
        public async Task SeedCollectionsAndLotteryAsync()
        {
            var now = DateTime.Now;
            var db = _dbContext.Db;

            db.Ado.BeginTran();
            try
            {
                // 文字图鉴系列
                var textSeries = CollectionLotterySeedData.BuildTextCollectionSeries(now);
                var existingTextSeries = await db.Queryable<TextCollectionSeriesEntity>().ToListAsync();
                var existingTextSeriesMap = existingTextSeries.ToDictionary(s => s.SeriesId, StringComparer.OrdinalIgnoreCase);
                foreach (var expected in textSeries)
                {
                    if (!existingTextSeriesMap.TryGetValue(expected.SeriesId, out var existing))
                    {
                        await db.Insertable(expected).ExecuteCommandAsync();
                        continue;
                    }
                    if (existing.IsBuiltIn || string.Equals(existing.SeedKey, expected.SeedKey, StringComparison.OrdinalIgnoreCase))
                    {
                        existing.Name = expected.Name;
                        existing.Description = expected.Description;
                        existing.Icon = expected.Icon;
                        existing.SortOrder = expected.SortOrder;
                        existing.IsEnabled = expected.IsEnabled;
                        existing.SeedKey = expected.SeedKey;
                        existing.IsBuiltIn = true;
                        existing.BuiltInVersion = expected.BuiltInVersion;
                        existing.LastUpdateTime = now;
                        await db.Updateable(existing).ExecuteCommandAsync();
                    }
                }

                // 文字图鉴条目
                var textItems = CollectionLotterySeedData.BuildTextCollectionItems(now);
                var existingTextItems = await db.Queryable<TextCollectionItemEntity>().ToListAsync();
                var existingTextItemsMap = existingTextItems.ToDictionary(i => i.ItemId, StringComparer.OrdinalIgnoreCase);
                foreach (var expected in textItems)
                {
                    if (!existingTextItemsMap.TryGetValue(expected.ItemId, out var existing))
                    {
                        await db.Insertable(expected).ExecuteCommandAsync();
                        continue;
                    }
                    if (existing.IsBuiltIn || string.Equals(existing.SeedKey, expected.SeedKey, StringComparison.OrdinalIgnoreCase))
                    {
                        existing.SeriesId = expected.SeriesId;
                        existing.Character = expected.Character;
                        existing.SlotIndex = expected.SlotIndex;
                        existing.SortOrder = expected.SortOrder;
                        existing.IsEnabled = expected.IsEnabled;
                        existing.SeedKey = expected.SeedKey;
                        existing.IsBuiltIn = true;
                        existing.BuiltInVersion = expected.BuiltInVersion;
                        existing.LastUpdateTime = now;
                        await db.Updateable(existing).ExecuteCommandAsync();
                    }
                }

                // 文字图鉴奖励
                var textBonuses = CollectionLotterySeedData.BuildTextCollectionBonuses(now);
                var existingTextBonuses = await db.Queryable<TextCollectionBonusEntity>().ToListAsync();
                var existingTextBonusesMap = existingTextBonuses.ToDictionary(b => b.BonusId, StringComparer.OrdinalIgnoreCase);
                foreach (var expected in textBonuses)
                {
                    if (!existingTextBonusesMap.TryGetValue(expected.BonusId, out var existing))
                    {
                        await db.Insertable(expected).ExecuteCommandAsync();
                        continue;
                    }
                    if (existing.IsBuiltIn || string.Equals(existing.SeedKey, expected.SeedKey, StringComparison.OrdinalIgnoreCase))
                    {
                        existing.SeriesId = expected.SeriesId;
                        existing.AttrType = expected.AttrType;
                        existing.AttrValue = expected.AttrValue;
                        existing.ValueType = expected.ValueType;
                        existing.SortOrder = expected.SortOrder;
                        existing.SeedKey = expected.SeedKey;
                        existing.IsBuiltIn = true;
                        existing.BuiltInVersion = expected.BuiltInVersion;
                        existing.LastUpdateTime = now;
                        await db.Updateable(existing).ExecuteCommandAsync();
                    }
                }

                // 图片图鉴系列
                var imgSeries = CollectionLotterySeedData.BuildImageCollectionSeries(now);
                var existingImgSeries = await db.Queryable<ImageCollectionSeriesEntity>().ToListAsync();
                var existingImgSeriesMap = existingImgSeries.ToDictionary(s => s.SeriesId, StringComparer.OrdinalIgnoreCase);
                foreach (var expected in imgSeries)
                {
                    if (!existingImgSeriesMap.TryGetValue(expected.SeriesId, out var existing))
                    {
                        await db.Insertable(expected).ExecuteCommandAsync();
                        continue;
                    }
                    if (existing.IsBuiltIn || string.Equals(existing.SeedKey, expected.SeedKey, StringComparison.OrdinalIgnoreCase))
                    {
                        existing.Name = expected.Name;
                        existing.Description = expected.Description;
                        existing.Icon = expected.Icon;
                        existing.SortOrder = expected.SortOrder;
                        existing.IsEnabled = expected.IsEnabled;
                        existing.SeedKey = expected.SeedKey;
                        existing.IsBuiltIn = true;
                        existing.BuiltInVersion = expected.BuiltInVersion;
                        existing.LastUpdateTime = now;
                        await db.Updateable(existing).ExecuteCommandAsync();
                    }
                }

                // 图片图鉴条目
                var imgItems = CollectionLotterySeedData.BuildImageCollectionItems(now);
                var existingImgItems = await db.Queryable<ImageCollectionItemEntity>().ToListAsync();
                var existingImgItemsMap = existingImgItems.ToDictionary(i => i.ItemId, StringComparer.OrdinalIgnoreCase);
                foreach (var expected in imgItems)
                {
                    if (!existingImgItemsMap.TryGetValue(expected.ItemId, out var existing))
                    {
                        await db.Insertable(expected).ExecuteCommandAsync();
                        continue;
                    }
                    if (existing.IsBuiltIn || string.Equals(existing.SeedKey, expected.SeedKey, StringComparison.OrdinalIgnoreCase))
                    {
                        existing.SeriesId = expected.SeriesId;
                        existing.ImageName = expected.ImageName;
                        existing.ThumbUrl = expected.ThumbUrl;
                        existing.OriginalUrl = expected.OriginalUrl;
                        existing.SortOrder = expected.SortOrder;
                        existing.IsEnabled = expected.IsEnabled;
                        existing.SeedKey = expected.SeedKey;
                        existing.IsBuiltIn = true;
                        existing.BuiltInVersion = expected.BuiltInVersion;
                        existing.LastUpdateTime = now;
                        await db.Updateable(existing).ExecuteCommandAsync();
                    }
                }

                // 图片图鉴奖励
                var imgBonuses = CollectionLotterySeedData.BuildImageCollectionBonuses(now);
                var existingImgBonuses = await db.Queryable<ImageCollectionBonusEntity>().ToListAsync();
                var existingImgBonusesMap = existingImgBonuses.ToDictionary(b => b.BonusId, StringComparer.OrdinalIgnoreCase);
                foreach (var expected in imgBonuses)
                {
                    if (!existingImgBonusesMap.TryGetValue(expected.BonusId, out var existing))
                    {
                        await db.Insertable(expected).ExecuteCommandAsync();
                        continue;
                    }
                    if (existing.IsBuiltIn || string.Equals(existing.SeedKey, expected.SeedKey, StringComparison.OrdinalIgnoreCase))
                    {
                        existing.SeriesId = expected.SeriesId;
                        existing.AttrType = expected.AttrType;
                        existing.AttrValue = expected.AttrValue;
                        existing.ValueType = expected.ValueType;
                        existing.SortOrder = expected.SortOrder;
                        existing.SeedKey = expected.SeedKey;
                        existing.IsBuiltIn = true;
                        existing.BuiltInVersion = expected.BuiltInVersion;
                        existing.LastUpdateTime = now;
                        await db.Updateable(existing).ExecuteCommandAsync();
                    }
                }

                // 抽奖池
                var pools = CollectionLotterySeedData.BuildLotteryPools(now);
                var existingPools = await db.Queryable<LotteryPoolEntity>().ToListAsync();
                var existingPoolsMap = existingPools.ToDictionary(p => p.PoolId, StringComparer.OrdinalIgnoreCase);
                foreach (var expected in pools)
                {
                    if (!existingPoolsMap.TryGetValue(expected.PoolId, out var existing))
                    {
                        await db.Insertable(expected).ExecuteCommandAsync();
                        continue;
                    }
                    if (existing.IsBuiltIn || string.Equals(existing.SeedKey, expected.SeedKey, StringComparison.OrdinalIgnoreCase))
                    {
                        existing.Name = expected.Name;
                        existing.LotteryType = expected.LotteryType;
                        existing.CostType = expected.CostType;
                        existing.CostAmount = expected.CostAmount;
                        existing.IsEnabled = expected.IsEnabled;
                        existing.SupportSingle = expected.SupportSingle;
                        existing.SupportTen = expected.SupportTen;
                        existing.DailyLimit = expected.DailyLimit;
                        existing.TotalLimit = expected.TotalLimit;
                        existing.SortOrder = expected.SortOrder;
                        existing.SeedKey = expected.SeedKey;
                        existing.IsBuiltIn = true;
                        existing.BuiltInVersion = expected.BuiltInVersion;
                        existing.LastUpdateTime = now;
                        await db.Updateable(existing).ExecuteCommandAsync();
                    }
                }

                // 抽奖奖项
                var prizes = CollectionLotterySeedData.BuildLotteryPrizes(now);
                var existingPrizes = await db.Queryable<LotteryPrizeEntity>().ToListAsync();
                var existingPrizesMap = existingPrizes.ToDictionary(p => p.PrizeId, StringComparer.OrdinalIgnoreCase);
                foreach (var expected in prizes)
                {
                    if (!existingPrizesMap.TryGetValue(expected.PrizeId, out var existing))
                    {
                        await db.Insertable(expected).ExecuteCommandAsync();
                        continue;
                    }
                    if (existing.IsBuiltIn || string.Equals(existing.SeedKey, expected.SeedKey, StringComparison.OrdinalIgnoreCase))
                    {
                        existing.PoolId = expected.PoolId;
                        existing.RewardType = expected.RewardType;
                        existing.RewardTargetId = expected.RewardTargetId;
                        existing.RewardAmount = expected.RewardAmount;
                        existing.Probability = expected.Probability;
                        existing.SortOrder = expected.SortOrder;
                        existing.IsEnabled = expected.IsEnabled;
                        existing.SeedKey = expected.SeedKey;
                        existing.IsBuiltIn = true;
                        existing.BuiltInVersion = expected.BuiltInVersion;
                        existing.LastUpdateTime = now;
                        await db.Updateable(existing).ExecuteCommandAsync();
                    }
                }

                db.Ado.CommitTran();
                _logger?.LogInformation(
                    "Ensured built-in collection and lottery seed data: {TextSeries} text series, {TextItems} text items, {ImgSeries} image series, {ImgItems} image items, {Pools} pools, {Prizes} prizes",
                    textSeries.Count, textItems.Count, imgSeries.Count, imgItems.Count, pools.Count, prizes.Count);
            }
            catch
            {
                db.Ado.RollbackTran();
                throw;
            }
        }

        /// <summary>
        /// 初始化默认管理员账号。
        /// 仅在管理员表为空时创建第一位管理员，避免覆盖后台后续维护的数据。
        /// </summary>
        public async Task SeedAdminUsersAsync()
        {
            if (!_adminSeedOptions.Enabled)
            {
                return;
            }

            // 中文注释：
            // 默认管理员只应服务于本地开发和首次调试，不应在正式环境自动生成固定口令账号。
            if (!_hostEnvironment.IsDevelopment())
            {
                _logger?.LogInformation("Skipped default admin seed because current environment {EnvironmentName} is not Development.", _hostEnvironment.EnvironmentName);
                return;
            }

            var existingAdminCount = await _dbContext.Db.Queryable<AdminUserEntity>().CountAsync();
            if (existingAdminCount > 0)
            {
                return;
            }

            var normalizedAccount = (_adminSeedOptions.DefaultAccount ?? string.Empty).Trim();
            var displayName = string.IsNullOrWhiteSpace(_adminSeedOptions.DefaultDisplayName)
                ? "系统管理员"
                : _adminSeedOptions.DefaultDisplayName.Trim();
            var password = _adminSeedOptions.DefaultPassword ?? string.Empty;
            var role = string.IsNullOrWhiteSpace(_adminSeedOptions.DefaultRole)
                ? "super_admin"
                : _adminSeedOptions.DefaultRole.Trim();

            if (string.IsNullOrWhiteSpace(normalizedAccount) || string.IsNullOrWhiteSpace(password))
            {
                _logger?.LogWarning("Skipped default admin seed because account or password is empty.");
                return;
            }

            var adminUser = new AdminUserEntity
            {
                AdminId = Guid.NewGuid().ToString("N"),
                Account = normalizedAccount,
                DisplayName = displayName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12)),
                Role = role,
                IsActive = true,
                IsDeleted = false,
                CreateTime = DateTime.Now,
                LastUpdateTime = DateTime.Now
            };

            await _dbContext.Db.Insertable(adminUser).ExecuteCommandAsync();
            _logger?.LogInformation("Seeded default admin account {Account} with role {Role}", normalizedAccount, role);
        }

        private static StarterPackageConfigEntity BuildDefaultStarterPackageConfig()
        {
            return new StarterPackageConfigEntity
            {
                PackageId = "starter_package_default",
                Name = "默认新手礼包",
                Description = "注册后自动发放的基础起步礼包，用于保证战斗、灵田、灵宠与炼丹链路可闭环验证。",
                IsEnabled = true,
                AutoGrantOnRegister = true,
                SortOrder = 1,
                ConfigVersion = StarterPackageVersion,
                SeedKey = StarterPackageSeedKey,
                IsBuiltIn = true,
                BuiltInVersion = StarterPackageVersion,
                Remark = "系统内置默认新手礼包",
                CreatedBy = "system",
                UpdatedBy = "system",
                CreateTime = DateTime.Now,
                LastUpdateTime = DateTime.Now
            };
        }

        private static List<StarterPackageGrantItemEntity> BuildDefaultStarterPackageItemGrants(string packageId)
        {
            return
            [
                CreateStarterPackageItem(packageId, "itm_cons_hp_small", 5, true, 1),
                CreateStarterPackageItem(packageId, "itm_cons_mp_small", 3, true, 2),
                CreateStarterPackageItem(packageId, "itm_mat_ore_s01", 5, true, 3),
                CreateStarterPackageItem(packageId, "itm_mat_wood_s01", 5, true, 4),
                CreateStarterPackageItem(packageId, "itm_mat_hide_s01", 5, true, 5),
                CreateStarterPackageItem(packageId, "itm_mat_crystal_s01", 5, true, 6),
                CreateStarterPackageItem(packageId, "itm_enhance_s01", 5, true, 7),
                CreateStarterPackageItem(packageId, "seed_s01_herb", 3, true, 8),
                CreateStarterPackageItem(packageId, "seed_s01_flower", 3, true, 9),
                CreateStarterPackageItem(packageId, "recipe_scroll_alchemy_alchemy_baseline_001", 1, true, 10),
                CreateStarterPackageItem(packageId, "recipe_scroll_forge_forge_10001", 1, true, 11)
            ];
        }

        private static List<StarterPackageGrantSkillEntity> BuildDefaultStarterPackageSkillGrants(string packageId)
        {
            return
            [
                new StarterPackageGrantSkillEntity
                {
                    GID = Guid.NewGuid().ToString("N"),
                    PackageId = packageId,
                    SkillId = 1001,
                    SortOrder = 1
                }
            ];
        }

        private static StarterPackageGrantItemEntity CreateStarterPackageItem(string packageId, string itemId, int quantity, bool isLocked, int sortOrder)
        {
            return new StarterPackageGrantItemEntity
            {
                GID = Guid.NewGuid().ToString("N"),
                PackageId = packageId,
                ItemId = itemId,
                Quantity = quantity,
                IsBound = isLocked,
                SortOrder = sortOrder
            };
        }

        private static string GetTieredItemId(string prefix, int nextLevel)
        {
            if (nextLevel >= 25)
            {
                return $"{prefix}_t3";
            }

            if (nextLevel >= 15)
            {
                return $"{prefix}_t2";
            }

            return $"{prefix}_t1";
        }

        private static SpiritFieldSpeedUpItemConfigEntity CreateSpiritFieldSpeedUpItem(string itemId, int speedUpSeconds, int sortOrder)
        {
            return new SpiritFieldSpeedUpItemConfigEntity
            {
                ItemId = itemId,
                SpeedUpSeconds = speedUpSeconds,
                SortOrder = sortOrder,
                IsEnabled = true,
                SeedKey = $"built-in:spirit-field:speedup:{itemId}",
                IsBuiltIn = true,
                BuiltInVersion = SpiritFieldRuleVersion,
                LastUpdateTime = DateTime.Now
            };
        }

        private static List<ElementRelationRuleEntity> BuildElementRelationRules(DateTime now)
        {
            // 对称循环克制矩阵：金→木→土→水→火→雷→冰→风→(回金)
            // 距离1: ±0.15(强克)  距离2: ±0.10(次克)  距离3: ±0.05(三级)  距离4: 0(无关系)
            var matrix = new Dictionary<Element, double[]>
            {
                [Element.Metal]  = [0.00,  0.15,  0.10,  0.05,  0.00, -0.05, -0.10, -0.15],
                [Element.Wood]   = [-0.15, 0.00,  0.15,  0.10,  0.05,  0.00, -0.05, -0.10],
                [Element.Earth]  = [-0.10, -0.15, 0.00,  0.15,  0.10,  0.05,  0.00, -0.05],
                [Element.Water]  = [-0.05, -0.10, -0.15, 0.00,  0.15,  0.10,  0.05,  0.00],
                [Element.Fire]   = [0.00, -0.05, -0.10, -0.15, 0.00,  0.15,  0.10,  0.05],
                [Element.Thunder]= [0.05,  0.00, -0.05, -0.10, -0.15, 0.00,  0.15,  0.10],
                [Element.Ice]    = [0.10,  0.05,  0.00, -0.05, -0.10, -0.15, 0.00,  0.15],
                [Element.Wind]   = [0.15,  0.10,  0.05,  0.00, -0.05, -0.10, -0.15, 0.00]
            };

            var ordered = new[]
            {
                Element.Metal,
                Element.Wood,
                Element.Earth,
                Element.Water,
                Element.Fire,
                Element.Thunder,
                Element.Ice,
                Element.Wind
            };

            var entries = new List<ElementRelationRuleEntity>();
            var sortOrder = 1;

            foreach (var attacker in ordered)
            {
                var row = matrix[attacker];
                for (var index = 0; index < ordered.Length; index++)
                {
                    entries.Add(CreateElementRelationRule(attacker, ordered[index], row[index], sortOrder++, now));
                }
            }

            return entries;
        }

        private static ElementRelationRuleEntity CreateElementRelationRule(Element attacker, Element defender, double modifier, int sortOrder, DateTime now)
        {
            return new ElementRelationRuleEntity
            {
                GID = $"{(int)attacker}_{(int)defender}",
                AttackerElement = (int)attacker,
                DefenderElement = (int)defender,
                Modifier = modifier,
                SortOrder = sortOrder,
                IsEnabled = true,
                SeedKey = $"built-in:element-relation:{(int)attacker}:{(int)defender}",
                IsBuiltIn = true,
                BuiltInVersion = ElementRelationRuleVersion,
                LastUpdateTime = now
            };
        }

        private static List<SeedForgeRecipeModel> BuildForgeRecipes(
            IReadOnlyList<EquipmentTemplateEntity> equipmentTemplates,
            IReadOnlyDictionary<string, ItemTemplateEntity> itemMap)
        {
            return equipmentTemplates
                .Where(template => template.Level <= 5)
                .OrderBy(template => template.Level)
                .ThenBy(template => template.Slot)
                .ThenBy(template => template.EquipmentId)
                .Take(16)
                .Select(templateEntity =>
                {
                    var template = TemplateDataMapper.ToRuntime(templateEntity);
                    var materialCount = GetMaterialCount(template);
                    return new SeedForgeRecipeModel
                    {
                        RecipeId = $"forge_{template.EquipmentId}",
                        TemplateId = template.EquipmentId.ToString(),
                        Name = template.Name,
                        Description = template.Description,
                        SlotName = template.Slot.ToString(),
                        Quality = (int)template.Quality,
                        Level = template.Level,
                        Icon = $"equipment-{template.Slot.ToString().ToLowerInvariant()}",
                        CostGold = 200 + template.Level * 120L + (int)template.Quality * 80L,
                        SuccessRate = GetBalancedForgeSuccessRate(template),
                        Materials =
                        [
                            new()
                            {
                                ItemId = "item_003",
                                Name = itemMap.TryGetValue("item_003", out var item) ? item.Name : "狼牙",
                                Icon = "💎",
                                Count = materialCount
                            }
                        ]
                    };
                })
                .ToList();
        }

        private static List<CropTemplateEntity> BuildSpiritFieldCropTemplates()
        {
            var now = DateTime.Now;
            return
            [
                new()
                {
                    TemplateId = "crop_blood_grass",
                    Name = "凝血草",
                    Description = "炼制基础回血丹药的常见灵草。",
                    Type = CropType.Normal,
                    GrowthCycle = 300,
                    Yield = 1,
                    SeedId = "seed_blood_grass",
                    SeedAmount = 1,
                    OutputItemId = "alchemy_herb",
                    OutputAmount = 2,
                    MinQuality = 1,
                    MaxQuality = 2,
                    UnlockLevel = 1,
                    SeedKey = "built-in:crop-template:crop_blood_grass",
                    IsBuiltIn = true,
                    BuiltInVersion = CropTemplateVersion,
                    LastUpdateTime = now
                },
                new()
                {
                    TemplateId = "crop_spirit_lotus",
                    Name = "灵泉莲",
                    Description = "蕴含灵泉气息的水生灵植，可稳定药力。",
                    Type = CropType.SpiritGrass,
                    GrowthCycle = 600,
                    Yield = 1,
                    SeedId = "seed_spirit_lotus",
                    SeedAmount = 1,
                    OutputItemId = "spirit_water",
                    OutputAmount = 2,
                    MinQuality = 1,
                    MaxQuality = 3,
                    UnlockLevel = 1,
                    SeedKey = "built-in:crop-template:crop_spirit_lotus",
                    IsBuiltIn = true,
                    BuiltInVersion = CropTemplateVersion,
                    LastUpdateTime = now
                },
                new()
                {
                    TemplateId = "crop_stone_moss",
                    Name = "灵石苔",
                    Description = "附着在灵石矿脉上的稀有苔藓，可产出灵石粉末。",
                    Type = CropType.Rare,
                    GrowthCycle = 900,
                    Yield = 1,
                    SeedId = "seed_stone_moss",
                    SeedAmount = 1,
                    OutputItemId = "spirit_dust",
                    OutputAmount = 2,
                    MinQuality = 2,
                    MaxQuality = 4,
                    UnlockLevel = 2,
                    SeedKey = "built-in:crop-template:crop_stone_moss",
                    IsBuiltIn = true,
                    BuiltInVersion = CropTemplateVersion,
                    LastUpdateTime = now
                }
            ];
        }

        private static List<SeedForgeRecipeModel> BuildBalancedForgeRecipes(
            IReadOnlyList<EquipmentTemplateEntity> equipmentTemplates,
            IReadOnlyDictionary<string, ItemTemplateEntity> itemMap)
        {
            return equipmentTemplates
                .Where(template => template.Level > 0)
                .OrderBy(template => template.Level)
                .ThenBy(template => template.Slot)
                .ThenBy(template => template.EquipmentId)
                .Select(templateEntity =>
                {
                    var template = TemplateDataMapper.ToRuntime(templateEntity);
                    var materials = BuildBalancedForgeMaterials(template, itemMap);
                    return new SeedForgeRecipeModel
                    {
                        RecipeId = $"forge_{template.EquipmentId}",
                        TemplateId = template.EquipmentId.ToString(),
                        Name = template.Name,
                        Description = template.Description,
                        SlotName = template.Slot.ToString(),
                        Quality = (int)template.Quality,
                        Level = template.Level,
                        Icon = $"equipment-{template.Slot.ToString().ToLowerInvariant()}",
                        CostGold = GetBalancedForgeGoldCost(template, materials.Sum(material => material.Count)),
                        SuccessRate = GetBalancedForgeSuccessRate(template),
                        Materials = materials
                    };
                })
                .Where(recipe => recipe.Materials.Count > 0)
                .ToList();
        }

        private static List<SeedForgeMaterialModel> BuildBalancedForgeMaterials(
            EquipmentTemplate template,
            IReadOnlyDictionary<string, ItemTemplateEntity> itemMap)
        {
            var materials = new List<SeedForgeMaterialModel>();
            var tierSuffix = GetForgeMaterialTierSuffix(template.Level);
            var tierStep = template.Level >= 25 ? 3 : template.Level >= 15 ? 2 : 1;
            var coreCount = 2 + tierStep;
            var supportCount = 1 + Math.Max(1, tierStep - 1);

            switch (template.Slot)
            {
                case EquipmentSlot.Weapon:
                    TryAddForgeMaterial(materials, itemMap, template.CombatStyle == CombatStyle.Magic ? $"crystal_{tierSuffix}" : $"ore_{tierSuffix}", coreCount);
                    TryAddForgeMaterial(materials, itemMap, $"wood_{tierSuffix}", supportCount);
                    TryAddForgeMaterial(materials, itemMap, "item_003", template.Level <= 10 ? 1 : 2);
                    if (template.Level >= 15)
                    {
                        TryAddForgeMaterial(materials, itemMap, template.CombatStyle == CombatStyle.Magic ? $"ore_{tierSuffix}" : $"crystal_{tierSuffix}", 1);
                    }
                    break;

                case EquipmentSlot.Helmet:
                case EquipmentSlot.Armor:
                case EquipmentSlot.Pants:
                case EquipmentSlot.Boots:
                    TryAddForgeMaterial(materials, itemMap, template.CombatStyle == CombatStyle.Magic ? $"crystal_{tierSuffix}" : $"ore_{tierSuffix}", 1 + tierStep);
                    TryAddForgeMaterial(materials, itemMap, $"hide_{tierSuffix}", 1 + tierStep);
                    TryAddForgeMaterial(materials, itemMap, "item_003", 1);
                    if (template.Level >= 20)
                    {
                        TryAddForgeMaterial(materials, itemMap, $"wood_{tierSuffix}", 1);
                    }
                    break;

                case EquipmentSlot.Necklace:
                case EquipmentSlot.Ring:
                    TryAddForgeMaterial(materials, itemMap, template.CombatStyle == CombatStyle.Magic ? $"crystal_{tierSuffix}" : $"ore_{tierSuffix}", 1 + tierStep);
                    TryAddForgeMaterial(materials, itemMap, $"hide_{tierSuffix}", 1);
                    TryAddForgeMaterial(materials, itemMap, "item_003", 1);
                    if (template.Level >= 15)
                    {
                        TryAddForgeMaterial(materials, itemMap, $"wood_{tierSuffix}", 1);
                    }
                    break;
            }

            var dungeonTokenId = GetForgeDungeonTokenId(template.Level);
            if (!string.IsNullOrWhiteSpace(dungeonTokenId))
            {
                TryAddForgeMaterial(materials, itemMap, dungeonTokenId, 1);
            }

            return materials;
        }

        private static void TryAddForgeMaterial(
            List<SeedForgeMaterialModel> materials,
            IReadOnlyDictionary<string, ItemTemplateEntity> itemMap,
            string itemId,
            int count)
        {
            if (count <= 0 || !itemMap.TryGetValue(itemId, out var itemTemplate))
            {
                return;
            }

            var existing = materials.FirstOrDefault(material => material.ItemId.Equals(itemId, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                existing.Count += count;
                return;
            }

            materials.Add(new SeedForgeMaterialModel
            {
                ItemId = itemId,
                Name = itemTemplate.Name,
                Icon = "item-material",
                Count = count
            });
        }

        private static long GetBalancedForgeGoldCost(EquipmentTemplate template, int materialCount)
        {
            var slotBaseCost = template.Slot switch
            {
                EquipmentSlot.Weapon => 120L,
                EquipmentSlot.Armor => 110L,
                EquipmentSlot.Helmet => 90L,
                EquipmentSlot.Pants => 95L,
                EquipmentSlot.Boots => 85L,
                EquipmentSlot.Necklace => 90L,
                EquipmentSlot.Ring => 80L,
                _ => 80L
            };

            return slotBaseCost +
                   template.Level * 28L +
                   materialCount * 14L +
                   (int)template.Quality * 35L;
        }

        private static int GetBalancedForgeSuccessRate(EquipmentTemplate template)
        {
            var qualityPenalty = Math.Max(0, (int)template.Quality) * 3;
            var levelPenalty = Math.Max(0, template.Level - 1) / 4 * 2;
            return Math.Clamp(96 - qualityPenalty - levelPenalty, 60, 95);
        }

        private static string? GetForgeDungeonTokenId(int level)
        {
            if (level >= 25)
            {
                return "dungeon_token_4";
            }

            if (level >= 20)
            {
                return "dungeon_token_3";
            }

            if (level >= 15)
            {
                return "dungeon_token_2";
            }

            if (level >= 10)
            {
                return "dungeon_token_1";
            }

            return null;
        }

        private static string GetForgeMaterialTierSuffix(int level)
        {
            if (level >= 25)
            {
                return "t3";
            }

            if (level >= 15)
            {
                return "t2";
            }

            return "t1";
        }

        private static int GetMaterialCount(EquipmentTemplate template)
        {
            var slotBase = template.Slot switch
            {
                EquipmentSlot.Weapon => 4,
                EquipmentSlot.Armor => 4,
                EquipmentSlot.Helmet => 3,
                EquipmentSlot.Pants => 3,
                EquipmentSlot.Boots => 3,
                EquipmentSlot.Necklace => 3,
                EquipmentSlot.Ring => 2,
                _ => 3
            };

            return slotBase + Math.Max(0, template.Level / 5);
        }

        private static ForgeRecipeEntity MapToForgeRecipeEntity(SeedForgeRecipeModel recipe)
        {
            return new ForgeRecipeEntity
            {
                RecipeId = recipe.RecipeId,
                TemplateId = recipe.TemplateId,
                Name = recipe.Name,
                Description = recipe.Description,
                SlotName = recipe.SlotName,
                Quality = recipe.Quality,
                Level = recipe.Level,
                Icon = recipe.Icon,
                CostGold = recipe.CostGold,
                SuccessRate = recipe.SuccessRate,
                MaterialsJson = System.Text.Json.JsonSerializer.Serialize(recipe.Materials
                    .Select(material => new ForgeRecipeMaterialSnapshot
                    {
                        ItemId = material.ItemId,
                        Name = material.Name,
                        Icon = material.Icon,
                        Count = material.Count
                    })
                    .ToList()),
                IsEnabled = true,
                SeedKey = $"built-in:forge-recipe:{recipe.RecipeId}",
                IsBuiltIn = true,
                BuiltInVersion = ForgeRecipeVersion,
                LastUpdateTime = DateTime.Now
            };
        }

        private static bool MatchesAlchemyRecipe(AlchemyRecipeEntity actual, AlchemyRecipeEntity expected)
        {
            return string.Equals(actual.PillTemplateId, expected.PillTemplateId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(actual.Name, expected.Name, StringComparison.Ordinal) &&
                   string.Equals(actual.Description ?? string.Empty, expected.Description ?? string.Empty, StringComparison.Ordinal) &&
                   actual.RequiredFurnaceLevel == expected.RequiredFurnaceLevel &&
                   actual.BaseSuccessRate == expected.BaseSuccessRate &&
                   actual.BaseCraftTime == expected.BaseCraftTime &&
                   string.Equals(actual.MaterialsJson ?? string.Empty, expected.MaterialsJson ?? string.Empty, StringComparison.Ordinal) &&
                   string.Equals(actual.UnlockCondition ?? string.Empty, expected.UnlockCondition ?? string.Empty, StringComparison.Ordinal) &&
                   actual.IsDefaultLearned == expected.IsDefaultLearned;
        }

        private static bool MatchesForgeRecipe(ForgeRecipeEntity actual, ForgeRecipeEntity expected)
        {
            return string.Equals(actual.RecipeId, expected.RecipeId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(actual.TemplateId, expected.TemplateId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(actual.Name, expected.Name, StringComparison.Ordinal) &&
                   string.Equals(actual.Description, expected.Description, StringComparison.Ordinal) &&
                   string.Equals(actual.SlotName, expected.SlotName, StringComparison.Ordinal) &&
                   actual.Quality == expected.Quality &&
                   actual.Level == expected.Level &&
                   string.Equals(actual.Icon, expected.Icon, StringComparison.Ordinal) &&
                   actual.CostGold == expected.CostGold &&
                   actual.SuccessRate == expected.SuccessRate &&
                   string.Equals(actual.MaterialsJson, expected.MaterialsJson, StringComparison.Ordinal) &&
                   actual.IsEnabled == expected.IsEnabled;
        }

        private static List<AttributePointConfigEntity> BuildAttributePointConfigEntries(DateTime now)
        {
            var definitions = AttributePointConfig.GetLegacyDefinitions()
                .OrderBy(item => item.AttributeType)
                .ToList();
            var levelRanges = AttributePointConfig.GetLegacyLevelRanges()
                .OrderBy(item => item.LevelStart)
                .ThenBy(item => item.LevelEnd)
                .ToList();
            var entries = new List<AttributePointConfigEntity>(definitions.Count * levelRanges.Count);

            foreach (var definition in definitions)
            {
                foreach (var range in levelRanges)
                {
                    entries.Add(new AttributePointConfigEntity
                    {
                        ConfigId = $"{definition.Profession}_{definition.Key}_{range.LevelStart}_{range.LevelEnd}",
                        Profession = definition.Profession,
                        Key = definition.Key,
                        Name = definition.Name,
                        AttributeType = (int)definition.AttributeType,
                        BonusPerPoint = definition.BonusPerPoint,
                        PointsPerBonus = definition.PointsPerBonus,
                        LevelStart = range.LevelStart,
                        LevelEnd = range.LevelEnd,
                        PointsGained = range.PointsGained,
                        SortOrder = range.LevelStart,
                        IsEnabled = true,
                        SeedKey = $"built-in:attribute-point:{definition.Profession}:{definition.Key}:{range.LevelStart}:{range.LevelEnd}",
                        IsBuiltIn = true,
                        BuiltInVersion = AttributePointConfigVersion,
                        LastUpdateTime = now
                    });
                }
            }

            return entries;
        }

        private static string BuildAttributePointRuntimeKey(string? profession, string? key, int levelStart, int levelEnd)
        {
            return $"{PlayerProfessionCatalog.Normalize(profession)}:{AttributePointConfig.NormalizeKey(key ?? string.Empty)}:{levelStart}:{levelEnd}";
        }

        private static bool RequiresCheckInRewardConfigReseed(
            IReadOnlyList<CheckInRewardConfigEntity> existingConfigs,
            IReadOnlyList<CheckInRewardConfigEntity> expectedConfigs)
        {
            if (existingConfigs.Count != expectedConfigs.Count)
            {
                return true;
            }

            for (var index = 0; index < expectedConfigs.Count; index++)
            {
                var actual = existingConfigs[index];
                var expected = expectedConfigs[index];
                if (actual.ContinuousDay != expected.ContinuousDay ||
                    !string.Equals(actual.ConfigVersion, expected.ConfigVersion, StringComparison.Ordinal) ||
                    actual.IsMilestone != expected.IsMilestone ||
                    !string.Equals(actual.RewardJson, expected.RewardJson, StringComparison.Ordinal) ||
                    !string.Equals(actual.Description ?? string.Empty, expected.Description ?? string.Empty, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool RequiresRedeemCodeConfigReseed(
            IReadOnlyList<RedeemCodeConfigEntity> existingConfigs,
            IReadOnlyList<RedeemCodeConfigEntity> expectedConfigs)
        {
            if (existingConfigs.Count != expectedConfigs.Count)
            {
                return true;
            }

            for (var index = 0; index < expectedConfigs.Count; index++)
            {
                var actual = existingConfigs[index];
                var expected = expectedConfigs[index];
                if (!string.Equals(actual.Code, expected.Code, StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(actual.ConfigVersion, expected.ConfigVersion, StringComparison.Ordinal) ||
                    actual.IsEnabled != expected.IsEnabled ||
                    !string.Equals(actual.Description ?? string.Empty, expected.Description ?? string.Empty, StringComparison.Ordinal) ||
                    !string.Equals(actual.RewardJson, expected.RewardJson, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private sealed class SeedForgeRecipeModel
        {
            public string RecipeId { get; set; } = string.Empty;

            public string TemplateId { get; set; } = string.Empty;

            public string Name { get; set; } = string.Empty;

            public string Description { get; set; } = string.Empty;

            public string SlotName { get; set; } = string.Empty;

            public int Quality { get; set; }

            public int Level { get; set; }

            public string Icon { get; set; } = string.Empty;

            public long CostGold { get; set; }

            public int SuccessRate { get; set; }

            public List<SeedForgeMaterialModel> Materials { get; set; } = [];
        }

        private sealed class SeedForgeMaterialModel
        {
            public string ItemId { get; set; } = string.Empty;

            public string Name { get; set; } = string.Empty;

            public string Icon { get; set; } = string.Empty;

            public int Count { get; set; }
        }
    }
}
