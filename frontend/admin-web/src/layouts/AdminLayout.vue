<template>
  <div class="admin-shell">
    <button
      v-if="isSidebarOpen"
      class="admin-shell__backdrop"
      type="button"
      aria-label="关闭导航"
      @click="closeSidebar"
    ></button>

    <aside class="admin-sidebar" :class="{ 'is-open': isSidebarOpen }">
      <div class="admin-sidebar__brand">
        <span class="admin-sidebar__logo">灵</span>
        <div class="admin-sidebar__brand-copy">
          <strong>Xiuxian Admin</strong>
          <span>游戏管理后台</span>
        </div>
      </div>

      <div class="admin-sidebar__user">
        <strong>{{ authStore.displayName || '未登录' }}</strong>
        <span>{{ roleText }}</span>
      </div>

      <nav class="admin-nav">
        <section v-for="section in visibleNavSections" :key="section.group" class="admin-nav__section">
          <button
            class="admin-nav__title"
            :class="{ 'is-collapsed': collapsedGroups[section.group] }"
            @click="toggleGroup(section.group)"
          >
            <span>{{ section.group }}</span>
            <span class="admin-nav__arrow">›</span>
          </button>
          <Transition name="collapse">
            <div v-show="!collapsedGroups[section.group]" class="admin-nav__items">
              <RouterLink
                v-for="item in section.items"
                :key="item.to"
                class="admin-nav__item"
                :to="item.to"
                @click="closeSidebar"
              >
                <span class="admin-nav__dot"></span>
                <span class="admin-nav__label">{{ item.label }}</span>
              </RouterLink>
            </div>
          </Transition>
        </section>
      </nav>
    </aside>

    <main class="admin-main">
      <header class="admin-header">
        <div class="admin-header__left">
          <button class="header-icon-button sidebar-toggle" type="button" @click="toggleSidebar">
            ≡
          </button>

          <div class="admin-breadcrumbs">
            <span>后台管理</span>
            <span>/</span>
            <span>{{ activeNavItem?.group || '总览' }}</span>
            <span>/</span>
            <strong>{{ activeNavItem?.label || '仪表盘' }}</strong>
          </div>
        </div>

        <div class="admin-header__right">
          <label class="admin-search">
            <span>搜索</span>
            <input type="text" placeholder="搜索页面功能" />
          </label>

          <button class="header-icon-button" type="button" title="刷新页面" @click="refreshCurrentRoute">
            ↻
          </button>
          <button class="header-icon-button" type="button" title="当前账号">
            {{ authStore.currentUser?.account?.slice(0, 1).toUpperCase() || 'A' }}
          </button>
          <button class="ghost-button ghost-button--header" type="button" @click="logout">
            退出
          </button>
        </div>
      </header>

      <div class="admin-tabs">
        <RouterLink
          v-for="item in currentGroupTabs"
          :key="item.to"
          class="admin-tab"
          :to="item.to"
        >
          {{ item.label }}
        </RouterLink>
      </div>

      <section class="admin-stage">
        <RouterView />
      </section>
    </main>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ADMIN_ROLE_TEXT_MAP, filterAdminNavItems } from '@/constants/admin-navigation'
import type { AdminNavItem } from '@/constants/admin-navigation'
import { useAuthStore } from '@/stores/auth'

type NavSection = {
  group: string
  items: AdminNavItem[]
}

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()
const isSidebarOpen = ref(false)

// 所有菜单组默认折叠
const DEFAULT_COLLAPSED_GROUPS: Record<string, boolean> = {
  '总览': true,
  '玩家与运营': true,
  '世界与战斗': true,
  '宗门': true,
  '经济与物品': true,
  '生活技能': true,
  '角色养成': true,
  '内容与活动': true
}

const collapsedGroups = ref<Record<string, boolean>>({ ...DEFAULT_COLLAPSED_GROUPS })

const visibleNavItems = computed(() => filterAdminNavItems(authStore.role, authStore.permissions))

const visibleNavSections = computed<NavSection[]>(() => {
  const groups = new Map<string, AdminNavItem[]>()

  for (const item of visibleNavItems.value) {
    if (!groups.has(item.group)) {
      groups.set(item.group, [])
    }

    groups.get(item.group)!.push(item)
  }

  return Array.from(groups.entries()).map(([group, items]) => ({ group, items }))
})

const activeNavItem = computed(() => visibleNavItems.value.find((item) => item.name === route.name) ?? visibleNavItems.value[0] ?? null)

const currentGroupTabs = computed(() => {
  if (!activeNavItem.value) {
    return visibleNavItems.value.slice(0, 6)
  }

  return visibleNavItems.value.filter((item) => item.group === activeNavItem.value?.group)
})

const roleText = computed(() => ADMIN_ROLE_TEXT_MAP[authStore.role] || authStore.role || '未登录')

// 自动展开当前页面所在的菜单组
watch(() => route.fullPath, () => {
  isSidebarOpen.value = false
  if (activeNavItem.value) {
    collapsedGroups.value[activeNavItem.value.group] = false
  }
}, { immediate: true })

function toggleSidebar() {
  isSidebarOpen.value = !isSidebarOpen.value
}

function closeSidebar() {
  isSidebarOpen.value = false
}

function toggleGroup(group: string) {
  collapsedGroups.value[group] = !collapsedGroups.value[group]
}

function refreshCurrentRoute() {
  window.location.reload()
}

async function logout() {
  await authStore.logout()
  router.replace({ name: 'admin-login' })
}
</script>

<style scoped>
.admin-nav__section {
  margin: 0 0 12px;
  padding: 0;
  gap: 0;
}

.admin-nav__section + .admin-nav__section {
  border-top: 1px solid rgba(148, 163, 184, 0.12);
  padding-top: 8px;
}

.admin-nav__title {
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
  min-height: 36px;
  padding: 8px 12px;
  margin: 0;
  background: transparent;
  border: none;
  border-radius: 6px;
  color: var(--text-primary, #f1f5f9);
  font-size: 14px;
  font-weight: 700;
  letter-spacing: 0.2px;
  text-transform: none;
  cursor: pointer;
  transition: color 0.2s, background 0.15s;
}

.admin-nav__title::before {
  content: '';
  width: 3px;
  height: 16px;
  margin-right: 8px;
  flex: 0 0 3px;
  border-radius: 2px;
  background: var(--accent-primary, #a78bfa);
  opacity: 1;
}

.admin-nav__title span:first-child {
  flex: 1;
  text-align: left;
}

.admin-nav__title:hover {
  color: var(--text-primary, #ffffff);
  background: var(--bg-panel-muted, rgba(255, 255, 255, 0.08));
}

.admin-nav__arrow {
  color: var(--text-secondary, #cbd5e1);
  font-size: 17px;
  transition: transform 0.2s ease;
  font-weight: 400;
}

.admin-nav__title.is-collapsed .admin-nav__arrow {
  transform: rotate(0deg);
}

.admin-nav__title:not(.is-collapsed) .admin-nav__arrow {
  transform: rotate(90deg);
}

.admin-nav__items {
  overflow: hidden;
  display: flex;
  flex-direction: column;
  gap: 2px;
  padding-bottom: 5px;
  padding-left: 8px;
}

/* Collapse transition */
.collapse-enter-active,
.collapse-leave-active {
  transition: all 0.2s ease;
}

.collapse-enter-from,
.collapse-leave-to {
  opacity: 0;
  max-height: 0;
  padding-bottom: 0;
}

.collapse-enter-to,
.collapse-leave-from {
  opacity: 1;
  max-height: 500px;
}
</style>
