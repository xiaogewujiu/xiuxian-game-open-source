<template>
  <aside class="app-sidebar">
    <!-- Brand / Header -->
    <div class="sidebar-brand">
      <div class="brand-logo-wrapper">
        <AssetIcon :source="ICON.misc_yinyang" size="18" class="brand-logo" />
      </div>
      <div class="brand-info">
        <span class="brand-title">修仙问道</span>
        <span class="brand-subtitle">Idle RPG / 文字修仙</span>
      </div>
      <button class="sidebar-close-toggle" title="收起导航栏" @click="$emit('toggle-sidebar')">
        <span>❮</span>
      </button>
    </div>

    <!-- Navigation List -->
    <div class="sidebar-scrollable">
      <div v-for="group in menuGroups" :key="group.key" class="menu-group">
        <div class="group-header" @click="toggleGroup(group.key)">
          <span class="group-title">{{ group.label }}</span>
          <span class="group-arrow" :class="{ collapsed: collapsedGroups[group.key] }">▼</span>
        </div>
        <div v-show="!collapsedGroups[group.key]" class="group-items">
          <button
            v-for="item in group.items"
            :key="item.key"
            class="menu-item"
            :class="{ active: activeModals[item.key] }"
            @click="$emit('open-modal', item.key)"
          >
            <!-- Active indicator line -->
            <span class="active-indicator"></span>
            <span class="item-icon"><AssetIcon :source="item.icon" size="20" /></span>
            <span class="item-label">{{ item.label }}</span>
          </button>
        </div>
      </div>
    </div>

    <!-- Footer / Status -->
    <div class="sidebar-footer">
      <div class="system-status">
        <span class="status-indicator" :class="connectionState"></span>
        <span class="status-text">{{ connectionText }}</span>
        <span v-if="onlineCount > 0" class="online-count">· {{ onlineCount }}人</span>
      </div>
      <div class="server-time">{{ serverTime }}</div>
    </div>
  </aside>
</template>

<script>
import { ref, reactive, onMounted, onUnmounted } from 'vue'
import { ICON } from '../../icons'
import AssetIcon from '../common/AssetIcon.vue'

/**
 * AppSidebar - 极简现代 SaaS 风格的左侧快捷导航边栏（暗色主题）
 */
