#pragma warning disable CS1591
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Data;
using XXX.Infrastructure.SeedData;

namespace XXX.Application.Services
{
    /// <summary>
    /// 管理后台保存后的统一缓存刷新实现。
    /// </summary>
    public class AdminRuntimeRefreshService : IAdminRuntimeRefreshService
    {
        private readonly DbContext _dbContext;
        private readonly IRuntimeTemplateLoader _runtimeTemplateLoader;
        private readonly IShopService _shopService;
        private readonly IRankingService _rankingService;
        private readonly IQuestService _questService;
        private readonly IAchievementService _achievementService;
        private readonly IStarterPackageService _starterPackageService;
        private readonly IAdminRewardConfigService _adminRewardConfigService;
        private readonly IGrowthConfigRuntimeService _growthConfigRuntimeService;
        private readonly IFiveElementRuleRuntimeService _fiveElementRuleRuntimeService;
        private readonly ISpiritFieldRuleRuntimeService _spiritFieldRuleRuntimeService;
        private readonly IAlchemyProfessionRuleRuntimeService _alchemyProfessionRuleRuntimeService;
        private readonly IForgeProfessionRuleRuntimeService _forgeProfessionRuleRuntimeService;
        private readonly IEquipmentRerollRuleRuntimeService _equipmentRerollRuleRuntimeService;
        private readonly IElementRelationRuleRuntimeService _elementRelationRuleRuntimeService;

        public AdminRuntimeRefreshService(
            DbContext dbContext,
            IRuntimeTemplateLoader runtimeTemplateLoader,
            IShopService shopService,
            IRankingService rankingService,
            IQuestService questService,
            IAchievementService achievementService,
            IStarterPackageService starterPackageService,
            IAdminRewardConfigService adminRewardConfigService,
            IGrowthConfigRuntimeService growthConfigRuntimeService,
            IFiveElementRuleRuntimeService fiveElementRuleRuntimeService,
            ISpiritFieldRuleRuntimeService spiritFieldRuleRuntimeService,
            IAlchemyProfessionRuleRuntimeService alchemyProfessionRuleRuntimeService,
            IForgeProfessionRuleRuntimeService forgeProfessionRuleRuntimeService,
            IEquipmentRerollRuleRuntimeService equipmentRerollRuleRuntimeService,
            IElementRelationRuleRuntimeService elementRelationRuleRuntimeService)
        {
            _dbContext = dbContext;
            _runtimeTemplateLoader = runtimeTemplateLoader;
            _shopService = shopService;
            _rankingService = rankingService;
            _questService = questService;
            _achievementService = achievementService;
            _starterPackageService = starterPackageService;
            _adminRewardConfigService = adminRewardConfigService;
            _growthConfigRuntimeService = growthConfigRuntimeService;
            _fiveElementRuleRuntimeService = fiveElementRuleRuntimeService;
            _spiritFieldRuleRuntimeService = spiritFieldRuleRuntimeService;
            _alchemyProfessionRuleRuntimeService = alchemyProfessionRuleRuntimeService;
            _forgeProfessionRuleRuntimeService = forgeProfessionRuleRuntimeService;
            _equipmentRerollRuleRuntimeService = equipmentRerollRuleRuntimeService;
            _elementRelationRuleRuntimeService = elementRelationRuleRuntimeService;
        }

        public Task ReloadRuntimeTemplatesAsync()
        {
            return _runtimeTemplateLoader.LoadAsync();
        }

        public Task ReloadShopCacheAsync()
        {
            return _shopService.ReloadCacheAsync();
        }

        public Task ReloadRankingCacheAsync()
        {
            return _rankingService.ReloadCacheAsync();
        }

        public Task ReloadQuestCacheAsync()
        {
            return _questService.ReloadCacheAsync();
        }

        public Task ReloadAchievementCacheAsync()
        {
            return _achievementService.ReloadCacheAsync();
        }

        public Task ReloadStarterPackageCacheAsync()
        {
            return _starterPackageService.ReloadCacheAsync();
        }

