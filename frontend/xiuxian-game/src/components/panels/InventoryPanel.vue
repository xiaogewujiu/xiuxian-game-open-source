<template>
  <!-- 背包系统面板 -->
  <div class="inventory-panel">
    <!-- 背包类型切换 -->
    <div class="inventory-tabs">
      <button
        v-for="tab in inventoryTabs"
        :key="tab.key"
        class="tab-btn"
        :class="{ active: currentTab === tab.key }"
        @click="currentTab = tab.key"
      >
        <span class="tab-icon"><AssetIcon :source="tab.icon" size="25" /></span>
        <span class="tab-name">{{ tab.name }}</span>
        <span class="tab-count">{{ getItemCount(tab.key) }}</span>
      </button>
    </div>

    <!-- 筛选栏 -->
    <div class="filter-bar">
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

    <!-- 背包内容 -->
    <div class="inventory-content">
      <!-- 装备背包 -->
      <div v-if="currentTab === 'equipment'" class="equipment-grid">
        <div
          v-for="item in filteredEquipment"
          :key="item.id"
          class="equipment-item"
          :class="[item.quality, { selected: selectedItem?.id === item.id }]"
          @click="selectItem(item)"
        >
          <div class="item-icon"><AssetIcon :source="item.icon" size="28" /></div>
          <div class="item-level">+{{ item.enhance }}</div>
          <div v-if="item.isNew" class="new-badge">新</div>
        </div>

        <!-- 空格 -->
        <div
          v-for="n in equipmentMax - equipmentList.length"
          :key="'empty-'+n"
          class="equipment-item empty"
        >
          <span class="empty-icon">+</span>
        </div>
      </div>

      <!-- 道具背包 -->
      <div v-else class="props-grid">
        <div
          v-for="item in filteredProps"
          :key="item.id"
          class="prop-item"
          :class="[item.quality, { selected: selectedItem?.id === item.id }]"
          @click="selectItem(item)"
        >
          <div class="item-icon"><AssetIcon :source="item.icon" size="28" /></div>
          <div class="item-count">{{ item.count }}</div>
          <div v-if="item.isNew" class="new-badge">新</div>
        </div>

        <div
          v-for="n in propsMax - propsList.length"
          :key="'empty-'+n"
          class="prop-item empty"
        >
          <span class="empty-icon">+</span>
        </div>
      </div>
    </div>

    <!-- 物品详情 -->
    <div v-if="selectedItem" class="item-detail">
      <div class="detail-header">
        <div class="detail-icon"><AssetIcon :source="selectedItem.icon" size="48" /></div>
        <div class="detail-info">
          <div class="detail-name" :class="selectedItem.quality">{{ selectedItem.name }}</div>
          <div class="detail-type">
            {{ selectedItem.type }}
            <span v-if="selectedItem.element" :class="selectedItem.element">
              [{{ getElementName(selectedItem.element) }}]
            </span>
          </div>
        </div>
      </div>

      <div class="detail-stats" v-if="selectedItem.stats">
        <div v-for="stat in selectedItem.stats" :key="stat.name" class="stat-line">
          <span>{{ stat.name }}:</span>
          <span :class="{ positive: stat.value > 0, negative: stat.value < 0 }">
            {{ stat.value > 0 ? '+' : '' }}{{ stat.value }}
          </span>
        </div>
      </div>

      <div class="detail-description" v-if="selectedItem.description">
        {{ selectedItem.description }}
      </div>

      <!-- 操作按钮 -->
      <div class="detail-actions">
        <template v-if="currentTab === 'equipment'">
          <button class="action-btn equip" @click="equipItem">穿戴</button>
          <button class="action-btn enhance" @click="enhanceItem">强化</button>
          <button class="action-btn identify" @click="identifyItem" v-if="!selectedItem.identified">鉴定</button>
          <button class="action-btn reforge" @click="reforgeItem">洗练</button>
          <button class="action-btn donate" @click="donateItem">捐献</button>
          <button class="action-btn bind" @click="bindItem">{{ selectedItem.isLocked ? '解锁' : '锁定' }}</button>
          <button class="action-btn sell" @click="sellItem">出售</button>
        </template>
        <template v-else>
          <button class="action-btn use" @click="useItem">使用</button>
          <button class="action-btn drop" @click="dropItem">丢弃</button>
        </template>
      </div>
    </div>

    <!-- 背包容量 -->
    <div class="inventory-capacity">
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
</template>

<script>
import { ref, reactive, computed } from 'vue'
import toast from '@/utils/toast'
import { ICON } from '../../icons'
import { confirm as showConfirm } from '@/utils/confirm'
import AssetIcon from '../common/AssetIcon.vue'

