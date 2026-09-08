<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">竞技场</p>
        <h2 class="section-title section-title--small">竞技场管理</h2>
        <p class="section-note">管理PVP竞技场、查看玩家排名和对战日志。</p>
      </div>

      <div class="detail-overview" v-if="overview">
        <article class="detail-overview__item"><span>赛季</span><strong>#{{ overview.seasonNumber }}</strong></article>
        <article class="detail-overview__item"><span>参与人数</span><strong>{{ overview.totalPlayers }}</strong></article>
        <article class="detail-overview__item"><span>今日战斗</span><strong>{{ overview.todayBattles }}</strong></article>
        <article class="detail-overview__item"><span>今日活跃</span><strong>{{ overview.activePlayers }}</strong></article>
        <article class="detail-overview__item"><span>平均积分</span><strong>{{ overview.averagePoints }}</strong></article>
      </div>

      <div class="tab-bar">
        <button
          v-for="tab in TAB_OPTIONS"
          :key="tab.value"
          class="tab-button"
          :class="{ 'is-active': activeTab === tab.value }"
          type="button"
          @click="activeTab = tab.value"
        >{{ tab.label }}</button>
      </div>
    </section>

    <!-- Tab 1: 玩家列表 -->
    <template v-if="activeTab === 'players'">
      <section class="panel-box filter-panel">
        <div class="section-header-copy">
          <p class="section-kicker">玩家查询</p>
          <h3 class="section-title section-title--small">竞技场玩家</h3>
        </div>
        <div class="filter-grid">
          <label class="field">
            <span>关键字</span>
            <input v-model.trim="playerKeyword" type="text" placeholder="搜索玩家ID或名称" />
          </label>
          <div class="filter-actions">
            <button class="secondary-button" type="button" @click="loadPlayers">查询</button>
          </div>
        </div>
      </section>

      <section class="panel-box table-panel">
        <div class="section-header-row">
          <div class="section-header-copy">
            <p class="section-kicker">玩家列表</p>
            <h3 class="section-title section-title--small">竞技场排名</h3>
          </div>
          <span class="selected-pill">共 {{ players.length }} 人</span>
        </div>
        <div class="table-wrap">
          <table class="data-table">
            <thead>
              <tr>
                <th>排名</th>
                <th>玩家</th>
                <th>积分</th>
                <th>胜/负</th>
                <th>连胜</th>
                <th>状态</th>
                <th>最后战斗</th>
                <th>操作</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="p in players" :key="p.id">
                <td>#{{ p.rank }}</td>
                <td>{{ p.playerName }} ({{ p.playerId }})</td>
                <td>{{ p.points }}</td>
                <td>{{ p.wins }}/{{ p.losses }}</td>
                <td>{{ p.winStreak }}</td>
                <td>
                  <span v-if="isBanned(p)" style="color: #ef4444; font-weight: 600;">禁赛中</span>
                  <span v-else style="color: #22c55e;">正常</span>
                </td>
                <td>{{ formatDateTime(p.lastBattleAt) }}</td>
                <td>
                  <button class="secondary-button secondary-button--sm" type="button" @click="openAdjustPoints(p)">调分</button>
                  <button v-if="isBanned(p)" class="secondary-button secondary-button--sm" type="button" style="margin-left:4px;color:#22c55e;" @click="handleUnban(p)">解禁</button>
                  <button v-else class="secondary-button secondary-button--sm" type="button" style="margin-left:4px;color:#ef4444;" @click="openBan(p)">禁赛</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </template>

    <!-- Tab 2: 对战日志 -->
    <template v-if="activeTab === 'logs'">
      <section class="panel-box filter-panel">
        <div class="section-header-copy">
          <p class="section-kicker">日志查询</p>
          <h3 class="section-title section-title--small">对战日志</h3>
        </div>
        <div class="filter-grid">
          <label class="field">
            <span>玩家</span>
            <select v-model="logPlayerId">
              <option value="">请选择玩家</option>
              <option v-for="p in players" :key="p.playerId" :value="p.playerId">{{ p.playerName }} ({{ p.playerId }})</option>
            </select>
          </label>
          <div class="filter-actions">
            <button class="secondary-button" type="button" @click="loadLogs">查询</button>
          </div>
        </div>
      </section>

      <section class="panel-box table-panel">
        <div class="section-header-row">
          <div class="section-header-copy">
            <p class="section-kicker">对战记录</p>
            <h3 class="section-title section-title--small">对战日志列表</h3>
          </div>
          <span class="selected-pill">共 {{ logs.length }} 条</span>
        </div>
        <div class="table-wrap">
          <table class="data-table">
            <thead>
              <tr>
                <th>挑战方</th>
                <th>被挑战方</th>
                <th>积分变化</th>
                <th>胜者</th>
                <th>时间</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="log in logs" :key="log.id">
                <td>{{ log.attackerName }} ({{ log.attackerPointsBefore }}→{{ log.attackerPointsAfter }})</td>
                <td>{{ log.defenderName }} ({{ log.defenderPointsBefore }}→{{ log.defenderPointsAfter }})</td>
                <td>{{ log.attackerPointsAfter - log.attackerPointsBefore }}</td>
                <td>{{ log.winnerId === log.attackerId ? log.attackerName : log.defenderName }}</td>
                <td>{{ formatDateTime(log.createdAt) }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </template>

    <!-- Tab 3: 赛季管理 -->
    <template v-if="activeTab === 'season'">
      <section class="panel-box filter-panel">
        <div class="section-header-copy">
          <p class="section-kicker">赛季配置</p>
          <h3 class="section-title section-title--small">赛季信息</h3>
        </div>
        <div class="detail-overview" v-if="seasonInfo">
          <article class="detail-overview__item"><span>当前赛季</span><strong>#{{ seasonInfo.seasonNumber }}</strong></article>
          <article class="detail-overview__item"><span>赛季状态</span><strong :style="{ color: seasonInfo.seasonEnabled ? '#22c55e' : '#ef4444' }">{{ seasonInfo.seasonEnabled ? '已启用' : '未启用' }}</strong></article>
          <article class="detail-overview__item"><span>赛季时长</span><strong>{{ seasonInfo.seasonDuration }} 天</strong></article>
          <article class="detail-overview__item"><span>开始时间</span><strong>{{ formatDateTime(seasonInfo.seasonStartTime) }}</strong></article>
          <article class="detail-overview__item"><span>结束时间</span><strong>{{ formatDateTime(seasonInfo.seasonEndTime) }}</strong></article>
          <article class="detail-overview__item"><span>剩余天数</span><strong>{{ seasonInfo.daysRemaining }} 天</strong></article>
          <article class="detail-overview__item"><span>参与人数</span><strong>{{ seasonInfo.totalPlayers }}</strong></article>
        </div>

        <div class="filter-grid" style="margin-top: 1rem;">
          <label class="field">
            <span>赛季时长（天）</span>
            <input v-model.number="editSeasonDuration" type="number" min="1" max="365" />
          </label>
          <label class="field">
            <span>启用赛季</span>
            <select v-model="editSeasonEnabled">
              <option :value="true">启用</option>
              <option :value="false">禁用</option>
            </select>
          </label>
          <div class="filter-actions">
            <button class="secondary-button" type="button" @click="handleAdjustSeason">保存配置</button>
            <button class="primary-button" type="button" style="margin-left:8px;" @click="handleSettleSeason">结算赛季</button>
            <button class="secondary-button" type="button" style="margin-left:8px;color:#ef4444;" @click="handleResetSeason">重置赛季</button>
          </div>
        </div>
      </section>

      <section class="panel-box table-panel">
        <div class="section-header-row">
          <div class="section-header-copy">
            <p class="section-kicker">奖励配置</p>
            <h3 class="section-title section-title--small">赛季奖励</h3>
          </div>
          <button class="primary-button" type="button" @click="openRewardEditor(null)">新增奖励</button>
        </div>
        <div class="table-wrap">
          <table class="data-table">
            <thead>
              <tr>
                <th>排名范围</th>
                <th>奖励名称</th>
                <th>金币</th>
                <th>灵石</th>
                <th>称号</th>
                <th>操作</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="r in seasonRewards" :key="r.gid">
                <td>第{{ r.minRank }}-{{ r.maxRank }}名</td>
                <td>{{ r.rewardTitle }}</td>
                <td>{{ r.gold }}</td>
                <td>{{ r.spiritStone }}</td>
                <td>{{ r.title || '-' }}</td>
                <td>
                  <button class="secondary-button secondary-button--sm" type="button" @click="openRewardEditor(r)">编辑</button>
                  <button class="secondary-button secondary-button--sm" type="button" style="margin-left:4px;color:#ef4444;" @click="handleDeleteReward(r)">删除</button>
                </td>
              </tr>
              <tr v-if="seasonRewards.length === 0">
                <td colspan="6" style="text-align:center;color:#94a3b8;">暂无奖励配置</td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>

      <!-- 结算结果弹窗 -->
      <AdminModal
        v-model="settleResultOpen"
        kicker="赛季结算"
        title="结算结果"
        description="赛季结算已完成。"
      >
        <div v-if="settleResult" style="white-space: pre-line;">{{ settleResult.messages.join('\n') }}</div>
        <template #footer>
          <button class="primary-button" type="button" @click="settleResultOpen = false">确定</button>
        </template>
      </AdminModal>

      <!-- 奖励编辑弹窗 -->
      <AdminModal
        v-model="rewardEditorOpen"
        kicker="奖励编辑"
        :title="rewardEditorGid ? '编辑奖励' : '新增奖励'"
        description="配置排名范围和奖励内容。"
      >
        <div class="editor-grid">
          <label class="field">
            <span>最小排名</span>
            <input v-model.number="rewardForm.minRank" type="number" min="1" />
          </label>
          <label class="field">
            <span>最大排名</span>
            <input v-model.number="rewardForm.maxRank" type="number" min="1" />
          </label>
          <label class="field">
            <span>奖励名称</span>
            <input v-model.trim="rewardForm.rewardTitle" type="text" placeholder="如：赛季冠军" />
          </label>
          <label class="field">
            <span>金币</span>
            <input v-model.number="rewardForm.gold" type="number" min="0" />
          </label>
          <label class="field">
            <span>灵石</span>
            <input v-model.number="rewardForm.spiritStone" type="number" min="0" />
          </label>
          <label class="field">
            <span>称号</span>
            <input v-model.trim="rewardForm.title" type="text" placeholder="可选" />
          </label>
        </div>
        <template #footer>
          <button class="secondary-button" type="button" @click="rewardEditorOpen = false">取消</button>
          <button class="primary-button" type="button" @click="handleSaveReward">保存</button>
        </template>
      </AdminModal>
    </template>

    <!-- 调分弹窗 -->
    <AdminModal
      v-model="adjustModalOpen"
      kicker="积分调整"
      :title="adjustPlayer ? `调整 ${adjustPlayer.playerName} 的积分` : '调整积分'"
      description="手动设置玩家的竞技场积分。"
    >
      <div v-if="adjustPlayer" class="editor-grid">
        <label class="field">
          <span>当前积分</span>
          <input :value="String(adjustPlayer.points)" type="text" disabled />
        </label>
        <label class="field">
          <span>新积分</span>
          <input v-model.number="adjustNewPoints" type="number" min="0" />
        </label>
      </div>
      <template #footer>
        <button class="secondary-button" type="button" @click="adjustModalOpen = false">取消</button>
        <button class="primary-button" type="button" @click="saveAdjustPoints">保存</button>
      </template>
    </AdminModal>

    <!-- 禁赛弹窗 -->
    <AdminModal
      v-model="banModalOpen"
      kicker="禁赛操作"
      :title="banPlayer ? `禁赛 ${banPlayer.playerName}` : '禁赛玩家'"
      description="设置禁赛时长和原因。"
    >
      <div v-if="banPlayer" class="editor-grid">
        <label class="field">
          <span>玩家</span>
          <input :value="`${banPlayer.playerName} (${banPlayer.playerId})`" type="text" disabled />
        </label>
        <label class="field">
          <span>禁赛时长（小时）</span>
          <input v-model.number="banHours" type="number" min="1" max="720" />
        </label>
        <label class="field">
          <span>禁赛原因</span>
          <input v-model.trim="banReason" type="text" placeholder="请输入禁赛原因" />
        </label>
      </div>
      <template #footer>
        <button class="secondary-button" type="button" @click="banModalOpen = false">取消</button>
        <button class="primary-button" type="button" @click="saveBan">确认禁赛</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import toast from '@/utils/toast'
import {
  getArenaOverview, getArenaPlayers, getArenaBattleLogs,
  adjustArenaPoints, banArenaPlayer, unbanArenaPlayer,
  getArenaSeasonInfo, settleArenaSeason, resetArenaSeason,
  adjustArenaSeason, getArenaSeasonRewards, saveArenaSeasonReward, deleteArenaSeasonReward
} from '@/services/arena'
import type {
  AdminArenaOverview, AdminArenaPlayer, AdminArenaBattleLog,
  AdminArenaSeason, AdminArenaSeasonReward, AdminSettleSeasonResult
} from '@/types/admin'

const TAB_OPTIONS = [
  { value: 'players', label: '玩家列表' },
  { value: 'logs', label: '对战日志' },
  { value: 'season', label: '赛季管理' }
] as const

type TabValue = (typeof TAB_OPTIONS)[number]['value']

const activeTab = ref<TabValue>('players')
const overview = ref<AdminArenaOverview | null>(null)

// ===== 玩家列表 =====
const playerKeyword = ref('')
const players = ref<AdminArenaPlayer[]>([])

// ===== 对战日志 =====
const logPlayerId = ref('')
const logs = ref<AdminArenaBattleLog[]>([])

// ===== 调分 =====
const adjustModalOpen = ref(false)
const adjustPlayer = ref<AdminArenaPlayer | null>(null)
const adjustNewPoints = ref(0)

// ===== 禁赛 =====
const banModalOpen = ref(false)
const banPlayer = ref<AdminArenaPlayer | null>(null)
const banHours = ref(24)
const banReason = ref('')

// ===== 赛季管理 =====
const seasonInfo = ref<AdminArenaSeason | null>(null)
const seasonRewards = ref<AdminArenaSeasonReward[]>([])
const editSeasonDuration = ref(30)
const editSeasonEnabled = ref(true)
const settleResultOpen = ref(false)
const settleResult = ref<AdminSettleSeasonResult | null>(null)

// ===== 奖励编辑 =====
const rewardEditorOpen = ref(false)
const rewardEditorGid = ref<string | null>(null)
const rewardForm = ref({ minRank: 1, maxRank: 1, rewardTitle: '', gold: 0, spiritStone: 0, title: '' })

function formatDateTime(value?: string | null) {
  if (!value) return '-'
  return value.replace('T', ' ').slice(0, 19)
}

async function loadOverview() {
  overview.value = await getArenaOverview()
}

async function loadPlayers() {
  players.value = await getArenaPlayers(playerKeyword.value)
}

async function loadLogs() {
  logs.value = await getArenaBattleLogs(logPlayerId.value)
}

async function loadSeasonInfo() {
  seasonInfo.value = await getArenaSeasonInfo()
  editSeasonDuration.value = seasonInfo.value.seasonDuration
  editSeasonEnabled.value = seasonInfo.value.seasonEnabled
}

async function loadSeasonRewards() {
  seasonRewards.value = await getArenaSeasonRewards()
}

function openAdjustPoints(player: AdminArenaPlayer) {
  adjustPlayer.value = player
  adjustNewPoints.value = player.points
  adjustModalOpen.value = true
}

async function saveAdjustPoints() {
  if (!adjustPlayer.value) return
  try {
    await adjustArenaPoints(adjustPlayer.value.playerId, adjustNewPoints.value)
    toast.success('积分已调整。')
    adjustModalOpen.value = false
    await Promise.all([loadPlayers(), loadOverview()])
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '调整失败。')
  }
}

