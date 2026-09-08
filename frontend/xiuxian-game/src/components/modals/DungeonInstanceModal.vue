<template>
  <XiuXianModal :model-value="modelValue" title="秘境探索" :width="1100" @update:model-value="$emit('update:modelValue', $event)">
    <div class="di-modal">
      <!-- 左栏：秘境列表 -->
      <div class="di-left">
        <div class="di-left-header">
          <span class="di-kicker">可进入秘境</span>
          <button class="di-refresh-btn" :disabled="loading" @click="loadData">刷新</button>
        </div>
        <div v-if="loading && dungeons.length === 0" class="di-loading">加载中...</div>
        <div v-else-if="dungeons.length === 0" class="di-empty">暂无可用秘境</div>
        <div class="di-list">
          <div
            v-for="d in dungeons"
            :key="d.id"
            :class="['di-card', `di-card--level-${levelTier(d.recommendedLevel)}`, { 'di-card--selected': selectedId === d.id, 'di-card--full': d.todayEnterCount >= d.dailyEnterLimit }]"
            @click="selectedId = d.id"
          >
            <div class="di-card-accent"></div>
            <div class="di-card-body">
              <div class="di-card-header">
                <span class="di-card-name">{{ d.name }}</span>
                <span class="di-card-level">Lv.{{ d.recommendedLevel }}</span>
              </div>
              <div class="di-card-desc">{{ d.description || '暂无描述' }}</div>
              <div class="di-card-footer">
                <span class="di-card-usage">今日 {{ d.todayEnterCount }}/{{ d.dailyEnterLimit }}</span>
                <span v-if="d.todayEnterCount >= d.dailyEnterLimit" class="di-card-full-tag">已达上限</span>
              </div>
              <div v-if="d.entryCostsJson" class="di-card-costs">
                <span v-for="(cost, i) in parseCosts(d.entryCostsJson)" :key="i" :class="['di-card-cost-pill', costPillClass(cost.type)]">
                  {{ costDisplay(cost) }}
                </span>
              </div>
              <button
                v-if="selectedId === d.id"
                class="di-enter-btn"
                :disabled="!canEnter || entering"
                @click.stop="handleEnter"
              >
                {{ entering ? '进入中...' : '进入秘境' }}
              </button>
            </div>
            <div v-if="d.todayEnterCount >= d.dailyEnterLimit" class="di-card-lock">已达上限</div>
          </div>
        </div>
      </div>

      <!-- 右栏：秘境状态 -->
      <div class="di-right">
        <div v-if="!status || !status.inDungeon">
          <div class="di-right-empty">
            <div class="di-right-empty-icon">
              <span>秘</span>
            </div>
            <div class="di-right-empty-text">尚未踏入秘境</div>
            <div class="di-right-empty-hint">在左侧选择一处秘境，开始你的探索之旅</div>
          </div>

          <!-- 历史记录 -->
          <div v-if="history.length > 0" class="di-history">
            <div class="di-history-header">探索记录</div>
            <div class="di-history-list">
              <div
                v-for="h in history"
                :key="h.instanceId"
                :class="['di-history-item', { 'di-history-item--expanded': expandedHistoryId === h.instanceId }]"
                @click="toggleHistory(h.instanceId)"
              >
                <div class="di-history-item-header">
                  <span class="di-history-status-icon">{{ historyStatusIcon(h.settleReason) }}</span>
                  <span class="di-history-name">{{ h.dungeonName }}</span>
                  <span class="di-history-reason">{{ h.settleReason }}</span>
                </div>
                <div class="di-history-time">
                  {{ relativeTime(h.enterTime) }}
                </div>
                <div v-if="expandedHistoryId === h.instanceId" class="di-history-detail">
                  <div class="di-history-log">
                    <div v-if="h.exploreLog && h.exploreLog.length > 0">
                      <div v-for="(entry, i) in h.exploreLog" :key="i" class="di-log-entry">{{ entry }}</div>
                    </div>
                    <div v-else class="di-log-empty">暂无日志</div>
                  </div>
                  <div v-if="h.rewardSummary && h.rewardSummary.length > 0" class="di-history-rewards">
                    <div class="di-history-rewards-title">收益：</div>
                    <div class="di-rewards-grid">
                      <div v-for="(r, i) in h.rewardSummary" :key="i" class="di-reward-item">
                        <span class="di-reward-icon">{{ rewardIcon(r.rewardType) }}</span>
                        <span class="di-reward-name">{{ rewardTypeLabel(r.rewardType) }}<template v-if="r.rewardId && !['Gold','Exp','SpiritStone'].includes(r.rewardType)"> {{ r.rewardName || r.rewardId }}</template></span>
                        <span class="di-reward-qty">x{{ r.quantity }}</span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <template v-else>
          <!-- 状态头部 -->
          <div class="di-status-header">
            <div class="di-status-title">
              <span class="di-status-icon">⚔</span>
              {{ status.dungeonName || status.dungeonId }}
            </div>
            <button class="di-quit-btn" :disabled="quitting" @click="handleQuit">
              {{ quitting ? '退出中...' : '退出秘境' }}
            </button>
          </div>

          <!-- HP/MP 条 -->
          <div class="di-bars">
            <div class="di-bar">
              <span class="di-bar-label">HP</span>
              <div class="di-bar-track">
                <div class="di-bar-fill di-bar-fill--hp" :style="{ width: hpPercent + '%' }">
                  <span class="di-bar-inner-text">{{ status.currentHp }}/{{ status.maxHp }}</span>
                </div>
              </div>
            </div>
            <div class="di-bar">
              <span class="di-bar-label">MP</span>
              <div class="di-bar-track">
                <div class="di-bar-fill di-bar-fill--mp" :style="{ width: mpPercent + '%' }">
                  <span class="di-bar-inner-text">{{ status.currentMp }}/{{ status.maxMp }}</span>
                </div>
              </div>
            </div>
          </div>

          <!-- 探索日志 -->
          <div class="di-log-section">
            <div class="di-log-header">探索日志</div>
            <div ref="logContainer" class="di-log">
              <div v-if="status.exploreLog && status.exploreLog.length > 0">
                <div v-for="(entry, i) in status.exploreLog" :key="i" class="di-log-entry">{{ entry }}</div>
              </div>
              <div v-else class="di-log-empty">暂无日志</div>
            </div>
          </div>

          <!-- 收益概要 -->
          <div v-if="status.rewardSummary && status.rewardSummary.length > 0" class="di-rewards">
            <div class="di-rewards-header">收益概要</div>
            <div class="di-rewards-grid">
              <div v-for="(r, i) in status.rewardSummary" :key="i" class="di-reward-item">
                <span class="di-reward-icon">{{ rewardIcon(r.rewardType) }}</span>
                <span class="di-reward-name">{{ rewardTypeLabel(r.rewardType) }}<template v-if="r.rewardId && !['Gold','Exp','SpiritStone'].includes(r.rewardType)"> {{ r.rewardName || r.rewardId }}</template></span>
                <span class="di-reward-qty">x{{ r.quantity }}</span>
              </div>
            </div>
          </div>
        </template>
      </div>
    </div>

    <!-- 结算覆盖层 -->
    <Transition name="settle">
      <div v-if="settlementResult" class="di-settle-overlay" @click.self="settlementResult = null">
        <div class="di-settle-card">
          <div class="di-settle-header">
            <div class="di-settle-title">秘境结算</div>
            <div class="di-settle-subtitle">{{ settlementResult.dungeonName || '探索完成' }}</div>
          </div>
          <div class="di-settle-body">
            <div class="di-settle-rewards">
              <div
                v-for="(r, i) in settlementRewards"
                :key="i"
                class="di-settle-reward"
                :style="{ animationDelay: i * 0.08 + 's' }"
              >
                <span class="di-settle-reward-icon">{{ rewardIcon(r.rewardType) }}</span>
                <span class="di-settle-reward-name">{{ rewardTypeLabel(r.rewardType) }}<template v-if="r.rewardId && !['Gold','Exp','SpiritStone'].includes(r.rewardType)"> - {{ r.rewardName || r.rewardId }}</template></span>
                <span class="di-settle-reward-qty">+{{ r.quantity }}</span>
              </div>
            </div>
            <div v-if="settlementRewards.length === 0" class="di-settle-empty">本次探索未获得收益</div>
          </div>
          <div class="di-settle-footer">
            <button class="di-settle-confirm" @click="settlementResult = null">确认</button>
          </div>
        </div>
      </div>
    </Transition>
  </XiuXianModal>
