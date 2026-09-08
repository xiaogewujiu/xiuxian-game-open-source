<template>
  <div class="dashboard-grid">
    <section class="panel-box" style="grid-column: 1 / -1;">
      <div class="section-header-copy">
        <p class="section-kicker">秘境实例</p>
        <h2 class="section-title section-title--small">秘境模板管理</h2>
        <p class="section-note">配置秘境模板参数、开放时间、进入消耗和事件组。</p>
      </div>
      <div style="display:flex;justify-content:flex-end;">
        <button class="secondary-button" type="button" :disabled="!canManageConfigs" @click="handleReloadRuntime">刷新运行时缓存</button>
      </div>
    </section>

    <!-- 搜索 + 列表 -->
    <section class="panel-box filter-panel">
      <div class="filter-grid">
        <label class="field">
          <span>关键字</span>
          <input v-model.trim="tplKeyword" type="text" placeholder="搜索编号或名称" />
        </label>
        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadTemplates">查询</button>
          <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="openCreateTemplate">新建</button>
        </div>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">模板列表</p>
          <h3 class="section-title section-title--small">模板列表</h3>
        </div>
        <span class="selected-pill">共 {{ templates.length }} 条</span>
      </div>
      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>编号</th>
              <th>名称</th>
              <th>启用</th>
              <th>推荐等级</th>
              <th>日限</th>
              <th>Tick间隔</th>
              <th>事件组</th>
              <th>来源</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="t in templates" :key="t.id"
                :class="{ 'is-active': tplDetail?.id === t.id }"
                @click="openEditTemplate(t.id)">
              <td>{{ t.id }}</td>
              <td>{{ t.name }}</td>
              <td>{{ t.enabled ? '是' : '否' }}</td>
              <td>Lv.{{ t.recommendedLevel }}</td>
              <td>{{ t.dailyEnterLimit }}</td>
              <td>{{ t.tickIntervalSeconds }}s</td>
              <td>{{ groupName(t.eventGroupId) }}</td>
              <td>{{ t.isBuiltIn ? '内置' : '人工' }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>

    <!-- 模板编辑弹窗 -->
    <AdminModal
      v-model="tplEditorOpen"
      kicker="秘境模板"
      :title="tplDetail?.id ? `编辑模板 ${tplDetail.id}` : '新建秘境模板'"
      description="配置秘境基础参数、开放时间、进入消耗和事件组。"
      size="xwide"
    >
      <fieldset v-if="tplDetail" class="form-fieldset" :disabled="!canManageConfigs">
        <div class="detail-overview">
          <article class="detail-overview__item"><span>来源</span><strong>{{ tplDetail.isBuiltIn ? '系统内置' : '人工配置' }}</strong></article>
          <article class="detail-overview__item"><span>种子键</span><strong>{{ tplDetail.seedKey || '无' }}</strong></article>
          <article class="detail-overview__item"><span>内置版本</span><strong>{{ tplDetail.builtInVersion || '无' }}</strong></article>
          <article class="detail-overview__item"><span>最后更新</span><strong>{{ formatDateTime(tplDetail.lastUpdateTime) }}</strong></article>
        </div>
        <div class="editor-grid">
          <label class="field"><span>编号</span><input v-model.trim="tplDetail.id" type="text" :disabled="!!tplDetail.lastUpdateTime" /></label>
          <label class="field"><span>名称</span><input v-model.trim="tplDetail.name" type="text" /></label>
          <label class="field"><span>推荐等级</span><input v-model.number="tplDetail.recommendedLevel" type="number" min="1" /></label>
          <label class="field"><span>每日进入上限</span><input v-model.number="tplDetail.dailyEnterLimit" type="number" min="0" /></label>
          <label class="field"><span>Tick间隔(秒)</span><input v-model.number="tplDetail.tickIntervalSeconds" type="number" min="5" /></label>
          <label class="field checkbox-field"><input v-model="tplDetail.enabled" type="checkbox" /><span>启用</span></label>
        </div>
        <label class="field"><span>描述</span><textarea v-model="tplDetail.description" rows="3"></textarea></label>

        <!-- 事件组选择 -->
        <label class="field">
          <span>事件组</span>
          <select v-model="tplDetail.eventGroupId">
            <option value="">未选择</option>
            <option v-for="g in eventGroups" :key="g.id" :value="g.id">{{ g.name }} ({{ g.id }})</option>
          </select>
        </label>

        <!-- 开放时间编辑器 -->
        <div class="editor-section">
          <h4 class="editor-section__title">开放时间</h4>
          <ScheduleEditor :model-value="tplDetail.openScheduleJson" @update:model-value="tplDetail.openScheduleJson = $event" />
        </div>

        <!-- 进入消耗编辑器 -->
        <div class="editor-section">
          <h4 class="editor-section__title">进入消耗</h4>
          <EntryCostEditor
            :model-value="tplDetail.entryCostsJson"
            :item-options="itemOptions"
            @update:model-value="tplDetail.entryCostsJson = $event"
          />
        </div>

        <!-- 自动用药编辑器 -->
        <div class="editor-section">
          <h4 class="editor-section__title">自动用药配置</h4>
          <MedicineConfigEditor :model-value="tplDetail.autoMedicineConfigJson" :item-options="itemOptions" @update:model-value="tplDetail.autoMedicineConfigJson = $event" />
        </div>

        <!-- 偶遇配置编辑器 -->
        <div class="editor-section">
          <h4 class="editor-section__title">玩家偶遇配置</h4>
          <EncounterConfigEditor :model-value="tplDetail.encounterConfigJson" @update:model-value="tplDetail.encounterConfigJson = $event" />
        </div>
      </fieldset>
      <template #footer>
        <button class="secondary-button" type="button" @click="tplEditorOpen = false">取消</button>
        <button class="danger-button" type="button" :disabled="!canManageConfigs || !tplDetail?.id" @click="removeTemplate">删除</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="saveTemplate">保存</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, computed } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import ScheduleEditor from '@/components/editors/ScheduleEditor.vue'
