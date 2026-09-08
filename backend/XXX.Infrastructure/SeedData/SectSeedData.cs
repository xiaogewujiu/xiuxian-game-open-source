using SqlSugar;

namespace XXX.Infrastructure.SeedData
{
    /// <summary>
    /// 宗门系统种子数据。
    /// 使用 INSERT OR IGNORE 模式避免重复主键错误。
    /// </summary>
    public static class SectSeedData
    {
        /// <summary>
        /// 向 SQLite 开发库播种宗门系统全部基础配置数据。
        /// </summary>
        public static void SeedAll(ISqlSugarClient db)
        {
            SeedSectTemplates(db);
            SeedHeartSutraTemplates(db);
            SeedSectShopConfig(db);
            SeedSectTournamentSchedule(db);
            SeedSectBlessingConfigs(db);
            SeedSectQuestConfigs(db);
        }

        private static void SeedSectTemplates(ISqlSugarClient db)
        {
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var sql = """
                INSERT OR IGNORE INTO sect_templates (SectId, Name, Description, Icon, PortraitPath, HeartSutraIdsJson, IsEnabled, SortOrder, LastUpdateTime) VALUES
                ('sect_tianjian', '天剑宗', '以剑入道，一剑破万法。天剑宗弟子皆以剑意淬体，追求剑道极致。宗门坐落于万剑峰，峰顶常年剑气纵横。', NULL, NULL, '["hs_tianjian_1"]', 1, 1, @now),
                ('sect_xuanming', '玄冥教', '阴寒之力，玄冥真意。玄冥教修炼阴寒功法，以冰封万物为最高境界。隐于北冥深渊之中。', NULL, NULL, '["hs_xuanming_1"]', 1, 2, @now),
                ('sect_qingyun', '青云门', '御风而行，青云直上。青云门弟子擅长风雷之术，身法飘逸，来去如风。', NULL, NULL, '["hs_qingyun_1"]', 1, 3, @now),
                ('sect_tianyin', '天音寺', '梵音入耳，净化心灵。天音寺以佛门功法见长，攻守兼备，可渡万魔。', NULL, NULL, '["hs_tianyin_1"]', 1, 4, @now),
                ('sect_huoling', '火灵宫', '烈焰焚天，火灵宫以火系功法称霸，攻击力冠绝天下。坐落于火山熔岩之上。', NULL, NULL, '["hs_huoling_1"]', 1, 5, @now),
                ('sect_wandu', '万毒门', '百毒不侵，万毒归宗。万毒门精通毒术，以毒攻敌，防不胜防。', NULL, NULL, '["hs_wandu_1"]', 1, 6, @now),
                ('sect_taixu', '太虚殿', '虚无缥缈，太虚之道。太虚殿修炼精神力，擅长控制与辅助，洞察天机。', NULL, NULL, '["hs_taixu_1"]', 1, 7, @now),
                ('sect_longmen', '龙门', '龙脉传承，龙门弟子肉身强横，以力破巧。据说门中有真龙血脉。', NULL, NULL, '["hs_longmen_1"]', 1, 8, @now),
                ('sect_guiyuan', '归元宗', '万法归元，归元宗功法中正平和，全面发展。乃修真界大宗。', NULL, NULL, '["hs_guiyuan_1"]', 1, 9, @now),
                ('sect_mozong', '魔宗', '以魔入道，不拘一格。魔宗功法诡异多变，出其不意，世人又惧又敬。', NULL, NULL, '["hs_mozong_1"]', 1, 10, @now);
                """;
            db.Ado.ExecuteCommand(sql, new { now });
        }

        private static void SeedHeartSutraTemplates(ISqlSugarClient db)
        {
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var templates = new (string SutraId, string Name, string SectId, string LayersJson, int SortOrder)[]
            {
                ("hs_tianjian_1", "御剑心经", "sect_tianjian", BuildTianjianLayers(), 1),
                ("hs_xuanming_1", "寒冰真诀", "sect_xuanming", BuildXuanmingLayers(), 1),
                ("hs_qingyun_1", "青云心法", "sect_qingyun", BuildQingyunLayers(), 1),
                ("hs_tianyin_1", "梵音心经", "sect_tianyin", BuildTianyinLayers(), 1),
                ("hs_huoling_1", "焚天心诀", "sect_huoling", BuildHuolingLayers(), 1),
                ("hs_wandu_1", "万毒心经", "sect_wandu", BuildWanduLayers(), 1),
                ("hs_taixu_1", "太虚心法", "sect_taixu", BuildTaixuLayers(), 1),
                ("hs_longmen_1", "龙脉心经", "sect_longmen", BuildLongmenLayers(), 1),
                ("hs_guiyuan_1", "归元心法", "sect_guiyuan", BuildGuiyuanLayers(), 1),
                ("hs_mozong_1", "天魔心经", "sect_mozong", BuildMozongLayers(), 1)
            };

            foreach (var t in templates)
            {
                var sql = """
                    INSERT OR REPLACE INTO heart_sutra_templates (SutraId, Name, Description, SectId, MaxLayer, LayersJson, SortOrder, LastUpdateTime)
                    VALUES (@SutraId, @Name, NULL, @SectId, 10, @LayersJson, @SortOrder, @now);
                    """;
                db.Ado.ExecuteCommand(sql, new { t.SutraId, t.Name, t.SectId, LayersJson = t.LayersJson, t.SortOrder, now });
            }
        }

