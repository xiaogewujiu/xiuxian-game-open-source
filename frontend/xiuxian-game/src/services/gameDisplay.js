/**
 * 中文注释：
 * 这个文件专门负责”前端展示层字段适配”。
 *
 * 原因说明：
 * 1. 后端返回的是偏业务的数据结构，例如 PlayerDto、EquipmentDto、ShopDto。
 * 2. 现有前端页面已经写成了另一套更偏 UI 的字段结构，例如 `player.realmLayer`、`equipmentSlots[].item.stats`。
 * 3. 如果直接在每个组件里零散地手工转换，后面维护时会越来越乱，而且很容易出现同一个字段在不同页面显示不一致的问题。
 *
 * 因此这里统一把后端真实数据转换成”当前旧界面能直接吃”的形状，
 * 这样既能保留你原来的页面结构，也能把真实接口稳定接进来。
 */

import { ICON } from '../icons'

const QUALITY_CLASS_MAP = {
  1: 'common',
  2: 'uncommon',
  3: 'rare',
  4: 'epic',
  5: 'legendary'
}

const SLOT_VIEW_MAP = {
  Weapon: { key: 'weapon', name: '武器', icon: ICON.slot_weapon, order: 1, enumValue: 0 },
  Helmet: { key: 'helmet', name: '头盔', icon: ICON.slot_helmet, order: 2, enumValue: 1 },
  Armor: { key: 'armor', name: '衣服', icon: ICON.slot_armor, order: 3, enumValue: 2 },
  Pants: { key: 'pants', name: '裤子', icon: ICON.slot_pants, order: 4, enumValue: 3 },
  Necklace: { key: 'necklace', name: '项链', icon: ICON.slot_necklace, order: 5, enumValue: 4 },
  Ring: { key: 'ring1', name: '戒指', icon: ICON.slot_ring, order: 6, enumValue: 5 },
  Boots: { key: 'boots', name: '鞋子', icon: ICON.slot_boots, order: 7, enumValue: 6 },
  Treasure: { key: 'treasure', name: '法宝', icon: ICON.slot_treasure, order: 8, enumValue: 7 }
}

const DEFAULT_EQUIPMENT_SLOTS = [
  { type: 'weapon', name: '武器', icon: ICON.slot_weapon, backendSlot: 'Weapon', backendSlotValue: 0, item: null },
  { type: 'armor', name: '衣服', icon: ICON.slot_armor, backendSlot: 'Armor', backendSlotValue: 2, item: null },
  { type: 'helmet', name: '头盔', icon: ICON.slot_helmet, backendSlot: 'Helmet', backendSlotValue: 1, item: null },
  { type: 'pants', name: '裤子', icon: ICON.slot_pants, backendSlot: 'Pants', backendSlotValue: 3, item: null },
  { type: 'boots', name: '鞋子', icon: ICON.slot_boots, backendSlot: 'Boots', backendSlotValue: 6, item: null },
  { type: 'necklace', name: '项链', icon: ICON.slot_necklace, backendSlot: 'Necklace', backendSlotValue: 4, item: null },
  { type: 'ring1', name: '戒指', icon: ICON.slot_ring, backendSlot: 'Ring', backendSlotValue: 5, item: null },
  { type: 'treasure', name: '法宝', icon: ICON.slot_treasure, backendSlot: 'Treasure', backendSlotValue: 7, item: null }
]

const BACKEND_ICON_MAP = {
  'item-consumable': ICON.item_potion,
  'item-material': ICON.item_chest,
  'item-seed': ICON.item_seed,
  'item-chest': ICON.item_chest,
  'item-skillbook': ICON.item_book,
  'item-petegg': ICON.item_egg,
  'item-quest': ICON.item_scroll,
  'item-default': ICON.nav_backpack,
  'unknown-item': ICON.item_chest,
  'shop-general': ICON.nav_shop,
  'shop-blacksmith': ICON.nav_forge
}

const EQUIPMENT_SLOT_BY_VALUE = ['Weapon', 'Helmet', 'Armor', 'Pants', 'Necklace', 'Ring', 'Boots', 'Treasure']

export function getEquipmentIconBySlot(slot) {
  const rawSlot = slot ?? 'Weapon'
  const slotName = typeof rawSlot === 'number' || /^\d+$/.test(String(rawSlot))
    ? EQUIPMENT_SLOT_BY_VALUE[Number(rawSlot)]
    : String(rawSlot)
  return SLOT_VIEW_MAP[slotName]?.icon || ICON.slot_weapon
}

function clamp(value, min, max) {
  return Math.max(min, Math.min(max, value))
}

function calculatePercent(current, max) {
  const safeMax = Math.max(1, Number(max) || 1)
  const safeCurrent = Math.max(0, Number(current) || 0)
  return Math.round(clamp(safeCurrent / safeMax, 0, 1) * 100)
}

function buildStat(name, value) {
  return {
    name,
    label: name,
    value
  }
}

function applyEnhanceGrowth(value) {
  const numericValue = Number(value) || 0
  if (numericValue <= 0) {
    return 0
  }

  return Math.ceil(numericValue * 1.1)
}

function rollbackEnhanceValue(currentValue, enhanceLevel = 0) {
  let resolvedValue = Number(currentValue) || 0
  const safeEnhanceLevel = Math.max(0, Number(enhanceLevel) || 0)

  if (resolvedValue <= 0 || safeEnhanceLevel <= 0) {
    return resolvedValue
  }

  for (let step = 0; step < safeEnhanceLevel; step += 1) {
    const approximate = Math.floor(resolvedValue / 1.1)
    let previousValue = null

    for (let offset = -2; offset <= 2; offset += 1) {
      const candidate = approximate + offset
      if (candidate < 0) {
        continue
      }

      if (applyEnhanceGrowth(candidate) === resolvedValue) {
        previousValue = candidate
        break
      }
    }

    if (previousValue === null) {
      return Number(currentValue) || 0
    }

    resolvedValue = previousValue
  }

  return resolvedValue
}

