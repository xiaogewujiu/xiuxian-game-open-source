import type { AssetUploadResult, CollectionImageUploadResult } from '@/types/admin'
import { request } from './http'

export function uploadAdminAsset(category: string, file: File) {
  const formData = new FormData()
  formData.append('category', category)
  formData.append('file', file)

  return request<AssetUploadResult>('/api/admin/assets/upload', {
    method: 'POST',
    body: formData
  })
}

export function uploadCollectionImage(file: File) {
  const formData = new FormData()
  formData.append('file', file)

  return request<CollectionImageUploadResult>('/api/admin/assets/upload-collection-image', {
    method: 'POST',
    body: formData
  })
}

export function generateIcon(prompt: string, category: string, oldPath?: string | null) {
  return request<AssetUploadResult>('/api/admin/assets/generate-icon', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ prompt, category, oldPath: oldPath || null })
  })
}