</template>

<script>
import { ref, computed, watch, onUnmounted, nextTick } from 'vue'
import XiuXianModal from '../common/XiuXianModal.vue'
import { useGameStore } from '../../state/gameStore'
import toast from '@/utils/toast'

export default {
  name: 'DungeonInstanceModal',
  components: { XiuXianModal },
  props: { modelValue: Boolean },
  emits: ['update:modelValue'],
  setup(props) {
    const gameStore = useGameStore()
    const loading = ref(false)
    const entering = ref(false)
    const quitting = ref(false)
    const dungeons = ref([])
    const selectedId = ref(null)
    const status = ref(null)
    const logContainer = ref(null)
    const history = ref([])
    const expandedHistoryId = ref(null)
    const settlementResult = ref(null)
    let pollTimer = null

    const hpPercent = computed(() => {
      if (!status.value || status.value.maxHp <= 0) return 0
      return Math.min(100, Math.round((status.value.currentHp / status.value.maxHp) * 100))
    })

    const mpPercent = computed(() => {
      if (!status.value || status.value.maxMp <= 0) return 0
      return Math.min(100, Math.round((status.value.currentMp / status.value.maxMp) * 100))
    })

    const canEnter = computed(() => {
      if (!selectedId.value) return false
      if (status.value?.inDungeon) return false
      const d = dungeons.value.find(x => x.id === selectedId.value)
      if (!d) return false
      if (d.todayEnterCount >= d.dailyEnterLimit) return false
      return true
    })

    const settlementRewards = computed(() => {
      if (!settlementResult.value) return []
      return settlementResult.value.grantedRewards || []
    })

    function levelTier(level) {
      if (level >= 50) return 'high'
      if (level >= 30) return 'mid'
      return 'low'
    }

    function costPillClass(type) {
      if (type === 'Gold') return 'di-cost--gold'
      if (type === 'SpiritStone') return 'di-cost--spirit'
      return 'di-cost--item'
    }

    function rewardIcon(type) {
      const map = { Gold: '✨', Exp: '✨', Item: '📦', Equipment: '⚔', SpiritStone: '💎', ShopSpend: '💰' }
      return map[type] || '🎁'
    }

    function historyStatusIcon(reason) {
      if (!reason) return '⏳'
      if (reason.includes('胜利') || reason.includes('通关') || reason.includes('完成')) return '🏆'
      if (reason.includes('死亡') || reason.includes('失败')) return '💀'
      if (reason.includes('退出') || reason.includes('主动')) return '🚪'
      return '📋'
    }

    function relativeTime(timeStr) {
      if (!timeStr) return ''
      try {
        const d = new Date(timeStr)
        const now = Date.now()
        const diff = now - d.getTime()
        if (diff < 60000) return '刚刚'
        if (diff < 3600000) return `${Math.floor(diff / 60000)}分钟前`
        if (diff < 86400000) return `${Math.floor(diff / 3600000)}小时前`
        if (diff < 604800000) return `${Math.floor(diff / 86400000)}天前`
        return `${d.getMonth() + 1}/${d.getDate()} ${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`
      } catch { return timeStr }
    }

    function parseCosts(json) {
      if (!json) return []
      try { return JSON.parse(json) } catch { return [] }
    }

    function costDisplay(cost) {
      const qty = cost.quantity || cost.amount || 0
      if (cost.type === 'Gold') return `${qty}金币`
      if (cost.type === 'SpiritStone') return `${qty}灵石`
      if (cost.type === 'Item') return `${cost.name || cost.id || cost.itemId} x${qty}`
      return `${cost.name || cost.type || cost.itemId} x${qty}`
    }

    function rewardTypeLabel(type) {
      const map = { Gold: '金币', Exp: '经验', Item: '道具', Equipment: '装备', SpiritStone: '灵石', ShopSpend: '消耗' }
      return map[type] || type
    }

    function formatTime(timeStr) {
      if (!timeStr) return ''
      try {
        const d = new Date(timeStr)
        return `${d.getMonth() + 1}/${d.getDate()} ${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`
      } catch { return timeStr }
    }

    function toggleHistory(id) {
      expandedHistoryId.value = expandedHistoryId.value === id ? null : id
    }

    async function loadHistory() {
      try {
        const result = await gameStore.getDungeonInstanceHistory(10)
        history.value = result || []
      } catch (e) {
        console.error('加载秘境历史失败', e)
      }
    }

    async function loadData() {
      loading.value = true
      try {
        const [avail, st] = await Promise.allSettled([
          gameStore.getAvailableDungeonInstances(),
          gameStore.getDungeonInstanceStatus()
        ])
        if (avail.status === 'fulfilled') dungeons.value = avail.value || []
        if (st.status === 'fulfilled') status.value = st.value || null
        if (!status.value?.inDungeon) {
          await loadHistory()
        }
      } catch (e) {
        console.error('加载秘境数据失败', e)
      } finally {
        loading.value = false
      }
    }

    async function handleEnter() {
      if (!canEnter.value) return
      entering.value = true
      try {
        await gameStore.enterDungeonInstance(selectedId.value)
      } catch (e) {
        toast.error(e.message || '进入秘境失败')
      } finally {
        entering.value = false
        await loadData()
        if (status.value?.inDungeon) {
          startPolling()
        }
      }
    }

    async function handleQuit() {
      quitting.value = true
      try {
        const result = await gameStore.quitDungeonInstance()
        stopPolling()
        await loadData()
        if (result && result.grantedRewards && result.grantedRewards.length > 0) {
          settlementResult.value = result
        }
      } catch (e) {
        toast.error(e.message || '退出秘境失败')
      } finally {
        quitting.value = false
      }
    }

    async function pollStatus() {
      try {
        const st = await gameStore.getDungeonInstanceStatus()
        status.value = st
        if (!st || !st.inDungeon) {
          stopPolling()
        }
        await nextTick()
        if (logContainer.value) {
          logContainer.value.scrollTop = logContainer.value.scrollHeight
        }
      } catch (e) {
        console.error('轮询秘境状态失败', e)
      }
    }

    function startPolling() {
      stopPolling()
      pollTimer = setInterval(pollStatus, 3000)
    }

    function stopPolling() {
      if (pollTimer) {
        clearInterval(pollTimer)
        pollTimer = null
      }
    }

    watch(() => props.modelValue, async (open) => {
      if (open) {
        await loadData()
        if (status.value?.inDungeon) {
          startPolling()
          await nextTick()
          if (logContainer.value) {
            logContainer.value.scrollTop = logContainer.value.scrollHeight
          }
        }
      } else {
        stopPolling()
      }
    })

    onUnmounted(() => {
      stopPolling()
    })

    return {
      loading, entering, quitting,
      dungeons, selectedId, status,
      logContainer,
      history, expandedHistoryId,
      hpPercent, mpPercent, canEnter,
      settlementResult, settlementRewards,
      levelTier, costPillClass, rewardIcon, historyStatusIcon, relativeTime,
      parseCosts, costDisplay, rewardTypeLabel, formatTime,
      loadData, handleEnter, handleQuit, toggleHistory
    }
  }
}
</script>

