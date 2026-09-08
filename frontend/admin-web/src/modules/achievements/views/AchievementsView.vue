<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">成就</p>
        <h2 class="section-title section-title--small">成就配置</h2>
        <p class="section-note">成就支持多条件和多奖励，新增与编辑统一在弹窗里完成。</p>
      </div>

      <div class="filter-grid">
        <label class="field">
          <span>关键字</span>
          <input v-model.trim="keyword" type="text" placeholder="搜索成就编号或名称" />
        </label>
        <label class="field">
          <span>当前成就</span>
          <input :value="selectedAchievement?.achievementId || ''" type="text" placeholder="未选择" disabled />
        </label>
        <label class="field">
          <span>难度</span>
          <select v-model="difficultyFilter">
            <option value="">全部</option>
            <option v-for="option in ACHIEVEMENT_DIFFICULTY_OPTIONS" :key="option.value" :value="option.value">{{ option.label }}</option>
          </select>
        </label>

        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadAchievements">查询</button>
          <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="openCreateModal">新建</button>
        </div>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">成就列表</p>
          <h3 class="section-title section-title--small">成就列表</h3>
        </div>
        <span class="selected-pill">共 {{ filteredAchievements.length }} 条</span>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>成就编号</th>
              <th>成就名称</th>
              <th>成就类型</th>
              <th>难度</th>
              <th>来源</th>
              <th>状态</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="achievement in filteredAchievements"
              :key="achievement.achievementId"
              :class="{ 'is-active': selectedAchievement?.achievementId === achievement.achievementId }"
              @click="openEditModal(achievement.achievementId)"
            >
              <td>{{ achievement.achievementId }}</td>
              <td>{{ achievement.achievementName }}</td>
              <td>{{ getTypeLabel(achievement.achievementType) }}</td>
              <td>{{ getDifficultyLabel(achievement.difficulty) }}</td>
              <td>{{ achievement.isBuiltIn ? `内置 · ${achievement.builtInVersion || '未标记版本'}` : '人工配置' }}</td>
              <td>{{ achievement.isEnabled ? '启用' : '停用' }}</td>
            </tr>
          </tbody>
        </table>
      </div>

    </section>

    <AdminModal
      v-model="editorOpen"
      kicker="成就编辑"
      :title="selectedAchievement?.achievementId ? `编辑成就 ${selectedAchievement.achievementId}` : '新建成就'"
      description="维护成就类型、多条件、奖励和前置依赖。"
      size="xwide"
    >
      <fieldset v-if="selectedAchievement" class="form-fieldset" :disabled="!canManageConfigs">
        <div class="detail-overview detail-overview--compact">
          <article class="detail-overview__item"><span>来源</span><strong>{{ selectedAchievement.isBuiltIn ? '系统内置' : '人工配置' }}</strong></article>
          <article class="detail-overview__item"><span>种子键</span><strong>{{ selectedAchievement.seedKey || '无' }}</strong></article>
          <article class="detail-overview__item"><span>内置版本</span><strong>{{ selectedAchievement.builtInVersion || '无' }}</strong></article>
          <article class="detail-overview__item"><span>最后更新</span><strong>{{ selectedAchievement.lastUpdateTime || '未记录' }}</strong></article>
        </div>

        <div class="editor-grid editor-grid--three">
          <label class="field"><span>成就编号</span><input v-model.trim="selectedAchievement.achievementId" type="text" /></label>
          <label class="field"><span>成就名称</span><input v-model.trim="selectedAchievement.achievementName" type="text" /></label>
          <label class="field"><span>成就类型</span><select v-model.number="selectedAchievement.achievementType"><option v-for="option in ACHIEVEMENT_TYPE_OPTIONS" :key="option.value" :value="option.value">{{ option.label }}</option></select></label>
          <label class="field"><span>难度</span><select v-model.number="selectedAchievement.difficulty"><option v-for="option in ACHIEVEMENT_DIFFICULTY_OPTIONS" :key="option.value" :value="option.value">{{ option.label }}</option></select></label>
          <label class="field"><span>分类</span><input v-model.trim="selectedAchievement.category" type="text" /></label>
          <label class="field"><span>积分</span><input v-model.number="selectedAchievement.points" type="number" min="0" /></label>
          <label class="field"><span>奖励金币</span><input v-model.number="selectedAchievement.rewardGold" type="number" min="0" /></label>
          <label class="field"><span>奖励灵石</span><input v-model.number="selectedAchievement.rewardSpiritStone" type="number" min="0" /></label>
          <label class="field"><span>奖励经验</span><input v-model.number="selectedAchievement.rewardExp" type="number" min="0" /></label>
          <label class="field"><span>奖励称号</span><input v-model.trim="selectedAchievement.rewardTitle" type="text" /></label>
          <label class="field"><span>排序值</span><input v-model.number="selectedAchievement.sortOrder" type="number" /></label>
        </div>

        <div class="editor-grid">
          <label class="field checkbox-field"><input v-model="selectedAchievement.isHidden" type="checkbox" /><span>隐藏成就</span></label>
          <label class="field checkbox-field"><input v-model="selectedAchievement.isEnabled" type="checkbox" /><span>启用成就</span></label>
        </div>

        <label class="field"><span>描述</span><textarea v-model="selectedAchievement.description" rows="3"></textarea></label>
        <IdAmountListEditor v-model="rewardItemsDraft" title="奖励道具" id-label="道具编号" amount-label="数量" :options="itemOptions" />
        <SelectableStringListEditor v-model="preAchievementIdsDraft" title="前置成就" :options="achievementOptions" />
        <SelectableStringListEditor v-model="rewardEquipmentIdsDraft" title="奖励装备" :options="equipmentOptions" />
        <AchievementRequirementsEditor
          v-model="requirementDraft"
          :monster-options="monsterOptions"
          :item-options="itemOptions"
          :dungeon-options="dungeonOptions"
          :crop-options="cropOptions"
          :skill-options="skillOptions"
        />
      </fieldset>

      <template #footer>
        <button class="secondary-button" type="button" @click="editorOpen = false">取消</button>
        <button class="danger-button" type="button" :disabled="!canManageConfigs || !selectedAchievement?.achievementId" @click="removeAchievement">删除</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="saveAchievement">保存成就</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import AchievementRequirementsEditor from '@/components/editors/AchievementRequirementsEditor.vue'