import EntryCostEditor from '@/components/editors/EntryCostEditor.vue'
import MedicineConfigEditor from '@/components/editors/MedicineConfigEditor.vue'
import EncounterConfigEditor from '@/components/editors/EncounterConfigEditor.vue'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import {
  getDungeonInstanceTemplates, getDungeonInstanceTemplateDetail, saveDungeonInstanceTemplate, deleteDungeonInstanceTemplate,
  getDungeonEventGroups, reloadDungeonInstanceRuntime
} from '@/services/dungeon-instances'
import { getAdminItems } from '@/services/items'
import type {
  AdminDungeonInstanceTemplateListItem, AdminDungeonInstanceTemplateDetail,
  AdminDungeonEventGroupListItem
} from '@/types/admin'
import toast from '@/utils/toast'

const { canManageConfigs } = useAdminPermissions()

const templates = ref<AdminDungeonInstanceTemplateListItem[]>([])
const tplKeyword = ref('')
const tplDetail = ref<AdminDungeonInstanceTemplateDetail | null>(null)
const tplEditorOpen = ref(false)
const eventGroups = ref<AdminDungeonEventGroupListItem[]>([])
const allItems = ref<{ value: string; label: string }[]>([])

const itemOptions = computed(() => allItems.value)

function groupName(groupId?: string | null): string {
  if (!groupId) return '未设置'
  return eventGroups.value.find(g => g.id === groupId)?.name ?? groupId
}

function formatDateTime(value?: string | null) {
  if (!value) return '无'
  return value.replace('T', ' ').slice(0, 19)
}


async function loadTemplates() {
  templates.value = await getDungeonInstanceTemplates(tplKeyword.value)
}

function createEmptyTemplate(): AdminDungeonInstanceTemplateDetail {
  return {
    id: '', name: '', description: '', enabled: true,
    recommendedLevel: 1, dailyEnterLimit: 3, tickIntervalSeconds: 30,
    openScheduleJson: null, entryCostsJson: null, eventGroupId: null,
    autoMedicineConfigJson: null, encounterConfigJson: null,
    isBuiltIn: false, seedKey: null, builtInVersion: null, lastUpdateTime: null
  }
}

function openCreateTemplate() {
  if (!canManageConfigs.value) return
  tplDetail.value = createEmptyTemplate()
  tplEditorOpen.value = true
}

async function openEditTemplate(id: string) {
  tplDetail.value = await getDungeonInstanceTemplateDetail(id)
  tplEditorOpen.value = true
}

async function saveTemplate() {
  if (!canManageConfigs.value || !tplDetail.value) return
  if (!tplDetail.value.id) { toast.error('编号不能为空'); return }
  if (!tplDetail.value.name) { toast.error('名称不能为空'); return }
  try {
    tplDetail.value = await saveDungeonInstanceTemplate(tplDetail.value)
    toast.success('秘境模板保存成功。')
    tplEditorOpen.value = false
    await loadTemplates()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '保存失败')
  }
}

async function removeTemplate() {
  if (!canManageConfigs.value || !tplDetail.value?.id) return
  if (!window.confirm(`确认删除秘境模板 ${tplDetail.value.id} 吗？`)) return
  try {
    await deleteDungeonInstanceTemplate(tplDetail.value.id)
    toast.success('秘境模板删除成功。')
    tplDetail.value = null
    tplEditorOpen.value = false
    await loadTemplates()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '删除失败')
  }
}

async function handleReloadRuntime() {
  if (!canManageConfigs.value) return
  try {
    await reloadDungeonInstanceRuntime()
    toast.success('运行时缓存已刷新。')
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '刷新失败')
  }
}

onMounted(async () => {
  const [, groups, items] = await Promise.all([loadTemplates(), getDungeonEventGroups(), getAdminItems()])
  eventGroups.value = groups
  allItems.value = items.map(i => ({ value: i.itemId, label: `${i.name} (${i.itemId})` }))
})
</script>

<style scoped>
.editor-section {
  margin-top: 16px;
  padding-top: 12px;
  border-top: 1px solid var(--border-color, #eee);
}
.editor-section__title {
  font-size: 14px;
  font-weight: 600;
  margin: 0 0 8px;
  color: var(--text-primary);
}
</style>
