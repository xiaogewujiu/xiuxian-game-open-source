<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">装备分解规则</p>
        <h2 class="section-title section-title--small">装备分解产出</h2>
        <p class="section-note">按装备品质配置分解后的指定道具和数量区间。已穿戴、已锁定装备始终不能分解。</p>
      </div>
      <div class="filter-actions filter-actions--inline">
        <button class="secondary-button" type="button" @click="loadRules" :disabled="loading">刷新</button>
        <button class="primary-button" type="button" @click="openCreate" :disabled="!canManageConfigs || loading">新增分解规则</button>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">品质规则</p>
          <h3 class="section-title section-title--small">分解产出规则</h3>
        </div>
        <span class="selected-pill">共 {{ rules.length }} 条</span>
      </div>
      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr><th>品质</th><th>产出道具 ID</th><th>数量区间</th><th>状态</th><th>来源</th><th>操作</th></tr>
          </thead>
          <tbody>
            <tr v-for="rule in rules" :key="rule.gid" @click="openEdit(rule)">
              <td>{{ qualityName(rule.quality) }}</td>
              <td>{{ rule.materialItemId }}</td>
              <td>{{ rule.minQuantity }} - {{ rule.maxQuantity }}</td>
              <td><span :class="rule.isEnabled ? 'status-on' : 'status-off'">{{ rule.isEnabled ? '启用' : '停用' }}</span></td>
              <td>{{ rule.isBuiltIn ? '内置' : '配置' }}</td>
              <td><button class="danger-button" type="button" @click.stop="removeRule(rule)" :disabled="!canManageConfigs">删除</button></td>
            </tr>
            <tr v-if="!rules.length"><td colspan="6" class="empty-state">暂无分解规则</td></tr>
          </tbody>
        </table>
      </div>
    </section>

    <AdminModal v-model="editorOpen" kicker="装备分解规则" :title="editor?.gid ? '编辑分解规则' : '新增分解规则'" description="同一品质只能有一条启用规则，产出道具必须存在。">
      <fieldset v-if="editor" class="form-fieldset" :disabled="!canManageConfigs">
        <div class="editor-grid editor-grid--three">
          <label class="field"><span>装备品质</span><select v-model.number="editor.quality"><option v-for="quality in 5" :key="quality" :value="quality">{{ qualityName(quality) }}</option></select></label>
          <label class="field"><span>产出道具 ID</span><input v-model.trim="editor.materialItemId" type="text" placeholder="例如 itm_essence_s01" /></label>
          <label class="field"><span>排序</span><input v-model.number="editor.sortOrder" type="number" min="0" /></label>
          <label class="field"><span>最少数量</span><input v-model.number="editor.minQuantity" type="number" min="1" /></label>
          <label class="field"><span>最多数量</span><input v-model.number="editor.maxQuantity" type="number" min="1" /></label>
          <label class="field checkbox-field"><input v-model="editor.isEnabled" type="checkbox" /><span>启用规则</span></label>
        </div>
      </fieldset>
      <template #footer>
        <button class="secondary-button" type="button" @click="editorOpen = false">取消</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs || saving" @click="saveRule">保存</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import type { AdminEquipmentDecomposeRule } from '@/types/admin'
import { deleteEquipmentDecomposeRule, getEquipmentDecomposeRules, saveEquipmentDecomposeRule } from '@/services/equipment-decompose-rules'
import toast from '@/utils/toast'

const { canManageConfigs } = useAdminPermissions()
const loading = ref(false)
const saving = ref(false)
const rules = ref<AdminEquipmentDecomposeRule[]>([])
const editorOpen = ref(false)
const editor = ref<AdminEquipmentDecomposeRule | null>(null)

function qualityName(quality: number) {
  return ({ 1: '普通', 2: '优秀', 3: '精良', 4: '史诗', 5: '传说' } as Record<number, string>)[quality] || `品质 ${quality}`
}

function openCreate() {
  editor.value = { gid: 0, quality: 1, materialItemId: 'itm_essence_s01', minQuantity: 1, maxQuantity: 1, sortOrder: 0, isEnabled: true, isBuiltIn: false }
  editorOpen.value = true
}

function openEdit(rule: AdminEquipmentDecomposeRule) {
  editor.value = { ...rule }
  editorOpen.value = true
}

async function loadRules() {
  loading.value = true
  try { rules.value = await getEquipmentDecomposeRules() } catch (error: any) { toast.error(error.message || '读取分解规则失败') } finally { loading.value = false }
}

async function saveRule() {
  if (!editor.value) return
  saving.value = true
  try {
    await saveEquipmentDecomposeRule(editor.value)
    toast.success('装备分解规则保存成功')
    editorOpen.value = false
    await loadRules()
  } catch (error: any) { toast.error(error.message || '保存分解规则失败') } finally { saving.value = false }
}

async function removeRule(rule: AdminEquipmentDecomposeRule) {
  if (!window.confirm(`确定删除 ${qualityName(rule.quality)} 品质分解规则？`)) return
  try { await deleteEquipmentDecomposeRule(rule.gid); toast.success('分解规则删除成功'); await loadRules() } catch (error: any) { toast.error(error.message || '删除分解规则失败') }
}

onMounted(loadRules)
</script>

<style scoped>
.status-on { color: #22c55e; font-weight: 600; }
.status-off { color: #ef4444; font-weight: 600; }
.empty-state { color: var(--text-muted, #666); text-align: center; padding: 16px 0; }
</style>
