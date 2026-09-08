<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">图片图鉴</p>
        <h2 class="section-title section-title--small">图片图鉴管理</h2>
        <p class="section-note">管理图片图鉴系列及其图鉴项、属性加成。</p>
      </div>
      <div class="filter-grid">
        <label class="field"><span>关键字</span><input v-model.trim="keyword" type="text" placeholder="搜索系列名称" /></label>
        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadSeries">查询</button>
          <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="openCreateSeries">新建系列</button>
        </div>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">系列列表</p>
          <h3 class="section-title section-title--small">系列列表</h3>
        </div>
        <span class="selected-pill">共 {{ seriesList.length }} 条</span>
      </div>
      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>系列ID</th>
              <th>名称</th>
              <th>图标</th>
              <th>描述</th>
              <th>来源</th>
              <th>状态</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="s in seriesList" :key="s.seriesId" :class="{ 'is-active': detailSeries?.seriesId === s.seriesId && detailOpen }" @click="openDetail(s.seriesId)">
              <td>{{ s.seriesId }}</td>
              <td>{{ s.name }}</td>
              <td>{{ s.icon || '-' }}</td>
              <td class="col-desc">{{ s.description || '-' }}</td>
              <td>{{ s.isBuiltIn ? '内置' : '人工' }}</td>
              <td><span :class="s.isEnabled ? 'tag-active' : 'tag-inactive'">{{ s.isEnabled ? '启用' : '停用' }}</span></td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>

    <!-- 系列详情/编辑弹窗（含基本信息、图鉴项和属性加成） -->
    <AdminModal v-model="detailOpen" kicker="系列详情" :title="detailCreateMode ? '新建系列' : `系列：${detailSeries?.name || ''}`" :description="detailSeries?.description || '维护图片图鉴系列的基本信息。'" size="xwide">
      <div v-if="detailSeries" class="detail-section">
        <div class="detail-overview" v-if="!detailCreateMode">
          <article class="detail-overview__item"><span>系列ID</span><strong>{{ detailSeries.seriesId }}</strong></article>
          <article class="detail-overview__item"><span>来源</span><strong>{{ detailSeries.isBuiltIn ? '内置' : '人工维护' }}</strong></article>
          <article class="detail-overview__item"><span>最后更新</span><strong>{{ formatDateTime(detailSeries.lastUpdateTime) }}</strong></article>
        </div>
        <div class="editor-grid editor-grid--three">
          <label v-if="!detailCreateMode" class="field"><span>系列ID</span><input :value="detailSeries.seriesId" type="text" disabled /></label>
          <label class="field"><span>系列名称</span><input v-model.trim="detailSeries.name" type="text" :disabled="!canManageConfigs" /></label>
        </div>
        <AdminImageField v-model="detailSeries.icon" label="系列图标" upload-category="collections" placeholder="图标路径或上传" />
        <label class="field"><span>描述</span><textarea v-model="detailSeries.description" rows="3" :disabled="!canManageConfigs"></textarea></label>
        <label class="field checkbox-field"><input v-model="detailSeries.isEnabled" type="checkbox" :disabled="!canManageConfigs" /><span>启用系列</span></label>

        <!-- 图鉴项子表 -->
        <div class="sub-section">
          <div class="sub-section-header">
            <h4 class="sub-section-title">图鉴项 <span class="count-badge">{{ detailItems.length }}</span></h4>
            <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="openCreateItem">新建图鉴项</button>
          </div>
          <div class="table-wrap">
            <table class="data-table data-table--compact">
              <thead>
                <tr>
                  <th>图鉴项ID</th>
                  <th>图片名称</th>
                  <th>缩略图</th>
                  <th>原图</th>
                  <th>来源</th>
                  <th>操作</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="item in detailItems" :key="item.itemId">
                  <td>{{ item.itemId }}</td>
                  <td>{{ item.imageName }}</td>
                  <td>{{ item.thumbUrl ? '有' : '-' }}</td>
                  <td>{{ item.originalUrl ? '有' : '-' }}</td>
                  <td>{{ item.isBuiltIn ? '内置' : '人工' }}</td>
                  <td class="col-actions">
                    <button class="link-button" type="button" :disabled="!canManageConfigs" @click="openEditItem(item.itemId)">编辑</button>
                    <button class="link-button danger" type="button" :disabled="!canManageConfigs" @click="removeItem(item.itemId)">删除</button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <!-- 属性加成子表 -->
        <div class="sub-section">
          <div class="sub-section-header">
            <h4 class="sub-section-title">属性加成 <span class="count-badge">{{ detailBonuses.length }}</span></h4>
            <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="openCreateBonus">新建加成</button>
          </div>
          <div class="table-wrap">
            <table class="data-table data-table--compact">
              <thead>
                <tr>
                  <th>属性类型</th>
                  <th>数值</th>
                  <th>值类型</th>
                  <th>来源</th>
                  <th>操作</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="b in detailBonuses" :key="b.bonusId">
                  <td>{{ attrTypeLabel(b.attrType) }}</td>
                  <td>{{ b.attrValue }}</td>
                  <td>{{ b.valueType === 0 ? '固定值' : '百分比' }}</td>
                  <td>{{ b.isBuiltIn ? '内置' : '人工' }}</td>
                  <td class="col-actions">
                    <button class="link-button" type="button" :disabled="!canManageConfigs" @click="openEditBonus(b.bonusId)">编辑</button>
                    <button class="link-button danger" type="button" :disabled="!canManageConfigs" @click="removeBonus(b.bonusId)">删除</button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
      <template #footer>
        <button class="secondary-button" type="button" @click="detailOpen = false">取消</button>
        <button class="danger-button" type="button" :disabled="!canManageConfigs || detailCreateMode" @click="removeSeries(detailSeries?.seriesId || '')">删除</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="saveDetailSeries">保存系列信息</button>
      </template>
    </AdminModal>

    <!-- 图鉴项编辑弹窗 -->
    <AdminModal v-model="itemEditorOpen" kicker="图片图鉴项" :title="itemEditorMode === 'create' ? '新建图鉴项' : '编辑图鉴项'" description="上传原图自动生成缩略图。" size="xwide">
      <fieldset v-if="editingItem" class="form-fieldset" :disabled="!canManageConfigs">
        <label class="field"><span>图片名称</span><input v-model.trim="editingItem.imageName" type="text" /></label>
        <div class="upload-image-row">
          <div class="upload-box">
            <span class="upload-label">原图</span>
            <div v-if="editingItem.originalUrl" class="upload-preview">
              <img :src="toAbsUrl(editingItem.originalUrl)" alt="原图" />
            </div>
            <div v-else class="upload-placeholder">暂无图片</div>
            <button class="secondary-button" type="button" :disabled="uploadingItem || !canManageConfigs" @click="triggerItemUpload">{{ uploadingItem ? '上传中...' : '上传图片' }}</button>
            <input ref="itemFileRef" type="file" accept="image/*" hidden @change="handleItemUpload" />
          </div>
          <div class="upload-box">
            <span class="upload-label">缩略图（自动生成）</span>
            <div v-if="editingItem.thumbUrl" class="upload-preview">
              <img :src="toAbsUrl(editingItem.thumbUrl)" alt="缩略图" />
            </div>
            <div v-else class="upload-placeholder">上传原图后自动生成</div>
          </div>
        </div>
      </fieldset>
      <template #footer>
        <button class="secondary-button" type="button" @click="itemEditorOpen = false">取消</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="saveItem">保存</button>
      </template>
    </AdminModal>

    <!-- 属性加成编辑弹窗 -->
    <AdminModal v-model="bonusEditorOpen" kicker="属性加成" :title="bonusEditorMode === 'create' ? '新建加成' : '编辑加成'" description="配置集齐系列后的属性加成。" size="xwide">
      <fieldset v-if="editingBonus" class="form-fieldset" :disabled="!canManageConfigs">
        <div class="editor-grid editor-grid--three">
          <label class="field"><span>属性类型</span><select v-model="editingBonus.attrType"><option v-for="opt in ATTR_TYPE_OPTIONS" :key="opt.value" :value="opt.value">{{ opt.label }}</option></select></label>
          <label class="field"><span>属性值</span><input v-model.number="editingBonus.attrValue" type="number" /></label>
          <label class="field"><span>值类型</span><select v-model.number="editingBonus.valueType"><option v-for="opt in BONUS_VALUE_TYPE_OPTIONS" :key="opt.value" :value="opt.value">{{ opt.label }}</option></select></label>
        </div>
      </fieldset>
      <template #footer>
        <button class="secondary-button" type="button" @click="bonusEditorOpen = false">取消</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="saveBonus">保存</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import AdminImageField from '@/components/AdminImageField.vue'
