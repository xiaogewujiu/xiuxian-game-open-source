<template>
  <div class="dashboard-grid">
    <section class="panel-box" style="grid-column: 1 / -1;">
      <div class="section-header-copy">
        <p class="section-kicker">事件类型</p>
        <h2 class="section-title section-title--small">事件类型管理</h2>
        <p class="section-note">点击事件类型卡片查看和管理该类型下的所有事件。</p>
      </div>
      <div style="display:flex;justify-content:flex-end;">
        <button class="secondary-button" type="button" :disabled="!canManageConfigs" @click="handleReloadRuntime">刷新运行时缓存</button>
      </div>
    </section>

    <!-- 事件类型卡片网格 -->
    <section class="panel-box" style="grid-column: 1 / -1;">
      <div class="type-grid">
        <div
          v-for="typeOpt in DUNGEON_EVENT_TYPE_OPTIONS"
          :key="typeOpt.value"
          class="type-card"
          @click="openTypeEvents(typeOpt.value)"
        >
          <div class="type-card__icon">{{ typeIcon(typeOpt.value) }}</div>
          <div class="type-card__label">{{ typeOpt.label }}</div>
          <div class="type-card__count">{{ typeEventCount(typeOpt.value) }} 个事件</div>
        </div>
      </div>
    </section>

    <!-- 事件列表弹窗 -->
    <AdminModal
      v-model="listModalOpen"
      kicker="事件列表"
      :title="`${currentTypeName} 事件列表`"
      :description="`查看和管理 ${currentTypeName} 类型下的所有事件配置。`"
      size="xwide"
    >
      <div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:12px;">
        <span class="selected-pill">共 {{ filteredEvents.length }} 个事件</span>
        <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="openCreateEvent">新建事件</button>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>编号</th>
              <th>名称</th>
              <th>所属秘境</th>
              <th>权重</th>
              <th>启用</th>
              <th>来源</th>
              <th>操作</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="e in filteredEvents" :key="e.id" @click="openEditEvent(e.id)">
              <td>{{ e.id }}</td>
              <td>{{ e.name }}</td>
              <td>{{ e.dungeonId }}</td>
              <td>{{ e.weight }}</td>
              <td>{{ e.enabled ? '是' : '否' }}</td>
              <td>{{ e.isBuiltIn ? '内置' : '人工' }}</td>
              <td @click.stop>
                <button class="secondary-button" style="padding:2px 8px;font-size:12px" @click="handleToggleEvent(e.id, !e.enabled)">
                  {{ e.enabled ? '禁用' : '启用' }}
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </AdminModal>

    <!-- 事件编辑弹窗 -->
    <AdminModal
      v-model="editorOpen"
      kicker="事件配置"
      :title="evtDetail?.id ? `编辑事件 ${evtDetail.id}` : '新建事件'"
      description="配置事件参数、权重和触发条件。"
      size="wide"
    >
      <fieldset v-if="evtDetail" class="form-fieldset" :disabled="!canManageConfigs">
        <div class="detail-overview">
          <article class="detail-overview__item"><span>来源</span><strong>{{ evtDetail.isBuiltIn ? '系统内置' : '人工配置' }}</strong></article>
          <article class="detail-overview__item"><span>种子键</span><strong>{{ evtDetail.seedKey || '无' }}</strong></article>
          <article class="detail-overview__item"><span>内置版本</span><strong>{{ evtDetail.builtInVersion || '无' }}</strong></article>
          <article class="detail-overview__item"><span>最后更新</span><strong>{{ formatDateTime(evtDetail.lastUpdateTime) }}</strong></article>
        </div>
        <div class="editor-grid editor-grid--three">
          <label class="field"><span>编号</span><input v-model.trim="evtDetail.id" type="text" :disabled="!!evtDetail.lastUpdateTime" /></label>
          <label class="field"><span>名称</span><input v-model.trim="evtDetail.name" type="text" /></label>
          <label class="field">
            <span>所属秘境</span>
            <select v-model="evtDetail.dungeonId">
              <option value="*">通用 (*)</option>
              <option v-for="t in templates" :key="t.id" :value="t.id">{{ t.name }} ({{ t.id }})</option>
            </select>
          </label>
          <label class="field">
            <span>事件类型</span>
            <select v-model.number="evtDetail.eventType">
              <option v-for="opt in DUNGEON_EVENT_TYPE_OPTIONS" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
            </select>
          </label>
          <label class="field"><span>权重</span><input v-model.number="evtDetail.weight" type="number" min="1" /></label>
          <label class="field checkbox-field"><input v-model="evtDetail.enabled" type="checkbox" /><span>启用</span></label>
          <label class="field checkbox-field"><input v-model="evtDetail.deathKeep" type="checkbox" /><span>死亡保留</span></label>
        </div>
        <label class="field"><span>描述</span><textarea v-model="evtDetail.description" rows="3"></textarea></label>
        <div class="editor-section">
          <h4 class="editor-section__title">事件参数</h4>
          <EventDataEditor :model-value="evtDetail.eventDataJson" :event-type="evtDetail.eventType" :monster-options="monsterOptions" :item-options="itemOptions" :chest-item-options="chestItemOptions" :equipment-options="equipmentOptions" :collection-series-options="collectionSeriesOptions" @update:model-value="evtDetail.eventDataJson = $event" />
        </div>
      </fieldset>
      <template #footer>
        <button class="secondary-button" type="button" @click="editorOpen = false">取消</button>
        <button class="danger-button" type="button" :disabled="!canManageConfigs || !evtDetail?.id" @click="removeEvent">删除</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="saveEvent">保存</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, computed } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import EventDataEditor from '@/components/editors/EventDataEditor.vue'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import { DUNGEON_EVENT_TYPE_OPTIONS } from '@/constants/game-options'
