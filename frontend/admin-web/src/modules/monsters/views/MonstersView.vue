<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">怪物</p>
        <h2 class="section-title section-title--small">怪物管理</h2>
        <p class="section-note">怪物模板统一改成表格列表，新增和编辑通过弹窗表单处理。</p>
      </div>

      <div class="filter-grid">
        <label class="field">
          <span>关键字</span>
          <input v-model.trim="keyword" type="text" placeholder="搜索怪物编号或名称" />
        </label>
        <label class="field">
          <span>当前怪物</span>
          <input :value="selectedMonster?.monsterId || ''" type="text" placeholder="未选择" disabled />
        </label>
        <label class="field">
          <span>等级</span>
          <input :value="selectedMonster ? String(selectedMonster.level) : ''" type="text" placeholder="全部" disabled />
        </label>

        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadMonsters">查询</button>
          <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="openCreateModal">新建</button>
        </div>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">怪物列表</p>
          <h3 class="section-title section-title--small">怪物列表</h3>
        </div>
        <span class="selected-pill">共 {{ monsters.length }} 条</span>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>怪物编号</th>
              <th>名称</th>
              <th>等级</th>
              <th>来源</th>
              <th>技能数</th>
              <th>被动数</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="monster in monsters"
              :key="monster.monsterId"
              :class="{ 'is-active': selectedMonster?.monsterId === monster.monsterId }"
              @click="openEditModal(monster.monsterId)"
            >
              <td>{{ monster.monsterId }}</td>
              <td>{{ monster.name }}</td>
              <td>Lv.{{ monster.level }}</td>
              <td>{{ monster.isBuiltIn ? '内置' : '人工' }}</td>
              <td>{{ monster.skillCount }}</td>
              <td>{{ monster.passiveCount }}</td>
            </tr>
          </tbody>
        </table>
      </div>

    </section>

    <AdminModal
      v-model="editorOpen"
      kicker="怪物编辑"
      :title="selectedMonster?.monsterId ? `编辑怪物 ${selectedMonster.monsterId}` : '新建怪物'"
      description="维护怪物等级、奖励、技能、属性和掉落配置。"
      size="xwide"
    >
      <fieldset v-if="selectedMonster" class="form-fieldset" :disabled="!canManageConfigs">
        <div class="detail-overview">
          <article class="detail-overview__item"><span>来源</span><strong>{{ selectedMonster.isBuiltIn ? '系统内置' : '人工配置' }}</strong></article>
          <article class="detail-overview__item"><span>种子键</span><strong>{{ selectedMonster.seedKey || '-' }}</strong></article>
          <article class="detail-overview__item"><span>内置版本</span><strong>{{ selectedMonster.builtInVersion || '-' }}</strong></article>
          <article class="detail-overview__item"><span>最后更新</span><strong>{{ formatDateTime(selectedMonster.lastUpdateTime) }}</strong></article>
        </div>
        <div class="editor-grid editor-grid--three">
          <label class="field"><span>怪物编号</span><input v-model.trim="selectedMonster.monsterId" type="text" /></label>
          <label class="field"><span>怪物名称</span><input v-model.trim="selectedMonster.name" type="text" /></label>
          <label class="field"><span>等级</span><input v-model.number="selectedMonster.level" type="number" min="1" /></label>
          <label class="field"><span>经验奖励下限</span><input v-model.number="selectedMonster.expRewardMin" type="number" min="0" /></label>
          <label class="field"><span>经验奖励上限</span><input v-model.number="selectedMonster.expRewardMax" type="number" min="0" /></label>
          <label class="field"><span>金币奖励下限</span><input v-model.number="selectedMonster.goldRewardMin" type="number" min="0" /></label>
          <label class="field"><span>金币奖励上限</span><input v-model.number="selectedMonster.goldRewardMax" type="number" min="0" /></label>
        </div>

        <SelectableStringListEditor v-model="selectedMonster.skillIds" title="技能列表" :options="skillOptions" />
        <SelectableStringListEditor v-model="selectedMonster.passiveIds" title="被动 Buff 列表" :options="buffOptions" />
        <ElementPoolEditor v-model="selectedMonster.elementPool" />
        <AttributesRangeEditor v-model="selectedMonster.attributes" />
        <DropListEditor v-model="selectedMonster.itemDrops" title="道具掉落" id-key="itemId" id-label="道具 ID" :options="itemOptions" />
        <DropListEditor v-model="selectedMonster.equipmentDrops" title="装备掉落" id-key="equipmentId" id-label="装备 ID" :options="equipmentOptions" />
        <CollectionDropListEditor v-model="selectedMonster.collectionDrops" title="图鉴掉落" :options="collectionSeriesOptions" />
        <div class="drop-rate-summary">
          <span :class="{ 'prob-over': totalDropRate > 10000 }">掉落概率总和: {{ totalDropRate }}/10000</span>
        </div>
      </fieldset>

      <template #footer>
        <button class="secondary-button" type="button" @click="editorOpen = false">取消</button>
        <button class="danger-button" type="button" :disabled="!canManageConfigs || !selectedMonster?.monsterId" @click="removeMonster">删除</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="saveMonster">保存怪物</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import type { AdminBuffListItem, AdminEquipmentListItem, AdminImageCollectionSeriesListItem, AdminItemListItem, AdminMonsterDetail, AdminMonsterListItem, AdminSkillListItem, AdminTextCollectionSeriesListItem } from '@/types/admin'
