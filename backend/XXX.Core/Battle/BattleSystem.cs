using XXX.Entity;

using System.Linq;

namespace XXX.Battle
{
    /// <summary>
    /// 战斗系统核心类
    /// 处理回合制战斗的所有逻辑
    /// </summary>
    public static class BattleSystem
    {
        /// <summary>
        /// 随机数生成器（线程安全）
        /// </summary>
        private static Random Rand = Random.Shared;

        /// <summary>
        /// 最大回合数限制
        /// </summary>
        public const int MaxRounds = 50;

        /// <summary>
        /// 单回合连击触发最大次数
        /// </summary>
        public const int MaxComboPerRound = 3;

        /// <summary>
        /// 单回合反击触发最大次数
        /// </summary>
        public const int MaxCounterPerRound = 3;

        /// <summary>
        /// 开始战斗
        /// </summary>
        /// <param name="players">参战玩家列表</param>
        /// <param name="mapId">地图ID</param>
        /// <param name="enableElementAdvantage">是否启用属性克制</param>
        /// <returns>战斗结果</returns>
        public static BattleResult StartBattle(
            List<UserEntity> players,
            string mapId,
            bool enableElementAdvantage = true,
            IReadOnlyDictionary<string, PetEntity>? activePets = null)
        {
            // 获取地图信息
            if (!GameData.Maps.ContainsKey(mapId))
            {
                return new BattleResult { IsVictory = false };
            }
            var map = GameData.Maps[mapId];

            // 创建战斗上下文
            var context = new BattleContext
            {
                Map = map,
                EnableElementAdvantage = enableElementAdvantage
            };

            // 初始化我方战斗单位
            BattleInitializer.InitializePlayerSide(context, players, activePets);

            // 根据地图配置生成野怪
            BattleInitializer.GenerateMonsters(context, map);

            // 执行战斗循环
            BattleExecutor.ExecuteBattle(context);

            // 生成战斗结果
            return BattleRecorder.GenerateBattleResult(context);
        }

        /// <summary>
        /// 开始战斗
        /// </summary>
        /// <param name="players">参战玩家列表</param>
        /// <param name="mapId">地图ID</param>
        /// <param name="enableElementAdvantage">是否启用属性克制</param>
        /// <returns>战斗结果</returns>
        public static List<BattleResult> StartBattleFuben(
            List<UserEntity> players,
            string mapId,
            bool enableElementAdvantage = true,
            IReadOnlyDictionary<string, PetEntity>? activePets = null)
        {
            List<BattleResult> results = [];
            // 获取地图信息
            if (!GameData.Maps.ContainsKey(mapId))
            {
                return [];
            }
            var map = GameData.Maps[mapId];



            // 创建战斗上下文
            var context = new BattleContext
            {
                Map = map,
                EnableElementAdvantage = enableElementAdvantage
            };
            // 初始化我方战斗单位
            BattleInitializer.InitializePlayerSide(context, players, activePets);
            // 根据地图配置生成野怪
            BattleInitializer.GenerateMonsters(context, map);
            // 执行战斗循环
            BattleExecutor.ExecuteBattle(context);

            // 关键逻辑：每层战斗都要冻结为独立快照，避免后续复用 context.Result 导致前层结果被覆盖。
            var firstStageResult = CloneBattleResult(BattleRecorder.GenerateBattleResult(context));
            results.Add(firstStageResult);

            // 关键逻辑：副本任意一层失败即结束，不再进入后续层。
            if (!firstStageResult.IsVictory)
            {
                return results;
            }

            // 关键逻辑：副本链按 NextMapId 串联，必须做安全读取，避免配置错误导致 KeyNotFound 异常。
            while (!string.IsNullOrEmpty(map.NextMapId))
            {
                var nextMapId = map.NextMapId;
                if (!GameData.Maps.TryGetValue(nextMapId, out var nextMap))
                {
                    results.Add(BuildInvalidNextMapResult(map.MapGId, nextMapId));
                    break;
                }

                map = nextMap;
                context.Map = map;
                context.EnableElementAdvantage = enableElementAdvantage;
                context.EnemySide.Clear();
                context.Result = new BattleResult();
                // 关键逻辑：副本每层重置结果时，同时重置该层日志时间戳计数器。
                // 确保每层内部日志时序独立，避免跨层延续造成阅读歧义。
                context.LogTimestampCounter = 0;
                // 根据地图配置生成野怪
                BattleInitializer.GenerateMonsters(context, map);
                if (!map.Carrying)
                {
                    context.PlayerSide.Clear();
                    BattleInitializer.InitializePlayerSide(context, players, activePets);
                }
                BattleExecutor.ExecuteBattle(context);
                // 关键逻辑：副本链每一层都写入独立拷贝，禁止直接存引用。
                var stageResult = CloneBattleResult(BattleRecorder.GenerateBattleResult(context));
                results.Add(stageResult);

                // 关键逻辑：任意层失败后立即中断副本链，避免继续推进后续层。
                if (!stageResult.IsVictory)
                {
                    break;
                }
            }
            return results;
        }