        private static void SeedSectShopConfig(ISqlSugarClient db)
        {
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var sql = """
                INSERT OR IGNORE INTO sect_shop_configs (ShopId, ShopName, Description, IsOpen, LastUpdateTime)
                VALUES ('sect_shop_default', '宗门宝库', NULL, 1, @now);
                """;
            db.Ado.ExecuteCommand(sql, new { now });
        }

        private static void SeedSectTournamentSchedule(ISqlSugarClient db)
        {
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var sql = """
                INSERT OR IGNORE INTO sect_tournament_schedules (ScheduleId, SpawnTimeText, TimeZoneId, IsEnabled, DurationMinutes, LastUpdateTime)
                VALUES ('default', '20:00', 'China Standard Time', 1, 60, @now);
                """;
            db.Ado.ExecuteCommand(sql, new { now });
        }

        private static void SeedSectBlessingConfigs(ISqlSugarClient db)
        {
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var blessings = new[]
            {
                ("bless_hp_5",     "气血加持", 1, "bless_hp_5_buff",     1),
                ("bless_atk_5",    "攻击加持", 2, "bless_atk_5_buff",    2),
                ("bless_def_5",    "防御加持", 3, "bless_def_5_buff",    3),
                ("bless_crit_3",   "暴击加持", 4, "bless_crit_3_buff",   4),
                ("bless_speed_5",  "速度加持", 5, "bless_speed_5_buff",  5),
                ("bless_all_3",    "全属性加持", 6, "bless_all_3_buff",    6)
            };

            foreach (var (id, name, level, buffId, sort) in blessings)
            {
                var sql = """
                    INSERT OR IGNORE INTO sect_blessing_configs (BlessingId, Name, Description, RequiredGuildLevel, BuffId, SortOrder, LastUpdateTime)
                    VALUES (@id, @name, NULL, @level, @buffId, @sort, @now);
                    """;
                db.Ado.ExecuteCommand(sql, new { id, name, level, buffId, sort, now });
            }
        }

        private static void SeedSectQuestConfigs(ISqlSugarClient db)
        {
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var quests = new (string QuestId, string QuestName, int ResetCycle, string Description, int RequiredLevel, long RewardExp, long RewardGold, int ContributionReward, int SortOrder, string ObjectivesJson)[]
            {
                ("sect_daily_donate",   "每日捐献",     1, "为宗门捐献金币，贡献一份力量",   1,  100,  0,     100, 1, """[{"Type":"Donate","TargetId":"","Count":1,"Value":0}]"""),
                ("sect_daily_battle",   "宗门历练",     1, "在外征战，扬宗门之威",           1,  200,  5000,  100, 2, """[{"Type":"WinBattle","TargetId":"","Count":5,"Value":0}]"""),
                ("sect_daily_alchemy",  "为宗炼丹",     1, "炼制丹药，供宗门使用",           1,  150,  0,     100, 3, """[{"Type":"AlchemySuccess","TargetId":"","Count":3,"Value":0}]"""),
                ("sect_weekly_dungeon", "宗门副本",     2, "副本征战，获取资源",             10, 500,  10000, 100, 4, """[{"Type":"CompleteDungeon","TargetId":"","Count":3,"Value":0}]"""),
                ("sect_weekly_forge",   "锻造贡献",     2, "为宗门锻造装备",                 10, 400,  0,     100, 5, """[{"Type":"ForgeSuccess","TargetId":"","Count":5,"Value":0}]"""),
                ("sect_weekly_boss",    "讨伐宗门Boss", 2, "讨伐宗门Boss，保卫宗门",         15, 800,  20000, 100, 6, """[{"Type":"SectBossDamage","TargetId":"","Count":1,"Value":10000}]""")
            };

            foreach (var q in quests)
            {
                var rewardItemsJson = $"{{\"contribution\":{q.ContributionReward}}}";
                var sql = """
                    INSERT OR REPLACE INTO quest_config (QuestId, SeedKey, IsBuiltIn, BuiltInVersion, QuestName, QuestType, ResetCycle, Description, RequiredLevel, PreQuestIds, AutoAccept, AutoSubmit, TimeLimit, RewardExp, RewardGold, RewardSpiritStone, RewardItemsJson, RewardEquipmentIds, ObjectivesJson, SortOrder, IsEnabled, LastUpdateTime)
                    VALUES (@QuestId, @SeedKey, 1, @BuiltInVersion, @QuestName, 5, @ResetCycle, @Description, @RequiredLevel, NULL, 1, 0, 0, @RewardExp, @RewardGold, 0, @RewardItemsJson, NULL, @ObjectivesJson, @SortOrder, 1, @now);
                    """;
                db.Ado.ExecuteCommand(sql, new
                {
                    q.QuestId,
                    SeedKey = $"built-in:sect-quest:{q.QuestId}",
                    BuiltInVersion = "sect-quest-v1-20260602",
                    q.QuestName,
                    q.ResetCycle,
                    q.Description,
                    q.RequiredLevel,
                    q.RewardExp,
                    q.RewardGold,
                    RewardItemsJson = rewardItemsJson,
                    q.ObjectivesJson,
                    q.SortOrder,
                    now
                });
            }
        }