import { deleteAdminMonster, getAdminMonsterDetail, getAdminMonsters, saveAdminMonster } from '@/services/monsters'
import { getAdminBuffs, getAdminSkills } from '@/services/skills-buffs'
import { getAdminItems } from '@/services/items'
import { getAdminEquipments } from '@/services/equipments'
import SelectableStringListEditor from '@/components/editors/SelectableStringListEditor.vue'
import ElementPoolEditor from '@/components/editors/ElementPoolEditor.vue'
import AttributesRangeEditor from '@/components/editors/AttributesRangeEditor.vue'
import DropListEditor from '@/components/editors/DropListEditor.vue'
import CollectionDropListEditor from '@/components/editors/CollectionDropListEditor.vue'
import { getAdminTextCollectionSeries, getAdminImageCollectionSeries } from '@/services/collections'
import toast from '@/utils/toast'

// 怪物页同时依赖技能、Buff、道具和装备下拉选项，
// 因此进入页面时会把这些模板一次性预加载。
const { canManageConfigs } = useAdminPermissions()
const monsters = ref<AdminMonsterListItem[]>([])
const skills = ref<AdminSkillListItem[]>([])
const buffs = ref<AdminBuffListItem[]>([])
const items = ref<AdminItemListItem[]>([])
const equipments = ref<AdminEquipmentListItem[]>([])
const textSeries = ref<AdminTextCollectionSeriesListItem[]>([])
const imageSeries = ref<AdminImageCollectionSeriesListItem[]>([])
const keyword = ref('')
const selectedMonster = ref<AdminMonsterDetail | null>(null)
const editorOpen = ref(false)

const skillOptions = computed(() => skills.value.map((skill) => ({ value: String(skill.skillId), label: `${skill.name} (#${skill.skillId})` })))
const buffOptions = computed(() => buffs.value.map((buff) => ({ value: buff.buffId, label: `${buff.name} (${buff.buffId})` })))
const itemOptions = computed(() => items.value.map((item) => ({ value: item.itemId, label: `${item.name} (${item.itemId})` })))
const equipmentOptions = computed(() => equipments.value.map((equipment) => ({ value: String(equipment.equipmentId), label: `${equipment.name} (${equipment.equipmentId})` })))
const collectionSeriesOptions = computed(() => [
  ...textSeries.value.map((s) => ({ value: s.seriesId, label: `[文字] ${s.name} (${s.seriesId})`, collectionType: 0 })),
  ...imageSeries.value.map((s) => ({ value: s.seriesId, label: `[图片] ${s.name} (${s.seriesId})`, collectionType: 1 }))
])

const totalDropRate = computed(() => {
  if (!selectedMonster.value) return 0
  const m = selectedMonster.value
  return [...m.itemDrops, ...m.equipmentDrops, ...m.collectionDrops].reduce((sum, d) => sum + (d.rate || 0), 0)
})

// 统一格式化后台时间字段。
function formatDateTime(value?: string | null) {
  if (!value) return '-'
  return value.replace('T', ' ').slice(0, 19)
}

// 新建怪物时的默认对象。
function createEmptyMonster(): AdminMonsterDetail {
  return {
    monsterId: '',
    name: '',
    level: 1,
    expRewardMin: 0,
    expRewardMax: 0,
    goldRewardMin: 0,
    goldRewardMax: 0,
    skillIds: [],
    passiveIds: [],
    elementPool: [],
    itemDrops: [],
    equipmentDrops: [],
    collectionDrops: [],
    isBuiltIn: false,
    seedKey: '',
    builtInVersion: '',
    lastUpdateTime: '',
    attributes: {}
  }
}

function openCreateModal() {
  if (!canManageConfigs.value) return
  selectedMonster.value = createEmptyMonster()
  editorOpen.value = true
}

async function loadLookupOptions() {
  const [nextSkills, nextBuffs, nextItems, nextEquipments, nextTextSeries, nextImageSeries] = await Promise.all([
    getAdminSkills(),
    getAdminBuffs(),
    getAdminItems(),
    getAdminEquipments(),
    getAdminTextCollectionSeries(),
    getAdminImageCollectionSeries()
  ])

  skills.value = nextSkills
  buffs.value = nextBuffs
  items.value = nextItems
  equipments.value = nextEquipments
  textSeries.value = nextTextSeries
  imageSeries.value = nextImageSeries
}

async function loadMonsters() {
  monsters.value = await getAdminMonsters(keyword.value)
}

async function openEditModal(monsterId: string) {
  selectedMonster.value = await getAdminMonsterDetail(monsterId)
  editorOpen.value = true
}

async function saveMonster() {
  if (!canManageConfigs.value || !selectedMonster.value) return

  if (totalDropRate.value > 10000) {
    toast.error(`掉落概率总和超过 10000（当前：${totalDropRate.value}），超出部分会被截断。`)
    return
  }

  try {
    selectedMonster.value = await saveAdminMonster(selectedMonster.value)
    toast.success('怪物模板保存成功。')
    editorOpen.value = false
    await loadMonsters()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '怪物模板保存失败。')
  }
}

async function removeMonster() {
  if (!canManageConfigs.value || !selectedMonster.value?.monsterId) return
  if (!window.confirm(`确认删除怪物模板 ${selectedMonster.value.monsterId} 吗？`)) return

  try {
    await deleteAdminMonster(selectedMonster.value.monsterId)
    toast.success('怪物模板删除成功。')
    selectedMonster.value = null
    editorOpen.value = false
    await loadMonsters()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '怪物模板删除失败。')
  }
}

onMounted(async () => {
  await Promise.all([loadLookupOptions(), loadMonsters()])
})
</script>

<style scoped>
.drop-rate-summary {
  text-align: right;
  font-size: 13px;
  color: #374151;
  font-weight: 500;
  padding: 4px 0;
}
.prob-over {
  color: var(--color-danger, #ef4444);
  font-weight: 700;
}
</style>
