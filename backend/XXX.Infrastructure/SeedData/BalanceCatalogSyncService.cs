using Microsoft.Extensions.Logging;
using XXX.Entity;
using XXX.Infrastructure.Data;

namespace XXX.Infrastructure.SeedData
{
    /// <summary>
    /// 将内置平衡表同步到数据库模板表。
    /// </summary>
    internal sealed class BalanceCatalogSyncService
    {
        private const string LegacySummonScrollItemId = "summon_scroll";
        private const string PetEggItemId = "pet_egg";
        private const string ItemTemplateBuiltInVersion = "item-template-built-in-v1-20260403";
        private const string EquipmentTemplateBuiltInVersion = "equipment-template-built-in-v1-20260403";
        private const string MapTemplateBuiltInVersion = "map-template-built-in-v1-20260403";
        private const string MonsterTemplateBuiltInVersion = "monster-template-built-in-v1-20260403";
        private const string PetTemplateBuiltInVersion = "pet-template-built-in-v1-20260403";
        private const string DungeonTemplateBuiltInVersion = "dungeon-template-built-in-v1-20260403";

        private static readonly HashSet<string> NormalSeedMapIds = new(StringComparer.OrdinalIgnoreCase)
        {
            "map_001",
            "map_002",
            "map_003",
            "map_004",
            "map_005",
            "map_006",
            "map_007",
            "map_008"
        };

        private static readonly HashSet<string> LegacySingleLayerDungeonMapIds = new(StringComparer.OrdinalIgnoreCase)
        {
            "fuben_001",
            "fuben_002",
            "fuben_003",
            "fuben_004"
        };

        private readonly DbContext _dbContext;
        private readonly ILogger<BalanceCatalogSyncService>? _logger;

        /// <summary>
        /// 初始化平衡表同步服务。
        /// </summary>
        /// <param name="dbContext">数据库上下文。</param>
        /// <param name="logger">日志记录器。</param>
        public BalanceCatalogSyncService(DbContext dbContext, ILogger<BalanceCatalogSyncService>? logger = null)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// 用内置快照初始化数据库中的模板数据。
        /// 仅在对应模板表为空时执行首轮补种，避免每次启动覆盖后台已经修改过的数据。
        /// </summary>
        public async Task SyncAsync()
        {
            var snapshot = BalanceCatalog.GetSeedSnapshot();
            var db = _dbContext.Db;

            try
            {
                // 中文注释：
                // 这里不再做“删表后全量重写”。
                // 管理后台上线后，数据库中的模板数据应当成为权威源；
                // 若每次启动都按代码快照覆盖，会直接抹掉后台维护结果。
                // 但地图 / 副本模板存在一次结构性升级：
                // 1. 普通地图 NextMapId 必须为空；
                // 2. 副本层才使用 NextMapId 串联下一层；
                // 3. 地图刷怪已从 MonsterTemplateIdsJson 切到 SpawnRulesJson。
                // 因此这里保留“仅修复明确旧结构”的定向迁移，避免旧库把新规则永久挡住。
                db.Ado.BeginTran();

                if (await db.Queryable<ItemTemplateEntity>().CountAsync() == 0)
                {
                    await db.Insertable(snapshot.Items.Select(StampBuiltInItemTemplate).ToList()).ExecuteCommandAsync();
                }
                else
                {
                    await BackfillMissingItemTemplatesAsync(snapshot.Items);
                }

                await RepairLegacyPetAcquireDataAsync(snapshot);

                if (await db.Queryable<EquipmentTemplateEntity>().CountAsync() == 0)
                {
                    await db.Insertable(snapshot.EquipmentTemplates.Select(StampBuiltInEquipmentTemplate).ToList()).ExecuteCommandAsync();
                }
                else
                {
                    await BackfillMissingEquipmentTemplatesAsync(snapshot.EquipmentTemplates);
                }

                if (await db.Queryable<MonsterTemplateEntity>().CountAsync() == 0)
                {
                    await db.Insertable(snapshot.MonsterTemplates.Select(StampBuiltInMonsterTemplate).ToList()).ExecuteCommandAsync();
                }
                else
                {
                    await BackfillMissingMonsterTemplatesAsync(snapshot.MonsterTemplates);
                }

                if (await db.Queryable<MapTemplateEntity>().CountAsync() == 0)
                {
                    await db.Insertable(snapshot.MapTemplates.Select(StampBuiltInMapTemplate).ToList()).ExecuteCommandAsync();
                }
                else
                {
                    await BackfillMissingMapTemplatesAsync(snapshot.MapTemplates);
                    await RepairLegacyMapTemplatesAsync(snapshot.MapTemplates);
                }

                if (await db.Queryable<PetTemplateEntity>().CountAsync() == 0)
                {
                    await db.Insertable(snapshot.PetTemplates.Select(StampBuiltInPetTemplate).ToList()).ExecuteCommandAsync();
                }
                else
                {
                    await BackfillMissingPetTemplatesAsync(snapshot.PetTemplates);
                }

                if (await db.Queryable<DungeonTemplateEntity>().CountAsync() == 0)
                {
                    await db.Insertable(snapshot.DungeonTemplates.Select(StampBuiltInDungeonTemplate).ToList()).ExecuteCommandAsync();
                }
                else
                {
                    await BackfillMissingDungeonTemplatesAsync(snapshot.DungeonTemplates);
                    await RepairLegacyDungeonTemplatesAsync(snapshot.DungeonTemplates);
                }

                db.Ado.CommitTran();

                _logger?.LogInformation(
                    "Ensured balance catalog {Version} exists in database without overwriting existing templates: {ItemCount} items, {EquipmentCount} equipment, {MonsterCount} monsters, {MapCount} maps, {PetCount} pets, {DungeonCount} dungeons",
                    snapshot.Version,
                    snapshot.Items.Count,
                    snapshot.EquipmentTemplates.Count,
                    snapshot.MonsterTemplates.Count,
                    snapshot.MapTemplates.Count,
                    snapshot.PetTemplates.Count,
                    snapshot.DungeonTemplates.Count);
            }
            catch
            {
                // 任意一步失败都回滚，保证数据库始终保留一份完整版本。
                db.Ado.RollbackTran();
                throw;
            }
        }