/**
 * InventoryPanel - 背包系统面板
 *
 * 功能：
 * - 装备背包：网格展示、品级筛选、五行筛选、搜索、操作按钮（穿戴、强化、鉴定、洗练、捐献、锁定/解锁、卖出）
 * - 道具背包：网格展示、使用、丢弃
 */
export default {
  name: 'InventoryPanel',
  components: { AssetIcon },

  setup() {
    // 背包类型
    const currentTab = ref('equipment')
    const inventoryTabs = [
      { key: 'equipment', name: '装备', icon: ICON.misc_sword_crossed },
      { key: 'props', name: '道具', icon: ICON.item_chest }
    ]

    // 容量上限
    const equipmentMax = 100
    const propsMax = 100

    // 筛选条件
    const filters = reactive({
      quality: '',
      element: '',
      search: ''
    })

    // 装备列表（模拟数据）
    const equipmentList = reactive([
      { id: 1, name: '青锋剑', icon: ICON.misc_dagger, quality: 'rare', type: '武器', element: 'metal', enhance: 8, level: 45, isNew: false, isLocked: true, identified: true, stats: [{ name: '攻击', value: 320 }, { name: '暴击', value: 15 }], description: '青云门制式飞剑，锋利无比' },
      { id: 2, name: '玄铁甲', icon: ICON.misc_shield, quality: 'uncommon', type: '护甲', element: 'earth', enhance: 5, level: 40, isNew: false, isLocked: true, identified: true, stats: [{ name: '防御', value: 280 }, { name: '生命', value: 500 }], description: '以玄铁打造的护甲' },
      { id: 3, name: '灵风头巾', icon: ICON.ui_mask, quality: 'rare', type: '头盔', element: 'wind', enhance: 6, level: 42, isNew: true, isLocked: false, identified: false, stats: [{ name: '法力', value: 100 }, { name: '回复', value: 10 }], description: '未鉴定的神秘头巾' },
      { id: 4, name: '烈焰戒指', icon: ICON.slot_ring, quality: 'epic', type: '戒指', element: 'fire', enhance: 4, level: 45, isNew: true, isLocked: false, identified: true, stats: [{ name: '攻击', value: 150 }, { name: '火伤', value: 20 }], description: '蕴含火焰之力的戒指' },
      { id: 5, name: '寒冰护符', icon: ICON.slot_necklace, quality: 'legendary', type: '项链', element: 'water', enhance: 3, level: 50, isNew: true, isLocked: false, identified: true, stats: [{ name: '防御', value: 200 }, { name: '冰抗', value: 30 }], description: '万年寒冰制成的护符' },
      { id: 6, name: '疾风靴', icon: ICON.slot_boots, quality: 'rare', type: '鞋子', element: 'wind', enhance: 7, level: 43, isNew: false, isLocked: true, identified: true, stats: [{ name: '速度', value: 50 }, { name: '闪避', value: 15 }], description: '穿上后身轻如燕' }
    ])

    // 道具列表（模拟数据）
    const propsList = reactive([
      { id: 101, name: '回血丹', icon: ICON.item_heart_red, quality: 'common', type: '药品', count: 50, isNew: false, description: '恢复500点生命值' },
      { id: 102, name: '回蓝丹', icon: ICON.misc_water_drop, quality: 'common', type: '药品', count: 30, isNew: false, description: '恢复200点法力值' },
      { id: 103, name: '修为丹', icon: ICON.misc_sparkles, quality: 'rare', type: '丹药', count: 10, isNew: true, description: '增加1000点修为' },
      { id: 104, name: '传送符', icon: ICON.item_scroll, quality: 'uncommon', type: '符箓', count: 20, isNew: false, description: '可传送至已激活的传送点' },
      { id: 105, name: '强化石', icon: ICON.misc_diamond, quality: 'uncommon', type: '材料', count: 100, isNew: false, description: '用于强化装备' },
      { id: 106, name: '洗练石', icon: ICON.stat_combo, quality: 'rare', type: '材料', count: 15, isNew: true, description: '用于洗练装备属性' }
    ])

    // 选中的物品
    const selectedItem = ref(null)

    // 五行名称映射
    const elementNames = {
      metal: '金',
      wood: '木',
      water: '水',
      fire: '火',
      earth: '土'
    }

    // 获取五行名称
    const getElementName = (element) => elementNames[element] || element

    // 获取物品数量
    const getItemCount = (tab) => {
      return tab === 'equipment' ? equipmentList.length : propsList.length
    }

    // 筛选后的装备
    const filteredEquipment = computed(() => {
      return equipmentList.filter(item => {
        if (filters.quality && item.quality !== filters.quality) return false
        if (filters.element && item.element !== filters.element) return false
        if (filters.search && !item.name.includes(filters.search)) return false
        return true
      })
    })

    // 筛选后的道具
    const filteredProps = computed(() => {
      return propsList.filter(item => {
        if (filters.quality && item.quality !== filters.quality) return false
        if (filters.search && !item.name.includes(filters.search)) return false
        return true
      })
    })

    // 选择物品
    const selectItem = (item) => {
      selectedItem.value = item
    }

    // 重置筛选
    const resetFilters = () => {
      filters.quality = ''
      filters.element = ''
      filters.search = ''
    }

    // 装备操作
    const equipItem = () => toast.info(`穿戴 ${selectedItem.value.name}`)
    const enhanceItem = () => toast.info(`强化 ${selectedItem.value.name}`)
    const identifyItem = () => {
      selectedItem.value.identified = true
      toast.success(`鉴定成功！${selectedItem.value.name} 已鉴定`)
    }
    const reforgeItem = () => toast.info(`洗练 ${selectedItem.value.name}`)
    const donateItem = () => toast.info(`捐献 ${selectedItem.value.name}`)
    const bindItem = () => {
      selectedItem.value.isLocked = !selectedItem.value.isLocked
      toast.success(`${selectedItem.value.name} 已${selectedItem.value.isLocked ? '锁定' : '解锁'}`)
    }
    const sellItem = async () => {
      if (await showConfirm(`确定要出售 ${selectedItem.value.name} 吗？`)) {
        const index = equipmentList.findIndex(i => i.id === selectedItem.value.id)
        if (index > -1) {
          equipmentList.splice(index, 1)
          selectedItem.value = null
        }
      }
    }

    // 道具操作
    const useItem = () => toast.info(`使用 ${selectedItem.value.name}`)
    const dropItem = async () => {
      if (await showConfirm(`确定要丢弃 ${selectedItem.value.name} 吗？`, { type: 'danger' })) {
        const index = propsList.findIndex(i => i.id === selectedItem.value.id)
        if (index > -1) {
          propsList.splice(index, 1)
          selectedItem.value = null
        }
      }
    }

    return {
      ICON,
      currentTab,
      inventoryTabs,
      equipmentMax,
      propsMax,
      filters,
      equipmentList,
      propsList,
      selectedItem,
      getItemCount,
      getElementName,
      filteredEquipment,
      filteredProps,
      selectItem,
      resetFilters,
      equipItem,
      enhanceItem,
      identifyItem,
      reforgeItem,
      donateItem,
      bindItem,
      sellItem,
      useItem,
      dropItem
    }
  }
}
</script>

