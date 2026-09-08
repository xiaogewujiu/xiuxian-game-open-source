<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">签到</p>
        <h2 class="section-title section-title--small">签到奖励</h2>
        <p class="section-note">签到奖励改成表格展示，新增和修改统一使用弹窗。</p>
      </div>

      <div class="filter-grid">
        <label class="field">
          <span>连续天数</span>
          <input :value="selectedConfig ? String(selectedConfig.continuousDay) : ''" type="text" placeholder="未选择" disabled />
        </label>
        <label class="field">
          <span>配置版本</span>
          <input :value="selectedConfig?.configVersion || ''" type="text" placeholder="未选择" disabled />
        </label>
        <label class="field">
          <span>奖励类型</span>
          <select v-model="milestoneFilter">
            <option value="">全部</option>
            <option value="milestone">里程碑</option>
            <option value="normal">普通奖励</option>
          </select>
        </label>

        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadConfigs">刷新</button>
          <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="openCreateModal">新建</button>
        </div>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">奖励列表</p>
          <h3 class="section-title section-title--small">签到奖励列表</h3>
        </div>
        <span class="selected-pill">共 {{ filteredConfigs.length }} 条</span>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>连续天数</th>
              <th>配置版本</th>
              <th>来源</th>
              <th>奖励类型</th>
              <th>说明</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="config in filteredConfigs"
              :key="config.continuousDay"
              :class="{ 'is-active': selectedConfig?.continuousDay === config.continuousDay }"
              @click="openEditModal(config)"
            >
              <td>第 {{ config.continuousDay }} 天</td>
              <td>{{ config.configVersion }}</td>
              <td>{{ config.isBuiltIn ? `内置 · ${config.builtInVersion || '未标记版本'}` : '人工配置' }}</td>
              <td>{{ config.isMilestone ? '里程碑' : '普通奖励' }}</td>
              <td class="data-table__message">{{ config.description || '-' }}</td>
            </tr>
          </tbody>
        </table>
      </div>

    </section>

    <AdminModal
      v-model="editorOpen"
      kicker="签到奖励"
      :title="selectedConfig ? `连续签到 ${selectedConfig.continuousDay} 天` : '新建签到奖励'"
      description="维护签到奖励版本、里程碑标记和奖励内容。"
      size="xwide"
    >
      <fieldset v-if="selectedConfig" class="form-fieldset" :disabled="!canManageConfigs">
        <div class="detail-overview detail-overview--compact">
          <article class="detail-overview__item"><span>来源</span><strong>{{ selectedConfig.isBuiltIn ? '系统内置' : '人工配置' }}</strong></article>
          <article class="detail-overview__item"><span>种子键</span><strong>{{ selectedConfig.seedKey || '无' }}</strong></article>
          <article class="detail-overview__item"><span>内置版本</span><strong>{{ selectedConfig.builtInVersion || '无' }}</strong></article>
          <article class="detail-overview__item"><span>最后更新</span><strong>{{ selectedConfig.lastUpdateTime || '未记录' }}</strong></article>
        </div>

        <div class="editor-grid">
          <label class="field"><span>连续天数</span><input v-model.number="selectedConfig.continuousDay" type="number" min="1" /></label>
          <label class="field"><span>配置版本</span><input v-model.trim="selectedConfig.configVersion" type="text" /></label>
        </div>

        <label class="field checkbox-field"><input v-model="selectedConfig.isMilestone" type="checkbox" /><span>里程碑奖励</span></label>
        <label class="field"><span>说明</span><textarea v-model="selectedConfig.description" rows="3"></textarea></label>
        <RewardGrantItemsEditor v-model="rewardDraft" title="签到奖励" :item-options="itemOptions" />
      </fieldset>

      <template #footer>
        <button class="secondary-button" type="button" @click="editorOpen = false">取消</button>
        <button class="danger-button" type="button" :disabled="!canManageConfigs || !selectedConfig || selectedConfig.continuousDay <= 0" @click="removeConfig">删除</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="saveConfig">保存配置</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import RewardGrantItemsEditor from '@/components/editors/RewardGrantItemsEditor.vue'
import toast from '@/utils/toast'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import { getAdminItems } from '@/services/items'
import { deleteCheckInConfig, getCheckInConfigs, saveCheckInConfig } from '@/services/reward-configs'
import type { AdminCheckInRewardConfig, AdminItemListItem, RewardGrantEntry } from '@/types/admin'

