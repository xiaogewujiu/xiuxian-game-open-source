<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">宝石系统</p>
        <h2 class="section-title section-title--small">宝石管理</h2>
        <p class="section-note">管理宝石模板、批量生成宝石系列。</p>
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

    <!-- Tab 1: 宝石模板 -->
    <template v-if="activeTab === 'templates'">
      <section class="panel-box filter-panel">
        <div class="section-header-copy">
          <p class="section-kicker">宝石模板</p>
          <h3 class="section-title section-title--small">宝石模板管理</h3>
        </div>
        <div class="filter-actions">
          <button class="primary-button" type="button" @click="openCreate">新增宝石</button>
          <button class="secondary-button" type="button" @click="openBatchGenerate">批量生成</button>
        </div>
      </section>

      <section class="panel-box table-panel">
        <div class="section-header-row">
          <div class="section-header-copy">
            <p class="section-kicker">宝石列表</p>
            <h3 class="section-title section-title--small">宝石模板列表</h3>
          </div>
          <span class="selected-pill">共 {{ templates.length }} 个</span>
        </div>
        <div class="table-wrap">
          <table class="data-table">
            <thead>
              <tr>
                <th>宝石ID</th>
                <th>名称</th>
                <th>等级</th>
                <th>属性</th>
                <th>加成</th>
                <th>模式</th>
                <th>品质</th>
                <th>合成来源</th>
                <th>成功率</th>
                <th>操作</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="g in templates" :key="g.id">
                <td>{{ g.gemId }}</td>
                <td>{{ g.name }}</td>
                <td>Lv.{{ g.level }}</td>
                <td>{{ ATTRIBUTE_TYPE_LABELS[g.attributeType] || g.attributeType }}</td>
                <td>{{ g.bonusMode === 'Percent' ? g.bonusValue + '%' : g.bonusValue }}</td>
                <td>{{ g.bonusMode === 'Flat' ? '固定值' : '百分比' }}</td>
                <td>{{ g.quality }}</td>
                <td>{{ g.synthFromGemId || '-' }}</td>
                <td>{{ g.synthSuccessRate }}%</td>
                <td>
                  <button class="secondary-button secondary-button--sm" type="button" @click="openEdit(g)">编辑</button>
                  <button class="secondary-button secondary-button--sm" type="button" @click="handleDelete(g)">删除</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </template>

    <!-- Tab 2: 合成链 -->
    <template v-if="activeTab === 'synth'">
      <section class="panel-box filter-panel">
        <div class="section-header-copy">
          <p class="section-kicker">合成链</p>
          <h3 class="section-title section-title--small">宝石合成关系</h3>
        </div>
      </section>

      <section class="panel-box table-panel">
        <div class="table-wrap">
          <table class="data-table">
            <thead>
              <tr>
                <th>低级宝石</th>
                <th>合成数量</th>
                <th>高级宝石</th>
                <th>成功率</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="chain in synthChains" :key="chain.fromGemId">
                <td>{{ chain.fromName }} (Lv.{{ chain.fromLevel }})</td>
                <td>{{ chain.synthCount }} → 1</td>
                <td>{{ chain.toName }} (Lv.{{ chain.toLevel }})</td>
                <td>{{ chain.successRate }}%</td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </template>

    <!-- 编辑弹窗 -->
    <AdminModal
      v-model="editModalOpen"
      kicker="宝石模板"
      :title="isCreating ? '新增宝石模板' : '编辑宝石模板'"
      description="配置宝石的属性、数值和合成关系。"
    >
      <div v-if="editForm" class="editor-grid editor-grid--three">
        <label class="field"><span>宝石ID</span><input v-model.trim="editForm.gemId" type="text" /></label>
        <label class="field"><span>名称</span><input v-model.trim="editForm.name" type="text" /></label>
        <label class="field"><span>等级</span><input v-model.number="editForm.level" type="number" min="1" max="10" /></label>
        <label class="field"><span>属性类型</span>
          <select v-model="editForm.attributeType">
            <option v-for="t in ATTRIBUTE_TYPES" :key="t" :value="t">{{ ATTRIBUTE_TYPE_LABELS[t] || t }}</option>
          </select>
        </label>
        <label class="field"><span>加成数值</span><input v-model.number="editForm.bonusValue" type="number" min="0" /></label>
        <label class="field"><span>加成模式</span>
          <select v-model="editForm.bonusMode">
            <option value="Flat">固定值</option>
            <option value="Percent">百分比</option>
          </select>
        </label>
        <label class="field"><span>品质</span><input v-model.number="editForm.quality" type="number" min="1" max="5" /></label>
        <label class="field"><span>合成所需数量</span><input v-model.number="editForm.synthCount" type="number" min="1" /></label>
        <label class="field"><span>合成来源宝石</span>
          <select v-model="editForm.synthFromGemId">
            <option value="">无（不合成）</option>
            <option v-for="g in templates" :key="g.gemId" :value="g.gemId">{{ g.name }} (Lv.{{ g.level }})</option>
          </select>
        </label>
        <label class="field"><span>合成成功率 (%)</span><input v-model.number="editForm.synthSuccessRate" type="number" min="1" max="100" /></label>
      </div>
      <template #footer>
        <button class="secondary-button" type="button" @click="editModalOpen = false">取消</button>
        <button class="primary-button" type="button" @click="handleSave">保存</button>
      </template>
    </AdminModal>

    <!-- 批量生成弹窗 -->
    <AdminModal
      v-model="batchModalOpen"
      kicker="批量生成"
      title="批量生成宝石模板"
      description="按规则批量生成宝石系列。已存在的等级会被跳过。"
    >
      <div class="editor-grid editor-grid--three">
        <label class="field"><span>属性类型</span>
          <select v-model="batchForm.attributeType">
            <option v-for="t in ATTRIBUTE_TYPES" :key="t" :value="t">{{ ATTRIBUTE_TYPE_LABELS[t] || t }}</option>
          </select>
        </label>
        <label class="field"><span>基础加成值</span><input v-model.number="batchForm.baseBonusValue" type="number" /></label>
        <label class="field"><span>每级增长</span><input v-model.number="batchForm.bonusValueGrowth" type="number" /></label>
        <label class="field"><span>加成模式</span>
          <select v-model="batchForm.bonusMode">
            <option value="Flat">固定值</option>
            <option value="Percent">百分比</option>
          </select>
        </label>
        <label class="field"><span>最大等级</span><input v-model.number="batchForm.maxLevel" type="number" min="1" max="10" /></label>
        <label class="field"><span>名称前缀</span><input v-model.trim="batchForm.namePrefix" type="text" /></label>
        <label class="field"><span>合成成功率 (%)</span><input v-model.number="batchForm.synthSuccessRate" type="number" min="1" max="100" /></label>
      </div>
      <template #footer>
        <button class="secondary-button" type="button" @click="batchModalOpen = false">取消</button>
        <button class="primary-button" type="button" @click="doBatchGenerate">生成</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, computed, reactive } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import toast from '@/utils/toast'