        #region 心法层数据构建

        // ============================================================
        //  设计规范：
        //  - Type1-7 为绝对值（int），Type8-15 为比率（float 0-1）
        //  - 每层每种属性只出现一次
        //  - 主属性每层递增约 30%，副属性穿插在偶数层
        //  - L10 给出独特副属性作为终极奖励，避免与主属性重复
        //  - 贡献和金币成本逐层递增
        // ============================================================

        /// <summary>
        /// 天剑宗·御剑心经：剑道极致，一剑破万法。
        /// 主属性 Type3(物攻)，副属性 Type14(破甲率)/Type10(暴伤)
        /// </summary>
        private static string BuildTianjianLayers()
        {
            return """
                [
                  {"Layer":1,"Name":"剑意初成","ContributionCost":100,"GoldCost":10000,"Bonuses":[{"AttributeName":"Type3","Value":50,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":2,"Name":"剑气外放","ContributionCost":200,"GoldCost":20000,"Bonuses":[{"AttributeName":"Type3","Value":65,"IsPercentage":false},{"AttributeName":"Type14","Value":0.02,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":3,"Name":"剑心通明","ContributionCost":400,"GoldCost":40000,"Bonuses":[{"AttributeName":"Type3","Value":85,"IsPercentage":false},{"AttributeName":"Type10","Value":0.03,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":4,"Name":"万剑归宗","ContributionCost":800,"GoldCost":80000,"Bonuses":[{"AttributeName":"Type3","Value":110,"IsPercentage":false},{"AttributeName":"Type14","Value":0.04,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":5,"Name":"剑意凝形","ContributionCost":1200,"GoldCost":120000,"Bonuses":[{"AttributeName":"Type3","Value":145,"IsPercentage":false},{"AttributeName":"Type10","Value":0.05,"IsPercentage":false}],"UnlockSkillId":"29","UnlockBuffId":null},
                  {"Layer":6,"Name":"剑破虚空","ContributionCost":1800,"GoldCost":180000,"Bonuses":[{"AttributeName":"Type3","Value":190,"IsPercentage":false},{"AttributeName":"Type14","Value":0.06,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":7,"Name":"天剑合一","ContributionCost":2500,"GoldCost":250000,"Bonuses":[{"AttributeName":"Type3","Value":250,"IsPercentage":false},{"AttributeName":"Type10","Value":0.08,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":8,"Name":"剑道大成","ContributionCost":3500,"GoldCost":350000,"Bonuses":[{"AttributeName":"Type3","Value":325,"IsPercentage":false},{"AttributeName":"Type14","Value":0.08,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":9,"Name":"剑仙之境","ContributionCost":5000,"GoldCost":500000,"Bonuses":[{"AttributeName":"Type3","Value":420,"IsPercentage":false},{"AttributeName":"Type10","Value":0.10,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":10,"Name":"无上剑道","ContributionCost":8000,"GoldCost":800000,"Bonuses":[{"AttributeName":"Type3","Value":550,"IsPercentage":false},{"AttributeName":"Type11","Value":0.10,"IsPercentage":false}],"UnlockSkillId":"49","UnlockBuffId":null}
                ]
                """;
        }

        /// <summary>
        /// 玄冥教·寒冰真诀：阴寒之力，冰封万物。
        /// 主属性 Type4(法攻)，副属性 Type6(法防)/Type10(暴伤)
        /// </summary>
        private static string BuildXuanmingLayers()
        {
            return """
                [
                  {"Layer":1,"Name":"寒冰初悟","ContributionCost":100,"GoldCost":10000,"Bonuses":[{"AttributeName":"Type4","Value":50,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":2,"Name":"玄冰掌","ContributionCost":200,"GoldCost":20000,"Bonuses":[{"AttributeName":"Type4","Value":65,"IsPercentage":false},{"AttributeName":"Type6","Value":15,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":3,"Name":"冰封千里","ContributionCost":400,"GoldCost":40000,"Bonuses":[{"AttributeName":"Type4","Value":85,"IsPercentage":false},{"AttributeName":"Type10","Value":0.03,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":4,"Name":"寒冰真气","ContributionCost":800,"GoldCost":80000,"Bonuses":[{"AttributeName":"Type4","Value":110,"IsPercentage":false},{"AttributeName":"Type6","Value":30,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":5,"Name":"玄冥之体","ContributionCost":1200,"GoldCost":120000,"Bonuses":[{"AttributeName":"Type4","Value":145,"IsPercentage":false},{"AttributeName":"Type10","Value":0.05,"IsPercentage":false}],"UnlockSkillId":"17","UnlockBuffId":null},
                  {"Layer":6,"Name":"冰魂觉醒","ContributionCost":1800,"GoldCost":180000,"Bonuses":[{"AttributeName":"Type4","Value":190,"IsPercentage":false},{"AttributeName":"Type6","Value":50,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":7,"Name":"极寒领域","ContributionCost":2500,"GoldCost":250000,"Bonuses":[{"AttributeName":"Type4","Value":250,"IsPercentage":false},{"AttributeName":"Type10","Value":0.08,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":8,"Name":"玄冥化形","ContributionCost":3500,"GoldCost":350000,"Bonuses":[{"AttributeName":"Type4","Value":325,"IsPercentage":false},{"AttributeName":"Type6","Value":80,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":9,"Name":"北冥深渊","ContributionCost":5000,"GoldCost":500000,"Bonuses":[{"AttributeName":"Type4","Value":420,"IsPercentage":false},{"AttributeName":"Type10","Value":0.10,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":10,"Name":"玄冥大道","ContributionCost":8000,"GoldCost":800000,"Bonuses":[{"AttributeName":"Type4","Value":550,"IsPercentage":false},{"AttributeName":"Type8","Value":0.05,"IsPercentage":false}],"UnlockSkillId":"19","UnlockBuffId":null}
                ]
                """;
        }

