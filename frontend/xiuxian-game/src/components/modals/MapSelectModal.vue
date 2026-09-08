<template>
  <!-- 地图选择 Modal -->
  <XiuXianModal :model-value="modelValue" title="选择地图" :width="1080" @update:model-value="$emit('update:modelValue', $event)">
    <div class="map-select-modal">
      <div class="map-browser">
        <!-- 地图列表筛选：不删除未开放地图，只切换当前展示范围。 -->
        <div class="map-browser-header">
          <div>
            <div class="map-browser-title">{{ isPartySelection ? `组队副本 · ${partyTeamSize}人` : '普通地图' }}</div>
            <div class="map-browser-summary">{{ filteredMaps.length }} / {{ maps.length }} 张地图</div>
          </div>
          <div class="map-filter-tabs" role="tablist" aria-label="地图筛选">
            <button
              v-for="filter in mapFilters"
              :key="filter.key"
              class="map-filter-tab"
              :class="{ active: mapFilter === filter.key }"
              type="button"
              @click="mapFilter = filter.key"
            >
              {{ filter.label }}
            </button>
          </div>
        </div>

        <!-- 地图列表 -->
        <div class="map-list">
          <button
            v-for="map in filteredMaps"
            :key="map.id"
            type="button"
            class="map-card"
            :class="{ locked: !map.canChallenge, selected: selectedMap?.id === map.id }"
            @click="selectMap(map)"
          >
            <div class="map-thumbnail"><AssetIcon :source="map.icon" size="28" /></div>
            <div class="map-info">
              <div class="map-name">{{ map.name }}</div>
              <div class="map-recommend">{{ map.recommendRealm }}</div>
            </div>
            <span class="map-status" :class="map.canChallenge ? 'available' : 'locked'">
              {{ map.canChallenge ? '可挑战' : '未开放' }}
            </span>
          </button>
          <div v-if="filteredMaps.length === 0" class="empty-map-list">当前筛选下暂无地图</div>
        </div>
      </div>

      <!-- 地图详情 -->
      <div v-if="selectedMap" class="map-details">
        <div class="details-header">
          <div>
            <div class="details-eyebrow">{{ selectedMap.dungeonId ? '组队副本' : '普通地图' }}</div>
            <h3 class="details-name">{{ selectedMap.name }}</h3>
          </div>
          <span class="details-difficulty" :class="selectedMap.difficulty">
            {{ selectedMap.difficultyText }}
          </span>
        </div>

        <p class="details-desc">{{ selectedMap.description }}</p>

        <!-- 进入条件 -->
        <div class="details-section">
          <div class="section-title">进入条件</div>
          <div class="restrictions">
            <div class="restriction-item">
              <span>推荐境界：</span>
              <span :class="{ unmet: !meetRealmReq }">{{ selectedMap.minRealm }}</span>
            </div>
            <div class="restriction-item">
              <span>进入方式：</span>
              <span>{{ selectedMap.entryMethod }}</span>
            </div>
            <div v-if="selectedMap.dungeonId" class="restriction-item">
              <span>今日奖励：</span>
              <span>{{ selectedMap.todayCount }}/{{ selectedMap.dailyLimit }} 次，剩余 {{ selectedMap.remainingCount }} 次</span>
            </div>
          </div>
        </div>

        <!-- 敌情信息 -->
        <div class="details-section">
          <div class="section-title">敌情信息</div>
          <div class="enemy-info">
            <div class="enemy-item">
              <span>敌人类型：</span>
              <span>{{ selectedMap.enemyTypes }}</span>
            </div>
            <div class="enemy-item">
              <span>敌人等级：</span>
              <span>Lv.{{ selectedMap.enemyLevelMin }} - Lv.{{ selectedMap.enemyLevelMax }}</span>
            </div>
            <div class="enemy-item">
              <span>怪物数量：</span>
              <span>{{ selectedMap.monsterCountText }}</span>
            </div>
          </div>
        </div>

        <!-- 掉落预览 -->
        <div class="details-section">
          <div class="section-title">掉落预览</div>
          <div v-if="selectedMap.drops?.length" class="drop-list">
            <div v-for="(drop, index) in selectedMap.drops" :key="index" class="drop-item">
              <span class="drop-icon"><AssetIcon :source="drop.icon" size="14" /></span>
              <span class="drop-name" :class="drop.quality">{{ drop.name }}</span>
            </div>
          </div>
          <div v-else class="empty-detail">暂无掉落预览</div>
        </div>

        <!-- 未开放提示 -->
        <div v-if="!selectedMap.canChallenge" class="map-warning">
          {{ selectedMap.unavailableReason || '当前地图尚未开放，请先提升角色等级。' }}
        </div>

        <!-- 操作按钮 -->
        <div class="details-actions">
          <button
            class="enter-btn"
            :disabled="!selectedMap.canChallenge"
            type="button"
            @click="enterMap"
          >
            {{ selectedMap.canChallenge ? '确认选择' : '暂不可进入' }}
          </button>
        </div>
      </div>

      <!-- 未选择状态 -->
      <div v-else class="no-selection">
        <div class="hint-icon"><AssetIcon :source="ICON.misc_map" size="25" /></div>
        <p>请选择一张地图查看详情。</p>
      </div>
    </div>
  </XiuXianModal>
