<template>
  <XiuXianModal :model-value="modelValue" title="通天塔" :width="640" @update:model-value="$emit('update:modelValue', $event)">
    <div class="tower-modal">
      <!-- 我的信息 -->
      <div class="tower-my-info" v-if="myInfo">
        <div class="tower-stats">
          <div class="stat-item">
            <span class="stat-label">最高层</span>
            <span class="stat-value">第{{ myInfo.highestFloor }}层</span>
          </div>
          <div class="stat-item">
            <span class="stat-label">当前层</span>
            <span class="stat-value">第{{ myInfo.currentFloor }}层</span>
          </div>
          <div class="stat-item">
            <span class="stat-label">最佳时间</span>
            <span class="stat-value">{{ myInfo.bestClearTimeMs ? (myInfo.bestClearTimeMs / 1000).toFixed(1) + 's' : '-' }}</span>
          </div>
        </div>
        <div class="tower-daily">
          <span>今日剩余: {{ myInfo.dailyAttemptsRemaining }}/{{ myInfo.dailyAttemptsMax }} 次</span>
          <button class="action-btn" :disabled="isLoading || myInfo.purchasableAttempts <= 0" @click="handleBuyAttempts">
            购买次数 ({{ myInfo.purchasableAttempts }}次可购)
          </button>
        </div>
      </div>

      <!-- 视图切换 -->
      <div class="tower-tabs">
        <button class="tab-btn" :class="{ active: currentView === 'floors' }" @click="currentView = 'floors'">楼层列表</button>
        <button class="tab-btn" :class="{ active: currentView === 'history' }" @click="currentView = 'history'">挑战记录</button>
        <button class="tab-btn" :class="{ active: currentView === 'leaderboard' }" @click="currentView = 'leaderboard'">排行榜</button>
      </div>

      <!-- 楼层列表 -->
      <div v-if="currentView === 'floors'" class="tower-floors">
        <div v-if="floors.length === 0" class="empty-state">暂无楼层数据</div>
        <div
          v-for="floor in floors"
          :key="floor.floor"
          class="floor-card"
          :class="{ cleared: floor.isCleared, current: floor.floor === myInfo?.currentFloor, milestone: floor.isMilestone }"
        >
          <div class="floor-info">
            <span class="floor-num">第{{ floor.floor }}层</span>
            <span class="floor-monster">{{ floor.monsterName }} Lv.{{ floor.monsterLevel }}</span>
            <span v-if="floor.isMilestone" class="floor-milestone">里程碑</span>
          </div>
          <div class="floor-reward">{{ floor.rewardPreview }}</div>
          <span class="floor-status">
            <template v-if="floor.isCleared">已通关</template>
            <template v-else-if="floor.floor === myInfo?.currentFloor">可挑战</template>
            <template v-else>未解锁</template>
          </span>
        </div>
      </div>

      <!-- 挑战记录 -->
      <div v-if="currentView === 'history'" class="tower-history">
        <div v-if="history.length === 0" class="empty-state">暂无挑战记录</div>
        <div
          v-for="log in history"
          :key="log.id"
          class="history-item"
          :class="{ win: log.isWin, lose: !log.isWin }"
        >
          <span class="history-result">{{ log.isWin ? '胜' : '负' }}</span>
          <span class="history-floor">第{{ log.floor }}层</span>
          <span class="history-time">{{ formatTime(log.createdAt) }}</span>
          <button v-if="log.battleLogJson" class="replay-btn" @click="openReplayFromHistory(log)">回放</button>
        </div>
      </div>

      <!-- 排行榜 -->
      <div v-if="currentView === 'leaderboard'" class="tower-history">
        <div v-if="leaderboard.length === 0" class="empty-state">暂无排行数据</div>
        <div
          v-for="(entry, idx) in leaderboard"
          :key="entry.playerId || idx"
          class="leaderboard-item"
          :class="{ 'top3': idx < 3 }"
        >
          <span class="lb-rank" :class="{ 'rank-gold': idx === 0, 'rank-silver': idx === 1, 'rank-bronze': idx === 2 }">{{ idx + 1 }}</span>
          <span class="lb-name">{{ entry.playerName }}</span>
          <span class="lb-floor">第{{ entry.highestFloor }}层</span>
          <span class="lb-time" v-if="entry.bestClearTimeMs">{{ (entry.bestClearTimeMs / 1000).toFixed(1) }}s</span>
        </div>
      </div>

      <!-- 挑战按钮 -->
      <div class="tower-challenge-area">
        <button
          class="challenge-main-btn"
          :disabled="isLoading || !myInfo || myInfo.dailyAttemptsRemaining <= 0 || myInfo.currentFloor > 100"
          @click="handleChallenge"
        >
          <template v-if="!myInfo">加载中...</template>
          <template v-else-if="myInfo.currentFloor > 100">已通关全部楼层</template>
          <template v-else-if="myInfo.dailyAttemptsRemaining <= 0">今日次数已用完</template>
          <template v-else>挑战第{{ myInfo.currentFloor }}层</template>
        </button>
      </div>

      <!-- 挑战结果 -->
      <div v-if="challengeResult" class="challenge-result-overlay">
        <div class="challenge-result-card">
          <h3 :class="challengeResult.isWin ? 'result-win' : 'result-lose'">
            {{ challengeResult.isWin ? '挑战胜利！' : '挑战失败' }}
          </h3>
          <div class="result-details">
            <div class="result-row">
              <span>楼层</span>
              <span>第{{ challengeResult.floor }}层</span>
            </div>
            <div v-if="challengeResult.isWin" class="result-row">
              <span>奖励</span>
              <span>金币x{{ challengeResult.rewardGold }} 经验x{{ challengeResult.rewardExp }}</span>
            </div>
            <div class="result-row">
              <span>最高层</span>
              <span>第{{ challengeResult.newHighestFloor }}层</span>
            </div>
            <div class="result-row">
              <span>剩余次数</span>
              <span>{{ challengeResult.dailyAttemptsRemaining }}</span>
            </div>
          </div>
          <div class="result-actions">
            <button v-if="challengeResult.battleLogJson" class="replay-result-btn" @click="openReplayFromResult">查看回放</button>
            <button class="action-btn" @click="challengeResult = null">继续</button>
          </div>
        </div>
      </div>

      <!-- 战斗回放弹窗 -->
      <BattleReplayModal
        v-model="replayVisible"
        :battle-log-json="replayLogJson"
        :attacker-name="replayAttackerName"
        :defender-name="replayDefenderName"
        :is-win="replayIsWin"
      />
    </div>
  </XiuXianModal>
