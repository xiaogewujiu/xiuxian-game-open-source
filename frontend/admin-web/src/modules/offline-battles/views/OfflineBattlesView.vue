<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">离线挂机</p>
        <h2 class="section-title section-title--small">离线挂机监控</h2>
        <p class="section-note">集中查看当前挂机账号，必要时可从后台强制停止。</p>
      </div>

      <div class="filter-grid">
        <label class="field">
          <span>关键字</span>
          <input v-model.trim="keyword" type="text" placeholder="搜索玩家、账号或地图" />
        </label>
        <label class="field">
          <span>当前记录</span>
          <input :value="selectedBattle?.playerId || ''" type="text" placeholder="未选择" disabled />
        </label>
        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadBattles">查询</button>
        </div>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">挂机列表</p>
          <h3 class="section-title section-title--small">挂机列表</h3>
        </div>
        <span class="selected-pill">共 {{ battles.length }} 条</span>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>玩家</th>
              <th>账号</th>
              <th>地图</th>
              <th>开始时间</th>
              <th>累计场次</th>
              <th>经验</th>
              <th>金币</th>
              <th>操作</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="battle in battles" :key="battle.playerId" :class="{ 'is-active': selectedBattle?.playerId === battle.playerId }" @click="selectedBattle = battle">
              <td>{{ battle.name }}</td>
              <td>{{ battle.account }}</td>
              <td>{{ battle.mapName }}</td>
              <td>{{ formatDateTime(battle.startedAtUtc) }}</td>
              <td>{{ battle.totalBattles }} / {{ battle.winBattles }}</td>
              <td>{{ battle.expGained }}</td>
              <td>{{ battle.goldGained }}</td>
              <td><button class="inline-action" type="button" :disabled="!canWritePlayers" @click.stop="stopBattle(battle)">强制停止</button></td>
            </tr>
          </tbody>
        </table>
      </div>

    </section>

    <AdminModal
      v-model="summaryModalOpen"
      kicker="挂机汇总"
      title="停止挂机汇总"
      description="记录后台强制停止后的本次挂机结果。"
    >
      <div v-if="lastSummary" class="section-stack">
        <div class="detail-overview detail-overview--compact">
          <article class="detail-overview__item"><span>玩家</span><strong>{{ lastSummary.name }}</strong></article>
          <article class="detail-overview__item"><span>地图</span><strong>{{ lastSummary.mapName }}</strong></article>
          <article class="detail-overview__item"><span>总时长</span><strong>{{ formatDuration(lastSummary.durationSeconds) }}</strong></article>
          <article class="detail-overview__item"><span>总场次</span><strong>{{ lastSummary.totalBattles }}</strong></article>
          <article class="detail-overview__item"><span>经验</span><strong>{{ lastSummary.expGained }}</strong></article>
          <article class="detail-overview__item"><span>金币</span><strong>{{ lastSummary.goldGained }}</strong></article>
        </div>
      </div>

      <template #footer>
        <button class="primary-button" type="button" @click="summaryModalOpen = false">关闭</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import { getAdminOfflineBattles, stopAdminPlayerOfflineBattle } from '@/services/players'
import type { AdminOfflineBattleListItem, AdminOfflineBattleSummary } from '@/types/admin'
import toast from '@/utils/toast'

// 离线挂机页只需要“写玩家运行态”权限来执行强制停止。
const { canWritePlayers } = useAdminPermissions()
const keyword = ref('')
const battles = ref<AdminOfflineBattleListItem[]>([])
const selectedBattle = ref<AdminOfflineBattleListItem | null>(null)
const lastSummary = ref<AdminOfflineBattleSummary | null>(null)
const summaryModalOpen = ref(false)

// 统一格式化后台时间字段。
function formatDateTime(value?: string | null) {
  if (!value) return '未设置'
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? String(value) : date.toLocaleString()
}

// 把总秒数转换成“小时 + 分钟”文案。
function formatDuration(totalSeconds: number) {
  const safeSeconds = Math.max(0, totalSeconds || 0)
  const hours = Math.floor(safeSeconds / 3600)
  const minutes = Math.floor((safeSeconds % 3600) / 60)
  return `${hours}小时${minutes}分钟`
}

// 拉取离线挂机中的玩家列表。
async function loadBattles() {
  battles.value = await getAdminOfflineBattles(keyword.value)
}

// 强制停止指定玩家的离线挂机，并弹出本次挂机汇总。
async function stopBattle(battle: AdminOfflineBattleListItem) {
  if (!canWritePlayers.value) return
  if (!window.confirm(`确认停止玩家 ${battle.name} 的离线挂机吗？`)) return

  try {
    lastSummary.value = await stopAdminPlayerOfflineBattle(battle.playerId)
    summaryModalOpen.value = true
    toast.success(`已停止 ${battle.name} 的离线挂机。`)
    await loadBattles()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '停止离线挂机失败。')
  }
}

onMounted(loadBattles)
</script>
