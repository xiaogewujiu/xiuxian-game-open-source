import { computed } from 'vue'
import { ADMIN_PERMISSIONS, ADMIN_USER_MANAGEMENT_ROLES } from '@/constants/admin'
import { useAuthStore } from '@/stores/auth'

/**
 * 统一封装后台前端的权限判断，避免页面内散落权限字符串。
 */
export function useAdminPermissions() {
  const authStore = useAuthStore()

  function hasPermission(permission: string) {
    return authStore.permissions.includes(permission)
  }

  function hasRole(...roles: string[]) {
    return roles.includes(authStore.role)
  }

  const canReadConfigs = computed(() => hasPermission(ADMIN_PERMISSIONS.configRead))
  const canManageConfigs = computed(() => hasPermission(ADMIN_PERMISSIONS.configWrite))
  const canReadPlayers = computed(() => hasPermission(ADMIN_PERMISSIONS.playerRead))
  const canWritePlayers = computed(() => hasPermission(ADMIN_PERMISSIONS.playerWrite))
  const canGrantPlayers = computed(() => hasPermission(ADMIN_PERMISSIONS.playerGrant))
  const canReadAudit = computed(() => hasPermission(ADMIN_PERMISSIONS.auditRead))
  const canManageAdminUsers = computed(() => hasRole(...ADMIN_USER_MANAGEMENT_ROLES))

  return {
    hasPermission,
    hasRole,
    canReadConfigs,
    canManageConfigs,
    canReadPlayers,
    canWritePlayers,
    canGrantPlayers,
    canReadAudit,
    canManageAdminUsers
  }
}
