<template>
  <!-- 背包装备弹窗 -->
  <XiuXianModal :model-value="modelValue" @update:model-value="$emit('update:modelValue', $event)" title="背包装备" :width="1000">
    <div class="inventory-modal">
      <!-- 背包类型切换 -->
      <div class="inventory-tabs">
        <button
          v-for="tab in inventoryTabs"
          :key="tab.key"
          class="tab-btn"
          :class="{ active: currentTab === tab.key }"
          @click="currentTab = tab.key"
        >
          <span class="tab-icon"><AssetIcon :source="tab.icon" size="16" /></span>
          <span class="tab-name">{{ tab.name }}</span>
          <span class="tab-count">{{ getItemCount(tab.key) }}</span>
        </button>
      </div>

      <!-- 筛选栏 -->
      <div v-if="currentTab === 'equipment' || currentTab === 'props' || currentTab === 'gems'" class="filter-bar">
        <div class="filter-group">
          <label>品级:</label>
          <select v-model="filters.quality">
            <option value="">全部</option>
            <option value="common">普通</option>
            <option value="uncommon">优秀</option>
            <option value="rare">稀有</option>
            <option value="epic">史诗</option>
            <option value="legendary">传说</option>
            <option value="mythic">神话</option>
          </select>
        </div>

        <div v-if="currentTab === 'equipment'" class="filter-group">
          <label>等级:</label>
          <select v-model="filters.level">
            <option value="">全部</option>
            <option v-for="level in equipmentLevels" :key="level" :value="String(level)">
              Lv.{{ level }}
            </option>
          </select>
        </div>

        <div v-if="currentTab === 'equipment'" class="filter-group">
          <label>部位:</label>
          <select v-model="filters.slot">
            <option value="">全部</option>
            <option v-for="slot in equipmentSlotOptions" :key="slot.key" :value="slot.key">
              {{ slot.name }}
            </option>
          </select>
        </div>

        <div v-if="currentTab === 'equipment'" class="filter-group">
          <label>五行:</label>
          <select v-model="filters.element">
            <option value="">全部</option>
            <option value="metal">金</option>
            <option value="wood">木</option>
            <option value="water">水</option>
            <option value="fire">火</option>
            <option value="earth">土</option>
          </select>
        </div>

        <div class="filter-group search">
          <input v-model="filters.search" type="text" placeholder="搜索物品名称..." />
        </div>

        <button class="reset-btn" @click="resetFilters">重置</button>
      </div>

      <!-- 批量操作栏 -->
      <div v-if="currentTab === 'equipment' || currentTab === 'props'" class="batch-toolbar">
        <button class="batch-toggle-btn" :class="{ active: batchMode }" @click="toggleBatchMode">
          {{ batchMode ? '退出批量' : '批量操作' }}
        </button>
        <template v-if="batchMode">
          <span class="batch-count">已选 {{ batchSelected.size }} 件</span>
          <button class="batch-select-btn" @click="selectAllBatch">全选</button>
          <button class="batch-select-btn" @click="invertBatchSelection">反选</button>
          <button class="batch-action-btn sell" :disabled="batchSelected.size === 0" @click="batchSell">
            批量出售
          </button>
          <button v-if="currentTab === 'equipment'" class="batch-action-btn decompose" :disabled="batchSelected.size === 0" @click="batchDecompose">
            批量分解
          </button>
          <button v-if="currentTab === 'props'" class="batch-action-btn drop" :disabled="batchSelected.size === 0" @click="batchDiscard">
            批量丢弃
          </button>
          <button class="batch-action-btn clear" :disabled="batchSelected.size === 0" @click="batchSelected.clear()">
            清空选择
          </button>
        </template>
      </div>

      <!-- 背包内容 -->
      <div ref="inventoryContentRef" class="inventory-content" @scroll="handleInventoryScroll">
        <!-- 装备背包 -->
        <div v-if="currentTab === 'equipment'" class="equipment-list">
          <div
            v-for="item in filteredEquipment"
            :key="item.id"
            class="equipment-row"
            :class="[item.quality, { selected: itemPopover.visible && selectedItem?.id === item.id && itemPopover.kind === 'equipment', 'batch-selected': batchSelected.has(item.instanceId), 'batch-locked': batchMode && !canBatchSelect(item), equipped: item.isEquipped }]"
            @click.stop="handleEquipmentClick(item, $event)"
            @mouseenter="handleItemMouseEnter(item, $event, 'equipment')"
            @mouseleave="schedulePopoverClose"
          >
            <div v-if="batchMode" class="batch-checkbox" :class="{ checked: batchSelected.has(item.instanceId), disabled: !canBatchSelect(item) }">
              <span v-if="batchSelected.has(item.instanceId)">✓</span>
            </div>
            <div class="equipment-row-info">
              <div class="equipment-row-name">
                {{ item.name }}
                <span v-if="item.isEquipped" class="equipped-tag">已穿戴</span>
                <span v-if="item.isLocked" class="bound-tag"><AssetIcon :source="ICON.ui_lock" size="14" /></span>
              </div>
              <div class="equipment-row-type">{{ item.typeText }}</div>
            </div>
            <div class="equipment-row-badges">
              <span class="badge-level">Lv.{{ item.level || 1 }}</span>
              <span v-if="item.enhance > 0" class="badge-enhance">+{{ item.enhance }}</span>
            </div>
          </div>

          <!-- 空行占位 -->
          <div
            v-for="n in Math.max(0, 8 - equipmentList.length)"
            :key="'empty-'+n"
            class="equipment-row empty"
          >
            <span class="empty-icon">+</span>
          </div>
        </div>

        <!-- 道具背包 -->
        <div v-else-if="currentTab === 'props'" class="props-grid">
          <div
            v-for="item in filteredProps"
            :key="item.id"
            class="equipment-row prop-item"
            :class="[item.quality, { selected: itemPopover.visible && selectedItem?.id === item.id && itemPopover.kind === 'props', 'batch-selected': batchSelected.has(item.id), 'batch-locked': batchMode && !canBatchSelect(item) }]"
            @click.stop="handlePropsClick(item, $event)"
            @mouseenter="handleItemMouseEnter(item, $event, 'props')"
            @mouseleave="schedulePopoverClose"
          >
            <div v-if="batchMode" class="batch-checkbox" :class="{ checked: batchSelected.has(item.id), disabled: !canBatchSelect(item) }">
              <span v-if="batchSelected.has(item.id)">✓</span>
            </div>
            <div class="equipment-row-info">
              <div class="equipment-row-name">
                {{ item.name }}
                <span v-if="item.isLocked" class="bound-tag"><AssetIcon :source="ICON.ui_lock" size="14" /></span>
              </div>
              <div class="equipment-row-type">{{ item.type }}</div>
            </div>
            <div class="equipment-row-badges">
              <span class="badge-level">x{{ item.count }}</span>
            </div>
          </div>

          <div
            v-for="n in propsMax - filteredProps.length"
            :key="'empty-'+n"
            class="equipment-row prop-item empty"
          >
            <span class="empty-icon">+</span>
          </div>
        </div>

        <!-- 宝石背包 -->
        <div v-else-if="currentTab === 'gems'" class="props-grid">
          <div
            v-for="item in filteredGems"
            :key="item.id"
            class="equipment-row prop-item"
            :class="[item.quality, { selected: itemPopover.visible && selectedItem?.id === item.id && itemPopover.kind === 'gems' }]"
            @click.stop="handleGemsClick(item, $event)"
            @mouseenter="handleItemMouseEnter(item, $event, 'gems')"
            @mouseleave="schedulePopoverClose"
          >
            <div class="equipment-row-info">
              <div class="equipment-row-name">
                {{ item.name }}
              </div>
              <div class="equipment-row-type">{{ item.description }}</div>
            </div>
            <div class="equipment-row-badges">
              <span class="badge-level">x{{ item.count }}</span>
            </div>
          </div>

          <div
            v-for="n in Math.max(0, 12 - filteredGems.length)"
            :key="'empty-'+n"
            class="equipment-row prop-item empty"
          >
            <span class="empty-icon">+</span>
          </div>
        </div>

        <!-- 文字图鉴 -->
        <div v-if="currentTab === 'textCollection'" class="collection-panel">
          <div v-if="collectionBonuses.length" class="collection-bonuses">
            <h4>属性加成</h4>
            <div class="bonus-list">
              <span v-for="b in collectionBonuses" :key="b.attrType" class="bonus-tag">
                {{ attrTypeLabel(b.attrType) }}: <template v-if="b.fixedValue">+{{ b.fixedValue }} </template><template v-if="b.percentValue">+{{ b.percentValue }}%</template>
              </span>
            </div>
          </div>
          <div v-for="series in textCollections" :key="series.seriesId" class="collection-series">
            <div class="series-header">
              <span class="series-name">{{ series.name }}</span>
              <span class="series-progress">{{ series.ownedCount }}/{{ series.totalCount }}</span>
              <span v-if="series.isComplete" class="complete-badge">集齐</span>
              <span v-for="b in series.bonuses" :key="b.attrType" class="series-bonus-tag" :class="{ active: series.isComplete }">
                {{ attrTypeLabel(b.attrType) }} {{ b.valueType === 1 ? '+' + b.attrValue + '%' : '+' + b.attrValue }}
              </span>
            </div>
            <div class="collection-grid">
              <div v-for="item in series.items" :key="item.itemId" class="collection-slot" :class="{ owned: item.isOwned }">
                <span class="slot-char">{{ item.displayName }}</span>
                <span v-if="item.ownedCount > 1" class="slot-count">x{{ item.ownedCount }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- 图片图鉴 -->
        <div v-if="currentTab === 'imageCollection'" class="collection-panel">
          <div v-if="collectionBonuses.length" class="collection-bonuses">
            <h4>属性加成</h4>
            <div class="bonus-list">
              <span v-for="b in collectionBonuses" :key="b.attrType" class="bonus-tag">
                {{ attrTypeLabel(b.attrType) }}: <template v-if="b.fixedValue">+{{ b.fixedValue }} </template><template v-if="b.percentValue">+{{ b.percentValue }}%</template>
              </span>
            </div>
          </div>
          <div v-for="series in imageCollections" :key="series.seriesId" class="collection-series">
            <div class="series-header">
              <span class="series-name">{{ series.name }}</span>
              <span class="series-progress">{{ series.ownedCount }}/{{ series.totalCount }}</span>
              <span v-if="series.isComplete" class="complete-badge">集齐</span>
              <span v-for="b in series.bonuses" :key="b.attrType" class="series-bonus-tag" :class="{ active: series.isComplete }">
                {{ attrTypeLabel(b.attrType) }} {{ b.valueType === 1 ? '+' + b.attrValue + '%' : '+' + b.attrValue }}
              </span>
            </div>
            <div class="collection-grid">
              <div v-for="item in series.items" :key="item.itemId" class="collection-slot" :class="{ owned: item.isOwned }">
                <img v-if="item.thumbUrl && item.isOwned" :src="imgUrl(item.thumbUrl)" :alt="item.displayName" class="slot-thumb" />
                <span v-else class="slot-char">{{ item.isOwned ? item.displayName : '?' }}</span>
                <span v-if="item.ownedCount > 1" class="slot-count">x{{ item.ownedCount }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <Teleport to="body">
        <div
          v-if="itemPopover.visible && selectedItem"
          ref="itemPopoverRef"
          class="inventory-popover-teleport"
          :class="[
            itemPopover.kind,
            selectedItem.quality,
            {
              'with-compare': showEquipmentCompare
            }
          ]"
          :style="{ left: itemPopoverPosition.x + 'px', top: itemPopoverPosition.y + 'px' }"
          @mouseenter="cancelPopoverClose"
          @mouseleave="schedulePopoverClose"
        >
          <div class="popover-header">
            <div class="popover-identity">
              <div class="popover-icon">
                <AssetIcon :source="selectedItem.icon" :fallback="itemPopover.kind === 'equipment' ? ICON.misc_dagger : ICON.misc_backpack" :alt="selectedItem.name" size="25" />
              </div>
              <div class="popover-meta">
                <div class="popover-name" :class="selectedItem.quality">{{ selectedItem.name }}</div>
                <div class="popover-type">
                  {{ itemPopover.kind === 'equipment' ? selectedItem.typeText : selectedItem.type }}
                  <span v-if="selectedItem.element" :class="selectedItem.element">
                    [{{ getElementName(selectedItem.element) }}]
                  </span>
                </div>
                <div class="popover-tags">
                  <span v-if="itemPopover.kind === 'equipment'" class="popover-tag">等级 Lv.{{ selectedItem.level || 1 }}</span>
                  <span v-if="itemPopover.kind === 'equipment'" class="popover-tag">强化 +{{ selectedItem.enhance || 0 }}</span>
                  <span v-if="itemPopover.kind === 'props'" class="popover-tag">数量 x{{ selectedItem.count || 0 }}</span>
                  <span v-if="selectedItem.isEquipped" class="popover-tag highlight">已穿戴</span>
                  <span v-if="selectedItem.isLocked" class="popover-tag warning">已锁定</span>
                </div>
              </div>
            </div>
            <button class="popover-close" type="button" @click.stop="hideItemPopover">×</button>
          </div>

          <div v-if="selectedItem.stats?.length" class="popover-stats">
            <div v-for="stat in selectedItem.stats" :key="stat.name || stat.label" class="popover-stat-line" :class="{ 'has-reroll': stat.tierName }">
              <span>{{ stat.label || stat.name }}:</span>
              <span class="popover-stat-right">
                <span v-if="stat.tierName" class="popover-stat-tier" :style="{ color: stat.tierColor }">{{ stat.tierName }}</span>
                <span :class="getStatValueClass(stat.value)">{{ stat.displayValue || formatStatValue(stat.value) }}</span>
              </span>
            </div>
          </div>

          <div v-if="selectedItem.hasRerollStats" class="popover-reroll-stats">
            <div class="popover-reroll-title">洗练词条</div>
            <div
              v-for="stat in selectedItem.rerollStats"
              :key="'reroll-' + stat.index"
              class="popover-reroll-line"
              :style="{ borderLeftColor: stat.tierColor }"
            >
              <span class="popover-reroll-name" :style="{ color: stat.tierColor }">
                {{ stat.description }}
              </span>
              <span class="popover-reroll-tier" :style="{ color: stat.tierColor }">
                {{ stat.tierName }}
              </span>
              <span class="popover-reroll-value">
                {{ stat.isPercentage ? stat.value + '%' : '+' + stat.value }}
              </span>
            </div>
          </div>

          <div v-if="selectedItem.gemSlots?.some(s => s.gemId)" class="popover-gem-slots">
            <div class="popover-gem-title">宝石镶嵌</div>
            <div
              v-for="slot in selectedItem.gemSlots.filter(s => s.gemId)"
              :key="'gem-' + slot.slotIndex"
              class="popover-gem-line"
            >
              <AssetIcon :source="ICON.misc_diamond" size="14" />
              <span class="popover-gem-name">{{ slot.gemName || slot.gemId }}</span>
              <span class="popover-gem-bonus">{{ slot.bonusText }}</span>
            </div>
          </div>

          <div v-if="showEquipmentCompare" class="popover-compare">
            <div class="compare-title">装备对比</div>
            <div v-if="comparePreview.loading" class="compare-loading">正在读取装备信息...</div>
            <div v-else-if="comparePreview.error" class="compare-error">{{ comparePreview.error }}</div>
            <div v-else-if="comparePreview.data" class="compare-columns">
              <div class="compare-col">
                <div class="compare-col-title">基础属性</div>
                <div v-if="compareBaseDiffs.length" class="compare-diff-list">
                  <div v-for="diff in compareBaseDiffs" :key="diff.statName" class="compare-diff-row">
                    <span class="compare-diff-name">{{ diff.statName }}</span>
                    <span class="compare-diff-current">{{ diff.currentDisplay }}</span>
                    <span class="compare-diff-arrow">→</span>
                    <span class="compare-diff-new" :class="diff.direction">{{ diff.newDisplay }}</span>
                    <span class="compare-diff-delta" :class="diff.direction">{{ diff.deltaDisplay }}</span>
                  </div>
                </div>
                <div v-else class="compare-diff-empty">无变化</div>
              </div>
              <div class="compare-col">
                <div class="compare-col-title">词条属性</div>
                <div v-if="compareRerollDiffs.length" class="compare-diff-list">
                  <div v-for="diff in compareRerollDiffs" :key="diff.statName" class="compare-diff-row">
                    <span class="compare-diff-name" :style="diff.tierColor ? { color: diff.tierColor } : null">{{ diff.statName }}</span>
                    <span class="compare-diff-current">{{ diff.currentDisplay }}</span>
                    <span class="compare-diff-arrow">→</span>
                    <span class="compare-diff-new" :class="diff.direction">{{ diff.newDisplay }}</span>
                    <span class="compare-diff-delta" :class="diff.direction">{{ diff.deltaDisplay }}</span>
                  </div>
                </div>
                <div v-else class="compare-diff-empty">无词条</div>
              </div>
            </div>
          </div>

          <div v-if="selectedItem.description" class="popover-description">
            {{ selectedItem.description }}
          </div>

          <div class="popover-actions">
            <template v-if="itemPopover.kind === 'equipment'">
              <button class="action-btn equip" @click="selectedItem.isEquipped ? unequipSelectedItem() : equipItem()">
                {{ selectedItem.isEquipped ? '卸下' : '穿戴' }}
              </button>
              <button class="action-btn identify" @click="identifyItem" v-if="!selectedItem.identified">鉴定</button>
              <button class="action-btn forge" @click="openForgeModal">锻造</button>
              <button class="action-btn donate" @click="donateItem">捐献</button>
              <button class="action-btn decompose" @click="decomposeEquipment">分解</button>
              <button class="action-btn bind" @click="selectedItem.isLocked ? unlockItem() : lockItem()">
                {{ selectedItem.isLocked ? '解锁' : '锁定' }}
              </button>
              <button class="action-btn sell" @click="sellItem">出售</button>
            </template>
            <template v-else-if="itemPopover.kind === 'gems'">
              <button class="action-btn enhance" @click="openGemSynth">合成</button>
              <button class="action-btn sell" @click="sellItem">出售</button>
            </template>
            <template v-else>
              <button v-if="!isRecipeScroll(selectedItem)" class="action-btn bind" @click="selectedItem.isLocked ? unlockItem() : lockItem()">
                {{ selectedItem.isLocked ? '解锁' : '锁定' }}
              </button>
              <button v-if="selectedItem.canUse" class="action-btn use" @click="useItem">{{ selectedItem.useActionText || '使用' }}</button>
              <button v-if="selectedItem.itemId && selectedItem.itemId.startsWith('skill_book_') && selectedItem.itemId !== 'skill_book_fragment'" class="action-btn use" @click="decomposeSkillBook">分解</button>
              <button v-if="selectedItem.canGift" class="action-btn gift" @click="openGiftFavorability">赠送</button>
              <button class="action-btn drop" @click="dropItem">丢弃</button>
            </template>
          </div>

          <div class="popover-footer-hint">
            移出格子或浮层后自动关闭，点击空白区域、按 Esc 或点右上角也可关闭。
          </div>
        </div>
      </Teleport>

      <!-- 背包容量 -->
      <div v-if="currentTab === 'equipment' || currentTab === 'props' || currentTab === 'gems'" class="inventory-capacity">
        <div class="capacity-bar">
          <div
            class="capacity-fill"
            :style="{ width: Math.min(100, getItemCount(currentTab) / (currentTab === 'equipment' ? equipmentMax : propsMax) * 100) + '%' }"
          ></div>
        </div>
        <span class="capacity-text">
          {{ getItemCount(currentTab) }} / {{ currentTab === 'equipment' ? equipmentMax : propsMax }}
        </span>
      </div>
    </div>
  </XiuXianModal>
</template>

<script>
import { computed, nextTick, onBeforeUnmount, onMounted, reactive, ref, watch } from 'vue'
import { confirm as showConfirm } from '@/utils/confirm'
import XiuXianModal from '../common/XiuXianModal.vue'
import AssetIcon from '../common/AssetIcon.vue'
import { ICON } from '../../icons'
import { useGameStore } from '../../state/gameStore'
import { apiClient, buildApiUrl } from '../../lib/apiClient'
import {
  buildEquipmentCard,
  buildInventoryEquipmentList,
  buildInventoryPropList,
  resolveBackendSlotValue
} from '../../services/gameDisplay'
import toast from '@/utils/toast'

export default {
  name: 'InventoryModal',

  components: {
    XiuXianModal,
    AssetIcon
  },

  props: {
    modelValue: {
      type: Boolean,
      default: false
    }
  },

  emits: ['update:modelValue', 'open-gift-favorability', 'open-gem-synth', 'open-forge'],

  setup(props, { emit }) {
    const gameStore = useGameStore()

    // 当前背包页签。
    const currentTab = ref('equipment')
    const scrollPositions = reactive({ equipment: 0, props: 0, gems: 0, textCollection: 0, imageCollection: 0 })
    const inventoryContentRef = ref(null)
    const inventoryTabs = [
      { key: 'equipment', name: '装备', icon: ICON.slot_weapon },
      { key: 'props', name: '道具', icon: ICON.nav_backpack },
      { key: 'gems', name: '宝石', icon: ICON.misc_diamond },
      { key: 'textCollection', name: '文字图鉴', icon: ICON.item_book },
      { key: 'imageCollection', name: '图片图鉴', icon: ICON.item_chest }
    ]

    // 视觉层使用的容量上限。
    // 当前用于渲染空格子，让背包布局在有无数据时都保持稳定。
    const equipmentMax = computed(() => Math.max(
      100,
      Number(gameStore.state.player?.equipmentInventoryCapacity ?? gameStore.state.player?.EquipmentInventoryCapacity ?? 100) || 100
    ))
    const propsMax = computed(() => Math.max(
      100,
      Number(gameStore.state.player?.itemInventoryCapacity ?? gameStore.state.player?.ItemInventoryCapacity ?? 100) || 100
    ))

    const equipmentSlotOptions = [
      { key: 'Weapon', name: '武器' },
      { key: 'Helmet', name: '头盔' },
      { key: 'Armor', name: '衣服' },
      { key: 'Pants', name: '裤子' },
      { key: 'Boots', name: '鞋子' },
      { key: 'Necklace', name: '项链' },
      { key: 'Ring', name: '戒指' },
      { key: 'Treasure', name: '法宝' }
    ]
    const equipmentSlotKeysByValue = ['Weapon', 'Helmet', 'Armor', 'Pants', 'Necklace', 'Ring', 'Boots', 'Treasure']

    // 当前筛选条件。
    const filters = reactive({
      quality: '',
      element: '',
      level: '',
      slot: '',
      search: ''
    })

    // 批量操作模式。
    const batchMode = ref(false)
    const batchSelected = reactive(new Set())

    const toggleBatchMode = () => {
      batchMode.value = !batchMode.value
      if (!batchMode.value) batchSelected.clear()
    }

    const handleEquipmentClick = (item, event) => {
      if (batchMode.value) {
        if (!canBatchSelect(item)) {
          toast.warning(`${item.name} 已锁定，不能加入批量出售。`)
          return
        }
        if (batchSelected.has(item.instanceId)) {
          batchSelected.delete(item.instanceId)
        } else {
          batchSelected.add(item.instanceId)
        }
      } else {
        toggleItemPopover(item, event, 'equipment')
      }
    }

    const handlePropsClick = (item, event) => {
      if (batchMode.value) {
        if (!canBatchSelect(item)) {
          toast.warning(`${item.name} 已锁定，不能加入批量操作。`)
          return
        }
        if (batchSelected.has(item.id)) {
          batchSelected.delete(item.id)
        } else {
          batchSelected.add(item.id)
        }
      } else {
        toggleItemPopover(item, event, 'props')
      }
    }

    const handleGemsClick = (item, event) => {
      toggleItemPopover(item, event, 'gems')
    }

    const canBatchSelect = (item) => {
      if (currentTab.value === 'equipment') {
        return !item?.isEquipped && !item?.isLocked
      }
      if (currentTab.value === 'props') {
        return !item?.isLocked
      }
      return false
    }

    const getBatchCandidates = () => currentTab.value === 'equipment'
      ? filteredEquipment.value.filter(canBatchSelect)
      : currentTab.value === 'props'
        ? filteredProps.value.filter(canBatchSelect)
        : []

    const selectAllBatch = () => {
      getBatchCandidates().forEach((item) => {
        batchSelected.add(currentTab.value === 'equipment' ? item.instanceId : item.id)
      })
    }

    const invertBatchSelection = () => {
      getBatchCandidates().forEach((item) => {
        const id = currentTab.value === 'equipment' ? item.instanceId : item.id
        if (batchSelected.has(id)) {
          batchSelected.delete(id)
        } else {
          batchSelected.add(id)
        }
      })
    }

    const batchSell = async () => {
      if (batchSelected.size === 0) return

      if (currentTab.value === 'equipment') {
        const selected = equipmentList.value.filter(item => batchSelected.has(item.instanceId) && canBatchSelect(item))
        const blockedCount = batchSelected.size - selected.length
        if (blockedCount > 0) {
          toast.warning(`${blockedCount} 件锁定装备已跳过，锁定装备不能出售。`)
          batchSelected.clear()
          selected.forEach(item => batchSelected.add(item.instanceId))
        }
        if (selected.length === 0) {
          return
        }
        if (!await showConfirm(`确定要出售 ${selected.length} 件装备吗？`)) return
        try {
          const result = await gameStore.sellEquipmentsBatch(selected.map(item => item.instanceId))
          batchSelected.clear()
          const soldCount = result?.soldCount ?? result?.SoldCount ?? 0
          const goldEarned = result?.goldEarned ?? result?.GoldEarned ?? 0
          toast.success(`成功出售 ${soldCount} 件装备，获得 ${goldEarned} 金币`)
        } catch (error) {
          toast.error(error.message || '批量出售装备失败')
        }
      } else {
        const bound = propsList.value.filter(item => batchSelected.has(item.id) && item.isLocked)
        const toSell = propsList.value.filter(item => batchSelected.has(item.id) && canBatchSelect(item))
        if (bound.length > 0) {
          toast.error(`${bound.length} 种道具已锁定，无法出售。`)
        }
        if (toSell.length === 0) return
        if (!await showConfirm(`确定要出售 ${toSell.length} 种道具吗？`)) return
        try {
          const result = await gameStore.sellInventoryItemsBatch(toSell.map((item) => ({ itemId: item.itemId, count: item.count })))
          batchSelected.clear()
          const soldItemCount = result?.soldItemCount ?? result?.SoldItemCount ?? 0
          const soldQuantity = result?.soldQuantity ?? result?.SoldQuantity ?? 0
          const goldEarned = result?.goldEarned ?? result?.GoldEarned ?? 0
          toast.success(`成功出售 ${soldItemCount} 种道具，共 ${soldQuantity} 件，获得 ${goldEarned} 金币`)
        } catch (error) {
          toast.error(error.message || '批量出售道具失败')
        }
      }
    }

    const batchDiscard = async () => {
      if (batchSelected.size === 0) return

      if (currentTab.value === 'equipment') {
        toast.info('装备暂不支持丢弃，请使用批量出售。')
        return
      } else {
        const bound = propsList.value.filter(item => batchSelected.has(item.id) && item.isLocked)
        const toDrop = propsList.value.filter(item => batchSelected.has(item.id) && canBatchSelect(item))
        if (bound.length > 0) {
          toast.error(`${bound.length} 种道具已锁定，无法丢弃。`)
        }
        if (toDrop.length === 0) return
        if (!await showConfirm(`确定要丢弃 ${toDrop.length} 种道具吗？此操作不可撤销。`, { type: 'danger' })) return
        try {
          const result = await gameStore.discardItemsBatch(toDrop.map((item) => ({ itemId: item.id, quantity: item.count })))
          batchSelected.clear()
          toast.success(result?.message || result?.Message || `成功丢弃 ${result?.discardedItemCount ?? result?.DiscardedItemCount ?? 0} 种道具`)
        } catch (error) {
          toast.error(error.message || '批量丢弃道具失败')
        }
      }
    }

    // 中文注释：
    // 这里直接把真实后端返回的装备列表和道具列表映射成旧弹窗原本使用的字段结构，
    // 这样模板层不需要跟着重写，仍然可以保留你原来的背包界面布局。
    const equipmentList = computed(() => (
      buildInventoryEquipmentList(gameStore.state.equipments)
        .filter((item) => !item.isEquipped)
    ))
    const propsList = computed(() => buildInventoryPropList(gameStore.state.inventory))

    // 装备与道具都继续沿用“悬浮 + 点击固定”这套浮层交互，
    // 所以这里需要同时维护选中项、浮层定位和延迟关闭计时器。

    // 中文注释：
    // 背包详情不再固定渲染在弹窗底部，而是改成靠近格子的浮层。
    // 这样不会把网格内容往下顶，也能在深色 / 浅色 / 雅致三主题里沿用同一套语义色变量。
    const selectedItem = ref(null)
    const itemPopoverRef = ref(null)
    const itemPopoverAnchorEl = ref(null)
    const itemPopoverPosition = reactive({ x: 0, y: 0 })
    const itemPopover = reactive({
      visible: false,
      kind: 'equipment',
      pinned: false
    })
    const comparePreview = reactive({
      loading: false,
      error: '',
      data: null,
      instanceId: '',
      requestId: 0
    })
    let popoverCloseTimer = null

    // 五行名称映射
    const elementNames = {
      metal: '金',
      wood: '木',
      water: '水',
      fire: '火',
      earth: '土'
    }

    const getElementName = (element) => elementNames[element] || element

    const isRecipeScroll = (item) => {
      const type = String(item?.itemType || '').toLowerCase()
      const itemId = String(item?.itemId || '').toLowerCase()
      return type === '17' || type.includes('recipescroll') || type.includes('recipe_scroll') || type.includes('配方') || type.includes('图纸') || itemId.includes('recipe_scroll')
    }

    const getItemCount = (tab) => {
      if (tab === 'equipment') return equipmentList.value.length
      if (tab === 'props') return filteredProps.value.length
      if (tab === 'gems') return filteredGems.value.length
      if (tab === 'textCollection') return textCollections.value.reduce((sum, s) => sum + s.ownedCount, 0)
      if (tab === 'imageCollection') return imageCollections.value.reduce((sum, s) => sum + s.ownedCount, 0)
      return 0
    }

    // 图鉴数据
    const collectionBonuses = ref([])
    const textCollections = ref([])
    const imageCollections = ref([])

    const ATTR_TYPE_MAP = {
      Type1: '生命', Type2: '法力', Type3: '物攻', Type4: '法攻',
      Type5: '物防', Type6: '法防', Type7: '速度', Type8: '命中',
      Type9: '闪避', Type10: '暴击', Type11: '暴伤', Type12: '连击',
      Type13: '反击', Type14: '破甲', Type15: '增伤'
    }
    function attrTypeLabel(type) { return ATTR_TYPE_MAP[type] || type }
    function imgUrl(path) { return path ? buildApiUrl(path) : '' }

    async function loadCollections() {
      try {
        const [playerCollections, bonuses] = await Promise.all([
          apiClient.getPlayerCollection(),
          apiClient.getPlayerBonuses()
        ])
        collectionBonuses.value = bonuses
        textCollections.value = playerCollections.filter(s => s.collectionType === 0)
        imageCollections.value = playerCollections.filter(s => s.collectionType === 1)
      } catch {
        // 静默失败，图鉴数据加载不阻塞背包
      }
    }

    const filteredEquipment = computed(() => {
      return equipmentList.value.filter((item) => {
        if (filters.quality && item.quality !== filters.quality) return false
        if (filters.level && String(item.level || 1) !== String(filters.level)) return false
        if (filters.slot && normalizeEquipmentSlot(item) !== filters.slot) return false
        if (filters.search && !item.name.includes(filters.search)) return false
        return true
      })
    })

    function normalizeEquipmentSlot(item) {
      const rawSlot = item?.backendSlot ?? item?.slot
      if (typeof rawSlot === 'number' || /^\d+$/.test(String(rawSlot ?? ''))) {
        return equipmentSlotKeysByValue[Number(rawSlot)] || ''
      }

      const normalized = String(rawSlot || '').trim().toLowerCase()
      return equipmentSlotOptions.find((slot) => slot.key.toLowerCase() === normalized)?.key || ''
    }

    const equipmentLevels = computed(() => (
      [...new Set(equipmentList.value.map((item) => Number(item.level || 1)).filter(Number.isFinite))]
        .sort((a, b) => a - b)
    ))

    const filteredProps = computed(() => {
      return propsList.value.filter((item) => {
        if (item.type === '宝石') return false
        if (filters.quality && item.quality !== filters.quality) return false
        if (filters.search && !item.name.includes(filters.search)) return false
        return true
      })
    })

    const filteredGems = computed(() => {
      return propsList.value.filter((item) => {
        if (item.type !== '宝石') return false
        if (filters.quality && item.quality !== filters.quality) return false
        if (filters.search && !item.name.includes(filters.search)) return false
        return true
      })
    })

    const normalizeComparePayload = (compareResult, hoveredItem) => {
      const currentEquipment = compareResult?.currentEquipment || compareResult?.CurrentEquipment
      const newEquipment = compareResult?.newEquipment || compareResult?.NewEquipment || hoveredItem

      return {
        current: currentEquipment ? buildEquipmentCard(currentEquipment) : null,
        next: hoveredItem || buildEquipmentCard(newEquipment),
        differences: (compareResult?.differences || compareResult?.Differences || []).map((diff) => ({
          statName: diff.statName ?? diff.StatName ?? '属性',
          currentValue: Number(diff.currentValue ?? diff.CurrentValue ?? 0) || 0,
          newValue: Number(diff.newValue ?? diff.NewValue ?? 0) || 0,
          difference: Number(diff.difference ?? diff.Difference ?? 0) || 0
        }))
      }
    }

    const showEquipmentCompare = computed(() => (
      itemPopover.visible &&
      itemPopover.kind === 'equipment' &&
      Boolean(selectedItem.value?.instanceId) &&
      !Boolean(selectedItem.value?.isEquipped) &&
      Boolean(comparePreview.data?.current?.stats?.length)
    ))

    const BASE_STAT_NAMES = new Set(['物攻', '法攻', '物防', '法防', '生命', '法力', '强化', '速度', '身法', '身法速度'])

    function buildCompareDiffs(currentStats, nextStats) {
      const currentMap = new Map()
      const nextMap = new Map()
      const allKeys = new Set()

      for (const stat of currentStats) {
        const key = stat.label || stat.name
        if (key) {
          currentMap.set(key, stat)
          allKeys.add(key)
        }
      }

      for (const stat of nextStats) {
        const key = stat.label || stat.name
        if (key) {
          nextMap.set(key, stat)
          allKeys.add(key)
        }
      }

      const diffs = []
      for (const key of allKeys) {
        const currentStat = currentMap.get(key)
        const nextStat = nextMap.get(key)

        const currentVal = currentStat ? parseStatNumeric(currentStat.value) : 0
        const nextVal = nextStat ? parseStatNumeric(nextStat.value) : 0
        const delta = nextVal - currentVal
        if (Math.abs(delta) < 0.01) continue

        const formattedDelta = parseFloat(delta.toFixed(2))

        diffs.push({
          statName: key,
          currentDisplay: currentStat?.displayValue || formatStatValue(currentStat?.value) || '0',
          newDisplay: nextStat?.displayValue || formatStatValue(nextStat?.value) || '0',
          deltaDisplay: formattedDelta > 0 ? `+${formattedDelta}` : `${formattedDelta}`,
          direction: delta > 0 ? 'positive' : 'negative',
          tierColor: nextStat?.tierColor || currentStat?.tierColor || ''
        })
      }
      return diffs
    }

    const compareBaseDiffs = computed(() => {
      const data = comparePreview.data
      if (!data?.current?.stats?.length || !data?.next?.stats?.length) return []
      const currentBase = data.current.stats.filter((s) => BASE_STAT_NAMES.has(s.label || s.name))
      const nextBase = data.next.stats.filter((s) => BASE_STAT_NAMES.has(s.label || s.name))
      return buildCompareDiffs(currentBase, nextBase)
    })

    const compareRerollDiffs = computed(() => {
      const data = comparePreview.data
      if (!data?.current?.stats?.length || !data?.next?.stats?.length) return []
      const currentReroll = data.current.stats.filter((s) => !BASE_STAT_NAMES.has(s.label || s.name))
      const nextReroll = data.next.stats.filter((s) => !BASE_STAT_NAMES.has(s.label || s.name))
      return buildCompareDiffs(currentReroll, nextReroll)
    })

    function parseStatNumeric(value) {
      if (typeof value === 'number') return value
      if (typeof value === 'string') {
        const cleaned = value.replace(/[+%]/g, '').trim()
        const num = Number(cleaned)
        return Number.isFinite(num) ? num : 0
      }
      return 0
    }

    const getStatValueClass = (value) => {
      if (typeof value === 'number') {
        return value > 0 ? 'positive' : value < 0 ? 'negative' : ''
      }

      const normalized = String(value ?? '').trim()
      if (!normalized) {
        return ''
      }

      if (normalized.startsWith('-')) {
        return 'negative'
      }

      const numericMatch = normalized.match(/^(\d+(?:\.\d+)?)(%?)$/)
      return numericMatch ? 'positive' : ''
    }

    const formatStatValue = (value) => {
      if (value === null || value === undefined || value === '') {
        return '0'
      }

      if (typeof value === 'number') {
        return value > 0 ? `+${value}` : `${value}`
      }

      const normalized = String(value).trim()
      if (!normalized) {
        return '0'
      }

      if (normalized.startsWith('+') || normalized.startsWith('-')) {
        return normalized
      }

      const numericMatch = normalized.match(/^(\d+(?:\.\d+)?)(%?)$/)
      if (!numericMatch) {
        return normalized
      }

      const numericValue = Number(numericMatch[1])
      const suffix = numericMatch[2] || ''
      return numericValue > 0 ? `+${numericValue}${suffix}` : `${numericValue}${suffix}`
    }

    const clearPopoverCloseTimer = () => {
      if (popoverCloseTimer) {
        clearTimeout(popoverCloseTimer)
        popoverCloseTimer = null
      }
    }

    const cancelPopoverClose = () => {
      clearPopoverCloseTimer()
    }

    const resetComparePreview = () => {
      comparePreview.loading = false
      comparePreview.error = ''
      comparePreview.data = null
      comparePreview.instanceId = ''
    }

    const getPopoverSize = (kind, item) => {
      if (kind === 'equipment') {
        return item?.instanceId && !item?.isEquipped
          ? { width: 460, height: 620 }
          : { width: 360, height: 420 }
      }

      if (kind === 'gems') {
        return { width: 320, height: 320 }
      }

      return { width: 320, height: 360 }
    }

    const updatePopoverPosition = (triggerElement, kind, item) => {
      if (!triggerElement) {
        return
      }

      const rect = triggerElement.getBoundingClientRect()
      const estimatedSize = getPopoverSize(kind, item)
      const renderedRect = itemPopoverRef.value?.getBoundingClientRect?.()
      const width = renderedRect?.width || estimatedSize.width
      const height = Math.min(
        renderedRect?.height || estimatedSize.height,
        window.innerHeight - 24
      )
      const viewportPadding = 12
      const gap = 12
      const preferredX = rect.right + gap
      const fallbackX = rect.left - width - gap
      const maxX = Math.max(viewportPadding, window.innerWidth - width - viewportPadding)
      const maxY = Math.max(viewportPadding, window.innerHeight - height - viewportPadding)

      itemPopoverPosition.x = preferredX + width > window.innerWidth - viewportPadding
        ? Math.max(viewportPadding, fallbackX)
        : preferredX
      itemPopoverPosition.x = Math.min(Math.max(viewportPadding, itemPopoverPosition.x), maxX)
      itemPopoverPosition.y = Math.min(Math.max(viewportPadding, rect.top), maxY)
    }

    const repositionActivePopover = () => {
      if (!itemPopover.visible || !itemPopoverAnchorEl.value || !selectedItem.value) {
        return
      }

      updatePopoverPosition(itemPopoverAnchorEl.value, itemPopover.kind, selectedItem.value)
    }

    const hideItemPopover = () => {
      clearPopoverCloseTimer()
      itemPopover.visible = false
      itemPopover.pinned = false
      itemPopoverAnchorEl.value = null
      selectedItem.value = null
      resetComparePreview()
    }

    const schedulePopoverClose = () => {
      if (itemPopover.pinned) {
        return
      }
      clearPopoverCloseTimer()
      popoverCloseTimer = window.setTimeout(() => {
        hideItemPopover()
      }, 450)
    }

    const loadComparePreview = async (item) => {
      if (!item?.instanceId || item.isEquipped) {
        resetComparePreview()
        return
      }

      comparePreview.error = ''
      comparePreview.instanceId = item.instanceId

      comparePreview.loading = true
      comparePreview.data = null
      comparePreview.requestId += 1
      const currentRequestId = comparePreview.requestId

      try {
        const compareResult = await apiClient.getEquipmentCompare(item.instanceId)
        const normalizedCompare = normalizeComparePayload(compareResult, item)

        if (
          comparePreview.requestId === currentRequestId &&
          comparePreview.instanceId === item.instanceId &&
          selectedItem.value?.instanceId === item.instanceId &&
          itemPopover.visible
        ) {
          comparePreview.data = normalizedCompare
        }
      } catch (error) {
        if (
          comparePreview.requestId === currentRequestId &&
          comparePreview.instanceId === item.instanceId &&
          selectedItem.value?.instanceId === item.instanceId &&
          itemPopover.visible
        ) {
          comparePreview.error = error.message || '读取装备对比失败。'
        }
      } finally {
        if (
          comparePreview.requestId === currentRequestId &&
          comparePreview.instanceId === item.instanceId &&
          selectedItem.value?.instanceId === item.instanceId &&
          itemPopover.visible
        ) {
          comparePreview.loading = false
        }
      }
    }

    const openItemPopover = (item, event, kind, pinned = false) => {
      cancelPopoverClose()
      selectedItem.value = item
      itemPopover.visible = true
      itemPopover.kind = kind
      itemPopover.pinned = pinned
      itemPopoverAnchorEl.value = event?.currentTarget || null
      updatePopoverPosition(itemPopoverAnchorEl.value, kind, item)
      nextTick(() => {
        repositionActivePopover()
      })

      if (kind === 'equipment') {
        void loadComparePreview(item)
      } else {
        resetComparePreview()
      }
    }

    const handleItemMouseEnter = (item, event, kind) => {
      cancelPopoverClose()

      if (itemPopover.pinned) {
        return
      }

      if (itemPopover.visible && selectedItem.value?.id === item.id && itemPopover.kind === kind) {
        itemPopoverAnchorEl.value = event?.currentTarget || itemPopoverAnchorEl.value
        updatePopoverPosition(event?.currentTarget, kind, item)
        return
      }

      openItemPopover(item, event, kind, false)
    }

    const toggleItemPopover = (item, event, kind) => {
      if (itemPopover.visible && selectedItem.value?.id === item.id && itemPopover.kind === kind) {
        if (itemPopover.pinned) {
          hideItemPopover()
        } else {
          itemPopover.pinned = true
          cancelPopoverClose()
        }
        return
      }

      openItemPopover(item, event, kind, true)
    }

    const handleInventoryScroll = () => {
      if (inventoryContentRef.value) {
        scrollPositions[currentTab.value] = inventoryContentRef.value.scrollTop
      }
      if (itemPopover.visible) {
        hideItemPopover()
      }
    }

    const handleDocumentPointerDown = (event) => {
      if (!itemPopover.visible) {
        return
      }

      const target = event.target instanceof Element ? event.target : null
      if (!target) {
        hideItemPopover()
        return
      }

      if (itemPopoverRef.value?.contains(target)) {
        return
      }

      if (target.closest('.equipment-row:not(.empty)') || target.closest('.prop-item:not(.empty)')) {
        return
      }

      hideItemPopover()
    }

    const handleDocumentKeydown = (event) => {
      if (event.key === 'Escape' && itemPopover.visible) {
        hideItemPopover()
      }
    }

    onMounted(() => {
      document.addEventListener('pointerdown', handleDocumentPointerDown, true)
      document.addEventListener('keydown', handleDocumentKeydown)
    })

    onBeforeUnmount(() => {
      clearPopoverCloseTimer()
      document.removeEventListener('pointerdown', handleDocumentPointerDown, true)
      document.removeEventListener('keydown', handleDocumentKeydown)
    })

    watch(() => props.modelValue, async (visible) => {
      if (!visible) {
        hideItemPopover()
        return
      }

      try {
        // 中文注释：
        // 背包弹窗打开时，才去拉取道具背包和装备背包数据。
        // 这样首页不会因为用户暂时没有查看背包的需求，就提前触发两组接口请求。
        await Promise.all([
          gameStore.loadInventory(true),
          gameStore.loadEquipment(true),
          loadCollections()
        ])
      } catch (error) {
        toast.error(error.message || '加载背包数据失败。')
      }
    }, { immediate: true })

    watch(currentTab, (newTab, oldTab) => {
      hideItemPopover()
      batchSelected.clear()
      if (inventoryContentRef.value) {
        scrollPositions[oldTab] = inventoryContentRef.value.scrollTop
        nextTick(() => {
          inventoryContentRef.value.scrollTop = scrollPositions[newTab] || 0
        })
      }
    })

      watch(() => `${filters.quality}|${filters.element}|${filters.level}|${filters.slot}|${filters.search}`, () => {
      if (itemPopover.visible && !itemPopover.pinned) {
        hideItemPopover()
      }
    })

    watch([equipmentList, propsList], () => {
      if (!selectedItem.value || !itemPopover.visible) {
        return
      }

      const activeList = itemPopover.kind === 'equipment' ? equipmentList.value : propsList.value
      const latestSelected = activeList.find((item) => item.id === selectedItem.value.id)
      if (!latestSelected) {
        hideItemPopover()
        return
      }

      selectedItem.value = latestSelected

      if (itemPopover.kind === 'equipment') {
        if (latestSelected.isEquipped) {
          resetComparePreview()
        } else {
          void loadComparePreview(latestSelected)
        }
      } else {
        resetComparePreview()
      }
      nextTick(() => {
        repositionActivePopover()
      })
    }, { immediate: true, deep: true })

    watch(
      () => [itemPopover.visible, itemPopover.kind, comparePreview.loading, comparePreview.error, comparePreview.data],
      () => {
        nextTick(() => {
          repositionActivePopover()
        })
      },
      { deep: true }
    )

    const resetFilters = () => {
      filters.quality = ''
      filters.element = ''
      filters.level = ''
      filters.slot = ''
      filters.search = ''
    }

    const equipItem = async () => {
      const activeItem = selectedItem.value
      if (!activeItem?.instanceId) return

      try {
        await gameStore.equipItem(activeItem.instanceId)
        toast.success(`已穿戴 ${activeItem.name}`)
        hideItemPopover()
      } catch (error) {
        toast.error(error.message || '穿戴失败')
      }
    }

    const unequipSelectedItem = async () => {
      const activeItem = selectedItem.value
      if (!activeItem?.isEquipped) return

      try {
        await gameStore.unequipItem(resolveBackendSlotValue(activeItem.backendSlot))
        toast.success(`已卸下 ${activeItem.name}`)
        hideItemPopover()
      } catch (error) {
        toast.error(error.message || '卸下失败')
      }
    }

    const enhanceItem = async () => {
      const activeItem = selectedItem.value
      if (!activeItem?.instanceId) return

      try {
        // 中文注释：
        // 当前最小真实闭环里，强化默认直接走后端既有材料规则。
        // 后端会自己校验金币与强化材料是否足够，前端这里只负责发起请求并刷新展示。
        const result = await gameStore.enhanceEquipment(activeItem.instanceId)
        const success = Boolean(result?.success ?? result?.Success)
        const level = result?.newEnhanceLevel ?? result?.NewEnhanceLevel ?? activeItem.enhance
        const message = result?.message || result?.Message || (success ? '强化成功' : '强化失败')
        if (success) {
          toast.success(`${message}，当前强化等级 +${level}`)
        } else {
          toast.error(`${message}，当前强化等级 +${level}`)
        }
        hideItemPopover()
      } catch (error) {
        toast.error(error.message || '强化失败')
      }
    }

    const identifyItem = () => {
      toast.info('当前版本的装备数据已经是可直接展示的真实装备，无需额外鉴定。')
    }

    const openForgeModal = () => {
      const activeItem = selectedItem.value
      hideItemPopover()
      emit('open-forge', activeItem)
    }

    const donateItem = async () => {
      const activeItem = selectedItem.value
      if (!activeItem?.instanceId) return

      if (activeItem.isEquipped) {
        toast.warning('请先卸下这件装备，再进行捐献。')
        return
      }

      if (!await showConfirm(`确定要捐献 ${activeItem.name} 吗？\n捐献后装备会被永久移除。`, { type: 'danger', icon: '🎁' })) {
        return
      }

      try {
        const result = await gameStore.donateEquipment(activeItem.instanceId)
        toast.success(result?.message || result?.Message || `已捐献 ${activeItem.name}`)
        hideItemPopover()
      } catch (error) {
        toast.error(error.message || '捐献失败')
      }
    }

    const lockItem = async () => {
      const activeItem = selectedItem.value
      if (!activeItem) return

      if (activeItem.isLocked) {
        toast.warning('该物品已经锁定。')
        return
      }

      if (!await showConfirm(`确定要锁定 ${activeItem.name} 吗？\n锁定后将不能再出售。`, { icon: '🔒' })) {
        return
      }

      try {
        let result
        if (itemPopover.kind === 'equipment') {
          result = await gameStore.bindEquipment(activeItem.instanceId)
        } else {
          result = await gameStore.lockItem(activeItem.id)
        }
        toast.success(result?.message || result?.Message || `已锁定 ${activeItem.name}`)
        hideItemPopover()
      } catch (error) {
        toast.error(error.message || '锁定失败')
      }
    }

    const unlockItem = async () => {
      const activeItem = selectedItem.value
      if (!activeItem) return

      if (!activeItem.isLocked) {
        toast.warning('该物品未锁定。')
        return
      }

      if (!await showConfirm(`确定要解锁 ${activeItem.name} 吗？`, { icon: '🔓' })) {
        return
      }

      try {
        let result
        if (itemPopover.kind === 'equipment') {
          result = await gameStore.unlockEquipment(activeItem.instanceId)
        } else {
          result = await gameStore.unlockItem(activeItem.id)
        }
        toast.success(result?.message || result?.Message || `已解锁 ${activeItem.name}`)
        hideItemPopover()
      } catch (error) {
        toast.error(error.message || '解锁失败')
      }
    }

    const sellItem = async () => {
      const activeItem = selectedItem.value
      if (!activeItem) return

      if (itemPopover.kind !== 'equipment') {
        if (activeItem.isLocked) {
          toast.warning('锁定物品不能出售。')
          return
        }
        if (!await showConfirm(`确定要出售 ${activeItem.name} x${activeItem.count} 吗？`)) return
        try {
          await gameStore.sellInventoryItem(activeItem.itemId, activeItem.count)
          toast.success(`已出售 ${activeItem.name}`)
          hideItemPopover()
        } catch (error) {
          toast.error(error.message || '出售失败')
        }
        return
      }

      if (activeItem.isEquipped) {
        toast.warning('请先卸下这件装备，再进行出售。')
        return
      }

      if (!await showConfirm(`确定要出售 ${activeItem.name} 吗？`)) {
        return
      }

      try {
        await gameStore.sellEquipment(activeItem.instanceId)
        toast.success(`已出售 ${activeItem.name}`)
        hideItemPopover()
      } catch (error) {
        toast.error(error.message || '出售失败')
      }
    }

    const useItem = async () => {
      const activeItem = selectedItem.value
      if (!activeItem?.id) return

      try {
        const result = await gameStore.useItem(activeItem.id, 1)
        toast.success(result?.message || result?.Message || `已使用 ${activeItem.name}`)
        hideItemPopover()
      } catch (error) {
        toast.error(error.message || '使用失败')
      }
    }

    const decomposeSkillBook = async () => {
      const activeItem = selectedItem.value
      if (!activeItem?.id || !activeItem.itemId?.startsWith('skill_book_') || activeItem.itemId === 'skill_book_fragment') return
      if (!await showConfirm(`确定分解 ${activeItem.name} 吗？将获得 5 个技能书碎片。`)) return

      try {
        const result = await gameStore.decomposeSkillBook(activeItem.id, 1)
        toast.success(result?.message || result?.Message || '技能书分解成功')
        hideItemPopover()
      } catch (error) {
        toast.error(error.message || '技能书分解失败')
      }
    }

    const decomposeEquipment = async () => {
      const activeItem = selectedItem.value
      if (!activeItem?.instanceId || activeItem.isEquipped || activeItem.isLocked) {
        toast.warning('已穿戴或锁定装备不能分解。')
        return
      }

      if (!await showConfirm(`确定分解 ${activeItem.name} 吗？`)) return

      try {
        const result = await gameStore.decomposeEquipments([activeItem.instanceId])
        const rewards = result?.rewards ?? result?.Rewards ?? []
        const rewardText = rewards.map(reward => `${reward.itemName ?? reward.ItemName ?? reward.itemId ?? reward.ItemId} x${reward.quantity ?? reward.Quantity ?? 0}`).join('、')
        toast.success(rewardText ? `分解成功，获得 ${rewardText}` : '分解成功')
        hideItemPopover()
      } catch (error) {
        toast.error(error.message || '装备分解失败')
      }
    }

    const batchDecompose = async () => {
      if (currentTab.value !== 'equipment' || batchSelected.size === 0) return

      const selected = equipmentList.value.filter(item => batchSelected.has(item.instanceId) && canBatchSelect(item))
      if (selected.length === 0) return
      if (!await showConfirm(`确定分解 ${selected.length} 件装备吗？`)) return

      try {
        const result = await gameStore.decomposeEquipments(selected.map(item => item.instanceId))
        batchSelected.clear()
        const decomposedCount = result?.decomposedCount ?? result?.DecomposedCount ?? 0
        const rewards = result?.rewards ?? result?.Rewards ?? []
        const rewardText = rewards.map(reward => `${reward.itemName ?? reward.ItemName ?? reward.itemId ?? reward.ItemId} x${reward.quantity ?? reward.Quantity ?? 0}`).join('、')
        toast.success(rewardText ? `成功分解 ${decomposedCount} 件，获得 ${rewardText}` : `成功分解 ${decomposedCount} 件装备`)
      } catch (error) {
        toast.error(error.message || '批量分解装备失败')
      }
    }

    const openGiftFavorability = () => {
      hideItemPopover()
      emit('open-gift-favorability')
    }

    const openGemSynth = () => {
      const activeItem = selectedItem.value
      hideItemPopover()
      emit('open-gem-synth', activeItem?.itemId || activeItem?.id)
    }

    const dropItem = async () => {
      const activeItem = selectedItem.value
      if (!activeItem?.id) return

      if (activeItem.isLocked) {
        toast.warning('锁定物品不能丢弃。')
        return
      }

      if (!await showConfirm(`确定要丢弃 ${activeItem.name} x1 吗？`, { type: 'danger' })) {
        return
      }

      try {
        await gameStore.discardItem(activeItem.id, 1)
        toast.success(`已丢弃 ${activeItem.name}`)
        hideItemPopover()
      } catch (error) {
        toast.error(error.message || '丢弃失败')
      }
    }

    return {
      ICON,
      inventoryContentRef,
      currentTab,
      inventoryTabs,
      equipmentMax,
      propsMax,
      filters,
      equipmentList,
      propsList,
      collectionBonuses,
      textCollections,
      imageCollections,
      selectedItem,
      itemPopoverRef,
      itemPopover,
      itemPopoverPosition,
      comparePreview,
      showEquipmentCompare,
      compareBaseDiffs,
      compareRerollDiffs,
      getItemCount,
      getElementName,
      isRecipeScroll,
      getStatValueClass,
      formatStatValue,
      filteredEquipment,
      equipmentSlotOptions,
      equipmentLevels,
      canBatchSelect,
      filteredProps,
      filteredGems,
      handleGemsClick,
      openGemSynth,
      cancelPopoverClose,
      schedulePopoverClose,
      handleItemMouseEnter,
      toggleItemPopover,
      hideItemPopover,
      handleInventoryScroll,
      resetFilters,
      equipItem,
      unequipSelectedItem,
      enhanceItem,
      identifyItem,
      openForgeModal,
      donateItem,
      lockItem,
      unlockItem,
      sellItem,
      useItem,
      decomposeSkillBook,
      openGiftFavorability,
      dropItem,
      batchMode,
      batchSelected,
      toggleBatchMode,
      selectAllBatch,
      invertBatchSelection,
      handleEquipmentClick,
      handlePropsClick,
      batchSell,
      decomposeEquipment,
      batchDecompose,
      batchDiscard,
      attrTypeLabel,
      imgUrl
    }
  }
}
</script>

<style scoped>
.inventory-modal {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
  max-height: 80vh;
}

/* 批量操作栏 */
.batch-toolbar {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  padding: var(--spacing-xs) var(--spacing-sm);
  background: var(--xiuxian-bg-secondary);
  border-radius: var(--radius-sm);
}

.batch-toggle-btn {
  padding: 4px 12px;
  border: 1px solid var(--border-color);
  border-radius: var(--radius-sm);
  background: var(--xiuxian-bg-secondary);
  color: var(--text-secondary);
  cursor: pointer;
  font-size: 13px;
  transition: all 0.15s;
}

.batch-toggle-btn:hover {
  border-color: var(--accent-text);
  color: var(--accent-text);
}

.batch-toggle-btn.active {
  background: var(--accent-soft-bg);
  border-color: var(--accent-text);
  color: var(--accent-text);
}

.batch-count {
  font-size: 13px;
  color: var(--text-secondary);
  margin-left: var(--spacing-xs);
}

.batch-select-btn {
  padding: 4px 10px;
  border: 1px solid var(--border-color);
  border-radius: var(--radius-sm);
  background: var(--xiuxian-bg-primary);
  color: var(--text-secondary);
  cursor: pointer;
  font-size: 13px;
}

.batch-select-btn:hover {
  border-color: var(--accent-text);
  color: var(--accent-text);
}

.batch-action-btn {
  padding: 4px 12px;
  border: 1px solid var(--border-color);
  border-radius: var(--radius-sm);
  cursor: pointer;
  font-size: 13px;
  transition: all 0.15s;
}

.batch-action-btn.sell {
  background: #f59e0b20;
  border-color: #f59e0b;
  color: #f59e0b;
}

.batch-action-btn.sell:hover:not(:disabled) {
  background: #f59e0b40;
}

.batch-action-btn.drop {
  background: #ef444420;
  border-color: #ef4444;
  color: #ef4444;
}

.batch-action-btn.drop:hover:not(:disabled) {
  background: #ef444440;
}

.batch-action-btn.clear {
  background: var(--xiuxian-bg-secondary);
  color: var(--text-secondary);
}

.batch-action-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.batch-checkbox {
  position: absolute;
  top: 4px;
  right: 4px;
  width: 20px;
  height: 20px;
  border: 2px solid var(--border-color);
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  color: white;
  background: transparent;
  z-index: 2;
  pointer-events: none;
}

.batch-checkbox.checked {
  background: var(--accent-text);
  border-color: var(--accent-text);
}

.batch-action-btn.decompose,
.action-btn.decompose {
  background: #8b5cf620;
  border-color: #8b5cf6;
  color: #a78bfa;
}

.batch-action-btn.decompose:hover:not(:disabled),
.action-btn.decompose:hover:not(:disabled) {
  background: #8b5cf640;
}

.batch-checkbox.disabled {
  opacity: 0.45;
  cursor: not-allowed;
}

.batch-selected {
  outline: 2px solid var(--accent-text);
  outline-offset: -2px;
}

.batch-locked {
  opacity: 0.72;
}

/* 背包类型切换 */
.inventory-tabs {
  display: flex;
  gap: var(--spacing-sm);
}

.tab-btn {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--spacing-xs);
  padding: var(--spacing-sm) var(--spacing-md);
  background: var(--xiuxian-bg-secondary);
  border: 1px solid transparent;
  border-radius: var(--radius-sm);
  cursor: pointer;
  transition: all 0.15s ease;
  color: var(--text-secondary);
}

