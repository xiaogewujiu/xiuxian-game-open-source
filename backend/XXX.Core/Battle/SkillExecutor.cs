using XXX.Entity;

namespace XXX.Battle
{
    /// <summary>
    /// 技能释放模式
    /// </summary>
    public enum SkillReleaseMode
    {
        /// <summary>智能模式：根据优先级选择最优技能</summary>
        Smart,
        /// <summary>随机模式：从可用技能中随机选择</summary>
        Random,
        /// <summary>顺序模式：按照技能列表顺序依次释放</summary>
        Sequential
    }

    public enum SkillLogContext
    {
        Normal,
        Combo,
        Counter
    }

    /// <summary>
    /// 技能执行器
    /// 负责技能的使用和执行
    /// </summary>
    public static class SkillExecutor
    {
        /// <summary>
        /// 随机数生成器（线程安全）
        /// </summary>
        private static Random Rand = Random.Shared;

        /// <summary>
        /// 尝试使用技能（默认智能模式）
        /// </summary>
        public static bool TryUseSkill(BattleContext context, BattleFighter fighter,
            List<BattleFighter> enemies, List<BattleFighter> allies, RoundLog log,
            SkillReleaseMode mode = SkillReleaseMode.Smart)
        {
            // 被沉默无法使用技能
            if (BattleHelper.IsSilenced(fighter))
            {
                new BattleLogBuilder(log, context, BattleLogType.Silence)
                    .Target(fighter)
                    .Description($"{fighter.Name} 受到[沉默]影响，无法使用技能。")
                    .Build();
                return false;
            }

            // 根据模式选择技能
            return mode switch
            {
                SkillReleaseMode.Smart => TryUseSkillSmart(context, fighter, enemies, allies, log),
                SkillReleaseMode.Random => TryUseSkillRandom(context, fighter, enemies, allies, log),
                SkillReleaseMode.Sequential => TryUseSkillSequential(context, fighter, enemies, allies, log),
                _ => TryUseSkillSmart(context, fighter, enemies, allies, log)
            };
        }

        /// <summary>
        /// 智能模式：选择优先级最高的技能
        /// </summary>
        private static bool TryUseSkillSmart(BattleContext context, BattleFighter fighter,
            List<BattleFighter> enemies, List<BattleFighter> allies, RoundLog log)
        {
            var availableSkills = GetAvailableSkills(fighter, enemies, allies);

            if (availableSkills.Count == 0)
                return false;

            // 选择优先级最高的技能
            var bestSkill = availableSkills
                .OrderByDescending(s => s.priority)
                .First().skill;
            UseSkill(context, fighter, bestSkill, enemies, allies, log);
            return true;
        }

        /// <summary>
        /// 随机模式：从可用技能中随机选择
        /// </summary>
        private static bool TryUseSkillRandom(BattleContext context, BattleFighter fighter,
            List<BattleFighter> enemies, List<BattleFighter> allies, RoundLog log)
        {
            var availableSkills = GetAvailableSkills(fighter, enemies, allies);

            if (availableSkills.Count == 0)
                return false;

            // 随机选择一个技能
            int index = Rand.Next(availableSkills.Count);
            var randomSkill = availableSkills[index].skill;
            UseSkill(context, fighter, randomSkill, enemies, allies, log);
            return true;
        }