function buildEnhancedStat(name, value, enhanceLevel = 0) {
  const currentValue = Number(value) || 0
  if (currentValue <= 0) {
    return null
  }

  const originalValue = rollbackEnhanceValue(currentValue, enhanceLevel)
  const enhanceBonus = Math.max(0, currentValue - originalValue)

  return {
    name,
    label: name,
    value: currentValue,
    originalValue,
    enhanceBonus,
    displayValue: enhanceBonus > 0
      ? `${currentValue}（+${enhanceBonus}）`
      : `${currentValue}`
  }
}

export function formatCompactNumber(num = 0) {
  const value = Number(num) || 0
  const absValue = Math.abs(value)

  if (absValue >= 100000000) {
    return `${(value / 100000000).toFixed(2)}亿`
  }

  if (absValue >= 10000) {
    return `${(value / 10000).toFixed(2)}万`
  }

  return value.toLocaleString('zh-CN')
}

/**
 * 中文注释：
 * 后端当前只有等级，没有直接返回“练气期/筑基期”这种前端修仙文案。
 * 这里按照等级区间映射成旧界面需要的境界文字，让 UI 保持原来的修仙风格。
 */
export function getRealmInfo(level = 1) {
  const safeLevel = Math.max(1, Number(level) || 1)
  const realms = ['练气', '筑基', '金丹', '元婴', '化神', '炼虚', '合体', '大乘', '渡劫', '真仙']
  const realmIndex = clamp(Math.floor((safeLevel - 1) / 10), 0, realms.length - 1)
  const realmLayer = ((safeLevel - 1) % 10) + 1
  const currentAlias = `${realms[realmIndex]}${['一', '二', '三', '四', '五', '六', '七', '八', '九', '十'][realmLayer - 1]}层`

  const bodyRealms = ['凡人之躯', '铜皮铁骨', '钢筋铁骨', '金刚不坏', '玉骨仙躯']
  const bodyIndex = clamp(Math.floor((safeLevel - 1) / 15), 0, bodyRealms.length - 1)

  const realmTiers = ['common', 'uncommon', 'rare', 'epic', 'legendary', 'mythic']
  const realmTier = realmTiers[clamp(Math.floor((safeLevel - 1) / 12), 0, realmTiers.length - 1)]

  return {
    realm: realms[realmIndex],
    realmLayer,
    currentAlias,
    realmTier,
    bodyRealm: bodyRealms[bodyIndex]
  }
}

export function getPlayerAvatar(level = 1) {
  if (level >= 60) return '🧙'
  if (level >= 40) return '⚔️'
  if (level >= 20) return '🥷'
  return '🧑'
}

export function getProfessionDisplayName(profession = '') {
  const normalized = String(profession || '').trim().toLowerCase()
  if (normalized === 'mage') return '法'
  if (normalized === 'body') return '体'
  return '战'
}

