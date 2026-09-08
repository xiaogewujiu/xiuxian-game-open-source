<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">后台用户</p>
        <h2 class="section-title section-title--small">后台用户管理</h2>
        <p class="section-note">后台账号统一使用表格列表管理，新增、编辑和重置密码通过弹窗完成。</p>
      </div>

      <div class="filter-grid">
        <label class="field">
          <span>关键字</span>
          <input v-model.trim="keyword" type="text" placeholder="搜索管理员账号、显示名或角色" />
        </label>
        <label class="field">
          <span>当前账号</span>
          <input :value="selectedUser?.account || ''" type="text" placeholder="未选择" disabled />
        </label>
        <label class="field">
          <span>状态</span>
          <select v-model="statusFilter">
            <option value="">全部</option>
            <option value="active">启用</option>
            <option value="inactive">停用</option>
          </select>
        </label>

        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadUsers">查询</button>
          <button class="primary-button" type="button" :disabled="!canManageAdminUsers" @click="openCreateModal">新建管理员</button>
        </div>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">管理员列表</p>
          <h3 class="section-title section-title--small">管理员列表</h3>
        </div>
        <span class="selected-pill">共 {{ filteredUsers.length }} 条</span>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>账号</th>
              <th>显示名</th>
              <th>角色</th>
              <th>状态</th>
              <th>操作</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="user in filteredUsers"
              :key="user.adminId"
              :class="{ 'is-active': selectedUser?.adminId === user.adminId }"
              @click="openEditModal(user.adminId)"
            >
              <td>{{ user.account }}</td>
              <td>{{ user.displayName }}</td>
              <td>{{ user.role }}</td>
              <td>{{ user.isActive ? '启用' : '停用' }}</td>
              <td>
                <button class="secondary-button" type="button" :disabled="!canManageAdminUsers" @click.stop="openPasswordModal(user.adminId)">重置密码</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

    </section>

    <AdminModal
      v-model="editorOpen"
      kicker="管理员"
      :title="selectedUser?.adminId ? `编辑管理员 ${selectedUser.account}` : '新建管理员'"
      description="维护管理员账号、角色和启停状态。"
      size="wide"
    >
      <div class="section-stack">
        <div v-if="!selectedUser?.adminId" class="editor-grid editor-grid--three">
          <label class="field"><span>账号</span><input v-model.trim="createForm.account" type="text" /></label>
          <label class="field"><span>显示名</span><input v-model.trim="createForm.displayName" type="text" /></label>
          <label class="field"><span>角色</span><select v-model="createForm.role"><option v-for="role in ADMIN_ROLE_OPTIONS" :key="role.value" :value="role.value">{{ role.label }}</option></select></label>
          <label class="field"><span>初始密码</span><input v-model="createForm.password" type="password" /></label>
        </div>

        <div v-else-if="selectedUser" class="section-stack">
          <div class="detail-overview">
            <article class="detail-overview__item"><span>账号</span><strong>{{ selectedUser.account }}</strong></article>
            <article class="detail-overview__item"><span>角色</span><strong>{{ selectedUser.role }}</strong></article>
            <article class="detail-overview__item"><span>最后登录</span><strong>{{ selectedUser.lastLoginTime || '暂无' }}</strong></article>
            <article class="detail-overview__item"><span>状态</span><strong>{{ selectedUser.isActive ? '启用' : '停用' }}</strong></article>
          </div>

          <div class="editor-grid editor-grid--three">
            <label class="field"><span>编号</span><input v-model="selectedUser.adminId" type="text" disabled /></label>
            <label class="field"><span>账号</span><input v-model="selectedUser.account" type="text" disabled /></label>
            <label class="field"><span>显示名</span><input v-model.trim="selectedUser.displayName" type="text" /></label>
            <label class="field"><span>角色</span><select v-model="selectedUser.role"><option v-for="role in ADMIN_ROLE_OPTIONS" :key="role.value" :value="role.value">{{ role.label }}</option></select></label>
            <label class="field"><span>最后登录</span><input :value="selectedUser.lastLoginTime || ''" type="text" disabled /></label>
          </div>
          <label class="field checkbox-field"><input v-model="selectedUser.isActive" type="checkbox" /><span>启用管理员</span></label>
        </div>
      </div>

      <template #footer>
        <button class="secondary-button" type="button" @click="editorOpen = false">取消</button>
        <button
          v-if="!selectedUser?.adminId"
          class="primary-button"
          type="button"
          :disabled="!canManageAdminUsers"
          @click="createUser"
        >
          创建管理员
        </button>
        <button
          v-else
          class="primary-button"
          type="button"
          :disabled="!canManageAdminUsers"
          @click="saveUser"
        >
          保存管理员
        </button>
      </template>
    </AdminModal>

    <AdminModal
      v-model="passwordModalOpen"
      kicker="安全"
      title="重置管理员密码"
      description="用于账号交接、权限恢复或安全事件处理。"
    >
      <div class="editor-grid">
        <label class="field"><span>新密码</span><input v-model="passwordForm.newPassword" type="password" /></label>
      </div>

      <template #footer>
        <button class="secondary-button" type="button" @click="passwordModalOpen = false">取消</button>
        <button class="primary-button" type="button" :disabled="!canManageAdminUsers" @click="resetPassword">重置密码</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import { ADMIN_ROLE_OPTIONS } from '@/constants/admin'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import type { AdminCreateUserRequest, AdminResetPasswordRequest, AdminUpdateUserRequest, AdminUserDetail, AdminUserListItem } from '@/types/admin'
