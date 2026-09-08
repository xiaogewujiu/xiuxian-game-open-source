<template>
  <XiuXianModal
    :model-value="modelValue"
    title="灵田"
    :width="1000"
    @update:model-value="$emit('update:modelValue', $event)"
  >
    <div class="spirit-field-modal">
      <div class="summary-grid">
        <div class="summary-card">
          <span class="summary-label">灵田等级</span>
          <strong class="summary-value">Lv.{{ fieldInfo.fieldLevel }}</strong>
        </div>
        <div class="summary-card">
          <span class="summary-label">已解锁地块</span>
          <strong class="summary-value">{{ fieldInfo.unlockedPlots }}/{{ fieldInfo.maxPlots }}</strong>
        </div>
        <div class="summary-card">
          <span class="summary-label">产量加成（含聚灵阵）</span>
          <strong class="summary-value">+{{ fieldInfo.globalYieldBonus }}%</strong>
        </div>
        <div class="summary-card">
          <span class="summary-label">生长加成</span>
          <strong class="summary-value">+{{ fieldInfo.globalGrowthSpeedBonus }}%</strong>
        </div>
        <div class="summary-card">
          <span class="summary-label">今日播种</span>
          <strong class="summary-value">{{ fieldInfo.todayPlantCount }}</strong>
        </div>
        <div class="summary-card">
          <span class="summary-label">今日收获</span>
          <strong class="summary-value">{{ fieldInfo.todayHarvestCount }}</strong>
        </div>
      </div>

      <div class="resource-bar">
        <span>金币 {{ formatNumber(playerGold) }}</span>
        <span>灵石 {{ formatNumber(playerSpiritStone) }}</span>
        <span>累计收获 {{ fieldInfo.totalHarvestCount }}</span>
        <button class="refresh-btn" :disabled="isLoading" @click="loadFieldData">
          {{ isLoading ? '刷新中...' : '刷新灵田' }}
        </button>
      </div>

      <div class="field-layout">
        <section class="crop-panel">
          <div class="panel-card">
            <div class="panel-title">可种灵植</div>
            <div class="crop-list">
              <button
                v-for="crop in crops"
                :key="crop.templateId"
                class="crop-card"
                :class="{ selected: crop.templateId === selectedCropId }"
                @click="selectedCropId = crop.templateId"
              >
                <div class="crop-card-top">
                  <span class="crop-icon"><AssetIcon :source="crop.icon" size="20" /></span>
                  <span class="crop-name">{{ crop.name }}</span>
                </div>
                <div class="crop-meta">{{ crop.typeText }} · {{ formatDuration(crop.growthCycle) }}</div>
                <div class="crop-meta">消耗 {{ crop.seedName }} x{{ crop.seedAmount }}</div>
                <div class="crop-meta">库存 {{ getInventoryCount(crop.seedId) }}</div>
              </button>
            </div>
          </div>

          <div class="panel-card selected-panel">
            <div class="panel-title">当前选中</div>
            <template v-if="selectedCrop">
              <div class="selected-crop">
                <div class="selected-header">
                  <span class="selected-icon"><AssetIcon :source="selectedCrop.icon" size="20" /></span>
                  <div>
                    <div class="selected-name">{{ selectedCrop.name }}</div>
                    <div class="selected-type">{{ selectedCrop.typeText }}</div>
                  </div>
                </div>
                <div class="selected-meta">
                  <div>播种消耗 {{ selectedCrop.seedName }} x{{ selectedCrop.seedAmount }}</div>
                  <div>基础产出 {{ selectedCrop.outputAmount }} {{ selectedCrop.outputName }}</div>
                  <div>生长周期 {{ formatDuration(selectedCrop.growthCycle) }}</div>
                </div>
              </div>
            </template>
            <div v-else class="empty-card">
              <div class="empty-icon"><AssetIcon :source="ICON.misc_seedling" size="25" /></div>
              <div class="empty-text">当前还没有可种植的灵植。</div>
            </div>
          </div>

          <div class="panel-card speedup-panel">
            <div class="panel-title">催熟材料</div>
            <select v-model="selectedSpeedItemId" :disabled="speedItems.length === 0">
              <option value="" disabled>选择要消耗的材料</option>
              <option
                v-for="item in speedItems"
                :key="item.itemId"
                :value="item.itemId"
              >
                {{ item.name }} x{{ item.count }}
              </option>
            </select>
            <div class="hint-text">
              {{ speedUpRuleText }}
            </div>
          </div>
        </section>

        <section class="plot-panel">
          <div class="panel-title">地块</div>
          <div class="plot-grid">
            <div
              v-for="plot in plotList"
              :key="plot.plotNumber"
              class="plot-card"
              :class="[plot.statusKey, { locked: !plot.isUnlocked }]"
            >
              <div class="plot-header">
                <div class="plot-name">灵田 {{ plot.plotNumber }}</div>
                <span class="plot-level">Lv.{{ plot.level }}</span>
              </div>

              <div class="plot-status">
                <span class="status-dot"></span>
                <span>{{ plot.statusText }}</span>
              </div>

              <div v-if="plot.isUnlocked" class="plot-body">
                <div class="plot-crop-name">{{ plot.cropName || '空闲地块' }}</div>
                <div class="plot-meta">
                  <span>产量加成 +{{ plot.yieldBonusPercent }}%</span>
                  <span class="plot-remaining">
                    <template v-if="plot.statusKey === 'growing' || plot.statusKey === 'harvestable'">
                      {{ remainingText(plot) }}
                    </template>
                    <template v-else>&nbsp;</template>
                  </span>
                </div>

                <div class="progress-bar">
                  <div class="progress-fill" :style="{ width: `${plot.progressPercent}%` }"></div>
                </div>

                <div class="action-row">
                  <button
                    :class="plot.statusKey === 'empty' || plot.statusKey === 'harvestable' ? 'primary-btn' : 'secondary-btn'"
                    :disabled="plot.statusKey === 'empty' ? !canPlant(plot) : plot.statusKey === 'harvestable' ? isSubmitting : !canSpeedUp(plot)"
                    @click="plot.statusKey === 'empty' ? plantCrop(plot) : plot.statusKey === 'harvestable' ? harvestCrop(plot) : speedUpCrop(plot)"
                  >
                    {{ plot.statusKey === 'empty' ? '播种' : plot.statusKey === 'harvestable' ? '收获' : `催熟 ${selectedSpeedUpDurationText}` }}
                  </button>

                  <button
                    class="secondary-btn"
                    :disabled="!canUpgrade(plot)"
                    @click="upgradePlot(plot)"
                  >
                    升级
                  </button>
                </div>

                <div class="upgrade-cost">
                  升级消耗：{{ plot.upgradeCost.gold }} 金币 / {{ plot.upgradeCost.spiritStone }} 灵石
                </div>
              </div>

              <div v-else class="locked-body">
                <div class="empty-icon"><AssetIcon :source="ICON.ui_lock" size="25" /></div>
                <div class="empty-text">当前地块尚未解锁。</div>
              </div>
            </div>
          </div>
        </section>
      </div>

      <div v-if="statusMessage" class="status-message">{{ statusMessage }}</div>
    </div>
  </XiuXianModal>
