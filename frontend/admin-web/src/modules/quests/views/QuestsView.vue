<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">任务</p>
        <h2 class="section-title section-title--small">任务配置</h2>
        <p class="section-note">任务配置统一改成表格列表，新增和编辑通过弹窗表单完成。</p>
      </div>

      <div class="filter-grid">
        <label class="field">
          <span>关键字</span>
          <input v-model.trim="keyword" type="text" placeholder="搜索任务编号或名称" />
        </label>
        <label class="field">
          <span>当前任务</span>
          <input :value="selectedQuest?.questId || ''" type="text" placeholder="未选择" disabled />
        </label>
        <label class="field">
          <span>任务类型</span>
          <select v-model="questTypeFilter">
            <option value="">全部</option>
            <option v-for="option in QUEST_TYPE_OPTIONS" :key="option.value" :value="option.value">{{ option.label }}</option>
          </select>
        </label>
        <label class="field">
          <span>重置周期</span>
          <select v-model="resetCycleFilter">
            <option value="">全部</option>
            <option v-for="option in QUEST_RESET_CYCLE_OPTIONS" :key="option.value" :value="option.value">{{ option.label }}</option>
          </select>
        </label>

        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadQuests">查询</button>
          <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="openCreateModal">新建</button>
        </div>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">任务列表</p>
          <h3 class="section-title section-title--small">任务列表</h3>
        </div>
        <span class="selected-pill">共 {{ filteredQuests.length }} 条</span>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>任务编号</th>
              <th>任务名称</th>
              <th>任务类型</th>
              <th>重置周期</th>
              <th>来源</th>
              <th>要求等级</th>
              <th>状态</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="quest in filteredQuests"
              :key="quest.questId"
              :class="{ 'is-active': selectedQuest?.questId === quest.questId }"
              @click="openEditModal(quest.questId)"
            >
              <td>{{ quest.questId }}</td>
              <td>{{ quest.questName }}</td>
              <td>{{ getQuestTypeLabel(quest.questType) }}</td>
              <td>{{ getResetCycleLabel(quest.resetCycle) }}</td>
              <td>{{ quest.isBuiltIn ? `内置 · ${quest.builtInVersion || '未标记版本'}` : '人工配置' }}</td>
              <td>Lv.{{ quest.requiredLevel }}</td>
              <td>{{ quest.isEnabled ? '启用' : '停用' }}</td>
            </tr>
          </tbody>
        </table>
      </div>

    </section>

    <AdminModal
      v-model="editorOpen"
      kicker="任务编辑"
      :title="selectedQuest?.questId ? `编辑任务 ${selectedQuest.questId}` : '新建任务'"
      description="维护任务类型、目标、奖励和前置依赖。"
      size="xwide"
    >
      <fieldset v-if="selectedQuest" class="form-fieldset" :disabled="!canManageConfigs">
        <div class="detail-overview detail-overview--compact">
          <article class="detail-overview__item"><span>来源</span><strong>{{ selectedQuest.isBuiltIn ? '系统内置' : '人工配置' }}</strong></article>
          <article class="detail-overview__item"><span>种子键</span><strong>{{ selectedQuest.seedKey || '无' }}</strong></article>
          <article class="detail-overview__item"><span>内置版本</span><strong>{{ selectedQuest.builtInVersion || '无' }}</strong></article>
          <article class="detail-overview__item"><span>最后更新</span><strong>{{ selectedQuest.lastUpdateTime || '未记录' }}</strong></article>
        </div>

        <div class="editor-grid editor-grid--three">
          <label class="field"><span>任务编号</span><input v-model.trim="selectedQuest.questId" type="text" /></label>
          <label class="field"><span>任务名称</span><input v-model.trim="selectedQuest.questName" type="text" /></label>
          <label class="field"><span>任务类型</span><select v-model.number="selectedQuest.questType"><option v-for="option in QUEST_TYPE_OPTIONS" :key="option.value" :value="option.value">{{ option.label }}</option></select></label>
          <label class="field"><span>重置周期</span><select v-model.number="selectedQuest.resetCycle"><option v-for="option in QUEST_RESET_CYCLE_OPTIONS" :key="option.value" :value="option.value">{{ option.label }}</option></select></label>
          <label class="field"><span>要求等级</span><input v-model.number="selectedQuest.requiredLevel" type="number" min="1" /></label>
          <label class="field"><span>时间限制</span><input v-model.number="selectedQuest.timeLimit" type="number" min="0" /></label>
          <label class="field"><span>排序值</span><input v-model.number="selectedQuest.sortOrder" type="number" /></label>
          <label class="field"><span>奖励经验</span><input v-model.number="selectedQuest.rewardExp" type="number" min="0" /></label>
          <label class="field"><span>奖励金币</span><input v-model.number="selectedQuest.rewardGold" type="number" min="0" /></label>
          <label class="field"><span>奖励灵石</span><input v-model.number="selectedQuest.rewardSpiritStone" type="number" min="0" /></label>
          <label class="field"><span>奖励贡献</span><input v-model.number="selectedQuest.rewardGuildContribution" type="number" min="0" /></label>
        </div>

        <div class="editor-grid">
          <label class="field checkbox-field"><input v-model="selectedQuest.autoAccept" type="checkbox" /><span>自动接取</span></label>
          <label class="field checkbox-field"><input v-model="selectedQuest.autoSubmit" type="checkbox" /><span>自动提交</span></label>
          <label class="field checkbox-field"><input v-model="selectedQuest.isEnabled" type="checkbox" /><span>启用任务</span></label>
        </div>

        <label class="field"><span>描述</span><textarea v-model="selectedQuest.description" rows="3"></textarea></label>
        <SelectableStringListEditor v-model="preQuestIdsDraft" title="前置任务" :options="questOptions" />
        <IdAmountListEditor v-model="rewardItemsDraft" title="奖励道具" id-label="道具编号" amount-label="数量" :options="itemOptions" />
        <SelectableStringListEditor v-model="rewardEquipmentIdsDraft" title="奖励装备" :options="equipmentOptions" />
        <QuestObjectivesEditor
          v-model="objectiveDraft"
          :monster-options="monsterOptions"
          :item-options="itemOptions"
          :map-options="mapOptions"
          :dungeon-options="dungeonOptions"
          :crop-options="cropOptions"
          :alchemy-recipe-options="alchemyRecipeOptions"
          :forge-recipe-options="forgeRecipeOptions"
        />
      </fieldset>

      <template #footer>
        <button class="secondary-button" type="button" @click="editorOpen = false">取消</button>
        <button class="danger-button" type="button" :disabled="!canManageConfigs || !selectedQuest?.questId" @click="removeQuest">删除</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="saveQuest">保存任务</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import { QUEST_RESET_CYCLE_OPTIONS, QUEST_TYPE_OPTIONS } from '@/constants/game-options'
