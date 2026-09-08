<template>
  <div class="admin-card login-card">
    <p class="section-kicker">管理员入口</p>
    <h2 class="section-title">管理员登录</h2>
    <p class="section-copy">
      当前登录将直连真实后台认证接口。默认开发账号可在后端配置中查看。
    </p>

    <form class="auth-form" @submit.prevent="submit">
      <label class="field">
        <span>账号</span>
        <input v-model.trim="account" type="text" placeholder="admin" />
      </label>

      <label class="field">
        <span>密码</span>
        <input v-model="password" type="password" placeholder="Admin123456" />
      </label>

      <button class="primary-button" type="submit" :disabled="isLoading">
        {{ isLoading ? '登录中...' : '进入后台' }}
      </button>
    </form>

    <div class="login-meta">
      <span class="selected-pill">环境: 开发</span>
      <span class="selected-pill">认证: 后端接口</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import toast from '@/utils/toast'

const router = useRouter()
const authStore = useAuthStore()

const account = ref('admin')
const password = ref('Admin123456')
const isLoading = ref(false)

// 提交管理员登录。
async function submit() {
  if (!account.value || !password.value) {
    toast.error('请输入管理员账号和密码。')
    return
  }

  isLoading.value = true

  try {
    await authStore.login(account.value, password.value)
    router.replace({ name: 'admin-dashboard' })
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '管理员登录失败。')
  } finally {
    isLoading.value = false
  }
}
</script>
