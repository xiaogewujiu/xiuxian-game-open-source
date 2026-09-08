using System.Text.Json;
using XXX.Entity;

namespace XXX.Achievement
{
    /// <summary>
    /// 成就内置补种上下文。
    /// </summary>
    public sealed class AchievementSeedContext
    {
        public string StarterMonsterId { get; init; } = "mon_s01_main_01";

        public string StarterMonsterName { get; init; } = "青岚山兔";

        public string StarterDungeonId { get; init; } = "dungeon_s01_01";

        public string StarterDungeonName { get; init; } = "青岚问道窟";

        public string StarterCropId { get; init; } = "crop_s01_herb";

        public string StarterCropName { get; init; } = "凝血青芽";

        public string StarterSkillId { get; init; } = "1001";

        public string StarterSkillName { get; init; } = "破军式";

        public string StarterAlchemyRecipeId { get; init; } = "alchemy_baseline_001";

        public string StarterAlchemyRecipeName { get; init; } = "烈阳增元";

    }

    /// <summary>
    /// 默认内置成就目录。
    /// </summary>
    public static class AchievementSeedCatalog
    {
        public const string BuiltInVersion = "achievement-built-in-v1-20260403";

        public static readonly string[] DefaultAchievementIds =
        [
            "pure_ach_001",
            "pure_ach_002",
            "pure_ach_003",
            "pure_ach_004",
            "pure_ach_005",
            "pure_ach_006",
            "pure_ach_007",
            "pure_ach_008",
            "pure_ach_009",
            "pure_ach_010",
            "pure_ach_011",
            "pure_ach_012",
            "pure_ach_013",
            "pure_ach_014",
            "pure_ach_015",
            "pure_ach_016",
            "pure_ach_017",
            "pure_ach_018",
            "pure_ach_019",
            "pure_ach_020",
            "pure_ach_021",
            "pure_ach_022",
            "pure_ach_023",
            "pure_ach_024",
            "pure_ach_025"
        ];

