using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    /// <summary>
    /// 管理后台地图模板服务。
    /// 提供地图模板的查询、编辑与删除能力，并负责处理地图和副本之间的引用保护。
    /// </summary>
    public class AdminMapService : IAdminMapService
    {
        private readonly IRepository<MapTemplateEntity> _mapRepository;
        private readonly IRepository<DungeonTemplateEntity> _dungeonRepository;
        private readonly IAdminRuntimeRefreshService _runtimeRefreshService;

        /// <summary>
        /// 初始化地图模板服务。
        /// </summary>
        public AdminMapService(
            IRepository<MapTemplateEntity> mapRepository,
            IRepository<DungeonTemplateEntity> dungeonRepository,
            IAdminRuntimeRefreshService runtimeRefreshService)
        {
            _mapRepository = mapRepository;
            _dungeonRepository = dungeonRepository;
            _runtimeRefreshService = runtimeRefreshService;
        }

        /// <summary>
        /// 获取地图列表。
        /// </summary>
        public async Task<List<AdminMapListItemDto>> GetListAsync(string? keyword = null, string? mapKind = null)
        {
            var normalizedKeyword = (keyword ?? string.Empty).Trim();
            var query = _mapRepository.Db.Queryable<MapTemplateEntity>();

            if (!string.IsNullOrWhiteSpace(normalizedKeyword))
            {
                query = query.Where(map =>
                    map.MapId.Contains(normalizedKeyword) ||
                    map.Name.Contains(normalizedKeyword));
            }

            if (mapKind == "dungeonLayer")
            {
                query = query.Where(map => map.MapId.StartsWith("fuben_"));
            }
            else if (mapKind == "normal")
            {
                query = query.Where(map => !map.MapId.StartsWith("fuben_"));
            }

            var maps = await query
                .OrderBy(map => map.Level)
                .OrderBy(map => map.MapId)
                .ToListAsync();

            return maps.Select(MapListItem).ToList();
        }

        /// <summary>
        /// 获取地图详情。
        /// </summary>
        public async Task<AdminMapDetailDto?> GetDetailAsync(string mapId)
        {
            if (string.IsNullOrWhiteSpace(mapId))
            {
                return null;
            }

            var map = await _mapRepository.GetByIdAsync(mapId.Trim());
            if (map == null) return null;
            var detail = MapDetail(map);
            var references = await _dungeonRepository.Db.Queryable<DungeonTemplateEntity>()
                .Where(dungeon => dungeon.FubenMapId == map.MapId)
                .Select(dungeon => dungeon.DungeonId)
                .ToListAsync();
            detail.ReferencingDungeonIds = references;
            detail.ChainValid = true;
            detail.ChainError = null;
            return detail;
        }

        /// <summary>
        /// 保存地图。
        /// </summary>
        public async Task<AdminMapDetailDto> SaveAsync(AdminSaveMapRequestDto request)
        {
            var mapId = (request.MapId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(mapId))
            {
                throw new InvalidOperationException("地图编号不能为空。");
            }

            var name = (request.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("地图名称不能为空。");
            }

            var isDungeonLayer = IsDungeonLayerMapId(mapId);
            var isNormalMap = IsNormalMapId(mapId);
            if (!isDungeonLayer && !isNormalMap)
            {
                throw new InvalidOperationException("地图编号必须使用 map_ 或 fuben_ 前缀。");
            }

            if (request.MonsterCountMin <= 0 || request.MonsterCountMax <= 0)
            {
                throw new InvalidOperationException("怪物数量必须大于 0。");
            }

            if (request.MonsterCountMin > request.MonsterCountMax)
            {
                throw new InvalidOperationException("最小怪物数量不能大于最大怪物数量。");
            }

            var spawnRules = NormalizeSpawnRules(request.SpawnRules);
            if (spawnRules.Count == 0)
            {
                throw new InvalidOperationException("至少需要一条刷怪规则。");
            }

            if (!string.IsNullOrWhiteSpace(request.NextMapId) &&
                string.Equals(mapId, request.NextMapId.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("下一层地图不能指向自己。");
            }

            if (!isDungeonLayer)
            {
                request.NextMapId = null;
                request.Carrying = false;
            }
            else if (!string.IsNullOrWhiteSpace(request.NextMapId) && !IsDungeonLayerMapId(request.NextMapId.Trim()))
            {
                throw new InvalidOperationException("副本层的 NextMapId 只能指向其他副本层地图。");
            }

            if (isDungeonLayer)
            {
                await ValidateMapChainAsync(mapId, request.NextMapId);
            }
            var existing = await _mapRepository.GetByIdAsync(mapId);
            if (existing == null)
            {
                existing = new MapTemplateEntity
                {
                    MapId = mapId
                };

                await _mapRepository.AddAsync(ApplyMap(existing, request, spawnRules));
                await _runtimeRefreshService.ReloadRuntimeTemplatesAsync();
                return MapDetail(existing);
            }

            ApplyMap(existing, request, spawnRules);
            await _mapRepository.UpdateAsync(existing);
            await _runtimeRefreshService.ReloadRuntimeTemplatesAsync();
            return MapDetail(existing);
        }

        /// <summary>
        /// 删除地图。
        /// </summary>
        public async Task<bool> DeleteAsync(string mapId)
        {
            var normalizedMapId = (mapId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedMapId))
            {
                return false;
            }

            var hasNextMapReference = await _mapRepository.Db.Queryable<MapTemplateEntity>()
                .Where(map => map.NextMapId == normalizedMapId)
                .AnyAsync();
            if (hasNextMapReference)
            {
                throw new InvalidOperationException("当前地图仍被其他地图的 NextMapId 引用，不能直接删除。");
            }

            var hasDungeonReference = await _dungeonRepository.Db.Queryable<DungeonTemplateEntity>()
                .Where(dungeon => dungeon.FubenMapId == normalizedMapId)
                .AnyAsync();
            if (hasDungeonReference)
            {
                throw new InvalidOperationException("当前地图仍被副本配置引用，不能直接删除。");
            }

            var deleteRows = await _mapRepository.DeleteAsync(normalizedMapId);
            if (deleteRows > 0)
            {
                await _runtimeRefreshService.ReloadRuntimeTemplatesAsync();
            }

            return deleteRows > 0;
        }

        private static MapTemplateEntity ApplyMap(
            MapTemplateEntity entity,
            AdminSaveMapRequestDto request,
            List<MapMonsterSpawnRule> spawnRules)
        {
            entity.Name = request.Name.Trim();
            entity.Level = Math.Max(1, request.Level);
            entity.Description = (request.Description ?? string.Empty).Trim();
            entity.NextMapId = string.IsNullOrWhiteSpace(request.NextMapId)
                ? null
                : request.NextMapId.Trim();
            entity.Carrying = request.Carrying;
            entity.MonsterCountMin = request.MonsterCountMin;
            entity.MonsterCountMax = request.MonsterCountMax;
            entity.SpawnRules = spawnRules;
            entity.SeedKey = null;
            entity.IsBuiltIn = false;
            entity.BuiltInVersion = null;
            entity.LastUpdateTime = DateTime.Now;
            return entity;
        }

        private async Task ValidateMapChainAsync(string mapId, string? requestedNextMapId)
        {
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var currentId = mapId;
            var overrideNext = true;
            for (var stage = 1; stage <= 20; stage++)
            {
                if (!visited.Add(currentId))
                {
                    throw new InvalidOperationException("地图链存在循环。");
                }
                var map = await _mapRepository.GetByIdAsync(currentId);
                if (map == null)
                {
                    if (stage == 1 || !overrideNext)
                    {
                        throw new InvalidOperationException($"地图链断裂：{currentId} 不存在。");
                    }
                    // 当前正在新建首节点，NextMapId 的目标仍必须存在。
                    throw new InvalidOperationException($"地图链断裂：{currentId} 不存在。");
                }
                if (mapId.StartsWith("fuben_", StringComparison.OrdinalIgnoreCase) && !map.MapId.StartsWith("fuben_", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException($"副本地图链包含普通地图：{map.MapId}。");
                }
                var next = overrideNext ? requestedNextMapId ?? map.NextMapId : map.NextMapId;
                overrideNext = false;
                if (string.IsNullOrWhiteSpace(next))
                {
                    return;
                }
                if (!next.StartsWith("fuben_", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException($"副本地图链包含普通地图：{next}。");
                }

                // 中文注释：仅检查 fuben_ 前缀无法阻止跨副本串链；这里比较源地图与目标地图的真实副本归属。
                var sourceDungeonId = await FindDungeonOwnerAsync(currentId);
                var nextDungeonId = await FindDungeonOwnerAsync(next);
                if (!string.IsNullOrWhiteSpace(sourceDungeonId) &&
                    !string.IsNullOrWhiteSpace(nextDungeonId) &&
                    !string.Equals(sourceDungeonId, nextDungeonId, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException($"副本地图链不能跨副本连接：{mapId} 属于 {sourceDungeonId}，{next} 属于 {nextDungeonId}。");
                }

                currentId = next;
            }
            throw new InvalidOperationException("地图链超过最大层数 20。");
        }

        /// <summary>
        /// 查找地图在完整副本链中的所属副本编号。
        /// 未被任何副本链引用的临时地图返回空值，兼容隔离测试地图。
        /// </summary>
        private async Task<string?> FindDungeonOwnerAsync(string mapId)
        {
            var dungeons = await _dungeonRepository.Db.Queryable<DungeonTemplateEntity>().ToListAsync();
            var maps = await _mapRepository.Db.Queryable<MapTemplateEntity>().ToListAsync();
            var mapLookup = maps.ToDictionary(item => item.MapId, StringComparer.OrdinalIgnoreCase);
            foreach (var dungeon in dungeons)
            {
                var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var current = dungeon.FubenMapId;
                for (var stage = 1; stage <= 20 && !string.IsNullOrWhiteSpace(current); stage++)
                {
                    if (!visited.Add(current)) break;
                    if (string.Equals(current, mapId, StringComparison.OrdinalIgnoreCase)) return dungeon.DungeonId;
                    if (!mapLookup.TryGetValue(current, out var map)) break;
                    current = map.NextMapId;
                }
            }
            return null;
        }

        /// <summary>
        /// 规范化地图刷怪规则。
        /// </summary>
        private static List<MapMonsterSpawnRule> NormalizeSpawnRules(IEnumerable<MapMonsterSpawnRule>? spawnRules)
        {
            return (spawnRules ?? Enumerable.Empty<MapMonsterSpawnRule>())
                .Where(rule => rule != null && !string.IsNullOrWhiteSpace(rule.MonsterTemplateId))
                .Select(rule => new MapMonsterSpawnRule
                {
                    MonsterTemplateId = rule.MonsterTemplateId.Trim(),
                    Weight = Math.Max(1, rule.Weight),
                    MaxCount = Math.Max(1, rule.MaxCount)
                })
                .ToList();
        }

        private static bool IsNormalMapId(string mapId)
        {
            return mapId.StartsWith("map_", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsDungeonLayerMapId(string mapId)
        {
            return mapId.StartsWith("fuben_", StringComparison.OrdinalIgnoreCase);
        }

        private static AdminMapListItemDto MapListItem(MapTemplateEntity map)
        {
            return new AdminMapListItemDto
            {
                MapId = map.MapId,
                Name = map.Name,
                Level = map.Level,
                NextMapId = map.NextMapId,
                MonsterCountMin = map.MonsterCountMin,
                MonsterCountMax = map.MonsterCountMax,
                SpawnRuleCount = map.SpawnRules.Count,
                IsBuiltIn = map.IsBuiltIn,
                BuiltInVersion = map.BuiltInVersion
            };
        }

        private static AdminMapDetailDto MapDetail(MapTemplateEntity map)
        {
            return new AdminMapDetailDto
            {
                MapId = map.MapId,
                Name = map.Name,
                Level = map.Level,
                Description = map.Description,
                NextMapId = map.NextMapId,
                Carrying = map.Carrying,
                MonsterCountMin = map.MonsterCountMin,
                MonsterCountMax = map.MonsterCountMax,
                SpawnRules = map.SpawnRules.ToList(),
                IsBuiltIn = map.IsBuiltIn,
                SeedKey = map.SeedKey,
                BuiltInVersion = map.BuiltInVersion,
                LastUpdateTime = map.LastUpdateTime
            };
        }
    }
}
