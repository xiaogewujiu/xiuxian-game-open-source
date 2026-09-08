<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">锻造运行时</p>
        <h2 class="section-title section-title--small">锻造系统监控</h2>
        <p class="section-note">查看玩家锻造师等级、当前制作任务和完成时间，支持后台修正系统状态。</p>
      </div>

      <div class="filter-grid">
        <label class="field">
          <span>关键字</span>
          <input v-model.trim="keyword" type="text" placeholder="搜索玩家编号、账号或名称" />
        </label>
        <label class="field">
          <span>当前玩家</span>
          <input :value="selectedDetail?.playerId || ''" type="text" placeholder="未选择" disabled />
        </label>
        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadItems">查询</button>
        </div>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">锻造列表</p>
          <h3 class="section-title section-title--small">锻造运行态</h3>
        </div>
        <span class="selected-pill">共 {{ items.length }} 条</span>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>玩家</th>
              <th>账号</th>
              <th>锻造师</th>
              <th>职业上限</th>
              <th>当前任务</th>
              <th>完成时间</th>
              <th>累计锻造</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="item in items"
              :key="item.playerId"
              :class="{ 'is-active': selectedDetail?.playerId === item.playerId }"
              @click="openDetail(item.playerId)"
            >
              <td>{{ item.name }}</td>
              <td>{{ item.account }}</td>
              <td>Lv.{{ item.blacksmithLevel }}</td>
              <td>Lv.{{ item.professionLevelCap }}</td>
              <td>{{ item.isForging ? (item.activeRecipeName || item.activeRecipeId || '进行中') : '空闲' }}</td>
              <td>{{ formatDateTime(item.activeForgeCompleteAt) }}</td>
              <td>{{ item.totalForgeCount }}</td>
            </tr>
          </tbody>
        </table>
      </div>

    </section>

    <AdminModal
      v-model="editorOpen"
      kicker="锻造运行时"
      :title="selectedDetail ? `编辑 ${selectedDetail.name} 的锻造系统` : '锻造系统详情'"
      description="锻造师等级受聚灵阵上限约束，当前任务可单独清空。"
      size="xwide"
    >
      <div v-if="selectedDetail" class="section-stack section-stack--spaced">
        <div class="detail-overview detail-overview--compact">
          <article class="detail-overview__item"><span>玩家</span><strong>{{ selectedDetail.name }}</strong></article>
          <article class="detail-overview__item"><span>账号</span><strong>{{ selectedDetail.account }}</strong></article>
          <article class="detail-overview__item"><span>职业上限</span><strong>Lv.{{ selectedDetail.professionLevelCap }}</strong></article>
          <article class="detail-overview__item"><span>当前任务</span><strong>{{ selectedDetail.isForging ? (selectedDetail.activeRecipeName || selectedDetail.activeRecipeId || '进行中') : '空闲' }}</strong></article>
          <article class="detail-overview__item"><span>开始时间</span><strong>{{ formatDateTime(selectedDetail.activeForgeStartedAt) }}</strong></article>
          <article class="detail-overview__item"><span>完成时间</span><strong>{{ formatDateTime(selectedDetail.activeForgeCompleteAt) }}</strong></article>
        </div>

        <fieldset class="form-fieldset" :disabled="!canWritePlayers">
          <div class="editor-grid editor-grid--three">
            <label class="field"><span>锻造师等级</span><input v-model.number="selectedDetail.blacksmithLevel" type="number" min="1" /></label>
            <label class="field"><span>锻造师经验</span><input v-model.number="selectedDetail.blacksmithExp" type="number" min="0" /></label>
            <label class="field"><span>今日锻造</span><input :value="selectedDetail.todayForgeCount" type="number" disabled /></label>
            <label class="field"><span>累计锻造</span><input :value="selectedDetail.totalForgeCount" type="number" disabled /></label>
            <label class="field"><span>成功锻造</span><input :value="selectedDetail.successForgeCount" type="number" disabled /></label>
          </div>
        </fieldset>
      </div>

      <template #footer>
        <button class="secondary-button" type="button" @click="editorOpen = false">取消</button>
        <button class="secondary-button" type="button" :disabled="!selectedDetail?.isForging || !canWritePlayers" @click="clearTask">清空任务</button>
        <button class="primary-button" type="button" :disabled="!canWritePlayers" @click="saveDetail">保存锻造系统</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import { clearAdminForgeTask, getAdminForgeSystemDetail, getAdminForgeSystems, saveAdminForgeSystem } from '@/services/forge-runtime'
import type { AdminForgeSystemDetail, AdminForgeSystemListItem } from '@/types/admin'
import toast from '@/utils/toast'

// 锻造运行态页聚焦玩家级系统状态修正，因此只需要“写玩家运行态”权限。
const { canWritePlayers } = useAdminPermissions()
const keyword = ref('')
const items = ref<AdminForgeSystemListItem[]>([])
const selectedDetail = ref<AdminForgeSystemDetail | null>(null)
const editorOpen = ref(false)

// 统一格式化后台时间字段。
function formatDateTime(value?: string | null) {
  if (!value) return '未设置'
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? String(value) : date.toLocaleString()
}

// 拉取锻造运行态列表。
async function loadItems() {
  items.value = await getAdminForgeSystems(keyword.value)
}

// 打开指定玩家的锻造系统详情。
async function openDetail(playerId: string) {
  selectedDetail.value = await getAdminForgeSystemDetail(playerId)
  editorOpen.value = true
}

// 保存锻造系统运行态。
async function saveDetail() {
  if (!canWritePlayers.value || !selectedDetail.value) return
  try {
    selectedDetail.value = await saveAdminForgeSystem(selectedDetail.value)
    toast.success('锻造系统数据保存成功。')
    editorOpen.value = false
    await loadItems()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '锻造系统数据保存失败。')
  }
}

async function clearTask() {
  if (!canWritePlayers.value || !selectedDetail.value?.isForging) return
  if (!window.confirm(`确认清空 ${selectedDetail.value.name} 的当前锻造任务吗？`)) return
  try {
    selectedDetail.value = await clearAdminForgeTask(selectedDetail.value.playerId)
    toast.success('锻造任务已清空。')
    await loadItems()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '清空锻造任务失败。')
  }
}

onMounted(loadItems)
</script>
