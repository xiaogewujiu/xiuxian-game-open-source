<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">抽奖系统</p>
        <h2 class="section-title section-title--small">抽奖池管理</h2>
        <p class="section-note">管理抽奖池和奖项配置。概率使用万分比（10000=100%），启用奖项概率总和不能超过10000。</p>
      </div>
      <div class="filter-grid">
        <label class="field"><span>关键字</span><input v-model.trim="keyword" type="text" placeholder="搜索抽奖池名称" /></label>
        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadPools">查询</button>
          <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="openCreatePool">新建抽奖池</button>
        </div>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">抽奖池列表</p>
          <h3 class="section-title section-title--small">抽奖池列表</h3>
        </div>
        <span class="selected-pill">共 {{ pools.length }} 条</span>
      </div>
      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>池ID</th>
              <th>名称</th>
              <th>抽奖类型</th>
              <th>来源</th>
              <th>状态</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="p in pools" :key="p.poolId" :class="{ 'is-active': detailPool?.poolId === p.poolId && detailOpen }" @click="openDetail(p.poolId)">
              <td>{{ p.poolId }}</td>
              <td>{{ p.name }}</td>
              <td>{{ getLotteryTypeLabel(p.lotteryType) }}</td>
              <td>{{ p.isBuiltIn ? '内置' : '人工' }}</td>
              <td><span :class="p.isEnabled ? 'tag-active' : 'tag-inactive'">{{ p.isEnabled ? '启用' : '停用' }}</span></td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>

    <!-- 抽奖池详情/编辑弹窗（含基本信息和奖项列表） -->
    <AdminModal v-model="detailOpen" kicker="抽奖池详情" :title="detailCreateMode ? '新建抽奖池' : `抽奖池：${detailPool?.name || ''}`" :description="detailPool ? (detailCreateMode ? '维护抽奖池的消耗、次数限制和时间范围。' : `${getLotteryTypeLabel(detailPool.lotteryType)} · 消耗 ${detailPool.costAmount} ${getCostTypeLabel(detailPool.costType)}`) : ''" size="xwide">
      <div v-if="detailPool" class="detail-section">
        <div class="detail-overview" v-if="!detailCreateMode">
          <article class="detail-overview__item"><span>来源</span><strong>{{ detailPool.isBuiltIn ? '内置' : '人工维护' }}</strong></article>
          <article class="detail-overview__item"><span>最后更新</span><strong>{{ formatDateTime(detailPool.lastUpdateTime) }}</strong></article>
        </div>
        <div class="editor-grid editor-grid--three">
          <label class="field"><span>池ID</span><input v-model.trim="detailPool.poolId" type="text" :disabled="!detailCreateMode || !canManageConfigs" /></label>
          <label class="field"><span>名称</span><input v-model.trim="detailPool.name" type="text" :disabled="!canManageConfigs" /></label>
          <label class="field"><span>抽奖类型</span><select v-model.number="detailPool.lotteryType" :disabled="!canManageConfigs"><option v-for="opt in LOTTERY_TYPE_OPTIONS" :key="opt.value" :value="opt.value">{{ opt.label }}</option></select></label>
          <label class="field"><span>消耗类型</span><select v-model.number="detailPool.costType" :disabled="!canManageConfigs"><option v-for="opt in LOTTERY_COST_TYPE_OPTIONS" :key="opt.value" :value="opt.value">{{ opt.label }}</option></select></label>
          <label class="field"><span>消耗道具</span><select v-model="detailPool.costItemId" :disabled="detailPool.costType !== 2 || !canManageConfigs"><option value="">请选择道具</option><option v-for="item in itemList" :key="item.itemId" :value="item.itemId">{{ item.name }} ({{ item.itemId }})</option></select></label>
          <label class="field"><span>消耗数量</span><input v-model.number="detailPool.costAmount" type="number" min="1" :disabled="!canManageConfigs" /></label>
          <label class="field"><span>每日限制</span><input v-model.number="detailPool.dailyLimit" type="number" min="-1" :disabled="!canManageConfigs" /></label>
          <label class="field"><span>总次数限制</span><input v-model.number="detailPool.totalLimit" type="number" min="-1" :disabled="!canManageConfigs" /></label>
          <label class="field"><span>开始时间</span><input v-model="detailPool.startTime" type="datetime-local" :disabled="!canManageConfigs" /></label>
          <label class="field"><span>结束时间</span><input v-model="detailPool.endTime" type="datetime-local" :disabled="!canManageConfigs" /></label>
        </div>
        <div class="editor-grid">
          <label class="field checkbox-field"><input v-model="detailPool.supportSingle" type="checkbox" :disabled="!canManageConfigs" /><span>支持单抽</span></label>
          <label class="field checkbox-field"><input v-model="detailPool.supportTen" type="checkbox" :disabled="!canManageConfigs" /><span>支持十连抽</span></label>
          <label class="field checkbox-field"><input v-model="detailPool.isEnabled" type="checkbox" :disabled="!canManageConfigs" /><span>启用抽奖池</span></label>
        </div>

        <!-- 奖项子表 -->
        <div class="sub-section">
          <div class="sub-section-header">
            <h4 class="sub-section-title">奖项配置 <span class="count-badge">{{ detailPrizes.length }}</span></h4>
            <div class="sub-section-actions">
              <span class="prob-sum" :class="{ 'prob-over': totalProbability > 10000 }">概率总和: {{ totalProbability }}/10000</span>
              <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="openCreatePrize">新建奖项</button>
            </div>
          </div>
          <div class="table-wrap">
            <table class="data-table data-table--compact">
              <thead>
                <tr>
                  <th>奖励类型</th>
                  <th>目标</th>
                  <th>数量</th>
                  <th>概率(‱)</th>
                  <th>来源</th>
                  <th>状态</th>
                  <th>操作</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="p in detailPrizes" :key="p.prizeId">
                  <td>{{ getRewardTypeLabel(p.rewardType) }}</td>
                  <td>{{ getRewardTargetLabel(p) }}</td>
                  <td><input class="inline-input" type="number" min="1" v-model.number="p.rewardAmount" :disabled="!canManageConfigs" @input="markPrizeDirty(p)" /></td>
                  <td><input class="inline-input" type="number" min="0" max="10000" v-model.number="p.probability" :disabled="!canManageConfigs" @input="markPrizeDirty(p)" /></td>
                  <td>{{ p.isBuiltIn ? '内置' : '人工' }}</td>
                  <td><span :class="p.isEnabled ? 'tag-active' : 'tag-inactive'">{{ p.isEnabled ? '启用' : '停用' }}</span></td>
                  <td class="col-actions">
                    <button class="link-button" type="button" :disabled="!canManageConfigs" @click="openEditPrize(p.prizeId)">编辑</button>
                    <button class="link-button danger" type="button" :disabled="!canManageConfigs" @click="removePrize(p.prizeId)">删除</button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
      <template #footer>
        <button class="secondary-button" type="button" @click="detailOpen = false">取消</button>
        <button class="danger-button" type="button" :disabled="!canManageConfigs || detailCreateMode" @click="removePool(detailPool?.poolId || '')">删除</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="saveDetailPool">保存抽奖池</button>
      </template>
    </AdminModal>

    <!-- 奖项编辑弹窗 -->
    <AdminModal v-model="prizeEditorOpen" kicker="抽奖奖项" :title="prizeEditorMode === 'create' ? '新建奖项' : '编辑奖项'" description="配置奖项的奖励内容和概率。" size="xwide">
      <fieldset v-if="editingPrize" class="form-fieldset" :disabled="!canManageConfigs">
        <div class="editor-grid editor-grid--three">
          <label class="field"><span>奖励类型</span><select v-model.number="editingPrize.rewardType" @change="onRewardTypeChange"><option v-for="opt in LOTTERY_REWARD_TYPE_OPTIONS" :key="opt.value" :value="opt.value">{{ opt.label }}</option></select></label>
          <label v-if="editingPrize.rewardType === 2" class="field"><span>道具</span>
            <select v-model="editingPrize.rewardTargetId">
              <option value="">请选择道具</option>
              <option v-for="item in itemList" :key="item.itemId" :value="item.itemId">{{ item.name }} ({{ item.itemId }})</option>
            </select>
          </label>
          <label v-else-if="editingPrize.rewardType === 3" class="field"><span>文字图鉴系列</span>
            <select v-model="editingPrize.rewardTargetId">
              <option value="">请选择系列</option>
              <option v-for="s in textSeriesList" :key="s.seriesId" :value="s.seriesId">{{ s.name }} ({{ s.seriesId }})</option>
            </select>
          </label>
          <label v-else-if="editingPrize.rewardType === 4" class="field"><span>图片图鉴系列</span>
            <select v-model="editingPrize.rewardTargetId">
              <option value="">请选择系列</option>
              <option v-for="s in imageSeriesList" :key="s.seriesId" :value="s.seriesId">{{ s.name }} ({{ s.seriesId }})</option>
            </select>
          </label>
          <label class="field"><span>奖励数量</span><input v-model.number="editingPrize.rewardAmount" type="number" min="1" /></label>
          <label class="field"><span>概率(万分比)</span><input v-model.number="editingPrize.probability" type="number" min="0" max="10000" /></label>
        </div>
        <label class="field checkbox-field"><input v-model="editingPrize.isEnabled" type="checkbox" /><span>启用奖项</span></label>
      </fieldset>
      <template #footer>
        <button class="secondary-button" type="button" @click="prizeEditorOpen = false">取消</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="savePrize">保存</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import { LOTTERY_TYPE_OPTIONS, LOTTERY_COST_TYPE_OPTIONS, LOTTERY_REWARD_TYPE_OPTIONS } from '@/constants/game-options'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import {
  getAdminLotteryPools, getAdminLotteryPoolDetail, saveAdminLotteryPool, deleteAdminLotteryPool,
  getAdminLotteryPrizes, getAdminLotteryPrizeDetail, saveAdminLotteryPrize, deleteAdminLotteryPrize
} from '@/services/collections'
import { getAdminItems } from '@/services/items'
import { getAdminTextCollectionSeries, getAdminImageCollectionSeries } from '@/services/collections'
import toast from '@/utils/toast'
import type {
  AdminLotteryPoolListItem, AdminLotteryPoolDetail,
  AdminLotteryPrizeListItem, AdminLotteryPrizeDetail,
  AdminItemListItem, AdminTextCollectionSeriesListItem, AdminImageCollectionSeriesListItem
} from '@/types/admin'