<style scoped>
.inventory-panel {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
  padding: var(--spacing-md);
  height: 100%;
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
  color: var(--text-primary);
}

.tab-btn.active {
  background: rgba(124, 58, 237, 0.2);
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
  border-color: var(--border-color);
  color: var(--accent-text);
}

/* 背包内容 */
.inventory-content {
  flex: 1;
  overflow-y: auto;
  background: var(--xiuxian-bg-panel);
  border-radius: var(--radius-md);
  padding: var(--spacing-md);
}

/* 装备网格 */
.equipment-grid,
.props-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(64px, 1fr));
  gap: var(--spacing-sm);
}

.equipment-item,
.prop-item {
  aspect-ratio: 1;
  background: var(--xiuxian-bg-secondary);
  border: 2px solid var(--border-color);
  border-radius: var(--radius-sm);
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.2s ease;
  position: relative;
}

.equipment-item:hover,
.prop-item:hover {
  transform: scale(1.05);
  border-color: var(--accent-text);
}

.equipment-item.selected,
.prop-item.selected {
  border-color: var(--highlight-text);
  box-shadow: var(--shadow-sm);
}

.equipment-item.empty,
.prop-item.empty {
  border-style: dashed;
  opacity: 0.5;
}

.equipment-item.common { border-color: var(--quality-common); }
.equipment-item.uncommon { border-color: var(--quality-uncommon); }
.equipment-item.rare { border-color: var(--quality-rare); }
.equipment-item.epic { border-color: var(--quality-epic); }
.equipment-item.legendary { border-color: var(--quality-legendary); }
.equipment-item.mythic { border-color: var(--quality-mythic); }

.prop-item.common { border-color: var(--quality-common); }
.prop-item.uncommon { border-color: var(--quality-uncommon); }
.prop-item.rare { border-color: var(--quality-rare); }
.prop-item.epic { border-color: var(--quality-epic); }

.item-icon {
  font-size: 28px;
}

