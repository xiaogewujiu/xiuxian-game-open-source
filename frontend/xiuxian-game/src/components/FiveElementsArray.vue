<template>
  <!-- 五行聚灵阵 -->
  <div class="five-elements-array" :class="{ 'in-modal': inModal }">
    <div v-if="!inModal" class="array-header">
      <div class="header-title">
        <AssetIcon :source="ICON.misc_yinyang" size="20" />
        <span class="title-text">五行聚灵阵</span>
      </div>
      <span class="level-text">Lv.{{ arrayLevel }}</span>
    </div>

    <div class="array-body">
      <div class="elements-strip" aria-label="五行元素">
        <button
          v-for="(element, key) in elements"
          :key="key"
          type="button"
          class="element-node"
          :class="key"
          @mouseenter="hoveredElementKey = key"
          @mouseleave="hoveredElementKey = null"
          @click="openElementUpgrade(key)"
        >
          <span class="element-node-icon"><AssetIcon :source="element.icon" size="20" /></span>
          <span class="element-node-name">{{ element.name }}</span>
          <span class="element-node-level">Lv.{{ element.level }}</span>
          <span class="element-node-track">
            <span class="element-node-fill" :style="{ width: Math.min(100, element.level / element.maxLevel * 100) + '%' }"></span>
          </span>
          <span v-if="hoveredElementKey === key" class="element-tooltip" role="tooltip">
            {{ element.name }}属性<br />
            等级：Lv.{{ element.level }} / Lv.{{ element.maxLevel }}<br />
            加成属性：{{ elementBonuses[key]?.attributeName || '未配置' }}<br />
            当前加成：+{{ elementBonuses[key]?.currentBonus || 0 }}<br />
            每级增加：+{{ elementBonuses[key]?.bonusPerLevel || 0 }}
          </span>
        </button>
      </div>

      <div class="effects-panel">
        <div class="panel-title">当前加成</div>
        <div class="effects-list">
          <div class="effect-item" v-for="(effect, index) in effectsList" :key="index">
            <span class="effect-name">{{ effect.name }}</span>
            <strong class="effect-value">+{{ effect.value }}{{ effect.unit }}</strong>
          </div>
        </div>
      </div>

      <div class="array-upgrade-section">
        <div class="upgrade-main">
          <div class="upgrade-info">
            <span class="upgrade-title">阵法等级 <strong>Lv.{{ arrayLevel }}</strong><small>/ {{ maxArrayLevel }}</small></span>
            <span class="level-progress">灵田 +{{ spiritFieldYieldBonus }}% · 战斗经验 +{{ battleExpBonus }}% · 五行等级上限 Lv.{{ maxElementLevel }}</span>
          </div>
          <button
            class="upgrade-btn"
            :class="{ 'can-upgrade': selectedDisplay.upgradeState === 'ready' }"
            :disabled="selectedDisplay.upgradeState === 'max'"
            @click="openSelectedUpgrade"
          >
            <span>{{ selectedDisplay.upgradeLabel }}</span>
          </button>
        </div>
        <div class="progress-bar">
          <div class="progress-fill" :style="{ width: (arrayLevel / maxArrayLevel * 100) + '%' }"></div>
        </div>
      </div>
    </div>

    <!-- 元素升级弹窗 -->
    <Teleport to="body">
      <div v-if="showUpgradeModal" class="upgrade-modal-overlay" @click.self="closeUpgradeModal">
        <div class="upgrade-modal">
          <div class="modal-header" :class="'modal-header-' + selectedElementKey">
            <span class="modal-icon"><AssetIcon :source="selectedElementData?.icon" size="28" /></span>
            <span class="modal-title">{{ selectedElementData?.name }}元素升级</span>
            <button class="close-btn" @click="closeUpgradeModal">×</button>
          </div>
          <div class="modal-body">
            <div class="element-preview" :class="'preview-' + selectedElementKey">
              <div class="preview-aura"></div>
              <div class="preview-ring"></div>
              <div class="preview-icon"><AssetIcon :source="selectedElementData?.icon" size="36" /></div>
              <div class="preview-level">Lv.{{ selectedElementData?.level }}</div>
            </div>

            <div class="upgrade-details">
              <div class="detail-item">
                <span class="detail-label">等级</span>
                <span class="detail-value">{{ selectedElementData?.level }}/{{ selectedElementData?.maxLevel }}</span>
              </div>
              <div class="detail-item" v-if="selectedElementData">
                <span class="detail-label">加成属性</span>
                <span class="detail-value">{{ elementBonuses[selectedElementKey]?.attributeName || '未配置' }}</span>
              </div>
              <div class="detail-item" v-if="selectedElementData">
                <span class="detail-label">当前加成</span>
                <span class="detail-value bonus">+{{ elementBonuses[selectedElementKey]?.currentBonus || 0 }}</span>
              </div>
              <div class="detail-item" v-if="selectedElementData && selectedElementData.level < selectedElementData.maxLevel">
                <span class="detail-label">升级后</span>
                <span class="detail-value next-bonus">+{{ (elementBonuses[selectedElementKey]?.currentBonus || 0) + (elementBonuses[selectedElementKey]?.bonusPerLevel || 0) }}</span>
              </div>
              <div class="detail-item" v-if="selectedElementData && selectedElementData.level < selectedElementData.maxLevel">
                <span class="detail-label">等级上限</span>
                <span class="detail-value next-bonus">Lv.{{ selectedElementData.maxLevel }}</span>
              </div>
            </div>

            <div class="upgrade-actions">
              <div v-if="selectedElementData && selectedElementData.level < selectedElementData.maxLevel" class="modal-materials">
                <div class="upgrade-material upgrade-currency" :class="{ enough: playerGold >= selectedElementCost.goldCost }">
                  <span class="upgrade-material-kind">货币</span>
                  <span class="upgrade-material-name">金币</span>
                  <span class="upgrade-material-count">{{ formatNumber(playerGold) }} / {{ formatNumber(selectedElementCost.goldCost) }}</span>
                </div>
                <div class="upgrade-material upgrade-currency" :class="{ enough: playerSpiritStone >= selectedElementCost.spiritStoneCost }">
                  <span class="upgrade-material-kind">货币</span>
                  <span class="upgrade-material-name">灵石</span>
                  <span class="upgrade-material-count">{{ formatNumber(playerSpiritStone) }} / {{ formatNumber(selectedElementCost.spiritStoneCost) }}</span>
                </div>
                <div
                  v-for="material in selectedElementCost.materials"
                  :key="material.itemId"
                  class="upgrade-material"
                  :class="{ enough: getInventoryCount(material.itemId) >= material.count }"
                >
                  <span class="upgrade-material-kind">材料</span>
                  <span class="upgrade-material-name">{{ material.itemName }}</span>
                  <span class="upgrade-material-count">{{ getInventoryCount(material.itemId) }} / {{ material.count }}</span>
                </div>
              </div>
              <div v-if="selectedElementData && selectedElementData.level >= selectedElementData.maxLevel" class="cost-info">
                <span class="cost-value">五行等级已达到当前上限</span>
              </div>
              <button
                v-if="selectedElementData && selectedElementData.level < selectedElementData.maxLevel"
                class="confirm-upgrade-btn"
                :class="{ 'can-upgrade': canUpgradeElement(selectedElementKey) }"
                :disabled="!canUpgradeElement(selectedElementKey)"
                @click="upgradeElement(selectedElementKey)"
              >
                确认升级
              </button>
              <button v-else class="confirm-upgrade-btn max">已达满级</button>
            </div>
          </div>
        </div>
      </div>
    </Teleport>

    <!-- 阵法升级弹窗 -->
    <Teleport to="body">
      <div v-if="showArrayUpgradeModal" class="upgrade-modal-overlay" @click.self="closeArrayUpgradeModal">
        <div class="upgrade-modal">
          <div class="modal-header">
            <span class="modal-icon"><AssetIcon :source="ICON.misc_yinyang" size="28" /></span>
            <span class="modal-title">聚灵阵升级</span>
            <button class="close-btn" @click="closeArrayUpgradeModal">×</button>
          </div>
          <div class="modal-body">
            <div class="element-preview">
              <div class="preview-aura"></div>
              <div class="preview-ring"></div>
              <div class="preview-icon"><AssetIcon :source="ICON.misc_yinyang" size="36" /></div>
              <div class="preview-level">Lv.{{ arrayLevel }}</div>
            </div>

            <div class="upgrade-details">
              <div class="detail-item">
                <span class="detail-label">当前等级</span>
                <span class="detail-value">{{ arrayLevel }}/{{ maxArrayLevel }}</span>
              </div>
              <div class="detail-item">
                <span class="detail-label">灵田加成</span>
                <span class="detail-value bonus">+{{ spiritFieldYieldBonus }}%</span>
              </div>
              <div class="detail-item">
                <span class="detail-label">战斗经验</span>
                <span class="detail-value next-bonus">+{{ battleExpBonus }}%</span>
              </div>
            </div>

            <div class="upgrade-actions">
              <div class="modal-materials" v-if="arrayLevel < maxArrayLevel">
                <div class="upgrade-material upgrade-currency" :class="{ enough: playerGold >= arrayUpgradeCost.goldCost }">
                  <span class="upgrade-material-kind">货币</span>
                  <span class="upgrade-material-name">金币</span>
                  <span class="upgrade-material-count">{{ formatNumber(playerGold) }} / {{ formatNumber(arrayUpgradeCost.goldCost) }}</span>
                </div>
                <div class="upgrade-material upgrade-currency" :class="{ enough: playerSpiritStone >= arrayUpgradeCost.spiritStoneCost }">
                  <span class="upgrade-material-kind">货币</span>
                  <span class="upgrade-material-name">灵石</span>
                  <span class="upgrade-material-count">{{ formatNumber(playerSpiritStone) }} / {{ formatNumber(arrayUpgradeCost.spiritStoneCost) }}</span>
                </div>
                <div
                  v-for="material in arrayUpgradeCost.materials"
                  :key="material.itemId"
                  class="upgrade-material"
                  :class="{ enough: getInventoryCount(material.itemId) >= material.count }"
                >
                  <span class="upgrade-material-kind">材料</span>
                  <span class="upgrade-material-name">{{ material.itemName }}</span>
                  <span class="upgrade-material-count">{{ getInventoryCount(material.itemId) }} / {{ material.count }}</span>
                </div>
              </div>
              <button
                v-if="arrayLevel < maxArrayLevel"
                class="confirm-upgrade-btn"
                :class="{ 'can-upgrade': canUpgradeArray }"
                :disabled="!canUpgradeArray"
                @click="confirmArrayUpgrade"
              >
                确认升级
              </button>
              <button v-else class="confirm-upgrade-btn max">已达满级</button>
            </div>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, watch, nextTick } from 'vue'
import { apiClient } from '../lib/apiClient'
import { useGameStore } from '../state/gameStore'
import { ICON } from '../icons'
import AssetIcon from './common/AssetIcon.vue'
import toast from '@/utils/toast'

const props = defineProps({
  inModal: {
    type: Boolean,
    default: false
  }
})

const gameStore = useGameStore()

// ==================== 数据定义 ====================