import IdAmountListEditor from '@/components/editors/IdAmountListEditor.vue'
import SelectableStringListEditor from '@/components/editors/SelectableStringListEditor.vue'
import toast from '@/utils/toast'
import { ACHIEVEMENT_DIFFICULTY_OPTIONS, ACHIEVEMENT_TYPE_OPTIONS } from '@/constants/game-options'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import { deleteAdminAchievement, getAdminAchievementDetail, getAdminAchievements, saveAdminAchievement } from '@/services/achievements'
import { getAdminCrops } from '@/services/crops'
import { getAdminDungeons } from '@/services/dungeons'
import { getAdminEquipments } from '@/services/equipments'
import { getAdminItems } from '@/services/items'
import { getAdminMonsters } from '@/services/monsters'
import { getAdminSkills } from '@/services/skills-buffs'
import type {
  AchievementRequirementEntry,
  AdminAchievementDetail,
  AdminAchievementListItem,
  AdminCropListItem,
  AdminDungeonListItem,
  AdminEquipmentListItem,
  AdminItemListItem,
  AdminMonsterListItem,
  AdminSkillListItem,
  IdAmountEntry
} from '@/types/admin'

// 成就页同时依赖成就本体、奖励下拉和条件下拉选项，
// 所以进入页面时会把相关模板一起预加载。
const { canManageConfigs } = useAdminPermissions()
const achievements = ref<AdminAchievementListItem[]>([])
const equipmentTemplates = ref<AdminEquipmentListItem[]>([])
const itemTemplates = ref<AdminItemListItem[]>([])
const monsterTemplates = ref<AdminMonsterListItem[]>([])
const dungeonTemplates = ref<AdminDungeonListItem[]>([])
const cropTemplates = ref<AdminCropListItem[]>([])
const skillTemplates = ref<AdminSkillListItem[]>([])
const keyword = ref('')
const difficultyFilter = ref<number | ''>('')
const selectedAchievement = ref<AdminAchievementDetail | null>(null)
const preAchievementIdsDraft = ref<string[]>([])
const rewardEquipmentIdsDraft = ref<string[]>([])
const rewardItemsDraft = ref<IdAmountEntry[]>([])
const requirementDraft = ref<AchievementRequirementEntry[]>([])
const editorOpen = ref(false)

// 后端已支持按难度筛选。
const filteredAchievements = achievements

// 过滤掉当前自己，生成前置成就选择器选项。
const achievementOptions = computed(() => achievements.value
  .filter((achievement) => achievement.achievementId !== selectedAchievement.value?.achievementId)
  .map((achievement) => ({ value: achievement.achievementId, label: `${achievement.achievementName} (${achievement.achievementId})` })))