        /// <summary>
        /// 构建副本链配置错误的战斗结果
        /// 用于在 NextMapId 无效时返回可观测信息，而不是直接抛异常中断请求
        /// </summary>
        private static BattleResult BuildInvalidNextMapResult(string currentMapId, string invalidNextMapId)
        {
            var result = new BattleResult
            {
                IsVictory = false,
                TotalRounds = 0,
                MapName = currentMapId
            };

            var roundLog = new RoundLog
            {
                RoundNumber = 0,
                Entries =
                [
                    new BattleLogEntry
                    {
                        Description = $"Invalid NextMapId '{invalidNextMapId}' from map '{currentMapId}'."
                    }
                ]
            };

            result.RoundLogs.Add(roundLog);
            return result;
        }

        /// <summary>
        /// 克隆战斗结果快照
        /// 1. 保证副本多层结果之间相互独立，避免后续层修改影响前层
        /// 2. 避免使用 JSON 深拷贝引入的性能与空值风险
        /// </summary>
        private static BattleResult CloneBattleResult(BattleResult source)
        {
            if (source == null)
            {
                return new BattleResult { IsVictory = false };
            }

            var clone = new BattleResult
            {
                MapName = source.MapName,
                IsVictory = source.IsVictory,
                TotalRounds = source.TotalRounds,
                ExpGained = source.ExpGained,
                GoldGained = source.GoldGained,
                DroppedItems = source.DroppedItems?.ToList() ?? [],
                DroppedEquipments = source.DroppedEquipments?.ToList() ?? [],
                DroppedCollections = source.DroppedCollections?.ToList() ?? [],
                // 关键逻辑：逐条复制回合日志，避免同一日志对象被多层共享引用。
                RoundLogs = source.RoundLogs?
                    .Select(CloneRoundLog)
                    .ToList() ?? [],

                // 关键逻辑：统计数据做字典级复制，保障每层统计可独立读取。
                FighterSkillStats = source.FighterSkillStats?
                    .ToDictionary(
                        kv => kv.Key,
                        kv => CloneFighterSkillStats(kv.Value))
                    ?? []
            };

            return clone;
        }

        /// <summary>
        /// 克隆单回合日志
        /// </summary>
        private static RoundLog CloneRoundLog(RoundLog source)
        {
            if (source == null)
            {
                return new RoundLog();
            }

            return new RoundLog
            {
                RoundNumber = source.RoundNumber,
                Entries = source.Entries?.Select(CloneBattleLogEntry).ToList() ?? [],
                FighterStates = source.FighterStates?.Select(CloneFighterStateSnapshot).ToList() ?? []
            };
        }

