using System.Text.Json;
using XXX.Entity;
using XXX.Infrastructure.Repositories;

namespace XXX.Infrastructure.SeedData
{
    /// <summary>
    /// 秘境系统种子数据目录。
    /// 包含秘境模板和全部事件配置的种子数据 + 同步逻辑。
    /// </summary>
    public static class DungeonInstanceSeedData
    {
        public const string DungeonInstanceTemplateBuiltInVersion = "dungeon-instance-template-built-in-v1-20260616";
        public const string DungeonEventConfigBuiltInVersion = "dungeon-event-config-built-in-v1-20260616";
        public const string DungeonEventGroupBuiltInVersion = "dungeon-event-group-built-in-v1-20260616";

        private static string BuildTemplateSeedKey(string dungeonId) => $"built-in:dungeon-instance-template:{dungeonId}";
        private static string BuildEventSeedKey(string eventId) => $"built-in:dungeon-event:{eventId}";
        private static string BuildGroupSeedKey(string groupId) => $"built-in:dungeon-event-group:{groupId}";

        // ───────────────────── 秘境模板种子 ─────────────────────

        public static List<DungeonInstanceTemplateEntity> BuildTemplates()
        {
            var now = DateTime.Now;
            return
            [
                new()
                {
                    Id = "dungeon_instance_001",
                    Name = "灵泉秘境",
                    Description = "入门级秘境，灵气充沛，适合初入修途的修士探索。",
                    Enabled = true,
                    RecommendedLevel = 10,
                    DailyEnterLimit = 1,
                    TickIntervalSeconds = 30,
                    OpenScheduleJson = JsonSerializer.Serialize(new
                    {
                        schedules = new object[]
                        {
                            new { type = "daily", startTime = "12:00", endTime = "14:00" },
                            new { type = "daily", startTime = "20:00", endTime = "22:00" }
                        }
                    }),
                    EntryCostsJson = JsonSerializer.Serialize(new object[]
                    {
                        new { type = "Item", id = "dungeon_token", quantity = 1 },
                        new { type = "Gold", quantity = 1000 },
                        new { type = "SpiritStone", quantity = 10 }
                    }),
                    EventGroupId = "eg_normal",
                    AutoMedicineConfigJson = JsonSerializer.Serialize(new
                    {
                        hpThresholdPercent = 50,
                        mpThresholdPercent = 30,
                        pills = new object[]
                        {
                            new { itemId = "item_001", healAmount = 100, healMpAmount = 0 },
                            new { itemId = "item_006", healAmount = 0, healMpAmount = 120 }
                        }
                    }),
                    EncounterConfigJson = JsonSerializer.Serialize(new
                    {
                        favorabilityTeamThresholds = new object[]
                        {
                            new { minFavorability = 1500, teamRate = 80 },
                            new { minFavorability = 500, teamRate = 60 },
                            new { minFavorability = 100, teamRate = 40 },
                            new { minFavorability = 0, teamRate = 20 },
                            new { minFavorability = -500, teamRate = 5 },
                            new { minFavorability = -99999, teamRate = 0 }
                        },
                        defaultTeamRate = 20,
                        rivalPvpBonusAttack = 10,
                        pvpPlunderConfig = new
                        {
                            currencyTypes = new[] { "Gold", "SpiritStone" },
                            currencyPlunderRate = 30,
                            equipmentPlunderCount = "1-2",
                            itemPlunderCount = "1-2",
                            equipmentPlunderRate = 40,
                            itemPlunderRate = 40
                        }
                    }),
                    SeedKey = BuildTemplateSeedKey("dungeon_instance_001"),
                    IsBuiltIn = true,
                    BuiltInVersion = DungeonInstanceTemplateBuiltInVersion,
                    LastUpdateTime = now
                },
                new()
                {
                    Id = "dungeon_instance_002",
                    Name = "幽冥深渊",
                    Description = "中级秘境，阴气弥漫，潜藏着强大的妖兽和珍贵的宝藏。",
                    Enabled = true,
                    RecommendedLevel = 25,
                    DailyEnterLimit = 1,
                    TickIntervalSeconds = 45,
                    OpenScheduleJson = JsonSerializer.Serialize(new
                    {
                        schedules = new object[]
                        {
                            new { type = "daily", startTime = "20:00", endTime = "23:00" },
                            new { type = "weekly", dayOfWeek = 6, startTime = "14:00", endTime = "18:00" },
                            new { type = "weekly", dayOfWeek = 0, startTime = "14:00", endTime = "18:00" }
                        }
                    }),
                    EntryCostsJson = JsonSerializer.Serialize(new object[]
                    {
                        new { type = "Item", id = "dungeon_token", quantity = 2 },
                        new { type = "Gold", quantity = 5000 },
                        new { type = "SpiritStone", quantity = 50 }
                    }),
                    EventGroupId = "eg_combat",
                    AutoMedicineConfigJson = JsonSerializer.Serialize(new
                    {
                        hpThresholdPercent = 60,
                        mpThresholdPercent = 40,
                        pills = new object[]
                        {
                            new { itemId = "item_002", healAmount = 300, healMpAmount = 0 },
                            new { itemId = "item_007", healAmount = 0, healMpAmount = 280 }
                        }
                    }),
                    EncounterConfigJson = JsonSerializer.Serialize(new
                    {
                        favorabilityTeamThresholds = new object[]
                        {
                            new { minFavorability = 1500, teamRate = 80 },
                            new { minFavorability = 500, teamRate = 60 },
                            new { minFavorability = 100, teamRate = 40 },
                            new { minFavorability = 0, teamRate = 20 },
                            new { minFavorability = -500, teamRate = 5 },
                            new { minFavorability = -99999, teamRate = 0 }
                        },
                        defaultTeamRate = 20,
                        rivalPvpBonusAttack = 10,
                        pvpPlunderConfig = new
                        {
                            currencyTypes = new[] { "Gold", "SpiritStone" },
                            currencyPlunderRate = 30,
                            equipmentPlunderCount = "1-3",
                            itemPlunderCount = "1-3",
                            equipmentPlunderRate = 50,
                            itemPlunderRate = 50
                        }
                    }),
                    SeedKey = BuildTemplateSeedKey("dungeon_instance_002"),
                    IsBuiltIn = true,
                    BuiltInVersion = DungeonInstanceTemplateBuiltInVersion,
                    LastUpdateTime = now
                },
                new()
                {
                    Id = "dungeon_instance_003",
                    Name = "天劫试炼",
                    Description = "高级秘境，天雷滚滚，只有实力强大的修士才能在此生存。",
                    Enabled = true,
                    RecommendedLevel = 40,
                    DailyEnterLimit = 1,
                    TickIntervalSeconds = 60,
                    OpenScheduleJson = JsonSerializer.Serialize(new
                    {
                        schedules = new object[]
                        {
                            new { type = "weekly", dayOfWeek = 6, startTime = "20:00", endTime = "23:00" },
                            new { type = "weekly", dayOfWeek = 0, startTime = "20:00", endTime = "23:00" }
                        }
                    }),
                    EntryCostsJson = JsonSerializer.Serialize(new object[]
                    {
                        new { type = "Item", id = "dungeon_token", quantity = 3 },
                        new { type = "Gold", quantity = 10000 },
                        new { type = "SpiritStone", quantity = 100 }
                    }),
                    EventGroupId = "eg_treasure",
                    AutoMedicineConfigJson = JsonSerializer.Serialize(new
                    {
                        hpThresholdPercent = 70,
                        mpThresholdPercent = 50,
                        pills = new object[]
                        {
                            new { itemId = "item_002", healAmount = 300, healMpAmount = 0 },
                            new { itemId = "item_007", healAmount = 0, healMpAmount = 280 }
                        }
                    }),
                    EncounterConfigJson = JsonSerializer.Serialize(new
                    {
                        favorabilityTeamThresholds = new object[]
                        {
                            new { minFavorability = 1500, teamRate = 80 },
                            new { minFavorability = 500, teamRate = 60 },
                            new { minFavorability = 100, teamRate = 40 },
                            new { minFavorability = 0, teamRate = 20 },
                            new { minFavorability = -500, teamRate = 5 },
                            new { minFavorability = -99999, teamRate = 0 }
                        },
                        defaultTeamRate = 20,
                        rivalPvpBonusAttack = 10,
                        pvpPlunderConfig = new
                        {
                            currencyTypes = new[] { "Gold", "SpiritStone" },
                            currencyPlunderRate = 30,
                            equipmentPlunderCount = "1-3",
                            itemPlunderCount = "1-3",
                            equipmentPlunderRate = 50,
                            itemPlunderRate = 50
                        }
                    }),
                    SeedKey = BuildTemplateSeedKey("dungeon_instance_003"),
                    IsBuiltIn = true,
                    BuiltInVersion = DungeonInstanceTemplateBuiltInVersion,
                    LastUpdateTime = now
                }
            ];
        }

