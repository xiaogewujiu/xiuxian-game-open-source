import type { AdminCheckInRewardConfig, AdminRedeemCodeConfig } from '@/types/admin'
import { request } from './http'

export function getCheckInConfigs(isMilestone?: boolean | null) {
  const params = new URLSearchParams()
  if (isMilestone != null) params.set('isMilestone', String(isMilestone))
  const query = params.toString()
  return request<AdminCheckInRewardConfig[]>(`/api/admin/checkin-configs${query ? `?${query}` : ''}`)
}

export function saveCheckInConfig(payload: AdminCheckInRewardConfig) {
  return request<AdminCheckInRewardConfig>('/api/admin/checkin-configs', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function deleteCheckInConfig(continuousDay: number) {
  return request<void>(`/api/admin/checkin-configs/${continuousDay}`, {
    method: 'DELETE'
  })
}

export function getRedeemCodeConfigs(keyword = '', isEnabled?: boolean | null) {
  const params = new URLSearchParams()
  if (keyword) params.set('keyword', keyword)
  if (isEnabled != null) params.set('isEnabled', String(isEnabled))
  const query = params.toString()
  return request<AdminRedeemCodeConfig[]>(`/api/admin/redeem-codes${query ? `?${query}` : ''}`)
}

export function saveRedeemCodeConfig(payload: AdminRedeemCodeConfig) {
  return request<AdminRedeemCodeConfig>('/api/admin/redeem-codes', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function deleteRedeemCodeConfig(code: string) {
  return request<void>(`/api/admin/redeem-codes/${encodeURIComponent(code)}`, {
    method: 'DELETE'
  })
}
