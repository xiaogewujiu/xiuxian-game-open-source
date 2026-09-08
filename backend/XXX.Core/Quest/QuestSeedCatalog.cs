using System.Text.Json;
using XXX.Entity;

namespace XXX.Quest
{
    /// <summary>
    /// 任务内置补种上下文。
    /// </summary>
    public sealed class QuestSeedContext
    {
        public string StarterMonsterTargetId { get; init; } = "mon_s01_main_01";

        public string StarterMonsterName { get; init; } = "青岚山兔";

        public string StarterMapId { get; init; } = "map_s01_main_01";

        public string StarterMapName { get; init; } = "青岚山门";

        public string GrowthMapId { get; init; } = "map_s02_main_01";

        public string GrowthMapName { get; init; } = "赤砂荒原";

        public string StarterCropId { get; init; } = "crop_s01_herb";

        public string StarterCropName { get; init; } = "凝血青芽";

        public string StarterAlchemyRecipeId { get; init; } = "alchemy_baseline_001";

        public string StarterAlchemyRecipeName { get; init; } = "烈阳增元";

    }

    /// <summary>
    /// 默认内置任务目录。
    /// </summary>
    public static class QuestSeedCatalog
    {
        public const string BuiltInVersion = "quest-built-in-v1-20260403";

        public static readonly string[] DefaultQuestIds =
        [
            "pure_main_001",
            "pure_main_002",
            "pure_main_003",
            "pure_main_004",
            "pure_main_005",
            "pure_main_006",
            "pure_main_007",
            "pure_main_008",
            "pure_main_009",
            "pure_main_010",
            "pure_side_001",
            "pure_side_002",
            "pure_side_003",
            "pure_side_004",
            "pure_side_005",
            "pure_side_006"
        ];