export function buildPlayerView(player) {
  const source = player || {}
  const level = Number(source.level ?? source.Level ?? 1) || 1
  const exp = Number(source.exp ?? source.Exp ?? 0) || 0
  const maxExp = Math.max(1, Number(source.xExp ?? source.XExp ?? 100) || 100)
  const realmInfo = getRealmInfo(level)
  const breakthroughSource = source.breakthrough ?? source.Breakthrough ?? {}
  const currentRealmName = breakthroughSource.currentRealmName ?? breakthroughSource.CurrentRealmName ?? realmInfo.realm
  const currentRealmLayer = Number(breakthroughSource.currentRealmLayer ?? breakthroughSource.CurrentRealmLayer ?? realmInfo.realmLayer) || realmInfo.realmLayer
  const nextRealmName = breakthroughSource.nextRealmName ?? breakthroughSource.NextRealmName ?? ''
  const currentAlias = breakthroughSource.currentAlias ?? breakthroughSource.CurrentAlias ?? realmInfo.currentAlias
  const nextAlias = breakthroughSource.nextAlias ?? breakthroughSource.NextAlias ?? nextRealmName
  const hp = Number(source.hp ?? source.HP ?? 0) || 0
  const maxHp = Math.max(1, Number(source.maxHp ?? source.MaxHP ?? 1) || 1)
  const mp = Number(source.mp ?? source.MP ?? 0) || 0
  const maxMp = Math.max(1, Number(source.maxMp ?? source.MaxMP ?? 1) || 1)
  const expPercent = calculatePercent(exp, maxExp)
  const hpPercent = calculatePercent(hp, maxHp)
  const mpPercent = calculatePercent(mp, maxMp)

  return {
    id: source.id ?? source.Id ?? '',
    name: source.name ?? source.Name ?? '无名修士',
    profession: source.profession ?? source.Profession ?? 'warrior',
    professionName: source.professionName ?? source.ProfessionName ?? getProfessionDisplayName(source.profession ?? source.Profession ?? 'warrior'),
    currentTitle: source.currentTitle ?? source.CurrentTitle ?? '',
    avatar: getPlayerAvatar(level),
    avatarImagePath: source.avatarImagePath ?? source.AvatarImagePath ?? '',
    level,
    realm: currentRealmName,
    nextRealm: nextRealmName || '境界圆满',
    realmLayer: currentRealmLayer,
    realmTier: realmInfo.realmTier,
    bodyRealm: realmInfo.bodyRealm,
    currentRealmText: currentAlias,
    nextRealmText: nextAlias || '境界圆满',
    hp,
    maxHp,
    hpPercent,
    mp,
    maxMp,
    mpPercent,
    realmExp: exp,
    maxRealmExp: maxExp,
    expPercent,
    bodyExp: Math.floor(exp * 0.4),
    maxBodyExp: Math.max(100, Math.floor(maxExp * 0.7)),
    attack: Number(source.attack ?? source.Attack ?? 0) || 0,
    magicAttack: Number(source.magicAttack ?? source.MagicAttack ?? 0) || 0,
    defense: Number(source.defense ?? source.Defense ?? 0) || 0,
    magicDefense: Number(source.magicDefense ?? source.MagicDefense ?? 0) || 0,
    speed: Number(source.speed ?? source.Speed ?? 0) || 0,
    hitRate: Number(source.hitRate ?? source.HitRate ?? 0) || 0,
    dodgeRate: Number(source.dodgeRate ?? source.DodgeRate ?? 0) || 0,
    critRate: Number(source.critRate ?? source.CritRate ?? 0) || 0,
    critDamage: Number(source.critDamage ?? source.CritDamage ?? 0) || 0,
    comboRate: Number(source.comboRate ?? source.ComboRate ?? 0) || 0,
    counterRate: Number(source.counterRate ?? source.CounterRate ?? 0) || 0,
    armorBreak: Number(source.armorBreak ?? source.ArmorBreak ?? 0) || 0,
    bonusDamage: Number(source.bonusDamage ?? source.BonusDamage ?? 0) || 0,
    gold: Number(source.gold ?? source.Gold ?? 0) || 0,
    spiritStone: Number(source.spiritStone ?? source.SpiritStone ?? 0) || 0,
    honor: Number(source.honor ?? source.Honor ?? 0) || 0,
    guildContribution: Number(source.guildContribution ?? source.GuildContribution ?? 0) || 0,
    guildId: source.guildId ?? source.GuildId ?? '',
    battleCooldownSeconds: Number(source.battleCooldownSeconds ?? source.BattleCooldownSeconds ?? 0) || 0,
    battleCooldownUntilUtc: source.battleCooldownUntilUtc ?? source.BattleCooldownUntilUtc ?? '',
    attributePoints: source.attributePoints ?? source.AttributePoints ?? null,
    breakthrough: {
      completedCount: Number(breakthroughSource.completedCount ?? breakthroughSource.CompletedCount ?? 0) || 0,
      currentRealmName,
      currentRealmLayer,
      currentAlias,
      nextRealmName,
      nextAlias,
      currentLevel: Number(breakthroughSource.currentLevel ?? breakthroughSource.CurrentLevel ?? level) || level,
      nextRequiredLevel: Number(breakthroughSource.nextRequiredLevel ?? breakthroughSource.NextRequiredLevel ?? 0) || 0,
      remainingLevels: Number(breakthroughSource.remainingLevels ?? breakthroughSource.RemainingLevels ?? 0) || 0,
      canBreakthrough: Boolean(breakthroughSource.canBreakthrough ?? breakthroughSource.CanBreakthrough),
      isBreakthroughPoint: Boolean(breakthroughSource.isBreakthroughPoint ?? breakthroughSource.IsBreakthroughPoint),
      breakthroughSuccessRate: Number(breakthroughSource.breakthroughSuccessRate ?? breakthroughSource.BreakthroughSuccessRate ?? 0) || 0,
      breakthroughExpLossPercent: Number(breakthroughSource.breakthroughExpLossPercent ?? breakthroughSource.BreakthroughExpLossPercent ?? 0) || 0,
      requiredMaterials: Array.isArray(breakthroughSource.requiredMaterials ?? breakthroughSource.RequiredMaterials)
        ? (breakthroughSource.requiredMaterials ?? breakthroughSource.RequiredMaterials).map((material) => ({
            itemId: material.itemId ?? material.ItemId ?? '',
            name: material.name ?? material.Name ?? '',
            count: Number(material.count ?? material.Count ?? 0) || 0,
            ownedCount: Number(material.ownedCount ?? material.OwnedCount ?? 0) || 0,
            isEnough: Boolean(material.isEnough ?? material.IsEnough)
          }))
        : [],
      attributeBonusPercent: Number(breakthroughSource.attributeBonusPercent ?? breakthroughSource.AttributeBonusPercent ?? 0) || 0,
      requirementText: breakthroughSource.requirementText ?? breakthroughSource.RequirementText ?? ''
    }
  }
}

export function getQualityClass(quality = 1) {
  return QUALITY_CLASS_MAP[Number(quality) || 1] || 'common'
}

export function getCurrencyIcon(currencyType = '') {
  const normalized = String(currencyType).toLowerCase()

  if (normalized.includes('spirit')) return ICON.currency_spirit
  return ICON.currency_gold
}