</template>

<script>
import { computed, ref, watch } from 'vue'
import XiuXianModal from '../common/XiuXianModal.vue'
import { useGameStore } from '../../state/gameStore'
import { buildMapCards, getRealmInfo } from '../../services/gameDisplay'
import toast from '@/utils/toast'
import { ICON } from '../../icons'
import AssetIcon from '../common/AssetIcon.vue'

/**
 * MapSelectModal - 普通地图选择弹窗
 *
 * 中文注释：
 * 1. 这里只展示普通地图，不再混入副本列表，避免用户在主界面选择地图时看到副本次数逻辑。
 * 2. 弹窗确认后只负责把选中的地图回传给主页面，真正开战仍由主页面的“开始战斗”按钮统一触发。
 * 3. 即使地图暂时不可挑战，也允许用户点开查看详情和未开放原因，方便提前规划发育路线。
 */
export default {
  name: 'MapSelectModal',

  components: {
    XiuXianModal,
    AssetIcon
  },

  props: {
    modelValue: Boolean
  },

  emits: ['update:modelValue', 'enter-map'],

  setup(props, { emit }) {
    const gameStore = useGameStore()
    const maps = computed(() => {
      const party = gameStore.state.currentParty
      const teamSize = Number(party?.maxMembers ?? party?.MaxMembers ?? 0)
      if (!teamSize) {
        return buildMapCards(gameStore.state.maps)
      }
      return (gameStore.state.dungeons || [])
        .filter((dungeon) => Number(dungeon?.requiredTeamSize ?? dungeon?.RequiredTeamSize ?? 0) === teamSize)
        .map((dungeon) => ({
          id: dungeon?.dungeonId || dungeon?.DungeonId,
          dungeonId: dungeon?.dungeonId || dungeon?.DungeonId,
          name: dungeon?.name || dungeon?.Name,
          description: dungeon?.description || dungeon?.Description || '',
          recommendRealm: `组队副本 · ${teamSize}人`,
          minRealm: getRealmInfo(Number(dungeon?.recommendedLevel ?? dungeon?.RecommendedLevel ?? 1) || 1).realm,
          minBody: '未配置',
          entryMethod: '组队副本，个人奖励次数独立结算',
          difficulty: 'dungeon',
          difficultyText: '副本',
          canChallenge: Boolean(dungeon?.isAvailable ?? dungeon?.IsAvailable),
          dailyLimit: Number(dungeon?.dailyLimit ?? dungeon?.DailyLimit ?? 0) || 0,
          todayCount: Number(dungeon?.todayCount ?? dungeon?.TodayCount ?? 0) || 0,
          remainingCount: Number(dungeon?.remainingCount ?? dungeon?.RemainingCount ?? 0) || 0,
          unavailableReason: dungeon?.unavailableReason || dungeon?.UnavailableReason || '',
          enemyTypes: '副本地图链',
          enemyLevelMin: dungeon?.recommendedLevel ?? dungeon?.RecommendedLevel ?? 1,
          enemyLevelMax: dungeon?.recommendedLevel ?? dungeon?.RecommendedLevel ?? 1,
          monsterCountText: '以后端副本链为准',
          drops: []
        }))
    })
    const selectedMap = ref(null)
    const mapFilter = ref('available')
    const mapFilters = [
      { key: 'all', label: '全部' },
      { key: 'available', label: '可挑战' },
      { key: 'locked', label: '未开放' }
    ]
    const isPartySelection = computed(() => Boolean(gameStore.state.currentParty))
    const partyTeamSize = computed(() => {
      const party = gameStore.state.currentParty
      return Number(party?.maxMembers ?? party?.MaxMembers ?? 0) || 0
    })
    const filteredMaps = computed(() => {
      if (mapFilter.value === 'available') {
        return maps.value.filter((map) => map.canChallenge)
      }
      if (mapFilter.value === 'locked') {
        return maps.value.filter((map) => !map.canChallenge)
      }
      return maps.value
    })

    // 弹窗打开时懒加载地图列表，并默认选中第一张可挑战地图。
    watch(() => props.modelValue, async (visible) => {
      if (!visible) {
        return
      }

      // 每次打开默认展示可挑战地图，避免玩家进入弹窗后先看到大量未开放内容。
      mapFilter.value = 'available'

      if (!gameStore.state.currentParty) {
        await gameStore.loadMaps(true)
      }
      await gameStore.loadDungeons(true)

      // 每次打开都默认选中第一张可挑战地图，确保详情区与默认筛选保持一致。
      selectedMap.value = maps.value.find((map) => map.canChallenge) || maps.value[0] || null
    }, { immediate: true })

    // 地图列表刷新后，尽量保留上一次选中的地图；
    // 如果那张地图已不存在，就回退到第一张可挑战地图。
    watch(maps, (value) => {
      if (!value.length) {
        selectedMap.value = null
        return
      }

      if (!selectedMap.value) {
        selectedMap.value = value.find((map) => map.canChallenge) || value[0]
        return
      }

      const latestSelectedMap = value.find((map) => map.id === selectedMap.value?.id)
      selectedMap.value = latestSelectedMap || value.find((map) => map.canChallenge) || value[0]
    }, { immediate: true })

    // 只切换当前选中地图，不在这里直接进入战斗。
    const selectMap = (map) => {
      selectedMap.value = map
    }

    const meetRealmReq = computed(() => {
      if (!selectedMap.value) return false
      return Boolean(selectedMap.value.canChallenge)
    })

    const meetBodyReq = computed(() => {
      if (!selectedMap.value) return false
      return Boolean(selectedMap.value.canChallenge)
    })

    // 把当前选中的地图回传给主页面，由主页面统一控制“开始战斗”按钮。
    const enterMap = () => {
      if (!selectedMap.value || !selectedMap.value.canChallenge) return
      emit('enter-map', selectedMap.value)
      emit('update:modelValue', false)
    }

    return {
      ICON,
      maps,
      filteredMaps,
      mapFilter,
      mapFilters,
      isPartySelection,
      partyTeamSize,
      selectedMap,
      selectMap,
      meetRealmReq,
      meetBodyReq,
      enterMap
    }
  }
}
</script>
<style scoped>
.map-select-modal {
  display: grid;
  grid-template-columns: minmax(420px, 0.95fr) minmax(440px, 1.2fr);
  gap: var(--spacing-lg);
  min-height: 450px;
}

