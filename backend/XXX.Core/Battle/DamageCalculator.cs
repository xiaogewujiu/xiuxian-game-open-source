using XXX.Entity;

namespace XXX.Battle
{
    /// <summary>
    /// 伤害结果
    /// </summary>
    public struct DamageResult
    {
        public int Damage;
        public bool IsCrit;

        /// <summary>
        /// 初始化一次伤害结算结果。
        /// </summary>
        public DamageResult(int damage, bool isCrit)
        {
            Damage = damage;
            IsCrit = isCrit;
        }
    }

    /// <summary>
    /// 伤害计算器
    /// 负责伤害计算和属性修正
    /// </summary>
    public static class DamageCalculator
    {/// <summary>
     /// 随机数生成器（线程安全）
     /// </summary>
        private static Random Rand = Random.Shared;
        /// <summary>
        /// 计算技能伤害
        /// 物理技能使用物攻/物防，法术技能使用法攻/法防
        /// </summary>
        public static int CalculateSkillDamage(BattleFighter caster, XXX.Entity.Skill skill,
            BattleFighter target, RoundLog log, BattleContext? context = null)
        {
            // 命中判定（命中率至少为0，闪避率至少为0）
            float hitRate = GetModifiedAccuracy(caster, Math.Max(0, caster.HitRate));
            float dodgeRate = Math.Max(0, target.DodgeRate);
            float hitChance = Math.Max(0.1f, hitRate - dodgeRate); // 最低10%命中
            if (Rand.NextDouble() > hitChance)
            {
                new BattleLogBuilder(log, context, BattleLogType.Dodge)
                    .Caster(caster)
                    .Target(target)
                    .Description($"{target.Name} 闪避了 {caster.Name} 的技能[{skill.Name}]。")
                    .Build();
                // 记录闪避和攻击被闪避
                if (context != null)
                {
                    BattleRecorder.RecordDodge(context, target);
                    BattleRecorder.RecordMissed(context, caster);
                }
                return 0;
            }

            // 根据技能类型选择攻击和防御属性
            int baseAtk, baseDef;
            if (skill.DamageType == DamageType.Physical)
            {
                baseAtk = caster.PhysicalAttack;
                baseDef = target.PhysicalDefense;
            }
            else if (skill.DamageType == DamageType.Magic)
            {
                baseAtk = caster.MagicAttack;
                baseDef = target.MagicDefense;
            }
            else // True伤害
            {
                baseAtk = Math.Max(caster.PhysicalAttack, caster.MagicAttack);
                baseDef = 0;
            }
            //攻击上下浮动5%;
            baseAtk = baseAtk.GetRandomValue();
            // 应用Buff修正攻击力和防御力
            baseAtk = GetModifiedAttack(caster, baseAtk);
            if (skill.DamageType != DamageType.True)
                baseDef = GetModifiedDefense(target, baseDef);

            // 破甲计算（破甲率至少为0，最高70%）
            float armorBreak = Math.Max(0, Math.Min(0.7f, caster.ArmorBreak));
            int effectiveDef = (int)(baseDef * (1 - armorBreak));

            // 伤害公式：攻击力² / (攻击力 + 防御力)，避免刮痧同时保证防御价值
            double damage = (baseAtk * baseAtk) / (double)(baseAtk + effectiveDef);
            damage *= skill.DamageMultiplier;
            // 伤害下限为攻击力的5%，避免刮痧
            double minDamage = baseAtk * 0.05;
            damage = Math.Max(minDamage, damage);

            // 暴击判定（应用Buff修正）
            float modifiedCritRate = GetModifiedCritRate(caster, caster.CritRate);
            float targetCritResist = GetCritResist(target);
            float finalCritRate = Math.Max(0, modifiedCritRate - targetCritResist); // 暴击率减去目标的暴击抵抗

            if (finalCritRate > 0 && Rand.NextDouble() < finalCritRate)
            {
                float modifiedCritDmg = GetModifiedCritDamage(caster, Math.Max(1.0f, caster.CritDamage));
                double damageBeforeCrit = damage;
                damage *= modifiedCritDmg;
                int critDamage = (int)(damage - damageBeforeCrit);
                // 记录暴击
                if (context != null)
                {
                    BattleRecorder.RecordCrit(context, caster, critDamage);
                }
            }

            // 属性克制修正
            if (context != null && context.EnableElementAdvantage)
            {
                float elementModifier = ElementRelation.GetModifier(caster.Element, target.Element);
                if (elementModifier != 0)
                {
                    damage *= (1 + elementModifier);
                }
            }

            // Buff加成
            damage = ApplyBuffModifiers(caster, target, damage);

            // 斩杀效果：对低血量目标造成额外伤害
            damage = ApplyExecuteDamage(caster, target, damage, log, context);

            // 额外伤害加成（至少为0）
            float extraDmg = Math.Max(0, caster.ExtraDamage);
            damage *= (1 + extraDmg);

            // 最终伤害保底已在基础伤害中处理，这里直接返回
            return (int)Math.Max(0, damage);
        }
        /// <summary>
        /// 攻击力浮动方法
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private static int GetRandomValue(this int x)
        {
            return x;
            //return (int)(x * (1 + (Random.Shared.NextDouble() * 0.1 - 0.05)));
        }
        /// <summary>
        /// 计算普通攻击伤害
        /// 物理攻击和法术攻击取高者，对应使用物理防御或法术防御
        /// </summary>
        public static DamageResult CalculateNormalDamage(BattleFighter attacker, BattleFighter target, RoundLog log, BattleContext? context = null)
        {
            // 检查无敌效果
            if (BuffProcessor.HasInvincible(target))
            {
                new BattleLogBuilder(log, context, BattleLogType.DamageTaken)
                    .Target(target)
                    .Description($"{target.Name} 处于无敌状态，免疫了这次伤害。")
                    .Build();
                return new DamageResult(0, false);
            }

            // 物理攻击和法术攻击取高者
            bool usePhysical = attacker.PhysicalAttack >= attacker.MagicAttack;
            int baseAtk = usePhysical ? attacker.PhysicalAttack : attacker.MagicAttack;
            int baseDef = usePhysical ? target.PhysicalDefense : target.MagicDefense;

            baseAtk = baseAtk.GetRandomValue();

            // 应用Buff修正攻击力和防御力
            baseAtk = GetModifiedAttack(attacker, baseAtk);
            baseDef = GetModifiedDefense(target, baseDef);

            // 破甲（破甲率限制在0-70%）
            float armorBreak = Math.Max(0, Math.Min(0.7f, attacker.ArmorBreak));
            int effectiveDef = (int)(baseDef * (1 - armorBreak));

            // 基础伤害：攻击力² / (攻击力 + 防御力)
            double damage = (baseAtk * baseAtk) / (double)(baseAtk + effectiveDef);
            // 伤害下限为攻击力的5%，避免刮痧
            double minDamage = baseAtk * 0.05;
            damage = Math.Max(minDamage, damage);

            // 暴击判定（应用Buff修正）
            float modifiedCritRate = GetModifiedCritRate(attacker, attacker.CritRate);
            float targetCritResist = GetCritResist(target);
            float finalCritRate = Math.Max(0, modifiedCritRate - targetCritResist); // 暴击率减去目标的暴击抵抗

            bool isCrit = false;
            if (finalCritRate > 0 && Rand.NextDouble() < finalCritRate)
            {
                isCrit = true;
                float modifiedCritDmg = GetModifiedCritDamage(attacker, Math.Max(1.0f, attacker.CritDamage));
                double damageBeforeCrit = damage;
                damage *= modifiedCritDmg;
                int critDamage = (int)(damage - damageBeforeCrit);
                // 记录暴击
                if (context != null)
                {
                    BattleRecorder.RecordCrit(context, attacker, critDamage);
                }
            }

            // 属性克制修正
            if (context != null && context.EnableElementAdvantage)
            {
                float elementModifier = ElementRelation.GetModifier(attacker.Element, target.Element);
                if (elementModifier != 0)
                {
                    damage *= (1 + elementModifier);
                }
            }

            // Buff修正
            damage = ApplyBuffModifiers(attacker, target, damage);

            // 斩杀效果：对低血量目标造成额外伤害
            damage = ApplyExecuteDamage(attacker, target, damage, log);
            // 额外伤害（仅当额外伤害大于0时生效）
            float extraDmg = Math.Max(0, attacker.ExtraDamage);
            damage *= (1 + extraDmg);

            // 最终伤害保底已在基础伤害中处理，这里直接返回
            return new DamageResult((int)Math.Max(0, damage), isCrit);
        }