import { buildAdminApiUrl } from '@/services/http'
import { uploadCollectionImage } from '@/services/assets'
import { BONUS_VALUE_TYPE_OPTIONS, ATTR_TYPE_OPTIONS } from '@/constants/game-options'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import {
  getAdminImageCollectionSeries, getAdminImageCollectionSeriesDetail, saveAdminImageCollectionSeries, deleteAdminImageCollectionSeries,
  getAdminImageCollectionItems, getAdminImageCollectionItemDetail, saveAdminImageCollectionItem, deleteAdminImageCollectionItem,
  getAdminImageCollectionBonuses, getAdminImageCollectionBonusDetail, saveAdminImageCollectionBonus, deleteAdminImageCollectionBonus
} from '@/services/collections'
import type {
  AdminImageCollectionSeriesListItem, AdminImageCollectionSeriesDetail,
  AdminImageCollectionItemListItem, AdminImageCollectionItemDetail,
  AdminImageCollectionBonusListItem, AdminImageCollectionBonusDetail
} from '@/types/admin'
import toast from '@/utils/toast'

function attrTypeLabel(v: string) { return ATTR_TYPE_OPTIONS.find(o => o.value === v)?.label || v }

const { canManageConfigs } = useAdminPermissions()
const keyword = ref('')
const seriesList = ref<AdminImageCollectionSeriesListItem[]>([])

