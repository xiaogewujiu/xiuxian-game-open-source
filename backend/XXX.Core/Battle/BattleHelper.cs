namespace XXX.Battle
{
    /// <summary>
    /// 战斗辅助方法
    /// </summary>
    public static class BattleHelper
    {
        /// <summary>
        /// 检查是否被眩晕
        /// </summary>
        public static bool IsStunned(BattleFighter fighter)
        {
            return fighter.ActiveBuffs.Any(b =>
                b.Template.Effects.Any(e => e.EffectType == XXX.Entity.BuffEffectType.Stun));
        }

        /// <summary>
        /// 检查是否被沉默
        /// </summary>
        public static bool IsSilenced(BattleFighter fighter)
        {
            return fighter.ActiveBuffs.Any(b =>
                b.Template.Effects.Any(e => e.EffectType == XXX.Entity.BuffEffectType.Silence));
        }

        /// <summary>
        /// 获取嘲讽目标（如果被嘲讽）
        /// </summary>
        public static BattleFighter? GetTauntTarget(BattleFighter fighter)
        {
            var tauntBuff = fighter.ActiveBuffs.FirstOrDefault(b =>
                b.Template.Effects.Any(e => e.EffectType == XXX.Entity.BuffEffectType.Taunt));

            return tauntBuff?.Source;
        }

        /// <summary>
        /// 检查是否免疫控制
        /// </summary>
        public static bool HasImmunity(BattleFighter fighter)
        {
            return fighter.ActiveBuffs.Any(b =>
                b.Template.Effects.Any(e => e.EffectType == XXX.Entity.BuffEffectType.Immunity));
        }

        /// <summary>
        /// 检查是否被禁疗
        /// </summary>
        public static bool HasHealBlock(BattleFighter fighter)
        {
            return fighter.ActiveBuffs.Any(b =>
                b.Template.Effects.Any(e => e.EffectType == XXX.Entity.BuffEffectType.HealBlock));
        }

        /// <summary>
        /// 检查是否被缴械
        /// </summary>
        public static bool IsDisarmed(BattleFighter fighter)
        {
            return fighter.ActiveBuffs.Any(b =>
                b.Template.Effects.Any(e => e.EffectType == XXX.Entity.BuffEffectType.Disarm));
        }

        /// <summary>
        /// 检查是否被束缚
        /// </summary>
        public static bool IsRooted(BattleFighter fighter)
        {
            return fighter.ActiveBuffs.Any(b =>
                b.Template.Effects.Any(e => e.EffectType == XXX.Entity.BuffEffectType.Root));
        }

        /// <summary>
        /// 获取实际的战斗者（如果分身则返回主人）
        /// </summary>
        public static BattleFighter GetActualFighter(BattleFighter fighter)
        {
            if (fighter.IsMirror && fighter.MirrorOwner != null)
            {
                return fighter.MirrorOwner;
            }
            return fighter;
        }

        /// <summary>
        /// 检查是否有分身
        /// </summary>
        public static bool HasMirror(BattleFighter fighter)
        {
            return fighter.Mirrors.Any(m => m.CurrentHp > 0);
        }

        /// <summary>
        /// 检查是否为分身
        /// </summary>
        public static bool IsMirror(BattleFighter fighter)
        {
            return fighter.IsMirror;
        }

        /// <summary>
        /// 统一致死结算
        /// 检查不屈、自动复活，最后记录死亡
        /// </summary>
        public static void ResolveFatalDamage(BattleFighter target, BattleFighter? killer,
            BattleContext context, RoundLog log, bool suppressDeathLog = false)
        {
            if (target.CurrentHp > 0) return;

            // 检查不屈
            if (BuffProcessor.HasUndying(target))
            {
                BuffProcessor.TriggerUndying(target, log, context);
                return;
            }

            // 检查自动复活
            if (BuffProcessor.HasAutoRevive(target))
            {
                BuffProcessor.TriggerAutoRevive(target, context, log);
                return;
            }

            if (!suppressDeathLog)
            {
                new BattleLogBuilder(log, context, XXX.Entity.BattleLogType.Death)
                    .Caster(killer)
                    .Target(target)
                    .Description($"{target.Name} 战败倒下。")
                    .Build();
            }

            // 记录死亡
            BattleRecorder.RecordKillAndDeath(context, killer, target);
        }
    }
}
