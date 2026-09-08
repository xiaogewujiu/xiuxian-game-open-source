<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">心法</p>
        <h2 class="section-title section-title--small">心法配置</h2>
        <p class="section-note">编辑心法层数、属性加成与技能解锁。可按宗门筛选。</p>
      </div>

      <div class="filter-grid">
        <label class="field">
          <span>筛选宗门</span>
          <select v-model="filterSectId">
            <option value="">全部宗门</option>
            <option v-for="sect in sects" :key="sect.sectId" :value="sect.sectId">{{ sect.name }} ({{ sect.sectId }})</option>
          </select>
        </label>
        <label class="field">
          <span>当前心法</span>
          <input :value="selected?.sutraId || ''" type="text" placeholder="未选择" disabled />
        </label>

        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadList">查询</button>
          <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="openCreateModal">新建心法</button>
        </div>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">心法列表</p>
          <h3 class="section-title section-title--small">心法列表</h3>
        </div>
        <span class="selected-pill">共 {{ list.length }} 条</span>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>编号</th>
              <th>名称</th>
              <th>所属宗门</th>
              <th>最大层数</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="item in list"
              :key="item.sutraId"
              :class="{ 'is-active': selected?.sutraId === item.sutraId }"
              @click="openEditModal(item.sutraId)"
            >
              <td>{{ item.sutraId }}</td>
              <td>{{ item.name }}</td>
              <td>{{ item.sectName }}</td>
              <td>{{ item.maxLayer }}</td>
            </tr>
          </tbody>
        </table>
      </div>

    </section>

    <AdminModal
      v-model="editorOpen"
      kicker="心法编辑"
      :title="selected?.sutraId ? `编辑心法 ${selected.sutraId}` : '新建心法'"
      description="配置心法层数、属性加成和技能解锁规则。"
      size="xwide"
    >
      <fieldset v-if="selected" class="form-fieldset" :disabled="!canManageConfigs">
        <div class="editor-grid editor-grid--three">
          <label class="field"><span>心法编号</span><input v-model.trim="selected.sutraId" type="text" /></label>
          <label class="field"><span>心法名称</span><input v-model.trim="selected.name" type="text" /></label>
          <label class="field"><span>所属宗门</span>
            <select v-model="selected.sectId">
              <option value="">请选择</option>
              <option v-for="sect in sects" :key="sect.sectId" :value="sect.sectId">{{ sect.name }} ({{ sect.sectId }})</option>
            </select>
          </label>
          <label class="field"><span>最大层数</span><input v-model.number="selected.maxLayer" type="number" min="1" /></label>
          <label class="field"><span>排序值</span><input v-model.number="selected.sortOrder" type="number" /></label>
        </div>
        <label class="field"><span>心法描述</span><textarea v-model="selected.description" rows="3"></textarea></label>
        <SutraLayerEditor
          v-model="selected.layersJson"
          :max-layer="selected.maxLayer"
          :skills="skills"
          :buffs="buffs"
        />
      </fieldset>

      <template #footer>
        <button class="secondary-button" type="button" @click="editorOpen = false">取消</button>
        <button class="danger-button" type="button" :disabled="!canManageConfigs || !selected?.sutraId" @click="remove">删除</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="save">保存心法</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import SutraLayerEditor from '@/components/editors/SutraLayerEditor.vue'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import { getHeartSutras, getHeartSutra, saveHeartSutra, deleteHeartSutra, getSectTemplates } from '@/services/sect'
import { getAdminSkills } from '@/services/skills-buffs'
import { getAdminBuffs } from '@/services/skills-buffs'
import type { AdminHeartSutraDetail, AdminHeartSutraListItem, AdminSectTemplateListItem, AdminSkillListItem, AdminBuffListItem } from '@/types/admin'
import toast from '@/utils/toast'

const { canManageConfigs } = useAdminPermissions()
const filterSectId = ref('')
const list = ref<AdminHeartSutraListItem[]>([])
const sects = ref<AdminSectTemplateListItem[]>([])
const skills = ref<AdminSkillListItem[]>([])
const buffs = ref<AdminBuffListItem[]>([])
const selected = ref<AdminHeartSutraDetail | null>(null)
const editorOpen = ref(false)

function createEmpty(): AdminHeartSutraDetail {
  return {
    sutraId: '', name: '', description: '', sectId: '', maxLayer: 1,
    layersJson: '[]', sortOrder: 0
  }
}

function openCreateModal() {
  if (!canManageConfigs.value) return
  selected.value = createEmpty()
      editorOpen.value = true
}

async function openEditModal(sutraId: string) {
  selected.value = await getHeartSutra(sutraId)
      editorOpen.value = true
}

async function loadList() {
  list.value = await getHeartSutras(filterSectId.value)
}

async function loadSects() {
  sects.value = await getSectTemplates()
}

async function loadSkillsAndBuffs() {
  const [s, b] = await Promise.all([getAdminSkills(), getAdminBuffs()])
  skills.value = s
  buffs.value = b
}

async function save() {
  if (!canManageConfigs.value || !selected.value) return
  if (!selected.value.sutraId.trim()) { toast.error('心法编号不能为空。'); return }
  if (!selected.value.name.trim()) { toast.error('心法名称不能为空。'); return }

  try {
    selected.value = await saveHeartSutra(selected.value)
    toast.success('心法保存成功。')
    editorOpen.value = false
    await loadList()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '保存失败。')
  }
}

async function remove() {
  if (!canManageConfigs.value || !selected.value?.sutraId) return
  if (!window.confirm(`确认删除心法 ${selected.value.sutraId} 吗？`)) return

  try {
    await deleteHeartSutra(selected.value.sutraId)
    selected.value = null
    toast.success('心法删除成功。')
    editorOpen.value = false
    await loadList()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '删除失败。')
  }
}

watch(filterSectId, () => { loadList() })

onMounted(async () => {
  await Promise.all([loadSects(), loadList(), loadSkillsAndBuffs()])
})
</script>