        public Task ReloadGrowthConfigCacheAsync()
        {
            return _growthConfigRuntimeService.LoadAsync();
        }

        public Task ReloadFiveElementRuleCacheAsync()
        {
            return _fiveElementRuleRuntimeService.LoadAsync();
        }

        public Task ReloadSpiritFieldRuleCacheAsync()
        {
            return _spiritFieldRuleRuntimeService.LoadAsync();
        }

        public Task ReloadAlchemyProfessionRuleCacheAsync()
        {
            return _alchemyProfessionRuleRuntimeService.LoadAsync();
        }

        public Task ReloadForgeProfessionRuleCacheAsync()
        {
            return _forgeProfessionRuleRuntimeService.LoadAsync();
        }

        public Task ReloadEquipmentRerollRuleCacheAsync()
        {
            return _equipmentRerollRuleRuntimeService.LoadAsync();
        }

        public Task ReloadElementRelationRuleCacheAsync()
        {
            return _elementRelationRuleRuntimeService.LoadAsync();
        }

        public async Task<List<AdminRuntimeConfigDomainDto>> GetDomainStatusesAsync()
        {
            await EnsureDomainRowsAsync();

            var entities = await _dbContext.Db.Queryable<SystemConfigVersionEntity>()
                .OrderBy(entity => entity.ConfigDomain)
                .ToListAsync();
            var entityMap = entities.ToDictionary(entity => entity.ConfigDomain, StringComparer.OrdinalIgnoreCase);
            var results = new List<AdminRuntimeConfigDomainDto>();
            foreach (var domain in RuntimeConfigDomainCatalog.GetAll())
            {
                var entity = entityMap.GetValueOrDefault(domain.Domain);
                results.Add(new AdminRuntimeConfigDomainDto
                {
                    Domain = domain.Domain,
                    Name = domain.Name,
                    Group = domain.Group,
                    Description = domain.Description,
                    RefreshSupported = domain.RefreshSupported,
                    CurrentVersion = entity?.CurrentVersion ?? await ResolveCurrentVersionAsync(domain),
                    LastAppliedAt = entity?.LastAppliedAt,
                    LastAppliedBy = entity?.LastAppliedBy,
                    LastRefreshStatus = entity?.LastRefreshStatus ?? (domain.RefreshSupported ? "未执行" : "待接入"),
                    LastRefreshMessage = entity?.LastRefreshMessage,
                    RefreshCount = entity?.RefreshCount ?? 0,
                    CreateTime = entity?.CreateTime,
                    LastUpdateTime = entity?.LastUpdateTime
                });
            }

            return results
                .OrderBy(item => item.Group)
                .ThenBy(item => item.Name)
                .ToList();
        }

        public async Task<AdminRuntimeConfigRefreshResultDto> RefreshDomainAsync(string domain, string? operatorName = null)
        {
            var definition = RuntimeConfigDomainCatalog.Find(domain)
                ?? throw new InvalidOperationException("运行时配置域不存在。");

            await EnsureDomainRowsAsync();
            var entity = await GetOrCreateDomainEntityAsync(definition);

            if (!definition.RefreshSupported)
            {
                entity.LastRefreshStatus = "待接入";
                entity.LastRefreshMessage = "该配置域尚未接入独立刷新器，暂不支持后台主动刷新。";
                entity.LastAppliedAt = DateTime.Now;
                entity.LastAppliedBy = NormalizeOperatorName(operatorName);
                entity.LastUpdateTime = DateTime.Now;
                await SaveDomainEntityAsync(entity);

                return new AdminRuntimeConfigRefreshResultDto
                {
                    Domain = definition.Domain,
                    Name = definition.Name,
                    Success = false,
                    Message = entity.LastRefreshMessage,
                    Status = await BuildDomainStatusAsync(definition, entity)
                };
            }

            try
            {
                await ExecuteDomainRefreshAsync(definition.Domain, operatorName);

                entity.CurrentVersion = await ResolveCurrentVersionAsync(definition);
                entity.LastAppliedAt = DateTime.Now;
                entity.LastAppliedBy = NormalizeOperatorName(operatorName);
                entity.LastRefreshStatus = "成功";
                entity.LastRefreshMessage = "运行时缓存刷新完成。";
                entity.RefreshCount += 1;
                entity.LastUpdateTime = DateTime.Now;
                await SaveDomainEntityAsync(entity);

                return new AdminRuntimeConfigRefreshResultDto
                {
                    Domain = definition.Domain,
                    Name = definition.Name,
                    Success = true,
                    Message = "运行时缓存刷新成功。",
                    Status = await BuildDomainStatusAsync(definition, entity)
                };
            }
            catch (Exception ex)
            {
                entity.LastAppliedAt = DateTime.Now;
                entity.LastAppliedBy = NormalizeOperatorName(operatorName);
                entity.LastRefreshStatus = "失败";
                entity.LastRefreshMessage = ex.Message;
                entity.RefreshCount += 1;
                entity.LastUpdateTime = DateTime.Now;
                await SaveDomainEntityAsync(entity);
                throw;
            }
        }

