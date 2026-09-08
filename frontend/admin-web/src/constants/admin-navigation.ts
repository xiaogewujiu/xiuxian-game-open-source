import { ADMIN_PERMISSIONS, ADMIN_USER_MANAGEMENT_ROLES } from '@/constants/admin'

export type AdminNavItem = {
  name: string
  label: string
  to: string
  group: string
  code: string
  blurb: string
  permission?: string
  roles?: string[]
}

export const ADMIN_NAV_ITEMS: AdminNavItem[] = [
  // ═══ 总览 ═══
  { name: 'admin-dashboard', label: '仪表盘', to: '/', group: '总览', code: 'OVR', blurb: '查看核心运行指标、业务入口与后台总体状态。' },
  { name: 'admin-runtime-configs', label: '运行配置', to: '/runtime-configs', group: '总览', code: 'RTC', blurb: '查看运行时配置域状态并执行后台缓存刷新。', permission: ADMIN_PERMISSIONS.configRead },
  { name: 'admin-audit-logs', label: '审计日志', to: '/audit-logs', group: '总览', code: 'ADT', blurb: '追踪后台写操作、请求详情与配置差异快照。', permission: ADMIN_PERMISSIONS.auditRead },

  // ═══ 玩家与运营 ═══
  { name: 'admin-admin-users', label: '后台用户', to: '/admin-users', group: '玩家与运营', code: 'ADM', blurb: '管理后台账号、角色分配与启停状态。', roles: [...ADMIN_USER_MANAGEMENT_ROLES] },
  { name: 'admin-players', label: '玩家管理', to: '/players', group: '玩家与运营', code: 'PLY', blurb: '查看玩家资料、执行 GM 发放与封禁处理。', permission: ADMIN_PERMISSIONS.playerRead },
  { name: 'admin-offline-battles', label: '离线挂机', to: '/offline-battles', group: '玩家与运营', code: 'OFB', blurb: '监控当前离线挂机账号并支持后台强制停止。', permission: ADMIN_PERMISSIONS.playerRead },
  { name: 'admin-feedback', label: '建议反馈', to: '/feedback', group: '玩家与运营', code: 'FDB', blurb: '查看玩家建议、问题反馈和处理结果。', permission: ADMIN_PERMISSIONS.playerRead },
  { name: 'admin-mail', label: '邮件管理', to: '/mail', group: '玩家与运营', code: 'MAL', blurb: '发送系统邮件、查看邮件列表、管理全服邮件与附件。', permission: ADMIN_PERMISSIONS.playerRead },
  { name: 'admin-title', label: '称号管理', to: '/title', group: '玩家与运营', code: 'TTL', blurb: '管理称号模板、发放和回收玩家称号。', permission: ADMIN_PERMISSIONS.playerRead },
  { name: 'admin-arena', label: '竞技场', to: '/arena', group: '玩家与运营', code: 'ARN', blurb: '管理PVP竞技场、查看玩家排名和对战日志。', permission: ADMIN_PERMISSIONS.playerRead },
  { name: 'admin-tower', label: '通天塔', to: '/tower', group: '玩家与运营', code: 'TWR', blurb: '管理通天塔楼层配置、查看玩家进度和战斗日志。', permission: ADMIN_PERMISSIONS.playerRead },
  { name: 'admin-favorability', label: '好感度', to: '/favorability', group: '玩家与运营', code: 'FAV', blurb: '管理玩家好感度关系、赠送记录和等级配置。', permission: ADMIN_PERMISSIONS.playerRead },

  // ═══ 世界与战斗 ═══
  { name: 'admin-maps', label: '地图管理', to: '/maps', group: '世界与战斗', code: 'MAP', blurb: '维护普通地图、副本层级与刷怪配置。', permission: ADMIN_PERMISSIONS.configRead },
  { name: 'admin-monsters', label: '怪物管理', to: '/monsters', group: '世界与战斗', code: 'MOB', blurb: '配置怪物模板、掉落规则与战斗参数。', permission: ADMIN_PERMISSIONS.configRead },
  { name: 'admin-world-boss', label: '世界Boss', to: '/world-boss', group: '世界与战斗', code: 'WBS', blurb: '配置世界Boss模板、刷新排期并查看当前运行状态。', permission: ADMIN_PERMISSIONS.configRead },
  { name: 'admin-skills', label: '技能模板', to: '/skills', group: '世界与战斗', code: 'SKL', blurb: '编辑技能目标、倍率、段数与 Buff 关联。', permission: ADMIN_PERMISSIONS.configRead },
  { name: 'admin-buffs', label: 'Buff模板', to: '/buffs', group: '世界与战斗', code: 'BUF', blurb: '维护 Buff 效果、叠层方式与持续回合。', permission: ADMIN_PERMISSIONS.configRead },
  { name: 'admin-element-rules', label: '元素克制', to: '/element-rules', group: '世界与战斗', code: 'ELR', blurb: '维护八元素克制矩阵。', permission: ADMIN_PERMISSIONS.configRead },
  { name: 'admin-dungeons', label: '副本管理', to: '/dungeons', group: '世界与战斗', code: 'DGN', blurb: '配置副本入口、推荐等级与队伍要求。', permission: ADMIN_PERMISSIONS.configRead },
  { name: 'admin-dungeon-instances', label: '秘境模板', to: '/dungeon-instances', group: '世界与战斗', code: 'DGI', blurb: '配置秘境模板参数、开放时间和进入消耗。', permission: ADMIN_PERMISSIONS.configRead },
  { name: 'admin-dungeon-event-types', label: '秘境事件类型', to: '/dungeon-instances/event-types', group: '世界与战斗', code: 'DET', blurb: '按类型查看和管理秘境事件配置。', permission: ADMIN_PERMISSIONS.configRead },
  { name: 'admin-dungeon-event-groups', label: '秘境事件组', to: '/dungeon-instances/event-groups', group: '世界与战斗', code: 'DEG', blurb: '管理秘境事件组，配置事件池和权重。', permission: ADMIN_PERMISSIONS.configRead },

  // ═══ 宗门 ═══
  { name: 'admin-sects', label: '宗门模板', to: '/sects', group: '宗门', code: 'SCT', blurb: '维护宗门预设、介绍与心法关联。', permission: ADMIN_PERMISSIONS.configRead },
  { name: 'admin-heart-sutras', label: '心法配置', to: '/heart-sutras', group: '宗门', code: 'HST', blurb: '编辑心法层数、属性加成与技能解锁。', permission: ADMIN_PERMISSIONS.configRead },
  { name: 'admin-sect-boss', label: '宗门Boss', to: '/sect-boss', group: '宗门', code: 'SBS', blurb: '配置宗门Boss模板与奖励。', permission: ADMIN_PERMISSIONS.configRead },
  { name: 'admin-sect-shop', label: '宗门商店', to: '/sect-shop', group: '宗门', code: 'SSH', blurb: '管理宗门贡献商店商品与价格。', permission: ADMIN_PERMISSIONS.configRead },
  { name: 'admin-sect-blessings', label: '宗门福利', to: '/sect-blessings', group: '宗门', code: 'SBL', blurb: '配置宗门等级解锁的被动Buff。', permission: ADMIN_PERMISSIONS.configRead },
  { name: 'admin-sect-guilds', label: '宗门监控', to: '/sect-guilds', group: '宗门', code: 'SGD', blurb: '查看和管理玩家宗门。', permission: ADMIN_PERMISSIONS.playerRead },

  // ═══ 经济与物品 ═══
  { name: 'admin-items', label: '道具管理', to: '/items', group: '经济与物品', code: 'ITM', blurb: '管理道具分类、品质、堆叠规则与描述。', permission: ADMIN_PERMISSIONS.configRead },
  { name: 'admin-equipments', label: '装备管理', to: '/equipments', group: '经济与物品', code: 'EQP', blurb: '维护装备模板、槽位、流派与随机区间。', permission: ADMIN_PERMISSIONS.configRead },
  { name: 'admin-equipment-reroll-rules', label: '洗练规则', to: '/equipment-reroll-rules', group: '经济与物品', code: 'ERR', blurb: '维护洗练系统配置、槽位词条池与品阶规则。', permission: ADMIN_PERMISSIONS.configRead },
  { name: 'admin-equipment-decompose-rules', label: '分解规则', to: '/equipment-decompose-rules', group: '经济与物品', code: 'EDC', blurb: '按装备品质维护分解产出道具和数量区间。', permission: ADMIN_PERMISSIONS.configRead },
  { name: 'admin-gem', label: '宝石管理', to: '/gem', group: '经济与物品', code: 'GEM', blurb: '管理宝石模板、批量生成宝石系列和合成链。', permission: ADMIN_PERMISSIONS.configRead },
  { name: 'admin-shop-configs', label: '商店配置', to: '/shop-configs', group: '经济与物品', code: 'SHP', blurb: '管理商店入口、商品库存与售卖价格。', permission: ADMIN_PERMISSIONS.configRead },
  { name: 'admin-market', label: '寄售行', to: '/market', group: '经济与物品', code: 'MKT', blurb: '管理寄售行配置、浏览商品、查看交易日志。', permission: ADMIN_PERMISSIONS.configRead },
  { name: 'admin-ranking-configs', label: '排行配置', to: '/ranking-configs', group: '经济与物品', code: 'RNK', blurb: '维护榜单类型、赛季结构与奖励区间。', permission: ADMIN_PERMISSIONS.configRead },

  // ═══ 生活技能 ═══
  { name: 'admin-alchemy-rules', label: '炼丹规则', to: '/alchemy-rules', group: '生活技能', code: 'ALP', blurb: '维护炼丹师等级经验曲线和成功率加成规则。', permission: ADMIN_PERMISSIONS.configRead },
  { name: 'admin-alchemy-systems', label: '炼丹监控', to: '/alchemy-systems', group: '生活技能', code: 'ALR', blurb: '查看和修正玩家炼丹师等级、制作任务与完成时间。', permission: ADMIN_PERMISSIONS.playerRead },
  { name: 'admin-alchemy-recipes', label: '炼丹配方', to: '/alchemy-recipes', group: '生活技能', code: 'ALC', blurb: '维护丹方材料、成功率与默认学习配置。', permission: ADMIN_PERMISSIONS.configRead },
  { name: 'admin-forge-rules', label: '锻造规则', to: '/forge-rules', group: '生活技能', code: 'FGP', blurb: '维护锻造师等级经验曲线和成功率加成规则。', permission: ADMIN_PERMISSIONS.configRead },
  { name: 'admin-forge-systems', label: '锻造监控', to: '/forge-systems', group: '生活技能', code: 'FGR', blurb: '查看和修正玩家锻造师等级、打造任务与完成时间。', permission: ADMIN_PERMISSIONS.playerRead },
  { name: 'admin-forge-recipes', label: '锻造配方', to: '/forge-recipes', group: '生活技能', code: 'FRG', blurb: '维护图纸材料、打造成功率与产物指向。', permission: ADMIN_PERMISSIONS.configRead },

  // ═══ 角色养成 ═══
  { name: 'admin-player-level-configs', label: '等级成长', to: '/player-level-configs', group: '角色养成', code: 'LVG', blurb: '维护升级经验和基础属性成长数值。', permission: ADMIN_PERMISSIONS.configRead },
  { name: 'admin-realm-level-configs', label: '境界突破', to: '/realm-level-configs', group: '角色养成', code: 'RLM', blurb: '维护等级对应境界、突破成功率、经验损失和突破材料。', permission: ADMIN_PERMISSIONS.configRead },
  { name: 'admin-attribute-point-configs', label: '属性点配置', to: '/attribute-point-configs', group: '角色养成', code: 'APT', blurb: '维护属性点整数收益和等级区间给点规则。', permission: ADMIN_PERMISSIONS.configRead },
  { name: 'admin-pets', label: '灵宠模板', to: '/pets', group: '角色养成', code: 'PET', blurb: '管理灵宠模板、品质上限与技能池。', permission: ADMIN_PERMISSIONS.configRead },
  { name: 'admin-five-elements', label: '聚灵阵监控', to: '/five-elements', group: '角色养成', code: 'FIV', blurb: '查看和修正玩家聚灵阵等级、五行分布与加成。', permission: ADMIN_PERMISSIONS.playerRead },
  { name: 'admin-five-element-rules', label: '聚灵阵规则', to: '/five-element-rules', group: '角色养成', code: 'FVR', blurb: '维护聚灵阵主等级加成与五行分支升级成本。', permission: ADMIN_PERMISSIONS.configRead },
  { name: 'admin-spirit-fields', label: '灵田监控', to: '/spirit-fields', group: '角色养成', code: 'SFD', blurb: '查看和修正玩家灵田等级、地块与种植状态。', permission: ADMIN_PERMISSIONS.playerRead },
  { name: 'admin-spirit-field-rules', label: '灵田规则', to: '/spirit-field-rules', group: '角色养成', code: 'SFR', blurb: '维护灵田默认地块、升级收益和催熟道具规则。', permission: ADMIN_PERMISSIONS.configRead },
  { name: 'admin-crops', label: '作物模板', to: '/crops', group: '角色养成', code: 'CRP', blurb: '配置灵田作物、种子消耗与产出内容。', permission: ADMIN_PERMISSIONS.configRead },

  // ═══ 内容与活动 ═══
  { name: 'admin-quests', label: '任务配置', to: '/quests', group: '内容与活动', code: 'QST', blurb: '编辑任务目标结构、奖励和前置依赖。', permission: ADMIN_PERMISSIONS.configRead },
  { name: 'admin-achievements', label: '成就配置', to: '/achievements', group: '内容与活动', code: 'ACH', blurb: '维护成就分类、条件与奖励内容。', permission: ADMIN_PERMISSIONS.configRead },
  { name: 'admin-checkin-configs', label: '签到奖励', to: '/checkin-configs', group: '内容与活动', code: 'CKI', blurb: '配置签到日历节点与里程碑奖励。', permission: ADMIN_PERMISSIONS.configRead },
  { name: 'admin-redeem-codes', label: '兑换码', to: '/redeem-codes', group: '内容与活动', code: 'RDM', blurb: '管理兑换码发放批次、状态与奖品内容。', permission: ADMIN_PERMISSIONS.configRead },
  { name: 'admin-starter-packages', label: '新手礼包', to: '/starter-packages', group: '内容与活动', code: 'STP', blurb: '维护注册初始技能与起步物资的礼包配置。', permission: ADMIN_PERMISSIONS.configRead },
  { name: 'admin-text-collections', label: '文字图鉴', to: '/text-collections', group: '内容与活动', code: 'TXC', blurb: '管理文字图鉴系列、图鉴项和集齐属性加成。', permission: ADMIN_PERMISSIONS.configRead },
  { name: 'admin-image-collections', label: '图片图鉴', to: '/image-collections', group: '内容与活动', code: 'IMC', blurb: '管理图片图鉴系列、图鉴项和集齐属性加成。', permission: ADMIN_PERMISSIONS.configRead },
  { name: 'admin-lottery-pools', label: '抽奖池管理', to: '/lottery-pools', group: '内容与活动', code: 'LTP', blurb: '配置抽奖池、消耗类型、次数限制和奖项概率。', permission: ADMIN_PERMISSIONS.configRead },
  { name: 'admin-lottery-logs', label: '抽奖日志', to: '/lottery-logs', group: '内容与活动', code: 'LTL', blurb: '查看玩家抽奖记录和奖励详情。', permission: ADMIN_PERMISSIONS.playerRead }
]

export const ADMIN_ROLE_TEXT_MAP: Record<string, string> = {
  super_admin: '超级管理员',
  admin: '管理员',
  operator: '运营管理员',
  gm_support: 'GM客服',
  content_designer: '内容策划',
  economy_designer: '数值策划',
  audit_viewer: '审计查看者'
}

export function filterAdminNavItems(role: string, permissions: string[]) {
  return ADMIN_NAV_ITEMS.filter((item) => {
    if (item.roles && !item.roles.includes(role)) {
      return false
    }

    if (item.permission && !permissions.includes(item.permission)) {
      return false
    }

    return true
  })
}