import IdAmountListEditor from '@/components/editors/IdAmountListEditor.vue'
import QuestObjectivesEditor from '@/components/editors/QuestObjectivesEditor.vue'
import SelectableStringListEditor from '@/components/editors/SelectableStringListEditor.vue'
import toast from '@/utils/toast'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import { getAdminAlchemyRecipes } from '@/services/alchemy'
import { getAdminCrops } from '@/services/crops'
import { getAdminDungeons } from '@/services/dungeons'
import { getAdminEquipments } from '@/services/equipments'
import { getAdminForgeRecipes } from '@/services/forge'
import { getAdminItems } from '@/services/items'
import { getAdminMaps } from '@/services/maps'
import { getAdminMonsters } from '@/services/monsters'
import { deleteAdminQuest, getAdminQuestDetail, getAdminQuests, saveAdminQuest } from '@/services/quests'
import type {
  AdminAlchemyRecipeListItem,
  AdminCropListItem,
  AdminDungeonListItem,
  AdminEquipmentListItem,
  AdminForgeRecipeListItem,
  AdminItemListItem,
  AdminMapListItem,
  AdminMonsterListItem,
  AdminQuestDetail,
  AdminQuestListItem,
  IdAmountEntry,
  QuestObjectiveEntry
} from '@/types/admin'

// 任务页会同时依赖多种模板选项：
// 怪物、地图、副本、作物、炼丹配方、锻造配方、道具、装备。
const { canManageConfigs } = useAdminPermissions()
const quests = ref<AdminQuestListItem[]>([])
const equipmentTemplates = ref<AdminEquipmentListItem[]>([])
const monsterTemplates = ref<AdminMonsterListItem[]>([])
const itemTemplates = ref<AdminItemListItem[]>([])
const mapTemplates = ref<AdminMapListItem[]>([])
const dungeonTemplates = ref<AdminDungeonListItem[]>([])
const cropTemplates = ref<AdminCropListItem[]>([])
const alchemyRecipeTemplates = ref<AdminAlchemyRecipeListItem[]>([])
const forgeRecipeTemplates = ref<AdminForgeRecipeListItem[]>([])
const keyword = ref('')
const questTypeFilter = ref<number | ''>('')
const resetCycleFilter = ref<number | ''>('')
const selectedQuest = ref<AdminQuestDetail | null>(null)
const preQuestIdsDraft = ref<string[]>([])
const rewardEquipmentIdsDraft = ref<string[]>([])
const rewardItemsDraft = ref<IdAmountEntry[]>([])
const objectiveDraft = ref<QuestObjectiveEntry[]>([])
const editorOpen = ref(false)

