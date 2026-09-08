import type { AdminTowerOverview, AdminTowerPlayer, AdminTowerFloorConfig, TowerFloorDistribution } from '@/types/admin'
import { request } from './http'

export function getTowerOverview() {
  return request<AdminTowerOverview>('/api/admin/tower/overview')
}

export function getTowerPlayers(keyword = '', take = 200) {
  const params = new URLSearchParams()
  if (keyword) params.set('keyword', keyword)
  params.set('take', String(take))
  return request<AdminTowerPlayer[]>(`/api/admin/tower/players?${params}`)
}

export function getTowerFloorConfigs() {
  return request<AdminTowerFloorConfig[]>('/api/admin/tower/floors')
}

export function updateTowerFloorConfig(id: number, payload: AdminTowerFloorConfig) {
  return request<void>(`/api/admin/tower/floors/${id}`, {
    method: 'PUT',
    body: JSON.stringify(payload)
  })
}

export function batchGenerateTowerFloors(payload: {
  monsterTemplateIdsJson: string
  monsterCountPerFloor: number
  baseMultiplier: number
  multiplierGrowth: number
  baseRewardGold: number
  baseRewardExp: number
}) {
  return request<number>('/api/admin/tower/floors/batch-generate', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function getTowerFloorDistribution() {
  return request<TowerFloorDistribution[]>('/api/admin/tower/distribution')
}