</template>

<script>
import { computed, onBeforeUnmount, onMounted, reactive, ref, watch } from 'vue'
import XiuXianModal from '../common/XiuXianModal.vue'
import { apiClient } from '../../lib/apiClient'
import { useGameStore } from '../../state/gameStore'
import { buildInventoryPropList } from '../../services/gameDisplay'
import { ICON } from '../../icons'
import AssetIcon from '../common/AssetIcon.vue'

/*
 * 中文注释：
 * 灵田弹窗同时依赖灵田系统、作物模板和背包材料三类数据。
 * 这里统一收口，保证播种、催熟、升级后能用同一份状态即时刷新页面。
 */
const STATUS_META = {
  empty: '空闲',
  growing: '生长中',
  harvestable: '可收获',
  locked: '未解锁'
}

const CROP_ICON_BY_TYPE = {
  normal: ICON.misc_herb,
  spiritgrass: ICON.misc_lotus,
  herb: ICON.misc_seedling,
  spiritherb: ICON.misc_sparkles
}

function toNumber(value, fallback = 0) {
  const numericValue = Number(value)
  return Number.isFinite(numericValue) ? numericValue : fallback
}

function normalizeCropType(value) {
  const normalized = String(value || '').toLowerCase()
  return normalized || 'normal'
}

