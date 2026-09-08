//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text.Encodings.Web;
//using System.Text.Json;
//using System.Text.Unicode;
//using XXX.Battle;
//using XXX.Entity;
//using XXX.RuiShi;

//namespace XXX
//{
//    /// <summary>
//    /// 主程序入口
//    /// 演示回合制战斗系统
//    /// </summary>
//    class Program
//    {
//        private static JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
//        {
//            WriteIndented = true,
//            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
//        };
//        static void Main(string[] args)
//        {
//            GameData.Initialize();


//            Console.OutputEncoding = System.Text.Encoding.UTF8;
//            Console.WriteLine("=== 回合制战斗系统演示 ===\n");

//            // 初始化所有游戏数据


//            var player1 = JsonSerializer.Deserialize<UserEntity>(@"{
//	""GID"": ""1"",
//	""Name"": ""小哥哥"",
//	""Level"": ""10"",
//	""Exp"": 0,
//	""XExp"": 1000,
//	""Exp2"": 0,
//	""XExp2"": 0,
//	""PetId"": null,
//	""Type1"":80000 ,
//	""Type2"": 800,
//	""Type3"": 1500,
//	""Type4"":0,
//	""Type5"": 1200,
//	""Type6"": 0,
//	""Type7"": 100,
//	""Type8"": 1,
//	""Type9"": 0.1,
//	""Type10"": 0,
//	""Type11"":1.5,
//	""Type12"": 0,
//	""Type13"": 1,
//	""Type14"": 0,
//	""Type15"": 0
//}");
//            var player2 = JsonSerializer.Deserialize<UserEntity>(@"{
//	""GID"": ""2"",
//	""Name"": ""魔法师"",
//	""Level"": ""10"",
//	""Exp"": 0,
//	""XExp"": 1000,
//	""Exp2"": 0,
//	""XExp2"": 0,
//	""PetId"": null,
//	""Type1"": 80000,
//	""Type2"": 8000,
//	""Type3"": 1500,
//	""Type4"":0,
//	""Type5"": 1200,
//	""Type6"": 0,
//	""Type7"": 80,
//	""Type8"": 1,
//	""Type9"": 0.1,
//	""Type10"": 0,
//	""Type11"":1.5,
//	""Type12"": 1,
//	""Type13"": 0,
//	""Type14"": 0,
//	""Type15"": 0
//}");

//            //            var player2 = JsonSerializer.Deserialize<UserEntity>(@"{
//            //	""GID"": ""19eee16c-4a52-4f4a-873b-5f3e8aaf59c0"",
//            //	""Name"": ""小哥哥1"",
//            //	""Level"": ""10"",
//            //	""Exp"": 0,
//            //	""XExp"": 1000,
//            //	""Exp2"": 0,
//            //	""XExp2"": 0,
//            //	""PetId"": null,
//            //	""Type1"": 80000,
//            //	""Type2"": 80000,
//            //	""Type3"": 2814,
//            //	""Type4"": 2814,
//            //	""Type5"": 100,
//            //	""Type6"": 1063,
//            //	""Type7"": 100,
//            //	""Type8"": 1,
//            //	""Type9"": 0,
//            //	""Type10"": 0.5,
//            //	""Type11"":1.5,
//            //	""Type12"": 0,
//            //	""Type13"": 0,
//            //	""Type14"": 0,
//            //	""Type15"": 0
//            //}"); 
//            var player3 = JsonSerializer.Deserialize<UserEntity>(@"{
//	""GID"": ""19eee16c-4a52-4f4a-873b-5f3e8aaf59c0"",
//	""Name"": ""小哥哥3"",
//	""Level"": ""10"",
//	""Exp"": 0,
//	""XExp"": 1000,
//	""Exp2"": 0,
//	""XExp2"": 0,
//	""PetId"": null,
//	""Type1"": 80000,
//	""Type2"": 80000,
//	""Type3"": 2814,
//	""Type4"": 2814,
//	""Type5"": 100,
//	""Type6"": 1063,
//	""Type7"": 100,
//	""Type8"": 1,
//	""Type9"": 0,
//	""Type10"": 0.5,
//	""Type11"":1.5,
//	""Type12"": 0,
//	""Type13"": 0,
//	""Type14"": 0,
//	""Type15"": 0
//}");
//            var monster1 = JsonSerializer.Deserialize<MonsterEntity>(@"{
//	""GID"": ""3"",
//	""Name"": ""苦力怕"",
//	""Level"": ""20"",
//	""Type1"": 80000,
//	""Type2"": 80000,
//	""Type3"": 1000,
//	""Type4"": 0,
//	""Type5"": 800,
//	""Type6"": 1063,
//	""Type7"": 100,
//	""Type8"": 1,
//	""Type9"": 0,
//	""Type10"": 0.5,
//	""Type11"":1.5,
//	""Type12"": 0,
//	""Type13"": 1,
//	""Type14"": 0,
//	""Type15"": 0
//}");
//            var monster2 = JsonSerializer.Deserialize<MonsterEntity>(@"{
//	""GID"": ""728da318-0665-4020-9a90-ea519bce63f6"",
//	""Name"": ""苦力怕2"",
//	""Level"": ""20"",
//	""Type1"": 20000,
//	""Type2"": 80000,
//	""Type3"": 1000,
//	""Type4"": 0,
//	""Type5"": 20,
//	""Type6"": 0,
//	""Type7"": 100,
//	""Type8"": 1,
//	""Type9"": 0,
//	""Type10"": 0.1,
//	""Type11"":1.5,
//	""Type12"": 0,
//	""Type13"": 0,
//	""Type14"": 0,
//	""Type15"":0.5
//}");
//            var monster3 = JsonSerializer.Deserialize<MonsterEntity>(@"{
//	""GID"": ""728da318-0665-4020-9a90-ea519bce63f6"",
//	""Name"": ""苦力怕2"",
//	""Level"": ""20"",
//	""Type1"": 80000,
//	""Type2"": 80000,
//	""Type3"": 100,
//	""Type4"": 100,
//	""Type5"": 100,
//	""Type6"": 1063,
//	""Type7"": 100,
//	""Type8"": 1,
//	""Type9"": 0,
//	""Type10"": 0.5,
//	""Type11"":1.5,
//	""Type12"": 0,
//	""Type13"": 0,
//	""Type14"": 0,
//	""Type15"": 0
//}");
//            //monster1.SkillIds = new List<string> { "16" };
//            //player1.SkillIds = new List<string> { "1" };
//            var result1 =BattleSystem.StartBattle(new List<UserEntity> { player1, player2 },new List<MonsterEntity> { monster1 }); ;
//			var json=JsonSerializer.Serialize(result1, JsonSerializerOptions);
//            PrintBattleResult(new List<BattleResult> { result1 });
//            Console.ReadKey();
//            Console.Clear();

