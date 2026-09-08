import type {
  AdminTextCollectionSeriesListItem, AdminTextCollectionSeriesDetail,
  AdminTextCollectionItemListItem, AdminTextCollectionItemDetail,
  AdminTextCollectionBonusListItem, AdminTextCollectionBonusDetail,
  AdminImageCollectionSeriesListItem, AdminImageCollectionSeriesDetail,
  AdminImageCollectionItemListItem, AdminImageCollectionItemDetail,
  AdminImageCollectionBonusListItem, AdminImageCollectionBonusDetail,
  AdminLotteryPoolListItem, AdminLotteryPoolDetail,
  AdminLotteryPrizeListItem, AdminLotteryPrizeDetail,
  AdminLotteryLogListItem, AdminPagedResult
} from '@/types/admin'
import { request } from './http'

// ===== 文字图鉴系列 =====
export function getAdminTextCollectionSeries(keyword = '') {
  const query = keyword ? `?keyword=${encodeURIComponent(keyword)}` : ''
  return request<AdminTextCollectionSeriesListItem[]>(`/api/admin/text-collection-series${query}`)
}
export function getAdminTextCollectionSeriesDetail(seriesId: string) {
  return request<AdminTextCollectionSeriesDetail>(`/api/admin/text-collection-series/${encodeURIComponent(seriesId)}`)
}
export function saveAdminTextCollectionSeries(payload: AdminTextCollectionSeriesDetail) {
  return request<AdminTextCollectionSeriesDetail>('/api/admin/text-collection-series', { method: 'POST', body: JSON.stringify(payload) })
}
export function deleteAdminTextCollectionSeries(seriesId: string) {
  return request<void>(`/api/admin/text-collection-series/${encodeURIComponent(seriesId)}`, { method: 'DELETE' })
}

// ===== 文字图鉴项 =====
export function getAdminTextCollectionItems(seriesId = '') {
  const query = seriesId ? `?seriesId=${encodeURIComponent(seriesId)}` : ''
  return request<AdminTextCollectionItemListItem[]>(`/api/admin/text-collection-items${query}`)
}
export function getAdminTextCollectionItemDetail(itemId: string) {
  return request<AdminTextCollectionItemDetail>(`/api/admin/text-collection-items/${encodeURIComponent(itemId)}`)
}
export function saveAdminTextCollectionItem(payload: AdminTextCollectionItemDetail) {
  return request<AdminTextCollectionItemDetail>('/api/admin/text-collection-items', { method: 'POST', body: JSON.stringify(payload) })
}
export function deleteAdminTextCollectionItem(itemId: string) {
  return request<void>(`/api/admin/text-collection-items/${encodeURIComponent(itemId)}`, { method: 'DELETE' })
}

// ===== 文字图鉴属性加成 =====
export function getAdminTextCollectionBonuses(seriesId = '') {
  const query = seriesId ? `?seriesId=${encodeURIComponent(seriesId)}` : ''
  return request<AdminTextCollectionBonusListItem[]>(`/api/admin/text-collection-bonuses${query}`)
}
export function getAdminTextCollectionBonusDetail(bonusId: string) {
  return request<AdminTextCollectionBonusDetail>(`/api/admin/text-collection-bonuses/${encodeURIComponent(bonusId)}`)
}
export function saveAdminTextCollectionBonus(payload: AdminTextCollectionBonusDetail) {
  return request<AdminTextCollectionBonusDetail>('/api/admin/text-collection-bonuses', { method: 'POST', body: JSON.stringify(payload) })
}
export function deleteAdminTextCollectionBonus(bonusId: string) {
  return request<void>(`/api/admin/text-collection-bonuses/${encodeURIComponent(bonusId)}`, { method: 'DELETE' })
}

// ===== 图片图鉴系列 =====
export function getAdminImageCollectionSeries(keyword = '') {
  const query = keyword ? `?keyword=${encodeURIComponent(keyword)}` : ''
  return request<AdminImageCollectionSeriesListItem[]>(`/api/admin/image-collection-series${query}`)
}
export function getAdminImageCollectionSeriesDetail(seriesId: string) {
  return request<AdminImageCollectionSeriesDetail>(`/api/admin/image-collection-series/${encodeURIComponent(seriesId)}`)
}
export function saveAdminImageCollectionSeries(payload: AdminImageCollectionSeriesDetail) {
  return request<AdminImageCollectionSeriesDetail>('/api/admin/image-collection-series', { method: 'POST', body: JSON.stringify(payload) })
}
export function deleteAdminImageCollectionSeries(seriesId: string) {
  return request<void>(`/api/admin/image-collection-series/${encodeURIComponent(seriesId)}`, { method: 'DELETE' })
}

