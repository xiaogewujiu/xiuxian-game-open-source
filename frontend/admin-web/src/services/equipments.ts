import type { AdminEquipmentDetail, AdminEquipmentListItem } from '@/types/admin'
import { request } from './http'

export function getAdminEquipments(keyword = '', slot?: number | null) {
  const params = new URLSearchParams()
  if (keyword) params.set('keyword', keyword)
  if (slot != null) params.set('slot', String(slot))
  const query = params.toString()
  return request<AdminEquipmentListItem[]>(`/api/admin/equipments${query ? `?${query}` : ''}`)
}

export function getAdminEquipmentDetail(equipmentId: number) {
  return request<AdminEquipmentDetail>(`/api/admin/equipments/${equipmentId}`)
}

export function saveAdminEquipment(payload: AdminEquipmentDetail) {
  return request<AdminEquipmentDetail>('/api/admin/equipments', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function deleteAdminEquipment(equipmentId: number) {
  return request<void>(`/api/admin/equipments/${equipmentId}`, {
    method: 'DELETE'
  })
}