.item-level,
.item-count {
  position: absolute;
  bottom: 2px;
  right: 2px;
  font-size: var(--font-size-xs);
  font-weight: 600;
  color: var(--highlight-text);
  background: var(--bg-overlay-dark);
  padding: 1px 4px;
  border-radius: 3px;
}

.new-badge {
  position: absolute;
  top: 2px;
  left: 2px;
  font-size: var(--font-size-xs);
  padding: 1px 4px;
  background: var(--hp-color);
  border-radius: 3px;
  color: var(--text-on-dark);
}

.empty-icon {
  font-size: 24px;
  color: var(--text-muted);
  opacity: 0.5;
}

/* 物品详情 */
.item-detail {
  background: var(--xiuxian-bg-secondary);
  border-radius: var(--radius-md);
  padding: var(--spacing-md);
  border: 1px solid var(--border-color);
}

.detail-header {
  display: flex;
  gap: var(--spacing-md);
  margin-bottom: var(--spacing-md);
  padding-bottom: var(--spacing-md);
  border-bottom: 1px solid var(--border-color);
}

.detail-icon {
  font-size: 48px;
}

.detail-name {
  font-size: 16px;
  font-weight: 600;
}

.detail-name.common { color: var(--quality-common); }
.detail-name.uncommon { color: var(--quality-uncommon); }
.detail-name.rare { color: var(--quality-rare); }
.detail-name.epic { color: var(--quality-epic); }
.detail-name.legendary { color: var(--quality-legendary); }
.detail-name.mythic { color: var(--quality-mythic); }

.detail-type {
  font-size: var(--font-size-base);
  color: var(--text-muted);
  margin-top: 4px;
}

.detail-type .metal { color: var(--element-metal); }
.detail-type .wood { color: var(--element-wood); }
.detail-type .water { color: var(--element-water); }
.detail-type .fire { color: var(--element-fire); }
.detail-type .earth { color: var(--element-earth); }

.detail-stats {
  margin-bottom: var(--spacing-md);
}

.stat-line {
  display: flex;
  justify-content: space-between;
  font-size: var(--font-size-base);
  padding: var(--spacing-xs) 0;
}

.stat-line span:first-child {
  color: var(--text-secondary);
}

.stat-line .positive {
  color: var(--quality-uncommon);
}

.stat-line .negative {
  color: var(--hp-color);
}

.detail-description {
  font-size: var(--font-size-base);
  color: var(--text-muted);
  font-style: italic;
  padding: var(--spacing-sm);
  background: rgba(124, 58, 237, 0.1);
  border-radius: var(--radius-sm);
  margin-bottom: var(--spacing-md);
}

/* 操作按钮 */
.detail-actions {
  display: flex;
  flex-wrap: wrap;
  gap: var(--spacing-sm);
}

.action-btn {
  padding: var(--spacing-sm) var(--spacing-md);
  border: none;
  border-radius: var(--radius-sm);
  font-size: var(--font-size-base);
  cursor: pointer;
  transition: all 0.15s ease;
}

.action-btn.equip {
  background: oklch(0.6 0.15 155 / 0.15);
  border: 1px solid oklch(0.6 0.15 155 / 0.3);
  color: #4ade80;
}

.action-btn.enhance {
  background: oklch(0.65 0.15 80 / 0.15);
  border: 1px solid oklch(0.65 0.15 80 / 0.3);
  color: #fbbf24;
}

.action-btn.identify {
  background: oklch(0.55 0.18 300 / 0.15);
  border: 1px solid oklch(0.55 0.18 300 / 0.3);
  color: #c084fc;
}

.action-btn.reforge {
  background: oklch(0.6 0.12 250 / 0.15);
  border: 1px solid oklch(0.6 0.12 250 / 0.3);
  color: #60a5fa;
}

.action-btn.donate {
  background: var(--xiuxian-bg-primary);
  border: 1px solid var(--border-color);
  color: var(--accent-text);
}

.action-btn.bind {
  background: var(--xiuxian-bg-primary);
  border: 1px solid var(--border-color);
  color: var(--highlight-text);
}

.action-btn.sell,
.action-btn.drop {
  background: transparent;
  border: 1px solid var(--hp-color);
  color: var(--hp-color);
}

.action-btn.use {
  background: oklch(0.6 0.15 155 / 0.15);
  border: 1px solid oklch(0.6 0.15 155 / 0.3);
  color: #4ade80;
}

.action-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 8px var(--shadow-sm);
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
  transition: width 0.15s ease;
}

.capacity-text {
  font-size: var(--font-size-base);
  color: var(--text-secondary);
  font-family: var(--font-mono);
}
</style>
