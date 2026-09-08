import type { AdminShopConfigDetail, AdminShopConfigListItem, AdminShopItemDetail, AdminShopItemListItem } from '@/types/admin'
import { request } from './http'

// 获取商店配置列表，可按关键字筛选。
export function getAdminShopConfigs(keyword = '') {
  const query = keyword ? `?keyword=${encodeURIComponent(keyword)}` : ''
  return request<AdminShopConfigListItem[]>(`/api/admin/shop-configs${query}`)
}

// 获取单个商店配置详情。
export function getAdminShopConfigDetail(shopId: string) {
  return request<AdminShopConfigDetail>(`/api/admin/shop-configs/${encodeURIComponent(shopId)}`)
}

// 保存商店配置。
export function saveAdminShopConfig(payload: AdminShopConfigDetail) {
  return request<AdminShopConfigDetail>('/api/admin/shop-configs', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

// 删除商店配置。
export function deleteAdminShopConfig(shopId: string) {
  return request<void>(`/api/admin/shop-configs/${encodeURIComponent(shopId)}`, {
    method: 'DELETE'
  })
}

// 获取商店商品列表；可选按商店编号筛选。
export function getAdminShopItems(shopId = '') {
  const query = shopId ? `?shopId=${encodeURIComponent(shopId)}` : ''
  return request<AdminShopItemListItem[]>(`/api/admin/shop-items${query}`)
}

// 获取单个商店商品详情。
export function getAdminShopItemDetail(gid: string) {
  return request<AdminShopItemDetail>(`/api/admin/shop-items/${encodeURIComponent(gid)}`)
}

// 保存商店商品配置。
export function saveAdminShopItem(payload: AdminShopItemDetail) {
  return request<AdminShopItemDetail>('/api/admin/shop-items', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

// 删除商店商品配置。
export function deleteAdminShopItem(gid: string) {
  return request<void>(`/api/admin/shop-items/${encodeURIComponent(gid)}`, {
    method: 'DELETE'
  })
}