// 后端已支持按任务类型和重置周期筛选。
const filteredQuests = quests

// 过滤掉当前自己，生成前置任务选择器选项。
const questOptions = computed(() => quests.value
  .filter((quest) => quest.questId !== selectedQuest.value?.questId)
  .map((quest) => ({ value: quest.questId, label: `${quest.questName} (${quest.questId})` })))

const equipmentOptions = computed(() => equipmentTemplates.value.map((equipment) => ({
  value: String(equipment.equipmentId),
  label: `${equipment.name} (${equipment.equipmentId})`
})))
const monsterOptions = computed(() => monsterTemplates.value.map((monster) => ({ value: monster.monsterId, label: `${monster.name} (${monster.monsterId})` })))
const itemOptions = computed(() => itemTemplates.value.map((item) => ({ value: item.itemId, label: `${item.name} (${item.itemId})` })))
const mapOptions = computed(() => mapTemplates.value
  .filter((map) => map.mapId.startsWith('map_'))
  .map((map) => ({ value: map.mapId, label: `${map.name} (${map.mapId})` })))
const dungeonOptions = computed(() => dungeonTemplates.value.map((dungeon) => ({ value: dungeon.dungeonId, label: `${dungeon.name} (${dungeon.dungeonId})` })))
const cropOptions = computed(() => cropTemplates.value.map((crop) => ({ value: crop.templateId, label: `${crop.name} (${crop.templateId})` })))
const alchemyRecipeOptions = computed(() => alchemyRecipeTemplates.value.map((recipe) => ({ value: recipe.recipeId, label: `${recipe.name} (${recipe.recipeId})` })))
const forgeRecipeOptions = computed(() => forgeRecipeTemplates.value.map((recipe) => ({ value: recipe.recipeId, label: `${recipe.name} (${recipe.recipeId})` })))

// 任务类型值转中文标签。
function getQuestTypeLabel(value: number) {
  return QUEST_TYPE_OPTIONS.find((option) => option.value === value)?.label || `类型 ${value}`
}

// 重置周期值转中文标签。
function getResetCycleLabel(value: number) {
  return QUEST_RESET_CYCLE_OPTIONS.find((option) => option.value === value)?.label || `周期 ${value}`
}

// 新建任务时的默认对象。
function createEmptyQuest(): AdminQuestDetail {
  return {
    questId: '',
    questName: '',
    questType: 0,
    resetCycle: 0,
    description: '',
    requiredLevel: 1,
    preQuestIds: '',
    autoAccept: false,
    autoSubmit: false,
    timeLimit: 0,
    rewardExp: 0,
    rewardGold: 0,
    rewardSpiritStone: 0,
    rewardGuildContribution: 0,
    rewardItemsJson: '',
    rewardEquipmentIds: '',
    objectivesJson: '',
    sortOrder: 0,
    isEnabled: true,
    isBuiltIn: false,
    seedKey: null,
    builtInVersion: null,
    lastUpdateTime: null
  }
}

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
  if (entries.length === 0) return null
  const payload: Record<string, number> = {}
  for (const entry of entries) {
    if (entry.id && entry.amount > 0) payload[entry.id] = entry.amount
  }
  return Object.keys(payload).length > 0 ? JSON.stringify(payload) : null
}

// 把 objectivesJson 解析成目标编辑器草稿。
function parseObjectives(value?: string | null) {
  if (!value) return []
  try {
    const parsed = JSON.parse(value) as Array<Record<string, unknown>>
    return parsed.map((item) => ({
      objectiveType: Number(item.ObjectiveType ?? item.objectiveType) || 0,
      targetId: String(item.TargetId ?? item.targetId ?? ''),
      targetCount: Number(item.TargetCount ?? item.targetCount) || 1,
      targetValue: Number(item.TargetValue ?? item.targetValue) || 0,
      description: String(item.Description ?? item.description ?? ''),
      activityType: Number(item.ActivityType ?? item.activityType) || 0
    })).filter((item) => item.targetId || item.targetValue > 0 || item.objectiveType === 9 || item.objectiveType === 10 || item.objectiveType === 13)
  } catch {
    return []
  }
}