.tab-btn:hover {
  border-color: var(--border-color);
  background: var(--button-neutral-hover-bg);
  color: var(--button-neutral-hover-text);
}

.tab-btn.active {
  background: var(--accent-soft-bg);
  border-color: var(--accent-text);
  color: var(--accent-text);
}

.tab-icon {
  font-size: 16px;
}

.tab-name {
  font-size: var(--font-size-base);
}

.tab-count {
  font-size: var(--font-size-sm);
  padding: 2px 6px;
  background: var(--xiuxian-bg-primary);
  border-radius: var(--radius-sm);
  color: var(--text-muted);
}

/* 筛选栏 */
.filter-bar {
  display: flex;
  gap: var(--spacing-md);
  align-items: center;
  padding: var(--spacing-md);
  background: var(--xiuxian-bg-secondary);
  border-radius: var(--radius-md);
}

.filter-group {
  display: flex;
  align-items: center;
  gap: var(--spacing-xs);
}

.filter-group label {
  font-size: var(--font-size-base);
  color: var(--text-secondary);
}

.filter-group select,
.filter-group input {
  padding: var(--spacing-xs) var(--spacing-sm);
  background: var(--xiuxian-bg-primary);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-sm);
  color: var(--text-primary);
  font-size: var(--font-size-base);
}

