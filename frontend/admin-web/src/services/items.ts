import type { AdminItemDetail, AdminItemListItem } from '@/types/admin'
import { request } from './http'

// 获取后台道具列表，可按关键字筛选。
export function getAdminItems(keyword = '', type?: number | null) {
  const params = new URLSearchParams()
  if (keyword) params.set('keyword', keyword)
  if (type != null) params.set('type', String(type))
  const query = params.toString()
  return request<AdminItemListItem[]>(`/api/admin/items${query ? `?${query}` : ''}`)
}

// 获取单个道具详情。
export function getAdminItemDetail(itemId: string) {
  return request<AdminItemDetail>(`/api/admin/items/${encodeURIComponent(itemId)}`)
}

// 保存道具配置。
export function saveAdminItem(payload: AdminItemDetail) {
  return request<AdminItemDetail>('/api/admin/items', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

// 删除道具配置。
export function deleteAdminItem(itemId: string) {
  return request<void>(`/api/admin/items/${encodeURIComponent(itemId)}`, {
    method: 'DELETE'
  })
}
