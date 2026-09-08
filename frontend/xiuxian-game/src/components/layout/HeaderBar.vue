<template>
  <!-- 顶部信息栏 -->
  <header class="header-bar">
    <!-- 左侧：角色基础信息 -->
    <div class="header-left">
      <button
        class="header-btn sidebar-toggle-btn"
        :title="isSidebarCollapsed ? '展开侧边栏' : '隐藏侧边栏'"
        @click="$emit('toggle-sidebar')"
      >
        <span class="toggle-icon">{{ isSidebarCollapsed ? '☰' : '◀' }}</span>
      </button>

      <div class="avatar-section">
        <div class="avatar-frame" role="button" tabindex="0" title="点击上传头像" @click="triggerAvatarUpload">
          <div class="avatar-image">
            <AssetIcon :source="player.avatarImagePath || player.avatar" :fallback="player.avatar" :alt="player.name" />
          </div>
        </div>
        <input ref="avatarInputRef" type="file" accept="image/*" hidden @change="handleAvatarChange" />
      </div>

      <div class="info-section">
        <div class="name-row">
          <span class="player-name">{{ player.name }}</span>
          <span class="realm-tag" :class="player.realmTier">{{ player.realm }}</span>
        </div>

        <div class="body-realm">
          <span class="label">肉身:</span>
          <span class="value">{{ player.bodyRealm }}</span>
        </div>
      </div>
    </div>

    <!-- 右侧：系统按钮 -->
    <div class="header-right">
      <button class="header-btn theme-btn" :title="getThemeTitle()" @click="toggleTheme">
        <AssetIcon :source="getThemeIcon()" size="25" />
      </button>

      <button class="header-btn" title="退出" @click="logout">
        <AssetIcon :source="ICON.ui_door" size="25" />
      </button>
    </div>
  </header>
</template>

<script>
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useGameStore } from '../../state/gameStore'
import { buildPlayerView, formatCompactNumber } from '../../services/gameDisplay'
import { apiClient } from '../../lib/apiClient'
import AssetIcon from '../common/AssetIcon.vue'
import { ICON } from '../../icons'
import toast from '@/utils/toast'

/**
 * HeaderBar - 顶部信息栏
 *
 * 显示内容：
 * - 左侧：角色头像、名字、修为境界、肉体境界
 * - 中间：灵石、金币、经验等资源
 * - 右侧：主题切换、设置、退出按钮
 */
export default {
  name: 'HeaderBar',

  components: {
    AssetIcon
  },

  props: {
    isSidebarCollapsed: {
      type: Boolean,
      default: true
    }
  },

  emits: ['toggle-theme', 'toggle-sidebar'],

  setup(props, { emit }) {
    const router = useRouter()
    const gameStore = useGameStore()

    // 顶栏自己的局部状态只有两类：
    // 1. 当前主题。
    // 2. 头像上传 input 的 DOM 引用。
    const currentTheme = ref('light')
    const avatarInputRef = ref(null)

    // 首次挂载时恢复主题设置，保证刷新页面后主题不跳回默认值。
    onMounted(() => {
      const savedTheme = localStorage.getItem('xiuxian-theme')
      if (savedTheme && ['dark', 'light', 'elegant'].includes(savedTheme)) {
        currentTheme.value = savedTheme
        applyTheme()
      }
    })

    // 把主题写回到根节点 data-theme，交给全局样式变量系统接管。
    const applyTheme = () => {
      document.documentElement.setAttribute('data-theme', currentTheme.value)
    }

    // 三套主题按固定顺序轮换：
    // 深色 -> 浅色 -> 雅致 -> 深色。
    const toggleTheme = () => {
      const themes = ['dark', 'light', 'elegant']
      const currentIndex = themes.indexOf(currentTheme.value)
      currentTheme.value = themes[(currentIndex + 1) % themes.length]
      applyTheme()
      localStorage.setItem('xiuxian-theme', currentTheme.value)
      emit('toggle-theme', currentTheme.value)
    }

    // 中文注释：
    // 顶栏展示的玩家信息全部来自真实后端玩家数据；
    // 如果数据还没回来，则给出一个稳定的占位对象，避免模板访问空值报错。
    const player = computed(() => buildPlayerView(gameStore.state.player))



    // 退出时统一走全局游戏仓库的登出逻辑，再跳回登录页。
    const logout = async () => {
      await gameStore.logout('已退出当前账号。')
      router.replace('/')
    }

    const triggerAvatarUpload = () => {
      avatarInputRef.value?.click()
    }

    // 头像上传完成后只刷新玩家快照，不额外刷新背包、装备和排行，减少无意义请求。
    const handleAvatarChange = async (event) => {
      const file = event?.target?.files?.[0]
      event.target.value = ''
      if (!file) {
        return
      }

      try {
        await apiClient.uploadPlayerAvatar(file)
        await gameStore.refreshPlayerSnapshot({
          includeInventory: false,
          includeEquipment: false,
          includeRankings: false
        })
      } catch (error) {
        toast.error(error.message || '头像上传失败。')
      }
    }

    // 当前主题对应的按钮图标。
    const getThemeIcon = () => {
      switch (currentTheme.value) {
        case 'dark': return ICON.ui_moon
        case 'light': return ICON.ui_sun
        case 'elegant': return ICON.ui_candle
        default: return ICON.ui_moon
      }
    }

    // 当前主题对应的按钮提示文案。
    const getThemeTitle = () => {
      switch (currentTheme.value) {
        case 'dark': return '切换到浅色主题'
        case 'light': return '切换到雅致主题'
        case 'elegant': return '切换到深色主题'
        default: return '切换主题'
      }
    }

    return {
      ICON,
      player,
      currentTheme,
      avatarInputRef,
      logout,
      triggerAvatarUpload,
      handleAvatarChange,
      toggleTheme,
      getThemeIcon,
      getThemeTitle
    }
  }
}
</script>

