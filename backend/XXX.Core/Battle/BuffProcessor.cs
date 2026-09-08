using XXX.Entity;

namespace XXX.Battle
{
    /// <summary>
    /// Buff处理器
    /// 负责Buff的处理和应用
    /// </summary>
    public static class BuffProcessor
    {/// <summary>
     /// 随机数生成器（线程安全）
     /// </summary>
        private static Random Rand = Random.Shared;
        /// <summary>
        /// 回合开始时处理Buff效果
        /// </summary>
        public static void ProcessBuffsAtRoundStart(BattleFighter fighter, BattleContext context, RoundLog log)
        {
            foreach (var buff in fighter.ActiveBuffs.ToList())
            {
                foreach (var effect in buff.Template.Effects)
                {
                    switch (effect.EffectType)
                    {
                        case BuffEffectType.DamageOverTime:
                            // 检查无敌效果，免疫持续伤害
                            if (HasInvincible(fighter))
                            {
                                new BattleLogBuilder(log, context, BattleLogType.DamageTaken)
                                    .Target(fighter)
                                    .Description($"{fighter.Name} 处于无敌状态，免疫了[{buff.Template.Name}]造成的伤害。")
                                    .Build();
                                break;
                            }

                            int dot = effect.IsPercentage
                                ? (int)(fighter.MaxHp * effect.Value * buff.CurrentStack)
                                : (int)(effect.Value * buff.CurrentStack);
                            fighter.CurrentHp -= dot;

                            new BattleLogBuilder(log, context, BattleLogType.DamageDealt)
                                .Caster(buff.Source)
                                .Target(fighter)
                                .Buff(buff.Template)
                                .Value(dot)
                                .Description($"{fighter.Name} 受到[{buff.Template.Name}]效果，损失了 {dot} 点生命。")
                                .Build();

                            // 记录受到的伤害
                            BattleRecorder.RecordDamageTaken(context, fighter, dot);

                            if (fighter.CurrentHp <= 0)
                            {
                                // 使用统一致死结算
                                var killer = buff.Source;
                                if (killer == null || !HasUndying(fighter) && !HasAutoRevive(fighter))
                                {
                                    // 记录死亡日志（统一函数不处理日志，需要保留）
                                    if (!HasUndying(fighter) && !HasAutoRevive(fighter))
                                    {
                                        new BattleLogBuilder(log, context, BattleLogType.Death)
                                            .Caster(buff.Source)
                                            .Target(fighter)
                                            .Buff(buff.Template)
                                            .Description($"{fighter.Name} 因[{buff.Template.Name}]效果而倒下。")
                                            .Build();
                                    }
                                }
                                BattleHelper.ResolveFatalDamage(fighter, killer, context, log, suppressDeathLog: true);
                            }
                            break;

                        case BuffEffectType.HealOverTime:
                            // 检查禁疗效果
                            if (HasHealBlock(fighter))
                            {
                                new BattleLogBuilder(log, context, BattleLogType.Heal)
                                    .Target(fighter)
                                    .Description($"{fighter.Name} 受到禁疗影响，无法恢复生命。")
                                    .Build();
                                break;
                            }

                            int hot = effect.IsPercentage
                                ? (int)(fighter.MaxHp * effect.Value * buff.CurrentStack)
                                : (int)(effect.Value * buff.CurrentStack);
                            int overheal = Math.Max(0, fighter.CurrentHp + hot - fighter.MaxHp);
                            fighter.CurrentHp = Math.Min(fighter.MaxHp, fighter.CurrentHp + hot);

                            new BattleLogBuilder(log, context, BattleLogType.Heal)
                                .Target(fighter)
                                .Buff(buff.Template)
                                .Value(hot)
                                .Description($"{fighter.Name} 受到[{buff.Template.Name}]效果，恢复了 {hot} 点生命。")
                                .Build();
                            break;

                        case BuffEffectType.ManaOverTime:

                            int mot = effect.IsPercentage
                                ? (int)(fighter.MaxMp * effect.Value * buff.CurrentStack)
                                : (int)(effect.Value * buff.CurrentStack);
                            int overmana = Math.Max(0, fighter.CurrentMp + mot - fighter.MaxMp);
                            fighter.CurrentMp = Math.Min(fighter.MaxMp, fighter.CurrentMp + mot);

                            new BattleLogBuilder(log, context, BattleLogType.BuffApply)
                                .Target(fighter)
                                .Buff(buff.Template)
                                .Value(mot)
                                .Description($"{fighter.Name} 受到[{buff.Template.Name}]效果，恢复了 {mot} 点灵力。")
                                .Build();
                            break;

                        case BuffEffectType.ManaDrain:

                            int drain = effect.IsPercentage
                                ? (int)(fighter.MaxMp * effect.Value * buff.CurrentStack)
                                : (int)(effect.Value * buff.CurrentStack);
                            fighter.CurrentMp = Math.Max(0, fighter.CurrentMp - drain);
                            new BattleLogBuilder(log, context, BattleLogType.BuffApply)
                                .Target(fighter)
                                .Buff(buff.Template)
                                .Value(drain)
                                .Description($"{fighter.Name} 受到[{buff.Template.Name}]效果，损失了 {drain} 点灵力。")
                                .Build();
                            break;

                        case BuffEffectType.Mirror:
                            // 处理镜像效果：在回合开始时检查分身持续时间
                            ProcessMirrorEffect(fighter, context, log);
                            break;

                        case BuffEffectType.Sanctuary:
                            // 处理庇护效果：回合开始时检查被庇护的单位
                            ProcessSanctuaryEffect(fighter, context, log);
                            break;
                    }
                }
            }

        }

