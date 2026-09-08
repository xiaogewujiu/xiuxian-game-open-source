<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">技能</p>
        <h2 class="section-title section-title--small">技能模板</h2>
        <p class="section-note">技能模板统一改成表格展示，新增和编辑通过弹窗完成。</p>
      </div>

      <div class="filter-grid">
        <label class="field">
          <span>关键字</span>
          <input v-model.trim="keyword" type="text" placeholder="搜索技能编号或名称" />
        </label>
        <label class="field">
          <span>当前技能</span>
          <input :value="selectedSkill ? String(selectedSkill.skillId) : ''" type="text" placeholder="未选择" disabled />
        </label>
        <label class="field">
          <span>技能目录</span>
          <select v-model="catalog">
            <option value="current">当前技能</option>
            <option value="legacy">旧技能</option>
            <option value="all">全部技能</option>
          </select>
        </label>
        <label class="field">
          <span>冷却</span>
          <input :value="selectedSkill ? String(selectedSkill.cooldown) : ''" type="text" placeholder="全部" disabled />
        </label>

        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadSkills">查询</button>
          <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="openCreateModal">新建</button>
        </div>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">技能列表</p>
          <h3 class="section-title section-title--small">技能列表</h3>
        </div>
        <span class="selected-pill">共 {{ skills.length }} 条</span>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>等级</th>
              <th>技能名称</th>
              <th>可用职业</th>
              <th>目录</th>
              <th>来源</th>
              <th>冷却</th>
              <th>操作</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="skill in skills.filter((item) => item.skillLevel === 1)"
              :key="skill.skillId"
              :class="{ 'is-active': selectedSkill?.skillId === skill.skillId }"
            >
              <td>Lv.{{ skill.skillLevel }}</td>
              <td>{{ skill.name }}</td>
              <td>{{ skill.allowedProfessionText }}</td>
              <td>{{ skill.skillCatalog === 'legacy' ? '旧技能' : '当前' }}</td>
              <td>{{ skill.isBuiltIn ? '内置' : '人工' }}</td>
              <td>{{ skill.cooldown }}</td>
              <td class="table-actions">
                <button class="secondary-button secondary-button--sm" type="button" @click="openEditModal(skill.skillId)">编辑</button>
                <button class="primary-button primary-button--sm" type="button" @click="openNextLevels(skill)">后续等级</button>
              </td>
            </tr>
            <tr v-if="!skills.some((item) => item.skillLevel === 1)">
              <td colspan="7" class="empty-cell">暂无 Lv.1 技能</td>
            </tr>
          </tbody>
        </table>
      </div>

    </section>

    <AdminModal
      v-model="editorOpen"
      kicker="技能编辑"
      :title="selectedSkill?.skillId ? `编辑技能 #${selectedSkill.skillId}` : '新建技能'"
      description="维护技能目标、冷却、伤害类型、倍率和关联 Buff。"
      size="xwide"
    >
      <fieldset v-if="selectedSkill" class="form-fieldset" :disabled="!canManageConfigs">
        <div class="detail-overview">
          <article class="detail-overview__item"><span>技能等级</span><strong>Lv.{{ selectedSkill.skillLevel }}</strong></article>
          <article class="detail-overview__item"><span>上一级</span><strong>{{ selectedSkill.previousSkillId ? `#${selectedSkill.previousSkillId}` : '无' }}</strong></article>
          <article class="detail-overview__item"><span>下一级</span><strong>{{ selectedSkill.nextSkillId ? `#${selectedSkill.nextSkillId} ${selectedSkill.nextSkillName || ''}` : '未配置' }}</strong></article>
          <article class="detail-overview__item"><span>来源</span><strong>{{ selectedSkill.isBuiltIn ? '系统内置' : '人工配置' }}</strong></article>
          <article class="detail-overview__item"><span>目录</span><strong>{{ selectedSkill.skillCatalog === 'legacy' ? '旧技能' : '当前技能' }}</strong></article>
        </div>
        <div class="editor-grid editor-grid--three">
          <label class="field"><span>技能编号</span><input v-model.number="selectedSkill.skillId" type="number" min="1" /></label>
          <label class="field"><span>技能名称</span><input v-model.trim="selectedSkill.name" type="text" /></label>
          <label class="field"><span>目标类型</span><select v-model.number="selectedSkill.targetType"><option v-for="option in SKILL_TARGET_TYPE_OPTIONS" :key="option.value" :value="option.value">{{ option.label }}</option></select></label>
          <label class="field"><span>法力消耗</span><input v-model.number="selectedSkill.manaCost" type="number" min="0" /></label>
          <label class="field"><span>冷却</span><input v-model.number="selectedSkill.cooldown" type="number" min="0" /></label>
          <label class="field"><span>伤害类型</span><select v-model.number="selectedSkill.damageType"><option v-for="option in DAMAGE_TYPE_OPTIONS" :key="option.value" :value="option.value">{{ option.label }}</option></select></label>
          <label class="field"><span>段数</span><input v-model.number="selectedSkill.hitCount" type="number" min="0" /></label>
          <label class="field"><span>范围</span><select v-model.number="selectedSkill.rangeType"><option v-for="option in SKILL_RANGE_TYPE_OPTIONS" :key="option.value" :value="option.value">{{ option.label }}</option></select></label>
          <label class="field"><span>默认倍率</span><input v-model.number="selectedSkill.damageMultiplier" type="number" step="0.01" /></label>
          <label class="field"><span>触发概率</span><input v-model.number="selectedSkill.triggerChance" type="number" min="0" step="0.01" /></label>
        </div>

        <label class="field"><span>描述</span><textarea v-model="selectedSkill.description" rows="3"></textarea></label>
        <SelectableStringListEditor v-model="selectedSkill.allowedProfessions" title="可用职业" :options="professionOptions" />
        <SelectableStringListEditor v-model="selectedSkill.buffIds" title="关联 Buff 列表" :options="buffOptions" />
        <SkillHitsEditor v-model="selectedSkill.hits" />
      </fieldset>

      <template #footer>
        <button class="secondary-button" type="button" @click="editorOpen = false">取消</button>
        <button class="danger-button" type="button" :disabled="!canManageConfigs || !selectedSkill?.skillId" @click="removeSkill">删除</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="saveSkill">保存技能</button>
      </template>
    </AdminModal>
    <AdminModal
      v-model="nextLevelsOpen"
      kicker="后续等级"
      :title="selectedSkill ? `${selectedSkill.name} Lv.${selectedSkill.skillLevel} · 后续等级` : '后续等级'"
      description="查看当前技能之后的所有等级，点击某个等级进入编辑。"
      size="wide"
    >
      <div class="next-levels-modal">
        <div v-if="nextLevelsLoading" class="empty-cell">正在加载后续等级...</div>
        <div v-else-if="nextLevels.length === 0" class="empty-cell">当前暂无后续等级。</div>
        <div v-else class="next-levels-table-wrap">
          <table class="data-table">
            <thead>
              <tr><th>等级</th><th>技能名称</th><th>技能编号</th><th>法力</th><th>冷却</th><th>升级条件</th><th>操作</th></tr>
            </thead>
            <tbody>
              <tr v-for="level in nextLevels" :key="level.skillId">
                <td>Lv.{{ level.skillLevel }}</td>
                <td>{{ level.name }}</td>
                <td>#{{ level.skillId }}</td>
                <td>{{ level.manaCost }}</td>
                <td>{{ level.cooldown }}</td>
                <td>{{ formatUpgradeConditionSummary(level) }}</td>
                <td class="table-actions">
                    <button class="secondary-button secondary-button--sm" type="button" @click="openNextLevelEditor(level.skillId)">编辑</button>
                    <button class="primary-button primary-button--sm" type="button" @click="openConditionsEditor(level)">升级条件</button>
                    <button class="danger-button danger-button--sm" type="button" @click="removeNextLevel(level)">删除</button>
                  </td>
              </tr>
            </tbody>
          </table>
        </div>
        <div class="next-levels-footer">
          <button class="secondary-button" type="button" :disabled="!selectedSkill?.skillId" @click="copyNextFromList">复制创建下一级</button>
        </div>
      </div>
    </AdminModal>
    <AdminModal
      v-model="conditionsOpen"
      kicker="升级条件"
      :title="conditionsTitle"
      description="配置当前等级升级到列表中下一级技能所需的全部条件。"
      size="default"
    >
      <div class="upgrade-conditions-modal">
        <div v-if="conditionDraft.length === 0" class="empty-cell">当前未配置升级条件。</div>
        <div v-for="(condition, index) in conditionDraft" :key="index" class="skill-upgrade-condition-row">
          <label class="field"><span>条件类型</span><select v-model.number="condition.type" @change="onUpgradeConditionTypeChanged(condition)"><option v-for="option in SKILL_UPGRADE_CONDITION_TYPE_OPTIONS" :key="option.value" :value="option.value">{{ option.label }}</option></select></label>
          <label v-if="condition.type === 3" class="field"><span>升级道具</span><select v-model="condition.itemId"><option value="">请选择道具</option><option v-for="item in itemOptions" :key="item.itemId" :value="item.itemId">{{ item.name }}（{{ item.itemId }}）</option></select></label>
          <div v-else class="field"><span>对应资源</span><div class="field-readonly">{{ getUpgradeConditionTarget(condition.type) }}</div></div>
          <label class="field"><span>{{ condition.type === 4 ? '最低玩家等级' : '数量' }}</span><input v-model.number="condition.amount" type="number" min="1" /></label>
          <button class="danger-button danger-button--sm" type="button" @click="removeUpgradeCondition(index)">删除</button>
        </div>
        <button class="secondary-button" type="button" :disabled="!canManageConfigs" @click="addConditionToDraft">新增条件</button>
      </div>
      <template #footer>
        <button class="secondary-button" type="button" @click="conditionsOpen = false">取消</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs || !conditionSourceSkillId || conditionsSaving" @click="saveConditions">保存条件</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import { DAMAGE_TYPE_OPTIONS, SKILL_RANGE_TYPE_OPTIONS, SKILL_TARGET_TYPE_OPTIONS } from '@/constants/game-options'