        /// <summary>
        /// 获取经过Buff修正后的攻击力
        /// </summary>
        public static int GetModifiedAttack(BattleFighter fighter, int baseAtk)
        {
            int modifiedAtk = baseAtk;
            foreach (var buff in fighter.ActiveBuffs)
            {
                foreach (var effect in buff.Template.Effects)
                {
                    if (effect.EffectType == BuffEffectType.AttackUp)
                    {
                        if (effect.IsPercentage)
                            modifiedAtk = (int)(modifiedAtk * (1 + effect.Value * buff.CurrentStack));
                        else
                            modifiedAtk += (int)(effect.Value * buff.CurrentStack);
                    }
                    else if (effect.EffectType == BuffEffectType.AttackDown)
                    {
                        if (effect.IsPercentage)
                            modifiedAtk = (int)(modifiedAtk * (1 - effect.Value * buff.CurrentStack));
                        else
                            modifiedAtk -= (int)(effect.Value * buff.CurrentStack);
                    }
                }
            }
            return Math.Max(0, modifiedAtk);
        }

        /// <summary>
        /// 获取经过Buff修正后的防御力
        /// </summary>
        public static int GetModifiedDefense(BattleFighter fighter, int baseDef)
        {
            int modifiedDef = baseDef;
            foreach (var buff in fighter.ActiveBuffs)
            {
                foreach (var effect in buff.Template.Effects)
                {
                    if (effect.EffectType == BuffEffectType.DefenseUp)
                    {
                        if (effect.IsPercentage)
                            modifiedDef = (int)(modifiedDef * (1 + effect.Value * buff.CurrentStack));
                        else
                            modifiedDef += (int)(effect.Value * buff.CurrentStack);
                    }
                    else if (effect.EffectType == BuffEffectType.DefenseDown)
                    {
                        if (effect.IsPercentage)
                            modifiedDef = (int)(modifiedDef * (1 - effect.Value * buff.CurrentStack));
                        else
                            modifiedDef -= (int)(effect.Value * buff.CurrentStack);
                    }
                    else if (effect.EffectType == BuffEffectType.ArmorBreak)
                    {
                        // 破防：直接减少防御值（非百分比）
                        modifiedDef -= (int)(effect.Value * buff.CurrentStack);
                    }
                }
            }
            return Math.Max(0, modifiedDef);
        }

