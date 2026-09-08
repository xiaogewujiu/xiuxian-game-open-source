<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">地图</p>
        <h2 class="section-title section-title--small">地图管理</h2>
        <p class="section-note">普通地图和副本层统一在表格中管理，新增和编辑通过弹窗完成。</p>
      </div>

      <div class="filter-grid">
        <label class="field">
          <span>关键字</span>
          <input v-model.trim="keyword" type="text" :placeholder="TEXT.searchPlaceholder" />
        </label>
        <label class="field">
          <span>{{ TEXT.mapId }}</span>
          <input :value="selectedMap?.mapId || ''" type="text" :placeholder="TEXT.unset" disabled />
        </label>
        <label class="field">
          <span>{{ TEXT.mapType }}</span>
          <select v-model="mapKindFilter">
            <option value="">全部</option>
            <option value="normal">{{ TEXT.normalMap }}</option>
            <option value="dungeonLayer">{{ TEXT.dungeonLayer }}</option>
          </select>
        </label>

        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadMaps">{{ TEXT.query }}</button>
          <button class="secondary-button" type="button" @click="resetKeyword">{{ TEXT.reset }}</button>
          <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="openCreateModal('normal')">{{ TEXT.createNormalMap }}</button>
          <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="openCreateModal('dungeonLayer')">{{ TEXT.createDungeonLayer }}</button>
        </div>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">地图列表</p>
          <h3 class="section-title section-title--small">地图列表</h3>
        </div>
        <div class="detail-actions detail-actions--inline">
          <span class="selected-pill">{{ TEXT.normalMap }} {{ normalMaps.length }}</span>
          <span class="selected-pill">{{ TEXT.dungeonLayer }} {{ dungeonLayerMaps.length }}</span>
        </div>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>{{ TEXT.mapId }}</th>
              <th>{{ TEXT.mapName }}</th>
              <th>{{ TEXT.mapType }}</th>
              <th>{{ TEXT.recommendedLevel }}</th>
              <th>{{ TEXT.source }}</th>
              <th>{{ TEXT.nextLayer }}</th>
              <th>{{ TEXT.spawnRules }}</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="map in filteredMaps"
              :key="map.mapId"
              :class="{ 'is-active': selectedMap?.mapId === map.mapId }"
              @click="openEditModal(map.mapId)"
            >
              <td>{{ map.mapId }}</td>
              <td>{{ map.name }}</td>
              <td>{{ inferMapKind(map.mapId) === 'dungeonLayer' ? TEXT.dungeonLayer : TEXT.normalMap }}</td>
              <td>Lv.{{ map.level }}</td>
              <td>{{ map.isBuiltIn ? '内置' : '人工' }}</td>
              <td>{{ map.nextMapId || TEXT.none }}</td>
              <td>{{ map.spawnRuleCount }} {{ TEXT.ruleUnit }}</td>
            </tr>
          </tbody>
        </table>
      </div>

    </section>

    <AdminModal
      v-model="editorOpen"
      kicker="地图编辑"
      :title="selectedMap?.mapId ? `编辑地图 ${selectedMap.mapId}` : TEXT.newMap"
      :description="TEXT.mapNote"
      size="xwide"
    >
      <fieldset v-if="selectedMap" class="form-fieldset" :disabled="!canManageConfigs">
        <div class="detail-overview">
          <article class="detail-overview__item">
            <span>{{ TEXT.source }}</span>
            <strong>{{ selectedMap.isBuiltIn ? '系统内置' : '人工配置' }}</strong>
          </article>
          <article class="detail-overview__item">
            <span>{{ TEXT.seedKey }}</span>
            <strong>{{ selectedMap.seedKey || TEXT.none }}</strong>
          </article>
          <article class="detail-overview__item">
            <span>{{ TEXT.builtInVersion }}</span>
            <strong>{{ selectedMap.builtInVersion || TEXT.none }}</strong>
          </article>
          <article class="detail-overview__item">
            <span>{{ TEXT.lastUpdateTime }}</span>
            <strong>{{ formatDateTime(selectedMap.lastUpdateTime) }}</strong>
          </article>
          <article class="detail-overview__item">
            <span>{{ TEXT.mapType }}</span>
            <strong>{{ selectedMap.mapKind === 'dungeonLayer' ? TEXT.dungeonLayer : TEXT.normalMap }}</strong>
          </article>
          <article class="detail-overview__item">
            <span>{{ TEXT.nextLayer }}</span>
            <strong>{{ selectedMap.mapKind === 'dungeonLayer' ? (selectedMap.nextMapId || TEXT.none) : TEXT.fixedEmpty }}</strong>
          </article>
          <article class="detail-overview__item">
            <span>{{ TEXT.mapReference }}</span>
            <strong>{{ selectedMap.referencingDungeonIds?.length ? selectedMap.referencingDungeonIds.join('、') : TEXT.none }}</strong>
          </article>
          <article class="detail-overview__item">
            <span>{{ TEXT.monsterCountMin }} / {{ TEXT.monsterCountMax }}</span>
            <strong>{{ selectedMap.monsterCountMin }} - {{ selectedMap.monsterCountMax }}</strong>
          </article>
        </div>

        <div class="editor-grid">
          <label class="field"><span>{{ TEXT.mapId }}</span><input v-model.trim="selectedMap.mapId" type="text" /></label>
          <label class="field"><span>{{ TEXT.mapName }}</span><input v-model.trim="selectedMap.name" type="text" /></label>
          <label class="field"><span>{{ TEXT.recommendedLevel }}</span><input v-model.number="selectedMap.level" type="number" min="1" /></label>
          <label class="field"><span>{{ TEXT.mapType }}</span><input :value="selectedMap.mapKind === 'dungeonLayer' ? TEXT.dungeonLayer : TEXT.normalMap" type="text" readonly /></label>
          <label class="field"><span>{{ TEXT.monsterCountMin }}</span><input v-model.number="selectedMap.monsterCountMin" type="number" min="1" /></label>
          <label class="field"><span>{{ TEXT.monsterCountMax }}</span><input v-model.number="selectedMap.monsterCountMax" type="number" min="1" /></label>
        </div>

        <div v-if="selectedMap.mapKind === 'dungeonLayer'" class="editor-grid">
          <label class="field"><span>{{ TEXT.nextLayer }}</span><select v-model="selectedMap.nextMapId"><option value="">{{ TEXT.none }}</option><option v-for="map in nextMapOptions" :key="map.mapId" :value="map.mapId">{{ map.name }} ({{ map.mapId }})</option></select></label>
          <label class="field checkbox-field"><input v-model="selectedMap.carrying" type="checkbox" /><span>{{ TEXT.carrying }}</span></label>
        </div>

        <label class="field"><span>{{ TEXT.description }}</span><textarea v-model="selectedMap.description" rows="4"></textarea></label>
        <SpawnRulesEditor v-model="selectedMap.spawnRules" :options="monsterOptions" />
      </fieldset>

      <template #footer>
        <button class="secondary-button" type="button" @click="editorOpen = false">取消</button>
        <button class="danger-button" type="button" :disabled="!canManageConfigs || !selectedMap?.mapId" @click="removeMap">{{ TEXT.delete }}</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="saveMap">{{ TEXT.save }}</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import type { AdminMapDetail, AdminMapListItem, AdminMonsterListItem } from '@/types/admin'