.filter-group.search {
  flex: 1;
}

.filter-group.search input {
  width: 100%;
}

.reset-btn {
  padding: var(--spacing-xs) var(--spacing-md);
  background: transparent;
  border: 1px solid var(--border-color);
  border-radius: var(--radius-sm);
  color: var(--text-secondary);
  font-size: var(--font-size-base);
  cursor: pointer;
  transition: all 0.15s ease;
}

.reset-btn:hover {
  background: var(--button-outline-hover-bg);
  border-color: var(--button-outline-hover-border);
  color: var(--button-outline-hover-text);
}

/* 背包内容 */
.inventory-content {
  flex: 1;
  overflow-y: auto;
  background: var(--xiuxian-bg-panel);
  border-radius: var(--radius-md);
  padding: var(--spacing-md);
  max-height: 600px;
}

/* 装备网格 */
/* 装备网格（宽格子） */
.equipment-list {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(185px, 1fr));
  gap: 6px;
}

.equipment-row {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 10px;
  background: var(--xiuxian-bg-secondary);
  border: 1px solid var(--border-color);
  border-left: 3px solid var(--border-color);
  border-radius: var(--radius-sm);
  cursor: pointer;
  transition: all 0.15s ease;
  min-height: 52px;
}

.equipment-row:hover {
  background: var(--xiuxian-bg-hover, rgba(255,255,255,0.05));
  border-color: var(--accent-text);
  border-left-color: var(--accent-text);
}

