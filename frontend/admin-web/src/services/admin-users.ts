import type { AdminCreateUserRequest, AdminResetPasswordRequest, AdminUpdateUserRequest, AdminUserDetail, AdminUserListItem } from '@/types/admin'
import { request } from './http'

export function getAdminUsers(keyword = '', isActive?: boolean | null) {
  const params = new URLSearchParams()
  if (keyword) params.set('keyword', keyword)
  if (isActive != null) params.set('isActive', String(isActive))
  const query = params.toString()
  return request<AdminUserListItem[]>(`/api/admin/admin-users${query ? `?${query}` : ''}`)
}

export function getAdminUserDetail(adminId: string) {
  return request<AdminUserDetail>(`/api/admin/admin-users/${encodeURIComponent(adminId)}`)
}

export function createAdminUser(payload: AdminCreateUserRequest) {
  return request<AdminUserDetail>('/api/admin/admin-users', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function updateAdminUser(adminId: string, payload: AdminUpdateUserRequest) {
  return request<AdminUserDetail>(`/api/admin/admin-users/${encodeURIComponent(adminId)}`, {
    method: 'PUT',
    body: JSON.stringify(payload)
  })
}

export function resetAdminUserPassword(adminId: string, payload: AdminResetPasswordRequest) {
  return request<void>(`/api/admin/admin-users/${encodeURIComponent(adminId)}/reset-password`, {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}
