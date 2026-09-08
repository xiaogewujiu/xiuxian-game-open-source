<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">玩家</p>
        <h2 class="section-title section-title--small">玩家管理</h2>
        <p class="section-note">玩家基础资料、GM 发放和运行态监控统一收口到这一个页面。</p>
      </div>

      <div class="filter-grid">
        <label class="field">
          <span>关键字</span>
          <input v-model.trim="keyword" type="text" placeholder="搜索玩家编号、账号或名称" />
        </label>
        <label class="field">
          <span>当前玩家</span>
          <input :value="selectedPlayer?.playerId || ''" type="text" placeholder="未选择" disabled />
        </label>
        <label class="field">
          <span>运行状态</span>
          <select v-model="battleModeFilter">
            <option value="">全部</option>
            <option value="offline">离线挂机中</option>
            <option value="idle">空闲</option>
          </select>
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
          <h3 class="section-title section-title--small">玩家列表</h3>
        </div>
        <span class="selected-pill">共 {{ filteredPlayers.length }} 条</span>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>玩家编号</th>
              <th>玩家名称</th>
              <th>账号</th>
              <th>等级</th>
              <th>战斗状态</th>
              <th>聚灵阵</th>
              <th>战斗职业</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="player in filteredPlayers"
              :key="player.playerId"
              :class="{ 'is-active': selectedPlayer?.playerId === player.playerId }"
              @click="openEditModal(player.playerId)"
            >
              <td>{{ player.playerId }}</td>
              <td>{{ player.name }}</td>
              <td>{{ player.account }}</td>
              <td>Lv.{{ player.level }}</td>
              <td>{{ player.isOfflineBattling ? `离线 · ${player.offlineBattleMapName || '未知地图'}` : player.battleMode }}</td>
              <td>阵 Lv.{{ player.arrayLevel }} / 田 Lv.{{ player.spiritFieldLevel }}</td>
              <td>{{ player.professionName }}</td>
            </tr>
          </tbody>
        </table>
      </div>

    </section>

    <AdminModal
      v-model="editorOpen"
      kicker="玩家详情"
      :title="selectedPlayer ? `编辑玩家 ${selectedPlayer.name}` : '玩家详情'"
      description="基础资料可编辑，战斗与养成运行态在同一处查看。"
      size="xwide"
    >
      <div v-if="selectedPlayer" class="section-stack section-stack--spaced">
        <div class="detail-overview">
          <article class="detail-overview__item"><span>账号</span><strong>{{ selectedPlayer.account }}</strong></article>
          <article class="detail-overview__item"><span>战斗模式</span><strong>{{ selectedPlayer.isOfflineBattling ? '离线挂机中' : selectedPlayer.battleMode }}</strong></article>
          <article class="detail-overview__item"><span>战斗职业</span><strong>{{ selectedPlayer.professionName }}</strong></article>
          <article class="detail-overview__item"><span>职业上限</span><strong>Lv.{{ selectedPlayer.professionLevelCap }}</strong></article>
          <article class="detail-overview__item"><span>封禁截止</span><strong>{{ formatDateTime(selectedPlayer.banExpiresAt) }}</strong></article>
        </div>

        <fieldset class="form-fieldset" :disabled="!canWritePlayers">
          <div class="editor-grid editor-grid--three">
            <label class="field"><span>玩家编号</span><input v-model="selectedPlayer.playerId" type="text" disabled /></label>
            <label class="field"><span>玩家名称</span><input v-model.trim="selectedPlayer.name" type="text" /></label>
            <label class="field"><span>账号</span><input v-model="selectedPlayer.account" type="text" disabled /></label>
            <label class="field"><span>当前称号</span><input v-model.trim="selectedPlayer.currentTitle" type="text" /></label>
            <label class="field"><span>等级</span><input v-model.number="selectedPlayer.level" type="number" min="1" /></label>
            <label class="field"><span>当前经验</span><input v-model.number="selectedPlayer.exp" type="number" min="0" /></label>
            <label class="field"><span>升级需求经验</span><input v-model.number="selectedPlayer.xExp" type="number" min="1" /></label>
            <label class="field"><span>金币</span><input v-model.number="selectedPlayer.gold" type="number" min="0" /></label>
            <label class="field"><span>荣誉</span><input v-model.number="selectedPlayer.honor" type="number" min="0" /></label>
            <label class="field"><span>公会贡献</span><input v-model.number="selectedPlayer.guildContribution" type="number" min="0" /></label>
            <label class="field"><span>灵石</span><input v-model.number="selectedPlayer.spiritStone" type="number" min="0" /></label>
          </div>
        </fieldset>

        <section class="panel-box panel-box--embedded">
          <div class="section-header-row">
            <div class="section-header-copy">
              <p class="section-kicker">运行时</p>
              <h3 class="section-title section-title--small">战斗与离线挂机</h3>
            </div>
            <span class="selected-pill">{{ selectedPlayer.isOfflineBattling ? '运行中' : '未开启' }}</span>
          </div>
          <div class="detail-overview detail-overview--compact">
            <article class="detail-overview__item"><span>冷却截止</span><strong>{{ formatDateTime(selectedPlayer.battleCooldownUntilUtc) }}</strong></article>
            <article class="detail-overview__item"><span>挂机地图</span><strong>{{ selectedPlayer.offlineBattleMapName || '无' }}</strong></article>
            <article class="detail-overview__item"><span>开始时间</span><strong>{{ formatDateTime(selectedPlayer.offlineBattleStartedAtUtc) }}</strong></article>
            <article class="detail-overview__item"><span>最后推进</span><strong>{{ formatDateTime(selectedPlayer.offlineBattleLastTickAtUtc) }}</strong></article>
            <article class="detail-overview__item"><span>累计场次</span><strong>{{ selectedPlayer.offlineBattleTotalBattles }}</strong></article>
            <article class="detail-overview__item"><span>累计收益</span><strong>{{ selectedPlayer.offlineBattleExpGained }} 经验 / {{ selectedPlayer.offlineBattleGoldGained }} 金币</strong></article>
          </div>
        </section>

        <section class="panel-box panel-box--embedded">
          <div class="section-header-copy">
            <p class="section-kicker">养成</p>
            <h3 class="section-title section-title--small">聚灵阵与职业状态</h3>
          </div>
          <div class="detail-overview detail-overview--compact">
            <article class="detail-overview__item"><span>聚灵阵</span><strong>Lv.{{ selectedPlayer.arrayLevel }}</strong></article>
            <article class="detail-overview__item"><span>五行</span><strong>金{{ selectedPlayer.metalLevel }} 木{{ selectedPlayer.woodLevel }} 水{{ selectedPlayer.waterLevel }} 火{{ selectedPlayer.fireLevel }} 土{{ selectedPlayer.earthLevel }}</strong></article>
            <article class="detail-overview__item"><span>炼丹</span><strong>Lv.{{ selectedPlayer.alchemistLevel }} / {{ selectedPlayer.isAlchemyCrafting ? `进行中 · ${selectedPlayer.activeAlchemyRecipeId || '未知丹方'}` : '空闲' }}</strong></article>
            <article class="detail-overview__item"><span>锻造</span><strong>Lv.{{ selectedPlayer.blacksmithLevel }} / {{ selectedPlayer.isForging ? `进行中 · ${selectedPlayer.activeForgeRecipeId || '未知图纸'}` : '空闲' }}</strong></article>
            <article class="detail-overview__item"><span>灵田</span><strong>Lv.{{ selectedPlayer.spiritFieldLevel }} · {{ selectedPlayer.spiritFieldUnlockedPlots }}/{{ selectedPlayer.spiritFieldMaxPlots }} 地块</strong></article>
            <article class="detail-overview__item"><span>灵田加成</span><strong>+{{ selectedPlayer.spiritFieldGlobalYieldBonus }}%</strong></article>
          </div>
        </section>
      </div>

      <template #footer>
        <button class="secondary-button" type="button" @click="editorOpen = false">取消</button>
        <button class="secondary-button" type="button" :disabled="!selectedPlayer || !selectedPlayer.isOfflineBattling || !canWritePlayers" @click="stopOfflineBattle">停止离线</button>
        <button class="secondary-button" type="button" :disabled="!selectedPlayer || !canGrantPlayers" @click="openCurrencyModal">发放货币</button>
        <button class="secondary-button" type="button" :disabled="!selectedPlayer || !canGrantPlayers" @click="openItemModal">发放道具</button>
        <button class="secondary-button" type="button" :disabled="!selectedPlayer || !canWritePlayers" @click="openBanModal">封禁管理</button>
        <button class="primary-button" type="button" :disabled="!canWritePlayers" @click="savePlayer">保存玩家</button>
      </template>
    </AdminModal>

    <AdminModal
      v-model="currencyModalOpen"
      kicker="货币发放"
      title="发放货币 / 经验"
      description="适合活动补偿、人工修复和异常回补。"
    >
      <fieldset class="form-fieldset" :disabled="!canGrantPlayers">
        <div class="editor-grid editor-grid--three">
          <label class="field"><span>资源类型</span><select v-model="grantCurrencyForm.resourceType"><option value="Gold">金币</option><option value="SpiritStone">灵石</option><option value="Honor">荣誉</option><option value="GuildContribution">公会贡献</option><option value="Exp">经验</option></select></label>
          <label class="field"><span>数量</span><input v-model.number="grantCurrencyForm.amount" type="number" min="1" /></label>
          <label class="field"><span>原因</span><input v-model.trim="grantCurrencyForm.reason" type="text" placeholder="例如：活动补偿" /></label>
        </div>
      </fieldset>

      <template #footer>
        <button class="secondary-button" type="button" @click="currencyModalOpen = false">取消</button>
        <button class="primary-button" type="button" :disabled="!canGrantPlayers" @click="grantCurrency">发放</button>
      </template>
    </AdminModal>

    <AdminModal
      v-model="itemModalOpen"
      kicker="道具发放"
      title="发放道具"
      description="用于补发材料、运营道具或工单处理。"
    >
      <fieldset class="form-fieldset" :disabled="!canGrantPlayers">
        <div class="editor-grid editor-grid--three">
          <label class="field"><span>道具编号</span><select v-model="grantItemForm.itemId"><option value="">请选择</option><option v-for="item in itemOptions" :key="item.itemId" :value="item.itemId">{{ item.name }} ({{ item.itemId }})</option></select></label>
          <label class="field"><span>数量</span><input v-model.number="grantItemForm.quantity" type="number" min="1" /></label>
          <label class="field"><span>原因</span><input v-model.trim="grantItemForm.reason" type="text" placeholder="例如：GM补发" /></label>
        </div>
        
      </fieldset>

      <template #footer>
        <button class="secondary-button" type="button" @click="itemModalOpen = false">取消</button>
        <button class="primary-button" type="button" :disabled="!canGrantPlayers" @click="grantItem">发放</button>
      </template>
    </AdminModal>

    <AdminModal
      v-model="banModalOpen"
      kicker="封禁管理"
      title="封禁管理"
      description="高危操作，建议始终填写明确原因。"
    >
      <div v-if="selectedPlayer" class="section-stack">
        <div class="editor-grid">
          <label class="field checkbox-field"><input v-model="selectedPlayer.isBanned" type="checkbox" disabled /><span>当前封禁状态</span></label>
          <label class="field"><span>封禁原因</span><input v-model="selectedPlayer.banReason" type="text" disabled /></label>
          <label class="field"><span>封禁截止</span><input :value="formatDateTime(selectedPlayer.banExpiresAt)" type="text" disabled /></label>
        </div>

        <fieldset class="form-fieldset" :disabled="!canWritePlayers">
          <div class="editor-grid editor-grid--three">
            <label class="field"><span>封禁原因</span><input v-model.trim="banForm.reason" type="text" placeholder="例如：外挂、辱骂、刷资源" /></label>
            <label class="field"><span>封禁时长（小时）</span><input v-model.number="banForm.hours" type="number" min="0" /></label>
          </div>
        </fieldset>
      </div>

      <template #footer>
        <button class="secondary-button" type="button" @click="banModalOpen = false">取消</button>
        <button class="secondary-button" type="button" :disabled="!canWritePlayers" @click="unbanPlayer">解除封禁</button>
        <button class="danger-button" type="button" :disabled="!canWritePlayers" @click="banPlayer">执行封禁</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import { getAdminItems } from '@/services/items'
