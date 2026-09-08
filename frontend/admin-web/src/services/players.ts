import type {
  AdminBanPlayerRequest,
  AdminGrantCurrencyRequest,
  AdminGrantItemRequest,
  AdminOfflineBattleListItem,
  AdminOfflineBattleSummary,
  AdminPlayerDetail,
  AdminPlayerListItem
} from '@/types/admin'
import { request } from './http'

// 获取玩家列表，可按关键字筛选。
export function getAdminPlayers(keyword = '', isOfflineBattling?: boolean | null) {
  const params = new URLSearchParams()
  if (keyword) params.set('keyword', keyword)
  if (isOfflineBattling != null) params.set('isOfflineBattling', String(isOfflineBattling))
  const query = params.toString()
  return request<AdminPlayerListItem[]>(`/api/admin/players${query ? `?${query}` : ''}`)
}

// 获取单个玩家详情。
export function getAdminPlayerDetail(playerId: string) {
  return request<AdminPlayerDetail>(`/api/admin/players/${encodeURIComponent(playerId)}`)
}

// 获取离线挂机中的玩家列表。
export function getAdminOfflineBattles(keyword = '') {
  const query = keyword ? `?keyword=${encodeURIComponent(keyword)}` : ''
  return request<AdminOfflineBattleListItem[]>(`/api/admin/players/offline-battles${query}`)
}

// 保存玩家后台编辑结果。
export function saveAdminPlayer(payload: AdminPlayerDetail) {
  return request<AdminPlayerDetail>('/api/admin/players', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

// 向指定玩家发放货币。
export function grantAdminPlayerCurrency(playerId: string, payload: AdminGrantCurrencyRequest) {
  return request<AdminPlayerDetail>(`/api/admin/players/${encodeURIComponent(playerId)}/grant-currency`, {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

// 向指定玩家发放道具。
export function grantAdminPlayerItem(playerId: string, payload: AdminGrantItemRequest) {
  return request(`/api/admin/players/${encodeURIComponent(playerId)}/grant-item`, {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

// 封禁指定玩家。
export function banAdminPlayer(playerId: string, payload: AdminBanPlayerRequest) {
  return request<AdminPlayerDetail>(`/api/admin/players/${encodeURIComponent(playerId)}/ban`, {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

// 解封指定玩家。
export function unbanAdminPlayer(playerId: string) {
  return request<AdminPlayerDetail>(`/api/admin/players/${encodeURIComponent(playerId)}/unban`, {
    method: 'POST'
  })
}

// 强制停止指定玩家的离线挂机，并返回本次挂机汇总。
export function stopAdminPlayerOfflineBattle(playerId: string) {
  return request<AdminOfflineBattleSummary>(`/api/admin/players/${encodeURIComponent(playerId)}/offline-stop`, {
    method: 'POST'
  })
}