        /// <summary>
        /// 构造默认内置任务实体。
        /// </summary>
        public static List<QuestConfigEntity> BuildDefaultQuestEntities(QuestSeedContext context, DateTime? now = null)
        {
            var lastUpdateTime = now ?? DateTime.Now;

            return
            [
                new QuestConfigEntity
                {
                    QuestId = "main_001",
                    QuestName = "新手引导",
                    QuestType = (int)QuestType.Main,
                    ResetCycle = (int)QuestResetCycle.None,
                    Description = "欢迎来到游戏世界！先达到5级熟悉一下基本操作吧。",
                    RequiredLevel = 1,
                    AutoAccept = true,
                    AutoSubmit = false,
                    RewardExp = 240,
                    RewardGold = 360,
                    RewardItemsJson = SerializeRewardItems(("item_003", 3), ("item_001", 2)),
                    ObjectivesJson = SerializeObjectives(
                        new QuestObjective
                        {
                            ObjectiveType = ObjectiveType.ReachLevel,
                            TargetId = "5",
                            TargetCount = 1,
                            Description = "达到等级5"
                        }),
                    SortOrder = 1,
                    IsEnabled = true,
                    SeedKey = BuildDefaultQuestSeedKey("main_001"),
                    IsBuiltIn = true,
                    BuiltInVersion = BuiltInVersion,
                    LastUpdateTime = lastUpdateTime
                },
                new QuestConfigEntity
                {
                    QuestId = "main_002",
                    QuestName = $"击败{context.StarterMonsterName}",
                    QuestType = (int)QuestType.Main,
                    ResetCycle = (int)QuestResetCycle.None,
                    Description = $"{context.StarterMonsterName}在附近出没，击败10只{context.StarterMonsterName}来证明你的实力！",
                    RequiredLevel = 5,
                    PreQuestIds = "main_001",
                    AutoAccept = false,
                    RewardExp = 360,
                    RewardGold = 520,
                    RewardItemsJson = SerializeRewardItems(("alchemy_herb", 3), ("item_003", 1)),
                    ObjectivesJson = SerializeObjectives(
                        new QuestObjective
                        {
                            ObjectiveType = ObjectiveType.KillMonster,
                            TargetId = context.StarterMonsterTargetId,
                            TargetCount = 10,
                            Description = $"击败{context.StarterMonsterName}"
                        }),
                    SortOrder = 2,
                    IsEnabled = true,
                    SeedKey = BuildDefaultQuestSeedKey("main_002"),
                    IsBuiltIn = true,
                    BuiltInVersion = BuiltInVersion,
                    LastUpdateTime = lastUpdateTime
                },
                new QuestConfigEntity
                {
                    QuestId = "main_003",
                    QuestName = $"巡游{context.StarterMapName}",
                    QuestType = (int)QuestType.Main,
                    ResetCycle = (int)QuestResetCycle.None,
                    Description = $"前往 {context.StarterMapName} 历练，在这片区域中完成 5 场战斗。",
                    RequiredLevel = 5,
                    PreQuestIds = "main_002",
                    AutoAccept = false,
                    RewardExp = 420,
                    RewardGold = 600,
                    RewardItemsJson = SerializeRewardItems(("item_003", 2), ("alchemy_herb", 2)),
                    ObjectivesJson = SerializeObjectives(
                        new QuestObjective
                        {
                            ObjectiveType = ObjectiveType.MapBattleCount,
                            TargetId = context.StarterMapId,
                            TargetCount = 5,
                            Description = $"在 {context.StarterMapName} 完成 5 场战斗"
                        }),
                    SortOrder = 3,
                    IsEnabled = true,
                    SeedKey = BuildDefaultQuestSeedKey("main_003"),
                    IsBuiltIn = true,
                    BuiltInVersion = BuiltInVersion,
                    LastUpdateTime = lastUpdateTime
                },
                new QuestConfigEntity
                {
                    QuestId = "daily_001",
                    QuestName = "日常：战斗训练",
                    QuestType = (int)QuestType.Daily,
                    ResetCycle = (int)QuestResetCycle.Daily,
                    Description = "每天保持训练，完成3场战斗。",
                    RequiredLevel = 1,
                    AutoAccept = false,
                    RewardExp = 160,
                    RewardGold = 180,
                    RewardSpiritStone = 3,
                    ObjectivesJson = SerializeObjectives(
                        new QuestObjective
                        {
                            ObjectiveType = ObjectiveType.BattleActivity,
                            ActivityType = BattleActivityType.WinCount,
                            TargetId = "win_count",
                            TargetCount = 3,
                            Description = "完成3场战斗胜利"
                        }),
                    SortOrder = 20,
                    IsEnabled = true,
                    SeedKey = BuildDefaultQuestSeedKey("daily_001"),
                    IsBuiltIn = true,
                    BuiltInVersion = BuiltInVersion,
                    LastUpdateTime = lastUpdateTime
                },
                new QuestConfigEntity
                {
                    QuestId = "daily_002",
                    QuestName = $"日常：征战{context.StarterMapName}",
                    QuestType = (int)QuestType.Daily,
                    ResetCycle = (int)QuestResetCycle.Daily,
                    Description = $"在 {context.StarterMapName} 完成 5 场战斗。",
                    RequiredLevel = 1,
                    AutoAccept = false,
                    RewardExp = 180,
                    RewardGold = 220,
                    RewardSpiritStone = 3,
                    ObjectivesJson = SerializeObjectives(
                        new QuestObjective
                        {
                            ObjectiveType = ObjectiveType.MapBattleCount,
                            TargetId = context.StarterMapId,
                            TargetCount = 5,
                            Description = $"在 {context.StarterMapName} 完成 5 场战斗"
                        }),
                    SortOrder = 21,
                    IsEnabled = true,
                    SeedKey = BuildDefaultQuestSeedKey("daily_002"),
                    IsBuiltIn = true,
                    BuiltInVersion = BuiltInVersion,
                    LastUpdateTime = lastUpdateTime
                },
                new QuestConfigEntity
                {
                    QuestId = "daily_003",
                    QuestName = $"日常：制胜{context.GrowthMapName}",
                    QuestType = (int)QuestType.Daily,
                    ResetCycle = (int)QuestResetCycle.Daily,
                    Description = $"在 {context.GrowthMapName} 获得 3 场胜利。",
                    RequiredLevel = 5,
                    AutoAccept = false,
                    RewardExp = 220,
                    RewardGold = 260,
                    RewardSpiritStone = 4,
                    ObjectivesJson = SerializeObjectives(
                        new QuestObjective
                        {
                            ObjectiveType = ObjectiveType.MapWinCount,
                            TargetId = context.GrowthMapId,
                            TargetCount = 3,
                            Description = $"在 {context.GrowthMapName} 获得 3 场胜利"
                        }),
                    SortOrder = 22,
                    IsEnabled = true,
                    SeedKey = BuildDefaultQuestSeedKey("daily_003"),
                    IsBuiltIn = true,
                    BuiltInVersion = BuiltInVersion,
                    LastUpdateTime = lastUpdateTime
                },
                new QuestConfigEntity
                {
                    QuestId = "daily_004",
                    QuestName = "日常：丹香初现",
                    QuestType = (int)QuestType.Daily,
                    ResetCycle = (int)QuestResetCycle.Daily,
                    Description = "领取 1 次炼丹结果。",
                    RequiredLevel = 1,
                    AutoAccept = false,
                    RewardExp = 180,
                    RewardGold = 200,
                    RewardItemsJson = SerializeRewardItems(("alchemy_herb", 2)),
                    ObjectivesJson = SerializeObjectives(
                        new QuestObjective
                        {
                            ObjectiveType = ObjectiveType.AlchemyCollect,
                            TargetCount = 1,
                            Description = "领取 1 次炼丹结果"
                        }),
                    SortOrder = 23,
                    IsEnabled = true,
                    SeedKey = BuildDefaultQuestSeedKey("daily_004"),
                    IsBuiltIn = true,
                    BuiltInVersion = BuiltInVersion,
                    LastUpdateTime = lastUpdateTime
                },
                new QuestConfigEntity
                {
                    QuestId = "daily_005",
                    QuestName = "日常：炉火锻形",
                    QuestType = (int)QuestType.Daily,
                    ResetCycle = (int)QuestResetCycle.Daily,
                    Description = "领取 1 次锻造结果。",
                    RequiredLevel = 1,
                    AutoAccept = false,
                    RewardExp = 180,
                    RewardGold = 220,
                    RewardItemsJson = SerializeRewardItems(("item_003", 2)),
                    ObjectivesJson = SerializeObjectives(
                        new QuestObjective
                        {
                            ObjectiveType = ObjectiveType.ForgeCollect,
                            TargetCount = 1,
                            Description = "领取 1 次锻造结果"
                        }),
                    SortOrder = 24,
                    IsEnabled = true,
                    SeedKey = BuildDefaultQuestSeedKey("daily_005"),
                    IsBuiltIn = true,
                    BuiltInVersion = BuiltInVersion,
                    LastUpdateTime = lastUpdateTime
                },
                new QuestConfigEntity
                {
                    QuestId = "daily_006",
                    QuestName = $"日常：栽培{context.StarterCropName}",
                    QuestType = (int)QuestType.Daily,
                    ResetCycle = (int)QuestResetCycle.Daily,
                    Description = $"种植 {context.StarterCropName} 3 次，并完成 1 次收获。",
                    RequiredLevel = 1,
                    AutoAccept = false,
                    RewardExp = 200,
                    RewardGold = 180,
                    RewardItemsJson = SerializeRewardItems(("alchemy_herb", 3)),
                    ObjectivesJson = SerializeObjectives(
                        new QuestObjective
                        {
                            ObjectiveType = ObjectiveType.PlantCrop,
                            TargetId = context.StarterCropId,
                            TargetCount = 3,
                            Description = $"种植 {context.StarterCropName} 3 次"
                        },
                        new QuestObjective
                        {
                            ObjectiveType = ObjectiveType.HarvestCrop,
                            TargetId = context.StarterCropId,
                            TargetCount = 1,
                            Description = $"收获 {context.StarterCropName} 1 次"
                        }),
                    SortOrder = 25,
                    IsEnabled = true,
                    SeedKey = BuildDefaultQuestSeedKey("daily_006"),
                    IsBuiltIn = true,
                    BuiltInVersion = BuiltInVersion,
                    LastUpdateTime = lastUpdateTime
                },
                new QuestConfigEntity
                {
                    QuestId = "daily_007",
                    QuestName = $"日常：热身{context.StarterMapName}",
                    QuestType = (int)QuestType.Daily,
                    ResetCycle = (int)QuestResetCycle.Daily,
                    Description = $"在 {context.StarterMapName} 完成 2 场战斗。",
                    RequiredLevel = 1,
                    AutoAccept = false,
                    RewardExp = 120,
                    RewardGold = 160,
                    RewardItemsJson = SerializeRewardItems(("item_003", 1)),
                    ObjectivesJson = SerializeObjectives(
                        new QuestObjective
                        {
                            ObjectiveType = ObjectiveType.MapBattleCount,
                            TargetId = context.StarterMapId,
                            TargetCount = 2,
                            Description = $"在 {context.StarterMapName} 完成 2 场战斗"
                        }),
                    SortOrder = 26,
                    IsEnabled = true,
                    SeedKey = BuildDefaultQuestSeedKey("daily_007"),
                    IsBuiltIn = true,
                    BuiltInVersion = BuiltInVersion,
                    LastUpdateTime = lastUpdateTime
                },
                new QuestConfigEntity
                {
                    QuestId = "daily_008",
                    QuestName = $"日常：制胜{context.StarterMapName}",
                    QuestType = (int)QuestType.Daily,
                    ResetCycle = (int)QuestResetCycle.Daily,
                    Description = $"在 {context.StarterMapName} 获得 2 场胜利。",
                    RequiredLevel = 1,
                    AutoAccept = false,
                    RewardExp = 140,
                    RewardGold = 180,
                    RewardItemsJson = SerializeRewardItems(("alchemy_herb", 2)),
                    ObjectivesJson = SerializeObjectives(
                        new QuestObjective
                        {
                            ObjectiveType = ObjectiveType.MapWinCount,
                            TargetId = context.StarterMapId,
                            TargetCount = 2,
                            Description = $"在 {context.StarterMapName} 获得 2 场胜利"
                        }),
                    SortOrder = 27,
                    IsEnabled = true,
                    SeedKey = BuildDefaultQuestSeedKey("daily_008"),
                    IsBuiltIn = true,
                    BuiltInVersion = BuiltInVersion,
                    LastUpdateTime = lastUpdateTime
                },
                new QuestConfigEntity
                {
                    QuestId = "daily_009",
                    QuestName = $"日常：试炼{context.StarterAlchemyRecipeName}",
                    QuestType = (int)QuestType.Daily,
                    ResetCycle = (int)QuestResetCycle.Daily,
                    Description = $"成功炼制并领取 1 次 {context.StarterAlchemyRecipeName} 的成果。",
                    RequiredLevel = 1,
                    AutoAccept = false,
                    RewardExp = 180,
                    RewardGold = 220,
                    RewardItemsJson = SerializeRewardItems(("spirit_water", 1)),
                    ObjectivesJson = SerializeObjectives(
                        new QuestObjective
                        {
                            ObjectiveType = ObjectiveType.AlchemySuccess,
                            TargetId = context.StarterAlchemyRecipeId,
                            TargetCount = 1,
                            Description = $"成功炼制 1 次 {context.StarterAlchemyRecipeName}"
                        },
                        new QuestObjective
                        {
                            ObjectiveType = ObjectiveType.AlchemyCollect,
                            TargetCount = 1,
                            Description = "领取 1 次炼丹结果"
                        }),
                    SortOrder = 28,
                    IsEnabled = true,
                    SeedKey = BuildDefaultQuestSeedKey("daily_009"),
                    IsBuiltIn = true,
                    BuiltInVersion = BuiltInVersion,
                    LastUpdateTime = lastUpdateTime
                },
                new QuestConfigEntity
                {
                    QuestId = "daily_010",
                    QuestName = $"{context.StarterCropName}启耕",
                    QuestType = (int)QuestType.Daily,
                    ResetCycle = (int)QuestResetCycle.Daily,
                    Description = $"种植并收获 1 次 {context.StarterCropName}。",
                    RequiredLevel = 1,
                    AutoAccept = false,
                    RewardExp = 180,
                    RewardGold = 220,
                    RewardItemsJson = SerializeRewardItems(("alchemy_herb", 2)),
                    ObjectivesJson = SerializeObjectives(
                        new QuestObjective
                        {
                            ObjectiveType = ObjectiveType.PlantCrop,
                            TargetId = context.StarterCropId,
                            TargetCount = 1,
                            Description = $"种植 1 次 {context.StarterCropName}"
                        },
                        new QuestObjective
                        {
                            ObjectiveType = ObjectiveType.HarvestCrop,
                            TargetId = context.StarterCropId,
                            TargetCount = 1,
                            Description = $"收获 1 次 {context.StarterCropName}"
                        }),
                    SortOrder = 29,
                    IsEnabled = true,
                    SeedKey = BuildDefaultQuestSeedKey("daily_010"),
                    IsBuiltIn = true,
                    BuiltInVersion = BuiltInVersion,
                    LastUpdateTime = lastUpdateTime
                },
                new QuestConfigEntity
                {
                    QuestId = "weekly_001",
                    QuestName = "周常：战斗达人",
                    QuestType = (int)QuestType.Weekly,
                    ResetCycle = (int)QuestResetCycle.Weekly,
                    Description = "本周内完成20场战斗胜利。",
                    RequiredLevel = 1,
                    AutoAccept = false,
                    RewardExp = 960,
                    RewardGold = 1200,
                    RewardSpiritStone = 18,
                    ObjectivesJson = SerializeObjectives(
                        new QuestObjective
                        {
                            ObjectiveType = ObjectiveType.BattleActivity,
                            ActivityType = BattleActivityType.WinCount,
                            TargetId = "win_count",
                            TargetCount = 20,
                            Description = "完成20场战斗胜利"
                        }),
                    SortOrder = 30,
                    IsEnabled = true,
                    SeedKey = BuildDefaultQuestSeedKey("weekly_001"),
                    IsBuiltIn = true,
                    BuiltInVersion = BuiltInVersion,
                    LastUpdateTime = lastUpdateTime
                },
                new QuestConfigEntity
                {
                    QuestId = "weekly_002",
                    QuestName = "周常：炼器进阶",
                    QuestType = (int)QuestType.Weekly,
                    ResetCycle = (int)QuestResetCycle.Weekly,
                    Description = "累计强化成功 10 次。",
                    RequiredLevel = 1,
                    AutoAccept = false,
                    RewardExp = 1200,
                    RewardGold = 1500,
                    RewardSpiritStone = 12,
                    ObjectivesJson = SerializeObjectives(
                        new QuestObjective
                        {
                            ObjectiveType = ObjectiveType.EquipEnhanceCount,
                            TargetCount = 10,
                            Description = "累计强化成功 10 次"
                        }),
                    SortOrder = 31,
                    IsEnabled = true,
                    SeedKey = BuildDefaultQuestSeedKey("weekly_002"),
                    IsBuiltIn = true,
                    BuiltInVersion = BuiltInVersion,
                    LastUpdateTime = lastUpdateTime
                },
                new QuestConfigEntity
                {
                    QuestId = "weekly_003",
                    QuestName = "周常：灵材储备",
                    QuestType = (int)QuestType.Weekly,
                    ResetCycle = (int)QuestResetCycle.Weekly,
                    Description = "累计获得强化石 20 个。",
                    RequiredLevel = 1,
                    AutoAccept = false,
                    RewardExp = 1100,
                    RewardGold = 1400,
                    RewardSpiritStone = 10,
                    ObjectivesJson = SerializeObjectives(
                        new QuestObjective
                        {
                            ObjectiveType = ObjectiveType.CollectItem,
                            TargetId = "item_003",
                            TargetCount = 20,
                            Description = "累计获得强化石 20 个"
                        }),
                    SortOrder = 32,
                    IsEnabled = true,
                    SeedKey = BuildDefaultQuestSeedKey("weekly_003"),
                    IsBuiltIn = true,
                    BuiltInVersion = BuiltInVersion,
                    LastUpdateTime = lastUpdateTime
                },
                new QuestConfigEntity
                {
                    QuestId = "weekly_004",
                    QuestName = $"周常：讨伐{context.StarterMonsterName}",
                    QuestType = (int)QuestType.Weekly,
                    ResetCycle = (int)QuestResetCycle.Weekly,
                    Description = $"本周内击败 3 只 {context.StarterMonsterName}。",
                    RequiredLevel = 1,
                    AutoAccept = false,
                    RewardExp = 420,
                    RewardGold = 520,
                    RewardItemsJson = SerializeRewardItems(("item_003", 3)),
                    ObjectivesJson = SerializeObjectives(
                        new QuestObjective
                        {
                            ObjectiveType = ObjectiveType.KillMonster,
                            TargetId = context.StarterMonsterTargetId,
                            TargetCount = 3,
                            Description = $"击败 3 只 {context.StarterMonsterName}"
                        }),
                    SortOrder = 33,
                    IsEnabled = true,
                    SeedKey = BuildDefaultQuestSeedKey("weekly_004"),
                    IsBuiltIn = true,
                    BuiltInVersion = BuiltInVersion,
                    LastUpdateTime = lastUpdateTime
                }
            ];
        }

        private static string SerializeObjectives(params QuestObjective[] objectives)
        {
            return JsonSerializer.Serialize(objectives.ToList());
        }

        private static string SerializeRewardItems(params (string ItemId, int Count)[] items)
        {
            var rewardItems = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var (itemId, count) in items)
            {
                if (string.IsNullOrWhiteSpace(itemId) || count <= 0)
                {
                    continue;
                }

                rewardItems[itemId] = count;
            }

            return rewardItems.Count == 0
                ? string.Empty
                : JsonSerializer.Serialize(rewardItems);
        }

        private static string BuildDefaultQuestSeedKey(string questId) => $"built-in:quest:{questId}";
    }
}
