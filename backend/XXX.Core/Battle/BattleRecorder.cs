using XXX.Entity;

namespace XXX.Battle
{
    /// <summary>
    /// 战斗记录器
    /// 负责战斗记录和结果生成
    /// </summary>
    public static class BattleRecorder
    {
        /// <summary>
        /// 创建角色状态快照
        /// </summary>
        public static FighterStateSnapshot CreateFighterStateSnapshot(BattleFighter fighter)
        {
            var snapshot = new FighterStateSnapshot
            {
                FighterId = fighter.Id,
                FighterName = fighter.Name,
                IsPlayerSide = fighter.IsPlayerSide,

                // 血量蓝量
                CurrentHp = fighter.CurrentHp,
                MaxHp = fighter.MaxHp,
                CurrentMp = fighter.CurrentMp,
                MaxMp = fighter.MaxMp,

                // 基础属性
                PhysicalAttack = fighter.PhysicalAttack,
                MagicAttack = fighter.MagicAttack,
                PhysicalDefense = fighter.PhysicalDefense,
                MagicDefense = fighter.MagicDefense,
                Speed = fighter.Speed,
                ElementName = ElementRelation.GetElementName(fighter.Element),

                // 高级属性（格式化为百分比字符串）
                HitRate = (fighter.HitRate * 100).ToString("F1") + "%",
                DodgeRate = (fighter.DodgeRate * 100).ToString("F1") + "%",
                CritRate = (fighter.CritRate * 100).ToString("F1") + "%",
                CritDamage = (fighter.CritDamage * 100).ToString("F1") + "%",
                ComboRate = (fighter.ComboRate * 100).ToString("F1") + "%",
                CounterRate = (fighter.CounterRate * 100).ToString("F1") + "%",
                ArmorBreak = (fighter.ArmorBreak * 100).ToString("F1") + "%",
                ExtraDamage = (fighter.ExtraDamage * 100).ToString("F1") + "%",

                // Buff列表
                Buffs = fighter.ActiveBuffs?.Select(b => new ActiveBuffSnapshot
                {
                    BuffId = b.Template?.Gid ?? string.Empty,
                    BuffName = b.Template?.Name ?? string.Empty,
                    RemainingDuration = b.RemainingDuration,
                    Stack = b.CurrentStack
                }).ToList() ?? []
            };

            return snapshot;
        }

        /// <summary>
        /// 捕获所有角色的状态快照
        /// </summary>
        public static Dictionary<string, FighterStateSnapshot> CaptureAllFighterStates(BattleContext context)
        {
            var states = new Dictionary<string, FighterStateSnapshot>();

            // 调试：检查是否有重复ID
            var allIds = context.PlayerSide.Select(f => f.Id).Concat(context.EnemySide.Select(f => f.Id)).ToList();
            var duplicateIds = allIds.GroupBy(id => id).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (duplicateIds.Any())
            {
                System.Diagnostics.Debug.WriteLine($"[警告] 发现重复的角色ID: {string.Join(", ", duplicateIds)}");
            }

            foreach (var fighter in context.PlayerSide)
            {
                states[fighter.Id] = CreateFighterStateSnapshot(fighter);
            }

            foreach (var fighter in context.EnemySide)
            {
                states[fighter.Id] = CreateFighterStateSnapshot(fighter);
            }

            return states;
        }

        /// <summary>
        /// 获取或创建战斗单位的统计信息
        /// </summary>
        private static FighterSkillStats? GetOrCreateFighterStats(BattleContext? context, BattleFighter? fighter)
        {
            if (context == null || context.Result == null || fighter == null)
            {
                return null;
            }

            if (!context.Result.FighterSkillStats.ContainsKey(fighter.Id))
            {
                context.Result.FighterSkillStats[fighter.Id] = new FighterSkillStats
                {
                    GID = fighter.Id,
                    Name = fighter.Name
                };
            }
            return context.Result.FighterSkillStats[fighter.Id];
        }

        /// <summary>
        /// 记录伤害输出
        /// </summary>
        public static void RecordDamageDealt(BattleContext context, BattleFighter attacker, int damage)
        {
            if (attacker == null || damage <= 0) return;
            var stats = GetOrCreateFighterStats(context, attacker);
            if (stats != null)
            {
                stats.TotalDamageDealt += damage;
            }
        }

        /// <summary>
        /// 记录受到的伤害
        /// </summary>
        public static void RecordDamageTaken(BattleContext context, BattleFighter target, int damage)
        {
            if (target == null || damage <= 0) return;
            var stats = GetOrCreateFighterStats(context, target);
            if (stats != null)
            {
                stats.TotalDamageTaken += damage;
            }
        }

        /// <summary>
        /// 记录治疗量
        /// </summary>
        public static void RecordHealingDone(BattleContext context, BattleFighter healer, int healAmount)
        {
            if (healer == null || healAmount <= 0) return;
            var stats = GetOrCreateFighterStats(context, healer);
            if (stats != null)
            {
                stats.TotalHealingDone += healAmount;
            }
        }

        /// <summary>
        /// 记录受到的治疗
        /// </summary>
        public static void RecordHealingReceived(BattleContext context, BattleFighter target, int healAmount)
        {
            if (target == null || healAmount <= 0) return;
            var stats = GetOrCreateFighterStats(context, target);
            if (stats != null)
            {
                stats.TotalHealingReceived += healAmount;
            }
        }

        /// <summary>
        /// 记录吸血量
        /// </summary>
        public static void RecordLifesteal(BattleContext context, BattleFighter fighter, int lifestealAmount)
        {
            if (fighter == null || lifestealAmount <= 0) return;
            var stats = GetOrCreateFighterStats(context, fighter);
            if (stats != null)
            {
                stats.TotalLifesteal += lifestealAmount;
            }
        }

