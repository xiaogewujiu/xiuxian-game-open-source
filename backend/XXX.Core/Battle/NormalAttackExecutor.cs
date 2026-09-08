using XXX.Entity;

namespace XXX.Battle
{
    /// <summary>
    /// 普通攻击执行器
    /// 负责普通攻击的执行
    /// </summary>
    public static class NormalAttackExecutor
    {/// <summary>
     /// 随机数生成器（线程安全）
     /// </summary>
        private static Random Rand = Random.Shared;

        /// <summary>
        /// 执行普通攻击
        /// </summary>
        public static void ExecuteNormalAttack(BattleFighter attacker, BattleFighter target, BattleContext context, RoundLog log)
        {
            // 分身不记录普通攻击次数
            if (!attacker.IsMirror)
            {
                BattleRecorder.RecordNormalAttack(context, attacker);
            }

            // 命中判定（处理属性为0的情况）
            float hitRate = DamageCalculator.GetModifiedAccuracy(attacker, Math.Max(0, attacker.HitRate));
            float dodgeRate = Math.Max(0, target.DodgeRate);
            float hitChance = Math.Max(0.1f, hitRate - dodgeRate); // 最低10%命中
            if (Rand.NextDouble() > hitChance)
            {
                new BattleLogBuilder(log, context, BattleLogType.Dodge)
                    .Caster(attacker)
                    .Target(target)
                    .Description($"{target.Name} 闪避了 {attacker.Name} 的普通攻击。")
                    .Build();

                // 记录闪避和攻击被闪避
                BattleRecorder.RecordDodge(context, target);
                if (!attacker.IsMirror)
                {
                    BattleRecorder.RecordMissed(context, attacker);
                }

                // 分身不触发反击
                if (!attacker.IsMirror && !target.IsMirror)
                {
                    ComboCounterExecutor.TryCounterAttack(target, attacker, context, log);
                }
                return;
            }

            // 计算伤害
            DamageResult damageResult = DamageCalculator.CalculateNormalDamage(attacker, target, log, context);
            int damage = damageResult.Damage;

            // 护盾吸收
            int afterShieldDamage = DamageCalculator.ApplyShieldDamage(target, damage, context, log);
            // 庇护分担
            int actualDamage = DamageCalculator.ApplySanctuaryDamage(target, afterShieldDamage, context, log);
            target.CurrentHp -= actualDamage;

            // 格式化伤害日志
            string damageText = damageResult.IsCrit ? $"{actualDamage}（暴击）" : actualDamage.ToString();
            new BattleLogBuilder(log, context, BattleLogType.DamageDealt)
                .Caster(attacker)
                .Target(target)
                .Value(actualDamage)
                .Crit(damageResult.IsCrit)
                .Description($"{attacker.Name} 对 {target.Name} 发动普通攻击，造成 {damageText} 点伤害，剩余气血 {Math.Max(0, target.CurrentHp)}。")
                .Build();

            // 分身的伤害记录到主人名下
            var damageOwner = attacker.IsMirror ? attacker.MirrorOwner ?? attacker : attacker;
            BattleRecorder.RecordDamageDealt(context, damageOwner, actualDamage);
            BattleRecorder.RecordDamageTaken(context, target, actualDamage);

            // 吸血效果
            DamageCalculator.ApplyLifesteal(damageOwner, actualDamage, context, log);

            // 反伤效果
            if (actualDamage > 0)
            {
                DamageCalculator.ApplyReflectDamage(target, damageOwner, actualDamage, context, log);
            }

            // 检查目标是否死亡，记录击杀和死亡
            if (target.CurrentHp <= 0)
            {
                BattleHelper.ResolveFatalDamage(target, damageOwner, context, log);
            }

            // 分身不触发连击
            if (!attacker.IsMirror)
            {
                ComboCounterExecutor.TryComboAttack(attacker, target, context, log);
            }

            // 反击判定（分身不触发也不被反击）
            if (target.CurrentHp > 0 && !attacker.IsMirror && !target.IsMirror)
            {
                ComboCounterExecutor.TryCounterAttack(target, attacker, context, log);
            }
        }
    }
}