// 阵法等级
const arrayLevel = ref(1)
const maxArrayLevel = 50
// 五行等级上限由后端聚灵阵等级控制：聚灵阵等级 × 10，且全局不超过 500。
// 这里仅用于进度条兜底展示，实际 maxLevel 以 ElementBonuses 的后端返回值为准。
const maxElementLevel = computed(() => Math.min(500, Math.max(1, arrayLevel.value) * 10))
const arrayTier = computed(() => Math.ceil(arrayLevel.value / 10))

// 阵法品级名称
const arrayTierName = computed(() => {
  const names = ['凡阶', '灵阶', '玄阶', '地阶', '天阶', '仙阶']
  return names[Math.min(arrayTier.value - 1, 5)]
})

// 五行元素配置 - 使用CSS变量
const elements = ref({
  metal: {
    name: '金',
    icon: ICON.misc_sword_crossed,
    cssVar: '--element-metal',
    cssGlow: '--element-metal-glow',
    level: 1,
    maxLevel: 500, // 后端加载前的显示兜底，接口返回后会覆盖为真实上限。
    bonus: 0, // 属性加成必须来自后端 ElementBonuses，不能在前端猜测。
    angle: -90 // 顶部
  },
  wood: {
    name: '木',
    icon: ICON.misc_herb,
    cssVar: '--element-wood',
    cssGlow: '--element-wood-glow',
    level: 1,
    maxLevel: 500, // 后端加载前的显示兜底，接口返回后会覆盖为真实上限。
    bonus: 0, // 属性加成必须来自后端 ElementBonuses，不能在前端猜测。
    angle: -18 // 右上
  },
  water: {
    name: '水',
    icon: ICON.misc_water_drop,
    cssVar: '--element-water',
    cssGlow: '--element-water-glow',
    level: 1,
    maxLevel: 500, // 后端加载前的显示兜底，接口返回后会覆盖为真实上限。
    bonus: 0, // 属性加成必须来自后端 ElementBonuses，不能在前端猜测。
    angle: 54 // 右下
  },
  fire: {
    name: '火',
    icon: ICON.misc_fire,
    cssVar: '--element-fire',
    cssGlow: '--element-fire-glow',
    level: 1,
    maxLevel: 500, // 后端加载前的显示兜底，接口返回后会覆盖为真实上限。
    bonus: 0, // 属性加成必须来自后端 ElementBonuses，不能在前端猜测。
    angle: 126 // 左下
  },
  earth: {
    name: '土',
    icon: ICON.element_mountain,
    cssVar: '--element-earth',
    cssGlow: '--element-earth-glow',
    level: 1,
    maxLevel: 500, // 后端加载前的显示兜底，接口返回后会覆盖为真实上限。
    bonus: 0, // 属性加成必须来自后端 ElementBonuses，不能在前端猜测。
    angle: 198 // 左上
  }
})

const arrayUpgradeCost = ref({ elementType: 'array', goldCost: 0, spiritStoneCost: 0, materials: [] })
const elementUpgradeCosts = ref({})
const spiritFieldYieldBonus = ref(0)
const battleExpBonus = ref(0)
const professionLevelCap = ref(1)
const elementBonuses = ref({})

// 弹窗状态
const showUpgradeModal = ref(false)
const showArrayUpgradeModal = ref(false)
const selectedElementKey = ref(null)

// Canvas 相关
const canvasRef = ref(null)
const diagramRef = ref(null)
let canvasCtx = null
let animationFrame = null
let particles = []

// 常驻面板不启用 Canvas 动画，升级交互仍保留。
const enableVisualEffects = false

// 当前查看的阵眼或五行元素，只影响面板内详情展示。
const selectedDisplayKey = ref('array')
const hoveredElementKey = ref(null)

const selectedDisplay = computed(() => {
  if (selectedDisplayKey.value === 'array') {
    const isMaxLevel = arrayLevel.value >= maxArrayLevel
    const upgradeState = isMaxLevel ? 'max' : (hasEnoughUpgradeCost(arrayUpgradeCost.value) ? 'ready' : 'insufficient')
    return {
      title: '阵眼总览',
      level: arrayLevel.value,
      maxLevel: maxArrayLevel,
      description: '聚灵阵统御五行节点，为修炼与技艺提供总加成。',
      nextLabel: '',
      nextValue: '',
      upgradeState,
      upgradeLabel: isMaxLevel ? '已满级' : (upgradeState === 'ready' ? '升级' : '查看材料')
    }
  }

  const element = elements.value[selectedDisplayKey.value]
  const bonus = elementBonuses.value[selectedDisplayKey.value]
  const isMaxLevel = element.level >= element.maxLevel
  const upgradeState = isMaxLevel ? 'max' : (hasEnoughUpgradeCost(elementUpgradeCosts.value[selectedDisplayKey.value]) ? 'ready' : 'insufficient')
  return {
    title: `${element.name}行节点`,
    level: element.level,
    maxLevel: element.maxLevel,
    description: `提升${element.name}行节点等级，等级上限由聚灵阵等级控制。`,
    attributeName: bonus?.attributeName || '',
    currentBonus: bonus?.currentBonus || 0,
    bonusPerLevel: bonus?.bonusPerLevel || 0,
    nextLabel: '下一等级',
    nextValue: isMaxLevel ? '已满级' : `Lv.${element.level + 1}`,
    upgradeState,
    upgradeLabel: isMaxLevel ? '已满级' : (upgradeState === 'ready' ? '升级' : '查看材料')
  }
})

// ==================== 计算属性 ====================

// 当前选择的五行节点，用于弹窗和 tooltip 展示后端真实属性数据。
const selectedBonus = computed(() => selectedElementKey.value ? elementBonuses.value[selectedElementKey.value] : null)

// 选中元素数据
const selectedElementData = computed(() => {
  if (!selectedElementKey.value) return null
  return elements.value[selectedElementKey.value]
})

const selectedElementCost = computed(() => {
  if (!selectedElementKey.value) {
    return { goldCost: 0, spiritStoneCost: 0, materials: [] }
  }

  return elementUpgradeCosts.value[selectedElementKey.value] || { goldCost: 0, spiritStoneCost: 0, materials: [] }
})

const playerGold = computed(() => gameStore.state.player?.gold || 0)
const playerSpiritStone = computed(() => gameStore.state.player?.spiritStone || 0)
const inventoryItems = computed(() => gameStore.state.inventory || [])

// 是否可以升级阵法
const canUpgradeArray = computed(() => {
  return arrayLevel.value < maxArrayLevel && hasEnoughUpgradeCost(arrayUpgradeCost.value)
})

// 效果列表
const effectsList = computed(() => {
  return [
    {
      icon: ICON.misc_seedling,
      name: '灵田增产',
      description: '聚灵阵提升灵田产量',
      value: spiritFieldYieldBonus.value,
      unit: '%',
      type: 'wood'
    },
    {
      icon: ICON.misc_swords,
      name: '战斗经验',
      description: '战斗胜利额外经验',
      value: battleExpBonus.value,
      unit: '%',
      type: 'fire'
    },
    {
      icon: ICON.nav_alchemy,
      name: '炼丹上限',
      description: '炼丹师等级上限',
      value: professionLevelCap.value,
      unit: '级',
      type: 'water'
    },
    {
      icon: ICON.nav_forge,
      name: '锻造上限',
      description: '锻造师等级上限',
      value: professionLevelCap.value,
      unit: '级',
      type: 'metal'
    }
  ]
})

/**
 * 中文注释：
 * 把后端五行聚灵阵数据同步到当前界面状态。
 * 模板层继续复用你原来的阵图节点结构，只把等级、升级成本和加成替换成真实接口返回值。
 */
function applyFiveElementInfo(data) {
  const source = data || {}
  arrayLevel.value = Number(source.arrayLevel ?? source.ArrayLevel ?? 1) || 1
  arrayUpgradeCost.value = normalizeUpgradeCost(source.arrayUpgradeCost ?? source.ArrayUpgradeCost, 'array')
  elementUpgradeCosts.value = normalizeElementUpgradeCosts(source.elementUpgradeCosts ?? source.ElementUpgradeCosts)
  spiritFieldYieldBonus.value = Number(source.spiritFieldYieldBonusPercent ?? source.SpiritFieldYieldBonusPercent ?? 0) || 0
  battleExpBonus.value = Number(source.battleExpBonusPercent ?? source.BattleExpBonusPercent ?? 0) || 0
  professionLevelCap.value = Number(source.professionLevelCap ?? source.ProfessionLevelCap ?? arrayLevel.value) || arrayLevel.value
  const bonusList = source.elementBonuses ?? source.ElementBonuses ?? []
  elementBonuses.value = Object.fromEntries((Array.isArray(bonusList) ? bonusList : []).map((item) => {
    // 五行等级、上限和属性加成全部使用后端返回值；前端不重新推导可配置数值。
    const normalized = {
      elementType: item.elementType ?? item.ElementType ?? '',
      elementName: item.elementName ?? item.ElementName ?? '',
      level: Number(item.level ?? item.Level ?? 1) || 1,
      maxLevel: Number(item.maxLevel ?? item.MaxLevel ?? maxElementLevel.value) || maxElementLevel.value,
      attributeType: item.attributeType ?? item.AttributeType ?? '',
      attributeName: item.attributeName ?? item.AttributeName ?? '',
      currentBonus: Number(item.currentBonus ?? item.CurrentBonus ?? 0) || 0,
      bonusPerLevel: Number(item.bonusPerLevel ?? item.BonusPerLevel ?? 0) || 0
    }
    return [normalized.elementType, normalized]
  }))

  const levelMap = {
    metal: Number(source.metalLevel ?? source.MetalLevel ?? 1) || 1,
    wood: Number(source.woodLevel ?? source.WoodLevel ?? 1) || 1,
    water: Number(source.waterLevel ?? source.WaterLevel ?? 1) || 1,
    fire: Number(source.fireLevel ?? source.FireLevel ?? 1) || 1,
    earth: Number(source.earthLevel ?? source.EarthLevel ?? 1) || 1
  }

  Object.keys(levelMap).forEach((key) => {
    const level = levelMap[key]
    elements.value[key].level = level
    const bonus = elementBonuses.value[key]
    elements.value[key].maxLevel = bonus?.maxLevel || maxElementLevel.value
    elements.value[key].bonus = bonus?.currentBonus || 0
  })
}

async function loadFiveElementInfo() {
  const [data] = await Promise.all([
    apiClient.getFiveElementInfo(),
    gameStore.loadPlayer(true),
    gameStore.loadInventory(true)
  ])
  applyFiveElementInfo(data)
}

function normalizeUpgradeCost(cost, fallbackElementType) {
  const source = cost || {}
  return {
    elementType: source.elementType || source.ElementType || fallbackElementType,
    goldCost: Number(source.goldCost ?? source.GoldCost ?? 0) || 0,
    spiritStoneCost: Number(source.spiritStoneCost ?? source.SpiritStoneCost ?? 0) || 0,
    materials: Array.isArray(source.materials || source.Materials)
      ? (source.materials || source.Materials).map((item) => ({
        itemId: item.itemId || item.ItemId,
        itemName: item.itemName || item.ItemName,
        count: Number(item.count ?? item.Count ?? 0) || 0
      }))
      : []
  }
}

