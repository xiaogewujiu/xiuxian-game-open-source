import type {
  AdminWorldBossRuntime,
  AdminWorldBossSchedule,
  AdminWorldBossTemplateDetail,
  AdminWorldBossTemplateListItem
} from '@/types/admin'
import { request } from './http'

// 获取世界 Boss 模板列表。
export function getAdminWorldBossTemplates() {
  return request<AdminWorldBossTemplateListItem[]>('/api/admin/world-boss/templates')
}

// 获取单个世界 Boss 模板详情。
export function getAdminWorldBossTemplate(bossId: string) {
  return request<AdminWorldBossTemplateDetail>(`/api/admin/world-boss/templates/${encodeURIComponent(bossId)}`)
}

// 保存世界 Boss 模板。
export function saveAdminWorldBossTemplate(payload: AdminWorldBossTemplateDetail) {
  return request<AdminWorldBossTemplateDetail>('/api/admin/world-boss/templates', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

// 删除世界 Boss 模板。
export function deleteAdminWorldBossTemplate(bossId: string) {
  return request<void>(`/api/admin/world-boss/templates/${encodeURIComponent(bossId)}`, {
    method: 'DELETE'
  })
}

// 获取世界 Boss 排期配置。
export function getAdminWorldBossSchedule() {
  return request<AdminWorldBossSchedule>('/api/admin/world-boss/schedule')
}

// 保存世界 Boss 排期配置。
export function saveAdminWorldBossSchedule(payload: AdminWorldBossSchedule) {
  return request<AdminWorldBossSchedule>('/api/admin/world-boss/schedule', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

// 获取后台世界 Boss 运行时总览。
export function getAdminWorldBossRuntime() {
  return request<AdminWorldBossRuntime>('/api/admin/world-boss/runtime')
}

// 后台手动生成一个世界 Boss；不传 bossId 时按后端规则自动选择。
export function spawnAdminWorldBoss(bossId = '') {
  const query = bossId ? `?bossId=${encodeURIComponent(bossId)}` : ''
  return request(`/api/admin/world-boss/runtime/spawn${query}`, {
    method: 'POST'
  })
}

// 后台关闭当前世界 Boss；reason 会作为查询参数传给后端。
export function closeAdminWorldBoss(reason = '') {
  const query = reason ? `?reason=${encodeURIComponent(reason)}` : ''
  return request(`/api/admin/world-boss/runtime/close${query}`, {
    method: 'POST'
  })
}