        /// <summary>
        /// 获取经过Buff修正后的暴击率
        /// </summary>
        public static float GetModifiedCritRate(BattleFighter fighter, float baseCritRate)
        {
            float modifiedCritRate = baseCritRate;
            foreach (var buff in fighter.ActiveBuffs)
            {
                foreach (var effect in buff.Template.Effects)
                {
                    if (effect.EffectType == BuffEffectType.CritRateUp)
                    {
                        if (effect.IsPercentage)
                            modifiedCritRate = modifiedCritRate + (float)(effect.Value * buff.CurrentStack);
                        else
                            modifiedCritRate += (float)(effect.Value * buff.CurrentStack);
                    }
                    else if (effect.EffectType == BuffEffectType.CritRateDown)
                    {
                        if (effect.IsPercentage)
                            modifiedCritRate = modifiedCritRate - (float)(effect.Value * buff.CurrentStack);
                        else
                            modifiedCritRate -= (float)(effect.Value * buff.CurrentStack);
                    }
                }
            }
            return Math.Max(0, Math.Min(1.0f, modifiedCritRate)); // 暴击率限制在0-1之间
        }

        /// <summary>
        /// 获取经过Buff修正后的暴击伤害
        /// </summary>
        public static float GetModifiedCritDamage(BattleFighter fighter, float baseCritDamage)
        {
            float modifiedCritDamage = baseCritDamage;
            foreach (var buff in fighter.ActiveBuffs)
            {
                foreach (var effect in buff.Template.Effects)
                {
                    if (effect.EffectType == BuffEffectType.CritDamageUp)
                    {
                        if (effect.IsPercentage)
                            modifiedCritDamage = modifiedCritDamage + (float)(effect.Value * buff.CurrentStack);
                        else
                            modifiedCritDamage += (float)(effect.Value * buff.CurrentStack);
                    }
                    else if (effect.EffectType == BuffEffectType.CritDamageDown)
                    {
                        if (effect.IsPercentage)
                            modifiedCritDamage = modifiedCritDamage - (float)(effect.Value * buff.CurrentStack);
                        else
                            modifiedCritDamage -= (float)(effect.Value * buff.CurrentStack);
                    }
                }
            }
            return Math.Max(1.0f, modifiedCritDamage); // 暴击伤害至少为1.0倍
        }

