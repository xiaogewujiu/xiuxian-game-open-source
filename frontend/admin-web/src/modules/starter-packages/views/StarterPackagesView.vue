<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">新手礼包</p>
        <h2 class="section-title section-title--small">新手礼包</h2>
        <p class="section-note">维护注册时自动发放的起步物资和初始技能，替代注册链路里的硬编码礼包。</p>
      </div>

      <div class="filter-grid">
        <label class="field">
          <span>关键字</span>
          <input v-model.trim="keyword" type="text" placeholder="搜索礼包编号或名称" />
        </label>
        <label class="field">
          <span>当前礼包</span>
          <input :value="selectedPackage?.packageId || ''" type="text" placeholder="未选择" disabled />
        </label>
        <label class="field">
          <span>自动发放</span>
          <select v-model="autoGrantFilter">
            <option value="">全部</option>
            <option value="yes">是</option>
            <option value="no">否</option>
          </select>
        </label>
        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadPackages">查询</button>
          <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="openCreateModal">新建</button>
        </div>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">礼包列表</p>
          <h3 class="section-title section-title--small">礼包列表</h3>
        </div>
        <span class="selected-pill">共 {{ filteredPackages.length }} 条</span>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>礼包</th>
              <th>版本</th>
              <th>启用</th>
              <th>自动发放</th>
              <th>道具数</th>
              <th>技能数</th>
              <th>排序</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="item in filteredPackages"
              :key="item.packageId"
              :class="{ 'is-active': selectedPackage?.packageId === item.packageId }"
              @click="openEditModal(item.packageId)"
            >
              <td>
                <strong>{{ item.name }}</strong>
                <div class="muted-text">{{ item.packageId }}</div>
              </td>
              <td>{{ item.configVersion || '-' }}</td>
              <td>{{ item.isEnabled ? '启用' : '禁用' }}</td>
              <td>{{ item.autoGrantOnRegister ? '是' : '否' }}</td>
              <td>{{ item.itemGrantCount }}</td>
              <td>{{ item.skillGrantCount }}</td>
              <td>{{ item.sortOrder }}</td>
            </tr>
          </tbody>
        </table>
      </div>

    </section>

    <AdminModal
      v-model="editorOpen"
      kicker="新手礼包"
      :title="selectedPackage?.packageId ? `编辑礼包 ${selectedPackage.name}` : '新建新手礼包'"
      description="维护注册时自动发放的技能和起步物资。"
      size="xwide"
    >
      <fieldset v-if="selectedPackage" class="form-fieldset" :disabled="!canManageConfigs">
        <div class="editor-grid">
          <label class="field"><span>礼包编号</span><input v-model.trim="selectedPackage.packageId" type="text" /></label>
          <label class="field"><span>礼包名称</span><input v-model.trim="selectedPackage.name" type="text" /></label>
          <label class="field"><span>配置版本</span><input v-model.trim="selectedPackage.configVersion" type="text" /></label>
          <label class="field"><span>排序</span><input v-model.number="selectedPackage.sortOrder" type="number" min="0" /></label>
        </div>

        <label class="field"><span>说明</span><textarea v-model="selectedPackage.description" rows="3"></textarea></label>
        <label class="field"><span>备注</span><textarea v-model="selectedPackage.remark" rows="2"></textarea></label>

        <div class="editor-grid">
          <label class="field checkbox-field"><input v-model="selectedPackage.isEnabled" type="checkbox" /><span>启用礼包</span></label>
          <label class="field checkbox-field"><input v-model="selectedPackage.autoGrantOnRegister" type="checkbox" /><span>注册自动发放</span></label>
          <label class="field checkbox-field"><input v-model="selectedPackage.isBuiltIn" type="checkbox" /><span>系统内置</span></label>
        </div>

        <RewardGrantItemsEditor v-model="rewardDraft" title="道具发放" :item-options="itemOptions" />

        <div class="array-editor">
          <div class="array-editor__header">
            <span>技能发放</span>
            <button class="secondary-button" type="button" @click="addSkillGrant">新增技能</button>
          </div>

          <div v-if="skillDraft.length === 0" class="empty-tip">当前没有技能发放配置。</div>

          <div v-else class="array-editor__list">
            <div v-for="(grant, index) in skillDraft" :key="`${grant.skillId}-${index}`" class="array-editor__row array-editor__row--three">
              <label class="field">
                <span>技能</span>
                <select v-model.number="grant.skillId">
                  <option :value="0">请选择</option>
                  <option v-for="option in skillOptions" :key="option.value" :value="option.value">{{ option.label }}</option>
                </select>
              </label>
              <label class="field">
                <span>排序</span>
                <input v-model.number="grant.sortOrder" type="number" min="0" />
              </label>
              <div class="field field--full field--actions-inline">
                <span>操作</span>
                <button class="danger-button danger-button--small" type="button" @click="removeSkillGrant(index)">删除</button>
              </div>
            </div>
          </div>
        </div>
      </fieldset>

      <template #footer>
        <button class="secondary-button" type="button" @click="editorOpen = false">取消</button>
        <button class="danger-button" type="button" :disabled="!canManageConfigs || !selectedPackage?.packageId" @click="removePackage">删除</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="savePackage">保存礼包</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import RewardGrantItemsEditor from '@/components/editors/RewardGrantItemsEditor.vue'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import { getAdminItems } from '@/services/items'
