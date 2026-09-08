import type { AdminGemListItem, AdminGemDetail, AdminGemSavePayload, AdminGemBatchGeneratePayload } from '@/types/admin'
import { request } from './http'

export function getGemTemplates() {
  return request<AdminGemListItem[]>('/api/admin/gem/templates')
}

export function getGemDetail(id: number) {
  return request<AdminGemDetail>(`/api/admin/gem/templates/${id}`)
}

export function createGemTemplate(payload: AdminGemSavePayload) {
  return request<void>('/api/admin/gem/templates', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function updateGemTemplate(id: number, payload: AdminGemSavePayload) {
  return request<void>(`/api/admin/gem/templates/${id}`, {
    method: 'PUT',
    body: JSON.stringify(payload)
  })
}

export function deleteGemTemplate(id: number) {
  return request<void>(`/api/admin/gem/templates/${id}`, {
    method: 'DELETE'
  })
}

export function batchGenerateGems(payload: AdminGemBatchGeneratePayload) {
  return request<number>('/api/admin/gem/batch-generate', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}
