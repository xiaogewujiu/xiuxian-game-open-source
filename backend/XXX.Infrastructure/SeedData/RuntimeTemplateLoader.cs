using Microsoft.Extensions.Logging;
using XXX.Dungeon;
using XXX.Entity;
using XXX.Infrastructure.Data;
using XXX.Player;

namespace XXX.Infrastructure.SeedData
{
    /// <summary>
    /// 从数据库装载运行时模板缓存。
    /// </summary>
    public class RuntimeTemplateLoader : IRuntimeTemplateLoader
    {
        private readonly DbContext _dbContext;
        private readonly ILogger<RuntimeTemplateLoader>? _logger;

        /// <summary>
        /// 初始化运行时模板加载器。
        /// </summary>
        /// <param name="dbContext">数据库上下文。</param>
        /// <param name="logger">日志记录器。</param>
        public RuntimeTemplateLoader(DbContext dbContext, ILogger<RuntimeTemplateLoader>? logger = null)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// 从数据库加载运行时模板并刷新内存缓存。
        /// </summary>
        public async Task LoadAsync()
        {
            var items = await _dbContext.Db.Queryable<ItemTemplateEntity>()
                .OrderBy(item => item.ItemId)
                .ToListAsync();
            var chestConfigs = await _dbContext.Db.Queryable<ItemChestConfigEntity>()
                .Where(item => item.IsEnabled)
                .ToListAsync();
            var chestRewardEntries = await _dbContext.Db.Queryable<ItemChestRewardEntryEntity>()
                .OrderBy(item => item.ItemId)
                .OrderBy(item => item.SortOrder)
                .ToListAsync();
            var skillBookConfigs = await _dbContext.Db.Queryable<ItemSkillBookConfigEntity>()
                .Where(item => item.IsEnabled)
                .ToListAsync();
            var petEggConfigs = await _dbContext.Db.Queryable<ItemPetEggConfigEntity>()
                .Where(item => item.IsEnabled)
                .ToListAsync();
            var pillConfigs = await _dbContext.Db.Queryable<ItemPillConfigEntity>()
                .Where(item => item.IsEnabled)
                .ToListAsync();
            var equipmentTemplates = await _dbContext.Db.Queryable<EquipmentTemplateEntity>()
                .OrderBy(template => template.EquipmentId)
                .ToListAsync();
            var monsterTemplates = await _dbContext.Db.Queryable<MonsterTemplateEntity>()
                .OrderBy(template => template.MonsterId)
                .ToListAsync();
            var mapTemplates = await _dbContext.Db.Queryable<MapTemplateEntity>()
                .OrderBy(template => template.MapId)
                .ToListAsync();
            var skillTemplates = await _dbContext.Db.Queryable<SkillTemplateEntity>()
                .OrderBy(template => template.SkillId)
                .ToListAsync();
            var buffTemplates = await _dbContext.Db.Queryable<BuffTemplateEntity>()
                .OrderBy(template => template.BuffId)
                .ToListAsync();
            var dungeonTemplates = await _dbContext.Db.Queryable<DungeonTemplateEntity>()
                .OrderBy(template => template.DungeonId)
                .ToListAsync();
            var dungeonInstanceTemplates = await _dbContext.Db.Queryable<DungeonInstanceTemplateEntity>()
                .Where(t => t.Enabled)
                .OrderBy(t => t.Id)
                .ToListAsync();
            var dungeonEventConfigs = await _dbContext.Db.Queryable<DungeonEventConfigEntity>()
                .Where(e => e.Enabled)
                .OrderBy(e => e.Id)
                .ToListAsync();
            var dungeonEventGroups = await _dbContext.Db.Queryable<DungeonEventGroupEntity>()
                .OrderBy(g => g.Id)
                .ToListAsync();
            var petTemplateCount = await _dbContext.Db.Queryable<PetTemplateEntity>().CountAsync();

            ValidateRequiredTemplates(items.Count, equipmentTemplates.Count, monsterTemplates.Count, mapTemplates.Count, skillTemplates.Count, buffTemplates.Count, dungeonTemplates.Count, petTemplateCount);

            XXX.GameData.ClearRuntimeCaches();
            XXX.SkillData.ClearRuntimeCaches();
            XXX.BuffDataTemplates.ClearRuntimeCaches();
            DungeonCatalog.ClearRuntimeEntries();

            var chestConfigMap = chestConfigs.ToDictionary(item => item.ItemId, StringComparer.OrdinalIgnoreCase);
            var chestRewardMap = chestRewardEntries
                .GroupBy(item => item.ItemId, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.OrderBy(item => item.SortOrder).ToList(), StringComparer.OrdinalIgnoreCase);
            var skillBookConfigMap = skillBookConfigs.ToDictionary(item => item.ItemId, StringComparer.OrdinalIgnoreCase);
            var petEggConfigMap = petEggConfigs.ToDictionary(item => item.ItemId, StringComparer.OrdinalIgnoreCase);
            var pillConfigMap = pillConfigs.ToDictionary(item => item.ItemId, StringComparer.OrdinalIgnoreCase);
            XXX.GameData.Items = items.ToDictionary(
                item => item.ItemId,
                item =>
                {
                    var runtime = TemplateDataMapper.ToRuntime(item);
                    if (chestConfigMap.TryGetValue(item.ItemId, out var chestConfig) &&
                        chestRewardMap.TryGetValue(item.ItemId, out var chestRewards) &&
                        chestRewards.Count > 0)
                    {
                        runtime.ChestConfig = new ItemChestConfig
                        {
                            OpenMode = Enum.IsDefined(typeof(ChestOpenMode), chestConfig.OpenMode)
                                ? (ChestOpenMode)chestConfig.OpenMode
                                : ChestOpenMode.SinglePick,
                            RollCount = Math.Max(1, chestConfig.RollCount),
                            Rewards = chestRewards.Select(reward => new ItemChestRewardEntry
                            {
                                RewardType = Enum.IsDefined(typeof(ChestRewardType), reward.RewardType)
                                    ? (ChestRewardType)reward.RewardType
                                    : ChestRewardType.Item,
                                TargetId = reward.TargetId,
                                MinCount = reward.MinCount,
                                MaxCount = reward.MaxCount,
                                Weight = reward.Weight,
                                Description = reward.Description
                            }).ToList()
                        };
                    }

                    if (skillBookConfigMap.TryGetValue(item.ItemId, out var skillBookConfig))
                    {
                        runtime.SkillBookConfig = new ItemSkillBookConfig
                        {
                            SkillId = skillBookConfig.SkillId
                        };
                    }

                    if (petEggConfigMap.TryGetValue(item.ItemId, out var petEggConfig))
                    {
                        runtime.PetEggConfig = new ItemPetEggConfig
                        {
                            PetTemplateId = petEggConfig.PetTemplateId
                        };
                    }

                    if (pillConfigMap.TryGetValue(item.ItemId, out var pillConfig))
                    {
                        runtime.PillConfig = new ItemPillConfig
                        {
                            EffectType = Enum.IsDefined(typeof(ItemPillEffectType), pillConfig.EffectType)
                                ? (ItemPillEffectType)pillConfig.EffectType
                                : ItemPillEffectType.AddExp,
                            BreakthroughBonusPercent = pillConfig.BreakthroughBonusPercent,
                            ExpGain = pillConfig.ExpGain,
                            AttributeType = !string.IsNullOrWhiteSpace(pillConfig.AttributeType) &&
                                Enum.TryParse<AttributeType>(pillConfig.AttributeType, true, out var parsedAttributeType)
                                ? parsedAttributeType
                                : null,
                            AttributeValue = pillConfig.AttributeValue,
                            DurationMinutes = pillConfig.DurationMinutes,
                            MaxUsageCount = pillConfig.MaxUsageCount
                        };
                    }

                    return runtime;
                },
                StringComparer.OrdinalIgnoreCase);
            XXX.GameData.EquipmentTemplates = equipmentTemplates.ToDictionary(
                template => template.EquipmentId,
                TemplateDataMapper.ToRuntime);
            XXX.GameData.MonsterTemplates = monsterTemplates.ToDictionary(
                template => template.MonsterId,
                TemplateDataMapper.ToRuntime,
                StringComparer.OrdinalIgnoreCase);
            XXX.GameData.Maps = mapTemplates.ToDictionary(
                template => template.MapId,
                TemplateDataMapper.ToRuntime,
                StringComparer.OrdinalIgnoreCase);
            XXX.SkillData.Skills = skillTemplates.ToDictionary(
                template => template.SkillId,
                TemplateDataMapper.ToRuntime);
            XXX.BuffDataTemplates.BuffTemplates = buffTemplates.ToDictionary(
                template => template.BuffId,
                TemplateDataMapper.ToRuntime,
                StringComparer.OrdinalIgnoreCase);
            DungeonCatalog.ReplaceEntries(dungeonTemplates.Select(TemplateDataMapper.ToRuntime));

            XXX.GameData.DungeonInstanceTemplates = dungeonInstanceTemplates.ToDictionary(
                t => t.Id,
                StringComparer.OrdinalIgnoreCase);
            XXX.GameData.DungeonEventConfigsById = dungeonEventConfigs.ToDictionary(
                e => e.Id,
                StringComparer.OrdinalIgnoreCase);
            XXX.GameData.DungeonEventGroups = dungeonEventGroups.ToDictionary(
                g => g.Id,
                StringComparer.OrdinalIgnoreCase);
            XXX.GameData.DungeonEventConfigsByDungeonAndType = dungeonEventConfigs
                .SelectMany(e =>
                {
                    var entries = new List<(string Key, DungeonEventConfigEntity Event)>
                    {
                        ($"{e.DungeonId}:{e.EventType}", e)
                    };
                    if (!string.Equals(e.DungeonId, "*", StringComparison.OrdinalIgnoreCase))
                    {
                        entries.Add(($"*:{e.EventType}", e));
                    }
                    return entries;
                })
                .GroupBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.Event).ToList(),
                    StringComparer.OrdinalIgnoreCase);
            _logger?.LogInformation(
                "Loaded runtime templates from database: {ItemCount} items, {EquipmentCount} equipment, {MonsterCount} monsters, {MapCount} maps, {SkillCount} skills, {BuffCount} buffs, {DungeonCount} dungeons, {DungeonInstanceCount} dungeon instances, {DungeonEventCount} dungeon events, {DungeonEventGroupCount} event groups, {PetCount} pet templates",
                items.Count,
                equipmentTemplates.Count,
                monsterTemplates.Count,
                mapTemplates.Count,
                skillTemplates.Count,
                buffTemplates.Count,
                dungeonTemplates.Count,
                dungeonInstanceTemplates.Count,
                dungeonEventConfigs.Count,
                dungeonEventGroups.Count,
                petTemplateCount);
        }

        private static void ValidateRequiredTemplates(
            int itemCount,
            int equipmentCount,
            int monsterCount,
            int mapCount,
            int skillCount,
            int buffCount,
            int dungeonCount,
            int petCount)
        {
            var missing = new List<string>();
            if (itemCount == 0)
            {
                missing.Add("ItemTemplates");
            }

            if (equipmentCount == 0)
            {
                missing.Add("EquipmentTemplates");
            }

            if (monsterCount == 0)
            {
                missing.Add("MonsterTemplates");
            }

            if (mapCount == 0)
            {
                missing.Add("MapTemplates");
            }

            if (skillCount == 0)
            {
                missing.Add("SkillTemplates");
            }

            if (buffCount == 0)
            {
                missing.Add("BuffTemplates");
            }

            if (dungeonCount == 0)
            {
                missing.Add("DungeonTemplates");
            }

            if (petCount == 0)
            {
                missing.Add("PetTemplates");
            }

            if (missing.Count > 0)
            {
                throw new InvalidOperationException($"Runtime template loading failed, missing required tables or rows: {string.Join(", ", missing)}");
            }
        }
    }
}