function openBan(player: AdminArenaPlayer) {
  banPlayer.value = player
  banHours.value = 24
  banReason.value = ''
  banModalOpen.value = true
}

async function saveBan() {
  if (!banPlayer.value) return
  try {
    await banArenaPlayer(banPlayer.value.playerId, banHours.value, banReason.value)
    toast.success('已禁赛。')
    banModalOpen.value = false
    await loadPlayers()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '禁赛失败。')
  }
}

async function handleUnban(player: AdminArenaPlayer) {
  try {
    await unbanArenaPlayer(player.playerId)
    toast.success('已解禁。')
    await loadPlayers()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '解禁失败。')
  }
}

function isBanned(player: AdminArenaPlayer): boolean {
  return !!player.bannedUntil && new Date(player.bannedUntil) > new Date()
}

async function handleAdjustSeason() {
  try {
    await adjustArenaSeason(editSeasonDuration.value, editSeasonEnabled.value)
    toast.success('赛季配置已更新。')
    await loadSeasonInfo()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '更新失败。')
  }
}

async function handleSettleSeason() {
  if (!confirm('确定要结算当前赛季吗？将发放奖励并开始新赛季。')) return
  try {
    settleResult.value = await settleArenaSeason()
    settleResultOpen.value = true
    toast.success(`第${settleResult.value.seasonNumber}赛季结算完成。`)
    await Promise.all([loadOverview(), loadSeasonInfo(), loadPlayers()])
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '结算失败。')
  }
}

