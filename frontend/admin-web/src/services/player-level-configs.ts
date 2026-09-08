import type {
  AdminPlayerLevelConfigDetail,
  AdminPlayerLevelConfigListItem
} from '@/types/admin'
import { request } from './http'

export function getAdminPlayerLevelConfigs() {
  return request<AdminPlayerLevelConfigListItem[]>('/api/admin/player-level-configs')
}

export function getAdminPlayerLevelConfigDetail(level: number) {
  return request<AdminPlayerLevelConfigDetail>(`/api/admin/player-level-configs/${level}`)
}

export function saveAdminPlayerLevelConfig(payload: AdminPlayerLevelConfigDetail) {
  return request<AdminPlayerLevelConfigDetail>('/api/admin/player-level-configs', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}