function normalizeElementUpgradeCosts(rawCosts) {
  const result = {}
  ;(Array.isArray(rawCosts) ? rawCosts : []).forEach((entry) => {
    const cost = normalizeUpgradeCost(entry, entry?.elementType || entry?.ElementType || '')
    if (cost.elementType) {
      result[cost.elementType] = cost
    }
  })
  return result
}

// ==================== 方法 ====================

// 获取CSS变量值
function getCssVar(varName, fallback = '') {
  if (typeof window === 'undefined') return fallback
  return getComputedStyle(document.documentElement).getPropertyValue(varName.replace('--', '--')).trim() || fallback
}

// 获取元素位置（五角星位置）
function getElementPosition(element) {
  const radius = 35 // 距离中心半径百分比
  const angle = (element.angle * Math.PI) / 180
  const x = 50 + radius * Math.cos(angle)
  const y = 50 + radius * Math.sin(angle) * 0.9
  return {
    left: x + '%',
    top: y + '%'
  }
}

// 获取元素光环样式
function getElementAuraStyle(key) {
  const el = elements.value[key]
  return {
    '--glow-color': `var(${el.cssGlow})`
  }
}

// 获取元素圆环样式
function getElementRingStyle(key) {
  const el = elements.value[key]
  return {
    borderColor: `var(${el.cssVar})`
  }
}

// 获取元素内环样式
function getElementInnerStyle(key) {
  const el = elements.value[key]
  return {
    backgroundColor: `var(${el.cssVar})`
  }
}

// 获取下级加成
function getNextBonus(key) {
  const el = elements.value[key]
  const bonus = elementBonuses.value[key]
  if (!bonus || el.level >= el.maxLevel) return bonus?.currentBonus || 0
  return (bonus.currentBonus || 0) + (bonus.bonusPerLevel || 0)
}

function getInventoryCount(itemId) {
  return inventoryItems.value
    .filter((item) => item.itemId === itemId)
    .reduce((sum, item) => sum + (item.quantity || 0), 0)
}

function hasEnoughUpgradeCost(cost) {
  if (!cost) {
    return false
  }

  if (playerGold.value < (cost.goldCost || 0)) {
    return false
  }

  if (playerSpiritStone.value < (cost.spiritStoneCost || 0)) {
    return false
  }

  return (cost.materials || []).every((material) => getInventoryCount(material.itemId) >= material.count)
}

// 是否可以升级元素
function canUpgradeElement(key) {
  const el = elements.value[key]
  // 材料不足时仍允许打开弹窗查看材料；真正升级按钮仍由资源校验控制。
  return el.level < el.maxLevel && hasEnoughUpgradeCost(elementUpgradeCosts.value[key])
}

async function upgradeArray() {
  if (!canUpgradeArray.value) return

  try {
    await apiClient.upgradeFiveElementArray()
    await loadFiveElementInfo()
    addUpgradeEffect()
  } catch (error) {
    toast.error(error.message || '升级聚灵阵失败。')
  }
}

function openArrayUpgradeModal() {
  showArrayUpgradeModal.value = true
}

function closeArrayUpgradeModal() {
  showArrayUpgradeModal.value = false
}

function selectElement(key) {
  selectedDisplayKey.value = key
}

function openSelectedUpgrade() {
  if (selectedDisplayKey.value === 'array') {
    openArrayUpgradeModal()
    return
  }

  openElementUpgrade(selectedDisplayKey.value)
}

async function confirmArrayUpgrade() {
  if (!canUpgradeArray.value) {
    return
  }

  await upgradeArray()
  closeArrayUpgradeModal()
}

// 打开元素升级弹窗
function openElementUpgrade(key) {
  selectedElementKey.value = key
  showUpgradeModal.value = true
}

// 关闭升级弹窗
function closeUpgradeModal() {
  showUpgradeModal.value = false
  selectedElementKey.value = null
}

async function upgradeElement(key) {
  if (!canUpgradeElement(key)) return

  try {
    await apiClient.upgradeFiveElement(key)
    await loadFiveElementInfo()
    addElementUpgradeEffect(key)
  } catch (error) {
    toast.error(error.message || '升级五行元素失败。')
  }
}

// 格式化数字
function formatNumber(num) {
  if (num >= 100000000) {
    return (num / 100000000).toFixed(2) + '亿'
  }
  if (num >= 10000) {
    return (num / 10000).toFixed(2) + '万'
  }
  return Math.floor(num).toLocaleString()
}

// ==================== Canvas 动画 ====================

// 获取当前主题颜色
function getThemeColors() {
  const root = document.documentElement
  const style = getComputedStyle(root)

  return {
    jade: style.getPropertyValue('--jade-primary').trim() || '#2d9e8f',
    gold: style.getPropertyValue('--gold-primary').trim() || '#d4a853',
    metal: style.getPropertyValue('--element-metal').trim() || '#E8E8E8',
    wood: style.getPropertyValue('--element-wood').trim() || '#2E8B57',
    water: style.getPropertyValue('--element-water').trim() || '#00BFFF',
    fire: style.getPropertyValue('--element-fire').trim() || '#FF6347',
    earth: style.getPropertyValue('--element-earth').trim() || '#CD853F'
  }
}

// 初始化 Canvas
function initCanvas() {
  const canvas = canvasRef.value
  const container = diagramRef.value
  if (!canvas || !container) return

  const rect = container.getBoundingClientRect()
  canvas.width = rect.width * window.devicePixelRatio
  canvas.height = rect.height * window.devicePixelRatio
  canvas.style.width = rect.width + 'px'
  canvas.style.height = rect.height + 'px'

  canvasCtx = canvas.getContext('2d')
  canvasCtx.scale(window.devicePixelRatio, window.devicePixelRatio)

  initParticles()
}

// 初始化粒子
function initParticles() {
  const colors = getThemeColors()
  particles = []

  // 少量飘浮粒子（营造氛围）
  for (let i = 0; i < 12; i++) {
    particles.push({
      x: Math.random() * 100,
      y: Math.random() * 100,
      size: Math.random() * 1.5 + 0.8,
      speedX: (Math.random() - 0.5) * 0.15,
      speedY: (Math.random() - 0.5) * 0.15,
      color: [colors.metal, colors.wood, colors.water, colors.fire, colors.earth][Math.floor(Math.random() * 5)],
      alpha: Math.random() * 0.3 + 0.2,
      pulse: Math.random() * Math.PI * 2,
      type: 'float'
    })
  }

  // 螺旋运动粒子（减少数量）
  for (let i = 0; i < 8; i++) {
    particles.push({
      angle: (i / 8) * Math.PI * 2,
      radius: 25 + Math.random() * 15,
      speed: 0.015 + Math.random() * 0.015,
      size: Math.random() * 1.2 + 0.8,
      color: colors.gold,
      alpha: Math.random() * 0.3 + 0.4,
      pulse: Math.random() * Math.PI * 2,
      type: 'spiral'
    })
  }

  // 向中心流动的粒子（减少数量）
  for (let i = 0; i < 6; i++) {
    particles.push({
      angle: (i / 6) * Math.PI * 2 + Math.random() * 0.5,
      radius: 30 + Math.random() * 8,
      speed: 0.08 + Math.random() * 0.06,
      size: Math.random() * 1.2 + 0.6,
      color: colors.jade,
      alpha: Math.random() * 0.3 + 0.3,
      pulse: Math.random() * Math.PI * 2,
      type: 'flow'
    })
  }
}

// 动画循环
function animate() {
  if (!canvasCtx || !canvasRef.value) return

  const width = canvasRef.value.width / window.devicePixelRatio
  const height = canvasRef.value.height / window.devicePixelRatio
  const centerX = width / 2
  const centerY = height / 2
  const colors = getThemeColors()
  const maxRadius = Math.min(width, height) * 0.45

  // 清空画布
  canvasCtx.clearRect(0, 0, width, height)

  const time = Date.now() / 1000

  // 1. 绘制最外层装饰圈
  drawOuterDecorations(centerX, centerY, maxRadius, time, colors)

  // 2. 绘制多层旋转阵纹圆环
  drawArrayRings(centerX, centerY, maxRadius, time, colors)

  // 3. 绘制八卦方位阵纹
  drawBaguaPattern(centerX, centerY, maxRadius * 0.72, time, colors)

  // 4. 绘制星芒效果
  drawStarBurst(centerX, centerY, maxRadius * 0.55, time, colors)

  // 5. 绘制符文环
  drawRuneCircles(centerX, centerY, maxRadius * 0.62, time, colors)

  // 6. 绘制五行连线
  drawElementConnections(centerX, centerY, maxRadius * 0.78, colors)

  // 7. 绘制能量流动线条
  drawEnergyFlows(centerX, centerY, maxRadius * 0.5, time, colors)

  // 8. 绘制能量粒子
  drawParticles(width, height, time)

  // 9. 绘制中心能量漩涡
  drawEnergyVortex(centerX, centerY, maxRadius * 0.33, time, colors)

  // 10. 绘制中心阵纹
  drawCenterPattern(centerX, centerY, maxRadius * 0.2, time, colors)

  animationFrame = requestAnimationFrame(animate)
}

// 绘制外层装饰
function drawOuterDecorations(cx, cy, radius, time, colors) {
  // 最外层虚线圆环
  canvasCtx.save()
  canvasCtx.translate(cx, cy)
  canvasCtx.rotate(time * 0.1)

  canvasCtx.beginPath()
  canvasCtx.arc(0, 0, radius * 0.98, 0, Math.PI * 2)
  canvasCtx.strokeStyle = hexToRgba(colors.gold, 0.15)
  canvasCtx.lineWidth = 1
  canvasCtx.setLineDash([3, 3])
  canvasCtx.stroke()
  canvasCtx.restore()

  // 四角星装饰
  canvasCtx.save()
  canvasCtx.translate(cx, cy)
  for (let i = 0; i < 4; i++) {
    canvasCtx.rotate(Math.PI / 2)
    canvasCtx.beginPath()
    canvasCtx.moveTo(0, -radius * 0.95)
    canvasCtx.lineTo(3, -radius * 0.90)
    canvasCtx.lineTo(0, -radius * 0.85)
    canvasCtx.lineTo(-3, -radius * 0.90)
    canvasCtx.closePath()
    canvasCtx.fillStyle = hexToRgba(colors.jade, 0.4 + Math.sin(time * 3 + i) * 0.2)
    canvasCtx.fill()
  }
  canvasCtx.restore()
}

