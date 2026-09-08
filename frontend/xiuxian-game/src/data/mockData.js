/**
 * 修仙问道 - 模拟数据文件
 *
 * 本文件包含所有前端模拟数据，方便后期接入真实后端时替换
 */

// ==================== 玩家数据 ====================
export const playerData = {
  id: 1,
  name: '青云道人',
  avatar: '🧙',
  level: 45,

  // 修为境界
  realm: {
    name: '筑基期',
    tier: 'rare',
    level: 3,
    currentExp: 85420,
    maxExp: 100000,
    description: '筑就道基，奠定修仙根基。气息悠长，寿元增至两百载。'
  },

  // 肉体境界
  bodyRealm: {
    name: '铜皮铁骨',
    stage: 5,
    currentExp: 3500,
    maxExp: 5000
  },

  // 资源
  resources: {
    spiritStone: 12580,
    gold: 9999999,
    cultivation: 85420,
    contribution: 3200
  },

  // 基础属性
  stats: {
    hp: 2580,
    maxHp: 2580,
    mp: 520,
    maxMp: 520,
    physicalAttack: 854,
    magicalAttack: 620,
    physicalDefense: 623,
    magicalDefense: 580,
    hit: 95,
    dodge: 12,
    crit: 15,
    armorBreak: 150,
    counterRate: 8,
    comboRate: 5
  },

  // 战斗力
  combatPower: 125680
}

// ==================== 技能数据 ====================
export const skillsData = {
  // 已装备技能
  equipped: [
    { id: 1, name: '火球术', icon: '🔥', quality: 'rare', type: '法术', cost: '50法力', cooldown: '3秒', damage: '320', description: '凝聚火元素，发射一颗火球攻击敌人' },
    { id: 2, name: '御剑术', icon: '🗡️', quality: 'epic', type: '物理', cost: '30法力', cooldown: '2秒', damage: '280', description: '以气御剑，远程攻击敌人' },
    { id: 3, name: '护盾术', icon: '🛡️', quality: 'uncommon', type: '防御', cost: '40法力', cooldown: '10秒', damage: '-', description: '召唤灵气护盾，吸收伤害' },
    { id: 4, name: '雷击术', icon: '⚡', quality: 'legendary', type: '法术', cost: '80法力', cooldown: '5秒', damage: '550', description: '召唤天雷轰击敌人' }
  ],

  // 技能库
  library: [
    { id: 5, name: '冰冻术', icon: '❄️', quality: 'rare', type: '控制', cost: '60法力', cooldown: '4秒', damage: '200', description: '冰冻敌人，使其无法行动' },
    { id: 6, name: '治愈术', icon: '💚', quality: 'uncommon', type: '治疗', cost: '50法力', cooldown: '8秒', damage: '-', description: '恢复生命值' },
    { id: 7, name: '风刃术', icon: '🌪️', quality: 'common', type: '物理', cost: '25法力', cooldown: '1.5秒', damage: '150', description: '发射风刃切割敌人' },
    { id: 8, name: '石化术', icon: '🗿', quality: 'epic', type: '控制', cost: '70法力', cooldown: '6秒', damage: '-', description: '将敌人石化' },
    { id: 9, name: '火焰风暴', icon: '🌋', quality: 'mythic', type: '法术', cost: '120法力', cooldown: '10秒', damage: '800', description: '召唤火焰风暴，对范围内敌人造成伤害' }
  ]
}