        /// <summary>
        /// 获取目标的暴击抵抗率
        /// </summary>
        public static float GetCritResist(BattleFighter fighter)
        {
            float critResist = 0;
            foreach (var buff in fighter.ActiveBuffs)
            {
                foreach (var effect in buff.Template.Effects)
                {
                    if (effect.EffectType == BuffEffectType.CritResist)
                    {
                        if (effect.IsPercentage)
                            critResist += (float)(effect.Value * buff.CurrentStack);
                        else
                            critResist += (float)(effect.Value * buff.CurrentStack);
                    }
                }
            }
            return Math.Max(0, Math.Min(1.0f, critResist)); // 暴击抵抗限制在0-1之间
        }

        /// <summary>
        /// 应用Buff对伤害的修正（伤害增幅和伤害减免）
        /// </summary>
        public static double ApplyBuffModifiers(BattleFighter caster, BattleFighter target, double damage)
        {
            double modifiedDamage = damage;

            // 攻击方的伤害增幅
            foreach (var buff in caster.ActiveBuffs)
            {
                foreach (var effect in buff.Template.Effects)
                {
                    if (effect.EffectType == BuffEffectType.DamageAmplify)
                    {
                        if (effect.IsPercentage)
                            modifiedDamage *= (1 + effect.Value * buff.CurrentStack);
                        else
                            modifiedDamage += effect.Value * buff.CurrentStack;
                    }
                }
            }

            // 防御方的伤害减免
            foreach (var buff in target.ActiveBuffs)
            {
                foreach (var effect in buff.Template.Effects)
                {
                    if (effect.EffectType == BuffEffectType.DamageReduction)
                    {
                        if (effect.IsPercentage)
                            modifiedDamage *= (1 - effect.Value * buff.CurrentStack);
                        else
                            modifiedDamage -= effect.Value * buff.CurrentStack;
                    }
                }
            }

            // 易伤效果：防御方受到的伤害增加
            foreach (var buff in target.ActiveBuffs)
            {
                foreach (var effect in buff.Template.Effects)
                {
                    if (effect.EffectType == BuffEffectType.Vulnerability)
                    {
                        if (effect.IsPercentage)
                            modifiedDamage *= (1 + effect.Value * buff.CurrentStack);
                        else
                            modifiedDamage += effect.Value * buff.CurrentStack;
                    }
                }
            }

            return Math.Max(0, modifiedDamage); // 最终伤害至少为0（保底已在基础计算中处理）
        }