import {
  banAdminPlayer,
  getAdminPlayerDetail,
  getAdminPlayers,
  grantAdminPlayerCurrency,
  grantAdminPlayerItem,
  saveAdminPlayer,
  stopAdminPlayerOfflineBattle,
  unbanAdminPlayer
} from '@/services/players'
import type { AdminBanPlayerRequest, AdminGrantCurrencyRequest, AdminGrantItemRequest, AdminItemListItem, AdminPlayerDetail, AdminPlayerListItem } from '@/types/admin'
import toast from '@/utils/toast'

// 玩家页权限拆成“写玩家资料”和“发 GM 奖励”两类，
// 按钮是否可用都从这里统一判断。
const { canWritePlayers, canGrantPlayers } = useAdminPermissions()

// 页面主状态：
// players 是列表，selectedPlayer 是当前编辑对象，
// 三个 modal 开关分别对应货币发放、道具发放和封禁管理。
const players = ref<AdminPlayerListItem[]>([])
const itemOptions = ref<AdminItemListItem[]>([])
const keyword = ref('')
const battleModeFilter = ref('')
const selectedPlayer = ref<AdminPlayerDetail | null>(null)
const editorOpen = ref(false)
const currencyModalOpen = ref(false)
const itemModalOpen = ref(false)
const banModalOpen = ref(false)

