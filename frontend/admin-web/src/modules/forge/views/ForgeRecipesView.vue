<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">锻造配方</p>
        <h2 class="section-title section-title--small">锻造配方</h2>
        <p class="section-note">图纸等级、成功率和材料在这里维护，玩家端按锻造师等级和图纸耗时执行制作。</p>
      </div>

      <div class="filter-grid">
        <label class="field">
          <span>关键字</span>
          <input v-model.trim="keyword" type="text" placeholder="搜索锻造配方编号或名称" />
        </label>
        <label class="field">
          <span>当前配方</span>
          <input :value="selectedRecipe?.recipeId || ''" type="text" placeholder="未选择" disabled />
        </label>
        <label class="field">
          <span>图纸等级</span>
          <input :value="selectedRecipe ? String(selectedRecipe.level) : ''" type="text" placeholder="全部" disabled />
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
          <p class="section-kicker">图纸列表</p>
          <h3 class="section-title section-title--small">图纸列表</h3>
        </div>
        <span class="selected-pill">共 {{ recipes.length }} 条</span>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>配方编号</th>
              <th>名称</th>
              <th>装备模板</th>
              <th>来源</th>
              <th>图纸等级</th>
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
              <td>{{ recipe.templateId }}</td>
              <td>{{ recipe.isBuiltIn ? '内置' : '人工' }}</td>
              <td>{{ recipe.level }}</td>
            </tr>
          </tbody>
        </table>
      </div>

    </section>

    <AdminModal
      v-model="editorOpen"
      kicker="锻造配方"
      :title="selectedRecipe?.recipeId ? `编辑图纸 ${selectedRecipe.recipeId}` : '新建锻造配方'"
      description="维护锻造图纸、装备模板、图纸等级、价格、成功率和材料。"
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
          <label class="field"><span>装备模板</span><select v-model="selectedRecipe.templateId"><option value="">请选择</option><option v-for="equipment in equipmentOptions" :key="equipment.equipmentId" :value="String(equipment.equipmentId)">{{ equipment.name }} ({{ equipment.equipmentId }})</option></select></label>
          <label class="field"><span>品质</span><select v-model.number="selectedRecipe.quality"><option v-for="option in EQUIPMENT_QUALITY_OPTIONS" :key="option.value" :value="option.value">{{ option.label }}</option></select></label>
          <label class="field"><span>图纸等级</span><input v-model.number="selectedRecipe.level" type="number" min="1" /></label>
          <label class="field"><span>价格</span><input v-model.number="selectedRecipe.costGold" type="number" min="0" /></label>
          <label class="field"><span>成功率</span><input v-model.number="selectedRecipe.successRate" type="number" min="0" /></label>
        </div>

        <AdminImageField
          v-model="selectedRecipe.icon"
          label="图纸图片"
          upload-category="forge-recipes"
          placeholder="未上传时保持为空"
        />

        <label class="field checkbox-field"><input v-model="selectedRecipe.isEnabled" type="checkbox" /><span>启用配方</span></label>
        <label class="field"><span>描述</span><textarea v-model="selectedRecipe.description" rows="3"></textarea></label>
        <ForgeMaterialsEditor v-model="materialsDraft" :options="itemOptions" />
      </fieldset>

      <template #footer>
        <button class="secondary-button" type="button" @click="editorOpen = false">取消</button>
        <button class="danger-button" type="button" :disabled="!canManageConfigs || !selectedRecipe?.recipeId" @click="removeRecipe">删除</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="saveRecipe">保存锻造配方</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import AdminImageField from '@/components/AdminImageField.vue'
import { EQUIPMENT_QUALITY_OPTIONS } from '@/constants/game-options'
import ForgeMaterialsEditor from '@/components/editors/ForgeMaterialsEditor.vue'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import { getAdminEquipments } from '@/services/equipments'
import { deleteAdminForgeRecipe, getAdminForgeRecipeDetail, getAdminForgeRecipes, saveAdminForgeRecipe } from '@/services/forge'
import { getAdminItems } from '@/services/items'
import type { AdminEquipmentListItem, AdminForgeRecipeDetail, AdminForgeRecipeListItem, AdminItemListItem, ForgeMaterialEntry } from '@/types/admin'
import toast from '@/utils/toast'

// 锻造配方页依赖装备模板和材料道具两个下拉数据源。
const { canManageConfigs } = useAdminPermissions()
const recipes = ref<AdminForgeRecipeListItem[]>([])
const equipmentOptions = ref<AdminEquipmentListItem[]>([])
const itemTemplates = ref<AdminItemListItem[]>([])
const keyword = ref('')
const selectedRecipe = ref<AdminForgeRecipeDetail | null>(null)
const materialsDraft = ref<ForgeMaterialEntry[]>([])
const editorOpen = ref(false)