const SKILL_UPGRADE_CONDITION_TYPE_OPTIONS = [
  { value: 1, label: '金币' },
  { value: 2, label: '灵石' },
  { value: 3, label: '道具' },
  { value: 4, label: '玩家等级' }
]
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import type { AdminBuffListItem, AdminItemListItem, AdminSkillDetail, AdminSkillListItem, AdminSkillNextLevel, SkillUpgradeCondition } from '@/types/admin'
import { deleteAdminSkill, getAdminBuffs, getAdminSkillDetail, getAdminSkills, getAdminSkillNextLevels, saveAdminSkill, copyAdminSkillNext, deleteAdminSkillNextChain, updateAdminSkillUpgradeConditions } from '@/services/skills-buffs'
import { getAdminItems } from '@/services/items'
import SelectableStringListEditor from '@/components/editors/SelectableStringListEditor.vue'
import SkillHitsEditor from '@/components/editors/SkillHitsEditor.vue'
import toast from '@/utils/toast'

// 技能页的编辑器依赖 Buff 下拉选项与职业下拉选项，
// 所以进入页面时会先把技能列表和 Buff 列表一起预加载。
const { canManageConfigs } = useAdminPermissions()
const skills = ref<AdminSkillListItem[]>([])
const buffs = ref<AdminBuffListItem[]>([])
const items = ref<AdminItemListItem[]>([])
const keyword = ref('')
const catalog = ref('current')
const selectedSkill = ref<AdminSkillDetail | null>(null)
const editorOpen = ref(false)
const nextLevelsOpen = ref(false)
const nextLevels = ref<AdminSkillNextLevel[]>([])
const nextLevelsLoading = ref(false)
const conditionsOpen = ref(false)
const conditionsSaving = ref(false)
const conditionDraft = ref<SkillUpgradeCondition[]>([])
const conditionSourceSkillId = ref(0)
const conditionSourceName = ref('')
const conditionTargetName = ref('')
const conditionsTitle = computed(() => `${conditionSourceName.value} → ${conditionTargetName.value} · 升级条件`)