// 绘制阵法圆环 - 增强版
function drawArrayRings(cx, cy, radius, time, colors) {
  const rings = [
    { r: 0.95, width: 1, alpha: 0.2, speed: 0.15, dash: [5, 5] },
    { r: 0.88, width: 2, alpha: 0.25, speed: -0.25, dash: [15, 5, 5, 5] },
    { r: 0.80, width: 1.5, alpha: 0.3, speed: 0.35, dash: [10, 3, 3, 3, 3, 3] },
    { r: 0.72, width: 1, alpha: 0.2, speed: -0.2, dash: [8, 8] },
    { r: 0.65, width: 2, alpha: 0.25, speed: 0.4, dash: [20, 5] },
    { r: 0.58, width: 1, alpha: 0.15, speed: -0.3, dash: [4, 4, 4, 4] },
    { r: 0.52, width: 1.5, alpha: 0.2, speed: 0.5, dash: [12, 4] },
    { r: 0.45, width: 1, alpha: 0.18, speed: -0.45, dash: [6, 6] }
  ]

  rings.forEach((ring, i) => {
    canvasCtx.save()
    canvasCtx.translate(cx, cy)
    canvasCtx.rotate(time * ring.speed)

    // 绘制圆环
    canvasCtx.beginPath()
    canvasCtx.arc(0, 0, radius * ring.r, 0, Math.PI * 2)
    canvasCtx.strokeStyle = hexToRgba(colors.jade, ring.alpha)
    canvasCtx.lineWidth = ring.width
    canvasCtx.setLineDash(ring.dash)
    canvasCtx.stroke()

    // 绘制流动的符文光点（沿着圆环移动）
    const runeCount = 3 + Math.floor(i / 2)
    for (let j = 0; j < runeCount; j++) {
      // 计算移动位置 - 光点沿着圆环移动
      const baseAngle = (j / runeCount) * Math.PI * 2
      const moveOffset = time * (0.5 + i * 0.1) // 移动速度
      const angle = baseAngle + moveOffset

      const x = Math.cos(angle) * radius * ring.r
      const y = Math.sin(angle) * radius * ring.r

      // 闪烁效果
      const breathe = Math.sin(time * 3 + j + i * 0.5) * 0.5 + 0.5
      const size = (2 + i * 0.3) * (0.5 + breathe * 0.5)

      // 外圈光晕（渐隐）
      const outerGradient = canvasCtx.createRadialGradient(x, y, 0, x, y, size * 4)
      outerGradient.addColorStop(0, hexToRgba(colors.gold, 0.6 * breathe))
      outerGradient.addColorStop(0.5, hexToRgba(colors.gold, 0.2 * breathe))
      outerGradient.addColorStop(1, hexToRgba(colors.gold, 0))
      canvasCtx.fillStyle = outerGradient
      canvasCtx.beginPath()
      canvasCtx.arc(x, y, size * 4, 0, Math.PI * 2)
      canvasCtx.fill()

      // 内圈光晕
      const innerGradient = canvasCtx.createRadialGradient(x, y, 0, x, y, size * 2)
      innerGradient.addColorStop(0, hexToRgba(colors.gold, 0.9 * breathe))
      innerGradient.addColorStop(1, hexToRgba(colors.gold, 0))
      canvasCtx.fillStyle = innerGradient
      canvasCtx.beginPath()
      canvasCtx.arc(x, y, size * 2, 0, Math.PI * 2)
      canvasCtx.fill()

      // 核心亮点
      canvasCtx.beginPath()
      canvasCtx.arc(x, y, size * 0.6, 0, Math.PI * 2)
      canvasCtx.fillStyle = hexToRgba('#ffffff', 0.8 + breathe * 0.2)
      canvasCtx.fill()

      // 绘制拖尾效果
      const trailLength = 0.3
      for (let t = 1; t <= 3; t++) {
        const trailAngle = angle - t * 0.1
        const trailX = Math.cos(trailAngle) * radius * ring.r
        const trailY = Math.sin(trailAngle) * radius * ring.r
        const trailAlpha = (0.3 / t) * breathe
        const trailSize = size * (1 - t * 0.2)

        canvasCtx.beginPath()
        canvasCtx.arc(trailX, trailY, trailSize, 0, Math.PI * 2)
        canvasCtx.fillStyle = hexToRgba(colors.gold, trailAlpha)
        canvasCtx.fill()
      }
    }

    canvasCtx.restore()
  })
}

// 绘制八卦方位阵纹
function drawBaguaPattern(cx, cy, radius, time, colors) {
  const trigrams = ['☰', '☱', '☲', '☳', '☴', '☵', '☶', '☷']
  const baguaChars = ['乾', '兑', '离', '震', '巽', '坎', '艮', '坤']

  canvasCtx.save()
  canvasCtx.translate(cx, cy)
  canvasCtx.rotate(time * 0.15)

  // 八卦方位线
  for (let i = 0; i < 8; i++) {
    const angle = (i / 8) * Math.PI * 2
    const innerR = radius * 0.7
    const outerR = radius * 0.95

    canvasCtx.beginPath()
    canvasCtx.moveTo(Math.cos(angle) * innerR, Math.sin(angle) * innerR)
    canvasCtx.lineTo(Math.cos(angle) * outerR, Math.sin(angle) * outerR)
    canvasCtx.strokeStyle = hexToRgba(colors.jade, 0.2)
    canvasCtx.lineWidth = 1
    canvasCtx.stroke()

    // 方位标记
    const textR = radius * 1.08
    const tx = Math.cos(angle) * textR
    const ty = Math.sin(angle) * textR

    canvasCtx.fillStyle = hexToRgba(colors.gold, 0.5 + Math.sin(time * 2 + i) * 0.2)
    canvasCtx.font = '10px serif'
    canvasCtx.textAlign = 'center'
    canvasCtx.textBaseline = 'middle'
    canvasCtx.fillText(baguaChars[i], tx, ty)
  }

  // 内圈八卦符号 - 呼吸效果
  for (let i = 0; i < 8; i++) {
    const angle = (i / 8) * Math.PI * 2 + Math.PI / 8 + Math.sin(time * 0.5 + i) * 0.05
    const symbolR = radius * (0.82 + Math.sin(time * 2 + i) * 0.03)
    const sx = Math.cos(angle) * symbolR
    const sy = Math.sin(angle) * symbolR

    const breathe = Math.sin(time * 2 + i) * 0.3 + 0.7
    canvasCtx.fillStyle = hexToRgba(colors.jade, 0.2 + breathe * 0.3)
    canvasCtx.font = `bold ${8 + breathe * 2}px serif`
    canvasCtx.textAlign = 'center'
    canvasCtx.textBaseline = 'middle'
    canvasCtx.fillText(trigrams[i], sx, sy)
  }

  canvasCtx.restore()
}

// 绘制星芒效果
function drawStarBurst(cx, cy, radius, time, colors) {
  canvasCtx.save()
  canvasCtx.translate(cx, cy)

  // 主星芒 - 8角
  for (let i = 0; i < 8; i++) {
    canvasCtx.rotate(Math.PI / 4)

    const gradient = canvasCtx.createLinearGradient(0, 0, 0, -radius)
    gradient.addColorStop(0, hexToRgba(colors.gold, 0))
    gradient.addColorStop(0.5, hexToRgba(colors.gold, 0.1 + Math.sin(time * 2 + i) * 0.05))
    gradient.addColorStop(1, hexToRgba(colors.gold, 0))

    canvasCtx.beginPath()
    canvasCtx.moveTo(-2, 0)
    canvasCtx.lineTo(0, -radius)
    canvasCtx.lineTo(2, 0)
    canvasCtx.closePath()
    canvasCtx.fillStyle = gradient
    canvasCtx.fill()
  }

  // 次星芒 - 16角（错位）
  canvasCtx.rotate(Math.PI / 8)
  for (let i = 0; i < 8; i++) {
    canvasCtx.rotate(Math.PI / 4)

    const gradient = canvasCtx.createLinearGradient(0, 0, 0, -radius * 0.7)
    gradient.addColorStop(0, hexToRgba(colors.jade, 0))
    gradient.addColorStop(0.5, hexToRgba(colors.jade, 0.08 + Math.sin(time * 3 + i) * 0.04))
    gradient.addColorStop(1, hexToRgba(colors.jade, 0))

    canvasCtx.beginPath()
    canvasCtx.moveTo(-1, 0)
    canvasCtx.lineTo(0, -radius * 0.7)
    canvasCtx.lineTo(1, 0)
    canvasCtx.closePath()
    canvasCtx.fillStyle = gradient
    canvasCtx.fill()
  }

  canvasCtx.restore()
}

// 绘制符文环
function drawRuneCircles(cx, cy, radius, time, colors) {
  const runes = ['☰', '☱', '☲', '☳', '☴', '☵', '☶', '☷', '⚊', '⚋', '⚌', '⚍']

  // 外圈符文 - 浮动发光效果
  canvasCtx.save()
  canvasCtx.translate(cx, cy)
  canvasCtx.rotate(time * 0.15)

  for (let i = 0; i < 8; i++) {
    const angle = (i / 8) * Math.PI * 2
    // 添加浮动偏移
    const floatR = radius + Math.sin(time * 2 + i) * 3
    const x = Math.cos(angle) * floatR
    const y = Math.sin(angle) * floatR

    const breathe = Math.sin(time * 2.5 + i) * 0.4 + 0.6

    // 符文光晕（大）
    const gradient = canvasCtx.createRadialGradient(x, y, 0, x, y, 12 * breathe)
    gradient.addColorStop(0, hexToRgba(colors.gold, 0.4 * breathe))
    gradient.addColorStop(0.5, hexToRgba(colors.gold, 0.15 * breathe))
    gradient.addColorStop(1, hexToRgba(colors.gold, 0))
    canvasCtx.fillStyle = gradient
    canvasCtx.beginPath()
    canvasCtx.arc(x, y, 12 * breathe, 0, Math.PI * 2)
    canvasCtx.fill()

    // 符文文字
    canvasCtx.fillStyle = hexToRgba(colors.gold, 0.6 + breathe * 0.4)
    canvasCtx.font = `bold ${10 + breathe * 4}px serif`
    canvasCtx.textAlign = 'center'
    canvasCtx.textBaseline = 'middle'
    canvasCtx.fillText(runes[i % runes.length], x, y)
  }
  canvasCtx.restore()

  // 内圈反向旋转符文 - 脉冲效果
  canvasCtx.save()
  canvasCtx.translate(cx, cy)
  canvasCtx.rotate(-time * 0.2)

  for (let i = 0; i < 6; i++) {
    const angle = (i / 6) * Math.PI * 2
    // 脉冲缩放
    const pulse = Math.sin(time * 3 + i) * 0.3 + 0.7
    const r = radius * 0.7 * (0.95 + pulse * 0.1)
    const x = Math.cos(angle) * r
    const y = Math.sin(angle) * r

    // 光晕效果
    const glow = canvasCtx.createRadialGradient(x, y, 0, x, y, 10 * pulse)
    glow.addColorStop(0, hexToRgba(colors.jade, 0.5 * pulse))
    glow.addColorStop(1, hexToRgba(colors.jade, 0))
    canvasCtx.fillStyle = glow
    canvasCtx.beginPath()
    canvasCtx.arc(x, y, 10 * pulse, 0, Math.PI * 2)
    canvasCtx.fill()

    canvasCtx.fillStyle = hexToRgba(colors.jade, 0.4 + pulse * 0.4)
    canvasCtx.font = `bold ${8 + pulse * 4}px serif`
    canvasCtx.textAlign = 'center'
    canvasCtx.textBaseline = 'middle'
    canvasCtx.fillText(['✦', '✧', '✪', '✯', '✰', '✵'][i], x, y)
  }
  canvasCtx.restore()
}