        /// <summary>
        /// 青云门·青云心法：御风而行，身法飘逸。
        /// 主属性 Type7(速度)，副属性 Type9(闪避率)/Type12(连击率)
        /// </summary>
        private static string BuildQingyunLayers()
        {
            return """
                [
                  {"Layer":1,"Name":"清风步","ContributionCost":100,"GoldCost":10000,"Bonuses":[{"AttributeName":"Type7","Value":10,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":2,"Name":"御风术","ContributionCost":200,"GoldCost":20000,"Bonuses":[{"AttributeName":"Type7","Value":13,"IsPercentage":false},{"AttributeName":"Type9","Value":0.02,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":3,"Name":"风行诀","ContributionCost":400,"GoldCost":40000,"Bonuses":[{"AttributeName":"Type7","Value":17,"IsPercentage":false},{"AttributeName":"Type12","Value":0.02,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":4,"Name":"青云步法","ContributionCost":800,"GoldCost":80000,"Bonuses":[{"AttributeName":"Type7","Value":22,"IsPercentage":false},{"AttributeName":"Type9","Value":0.03,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":5,"Name":"疾风幻影","ContributionCost":1200,"GoldCost":120000,"Bonuses":[{"AttributeName":"Type7","Value":29,"IsPercentage":false},{"AttributeName":"Type12","Value":0.03,"IsPercentage":false}],"UnlockSkillId":"38","UnlockBuffId":null},
                  {"Layer":6,"Name":"风雷之体","ContributionCost":1800,"GoldCost":180000,"Bonuses":[{"AttributeName":"Type7","Value":38,"IsPercentage":false},{"AttributeName":"Type9","Value":0.04,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":7,"Name":"天风化龙","ContributionCost":2500,"GoldCost":250000,"Bonuses":[{"AttributeName":"Type7","Value":49,"IsPercentage":false},{"AttributeName":"Type12","Value":0.04,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":8,"Name":"青云直上","ContributionCost":3500,"GoldCost":350000,"Bonuses":[{"AttributeName":"Type7","Value":64,"IsPercentage":false},{"AttributeName":"Type9","Value":0.05,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":9,"Name":"御风九天","ContributionCost":5000,"GoldCost":500000,"Bonuses":[{"AttributeName":"Type7","Value":83,"IsPercentage":false},{"AttributeName":"Type12","Value":0.05,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":10,"Name":"青云大道","ContributionCost":8000,"GoldCost":800000,"Bonuses":[{"AttributeName":"Type7","Value":108,"IsPercentage":false},{"AttributeName":"Type9","Value":0.05,"IsPercentage":false}],"UnlockSkillId":"63","UnlockBuffId":null}
                ]
                """;
        }

        /// <summary>
        /// 天音寺·梵音心经：佛门正宗，攻守兼备。
        /// 主属性 Type1(气血)，副属性 Type5(物防)/Type6(法防)
        /// </summary>
        private static string BuildTianyinLayers()
        {
            return """
                [
                  {"Layer":1,"Name":"梵音初闻","ContributionCost":100,"GoldCost":10000,"Bonuses":[{"AttributeName":"Type1","Value":120,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":2,"Name":"金刚护体","ContributionCost":200,"GoldCost":20000,"Bonuses":[{"AttributeName":"Type1","Value":156,"IsPercentage":false},{"AttributeName":"Type5","Value":15,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":3,"Name":"般若心经","ContributionCost":400,"GoldCost":40000,"Bonuses":[{"AttributeName":"Type1","Value":203,"IsPercentage":false},{"AttributeName":"Type6","Value":15,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":4,"Name":"佛光普照","ContributionCost":800,"GoldCost":80000,"Bonuses":[{"AttributeName":"Type1","Value":264,"IsPercentage":false},{"AttributeName":"Type5","Value":25,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":5,"Name":"罗汉金身","ContributionCost":1200,"GoldCost":120000,"Bonuses":[{"AttributeName":"Type1","Value":343,"IsPercentage":false},{"AttributeName":"Type6","Value":25,"IsPercentage":false}],"UnlockSkillId":"9","UnlockBuffId":null},
                  {"Layer":6,"Name":"渡厄经文","ContributionCost":1800,"GoldCost":180000,"Bonuses":[{"AttributeName":"Type1","Value":446,"IsPercentage":false},{"AttributeName":"Type5","Value":40,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":7,"Name":"天龙禅唱","ContributionCost":2500,"GoldCost":250000,"Bonuses":[{"AttributeName":"Type1","Value":580,"IsPercentage":false},{"AttributeName":"Type6","Value":40,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":8,"Name":"不动明王","ContributionCost":3500,"GoldCost":350000,"Bonuses":[{"AttributeName":"Type1","Value":754,"IsPercentage":false},{"AttributeName":"Type5","Value":60,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":9,"Name":"地藏法身","ContributionCost":5000,"GoldCost":500000,"Bonuses":[{"AttributeName":"Type1","Value":980,"IsPercentage":false},{"AttributeName":"Type6","Value":60,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":10,"Name":"佛门大道","ContributionCost":8000,"GoldCost":800000,"Bonuses":[{"AttributeName":"Type1","Value":1274,"IsPercentage":false},{"AttributeName":"Type5","Value":80,"IsPercentage":false}],"UnlockSkillId":"14","UnlockBuffId":null}
                ]
                """;
        }