// ==================== 装备数据 ====================
export const equipmentData = {
  // 当前穿戴装备
  equipped: {
    weapon: { name: '青锋剑', icon: '🗡️', quality: 'rare', level: 45, enhance: 8, description: '青云门制式飞剑，锋利无比', stats: [{ name: '攻击', value: 320 }, { name: '暴击', value: 15 }] },
    helmet: { name: '灵风头巾', icon: '🎭', quality: 'uncommon', level: 40, enhance: 5, description: '以灵蚕丝织成，轻便透气', stats: [{ name: '防御', value: 120 }, { name: '法力', value: 50 }] },
    armor: { name: '青云道袍', icon: '👘', quality: 'rare', level: 45, enhance: 6, description: '青云门弟子服装，带有防御法阵', stats: [{ name: '防御', value: 280 }, { name: '生命', value: 200 }] },
    pants: null,
    shoes: { name: '疾风靴', icon: '👢', quality: 'epic', level: 42, enhance: 4, description: '穿上后身轻如燕', stats: [{ name: '速度', value: 25 }, { name: '闪避', value: 10 }] },
    necklace: null,
    ring1: { name: '储物戒', icon: '💍', quality: 'legendary', level: 50, enhance: 0, description: '内含一方小天地，可储存物品', stats: [{ name: '背包', value: 100 }] },
    treasure: null
  },

  // 装备槽位配置
  slots: [
    { position: 'weapon', name: '武器', icon: '⚔️' },
    { position: 'helmet', name: '头盔', icon: '⛑️' },
    { position: 'armor', name: '衣服', icon: '👕' },
    { position: 'pants', name: '裤子', icon: '👖' },
    { position: 'shoes', name: '鞋子', icon: '👟' },
    { position: 'necklace', name: '项链', icon: '📿' },
    { position: 'ring1', name: '戒指', icon: '💍' },
    { position: 'treasure', name: '法宝', icon: '🔮' }
  ]
}

// ==================== 地图数据 ====================
export const mapsData = [
  {
    id: 1,
    name: '青云山脉',
    icon: '⛰️',
    recommendRealm: '练气期',
    difficulty: 'easy',
    difficultyText: '简单',
    description: '青云门所在山脉，灵气充沛，适合初学者修炼。山中多野兽，需谨慎行事。',
    minRealm: '练气期',
    minBody: '凡人',
    teamSize: 1,
    enemyTypes: '野兽、妖兽',
    enemyLevelMin: 1,
    enemyLevelMax: 15,
    hasBoss: false,
    drops: [
      { name: '野兽皮毛', icon: '🧥', quality: 'common' },
      { name: '低阶灵石', icon: '💎', quality: 'uncommon' },
      { name: '草药', icon: '🌿', quality: 'common' }
    ],
    unlocked: true
  },
  {
    id: 2,
    name: '幽暗森林',
    icon: '🌲',
    recommendRealm: '筑基期',
    difficulty: 'normal',
    difficultyText: '普通',
    description: '常年被迷雾笼罩的古老森林，深处隐藏着强大的妖兽。传言森林中心有一株千年灵芝。',
    minRealm: '筑基期',
    minBody: '铜皮铁骨',
    teamSize: 1,
    enemyTypes: '妖兽、树妖',
    enemyLevelMin: 15,
    enemyLevelMax: 35,
    hasBoss: true,
    drops: [
      { name: '妖兽内丹', icon: '🔮', quality: 'rare' },
      { name: '中阶灵石', icon: '💎', quality: 'rare' },
      { name: '千年灵芝', icon: '🍄', quality: 'epic' }
    ],
    unlocked: true
  },
  {
    id: 3,
    name: '火焰深渊',
    icon: '🌋',
    recommendRealm: '金丹期',
    difficulty: 'hard',
    difficultyText: '困难',
    description: '地下岩浆世界，火元素极度浓郁。只有金丹期以上的修士才能在此生存。',
    minRealm: '金丹期',
    minBody: '钢筋铁骨',
    teamSize: 3,
    enemyTypes: '火灵、炎魔',
    enemyLevelMin: 35,
    enemyLevelMax: 60,
    hasBoss: true,
    drops: [
      { name: '火灵珠', icon: '🔥', quality: 'epic' },
      { name: '高阶灵石', icon: '💎', quality: 'epic' },
      { name: '火属性功法', icon: '📜', quality: 'legendary' }
    ],
    unlocked: true
  },
  {
    id: 4,
    name: '寒冰禁地',
    icon: '❄️',
    recommendRealm: '元婴期',
    difficulty: 'hard',
    difficultyText: '困难',
    description: '万年冰封的禁地，元婴期以下进入必死无疑。传说中封印着上古魔物。',
    minRealm: '元婴期',
    minBody: '金刚不坏',
    teamSize: 5,
    enemyTypes: '冰灵、雪妖',
    enemyLevelMin: 60,
    enemyLevelMax: 90,
    hasBoss: true,
    drops: [
      { name: '玄冰晶', icon: '💎', quality: 'legendary' },
      { name: '极品灵石', icon: '💎', quality: 'mythic' },
      { name: '冰系神通', icon: '📜', quality: 'mythic' }
    ],
    unlocked: false
  }
]

