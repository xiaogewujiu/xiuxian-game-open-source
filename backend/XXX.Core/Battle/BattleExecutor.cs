using XXX.Entity;

namespace XXX.Battle
{
    /// <summary>
    /// 战斗执行器
    /// 负责战斗主循环的执行
    /// </summary>
    public static class BattleExecutor
    {
        /// <summary>
        /// 执行战斗主循环
        /// </summary>
        public static void ExecuteBattle(BattleContext context)
        {
            // 战斗开始：应用所有被动Buff
            var passiveLog = new RoundLog
            {
                RoundNumber = 0,             // 捕获回合0初始状态快照
                FighterStates = BattleRecorder.CaptureAllFighterStates(context)
                    .Values.ToList()
            };

            ApplyPassiveBuffs(context, passiveLog);

            // 无论是否有被动Buff日志，都添加Round 0（包含初始状态）
            context.Result.RoundLogs.Add(passiveLog);

            int round = 0;
            while (round < BattleSystem.MaxRounds)
            {
                round++;
                var roundLog = new RoundLog { RoundNumber = round };

                // 获取所有存活单位并按速度排序（考虑减速/加速效果）
                var allFighters = GetAllAliveFighters(context);
                allFighters = allFighters.OrderByDescending(f => DamageCalculator.GetModifiedSpeed(f, f.Speed)).ToList();

                // 记录本回合开始时的存活状态
                var aliveBeforeRound = new HashSet<string>();
                foreach (var fighter in allFighters)
                {
                    aliveBeforeRound.Add(fighter.Id);
                }

                // 回合开始：处理Buff效果
                foreach (var fighter in allFighters)
                {
                    // 重置本回合的连击计数器和对每个攻击者的反击计数器
                    fighter.ComboCountThisRound = 0;
                    fighter.CounterCountPerAttacker.Clear();

                    BuffProcessor.ProcessBuffsAtRoundStart(fighter, context, roundLog);
                }

                // 每个单位依次行动
                foreach (var fighter in allFighters)
                {
                    if (fighter.CurrentHp <= 0) continue;

                    // 清空该单位自己对所有人的反击计数（确保被反击时可以反击回去）
                    fighter.CounterCountPerAttacker.Clear();

                    // 清空所有人对该单位的反击计数（确保攻击该单位时对方可以反击）
                    foreach (var other in allFighters)
                    {
                        if (other != fighter)
                        {
                            other.CounterCountPerAttacker.Remove(fighter.Id);
                        }
                    }

                    if (BattleHelper.IsStunned(fighter))
                    {
                        new BattleLogBuilder(roundLog, context, BattleLogType.Stun)
                            .Target(fighter)
                            .Description($"{fighter.Name} 受到[眩晕]影响，本回合无法行动。")
                            .Build();
                        continue;
                    }

                    ExecuteFighterAction(context, fighter, roundLog);

                    // 检查战斗是否结束
                    if (IsBattleOver(context))
                        break;
                }

                // 回合结束：更新Buff持续时间
                BuffProcessor.UpdateBuffDurations(context);

                context.Result.RoundLogs.Add(roundLog);

                if (IsBattleOver(context))
                    break;
            }

            // 设置战斗结果
            context.Result.MapName = context.Map?.Name ?? string.Empty;
            context.Result.TotalRounds = round;
            context.Result.IsVictory = context.EnemySide.All(f => f.CurrentHp <= 0);

            // 如果胜利，计算奖励
            if (context.Result.IsVictory)
            {
                BattleRewards.CalculateRewards(context);
            }
        }

        /// <summary>
        /// 执行单个战斗单位的行动
        /// </summary>
        public static void ExecuteFighterAction(BattleContext context, BattleFighter fighter, RoundLog log)
        {
            // 获取敌方目标列表
            var enemies = fighter.IsPlayerSide
                ? context.EnemySide.Where(f => f.CurrentHp > 0).ToList()
                : context.PlayerSide.Where(f => f.CurrentHp > 0).ToList();

            var allies = fighter.IsPlayerSide
                ? context.PlayerSide.ToList()  // 包含死亡的友方，用于复活
                : context.EnemySide.ToList();

            var aliveAllies = allies.Where(f => f.CurrentHp > 0).ToList();

            if (enemies.Count == 0) return;

            // 检查是否被魅惑
            bool isCharmed = BuffProcessor.IsCharmed(fighter);
            if (isCharmed)
            {
                // 魅惑状态下，目标选择反转（攻击友方）
                new BattleLogBuilder(log, context, BattleLogType.Charm)
                    .Target(fighter)
                    .Description($"{fighter.Name} 受到[魅惑]影响，转而攻击友方单位。")
                    .Build();
                // 将友方视为敌方目标（排除自己）
                enemies = aliveAllies.Where(f => f != fighter).ToList();
                if (enemies.Count == 0) return; // 没有友方可攻击
            }

            // 检查是否被嘲讽
            var tauntTarget = BattleHelper.GetTauntTarget(fighter);
            if (tauntTarget != null && tauntTarget.CurrentHp > 0)
            {
                // 被嘲讽，必须攻击嘲讽者
                new BattleLogBuilder(log, context, BattleLogType.BuffApply)
                    .Caster(fighter)
                    .Target(tauntTarget)
                    .Description($"{fighter.Name} 受到[嘲讽]影响，被迫攻击 {tauntTarget.Name}。")
                    .Build();
                NormalAttackExecutor.ExecuteNormalAttack(fighter, tauntTarget, context, log);
                return;
            }

            // 分身没有技能，只能普通攻击
            if (fighter.IsMirror)
            {
                // 智能选择攻击目标
                var target = TargetSelector.SelectAttackTarget(enemies);
                if (target != null)
                {
                    NormalAttackExecutor.ExecuteNormalAttack(fighter, target, context, log);
                }
                return;
            }

            // 检查是否被缴械（无法使用普通攻击，只能使用技能）
            bool isDisarmed = BattleHelper.IsDisarmed(fighter);

            // 尝试使用技能
            bool usedSkill = SkillExecutor.TryUseSkill(context, fighter, enemies, allies, log);

            // 如果没有使用技能，执行普通攻击
            if (!usedSkill)
            {
                // 被缴械时无法使用普通攻击
                if (isDisarmed)
                {
                    new BattleLogBuilder(log, context, BattleLogType.Disarm)
                        .Target(fighter)
                        .Description($"{fighter.Name} 受到[缴械]影响，无法使用普通攻击。")
                        .Build();
                    return;
                }

                // 智能选择攻击目标
                var target = TargetSelector.SelectAttackTarget(enemies);
                if (target != null)
                {
                    NormalAttackExecutor.ExecuteNormalAttack(fighter, target, context, log);
                }
            }
        }