function normalizeCrop(crop) {
  // 中文注释：
  // 作物模板会在这里补齐图标、种子、产出等展示字段，
  // 后面的“可种灵植”和“当前选中”区域可以直接复用。
  const typeKey = normalizeCropType(crop?.Type ?? crop?.type)
  const icon = CROP_ICON_BY_TYPE[typeKey] || ICON.misc_seedling
  const seedId = crop?.SeedId ?? crop?.seedId ?? ''
  const outputItemId = crop?.OutputItemId ?? crop?.outputItemId ?? ''
  const seedName = crop?.SeedName ?? crop?.seedName ?? ''
  const outputName = crop?.OutputName ?? crop?.outputName ?? ''

  return {
    templateId: crop?.TemplateId ?? crop?.templateId ?? '',
    name: crop?.Name ?? crop?.name ?? '未知灵植',
    typeKey,
    typeText: crop?.Type ?? crop?.type ?? 'Normal',
    growthCycle: toNumber(crop?.GrowthCycle ?? crop?.growthCycle, 0),
    yield: toNumber(crop?.Yield ?? crop?.yield, 0),
    seedId,
    seedName: seedName || seedId || '种子',
    seedAmount: toNumber(crop?.SeedAmount ?? crop?.seedAmount, 1),
    outputItemId,
    outputName: outputName || outputItemId || '灵植产物',
    outputAmount: toNumber(crop?.OutputAmount ?? crop?.outputAmount, 0),
    unlockLevel: toNumber(crop?.UnlockLevel ?? crop?.unlockLevel, 1),
    icon
  }
}

function normalizePlot(plot, unlockedPlots) {
  // 中文注释：
  // 地块统一归一化成空闲、生长中、可收获、未解锁四种前端状态，
  // 模板层无需再关心后端原始枚举值和时间字段组合。
  const plotNumber = toNumber(plot?.PlotNumber ?? plot?.plotNumber, 0)
  const isUnlocked = plotNumber > 0 && plotNumber <= unlockedPlots
  const rawStatus = String(plot?.Status ?? plot?.status ?? '').toLowerCase()
  let statusKey = 'empty'

  if (!isUnlocked) {
    statusKey = 'locked'
  } else if (Boolean(plot?.CanHarvest ?? plot?.canHarvest) || rawStatus === 'harvestable') {
    statusKey = 'harvestable'
  } else if (rawStatus === 'growing') {
    statusKey = 'growing'
  }

  const level = toNumber(plot?.Level ?? plot?.level, 1)

  return {
    id: plot?.Id ?? plot?.id ?? plotNumber,
    plotNumber,
    level,
    statusKey,
    statusText: STATUS_META[statusKey],
    cropTemplateId: plot?.CropTemplateId ?? plot?.cropTemplateId ?? '',
    cropName: plot?.CropName ?? plot?.cropName ?? '',
    progressPercent: Math.max(0, Math.min(100, toNumber(plot?.ProgressPercent ?? plot?.progressPercent, statusKey === 'empty' ? 0 : 100))),
    plantTime: plot?.PlantTime ?? plot?.plantTime ?? null,
    expectedHarvestTime: plot?.ExpectedHarvestTime ?? plot?.expectedHarvestTime ?? null,
    canHarvest: Boolean(plot?.CanHarvest ?? plot?.canHarvest),
    isUnlocked,
    yieldBonusPercent: Math.max(0, (level - 1) * 5),
    upgradeCost: {
      gold: level * 1000,
      spiritStone: level * 10
    }
  }
}