        /// <summary>
        /// 应用吸血效果
        /// </summary>
        public static void ApplyLifesteal(BattleFighter fighter, int damage, BattleContext context, RoundLog log)
        {
            float lifestealRate = 0;
            foreach (var buff in fighter.ActiveBuffs)
            {
                foreach (var effect in buff.Template.Effects)
                {
                    if (effect.EffectType == BuffEffectType.Lifesteal)
                    {
                        lifestealRate += (float)(effect.Value * buff.CurrentStack);
                    }
                }
            }

            if (lifestealRate > 0)
            {
                int healAmount = (int)(damage * lifestealRate);
                int overheal = Math.Max(0, fighter.CurrentHp + healAmount - fighter.MaxHp);
                fighter.CurrentHp = Math.Min(fighter.MaxHp, fighter.CurrentHp + healAmount);

                new BattleLogBuilder(log, context, BattleLogType.Lifesteal)
                    .Target(fighter)
                    .Value(healAmount)
                    .Description($"{fighter.Name} 通过吸血恢复了 {healAmount} 点生命。")
                    .Build();

                // 记录吸血量
                BattleRecorder.RecordLifesteal(context, fighter, healAmount);
            }
        }

        /// <summary>
        /// 获取命中率修正
        /// </summary>
        public static float GetModifiedAccuracy(BattleFighter fighter, float baseAccuracy)
        {
            float modifiedAccuracy = baseAccuracy;
            foreach (var buff in fighter.ActiveBuffs)
            {
                foreach (var effect in buff.Template.Effects)
                {
                    if (effect.EffectType == BuffEffectType.AccuracyUp)
                    {
                        modifiedAccuracy += (float)(effect.Value * buff.CurrentStack);
                    }
                    else if (effect.EffectType == BuffEffectType.AccuracyDown)
                    {
                        modifiedAccuracy -= (float)(effect.Value * buff.CurrentStack);
                    }
                }
            }
            return Math.Max(0, Math.Min(1.0f, modifiedAccuracy));
        }

        /// <summary>
        /// 应用护盾吸收伤害
        /// </summary>
        public static int ApplyShieldDamage(BattleFighter target, int damage, RoundLog log, BattleContext? context = null)
        {
            int remainingDamage = damage;
            int totalAbsorbed = 0;

            foreach (var buff in target.ActiveBuffs.ToList())
            {
                if (remainingDamage <= 0) break;

                foreach (var effect in buff.Template.Effects)
                {
                    if (effect.EffectType == BuffEffectType.Shield && remainingDamage > 0)
                    {
                        // 初始化护盾值
                        if (buff.ShieldValue == 0)
                        {
                            buff.ShieldValue = effect.Value * buff.CurrentStack;
                        }

                        if (buff.ShieldValue > 0)
                        {
                            int absorbed = (int)Math.Min(buff.ShieldValue, remainingDamage);
                            remainingDamage -= absorbed;
                            buff.ShieldValue -= absorbed;
                            totalAbsorbed += absorbed;

                            new BattleLogBuilder(log, context, BattleLogType.ShieldAbsorb)
                                .Target(target)
                                .Buff(buff.Template)
                                .Value(absorbed)
                                .Description($"{target.Name} 的护盾吸收了 {absorbed} 点伤害。")
                                .Build();

                            if (buff.ShieldValue <= 0)
                            {
                                target.ActiveBuffs.Remove(buff);
                                new BattleLogBuilder(log, context, BattleLogType.BuffRemove)
                                    .Target(target)
                                    .Buff(buff.Template)
                                    .Description($"{target.Name} 的护盾被击破了。")
                                    .Build();
                                break;
                            }
                        }
                    }
                }
            }

            return Math.Max(0, remainingDamage);
        }

        /// <summary>
        /// 应用护盾吸收伤害（带上下文记录）
        /// </summary>
        public static int ApplyShieldDamage(BattleFighter target, int damage, BattleContext context, RoundLog log)
        {
            int remainingDamage = ApplyShieldDamage(target, damage, log, context);
            int absorbed = damage - remainingDamage;
            if (absorbed > 0)
            {
                BattleRecorder.RecordShieldAbsorbed(context, target, absorbed);
            }
            return remainingDamage;
        }