const { canManageConfigs } = useAdminPermissions()
const keyword = ref('')
const pools = ref<AdminLotteryPoolListItem[]>([])

// 抽奖池详情/编辑
const detailOpen = ref(false)
const detailPool = ref<AdminLotteryPoolDetail | null>(null)
const detailCreateMode = ref(false)
const detailPrizes = ref<AdminLotteryPrizeListItem[]>([])

// 奖项编辑
const prizeEditorOpen = ref(false)
const prizeEditorMode = ref<'create' | 'edit'>('create')
const editingPrize = ref<AdminLotteryPrizeDetail | null>(null)
const dirtyPrizes = new Set<string>()
function markPrizeDirty(p: AdminLotteryPrizeListItem) { dirtyPrizes.add(p.prizeId) }

// 下拉数据
const itemList = ref<AdminItemListItem[]>([])
const textSeriesList = ref<AdminTextCollectionSeriesListItem[]>([])
const imageSeriesList = ref<AdminImageCollectionSeriesListItem[]>([])

const totalProbability = computed(() => detailPrizes.value.filter(p => p.isEnabled).reduce((sum, p) => sum + p.probability, 0))

function formatDateTime(value?: string | null) { return value ? value.replace('T', ' ').slice(0, 19) : '-' }
function getLotteryTypeLabel(v: number) { return LOTTERY_TYPE_OPTIONS.find(o => o.value === v)?.label || `类型${v}` }
function getCostTypeLabel(v: number) { return LOTTERY_COST_TYPE_OPTIONS.find(o => o.value === v)?.label || '' }
function getRewardTypeLabel(v: number) { return LOTTERY_REWARD_TYPE_OPTIONS.find(o => o.value === v)?.label || `类型${v}` }

