import type { AdminCurrentUser, AdminLoginResponse } from '@/types/admin'
import { request } from './http'

// 管理员登录。
export function adminLogin(account: string, password: string) {
  return request<AdminLoginResponse>('/api/admin/auth/login', {
    method: 'POST',
    body: JSON.stringify({ account, password })
  })
}

// 主动刷新管理员访问令牌。
export function adminRefresh(accessToken: string, refreshToken: string) {
  return request<AdminLoginResponse>('/api/admin/auth/refresh', {
    method: 'POST',
    body: JSON.stringify({ accessToken, refreshToken })
  })
}

// 获取当前登录管理员信息。
export function adminMe() {
  return request<AdminCurrentUser>('/api/admin/auth/me')
}

// 管理员登出。
export function adminLogout(refreshToken: string) {
  return request<void>('/api/admin/auth/logout', {
    method: 'POST',
    body: JSON.stringify({ accessToken: '', refreshToken })
  })
}
