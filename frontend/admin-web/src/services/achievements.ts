import type { AdminAchievementDetail, AdminAchievementListItem } from '@/types/admin'
import { request } from './http'

export function getAdminAchievements(keyword = '', difficulty?: number | null) {
  const params = new URLSearchParams()
  if (keyword) params.set('keyword', keyword)
  if (difficulty != null) params.set('difficulty', String(difficulty))
  const query = params.toString()
  return request<AdminAchievementListItem[]>(`/api/admin/achievements${query ? `?${query}` : ''}`)
}

export function getAdminAchievementDetail(achievementId: string) {
  return request<AdminAchievementDetail>(`/api/admin/achievements/${encodeURIComponent(achievementId)}`)
}

export function saveAdminAchievement(payload: AdminAchievementDetail) {
  return request<AdminAchievementDetail>('/api/admin/achievements', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function deleteAdminAchievement(achievementId: string) {
  return request<void>(`/api/admin/achievements/${encodeURIComponent(achievementId)}`, {
    method: 'DELETE'
  })
}