        /// <summary>
        /// 克隆结构化日志条目
        /// </summary>
        private static BattleLogEntry CloneBattleLogEntry(BattleLogEntry source)
        {
            if (source == null)
            {
                return new BattleLogEntry();
            }

            return new BattleLogEntry
            {
                Type = source.Type,
                Timestamp = source.Timestamp,
                CasterId = source.CasterId,
                CasterName = source.CasterName,
                TargetId = source.TargetId,
                TargetName = source.TargetName,
                SkillId = source.SkillId,
                SkillName = source.SkillName,
                BuffId = source.BuffId,
                BuffName = source.BuffName,
                Value = source.Value,
                Value2 = source.Value2,
                TargetCurrentHp = source.TargetCurrentHp,
                TargetMaxHp = source.TargetMaxHp,
                Description = source.Description,
                ExtraData = source.ExtraData,
                IsCrit = source.IsCrit,
                IsDodge = source.IsDodge,
                HitIndex = source.HitIndex,
                TotalHits = source.TotalHits,
                FighterStates = source.FighterStates?.ToDictionary(
                    kv => kv.Key,
                    kv => CloneFighterStateSnapshot(kv.Value))
                    ?? []
            };
        }

        /// <summary>
        /// 克隆战斗单位状态快照
        /// </summary>
        private static FighterStateSnapshot CloneFighterStateSnapshot(FighterStateSnapshot source)
        {
            if (source == null)
            {
                return new FighterStateSnapshot();
            }

            return new FighterStateSnapshot
            {
                FighterId = source.FighterId,
                FighterName = source.FighterName,
                IsPlayerSide = source.IsPlayerSide,
                CurrentHp = source.CurrentHp,
                MaxHp = source.MaxHp,
                CurrentMp = source.CurrentMp,
                MaxMp = source.MaxMp,
                PhysicalAttack = source.PhysicalAttack,
                MagicAttack = source.MagicAttack,
                PhysicalDefense = source.PhysicalDefense,
                MagicDefense = source.MagicDefense,
                Speed = source.Speed,
                HitRate = source.HitRate,
                DodgeRate = source.DodgeRate,
                CritRate = source.CritRate,
                CritDamage = source.CritDamage,
                ComboRate = source.ComboRate,
                CounterRate = source.CounterRate,
                ArmorBreak = source.ArmorBreak,
                ExtraDamage = source.ExtraDamage,
                Buffs = source.Buffs?.Select(buff => new ActiveBuffSnapshot
                {
                    BuffId = buff.BuffId,
                    BuffName = buff.BuffName,
                    RemainingDuration = buff.RemainingDuration,
                    Stack = buff.Stack
                }).ToList() ?? []
            };
        }

        /// <summary>
        /// 克隆战斗统计数据
        /// </summary>
        private static FighterSkillStats CloneFighterSkillStats(FighterSkillStats source)
        {
            if (source == null)
            {
                return new FighterSkillStats();
            }

            return new FighterSkillStats
            {
                GID = source.GID,
                Name = source.Name,
                SkillUsageCount = source.SkillUsageCount?.ToDictionary(kv => kv.Key, kv => kv.Value) ?? [],
                TotalDamageDealt = source.TotalDamageDealt,
                TotalDamageTaken = source.TotalDamageTaken,
                TotalHealingDone = source.TotalHealingDone,
                TotalHealingReceived = source.TotalHealingReceived,
                TotalLifesteal = source.TotalLifesteal,
                TotalShieldAbsorbed = source.TotalShieldAbsorbed,
                TotalReflectDamage = source.TotalReflectDamage,
                Kills = source.Kills,
                KilledMonsterTemplateIds = source.KilledMonsterTemplateIds?.ToList() ?? [],
                Deaths = source.Deaths,
                NormalAttackCount = source.NormalAttackCount,
                ComboCount = source.ComboCount,
                CounterCount = source.CounterCount,
                DodgeCount = source.DodgeCount,
                MissedCount = source.MissedCount,
                CritCount = source.CritCount,
                TotalCritDamage = source.TotalCritDamage,
                TotalMpConsumed = source.TotalMpConsumed,
                TotalHpConsumed = source.TotalHpConsumed,
                SkillDamageDealt = source.SkillDamageDealt?.ToDictionary(kv => kv.Key, kv => kv.Value) ?? []
            };
        }


