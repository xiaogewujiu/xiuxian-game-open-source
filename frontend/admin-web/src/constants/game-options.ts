export type NumericOption = {
  value: number
  label: string
}

export const ITEM_TYPE_OPTIONS: NumericOption[] = [
  { value: 0, label: '材料' },
  { value: 10, label: '道具箱' },
  { value: 11, label: '技能书' },
  { value: 12, label: '丹药' },
  { value: 13, label: '种子' },
  { value: 14, label: '宠物蛋' },
  { value: 15, label: '好感礼物' },
  { value: 16, label: '宝石' },
  { value: 17, label: '配方卷轴' },
  { value: 18, label: '背包扩容' },
  { value: 2, label: '任务物品' },
  { value: 1, label: '旧消耗品' }
]

export const ITEM_CHEST_OPEN_MODE_OPTIONS: NumericOption[] = [
  { value: 1, label: '每次开启抽 1 项' },
  { value: 2, label: '每次开启多次抽取' }
]

export const ITEM_CHEST_REWARD_TYPE_OPTIONS: NumericOption[] = [
  { value: 1, label: '金币' },
  { value: 3, label: '道具' },
  { value: 4, label: '装备' },
  { value: 5, label: '灵石' }
]

export const ITEM_PILL_EFFECT_OPTIONS: NumericOption[] = [
  { value: 1, label: '增加突破概率' },
  { value: 2, label: '增加经验' },
  { value: 3, label: '增加属性' },
  { value: 4, label: '恢复HP/MP' }
]

export const ITEM_PILL_ATTRIBUTE_OPTIONS = [
  { value: 'Type1', label: '生命' },
  { value: 'Type2', label: '法力' },
  { value: 'Type3', label: '物攻' },
  { value: 'Type4', label: '法攻' },
  { value: 'Type5', label: '物防' },
  { value: 'Type6', label: '法防' },
  { value: 'Type7', label: '速度' },
  { value: 'Type8', label: '命中' },
  { value: 'Type9', label: '闪避' },
  { value: 'Type10', label: '暴击' },
  { value: 'Type11', label: '暴伤' },
  { value: 'Type12', label: '连击' },
  { value: 'Type13', label: '反击' },
  { value: 'Type14', label: '破甲' },
  { value: 'Type15', label: '增伤' }
]

export const PET_TYPE_OPTIONS: NumericOption[] = [
  { value: 1, label: '攻击型' },
  { value: 2, label: '防御型' },
  { value: 3, label: '辅助型' },
  { value: 4, label: '平衡型' }
]

export const CROP_TYPE_OPTIONS: NumericOption[] = [
  { value: 1, label: '普通作物' },
  { value: 2, label: '灵草' },
  { value: 3, label: '稀有作物' },
  { value: 4, label: '传说作物' }
]

export const PILL_RANK_OPTIONS: NumericOption[] = [
  { value: 1, label: '一品' },
  { value: 2, label: '二品' },
  { value: 3, label: '三品' },
  { value: 4, label: '四品' },
  { value: 5, label: '五品' },
  { value: 6, label: '六品' },
  { value: 7, label: '七品' },
  { value: 8, label: '八品' },
  { value: 9, label: '九品' }
]

export const PILL_TYPE_OPTIONS: NumericOption[] = [
  { value: 2, label: '修为增益类' },
  { value: 3, label: '突破类' },
  { value: 4, label: '属性类' }
]

export const PILL_EFFECT_TYPE_OPTIONS: NumericOption[] = [
  { value: 3, label: '增加攻击/属性' },
  { value: 4, label: '增加防御/属性' },
  { value: 5, label: '增加经验' },
  { value: 6, label: '特殊/突破辅助' }
]

export const EQUIPMENT_QUALITY_OPTIONS: NumericOption[] = [
  { value: 1, label: '普通' },
  { value: 2, label: '优秀' },
  { value: 3, label: '稀有' },
  { value: 4, label: '史诗' },
  { value: 5, label: '传说' }
]

