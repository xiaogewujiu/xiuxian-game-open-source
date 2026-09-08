<template>
  <!-- 排行榜 Modal -->
  <XiuXianModal :model-value="modelValue" title="排行榜" :width="600" @update:model-value="$emit('update:modelValue', $event)">
    <div class="rankings-modal">
      <!-- 排行榜类型切换 -->
      <div class="ranking-tabs">
        <button
          v-for="tab in rankingTabs"
          :key="tab.key"
          class="tab-btn"
          :class="{ active: currentTab === tab.key }"
          @click="currentTab = tab.key"
        >
          <span class="tab-icon"><AssetIcon :source="tab.icon" size="20" /></span>
          <span class="tab-name">{{ tab.name }}</span>
        </button>
      </div>

      <!-- 排行榜列表 -->
      <div class="ranking-list">
        <!-- 前三名特殊展示 -->
        <div class="top-three">
          <div
            v-for="(player, index) in topThree"
            :key="player.id"
            class="top-player"
            :class="`rank-${index + 1}`"
          >
            <div class="rank-badge">{{ index + 1 }}</div>
            <div class="player-avatar">{{ player.avatar }}</div>
            <div class="player-name">{{ player.name }} <span v-if="player.title" class="player-title">[{{ player.title }}]</span></div>
            <div class="player-value">{{ formatNumber(player.value) }}</div>
          </div>
        </div>

        <!-- 普通排名 -->
        <div class="normal-ranks">
          <div
            v-for="(player, index) in normalRanks"
            :key="player.id"
            class="rank-item"
            :class="{ 'is-me': player.isMe }"
          >
            <span class="rank-number">{{ index + 4 }}</span>
            <span class="player-avatar-small">{{ player.avatar }}</span>
            <span class="player-name">{{ player.name }} <span v-if="player.title" class="player-title">[{{ player.title }}]</span></span>
            <span class="player-realm">{{ player.realm }}</span>
            <span class="player-value">{{ formatNumber(player.value) }}</span>
          </div>
        </div>
      </div>

      <!-- 我的排名 -->
      <div class="my-rank">
        <div class="my-rank-info">
          <span class="my-rank-label">我的排名</span>
          <span class="my-rank-position">第 {{ myRank.position }} 名</span>
          <span class="my-rank-value">{{ formatNumber(myRank.value) }}</span>
        </div>
      </div>
    </div>
  </XiuXianModal>
</template>

<script>
import { computed, ref, watch } from 'vue'
import XiuXianModal from '../common/XiuXianModal.vue'
import { useGameStore } from '../../state/gameStore'
import { formatCompactNumber, getPlayerAvatar, getRealmInfo } from '../../services/gameDisplay'
import { ICON } from '../../icons'
import AssetIcon from '../common/AssetIcon.vue'

/**
 * RankingsModal - 排行榜弹窗
 *
 * 旧界面的榜单标签页会继续保留，
 * 但底层数据改为真实排行榜接口。
 */
export default {
  name: 'RankingsModal',

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

    // 当前选中的排行榜
    const currentTab = ref('realm')

    // 中文注释：
    // 当前排行榜由后端真实业务数据源构建，前端仅负责展示五个榜单。
    const rankingTabs = [
      { key: 'realm', name: '等级榜', icon: ICON.misc_diamond, rankingId: 'ranking_level' },
      { key: 'gold', name: '财富榜', icon: ICON.misc_coin, rankingId: 'ranking_wealth' },
      { key: 'achievement', name: '成就榜', icon: ICON.misc_sparkles, rankingId: 'ranking_achievement' },
      { key: 'arena', name: '竞技场', icon: ICON.stat_attack, rankingId: 'ranking_arena' },
      { key: 'tower', name: '通天塔', icon: ICON.misc_diamond, rankingId: 'ranking_tower' }
    ]

    const activeRankingId = computed(() => {
      return rankingTabs.find((tab) => tab.key === currentTab.value)?.rankingId || 'ranking_level'
    })

    const ensureRankingLoaded = async () => {
      try {
        await gameStore.loadRanking(activeRankingId.value, 10, true)
      } catch (error) {
        // 中文注释：
        // 某一个榜单加载失败时，保留空列表显示，避免整个弹窗直接报错打不开。
      }
    }

    watch(() => props.modelValue, async (visible) => {
      if (visible) {
        await ensureRankingLoaded()
      }
    }, { immediate: true })

    watch(currentTab, async () => {
      if (props.modelValue) {
        await ensureRankingLoaded()
      }
    })

    const currentRankings = computed(() => {
      const list = gameStore.state.rankings[activeRankingId.value] || []
      return list.map((entry, index) => {
        const level = Number(entry.Level ?? entry.level ?? 1) || 1
        const realmInfo = getRealmInfo(level)
        return {
          id: entry.PlayerId ?? entry.playerId ?? `rank_${index}`,
          avatar: getPlayerAvatar(level),
          name: entry.PlayerName ?? entry.playerName ?? '未知修士',
          realm: realmInfo.realm,
          value: Number(entry.Score ?? entry.score ?? 0) || 0,
          rank: Number(entry.Rank ?? entry.rank ?? index + 1) || index + 1,
          isMe: (entry.PlayerId ?? entry.playerId ?? '') === (gameStore.state.player?.Id ?? gameStore.state.player?.id ?? ''),
          title: entry.CurrentTitle ?? entry.currentTitle ?? ''
        }
      })
    })

    const topThree = computed(() => currentRankings.value.slice(0, 3))
    const normalRanks = computed(() => currentRankings.value.slice(3, 10))

    const myRank = computed(() => {
      const me = currentRankings.value.find((item) => item.isMe)
      if (me) {
        return {
          position: me.rank,
          value: me.value
        }
      }

      return {
        position: '--',
        value: 0
      }
    })

    return {
      ICON,
      currentTab,
      rankingTabs,
      topThree,
      normalRanks,
      myRank,
      formatNumber: formatCompactNumber
    }
  }
}
</script>