        // ───────────────────── 事件配置种子 ─────────────────────

        public static List<DungeonEventConfigEntity> BuildEventConfigs()
        {
            var events = new List<DungeonEventConfigEntity>();
            events.AddRange(BuildBattleEvents());
            events.AddRange(BuildHealEvents());
            events.AddRange(BuildBuffEvents());
            events.AddRange(BuildDebuffEvents());
            events.AddRange(BuildResourceEvents());
            events.AddRange(BuildTreasureEvents());
            events.AddRange(BuildAdventureEvents());
            events.AddRange(BuildShopEvents());
            events.AddRange(BuildSpecialEvents());
            events.AddRange(BuildEmptyEvents());
            events.AddRange(BuildPlayerEncounterEvents());
            return events;
        }

        // ── 战斗类 (19个) ──

        private static List<DungeonEventConfigEntity> BuildBattleEvents() =>
        [
            MakeEvent("battle_normal_001", "遭遇野狼", "你沿着小径前行，忽然草丛中窜出一只野狼，龇牙咧嘴地挡住了去路。", DungeonEventType.Battle, 30, "{\"monsterId\":\"monster_001\"}", false),
            MakeEvent("battle_normal_002", "遭遇山贼", "前方传来一阵嘈杂声，几个山贼从树后跳了出来：\"留下买路财！\"", DungeonEventType.Battle, 25, "{\"monsterId\":\"monster_002\"}", false),
            MakeEvent("battle_normal_003", "遭遇毒蛇", "你正走着，脚边忽然传来\"嘶嘶\"声，一条毒蛇从石缝中探出头来。", DungeonEventType.Battle, 20, "{\"monsterId\":\"monster_003\"}", false),
            MakeEvent("battle_normal_004", "遭遇妖兽", "一股腥风扑面而来，一只浑身散发着妖气的野兽拦住了你的去路。", DungeonEventType.Battle, 18, "{\"monsterId\":\"monster_004\"}", false),
            MakeEvent("battle_normal_005", "遭遇傀儡", "地面忽然震动，一具石制傀儡从地下钻出，机械地朝你逼近。", DungeonEventType.Battle, 15, "{\"monsterId\":\"monster_005\"}", false),
            MakeEvent("battle_normal_006", "遭遇幽灵", "阴风阵阵，一个半透明的幽灵从墙壁中飘出，发出凄厉的叫声。", DungeonEventType.Battle, 15, "{\"monsterId\":\"monster_006\"}", false),
            MakeEvent("battle_elite_001", "精英·石魔", "你踏入一片碎石地带，脚下的石块忽然聚拢，化作一尊巨大的石魔！", DungeonEventType.Battle, 10, "{\"monsterId\":\"monster_elite_001\"}", true),
            MakeEvent("battle_elite_002", "精英·影刺客", "一道黑影从你背后闪过，你猛地转身，只见一个黑衣刺客正冷冷地盯着你。", DungeonEventType.Battle, 10, "{\"monsterId\":\"monster_elite_002\"}", true),
            MakeEvent("battle_elite_003", "精英·火焰灵", "空气突然变得灼热，一团火焰凝聚成形，化作一只火焰精灵朝你扑来！", DungeonEventType.Battle, 8, "{\"monsterId\":\"monster_elite_003\"}", true),
            MakeEvent("battle_elite_004", "精英·寒冰蛟", "温度骤降，一条浑身覆盖寒冰的蛟龙从冰雾中现身，龙吟震耳欲聋。", DungeonEventType.Battle, 8, "{\"monsterId\":\"monster_elite_004\"}", true),
            MakeEvent("battle_elite_005", "精英·雷兽", "天空忽然乌云密布，一只浑身缠绕雷电的巨兽从云中落下！", DungeonEventType.Battle, 6, "{\"monsterId\":\"monster_elite_005\"}", true),
            MakeEvent("battle_boss_001", "秘境守卫", "你来到一处开阔地带，一尊古老的石像忽然睁开双眼，发出低沉的声音：\"擅闯者，死！\"", DungeonEventType.Battle, 3, "{\"monsterId\":\"monster_boss_001\"}", true),
            MakeEvent("battle_boss_002", "远古妖灵", "大地剧烈震动，一个巨大的妖灵从地底苏醒，它的妖气令天地变色！", DungeonEventType.Battle, 2, "{\"monsterId\":\"monster_boss_002\"}", true),
            MakeEvent("battle_boss_003", "魔化修士", "一个浑身散发着魔气的修士挡在路中央，他的眼中已无半分理智。", DungeonEventType.Battle, 2, "{\"monsterId\":\"monster_boss_003\"}", true),
            MakeEvent("battle_swarm_001", "群狼围攻", "四面八方传来狼嚎声，十几只野狼将你团团围住，眼中泛着绿光。", DungeonEventType.Battle, 8, "{\"monsterId\":\"monster_001\",\"quantity\":3}", false),
            MakeEvent("battle_swarm_002", "虫群来袭", "地面忽然涌出大量毒虫，密密麻麻地朝你爬来！", DungeonEventType.Battle, 8, "{\"monsterId\":\"monster_003\",\"quantity\":3}", false),
            MakeEvent("battle_ambush_001", "伏击", "你正走着，忽然脚下踩空，落入一个陷阱中，几个黑衣人从暗处跳出！", DungeonEventType.Battle, 4, "{\"monsterId\":\"monster_004\",\"ambush\":true}", false),
            MakeEvent("battle_ambush_002", "暗箭伤人", "你忽然感到一阵剧痛，一支暗箭射中了你的肩膀，敌人从暗处现身！", DungeonEventType.Battle, 4, "{\"monsterId\":\"monster_002\",\"ambush\":true,\"startDamage\":\"5%\"}", false),
            MakeEvent("battle_guardian_001", "宝箱守卫", "你发现了一个宝箱，但旁边蹲着一只凶猛的守护兽，它正死死盯着宝箱。", DungeonEventType.Battle, 5, "{\"monsterId\":\"monster_elite_001\",\"extraReward\":\"chest_common_001\"}", true),
        ];

        // ── 回复类 (10个) ──

        private static List<DungeonEventConfigEntity> BuildHealEvents() =>
        [
            MakeEvent("heal_hp_001", "灵泉涌出", "你发现山壁上有一股清泉潺潺流下，泉水散发着淡淡的灵气。你捧起一饮，顿感浑身舒畅，伤势减轻了不少。", DungeonEventType.Heal, 25, "{\"healPercent\":30,\"target\":\"hp\"}", null),
            MakeEvent("heal_hp_002", "仙草发现", "石缝中长着一株散发荧光的灵草，你小心采摘服下，一股暖流涌入四肢百骸。", DungeonEventType.Heal, 20, "{\"healPercent\":50,\"target\":\"hp\"}", null),
            MakeEvent("heal_hp_003", "灵丹妙药", "你在一处隐蔽的石台上发现了一颗散发着金光的丹药，服下后伤势瞬间痊愈！", DungeonEventType.Heal, 10, "{\"healPercent\":100,\"target\":\"hp\"}", null),
            MakeEvent("heal_mp_001", "灵脉涌动", "脚下的地面忽然泛起微光，一股纯净的灵力从地底涌入你的体内。", DungeonEventType.Heal, 25, "{\"healPercent\":30,\"target\":\"mp\"}", null),
            MakeEvent("heal_mp_002", "灵石矿脉", "你发现了一处小型灵石矿脉，灵力充沛，你盘膝打坐片刻，灵力恢复了不少。", DungeonEventType.Heal, 20, "{\"healPercent\":50,\"target\":\"mp\"}", null),
            MakeEvent("heal_mp_003", "聚灵阵", "地面上刻着一个古老的聚灵阵，阵法自动运转，将天地灵气灌入你的体内。", DungeonEventType.Heal, 10, "{\"healPercent\":100,\"target\":\"mp\"}", null),
            MakeEvent("heal_both_001", "神秘温泉", "你发现了一处热气腾腾的温泉，泉水中蕴含着浓郁的灵气。你泡了一会儿，身心俱畅。", DungeonEventType.Heal, 15, "{\"healPercent\":50,\"target\":\"both\"}", null),
            MakeEvent("heal_both_002", "上古灵池", "一汪碧绿的灵池出现在眼前，池水散发着令人心旷神怡的香气。你跃入池中，伤势和灵力完全恢复！", DungeonEventType.Heal, 8, "{\"healPercent\":100,\"target\":\"both\"}", null),
            MakeEvent("heal_pill_001", "野外药圃", "灌木丛后藏着一片小小的药圃，几株灵草长势正旺。你小心翼翼地采集了一些。", DungeonEventType.Heal, 25, "{\"itemId\":\"item_001\",\"quantity\":\"1-3\"}", false),
            MakeEvent("heal_pill_002", "药师遗骸", "角落里躺着一具骸骨，旁边散落着一个药葫芦。你打开一看，里面竟还有几颗完好的丹药。", DungeonEventType.Heal, 15, "{\"itemId\":\"item_002\",\"quantity\":1}", true),
        ];