// 统一格式化后台时间字段。
function formatDateTime(value?: string | null) {
  if (!value) return '-'
  return value.replace('T', ' ').slice(0, 19)
}

// 材料编辑器下拉选项。
const itemOptions = computed(() => itemTemplates.value.map((item) => ({ value: item.itemId, label: `${item.name} (${item.itemId})`, name: item.name })))

const SLOT_NAME_MAP: Record<number, string> = { 0: 'Weapon', 1: 'Helmet', 2: 'Armor', 3: 'Pants', 4: 'Necklace', 5: 'Ring', 6: 'Boots', 7: 'Treasure' }

watch(() => selectedRecipe.value?.templateId, (templateId) => {
  if (!selectedRecipe.value || !templateId) return
  const template = equipmentOptions.value.find((e) => String(e.equipmentId) === templateId)
  if (template) {
    selectedRecipe.value.slotName = SLOT_NAME_MAP[template.slot] ?? ''
  }
})

// 把 materialsJson 解析成材料编辑器草稿。
function parseMaterials(value?: string | null) {
  if (!value) return []
  try {
    const parsed = JSON.parse(value) as Array<Record<string, unknown>>
    return parsed.map((item) => ({ itemId: String(item.ItemId ?? item.itemId ?? ''), name: String(item.Name ?? item.name ?? ''), icon: String(item.Icon ?? item.icon ?? ''), count: Number(item.Count ?? item.count) || 1 })).filter((item) => item.itemId)
  } catch {
    return []
  }
}

// 把材料草稿重新序列化成 materialsJson。
function serializeMaterials(entries: ForgeMaterialEntry[]) {
  if (entries.length === 0) return '[]'
  return JSON.stringify(entries.map((item) => ({ ItemId: item.itemId, Name: item.name, Icon: item.icon, Count: item.count })))
}

// 根据当前图纸详情同步材料草稿。
function syncRecipeEditors(recipe: AdminForgeRecipeDetail | null) {
  materialsDraft.value = parseMaterials(recipe?.materialsJson)
}

// 把当前材料草稿回写到 selectedRecipe。
function applyRecipeEditors() {
  if (!selectedRecipe.value) return
  selectedRecipe.value.materialsJson = serializeMaterials(materialsDraft.value)
}

// 新建图纸时的默认对象。
function createEmptyRecipe(): AdminForgeRecipeDetail {
  return { recipeId: '', templateId: '', name: '', description: '', slotName: '', quality: 1, level: 1, icon: '', costGold: 0, successRate: 0, materialsJson: '[]', isEnabled: true, isBuiltIn: false, seedKey: null, builtInVersion: null, lastUpdateTime: null }
}

function getQualityLabel(value: number) {
  return EQUIPMENT_QUALITY_OPTIONS.find((option) => option.value === value)?.label || `品质 ${value}`
}

// 打开新建弹窗。
function openCreateModal() {
  if (!canManageConfigs.value) return
  selectedRecipe.value = createEmptyRecipe()
  syncRecipeEditors(selectedRecipe.value)
  editorOpen.value = true
}

// 预加载装备模板选项。
async function loadEquipmentOptions() {
  equipmentOptions.value = await getAdminEquipments()
}

// 预加载材料道具选项。
async function loadItemOptions() {
  itemTemplates.value = await getAdminItems()
}

// 拉取锻造配方列表。
async function loadRecipes() {
  recipes.value = await getAdminForgeRecipes(keyword.value)
}

// 打开图纸详情弹窗。
async function openEditModal(recipeId: string) {
  selectedRecipe.value = await getAdminForgeRecipeDetail(recipeId)
  syncRecipeEditors(selectedRecipe.value)
  editorOpen.value = true
}

async function saveRecipe() {
  if (!canManageConfigs.value || !selectedRecipe.value) return
  if (!selectedRecipe.value.templateId.trim()) {
    toast.error('请选择装备模板。')
    return
  }
  applyRecipeEditors()
  try {
    selectedRecipe.value = await saveAdminForgeRecipe(selectedRecipe.value)
    syncRecipeEditors(selectedRecipe.value)
    toast.success('锻造配方保存成功。')
    editorOpen.value = false
    await loadRecipes()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '锻造配方保存失败。')
  }
}

async function removeRecipe() {
  if (!canManageConfigs.value || !selectedRecipe.value?.recipeId) return
  if (!window.confirm(`确认删除锻造配方 ${selectedRecipe.value.recipeId} 吗？`)) return
  try {
    await deleteAdminForgeRecipe(selectedRecipe.value.recipeId)
    toast.success('锻造配方删除成功。')
    selectedRecipe.value = null
    syncRecipeEditors(null)
    editorOpen.value = false
    await loadRecipes()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '锻造配方删除失败。')
  }
}

onMounted(async () => {
  await Promise.all([loadEquipmentOptions(), loadItemOptions(), loadRecipes()])
})
</script>