import { getGemTemplates, createGemTemplate, updateGemTemplate, deleteGemTemplate, batchGenerateGems } from '@/services/gem'
import type { AdminGemListItem, AdminGemSavePayload, AdminGemBatchGeneratePayload } from '@/types/admin'

const TAB_OPTIONS = [
  { value: 'templates', label: '宝石模板' },
  { value: 'synth', label: '合成链' }
] as const

type TabValue = (typeof TAB_OPTIONS)[number]['value']

const ATTRIBUTE_TYPES = ['Type1', 'Type2', 'Type3', 'Type4', 'Type5', 'Type6', 'Type7',
  'Type8', 'Type9', 'Type10', 'Type11', 'Type12', 'Type13', 'Type14', 'Type15']

const ATTRIBUTE_TYPE_LABELS: Record<string, string> = {
  Type1: '生命', Type2: '法力', Type3: '物攻', Type4: '法攻', Type5: '物防',
  Type6: '法防', Type7: '速度', Type8: '暴击率', Type9: '暴击伤害', Type10: '闪避率',
  Type11: '命中率', Type12: '吸血', Type13: '反伤', Type14: '治疗加成', Type15: '伤害减免'
}

const activeTab = ref<TabValue>('templates')
const templates = ref<AdminGemListItem[]>([])

