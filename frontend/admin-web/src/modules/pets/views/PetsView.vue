<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">灵宠</p>
        <h2 class="section-title section-title--small">灵宠模板</h2>
        <p class="section-note">灵宠生成规则统一放在表格中管理，编辑使用弹窗表单。</p>
      </div>

      <div class="filter-grid">
        <label class="field">
          <span>关键字</span>
          <input v-model.trim="keyword" type="text" :placeholder="TEXT.searchPlaceholder" />
        </label>
        <label class="field">
          <span>{{ TEXT.templateId }}</span>
          <input :value="selectedPet?.templateId || ''" type="text" :placeholder="TEXT.unset" disabled />
        </label>
        <label class="field">
          <span>{{ TEXT.qualityRange }}</span>
          <input :value="selectedPet ? `${selectedPet.initialQualityMin}-${selectedPet.initialQualityMax}` : ''" type="text" :placeholder="TEXT.unset" disabled />
        </label>

        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadPets">{{ TEXT.query }}</button>
          <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="openCreateModal">{{ TEXT.create }}</button>
        </div>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">灵宠列表</p>
          <h3 class="section-title section-title--small">灵宠列表</h3>
        </div>
        <span class="selected-pill">共 {{ pets.length }} 条</span>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>{{ TEXT.templateId }}</th>
              <th>{{ TEXT.name }}</th>
              <th>{{ TEXT.type }}</th>
              <th>{{ TEXT.source }}</th>
              <th>{{ TEXT.qualityRange }}</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="pet in pets"
              :key="pet.templateId"
              :class="{ 'is-active': selectedPet?.templateId === pet.templateId }"
              @click="openEditModal(pet.templateId)"
            >
              <td>{{ pet.templateId }}</td>
              <td>{{ pet.name }}</td>
              <td>{{ PET_TYPE_OPTIONS.find((option) => option.value === pet.type)?.label || pet.type }}</td>
              <td>{{ pet.isBuiltIn ? '内置' : '人工' }}</td>
              <td>{{ pet.initialQualityMin }}-{{ pet.initialQualityMax }}</td>
            </tr>
          </tbody>
        </table>
      </div>

    </section>

    <AdminModal
      v-model="editorOpen"
      kicker="灵宠编辑"
      :title="selectedPet?.templateId ? `编辑灵宠 ${selectedPet.templateId}` : TEXT.newPet"
      :description="TEXT.note"
      size="xwide"
    >
      <fieldset v-if="selectedPet" class="form-fieldset" :disabled="!canManageConfigs">
        <div class="detail-overview">
          <article class="detail-overview__item"><span>{{ TEXT.source }}</span><strong>{{ selectedPet.isBuiltIn ? '系统内置' : '人工配置' }}</strong></article>
          <article class="detail-overview__item"><span>{{ TEXT.seedKey }}</span><strong>{{ selectedPet.seedKey || TEXT.none }}</strong></article>
          <article class="detail-overview__item"><span>{{ TEXT.builtInVersion }}</span><strong>{{ selectedPet.builtInVersion || TEXT.none }}</strong></article>
          <article class="detail-overview__item"><span>{{ TEXT.lastUpdateTime }}</span><strong>{{ formatDateTime(selectedPet.lastUpdateTime) }}</strong></article>
        </div>
        <div class="editor-grid editor-grid--three">
          <label class="field"><span>{{ TEXT.templateId }}</span><input v-model.trim="selectedPet.templateId" type="text" /></label>
          <label class="field"><span>{{ TEXT.name }}</span><input v-model.trim="selectedPet.name" type="text" /></label>
          <label class="field"><span>{{ TEXT.type }}</span><select v-model.number="selectedPet.type"><option v-for="option in PET_TYPE_OPTIONS" :key="option.value" :value="option.value">{{ option.label }}</option></select></label>
          <label class="field"><span>{{ TEXT.initialQualityMin }}</span><input v-model.number="selectedPet.initialQualityMin" type="number" min="1" /></label>
          <label class="field"><span>{{ TEXT.initialQualityMax }}</span><input v-model.number="selectedPet.initialQualityMax" type="number" min="1" /></label>
          <label class="field"><span>{{ TEXT.maxQuality }}</span><input v-model.number="selectedPet.maxQuality" type="number" min="1" /></label>
          <label class="field"><span>{{ TEXT.growthRateMin }}</span><input v-model.number="selectedPet.growthRateMin" type="number" min="0.1" step="0.01" /></label>
          <label class="field"><span>{{ TEXT.growthRateMax }}</span><input v-model.number="selectedPet.growthRateMax" type="number" min="0.1" step="0.01" /></label>
          <label class="field"><span>{{ TEXT.initialSkillCount }}</span><input v-model.number="selectedPet.initialSkillCount" type="number" min="0" /></label>
          <label class="field checkbox-field"><input v-model="selectedPet.isTradable" type="checkbox" /><span>{{ TEXT.tradable }}</span></label>
        </div>

        <label class="field"><span>{{ TEXT.description }}</span><textarea v-model="selectedPet.description" rows="4"></textarea></label>
        <label class="field"><span>{{ TEXT.obtainMethod }}</span><textarea v-model="selectedPet.obtainMethod" rows="2"></textarea></label>
        <AttributesRangeEditor v-model="selectedPet.attributes" />
        <SelectableStringListEditor v-model="selectedPet.skillIds" :title="TEXT.skillPool" :options="skillOptions" />
      </fieldset>

      <template #footer>
        <button class="secondary-button" type="button" @click="editorOpen = false">取消</button>
        <button class="danger-button" type="button" :disabled="!canManageConfigs || !selectedPet?.templateId" @click="removePet">{{ TEXT.delete }}</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="savePet">{{ TEXT.save }}</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import { PET_TYPE_OPTIONS } from '@/constants/game-options'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import type { AdminPetDetail, AdminPetListItem, AdminSkillListItem } from '@/types/admin'
