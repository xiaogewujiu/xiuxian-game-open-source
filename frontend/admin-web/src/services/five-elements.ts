import type {
  AdminFiveElementBranchRuleDetail,
  AdminFiveElementBranchRuleListItem,
  AdminFiveElementBranchRuleRangeDetail,
  AdminFiveElementBranchRuleRangeListItem,
  AdminFiveElementDetail,
  AdminFiveElementLevelRuleDetail,
  AdminFiveElementLevelRuleListItem,
  AdminFiveElementListItem
} from '@/types/admin'
import { request } from './http'

export function getAdminFiveElements(keyword = '') {
  const query = keyword ? `?keyword=${encodeURIComponent(keyword)}` : ''
  return request<AdminFiveElementListItem[]>(`/api/admin/five-elements${query}`)
}

export function getAdminFiveElementDetail(playerId: string) {
  return request<AdminFiveElementDetail>(`/api/admin/five-elements/${encodeURIComponent(playerId)}`)
}

export function saveAdminFiveElement(payload: AdminFiveElementDetail) {
  return request<AdminFiveElementDetail>('/api/admin/five-elements', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function getAdminFiveElementLevelRules() {
  return request<AdminFiveElementLevelRuleListItem[]>('/api/admin/five-element-rules/levels')
}

export function getAdminFiveElementLevelRuleDetail(arrayLevel: number) {
  return request<AdminFiveElementLevelRuleDetail>(`/api/admin/five-element-rules/levels/${arrayLevel}`)
}

export function saveAdminFiveElementLevelRule(payload: AdminFiveElementLevelRuleDetail) {
  return request<AdminFiveElementLevelRuleDetail>('/api/admin/five-element-rules/levels', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function deleteAdminFiveElementLevelRule(arrayLevel: number) {
  return request<void>(`/api/admin/five-element-rules/levels/${arrayLevel}`, {
    method: 'DELETE'
  })
}

export function getAdminFiveElementBranchRuleRanges(elementType = '') {
  const query = elementType ? `?elementType=${encodeURIComponent(elementType)}` : ''
  return request<AdminFiveElementBranchRuleRangeListItem[]>(`/api/admin/five-element-rules/ranges${query}`)
}

export function getAdminFiveElementBranchRuleRangeDetail(gid: string) {
  return request<AdminFiveElementBranchRuleRangeDetail>(`/api/admin/five-element-rules/ranges/${encodeURIComponent(gid)}`)
}

export function saveAdminFiveElementBranchRuleRange(payload: AdminFiveElementBranchRuleRangeDetail) {
  return request<AdminFiveElementBranchRuleRangeDetail>('/api/admin/five-element-rules/ranges', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function deleteAdminFiveElementBranchRuleRange(gid: string) {
  return request<void>(`/api/admin/five-element-rules/ranges/${encodeURIComponent(gid)}`, {
    method: 'DELETE'
  })
}

export function getAdminFiveElementBranchRules(elementType = '') {
  const query = elementType ? `?elementType=${encodeURIComponent(elementType)}` : ''
  return request<AdminFiveElementBranchRuleListItem[]>(`/api/admin/five-element-rules/branches${query}`)
}

export function getAdminFiveElementBranchRuleDetail(gid: string) {
  return request<AdminFiveElementBranchRuleDetail>(`/api/admin/five-element-rules/branches/${encodeURIComponent(gid)}`)
}

export function saveAdminFiveElementBranchRule(payload: AdminFiveElementBranchRuleDetail) {
  return request<AdminFiveElementBranchRuleDetail>('/api/admin/five-element-rules/branches', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function deleteAdminFiveElementBranchRule(gid: string) {
  return request<void>(`/api/admin/five-element-rules/branches/${encodeURIComponent(gid)}`, {
    method: 'DELETE'
  })
}
