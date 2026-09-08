import type { AdminRuntimeConfigDomainStatus, AdminRuntimeConfigRefreshResult } from '@/types/admin'
import { request } from './http'

// 获取所有运行时配置域的当前状态。
export function getAdminRuntimeConfigs() {
  return request<AdminRuntimeConfigDomainStatus[]>('/api/admin/runtime-configs')
}

// 刷新指定运行时配置域。
export function refreshAdminRuntimeConfig(domain: string) {
  return request<AdminRuntimeConfigRefreshResult>(`/api/admin/runtime-configs/refresh/${encodeURIComponent(domain)}`, {
    method: 'POST'
  })
}

// 刷新全部运行时配置域。
export function refreshAllAdminRuntimeConfigs() {
  return request<AdminRuntimeConfigRefreshResult[]>('/api/admin/runtime-configs/refresh-all', {
    method: 'POST'
  })
}