import { deleteAdminPet, getAdminPetDetail, getAdminPets, saveAdminPet } from '@/services/pets'
import { getAdminSkills } from '@/services/skills-buffs'
import SelectableStringListEditor from '@/components/editors/SelectableStringListEditor.vue'
import AttributesRangeEditor from '@/components/editors/AttributesRangeEditor.vue'
import toast from '@/utils/toast'

// 灵宠页维护的是“生成模板”，不是玩家已经拥有的灵宠实例。
const TEXT = {
  searchPlaceholder: '搜索灵宠编号或名称',
  query: '查询',
  create: '新建',
  delete: '删除',
  save: '保存灵宠模板',
  newPet: '新灵宠',
  unset: '未设置',
  none: '无',
  templateId: '模板编号',
  name: '名称',
  type: '类型',
  source: '来源',
  seedKey: '种子键',
  builtInVersion: '内置版本',
  lastUpdateTime: '最后更新',
  qualityRange: '品质区间',
  initialQualityMin: '初始品质下限',
  initialQualityMax: '初始品质上限',
  maxQuality: '最大品质',
  growthRange: '成长率区间',
  growthRateMin: '成长率下限',
  growthRateMax: '成长率上限',
  initialSkillCount: '初始技能数',
  tradable: '可交易',
  description: '描述',
  obtainMethod: '获取方式',
  skillPool: '技能池',
  note: '灵宠模板定义的是生成规则，不是固定宠物实例。生成时会按这里的品质、成长和技能池进行初始化。',
  saveSuccess: '灵宠模板保存成功。',
  saveFailed: '灵宠模板保存失败。',
  deleteSuccess: '灵宠模板删除成功。',
  deleteFailed: '灵宠模板删除失败。',
  deleteConfirm: '确认删除灵宠模板',
  deleteConfirmSuffix: '吗？',
  qualityRangeInvalid: '初始品质范围必须大于 0。',
  qualityRangeOrder: '初始品质下限不能大于上限。',
  maxQualityTooLow: '最大品质不能小于初始品质上限。',
  growthRangeInvalid: '成长率范围必须大于 0。',
  growthRangeOrder: '成长率下限不能大于上限。',
  initialSkillCountInvalid: '初始技能数不能小于 0。',
  initialSkillCountTooLarge: '初始技能数不能大于技能池数量。'
} as const