:deep(.modal-container) {
  width: min(1080px, calc(100vw - 32px)) !important;
}

.map-browser {
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
}

.map-browser-header {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: var(--spacing-md);
  padding-bottom: var(--spacing-sm);
  border-bottom: 1px solid var(--border-color);
}

.map-browser-title {
  color: var(--text-primary);
  font-size: var(--font-size-base);
  font-weight: 600;
}

.map-browser-summary {
  margin-top: 3px;
  color: var(--text-muted);
  font-size: var(--font-size-sm);
}

.map-filter-tabs {
  display: flex;
  flex-wrap: wrap;
  justify-content: flex-end;
  gap: 4px;
}

.map-filter-tab {
  padding: 4px 8px;
  border: 1px solid var(--control-border);
  border-radius: var(--radius-sm);
  background: var(--control-bg);
  color: var(--text-secondary);
  font-size: var(--font-size-sm);
  cursor: pointer;
}

.map-filter-tab:hover,
.map-filter-tab.active {
  border-color: var(--primary-color);
  background: oklch(from var(--primary-color) l c h / 0.1);
  color: var(--accent-text);
}

.map-list {
  min-width: 0;
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  align-content: start;
  gap: var(--spacing-sm);
  overflow-y: auto;
  max-height: 450px;
  padding: 2px var(--spacing-sm) 2px 1px;
}

