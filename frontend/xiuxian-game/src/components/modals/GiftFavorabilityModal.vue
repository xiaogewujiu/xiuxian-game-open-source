<template>
  <!-- 赠送好感度道具 Modal -->
  <XiuXianModal :model-value="modelValue" :title="modalTitle" :width="480" @update:model-value="$emit('update:modelValue', $event)">
    <div class="gift-modal">
      <!-- 加载中 -->
      <div v-if="isLoading" class="loading-state">
        <div class="spinner"></div>
        <span>加载中...</span>
      </div>

      <!-- 无可赠送道具 -->
      <div v-else-if="giftItems.length === 0" class="empty-state">
        <div class="empty-icon"><AssetIcon :source="ICON.misc_gift" size="48" /></div>
        <p class="empty-text">暂无可赠送的好感度道具</p>
        <p class="empty-hint">请先获取好感度道具后再来赠送</p>
      </div>

      <!-- 道具列表 -->
      <template v-else>
        <div class="item-list">
          <div
            v-for="item in giftItems"
            :key="item.id"
            class="gift-item"
            :class="{ selected: selectedItemId === item.id, disabled: item.dailyRemaining <= 0 }"
            @click="selectItem(item)"
          >
            <div class="item-icon"><AssetIcon :source="item.icon" size="32" /></div>
            <div class="item-info">
              <div class="item-name" :class="item.quality">{{ item.name }}</div>
              <div class="item-detail">
                <span class="item-count">拥有: {{ item.count }}</span>
                <span v-if="item.dailyRemaining > 0" class="daily-remain">今日剩余: {{ item.dailyRemaining }}</span>
                <span v-else class="daily-empty">今日已用完</span>
              </div>
              <div v-if="item.favorabilityChange" class="favorability-change">
                <span :class="item.favorabilityChange > 0 ? 'positive' : 'negative'">
                  {{ item.favorabilityChange > 0 ? '+' : '' }}{{ item.favorabilityChange }} 好感度
                </span>
              </div>
            </div>
            <div v-if="selectedItemId === item.id" class="selected-mark">
              <span>✓</span>
            </div>
          </div>
        </div>

        <!-- 赠送按钮 -->
        <div class="gift-action">
          <button
            class="gift-btn"
            :disabled="!selectedItemId || isGifting"
            @click="doGift"
          >
            <span v-if="isGifting" class="spinner-small"></span>
            <span v-else>确认赠送</span>
          </button>
          <div v-if="statusMessage" class="status-message" :class="statusType">{{ statusMessage }}</div>
        </div>
      </template>
    </div>
  </XiuXianModal>
</template>

<script>
import { ref, computed, watch } from 'vue'
import XiuXianModal from '../common/XiuXianModal.vue'
import AssetIcon from '../common/AssetIcon.vue'
import { useGameStore } from '../../state/gameStore'
import { ICON } from '../../icons'
import toast from '@/utils/toast'

/**
 * GiftFavorabilityModal - 赠送好感度道具弹窗
 *
 * 功能：
 * - 列出背包中可赠送的好感度道具
 * - 显示每件道具的图标、名称、数量、每日剩余赠送次数
 * - 选中道具后显示可增加/减少的好感度
 * - 确认赠送并显示结果
 */
export default {
  name: 'GiftFavorabilityModal',

  components: {
    XiuXianModal,
    AssetIcon
  },

  props: {
    modelValue: Boolean,
    targetPlayerId: {
      type: String,
      default: ''
    },
    targetPlayerName: {
      type: String,
      default: '未知'
    }
  },

  emits: ['update:modelValue', 'gifted'],

  setup(props, { emit }) {
    const gameStore = useGameStore()
    const isLoading = ref(false)
    const isGifting = ref(false)
    const selectedItemId = ref(null)
    const statusMessage = ref('')
    const statusType = ref('')

    const modalTitle = computed(() => `赠送好感度道具给 ${props.targetPlayerName || '未知'}`)

    // 从背包中筛选可赠送的好感度道具
    const giftItems = computed(() => {
      const inventory = gameStore.state.inventory || []
      return inventory
        .filter((item) => {
          const itemType = String(item.ItemType ?? item.itemType ?? '').toLowerCase()
          const canGift = Boolean(item.CanGift ?? item.canGift)
          return canGift || itemType.includes('favorability') || itemType.includes('gift')
        })
        .map((item) => ({
          id: item.Id ?? item.id ?? '',
          itemId: item.ItemId ?? item.itemId ?? '',
          name: item.Name ?? item.name ?? '未知道具',
          icon: item.Icon || item.icon || ICON.misc_gift,
          quality: String(item.Quality ?? item.quality ?? 'common').toLowerCase(),
          count: item.Quantity ?? item.quantity ?? 0,
          dailyRemaining: item.DailyGiftRemaining ?? item.dailyGiftRemaining ?? 999,
          favorabilityChange: item.FavorabilityChange ?? item.favorabilityChange ?? 0,
          description: item.Description ?? item.description ?? ''
        }))
    })

    // 选中道具
    const selectItem = (item) => {
      if (item.dailyRemaining <= 0) return
      selectedItemId.value = selectedItemId.value === item.id ? null : item.id
      statusMessage.value = ''
    }

    // 执行赠送
    const doGift = async () => {
      if (!selectedItemId.value || !props.targetPlayerId) return

      const selectedItem = giftItems.value.find((i) => i.id === selectedItemId.value)
      if (!selectedItem) return

      if (selectedItem.dailyRemaining <= 0) {
        toast.error('该道具今日赠送次数已用完')
        return
      }

      isGifting.value = true
      statusMessage.value = ''

      try {
        const result = await gameStore.giftFavorabilityItem(props.targetPlayerId, selectedItem.itemId || selectedItem.id)

        if (result) {
          toast.success(`成功赠送 ${selectedItem.name} 给 ${props.targetPlayerName}`)
          selectedItemId.value = null

          // 刷新背包和好感度数据
          await Promise.allSettled([
            gameStore.loadInventory(true),
            gameStore.loadFavorabilityList('ToOthers'),
            gameStore.loadFavorabilityLog(props.targetPlayerId)
          ])

          emit('gifted')
        }
      } catch (error) {
        toast.error(error.message || '赠送失败，请稍后重试')
        statusMessage.value = error.message || '赠送失败'
        statusType.value = 'error'
      } finally {
        isGifting.value = false
      }
    }

    // 弹窗打开时加载背包数据
    watch(() => props.modelValue, async (visible) => {
      if (!visible) {
        selectedItemId.value = null
        statusMessage.value = ''
        return
      }

      isLoading.value = true
      try {
        await gameStore.loadInventory()
      } catch {
        // 静默失败，背包数据可能已在缓存中
      } finally {
        isLoading.value = false
      }
    }, { immediate: true })

    return {
      ICON,
      modalTitle,
      isLoading,
      isGifting,
      selectedItemId,
      giftItems,
      statusMessage,
      statusType,
      selectItem,
      doGift
    }
  }
}
</script>

