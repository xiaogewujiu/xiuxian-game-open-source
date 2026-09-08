<template>
  <XiuXianModal :model-value="modelValue" title="竞技场" :width="560" @update:model-value="$emit('update:modelValue', $event)">
    <div class="arena-modal">
      <!-- 我的信息 -->
      <div class="arena-my-info" v-if="myInfo">
        <div class="my-rank-display">
          <span class="rank-hash">#</span>
          <span class="rank-number">{{ myInfo.rank }}</span>
          <span class="rank-label">当前排名</span>
        </div>
        <div class="my-stats-grid">
          <div class="stat-cell">
            <span class="stat-val">{{ myInfo.points }}</span>
            <span class="stat-lbl">积分</span>
          </div>
          <div class="stat-cell">
            <span class="stat-val">{{ myInfo.wins }}胜{{ myInfo.losses }}负</span>
            <span class="stat-lbl">战绩</span>
          </div>
          <div class="stat-cell">
            <span class="stat-val">{{ myInfo.winStreak }}</span>
            <span class="stat-lbl">连胜</span>
          </div>
          <div class="stat-cell">
            <span class="stat-val">#{{ myInfo.bestRank || '-' }}</span>
            <span class="stat-lbl">最佳排名</span>
          </div>
        </div>
        <div class="daily-bar">
          <div class="daily-info">
            <span class="daily-label">今日剩余挑战</span>
            <span class="daily-count">{{ myInfo.dailyAttemptsRemaining }}/{{ myInfo.dailyAttemptsMax }}</span>
          </div>
          <div class="daily-progress-track">
            <div class="daily-progress-fill" :style="{ width: dailyPercent + '%' }"></div>
          </div>
          <button class="buy-btn" :disabled="isLoading || myInfo.purchasableAttempts <= 0" @click="handleBuyAttempts">
            购买次数 ({{ myInfo.purchasableAttempts }})
          </button>
        </div>
      </div>

      <!-- Tab 栏 -->
      <div class="arena-tabs">
        <button class="tab-btn" :class="{ active: currentView === 'opponents' }" @click="currentView = 'opponents'">
          <span class="tab-text">挑战对手</span>
          <span class="tab-indicator" v-if="currentView === 'opponents'"></span>
        </button>
        <button class="tab-btn" :class="{ active: currentView === 'history' }" @click="currentView = 'history'">
          <span class="tab-text">对战历史</span>
          <span class="tab-indicator" v-if="currentView === 'history'"></span>
        </button>
        <button class="tab-btn" :class="{ active: currentView === 'season' }" @click="currentView = 'season'">
          <span class="tab-text">赛季信息</span>
          <span class="tab-indicator" v-if="currentView === 'season'"></span>
        </button>
      </div>

      <!-- 推荐对手 -->
      <div v-if="currentView === 'opponents'" class="arena-content">
        <div v-if="opponents.length === 0" class="empty-state">暂无推荐对手</div>
        <div
          v-for="(opp, idx) in opponents"
          :key="opp.playerId"
          class="opponent-card"
          :class="{ 'rank-gold': idx === 0, 'rank-silver': idx === 1, 'rank-bronze': idx === 2 }"
        >
          <div class="opp-rank-badge" :class="{ 'top3': idx < 3 }">
            <span class="rank-num">{{ opp.rank }}</span>
          </div>
          <div class="opp-main">
            <div class="opp-name-row">
              <span class="opp-name">{{ opp.playerName }}</span>
              <span class="opp-level">Lv.{{ opp.level }}</span>
            </div>
            <span class="opp-points">{{ opp.points }} 积分</span>
          </div>
          <button
            class="challenge-btn"
            :disabled="isLoading || (myInfo && myInfo.dailyAttemptsRemaining <= 0)"
            @click="handleChallenge(opp.playerId)"
          >挑战</button>
        </div>
        <button class="refresh-btn" :disabled="isLoading" @click="loadOpponents">
          <span class="refresh-icon">&#8635;</span> 刷新对手
        </button>
      </div>

      <!-- 对战历史 -->
      <div v-if="currentView === 'history'" class="arena-content">
        <div v-if="history.length === 0" class="empty-state">暂无对战记录</div>
        <div
          v-for="log in history"
          :key="log.id"
          class="history-item"
          :class="{ win: log.isWin, lose: !log.isWin }"
        >
          <span class="history-result-badge" :class="log.isWin ? 'win' : 'lose'">{{ log.isWin ? '胜' : '负' }}</span>
          <div class="history-detail">
            <span class="history-role">{{ log.isAttacker ? '挑战' : '防守' }}</span>
            <span class="history-opp">{{ log.opponentName }}</span>
          </div>
          <span class="history-points" :class="{ positive: log.pointsChange > 0, negative: log.pointsChange < 0 }">
            {{ log.pointsChange > 0 ? '+' : '' }}{{ log.pointsChange }}
          </span>
          <span class="history-time">{{ formatTime(log.battleTime) }}</span>
          <button v-if="log.battleLogJson" class="replay-btn" @click="openReplay(log)">回放</button>
        </div>
      </div>

      <!-- 赛季信息 -->
      <div v-if="currentView === 'season'" class="arena-content">
        <div v-if="!seasonInfo" class="empty-state">加载中...</div>
        <div v-else class="season-panel">
          <div class="season-header">
            <div class="season-number">第 {{ seasonInfo.seasonNumber }} 赛季</div>
            <div class="season-time" v-if="seasonInfo.timeRemaining">剩余 {{ seasonInfo.timeRemaining }}</div>
          </div>
          <div class="season-my-stats">
            <div class="season-stat">
              <span class="season-stat-label">我的排名</span>
              <span class="season-stat-val">#{{ seasonInfo.myRank || '--' }}</span>
            </div>
            <div class="season-stat">
              <span class="season-stat-label">我的积分</span>
              <span class="season-stat-val">{{ seasonInfo.myPoints || 0 }}</span>
            </div>
          </div>
          <div class="season-rewards-title">赛季结算奖励</div>
          <div v-if="seasonInfo.rewards && seasonInfo.rewards.length" class="season-rewards-list">
            <div v-for="(reward, idx) in seasonInfo.rewards" :key="idx" class="season-reward-item">
              <span class="reward-rank">第{{ reward.minRank }}-{{ reward.maxRank }}名</span>
              <span class="reward-detail">
                <template v-if="reward.gold">金币 {{ reward.gold }}</template>
                <template v-if="reward.spiritStone"> 灵石 {{ reward.spiritStone }}</template>
                <template v-if="reward.title"> 称号「{{ reward.title }}」</template>
                <template v-if="reward.rewardTitle"> {{ reward.rewardTitle }}</template>
              </span>
            </div>
          </div>
          <div v-else class="empty-state">暂无赛季奖励配置</div>
        </div>
      </div>

      <!-- 挑战结果 -->
      <div v-if="challengeResult" class="challenge-overlay">
        <div class="challenge-result-card">
          <div class="result-banner" :class="challengeResult.isWin ? 'result-win' : 'result-lose'">
            <span class="result-icon">{{ challengeResult.isWin ? '&#9733;' : '&#10006;' }}</span>
            <span class="result-title">{{ challengeResult.isWin ? '挑战胜利！' : '挑战失败' }}</span>
          </div>
          <div class="result-details">
            <div class="result-row">
              <span class="result-label">排名变化</span>
              <span class="result-val">#{{ challengeResult.oldRank }} &#8594; #{{ challengeResult.newRank }}</span>
            </div>
            <div class="result-row">
              <span class="result-label">积分变化</span>
              <span class="result-val" :class="challengeResult.pointsChange > 0 ? 'positive' : 'negative'">
                {{ challengeResult.newPoints }} ({{ challengeResult.pointsChange > 0 ? '+' : '' }}{{ challengeResult.pointsChange }})
              </span>
            </div>
            <div class="result-row">
              <span class="result-label">战斗奖励</span>
              <span class="result-val reward-text">金币 x{{ challengeResult.rewardGold }} &nbsp; 荣誉 x{{ challengeResult.rewardHonor }}</span>
            </div>
          </div>
          <div class="result-actions">
            <button v-if="challengeResult.battleLogJson" class="replay-result-btn" @click="openReplayFromResult">查看回放</button>
            <button class="continue-btn" @click="challengeResult = null">继续</button>
          </div>
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
  </XiuXianModal>
