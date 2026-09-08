<template>
  <!-- 好感度系统 Modal -->
  <XiuXianModal :model-value="modelValue" title="好感度" :width="600" @update:model-value="$emit('update:modelValue', $event)">
    <div class="favorability-modal">
      <!-- 标签页切换 -->
      <div class="tab-row">
        <button
          v-for="tab in tabs"
          :key="tab.key"
          class="tab-btn"
          :class="{ active: currentTab === tab.key }"
          @click="switchTab(tab.key)"
        >
          <span class="tab-icon"><AssetIcon :source="tab.icon" size="20" /></span>
          <span>{{ tab.label }}</span>
        </button>
      </div>

      <!-- 加载状态 -->
      <div v-if="isLoading" class="loading-state">
        <span class="loading-text">加载中...</span>
      </div>

      <!-- Tab 1: 我对别人的好感度 -->
      <template v-else-if="currentTab === 'toOthers'">
        <div v-if="favorabilityList.length === 0" class="empty-state">
          <div class="empty-icon"><AssetIcon :source="ICON.misc_heart_red" size="30" /></div>
          <div class="empty-text">暂无好感度数据。</div>
        </div>

        <div v-else class="favor-list">
          <div
            v-for="item in favorabilityList"
            :key="item.playerId"
            class="favor-item"
          >
            <div class="favor-main" @click="toggleExpand(item.playerId)">
              <div class="favor-left">
                <div class="favor-avatar">
                  <img v-if="item.avatarPath" :src="item.avatarPath" :alt="item.playerName" class="avatar-img" />
                  <span v-else class="avatar-placeholder">{{ item.playerName?.charAt(0) || '?' }}</span>
                </div>
                <div class="favor-info">
                  <div class="favor-name">{{ item.playerName }}</div>
                  <div class="favor-value">
                    <span class="value-number">{{ item.value }}</span>
                    <span class="value-label">好感度</span>
                  </div>
                </div>
              </div>
              <div class="favor-right">
                <span
                  class="level-tag"
                  :style="{ backgroundColor: item.levelColor || 'var(--accent-text)', color: '#fff' }"
                >
                  {{ item.levelName || '未知' }}
                </span>
                <span class="expand-icon" :class="{ expanded: expandedId === item.playerId }">›</span>
              </div>
            </div>

            <!-- 展开区域：最近5条赠送记录 -->
            <div v-if="expandedId === item.playerId" class="favor-expand">
              <div v-if="item.recentGifts && item.recentGifts.length > 0" class="recent-gifts">
                <div class="expand-title">最近赠送记录</div>
                <div
                  v-for="(gift, index) in item.recentGifts.slice(0, 5)"
                  :key="index"
                  class="gift-record"
                >
                  <span class="gift-name">{{ gift.itemName || gift.itemId }}</span>
                  <span class="gift-change">+{{ gift.favorabilityChange }}</span>
                  <span class="gift-time">{{ formatTime(gift.giftTime) }}</span>
                </div>
              </div>
              <div v-else class="no-gifts">暂无赠送记录</div>

              <div class="expand-actions">
                <button class="gift-btn" @click.stop="openGiftModal(item)">
                  <AssetIcon :source="ICON.misc_gift" size="16" />
                  <span>赠送</span>
                </button>
              </div>
            </div>
          </div>
        </div>
      </template>

      <!-- Tab 2: 别人对我的好感度 -->
      <template v-else-if="currentTab === 'fromOthers'">
        <div v-if="favorabilityFromOthers.length === 0" class="empty-state">
          <div class="empty-icon"><AssetIcon :source="ICON.misc_heart_green" size="30" /></div>
          <div class="empty-text">暂无他人好感度数据。</div>
        </div>

        <div v-else class="favor-list">
          <div
            v-for="item in favorabilityFromOthers"
            :key="item.playerId"
            class="favor-item"
          >
            <div class="favor-main" @click="toggleExpand(item.playerId)">
              <div class="favor-left">
                <div class="favor-avatar">
                  <img v-if="item.avatarPath" :src="item.avatarPath" :alt="item.playerName" class="avatar-img" />
                  <span v-else class="avatar-placeholder">{{ item.playerName?.charAt(0) || '?' }}</span>
                </div>
                <div class="favor-info">
                  <div class="favor-name">{{ item.playerName }}</div>
                  <div class="favor-value">
                    <span class="value-number">{{ item.value }}</span>
                    <span class="value-label">好感度</span>
                  </div>
                </div>
              </div>
              <div class="favor-right">
                <span
                  class="level-tag"
                  :style="{ backgroundColor: item.levelColor || 'var(--accent-text)', color: '#fff' }"
                >
                  {{ item.levelName || '未知' }}
                </span>
                <span class="expand-icon" :class="{ expanded: expandedId === item.playerId }">›</span>
              </div>
            </div>

            <!-- 展开区域：最近5条赠送记录 -->
            <div v-if="expandedId === item.playerId" class="favor-expand">
              <div v-if="item.recentGifts && item.recentGifts.length > 0" class="recent-gifts">
                <div class="expand-title">最近赠送记录</div>
                <div
                  v-for="(gift, index) in item.recentGifts.slice(0, 5)"
                  :key="index"
                  class="gift-record"
                >
                  <span class="gift-name">{{ gift.itemName || gift.itemId }}</span>
                  <span class="gift-change">+{{ gift.favorabilityChange }}</span>
                  <span class="gift-time">{{ formatTime(gift.giftTime) }}</span>
                </div>
              </div>
              <div v-else class="no-gifts">暂无赠送记录</div>
            </div>
          </div>
        </div>
      </template>

      <!-- Tab 3: 赠送记录 -->
      <template v-else-if="currentTab === 'log'">
        <div v-if="favorabilityLog.length === 0" class="empty-state">
          <div class="empty-icon"><AssetIcon :source="ICON.misc_gift" size="30" /></div>
          <div class="empty-text">暂无赠送记录。</div>
        </div>

        <div v-else class="log-timeline">
          <div
            v-for="(record, index) in paginatedLog"
            :key="index"
            class="log-item"
          >
            <div class="log-dot"></div>
            <div class="log-content">
              <div class="log-header">
                <span class="log-player">{{ record.senderName || '我' }} → {{ record.receiverName || '对方' }}</span>
                <span class="log-time">{{ formatTime(record.giftTime) }}</span>
              </div>
              <div class="log-detail">
                <span class="log-item-name">{{ record.itemName || record.itemId }}</span>
                <span class="log-change">好感度 +{{ record.favorabilityChange }}</span>
              </div>
            </div>
          </div>

          <!-- 分页 -->
          <div v-if="totalLogPages > 1" class="pagination">
            <button
              class="page-btn"
              :disabled="currentLogPage <= 1"
              @click="currentLogPage--"
            >
              ‹
            </button>
            <span class="page-info">{{ currentLogPage }} / {{ totalLogPages }}</span>
            <button
              class="page-btn"
              :disabled="currentLogPage >= totalLogPages"
              @click="currentLogPage++"
            >
              ›
            </button>
          </div>
        </div>
      </template>

      <!-- 状态消息 -->
      <div v-if="statusMessage" class="status-message">{{ statusMessage }}</div>
    </div>
  </XiuXianModal>