.map-card {
  min-width: 0;
  min-height: 68px;
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  padding: var(--spacing-sm) var(--spacing-md);
  background: var(--xiuxian-bg-secondary);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  color: inherit;
  text-align: left;
  cursor: pointer;
  transition: border-color 0.15s ease, background 0.15s ease;
  position: relative;
}

.map-card:hover {
  border-color: oklch(from var(--primary-color) l c h / 0.55);
  background: oklch(from var(--primary-color) l c h / 0.05);
}

.map-card.selected {
  border-color: var(--primary-color);
  background: oklch(from var(--primary-color) l c h / 0.1);
  box-shadow: 0 0 12px oklch(from var(--primary-color) l c h / 0.1);
}

.map-card.locked {
  border-style: dashed;
}

.map-thumbnail {
  flex: 0 0 38px;
  width: 38px;
  height: 38px;
  background: var(--xiuxian-bg-primary);
  border-radius: var(--radius-sm);
  display: flex;
  align-items: center;
  justify-content: center;
}

.map-info {
  min-width: 0;
  flex: 1;
}

.map-name {
  min-width: 0;
  overflow: hidden;
  color: var(--text-primary);
  font-size: 15px;
  font-weight: 600;
  line-height: 1.35;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.map-recommend {
  overflow: hidden;
  margin-top: 3px;
  color: var(--accent-text);
  font-size: var(--font-size-sm);
  text-overflow: ellipsis;
  white-space: nowrap;
}

.map-status {
  flex: 0 0 auto;
  padding: 2px 5px;
  border-radius: var(--radius-sm);
  font-size: 11px;
  white-space: nowrap;
}

.map-status.available {
  background: var(--status-success-bg);
  color: var(--status-success-text);
}

.map-status.locked {
  background: var(--status-danger-bg);
  color: var(--status-danger-text);
}

.empty-map-list,
.empty-detail {
  padding: var(--spacing-lg) var(--spacing-sm);
  color: var(--text-muted);
  font-size: var(--font-size-sm);
  text-align: center;
}

.map-details {
  min-width: 0;
  overflow-y: auto;
  max-height: 450px;
  background: var(--xiuxian-bg-secondary);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  padding: var(--spacing-lg);
}

.details-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: var(--spacing-md);
  margin-bottom: var(--spacing-md);
  padding-bottom: var(--spacing-md);
  border-bottom: 1px solid var(--border-color);
}

.details-eyebrow {
  margin-bottom: 4px;
  color: var(--text-muted);
  font-size: var(--font-size-sm);
}

