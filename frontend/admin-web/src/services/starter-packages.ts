import type { AdminStarterPackageDetail, AdminStarterPackageListItem } from '@/types/admin'
import { request } from './http'

export function getAdminStarterPackages(keyword = '', autoGrantOnRegister?: boolean | null) {
  const params = new URLSearchParams()
  if (keyword) params.set('keyword', keyword)
  if (autoGrantOnRegister != null) params.set('autoGrantOnRegister', String(autoGrantOnRegister))
  const query = params.toString()
  return request<AdminStarterPackageListItem[]>(`/api/admin/starter-packages${query ? `?${query}` : ''}`)
}

export function getAdminStarterPackageDetail(packageId: string) {
  return request<AdminStarterPackageDetail>(`/api/admin/starter-packages/${encodeURIComponent(packageId)}`)
}

export function saveAdminStarterPackage(payload: AdminStarterPackageDetail) {
  return request<AdminStarterPackageDetail>('/api/admin/starter-packages', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function deleteAdminStarterPackage(packageId: string) {
  return request<void>(`/api/admin/starter-packages/${encodeURIComponent(packageId)}`, {
    method: 'DELETE'
  })
}
