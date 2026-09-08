using XXX.Entity;

namespace XXX
{
    /// <summary>
    /// 技能数据类
    /// 提供所有技能数据的初始化和管理功能
    /// </summary>
    /// <remarks>
    /// 主要功能：
    /// - 初始化所有技能数据
    /// - 提供技能的静态访问字典
    /// - 覆盖所有伤害类型和技能效果
    /// 
    /// 技能类型包括：
    /// - 物理伤害技能
    /// - 魔法伤害技能
    /// - 真实伤害技能
    /// - 治疗技能
    /// - 增益Buff技能
    /// - 减益Debuff技能
    /// - 召唤技能
    /// - 多段攻击技能
    /// </remarks>
    public class SkillData
    {
        /// <summary>
        /// 技能字典，Key为技能ID
        /// </summary>
        public static Dictionary<int, Skill> Skills = [];

        public static bool HasLoadedRuntimeTemplates => Skills.Count > 0;

        /// <summary>
        /// 清空运行时技能模板缓存。
        /// </summary>
        public static void ClearRuntimeCaches()
        {
            Skills = [];
        }
        #region 技能初始化
        /// <summary>
        /// 初始化所有技能数据
        /// 覆盖所有伤害类型和技能效果
        /// </summary>
        public static void InitializeSkills()
        {
            ClearRuntimeCaches();
            // 技能1: 重击 - 物理单体伤害
            Skills[1] = new Skill
            {
                Id = 1,
                Name = "重击",
                Description = "对单个敌人造成物理伤害",
                TargetType = 1,
                ManaCost = 20,
                Cooldown = 0,
                DamageType = DamageType.Physical,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 1.5,
                BuffIds = [],
                TriggerChance = 1.0
            };

            // 技能2: 毒刃 - 物理伤害+中毒
            Skills[2] = new Skill
            {
                Id = 2,
                Name = "毒刃",
                Description = "造成物理伤害并附加中毒效果",
                TargetType = 1,
                ManaCost = 35,
                Cooldown = 2,
                DamageType = DamageType.Physical,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 1.2,
                BuffIds = ["buff_001"],
                TriggerChance = 0.8
            };

            // 技能3: 火球术 - 法术单体伤害+灼烧
            Skills[3] = new Skill
            {
                Id = 3,
                Name = "火球术",
                Description = "发射火球造成法术伤害并灼烧敌人",
                TargetType = 1,
                ManaCost = 40,
                Cooldown = 2,
                DamageType = DamageType.Magic,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 1.8,
                BuffIds = ["buff_002"],
                TriggerChance = 0.5
            };

            // 技能4: 治愈术 - 单体治疗
            Skills[4] = new Skill
            {
                Id = 4,
                Name = "治愈术",
                Description = "恢复友方单位生命值",
                TargetType = 2,
                ManaCost = 30,
                Cooldown = 1,
                DamageType = DamageType.Heal,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 1.5,
                BuffIds = [],
                TriggerChance = 1.0
            };

            // 技能5: 群体治疗 - 全体治疗+回春
            Skills[5] = new Skill
            {
                Id = 5,
                Name = "群体治疗",
                Description = "恢复全体友方生命值并附加回春",
                TargetType = 2,
                ManaCost = 60,
                Cooldown = 3,
                DamageType = DamageType.Heal,
                HitCount = 1,
                RangeType = 0,
                DamageMultiplier = 1.0,
                BuffIds = ["buff_003"],
                TriggerChance = 1.0
            };

            // 技能6: 雷击 - 法术群体伤害
            Skills[6] = new Skill
            {
                Id = 6,
                Name = "雷击",
                Description = "召唤雷电攻击全体敌人",
                TargetType = 1,
                ManaCost = 50,
                Cooldown = 3,
                DamageType = DamageType.Magic,
                HitCount = 1,
                RangeType = 0,
                DamageMultiplier = 1.2,
                BuffIds = [],
                TriggerChance = 1.0
            };

            // 技能7: 震慑 - 物理伤害+眩晕
            Skills[7] = new Skill
            {
                Id = 7,
                Name = "震慑",
                Description = "重击敌人造成眩晕",
                TargetType = 1,
                ManaCost = 45,
                Cooldown = 4,
                DamageType = DamageType.Physical,
                HitCount = 1,
                RangeType = 2,
                DamageMultiplier = 1.0,
                BuffIds = ["buff_005"],
                TriggerChance = 0.6
            };

            // 技能8: 破甲斩 - 物理伤害+破甲
            Skills[8] = new Skill
            {
                Id = 8,
                Name = "破甲斩",
                Description = "撕裂敌人护甲降低防御",
                TargetType = 1,
                ManaCost = 30,
                Cooldown = 4,
                DamageType = DamageType.Physical,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 1.3,
                BuffIds = ["buff_006"],
                TriggerChance = 0.5
            };

            // 技能9: 铁壁守护 - 自身防御提升
            Skills[9] = new Skill
            {
                Id = 9,
                Name = "铁壁守护",
                Description = "提升自身防御力",
                TargetType = 2,
                ManaCost = 25,
                Cooldown = 3,
                DamageType = DamageType.Heal,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 0,
                BuffIds = ["buff_007"],
                TriggerChance = 1.0
            };

            // 技能10: 狂暴之力 - 攻击提升
            Skills[10] = new Skill
            {
                Id = 10,
                Name = "狂暴之力",
                Description = "激发潜能提升攻击力",
                TargetType = 2,
                ManaCost = 30,
                Cooldown = 3,
                DamageType = DamageType.Heal,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 0,
                BuffIds = ["buff_008"],
                TriggerChance = 0.5
            };

            // 技能11: 虚弱诅咒 - 降低敌人攻击
            Skills[11] = new Skill
            {
                Id = 11,
                Name = "虚弱诅咒",
                Description = "诅咒敌人降低其攻击力",
                TargetType = 1,
                ManaCost = 25,
                Cooldown = 2,
                DamageType = DamageType.Magic,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 0.5,
                BuffIds = ["buff_009"],
                TriggerChance = 1.0
            };

            // 技能12: 沉默之印 - 沉默敌人
            Skills[12] = new Skill
            {
                Id = 12,
                Name = "沉默之印",
                Description = "封印敌人使其无法使用技能",
                TargetType = 1,
                ManaCost = 35,
                Cooldown = 4,
                DamageType = DamageType.Magic,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 0.8,
                BuffIds = ["buff_010"],
                TriggerChance = 0.7
            };

            // 技能13: 剧毒之刺 - 剧毒效果
            Skills[13] = new Skill
            {
                Id = 13,
                Name = "剧毒之刺",
                Description = "释放剧毒造成持续伤害和攻击降低",
                TargetType = 1,
                ManaCost = 40,
                Cooldown = 3,
                DamageType = DamageType.Physical,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 1.0,
                BuffIds = ["buff_011"],
                TriggerChance = 0.75
            };

            // 技能14: 神圣庇护 - 治疗+防御
            Skills[14] = new Skill
            {
                Id = 14,
                Name = "神圣庇护",
                Description = "神圣之力治疗并提升防御",
                TargetType = 2,
                ManaCost = 50,
                Cooldown = 4,
                DamageType = DamageType.Heal,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 1.2,
                BuffIds = ["buff_012"],
                TriggerChance = 1.0
            };

            // 技能15: 真实打击 - 真实伤害
            Skills[15] = new Skill
            {
                Id = 15,
                Name = "真实打击",
                Description = "造成无视防御的真实伤害",
                TargetType = 1,
                ManaCost = 55,
                Cooldown = 4,
                DamageType = DamageType.True,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 1.0,
                BuffIds = [],
                TriggerChance = 1.0
            };

            // 技能16: 连环斩 - 多段物理伤害
            Skills[16] = new Skill
            {
                Id = 16,
                Name = "连环斩",
                Description = "连续攻击敌人3次",
                TargetType = 1,
                ManaCost = 45,
                Cooldown = 3,
                DamageType = DamageType.Physical,
                HitCount = 3,
                RangeType = 1,
                DamageMultiplier = 0.6,
                BuffIds = [],
                TriggerChance = 1.0
            };

            // 技能17: 冰霜新星 - 法术群体伤害
            Skills[17] = new Skill
            {
                Id = 17,
                Name = "冰霜新星",
                Description = "冰霜爆发攻击全体敌人",
                TargetType = 1,
                ManaCost = 55,
                Cooldown = 3,
                DamageType = DamageType.Magic,
                HitCount = 1,
                RangeType = 0,
                DamageMultiplier = 1.0,
                BuffIds = [],
                TriggerChance = 1.0
            };

            // 技能19: 冰封 - 冰冻敌人
            Skills[19] = new Skill
            {
                Id = 19,
                Name = "冰封",
                Description = "冻结敌人使其无法行动",
                TargetType = 1,
                ManaCost = 50,
                Cooldown = 5,
                DamageType = DamageType.Magic,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 0.8,
                BuffIds = ["buff_015"],
                TriggerChance = 0.65
            };

            // 技能20: 撕裂 - 物理伤害+流血
            Skills[20] = new Skill
            {
                Id = 20,
                Name = "撕裂",
                Description = "撕裂敌人造成流血",
                TargetType = 1,
                ManaCost = 35,
                Cooldown = 2,
                DamageType = DamageType.Physical,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 1.4,
                BuffIds = ["buff_016"],
                TriggerChance = 0.9
            };

            // 技能21: 魔法屏障 - 法防提升
            Skills[21] = new Skill
            {
                Id = 21,
                Name = "魔法屏障",
                Description = "提升法术防御力",
                TargetType = 2,
                ManaCost = 30,
                Cooldown = 3,
                DamageType = DamageType.Heal,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 0,
                BuffIds = ["buff_017"],
                TriggerChance = 1.0
            };

            // 技能22: 魔法侵蚀 - 法术伤害+法防降低
            Skills[22] = new Skill
            {
                Id = 22,
                Name = "魔法侵蚀",
                Description = "侵蚀敌人降低法术防御",
                TargetType = 1,
                ManaCost = 40,
                Cooldown = 2,
                DamageType = DamageType.Magic,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 1.3,
                BuffIds = ["buff_018"],
                TriggerChance = 0.85
            };

            // 技能23: 嗜血狂暴 - 攻击提升+吸血
            Skills[23] = new Skill
            {
                Id = 23,
                Name = "嗜血狂暴",
                Description = "激发嗜血本能",
                TargetType = 2,
                ManaCost = 40,
                Cooldown = 4,
                DamageType = DamageType.Heal,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 0,
                BuffIds = ["buff_019"],
                TriggerChance = 1.0
            };

            // 技能24: 荆棘护甲 - 反伤
            Skills[24] = new Skill
            {
                Id = 24,
                Name = "荆棘护甲",
                Description = "反弹受到的伤害",
                TargetType = 2,
                ManaCost = 35,
                Cooldown = 4,
                DamageType = DamageType.Heal,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 0,
                BuffIds = ["buff_020"],
                TriggerChance = 1.0
            };

            // 技能25: 圣光护盾 - 护盾
            Skills[25] = new Skill
            {
                Id = 25,
                Name = "圣光护盾",
                Description = "为目标施加护盾",
                TargetType = 2,
                ManaCost = 45,
                Cooldown = 3,
                DamageType = DamageType.Heal,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 0,
                BuffIds = ["buff_021"],
                TriggerChance = 1.0
            };

            // 技能26: 狂怒打击 - 物理伤害+狂怒
            Skills[26] = new Skill
            {
                Id = 26,
                Name = "狂怒打击",
                Description = "狂怒状态大幅提升攻击",
                TargetType = 1,
                ManaCost = 50,
                Cooldown = 4,
                DamageType = DamageType.Physical,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 1.0,
                BuffIds = ["buff_022"],
                TriggerChance = 1.0
            };

            // 技能27: 闪光弹 - 致盲
            Skills[27] = new Skill
            {
                Id = 27,
                Name = "闪光弹",
                Description = "致盲敌人降低命中",
                TargetType = 1,
                ManaCost = 30,
                Cooldown = 3,
                DamageType = DamageType.Magic,
                HitCount = 1,
                RangeType = 0,
                DamageMultiplier = 0.5,
                BuffIds = ["buff_023"],
                TriggerChance = 0.9
            };

            // 技能28: 鹰眼 - 命中提升
            Skills[28] = new Skill
            {
                Id = 28,
                Name = "鹰眼",
                Description = "提升命中率",
                TargetType = 2,
                ManaCost = 25,
                Cooldown = 3,
                DamageType = DamageType.Heal,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 0,
                BuffIds = ["buff_024"],
                TriggerChance = 1.0
            };

            // 技能29: 致命一击 - 物理伤害+暴击强化
            Skills[29] = new Skill
            {
                Id = 29,
                Name = "致命一击",
                Description = "提升暴击率的强力攻击",
                TargetType = 1,
                ManaCost = 40,
                Cooldown = 3,
                DamageType = DamageType.Physical,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 1.6,
                BuffIds = ["buff_025"],
                TriggerChance = 1.0
            };

            // 技能30: 神圣祝福 - 免疫控制
            Skills[30] = new Skill
            {
                Id = 30,
                Name = "神圣祝福",
                Description = "免疫所有控制效果",
                TargetType = 2,
                ManaCost = 50,
                Cooldown = 5,
                DamageType = DamageType.Heal,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 0,
                BuffIds = ["buff_026"],
                TriggerChance = 1.0
            };

            // 技能31: 全能祝福 - 全属性提升
            Skills[31] = new Skill
            {
                Id = 31,
                Name = "全能祝福",
                Description = "提升所有属性",
                TargetType = 2,
                ManaCost = 60,
                Cooldown = 5,
                DamageType = DamageType.Heal,
                HitCount = 1,
                RangeType = 0,
                DamageMultiplier = 0,
                BuffIds = ["buff_027"],
                TriggerChance = 1.0
            };

            // 技能32: 黑暗诅咒 - 全属性降低
            Skills[32] = new Skill
            {
                Id = 32,
                Name = "黑暗诅咒",
                Description = "诅咒敌人降低所有属性",
                TargetType = 1,
                ManaCost = 55,
                Cooldown = 4,
                DamageType = DamageType.Magic,
                HitCount = 1,
                RangeType = 0,
                DamageMultiplier = 0.8,
                BuffIds = ["buff_028"],
                TriggerChance = 0.8
            };

            // 技能33: 魔力充能 - 恢复蓝量
            Skills[33] = new Skill
            {
                Id = 33,
                Name = "魔力充能",
                Description = "持续恢复魔法值",
                TargetType = 2,
                ManaCost = 20,
                Cooldown = 3,
                DamageType = DamageType.Heal,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 0,
                BuffIds = ["buff_029"],
                TriggerChance = 1.0
            };

            // 技能34: 魔力吸取 - 法术伤害+魔力枯竭
            Skills[34] = new Skill
            {
                Id = 34,
                Name = "魔力吸取",
                Description = "吸取敌人魔法值",
                TargetType = 1,
                ManaCost = 35,
                Cooldown = 3,
                DamageType = DamageType.Magic,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 1.0,
                BuffIds = ["buff_030"],
                TriggerChance = 0.85
            };

            // 技能35: 陨石术 - 法术群体高伤害
            Skills[35] = new Skill
            {
                Id = 35,
                Name = "陨石术",
                Description = "召唤陨石攻击全体敌人",
                TargetType = 1,
                ManaCost = 80,
                Cooldown = 5,
                DamageType = DamageType.Magic,
                HitCount = 1,
                RangeType = 0,
                DamageMultiplier = 2.0,
                BuffIds = [],
                TriggerChance = 1.0
            };

            // 技能36: 旋风斩 - 物理群体伤害
            Skills[36] = new Skill
            {
                Id = 36,
                Name = "旋风斩",
                Description = "旋转攻击周围所有敌人",
                TargetType = 1,
                ManaCost = 60,
                Cooldown = 4,
                DamageType = DamageType.Physical,
                HitCount = 1,
                RangeType = 0,
                DamageMultiplier = 1.5,
                BuffIds = [],
                TriggerChance = 1.0
            };

            // 技能37: 复活术 - 复活友方
            Skills[37] = new Skill
            {
                Id = 37,
                Name = "复活术",
                Description = "复活倒下的友方单位",
                TargetType = 2,
                ManaCost = 100,
                Cooldown = 10,
                DamageType = DamageType.Revive,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 0.5,
                BuffIds = [],
                TriggerChance = 1.0
            };

            // 技能38: 狂风暴雨 - 多段法术群体伤害
            Skills[38] = new Skill
            {
                Id = 38,
                Name = "狂风暴雨",
                Description = "连续攻击全体敌人5次",
                TargetType = 1,
                ManaCost = 70,
                Cooldown = 4,
                DamageType = DamageType.Magic,
                HitCount = 5,
                RangeType = 0,
                DamageMultiplier = 0.4,
                BuffIds = [],
                TriggerChance = 1.0
            };

            // 技能39: 终极治疗 - 全体大量治疗
            Skills[39] = new Skill
            {
                Id = 39,
                Name = "终极治疗",
                Description = "大量恢复全体友方生命值",
                TargetType = 2,
                ManaCost = 80,
                Cooldown = 5,
                DamageType = DamageType.Heal,
                HitCount = 1,
                RangeType = 0,
                DamageMultiplier = 2.5,
                BuffIds = [],
                TriggerChance = 1.0
            };

            // 技能40: 毁灭打击 - 真实伤害群体
            Skills[40] = new Skill
            {
                Id = 40,
                Name = "毁灭打击",
                Description = "对全体敌人造成真实伤害",
                TargetType = 1,
                ManaCost = 90,
                Cooldown = 6,
                DamageType = DamageType.True,
                HitCount = 1,
                RangeType = 0,
                DamageMultiplier = 1.5,
                BuffIds = [],
                TriggerChance = 1.0
            };

            // 技能41: 连环火球 - 多段法术单体
            Skills[41] = new Skill
            {
                Id = 41,
                Name = "连环火球",
                Description = "连续发射4个火球",
                TargetType = 1,
                ManaCost = 55,
                Cooldown = 3,
                DamageType = DamageType.Magic,
                HitCount = 4,
                RangeType = 1,
                DamageMultiplier = 0.5,
                BuffIds = ["buff_002"],
                TriggerChance = 0.6
            };

            // 技能42: 剧毒爆发 - 群体中毒
            Skills[42] = new Skill
            {
                Id = 42,
                Name = "剧毒爆发",
                Description = "释放剧毒攻击全体敌人",
                TargetType = 1,
                ManaCost = 65,
                Cooldown = 4,
                DamageType = DamageType.Magic,
                HitCount = 1,
                RangeType = 0,
                DamageMultiplier = 1.0,
                BuffIds = ["buff_011"],
                TriggerChance = 0.8
            };

            // 技能43: 群体护盾 - 全体护盾
            Skills[43] = new Skill
            {
                Id = 43,
                Name = "群体护盾",
                Description = "为全体友方施加护盾",
                TargetType = 2,
                ManaCost = 70,
                Cooldown = 5,
                DamageType = DamageType.Heal,
                HitCount = 1,
                RangeType = 0,
                DamageMultiplier = 0,
                BuffIds = ["buff_021"],
                TriggerChance = 1.0
            };

            // 技能44: 血祭 - 消耗生命造成高伤害
            Skills[44] = new Skill
            {
                Id = 44,
                Name = "血祭",
                Description = "消耗自身生命造成巨额伤害",
                TargetType = 1,
                ManaCost = 0,
                Cooldown = 5,
                DamageType = DamageType.BloodSacrifice,  // 血祭技能
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 3.0,
                BuffIds = [],
                TriggerChance = 1.0
            };

            // 技能47: 生命转换 - 治疗+伤害
            Skills[47] = new Skill
            {
                Id = 47,
                Name = "生命转换",
                Description = "吸取敌人生命治疗自己",
                TargetType = 1,
                ManaCost = 45,
                Cooldown = 3,
                DamageType = DamageType.LifeConversion,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 1.5,
                BuffIds = [],
                TriggerChance = 1.0
            };

            // 技能48: 反击姿态 - 提升反击
            Skills[48] = new Skill
            {
                Id = 48,
                Name = "反击姿态",
                Description = "进入反击姿态",
                TargetType = 2,
                ManaCost = 30,
                Cooldown = 4,
                DamageType = DamageType.Heal,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 0,
                BuffIds = ["buff_020"],
                TriggerChance = 1.0
            };

            // 技能49: 终极爆发 - 复合高伤害
            Skills[49] = new Skill
            {
                Id = 49,
                Name = "终极爆发",
                Description = "释放全部力量造成毁灭性伤害",
                TargetType = 1,
                ManaCost = 100,
                Cooldown = 8,
                DamageType = DamageType.Physical,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 4.0,
                BuffIds = [],
                TriggerChance = 1.0
            };

            // 技能50: 神圣审判 - 法术+多重Buff
            Skills[50] = new Skill
            {
                Id = 50,
                Name = "神圣审判",
                Description = "神圣之力审判敌人",
                TargetType = 1,
                ManaCost = 85,
                Cooldown = 6,
                DamageType = DamageType.Magic,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 2.5,
                BuffIds = ["buff_009", "buff_018", "buff_023"],
                TriggerChance = 0.9
            };

            // 技能51: 连环斩 - 多段物理伤害（新版本，使用Hits）
            Skills[51] = new Skill
            {
                Id = 51,
                Name = "连环斩(多段)",
                Description = "连续攻击敌人两次，第一次造成85%+500伤害，第二次造成135%+1000伤害",
                TargetType = 1,
                ManaCost = 30,
                Cooldown = 2,
                DamageType = DamageType.Physical,
                RangeType = 1,
                TriggerChance = 1.0,
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

            // 技能52: 元素爆发 - 三段不同伤害类型
            Skills[52] = new Skill
            {
                Id = 52,
                Name = "元素爆发",
                Description = "三段元素伤害，每段附带不同效果",
                TargetType = 1,
                ManaCost = 50,
                Cooldown = 3,
                DamageType = DamageType.Magic,
                RangeType = 0,
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
                BuffIds = ["buff_002"]
            };

            // 技能53: 治疗连击 - 两段治疗
            Skills[53] = new Skill
            {
                Id = 53,
                Name = "治疗连击",
                Description = "连续治疗目标两次，第一次恢复80%法术攻击，第二次恢复120%法术攻击",
                TargetType = 2,
                ManaCost = 40,
                Cooldown = 1,
                DamageType = DamageType.Heal,
                RangeType = 1,
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

            // 技能54: 蓄力一击 - 单段高伤害
            Skills[54] = new Skill
            {
                Id = 54,
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

            // 技能55: 多段治疗+护盾
            Skills[55] = new Skill
            {
                Id = 55,
                Name = "神圣连击",
                Description = "连续治疗并施加护盾",
                TargetType = 2,
                ManaCost = 60,
                Cooldown = 4,
                DamageType = DamageType.Heal,
                RangeType = 1,
                TriggerChance = 1.0,
                Hits =
                [
                    new SkillHit
                    {
                        DamageMultiplier = 1.0,
                        BaseDamage = 0,
                        HitDamageType = DamageType.Heal,
                        Description = "第一段：恢复生命"
                    },
                    new SkillHit
                    {
                        DamageMultiplier = 0,
                        BaseDamage = 0,
                        HitDamageType = DamageType.Heal,
                        Description = "第二段：施加护盾"
                    }
                ],
                BuffIds = ["buff_021"]
            };

            // 技能56: 连环毒刃 - 多段带毒
            Skills[56] = new Skill
            {
                Id = 56,
                Name = "连环毒刃",
                Description = "连续攻击并附加中毒",
                TargetType = 1,
                ManaCost = 45,
                Cooldown = 3,
                DamageType = DamageType.Physical,
                RangeType = 1,
                TriggerChance = 0.8,
                Hits =
                [
                    new SkillHit
                    {
                        DamageMultiplier = 0.7,
                        BaseDamage = 200,
                        Description = "第一击：70%攻击力 + 200伤害"
                    },
                    new SkillHit
                    {
                        DamageMultiplier = 0.9,
                        BaseDamage = 300,
                        Description = "第二击：90%攻击力 + 300伤害"
                    }
                ],
                BuffIds = ["buff_001"]
            };

            // 技能57: 连环火球 - 多段法术
            Skills[57] = new Skill
            {
                Id = 57,
                Name = "连环火球",
                Description = "连续发射4个火球",
                TargetType = 1,
                ManaCost = 55,
                Cooldown = 3,
                DamageType = DamageType.Magic,
                RangeType = 1,
                TriggerChance = 0.6,
                Hits =
                [
                    new SkillHit
                    {
                        DamageMultiplier = 0.5,
                        BaseDamage = 100,
                        HitDamageType = DamageType.Magic,
                        Description = "火球1：50%法攻 + 100伤害"
                    },
                    new SkillHit
                    {
                        DamageMultiplier = 0.5,
                        BaseDamage = 100,
                        HitDamageType = DamageType.Magic,
                        Description = "火球2：50%法攻 + 100伤害"
                    },
                    new SkillHit
                    {
                        DamageMultiplier = 0.5,
                        BaseDamage = 100,
                        HitDamageType = DamageType.Magic,
                        Description = "火球3：50%法攻 + 100伤害"
                    },
                    new SkillHit
                    {
                        DamageMultiplier = 0.5,
                        BaseDamage = 100,
                        HitDamageType = DamageType.Magic,
                        Description = "火球4：50%法攻 + 100伤害"
                    }
                ],
                BuffIds = ["buff_002"]
            };

            // 技能58: 狂风暴雨 - 5段群体伤害
            Skills[58] = new Skill
            {
                Id = 58,
                Name = "狂风暴雨",
                Description = "连续攻击全体敌人5次",
                TargetType = 1,
                ManaCost = 70,
                Cooldown = 4,
                DamageType = DamageType.Magic,
                RangeType = 0,
                TriggerChance = 1.0,
                Hits =
                [
                    new SkillHit
                    {
                        DamageMultiplier = 0.4,
                        BaseDamage = 50,
                        Description = "第1击：40%法攻 + 50伤害"
                    },
                    new SkillHit
                    {
                        DamageMultiplier = 0.4,
                        BaseDamage = 50,
                        Description = "第2击：40%法攻 + 50伤害"
                    },
                    new SkillHit
                    {
                        DamageMultiplier = 0.4,
                        BaseDamage = 50,
                        Description = "第3击：40%法攻 + 50伤害"
                    },
                    new SkillHit
                    {
                        DamageMultiplier = 0.4,
                        BaseDamage = 50,
                        Description = "第4击：40%法攻 + 50伤害"
                    },
                    new SkillHit
                    {
                        DamageMultiplier = 0.4,
                        BaseDamage = 50,
                        Description = "第5击：40%法攻 + 50伤害"
                    }
                ]
            };

            // 技能59: 旋风斩 - 3段物理群体
            Skills[59] = new Skill
            {
                Id = 59,
                Name = "旋风斩",
                Description = "旋转攻击周围所有敌人3次",
                TargetType = 1,
                ManaCost = 60,
                Cooldown = 4,
                DamageType = DamageType.Physical,
                RangeType = 0,
                TriggerChance = 1.0,
                Hits =
                [
                    new SkillHit
                    {
                        DamageMultiplier = 0.5,
                        BaseDamage = 100,
                        Description = "第1击：50%物攻 + 100伤害"
                    },
                    new SkillHit
                    {
                        DamageMultiplier = 0.5,
                        BaseDamage = 100,
                        Description = "第2击：50%物攻 + 100伤害"
                    },
                    new SkillHit
                    {
                        DamageMultiplier = 0.5,
                        BaseDamage = 100,
                        Description = "第3击：50%物攻 + 100伤害"
                    }
                ]
            };

            // 技能60: 神圣审判 - 多段+多重Buff
            Skills[60] = new Skill
            {
                Id = 60,
                Name = "神圣审判",
                Description = "神圣之力审判敌人，造成多段伤害并附加多重负面效果",
                TargetType = 1,
                ManaCost = 85,
                Cooldown = 6,
                DamageType = DamageType.Magic,
                RangeType = 1,
                TriggerChance = 0.9,
                Hits =
                [
                    new SkillHit
                    {
                        DamageMultiplier = 1.0,
                        BaseDamage = 200,
                        Description = "第一击：100%法攻 + 200伤害"
                    },
                    new SkillHit
                    {
                        DamageMultiplier = 1.5,
                        BaseDamage = 300,
                        Description = "第二击：150%法攻 + 300伤害"
                    }
                ],
                BuffIds = ["buff_009", "buff_018", "buff_023"]
            };

            // ========== 新增技能（使用新Buff效果） ==========

            // 技能61: 寒冰箭 - 物理伤害+减速
            Skills[61] = new Skill
            {
                Id = 61,
                Name = "寒冰箭",
                Description = "射出寒冰箭造成物理伤害并降低目标速度",
                TargetType = 1,
                ManaCost = 25,
                Cooldown = 2,
                DamageType = DamageType.Physical,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 1.3,
                BuffIds = ["buff_041_frost_arrow"],
                TriggerChance = 1.0
            };

            // 技能62: 战术标记 - 易伤+禁疗
            Skills[62] = new Skill
            {
                Id = 62,
                Name = "战术标记",
                Description = "标记敌人，使其受到更多伤害且无法回复生命",
                TargetType = 1,
                ManaCost = 30,
                Cooldown = 3,
                DamageType = DamageType.Buff,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 0,
                BuffIds = ["buff_042_tactical_weakness"],
                TriggerChance = 0.8
            };

            // 技能63: 疾风步 - 加速+冷却缩减
            Skills[63] = new Skill
            {
                Id = 63,
                Name = "疾风步",
                Description = "提升自身速度，加快技能冷却",
                TargetType = 2,
                ManaCost = 35,
                Cooldown = 4,
                DamageType = DamageType.Buff,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 0,
                BuffIds = ["buff_043_wind_step"],
                TriggerChance = 1.0
            };

            // 技能64: 不屈意志 - 获得不屈效果
            Skills[64] = new Skill
            {
                Id = 64,
                Name = "不屈意志",
                Description = "激发潜能，获得不屈效果",
                TargetType = 2,
                ManaCost = 50,
                Cooldown = 5,
                DamageType = DamageType.Buff,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 0,
                BuffIds = ["buff_037_undying"],
                TriggerChance = 1.0
            };

            // 技能65: 斩杀一击 - 对低血量目标额外伤害
            Skills[65] = new Skill
            {
                Id = 65,
                Name = "斩杀一击",
                Description = "对低血量目标造成大量物理伤害",
                TargetType = 1,
                ManaCost = 40,
                Cooldown = 3,
                DamageType = DamageType.Physical,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 2.0,
                BuffIds = ["buff_040_execute"],
                TriggerChance = 1.0
            };

            // 技能66: 禁咒 - 禁止目标回复生命
            Skills[66] = new Skill
            {
                Id = 66,
                Name = "禁咒",
                Description = "诅咒目标，使其无法回复生命",
                TargetType = 1,
                ManaCost = 20,
                Cooldown = 2,
                DamageType = DamageType.Buff,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 0,
                BuffIds = ["buff_033_heal_block"],
                TriggerChance = 0.9
            };

            // 技能67: 时间扭曲 - 群体加速
            Skills[67] = new Skill
            {
                Id = 67,
                Name = "时间扭曲",
                Description = "扭曲时间，全体友方速度提升",
                TargetType = 2,
                ManaCost = 50,
                Cooldown = 4,
                DamageType = DamageType.Buff,
                HitCount = 1,
                RangeType = 0,
                DamageMultiplier = 0,
                BuffIds = ["buff_035_haste"],
                TriggerChance = 1.0
            };

            // 技能68: 泥沼陷阱 - 群体减速
            Skills[68] = new Skill
            {
                Id = 68,
                Name = "泥沼陷阱",
                Description = "布置陷阱，降低全体敌方速度",
                TargetType = 1,
                ManaCost = 45,
                Cooldown = 4,
                DamageType = DamageType.Buff,
                HitCount = 1,
                RangeType = 0,
                DamageMultiplier = 0,
                BuffIds = ["buff_034_slow"],
                TriggerChance = 0.85
            };

            // 技能69: 法力燃烧 - 冷却延迟
            Skills[69] = new Skill
            {
                Id = 69,
                Name = "法力燃烧",
                Description = "干扰敌人，使其技能冷却变慢",
                TargetType = 1,
                ManaCost = 30,
                Cooldown = 3,
                DamageType = DamageType.Buff,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 0,
                BuffIds = ["buff_039_cooldown_increase"],
                TriggerChance = 0.8
            };

            // 技能70: 急速冷却 - 自身冷却缩减
            Skills[70] = new Skill
            {
                Id = 70,
                Name = "急速冷却",
                Description = "加速自身技能冷却",
                TargetType = 2,
                ManaCost = 25,
                Cooldown = 2,
                DamageType = DamageType.Buff,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 0,
                BuffIds = ["buff_038_cooldown_reduction"],
                TriggerChance = 1.0
            };

            // ========== 新增5个Buff效果的技能 ==========

            // 技能71: 缴械射击 - 物理伤害+缴械
            Skills[71] = new Skill
            {
                Id = 71,
                Name = "缴械射击",
                Description = "射击敌人武器，使其无法使用普通攻击",
                TargetType = 1,
                ManaCost = 30,
                Cooldown = 4,
                DamageType = DamageType.Physical,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 1.2,
                BuffIds = ["buff_044_disarm"],
                TriggerChance = 0.75
            };

            // 技能72: 土牢术 - 法术伤害+束缚
            Skills[72] = new Skill
            {
                Id = 72,
                Name = "土牢术",
                Description = "召唤土牢困住敌人，使其无法反击",
                TargetType = 1,
                ManaCost = 35,
                Cooldown = 3,
                DamageType = DamageType.Magic,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 1.0,
                BuffIds = ["buff_045_root"],
                TriggerChance = 0.8
            };

            // 技能73: 破防重击 - 物理伤害+破防
            Skills[73] = new Skill
            {
                Id = 73,
                Name = "破防重击",
                Description = "重击敌人护甲，直接降低其防御值",
                TargetType = 1,
                ManaCost = 40,
                Cooldown = 3,
                DamageType = DamageType.Physical,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 1.5,
                BuffIds = ["buff_046_armor_break"],
                TriggerChance = 0.9
            };

            // 技能74: 守护誓言 - 为友方提供庇护
            Skills[74] = new Skill
            {
                Id = 74,
                Name = "守护誓言",
                Description = "立下誓言，为附近的友方分担30%受到的伤害",
                TargetType = 2,
                ManaCost = 45,
                Cooldown = 5,
                DamageType = DamageType.Buff,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 0,
                BuffIds = ["buff_047_sanctuary"],
                TriggerChance = 1.0
            };

            // 技能75: 复活誓言 - 获得自动复活效果
            Skills[75] = new Skill
            {
                Id = 75,
                Name = "复活誓言",
                Description = "立下复活誓言，死亡时自动复活并恢复50%生命",
                TargetType = 2,
                ManaCost = 60,
                Cooldown = 8,
                DamageType = DamageType.Buff,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 0,
                BuffIds = ["buff_048_auto_revive"],
                TriggerChance = 1.0
            };

            // ========== 新增无敌、驱散、净化、百分比伤害技能 ==========

            // 技能76: 圣光驱散 - 驱散敌人有益Buff
            Skills[76] = new Skill
            {
                Id = 76,
                Name = "圣光驱散",
                Description = "驱散目标2个有益效果",
                TargetType = 1,
                ManaCost = 30,
                Cooldown = 4,
                DamageType = DamageType.Dispel,
                HitCount = 2,  // 驱散2个Buff
                RangeType = 1,
                DamageMultiplier = 0,
                BuffIds = [],
                TriggerChance = 1.0
            };

            // 技能77: 净化之光 - 净化友方有害Buff
            Skills[77] = new Skill
            {
                Id = 77,
                Name = "净化之光",
                Description = "净化目标2个有害效果",
                TargetType = 2,
                ManaCost = 25,
                Cooldown = 3,
                DamageType = DamageType.Cleanse,
                HitCount = 2,  // 净化2个Buff
                RangeType = 1,
                DamageMultiplier = 0,
                BuffIds = [],
                TriggerChance = 1.0
            };

            // 技能78: 生命凋零 - 百分比伤害
            Skills[78] = new Skill
            {
                Id = 78,
                Name = "生命凋零",
                Description = "对目标造成最大生命值15%的伤害（不超过施法者攻击力300%）",
                TargetType = 1,
                ManaCost = 40,
                Cooldown = 4,
                DamageType = DamageType.PercentDamage,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 0.15,  // 15%最大生命值
                BuffIds = [],
                TriggerChance = 1.0
            };

            // 技能79: 神圣护盾 - 获得无敌效果
            Skills[79] = new Skill
            {
                Id = 79,
                Name = "神圣护盾",
                Description = "获得无敌效果，免疫所有伤害1回合",
                TargetType = 2,
                ManaCost = 50,
                Cooldown = 6,
                DamageType = DamageType.Buff,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 0,
                BuffIds = ["buff_invincible"],
                TriggerChance = 1.0
            };

            // 技能80: 绝对防御 - 2回合无敌
            Skills[80] = new Skill
            {
                Id = 80,
                Name = "绝对防御",
                Description = "获得绝对防御，免疫所有伤害2回合（高阶技能）",
                TargetType = 2,
                ManaCost = 80,
                Cooldown = 10,
                DamageType = DamageType.Buff,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 0,
                BuffIds = ["buff_invincible_2"],
                TriggerChance = 1.0
            };

            // 技能81: 强力驱散 - 驱散更多Buff
            Skills[81] = new Skill
            {
                Id = 81,
                Name = "强力驱散",
                Description = "驱散目标所有有益效果（高阶驱散）",
                TargetType = 1,
                ManaCost = 50,
                Cooldown = 6,
                DamageType = DamageType.Dispel,
                HitCount = 99,  // 驱散所有（设置一个足够大的数）
                RangeType = 1,
                DamageMultiplier = 0,
                BuffIds = [],
                TriggerChance = 1.0
            };

            // 技能82: 完全净化 - 净化所有有害效果
            Skills[82] = new Skill
            {
                Id = 82,
                Name = "完全净化",
                Description = "净化目标所有有害效果（高阶净化）",
                TargetType = 2,
                ManaCost = 45,
                Cooldown = 5,
                DamageType = DamageType.Cleanse,
                HitCount = 99,  // 净化所有
                RangeType = 1,
                DamageMultiplier = 0,
                BuffIds = [],
                TriggerChance = 1.0
            };

            // 技能83: 死亡一指 - 高百分比伤害
            Skills[83] = new Skill
            {
                Id = 83,
                Name = "死亡一指",
                Description = "对目标造成最大生命值25%的伤害（不超过施法者攻击力300%，高阶技能）",
                TargetType = 1,
                ManaCost = 60,
                Cooldown = 6,
                DamageType = DamageType.PercentDamage,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 0.25,  // 25%最大生命值
                BuffIds = [],
                TriggerChance = 1.0
            };

            // ========== 魅惑类技能 ==========

            // 技能84: 魅惑之术 - 单体魅惑
            Skills[84] = new Skill
            {
                Id = 84,
                Name = "魅惑之术",
                Description = "魅惑目标，使其攻击友方单位1回合",
                TargetType = 1,
                ManaCost = 40,
                Cooldown = 5,
                DamageType = DamageType.Buff,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 0,
                BuffIds = ["buff_charm"],
                TriggerChance = 0.7  // 70%成功率
            };

            // 技能85: 混乱迷雾 - 群体魅惑
            Skills[85] = new Skill
            {
                Id = 85,
                Name = "混乱迷雾",
                Description = "释放迷雾，使敌方全体有50%概率被魅惑1回合",
                TargetType = 1,
                ManaCost = 70,
                Cooldown = 8,
                DamageType = DamageType.Buff,
                HitCount = 1,
                RangeType = 0,  // 全体
                DamageMultiplier = 0,
                BuffIds = ["buff_charm"],
                TriggerChance = 0.5  // 50%成功率
            };

            // 技能86: 深度魅惑 - 更强力的单体魅惑
            Skills[86] = new Skill
            {
                Id = 86,
                Name = "深度魅惑",
                Description = "深度魅惑目标，使其攻击友方单位2回合（高阶技能）",
                TargetType = 1,
                ManaCost = 55,
                Cooldown = 7,
                DamageType = DamageType.Buff,
                HitCount = 1,
                RangeType = 1,
                DamageMultiplier = 0,
                BuffIds = ["buff_charm_deep"],
                TriggerChance = 0.8  // 80%成功率
            };
        }
        #endregion
    }
}