import {
  getDungeonInstanceTemplates,
  getDungeonEventConfigs, getDungeonEventConfigDetail, saveDungeonEventConfig, deleteDungeonEventConfig,
  toggleDungeonEventConfig, reloadDungeonInstanceRuntime
} from '@/services/dungeon-instances'
import { getAdminMonsters } from '@/services/monsters'
import { getAdminItems } from '@/services/items'
import { getAdminEquipments } from '@/services/equipments'
import { getAdminTextCollectionSeries, getAdminImageCollectionSeries } from '@/services/collections'
import type {
  AdminDungeonInstanceTemplateListItem,
  AdminDungeonEventConfigListItem, AdminDungeonEventConfigDetail
} from '@/types/admin'
import toast from '@/utils/toast'

const { canManageConfigs } = useAdminPermissions()

const templates = ref<AdminDungeonInstanceTemplateListItem[]>([])
const events = ref<AdminDungeonEventConfigListItem[]>([])
const monsters = ref<{ value: string; label: string }[]>([])
const items = ref<{ value: string; label: string }[]>([])
const chestItems = ref<{ value: string; label: string }[]>([])
const equipments = ref<{ value: string; label: string }[]>([])
const collectionSeries = ref<{ value: string; label: string }[]>([])
const currentType = ref(1)
const listModalOpen = ref(false)
const editorOpen = ref(false)
const evtDetail = ref<AdminDungeonEventConfigDetail | null>(null)

const currentTypeName = computed(() => DUNGEON_EVENT_TYPE_OPTIONS.find(o => o.value === currentType.value)?.label ?? '')
const filteredEvents = computed(() => events.value.filter(e => e.eventType === currentType.value))
const monsterOptions = computed(() => monsters.value)
const itemOptions = computed(() => items.value)
const chestItemOptions = computed(() => chestItems.value)
const equipmentOptions = computed(() => equipments.value)
const collectionSeriesOptions = computed(() => collectionSeries.value)

function typeIcon(type: number): string {
  const icons: Record<number, string> = { 1: '⚔', 2: '💚', 3: '⬆', 4: '⬇', 5: '📦', 6: '🎁', 7: '✨', 8: '🏪', 9: '⚡', 10: '—', 11: '👥' }
  return icons[type] ?? '?'
}

