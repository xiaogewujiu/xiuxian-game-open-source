<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">称号</p>
        <h2 class="section-title section-title--small">称号管理</h2>
        <p class="section-note">管理称号模板、发放和回收玩家称号。</p>
      </div>

      <div class="tab-bar">
        <button
          v-for="tab in TAB_OPTIONS"
          :key="tab.value"
          class="tab-button"
          :class="{ 'is-active': activeTab === tab.value }"
          type="button"
          @click="activeTab = tab.value"
        >{{ tab.label }}</button>
      </div>
    </section>

    <!-- Tab 1: 称号模板 -->
    <template v-if="activeTab === 'templates'">
      <section class="panel-box filter-panel">
        <div class="section-header-copy">
          <p class="section-kicker">模板查询</p>
          <h3 class="section-title section-title--small">称号模板</h3>
        </div>
        <div class="filter-grid">
          <label class="field">
            <span>关键字</span>
            <input v-model.trim="keyword" type="text" placeholder="搜索称号ID或名称" />
          </label>
          <div class="filter-actions">
            <button class="secondary-button" type="button" @click="loadTemplates">查询</button>
            <button class="primary-button" type="button" @click="openCreate">新建称号</button>
          </div>
        </div>
      </section>

      <section class="panel-box table-panel">
        <div class="section-header-row">
          <div class="section-header-copy">
            <p class="section-kicker">模板列表</p>
            <h3 class="section-title section-title--small">称号模板列表</h3>
          </div>
          <span class="selected-pill">共 {{ templates.length }} 条</span>
        </div>
        <div class="table-wrap">
          <table class="data-table">
            <thead>
              <tr>
                <th>称号ID</th>
                <th>名称</th>
                <th>来源</th>
                <th>稀有度</th>
                <th>可见</th>
                <th>操作</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="t in templates" :key="t.id">
                <td>{{ t.titleId }}</td>
                <td>{{ t.name }}</td>
                <td>{{ t.source }}</td>
                <td>{{ getRarityLabel(t.rarity) }}</td>
                <td>{{ t.isVisible ? '是' : '否' }}</td>
                <td>
                  <button class="secondary-button secondary-button--sm" type="button" @click="openEdit(t)">编辑</button>
                  <button class="danger-button danger-button--sm" type="button" @click="removeTemplate(t)">删除</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </template>

    <!-- Tab 2: 发放/回收 -->
    <template v-if="activeTab === 'grant'">
      <section class="panel-box filter-panel">
        <div class="section-header-copy">
          <p class="section-kicker">称号发放</p>
          <h3 class="section-title section-title--small">发放 / 回收称号</h3>
        </div>
        <div class="filter-grid">
          <label class="field">
            <span>玩家</span>
            <select v-model="grantPlayerId">
              <option value="">请选择玩家</option>
              <option v-for="p in playerOptions" :key="p.playerId" :value="p.playerId">{{ p.name }} (Lv.{{ p.level }})</option>
            </select>
          </label>
          <label class="field">
            <span>称号</span>
            <select v-model="grantTitleId">
              <option value="">请选择称号</option>
              <option v-for="t in templates" :key="t.titleId" :value="t.titleId">{{ t.name }} ({{ t.titleId }})</option>
            </select>
          </label>
          <div class="filter-actions">
            <button class="primary-button" type="button" @click="doGrant">发放</button>
            <button class="danger-button" type="button" @click="doRevoke">回收</button>
          </div>
        </div>
      </section>
    </template>

    <!-- 新建/编辑称号弹窗 -->
    <AdminModal
      v-model="modalOpen"
      kicker="称号模板"
      :title="editingId ? '编辑称号模板' : '新建称号模板'"
      description="配置称号的基本信息、来源和展示属性。"
    >
      <div v-if="form">
        <div class="editor-grid editor-grid--three">
          <label v-if="editingId" class="field"><span>称号ID</span><input :value="form.titleId" type="text" disabled /></label>
          <label class="field"><span>名称</span><input v-model.trim="form.name" type="text" /></label>
          <label class="field"><span>来源</span>
            <select v-model="form.source">
              <option value="Admin">后台</option>
              <option value="Achievement">成就</option>
              <option value="Arena">竞技场</option>
              <option value="Tower">通天塔</option>
            </select>
          </label>
          <label class="field"><span>来源ID</span><input v-model.trim="form.sourceId" type="text" placeholder="可选" /></label>
          <label class="field"><span>稀有度</span>
            <select v-model="form.rarity">
              <option value="Common">普通</option>
              <option value="Rare">稀有</option>
              <option value="Epic">史诗</option>
              <option value="Legendary">传说</option>
            </select>
          </label>
          <label class="field"><span>描述</span><input v-model.trim="form.description" type="text" placeholder="可选" /></label>
          <label class="field checkbox-field">
            <input v-model="form.isVisible" type="checkbox" />
            <span>在列表中可见</span>
          </label>
        </div>
        <div style="margin-top: 0.5rem;">
          <AdminImageField v-model="form.imagePath" label="称号图片（小图标自动生成）" upload-category="title-image" placeholder="上传称号图片" :preview-width="160" :preview-height="60" />
        </div>
      </div>
      <template #footer>
        <button class="secondary-button" type="button" @click="modalOpen = false">取消</button>
        <button class="primary-button" type="button" @click="save">保存</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import AdminImageField from '@/components/AdminImageField.vue'