        /// <summary>
        /// 火灵宫·焚天心诀：烈焰焚天，暴力输出。
        /// 主属性 Type4(法攻)，副属性 Type10(暴伤)/Type15(额外伤害)
        /// </summary>
        private static string BuildHuolingLayers()
        {
            return """
                [
                  {"Layer":1,"Name":"引火入体","ContributionCost":100,"GoldCost":10000,"Bonuses":[{"AttributeName":"Type4","Value":50,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":2,"Name":"烈焰掌","ContributionCost":200,"GoldCost":20000,"Bonuses":[{"AttributeName":"Type4","Value":65,"IsPercentage":false},{"AttributeName":"Type10","Value":0.03,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":3,"Name":"焚天真火","ContributionCost":400,"GoldCost":40000,"Bonuses":[{"AttributeName":"Type4","Value":85,"IsPercentage":false},{"AttributeName":"Type15","Value":0.02,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":4,"Name":"火灵附体","ContributionCost":800,"GoldCost":80000,"Bonuses":[{"AttributeName":"Type4","Value":110,"IsPercentage":false},{"AttributeName":"Type10","Value":0.05,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":5,"Name":"九天烈焰","ContributionCost":1200,"GoldCost":120000,"Bonuses":[{"AttributeName":"Type4","Value":145,"IsPercentage":false},{"AttributeName":"Type15","Value":0.03,"IsPercentage":false}],"UnlockSkillId":"35","UnlockBuffId":null},
                  {"Layer":6,"Name":"焚天灭地","ContributionCost":1800,"GoldCost":180000,"Bonuses":[{"AttributeName":"Type4","Value":190,"IsPercentage":false},{"AttributeName":"Type10","Value":0.08,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":7,"Name":"火凤涅槃","ContributionCost":2500,"GoldCost":250000,"Bonuses":[{"AttributeName":"Type4","Value":250,"IsPercentage":false},{"AttributeName":"Type15","Value":0.05,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":8,"Name":"天火焚世","ContributionCost":3500,"GoldCost":350000,"Bonuses":[{"AttributeName":"Type4","Value":325,"IsPercentage":false},{"AttributeName":"Type10","Value":0.10,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":9,"Name":"火神降世","ContributionCost":5000,"GoldCost":500000,"Bonuses":[{"AttributeName":"Type4","Value":420,"IsPercentage":false},{"AttributeName":"Type15","Value":0.08,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":10,"Name":"焚天大道","ContributionCost":8000,"GoldCost":800000,"Bonuses":[{"AttributeName":"Type4","Value":550,"IsPercentage":false},{"AttributeName":"Type11","Value":0.10,"IsPercentage":false}],"UnlockSkillId":"41","UnlockBuffId":null}
                ]
                """;
        }