export default {
  name: 'SpiritFieldModal',

  components: {
    XiuXianModal,
    AssetIcon
  },

  props: {
    modelValue: Boolean
  },

  emits: ['update:modelValue'],

  setup(props) {
    const gameStore = useGameStore()
    // 灵田弹窗核心状态：
    // crops 是可种植灵植，fieldInfo 是灵田总览，selectedCropId/selectedSpeedItemId 是当前操作上下文。
    const crops = ref([])
    const selectedCropId = ref('')
    const selectedSpeedItemId = ref('')
    const isLoading = ref(false)
    const isSubmitting = ref(false)
    const statusMessage = ref('')
    const nowTick = ref(Date.now())
    let clockTimer = null

    const fieldInfo = reactive({
      speedUpItems: [],
      fieldLevel: 1,
      unlockedPlots: 0,
      maxPlots: 9,
      globalYieldBonus: 0,
      globalGrowthSpeedBonus: 0,
      todayPlantCount: 0,
      todayHarvestCount: 0,
      totalPlantCount: 0,
      totalHarvestCount: 0,
      plots: []
    })

    const inventoryItems = computed(() => buildInventoryPropList(gameStore.state.inventory || []))
    const playerGold = computed(() => toNumber(gameStore.state.player?.gold ?? gameStore.state.player?.Gold, 0))
    const playerSpiritStone = computed(() => toNumber(gameStore.state.player?.spiritStone ?? gameStore.state.player?.SpiritStone, 0))

    const selectedCrop = computed(() => {
      return crops.value.find((crop) => crop.templateId === selectedCropId.value) || null
    })

    const speedItems = computed(() => {
      const inventoryMap = new Map(inventoryItems.value.map((item) => [item.itemId, item]))
      return fieldInfo.speedUpItems
        .map((rule) => ({
          ...rule,
          count: inventoryMap.get(rule.itemId)?.count || 0
        }))
        .filter((item) => item.count > 0)
    })

    const selectedSpeedItem = computed(() => {
      return speedItems.value.find((item) => item.itemId === selectedSpeedItemId.value) || null
    })

    const selectedSpeedUpDurationText = computed(() => {
      return selectedSpeedItem.value ? formatDuration(selectedSpeedItem.value.speedUpSeconds) : '加速'
    })

    const speedUpRuleText = computed(() => {
      const rules = fieldInfo.speedUpItems
        .map((item) => `${item.name}每次缩短${formatDuration(item.speedUpSeconds)}`)
      return rules.length > 0
        ? `可用催熟材料：${rules.join('、')}；每次消耗 1 件材料。`
        : '当前没有配置可用的催熟材料。'
    })

    const plotList = computed(() => {
      const normalizedPlots = Array.isArray(fieldInfo.plots) ? fieldInfo.plots : []
      const plotMap = new Map(normalizedPlots.map((plot) => [plot.plotNumber, plot]))
      const totalPlots = Math.max(fieldInfo.maxPlots || 0, normalizedPlots.length || 0, 9)
      const plots = []

      for (let plotNumber = 1; plotNumber <= totalPlots; plotNumber += 1) {
        const plot = plotMap.get(plotNumber) || normalizePlot({ PlotNumber: plotNumber, Level: 1 }, fieldInfo.unlockedPlots)
        plots.push({
          ...plot,
          isUnlocked: plotNumber <= fieldInfo.unlockedPlots,
          statusKey: plotNumber <= fieldInfo.unlockedPlots ? plot.statusKey : 'locked',
          statusText: plotNumber <= fieldInfo.unlockedPlots ? plot.statusText : STATUS_META.locked
        })
      }

      return plots.map((plot) => {
        if (!plot.isUnlocked || plot.statusKey !== 'growing' || !plot.expectedHarvestTime) {
          return plot
        }

        const start = plot.plantTime ? new Date(plot.plantTime).getTime() : 0
        const end = new Date(plot.expectedHarvestTime).getTime()
        const current = nowTick.value
        const liveProgress = start > 0 && end > start
          ? Math.max(0, Math.min(100, ((current - start) / (end - start)) * 100))
          : plot.progressPercent

        if (current >= end) {
          return {
            ...plot,
            statusKey: 'harvestable',
            statusText: STATUS_META.harvestable,
            progressPercent: 100,
            canHarvest: true
          }
        }

        return {
          ...plot,
          progressPercent: liveProgress
        }
      })
    })

    function startClock() {
      if (clockTimer) return
      nowTick.value = Date.now()
      clockTimer = window.setInterval(() => {
        nowTick.value = Date.now()
      }, 1000)
    }

    function stopClock() {
      if (!clockTimer) return
      window.clearInterval(clockTimer)
      clockTimer = null
    }

    function getInventoryItem(itemId) {
      return inventoryItems.value.find((item) => item.itemId === itemId) || null
    }

    function getInventoryCount(itemId) {
      return getInventoryItem(itemId)?.count || 0
    }

    function formatNumber(value) {
      return new Intl.NumberFormat('zh-CN').format(toNumber(value, 0))
    }

    function formatDuration(seconds) {
      const totalSeconds = Math.max(0, toNumber(seconds, 0))
      if (totalSeconds < 60) {
        return `${totalSeconds} 秒`
      }

      if (totalSeconds < 3600) {
        return `${Math.ceil(totalSeconds / 60)} 分钟`
      }

      const hours = Math.floor(totalSeconds / 3600)
      const minutes = Math.ceil((totalSeconds % 3600) / 60)
      return minutes > 0 ? `${hours} 小时 ${minutes} 分` : `${hours} 小时`
    }

    // 计算地块剩余时间文案。
    function remainingText(plot) {
      if (plot.statusKey === 'harvestable') {
        return '灵植已成熟'
      }

      if (!plot.expectedHarvestTime) {
        return '等待播种'
      }

      const expectedTime = new Date(plot.expectedHarvestTime)
      const remainingSeconds = Math.max(0, Math.ceil((expectedTime.getTime() - Date.now()) / 1000))
      if (remainingSeconds <= 0) {
        return '即将成熟'
      }

      return `剩余 ${formatDuration(remainingSeconds)}`
    }

    // 统一把后端灵田总览同步到前端展示结构。
    function applyFieldInfo(info) {
      fieldInfo.speedUpItems = Array.isArray(info?.SpeedUpItems || info?.speedUpItems)
        ? (info?.SpeedUpItems || info?.speedUpItems)
          .map((item) => ({
            itemId: item?.ItemId ?? item?.itemId ?? '',
            name: item?.Name ?? item?.name ?? item?.ItemId ?? item?.itemId ?? '未知材料',
            speedUpSeconds: Math.max(1, toNumber(item?.SpeedUpSeconds ?? item?.speedUpSeconds, 0))
          }))
          .filter((item) => item.itemId)
        : []
      fieldInfo.fieldLevel = toNumber(info?.FieldLevel ?? info?.fieldLevel, 1)
      fieldInfo.unlockedPlots = toNumber(info?.UnlockedPlots ?? info?.unlockedPlots, 0)
      fieldInfo.maxPlots = toNumber(info?.MaxPlots ?? info?.maxPlots, 9)
      fieldInfo.globalYieldBonus = toNumber(info?.GlobalYieldBonus ?? info?.globalYieldBonus, 0)
      fieldInfo.globalGrowthSpeedBonus = toNumber(info?.GlobalGrowthSpeedBonus ?? info?.globalGrowthSpeedBonus, 0)
      fieldInfo.todayPlantCount = toNumber(info?.TodayPlantCount ?? info?.todayPlantCount, 0)
      fieldInfo.todayHarvestCount = toNumber(info?.TodayHarvestCount ?? info?.todayHarvestCount, 0)
      fieldInfo.totalPlantCount = toNumber(info?.TotalPlantCount ?? info?.totalPlantCount, 0)
      fieldInfo.totalHarvestCount = toNumber(info?.TotalHarvestCount ?? info?.totalHarvestCount, 0)
      fieldInfo.plots = Array.isArray(info?.Plots || info?.plots)
        ? (info?.Plots || info?.plots).map((plot) => normalizePlot(plot, toNumber(info?.UnlockedPlots ?? info?.unlockedPlots, 0)))
        : []
    }

    // 读取灵田总览、作物模板和背包道具。
    async function loadFieldData() {
      isLoading.value = true

      try {
        const [field, cropList] = await Promise.all([
          apiClient.getSpiritFieldInfo(),
          apiClient.getSpiritFieldCrops(),
          gameStore.refreshPlayerSnapshot({
            includeInventory: true,
            includeRankings: false
          })
        ])

        applyFieldInfo(field)
        crops.value = Array.isArray(cropList) ? cropList.map(normalizeCrop) : []

        if (!selectedCropId.value || !crops.value.find((crop) => crop.templateId === selectedCropId.value)) {
          selectedCropId.value = crops.value[0]?.templateId || ''
        }

        if (!selectedSpeedItemId.value || !speedItems.value.find((item) => item.itemId === selectedSpeedItemId.value)) {
          selectedSpeedItemId.value = speedItems.value[0]?.itemId || ''
        }

        statusMessage.value = ''
      } catch (error) {
        statusMessage.value = error.message || '加载灵田数据失败。'
      } finally {
        isLoading.value = false
      }
    }

    // 当前是否允许播种。
    function canPlant(plot) {
      if (!plot.isUnlocked || plot.statusKey !== 'empty' || !selectedCrop.value || isSubmitting.value) {
        return false
      }

      return getInventoryCount(selectedCrop.value.seedId) >= selectedCrop.value.seedAmount
    }

    // 当前是否允许催熟。
    function canSpeedUp(plot) {
      if (!plot.isUnlocked || plot.statusKey !== 'growing' || !selectedSpeedItem.value || isSubmitting.value) {
        return false
      }

      return selectedSpeedItem.value.count > 0
    }

    // 当前是否允许升级地块。
    function canUpgrade(plot) {
      if (!plot.isUnlocked || isSubmitting.value) {
        return false
      }

      return playerGold.value >= plot.upgradeCost.gold && playerSpiritStone.value >= plot.upgradeCost.spiritStone
    }

    // 对灵田操作做一层统一包装：
    // 负责锁定按钮、刷新人物快照和重新拉取灵田数据。
    async function mutateField(action, successMessage) {
      isSubmitting.value = true

      try {
        await action()
        statusMessage.value = successMessage
        await gameStore.refreshPlayerSnapshot({
          includeInventory: true,
          includeRankings: false
        })
        await loadFieldData()
      } catch (error) {
        statusMessage.value = error.message || '灵田操作失败。'
      } finally {
        isSubmitting.value = false
      }
    }

    // 在指定地块播种当前选中的灵植。
    async function plantCrop(plot) {
      if (!selectedCrop.value) {
        return
      }

      await mutateField(async () => {
        const result = await apiClient.plantSpiritFieldCrop(plot.plotNumber, selectedCrop.value.templateId)
        if (!result) {
          throw new Error('播种失败。')
        }
      }, `已在灵田 ${plot.plotNumber} 播种 ${selectedCrop.value.name}。`)
    }

    // 收获指定地块。
    async function harvestCrop(plot) {
      await mutateField(async () => {
        const result = await apiClient.harvestSpiritFieldCrop(plot.plotNumber)
        if (!result) {
          throw new Error('收获失败。')
        }
      }, `灵田 ${plot.plotNumber} 收获成功。`)
    }

    // 使用当前选择的催熟材料，按数据库规则缩短作物生长时间。
    async function speedUpCrop(plot) {
      const speedUpItem = selectedSpeedItem.value
      if (!speedUpItem) {
        return
      }
      const durationText = formatDuration(speedUpItem.speedUpSeconds)

      await mutateField(async () => {
        const result = await apiClient.speedUpSpiritFieldCrop(plot.plotNumber, speedUpItem.itemId)
        if (!result) {
          throw new Error('催熟失败。')
        }
      }, `灵田 ${plot.plotNumber} 已催熟 ${durationText}。`)
    }

    // 升级指定地块。
    async function upgradePlot(plot) {
      await mutateField(async () => {
        const result = await apiClient.upgradeSpiritFieldPlot(plot.plotNumber)
        if (!result) {
          throw new Error('升级失败。')
        }
      }, `灵田 ${plot.plotNumber} 已提升至 Lv.${plot.level + 1}。`)
    }

    watch(speedItems, (items) => {
      if (!items.find((item) => item.itemId === selectedSpeedItemId.value)) {
        selectedSpeedItemId.value = items[0]?.itemId || ''
      }
    }, { immediate: true })

    watch(crops, (items) => {
      if (!items.find((item) => item.templateId === selectedCropId.value)) {
        selectedCropId.value = items[0]?.templateId || ''
      }
    }, { immediate: true })

    watch(() => props.modelValue, async (visible) => {
      if (!visible) {
        stopClock()
        return
      }

      startClock()
      await loadFieldData()
    })

    onMounted(() => {
      if (props.modelValue) startClock()
    })

    onBeforeUnmount(stopClock)

    return {
      ICON,
      fieldInfo,
      crops,
      selectedCropId,
      selectedCrop,
      selectedSpeedItemId,
      selectedSpeedUpDurationText,
      speedUpRuleText,
      speedItems,
      plotList,
      playerGold,
      playerSpiritStone,
      isLoading,
      isSubmitting,
      statusMessage,
      getInventoryCount,
      formatNumber,
      formatDuration,
      remainingText,
      canPlant,
      canSpeedUp,
      canUpgrade,
      loadFieldData,
      plantCrop,
      harvestCrop,
      speedUpCrop,
      upgradePlot
    }
  }
}
</script>

