<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">文字图鉴</p>
        <h2 class="section-title section-title--small">文字图鉴管理</h2>
        <p class="section-note">管理文字图鉴系列及其图鉴项、属性加成。</p>
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
    <AdminModal v-model="detailOpen" kicker="系列详情" :title="detailCreateMode ? '新建系列' : `系列：${detailSeries?.name || ''}`" :description="detailSeries?.description || '维护文字图鉴系列的基本信息。'" size="xwide">
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
                  <th>汉字</th>
                  <th>槽位</th>
                  <th>来源</th>
                  <th>操作</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="item in detailItems" :key="item.itemId">
                  <td>{{ item.itemId }}</td>
                  <td>{{ item.character }}</td>
                  <td>{{ item.slotIndex }}</td>
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
    <AdminModal v-model="itemEditorOpen" kicker="文字图鉴项" :title="itemEditorMode === 'create' ? '批量新建图鉴项' : `编辑图鉴项`" :description="itemEditorMode === 'create' ? '粘贴一段文字，自动提取汉字生成图鉴项。' : '维护图鉴项的汉字内容。'" size="xwide">
      <!-- 新建模式：批量提取 -->
      <fieldset v-if="itemEditorMode === 'create'" class="form-fieldset" :disabled="!canManageConfigs">
        <label class="field"><span>粘贴文字</span><textarea v-model="batchText" rows="4" placeholder="粘贴一段话，自动提取其中的汉字（去重、保留顺序）"></textarea></label>
        <div class="filter-actions" style="margin-bottom:12px">
          <button class="secondary-button" type="button" :disabled="!batchText.trim()" @click="extractChars">提取汉字</button>
        </div>
        <div v-if="extractedChars.length" class="batch-preview">
          <p class="batch-preview-info">提取到 <strong>{{ extractedChars.length }}</strong> 个汉字：</p>
          <div class="batch-char-list">
            <span v-for="(c, i) in extractedChars" :key="i" class="batch-char">{{ c }}</span>
          </div>
        </div>
      </fieldset>
      <!-- 编辑模式：单字编辑 -->
      <fieldset v-else-if="editingItem" class="form-fieldset" :disabled="!canManageConfigs">
        <div class="editor-grid editor-grid--three">
          <label class="field"><span>汉字内容</span><input v-model.trim="editingItem.character" type="text" /></label>
        </div>
      </fieldset>
      <template #footer>
        <button class="secondary-button" type="button" @click="itemEditorOpen = false">取消</button>
        <button v-if="itemEditorMode === 'create'" class="primary-button" type="button" :disabled="!canManageConfigs || newCharCount === 0 || batchSaving" @click="batchSaveItems">{{ batchSaving ? '保存中...' : `新增 ${newCharCount} 项` }}</button>
        <button v-else class="primary-button" type="button" :disabled="!canManageConfigs" @click="saveItem">保存</button>
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
import { computed, onMounted, ref } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import AdminImageField from '@/components/AdminImageField.vue'
import { BONUS_VALUE_TYPE_OPTIONS, ATTR_TYPE_OPTIONS } from '@/constants/game-options'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import {
  getAdminTextCollectionSeries, getAdminTextCollectionSeriesDetail, saveAdminTextCollectionSeries, deleteAdminTextCollectionSeries,
  getAdminTextCollectionItems, getAdminTextCollectionItemDetail, saveAdminTextCollectionItem, deleteAdminTextCollectionItem,
  getAdminTextCollectionBonuses, getAdminTextCollectionBonusDetail, saveAdminTextCollectionBonus, deleteAdminTextCollectionBonus
} from '@/services/collections'
import type {
  AdminTextCollectionSeriesListItem, AdminTextCollectionSeriesDetail,
  AdminTextCollectionItemListItem, AdminTextCollectionItemDetail,
  AdminTextCollectionBonusListItem, AdminTextCollectionBonusDetail
} from '@/types/admin'
import toast from '@/utils/toast'

function attrTypeLabel(v: string) { return ATTR_TYPE_OPTIONS.find(o => o.value === v)?.label || v }

const { canManageConfigs } = useAdminPermissions()
const keyword = ref('')
const seriesList = ref<AdminTextCollectionSeriesListItem[]>([])

// 系列详情/编辑
const detailOpen = ref(false)
const detailSeries = ref<AdminTextCollectionSeriesDetail | null>(null)
const detailCreateMode = ref(false)
const detailItems = ref<AdminTextCollectionItemListItem[]>([])
const detailBonuses = ref<AdminTextCollectionBonusListItem[]>([])

// 图鉴项编辑
const itemEditorOpen = ref(false)
const itemEditorMode = ref<'create' | 'edit'>('create')
const editingItem = ref<AdminTextCollectionItemDetail | null>(null)
const batchText = ref('')
const extractedChars = ref<string[]>([])
const batchSaving = ref(false)

// 属性加成编辑
const bonusEditorOpen = ref(false)
const bonusEditorMode = ref<'create' | 'edit'>('create')
const editingBonus = ref<AdminTextCollectionBonusDetail | null>(null)

function formatDateTime(value?: string | null) { return value ? value.replace('T', ' ').slice(0, 19) : '-' }

function genId(prefix: string) { return `${prefix}_${Date.now().toString(36)}_${Math.random().toString(36).slice(2, 6)}` }

