<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">副本</p>
        <h2 class="section-title section-title--small">副本管理</h2>
        <p class="section-note">副本入口统一以表格管理，新增和编辑使用弹窗表单。</p>
      </div>

      <div class="filter-grid">
        <label class="field">
          <span>关键字</span>
          <input v-model.trim="keyword" type="text" :placeholder="TEXT.searchPlaceholder" />
        </label>
        <label class="field">
          <span>当前副本</span>
          <input :value="selectedDungeon?.dungeonId || ''" type="text" :placeholder="TEXT.unset" disabled />
        </label>
        <label class="field">
          <span>{{ TEXT.teamSize }}</span>
          <input :value="selectedDungeon ? String(selectedDungeon.requiredTeamSize) : ''" type="text" :placeholder="TEXT.unset" disabled />
        </label>

        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadDungeons">{{ TEXT.query }}</button>
          <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="openCreateModal">{{ TEXT.create }}</button>
        </div>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">副本列表</p>
          <h3 class="section-title section-title--small">副本列表</h3>
        </div>
        <span class="selected-pill">共 {{ dungeons.length }} 条</span>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>{{ TEXT.dungeonId }}</th>
              <th>{{ TEXT.dungeonName }}</th>
              <th>{{ TEXT.recommendedLevel }}</th>
              <th>{{ TEXT.source }}</th>
              <th>{{ TEXT.teamSize }}</th>
              <th>{{ TEXT.dailyLimit }}</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="dungeon in dungeons"
              :key="dungeon.dungeonId"
              :class="{ 'is-active': selectedDungeon?.dungeonId === dungeon.dungeonId }"
              @click="openEditModal(dungeon.dungeonId)"
            >
              <td>{{ dungeon.dungeonId }}</td>
              <td>{{ dungeon.name }}</td>
              <td>Lv.{{ dungeon.recommendedLevel }}</td>
              <td>{{ dungeon.isBuiltIn ? '内置' : '人工' }}</td>
              <td>{{ dungeon.requiredTeamSize }} {{ TEXT.personUnit }}</td>
              <td>{{ dungeon.dailyLimit }}</td>
            </tr>
          </tbody>
        </table>
      </div>

    </section>

    <AdminModal
      v-model="editorOpen"
      kicker="副本编辑"
      :title="selectedDungeon?.dungeonId ? `编辑副本 ${selectedDungeon.dungeonId}` : TEXT.newDungeon"
      :description="TEXT.dungeonNote"
      size="wide"
    >
      <fieldset v-if="selectedDungeon" class="form-fieldset" :disabled="!canManageConfigs">
        <div class="detail-overview">
          <article class="detail-overview__item"><span>{{ TEXT.source }}</span><strong>{{ selectedDungeon.isBuiltIn ? '系统内置' : '人工配置' }}</strong></article>
          <article class="detail-overview__item"><span>{{ TEXT.seedKey }}</span><strong>{{ selectedDungeon.seedKey || TEXT.none }}</strong></article>
          <article class="detail-overview__item"><span>{{ TEXT.builtInVersion }}</span><strong>{{ selectedDungeon.builtInVersion || TEXT.none }}</strong></article>
          <article class="detail-overview__item"><span>{{ TEXT.lastUpdateTime }}</span><strong>{{ formatDateTime(selectedDungeon.lastUpdateTime) }}</strong></article>
        </div>
        <div v-if="!selectedDungeon.chainValid" class="chain-error">地图链非法：{{ selectedDungeon.chainError || '未知错误' }}</div>
        <div v-else class="chain-preview">
          <div v-for="stage in selectedDungeon.chainStages" :key="`${stage.stageIndex}-${stage.mapId}`" class="chain-stage">
            <strong>第 {{ stage.stageIndex }} 层</strong>
            <span>{{ stage.mapName }}（{{ stage.mapId }}）</span>
            <small>NextMapId：{{ stage.nextMapId || TEXT.none }}</small>
          </div>
        </div>
        <div class="editor-grid">
          <label class="field"><span>{{ TEXT.dungeonId }}</span><input v-model.trim="selectedDungeon.dungeonId" type="text" /></label>
          <label class="field"><span>{{ TEXT.dungeonName }}</span><input v-model.trim="selectedDungeon.name" type="text" /></label>
          <label class="field"><span>{{ TEXT.recommendedLevel }}</span><input v-model.number="selectedDungeon.recommendedLevel" type="number" min="1" /></label>
          <label class="field"><span>{{ TEXT.dailyLimit }}</span><input v-model.number="selectedDungeon.dailyLimit" type="number" min="0" /></label>
          <label class="field"><span>{{ TEXT.firstLayerMap }}</span><select v-model="selectedDungeon.fubenMapId"><option value="">{{ TEXT.pleaseSelect }}</option><option v-for="map in fubenMapOptions" :key="`${map.mapId}-fuben`" :value="map.mapId">{{ map.name }} ({{ map.mapId }})</option></select></label>
          <label class="field"><span>{{ TEXT.teamSize }}</span><input v-model.number="selectedDungeon.requiredTeamSize" type="number" min="1" /></label>
        </div>

        <label class="field">
          <span>{{ TEXT.description }}</span>
          <textarea v-model="selectedDungeon.description" rows="5"></textarea>
        </label>
      </fieldset>

      <template #footer>
        <button class="secondary-button" type="button" @click="editorOpen = false">取消</button>
        <button class="danger-button" type="button" :disabled="!canManageConfigs || !selectedDungeon?.dungeonId" @click="removeDungeon">{{ TEXT.delete }}</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="saveDungeon">{{ TEXT.save }}</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import { getAdminDungeons, getAdminDungeonDetail, saveAdminDungeon, deleteAdminDungeon } from '@/services/dungeons'
