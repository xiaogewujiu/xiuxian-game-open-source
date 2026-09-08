<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">宗门</p>
        <h2 class="section-title section-title--small">宗门模板配置</h2>
        <p class="section-note">维护宗门预设、介绍与心法关联。点击列表行打开编辑弹窗。</p>
      </div>

      <div class="filter-grid">
        <label class="field">
          <span>关键字</span>
          <input v-model.trim="keyword" type="text" placeholder="搜索宗门编号或名称" />
        </label>
        <label class="field">
          <span>当前宗门</span>
          <input :value="selected?.sectId || ''" type="text" placeholder="未选择" disabled />
        </label>

        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadList">查询</button>
          <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="openCreateModal">新建宗门</button>
        </div>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">宗门列表</p>
          <h3 class="section-title section-title--small">宗门列表</h3>
        </div>
        <span class="selected-pill">共 {{ filteredList.length }} 条</span>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>编号</th>
              <th>名称</th>
              <th>排序</th>
              <th>人数</th>
              <th>状态</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="item in filteredList"
              :key="item.sectId"
              :class="{ 'is-active': selected?.sectId === item.sectId }"
              @click="openEditModal(item.sectId)"
            >
              <td>{{ item.sectId }}</td>
              <td>{{ item.name }}</td>
              <td>{{ item.sortOrder }}</td>
              <td>{{ item.memberCount }}</td>
              <td>{{ item.isEnabled ? '启用' : '停用' }}</td>
            </tr>
          </tbody>
        </table>
      </div>

    </section>

    <AdminModal
      v-model="editorOpen"
      kicker="宗门模板"
      :title="selected?.sectId ? `编辑宗门 ${selected.sectId}` : '新建宗门'"
      description="维护宗门名称、描述、图标、心法关联和排序。"
      size="xwide"
    >
      <fieldset v-if="selected" class="form-fieldset" :disabled="!canManageConfigs">
        <div class="editor-grid editor-grid--three">
          <label class="field"><span>宗门编号</span><input v-model.trim="selected.sectId" type="text" /></label>
          <label class="field"><span>宗门名称</span><input v-model.trim="selected.name" type="text" /></label>
          <label class="field"><span>排序值</span><input v-model.number="selected.sortOrder" type="number" /></label>
        </div>
        <label class="field checkbox-field"><input v-model="selected.isEnabled" type="checkbox" /><span>启用宗门</span></label>
        <label class="field"><span>宗门描述</span><textarea v-model="selected.description" rows="4"></textarea></label>

        <AdminImageField
          v-model="selected.icon"
          label="宗门图标"
          upload-category="sect"
          placeholder="输入图标路径或上传图片"
        />

        <AdminImageField
          v-model="selected.portraitPath"
          label="宗门立绘"
          upload-category="sect"
          placeholder="输入立绘路径或上传图片"
        />

        <StringListEditor
          v-model="heartSutraIds"
          title="关联心法编号列表"
          :options="heartSutraOptions"
        />
      </fieldset>

      <template #footer>
        <button class="secondary-button" type="button" @click="editorOpen = false">取消</button>
        <button class="danger-button" type="button" :disabled="!canManageConfigs || !selected?.sectId" @click="remove">删除宗门</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="save">保存宗门</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import AdminImageField from '@/components/AdminImageField.vue'
import StringListEditor from '@/components/editors/StringListEditor.vue'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import { getSectTemplates, getSectTemplate, saveSectTemplate, deleteSectTemplate, getHeartSutras } from '@/services/sect'
import type { AdminSectTemplateDetail, AdminSectTemplateListItem, AdminHeartSutraListItem } from '@/types/admin'
import toast from '@/utils/toast'

const { canManageConfigs } = useAdminPermissions()
const keyword = ref('')
const list = ref<AdminSectTemplateListItem[]>([])
const selected = ref<AdminSectTemplateDetail | null>(null)
const editorOpen = ref(false)

const heartSutraIds = ref<string[]>([])
const allHeartSutras = ref<AdminHeartSutraListItem[]>([])

const heartSutraOptions = computed(() =>
  allHeartSutras.value.map((s) => ({ value: s.sutraId, label: `${s.name} (${s.sutraId})` }))
)

const filteredList = computed(() => {
  if (!keyword.value) return list.value
  const kw = keyword.value.toLowerCase()
  return list.value.filter((item) =>
    item.sectId.toLowerCase().includes(kw) || item.name.toLowerCase().includes(kw)
  )
})

// 同步 heartSutraIds 到 selected.heartSutraIdsJson
watch(heartSutraIds, (ids) => {
  if (selected.value) {
    selected.value.heartSutraIdsJson = JSON.stringify(ids)
  }
}, { deep: true })

function parseJsonArray(json: string): string[] {
  try {
    const arr = JSON.parse(json)
    return Array.isArray(arr) ? arr.map(String) : []
  } catch {
    return []
  }
}

function createEmpty(): AdminSectTemplateDetail {
  return {
    sectId: '', name: '', description: '', icon: '', portraitPath: '',
    heartSutraIdsJson: '[]', isEnabled: true, sortOrder: 0
  }
}

function openCreateModal() {
  if (!canManageConfigs.value) return
  selected.value = createEmpty()
  heartSutraIds.value = []
  editorOpen.value = true
}

async function openEditModal(sectId: string) {
  selected.value = await getSectTemplate(sectId)
  heartSutraIds.value = parseJsonArray(selected.value.heartSutraIdsJson)
  editorOpen.value = true
}

async function loadList() {
  list.value = await getSectTemplates()
}

async function save() {
  if (!canManageConfigs.value || !selected.value) return
  if (!selected.value.sectId.trim()) { toast.error('宗门编号不能为空。'); return }
  if (!selected.value.name.trim()) { toast.error('宗门名称不能为空。'); return }

  // 确保 heartSutraIdsJson 已同步
  selected.value.heartSutraIdsJson = JSON.stringify(heartSutraIds.value)

  try {
    selected.value = await saveSectTemplate(selected.value)
    toast.success('宗门模板保存成功。')
    editorOpen.value = false
    await loadList()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '保存失败。')
  }
}

async function remove() {
  if (!canManageConfigs.value || !selected.value?.sectId) return
  if (!window.confirm(`确认删除宗门 ${selected.value.sectId} 吗？`)) return

  try {
    await deleteSectTemplate(selected.value.sectId)
    selected.value = null
    toast.success('宗门模板删除成功。')
    editorOpen.value = false
    await loadList()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '删除失败。')
  }
}

onMounted(async () => {
  await Promise.all([loadList(), (async () => { allHeartSutras.value = await getHeartSutras() })()])
})
</script>