.equipment-row.selected {
  border-color: var(--highlight-text);
  border-left-color: var(--highlight-text);
  box-shadow: var(--shadow-sm);
}

.equipment-row.equipped {
  opacity: 0.55;
}

.equipment-row.empty {
  justify-content: center;
  border-style: dashed;
  opacity: 0.3;
  min-height: 52px;
}

/* 品质左边框颜色 */
.equipment-row.common { border-left-color: var(--quality-common); }
.equipment-row.uncommon { border-left-color: var(--quality-uncommon); }
.equipment-row.rare { border-left-color: var(--quality-rare); }
.equipment-row.epic { border-left-color: var(--quality-epic); }
.equipment-row.legendary { border-left-color: var(--quality-legendary); }
.equipment-row.mythic { border-left-color: var(--quality-mythic); }

.equipment-row-info {
  flex: 1;
  min-width: 0;
}

.equipment-row-name {
  font-size: var(--font-size-xs);
  font-weight: 600;
  color: var(--text-primary);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  display: flex;
  align-items: center;
  gap: 4px;
}

.equipped-tag {
  font-size: 9px;
  font-weight: 500;
  color: var(--accent-text);
  background: var(--accent-soft-bg, rgba(59,130,246,0.15));
  padding: 1px 4px;
  border-radius: 3px;
  flex-shrink: 0;
}