// 把任务目标草稿重新序列化成 objectivesJson。
function serializeObjectives(entries: QuestObjectiveEntry[]) {
  const normalizedEntries = entries.filter((item) => {
    const objectiveType = Number(item.objectiveType) || 0
    if ([4, 5, 14].includes(objectiveType)) {
      return Number(item.targetValue) > 0
    }

    if ([9, 10, 13].includes(objectiveType)) {
      return true
    }

    return String(item.targetId || '').trim().length > 0
  })

  if (normalizedEntries.length === 0) return null
  return JSON.stringify(normalizedEntries.map((item) => ({
    ObjectiveType: item.objectiveType,
    TargetId: item.targetId,
    TargetCount: item.targetCount,
    TargetValue: item.targetValue,
    Description: item.description,
    ActivityType: item.activityType
  })))
}

// 根据当前任务详情同步“前置任务 / 奖励 / 目标”三份编辑草稿。
function syncQuestEditors(quest: AdminQuestDetail | null) {
  preQuestIdsDraft.value = splitCsv(quest?.preQuestIds)
  rewardEquipmentIdsDraft.value = splitCsv(quest?.rewardEquipmentIds)
  rewardItemsDraft.value = parseRewardItems(quest?.rewardItemsJson)
  objectiveDraft.value = parseObjectives(quest?.objectivesJson)
}

// 把当前编辑草稿回写到 selectedQuest。
function applyQuestEditors() {
  if (!selectedQuest.value) return
  selectedQuest.value.preQuestIds = joinCsv(preQuestIdsDraft.value)
  selectedQuest.value.rewardEquipmentIds = joinCsv(rewardEquipmentIdsDraft.value)
  selectedQuest.value.rewardItemsJson = serializeRewardItems(rewardItemsDraft.value)
  selectedQuest.value.objectivesJson = serializeObjectives(objectiveDraft.value)
}

// 打开新建任务弹窗。
function openCreateModal() {
  if (!canManageConfigs.value) return
  selectedQuest.value = createEmptyQuest()
  syncQuestEditors(selectedQuest.value)
      editorOpen.value = true
}

// 预加载装备下拉选项。
async function loadEquipmentOptions() {
  equipmentTemplates.value = await getAdminEquipments()
}

// 预加载任务目标编辑器依赖的所有模板选项。
async function loadObjectiveReferenceOptions() {
  const [monsters, items, maps, dungeons, crops, alchemyRecipes, forgeRecipes] = await Promise.all([
    getAdminMonsters(),
    getAdminItems(),
    getAdminMaps(),
    getAdminDungeons(),
    getAdminCrops(),
    getAdminAlchemyRecipes(),
    getAdminForgeRecipes()
  ])
  monsterTemplates.value = monsters
  itemTemplates.value = items
  mapTemplates.value = maps
  dungeonTemplates.value = dungeons
  cropTemplates.value = crops
  alchemyRecipeTemplates.value = alchemyRecipes
  forgeRecipeTemplates.value = forgeRecipes
}

// 拉取任务列表。
async function loadQuests() {
  const filterQuestType = questTypeFilter.value === '' ? null : Number(questTypeFilter.value)
  const filterResetCycle = resetCycleFilter.value === '' ? null : Number(resetCycleFilter.value)
  quests.value = await getAdminQuests(keyword.value, filterQuestType, filterResetCycle)
}

// 打开任务详情弹窗。
async function openEditModal(questId: string) {
  selectedQuest.value = await getAdminQuestDetail(questId)
  syncQuestEditors(selectedQuest.value)
      editorOpen.value = true
}

// 保存任务配置。
async function saveQuest() {
  if (!canManageConfigs.value || !selectedQuest.value) return
  applyQuestEditors()

  try {
    selectedQuest.value = await saveAdminQuest(selectedQuest.value)
    syncQuestEditors(selectedQuest.value)
    toast.success('任务配置保存成功。')
    editorOpen.value = false
    await loadQuests()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '任务配置保存失败。')
  }
}

// 删除任务配置。
async function removeQuest() {
  if (!canManageConfigs.value || !selectedQuest.value?.questId) return
  if (!window.confirm(`确认删除任务 ${selectedQuest.value.questId} 吗？`)) return

  try {
    await deleteAdminQuest(selectedQuest.value.questId)
    toast.success('任务配置删除成功。')
    selectedQuest.value = null
    syncQuestEditors(null)
    editorOpen.value = false
    await loadQuests()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '任务配置删除失败。')
  }
}

onMounted(async () => {
  await Promise.all([loadEquipmentOptions(), loadObjectiveReferenceOptions(), loadQuests()])
})
</script>
