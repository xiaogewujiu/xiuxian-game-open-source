<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">作物</p>
        <h2 class="section-title section-title--small">作物模板</h2>
        <p class="section-note">作物模板统一通过表格查看，新增和修改使用弹窗表单。</p>
      </div>

      <div class="filter-grid">
        <label class="field">
          <span>关键字</span>
          <input v-model.trim="keyword" type="text" placeholder="搜索作物编号或名称" />
        </label>
        <label class="field">
          <span>当前作物</span>
          <input :value="selectedCrop?.templateId || ''" type="text" placeholder="未选择" disabled />
        </label>
        <label class="field">
          <span>解锁等级</span>
          <input :value="selectedCrop ? String(selectedCrop.unlockLevel) : ''" type="text" placeholder="全部" disabled />
        </label>

        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadCrops">查询</button>
          <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="openCreateModal">新建</button>
        </div>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">作物列表</p>
          <h3 class="section-title section-title--small">作物列表</h3>
        </div>
        <span class="selected-pill">共 {{ crops.length }} 条</span>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>模板编号</th>
              <th>名称</th>
              <th>类型</th>
              <th>来源</th>
              <th>解锁等级</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="crop in crops"
              :key="crop.templateId"
              :class="{ 'is-active': selectedCrop?.templateId === crop.templateId }"
              @click="openEditModal(crop.templateId)"
            >
              <td>{{ crop.templateId }}</td>
              <td>{{ crop.name }}</td>
              <td>{{ getCropTypeLabel(crop.type) }}</td>
              <td>{{ crop.isBuiltIn ? '内置' : '人工' }}</td>
              <td>Lv.{{ crop.unlockLevel }}</td>
            </tr>
          </tbody>
        </table>
      </div>

    </section>

    <AdminModal
      v-model="editorOpen"
      kicker="作物编辑"
      :title="selectedCrop?.templateId ? `编辑作物 ${selectedCrop.templateId}` : '新建作物'"
      description="维护作物成长周期、种子消耗、产出和品质区间。"
      size="xwide"
    >
      <fieldset v-if="selectedCrop" class="form-fieldset" :disabled="!canManageConfigs">
        <div class="detail-overview">
          <article class="detail-overview__item"><span>来源</span><strong>{{ selectedCrop.isBuiltIn ? '系统内置' : '人工配置' }}</strong></article>
          <article class="detail-overview__item"><span>种子键</span><strong>{{ selectedCrop.seedKey || '-' }}</strong></article>
          <article class="detail-overview__item"><span>内置版本</span><strong>{{ selectedCrop.builtInVersion || '-' }}</strong></article>
          <article class="detail-overview__item"><span>最后更新</span><strong>{{ formatDateTime(selectedCrop.lastUpdateTime) }}</strong></article>
        </div>
        <div class="editor-grid editor-grid--three">
          <label class="field"><span>模板编号</span><input v-model.trim="selectedCrop.templateId" type="text" /></label>
          <label class="field"><span>名称</span><input v-model.trim="selectedCrop.name" type="text" /></label>
          <label class="field"><span>类型</span><select v-model.number="selectedCrop.type"><option v-for="option in CROP_TYPE_OPTIONS" :key="option.value" :value="option.value">{{ option.label }}</option></select></label>
          <label class="field"><span>生长周期</span><input v-model.number="selectedCrop.growthCycle" type="number" min="1" /></label>
          <label class="field"><span>产量</span><input v-model.number="selectedCrop.yield" type="number" min="1" /></label>
          <label class="field"><span>解锁等级</span><input v-model.number="selectedCrop.unlockLevel" type="number" min="1" /></label>
          <label class="field"><span>种子道具</span><select v-model="selectedCrop.seedId"><option value="">请选择</option><option v-for="item in seedOptions" :key="item.itemId" :value="item.itemId">{{ item.name }} ({{ item.itemId }})</option></select></label>
          <label class="field"><span>种子数量</span><input v-model.number="selectedCrop.seedAmount" type="number" min="1" /></label>
          <label class="field"><span>产出道具</span><select v-model="selectedCrop.outputItemId"><option value="">请选择</option><option v-for="item in outputItemOptions" :key="`${item.itemId}-output`" :value="item.itemId">{{ item.name }} ({{ item.itemId }})</option></select></label>
          <label class="field"><span>产出数量</span><input v-model.number="selectedCrop.outputAmount" type="number" min="1" /></label>
          <label class="field"><span>最低品质</span><input v-model.number="selectedCrop.minQuality" type="number" min="1" /></label>
          <label class="field"><span>最高品质</span><input v-model.number="selectedCrop.maxQuality" type="number" min="1" /></label>
        </div>

        <label class="field"><span>描述</span><textarea v-model="selectedCrop.description" rows="4"></textarea></label>
      </fieldset>

      <template #footer>
        <button class="secondary-button" type="button" @click="editorOpen = false">取消</button>
        <button class="danger-button" type="button" :disabled="!canManageConfigs || !selectedCrop?.templateId" @click="removeCrop">删除</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="saveCrop">保存作物</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import { CROP_TYPE_OPTIONS } from '@/constants/game-options'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import { deleteAdminCrop, getAdminCropDetail, getAdminCrops, saveAdminCrop } from '@/services/crops'