        /// <summary>
        /// 顺序模式：按照技能列表顺序依次释放
        /// </summary>
        private static bool TryUseSkillSequential(BattleContext context, BattleFighter fighter,
            List<BattleFighter> enemies, List<BattleFighter> allies, RoundLog log)
        {
            // 按照 SkillIds 列表顺序遍历，找到第一个可用的技能
            foreach (var skillIdStr in fighter.SkillIds)
            {
                if (!int.TryParse(skillIdStr, out int skillId)) continue;
                if (!SkillData.Skills.ContainsKey(skillId)) continue;

                var skill = SkillData.Skills[skillId];

                // 检查技能是否可用
                if (!IsSkillAvailable(fighter, skill)) continue;

                // 检查触发概率
                if (Rand.NextDouble() > skill.TriggerChance)
                    continue;

                UseSkill(context, fighter, skill, enemies, allies, log);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 获取所有可用技能及其优先级
        /// </summary>
        private static List<(XXX.Entity.Skill skill, float priority)> GetAvailableSkills(
            BattleFighter fighter, List<BattleFighter> enemies, List<BattleFighter> allies)
        {
            var availableSkills = new List<(XXX.Entity.Skill skill, float priority)>();

            foreach (var skillIdStr in fighter.SkillIds)
            {
                if (!int.TryParse(skillIdStr, out int skillId)) continue;
                if (!SkillData.Skills.ContainsKey(skillId)) continue;

                var skill = SkillData.Skills[skillId];

                // 检查技能是否可用
                if (!IsSkillAvailable(fighter, skill)) continue;

                // 检查触发概率
                if (Rand.NextDouble() > skill.TriggerChance)
                    continue;

                // 计算技能优先级
                float priority = BattleAI.CalculateSkillPriority(skill, fighter, enemies, allies);
                availableSkills.Add((skill, priority));
            }

            return availableSkills;
        }

        /// <summary>
        /// 检查技能是否可用（冷却、蓝量等条件）
        /// </summary>
        private static bool IsSkillAvailable(BattleFighter fighter, XXX.Entity.Skill skill)
        {
            // 检查冷却
            if (fighter.SkillCooldowns.ContainsKey(skill.Id) && fighter.SkillCooldowns[skill.Id] > 0)
                return false;

            // 检查蓝量
            if (fighter.CurrentMp < skill.ManaCost)
                return false;

            return true;
        }

        /// <summary>
        /// 使用技能
        /// </summary>
        public static void UseSkill(BattleContext context, BattleFighter caster, XXX.Entity.Skill skill,
            List<BattleFighter> enemies, List<BattleFighter> allies, RoundLog log)
        {
            // 先确定目标
            List<BattleFighter> targets = TargetSelector.GetSkillTargets(skill, caster, enemies, allies);

            // 校验目标有效性：若目标为空（如复活术无可复活对象），直接返回，不消耗资源
            if (targets.Count == 0)
            {
                return;
            }

            // 消耗蓝量
            caster.CurrentMp -= skill.ManaCost;

            // 设置冷却
            caster.SkillCooldowns[skill.Id] = skill.Cooldown;
            caster.SkillsUsedThisRound.Add(skill.Id); // 标记为本回合使用的技能

            // 记录技能使用次数
            RecordSkillUsage(context, caster, skill.Id);

            // 记录蓝量消耗
            if (skill.ManaCost > 0)
            {
                BattleRecorder.RecordMpConsumed(context, caster, skill.ManaCost);
            }

            // 对每个目标执行技能效果，并收集命中的目标
            var hitTargets = new List<BattleFighter>();
            foreach (var target in targets)
            {
                bool isHit = ExecuteSkillOnTarget(caster, skill, target, context, log, SkillLogContext.Normal);
                if (isHit)
                {
                    hitTargets.Add(target);
                }

                // 技能命中后，目标可以反击（分身不触发也不被反击）
                if (target.CurrentHp > 0 && !caster.IsMirror && !target.IsMirror)
                {
                    ComboCounterExecutor.TryCounterAttack(target, caster, context, log);
                }
            }

            // 技能执行完毕后，应用Buff（只传入技能实际命中的目标）
            if (hitTargets.Count > 0)
            {
                BuffProcessor.ApplySkillBuffs(caster, skill, hitTargets, context, log);
            }
        }

        /// <summary>
        /// 记录技能使用次数
        /// </summary>
        public static void RecordSkillUsage(BattleContext context, BattleFighter caster, int skillId)
        {
            // 获取或创建战斗单位的技能统计
            if (!context.Result.FighterSkillStats.ContainsKey(caster.Id))
            {
                context.Result.FighterSkillStats[caster.Id] = new FighterSkillStats
                {
                    GID = caster.Id,
                    Name = caster.Name
                };
            }

            var fighterStats = context.Result.FighterSkillStats[caster.Id];

            // 记录技能使用次数
            if (!fighterStats.SkillUsageCount.ContainsKey(skillId))
            {
                fighterStats.SkillUsageCount[skillId] = 0;
            }
            fighterStats.SkillUsageCount[skillId]++;
        }

        /// <summary>
        /// 对目标执行技能效果（支持多段伤害）
        /// 返回值：true表示至少命中一次或技能效果生效，false表示全部闪避
        /// </summary>
        public static bool ExecuteSkillOnTarget(BattleFighter caster, XXX.Entity.Skill skill,
            BattleFighter target, BattleContext context, RoundLog log, SkillLogContext logContext = SkillLogContext.Normal)
        {
            // 血祭技能特殊处理：消耗30%生命值（保留至少1点生命）
            if (skill.DamageType == DamageType.BloodSacrifice)
            {
                int hpCost = (int)(caster.MaxHp * 0.3);
                // 确保施法者不会因血祭而死亡，至少保留1点生命
                int maxAllowedCost = caster.CurrentHp - 1;
                if (maxAllowedCost > 0)
                {
                    hpCost = Math.Min(hpCost, maxAllowedCost);
                    caster.CurrentHp -= hpCost;

                    // 记录生命消耗
                    BattleRecorder.RecordHpConsumed(context, caster, hpCost);
                }
                else
                {
                    return false;
                }
            }

            // 纯Buff技能不执行直接效果
            if (skill.DamageType == DamageType.Buff)
            {
                return true; // 纯Buff技能算命中
            }

            // 驱散技能：移除目标的有益Buff
            if (skill.DamageType == DamageType.Dispel)
            {
                int dispelCount = skill.HitCount > 0 ? skill.HitCount : 1; // 默认驱散1个
                int actualDispel = BuffProcessor.DispelBuffs(target, dispelCount, context, log, caster, skill);
                return actualDispel > 0;
            }

            // 净化技能：移除目标的有害Buff
            if (skill.DamageType == DamageType.Cleanse)
            {
                int cleanseCount = skill.HitCount > 0 ? skill.HitCount : 1; // 默认净化1个
                int actualCleanse = BuffProcessor.CleanseBuffs(target, cleanseCount, context, log, caster, skill);
                return actualCleanse > 0;
            }

            // 获取实际的伤害段数
            int hitCount = skill.GetActualHitCount();

            var damageList = new List<int>();
            var isCritList = new List<bool>();
            bool hasHeal = false;
            int totalHealAmount = 0;
            bool hasManaRestore = false;
            int totalManaAmount = 0;
            bool anyHit = false; // 跟踪是否有至少一段命中

            // 创建临时日志收集器
            var tempLog = new RoundLog { RoundNumber = log.RoundNumber };

            for (int hit = 0; hit < hitCount; hit++)
            {
                if (target.CurrentHp <= 0 && skill.DamageType != DamageType.Revive) break;

                // 获取当前段的伤害类型
                var hitDamageType = skill.GetHitDamageType(hit);

                // 处理治疗和复活（复活术本质上也是治疗类型）
                if (hitDamageType == DamageType.Heal || hitDamageType == DamageType.Revive)
                {
                    hasHeal = true;
                    // 复活术特殊处理
                    if (skill.DamageType == DamageType.Revive && target.CurrentHp <= 0)
                    {
                        int reviveAmount = (int)(target.MaxHp * skill.GetHitDamageMultiplier(hit));
                        target.CurrentHp = reviveAmount;
                        totalHealAmount += reviveAmount;

                        // 记录治疗量和受到的治疗
                        BattleRecorder.RecordHealingDone(context, caster, reviveAmount);
                        BattleRecorder.RecordHealingReceived(context, target, reviveAmount);
                    }
                    else if (target.CurrentHp > 0)
                    {
                        // 检查禁疗效果
                        if (BuffProcessor.HasHealBlock(target))
                        {
                            new BattleLogBuilder(log, context, BattleLogType.Heal)
                                .Caster(caster)
                                .Target(target)
                                .Skill(skill)
                                .Description($"{BuildSkillActionPrefix(caster, skill, logContext)}治疗 {target.Name}，但{target.Name}受到禁疗影响，无法恢复生命。")
                                .Build();
                        }
                        else
                        {
                            // 治疗技能
                            int healAmount = (int)(caster.MagicAttack * skill.GetHitDamageMultiplier(hit));
                            target.CurrentHp = Math.Min(target.MaxHp, target.CurrentHp + healAmount);
                            totalHealAmount += healAmount;

                            // 记录治疗量和受到的治疗
                            BattleRecorder.RecordHealingDone(context, caster, healAmount);
                            BattleRecorder.RecordHealingReceived(context, target, healAmount);
                        }
                    }
                }
                else if (hitDamageType == DamageType.ManaRestore)
                {
                    hasManaRestore = true;
                    // 恢复蓝量技能
                    int manaAmount = (int)(caster.MagicAttack * skill.GetHitDamageMultiplier(hit));
                    target.CurrentMp = Math.Min(target.MaxMp, target.CurrentMp + manaAmount);
                    totalManaAmount += manaAmount;
                }
                else if (hitDamageType == DamageType.PercentDamage || skill.DamageType == DamageType.PercentDamage)
                {
                    // 百分比伤害：基于目标最大生命值，无视防御，不可暴击
                    double percent = skill.GetHitDamageMultiplier(hit);
                    int damage = (int)(target.MaxHp * percent);

                    // 应用上限：不超过施法者攻击力的300%（避免后期过强）
                    int maxDamageCap = Math.Max(caster.PhysicalAttack, caster.MagicAttack) * 3;
                    if (damage > maxDamageCap)
                    {
                        damage = maxDamageCap;
                    }

                    // 百分比伤害无视防御，直接应用
                    if (damage > 0)
                    {
                        anyHit = true;

                        // 护盾吸收（百分比伤害可以被护盾吸收）
                        int afterShieldDamage = DamageCalculator.ApplyShieldDamage(target, damage, context, tempLog);
                        // 庇护分担
                        int actualDamage = DamageCalculator.ApplySanctuaryDamage(target, afterShieldDamage, context, tempLog);
                        target.CurrentHp -= actualDamage;

                        // 格式化日志
                        new BattleLogBuilder(tempLog, context, BattleLogType.DamageDealt)
                            .Caster(caster)
                            .Target(target)
                            .Skill(skill)
                            .Value(actualDamage)
                            .Description($"{BuildSkillActionPrefix(caster, skill, logContext)}对 {target.Name} 造成 {actualDamage} 点伤害（百分比伤害），剩余气血 {Math.Max(0, target.CurrentHp)}。")
                            .Build();

                        // 记录伤害
                        BattleRecorder.RecordDamageDealt(context, caster, actualDamage);
                        BattleRecorder.RecordDamageTaken(context, target, actualDamage);
                        BattleRecorder.RecordSkillDamage(context, caster, skill.Id, actualDamage);

                        damageList.Add(actualDamage);
                        isCritList.Add(false); // 百分比伤害不可暴击

                        // 吸血效果
                        DamageCalculator.ApplyLifesteal(caster, actualDamage, context, tempLog);

                        // 反伤效果
                        if (actualDamage > 0)
                        {
                            DamageCalculator.ApplyReflectDamage(target, caster, actualDamage, context, tempLog);
                        }

                        // 检查死亡
                        if (target.CurrentHp <= 0)
                        {
                            BattleHelper.ResolveFatalDamage(target, caster, context, tempLog);
                        }
                    }
                }
                else
                {
                    // 伤害技能 - 使用临时日志避免重复记录
                    DamageResult damageResult = CalculateHitDamage(caster, skill, target, hit, tempLog, context, logContext);
                    int damage = damageResult.Damage;
                    if (damage > 0)
                    {
                        anyHit = true; // 标记为命中
                        // 护盾吸收
                        int afterShieldDamage = DamageCalculator.ApplyShieldDamage(target, damage, context, tempLog);
                        // 庇护分担
                        int actualDamage = DamageCalculator.ApplySanctuaryDamage(target, afterShieldDamage, context, tempLog);
                        target.CurrentHp -= actualDamage;

                        // 记录伤害输出和受到的伤害
                        BattleRecorder.RecordDamageDealt(context, caster, actualDamage);
                        BattleRecorder.RecordDamageTaken(context, target, actualDamage);

                        // 记录技能伤害
                        BattleRecorder.RecordSkillDamage(context, caster, skill.Id, actualDamage);

                        // 收集数据用于合并日志
                        damageList.Add(actualDamage);

                        // 判断是否暴击
                        isCritList.Add(damageResult.IsCrit);

                        // 生命转换技能特殊处理：治疗自己
                        if (skill.DamageType == DamageType.LifeConversion)
                        {
                            int healAmount = (int)(actualDamage * 0.5);
                            caster.CurrentHp = Math.Min(caster.MaxHp, caster.CurrentHp + healAmount);

                            // 记录治疗量和受到的治疗
                            BattleRecorder.RecordHealingDone(context, caster, healAmount);
                            BattleRecorder.RecordHealingReceived(context, caster, healAmount);
                        }

                        // 吸血效果
                        DamageCalculator.ApplyLifesteal(caster, actualDamage, context, tempLog);

                        // 反伤效果
                        if (actualDamage > 0)
                        {
                            DamageCalculator.ApplyReflectDamage(target, caster, actualDamage, context, tempLog);
                        }

                        // 检查目标是否死亡，记录击杀和死亡
                        if (target.CurrentHp <= 0)
                        {
                            BattleHelper.ResolveFatalDamage(target, caster, context, tempLog);
                        }
                    }
                    else
                    {
                        // 闪避了
                        damageList.Add(0);
                        isCritList.Add(false);
                    }
                }
            }

            // 统一输出合并日志
            if (hasHeal)
            {
                anyHit = true; // 治疗算命中
                if (skill.DamageType == DamageType.Revive && target.CurrentHp > 0)
                {
                    new BattleLogBuilder(log, context, BattleLogType.Revive)
                        .Caster(caster)
                        .Target(target)
                        .Skill(skill)
                        .Value(totalHealAmount)
                        .Description($"{BuildSkillActionPrefix(caster, skill, logContext)}复活了 {target.Name}，并恢复了 {totalHealAmount} 点生命。")
                        .Build();
                }
                else if (target.CurrentHp > 0)
                {
                    if (totalHealAmount > 0)
                    {
                        new BattleLogBuilder(log, context, BattleLogType.Heal)
                            .Caster(caster)
                            .Target(target)
                            .Skill(skill)
                            .Value(totalHealAmount)
                            .Description($"{BuildSkillActionPrefix(caster, skill, logContext)}使 {target.Name} 恢复了 {totalHealAmount} 点生命。")
                            .Build();
                    }
                }
            }
            else if (hasManaRestore)
            {
                anyHit = true; // 回蓝算命中
                new BattleLogBuilder(log, context, BattleLogType.Heal)
                    .Caster(caster)
                    .Target(target)
                    .Skill(skill)
                    .Value(totalManaAmount)
                    .Description($"{BuildSkillActionPrefix(caster, skill, logContext)}使 {target.Name} 恢复了 {totalManaAmount} 点灵力。")
                    .Build();
            }
            else if (damageList.Count > 0)
            {
                bool hasInvincibleEntry = tempLog.Entries.Any(entry =>
                    entry.Type == BattleLogType.DamageTaken &&
                    entry.Description.Contains("无敌状态", StringComparison.Ordinal));

                if (damageList.All(d => d == 0))
                {
                    if (!hasInvincibleEntry)
                    {
                        // 全部闪避
                        new BattleLogBuilder(log, context, BattleLogType.Dodge)
                            .Caster(caster)
                            .Target(target)
                        .Skill(skill)
                        .Value(damageList.Count)
                        .Description(damageList.Count > 1
                            ? $"{BuildSkillActionPrefix(caster, skill, logContext)}时，{target.Name} 闪避了全部攻击（{damageList.Count}段）。"
                            : $"{BuildSkillActionPrefix(caster, skill, logContext)}，但被{target.Name}闪避了。")
                        .Build();
                    }
                }
                else
                {
                    // 输出合并的伤害日志
                    int totalDamage = damageList.Sum();

                    // 构建每段伤害的显示字符串
                    List<string> damageParts = [];
                    for (int i = 0; i < damageList.Count; i++)
                    {
                        string part;
                        if (damageList[i] == 0)
                        {
                            part = "0（闪避）";
                        }
                        else
                        {
                            part = damageList[i].ToString();
                            if (isCritList[i])
                            {
                                part += "（暴击）";
                            }
                        }
                        damageParts.Add(part);
                    }

                    string damagesStr = string.Join("、", damageParts);
                    string resultText = damageParts.Count == 1
                        ? $"造成 {damageParts[0]} 点伤害"
                        : $"造成总计 {damagesStr} 点伤害";

                    new BattleLogBuilder(log, context, BattleLogType.DamageDealt)
                        .Caster(caster)
                        .Target(target)
                        .Skill(skill)
                        .Value(totalDamage)
                        .Description($"{BuildSkillActionPrefix(caster, skill, logContext)}对 {target.Name}{resultText}，剩余气血 {Math.Max(0, target.CurrentHp)}。")
                        .Build();
                }
            }

            AppendSupplementalEntries(log, tempLog, context);

            // 返回是否命中
            return anyHit;
        }

        /// <summary>
        /// 判断最近一次伤害是否是暴击
        /// 通过比较暴击率和基础伤害来推断
        /// </summary>
        private static bool WasCritHit(BattleFighter attacker, BattleFighter target)
        {
            float modifiedCritRate = DamageCalculator.GetModifiedCritRate(attacker, attacker.CritRate);
            float targetCritResist = DamageCalculator.GetCritResist(target);
            float finalCritRate = Math.Max(0, modifiedCritRate - targetCritResist);

            // 如果暴击率超过50%，很可能这次是暴击
            // 这只是推断，不是100%准确
            return finalCritRate > 0.5f;
        }

        /// <summary>
        /// 计算单次伤害（支持多段伤害）
        /// </summary>
        private static DamageResult CalculateHitDamage(BattleFighter caster, XXX.Entity.Skill skill,
            BattleFighter target, int hitIndex, RoundLog log, BattleContext? context = null,
            SkillLogContext logContext = SkillLogContext.Normal)
        {
            // 检查无敌效果（在命中判定前检查，确保无敌状态优先于闪避）
            if (BuffProcessor.HasInvincible(target))
            {
                new BattleLogBuilder(log, context, BattleLogType.DamageTaken)
                    .Caster(caster)
                    .Target(target)
                    .Skill(skill)
                    .Description($"{BuildSkillActionPrefix(caster, skill, logContext)}，但未能奏效，{target.Name}处于无敌状态。")
                    .Build();
                return new DamageResult(0, false);
            }

            // 命中判定（命中率至少为0，闪避率至少为0）
            float hitRate = DamageCalculator.GetModifiedAccuracy(caster, Math.Max(0, caster.HitRate));
            float dodgeRate = Math.Max(0, target.DodgeRate);
            float hitChance = Math.Max(0.1f, hitRate - dodgeRate); // 最低10%命中
            if (Rand.NextDouble() > hitChance)
            {
                new BattleLogBuilder(log, context, BattleLogType.Dodge)
                    .Caster(caster)
                    .Target(target)
                    .Skill(skill)
                    .Description($"{BuildSkillActionPrefix(caster, skill, logContext)}，但被{target.Name}闪避了。")
                    .Build();
                // 记录闪避和攻击被闪避
                if (context != null)
                {
                    BattleRecorder.RecordDodge(context, target);
                    BattleRecorder.RecordMissed(context, caster);
                }
                return new DamageResult(0, false);
            }

            // 获取当前段的伤害类型
            var hitDamageType = skill.GetHitDamageType(hitIndex);

            // 根据技能类型选择攻击和防御属性
            int baseAtk, baseDef;
            if (hitDamageType == DamageType.Physical)
            {
                baseAtk = caster.PhysicalAttack;
                baseDef = target.PhysicalDefense;
            }
            else if (hitDamageType == DamageType.Magic)
            {
                baseAtk = caster.MagicAttack;
                baseDef = target.MagicDefense;
            }
            else // True伤害
            {
                baseAtk = Math.Max(caster.PhysicalAttack, caster.MagicAttack);
                baseDef = 0;
            }

            // 应用Buff修正攻击力和防御力
            baseAtk = DamageCalculator.GetModifiedAttack(caster, baseAtk);
            if (hitDamageType != DamageType.True)
                baseDef = DamageCalculator.GetModifiedDefense(target, baseDef);

            // 破甲计算（破甲率至少为0，最高70%）
            float armorBreak = Math.Max(0, Math.Min(0.7f, caster.ArmorBreak));
            int effectiveDef = (int)(baseDef * (1 - armorBreak));

            // 获取当前段的伤害倍率和基础伤害
            double damageMultiplier = skill.GetHitDamageMultiplier(hitIndex);
            int baseDamage = skill.GetHitBaseDamage(hitIndex);

            // 伤害公式：攻击力² / (攻击力 + 防御力) * 伤害倍率 + 基础伤害，避免刮痧同时保证防御价值
            double damage = (baseAtk * baseAtk) / (double)(baseAtk + effectiveDef);
            damage *= damageMultiplier;
            damage += baseDamage;
            // 伤害下限为攻击力的5%，避免刮痧
            double minDamage = baseAtk * 0.05;
            damage = Math.Max(minDamage, damage);

            // 暴击判定（应用Buff修正）
            float modifiedCritRate = DamageCalculator.GetModifiedCritRate(caster, caster.CritRate);
            float targetCritResist = DamageCalculator.GetCritResist(target);
            float finalCritRate = Math.Max(0, modifiedCritRate - targetCritResist); // 暴击率减去目标的暴击抵抗

            bool isCrit = false;
            if (finalCritRate > 0 && Rand.NextDouble() < finalCritRate)
            {
                isCrit = true;
                float modifiedCritDmg = DamageCalculator.GetModifiedCritDamage(caster, Math.Max(1.0f, caster.CritDamage));
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
                    // 属性克制日志已按规范移除，不再展示给玩家
                }
            }

            // Buff加成
            damage = DamageCalculator.ApplyBuffModifiers(caster, target, damage);

            // 额外伤害加成（至少为0）
            float extraDmg = Math.Max(0, caster.ExtraDamage);
            damage *= (1 + extraDmg);

            return new DamageResult((int)Math.Max(1, damage), isCrit);
        }

        /// <summary>
        /// 反击时对目标执行技能效果（伤害减半，支持多段伤害）
        /// </summary>
        public static bool ExecuteCounterSkillOnTarget(BattleFighter caster, XXX.Entity.Skill skill,
            BattleFighter target, BattleContext context, RoundLog log, SkillLogContext logContext = SkillLogContext.Counter)
        {
            // 获取实际的伤害段数
            int hitCount = skill.GetActualHitCount();
            bool anyHit = false;

            for (int hit = 0; hit < hitCount; hit++)
            {
                if (target.CurrentHp <= 0) break;

                // 伤害技能（反击伤害减半）
                DamageResult damageResult = CalculateHitDamage(caster, skill, target, hit, log, context, logContext);
                int damage = (int)(damageResult.Damage * 0.5);
                if (damage > 0)
                {
                    anyHit = true;
                    int afterShieldDamage = DamageCalculator.ApplyShieldDamage(target, damage, context, log);
                    int actualDamage = DamageCalculator.ApplySanctuaryDamage(target, afterShieldDamage, context, log);
                    target.CurrentHp -= actualDamage;

                    // 格式化伤害日志
                    string damageText = damageResult.IsCrit ? $"{actualDamage}（暴击）" : actualDamage.ToString();
                    new BattleLogBuilder(log, context, BattleLogType.CounterAttack)
                        .Caster(caster)
                        .Target(target)
                        .Skill(skill)
                        .Value(actualDamage)
                        .Crit(damageResult.IsCrit)
                        .Description($"{BuildSkillActionPrefix(caster, skill, logContext)}对 {target.Name} 造成 {damageText} 点伤害，剩余气血 {Math.Max(0, target.CurrentHp)}。")
                        .Build();

                    // 记录伤害输出和受到的伤害
                    BattleRecorder.RecordDamageDealt(context, caster, actualDamage);
                    BattleRecorder.RecordDamageTaken(context, target, actualDamage);

                    // 记录技能伤害
                    BattleRecorder.RecordSkillDamage(context, caster, skill.Id, actualDamage);

                    // 吸血效果
                    DamageCalculator.ApplyLifesteal(caster, actualDamage, context, log);

                    // 反伤效果
                    if (actualDamage > 0)
                    {
                        DamageCalculator.ApplyReflectDamage(target, caster, actualDamage, context, log);
                    }

                    // 检查目标是否死亡，使用统一致死结算
                    if (target.CurrentHp <= 0)
                    {
                        BattleHelper.ResolveFatalDamage(target, caster, context, log);
                    }
                }
            }
            return anyHit;
        }

        private static string BuildSkillActionPrefix(BattleFighter caster, XXX.Entity.Skill skill, SkillLogContext logContext)
        {
            return logContext switch
            {
                SkillLogContext.Combo => $"{caster.Name} 触发连击，并使用技能[{skill.Name}]",
                SkillLogContext.Counter => $"{caster.Name} 发起反击，并使用技能[{skill.Name}]",
                _ => $"{caster.Name} 使用技能[{skill.Name}]"
            };
        }

        private static void AppendSupplementalEntries(RoundLog log, RoundLog tempLog, BattleContext context)
        {
            foreach (var entry in tempLog.Entries.Where(ShouldKeepSupplementalEntry))
            {
                entry.Timestamp = context.LogTimestampCounter++;
                log.Entries.Add(entry);
            }
        }

        private static bool ShouldKeepSupplementalEntry(BattleLogEntry entry)
        {
            if (entry.Type == BattleLogType.DamageTaken &&
                entry.Description.Contains("无敌状态", StringComparison.Ordinal))
            {
                return true;
            }

            return entry.Type == BattleLogType.ShieldAbsorb
                || entry.Type == BattleLogType.BuffRemove
                || entry.Type == BattleLogType.SanctuaryShare
                || entry.Type == BattleLogType.ReflectDamage
                || entry.Type == BattleLogType.Lifesteal
                || entry.Type == BattleLogType.Death
                || entry.Type == BattleLogType.Execute;
        }
    }
}
