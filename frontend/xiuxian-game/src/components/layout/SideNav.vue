<template>
  <!-- 可折叠左侧导航栏 -->
  <nav class="side-nav" :class="{ expanded: isExpanded }" @mouseenter="isExpanded = true" @mouseleave="isExpanded = false">
    <!-- 顶部：头像 + 标题 -->
    <div class="nav-header">
      <div class="avatar-wrapper" role="button" tabindex="0" title="点击上传头像" @click="triggerAvatarUpload">
        <div class="avatar">
          <AssetIcon :source="player.avatarImagePath || player.avatar" :fallback="player.avatar" :alt="player.name" />
        </div>
        <span class="status-dot" :class="{ online: true }"></span>
      </div>
      <input ref="avatarInputRef" type="file" accept="image/*" hidden @change="handleAvatarChange" />
      <div class="header-info" v-show="isExpanded">
        <span class="player-name">{{ player.name }}</span>
        <span class="realm-tag" :class="player.realmTier">{{ player.realm }}</span>
      </div>
    </div>

    <!-- 分隔线 -->
    <div class="nav-divider"></div>

    <!-- 资源信息（展开时显示） -->
    <div class="resource-strip" v-show="isExpanded">
      <div v-for="res in resources" :key="res.key" class="resource-row">
        <span class="res-icon"><AssetIcon :source="res.icon" size="25" /></span>
        <span class="res-value">{{ formatNumber(res.value) }}</span>
      </div>
    </div>
    <div class="nav-divider" v-show="isExpanded"></div>

    <!-- 导航项 -->
    <div class="nav-items">
      <button
        v-for="tab in navTabs"
        :key="tab.key"
        class="nav-item"
        :class="{ active: activeTab === tab.key }"
        :title="tab.label"
        @click="handleNavClick(tab)"
      >
        <span class="nav-icon"><AssetIcon :source="tab.icon" size="25" /></span>
        <span class="nav-label" v-show="isExpanded">{{ tab.label }}</span>
        <span v-if="tab.key === 'mail' && unreadMailCount > 0" class="unread-badge" :title="`${unreadMailCount} 封未读`">{{ unreadMailCount > 99 ? '99+' : unreadMailCount }}</span>
      </button>
    </div>

    <!-- 底部：系统信息 + 操作 -->
    <div class="nav-footer">
      <div class="system-row" v-show="isExpanded">
        <span class="server-time">{{ serverTime }}</span>
        <span class="latency" :class="latencyClass">{{ latency }}ms</span>
      </div>
      <div class="nav-divider"></div>
      <button class="nav-item" :title="getThemeTitle()" @click="toggleTheme">
        <span class="nav-icon"><AssetIcon :source="getThemeIcon()" size="25" /></span>
        <span class="nav-label" v-show="isExpanded">{{ getThemeTitle() }}</span>
      </button>
      <button class="nav-item" title="设置" @click="showSettings">
        <span class="nav-icon"><AssetIcon :source="ICON.ui_gear" size="25" /></span>
        <span class="nav-label" v-show="isExpanded">设置</span>
      </button>
      <button class="nav-item" title="退出" @click="logout">
        <span class="nav-icon"><AssetIcon :source="ICON.ui_door" size="25" /></span>
        <span class="nav-label" v-show="isExpanded">退出</span>
      </button>
    </div>
  </nav>
</template>

<script>
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useGameStore } from '../../state/gameStore'
import { buildPlayerView, formatCompactNumber } from '../../services/gameDisplay'
import { apiClient } from '../../lib/apiClient'
import AssetIcon from '../common/AssetIcon.vue'
import { ICON } from '../../icons'
import toast from '@/utils/toast'