        /// <summary>
        /// 处理镜像效果：检查分身持续时间
        /// </summary>
        private static void ProcessMirrorEffect(BattleFighter fighter, BattleContext context, RoundLog log)
        {
            var mirrorsToRemove = new List<BattleFighter>();

            foreach (var mirror in fighter.Mirrors.ToList())
            {
                if (mirror.CurrentHp <= 0)
                {
                    // 分身已死亡，移除
                    mirrorsToRemove.Add(mirror);
                    continue;
                }

                // 减少分身持续时间
                mirror.MirrorDuration--;

                if (mirror.MirrorDuration <= 0)
                {
                    new BattleLogBuilder(log, context, BattleLogType.BuffExpired)
                        .Caster(mirror)
                        .Description($"{fighter.Name} 的分身消失了。")
                        .Build();
                    RemoveMirror(fighter, mirror, context, log);
                }
                else
                {
                    new BattleLogBuilder(log, context, BattleLogType.BuffApply)
                        .Caster(mirror)
                        .Value(mirror.MirrorDuration)
                        .Description($"{fighter.Name} 的分身还可持续 {mirror.MirrorDuration} 回合。")
                        .Build();
                }
            }

            foreach (var mirror in mirrorsToRemove)
            {
                RemoveMirror(fighter, mirror, context, log);
            }
        }

        /// <summary>
        /// 创建分身
        /// </summary>
        public static BattleFighter? CreateMirror(BattleFighter owner, float attackRatio, float hpRatio, int duration, RoundLog log, BattleContext context)
        {
            // 检查分身数量限制
            int aliveMirrorCount = owner.Mirrors.Count(m => m.CurrentHp > 0 && m.MirrorDuration > 0);
            if (aliveMirrorCount >= 1)
            {
                new BattleLogBuilder(log, context, BattleLogType.BuffApply)
                    .Caster(owner)
                    .Description($"{owner.Name} 已有分身在场，无法再次召出。")
                    .Build();
                return null;
            }

            var mirror = new BattleFighter
            {
                Id = $"mirror_{owner.Id}_{Guid.NewGuid().ToString().Substring(0, 8)}",
                Name = $"{owner.Name}的分身",
                IsPlayerSide = owner.IsPlayerSide,
                FighterType = FighterType.Mirror,
                IsMirror = true,
                MirrorOwner = owner,
                MirrorDuration = duration,
                MirrorAttackRatio = attackRatio,
                MirrorHpRatio = hpRatio,

                // 属性继承
                CurrentHp = (int)(owner.MaxHp * hpRatio),
                MaxHp = (int)(owner.MaxHp * hpRatio),
                CurrentMp = 0,
                MaxMp = 0,
                PhysicalAttack = (int)(owner.PhysicalAttack * attackRatio),
                MagicAttack = (int)(owner.MagicAttack * attackRatio),
                PhysicalDefense = (int)(owner.PhysicalDefense * hpRatio),
                MagicDefense = (int)(owner.MagicDefense * hpRatio),
                Speed = owner.Speed, // 速度完全继承

                // 分身没有技能和Buff
                SkillIds = [],
                SkillCooldowns = [],
                ActiveBuffs = [],

                // 分身没有连击/反击
                ComboRate = 0,
                CounterRate = 0,

                // 复制灵根属性
                Element = owner.Element
            };

            owner.Mirrors.Add(mirror);
            new BattleLogBuilder(log, context, BattleLogType.BuffApply)
                .Caster(owner)
                .Target(mirror)
                .Description($"{owner.Name} 召出了分身，分身拥有 {mirror.PhysicalAttack} 点攻击和 {mirror.MaxHp} 点生命。")
                .Build();

            return mirror;
        }