</template>

<script>
import { ref, watch } from 'vue'
import XiuXianModal from '../common/XiuXianModal.vue'
import BattleReplayModal from './BattleReplayModal.vue'
import { apiClient } from '../../lib/apiClient.js'

export default {
  name: 'TowerModal',
  components: { XiuXianModal, BattleReplayModal },
  props: {
    modelValue: { type: Boolean, default: false }
  },
  emits: ['update:modelValue'],
  setup(props) {
    const isLoading = ref(false)
    const currentView = ref('floors')
    const myInfo = ref(null)
    const floors = ref([])
    const history = ref([])
    const challengeResult = ref(null)
    const leaderboard = ref([])

    // 回放状态
    const replayVisible = ref(false)
    const replayLogJson = ref('')
    const replayAttackerName = ref('')
    const replayDefenderName = ref('')
    const replayIsWin = ref(null)

    async function loadMyInfo() {
      try {
        myInfo.value = await apiClient.getTowerMe()
      } catch (e) {
        console.error('加载通天塔信息失败', e)
      }
    }

    async function loadFloors() {
      isLoading.value = true
      try {
        floors.value = await apiClient.getTowerFloors()
      } catch (e) {
        console.error('加载楼层失败', e)
      } finally {
        isLoading.value = false
      }
    }

    async function loadHistory() {
      isLoading.value = true
      try {
        history.value = await apiClient.getTowerHistory(20)
      } catch (e) {
        console.error('加载历史失败', e)
      } finally {
        isLoading.value = false
      }
    }

    async function loadLeaderboard() {
      try {
        leaderboard.value = await apiClient.getTowerLeaderboard(50)
      } catch (e) {
        console.error('加载排行榜失败', e)
      }
    }

    async function handleChallenge() {
      isLoading.value = true
      try {
        const result = await apiClient.towerChallenge()
        challengeResult.value = result
        await loadMyInfo()
        await loadFloors()
      } catch (e) {
        console.error('挑战失败', e)
      } finally {
        isLoading.value = false
      }
    }

    async function handleBuyAttempts() {
      isLoading.value = true
      try {
        await apiClient.buyTowerAttempts()
        await loadMyInfo()
      } catch (e) {
        console.error('购买失败', e)
      } finally {
        isLoading.value = false
      }
    }

    function formatTime(value) {
      if (!value) return ''
      return value.replace('T', ' ').slice(0, 16)
    }

    function getMonsterName(floorNum) {
      const f = floors.value.find(f => f.floor === floorNum)
      return f ? f.monsterName : '怪物'
    }

    function openReplayFromHistory(log) {
      replayAttackerName.value = '你'
      replayDefenderName.value = getMonsterName(log.floor)
      replayIsWin.value = log.isWin
      replayLogJson.value = log.battleLogJson || ''
      replayVisible.value = true
    }

    function openReplayFromResult() {
      if (!challengeResult.value) return
      replayAttackerName.value = '你'
      replayDefenderName.value = getMonsterName(challengeResult.value.floor)
      replayIsWin.value = challengeResult.value.isWin
      replayLogJson.value = challengeResult.value.battleLogJson || ''
      replayVisible.value = true
    }

    watch(() => props.modelValue, (val) => {
      if (val) {
        currentView.value = 'floors'
        challengeResult.value = null
        Promise.all([loadMyInfo(), loadFloors()])
      }
    })

    watch(currentView, (view) => {
      if (view === 'history' && history.value.length === 0) {
        loadHistory()
      }
      if (view === 'leaderboard' && leaderboard.value.length === 0) {
        loadLeaderboard()
      }
    })

    return {
      isLoading, currentView, myInfo, floors, history, challengeResult, leaderboard,
      replayVisible, replayLogJson, replayAttackerName, replayDefenderName, replayIsWin,
      loadFloors, handleChallenge, handleBuyAttempts, formatTime,
      openReplayFromHistory, openReplayFromResult
    }
  }
}
</script>

