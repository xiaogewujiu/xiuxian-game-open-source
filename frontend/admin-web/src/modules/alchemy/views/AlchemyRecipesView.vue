<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">炼丹配方</p>
        <h2 class="section-title section-title--small">炼丹配方</h2>
        <p class="section-note">维护修为丹、突破丹、属性丹的丹方，玩家端按炼丹师等级判定是否可炼制。</p>
      </div>

      <div class="filter-grid">
        <label class="field">
          <span>关键字</span>
          <input v-model.trim="keyword" type="text" placeholder="搜索炼丹配方编号或名称" />
        </label>
        <label class="field">
          <span>当前配方</span>
          <input :value="selectedRecipe?.recipeId || ''" type="text" placeholder="未选择" disabled />
        </label>
        <label class="field">
          <span>丹方等级要求</span>
          <input :value="selectedRecipe ? String(selectedRecipe.requiredFurnaceLevel) : ''" type="text" placeholder="全部" disabled />
        </label>

        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadRecipes">查询</button>
          <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="openCreateModal">新建配方</button>
        </div>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">丹方列表</p>
          <h3 class="section-title section-title--small">丹方列表</h3>
        </div>
        <span class="selected-pill">共 {{ recipes.length }} 条</span>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>配方编号</th>
              <th>名称</th>
              <th>产出丹药</th>
              <th>来源</th>
              <th>等级要求</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="recipe in recipes"
              :key="recipe.recipeId"
              :class="{ 'is-active': selectedRecipe?.recipeId === recipe.recipeId }"
              @click="openEditModal(recipe.recipeId)"
            >
              <td>{{ recipe.recipeId }}</td>
              <td>{{ recipe.name }}</td>
              <td>{{ recipe.pillTemplateId }}</td>
              <td>{{ recipe.isBuiltIn ? '内置' : '人工' }}</td>
              <td>{{ recipe.requiredFurnaceLevel }}</td>
            </tr>
          </tbody>
        </table>
      </div>

    </section>

    <AdminModal
      v-model="editorOpen"
      kicker="炼丹配方"
      :title="selectedRecipe?.recipeId ? `编辑丹方 ${selectedRecipe.recipeId}` : '新建炼丹配方'"
      description="维护丹方基础信息、炼丹师等级要求、成功率、耗时和材料清单。"
      size="xwide"
    >
      <fieldset v-if="selectedRecipe" class="form-fieldset" :disabled="!canManageConfigs">
        <div class="detail-overview">
          <article class="detail-overview__item"><span>来源</span><strong>{{ selectedRecipe.isBuiltIn ? '系统内置' : '人工配置' }}</strong></article>
          <article class="detail-overview__item"><span>种子键</span><strong>{{ selectedRecipe.seedKey || '-' }}</strong></article>
          <article class="detail-overview__item"><span>内置版本</span><strong>{{ selectedRecipe.builtInVersion || '-' }}</strong></article>
          <article class="detail-overview__item"><span>最后更新</span><strong>{{ formatDateTime(selectedRecipe.lastUpdateTime) }}</strong></article>
        </div>
        <div class="editor-grid editor-grid--three">
          <label class="field"><span>配方编号</span><input v-model.trim="selectedRecipe.recipeId" type="text" /></label>
          <label class="field"><span>名称</span><input v-model.trim="selectedRecipe.name" type="text" /></label>
          <label class="field"><span>产出丹药</span><select v-model="selectedRecipe.pillTemplateId"><option value="">请选择</option><option v-for="pill in pillOptions" :key="pill.value" :value="pill.value">{{ pill.label }}</option></select></label>
          <label class="field"><span>炼丹师等级要求</span><input v-model.number="selectedRecipe.requiredFurnaceLevel" type="number" min="1" /></label>
          <label class="field"><span>基础成功率</span><input v-model.number="selectedRecipe.baseSuccessRate" type="number" min="0" /></label>
          <label class="field"><span>基础炼制时长（分钟）</span><input v-model.number="selectedRecipe.baseCraftTime" type="number" min="0" /></label>
        </div>

        <label class="field checkbox-field"><input v-model="selectedRecipe.isDefaultLearned" type="checkbox" /><span>默认已学会</span></label>
        <label class="field"><span>描述</span><textarea v-model="selectedRecipe.description" rows="3"></textarea></label>
        <label class="field"><span>解锁条件</span><textarea v-model="selectedRecipe.unlockCondition" rows="2"></textarea></label>
        <AlchemyMaterialsEditor v-model="materialsDraft" :options="itemOptions" />
      </fieldset>

      <template #footer>
        <button class="secondary-button" type="button" @click="editorOpen = false">取消</button>
        <button class="danger-button" type="button" :disabled="!canManageConfigs || !selectedRecipe?.recipeId" @click="removeRecipe">删除</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="saveRecipe">保存炼丹配方</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import AlchemyMaterialsEditor from '@/components/editors/AlchemyMaterialsEditor.vue'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import { getAdminItems } from '@/services/items'
import { deleteAdminAlchemyRecipe, getAdminAlchemyRecipeDetail, getAdminAlchemyRecipes, saveAdminAlchemyRecipe } from '@/services/alchemy'
import type { AdminAlchemyRecipeDetail, AdminAlchemyRecipeListItem, AdminItemListItem, AlchemyMaterialEntry } from '@/types/admin'
import toast from '@/utils/toast'

