import type {
  AdminFavorabilityGiftLog,
  AdminFavorabilityLevelConfig,
  AdminFavorabilityRelation,
  AdminFavorabilityStats,
  AdminFavorabilityLeaderboard,
  AdminDeepFriendship,
  AdminPagedResult
} from '@/types/admin'
import { request } from './http'

export function getFavorabilityRelations(keyword = '', pageIndex = 1, pageSize = 20) {
  const params = new URLSearchParams()
  if (keyword) params.set('keyword', keyword)
  params.set('pageIndex', String(pageIndex))
  params.set('pageSize', String(pageSize))
  return request<AdminPagedResult<AdminFavorabilityRelation>>(`/api/admin/favorability/relations?${params}`)
}

export function updateFavorabilityRelation(id: string, newValue: number) {
  return request<AdminFavorabilityRelation>(`/api/admin/favorability/relations/${id}`, {
    method: 'PUT',
    body: JSON.stringify({ newValue })
  })
}

export function deleteFavorabilityRelation(id: string) {
  return request<void>(`/api/admin/favorability/relations/${id}`, {
    method: 'DELETE'
  })
}

export function getFavorabilityGiftLog(playerId = '', targetPlayerId = '', pageIndex = 1, pageSize = 20) {
  const params = new URLSearchParams()
  if (playerId) params.set('playerId', playerId)
  if (targetPlayerId) params.set('targetPlayerId', targetPlayerId)
  params.set('pageIndex', String(pageIndex))
  params.set('pageSize', String(pageSize))
  return request<AdminPagedResult<AdminFavorabilityGiftLog>>(`/api/admin/favorability/gift-log?${params}`)
}

export function getFavorabilityStats() {
  return request<AdminFavorabilityStats>('/api/admin/favorability/stats')
}

export function getFavorabilityLevels() {
  return request<AdminFavorabilityLevelConfig[]>('/api/admin/favorability/levels')
}

export function saveFavorabilityLevel(payload: Partial<AdminFavorabilityLevelConfig>) {
  return request<AdminFavorabilityLevelConfig>('/api/admin/favorability/levels', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function deleteFavorabilityLevel(id: string) {
  return request<void>(`/api/admin/favorability/levels/${id}`, {
    method: 'DELETE'
  })
}

export function getFavorabilityLeaderboard(count = 20) {
  return request<AdminFavorabilityLeaderboard[]>(`/api/admin/favorability/leaderboard?count=${count}`)
}

export function getDeepFriendshipLeaderboard(count = 10) {
  return request<AdminDeepFriendship[]>(`/api/admin/favorability/deep-friendship?count=${count}`)
}