.bound-tag {
  font-size: 9px;
  flex-shrink: 0;
}

.equipment-row-type {
  font-size: 10px;
  color: var(--text-muted);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  margin-top: 2px;
}

.equipment-row-badges {
  display: flex;
  align-items: center;
  gap: 3px;
  flex-shrink: 0;
}

.badge-level {
  font-size: 10px;
  font-weight: 600;
  color: var(--highlight-text);
  background: var(--bg-overlay-light);
  padding: 1px 4px;
  border-radius: 3px;
  white-space: nowrap;
}

.badge-enhance {
  font-size: 10px;
  font-weight: 700;
  color: var(--status-success-text);
  background: rgba(34,197,94,0.1);
  padding: 1px 4px;
  border-radius: 3px;
  white-space: nowrap;
}

/* 道具列表 */
.props-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(185px, 1fr));
  gap: 6px;
}

.empty-icon {
  font-size: 24px;
  color: var(--text-muted);
  opacity: 0.5;
}

.inventory-popover-teleport {
  position: fixed;
  width: min(360px, calc(100vw - 24px));
  max-height: calc(100vh - 24px);
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
  padding: var(--spacing-md);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  background: var(--xiuxian-bg-panel);
  box-shadow: var(--shadow-lg);
  backdrop-filter: blur(12px);
  color: var(--text-primary);
  z-index: 3000;
  pointer-events: auto;
  overflow-y: auto;
  overscroll-behavior: contain;
}