</template>

<script>
import { ref, computed, watch } from 'vue'
import XiuXianModal from '../common/XiuXianModal.vue'
import BattleReplayModal from './BattleReplayModal.vue'
import { apiClient } from '../../lib/apiClient.js'

export default {
  name: 'ArenaModal',
  components: { XiuXianModal, BattleReplayModal },
  props: {
    modelValue: { type: Boolean, default: false },
    playerName: { type: String, default: '我' }
  },
  emits: ['update:modelValue'],
  setup(props) {
    const isLoading = ref(false)
    const currentView = ref('opponents')
    const myInfo = ref(null)
    const opponents = ref([])
    const history = ref([])
    const challengeResult = ref(null)
    const seasonInfo = ref(null)

    // 回放状态
    const replayVisible = ref(false)
    const replayLogJson = ref('')
    const replayAttackerName = ref('')
    const replayDefenderName = ref('')
    const replayIsWin = ref(null)

    const dailyPercent = computed(() => {
      if (!myInfo.value || !myInfo.value.dailyAttemptsMax) return 0
      return (myInfo.value.dailyAttemptsRemaining / myInfo.value.dailyAttemptsMax) * 100
    })

    async function loadMyInfo() {
      try {
        myInfo.value = await apiClient.getArenaMe()
      } catch (e) {
        console.error('加载竞技场信息失败', e)
      }
    }

    async function loadOpponents() {
      isLoading.value = true
      try {
        opponents.value = await apiClient.getArenaOpponents()
      } catch (e) {
        console.error('加载对手失败', e)
      } finally {
        isLoading.value = false
      }
    }

    async function loadHistory() {
      isLoading.value = true
      try {
        history.value = await apiClient.getArenaHistory(1, 20)
      } catch (e) {
        console.error('加载历史失败', e)
      } finally {
        isLoading.value = false
      }
    }

    async function loadSeason() {
      try {
        seasonInfo.value = await apiClient.getArenaSeason()
      } catch (e) {
        console.error('加载赛季信息失败', e)
      }
    }

    async function handleChallenge(defenderId) {
      isLoading.value = true
      try {
        const result = await apiClient.arenaChallenge(defenderId)
        challengeResult.value = result
        await loadMyInfo()
        await loadOpponents()
      } catch (e) {
        console.error('挑战失败', e)
      } finally {
        isLoading.value = false
      }
    }

    async function handleBuyAttempts() {
      isLoading.value = true
      try {
        await apiClient.buyArenaAttempts()
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

    function openReplay(log) {
      replayAttackerName.value = log.isAttacker ? props.playerName : log.opponentName
      replayDefenderName.value = log.isAttacker ? log.opponentName : props.playerName
      replayIsWin.value = log.isAttacker ? log.isWin : !log.isWin
      replayLogJson.value = log.battleLogJson || ''
      replayVisible.value = true
    }

    function openReplayFromResult() {
      if (!challengeResult.value) return
      replayAttackerName.value = props.playerName
      // 优先用后端返回的对手名字，fallback 从日志解析
      let defenderName = challengeResult.value.opponentName || ''
      if (!defenderName) {
        try {
          const rounds = JSON.parse(challengeResult.value.battleLogJson || '[]')
          const myName = props.playerName
          for (const round of rounds) {
            for (const entry of (round.Entries || [])) {
              const caster = entry.CasterName || entry.casterName || ''
              const target = entry.TargetName || entry.targetName || ''
              if (caster && caster !== myName) { defenderName = caster; break }
              if (target && target !== myName) { defenderName = target; break }
            }
            if (defenderName) break
          }
        } catch { /* ignore */ }
      }
      replayDefenderName.value = defenderName || '对手'
      replayIsWin.value = challengeResult.value.isWin
      replayLogJson.value = challengeResult.value.battleLogJson || ''
      replayVisible.value = true
    }

    watch(() => props.modelValue, (val) => {
      if (val) {
        currentView.value = 'opponents'
        challengeResult.value = null
        Promise.all([loadMyInfo(), loadOpponents()])
      }
    })

    watch(currentView, (view) => {
      if (view === 'history' && history.value.length === 0) {
        loadHistory()
      }
      if (view === 'season' && !seasonInfo.value) {
        loadSeason()
      }
    })

    return {
      isLoading, currentView, myInfo, opponents, history, challengeResult, dailyPercent,
      seasonInfo,
      replayVisible, replayLogJson, replayAttackerName, replayDefenderName, replayIsWin,
      loadOpponents, handleChallenge, handleBuyAttempts, formatTime,
      openReplay, openReplayFromResult
    }
  }
}
</script>

<style scoped>
.arena-modal {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  position: relative;
}

/* ===== 我的信息 ===== */
.arena-my-info {
  background: var(--highlight-soft-bg);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-lg);
  padding: 0.875rem;
  display: flex;
  flex-direction: column;
  gap: 0.625rem;
}

.my-rank-display {
  display: flex;
  align-items: baseline;
  justify-content: center;
  gap: 2px;
}

.rank-hash {
  font-size: 1rem;
  color: var(--accent-text);
  font-weight: 600;
}

.rank-number {
  font-size: 2.2rem;
  font-weight: 900;
  color: var(--highlight-text-strong);
  line-height: 1;
}

.rank-label {
  font-size: 0.7rem;
  color: var(--text-muted);
  margin-left: 0.5rem;
  align-self: center;
}

.my-stats-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 0.375rem;
}