<style scoped>
.di-modal {
  display: flex;
  gap: 16px;
  min-height: 500px;
}

/* ===== 左栏 ===== */
.di-left {
  width: 380px;
  flex-shrink: 0;
  display: flex;
  flex-direction: column;
  gap: 10px;
  /* 红框区域整体滚动，秘境卡片本身保持原样。 */
  max-height: 500px;
  overflow-y: auto;
  padding-right: 4px;
}
.di-left-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.di-kicker {
  font-size: var(--font-size-sm);
  font-weight: 600;
  color: var(--text-primary);
  letter-spacing: 1px;
}
.di-refresh-btn {
  padding: 4px 12px;
  font-size: var(--font-size-xs);
  background: var(--button-neutral-bg);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-sm);
  color: var(--button-neutral-text);
  cursor: pointer;
  transition: background 0.15s, color 0.15s;
}
.di-refresh-btn:hover { background: var(--button-neutral-hover-bg); color: var(--button-neutral-hover-text); }

.di-list {
  flex: 1;
  overflow: visible;
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding-right: 4px;
}

/* 秘境卡片 */
.di-card {
  position: relative;
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  cursor: pointer;
  transition: border-color 0.2s, box-shadow 0.2s, background 0.2s;
  background: var(--xiuxian-bg-panel);
  overflow: hidden;
  display: flex;
}
.di-card:hover {
  border-color: var(--jade-primary);
  box-shadow: 0 0 12px oklch(1 0 0 / 0.04);
}
.di-card--selected {
  border-color: var(--jade-primary);
  background: var(--control-bg-hover);
  box-shadow: 0 0 16px oklch(1 0 0 / 0.06);
}
.di-card--full { opacity: 0.5; pointer-events: none; }