        // ── 增益类 (19个) ──

        private static List<DungeonEventConfigEntity> BuildBuffEvents() =>
        [
            MakeEvent("buff_atk_001", "战意昂扬", "你感到一股热血涌上心头，战意沸腾，手中的武器似乎也变得更加锋利。", DungeonEventType.Buff, 20, "{\"buffType\":\"AttackUp\",\"value\":20,\"isPercent\":true,\"duration\":3,\"durationType\":\"battle\"}", null),
            MakeEvent("buff_atk_002", "狂暴之力", "一股狂暴的力量涌入你的身体，你的双眼泛红，攻击力暴涨！但理智也在逐渐消退……", DungeonEventType.Buff, 10, "{\"buffType\":\"AttackUp\",\"value\":50,\"isPercent\":true,\"duration\":1,\"durationType\":\"battle\"}", null),
            MakeEvent("buff_def_001", "铁壁金身", "你运转功法，体表泛起一层金色光芒，仿佛穿上了一层无形的铠甲。", DungeonEventType.Buff, 20, "{\"buffType\":\"DefenseUp\",\"value\":30,\"isPercent\":true,\"duration\":3,\"durationType\":\"battle\"}", null),
            MakeEvent("buff_def_002", "玄武护体", "你感应到远古玄武的残念，一层厚重的灵力护罩将你包裹其中。", DungeonEventType.Buff, 10, "{\"buffType\":\"DefenseUp\",\"value\":50,\"isPercent\":true,\"duration\":2,\"durationType\":\"battle\"}", null),
            MakeEvent("buff_crit_001", "锐利之眼", "你的双眼变得锐利无比，敌人的每一个破绽都清晰可见。", DungeonEventType.Buff, 20, "{\"buffType\":\"CritRateUp\",\"value\":15,\"isPercent\":true,\"duration\":3,\"durationType\":\"battle\"}", null),
            MakeEvent("buff_crit_002", "必杀之心", "你的心境进入一种奇妙的状态，每一次出手都直指要害。", DungeonEventType.Buff, 10, "{\"buffType\":\"CritRateUp\",\"value\":30,\"isPercent\":true,\"duration\":2,\"durationType\":\"battle\"}", null),
            MakeEvent("buff_speed_001", "疾风步", "你感到身体变得轻盈如风，步伐快如闪电。", DungeonEventType.Buff, 20, "{\"buffType\":\"SpeedUp\",\"value\":30,\"isPercent\":true,\"duration\":3,\"durationType\":\"battle\"}", null),
            MakeEvent("buff_combo_001", "连击之势", "你领悟了一种连绵不绝的攻击节奏，出手越来越快。", DungeonEventType.Buff, 15, "{\"buffType\":\"ComboRateUp\",\"value\":20,\"isPercent\":true,\"duration\":2,\"durationType\":\"battle\"}", null),
            MakeEvent("buff_counter_001", "反击之姿", "你摆出了一种独特的防御姿态，敌人攻击的瞬间便是你反击的时机。", DungeonEventType.Buff, 15, "{\"buffType\":\"CounterRateUp\",\"value\":20,\"isPercent\":true,\"duration\":2,\"durationType\":\"battle\"}", null),
            MakeEvent("buff_dodge_001", "鬼魅身法", "你的身法变得飘忽不定，如同鬼魅一般难以捉摸。", DungeonEventType.Buff, 20, "{\"buffType\":\"DodgeRateUp\",\"value\":25,\"isPercent\":true,\"duration\":3,\"durationType\":\"battle\"}", null),
            MakeEvent("buff_hit_001", "鹰眼术", "你凝聚灵力于双眼，视野瞬间清晰数倍，再快的敌人也逃不过你的眼睛。", DungeonEventType.Buff, 20, "{\"buffType\":\"HitRateUp\",\"value\":20,\"isPercent\":true,\"duration\":3,\"durationType\":\"battle\"}", null),
            MakeEvent("buff_lifesteal_001", "吸血光环", "你的武器泛起诡异的红光，每一次攻击都能汲取敌人的生命力。", DungeonEventType.Buff, 15, "{\"buffType\":\"Lifesteal\",\"value\":10,\"isPercent\":true,\"duration\":2,\"durationType\":\"battle\"}", null),
            MakeEvent("buff_shield_001", "灵力护盾", "你在身周凝聚了一层灵力护盾，能够抵挡一定的伤害。", DungeonEventType.Buff, 15, "{\"buffType\":\"Shield\",\"value\":500,\"isPercent\":false,\"duration\":1,\"durationType\":\"battle\"}", null),
            MakeEvent("buff_regen_001", "生命涌泉", "一股温暖的生命之力在你体内流淌，每场战斗后都能自动恢复一些伤势。", DungeonEventType.Buff, 15, "{\"buffType\":\"HealOverTime\",\"value\":10,\"isPercent\":true,\"duration\":3,\"durationType\":\"battle\"}", null),
            MakeEvent("buff_regen_002", "灵力回复", "你的经脉中灵力流转不息，每场战斗后都能自动回复一些灵力。", DungeonEventType.Buff, 15, "{\"buffType\":\"ManaRegen\",\"value\":10,\"isPercent\":true,\"duration\":3,\"durationType\":\"battle\"}", null),
            MakeEvent("buff_undying_001", "不屈意志", "你在心中立下誓言：纵使身死，也要战到最后一刻。一股不屈的意志支撑着你。", DungeonEventType.Buff, 5, "{\"buffType\":\"Undying\",\"value\":1,\"isPercent\":false,\"duration\":999,\"durationType\":\"dungeon\"}", null),
            MakeEvent("buff_element_001", "灵根觉醒", "你的灵根忽然发出耀眼的光芒，五行相克的力量在你体内觉醒。", DungeonEventType.Buff, 10, "{\"buffType\":\"ElementBonus\",\"value\":50,\"isPercent\":true,\"duration\":3,\"durationType\":\"battle\"}", null),
            MakeEvent("buff_allstats_001", "天人合一", "你进入了一种天人合一的境界，身心与天地融为一体，实力全面提升。", DungeonEventType.Buff, 8, "{\"buffType\":\"AllStatsUp\",\"value\":10,\"isPercent\":true,\"duration\":2,\"durationType\":\"battle\"}", null),
            MakeEvent("buff_allstats_002", "秘境祝福", "秘境的力量眷顾了你，一股神秘的祝福之力灌入你的全身。", DungeonEventType.Buff, 5, "{\"buffType\":\"AllStatsUp\",\"value\":25,\"isPercent\":true,\"duration\":1,\"durationType\":\"battle\"}", null),
        ];

        // ── 减益/陷阱类 (16个) ──