import { createAdminUser, getAdminUserDetail, getAdminUsers, resetAdminUserPassword, updateAdminUser } from '@/services/admin-users'
import toast from '@/utils/toast'

// 管理员页分成两套编辑上下文：
// createForm 负责新建管理员，selectedUser 负责编辑已有管理员。
const { canManageAdminUsers } = useAdminPermissions()
const keyword = ref('')
const statusFilter = ref('')
const users = ref<AdminUserListItem[]>([])
const selectedUser = ref<AdminUserDetail | null>(null)
const editorOpen = ref(false)
const passwordModalOpen = ref(false)

const createForm = reactive<AdminCreateUserRequest>({
  account: '',
  displayName: '',
  role: 'admin',
  password: ''
})

const passwordForm = reactive<AdminResetPasswordRequest>({
  newPassword: ''
})

// 后端已支持按状态筛选。
const filteredUsers = users

// 重置新建管理员表单。
function resetCreateForm() {
  createForm.account = ''
  createForm.displayName = ''
  createForm.role = 'admin'
  createForm.password = ''
}

// 打开新建管理员弹窗。
function openCreateModal() {
  if (!canManageAdminUsers.value) return
  selectedUser.value = null
  resetCreateForm()
  editorOpen.value = true
}

// 拉取管理员列表。
async function loadUsers() {
  const filterActive = statusFilter.value === '' ? null : statusFilter.value === 'active'
  users.value = await getAdminUsers(keyword.value, filterActive)
}

// 打开管理员详情弹窗。
async function openEditModal(adminId: string) {
  selectedUser.value = await getAdminUserDetail(adminId)
  editorOpen.value = true
}

// 打开密码重置弹窗。
async function openPasswordModal(adminId: string) {
  selectedUser.value = await getAdminUserDetail(adminId)
  passwordForm.newPassword = ''
  passwordModalOpen.value = true
}

// 创建管理员。
async function createUser() {
  if (!canManageAdminUsers.value) return

  try {
    const user = await createAdminUser(createForm)
    selectedUser.value = user
    resetCreateForm()
    toast.success('管理员创建成功。')
    editorOpen.value = false
    await loadUsers()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '管理员创建失败。')
  }
}

// 保存管理员基本资料。
async function saveUser() {
  if (!canManageAdminUsers.value || !selectedUser.value) return

  try {
    const payload: AdminUpdateUserRequest = {
      displayName: selectedUser.value.displayName,
      role: selectedUser.value.role,
      isActive: selectedUser.value.isActive
    }
    selectedUser.value = await updateAdminUser(selectedUser.value.adminId, payload)
    toast.success('管理员更新成功。')
    editorOpen.value = false
    await loadUsers()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '管理员更新失败。')
  }
}

// 重置管理员密码。
async function resetPassword() {
  if (!canManageAdminUsers.value || !selectedUser.value) return

  try {
    await resetAdminUserPassword(selectedUser.value.adminId, passwordForm)
    passwordForm.newPassword = ''
    toast.success('密码重置成功。')
    passwordModalOpen.value = false
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '密码重置失败。')
  }
}

onMounted(loadUsers)
</script>
