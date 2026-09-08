import type {
  AdminForgeProfessionLevelRule,
  AdminForgeProfessionRule,
  AdminForgeSystemDetail,
  AdminForgeSystemListItem
} from '@/types/admin'
import { request } from './http'

export function getAdminForgeSystems(keyword = '') {
  const query = keyword ? `?keyword=${encodeURIComponent(keyword)}` : ''
  return request<AdminForgeSystemListItem[]>(`/api/admin/forge-systems${query}`)
}

export function getAdminForgeSystemDetail(playerId: string) {
  return request<AdminForgeSystemDetail>(`/api/admin/forge-systems/${encodeURIComponent(playerId)}`)
}

export function saveAdminForgeSystem(payload: AdminForgeSystemDetail) {
  return request<AdminForgeSystemDetail>('/api/admin/forge-systems', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function clearAdminForgeTask(playerId: string) {
  return request<AdminForgeSystemDetail>(`/api/admin/forge-systems/${encodeURIComponent(playerId)}/clear-task`, {
    method: 'POST'
  })
}

export function getAdminForgeProfessionLevelRules() {
  return request<AdminForgeProfessionLevelRule[]>('/api/admin/forge-rules/levels')
}

export function getAdminForgeProfessionLevelRule(level: number) {
  return request<AdminForgeProfessionLevelRule>(`/api/admin/forge-rules/levels/${level}`)
}

export function saveAdminForgeProfessionLevelRule(payload: AdminForgeProfessionLevelRule) {
  return request<AdminForgeProfessionLevelRule>('/api/admin/forge-rules/levels', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function deleteAdminForgeProfessionLevelRule(level: number) {
  return request<void>(`/api/admin/forge-rules/levels/${level}`, {
    method: 'DELETE'
  })
}

export function getAdminForgeProfessionRule() {
  return request<AdminForgeProfessionRule>('/api/admin/forge-rules/config')
}

export function saveAdminForgeProfessionRule(payload: AdminForgeProfessionRule) {
  return request<AdminForgeProfessionRule>('/api/admin/forge-rules/config', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}