export function getItemIconByType(itemType = '', name = '') {
  const normalizedType = String(itemType).toLowerCase()
  const normalizedName = String(name)

  if (normalizedType.includes('equipment')) return ICON.slot_armor
  if (normalizedType.includes('inventoryexpansion') || normalizedType.includes('inventory_expansion')) return ICON.nav_backpack
  if (normalizedType.includes('chest')) return ICON.item_chest
  if (normalizedType.includes('seed')) return ICON.item_seed
  if (normalizedType.includes('petegg') || normalizedName.includes('蛋')) return ICON.item_egg
  if (normalizedType.includes('consumable')) return ICON.item_potion
  if (normalizedType.includes('pill')) return ICON.item_potion
  if (normalizedType.includes('material')) return ICON.item_chest
  if (normalizedType.includes('quest')) return ICON.item_scroll
  if (normalizedType.includes('gem')) return ICON.misc_diamond
  if (normalizedType.includes('skill') || normalizedType.includes('book')) return ICON.item_book
  if (normalizedType.includes('recipe') || normalizedType.includes('scroll')) return ICON.item_scroll
  if (normalizedName.includes('血')) return ICON.item_heart_red
  if (normalizedName.includes('丹') || normalizedName.includes('药')) return ICON.item_potion
  if (normalizedName.includes('蓝') || normalizedName.includes('灵')) return ICON.stat_mp
  if (normalizedName.includes('书') || normalizedName.includes('功法')) return ICON.item_book
  if (normalizedName.includes('草') || normalizedName.includes('药')) return ICON.item_seed
  if (normalizedName.includes('石')) return ICON.currency_spirit
  return ICON.nav_backpack
}

export function getItemTypeLabel(itemType = '', name = '') {
  const normalizedType = String(itemType).toLowerCase()
  const normalizedName = String(name)

  if (normalizedType.includes('equipment')) return '装备'
  if (normalizedType.includes('inventoryexpansion') || normalizedType.includes('inventory_expansion')) return '背包扩容'
  if (normalizedType.includes('chest')) return '道具箱'
  if (normalizedType.includes('seed')) return '种子'
  if (normalizedType.includes('petegg')) return '宠物蛋'
  if (normalizedType.includes('pill')) return '丹药'
  if (normalizedType.includes('consumable')) return '消耗品'
  if (normalizedType.includes('material')) return '材料'
  if (normalizedType.includes('quest')) return '任务物品'
  if (normalizedType.includes('gem')) return '宝石'
  if (normalizedType.includes('skill') || normalizedType.includes('book')) return '功法'
  if (normalizedType.includes('recipe') || normalizedType.includes('scroll')) return '配方卷轴'
  if (normalizedName.includes('书') || normalizedName.includes('诀') || normalizedName.includes('功法')) return '功法'
  if (normalizedName.includes('石') || normalizedName.includes('矿') || normalizedName.includes('材料')) return '材料'

  return normalizedType ? String(itemType) : '道具'
}

export function resolveDisplayIcon(icon, fallbackIcon = ICON.nav_backpack) {
  const rawIcon = String(icon ?? '').trim()
  if (!rawIcon) {
    return fallbackIcon
  }

  const normalized = rawIcon.toLowerCase()
  if (BACKEND_ICON_MAP[normalized]) {
    return BACKEND_ICON_MAP[normalized]
  }

  if (normalized.startsWith('equipment-')) {
    const slotKey = normalized.slice('equipment-'.length)
    const slotView = Object.entries(SLOT_VIEW_MAP)
      .find(([slotName]) => slotName.toLowerCase() === slotKey)?.[1]
    return slotView?.icon || fallbackIcon
  }

  if (normalized.startsWith('item-')) {
    return getItemIconByType(normalized.slice('item-'.length), '')
  }

  // If it looks like an emoji (no ASCII letters, not a path), use fallback
  if (!/[a-z]/i.test(rawIcon) && !rawIcon.includes('/') && !rawIcon.includes('.')) {
    return fallbackIcon
  }

  return rawIcon
}

function buildEquipmentStats(equipment) {
  const stats = []
  const enhanceLevel = Number(equipment.EnhanceLevel ?? equipment.enhanceLevel ?? 0) || 0

  const baseStats = [
    buildEnhancedStat('物攻', equipment.BasePhysicalAttack ?? equipment.basePhysicalAttack ?? 0, enhanceLevel),
    buildEnhancedStat('法攻', equipment.BaseMagicAttack ?? equipment.baseMagicAttack ?? 0, enhanceLevel),
    buildEnhancedStat('物防', equipment.BasePhysicalDefense ?? equipment.basePhysicalDefense ?? 0, enhanceLevel),
    buildEnhancedStat('法防', equipment.BaseMagicDefense ?? equipment.baseMagicDefense ?? 0, enhanceLevel),
    buildEnhancedStat('生命', equipment.BaseHP ?? equipment.baseHP ?? 0, enhanceLevel),
    buildEnhancedStat('法力', equipment.BaseMP ?? equipment.baseMP ?? 0, enhanceLevel)
  ].filter(Boolean)

  stats.push(...baseStats)

  // 构建洗练词条的 StatType → 品阶信息映射
  const rerollMap = new Map()
  const rerollStats = equipment.RerollStats || equipment.rerollStats || []
  rerollStats.forEach((r) => {
    const key = r.StatType || r.statType || ''
    if (key) {
      rerollMap.set(key, {
        tierName: r.TierName || r.tierName || '',
        tierColor: r.TierColor || r.tierColor || '#9ca3af'
      })
    }
  })

  const bonusStats = equipment.BonusStats || equipment.bonusStats || []
  const matchedTypes = new Set()

  bonusStats.forEach((bonus) => {
    const bonusValue = bonus.isPercentage || bonus.IsPercentage
      ? `${bonus.Value ?? bonus.value ?? 0}%`
      : (bonus.Value ?? bonus.value ?? 0)

    const statType = bonus.StatType || bonus.statType || ''
    const rerollInfo = rerollMap.get(statType)
    if (rerollInfo) matchedTypes.add(statType)

    stats.push({
      ...buildStat(
        bonus.Description || bonus.description || statType || '附加属性',
        bonusValue
      ),
      ...(rerollInfo ? { tierName: rerollInfo.tierName, tierColor: rerollInfo.tierColor } : {})
    })
  })

  // 洗练词条中未匹配 BonusStats 的属性（如新增的词条类型），追加到列表末尾
  rerollStats.forEach((r) => {
    const statType = r.StatType || r.statType || ''
    if (matchedTypes.has(statType)) return
    const value = r.IsPercentage ?? r.isPercentage
      ? `${r.Value ?? r.value ?? 0}%`
      : `+${r.Value ?? r.value ?? 0}`
    stats.push({
      label: r.Description || r.description || '洗练属性',
      name: r.Description || r.description || '洗练属性',
      value,
      displayValue: value,
      tierName: r.TierName || r.tierName || '',
      tierColor: r.TierColor || r.tierColor || '#9ca3af'
    })
  })

  if (enhanceLevel > 0) {
    stats.push(buildStat('强化', `+${enhanceLevel}`))
  }

  return stats
}

