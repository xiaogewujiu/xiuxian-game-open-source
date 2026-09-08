/**
 * 后台前端共享的角色与权限常量。
 * 这里保持与后端 AdminPermissionCatalog / AdminRoleCatalog 的口径一致，避免页面散落硬编码。
 */
export const ADMIN_PERMISSIONS = {
  configRead: 'admin.config.read',
  configWrite: 'admin.config.write',
  playerRead: 'admin.player.read',
  playerWrite: 'admin.player.write',
  playerGrant: 'admin.player.grant',
  auditRead: 'admin.audit.read'
} as const

/**
 * 当前允许进入后台用户管理的角色。
 * 这个限制比通用配置权限更严，避免非核心管理员误改后台账号体系。
 */
export const ADMIN_USER_MANAGEMENT_ROLES = ['super_admin', 'admin'] as const

/**
 * 后台角色选项，用于管理员创建和编辑时的结构化选择。
 */
export const ADMIN_ROLE_OPTIONS = [
  { value: 'super_admin', label: '超级管理员' },
  { value: 'admin', label: '管理员' },
  { value: 'operator', label: '运营管理员' },
  { value: 'gm_support', label: 'GM客服' },
  { value: 'content_designer', label: '内容策划' },
  { value: 'economy_designer', label: '数值策划' },
  { value: 'audit_viewer', label: '审计查看者' }
] as const
