<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">装备</p>
        <h2 class="section-title section-title--small">装备模板</h2>
        <p class="section-note">装备模板统一使用表格列表管理，新增和编辑通过弹窗完成。</p>
      </div>

      <div class="filter-grid">
        <label class="field">
          <span>关键字</span>
          <input v-model.trim="keyword" type="text" placeholder="搜索装备编号或名称" />
        </label>
        <label class="field">
          <span>当前装备</span>
          <input :value="selectedEquipment ? String(selectedEquipment.equipmentId) : ''" type="text" placeholder="未选择" disabled />
        </label>
        <label class="field">
          <span>槽位</span>
          <select v-model="slotFilter">
            <option value="">全部</option>
            <option v-for="option in EQUIPMENT_SLOT_OPTIONS" :key="option.value" :value="option.value">{{ option.label }}</option>
          </select>
        </label>

        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadEquipments">查询</button>
          <button class="primary-button" type="button" @click="openCreateModal">新建</button>
        </div>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">装备列表</p>
          <h3 class="section-title section-title--small">装备列表</h3>
        </div>
        <span class="selected-pill">共 {{ filteredEquipments.length }} 条</span>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>图标</th>
              <th>装备编号</th>
              <th>名称</th>
              <th>等级</th>
              <th>来源</th>
              <th>品质</th>
              <th>槽位</th>
              <th>可交易</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="equipment in filteredEquipments"
              :key="equipment.equipmentId"
              :class="{ 'is-active': selectedEquipment?.equipmentId === equipment.equipmentId }"
              @click="openEditModal(equipment.equipmentId)"
            >
              <td>
                <img v-if="equipment.iconPath" :src="buildAdminApiUrl(equipment.iconPath)" class="list-icon" alt="" />
                <span v-else class="list-icon-placeholder">-</span>
              </td>
              <td>#{{ equipment.equipmentId }}</td>
              <td>{{ equipment.name }}</td>
              <td>Lv.{{ equipment.level }}</td>
              <td>{{ equipment.isBuiltIn ? '内置' : '人工' }}</td>
              <td>{{ getQualityLabel(equipment.quality) }}</td>
              <td>{{ getSlotLabel(equipment.slot) }}</td>
              <td>
                <input type="checkbox" :checked="equipment.isTradeable" @click.stop @change="toggleEquipmentTradeable(equipment, $event)" />
              </td>
            </tr>
          </tbody>
        </table>
      </div>

    </section>

    <AdminModal
      v-model="editorOpen"
      kicker="装备编辑"
      :title="selectedEquipment?.equipmentId ? `编辑装备 #${selectedEquipment.equipmentId}` : '新建装备'"
      description="维护装备槽位、品质、流派和属性区间。"
      size="xwide"
    >
      <fieldset v-if="selectedEquipment" class="form-fieldset">
        <div class="detail-overview">
          <article class="detail-overview__item"><span>来源</span><strong>{{ selectedEquipment.isBuiltIn ? '内置' : '人工维护' }}</strong></article>
          <article class="detail-overview__item"><span>种子键</span><strong>{{ selectedEquipment.seedKey || '-' }}</strong></article>
          <article class="detail-overview__item"><span>内置版本</span><strong>{{ selectedEquipment.builtInVersion || '-' }}</strong></article>
          <article class="detail-overview__item"><span>最后更新</span><strong>{{ formatDateTime(selectedEquipment.lastUpdateTime) }}</strong></article>
        </div>
        <div class="editor-grid editor-grid--three">
          <label class="field"><span>装备编号</span><input v-model.number="selectedEquipment.equipmentId" type="number" min="1" /></label>
          <label class="field"><span>装备名称</span><input v-model.trim="selectedEquipment.name" type="text" /></label>
          <label class="field"><span>等级</span><input v-model.number="selectedEquipment.level" type="number" min="1" /></label>
          <label class="field"><span>品质</span><select v-model.number="selectedEquipment.quality"><option v-for="option in EQUIPMENT_QUALITY_OPTIONS" :key="option.value" :value="option.value">{{ option.label }}</option></select></label>
          <label class="field"><span>槽位</span><select v-model.number="selectedEquipment.slot"><option v-for="option in EQUIPMENT_SLOT_OPTIONS" :key="option.value" :value="option.value">{{ option.label }}</option></select></label>
          <label class="field"><span>装备标签</span><select v-model.number="selectedEquipment.combatStyle"><option v-for="option in COMBAT_STYLE_OPTIONS" :key="option.value" :value="option.value">{{ option.label }}</option></select></label>
          <label class="field"><span>武器类别</span><select v-model.number="selectedEquipment.weaponCategory"><option v-for="option in WEAPON_CATEGORY_OPTIONS" :key="option.value" :value="option.value">{{ option.label }}</option></select></label>
          <label class="field checkbox-field"><input v-model="selectedEquipment.isTradeable" type="checkbox" /><span>可交易</span></label>
        </div>

        <AdminImageField
          v-model="selectedEquipment.iconPath"
          label="装备图片"
          upload-category="equipments"
          placeholder="未上传时保持为空"
        />

        <label class="field"><span>描述</span><textarea v-model="selectedEquipment.description" rows="4"></textarea></label>
        <AttributesRangeEditor v-model="selectedEquipment.attributes" />
      </fieldset>

      <template #footer>
        <button class="secondary-button" type="button" @click="editorOpen = false">取消</button>
        <button class="danger-button" type="button" :disabled="!selectedEquipment || selectedEquipment.equipmentId <= 0" @click="removeEquipment">删除</button>
        <button class="primary-button" type="button" @click="saveEquipment">保存装备</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import AdminImageField from '@/components/AdminImageField.vue'
