<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">排行</p>
        <h2 class="section-title section-title--small">排行系统配置</h2>
        <p class="section-note">榜单配置和奖励区间统一使用表格查看，新增和编辑通过弹窗完成。</p>
      </div>

      <div class="filter-grid">
        <label class="field">
          <span>关键字</span>
          <input v-model.trim="keyword" type="text" placeholder="搜索排行编号或名称" />
        </label>
        <label class="field">
          <span>当前榜单</span>
          <input :value="selectedConfig?.rankingId || ''" type="text" placeholder="未选择" disabled />
        </label>
        <label class="field">
          <span>奖励区间</span>
          <input :value="selectedConfig ? String(rewards.length) : ''" type="text" placeholder="0" disabled />
        </label>

        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadConfigs">查询</button>
          <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="openCreateConfigModal">新建排行</button>
        </div>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">榜单列表</p>
          <h3 class="section-title section-title--small">榜单列表</h3>
        </div>
        <span class="selected-pill">共 {{ configs.length }} 条</span>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>排行编号</th>
              <th>排行名称</th>
              <th>排行类型</th>
              <th>来源</th>
              <th>状态</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="config in configs"
              :key="config.rankingId"
              :class="{ 'is-active': selectedConfig?.rankingId === config.rankingId }"
              @click="openEditConfigModal(config.rankingId)"
            >
              <td>{{ config.rankingId }}</td>
              <td>{{ config.rankingName }}</td>
              <td>{{ getRankingTypeLabel(config.rankingType) }}</td>
              <td>{{ config.isBuiltIn ? '内置' : '人工' }}</td>
              <td>{{ config.isEnabled ? '启用' : '停用' }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">奖励列表</p>
          <h3 class="section-title section-title--small">排行奖励</h3>
        </div>
        <div class="detail-actions detail-actions--inline">
          <span class="selected-pill">共 {{ rewards.length }} 条</span>
          <button class="primary-button" type="button" :disabled="!canManageConfigs || !selectedConfig" @click="openCreateRewardModal">新建奖励</button>
        </div>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>奖励编号</th>
              <th>奖励标题</th>
              <th>榜单编号</th>
              <th>名次区间</th>
              <th>来源</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="reward in rewards"
              :key="reward.gid"
              :class="{ 'is-active': selectedReward?.gid === reward.gid }"
              @click="openEditRewardModal(reward.gid)"
            >
              <td>{{ reward.gid }}</td>
              <td>{{ reward.rewardTitle }}</td>
              <td>{{ reward.rankingId }}</td>
              <td>{{ reward.minRank }} - {{ reward.maxRank }}</td>
              <td>{{ reward.isBuiltIn ? '内置' : '人工' }}</td>
            </tr>
          </tbody>
        </table>
      </div>

    </section>

    <AdminModal
      v-model="configEditorOpen"
      kicker="排行配置"
      :title="selectedConfig?.rankingId ? `编辑榜单 ${selectedConfig.rankingId}` : '新建排行'"
      description="维护榜单类型、赛季参数、容量和刷新间隔。"
      size="wide"
    >
      <fieldset v-if="selectedConfig" class="form-fieldset" :disabled="!canManageConfigs">
        <div class="detail-overview">
          <article class="detail-overview__item"><span>来源</span><strong>{{ selectedConfig.isBuiltIn ? '内置' : '人工维护' }}</strong></article>
          <article class="detail-overview__item"><span>种子键</span><strong>{{ selectedConfig.seedKey || '-' }}</strong></article>
          <article class="detail-overview__item"><span>内置版本</span><strong>{{ selectedConfig.builtInVersion || '-' }}</strong></article>
          <article class="detail-overview__item"><span>最后更新</span><strong>{{ formatDateTime(selectedConfig.lastUpdateTime) }}</strong></article>
        </div>
        <div class="editor-grid editor-grid--three">
          <label class="field"><span>排行编号</span><input v-model.trim="selectedConfig.rankingId" type="text" /></label>
          <label class="field"><span>排行名称</span><input v-model.trim="selectedConfig.rankingName" type="text" /></label>
          <label class="field"><span>排行类型</span><select v-model.number="selectedConfig.rankingType"><option v-for="option in RANKING_TYPE_OPTIONS" :key="option.value" :value="option.value">{{ option.label }}</option></select></label>
          <label class="field"><span>最大容量</span><input v-model.number="selectedConfig.maxSize" type="number" min="1" /></label>
          <label class="field"><span>更新间隔</span><input v-model.number="selectedConfig.updateInterval" type="number" min="1" /></label>
          <label class="field"><span>赛季时长</span><input v-model.number="selectedConfig.seasonDuration" type="number" min="1" /></label>
          <label class="field"><span>当前赛季</span><input v-model.number="selectedConfig.currentSeason" type="number" min="1" /></label>
          <label class="field"><span>赛季开始时间</span><input v-model="selectedConfig.seasonStartTime" type="datetime-local" /></label>
          <label class="field"><span>排序值</span><input v-model.number="selectedConfig.sortOrder" type="number" /></label>
        </div>
        <div class="editor-grid">
          <label class="field checkbox-field"><input v-model="selectedConfig.seasonEnabled" type="checkbox" /><span>启用赛季</span></label>
          <label class="field checkbox-field"><input v-model="selectedConfig.isEnabled" type="checkbox" /><span>启用排行</span></label>
        </div>
        <label class="field"><span>描述</span><textarea v-model="selectedConfig.description" rows="3"></textarea></label>
      </fieldset>

      <template #footer>
        <button class="secondary-button" type="button" @click="configEditorOpen = false">取消</button>
        <button class="danger-button" type="button" :disabled="!canManageConfigs || !selectedConfig?.rankingId" @click="removeConfig">删除排行</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="saveConfig">保存排行</button>
      </template>
    </AdminModal>

    <AdminModal
      v-model="rewardEditorOpen"
      kicker="排行奖励"
      :title="selectedReward?.gid ? `编辑奖励 ${selectedReward.gid}` : '新建排行奖励'"
      description="维护榜单的名次区间与奖励内容。"
      size="wide"
    >
      <fieldset v-if="selectedReward" class="form-fieldset" :disabled="!canManageConfigs">
        <div class="detail-overview">
          <article class="detail-overview__item"><span>来源</span><strong>{{ selectedReward.isBuiltIn ? '内置' : '人工维护' }}</strong></article>
          <article class="detail-overview__item"><span>种子键</span><strong>{{ selectedReward.seedKey || '-' }}</strong></article>
          <article class="detail-overview__item"><span>内置版本</span><strong>{{ selectedReward.builtInVersion || '-' }}</strong></article>
          <article class="detail-overview__item"><span>最后更新</span><strong>{{ formatDateTime(selectedReward.lastUpdateTime) }}</strong></article>
        </div>
        <div class="editor-grid editor-grid--three">
          <label class="field"><span>奖励编号</span><input v-model.trim="selectedReward.gid" type="text" /></label>
          <label class="field"><span>排行编号</span><select v-model="selectedReward.rankingId"><option value="">请选择</option><option v-for="config in configs" :key="config.rankingId" :value="config.rankingId">{{ config.rankingName }} ({{ config.rankingId }})</option></select></label>
          <label class="field"><span>奖励标题</span><input v-model.trim="selectedReward.rewardTitle" type="text" /></label>
          <label class="field"><span>最小名次</span><input v-model.number="selectedReward.minRank" type="number" min="1" /></label>
          <label class="field"><span>最大名次</span><input v-model.number="selectedReward.maxRank" type="number" min="1" /></label>
          <label class="field"><span>金币</span><input v-model.number="selectedReward.gold" type="number" min="0" /></label>
          <label class="field"><span>灵石</span><input v-model.number="selectedReward.spiritStone" type="number" min="0" /></label>
          <label class="field"><span>称号</span><input v-model.trim="selectedReward.title" type="text" /></label>
        </div>
      </fieldset>

      <template #footer>
        <button class="secondary-button" type="button" @click="rewardEditorOpen = false">取消</button>
        <button class="danger-button" type="button" :disabled="!canManageConfigs || !selectedReward?.gid" @click="removeReward">删除奖励</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="saveReward">保存奖励</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import { RANKING_TYPE_OPTIONS } from '@/constants/game-options'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import type { AdminRankingConfigDetail, AdminRankingConfigListItem, AdminRankingRewardDetail, AdminRankingRewardListItem } from '@/types/admin'
import { deleteAdminRankingConfig, deleteAdminRankingReward, getAdminRankingConfigDetail, getAdminRankingConfigs, getAdminRankingRewardDetail, getAdminRankingRewards, saveAdminRankingConfig, saveAdminRankingReward } from '@/services/rankings'
import toast from '@/utils/toast'

// 页面主状态：
// configs 是榜单列表，rewards 是当前选中榜单的奖励区间，
// selectedConfig / selectedReward 分别用于两个编辑弹窗。
const { canManageConfigs } = useAdminPermissions()
const keyword = ref('')
const configs = ref<AdminRankingConfigListItem[]>([])
const rewards = ref<AdminRankingRewardListItem[]>([])
const selectedConfig = ref<AdminRankingConfigDetail | null>(null)
const selectedReward = ref<AdminRankingRewardDetail | null>(null)
const configEditorOpen = ref(false)
const rewardEditorOpen = ref(false)

// 统一格式化后台时间字段。
function formatDateTime(value?: string | null) {
  if (!value) return '-'
  return value.replace('T', ' ').slice(0, 19)
}

// 排行类型值转中文标签。
function getRankingTypeLabel(value: number) {
  return RANKING_TYPE_OPTIONS.find((option) => option.value === value)?.label || `类型 ${value}`
}

// 新建榜单时的默认对象。
function createEmptyConfig(): AdminRankingConfigDetail {
  return { rankingId: '', rankingName: '', rankingType: 0, description: '', maxSize: 100, updateInterval: 60, seasonEnabled: false, seasonDuration: 30, currentSeason: 1, seasonStartTime: '', sortOrder: 0, isEnabled: true, isBuiltIn: false, seedKey: null, builtInVersion: null, lastUpdateTime: null }
}

// 新建奖励区间时的默认对象。
function createEmptyReward(rankingId = ''): AdminRankingRewardDetail {
  return { gid: '', rankingId, minRank: 1, maxRank: 1, rewardTitle: '', gold: 0, spiritStone: 0, title: '', isBuiltIn: false, seedKey: null, builtInVersion: null, lastUpdateTime: null }
}

// 打开新建榜单弹窗。
function openCreateConfigModal() {
  if (!canManageConfigs.value) return
  selectedConfig.value = createEmptyConfig()
  configEditorOpen.value = true
}

// 打开新建奖励弹窗。
function openCreateRewardModal() {
  if (!canManageConfigs.value || !selectedConfig.value) return
  selectedReward.value = createEmptyReward(selectedConfig.value.rankingId)
  rewardEditorOpen.value = true
}

// 校验榜单配置的基础合法性。
function validateConfig() {
  if (!selectedConfig.value) return '当前没有可保存的排行配置。'
  if (!selectedConfig.value.rankingId.trim()) return '排行编号不能为空。'
  if (!selectedConfig.value.rankingName.trim()) return '排行名称不能为空。'
  if (selectedConfig.value.seasonEnabled && !selectedConfig.value.seasonStartTime) return '启用赛季时必须填写赛季开始时间。'
  return ''
}

// 校验奖励区间的基础合法性。
function validateReward() {
  if (!selectedReward.value) return '当前没有可保存的排行奖励。'
  if (!selectedReward.value.rankingId.trim()) return '排行编号不能为空。'
  if (!selectedReward.value.rewardTitle.trim()) return '奖励标题不能为空。'
  if (selectedReward.value.minRank <= 0 || selectedReward.value.maxRank <= 0 || selectedReward.value.minRank > selectedReward.value.maxRank) return '奖励名次区间必须合法，且最小名次不能大于最大名次。'
  return ''
}

// 拉取榜单列表。
async function loadConfigs() {
  configs.value = await getAdminRankingConfigs(keyword.value)
}

// 打开榜单详情弹窗，并同步读取其奖励区间。
async function openEditConfigModal(rankingId: string) {
  selectedConfig.value = await getAdminRankingConfigDetail(rankingId)
  rewards.value = await getAdminRankingRewards(rankingId)
  configEditorOpen.value = true
}

// 保存榜单配置。
async function saveConfig() {
  if (!canManageConfigs.value) return
  const validationMessage = validateConfig()
  if (validationMessage) { toast.error(validationMessage); return }
  try {
    selectedConfig.value = await saveAdminRankingConfig(selectedConfig.value!)
    toast.success('排行配置保存成功。')
    configEditorOpen.value = false
    await loadConfigs()
    if (selectedConfig.value?.rankingId) {
      rewards.value = await getAdminRankingRewards(selectedConfig.value.rankingId)
    }
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '排行配置保存失败。')
  }
}