</template>

<script>
import { ref, computed, watch } from 'vue'
import XiuXianModal from '../common/XiuXianModal.vue'
import AssetIcon from '../common/AssetIcon.vue'
import { useGameStore } from '../../state/gameStore'
import { ICON } from '../../icons'

/**
 * FavorabilityModal - 好感度面板弹窗
 *
 * 功能：
 * - 三个标签页：我对别人的好感度、别人对我的好感度、赠送记录
 * - 列表展示头像、名称、好感度值、等级标签
 * - 展开显示最近5条赠送记录
 * - Tab1 支持赠送按钮，打开 GiftFavorabilityModal
 * - Tab3 支持分页浏览赠送记录
 */
export default {
  name: 'FavorabilityModal',

  components: {
    XiuXianModal,
    AssetIcon
  },

  props: {
    modelValue: Boolean
  },

  emits: ['update:modelValue', 'openGiftModal'],

  setup(props, { emit }) {
    const gameStore = useGameStore()
    const isLoading = ref(false)
    const statusMessage = ref('')
    const currentTab = ref('toOthers')
    const expandedId = ref(null)
    const currentLogPage = ref(1)
    const logPageSize = 20

    const tabs = [
      { key: 'toOthers', label: '我对别人的好感度', icon: ICON.misc_heart_red },
      { key: 'fromOthers', label: '别人对我的好感度', icon: ICON.misc_heart_green },
      { key: 'log', label: '赠送记录', icon: ICON.misc_gift }
    ]

    // 从 gameStore 获取数据
    const favorabilityList = computed(() => gameStore.state.favorabilityList || [])
    const favorabilityFromOthers = computed(() => gameStore.state.favorabilityFromOthers || [])
    const favorabilityLog = computed(() => gameStore.state.favorabilityLog || [])

    // 分页计算
    const totalLogPages = computed(() => {
      return Math.max(1, Math.ceil(favorabilityLog.value.length / logPageSize))
    })

    const paginatedLog = computed(() => {
      const start = (currentLogPage.value - 1) * logPageSize
      return favorabilityLog.value.slice(start, start + logPageSize)
    })

    // 切换标签页
    function switchTab(tabKey) {
      currentTab.value = tabKey
      expandedId.value = null
      currentLogPage.value = 1
    }

    // 展开/收起详情
    function toggleExpand(playerId) {
      expandedId.value = expandedId.value === playerId ? null : playerId
    }

    // 格式化时间
    function formatTime(timeStr) {
      if (!timeStr) return ''
      const date = new Date(timeStr)
      if (Number.isNaN(date.getTime())) return String(timeStr)
      return date.toLocaleString('zh-CN', { hour12: false })
    }

    // 打开赠送弹窗
    function openGiftModal(item) {
      emit('openGiftModal', item)
    }

    // 加载数据
    async function loadData() {
      isLoading.value = true
      statusMessage.value = ''

      try {
        // 并行加载所有好感度数据
        await Promise.all([
          gameStore.loadFavorabilityList('ToOthers'),
          gameStore.loadFavorabilityList('FromOthers'),
          gameStore.loadFavorabilityLog(),
          gameStore.loadFavorabilityLevels()
        ])
      } catch (error) {
        statusMessage.value = error.message || '加载好感度数据失败。'
      } finally {
        isLoading.value = false
      }
    }

    // 监听弹窗打开，懒加载数据
    watch(() => props.modelValue, async (visible) => {
      if (!visible) return

      currentTab.value = 'toOthers'
      expandedId.value = null
      currentLogPage.value = 1
      await loadData()
    }, { immediate: true })

    return {
      ICON,
      tabs,
      currentTab,
      isLoading,
      statusMessage,
      expandedId,
      currentLogPage,
      totalLogPages,
      paginatedLog,
      favorabilityList,
      favorabilityFromOthers,
      favorabilityLog,
      switchTab,
      toggleExpand,
      formatTime,
      openGiftModal,
      loadData
    }
  }
}
</script>

