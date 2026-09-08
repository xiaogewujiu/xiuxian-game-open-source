<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">审计日志</p>
        <h2 class="section-title section-title--small">审计日志</h2>
        <p class="section-note">按操作人、请求路径和资源关键字查看后台操作记录。</p>
      </div>

      <div class="filter-grid">
        <label class="field">
          <span>关键字</span>
          <input v-model.trim="keyword" type="text" placeholder="搜索操作人、路径或资源" />
        </label>
        <label class="field">
          <span>当前选中</span>
          <input :value="selectedLog?.operatorName || ''" type="text" placeholder="未选择记录" disabled />
        </label>
        <label class="field">
          <span>执行结果</span>
          <select v-model="successFilter">
            <option value="">全部</option>
            <option value="success">成功</option>
            <option value="failure">失败</option>
          </select>
        </label>

        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="resetFilters">重置</button>
          <button class="primary-button" type="button" @click="loadLogs">查询</button>
        </div>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">日志列表</p>
          <h3 class="section-title section-title--small">日志列表</h3>
        </div>
        <span class="selected-pill">共 {{ filteredLogs.length }} 条记录</span>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>操作人</th>
              <th>角色</th>
              <th>方法</th>
              <th>请求路径</th>
              <th>资源键</th>
              <th>目标编号</th>
              <th>状态</th>
              <th>时间</th>
              <th>错误信息</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="log in filteredLogs"
              :key="log.logId"
              :class="{ 'is-active': selectedLog?.logId === log.logId }"
              @click="selectLog(log.logId)"
            >
              <td>{{ log.operatorName || '未知操作人' }}</td>
              <td>{{ log.operatorRole || '无角色' }}</td>
              <td>{{ log.httpMethod }}</td>
              <td>{{ log.path }}</td>
              <td>{{ log.resourceKey || '无资源键' }}</td>
              <td>{{ log.targetId || '无目标编号' }}</td>
              <td>
                <span :class="log.success ? 'status-badge status-badge--success' : 'status-badge status-badge--danger'">
                  {{ log.success ? '成功' : '失败' }} / {{ log.statusCode }}
                </span>
              </td>
              <td>{{ log.createTime }}</td>
              <td class="data-table__message">{{ log.errorMessage || '-' }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>

    <section class="panel-box audit-detail" v-if="selectedLog">
      <div class="detail-header">
        <div>
          <p class="section-kicker">日志详情</p>
          <h2 class="section-title section-title--small">{{ selectedLog.path }}</h2>
        </div>
        <span :class="selectedLog.success ? 'status-badge status-badge--success' : 'status-badge status-badge--danger'">
          {{ selectedLog.success ? '执行成功' : '执行失败' }}
        </span>
      </div>

      <div class="detail-overview">
        <article class="detail-overview__item"><span>方法</span><strong>{{ selectedLog.httpMethod }}</strong></article>
        <article class="detail-overview__item"><span>操作人</span><strong>{{ selectedLog.operatorName || '未知' }}</strong></article>
        <article class="detail-overview__item"><span>资源键</span><strong>{{ selectedLog.resourceKey || '无' }}</strong></article>
        <article class="detail-overview__item"><span>目标编号</span><strong>{{ selectedLog.targetId || '无' }}</strong></article>
      </div>

      <div class="editor-grid editor-grid--three">
        <label class="field"><span>日志编号</span><input :value="selectedLog.logId" type="text" disabled /></label>
        <label class="field"><span>角色</span><input :value="selectedLog.operatorRole || ''" type="text" disabled /></label>
        <label class="field"><span>客户端 IP</span><input :value="selectedLog.ipAddress || ''" type="text" disabled /></label>
        <label class="field"><span>操作人编号</span><input :value="selectedLog.operatorId || ''" type="text" disabled /></label>
        <label class="field"><span>状态码</span><input :value="String(selectedLog.statusCode)" type="text" disabled /></label>
        <label class="field"><span>记录时间</span><input :value="selectedLog.createTime" type="text" disabled /></label>
      </div>

      <p v-if="selectedLog.errorMessage" class="form-message">{{ selectedLog.errorMessage }}</p>

      <section class="audit-detail__section">
        <h3>字段差异</h3>
        <pre class="json-block">{{ formatJsonBlock(selectedLog.diffJson) }}</pre>
      </section>
      <section class="audit-detail__section">
        <h3>操作前快照</h3>
        <pre class="json-block">{{ formatJsonBlock(selectedLog.beforeJson) }}</pre>
      </section>
      <section class="audit-detail__section">
        <h3>操作后快照</h3>
        <pre class="json-block">{{ formatJsonBlock(selectedLog.afterJson) }}</pre>
      </section>
      <section class="audit-detail__section">
        <h3>请求体</h3>
        <pre class="json-block">{{ formatJsonBlock(selectedLog.requestJson) }}</pre>
      </section>
      <section class="audit-detail__section">
        <h3>响应体</h3>
        <pre class="json-block">{{ formatJsonBlock(selectedLog.responseJson) }}</pre>
      </section>
    </section>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import type { AdminAuditLogDetail, AdminAuditLogListItem } from '@/types/admin'
import { getAdminAuditLogDetail, getAdminAuditLogs } from '@/services/audit'

// 审计页的核心流程很简单：
// 先查列表，再按点击的日志编号读取完整详情。
const keyword = ref('')
const successFilter = ref('')
const logs = ref<AdminAuditLogListItem[]>([])
const selectedLog = ref<AdminAuditLogDetail | null>(null)

// 后端已支持按执行结果筛选。
const filteredLogs = logs

// 统一把 JSON 文本格式化成可阅读的代码块。
function formatJsonBlock(value?: string | null) {
  if (!value) {
    return '无'
  }

  try {
    return JSON.stringify(JSON.parse(value), null, 2)
  } catch {
    return value
  }
}

// 重置筛选关键字并重新查询。
function resetFilters() {
  keyword.value = ''
  loadLogs()
}

// 查询日志列表，并自动选中第一条详情。
async function loadLogs() {
  const filterSuccess = successFilter.value === '' ? null : successFilter.value === 'success'
  logs.value = await getAdminAuditLogs(keyword.value, 200, filterSuccess)
  if (logs.value.length === 0) {
    selectedLog.value = null
    return
  }

  if (!selectedLog.value || !logs.value.some((log) => log.logId === selectedLog.value?.logId)) {
    await selectLog(logs.value[0].logId)
  }
}

// 查询指定日志详情。
async function selectLog(logId: string) {
  selectedLog.value = await getAdminAuditLogDetail(logId)
}

onMounted(loadLogs)
</script>
