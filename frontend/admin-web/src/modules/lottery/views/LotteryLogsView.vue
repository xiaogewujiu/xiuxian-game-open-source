<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">抽奖日志</p>
        <h2 class="section-title section-title--small">抽奖日志查询</h2>
        <p class="section-note">查看所有玩家的抽奖记录，支持按玩家ID和抽奖池筛选。</p>
      </div>
      <div class="filter-grid">
        <label class="field"><span>玩家ID</span><input v-model.trim="playerId" type="text" placeholder="输入玩家ID" /></label>
        <label class="field"><span>抽奖池ID</span><input v-model.trim="poolId" type="text" placeholder="输入抽奖池ID" /></label>
        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadLogs(1)">查询</button>
        </div>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy"><p class="section-kicker">日志列表</p><h3 class="section-title section-title--small">抽奖日志</h3></div>
        <span class="selected-pill">共 {{ total }} 条</span>
      </div>
      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr><th>玩家ID</th><th>抽奖池</th><th>类型</th><th>消耗</th><th>奖励类型</th><th>奖励名称</th><th>数量</th><th>抽奖时间</th></tr>
          </thead>
          <tbody>
            <tr v-for="log in logs" :key="log.logId">
              <td>{{ log.playerId }}</td><td>{{ log.poolId }}</td>
              <td>{{ getLotteryTypeLabel(log.lotteryType) }}</td>
              <td>{{ getCostLabel(log.costType, log.costAmount) }}</td>
              <td>{{ getRewardTypeLabel(log.rewardType) }}</td>
              <td>{{ log.rewardName }}</td><td>{{ log.rewardAmount }}</td>
              <td>{{ formatDateTime(log.lotteryTime) }}</td>
            </tr>
          </tbody>
        </table>
      </div>
      <div class="pagination-bar" v-if="totalPages > 1">
        <button class="secondary-button" type="button" :disabled="page <= 1" @click="loadLogs(page - 1)">上一页</button>
        <span>第 {{ page }} / {{ totalPages }} 页</span>
        <button class="secondary-button" type="button" :disabled="page >= totalPages" @click="loadLogs(page + 1)">下一页</button>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { LOTTERY_TYPE_OPTIONS, LOTTERY_COST_TYPE_OPTIONS, LOTTERY_REWARD_TYPE_OPTIONS } from '@/constants/game-options'
import { getAdminLotteryLogs } from '@/services/collections'
import type { AdminLotteryLogListItem } from '@/types/admin'

const playerId = ref('')
const poolId = ref('')
const logs = ref<AdminLotteryLogListItem[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = 20
const totalPages = computed(() => Math.ceil(total.value / pageSize))

function formatDateTime(value?: string | null) { return value ? value.replace('T', ' ').slice(0, 19) : '-' }
function getLotteryTypeLabel(v: number) { return LOTTERY_TYPE_OPTIONS.find(o => o.value === v)?.label || `类型${v}` }
function getRewardTypeLabel(v: number) { return LOTTERY_REWARD_TYPE_OPTIONS.find(o => o.value === v)?.label || `类型${v}` }
function getCostLabel(type: number, amount: number) { const label = LOTTERY_COST_TYPE_OPTIONS.find(o => o.value === type)?.label || `类型${type}`; return `${amount} ${label}` }

async function loadLogs(p: number) {
  page.value = p
  const result = await getAdminLotteryLogs(playerId.value, poolId.value, page.value, pageSize)
  logs.value = result.items
  total.value = result.total
}

onMounted(() => { loadLogs(1) })
</script>

<style scoped>
.pagination-bar {
  display: flex; align-items: center; justify-content: center; gap: 1rem; padding: 1rem 0;
}
</style>