export default {
  name: 'SideNav',
  components: { AssetIcon },
  emits: ['open-modal', 'toggle-theme'],

  setup(props, { emit }) {
    const router = useRouter()
    const gameStore = useGameStore()
    const isExpanded = ref(false)
    const activeTab = ref('')
    const currentTheme = ref('light')
    const avatarInputRef = ref(null)
    const unreadMailCount = ref(0)

    const player = computed(() => buildPlayerView(gameStore.state.player))

    const resources = computed(() => [
      { key: 'spiritStone', name: '灵石', icon: ICON.currency_spirit, value: player.value.spiritStone },
      { key: 'gold', name: '金币', icon: ICON.currency_gold, value: player.value.gold },
      { key: 'exp', name: '经验', icon: ICON.currency_exp, value: player.value.realmExp },
      { key: 'honor', name: '荣誉', icon: ICON.currency_honor, value: player.value.honor }
    ])

    const navTabs = [
      { key: 'checkIn', label: '签到', icon: ICON.nav_checkin },
      { key: 'quest', label: '任务', icon: ICON.nav_quest },
      { key: 'achievement', label: '成就', icon: ICON.nav_achievement },
      { key: 'sect', label: '宗门', icon: ICON.nav_sect },
      { key: 'spiritField', label: '灵田', icon: ICON.nav_spirit_field },
      { key: 'rankings', label: '排行', icon: ICON.nav_ranking },
      { key: 'worldBoss', label: 'Boss', icon: ICON.nav_boss },
      { key: 'team', label: '组队', icon: ICON.nav_team },
      { key: 'inventory', label: '背包', icon: ICON.nav_backpack },
      { key: 'skill', label: '技能', icon: ICON.nav_skill },
      { key: 'shop', label: '商店', icon: ICON.nav_shop },
      { key: 'forge', label: '锻造', icon: ICON.nav_forge },
      { key: 'alchemy', label: '炼丹', icon: ICON.nav_alchemy },
      { key: 'pet', label: '灵宠', icon: ICON.nav_pet },
      { key: 'redeem', label: '兑换', icon: ICON.nav_redeem },
      { key: 'mail', label: '邮件', icon: ICON.nav_changelog },
      { key: 'title', label: '称号', icon: ICON.nav_changelog },
      { key: 'arena', label: '竞技场', icon: ICON.nav_changelog },
      { key: 'tower', label: '通天塔', icon: ICON.nav_changelog },
      { key: 'market', label: '寄售行', icon: ICON.nav_shop },
      { key: 'changelog', label: '更新', icon: ICON.nav_changelog }
    ]

    const handleNavClick = (tab) => {
      activeTab.value = tab.key
      emit('open-modal', tab.key)
    }

    // 主题
    onMounted(() => {
      const savedTheme = localStorage.getItem('xiuxian-theme')
      if (savedTheme && ['dark', 'light', 'elegant'].includes(savedTheme)) {
        currentTheme.value = savedTheme
      }
      // 获取未读邮件数
      apiClient.getMails(1, 1).then(result => {
        unreadMailCount.value = result.unreadCount || 0
      }).catch(() => {})
    })

    const toggleTheme = () => {
      const themes = ['dark', 'light', 'elegant']
      const idx = themes.indexOf(currentTheme.value)
      currentTheme.value = themes[(idx + 1) % themes.length]
      document.documentElement.setAttribute('data-theme', currentTheme.value)
      localStorage.setItem('xiuxian-theme', currentTheme.value)
      emit('toggle-theme', currentTheme.value)
    }

    const getThemeIcon = () => {
      return { dark: ICON.ui_moon, light: ICON.ui_sun, elegant: ICON.ui_candle }[currentTheme.value] || ICON.ui_moon
    }

    const getThemeTitle = () => {
      return { dark: '浅色主题', light: '雅致主题', elegant: '深色主题' }[currentTheme.value] || '主题'
    }

    // 系统信息
    const serverTime = ref('')
    const latency = ref(28)
    const latencyClass = computed(() => latency.value < 50 ? 'good' : latency.value < 100 ? 'normal' : 'bad')

    let timeInterval = null
    const updateTime = () => {
      serverTime.value = new Date().toLocaleTimeString('zh-CN', { hour: '2-digit', minute: '2-digit', second: '2-digit' })
    }
    onMounted(() => { updateTime(); timeInterval = setInterval(updateTime, 1000) })
    onUnmounted(() => clearInterval(timeInterval))

    // 头像上传
    const triggerAvatarUpload = () => avatarInputRef.value?.click()
    const handleAvatarChange = async (e) => {
      const file = e?.target?.files?.[0]
      e.target.value = ''
      if (!file) return
      try {
        await apiClient.uploadPlayerAvatar(file)
        await gameStore.refreshPlayerSnapshot({ includeInventory: false, includeEquipment: false, includeRankings: false })
      } catch (err) {
        toast.error(err.message || '头像上传失败。')
      }
    }

    const showSettings = () => toast.info('设置面板后续开放。')
    const logout = async () => { await gameStore.logout('已退出。'); router.replace('/') }

    return {
      ICON,
      isExpanded, activeTab, player, resources, navTabs, handleNavClick,
      unreadMailCount,
      currentTheme, toggleTheme, getThemeIcon, getThemeTitle,
      serverTime, latency, latencyClass,
      avatarInputRef, triggerAvatarUpload, handleAvatarChange,
      showSettings, logout, formatNumber: formatCompactNumber
    }
  }
}
</script>