        /// <summary>
        /// 应用庇护效果：让有庇护Buff的友方分担伤害
        /// </summary>
        public static int ApplySanctuaryDamage(BattleFighter target, int damage, BattleContext context, RoundLog log)
        {
            int remainingDamage = damage;

            // 获取所有友方单位
            var allies = target.IsPlayerSide
                ? context.PlayerSide.Where(f => f.CurrentHp > 0 && f.Id != target.Id).ToList()
                : context.EnemySide.Where(f => f.CurrentHp > 0 && f.Id != target.Id).ToList();

            foreach (var ally in allies)
            {
                if (remainingDamage <= 0) break;

                // 检查是否有庇护Buff
                foreach (var buff in ally.ActiveBuffs)
                {
                    foreach (var effect in buff.Template.Effects)
                    {
                        if (effect.EffectType == BuffEffectType.Sanctuary)
                        {
                            // effect.Value 是分担比例（百分比）
                            float shareRatio = effect.IsPercentage ? (float)effect.Value * buff.CurrentStack : 0.3f;
                            int shareDamage = (int)(remainingDamage * shareRatio);

                            if (shareDamage > 0)
                            {
                                // 庇护者受到分担的伤害
                                ally.CurrentHp -= shareDamage;
                                remainingDamage -= shareDamage;

                                new BattleLogBuilder(log, context, BattleLogType.SanctuaryShare)
                                    .Target(ally)
                                    .Value(shareDamage)
                                    .Description($"{ally.Name} 为 {target.Name} 分担了 {shareDamage} 点伤害。")
                                    .Build();

                                // 记录伤害
                                BattleRecorder.RecordDamageTaken(context, ally, shareDamage);

                                // 检查庇护者是否死亡，使用统一致死结算
                                if (ally.CurrentHp <= 0)
                                {
                                    if (!BuffProcessor.HasUndying(ally) && !BuffProcessor.HasAutoRevive(ally))
                                    {
                                        new BattleLogBuilder(log, context, BattleLogType.Death)
                                            .Target(ally)
                                            .Description($"{ally.Name} 因分担伤害而倒下。")
                                            .Build();
                                    }
                                    // 使用统一致死结算（庇护者死亡，攻击者为null）
                                    BattleHelper.ResolveFatalDamage(ally, null, context, log, suppressDeathLog: true);
                                }
                            }

                            break; // 每个单位只触发一个庇护效果
                        }
                    }
                }
            }

            return Math.Max(0, remainingDamage);
        }

        /// <summary>
        /// 应用反伤效果
        /// </summary>
        public static void ApplyReflectDamage(BattleFighter target, BattleFighter attacker, int damage, BattleContext context, RoundLog log)
        {
            float reflectRate = 0;
            foreach (var buff in target.ActiveBuffs)
            {
                foreach (var effect in buff.Template.Effects)
                {
                    if (effect.EffectType == BuffEffectType.ReflectDamage)
                    {
                        reflectRate += (float)(effect.Value * buff.CurrentStack);
                    }
                }
            }

            if (reflectRate > 0)
            {
                int reflectDamage = (int)(damage * reflectRate);
                attacker.CurrentHp -= reflectDamage;

                new BattleLogBuilder(log, context, BattleLogType.ReflectDamage)
                    .Caster(target)
                    .Target(attacker)
                    .Value(reflectDamage)
                    .Description($"{target.Name} 反弹了 {reflectDamage} 点伤害给 {attacker.Name}。")
                    .Build();

                // 记录反伤伤害
                BattleRecorder.RecordReflectDamage(context, target, reflectDamage);
                BattleRecorder.RecordDamageTaken(context, attacker, reflectDamage);

                // 检查攻击者是否因反伤死亡，使用统一致死结算
                if (attacker.CurrentHp <= 0)
                {
                    if (!BuffProcessor.HasUndying(attacker) && !BuffProcessor.HasAutoRevive(attacker))
                    {
                        new BattleLogBuilder(log, context, BattleLogType.Death)
                            .Caster(target)
                            .Target(attacker)
                            .Description($"{attacker.Name} 因反伤而倒下。")
                            .Build();
                    }
                    BattleHelper.ResolveFatalDamage(attacker, target, context, log, suppressDeathLog: true);
                }
            }
        }