        private static List<DungeonEventConfigEntity> BuildDebuffEvents() =>
        [
            MakeEvent("debuff_poison_001", "毒沼陷阱", "你不慎踩入一片毒沼，紫黑色的毒液瞬间浸透了你的鞋袜，一股剧痛从脚底传来。", DungeonEventType.Debuff, 20, "{\"debuffType\":\"DamagePercent\",\"value\":15,\"target\":\"hp\",\"duration\":0}", null),
            MakeEvent("debuff_poison_002", "毒雾弥漫", "周围忽然弥漫起一阵紫色的毒雾，你连忙屏住呼吸，但还是吸入了不少毒素。", DungeonEventType.Debuff, 15, "{\"debuffType\":\"DamageOverTime\",\"value\":5,\"target\":\"hp\",\"duration\":3,\"durationType\":\"battle\"}", null),
            MakeEvent("debuff_fire_001", "烈焰陷阱", "脚下的地面忽然裂开，滚烫的岩浆喷涌而出！你被灼伤了。", DungeonEventType.Debuff, 15, "{\"debuffType\":\"DamagePercent\",\"value\":20,\"target\":\"hp\",\"duration\":0}", null),
            MakeEvent("debuff_fire_002", "地火灼烧", "一股地底的火焰从石缝中窜出，灼烧着你的身体，疼痛难忍。", DungeonEventType.Debuff, 12, "{\"debuffType\":\"DamageOverTime\",\"value\":8,\"target\":\"hp\",\"duration\":2,\"durationType\":\"battle\"}", null),
            MakeEvent("debuff_atk_down_001", "虚弱诅咒", "一道黑色的咒文缠上了你的身体，你感到力量正在被抽走。", DungeonEventType.Debuff, 18, "{\"debuffType\":\"AttackDown\",\"value\":20,\"isPercent\":true,\"duration\":3,\"durationType\":\"battle\"}", null),
            MakeEvent("debuff_def_down_001", "破甲陷阱", "你不小心触发了一个机关，数根钢针刺穿了你的护甲，防御力大减。", DungeonEventType.Debuff, 15, "{\"debuffType\":\"DefenseDown\",\"value\":30,\"isPercent\":true,\"duration\":2,\"durationType\":\"battle\"}", null),
            MakeEvent("debuff_speed_down_001", "泥沼陷阱", "你一脚踩入泥沼，双腿深陷其中，费了好大劲才拔出来，但行动变得迟缓了。", DungeonEventType.Debuff, 15, "{\"debuffType\":\"SpeedDown\",\"value\":30,\"isPercent\":true,\"duration\":3,\"durationType\":\"battle\"}", null),
            MakeEvent("debuff_crit_down_001", "迷雾笼罩", "浓雾忽然笼罩了四周，你的视野变得模糊，很难再找到敌人的破绽。", DungeonEventType.Debuff, 15, "{\"debuffType\":\"CritRateDown\",\"value\":15,\"isPercent\":true,\"duration\":3,\"durationType\":\"battle\"}", null),
            MakeEvent("debuff_silence_001", "封印之地", "你踏入了一片被封印的区域，体内的灵力被一股神秘力量压制，无法运转功法。", DungeonEventType.Debuff, 10, "{\"debuffType\":\"Silence\",\"duration\":1,\"durationType\":\"battle\"}", null),
            MakeEvent("debuff_stun_001", "雷击陷阱", "天空忽然劈下一道闪电，正中你的身体！你被电得浑身麻痹，动弹不得。", DungeonEventType.Debuff, 8, "{\"debuffType\":\"Stun\",\"duration\":1,\"durationType\":\"battle\"}", null),
            MakeEvent("debuff_stun_002", "被雷劈", "你正走着，忽然一道紫色的雷霆从天而降，直接劈在你头上！你整个人都麻了。", DungeonEventType.Debuff, 6, "{\"debuffType\":\"Stun\",\"duration\":1,\"durationType\":\"battle\",\"instantDamage\":\"10%\"}", null),
            MakeEvent("debuff_heal_block_001", "禁疗领域", "你进入了一片诡异的区域，伤口处泛起黑光，任何疗伤药物都失去了效果。", DungeonEventType.Debuff, 8, "{\"debuffType\":\"HealBlock\",\"duration\":1,\"durationType\":\"battle\"}", null),
            MakeEvent("debuff_mp_drain_001", "灵力枯竭", "你忽然感到丹田一阵剧痛，灵力如潮水般退去，经脉中空空如也。", DungeonEventType.Debuff, 12, "{\"debuffType\":\"DamagePercent\",\"value\":30,\"target\":\"mp\",\"duration\":0}", null),
            MakeEvent("debuff_allstats_001", "咒术缠身", "一道诡异的黑气缠绕在你身上，你感到全身的力量都在衰退。", DungeonEventType.Debuff, 10, "{\"debuffType\":\"AllStatsDown\",\"value\":15,\"isPercent\":true,\"duration\":2,\"durationType\":\"battle\"}", null),
            MakeEvent("debuff_hp_drain_001", "噬魂之地", "脚下的地面泛起血红色的光芒，一股邪恶的力量正在吞噬你的生命力。但你也感受到了一股狂暴的力量。", DungeonEventType.Debuff, 8, "{\"debuffType\":\"DamagePercent\",\"value\":10,\"target\":\"hp\",\"duration\":0,\"bonusBuff\":{\"buffType\":\"AttackUp\",\"value\":10,\"duration\":1}}", null),
            MakeEvent("debuff_curse_001", "厄运缠身", "你感到一股不祥的气息笼罩着自己，仿佛厄运即将降临。", DungeonEventType.Debuff, 10, "{\"debuffType\":\"WeightModifier\",\"targetType\":4,\"multiplier\":2,\"duration\":1,\"durationType\":\"tick\"}", null),
        ];

        // ── 资源获取类 (20个) ──

        private static List<DungeonEventConfigEntity> BuildResourceEvents() =>
        [
            MakeEvent("res_gold_001", "散落铜钱", "草丛中散落着几枚铜钱，大概是以前的探险者遗落的。你弯腰捡了起来。", DungeonEventType.Resource, 30, "{\"rewardType\":\"Gold\",\"amount\":\"100-500\"}", false),
            MakeEvent("res_gold_002", "藏金洞", "你发现了一个隐蔽的小洞穴，里面堆着一堆闪闪发光的金币！", DungeonEventType.Resource, 15, "{\"rewardType\":\"Gold\",\"amount\":\"1000-3000\"}", true),
            MakeEvent("res_gold_003", "金矿脉", "你敲开一块岩石，里面竟然是纯金矿脉！你兴奋地采集了不少。", DungeonEventType.Resource, 5, "{\"rewardType\":\"Gold\",\"amount\":\"5000-10000\"}", true),
            MakeEvent("res_spirit_001", "灵石碎片", "地上有几块碎裂的灵石，虽然不大但灵力尚存。", DungeonEventType.Resource, 25, "{\"rewardType\":\"SpiritStone\",\"amount\":\"10-30\"}", false),
            MakeEvent("res_spirit_002", "灵石矿", "你发现了一处灵石矿脉，灵力充沛，你采集了不少灵石。", DungeonEventType.Resource, 12, "{\"rewardType\":\"SpiritStone\",\"amount\":\"50-100\"}", true),
            MakeEvent("res_spirit_003", "灵石宝藏", "一个隐藏的宝库中堆满了高品质灵石，你简直不敢相信自己的眼睛！", DungeonEventType.Resource, 5, "{\"rewardType\":\"SpiritStone\",\"amount\":\"200-500\"}", true),
            MakeEvent("res_exp_001", "灵光一闪", "你忽然有所感悟，对修炼之道有了更深的理解。", DungeonEventType.Resource, 25, "{\"rewardType\":\"Exp\",\"amount\":\"500-1000\"}", false),
            MakeEvent("res_exp_002", "悟道石壁", "石壁上刻着古老的修炼心得，你驻足参悟，受益匪浅。", DungeonEventType.Resource, 12, "{\"rewardType\":\"Exp\",\"amount\":\"2000-5000\"}", true),
            MakeEvent("res_exp_003", "传承之地", "你来到一处古老的传承之地，残留的道韵涌入你的脑海，你对大道的领悟突飞猛进！", DungeonEventType.Resource, 5, "{\"rewardType\":\"Exp\",\"amount\":\"10000-20000\"}", true),
            MakeEvent("res_exp_004", "感悟顿悟", "你盘膝而坐，忽然进入了一种奇妙的顿悟状态，修为突飞猛进！", DungeonEventType.Resource, 8, "{\"rewardType\":\"Exp\",\"amount\":\"3000-8000\"}", true),
            MakeEvent("res_item_alchemy_herb", "灵草采集", "溪边长着几株灵草，虽然普通但也是炼丹的好材料。", DungeonEventType.Resource, 20, "{\"rewardType\":\"Item\",\"itemId\":\"alchemy_herb\",\"quantity\":\"1-3\"}", false),
            MakeEvent("res_item_herb_002", "珍稀灵草", "崖壁上长着一株散发着七彩光芒的灵草，这可是难得的珍品！", DungeonEventType.Resource, 8, "{\"rewardType\":\"Item\",\"itemId\":\"spirit_water\",\"quantity\":1}", true),
            MakeEvent("res_item_ore_t1", "矿石采集", "你注意到岩壁上有矿石的痕迹，敲了几块下来。", DungeonEventType.Resource, 18, "{\"rewardType\":\"Item\",\"itemId\":\"ore_t1\",\"quantity\":\"1-3\"}", false),
            MakeEvent("res_item_ore_002", "稀有矿脉", "你发现了一处稀有矿脉，矿石散发着奇异的光芒。", DungeonEventType.Resource, 8, "{\"rewardType\":\"Item\",\"itemId\":\"ore_t3\",\"quantity\":1}", true),
            MakeEvent("res_item_beast_core", "妖兽材料", "地上散落着妖兽蜕下的皮和角，这些都是炼器的好材料。", DungeonEventType.Resource, 15, "{\"rewardType\":\"Item\",\"itemId\":\"beast_core\",\"quantity\":\"1-2\"}", false),
            MakeEvent("res_item_mat_002", "上古遗物", "你在废墟中发现了一件散发着远古气息的遗物。", DungeonEventType.Resource, 5, "{\"rewardType\":\"Item\",\"itemId\":\"evolution_stone\",\"quantity\":1}", true),
            MakeEvent("res_equip_001", "废弃装备", "角落里堆着一些破旧的装备，虽然品相不佳但还能用。", DungeonEventType.Resource, 12, "{\"rewardType\":\"Equipment\",\"quality\":\"1-2\"}", false),
            MakeEvent("res_equip_002", "精良装备", "你在一个石台上发现了一件保养精良的装备，看起来是某位前辈留下的。", DungeonEventType.Resource, 6, "{\"rewardType\":\"Equipment\",\"quality\":\"3\"}", true),
            MakeEvent("res_equip_003", "神兵利器", "一道金光从石壁中透出，你挖开一看，竟是一件品质极高的神兵！", DungeonEventType.Resource, 2, "{\"rewardType\":\"Equipment\",\"quality\":\"4-5\"}", true),
            MakeEvent("res_collection_001", "残破古卷", "你捡到了一卷残破的古卷，上面的文字已经模糊不清，但还能辨认出一些内容。", DungeonEventType.Resource, 10, "{\"rewardType\":\"Collection\",\"collectionType\":\"text\"}", false),
            MakeEvent("res_collection_002", "上古秘典", "石壁上刻着一幅精美的图案，你将其拓印下来，这是一份珍贵的图鉴资料。", DungeonEventType.Resource, 5, "{\"rewardType\":\"Collection\",\"collectionType\":\"image\"}", true),
        ];

