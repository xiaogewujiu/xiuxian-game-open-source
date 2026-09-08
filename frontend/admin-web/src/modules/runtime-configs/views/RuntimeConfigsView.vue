<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">运行配置</p>
        <h2 class="section-title section-title--small">运行配置域</h2>
        <p class="section-note">第 1 阶段先统一查看运行时配置域状态，并提供后台缓存刷新入口。</p>
      </div>

      <div class="filter-grid">
        <label class="field">
          <span>关键字</span>
          <input v-model.trim="keyword" type="text" placeholder="搜索配置域、分组或说明" />
        </label>
        <label class="field">
          <span>最近执行</span>
          <input :value="lastResult?.name || ''" type="text" placeholder="未执行" disabled />
        </label>
        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadConfigs">刷新列表</button>
          <button class="primary-button" type="button" :disabled="!canManageConfigs || refreshingAll" @click="refreshAll">
            {{ refreshingAll ? '刷新中...' : '刷新全部已接入域' }}
          </button>
        </div>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">配置域列表</p>
          <h3 class="section-title section-title--small">配置域列表</h3>
        </div>
        <span class="selected-pill">共 {{ filteredConfigs.length }} 个域</span>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>配置域</th>
              <th>分组</th>
              <th>当前版本</th>
              <th>刷新能力</th>
              <th>最近状态</th>
              <th>最后应用时间</th>
              <th>最后执行人</th>
              <th>刷新次数</th>
              <th>说明</th>
              <th>操作</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="config in filteredConfigs"
              :key="config.domain"
              :class="{ 'is-active': selectedConfig?.domain === config.domain }"
              @click="selectedConfig = config"
            >
              <td>
                <strong>{{ config.name }}</strong>
                <div class="muted-text">{{ config.domain }}</div>
              </td>
              <td>{{ config.group }}</td>
              <td>{{ config.currentVersion || '未设置' }}</td>
              <td>
                <span :class="config.refreshSupported ? 'status-chip status-chip--success' : 'status-chip status-chip--muted'">
                  {{ config.refreshSupported ? '已接入' : '待接入' }}
                </span>
              </td>
              <td>
                <span :class="statusClass(config.lastRefreshStatus)">{{ config.lastRefreshStatus }}</span>
                <div v-if="config.lastRefreshMessage" class="muted-text">{{ config.lastRefreshMessage }}</div>
              </td>
              <td>{{ formatDateTime(config.lastAppliedAt) }}</td>
              <td>{{ config.lastAppliedBy || '未记录' }}</td>
              <td>{{ config.refreshCount }}</td>
              <td>{{ config.description }}</td>
              <td>
                <button
                  class="inline-action"
                  type="button"
                  :disabled="!canManageConfigs || !config.refreshSupported || refreshingDomain === config.domain"
                  @click.stop="refreshOne(config)"
                >
                  {{ refreshingDomain === config.domain ? '刷新中...' : '刷新' }}
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

    </section>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import { getAdminRuntimeConfigs, refreshAdminRuntimeConfig, refreshAllAdminRuntimeConfigs } from '@/services/runtime-configs'
import type { AdminRuntimeConfigDomainStatus, AdminRuntimeConfigRefreshResult } from '@/types/admin'
import toast from '@/utils/toast'

// 页面主状态：
// configs 是全部运行时配置域，lastResult 记录最近一次刷新结果，
// refreshingDomain / refreshingAll 用来锁定按钮状态。
const { canManageConfigs } = useAdminPermissions()
const keyword = ref('')
const configs = ref<AdminRuntimeConfigDomainStatus[]>([])
const selectedConfig = ref<AdminRuntimeConfigDomainStatus | null>(null)
const lastResult = ref<AdminRuntimeConfigRefreshResult | null>(null)
const refreshingDomain = ref('')
const refreshingAll = ref(false)

// 关键字过滤配置域列表。
const filteredConfigs = computed(() => {
  const search = keyword.value.trim().toLowerCase()
  if (!search) return configs.value

  return configs.value.filter((item) =>
    item.domain.toLowerCase().includes(search) ||
    item.name.toLowerCase().includes(search) ||
    item.group.toLowerCase().includes(search) ||
    item.description.toLowerCase().includes(search)
  )
})

// 统一格式化后台时间字段。
function formatDateTime(value?: string | null) {
  if (!value) return '未执行'
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? String(value) : date.toLocaleString()
}

// 根据最近刷新状态映射对应的颜色标签。
function statusClass(status: string) {
  switch (status) {
    case '成功':
      return 'status-chip status-chip--success'
    case '失败':
      return 'status-chip status-chip--danger'
    case '待接入':
      return 'status-chip status-chip--warning'
    default:
      return 'status-chip status-chip--muted'
  }
}

// 拉取配置域列表。
async function loadConfigs() {
  configs.value = await getAdminRuntimeConfigs()
}

// 刷新单个配置域。
async function refreshOne(config: AdminRuntimeConfigDomainStatus) {
  if (!canManageConfigs.value || !config.refreshSupported) return

  try {
    refreshingDomain.value = config.domain
    const result = await refreshAdminRuntimeConfig(config.domain)
    lastResult.value = result
    toast.success(`${result.name}刷新成功。`)
    await loadConfigs()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '刷新运行时配置域失败。')
  } finally {
    refreshingDomain.value = ''
  }
}

// 刷新全部已接入配置域。
async function refreshAll() {
  if (!canManageConfigs.value) return

  try {
    refreshingAll.value = true
    const results = await refreshAllAdminRuntimeConfigs()
    lastResult.value = results.length > 0 ? results[results.length - 1] : null
    toast.success(`已刷新 ${results.length} 个已接入配置域。`)
    await loadConfigs()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '批量刷新运行时配置域失败。')
  } finally {
    refreshingAll.value = false
  }
}

onMounted(loadConfigs)
</script>