<style scoped>
.favorability-modal {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
  color: var(--text-primary);
}

/* 标签页 */
.tab-row {
  display: flex;
  flex-wrap: wrap;
  gap: var(--spacing-sm);
}

.tab-btn {
  display: inline-flex;
  align-items: center;
  gap: var(--spacing-xs);
  padding: var(--spacing-sm) var(--spacing-md);
  background: transparent;
  border: 1px solid var(--border-color);
  border-radius: 999px;
  color: var(--text-secondary);
  cursor: pointer;
  transition: all 0.25s ease;
}

.tab-btn:hover,
.tab-btn.active {
  background: var(--accent-soft-bg);
  border-color: var(--accent-text);
  color: var(--text-primary);
  box-shadow: var(--shadow-sm);
}

.tab-icon {
  display: flex;
  align-items: center;
}

/* 加载状态 */
.loading-state {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 200px;
}

.loading-text {
  color: var(--text-secondary);
  font-size: var(--font-size-base);
}

/* 空状态 */
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 200px;
  gap: var(--spacing-sm);
  color: var(--text-secondary);
}

.empty-icon {
  font-size: 30px;
}

.empty-text {
  font-size: var(--font-size-base);
}

/* 好感度列表 */
.favor-list {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
  max-height: 50vh;
  overflow-y: auto;
}

.favor-item {
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  overflow: hidden;
}

.favor-main {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--spacing-md);
  cursor: pointer;
  transition: background 0.15s ease;
}

.favor-main:hover {
  background: rgba(124, 58, 237, 0.08);
}

.favor-left {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
}

.favor-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  overflow: hidden;
  background: var(--xiuxian-bg-secondary);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.avatar-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.avatar-placeholder {
  font-size: 18px;
  font-weight: 600;
  color: var(--text-primary);
}

