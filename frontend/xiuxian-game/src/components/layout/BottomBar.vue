<template>
  <!-- 底部快捷栏 -->
  <footer class="bottom-bar">
    <!-- 视图切换按钮 -->
    <div class="view-tabs">
      <button
        v-for="tab in filteredTabs"
        :key="tab.key"
        class="tab-btn"
        :class="{ active: currentView === tab.key }"
        @click="switchView(tab)"
      >
        <span class="tab-icon"><AssetIcon :source="tab.icon" size="25" /></span>
        <span class="tab-label">{{ tab.label }}</span>
      </button>
    </div>

    <!-- 系统信息 -->
    <div class="system-info">
      <span class="server-time">{{ serverTime }}</span>
      <span class="separator">|</span>
      <span class="connection-status" :class="connectionState">{{ connectionText }}</span>
      <span v-if="onlineCount > 0" class="separator">|</span>
      <span v-if="onlineCount > 0" class="online-count">在线 {{ onlineCount }}</span>
    </div>
  </footer>
</template>

<script>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { ICON } from '../../icons'
import AssetIcon from '../common/AssetIcon.vue'

/**
 * BottomBar - 底部快捷栏
 *
 * 功能：
 * - 视图切换选项卡
 * - 系统信息（时间、服务器、延迟）
 */
export default {
  name: 'BottomBar',

  components: { AssetIcon },

  props: {
    connectionState: { type: String, default: 'offline' },
    connectionText: { type: String, default: '未连接' },
    onlineCount: { type: Number, default: 0 },
    activeCategory: { type: String, default: 'cultivation' }
  },

  emits: ['change-view', 'open-modal'],

  setup(props, { emit }) {
    // 当前高亮的快捷入口键。
    const currentView = ref('cultivation')

    // 底部快捷栏只是”发出打开哪个弹窗/切到哪个视图”的意图，
    // 真正的弹窗开关统一由 MainView 集中维护。
    const viewTabs = [
      { key: 'checkIn', label: '签到', icon: ICON.nav_checkin, action: () => emit('open-modal', 'checkIn') },
      { key: 'quest', label: '任务', icon: ICON.nav_quest, action: () => emit('open-modal', 'quest') },
      { key: 'achievement', label: '成就', icon: ICON.nav_achievement, action: () => emit('open-modal', 'achievement') },
      { key: 'sect', label: '宗门', icon: ICON.nav_sect, action: () => emit('open-modal', 'sect') },
      { key: 'spiritField', label: '灵田', icon: ICON.nav_spirit_field, action: () => emit('open-modal', 'spiritField') },
      { key: 'rankings', label: '排行', icon: ICON.nav_ranking, action: () => emit('open-modal', 'rankings') },
      { key: 'worldBoss', label: 'Boss', icon: ICON.nav_boss, action: () => emit('open-modal', 'worldBoss') },
      { key: 'dungeonInstance', label: '秘境', icon: ICON.nav_map, action: () => emit('open-modal', 'dungeonInstance') },
      { key: 'team', label: '组队', icon: ICON.nav_team, action: () => emit('open-modal', 'team') },
      { key: 'inventory', label: '背包', icon: ICON.nav_backpack, action: () => emit('open-modal', 'inventory') },
      { key: 'skill', label: '技能', icon: ICON.nav_skill, action: () => emit('open-modal', 'skill') },
      { key: 'shop', label: '商店', icon: ICON.nav_shop, action: () => emit('open-modal', 'shop') },
      { key: 'forge', label: '锻造', icon: ICON.nav_forge, action: () => emit('open-modal', 'forge') },
      { key: 'alchemy', label: '炼丹', icon: ICON.nav_alchemy, action: () => emit('open-modal', 'alchemy') },
      { key: 'pet', label: '灵宠', icon: ICON.nav_pet, action: () => emit('open-modal', 'pet') },
      { key: 'redeem', label: '兑换', icon: ICON.nav_redeem, action: () => emit('open-modal', 'redeem') },
      { key: 'mail', label: '邮件', icon: ICON.ui_mail, action: () => emit('open-modal', 'mail') },
      { key: 'title', label: '称号', icon: ICON.ui_title, action: () => emit('open-modal', 'title') },
      { key: 'arena', label: '竞技场', icon: ICON.misc_sword_crossed, action: () => emit('open-modal', 'arena') },
      { key: 'tower', label: '通天塔', icon: ICON.ui_pagoda, action: () => emit('open-modal', 'tower') },
      { key: 'market', label: '寄售行', icon: ICON.ui_market, action: () => emit('open-modal', 'market') },
      { key: 'changelog', label: '更新', icon: ICON.nav_changelog, action: () => emit('open-modal', 'changelog') },
      { key: 'lottery', label: '抽奖', icon: ICON.nav_lottery, action: () => emit('open-modal', 'lottery') },
      { key: 'favorability', label: '好感度', icon: ICON.ui_favorability, action: () => emit('open-modal', 'favorability') }
    ]

    const categoryMap = {
      cultivation: ['spiritField', 'skill', 'title', 'forge', 'alchemy', 'pet', 'inventory'],
      adventure: ['dungeonInstance', 'worldBoss', 'arena', 'tower', 'team'],
      market: ['sect', 'market', 'shop', 'favorability'],
      welfare: ['checkIn', 'quest', 'achievement', 'rankings', 'lottery', 'redeem'],
      system: ['mail', 'changelog']
    }

    const filteredTabs = computed(() => {
      const allowedKeys = categoryMap[props.activeCategory] || []
      return viewTabs.filter(tab => allowedKeys.includes(tab.key))
    })

    // 切换快捷入口时，同时更新本地高亮状态，并把动作向上抛给主页面。
    const switchView = (tab) => {
      currentView.value = tab.key
      if (tab.action) {
        tab.action()
      }
      emit('change-view', tab.key)
    }

    // 底栏系统信息当前使用本地时间做展示占位。
    const serverTime = ref('')

    // 每秒刷新一次底栏时间。
    let timeInterval = null
    const updateTime = () => {
      const now = new Date()
      serverTime.value = now.toLocaleTimeString('zh-CN', {
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit'
      })
    }

    // 组件挂载后启动时间刷新。
    onMounted(() => {
      updateTime()
      timeInterval = setInterval(updateTime, 1000)
    })

    // 组件销毁时清掉计时器，避免页面切换后仍然持续刷新。
    onUnmounted(() => {
      clearInterval(timeInterval)
    })

    return {
      ICON,
      currentView,
      filteredTabs,
      switchView,
      serverTime
    }
  }
}
</script>

