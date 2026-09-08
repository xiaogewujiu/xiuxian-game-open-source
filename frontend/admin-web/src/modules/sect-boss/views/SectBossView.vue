<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">宗门Boss</p>
        <h2 class="section-title section-title--small">宗门Boss模板</h2>
        <p class="section-note">配置宗门Boss模板与奖励。点击列表行打开编辑弹窗。</p>
      </div>

      <div class="filter-grid">
        <label class="field">
          <span>关键字</span>
          <input v-model.trim="keyword" type="text" placeholder="搜索Boss编号或名称" />
        </label>
        <label class="field">
          <span>当前Boss</span>
          <input :value="selected?.bossId || ''" type="text" placeholder="未选择" disabled />
        </label>

        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadList">查询</button>
          <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="openCreateModal">新建Boss</button>
        </div>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">Boss列表</p>
          <h3 class="section-title section-title--small">Boss模板列表</h3>
        </div>
        <span class="selected-pill">共 {{ filteredList.length }} 条</span>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>编号</th>
              <th>名称</th>
              <th>怪物模板</th>
              <th>状态</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="item in filteredList"
              :key="item.bossId"
              :class="{ 'is-active': selected?.bossId === item.bossId }"
              @click="openEditModal(item.bossId)"
            >
              <td>{{ item.bossId }}</td>
              <td>{{ item.name }}</td>
              <td>{{ item.monsterTemplateId }}</td>
              <td>{{ item.isEnabled ? '启用' : '停用' }}</td>
            </tr>
          </tbody>
        </table>
      </div>

    </section>

    <AdminModal
      v-model="editorOpen"
      kicker="宗门Boss编辑"
      :title="selected?.bossId ? `编辑Boss ${selected.bossId}` : '新建宗门Boss'"
      description="配置宗门Boss怪物绑定、时长和排行奖励。"
      size="xwide"
    >
      <fieldset v-if="selected" class="form-fieldset" :disabled="!canManageConfigs">
        <div class="editor-grid editor-grid--three">
          <label class="field"><span>Boss编号</span><input v-model.trim="selected.bossId" type="text" /></label>
          <label class="field"><span>Boss名称</span><input v-model.trim="selected.name" type="text" /></label>
          <label class="field"><span>怪物模板</span>
            <select v-model="selected.monsterTemplateId">
              <option value="">请选择怪物模板</option>
              <option v-for="m in monsters" :key="m.monsterId" :value="m.monsterId">{{ m.name }} ({{ m.monsterId }})</option>
            </select>
          </label>
          <label class="field"><span>持续分钟</span><input v-model.number="selected.durationMinutes" type="number" min="1" /></label>
          <label class="field"><span>排序值</span><input v-model.number="selected.sortOrder" type="number" /></label>
          <label class="field checkbox-field"><input v-model="selected.isEnabled" type="checkbox" /><span>启用Boss</span></label>
        </div>

        <AdminImageField
          v-model="selected.portraitPath"
          label="Boss画像"
          upload-category="sect-boss"
          placeholder="输入画像路径或上传图片"
        />

        <div class="section-split">
          <h4 class="subheading">排行奖励贡献</h4>
          <div class="editor-grid editor-grid--three">
            <label class="field"><span>参与奖励贡献</span><input v-model.number="selected.participationRewardContribution" type="number" min="0" /></label>
            <label class="field"><span>第1名贡献</span><input v-model.number="selected.rank1RewardContribution" type="number" min="0" /></label>
            <label class="field"><span>第2名贡献</span><input v-model.number="selected.rank2RewardContribution" type="number" min="0" /></label>
            <label class="field"><span>第3名贡献</span><input v-model.number="selected.rank3RewardContribution" type="number" min="0" /></label>
          </div>
        </div>
      </fieldset>

      <template #footer>
        <button class="secondary-button" type="button" @click="editorOpen = false">取消</button>
        <button class="danger-button" type="button" :disabled="!canManageConfigs || !selected?.bossId" @click="remove">删除</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="save">保存Boss</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import AdminImageField from '@/components/AdminImageField.vue'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import { getSectBossTemplates, getSectBossTemplate, saveSectBossTemplate, deleteSectBossTemplate } from '@/services/sect'
import { getAdminMonsters } from '@/services/monsters'
import type { AdminSectBossTemplateDetail, AdminSectBossTemplateListItem, AdminMonsterListItem } from '@/types/admin'
import toast from '@/utils/toast'

const { canManageConfigs } = useAdminPermissions()
const keyword = ref('')
const monsters = ref<AdminMonsterListItem[]>([])
const list = ref<AdminSectBossTemplateListItem[]>([])
const selected = ref<AdminSectBossTemplateDetail | null>(null)
const editorOpen = ref(false)

const filteredList = computed(() => {
  if (!keyword.value) return list.value
  const kw = keyword.value.toLowerCase()
  return list.value.filter((item) =>
    item.bossId.toLowerCase().includes(kw) || item.name.toLowerCase().includes(kw)
  )
})

function createEmpty(): AdminSectBossTemplateDetail {
  return {
    bossId: '', name: '', monsterTemplateId: '', portraitPath: '',
    isEnabled: true, durationMinutes: 60,
    participationRewardContribution: 0,
    rank1RewardContribution: 0,
    rank2RewardContribution: 0,
    rank3RewardContribution: 0,
    sortOrder: 0
  }
}

function openCreateModal() {
  if (!canManageConfigs.value) return
  selected.value = createEmpty()
  editorOpen.value = true
}

async function openEditModal(bossId: string) {
  selected.value = await getSectBossTemplate(bossId)
  editorOpen.value = true
}

async function loadList() {
  list.value = await getSectBossTemplates()
}

async function save() {
  if (!canManageConfigs.value || !selected.value) return
  if (!selected.value.bossId.trim()) { toast.error('Boss编号不能为空。'); return }
  if (!selected.value.name.trim()) { toast.error('Boss名称不能为空。'); return }

  try {
    selected.value = await saveSectBossTemplate(selected.value)
    toast.success('宗门Boss模板保存成功。')
    editorOpen.value = false
    await loadList()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '保存失败。')
  }
}

async function remove() {
  if (!canManageConfigs.value || !selected.value?.bossId) return
  if (!window.confirm(`确认删除Boss ${selected.value.bossId} 吗？`)) return

  try {
    await deleteSectBossTemplate(selected.value.bossId)
    selected.value = null
    toast.success('宗门Boss模板删除成功。')
    editorOpen.value = false
    await loadList()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '删除失败。')
  }
}

onMounted(async () => {
  await Promise.all([loadList(), (async () => { monsters.value = await getAdminMonsters() })()])
})
</script>

<style scoped>
.subheading {
  margin: 0 0 12px;
  font-size: 14px;
  color: var(--text-secondary);
}

.section-split {
  display: grid;
  grid-template-columns: 1fr;
  gap: 12px;
}
</style>