export function buildEquipmentCard(equipment) {
  const backendSlot = equipment.SlotName ?? equipment.slotName ?? equipment.Slot ?? equipment.slot ?? 'Weapon'
  const slotView = SLOT_VIEW_MAP[backendSlot] || SLOT_VIEW_MAP.Weapon
  const qualityName = equipment.QualityName ?? equipment.qualityName ?? '普通'
  const combatStyleName = equipment.CombatStyleName ?? equipment.combatStyleName ?? ''
  const weaponCategoryName = equipment.WeaponCategoryName ?? equipment.weaponCategoryName ?? ''
  const typeSegments = backendSlot === 'Weapon'
    ? [slotView.name, combatStyleName, weaponCategoryName, qualityName]
    : [slotView.name, combatStyleName && combatStyleName !== '中性' ? combatStyleName : '', qualityName]

  // 没有配置装备图片时始终使用对应部位的默认 PNG，
  // 不再根据武器类别回退到表情符号。
  const displayIcon = resolveDisplayIcon(
    equipment.Icon || equipment.IconPath || equipment.icon || equipment.iconPath,
    slotView.icon
  )

  return {
    id: equipment.InstanceId ?? equipment.instanceId ?? '',
    instanceId: equipment.InstanceId ?? equipment.instanceId ?? '',
    templateId: equipment.TemplateId ?? equipment.templateId ?? '',
    backendSlot,
    slotOrder: slotView.order,
    name: equipment.Name ?? equipment.name ?? '未知装备',
    icon: displayIcon,
    quality: getQualityClass(equipment.Quality ?? equipment.quality),
    type: slotView.name,
    typeText: typeSegments.filter(Boolean).join(' · '),
    enhance: equipment.EnhanceLevel ?? equipment.enhanceLevel ?? 0,
    level: equipment.Level ?? equipment.level ?? 1,
    isLocked: Boolean(equipment.IsLocked ?? equipment.isLocked ?? equipment.IsBound ?? equipment.isBound),
    isEquipped: Boolean(equipment.IsEquipped ?? equipment.isEquipped),
    identified: true,
    stats: buildEquipmentStats(equipment),
    description: equipment.Description ?? equipment.description ?? '暂无描述',
    rerollStats: (equipment.RerollStats || equipment.rerollStats || []).map(s => ({
      description: s.Description || s.description || '',
      value: s.Value ?? s.value ?? 0,
      isPercentage: s.IsPercentage ?? s.isPercentage ?? false,
      tier: s.Tier ?? s.tier ?? 0,
      tierName: s.TierName || s.tierName || '',
      tierColor: s.TierColor || s.tierColor || '#9ca3af',
      index: s.Index ?? s.index ?? 0
    })),
    rerollCount: equipment.RerollCount ?? equipment.rerollCount ?? 0,
    hasRerollStats: Boolean((equipment.RerollStats || equipment.rerollStats || []).length),
    gemSlots: (equipment.GemSlots || equipment.gemSlots || []).map(s => ({
      slotIndex: s.SlotIndex ?? s.slotIndex ?? 0,
      gemId: s.GemId || s.gemId || null,
      gemName: s.GemName || s.gemName || null,
      gemLevel: s.GemLevel ?? s.gemLevel ?? null,
      bonusText: s.BonusText || s.bonusText || null
    })),
    maxGemSlots: equipment.MaxGemSlots ?? equipment.maxGemSlots ?? 0,
    hasGemSlots: (equipment.MaxGemSlots ?? equipment.maxGemSlots ?? 0) > 0
  }
}

export function buildEquipmentSlots(equippedItems = []) {
  const slots = DEFAULT_EQUIPMENT_SLOTS.map((slot) => ({ ...slot, item: null }))
  const ringSlots = slots.filter((slot) => slot.backendSlot === 'Ring')

  equippedItems.forEach((equipment) => {
    const backendSlot = equipment.SlotName ?? equipment.slotName ?? equipment.Slot ?? equipment.slot
    const mapping = SLOT_VIEW_MAP[backendSlot]
    const card = buildEquipmentCard(equipment)

    if (backendSlot === 'Ring') {
      const nextRingSlot = ringSlots.find((slot) => !slot.item)
      if (nextRingSlot) {
        nextRingSlot.item = card
      }
      return
    }

    if (!mapping) {
      return
    }

    const targetSlot = slots.find((slot) => slot.type === mapping.key)
    if (targetSlot) {
      targetSlot.item = card
    }
  })

  return slots
}