        // ── 宝箱类 (9个) ──

        private static List<DungeonEventConfigEntity> BuildTreasureEvents() =>
        [
            MakeEvent("chest_common_001", "铜宝箱", "墙角放着一个锈迹斑斑的铜宝箱，你打开一看，里面还有一些零散的钱币和小物件。", DungeonEventType.Treasure, 30, "{\"rewards\":[{\"type\":\"Gold\",\"amount\":\"200-500\"},{\"type\":\"Item\",\"id\":\"random\",\"count\":1}]}", false),
            MakeEvent("chest_common_002", "铁宝箱", "一个铁制宝箱半埋在碎石中，你费了点力气才打开它。", DungeonEventType.Treasure, 25, "{\"rewards\":[{\"type\":\"Gold\",\"amount\":\"500-1000\"},{\"type\":\"Item\",\"id\":\"random\",\"count\":2}]}", false),
            MakeEvent("chest_silver_001", "银宝箱", "石台上放着一个银光闪闪的宝箱，看起来保养得不错。你小心翼翼地打开，里面有不少好东西。", DungeonEventType.Treasure, 15, "{\"rewards\":[{\"type\":\"Gold\",\"amount\":\"1000-2000\"},{\"type\":\"Equipment\",\"quality\":\"2-3\"},{\"type\":\"Item\",\"id\":\"random\",\"count\":1}]}", true),
            MakeEvent("chest_gold_001", "金宝箱", "一个金色的宝箱散发着柔和的光芒，你打开后金光四溢，里面装满了财宝！", DungeonEventType.Treasure, 10, "{\"rewards\":[{\"type\":\"Gold\",\"amount\":\"3000-5000\"},{\"type\":\"SpiritStone\",\"amount\":\"50-100\"},{\"type\":\"Equipment\",\"quality\":\"3\"}]}", true),
            MakeEvent("chest_diamond_001", "钻石宝箱", "宝石镶嵌的宝箱在火把的照耀下熠熠生辉，你从未见过如此精美的宝箱。打开后，里面的宝物让你目瞪口呆！", DungeonEventType.Treasure, 5, "{\"rewards\":[{\"type\":\"Gold\",\"amount\":\"5000-10000\"},{\"type\":\"SpiritStone\",\"amount\":\"100-200\"},{\"type\":\"Equipment\",\"quality\":\"4\"},{\"type\":\"Item\",\"id\":\"random\",\"count\":1}]}", true),
            MakeEvent("chest_legend_001", "传说宝箱", "一个散发着远古气息的宝箱悬浮在半空中，上面刻满了神秘的符文。你伸手触碰，宝箱自动打开，传说中的宝物就在眼前！", DungeonEventType.Treasure, 2, "{\"rewards\":[{\"type\":\"Gold\",\"amount\":\"10000\"},{\"type\":\"SpiritStone\",\"amount\":\"300\"},{\"type\":\"Equipment\",\"quality\":\"5\"},{\"type\":\"Item\",\"id\":\"random\",\"count\":1},{\"type\":\"Collection\",\"collectionType\":\"image\"}]}", true),
            MakeEvent("chest_trap_001", "陷阱宝箱", "你发现了一个宝箱，刚伸手去开，宝箱忽然弹开，一只凶猛的守护兽从里面跳了出来！", DungeonEventType.Treasure, 8, "{\"trapBattle\":true,\"monsterId\":\"monster_elite_001\"}", null),
            MakeEvent("chest_mimic_001", "拟态宝箱", "你看到一个宝箱，走近一看，宝箱忽然张开了血盆大口——这是一个拟态怪物！", DungeonEventType.Treasure, 6, "{\"trapBattle\":true,\"monsterId\":\"monster_004\"}", null),
            MakeEvent("chest_locked_001", "上锁的宝箱", "你发现了一个精致的宝箱，但上面有一把复杂的锁。你需要钥匙才能打开它。", DungeonEventType.Treasure, 5, "{\"requireItem\":\"key_001\",\"rewards\":[{\"type\":\"Gold\",\"amount\":\"2000-5000\"},{\"type\":\"Equipment\",\"quality\":\"3-4\"}]}", true),
        ];

        // ── 奇遇类 (19个) ──

