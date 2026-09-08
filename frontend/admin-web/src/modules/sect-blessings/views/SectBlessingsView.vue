<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">宗门福利</p>
        <h2 class="section-title section-title--small">宗门福利配置</h2>
        <p class="section-note">配置宗门等级解锁的被动Buff。点击列表行打开编辑弹窗。</p>
      </div>

      <div class="filter-grid">
        <label class="field">
          <span>当前福利</span>
          <input :value="selected?.blessingId || ''" type="text" placeholder="未选择" disabled />
        </label>

        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadList">刷新</button>
          <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="openCreateModal">新建福利</button>
        </div>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">福利列表</p>
          <h3 class="section-title section-title--small">宗门福利列表</h3>
        </div>
        <span class="selected-pill">共 {{ list.length }} 条</span>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>编号</th>
              <th>名称</th>
              <th>所需宗门等级</th>
              <th>Buff编号</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="item in list"
              :key="item.blessingId"
              :class="{ 'is-active': selected?.blessingId === item.blessingId }"
              @click="openEditModal(item.blessingId)"
            >
              <td>{{ item.blessingId }}</td>
              <td>{{ item.name }}</td>
              <td>{{ item.requiredGuildLevel }}</td>
              <td>{{ item.buffId }}</td>
            </tr>
          </tbody>
        </table>
      </div>

    </section>

    <AdminModal
      v-model="editorOpen"
      kicker="宗门福利编辑"
      :title="selected?.blessingId ? `编辑福利 ${selected.blessingId}` : '新建宗门福利'"
      description="配置福利名称、所需宗门等级和关联Buff。"
      size="xwide"
    >
      <fieldset v-if="selected" class="form-fieldset" :disabled="!canManageConfigs">
        <div class="editor-grid editor-grid--three">
          <label class="field"><span>福利编号</span><input v-model.trim="selected.blessingId" type="text" /></label>
          <label class="field"><span>福利名称</span><input v-model.trim="selected.name" type="text" /></label>
          <label class="field"><span>所需宗门等级</span><input v-model.number="selected.requiredGuildLevel" type="number" min="1" /></label>
          <label class="field"><span>关联Buff</span>
            <select v-model="selected.buffId">
              <option value="">请选择Buff</option>
              <option v-for="b in buffs" :key="b.buffId" :value="b.buffId">{{ b.name }} ({{ b.buffId }})</option>
            </select>
          </label>
          <label class="field"><span>排序值</span><input v-model.number="selected.sortOrder" type="number" /></label>
        </div>
        <label class="field"><span>福利描述</span><textarea v-model="selected.description" rows="3"></textarea></label>
      </fieldset>

      <template #footer>
        <button class="secondary-button" type="button" @click="editorOpen = false">取消</button>
        <button class="danger-button" type="button" :disabled="!canManageConfigs || !selected?.blessingId" @click="remove">删除</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="save">保存福利</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import { getSectBlessings, getSectBlessing, saveSectBlessing, deleteSectBlessing } from '@/services/sect'
import { getAdminBuffs } from '@/services/skills-buffs'
import type { AdminSectBlessingDetail, AdminSectBlessingListItem, AdminBuffListItem } from '@/types/admin'
import toast from '@/utils/toast'

const { canManageConfigs } = useAdminPermissions()
const buffs = ref<AdminBuffListItem[]>([])
const list = ref<AdminSectBlessingListItem[]>([])
const selected = ref<AdminSectBlessingDetail | null>(null)
const editorOpen = ref(false)

function createEmpty(): AdminSectBlessingDetail {
  return {
    blessingId: '', name: '', description: '', requiredGuildLevel: 1,
    buffId: '', sortOrder: 0
  }
}

function openCreateModal() {
  if (!canManageConfigs.value) return
  selected.value = createEmpty()
  editorOpen.value = true
}

async function openEditModal(blessingId: string) {
  selected.value = await getSectBlessing(blessingId)
  editorOpen.value = true
}

async function loadList() {
  list.value = await getSectBlessings()
}

async function save() {
  if (!canManageConfigs.value || !selected.value) return
  if (!selected.value.blessingId.trim()) { toast.error('福利编号不能为空。'); return }
  if (!selected.value.name.trim()) { toast.error('福利名称不能为空。'); return }

  try {
    selected.value = await saveSectBlessing(selected.value)
    toast.success('宗门福利保存成功。')
    editorOpen.value = false
    await loadList()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '保存失败。')
  }
}

async function remove() {
  if (!canManageConfigs.value || !selected.value?.blessingId) return
  if (!window.confirm(`确认删除福利 ${selected.value.blessingId} 吗？`)) return

  try {
    await deleteSectBlessing(selected.value.blessingId)
    selected.value = null
    toast.success('宗门福利删除成功。')
    editorOpen.value = false
    await loadList()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '删除失败。')
  }
}

onMounted(async () => {
  await Promise.all([loadList(), (async () => { buffs.value = await getAdminBuffs() })()])
})
</script>
