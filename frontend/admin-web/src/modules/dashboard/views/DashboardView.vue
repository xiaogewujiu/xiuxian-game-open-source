<template>
  <div class="dashboard-grid">
    <section class="panel-box dashboard-overview">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">概览</p>
          <h2 class="section-title section-title--small">系统概览</h2>
          <p class="section-note">展示后台总体规模、常用入口和当前模块分区。</p>
        </div>
        <span class="selected-pill">模块总数 {{ visibleNavItems.length }}</span>
      </div>

      <div class="stat-grid">
        <article class="stat-card">
          <span class="stat-card__label">玩家总数</span>
          <strong class="stat-card__value">{{ summary?.playerCount ?? '-' }}</strong>
          <span class="stat-card__hint">数据库内玩家角色数量</span>
        </article>
        <article class="stat-card">
          <span class="stat-card__label">后台账号</span>
          <strong class="stat-card__value">{{ summary?.adminCount ?? '-' }}</strong>
          <span class="stat-card__hint">拥有后台登录权限的管理员数量</span>
        </article>
        <article class="stat-card">
          <span class="stat-card__label">地图模板</span>
          <strong class="stat-card__value">{{ summary?.mapTemplateCount ?? '-' }}</strong>
          <span class="stat-card__hint">普通地图与副本层模板总量</span>
        </article>
        <article class="stat-card">
          <span class="stat-card__label">怪物模板</span>
          <strong class="stat-card__value">{{ summary?.monsterTemplateCount ?? '-' }}</strong>
          <span class="stat-card__hint">战斗与掉落相关怪物配置数量</span>
        </article>
      </div>
    </section>

    <section class="dashboard-section-grid">
      <section class="panel-box table-panel">
        <div class="section-header-copy">
          <p class="section-kicker">常用入口</p>
          <h3 class="section-title section-title--small">常用入口</h3>
        </div>

        <table class="dashboard-table">
          <thead>
            <tr>
              <th>页面</th>
              <th>分区</th>
              <th>说明</th>
              <th>进入</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in featuredItems" :key="item.name">
              <td>{{ item.label }}</td>
              <td>{{ item.group }}</td>
              <td>{{ item.blurb }}</td>
              <td>
                <RouterLink class="secondary-button dashboard-link-button" :to="item.to">
                  打开
                </RouterLink>
              </td>
            </tr>
          </tbody>
        </table>
      </section>

      <section class="panel-box table-panel">
        <div class="section-header-copy">
          <p class="section-kicker">模块分区</p>
          <h3 class="section-title section-title--small">模块分区</h3>
        </div>

        <table class="dashboard-table">
          <thead>
            <tr>
              <th>分区</th>
              <th>数量</th>
              <th>页面示例</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="section in visibleNavSections" :key="section.group">
              <td>{{ section.group }}</td>
              <td>{{ section.items.length }}</td>
              <td>{{ section.items.slice(0, 3).map((item) => item.label).join(' / ') }}</td>
            </tr>
          </tbody>
        </table>
      </section>
    </section>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { filterAdminNavItems } from '@/constants/admin-navigation'
import type { AdminNavItem } from '@/constants/admin-navigation'
import { useAuthStore } from '@/stores/auth'
import type { AdminDashboardSummary } from '@/types/admin'
import { getDashboardSummary } from '@/services/dashboard'

// 仪表盘只负责把“当前账号可见的导航项”和“后台总览统计”聚合展示出来。
const authStore = useAuthStore()
const summary = ref<AdminDashboardSummary | null>(null)

// 根据当前管理员权限过滤出真正可见的导航项。
const visibleNavItems = computed(() => filterAdminNavItems(authStore.role, authStore.permissions))

// 按导航分组整理页面分区，供右侧分区表复用。
const visibleNavSections = computed(() => {
  const groups = new Map<string, AdminNavItem[]>()

  for (const item of visibleNavItems.value) {
    if (!groups.has(item.group)) {
      groups.set(item.group, [])
    }

    groups.get(item.group)!.push(item)
  }

  return Array.from(groups.entries()).map(([group, items]) => ({ group, items }))
})

const featuredNames = ['admin-players', 'admin-items', 'admin-maps', 'admin-monsters', 'admin-alchemy-recipes', 'admin-audit-logs']

// 常用入口固定从可见导航里抽一组最常用页面。
const featuredItems = computed(() => {
  const byName = new Map(visibleNavItems.value.map((item) => [item.name, item]))
  return featuredNames.map((name) => byName.get(name)).filter((item): item is AdminNavItem => Boolean(item))
})

// 页面首次进入时拉取后台概览统计。
onMounted(async () => {
  summary.value = await getDashboardSummary()
})
</script>