const editModalOpen = ref(false)
const isCreating = ref(false)
const editingId = ref<number | null>(null)
const editForm = ref<AdminGemSavePayload | null>(null)

const batchModalOpen = ref(false)
const batchForm = reactive<AdminGemBatchGeneratePayload>({
  attributeType: 'Type3',
  baseBonusValue: 10,
  bonusValueGrowth: 5,
  bonusMode: 'Flat',
  maxLevel: 10,
  namePrefix: '物攻宝石',
  synthSuccessRate: 100
})

const synthChains = computed(() => {
  return templates.value
    .filter(g => g.synthFromGemId)
    .map(g => {
      const from = templates.value.find(t => t.gemId === g.synthFromGemId)
      return {
        fromGemId: g.synthFromGemId!,
        fromName: from?.name ?? g.synthFromGemId!,
        fromLevel: from?.level ?? 0,
        synthCount: g.synthCount,
        toName: g.name,
        toLevel: g.level,
        successRate: g.synthSuccessRate
      }
    })
    .sort((a, b) => a.fromLevel - b.fromLevel)
})

async function loadTemplates() {
  templates.value = await getGemTemplates()
}

function openCreate() {
  isCreating.value = true
  editingId.value = null
  editForm.value = {
    gemId: '',
    name: '',
    level: 1,
    attributeType: 'Type3',
    bonusValue: 10,
    bonusMode: 'Flat',
    quality: 1,
    synthCount: 3,
    synthFromGemId: '',
    synthSuccessRate: 100
  }
  editModalOpen.value = true
}

function openEdit(g: AdminGemListItem) {
  isCreating.value = false
  editingId.value = g.id
  editForm.value = {
    gemId: g.gemId,
    name: g.name,
    level: g.level,
    attributeType: g.attributeType,
    bonusValue: g.bonusValue,
    bonusMode: g.bonusMode,
    quality: g.quality,
    synthCount: g.synthCount,
    synthFromGemId: g.synthFromGemId || '',
    synthSuccessRate: g.synthSuccessRate
  }
  editModalOpen.value = true
}

async function handleSave() {
  if (!editForm.value) return
  try {
    if (isCreating.value) {
      await createGemTemplate(editForm.value)
      toast.success('宝石模板已创建。')
    } else {
      await updateGemTemplate(editingId.value!, editForm.value)
      toast.success('宝石模板已更新。')
    }
    editModalOpen.value = false
    await loadTemplates()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '保存失败。')
  }
}

async function handleDelete(g: AdminGemListItem) {
  if (!confirm(`确定删除宝石 "${g.name}"？`)) return
  try {
    await deleteGemTemplate(g.id)
    toast.success('宝石模板已删除。')
    await loadTemplates()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '删除失败。')
  }
}

function openBatchGenerate() {
  batchModalOpen.value = true
}

async function doBatchGenerate() {
  try {
    const count = await batchGenerateGems(batchForm)
    toast.success(`生成了 ${count} 个宝石模板。`)
    batchModalOpen.value = false
    await loadTemplates()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '生成失败。')
  }
}

onMounted(loadTemplates)
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

.secondary-button--sm {
  padding: 0.25rem 0.5rem;
  font-size: 0.75rem;
  margin-right: 0.25rem;
}
</style>
