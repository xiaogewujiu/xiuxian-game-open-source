using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    /// <summary>
    /// 管理后台灵田系统服务。
    /// </summary>
    public class AdminSpiritFieldService : IAdminSpiritFieldService
    {
        private readonly IRepository<UserEntity> _userRepository;
        private readonly IRepository<SpiritFieldSystemEntity> _systemRepository;
        private readonly IRepository<SpiritFieldPlotEntity> _plotRepository;
        private readonly IRepository<CropTemplateEntity> _cropRepository;

        /// <summary>
        /// 初始化灵田系统服务。
        /// </summary>
        public AdminSpiritFieldService(
            IRepository<UserEntity> userRepository,
            IRepository<SpiritFieldSystemEntity> systemRepository,
            IRepository<SpiritFieldPlotEntity> plotRepository,
            IRepository<CropTemplateEntity> cropRepository)
        {
            _userRepository = userRepository;
            _systemRepository = systemRepository;
            _plotRepository = plotRepository;
            _cropRepository = cropRepository;
        }

        /// <summary>
        /// 获取灵田系统列表。
        /// </summary>
        public async Task<List<AdminSpiritFieldListItemDto>> GetListAsync(string? keyword = null)
        {
            var normalizedKeyword = (keyword ?? string.Empty).Trim();
            var query = _userRepository.Db.Queryable<UserEntity>()
                .Where(user => !user.IsDeleted);

            if (!string.IsNullOrWhiteSpace(normalizedKeyword))
            {
                query = query.Where(user =>
                    user.GID.Contains(normalizedKeyword) ||
                    user.Name.Contains(normalizedKeyword) ||
                    user.Account.Contains(normalizedKeyword));
            }

            var users = await query.OrderByDescending(user => user.Level).ToListAsync();
            if (users.Count == 0)
            {
                return [];
            }

            var playerIds = users.Select(user => user.GID).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var systems = await _systemRepository.Db.Queryable<SpiritFieldSystemEntity>()
                .Where(entity => playerIds.Contains(entity.PlayerId))
                .ToListAsync();
            var systemMap = systems.ToDictionary(entity => entity.PlayerId, entity => entity, StringComparer.OrdinalIgnoreCase);

            return users.Select(user => MapListItem(user, systemMap.TryGetValue(user.GID, out var system) ? system : null)).ToList();
        }

        /// <summary>
        /// 获取灵田系统详情。
        /// </summary>
        public async Task<AdminSpiritFieldDetailDto?> GetDetailAsync(string playerId)
        {
            if (string.IsNullOrWhiteSpace(playerId))
            {
                return null;
            }

            var normalizedPlayerId = playerId.Trim();
            var user = await _userRepository.GetByIdAsync(normalizedPlayerId);
            if (user == null || user.IsDeleted)
            {
                return null;
            }

            var system = await _systemRepository.GetFirstAsync(entity => entity.PlayerId == normalizedPlayerId);
            var plots = await _plotRepository.Db.Queryable<SpiritFieldPlotEntity>()
                .Where(entity => entity.PlayerId == normalizedPlayerId)
                .OrderBy(entity => entity.PlotNumber)
                .ToListAsync();
            var cropNames = await LoadCropNameMapAsync(plots);
            return MapDetail(user, system, plots, cropNames);
        }

        /// <summary>
        /// 保存灵田系统数据。
        /// </summary>
        public async Task<AdminSpiritFieldDetailDto> SaveAsync(AdminSpiritFieldDetailDto request)
        {
            if (string.IsNullOrWhiteSpace(request.PlayerId))
            {
                throw new InvalidOperationException("玩家编号不能为空。");
            }

            var normalizedPlayerId = request.PlayerId.Trim();
            var user = await _userRepository.GetByIdAsync(normalizedPlayerId);
            if (user == null || user.IsDeleted)
            {
                throw new InvalidOperationException("玩家不存在。");
            }

            var now = DateTime.Now;
            var system = await _systemRepository.GetFirstAsync(entity => entity.PlayerId == normalizedPlayerId);
            if (system == null)
            {
                system = new SpiritFieldSystemEntity
                {
                    PlayerId = normalizedPlayerId,
                    DailyResetTime = now.Date
                };
                ApplySystemValues(system, request, now);
                await _systemRepository.AddAsync(system);
            }
            else
            {
                ApplySystemValues(system, request, now);
                await _systemRepository.UpdateAsync(system);
            }

            var existingPlots = await _plotRepository.Db.Queryable<SpiritFieldPlotEntity>()
                .Where(entity => entity.PlayerId == normalizedPlayerId)
                .ToListAsync();
            var plotMap = existingPlots.ToDictionary(entity => entity.PlotNumber, entity => entity);
            var plotsToInsert = new List<SpiritFieldPlotEntity>();
            var plotsToUpdate = new List<SpiritFieldPlotEntity>();

            foreach (var plot in (request.Plots ?? []).Where(item => item.PlotNumber > 0).GroupBy(item => item.PlotNumber).Select(group => group.Last()))
            {
                if (plot.PlotNumber > system.MaxPlots)
                {
                    continue;
                }

                if (plotMap.TryGetValue(plot.PlotNumber, out var existing))
                {
                    existing.Level = Math.Max(1, plot.Level);
                    existing.YieldBonusPercent = Math.Max(0, plot.YieldBonusPercent);
                    existing.LastUpdateTime = now;
                    plotsToUpdate.Add(existing);
                }
                else
                {
                    plotsToInsert.Add(new SpiritFieldPlotEntity
                    {
                        PlayerId = normalizedPlayerId,
                        PlotNumber = plot.PlotNumber,
                        Level = Math.Max(1, plot.Level),
                        Status = PlotStatus.Empty,
                        YieldBonusPercent = Math.Max(0, plot.YieldBonusPercent),
                        LastUpdateTime = now
                    });
                }
            }

            if (plotsToInsert.Count > 0)
            {
                await _plotRepository.AddRangeAsync(plotsToInsert);
            }

            if (plotsToUpdate.Count > 0)
            {
                await _plotRepository.UpdateRangeAsync(plotsToUpdate);
            }

            return await GetDetailAsync(normalizedPlayerId) ?? throw new InvalidOperationException("玩家不存在。");
        }

        private static void ApplySystemValues(SpiritFieldSystemEntity system, AdminSpiritFieldDetailDto request, DateTime now)
        {
            system.FieldLevel = Math.Max(SpiritFieldRuleRuntimeCatalog.GetDefaultFieldLevel(), request.FieldLevel);
            system.MaxPlots = Math.Max(1, request.MaxPlots);
            system.UnlockedPlots = Math.Clamp(request.UnlockedPlots, 1, system.MaxPlots);
            system.GlobalGrowthSpeedBonus = Math.Max(0, request.GlobalGrowthSpeedBonus);
            system.LastUpdateTime = now;
        }

        private async Task<Dictionary<string, string>> LoadCropNameMapAsync(IEnumerable<SpiritFieldPlotEntity> plots)
        {
            var templateIds = plots
                .Select(plot => plot.CropTemplateId)
                .Where(templateId => !string.IsNullOrWhiteSpace(templateId))
                .Select(templateId => templateId!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (templateIds.Count == 0)
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            var crops = await _cropRepository.Db.Queryable<CropTemplateEntity>()
                .Where(crop => templateIds.Contains(crop.TemplateId))
                .ToListAsync();
            return crops.ToDictionary(crop => crop.TemplateId, crop => crop.Name, StringComparer.OrdinalIgnoreCase);
        }

        private static AdminSpiritFieldListItemDto MapListItem(UserEntity user, SpiritFieldSystemEntity? system)
        {
            return new AdminSpiritFieldListItemDto
            {
                PlayerId = user.GID,
                Name = user.Name,
                Account = user.Account,
                FieldLevel = Math.Max(SpiritFieldRuleRuntimeCatalog.GetDefaultFieldLevel(), system?.FieldLevel ?? SpiritFieldRuleRuntimeCatalog.GetDefaultFieldLevel()),
                UnlockedPlots = Math.Max(SpiritFieldRuleRuntimeCatalog.GetDefaultUnlockedPlots(), system?.UnlockedPlots ?? SpiritFieldRuleRuntimeCatalog.GetDefaultUnlockedPlots()),
                MaxPlots = Math.Max(SpiritFieldRuleRuntimeCatalog.GetDefaultMaxPlots(), system?.MaxPlots ?? SpiritFieldRuleRuntimeCatalog.GetDefaultMaxPlots()),
                GlobalYieldBonus = Math.Max(0, system?.GlobalYieldBonus ?? 0),
                GlobalGrowthSpeedBonus = Math.Max(0, system?.GlobalGrowthSpeedBonus ?? 0),
                TotalPlantCount = Math.Max(0, system?.TotalPlantCount ?? 0),
                TotalHarvestCount = Math.Max(0, system?.TotalHarvestCount ?? 0)
            };
        }

        private static AdminSpiritFieldDetailDto MapDetail(
            UserEntity user,
            SpiritFieldSystemEntity? system,
            IReadOnlyCollection<SpiritFieldPlotEntity> plots,
            IReadOnlyDictionary<string, string> cropNames)
        {
            var safeMaxPlots = Math.Max(SpiritFieldRuleRuntimeCatalog.GetDefaultMaxPlots(), system?.MaxPlots ?? SpiritFieldRuleRuntimeCatalog.GetDefaultMaxPlots());
            var safeUnlockedPlots = Math.Max(SpiritFieldRuleRuntimeCatalog.GetDefaultUnlockedPlots(), system?.UnlockedPlots ?? SpiritFieldRuleRuntimeCatalog.GetDefaultUnlockedPlots());
            var plotMap = plots.ToDictionary(plot => plot.PlotNumber, plot => plot);
            var detailPlots = new List<AdminSpiritFieldPlotDto>(safeMaxPlots);

            for (var plotNumber = 1; plotNumber <= safeMaxPlots; plotNumber++)
            {
                if (!plotMap.TryGetValue(plotNumber, out var plot))
                {
                    detailPlots.Add(new AdminSpiritFieldPlotDto
                    {
                        PlotNumber = plotNumber,
                        Level = 1,
                        Status = plotNumber <= safeUnlockedPlots
                            ? PlotStatus.Empty.ToString()
                            : "Locked",
                        YieldBonusPercent = 0
                    });
                    continue;
                }

                detailPlots.Add(new AdminSpiritFieldPlotDto
                {
                    Id = plot.Id,
                    PlotNumber = plot.PlotNumber,
                    Level = plot.Level,
                    Status = plot.Status.ToString(),
                    CropTemplateId = plot.CropTemplateId,
                    CropName = !string.IsNullOrWhiteSpace(plot.CropTemplateId) && cropNames.TryGetValue(plot.CropTemplateId, out var cropName)
                        ? cropName
                        : null,
                    PlantTime = plot.PlantTime,
                    ExpectedHarvestTime = plot.ExpectedHarvestTime,
                    SpeedUpCount = plot.SpeedUpCount,
                    YieldBonusPercent = plot.YieldBonusPercent
                });
            }

            return new AdminSpiritFieldDetailDto
            {
                PlayerId = user.GID,
                Name = user.Name,
                Account = user.Account,
                FieldLevel = Math.Max(SpiritFieldRuleRuntimeCatalog.GetDefaultFieldLevel(), system?.FieldLevel ?? SpiritFieldRuleRuntimeCatalog.GetDefaultFieldLevel()),
                UnlockedPlots = safeUnlockedPlots,
                MaxPlots = safeMaxPlots,
                GlobalYieldBonus = Math.Max(0, system?.GlobalYieldBonus ?? 0),
                GlobalGrowthSpeedBonus = Math.Max(0, system?.GlobalGrowthSpeedBonus ?? 0),
                TodayPlantCount = Math.Max(0, system?.TodayPlantCount ?? 0),
                TodayHarvestCount = Math.Max(0, system?.TodayHarvestCount ?? 0),
                TotalPlantCount = Math.Max(0, system?.TotalPlantCount ?? 0),
                TotalHarvestCount = Math.Max(0, system?.TotalHarvestCount ?? 0),
                Plots = detailPlots
            };
        }
    }
}
