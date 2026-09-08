<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">炼丹运行时</p>
        <h2 class="section-title section-title--small">炼丹系统监控</h2>
        <p class="section-note">查看玩家炼丹师等级、当前炼制任务和完成时间，支持后台修正系统状态。</p>
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
          <p class="section-kicker">炼丹列表</p>
          <h3 class="section-title section-title--small">炼丹运行态</h3>
        </div>
        <span class="selected-pill">共 {{ items.length }} 条</span>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>玩家</th>
              <th>账号</th>
              <th>丹炉</th>
              <th>炼丹师</th>
              <th>职业上限</th>
              <th>当前任务</th>
              <th>完成时间</th>
              <th>累计炼制</th>
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
              <td>Lv.{{ item.furnaceLevel }}</td>
              <td>Lv.{{ item.alchemistLevel }}</td>
              <td>Lv.{{ item.professionLevelCap }}</td>
              <td>{{ item.isCrafting ? (item.activeRecipeName || item.activeRecipeId || '进行中') : '空闲' }}</td>
              <td>{{ formatDateTime(item.activeCraftCompleteAt) }}</td>
              <td>{{ item.totalCraftCount }}</td>
            </tr>
          </tbody>
        </table>
      </div>

    </section>

    <AdminModal
      v-model="editorOpen"
      kicker="炼丹运行时"
      :title="selectedDetail ? `编辑 ${selectedDetail.name} 的炼丹系统` : '炼丹系统详情'"
      description="职业等级按聚灵阵上限约束，当前任务可单独清空。"
      size="xwide"
    >
      <div v-if="selectedDetail" class="section-stack section-stack--spaced">
        <div class="detail-overview detail-overview--compact">
          <article class="detail-overview__item"><span>玩家</span><strong>{{ selectedDetail.name }}</strong></article>
          <article class="detail-overview__item"><span>账号</span><strong>{{ selectedDetail.account }}</strong></article>
          <article class="detail-overview__item"><span>职业上限</span><strong>Lv.{{ selectedDetail.professionLevelCap }}</strong></article>
          <article class="detail-overview__item"><span>当前任务</span><strong>{{ selectedDetail.isCrafting ? (selectedDetail.activeRecipeName || selectedDetail.activeRecipeId || '进行中') : '空闲' }}</strong></article>
          <article class="detail-overview__item"><span>开始时间</span><strong>{{ formatDateTime(selectedDetail.activeCraftStartedAt) }}</strong></article>
          <article class="detail-overview__item"><span>完成时间</span><strong>{{ formatDateTime(selectedDetail.activeCraftCompleteAt) }}</strong></article>
        </div>

        <fieldset class="form-fieldset" :disabled="!canWritePlayers">
          <div class="editor-grid editor-grid--three">
            <label class="field"><span>丹炉等级</span><input v-model.number="selectedDetail.furnaceLevel" type="number" min="1" /></label>
            <label class="field"><span>炼丹师等级</span><input v-model.number="selectedDetail.alchemistLevel" type="number" min="1" /></label>
            <label class="field"><span>炼丹师经验</span><input v-model.number="selectedDetail.alchemistExp" type="number" min="0" /></label>
            <label class="field"><span>熟练度</span><input v-model.number="selectedDetail.proficiency" type="number" min="0" /></label>
            <label class="field"><span>成功率加成</span><input v-model.number="selectedDetail.successRateBonus" type="number" min="0" /></label>
            <label class="field"><span>耗时减免</span><input v-model.number="selectedDetail.craftTimeReduction" type="number" min="0" /></label>
            <label class="field"><span>产量加成</span><input v-model.number="selectedDetail.yieldBonus" type="number" min="0" /></label>
            <label class="field"><span>今日炼制</span><input :value="selectedDetail.todayCraftCount" type="number" disabled /></label>
            <label class="field"><span>累计炼制</span><input :value="selectedDetail.totalCraftCount" type="number" disabled /></label>
          </div>
        </fieldset>
      </div>

      <template #footer>
        <button class="secondary-button" type="button" @click="editorOpen = false">取消</button>
        <button class="secondary-button" type="button" :disabled="!selectedDetail?.isCrafting || !canWritePlayers" @click="clearTask">清空任务</button>
        <button class="primary-button" type="button" :disabled="!canWritePlayers" @click="saveDetail">保存炼丹系统</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import { clearAdminAlchemyTask, getAdminAlchemySystemDetail, getAdminAlchemySystems, saveAdminAlchemySystem } from '@/services/alchemy-runtime'
import type { AdminAlchemySystemDetail, AdminAlchemySystemListItem } from '@/types/admin'
import toast from '@/utils/toast'

// 炼丹运行态页聚焦玩家级系统状态修正，因此只需要“写玩家运行态”权限。
const { canWritePlayers } = useAdminPermissions()
const keyword = ref('')
const items = ref<AdminAlchemySystemListItem[]>([])
const selectedDetail = ref<AdminAlchemySystemDetail | null>(null)
const editorOpen = ref(false)

// 统一格式化后台时间字段。
function formatDateTime(value?: string | null) {
  if (!value) return '未设置'
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? String(value) : date.toLocaleString()
}

// 拉取炼丹运行态列表。
async function loadItems() {
  items.value = await getAdminAlchemySystems(keyword.value)
}

// 打开指定玩家的炼丹系统详情。
async function openDetail(playerId: string) {
  selectedDetail.value = await getAdminAlchemySystemDetail(playerId)
  editorOpen.value = true
}

// 保存炼丹系统运行态。
async function saveDetail() {
  if (!canWritePlayers.value || !selectedDetail.value) return
  try {
    selectedDetail.value = await saveAdminAlchemySystem(selectedDetail.value)
    toast.success('炼丹系统数据保存成功。')
    editorOpen.value = false
    await loadItems()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '炼丹系统数据保存失败。')
  }
}

// 清空当前炼丹任务。
async function clearTask() {
  if (!canWritePlayers.value || !selectedDetail.value?.isCrafting) return
  if (!window.confirm(`确认清空 ${selectedDetail.value.name} 的当前炼丹任务吗？`)) return
  try {
    selectedDetail.value = await clearAdminAlchemyTask(selectedDetail.value.playerId)
    toast.success('炼丹任务已清空。')
    await loadItems()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '清空炼丹任务失败。')
  }
}

onMounted(loadItems)
</script>