        private async Task RepairLegacyMapTemplatesAsync(IReadOnlyList<MapTemplateEntity> targetMaps)
        {
            var existingMaps = await _dbContext.Db.Queryable<MapTemplateEntity>()
                .OrderBy(map => map.MapId)
                .ToListAsync();

            if (!NeedsLegacyMapRepair(existingMaps))
            {
                return;
            }

            await _dbContext.Db.Deleteable<MapTemplateEntity>().ExecuteCommandAsync();
            await _dbContext.Db.Insertable(targetMaps.ToList()).ExecuteCommandAsync();

            _logger?.LogWarning(
                "Detected legacy map template layout in SQLite. Replaced {MapCount} map templates with layered dungeon maps and SpawnRules-based seeds.",
                targetMaps.Count);
        }

        private async Task RepairLegacyDungeonTemplatesAsync(IReadOnlyList<DungeonTemplateEntity> targetDungeons)
        {
            var existingDungeons = await _dbContext.Db.Queryable<DungeonTemplateEntity>()
                .OrderBy(dungeon => dungeon.DungeonId)
                .ToListAsync();

            if (!NeedsLegacyDungeonRepair(existingDungeons))
            {
                return;
            }

            await _dbContext.Db.Deleteable<DungeonTemplateEntity>().ExecuteCommandAsync();
            await _dbContext.Db.Insertable(targetDungeons.ToList()).ExecuteCommandAsync();

            _logger?.LogWarning(
                "Detected legacy dungeon template layout in SQLite. Replaced {DungeonCount} dungeon templates to point at layered dungeon map entries.",
                targetDungeons.Count);
        }