<style scoped>
.side-nav {
  position: fixed;
  left: 0;
  top: 0;
  bottom: 0;
  width: 64px;
  background: var(--card);
  border-right: 1px solid var(--border);
  display: flex;
  flex-direction: column;
  z-index: 100;
  transition: width 0.3s ease-in-out;
  overflow: hidden;
}

.side-nav.expanded {
  width: 200px;
}

/* 顶部 */
.nav-header {
  padding: 16px 12px 8px;
  display: flex;
  align-items: center;
  gap: 12px;
  flex-shrink: 0;
}

.avatar-wrapper {
  position: relative;
  flex-shrink: 0;
  cursor: pointer;
}

.avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: var(--muted);
  border: 2px solid var(--border);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 22px;
  overflow: hidden;
  transition: border-color 0.15s;
}

.avatar-wrapper:hover .avatar {
  border-color: var(--foreground);
}

.status-dot {
  position: absolute;
  bottom: 0;
  right: 0;
  width: 10px;
  height: 10px;
  border-radius: 50%;
  background: var(--muted-foreground);
  border: 2px solid var(--card);
}

.status-dot.online {
  background: #22c55e;
}

.header-info {
  display: flex;
  flex-direction: column;
  min-width: 0;
}

.player-name {
  font-size: 14px;
  font-weight: 600;
  color: var(--foreground);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.realm-tag {
  font-size: 11px;
  padding: 1px 6px;
  border-radius: 999px;
  width: fit-content;
  font-weight: 500;
}

.realm-tag.common { background: oklch(from var(--quality-common) l c h / 0.15); color: var(--quality-common); }
.realm-tag.uncommon { background: oklch(from var(--quality-uncommon) l c h / 0.15); color: var(--quality-uncommon); }
.realm-tag.rare { background: oklch(from var(--quality-rare) l c h / 0.15); color: var(--quality-rare); }
.realm-tag.epic { background: oklch(from var(--quality-epic) l c h / 0.15); color: var(--quality-epic); }
.realm-tag.legendary { background: oklch(from var(--quality-legendary) l c h / 0.15); color: var(--quality-legendary); }
.realm-tag.mythic { background: oklch(from var(--quality-mythic) l c h / 0.15); color: var(--quality-mythic); }

/* 分隔线 */
.nav-divider {
  height: 1px;
  background: var(--border);
  margin: 4px 12px;
  flex-shrink: 0;
}

/* 资源 */
.resource-strip {
  padding: 4px 12px;
  display: flex;
  flex-direction: column;
  gap: 2px;
  flex-shrink: 0;
}

.resource-row {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 12px;
  color: var(--muted-foreground);
}

.resource-row .res-icon {
  font-size: 16px;
  flex-shrink: 0;
  width: 20px;
  height: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.resource-row .res-value {
  font-family: var(--font-mono);
  font-weight: 600;
  color: var(--foreground);
  font-size: 12px;
}

/* 导航项 */
.nav-items {
  flex: 1;
  overflow-y: auto;
  padding: 4px 8px;
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.nav-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 8px;
  border: none;
  background: transparent;
  border-radius: var(--radius-md);
  cursor: pointer;
  color: var(--muted-foreground);
  font-size: 13px;
  transition: all 0.15s ease;
  white-space: nowrap;
  flex-shrink: 0;
  position: relative;
}

.nav-item:hover {
  background: var(--accent);
  color: var(--foreground);
}

.nav-item.active {
  background: oklch(from var(--primary) l c h / 0.1);
  color: var(--foreground);
}

.nav-icon {
  font-size: 22px;
  flex-shrink: 0;
  width: 28px;
  height: 28px;
  display: flex;
  align-items: center;
  justify-content: center;
  text-align: center;
}

.nav-label {
  overflow: hidden;
  text-overflow: ellipsis;
}

/* 底部 */
.nav-footer {
  flex-shrink: 0;
  padding: 4px 8px 12px;
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.system-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 4px 8px;
  font-size: 11px;
  color: var(--muted-foreground);
  font-family: var(--font-mono);
}

.latency.good { color: #22c55e; }
.latency.normal { color: #eab308; }
.latency.bad { color: #ef4444; }

/* 未读邮件红点 */
.unread-badge {
  position: absolute;
  top: 4px;
  right: 4px;
  min-width: 16px;
  height: 16px;
  padding: 0 4px;
  border-radius: 8px;
  background: #ef4444;
  color: #fff;
  font-size: 10px;
  font-weight: 700;
  line-height: 16px;
  text-align: center;
  pointer-events: none;
}
</style>