import { deleteAdminMap, getAdminMapDetail, getAdminMaps, saveAdminMap } from '@/services/maps'
import { getAdminMonsters } from '@/services/monsters'
import SpawnRulesEditor from '@/components/editors/SpawnRulesEditor.vue'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import toast from '@/utils/toast'

// 地图页把普通图和副本层统一维护，
// 通过 mapKind 把两种编辑逻辑收口到同一个弹窗。
const TEXT = {
  searchPlaceholder: '搜索地图编号或名称',
  query: '查询',
  reset: '清空',
  createNormalMap: '新建普通图',
  createDungeonLayer: '新建副本层',
  normalMap: '普通地图',
  dungeonLayer: '副本层',
  newMap: '新地图',
  delete: '删除',
  mapId: '地图编号',
  mapName: '地图名称',
  mapType: '地图类型',
  source: '来源',
  seedKey: '种子键',
  builtInVersion: '内置版本',
  lastUpdateTime: '最后更新',
  nextLayer: '下一层',
  mapReference: '副本链引用',
  spawnRules: '刷怪规则',
  ruleUnit: '条',
  recommendedLevel: '推荐等级',
  monsterCountMin: '最小怪物数',
  monsterCountMax: '最大怪物数',
  carrying: '继承上一层状态',
  description: '地图描述',
  none: '无',
  unset: '未设置',
  fixedEmpty: '固定为空',
  mapNote: '普通地图不保存 NextMapId，只有副本层地图允许配置下一层和继承状态。',
  enterMapId: '请输入地图编号。',
  normalPrefixError: '普通地图编号必须使用 map_ 前缀。',
  dungeonPrefixError: '副本层地图编号必须使用 fuben_ 前缀。',
  save: '保存地图',
  saveSuccess: '地图保存成功。',
  saveFailed: '地图保存失败。',
  deleteConfirm: '确认删除地图',
  deleteConfirmSuffix: '吗？',
  deleteSuccess: '地图删除成功。',
  deleteFailed: '地图删除失败。'
} as const

type MapKind = 'normal' | 'dungeonLayer'
type EditableMapDetail = AdminMapDetail & { mapKind: MapKind }

const { canManageConfigs } = useAdminPermissions()
const maps = ref<AdminMapListItem[]>([])
const monsters = ref<AdminMonsterListItem[]>([])
const keyword = ref('')
const mapKindFilter = ref('')
const selectedMap = ref<EditableMapDetail | null>(null)
const editorOpen = ref(false)

// 后端已支持按地图类型筛选。
const filteredMaps = maps

const normalMaps = computed(() => maps.value.filter((map) => inferMapKind(map.mapId) === 'normal'))
const dungeonLayerMaps = computed(() => maps.value.filter((map) => inferMapKind(map.mapId) === 'dungeonLayer'))
const nextMapOptions = computed(() => {
  if (!selectedMap.value || selectedMap.value.mapKind !== 'dungeonLayer') {
    return []
  }

  return dungeonLayerMaps.value.filter((map) => map.mapId !== selectedMap.value?.mapId)
})
const monsterOptions = computed(() => monsters.value.map((monster) => ({ value: monster.monsterId, label: `${monster.name} (${monster.monsterId})` })))

