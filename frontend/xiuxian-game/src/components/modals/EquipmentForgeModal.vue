<template>
  <XiuXianModal :model-value="modelValue" title="装备锻造" :width="520" @update:model-value="$emit('update:modelValue', $event)">
    <div class="forge-modal">
      <!-- 装备信息头 -->
      <div v-if="equipment" class="forge-equip-header" :class="equipment.quality">
        <AssetIcon :source="equipment.icon || ICON.misc_dagger" size="28" />
        <div class="forge-equip-info">
          <span class="forge-equip-name" :class="equipment.quality">{{ equipment.name }}</span>
          <span class="forge-equip-type">{{ equipment.typeText }} · 强化 +{{ equipment.enhance || 0 }}</span>
        </div>
      </div>

      <!-- Tab 栏 -->
      <div class="forge-tabs">
        <button
          v-for="tab in tabs"
          :key="tab.key"
          class="forge-tab"
          :class="{ active: currentTab === tab.key }"
          @click="switchTab(tab.key)"
        >
          <AssetIcon :source="tab.icon" size="16" />
          <span>{{ tab.label }}</span>
        </button>
      </div>

      <!-- Tab 内容 -->
      <div class="forge-content">

        <!-- ========== 强化 Tab ========== -->
        <div v-if="currentTab === 'enhance'" class="forge-tab-content enhance-tab">
          <div class="enhance-display">
            <div class="enhance-level">
              <span class="enhance-label">当前强化等级</span>
              <span class="enhance-value" :class="{ max: (equipment?.enhance || 0) >= 15 }">
                +{{ equipment?.enhance || 0 }}
              </span>
            </div>
            <div v-if="(equipment?.enhance || 0) < 15" class="enhance-arrow">
              <span class="arrow-icon">→</span>
            </div>
            <div v-if="(equipment?.enhance || 0) < 15" class="enhance-level preview">
              <span class="enhance-label">强化后</span>
              <span class="enhance-value highlight">+{{ (equipment?.enhance || 0) + 1 }}</span>
            </div>
          </div>

          <div v-if="enhanceResult" class="enhance-result" :class="{ success: enhanceResult.success, fail: !enhanceResult.success }">
            {{ enhanceResult.message }}
          </div>

          <div class="enhance-actions">
            <button
              class="forge-btn enhance"
              :disabled="enhanceLoading || (equipment?.enhance || 0) >= 15"
              @click="handleEnhance"
            >
              {{ enhanceLoading ? '强化中...' : (equipment?.enhance || 0) >= 15 ? '已满级' : '强化' }}
            </button>
          </div>
          <div class="enhance-hint">
            消耗金币和强化石，强化失败不降级
          </div>
        </div>

        <!-- ========== 洗练 Tab ========== -->
        <div v-if="currentTab === 'reroll'" class="forge-tab-content reroll-tab">
          <div v-if="rerollLoading && !rerollPreview" class="forge-loading">加载中...</div>
          <template v-else-if="rerollPreview">
            <div class="reroll-columns">
              <div class="reroll-col">
                <div class="reroll-col-title">当前词条 <span class="reroll-hint">点击锁定</span></div>
                <div v-if="rerollPreview.currentStats?.length" class="reroll-stats-list">
                  <div
                    v-for="(stat, idx) in rerollPreview.currentStats"
                    :key="'cur-' + idx"
                    class="reroll-stat-item"
                    :class="{ locked: rerollLockedIndices.includes(idx) }"
                    :style="{ borderColor: stat.tierColor || '#9ca3af' }"
                    @click="toggleRerollLock(idx)"
                  >
                    <span class="reroll-lock"><AssetIcon :source="rerollLockedIndices.includes(idx) ? ICON.ui_lock : ICON.ui_unlock" size="14" /></span>
                    <span class="reroll-desc" :style="{ color: stat.tierColor }">{{ stat.description }}</span>
                    <span class="reroll-tier" :style="{ color: stat.tierColor }">{{ stat.tierName }}</span>
                    <span class="reroll-val">{{ stat.isPercentage ? stat.value + '%' : '+' + stat.value }}</span>
                  </div>
                </div>
                <div v-else class="reroll-empty">暂无词条</div>
              </div>
              <div class="reroll-col">
                <div class="reroll-col-title">候选结果 <span v-if="rerollPreview.candidateStats?.length" class="reroll-candidate-tag">未生效</span></div>
                <div v-if="rerollPreview.candidateStats?.length" class="reroll-stats-list">
                  <div
                    v-for="(stat, idx) in rerollPreview.candidateStats"
                    :key="'cand-' + idx"
                    class="reroll-stat-item candidate"
                    :style="{ borderColor: stat.tierColor || '#9ca3af' }"
                  >
                    <span class="reroll-desc" :style="{ color: stat.tierColor }">{{ stat.description }}</span>
                    <span class="reroll-tier" :style="{ color: stat.tierColor }">{{ stat.tierName }}</span>
                    <span class="reroll-val">{{ stat.isPercentage ? stat.value + '%' : '+' + stat.value }}</span>
                  </div>
                </div>
                <div v-else class="reroll-empty">洗练后显示</div>
              </div>
            </div>

            <div class="reroll-footer">
              <div class="reroll-cost">
                <span>{{ rerollPreview.cost?.gold || 0 }} 金币</span>
                <span>{{ rerollPreview.cost?.stoneCount || 0 }} {{ rerollPreview.cost?.materialName || '洗练材料' }}</span>
                <span v-if="rerollLockedIndices.length > 0" class="cost-locked">锁定 {{ rerollLockedIndices.length }} 条</span>
              </div>
              <div class="reroll-actions">
                <button v-if="rerollPreview.candidateStats?.length" class="forge-btn discard" @click="discardReroll">丢弃</button>
                <button v-if="rerollPreview.candidateStats?.length" class="forge-btn accept" @click="acceptReroll">接受</button>
                <button class="forge-btn roll" :disabled="rerollLoading" @click="executeReroll">
                  {{ rerollLoading ? '洗练中...' : '洗练' }}
                </button>
              </div>
            </div>
          </template>
          <div v-else class="forge-empty">暂无洗练数据</div>
        </div>

        <!-- ========== 镶嵌 Tab ========== -->
        <div v-if="currentTab === 'socket'" class="forge-tab-content socket-tab">
          <div v-if="!equipment?.hasGemSlots" class="forge-empty">该装备没有宝石孔位</div>
          <template v-else>
            <!-- 孔位展示 -->
            <div class="gem-slots-row">
              <div
                v-for="slot in gemPanelSlots"
                :key="'slot-' + slot.index"
                class="gem-slot-circle"
                :class="{ filled: slot.gemId, selected: selectedSlotIndex === slot.index }"
                @click="selectGemSlot(slot)"
              >
                <template v-if="slot.gemId">
                  <AssetIcon :source="ICON.misc_diamond" size="22" />
                  <span class="gem-slot-name">{{ slot.gemName || slot.gemId }}</span>
                  <span class="gem-slot-bonus">{{ slot.bonusText }}</span>
                  <button class="gem-unsocket-btn" @click.stop="unsocketGemFromSlot(slot.index)" title="取下宝石">×</button>
                </template>
                <template v-else>
                  <span class="gem-slot-plus">+</span>
                  <span class="gem-slot-label">空孔位</span>
                </template>
              </div>
            </div>

            <!-- 可用宝石 -->
            <div v-if="selectedSlotIndex >= 0 && !currentSlotGem" class="gem-available">
              <div class="gem-available-title">选择宝石镶嵌</div>
              <div v-if="gemPanelLoading" class="forge-loading">加载中...</div>
              <div v-else-if="availableGems.length === 0" class="forge-empty">背包中没有可用宝石</div>
              <div v-else class="gem-available-grid">
                <div
                  v-for="gem in availableGems"
                  :key="gem.inventoryItemId"
                  class="gem-available-item"
                  :class="gem.qualityClass"
                  @click="socketGemToSlot(gem)"
                >
                  <AssetIcon :source="ICON.misc_diamond" size="20" />
                  <span class="gem-item-name">{{ gem.name }}</span>
                  <span class="gem-item-attr">{{ gem.attrDisplay }}</span>
                  <span class="gem-item-qty">x{{ gem.quantity }}</span>
                </div>
              </div>
            </div>
          </template>
        </div>
      </div>
    </div>
  </XiuXianModal>
