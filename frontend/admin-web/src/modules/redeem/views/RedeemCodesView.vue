<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">兑换码</p>
        <h2 class="section-title section-title--small">兑换码</h2>
        <p class="section-note">兑换码配置统一使用表格展示，新增和编辑使用弹窗表单。</p>
      </div>

      <div class="filter-grid">
        <label class="field">
          <span>关键字</span>
          <input v-model.trim="keyword" type="text" placeholder="搜索兑换码或说明" />
        </label>
        <label class="field">
          <span>当前兑换码</span>
          <input :value="selectedConfig?.code || ''" type="text" placeholder="未选择" disabled />
        </label>
        <label class="field">
          <span>状态</span>
          <select v-model="statusFilter">
            <option value="">全部</option>
            <option value="enabled">启用中</option>
            <option value="disabled">已禁用</option>
          </select>
        </label>

        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadConfigs">查询</button>
          <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="openCreateModal">新建</button>
        </div>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">兑换码列表</p>
          <h3 class="section-title section-title--small">兑换码列表</h3>
        </div>
        <span class="selected-pill">共 {{ filteredConfigs.length }} 条</span>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>兑换码</th>
              <th>配置版本</th>
              <th>来源</th>
              <th>状态</th>
              <th>说明</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="config in filteredConfigs"
              :key="config.code"
              :class="{ 'is-active': selectedConfig?.code === config.code }"
              @click="openEditModal(config)"
            >
              <td>{{ config.code }}</td>
              <td>{{ config.configVersion }}</td>
              <td>{{ config.isBuiltIn ? `内置 · ${config.builtInVersion || '未标记版本'}` : '人工配置' }}</td>
              <td>{{ config.isEnabled ? '启用中' : '已禁用' }}</td>
              <td class="data-table__message">{{ config.description || '-' }}</td>
            </tr>
          </tbody>
        </table>
      </div>

    </section>

    <AdminModal
      v-model="editorOpen"
      kicker="兑换码"
      :title="selectedConfig?.code ? `编辑兑换码 ${selectedConfig.code}` : '新建兑换码'"
      description="维护兑换码状态、版本说明和奖励内容。"
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
          <label class="field"><span>兑换码</span><input v-model.trim="selectedConfig.code" type="text" /></label>
          <label class="field"><span>配置版本</span><input v-model.trim="selectedConfig.configVersion" type="text" /></label>
        </div>

        <label class="field checkbox-field"><input v-model="selectedConfig.isEnabled" type="checkbox" /><span>启用兑换码</span></label>
        <label class="field"><span>说明</span><textarea v-model="selectedConfig.description" rows="3"></textarea></label>
        <RewardGrantItemsEditor v-model="rewardDraft" title="兑换码奖励" :item-options="itemOptions" />
      </fieldset>

      <template #footer>
        <button class="secondary-button" type="button" @click="editorOpen = false">取消</button>
        <button class="danger-button" type="button" :disabled="!canManageConfigs || !selectedConfig?.code" @click="removeConfig">删除</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="saveConfig">保存兑换码</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import RewardGrantItemsEditor from '@/components/editors/RewardGrantItemsEditor.vue'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import { getAdminItems } from '@/services/items'
import { deleteRedeemCodeConfig, getRedeemCodeConfigs, saveRedeemCodeConfig } from '@/services/reward-configs'
import type { AdminItemListItem, AdminRedeemCodeConfig, RewardGrantEntry } from '@/types/admin'
import toast from '@/utils/toast'

// 兑换码页维护的是“可配置奖励”，真正核销逻辑仍由游戏后端处理。
const { canManageConfigs } = useAdminPermissions()
const keyword = ref('')
const statusFilter = ref('')
const configs = ref<AdminRedeemCodeConfig[]>([])
const items = ref<AdminItemListItem[]>([])
const selectedConfig = ref<AdminRedeemCodeConfig | null>(null)
const rewardDraft = ref<RewardGrantEntry[]>([])
const editorOpen = ref(false)

// 后端已支持按状态筛选。
const filteredConfigs = configs

// 兑换码奖励编辑器的道具下拉选项。
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

// 根据当前选中的兑换码同步奖励草稿。
function syncRewardDraft(config: AdminRedeemCodeConfig | null) {
  rewardDraft.value = parseRewards(config?.rewardJson)
}

// 把当前奖励草稿回写到 selectedConfig。
function applyRewardDraft() {
  if (!selectedConfig.value) return
  selectedConfig.value.rewardJson = serializeRewards(rewardDraft.value)
}

// 新建兑换码时的默认对象。
function createEmptyConfig(): AdminRedeemCodeConfig {
  return { code: '', isBuiltIn: false, seedKey: null, builtInVersion: null, configVersion: 'manual-admin', isEnabled: true, description: '', rewardJson: '[]', lastUpdateTime: null }
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

// 拉取兑换码列表。
async function loadConfigs() {
  const filterEnabled = statusFilter.value === '' ? null : statusFilter.value === 'enabled'
  configs.value = await getRedeemCodeConfigs(keyword.value, filterEnabled)
}

// 打开兑换码详情弹窗。
function openEditModal(config: AdminRedeemCodeConfig) {
  selectedConfig.value = { ...config }
  syncRewardDraft(selectedConfig.value)
  editorOpen.value = true
}

// 保存兑换码配置。
async function saveConfig() {
  if (!canManageConfigs.value || !selectedConfig.value) return
  applyRewardDraft()
  try {
    selectedConfig.value = await saveRedeemCodeConfig(selectedConfig.value)
    syncRewardDraft(selectedConfig.value)
    toast.success('兑换码配置保存成功。')
    editorOpen.value = false
    await loadConfigs()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '兑换码配置保存失败。')
  }
}

// 删除兑换码配置。
async function removeConfig() {
  if (!canManageConfigs.value || !selectedConfig.value?.code) return
  if (!window.confirm(`确认删除兑换码 ${selectedConfig.value.code} 吗？`)) return
  try {
    await deleteRedeemCodeConfig(selectedConfig.value.code)
    toast.success('兑换码配置删除成功。')
    selectedConfig.value = null
    syncRewardDraft(null)
    editorOpen.value = false
    await loadConfigs()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '兑换码配置删除失败。')
  }
}

onMounted(async () => {
  await Promise.all([loadItemOptions(), loadConfigs()])
})
</script>