        /// <summary>
        /// 万毒门·万毒心经：百毒不侵，以毒破防。
        /// 主属性 Type14(破甲率)，副属性 Type10(暴伤)/Type13(反击率)
        /// </summary>
        private static string BuildWanduLayers()
        {
            return """
                [
                  {"Layer":1,"Name":"百毒初试","ContributionCost":100,"GoldCost":10000,"Bonuses":[{"AttributeName":"Type14","Value":0.03,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":2,"Name":"毒雾弥漫","ContributionCost":200,"GoldCost":20000,"Bonuses":[{"AttributeName":"Type14","Value":0.04,"IsPercentage":false},{"AttributeName":"Type10","Value":0.03,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":3,"Name":"蚀骨毒掌","ContributionCost":400,"GoldCost":40000,"Bonuses":[{"AttributeName":"Type14","Value":0.05,"IsPercentage":false},{"AttributeName":"Type13","Value":0.02,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":4,"Name":"万毒噬心","ContributionCost":800,"GoldCost":80000,"Bonuses":[{"AttributeName":"Type14","Value":0.07,"IsPercentage":false},{"AttributeName":"Type10","Value":0.05,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":5,"Name":"毒龙之体","ContributionCost":1200,"GoldCost":120000,"Bonuses":[{"AttributeName":"Type14","Value":0.09,"IsPercentage":false},{"AttributeName":"Type13","Value":0.03,"IsPercentage":false}],"UnlockSkillId":"13","UnlockBuffId":null},
                  {"Layer":6,"Name":"腐毒领域","ContributionCost":1800,"GoldCost":180000,"Bonuses":[{"AttributeName":"Type14","Value":0.12,"IsPercentage":false},{"AttributeName":"Type10","Value":0.08,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":7,"Name":"毒蛊化形","ContributionCost":2500,"GoldCost":250000,"Bonuses":[{"AttributeName":"Type14","Value":0.15,"IsPercentage":false},{"AttributeName":"Type13","Value":0.04,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":8,"Name":"毒霸天下","ContributionCost":3500,"GoldCost":350000,"Bonuses":[{"AttributeName":"Type14","Value":0.19,"IsPercentage":false},{"AttributeName":"Type10","Value":0.10,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":9,"Name":"万毒归宗","ContributionCost":5000,"GoldCost":500000,"Bonuses":[{"AttributeName":"Type14","Value":0.25,"IsPercentage":false},{"AttributeName":"Type13","Value":0.06,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":10,"Name":"毒道至尊","ContributionCost":8000,"GoldCost":800000,"Bonuses":[{"AttributeName":"Type14","Value":0.32,"IsPercentage":false},{"AttributeName":"Type11","Value":0.10,"IsPercentage":false}],"UnlockSkillId":"42","UnlockBuffId":null}
                ]
                """;
        }

        /// <summary>
        /// 太虚殿·太虚心法：精神力修炼，洞察天机。
        /// 主属性 Type2(法力)/Type8(命中率)，副属性 Type4(法攻)
        /// </summary>
        private static string BuildTaixuLayers()
        {
            return """
                [
                  {"Layer":1,"Name":"虚无初感","ContributionCost":100,"GoldCost":10000,"Bonuses":[{"AttributeName":"Type2","Value":80,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":2,"Name":"灵识扩展","ContributionCost":200,"GoldCost":20000,"Bonuses":[{"AttributeName":"Type2","Value":104,"IsPercentage":false},{"AttributeName":"Type8","Value":0.02,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":3,"Name":"天眼通","ContributionCost":400,"GoldCost":40000,"Bonuses":[{"AttributeName":"Type2","Value":135,"IsPercentage":false},{"AttributeName":"Type4","Value":20,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":4,"Name":"太虚真气","ContributionCost":800,"GoldCost":80000,"Bonuses":[{"AttributeName":"Type2","Value":176,"IsPercentage":false},{"AttributeName":"Type8","Value":0.03,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":5,"Name":"精神领域","ContributionCost":1200,"GoldCost":120000,"Bonuses":[{"AttributeName":"Type2","Value":229,"IsPercentage":false},{"AttributeName":"Type4","Value":40,"IsPercentage":false}],"UnlockSkillId":"11","UnlockBuffId":null},
                  {"Layer":6,"Name":"天机洞察","ContributionCost":1800,"GoldCost":180000,"Bonuses":[{"AttributeName":"Type2","Value":298,"IsPercentage":false},{"AttributeName":"Type8","Value":0.05,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":7,"Name":"虚空凝神","ContributionCost":2500,"GoldCost":250000,"Bonuses":[{"AttributeName":"Type2","Value":387,"IsPercentage":false},{"AttributeName":"Type4","Value":65,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":8,"Name":"太虚化境","ContributionCost":3500,"GoldCost":350000,"Bonuses":[{"AttributeName":"Type2","Value":503,"IsPercentage":false},{"AttributeName":"Type8","Value":0.08,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":9,"Name":"窥天之道","ContributionCost":5000,"GoldCost":500000,"Bonuses":[{"AttributeName":"Type2","Value":654,"IsPercentage":false},{"AttributeName":"Type4","Value":100,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":10,"Name":"太虚大道","ContributionCost":8000,"GoldCost":800000,"Bonuses":[{"AttributeName":"Type2","Value":850,"IsPercentage":false},{"AttributeName":"Type8","Value":0.05,"IsPercentage":false}],"UnlockSkillId":"50","UnlockBuffId":null}
                ]
                """;
        }