export const EQUIPMENT_SLOT_OPTIONS: NumericOption[] = [
  { value: 0, label: '武器' },
  { value: 1, label: '头盔' },
  { value: 2, label: '护甲' },
  { value: 3, label: '裤子' },
  { value: 4, label: '项链' },
  { value: 5, label: '戒指' },
  { value: 6, label: '靴子' },
  { value: 7, label: '法宝' }
]

export const COMBAT_STYLE_OPTIONS: NumericOption[] = [
  { value: 0, label: '中性' },
  { value: 1, label: '物理' },
  { value: 2, label: '法术' }
]

export const WEAPON_CATEGORY_OPTIONS: NumericOption[] = [
  { value: 0, label: '无' },
  { value: 1, label: '剑' },
  { value: 2, label: '刀' },
  { value: 3, label: '斧' },
  { value: 4, label: '枪' },
  { value: 5, label: '琴' },
  { value: 6, label: '棋' },
  { value: 7, label: '书' },
  { value: 8, label: '笔' }
]

export const QUEST_TYPE_OPTIONS: NumericOption[] = [
  { value: 0, label: '主线' },
  { value: 1, label: '支线' },
  { value: 2, label: '日常' },
  { value: 3, label: '周常' },
  { value: 4, label: '成就任务' },
  { value: 5, label: '宗门任务' }
]

export const QUEST_RESET_CYCLE_OPTIONS: NumericOption[] = [
  { value: 0, label: '一次性' },
  { value: 1, label: '每日' },
  { value: 2, label: '每周' }
]

export const QUEST_OBJECTIVE_TYPE_OPTIONS: NumericOption[] = [
  { value: 0, label: '击杀怪物' },
  { value: 1, label: '收集道具' },
  { value: 2, label: '对话 NPC' },
  { value: 3, label: '完成副本' },
  { value: 4, label: '达到等级' },
  { value: 5, label: '装备强化' },
  { value: 6, label: '战斗活动' },
  { value: 7, label: '指定地图战斗次数' },
  { value: 8, label: '指定地图胜利次数' },
  { value: 9, label: '炼丹领取次数' },
  { value: 10, label: '锻造领取次数' },
  { value: 11, label: '指定丹方炼丹成功' },
  { value: 12, label: '指定图纸锻造成功' },
  { value: 13, label: '装备强化次数' },
  { value: 14, label: '装备达到指定强化等级件数' },
  { value: 15, label: '种植指定作物次数' },
  { value: 16, label: '收获指定作物次数' },
  { value: 17, label: '宗门捐献次数' },
  { value: 18, label: '战斗胜利次数' },
  { value: 19, label: '宗门Boss伤害' }
]

export const BATTLE_ACTIVITY_TYPE_OPTIONS: NumericOption[] = [
  { value: 0, label: '造成伤害' },
  { value: 1, label: '承受伤害' },
  { value: 2, label: '击杀怪物' },
  { value: 3, label: '获得胜利' },
  { value: 4, label: '使用技能' },
  { value: 5, label: '触发暴击' }
]

export const ACHIEVEMENT_TYPE_OPTIONS: NumericOption[] = [
  { value: 0, label: '等级' },
  { value: 1, label: '战斗' },
  { value: 2, label: '装备' },
  { value: 3, label: '收集' },
  { value: 4, label: '社交' },
  { value: 5, label: '活动' },
  { value: 6, label: '特殊' }
]

export const ACHIEVEMENT_DIFFICULTY_OPTIONS: NumericOption[] = [
  { value: 10, label: '简单' },
  { value: 25, label: '普通' },
  { value: 50, label: '困难' },
  { value: 100, label: '极限' }
]