// 系列详情/编辑
const detailOpen = ref(false)
const detailSeries = ref<AdminImageCollectionSeriesDetail | null>(null)
const detailCreateMode = ref(false)
const detailItems = ref<AdminImageCollectionItemListItem[]>([])
const detailBonuses = ref<AdminImageCollectionBonusListItem[]>([])

// 图鉴项编辑
const itemEditorOpen = ref(false)
const itemEditorMode = ref<'create' | 'edit'>('create')
const editingItem = ref<AdminImageCollectionItemDetail | null>(null)

// 属性加成编辑
const bonusEditorOpen = ref(false)
const bonusEditorMode = ref<'create' | 'edit'>('create')
const editingBonus = ref<AdminImageCollectionBonusDetail | null>(null)

// 图片上传
const itemFileRef = ref<HTMLInputElement | null>(null)
const uploadingItem = ref(false)

function toAbsUrl(path: string | null | undefined) {
  if (!path) return ''
  return /^https?:\/\//i.test(path) ? path : buildAdminApiUrl(path)
}

function triggerItemUpload() { itemFileRef.value?.click() }

async function handleItemUpload(event: Event) {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]
  target.value = ''
  if (!file || !editingItem.value) return
  uploadingItem.value = true
  try {
    const result = await uploadCollectionImage(file)
    editingItem.value.originalUrl = result.originalPath
    editingItem.value.thumbUrl = result.thumbPath
  } catch (e) {
    toast.error(e instanceof Error ? e.message : '上传失败。')
  } finally {
    uploadingItem.value = false
  }
}

