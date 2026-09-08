import type { AdminDungeonDetail, AdminDungeonListItem } from '@/types/admin'
import { request } from './http'

export function getAdminDungeons(keyword = '') {
  const query = keyword ? `?keyword=${encodeURIComponent(keyword)}` : ''
  return request<AdminDungeonListItem[]>(`/api/admin/dungeons${query}`)
}

export function getAdminDungeonDetail(dungeonId: string) {
  return request<AdminDungeonDetail>(`/api/admin/dungeons/${encodeURIComponent(dungeonId)}`)
}

export function saveAdminDungeon(payload: AdminDungeonDetail) {
  return request<AdminDungeonDetail>('/api/admin/dungeons', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function deleteAdminDungeon(dungeonId: string) {
  return request<void>(`/api/admin/dungeons/${encodeURIComponent(dungeonId)}`, {
    method: 'DELETE'
  })
}
