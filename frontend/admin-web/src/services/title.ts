import type { AdminTitleDetail, AdminTitleListItem, AdminGrantTitlePayload } from '@/types/admin'
import { request } from './http'

export function getAdminTitles(keyword = '', take = 200) {
  const params = new URLSearchParams()
  if (keyword) params.set('keyword', keyword)
  params.set('take', String(take))
  const query = params.toString()
  return request<AdminTitleListItem[]>(`/api/admin/title?${query}`)
}

export function getAdminTitleDetail(id: number) {
  return request<AdminTitleDetail>(`/api/admin/title/${id}`)
}

export function createAdminTitle(payload: AdminTitleDetail) {
  return request<AdminTitleDetail>('/api/admin/title', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function updateAdminTitle(id: number, payload: AdminTitleDetail) {
  return request<AdminTitleDetail>(`/api/admin/title/${id}`, {
    method: 'PUT',
    body: JSON.stringify(payload)
  })
}

export function deleteAdminTitle(id: number) {
  return request<void>(`/api/admin/title/${id}`, {
    method: 'DELETE'
  })
}

export function grantTitle(payload: AdminGrantTitlePayload) {
  return request<void>('/api/admin/title/grant', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function revokeTitle(payload: AdminGrantTitlePayload) {
  return request<void>('/api/admin/title/revoke', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}