export default {
  name: 'AppSidebar',

  components: {
    AssetIcon
  },

  props: {
    connectionState: { type: String, default: 'offline' },
    connectionText: { type: String, default: '未连接' },
    onlineCount: { type: Number, default: 0 },
    activeModals: { type: Object, default: () => ({}) }
  },

  emits: ['open-modal'],

  setup(props) {
    // 1-3 组默认展开，4-5 组默认折叠
    const collapsedGroups = reactive({
      character: false,
      cultivation: false,
      adventure: false,
      social: true,
      system: true
    })

    const toggleGroup = (key) => {
      collapsedGroups[key] = !collapsedGroups[key]
    }

    const menuGroups = [
      {
        key: 'character',
        label: '角色',
        items: [
          { key: 'attributes', label: '人物属性', icon: ICON.misc_wizard },
          { key: 'inventory', label: '背包', icon: ICON.nav_backpack },
          { key: 'skill', label: '技能', icon: ICON.nav_skill },
          { key: 'title', label: '称号', icon: ICON.ui_title },
          { key: 'pet', label: '灵宠', icon: ICON.nav_pet }
        ]
      },
      {
        key: 'cultivation',
        label: '修炼',
        items: [
          { key: 'spiritField', label: '灵田', icon: ICON.nav_spirit_field },
          { key: 'forge', label: '锻造', icon: ICON.nav_forge },
          { key: 'alchemy', label: '炼丹', icon: ICON.nav_alchemy }
        ]
      },
      {
        key: 'adventure',
        label: '历练',
        items: [
          { key: 'dungeonInstance', label: '秘境', icon: ICON.nav_map },
          { key: 'worldBoss', label: 'Boss', icon: ICON.nav_boss },
          { key: 'arena', label: '竞技场', icon: ICON.misc_sword_crossed },
          { key: 'tower', label: '通天塔', icon: ICON.ui_pagoda },
          { key: 'team', label: '组队', icon: ICON.nav_team }
        ]
      },
      {
        key: 'social',
        label: '社交与交易',
        items: [
          { key: 'sect', label: '宗门', icon: ICON.nav_sect },
          { key: 'market', label: '寄售行', icon: ICON.ui_market },
          { key: 'shop', label: '商店', icon: ICON.nav_shop },
          { key: 'favorability', label: '好感度', icon: ICON.ui_favorability },
          { key: 'feedback', label: '建议反馈', icon: ICON.ui_chat },
          { key: 'mail', label: '邮件', icon: ICON.ui_mail }
        ]
      },
      {
        key: 'system',
        label: '福利与系统',
        items: [
          { key: 'checkIn', label: '签到', icon: ICON.nav_checkin },
          { key: 'quest', label: '任务', icon: ICON.nav_quest },
          { key: 'achievement', label: '成就', icon: ICON.nav_achievement },
          { key: 'rankings', label: '排行', icon: ICON.nav_ranking },
          { key: 'lottery', label: '抽奖', icon: ICON.nav_lottery },
          { key: 'redeem', label: '兑换', icon: ICON.nav_redeem },
          { key: 'settings', label: '设置', icon: ICON.ui_gear },
          { key: 'theme', label: '切换主题', icon: ICON.ui_theme_switch },
          { key: 'changelog', label: '更新日志', icon: ICON.nav_changelog },
          { key: 'help', label: '游戏帮助', icon: ICON.misc_book_open },
          { key: 'logout', label: '退出登录', icon: ICON.ui_door }
        ]
      }
    ]

    const serverTime = ref('')
    let timeInterval = null

    const updateTime = () => {
      const now = new Date()
      serverTime.value = now.toLocaleTimeString('zh-CN', {
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit'
      })
    }

    onMounted(() => {
      updateTime()
      timeInterval = setInterval(updateTime, 1000)
    })

    onUnmounted(() => {
      clearInterval(timeInterval)
    })

    return {
      ICON,
      collapsedGroups,
      toggleGroup,
      menuGroups,
      serverTime
    }
  }
}
</script>

<style scoped>
.app-sidebar {
  position: absolute;
  left: 0;
  top: 0;
  bottom: 0;
  z-index: 100;
  width: 248px;
  background: var(--xiuxian-bg-secondary);
  border-right: 1px solid var(--border-color);
  display: flex;
  flex-direction: column;
  flex-shrink: 0;
  height: 100%;
  overflow: hidden;
  transition: width 0.18s cubic-bezier(0.4, 0, 0.2, 1), background 0.18s ease, border-right-color 0.18s ease;
  pointer-events: auto;
}

.app-sidebar.collapsed {
  width: 0;
  border-right-color: transparent;
  pointer-events: none;
}

/* 顶部品牌区 */
.sidebar-brand {
  height: 64px;
  padding: 16px;
  display: flex;
  align-items: center;
  gap: 12px;
  border-bottom: 1px solid var(--border-color);
  flex-shrink: 0;
}

.brand-logo-wrapper {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background: var(--bg-overlay-light);
  display: flex;
  align-items: center;
  justify-content: center;
  border: 1px solid var(--border-color);
  transition: all 0.15s ease;
  color: var(--primary-color);
}

.brand-logo-wrapper:hover {
  background: var(--bg-overlay-medium);
  border-color: var(--primary-color);
  box-shadow: 0 0 8px var(--primary-color);
}

.brand-info {
  display: flex;
  flex-direction: column;
  line-height: 1.2;
}

.brand-title {
  font-size: 15px;
  font-weight: 600;
  color: var(--text-primary);
  letter-spacing: 0.5px;
}