        /// <summary>
        /// 获取所有存活的战斗单位
        /// </summary>
        public static List<BattleFighter> GetAllAliveFighters(BattleContext context)
        {
            var result = new List<BattleFighter>();

            // 添加所有主战斗单位
            result.AddRange(context.PlayerSide.Where(f => f.CurrentHp > 0 && !f.IsMirror));
            result.AddRange(context.EnemySide.Where(f => f.CurrentHp > 0 && !f.IsMirror));

            // 添加所有存活分身
            foreach (var fighter in context.PlayerSide.Concat(context.EnemySide).Where(f => !f.IsMirror))
            {
                foreach (var mirror in fighter.Mirrors.Where(m => m.CurrentHp > 0))
                {
                    result.Add(mirror);
                }
            }

            return result;
        }

        /// <summary>
        /// 检查战斗是否结束
        /// </summary>
        public static bool IsBattleOver(BattleContext context)
        {
            // 检查玩家方是否失败：所有主单位死亡，且没有存活的分身
            bool playerDefeated = context.PlayerSide
                .Where(f => !f.IsMirror) // 只检查主单位
                .All(f => f.CurrentHp <= 0) && // 所有主单位死亡
                context.PlayerSide
                .Where(f => f.IsMirror) // 检查分身
                .All(f => f.CurrentHp <= 0 || f.MirrorOwner?.CurrentHp <= 0); // 所有分身死亡或其主人死亡

            // 检查敌方是否失败：所有主单位死亡，且没有存活的分身
            bool enemyDefeated = context.EnemySide
                .Where(f => !f.IsMirror) // 只检查主单位
                .All(f => f.CurrentHp <= 0) && // 所有主单位死亡
                context.EnemySide
                .Where(f => f.IsMirror) // 检查分身
                .All(f => f.CurrentHp <= 0 || f.MirrorOwner?.CurrentHp <= 0); // 所有分身死亡或其主人死亡

            return playerDefeated || enemyDefeated;
        }

        /// <summary>
        /// 应用所有战斗单位的被动Buff（战斗开始时调用一次）
        /// </summary>
        public static void ApplyPassiveBuffs(BattleContext context, RoundLog log)
        {
            foreach (var fighter in GetAllAliveFighters(context))
            {
                if (fighter.IsMirror) continue; // 分身不触发被动

                if (fighter.PassiveIds == null || fighter.PassiveIds.Count == 0)
                    continue;

                foreach (var passiveId in fighter.PassiveIds)
                {
                    if (!BuffDataTemplates.BuffTemplates.ContainsKey(passiveId))
                        continue;

                    var template = BuffDataTemplates.BuffTemplates[passiveId];

                    // 根据Buff目标类型，传入正确的目标列表
                    List<BattleFighter> skillTargets = GetPassiveTargets(fighter, template, context);

                    // 使用现有的目标选择机制（被动触发无关联技能，传null）
                    BuffProcessor.ApplyBuffWithTargetSelection(fighter, null, template, skillTargets, context, log);
                }
            }
        }

        /// <summary>
        /// 根据被动Buff的目标类型获取目标列表
        /// </summary>
        private static List<BattleFighter> GetPassiveTargets(BattleFighter caster, BuffTemplate template, BattleContext context)
        {
            if (template.Effects == null || template.Effects.Count == 0)
                return [];

            var firstEffect = template.Effects[0];

            // 自己的被动 - 传给自己，让TargetSelfOnly处理
            if (firstEffect.TargetSelfOnly)
                return [caster];

            // 对敌人的被动 - 传入敌方列表
            if (firstEffect.TargetCamp == 1)
            {
                return caster.IsPlayerSide
                    ? context.EnemySide.Where(f => f.CurrentHp > 0).ToList()
                    : context.PlayerSide.Where(f => f.CurrentHp > 0).ToList();
            }

            // 对友方的被动 - 传入友方列表（包括自己）
            if (firstEffect.TargetCamp == 2)
            {
                return caster.IsPlayerSide
                    ? context.PlayerSide.Where(f => f.CurrentHp > 0).ToList()
                    : context.EnemySide.Where(f => f.CurrentHp > 0).ToList();
            }

            return [];
        }
    }
}