        /// <summary>
        /// 移除分身
        /// </summary>
        public static void RemoveMirror(BattleFighter owner, BattleFighter mirror, BattleContext context, RoundLog log)
        {
            if (owner.Mirrors.Contains(mirror))
            {
                owner.Mirrors.Remove(mirror);
            }

            // 从战斗列表中移除
            if (context.PlayerSide.Contains(mirror))
            {
                context.PlayerSide.Remove(mirror);
            }
            if (context.EnemySide.Contains(mirror))
            {
                context.EnemySide.Remove(mirror);
            }
        }

        /// <summary>
        /// 获取所有存活单位（包括分身）
        /// </summary>
        public static List<BattleFighter> GetAllAliveFightersIncludingMirrors(BattleContext context)
        {
            var result = new List<BattleFighter>();
            result.AddRange(context.PlayerSide.Where(f => f.CurrentHp > 0));
            result.AddRange(context.EnemySide.Where(f => f.CurrentHp > 0));

            // 添加所有存活分身
            foreach (var fighter in context.PlayerSide.Concat(context.EnemySide))
            {
                foreach (var mirror in fighter.Mirrors.Where(m => m.CurrentHp > 0))
                {
                    result.Add(mirror);
                }
            }

            return result;
        }

        /// <summary>
        /// 检查是否为分身
        /// </summary>
        public static bool IsMirror(BattleFighter fighter)
        {
            return fighter.IsMirror;
        }

        /// <summary>
        /// 更新所有Buff持续时间
        /// </summary>
        public static void UpdateBuffDurations(BattleContext context)
        {
            var allFighters = BattleExecutor.GetAllAliveFighters(context);
            foreach (var fighter in allFighters)
            {
                // 更新Buff持续时间
                foreach (var buff in fighter.ActiveBuffs.ToList())
                {
                    // Duration为-1表示永久buff，不递减
                    if (buff.RemainingDuration == -1)
                        continue;

                    // 如果是本回合新施加的buff，不递减，只标记为false
                    if (buff.IsNewlyApplied)
                    {
                        buff.IsNewlyApplied = false;
                        continue;
                    }

                    buff.RemainingDuration--;
                    if (buff.RemainingDuration <= 0)
                    {
                        fighter.ActiveBuffs.Remove(buff);
                    }
                }

                // 更新技能冷却时间（考虑冷却缩减/增加效果）
                float cooldownModifier = GetCooldownModifier(fighter);
                foreach (var skillId in fighter.SkillCooldowns.Keys.ToList())
                {
                    if (fighter.SkillCooldowns[skillId] > 0)
                    {
                        // 如果是本回合新使用的技能，不递减
                        if (fighter.SkillsUsedThisRound.Contains(skillId))
                            continue;

                        // 应用冷却缩减/增加效果
                        // cooldownModifier > 1 表示冷却缩减更快，< 1 表示冷却缩减更慢
                        int reduction = (int)Math.Ceiling(cooldownModifier) + 1;
                        if (reduction >= 0)
                        {
                            fighter.SkillCooldowns[skillId] = Math.Max(0, fighter.SkillCooldowns[skillId] - reduction);
                        }
                        else
                        {
                            fighter.SkillCooldowns[skillId]--;
                        }
                    }
                }

                // 清空本回合使用的技能标记
                fighter.SkillsUsedThisRound.Clear();
            }
        }

        /// <summary>
        /// 应用技能携带的Buff（新版本：支持概率触发和智能目标选择）
        /// </summary>
        public static void ApplySkillBuffs(BattleFighter caster, XXX.Entity.Skill skill,
            List<BattleFighter> skillTargets, BattleContext context, RoundLog log)
        {
            if (skill.BuffIds == null) return;

            foreach (var buffId in skill.BuffIds)
            {
                if (!BuffDataTemplates.BuffTemplates.ContainsKey(buffId)) continue;

                var template = BuffDataTemplates.BuffTemplates[buffId];

                // 根据Buff的每个效果来确定目标
                ApplyBuffWithTargetSelection(caster, skill, template, skillTargets, context, log);
            }
        }