        public async Task<List<AdminRuntimeConfigRefreshResultDto>> RefreshAllAsync(string? operatorName = null)
        {
            var results = new List<AdminRuntimeConfigRefreshResultDto>();
            foreach (var definition in RuntimeConfigDomainCatalog.GetAll().Where(item => item.RefreshSupported))
            {
                results.Add(await RefreshDomainAsync(definition.Domain, operatorName));
            }

            return results;
        }

        private async Task EnsureDomainRowsAsync()
        {
            var existingEntities = await _dbContext.Db.Queryable<SystemConfigVersionEntity>()
                .ToListAsync();
            var existingSet = existingEntities
                .Select(entity => entity.ConfigDomain)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var entity in existingEntities)
            {
                var definition = RuntimeConfigDomainCatalog.Find(entity.ConfigDomain);
                if (definition == null)
                {
                    continue;
                }

                var changed = false;
                var resolvedVersion = await ResolveCurrentVersionAsync(definition);
                if (!string.Equals(entity.CurrentVersion, resolvedVersion, StringComparison.OrdinalIgnoreCase))
                {
                    entity.CurrentVersion = resolvedVersion;
                    changed = true;
                }

                if (definition.RefreshSupported && string.Equals(entity.LastRefreshStatus, "待接入", StringComparison.OrdinalIgnoreCase))
                {
                    entity.LastRefreshStatus = "未执行";
                    entity.LastRefreshMessage = "尚未执行后台刷新。";
                    changed = true;
                }

                if (!definition.RefreshSupported && !string.Equals(entity.LastRefreshStatus, "待接入", StringComparison.OrdinalIgnoreCase))
                {
                    entity.LastRefreshStatus = "待接入";
                    entity.LastRefreshMessage = "该配置域将在后续阶段接入独立刷新器。";
                    changed = true;
                }

                if (changed)
                {
                    entity.LastUpdateTime = DateTime.Now;
                    await SaveDomainEntityAsync(entity);
                }
            }

            var missingEntities = new List<SystemConfigVersionEntity>();
            foreach (var domain in RuntimeConfigDomainCatalog.GetAll().Where(domain => !existingSet.Contains(domain.Domain)))
            {
                missingEntities.Add(new SystemConfigVersionEntity
                {
                    ConfigDomain = domain.Domain,
                    CurrentVersion = await ResolveCurrentVersionAsync(domain),
                    LastRefreshStatus = domain.RefreshSupported ? "未执行" : "待接入",
                    LastRefreshMessage = domain.RefreshSupported ? "尚未执行后台刷新。" : "该配置域将在后续阶段接入独立刷新器。",
                    CreateTime = DateTime.Now,
                    LastUpdateTime = DateTime.Now
                });
            }

            if (missingEntities.Count == 0)
            {
                return;
            }

            await _dbContext.Db.Insertable(missingEntities).ExecuteCommandAsync();
        }

