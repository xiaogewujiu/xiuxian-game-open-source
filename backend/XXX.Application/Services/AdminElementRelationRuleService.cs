#pragma warning disable CS1591
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Battle;
using XXX.Entity;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    public class AdminElementRelationRuleService : IAdminElementRelationRuleService
    {
        private readonly IRepository<ElementRelationRuleEntity> _repository;
        private readonly IAdminRuntimeRefreshService _runtimeRefreshService;

        public AdminElementRelationRuleService(
            IRepository<ElementRelationRuleEntity> repository,
            IAdminRuntimeRefreshService runtimeRefreshService)
        {
            _repository = repository;
            _runtimeRefreshService = runtimeRefreshService;
        }

        public async Task<List<AdminElementRelationEntryDto>> GetEntriesAsync()
        {
            var items = await _repository.Db.Queryable<ElementRelationRuleEntity>()
                .OrderBy(item => item.SortOrder)
                .ToListAsync();
            return items.Select(MapEntry).ToList();
        }

        public async Task<List<AdminElementRelationEntryDto>> SaveEntriesAsync(List<AdminElementRelationEntryDto> entries)
        {
            ValidateEntries(entries);

            try
            {
                _repository.Db.Ado.BeginTran();

                var existingMap = (await _repository.Db.Queryable<ElementRelationRuleEntity>()
                    .ToListAsync())
                    .ToDictionary(item => (item.AttackerElement, item.DefenderElement));

                foreach (var entry in entries)
                {
                    if (!existingMap.TryGetValue((entry.AttackerElement, entry.DefenderElement), out var existing))
                    {
                        existing = new ElementRelationRuleEntity
                        {
                            GID = string.IsNullOrWhiteSpace(entry.GID)
                                ? $"{entry.AttackerElement}_{entry.DefenderElement}"
                                : entry.GID.Trim()
                        };

                        ApplyEntry(existing, entry);
                        await _repository.Db.Insertable(existing).ExecuteCommandAsync();
                        continue;
                    }

                    ApplyEntry(existing, entry);
                    await _repository.Db.Updateable(existing).ExecuteCommandAsync();
                }

                _repository.Db.Ado.CommitTran();
            }
            catch
            {
                _repository.Db.Ado.RollbackTran();
                throw;
            }

            await _runtimeRefreshService.ReloadElementRelationRuleCacheAsync();
            return await GetEntriesAsync();
        }

        private static void ValidateEntries(List<AdminElementRelationEntryDto> entries)
        {
            if (entries == null || entries.Count == 0)
            {
                throw new InvalidOperationException("元素矩阵配置不能为空。");
            }

            var supportedElements = GetSupportedElements();
            var expectedCount = supportedElements.Length * supportedElements.Length;
            if (entries.Count != expectedCount)
            {
                throw new InvalidOperationException($"元素矩阵必须提交完整的 {expectedCount} 条配置。");
            }

            var duplicatePair = entries
                .GroupBy(item => (item.AttackerElement, item.DefenderElement))
                .FirstOrDefault(group => group.Count() > 1);
            if (duplicatePair != null)
            {
                throw new InvalidOperationException(
                    $"元素矩阵存在重复配置：{ElementRelation.GetElementName((Element)duplicatePair.Key.AttackerElement)} -> {ElementRelation.GetElementName((Element)duplicatePair.Key.DefenderElement)}。");
            }

            var entryMap = entries.ToDictionary(item => (item.AttackerElement, item.DefenderElement));
            foreach (var attacker in supportedElements)
            {
                foreach (var defender in supportedElements)
                {
                    var attackerValue = (int)attacker;
                    var defenderValue = (int)defender;
                    if (!entryMap.ContainsKey((attackerValue, defenderValue)))
                    {
                        throw new InvalidOperationException(
                            $"元素矩阵缺少配置：{ElementRelation.GetElementName(attacker)} -> {ElementRelation.GetElementName(defender)}。");
                    }
                }
            }
        }

        private static Element[] GetSupportedElements()
        {
            return Enum.GetValues<Element>()
                .Where(item => item is >= Element.Metal and <= Element.Thunder)
                .ToArray();
        }

        private static void ApplyEntry(ElementRelationRuleEntity entity, AdminElementRelationEntryDto dto)
        {
            entity.AttackerElement = dto.AttackerElement;
            entity.DefenderElement = dto.DefenderElement;
            entity.Modifier = dto.Modifier;
            entity.SortOrder = dto.SortOrder;
            entity.IsEnabled = dto.IsEnabled;
            entity.IsBuiltIn = false;
            entity.SeedKey = null;
            entity.BuiltInVersion = null;
            entity.LastUpdateTime = DateTime.Now;
        }

        private static AdminElementRelationEntryDto MapEntry(ElementRelationRuleEntity entity)
        {
            return new AdminElementRelationEntryDto
            {
                GID = entity.GID,
                AttackerElement = entity.AttackerElement,
                DefenderElement = entity.DefenderElement,
                Modifier = entity.Modifier,
                SortOrder = entity.SortOrder,
                IsEnabled = entity.IsEnabled,
                IsBuiltIn = entity.IsBuiltIn,
                SeedKey = entity.SeedKey,
                BuiltInVersion = entity.BuiltInVersion,
                LastUpdateTime = entity.LastUpdateTime
            };
        }
    }
}
#pragma warning restore CS1591
