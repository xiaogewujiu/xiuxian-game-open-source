using XXX.Entity;

namespace XXX
{
    /// <summary>
    /// Buff数据模板类
    /// 提供所有Buff模板的初始化和管理功能
    /// </summary>
    /// <remarks>
    /// 主要功能：
    /// - 初始化所有Buff模板数据
    /// - 提供Buff模板的静态访问字典
    /// - 覆盖所有BuffEffectType效果类型
    /// 
    /// Buff类型包括：
    /// - 持续伤害（中毒、灼烧）
    /// - 持续治疗（回血、回蓝）
    /// - 属性增益（攻击提升、防御提升）
    /// - 属性减益（攻击降低、防御降低）
    /// - 控制效果（眩晕、沉默、减速）
    /// - 特殊效果（护盾、反伤、吸血）
    /// </remarks>
    public class BuffDataTemplates
    {
        /// <summary>
        /// Buff模版字典，Key为Buff的GID
        /// </summary>
        public static Dictionary<string, BuffTemplate> BuffTemplates = [];

        public static bool HasLoadedRuntimeTemplates => BuffTemplates.Count > 0;

        /// <summary>
        /// 清空运行时 Buff 模板缓存。
        /// </summary>
        public static void ClearRuntimeCaches()
        {
            BuffTemplates = new Dictionary<string, BuffTemplate>(StringComparer.OrdinalIgnoreCase);
        }
        #region Buff模版初始化
        /// <summary>
        /// 初始化所有Buff模版数据
        /// 覆盖所有BuffEffectType效果类型
        /// </summary>
        public static void InitializeBuffTemplates()
        {
            ClearRuntimeCaches();
            // Buff1: 中毒 - 持续伤害
            BuffTemplates["buff_001"] = new BuffTemplate
            {
                Gid = "buff_001",
                Name = "中毒",
                Description = "每回合受到持续伤害",
                Duration = 3,
                MaxStack = 3,
                StackRule = StackRule.StackValue,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.DamageOverTime,
                        Value = 50,
                        IsPercentage = false,
                        TargetCamp = 1,
                        TargetCount = 1,
                        IsRandomTarget = false
                    }
                ]
            };

