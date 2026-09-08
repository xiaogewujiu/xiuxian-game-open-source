import type { AdminMapDetail, AdminMapListItem } from '@/types/admin'
import { request } from './http'

export function getAdminMaps(keyword = '', mapKind?: string | null) {
  const params = new URLSearchParams()
  if (keyword) params.set('keyword', keyword)
  if (mapKind) params.set('mapKind', mapKind)
  const query = params.toString()
  return request<AdminMapListItem[]>(`/api/admin/maps${query ? `?${query}` : ''}`)
}

export function getAdminMapDetail(mapId: string) {
  return request<AdminMapDetail>(`/api/admin/maps/${encodeURIComponent(mapId)}`)
}

export function saveAdminMap(payload: AdminMapDetail) {
  return request<AdminMapDetail>('/api/admin/maps', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function deleteAdminMap(mapId: string) {
  return request<void>(`/api/admin/maps/${encodeURIComponent(mapId)}`, {
    method: 'DELETE'
  })
}