import { getAdminItems } from '@/services/items'
import type { AdminCropDetail, AdminCropListItem, AdminItemListItem } from '@/types/admin'
import toast from '@/utils/toast'

// 作物页会同时依赖“种子道具”和“产出道具”两个下拉数据源。
const { canManageConfigs } = useAdminPermissions()
const crops = ref<AdminCropListItem[]>([])
const itemOptions = ref<AdminItemListItem[]>([])
const keyword = ref('')
const selectedCrop = ref<AdminCropDetail | null>(null)
const editorOpen = ref(false)

// 统一格式化后台时间字段。
function formatDateTime(value?: string | null) {
  if (!value) return '-'
  return value.replace('T', ' ').slice(0, 19)
}

// 新建作物时的默认对象。
function createEmptyCrop(): AdminCropDetail {
  return {
    templateId: '',
    name: '',
    description: '',
    type: 1,
    growthCycle: 1,
    yield: 1,
    seedId: '',
    seedAmount: 1,
    outputItemId: '',
    outputAmount: 1,
    minQuality: 1,
    maxQuality: 1,
    unlockLevel: 1,
    isBuiltIn: false,
    seedKey: null,
    builtInVersion: null,
    lastUpdateTime: null
  }
}

// 过滤种子道具。
const seedOptions = computed(() => itemOptions.value.filter((item) => item.type === 13))
// 过滤产出道具。
const outputItemOptions = computed(() => itemOptions.value.filter((item) => item.type !== 13))

// 作物类型值转中文标签。
function getCropTypeLabel(value: number) {
  return CROP_TYPE_OPTIONS.find((option) => option.value === value)?.label || `类型 ${value}`
}

function openCreateModal() {
  if (!canManageConfigs.value) return

  selectedCrop.value = createEmptyCrop()
      editorOpen.value = true
}

async function loadItemOptions() {
  itemOptions.value = await getAdminItems()
}

async function loadCrops() {
  crops.value = await getAdminCrops(keyword.value)
}

async function openEditModal(templateId: string) {
  selectedCrop.value = await getAdminCropDetail(templateId)
      editorOpen.value = true
}

async function saveCrop() {
  if (!canManageConfigs.value || !selectedCrop.value) return
  if (!selectedCrop.value.seedId || !selectedCrop.value.outputItemId) {
    toast.error('请选择种子道具和产出道具。')
        return
  }

  try {
    selectedCrop.value = await saveAdminCrop(selectedCrop.value)
    toast.success('作物模板保存成功。')
    editorOpen.value = false
    await loadCrops()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '作物模板保存失败。')
  }
}

async function removeCrop() {
  if (!canManageConfigs.value || !selectedCrop.value?.templateId) return
  if (!window.confirm(`确认删除作物 ${selectedCrop.value.templateId} 吗？`)) return

  try {
    await deleteAdminCrop(selectedCrop.value.templateId)
    toast.success('作物模板删除成功。')
    selectedCrop.value = null
    editorOpen.value = false
    await loadCrops()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '作物模板删除失败。')
  }
}

onMounted(async () => {
  await Promise.all([loadItemOptions(), loadCrops()])
})
</script>
