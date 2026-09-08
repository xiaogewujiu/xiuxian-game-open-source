using XXX.Entity;

namespace XXX.Battle
{
    /// <summary>
    /// 战斗AI
    /// 负责智能决策
    /// </summary>
    public static class BattleAI
    {
        /// <summary>
        /// 计算技能优先级评分（改进版本 - 考虑蓝量消耗和冷却时间）
        /// </summary>
        public static float CalculateSkillPriority(XXX.Entity.Skill skill, BattleFighter caster,
            List<BattleFighter> enemies, List<BattleFighter> allies)
        {
            float priority = 0;

            // 复活术特殊处理
            if (skill.DamageType == DamageType.Revive)
            {
                int deadCount = allies.Count(a => a.CurrentHp <= 0);
                if (deadCount > 0)
                    priority += 150; // 有死亡友方时，复活术最高优先级
                else
                    priority -= 100; // 没有死亡友方时，不使用
                return priority;
            }

            // 1. 治疗技能评估
            if (skill.DamageType == DamageType.Heal)
            {
                var aliveAllies = allies.Where(a => a.CurrentHp > 0).ToList();
                if (aliveAllies.Count > 0)
                {
                    float minHpPercent = aliveAllies.Min(a => (float)a.CurrentHp / a.MaxHp);
                    if (minHpPercent < 0.3f)
                        priority += 100; // 有人快死了，治疗最优先
                    else if (minHpPercent < 0.5f)
                        priority += 60;
                    else if (minHpPercent < 0.7f)
                        priority += 30;
                    else
                        priority -= 50; // 血量健康，不需要治疗
                }
            }
            // 2. 蓝量恢复技能评估
            else if (skill.DamageType == DamageType.ManaRestore)
            {
                var aliveAllies = allies.Where(a => a.CurrentHp > 0).ToList();
                if (aliveAllies.Count > 0)
                {
                    float minMpPercent = aliveAllies.Min(a => (float)a.CurrentMp / a.MaxMp);
                    if (minMpPercent < 0.2f)
                        priority += 80; // 有人快没蓝了，恢复蓝量优先
                    else if (minMpPercent < 0.4f)
                        priority += 50;
                    else if (minMpPercent < 0.6f)
                        priority += 20;
                    else
                        priority -= 40; // 蓝量充足，不需要恢复
                }
            }
            // 3. 纯Buff技能评估
            else if (skill.DamageType == DamageType.Buff)
            {
                // Buff技能基础优先级
                priority += 40;

                // 根据Buff价值调整优先级
                if (skill.BuffIds != null && skill.BuffIds.Count > 0)
                {
                    priority += EvaluateBuffValue(skill.BuffIds, enemies, allies);
                }
            }
            else
            {
                // 4. 伤害技能评估
                priority += (float)skill.DamageMultiplier * 20;
                priority += skill.HitCount * 10;

                // 5. AOE技能在敌人多时优先
                if (skill.RangeType == 0 && enemies.Count >= 3)
                    priority += 40;
            }

            // 6. Buff技能评估（伤害/治疗技能也可能携带Buff）
            if (skill.DamageType != DamageType.Buff && skill.BuffIds != null && skill.BuffIds.Count > 0)
            {
                priority += EvaluateBuffValue(skill.BuffIds, enemies, allies) * 0.5f; // 附带Buff价值减半
            }

            // 7. 蓝量消耗考虑（相对值惩罚）
            if (caster.MaxMp > 0)
            {
                float mpCostRatio = (float)skill.ManaCost / caster.MaxMp;
                if (mpCostRatio > 0.5f) // 消耗超过50%最大蓝量
                    priority -= 30;
                else if (mpCostRatio > 0.3f) // 消耗超过30%最大蓝量
                    priority -= 15;
                else if (mpCostRatio > 0.1f) // 消耗超过10%最大蓝量
                    priority -= 5;
            }

            // 8. 冷却时间考虑（相对值惩罚）
            if (skill.Cooldown > 0)
            {
                float cooldownRatio = (float)skill.Cooldown / 5; // 假设5回合为长冷却
                if (cooldownRatio > 1.0f) // 冷却超过5回合
                    priority -= 20;
                else if (cooldownRatio > 0.6f) // 冷却超过3回合
                    priority -= 10;
                else if (cooldownRatio > 0.2f) // 冷却超过1回合
                    priority -= 5;
            }

            // 9. 自身状态考虑
            float selfHpPercent = (float)caster.CurrentHp / caster.MaxHp;
            if (selfHpPercent < 0.3f)
            {
                // 自身残血时，治疗技能优先级大幅提升
                if (skill.DamageType == DamageType.Heal)
                    priority += 50;
            }

            return priority;
        }

        /// <summary>
        /// 评估Buff的价值
        /// </summary>
        public static float EvaluateBuffValue(List<string> buffIds, List<BattleFighter> enemies, List<BattleFighter> allies)
        {
            float value = 0;

            foreach (var buffId in buffIds)
            {
                if (!BuffDataTemplates.BuffTemplates.ContainsKey(buffId)) continue;
                var template = BuffDataTemplates.BuffTemplates[buffId];

                foreach (var effect in template.Effects)
                {
                    switch (effect.EffectType)
                    {
                        case BuffEffectType.Stun:
                        case BuffEffectType.Silence:
                            value += 50; // 控制技能高价值
                            break;
                        case BuffEffectType.Taunt:
                            value += 45; // 嘲讽高价值
                            break;
                        case BuffEffectType.Immunity:
                            value += 40; // 免疫控制高价值
                            break;
                        case BuffEffectType.AttackUp:
                        case BuffEffectType.DefenseUp:
                            value += 30; // 增益Buff
                            break;
                        case BuffEffectType.AttackDown:
                        case BuffEffectType.DefenseDown:
                            value += 35; // 减益Buff
                            break;
                        case BuffEffectType.Shield:
                            value += 35; // 护盾高价值
                            break;
                        case BuffEffectType.ReflectDamage:
                            value += 30; // 反伤中等价值
                            break;
                        case BuffEffectType.Lifesteal:
                            value += 28; // 吸血中等价值
                            break;
                        case BuffEffectType.DamageOverTime:
                            value += 25; // 持续伤害
                            break;
                        case BuffEffectType.AccuracyDown:
                            value += 25; // 降低命中
                            break;
                        case BuffEffectType.AccuracyUp:
                            value += 20; // 提升命中
                            break;
                        case BuffEffectType.ManaDrain:
                            value += 20; // 蓝量消耗
                            break;
                    }
                }
            }

            return value;
        }
    }
}
