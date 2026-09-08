import type { AdminMarketConfig, AdminMarketListing, AdminMarketTransaction } from '@/types/admin'
import { request } from './http'

export function getMarketConfig() {
  return request<AdminMarketConfig>('/api/admin/market/config')
}

export function updateMarketConfig(payload: AdminMarketConfig) {
  return request<void>('/api/admin/market/config', {
    method: 'PUT',
    body: JSON.stringify(payload)
  })
}

export function getMarketListings(status?: string, keyword?: string, take = 200) {
  const params = new URLSearchParams()
  if (status) params.set('status', status)
  if (keyword) params.set('keyword', keyword)
  params.set('take', String(take))
  return request<AdminMarketListing[]>(`/api/admin/market/listings?${params}`)
}

export function forceCancelListing(id: number) {
  return request<void>(`/api/admin/market/listings/${id}/force-cancel`, {
    method: 'POST'
  })
}

export function getMarketTransactions(keyword?: string, take = 200) {
  const params = new URLSearchParams()
  if (keyword) params.set('keyword', keyword)
  params.set('take', String(take))
  return request<AdminMarketTransaction[]>(`/api/admin/market/transactions?${params}`)
}