// ==================== 怪物数据 ====================
export const monstersData = [
  { id: 1, name: '野狼', avatar: '🐺', hp: 500, maxHp: 500, mp: 0, maxMp: 0, level: 5 },
  { id: 2, name: '妖狼', avatar: '🐺', hp: 800, maxHp: 800, mp: 0, maxMp: 0, level: 10 },
  { id: 3, name: '妖狼王', avatar: '🐺', hp: 1500, maxHp: 1500, mp: 200, maxMp: 200, level: 15, isBoss: true },
  { id: 4, name: '树妖', avatar: '🌲', hp: 1200, maxHp: 1200, mp: 300, maxMp: 300, level: 20 },
  { id: 5, name: '火灵', avatar: '🔥', hp: 2000, maxHp: 2000, mp: 500, maxMp: 500, level: 40 }
]

// ==================== 商店商品数据 ====================
export const shopItemsData = {
  equipment: [
    { id: 1, name: '青锋剑', icon: '🗡️', quality: 'rare', type: '武器', effect: '攻击+150', price: 5000, currency: 'spirit', stock: 3, description: '青云门制式飞剑' },
    { id: 2, name: '玄铁甲', icon: '🛡️', quality: 'uncommon', type: '防具', effect: '防御+80', price: 3000, currency: 'spirit', stock: 5, description: '以玄铁打造的护甲' },
    { id: 3, name: '灵风头巾', icon: '🎭', quality: 'rare', type: '头盔', effect: '法力+100', price: 4000, currency: 'spirit', stock: 2, description: '可提升法力恢复速度' },
    { id: 4, name: '疾风靴', icon: '👢', quality: 'epic', type: '鞋子', effect: '速度+30', price: 8000, currency: 'spirit', stock: 1, description: '穿上后身轻如燕' }
  ],
  consumables: [
    { id: 5, name: '聚气丹', icon: '✨', quality: 'common', type: '丹药', effect: '修为+1000', price: 500, currency: 'gold', stock: 99, description: '服用后可提升修为' },
    { id: 6, name: '破障丹', icon: '🜂', quality: 'rare', type: '丹药', effect: '突破率+8%', price: 800, currency: 'gold', stock: 99, description: '服用后提高下次突破成功率' },
    { id: 7, name: '护脉丹', icon: '🛡️', quality: 'rare', type: '丹药', effect: '防御+12', price: 1000, currency: 'spirit', stock: 10, description: '可提升基础防御的丹药' },
    { id: 8, name: '悟道茶', icon: '🍵', quality: 'epic', type: '饮品', effect: '修炼速度+50%', price: 2000, currency: 'spirit', stock: 5, description: '喝后可提升悟道速度' }
  ],
  materials: [
    { id: 9, name: '玄铁矿石', icon: '⛏️', quality: 'uncommon', type: '矿石', effect: '锻造材料', price: 200, currency: 'gold', stock: 50, description: '锻造装备的常用材料' },
    { id: 10, name: '灵草', icon: '🌿', quality: 'common', type: '草药', effect: '炼丹材料', price: 100, currency: 'gold', stock: 99, description: '炼制基础丹药的材料' },
    { id: 11, name: '妖兽内丹', icon: '🔮', quality: 'rare', type: '材料', effect: '炼器材料', price: 500, currency: 'spirit', stock: 20, description: '击杀妖兽获得' },
    { id: 12, name: '千年灵芝', icon: '🍄', quality: 'epic', type: '仙草', effect: '炼丹材料', price: 3000, currency: 'spirit', stock: 3, description: '极为珍贵的炼丹材料' }
  ],
  skillbooks: [
    { id: 13, name: '基础剑法', icon: '📖', quality: 'common', type: '功法', effect: '学会基础剑法', price: 1000, currency: 'gold', stock: 1, description: '剑修入门功法' },
    { id: 14, name: '火球术', icon: '🔥', quality: 'uncommon', type: '法术', effect: '学会火球术', price: 2000, currency: 'gold', stock: 1, description: '基础火系法术' },
    { id: 15, name: '御剑术', icon: '⚔️', quality: 'rare', type: '剑诀', effect: '学会御剑术', price: 5000, currency: 'spirit', stock: 1, description: '可御剑飞行的剑诀' },
    { id: 16, name: '金丹要诀', icon: '📜', quality: 'epic', type: '秘籍', effect: '突破金丹期', price: 10000, currency: 'spirit', stock: 1, description: '突破金丹期的关键秘籍' }
  ]
}

