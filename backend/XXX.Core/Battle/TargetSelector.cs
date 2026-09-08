using XXX.Entity;

namespace XXX.Battle
{
    /// <summary>
    /// 目标选择模式
    /// </summary>
    public enum TargetSelectMode
    {
        /// <summary>智能模式：根据规则选择最优目标</summary>
        Smart,
        /// <summary>随机模式：随机选择目标</summary>
        Random,
        /// <summary>顺序模式：按列表顺序选择目标</summary>
        Sequential
    }

    /// <summary>
    /// 目标选择器
    /// 负责技能目标和攻击目标的选择
    /// </summary>
    public static class TargetSelector
    {
        /// <summary>
        /// 随机数生成器（线程安全）
        /// </summary>
        private static Random Rand = Random.Shared;

        /// <summary>
        /// 获取技能目标（支持多种选择模式）
        /// </summary>
        public static List<BattleFighter> GetSkillTargets(XXX.Entity.Skill skill, BattleFighter caster,
            List<BattleFighter> enemies, List<BattleFighter> allies,
            TargetSelectMode mode = TargetSelectMode.Smart)
        {
            var targets = new List<BattleFighter>();

            // 复活术特殊处理：选择死亡的友方
            if (skill.DamageType == DamageType.Revive)
            {
                return GetReviveTargets(skill, allies, mode);
            }

            // 检查魅惑状态，魅惑时目标类型反转
            bool isCharmed = BuffProcessor.IsCharmed(caster);
            int effectiveTargetType = isCharmed
                ? (skill.TargetType == 1 ? 2 : 1)  // 1变2，2变1
                : skill.TargetType;

            var targetPool = effectiveTargetType == 1 ? enemies : allies;

            // 对于伤害技能，只选择存活的目标
            if (skill.DamageType == DamageType.Physical ||
                skill.DamageType == DamageType.Magic ||
                skill.DamageType == DamageType.True ||
                skill.DamageType == DamageType.PercentDamage)
            {
                targetPool = targetPool.Where(t => t.CurrentHp > 0).ToList();
            }

            if (skill.RangeType == 0)
            {
                // 全体目标
                targets.AddRange(targetPool);
            }
            else
            {
                // 根据模式选择指定数量目标
                int count = Math.Min(skill.RangeType, targetPool.Count);
                targets.AddRange(mode switch
                {
                    TargetSelectMode.Smart => SelectSmartTargets(skill, targetPool, count),
                    TargetSelectMode.Random => SelectRandomTargets(targetPool, count),
                    TargetSelectMode.Sequential => SelectSequentialTargets(targetPool, count),
                    _ => SelectSmartTargets(skill, targetPool, count)
                });
            }
            return targets;
        }

        /// <summary>
        /// 获取复活术目标
        /// </summary>
        private static List<BattleFighter> GetReviveTargets(XXX.Entity.Skill skill,
            List<BattleFighter> allies, TargetSelectMode mode)
        {
            var targets = new List<BattleFighter>();
            var deadAllies = allies.Where(f => f.CurrentHp <= 0).ToList();

            if (deadAllies.Count == 0)
                return targets;

            BattleFighter target = mode switch
            {
                // 智能模式：优先复活攻击力最高的单位
                TargetSelectMode.Smart => deadAllies
                    .OrderByDescending(f => Math.Max(f.PhysicalAttack, f.MagicAttack))
                    .First(),
                // 随机模式：随机选择
                TargetSelectMode.Random => deadAllies[Rand.Next(deadAllies.Count)],
                // 顺序模式：选择第一个死亡的友方
                TargetSelectMode.Sequential => deadAllies.First(),
                _ => deadAllies
                    .OrderByDescending(f => Math.Max(f.PhysicalAttack, f.MagicAttack))
                    .First()
            };

            targets.Add(target);
            return targets;
        }