<style scoped>
.bottom-bar {
  min-height: 50px;
  background: var(--xiuxian-bg-secondary);
  border-top: 1px solid var(--border-color);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--spacing-sm) var(--spacing-lg);
  gap: var(--spacing-md);
}

/* 视图选项卡 */
.view-tabs {
  display: flex;
  gap: var(--spacing-xs);
  flex-wrap: wrap;
  justify-content: center;
}

.tab-btn {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 2px;
  padding: var(--spacing-xs) var(--spacing-sm);
  min-width: 44px;
  background: transparent;
  border: 1px solid transparent;
  border-radius: var(--radius-md);
  color: var(--text-secondary);
  cursor: pointer;
  transition: all 0.15s ease;
}

.tab-btn:hover {
  background: var(--control-bg-hover);
  color: var(--text-primary);
}

.tab-btn.active {
  background: var(--accent-soft-bg);
  border-color: var(--border-color);
  color: var(--text-primary);
}

.tab-icon {
  font-size: 18px;
  width: 22px;
  height: 22px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.tab-label {
  font-size: var(--font-size-xs);
}

/* 系统信息 */
.system-info {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  font-size: var(--font-size-sm);
  color: var(--text-muted);
  flex-shrink: 0;
  white-space: nowrap;
}

.separator {
  opacity: 0.3;
}

.connection-status.connected {
  color: #8be0a8;
}

.connection-status.reconnecting {
  color: #f1d58b;
}

.connection-status.offline {
  color: var(--text-muted);
}
</style>