<style scoped>
.gift-modal {
  padding: var(--spacing-md);
}

/* 加载状态 */
.loading-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: var(--spacing-xl);
  gap: var(--spacing-md);
  color: var(--text-secondary);
}

.spinner {
  width: 32px;
  height: 32px;
  border: 3px solid var(--border-color);
  border-top-color: var(--accent-text);
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

.spinner-small {
  display: inline-block;
  width: 16px;
  height: 16px;
  border: 2px solid var(--bg-overlay-medium);
  border-top-color: var(--text-on-dark);
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* 空状态 */
.empty-state {
  text-align: center;
  padding: var(--spacing-xl);
}

.empty-icon {
  margin-bottom: var(--spacing-md);
  opacity: 0.5;
}

.empty-text {
  font-size: var(--font-size-md);
  color: var(--text-primary);
  margin-bottom: var(--spacing-xs);
}

.empty-hint {
  font-size: var(--font-size-base);
  color: var(--text-muted);
}

/* 道具列表 */
.item-list {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
  max-height: 320px;
  overflow-y: auto;
  margin-bottom: var(--spacing-md);
  padding-right: var(--spacing-xs);
}

.gift-item {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  padding: var(--spacing-md);
  background: var(--xiuxian-bg-secondary);
  border: 2px solid transparent;
  border-radius: var(--radius-md);
  cursor: pointer;
  transition: all 0.15s ease;
}

.gift-item:hover:not(.disabled) {
  background: rgba(124, 58, 237, 0.15);
  border-color: var(--border-color);
}

.gift-item.selected {
  border-color: var(--accent-text);
  background: rgba(124, 58, 237, 0.1);
}

.gift-item.disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.item-icon {
  flex-shrink: 0;
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--xiuxian-bg-primary);
  border-radius: var(--radius-sm);
}

.item-info {
  flex: 1;
  min-width: 0;
}

.item-name {
  font-size: var(--font-size-base);
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 2px;
}

.item-name.common { color: var(--text-primary); }
.item-name.uncommon { color: var(--quality-uncommon); }
.item-name.rare { color: var(--quality-rare); }
.item-name.epic { color: var(--quality-epic); }
.item-name.legendary { color: var(--quality-legendary); }
.item-name.mythic { color: var(--quality-mythic); }

.item-detail {
  display: flex;
  gap: var(--spacing-md);
  font-size: var(--font-size-sm);
  color: var(--text-secondary);
}

.daily-remain {
  color: var(--accent-text);
}

.daily-empty {
  color: var(--hp-color);
}

.favorability-change {
  margin-top: 2px;
  font-size: var(--font-size-sm);
}

.favorability-change .positive {
  color: var(--quality-uncommon);
  font-weight: 600;
}

.favorability-change .negative {
  color: var(--hp-color);
  font-weight: 600;
}

.selected-mark {
  flex-shrink: 0;
  width: 24px;
  height: 24px;
  background: var(--accent-text);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 14px;
  color: var(--text-on-dark);
}

/* 赠送按钮 */
.gift-action {
  padding-top: var(--spacing-md);
  border-top: 1px solid var(--border-color);
}

.gift-btn {
  width: 100%;
  padding: var(--spacing-md);
  background: var(--button-primary-start);
  border: none;
  border-radius: var(--radius-sm);
  color: var(--text-on-dark);
  font-size: var(--font-size-md);
  font-weight: 600;
  cursor: pointer;
  transition: all 0.15s ease;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--spacing-sm);
}

.gift-btn:hover:not(:disabled) {
  box-shadow: var(--shadow-sm);
  transform: translateY(-1px);
}

.gift-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.status-message {
  margin-top: var(--spacing-sm);
  font-size: var(--font-size-base);
  text-align: center;
}

.status-message.error {
  color: var(--hp-color);
}

.status-message.success {
  color: var(--quality-uncommon);
}
</style>