// 炼丹配方页依赖丹药模板和材料道具两个下拉数据源。
const { canManageConfigs } = useAdminPermissions()
const recipes = ref<AdminAlchemyRecipeListItem[]>([])
const itemTemplates = ref<AdminItemListItem[]>([])
const pillOptions = computed(() => itemTemplates.value.filter((item) => item.type === 12).map((item) => ({ value: item.itemId, label: `${item.name} (${item.itemId})` })))
const keyword = ref('')
const selectedRecipe = ref<AdminAlchemyRecipeDetail | null>(null)
const materialsDraft = ref<AlchemyMaterialEntry[]>([])
const editorOpen = ref(false)

// 统一格式化后台时间字段。
function formatDateTime(value?: string | null) {
  if (!value) return '-'
  return value.replace('T', ' ').slice(0, 19)
}

// 材料编辑器下拉选项。
const itemOptions = computed(() => itemTemplates.value.map((item) => ({ value: item.itemId, label: `${item.name} (${item.itemId})`, name: item.name })))

// 把 materialsJson 解析成材料编辑器草稿。
function parseMaterials(value?: string | null) {
  if (!value) return []
  try {
    const parsed = JSON.parse(value) as Array<Record<string, unknown>>
    return parsed.map((item) => ({ itemId: String(item.ItemId ?? item.itemId ?? ''), itemName: String(item.ItemName ?? item.itemName ?? ''), amount: Number(item.Amount ?? item.amount) || 1, isReplaceable: Boolean(item.IsReplaceable ?? item.isReplaceable), alternativeItemIds: (() => { const alternativeItemIds = item.AlternativeItemIds ?? item.alternativeItemIds; return Array.isArray(alternativeItemIds) ? alternativeItemIds.map((entry) => String(entry)).filter(Boolean) : [] })() })).filter((item) => item.itemId)
  } catch {
    return []
  }
}

// 把材料草稿重新序列化成 materialsJson。
function serializeMaterials(entries: AlchemyMaterialEntry[]) {
  if (entries.length === 0) return '[]'
  return JSON.stringify(entries.map((item) => ({ ItemId: item.itemId, ItemName: item.itemName, Amount: item.amount, IsReplaceable: item.isReplaceable, AlternativeItemIds: item.alternativeItemIds })))
}

// 根据当前配方详情同步材料草稿。
function syncRecipeEditors(recipe: AdminAlchemyRecipeDetail | null) {
  materialsDraft.value = parseMaterials(recipe?.materialsJson)
}

// 把当前材料草稿回写到 selectedRecipe。
function applyRecipeEditors() {
  if (!selectedRecipe.value) return
  selectedRecipe.value.materialsJson = serializeMaterials(materialsDraft.value)
}

// 新建配方时的默认对象。
function createEmptyRecipe(): AdminAlchemyRecipeDetail {
  return { recipeId: '', pillTemplateId: '', name: '', description: '', requiredFurnaceLevel: 1, baseSuccessRate: 0, baseCraftTime: 0, materialsJson: '[]', unlockCondition: '', isDefaultLearned: false, isBuiltIn: false, seedKey: null, builtInVersion: null, lastUpdateTime: null }
}

// 打开新建弹窗。
function openCreateModal() {
  if (!canManageConfigs.value) return
  selectedRecipe.value = createEmptyRecipe()
  syncRecipeEditors(selectedRecipe.value)
  editorOpen.value = true
}

// 拉取配方列表。
async function loadRecipes() {
  recipes.value = await getAdminAlchemyRecipes(keyword.value)
}

// 预加载材料道具选项。
async function loadItemOptions() {
  itemTemplates.value = await getAdminItems()
}

// 打开配方详情弹窗。
async function openEditModal(recipeId: string) {
  selectedRecipe.value = await getAdminAlchemyRecipeDetail(recipeId)
  syncRecipeEditors(selectedRecipe.value)
  editorOpen.value = true
}

// 保存炼丹配方。
async function saveRecipe() {
  if (!canManageConfigs.value || !selectedRecipe.value) return
  if (!selectedRecipe.value.pillTemplateId.trim()) {
    toast.error('请选择产出丹药。')
    return
  }
  applyRecipeEditors()
  try {
    selectedRecipe.value = await saveAdminAlchemyRecipe(selectedRecipe.value)
    syncRecipeEditors(selectedRecipe.value)
    toast.success('炼丹配方保存成功。')
    editorOpen.value = false
    await loadRecipes()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '炼丹配方保存失败。')
  }
}

// 删除炼丹配方。
async function removeRecipe() {
  if (!canManageConfigs.value || !selectedRecipe.value?.recipeId) return
  if (!window.confirm(`确认删除炼丹配方 ${selectedRecipe.value.recipeId} 吗？`)) return
  try {
    await deleteAdminAlchemyRecipe(selectedRecipe.value.recipeId)
    toast.success('炼丹配方删除成功。')
    selectedRecipe.value = null
    syncRecipeEditors(null)
    editorOpen.value = false
    await loadRecipes()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '炼丹配方删除失败。')
  }
}

onMounted(async () => {
  await Promise.all([loadItemOptions(), loadRecipes()])
})
</script>
