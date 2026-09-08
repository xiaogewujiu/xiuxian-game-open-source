using XXX.Entity;

namespace XXX.Battle
{
    /// <summary>
    /// 战斗初始化器。
    /// 负责战斗开始前的参战单位初始化。
    /// </summary>
    public static class BattleInitializer
    {
        /// <summary>
        /// 随机数生成器。
        /// </summary>
        private static readonly Random Rand = Random.Shared;
        private static readonly IReadOnlyDictionary<string, PetEntity> EmptyPetLookup =
            new Dictionary<string, PetEntity>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 初始化玩家侧战斗单位。
        /// </summary>
        public static void InitializePlayerSide(BattleContext context, List<UserEntity> players)
        {
            InitializePlayerSide(context, players, null);
        }

        /// <summary>
        /// 初始化玩家侧战斗单位，并允许显式传入当前战斗的灵宠快照。
        /// </summary>
        public static void InitializePlayerSide(
            BattleContext context,
            List<UserEntity> players,
            IReadOnlyDictionary<string, PetEntity>? activePets)
        {
            InitializeUserSide(context.PlayerSide, players, true, activePets);
        }

        /// <summary>
        /// 初始化敌方玩家战斗单位，并允许显式传入当前战斗的灵宠快照。
        /// </summary>
        public static void InitializeEnemyPlayerSide(
            BattleContext context,
            List<UserEntity> players,
            IReadOnlyDictionary<string, PetEntity>? activePets)
        {
            InitializeUserSide(context.EnemySide, players, false, activePets);
        }

        private static void InitializeUserSide(
            List<BattleFighter> targetSide,
            List<UserEntity> players,
            bool isPlayerSide,
            IReadOnlyDictionary<string, PetEntity>? activePets)
        {
            var petLookup = activePets ?? EmptyPetLookup;
            foreach (var player in players)
            {
                var fighter = new BattleFighter
                {
                    Id = player.GID,
                    Name = player.Name,
                    IsPlayerSide = isPlayerSide,
                    FighterType = FighterType.Player,
                    CurrentHp = player.Type1,
                    MaxHp = player.Type1,
                    CurrentMp = player.Type2,
                    MaxMp = player.Type2
                };

                CopyAttributes(fighter, player);
                fighter.SkillIds = player.SkillIds;
                fighter.PassiveIds = player.PassiveIds;
                fighter.SkillCooldowns = [];
                fighter.ActiveBuffs = [];
                targetSide.Add(fighter);

                if (!string.IsNullOrEmpty(player.PetId) && petLookup.ContainsKey(player.PetId))
                {
                    var pet = petLookup[player.PetId];
                    var petFighter = new BattleFighter
                    {
                        Id = pet.GID,
                        Name = pet.Name,
                        IsPlayerSide = isPlayerSide,
                        FighterType = FighterType.Pet,
                        CurrentHp = pet.Type1,
                        MaxHp = pet.Type1,
                        CurrentMp = pet.Type2,
                        MaxMp = pet.Type2
                    };

                    CopyAttributes(petFighter, pet);
                    petFighter.SkillIds = [.. pet.GetSkillIds()];
                    petFighter.SkillCooldowns = [];
                    petFighter.ActiveBuffs = [];
                    targetSide.Add(petFighter);
                }
            }
        }