import { getAdminSkills } from '@/services/skills-buffs'
import { deleteAdminStarterPackage, getAdminStarterPackageDetail, getAdminStarterPackages, saveAdminStarterPackage } from '@/services/starter-packages'
import type { AdminItemListItem, AdminSkillListItem, AdminStarterPackageDetail, AdminStarterPackageItemGrant, AdminStarterPackageListItem, AdminStarterPackageSkillGrant, RewardGrantEntry } from '@/types/admin'
import toast from '@/utils/toast'

// 新手礼包页维护的是“注册时自动发放的礼包模板”，不是玩家已领取记录。
const { canManageConfigs } = useAdminPermissions()
const keyword = ref('')
const autoGrantFilter = ref('')
const packages = ref<AdminStarterPackageListItem[]>([])
const items = ref<AdminItemListItem[]>([])
const skills = ref<AdminSkillListItem[]>([])
const selectedPackage = ref<AdminStarterPackageDetail | null>(null)
const rewardDraft = ref<RewardGrantEntry[]>([])
const skillDraft = ref<AdminStarterPackageSkillGrant[]>([])
const editorOpen = ref(false)

// 后端已支持按自动发放筛选。
const filteredPackages = packages

// 道具奖励编辑器的下拉选项。
const itemOptions = computed(() => items.value.map((item) => ({ value: item.itemId, label: `${item.name} (${item.itemId})` })))
// 技能奖励下拉选项。
const skillOptions = computed(() => skills.value.map((skill) => ({ value: skill.skillId, label: `${skill.name} (${skill.skillId})` })))

// 把礼包物品奖励转成通用奖励编辑器草稿。
function mapItemGrantToRewardEntry(item: AdminStarterPackageItemGrant): RewardGrantEntry {
  return { type: 'item', count: item.quantity, itemId: item.itemId, description: null }
}

// 把通用奖励草稿重新转回礼包物品奖励结构。
function mapRewardEntryToItemGrant(entry: RewardGrantEntry, index: number): AdminStarterPackageItemGrant {
  return {
    itemId: entry.itemId?.trim() || '',
    quantity: Math.max(1, Number(entry.count) || 1),
    sortOrder: index + 1
  }
}

// 根据当前礼包详情同步物品奖励和技能奖励草稿。
function syncDrafts(detail: AdminStarterPackageDetail | null) {
  rewardDraft.value = (detail?.itemGrants ?? []).map(mapItemGrantToRewardEntry)
  skillDraft.value = (detail?.skillGrants ?? []).map((item) => ({ ...item }))
}

// 把当前草稿回写到 selectedPackage。
function applyDrafts() {
  if (!selectedPackage.value) return
  selectedPackage.value.itemGrants = rewardDraft.value
    .filter((item) => item.type === 'item' && (item.itemId?.trim() || ''))
    .map(mapRewardEntryToItemGrant)
  selectedPackage.value.skillGrants = skillDraft.value
    .filter((item) => Number(item.skillId) > 0)
    .map((item, index) => ({ ...item, sortOrder: item.sortOrder > 0 ? item.sortOrder : index + 1 }))
}

// 新建礼包时的默认对象。
function createEmptyPackage(): AdminStarterPackageDetail {
  return {
    packageId: '',
    name: '',
    description: '',
    isEnabled: true,
    autoGrantOnRegister: false,
    sortOrder: 0,
    configVersion: 'manual-admin',
    seedKey: null,
    isBuiltIn: false,
    builtInVersion: null,
    remark: '',
    createdBy: null,
    updatedBy: null,
    createTime: null,
    lastUpdateTime: null,
    itemGrants: [],
    skillGrants: []
  }
}

// 预加载道具与技能选项。
async function loadOptions() {
  const [itemList, skillList] = await Promise.all([getAdminItems(), getAdminSkills()])
  items.value = itemList
  skills.value = skillList
}

async function loadPackages() {
  const filterAutoGrant = autoGrantFilter.value === '' ? null : autoGrantFilter.value === 'yes'
  packages.value = await getAdminStarterPackages(keyword.value, filterAutoGrant)
}

function openCreateModal() {
  if (!canManageConfigs.value) return
  selectedPackage.value = createEmptyPackage()
  syncDrafts(selectedPackage.value)
  editorOpen.value = true
}

async function openEditModal(packageId: string) {
  selectedPackage.value = await getAdminStarterPackageDetail(packageId)
  syncDrafts(selectedPackage.value)
  editorOpen.value = true
}

function addSkillGrant() {
  skillDraft.value.push({ skillId: 0, sortOrder: skillDraft.value.length + 1 })
}

function removeSkillGrant(index: number) {
  skillDraft.value.splice(index, 1)
}

async function savePackage() {
  if (!canManageConfigs.value || !selectedPackage.value) return
  applyDrafts()
  try {
    selectedPackage.value = await saveAdminStarterPackage(selectedPackage.value)
    syncDrafts(selectedPackage.value)
    toast.success('新手礼包保存成功。')
    editorOpen.value = false
    await loadPackages()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '新手礼包保存失败。')
  }
}

async function removePackage() {
  if (!canManageConfigs.value || !selectedPackage.value?.packageId) return
  if (!window.confirm(`确认删除新手礼包 ${selectedPackage.value.name} 吗？`)) return
  try {
    await deleteAdminStarterPackage(selectedPackage.value.packageId)
    selectedPackage.value = null
    syncDrafts(null)
    editorOpen.value = false
    toast.success('新手礼包删除成功。')
    await loadPackages()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '新手礼包删除失败。')
  }
}

onMounted(async () => {
  await Promise.all([loadOptions(), loadPackages()])
})
</script>