function formatDateTime(value?: string | null) { return value ? value.replace('T', ' ').slice(0, 19) : '-' }

function genId(prefix: string) { return `${prefix}_${Date.now().toString(36)}_${Math.random().toString(36).slice(2, 6)}` }

function createEmptySeries(): AdminImageCollectionSeriesDetail {
  return { seriesId: genId('img'), name: '', description: '', icon: '', sortOrder: 0, isEnabled: true, isBuiltIn: false, seedKey: null, builtInVersion: null, lastUpdateTime: null }
}
function createEmptyItem(): AdminImageCollectionItemDetail {
  const sid = detailSeries.value?.seriesId || ''
  return { itemId: genId(sid), seriesId: sid, imageName: '', thumbUrl: '', originalUrl: '', sortOrder: 0, isEnabled: true, isBuiltIn: false, seedKey: null, builtInVersion: null, lastUpdateTime: null }
}
function createEmptyBonus(): AdminImageCollectionBonusDetail {
  const sid = detailSeries.value?.seriesId || ''
  return { bonusId: genId(sid + '_b'), seriesId: sid, attrType: 'Attack', attrValue: 0, valueType: 0, sortOrder: 0, isBuiltIn: false, seedKey: null, builtInVersion: null, lastUpdateTime: null }
}

async function loadSeries() { seriesList.value = await getAdminImageCollectionSeries(keyword.value) }

// --- 系列 CRUD ---
function openCreateSeries() {
  if (!canManageConfigs.value) return
  detailSeries.value = createEmptySeries()
  detailItems.value = []
  detailBonuses.value = []
  detailCreateMode.value = true
  detailOpen.value = true
}

async function saveDetailSeries() {
  if (!canManageConfigs.value || !detailSeries.value) return
  if (!detailSeries.value.name.trim()) { toast.error('系列名称不能为空。'); return }
  try {
    await saveAdminImageCollectionSeries(detailSeries.value)
    toast.success('保存成功。')
    if (detailCreateMode.value) {
      detailCreateMode.value = false
    }
    detailOpen.value = false; await loadSeries()
  } catch (e) { toast.error(e instanceof Error ? e.message : '保存失败。') }
}

async function removeSeries(seriesId: string) {
  if (!canManageConfigs.value || !seriesId) return
  if (!window.confirm(`确认删除系列 ${seriesId} 吗？`)) return
  try {
    await deleteAdminImageCollectionSeries(seriesId)
    toast.success('删除成功。')
    detailOpen.value = false
    await loadSeries()
  } catch (e) { toast.error(e instanceof Error ? e.message : '删除失败。') }
}

// --- 系列详情 ---
async function openDetail(seriesId: string) {
  detailSeries.value = await getAdminImageCollectionSeriesDetail(seriesId)
  detailItems.value = await getAdminImageCollectionItems(seriesId)
  detailBonuses.value = await getAdminImageCollectionBonuses(seriesId)
  detailCreateMode.value = false
  detailOpen.value = true
}

async function reloadDetail() {
  if (!detailSeries.value) return
  detailItems.value = await getAdminImageCollectionItems(detailSeries.value.seriesId)
  detailBonuses.value = await getAdminImageCollectionBonuses(detailSeries.value.seriesId)
}

// --- 图鉴项 CRUD ---
function openCreateItem() { if (!canManageConfigs.value || !detailSeries.value) return; editingItem.value = createEmptyItem(); itemEditorMode.value = 'create'; itemEditorOpen.value = true }
async function openEditItem(itemId: string) { editingItem.value = await getAdminImageCollectionItemDetail(itemId); itemEditorMode.value = 'edit'; itemEditorOpen.value = true }

