<template>
  <div class="dashboard-grid five-element-rules-page">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">聚灵阵规则</p>
        <h2 class="section-title section-title--small">聚灵阵规则</h2>
        <p class="section-note">维护聚灵阵主等级加成、升级成本和五行等级区间规则。</p>
      </div>

      <div class="filter-grid">
        <label class="field">
          <span>五行筛选</span>
          <select v-model="elementFilter">
            <option value="">全部</option>
            <option v-for="option in elementOptions" :key="option.value" :value="option.value">{{ option.label }}</option>
          </select>
        </label>
        <label class="field">
          <span>当前规则</span>
          <input :value="selectedRangeRule?.gid || selectedLevelRule?.arrayLevel || ''" type="text" placeholder="未选择" disabled />
        </label>
        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadData">刷新</button>
          <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="openCreateLevelModal">新建主等级规则</button>
          <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="openCreateRangeModal">新建区间规则</button>
        </div>
      </div>
    </section>

    <section class="dashboard-section-grid">
      <section class="panel-box table-panel">
        <div class="section-header-row">
          <div class="section-header-copy">
            <p class="section-kicker">主等级规则</p>
            <h3 class="section-title section-title--small">主等级规则</h3>
          </div>
          <span class="selected-pill">共 {{ levelRules.length }} 条</span>
        </div>

        <div class="table-wrap five-element-rules-page__table-wrap">
          <table class="data-table">
            <thead>
              <tr>
                <th>等级</th>
                <th>金币</th>
                <th>灵石</th>
                <th>灵田加成</th>
                <th>战斗经验</th>
                <th>职业上限</th>
                <th>来源</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="rule in levelRules"
                :key="rule.arrayLevel"
                :class="{ 'is-active': selectedLevelRule?.arrayLevel === rule.arrayLevel }"
                @click="openEditLevelModal(rule.arrayLevel)"
              >
                <td>Lv.{{ rule.arrayLevel }}</td>
                <td>{{ rule.upgradeGoldCost }}</td>
                <td>{{ rule.upgradeSpiritStoneCost }}</td>
                <td>+{{ rule.spiritFieldYieldBonusPercent }}%</td>
                <td>+{{ rule.battleExpBonusPercent }}%</td>
                <td>Lv.{{ rule.professionLevelCap }}</td>
                <td>{{ rule.isBuiltIn ? `内置 · ${rule.builtInVersion || '未标记版本'}` : '人工配置' }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>

      <section class="panel-box table-panel">
        <div class="section-header-row">
          <div class="section-header-copy">
            <p class="section-kicker">分支规则</p>
            <h3 class="section-title section-title--small">五行等级区间规则</h3>
          </div>
          <span class="selected-pill">共 {{ branchRules.length }} 条</span>
        </div>

        <div class="table-wrap five-element-rules-page__table-wrap">
          <table class="data-table">
            <thead>
              <tr>
                <th>五行</th>
                <th>等级区间</th>
                <th>属性</th>
                <th>每级加成</th>
                <th>金币</th>
                <th>灵石</th>
                <th>来源</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="rule in branchRules"
                :key="rule.gid"
                :class="{ 'is-active': selectedRangeRule?.gid === rule.gid }"
                @click="openEditBranchModal(rule.gid)"
              >
                <td>{{ elementLabel(rule.elementType) }}</td>
                <td>Lv.{{ rule.minLevel }} - Lv.{{ rule.maxLevel }}</td>
                <td>{{ attributeLabel(rule.attributeType) }}</td>
                <td>+{{ rule.bonusPerLevel }}</td>
                <td>{{ rule.goldCost }}</td>
                <td>{{ rule.spiritStoneCost }}</td>
                <td>{{ rule.isBuiltIn ? `内置 · ${rule.builtInVersion || '未标记版本'}` : '人工配置' }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </section>

    <AdminModal
      v-model="levelEditorOpen"
      kicker="主等级规则"
      :title="selectedLevelRule ? `主等级 Lv.${selectedLevelRule.arrayLevel}` : '新建主等级规则'"
      description="维护聚灵阵主等级的升级成本和对应加成。"
      size="xwide"
    >
      <fieldset v-if="selectedLevelRule" class="form-fieldset" :disabled="!canManageConfigs">
        <div class="detail-overview detail-overview--compact">
          <article class="detail-overview__item"><span>来源</span><strong>{{ selectedLevelRule.isBuiltIn ? '系统内置' : '人工配置' }}</strong></article>
          <article class="detail-overview__item"><span>种子键</span><strong>{{ selectedLevelRule.seedKey || '无' }}</strong></article>
          <article class="detail-overview__item"><span>内置版本</span><strong>{{ selectedLevelRule.builtInVersion || '无' }}</strong></article>
          <article class="detail-overview__item"><span>最后更新</span><strong>{{ selectedLevelRule.lastUpdateTime || '未记录' }}</strong></article>
        </div>

        <div class="editor-grid editor-grid--three">
          <label class="field"><span>等级</span><input v-model.number="selectedLevelRule.arrayLevel" type="number" min="1" max="50" /></label>
          <label class="field"><span>金币消耗</span><input v-model.number="selectedLevelRule.upgradeGoldCost" type="number" min="0" /></label>
          <label class="field"><span>灵石消耗</span><input v-model.number="selectedLevelRule.upgradeSpiritStoneCost" type="number" min="0" /></label>
          <label class="field"><span>灵田加成</span><input v-model.number="selectedLevelRule.spiritFieldYieldBonusPercent" type="number" min="0" /></label>
          <label class="field"><span>战斗经验加成</span><input v-model.number="selectedLevelRule.battleExpBonusPercent" type="number" min="0" /></label>
          <label class="field"><span>职业上限</span><input v-model.number="selectedLevelRule.professionLevelCap" type="number" min="1" /></label>
          <label class="field checkbox-field"><input v-model="selectedLevelRule.isEnabled" type="checkbox" /><span>启用规则</span></label>
        </div>

        <IdAmountListEditor v-model="levelMaterialsDraft" title="升级材料" id-label="道具编号" amount-label="数量" :options="itemOptions" />
      </fieldset>

      <template #footer>
        <button class="secondary-button" type="button" @click="levelEditorOpen = false">取消</button>
        <button class="danger-button" type="button" :disabled="!canManageConfigs || !selectedLevelRule" @click="removeLevelRule">删除</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="saveLevelRule">保存规则</button>
      </template>
    </AdminModal>

    <AdminModal
      v-model="branchEditorOpen"
      kicker="五行区间规则"
      :title="selectedRangeRule ? `${elementLabel(selectedRangeRule.elementType)}系 Lv.${selectedRangeRule.minLevel}-${selectedRangeRule.maxLevel}` : '新建区间规则'"
      description="维护五行等级区间、属性类型、每级加成和升级成本。启用规则必须覆盖 1～500 且不能重叠。"
      size="xwide"
    >
      <fieldset v-if="selectedRangeRule" class="form-fieldset" :disabled="!canManageConfigs">
        <div class="detail-overview detail-overview--compact">
          <article class="detail-overview__item"><span>来源</span><strong>{{ selectedRangeRule.isBuiltIn ? '系统内置' : '人工配置' }}</strong></article>
          <article class="detail-overview__item"><span>种子键</span><strong>{{ selectedRangeRule.seedKey || '无' }}</strong></article>
          <article class="detail-overview__item"><span>类型</span><strong>{{ attributeLabel(selectedRangeRule.attributeType) }}</strong></article>
          <article class="detail-overview__item"><span>最后更新</span><strong>{{ selectedRangeRule.lastUpdateTime || '未记录' }}</strong></article>
        </div>
        <div class="editor-grid editor-grid--three">
          <label class="field"><span>五行</span><select v-model="selectedRangeRule.elementType"><option v-for="option in elementOptions" :key="option.value" :value="option.value">{{ option.label }}</option></select></label>
          <label class="field"><span>起始等级</span><input v-model.number="selectedRangeRule.minLevel" type="number" min="1" max="500" /></label>
          <label class="field"><span>结束等级</span><input v-model.number="selectedRangeRule.maxLevel" type="number" min="1" max="500" /></label>
          <label class="field"><span>加成属性</span><select v-model="selectedRangeRule.attributeType"><option v-for="option in attributeOptions" :key="option.value" :value="option.value">{{ option.label }}</option></select></label>
          <label class="field"><span>每级加成</span><input v-model.number="selectedRangeRule.bonusPerLevel" type="number" min="0" /></label>
          <label class="field"><span>排序</span><input v-model.number="selectedRangeRule.sortOrder" type="number" min="0" /></label>
          <label class="field"><span>金币消耗</span><input v-model.number="selectedRangeRule.goldCost" type="number" min="0" /></label>
          <label class="field"><span>灵石消耗</span><input v-model.number="selectedRangeRule.spiritStoneCost" type="number" min="0" /></label>
          <label class="field checkbox-field"><input v-model="selectedRangeRule.isEnabled" type="checkbox" /><span>启用规则</span></label>
        </div>
        <IdAmountListEditor v-model="rangeMaterialsDraft" title="升级材料" id-label="道具编号" amount-label="数量" :options="itemOptions" />
      </fieldset>
      <template #footer>
        <button class="secondary-button" type="button" @click="branchEditorOpen = false">取消</button>
        <button class="danger-button" type="button" :disabled="!canManageConfigs || !selectedRangeRule?.gid" @click="removeRangeRule">删除</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="saveRangeRule">保存规则</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import IdAmountListEditor from '@/components/editors/IdAmountListEditor.vue'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import { deleteAdminFiveElementBranchRuleRange, deleteAdminFiveElementLevelRule, getAdminFiveElementBranchRuleRangeDetail, getAdminFiveElementBranchRuleRanges, getAdminFiveElementLevelRuleDetail, getAdminFiveElementLevelRules, saveAdminFiveElementBranchRuleRange, saveAdminFiveElementLevelRule } from '@/services/five-elements'
import { getAdminItems } from '@/services/items'
import type { AdminFiveElementBranchRuleRangeDetail, AdminFiveElementBranchRuleRangeListItem, AdminFiveElementLevelRuleDetail, AdminFiveElementLevelRuleListItem, AdminItemListItem, IdAmountEntry } from '@/types/admin'
import toast from '@/utils/toast'

// 页面主状态：
// levelRules 是主等级规则，branchRules 是五行分支规则，
// levelMaterialsDraft / branchMaterialsDraft 用于弹窗里的材料编辑草稿。
const { canManageConfigs } = useAdminPermissions()
const levelRules = ref<AdminFiveElementLevelRuleListItem[]>([])
const branchRules = ref<AdminFiveElementBranchRuleRangeListItem[]>([])
const itemTemplates = ref<AdminItemListItem[]>([])
const elementFilter = ref('')
const selectedLevelRule = ref<AdminFiveElementLevelRuleDetail | null>(null)
const selectedRangeRule = ref<AdminFiveElementBranchRuleRangeDetail | null>(null)
const levelMaterialsDraft = ref<IdAmountEntry[]>([])
const rangeMaterialsDraft = ref<IdAmountEntry[]>([])
const levelEditorOpen = ref(false)
const branchEditorOpen = ref(false)
const itemOptions = computed(() => itemTemplates.value.map((item) => ({ value: item.itemId, label: `${item.name} (${item.itemId})` })))

const attributeOptions = [
  { value: 'Type1', label: 'Type1 最大生命' },
  { value: 'Type2', label: 'Type2 最大灵力' },
  { value: 'Type3', label: 'Type3 物理攻击' },
  { value: 'Type4', label: 'Type4 法术攻击' },
  { value: 'Type5', label: 'Type5 物理防御' },
  { value: 'Type6', label: 'Type6 法术防御' },
  { value: 'Type7', label: 'Type7 速度' },
  { value: 'Type8', label: 'Type8 命中率' },
  { value: 'Type9', label: 'Type9 闪避率' },
  { value: 'Type10', label: 'Type10 暴击率' },
  { value: 'Type11', label: 'Type11 暴击伤害' },
  { value: 'Type12', label: 'Type12 连击率' },
  { value: 'Type13', label: 'Type13 反击率' },
  { value: 'Type14', label: 'Type14 破甲率' },
  { value: 'Type15', label: 'Type15 额外伤害' }
]

// 将后端属性类型映射为管理端显示的中文名称，保存值仍保持 Type1～Type15。
function attributeLabel(value: string) {
  return attributeOptions.find((item) => item.value === value)?.label || value
}

const elementOptions = [
  { value: 'metal', label: '金' },
  { value: 'wood', label: '木' },
  { value: 'water', label: '水' },
  { value: 'fire', label: '火' },
  { value: 'earth', label: '土' }
]

// 元素类型值转中文标签。
function elementLabel(value: string) {
  return elementOptions.find((item) => item.value === value)?.label || value
}

// 把材料 JSON 解析成编辑器使用的 id/amount 结构。
function parseMaterials(value?: string | null) {
  if (!value) return []
  try {
    const parsed = JSON.parse(value) as Array<Record<string, unknown>>
    return parsed.map((item) => ({ id: String(item.ItemId ?? item.itemId ?? ''), amount: Number(item.Count ?? item.count) || 0 })).filter((item) => item.id && item.amount > 0)
  } catch {
    return []
  }
}

// 把材料编辑器草稿重新序列化成后端使用的 JSON。
function serializeMaterials(entries: IdAmountEntry[]) {
  const normalized = entries
    .map((item) => ({ ItemId: String(item.id || '').trim(), Count: Math.max(0, Number(item.amount) || 0) }))
    .filter((item) => item.ItemId && item.Count > 0)
  return normalized.length > 0 ? JSON.stringify(normalized) : '[]'
}

async function loadData() {
  levelRules.value = await getAdminFiveElementLevelRules()
  branchRules.value = await getAdminFiveElementBranchRuleRanges(elementFilter.value)
}

async function loadItemOptions() {
  itemTemplates.value = await getAdminItems()
}

async function openEditLevelModal(arrayLevel: number) {
  selectedLevelRule.value = await getAdminFiveElementLevelRuleDetail(arrayLevel)
  levelMaterialsDraft.value = parseMaterials(selectedLevelRule.value?.upgradeMaterialsJson)
  levelEditorOpen.value = true
}

function openCreateLevelModal() {
  selectedLevelRule.value = {
    arrayLevel: 1,
    upgradeGoldCost: 0,
    upgradeSpiritStoneCost: 0,
    upgradeMaterialsJson: '[]',
    spiritFieldYieldBonusPercent: 0,
    battleExpBonusPercent: 0,
    professionLevelCap: 1,
    isEnabled: true,
    isBuiltIn: false,
    seedKey: null,
    builtInVersion: null,
    lastUpdateTime: null
  }
  levelMaterialsDraft.value = []
  levelEditorOpen.value = true
}

async function openEditBranchModal(gid: string) {
  selectedRangeRule.value = await getAdminFiveElementBranchRuleRangeDetail(gid)
  rangeMaterialsDraft.value = parseMaterials(selectedRangeRule.value?.materialsJson)
  branchEditorOpen.value = true
}

function openCreateRangeModal() {
  selectedRangeRule.value = {
    gid: '', elementType: 'metal', minLevel: 1, maxLevel: 500,
    attributeType: 'Type3', bonusPerLevel: 0, goldCost: 0, spiritStoneCost: 0,
    materialsJson: '[]', sortOrder: 0, isEnabled: true, isBuiltIn: false,
    seedKey: null, builtInVersion: null, lastUpdateTime: null
  }
  rangeMaterialsDraft.value = []
  branchEditorOpen.value = true
}

async function saveLevelRule() {
  if (!canManageConfigs.value || !selectedLevelRule.value) return
  selectedLevelRule.value.upgradeMaterialsJson = serializeMaterials(levelMaterialsDraft.value)
  try {
    selectedLevelRule.value = await saveAdminFiveElementLevelRule(selectedLevelRule.value)
    toast.success('聚灵阵主等级规则保存成功。')
    levelEditorOpen.value = false
    await loadData()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '聚灵阵主等级规则保存失败。')
  }
}

async function removeLevelRule() {
  if (!canManageConfigs.value || !selectedLevelRule.value) return
  if (!window.confirm(`确认删除 Lv.${selectedLevelRule.value.arrayLevel} 的主等级规则吗？`)) return
  try {
    await deleteAdminFiveElementLevelRule(selectedLevelRule.value.arrayLevel)
    toast.success('聚灵阵主等级规则删除成功。')
    levelEditorOpen.value = false
    await loadData()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '聚灵阵主等级规则删除失败。')
  }
}

