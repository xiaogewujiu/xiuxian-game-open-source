import type { FeedbackDetail, FeedbackListItem, ProcessFeedbackPayload } from '@/types/admin'
import { request } from './http'

export function getAdminFeedbackList(keyword = '', type = '', status = '', pageIndex = 1, pageSize = 20) {
  const params = new URLSearchParams({ pageIndex: String(pageIndex), pageSize: String(pageSize) })
  if (keyword) params.set('keyword', keyword)
  if (type) params.set('type', type)
  if (status) params.set('status', status)
  return request<{ items: FeedbackListItem[]; total: number; page: number; pageSize: number }>(`/api/admin/feedback?${params}`)
}

export function getAdminFeedbackDetail(id: number) {
  return request<FeedbackDetail>(`/api/admin/feedback/${id}`)
}

export function processAdminFeedback(id: number, payload: ProcessFeedbackPayload) {
  return request<FeedbackDetail>(`/api/admin/feedback/${id}/process`, { method: 'POST', body: JSON.stringify(payload) })
}