async function saveItem() {
  if (!canManageConfigs.value || !editingItem.value) return
  if (!editingItem.value.imageName.trim()) { toast.error('图片名称不能为空。'); return }
  try {
    await saveAdminImageCollectionItem(editingItem.value)
    toast.success('保存成功。'); itemEditorOpen.value = false; await reloadDetail()
  } catch (e) { toast.error(e instanceof Error ? e.message : '保存失败。') }
}

async function removeItem(itemId: string) {
  if (!canManageConfigs.value) return
  if (!window.confirm(`确认删除此图鉴项吗？`)) return
  try {
    await deleteAdminImageCollectionItem(itemId)
    toast.success('删除成功。'); await reloadDetail()
  } catch (e) { toast.error(e instanceof Error ? e.message : '删除失败。') }
}

// --- 属性加成 CRUD ---
function openCreateBonus() { if (!canManageConfigs.value || !detailSeries.value) return; editingBonus.value = createEmptyBonus(); bonusEditorMode.value = 'create'; bonusEditorOpen.value = true }
async function openEditBonus(bonusId: string) { editingBonus.value = await getAdminImageCollectionBonusDetail(bonusId); bonusEditorMode.value = 'edit'; bonusEditorOpen.value = true }

async function saveBonus() {
  if (!canManageConfigs.value || !editingBonus.value) return
  try {
    await saveAdminImageCollectionBonus(editingBonus.value)
    toast.success('保存成功。'); bonusEditorOpen.value = false; await reloadDetail()
  } catch (e) { toast.error(e instanceof Error ? e.message : '保存失败。') }
}

async function removeBonus(bonusId: string) {
  if (!canManageConfigs.value) return
  if (!window.confirm(`确认删除此加成吗？`)) return
  try {
    await deleteAdminImageCollectionBonus(bonusId)
    toast.success('删除成功。'); await reloadDetail()
  } catch (e) { toast.error(e instanceof Error ? e.message : '删除失败。') }
}

onMounted(() => { loadSeries() })
</script>

<style scoped>
.col-desc { max-width: 200px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.col-actions { white-space: nowrap; }
.col-actions .link-button { margin-right: 8px; }
.col-actions .link-button:last-child { margin-right: 0; }

.tag-active { color: var(--color-success, #22c55e); font-weight: 600; font-size: 12px; }
.tag-inactive { color: var(--color-muted, #888); font-weight: 600; font-size: 12px; }

.link-button { background: none; border: none; color: var(--color-primary, #6366f1); cursor: pointer; font-size: 13px; padding: 2px 4px; text-decoration: underline; }
.link-button:hover { opacity: 0.8; }
.link-button:disabled { opacity: 0.4; cursor: not-allowed; }
.link-button.danger { color: var(--color-danger, #ef4444); }

.detail-section { display: flex; flex-direction: column; gap: 24px; }

.sub-section { border: 1px solid var(--border-color, #e5e7eb); border-radius: 8px; padding: 16px; }
.sub-section-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 12px; }
.sub-section-title { font-size: 15px; font-weight: 600; margin: 0; display: flex; align-items: center; gap: 8px; }
.count-badge { font-size: 12px; background: var(--color-bg-muted, #f3f4f6); padding: 2px 8px; border-radius: 10px; font-weight: 500; }

.data-table--compact { font-size: 13px; }
.data-table--compact th, .data-table--compact td { padding: 8px 10px; }

.upload-image-row { display: flex; gap: 24px; }
.upload-box { display: flex; flex-direction: column; gap: 8px; align-items: center; }
.upload-label { font-size: 13px; font-weight: 600; color: #374151; }
.upload-preview { width: 120px; height: 120px; border-radius: 10px; border: 1px solid var(--border-color, #e5e7eb); overflow: hidden; }
.upload-preview img { width: 100%; height: 100%; object-fit: cover; }
.upload-placeholder { width: 120px; height: 120px; border-radius: 10px; border: 1px solid var(--border-color, #e5e7eb); background: #f8fafc; display: flex; align-items: center; justify-content: center; font-size: 12px; color: #64748b; }
</style>