const grantCurrencyForm = reactive<AdminGrantCurrencyRequest>({ resourceType: 'Gold', amount: 1, reason: '' })
const grantItemForm = reactive<AdminGrantItemRequest>({ itemId: '', quantity: 1, reason: '' })
const banForm = reactive<AdminBanPlayerRequest>({ reason: '', hours: 0 })

// 后端已支持按运行状态筛选。
const filteredPlayers = players

// 统一格式化后台返回的时间字段。
function formatDateTime(value?: string | null) {
  if (!value) return '未设置'
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? String(value) : date.toLocaleString()
}

// 发放道具弹窗的下拉框依赖道具列表，这里单独预加载一次。
async function loadItemOptions() {
  itemOptions.value = await getAdminItems()
}

// 拉取玩家列表。
async function loadPlayers() {
  const filterOffline = battleModeFilter.value === '' ? null : battleModeFilter.value === 'offline'
  players.value = await getAdminPlayers(keyword.value, filterOffline)
}

// 打开玩家详情弹窗，并先读取完整详情。
async function openEditModal(playerId: string) {
  selectedPlayer.value = await getAdminPlayerDetail(playerId)
  editorOpen.value = true
}

// 打开发放货币弹窗。
function openCurrencyModal() {
  currencyModalOpen.value = true
}

