import type { AdminForgeRecipeDetail, AdminForgeRecipeListItem } from '@/types/admin'
import { request } from './http'

// 获取锻造配方列表，可按关键字筛选。
export function getAdminForgeRecipes(keyword = '') {
  const query = keyword ? `?keyword=${encodeURIComponent(keyword)}` : ''
  return request<AdminForgeRecipeListItem[]>(`/api/admin/forge-recipes${query}`)
}

// 获取单个锻造配方详情。
export function getAdminForgeRecipeDetail(recipeId: string) {
  return request<AdminForgeRecipeDetail>(`/api/admin/forge-recipes/${encodeURIComponent(recipeId)}`)
}

// 保存锻造配方。
export function saveAdminForgeRecipe(payload: AdminForgeRecipeDetail) {
  return request<AdminForgeRecipeDetail>('/api/admin/forge-recipes', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

// 删除锻造配方。
export function deleteAdminForgeRecipe(recipeId: string) {
  return request<void>(`/api/admin/forge-recipes/${encodeURIComponent(recipeId)}`, {
    method: 'DELETE'
  })
}