// 绘制能量流动线条
function drawEnergyFlows(cx, cy, radius, time, colors) {
  canvasCtx.save()
  canvasCtx.translate(cx, cy)

  // 螺旋能量线
  for (let i = 0; i < 6; i++) {
    canvasCtx.save()
    canvasCtx.rotate((i / 6) * Math.PI * 2 + time * 0.3)

    const gradient = canvasCtx.createLinearGradient(0, 0, radius, 0)
    gradient.addColorStop(0, hexToRgba(colors.gold, 0))
    gradient.addColorStop(0.3, hexToRgba(colors.gold, 0.15))
    gradient.addColorStop(0.7, hexToRgba(colors.jade, 0.1))
    gradient.addColorStop(1, hexToRgba(colors.gold, 0))

    canvasCtx.strokeStyle = gradient
    canvasCtx.lineWidth = 1
    canvasCtx.setLineDash([5, 10])
    canvasCtx.lineDashOffset = -time * 20

    canvasCtx.beginPath()
    canvasCtx.moveTo(0, 0)
    canvasCtx.quadraticCurveTo(radius * 0.5, -radius * 0.1, radius, 0)
    canvasCtx.stroke()

    canvasCtx.restore()
  }

  // 圆形波纹
  const waveCount = 3
  for (let i = 0; i < waveCount; i++) {
    const waveRadius = ((time * 0.3 + i / waveCount) % 1) * radius * 0.8
    const alpha = 0.3 * (1 - waveRadius / (radius * 0.8))

    canvasCtx.beginPath()
    canvasCtx.arc(0, 0, waveRadius, 0, Math.PI * 2)
    canvasCtx.strokeStyle = hexToRgba(colors.jade, alpha)
    canvasCtx.lineWidth = 1
    canvasCtx.setLineDash([2, 4])
    canvasCtx.stroke()
  }

  canvasCtx.restore()
}

// 绘制五行连线 - 增强版
function drawElementConnections(cx, cy, radius, colors) {
  const elementList = Object.values(elements.value)
  const elementColors = [colors.metal, colors.wood, colors.water, colors.fire, colors.earth]

  const positions = elementList.map((el, idx) => {
    const angle = (el.angle * Math.PI) / 180
    return {
      x: cx + radius * Math.cos(angle),
      y: cy + radius * Math.sin(angle) * 0.9,
      color: elementColors[idx]
    }
  })

  canvasCtx.save()

  // 绘制外层五边形连线（相邻元素）
  for (let i = 0; i < positions.length; i++) {
    const start = positions[i]
    const end = positions[(i + 1) % positions.length]

    const gradient = canvasCtx.createLinearGradient(start.x, start.y, end.x, end.y)
    gradient.addColorStop(0, hexToRgba(start.color, 0.15))
    gradient.addColorStop(0.5, hexToRgba(colors.jade, 0.1))
    gradient.addColorStop(1, hexToRgba(end.color, 0.15))

    canvasCtx.beginPath()
    canvasCtx.moveTo(start.x, start.y)
    canvasCtx.lineTo(end.x, end.y)
    canvasCtx.strokeStyle = gradient
    canvasCtx.lineWidth = 1
    canvasCtx.setLineDash([5, 5])
    canvasCtx.stroke()
  }

  // 绘制内层五角星连线（五行相生）
  for (let i = 0; i < positions.length; i++) {
    const start = positions[i]
    const end = positions[(i + 2) % positions.length]

    const gradient = canvasCtx.createLinearGradient(start.x, start.y, end.x, end.y)
    gradient.addColorStop(0, hexToRgba(start.color, 0.35))
    gradient.addColorStop(0.5, hexToRgba(colors.gold, 0.25))
    gradient.addColorStop(1, hexToRgba(end.color, 0.35))

    canvasCtx.beginPath()
    canvasCtx.moveTo(start.x, start.y)
    canvasCtx.lineTo(end.x, end.y)
    canvasCtx.strokeStyle = gradient
    canvasCtx.lineWidth = 2.5
    canvasCtx.setLineDash([])
    canvasCtx.stroke()

    // 流动光点效果
    const flowPos = (Date.now() / 2000 + i * 0.2) % 1
    const fx = start.x + (end.x - start.x) * flowPos
    const fy = start.y + (end.y - start.y) * flowPos

    const glowGradient = canvasCtx.createRadialGradient(fx, fy, 0, fx, fy, 6)
    glowGradient.addColorStop(0, hexToRgba(colors.gold, 1))
    glowGradient.addColorStop(0.5, hexToRgba(start.color, 0.5))
    glowGradient.addColorStop(1, hexToRgba(colors.gold, 0))

    canvasCtx.fillStyle = glowGradient
    canvasCtx.beginPath()
    canvasCtx.arc(fx, fy, 6, 0, Math.PI * 2)
    canvasCtx.fill()
  }

  // 绘制到中心的虚线
  for (let i = 0; i < positions.length; i++) {
    const pos = positions[i]

    canvasCtx.beginPath()
    canvasCtx.moveTo(pos.x, pos.y)
    canvasCtx.lineTo(cx, cy)
    canvasCtx.strokeStyle = hexToRgba(pos.color, 0.1)
    canvasCtx.lineWidth = 1
    canvasCtx.setLineDash([3, 6])
    canvasCtx.stroke()
  }

  canvasCtx.restore()

  // 绘制元素节点光晕
  positions.forEach((pos, i) => {
    const gradient = canvasCtx.createRadialGradient(pos.x, pos.y, 0, pos.x, pos.y, 15)
    gradient.addColorStop(0, hexToRgba(pos.color, 0.6))
    gradient.addColorStop(0.5, hexToRgba(pos.color, 0.2))
    gradient.addColorStop(1, hexToRgba(pos.color, 0))

    canvasCtx.fillStyle = gradient
    canvasCtx.beginPath()
    canvasCtx.arc(pos.x, pos.y, 15, 0, Math.PI * 2)
    canvasCtx.fill()
  })
}

// 绘制能量粒子 - 增强版
function drawParticles(width, height, time) {
  const centerX = width / 2
  const centerY = height / 2

  particles.forEach((p, i) => {
    let x, y, alpha, size

    if (p.type === 'float') {
      // 普通飘浮粒子
      p.x += p.speedX
      p.y += p.speedY
      p.pulse += 0.05

      if (p.x < 0 || p.x > 100) p.speedX *= -1
      if (p.y < 0 || p.y > 100) p.speedY *= -1

      x = (p.x / 100) * width
      y = (p.y / 100) * height
      alpha = Math.max(0, Math.min(1, p.alpha + Math.sin(p.pulse) * 0.1))
      size = p.size + Math.sin(p.pulse) * 0.5
    } else if (p.type === 'spiral') {
      // 螺旋运动粒子
      p.angle += p.speed
      const r = (p.radius / 100) * Math.min(width, height) * 0.5
      x = centerX + Math.cos(p.angle) * r
      y = centerY + Math.sin(p.angle) * r
      alpha = p.alpha + Math.sin(time * 2 + i) * 0.1
      size = p.size
    } else if (p.type === 'flow') {
      // 向中心流动的粒子
      p.radius -= p.speed
      if (p.radius < 5) {
        p.radius = 35 + Math.random() * 10
        p.angle = Math.random() * Math.PI * 2
      }
      const r = (p.radius / 100) * Math.min(width, height) * 0.5
      x = centerX + Math.cos(p.angle) * r
      y = centerY + Math.sin(p.angle) * r
      alpha = p.alpha * (p.radius / 35)
      size = p.size * (p.radius / 35)
    }

    // 绘制粒子光晕
    const gradient = canvasCtx.createRadialGradient(x, y, 0, x, y, size * 3)
    gradient.addColorStop(0, hexToRgba(p.color, alpha))
    gradient.addColorStop(1, hexToRgba(p.color, 0))

    canvasCtx.beginPath()
    canvasCtx.arc(x, y, size * 3, 0, Math.PI * 2)
    canvasCtx.fillStyle = gradient
    canvasCtx.fill()

    // 绘制粒子核心
    canvasCtx.beginPath()
    canvasCtx.arc(x, y, size, 0, Math.PI * 2)
    canvasCtx.fillStyle = p.color
    canvasCtx.fill()

    // 流动粒子绘制轨迹
    if (p.type === 'flow') {
      canvasCtx.beginPath()
      canvasCtx.moveTo(x, y)
      const trailR = ((p.radius + 3) / 100) * Math.min(width, height) * 0.5
      const trailX = centerX + Math.cos(p.angle) * trailR
      const trailY = centerY + Math.sin(p.angle) * trailR
      canvasCtx.lineTo(trailX, trailY)
      canvasCtx.strokeStyle = hexToRgba(p.color, alpha * 0.5)
      canvasCtx.lineWidth = 1
      canvasCtx.stroke()
    }
  })
}

// 绘制能量漩涡
function drawEnergyVortex(cx, cy, radius, time, colors) {
  // 多层螺旋
  const spirals = 5
  const points = 60

  for (let s = 0; s < spirals; s++) {
    canvasCtx.save()
    canvasCtx.translate(cx, cy)
    canvasCtx.rotate(time * (0.4 + s * 0.15) + (s * Math.PI * 2) / spirals)

    canvasCtx.beginPath()
    for (let i = 0; i < points; i++) {
      const t = i / points
      const r = radius * t
      const angle = t * Math.PI * 5
      const x = Math.cos(angle) * r
      const y = Math.sin(angle) * r

      if (i === 0) {
        canvasCtx.moveTo(x, y)
      } else {
        canvasCtx.lineTo(x, y)
      }
    }

    const gradient = canvasCtx.createLinearGradient(0, 0, radius, 0)
    gradient.addColorStop(0, hexToRgba(colors.gold, 0))
    gradient.addColorStop(0.5, hexToRgba(colors.jade, 0.25 - s * 0.03))
    gradient.addColorStop(1, hexToRgba(colors.gold, 0))

    canvasCtx.strokeStyle = gradient
    canvasCtx.lineWidth = 2 - s * 0.2
    canvasCtx.stroke()
    canvasCtx.restore()
  }

  // 内层小螺旋
  for (let s = 0; s < 3; s++) {
    canvasCtx.save()
    canvasCtx.translate(cx, cy)
    canvasCtx.rotate(-time * (0.3 + s * 0.1) + (s * Math.PI * 2) / 3)

    canvasCtx.beginPath()
    for (let i = 0; i < 40; i++) {
      const t = i / 40
      const r = radius * t * 0.6
      const angle = t * Math.PI * 3
      const x = Math.cos(angle) * r
      const y = Math.sin(angle) * r

      if (i === 0) {
        canvasCtx.moveTo(x, y)
      } else {
        canvasCtx.lineTo(x, y)
      }
    }

    canvasCtx.strokeStyle = hexToRgba(colors.gold, 0.2)
    canvasCtx.lineWidth = 1
    canvasCtx.stroke()
    canvasCtx.restore()
  }

  // 中心光球
  const ballGradient = canvasCtx.createRadialGradient(cx, cy, 0, cx, cy, radius * 0.4)
  ballGradient.addColorStop(0, hexToRgba(colors.gold, 0.9))
  ballGradient.addColorStop(0.3, hexToRgba(colors.jade, 0.5))
  ballGradient.addColorStop(0.7, hexToRgba(colors.gold, 0.2))
  ballGradient.addColorStop(1, hexToRgba(colors.gold, 0))

  canvasCtx.beginPath()
  canvasCtx.arc(cx, cy, radius * 0.4, 0, Math.PI * 2)
  canvasCtx.fillStyle = ballGradient
  canvasCtx.fill()
}