async function saveRangeRule() {
  if (!canManageConfigs.value || !selectedRangeRule.value) return
  if (selectedRangeRule.value.minLevel < 1 || selectedRangeRule.value.minLevel > selectedRangeRule.value.maxLevel || selectedRangeRule.value.maxLevel > 500) {
    toast.error('等级区间必须满足 1 ≤ 起始等级 ≤ 结束等级 ≤ 500。')
    return
  }
  if (selectedRangeRule.value.bonusPerLevel < 0) {
    toast.error('每级加成不能为负数。')
    return
  }
  selectedRangeRule.value.materialsJson = serializeMaterials(rangeMaterialsDraft.value)
  if (rangeMaterialsDraft.value.length === 0) {
    toast.error('至少需要配置一项数量大于 0 的升级材料。')
    return
  }
  try {
    selectedRangeRule.value = await saveAdminFiveElementBranchRuleRange(selectedRangeRule.value)
    toast.success('五行区间规则保存成功。')
    branchEditorOpen.value = false
    await loadData()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '五行区间规则保存失败。')
  }
}

async function removeRangeRule() {
  if (!canManageConfigs.value || !selectedRangeRule.value?.gid) return
  if (!window.confirm(`确认删除 ${elementLabel(selectedRangeRule.value.elementType)} Lv.${selectedRangeRule.value.minLevel}-${selectedRangeRule.value.maxLevel} 区间规则吗？`)) return
  try {
    await deleteAdminFiveElementBranchRuleRange(selectedRangeRule.value.gid)
    toast.success('五行区间规则删除成功。')
    branchEditorOpen.value = false
    await loadData()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '五行区间规则删除失败。')
  }
}


onMounted(async () => {
  await Promise.all([loadItemOptions(), loadData()])
})
</script>
