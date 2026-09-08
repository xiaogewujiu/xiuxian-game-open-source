using XXX.Entity;

namespace XXX.Battle
{
    /// <summary>
    /// 战斗奖励
    /// 负责战斗奖励的计算
    /// </summary>
    public static class BattleRewards
    {/// <summary>
     /// 随机数生成器（线程安全）
     /// </summary>
        private static Random Rand = Random.Shared;
        /// <summary>
        /// 计算战斗奖励
        /// </summary>
        public static void CalculateRewards(BattleContext context)
        {
            if (context == null)
                return;

            var enemyside = context.EnemySide;
            if (enemyside == null)
                return;

            foreach (var item in enemyside)
            {
                if (item == null || string.IsNullOrEmpty(item.MonsterTempID))
                    continue;

                if (!GameData.MonsterTemplates.ContainsKey(item.MonsterTempID))
                    continue;

                var temp = GameData.MonsterTemplates[item.MonsterTempID];
                if (temp == null)
                    continue;

                // 经验奖励
                if (temp.ExpReward.max >= temp.ExpReward.min)
                {
                    context.Result.ExpGained += Rand.Next(temp.ExpReward.min, temp.ExpReward.max + 1);
                }

                // 金币奖励
                if (temp.GoldReward.max >= temp.GoldReward.min)
                {
                    context.Result.GoldGained += Rand.Next(temp.GoldReward.min, temp.GoldReward.max + 1);
                }

                // 道具、装备、图鉴合并为掉落池，加权随机抽取 1 件
                // DropType: 0=道具, 1=装备, 2=图鉴
                // Rate 为万分比整数（10000 = 100%）
                var dropPool = new List<(string Id, int Rate, int DropType, int CollectionType)>();

                if (temp.ItemDrops != null)
                {
                    foreach (var drop in temp.ItemDrops)
                    {
                        if (drop.Rate > 0 && GameData.Items.ContainsKey(drop.ItemId))
                        {
                            dropPool.Add((drop.ItemId, drop.Rate, 0, 0));
                        }
                    }
                }

                if (temp.EquipmentDrops != null)
                {
                    foreach (var drop in temp.EquipmentDrops)
                    {
                        if (drop.Rate > 0 && int.TryParse(drop.EquipmentId, out int equipId)
                            && GameData.EquipmentTemplates.ContainsKey(equipId))
                        {
                            dropPool.Add((drop.EquipmentId, drop.Rate, 1, 0));
                        }
                    }
                }

                if (temp.CollectionDrops != null)
                {
                    foreach (var drop in temp.CollectionDrops)
                    {
                        if (drop.Rate > 0 && !string.IsNullOrWhiteSpace(drop.SeriesId))
                        {
                            dropPool.Add((drop.SeriesId, drop.Rate, 2, drop.CollectionType));
                        }
                    }
                }

                if (dropPool.Count > 0)
                {
                    var totalWeight = dropPool.Sum(e => e.Rate);
                    var threshold = Math.Min(totalWeight, 10000);

                    if (Rand.Next(0, 10000) < threshold)
                    {
                        var pick = Rand.Next(0, totalWeight);
                        var cumulative = 0;
                        foreach (var entry in dropPool)
                        {
                            cumulative += entry.Rate;
                            if (pick < cumulative)
                            {
                                switch (entry.DropType)
                                {
                                    case 1: // 装备
                                        if (int.TryParse(entry.Id, out int equipId))
                                        {
                                            var equip = CreateEquipmentFromTemplate(equipId);
                                            if (equip != null)
                                            {
                                                context.Result.DroppedEquipments.Add(equip);
                                            }
                                        }
                                        break;
                                    case 2: // 图鉴
                                        context.Result.DroppedCollections.Add((entry.Id, entry.CollectionType));
                                        break;
                                    default: // 道具
                                        context.Result.DroppedItems.Add(GameData.Items[entry.Id]);
                                        break;
                                }
                                break;
                            }
                        }
                    }
                }
            }

        }

        /// <summary>
        /// 根据已击杀的怪物模板独立计算一份奖励。
        /// 该方法只复用掉落配置和随机规则，不复用其他成员已经抽出的结果。
        /// </summary>
        public static BattleResult CalculateRewardsForMonsterTemplates(IEnumerable<string> monsterTemplateIds)
        {
            var context = new BattleContext
            {
                EnemySide = (monsterTemplateIds ?? [])
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Select(id => new BattleFighter { MonsterTempID = id.Trim() })
                    .ToList()
            };
            CalculateRewards(context);
            return context.Result;
        }
        /// <summary>
        /// 从模版生成装备实例。
        /// </summary>
        public static EquipmentInstance? CreateEquipmentFromTemplate(int templateId)
        {
            if (!GameData.EquipmentTemplates.ContainsKey(templateId))
                return null;

            var template = GameData.EquipmentTemplates[templateId];
            if (template == null)
                return null;

            var equip = new EquipmentInstance
            {
                Template = template,
                // 随机生成属性
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
                Type15 = RandomRange(template.MinType15, template.MaxType15) / 100f
            };

            return equip;
        }

        private static int RandomRange(int? min, int? max)
        {
            int minVal = min ?? 0;
            int maxVal = max ?? minVal;
            if (maxVal < minVal) maxVal = minVal;
            return Rand.Next(minVal, maxVal + 1);
        }
    }
}
