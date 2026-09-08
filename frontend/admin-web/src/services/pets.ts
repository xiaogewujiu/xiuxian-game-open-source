import type { AdminPetDetail, AdminPetListItem } from '@/types/admin'
import { request } from './http'

export function getAdminPets(keyword = '') {
  const query = keyword ? `?keyword=${encodeURIComponent(keyword)}` : ''
  return request<AdminPetListItem[]>(`/api/admin/pets${query}`)
}

export function getAdminPetDetail(templateId: string) {
  return request<AdminPetDetail>(`/api/admin/pets/${encodeURIComponent(templateId)}`)
}

export function saveAdminPet(payload: AdminPetDetail) {
  return request<AdminPetDetail>('/api/admin/pets', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function deleteAdminPet(templateId: string) {
  return request<void>(`/api/admin/pets/${encodeURIComponent(templateId)}`, {
    method: 'DELETE'
  })
}