// 删除榜单配置。
async function removeConfig() {
  if (!canManageConfigs.value || !selectedConfig.value?.rankingId) return
  if (!window.confirm(`确认删除排行 ${selectedConfig.value.rankingId} 吗？`)) return
  try {
    await deleteAdminRankingConfig(selectedConfig.value.rankingId)
    selectedConfig.value = null
    rewards.value = []
    toast.success('排行配置删除成功。')
    configEditorOpen.value = false
    await loadConfigs()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '排行配置删除失败。')
  }
}

// 打开奖励详情弹窗。
async function openEditRewardModal(gid: string) {
  selectedReward.value = await getAdminRankingRewardDetail(gid)
  rewardEditorOpen.value = true
}

// 保存奖励区间。
async function saveReward() {
  if (!canManageConfigs.value) return
  const validationMessage = validateReward()
  if (validationMessage) { toast.error(validationMessage); return }
  try {
    selectedReward.value = await saveAdminRankingReward(selectedReward.value!)
    toast.success('排行奖励保存成功。')
    rewardEditorOpen.value = false
    rewards.value = await getAdminRankingRewards(selectedReward.value.rankingId)
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '排行奖励保存失败。')
  }
}

async function removeReward() {
  if (!canManageConfigs.value || !selectedReward.value?.gid) return
  if (!window.confirm(`确认删除奖励区间 ${selectedReward.value.minRank}-${selectedReward.value.maxRank} 吗？`)) return
  try {
    const rankingId = selectedReward.value.rankingId
    await deleteAdminRankingReward(selectedReward.value.gid)
    selectedReward.value = null
    toast.success('排行奖励删除成功。')
    rewardEditorOpen.value = false
    rewards.value = await getAdminRankingRewards(rankingId)
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '排行奖励删除失败。')
  }
}

onMounted(loadConfigs)
</script>