        /// <summary>
        /// 龙门·龙脉心经：龙血传承，肉身强横。
        /// 主属性 Type1(气血)，副属性 Type5(物防)/Type13(反击率)
        /// </summary>
        private static string BuildLongmenLayers()
        {
            return """
                [
                  {"Layer":1,"Name":"龙血觉醒","ContributionCost":100,"GoldCost":10000,"Bonuses":[{"AttributeName":"Type1","Value":120,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":2,"Name":"龙鳞护体","ContributionCost":200,"GoldCost":20000,"Bonuses":[{"AttributeName":"Type1","Value":156,"IsPercentage":false},{"AttributeName":"Type5","Value":15,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":3,"Name":"龙脉贯体","ContributionCost":400,"GoldCost":40000,"Bonuses":[{"AttributeName":"Type1","Value":203,"IsPercentage":false},{"AttributeName":"Type13","Value":0.02,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":4,"Name":"龙啸九天","ContributionCost":800,"GoldCost":80000,"Bonuses":[{"AttributeName":"Type1","Value":264,"IsPercentage":false},{"AttributeName":"Type5","Value":25,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":5,"Name":"龙骨锻体","ContributionCost":1200,"GoldCost":120000,"Bonuses":[{"AttributeName":"Type1","Value":343,"IsPercentage":false},{"AttributeName":"Type13","Value":0.03,"IsPercentage":false}],"UnlockSkillId":"8","UnlockBuffId":null},
                  {"Layer":6,"Name":"龙威压世","ContributionCost":1800,"GoldCost":180000,"Bonuses":[{"AttributeName":"Type1","Value":446,"IsPercentage":false},{"AttributeName":"Type5","Value":40,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":7,"Name":"真龙之血","ContributionCost":2500,"GoldCost":250000,"Bonuses":[{"AttributeName":"Type1","Value":580,"IsPercentage":false},{"AttributeName":"Type13","Value":0.04,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":8,"Name":"龙神降世","ContributionCost":3500,"GoldCost":350000,"Bonuses":[{"AttributeName":"Type1","Value":754,"IsPercentage":false},{"AttributeName":"Type5","Value":60,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":9,"Name":"龙帝之躯","ContributionCost":5000,"GoldCost":500000,"Bonuses":[{"AttributeName":"Type1","Value":980,"IsPercentage":false},{"AttributeName":"Type13","Value":0.05,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":10,"Name":"真龙大道","ContributionCost":8000,"GoldCost":800000,"Bonuses":[{"AttributeName":"Type1","Value":1274,"IsPercentage":false},{"AttributeName":"Type13","Value":0.05,"IsPercentage":false}],"UnlockSkillId":"65","UnlockBuffId":null}
                ]
                """;
        }

        /// <summary>
        /// 归元宗·归元心法：万法归元，均衡发展。
        /// 全属性均衡，每层覆盖多种基础属性，偶数层穿插高级属性。
        /// </summary>
        private static string BuildGuiyuanLayers()
        {
            return """
                [
                  {"Layer":1,"Name":"归元初悟","ContributionCost":100,"GoldCost":10000,"Bonuses":[{"AttributeName":"Type1","Value":60,"IsPercentage":false},{"AttributeName":"Type3","Value":12,"IsPercentage":false},{"AttributeName":"Type5","Value":6,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":2,"Name":"五行调和","ContributionCost":200,"GoldCost":20000,"Bonuses":[{"AttributeName":"Type1","Value":78,"IsPercentage":false},{"AttributeName":"Type3","Value":16,"IsPercentage":false},{"AttributeName":"Type4","Value":14,"IsPercentage":false},{"AttributeName":"Type5","Value":8,"IsPercentage":false},{"AttributeName":"Type6","Value":7,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":3,"Name":"阴阳调和","ContributionCost":400,"GoldCost":40000,"Bonuses":[{"AttributeName":"Type1","Value":101,"IsPercentage":false},{"AttributeName":"Type3","Value":21,"IsPercentage":false},{"AttributeName":"Type4","Value":18,"IsPercentage":false},{"AttributeName":"Type5","Value":10,"IsPercentage":false},{"AttributeName":"Type6","Value":9,"IsPercentage":false},{"AttributeName":"Type7","Value":3,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":4,"Name":"天地归元","ContributionCost":800,"GoldCost":80000,"Bonuses":[{"AttributeName":"Type1","Value":131,"IsPercentage":false},{"AttributeName":"Type3","Value":27,"IsPercentage":false},{"AttributeName":"Type4","Value":23,"IsPercentage":false},{"AttributeName":"Type5","Value":13,"IsPercentage":false},{"AttributeName":"Type6","Value":12,"IsPercentage":false},{"AttributeName":"Type7","Value":4,"IsPercentage":false},{"AttributeName":"Type8","Value":0.02,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":5,"Name":"太极归真","ContributionCost":1200,"GoldCost":120000,"Bonuses":[{"AttributeName":"Type1","Value":170,"IsPercentage":false},{"AttributeName":"Type3","Value":35,"IsPercentage":false},{"AttributeName":"Type4","Value":30,"IsPercentage":false},{"AttributeName":"Type5","Value":17,"IsPercentage":false},{"AttributeName":"Type6","Value":16,"IsPercentage":false},{"AttributeName":"Type7","Value":5,"IsPercentage":false},{"AttributeName":"Type9","Value":0.02,"IsPercentage":false}],"UnlockSkillId":"10","UnlockBuffId":null},
                  {"Layer":6,"Name":"万法归一","ContributionCost":1800,"GoldCost":180000,"Bonuses":[{"AttributeName":"Type1","Value":221,"IsPercentage":false},{"AttributeName":"Type3","Value":46,"IsPercentage":false},{"AttributeName":"Type4","Value":39,"IsPercentage":false},{"AttributeName":"Type5","Value":22,"IsPercentage":false},{"AttributeName":"Type6","Value":21,"IsPercentage":false},{"AttributeName":"Type7","Value":7,"IsPercentage":false},{"AttributeName":"Type10","Value":0.03,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":7,"Name":"大道无形","ContributionCost":2500,"GoldCost":250000,"Bonuses":[{"AttributeName":"Type1","Value":287,"IsPercentage":false},{"AttributeName":"Type3","Value":60,"IsPercentage":false},{"AttributeName":"Type4","Value":51,"IsPercentage":false},{"AttributeName":"Type5","Value":29,"IsPercentage":false},{"AttributeName":"Type6","Value":27,"IsPercentage":false},{"AttributeName":"Type7","Value":9,"IsPercentage":false},{"AttributeName":"Type11","Value":0.02,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":8,"Name":"天人合一","ContributionCost":3500,"GoldCost":350000,"Bonuses":[{"AttributeName":"Type1","Value":373,"IsPercentage":false},{"AttributeName":"Type3","Value":78,"IsPercentage":false},{"AttributeName":"Type4","Value":66,"IsPercentage":false},{"AttributeName":"Type5","Value":37,"IsPercentage":false},{"AttributeName":"Type6","Value":35,"IsPercentage":false},{"AttributeName":"Type7","Value":12,"IsPercentage":false},{"AttributeName":"Type14","Value":0.02,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":9,"Name":"归元圣体","ContributionCost":5000,"GoldCost":500000,"Bonuses":[{"AttributeName":"Type1","Value":485,"IsPercentage":false},{"AttributeName":"Type3","Value":101,"IsPercentage":false},{"AttributeName":"Type4","Value":86,"IsPercentage":false},{"AttributeName":"Type5","Value":48,"IsPercentage":false},{"AttributeName":"Type6","Value":46,"IsPercentage":false},{"AttributeName":"Type7","Value":16,"IsPercentage":false},{"AttributeName":"Type10","Value":0.05,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":10,"Name":"万法归元","ContributionCost":8000,"GoldCost":800000,"Bonuses":[{"AttributeName":"Type1","Value":630,"IsPercentage":false},{"AttributeName":"Type3","Value":131,"IsPercentage":false},{"AttributeName":"Type4","Value":112,"IsPercentage":false},{"AttributeName":"Type5","Value":63,"IsPercentage":false},{"AttributeName":"Type6","Value":60,"IsPercentage":false},{"AttributeName":"Type7","Value":21,"IsPercentage":false},{"AttributeName":"Type11","Value":0.05,"IsPercentage":false}],"UnlockSkillId":"52","UnlockBuffId":null}
                ]
                """;
        }