async function openNextLevels(skill: AdminSkillListItem) {
  const detail = await getAdminSkillDetail(skill.skillId)
  selectedSkill.value = detail as unknown as AdminSkillDetail
  nextLevelsLoading.value = true
  nextLevelsOpen.value = true
  try {
    nextLevels.value = await getAdminSkillNextLevels(skill.skillId)
  } catch (error) {
    nextLevels.value = []
    toast.error(error instanceof Error ? error.message : '加载后续等级失败。')
  } finally {
    nextLevelsLoading.value = false
  }
}

async function openNextLevelEditor(skillId: number) {
  nextLevelsOpen.value = false
  await openEditModal(skillId)
}

async function openConditionsEditor(level: AdminSkillNextLevel) {
  const levelIndex = nextLevels.value.findIndex((item) => item.skillId === level.skillId)
  const sourceSkillId = level.previousSkillId
    || (levelIndex === 0 ? selectedSkill.value?.skillId : nextLevels.value[levelIndex - 1]?.skillId)
  if (!sourceSkillId) {
    toast.error('无法确定升级条件所属的当前技能。')
    return
  }

  const sourceDetail = await getAdminSkillDetail(sourceSkillId)
  conditionSourceSkillId.value = sourceSkillId
  conditionSourceName.value = sourceDetail.name
  conditionTargetName.value = `${level.name} Lv.${level.skillLevel}`
  conditionDraft.value = sourceDetail.upgradeConditions.map((condition) => ({ ...condition }))
  conditionsOpen.value = true
}