        private static List<DungeonEventConfigEntity> BuildAdventureEvents() =>
        [
            MakeEvent("adv_sage_001", "隐世高人", "一棵古树下坐着一位白发老者，他微微一笑：\"有缘人，老夫送你一场造化。\"说着，一道金光没入你的体内。", DungeonEventType.Adventure, 10, "{\"adventureType\":\"randomBuff\",\"buffPool\":[\"AttackUp\",\"DefenseUp\",\"CritRateUp\",\"SpeedUp\",\"AllStatsUp\"],\"duration\":3,\"durationType\":\"battle\"}", null),
            MakeEvent("adv_sage_002", "传功长老", "一位鹤发童颜的长者盘膝坐在巨石上，他看了你一眼，点了点头：\"资质不错，老夫传你一些功力。\"", DungeonEventType.Adventure, 10, "{\"adventureType\":\"grantExp\",\"amount\":\"5000-10000\"}", null),
            MakeEvent("adv_sage_003", "炼丹师", "你闻到一股药香，循着气味走去，发现一位炼丹师正在炼丹。他随手递给你几颗：\"拿着吧，小友。\"", DungeonEventType.Adventure, 10, "{\"adventureType\":\"grantItem\",\"itemId\":\"item_001\",\"quantity\":\"1-2\"}", null),
            MakeEvent("adv_sage_004", "铸剑师", "一位满身肌肉的铸剑师正在打铁，他看了看你手中的武器，说道：\"让老夫帮你淬炼一番。\"", DungeonEventType.Adventure, 8, "{\"adventureType\":\"permanentBuff\",\"buffType\":\"AttackUp\",\"value\":15,\"isPercent\":true}", null),
            MakeEvent("adv_spirit_001", "灵兽幼崽", "草丛中传来微弱的叫声，你拨开草丛，发现一只受伤的灵兽幼崽。你为它疗伤后，它感激地送你一些礼物。", DungeonEventType.Adventure, 10, "{\"adventureType\":\"grantItem\",\"itemId\":\"pill_exp_small\",\"quantity\":\"1-3\"}", null),
            MakeEvent("adv_spirit_002", "灵脉共振", "你脚下的地面忽然泛起光芒，一股强大的灵力从地底涌出，与你体内的灵力产生共振。", DungeonEventType.Adventure, 8, "{\"adventureType\":\"permanentBuff\",\"buffType\":\"AllStatsUp\",\"value\":10,\"isPercent\":true}", null),
            MakeEvent("adv_riddle_001", "石碑谜题", "你发现一块古老的石碑，上面刻着一道谜题。你冥思苦想……", DungeonEventType.Adventure, 10, "{\"adventureType\":\"riddle\",\"successRate\":50,\"successReward\":{\"type\":\"Gold\",\"amount\":\"2000-5000\"},\"failDebuff\":{\"debuffType\":\"AllStatsDown\",\"value\":10,\"duration\":2}}", null),
            MakeEvent("adv_choice_001", "分岔路口", "前方出现两条路：左边阳光明媚，右边阴风阵阵。你选择了……", DungeonEventType.Adventure, 10, "{\"adventureType\":\"choice\",\"optionA\":{\"name\":\"左边\",\"reward\":{\"type\":\"Gold\",\"amount\":\"1000-3000\"}},\"optionB\":{\"name\":\"右边\",\"reward\":{\"type\":\"SpiritStone\",\"amount\":\"50-100\"}}}", null),
            MakeEvent("adv_choice_002", "神秘祭坛", "一座古老的祭坛矗立在眼前，上面刻着：\"献上你的血，换取力量。\"你是否要献祭？", DungeonEventType.Adventure, 8, "{\"adventureType\":\"sacrifice\",\"hpCost\":\"20%\",\"reward\":{\"buffType\":\"AttackUp\",\"value\":30,\"duration\":3,\"durationType\":\"battle\"}}", null),
            MakeEvent("adv_choice_003", "遗忘之泉", "一汪清澈的泉水散发着柔和的光芒，旁边的石碑上写着：\"饮此泉水，可消百厄。\"", DungeonEventType.Adventure, 10, "{\"adventureType\":\"purgeDebuff\"}", null),
            MakeEvent("adv_choice_004", "天平之厅", "你进入一个大厅，中央有一个天平。左边放着\"生命\"，右边放着\"力量\"。天平等待你的选择。", DungeonEventType.Adventure, 8, "{\"adventureType\":\"choice\",\"optionA\":{\"name\":\"生命\",\"reward\":{\"type\":\"Heal\",\"healPercent\":30}},\"optionB\":{\"name\":\"力量\",\"reward\":{\"type\":\"Buff\",\"buffType\":\"AttackUp\",\"value\":20,\"duration\":2}}}", null),
            MakeEvent("adv_fairy_001", "仙子赐福", "一位白衣仙子从天而降，她轻轻一挥手，一道温暖的光芒笼罩了你。", DungeonEventType.Adventure, 3, "{\"adventureType\":\"fullRestore\",\"extraBuff\":{\"buffType\":\"AllStatsUp\",\"value\":15,\"duration\":2,\"durationType\":\"battle\"}}", null),
            MakeEvent("adv_demon_001", "魔气侵袭", "一股浓郁的魔气忽然涌入你的身体，你感到剧痛无比，但同时也感受到了一股狂暴的力量。", DungeonEventType.Adventure, 6, "{\"adventureType\":\"risk\",\"hpCost\":\"25%\",\"winRate\":60,\"bigReward\":{\"type\":\"Buff\",\"buffType\":\"AttackUp\",\"value\":25,\"duration\":3},\"smallReward\":{\"type\":\"Gold\",\"amount\":\"100-500\"}}", null),
            MakeEvent("adv_gamble_001", "赌博石台", "你发现一个石台，上面刻着：\"投下一千金币，命运将给你答案。\"你是否要试试运气？", DungeonEventType.Adventure, 8, "{\"adventureType\":\"gamble\",\"costType\":\"Gold\",\"costAmount\":1000,\"winRate\":50,\"winReward\":{\"type\":\"Gold\",\"amount\":3000},\"losePenalty\":{\"type\":\"Gold\",\"amount\":500}}", null),
            MakeEvent("adv_gamble_002", "命运之轮", "一个巨大的转盘悬浮在空中，上面刻着各种奖励。你伸手转动了它……", DungeonEventType.Adventure, 6, "{\"adventureType\":\"wheel\",\"rewards\":[{\"type\":\"Gold\",\"amount\":\"500-1000\",\"weight\":40},{\"type\":\"Gold\",\"amount\":\"2000-5000\",\"weight\":20},{\"type\":\"SpiritStone\",\"amount\":\"50-100\",\"weight\":20},{\"type\":\"Equipment\",\"quality\":\"3-4\",\"weight\":15},{\"type\":\"Equipment\",\"quality\":\"5\",\"weight\":5}]}", null),
            MakeEvent("adv_rest_001", "安全营地", "你发现了一处隐蔽的山洞，洞口有篝火的痕迹，看起来曾经有人在这里歇脚。你决定休息一会儿。", DungeonEventType.Adventure, 12, "{\"adventureType\":\"rest\",\"healHpPercent\":50,\"healMpPercent\":30,\"buff\":{\"buffType\":\"DefenseUp\",\"value\":20,\"duration\":1,\"durationType\":\"battle\"}}", null),
            MakeEvent("adv_trap_001", "伪装的奇遇", "你看到远处有一道金光，兴奋地跑过去，却发现那是一个精心布置的陷阱！", DungeonEventType.Adventure, 6, "{\"adventureType\":\"fake\",\"realEvent\":\"debuff_poison_001\"}", null),
            MakeEvent("adv_chest_001", "遗落宝箱", "你在角落里发现了一个被遗忘的宝箱，上面布满了灰尘。你打开一看，里面竟然还有宝物！", DungeonEventType.Adventure, 8, "{\"adventureType\":\"treasureChest\",\"quantity\":1}", null),
            MakeEvent("adv_lore_t1", "古代壁画", "洞壁上画着精美的壁画，描绘着远古修士的战斗场景。你仔细观摩，有所感悟。", DungeonEventType.Adventure, 10, "{\"adventureType\":\"lore\"}", null),
        ];

        // ── 商店/交易类 (4个) ──

        private static List<DungeonEventConfigEntity> BuildShopEvents() =>
        [
            MakeEvent("shop_general_001", "行商出现", "一个背着大包小包的行商从远处走来，热情地招呼你：\"道友，赶路辛苦了，看看有没有需要的？\"", DungeonEventType.Shop, 25, "{\"items\":[{\"itemId\":\"item_001\",\"price\":300,\"weight\":30},{\"itemId\":\"item_006\",\"price\":300,\"weight\":25},{\"itemId\":\"alchemy_herb\",\"price\":100,\"weight\":20}],\"healing\":{\"hpPerGold\":10,\"mpPerGold\":10,\"weight\":30},\"enhance\":{\"type\":\"attack\",\"value\":10,\"cost\":500,\"weight\":15}}", null),
            MakeEvent("shop_rare_001", "稀有商人", "一位衣着华贵的商人坐在路边，他的摊位上摆着几件品质不错的装备。\"识货的来看看。\"", DungeonEventType.Shop, 20, "{\"items\":[{\"equipId\":101,\"price\":2000,\"weight\":20},{\"equipId\":102,\"price\":3000,\"weight\":15},{\"itemId\":\"item_002\",\"price\":800,\"weight\":30}],\"healing\":{\"hpPerGold\":15,\"mpPerGold\":15,\"weight\":25},\"enhance\":{\"type\":\"defense\",\"value\":15,\"cost\":800,\"weight\":10}}", null),
            MakeEvent("shop_healer_001", "游方郎中", "一位背着药箱的郎中走了过来，看你一身伤痕，说道：\"道友受伤不轻，老夫可以帮你疗伤，不过需要一些诊金。\"", DungeonEventType.Shop, 20, "{\"items\":[{\"itemId\":\"item_001\",\"price\":200,\"weight\":20},{\"itemId\":\"item_002\",\"price\":600,\"weight\":15}],\"healing\":{\"hpPerGold\":20,\"mpPerGold\":10,\"weight\":60},\"enhance\":{\"type\":\"attack\",\"value\":5,\"cost\":300,\"weight\":5}}", null),
            MakeEvent("shop_smith_001", "流浪铁匠", "一个满身肌肉的铁匠正在路边生火打铁，他看了看你说道：\"你的武器需要淬炼吗？老夫手艺还行。\"", DungeonEventType.Shop, 15, "{\"items\":[{\"itemId\":\"ore_t1\",\"price\":150,\"weight\":20},{\"itemId\":\"beast_core\",\"price\":200,\"weight\":15}],\"healing\":{\"hpPerGold\":5,\"mpPerGold\":5,\"weight\":15},\"enhance\":{\"type\":\"attack\",\"value\":20,\"cost\":1000,\"weight\":50}}", null),
        ];