.details-name {
  margin: 0;
  color: var(--highlight-text);
  font-size: 18px;
  font-weight: 600;
}

.details-difficulty {
  flex: 0 0 auto;
  padding: 2px 10px;
  border-radius: var(--radius-sm);
  font-size: var(--font-size-sm);
}

.details-difficulty.easy { background: var(--status-success-bg); color: var(--status-success-text); }
.details-difficulty.normal { background: var(--status-warning-bg); color: var(--status-warning-text); }
.details-difficulty.hard { background: var(--status-danger-bg); color: var(--status-danger-text); }
.details-difficulty.dungeon { background: var(--status-warning-bg); color: var(--status-warning-text); }

.details-desc {
  margin: 0 0 var(--spacing-lg);
  color: var(--text-secondary);
  font-size: var(--font-size-base);
  line-height: 1.6;
}

.details-section {
  margin-bottom: var(--spacing-md);
}

.section-title {
  margin-bottom: var(--spacing-sm);
  color: var(--accent-text);
  font-size: var(--font-size-base);
  font-weight: 600;
}

.restrictions,
.enemy-info {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-xs);
}

.restriction-item,
.enemy-item {
  display: flex;
  justify-content: space-between;
  gap: var(--spacing-md);
  font-size: var(--font-size-base);
}

.restriction-item span:first-child,
.enemy-item span:first-child {
  flex: 0 0 auto;
  color: var(--text-muted);
}

.restriction-item span:last-child,
.enemy-item span:last-child {
  min-width: 0;
  color: var(--text-primary);
  text-align: right;
}

.restriction-item .unmet {
  color: var(--hp-color);
}

.drop-list {
  display: flex;
  flex-wrap: wrap;
  gap: var(--spacing-sm);
}

.drop-item {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 4px 8px;
  background: var(--xiuxian-bg-primary);
  border-radius: var(--radius-sm);
  font-size: var(--font-size-base);
}

.drop-icon {
  font-size: 14px;
}

.drop-name.common { color: var(--quality-common); }
.drop-name.uncommon { color: var(--quality-uncommon); }
.drop-name.rare { color: var(--quality-rare); }
.drop-name.epic { color: var(--quality-epic); }
.drop-name.legendary { color: var(--quality-legendary); }
.drop-name.mythic { color: var(--quality-mythic); }

.map-warning {
  margin-top: var(--spacing-md);
  padding: var(--spacing-sm) var(--spacing-md);
  background: var(--status-danger-bg);
  border: 1px solid var(--hp-color);
  border-radius: var(--radius-sm);
  color: var(--status-danger-text);
  line-height: 1.6;
}

.details-actions {
  display: flex;
  justify-content: flex-end;
  margin-top: var(--spacing-lg);
}

.enter-btn {
  padding: var(--spacing-sm) var(--spacing-xl);
  background: var(--button-primary-start);
  border: none;
  border-radius: var(--radius-sm);
  color: var(--button-primary-text);
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.15s ease;
}

.enter-btn:hover:not(:disabled) {
  background: var(--button-primary-start);
  opacity: 0.85;
  color: var(--button-primary-hover-text);
  box-shadow: var(--shadow-sm);
}

.enter-btn:disabled {
  background: var(--xiuxian-bg-primary);
  color: var(--text-muted);
  cursor: not-allowed;
}

.no-selection {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: var(--text-muted);
}

.hint-icon {
  font-size: 48px;
  margin-bottom: var(--spacing-md);
  opacity: 0.5;
}

@media (max-width: 760px) {
  .map-select-modal {
    grid-template-columns: 1fr;
  }

  .map-list,
  .map-details {
    max-height: 300px;
  }
}

@media (max-width: 460px) {
  .map-list {
    grid-template-columns: 1fr;
  }

  .map-browser-header {
    align-items: flex-start;
    flex-direction: column;
  }

  .map-filter-tabs {
    justify-content: flex-start;
  }
}
</style>