export function buildInventoryEquipmentList(equipments = []) {
  return (equipments || [])
    .map((equipment) => buildEquipmentCard(equipment))
    .sort((left, right) => left.slotOrder - right.slotOrder || (right.enhance || 0) - (left.enhance || 0))
}

export function buildInventoryPropList(items = []) {
  return (items || []).map((item) => ({
    id: item.Id ?? item.id ?? '',
    itemId: item.ItemId ?? item.itemId ?? '',
    itemType: item.ItemType ?? item.itemType ?? '',
    name: item.Name ?? item.name ?? '未知道具',
    icon: resolveDisplayIcon(
      item.Icon || item.icon,
      getItemIconByType(item.ItemType ?? item.itemType, item.Name ?? item.name)
    ),
    quality: getQualityClass(item.Quality ?? item.quality),
    type: getItemTypeLabel(item.ItemType ?? item.itemType, item.Name ?? item.name),
    count: item.Quantity ?? item.quantity ?? 0,
    isNew: false,
    description: item.Description ?? item.description ?? '暂无描述',
    isBound: Boolean(item.IsBound ?? item.isBound),
    canUse: Boolean(item.CanUse ?? item.canUse),
    useActionText: item.UseActionText ?? item.useActionText ?? '使用',
    canGift: Boolean(item.CanGift ?? item.canGift) || String(item.ItemType ?? item.itemType ?? '').toLowerCase().includes('favorability')
  }))
}

function buildShopEffect(item) {
  const itemType = item.ItemType ?? item.itemType ?? ''

  if (String(itemType).toLowerCase().includes('equipment')) {
    return `${item.QualityName ?? item.qualityName ?? '普通'}装备`
  }

  return item.Description || item.description || '购买后可在背包中使用'
}

export function buildShopItemsByTab(shops = []) {
  const groups = {
    equipment: [],
    consumables: [],
    materials: [],
    skillbooks: [],
    gems: []
  }

  ;(shops || []).forEach((shop) => {
    ;(shop.Items || shop.items || []).forEach((item) => {
      const stockValue = item.Stock ?? item.stock ?? -1
      const itemType = String(item.ItemType ?? item.itemType ?? '')
      const normalizedItemType = itemType.toLowerCase()
      const itemName = item.Name ?? item.name ?? ''
      const entry = {
        id: `${shop.ShopId ?? shop.shopId}_${item.ItemId ?? item.itemId}`,
        itemId: item.ItemId ?? item.itemId ?? '',
        shopId: shop.ShopId ?? shop.shopId ?? '',
        name: itemName || '未知商品',
        icon: resolveDisplayIcon(
          item.Icon || item.icon,
          normalizedItemType.includes('equipment')
            ? getEquipmentIconBySlot(item.SlotName ?? item.slotName ?? item.Slot ?? item.slot)
            : getItemIconByType(itemType, itemName)
        ),
        quality: getQualityClass(item.Quality ?? item.quality),
        type: getItemTypeLabel(itemType, itemName),
        effect: buildShopEffect(item),
        price: item.Price?.Amount ?? item.price?.amount ?? 0,
        currency: String(item.Price?.CurrencyType ?? item.price?.currencyType ?? 'Gold').toLowerCase(),
        stock: stockValue < 0 ? 9999 : stockValue,
        description: item.Description ?? item.description ?? '暂无描述',
        canPurchase: Boolean(item.CanPurchase ?? item.canPurchase ?? true),
        requiredLevel: item.RequiredLevel ?? item.requiredLevel ?? 1,
        remainingDailyLimit: item.RemainingDailyLimit ?? item.remainingDailyLimit ?? -1,
        stats: (item.Stats ?? item.stats ?? []).map((stat) => ({
          name: stat.Name ?? stat.name ?? '属性',
          value: stat.Value ?? stat.value ?? ''
        }))
      }

      entry.tooltipStats = normalizedItemType.includes('equipment') && entry.stats.length > 0
        ? [
            { name: '类型', value: entry.type },
            { name: '等级', value: `Lv.${entry.requiredLevel}` },
            ...entry.stats
          ]
        : [
            { name: '类型', value: entry.type },
            { name: '效果', value: entry.effect }
          ]

      const name = entry.name

      if (normalizedItemType.includes('equipment')) {
        groups.equipment.push(entry)
      } else if (normalizedItemType.includes('gem')) {
        groups.gems.push(entry)
      } else if (name.includes('书') || name.includes('诀') || name.includes('功法')) {
        groups.skillbooks.push(entry)
      } else if (name.includes('石') || name.includes('矿') || name.includes('材料')) {
        groups.materials.push(entry)
      } else {
        groups.consumables.push(entry)
      }
    })
  })

  return groups
}

