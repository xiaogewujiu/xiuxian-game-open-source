using Microsoft.Extensions.Logging;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Data;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    /// <summary>
    /// 灵田服务。
    /// </summary>
    /// <remarks>
    /// 负责灵田系统的懒初始化、播种、收获、催熟、升级，以及与玩家背包的真实联动。
    /// </remarks>
    public class SpiritFieldService : ISpiritFieldService
    {
        private readonly DbContext _dbContext;
        private readonly IRepository<SpiritFieldPlotEntity> _plotRepository;
        private readonly IRepository<SpiritFieldSystemEntity> _systemRepository;
        private readonly IRepository<CropTemplateEntity> _cropTemplateRepository;
        private readonly IRepository<FiveElementArrayEntity> _fiveElementRepository;
        private readonly IAchievementService _achievementService;
        private readonly IQuestService _questService;
        private readonly ILogger<SpiritFieldService> _logger;

        /// <summary>
        /// 初始化灵田服务。
        /// </summary>
        public SpiritFieldService(
            DbContext dbContext,
            IRepository<SpiritFieldPlotEntity> plotRepository,
            IRepository<SpiritFieldSystemEntity> systemRepository,
            IRepository<CropTemplateEntity> cropTemplateRepository,
            IRepository<FiveElementArrayEntity> fiveElementRepository,
            IAchievementService achievementService,
            IQuestService questService,
            ILogger<SpiritFieldService> logger)
        {
            _dbContext = dbContext;
            _plotRepository = plotRepository;
            _systemRepository = systemRepository;
            _cropTemplateRepository = cropTemplateRepository;
            _fiveElementRepository = fiveElementRepository;
            _achievementService = achievementService;
            _questService = questService;
            _logger = logger;
        }

        /// <summary>
        /// 获取玩家灵田总览。
        /// </summary>
        public async Task<SpiritFieldDto> GetSpiritFieldAsync(string playerId)
        {
            // 中文注释：
            // 灵田系统按需初始化。玩家第一次打开灵田弹窗时，如果没有系统记录，会先自动补齐默认灵田和地块。
            var system = await PreparePlayerFieldAsync(playerId);
            var plots = await _plotRepository.GetListAsync(plot => plot.PlayerId == playerId);
            var cropNameMap = await LoadCropNameMapAsync(plots);
            var speedUpItems = await LoadSpeedUpItemsAsync();

            return new SpiritFieldDto
            {
                SpeedUpItems = speedUpItems,
                FieldLevel = system.FieldLevel,
                UnlockedPlots = system.UnlockedPlots,
                MaxPlots = system.MaxPlots,
                GlobalYieldBonus = system.GlobalYieldBonus,
                GlobalGrowthSpeedBonus = system.GlobalGrowthSpeedBonus,
                TodayPlantCount = system.TodayPlantCount,
                TodayHarvestCount = system.TodayHarvestCount,
                TotalPlantCount = system.TotalPlantCount,
                TotalHarvestCount = system.TotalHarvestCount,
                Plots = plots
                    .OrderBy(plot => plot.PlotNumber)
                    .Select(plot => MapPlotToDto(plot, cropNameMap))
                .ToList()
            };
        }

        private async Task<List<SpiritFieldSpeedUpItemDto>> LoadSpeedUpItemsAsync()
        {
            var configs = await _dbContext.Db.Queryable<SpiritFieldSpeedUpItemConfigEntity>()
                .Where(item => item.IsEnabled)
                .OrderBy(item => item.SortOrder)
                .ToListAsync();
            configs = configs
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.ItemId, StringComparer.OrdinalIgnoreCase)
                .ToList();
            var itemIds = configs
                .Select(item => item.ItemId)
                .Where(itemId => !string.IsNullOrWhiteSpace(itemId))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            var itemNames = itemIds.Count == 0
                ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                : (await _dbContext.Db.Queryable<ItemTemplateEntity>()
                        .Where(item => itemIds.Contains(item.ItemId))
                        .ToListAsync())
                    .ToDictionary(item => item.ItemId, item => item.Name, StringComparer.OrdinalIgnoreCase);

            return configs
                .Where(item => !string.IsNullOrWhiteSpace(item.ItemId))
                .Select(item => new SpiritFieldSpeedUpItemDto
                {
                    ItemId = item.ItemId,
                    Name = itemNames.TryGetValue(item.ItemId, out var name) && !string.IsNullOrWhiteSpace(name)
                        ? name
                        : item.ItemId,
                    SpeedUpSeconds = Math.Max(1, item.SpeedUpSeconds)
                })
                .ToList();
        }

        /// <summary>
        /// 获取当前可种植作物列表。
        /// </summary>
        public async Task<List<CropTemplateDto>> GetAvailableCropsAsync(string playerId)
        {
            var system = await PreparePlayerFieldAsync(playerId);
            var crops = await _cropTemplateRepository.GetListAsync(crop => crop.UnlockLevel <= system.FieldLevel);

            return crops
                .OrderBy(crop => crop.UnlockLevel)
                .ThenBy(crop => crop.TemplateId)
                .Select(crop => new CropTemplateDto
                {
                    TemplateId = crop.TemplateId,
                    Name = crop.Name,
                    Type = crop.Type.ToString(),
                    GrowthCycle = crop.GrowthCycle,
                    Yield = crop.Yield,
                    SeedId = crop.SeedId,
                    SeedName = ResolveItemDisplayName(crop.SeedId),
                    SeedAmount = crop.SeedAmount,
                    OutputItemId = crop.OutputItemId,
                    OutputName = ResolveItemDisplayName(crop.OutputItemId),
                    OutputAmount = crop.OutputAmount,
                    UnlockLevel = crop.UnlockLevel
                })
                .ToList();
        }

        /// <summary>
        /// 在指定地块播种。
        /// </summary>
        public async Task<bool> PlantCropAsync(string playerId, PlantCropRequestDto request)
        {
            if (request.PlotNumber <= 0 || string.IsNullOrWhiteSpace(request.CropTemplateId))
            {
                _logger.LogWarning(
                    "Plant crop rejected due to invalid request. PlayerId={PlayerId}, PlotNumber={PlotNumber}, CropTemplateId={CropTemplateId}",
                    playerId,
                    request.PlotNumber,
                    request.CropTemplateId);
                return false;
            }

            var system = await PreparePlayerFieldAsync(playerId);
            if (request.PlotNumber > system.UnlockedPlots)
            {
                _logger.LogWarning(
                    "Plant crop rejected because plot is locked. PlayerId={PlayerId}, PlotNumber={PlotNumber}, UnlockedPlots={UnlockedPlots}",
                    playerId,
                    request.PlotNumber,
                    system.UnlockedPlots);
                return false;
            }

            var plot = await _plotRepository.GetFirstAsync(plot => plot.PlayerId == playerId && plot.PlotNumber == request.PlotNumber);
            if (plot == null)
            {
                _logger.LogWarning("Plant crop failed because plot does not exist. PlayerId={PlayerId}, PlotNumber={PlotNumber}", playerId, request.PlotNumber);
                return false;
            }

            if (plot.Status != PlotStatus.Empty)
            {
                _logger.LogWarning(
                    "Plant crop rejected because plot is not empty. PlayerId={PlayerId}, PlotNumber={PlotNumber}, Status={Status}",
                    playerId,
                    request.PlotNumber,
                    plot.Status);
                return false;
            }

            var crop = await _cropTemplateRepository.GetByIdAsync(request.CropTemplateId);
            if (crop == null)
            {
                _logger.LogWarning(
                    "Plant crop failed because crop template does not exist. PlayerId={PlayerId}, CropTemplateId={CropTemplateId}",
                    playerId,
                    request.CropTemplateId);
                return false;
            }

            if (crop.UnlockLevel > system.FieldLevel)
            {
                _logger.LogWarning(
                    "Plant crop rejected because field level is too low. PlayerId={PlayerId}, FieldLevel={FieldLevel}, RequiredLevel={RequiredLevel}, CropTemplateId={CropTemplateId}",
                    playerId,
                    system.FieldLevel,
                    crop.UnlockLevel,
                    crop.TemplateId);
                return false;
            }

            var now = DateTime.Now;

            try
            {
                // 中文注释：
                // 播种要把“扣种子、写地块、记统计”放在同一笔事务里，
                // 否则异常中断后很容易出现背包和地块状态不一致。
                _dbContext.BeginTransaction();

                var deducted = await DeductInventoryItemAsync(playerId, crop.SeedId, crop.SeedAmount);
                if (!deducted)
                {
                    _dbContext.RollbackTransaction();
                    _logger.LogWarning(
                        "Plant crop rejected because seeds are insufficient. PlayerId={PlayerId}, CropTemplateId={CropTemplateId}, SeedId={SeedId}, SeedAmount={SeedAmount}",
                        playerId,
                        crop.TemplateId,
                        crop.SeedId,
                        crop.SeedAmount);
                    return false;
                }

                plot.CropTemplateId = crop.TemplateId;
                plot.PlantTime = now;
                plot.ExpectedHarvestTime = now.AddSeconds(ApplyGrowthSpeedBonus(crop.GrowthCycle, system.GlobalGrowthSpeedBonus));
                plot.ActualHarvestTime = null;
                plot.SpeedUpCount = 0;
                plot.SpeedUpDuration = 0;
                plot.Status = PlotStatus.Growing;
                plot.LastUpdateTime = now;
                await _plotRepository.UpdateAsync(plot);

                system.TodayPlantCount += 1;
                system.TotalPlantCount += 1;
                system.LastUpdateTime = now;
                await _systemRepository.UpdateAsync(system);

                _dbContext.CommitTransaction();
                _logger.LogInformation(
                    "Player {PlayerId} planted crop {CropTemplateId} on plot {PlotNumber}. HarvestAt={HarvestAt}",
                    playerId,
                    crop.TemplateId,
                    request.PlotNumber,
                    plot.ExpectedHarvestTime);

                await _questService.RecordObjectiveEventAsync(playerId, new XXX.Quest.QuestObjectiveEvent
                {
                    ObjectiveType = XXX.Quest.ObjectiveType.PlantCrop,
                    TargetId = crop.TemplateId,
                    Delta = 1
                });
                await _achievementService.RecordRequirementEventAsync(playerId, new XXX.Achievement.AchievementRequirementEvent
                {
                    RequirementType = XXX.Achievement.AchievementRequirementType.PlantCount,
                    TargetId = crop.TemplateId,
                    Delta = 1
                });
                return true;
            }
            catch (Exception ex)
            {
                _dbContext.RollbackTransaction();
                _logger.LogError(
                    ex,
                    "Plant crop failed with exception. PlayerId={PlayerId}, PlotNumber={PlotNumber}, CropTemplateId={CropTemplateId}",
                    playerId,
                    request.PlotNumber,
                    request.CropTemplateId);
                return false;
            }
        }

        /// <summary>
        /// 收获指定地块作物。
        /// </summary>
        public async Task<HarvestResultDto?> HarvestCropAsync(string playerId, HarvestCropRequestDto request)
        {
            if (request.PlotNumber <= 0)
            {
                _logger.LogWarning("Harvest crop rejected due to invalid plot number. PlayerId={PlayerId}, PlotNumber={PlotNumber}", playerId, request.PlotNumber);
                return null;
            }

            var system = await PreparePlayerFieldAsync(playerId);
            var plot = await _plotRepository.GetFirstAsync(entity => entity.PlayerId == playerId && entity.PlotNumber == request.PlotNumber);
            if (plot == null)
            {
                _logger.LogWarning("Harvest crop failed because plot does not exist. PlayerId={PlayerId}, PlotNumber={PlotNumber}", playerId, request.PlotNumber);
                return null;
            }

            if (plot.Status != PlotStatus.Harvestable || string.IsNullOrWhiteSpace(plot.CropTemplateId))
            {
                _logger.LogWarning(
                    "Harvest crop rejected because plot is not harvestable. PlayerId={PlayerId}, PlotNumber={PlotNumber}, Status={Status}",
                    playerId,
                    request.PlotNumber,
                    plot.Status);
                return null;
            }

            var crop = await _cropTemplateRepository.GetByIdAsync(plot.CropTemplateId);
            if (crop == null)
            {
                _logger.LogWarning(
                    "Harvest crop failed because crop template is missing. PlayerId={PlayerId}, PlotNumber={PlotNumber}, CropTemplateId={CropTemplateId}",
                    playerId,
                    request.PlotNumber,
                    plot.CropTemplateId);
                return null;
            }

            var result = new HarvestResultDto
            {
                CropName = crop.Name,
                Quantity = CalculateHarvestQuantity(crop, plot, system),
                Quality = Random.Shared.Next(crop.MinQuality, crop.MaxQuality + 1)
            };

            var now = DateTime.Now;

            try
            {
                // 中文注释：
                // 收获同样走事务，保证不会发生“产物已到账但地块没清空”或反过来的脏状态。
                _dbContext.BeginTransaction();

                await InventoryItemGrantHelper.AddOrMergeAsync(_dbContext.Db, playerId, crop.OutputItemId, result.Quantity, "Inventory is full.");

                plot.Status = PlotStatus.Empty;
                plot.CropTemplateId = null;
                plot.PlantTime = null;
                plot.ExpectedHarvestTime = null;
                plot.ActualHarvestTime = now;
                plot.SpeedUpCount = 0;
                plot.SpeedUpDuration = 0;
                plot.LastUpdateTime = now;
                await _plotRepository.UpdateAsync(plot);

                system.TodayHarvestCount += 1;
                system.TotalHarvestCount += 1;
                system.LastUpdateTime = now;
                await _systemRepository.UpdateAsync(system);

                _dbContext.CommitTransaction();
                _logger.LogInformation(
                    "Player {PlayerId} harvested crop {CropTemplateId} from plot {PlotNumber}. OutputItemId={OutputItemId}, Quantity={Quantity}",
                    playerId,
                    crop.TemplateId,
                    request.PlotNumber,
                    crop.OutputItemId,
                    result.Quantity);

                await _questService.RecordObjectiveEventAsync(playerId, new XXX.Quest.QuestObjectiveEvent
                {
                    ObjectiveType = XXX.Quest.ObjectiveType.HarvestCrop,
                    TargetId = crop.TemplateId,
                    Delta = 1
                });
                await _questService.RecordObjectiveEventAsync(playerId, new XXX.Quest.QuestObjectiveEvent
                {
                    ObjectiveType = XXX.Quest.ObjectiveType.CollectItem,
                    TargetId = crop.OutputItemId,
                    Delta = result.Quantity
                });
                await _achievementService.RecordRequirementEventAsync(playerId, new XXX.Achievement.AchievementRequirementEvent
                {
                    RequirementType = XXX.Achievement.AchievementRequirementType.HarvestCount,
                    TargetId = crop.TemplateId,
                    Delta = 1
                });
                return result;
            }
            catch (Exception ex)
            {
                _dbContext.RollbackTransaction();
                _logger.LogError(ex, "Harvest crop failed. PlayerId={PlayerId}, PlotNumber={PlotNumber}", playerId, request.PlotNumber);
                return null;
            }
        }

        /// <summary>
        /// 对指定地块执行加速。
        /// </summary>
        public async Task<bool> SpeedUpCropAsync(string playerId, SpeedUpCropRequestDto request)
        {
            if (request.PlotNumber <= 0 || string.IsNullOrWhiteSpace(request.ItemId))
            {
                _logger.LogWarning(
                    "Speed up crop rejected due to invalid request. PlayerId={PlayerId}, PlotNumber={PlotNumber}, ItemId={ItemId}",
                    playerId,
                    request.PlotNumber,
                    request.ItemId);
                return false;
            }

            var normalizedItemId = request.ItemId.Trim();
            if (!SpiritFieldRuleRuntimeCatalog.TryGetSpeedUpSeconds(normalizedItemId, out var speedUpSeconds))
            {
                _logger.LogWarning(
                    "Speed up crop rejected because item is not allowed. PlayerId={PlayerId}, PlotNumber={PlotNumber}, ItemId={ItemId}",
                    playerId,
                    request.PlotNumber,
                    request.ItemId);
                return false;
            }

            await PreparePlayerFieldAsync(playerId);
            var plot = await _plotRepository.GetFirstAsync(entity => entity.PlayerId == playerId && entity.PlotNumber == request.PlotNumber);
            if (plot == null || plot.Status != PlotStatus.Growing)
            {
                _logger.LogWarning(
                    "Speed up crop rejected because plot is not growing. PlayerId={PlayerId}, PlotNumber={PlotNumber}, Status={Status}",
                    playerId,
                    request.PlotNumber,
                    plot?.Status);
                return false;
            }

            var now = DateTime.Now;

            try
            {
                _dbContext.BeginTransaction();

                var deducted = await DeductInventoryItemAsync(playerId, normalizedItemId, 1);
                if (!deducted)
                {
                    _dbContext.RollbackTransaction();
                    _logger.LogWarning(
                        "Speed up crop rejected because speed-up item is insufficient. PlayerId={PlayerId}, PlotNumber={PlotNumber}, ItemId={ItemId}",
                        playerId,
                        request.PlotNumber,
                        request.ItemId);
                    return false;
                }

                plot.SpeedUpCount += 1;
                plot.SpeedUpDuration += speedUpSeconds;
                if (plot.ExpectedHarvestTime.HasValue)
                {
                    plot.ExpectedHarvestTime = plot.ExpectedHarvestTime.Value.AddSeconds(-speedUpSeconds);
                    if (plot.ExpectedHarvestTime <= now)
                    {
                        plot.Status = PlotStatus.Harvestable;
                    }
                }

                plot.LastUpdateTime = now;
                await _plotRepository.UpdateAsync(plot);

                _dbContext.CommitTransaction();
                _logger.LogInformation(
                    "Player {PlayerId} speeded up plot {PlotNumber}. NewHarvestAt={HarvestAt}",
                    playerId,
                    request.PlotNumber,
                    plot.ExpectedHarvestTime);
                return true;
            }
            catch (Exception ex)
            {
                _dbContext.RollbackTransaction();
                _logger.LogError(ex, "Speed up crop failed. PlayerId={PlayerId}, PlotNumber={PlotNumber}", playerId, request.PlotNumber);
                return false;
            }
        }

        /// <summary>
        /// 升级指定地块。
        /// </summary>
        public async Task<bool> UpgradePlotAsync(string playerId, int plotNumber)
        {
            if (plotNumber <= 0)
            {
                _logger.LogWarning("Upgrade plot rejected due to invalid plot number. PlayerId={PlayerId}, PlotNumber={PlotNumber}", playerId, plotNumber);
                return false;
            }

            var system = await PreparePlayerFieldAsync(playerId);
            if (plotNumber > system.UnlockedPlots)
            {
                _logger.LogWarning(
                    "Upgrade plot rejected because plot is locked. PlayerId={PlayerId}, PlotNumber={PlotNumber}, UnlockedPlots={UnlockedPlots}",
                    playerId,
                    plotNumber,
                    system.UnlockedPlots);
                return false;
            }

            var plot = await _plotRepository.GetFirstAsync(entity => entity.PlayerId == playerId && entity.PlotNumber == plotNumber);
            if (plot == null)
            {
                _logger.LogWarning("Upgrade plot rejected because plot does not exist. PlayerId={PlayerId}, PlotNumber={PlotNumber}", playerId, plotNumber);
                return false;
            }

            var costGold = SpiritFieldRuleRuntimeCatalog.GetPlotUpgradeGoldCost(plot.Level);
            var costSpiritStone = SpiritFieldRuleRuntimeCatalog.GetPlotUpgradeSpiritStoneCost(plot.Level);
            var now = DateTime.Now;

            try
            {
                _dbContext.BeginTransaction();

                var userRows = await _dbContext.Db.Updateable<UserEntity>()
                    .SetColumns(user => user.Gold == user.Gold - costGold)
                    .SetColumns(user => user.SpiritStone == user.SpiritStone - costSpiritStone)
                    .SetColumns(user => user.TotalGoldSpent == user.TotalGoldSpent + costGold)
                    .SetColumns(user => user.LastUpdateTime == now)
                    .Where(user => user.GID == playerId &&
                                   !user.IsDeleted &&
                                   user.Gold >= costGold &&
                                   user.SpiritStone >= costSpiritStone)
                    .ExecuteCommandAsync();

                if (userRows == 0)
                {
                    _dbContext.RollbackTransaction();
                    _logger.LogWarning(
                        "Upgrade plot rejected because resources are insufficient. PlayerId={PlayerId}, PlotNumber={PlotNumber}, CostGold={CostGold}, CostSpiritStone={CostSpiritStone}",
                        playerId,
                        plotNumber,
                        costGold,
                        costSpiritStone);
                    return false;
                }

                var plotRows = await _dbContext.Db.Updateable<SpiritFieldPlotEntity>()
                    .SetColumns(entity => entity.Level == entity.Level + 1)
                    .SetColumns(entity => entity.YieldBonusPercent == entity.YieldBonusPercent + SpiritFieldRuleRuntimeCatalog.GetPlotUpgradeYieldBonusPerLevel())
                    .SetColumns(entity => entity.LastUpdateTime == now)
                    .Where(entity => entity.Id == plot.Id &&
                                     entity.PlayerId == playerId &&
                                     entity.PlotNumber == plotNumber &&
                                     entity.Level == plot.Level)
                    .ExecuteCommandAsync();

                if (plotRows == 0)
                {
                    _dbContext.RollbackTransaction();
                    _logger.LogWarning(
                        "Upgrade plot rejected because plot state changed concurrently. PlayerId={PlayerId}, PlotNumber={PlotNumber}",
                        playerId,
                        plotNumber);
                    return false;
                }

                _dbContext.CommitTransaction();
                _logger.LogInformation(
                    "Player {PlayerId} upgraded plot {PlotNumber} to level {Level}",
                    playerId,
                    plotNumber,
                    plot.Level + 1);
                return true;
            }
            catch (Exception ex)
            {
                _dbContext.RollbackTransaction();
                _logger.LogError(ex, "Upgrade plot failed. PlayerId={PlayerId}, PlotNumber={PlotNumber}", playerId, plotNumber);
                return false;
            }
        }

        private async Task<SpiritFieldSystemEntity> PreparePlayerFieldAsync(string playerId)
        {
            await EnsureCropTemplatesSeededAsync();

            var system = await EnsurePlayerSystemAsync(playerId);
            await ResetDailyCountersAsync(system);
            await EnsureUnlockedPlotsAsync(playerId, system);
            await MarkDuePlotsHarvestableAsync(playerId);
            await SyncArrayYieldBonusAsync(system);

            return system;
        }

        private async Task EnsureCropTemplatesSeededAsync()
        {
            var existingTemplateIds = (await _cropTemplateRepository.GetAllAsync())
                .Select(template => template.TemplateId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // 中文注释：
            // 作物模板由数据库基准数据管理，不能再依赖旧版固定 ID。
            // 纯净数据重建后作物编号可以变化，只要数据库中存在模板，灵田就可以正常运行。
            if (existingTemplateIds.Count > 0)
            {
                return;
            }

            _logger.LogError(
                "CropTemplates is empty. Runtime crop seeding has been disabled; database templates are required.");
            throw new InvalidOperationException("CropTemplates is empty. Please import database crop templates before using spirit field.");
        }

        private async Task<SpiritFieldSystemEntity> EnsurePlayerSystemAsync(string playerId)
        {
            var system = await _systemRepository.GetFirstAsync(entity => entity.PlayerId == playerId);
            if (system != null)
            {
                return await NormalizeSystemAsync(system);
            }

            return await CreateDefaultSystemAsync(playerId);
        }

        private async Task<SpiritFieldSystemEntity> NormalizeSystemAsync(SpiritFieldSystemEntity system)
        {
            var changed = false;
            var now = DateTime.Now;

            if (system.FieldLevel <= 0)
            {
                system.FieldLevel = 1;
                changed = true;
            }

            if (system.MaxPlots <= 0)
            {
                system.MaxPlots = SpiritFieldRuleRuntimeCatalog.GetDefaultMaxPlots();
                changed = true;
            }

            if (system.UnlockedPlots < SpiritFieldRuleRuntimeCatalog.GetDefaultUnlockedPlots())
            {
                system.UnlockedPlots = SpiritFieldRuleRuntimeCatalog.GetDefaultUnlockedPlots();
                changed = true;
            }

            if (system.UnlockedPlots > system.MaxPlots)
            {
                system.UnlockedPlots = system.MaxPlots;
                changed = true;
            }

            if (!changed)
            {
                return system;
            }

            system.LastUpdateTime = now;
            await _systemRepository.UpdateAsync(system);
            _logger.LogInformation("Normalized spirit field system data. PlayerId={PlayerId}", system.PlayerId);
            return system;
        }

        private async Task<SpiritFieldSystemEntity> CreateDefaultSystemAsync(string playerId)
        {
            var now = DateTime.Now;
            var system = new SpiritFieldSystemEntity
            {
                PlayerId = playerId,
                FieldLevel = SpiritFieldRuleRuntimeCatalog.GetDefaultFieldLevel(),
                UnlockedPlots = SpiritFieldRuleRuntimeCatalog.GetDefaultUnlockedPlots(),
                MaxPlots = SpiritFieldRuleRuntimeCatalog.GetDefaultMaxPlots(),
                DailyResetTime = now.Date,
                LastUpdateTime = now
            };

            try
            {
                _dbContext.BeginTransaction();
                await _systemRepository.AddAsync(system);
                await InsertMissingPlotsAsync(playerId, SpiritFieldRuleRuntimeCatalog.GetDefaultUnlockedPlots(), now);
                _dbContext.CommitTransaction();

                _logger.LogInformation("Created default spirit field system for player {PlayerId}", playerId);
                return system;
            }
            catch (Exception ex)
            {
                _dbContext.RollbackTransaction();
                _logger.LogError(ex, "Create default spirit field system failed. PlayerId={PlayerId}", playerId);

                var existingSystem = await _systemRepository.GetFirstAsync(entity => entity.PlayerId == playerId);
                if (existingSystem != null)
                {
                    return await NormalizeSystemAsync(existingSystem);
                }

                throw;
            }
        }

        private async Task EnsureUnlockedPlotsAsync(string playerId, SpiritFieldSystemEntity system)
        {
            var requiredPlotCount = Math.Max(SpiritFieldRuleRuntimeCatalog.GetDefaultUnlockedPlots(), Math.Min(system.UnlockedPlots, system.MaxPlots));
            var now = DateTime.Now;
            var existingPlotNumbers = (await _plotRepository.GetListAsync(plot => plot.PlayerId == playerId))
                .Select(plot => plot.PlotNumber)
                .ToHashSet();

            var missingPlots = new List<SpiritFieldPlotEntity>();
            for (var plotNumber = 1; plotNumber <= requiredPlotCount; plotNumber++)
            {
                if (existingPlotNumbers.Contains(plotNumber))
                {
                    continue;
                }

                missingPlots.Add(new SpiritFieldPlotEntity
                {
                    PlayerId = playerId,
                    PlotNumber = plotNumber,
                    Level = 1,
                    Status = PlotStatus.Empty,
                    LastUpdateTime = now
                });
            }

            if (missingPlots.Count == 0)
            {
                return;
            }

            await _plotRepository.AddRangeAsync(missingPlots);
            _logger.LogInformation("Backfilled {Count} missing spirit field plots for player {PlayerId}", missingPlots.Count, playerId);
        }

        private async Task InsertMissingPlotsAsync(string playerId, int plotCount, DateTime now)
        {
            var plots = Enumerable.Range(1, plotCount)
                .Select(plotNumber => new SpiritFieldPlotEntity
                {
                    PlayerId = playerId,
                    PlotNumber = plotNumber,
                    Level = 1,
                    Status = PlotStatus.Empty,
                    LastUpdateTime = now
                })
                .ToList();

            await _plotRepository.AddRangeAsync(plots);
        }

        private async Task ResetDailyCountersAsync(SpiritFieldSystemEntity system)
        {
            var today = DateTime.Today;
            if (system.DailyResetTime.Date >= today)
            {
                return;
            }

            system.TodayPlantCount = 0;
            system.TodayHarvestCount = 0;
            system.DailyResetTime = today;
            system.LastUpdateTime = DateTime.Now;

            await _systemRepository.UpdateAsync(system);
            _logger.LogInformation("Reset spirit field daily counters. PlayerId={PlayerId}", system.PlayerId);
        }

        private async Task MarkDuePlotsHarvestableAsync(string playerId)
        {
            var now = DateTime.Now;
            var duePlots = await _plotRepository.Db.Queryable<SpiritFieldPlotEntity>()
                .Where(plot => plot.PlayerId == playerId &&
                               plot.Status == PlotStatus.Growing &&
                               plot.ExpectedHarvestTime != null &&
                               plot.ExpectedHarvestTime <= now)
                .ToListAsync();

            if (duePlots.Count == 0)
            {
                return;
            }

            foreach (var plot in duePlots)
            {
                plot.Status = PlotStatus.Harvestable;
                plot.LastUpdateTime = now;
            }

            await _plotRepository.UpdateRangeAsync(duePlots);
            _logger.LogInformation("Promoted {Count} mature spirit field plots to harvestable. PlayerId={PlayerId}", duePlots.Count, playerId);
        }

        private async Task<Dictionary<string, string>> LoadCropNameMapAsync(IEnumerable<SpiritFieldPlotEntity> plots)
        {
            var cropIds = plots
                .Where(plot => !string.IsNullOrWhiteSpace(plot.CropTemplateId))
                .Select(plot => plot.CropTemplateId!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (cropIds.Count == 0)
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            var cropTemplates = await _cropTemplateRepository.Db.Queryable<CropTemplateEntity>()
                .Where(template => cropIds.Contains(template.TemplateId))
                .ToListAsync();

            return cropTemplates.ToDictionary(
                template => template.TemplateId,
                template => template.Name,
                StringComparer.OrdinalIgnoreCase);
        }

        private PlotDto MapPlotToDto(SpiritFieldPlotEntity entity, IReadOnlyDictionary<string, string> cropNameMap)
        {
            var now = DateTime.Now;
            var progress = 0;
            var canHarvest = entity.Status == PlotStatus.Harvestable;

            if (entity.PlantTime.HasValue && entity.ExpectedHarvestTime.HasValue)
            {
                var totalSeconds = Math.Max(1d, (entity.ExpectedHarvestTime.Value - entity.PlantTime.Value).TotalSeconds);
                var elapsedSeconds = Math.Max(0d, (now - entity.PlantTime.Value).TotalSeconds);
                progress = (int)Math.Min(100, Math.Floor(elapsedSeconds / totalSeconds * 100));
                canHarvest = canHarvest || now >= entity.ExpectedHarvestTime.Value;
            }

            return new PlotDto
            {
                Id = entity.Id,
                PlotNumber = entity.PlotNumber,
                Level = entity.Level,
                Status = entity.Status.ToString(),
                CropTemplateId = entity.CropTemplateId,
                CropName = !string.IsNullOrWhiteSpace(entity.CropTemplateId) &&
                           cropNameMap.TryGetValue(entity.CropTemplateId, out var cropName)
                    ? cropName
                    : null,
                PlantTime = entity.PlantTime,
                ExpectedHarvestTime = entity.ExpectedHarvestTime,
                ProgressPercent = progress,
                CanHarvest = canHarvest
            };
        }

        private static int ApplyGrowthSpeedBonus(int growthCycleSeconds, int growthSpeedBonusPercent)
        {
            var safeGrowthCycle = Math.Max(1, growthCycleSeconds);
            var multiplier = 1d + Math.Max(0, growthSpeedBonusPercent) / 100d;
            return Math.Max(1, (int)Math.Ceiling(safeGrowthCycle / multiplier));
        }

        private static int CalculateHarvestQuantity(
            CropTemplateEntity crop,
            SpiritFieldPlotEntity plot,
            SpiritFieldSystemEntity system)
        {
            var baseQuantity = Math.Max(1, crop.OutputAmount > 0 ? crop.OutputAmount : crop.Yield);
            var totalBonusPercent = Math.Max(0, plot.YieldBonusPercent + system.GlobalYieldBonus);
            return Math.Max(1, (int)Math.Floor(baseQuantity * (1d + totalBonusPercent / 100d)));
        }

        private async Task SyncArrayYieldBonusAsync(SpiritFieldSystemEntity system)
        {
            var arrayLevel = await GetArrayLevelAsync(system.PlayerId);
            var expectedBonus = FiveElementProgressionRules.GetSpiritFieldYieldBonusPercent(arrayLevel);
            if (system.GlobalYieldBonus == expectedBonus)
            {
                return;
            }

            system.GlobalYieldBonus = expectedBonus;
            system.LastUpdateTime = DateTime.Now;
            await _systemRepository.UpdateAsync(system);
        }

        private async Task<int> GetArrayLevelAsync(string playerId)
        {
            var array = await _fiveElementRepository.GetFirstAsync(entity => entity.PlayerId == playerId);
            return array == null
                ? 1
                : Math.Clamp(array.ArrayLevel <= 0 ? 1 : array.ArrayLevel, 1, 50);
        }

        private async Task<bool> DeductInventoryItemAsync(string playerId, string itemId, int quantity)
        {
            if (quantity <= 0)
            {
                return false;
            }

            var items = await _dbContext.Db.Queryable<InventoryItemEntity>()
                .Where(item => item.PlayerId == playerId && item.ItemId == itemId)
                .OrderBy(item => item.IsLocked)
                .OrderBy(item => item.Id)
                .ToListAsync();

            if (items.Sum(item => item.Quantity) < quantity)
            {
                return false;
            }

            var remaining = quantity;
            foreach (var item in items)
            {
                if (remaining <= 0)
                {
                    break;
                }

                var deductCount = Math.Min(item.Quantity, remaining);
                remaining -= deductCount;

                if (deductCount == item.Quantity)
                {
                    var deleteRows = await _dbContext.Db.Deleteable<InventoryItemEntity>()
                        .Where(entity => entity.Id == item.Id && entity.Quantity == item.Quantity)
                        .ExecuteCommandAsync();

                    if (deleteRows == 0)
                    {
                        throw new InvalidOperationException("Inventory item changed while deducting.");
                    }

                    continue;
                }

                var updateRows = await _dbContext.Db.Updateable<InventoryItemEntity>()
                    .SetColumns(entity => entity.Quantity == entity.Quantity - deductCount)
                    .Where(entity => entity.Id == item.Id && entity.Quantity >= deductCount)
                    .ExecuteCommandAsync();

                if (updateRows == 0)
                {
                    throw new InvalidOperationException("Inventory item changed while deducting.");
                }
            }

            if (remaining > 0)
            {
                throw new InvalidOperationException("Inventory deduction did not finish.");
            }

            return true;
        }

        private static string ResolveItemDisplayName(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return string.Empty;
            }

            return GameData.Items.TryGetValue(itemId, out var item)
                ? item.Name
                : itemId;
        }
    }
}
