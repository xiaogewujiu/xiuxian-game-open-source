using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    /// <summary>
    /// 管理后台怪物模板服务。
    /// </summary>
    public class AdminMonsterService : IAdminMonsterService
    {
        private readonly IRepository<MonsterTemplateEntity> _monsterRepository;
        private readonly IRepository<MapTemplateEntity> _mapRepository;
        private readonly IAdminRuntimeRefreshService _runtimeRefreshService;

        /// <summary>
        /// 初始化怪物模板服务。
        /// </summary>
        public AdminMonsterService(
            IRepository<MonsterTemplateEntity> monsterRepository,
            IRepository<MapTemplateEntity> mapRepository,
            IAdminRuntimeRefreshService runtimeRefreshService)
        {
            _monsterRepository = monsterRepository;
            _mapRepository = mapRepository;
            _runtimeRefreshService = runtimeRefreshService;
        }

        /// <summary>
        /// 获取怪物列表。
        /// </summary>
        public async Task<List<AdminMonsterListItemDto>> GetListAsync(string? keyword = null)
        {
            var normalizedKeyword = (keyword ?? string.Empty).Trim();
            var query = _monsterRepository.Db.Queryable<MonsterTemplateEntity>();

            if (!string.IsNullOrWhiteSpace(normalizedKeyword))
            {
                query = query.Where(monster =>
                    monster.MonsterId.Contains(normalizedKeyword) ||
                    monster.Name.Contains(normalizedKeyword));
            }

            var monsters = await query
                .OrderBy(monster => monster.Level)
                .OrderBy(monster => monster.MonsterId)
                .ToListAsync();

            return monsters.Select(monster => new AdminMonsterListItemDto
            {
                MonsterId = monster.MonsterId,
                Name = monster.Name,
                Level = monster.Level,
                SkillCount = monster.SkillIds.Count,
                PassiveCount = monster.PassiveIds.Count,
                IsBuiltIn = monster.IsBuiltIn,
                BuiltInVersion = monster.BuiltInVersion
            }).ToList();
        }

        /// <summary>
        /// 获取怪物详情。
        /// </summary>
        public async Task<AdminMonsterDetailDto?> GetDetailAsync(string monsterId)
        {
            if (string.IsNullOrWhiteSpace(monsterId))
            {
                return null;
            }

            var monster = await _monsterRepository.GetByIdAsync(monsterId.Trim());
            return monster == null ? null : MapDetail(monster);
        }

        /// <summary>
        /// 保存怪物模板。
        /// </summary>
        public async Task<AdminMonsterDetailDto> SaveAsync(AdminSaveMonsterRequestDto request)
        {
            var monsterId = (request.MonsterId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(monsterId))
            {
                throw new InvalidOperationException("怪物编号不能为空。");
            }

            var name = (request.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("怪物名称不能为空。");
            }

            var existing = await _monsterRepository.GetByIdAsync(monsterId);
            if (existing == null)
            {
                existing = new MonsterTemplateEntity
                {
                    MonsterId = monsterId
                };

                await _monsterRepository.AddAsync(ApplyMonster(existing, request));
                await _runtimeRefreshService.ReloadRuntimeTemplatesAsync();
                return MapDetail(existing);
            }

            ApplyMonster(existing, request);
            await _monsterRepository.UpdateAsync(existing);
            await _runtimeRefreshService.ReloadRuntimeTemplatesAsync();
            return MapDetail(existing);
        }

        /// <summary>
        /// 删除怪物模板。
        /// </summary>
        public async Task<bool> DeleteAsync(string monsterId)
        {
            var normalizedMonsterId = (monsterId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedMonsterId))
            {
                return false;
            }

            var maps = await _mapRepository.Db.Queryable<MapTemplateEntity>().ToListAsync();
            var referencedMap = maps.FirstOrDefault(map =>
                map.SpawnRules.Any(rule => string.Equals(rule.MonsterTemplateId, normalizedMonsterId, StringComparison.OrdinalIgnoreCase)));
            if (referencedMap != null)
            {
                throw new InvalidOperationException($"当前怪物仍被地图 {referencedMap.MapId} 引用，不能直接删除。");
            }

            var deleteRows = await _monsterRepository.DeleteAsync(normalizedMonsterId);
            if (deleteRows > 0)
            {
                await _runtimeRefreshService.ReloadRuntimeTemplatesAsync();
            }

            return deleteRows > 0;
        }

        private static MonsterTemplateEntity ApplyMonster(MonsterTemplateEntity entity, AdminSaveMonsterRequestDto request)
        {
            entity.Name = request.Name.Trim();
            entity.Level = Math.Max(1, request.Level);
            entity.ExpRewardMin = Math.Max(0, request.ExpRewardMin);
            entity.ExpRewardMax = Math.Max(entity.ExpRewardMin, request.ExpRewardMax);
            entity.GoldRewardMin = Math.Max(0, request.GoldRewardMin);
            entity.GoldRewardMax = Math.Max(entity.GoldRewardMin, request.GoldRewardMax);
            entity.SkillIds = NormalizeIds(request.SkillIds);
            entity.PassiveIds = NormalizeIds(request.PassiveIds);
            entity.ElementPool = (request.ElementPool ?? [])
                .Where(element => element != Element.None)
                .Distinct()
                .ToList();
            entity.ItemDrops = NormalizeItemDrops(request.ItemDrops);
            entity.EquipmentDrops = NormalizeEquipmentDrops(request.EquipmentDrops);
            entity.CollectionDrops = NormalizeCollectionDrops(request.CollectionDrops);
            ApplyAttributes(entity, request.Attributes);
            entity.SeedKey = null;
            entity.IsBuiltIn = false;
            entity.BuiltInVersion = null;
            entity.LastUpdateTime = DateTime.Now;
            return entity;
        }

        private static List<string> NormalizeIds(IEnumerable<string>? values)
        {
            return (values ?? Enumerable.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static List<DropItem> NormalizeItemDrops(IEnumerable<DropItem>? itemDrops)
        {
            return (itemDrops ?? Enumerable.Empty<DropItem>())
                .Where(drop => drop != null && !string.IsNullOrWhiteSpace(drop.ItemId))
                .Select(drop => new DropItem
                {
                    ItemId = drop.ItemId.Trim(),
                    Rate = Math.Clamp(drop.Rate, 0, 10000)
                })
                .ToList();
        }

        private static List<DropEquipment> NormalizeEquipmentDrops(IEnumerable<DropEquipment>? equipmentDrops)
        {
            return (equipmentDrops ?? Enumerable.Empty<DropEquipment>())
                .Where(drop => drop != null && !string.IsNullOrWhiteSpace(drop.EquipmentId))
                .Select(drop => new DropEquipment
                {
                    EquipmentId = drop.EquipmentId.Trim(),
                    Rate = Math.Clamp(drop.Rate, 0, 10000)
                })
                .ToList();
        }

        private static List<DropCollection> NormalizeCollectionDrops(IEnumerable<DropCollection>? collectionDrops)
        {
            return (collectionDrops ?? Enumerable.Empty<DropCollection>())
                .Where(drop => drop != null && !string.IsNullOrWhiteSpace(drop.SeriesId))
                .Select(drop => new DropCollection
                {
                    SeriesId = drop.SeriesId.Trim(),
                    CollectionType = drop.CollectionType,
                    Rate = Math.Clamp(drop.Rate, 0, 10000)
                })
                .ToList();
        }

        private static void ApplyAttributes(MonsterTemplateEntity entity, BaseAttributesRangeDto attributes)
        {
            entity.MinType1 = attributes.MinType1;
            entity.MaxType1 = attributes.MaxType1;
            entity.MinType2 = attributes.MinType2;
            entity.MaxType2 = attributes.MaxType2;
            entity.MinType3 = attributes.MinType3;
            entity.MaxType3 = attributes.MaxType3;
            entity.MinType4 = attributes.MinType4;
            entity.MaxType4 = attributes.MaxType4;
            entity.MinType5 = attributes.MinType5;
            entity.MaxType5 = attributes.MaxType5;
            entity.MinType6 = attributes.MinType6;
            entity.MaxType6 = attributes.MaxType6;
            entity.MinType7 = attributes.MinType7;
            entity.MaxType7 = attributes.MaxType7;
            entity.MinType8 = attributes.MinType8;
            entity.MaxType8 = attributes.MaxType8;
            entity.MinType9 = attributes.MinType9;
            entity.MaxType9 = attributes.MaxType9;
            entity.MinType10 = attributes.MinType10;
            entity.MaxType10 = attributes.MaxType10;
            entity.MinType11 = attributes.MinType11;
            entity.MaxType11 = attributes.MaxType11;
            entity.MinType12 = attributes.MinType12;
            entity.MaxType12 = attributes.MaxType12;
            entity.MinType13 = attributes.MinType13;
            entity.MaxType13 = attributes.MaxType13;
            entity.MinType14 = attributes.MinType14;
            entity.MaxType14 = attributes.MaxType14;
            entity.MinType15 = attributes.MinType15;
            entity.MaxType15 = attributes.MaxType15;
            entity.Element = attributes.Element;
        }

        private static AdminMonsterDetailDto MapDetail(MonsterTemplateEntity monster)
        {
            return new AdminMonsterDetailDto
            {
                MonsterId = monster.MonsterId,
                Name = monster.Name,
                Level = monster.Level,
                ExpRewardMin = monster.ExpRewardMin,
                ExpRewardMax = monster.ExpRewardMax,
                GoldRewardMin = monster.GoldRewardMin,
                GoldRewardMax = monster.GoldRewardMax,
                SkillIds = monster.SkillIds.ToList(),
                PassiveIds = monster.PassiveIds.ToList(),
                ElementPool = monster.ElementPool.ToList(),
                ItemDrops = monster.ItemDrops.ToList(),
                EquipmentDrops = monster.EquipmentDrops.ToList(),
                CollectionDrops = monster.CollectionDrops.ToList(),
                IsBuiltIn = monster.IsBuiltIn,
                SeedKey = monster.SeedKey,
                BuiltInVersion = monster.BuiltInVersion,
                LastUpdateTime = monster.LastUpdateTime,
                Attributes = new BaseAttributesRangeDto
                {
                    MinType1 = monster.MinType1,
                    MaxType1 = monster.MaxType1,
                    MinType2 = monster.MinType2,
                    MaxType2 = monster.MaxType2,
                    MinType3 = monster.MinType3,
                    MaxType3 = monster.MaxType3,
                    MinType4 = monster.MinType4,
                    MaxType4 = monster.MaxType4,
                    MinType5 = monster.MinType5,
                    MaxType5 = monster.MaxType5,
                    MinType6 = monster.MinType6,
                    MaxType6 = monster.MaxType6,
                    MinType7 = monster.MinType7,
                    MaxType7 = monster.MaxType7,
                    MinType8 = monster.MinType8,
                    MaxType8 = monster.MaxType8,
                    MinType9 = monster.MinType9,
                    MaxType9 = monster.MaxType9,
                    MinType10 = monster.MinType10,
                    MaxType10 = monster.MaxType10,
                    MinType11 = monster.MinType11,
                    MaxType11 = monster.MaxType11,
                    MinType12 = monster.MinType12,
                    MaxType12 = monster.MaxType12,
                    MinType13 = monster.MinType13,
                    MaxType13 = monster.MaxType13,
                    MinType14 = monster.MinType14,
                    MaxType14 = monster.MaxType14,
                    MinType15 = monster.MinType15,
                    MaxType15 = monster.MaxType15,
                    Element = monster.Element
                }
            };
        }
    }
}