// ===== 图片图鉴项 =====
export function getAdminImageCollectionItems(seriesId = '') {
  const query = seriesId ? `?seriesId=${encodeURIComponent(seriesId)}` : ''
  return request<AdminImageCollectionItemListItem[]>(`/api/admin/image-collection-items${query}`)
}
export function getAdminImageCollectionItemDetail(itemId: string) {
  return request<AdminImageCollectionItemDetail>(`/api/admin/image-collection-items/${encodeURIComponent(itemId)}`)
}
export function saveAdminImageCollectionItem(payload: AdminImageCollectionItemDetail) {
  return request<AdminImageCollectionItemDetail>('/api/admin/image-collection-items', { method: 'POST', body: JSON.stringify(payload) })
}
export function deleteAdminImageCollectionItem(itemId: string) {
  return request<void>(`/api/admin/image-collection-items/${encodeURIComponent(itemId)}`, { method: 'DELETE' })
}

// ===== 图片图鉴属性加成 =====
export function getAdminImageCollectionBonuses(seriesId = '') {
  const query = seriesId ? `?seriesId=${encodeURIComponent(seriesId)}` : ''
  return request<AdminImageCollectionBonusListItem[]>(`/api/admin/image-collection-bonuses${query}`)
}
export function getAdminImageCollectionBonusDetail(bonusId: string) {
  return request<AdminImageCollectionBonusDetail>(`/api/admin/image-collection-bonuses/${encodeURIComponent(bonusId)}`)
}
export function saveAdminImageCollectionBonus(payload: AdminImageCollectionBonusDetail) {
  return request<AdminImageCollectionBonusDetail>('/api/admin/image-collection-bonuses', { method: 'POST', body: JSON.stringify(payload) })
}
export function deleteAdminImageCollectionBonus(bonusId: string) {
  return request<void>(`/api/admin/image-collection-bonuses/${encodeURIComponent(bonusId)}`, { method: 'DELETE' })
}

// ===== 抽奖池 =====
export function getAdminLotteryPools(keyword = '') {
  const query = keyword ? `?keyword=${encodeURIComponent(keyword)}` : ''
  return request<AdminLotteryPoolListItem[]>(`/api/admin/lottery-pools${query}`)
}
export function getAdminLotteryPoolDetail(poolId: string) {
  return request<AdminLotteryPoolDetail>(`/api/admin/lottery-pools/${encodeURIComponent(poolId)}`)
}
export function saveAdminLotteryPool(payload: AdminLotteryPoolDetail) {
  return request<AdminLotteryPoolDetail>('/api/admin/lottery-pools', { method: 'POST', body: JSON.stringify(payload) })
}
export function deleteAdminLotteryPool(poolId: string) {
  return request<void>(`/api/admin/lottery-pools/${encodeURIComponent(poolId)}`, { method: 'DELETE' })
}

// ===== 抽奖奖项 =====
export function getAdminLotteryPrizes(poolId = '') {
  const query = poolId ? `?poolId=${encodeURIComponent(poolId)}` : ''
  return request<AdminLotteryPrizeListItem[]>(`/api/admin/lottery-prizes${query}`)
}
export function getAdminLotteryPrizeDetail(prizeId: string) {
  return request<AdminLotteryPrizeDetail>(`/api/admin/lottery-prizes/${encodeURIComponent(prizeId)}`)
}
export function saveAdminLotteryPrize(payload: AdminLotteryPrizeDetail) {
  return request<AdminLotteryPrizeDetail>('/api/admin/lottery-prizes', { method: 'POST', body: JSON.stringify(payload) })
}
export function deleteAdminLotteryPrize(prizeId: string) {
  return request<void>(`/api/admin/lottery-prizes/${encodeURIComponent(prizeId)}`, { method: 'DELETE' })
}

// ===== 抽奖日志 =====
export function getAdminLotteryLogs(playerId = '', poolId = '', page = 1, pageSize = 20) {
  const params = new URLSearchParams()
  if (playerId) params.set('playerId', playerId)
  if (poolId) params.set('poolId', poolId)
  params.set('page', String(page))
  params.set('pageSize', String(pageSize))
  return request<AdminPagedResult<AdminLotteryLogListItem>>(`/api/admin/lottery-logs?${params.toString()}`)
}