</template>

<script>
import { ref, computed, watch } from 'vue'
import XiuXianModal from '../common/XiuXianModal.vue'
import AssetIcon from '../common/AssetIcon.vue'
import { ICON } from '../../icons'
import { apiClient } from '../../lib/apiClient.js'
import { useGameStore } from '../../state/gameStore'
import toast from '@/utils/toast'

const ATTR_TYPE_MAP = {
  Type1: '生命', Type2: '法力', Type3: '物攻', Type4: '法攻',
  Type5: '物防', Type6: '法防', Type7: '速度', Type8: '命中',
  Type9: '闪避', Type10: '暴击', Type11: '暴伤', Type12: '连击',
  Type13: '反击', Type14: '破甲', Type15: '增伤'
}
const GEM_QUALITY_MAP = { 1: 'common', 2: 'uncommon', 3: 'rare', 4: 'epic', 5: 'legendary' }
function attrTypeLabel(type) { return ATTR_TYPE_MAP[type] || type }

export default {
  name: 'EquipmentForgeModal',
  components: { XiuXianModal, AssetIcon },
  props: {
    modelValue: { type: Boolean, default: false },
    equipment: { type: Object, default: null }
  },
  emits: ['update:modelValue'],
  setup(props) {
    const gameStore = useGameStore()

    const tabs = [
      { key: 'enhance', label: '强化', icon: ICON.ui_star || ICON.misc_dagger },
      { key: 'reroll', label: '洗练', icon: ICON.ui_refresh || ICON.misc_dagger },
      { key: 'socket', label: '镶嵌', icon: ICON.misc_diamond }
    ]
    const currentTab = ref('enhance')

    // ---- 强化 ----
    const enhanceLoading = ref(false)
    const enhanceResult = ref(null)

    async function handleEnhance() {
      if (!props.equipment?.instanceId) return
      enhanceLoading.value = true
      enhanceResult.value = null
      try {
        const result = await gameStore.enhanceEquipment(props.equipment.instanceId)
        const success = Boolean(result?.success ?? result?.Success)
        const level = result?.newEnhanceLevel ?? result?.NewEnhanceLevel ?? props.equipment.enhance
        const message = result?.message || result?.Message || (success ? '强化成功' : '强化失败')
        enhanceResult.value = { success, message: `${message}，当前 +${level}` }
        if (success) {
          toast.success(enhanceResult.value.message)
          props.equipment.enhance = level
        } else {
          toast.error(enhanceResult.value.message)
        }
      } catch (e) {
        enhanceResult.value = { success: false, message: e.message || '强化失败' }
        toast.error(e.message || '强化失败')
      } finally {
        enhanceLoading.value = false
      }
    }

    // ---- 洗练 ----
    const rerollPreview = ref(null)
    const rerollLockedIndices = ref([])
    const rerollLoading = ref(false)

    async function loadRerollPreview() {
      if (!props.equipment?.instanceId) return
      rerollLoading.value = true
      try {
        const preview = await gameStore.getEquipmentRerollPreview(props.equipment.instanceId)
        rerollPreview.value = preview
        rerollLockedIndices.value = []
      } catch (e) {
        toast.error(e.message || '获取洗练预览失败')
      } finally {
        rerollLoading.value = false
      }
    }

    function toggleRerollLock(index) {
      const idx = rerollLockedIndices.value.indexOf(index)
      if (idx >= 0) {
        rerollLockedIndices.value.splice(idx, 1)
      } else {
        const max = rerollPreview.value?.maxLockedLineCount || 3
        if (rerollLockedIndices.value.length >= max) {
          toast.warning(`最多锁定 ${max} 条词条`)
          return
        }
        rerollLockedIndices.value.push(index)
      }
    }

    async function executeReroll() {
      if (!props.equipment?.instanceId) return
      rerollLoading.value = true
      try {
        const result = await gameStore.rollEquipmentReroll(props.equipment.instanceId, rerollLockedIndices.value)
        await loadRerollPreview()
        toast.success(result?.message || result?.Message || '洗练完成')
      } catch (e) {
        toast.error(e.message || '洗练失败')
      } finally {
        rerollLoading.value = false
      }
    }

    async function acceptReroll() {
      if (!props.equipment?.instanceId) return
      try {
        const result = await gameStore.acceptEquipmentReroll(props.equipment.instanceId)
        rerollPreview.value = null
        toast.success(result?.message || result?.Message || '已接受洗练结果')
        await loadRerollPreview()
      } catch (e) {
        toast.error(e.message || '接受失败')
      }
    }

    async function discardReroll() {
      if (!props.equipment?.instanceId) return
      try {
        const result = await gameStore.discardEquipmentReroll(props.equipment.instanceId)
        await loadRerollPreview()
        toast.success(result?.message || result?.Message || '已丢弃')
      } catch (e) {
        toast.error(e.message || '丢弃失败')
      }
    }

    // ---- 镶嵌 ----
    const gemPanelLoading = ref(false)
    const gemInventory = ref([])
    const gemTemplates = ref([])
    const selectedSlotIndex = ref(-1)

    const gemPanelSlots = computed(() => {
      const item = props.equipment
      if (!item) return []
      const max = item.maxGemSlots || 0
      const filled = item.gemSlots || []
      const slots = []
      for (let i = 0; i < max; i++) {
        const found = filled.find(s => s.slotIndex === i)
        slots.push(found
          ? { index: i, gemId: found.gemId, gemName: found.gemName, bonusText: found.bonusText }
          : { index: i, gemId: null, gemName: null, bonusText: null }
        )
      }
      return slots
    })

    const currentSlotGem = computed(() => {
      if (selectedSlotIndex.value < 0) return null
      return gemPanelSlots.value.find(s => s.index === selectedSlotIndex.value && s.gemId) || null
    })

    const availableGems = computed(() => {
      return gemInventory.value
        .filter(g => (g.quantity ?? g.Quantity ?? 0) > 0)
        .map(g => ({
          inventoryItemId: g.inventoryItemId ?? g.InventoryItemId,
          gemId: g.gemId ?? g.GemId,
          name: g.name ?? g.Name ?? '宝石',
          quantity: g.quantity ?? g.Quantity ?? 0,
          attrDisplay: attrTypeLabel(g.attributeType || g.AttributeType || '') +
            ' +' + (g.bonusValue ?? g.BonusValue ?? 0) +
            ((g.bonusMode || g.BonusMode) === 'Percent' ? '%' : ''),
          qualityClass: GEM_QUALITY_MAP[g.quality ?? g.Quality] || 'common'
        }))
    })

    async function loadGemData() {
      gemPanelLoading.value = true
      try {
        const [templates, inventory] = await Promise.all([
          apiClient.getGemTemplates(),
          apiClient.getGemInventory()
        ])
        gemTemplates.value = templates || []
        gemInventory.value = inventory || []
      } catch (e) {
        toast.error(e.message || '加载宝石数据失败')
      } finally {
        gemPanelLoading.value = false
      }
    }

    function selectGemSlot(slot) {
      selectedSlotIndex.value = slot.index
    }

    async function socketGemToSlot(gem) {
      if (!props.equipment?.instanceId || selectedSlotIndex.value < 0) return
      gemPanelLoading.value = true
      try {
        await apiClient.socketGem(props.equipment.instanceId, selectedSlotIndex.value, gem.inventoryItemId)
        toast.success(`已镶嵌 ${gem.name}`)
        await refreshAfterGemOp()
        selectedSlotIndex.value = -1
      } catch (e) {
        toast.error(e.message || '镶嵌失败')
      } finally {
        gemPanelLoading.value = false
      }
    }

    async function unsocketGemFromSlot(slotIndex) {
      if (!props.equipment?.instanceId) return
      gemPanelLoading.value = true
      try {
        await apiClient.unsocketGem(props.equipment.instanceId, slotIndex)
        toast.success('已取下宝石')
        await refreshAfterGemOp()
      } catch (e) {
        toast.error(e.message || '取下失败')
      } finally {
        gemPanelLoading.value = false
      }
    }

    async function refreshAfterGemOp() {
      await Promise.all([
        gameStore.loadInventory(true),
        gameStore.loadEquipment(true),
        loadGemData()
      ])
      // 刷新 equipment prop 的宝石数据
      const freshList = gameStore.state.equipments || []
      const fresh = freshList.find(e => (e.InstanceId || e.instanceId) === props.equipment?.instanceId)
      if (fresh && props.equipment) {
        const slots = fresh.GemSlots || fresh.gemSlots || []
        props.equipment.gemSlots = slots.map(s => ({
          slotIndex: s.SlotIndex ?? s.slotIndex ?? 0,
          gemId: s.GemId || s.gemId || null,
          gemName: s.GemName || s.gemName || null,
          bonusText: s.BonusText || s.bonusText || null
        }))
      }
    }

    // Tab 切换时按需加载
    function switchTab(key) {
      currentTab.value = key
      if (key === 'reroll' && !rerollPreview.value) loadRerollPreview()
      if (key === 'socket' && gemInventory.value.length === 0) loadGemData()
    }

    // 打开时重置状态
    watch(() => props.modelValue, (val) => {
      if (val) {
        currentTab.value = 'enhance'
        enhanceResult.value = null
        rerollPreview.value = null
        rerollLockedIndices.value = []
        selectedSlotIndex.value = -1
        gemInventory.value = []
      }
    })

    return {
      ICON, tabs, currentTab, switchTab,
      // 强化
      enhanceLoading, enhanceResult, handleEnhance,
      // 洗练
      rerollPreview, rerollLockedIndices, rerollLoading,
      toggleRerollLock, executeReroll, acceptReroll, discardReroll,
      // 镶嵌
      gemPanelLoading, gemInventory, gemTemplates, selectedSlotIndex,
      gemPanelSlots, currentSlotGem, availableGems,
      selectGemSlot, socketGemToSlot, unsocketGemFromSlot
    }
  }
}
</script>

