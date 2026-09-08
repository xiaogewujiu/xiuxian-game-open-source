#pragma warning disable CS1591
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    public class AdminDungeonService : IAdminDungeonService
    {
        private readonly IRepository<DungeonTemplateEntity> _dungeonRepository;
        private readonly IRepository<MapTemplateEntity> _mapRepository;
        private readonly IRepository<DungeonDailyRecordEntity> _dungeonDailyRecordRepository;
        private readonly IAdminRuntimeRefreshService _runtimeRefreshService;

        public AdminDungeonService(
            IRepository<DungeonTemplateEntity> dungeonRepository,
            IRepository<MapTemplateEntity> mapRepository,
            IRepository<DungeonDailyRecordEntity> dungeonDailyRecordRepository,
            IAdminRuntimeRefreshService runtimeRefreshService)
        {
            _dungeonRepository = dungeonRepository;
            _mapRepository = mapRepository;
            _dungeonDailyRecordRepository = dungeonDailyRecordRepository;
            _runtimeRefreshService = runtimeRefreshService;
        }

        public async Task<List<AdminDungeonListItemDto>> GetListAsync(string? keyword = null)
        {
            var normalizedKeyword = (keyword ?? string.Empty).Trim();
            var query = _dungeonRepository.Db.Queryable<DungeonTemplateEntity>();

            if (!string.IsNullOrWhiteSpace(normalizedKeyword))
            {
                query = query.Where(dungeon => dungeon.DungeonId.Contains(normalizedKeyword) || dungeon.Name.Contains(normalizedKeyword));
            }

            var dungeons = await query.OrderBy(dungeon => dungeon.RecommendedLevel).OrderBy(dungeon => dungeon.DungeonId).ToListAsync();
            return dungeons.Select(dungeon => new AdminDungeonListItemDto
            {
                DungeonId = dungeon.DungeonId,
                Name = dungeon.Name,
                RecommendedLevel = dungeon.RecommendedLevel,
                DailyLimit = dungeon.DailyLimit,
                RequiredTeamSize = dungeon.RequiredTeamSize,
                IsBuiltIn = dungeon.IsBuiltIn,
                BuiltInVersion = dungeon.BuiltInVersion
            }).ToList();
        }

        /// <summary>
        /// 获取当前副本地图链的层级和校验结果。
        /// </summary>
        public async Task<AdminDungeonDetailDto?> GetDetailAsync(string dungeonId)
        {
            if (string.IsNullOrWhiteSpace(dungeonId))
            {
                return null;
            }

            var dungeon = await _dungeonRepository.GetByIdAsync(dungeonId.Trim());
            if (dungeon == null)
            {
                return null;
            }

            var stages = new List<AdminDungeonMapStageDto>();
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var mapId = dungeon.FubenMapId;
            string? chainError = null;
            for (var stageIndex = 1; stageIndex <= 20 && !string.IsNullOrWhiteSpace(mapId); stageIndex++)
            {
                if (!visited.Add(mapId))
                {
                    chainError = $"地图链存在循环：{mapId}";
                    break;
                }
                var map = await _mapRepository.GetByIdAsync(mapId);
                if (map == null)
                {
                    chainError = $"地图链断裂，地图不存在：{mapId}";
                    break;
                }
                if (!map.MapId.StartsWith("fuben_", StringComparison.OrdinalIgnoreCase))
                {
                    chainError = $"地图链包含普通地图：{map.MapId}";
                    break;
                }
                stages.Add(new AdminDungeonMapStageDto { StageIndex = stageIndex, MapId = map.MapId, MapName = map.Name, NextMapId = map.NextMapId });
                mapId = map.NextMapId;
            }
            if (chainError == null && !string.IsNullOrWhiteSpace(mapId))
            {
                chainError = "地图链超过最大层数 20";
            }

            var detail = new AdminDungeonDetailDto
            {
                DungeonId = dungeon.DungeonId,
                Name = dungeon.Name,
                Description = dungeon.Description,
                RecommendedLevel = dungeon.RecommendedLevel,
                DailyLimit = dungeon.DailyLimit,
                FubenMapId = dungeon.FubenMapId,
                RequiredTeamSize = dungeon.RequiredTeamSize,
                IsBuiltIn = dungeon.IsBuiltIn,
                SeedKey = dungeon.SeedKey,
                BuiltInVersion = dungeon.BuiltInVersion,
                LastUpdateTime = dungeon.LastUpdateTime,
                ChainValid = chainError == null && stages.Count > 0,
                ChainError = chainError,
                ChainStages = stages
            };
            return detail;
        }

        /// <summary>
        /// 保存副本模板前校验完整地图链。
        /// </summary>
        private async Task ValidateDungeonChainAsync(string dungeonId, string startMapId)
        {
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var mapId = startMapId;
            for (var stage = 1; stage <= 20 && !string.IsNullOrWhiteSpace(mapId); stage++)
            {
                if (!visited.Add(mapId))
                {
                    throw new InvalidOperationException("副本地图链存在循环。");
                }
                var map = await _mapRepository.GetByIdAsync(mapId);
                if (map == null)
                {
                    throw new InvalidOperationException($"副本地图链断裂：{mapId} 不存在。");
                }
                if (!map.MapId.StartsWith("fuben_", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException($"副本地图链包含普通地图：{map.MapId}。");
                }

                // 中文注释：完整链路中的每个节点都必须归属于当前副本，禁止跨副本 fuben_ 地图串联。
                var otherDungeon = await FindMapOwnerAsync(map.MapId, dungeonId);
                if (!string.IsNullOrWhiteSpace(otherDungeon))
                {
                    throw new InvalidOperationException($"副本地图链不能跨副本连接：{map.MapId} 已属于副本 {otherDungeon}。");
                }

                mapId = map.NextMapId;
                if (stage == 20 && !string.IsNullOrWhiteSpace(mapId))
                {
                    throw new InvalidOperationException("副本地图链超过最大层数 20。");
                }
            }
        }
        /// <summary>
        /// 查找地图在所有副本完整链路中的归属。
        /// </summary>
        private async Task<string?> FindMapOwnerAsync(string mapId, string currentDungeonId)
        {
            var dungeons = await _dungeonRepository.Db.Queryable<DungeonTemplateEntity>().ToListAsync();
            var maps = await _mapRepository.Db.Queryable<MapTemplateEntity>().ToListAsync();
            var lookup = maps.ToDictionary(item => item.MapId, StringComparer.OrdinalIgnoreCase);
            foreach (var dungeon in dungeons)
            {
                if (string.Equals(dungeon.DungeonId, currentDungeonId, StringComparison.OrdinalIgnoreCase)) continue;
                var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var current = dungeon.FubenMapId;
                for (var stage = 1; stage <= 20 && !string.IsNullOrWhiteSpace(current); stage++)
                {
                    if (!visited.Add(current)) break;
                    if (string.Equals(current, mapId, StringComparison.OrdinalIgnoreCase)) return dungeon.DungeonId;
                    if (!lookup.TryGetValue(current, out var map)) break;
                    current = map.NextMapId;
                }
            }
            return null;
        }

        /// <summary>
        /// 保存副本模板。
        /// </summary>
        public async Task<AdminDungeonDetailDto> SaveAsync(AdminDungeonDetailDto request)
        {
            var dungeonId = (request.DungeonId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(dungeonId))
            {
                throw new InvalidOperationException("副本编号不能为空。");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new InvalidOperationException("副本名称不能为空。");
            }

            var fubenMapId = (request.FubenMapId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(fubenMapId))
            {
                throw new InvalidOperationException("副本首层地图编号不能为空。");
            }

            if (!fubenMapId.StartsWith("fuben_", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("副本首层地图必须使用 fuben_ 前缀。");
            }

            if (!await _mapRepository.ExistsAsync(map => map.MapId == fubenMapId))
            {
                throw new InvalidOperationException("副本首层地图不存在，请先创建对应地图模板。");
            }

            await ValidateDungeonChainAsync(dungeonId, fubenMapId);

            var existing = await _dungeonRepository.GetByIdAsync(dungeonId);
            if (existing == null)
            {
                existing = new DungeonTemplateEntity { DungeonId = dungeonId };
                await _dungeonRepository.AddAsync(ApplyDungeon(existing, request));
                await _runtimeRefreshService.ReloadRuntimeTemplatesAsync();
                return (await GetDetailAsync(dungeonId))!;
            }

            ApplyDungeon(existing, request);
            await _dungeonRepository.UpdateAsync(existing);
            await _runtimeRefreshService.ReloadRuntimeTemplatesAsync();
            return (await GetDetailAsync(dungeonId))!;
        }

        public async Task<bool> DeleteAsync(string dungeonId)
        {
            var normalizedDungeonId = (dungeonId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedDungeonId))
            {
                return false;
            }

            var hasDailyRecords = await _dungeonDailyRecordRepository.Db.Queryable<DungeonDailyRecordEntity>()
                .Where(record => record.DungeonId == normalizedDungeonId)
                .AnyAsync();
            if (hasDailyRecords)
            {
                throw new InvalidOperationException("当前副本仍存在挑战记录，不能直接删除。");
            }

            var deleteRows = await _dungeonRepository.DeleteAsync(normalizedDungeonId);
            if (deleteRows > 0)
            {
                await _runtimeRefreshService.ReloadRuntimeTemplatesAsync();
            }

            return deleteRows > 0;
        }

        private static DungeonTemplateEntity ApplyDungeon(DungeonTemplateEntity entity, AdminDungeonDetailDto request)
        {
            entity.Name = request.Name.Trim();
            entity.Description = (request.Description ?? string.Empty).Trim();
            entity.RecommendedLevel = Math.Max(1, request.RecommendedLevel);
            entity.DailyLimit = Math.Max(0, request.DailyLimit);
            entity.NormalMapId = string.Empty;
            entity.FubenMapId = request.FubenMapId.Trim();
            entity.RequiredTeamSize = Math.Max(1, request.RequiredTeamSize);
            entity.SeedKey = null;
            entity.IsBuiltIn = false;
            entity.BuiltInVersion = null;
            entity.LastUpdateTime = DateTime.Now;
            return entity;
        }
    }
}
#pragma warning restore CS1591
