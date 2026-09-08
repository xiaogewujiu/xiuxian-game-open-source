import type { AdminCropDetail, AdminCropListItem } from '@/types/admin'
import { request } from './http'

export function getAdminCrops(keyword = '') {
  const query = keyword ? `?keyword=${encodeURIComponent(keyword)}` : ''
  return request<AdminCropListItem[]>(`/api/admin/crops${query}`)
}

export function getAdminCropDetail(templateId: string) {
  return request<AdminCropDetail>(`/api/admin/crops/${encodeURIComponent(templateId)}`)
}

export function saveAdminCrop(payload: AdminCropDetail) {
  return request<AdminCropDetail>('/api/admin/crops', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function deleteAdminCrop(templateId: string) {
  return request<void>(`/api/admin/crops/${encodeURIComponent(templateId)}`, {
    method: 'DELETE'
  })
}