        /// <summary>
        /// 获取经过Buff修正后的连击率
        /// </summary>
        public static float GetModifiedComboRate(BattleFighter fighter)
        {
            float modifiedComboRate = fighter.ComboRate;
            foreach (var buff in fighter.ActiveBuffs)
            {
                foreach (var effect in buff.Template.Effects)
                {
                    if (effect.EffectType == BuffEffectType.ComboRateUp)
                    {
                        if (effect.IsPercentage)
                            modifiedComboRate = modifiedComboRate + (float)(effect.Value * buff.CurrentStack);
                        else
                            modifiedComboRate += (float)(effect.Value * buff.CurrentStack);
                    }
                    else if (effect.EffectType == BuffEffectType.ComboRateDown)
                    {
                        if (effect.IsPercentage)
                            modifiedComboRate = modifiedComboRate - (float)(effect.Value * buff.CurrentStack);
                        else
                            modifiedComboRate -= (float)(effect.Value * buff.CurrentStack);
                    }
                }
            }
            return Math.Max(0, Math.Min(1.0f, modifiedComboRate)); // 连击率限制在0-1之间
        }

        /// <summary>
        /// 应用连击伤害修正
        /// </summary>
        public static int ApplyComboDamageModifier(BattleFighter fighter, int damage)
        {
            float comboDamageModifier = 1.0f;
            foreach (var buff in fighter.ActiveBuffs)
            {
                foreach (var effect in buff.Template.Effects)
                {
                    if (effect.EffectType == BuffEffectType.ComboDamageUp)
                    {
                        if (effect.IsPercentage)
                            comboDamageModifier *= (float)(1 + effect.Value * buff.CurrentStack);
                        else
                            comboDamageModifier += (float)(effect.Value * buff.CurrentStack);
                    }
                    else if (effect.EffectType == BuffEffectType.ComboDamageDown)
                    {
                        if (effect.IsPercentage)
                            comboDamageModifier *= (float)(1 - effect.Value * buff.CurrentStack);
                        else
                            comboDamageModifier -= (float)(effect.Value * buff.CurrentStack);
                    }
                }
            }
            return (int)(damage * comboDamageModifier);
        }

        /// <summary>
        /// 获取经过Buff修正后的反击率
        /// </summary>
        public static float GetModifiedCounterRate(BattleFighter fighter)
        {
            float modifiedCounterRate = fighter.CounterRate;
            foreach (var buff in fighter.ActiveBuffs)
            {
                foreach (var effect in buff.Template.Effects)
                {
                    if (effect.EffectType == BuffEffectType.CounterRateUp)
                    {
                        if (effect.IsPercentage)
                            modifiedCounterRate = modifiedCounterRate + (float)(effect.Value * buff.CurrentStack);
                        else
                            modifiedCounterRate += (float)(effect.Value * buff.CurrentStack);
                    }
                    else if (effect.EffectType == BuffEffectType.CounterRateDown)
                    {
                        if (effect.IsPercentage)
                            modifiedCounterRate = modifiedCounterRate - (float)(effect.Value * buff.CurrentStack);
                        else
                            modifiedCounterRate -= (float)(effect.Value * buff.CurrentStack);
                    }
                }
            }
            return Math.Max(0, Math.Min(1.0f, modifiedCounterRate)); // 反击率限制在0-1之间
        }

