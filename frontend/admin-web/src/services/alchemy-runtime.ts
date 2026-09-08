import type {
  AdminAlchemyProfessionLevelRule,
  AdminAlchemyProfessionRule,
  AdminAlchemySystemDetail,
  AdminAlchemySystemListItem
} from '@/types/admin'
import { request } from './http'

export function getAdminAlchemySystems(keyword = '') {
  const query = keyword ? `?keyword=${encodeURIComponent(keyword)}` : ''
  return request<AdminAlchemySystemListItem[]>(`/api/admin/alchemy-systems${query}`)
}

export function getAdminAlchemySystemDetail(playerId: string) {
  return request<AdminAlchemySystemDetail>(`/api/admin/alchemy-systems/${encodeURIComponent(playerId)}`)
}

export function saveAdminAlchemySystem(payload: AdminAlchemySystemDetail) {
  return request<AdminAlchemySystemDetail>('/api/admin/alchemy-systems', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function clearAdminAlchemyTask(playerId: string) {
  return request<AdminAlchemySystemDetail>(`/api/admin/alchemy-systems/${encodeURIComponent(playerId)}/clear-task`, {
    method: 'POST'
  })
}

export function getAdminAlchemyProfessionLevelRules() {
  return request<AdminAlchemyProfessionLevelRule[]>('/api/admin/alchemy-rules/levels')
}

export function getAdminAlchemyProfessionLevelRule(level: number) {
  return request<AdminAlchemyProfessionLevelRule>(`/api/admin/alchemy-rules/levels/${level}`)
}

export function saveAdminAlchemyProfessionLevelRule(payload: AdminAlchemyProfessionLevelRule) {
  return request<AdminAlchemyProfessionLevelRule>('/api/admin/alchemy-rules/levels', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function deleteAdminAlchemyProfessionLevelRule(level: number) {
  return request<void>(`/api/admin/alchemy-rules/levels/${level}`, {
    method: 'DELETE'
  })
}

export function getAdminAlchemyProfessionRule() {
  return request<AdminAlchemyProfessionRule>('/api/admin/alchemy-rules/config')
}

export function saveAdminAlchemyProfessionRule(payload: AdminAlchemyProfessionRule) {
  return request<AdminAlchemyProfessionRule>('/api/admin/alchemy-rules/config', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}