            // Buff2: 灼烧 - 百分比持续伤害
            BuffTemplates["buff_002"] = new BuffTemplate
            {
                Gid = "buff_002",
                Name = "灼烧",
                Description = "每回合受到最大生命值5%的伤害",
                Duration = 2,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.DamageOverTime,
                        Value = 0.05,
                        IsPercentage = true,
                        TargetCamp = 1,
                        TargetCount = 1,
                        IsRandomTarget = false
                    }
                ]
            };

            // Buff3: 回春 - 持续治疗
            BuffTemplates["buff_003"] = new BuffTemplate
            {
                Gid = "buff_003",
                Name = "回春",
                Description = "每回合恢复生命值",
                Duration = 3,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.HealOverTime,
                        Value = 80,
                        IsPercentage = false,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = true
                    }
                ]
            };

            // Buff4: 生命涌动 - 百分比持续治疗
            BuffTemplates["buff_004"] = new BuffTemplate
            {
                Gid = "buff_004",
                Name = "生命涌动",
                Description = "每回合恢复最大生命值8%",
                Duration = 3,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.HealOverTime,
                        Value = 0.08,
                        IsPercentage = true,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = true
                    }
                ]
            };

            // Buff5: 眩晕
            BuffTemplates["buff_005"] = new BuffTemplate
            {
                Gid = "buff_005",
                Name = "眩晕",
                Description = "无法行动",
                Duration = 1,
                MaxStack = 1,
                StackRule = StackRule.Replace,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.Stun,
                        Value = 1,
                        IsPercentage = false,
                        TargetCamp = 1,
                        TargetCount = 2,
                        IsRandomTarget = false,
                        TargetSelectionMode = BuffTargetSelectionMode.PrioritizeSkillTargets
                    }
                ]
            };

            // Buff6: 破甲 - 防御降低
            BuffTemplates["buff_006"] = new BuffTemplate
            {
                Gid = "buff_006",
                Name = "破甲",
                Description = "降低物理防御30%",
                Duration = 3,
                MaxStack = 2,
                StackRule = StackRule.StackValue,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.DefenseDown,
                        Value = 0.30,
                        IsPercentage = true,
                        TargetCamp = 1,
                        TargetCount = 1,
                        IsRandomTarget = false
                    }
                ]
            };

            // Buff7: 铁壁 - 防御提升
            BuffTemplates["buff_007"] = new BuffTemplate
            {
                Gid = "buff_007",
                Name = "铁壁",
                Description = "提升物理防御50点",
                Duration = 3,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.DefenseUp,
                        Value = 50,
                        IsPercentage = false,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = true
                    }
                ]
            };

            // Buff8: 狂暴 - 攻击提升
            BuffTemplates["buff_008"] = new BuffTemplate
            {
                Gid = "buff_008",
                Name = "狂暴",
                Description = "提升物理攻击25%",
                Duration = 3,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.AttackUp,
                        Value = 0.25,
                        IsPercentage = true,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = true
                    }
                ]
            };

            // Buff9: 虚弱 - 攻击降低
            BuffTemplates["buff_009"] = new BuffTemplate
            {
                Gid = "buff_009",
                Name = "虚弱",
                Description = "降低物理攻击20%",
                Duration = 2,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.AttackDown,
                        Value = 0.20,
                        IsPercentage = true,
                        TargetCamp = 1,
                        TargetCount = 1,
                        IsRandomTarget = false
                    }
                ]
            };

            // Buff10: 沉默
            BuffTemplates["buff_010"] = new BuffTemplate
            {
                Gid = "buff_010",
                Name = "沉默",
                Description = "无法使用技能",
                Duration = 2,
                MaxStack = 1,
                StackRule = StackRule.Replace,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.Silence,
                        Value = 1,
                        IsPercentage = false,
                        TargetCamp = 1,
                        TargetCount = 1,
                        IsRandomTarget = false
                    }
                ]
            };

            // Buff11: 复合Buff - 中毒+虚弱
            BuffTemplates["buff_011"] = new BuffTemplate
            {
                Gid = "buff_011",
                Name = "剧毒",
                Description = "持续伤害并降低攻击",
                Duration = 3,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.DamageOverTime,
                        Value = 30,
                        IsPercentage = false,
                        TargetCamp = 1,
                        TargetCount = 1,
                        IsRandomTarget = false
                    },
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.AttackDown,
                        Value = 0.15,
                        IsPercentage = true,
                        TargetCamp = 1,
                        TargetCount = 1,
                        IsRandomTarget = false
                    }
                ]
            };

            // Buff12: 复合Buff - 治疗+防御提升
            BuffTemplates["buff_012"] = new BuffTemplate
            {
                Gid = "buff_012",
                Name = "神圣护盾",
                Description = "持续治疗并提升防御",
                Duration = 3,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.HealOverTime,
                        Value = 60,
                        IsPercentage = false,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = true
                    },
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.DefenseUp,
                        Value = 30,
                        IsPercentage = false,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = true
                    }
                ]
            };

            // Buff15: 冰冻 - 眩晕
            BuffTemplates["buff_015"] = new BuffTemplate
            {
                Gid = "buff_015",
                Name = "冰冻",
                Description = "冻结目标无法行动",
                Duration = 1,
                MaxStack = 1,
                StackRule = StackRule.Replace,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.Stun,
                        Value = 1,
                        IsPercentage = false,
                        TargetCamp = 1,
                        TargetCount = 1,
                        IsRandomTarget = false
                    }
                ]
            };

            // Buff16: 流血 - 持续伤害（可叠加）
            BuffTemplates["buff_016"] = new BuffTemplate
            {
                Gid = "buff_016",
                Name = "流血",
                Description = "持续流血造成伤害",
                Duration = 4,
                MaxStack = 5,
                StackRule = StackRule.StackValue,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.DamageOverTime,
                        Value = 40,
                        IsPercentage = false,
                        TargetCamp = 1,
                        TargetCount = 1,
                        IsRandomTarget = false
                    }
                ]
            };

            // Buff17: 魔法护盾 - 法防提升
            BuffTemplates["buff_017"] = new BuffTemplate
            {
                Gid = "buff_017",
                Name = "魔法护盾",
                Description = "提升法术防御35%",
                Duration = 3,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.DefenseUp,
                        Value = 0.35,
                        IsPercentage = true,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = true
                    }
                ]
            };

            // Buff18: 魔法易伤 - 法防降低
            BuffTemplates["buff_018"] = new BuffTemplate
            {
                Gid = "buff_018",
                Name = "魔法易伤",
                Description = "降低法术防御30%",
                Duration = 3,
                MaxStack = 2,
                StackRule = StackRule.StackValue,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.DefenseDown,
                        Value = 0.30,
                        IsPercentage = true,
                        TargetCamp = 1,
                        TargetCount = 1,
                        IsRandomTarget = false
                    }
                ]
            };

            // Buff19: 嗜血 - 攻击+吸血
            BuffTemplates["buff_019"] = new BuffTemplate
            {
                Gid = "buff_019",
                Name = "嗜血",
                Description = "提升攻击并获得吸血效果",
                Duration = 3,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.AttackUp,
                        Value = 0.20,
                        IsPercentage = true,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = true
                    },
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.Lifesteal,
                        Value = 0.25,
                        IsPercentage = true,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = true
                    }
                ]
            };

            // Buff20: 反伤 - 伤害反弹
            BuffTemplates["buff_020"] = new BuffTemplate
            {
                Gid = "buff_020",
                Name = "荆棘之甲",
                Description = "反弹受到伤害的30%",
                Duration = 3,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.ReflectDamage,
                        Value = 0.30,
                        IsPercentage = true,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = true
                    }
                ]
            };

            // Buff21: 护盾 - 伤害吸收
            BuffTemplates["buff_021"] = new BuffTemplate
            {
                Gid = "buff_021",
                Name = "护盾",
                Description = "吸收300点伤害",
                Duration = 3,
                MaxStack = 1,
                StackRule = StackRule.Replace,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.Shield,
                        Value = 300,
                        IsPercentage = false,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = true
                    }
                ]
            };

            // Buff22: 狂怒 - 攻击大幅提升+防御降低
            BuffTemplates["buff_022"] = new BuffTemplate
            {
                Gid = "buff_022",
                Name = "狂怒",
                Description = "大幅提升攻击但降低防御",
                Duration = 2,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.AttackUp,
                        Value = 0.50,
                        IsPercentage = true,
                        TargetSelfOnly = true
                    },
                    //new BuffEffect
                    //{
                    //    EffectType = BuffEffectType.DefenseDown,
                    //    Value = 0.30,
                    //    IsPercentage = true
                    //}
                ]
            };

            // Buff23: 致盲 - 命中率降低
            BuffTemplates["buff_023"] = new BuffTemplate
            {
                Gid = "buff_023",
                Name = "致盲",
                Description = "降低命中率50%",
                Duration = 2,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.AccuracyDown,
                        Value = 0.50,
                        IsPercentage = true,
                        TargetCamp = 1,
                        TargetCount = 1,
                        IsRandomTarget = false
                    }
                ]
            };

            // Buff24: 专注 - 命中率提升
            BuffTemplates["buff_024"] = new BuffTemplate
            {
                Gid = "buff_024",
                Name = "专注",
                Description = "提升命中率30%",
                Duration = 3,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.AccuracyUp,
                        Value = 0.30,
                        IsPercentage = true,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = true
                    }
                ]
            };

            // Buff25: 暴击强化 - 暴击率提升
            BuffTemplates["buff_025"] = new BuffTemplate
            {
                Gid = "buff_025",
                Name = "暴击强化",
                Description = "提升暴击率20%",
                Duration = 3,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.CritRateUp,
                        Value =0.2,
                        IsPercentage = true,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = true
                    }
                ]
            };

            // Buff26: 免疫 - 免疫控制
            BuffTemplates["buff_026"] = new BuffTemplate
            {
                Gid = "buff_026",
                Name = "免疫",
                Description = "免疫所有控制效果",
                Duration = 2,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.Immunity,
                        Value = 1,
                        IsPercentage = false,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = true
                    }
                ]
            };

            // Buff27: 全能强化 - 攻击和防御提升
            BuffTemplates["buff_027"] = new BuffTemplate
            {
                Gid = "buff_027",
                Name = "全能强化",
                Description = "提升攻击和防御15%",
                Duration = 3,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.AttackUp,
                        Value = 0.15,
                        IsPercentage = true,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = true
                    },
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.DefenseUp,
                        Value = 0.15,
                        IsPercentage = true,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = true
                    }
                ]
            };

            // Buff28: 诅咒 - 攻击和防御降低
            BuffTemplates["buff_028"] = new BuffTemplate
            {
                Gid = "buff_028",
                Name = "诅咒",
                Description = "降低攻击和防御20%",
                Duration = 3,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.AttackDown,
                        Value = 0.20,
                        IsPercentage = true,
                        TargetCamp = 1,
                        TargetCount = 1,
                        IsRandomTarget = false
                    },
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.DefenseDown,
                        Value = 0.20,
                        IsPercentage = true,
                        TargetCamp = 1,
                        TargetCount = 1,
                        IsRandomTarget = false
                    }
                ]
            };

            // Buff29: 魔力涌动 - 持续恢复蓝量
            BuffTemplates["buff_029"] = new BuffTemplate
            {
                Gid = "buff_029",
                Name = "魔力涌动",
                Description = "每回合恢复50点魔法值",
                Duration = 3,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.ManaOverTime,
                        Value = 50,
                        IsPercentage = false,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = true
                    }
                ]
            };

            // Buff30: 魔力枯竭 - 持续消耗蓝量
            BuffTemplates["buff_030"] = new BuffTemplate
            {
                Gid = "buff_030",
                Name = "魔力枯竭",
                Description = "每回合消耗30点魔法值",
                Duration = 3,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.ManaDrain,
                        Value = 30,
                        IsPercentage = false,
                        TargetCamp = 1,
                        TargetCount = 1,
                        IsRandomTarget = false
                    }
                ]
            };

            // Buff31: 伤害增幅 - 伤害提升
            BuffTemplates["buff_031"] = new BuffTemplate
            {
                Gid = "buff_031",
                Name = "伤害增幅",
                Description = "提升造成的伤害25%",
                Duration = 3,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.DamageAmplify,
                        Value = 0.25,
                        IsPercentage = true,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = true
                    }
                ]
            };

            // Buff32: 伤害减免 - 伤害降低
            BuffTemplates["buff_032"] = new BuffTemplate
            {
                Gid = "buff_032",
                Name = "伤害减免",
                Description = "降低受到的伤害20%",
                Duration = 3,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.DamageReduction,
                        Value = 0.20,
                        IsPercentage = true,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = true
                    }
                ]
            };

            // Buff33: 嘲讽 - 强制攻击
            BuffTemplates["buff_033"] = new BuffTemplate
            {
                Gid = "buff_033",
                Name = "嘲讽",
                Description = "强制敌方攻击嘲讽者",
                Duration = 2,
                MaxStack = 1,
                StackRule = StackRule.Replace,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.Taunt,
                        Value = 1,
                        IsPercentage = false,
                        TargetCamp = 1,
                        TargetCount = 1,
                        IsRandomTarget = false
                    }
                ]
            };

            // Buff34: 暴击抵抗 - 降低被暴击率
            BuffTemplates["buff_034"] = new BuffTemplate
            {
                Gid = "buff_034",
                Name = "暴击抵抗",
                Description = "降低被暴击的概率",
                Duration = 3,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.CritResist,
                        Value = 0.20,
                        IsPercentage = true,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = true
                    }
                ]
            };

            // Buff35: 连击率提升
            BuffTemplates["buff_035"] = new BuffTemplate
            {
                Gid = "buff_035",
                Name = "连击强化",
                Description = "提升连击率20%",
                Duration = 3,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.ComboRateUp,
                        Value = 0.20,
                        IsPercentage = true,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = true
                    }
                ]
            };

            // Buff36: 连击伤害提升
            BuffTemplates["buff_036"] = new BuffTemplate
            {
                Gid = "buff_036",
                Name = "连击增幅",
                Description = "提升连击伤害30%",
                Duration = 3,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.ComboDamageUp,
                        Value = 0.30,
                        IsPercentage = true,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = true
                    }
                ]
            };

            // Buff37: 反击率提升
            BuffTemplates["buff_037"] = new BuffTemplate
            {
                Gid = "buff_037",
                Name = "反击强化",
                Description = "提升反击率20%",
                Duration = 3,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.CounterRateUp,
                        Value = 0.20,
                        IsPercentage = true,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = true
                    }
                ]
            };

            // Buff38: 反击伤害提升
            BuffTemplates["buff_038"] = new BuffTemplate
            {
                Gid = "buff_038",
                Name = "反击增幅",
                Description = "提升反击伤害30%",
                Duration = 3,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.CounterDamageUp,
                        Value = 0.30,
                        IsPercentage = true,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = true
                    }
                ]
            };

            // ========== 新增Buff效果 ==========

            // Buff 033: 禁疗 - 目标无法回复生命值
            BuffTemplates["buff_033_heal_block"] = new BuffTemplate
            {
                Gid = "buff_033_heal_block",
                Name = "禁疗",
                Description = "无法回复生命值",
                Duration = 2,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.HealBlock,
                        Value = 0,
                        IsPercentage = false,
                        TargetCamp = 1,
                        TargetCount = 1,
                        IsRandomTarget = false
                    }
                ]
            };

            // Buff 034: 减速 - 降低目标的速度
            BuffTemplates["buff_034_slow"] = new BuffTemplate
            {
                Gid = "buff_034_slow",
                Name = "减速",
                Description = "速度降低30%",
                Duration = 3,
                MaxStack = 2,
                StackRule = StackRule.StackValue,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.Slow,
                        Value = 0.30,
                        IsPercentage = true,
                        TargetCamp = 1,
                        TargetCount = 1,
                        IsRandomTarget = false
                    }
                ]
            };

            // Buff 035: 加速 - 提升目标的速度
            BuffTemplates["buff_035_haste"] = new BuffTemplate
            {
                Gid = "buff_035_haste",
                Name = "加速",
                Description = "速度提升30%",
                Duration = 3,
                MaxStack = 2,
                StackRule = StackRule.StackValue,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.Haste,
                        Value = 0.30,
                        IsPercentage = true,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = false
                    }
                ]
            };

            // Buff 036: 易伤 - 受到的伤害增加
            BuffTemplates["buff_036_vulnerability"] = new BuffTemplate
            {
                Gid = "buff_036_vulnerability",
                Name = "易伤",
                Description = "受到的伤害增加25%",
                Duration = 2,
                MaxStack = 2,
                StackRule = StackRule.StackValue,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.Vulnerability,
                        Value = 0.25,
                        IsPercentage = true,
                        TargetCamp = 1,
                        TargetCount = 1,
                        IsRandomTarget = false
                    }
                ]
            };

            // Buff 037: 不屈 - 致死伤害时保留1点生命
            BuffTemplates["buff_037_undying"] = new BuffTemplate
            {
                Gid = "buff_037_undying",
                Name = "不屈",
                Description = "受到致死伤害时保留1点生命（每场战斗触发一次）",
                Duration = -1,
                MaxStack = 1,
                StackRule = StackRule.Replace,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.Undying,
                        Value = 0,
                        IsPercentage = false,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = true,
                        TargetSelfOnly = true
                    }
                ]
            };

            // Buff 038: 冷却缩减 - 技能冷却减少更快
            BuffTemplates["buff_038_cooldown_reduction"] = new BuffTemplate
            {
                Gid = "buff_038_cooldown_reduction",
                Name = "冷却缩减",
                Description = "技能冷却每回合额外减少1点",
                Duration = 4,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.CooldownReduction,
                        Value = 1,
                        IsPercentage = false,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = true,
                        TargetSelfOnly = true
                    }
                ]
            };

            // Buff 039: 冷却增加 - 技能冷却减少更慢
            BuffTemplates["buff_039_cooldown_increase"] = new BuffTemplate
            {
                Gid = "buff_039_cooldown_increase",
                Name = "冷却延迟",
                Description = "技能冷却每回合减少量-1（最低减少0）",
                Duration = 2,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.CooldownIncrease,
                        Value =1,
                        IsPercentage = false,
                        TargetCamp = 1,
                        TargetCount = 1,
                        IsRandomTarget = false,
                    }
                ]
            };

            // Buff 040: 斩杀 - 对低血量目标造成额外伤害
            BuffTemplates["buff_040_execute"] = new BuffTemplate
            {
                Gid = "buff_040_execute",
                Name = "斩杀意图",
                Description = "对血量低于30%的目标造成额外50%伤害",
                Duration = 3,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.Execute,
                        Value = 0.50,
                        IsPercentage = true,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = true,
                        TargetSelfOnly = true
                    }
                ]
            };

            // ========== 组合Buff示例 ==========

            // Buff 041: 寒冰箭减速 - 伤害+减速
            BuffTemplates["buff_041_frost_arrow"] = new BuffTemplate
            {
                Gid = "buff_041_frost_arrow",
                Name = "寒冰减速",
                Description = "速度降低20%",
                Duration = 2,
                MaxStack = 3,
                StackRule = StackRule.StackValue,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.Slow,
                        Value = 0.20,
                        IsPercentage = true,
                        TargetCamp = 1,
                        TargetCount = 1,
                        IsRandomTarget = false
                    }
                ]
            };

            // Buff 042: 战术弱点 - 易伤+禁疗
            BuffTemplates["buff_042_tactical_weakness"] = new BuffTemplate
            {
                Gid = "buff_042_tactical_weakness",
                Name = "战术弱点",
                Description = "受到的伤害增加30%，无法回复生命",
                Duration = 2,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.Vulnerability,
                        Value = 0.30,
                        IsPercentage = true,
                        TargetCamp = 1,
                        TargetCount = 1,
                        IsRandomTarget = false
                    },
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.HealBlock,
                        Value = 0,
                        IsPercentage = false,
                        TargetCamp = 1,
                        TargetCount = 0,
                        IsRandomTarget = false
                    }
                ]
            };

            // Buff 043: 疾风步 - 加速+冷却缩减
            BuffTemplates["buff_043_wind_step"] = new BuffTemplate
            {
                Gid = "buff_043_wind_step",
                Name = "疾风步",
                Description = "速度提升40%，技能冷却每回合额外减少1点",
                Duration = 3,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.Haste,
                        Value = 0.40,
                        IsPercentage = true,
                        TargetCamp = 2,
                        TargetCount = 0,
                        IsRandomTarget = false
                    },
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.CooldownReduction,
                        Value = 1,
                        IsPercentage = false,
                        TargetCamp = 2,
                        TargetCount = 0,
                        IsRandomTarget = false
                    }
                ]
            };

            // ========== 新增5个Buff效果 ==========

            // Buff 044: 缴械 - 无法使用普通攻击
            BuffTemplates["buff_044_disarm"] = new BuffTemplate
            {
                Gid = "buff_044_disarm",
                Name = "缴械",
                Description = "无法使用普通攻击，只能使用技能",
                Duration = 2,
                MaxStack = 1,
                StackRule = StackRule.Replace,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.Disarm,
                        Value = 0,
                        IsPercentage = false,
                        TargetCamp = 1,
                        TargetCount = 1,
                        IsRandomTarget = false
                    }
                ]
            };

            // Buff 045: 束缚 - 无法反击
            BuffTemplates["buff_045_root"] = new BuffTemplate
            {
                Gid = "buff_045_root",
                Name = "束缚",
                Description = "无法进行反击",
                Duration = 2,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.Root,
                        Value = 0,
                        IsPercentage = false,
                        TargetCamp = 1,
                        TargetCount = 1,
                        IsRandomTarget = false
                    }
                ]
            };

            // Buff 046: 破防 - 直接减少防御值
            BuffTemplates["buff_046_armor_break"] = new BuffTemplate
            {
                Gid = "buff_046_armor_break",
                Name = "破防",
                Description = "防御值降低100点（无视百分比）",
                Duration = 3,
                MaxStack = 3,
                StackRule = StackRule.StackValue,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.ArmorBreak,
                        Value = 100,
                        IsPercentage = false,
                        TargetCamp = 1,
                        TargetCount = 1,
                        IsRandomTarget = false
                    }
                ]
            };

            // Buff 047: 庇护 - 为友方分担伤害
            BuffTemplates["buff_047_sanctuary"] = new BuffTemplate
            {
                Gid = "buff_047_sanctuary",
                Name = "庇护",
                Description = "为附近的友方分担30%受到的伤害",
                Duration = 3,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.Sanctuary,
                        Value = 0.30,
                        IsPercentage = true,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = true,
                        TargetSelfOnly = true
                    }
                ]
            };

            // Buff 048: 复活 - 死亡时自动复活
            BuffTemplates["buff_048_auto_revive"] = new BuffTemplate
            {
                Gid = "buff_048_auto_revive",
                Name = "自动复活",
                Description = "死亡时自动复活并恢复50%最大生命值（每场战斗触发一次）",
                Duration = -1,
                MaxStack = 1,
                StackRule = StackRule.Replace,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.AutoRevive,
                        Value = 0,
                        IsPercentage = false,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = true,
                        TargetSelfOnly = true
                    }
                ]
            };

            #region 被动Buff模板 (Duration = -1 表示永久)

            // ========== 自我强化类被动 ==========

            BuffTemplates["passive_atk_20"] = new BuffTemplate
            {
                Gid = "passive_atk_20",
                Name = "攻击强化",
                Description = "被动：攻击力永久提升20%",
                Duration = -1,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.AttackUp,
                        Value = 0.20f,
                        IsPercentage = true,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = false,
                        TargetSelfOnly = true
                    }
                ]
            };

            BuffTemplates["passive_def_20"] = new BuffTemplate
            {
                Gid = "passive_def_20",
                Name = "防御强化",
                Description = "被动：防御力永久提升20%",
                Duration = -1,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.DefenseUp,
                        Value = 0.20f,
                        IsPercentage = true,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = false,
                        TargetSelfOnly = true
                    }
                ]
            };

            BuffTemplates["passive_speed_15"] = new BuffTemplate
            {
                Gid = "passive_speed_15",
                Name = "速度强化",
                Description = "被动：速度永久提升15%",
                Duration = -1,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.Haste,
                        Value = 0.15f,
                        IsPercentage = true,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = false,
                        TargetSelfOnly = true
                    }
                ]
            };

            BuffTemplates["passive_combo_25"] = new BuffTemplate
            {
                Gid = "passive_combo_25",
                Name = "连击强化",
                Description = "被动：连击率永久提升25%",
                Duration = -1,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.ComboRateUp,
                        Value = 0.25f,
                        IsPercentage = true,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = false,
                        TargetSelfOnly = true
                    }
                ]
            };

            BuffTemplates["passive_counter_25"] = new BuffTemplate
            {
                Gid = "passive_counter_25",
                Name = "反击强化",
                Description = "被动：反击率永久提升25%",
                Duration = -1,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.CounterRateUp,
                        Value = 0.25f,
                        IsPercentage = true,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = false,
                        TargetSelfOnly = true
                    }
                ]
            };

            BuffTemplates["passive_crit_20"] = new BuffTemplate
            {
                Gid = "passive_crit_20",
                Name = "暴击强化",
                Description = "被动：暴击率永久提升20%",
                Duration = -1,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.CritRateUp,
                        Value = 0.20f,
                        IsPercentage = true,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = false,
                        TargetSelfOnly = true
                    }
                ]
            };

            BuffTemplates["passive_lifesteal_15"] = new BuffTemplate
            {
                Gid = "passive_lifesteal_15",
                Name = "吸血强化",
                Description = "被动：获得15%吸血",
                Duration = -1,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.Lifesteal,
                        Value = 0.15f,
                        IsPercentage = true,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = false,
                        TargetSelfOnly = true
                    }
                ]
            };

            BuffTemplates["passive_reflect_20"] = new BuffTemplate
            {
                Gid = "passive_reflect_20",
                Name = "反伤强化",
                Description = "被动：反弹20%受到的伤害",
                Duration = -1,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.ReflectDamage,
                        Value = 0.20f,
                        IsPercentage = true,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = false,
                        TargetSelfOnly = true
                    }
                ]
            };

            // ========== 敌人干扰类被动 ==========

            BuffTemplates["passive_curse_slow"] = new BuffTemplate
            {
                Gid = "passive_curse_slow",
                Name = "减速光环",
                Description = "被动：战斗开始时随机使1-2个敌人减速20%，持续3回合",
                Duration = -1,  // 被动本身永久
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.Slow,
                        Value = 0.20f,
                        IsPercentage = true,
                        TargetCamp = 1,  // 敌方
                        TargetCount = 2,  // 最多2个
                        IsRandomTarget = true,  // 随机选择
                        TriggerChance = 1.0f,  // 100%触发
                        EffectDuration = 3  // ⭐ 施加给敌人的减速持续3回合
                    }
                ]
            };

            BuffTemplates["passive_curse_weak"] = new BuffTemplate
            {
                Gid = "passive_curse_weak",
                Name = "虚弱光环",
                Description = "被动：战斗开始时随机使1个敌人攻击降低15%，持续2回合",
                Duration = -1,  // 被动本身永久
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.AttackDown,
                        Value = 0.15f,
                        IsPercentage = true,
                        TargetCamp = 1,  // 敌方
                        TargetCount = 1,  // 1个
                        IsRandomTarget = true,
                        TriggerChance = 1.0f,
                        EffectDuration = 2  // ⭐ 施加给敌人的虚弱持续2回合
                    }
                ]
            };

            BuffTemplates["passive_curse_vuln"] = new BuffTemplate
            {
                Gid = "passive_curse_vuln",
                Name = "易伤光环",
                Description = "被动：战斗开始时随机使1个敌人易伤25%，持续2回合",
                Duration = -1,  // 被动本身永久
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.Vulnerability,
                        Value = 0.25f,
                        IsPercentage = true,
                        TargetCamp = 1,
                        TargetCount = 1,
                        IsRandomTarget = true,
                        TriggerChance = 1.0f,
                        EffectDuration = 2  // ⭐ 施加给敌人的易伤持续2回合
                    }
                ]
            };

            // ========== 友方光环类被动 ==========

            BuffTemplates["passive_aura_atk"] = new BuffTemplate
            {
                Gid = "passive_aura_atk",
                Name = "攻击光环",
                Description = "被动：战斗开始时全体友方攻击力提升10%（永久）",
                Duration = -1,  // 被动本身永久
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.AttackUp,
                        Value = 0.10f,
                        IsPercentage = true,
                        TargetCamp = 2,  // 己方
                        TargetCount = 0,  // 0表示全体
                        IsRandomTarget = false,
                        TriggerChance = 1.0f,
                        EffectDuration = -1  // ⭐ 使用Template的Duration（永久）
                    }
                ]
            };

            BuffTemplates["passive_aura_def"] = new BuffTemplate
            {
                Gid = "passive_aura_def",
                Name = "防御光环",
                Description = "被动：战斗开始时全体友方防御力提升10%（永久）",
                Duration = -1,  // 被动本身永久
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.DefenseUp,
                        Value = 0.10f,
                        IsPercentage = true,
                        TargetCamp = 2,
                        TargetCount = 0,  // 全体
                        IsRandomTarget = false,
                        TriggerChance = 1.0f,
                        EffectDuration = -1  // ⭐ 使用Template的Duration（永久）
                    }
                ]
            };

            BuffTemplates["passive_aura_speed"] = new BuffTemplate
            {
                Gid = "passive_aura_speed",
                Name = "速度光环",
                Description = "被动：战斗开始时全体友方速度提升10%（永久）",
                Duration = -1,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.Haste,
                        Value = 0.10f,
                        IsPercentage = true,
                        TargetCamp = 2,
                        TargetCount = 0,  // 全体
                        IsRandomTarget = false,
                        TriggerChance = 1.0f,
                        EffectDuration = -1  // ⭐ 使用Template的Duration（永久）
                    }
                ]
            };

            // ========== 特殊类被动 ==========

            BuffTemplates["passive_execute"] = new BuffTemplate
            {
                Gid = "passive_execute",
                Name = "斩杀意图",
                Description = "被动：对血量低于30%的敌人造成额外50%伤害（永久）",
                Duration = -1,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.Execute,
                        Value = 0.50f,
                        IsPercentage = true,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = false,
                        TargetSelfOnly = true,
                        EffectDuration = -1  // ⭐ 使用Template的Duration（永久）
                    }
                ]
            };

            BuffTemplates["passive_undying"] = new BuffTemplate
            {
                Gid = "passive_undying",
                Name = "不屈意志",
                Description = "被动：致死时保留1点生命（每场战斗一次，永久）",
                Duration = -1,
                MaxStack = 1,
                StackRule = StackRule.Replace,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.Undying,
                        Value = 0,
                        IsPercentage = false,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = false,
                        TargetSelfOnly = true,
                        EffectDuration = -1  // ⭐ 使用Template的Duration（永久）
                    }
                ]
            };

            // ========== 无敌类Buff ==========

            BuffTemplates["buff_invincible"] = new BuffTemplate
            {
                Gid = "buff_invincible",
                Name = "无敌",
                Description = "免疫所有伤害，持续1回合",
                Duration = 1,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.Invincible,
                        Value = 1,
                        IsPercentage = false,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = false,
                        TriggerChance = 1.0f,
                        EffectDuration = 1
                    }
                ]
            };

            // 高级无敌（2回合）
            BuffTemplates["buff_invincible_2"] = new BuffTemplate
            {
                Gid = "buff_invincible_2",
                Name = "绝对防御",
                Description = "免疫所有伤害，持续2回合",
                Duration = 2,
                MaxStack = 1,
                StackRule = StackRule.RefreshDuration,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.Invincible,
                        Value = 1,
                        IsPercentage = false,
                        TargetCamp = 2,
                        TargetCount = 1,
                        IsRandomTarget = false,
                        TriggerChance = 1.0f,
                        EffectDuration = 2
                    }
                ]
            };

            // ========== 魅惑类Buff ==========

            // 基础魅惑（1回合）
            BuffTemplates["buff_charm"] = new BuffTemplate
            {
                Gid = "buff_charm",
                Name = "魅惑",
                Description = "被魅惑，攻击友方单位，持续1回合",
                Duration = 1,
                MaxStack = 1,
                StackRule = StackRule.Replace,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.Charm,
                        Value = 1,
                        IsPercentage = false,
                        TargetCamp = 1,
                        TargetCount = 1,
                        IsRandomTarget = false,
                        TriggerChance = 1.0f,
                        EffectDuration = 1
                    }
                ]
            };

            // 深度魅惑（2回合）
            BuffTemplates["buff_charm_deep"] = new BuffTemplate
            {
                Gid = "buff_charm_deep",
                Name = "深度魅惑",
                Description = "被深度魅惑，攻击友方单位，持续2回合",
                Duration = 2,
                MaxStack = 1,
                StackRule = StackRule.Replace,
                Effects =
                [
                    new BuffEffect
                    {
                        EffectType = BuffEffectType.Charm,
                        Value = 1,
                        IsPercentage = false,
                        TargetCamp = 1,
                        TargetCount = 1,
                        IsRandomTarget = false,
                        TriggerChance = 1.0f,
                        EffectDuration = 2
                    }
                ]
            };

            #endregion
        }
        #endregion

    }
}