        /// <summary>
        /// 根据BuffEffect配置选择目标并应用Buff
        /// </summary>
        public static void ApplyBuffWithTargetSelection(BattleFighter caster, XXX.Entity.Skill? skill, BuffTemplate template,
            List<BattleFighter> skillTargets, BattleContext context, RoundLog log)
        {
            // 检查Buff的第一个效果来确定目标阵营和数量
            // 如果Buff有多个效果，它们应该有相同的目标配置
            if (template.Effects == null || template.Effects.Count == 0) return;

            var firstEffect = template.Effects[0];

            // 检查Buff触发概率
            if (Rand.NextDouble() > firstEffect.TriggerChance)
            {
                return; // Buff未触发
            }

            // 新增：固定选择施法者自己
            if (firstEffect.TargetSelfOnly)
            {
                // 特殊处理：镜像效果
                if (firstEffect.EffectType == BuffEffectType.Mirror)
                {
                    float attackRatio = firstEffect.IsPercentage ? (float)firstEffect.Value : 0.4f;
                    float hpRatio = 0.5f; // 默认50%血量
                    int duration = template.Duration > 0 ? template.Duration : 2;

                    var mirror = CreateMirror(caster, attackRatio, hpRatio, duration, log, context);

                    // 将分身添加到战斗列表中
                    if (mirror != null)
                    {
                        if (caster.IsPlayerSide)
                        {
                            context.PlayerSide.Add(mirror);
                        }
                        else
                        {
                            context.EnemySide.Add(mirror);
                        }
                    }
                }
                else
                {
                    if (ApplyBuffToTarget(caster, template, caster, context, log))
                    {
                        AppendSkillBuffLog(log, context, caster, skill, caster, template);
                    }
                }
                return;
            }

            int targetCamp = firstEffect.TargetCamp;
            int targetCount = firstEffect.TargetCount;
            var selectionMode = firstEffect.TargetSelectionMode;
            bool isRandomTarget = firstEffect.IsRandomTarget;

            // 根据是否随机选择目标来决定目标选择策略
            List<BattleFighter> selectedTargets;

            if (isRandomTarget)
            {
                // 随机选择目标：根据TargetCamp获取目标
                List<BattleFighter> targetPool;
                if (targetCamp == 2) // 己方
                {
                    targetPool = caster.IsPlayerSide
                        ? context.PlayerSide.Where(f => f.CurrentHp > 0).ToList()
                        : context.EnemySide.Where(f => f.CurrentHp > 0).ToList();
                }
                else // 敌方
                {
                    targetPool = caster.IsPlayerSide
                        ? context.EnemySide.Where(f => f.CurrentHp > 0).ToList()
                        : context.PlayerSide.Where(f => f.CurrentHp > 0).ToList();
                }

                // 筛选出技能目标池中实际在目标阵营的单位
                var validSkillTargets = skillTargets.Where(t => targetPool.Contains(t)).ToList();

                // 根据选择模式决定目标选择策略
                if (selectionMode == BuffTargetSelectionMode.PrioritizeSkillTargets && validSkillTargets.Count > 0)
                {
                    // 模式2：优先使用技能目标，不足的部分随机选择
                    if (targetCount == 0 || targetCount >= targetPool.Count)
                    {
                        // 全体目标
                        selectedTargets = targetPool;
                    }
                    else if (targetCount <= validSkillTargets.Count)
                    {
                        // buff目标数量 <= 技能目标数量，从技能目标中随机选择
                        selectedTargets = validSkillTargets.OrderBy(x => Rand.Next()).Take(targetCount).ToList();
                    }
                    else
                    {
                        // buff目标数量 > 技能目标数量，先使用所有技能目标，不足的部分从目标池随机选择
                        selectedTargets = [.. validSkillTargets];
                        int remainingCount = targetCount - validSkillTargets.Count;
                        var remainingPool = targetPool.Where(t => !validSkillTargets.Contains(t)).ToList();
                        if (remainingPool.Count > 0)
                        {
                            selectedTargets.AddRange(remainingPool.OrderBy(x => Rand.Next()).Take(remainingCount));
                        }
                    }
                }
                else
                {
                    // 模式1：完全随机选择目标
                    if (targetCount == 0 || targetCount >= targetPool.Count)
                    {
                        // 全体目标
                        selectedTargets = targetPool;
                    }
                    else
                    {
                        // 随机选择指定数量的目标
                        selectedTargets = targetPool.OrderBy(x => Rand.Next()).Take(targetCount).ToList();
                    }
                }
            }
            else
            {
                // 不随机：使用技能目标
                // 筛选出技能目标池中实际在目标阵营的单位
                List<BattleFighter> targetPool;
                if (targetCamp == 2) // 己方
                {
                    targetPool = caster.IsPlayerSide
                        ? context.PlayerSide.Where(f => f.CurrentHp > 0).ToList()
                        : context.EnemySide.Where(f => f.CurrentHp > 0).ToList();
                }
                else // 敌方
                {
                    targetPool = caster.IsPlayerSide
                        ? context.EnemySide.Where(f => f.CurrentHp > 0).ToList()
                        : context.PlayerSide.Where(f => f.CurrentHp > 0).ToList();
                }

                var validSkillTargets = skillTargets.Where(t => targetPool.Contains(t)).ToList();

                if (targetCount == 0 || targetCount >= validSkillTargets.Count)
                {
                    // 全体技能目标
                    selectedTargets = validSkillTargets;
                }
                else
                {
                    // 从技能目标中选择指定数量
                    selectedTargets = validSkillTargets.Take(targetCount).ToList();
                }
            }

            // 对每个选中的目标应用Buff
            foreach (var target in selectedTargets)
            {
                if (ApplyBuffToTarget(target, template, caster, context, log))
                {
                    AppendSkillBuffLog(log, context, caster, skill, target, template);
                }
            }
        }