import { getAdminMaps } from '@/services/maps'
import type { AdminDungeonDetail, AdminDungeonListItem, AdminMapListItem } from '@/types/admin'
import toast from '@/utils/toast'

// 副本页只维护“副本入口”本身；
// 首层之后的层级关系仍然通过地图配置里的 NextMapId 串联。
const TEXT = {
  searchPlaceholder: '搜索副本编号或名称',
  query: '查询',
  create: '新建',
  delete: '删除',
  save: '保存副本',
  newDungeon: '新副本',
  unset: '未设置',
  none: '无',
  dungeonId: '副本编号',
  dungeonName: '副本名称',
  recommendedLevel: '推荐等级',
  source: '来源',
  seedKey: '种子键',
  builtInVersion: '内置版本',
  lastUpdateTime: '最后更新',
  firstLayerMap: '副本首层地图',
  teamSize: '所需人数',
  dailyLimit: '日次数限制',
  description: '描述',
  personUnit: '人',
  pleaseSelect: '请选择',
  dungeonNote: '副本只维护首层地图入口，后续层级由首层地图的 NextMapId 继续串联。',
  selectFirstLayer: '请选择副本首层地图。',
  saveSuccess: '副本模板保存成功。',
  saveFailed: '副本模板保存失败。',
  deleteConfirm: '确认删除副本',
  deleteConfirmSuffix: '吗？',
  deleteSuccess: '副本模板删除成功。',
  deleteFailed: '副本模板删除失败。'
} as const

const { canManageConfigs } = useAdminPermissions()
const dungeons = ref<AdminDungeonListItem[]>([])
const mapOptions = ref<AdminMapListItem[]>([])
const keyword = ref('')
const selectedDungeon = ref<AdminDungeonDetail | null>(null)
const editorOpen = ref(false)
const fubenMapOptions = computed(() => mapOptions.value.filter((map) => map.mapId.startsWith('fuben_')))

// 统一格式化后台时间字段。
function formatDateTime(value?: string | null) {
  if (!value) return TEXT.none
  return value.replace('T', ' ').slice(0, 19)
}

// 新建副本时的默认对象。
function createEmptyDungeon(): AdminDungeonDetail {
  return {
    dungeonId: '',
    name: '',
    description: '',
    recommendedLevel: 1,
    dailyLimit: 0,
    fubenMapId: '',
    requiredTeamSize: 1,
    isBuiltIn: false,
    seedKey: null,
    builtInVersion: null,
    lastUpdateTime: null,
    chainValid: true,
    chainError: null,
    chainStages: []
  }
}

// 打开新建副本弹窗。
function openCreateModal() {
  if (!canManageConfigs.value) return
  selectedDungeon.value = createEmptyDungeon()
      editorOpen.value = true
}

// 拉取地图选项，供“副本首层地图”下拉框使用。
async function loadMapOptions() {
  mapOptions.value = await getAdminMaps()
}

// 拉取副本列表。
async function loadDungeons() {
  dungeons.value = await getAdminDungeons(keyword.value)
}

// 打开副本详情弹窗。
async function openEditModal(dungeonId: string) {
  selectedDungeon.value = await getAdminDungeonDetail(dungeonId)
      editorOpen.value = true
}

// 保存副本配置。
async function saveDungeon() {
  if (!canManageConfigs.value || !selectedDungeon.value) return
  if (!selectedDungeon.value.fubenMapId) {
    toast.error(TEXT.selectFirstLayer)
        return
  }

  try {
    selectedDungeon.value = await saveAdminDungeon(selectedDungeon.value)
    toast.success(TEXT.saveSuccess)
    editorOpen.value = false
    await loadDungeons()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : TEXT.saveFailed)
  }
}

// 删除副本配置。
async function removeDungeon() {
  if (!canManageConfigs.value || !selectedDungeon.value?.dungeonId) return
  if (!window.confirm(`${TEXT.deleteConfirm} ${selectedDungeon.value.dungeonId} ${TEXT.deleteConfirmSuffix}`)) return

  try {
    await deleteAdminDungeon(selectedDungeon.value.dungeonId)
    toast.success(TEXT.deleteSuccess)
    selectedDungeon.value = null
    editorOpen.value = false
    await loadDungeons()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : TEXT.deleteFailed)
  }
}

onMounted(async () => {
  await Promise.all([loadMapOptions(), loadDungeons()])
})
</script>

<style scoped>
.chain-error {
  margin: 12px 0;
  padding: 10px 12px;
  border: 1px solid #ef4444;
  color: #b91c1c;
  background: #fef2f2;
  border-radius: 8px;
}

.chain-preview {
  display: grid;
  gap: 8px;
  margin: 12px 0;
}

.chain-stage {
  display: grid;
  grid-template-columns: 80px minmax(0, 1fr) auto;
  gap: 10px;
  align-items: center;
  padding: 8px 10px;
  border: 1px solid var(--border-color, #d1d5db);
  border-radius: 8px;
}

.chain-stage small {
  color: var(--muted-color, #6b7280);
}
</style>