.favor-info {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.favor-name {
  font-size: var(--font-size-base);
  font-weight: 600;
  color: var(--text-primary);
}

.favor-value {
  display: flex;
  align-items: baseline;
  gap: 4px;
}

.value-number {
  font-size: var(--font-size-md);
  font-weight: 700;
  color: var(--highlight-text);
}

.value-label {
  font-size: var(--font-size-sm);
  color: var(--text-secondary);
}

.favor-right {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
}

.level-tag {
  display: inline-flex;
  align-items: center;
  padding: 4px 10px;
  border-radius: 999px;
  font-size: var(--font-size-sm);
  white-space: nowrap;
}

.expand-icon {
  font-size: 18px;
  color: var(--text-secondary);
  transition: transform 0.2s ease;
}

.expand-icon.expanded {
  transform: rotate(90deg);
}

/* 展开区域 */
.favor-expand {
  padding: 0 var(--spacing-md) var(--spacing-md);
  border-top: 1px solid var(--border-color);
  background: rgba(0, 0, 0, 0.1);
}

.expand-title {
  font-size: var(--font-size-sm);
  font-weight: 600;
  color: var(--accent-text);
  margin-top: var(--spacing-sm);
  margin-bottom: var(--spacing-xs);
}

.recent-gifts {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-xs);
}

.gift-record {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--spacing-xs) var(--spacing-sm);
  background: rgba(255, 255, 255, 0.03);
  border-radius: var(--radius-sm);
  font-size: var(--font-size-sm);
}

.gift-name {
  color: var(--text-primary);
  flex: 1;
}

.gift-change {
  color: var(--status-success-text);
  font-weight: 600;
  margin: 0 var(--spacing-md);
}

.gift-time {
  color: var(--text-muted);
  font-size: var(--font-size-xs);
}

.no-gifts {
  padding: var(--spacing-sm);
  color: var(--text-muted);
  font-size: var(--font-size-sm);
  text-align: center;
}

.expand-actions {
  display: flex;
  justify-content: flex-end;
  margin-top: var(--spacing-sm);
}

.gift-btn {
  display: inline-flex;
  align-items: center;
  gap: var(--spacing-xs);
  padding: var(--spacing-xs) var(--spacing-md);
  background: var(--button-primary-start);
  border: none;
  border-radius: var(--radius-sm);
  color: var(--text-on-dark);
  font-size: var(--font-size-sm);
  font-weight: 600;
  cursor: pointer;
  transition: all 0.15s ease;
}

.gift-btn:hover {
  box-shadow: var(--shadow-sm);
  transform: translateY(-1px);
}

/* 赠送记录时间线 */
.log-timeline {
  display: flex;
  flex-direction: column;
  gap: 0;
  max-height: 50vh;
  overflow-y: auto;
  position: relative;
  padding-left: var(--spacing-lg);
}

.log-timeline::before {
  content: '';
  position: absolute;
  left: 8px;
  top: 0;
  bottom: 0;
  width: 2px;
  background: var(--border-color);
}

.log-item {
  position: relative;
  padding: var(--spacing-sm) 0;
}

.log-dot {
  position: absolute;
  left: calc(-1 * var(--spacing-lg) + 4px);
  top: calc(var(--spacing-sm) + 8px);
  width: 10px;
  height: 10px;
  border-radius: 50%;
  background: var(--accent-text);
  border: 2px solid var(--xiuxian-bg-panel);
}

.log-content {
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  padding: var(--spacing-sm) var(--spacing-md);
}

.log-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--spacing-xs);
}

.log-player {
  font-size: var(--font-size-base);
  font-weight: 600;
  color: var(--text-primary);
}

.log-time {
  font-size: var(--font-size-xs);
  color: var(--text-muted);
}

.log-detail {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  font-size: var(--font-size-sm);
}

.log-item-name {
  color: var(--text-secondary);
}

.log-change {
  color: var(--status-success-text);
  font-weight: 600;
}

/* 分页 */
.pagination {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--spacing-md);
  padding: var(--spacing-md) 0;
}

.page-btn {
  width: 32px;
  height: 32px;
  background: transparent;
  border: 1px solid var(--border-color);
  border-radius: var(--radius-sm);
  color: var(--text-secondary);
  cursor: pointer;
  font-size: 18px;
  transition: all 0.15s ease;
}

.page-btn:hover:not(:disabled) {
  background: var(--button-outline-hover-bg);
  border-color: var(--button-outline-hover-border);
  color: var(--button-outline-hover-text);
}

.page-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.page-info {
  font-size: var(--font-size-sm);
  color: var(--text-secondary);
}

/* 状态消息 */
.status-message {
  padding: var(--spacing-sm) var(--spacing-md);
  background: rgba(124, 58, 237, 0.12);
  border: 1px solid rgba(124, 58, 237, 0.28);
  border-radius: var(--radius-sm);
  color: var(--text-primary);
  font-size: var(--font-size-sm);
  text-align: center;
}
</style>