export const ACHIEVEMENT_REQUIREMENT_TYPE_OPTIONS: NumericOption[] = [
  { value: 0, label: '达到等级' },
  { value: 1, label: '总击杀数' },
  { value: 2, label: '总胜利场次' },
  { value: 3, label: '最大强化等级' },
  { value: 4, label: '装备收集数量' },
  { value: 5, label: '道具收集数量' },
  { value: 6, label: '累计获得金币' },
  { value: 7, label: '累计消费灵石' },
  { value: 8, label: '累计登录天数' },
  { value: 9, label: '连续登录天数' },
  { value: 10, label: '完成任务数量' },
  { value: 11, label: '完成副本次数' },
  { value: 12, label: '无伤战斗' },
  { value: 13, label: '最大连击数' },
  { value: 14, label: '累计造成伤害' },
  { value: 15, label: '累计获得经验' },
  { value: 16, label: '累计受到伤害' },
  { value: 17, label: '炼丹成功次数' },
  { value: 18, label: '炼丹失败次数' },
  { value: 19, label: '锻造成功次数' },
  { value: 20, label: '锻造失败次数' },
  { value: 21, label: '强化成功次数' },
  { value: 22, label: '强化失败次数' },
  { value: 23, label: '种植次数' },
  { value: 24, label: '收获次数' },
  { value: 25, label: '普通副本胜利次数' },
  { value: 26, label: '击杀指定野怪次数' },
  { value: 27, label: '使用指定技能次数' },
  { value: 28, label: '累计消耗蓝量' },
  { value: 29, label: '暴击次数' },
  { value: 30, label: '死亡次数' },
  { value: 31, label: '闪避次数' },
  { value: 32, label: '指定技能累计伤害' }
]

export const SHOP_TYPE_OPTIONS: NumericOption[] = [
  { value: 0, label: '杂货铺' },
  { value: 1, label: '铁匠铺' },
  { value: 2, label: '魔法商店' },
  { value: 3, label: '黑市商人' },
  { value: 4, label: '限时商店' }
]

export const RANKING_TYPE_OPTIONS: NumericOption[] = [
  { value: 0, label: '等级排行' },
  { value: 1, label: '战力排行' },
  { value: 2, label: '竞技场排行' },
  { value: 3, label: '副本排行' },
  { value: 4, label: '成就排行' },
  { value: 5, label: '财富排行' }
]

export const SKILL_TARGET_TYPE_OPTIONS: NumericOption[] = [
  { value: 1, label: '敌方' },
  { value: 2, label: '友方' }
]

export const SKILL_RANGE_TYPE_OPTIONS: NumericOption[] = [
  { value: 0, label: '全体' },
  { value: 1, label: '单体' },
  { value: 2, label: '双体' },
  { value: 3, label: '三体' },
  { value: 4, label: '四体' },
  { value: 5, label: '五体' }
]

export const DAMAGE_TYPE_OPTIONS: NumericOption[] = [
  { value: 0, label: '物理伤害' },
  { value: 1, label: '法术伤害' },
  { value: 2, label: '真实伤害' },
  { value: 3, label: '治疗' },
  { value: 4, label: '增益/减益' },
  { value: 5, label: '恢复蓝量' },
  { value: 6, label: '复活' },
  { value: 7, label: '血祭' },
  { value: 8, label: '生命转换' },
  { value: 9, label: '百分比伤害' },
  { value: 10, label: '驱散' },
  { value: 11, label: '净化' }
]

export const STACK_RULE_OPTIONS: NumericOption[] = [
  { value: 0, label: '刷新持续时间' },
  { value: 1, label: '数值叠加' },
  { value: 2, label: '替换旧 Buff' }
]

export const BUFF_TARGET_CAMP_OPTIONS: NumericOption[] = [
  { value: 1, label: '敌方' },
  { value: 2, label: '己方' }
]

export const BUFF_TARGET_SELECTION_MODE_OPTIONS: NumericOption[] = [
  { value: 1, label: '完全随机' },
  { value: 2, label: '优先技能目标' },
  { value: 3, label: '仅技能目标' }
]