<style scoped>
.header-bar {
  min-height: 78px;
  background: var(--xiuxian-bg-secondary);
  border-bottom: 1px solid var(--border-color);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 8px var(--spacing-lg);
  box-shadow: var(--shadow-sm);
}

/* 左侧 - 角色信息 */
.header-left {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
}

.avatar-frame {
  position: relative;
  width: 50px;
  height: 50px;
  border: 2px solid var(--border-color);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--xiuxian-bg-primary);
}

.avatar-image {
  width: 100%;
  height: 100%;
  font-size: 28px;
  overflow: hidden;
  border-radius: 50%;
}

.info-section {
  display: flex;
  flex-direction: column;
  gap: 3px;
}

.name-row {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
}

.player-name {
  font-size: 16px;
  font-weight: 600;
  color: var(--text-primary);
}

.realm-tag {
  display: inline-flex;
  align-items: center;
  padding: 1px 6px;
  border-radius: var(--radius-sm);
  font-size: 11px;
  font-weight: 600;
  line-height: 1.2;
}

.realm-tag.common { background: oklch(from var(--quality-common) l c h / 0.1); color: var(--quality-common); border: 1px solid oklch(from var(--quality-common) l c h / 0.15); }
.realm-tag.uncommon { background: oklch(from var(--quality-uncommon) l c h / 0.1); color: var(--quality-uncommon); border: 1px solid oklch(from var(--quality-uncommon) l c h / 0.15); }
.realm-tag.rare { background: oklch(from var(--quality-rare) l c h / 0.1); color: var(--quality-rare); border: 1px solid oklch(from var(--quality-rare) l c h / 0.15); }
.realm-tag.epic { background: oklch(from var(--quality-epic) l c h / 0.1); color: var(--quality-epic); border: 1px solid oklch(from var(--quality-epic) l c h / 0.15); }
.realm-tag.legendary { background: oklch(from var(--quality-legendary) l c h / 0.1); color: var(--quality-legendary); border: 1px solid oklch(from var(--quality-legendary) l c h / 0.15); }
.realm-tag.mythic { background: oklch(from var(--quality-mythic) l c h / 0.1); color: var(--quality-mythic); border: 1px solid oklch(from var(--quality-mythic) l c h / 0.15); }

.body-realm {
  font-size: var(--font-size-sm);
  color: var(--text-secondary);
}

.body-realm .label {
  margin-right: 4px;
}

.body-realm .value {
  color: var(--accent-text);
}

/* 中间 - 资源 */
.header-center {
  flex: 1;
  display: flex;
  justify-content: center;
}

.resource-list {
  display: flex;
  gap: var(--spacing-xl);
}

.resource-item {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  padding: var(--spacing-sm) var(--spacing-md);
  background: var(--bg-overlay-light);
  border-radius: var(--radius-md);
  border: 1px solid var(--border-color);
}

.res-icon {
  font-size: 24px;
  width: 28px;
  height: 28px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.res-info {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
}

.res-name {
  font-size: var(--font-size-sm);
  color: var(--text-muted);
}

.res-value {
  font-size: 14px;
  font-weight: 600;
  color: var(--highlight-text);
  font-family: var(--font-mono);
}

/* 右侧 - 按钮 */
.header-right {
  display: flex;
  gap: var(--spacing-sm);
}

.header-btn {
  width: 36px;
  height: 36px;
  background: var(--control-bg);
  border: 1px solid var(--control-border);
  border-radius: var(--radius-md);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 18px;
  color: var(--text-secondary);
  transition: all 0.15s ease;
}

.header-btn:hover {
  border-color: var(--border-color);
  color: var(--accent-text);
  background: var(--control-bg-hover);
}

.sidebar-toggle-btn {
  margin-right: var(--spacing-xs);
  flex-shrink: 0;
}

.toggle-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  line-height: 1;
}
</style>