// 绘制中心阵纹
function drawCenterPattern(cx, cy, radius, time, colors) {
  canvasCtx.save()
  canvasCtx.translate(cx, cy)

  // 同心圆环
  for (let i = 0; i < 3; i++) {
    canvasCtx.beginPath()
    canvasCtx.arc(0, 0, radius * (0.3 + i * 0.25), 0, Math.PI * 2)
    canvasCtx.strokeStyle = hexToRgba(colors.gold, 0.15 - i * 0.03)
    canvasCtx.lineWidth = 1
    canvasCtx.setLineDash([3, 3])
    canvasCtx.stroke()
  }

  // 四象限分隔线
  for (let i = 0; i < 4; i++) {
    canvasCtx.rotate(Math.PI / 2)
    canvasCtx.beginPath()
    canvasCtx.moveTo(0, 0)
    canvasCtx.lineTo(0, -radius * 0.9)
    canvasCtx.strokeStyle = hexToRgba(colors.jade, 0.2)
    canvasCtx.lineWidth = 1
    canvasCtx.stroke()
  }

  // 旋转的阴阳分割线
  canvasCtx.rotate(time * 0.5)
  canvasCtx.beginPath()
  canvasCtx.arc(0, 0, radius * 0.5, 0, Math.PI)
  canvasCtx.strokeStyle = hexToRgba(colors.gold, 0.4)
  canvasCtx.lineWidth = 2
  canvasCtx.stroke()

  canvasCtx.beginPath()
  canvasCtx.arc(radius * 0.25, 0, radius * 0.25, 0, Math.PI * 2)
  canvasCtx.fillStyle = hexToRgba(colors.gold, 0.3)
  canvasCtx.fill()

  canvasCtx.beginPath()
  canvasCtx.arc(-radius * 0.25, 0, radius * 0.25, 0, Math.PI * 2)
  canvasCtx.fillStyle = hexToRgba(colors.jade, 0.3)
  canvasCtx.fill()

  canvasCtx.restore()
}

// 颜色转换辅助函数
function hexToRgba(hex, alpha) {
  // 处理CSS变量值可能带有的空格
  hex = hex.trim()

  // 如果是rgb/rgba格式，直接解析
  if (hex.startsWith('rgb')) {
    const match = hex.match(/rgba?\(([^)]+)\)/)
    if (match) {
      const parts = match[1].split(',').map(p => parseFloat(p.trim()))
      if (parts.length >= 3) {
        return `rgba(${parts[0]}, ${parts[1]}, ${parts[2]}, ${alpha})`
      }
    }
  }

  // 处理十六进制
  let r, g, b
  if (hex.startsWith('#')) {
    if (hex.length === 4) {
      r = parseInt(hex[1] + hex[1], 16)
      g = parseInt(hex[2] + hex[2], 16)
      b = parseInt(hex[3] + hex[3], 16)
    } else if (hex.length === 7) {
      r = parseInt(hex.slice(1, 3), 16)
      g = parseInt(hex.slice(3, 5), 16)
      b = parseInt(hex.slice(5, 7), 16)
    } else {
      return `rgba(124, 58, 237, ${alpha})` // 默认紫罗兰色
    }
  } else {
    return `rgba(124, 58, 237, ${alpha})` // 默认紫罗兰色
  }

  return `rgba(${r}, ${g}, ${b}, ${alpha})`
}

// 添加升级特效
function addUpgradeEffect() {
  const core = document.querySelector('.array-core')
  if (core) {
    core.classList.add('upgrading')
    setTimeout(() => core.classList.remove('upgrading'), 1000)
  }
}

// 添加元素升级特效
function addElementUpgradeEffect(key) {
  const element = document.querySelector(`.element-node.${key}`)
  if (element) {
    element.classList.add('upgrading')
    setTimeout(() => element.classList.remove('upgrading'), 1000)
  }
}

// ==================== 生命周期 ====================

onMounted(async () => {
  try {
    await loadFiveElementInfo()
  } catch (error) {
    toast.error(error.message || '加载五行聚灵阵失败。')
  }

  if (enableVisualEffects) {
    initCanvas()

    // 启动动画循环
    if (animationFrame) {
      cancelAnimationFrame(animationFrame)
    }
    animate()

    window.addEventListener('resize', initCanvas)

    // 监听主题变化
    const observer = new MutationObserver((mutations) => {
      mutations.forEach((mutation) => {
        if (mutation.attributeName === 'data-theme') {
          initParticles() // 重新初始化粒子颜色
        }
      })
    })

    observer.observe(document.documentElement, {
      attributes: true,
      attributeFilter: ['data-theme']
    })
  }
})

onUnmounted(() => {
  if (animationFrame) {
    cancelAnimationFrame(animationFrame)
  }
  window.removeEventListener('resize', initCanvas)
})

// 监听等级变化
watch([arrayLevel, elements], () => {
  // 可以在这里添加等级变化时的特殊效果
}, { deep: true })
</script>

<style scoped>
/* 使用CSS变量适配三主题 */
.five-elements-array {
  width: 100%;
  height: 100%;
  background: var(--xiuxian-bg-panel);
  border: 2px solid var(--border-color);
  border-radius: var(--radius-lg);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  box-shadow: var(--shadow-sm);
}

/* 标题栏 */
.array-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--spacing-md);
  background: var(--xiuxian-bg-secondary);
  border-bottom: 1px solid var(--border-color);
}

.header-title {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
}

.title-icon {
  font-size: 24px;
  animation: rotate 10s linear infinite;
}