        /// <summary>
        /// 应用Buff到目标（返回是否成功应用，不记录单个日志）
        /// </summary>
        private static bool ApplyBuffToTarget(BattleFighter target, BuffTemplate template, BattleFighter source, BattleContext context, RoundLog log)
        {
            // 检查免疫控制
            if (BattleHelper.HasImmunity(target))
            {
                bool isControlBuff = template.Effects.Any(e =>
                    e.EffectType == BuffEffectType.Stun ||
                    e.EffectType == BuffEffectType.Silence ||
                    e.EffectType == BuffEffectType.Taunt ||
                    e.EffectType == BuffEffectType.Disarm ||
                    e.EffectType == BuffEffectType.Root);

                if (isControlBuff)
                {
                    new BattleLogBuilder(log, context, BattleLogType.BuffResist)
                        .Target(target)
                        .Buff(template)
                        .Description($"{target.Name} 免疫了[{template.Name}]效果。")
                        .Build();
                    return false; // 免疫，不应用
                }
            }

            var existing = target.ActiveBuffs.FirstOrDefault(b => b.Template.Gid == template.Gid);
            bool hasShieldEffect = template.Effects.Any(e => e.EffectType == BuffEffectType.Shield);

            // 获取施加给目标的持续时间
            // 如果 EffectDuration != -1，使用 EffectDuration
            // 否则使用 template.Duration
            int effectDuration = template.Duration;
            if (template.Effects != null && template.Effects.Count > 0)
            {
                int firstEffectDuration = template.Effects[0].EffectDuration;
                if (firstEffectDuration != -1)
                {
                    effectDuration = firstEffectDuration;
                }
            }

            if (existing != null)
            {
                // 根据叠加规则处理
                switch (template.StackRule)
                {
                    case StackRule.RefreshDuration:
                        existing.RemainingDuration = effectDuration;
                        existing.Source = source; // 更新来源
                        existing.IsNewlyApplied = true; // 刷新时也标记为新施加
                        break;
                    case StackRule.StackValue:
                        if (existing.CurrentStack < template.MaxStack)
                            existing.CurrentStack++;
                        existing.RemainingDuration = effectDuration;
                        existing.Source = source; // 更新来源
                        existing.IsNewlyApplied = true; // 叠加时也标记为新施加
                        break;
                    case StackRule.Replace:
                        existing.RemainingDuration = effectDuration;
                        existing.CurrentStack = 1;
                        existing.Source = source; // 更新来源
                        existing.IsNewlyApplied = true; // 替换时也标记为新施加
                        break;
                }
                if (hasShieldEffect)
                {
                    existing.ShieldValue = 0;
                }
            }
            else
            {
                target.ActiveBuffs.Add(new ActiveBuff
                {
                    Template = template,
                    RemainingDuration = effectDuration,
                    CurrentStack = 1,
                    Source = source,
                    IsNewlyApplied = true // 新施加的buff标记为true
                });
            }

            return true;
        }