<style scoped>
.spirit-field-modal {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}

.summary-grid {
  display: grid;
  grid-template-columns: repeat(6, minmax(0, 1fr));
  gap: var(--spacing-sm);
}

.summary-card,
.panel-card,
.plot-card {
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
}

.summary-card {
  padding: var(--spacing-sm) var(--spacing-md);
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.summary-label,
.hint-text,
.crop-meta,
.selected-type,
.selected-meta,
.plot-meta,
.upgrade-cost,
.resource-bar {
  color: var(--text-secondary);
}

.summary-value {
  color: var(--highlight-text);
}

.resource-bar,
.plot-header,
.plot-status,
.action-row,
.crop-card-top,
.selected-header {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
}

.resource-bar {
  justify-content: space-between;
  flex-wrap: wrap;
  padding: var(--spacing-sm) var(--spacing-md);
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
}

.refresh-btn,
.primary-btn,
.secondary-btn,
.crop-card {
  border: 1px solid transparent;
  border-radius: var(--radius-sm);
  transition: all 0.25s ease;
}

.refresh-btn,
.primary-btn,
.secondary-btn {
  cursor: pointer;
  font-family: var(--font-primary);
}

.refresh-btn {
  padding: var(--spacing-sm) var(--spacing-md);
  background: var(--button-neutral-bg);
  border-color: var(--button-neutral-border);
  color: var(--button-neutral-text);
}

.refresh-btn:hover:not(:disabled) {
  background: var(--button-neutral-hover-bg);
  border-color: var(--button-outline-hover-border);
  color: var(--button-neutral-hover-text);
}

.refresh-btn:disabled,
.primary-btn:disabled,
.secondary-btn:disabled {
  opacity: 0.55;
  cursor: not-allowed;
}

.field-layout {
  display: grid;
  grid-template-columns: 320px minmax(0, 1fr);
  gap: var(--spacing-md);
  min-height: 540px;
}

.crop-panel,
.plot-panel {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
  overflow-y: auto;
  min-height: 0;
  max-height: 65vh;
  scrollbar-gutter: stable;
}

.panel-card {
  padding: var(--spacing-md);
}

.panel-title {
  color: var(--highlight-text);
  margin-bottom: var(--spacing-md);
  font-weight: 600;
}

.crop-list {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
  max-height: 260px;
  overflow-y: auto;
  scrollbar-gutter: stable;
}

.crop-card {
  min-height: 94px;
  padding: var(--spacing-sm) var(--spacing-md);
  text-align: left;
  background: rgba(255, 255, 255, 0.03);
  border-color: rgba(124, 58, 237, 0.18);
  color: var(--text-primary);
  cursor: pointer;
}

.crop-card:hover,
.crop-card.selected {
  border-color: var(--accent-text);
  background: rgba(124, 58, 237, 0.14);
  box-shadow: var(--shadow-sm);
}

.crop-name,
.selected-name,
.plot-name,
.plot-crop-name {
  color: var(--text-primary);
  font-weight: 600;
}

.selected-crop {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
}

.selected-meta,
.plot-meta {
  display: flex;
  flex-direction: column;
  gap: 4px;
  font-size: var(--font-size-sm);
}

.plot-remaining {
  min-height: 1.4em;
}

.plot-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: var(--spacing-sm);
}