        /// <summary>
        /// 开始战斗（测试用重载方法）
        /// 直接传入玩家方和敌方的战斗单位列表
        /// </summary>
        /// <param name="playerSide">玩家方战斗单位列表</param>
        /// <param name="enemySide">敌方战斗单位列表</param>
        /// <param name="enableElementAdvantage">是否启用属性克制</param>
        /// <returns>战斗结果</returns>
        public static BattleResult StartBattle(List<BattleFighter> playerSide, List<BattleFighter> enemySide, bool enableElementAdvantage = true)
        {
            // 创建战斗上下文
            var context = new BattleContext
            {
                PlayerSide = playerSide,
                EnemySide = enemySide,
                EnableElementAdvantage = enableElementAdvantage
            };

            // 初始化所有战斗单位的技能冷却
            foreach (var fighter in playerSide.Concat(enemySide))
            {
                fighter.SkillCooldowns = [];
                fighter.ActiveBuffs = [];
            }

            // 执行战斗循环
            BattleExecutor.ExecuteBattle(context);

            // 生成战斗结果
            return BattleRecorder.GenerateBattleResult(context);
        }

        /// <summary>
        /// 开始战斗（测试用重载方法）
        /// 传入玩家实体列表和敌方怪物实体列表
        /// </summary>
        /// <param name="players">玩家实体列表</param>
        /// <param name="monsters">怪物实体列表</param>
        /// <param name="enableElementAdvantage">是否启用属性克制</param>
        /// <returns>战斗结果</returns>
        public static BattleResult StartBattle(
            List<UserEntity> players,
            List<MonsterEntity> monsters,
            bool enableElementAdvantage = true,
            IReadOnlyDictionary<string, PetEntity>? activePets = null)
        {
            // 创建战斗上下文
            var context = new BattleContext();
            var map = GameData.Maps["map_001"];//测试使用

            context.Map = map;
            context.EnableElementAdvantage = enableElementAdvantage;
            // 初始化我方战斗单位
            BattleInitializer.InitializePlayerSide(context, players, activePets);

            // 初始化敌方战斗单位
            foreach (var monster in monsters)
            {
                var fighter = new BattleFighter
                {
                    Id = monster.GID,
                    Name = monster.Name,
                    IsPlayerSide = false,
                    FighterType = FighterType.Monster,
                    CurrentHp = monster.Type1,
                    MaxHp = monster.Type1,
                    CurrentMp = monster.Type2,
                    MaxMp = monster.Type2,
                    MonsterTempID = monster.MonsterTempID
                };
                BattleInitializer.CopyAttributesFromMonster(fighter, monster);
                fighter.SkillIds = monster.SkillIds;
                fighter.PassiveIds = monster.PassiveIds;
                context.EnemySide.Add(fighter);
            }

            // 执行战斗循环
            BattleExecutor.ExecuteBattle(context);

            // 生成战斗结果
            return BattleRecorder.GenerateBattleResult(context);
        }

        /// <summary>
        /// 开始战斗（PvP重载方法）
        /// 传入玩家方实体列表和敌方玩家实体列表
        /// </summary>
        /// <param name="playerEntities">玩家方实体列表</param>
        /// <param name="enemyEntities">敌方玩家实体列表</param>
        /// <param name="enableElementAdvantage">是否启用属性克制</param>
        /// <returns>战斗结果</returns>
        public static BattleResult StartBattle(
            List<UserEntity> playerEntities,
            List<UserEntity> enemyEntities,
            bool enableElementAdvantage = true,
            IReadOnlyDictionary<string, PetEntity>? activePets = null)
        {
            // 创建战斗上下文
            var context = new BattleContext();
            var map = GameData.Maps["map_001"];//测试使用

            context.Map = map;
            context.EnableElementAdvantage = enableElementAdvantage;

            // 初始化玩家方战斗单位
            BattleInitializer.InitializePlayerSide(context, playerEntities, activePets);
            BattleInitializer.InitializeEnemyPlayerSide(context, enemyEntities, activePets);

            // 执行战斗循环
            BattleExecutor.ExecuteBattle(context);

            // 生成战斗结果
            return BattleRecorder.GenerateBattleResult(context);
        }
    }
}