@keyframes rotate {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

.title-text {
  font-size: 18px;
  font-weight: 700;
  color: var(--highlight-text);
}



.header-level {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
}

.level-badge {
  padding: 4px 10px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 600;
  color: white;
  text-shadow: 0 1px 2px rgba(0, 0, 0, 0.5);
}

.level-badge.tier-1 { background: oklch(0.6 0 0 / 0.15); }
.level-badge.tier-2 { background: oklch(0.6 0.12 145 / 0.15); }
.level-badge.tier-3 { background: oklch(0.6 0.12 250 / 0.15); }
.level-badge.tier-4 { background: oklch(0.55 0.15 25 / 0.15); }
.level-badge.tier-5 { background: oklch(0.7 0.12 80 / 0.15); }
.level-badge.tier-6 { background: oklch(0.55 0.18 300 / 0.15); }

.level-text {
  font-size: 14px;
  color: var(--highlight-text);
  font-weight: 600;
}

/* 主体区域 */
.array-body {
  flex: 1;
  display: flex;
  flex-direction: column;
  padding: var(--spacing-md);
  gap: var(--spacing-md);
  overflow-y: auto;
  background: var(--xiuxian-bg-primary);
}

/* 阵图区域 */
.array-diagram {
  position: relative;
  width: 100%;
  aspect-ratio: 1;
  max-width: 280px;
  margin: 0 auto;
  border-radius: 50%;
  background: radial-gradient(circle at center, rgba(124, 58, 237, 0.15) 0%, transparent 70%);
}

.array-canvas {
  position: absolute;
  top: -20px;
  left: 0;
  width: 100%;
  height: 100%;
  border-radius: 50%;
}

/* 五行元素节点 */
.elements-container {
  position: absolute;
  top: -20px;
  left: 0;
  width: 100%;
  height: 100%;
  pointer-events: none;
}

.element-node {
  position: absolute;
  transform: translate(-50%, -50%);
  width: 48px;
  height: 48px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  pointer-events: auto;
  transition: all 0.15s ease;
}

.element-node:hover {
  transform: translate(-50%, -50%) scale(1.15);
  z-index: 10;
}

.element-node.upgrading {
  animation: upgradePulse 1s ease;
}

@keyframes upgradePulse {
  0%, 100% { transform: translate(-50%, -50%) scale(1); }
  50% { transform: translate(-50%, -50%) scale(1.3); filter: brightness(1.5); }
}

.element-aura {
  position: absolute;
  width: 100%;
  height: 100%;
  border-radius: 50%;
  background: radial-gradient(circle at center, var(--glow-color) 0%, transparent 70%);
  opacity: 0.3;
  animation: auraPulse 2s ease-in-out infinite;
}

@keyframes auraPulse {
  0%, 100% { transform: scale(1); opacity: 0.3; }
  50% { transform: scale(1.2); opacity: 0.5; }
}

.element-ring {
  position: absolute;
  border-radius: 50%;
  transition: all 0.15s ease;
}

.element-ring.outer {
  width: 48px;
  height: 48px;
  border: 2px solid;
  opacity: 0.6;
  animation: ringRotate 10s linear infinite;
}

.element-ring.middle {
  width: 38px;
  height: 38px;
  border: 1.5px dashed;
  opacity: 0.5;
  animation: ringRotate 8s linear infinite reverse;
}

.element-ring.inner {
  width: 28px;
  height: 28px;
  opacity: 0.3;
  box-shadow: 0 0 10px currentColor;
}

@keyframes ringRotate {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

.element-icon {
  position: relative;
  z-index: 1;
  font-size: 20px;
  filter: drop-shadow(0 0 5px rgba(0, 0, 0, 0.5));
}

.element-level-badge {
  position: absolute;
  bottom: -5px;
  right: -5px;
  width: 18px;
  height: 18px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  font-weight: 700;
  color: white;
  background: var(--button-primary-start);
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.3);
}

/* 各元素特殊样式 */
.element-node.metal .element-aura { --glow-color: var(--element-metal-glow); }
.element-node.metal .element-ring { border-color: var(--element-metal); }
.element-node.metal .element-ring.inner { background-color: var(--element-metal); }
.element-node.metal .element-level-badge { background-color: var(--element-metal); color: #333; }

.element-node.wood .element-aura { --glow-color: var(--element-wood-glow); }
.element-node.wood .element-ring { border-color: var(--element-wood); }
.element-node.wood .element-ring.inner { background-color: var(--element-wood); }
.element-node.wood .element-level-badge { background-color: var(--element-wood); }

.element-node.water .element-aura { --glow-color: var(--element-water-glow); }
.element-node.water .element-ring { border-color: var(--element-water); }
.element-node.water .element-ring.inner { background-color: var(--element-water); }
.element-node.water .element-level-badge { background-color: var(--element-water); }

.element-node.fire .element-aura { --glow-color: var(--element-fire-glow); }
.element-node.fire .element-ring { border-color: var(--element-fire); }
.element-node.fire .element-ring.inner { background-color: var(--element-fire); }
.element-node.fire .element-level-badge { background-color: var(--element-fire); }

.element-node.earth .element-aura { --glow-color: var(--element-earth-glow); }
.element-node.earth .element-ring { border-color: var(--element-earth); }
.element-node.earth .element-ring.inner { background-color: var(--element-earth); }
.element-node.earth .element-level-badge { background-color: var(--element-earth); }

/* 中心阵眼 */
.array-core {
  position: absolute;
  top: calc(50% - 20px);
  left: 50%;
  transform: translate(-50%, -50%);
  width: 60px;
  height: 60px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.array-core.upgrading {
  animation: coreUpgrade 1s ease;
}

@keyframes coreUpgrade {
  0%, 100% { transform: translate(-50%, -50%) scale(1); }
  50% { transform: translate(-50%, -50%) scale(1.5); }
}

.core-aura {
  position: absolute;
  width: 100%;
  height: 100%;
  border-radius: 50%;
  background: radial-gradient(circle at center, rgba(212, 168, 83, 0.15) 0%, transparent 70%);
  animation: coreAuraPulse 2s ease-in-out infinite;
}

@keyframes coreAuraPulse {
  0%, 100% { transform: scale(1); opacity: 0.5; }
  50% { transform: scale(1.3); opacity: 0.8; }
}

.core-ring {
  position: absolute;
  border-radius: 50%;
  border: 2px solid var(--highlight-text);
}

.core-ring.outer {
  width: 60px;
  height: 60px;
  opacity: 0.4;
  animation: coreRotate 15s linear infinite;
}

.core-ring.middle {
  width: 48px;
  height: 48px;
  opacity: 0.5;
  border-style: dashed;
  animation: coreRotate 10s linear infinite reverse;
}

.core-ring.inner {
  width: 36px;
  height: 36px;
  background: var(--highlight-text);
  opacity: 0.8;
  box-shadow: var(--shadow-sm);
}

@keyframes coreRotate {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

.core-symbol {
  position: relative;
  z-index: 1;
  font-size: 20px;
  color: var(--xiuxian-bg-primary);
  font-weight: 700;
}

.core-level {
  position: absolute;
  bottom: -8px;
  font-size: 12px;
  font-weight: 700;
  color: var(--highlight-text);
  background: var(--xiuxian-bg-secondary);
  padding: 2px 6px;
  border-radius: 10px;
  border: 1px solid var(--border-color);
}

/* 点击提示 */
.click-hint {
  position: absolute;
  bottom: -12px;
  left: 50%;
  transform: translateX(-50%);
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 4px 10px;
  background: var(--bg-overlay-medium);
  border: 1px solid var(--border-color);
  border-radius: 12px;
  font-size: 11px;
  color: var(--text-muted);
  pointer-events: none;
  opacity: 0.8;
}

.hint-icon {
  font-size: 12px;
}

/* 效果展示面板 */
.effects-panel {
  background: var(--bg-overlay-light);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  padding: 12px;
}

.panel-title {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  font-weight: 600;
  color: var(--highlight-text);
  margin-bottom: 10px;
}

.effects-list {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 8px;
}

.effect-item {
  display: grid;
  grid-template-columns: 28px 1fr auto;
  align-items: center;
  gap: 6px;
  padding: 6px 8px;
  background: var(--bg-overlay-light);
  border-radius: var(--radius-sm);
  transition: all 0.15s ease;
  min-width: 0;
}

.effect-item:hover {
  background: var(--bg-overlay-medium);
}

.effect-icon {
  width: 28px;
  height: 28px;
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 14px;
  flex-shrink: 0;
}

.effect-bg-metal { background: linear-gradient(135deg, var(--element-metal), var(--element-metal-glow)); }
.effect-bg-wood { background: linear-gradient(135deg, var(--element-wood), var(--element-wood-glow)); }
.effect-bg-water { background: linear-gradient(135deg, var(--element-water), var(--element-water-glow)); }
.effect-bg-fire { background: linear-gradient(135deg, var(--element-fire), var(--element-fire-glow)); }
.effect-bg-earth { background: linear-gradient(135deg, var(--element-earth), var(--element-earth-glow)); }
.effect-bg-array { background: var(--highlight-text); }

.effect-content {
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 1px;
}

.effect-name {
  font-size: 11px;
  font-weight: 600;
  color: var(--text-primary);
  line-height: 1.3;
}

.effect-desc {
  font-size: 10px;
  color: var(--text-muted);
  line-height: 1.2;
}

.effect-value {
  font-size: 12px;
  font-weight: 700;
  font-family: var(--font-mono);
  white-space: nowrap;
  padding-left: 4px;
}

/* 阵法升级区域 - 简化版 */
.array-upgrade-section {
  background: var(--bg-overlay-light);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  padding: var(--spacing-md);
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
}

.upgrade-main {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: var(--spacing-md);
}

.upgrade-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
  flex: 1;
}

.upgrade-title {
  font-size: 14px;
  font-weight: 600;
  color: var(--text-primary);
}

.upgrade-meta {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  font-size: 12px;
  flex-wrap: wrap;
}

.upgrade-cost {
  color: var(--highlight-text);
  font-weight: 500;
}

.upgrade-cost.max {
  color: var(--text-muted);
}

.level-progress {
  color: var(--text-muted);
  font-size: 11px;
}

.upgrade-materials {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 10px;
}

.upgrade-material {
  position: relative;
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 4px;
  min-height: 72px;
  padding: 10px 12px;
  border-radius: var(--radius-sm);
  background: linear-gradient(180deg, rgba(255,255,255,0.03), var(--bg-overlay-medium));
  color: var(--text-muted);
  font-size: 11px;
  border: 1px solid rgba(255,255,255,0.06);
}

.upgrade-material.enough {
  color: var(--quality-uncommon);
  border-color: rgba(76, 175, 80, 0.45);
  box-shadow: inset 0 0 0 1px rgba(76, 175, 80, 0.08);
}

.upgrade-material-kind {
  display: inline-flex;
  align-items: center;
  padding: 2px 8px;
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.08);
  color: var(--text-muted);
  font-size: 10px;
  letter-spacing: 0.04em;
}

.upgrade-material-name {
  color: var(--text-secondary);
  font-size: 13px;
  font-weight: 600;
  line-height: 1.3;
}

.upgrade-material-count {
  color: inherit;
  font-family: var(--font-mono);
  font-size: 12px;
  margin-top: auto;
}

.modal-materials {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
}

.upgrade-material.upgrade-currency .upgrade-material-kind {
  background: rgba(212, 168, 83, 0.12);
}

.modal-materials .upgrade-material {
  min-height: auto;
  flex-direction: row;
  align-items: center;
  justify-content: space-between;
}

.modal-materials .upgrade-material-kind {
  min-width: 42px;
  justify-content: center;
}

.modal-materials .upgrade-material-name {
  flex: 1;
  min-width: 0;
}

.modal-materials .upgrade-material-count {
  margin-top: 0;
  white-space: nowrap;
}

.upgrade-btn {
  padding: 8px 16px;
  border: none;
  border-radius: var(--radius-sm);
  background: var(--xiuxian-bg-secondary);
  color: var(--text-muted);
  font-size: 13px;
  font-weight: 600;
  cursor: not-allowed;
  transition: all 0.15s ease;
  white-space: nowrap;
}

.upgrade-btn.can-upgrade {
  background: var(--button-primary-start);
  color: white;
  cursor: pointer;
}

.upgrade-btn.can-upgrade:hover {
  background: var(--button-primary-start);
  opacity: 0.85;
  box-shadow: var(--shadow-sm);
}

.upgrade-btn.max {
  background: var(--xiuxian-bg-secondary);
}

.array-upgrade-section .progress-bar {
  width: 100%;
  height: 4px;
  background: var(--xiuxian-bg-secondary);
  border-radius: 2px;
  overflow: hidden;
}

.array-upgrade-section .progress-fill {
  height: 100%;
  background: var(--button-primary-start);
  border-radius: 2px;
  transition: width 0.5s ease;
}

/* 升级弹窗 */
.upgrade-modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: var(--overlay-bg);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 100000;
  backdrop-filter: blur(5px);
}

.upgrade-modal {
  width: 320px;
  background: var(--xiuxian-bg-panel);
  border: 2px solid var(--border-color);
  border-radius: var(--radius-lg);
  overflow: hidden;
  box-shadow: var(--modal-shadow);
}

.modal-header {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  padding: var(--spacing-md);
  background: var(--xiuxian-bg-secondary);
  position: relative;
}

.modal-header-metal { background: linear-gradient(135deg, rgba(192, 192, 192, 0.3), var(--xiuxian-bg-secondary)); }
.modal-header-wood { background: linear-gradient(135deg, rgba(46, 139, 87, 0.3), var(--xiuxian-bg-secondary)); }
.modal-header-water { background: linear-gradient(135deg, rgba(0, 191, 255, 0.3), var(--xiuxian-bg-secondary)); }
.modal-header-fire { background: linear-gradient(135deg, rgba(255, 99, 71, 0.3), var(--xiuxian-bg-secondary)); }
.modal-header-earth { background: linear-gradient(135deg, rgba(205, 133, 63, 0.3), var(--xiuxian-bg-secondary)); }

.modal-icon {
  font-size: 28px;
}

.modal-title {
  flex: 1;
  font-size: 18px;
  font-weight: 700;
  color: var(--text-primary);
}

.close-btn {
  width: 28px;
  height: 28px;
  border: none;
  border-radius: 50%;
  background: var(--bg-overlay-medium);
  color: var(--text-primary);
  font-size: 20px;
  cursor: pointer;
  transition: all 0.15s ease;
}

.close-btn:hover {
  background: var(--status-danger-bg);
  color: var(--status-danger-text);
}

.modal-body {
  padding: var(--spacing-lg);
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--spacing-md);
}

