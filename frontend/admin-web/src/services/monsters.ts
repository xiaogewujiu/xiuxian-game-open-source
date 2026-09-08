import type { AdminMonsterDetail, AdminMonsterListItem } from '@/types/admin'
import { request } from './http'

export function getAdminMonsters(keyword = '') {
  const query = keyword ? `?keyword=${encodeURIComponent(keyword)}` : ''
  return request<AdminMonsterListItem[]>(`/api/admin/monsters${query}`)
}

export function getAdminMonsterDetail(monsterId: string) {
  return request<AdminMonsterDetail>(`/api/admin/monsters/${encodeURIComponent(monsterId)}`)
}

export function saveAdminMonster(payload: AdminMonsterDetail) {
  return request<AdminMonsterDetail>('/api/admin/monsters', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function deleteAdminMonster(monsterId: string) {
  return request<void>(`/api/admin/monsters/${encodeURIComponent(monsterId)}`, {
    method: 'DELETE'
  })
}