async function handleResetSeason() {
  if (!confirm('确定要重置赛季吗？所有玩家数据将被清空，赛季从1开始。此操作不可撤销！')) return
  try {
    await resetArenaSeason()
    toast.success('赛季已重置。')
    await Promise.all([loadOverview(), loadSeasonInfo(), loadPlayers()])
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '重置失败。')
  }
}

function openRewardEditor(reward: AdminArenaSeasonReward | null) {
  if (reward) {
    rewardEditorGid.value = reward.gid
    rewardForm.value = {
      minRank: reward.minRank,
      maxRank: reward.maxRank,
      rewardTitle: reward.rewardTitle,
      gold: reward.gold,
      spiritStone: reward.spiritStone,
      title: reward.title || ''
    }
  } else {
    rewardEditorGid.value = null
    rewardForm.value = { minRank: 1, maxRank: 3, rewardTitle: '', gold: 0, spiritStone: 0, title: '' }
  }
  rewardEditorOpen.value = true
}

async function handleSaveReward() {
  try {
    await saveArenaSeasonReward({
      gid: rewardEditorGid.value || undefined,
      ...rewardForm.value
    })
    toast.success('奖励已保存。')
    rewardEditorOpen.value = false
    await loadSeasonRewards()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '保存失败。')
  }
}