.brand-subtitle {
  font-size: 11px;
  color: var(--text-muted);
  margin-top: 2px;
}

.sidebar-close-toggle {
  margin-left: auto;
  width: 24px;
  height: 24px;
  border-radius: 6px;
  background: var(--bg-overlay-light);
  border: 1px solid var(--border-color);
  color: var(--text-muted);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  transition: all 0.15s ease;
}

.sidebar-close-toggle:hover {
  background: var(--control-bg-hover);
  color: var(--text-primary);
  border-color: var(--text-secondary);
}

/* 滚动导航区域 */
.sidebar-scrollable {
  flex: 1;
  overflow-y: auto;
  padding: 16px 8px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

/* 极细滚动条 */
.sidebar-scrollable::-webkit-scrollbar {
  width: 4px;
}
.sidebar-scrollable::-webkit-scrollbar-track {
  background: transparent;
}
.sidebar-scrollable::-webkit-scrollbar-thumb {
  background: var(--scrollbar-thumb);
  border-radius: 2px;
}
.sidebar-scrollable::-webkit-scrollbar-thumb:hover {
  background: var(--scrollbar-thumb-hover);
}

/* 菜单分组 */
.menu-group {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.group-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 4px 8px;
  cursor: pointer;
  user-select: none;
  border-radius: 4px;
  transition: background 0.12s ease;
}

.group-header:hover {
  background: var(--bg-overlay-light);
}

.group-title {
  font-size: 11px;
  font-weight: 600;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

.group-arrow {
  font-size: 8px;
  color: var(--text-muted);
  transition: transform 0.15s ease;
}

.group-arrow.collapsed {
  transform: rotate(-90deg);
}

.group-items {
  display: flex;
  flex-direction: column;
  gap: 2px;
  margin-top: 2px;
}

/* 菜单选项 */
.menu-item {
  position: relative;
  display: flex;
  align-items: center;
  gap: 10px;
  height: 38px;
  padding: 0 12px 0 16px;
  background: transparent;
  border: none;
  border-radius: 8px;
  color: var(--text-secondary);
  cursor: pointer;
  transition: all 0.15s cubic-bezier(0.4, 0, 0.2, 1);
  text-align: left;
  width: 100%;
}

.menu-item:hover {
  background: var(--control-bg-hover);
  color: var(--text-primary);
  transform: translateX(2px);
}

.menu-item.active {
  background: var(--accent-soft-bg);
  color: var(--text-primary);
  font-weight: 600;
}

/* Active Left Indicator */
.active-indicator {
  position: absolute;
  left: 0;
  top: 10px;
  bottom: 10px;
  width: 3px;
  background: var(--primary-color);
  border-radius: 99px;
  opacity: 0;
  transition: opacity 0.15s ease;
}

.menu-item.active .active-indicator {
  opacity: 1;
}

.item-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 22px;
  height: 22px;
  color: inherit;
}

.item-label {
  font-size: 14px;
  font-weight: 500;
}

/* 底部状态区域 */
.sidebar-footer {
  height: 52px;
  padding: 12px 16px;
  border-top: 1px solid var(--border-color);
  background: var(--bg-overlay-light);
  display: flex;
  flex-direction: column;
  justify-content: center;
  gap: 2px;
  flex-shrink: 0;
}

.system-status {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 12px;
  color: var(--text-secondary);
}

.status-indicator {
  width: 6px;
  height: 6px;
  border-radius: 50%;
}

.status-indicator.connected {
  background-color: #22c55e;
}

.status-indicator.reconnecting {
  background-color: #eab308;
}

.status-indicator.offline {
  background-color: var(--text-muted);
}

.status-text {
  font-weight: 500;
}

.online-count {
  color: var(--text-muted);
}

.server-time {
  font-size: 12px;
  font-family: var(--font-mono);
  color: var(--text-muted);
  line-height: 1;
}
</style>