function getRewardTargetLabel(p: AdminLotteryPrizeListItem) {
  if (p.rewardType === 0 || p.rewardType === 1 || p.rewardType === 5) return '-'
  if (p.rewardType === 2) {
    const item = itemList.value.find(i => i.itemId === p.rewardTargetId)
    return item ? item.name : (p.rewardTargetId || '-')
  }
  if (p.rewardType === 3) {
    const s = textSeriesList.value.find(s => s.seriesId === p.rewardTargetId)
    return s ? s.name : (p.rewardTargetId || '-')
  }
  if (p.rewardType === 4) {
    const s = imageSeriesList.value.find(s => s.seriesId === p.rewardTargetId)
    return s ? s.name : (p.rewardTargetId || '-')
  }
  return p.rewardTargetId || '-'
}

function genId(prefix: string) { return `${prefix}_${Date.now().toString(36)}_${Math.random().toString(36).slice(2, 6)}` }

function createEmptyPool(): AdminLotteryPoolDetail {
  return { poolId: '', name: '', lotteryType: 0, costType: 0, costItemId: null, costAmount: 100, isEnabled: true, startTime: null, endTime: null, supportSingle: true, supportTen: false, dailyLimit: -1, totalLimit: -1, sortOrder: 0, isBuiltIn: false, seedKey: null, builtInVersion: null, lastUpdateTime: null }
}
function createEmptyPrize(): AdminLotteryPrizeDetail {
  return { prizeId: genId('prize'), poolId: detailPool.value?.poolId || '', rewardType: 0, rewardTargetId: null, rewardAmount: 1, probability: 0, sortOrder: 0, isEnabled: true, isBuiltIn: false, seedKey: null, builtInVersion: null, lastUpdateTime: null }
}