        /// <summary>
        /// 应用Buff到目标
        /// </summary>
        public static void ApplyBuff(BattleFighter target, BuffTemplate template, BattleFighter source, BattleContext context, RoundLog log)
        {
            // 检查免疫控制
            if (BattleHelper.HasImmunity(target))
            {
                bool isControlBuff = template.Effects.Any(e =>
                    e.EffectType == BuffEffectType.Stun ||
                    e.EffectType == BuffEffectType.Silence ||
                    e.EffectType == BuffEffectType.Taunt ||
                    e.EffectType == BuffEffectType.Disarm ||
                    e.EffectType == BuffEffectType.Root);

                if (isControlBuff)
                {
                    new BattleLogBuilder(log, context, BattleLogType.BuffResist)
                        .Target(target)
                        .Buff(template)
                        .Description($"{target.Name} 免疫了[{template.Name}]效果。")
                        .Build();
                    return;
                }
            }

            var existing = target.ActiveBuffs.FirstOrDefault(b => b.Template.Gid == template.Gid);
            bool hasShieldEffect = template.Effects.Any(e => e.EffectType == BuffEffectType.Shield);

            if (existing != null)
            {
                // 根据叠加规则处理
                switch (template.StackRule)
                {
                    case StackRule.RefreshDuration:
                        existing.RemainingDuration = template.Duration;
                        existing.Source = source; // 更新来源
                        existing.IsNewlyApplied = true; // 刷新时也标记为新施加
                        break;
                    case StackRule.StackValue:
                        if (existing.CurrentStack < template.MaxStack)
                            existing.CurrentStack++;
                        existing.RemainingDuration = template.Duration;
                        existing.Source = source; // 更新来源
                        existing.IsNewlyApplied = true; // 叠加时也标记为新施加
                        break;
                    case StackRule.Replace:
                        existing.RemainingDuration = template.Duration;
                        existing.CurrentStack = 1;
                        existing.Source = source; // 更新来源
                        existing.IsNewlyApplied = true; // 替换时也标记为新施加
                        break;
                }
                if (hasShieldEffect)
                {
                    existing.ShieldValue = 0;
                }
            }
            else
            {
                target.ActiveBuffs.Add(new ActiveBuff
                {
                    Template = template,
                    RemainingDuration = template.Duration,
                    CurrentStack = 1,
                    Source = source,
                    IsNewlyApplied = true // 新施加的buff标记为true
                });

            }
            new BattleLogBuilder(log, context, BattleLogType.BuffApply)
                .Caster(source)
                .Target(target)
                .Buff(template)
                .Description(BuildStandaloneBuffDescription(source, target, template))
                .Build();
        }

        /// <summary>
        /// 检查是否被禁疗
        /// </summary>
        public static bool HasHealBlock(BattleFighter fighter)
        {
            return fighter.ActiveBuffs.Any(b =>
                b.Template.Effects.Any(e => e.EffectType == BuffEffectType.HealBlock));
        }

        /// <summary>
        /// 获取冷却修正值（返回额外减少的冷却回合数）
        /// </summary>
        public static float GetCooldownModifier(BattleFighter fighter)
        {
            float modifier = 0f; // 默认每回合减少1点冷却
            foreach (var buff in fighter.ActiveBuffs)
            {
                foreach (var effect in buff.Template.Effects)
                {
                    if (effect.EffectType == BuffEffectType.CooldownReduction)
                    {
                        if (effect.IsPercentage)
                            modifier += (float)effect.Value * buff.CurrentStack;
                        else
                            modifier += (float)effect.Value * buff.CurrentStack;
                    }
                    else if (effect.EffectType == BuffEffectType.CooldownIncrease)
                    {
                        if (effect.IsPercentage)
                            modifier -= (float)effect.Value * buff.CurrentStack;
                        else
                            modifier -= (float)effect.Value * buff.CurrentStack;
                    }
                }
            }
            return modifier; // 可以是负数，表示冷却减少更慢
        }

        /// <summary>
        /// 检查是否有不屈效果（每场战斗触发一次）
        /// </summary>
        public static bool HasUndying(BattleFighter fighter)
        {
            return fighter.ActiveBuffs.Any(b =>
                b.Template.Effects.Any(e => e.EffectType == BuffEffectType.Undying));
        }

        /// <summary>
        /// 触发不屈效果（保留1点生命并移除不屈Buff）
        /// </summary>
        public static void TriggerUndying(BattleFighter fighter, RoundLog log, BattleContext context)
        {
            var undyingBuff = fighter.ActiveBuffs.FirstOrDefault(b =>
                b.Template.Effects.Any(e => e.EffectType == BuffEffectType.Undying));

            if (undyingBuff != null)
            {
                fighter.CurrentHp = 1;
                fighter.ActiveBuffs.Remove(undyingBuff);
                new BattleLogBuilder(log, context, BattleLogType.PassiveTrigger)
                    .Target(fighter)
                    .Description($"{fighter.Name} 触发[不屈]效果，保留了 1 点生命。")
                    .Build();
            }
        }