export function buildMapCards(maps = []) {
  return (maps || []).map((map) => {
    const recommendedLevel = Number(map.RecommendedLevel ?? map.recommendedLevel ?? 1) || 1
    const realmInfo = getRealmInfo(recommendedLevel)
    const difficulty = recommendedLevel >= 40 ? 'hard' : recommendedLevel >= 20 ? 'normal' : 'easy'
    const monsterCountMin = Number(map.MonsterCountMin ?? map.monsterCountMin ?? 1) || 1
    const monsterCountMax = Number(map.MonsterCountMax ?? map.monsterCountMax ?? monsterCountMin) || monsterCountMin
    const availabilityValue = map.IsAvailable ?? map.isAvailable
    const isAvailable = availabilityValue === undefined ? true : Boolean(availabilityValue)
    const monsterPreviews = Array.isArray(map.Monsters ?? map.monsters) ? (map.Monsters ?? map.monsters) : []
    const monsterLevels = monsterPreviews
      .map((monster) => Number(monster.Level ?? monster.level ?? 0))
      .filter((monsterLevel) => monsterLevel > 0)
    const monsterNames = [...new Set(monsterPreviews
      .map((monster) => monster.Name ?? monster.name ?? '')
      .filter(Boolean))]
    const mapDrops = Array.isArray(map.Drops ?? map.drops) ? (map.Drops ?? map.drops) : []

    return {
      id: map.MapId ?? map.mapId ?? '',
      mapId: map.MapId ?? map.mapId ?? '',
      name: map.Name ?? map.name ?? '未知地图',
      icon: difficulty === 'hard' ? ICON.element_fire : difficulty === 'normal' ? ICON.element_wood : ICON.element_earth,
      recommendRealm: `推荐等级 Lv.${recommendedLevel}`,
      entryMethod: '普通地图',
      difficulty,
      difficultyText: difficulty === 'hard' ? '困难' : difficulty === 'normal' ? '普通' : '简单',
      description: map.Description ?? map.description ?? '暂无地图描述',
      minRealm: realmInfo.realm,
      minBody: realmInfo.bodyRealm,
      teamSize: 1,
      enemyTypes: monsterNames.length > 0 ? monsterNames.join('、') : '暂无敌人信息',
      enemyLevelMin: monsterLevels.length > 0 ? Math.min(...monsterLevels) : 0,
      enemyLevelMax: monsterLevels.length > 0 ? Math.max(...monsterLevels) : 0,
      hasBoss: false,
      drops: mapDrops.map((drop) => ({
        name: drop.Name ?? drop.name ?? '未知掉落',
        icon: drop.IconPath ?? drop.iconPath ?? ICON.nav_redeem,
        quality: getQualityClass(drop.Quality ?? drop.quality)
      })),
      // 中文注释：
      // 普通地图只有“是否已开放”的概念，没有副本那种每日次数限制。
      // 因此前端这里把 unlocked 和 canChallenge 统一绑定到后端 IsAvailable，
      // 这样主界面选择地图、地图弹窗高亮、开始战斗校验都会使用同一份开放状态，避免出现显示能进但点开报错的情况。
      unlocked: isAvailable,
      canChallenge: isAvailable,
      unavailableReason: map.UnavailableReason ?? map.unavailableReason ?? '',
      monsterCountText: `${monsterCountMin}-${monsterCountMax}只`,
      raw: map
    }
  })
}
export function buildDungeonCards(dungeons = []) {
  return (dungeons || []).map((dungeon) => {
    const recommendedLevel = Number(dungeon.RecommendedLevel ?? dungeon.recommendedLevel ?? 1) || 1
    const realmInfo = getRealmInfo(recommendedLevel)
    const difficulty = recommendedLevel >= 40 ? 'hard' : recommendedLevel >= 20 ? 'normal' : 'easy'
    const requiredTeamSize = Number(dungeon.RequiredTeamSize ?? dungeon.requiredTeamSize ?? 1) || 1
    const supportsParty = Boolean(dungeon.SupportsParty ?? dungeon.supportsParty)

    return {
      id: dungeon.DungeonId ?? dungeon.dungeonId ?? '',
      dungeonId: dungeon.DungeonId ?? dungeon.dungeonId ?? '',
      name: dungeon.Name ?? dungeon.name ?? '未知副本',
      icon: difficulty === 'hard' ? ICON.element_fire : difficulty === 'normal' ? ICON.element_wood : ICON.element_earth,
      recommendRealm: `推荐等级 Lv.${recommendedLevel}`,
      difficulty,
      difficultyText: difficulty === 'hard' ? '困难' : difficulty === 'normal' ? '普通' : '简单',
      description: dungeon.Description ?? dungeon.description ?? '暂无副本描述',
      entryMethod: '组队副本，个人奖励次数独立结算',
      minRealm: realmInfo.realm,
      minBody: realmInfo.bodyRealm,
      teamSize: requiredTeamSize,
      requiredTeamSize,
      supportsParty,
      enemyTypes: recommendedLevel >= 30 ? '妖兽、守卫' : '野兽、妖兽',
      enemyLevelMin: Math.max(1, recommendedLevel - 3),
      enemyLevelMax: recommendedLevel + 3,
      hasBoss: recommendedLevel >= 10,
      drops: [{ name: '副本奖励', icon: ICON.nav_redeem, quality: 'rare' }],
      unlocked: Boolean(dungeon.IsAvailable ?? dungeon.isAvailable),
      canChallenge: Boolean(dungeon.IsAvailable ?? dungeon.isAvailable),
      dailyLimit: dungeon.DailyLimit ?? dungeon.dailyLimit ?? 0,
      todayCount: dungeon.TodayCount ?? dungeon.todayCount ?? 0,
      remainingCount: dungeon.RemainingCount ?? dungeon.remainingCount ?? 0,
      unavailableReason: dungeon.UnavailableReason ?? dungeon.unavailableReason ?? '',
      raw: dungeon
    }
  })
}

export function resolveBackendSlotValue(slotType) {
  const slot = DEFAULT_EQUIPMENT_SLOTS.find((item) => item.type === slotType)
  return slot ? slot.backendSlotValue : 0
}

// ==================== 宗门系统 DTO 归一化 ====================