        /// <summary>
        /// 魔宗·天魔心经：以魔入道，暴击流极致。
        /// 主属性 Type9(暴击率)/Type10(暴伤)，副属性 Type15(额外伤害)
        /// </summary>
        private static string BuildMozongLayers()
        {
            return """
                [
                  {"Layer":1,"Name":"入魔初感","ContributionCost":100,"GoldCost":10000,"Bonuses":[{"AttributeName":"Type9","Value":0.02,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":2,"Name":"魔气入体","ContributionCost":200,"GoldCost":20000,"Bonuses":[{"AttributeName":"Type10","Value":0.03,"IsPercentage":false},{"AttributeName":"Type15","Value":0.01,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":3,"Name":"天魔心法","ContributionCost":400,"GoldCost":40000,"Bonuses":[{"AttributeName":"Type9","Value":0.03,"IsPercentage":false},{"AttributeName":"Type10","Value":0.05,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":4,"Name":"魔焰滔天","ContributionCost":800,"GoldCost":80000,"Bonuses":[{"AttributeName":"Type15","Value":0.02,"IsPercentage":false},{"AttributeName":"Type9","Value":0.04,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":5,"Name":"万魔朝宗","ContributionCost":1200,"GoldCost":120000,"Bonuses":[{"AttributeName":"Type10","Value":0.08,"IsPercentage":false},{"AttributeName":"Type9","Value":0.05,"IsPercentage":false}],"UnlockSkillId":"32","UnlockBuffId":null},
                  {"Layer":6,"Name":"天魔化形","ContributionCost":1800,"GoldCost":180000,"Bonuses":[{"AttributeName":"Type15","Value":0.03,"IsPercentage":false},{"AttributeName":"Type10","Value":0.10,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":7,"Name":"魔帝降临","ContributionCost":2500,"GoldCost":250000,"Bonuses":[{"AttributeName":"Type9","Value":0.08,"IsPercentage":false},{"AttributeName":"Type10","Value":0.13,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":8,"Name":"灭世魔功","ContributionCost":3500,"GoldCost":350000,"Bonuses":[{"AttributeName":"Type15","Value":0.05,"IsPercentage":false},{"AttributeName":"Type9","Value":0.10,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":9,"Name":"天魔真身","ContributionCost":5000,"GoldCost":500000,"Bonuses":[{"AttributeName":"Type10","Value":0.18,"IsPercentage":false},{"AttributeName":"Type9","Value":0.12,"IsPercentage":false}],"UnlockSkillId":null,"UnlockBuffId":null},
                  {"Layer":10,"Name":"魔道至尊","ContributionCost":8000,"GoldCost":800000,"Bonuses":[{"AttributeName":"Type9","Value":0.15,"IsPercentage":false},{"AttributeName":"Type10","Value":0.22,"IsPercentage":false}],"UnlockSkillId":"83","UnlockBuffId":null}
                ]
                """;
        }

        #endregion
    }
}