//            //// 演示1：单人无宠物战斗
//            //Console.WriteLine("【演示1】单人无宠物战斗");
//            //Console.WriteLine("------------------------");
//            //var player1 = GameData.CreateTestPlayer("玩家一", "10", false);
//            //var result2 = BattleSystem.StartBattleFuben(new List<UserEntity> { player2 }, "fuben_001");
//            //PrintBattleResult(result2);


//            //Console.WriteLine("\n按任意键继续下一场战斗...");
//            //Console.ReadKey();
//            //Console.Clear();

//            //// 演示2：单人带宠物战斗
//            //Console.WriteLine("【演示2】单人带宠物战斗");
//            //Console.WriteLine("------------------------");

//            //var result2 = BattleSystem.StartBattle(new List<UserEntity> { player2 }, "map_002");
//            //PrintBattleResult(result2);

//            //Console.WriteLine("\n按任意键继续下一场战斗...");
//            //Console.ReadKey();
//            //Console.Clear();

//            //// 演示3：多人组队战斗
//            //Console.WriteLine("【演示3】多人组队战斗（2人带宠物）");
//            //Console.WriteLine("----------------------------------");
//            //var player3 = GameData.CreateTestPlayer("队长", "20", true);
//            //var player4 = GameData.CreateTestPlayer("队员", "18", true);
//            //var result3 = BattleSystem.StartBattle(new List<UserEntity> { player3, player4 }, "map_003");
//            //PrintBattleResult(result3);

//            //Console.WriteLine("\n按任意键继续下一场战斗...");
//            //Console.ReadKey();
//            //Console.Clear();

//            /// <summary>
//            /// 打印战斗结果
//            /// </summary>
//            static void PrintBattleResult(List<BattleResult> results)
//            {
//                foreach (var result in results)
//                {
//                    Console.WriteLine($"\n========== 战斗结果{result.MapName} ==========");
//                    Console.WriteLine($"战斗结果: {(result.IsVictory ? "胜利" : "失败")}");
//                    Console.WriteLine($"总回合数: {result.TotalRounds}");
//                    Console.WriteLine($"获得经验: {result.ExpGained}");
//                    Console.WriteLine($"获得金币: {result.GoldGained}");

//                    if (result.DroppedItems.Count > 0)
//                    {
//                        Console.WriteLine("\n掉落道具:");
//                        foreach (var item in result.DroppedItems)
//                        {
//                            Console.WriteLine($"  - {item.Name}");
//                        }
//                    }

//                    if (result.DroppedEquipments.Count > 0)
//                    {
//                        Console.WriteLine("\n掉落装备:");
//                        foreach (var equip in result.DroppedEquipments)
//                        {
//                            Console.WriteLine($"  - {equip.Template.Name} (品质{equip.Template.Quality})");
//                        }
//                    }

//                    Console.WriteLine("\n========== 战斗日志 ==========");
//                    foreach (var log in result.RoundLogs)
//                    {
//                        Console.WriteLine($"\n--- 第 {log.RoundNumber} 回合 ---");
//                        foreach (var action in log.Actions)
//                        {
//                            Console.WriteLine(action);
//                        }
//                        // 从最后一条日志条目获取所有角色状态快照
//                        var lastEntry = log.Entries.LastOrDefault();
//                        if (lastEntry?.FighterStates != null)
//                        {
//                            Console.WriteLine("\n当前角色状态:");
//                            foreach (var state in lastEntry.FighterStates.Values)
//                            {
//                                string side = state.IsPlayerSide ? "[我方]" : "[敌方]";
//                                Console.WriteLine($"  {side}{state.FighterName}: HP {state.CurrentHp}/{state.MaxHp}, MP {state.CurrentMp}/{state.MaxMp}, 物攻{state.PhysicalAttack}, 法攻{state.MagicAttack}");
//                            }
//                        }
//                    }
//                }
//            }


//        }
//    }
//}