        /// <summary>
        /// 构造默认内置成就实体。
        /// </summary>
        public static List<AchievementConfigEntity> BuildDefaultAchievementEntities(AchievementSeedContext context, DateTime? now = null)
        {
            var lastUpdateTime = now ?? DateTime.Now;

            return
            [
                CreateDefaultAchievementEntity(
                    lastUpdateTime,
                    achievementId: "ach_level_10",
                    achievementName: "初出茅庐",
                    achievementType: AchievementType.Level,
                    difficulty: AchievementDifficulty.Easy,
                    description: "角色达到 10 级。",
                    category: "成长",
                    sortOrder: 1,
                    requirements:
                    [
                        BuildRequirement(AchievementRequirementType.ReachLevel, 10, "达到 10 级")
                    ],
                    rewardGold: 1200,
                    rewardItems: new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) { ["item_003"] = 3 }),
                CreateDefaultAchievementEntity(
                    lastUpdateTime,
                    achievementId: "ach_damage_total_10000",
                    achievementName: "攻势如潮",
                    achievementType: AchievementType.Battle,
                    difficulty: AchievementDifficulty.Easy,
                    description: "累计造成 10000 点伤害。",
                    category: "战斗",
                    sortOrder: 10,
                    requirements:
                    [
                        BuildRequirement(AchievementRequirementType.TotalDamageDealt, 10000, "累计造成 10000 点伤害")
                    ],
                    rewardGold: 1800),
                CreateDefaultAchievementEntity(
                    lastUpdateTime,
                    achievementId: "ach_damage_taken_5000",
                    achievementName: "百炼成钢",
                    achievementType: AchievementType.Battle,
                    difficulty: AchievementDifficulty.Easy,
                    description: "累计承受 5000 点伤害。",
                    category: "战斗",
                    sortOrder: 11,
                    requirements:
                    [
                        BuildRequirement(AchievementRequirementType.TotalDamageTaken, 5000, "累计承受 5000 点伤害")
                    ],
                    rewardGold: 1500),
                CreateDefaultAchievementEntity(
                    lastUpdateTime,
                    achievementId: "ach_alchemy_success_10",
                    achievementName: "丹道初成",
                    achievementType: AchievementType.Activity,
                    difficulty: AchievementDifficulty.Easy,
                    description: "累计炼丹成功 10 次。",
                    category: "炼丹",
                    sortOrder: 20,
                    requirements:
                    [
                        BuildRequirement(AchievementRequirementType.AlchemySuccessCount, 10, "累计炼丹成功 10 次")
                    ],
                    rewardGold: 2000,
                    rewardItems: new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) { ["alchemy_herb"] = 5 }),
                CreateDefaultAchievementEntity(
                    lastUpdateTime,
                    achievementId: "ach_alchemy_failure_5",
                    achievementName: "丹炉尚温",
                    achievementType: AchievementType.Activity,
                    difficulty: AchievementDifficulty.Easy,
                    description: "累计炼丹失败 5 次。",
                    category: "炼丹",
                    sortOrder: 21,
                    requirements:
                    [
                        BuildRequirement(AchievementRequirementType.AlchemyFailureCount, 5, "累计炼丹失败 5 次")
                    ],
                    rewardGold: 1200),
                CreateDefaultAchievementEntity(
                    lastUpdateTime,
                    achievementId: "ach_forge_success_10",
                    achievementName: "百锤成器",
                    achievementType: AchievementType.Equipment,
                    difficulty: AchievementDifficulty.Easy,
                    description: "累计锻造成功 10 次。",
                    category: "锻造",
                    sortOrder: 30,
                    requirements:
                    [
                        BuildRequirement(AchievementRequirementType.ForgeSuccessCount, 10, "累计锻造成功 10 次")
                    ],
                    rewardGold: 2200),
                CreateDefaultAchievementEntity(
                    lastUpdateTime,
                    achievementId: "ach_forge_failure_5",
                    achievementName: "炉火未熄",
                    achievementType: AchievementType.Equipment,
                    difficulty: AchievementDifficulty.Easy,
                    description: "累计锻造失败 5 次。",
                    category: "锻造",
                    sortOrder: 31,
                    requirements:
                    [
                        BuildRequirement(AchievementRequirementType.ForgeFailureCount, 5, "累计锻造失败 5 次")
                    ],
                    rewardGold: 1200),
                CreateDefaultAchievementEntity(
                    lastUpdateTime,
                    achievementId: "ach_enhance_success_10",
                    achievementName: "灵光渐盛",
                    achievementType: AchievementType.Equipment,
                    difficulty: AchievementDifficulty.Easy,
                    description: "累计强化成功 10 次。",
                    category: "强化",
                    sortOrder: 40,
                    requirements:
                    [
                        BuildRequirement(AchievementRequirementType.EnhanceSuccessCount, 10, "累计强化成功 10 次")
                    ],
                    rewardGold: 2600),
                CreateDefaultAchievementEntity(
                    lastUpdateTime,
                    achievementId: "ach_enhance_failure_5",
                    achievementName: "锻心之路",
                    achievementType: AchievementType.Equipment,
                    difficulty: AchievementDifficulty.Easy,
                    description: "累计强化失败 5 次。",
                    category: "强化",
                    sortOrder: 41,
                    requirements:
                    [
                        BuildRequirement(AchievementRequirementType.EnhanceFailureCount, 5, "累计强化失败 5 次")
                    ],
                    rewardGold: 1500),
                CreateDefaultAchievementEntity(
                    lastUpdateTime,
                    achievementId: "ach_plant_crop_10",
                    achievementName: "播种者",
                    achievementType: AchievementType.Collection,
                    difficulty: AchievementDifficulty.Easy,
                    description: $"累计种植 {context.StarterCropName} 10 次。",
                    category: "灵田",
                    sortOrder: 50,
                    requirements:
                    [
                        BuildRequirement(AchievementRequirementType.PlantCount, 10, $"累计种植 {context.StarterCropName} 10 次", context.StarterCropId)
                    ],
                    rewardGold: 1000),
                CreateDefaultAchievementEntity(
                    lastUpdateTime,
                    achievementId: "ach_harvest_crop_10",
                    achievementName: "丰收在望",
                    achievementType: AchievementType.Collection,
                    difficulty: AchievementDifficulty.Easy,
                    description: $"累计收获 {context.StarterCropName} 10 次。",
                    category: "灵田",
                    sortOrder: 51,
                    requirements:
                    [
                        BuildRequirement(AchievementRequirementType.HarvestCount, 10, $"累计收获 {context.StarterCropName} 10 次", context.StarterCropId)
                    ],
                    rewardGold: 1200),
                CreateDefaultAchievementEntity(
                    lastUpdateTime,
                    achievementId: "ach_dungeon_win_5",
                    achievementName: "试炼常客",
                    achievementType: AchievementType.Battle,
                    difficulty: AchievementDifficulty.Normal,
                    description: $"在 {context.StarterDungeonName} 中获胜 5 次。",
                    category: "副本",
                    sortOrder: 60,
                    requirements:
                    [
                        BuildRequirement(AchievementRequirementType.DungeonWinCount, 5, $"在 {context.StarterDungeonName} 中获胜 5 次", context.StarterDungeonId)
                    ],
                    rewardGold: 3000,
                    rewardSpiritStone: 20),
                CreateDefaultAchievementEntity(
                    lastUpdateTime,
                    achievementId: "ach_monster_kill_20",
                    achievementName: "妖物克星",
                    achievementType: AchievementType.Battle,
                    difficulty: AchievementDifficulty.Easy,
                    description: $"击杀 {context.StarterMonsterName} 20 次。",
                    category: "战斗",
                    sortOrder: 70,
                    requirements:
                    [
                        BuildRequirement(AchievementRequirementType.MonsterKillCount, 20, $"击杀 {context.StarterMonsterName} 20 次", context.StarterMonsterId)
                    ],
                    rewardGold: 1800),
                CreateDefaultAchievementEntity(
                    lastUpdateTime,
                    achievementId: "ach_skill_use_50",
                    achievementName: "术法熟练",
                    achievementType: AchievementType.Battle,
                    difficulty: AchievementDifficulty.Easy,
                    description: $"使用技能 {context.StarterSkillName} 50 次。",
                    category: "技能",
                    sortOrder: 80,
                    requirements:
                    [
                        BuildRequirement(AchievementRequirementType.SkillUseCount, 50, $"使用技能 {context.StarterSkillName} 50 次", context.StarterSkillId)
                    ],
                    rewardGold: 2200),
                CreateDefaultAchievementEntity(
                    lastUpdateTime,
                    achievementId: "ach_skill_damage_5000",
                    achievementName: "术式奔涌",
                    achievementType: AchievementType.Battle,
                    difficulty: AchievementDifficulty.Normal,
                    description: $"使用技能 {context.StarterSkillName} 累计造成 5000 点伤害。",
                    category: "技能",
                    sortOrder: 81,
                    requirements:
                    [
                        BuildRequirement(AchievementRequirementType.SkillDamageDealt, 5000, $"使用技能 {context.StarterSkillName} 累计造成 5000 点伤害", context.StarterSkillId)
                    ],
                    rewardGold: 2600,
                    rewardSpiritStone: 10),
                CreateDefaultAchievementEntity(
                    lastUpdateTime,
                    achievementId: "ach_mp_consumed_500",
                    achievementName: "灵力流转",
                    achievementType: AchievementType.Battle,
                    difficulty: AchievementDifficulty.Easy,
                    description: "累计消耗 500 点蓝量。",
                    category: "战斗",
                    sortOrder: 90,
                    requirements:
                    [
                        BuildRequirement(AchievementRequirementType.TotalMpConsumed, 500, "累计消耗 500 点蓝量")
                    ],
                    rewardGold: 1600),
                CreateDefaultAchievementEntity(
                    lastUpdateTime,
                    achievementId: "ach_gold_earned_10000",
                    achievementName: "财运渐盛",
                    achievementType: AchievementType.Collection,
                    difficulty: AchievementDifficulty.Normal,
                    description: "累计获得 10000 金币。",
                    category: "财富",
                    sortOrder: 100,
                    requirements:
                    [
                        BuildRequirement(AchievementRequirementType.TotalGoldEarned, 10000, "累计获得 10000 金币")
                    ],
                    rewardSpiritStone: 15),
                CreateDefaultAchievementEntity(
                    lastUpdateTime,
                    achievementId: "ach_crit_20",
                    achievementName: "刀锋如电",
                    achievementType: AchievementType.Battle,
                    difficulty: AchievementDifficulty.Easy,
                    description: "累计触发 20 次暴击。",
                    category: "战斗",
                    sortOrder: 110,
                    requirements:
                    [
                        BuildRequirement(AchievementRequirementType.CriticalHitCount, 20, "累计触发 20 次暴击")
                    ],
                    rewardGold: 1800),
                CreateDefaultAchievementEntity(
                    lastUpdateTime,
                    achievementId: "ach_death_10",
                    achievementName: "九死一生",
                    achievementType: AchievementType.Special,
                    difficulty: AchievementDifficulty.Easy,
                    description: "累计死亡 10 次。",
                    category: "特殊",
                    sortOrder: 111,
                    requirements:
                    [
                        BuildRequirement(AchievementRequirementType.DeathCount, 10, "累计死亡 10 次")
                    ],
                    rewardGold: 1000),
                CreateDefaultAchievementEntity(
                    lastUpdateTime,
                    achievementId: "ach_dodge_20",
                    achievementName: "身法飘逸",
                    achievementType: AchievementType.Battle,
                    difficulty: AchievementDifficulty.Easy,
                    description: "累计闪避 20 次。",
                    category: "战斗",
                    sortOrder: 112,
                    requirements:
                    [
                        BuildRequirement(AchievementRequirementType.DodgeCount, 20, "累计闪避 20 次")
                    ],
                    rewardGold: 1800),
                CreateDefaultAchievementEntity(
                    lastUpdateTime,
                    achievementId: "ach_starter_damage_300",
                    achievementName: "初试锋芒",
                    achievementType: AchievementType.Battle,
                    difficulty: AchievementDifficulty.Easy,
                    description: "累计造成 300 点伤害。",
                    category: "战斗",
                    sortOrder: 120,
                    requirements:
                    [
                        BuildRequirement(AchievementRequirementType.TotalDamageDealt, 300, "累计造成 300 点伤害")
                    ],
                    rewardGold: 300,
                    rewardItems: new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) { ["item_003"] = 1 }),
                CreateDefaultAchievementEntity(
                    lastUpdateTime,
                    achievementId: "ach_starter_alchemy_success_1",
                    achievementName: "丹火初燃",
                    achievementType: AchievementType.Activity,
                    difficulty: AchievementDifficulty.Easy,
                    description: $"成功炼制 1 次 {context.StarterAlchemyRecipeName}。",
                    category: "炼丹",
                    sortOrder: 121,
                    requirements:
                    [
                        BuildRequirement(AchievementRequirementType.AlchemySuccessCount, 1, $"成功炼制 1 次 {context.StarterAlchemyRecipeName}", context.StarterAlchemyRecipeId)
                    ],
                    rewardGold: 400,
                    rewardItems: new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) { ["alchemy_herb"] = 2 }),
                CreateDefaultAchievementEntity(
                    lastUpdateTime,
                    achievementId: "ach_starter_plant_1",
                    achievementName: "初次播种",
                    achievementType: AchievementType.Collection,
                    difficulty: AchievementDifficulty.Easy,
                    description: $"种植 1 次 {context.StarterCropName}。",
                    category: "灵田",
                    sortOrder: 122,
                    requirements:
                    [
                        BuildRequirement(AchievementRequirementType.PlantCount, 1, $"种植 1 次 {context.StarterCropName}", context.StarterCropId)
                    ],
                    rewardGold: 200),
                CreateDefaultAchievementEntity(
                    lastUpdateTime,
                    achievementId: "ach_starter_harvest_1",
                    achievementName: "初次收获",
                    achievementType: AchievementType.Collection,
                    difficulty: AchievementDifficulty.Easy,
                    description: $"收获 1 次 {context.StarterCropName}。",
                    category: "灵田",
                    sortOrder: 123,
                    requirements:
                    [
                        BuildRequirement(AchievementRequirementType.HarvestCount, 1, $"收获 1 次 {context.StarterCropName}", context.StarterCropId)
                    ],
                    rewardGold: 220),
                CreateDefaultAchievementEntity(
                    lastUpdateTime,
                    achievementId: "ach_starter_monster_kill_3",
                    achievementName: "试斩妖物",
                    achievementType: AchievementType.Battle,
                    difficulty: AchievementDifficulty.Easy,
                    description: $"击杀 3 只 {context.StarterMonsterName}。",
                    category: "战斗",
                    sortOrder: 124,
                    requirements:
                    [
                        BuildRequirement(AchievementRequirementType.MonsterKillCount, 3, $"击杀 3 只 {context.StarterMonsterName}", context.StarterMonsterId)
                    ],
                    rewardGold: 360)
            ];
        }

        private static AchievementConfigEntity CreateDefaultAchievementEntity(
            DateTime lastUpdateTime,
            string achievementId,
            string achievementName,
            AchievementType achievementType,
            AchievementDifficulty difficulty,
            string description,
            string category,
            int sortOrder,
            IReadOnlyList<AchievementRequirement> requirements,
            long rewardGold = 0,
            long rewardSpiritStone = 0,
            long rewardExp = 0,
            string? rewardTitle = null,
            Dictionary<string, int>? rewardItems = null,
            string? preAchievementIds = null)
        {
            var normalizedRequirements = requirements
                .Where(requirement => requirement != null)
                .Select(requirement => new AchievementRequirement
                {
                    RequirementType = requirement.RequirementType,
                    TargetId = NormalizeTargetId(requirement.TargetId),
                    TargetValue = Math.Max(0L, requirement.TargetValue),
                    Description = requirement.Description ?? string.Empty
                })
                .Where(requirement => requirement.TargetValue > 0)
                .ToList();

            var primaryRequirement = normalizedRequirements.FirstOrDefault()
                ?? new AchievementRequirement { RequirementType = AchievementRequirementType.TotalDamageDealt, TargetValue = 1, Description = "占位条件" };

            return new AchievementConfigEntity
            {
                AchievementId = achievementId,
                SeedKey = BuildDefaultAchievementSeedKey(achievementId),
                IsBuiltIn = true,
                BuiltInVersion = BuiltInVersion,
                AchievementName = achievementName,
                AchievementType = (int)achievementType,
                Difficulty = (int)difficulty,
                Description = description,
                Category = category,
                Points = (int)difficulty,
                IsHidden = false,
                PreAchievementIds = string.IsNullOrWhiteSpace(preAchievementIds) ? null : preAchievementIds,
                RewardGold = Math.Max(0L, rewardGold),
                RewardSpiritStone = Math.Max(0L, rewardSpiritStone),
                RewardExp = Math.Max(0L, rewardExp),
                RewardTitle = string.IsNullOrWhiteSpace(rewardTitle) ? null : rewardTitle.Trim(),
                RewardItemsJson = rewardItems != null && rewardItems.Count > 0 ? JsonSerializer.Serialize(rewardItems) : null,
                RequirementType = (int)primaryRequirement.RequirementType,
                RequirementTargetValue = primaryRequirement.TargetValue,
                RequirementDescription = primaryRequirement.Description,
                RequirementsJson = JsonSerializer.Serialize(normalizedRequirements),
                SortOrder = sortOrder,
                IsEnabled = true,
                LastUpdateTime = lastUpdateTime
            };
        }

        private static AchievementRequirement BuildRequirement(
            AchievementRequirementType requirementType,
            long targetValue,
            string description,
            string? targetId = null)
        {
            return new AchievementRequirement
            {
                RequirementType = requirementType,
                TargetId = NormalizeTargetId(targetId),
                TargetValue = Math.Max(0L, targetValue),
                Description = description
            };
        }

        private static string BuildDefaultAchievementSeedKey(string achievementId) => $"built-in:achievement:{achievementId}";

        private static string NormalizeTargetId(string? targetId)
        {
            return (targetId ?? string.Empty).Trim();
        }
    }
}
