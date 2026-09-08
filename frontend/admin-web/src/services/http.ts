const LOCAL_DEV_API_BASE_URL = 'http://127.0.0.1:5247'
const LOCAL_DEV_HOSTS = new Set(['localhost', '127.0.0.1'])
const LOCAL_DEV_PORTS = new Set(['3000', '3001', '3002', '3003', '3004', '5173', '5175', '5176', '5177', '5178'])
const ACCESS_TOKEN_KEY = 'admin-access-token'
const REFRESH_TOKEN_KEY = 'admin-refresh-token'

// 管理后台多个接口同时 401 时，共用一个刷新 Promise，
// 避免重复触发刷新令牌请求。
let refreshPromise: Promise<string> | null = null

// 统一解析后台 API 根地址。
// 本地开发默认直连后端；部署到同源环境时则保留相对路径。
function resolveApiBaseUrl() {
  const configuredBaseUrl = (import.meta.env.VITE_ADMIN_API_BASE_URL || '').trim().replace(/\/$/, '')
  if (configuredBaseUrl) {
    return configuredBaseUrl
  }

  if (typeof window === 'undefined') {
    return ''
  }

  const { hostname, port } = window.location
  if (LOCAL_DEV_HOSTS.has(hostname) && LOCAL_DEV_PORTS.has(port)) {
    return LOCAL_DEV_API_BASE_URL
  }

  return ''
}

const API_BASE_URL = resolveApiBaseUrl()

// 给相对路径补上 API 根地址。
function buildUrl(path: string) {
  if (/^https?:\/\//i.test(path) || !API_BASE_URL) {
    return path
  }

  return `${API_BASE_URL}${path}`
}

export function buildAdminApiUrl(path: string) {
  return buildUrl(path)
}

// 登录和刷新令牌接口本身不能再触发自动刷新，否则会形成死循环。
function shouldAttemptRefresh(path: string) {
  return !path.startsWith('/api/admin/auth/login') && !path.startsWith('/api/admin/auth/refresh')
}

function getAdminRefreshToken() {
  return localStorage.getItem(REFRESH_TOKEN_KEY) || ''
}

// 用自定义事件通知其他模块：管理员会话已经变化。
function notifyAdminSessionChanged() {
  if (typeof window !== 'undefined') {
    window.dispatchEvent(new CustomEvent('admin-session-changed'))
  }
}

function persistAdminTokenPair(accessToken: string, refreshToken: string) {
  localStorage.setItem(ACCESS_TOKEN_KEY, accessToken)
  localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken)
  notifyAdminSessionChanged()
}

function clearAdminTokenPair() {
  localStorage.removeItem(ACCESS_TOKEN_KEY)
  localStorage.removeItem(REFRESH_TOKEN_KEY)
  notifyAdminSessionChanged()
}

// 统一解析后端响应体。
async function parsePayload(response: Response) {
  const text = await response.text()
  if (!text) {
    return null
  }

  try {
    return JSON.parse(text)
  } catch {
    throw new Error('服务端返回了无法解析的响应内容。')
  }
}

async function refreshAdminAccessToken() {
  const accessToken = getAdminAccessToken()
  const refreshToken = getAdminRefreshToken()
  if (!accessToken || !refreshToken) {
    throw new Error('当前没有可用的管理员刷新令牌。')
  }

  const response = await fetch(buildUrl('/api/admin/auth/refresh'), {
    method: 'POST',
    headers: {
      'Accept': 'application/json',
      'Content-Type': 'application/json; charset=UTF-8'
    },
    body: JSON.stringify({ accessToken, refreshToken })
  })

  const payload = await parsePayload(response)
  if (!response.ok || payload?.success === false) {
    clearAdminTokenPair()
    throw new Error(payload?.message || `管理员令牌刷新失败（${response.status}）`)
  }

  const tokenPair = payload?.data ?? payload
  persistAdminTokenPair(tokenPair.accessToken, tokenPair.refreshToken)
  return tokenPair.accessToken as string
}

export function getAdminAccessToken() {
  return localStorage.getItem(ACCESS_TOKEN_KEY) || ''
}

// 管理后台统一请求入口。
// 负责自动带令牌、处理 401 自动刷新，并把 ApiResponse 解包成业务数据。
export async function request<T>(path: string, init?: RequestInit, allowRetry = true): Promise<T> {
  const headers = new Headers(init?.headers || {})
  headers.set('Accept', 'application/json')

  if (init?.body && !(init.body instanceof FormData) && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json; charset=UTF-8')
  }

  const accessToken = getAdminAccessToken()
  if (accessToken) {
    headers.set('Authorization', `Bearer ${accessToken}`)
  }

  const response = await fetch(buildUrl(path), {
    ...init,
    headers
  })

  const payload = await parsePayload(response)

  if (response.status === 401 && allowRetry && shouldAttemptRefresh(path) && getAdminRefreshToken()) {
    try {
      refreshPromise ??= refreshAdminAccessToken().finally(() => {
        refreshPromise = null
      })
      await refreshPromise
      return request<T>(path, init, false)
    } catch {
      clearAdminTokenPair()
      throw new Error(payload?.message || '管理员登录状态已失效，请重新登录。')
    }
  }

  if (!response.ok || payload?.success === false) {
    throw new Error(payload?.message || `请求失败（${response.status}）`)
  }

  return payload?.data ?? payload
}

