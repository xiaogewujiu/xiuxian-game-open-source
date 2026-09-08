using XXX.Entity;

namespace XXX.Battle
{
    /// <summary>
    /// 连击反击执行器
    /// 负责连击和反击的执行
    /// </summary>
    public static class ComboCounterExecutor
    {/// <summary>
     /// 随机数生成器（线程安全）
     /// </summary>
        private static Random Rand = Random.Shared;
        /// <summary>
        /// 尝试连击
        /// </summary>
        public static void TryComboAttack(BattleFighter attacker, BattleFighter target, BattleContext context, RoundLog log)
        {
            if (target.CurrentHp <= 0) return;

            // 分身不触发连击，也不会被连击
            if (attacker.IsMirror || target.IsMirror) return;

            // 检查连击次数是否已达到上限
            if (attacker.ComboCountThisRound >= BattleSystem.MaxComboPerRound)
            {
                return;
            }

            // 计算连击率（考虑Buff）
            float comboRate = DamageCalculator.GetModifiedComboRate(attacker);

            // 连击率大于0才判定
            if (comboRate > 0 && Rand.NextDouble() < comboRate)
            {

                // 增加连击计数器
                attacker.ComboCountThisRound++;

                // 记录连击次数
                BattleRecorder.RecordCombo(context, attacker);

                // 尝试使用技能进行连击
                bool usedSkill = TryComboSkill(attacker, target, context, log);

                // 如果没有可用技能，使用普通攻击
                if (!usedSkill)
                {
                    // 被缴械时无法使用普通攻击
                    if (BattleHelper.IsDisarmed(attacker))
                    {
                        new BattleLogBuilder(log, context, BattleLogType.Disarm)
                            .Target(attacker)
                            .Description($"{attacker.Name} 受到[缴械]影响，无法触发普通攻击连击。")
                            .Build();
                        return;
                    }

                    DamageResult damageResult = DamageCalculator.CalculateNormalDamage(attacker, target, log, context);
                    int damage = damageResult.Damage;

                    // 应用连击伤害修正
                    damage = DamageCalculator.ApplyComboDamageModifier(attacker, damage);

                    // 护盾吸收
                    int afterShieldDamage = DamageCalculator.ApplyShieldDamage(target, damage, context, log);
                    // 庇护分担
                    int actualDamage = DamageCalculator.ApplySanctuaryDamage(target, afterShieldDamage, context, log);
                    target.CurrentHp -= actualDamage;

                    // 格式化伤害日志
                    string damageText = damageResult.IsCrit ? $"{actualDamage}（暴击）" : actualDamage.ToString();
                    new BattleLogBuilder(log, context, BattleLogType.ComboAttack)
                        .Caster(attacker)
                        .Target(target)
                        .Value(actualDamage)
                        .Crit(damageResult.IsCrit)
                        .Description($"{attacker.Name} 触发连击，对 {target.Name} 追加造成 {damageText} 点伤害，剩余气血 {Math.Max(0, target.CurrentHp)}。")
                        .Build();

                    // 记录伤害输出和受到的伤害
                    BattleRecorder.RecordDamageDealt(context, attacker, actualDamage);
                    BattleRecorder.RecordDamageTaken(context, target, actualDamage);

                    // 吸血效果
                    DamageCalculator.ApplyLifesteal(attacker, actualDamage, context, log);

                    // 反伤效果
                    if (actualDamage > 0)
                    {
                        DamageCalculator.ApplyReflectDamage(target, attacker, actualDamage, context, log);
                    }

                    // 检查目标是否死亡，记录击杀和死亡
                    if (target.CurrentHp <= 0)
                    {
                        BattleHelper.ResolveFatalDamage(target, attacker, context, log);
                    }

                    // 连击使用普通攻击后，可以触发反击
                    if (target.CurrentHp > 0 && !attacker.IsMirror && !target.IsMirror)
                    {
                        ComboCounterExecutor.TryCounterAttack(target, attacker, context, log);
                    }
                }

                // 连击本身也可以再次触发连击（支持连锁连击）
                // 只有目标存活时才能继续连击
                if (target.CurrentHp > 0)
                {
                    TryComboAttack(attacker, target, context, log);
                }
            }
        }

