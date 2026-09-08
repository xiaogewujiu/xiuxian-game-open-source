import type { AdminMailDetail, AdminMailListItem, AdminSendMailPayload } from '@/types/admin'
import { request } from './http'

export function getAdminMails(keyword = '', isGlobal?: boolean | null, take = 200) {
  const params = new URLSearchParams()
  if (keyword) params.set('keyword', keyword)
  if (isGlobal != null) params.set('isGlobal', String(isGlobal))
  params.set('take', String(take))
  const query = params.toString()
  return request<AdminMailListItem[]>(`/api/admin/mail?${query}`)
}

export function getAdminMailDetail(mailId: number) {
  return request<AdminMailDetail>(`/api/admin/mail/${mailId}`)
}

export function sendAdminMail(payload: AdminSendMailPayload) {
  return request<AdminMailDetail>('/api/admin/mail', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function deleteAdminMail(mailId: number) {
  return request<void>(`/api/admin/mail/${mailId}`, {
    method: 'DELETE'
  })
}

export function recallGlobalMail(mailId: number) {
  return request<void>(`/api/admin/mail/${mailId}/recall`, {
    method: 'POST'
  })
}