.stat-cell {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 0.375rem 0;
  background: var(--bg-overlay-light);
  border-radius: var(--radius-sm);
}

.stat-val {
  font-size: 0.9rem;
  font-weight: 700;
  color: var(--text-primary);
}

.stat-lbl {
  font-size: 0.65rem;
  color: var(--text-muted);
  margin-top: 2px;
}

.daily-bar {
  display: flex;
  align-items: center;
  gap: 0.625rem;
}

.daily-info {
  display: flex;
  flex-direction: column;
  white-space: nowrap;
}

.daily-label {
  font-size: 0.65rem;
  color: var(--text-muted);
}

.daily-count {
  font-size: 0.85rem;
  font-weight: 700;
  color: var(--accent-text);
}

.daily-progress-track {
  flex: 1;
  height: 6px;
  background: var(--bg-overlay-dark);
  border-radius: 3px;
  overflow: hidden;
}

.daily-progress-fill {
  height: 100%;
  background: var(--jade-primary);
  border-radius: 3px;
  transition: width 0.3s;
}

.buy-btn {
  padding: 0.25rem 0.625rem;
  border: 1px solid var(--control-border);
  border-radius: var(--radius-sm);
  background: var(--control-bg);
  color: var(--control-text);
  font-size: 0.7rem;
  cursor: pointer;
  transition: all 0.15s;
  white-space: nowrap;
}