.inventory-popover-teleport.props {
  width: min(320px, calc(100vw - 24px));
}

.inventory-popover-teleport.gems {
  width: min(320px, calc(100vw - 24px));
}

.inventory-popover-teleport.equipment.with-compare {
  width: min(580px, calc(100vw - 24px));
}

.inventory-popover-teleport.common { border-color: var(--quality-common); }
.inventory-popover-teleport.uncommon { border-color: var(--quality-uncommon); }
.inventory-popover-teleport.rare { border-color: var(--quality-rare); }
.inventory-popover-teleport.epic { border-color: var(--quality-epic); }
.inventory-popover-teleport.legendary { border-color: var(--quality-legendary); }
.inventory-popover-teleport.mythic { border-color: var(--quality-mythic); }

.popover-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: var(--spacing-md);
  padding-bottom: var(--spacing-sm);
  border-bottom: 1px solid var(--border-color);
}

.popover-identity {
  display: flex;
  gap: var(--spacing-md);
  min-width: 0;
}

.popover-icon {
  width: 52px;
  height: 52px;
  flex-shrink: 0;
  display: grid;
  place-items: center;
  font-size: 25px;
  border-radius: var(--radius-sm);
  border: 1px solid var(--border-color);
  background: var(--bg-overlay-medium);
}

