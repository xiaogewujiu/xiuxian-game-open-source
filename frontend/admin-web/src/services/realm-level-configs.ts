import type {
  AdminRealmLevelConfigDetail,
  AdminRealmLevelConfigListItem
} from '@/types/admin'
import { request } from './http'

export function getAdminRealmLevelConfigs() {
  return request<AdminRealmLevelConfigListItem[]>('/api/admin/realm-level-configs')
}

export function getAdminRealmLevelConfigDetail(level: number) {
  return request<AdminRealmLevelConfigDetail>(`/api/admin/realm-level-configs/${level}`)
}

export function saveAdminRealmLevelConfig(payload: AdminRealmLevelConfigDetail) {
  return request<AdminRealmLevelConfigDetail>('/api/admin/realm-level-configs', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}