.element-preview {
  position: relative;
  width: 80px;
  height: 80px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.preview-aura {
  position: absolute;
  width: 100%;
  height: 100%;
  border-radius: 50%;
  background: radial-gradient(circle at center, var(--glow-color, rgba(124, 58, 237, 0.15)) 0%, transparent 70%);
  animation: previewPulse 2s ease-in-out infinite;
}

@keyframes previewPulse {
  0%, 100% { transform: scale(1); opacity: 0.5; }
  50% { transform: scale(1.2); opacity: 0.8; }
}

.preview-ring {
  position: absolute;
  width: 60px;
  height: 60px;
  border-radius: 50%;
  border: 2px solid var(--border-color);
  animation: previewRotate 5s linear infinite;
}

@keyframes previewRotate {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

.preview-icon {
  position: relative;
  font-size: 36px;
  z-index: 1;
}

.preview-level {
  position: absolute;
  bottom: 0;
  font-size: 14px;
  font-weight: 700;
  color: var(--highlight-text);
  background: var(--xiuxian-bg-secondary);
  padding: 2px 10px;
  border-radius: 10px;
}

.upgrade-details {
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: var(--spacing-xs);
}

.detail-item {
  display: flex;
  justify-content: space-between;
  padding: var(--spacing-sm);
  background: var(--bg-overlay-light);
  border-radius: var(--radius-sm);
}

.detail-label {
  font-size: 13px;
  color: var(--text-muted);
}

.detail-value {
  font-size: 13px;
  font-weight: 600;
  color: var(--text-primary);
}

.detail-value.bonus {
  color: var(--accent-text);
}

.detail-value.next-bonus {
  color: var(--highlight-text);
}

.upgrade-actions {
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
}

.cost-info {
  display: flex;
  justify-content: space-between;
  padding: var(--spacing-sm);
  background: rgba(212, 168, 83, 0.1);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-sm);
}

.cost-label {
  font-size: 13px;
  color: var(--text-muted);
}

.cost-value {
  font-size: 14px;
  font-weight: 700;
  color: var(--highlight-text);
}

.confirm-upgrade-btn {
  width: 100%;
  padding: 12px;
  border: none;
  border-radius: var(--radius-md);
  background: var(--xiuxian-bg-secondary);
  color: var(--text-muted);
  font-size: 15px;
  font-weight: 600;
  cursor: not-allowed;
  transition: all 0.15s ease;
}

.confirm-upgrade-btn.can-upgrade {
  background: var(--button-primary-start);
  color: white;
  cursor: pointer;
}

.confirm-upgrade-btn.can-upgrade:hover {
  background: var(--button-primary-start);
  opacity: 0.85;
  box-shadow: var(--shadow-sm);
}

.confirm-upgrade-btn.max {
  background: var(--xiuxian-bg-secondary);
}

/* 满级样式 */
.element-node.max-level .element-level-badge {
  background: var(--highlight-text) !important;
  animation: maxLevelGlow 2s ease-in-out infinite;
}

@keyframes maxLevelGlow {
  0%, 100% { box-shadow: 0 0 5px var(--shadow-sm); }
  50% { box-shadow: 0 0 15px var(--shadow-sm); }
}

.array-core.max-level .core-ring.inner {
  background: var(--highlight-text);
  animation: coreMaxGlow 2s ease-in-out infinite;
}

@keyframes coreMaxGlow {
  0%, 100% { box-shadow: 0 0 20px var(--shadow-sm); }
  50% { box-shadow: 0 0 40px var(--shadow-sm); }
}

@media (max-width: 900px) {
  .upgrade-materials {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

/* 常驻聚灵阵使用静态、紧凑展示，保留升级交互。 */
.five-elements-array {
  border-width: 1px;
  border-radius: var(--radius-lg);
  box-shadow: none;
}

.array-header {
  padding: var(--spacing-sm) var(--spacing-md);
}

.title-text {
  font-size: var(--font-size-base);
  color: var(--text-primary);
}

.array-body {
  display: grid;
  grid-template-columns: 1fr;
  grid-template-rows: auto auto auto;
  align-content: start;
  min-height: 0;
  padding: var(--spacing-sm) var(--spacing-lg);
  gap: var(--spacing-xs);
  overflow: hidden;
}

.array-diagram {
  position: relative;
  grid-column: 1 / -1;
  width: 166px;
  height: 166px;
  justify-self: center;
  align-self: center;
  margin: 0;
}

.array-links {
  position: absolute;
  inset: 9px;
  width: calc(100% - 18px);
  height: calc(100% - 18px);
  fill: none;
  stroke-linecap: round;
  stroke-linejoin: round;
}

.array-orbit {
  stroke: var(--border-color);
  stroke-width: 1.25;
}

.array-spokes {
  stroke: var(--border-color);
  stroke-width: 1;
}

.element-node {
  position: absolute;
  transform: translate(-50%, -50%);
  width: 30px;
  height: 30px;
  padding: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 1px;
  border: 1px solid var(--border-color);
  border-radius: 50%;
  background: var(--xiuxian-bg-panel);
  color: var(--text-primary);
  cursor: pointer;
  transition: background 0.15s ease, border-color 0.15s ease;
}

.element-node:hover,
.element-node.active {
  transform: translate(-50%, -50%);
  background: var(--bg-overlay-medium);
  border-color: var(--text-secondary);
}

.element-node.metal { color: var(--element-metal); }
.element-node.wood { color: var(--element-wood); }
.element-node.water { color: var(--element-water); }
.element-node.fire { color: var(--element-fire); }
.element-node.earth { color: var(--element-earth); }

.element-name {
  color: currentColor;
  font-size: 7px;
  line-height: 1;
}

.element-level {
  position: absolute;
  right: -5px;
  bottom: -5px;
  min-width: 15px;
  height: 15px;
  border: 1px solid var(--border-color);
  border-radius: 50%;
  background: var(--xiuxian-bg-panel);
  color: var(--text-secondary);
  font-size: 8px;
  line-height: 13px;
  text-align: center;
  font-family: var(--font-mono);
}

.array-core {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  width: 44px;
  height: 44px;
  padding: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  border: 1px solid var(--text-secondary);
  border-radius: 50%;
  background: var(--xiuxian-bg-panel);
  color: var(--text-primary);
  cursor: pointer;
}

.array-core.active {
  background: var(--bg-overlay-medium);
}

.core-level {
  position: absolute;
  bottom: -9px;
  padding: 1px 6px;
  border: 1px solid var(--border-color);
  border-radius: 999px;
  background: var(--xiuxian-bg-panel);
  color: var(--text-secondary);
  font-size: 9px;
  white-space: nowrap;
}

.effects-panel {
  min-width: 0;
  align-self: end;
  padding: 6px var(--spacing-md);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  background: var(--bg-overlay-light);
}

.panel-title {
  margin-bottom: 2px;
  color: var(--text-secondary);
  font-size: var(--font-size-xs);
  font-weight: 600;
}

.effects-list {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0 var(--spacing-lg);
}

.effect-item {
  display: flex;
  justify-content: space-between;
  gap: var(--spacing-sm);
  padding: 4px 0;
  border-bottom: 1px solid var(--border-color);
  color: var(--text-secondary);
  font-size: var(--font-size-xs);
}

.effect-item:last-child {
  border-bottom: 0;
}

.effect-value {
  color: var(--text-primary);
  font-family: var(--font-mono);
}

.array-upgrade-section {
  grid-column: 1 / -1;
  padding-top: var(--spacing-sm);
  border-top: 1px solid var(--border-color);
}

.upgrade-main {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--spacing-md);
}

.upgrade-info {
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.upgrade-title {
  color: var(--text-primary);
  font-size: var(--font-size-sm);
}

.upgrade-title strong {
  font-family: var(--font-mono);
}

.upgrade-title small,
.level-progress {
  color: var(--text-muted);
  font-size: var(--font-size-xs);
}

.upgrade-btn {
  min-width: 56px;
  padding: 7px 12px;
}

.array-upgrade-section .progress-bar {
  margin-top: var(--spacing-sm);
}

.array-core.upgrading,
.element-node.upgrading,
.title-icon,
.element-aura,
.element-ring.outer,
.element-ring.middle,
.core-aura,
.core-ring.outer,
.core-ring.middle {
  animation: none;
}

@media (max-width: 640px) {
  .array-body {
    grid-template-columns: 1fr;
    grid-template-rows: auto auto auto;
    overflow-y: auto;
  }

  .array-diagram {
    grid-row: auto;
    margin: 0 auto;
  }
}

@media (max-width: 640px) {
  .upgrade-materials {
    grid-template-columns: 1fr;
  }
}

.five-elements-array.in-modal {
  border: none;
  box-shadow: none;
  background: transparent;
  border-radius: 0;
}

@media (min-width: 640px) {
  .five-elements-array.in-modal .array-body {
    display: grid;
    grid-template-columns: 280px 1fr;
    grid-template-rows: auto auto;
    align-content: start;
    min-height: 0;
    align-items: start;
  }
  .five-elements-array.in-modal .array-diagram {
    grid-column: 1;
    grid-row: 1 / span 2;
    margin: 0;
  }
  .five-elements-array.in-modal .effects-panel {
    grid-column: 2;
    grid-row: 1;
  }
  .five-elements-array.in-modal .array-upgrade-section {
    grid-column: 2;
    grid-row: 2;
  }
}
/* 方案二：五行进度条列表。每个元素独立展示等级与成长进度。 */
.elements-strip {
  display: flex;
  flex-direction: column;
  gap: 4px;
  min-width: 0;
  padding: 1px 0;
}

.elements-strip .element-node {
  position: relative;
  overflow: visible;
  transform: none;
  width: 100%;
  height: 28px;
  min-width: 0;
  padding: 3px 8px;
  display: grid;
  grid-template-columns: 24px 30px 42px minmax(60px, 1fr);
  align-items: center;
  gap: 6px;
  border: 1px solid color-mix(in srgb, currentColor 24%, var(--border-color));
  border-radius: var(--radius-sm);
  background: color-mix(in srgb, currentColor 7%, var(--xiuxian-bg-panel));
  color: var(--text-primary);
  cursor: pointer;
  transition: background 0.15s ease, border-color 0.15s ease;
}

.elements-strip .element-node:hover,
.elements-strip .element-node.active {
  border-color: currentColor;
  background: color-mix(in srgb, currentColor 12%, var(--xiuxian-bg-panel));
}

.elements-strip .element-node.metal { color: var(--element-metal); }
.elements-strip .element-node.wood { color: var(--element-wood); }
.elements-strip .element-node.water { color: var(--element-water); }
.elements-strip .element-node.fire { color: var(--element-fire); }
.elements-strip .element-node.earth { color: var(--element-earth); }

.element-node-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.element-node-name {
  color: currentColor;
  font-size: 12px;
  font-weight: 700;
  line-height: 1;
}

.element-node-level {
  color: var(--text-secondary);
  font-size: 10px;
  font-family: var(--font-mono);
  line-height: 1;
  white-space: nowrap;
}

.element-tooltip {
  position: absolute;
  z-index: 30;
  top: calc(100% + 6px);
  left: 8px;
  width: max-content;
  max-width: min(280px, calc(100vw - 32px));
  padding: 7px 10px;
  border: 1px solid color-mix(in srgb, currentColor 45%, var(--border-color));
  border-radius: var(--radius-sm);
  background: var(--xiuxian-bg-panel);
  color: var(--text-primary);
  box-shadow: 0 8px 20px rgb(0 0 0 / 24%);
  font-size: 11px;
  font-weight: 500;
  line-height: 1.65;
  text-align: left;
  white-space: nowrap;
  pointer-events: none;
}

.element-node-track {
  display: block;
  width: 100%;
  height: 5px;
  overflow: hidden;
  border-radius: 999px;
  background: var(--xiuxian-bg-secondary);
}

.element-node-fill {
  display: block;
  height: 100%;
  min-width: 2px;
  border-radius: inherit;
  background: currentColor;
  opacity: 0.75;
}

/* 覆盖旧版阵图预留高度，列表按内容排列，避免缩放时出现空白。 */
.five-elements-array:not(.in-modal) .array-body {
  grid-template-rows: auto auto auto;
  align-content: start;
  gap: var(--spacing-sm);
  padding: var(--spacing-sm) var(--spacing-md);
  overflow: visible;
}

/* 悬浮详情位于列表项外部，不能被主体容器的裁剪规则截断。 */
.five-elements-array:not(.in-modal) .effects-panel {
  align-self: stretch;
  padding: 7px 10px 5px;
}

.five-elements-array:not(.in-modal) .array-upgrade-section {
  padding: 7px 0 0;
}

@media (max-width: 420px) {
  .elements-strip .element-node {
    grid-template-columns: 22px 26px 38px minmax(40px, 1fr);
    gap: 4px;
    padding-inline: 6px;
  }
}

</style>