        /// <summary>
        /// 处理庇护效果：为友方分担伤害
        /// </summary>
        private static void ProcessSanctuaryEffect(BattleFighter fighter, BattleContext context, RoundLog log)
        {
            foreach (var buff in fighter.ActiveBuffs.ToList())
            {
                foreach (var effect in buff.Template.Effects)
                {
                    if (effect.EffectType == BuffEffectType.Sanctuary)
                    {
                        // 庇护效果在伤害计算时处理，这里只记录日志
                        // 如果有庇护Buff，会在 DamageCalculator 中处理
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 触发复活效果
        /// </summary>
        public static bool TriggerAutoRevive(BattleFighter fighter, BattleContext context, RoundLog log)
        {
            var reviveBuff = fighter.ActiveBuffs.FirstOrDefault(b =>
                b.Template.Effects.Any(e => e.EffectType == BuffEffectType.AutoRevive));

            if (reviveBuff != null)
            {
                // 移除复活Buff
                fighter.ActiveBuffs.Remove(reviveBuff);

                // 复活并恢复生命值
                int reviveAmount = (int)(fighter.MaxHp * 0.5); // 恢复50%最大血量
                fighter.CurrentHp = Math.Min(fighter.MaxHp, reviveAmount);

                new BattleLogBuilder(log, context, BattleLogType.Revive)
                    .Target(fighter)
                    .Value(reviveAmount)
                    .Description($"{fighter.Name} 触发[复活]效果，重新站起，并恢复了 {reviveAmount} 点生命。")
                    .Build();

                // 记录治疗量和受到的治疗
                BattleRecorder.RecordHealingDone(context, fighter, reviveAmount);
                BattleRecorder.RecordHealingReceived(context, fighter, reviveAmount);

                return true;
            }
            return false;
        }

        /// <summary>
        /// 检查是否有无敌效果
        /// </summary>
        public static bool HasInvincible(BattleFighter fighter)
        {
            return fighter.ActiveBuffs.Any(b =>
                b.Template.Effects.Any(e => e.EffectType == BuffEffectType.Invincible));
        }

        /// <summary>
        /// 检查是否被魅惑
        /// </summary>
        public static bool IsCharmed(BattleFighter fighter)
        {
            return fighter.ActiveBuffs.Any(b =>
                b.Template.Effects.Any(e => e.EffectType == BuffEffectType.Charm));
        }

        /// <summary>
        /// 检查是否有复活效果
        /// </summary>
        public static bool HasAutoRevive(BattleFighter fighter)
        {
            return fighter.ActiveBuffs.Any(b =>
                b.Template.Effects.Any(e => e.EffectType == BuffEffectType.AutoRevive));
        }

        /// <summary>
        /// 有益Buff效果类型列表
        /// </summary>
        private static readonly HashSet<BuffEffectType> PositiveEffects =
        [
            BuffEffectType.AttackUp,
            BuffEffectType.DefenseUp,
            BuffEffectType.CritRateUp,
            BuffEffectType.CritDamageUp,
            BuffEffectType.DamageAmplify,
            BuffEffectType.DamageReduction,
            BuffEffectType.Shield,
            BuffEffectType.Immunity,
            BuffEffectType.ComboRateUp,
            BuffEffectType.CounterRateUp,
            BuffEffectType.ComboDamageUp,
            BuffEffectType.CounterDamageUp,
            BuffEffectType.Lifesteal,
            BuffEffectType.ReflectDamage,
            BuffEffectType.Haste,
            BuffEffectType.CooldownReduction,
            BuffEffectType.Undying,
            BuffEffectType.AutoRevive,
            BuffEffectType.Sanctuary,
            BuffEffectType.HealOverTime,
            BuffEffectType.ManaOverTime,
            BuffEffectType.AccuracyUp,
            BuffEffectType.CritResist,
            BuffEffectType.ArmorBreak,
        ];

        /// <summary>
        /// 判断Buff是否为有益Buff
        /// </summary>
        public static bool IsPositiveBuff(ActiveBuff buff)
        {
            return buff.Template.Effects.Any(e => PositiveEffects.Contains(e.EffectType));
        }

        /// <summary>
        /// 判断Buff是否为有害Buff
        /// </summary>
        public static bool IsNegativeBuff(ActiveBuff buff)
        {
            return !IsPositiveBuff(buff);
        }

        /// <summary>
        /// 获取目标的有益Buff列表（按剩余回合数排序，最新的在前）
        /// </summary>
        public static List<ActiveBuff> GetPositiveBuffs(BattleFighter fighter)
        {
            return fighter.ActiveBuffs
                .Where(b => IsPositiveBuff(b))
                .OrderByDescending(b => b.RemainingDuration)
                .ToList();
        }

        /// <summary>
        /// 获取目标的有害Buff列表（按剩余回合数排序，最新的在前）
        /// </summary>
        public static List<ActiveBuff> GetNegativeBuffs(BattleFighter fighter)
        {
            return fighter.ActiveBuffs
                .Where(b => IsNegativeBuff(b))
                .OrderByDescending(b => b.RemainingDuration)
                .ToList();
        }

        /// <summary>
        /// 驱散目标的有益Buff
        /// </summary>
        /// <param name="target">目标</param>
        /// <param name="count">驱散数量</param>
        /// <param name="context">战斗上下文</param>
        /// <param name="log">战斗日志</param>
        /// <returns>实际驱散的数量</returns>
        public static int DispelBuffs(BattleFighter target, int count, BattleContext context, RoundLog log, BattleFighter? caster = null, XXX.Entity.Skill? skill = null)
        {
            var buffsToRemove = GetPositiveBuffs(target).Take(count).ToList();

            foreach (var buff in buffsToRemove)
            {
                target.ActiveBuffs.Remove(buff);
            }

            if (buffsToRemove.Count > 0 && log != null)
            {
                string buffNames = string.Join("、", buffsToRemove.Select(b => b.Template.Name));
                new BattleLogBuilder(log, context, BattleLogType.Dispel)
                    .Caster(caster)
                    .Target(target)
                    .Skill(skill)
                    .Description(caster != null && skill != null
                        ? $"{caster.Name} 驱散了 {target.Name} 的 {buffsToRemove.Count} 个增益效果：[{buffNames}]。"
                        : $"{target.Name} 被驱散了 {buffsToRemove.Count} 个增益效果：[{buffNames}]。")
                    .Build();
            }

            return buffsToRemove.Count;
        }

        /// <summary>
        /// 净化目标的有害Buff
        /// </summary>
        /// <param name="target">目标</param>
        /// <param name="count">净化数量</param>
        /// <param name="context">战斗上下文</param>
        /// <param name="log">战斗日志</param>
        /// <returns>实际净化的数量</returns>
        public static int CleanseBuffs(BattleFighter target, int count, BattleContext context, RoundLog log, BattleFighter? caster = null, XXX.Entity.Skill? skill = null)
        {
            var buffsToRemove = GetNegativeBuffs(target).Take(count).ToList();

            foreach (var buff in buffsToRemove)
            {
                target.ActiveBuffs.Remove(buff);
            }

            if (buffsToRemove.Count > 0 && log != null)
            {
                string buffNames = string.Join("、", buffsToRemove.Select(b => b.Template.Name));
                new BattleLogBuilder(log, context, BattleLogType.Cleanse)
                    .Caster(caster)
                    .Target(target)
                    .Skill(skill)
                    .Description(caster != null && skill != null
                        ? $"{caster.Name} 净化了 {target.Name} 的 {buffsToRemove.Count} 个减益效果：[{buffNames}]。"
                        : $"{target.Name} 被净化了 {buffsToRemove.Count} 个减益效果：[{buffNames}]。")
                    .Build();
            }

            return buffsToRemove.Count;
        }

        private static void AppendSkillBuffLog(RoundLog log, BattleContext context, BattleFighter caster, XXX.Entity.Skill? skill, BattleFighter target, BuffTemplate template)
        {
            if (skill == null)
            {
                new BattleLogBuilder(log, context, BattleLogType.BuffApply)
                    .Caster(caster)
                    .Target(target)
                    .Buff(template)
                    .Description(BuildStandaloneBuffDescription(caster, target, template))
                    .Build();
                return;
            }

            var existingEntry = log.Entries.LastOrDefault(entry =>
                entry.CasterId == caster.Id &&
                entry.TargetId == target.Id &&
                entry.SkillId == skill.Id &&
                (entry.Type == BattleLogType.DamageDealt ||
                 entry.Type == BattleLogType.Heal ||
                 entry.Type == BattleLogType.Revive ||
                 entry.Type == BattleLogType.ComboAttack ||
                 entry.Type == BattleLogType.CounterAttack));

            if (existingEntry != null)
            {
                existingEntry.BuffId = template.Gid;
                existingEntry.BuffName = template.Name;
                existingEntry.Description = AppendBuffClause(existingEntry.Description, template.Name);
                return;
            }

            new BattleLogBuilder(log, context, BattleLogType.BuffApply)
                .Caster(caster)
                .Target(target)
                .Skill(skill)
                .Buff(template)
                .Description(caster.Id == target.Id
                    ? $"{caster.Name} 使用技能[{skill.Name}]为自己施加[{template.Name}]。"
                    : $"{caster.Name} 使用技能[{skill.Name}]为 {target.Name} 施加[{template.Name}]。")
                .Build();
        }

        private static string AppendBuffClause(string description, string buffName)
        {
            var text = (description ?? string.Empty).TrimEnd('。', '！', '？');
            if (string.IsNullOrWhiteSpace(text))
            {
                return $"施加了[{buffName}]。";
            }

            if (text.Contains("，并施加[", StringComparison.Ordinal))
            {
                return $"{text}、[{buffName}]。";
            }

            return $"{text}，并施加[{buffName}]。";
        }

        private static string BuildStandaloneBuffDescription(BattleFighter source, BattleFighter target, BuffTemplate template)
        {
            if (source.Id == target.Id)
            {
                return $"{source.Name} 为自己施加了[{template.Name}]。";
            }

            return $"{source.Name} 为 {target.Name} 施加了[{template.Name}]。";
        }
    }
}