        /// <summary>
        /// 根据地图刷怪规则生成敌方怪物。
        /// </summary>
        public static void GenerateMonsters(BattleContext context, Map map)
        {
            if (map.SpawnRules == null || map.SpawnRules.Count == 0)
            {
                return;
            }

            var spawnRules = map.SpawnRules
                .Where(rule => rule != null &&
                    !string.IsNullOrWhiteSpace(rule.MonsterTemplateId) &&
                    rule.Weight > 0 &&
                    rule.MaxCount > 0)
                .GroupBy(rule => rule.MonsterTemplateId.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(group => new MapMonsterSpawnRule
                {
                    MonsterTemplateId = group.Key,
                    Weight = group.Sum(rule => Math.Max(0, rule.Weight)),
                    MaxCount = group.Max(rule => Math.Max(0, rule.MaxCount))
                })
                .Where(rule => GameData.MonsterTemplates.ContainsKey(rule.MonsterTemplateId))
                .ToList();

            if (spawnRules.Count == 0)
            {
                return;
            }

            var maxSpawnableCount = spawnRules.Sum(rule => rule.MaxCount);
            if (maxSpawnableCount <= 0)
            {
                return;
            }

            // 地图先决定本场总怪物数，再按权重逐只抽取。
            var requestedMonsterCount = Rand.Next(map.MonsterCount.min, map.MonsterCount.max + 1);
            var monsterCount = Math.Min(requestedMonsterCount, maxSpawnableCount);
            var spawnedCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (var index = 0; index < monsterCount; index++)
            {
                var selectableRules = spawnRules
                    .Where(rule => !spawnedCounts.TryGetValue(rule.MonsterTemplateId, out var currentCount) || currentCount < rule.MaxCount)
                    .ToList();
                if (selectableRules.Count == 0)
                {
                    break;
                }

                var selectedRule = SelectSpawnRule(selectableRules);
                var monster = CreateMonsterFromTemplate(selectedRule.MonsterTemplateId);
                if (monster == null)
                {
                    continue;
                }

                spawnedCounts[selectedRule.MonsterTemplateId] = spawnedCounts.TryGetValue(selectedRule.MonsterTemplateId, out var generatedCount)
                    ? generatedCount + 1
                    : 1;

                var fighter = new BattleFighter
                {
                    Id = monster.GID,
                    Name = monster.Name + (index + 1),
                    IsPlayerSide = false,
                    FighterType = FighterType.Monster,
                    CurrentHp = monster.Type1,
                    MaxHp = monster.Type1,
                    CurrentMp = monster.Type2,
                    MaxMp = monster.Type2,
                    MonsterTempID = monster.MonsterTempID
                };

                CopyAttributesFromMonster(fighter, monster);
                fighter.SkillIds = monster.SkillIds;
                fighter.PassiveIds = monster.PassiveIds;
                fighter.SkillCooldowns = [];
                fighter.ActiveBuffs = [];
                context.EnemySide.Add(fighter);
            }
        }

        private static MapMonsterSpawnRule SelectSpawnRule(IReadOnlyList<MapMonsterSpawnRule> spawnRules)
        {
            var totalWeight = spawnRules.Sum(rule => Math.Max(0, rule.Weight));
            if (totalWeight <= 0)
            {
                return spawnRules[Rand.Next(spawnRules.Count)];
            }

            var roll = Rand.Next(totalWeight);
            var cumulativeWeight = 0;
            foreach (var rule in spawnRules)
            {
                cumulativeWeight += Math.Max(0, rule.Weight);
                if (roll < cumulativeWeight)
                {
                    return rule;
                }
            }

            return spawnRules[^1];
        }

        private static MonsterEntity? CreateMonsterFromTemplate(string templateId)
        {
            if (string.IsNullOrWhiteSpace(templateId) ||
                !GameData.MonsterTemplates.TryGetValue(templateId, out var template))
            {
                return null;
            }

            var selectedElement = template.ElementPool.Count > 0
                ? template.ElementPool[Random.Shared.Next(0, template.ElementPool.Count)]
                : template.Element ?? Element.None;

            return new MonsterEntity
            {
                MonsterTempID = template.GID,
                GID = Guid.NewGuid().ToString("N"),
                Name = template.Name,
                Level = template.Level,
                SkillIds = template.SkillIds.ToList(),
                PassiveIds = template.PassiveIds.ToList(),
                ItemDrops = template.ItemDrops.ToList(),
                EquipmentDrops = template.EquipmentDrops.ToList(),
                ExpReward = template.ExpReward,
                GoldReward = template.GoldReward,
                Type1 = RandomRange(template.MinType1, template.MaxType1),
                Type2 = RandomRange(template.MinType2, template.MaxType2),
                Type3 = RandomRange(template.MinType3, template.MaxType3),
                Type4 = RandomRange(template.MinType4, template.MaxType4),
                Type5 = RandomRange(template.MinType5, template.MaxType5),
                Type6 = RandomRange(template.MinType6, template.MaxType6),
                Type7 = RandomRange(template.MinType7, template.MaxType7),
                Type8 = RandomRange(template.MinType8, template.MaxType8) / 100f,
                Type9 = RandomRange(template.MinType9, template.MaxType9) / 100f,
                Type10 = RandomRange(template.MinType10, template.MaxType10) / 100f,
                Type11 = RandomRange(template.MinType11, template.MaxType11) / 100f,
                Type12 = RandomRange(template.MinType12, template.MaxType12) / 100f,
                Type13 = RandomRange(template.MinType13, template.MaxType13) / 100f,
                Type14 = RandomRange(template.MinType14, template.MaxType14) / 100f,
                Type15 = RandomRange(template.MinType15, template.MaxType15) / 100f,
                Element = selectedElement
            };
        }

        private static int RandomRange(int? min, int? max)
        {
            var minValue = min ?? 0;
            var maxValue = max ?? minValue;
            if (maxValue < minValue)
            {
                maxValue = minValue;
            }

            return Random.Shared.Next(minValue, maxValue + 1);
        }

        /// <summary>
        /// 从玩家或宠物复制战斗属性。
        /// </summary>
        public static void CopyAttributes(BattleFighter fighter, BaseAttributes source)
        {
            fighter.PhysicalAttack = source.Type3;
            fighter.MagicAttack = source.Type4;
            fighter.PhysicalDefense = source.Type5;
            fighter.MagicDefense = source.Type6;
            fighter.Speed = source.Type7;
            fighter.HitRate = source.Type8;
            fighter.DodgeRate = source.Type9;
            fighter.CritRate = source.Type10;
            fighter.CritDamage = source.Type11;
            fighter.ComboRate = source.Type12;
            fighter.CounterRate = source.Type13;
            fighter.ArmorBreak = source.Type14;
            fighter.ExtraDamage = source.Type15;
            fighter.Element = source.Element;
        }

        /// <summary>
        /// 从怪物实体复制战斗属性。
        /// </summary>
        public static void CopyAttributesFromMonster(BattleFighter fighter, MonsterEntity monster)
        {
            CopyAttributes(fighter, monster);
        }
    }
}
