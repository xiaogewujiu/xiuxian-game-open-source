import type { AdminAuditLogDetail, AdminAuditLogListItem } from '@/types/admin'
import { request } from './http'

export function getAdminAuditLogs(keyword = '', take = 200, success?: boolean | null) {
  const params = new URLSearchParams()
  if (keyword) {
    params.set('keyword', keyword)
  }
  params.set('take', String(take))
  if (success != null) params.set('success', String(success))
  return request<AdminAuditLogListItem[]>(`/api/admin/audit-logs?${params.toString()}`)
}

export function getAdminAuditLogDetail(logId: string) {
  return request<AdminAuditLogDetail>(`/api/admin/audit-logs/${encodeURIComponent(logId)}`)
}