        // ── 特殊事件类 (18个) ──

        private static List<DungeonEventConfigEntity> BuildSpecialEvents() =>
        [
            MakeEvent("event_weather_001", "灵气暴涌", "天地间的灵气忽然变得异常浓郁，你感到浑身充满了力量。这是一个难得的修炼良机！", DungeonEventType.Event, 10, "{\"effectType\":\"rewardMultiplier\",\"value\":2,\"duration\":3,\"durationType\":\"tick\"}", null),
            MakeEvent("event_weather_002", "魔气弥漫", "天空忽然变得阴沉，一股浓郁的魔气从地底涌出，周围的怪物变得更加狂暴。", DungeonEventType.Event, 8, "{\"effectType\":\"monsterBuff\",\"value\":20,\"duration\":3,\"durationType\":\"tick\"}", null),
            MakeEvent("event_weather_003", "祥瑞之兆", "天空中出现了一道七彩祥云，你感到一股祥和之气笼罩四周。好兆头！", DungeonEventType.Event, 6, "{\"effectType\":\"rareBoost\",\"value\":50,\"duration\":3,\"durationType\":\"tick\"}", null),
            MakeEvent("event_weather_004", "灾厄降临", "天空忽然乌云密布，电闪雷鸣。你感到一股不祥的气息，仿佛灾厄即将降临。", DungeonEventType.Event, 6, "{\"effectType\":\"trapBoost\",\"value\":50,\"duration\":3,\"durationType\":\"tick\"}", null),
            MakeEvent("event_weather_005", "月华如水", "夜幕降临，皎洁的月光洒满大地。月华之力渗入你的体内，治愈着你的伤势。", DungeonEventType.Event, 8, "{\"effectType\":\"regenTick\",\"healHpPercent\":5,\"healMpPercent\":5,\"duration\":3,\"durationType\":\"tick\"}", null),
            MakeEvent("event_weather_006", "日精灌顶", "正午的阳光格外炽烈，一道精纯的太阳之力灌入你的天灵盖。", DungeonEventType.Event, 5, "{\"effectType\":\"buffMultiplier\",\"buffCategory\":\"attack\",\"value\":2,\"duration\":2,\"durationType\":\"tick\"}", null),
            MakeEvent("event_env_001", "地动山摇", "脚下的地面忽然剧烈震动，你不得不蹲下稳住身形。碎石纷纷落下，你只能等待震动过去。", DungeonEventType.Event, 8, "{\"effectType\":\"skipTick\"}", null),
            MakeEvent("event_env_002", "灵雾散开", "笼罩四周的灵雾忽然散去，视野变得开阔起来。你隐约看到了远处的宝箱和奇遇。", DungeonEventType.Event, 6, "{\"effectType\":\"weightModifier\",\"boostTypes\":[6,7],\"multiplier\":2,\"duration\":1,\"durationType\":\"tick\"}", null),
            MakeEvent("event_env_003", "暗流涌动", "你感到脚下的地面传来微弱的震动，远处传来低沉的咆哮声。危险正在逼近。", DungeonEventType.Event, 6, "{\"effectType\":\"weightModifier\",\"boostTypes\":[1,4],\"multiplier\":2,\"duration\":1,\"durationType\":\"tick\"}", null),
            MakeEvent("event_env_004", "天降甘霖", "天空忽然下起了细雨，雨水带着淡淡的灵气，滋润着万物。", DungeonEventType.Event, 10, "{\"effectType\":\"instantHeal\",\"healHpPercent\":20,\"healMpPercent\":20}", null),
            MakeEvent("event_env_005", "星辰之力", "夜空中繁星点点，星辰之力洒落大地，你沐浴其中，感到灵力涌动。", DungeonEventType.Event, 6, "{\"effectType\":\"instantBuff\",\"buffType\":\"AllStatsUp\",\"value\":5,\"duration\":2,\"durationType\":\"battle\"}", null),
            MakeEvent("event_mystery_001", "神秘声音", "你忽然听到一个神秘的声音在耳边回响，但你无法分辨它来自何方。声音似乎在说……", DungeonEventType.Event, 5, "{\"effectType\":\"mystery\",\"options\":[\"fullRestore\",\"allStatsBuff\",\"forceBattle\",\"noEffect\"],\"weights\":[25,25,25,25]}", null),
            MakeEvent("event_mystery_002", "命运转盘", "一个金色的转盘凭空出现，上面刻着各种命运。转盘开始旋转，指针缓缓停下……", DungeonEventType.Event, 5, "{\"effectType\":\"mystery\",\"options\":[\"legendEquipment\",\"goldBonus\",\"expBonus\",\"debuff\",\"noEffect\"],\"weights\":[5,30,30,20,15]}", null),
            MakeEvent("event_mystery_003", "时空裂缝", "你面前的空间忽然出现了一道裂缝，裂缝中透出奇异的光芒。你伸手触碰……", DungeonEventType.Event, 4, "{\"effectType\":\"randomEvent\",\"eventTypes\":[6,3,4,1],\"weights\":[30,30,20,20]}", null),
            MakeEvent("event_buff_extend_001", "时光延展", "你感到时间的流速变得缓慢，身上的增益效果似乎延长了。", DungeonEventType.Event, 6, "{\"effectType\":\"extendBuffs\",\"duration\":1}", null),
            MakeEvent("event_buff_purge_001", "净化之光", "一道圣洁的光芒从天而降，笼罩了你的全身。你感到身上的负面效果正在消散。", DungeonEventType.Event, 8, "{\"effectType\":\"purgeDebuffs\"}", null),
            MakeEvent("event_hp_swap_001", "生命交换", "你触碰了一个神秘的符文，体内的生命力和灵力忽然产生了剧烈的波动。", DungeonEventType.Event, 4, "{\"effectType\":\"hpMpSwap\"}", null),
            MakeEvent("event_mirror_001", "镜中世界", "你看到一面古镜，镜中的自己忽然走了出来，站在你身旁。\"我来帮你。\"", DungeonEventType.Event, 3, "{\"effectType\":\"summonClone\",\"attackPercent\":40,\"hpPercent\":50,\"duration\":1,\"durationType\":\"battle\"}", null),
            MakeEvent("event_revive_001", "凤凰之羽", "一根散发着火焰光芒的羽毛飘落在你手中。你感到一股强大的生命之力。", DungeonEventType.Event, 2, "{\"effectType\":\"reviveOnce\",\"healPercent\":50}", null),
            MakeEvent("event_treasure_map_001", "藏宝图", "你捡到了一张泛黄的藏宝图，上面标注着附近宝箱的位置。", DungeonEventType.Event, 4, "{\"effectType\":\"weightModifier\",\"boostTypes\":[6],\"multiplier\":100,\"duration\":1,\"durationType\":\"tick\"}", null),
        ];

        // ── 空事件类 (8个) ──

        private static List<DungeonEventConfigEntity> BuildEmptyEvents() =>
        [
            MakeEvent("empty_001", "风平浪静", "四周一片宁静，微风拂过脸颊。你深吸一口气，继续前行。", DungeonEventType.Empty, 25, "{}", null),
            MakeEvent("empty_002", "静谧小径", "你沿着一条安静的小径前行，两旁是茂密的灌木丛。一切都很平静。", DungeonEventType.Empty, 20, "{}", null),
            MakeEvent("empty_003", "荒芜之地", "你来到一片荒芜的区域，地上只有碎石和枯草。什么都没有。", DungeonEventType.Empty, 15, "{}", null),
            MakeEvent("empty_004", "迷雾散去", "笼罩四周的迷雾渐渐散去，你发现自己身处一片空旷的地带。前方什么都没有。", DungeonEventType.Empty, 12, "{}", null),
            MakeEvent("empty_005", "枯井", "你发现了一口枯井，往下望去只有无尽的黑暗。你决定不下去冒险。", DungeonEventType.Empty, 10, "{}", null),
            MakeEvent("empty_006", "断桥", "前方是一座断桥，桥下是深不见底的深渊。你只能绕路而行。", DungeonEventType.Empty, 10, "{}", null),
            MakeEvent("empty_007", "石碑", "你发现一块石碑，上面的文字已经风化得无法辨认。你只能继续前行。", DungeonEventType.Empty, 10, "{}", null),
            MakeEvent("empty_008", "鸟语花香", "你来到一片花丛中，鸟儿在枝头歌唱。你驻足欣赏了片刻，心情愉悦。", DungeonEventType.Empty, 8, "{\"hiddenBuff\":{\"buffType\":\"CritRateUp\",\"value\":1,\"duration\":1,\"durationType\":\"battle\"}}", null),
        ];

