<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">增益效果</p>
        <h2 class="section-title section-title--small">Buff 模板</h2>
        <p class="section-note">统一使用表格展示 Buff 模板，新增和编辑走弹窗表单。</p>
      </div>

      <div class="filter-grid">
        <label class="field">
          <span>关键字</span>
          <input v-model.trim="keyword" type="text" placeholder="搜索 Buff 编号或名称" />
        </label>
        <label class="field">
          <span>当前 Buff</span>
          <input :value="selectedBuff?.buffId || ''" type="text" placeholder="未选择" disabled />
        </label>
        <label class="field">
          <span>持续回合</span>
          <input :value="selectedBuff ? String(selectedBuff.duration) : ''" type="text" placeholder="全部" disabled />
        </label>

        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadBuffs">查询</button>
          <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="openCreateModal">新建 Buff</button>
        </div>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">效果列表</p>
          <h3 class="section-title section-title--small">Buff 列表</h3>
        </div>
        <span class="selected-pill">共 {{ buffs.length }} 条</span>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>Buff 编号</th>
              <th>名称</th>
              <th>来源</th>
              <th>持续回合</th>
              <th>最大层数</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="buff in buffs"
              :key="buff.buffId"
              :class="{ 'is-active': selectedBuff?.buffId === buff.buffId }"
              @click="openEditModal(buff.buffId)"
            >
              <td>{{ buff.buffId }}</td>
              <td>{{ buff.name }}</td>
              <td>{{ buff.isBuiltIn ? '内置' : '人工' }}</td>
              <td>{{ buff.duration }}</td>
              <td>{{ buff.maxStack }}</td>
            </tr>
          </tbody>
        </table>
      </div>

    </section>

    <AdminModal
      v-model="editorOpen"
      kicker="效果编辑"
      :title="selectedBuff?.buffId ? `编辑 Buff ${selectedBuff.buffId}` : '新建 Buff'"
      description="维护 Buff 的持续时间、叠层规则和效果列表。"
      size="wide"
    >
      <fieldset v-if="selectedBuff" class="form-fieldset" :disabled="!canManageConfigs">
        <div class="detail-overview">
          <article class="detail-overview__item"><span>来源</span><strong>{{ selectedBuff.isBuiltIn ? '系统内置' : '人工配置' }}</strong></article>
          <article class="detail-overview__item"><span>种子键</span><strong>{{ selectedBuff.seedKey || '-' }}</strong></article>
          <article class="detail-overview__item"><span>内置版本</span><strong>{{ selectedBuff.builtInVersion || '-' }}</strong></article>
          <article class="detail-overview__item"><span>最后更新</span><strong>{{ formatDateTime(selectedBuff.lastUpdateTime) }}</strong></article>
        </div>
        <div class="editor-grid editor-grid--three">
          <label class="field"><span>Buff 编号</span><input v-model.trim="selectedBuff.buffId" type="text" /></label>
          <label class="field"><span>Buff 名称</span><input v-model.trim="selectedBuff.name" type="text" /></label>
          <label class="field"><span>持续回合</span><input v-model.number="selectedBuff.duration" type="number" /></label>
          <label class="field"><span>最大层数</span><input v-model.number="selectedBuff.maxStack" type="number" min="1" /></label>
          <label class="field"><span>叠层规则</span><select v-model.number="selectedBuff.stackRule"><option v-for="option in STACK_RULE_OPTIONS" :key="option.value" :value="option.value">{{ option.label }}</option></select></label>
        </div>

        <label class="field"><span>描述</span><textarea v-model="selectedBuff.description" rows="3"></textarea></label>
        <BuffEffectsEditor v-model="selectedBuff.effects" />
      </fieldset>

      <template #footer>
        <button class="secondary-button" type="button" @click="editorOpen = false">取消</button>
        <button class="danger-button" type="button" :disabled="!canManageConfigs || !selectedBuff?.buffId" @click="removeBuff">删除</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="saveBuff">保存 Buff</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import { STACK_RULE_OPTIONS } from '@/constants/game-options'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import type { AdminBuffDetail, AdminBuffListItem } from '@/types/admin'
import { deleteAdminBuff, getAdminBuffDetail, getAdminBuffs, saveAdminBuff } from '@/services/skills-buffs'
import BuffEffectsEditor from '@/components/editors/BuffEffectsEditor.vue'
import toast from '@/utils/toast'

// Buff 页维护的是模板配置，不是运行时 Buff 状态。
const { canManageConfigs } = useAdminPermissions()
const buffs = ref<AdminBuffListItem[]>([])
const keyword = ref('')
const selectedBuff = ref<AdminBuffDetail | null>(null)
const editorOpen = ref(false)

// 统一格式化后台时间字段。
function formatDateTime(value?: string | null) {
  if (!value) return '-'
  return value.replace('T', ' ').slice(0, 19)
}

// 新建 Buff 时的默认对象。
function createEmptyBuff(): AdminBuffDetail {
  return {
    buffId: '',
    name: '',
    description: '',
    duration: 1,
    maxStack: 1,
    stackRule: 0,
    effects: [],
    isBuiltIn: false,
    seedKey: null,
    builtInVersion: null,
    lastUpdateTime: null
  }
}

// 叠层规则值转中文标签。
function getStackRuleLabel(value: number) {
  return STACK_RULE_OPTIONS.find((option) => option.value === value)?.label || `规则 ${value}`
}

// 打开新建弹窗。
function openCreateModal() {
  if (!canManageConfigs.value) return
  selectedBuff.value = createEmptyBuff()
      editorOpen.value = true
}

// 拉取 Buff 列表。
async function loadBuffs() {
  buffs.value = await getAdminBuffs(keyword.value)
}

// 打开 Buff 详情弹窗。
async function openEditModal(buffId: string) {
  selectedBuff.value = await getAdminBuffDetail(buffId)
      editorOpen.value = true
}

// 保存 Buff 模板。
async function saveBuff() {
  if (!canManageConfigs.value || !selectedBuff.value) return

  try {
    selectedBuff.value = await saveAdminBuff(selectedBuff.value)
    toast.success('Buff 模板保存成功。')
    editorOpen.value = false
    await loadBuffs()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : 'Buff 模板保存失败。')
  }
}

// 删除 Buff 模板。
async function removeBuff() {
  if (!canManageConfigs.value || !selectedBuff.value?.buffId) return
  if (!window.confirm(`确认删除 Buff 模板 ${selectedBuff.value.buffId} 吗？`)) return

  try {
    await deleteAdminBuff(selectedBuff.value.buffId)
    toast.success('Buff 模板删除成功。')
    selectedBuff.value = null
    editorOpen.value = false
    await loadBuffs()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : 'Buff 模板删除失败。')
  }
}

onMounted(loadBuffs)
</script>