        /// <summary>
        /// 应用反击伤害修正
        /// </summary>
        public static int ApplyCounterDamageModifier(BattleFighter fighter, int damage)
        {
            float counterDamageModifier = 1.0f;
            foreach (var buff in fighter.ActiveBuffs)
            {
                foreach (var effect in buff.Template.Effects)
                {
                    if (effect.EffectType == BuffEffectType.CounterDamageUp)
                    {
                        if (effect.IsPercentage)
                            counterDamageModifier *= (float)(1 + effect.Value * buff.CurrentStack);
                        else
                            counterDamageModifier += (float)(effect.Value * buff.CurrentStack);
                    }
                    else if (effect.EffectType == BuffEffectType.CounterDamageDown)
                    {
                        if (effect.IsPercentage)
                            counterDamageModifier *= (float)(1 - effect.Value * buff.CurrentStack);
                        else
                            counterDamageModifier -= (float)(effect.Value * buff.CurrentStack);
                    }
                }
            }
            return (int)(damage * counterDamageModifier);
        }

        /// <summary>
        /// 应用斩杀效果：对低血量目标造成额外伤害
        /// </summary>
        public static double ApplyExecuteDamage(BattleFighter caster, BattleFighter target, double damage, RoundLog log, BattleContext? context = null)
        {
            double modifiedDamage = damage;
            float executeThreshold = 0.3f; // 默认30%血量以下触发斩杀
            float executeBonus = 0.5f; // 默认额外50%伤害
            bool zhanxs = false; // 默认额外50%伤害
            foreach (var buff in caster.ActiveBuffs)
            {
                foreach (var effect in buff.Template.Effects)
                {
                    if (effect.EffectType == BuffEffectType.Execute)
                    {
                        zhanxs = true;
                        // effect.Value 存储斩杀阈值（百分比）
                        // buff.Template.Effects 可以有多个值，这里简化处理
                        // 假设 effect.Value 是额外伤害百分比，effect.IsPercentage 表示是否使用默认阈值
                        if (effect.IsPercentage)
                        {
                            executeBonus += (float)effect.Value * buff.CurrentStack;
                        }
                        else
                        {
                            executeBonus += (float)effect.Value * buff.CurrentStack;
                        }
                    }
                }
            }

            // 检查目标血量是否低于阈值
            if (target.MaxHp > 0 && zhanxs)
            {
                float hpPercent = (float)target.CurrentHp / target.MaxHp;
                if (hpPercent < executeThreshold)
                {
                    double bonusDamage = modifiedDamage * executeBonus;
                    modifiedDamage += bonusDamage;
                    new BattleLogBuilder(log, context, BattleLogType.Execute)
                        .Caster(caster)
                        .Target(target)
                        .Value((int)bonusDamage)
                        .Description($"{caster.Name} 触发斩杀，对 {target.Name} 追加造成 {(int)bonusDamage} 点伤害。")
                        .Build();
                }
            }

            return modifiedDamage;
        }

        /// <summary>
        /// 获取经过Buff修正后的速度
        /// </summary>
        public static int GetModifiedSpeed(BattleFighter fighter, int baseSpeed)
        {
            int modifiedSpeed = baseSpeed;
            foreach (var buff in fighter.ActiveBuffs)
            {
                foreach (var effect in buff.Template.Effects)
                {
                    if (effect.EffectType == BuffEffectType.Haste)
                    {
                        if (effect.IsPercentage)
                            modifiedSpeed = (int)(modifiedSpeed * (1 + effect.Value * buff.CurrentStack));
                        else
                            modifiedSpeed += (int)(effect.Value * buff.CurrentStack);
                    }
                    else if (effect.EffectType == BuffEffectType.Slow)
                    {
                        if (effect.IsPercentage)
                            modifiedSpeed = (int)(modifiedSpeed * (1 - effect.Value * buff.CurrentStack));
                        else
                            modifiedSpeed -= (int)(effect.Value * buff.CurrentStack);
                    }
                }
            }
            return Math.Max(0, modifiedSpeed);
        }
    }
}