<style scoped>
.forge-modal {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}

/* 装备信息头 */
.forge-equip-header {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  padding: var(--spacing-md);
  background: var(--xiuxian-bg-secondary);
  border-radius: var(--radius-md);
  border-left: 3px solid var(--border-color);
}
.forge-equip-header.common { border-left-color: var(--quality-common); }
.forge-equip-header.uncommon { border-left-color: var(--quality-uncommon); }
.forge-equip-header.rare { border-left-color: var(--quality-rare); }
.forge-equip-header.epic { border-left-color: var(--quality-epic); }
.forge-equip-header.legendary { border-left-color: var(--quality-legendary); }

.forge-equip-info {
  display: flex;
  flex-direction: column;
  gap: 2px;
}
.forge-equip-name {
  font-weight: 700;
  font-size: var(--font-size-base);
}
.forge-equip-name.common { color: var(--quality-common); }
.forge-equip-name.uncommon { color: var(--quality-uncommon); }
.forge-equip-name.rare { color: var(--quality-rare); }
.forge-equip-name.epic { color: var(--quality-epic); }
.forge-equip-name.legendary { color: var(--quality-legendary); }
.forge-equip-type {
  font-size: var(--font-size-xs);
  color: var(--text-muted);
}

/* Tab 栏 */
.forge-tabs {
  display: flex;
  gap: 2px;
  background: var(--xiuxian-bg-secondary);
  border-radius: var(--radius-md);
  padding: 3px;
}
.forge-tab {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  padding: 8px 0;
  border: none;
  border-radius: var(--radius-sm);
  background: transparent;
  color: var(--text-secondary);
  font-size: var(--font-size-sm);
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
}
.forge-tab:hover {
  color: var(--text-primary);
  background: var(--control-bg-hover);
}
.forge-tab.active {
  color: var(--accent-text, #a78bfa);
  background: var(--xiuxian-bg-primary);
  box-shadow: 0 1px 4px rgba(0,0,0,0.12);
  font-weight: 700;
}

/* Tab 内容区 */
.forge-content {
  min-height: 200px;
}
.forge-tab-content {
  animation: fadeIn 0.2s ease;
}
@keyframes fadeIn {
  from { opacity: 0; transform: translateY(4px); }
  to { opacity: 1; transform: translateY(0); }
}

.forge-loading,
.forge-empty {
  text-align: center;
  padding: var(--spacing-xl);
  color: var(--text-muted);
  font-size: var(--font-size-sm);
}

/* ========== 强化 Tab ========== */
.enhance-tab {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-lg);
  align-items: center;
  padding: var(--spacing-md) 0;
}
.enhance-display {
  display: flex;
  align-items: center;
  gap: var(--spacing-lg);
}
.enhance-level {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
}
.enhance-label {
  font-size: var(--font-size-xs);
  color: var(--text-muted);
}
.enhance-value {
  font-size: 32px;
  font-weight: 800;
  color: var(--highlight-text, #fbbf24);
  font-family: var(--font-mono);
}
.enhance-value.max {
  color: var(--quality-legendary, #f59e0b);
}
.enhance-value.highlight {
  color: var(--quality-uncommon, #22c55e);
}
.enhance-arrow {
  font-size: 24px;
  color: var(--text-muted);
}
.enhance-result {
  text-align: center;
  font-size: var(--font-size-sm);
  padding: var(--spacing-sm) var(--spacing-md);
  border-radius: var(--radius-sm);
}
.enhance-result.success {
  color: var(--quality-uncommon);
  background: rgba(34, 197, 94, 0.1);
}
.enhance-result.fail {
  color: var(--status-danger-text, #ef4444);
  background: rgba(239, 68, 68, 0.1);
}
.enhance-actions {
  display: flex;
  justify-content: center;
}
.enhance-hint {
  font-size: var(--font-size-xs);
  color: var(--text-muted);
  text-align: center;
}

/* ========== 洗练 Tab ========== */
.reroll-tab {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}
.reroll-columns {
  display: flex;
  gap: var(--spacing-md);
}
.reroll-col {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: var(--spacing-xs);
}
.reroll-col-title {
  font-size: var(--font-size-sm);
  font-weight: 600;
  color: var(--text-secondary);
  display: flex;
  align-items: center;
  gap: 6px;
}
.reroll-hint {
  font-size: var(--font-size-xs);
  color: var(--text-muted);
  font-weight: 400;
}
.reroll-candidate-tag {
  font-size: var(--font-size-xs);
  color: var(--status-warning-text, #f59e0b);
  font-weight: 400;
}
.reroll-stats-list {
  display: flex;
  flex-direction: column;
  gap: 4px;
}
.reroll-stat-item {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 6px 8px;
  border-radius: var(--radius-sm);
  border-left: 3px solid var(--border-color);
  background: var(--xiuxian-bg-secondary);
  cursor: pointer;
  transition: all 0.15s ease;
  font-size: var(--font-size-xs);
}
.reroll-stat-item:hover {
  background: var(--control-bg-hover);
}
.reroll-stat-item.locked {
  background: rgba(139, 92, 246, 0.08);
}
.reroll-stat-item.candidate {
  cursor: default;
  border-style: dashed;
}
.reroll-lock {
  flex-shrink: 0;
  display: flex;
}
.reroll-desc {
  flex: 1;
  font-weight: 500;
}
.reroll-tier {
  font-size: 10px;
  font-weight: 600;
}
.reroll-val {
  font-family: var(--font-mono);
  font-weight: 600;
  color: var(--text-primary);
}
.reroll-empty {
  text-align: center;
  padding: var(--spacing-md);
  color: var(--text-muted);
  font-size: var(--font-size-xs);
}
.reroll-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding-top: var(--spacing-sm);
  border-top: 1px solid var(--border-color);
}
.reroll-cost {
  display: flex;
  gap: var(--spacing-sm);
  font-size: var(--font-size-xs);
  color: var(--text-muted);
}
.cost-locked {
  color: var(--accent-text, #a78bfa);
}
.reroll-actions {
  display: flex;
  gap: var(--spacing-xs);
}

/* ========== 镶嵌 Tab ========== */
.socket-tab {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}
.gem-slots-row {
  display: flex;
  gap: var(--spacing-md);
  justify-content: center;
  flex-wrap: wrap;
}
.gem-slot-circle {
  width: 100px;
  min-height: 110px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 4px;
  border: 2px dashed var(--border-color);
  border-radius: var(--radius-lg);
  background: var(--xiuxian-bg-secondary);
  cursor: pointer;
  transition: all 0.2s ease;
  position: relative;
  padding: var(--spacing-sm);
}
.gem-slot-circle:hover {
  border-color: var(--accent-text, #a78bfa);
  background: var(--accent-soft-bg, rgba(139, 92, 246, 0.08));
}
.gem-slot-circle.selected {
  border-color: var(--accent-text, #a78bfa);
  border-style: solid;
  box-shadow: 0 0 12px rgba(139, 92, 246, 0.3);
}
.gem-slot-circle.filled {
  border-style: solid;
  border-color: var(--quality-uncommon, #22c55e);
  background: linear-gradient(135deg, rgba(34, 197, 94, 0.06), rgba(139, 92, 246, 0.06));
}
.gem-slot-circle.filled.selected {
  border-color: var(--accent-text, #a78bfa);
}
.gem-slot-plus {
  font-size: 28px;
  color: var(--text-muted);
  font-weight: 300;
}
.gem-slot-label {
  font-size: var(--font-size-xs);
  color: var(--text-muted);
}
.gem-slot-name {
  font-size: var(--font-size-sm);
  font-weight: 600;
  color: var(--highlight-text, #fbbf24);
  text-align: center;
  line-height: 1.2;
}
.gem-slot-bonus {
  font-size: var(--font-size-xs);
  color: var(--quality-uncommon, #22c55e);
  text-align: center;
}
.gem-unsocket-btn {
  position: absolute;
  top: 4px;
  right: 4px;
  width: 20px;
  height: 20px;
  border-radius: 50%;
  border: 1px solid var(--border-color);
  background: var(--xiuxian-bg-primary);
  color: var(--status-danger-text, #ef4444);
  font-size: 14px;
  line-height: 1;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.15s ease;
}
.gem-unsocket-btn:hover {
  background: var(--status-danger-text, #ef4444);
  color: #fff;
  border-color: var(--status-danger-text, #ef4444);
}

/* 可用宝石列表 */
.gem-available {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
}
.gem-available-title {
  font-size: var(--font-size-sm);
  color: var(--text-secondary);
  font-weight: 600;
}
.gem-available-grid {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-xs);
  max-height: 180px;
  overflow-y: auto;
}
.gem-available-item {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  padding: var(--spacing-sm) var(--spacing-md);
  border-radius: var(--radius-md);
  border: 1px solid var(--border-color);
  background: var(--xiuxian-bg-primary);
  cursor: pointer;
  transition: all 0.15s ease;
}
.gem-available-item:hover {
  border-color: var(--accent-text, #a78bfa);
  background: var(--accent-soft-bg, rgba(139, 92, 246, 0.08));
}
.gem-available-item.common { border-left: 3px solid var(--quality-common); }
.gem-available-item.uncommon { border-left: 3px solid var(--quality-uncommon); }
.gem-available-item.rare { border-left: 3px solid var(--quality-rare); }
.gem-available-item.epic { border-left: 3px solid var(--quality-epic); }
.gem-available-item.legendary { border-left: 3px solid var(--quality-legendary); }
.gem-item-name {
  font-size: var(--font-size-sm);
  font-weight: 600;
  color: var(--text-primary);
  flex-shrink: 0;
}
.gem-item-attr {
  font-size: var(--font-size-xs);
  color: var(--quality-uncommon, #22c55e);
  flex: 1;
}
.gem-item-qty {
  font-size: var(--font-size-xs);
  color: var(--text-muted);
  font-family: var(--font-mono);
}

/* 通用按钮 */
.forge-btn {
  padding: 6px 18px;
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  font-size: var(--font-size-sm);
  font-weight: 600;
  cursor: pointer;
  transition: all 0.15s ease;
}
.forge-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
.forge-btn.enhance {
  background: var(--button-primary-start);
  color: var(--button-primary-text);
  border-color: transparent;
}
.forge-btn.enhance:hover:not(:disabled) {
  background: var(--button-primary-hover-start);
  color: var(--button-primary-hover-text);
}
.forge-btn.roll {
  background: var(--button-secondary-start);
  color: var(--button-secondary-text);
  border-color: var(--border-color);
}
.forge-btn.roll:hover:not(:disabled) {
  background: var(--button-secondary-hover-start);
  color: var(--button-secondary-hover-text);
}
.forge-btn.accept {
  background: var(--status-success-bg);
  color: var(--status-success-text);
  border-color: var(--status-success-text);
}
.forge-btn.accept:hover:not(:disabled) {
  background: var(--status-success-text);
  color: var(--xiuxian-bg-panel);
}
.forge-btn.discard {
  background: var(--status-danger-bg);
  color: var(--status-danger-text);
  border-color: var(--status-danger-text);
}
.forge-btn.discard:hover:not(:disabled) {
  background: var(--status-danger-text);
  color: var(--text-on-dark);
}
</style>
