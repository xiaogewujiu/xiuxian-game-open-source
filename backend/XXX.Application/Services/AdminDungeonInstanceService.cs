#pragma warning disable CS1591
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    public class AdminDungeonInstanceService : IAdminDungeonInstanceService
    {
        private readonly IRepository<DungeonInstanceTemplateEntity> _templateRepository;
        private readonly IRepository<DungeonEventConfigEntity> _eventRepository;
        private readonly IRepository<DungeonEventGroupEntity> _groupRepository;
        private readonly IRepository<DungeonInstanceEntity> _instanceRepository;
        private readonly IAdminRuntimeRefreshService _runtimeRefreshService;

        public AdminDungeonInstanceService(
            IRepository<DungeonInstanceTemplateEntity> templateRepository,
            IRepository<DungeonEventConfigEntity> eventRepository,
            IRepository<DungeonEventGroupEntity> groupRepository,
            IRepository<DungeonInstanceEntity> instanceRepository,
            IAdminRuntimeRefreshService runtimeRefreshService)
        {
            _templateRepository = templateRepository;
            _eventRepository = eventRepository;
            _groupRepository = groupRepository;
            _instanceRepository = instanceRepository;
            _runtimeRefreshService = runtimeRefreshService;
        }

        public async Task<List<AdminDungeonInstanceTemplateListItemDto>> GetTemplateListAsync(string? keyword = null)
        {
            var normalizedKeyword = (keyword ?? string.Empty).Trim();
            var query = _templateRepository.Db.Queryable<DungeonInstanceTemplateEntity>();

            if (!string.IsNullOrWhiteSpace(normalizedKeyword))
            {
                query = query.Where(t => t.Id.Contains(normalizedKeyword) || t.Name.Contains(normalizedKeyword));
            }

            var templates = await query.OrderBy(t => t.RecommendedLevel).OrderBy(t => t.Id).ToListAsync();
            return templates.Select(t => new AdminDungeonInstanceTemplateListItemDto
            {
                Id = t.Id,
                Name = t.Name,
                Enabled = t.Enabled,
                RecommendedLevel = t.RecommendedLevel,
                DailyEnterLimit = t.DailyEnterLimit,
                TickIntervalSeconds = t.TickIntervalSeconds,
                IsBuiltIn = t.IsBuiltIn,
                BuiltInVersion = t.BuiltInVersion,
                EventGroupId = t.EventGroupId
            }).ToList();
        }

        public async Task<AdminDungeonInstanceTemplateDetailDto?> GetTemplateDetailAsync(string dungeonId)
        {
            if (string.IsNullOrWhiteSpace(dungeonId))
            {
                return null;
            }

            var template = await _templateRepository.GetByIdAsync(dungeonId.Trim());
            if (template == null)
            {
                return null;
            }

            return new AdminDungeonInstanceTemplateDetailDto
            {
                Id = template.Id,
                Name = template.Name,
                Description = template.Description,
                Enabled = template.Enabled,
                RecommendedLevel = template.RecommendedLevel,
                DailyEnterLimit = template.DailyEnterLimit,
                TickIntervalSeconds = template.TickIntervalSeconds,
                OpenScheduleJson = template.OpenScheduleJson,
                EntryCostsJson = template.EntryCostsJson,
                EventGroupId = template.EventGroupId,
                AutoMedicineConfigJson = template.AutoMedicineConfigJson,
                EncounterConfigJson = template.EncounterConfigJson,
                IsBuiltIn = template.IsBuiltIn,
                SeedKey = template.SeedKey,
                BuiltInVersion = template.BuiltInVersion,
                LastUpdateTime = template.LastUpdateTime
            };
        }

        public async Task<AdminDungeonInstanceTemplateDetailDto> SaveTemplateAsync(AdminDungeonInstanceTemplateDetailDto request)
        {
            var id = (request.Id ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new InvalidOperationException("秘境模板 ID 不能为空。");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new InvalidOperationException("秘境模板名称不能为空。");
            }

            if (request.RecommendedLevel < 1)
            {
                throw new InvalidOperationException("推荐等级不能小于 1。");
            }

            if (request.TickIntervalSeconds < 5)
            {
                throw new InvalidOperationException("tick 间隔不能小于 5 秒。");
            }

            var existing = await _templateRepository.GetByIdAsync(id);
            if (existing == null)
            {
                existing = new DungeonInstanceTemplateEntity { Id = id };
                await _templateRepository.AddAsync(ApplyTemplate(existing, request));
                await _runtimeRefreshService.ReloadRuntimeTemplatesAsync();
                return (await GetTemplateDetailAsync(id))!;
            }

            ApplyTemplate(existing, request);
            await _templateRepository.UpdateAsync(existing);
            await _runtimeRefreshService.ReloadRuntimeTemplatesAsync();
            return (await GetTemplateDetailAsync(id))!;
        }

        public async Task<bool> DeleteTemplateAsync(string dungeonId)
        {
            var normalizedId = (dungeonId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedId))
            {
                return false;
            }

            var hasRunningInstances = await _instanceRepository.Db.Queryable<DungeonInstanceEntity>()
                .Where(i => i.DungeonId == normalizedId && i.Status == (int)DungeonInstanceStatus.Running)
                .AnyAsync();
            if (hasRunningInstances)
            {
                throw new InvalidOperationException("当前秘境仍有运行中的实例，不能删除。");
            }

            var deleteRows = await _templateRepository.DeleteAsync(normalizedId);
            if (deleteRows > 0)
            {
                await _runtimeRefreshService.ReloadRuntimeTemplatesAsync();
            }

            return deleteRows > 0;
        }

        public async Task<List<AdminDungeonEventConfigListItemDto>> GetEventListAsync(
            string? dungeonId = null,
            int? eventType = null,
            string? keyword = null)
        {
            var query = _eventRepository.Db.Queryable<DungeonEventConfigEntity>();

            if (!string.IsNullOrWhiteSpace(dungeonId))
            {
                var normalizedDungeonId = dungeonId.Trim();
                query = query.Where(e => e.DungeonId == normalizedDungeonId || e.DungeonId == "*");
            }

            if (eventType.HasValue)
            {
                query = query.Where(e => e.EventType == eventType.Value);
            }

            var normalizedKeyword = (keyword ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(normalizedKeyword))
            {
                query = query.Where(e => e.Id.Contains(normalizedKeyword) || e.Name.Contains(normalizedKeyword));
            }

            var events = await query.OrderBy(e => e.EventType).OrderBy(e => e.Id).ToListAsync();
            return events.Select(e => new AdminDungeonEventConfigListItemDto
            {
                Id = e.Id,
                Name = e.Name,
                DungeonId = e.DungeonId,
                EventType = e.EventType,
                Weight = e.Weight,
                Enabled = e.Enabled,
                IsBuiltIn = e.IsBuiltIn,
                BuiltInVersion = e.BuiltInVersion
            }).ToList();
        }

        public async Task<AdminDungeonEventConfigDetailDto?> GetEventDetailAsync(string eventId)
        {
            if (string.IsNullOrWhiteSpace(eventId))
            {
                return null;
            }

            var entity = await _eventRepository.GetByIdAsync(eventId.Trim());
            if (entity == null)
            {
                return null;
            }

            return new AdminDungeonEventConfigDetailDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                DungeonId = entity.DungeonId,
                EventType = entity.EventType,
                Weight = entity.Weight,
                Enabled = entity.Enabled,
                EventDataJson = entity.EventDataJson,
                DeathKeep = entity.DeathKeep,
                IsBuiltIn = entity.IsBuiltIn,
                SeedKey = entity.SeedKey,
                BuiltInVersion = entity.BuiltInVersion,
                LastUpdateTime = entity.LastUpdateTime
            };
        }

        public async Task<AdminDungeonEventConfigDetailDto> SaveEventAsync(AdminDungeonEventConfigDetailDto request)
        {
            var id = (request.Id ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new InvalidOperationException("事件 ID 不能为空。");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new InvalidOperationException("事件名称不能为空。");
            }

            if (request.EventType < 1 || request.EventType > 11)
            {
                throw new InvalidOperationException("事件类型无效。");
            }

            if (request.Weight < 0)
            {
                throw new InvalidOperationException("权重不能为负数。");
            }

            var dungeonId = (request.DungeonId ?? "*").Trim();
            if (string.IsNullOrWhiteSpace(dungeonId))
            {
                dungeonId = "*";
            }

            var existing = await _eventRepository.GetByIdAsync(id);
            if (existing == null)
            {
                existing = new DungeonEventConfigEntity { Id = id };
                await _eventRepository.AddAsync(ApplyEvent(existing, request, dungeonId));
                await _runtimeRefreshService.ReloadRuntimeTemplatesAsync();
                return (await GetEventDetailAsync(id))!;
            }

            ApplyEvent(existing, request, dungeonId);
            await _eventRepository.UpdateAsync(existing);
            await _runtimeRefreshService.ReloadRuntimeTemplatesAsync();
            return (await GetEventDetailAsync(id))!;
        }

        public async Task<bool> DeleteEventAsync(string eventId)
        {
            var normalizedId = (eventId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedId))
            {
                return false;
            }

            var deleteRows = await _eventRepository.DeleteAsync(normalizedId);
            if (deleteRows > 0)
            {
                await _runtimeRefreshService.ReloadRuntimeTemplatesAsync();
            }

            return deleteRows > 0;
        }

        public async Task<bool> ToggleEventAsync(string eventId, bool enabled)
        {
            var normalizedId = (eventId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedId))
            {
                return false;
            }

            var entity = await _eventRepository.GetByIdAsync(normalizedId);
            if (entity == null)
            {
                return false;
            }

            entity.Enabled = enabled;
            entity.LastUpdateTime = DateTime.Now;
            await _eventRepository.UpdateAsync(entity);
            await _runtimeRefreshService.ReloadRuntimeTemplatesAsync();
            return true;
        }

        public async Task ReloadRuntimeAsync()
        {
            await _runtimeRefreshService.ReloadRuntimeTemplatesAsync();
        }

        public async Task<List<AdminDungeonEventGroupListItemDto>> GetGroupListAsync(string? keyword = null)
        {
            var normalizedKeyword = (keyword ?? string.Empty).Trim();
            var query = _groupRepository.Db.Queryable<DungeonEventGroupEntity>();

            if (!string.IsNullOrWhiteSpace(normalizedKeyword))
            {
                query = query.Where(g => g.Id.Contains(normalizedKeyword) || g.Name.Contains(normalizedKeyword));
            }

            var groups = await query.OrderBy(g => g.Id).ToListAsync();
            return groups.Select(g => new AdminDungeonEventGroupListItemDto
            {
                Id = g.Id,
                Name = g.Name,
                EventCount = ParseGroupItemCount(g.GroupItemsJson),
                IsBuiltIn = g.IsBuiltIn,
                BuiltInVersion = g.BuiltInVersion
            }).ToList();
        }

        public async Task<AdminDungeonEventGroupDetailDto?> GetGroupDetailAsync(string groupId)
        {
            if (string.IsNullOrWhiteSpace(groupId))
            {
                return null;
            }

            var entity = await _groupRepository.GetByIdAsync(groupId.Trim());
            if (entity == null)
            {
                return null;
            }

            return new AdminDungeonEventGroupDetailDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                GroupItemsJson = entity.GroupItemsJson,
                IsBuiltIn = entity.IsBuiltIn,
                SeedKey = entity.SeedKey,
                BuiltInVersion = entity.BuiltInVersion,
                LastUpdateTime = entity.LastUpdateTime
            };
        }

        public async Task<AdminDungeonEventGroupDetailDto> SaveGroupAsync(AdminDungeonEventGroupDetailDto request)
        {
            var id = (request.Id ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new InvalidOperationException("事件组 ID 不能为空。");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new InvalidOperationException("事件组名称不能为空。");
            }

            var existing = await _groupRepository.GetByIdAsync(id);
            if (existing == null)
            {
                existing = new DungeonEventGroupEntity { Id = id };
                await _groupRepository.AddAsync(ApplyGroup(existing, request));
                await _runtimeRefreshService.ReloadRuntimeTemplatesAsync();
                return (await GetGroupDetailAsync(id))!;
            }

            ApplyGroup(existing, request);
            await _groupRepository.UpdateAsync(existing);
            await _runtimeRefreshService.ReloadRuntimeTemplatesAsync();
            return (await GetGroupDetailAsync(id))!;
        }

        public async Task<bool> DeleteGroupAsync(string groupId)
        {
            var normalizedId = (groupId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedId))
            {
                return false;
            }

            var isReferenced = await _templateRepository.Db.Queryable<DungeonInstanceTemplateEntity>()
                .Where(t => t.EventGroupId == normalizedId)
                .AnyAsync();
            if (isReferenced)
            {
                throw new InvalidOperationException("该事件组仍被秘境模板引用，不能删除。");
            }

            var deleteRows = await _groupRepository.DeleteAsync(normalizedId);
            if (deleteRows > 0)
            {
                await _runtimeRefreshService.ReloadRuntimeTemplatesAsync();
            }

            return deleteRows > 0;
        }

        private static int ParseGroupItemCount(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return 0;
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                return doc.RootElement.GetArrayLength();
            }
            catch
            {
                return 0;
            }
        }

        private static DungeonEventGroupEntity ApplyGroup(
            DungeonEventGroupEntity entity,
            AdminDungeonEventGroupDetailDto request)
        {
            entity.Name = request.Name.Trim();
            entity.Description = (request.Description ?? string.Empty).Trim();
            entity.GroupItemsJson = request.GroupItemsJson;
            entity.SeedKey = null;
            entity.IsBuiltIn = false;
            entity.BuiltInVersion = null;
            entity.LastUpdateTime = DateTime.Now;
            return entity;
        }

        private static DungeonInstanceTemplateEntity ApplyTemplate(
            DungeonInstanceTemplateEntity entity,
            AdminDungeonInstanceTemplateDetailDto request)
        {
            entity.Name = request.Name.Trim();
            entity.Description = (request.Description ?? string.Empty).Trim();
            entity.Enabled = request.Enabled;
            entity.RecommendedLevel = Math.Max(1, request.RecommendedLevel);
            entity.DailyEnterLimit = Math.Max(1, request.DailyEnterLimit);
            entity.TickIntervalSeconds = Math.Max(5, request.TickIntervalSeconds);
            entity.OpenScheduleJson = request.OpenScheduleJson;
            entity.EntryCostsJson = request.EntryCostsJson;
            entity.EventGroupId = request.EventGroupId;
            entity.AutoMedicineConfigJson = request.AutoMedicineConfigJson;
            entity.EncounterConfigJson = request.EncounterConfigJson;
            entity.SeedKey = null;
            entity.IsBuiltIn = false;
            entity.BuiltInVersion = null;
            entity.LastUpdateTime = DateTime.Now;
            return entity;
        }

        private static DungeonEventConfigEntity ApplyEvent(
            DungeonEventConfigEntity entity,
            AdminDungeonEventConfigDetailDto request,
            string dungeonId)
        {
            entity.Name = request.Name.Trim();
            entity.Description = (request.Description ?? string.Empty).Trim();
            entity.DungeonId = dungeonId;
            entity.EventType = request.EventType;
            entity.Weight = Math.Max(0, request.Weight);
            entity.Enabled = request.Enabled;
            entity.EventDataJson = request.EventDataJson;
            entity.DeathKeep = request.DeathKeep;
            entity.SeedKey = null;
            entity.IsBuiltIn = false;
            entity.BuiltInVersion = null;
            entity.LastUpdateTime = DateTime.Now;
            return entity;
        }
    }
}
#pragma warning restore CS1591
