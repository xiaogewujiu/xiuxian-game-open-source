using XXX.Dungeon;
using XXX.Entity;

namespace XXX.Infrastructure.SeedData
{
    /// <summary>
    /// 运行时模板与数据库模板实体之间的双向映射工具。
    /// </summary>
    internal static class TemplateDataMapper
    {
        /// <summary>
        /// 将运行时道具模板转换为数据库实体。
        /// </summary>
        public static ItemTemplateEntity ToEntity(ItemTable item)
        {
            return new ItemTemplateEntity
            {
                ItemId = item.ItemId,
                Name = item.Name,
                UseLevel = item.UseLevel,
                Type = (int)item.Type,
                Description = item.Description,
                MaxStack = item.MaxStack <= 0 ? 1 : item.MaxStack,
                Quality = item.Quality <= 0 ? 1 : item.Quality,
                IconPath = string.IsNullOrWhiteSpace(item.IconPath) ? null : item.IconPath.Trim(),
                ChestConfig = item.ChestConfig,
                SkillBookConfig = item.SkillBookConfig,
                PetEggConfig = item.PetEggConfig,
                PillConfig = item.PillConfig,
                FavorabilityGiftConfig = item.FavorabilityGiftConfig
            };
        }

        /// <summary>
        /// 将数据库道具模板还原为运行时结构。
        /// </summary>
        public static ItemTable ToRuntime(ItemTemplateEntity entity)
        {
            return new ItemTable
            {
                ItemId = entity.ItemId,
                Name = entity.Name,
                UseLevel = entity.UseLevel,
                Type = Enum.IsDefined(typeof(ItemType), entity.Type) ? (ItemType)entity.Type : ItemType.Material,
                Description = entity.Description,
                MaxStack = entity.MaxStack <= 0 ? 1 : entity.MaxStack,
                Quality = entity.Quality <= 0 ? 1 : entity.Quality,
                IconPath = string.IsNullOrWhiteSpace(entity.IconPath) ? null : entity.IconPath.Trim(),
                ChestConfig = entity.ChestConfig,
                SkillBookConfig = entity.SkillBookConfig,
                PetEggConfig = entity.PetEggConfig,
                PillConfig = entity.PillConfig,
                FavorabilityGiftConfig = entity.FavorabilityGiftConfig
            };
        }

        /// <summary>
        /// 将运行时装备模板转换为数据库实体。
        /// </summary>
        public static EquipmentTemplateEntity ToEntity(EquipmentTemplate template)
        {
            return CopyAttributes(template, new EquipmentTemplateEntity
            {
                EquipmentId = template.EquipmentId,
                Name = template.Name,
                Level = template.Level,
                Quality = (int)template.Quality,
                Slot = (int)template.Slot,
                CombatStyle = (int)template.CombatStyle,
                WeaponCategory = (int)template.WeaponCategory,
                Description = template.Description,
                IconPath = string.IsNullOrWhiteSpace(template.IconPath) ? null : template.IconPath.Trim()
            });
        }

        /// <summary>
        /// 将数据库装备模板还原为运行时结构。
        /// </summary>
        public static EquipmentTemplate ToRuntime(EquipmentTemplateEntity entity)
        {
            return CopyAttributes(entity, new EquipmentTemplate
            {
                EquipmentId = entity.EquipmentId,
                Name = entity.Name,
                Level = entity.Level,
                Quality = Enum.IsDefined(typeof(EquipmentQuality), entity.Quality)
                    ? (EquipmentQuality)entity.Quality
                    : EquipmentQuality.Common,
                Slot = Enum.IsDefined(typeof(EquipmentSlot), entity.Slot)
                    ? (EquipmentSlot)entity.Slot
                    : EquipmentSlot.Weapon,
                CombatStyle = Enum.IsDefined(typeof(CombatStyle), entity.CombatStyle)
                    ? (CombatStyle)entity.CombatStyle
                    : CombatStyle.Neutral,
                WeaponCategory = Enum.IsDefined(typeof(WeaponCategory), entity.WeaponCategory)
                    ? (WeaponCategory)entity.WeaponCategory
                    : WeaponCategory.None,
                Description = entity.Description,
                IconPath = string.IsNullOrWhiteSpace(entity.IconPath) ? null : entity.IconPath.Trim()
            });
        }