        /// <summary>
        /// 智能选择技能目标
        /// </summary>
        public static List<BattleFighter> SelectSmartTargets(XXX.Entity.Skill skill, List<BattleFighter> targetPool, int count)
        {
            var targets = new List<BattleFighter>();

            // 治疗技能：选择血量最低的存活目标
            if (skill.DamageType == DamageType.Heal)
            {
                targets = targetPool
                    .Where(t => t.CurrentHp > 0)  // 只选择存活的目标
                    .OrderBy(t => (float)t.CurrentHp / t.MaxHp)
                    .Take(count)
                    .ToList();
            }
            // 蓝量恢复技能：选择蓝量最低的存活目标
            else if (skill.DamageType == DamageType.ManaRestore)
            {
                targets = targetPool
                    .Where(t => t.CurrentHp > 0)  // 只选择存活的目标
                    .OrderBy(t => (float)t.CurrentMp / t.MaxMp)
                    .Take(count)
                    .ToList();
            }
            // 伤害技能：选择威胁值最高的目标
            else if (skill.TargetType == 1)
            {
                targets = targetPool
                    .OrderByDescending(t => CalculateThreatScore(t))
                    .Take(count)
                    .ToList();
            }
            // Buff技能或其他友方技能：随机选择
            else
            {
                targets = targetPool
                    .OrderBy(x => Rand.Next())
                    .Take(count)
                    .ToList();
            }

            return targets;
        }

        /// <summary>
        /// 随机选择目标
        /// </summary>
        public static List<BattleFighter> SelectRandomTargets(List<BattleFighter> targetPool, int count)
        {
            return targetPool
                .OrderBy(x => Rand.Next())
                .Take(count)
                .ToList();
        }

        /// <summary>
        /// 顺序选择目标
        /// </summary>
        public static List<BattleFighter> SelectSequentialTargets(List<BattleFighter> targetPool, int count)
        {
            return targetPool
                .Take(count)
                .ToList();
        }

        /// <summary>
        /// 计算目标威胁值（用于选择攻击目标）
        /// </summary>
        public static float CalculateThreatScore(BattleFighter target)
        {
            float score = 0;

            // 1. 残血目标优先（可以击杀）
            if (target.MaxHp > 0)
            {
                float hpPercent = (float)target.CurrentHp / target.MaxHp;
                if (hpPercent < 0.3f)
                    score += 50; // 残血高优先级
                else if (hpPercent < 0.5f)
                    score += 30;
            }

            // 2. 攻击力高的目标威胁大
            int totalAtk = Math.Max(target.PhysicalAttack, target.MagicAttack);
            score += totalAtk * 0.01f;

            // 3. 防御低的目标容易击杀
            int totalDef = Math.Max(target.PhysicalDefense, target.MagicDefense);
            score += (1000 - totalDef) * 0.005f;

            return score;
        }

        /// <summary>
        /// 选择攻击目标（支持多种选择模式）
        /// </summary>
        public static BattleFighter? SelectAttackTarget(List<BattleFighter> enemies,
            TargetSelectMode mode = TargetSelectMode.Smart)
        {
            if (enemies.Count == 0) return null;
            if (enemies.Count == 1) return enemies[0];

            return mode switch
            {
                TargetSelectMode.Smart => SelectSmartAttackTarget(enemies),
                TargetSelectMode.Random => SelectRandomAttackTarget(enemies),
                TargetSelectMode.Sequential => SelectSequentialAttackTarget(enemies),
                _ => SelectSmartAttackTarget(enemies)
            };
        }

        /// <summary>
        /// 智能选择攻击目标
        /// </summary>
        public static BattleFighter? SelectSmartAttackTarget(List<BattleFighter> enemies)
        {
            if (enemies.Count == 0) return null;
            if (enemies.Count == 1) return enemies[0];

            // 计算每个目标的威胁值
            var targetScores = enemies.Select(e => new { Target = e, Score = CalculateThreatScore(e) }).ToList();

            // 选择威胁值最高的目标
            return targetScores.OrderByDescending(t => t.Score).First().Target;
        }

        /// <summary>
        /// 随机选择攻击目标
        /// </summary>
        public static BattleFighter? SelectRandomAttackTarget(List<BattleFighter> enemies)
        {
            if (enemies.Count == 0) return null;
            return enemies[Rand.Next(enemies.Count)];
        }

        /// <summary>
        /// 顺序选择攻击目标
        /// </summary>
        public static BattleFighter? SelectSequentialAttackTarget(List<BattleFighter> enemies)
        {
            if (enemies.Count == 0) return null;
            return enemies[0];
        }

        // 保持向后兼容的旧方法
        /// <summary>
        /// 智能选择攻击目标（兼容旧版本）
        /// </summary>
        [Obsolete("请使用 SelectAttackTarget 方法代替")]
        public static BattleFighter? SelectBestAttackTarget(List<BattleFighter> enemies)
        {
            return SelectSmartAttackTarget(enemies);
        }
    }
}