<style scoped>
.rankings-modal {
  padding: var(--spacing-md);
}

.ranking-list {
  height: min(460px, 65vh);
  min-height: 0;
  overflow-y: auto;
}

/* 排行榜标签 */
.ranking-tabs {
  display: flex;
  gap: var(--spacing-xs);
  margin-bottom: var(--spacing-lg);
  padding-bottom: var(--spacing-md);
  border-bottom: 1px solid var(--border-color);
}

.tab-btn {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
  padding: var(--spacing-sm);
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
  font-size: 20px;
}

.tab-name {
  font-size: var(--font-size-base);
}

/* 前三名 */
.top-three {
  display: flex;
  justify-content: center;
  align-items: flex-end;
  gap: var(--spacing-lg);
  margin-bottom: var(--spacing-lg);
  padding: var(--spacing-md);
  background: linear-gradient(180deg, rgba(212, 168, 83, 0.1), transparent);
  border-radius: var(--radius-md);
}

.top-player {
  text-align: center;
  position: relative;
}

.top-player.rank-1 {
  transform: scale(1.2);
}

.top-player.rank-2 {
  transform: scale(1);
}

.top-player.rank-3 {
  transform: scale(0.9);
}

.rank-badge {
  width: 30px;
  height: 30px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  margin: 0 auto var(--spacing-sm);
}

.rank-1 .rank-badge {
  background: linear-gradient(135deg, #ffd700, #ffed4a);
  color: var(--text-on-dark);
  box-shadow: 0 0 15px rgba(255, 215, 0, 0.5);
}

.rank-2 .rank-badge {
  background: linear-gradient(135deg, #c0c0c0, #e0e0e0);
  color: var(--text-on-dark);
}

.rank-3 .rank-badge {
  background: linear-gradient(135deg, #cd7f32, #daa520);
  color: var(--text-on-dark);
}

.player-avatar {
  font-size: 40px;
  margin-bottom: var(--spacing-xs);
}

.top-player .player-name {
  font-size: var(--font-size-base);
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 2px;
}

.top-player .player-value {
  font-size: var(--font-size-base);
  color: var(--highlight-text);
  font-family: var(--font-mono);
}

/* 普通排名 */
.normal-ranks {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-xs);
}

.rank-item {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  padding: var(--spacing-sm) var(--spacing-md);
  background: var(--xiuxian-bg-secondary);
  border-radius: var(--radius-sm);
  transition: all 0.2s ease;
}

.rank-item:hover {
  background: rgba(124, 58, 237, 0.1);
}

.rank-item.is-me {
  background: rgba(212, 168, 83, 0.2);
  border: 1px solid var(--border-color);
}

.rank-number {
  width: 24px;
  text-align: center;
  font-size: 14px;
  font-weight: 600;
  color: var(--text-muted);
  font-family: var(--font-mono);
}

.player-avatar-small {
  font-size: 20px;
}

.rank-item .player-name {
  flex: 1;
  font-size: var(--font-size-base);
  color: var(--text-primary);
}

.player-realm {
  font-size: var(--font-size-sm);
  color: var(--accent-text);
  background: rgba(124, 58, 237, 0.2);
  padding: 2px 8px;
  border-radius: var(--radius-sm);
}

.player-title {
  font-size: 0.7em;
  color: var(--highlight-text);
  font-weight: 400;
  margin-left: 2px;
}

.rank-item .player-value {
  font-size: var(--font-size-base);
  color: var(--highlight-text);
  font-family: var(--font-mono);
  font-weight: 500;
}

/* 我的排名 */
.my-rank {
  margin-top: var(--spacing-lg);
  padding-top: var(--spacing-md);
  border-top: 1px solid var(--border-color);
}

.my-rank-info {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--spacing-md);
  background: rgba(212, 168, 83, 0.1);
  border-radius: var(--radius-md);
}

.my-rank-label {
  font-size: var(--font-size-base);
  color: var(--text-secondary);
}

.my-rank-position {
  font-size: 14px;
  font-weight: 600;
  color: var(--highlight-text);
}

.my-rank-value {
  font-size: 14px;
  color: var(--text-primary);
  font-family: var(--font-mono);
  font-weight: 600;
}
</style>