// 打开发放道具弹窗。
function openItemModal() {
  itemModalOpen.value = true
}

// 打开封禁管理弹窗。
function openBanModal() {
  banModalOpen.value = true
}

// 保存当前玩家基础资料。
async function savePlayer() {
  if (!canWritePlayers.value || !selectedPlayer.value) return
  if (!window.confirm(`确认保存玩家 ${selectedPlayer.value.name} 的基础资料吗？`)) return

  try {
    selectedPlayer.value = await saveAdminPlayer(selectedPlayer.value)
    toast.success('玩家资料保存成功。')
    editorOpen.value = false
    await loadPlayers()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '玩家资料保存失败。')
  }
}

// 强制停止玩家离线挂机，并回写最新详情。
async function stopOfflineBattle() {
  if (!canWritePlayers.value || !selectedPlayer.value || !selectedPlayer.value.isOfflineBattling) return
  if (!window.confirm(`确认停止玩家 ${selectedPlayer.value.name} 的离线挂机吗？`)) return

  try {
    const summary = await stopAdminPlayerOfflineBattle(selectedPlayer.value.playerId)
    selectedPlayer.value = await getAdminPlayerDetail(selectedPlayer.value.playerId)
    toast.success(`已停止离线挂机：${summary.mapName}，累计 ${summary.totalBattles} 场。`)
    await loadPlayers()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '停止离线挂机失败。')
  }
}