async function copyNextFromList() {
  if (!selectedSkill.value?.skillId) return
  try {
    const sourceSkillId = nextLevels.value.length
      ? nextLevels.value[nextLevels.value.length - 1].skillId
      : selectedSkill.value.skillId
    await copyAdminSkillNext(sourceSkillId)
    const rootSkill = await getAdminSkillDetail(selectedSkill.value.skillId) as unknown as AdminSkillDetail
    await openNextLevels({ skillId: rootSkill.skillId, name: rootSkill.name, skillLevel: rootSkill.skillLevel, targetType: rootSkill.targetType, cooldown: rootSkill.cooldown, allowedProfessionText: '', isBuiltIn: rootSkill.isBuiltIn })
    toast.success('下一级技能创建成功。')
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '创建下一级技能失败。')
  }
}

async function removeNextLevel(level: AdminSkillNextLevel) {
  if (!selectedSkill.value?.skillId) return
  if (!window.confirm(`确认删除 Lv.${level.skillLevel} 及其后续等级吗？\n前置技能 Lv.${selectedSkill.value.skillLevel} 将保留。`)) return
  try {
    await deleteAdminSkillNextChain(level.skillId)
    await openNextLevels({ skillId: selectedSkill.value.skillId, name: selectedSkill.value.name, skillLevel: selectedSkill.value.skillLevel, targetType: selectedSkill.value.targetType, cooldown: selectedSkill.value.cooldown, allowedProfessionText: '', isBuiltIn: selectedSkill.value.isBuiltIn })
    toast.success('当前等级及后续等级删除成功。')
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '删除后续等级失败。')
  }
}


// 统一格式化后台时间字段。
function formatDateTime(value?: string | null) {
  if (!value) return '-'
  return value.replace('T', ' ').slice(0, 19)
}

// Buff 选择器使用的选项结构。
const buffOptions = computed(() => buffs.value.map((buff) => ({
  value: buff.buffId,
  label: `${buff.name} (${buff.buffId})`
})))

const itemOptions = computed(() => items.value.filter((item) => item.type !== 13))

function formatUpgradeConditionSummary(level: AdminSkillNextLevel) {
  if (!level.upgradeConditions?.length) return level.upgradeConditionSummary
  return level.upgradeConditions
    .filter((condition) => condition.amount > 0)
    .map((condition) => {
      if (condition.type === 1) return `金币 ${condition.amount}`
      if (condition.type === 2) return `灵石 ${condition.amount}`
      if (condition.type === 4) return `等级 ${condition.amount}`
      const item = itemOptions.value.find((entry) => entry.itemId === condition.itemId)
      return `道具 ${item?.name || condition.itemId || '未选择道具'}×${condition.amount}`
    })
    .join('、') || '无条件'
}

// 当前三职业固定选项。

const professionOptions = [
  { value: 'all', label: '全职业' },
  { value: 'warrior', label: '战' },
  { value: 'mage', label: '法' },
  { value: 'body', label: '体' }
]

// 新建技能时的默认对象。
function createEmptySkill(): AdminSkillDetail {
  return {
    skillId: 0,
    name: '',
    description: '',
    skillLevel: 1,
    previousSkillId: null,
    nextSkillId: null,
    nextSkillName: null,
    upgradeConditions: [],
    targetType: 1,
    manaCost: 0,
    cooldown: 0,
    damageType: 0,
    hitCount: 0,
    rangeType: 1,
    damageMultiplier: 0,
    triggerChance: 1,
    hits: [],
    buffIds: [],
    allowedProfessions: ['all'],
    isBuiltIn: false,
    seedKey: null,
    builtInVersion: null,
    lastUpdateTime: null
  }
}

// 打开新建弹窗。
function openCreateModal() {
  if (!canManageConfigs.value) return
  selectedSkill.value = createEmptySkill()
      editorOpen.value = true
}

// 预加载 Buff 下拉选项。
async function loadBuffOptions() {
  buffs.value = await getAdminBuffs()
}

// 拉取技能列表。
async function loadSkills() {
  skills.value = await getAdminSkills(keyword.value, catalog.value)
}

// 打开技能详情弹窗。
async function openEditModal(skillId: number) {
  const detail = await getAdminSkillDetail(skillId)
  selectedSkill.value = detail as AdminSkillDetail
  editorOpen.value = true
}

function addConditionToDraft() {
  conditionDraft.value.push({ type: 1, amount: 0, itemId: null })
}