.buy-btn:hover:not(:disabled) {
  background: var(--control-bg-hover);
}

.buy-btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

/* ===== Tab 栏 ===== */
.arena-tabs {
  display: flex;
  gap: 0;
  border-bottom: 1px solid var(--border-color);
}

.tab-btn {
  flex: 1;
  padding: 0.5rem 0;
  background: transparent;
  border: none;
  cursor: pointer;
  position: relative;
  transition: all 0.15s;
}

.tab-text {
  font-size: 0.85rem;
  color: var(--text-secondary);
  transition: color 0.15s;
}

.tab-btn.active .tab-text {
  color: var(--text-primary);
  font-weight: 600;
}

.tab-indicator {
  position: absolute;
  bottom: -1px;
  left: 20%;
  right: 20%;
  height: 2px;
  background: var(--jade-primary);
  border-radius: 1px;
}

/* ===== 内容区 ===== */
.arena-content {
  display: flex;
  flex-direction: column;
  gap: 0.375rem;
  max-height: 360px;
  overflow-y: auto;
}

.arena-content::-webkit-scrollbar { width: 4px; }
.arena-content::-webkit-scrollbar-thumb { background: var(--scrollbar-thumb); border-radius: 2px; }

/* ===== 对手卡片 ===== */
.opponent-card {
  display: flex;
  align-items: center;
  gap: 0.625rem;
  padding: 0.5rem 0.75rem;
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  background: var(--bg-overlay-light);
  transition: all 0.15s;
}

