<template>
  <!-- 右侧面板 -->
  <aside class="right-panel">
    <!-- 修炼状态卡片 -->
    <div class="panel-card">
      <div class="card-header">
        <span class="card-title">⏱️ 修炼状态</span>
      </div>

      <div class="card-body">
        <div class="cultivation-info">
          <div class="info-row">
            <span class="label">当前效率</span>
            <span class="value highlight">+{{ expPerSecond }}/秒</span>
          </div>

          <div class="info-row">
            <span class="label">预计突破</span>
            <span class="value">{{ timeToBreakthrough }}</span>
          </div>

          <div class="info-row">
            <span class="label">挂机时长</span>
            <span class="value">{{ afkTime }}</span>
          </div>
        </div>
      </div>
    </div>

    <!-- 快捷功能按钮 -->
    <div class="panel-card">
      <div class="card-header">
        <span class="card-title"><AssetIcon :source="ICON.ui_target" size="25" /> 快捷功能</span>
      </div>

      <div class="card-body">
        <div class="quick-actions">
          <button v-for="action in quickActions" :key="action.key" class="action-btn" @click="$emit('open-modal', action.key)">
            <span class="action-icon"><AssetIcon :source="action.icon" size="25" /></span>
            <span class="action-label">{{ action.label }}</span>
          </button>
        </div>
      </div>
    </div>

    <!-- 在线玩家 -->
    <div class="panel-card">
      <div class="card-header">
        <span class="card-title"><AssetIcon :source="onlineIcon" size="25" /> 在线道友</span>
        <span class="online-count">{{ onlinePlayers.length }}人</span>
      </div>

      <div class="card-body">
        <div class="player-list">
          <div v-for="player in onlinePlayers" :key="player.id" class="player-item">
            <span class="status-dot"></span>
            <span class="player-name">{{ player.name }}</span>
            <span class="player-realm">{{ player.realm }}</span>
          </div>
        </div>
      </div>
    </div>
  </aside>
</template>

<script>
import { ref, reactive } from 'vue'
import { ICON } from '../../icons'
import AssetIcon from '../common/AssetIcon.vue'

/**
 * RightPanel - 右侧面板
 *
 * 显示内容：
 * - 修炼状态（效率、突破时间、挂机时长）
 * - 快捷功能按钮（签到、排行榜、兑换码等）
 * - 在线玩家列表
 */
export default {
  name: 'RightPanel',

  components: { AssetIcon },

  emits: ['open-modal'],

  setup() {
    // 修炼数据（模拟）
    const expPerSecond = ref(10)
    const timeToBreakthrough = ref('2小时35分')
    const afkTime = ref('12小时18分')

    // 快捷功能
    const quickActions = [
      { key: 'checkIn', label: '每日签到', icon: ICON.nav_checkin },
      { key: 'quest', label: '任务簿', icon: ICON.nav_quest },
      { key: 'achievement', label: '成就谱', icon: ICON.nav_achievement },
      { key: 'guild', label: '宗门', icon: ICON.nav_sect },
      { key: 'spiritField', label: '灵田', icon: ICON.nav_spirit_field },
      { key: 'rankings', label: '排行榜', icon: ICON.nav_ranking },
      { key: 'team', label: '组队', icon: ICON.nav_team },
      { key: 'redeem', label: '兑换码', icon: ICON.nav_redeem },
      { key: 'pet', label: '宠物', icon: ICON.nav_pet },
      { key: 'shop', label: '商店', icon: ICON.nav_shop },
      { key: 'forge', label: '锻造', icon: ICON.nav_forge },
      { key: 'alchemy', label: '炼丹', icon: ICON.nav_alchemy },
      { key: 'changelog', label: '更新日志', icon: ICON.nav_changelog }
    ]

    // 在线玩家标题图标
    const onlineIcon = ICON.misc_group

    // 在线玩家（模拟数据）
    const onlinePlayers = reactive([
      { id: 1, name: '剑舞长空', realm: '金丹期' },
      { id: 2, name: '月下独酌', realm: '筑基期' },
      { id: 3, name: '九转还魂', realm: '元婴期' },
      { id: 4, name: '青云直上', realm: '筑基期' },
      { id: 5, name: '一念永恒', realm: '金丹期' }
    ])

    return {
      ICON,
      expPerSecond,
      timeToBreakthrough,
      afkTime,
      quickActions,
      onlinePlayers,
      onlineIcon
    }
  }
}
</script>

<style scoped>
.right-panel {
  width: 250px;
  background: var(--xiuxian-bg-secondary);
  border-left: 1px solid var(--border-color);
  padding: var(--spacing-md);
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
  overflow-y: auto;
}

/* 卡片样式 */
.panel-card {
  background: var(--xiuxian-bg-panel);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  overflow: hidden;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--spacing-sm) var(--spacing-md);
  background: var(--accent-soft-bg);
  border-bottom: 1px solid var(--border-color);
}

.card-title {
  font-size: var(--font-size-base);
  font-weight: 500;
  color: var(--text-primary);
}

.online-count {
  font-size: var(--font-size-sm);
  color: var(--text-muted);
}

.card-body {
  padding: var(--spacing-md);
}

/* 修炼信息 */
.cultivation-info {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
}

.info-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: var(--font-size-base);
}

.info-row .label {
  color: var(--text-secondary);
}

.info-row .value {
  color: var(--text-primary);
  font-family: var(--font-mono);
}

.info-row .value.highlight {
  color: var(--highlight-text);
  font-weight: 600;
}

/* 快捷功能 */
.quick-actions {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: var(--spacing-sm);
}

.action-btn {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
  padding: var(--spacing-sm);
  background: var(--xiuxian-bg-secondary);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  cursor: pointer;
  transition: all 0.15s ease;
}

.action-btn:hover {
  border-color: var(--border-color);
  background: var(--control-bg-hover);
}

.action-icon {
  font-size: 18px;
  width: 22px;
  height: 22px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.action-label {
  font-size: var(--font-size-xs);
  color: var(--text-secondary);
}

/* 玩家列表 */
.player-list {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-xs);
}

.player-item {
  display: flex;
  align-items: center;
  gap: var(--spacing-xs);
  padding: var(--spacing-xs) var(--spacing-sm);
  border-radius: var(--radius-md);
  font-size: var(--font-size-base);
  transition: background 0.15s ease;
}

.player-item:hover {
  background: var(--control-bg-hover);
}

.status-dot {
  width: 6px;
  height: 6px;
  background: #4caf50;
  border-radius: 50%;
  flex-shrink: 0;
}

.player-name {
  color: var(--text-primary);
  flex: 1;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.player-realm {
  font-size: var(--font-size-xs);
  color: var(--text-secondary);
  background: var(--accent-soft-bg);
  padding: 1px 6px;
  border-radius: var(--radius-md);
}
</style>