import toast from '@/utils/toast'
import {
  getAdminTitles,
  getAdminTitleDetail,
  createAdminTitle,
  updateAdminTitle,
  deleteAdminTitle,
  grantTitle,
  revokeTitle
} from '@/services/title'
import { getAdminPlayers } from '@/services/players'
import type { AdminTitleListItem, AdminTitleDetail, AdminPlayerListItem } from '@/types/admin'

const TAB_OPTIONS = [
  { value: 'templates', label: '称号模板' },
  { value: 'grant', label: '发放/回收' }
] as const

type TabValue = (typeof TAB_OPTIONS)[number]['value']

const activeTab = ref<TabValue>('templates')
const keyword = ref('')
const templates = ref<AdminTitleListItem[]>([])

const RARITY_LABELS: Record<string, string> = {
  Common: '普通',
  Rare: '稀有',
  Epic: '史诗',
  Legendary: '传说'
}

function getRarityLabel(rarity: string) {
  return RARITY_LABELS[rarity] || rarity
}

// ===== 模板编辑 =====
const modalOpen = ref(false)
const editingId = ref<number | null>(null)
const form = ref<AdminTitleDetail | null>(null)

// 称号图片变化时自动同步到小图标
watch(() => form.value?.imagePath, (val) => {
  if (form.value && val) {
    form.value.iconPath = val
  }
})

// ===== 发放/回收 =====
const grantPlayerId = ref('')
const grantTitleId = ref('')
const playerOptions = ref<AdminPlayerListItem[]>([])

async function loadTemplates() {
  templates.value = await getAdminTitles(keyword.value)
}

function openCreate() {
  editingId.value = null
  form.value = {
    id: 0,
    titleId: '',
    name: '',
    description: '',
    source: 'Admin',
    sourceId: '',
    rarity: 'Common',
    iconPath: '',
    imagePath: '',
    isVisible: true,
    createdAt: ''
  }
  modalOpen.value = true
}

async function openEdit(item: AdminTitleListItem) {
  editingId.value = item.id
  const detail = await getAdminTitleDetail(item.id)
  // 确保 iconPath 与 imagePath 同步（兼容旧数据）
  if (!detail.iconPath && detail.imagePath) {
    detail.iconPath = detail.imagePath
  }
  form.value = { ...detail }
  modalOpen.value = true
}

async function save() {
  if (!form.value) return
  try {
    if (editingId.value) {
      await updateAdminTitle(editingId.value, form.value)
      toast.success('称号模板更新成功。')
    } else {
      const { id, createdAt, ...payload } = form.value
      await createAdminTitle(payload as AdminTitleDetail)
      toast.success('称号模板创建成功。')
    }
    modalOpen.value = false
    await loadTemplates()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '保存失败。')
  }
}

async function removeTemplate(item: AdminTitleListItem) {
  if (!window.confirm(`确认删除称号模板「${item.name}」（${item.titleId}）吗？`)) return
  try {
    await deleteAdminTitle(item.id)
    toast.success('称号模板已删除。')
    await loadTemplates()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '删除失败。')
  }
}

async function doGrant() {
  if (!grantPlayerId.value || !grantTitleId.value) {
    toast.error('请填写玩家ID和称号ID。')
    return
  }
  try {
    await grantTitle({ playerId: grantPlayerId.value, titleId: grantTitleId.value })
    toast.success('称号已发放。')
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '发放失败。')
  }
}

async function doRevoke() {
  if (!grantPlayerId.value || !grantTitleId.value) {
    toast.error('请填写玩家ID和称号ID。')
    return
  }
  try {
    await revokeTitle({ playerId: grantPlayerId.value, titleId: grantTitleId.value })
    toast.success('称号已回收。')
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '回收失败。')
  }
}

onMounted(async () => {
  await loadTemplates()
  playerOptions.value = await getAdminPlayers()
})
</script>

<style scoped>
.tab-bar {
  display: flex;
  gap: 0.5rem;
  margin-top: 1rem;
  flex-wrap: wrap;
}

.tab-button {
  padding: 0.5rem 1rem;
  border: 1px solid var(--border-color, #e2e8f0);
  border-radius: 6px;
  background: transparent;
  cursor: pointer;
  font-size: 0.875rem;
  color: var(--text-secondary, #64748b);
  transition: all 0.15s ease;
}

.tab-button.is-active {
  background: var(--primary-color, #3b82f6);
  color: #fff;
  border-color: var(--primary-color, #3b82f6);
}

.tab-button:hover:not(.is-active) {
  background: var(--bg-hover, #f1f5f9);
}

.secondary-button--sm,
.danger-button--sm {
  padding: 0.25rem 0.5rem;
  font-size: 0.75rem;
}

.checkbox-field {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.checkbox-field input[type="checkbox"] {
  width: auto;
}
</style>