/* 左侧彩色边条 */
.di-card-accent {
  width: 4px;
  flex-shrink: 0;
  border-radius: 4px 0 0 4px;
  transition: background 0.2s;
}
.di-card--level-low .di-card-accent { background: #22c55e; }
.di-card--level-mid .di-card-accent { background: #3b82f6; }
.di-card--level-high .di-card-accent { background: #a855f7; }
.di-card--selected .di-card-accent { background: var(--jade-primary); }

.di-card-body {
  flex: 1;
  padding: 10px 12px;
  min-width: 0;
}

.di-card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 4px;
}
.di-card-name { font-weight: 600; color: var(--text-primary); font-size: var(--font-size-sm); }
.di-card-level {
  font-size: var(--font-size-xs);
  color: var(--text-secondary);
  padding: 1px 6px;
  background: var(--bg-overlay-medium);
  border-radius: 10px;
}
.di-card-desc {
  font-size: var(--font-size-xs);
  color: var(--text-secondary);
  line-height: 1.4;
  margin-bottom: 6px;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
.di-card-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: var(--font-size-xs);
}
.di-card-usage { color: var(--text-secondary); }
.di-card-full-tag { color: var(--status-danger-text); font-weight: 500; }

.di-card-costs {
  margin-top: 4px;
  font-size: var(--font-size-xs);
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}
.di-card-cost-pill {
  padding: 2px 8px;
  border-radius: 10px;
  font-size: 11px;
  font-weight: 500;
}
.di-cost--gold { background: oklch(0.85 0.06 85 / 0.15); color: #f59e0b; }
.di-cost--spirit { background: oklch(0.75 0.08 250 / 0.15); color: #60a5fa; }
.di-cost--item { background: var(--bg-overlay-medium); color: var(--text-secondary); }

/* 已达上限锁 */
.di-card-lock {
  position: absolute;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  background: oklch(0 0 0 / 0.35);
  font-size: var(--font-size-sm);
  font-weight: 600;
  color: var(--text-muted);
  letter-spacing: 2px;
  pointer-events: none;
}

.di-enter-btn {
  margin-top: 8px;
  padding: 8px 16px;
  font-size: var(--font-size-sm);
  font-weight: 600;
  background: var(--button-primary-start);
  color: var(--button-primary-text);
  border: 1px solid transparent;
  border-radius: var(--radius-md);
  cursor: pointer;
  transition: all 0.15s ease;
  width: 100%;
  display: block;
  text-align: center;
}
.di-enter-btn:hover:not(:disabled) {
  background: var(--button-primary-hover-start);
  color: var(--button-primary-hover-text);
}
.di-enter-btn:disabled { opacity: 0.4; cursor: not-allowed; }

.di-loading, .di-empty {
  padding: 20px;
  text-align: center;
  color: var(--text-secondary);
  font-size: var(--font-size-sm);
}

/* ===== 右栏 ===== */
.di-right {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 12px;
  min-width: 0;
}

/* 空状态 */
.di-right-empty {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 10px;
  padding: 32px 0;
}
.di-right-empty-icon {
  width: 72px;
  height: 72px;
  border-radius: 50%;
  background: linear-gradient(135deg, var(--bg-overlay-medium), var(--bg-overlay-dark));
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 32px;
  font-weight: 700;
  color: var(--jade-primary);
  animation: emptyPulse 3s ease-in-out infinite;
  box-shadow: 0 0 24px oklch(1 0 0 / 0.05);
}
@keyframes emptyPulse {
  0%, 100% { transform: scale(1); opacity: 0.8; }
  50% { transform: scale(1.06); opacity: 1; }
}
.di-right-empty-text { font-size: var(--font-size-lg); font-weight: 600; color: var(--text-primary); }
.di-right-empty-hint { font-size: var(--font-size-sm); color: var(--text-muted); }

/* 状态头部 */
.di-status-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.di-status-title {
  font-size: var(--font-size-lg);
  font-weight: 700;
  color: var(--text-primary);
  display: flex;
  align-items: center;
  gap: 8px;
}
.di-status-icon { font-size: 20px; }
.di-quit-btn {
  padding: 6px 16px;
  font-size: var(--font-size-sm);
  background: var(--button-danger-start);
  border: 1px solid transparent;
  border-radius: var(--radius-sm);
  color: var(--button-danger-text);
  cursor: pointer;
  transition: background 0.15s, color 0.15s;
}
.di-quit-btn:hover { background: var(--button-danger-hover-start); color: var(--button-danger-hover-text); }
.di-quit-btn:disabled { opacity: 0.4; cursor: not-allowed; }

/* HP/MP 条 */
.di-bars { display: flex; flex-direction: column; gap: 8px; }
.di-bar { display: flex; align-items: center; gap: 8px; }
.di-bar-label { width: 28px; font-size: var(--font-size-xs); font-weight: 700; color: var(--text-secondary); text-transform: uppercase; }
.di-bar-track {
  flex: 1;
  height: 20px;
  background: var(--bg-overlay-dark);
  border-radius: 10px;
  overflow: hidden;
  box-shadow: inset 0 1px 3px oklch(0 0 0 / 0.3);
}
.di-bar-fill {
  height: 100%;
  border-radius: 10px;
  transition: width 0.4s ease;
  display: flex;
  align-items: center;
  justify-content: flex-end;
  padding-right: 8px;
  min-width: 0;
}
.di-bar-fill--hp {
  background: var(--hp-color);
}
.di-bar-fill--mp {
  background: var(--mp-color);
}
.di-bar-inner-text {
  font-size: 11px;
  font-weight: 600;
  color: white;
  text-shadow: 0 1px 2px oklch(0 0 0 / 0.4);
  white-space: nowrap;
}

/* 探索日志 */
.di-log-section { flex: 1; display: flex; flex-direction: column; min-height: 0; }
.di-log-header {
  font-size: var(--font-size-sm);
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 6px;
  letter-spacing: 1px;
}
.di-log {
  flex: 1;
  min-height: 180px;
  max-height: 320px;
  overflow-y: auto;
  background: var(--xiuxian-bg-primary);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  padding: 8px 10px;
  font-family: var(--font-mono);
  font-size: var(--font-size-xs);
  line-height: 1.7;
  box-shadow: inset 0 2px 6px oklch(0 0 0 / 0.15);
}
.di-log-entry {
  color: var(--text-secondary);
  padding: 2px 4px;
  border-radius: 3px;
  transition: background 0.1s;
}
.di-log-entry:hover { background: var(--bg-overlay-light); }
.di-log-entry + .di-log-entry { border-top: 1px solid oklch(1 0 0 / 0.04); }
.di-log-empty { color: var(--text-muted); font-style: italic; }

/* 收益概要 */
.di-rewards { flex-shrink: 0; }
.di-rewards-header {
  font-size: var(--font-size-sm);
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 6px;
  letter-spacing: 1px;
}
.di-rewards-grid { display: flex; flex-wrap: wrap; gap: 6px; }
.di-reward-item {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 4px 10px;
  background: var(--bg-overlay-medium);
  border-radius: 12px;
  font-size: var(--font-size-xs);
  border: 1px solid oklch(1 0 0 / 0.04);
}
.di-reward-icon { font-size: 13px; }
.di-reward-name { color: var(--text-primary); font-size: var(--font-size-xs); }
.di-reward-qty { color: var(--accent-text-strong); font-weight: 700; }

/* ===== 历史记录 ===== */
.di-history {
  margin-top: 20px;
  border-top: 1px solid var(--border-color);
  padding-top: 14px;
}
.di-history-header {
  font-size: var(--font-size-sm);
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 10px;
  letter-spacing: 1px;
}
.di-history-list {
  display: flex;
  flex-direction: column;
  gap: 6px;
  max-height: 300px;
  overflow-y: auto;
}
.di-history-item {
  padding: 8px 10px;
  background: var(--xiuxian-bg-panel);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  cursor: pointer;
  transition: border-color 0.15s, background 0.15s;
}
.di-history-item:hover { border-color: var(--jade-primary); background: var(--control-bg-hover); }
.di-history-item--expanded { border-color: var(--jade-primary); }
.di-history-item-header {
  display: flex;
  align-items: center;
  gap: 6px;
  margin-bottom: 2px;
}
.di-history-status-icon { font-size: 14px; }
.di-history-name { font-size: var(--font-size-sm); font-weight: 600; color: var(--text-primary); }
.di-history-reason { font-size: var(--font-size-xs); color: var(--text-secondary); margin-left: auto; }
.di-history-time { font-size: var(--font-size-xs); color: var(--text-muted); }
.di-history-detail {
  margin-top: 8px;
  padding-top: 8px;
  border-top: 1px solid var(--border-color);
  animation: detailSlide 0.2s ease;
}
@keyframes detailSlide {
  from { opacity: 0; transform: translateY(-4px); }
  to { opacity: 1; transform: translateY(0); }
}
.di-history-log {
  max-height: 200px;
  overflow-y: auto;
  background: var(--xiuxian-bg-primary);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-sm);
  padding: 6px 8px;
  font-family: var(--font-mono);
  font-size: var(--font-size-xs);
  line-height: 1.6;
  margin-bottom: 8px;
  box-shadow: inset 0 1px 4px oklch(0 0 0 / 0.1);
}
.di-history-rewards { margin-top: 6px; }
.di-history-rewards-title { font-size: var(--font-size-xs); color: var(--text-secondary); margin-bottom: 4px; }

/* ===== 结算覆盖层 ===== */
.di-settle-overlay {
  position: absolute;
  inset: 0;
  background: oklch(0 0 0 / 0.6);
  backdrop-filter: blur(4px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 10;
  border-radius: var(--radius-lg);
}
.di-settle-card {
  background: var(--xiuxian-bg-panel);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-lg);
  box-shadow: 0 20px 60px oklch(0 0 0 / 0.4);
  width: 360px;
  max-width: 90%;
  overflow: hidden;
}
.di-settle-header {
  padding: 20px 24px 12px;
  text-align: center;
  border-bottom: 1px solid var(--border-color);
  background: linear-gradient(180deg, var(--bg-overlay-light), transparent);
}
.di-settle-title {
  font-size: 20px;
  font-weight: 700;
  color: var(--text-primary);
  letter-spacing: 3px;
}
.di-settle-subtitle {
  font-size: var(--font-size-sm);
  color: var(--text-muted);
  margin-top: 4px;
}
.di-settle-body {
  padding: 20px 24px;
}
.di-settle-rewards {
  display: flex;
  flex-direction: column;
  gap: 8px;
}
.di-settle-reward {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  background: var(--bg-overlay-medium);
  border-radius: var(--radius-md);
  animation: rewardFadeIn 0.3s ease both;
}
@keyframes rewardFadeIn {
  from { opacity: 0; transform: translateX(-12px); }
  to { opacity: 1; transform: translateX(0); }
}
.di-settle-reward-icon { font-size: 18px; }
.di-settle-reward-name { flex: 1; font-size: var(--font-size-sm); color: var(--text-primary); }
.di-settle-reward-qty {
  font-size: var(--font-size-sm);
  font-weight: 700;
  color: var(--accent-text-strong);
}
.di-settle-empty {
  text-align: center;
  color: var(--text-muted);
  font-size: var(--font-size-sm);
  padding: 16px 0;
}
.di-settle-footer {
  padding: 12px 24px 20px;
  text-align: center;
}
.di-settle-confirm {
  padding: 10px 40px;
  font-size: var(--font-size-sm);
  font-weight: 600;
  background: var(--button-primary-start);
  color: var(--button-primary-text);
  border: 1px solid transparent;
  border-radius: var(--radius-md);
  cursor: pointer;
  transition: all 0.15s ease;
  letter-spacing: 2px;
}
.di-settle-confirm:hover {
  background: var(--button-primary-hover-start);
  color: var(--button-primary-hover-text);
}

/* 结算动画 */
.settle-enter-active { transition: all 0.25s ease; }
.settle-leave-active { transition: all 0.15s ease; }
.settle-enter-from { opacity: 0; }
.settle-enter-from .di-settle-card { transform: translateY(24px) scale(0.95); opacity: 0; }
.settle-leave-to { opacity: 0; }
.settle-leave-to .di-settle-card { transform: scale(0.95); opacity: 0; }
</style>