.opponent-card:hover {
  background: var(--bg-overlay-medium);
}

.opp-rank-badge {
  width: 2rem;
  height: 2rem;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  background: var(--bg-overlay-light);
  border: 1px solid var(--border-color);
  flex-shrink: 0;
}

.opp-rank-badge.top3 {
  border-color: var(--highlight-text);
  background: var(--highlight-soft-bg);
}

.rank-gold .opp-rank-badge { border-color: var(--quality-legendary); background: oklch(0.65 0.15 70 / 0.15); }
.rank-silver .opp-rank-badge { border-color: var(--quality-common); background: oklch(0.7 0 0 / 0.1); }
.rank-bronze .opp-rank-badge { border-color: var(--quality-uncommon); background: oklch(0.6 0.1 145 / 0.1); }

.rank-num {
  font-size: 0.75rem;
  font-weight: 700;
  color: var(--text-primary);
}

.rank-gold .rank-num { color: var(--quality-legendary); }
.rank-silver .rank-num { color: var(--quality-common); }
.rank-bronze .rank-num { color: var(--quality-uncommon); }

.opp-main {
  flex: 1;
  min-width: 0;
}

.opp-name-row {
  display: flex;
  align-items: center;
  gap: 0.375rem;
}

.opp-name {
  font-size: 0.85rem;
  font-weight: 600;
  color: var(--text-primary);
}

.opp-level {
  font-size: 0.65rem;
  color: var(--text-muted);
}

.opp-points {
  font-size: 0.7rem;
  color: var(--text-secondary);
}

.challenge-btn {
  padding: 0.3rem 0.875rem;
  border: 1px solid var(--control-border);
  border-radius: var(--radius-sm);
  background: var(--button-secondary-start);
  color: var(--button-secondary-text);
  font-size: 0.75rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.15s;
  white-space: nowrap;
}

.challenge-btn:hover:not(:disabled) {
  background: var(--button-secondary-hover-start);
  color: var(--button-secondary-hover-text);
}

.challenge-btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.refresh-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.375rem;
  width: 100%;
  padding: 0.4rem;
  border: 1px dashed var(--border-color);
  border-radius: var(--radius-sm);
  background: transparent;
  color: var(--text-muted);
  font-size: 0.75rem;
  cursor: pointer;
  transition: all 0.15s;
}

.refresh-btn:hover:not(:disabled) {
  border-color: var(--jade-primary);
  color: var(--accent-text);
}

.refresh-icon {
  font-size: 0.9rem;
}

/* ===== 对战历史 ===== */
.history-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.375rem 0.625rem;
  border-radius: var(--radius-sm);
  border: 1px solid var(--border-color);
  background: var(--bg-overlay-light);
  transition: background 0.15s;
}

.history-item.win {
  border-left: 3px solid var(--quality-uncommon);
}

.history-item.lose {
  border-left: 3px solid var(--quality-mythic);
}

.history-result-badge {
  width: 1.5rem;
  height: 1.5rem;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 4px;
  font-size: 0.7rem;
  font-weight: 700;
  flex-shrink: 0;
}

.history-result-badge.win {
  background: var(--button-success-start);
  color: var(--button-success-text);
}

.history-result-badge.lose {
  background: var(--button-danger-start);
  color: var(--button-danger-text);
}

.history-detail {
  flex: 1;
  min-width: 0;
  display: flex;
  align-items: center;
  gap: 0.25rem;
}

.history-role {
  font-size: 0.65rem;
  color: var(--text-muted);
  white-space: nowrap;
}

