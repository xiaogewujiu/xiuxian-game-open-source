import type { AdminElementRelationEntry } from '@/types/admin'
import { request } from './http'

export function getAdminElementRelations() {
  return request<AdminElementRelationEntry[]>('/api/admin/element-rules')
}

export function saveAdminElementRelations(payload: AdminElementRelationEntry[]) {
  return request<AdminElementRelationEntry[]>('/api/admin/element-rules', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}