        private async Task<SystemConfigVersionEntity> GetOrCreateDomainEntityAsync(RuntimeConfigDomainCatalog.RuntimeConfigDomainDefinition definition)
        {
            var entity = await _dbContext.Db.Queryable<SystemConfigVersionEntity>()
                .FirstAsync(item => item.ConfigDomain == definition.Domain);
            if (entity != null)
            {
                return entity;
            }

            entity = new SystemConfigVersionEntity
            {
                ConfigDomain = definition.Domain,
                CurrentVersion = await ResolveCurrentVersionAsync(definition),
                LastRefreshStatus = definition.RefreshSupported ? "未执行" : "待接入",
                LastRefreshMessage = definition.RefreshSupported ? "尚未执行后台刷新。" : "该配置域将在后续阶段接入独立刷新器。",
                CreateTime = DateTime.Now,
                LastUpdateTime = DateTime.Now
            };

            await _dbContext.Db.Insertable(entity).ExecuteCommandAsync();
            return entity;
        }

        private async Task SaveDomainEntityAsync(SystemConfigVersionEntity entity)
        {
            var exists = await _dbContext.Db.Queryable<SystemConfigVersionEntity>()
                .AnyAsync(item => item.ConfigDomain == entity.ConfigDomain);

            if (exists)
            {
                await _dbContext.Db.Updateable(entity)
                    .WhereColumns(item => item.ConfigDomain)
                    .ExecuteCommandAsync();
                return;
            }

            await _dbContext.Db.Insertable(entity).ExecuteCommandAsync();
        }

        private async Task<AdminRuntimeConfigDomainDto> BuildDomainStatusAsync(
            RuntimeConfigDomainCatalog.RuntimeConfigDomainDefinition definition,
            SystemConfigVersionEntity? entity)
        {
            return new AdminRuntimeConfigDomainDto
            {
                Domain = definition.Domain,
                Name = definition.Name,
                Group = definition.Group,
                Description = definition.Description,
                RefreshSupported = definition.RefreshSupported,
                CurrentVersion = entity?.CurrentVersion ?? await ResolveCurrentVersionAsync(definition),
                LastAppliedAt = entity?.LastAppliedAt,
                LastAppliedBy = entity?.LastAppliedBy,
                LastRefreshStatus = entity?.LastRefreshStatus ?? (definition.RefreshSupported ? "未执行" : "待接入"),
                LastRefreshMessage = entity?.LastRefreshMessage,
                RefreshCount = entity?.RefreshCount ?? 0,
                CreateTime = entity?.CreateTime,
                LastUpdateTime = entity?.LastUpdateTime
            };
        }

        private async Task ExecuteDomainRefreshAsync(string domain, string? operatorName)
        {
            switch (domain.ToLowerInvariant())
            {
                case RuntimeConfigDomainCatalog.Template:
                    await _runtimeTemplateLoader.LoadAsync();
                    break;
                case RuntimeConfigDomainCatalog.Shop:
                    await _shopService.ReloadCacheAsync();
                    break;
                case RuntimeConfigDomainCatalog.Ranking:
                    await _rankingService.ReloadCacheAsync();
                    break;
                case RuntimeConfigDomainCatalog.Quest:
                    await _questService.ReloadCacheAsync();
                    break;
                case RuntimeConfigDomainCatalog.Achievement:
                    await _achievementService.ReloadCacheAsync();
                    break;
                case RuntimeConfigDomainCatalog.Content:
                    await _adminRewardConfigService.ReloadBuiltInActivityConfigsAsync(operatorName);
                    await _questService.ReloadCacheAsync();
                    await _achievementService.ReloadCacheAsync();
                    await _shopService.ReloadCacheAsync();
                    await _rankingService.ReloadCacheAsync();
                    await _starterPackageService.ReloadCacheAsync();
                    break;
                case RuntimeConfigDomainCatalog.Activity:
                    await _adminRewardConfigService.ReloadBuiltInActivityConfigsAsync(operatorName);
                    break;
                case RuntimeConfigDomainCatalog.Growth:
                    await _growthConfigRuntimeService.LoadAsync();
                    break;
                case RuntimeConfigDomainCatalog.FiveElement:
                    await _fiveElementRuleRuntimeService.LoadAsync();
                    break;
                case RuntimeConfigDomainCatalog.SpiritField:
                    await _spiritFieldRuleRuntimeService.LoadAsync();
                    break;
                case RuntimeConfigDomainCatalog.Alchemy:
                    await _alchemyProfessionRuleRuntimeService.LoadAsync();
                    break;
                case RuntimeConfigDomainCatalog.Forge:
                    await _forgeProfessionRuleRuntimeService.LoadAsync();
                    break;
                case RuntimeConfigDomainCatalog.EquipmentReroll:
                    await _equipmentRerollRuleRuntimeService.LoadAsync();
                    break;
                case RuntimeConfigDomainCatalog.BattleRule:
                    await _elementRelationRuleRuntimeService.LoadAsync();
                    break;
                case RuntimeConfigDomainCatalog.StarterPackage:
                    await _starterPackageService.ReloadCacheAsync();
                    break;
                default:
                    throw new InvalidOperationException("当前配置域尚未接入刷新实现。");
            }
        }