        /// <summary>
        /// 将运行时怪物模板转换为数据库实体。
        /// </summary>
        public static MonsterTemplateEntity ToEntity(MonsterTemplate template)
        {
            var entity = CopyAttributes(template, new MonsterTemplateEntity
            {
                MonsterId = template.GID,
                Name = template.Name,
                Level = int.TryParse(template.Level, out var parsedLevel) ? parsedLevel : 1,
                SkillIds = template.SkillIds ?? [],
                PassiveIds = template.PassiveIds ?? [],
                ItemDrops = template.ItemDrops ?? [],
                EquipmentDrops = template.EquipmentDrops ?? [],
                CollectionDrops = template.CollectionDrops ?? [],
                ExpRewardMin = template.ExpReward.min,
                ExpRewardMax = template.ExpReward.max,
                GoldRewardMin = template.GoldReward.min,
                GoldRewardMax = template.GoldReward.max
            });

            entity.ElementPool = template.ElementPool?.Count > 0
                ? template.ElementPool
                : template.Element.HasValue && template.Element.Value != Element.None
                    ? [template.Element.Value]
                    : [];

            return entity;
        }

        /// <summary>
        /// 将数据库怪物模板还原为运行时结构。
        /// </summary>
        public static MonsterTemplate ToRuntime(MonsterTemplateEntity entity)
        {
            var template = CopyAttributes(entity, new MonsterTemplate
            {
                GID = entity.MonsterId,
                Name = entity.Name,
                Level = entity.Level.ToString(),
                SkillIds = entity.SkillIds ?? [],
                PassiveIds = entity.PassiveIds ?? [],
                ItemDrops = entity.ItemDrops ?? [],
                EquipmentDrops = entity.EquipmentDrops ?? [],
                CollectionDrops = entity.CollectionDrops ?? [],
                ExpReward = (entity.ExpRewardMin, entity.ExpRewardMax),
                GoldReward = (entity.GoldRewardMin, entity.GoldRewardMax)
            });

            template.ElementPool = entity.ElementPool?.Count > 0
                ? entity.ElementPool
                : entity.Element.HasValue && entity.Element.Value != Element.None
                    ? [entity.Element.Value]
                    : [];

            return template;
        }

        /// <summary>
        /// 将运行时地图模板转换为数据库实体。
        /// </summary>
        public static MapTemplateEntity ToEntity(Map map)
        {
            return new MapTemplateEntity
            {
                MapId = map.MapGId,
                Name = map.Name,
                Level = map.Level,
                Description = map.Description,
                NextMapId = map.NextMapId,
                Carrying = map.Carrying,
                MonsterCountMin = map.MonsterCount.min,
                MonsterCountMax = map.MonsterCount.max,
                SpawnRules = map.SpawnRules ?? []
            };
        }

        /// <summary>
        /// 将数据库地图模板还原为运行时结构。
        /// </summary>
        public static Map ToRuntime(MapTemplateEntity entity)
        {
            return new Map
            {
                MapGId = entity.MapId,
                Name = entity.Name,
                Level = entity.Level,
                Description = entity.Description,
                NextMapId = entity.NextMapId,
                Carrying = entity.Carrying,
                MonsterCount = (entity.MonsterCountMin, entity.MonsterCountMax),
                SpawnRules = entity.SpawnRules ?? []
            };
        }

        /// <summary>
        /// 将运行时技能模板转换为数据库实体。
        /// </summary>
        public static SkillTemplateEntity ToEntity(Skill skill)
        {
            return new SkillTemplateEntity
            {
                SkillId = skill.Id,
                Name = skill.Name,
                Description = skill.Description,
                TargetType = skill.TargetType,
                ManaCost = skill.ManaCost,
                Cooldown = skill.Cooldown,
                DamageType = (int)skill.DamageType,
                HitCount = skill.HitCount,
                RangeType = skill.RangeType,
                DamageMultiplier = skill.DamageMultiplier,
                TriggerChance = skill.TriggerChance,
                Hits = skill.Hits ?? [],
                BuffIds = skill.BuffIds ?? [],
                AllowedProfessions = skill.AllowedProfessions ?? [PlayerProfessionCatalog.All]
            };
        }

