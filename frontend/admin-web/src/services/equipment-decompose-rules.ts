import type { AdminEquipmentDecomposeRule } from '@/types/admin'
import { request } from './http'

export function getEquipmentDecomposeRules() {
  return request<AdminEquipmentDecomposeRule[]>('/api/admin/equipment-decompose-rules')
}

export function saveEquipmentDecomposeRule(payload: AdminEquipmentDecomposeRule) {
  return request<void>('/api/admin/equipment-decompose-rules', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function deleteEquipmentDecomposeRule(gid: number) {
  return request<void>(`/api/admin/equipment-decompose-rules/${gid}`, { method: 'DELETE' })
}