.history-opp {
  font-size: 0.8rem;
  font-weight: 500;
  color: var(--text-primary);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.history-points {
  font-size: 0.8rem;
  font-weight: 700;
  font-family: var(--font-mono);
  white-space: nowrap;
}

.history-points.positive { color: var(--button-success-text); }
.history-points.negative { color: var(--button-danger-text); }

.history-time {
  font-size: 0.65rem;
  color: var(--text-muted);
  white-space: nowrap;
}

.replay-btn {
  padding: 0.2rem 0.5rem;
  border: 1px solid var(--control-border);
  border-radius: var(--radius-sm);
  background: var(--control-bg);
  color: var(--accent-text);
  font-size: 0.65rem;
  cursor: pointer;
  transition: all 0.15s;
  white-space: nowrap;
}

.replay-btn:hover {
  background: var(--control-bg-hover);
}

/* ===== 挑战结果 ===== */
.challenge-overlay {
  position: absolute;
  inset: 0;
  background: var(--overlay-bg);
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: var(--radius-md);
  z-index: 10;
  backdrop-filter: blur(2px);
}

.challenge-result-card {
  background: var(--xiuxian-bg-secondary);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-lg);
  padding: 1.25rem;
  text-align: center;
  min-width: 300px;
  max-width: 380px;
  box-shadow: var(--shadow-lg);
}

.result-banner {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.375rem;
  margin-bottom: 1rem;
}

.result-icon {
  font-size: 2rem;
}

.result-win .result-icon { color: var(--quality-legendary); }
.result-lose .result-icon { color: var(--quality-mythic); }

.result-title {
  font-size: 1.2rem;
  font-weight: 900;
}

.result-win .result-title { color: var(--highlight-text-strong); }
.result-lose .result-title { color: var(--quality-mythic); }

.result-details {
  margin-bottom: 1rem;
}

.result-row {
  display: flex;
  justify-content: space-between;
  padding: 0.3rem 0.5rem;
  font-size: 0.8rem;
}

.result-label {
  color: var(--text-muted);
}

.result-val {
  font-weight: 600;
  color: var(--text-primary);
}

.result-val.positive { color: var(--button-success-text); }
.result-val.negative { color: var(--button-danger-text); }

.reward-text {
  color: var(--highlight-text);
}

.result-actions {
  display: flex;
  gap: 0.5rem;
  justify-content: center;
}

.replay-result-btn {
  padding: 0.4rem 1rem;
  border: 1px solid var(--control-border);
  border-radius: var(--radius-md);
  background: var(--control-bg);
  color: var(--accent-text);
  font-size: 0.8rem;
  cursor: pointer;
  transition: all 0.15s;
}

.replay-result-btn:hover {
  background: var(--control-bg-hover);
}

.continue-btn {
  padding: 0.4rem 1.5rem;
  border: 1px solid transparent;
  border-radius: var(--radius-md);
  background: var(--button-primary-start);
  color: var(--button-primary-text);
  font-size: 0.8rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.15s;
}

.continue-btn:hover {
  background: var(--button-primary-hover-start);
  color: var(--button-primary-hover-text);
}

/* ===== 通用 ===== */
.empty-state {
  text-align: center;
  padding: 1.5rem;
  color: var(--text-muted);
  font-size: 0.85rem;
}

/* ===== 赛季信息 ===== */
.season-panel {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.season-header {
  text-align: center;
  padding: 0.75rem;
  background: var(--highlight-soft-bg);
  border-radius: var(--radius-md);
  border: 1px solid var(--border-color);
}

.season-number {
  font-size: 1.1rem;
  font-weight: 900;
  color: var(--highlight-text-strong);
}

.season-time {
  font-size: 0.75rem;
  color: var(--text-muted);
  margin-top: 0.25rem;
}

.season-my-stats {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 0.5rem;
}

.season-stat {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 0.5rem;
  background: var(--bg-overlay-light);
  border-radius: var(--radius-sm);
  border: 1px solid var(--border-color);
}

.season-stat-label {
  font-size: 0.65rem;
  color: var(--text-muted);
}

.season-stat-val {
  font-size: 1.1rem;
  font-weight: 800;
  color: var(--accent-text);
  font-family: var(--font-mono);
}

.season-rewards-title {
  font-size: 0.8rem;
  font-weight: 600;
  color: var(--text-primary);
  padding-bottom: 0.25rem;
  border-bottom: 1px solid var(--border-color);
}

.season-rewards-list {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.season-reward-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.375rem 0.5rem;
  background: var(--bg-overlay-light);
  border-radius: var(--radius-sm);
  font-size: 0.75rem;
}

.reward-rank {
  font-weight: 600;
  color: var(--highlight-text);
  white-space: nowrap;
}

.reward-detail {
  color: var(--text-secondary);
  text-align: right;
}
</style>
