import type { AdminRankingConfigDetail, AdminRankingConfigListItem, AdminRankingRewardDetail, AdminRankingRewardListItem } from '@/types/admin'
import { request } from './http'

export function getAdminRankingConfigs(keyword = '') {
  const query = keyword ? `?keyword=${encodeURIComponent(keyword)}` : ''
  return request<AdminRankingConfigListItem[]>(`/api/admin/ranking-configs${query}`)
}

export function getAdminRankingConfigDetail(rankingId: string) {
  return request<AdminRankingConfigDetail>(`/api/admin/ranking-configs/${encodeURIComponent(rankingId)}`)
}

export function saveAdminRankingConfig(payload: AdminRankingConfigDetail) {
  return request<AdminRankingConfigDetail>('/api/admin/ranking-configs', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function deleteAdminRankingConfig(rankingId: string) {
  return request<void>(`/api/admin/ranking-configs/${encodeURIComponent(rankingId)}`, {
    method: 'DELETE'
  })
}

export function getAdminRankingRewards(rankingId = '') {
  const query = rankingId ? `?rankingId=${encodeURIComponent(rankingId)}` : ''
  return request<AdminRankingRewardListItem[]>(`/api/admin/ranking-rewards${query}`)
}

export function getAdminRankingRewardDetail(gid: string) {
  return request<AdminRankingRewardDetail>(`/api/admin/ranking-rewards/${encodeURIComponent(gid)}`)
}

export function saveAdminRankingReward(payload: AdminRankingRewardDetail) {
  return request<AdminRankingRewardDetail>('/api/admin/ranking-rewards', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function deleteAdminRankingReward(gid: string) {
  return request<void>(`/api/admin/ranking-rewards/${encodeURIComponent(gid)}`, {
    method: 'DELETE'
  })
}