        /// <summary>
        /// 尝试反击
        /// </summary>
        public static void TryCounterAttack(BattleFighter defender, BattleFighter attacker, BattleContext context, RoundLog log)
        {
            if (attacker.CurrentHp <= 0) return;

            // 分身不触发反击，也不会被反击
            if (defender.IsMirror || attacker.IsMirror) return;

            // 检查是否被束缚
            if (BattleHelper.IsRooted(defender))
            {
                new BattleLogBuilder(log, context, BattleLogType.Root)
                    .Target(defender)
                    .Description($"{defender.Name} 受到[束缚]影响，无法进行反击。")
                    .Build();
                return; // 被束缚无法反击
            }

            // 检查对当前攻击者的反击次数是否已达到上限
            defender.CounterCountPerAttacker.TryGetValue(attacker.Id, out int currentCount);
            if (currentCount >= BattleSystem.MaxCounterPerRound)
            {
                return;
            }

            // 计算反击率（考虑Buff）
            float counterRate = DamageCalculator.GetModifiedCounterRate(defender);

            // 反击率大于0才判定
            if (counterRate > 0 && Rand.NextDouble() < counterRate)
            {

                // 增加对当前攻击者的反击计数器
                defender.CounterCountPerAttacker[attacker.Id] = currentCount + 1;

                // 记录反击次数
                BattleRecorder.RecordCounter(context, defender);

                // 尝试使用技能进行反击
                bool usedSkill = TryCounterSkill(defender, attacker, context, log);

                // 如果没有可用技能，使用普通攻击（反击伤害为50%）
                if (!usedSkill)
                {
                    // 被缴械时无法使用普通攻击
                    if (BattleHelper.IsDisarmed(defender))
                    {
                        new BattleLogBuilder(log, context, BattleLogType.Disarm)
                            .Target(defender)
                            .Description($"{defender.Name} 受到[缴械]影响，无法发动普通攻击反击。")
                            .Build();
                        return;
                    }

                    DamageResult damageResult = DamageCalculator.CalculateNormalDamage(defender, attacker, log, context);
                    int damage = (int)(damageResult.Damage * 0.5);

                    // 应用反击伤害修正
                    damage = DamageCalculator.ApplyCounterDamageModifier(defender, damage);

                    // 护盾吸收
                    int afterShieldDamage = DamageCalculator.ApplyShieldDamage(attacker, damage, context, log);
                    // 庇护分担
                    int actualDamage = DamageCalculator.ApplySanctuaryDamage(attacker, afterShieldDamage, context, log);
                    attacker.CurrentHp -= actualDamage;

                    // 格式化伤害日志
                    string damageText = damageResult.IsCrit ? $"{actualDamage}（暴击）" : actualDamage.ToString();
                    new BattleLogBuilder(log, context, BattleLogType.CounterAttack)
                        .Caster(defender)
                        .Target(attacker)
                        .Value(actualDamage)
                        .Crit(damageResult.IsCrit)
                        .Description($"{defender.Name} 发起反击，对 {attacker.Name} 造成 {damageText} 点伤害，剩余气血 {Math.Max(0, attacker.CurrentHp)}。")
                        .Build();

                    // 记录伤害输出和受到的伤害
                    BattleRecorder.RecordDamageDealt(context, defender, actualDamage);
                    BattleRecorder.RecordDamageTaken(context, attacker, actualDamage);

                    // 吸血效果
                    DamageCalculator.ApplyLifesteal(defender, actualDamage, context, log);

                    // 反伤效果
                    if (actualDamage > 0)
                    {
                        DamageCalculator.ApplyReflectDamage(attacker, defender, actualDamage, context, log);
                    }

                    // 检查目标是否死亡，使用统一致死结算
                    if (attacker.CurrentHp <= 0)
                    {
                        BattleHelper.ResolveFatalDamage(attacker, defender, context, log);
                    }
                    // 反击使用普通攻击后，可以触发连击
                    else if (!defender.IsMirror && !attacker.IsMirror)
                    {
                        ComboCounterExecutor.TryComboAttack(attacker, defender, context, log);
                    }
                }
                // 技能反击后也可以触发连击
                else if (attacker.CurrentHp > 0 && !defender.IsMirror && !attacker.IsMirror)
                {
                    ComboCounterExecutor.TryComboAttack(attacker, defender, context, log);
                }

                // 反击本身也可以再次触发反击（支持连锁反击）
                // 注意：这里反击的对象是"反击者"（attacker），而不是"被反击者"（defender）
                // 这样可以实现"你来我往"的反击效果
                if (attacker.CurrentHp > 0)
                {
                    TryCounterAttack(attacker, defender, context, log);
                }
            }
        }