        private async Task<string> ResolveCurrentVersionAsync(RuntimeConfigDomainCatalog.RuntimeConfigDomainDefinition definition)
        {
            if (string.Equals(definition.Domain, RuntimeConfigDomainCatalog.Growth, StringComparison.OrdinalIgnoreCase))
            {
                return await ResolveGrowthCurrentVersionAsync();
            }

            if (string.Equals(definition.Domain, RuntimeConfigDomainCatalog.Template, StringComparison.OrdinalIgnoreCase))
            {
                return await ResolveTemplateCurrentVersionAsync();
            }

            if (string.Equals(definition.Domain, RuntimeConfigDomainCatalog.Activity, StringComparison.OrdinalIgnoreCase))
            {
                return await ResolveActivityCurrentVersionAsync();
            }

            if (string.Equals(definition.Domain, RuntimeConfigDomainCatalog.Quest, StringComparison.OrdinalIgnoreCase))
            {
                return await ResolveQuestCurrentVersionAsync();
            }

            if (string.Equals(definition.Domain, RuntimeConfigDomainCatalog.Achievement, StringComparison.OrdinalIgnoreCase))
            {
                return await ResolveAchievementCurrentVersionAsync();
            }

            if (string.Equals(definition.Domain, RuntimeConfigDomainCatalog.Shop, StringComparison.OrdinalIgnoreCase))
            {
                return await ResolveShopCurrentVersionAsync();
            }

            if (string.Equals(definition.Domain, RuntimeConfigDomainCatalog.Ranking, StringComparison.OrdinalIgnoreCase))
            {
                return await ResolveRankingCurrentVersionAsync();
            }

            if (string.Equals(definition.Domain, RuntimeConfigDomainCatalog.Content, StringComparison.OrdinalIgnoreCase))
            {
                return await ResolveContentCurrentVersionAsync();
            }

            if (string.Equals(definition.Domain, RuntimeConfigDomainCatalog.BattleRule, StringComparison.OrdinalIgnoreCase))
            {
                return await ResolveBattleRuleCurrentVersionAsync();
            }

            if (string.Equals(definition.Domain, RuntimeConfigDomainCatalog.FiveElement, StringComparison.OrdinalIgnoreCase))
            {
                return await ResolveFiveElementCurrentVersionAsync();
            }

            if (string.Equals(definition.Domain, RuntimeConfigDomainCatalog.SpiritField, StringComparison.OrdinalIgnoreCase))
            {
                return await ResolveSpiritFieldCurrentVersionAsync();
            }

            if (string.Equals(definition.Domain, RuntimeConfigDomainCatalog.Alchemy, StringComparison.OrdinalIgnoreCase))
            {
                return await ResolveAlchemyCurrentVersionAsync();
            }

            if (string.Equals(definition.Domain, RuntimeConfigDomainCatalog.Forge, StringComparison.OrdinalIgnoreCase))
            {
                return await ResolveForgeCurrentVersionAsync();
            }

            if (string.Equals(definition.Domain, RuntimeConfigDomainCatalog.EquipmentReroll, StringComparison.OrdinalIgnoreCase))
            {
                return await ResolveEquipmentRerollCurrentVersionAsync();
            }

            if (string.Equals(definition.Domain, RuntimeConfigDomainCatalog.StarterPackage, StringComparison.OrdinalIgnoreCase))
            {
                return await ResolveStarterPackageCurrentVersionAsync();
            }

            return ResolveFallbackCurrentVersion(definition);
        }