// 签到页维护的是“连续签到到第几天给什么奖励”的配置表。
const { canManageConfigs } = useAdminPermissions()
const milestoneFilter = ref('')
const configs = ref<AdminCheckInRewardConfig[]>([])
const items = ref<AdminItemListItem[]>([])
const selectedConfig = ref<AdminCheckInRewardConfig | null>(null)
const rewardDraft = ref<RewardGrantEntry[]>([])
const editorOpen = ref(false)

// 后端已支持按奖励类型筛选。
const filteredConfigs = configs

// 奖励编辑器的道具下拉选项。
const itemOptions = computed(() => items.value.map((item) => ({ value: item.itemId, label: `${item.name} (${item.itemId})` })))

// 把后端 rewardJson 解析成奖励编辑器草稿。
function parseRewards(value?: string | null) {
  if (!value) return []
  try {
    const parsed = JSON.parse(value) as Array<Record<string, unknown>>
    return parsed.map((item) => ({ type: String(item.Type ?? item.type ?? ''), count: Number(item.Count ?? item.count) || 1, itemId: item.ItemId ?? item.itemId ? String(item.ItemId ?? item.itemId) : null, description: item.Description ?? item.description ? String(item.Description ?? item.description) : null })).filter((item) => item.type)
  } catch {
    return []
  }
}

// 把奖励草稿重新序列化成 rewardJson。
function serializeRewards(entries: RewardGrantEntry[]) {
  return JSON.stringify(entries.map((item) => ({ Type: item.type, Count: item.count, ItemId: item.itemId, Description: item.description })))
}

// 根据当前选中的签到配置同步奖励草稿。
function syncRewardDraft(config: AdminCheckInRewardConfig | null) {
  rewardDraft.value = parseRewards(config?.rewardJson)
}

// 把当前奖励草稿回写到 selectedConfig。
function applyRewardDraft() {
  if (!selectedConfig.value) return
  selectedConfig.value.rewardJson = serializeRewards(rewardDraft.value)
}

// 新建签到配置时的默认对象。
function createEmptyConfig(): AdminCheckInRewardConfig {
  return { continuousDay: 1, isBuiltIn: false, seedKey: null, builtInVersion: null, configVersion: 'manual-admin', isMilestone: false, rewardJson: '[]', description: '', lastUpdateTime: null }
}

// 打开新建弹窗。
function openCreateModal() {
  if (!canManageConfigs.value) return
  selectedConfig.value = createEmptyConfig()
  syncRewardDraft(selectedConfig.value)
      editorOpen.value = true
}

// 预加载道具选项。
async function loadItemOptions() {
  items.value = await getAdminItems()
}

// 拉取签到配置列表。
async function loadConfigs() {
  const filterMilestone = milestoneFilter.value === '' ? null : milestoneFilter.value === 'milestone'
  configs.value = await getCheckInConfigs(filterMilestone)
}

// 打开签到配置详情弹窗。
function openEditModal(config: AdminCheckInRewardConfig) {
  selectedConfig.value = { ...config }
  syncRewardDraft(selectedConfig.value)
      editorOpen.value = true
}

// 保存签到配置。
async function saveConfig() {
  if (!canManageConfigs.value || !selectedConfig.value) return
  applyRewardDraft()
  try {
    selectedConfig.value = await saveCheckInConfig(selectedConfig.value)
    syncRewardDraft(selectedConfig.value)
    toast.success('签到奖励配置保存成功。')
    editorOpen.value = false
    await loadConfigs()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '签到奖励配置保存失败。')
  }
}

// 删除签到配置。
async function removeConfig() {
  if (!canManageConfigs.value || !selectedConfig.value) return
  if (!window.confirm(`确认删除签到奖励第 ${selectedConfig.value.continuousDay} 天配置吗？`)) return
  try {
    await deleteCheckInConfig(selectedConfig.value.continuousDay)
    toast.success('签到奖励配置删除成功。')
    selectedConfig.value = null
    syncRewardDraft(null)
    editorOpen.value = false
    await loadConfigs()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '签到奖励配置删除失败。')
  }
}

onMounted(async () => {
  await Promise.all([loadItemOptions(), loadConfigs()])
})
</script>
