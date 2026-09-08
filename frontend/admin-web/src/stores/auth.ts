import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import type { AdminCurrentUser } from '@/types/admin'
import { adminLogin, adminLogout, adminMe } from '@/services/auth'

const ACCESS_TOKEN_KEY = 'admin-access-token'
const REFRESH_TOKEN_KEY = 'admin-refresh-token'
const DISPLAY_NAME_KEY = 'admin-display-name'
const ROLE_KEY = 'admin-role'
const PERMISSIONS_KEY = 'admin-permissions'
const ACCOUNT_KEY = 'admin-account'
const ADMIN_ID_KEY = 'admin-id'

export const useAuthStore = defineStore('admin-auth', () => {
  // 管理员登录态的最小持久化集合。
  // 页面刷新后会从 localStorage 直接恢复。
  const accessToken = ref(localStorage.getItem(ACCESS_TOKEN_KEY) || '')
  const refreshToken = ref(localStorage.getItem(REFRESH_TOKEN_KEY) || '')
  const currentUser = ref<AdminCurrentUser | null>(
    localStorage.getItem(ADMIN_ID_KEY)
      ? {
          adminId: localStorage.getItem(ADMIN_ID_KEY) || '',
          account: localStorage.getItem(ACCOUNT_KEY) || '',
          displayName: localStorage.getItem(DISPLAY_NAME_KEY) || '',
          role: localStorage.getItem(ROLE_KEY) || '',
          permissions: JSON.parse(localStorage.getItem(PERMISSIONS_KEY) || '[]')
        }
      : null
  )

  const isAuthenticated = computed(() => Boolean(accessToken.value))
  const displayName = computed(() => currentUser.value?.displayName || '')
  const role = computed(() => currentUser.value?.role || '')
  const permissions = computed(() => currentUser.value?.permissions || [])

  // 持久化当前管理员基础信息。
  function persistCurrentUser(user: AdminCurrentUser) {
    currentUser.value = user
    localStorage.setItem(ADMIN_ID_KEY, user.adminId)
    localStorage.setItem(ACCOUNT_KEY, user.account)
    localStorage.setItem(DISPLAY_NAME_KEY, user.displayName)
    localStorage.setItem(ROLE_KEY, user.role)
    localStorage.setItem(PERMISSIONS_KEY, JSON.stringify(user.permissions || []))
  }

  // 持久化访问令牌和刷新令牌。
  function setTokenPair(nextAccessToken: string, nextRefreshToken: string) {
    accessToken.value = nextAccessToken
    refreshToken.value = nextRefreshToken
    localStorage.setItem(ACCESS_TOKEN_KEY, nextAccessToken)
    localStorage.setItem(REFRESH_TOKEN_KEY, nextRefreshToken)
  }

  // 响应 http.ts 发出的会话变更事件，确保 store 与 localStorage 保持一致。
  function syncSessionFromStorage() {
    accessToken.value = localStorage.getItem(ACCESS_TOKEN_KEY) || ''
    refreshToken.value = localStorage.getItem(REFRESH_TOKEN_KEY) || ''

    currentUser.value = localStorage.getItem(ADMIN_ID_KEY)
      ? {
          adminId: localStorage.getItem(ADMIN_ID_KEY) || '',
          account: localStorage.getItem(ACCOUNT_KEY) || '',
          displayName: localStorage.getItem(DISPLAY_NAME_KEY) || '',
          role: localStorage.getItem(ROLE_KEY) || '',
          permissions: JSON.parse(localStorage.getItem(PERMISSIONS_KEY) || '[]')
        }
      : null
  }

  if (typeof window !== 'undefined') {
    window.addEventListener('admin-session-changed', syncSessionFromStorage)
  }

  // 登录：拿令牌，再写入当前管理员信息。
  async function login(account: string, password: string) {
    const response = await adminLogin(account, password)
    setTokenPair(response.accessToken, response.refreshToken)
    persistCurrentUser(response.currentUser)
    return response
  }

  // 使用 /me 接口重新拉取当前管理员资料。
  async function loadCurrentUser() {
    if (!accessToken.value) {
      return null
    }

    const user = await adminMe()
    persistCurrentUser(user)
    return user
  }

  // 登出时优先通知后端撤销刷新令牌；
  // 即便接口失败，也要保证本地会话被清空。
  async function logout() {
    const latestRefreshToken = localStorage.getItem(REFRESH_TOKEN_KEY) || refreshToken.value
    if (latestRefreshToken) {
      try {
        await adminLogout(latestRefreshToken)
      } catch {
      }
    }

    clearSession()
  }

  // 本地彻底清空管理员会话。
  function clearSession() {
    accessToken.value = ''
    refreshToken.value = ''
    currentUser.value = null

    localStorage.removeItem(ACCESS_TOKEN_KEY)
    localStorage.removeItem(REFRESH_TOKEN_KEY)
    localStorage.removeItem(ADMIN_ID_KEY)
    localStorage.removeItem(ACCOUNT_KEY)
    localStorage.removeItem(DISPLAY_NAME_KEY)
    localStorage.removeItem(ROLE_KEY)
    localStorage.removeItem(PERMISSIONS_KEY)
  }

  return {
    accessToken,
    refreshToken,
    currentUser,
    isAuthenticated,
    displayName,
    role,
    permissions,
    login,
    loadCurrentUser,
    logout,
    clearSession
  }
})








