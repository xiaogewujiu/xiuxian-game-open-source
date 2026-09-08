import type { AdminAttributePointConfigBundle } from '@/types/admin'
import { request } from './http'

export function getAdminAttributePointConfig() {
  return request<AdminAttributePointConfigBundle>('/api/admin/attribute-point-configs')
}

export function saveAdminAttributePointConfig(payload: AdminAttributePointConfigBundle) {
  return request<AdminAttributePointConfigBundle>('/api/admin/attribute-point-configs', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}