.popover-meta {
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.popover-name {
  font-size: var(--font-size-lg);
  font-weight: 700;
  line-height: 1.3;
  word-break: break-word;
}

.popover-name.common { color: var(--quality-common); }
.popover-name.uncommon { color: var(--quality-uncommon); }
.popover-name.rare { color: var(--quality-rare); }
.popover-name.epic { color: var(--quality-epic); }
.popover-name.legendary { color: var(--quality-legendary); }
.popover-name.mythic { color: var(--quality-mythic); }

.popover-type {
  font-size: var(--font-size-sm);
  line-height: 1.5;
  color: var(--text-secondary);
}

.popover-type .metal { color: var(--element-metal); }
.popover-type .wood { color: var(--element-wood); }
.popover-type .water { color: var(--element-water); }
.popover-type .fire { color: var(--element-fire); }
.popover-type .earth { color: var(--element-earth); }

.popover-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.popover-tag {
  padding: 2px 8px;
  border-radius: 999px;
  border: 1px solid var(--border-color);
  background: var(--bg-overlay-light);
  font-size: var(--font-size-xs);
  color: var(--text-secondary);
}

.popover-tag.highlight {
  color: var(--highlight-text);
  border-color: var(--highlight-text);
  background: var(--highlight-soft-bg);
}

.popover-tag.warning {
  color: var(--accent-text);
  border-color: var(--accent-text);
  background: var(--accent-soft-bg);
}

.popover-close {
  width: 28px;
  height: 28px;
  flex-shrink: 0;
  border: 1px solid var(--control-border);
  border-radius: var(--radius-sm);
  background: var(--control-bg);
  color: var(--text-secondary);
  cursor: pointer;
  transition: all 0.2s ease;
}

.popover-close:hover {
  border-color: var(--accent-text);
  background: var(--control-bg-hover);
  color: var(--accent-text);
}

.popover-stats {
  display: grid;
  gap: 6px;
}

.popover-stat-line {
  display: flex;
  justify-content: space-between;
  gap: var(--spacing-sm);
  font-size: var(--font-size-sm);
}

.popover-stat-line span:first-child {
  color: var(--text-secondary);
}

.popover-stat-line .positive {
  color: var(--status-success-text);
}

.popover-stat-line .negative {
  color: var(--status-danger-text);
}

.popover-reroll-stats {
  display: flex;
  flex-direction: column;
  gap: 4px;
  padding: var(--spacing-sm);
  border-radius: var(--radius-sm);
  border: 1px solid var(--border-color);
  background: var(--bg-overlay-light);
}

.popover-reroll-title {
  font-size: var(--font-size-xs);
  font-weight: 600;
  color: var(--accent-text);
  margin-bottom: 2px;
}

.popover-reroll-line {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  padding: 3px var(--spacing-sm);
  border-left: 3px solid #9ca3af;
  border-radius: 0 var(--radius-sm) var(--radius-sm) 0;
  background: var(--xiuxian-bg-primary);
  font-size: var(--font-size-xs);
}

.popover-reroll-name {
  flex: 1;
  font-weight: 500;
}

.popover-reroll-tier {
  font-size: 10px;
  opacity: 0.8;
}

.popover-reroll-value {
  color: var(--highlight-text);
  font-weight: 600;
  min-width: 48px;
  text-align: right;
}

.popover-compare {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
  padding: var(--spacing-sm);
  border-radius: var(--radius-sm);
  border: 1px solid var(--border-color);
  background: var(--bg-overlay-light);
}

.compare-title {
  font-size: var(--font-size-sm);
  font-weight: 700;
  letter-spacing: 1px;
  color: var(--highlight-text);
}

.compare-loading,
.compare-error {
  padding: var(--spacing-sm);
  border-radius: var(--radius-sm);
  font-size: var(--font-size-sm);
  line-height: 1.6;
}

.compare-loading {
  background: var(--status-info-bg);
  color: var(--status-info-text);
}

.compare-error {
  background: var(--status-danger-bg);
  color: var(--status-danger-text);
}

.compare-vs {
  margin-left: var(--spacing-sm);
  font-size: var(--font-size-xs);
  font-weight: 400;
  color: var(--text-secondary);
}

.compare-columns {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--spacing-sm);
}

