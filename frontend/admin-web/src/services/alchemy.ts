import type { AdminAlchemyRecipeDetail, AdminAlchemyRecipeListItem } from '@/types/admin'
import { request } from './http'

// 获取炼丹配方列表，可按关键字筛选。
export function getAdminAlchemyRecipes(keyword = '') {
  const query = keyword ? `?keyword=${encodeURIComponent(keyword)}` : ''
  return request<AdminAlchemyRecipeListItem[]>(`/api/admin/alchemy-recipes${query}`)
}

// 获取单个炼丹配方详情。
export function getAdminAlchemyRecipeDetail(recipeId: string) {
  return request<AdminAlchemyRecipeDetail>(`/api/admin/alchemy-recipes/${encodeURIComponent(recipeId)}`)
}

// 保存炼丹配方。
export function saveAdminAlchemyRecipe(payload: AdminAlchemyRecipeDetail) {
  return request<AdminAlchemyRecipeDetail>('/api/admin/alchemy-recipes', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

// 删除炼丹配方。
export function deleteAdminAlchemyRecipe(recipeId: string) {
  return request<void>(`/api/admin/alchemy-recipes/${encodeURIComponent(recipeId)}`, {
    method: 'DELETE'
  })
}