const { canManageConfigs } = useAdminPermissions()
const pets = ref<AdminPetListItem[]>([])
const skillTemplates = ref<AdminSkillListItem[]>([])
const keyword = ref('')
const selectedPet = ref<AdminPetDetail | null>(null)
const editorOpen = ref(false)

// 统一格式化后台时间字段。
function formatDateTime(value?: string | null) {
  if (!value) return TEXT.none
  return value.replace('T', ' ').slice(0, 19)
}

// 技能池下拉选项。
const skillOptions = computed(() => skillTemplates.value.map((skill) => ({
  value: String(skill.skillId),
  label: `${skill.name} (#${skill.skillId})`
})))

// 新建灵宠时的默认对象。
function createEmptyPet(): AdminPetDetail {
  return {
    templateId: '',
    name: '',
    description: '',
    type: 1,
    initialQualityMin: 1,
    initialQualityMax: 1,
    maxQuality: 1,
    growthRateMin: 1,
    growthRateMax: 1,
    initialSkillCount: 1,
    attributes: {},
    skillIds: [],
    obtainMethod: '',
    isTradable: true,
    isBuiltIn: false,
    seedKey: null,
    builtInVersion: null,
    lastUpdateTime: null
  }
}

// 打开新建弹窗。
function openCreateModal() {
  if (!canManageConfigs.value) return
  selectedPet.value = createEmptyPet()
  editorOpen.value = true
}

// 预加载技能池选项。
async function loadSkillOptions() {
  skillTemplates.value = await getAdminSkills()
}

// 拉取灵宠列表。
async function loadPets() {
  pets.value = await getAdminPets(keyword.value)
}

// 打开灵宠详情弹窗。
async function openEditModal(templateId: string) {
  selectedPet.value = await getAdminPetDetail(templateId)
  editorOpen.value = true
}

// 保存灵宠模板。
async function savePet() {
  if (!canManageConfigs.value || !selectedPet.value) return

  if (selectedPet.value.initialQualityMin <= 0 || selectedPet.value.initialQualityMax <= 0) {
    toast.error(TEXT.qualityRangeInvalid)
    return
  }

  if (selectedPet.value.initialQualityMin > selectedPet.value.initialQualityMax) {
    toast.error(TEXT.qualityRangeOrder)
    return
  }

  if (selectedPet.value.maxQuality < selectedPet.value.initialQualityMax) {
    toast.error(TEXT.maxQualityTooLow)
    return
  }

  if (selectedPet.value.growthRateMin <= 0 || selectedPet.value.growthRateMax <= 0) {
    toast.error(TEXT.growthRangeInvalid)
    return
  }

  if (selectedPet.value.growthRateMin > selectedPet.value.growthRateMax) {
    toast.error(TEXT.growthRangeOrder)
    return
  }

  if (selectedPet.value.initialSkillCount < 0) {
    toast.error(TEXT.initialSkillCountInvalid)
    return
  }

  if (selectedPet.value.initialSkillCount > selectedPet.value.skillIds.length) {
    toast.error(TEXT.initialSkillCountTooLarge)
    return
  }

  try {
    selectedPet.value = await saveAdminPet(selectedPet.value)
    toast.success(TEXT.saveSuccess)
    editorOpen.value = false
    await loadPets()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : TEXT.saveFailed)
  }
}

// 删除灵宠模板。
async function removePet() {
  if (!canManageConfigs.value || !selectedPet.value?.templateId) return
  if (!window.confirm(`${TEXT.deleteConfirm} ${selectedPet.value.templateId} ${TEXT.deleteConfirmSuffix}`)) return

  try {
    await deleteAdminPet(selectedPet.value.templateId)
    toast.success(TEXT.deleteSuccess)
    selectedPet.value = null
    editorOpen.value = false
    await loadPets()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : TEXT.deleteFailed)
  }
}

onMounted(async () => {
  await Promise.all([loadSkillOptions(), loadPets()])
})
</script>