async function grantCurrency() {
  if (!canGrantPlayers.value || !selectedPlayer.value) return
  if (!grantCurrencyForm.resourceType.trim()) {
    toast.error('资源类型不能为空。')
    return
  }
  if (grantCurrencyForm.amount <= 0) {
    toast.error('发放数量必须大于 0。')
    return
  }
  if (!grantCurrencyForm.reason.trim()) {
    toast.error('请填写发放原因，避免审计日志无法追踪。')
    return
  }
  if (!window.confirm(`确认向玩家 ${selectedPlayer.value.name} 发放 ${grantCurrencyForm.amount} ${grantCurrencyForm.resourceType} 吗？`)) return

  try {
    selectedPlayer.value = await grantAdminPlayerCurrency(selectedPlayer.value.playerId, grantCurrencyForm)
    toast.success('资源发放成功。')
    currencyModalOpen.value = false
    await loadPlayers()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '资源发放失败。')
  }
}

async function grantItem() {
  if (!canGrantPlayers.value || !selectedPlayer.value) return
  if (!grantItemForm.itemId.trim()) {
    toast.error('请选择道具。')
    return
  }
  if (grantItemForm.quantity <= 0) {
    toast.error('道具数量必须大于 0。')
    return
  }
  if (!grantItemForm.reason.trim()) {
    toast.error('请填写发放原因，避免审计日志无法追踪。')
    return
  }
  if (!window.confirm(`确认向玩家 ${selectedPlayer.value.name} 发放道具 ${grantItemForm.itemId} x${grantItemForm.quantity} 吗？`)) return

  try {
    await grantAdminPlayerItem(selectedPlayer.value.playerId, grantItemForm)
    toast.success('道具发放成功。')
    itemModalOpen.value = false
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '道具发放失败。')
  }
}

async function banPlayer() {
  if (!canWritePlayers.value || !selectedPlayer.value) return
  if (!banForm.reason.trim()) {
    toast.error('封禁原因不能为空。')
    return
  }
  if (!window.confirm(`确认封禁玩家 ${selectedPlayer.value.name} 吗？该操作会立即影响登录和在线状态。`)) return

  try {
    selectedPlayer.value = await banAdminPlayer(selectedPlayer.value.playerId, banForm)
    toast.success('玩家封禁成功。')
    banModalOpen.value = false
    await loadPlayers()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '玩家封禁失败。')
  }
}

async function unbanPlayer() {
  if (!canWritePlayers.value || !selectedPlayer.value) return
  if (!window.confirm(`确认解除玩家 ${selectedPlayer.value.name} 的封禁状态吗？`)) return

  try {
    selectedPlayer.value = await unbanAdminPlayer(selectedPlayer.value.playerId)
    toast.success('玩家解封成功。')
    banModalOpen.value = false
    await loadPlayers()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '玩家解封失败。')
  }
}

onMounted(async () => {
  await Promise.all([loadItemOptions(), loadPlayers()])
})
</script>