<style scoped>
.tower-modal {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  position: relative;
  overflow: hidden;
  max-height: 70vh;
}

.tower-my-info {
  padding: 0.75rem;
  background: var(--bg-secondary, #f8fafc);
  border-radius: 8px;
}

.tower-stats {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 0.5rem;
  margin-bottom: 0.5rem;
}

.stat-item {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.stat-label {
  font-size: 0.7rem;
  color: var(--text-muted, #94a3b8);
}

.stat-value {
  font-size: 1rem;
  font-weight: 700;
  color: var(--text-primary, #1e293b);
}

.tower-daily {
  display: flex;
  align-items: center;
  justify-content: space-between;
  font-size: 0.8rem;
  color: var(--text-secondary, #64748b);
}

.tower-tabs {
  display: flex;
  gap: 0.5rem;
}

.tab-btn {
  flex: 1;
  padding: 0.5rem;
  border: 1px solid var(--border-color, #e2e8f0);
  border-radius: 6px;
  background: transparent;
  cursor: pointer;
  font-size: 0.8rem;
  color: var(--text-secondary, #64748b);
  transition: all 0.15s;
}

.tab-btn.active {
  background: var(--accent-soft-bg);
  color: var(--text-primary);
  border-color: var(--text-secondary);
}

.tower-floors {
  flex: 1;
  overflow-y: auto;
  max-height: 400px;
  padding-right: 0.25rem;
}

.tower-floors::-webkit-scrollbar {
  width: 4px;
}

.tower-floors::-webkit-scrollbar-thumb {
  background: var(--border-color, #d1d5db);
  border-radius: 2px;
}

.floor-card {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.4rem 0.75rem;
  border-radius: 6px;
  background: var(--bg-secondary, #f8fafc);
  margin-bottom: 0.25rem;
  font-size: 0.8rem;
  transition: background 0.15s;
}

.floor-card:hover {
  background: var(--bg-hover, #f1f5f9);
}

.floor-card.cleared { opacity: 0.6; }
.floor-card.current { border: 1px solid var(--text-primary); background: var(--accent-soft-bg); }
.floor-card.milestone { border-left: 3px solid var(--quality-legendary, #f59e0b); }

.floor-info {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.floor-num {
  font-weight: 700;
  color: var(--text-primary);
  min-width: 3.5rem;
}

.floor-monster {
  font-size: 0.75rem;
}

.floor-milestone {
  font-size: 0.65rem;
  background: var(--quality-legendary-bg, #fef3c7);
  color: var(--quality-legendary-text, #92400e);
  padding: 1px 4px;
  border-radius: 3px;
}

.floor-reward {
  font-size: 0.7rem;
  color: var(--text-muted, #94a3b8);
}

.floor-status {
  font-size: 0.7rem;
  color: var(--text-muted, #94a3b8);
  min-width: 3rem;
  text-align: right;
}

.floor-card.cleared .floor-status { color: #22c55e; }
.floor-card.current .floor-status { color: var(--primary-color, #3b82f6); font-weight: 600; }

.history-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.375rem 0.5rem;
  border-radius: 4px;
  margin-bottom: 0.25rem;
  font-size: 0.8rem;
}

.history-item.win { background: var(--status-success-bg, #f0fdf4); }
.history-item.lose { background: var(--status-error-bg, #fef2f2); }

.history-result {
  font-weight: 700;
  min-width: 1.5rem;
}

.history-item.win .history-result { color: var(--status-success-text, #22c55e); }
.history-item.lose .history-result { color: var(--status-error-text, #ef4444); }

.history-floor {
  flex: 1;
}

.history-time {
  font-size: 0.7rem;
  color: var(--text-muted, #94a3b8);
}

.tower-history {
  flex: 1;
  overflow-y: auto;
  max-height: 400px;
}

.tower-challenge-area {
  margin-top: 0.5rem;
  padding-top: 0.5rem;
  border-top: 1px solid var(--border-color, #e2e8f0);
  flex-shrink: 0;
}

.challenge-main-btn {
  width: 100%;
  padding: 0.75rem;
  border-radius: 8px;
  border: none;
  background: linear-gradient(135deg, #3b82f6, #8b5cf6);
  color: #fff;
  font-size: 1rem;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.2s;
}

.challenge-main-btn:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(59, 130, 246, 0.4);
}

.challenge-main-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.action-btn {
  padding: 0.25rem 0.75rem;
  border-radius: 4px;
  border: 1px solid var(--border-color, #e2e8f0);
  background: var(--bg-primary, #fff);
  cursor: pointer;
  font-size: 0.75rem;
  transition: all 0.15s;
}

.action-btn:hover:not(:disabled) {
  background: var(--bg-hover, #f1f5f9);
}

.action-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.challenge-result-overlay {
  position: absolute;
  inset: 0;
  background: rgba(0, 0, 0, 0.7);
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 8px;
  z-index: 10;
}

.challenge-result-card {
  background: var(--bg-primary, #fff);
  border-radius: 12px;
  padding: 1.5rem;
  text-align: center;
  min-width: 280px;
}

.result-win { color: var(--status-success-text, #22c55e); }
.result-lose { color: var(--status-error-text, #ef4444); }

.result-details {
  margin: 1rem 0;
}

.result-row {
  display: flex;
  justify-content: space-between;
  padding: 0.25rem 0;
  font-size: 0.875rem;
}

.result-actions {
  display: flex;
  gap: 0.5rem;
  justify-content: center;
  margin-top: 1rem;
}

.replay-btn {
  padding: 0.15rem 0.5rem;
  border-radius: 4px;
  border: 1px solid var(--border-color);
  background: var(--button-secondary-start);
  color: var(--button-secondary-text);
  font-size: 0.7rem;
  cursor: pointer;
  transition: all 0.15s;
}

.replay-btn:hover {
  background: var(--button-secondary-hover-start);
  color: var(--button-secondary-hover-text);
}

.replay-result-btn {
  padding: 0.4rem 1rem;
  border-radius: 6px;
  border: 1px solid var(--button-primary-start);
  background: var(--button-primary-start);
  color: var(--button-primary-text);
  font-size: 0.85rem;
  cursor: pointer;
  transition: all 0.15s;
}

.replay-result-btn:hover {
  background: var(--button-primary-hover-start);
  color: var(--button-primary-hover-text);
}

.empty-state {
  text-align: center;
  padding: 1.5rem;
  color: var(--text-muted, #94a3b8);
  font-size: 0.875rem;
}

/* ===== 排行榜 ===== */
.leaderboard-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.375rem 0.5rem;
  border-radius: 4px;
  margin-bottom: 0.25rem;
  font-size: 0.8rem;
  background: var(--bg-secondary, #f8fafc);
}

.leaderboard-item.top3 {
  background: var(--highlight-soft-bg, rgba(212, 168, 83, 0.1));
  border: 1px solid var(--border-color, #e2e8f0);
}

.lb-rank {
  font-weight: 700;
  min-width: 1.5rem;
  text-align: center;
  color: var(--text-muted, #94a3b8);
  font-family: var(--font-mono);
}

.lb-rank.rank-gold { color: var(--quality-legendary, #f59e0b); }
.lb-rank.rank-silver { color: var(--quality-common, #94a3b8); }
.lb-rank.rank-bronze { color: var(--quality-uncommon, #22c55e); }

.lb-name {
  flex: 1;
  font-weight: 500;
  color: var(--text-primary, #1e293b);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.lb-floor {
  font-weight: 600;
  color: var(--primary-color, #3b82f6);
  white-space: nowrap;
}

.lb-time {
  font-size: 0.7rem;
  color: var(--text-muted, #94a3b8);
  font-family: var(--font-mono);
  white-space: nowrap;
}
</style>