async function handleDeleteReward(reward: AdminArenaSeasonReward) {
  if (!confirm(`确定删除「${reward.rewardTitle}」(第${reward.minRank}-${reward.maxRank}名)？`)) return
  try {
    await deleteArenaSeasonReward(reward.gid)
    toast.success('奖励已删除。')
    await loadSeasonRewards()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '删除失败。')
  }
}

onMounted(async () => {
  await Promise.all([loadOverview(), loadPlayers(), loadSeasonInfo(), loadSeasonRewards()])
})
</script>

<style scoped>
.tab-bar {
  display: flex;
  gap: 0.5rem;
  margin-top: 1rem;
  flex-wrap: wrap;
}

.tab-button {
  padding: 0.5rem 1rem;
  border: 1px solid var(--border-color, #e2e8f0);
  border-radius: 6px;
  background: transparent;
  cursor: pointer;
  font-size: 0.875rem;
  color: var(--text-secondary, #64748b);
  transition: all 0.15s ease;
}

.tab-button.is-active {
  background: var(--primary-color, #3b82f6);
  color: #fff;
  border-color: var(--primary-color, #3b82f6);
}

.tab-button:hover:not(.is-active) {
  background: var(--bg-hover, #f1f5f9);
}

.secondary-button--sm {
  padding: 0.25rem 0.5rem;
  font-size: 0.75rem;
}
</style>