async function saveConditions() {
  if (!canManageConfigs.value || !conditionSourceSkillId.value) return
  conditionsSaving.value = true
  try {
    const saved = await updateAdminSkillUpgradeConditions(conditionSourceSkillId.value, conditionDraft.value)
    const level = nextLevels.value.find((item) => item.previousSkillId === conditionSourceSkillId.value && item.skillId === selectedSkill.value?.nextSkillId)
    if (level) {
      level.upgradeConditions = saved.upgradeConditions
    }
    await openNextLevels({ skillId: selectedSkill.value!.skillId, name: selectedSkill.value!.name, skillLevel: selectedSkill.value!.skillLevel, targetType: selectedSkill.value!.targetType, cooldown: selectedSkill.value!.cooldown, allowedProfessionText: '', isBuiltIn: selectedSkill.value!.isBuiltIn })
    conditionsOpen.value = false
    toast.success('技能升级条件保存成功。')
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '技能升级条件保存失败。')
  } finally {
    conditionsSaving.value = false
  }
}

// 复制当前技能创建下一级技能。
async function copyNextSkill() {
  if (!canManageConfigs.value || !selectedSkill.value?.skillId) return
  try {
    selectedSkill.value = await copyAdminSkillNext(selectedSkill.value.skillId)
    await loadSkills()
    toast.success('下一级技能创建成功，请继续配置技能属性和升级条件。')
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '创建下一级技能失败。')
  }
}

// 删除当前技能及其全部后续等级。
async function deleteNextChain() {
  if (!canManageConfigs.value || !selectedSkill.value?.skillId) return
  if (!window.confirm(`确认删除技能 #${selectedSkill.value.skillId} 及其后续等级吗？`)) return
  try {
    await deleteAdminSkillNextChain(selectedSkill.value.skillId)
    selectedSkill.value = null
    editorOpen.value = false
    await loadSkills()
    toast.success('当前技能及后续等级删除成功。')
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '删除技能链失败。')
  }
}

// 添加一条技能升级条件。
function addUpgradeCondition() {
  if (!selectedSkill.value) return
  selectedSkill.value.upgradeConditions.push({ type: 1, amount: 0, itemId: null } satisfies SkillUpgradeCondition)
}

// 切换条件类型时清理不再适用的道具编号。
function onUpgradeConditionTypeChanged(condition: SkillUpgradeCondition) {
  if (condition.type !== 3) condition.itemId = null
}

// 显示金币、灵石和玩家等级条件对应的资源名称。
function getUpgradeConditionTarget(type: number) {
  return SKILL_UPGRADE_CONDITION_TYPE_OPTIONS.find((option) => option.value === type)?.label || '未知条件'
}

function removeUpgradeCondition(index: number) {
  conditionDraft.value.splice(index, 1)
}


// 保存技能模板。
async function saveSkill() {
  if (!canManageConfigs.value || !selectedSkill.value) return

  try {
    selectedSkill.value = await saveAdminSkill(selectedSkill.value)
    toast.success('技能模板保存成功。')
    editorOpen.value = false
    await loadSkills()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '技能模板保存失败。')
  }
}

// 删除技能模板。
async function removeSkill() {
  if (!canManageConfigs.value || !selectedSkill.value?.skillId) return
  if (!window.confirm(`确认删除技能模板 #${selectedSkill.value.skillId} 吗？`)) return

  try {
    await deleteAdminSkill(selectedSkill.value.skillId)
    toast.success('技能模板删除成功。')
    selectedSkill.value = null
    editorOpen.value = false
    await loadSkills()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '技能模板删除失败。')
  }
}

onMounted(async () => {
  const itemListPromise = getAdminItems()
  await loadBuffOptions()
  items.value = await itemListPromise
  await loadSkills()
})
</script>

<style scoped>
.upgrade-conditions-modal {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.upgrade-conditions-modal .skill-upgrade-condition-row {
  grid-template-columns: 1fr 1.5fr 1fr auto;
}

.skill-upgrade-conditions-editor {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.skill-upgrade-condition-row {
  display: grid;
  grid-template-columns: 1fr 1.5fr 1fr auto;
  align-items: end;
  gap: 12px;
  padding: 12px;
  border: 1px solid var(--border-color);
  border-radius: 10px;
  background: var(--panel-soft-bg, var(--bg-overlay-light));
}

.skill-upgrade-condition-placeholder {
  min-width: 0;
}

.field-readonly {
  min-height: 38px;
  display: flex;
  align-items: center;
  padding: 8px 10px;
  border: 1px solid var(--control-border);
  border-radius: 8px;
  color: var(--text-secondary);
  background: var(--control-bg);
}

.skill-upgrade-condition-remove {
  margin-bottom: 1px;
}

@media (max-width: 800px) {
  .skill-upgrade-condition-row {
    grid-template-columns: 1fr 1fr;
  }
}
</style>