async function loadPools() { pools.value = await getAdminLotteryPools(keyword.value) }

async function loadDropdownData() {
  const [items, textSeries, imageSeries] = await Promise.all([
    getAdminItems(),
    getAdminTextCollectionSeries(),
    getAdminImageCollectionSeries()
  ])
  itemList.value = items
  textSeriesList.value = textSeries
  imageSeriesList.value = imageSeries
}

// --- 抽奖池 CRUD ---
function openCreatePool() {
  if (!canManageConfigs.value) return
  detailPool.value = createEmptyPool()
  detailPrizes.value = []
  detailCreateMode.value = true
  dirtyPrizes.clear()
  detailOpen.value = true
}

async function saveDetailPool() {
  if (!canManageConfigs.value || !detailPool.value) return
  if (!detailPool.value.poolId.trim()) { toast.error('抽奖池ID不能为空。'); return }
  if (!detailPool.value.name.trim()) { toast.error('抽奖池名称不能为空。'); return }
  if (detailPool.value.costAmount <= 0) { toast.error('消耗数量必须大于0。'); return }
  const enabledTotal = detailPrizes.value.filter(p => p.isEnabled).reduce((sum, p) => sum + p.probability, 0)
  if (enabledTotal > 10000) { toast.error(`启用奖项概率总和超过10000（当前：${enabledTotal}）。`); return }
  try {
    // 保存脏奖项
    for (const prizeId of dirtyPrizes) {
      const prize = detailPrizes.value.find(p => p.prizeId === prizeId)
      if (prize) await saveAdminLotteryPrize({ ...prize, sortOrder: 0, seedKey: null, lastUpdateTime: null } as AdminLotteryPrizeDetail)
    }
    dirtyPrizes.clear()
    await saveAdminLotteryPool(detailPool.value)
    toast.success('保存成功。')
    if (detailCreateMode.value) {
      detailCreateMode.value = false
    }
    detailOpen.value = false; await loadPools()
  } catch (e) { toast.error(e instanceof Error ? e.message : '保存失败。') }
}

async function removePool(poolId: string) {
  if (!canManageConfigs.value || !poolId) return
  if (!window.confirm(`确认删除抽奖池 ${poolId} 吗？`)) return
  try {
    await deleteAdminLotteryPool(poolId)
    toast.success('删除成功。')
    detailOpen.value = false
    await loadPools()
  } catch (e) { toast.error(e instanceof Error ? e.message : '删除失败。') }
}

// --- 抽奖池详情 ---
async function openDetail(poolId: string) {
  detailPool.value = await getAdminLotteryPoolDetail(poolId)
  detailPrizes.value = await getAdminLotteryPrizes(poolId)
  detailCreateMode.value = false
  dirtyPrizes.clear()
  detailOpen.value = true
}

async function reloadDetail() {
  if (!detailPool.value) return
  detailPrizes.value = await getAdminLotteryPrizes(detailPool.value.poolId)
}