        /// <summary>
        /// 记录护盾吸收的伤害
        /// </summary>
        public static void RecordShieldAbsorbed(BattleContext context, BattleFighter target, int absorbedAmount)
        {
            if (target == null || absorbedAmount <= 0) return;
            var stats = GetOrCreateFighterStats(context, target);
            if (stats != null)
            {
                stats.TotalShieldAbsorbed += absorbedAmount;
            }
        }

        /// <summary>
        /// 记录反伤造成的伤害
        /// </summary>
        public static void RecordReflectDamage(BattleContext context, BattleFighter reflector, int reflectAmount)
        {
            if (reflector == null || reflectAmount <= 0) return;
            var stats = GetOrCreateFighterStats(context, reflector);
            if (stats != null)
            {
                stats.TotalReflectDamage += reflectAmount;
            }
        }

        /// <summary>
        /// 记录击杀
        /// </summary>
        public static void RecordKill(BattleContext context, BattleFighter killer, BattleFighter? victim = null)
        {
            if (killer == null) return;
            var stats = GetOrCreateFighterStats(context, killer);
            if (stats != null)
            {
                stats.Kills++;
                if (victim != null &&
                    !victim.IsPlayerSide &&
                    !string.IsNullOrWhiteSpace(victim.MonsterTempID))
                {
                    stats.KilledMonsterTemplateIds.Add(victim.MonsterTempID.Trim());
                }
            }
        }

        /// <summary>
        /// 记录死亡
        /// </summary>
        public static void RecordDeath(BattleContext context, BattleFighter victim)
        {
            if (victim == null) return;
            var stats = GetOrCreateFighterStats(context, victim);
            if (stats != null)
            {
                stats.Deaths++;
            }
        }

        /// <summary>
        /// 记录击杀和死亡（同时记录击杀者和被击杀者）
        /// </summary>
        public static void RecordKillAndDeath(BattleContext context, BattleFighter? killer, BattleFighter victim)
        {
            if (killer != null)
            {
                RecordKill(context, killer, victim);
            }
            RecordDeath(context, victim);
        }



        /// <summary>
        /// 记录普通攻击
        /// </summary>
        public static void RecordNormalAttack(BattleContext context, BattleFighter attacker)
        {
            if (attacker == null) return;
            var stats = GetOrCreateFighterStats(context, attacker);
            if (stats != null)
            {
                stats.NormalAttackCount++;
            }
        }

        /// <summary>
        /// 记录连击
        /// </summary>
        public static void RecordCombo(BattleContext context, BattleFighter attacker)
        {
            if (attacker == null) return;
            var stats = GetOrCreateFighterStats(context, attacker);
            if (stats != null)
            {
                stats.ComboCount++;
            }
        }

        /// <summary>
        /// 记录反击
        /// </summary>
        public static void RecordCounter(BattleContext context, BattleFighter counter)
        {
            if (counter == null) return;
            var stats = GetOrCreateFighterStats(context, counter);
            if (stats != null)
            {
                stats.CounterCount++;
            }
        }

        /// <summary>
        /// 记录闪避
        /// </summary>
        public static void RecordDodge(BattleContext context, BattleFighter dodger)
        {
            if (dodger == null) return;
            var stats = GetOrCreateFighterStats(context, dodger);
            if (stats != null)
            {
                stats.DodgeCount++;
            }
        }

        /// <summary>
        /// 记录攻击被闪避
        /// </summary>
        public static void RecordMissed(BattleContext context, BattleFighter attacker)
        {
            if (attacker == null) return;
            var stats = GetOrCreateFighterStats(context, attacker);
            if (stats != null)
            {
                stats.MissedCount++;
            }
        }

        /// <summary>
        /// 记录暴击
        /// </summary>
        public static void RecordCrit(BattleContext context, BattleFighter attacker, int critDamage)
        {
            if (attacker == null) return;
            var stats = GetOrCreateFighterStats(context, attacker);
            if (stats != null)
            {
                stats.CritCount++;
                if (critDamage > 0)
                {
                    stats.TotalCritDamage += critDamage;
                }
            }
        }

        /// <summary>
        /// 记录蓝量消耗
        /// </summary>
        public static void RecordMpConsumed(BattleContext context, BattleFighter caster, int mpAmount)
        {
            if (caster == null || mpAmount <= 0) return;
            var stats = GetOrCreateFighterStats(context, caster);
            if (stats != null)
            {
                stats.TotalMpConsumed += mpAmount;
            }
        }

        /// <summary>
        /// 记录生命消耗
        /// </summary>
        public static void RecordHpConsumed(BattleContext context, BattleFighter caster, int hpAmount)
        {
            if (caster == null || hpAmount <= 0) return;
            var stats = GetOrCreateFighterStats(context, caster);
            if (stats != null)
            {
                stats.TotalHpConsumed += hpAmount;
            }
        }

        /// <summary>
        /// 记录技能伤害
        /// </summary>
        public static void RecordSkillDamage(BattleContext context, BattleFighter attacker, int skillId, int damage)
        {
            if (attacker == null || damage <= 0) return;
            var stats = GetOrCreateFighterStats(context, attacker);
            if (stats != null)
            {
                if (!stats.SkillDamageDealt.ContainsKey(skillId))
                {
                    stats.SkillDamageDealt[skillId] = 0;
                }
                stats.SkillDamageDealt[skillId] += damage;
            }
        }

        /// <summary>
        /// 生成战斗结果
        /// </summary>
        public static BattleResult GenerateBattleResult(BattleContext context)
        {
            return context.Result;
        }
    }
}