        private async Task BackfillMissingItemTemplatesAsync(IReadOnlyList<ItemTemplateEntity> targetItems)
        {
            var existingItems = await _dbContext.Db.Queryable<ItemTemplateEntity>()
                .ToListAsync();
            var existingItemIds = existingItems
                .Select(item => item.ItemId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var missingItems = targetItems
                .Where(item => !existingItemIds.Contains(item.ItemId))
                .Select(StampBuiltInItemTemplate)
                .ToList();
            if (missingItems.Count > 0)
            {
                await _dbContext.Db.Insertable(missingItems).ExecuteCommandAsync();
                _logger?.LogInformation(
                    "Backfilled {Count} missing item templates into SQLite: {ItemIds}",
                    missingItems.Count,
                    string.Join(", ", missingItems.Select(item => item.ItemId)));
            }

            var now = DateTime.Now;
            var targetMap = targetItems.ToDictionary(item => item.ItemId, StringComparer.OrdinalIgnoreCase);
            foreach (var item in existingItems)
            {
                if (item.IsBuiltIn || !string.IsNullOrWhiteSpace(item.SeedKey))
                {
                    continue;
                }

                if (!targetMap.TryGetValue(item.ItemId, out var expected))
                {
                    continue;
                }

                if (!MatchesItemTemplate(item, expected))
                {
                    continue;
                }

                item.SeedKey = BuildItemTemplateSeedKey(item.ItemId);
                item.IsBuiltIn = true;
                item.BuiltInVersion = ItemTemplateBuiltInVersion;
                item.LastUpdateTime = now;
                await _dbContext.Db.Updateable(item)
                    .UpdateColumns(entity => new { entity.SeedKey, entity.IsBuiltIn, entity.BuiltInVersion, entity.LastUpdateTime })
                    .ExecuteCommandAsync();
            }
        }

        private async Task BackfillMissingEquipmentTemplatesAsync(IReadOnlyList<EquipmentTemplateEntity> targetEquipmentTemplates)
        {
            var existingEquipmentTemplates = await _dbContext.Db.Queryable<EquipmentTemplateEntity>()
                .ToListAsync();
            var existingEquipmentIds = existingEquipmentTemplates
                .Select(template => template.EquipmentId)
                .ToHashSet();

            var missingEquipmentTemplates = targetEquipmentTemplates
                .Where(template => !existingEquipmentIds.Contains(template.EquipmentId))
                .Select(StampBuiltInEquipmentTemplate)
                .ToList();
            if (missingEquipmentTemplates.Count > 0)
            {
                await _dbContext.Db.Insertable(missingEquipmentTemplates).ExecuteCommandAsync();
                _logger?.LogInformation(
                    "Backfilled {Count} missing equipment templates into SQLite: {EquipmentIds}",
                    missingEquipmentTemplates.Count,
                    string.Join(", ", missingEquipmentTemplates.Select(template => template.EquipmentId)));
            }

            var now = DateTime.Now;
            var targetMap = targetEquipmentTemplates.ToDictionary(template => template.EquipmentId);
            foreach (var equipmentTemplate in existingEquipmentTemplates)
            {
                if (equipmentTemplate.IsBuiltIn || !string.IsNullOrWhiteSpace(equipmentTemplate.SeedKey))
                {
                    continue;
                }

                if (!targetMap.TryGetValue(equipmentTemplate.EquipmentId, out var expected))
                {
                    continue;
                }

                if (!MatchesEquipmentTemplate(equipmentTemplate, expected))
                {
                    continue;
                }

                equipmentTemplate.SeedKey = BuildEquipmentTemplateSeedKey(equipmentTemplate.EquipmentId);
                equipmentTemplate.IsBuiltIn = true;
                equipmentTemplate.BuiltInVersion = EquipmentTemplateBuiltInVersion;
                equipmentTemplate.LastUpdateTime = now;
                await _dbContext.Db.Updateable(equipmentTemplate)
                    .UpdateColumns(entity => new { entity.SeedKey, entity.IsBuiltIn, entity.BuiltInVersion, entity.LastUpdateTime })
                    .ExecuteCommandAsync();
            }
        }

        private async Task BackfillMissingMapTemplatesAsync(IReadOnlyList<MapTemplateEntity> targetMaps)
        {
            var existingMaps = await _dbContext.Db.Queryable<MapTemplateEntity>()
                .ToListAsync();
            var existingMapIds = existingMaps
                .Select(map => map.MapId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var missingMaps = targetMaps
                .Where(map => !existingMapIds.Contains(map.MapId))
                .Select(StampBuiltInMapTemplate)
                .ToList();
            if (missingMaps.Count > 0)
            {
                await _dbContext.Db.Insertable(missingMaps).ExecuteCommandAsync();
                _logger?.LogInformation(
                    "Backfilled {Count} missing map templates into SQLite: {MapIds}",
                    missingMaps.Count,
                    string.Join(", ", missingMaps.Select(map => map.MapId)));
            }

            var now = DateTime.Now;
            var targetMap = targetMaps.ToDictionary(map => map.MapId, StringComparer.OrdinalIgnoreCase);
            foreach (var existingMap in existingMaps)
            {
                if (existingMap.IsBuiltIn || !string.IsNullOrWhiteSpace(existingMap.SeedKey))
                {
                    continue;
                }

                if (!targetMap.TryGetValue(existingMap.MapId, out var expected))
                {
                    continue;
                }

                if (!MatchesMapTemplate(existingMap, expected))
                {
                    continue;
                }

                existingMap.SeedKey = BuildMapTemplateSeedKey(existingMap.MapId);
                existingMap.IsBuiltIn = true;
                existingMap.BuiltInVersion = MapTemplateBuiltInVersion;
                existingMap.LastUpdateTime = now;
                await _dbContext.Db.Updateable(existingMap)
                    .UpdateColumns(entity => new { entity.SeedKey, entity.IsBuiltIn, entity.BuiltInVersion, entity.LastUpdateTime })
                    .ExecuteCommandAsync();
            }
        }

        private async Task BackfillMissingMonsterTemplatesAsync(IReadOnlyList<MonsterTemplateEntity> targetMonsters)
        {
            var existingMonsters = await _dbContext.Db.Queryable<MonsterTemplateEntity>()
                .ToListAsync();
            var existingMonsterIds = existingMonsters
                .Select(monster => monster.MonsterId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var missingMonsters = targetMonsters
                .Where(monster => !existingMonsterIds.Contains(monster.MonsterId))
                .Select(StampBuiltInMonsterTemplate)
                .ToList();
            if (missingMonsters.Count > 0)
            {
                await _dbContext.Db.Insertable(missingMonsters).ExecuteCommandAsync();
                _logger?.LogInformation(
                    "Backfilled {Count} missing monster templates into SQLite: {MonsterIds}",
                    missingMonsters.Count,
                    string.Join(", ", missingMonsters.Select(monster => monster.MonsterId)));
            }

            var now = DateTime.Now;
            var targetMap = targetMonsters.ToDictionary(monster => monster.MonsterId, StringComparer.OrdinalIgnoreCase);
            foreach (var existingMonster in existingMonsters)
            {
                if (existingMonster.IsBuiltIn || !string.IsNullOrWhiteSpace(existingMonster.SeedKey))
                {
                    continue;
                }

                if (!targetMap.TryGetValue(existingMonster.MonsterId, out var expected))
                {
                    continue;
                }

                if (!MatchesMonsterTemplate(existingMonster, expected))
                {
                    continue;
                }

                existingMonster.SeedKey = BuildMonsterTemplateSeedKey(existingMonster.MonsterId);
                existingMonster.IsBuiltIn = true;
                existingMonster.BuiltInVersion = MonsterTemplateBuiltInVersion;
                existingMonster.LastUpdateTime = now;
                await _dbContext.Db.Updateable(existingMonster)
                    .UpdateColumns(entity => new { entity.SeedKey, entity.IsBuiltIn, entity.BuiltInVersion, entity.LastUpdateTime })
                    .ExecuteCommandAsync();
            }
        }

        private async Task BackfillMissingDungeonTemplatesAsync(IReadOnlyList<DungeonTemplateEntity> targetDungeons)
        {
            var existingDungeons = await _dbContext.Db.Queryable<DungeonTemplateEntity>()
                .ToListAsync();
            var existingDungeonIds = existingDungeons
                .Select(dungeon => dungeon.DungeonId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var missingDungeons = targetDungeons
                .Where(dungeon => !existingDungeonIds.Contains(dungeon.DungeonId))
                .Select(StampBuiltInDungeonTemplate)
                .ToList();
            if (missingDungeons.Count > 0)
            {
                await _dbContext.Db.Insertable(missingDungeons).ExecuteCommandAsync();
                _logger?.LogInformation(
                    "Backfilled {Count} missing dungeon templates into SQLite: {DungeonIds}",
                    missingDungeons.Count,
                    string.Join(", ", missingDungeons.Select(dungeon => dungeon.DungeonId)));
            }

            var now = DateTime.Now;
            var targetMap = targetDungeons.ToDictionary(dungeon => dungeon.DungeonId, StringComparer.OrdinalIgnoreCase);
            foreach (var existingDungeon in existingDungeons)
            {
                if (existingDungeon.IsBuiltIn || !string.IsNullOrWhiteSpace(existingDungeon.SeedKey))
                {
                    continue;
                }

                if (!targetMap.TryGetValue(existingDungeon.DungeonId, out var expected))
                {
                    continue;
                }

                if (!MatchesDungeonTemplate(existingDungeon, expected))
                {
                    continue;
                }

                existingDungeon.SeedKey = BuildDungeonTemplateSeedKey(existingDungeon.DungeonId);
                existingDungeon.IsBuiltIn = true;
                existingDungeon.BuiltInVersion = DungeonTemplateBuiltInVersion;
                existingDungeon.LastUpdateTime = now;
                await _dbContext.Db.Updateable(existingDungeon)
                    .UpdateColumns(entity => new { entity.SeedKey, entity.IsBuiltIn, entity.BuiltInVersion, entity.LastUpdateTime })
                    .ExecuteCommandAsync();
            }
        }

        private async Task BackfillMissingPetTemplatesAsync(IReadOnlyList<PetTemplateEntity> targetPets)
        {
            var existingPets = await _dbContext.Db.Queryable<PetTemplateEntity>()
                .ToListAsync();
            var existingPetIds = existingPets
                .Select(pet => pet.TemplateId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var missingPets = targetPets
                .Where(pet => !existingPetIds.Contains(pet.TemplateId))
                .Select(StampBuiltInPetTemplate)
                .ToList();
            if (missingPets.Count > 0)
            {
                await _dbContext.Db.Insertable(missingPets).ExecuteCommandAsync();
                _logger?.LogInformation(
                    "Backfilled {Count} missing pet templates into SQLite: {TemplateIds}",
                    missingPets.Count,
                    string.Join(", ", missingPets.Select(pet => pet.TemplateId)));
            }

            var now = DateTime.Now;
            var targetMap = targetPets.ToDictionary(pet => pet.TemplateId, StringComparer.OrdinalIgnoreCase);
            foreach (var existingPet in existingPets)
            {
                if (!targetMap.TryGetValue(existingPet.TemplateId, out var expected))
                {
                    continue;
                }

                var builtInSeedKey = BuildPetTemplateSeedKey(existingPet.TemplateId);
                var hasBuiltInIdentity = existingPet.IsBuiltIn ||
                    string.Equals(existingPet.SeedKey, builtInSeedKey, StringComparison.OrdinalIgnoreCase);

                if (hasBuiltInIdentity)
                {
                    ApplyPetTemplateSnapshot(existingPet, expected);
                }
                else if (!MatchesPetTemplate(existingPet, expected))
                {
                    if (!LooksLikeLegacyPetTemplate(existingPet, expected))
                    {
                        continue;
                    }

                    ApplyPetTemplateSnapshot(existingPet, expected);
                }

                existingPet.SeedKey = builtInSeedKey;
                existingPet.IsBuiltIn = true;
                existingPet.BuiltInVersion = PetTemplateBuiltInVersion;
                existingPet.LastUpdateTime = now;
                await _dbContext.Db.Updateable(existingPet).ExecuteCommandAsync();
            }
        }

        /// <summary>
        /// 中文注释：
        /// 旧版本把灵宠获取入口定义为 summon_scroll。
        /// 新版本改为 pet_egg 后，需要把数据库里仍残留的旧道具、背包、商店和怪物掉落一起迁过来，
        /// 否则玩家会继续拿到已无用途的旧道具。
        /// </summary>
        private async Task RepairLegacyPetAcquireDataAsync(BalanceCatalogSnapshot snapshot)
        {
            var targetPetEggTemplate = snapshot.Items
                .FirstOrDefault(item => string.Equals(item.ItemId, PetEggItemId, StringComparison.OrdinalIgnoreCase));
            if (targetPetEggTemplate == null)
            {
                return;
            }

            await RepairLegacyItemTemplatesAsync(targetPetEggTemplate);
            await RepairLegacyInventoryItemsAsync();
            await RepairLegacyShopItemsAsync();
            await RepairLegacyShopDailyRecordsAsync();
            await RepairLegacyMonsterDropsAsync();
        }

        private async Task RepairLegacyItemTemplatesAsync(ItemTemplateEntity targetPetEggTemplate)
        {
            var itemTemplates = await _dbContext.Db.Queryable<ItemTemplateEntity>()
                .Where(item => item.ItemId == LegacySummonScrollItemId || item.ItemId == PetEggItemId)
                .ToListAsync();
            if (itemTemplates.Count == 0)
            {
                return;
            }

            var petEggTemplate = itemTemplates.FirstOrDefault(item => item.ItemId == PetEggItemId);
            var legacyTemplate = itemTemplates.FirstOrDefault(item => item.ItemId == LegacySummonScrollItemId);
            if (legacyTemplate == null)
            {
                if (petEggTemplate != null &&
                    string.IsNullOrWhiteSpace(petEggTemplate.SeedKey) &&
                    IsLegacyPetEggTemplate(petEggTemplate))
                {
                    petEggTemplate.Name = targetPetEggTemplate.Name;
                    petEggTemplate.UseLevel = targetPetEggTemplate.UseLevel;
                    petEggTemplate.Type = targetPetEggTemplate.Type;
                    petEggTemplate.Description = targetPetEggTemplate.Description;
                    petEggTemplate.MaxStack = targetPetEggTemplate.MaxStack;
                    petEggTemplate.Quality = targetPetEggTemplate.Quality;
                    petEggTemplate.IconPath = targetPetEggTemplate.IconPath;
                    petEggTemplate.SeedKey = BuildItemTemplateSeedKey(targetPetEggTemplate.ItemId);
                    petEggTemplate.IsBuiltIn = true;
                    petEggTemplate.BuiltInVersion = ItemTemplateBuiltInVersion;
                    petEggTemplate.LastUpdateTime = DateTime.Now;
                    petEggTemplate.ChestConfigJson = targetPetEggTemplate.ChestConfigJson;
                    petEggTemplate.SkillBookConfigJson = targetPetEggTemplate.SkillBookConfigJson;
                    petEggTemplate.PetEggConfigJson = targetPetEggTemplate.PetEggConfigJson;
                    petEggTemplate.PillConfigJson = targetPetEggTemplate.PillConfigJson;
                    await _dbContext.Db.Updateable(petEggTemplate)
                        .UpdateColumns(item => new
                        {
                            item.Name,
                            item.UseLevel,
                            item.Type,
                            item.Description,
                            item.MaxStack,
                            item.Quality,
                            item.IconPath,
                            item.SeedKey,
                            item.IsBuiltIn,
                            item.BuiltInVersion,
                            item.LastUpdateTime,
                            item.ChestConfigJson,
                            item.SkillBookConfigJson,
                            item.PetEggConfigJson,
                            item.PillConfigJson
                        })
                        .ExecuteCommandAsync();

                    _logger?.LogInformation("Upgraded legacy pet egg template {ItemId} to the current built-in template definition.", PetEggItemId);
                }

                return;
            }

            if (petEggTemplate == null)
            {
                legacyTemplate.ItemId = targetPetEggTemplate.ItemId;
                legacyTemplate.Name = targetPetEggTemplate.Name;
                legacyTemplate.UseLevel = targetPetEggTemplate.UseLevel;
                legacyTemplate.Type = targetPetEggTemplate.Type;
                legacyTemplate.Description = targetPetEggTemplate.Description;
                legacyTemplate.MaxStack = targetPetEggTemplate.MaxStack;
                legacyTemplate.Quality = targetPetEggTemplate.Quality;
                legacyTemplate.IconPath = targetPetEggTemplate.IconPath;
                legacyTemplate.SeedKey = BuildItemTemplateSeedKey(targetPetEggTemplate.ItemId);
                legacyTemplate.IsBuiltIn = true;
                legacyTemplate.BuiltInVersion = ItemTemplateBuiltInVersion;
                legacyTemplate.LastUpdateTime = DateTime.Now;
                legacyTemplate.ChestConfigJson = targetPetEggTemplate.ChestConfigJson;
                legacyTemplate.SkillBookConfigJson = targetPetEggTemplate.SkillBookConfigJson;
                legacyTemplate.PetEggConfigJson = targetPetEggTemplate.PetEggConfigJson;
                legacyTemplate.PillConfigJson = targetPetEggTemplate.PillConfigJson;
                await _dbContext.Db.Updateable(legacyTemplate)
                    .UpdateColumns(item => new
                    {
                        item.ItemId,
                        item.Name,
                        item.UseLevel,
                        item.Type,
                        item.Description,
                        item.MaxStack,
                        item.Quality,
                        item.IconPath,
                        item.SeedKey,
                        item.IsBuiltIn,
                        item.BuiltInVersion,
                        item.LastUpdateTime,
                        item.ChestConfigJson,
                        item.SkillBookConfigJson,
                        item.PetEggConfigJson,
                        item.PillConfigJson
                    })
                    .ExecuteCommandAsync();

                _logger?.LogInformation("Migrated legacy item template {LegacyItemId} to {NewItemId}.", LegacySummonScrollItemId, PetEggItemId);
                return;
            }

            await _dbContext.Db.Deleteable<ItemTemplateEntity>()
                .Where(item => item.ItemId == LegacySummonScrollItemId)
                .ExecuteCommandAsync();

            _logger?.LogInformation("Removed legacy item template {LegacyItemId} after {NewItemId} was present.", LegacySummonScrollItemId, PetEggItemId);
        }

        private static ItemTemplateEntity StampBuiltInItemTemplate(ItemTemplateEntity item)
        {
            item.SeedKey = BuildItemTemplateSeedKey(item.ItemId);
            item.IsBuiltIn = true;
            item.BuiltInVersion = ItemTemplateBuiltInVersion;
            item.LastUpdateTime = DateTime.Now;
            return item;
        }

        private static EquipmentTemplateEntity StampBuiltInEquipmentTemplate(EquipmentTemplateEntity equipmentTemplate)
        {
            equipmentTemplate.SeedKey = BuildEquipmentTemplateSeedKey(equipmentTemplate.EquipmentId);
            equipmentTemplate.IsBuiltIn = true;
            equipmentTemplate.BuiltInVersion = EquipmentTemplateBuiltInVersion;
            equipmentTemplate.LastUpdateTime = DateTime.Now;
            return equipmentTemplate;
        }

        private static MapTemplateEntity StampBuiltInMapTemplate(MapTemplateEntity mapTemplate)
        {
            mapTemplate.SeedKey = BuildMapTemplateSeedKey(mapTemplate.MapId);
            mapTemplate.IsBuiltIn = true;
            mapTemplate.BuiltInVersion = MapTemplateBuiltInVersion;
            mapTemplate.LastUpdateTime = DateTime.Now;
            return mapTemplate;
        }

        private static MonsterTemplateEntity StampBuiltInMonsterTemplate(MonsterTemplateEntity monsterTemplate)
        {
            monsterTemplate.SeedKey = BuildMonsterTemplateSeedKey(monsterTemplate.MonsterId);
            monsterTemplate.IsBuiltIn = true;
            monsterTemplate.BuiltInVersion = MonsterTemplateBuiltInVersion;
            monsterTemplate.LastUpdateTime = DateTime.Now;
            return monsterTemplate;
        }

        private static PetTemplateEntity StampBuiltInPetTemplate(PetTemplateEntity petTemplate)
        {
            petTemplate.SeedKey = BuildPetTemplateSeedKey(petTemplate.TemplateId);
            petTemplate.IsBuiltIn = true;
            petTemplate.BuiltInVersion = PetTemplateBuiltInVersion;
            petTemplate.LastUpdateTime = DateTime.Now;
            return petTemplate;
        }

        private static DungeonTemplateEntity StampBuiltInDungeonTemplate(DungeonTemplateEntity dungeonTemplate)
        {
            dungeonTemplate.SeedKey = BuildDungeonTemplateSeedKey(dungeonTemplate.DungeonId);
            dungeonTemplate.IsBuiltIn = true;
            dungeonTemplate.BuiltInVersion = DungeonTemplateBuiltInVersion;
            dungeonTemplate.LastUpdateTime = DateTime.Now;
            return dungeonTemplate;
        }

        private static string BuildItemTemplateSeedKey(string itemId) => $"built-in:item-template:{itemId}";

        private static string BuildEquipmentTemplateSeedKey(int equipmentId) => $"built-in:equipment-template:{equipmentId}";

        private static string BuildMapTemplateSeedKey(string mapId) => $"built-in:map-template:{mapId}";

        private static string BuildMonsterTemplateSeedKey(string monsterId) => $"built-in:monster-template:{monsterId}";

        private static string BuildPetTemplateSeedKey(string templateId) => $"built-in:pet-template:{templateId}";

        private static string BuildDungeonTemplateSeedKey(string dungeonId) => $"built-in:dungeon-template:{dungeonId}";

        private static bool MatchesItemTemplate(ItemTemplateEntity actual, ItemTemplateEntity expected)
        {
            return string.Equals(actual.Name, expected.Name, StringComparison.Ordinal) &&
                   actual.UseLevel == expected.UseLevel &&
                   actual.Type == expected.Type &&
                   string.Equals(actual.Description, expected.Description, StringComparison.Ordinal) &&
                   actual.MaxStack == expected.MaxStack &&
                   actual.Quality == expected.Quality &&
                   string.Equals(actual.IconPath ?? string.Empty, expected.IconPath ?? string.Empty, StringComparison.Ordinal);
        }

        private static bool MatchesEquipmentTemplate(EquipmentTemplateEntity actual, EquipmentTemplateEntity expected)
        {
            return string.Equals(actual.Name, expected.Name, StringComparison.Ordinal) &&
                   actual.Level == expected.Level &&
                   actual.Quality == expected.Quality &&
                   actual.Slot == expected.Slot &&
                   actual.CombatStyle == expected.CombatStyle &&
                   actual.WeaponCategory == expected.WeaponCategory &&
                   string.Equals(actual.Description, expected.Description, StringComparison.Ordinal) &&
                   string.Equals(actual.IconPath ?? string.Empty, expected.IconPath ?? string.Empty, StringComparison.Ordinal) &&
                   actual.MinType1 == expected.MinType1 &&
                   actual.MaxType1 == expected.MaxType1 &&
                   actual.MinType2 == expected.MinType2 &&
                   actual.MaxType2 == expected.MaxType2 &&
                   actual.MinType3 == expected.MinType3 &&
                   actual.MaxType3 == expected.MaxType3 &&
                   actual.MinType4 == expected.MinType4 &&
                   actual.MaxType4 == expected.MaxType4 &&
                   actual.MinType5 == expected.MinType5 &&
                   actual.MaxType5 == expected.MaxType5 &&
                   actual.MinType6 == expected.MinType6 &&
                   actual.MaxType6 == expected.MaxType6 &&
                   actual.MinType7 == expected.MinType7 &&
                   actual.MaxType7 == expected.MaxType7 &&
                   actual.MinType8 == expected.MinType8 &&
                   actual.MaxType8 == expected.MaxType8 &&
                   actual.MinType9 == expected.MinType9 &&
                   actual.MaxType9 == expected.MaxType9 &&
                   actual.MinType10 == expected.MinType10 &&
                   actual.MaxType10 == expected.MaxType10 &&
                   actual.MinType11 == expected.MinType11 &&
                   actual.MaxType11 == expected.MaxType11 &&
                   actual.MinType12 == expected.MinType12 &&
                   actual.MaxType12 == expected.MaxType12 &&
                   actual.MinType13 == expected.MinType13 &&
                   actual.MaxType13 == expected.MaxType13 &&
                   actual.MinType14 == expected.MinType14 &&
                   actual.MaxType14 == expected.MaxType14 &&
                   actual.MinType15 == expected.MinType15 &&
                   actual.MaxType15 == expected.MaxType15 &&
                   actual.Element == expected.Element;
        }

        private static bool MatchesMapTemplate(MapTemplateEntity actual, MapTemplateEntity expected)
        {
            return string.Equals(actual.Name, expected.Name, StringComparison.Ordinal) &&
                   actual.Level == expected.Level &&
                   string.Equals(actual.Description, expected.Description, StringComparison.Ordinal) &&
                   string.Equals(actual.NextMapId ?? string.Empty, expected.NextMapId ?? string.Empty, StringComparison.Ordinal) &&
                   actual.Carrying == expected.Carrying &&
                   actual.MonsterCountMin == expected.MonsterCountMin &&
                   actual.MonsterCountMax == expected.MonsterCountMax &&
                   string.Equals(actual.SpawnRulesJson ?? string.Empty, expected.SpawnRulesJson ?? string.Empty, StringComparison.Ordinal);
        }

        private static bool MatchesMonsterTemplate(MonsterTemplateEntity actual, MonsterTemplateEntity expected)
        {
            return string.Equals(actual.Name, expected.Name, StringComparison.Ordinal) &&
                   actual.Level == expected.Level &&
                   actual.ExpRewardMin == expected.ExpRewardMin &&
                   actual.ExpRewardMax == expected.ExpRewardMax &&
                   actual.GoldRewardMin == expected.GoldRewardMin &&
                   actual.GoldRewardMax == expected.GoldRewardMax &&
                   string.Equals(actual.SkillIdsJson ?? string.Empty, expected.SkillIdsJson ?? string.Empty, StringComparison.Ordinal) &&
                   string.Equals(actual.PassiveIdsJson ?? string.Empty, expected.PassiveIdsJson ?? string.Empty, StringComparison.Ordinal) &&
                   string.Equals(actual.ItemDropsJson ?? string.Empty, expected.ItemDropsJson ?? string.Empty, StringComparison.Ordinal) &&
                   string.Equals(actual.EquipmentDropsJson ?? string.Empty, expected.EquipmentDropsJson ?? string.Empty, StringComparison.Ordinal) &&
                   string.Equals(actual.ElementPoolJson ?? string.Empty, expected.ElementPoolJson ?? string.Empty, StringComparison.Ordinal) &&
                   actual.MinType1 == expected.MinType1 &&
                   actual.MaxType1 == expected.MaxType1 &&
                   actual.MinType2 == expected.MinType2 &&
                   actual.MaxType2 == expected.MaxType2 &&
                   actual.MinType3 == expected.MinType3 &&
                   actual.MaxType3 == expected.MaxType3 &&
                   actual.MinType4 == expected.MinType4 &&
                   actual.MaxType4 == expected.MaxType4 &&
                   actual.MinType5 == expected.MinType5 &&
                   actual.MaxType5 == expected.MaxType5 &&
                   actual.MinType6 == expected.MinType6 &&
                   actual.MaxType6 == expected.MaxType6 &&
                   actual.MinType7 == expected.MinType7 &&
                   actual.MaxType7 == expected.MaxType7 &&
                   actual.MinType8 == expected.MinType8 &&
                   actual.MaxType8 == expected.MaxType8 &&
                   actual.MinType9 == expected.MinType9 &&
                   actual.MaxType9 == expected.MaxType9 &&
                   actual.MinType10 == expected.MinType10 &&
                   actual.MaxType10 == expected.MaxType10 &&
                   actual.MinType11 == expected.MinType11 &&
                   actual.MaxType11 == expected.MaxType11 &&
                   actual.MinType12 == expected.MinType12 &&
                   actual.MaxType12 == expected.MaxType12 &&
                   actual.MinType13 == expected.MinType13 &&
                   actual.MaxType13 == expected.MaxType13 &&
                   actual.MinType14 == expected.MinType14 &&
                   actual.MaxType14 == expected.MaxType14 &&
                   actual.MinType15 == expected.MinType15 &&
                   actual.MaxType15 == expected.MaxType15 &&
                   actual.Element == expected.Element;
        }

        private static bool MatchesPetTemplate(PetTemplateEntity actual, PetTemplateEntity expected)
        {
            return string.Equals(actual.Name, expected.Name, StringComparison.Ordinal) &&
                   string.Equals(actual.Description ?? string.Empty, expected.Description ?? string.Empty, StringComparison.Ordinal) &&
                   actual.Type == expected.Type &&
                   actual.InitialQualityMin == expected.InitialQualityMin &&
                   actual.InitialQualityMax == expected.InitialQualityMax &&
                   actual.MaxQuality == expected.MaxQuality &&
                   actual.GrowthRateMin.Equals(expected.GrowthRateMin) &&
                   actual.GrowthRateMax.Equals(expected.GrowthRateMax) &&
                   actual.InitialSkillCount == expected.InitialSkillCount &&
                   actual.Element == expected.Element &&
                   actual.MinType1 == expected.MinType1 &&
                   actual.MaxType1 == expected.MaxType1 &&
                   actual.MinType2 == expected.MinType2 &&
                   actual.MaxType2 == expected.MaxType2 &&
                   actual.MinType3 == expected.MinType3 &&
                   actual.MaxType3 == expected.MaxType3 &&
                   actual.MinType4 == expected.MinType4 &&
                   actual.MaxType4 == expected.MaxType4 &&
                   actual.MinType5 == expected.MinType5 &&
                   actual.MaxType5 == expected.MaxType5 &&
                   actual.MinType6 == expected.MinType6 &&
                   actual.MaxType6 == expected.MaxType6 &&
                   actual.MinType7 == expected.MinType7 &&
                   actual.MaxType7 == expected.MaxType7 &&
                   actual.MinType8 == expected.MinType8 &&
                   actual.MaxType8 == expected.MaxType8 &&
                   actual.MinType9 == expected.MinType9 &&
                   actual.MaxType9 == expected.MaxType9 &&
                   actual.MinType10 == expected.MinType10 &&
                   actual.MaxType10 == expected.MaxType10 &&
                   actual.MinType11 == expected.MinType11 &&
                   actual.MaxType11 == expected.MaxType11 &&
                   actual.MinType12 == expected.MinType12 &&
                   actual.MaxType12 == expected.MaxType12 &&
                   actual.MinType13 == expected.MinType13 &&
                   actual.MaxType13 == expected.MaxType13 &&
                   actual.MinType14 == expected.MinType14 &&
                   actual.MaxType14 == expected.MaxType14 &&
                   actual.MinType15 == expected.MinType15 &&
                   actual.MaxType15 == expected.MaxType15 &&
                   string.Equals(actual.SkillIdsJson ?? string.Empty, expected.SkillIdsJson ?? string.Empty, StringComparison.Ordinal) &&
                   string.Equals(actual.ObtainMethod ?? string.Empty, expected.ObtainMethod ?? string.Empty, StringComparison.Ordinal) &&
                   actual.IsTradable == expected.IsTradable;
        }

        private static bool LooksLikeLegacyPetTemplate(PetTemplateEntity actual, PetTemplateEntity expected)
        {
            return string.Equals(actual.Name, expected.Name, StringComparison.Ordinal) &&
                   actual.Type == expected.Type &&
                   actual.InitialQualityMin == 1 &&
                   actual.InitialQualityMax == 1 &&
                   actual.GrowthRateMin.Equals(1.0) &&
                   actual.GrowthRateMax.Equals(1.0) &&
                   actual.InitialSkillCount == 1 &&
                   actual.MaxQuality == expected.MaxQuality;
        }

        private static void ApplyPetTemplateSnapshot(PetTemplateEntity target, PetTemplateEntity snapshot)
        {
            target.Name = snapshot.Name;
            target.Description = snapshot.Description;
            target.Type = snapshot.Type;
            target.InitialQualityMin = snapshot.InitialQualityMin;
            target.InitialQualityMax = snapshot.InitialQualityMax;
            target.MaxQuality = snapshot.MaxQuality;
            target.GrowthRateMin = snapshot.GrowthRateMin;
            target.GrowthRateMax = snapshot.GrowthRateMax;
            target.InitialSkillCount = snapshot.InitialSkillCount;
            target.Element = snapshot.Element;
            target.MinType1 = snapshot.MinType1;
            target.MaxType1 = snapshot.MaxType1;
            target.MinType2 = snapshot.MinType2;
            target.MaxType2 = snapshot.MaxType2;
            target.MinType3 = snapshot.MinType3;
            target.MaxType3 = snapshot.MaxType3;
            target.MinType4 = snapshot.MinType4;
            target.MaxType4 = snapshot.MaxType4;
            target.MinType5 = snapshot.MinType5;
            target.MaxType5 = snapshot.MaxType5;
            target.MinType6 = snapshot.MinType6;
            target.MaxType6 = snapshot.MaxType6;
            target.MinType7 = snapshot.MinType7;
            target.MaxType7 = snapshot.MaxType7;
            target.MinType8 = snapshot.MinType8;
            target.MaxType8 = snapshot.MaxType8;
            target.MinType9 = snapshot.MinType9;
            target.MaxType9 = snapshot.MaxType9;
            target.MinType10 = snapshot.MinType10;
            target.MaxType10 = snapshot.MaxType10;
            target.MinType11 = snapshot.MinType11;
            target.MaxType11 = snapshot.MaxType11;
            target.MinType12 = snapshot.MinType12;
            target.MaxType12 = snapshot.MaxType12;
            target.MinType13 = snapshot.MinType13;
            target.MaxType13 = snapshot.MaxType13;
            target.MinType14 = snapshot.MinType14;
            target.MaxType14 = snapshot.MaxType14;
            target.MinType15 = snapshot.MinType15;
            target.MaxType15 = snapshot.MaxType15;
            target.SkillIdsJson = snapshot.SkillIdsJson;
            target.ObtainMethod = snapshot.ObtainMethod;
            target.IsTradable = snapshot.IsTradable;
        }

        private static bool MatchesDungeonTemplate(DungeonTemplateEntity actual, DungeonTemplateEntity expected)
        {
            return string.Equals(actual.Name, expected.Name, StringComparison.Ordinal) &&
                   string.Equals(actual.Description, expected.Description, StringComparison.Ordinal) &&
                   actual.RecommendedLevel == expected.RecommendedLevel &&
                   actual.DailyLimit == expected.DailyLimit &&
                   string.Equals(actual.NormalMapId, expected.NormalMapId, StringComparison.Ordinal) &&
                   string.Equals(actual.FubenMapId, expected.FubenMapId, StringComparison.Ordinal) &&
                   actual.RequiredTeamSize == expected.RequiredTeamSize;
        }

        private static bool IsLegacyPetEggTemplate(ItemTemplateEntity item)
        {
            return string.Equals(item.ItemId, PetEggItemId, StringComparison.OrdinalIgnoreCase) &&
                   item.Type == (int)ItemType.PetEgg &&
                   string.Equals(item.Name, "宠物蛋", StringComparison.Ordinal) &&
                   string.Equals(item.Description, "使用后可随机孵化出一只灵宠。", StringComparison.Ordinal);
        }

        private async Task RepairLegacyInventoryItemsAsync()
        {
            var legacyItems = await _dbContext.Db.Queryable<InventoryItemEntity>()
                .Where(item => item.ItemId == LegacySummonScrollItemId || item.ItemId == PetEggItemId)
                .OrderBy(item => item.PlayerId)
                .OrderBy(item => item.IsLocked)
                .OrderBy(item => item.Id)
                .ToListAsync();
            if (legacyItems.Count == 0)
            {
                return;
            }

            foreach (var group in legacyItems.GroupBy(item => new { item.PlayerId, item.IsLocked }))
            {
                var totalQuantity = group.Sum(item => item.Quantity);
                var keeper = group
                    .OrderByDescending(item => string.Equals(item.ItemId, PetEggItemId, StringComparison.OrdinalIgnoreCase))
                    .ThenBy(item => item.Id)
                    .First();

                keeper.ItemId = PetEggItemId;
                keeper.Quantity = totalQuantity;
                await _dbContext.Db.Updateable(keeper)
                    .UpdateColumns(item => new { item.ItemId, item.Quantity })
                    .ExecuteCommandAsync();

                var redundantIds = group
                    .Where(item => item.Id != keeper.Id)
                    .Select(item => item.Id)
                    .ToList();
                if (redundantIds.Count > 0)
                {
                    await _dbContext.Db.Deleteable<InventoryItemEntity>()
                        .Where(item => redundantIds.Contains(item.Id))
                        .ExecuteCommandAsync();
                }
            }

            _logger?.LogInformation("Migrated legacy inventory item {LegacyItemId} to {NewItemId}.", LegacySummonScrollItemId, PetEggItemId);
        }

        private async Task RepairLegacyShopItemsAsync()
        {
            var legacyShopItems = await _dbContext.Db.Queryable<ShopItemEntity>()
                .Where(item => item.ItemType == 0 && (item.ItemId == LegacySummonScrollItemId || item.ItemId == PetEggItemId))
                .OrderBy(item => item.ShopId)
                .OrderBy(item => item.SortOrder)
                .ToListAsync();
            if (legacyShopItems.Count == 0)
            {
                return;
            }

            foreach (var group in legacyShopItems.GroupBy(item => item.ShopId))
            {
                var keeper = group
                    .OrderByDescending(item => string.Equals(item.ItemId, PetEggItemId, StringComparison.OrdinalIgnoreCase))
                    .ThenBy(item => item.SortOrder)
                    .ThenBy(item => item.GID)
                    .First();

                if (!string.Equals(keeper.ItemId, PetEggItemId, StringComparison.OrdinalIgnoreCase))
                {
                    keeper.ItemId = PetEggItemId;
                    await _dbContext.Db.Updateable(keeper)
                        .UpdateColumns(item => item.ItemId)
                        .ExecuteCommandAsync();
                }

                var redundantIds = group
                    .Where(item => !string.Equals(item.GID, keeper.GID, StringComparison.Ordinal))
                    .Select(item => item.GID)
                    .ToList();
                if (redundantIds.Count > 0)
                {
                    await _dbContext.Db.Deleteable<ShopItemEntity>()
                        .Where(item => redundantIds.Contains(item.GID))
                        .ExecuteCommandAsync();
                }
            }

            _logger?.LogInformation("Migrated legacy shop item {LegacyItemId} to {NewItemId}.", LegacySummonScrollItemId, PetEggItemId);
        }

        private async Task RepairLegacyShopDailyRecordsAsync()
        {
            var legacyRecords = await _dbContext.Db.Queryable<ShopDailyRecordEntity>()
                .Where(record => record.ItemId == LegacySummonScrollItemId || record.ItemId == PetEggItemId)
                .OrderBy(record => record.PlayerId)
                .OrderBy(record => record.ShopId)
                .OrderBy(record => record.RecordDate)
                .ToListAsync();
            if (legacyRecords.Count == 0)
            {
                return;
            }

            foreach (var group in legacyRecords.GroupBy(record => new { record.PlayerId, record.ShopId, record.RecordDate }))
            {
                var keeper = group
                    .OrderByDescending(record => string.Equals(record.ItemId, PetEggItemId, StringComparison.OrdinalIgnoreCase))
                    .ThenBy(record => record.GID)
                    .First();

                keeper.ItemId = PetEggItemId;
                keeper.PurchasedCount = group.Sum(record => record.PurchasedCount);
                keeper.LastUpdateTime = group.Max(record => record.LastUpdateTime);
                await _dbContext.Db.Updateable(keeper)
                    .UpdateColumns(record => new { record.ItemId, record.PurchasedCount, record.LastUpdateTime })
                    .ExecuteCommandAsync();

                var redundantIds = group
                    .Where(record => !string.Equals(record.GID, keeper.GID, StringComparison.Ordinal))
                    .Select(record => record.GID)
                    .ToList();
                if (redundantIds.Count > 0)
                {
                    await _dbContext.Db.Deleteable<ShopDailyRecordEntity>()
                        .Where(record => redundantIds.Contains(record.GID))
                        .ExecuteCommandAsync();
                }
            }

            _logger?.LogInformation("Migrated legacy daily shop records from {LegacyItemId} to {NewItemId}.", LegacySummonScrollItemId, PetEggItemId);
        }

        private async Task RepairLegacyMonsterDropsAsync()
        {
            var monsters = await _dbContext.Db.Queryable<MonsterTemplateEntity>()
                .Where(monster => monster.ItemDropsJson != null && monster.ItemDropsJson.Contains(LegacySummonScrollItemId))
                .ToListAsync();
            if (monsters.Count == 0)
            {
                return;
            }

            foreach (var monster in monsters)
            {
                var updated = false;
                var drops = monster.ItemDrops;
                foreach (var drop in drops)
                {
                    if (!string.Equals(drop.ItemId, LegacySummonScrollItemId, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    drop.ItemId = PetEggItemId;
                    updated = true;
                }

                if (!updated)
                {
                    continue;
                }

                monster.ItemDrops = drops;
                await _dbContext.Db.Updateable(monster)
                    .UpdateColumns(item => item.ItemDropsJson)
                    .ExecuteCommandAsync();
            }

            _logger?.LogInformation("Migrated legacy monster drops from {LegacyItemId} to {NewItemId}.", LegacySummonScrollItemId, PetEggItemId);
        }

        private static bool NeedsLegacyMapRepair(IReadOnlyList<MapTemplateEntity> existingMaps)
        {
            if (existingMaps.Count == 0)
            {
                return false;
            }

            if (existingMaps.Any(map => LegacySingleLayerDungeonMapIds.Contains(map.MapId)))
            {
                return true;
            }

            if (existingMaps.Any(map =>
                    NormalSeedMapIds.Contains(map.MapId) &&
                    !string.IsNullOrWhiteSpace(map.NextMapId)))
            {
                return true;
            }

            return existingMaps.Any(map =>
                (NormalSeedMapIds.Contains(map.MapId) || map.MapId.StartsWith("fuben_", StringComparison.OrdinalIgnoreCase)) &&
                (map.SpawnRules == null || map.SpawnRules.Count == 0));
        }

        private static bool NeedsLegacyDungeonRepair(IReadOnlyList<DungeonTemplateEntity> existingDungeons)
        {
            if (existingDungeons.Count == 0)
            {
                return false;
            }

            return existingDungeons.Any(dungeon =>
                LegacySingleLayerDungeonMapIds.Contains(dungeon.FubenMapId));
        }
    }
}