// --- 奖项 CRUD ---
function openCreatePrize() {
  if (!canManageConfigs.value || !detailPool.value) return
  editingPrize.value = createEmptyPrize()
  prizeEditorMode.value = 'create'
  prizeEditorOpen.value = true
}

async function openEditPrize(prizeId: string) {
  editingPrize.value = await getAdminLotteryPrizeDetail(prizeId)
  prizeEditorMode.value = 'edit'
  prizeEditorOpen.value = true
}

function onRewardTypeChange() {
  if (!editingPrize.value) return
  // 金币/灵石/谢谢惠顾不需要目标
  if (editingPrize.value.rewardType === 0 || editingPrize.value.rewardType === 1 || editingPrize.value.rewardType === 5) {
    editingPrize.value.rewardTargetId = null
  } else {
    editingPrize.value.rewardTargetId = ''
  }
}

async function savePrize() {
  if (!canManageConfigs.value || !editingPrize.value) return
  if (editingPrize.value.probability < 0) { toast.error('概率不能为负数。'); return }
  if ([2, 3, 4].includes(editingPrize.value.rewardType) && !editingPrize.value.rewardTargetId) {
    toast.error('请选择目标。'); return
  }
  // 前端校验概率总和
  if (editingPrize.value.isEnabled) {
    const otherSum = detailPrizes.value.filter(p => p.isEnabled && p.prizeId !== editingPrize.value!.prizeId).reduce((s, p) => s + p.probability, 0)
    if (otherSum + editingPrize.value.probability > 10000) {
      toast.error(`启用奖项概率总和将超过10000（其他：${otherSum}，本奖项：${editingPrize.value.probability}）。`); return
    }
  }
  try {
    await saveAdminLotteryPrize(editingPrize.value)
    toast.success('保存成功。'); prizeEditorOpen.value = false; await reloadDetail()
  } catch (e) { toast.error(e instanceof Error ? e.message : '保存失败。') }
}

async function removePrize(prizeId: string) {
  if (!canManageConfigs.value) return
  if (!window.confirm('确认删除此奖项吗？')) return
  try {
    await deleteAdminLotteryPrize(prizeId)
    toast.success('删除成功。'); await reloadDetail()
  } catch (e) { toast.error(e instanceof Error ? e.message : '删除失败。') }
}

onMounted(() => { loadPools(); loadDropdownData() })
</script>

<style scoped>
.col-actions { white-space: nowrap; }
.col-actions .link-button { margin-right: 8px; }
.col-actions .link-button:last-child { margin-right: 0; }

.tag-active { color: var(--color-success, #22c55e); font-weight: 600; font-size: 12px; }
.tag-inactive { color: var(--color-muted, #888); font-weight: 600; font-size: 12px; }

.link-button { background: none; border: none; color: var(--color-primary, #6366f1); cursor: pointer; font-size: 13px; padding: 2px 4px; text-decoration: underline; }
.link-button:hover { opacity: 0.8; }
.link-button:disabled { opacity: 0.4; cursor: not-allowed; }
.link-button.danger { color: var(--color-danger, #ef4444); }

.detail-section { display: flex; flex-direction: column; gap: 24px; }

.sub-section { border: 1px solid var(--border-color, #e5e7eb); border-radius: 8px; padding: 16px; }
.sub-section-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 12px; }
.sub-section-title { font-size: 15px; font-weight: 600; margin: 0; display: flex; align-items: center; gap: 8px; }
.sub-section-actions { display: flex; align-items: center; gap: 12px; }
.count-badge { font-size: 12px; background: var(--color-bg-muted, #f3f4f6); padding: 2px 8px; border-radius: 10px; font-weight: 500; }
.prob-sum { font-size: 13px; color: #374151; font-weight: 500; }
.prob-over { color: var(--color-danger, #ef4444); font-weight: 700; }

.data-table--compact { font-size: 13px; }
.data-table--compact th, .data-table--compact td { padding: 8px 10px; }

.inline-input { width: 72px; padding: 3px 6px; border: 1px solid var(--border-color, #d1d5db); border-radius: 4px; font-size: 13px; text-align: center; background: var(--color-bg, #fff); }
.inline-input:focus { outline: none; border-color: var(--color-primary, #6366f1); box-shadow: 0 0 0 2px rgba(99,102,241,0.15); }
.inline-input:disabled { opacity: 0.5; cursor: not-allowed; }
</style>
