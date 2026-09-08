#pragma warning disable CS1591
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Repositories;
using XXX.Player;

namespace XXX.Application.Services
{
    public class AdminAttributePointConfigService : IAdminAttributePointConfigService
    {
        private readonly IRepository<AttributePointConfigEntity> _repository;
        private readonly IAdminRuntimeRefreshService _runtimeRefreshService;

        public AdminAttributePointConfigService(
            IRepository<AttributePointConfigEntity> repository,
            IAdminRuntimeRefreshService runtimeRefreshService)
        {
            _repository = repository;
            _runtimeRefreshService = runtimeRefreshService;
        }

        public async Task<AdminAttributePointConfigBundleDto> GetConfigAsync()
        {
            var entities = await _repository.Db.Queryable<AttributePointConfigEntity>()
                .OrderBy(item => item.SortOrder)
                .OrderBy(item => item.LevelStart)
                .OrderBy(item => item.Key)
                .ToListAsync();

            if (entities.Count == 0)
            {
                throw new InvalidOperationException("Attribute point configs are missing. Please run startup seed sync before using growth admin pages.");
            }

            return new AdminAttributePointConfigBundleDto
            {
                IsBuiltIn = entities.All(item => item.IsBuiltIn),
                BuiltInVersion = entities.Select(item => item.BuiltInVersion).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)),
                LastUpdateTime = entities.Max(item => (DateTime?)item.LastUpdateTime),
                Attributes = entities
                    .GroupBy(item => new
                    {
                        Profession = PlayerProfessionCatalog.Normalize(item.Profession),
                        Key = AttributePointConfig.NormalizeKey(item.Key)
                    })
                    .Select(group =>
                    {
                        var first = group
                            .OrderByDescending(item => item.LastUpdateTime)
                            .ThenBy(item => item.ConfigId, StringComparer.OrdinalIgnoreCase)
                            .First();
                        return new AdminAttributePointDefinitionDto
                        {
                            Profession = PlayerProfessionCatalog.Normalize(first.Profession),
                            Key = first.Key,
                            Name = first.Name,
                            AttributeType = first.AttributeType,
                            BonusPerPoint = Math.Max(0, first.BonusPerPoint),
                            PointsPerBonus = Math.Max(1, first.PointsPerBonus)
                        };
                    })
                    .OrderBy(item => GetProfessionSortOrder(item.Profession))
                    .ThenBy(item => item.AttributeType)
                    .ToList(),
                LevelRanges = entities
                    .GroupBy(item => new { item.LevelStart, item.LevelEnd, item.PointsGained })
                    .Select(group => new AdminAttributePointLevelRangeDto
                    {
                        LevelStart = group.Key.LevelStart,
                        LevelEnd = group.Key.LevelEnd,
                        PointsGained = group.Key.PointsGained
                    })
                    .OrderBy(item => item.LevelStart)
                    .ThenBy(item => item.LevelEnd)
                    .ToList()
            };
        }

        public async Task<AdminAttributePointConfigBundleDto> SaveConfigAsync(AdminAttributePointConfigBundleDto request)
        {
            var attributes = NormalizeAttributes(request.Attributes);
            var levelRanges = NormalizeLevelRanges(request.LevelRanges);
            var now = DateTime.Now;
            var entities = attributes
                .SelectMany(attribute => levelRanges.Select(range => new AttributePointConfigEntity
                {
                    ConfigId = BuildConfigId(attribute.Profession, attribute.Key, range.LevelStart, range.LevelEnd),
                    Profession = PlayerProfessionCatalog.Normalize(attribute.Profession),
                    Key = attribute.Key,
                    Name = attribute.Name,
                    AttributeType = attribute.AttributeType,
                    BonusPerPoint = Math.Max(0, attribute.BonusPerPoint),
                    PointsPerBonus = Math.Max(1, attribute.PointsPerBonus),
                    LevelStart = range.LevelStart,
                    LevelEnd = range.LevelEnd,
                    PointsGained = range.PointsGained,
                    SortOrder = GetProfessionSortOrder(attribute.Profession) * 1000 + range.LevelStart * 10 + attribute.AttributeType,
                    IsEnabled = true,
                    SeedKey = null,
                    IsBuiltIn = false,
                    BuiltInVersion = null,
                    LastUpdateTime = now
                }))
                .ToList();

            try
            {
                _repository.Db.Ado.BeginTran();
                await _repository.Db.Deleteable<AttributePointConfigEntity>().ExecuteCommandAsync();
                await _repository.Db.Insertable(entities).ExecuteCommandAsync();
                _repository.Db.Ado.CommitTran();
            }
            catch
            {
                _repository.Db.Ado.RollbackTran();
                throw;
            }

            await _runtimeRefreshService.ReloadGrowthConfigCacheAsync();
            return await GetConfigAsync();
        }

        private static List<AdminAttributePointDefinitionDto> NormalizeAttributes(List<AdminAttributePointDefinitionDto>? attributes)
        {
            if (attributes == null || attributes.Count == 0)
            {
                throw new InvalidOperationException("至少需要一条属性倍率配置。");
            }

            var legacyDefinitions = AttributePointConfig.GetLegacyDefinitions()
                .GroupBy(item => PlayerProfessionCatalog.Normalize(item.Profession))
                .ToDictionary(
                    group => group.Key,
                    group => group.ToDictionary(item => item.Key, StringComparer.OrdinalIgnoreCase),
                    StringComparer.OrdinalIgnoreCase);

            var normalized = attributes
                .Select(item => new AdminAttributePointDefinitionDto
                {
                    Profession = PlayerProfessionCatalog.Normalize(item.Profession),
                    Key = AttributePointConfig.NormalizeKey(item.Key),
                    Name = (item.Name ?? string.Empty).Trim(),
                    AttributeType = item.AttributeType,
                    BonusPerPoint = Math.Max(0, item.BonusPerPoint),
                    PointsPerBonus = Math.Max(1, item.PointsPerBonus)
                })
                .OrderBy(item => GetProfessionSortOrder(item.Profession))
                .ThenBy(item => item.AttributeType)
                .ToList();

            var duplicateKey = normalized
                .GroupBy(item => new { item.Profession, item.Key })
                .FirstOrDefault(group => group.Count() > 1);
            if (duplicateKey != null)
            {
                throw new InvalidOperationException($"属性键重复：{PlayerProfessionCatalog.GetDisplayName(duplicateKey.Key.Profession)} / {duplicateKey.Key.Key}。");
            }

            var duplicateType = normalized
                .GroupBy(item => new { item.Profession, item.AttributeType })
                .FirstOrDefault(group => group.Count() > 1);
            if (duplicateType != null)
            {
                throw new InvalidOperationException($"属性类型重复：{PlayerProfessionCatalog.GetDisplayName(duplicateType.Key.Profession)} / {duplicateType.Key.AttributeType}。");
            }

            foreach (var profession in PlayerProfessionCatalog.GetPlayableProfessions())
            {
                if (!legacyDefinitions.TryGetValue(profession, out var professionLegacyDefinitions))
                {
                    throw new InvalidOperationException($"职业 {profession} 的属性配置模板不存在。");
                }

                var professionItems = normalized
                    .Where(item => item.Profession == profession)
                    .ToList();

                if (professionItems.Count != professionLegacyDefinitions.Count)
                {
                    throw new InvalidOperationException($"职业 {PlayerProfessionCatalog.GetDisplayName(profession)} 的属性倍率配置必须完整覆盖当前支持的全部属性。");
                }

                foreach (var item in professionItems)
                {
                    if (string.IsNullOrWhiteSpace(item.Key) || !professionLegacyDefinitions.TryGetValue(item.Key, out var legacy))
                    {
                        throw new InvalidOperationException($"职业 {PlayerProfessionCatalog.GetDisplayName(profession)} 存在不支持的属性键：{item.Key}。");
                    }

                    if (string.IsNullOrWhiteSpace(item.Name))
                    {
                        throw new InvalidOperationException($"职业 {PlayerProfessionCatalog.GetDisplayName(profession)} 的属性 {item.Key} 名称不能为空。");
                    }

                    if (item.AttributeType != (int)legacy.AttributeType)
                    {
                        throw new InvalidOperationException($"职业 {PlayerProfessionCatalog.GetDisplayName(profession)} 的属性 {item.Key} 底层类型不允许变更。");
                    }
                }
            }

            return normalized;
        }

        private static List<AdminAttributePointLevelRangeDto> NormalizeLevelRanges(List<AdminAttributePointLevelRangeDto>? levelRanges)
        {
            if (levelRanges == null || levelRanges.Count == 0)
            {
                throw new InvalidOperationException("至少需要一条等级区间给点规则。");
            }

            var normalized = levelRanges
                .Select(item => new AdminAttributePointLevelRangeDto
                {
                    LevelStart = item.LevelStart,
                    LevelEnd = item.LevelEnd,
                    PointsGained = Math.Max(0, item.PointsGained)
                })
                .OrderBy(item => item.LevelStart)
                .ThenBy(item => item.LevelEnd)
                .ToList();

            var expectedStart = 2;
            foreach (var item in normalized)
            {
                if (item.LevelStart < 2 || item.LevelEnd > LevelConfig.MaxLevel || item.LevelStart > item.LevelEnd)
                {
                    throw new InvalidOperationException($"非法的等级区间：{item.LevelStart}-{item.LevelEnd}。");
                }

                if (item.LevelStart != expectedStart)
                {
                    throw new InvalidOperationException($"等级区间必须从 {expectedStart} 级开始连续配置，当前检测到 {item.LevelStart}-{item.LevelEnd}。");
                }

                expectedStart = item.LevelEnd + 1;
            }

            if (expectedStart != LevelConfig.MaxLevel + 1)
            {
                throw new InvalidOperationException($"等级区间必须连续覆盖到 {LevelConfig.MaxLevel} 级。");
            }

            return normalized;
        }

        private static string BuildConfigId(string profession, string key, int levelStart, int levelEnd)
        {
            return $"{PlayerProfessionCatalog.Normalize(profession)}_{AttributePointConfig.NormalizeKey(key)}_{levelStart}_{levelEnd}";
        }

        private static int GetProfessionSortOrder(string? profession)
        {
            return PlayerProfessionCatalog.Normalize(profession) switch
            {
                PlayerProfessionCatalog.Warrior => 1,
                PlayerProfessionCatalog.Mage => 2,
                PlayerProfessionCatalog.Body => 3,
                _ => 9
            };
        }

    }
}
#pragma warning restore CS1591