// 统一格式化后台时间字段。
function formatDateTime(value?: string | null) {
  if (!value) return TEXT.none
  return value.replace('T', ' ').slice(0, 19)
}

// 根据地图编号前缀判断是普通图还是副本层。
function inferMapKind(mapId: string): MapKind {
  return mapId.startsWith('fuben_') ? 'dungeonLayer' : 'normal'
}

// 把后端详情 DTO 转成当前页面的可编辑对象。
function toEditableMap(map: AdminMapDetail): EditableMapDetail {
  return {
    ...map,
    nextMapId: map.nextMapId || '',
    chainValid: map.chainValid ?? true,
    chainError: map.chainError || null,
    chainStages: map.chainStages || [],
    referencingDungeonIds: map.referencingDungeonIds || [],
    mapKind: inferMapKind(map.mapId || '')
  }
}

// 新建地图时的默认对象。
function createEmptyMap(mapKind: MapKind): EditableMapDetail {
  return {
    mapId: '',
    name: '',
    level: 1,
    description: '',
    nextMapId: '',
    carrying: false,
    monsterCountMin: 1,
    monsterCountMax: 1,
    spawnRules: [],
    isBuiltIn: false,
    seedKey: null,
    builtInVersion: null,
    lastUpdateTime: null,
    chainValid: true,
    chainError: null,
    chainStages: [],
    mapKind
  }
}

// 打开新建地图弹窗。
function openCreateModal(mapKind: MapKind) {
  if (!canManageConfigs.value) return
  selectedMap.value = createEmptyMap(mapKind)
  editorOpen.value = true
}

// 清空关键字并重新查询。
function resetKeyword() {
  keyword.value = ''
  void loadMaps()
}

// 拉取怪物选项，供刷怪规则编辑器使用。
async function loadMonsterOptions() {
  monsters.value = await getAdminMonsters()
}

// 拉取地图列表。
async function loadMaps() {
  const filterKind = mapKindFilter.value || null
  maps.value = await getAdminMaps(keyword.value, filterKind)
}

// 打开地图详情弹窗。
async function openEditModal(mapId: string) {
  const detail = await getAdminMapDetail(mapId)
  selectedMap.value = toEditableMap(detail)
  editorOpen.value = true
}

// 保存地图配置。
async function saveMap() {
  if (!canManageConfigs.value || !selectedMap.value) return
  if (!selectedMap.value.mapId) {
    toast.error(TEXT.enterMapId)
    return
  }

  if (selectedMap.value.mapKind === 'normal' && !selectedMap.value.mapId.startsWith('map_')) {
    toast.error(TEXT.normalPrefixError)
    return
  }

  if (selectedMap.value.mapKind === 'dungeonLayer' && !selectedMap.value.mapId.startsWith('fuben_')) {
    toast.error(TEXT.dungeonPrefixError)
    return
  }

  try {
    const payload: AdminMapDetail = {
      mapId: selectedMap.value.mapId,
      name: selectedMap.value.name,
      level: selectedMap.value.level,
      description: selectedMap.value.description,
      nextMapId: selectedMap.value.mapKind === 'dungeonLayer' ? (selectedMap.value.nextMapId || null) : null,
      carrying: selectedMap.value.mapKind === 'dungeonLayer' ? selectedMap.value.carrying : false,
      monsterCountMin: selectedMap.value.monsterCountMin,
      monsterCountMax: selectedMap.value.monsterCountMax,
      spawnRules: selectedMap.value.spawnRules,
      isBuiltIn: selectedMap.value.isBuiltIn,
      seedKey: selectedMap.value.seedKey,
      builtInVersion: selectedMap.value.builtInVersion,
      lastUpdateTime: selectedMap.value.lastUpdateTime,
      chainValid: selectedMap.value.chainValid,
      chainError: selectedMap.value.chainError,
      chainStages: selectedMap.value.chainStages,
      referencingDungeonIds: selectedMap.value.referencingDungeonIds
    }
    const saved = await saveAdminMap(payload)
    selectedMap.value = toEditableMap(saved)
    toast.success(TEXT.saveSuccess)
    editorOpen.value = false
    await loadMaps()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : TEXT.saveFailed)
  }
}

// 删除地图配置。
async function removeMap() {
  if (!canManageConfigs.value || !selectedMap.value?.mapId) return
  if (!window.confirm(`${TEXT.deleteConfirm} ${selectedMap.value.mapId} ${TEXT.deleteConfirmSuffix}`)) return
  try {
    await deleteAdminMap(selectedMap.value.mapId)
    toast.success(TEXT.deleteSuccess)
    selectedMap.value = null
    editorOpen.value = false
    await loadMaps()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : TEXT.deleteFailed)
  }
}

onMounted(async () => {
  await Promise.all([loadMonsterOptions(), loadMaps()])
})
</script>