const equipmentOptions = computed(() => equipmentTemplates.value.map((equipment) => ({
  value: String(equipment.equipmentId),
  label: `${equipment.name} (${equipment.equipmentId})`
})))
const itemOptions = computed(() => itemTemplates.value.map((item) => ({ value: item.itemId, label: `${item.name} (${item.itemId})` })))
const monsterOptions = computed(() => monsterTemplates.value.map((monster) => ({ value: monster.monsterId, label: `${monster.name} (${monster.monsterId})` })))
const dungeonOptions = computed(() => dungeonTemplates.value.map((dungeon) => ({ value: dungeon.dungeonId, label: `${dungeon.name} (${dungeon.dungeonId})` })))
const cropOptions = computed(() => cropTemplates.value.map((crop) => ({ value: crop.templateId, label: `${crop.name} (${crop.templateId})` })))
const skillOptions = computed(() => skillTemplates.value.map((skill) => ({ value: String(skill.skillId), label: `${skill.name} (${skill.skillId})` })))

// 成就类型值转中文标签。
function getTypeLabel(value: number) {
  return ACHIEVEMENT_TYPE_OPTIONS.find((option) => option.value === value)?.label || `类型 ${value}`
}

// 难度值转中文标签。
function getDifficultyLabel(value: number) {
  return ACHIEVEMENT_DIFFICULTY_OPTIONS.find((option) => option.value === value)?.label || `难度 ${value}`
}

// 把逗号分隔字符串拆成数组。
function splitCsv(value?: string | null) {
  return (value || '').split(',').map((item) => item.trim()).filter(Boolean)
}

// 把数组重新拼成逗号分隔字符串。
function joinCsv(values: string[]) {
  const normalized = values.map((item) => item.trim()).filter(Boolean)
  return normalized.length > 0 ? normalized.join(',') : null
}

// 把奖励道具 JSON 解析成编辑器草稿。
function parseRewardItems(value?: string | null) {
  if (!value) return []
  try {
    const parsed = JSON.parse(value) as Record<string, number>
    return Object.entries(parsed).map(([id, amount]) => ({ id, amount: Number(amount) || 0 })).filter((item) => item.id && item.amount > 0)
  } catch {
    return []
  }
}

// 把奖励道具草稿重新序列化成 rewardItemsJson。
function serializeRewardItems(entries: IdAmountEntry[]) {
  const payload: Record<string, number> = {}
  for (const entry of entries) {
    const id = String(entry.id || '').trim()
    const amount = Number(entry.amount) || 0
    if (id && amount > 0) payload[id] = amount
  }
  return Object.keys(payload).length > 0 ? JSON.stringify(payload) : null
}

// 把 requirementsJson 解析成条件编辑器草稿。
function parseRequirements(value?: string | null, fallback?: AdminAchievementDetail | null) {
  if (value) {
    try {
      const parsed = JSON.parse(value) as Array<Record<string, unknown>>
      return parsed.map((item) => ({
        requirementType: Number(item.RequirementType ?? item.requirementType) || 0,
        targetId: String(item.TargetId ?? item.targetId ?? ''),
        targetValue: Number(item.TargetValue ?? item.targetValue) || 0,
        description: String(item.Description ?? item.description ?? '')
      }))
    } catch {
      // 回退到旧结构
    }
  }

  if (!fallback) return []
  return [{
    requirementType: Number(fallback.requirementType) || 0,
    targetId: '',
    targetValue: Number(fallback.requirementTargetValue) || 0,
    description: String(fallback.requirementDescription || '')
  }]
}

// 把条件草稿重新序列化成 requirementsJson。
function serializeRequirements(entries: AchievementRequirementEntry[]) {
  const normalized = entries
    .map((entry) => ({
      requirementType: Number(entry.requirementType) || 0,
      targetId: String(entry.targetId || '').trim(),
      targetValue: Math.max(0, Number(entry.targetValue) || 0),
      description: String(entry.description || '').trim()
    }))
    .filter((entry) => entry.targetValue > 0)

  if (normalized.length === 0) return null
  return JSON.stringify(normalized.map((entry) => ({
    RequirementType: entry.requirementType,
    TargetId: entry.targetId,
    TargetValue: entry.targetValue,
    Description: entry.description
  })))
}

