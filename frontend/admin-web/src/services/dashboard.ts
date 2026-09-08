import type { AdminDashboardSummary } from '@/types/admin'
import { request } from './http'

export function getDashboardSummary() {
  return request<AdminDashboardSummary>('/api/admin/dashboard/summary')
}