        private async Task<string> ResolveGrowthCurrentVersionAsync()
        {
            var versions = new List<string>();

            await AppendVersionMarkersAsync<PlayerLevelConfigEntity>(versions, "player-level-config-manual");
            await AppendVersionMarkersAsync<RealmLevelConfigEntity>(versions, "realm-level-config-manual");
            await AppendVersionMarkersAsync<AttributePointConfigEntity>(versions, "attribute-point-config-manual");
            await AppendVersionMarkersAsync<PlayerInitialResourceConfigEntity>(versions, "player-initial-resource-config-manual");

            return JoinVersionsOrFallback(versions, RuntimeConfigDomainCatalog.Growth);
        }

        private async Task<string> ResolveActivityCurrentVersionAsync()
        {
            var versions = new List<string>();

            versions.AddRange(await QueryConfigVersionsAsync<CheckInRewardConfigEntity>());
            versions.AddRange(await QueryConfigVersionsAsync<RedeemCodeConfigEntity>());

            return JoinVersionsOrFallback(versions, RuntimeConfigDomainCatalog.Activity);
        }

        private async Task<string> ResolveShopCurrentVersionAsync()
        {
            var versions = new List<string>();

            versions.AddRange(await _dbContext.Db.Queryable<ShopConfigEntity>()
                .Where(item => item.IsBuiltIn && item.BuiltInVersion != null && item.BuiltInVersion != "")
                .Select(item => item.BuiltInVersion!)
                .Distinct()
                .ToListAsync());

            versions.AddRange(await _dbContext.Db.Queryable<ShopItemEntity>()
                .Where(item => item.IsBuiltIn && item.BuiltInVersion != null && item.BuiltInVersion != "")
                .Select(item => item.BuiltInVersion!)
                .Distinct()
                .ToListAsync());

            var distinctVersions = versions
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(item => item, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return distinctVersions.Count == 0
                ? ResolveFallbackCurrentVersion(RuntimeConfigDomainCatalog.Find(RuntimeConfigDomainCatalog.Shop)!)
                : string.Join(" / ", distinctVersions);
        }

        private async Task<string> ResolveQuestCurrentVersionAsync()
        {
            var versions = await QueryBuiltInVersionsAsync<QuestConfigEntity>();
            return JoinVersionsOrFallback(versions, RuntimeConfigDomainCatalog.Quest);
        }

        private async Task<string> ResolveAchievementCurrentVersionAsync()
        {
            var versions = await QueryBuiltInVersionsAsync<AchievementConfigEntity>();
            return JoinVersionsOrFallback(versions, RuntimeConfigDomainCatalog.Achievement);
        }

        private async Task<string> ResolveTemplateCurrentVersionAsync()
        {
            var versions = new List<string>();

            async Task AddVersionsAsync<T>() where T : class, new()
            {
                versions.AddRange(await _dbContext.Db.Queryable<T>()
                    .Where("IsBuiltIn = 1 AND BuiltInVersion IS NOT NULL AND BuiltInVersion <> ''")
                    .Select<string>("BuiltInVersion")
                    .Distinct()
                    .ToListAsync());
            }

            await AddVersionsAsync<ItemTemplateEntity>();
            await AddVersionsAsync<EquipmentTemplateEntity>();
            await AddVersionsAsync<MonsterTemplateEntity>();
            await AddVersionsAsync<MapTemplateEntity>();
            await AddVersionsAsync<SkillTemplateEntity>();
            await AddVersionsAsync<BuffTemplateEntity>();
            await AddVersionsAsync<PetTemplateEntity>();
            await AddVersionsAsync<DungeonTemplateEntity>();
            await AddVersionsAsync<CropTemplateEntity>();
            await AddVersionsAsync<AlchemyRecipeEntity>();
            await AddVersionsAsync<ForgeRecipeEntity>();

            var distinctVersions = versions
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(item => item, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return distinctVersions.Count == 0
                ? ResolveFallbackCurrentVersion(RuntimeConfigDomainCatalog.Find(RuntimeConfigDomainCatalog.Template)!)
                : string.Join(" / ", distinctVersions);
        }

        private async Task<string> ResolveRankingCurrentVersionAsync()
        {
            var versions = new List<string>();

            versions.AddRange(await _dbContext.Db.Queryable<RankingConfigEntity>()
                .Where(item => item.IsBuiltIn && item.BuiltInVersion != null && item.BuiltInVersion != "")
                .Select(item => item.BuiltInVersion!)
                .Distinct()
                .ToListAsync());

            versions.AddRange(await _dbContext.Db.Queryable<RankingRewardEntity>()
                .Where(item => item.IsBuiltIn && item.BuiltInVersion != null && item.BuiltInVersion != "")
                .Select(item => item.BuiltInVersion!)
                .Distinct()
                .ToListAsync());

            var distinctVersions = versions
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(item => item, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return distinctVersions.Count == 0
                ? ResolveFallbackCurrentVersion(RuntimeConfigDomainCatalog.Find(RuntimeConfigDomainCatalog.Ranking)!)
                : string.Join(" / ", distinctVersions);
        }

        private async Task<string> ResolveContentCurrentVersionAsync()
        {
            var versions = new List<string>();

            versions.AddRange(await QueryBuiltInVersionsAsync<QuestConfigEntity>());
            versions.AddRange(await QueryBuiltInVersionsAsync<AchievementConfigEntity>());
            versions.AddRange(await QueryBuiltInVersionsAsync<ShopConfigEntity>());
            versions.AddRange(await QueryBuiltInVersionsAsync<ShopItemEntity>());
            versions.AddRange(await QueryBuiltInVersionsAsync<RankingConfigEntity>());
            versions.AddRange(await QueryBuiltInVersionsAsync<RankingRewardEntity>());

            return JoinVersionsOrFallback(versions, RuntimeConfigDomainCatalog.Content);
        }

        private async Task<string> ResolveBattleRuleCurrentVersionAsync()
        {
            var versions = await _dbContext.Db.Queryable<ElementRelationRuleEntity>()
                .Where(item => item.IsBuiltIn && item.BuiltInVersion != null && item.BuiltInVersion != "")
                .Select(item => item.BuiltInVersion!)
                .Distinct()
                .ToListAsync();

            var distinctVersions = versions
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(item => item, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return distinctVersions.Count == 0
                ? ResolveFallbackCurrentVersion(RuntimeConfigDomainCatalog.Find(RuntimeConfigDomainCatalog.BattleRule)!)
                : string.Join(" / ", distinctVersions);
        }

        private async Task<string> ResolveFiveElementCurrentVersionAsync()
        {
            var versions = new List<string>();

            versions.AddRange(await QueryBuiltInVersionsAsync<FiveElementLevelConfigEntity>());
            versions.AddRange(await QueryBuiltInVersionsAsync<FiveElementBranchUpgradeConfigEntity>());

            return JoinVersionsOrFallback(versions, RuntimeConfigDomainCatalog.FiveElement);
        }

        private async Task<string> ResolveSpiritFieldCurrentVersionAsync()
        {
            var versions = new List<string>();

            versions.AddRange(await QueryBuiltInVersionsAsync<SpiritFieldSystemConfigEntity>());
            versions.AddRange(await QueryBuiltInVersionsAsync<SpiritFieldSpeedUpItemConfigEntity>());

            return JoinVersionsOrFallback(versions, RuntimeConfigDomainCatalog.SpiritField);
        }

        private async Task<string> ResolveAlchemyCurrentVersionAsync()
        {
            var versions = new List<string>();

            versions.AddRange(await QueryBuiltInVersionsAsync<AlchemyProfessionLevelConfigEntity>());
            versions.AddRange(await QueryBuiltInVersionsAsync<AlchemyProfessionRuleConfigEntity>());

            return JoinVersionsOrFallback(versions, RuntimeConfigDomainCatalog.Alchemy);
        }

        private async Task<string> ResolveForgeCurrentVersionAsync()
        {
            var versions = new List<string>();

            versions.AddRange(await QueryBuiltInVersionsAsync<ForgeProfessionLevelConfigEntity>());
            versions.AddRange(await QueryBuiltInVersionsAsync<ForgeProfessionRuleConfigEntity>());

            return JoinVersionsOrFallback(versions, RuntimeConfigDomainCatalog.Forge);
        }

        private async Task<string> ResolveEquipmentRerollCurrentVersionAsync()
        {
            var versions = new List<string>();

            versions.AddRange(await QueryBuiltInVersionsAsync<EquipmentRerollSystemConfigEntity>());
            versions.AddRange(await QueryBuiltInVersionsAsync<EquipmentRerollSlotPoolConfigEntity>());
            versions.AddRange(await QueryBuiltInVersionsAsync<EquipmentRerollTierConfigEntity>());
            versions.AddRange(await QueryBuiltInVersionsAsync<EquipmentRerollAttributeValueConfigEntity>());

            return JoinVersionsOrFallback(versions, RuntimeConfigDomainCatalog.EquipmentReroll);
        }

        private async Task<string> ResolveStarterPackageCurrentVersionAsync()
        {
            var versions = await QueryBuiltInVersionsAsync<StarterPackageConfigEntity>();
            return JoinVersionsOrFallback(versions, RuntimeConfigDomainCatalog.StarterPackage);
        }

        private async Task AppendVersionMarkersAsync<T>(List<string> versions, string manualVersionPrefix)
            where T : class, new()
        {
            versions.AddRange(await QueryBuiltInVersionsAsync<T>());

            var latestManualUpdate = await QueryLatestManualUpdateAsync<T>();
            if (latestManualUpdate.HasValue)
            {
                versions.Add($"{manualVersionPrefix}-{latestManualUpdate.Value:yyyyMMddHHmmss}");
            }
        }

        private async Task<List<string>> QueryBuiltInVersionsAsync<T>()
            where T : class, new()
        {
            return await _dbContext.Db.Queryable<T>()
                .Where("IsBuiltIn = 1 AND BuiltInVersion IS NOT NULL AND BuiltInVersion <> ''")
                .Select<string>("BuiltInVersion")
                .Distinct()
                .ToListAsync();
        }

        private async Task<List<string>> QueryConfigVersionsAsync<T>()
            where T : class, new()
        {
            return await _dbContext.Db.Queryable<T>()
                .Where("ConfigVersion IS NOT NULL AND ConfigVersion <> ''")
                .Select<string>("ConfigVersion")
                .Distinct()
                .ToListAsync();
        }

        private async Task<DateTime?> QueryLatestManualUpdateAsync<T>()
            where T : class, new()
        {
            var timestamps = await _dbContext.Db.Queryable<T>()
                .Where("IsBuiltIn = 0")
                .OrderBy("LastUpdateTime DESC")
                .Select<DateTime>("LastUpdateTime")
                .Take(1)
                .ToListAsync();

            return timestamps.Count == 0 ? null : timestamps[0];
        }

        private string JoinVersionsOrFallback(IEnumerable<string> versions, string domain)
        {
            var distinctVersions = versions
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(item => item, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return distinctVersions.Count == 0
                ? ResolveFallbackCurrentVersion(RuntimeConfigDomainCatalog.Find(domain)!)
                : string.Join(" / ", distinctVersions);
        }

        private static string ResolveFallbackCurrentVersion(RuntimeConfigDomainCatalog.RuntimeConfigDomainDefinition definition)
        {
            return string.IsNullOrWhiteSpace(definition.CurrentVersion)
                ? "pending"
                : definition.CurrentVersion;
        }

        private static string NormalizeOperatorName(string? operatorName)
        {
            return string.IsNullOrWhiteSpace(operatorName) ? "system" : operatorName.Trim();
        }
    }
}
#pragma warning restore CS1591