function typeEventCount(type: number): number {
  return events.value.filter(e => e.eventType === type).length
}

function formatDateTime(value?: string | null) {
  if (!value) return '无'
  return value.replace('T', ' ').slice(0, 19)
}

async function openTypeEvents(type: number) {
  currentType.value = type
  listModalOpen.value = true
}

function createEmptyEvent(): AdminDungeonEventConfigDetail {
  return {
    id: '', name: '', description: '', dungeonId: '*',
    eventType: currentType.value, weight: 10, enabled: true,
    eventDataJson: null, deathKeep: null,
    isBuiltIn: false, seedKey: null, builtInVersion: null, lastUpdateTime: null
  }
}

function openCreateEvent() {
  if (!canManageConfigs.value) return
  evtDetail.value = createEmptyEvent()
  editorOpen.value = true
}

async function openEditEvent(id: string) {
  evtDetail.value = await getDungeonEventConfigDetail(id)
  editorOpen.value = true
}

async function saveEvent() {
  if (!canManageConfigs.value || !evtDetail.value) return
  if (!evtDetail.value.id) { toast.error('编号不能为空'); return }
  if (!evtDetail.value.name) { toast.error('名称不能为空'); return }
  try {
    evtDetail.value = await saveDungeonEventConfig(evtDetail.value)
    toast.success('事件配置保存成功。')
    editorOpen.value = false
    events.value = await getDungeonEventConfigs()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '保存失败')
  }
}

async function removeEvent() {
  if (!canManageConfigs.value || !evtDetail.value?.id) return
  if (!window.confirm(`确认删除事件 ${evtDetail.value.id} 吗？`)) return
  try {
    await deleteDungeonEventConfig(evtDetail.value.id)
    toast.success('事件配置删除成功。')
    evtDetail.value = null
    editorOpen.value = false
    events.value = await getDungeonEventConfigs()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '删除失败')
  }
}

async function handleToggleEvent(eventId: string, enabled: boolean) {
  if (!canManageConfigs.value) return
  try {
    await toggleDungeonEventConfig(eventId, enabled)
    toast.success(enabled ? '事件已启用' : '事件已禁用')
    events.value = await getDungeonEventConfigs()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '操作失败')
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
  const [tpls, evts, mons, itmList, chestItmList, eqList, textSeries, imgSeries] = await Promise.all([
    getDungeonInstanceTemplates(), getDungeonEventConfigs(), getAdminMonsters(), getAdminItems(),
    getAdminItems('', 10), getAdminEquipments(), getAdminTextCollectionSeries(), getAdminImageCollectionSeries()
  ])
  templates.value = tpls
  events.value = evts
  monsters.value = mons.map(m => ({ value: m.monsterId, label: `${m.name} (${m.monsterId})` }))
  items.value = itmList.map(i => ({ value: i.itemId, label: `${i.name} (${i.itemId})` }))
  chestItems.value = chestItmList.map(i => ({ value: i.itemId, label: `${i.name} (${i.itemId})` }))
  equipments.value = eqList.map(e => ({ value: String(e.equipmentId), label: `${e.name} (Lv.${e.level})` }))
  collectionSeries.value = [
    ...textSeries.map(s => ({ value: `text:${s.seriesId}`, label: `[文字] ${s.name}` })),
    ...imgSeries.map(s => ({ value: `image:${s.seriesId}`, label: `[图片] ${s.name}` }))
  ]
})
</script>

<style scoped>
.type-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
  gap: 16px;
  margin-top: 12px;
}
.type-card {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  padding: 20px 12px;
  background: var(--surface-secondary, #f8f9fa);
  border: 1px solid var(--border-color, #e0e0e0);
  border-radius: 10px;
  cursor: pointer;
  transition: transform 0.15s, box-shadow 0.15s;
}
.type-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0,0,0,0.1);
}
.type-card__icon {
  font-size: 28px;
  line-height: 1;
}
.type-card__label {
  font-size: 15px;
  font-weight: 600;
  color: var(--text-primary);
}
.type-card__count {
  font-size: 12px;
  color: var(--text-secondary);
}
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