        /// <summary>
        /// 将数据库技能模板还原为运行时结构。
        /// </summary>
        public static Skill ToRuntime(SkillTemplateEntity entity)
        {
            return new Skill
            {
                Id = entity.SkillId,
                Name = entity.Name,
                Description = entity.Description,
                TargetType = entity.TargetType,
                ManaCost = entity.ManaCost,
                Cooldown = entity.Cooldown,
                DamageType = Enum.IsDefined(typeof(DamageType), entity.DamageType)
                    ? (DamageType)entity.DamageType
                    : DamageType.Physical,
                HitCount = entity.HitCount,
                Hits = entity.Hits ?? [],
                RangeType = entity.RangeType,
                DamageMultiplier = entity.DamageMultiplier,
                BuffIds = entity.BuffIds ?? [],
                AllowedProfessions = entity.AllowedProfessions ?? [PlayerProfessionCatalog.All],
                TriggerChance = entity.TriggerChance
            };
        }

        /// <summary>
        /// 将运行时 Buff 模板转换为数据库实体。
        /// </summary>
        public static BuffTemplateEntity ToEntity(BuffTemplate template)
        {
            return new BuffTemplateEntity
            {
                BuffId = template.Gid,
                Name = template.Name,
                Description = template.Description,
                Duration = template.Duration,
                MaxStack = template.MaxStack,
                StackRule = (int)template.StackRule,
                Effects = template.Effects ?? []
            };
        }

        /// <summary>
        /// 将数据库 Buff 模板还原为运行时结构。
        /// </summary>
        public static BuffTemplate ToRuntime(BuffTemplateEntity entity)
        {
            return new BuffTemplate
            {
                Gid = entity.BuffId,
                Name = entity.Name,
                Description = entity.Description,
                Duration = entity.Duration,
                MaxStack = entity.MaxStack,
                StackRule = Enum.IsDefined(typeof(StackRule), entity.StackRule)
                    ? (StackRule)entity.StackRule
                    : StackRule.Replace,
                Effects = entity.Effects ?? []
            };
        }

        /// <summary>
        /// 将副本目录条目转换为数据库实体。
        /// </summary>
        public static DungeonTemplateEntity ToEntity(DungeonCatalogEntry entry)
        {
            return new DungeonTemplateEntity
            {
                DungeonId = entry.DungeonId,
                Name = entry.Name,
                Description = entry.Description,
                RecommendedLevel = entry.RecommendedLevel,
                DailyLimit = entry.DailyLimit,
                NormalMapId = entry.NormalMapId,
                FubenMapId = entry.FubenMapId,
                RequiredTeamSize = entry.RequiredTeamSize
            };
        }

        /// <summary>
        /// 将数据库副本实体还原为运行时目录条目。
        /// </summary>
        public static DungeonCatalogEntry ToRuntime(DungeonTemplateEntity entity)
        {
            return new DungeonCatalogEntry(
                entity.DungeonId,
                entity.Name,
                entity.Description,
                entity.RecommendedLevel,
                entity.DailyLimit,
                entity.NormalMapId,
                entity.FubenMapId,
                entity.RequiredTeamSize);
        }

        /// <summary>
        /// 复制基础属性区间，避免运行时模板和数据库实体分别写两套字段映射。
        /// </summary>
        private static TTarget CopyAttributes<TTarget>(BaseAttributesTemplate source, TTarget target)
            where TTarget : BaseAttributesTemplate
        {
            target.MinType1 = source.MinType1;
            target.MaxType1 = source.MaxType1;
            target.MinType2 = source.MinType2;
            target.MaxType2 = source.MaxType2;
            target.MinType3 = source.MinType3;
            target.MaxType3 = source.MaxType3;
            target.MinType4 = source.MinType4;
            target.MaxType4 = source.MaxType4;
            target.MinType5 = source.MinType5;
            target.MaxType5 = source.MaxType5;
            target.MinType6 = source.MinType6;
            target.MaxType6 = source.MaxType6;
            target.MinType7 = source.MinType7;
            target.MaxType7 = source.MaxType7;
            target.MinType8 = source.MinType8;
            target.MaxType8 = source.MaxType8;
            target.MinType9 = source.MinType9;
            target.MaxType9 = source.MaxType9;
            target.MinType10 = source.MinType10;
            target.MaxType10 = source.MaxType10;
            target.MinType11 = source.MinType11;
            target.MaxType11 = source.MaxType11;
            target.MinType12 = source.MinType12;
            target.MaxType12 = source.MaxType12;
            target.MinType13 = source.MinType13;
            target.MaxType13 = source.MaxType13;
            target.MinType14 = source.MinType14;
            target.MaxType14 = source.MaxType14;
            target.MinType15 = source.MinType15;
            target.MaxType15 = source.MaxType15;
            target.Element = source.Element;
            return target;
        }
    }
}