// ==================== 宠物数据 ====================
export const petsData = [
  {
    id: 1,
    name: '小火狐',
    avatar: '🦊',
    level: 25,
    exp: 3500,
    maxExp: 5000,
    quality: 'rare',
    qualityText: '稀有',
    type: '火属性',
    growth: '优秀',
    isActive: true,
    stats: [
      { name: '攻击', value: 320 },
      { name: '防御', value: 180 },
      { name: '生命', value: 1200 },
      { name: '速度', value: 85 },
      { name: '暴击', value: 15 },
      { name: '闪避', value: 10 }
    ],
    skills: [
      { name: '火焰爪', icon: '🔥', desc: '用燃烧的爪子攻击敌人' },
      { name: '火焰护盾', icon: '🛡️', desc: '为主人提供火焰护盾' }
    ]
  },
  {
    id: 2,
    name: '青风狼',
    avatar: '🐺',
    level: 18,
    exp: 1200,
    maxExp: 3600,
    quality: 'uncommon',
    qualityText: '优秀',
    type: '风属性',
    growth: '良好',
    isActive: false,
    stats: [
      { name: '攻击', value: 280 },
      { name: '防御', value: 150 },
      { name: '生命', value: 1000 },
      { name: '速度', value: 120 },
      { name: '暴击', value: 12 },
      { name: '闪避', value: 15 }
    ],
    skills: [
      { name: '风刃', icon: '🌪️', desc: '发射锋利的风刃' },
      { name: '疾风步', icon: '💨', desc: '大幅提升速度' }
    ]
  },
  {
    id: 3,
    name: '玄龟',
    avatar: '🐢',
    level: 30,
    exp: 2000,
    maxExp: 6000,
    quality: 'epic',
    qualityText: '史诗',
    type: '水属性',
    growth: '完美',
    isActive: false,
    stats: [
      { name: '攻击', value: 250 },
      { name: '防御', value: 450 },
      { name: '生命', value: 2500 },
      { name: '速度', value: 40 },
      { name: '暴击', value: 5 },
      { name: '格挡', value: 25 }
    ],
    skills: [
      { name: '水盾', icon: '💧', desc: '召唤水元素护盾' },
      { name: '治疗波', icon: '💚', desc: '恢复主人生命值' }
    ]
  }
]