        // ── 玩家偶遇类 (3个) ──

        private static List<DungeonEventConfigEntity> BuildPlayerEncounterEvents() =>
        [
            MakeEvent("encounter_friend_001", "故人重逢", "远处走来一个熟悉的身影，定睛一看，竟是一位旧识！你们相视而笑。", DungeonEventType.PlayerEncounter, 35, "{}", null),
            MakeEvent("encounter_rival_001", "狭路相逢", "前方传来一阵脚步声，你警觉地握紧武器。转角处，一个身影出现在你面前——是敌是友？", DungeonEventType.PlayerEncounter, 40, "{}", null),
            MakeEvent("encounter_mystery_001", "神秘来客", "一道身影从迷雾中走出，你看不清对方的面容。空气中弥漫着一股莫名的气息。", DungeonEventType.PlayerEncounter, 25, "{}", null),
        ];

        // ───────────────────── 辅助方法 ─────────────────────

        private static DungeonEventConfigEntity MakeEvent(
            string id, string name, string description,
            DungeonEventType eventType, int weight, string eventDataJson, bool? deathKeep)
        {
            return new DungeonEventConfigEntity
            {
                Id = id,
                Name = name,
                Description = description,
                DungeonId = "*",
                EventType = (int)eventType,
                Weight = weight,
                Enabled = true,
                EventDataJson = eventDataJson,
                DeathKeep = deathKeep,
                SeedKey = BuildEventSeedKey(id),
                IsBuiltIn = true,
                BuiltInVersion = DungeonEventConfigBuiltInVersion,
                LastUpdateTime = DateTime.Now
            };
        }

        // ───────────────────── 同步逻辑 ─────────────────────

        public static async Task SyncTemplatesAsync(IRepository<DungeonInstanceTemplateEntity> repo)
        {
            var expected = BuildTemplates();
            foreach (var item in expected)
            {
                var existing = await repo.GetByIdAsync(item.Id);
                if (existing == null)
                {
                    await repo.AddAsync(item);
                    continue;
                }

                var canOverwrite = existing.IsBuiltIn ||
                    string.Equals(existing.SeedKey, item.SeedKey, StringComparison.OrdinalIgnoreCase);
                if (!canOverwrite) continue;

                existing.Name = item.Name;
                existing.Description = item.Description;
                existing.Enabled = item.Enabled;
                existing.RecommendedLevel = item.RecommendedLevel;
                existing.DailyEnterLimit = item.DailyEnterLimit;
                existing.TickIntervalSeconds = item.TickIntervalSeconds;
                existing.OpenScheduleJson = item.OpenScheduleJson;
                existing.EntryCostsJson = item.EntryCostsJson;
                existing.EventGroupId = item.EventGroupId;
                existing.AutoMedicineConfigJson = item.AutoMedicineConfigJson;
                existing.EncounterConfigJson = item.EncounterConfigJson;
                existing.SeedKey = item.SeedKey;
                existing.IsBuiltIn = true;
                existing.BuiltInVersion = item.BuiltInVersion;
                existing.LastUpdateTime = DateTime.Now;
                await repo.UpdateAsync(existing);
            }
        }

        public static async Task SyncEventConfigsAsync(IRepository<DungeonEventConfigEntity> repo)
        {
            var expected = BuildEventConfigs();
            foreach (var item in expected)
            {
                var existing = await repo.GetByIdAsync(item.Id);
                if (existing == null)
                {
                    await repo.AddAsync(item);
                    continue;
                }

                var canOverwrite = existing.IsBuiltIn ||
                    string.Equals(existing.SeedKey, item.SeedKey, StringComparison.OrdinalIgnoreCase);
                if (!canOverwrite) continue;

                existing.Name = item.Name;
                existing.Description = item.Description;
                existing.DungeonId = item.DungeonId;
                existing.EventType = item.EventType;
                existing.Weight = item.Weight;
                existing.Enabled = item.Enabled;
                existing.EventDataJson = item.EventDataJson;
                existing.DeathKeep = item.DeathKeep;
                existing.SeedKey = item.SeedKey;
                existing.IsBuiltIn = true;
                existing.BuiltInVersion = item.BuiltInVersion;
                existing.LastUpdateTime = DateTime.Now;
                await repo.UpdateAsync(existing);
            }
        }

        // ───────────────────── 事件组种子 ─────────────────────

        public static List<DungeonEventGroupEntity> BuildEventGroups()
        {
            var now = DateTime.Now;
            var allEvents = BuildEventConfigs();

            // 普通秘境事件组 — 各类型均衡
            var normalItems = allEvents.Select(e => new { e.Id, Weight = e.Weight }).ToList();
            // 战斗秘境事件组 — 战斗/减益权重大
            var combatItems = allEvents.Select(e => new
            {
                e.Id,
                Weight = e.EventType switch
                {
                    1 => e.Weight * 2,   // Battle
                    4 => (int)(e.Weight * 1.5), // Debuff
                    2 => e.Weight / 2,   // Heal
                    10 => e.Weight / 2,  // Empty
                    _ => e.Weight
                }
            }).ToList();
            // 宝藏秘境事件组 — 资源/宝箱/奇遇权重大
            var treasureItems = allEvents.Select(e => new
            {
                e.Id,
                Weight = e.EventType switch
                {
                    5 => e.Weight * 2,   // Resource
                    6 => e.Weight * 2,   // Treasure
                    7 => (int)(e.Weight * 1.5), // Adventure
                    1 => e.Weight / 2,   // Battle
                    4 => e.Weight / 2,   // Debuff
                    _ => e.Weight
                }
            }).ToList();

            return
            [
                new DungeonEventGroupEntity
                {
                    Id = "eg_normal",
                    Name = "普通秘境事件组",
                    Description = "各类型事件均衡配置，适合大多数秘境。",
                    GroupItemsJson = JsonSerializer.Serialize(normalItems.Select(i => new { eventId = i.Id, weight = i.Weight })),
                    SeedKey = BuildGroupSeedKey("eg_normal"),
                    IsBuiltIn = true,
                    BuiltInVersion = DungeonEventGroupBuiltInVersion,
                    LastUpdateTime = now
                },
                new DungeonEventGroupEntity
                {
                    Id = "eg_combat",
                    Name = "战斗秘境事件组",
                    Description = "战斗和减益事件权重更高，挑战性更强。",
                    GroupItemsJson = JsonSerializer.Serialize(combatItems.Select(i => new { eventId = i.Id, weight = i.Weight })),
                    SeedKey = BuildGroupSeedKey("eg_combat"),
                    IsBuiltIn = true,
                    BuiltInVersion = DungeonEventGroupBuiltInVersion,
                    LastUpdateTime = now
                },
                new DungeonEventGroupEntity
                {
                    Id = "eg_treasure",
                    Name = "宝藏秘境事件组",
                    Description = "资源、宝箱和奇遇事件权重更高，探索收益更大。",
                    GroupItemsJson = JsonSerializer.Serialize(treasureItems.Select(i => new { eventId = i.Id, weight = i.Weight })),
                    SeedKey = BuildGroupSeedKey("eg_treasure"),
                    IsBuiltIn = true,
                    BuiltInVersion = DungeonEventGroupBuiltInVersion,
                    LastUpdateTime = now
                }
            ];
        }

        public static async Task SyncEventGroupsAsync(IRepository<DungeonEventGroupEntity> repo)
        {
            var expected = BuildEventGroups();
            foreach (var item in expected)
            {
                var existing = await repo.GetByIdAsync(item.Id);
                if (existing == null)
                {
                    await repo.AddAsync(item);
                    continue;
                }

                var canOverwrite = existing.IsBuiltIn ||
                    string.Equals(existing.SeedKey, item.SeedKey, StringComparison.OrdinalIgnoreCase);
                if (!canOverwrite) continue;

                existing.Name = item.Name;
                existing.Description = item.Description;
                existing.GroupItemsJson = item.GroupItemsJson;
                existing.SeedKey = item.SeedKey;
                existing.IsBuiltIn = true;
                existing.BuiltInVersion = item.BuiltInVersion;
                existing.LastUpdateTime = DateTime.Now;
                await repo.UpdateAsync(existing);
            }
        }
    }
}
