import type {
  AdminSpiritFieldDetail,
  AdminSpiritFieldListItem,
  AdminSpiritFieldSpeedUpItemRule,
  AdminSpiritFieldSystemRule
} from '@/types/admin'
import { request } from './http'

export function getAdminSpiritFields(keyword = '') {
  const query = keyword ? `?keyword=${encodeURIComponent(keyword)}` : ''
  return request<AdminSpiritFieldListItem[]>(`/api/admin/spirit-fields${query}`)
}

export function getAdminSpiritFieldDetail(playerId: string) {
  return request<AdminSpiritFieldDetail>(`/api/admin/spirit-fields/${encodeURIComponent(playerId)}`)
}

export function saveAdminSpiritField(payload: AdminSpiritFieldDetail) {
  return request<AdminSpiritFieldDetail>('/api/admin/spirit-fields', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function getAdminSpiritFieldSystemRule() {
  return request<AdminSpiritFieldSystemRule>('/api/admin/spirit-field-rules/system')
}

export function saveAdminSpiritFieldSystemRule(payload: AdminSpiritFieldSystemRule) {
  return request<AdminSpiritFieldSystemRule>('/api/admin/spirit-field-rules/system', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function getAdminSpiritFieldSpeedUpRules() {
  return request<AdminSpiritFieldSpeedUpItemRule[]>('/api/admin/spirit-field-rules/speedup-items')
}

export function getAdminSpiritFieldSpeedUpRule(itemId: string) {
  return request<AdminSpiritFieldSpeedUpItemRule>(`/api/admin/spirit-field-rules/speedup-items/${encodeURIComponent(itemId)}`)
}

export function saveAdminSpiritFieldSpeedUpRule(payload: AdminSpiritFieldSpeedUpItemRule) {
  return request<AdminSpiritFieldSpeedUpItemRule>('/api/admin/spirit-field-rules/speedup-items', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function deleteAdminSpiritFieldSpeedUpRule(itemId: string) {
  return request<void>(`/api/admin/spirit-field-rules/speedup-items/${encodeURIComponent(itemId)}`, {
    method: 'DELETE'
  })
}
