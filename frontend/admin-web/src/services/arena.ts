import type { AdminArenaOverview, AdminArenaPlayer, AdminArenaBattleLog, AdminArenaSeason, AdminArenaSeasonReward, AdminSettleSeasonResult } from '@/types/admin'
import { request } from './http'

export function getArenaOverview() {
  return request<AdminArenaOverview>('/api/admin/arena/overview')
}

export function getArenaPlayers(keyword = '', take = 200) {
  const params = new URLSearchParams()
  if (keyword) params.set('keyword', keyword)
  params.set('take', String(take))
  return request<AdminArenaPlayer[]>(`/api/admin/arena/players?${params}`)
}

export function getArenaBattleLogs(playerId = '', take = 100) {
  const params = new URLSearchParams()
  if (playerId) params.set('playerId', playerId)
  params.set('take', String(take))
  return request<AdminArenaBattleLog[]>(`/api/admin/arena/logs?${params}`)
}

export function adjustArenaPoints(playerId: string, points: number) {
  return request<void>('/api/admin/arena/adjust-points', {
    method: 'POST',
    body: JSON.stringify({ playerId, points })
  })
}

export function banArenaPlayer(playerId: string, hours: number, reason: string) {
  return request<void>('/api/admin/arena/ban', {
    method: 'POST',
    body: JSON.stringify({ playerId, hours, reason })
  })
}

export function unbanArenaPlayer(playerId: string) {
  return request<void>('/api/admin/arena/unban', {
    method: 'POST',
    body: JSON.stringify({ playerId })
  })
}

export function getArenaSeasonInfo() {
  return request<AdminArenaSeason>('/api/admin/arena/season-info')
}

export function settleArenaSeason() {
  return request<AdminSettleSeasonResult>('/api/admin/arena/settle-season', {
    method: 'POST'
  })
}

export function resetArenaSeason() {
  return request<void>('/api/admin/arena/reset-season', {
    method: 'POST'
  })
}

export function adjustArenaSeason(seasonDuration: number, seasonEnabled?: boolean) {
  return request<void>('/api/admin/arena/adjust-season', {
    method: 'POST',
    body: JSON.stringify({ seasonDuration, seasonEnabled })
  })
}

export function getArenaSeasonRewards() {
  return request<AdminArenaSeasonReward[]>('/api/admin/arena/season-rewards')
}

export function saveArenaSeasonReward(reward: Partial<AdminArenaSeasonReward> & { minRank: number; maxRank: number; rewardTitle: string }) {
  return request<void>('/api/admin/arena/season-rewards', {
    method: 'POST',
    body: JSON.stringify(reward)
  })
}

export function deleteArenaSeasonReward(gid: string) {
  return request<void>(`/api/admin/arena/season-rewards/${gid}`, {
    method: 'DELETE'
  })
}