// ==================== 排行榜数据 ====================
export const rankingsData = {
  realm: [
    { id: 1, name: '太上忘情', avatar: '🧙', realm: '化神期', value: 9999999 },
    { id: 2, name: '一剑霜寒', avatar: '⚔️', realm: '元婴期', value: 8500000 },
    { id: 3, name: '九转金丹', avatar: '💊', realm: '元婴期', value: 7800000 },
    { id: 4, name: '青云直上', avatar: '☁️', realm: '金丹期', value: 5200000 },
    { id: 5, name: '月下独酌', avatar: '🌙', realm: '金丹期', value: 4800000 },
    { id: 6, name: '万古长青', avatar: '🌲', realm: '金丹期', value: 4500000 },
    { id: 7, name: '剑舞长空', avatar: '🗡️', realm: '筑基期', value: 3200000 },
    { id: 8, name: '青云道人', avatar: '🧙', realm: '筑基期', value: 2800000, isMe: true }
  ],
  body: [
    { id: 1, name: '不灭金身', avatar: '🛡️', realm: '金刚不坏', value: 10000000 },
    { id: 2, name: '铁骨铮铮', avatar: '💪', realm: '钢筋铁骨', value: 8500000 },
    { id: 3, name: '铜墙铁壁', avatar: '🧱', realm: '钢筋铁骨', value: 7200000 },
    { id: 4, name: '百炼成钢', avatar: '⚒️', realm: '铜皮铁骨', value: 5500000 },
    { id: 5, name: '青云道人', avatar: '🧙', realm: '铜皮铁骨', value: 3500000, isMe: true }
  ],
  gold: [
    { id: 1, name: '富可敌国', avatar: '💰', realm: '金丹期', value: 999999999 },
    { id: 2, name: '金银满堂', avatar: '🏦', realm: '筑基期', value: 888888888 },
    { id: 3, name: '财源滚滚', avatar: '🌊', realm: '金丹期', value: 666666666 },
    { id: 4, name: '青云道人', avatar: '🧙', realm: '筑基期', value: 9999999, isMe: true }
  ],
  spirit: [
    { id: 1, name: '灵石大亨', avatar: '💎', realm: '元婴期', value: 5000000 },
    { id: 2, name: '灵气逼人', avatar: '✨', realm: '金丹期', value: 3800000 },
    { id: 3, name: '聚灵成海', avatar: '🌊', realm: '金丹期', value: 3200000 },
    { id: 4, name: '青云道人', avatar: '🧙', realm: '筑基期', value: 12580, isMe: true }
  ]
}

// ==================== 丹方数据 ====================
export const recipesData = [
  {
    id: 1,
    name: '回血丹',
    icon: '❤️',
    quality: 'common',
    effect: '恢复500生命值',
    successRate: 95,
    materials: [
      { name: '灵草', icon: '🌿', count: 3 },
      { name: '清水', icon: '💧', count: 1 }
    ]
  },
  {
    id: 2,
    name: '回蓝丹',
    icon: '💧',
    quality: 'common',
    effect: '恢复200法力值',
    successRate: 90,
    materials: [
      { name: '灵草', icon: '🌿', count: 2 },
      { name: '灵石粉末', icon: '✨', count: 1 }
    ]
  },
  {
    id: 3,
    name: '修为丹',
    icon: '✨',
    quality: 'rare',
    effect: '增加1000修为',
    successRate: 75,
    materials: [
      { name: '灵草', icon: '🌿', count: 5 },
      { name: '妖兽内丹', icon: '🔮', count: 1 }
    ]
  },
  {
    id: 4,
    name: '筑基丹',
    icon: '💎',
    quality: 'epic',
    effect: '突破筑基期必备',
    successRate: 60,
    materials: [
      { name: '千年灵芝', icon: '🍄', count: 1 },
      { name: '妖兽内丹', icon: '🔮', count: 3 },
      { name: '灵草', icon: '🌿', count: 10 }
    ]
  }
]

