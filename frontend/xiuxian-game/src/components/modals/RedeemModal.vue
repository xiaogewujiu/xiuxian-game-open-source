<template>
  <!-- 兑换码 Modal -->
  <XiuXianModal :model-value="modelValue" title="兑换码" :width="400" @update:model-value="$emit('update:modelValue', $event)">
    <div class="redeem-modal">
      <div class="redeem-icon"><AssetIcon :source="ICON.misc_gift" size="25" /></div>
      <p class="redeem-desc">输入兑换码领取丰厚奖励</p>

      <div class="input-group">
        <input
          v-model="code"
          type="text"
          placeholder="请输入兑换码"
          @keyup.enter="redeem"
        />
        <button class="redeem-btn" :disabled="!code.trim() || isLoading" @click="redeem">
          <span v-if="isLoading" class="spinner"></span>
          <span v-else>确认兑换</span>
        </button>
      </div>

      <!-- 兑换结果 -->
      <Transition name="fade">
        <div v-if="result.show" class="result-message" :class="result.type">
          {{ result.message }}
        </div>
      </Transition>

      <!-- 兑换奖励展示 -->
      <div v-if="result.rewards" class="reward-display">
        <div class="reward-title">获得奖励</div>
        <div class="reward-items">
          <div v-for="(reward, index) in result.rewards" :key="index" class="reward-item">
            <span class="reward-icon">{{ reward.icon }}</span>
            <span class="reward-name">{{ reward.name }}</span>
            <span class="reward-count">x{{ reward.count }}</span>
          </div>
        </div>
      </div>
    </div>
  </XiuXianModal>
</template>

<script>
import { ref, reactive } from 'vue'
import XiuXianModal from '../common/XiuXianModal.vue'
import { apiClient } from '../../lib/apiClient'
import { useGameStore } from '../../state/gameStore'
import { ICON } from '../../icons'
import AssetIcon from '../common/AssetIcon.vue'

/**
 * RedeemModal - 兑换码弹窗
 *
 * 功能：
 * - 输入兑换码
 * - 显示兑换结果
 */
export default {
  name: 'RedeemModal',

  components: {
    XiuXianModal,
    AssetIcon
  },

  props: {
    modelValue: Boolean
  },

  emits: ['update:modelValue'],

  setup() {
    const gameStore = useGameStore()
    const code = ref('')
    const isLoading = ref(false)
    const result = reactive({
      show: false,
      type: '',
      message: '',
      rewards: null
    })

    const rewardIcons = {
      gold: ICON.misc_coin,
      exp: ICON.misc_sparkles,
      spiritStone: ICON.misc_diamond,
      item: ICON.misc_gift
    }

    // 兑换
    const redeem = async () => {
      if (!code.value.trim()) return

      isLoading.value = true

      try {
        // 中文注释：
        // 兑换码成功后，奖励可能同时影响人物货币和背包物品。
        // 因此前端这里统一刷新玩家信息和背包，避免用户兑换成功后还看到旧数值。
        const response = await apiClient.redeemCode(code.value.trim())

        await Promise.allSettled([
          gameStore.loadPlayer(true),
          gameStore.loadInventory(true)
        ])

        result.type = 'success'
        result.message = response?.message || '兑换成功！'
        result.rewards = (response?.rewards || []).map((reward) => ({
          icon: rewardIcons[reward.type] || ICON.misc_gift,
          name: reward.name,
          count: reward.count
        }))
      } catch (error) {
        result.type = 'error'
        result.message = error.message || '兑换失败。'
        result.rewards = null
      } finally {
        result.show = true
        isLoading.value = false

        setTimeout(() => {
          result.show = false
        }, 3000)
      }
    }

    return {
      ICON,
      code,
      isLoading,
      result,
      redeem
    }
  }
}
</script>

<style scoped>
.redeem-modal {
  padding: var(--spacing-lg);
  text-align: center;
}

.redeem-icon {
  font-size: 64px;
  margin-bottom: var(--spacing-md);
}

.redeem-desc {
  font-size: 14px;
  color: var(--text-secondary);
  margin-bottom: var(--spacing-lg);
}

.input-group {
  display: flex;
  gap: var(--spacing-sm);
  margin-bottom: var(--spacing-md);
}

.input-group input {
  flex: 1;
  padding: var(--spacing-md);
  background: var(--xiuxian-bg-secondary);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-sm);
  color: var(--text-primary);
  font-size: 14px;
  text-align: center;
  text-transform: uppercase;
}

.input-group input:focus {
  outline: none;
  border-color: var(--accent-text);
  box-shadow: var(--shadow-sm);
}

.redeem-btn {
  padding: var(--spacing-md) var(--spacing-lg);
  background: var(--button-primary-start);
  border: none;
  border-radius: var(--radius-sm);
  color: var(--text-on-dark);
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.15s ease;
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
}

.redeem-btn:hover:not(:disabled) {
  box-shadow: var(--shadow-sm);
}

.redeem-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.spinner {
  width: 16px;
  height: 16px;
  border: 2px solid var(--bg-overlay-medium);
  border-top-color: var(--text-on-dark);
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* 结果消息 */
.result-message {
  padding: var(--spacing-md);
  border-radius: var(--radius-sm);
  margin-bottom: var(--spacing-md);
  animation: slide-down 0.3s ease;
}

.result-message.success {
  background: rgba(76, 175, 80, 0.2);
  color: var(--quality-uncommon);
}

.result-message.error {
  background: var(--status-danger-bg);
  color: var(--hp-color);
}

@keyframes slide-down {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.fade-enter-active,
.fade-leave-active {
  transition: all 0.15s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

/* 奖励展示 */
.reward-display {
  background: rgba(212, 168, 83, 0.1);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  padding: var(--spacing-md);
}

.reward-title {
  font-size: var(--font-size-base);
  font-weight: 600;
  color: var(--highlight-text);
  margin-bottom: var(--spacing-sm);
}

.reward-items {
  display: flex;
  justify-content: center;
  gap: var(--spacing-md);
}

.reward-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
  padding: var(--spacing-sm);
  background: var(--xiuxian-bg-primary);
  border-radius: var(--radius-sm);
  min-width: 80px;
}

.reward-item .reward-icon {
  font-size: 24px;
}

.reward-item .reward-name {
  font-size: var(--font-size-base);
  color: var(--text-primary);
}

.reward-item .reward-count {
  font-size: var(--font-size-base);
  color: var(--highlight-text);
  font-weight: 600;
}
</style>