// 根据当前成就详情同步“前置成就 / 奖励 / 条件”三份编辑草稿。
function syncAchievementEditors(achievement: AdminAchievementDetail | null) {
  preAchievementIdsDraft.value = splitCsv(achievement?.preAchievementIds)
  rewardEquipmentIdsDraft.value = splitCsv(achievement?.rewardEquipmentIds)
  rewardItemsDraft.value = parseRewardItems(achievement?.rewardItemsJson)
  requirementDraft.value = parseRequirements(achievement?.requirementsJson, achievement)
}

// 把当前编辑草稿回写到 selectedAchievement。
function applyAchievementEditors() {
  if (!selectedAchievement.value) return
  selectedAchievement.value.preAchievementIds = joinCsv(preAchievementIdsDraft.value)
  selectedAchievement.value.rewardEquipmentIds = joinCsv(rewardEquipmentIdsDraft.value)
  selectedAchievement.value.rewardItemsJson = serializeRewardItems(rewardItemsDraft.value)
  selectedAchievement.value.requirementsJson = serializeRequirements(requirementDraft.value)
  const primaryRequirement = requirementDraft.value.find((entry) => entry.targetValue > 0)
  selectedAchievement.value.requirementType = primaryRequirement?.requirementType ?? 0
  selectedAchievement.value.requirementTargetValue = primaryRequirement?.targetValue ?? 0
  selectedAchievement.value.requirementDescription = primaryRequirement?.description ?? ''
}

// 新建成就时的默认对象。
function createEmptyAchievement(): AdminAchievementDetail {
  return {
    achievementId: '',
    achievementName: '',
    achievementType: 0,
    difficulty: 10,
    description: '',
    category: '',
    points: 0,
    isHidden: false,
    preAchievementIds: '',
    rewardGold: 0,
    rewardSpiritStone: 0,
    rewardExp: 0,
    rewardTitle: '',
    rewardItemsJson: '',
    rewardEquipmentIds: '',
    requirementType: 0,
    requirementTargetValue: 0,
    requirementDescription: '',
    requirementsJson: '',
    sortOrder: 0,
    isEnabled: true,
    isBuiltIn: false,
    seedKey: null,
    builtInVersion: null,
    lastUpdateTime: null
  }
}

// 打开新建成就弹窗。
function openCreateModal() {
  if (!canManageConfigs.value) return
  selectedAchievement.value = createEmptyAchievement()
  syncAchievementEditors(selectedAchievement.value)
      editorOpen.value = true
}

// 预加载装备下拉选项。
async function loadEquipmentOptions() {
  equipmentTemplates.value = await getAdminEquipments()
}

// 预加载条件编辑器依赖的所有模板选项。
async function loadReferenceOptions() {
  const [items, monsters, dungeons, crops, skills] = await Promise.all([
    getAdminItems(),
    getAdminMonsters(),
    getAdminDungeons(),
    getAdminCrops(),
    getAdminSkills()
  ])
  itemTemplates.value = items
  monsterTemplates.value = monsters
  dungeonTemplates.value = dungeons
  cropTemplates.value = crops
  skillTemplates.value = skills
}

// 拉取成就列表。
async function loadAchievements() {
  const filterDifficulty = difficultyFilter.value === '' ? null : Number(difficultyFilter.value)
  achievements.value = await getAdminAchievements(keyword.value, filterDifficulty)
}

// 打开成就详情弹窗。
async function openEditModal(achievementId: string) {
  selectedAchievement.value = await getAdminAchievementDetail(achievementId)
  syncAchievementEditors(selectedAchievement.value)
      editorOpen.value = true
}

// 保存成就配置。
async function saveAchievement() {
  if (!canManageConfigs.value || !selectedAchievement.value) return
  applyAchievementEditors()

  try {
    selectedAchievement.value = await saveAdminAchievement(selectedAchievement.value)
    syncAchievementEditors(selectedAchievement.value)
    toast.success('成就配置保存成功。')
    editorOpen.value = false
    await loadAchievements()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '成就配置保存失败。')
  }
}

// 删除成就配置。
async function removeAchievement() {
  if (!canManageConfigs.value || !selectedAchievement.value?.achievementId) return
  if (!window.confirm(`确认删除成就 ${selectedAchievement.value.achievementId} 吗？`)) return

  try {
    await deleteAdminAchievement(selectedAchievement.value.achievementId)
    toast.success('成就配置删除成功。')
    selectedAchievement.value = null
    syncAchievementEditors(null)
    editorOpen.value = false
    await loadAchievements()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '成就配置删除失败。')
  }
}

onMounted(async () => {
  await Promise.all([loadEquipmentOptions(), loadReferenceOptions(), loadAchievements()])
})
</script>