export function normalizeSectTemplate(raw) {
  if (!raw) return null
  return {
    sectId: raw.sectId || raw.SectId || '',
    name: raw.name || raw.Name || '',
    description: raw.description || raw.Description || '',
    icon: raw.icon || raw.Icon || '',
    portraitPath: raw.portraitPath || raw.PortraitPath || '',
    heartSutraIdsJson: raw.heartSutraIdsJson || raw.HeartSutraIdsJson || '[]',
    memberCount: raw.memberCount ?? raw.MemberCount ?? 0,
    isEnabled: raw.isEnabled ?? raw.IsEnabled ?? true,
    sortOrder: raw.sortOrder ?? raw.SortOrder ?? 0,
  }
}

export function normalizeSectDetail(raw) {
  const base = normalizeSectTemplate(raw)
  if (!base) return null
  return {
    ...base,
    guildLevel: raw.guildLevel ?? raw.GuildLevel ?? 1,
    announcement: raw.announcement || raw.Announcement || '',
    heartSutras: (raw.heartSutras || raw.HeartSutras || []).map(normalizeHeartSutra),
  }
}

export function normalizeHeartSutra(raw) {
  if (!raw) return null
  return {
    sutraId: raw.sutraId || raw.SutraId || '',
    name: raw.name || raw.Name || '',
    description: raw.description || raw.Description || '',
    maxLayer: raw.maxLayer ?? raw.MaxLayer ?? 10,
    currentLayer: raw.currentLayer ?? raw.CurrentLayer ?? 0,
    layers: (raw.layers || raw.Layers || []).map(normalizeSutraLayer),
  }
}

export function normalizeSutraLayer(raw) {
  if (!raw) return null
  return {
    layer: raw.layer ?? raw.Layer ?? 0,
    name: raw.name || raw.Name || '',
    contributionCost: raw.contributionCost ?? raw.ContributionCost ?? 0,
    goldCost: raw.goldCost ?? raw.GoldCost ?? 0,
    bonuses: (raw.bonuses || raw.Bonuses || []).map(b => ({
      attributeName: b.attributeName || b.AttributeName || '',
      value: b.value ?? b.Value ?? 0,
      isPercentage: b.isPercentage ?? b.IsPercentage ?? false,
    })),
    unlockSkillId: raw.unlockSkillId || raw.UnlockSkillId || null,
    unlockBuffId: raw.unlockBuffId || raw.UnlockBuffId || null,
    isUnlocked: raw.isUnlocked ?? raw.IsUnlocked ?? false,
  }
}

export function normalizeDonationStatus(raw) {
  if (!raw) return null
  return {
    canDonate: raw.canDonate ?? raw.CanDonate ?? true,
    goldAmount: raw.goldAmount ?? raw.GoldAmount ?? 0,
    contributionReward: raw.contributionReward ?? raw.ContributionReward ?? 0,
    alreadyDonatedToday: raw.alreadyDonatedToday ?? raw.AlreadyDonatedToday ?? false,
  }
}

export function normalizeSectTournament(raw) {
  if (!raw) return null
  return {
    tournamentId: raw.tournamentId || raw.TournamentId || '',
    state: raw.state ?? raw.State ?? 0,
    startTime: raw.startTime || raw.StartTime || '',
    endTime: raw.endTime || raw.EndTime || '',
    myRank: raw.myRank ?? raw.MyRank ?? 0,
    participants: raw.participants || raw.Participants || [],
  }
}

export function normalizeSectBossStatus(raw) {
  if (!raw) return null
  return {
    bossName: raw.bossName || raw.BossName || '',
    currentHp: raw.currentHp ?? raw.CurrentHp ?? 0,
    maxHp: raw.maxHp ?? raw.MaxHp ?? 0,
    endTime: raw.endTime || raw.EndTime || '',
    myDamage: raw.myDamage ?? raw.MyDamage ?? 0,
    myRank: raw.myRank ?? raw.MyRank ?? 0,
    canClaim: raw.canClaim ?? raw.CanClaim ?? false,
  }
}

export function normalizeSectShopItem(raw) {
  if (!raw) return null
  return {
    gid: raw.gid || raw.GID || '',
    itemId: raw.itemId || raw.ItemId || '',
    itemName: raw.itemName || raw.ItemName || '',
    itemType: raw.itemType ?? raw.ItemType ?? 0,
    contributionCost: raw.contributionCost ?? raw.ContributionCost ?? 0,
    stock: raw.stock ?? raw.Stock ?? -1,
    dailyLimit: raw.dailyLimit ?? raw.DailyLimit ?? -1,
    purchasedToday: raw.purchasedToday ?? raw.PurchasedToday ?? 0,
  }
}

export function normalizeSectBlessing(raw) {
  if (!raw) return null
  return {
    blessingId: raw.blessingId || raw.BlessingId || '',
    name: raw.name || raw.Name || '',
    description: raw.description || raw.Description || '',
    requiredGuildLevel: raw.requiredGuildLevel ?? raw.RequiredGuildLevel ?? 1,
    isActive: raw.isActive ?? raw.IsActive ?? false,
  }
}

export function normalizeSectDisciple(raw) {
  if (!raw) return null
  return {
    playerId: raw.playerId || raw.PlayerId || '',
    playerName: raw.playerName || raw.PlayerName || '',
    playerLevel: raw.playerLevel ?? raw.PlayerLevel ?? 1,
    position: raw.position || raw.Position || 'Member',
    contribution: raw.contribution ?? raw.Contribution ?? 0,
    element: raw.element || raw.Element || 'None',
    profession: raw.profession || raw.Profession || '',
    totalBattles: raw.totalBattles ?? raw.TotalBattles ?? 0,
    winBattles: raw.winBattles ?? raw.WinBattles ?? 0,
  }
}