import { buildAdminApiUrl } from '@/services/http'
import { COMBAT_STYLE_OPTIONS, EQUIPMENT_QUALITY_OPTIONS, EQUIPMENT_SLOT_OPTIONS, WEAPON_CATEGORY_OPTIONS } from '@/constants/game-options'
import type { AdminEquipmentDetail, AdminEquipmentListItem } from '@/types/admin'
import { deleteAdminEquipment, getAdminEquipmentDetail, getAdminEquipments, saveAdminEquipment } from '@/services/equipments'
import AttributesRangeEditor from '@/components/editors/AttributesRangeEditor.vue'
import toast from '@/utils/toast'

// 装备页的编辑对象和列表摘要拆开维护，
// 避免在表格切换行时直接污染还未保存的详情对象。
const equipments = ref<AdminEquipmentListItem[]>([])
const keyword = ref('')
const slotFilter = ref<number | ''>('')
const selectedEquipment = ref<AdminEquipmentDetail | null>(null)
const editorOpen = ref(false)

// 后端已支持按槽位筛选。
const filteredEquipments = equipments

// 统一格式化后台时间字段。
function formatDateTime(value?: string | null) {
  if (!value) return '-'
  return value.replace('T', ' ').slice(0, 19)
}

// 品质值转中文标签。
function getQualityLabel(value: number) {
  return EQUIPMENT_QUALITY_OPTIONS.find((option) => option.value === value)?.label || `品质 ${value}`
}

// 槽位值转中文标签。
function getSlotLabel(value: number) {
  return EQUIPMENT_SLOT_OPTIONS.find((option) => option.value === value)?.label || `槽位 ${value}`
}

// 新建装备时的默认对象。
function createEmptyEquipment(): AdminEquipmentDetail {
  return {
    equipmentId: 0,
    name: '',
    level: 1,
    quality: 1,
    slot: 0,
    combatStyle: 0,
    weaponCategory: 0,
    description: '',
    iconPath: '',
    isBuiltIn: false,
    seedKey: '',
    builtInVersion: '',
    lastUpdateTime: '',
    attributes: {}
  }
}

// 打开新建弹窗。
function openCreateModal() {
  selectedEquipment.value = createEmptyEquipment()
      editorOpen.value = true
}

// 拉取装备列表。
async function loadEquipments() {
  const filterSlot = slotFilter.value === '' ? null : Number(slotFilter.value)
  equipments.value = await getAdminEquipments(keyword.value, filterSlot)
}

// 打开装备详情弹窗。
async function openEditModal(equipmentId: number) {
  selectedEquipment.value = await getAdminEquipmentDetail(equipmentId)
      editorOpen.value = true
}

async function toggleEquipmentTradeable(equipment: AdminEquipmentListItem, event: Event) {
  const checked = (event.target as HTMLInputElement).checked
  try {
    const detail = await getAdminEquipmentDetail(equipment.equipmentId)
    detail.isTradeable = checked
    await saveAdminEquipment(detail)
    equipment.isTradeable = checked
    toast.success(`装备 ${equipment.name} 可交易状态已更新。`)
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '更新失败。')
    ;(event.target as HTMLInputElement).checked = !checked
  }
}

// 保存装备模板。
async function saveEquipment() {
  if (!selectedEquipment.value) return

  try {
    selectedEquipment.value = await saveAdminEquipment(selectedEquipment.value)
    toast.success('装备模板保存成功。')
    editorOpen.value = false
    await loadEquipments()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '装备模板保存失败。')
  }
}

// 删除装备模板。
async function removeEquipment() {
  if (!selectedEquipment.value?.equipmentId) return
  if (!window.confirm(`确认删除装备模板 #${selectedEquipment.value.equipmentId} 吗？`)) return

  try {
    await deleteAdminEquipment(selectedEquipment.value.equipmentId)
    toast.success('装备模板删除成功。')
    selectedEquipment.value = null
    editorOpen.value = false
    await loadEquipments()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '装备模板删除失败。')
  }
}

onMounted(loadEquipments)
</script>