function createEmptySeries(): AdminTextCollectionSeriesDetail {
  return { seriesId: genId('txt'), name: '', description: '', icon: '', sortOrder: 0, isEnabled: true, isBuiltIn: false, seedKey: null, builtInVersion: null, lastUpdateTime: null }
}
function createEmptyItem(): AdminTextCollectionItemDetail {
  const sid = detailSeries.value?.seriesId || ''
  return { itemId: genId(sid), seriesId: sid, character: '', slotIndex: detailItems.value.length, sortOrder: 0, isEnabled: true, isBuiltIn: false, seedKey: null, builtInVersion: null, lastUpdateTime: null }
}
function createEmptyBonus(): AdminTextCollectionBonusDetail {
  const sid = detailSeries.value?.seriesId || ''
  return { bonusId: genId(sid + '_b'), seriesId: sid, attrType: 'Attack', attrValue: 0, valueType: 0, sortOrder: 0, isBuiltIn: false, seedKey: null, builtInVersion: null, lastUpdateTime: null }
}

async function loadSeries() { seriesList.value = await getAdminTextCollectionSeries(keyword.value) }

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
    await saveAdminTextCollectionSeries(detailSeries.value)
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
    await deleteAdminTextCollectionSeries(seriesId)
    toast.success('删除成功。')
    detailOpen.value = false
    await loadSeries()
  } catch (e) { toast.error(e instanceof Error ? e.message : '删除失败。') }
}

// --- 系列详情 ---
async function openDetail(seriesId: string) {
  detailSeries.value = await getAdminTextCollectionSeriesDetail(seriesId)
  detailItems.value = await getAdminTextCollectionItems(seriesId)
  detailBonuses.value = await getAdminTextCollectionBonuses(seriesId)
  detailCreateMode.value = false
  detailOpen.value = true
}

async function reloadDetail() {
  if (!detailSeries.value) return
  detailItems.value = await getAdminTextCollectionItems(detailSeries.value.seriesId)
  detailBonuses.value = await getAdminTextCollectionBonuses(detailSeries.value.seriesId)
}

// --- 图鉴项 CRUD ---
function openCreateItem() {
  if (!canManageConfigs.value || !detailSeries.value) return
  batchText.value = ''; extractedChars.value = []; batchSaving.value = false
  itemEditorMode.value = 'create'; itemEditorOpen.value = true
}
async function openEditItem(itemId: string) { editingItem.value = await getAdminTextCollectionItemDetail(itemId); itemEditorMode.value = 'edit'; itemEditorOpen.value = true }

function extractChars() {
  extractedChars.value = batchText.value.match(/[一-鿿]/g) || []
}
const newCharCount = computed(() => extractedChars.value.length)

async function batchSaveItems() {
  if (!canManageConfigs.value || !detailSeries.value) return
  const toCreate = extractedChars.value
  if (toCreate.length === 0) return
  batchSaving.value = true
  try {
    const sid = detailSeries.value.seriesId
    const baseSlot = detailItems.value.length
    for (let i = 0; i < toCreate.length; i++) {
      const item: AdminTextCollectionItemDetail = {
        itemId: genId(sid), seriesId: sid, character: toCreate[i], slotIndex: baseSlot + i,
        sortOrder: 0, isEnabled: true, isBuiltIn: false, seedKey: null, builtInVersion: null, lastUpdateTime: null
      }
      await saveAdminTextCollectionItem(item)
    }
    toast.success(`成功新增 ${toCreate.length} 个图鉴项。`); itemEditorOpen.value = false; await reloadDetail()
  } catch (e) { toast.error(e instanceof Error ? e.message : '批量保存失败。') }
  finally { batchSaving.value = false }
}

async function saveItem() {
  if (!canManageConfigs.value || !editingItem.value) return
  if (!editingItem.value.character.trim()) { toast.error('汉字内容不能为空。'); return }
  try {
    await saveAdminTextCollectionItem(editingItem.value)
    toast.success('保存成功。'); itemEditorOpen.value = false; await reloadDetail()
  } catch (e) { toast.error(e instanceof Error ? e.message : '保存失败。') }
}

async function removeItem(itemId: string) {
  if (!canManageConfigs.value) return
  if (!window.confirm(`确认删除此图鉴项吗？`)) return
  try {
    await deleteAdminTextCollectionItem(itemId)
    toast.success('删除成功。'); await reloadDetail()
  } catch (e) { toast.error(e instanceof Error ? e.message : '删除失败。') }
}

// --- 属性加成 CRUD ---
function openCreateBonus() { if (!canManageConfigs.value || !detailSeries.value) return; editingBonus.value = createEmptyBonus(); bonusEditorMode.value = 'create'; bonusEditorOpen.value = true }
async function openEditBonus(bonusId: string) { editingBonus.value = await getAdminTextCollectionBonusDetail(bonusId); bonusEditorMode.value = 'edit'; bonusEditorOpen.value = true }

async function saveBonus() {
  if (!canManageConfigs.value || !editingBonus.value) return
  try {
    await saveAdminTextCollectionBonus(editingBonus.value)
    toast.success('保存成功。'); bonusEditorOpen.value = false; await reloadDetail()
  } catch (e) { toast.error(e instanceof Error ? e.message : '保存失败。') }
}

async function removeBonus(bonusId: string) {
  if (!canManageConfigs.value) return
  if (!window.confirm(`确认删除此加成吗？`)) return
  try {
    await deleteAdminTextCollectionBonus(bonusId)
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

.batch-preview { background: var(--color-bg-muted, #f3f4f6); border-radius: 8px; padding: 12px 16px; }
.batch-preview-info { font-size: 13px; color: var(--color-text-secondary, #666); margin: 0 0 8px; }
.batch-char-list { display: flex; flex-wrap: wrap; gap: 6px; }
.batch-char { display: inline-flex; align-items: center; justify-content: center; width: 32px; height: 32px; border-radius: 6px; background: var(--color-primary-soft, #eef2ff); color: var(--color-primary, #6366f1); font-size: 16px; font-weight: 600; border: 1px solid var(--color-primary, #6366f1); }
</style>
