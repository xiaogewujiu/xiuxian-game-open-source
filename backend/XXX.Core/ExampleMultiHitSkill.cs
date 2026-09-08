using XXX.Entity;

namespace XXX
{
    /// <summary>
    /// 多段伤害技能示例
    /// 展示如何使用新的多段伤害技能系统
    /// </summary>
    public static class ExampleMultiHitSkill
    {
        /// <summary>
        /// 创建示例技能：连环斩
        /// 第一次造成85%+500伤害
        /// 第二击造成135%+1000伤害
        /// </summary>
        public static Skill CreateComboSlashSkill()
        {
            return new Skill
            {
                Id = 100,
                Name = "连环斩",
                Description = "连续攻击目标两次，第一次造成85%+500伤害，第二次造成135%+1000伤害",
                TargetType = 1, // 敌方
                ManaCost = 30,
                Cooldown = 2,
                DamageType = DamageType.Physical,
                RangeType = 1, // 单体
                TriggerChance = 1.0,
                // 多段伤害配置
                Hits =
                [
                    new SkillHit
                    {
                        DamageMultiplier = 0.85,
                        BaseDamage = 500,
                        Description = "第一击：85%攻击力 + 500伤害"
                    },
                    new SkillHit
                    {
                        DamageMultiplier = 1.35,
                        BaseDamage = 1000,
                        Description = "第二击：135%攻击力 + 1000伤害"
                    }
                ]
            };
        }

        /// <summary>
        /// 创建示例技能：元素爆发
        /// 三段伤害，每段伤害类型不同
        /// </summary>
        public static Skill CreateElementalBurstSkill()
        {
            return new Skill
            {
                Id = 101,
                Name = "元素爆发",
                Description = "三段元素伤害，每段附带不同效果",
                TargetType = 1,
                ManaCost = 50,
                Cooldown = 3,
                DamageType = DamageType.Magic,
                RangeType = 0, // 全体
                TriggerChance = 1.0,
                Hits =
                [
                    new SkillHit
                    {
                        DamageMultiplier = 0.6,
                        BaseDamage = 300,
                        HitDamageType = DamageType.Physical,
                        Description = "第一段：物理伤害"
                    },
                    new SkillHit
                    {
                        DamageMultiplier = 0.8,
                        BaseDamage = 400,
                        HitDamageType = DamageType.Magic,
                        Description = "第二段：法术伤害"
                    },
                    new SkillHit
                    {
                        DamageMultiplier = 1.0,
                        BaseDamage = 500,
                        HitDamageType = DamageType.True,
                        Description = "第三段：真实伤害"
                    }
                ],
                BuffIds = ["burn_001"] // 携带灼烧Buff
            };
        }

        /// <summary>
        /// 创建示例技能：治疗连击
        /// 两段治疗，每段治疗量不同
        /// </summary>
        public static Skill CreateHealComboSkill()
        {
            return new Skill
            {
                Id = 102,
                Name = "治疗连击",
                Description = "连续治疗目标两次，第一次恢复80%法术攻击，第二次恢复120%法术攻击",
                TargetType = 2, // 友方
                ManaCost = 40,
                Cooldown = 1,
                DamageType = DamageType.Heal,
                RangeType = 1, // 单体
                TriggerChance = 1.0,
                Hits =
                [
                    new SkillHit
                    {
                        DamageMultiplier = 0.8,
                        BaseDamage = 0,
                        HitDamageType = DamageType.Heal,
                        Description = "第一段：恢复80%法术攻击"
                    },
                    new SkillHit
                    {
                        DamageMultiplier = 1.2,
                        BaseDamage = 0,
                        HitDamageType = DamageType.Heal,
                        Description = "第二段：恢复120%法术攻击"
                    }
                ]
            };
        }

        /// <summary>
        /// 创建示例技能：蓄力一击
        /// 单段伤害，但有基础伤害和倍率
        /// </summary>
        public static Skill CreateChargedStrikeSkill()
        {
            return new Skill
            {
                Id = 103,
                Name = "蓄力一击",
                Description = "蓄力后攻击，造成150%攻击力 + 800点伤害",
                TargetType = 1,
                ManaCost = 25,
                Cooldown = 3,
                DamageType = DamageType.Physical,
                RangeType = 1,
                TriggerChance = 1.0,
                Hits =
                [
                    new SkillHit
                    {
                        DamageMultiplier = 1.5,
                        BaseDamage = 800,
                        Description = "蓄力攻击：150%攻击力 + 800伤害"
                    }
                ]
            };
        }

        /// <summary>
        /// 创建示例技能：旧版本兼容技能
        /// 使用旧的HitCount和DamageMultiplier字段
        /// </summary>
        public static Skill CreateLegacySkill()
        {
            return new Skill
            {
                Id = 104,
                Name = "普通攻击",
                Description = "普通攻击，造成100%攻击力伤害",
                TargetType = 1,
                ManaCost = 0,
                Cooldown = 0,
                DamageType = DamageType.Physical,
                HitCount = 1, // 旧版本使用HitCount
                DamageMultiplier = 1.0, // 旧版本使用DamageMultiplier
                RangeType = 1,
                TriggerChance = 1.0
            };
        }
    }
}