// ==================== 锻造图纸数据 ====================
export const blueprintsData = [
  {
    id: 1,
    name: '青锋剑',
    icon: '🗡️',
    quality: 'rare',
    materials: [
      { name: '玄铁矿石', icon: '⛏️', count: 5 },
      { name: '妖兽内丹', icon: '🔮', count: 1 }
    ]
  },
  {
    id: 2,
    name: '玄铁甲',
    icon: '🛡️',
    quality: 'uncommon',
    materials: [
      { name: '玄铁矿石', icon: '⛏️', count: 10 }
    ]
  },
  {
    id: 3,
    name: '灵风头巾',
    icon: '🎭',
    quality: 'rare',
    materials: [
      { name: '灵草', icon: '🌿', count: 10 },
      { name: '妖兽内丹', icon: '🔮', count: 2 }
    ]
  },
  {
    id: 4,
    name: '疾风靴',
    icon: '👢',
    quality: 'epic',
    materials: [
      { name: '玄铁矿石', icon: '⛏️', count: 5 },
      { name: '千年灵芝', icon: '🍄', count: 1 }
    ]
  }
]

// ==================== 更新日志数据 ====================
export const changelogsData = [
  {
    version: '1.0.0',
    date: '2024-02-02',
    isNew: true,
    features: [
      '全新修仙世界开放，支持角色修炼升级',
      '新增战斗系统，支持自动战斗',
      '新增地图探索功能',
      '新增宠物系统，可捕获和培养宠物',
      '新增装备锻造和炼丹系统',
      '新增组队功能，可与其他道友组队探险'
    ],
    optimizations: [
      '优化UI界面，提升用户体验',
      '优化战斗流畅度'
    ],
    fixes: []
  },
  {
    version: '0.9.0',
    date: '2024-01-15',
    isNew: false,
    features: [
      '新增签到系统',
      '新增排行榜功能',
      '新增商店系统'
    ],
    optimizations: [
      '优化资源加载速度'
    ],
    fixes: [
      '修复部分界面显示异常的问题'
    ]
  },
  {
    version: '0.8.0',
    date: '2024-01-01',
    isNew: false,
    features: [
      '游戏基础框架搭建完成',
      '新增登录注册系统',
      '新增角色属性面板'
    ],
    optimizations: [],
    fixes: []
  }
]

// ==================== 兑换码数据 ====================
export const redeemCodesData = {
  'XIUXIAN2024': [
    { icon: '💎', name: '灵石', count: 1000 },
    { icon: '🪙', name: '金币', count: 50000 }
  ],
  'WELCOME': [
    { icon: '💎', name: '灵石', count: 500 },
    { icon: '✨', name: '修为', count: 1000 }
  ],
  'VIP666': [
    { icon: '💎', name: '灵石', count: 666 },
    { icon: '🎁', name: '神秘宝箱', count: 1 }
  ]
}

// ==================== 境界数据 ====================
export const realmsData = [
  { name: '练气期', tier: 'common', stages: 9 },
  { name: '筑基期', tier: 'rare', stages: 3 },
  { name: '金丹期', tier: 'epic', stages: 9 },
  { name: '元婴期', tier: 'legendary', stages: 3 },
  { name: '化神期', tier: 'mythic', stages: 9 }
]

// ==================== 肉体境界数据 ====================
export const bodyRealmsData = [
  { name: '凡人', tier: 'common' },
  { name: '铜皮铁骨', tier: 'uncommon' },
  { name: '钢筋铁骨', tier: 'rare' },
  { name: '金刚不坏', tier: 'epic' },
  { name: '不灭金身', tier: 'legendary' }
]

// ==================== 五行数据 ====================
export const elementsData = {
  metal: { name: '金', color: '#b8b8b8', icon: '⚔️' },
  wood: { name: '木', color: '#4caf50', icon: '🌲' },
  water: { name: '水', color: '#2196f3', icon: '💧' },
  fire: { name: '火', color: '#f44336', icon: '🔥' },
  earth: { name: '土', color: '#8b4513', icon: '⛰️' }
}
