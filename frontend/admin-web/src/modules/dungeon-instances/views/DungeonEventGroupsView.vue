<template>
  <div class="dashboard-grid">
    <section class="panel-box" style="grid-column: 1 / -1;">
      <div class="section-header-copy">
        <p class="section-kicker">事件组</p>
        <h2 class="section-title section-title--small">事件组管理</h2>
        <p class="section-note">管理事件组，为秘境模板配置随机事件池和权重。</p>
      </div>
      <div style="display:flex;justify-content:flex-end;">
        <button class="secondary-button" type="button" :disabled="!canManageConfigs" @click="handleReloadRuntime">刷新运行时缓存</button>
      </div>
    </section>

    <!-- 事件组列表 -->
    <section class="panel-box filter-panel">
      <div class="filter-grid">
        <label class="field">
          <span>关键字</span>
          <input v-model.trim="keyword" type="text" placeholder="搜索编号或名称" />
        </label>
        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadGroups">查询</button>
          <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="openCreate">新建事件组</button>
        </div>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">事件组列表</p>
          <h3 class="section-title section-title--small">事件组列表</h3>
        </div>
        <span class="selected-pill">共 {{ groups.length }} 条</span>
      </div>
      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>编号</th>
              <th>名称</th>
              <th>事件数量</th>
              <th>来源</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="g in groups" :key="g.id"
              :class="{ 'is-active': detail?.id === g.id }"
              @click="openEdit(g.id)"
            >
              <td>{{ g.id }}</td>
              <td>{{ g.name }}</td>
              <td>{{ g.eventCount }}</td>
              <td>{{ g.isBuiltIn ? '内置' : '人工' }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>

    <!-- 事件组编辑弹窗 -->
    <AdminModal
      v-model="editorOpen"
      kicker="事件组"
      :title="detail?.id ? `编辑事件组 ${detail.id}` : '新建事件组'"
      description="配置事件组名称和组内事件权重。"
      size="xwide"
    >
      <fieldset v-if="detail" class="form-fieldset" :disabled="!canManageConfigs">
        <div class="detail-overview">
          <article class="detail-overview__item"><span>来源</span><strong>{{ detail.isBuiltIn ? '系统内置' : '人工配置' }}</strong></article>
          <article class="detail-overview__item"><span>种子键</span><strong>{{ detail.seedKey || '无' }}</strong></article>
          <article class="detail-overview__item"><span>内置版本</span><strong>{{ detail.builtInVersion || '无' }}</strong></article>
          <article class="detail-overview__item"><span>最后更新</span><strong>{{ formatDateTime(detail.lastUpdateTime) }}</strong></article>
        </div>
        <div class="editor-grid">
          <label class="field"><span>编号</span><input v-model.trim="detail.id" type="text" :disabled="!!detail.lastUpdateTime" /></label>
          <label class="field"><span>名称</span><input v-model.trim="detail.name" type="text" /></label>
        </div>
        <label class="field"><span>描述</span><textarea v-model="detail.description" rows="2"></textarea></label>

        <EventGroupEditor :model-value="parsedGroupItems" :event-options="eventOptions" @update:model-value="onGroupItemsChange" />
      </fieldset>
      <template #footer>
        <button class="secondary-button" type="button" @click="editorOpen = false">取消</button>
        <button class="danger-button" type="button" :disabled="!canManageConfigs || !detail?.id" @click="removeGroup">删除</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="saveGroup">保存</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, computed } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import EventGroupEditor from '@/components/editors/EventGroupEditor.vue'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import {
  getDungeonEventGroups, getDungeonEventGroupDetail, saveDungeonEventGroup, deleteDungeonEventGroup,
  getDungeonEventConfigs, reloadDungeonInstanceRuntime
} from '@/services/dungeon-instances'
import type { AdminDungeonEventGroupListItem, AdminDungeonEventGroupDetail, EventGroupItem } from '@/types/admin'
import toast from '@/utils/toast'

const { canManageConfigs } = useAdminPermissions()

const groups = ref<AdminDungeonEventGroupListItem[]>([])
const keyword = ref('')
const detail = ref<AdminDungeonEventGroupDetail | null>(null)
const editorOpen = ref(false)
const eventOptions = ref<{ value: string; label: string }[]>([])

const parsedGroupItems = computed((): EventGroupItem[] => {
  if (!detail.value?.groupItemsJson) return []
  try {
    return JSON.parse(detail.value.groupItemsJson)
  } catch { return [] }
})

function formatDateTime(value?: string | null) {
  if (!value) return '无'
  return value.replace('T', ' ').slice(0, 19)
}

async function loadGroups() {
  groups.value = await getDungeonEventGroups(keyword.value)
}

async function loadEventOptions() {
  const events = await getDungeonEventConfigs()
  eventOptions.value = events.map(e => ({ value: e.id, label: `${e.name} (${e.id})` }))
}

function openCreate() {
  if (!canManageConfigs.value) return
  detail.value = {
    id: '', name: '', description: '', groupItemsJson: null,
    isBuiltIn: false, seedKey: null, builtInVersion: null, lastUpdateTime: null
  }
  editorOpen.value = true
}

async function openEdit(id: string) {
  detail.value = await getDungeonEventGroupDetail(id)
  editorOpen.value = true
}

function onGroupItemsChange(items: EventGroupItem[]) {
  if (!detail.value) return
  detail.value.groupItemsJson = items.length > 0 ? JSON.stringify(items) : null
}

async function saveGroup() {
  if (!canManageConfigs.value || !detail.value) return
  if (!detail.value.id) { toast.error('编号不能为空'); return }
  if (!detail.value.name) { toast.error('名称不能为空'); return }
  try {
    detail.value = await saveDungeonEventGroup(detail.value)
    toast.success('事件组保存成功。')
    editorOpen.value = false
    await loadGroups()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '保存失败')
  }
}

async function removeGroup() {
  if (!canManageConfigs.value || !detail.value?.id) return
  if (!window.confirm(`确认删除事件组 ${detail.value.id} 吗？`)) return
  try {
    await deleteDungeonEventGroup(detail.value.id)
    toast.success('事件组删除成功。')
    detail.value = null
    editorOpen.value = false
    await loadGroups()
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
  await Promise.all([loadGroups(), loadEventOptions()])
})
</script>