.compare-col {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.compare-col-title {
  font-size: var(--font-size-xs);
  font-weight: 600;
  color: var(--text-secondary);
  padding-bottom: 2px;
  border-bottom: 1px solid var(--border-color);
  margin-bottom: 2px;
}

.compare-diff-list {
  display: grid;
  gap: 4px;
}

.compare-diff-empty {
  font-size: var(--font-size-xs);
  color: var(--text-muted);
  padding: var(--spacing-xs) 0;
}

.compare-diff-row {
  display: flex;
  align-items: center;
  gap: var(--spacing-xs);
  padding: 5px var(--spacing-xs);
  border-bottom: 1px dashed var(--border-color);
  font-size: var(--font-size-xs);
}

.compare-diff-name {
  flex-shrink: 0;
  color: var(--text-secondary);
}

.compare-diff-current {
  color: var(--text-muted);
  margin-left: auto;
}

.compare-diff-arrow {
  color: var(--text-muted);
  font-size: 10px;
  opacity: 0.6;
}

.compare-diff-new.positive {
  color: var(--status-success-text);
  font-weight: 600;
}

.compare-diff-new.negative {
  color: var(--status-danger-text);
  font-weight: 600;
}

.compare-diff-delta {
  font-weight: 600;
  min-width: 48px;
  text-align: right;
}

.compare-diff-delta.positive {
  color: var(--status-success-text);
}

.compare-diff-delta.negative {
  color: var(--status-danger-text);
}

.popover-description {
  padding: var(--spacing-sm);
  border: 1px dashed var(--border-color);
  border-radius: var(--radius-sm);
  background: var(--bg-overlay-light);
  color: var(--text-secondary);
  font-size: var(--font-size-sm);
  line-height: 1.6;
  font-style: italic;
}

.popover-actions {
  display: flex;
  flex-wrap: wrap;
  gap: var(--spacing-sm);
}

.action-btn {
  min-width: 72px;
  padding: var(--spacing-sm) var(--spacing-md);
  border: 1px solid var(--button-neutral-border);
  border-radius: var(--radius-sm);
  background: var(--button-neutral-bg);
  color: var(--button-neutral-text);
  font-size: var(--font-size-sm);
  cursor: pointer;
  transition: all 0.2s ease;
}

.action-btn:disabled {
  opacity: 0.55;
  cursor: not-allowed;
}

.action-btn.equip,
.action-btn.use {
  background: linear-gradient(135deg, var(--button-primary-start), var(--button-primary-end));
  border-color: var(--accent-text);
  color: var(--button-primary-text);
}

.action-btn.enhance {
  background: linear-gradient(135deg, var(--button-secondary-start), var(--button-secondary-end));
  border-color: var(--highlight-text);
  color: var(--button-secondary-text);
}

.action-btn.identify,
.action-btn.reforge,
.action-btn.donate {
  border-color: var(--accent-text);
  color: var(--accent-text);
}

.action-btn.bind {
  border-color: var(--highlight-text);
  color: var(--highlight-text);
}

.action-btn.gift {
  border-color: #e91e8c;
  color: #e91e8c;
}

.action-btn.sell,
.action-btn.drop {
  border-color: var(--status-danger-text);
  color: var(--status-danger-text);
}

.action-btn:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: var(--shadow-sm);
}

.action-btn.equip:hover:not(:disabled),
.action-btn.use:hover:not(:disabled) {
  background: linear-gradient(135deg, var(--button-primary-hover-start), var(--button-primary-hover-end));
  color: var(--button-primary-hover-text);
  box-shadow: var(--shadow-sm);
}

.action-btn.enhance:hover:not(:disabled) {
  background: linear-gradient(135deg, var(--button-secondary-hover-start), var(--button-secondary-hover-end));
  color: var(--button-secondary-hover-text);
  box-shadow: var(--shadow-sm);
}

.action-btn.identify:hover:not(:disabled),
.action-btn.reforge:hover:not(:disabled),
.action-btn.donate:hover:not(:disabled),
.action-btn.bind:hover:not(:disabled),
.action-btn.sell:hover:not(:disabled),
.action-btn.drop:hover:not(:disabled) {
  background: var(--button-outline-hover-bg);
}

.action-btn.identify:hover:not(:disabled),
.action-btn.reforge:hover:not(:disabled),
.action-btn.donate:hover:not(:disabled) {
  border-color: var(--button-outline-hover-border);
  color: var(--button-outline-hover-text);
}

.action-btn.bind:hover:not(:disabled) {
  border-color: var(--highlight-text);
  color: var(--highlight-text-strong);
}

.action-btn.gift:hover:not(:disabled) {
  border-color: #e91e8c;
  color: #e91e8c;
  background: rgba(233, 30, 140, 0.1);
}

.action-btn.sell:hover:not(:disabled),
.action-btn.drop:hover:not(:disabled) {
  border-color: var(--status-danger-text);
  color: var(--status-danger-text);
}

.popover-footer-hint {
  font-size: var(--font-size-xs);
  line-height: 1.5;
  color: var(--text-muted);
}

/* 洗练词条内联标记 */
.popover-stat-line.has-reroll {
  padding-left: 3px;
  border-left: 3px solid var(--border-color);
}
.popover-stat-right {
  display: flex;
  align-items: center;
  gap: 6px;
}
.popover-stat-tier {
  font-size: 10px;
  font-weight: 500;
  opacity: 0.85;
}

/* 悬浮窗宝石镶嵌 */
.popover-gem-slots {
  margin-top: 8px;
  padding-top: 8px;
  border-top: 1px dashed rgba(255, 255, 255, 0.12);
}
.popover-gem-title {
  font-size: 12px;
  font-weight: 600;
  color: #a78bfa;
  margin-bottom: 6px;
  display: flex;
  align-items: center;
  gap: 4px;
}
.popover-gem-title::before {
  content: '';
  display: inline-block;
  width: 3px;
  height: 12px;
  background: linear-gradient(180deg, #a78bfa, #7c3aed);
  border-radius: 2px;
}
.popover-gem-line {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 12px;
  color: rgba(255, 255, 255, 0.85);
  padding: 2px 0 2px 6px;
}
.popover-gem-name {
  color: #c4b5fd;
  font-weight: 500;
}
.popover-gem-bonus {
  color: #34d399;
  font-size: 11px;
  margin-left: auto;
}

/* 锻造按钮 */
.action-btn.forge {
  background: linear-gradient(135deg, #f59e0b, #d97706);
  color: #fff;
  border-color: #f59e0b;
}
.action-btn.forge:hover {
  background: linear-gradient(135deg, #d97706, #b45309);
}

/* 背包容量 */
.inventory-capacity {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  padding: var(--spacing-sm) var(--spacing-md);
  background: var(--xiuxian-bg-secondary);
  border-radius: var(--radius-md);
}

.capacity-bar {
  flex: 1;
  height: 8px;
  background: var(--xiuxian-bg-primary);
  border-radius: 4px;
  overflow: hidden;
}

.capacity-fill {
  height: 100%;
  background: var(--button-primary-start);
  border-radius: 4px;
  transition: width 0.3s ease;
}

.capacity-text {
  font-size: var(--font-size-base);
  color: var(--text-secondary);
  font-family: var(--font-mono);
}

@media (max-width: 720px) {
  .inventory-popover-teleport,
  .inventory-popover-teleport.props,
  .inventory-popover-teleport.equipment.with-compare {
    width: min(420px, calc(100vw - 24px));
  }

  .popover-header,
  .popover-identity {
    align-items: flex-start;
  }

  .action-btn {
    flex: 1 1 calc(50% - var(--spacing-sm));
  }

  .compare-columns {
    grid-template-columns: 1fr;
  }

}

/* 图鉴样式 */
.collection-panel {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
  padding: var(--spacing-xs);
}
.collection-bonuses {
  background: var(--bg-overlay-medium);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  padding: var(--spacing-sm) var(--spacing-md);
}
.collection-bonuses h4 {
  margin: 0 0 var(--spacing-xs);
  color: var(--accent-text);
  font-size: var(--font-size-sm);
  font-weight: 600;
}
.bonus-list { display: flex; flex-wrap: wrap; gap: var(--spacing-xs); }
.bonus-tag {
  background: var(--accent-soft-bg);
  color: var(--text-secondary);
  padding: 2px 8px;
  border: 1px solid var(--border-color);
  border-radius: var(--radius-sm);
  font-size: var(--font-size-xs);
}
.collection-series {
  background: var(--bg-overlay-medium);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  padding: var(--spacing-sm) var(--spacing-md);
}
.series-header {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  margin-bottom: var(--spacing-sm);
}
.series-name {
  color: var(--text-primary);
  font-weight: 600;
  font-size: var(--font-size-sm);
}
.series-progress {
  color: var(--text-muted);
  font-size: var(--font-size-xs);
  font-family: var(--font-mono);
}
.complete-badge {
  background: var(--quality-uncommon);
  color: #000;
  font-size: 10px;
  font-weight: 700;
  padding: 1px 6px;
  border-radius: var(--radius-sm);
}
.collection-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(48px, 1fr));
  gap: var(--spacing-xs);
}
.collection-slot {
  background: var(--xiuxian-bg-secondary);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-sm);
  aspect-ratio: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  position: relative;
  font-size: var(--font-size-xs);
  color: var(--text-muted);
  transition: border-color 0.2s ease, background 0.2s ease;
}
.collection-slot.owned {
  border-color: var(--accent-text);
  color: var(--text-primary);
  background: var(--bg-overlay-light);
}
.slot-char { font-size: 16px; font-weight: 700; }
.slot-thumb { width: 100%; height: 100%; object-fit: cover; border-radius: 4px; }
.slot-count {
  position: absolute;
  bottom: 2px;
  right: 4px;
  font-size: 9px;
  color: var(--text-secondary);
  font-family: var(--font-mono);
}
.series-bonus-tag {
  font-size: 10px;
  padding: 1px 6px;
  border-radius: var(--radius-sm);
  background: var(--bg-overlay-light);
  color: var(--text-muted);
  white-space: nowrap;
}
.series-bonus-tag.active {
  background: rgba(34, 197, 94, 0.1);
  color: var(--quality-uncommon);
}
</style>