        /// <summary>
        /// 连击时尝试使用技能（支持多段伤害）
        /// </summary>
        public static bool TryComboSkill(BattleFighter attacker, BattleFighter target, BattleContext context, RoundLog log)
        {
            // 被沉默无法使用技能
            if (BattleHelper.IsSilenced(attacker)) return false;

            foreach (var skillIdStr in attacker.SkillIds)
            {
                if (!int.TryParse(skillIdStr, out int skillId)) continue;
                if (!SkillData.Skills.ContainsKey(skillId)) continue;

                var skill = SkillData.Skills[skillId];

                // 只能使用对敌方的单体攻击技能
                if (skill.TargetType != 1) continue;
                if (skill.RangeType == 0) continue; // 跳过全体技能
                if (skill.DamageType == DamageType.Heal ||
                    skill.DamageType == DamageType.Buff ||
                    skill.DamageType == DamageType.Revive ||
                    skill.DamageType == DamageType.ManaRestore) continue;

                // 检查冷却
                if (attacker.SkillCooldowns.ContainsKey(skillId) && attacker.SkillCooldowns[skillId] > 0)
                    continue;

                // 检查蓝量
                if (attacker.CurrentMp < skill.ManaCost)
                    continue;

                // 检查触发概率
                if (Rand.NextDouble() > skill.TriggerChance)
                    continue;

                // 使用技能
                attacker.CurrentMp -= skill.ManaCost;
                attacker.SkillCooldowns[skill.Id] = skill.Cooldown;
                attacker.SkillsUsedThisRound.Add(skill.Id); // 标记为本回合使用的技能
                SkillExecutor.RecordSkillUsage(context, attacker, skill.Id); // 记录技能使用次数
                if (skill.ManaCost > 0)
                {
                    BattleRecorder.RecordMpConsumed(context, attacker, skill.ManaCost); // 记录蓝量消耗
                }
                bool isHit = SkillExecutor.ExecuteSkillOnTarget(attacker, skill, target, context, log, SkillLogContext.Combo);

                // 应用Buff（传入技能命中的目标列表，让ApplySkillBuffs根据配置自动选择目标）
                if (isHit)
                {
                    BuffProcessor.ApplySkillBuffs(attacker, skill, [target], context, log);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 反击时尝试使用技能（支持多段伤害）
        /// </summary>
        public static bool TryCounterSkill(BattleFighter defender, BattleFighter attacker, BattleContext context, RoundLog log)
        {
            // 被沉默无法使用技能
            if (BattleHelper.IsSilenced(defender)) return false;

            foreach (var skillIdStr in defender.SkillIds)
            {
                if (!int.TryParse(skillIdStr, out int skillId)) continue;
                if (!SkillData.Skills.ContainsKey(skillId)) continue;

                var skill = SkillData.Skills[skillId];

                // 只能使用对敌方的单体攻击技能
                if (skill.TargetType != 1) continue;
                if (skill.RangeType == 0) continue; // 跳过全体技能
                if (skill.DamageType == DamageType.Heal ||
                    skill.DamageType == DamageType.Buff ||
                    skill.DamageType == DamageType.Revive ||
                    skill.DamageType == DamageType.ManaRestore) continue;

                // 检查冷却
                if (defender.SkillCooldowns.ContainsKey(skillId) && defender.SkillCooldowns[skillId] > 0)
                    continue;

                // 检查蓝量
                if (defender.CurrentMp < skill.ManaCost)
                    continue;

                // 检查触发概率
                if (Rand.NextDouble() > skill.TriggerChance)
                    continue;

                // 使用技能（反击技能伤害减半）
                defender.CurrentMp -= skill.ManaCost;
                defender.SkillCooldowns[skill.Id] = skill.Cooldown;
                defender.SkillsUsedThisRound.Add(skill.Id); // 标记为本回合使用的技能
                SkillExecutor.RecordSkillUsage(context, defender, skill.Id); // 记录技能使用次数
                if (skill.ManaCost > 0)
                {
                    BattleRecorder.RecordMpConsumed(context, defender, skill.ManaCost); // 记录蓝量消耗
                }
                bool isHit = SkillExecutor.ExecuteCounterSkillOnTarget(defender, skill, attacker, context, log, SkillLogContext.Counter);

                // 应用Buff（传入技能命中的目标列表，让ApplySkillBuffs根据配置自动选择目标）
                if (isHit)
                {
                    BuffProcessor.ApplySkillBuffs(defender, skill, [attacker], context, log);
                }
                return true;
            }
            return false;
        }
    }
}