.plot-card {
  padding: 6px;
  display: flex;
  flex-direction: column;
  gap: 3px;
}

.plot-card.growing {
  border-color: rgba(33, 150, 243, 0.35);
}

.plot-card.harvestable {
  border-color: rgba(76, 175, 80, 0.35);
  box-shadow: 0 0 14px rgba(76, 175, 80, 0.12);
}

.plot-card.locked {
  opacity: 0.75;
}

.plot-header {
  justify-content: space-between;
}

.plot-level {
  padding: 1px 5px;
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.06);
  color: var(--text-secondary);
  font-size: var(--font-size-sm);
}

.plot-status {
  color: var(--text-secondary);
  font-size: var(--font-size-sm);
}

.status-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: var(--accent-text);
  box-shadow: var(--shadow-sm);
}

.plot-body,
.locked-body {
  display: flex;
  flex-direction: column;
  gap: 3px;
  flex: 1;
}

.progress-bar {
  height: 5px;
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.08);
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  background: var(--button-primary-start);
}

.action-row {
  justify-content: space-between;
  min-height: 28px;
}

.primary-btn,
.secondary-btn {
  flex: 1;
  min-width: 0;
  min-height: 28px;
  padding: 4px var(--spacing-sm);
  white-space: nowrap;
}

.primary-btn {
  background: var(--button-primary-start);
  color: var(--button-primary-text);
}

.primary-btn:hover:not(:disabled) {
  background: var(--button-primary-start);
  opacity: 0.85;
  color: var(--button-primary-hover-text);
}

.secondary-btn {
  background: var(--button-neutral-bg);
  border-color: var(--button-neutral-border);
  color: var(--button-neutral-text);
}

.secondary-btn:hover:not(:disabled) {
  background: var(--button-neutral-hover-bg);
  border-color: var(--button-outline-hover-border);
  color: var(--button-neutral-hover-text);
}

.upgrade-cost {
  font-size: var(--font-size-sm);
}

.empty-card,
.locked-body {
  align-items: center;
  justify-content: center;
  min-height: 140px;
  color: var(--text-secondary);
}

.empty-icon {
  font-size: 26px;
}

.status-message {
  padding: var(--spacing-sm) var(--spacing-md);
  background: rgba(124, 58, 237, 0.12);
  border: 1px solid rgba(124, 58, 237, 0.28);
  border-radius: var(--radius-sm);
  color: var(--text-primary);
}

@media (max-width: 980px) {
  .summary-grid,
  .field-layout,
  .plot-grid {
    grid-template-columns: 1fr;
  }
}
</style>
