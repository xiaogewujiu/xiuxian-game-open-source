import type { AdminQuestDetail, AdminQuestListItem } from '@/types/admin'
import { request } from './http'

export function getAdminQuests(keyword = '', questType?: number | null, resetCycle?: number | null) {
  const params = new URLSearchParams()
  if (keyword) params.set('keyword', keyword)
  if (questType != null) params.set('questType', String(questType))
  if (resetCycle != null) params.set('resetCycle', String(resetCycle))
  const query = params.toString()
  return request<AdminQuestListItem[]>(`/api/admin/quests${query ? `?${query}` : ''}`)
}

export function getAdminQuestDetail(questId: string) {
  return request<AdminQuestDetail>(`/api/admin/quests/${encodeURIComponent(questId)}`)
}

export function saveAdminQuest(payload: AdminQuestDetail) {
  return request<AdminQuestDetail>('/api/admin/quests', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function deleteAdminQuest(questId: string) {
  return request<void>(`/api/admin/quests/${encodeURIComponent(questId)}`, {
    method: 'DELETE'
  })
}