export const BUFF_EFFECT_TYPE_OPTIONS: NumericOption[] = [
  { value: 1, label: '持续伤害' },
  { value: 2, label: '持续治疗' },
  { value: 3, label: '眩晕' },
  { value: 4, label: '防御降低' },
  { value: 5, label: '防御提升' },
  { value: 6, label: '攻击提升' },
  { value: 7, label: '攻击降低' },
  { value: 8, label: '沉默' },
  { value: 9, label: '暴击率提升' },
  { value: 10, label: '暴击率降低' },
  { value: 11, label: '暴击伤害提升' },
  { value: 12, label: '暴击伤害降低' },
  { value: 13, label: '暴击抵抗' },
  { value: 14, label: '伤害增幅' },
  { value: 15, label: '伤害减免' },
  { value: 16, label: '嘲讽' },
  { value: 17, label: '持续恢复蓝量' },
  { value: 18, label: '命中率提升' },
  { value: 19, label: '命中率降低' },
  { value: 20, label: '吸血' },
  { value: 21, label: '反伤' },
  { value: 22, label: '护盾' },
  { value: 23, label: '免疫控制' },
  { value: 24, label: '蓝量消耗' },
  { value: 25, label: '连击率提升' },
  { value: 26, label: '连击率降低' },
  { value: 27, label: '连击伤害提升' },
  { value: 28, label: '连击伤害降低' },
  { value: 29, label: '反击率提升' },
  { value: 30, label: '反击率降低' },
  { value: 31, label: '反击伤害提升' },
  { value: 32, label: '反击伤害降低' },
  { value: 33, label: '禁疗' },
  { value: 34, label: '减速' },
  { value: 35, label: '加速' },
  { value: 36, label: '易伤' },
  { value: 37, label: '不屈' },
  { value: 38, label: '冷却缩减' },
  { value: 39, label: '冷却增加' },
  { value: 40, label: '斩杀' },
  { value: 41, label: '镜像' },
  { value: 42, label: '缴械' },
  { value: 43, label: '束缚' },
  { value: 44, label: '破防' },
  { value: 45, label: '庇护' },
  { value: 46, label: '自动复活' },
  { value: 47, label: '无敌' },
  { value: 48, label: '魅惑' }
]

export const LOTTERY_TYPE_OPTIONS: NumericOption[] = [
  { value: 0, label: '文字图鉴' },
  { value: 1, label: '图片图鉴' },
  { value: 2, label: '幸运抽奖' }
]

export const LOTTERY_COST_TYPE_OPTIONS: NumericOption[] = [
  { value: 0, label: '金币' },
  { value: 1, label: '灵石' },
  { value: 2, label: '道具' }
]

export const LOTTERY_REWARD_TYPE_OPTIONS: NumericOption[] = [
  { value: 0, label: '金币' },
  { value: 1, label: '灵石' },
  { value: 2, label: '道具' },
  { value: 3, label: '文字图鉴' },
  { value: 4, label: '图片图鉴' },
  { value: 5, label: '谢谢惠顾' }
]

export const BONUS_VALUE_TYPE_OPTIONS: NumericOption[] = [
  { value: 0, label: '固定值' },
  { value: 1, label: '百分比' }
]

export const ATTR_TYPE_OPTIONS = [
  { value: 'Type1', label: '生命' },
  { value: 'Type2', label: '法力' },
  { value: 'Type3', label: '物攻' },
  { value: 'Type4', label: '法攻' },
  { value: 'Type5', label: '物防' },
  { value: 'Type6', label: '法防' },
  { value: 'Type7', label: '速度' },
  { value: 'Type8', label: '命中' },
  { value: 'Type9', label: '闪避' },
  { value: 'Type10', label: '暴击' },
  { value: 'Type11', label: '暴伤' },
  { value: 'Type12', label: '连击' },
  { value: 'Type13', label: '反击' },
  { value: 'Type14', label: '破甲' },
  { value: 'Type15', label: '增伤' }
]

export const DUNGEON_EVENT_TYPE_OPTIONS: NumericOption[] = [
  { value: 1, label: '战斗' },
  { value: 2, label: '回复' },
  { value: 3, label: '增益' },
  { value: 4, label: '减益/陷阱' },
  { value: 5, label: '资源' },
  { value: 6, label: '宝箱' },
  { value: 7, label: '奇遇' },
  { value: 8, label: '商店' },
  { value: 9, label: '特殊事件' },
  { value: 10, label: '空事件' },
  { value: 11, label: '玩家偶遇' }
]
