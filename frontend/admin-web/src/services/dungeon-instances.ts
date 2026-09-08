import type {
  AdminDungeonInstanceTemplateListItem,
  AdminDungeonInstanceTemplateDetail,
  AdminDungeonEventConfigListItem,
  AdminDungeonEventConfigDetail,
  AdminDungeonEventGroupListItem,
  AdminDungeonEventGroupDetail
} from '@/types/admin'
import { request } from './http'

// ---- 秘境模板 ----
export function getDungeonInstanceTemplates(keyword = '') {
  const query = keyword ? `?keyword=${encodeURIComponent(keyword)}` : ''
  return request<AdminDungeonInstanceTemplateListItem[]>(`/api/admin/dungeon-instances/templates${query}`)
}

export function getDungeonInstanceTemplateDetail(dungeonId: string) {
  return request<AdminDungeonInstanceTemplateDetail>(`/api/admin/dungeon-instances/templates/${encodeURIComponent(dungeonId)}`)
}

export function saveDungeonInstanceTemplate(payload: AdminDungeonInstanceTemplateDetail) {
  return request<AdminDungeonInstanceTemplateDetail>('/api/admin/dungeon-instances/templates', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function deleteDungeonInstanceTemplate(dungeonId: string) {
  return request<void>(`/api/admin/dungeon-instances/templates/${encodeURIComponent(dungeonId)}`, {
    method: 'DELETE'
  })
}

// ---- 事件配置 ----
export function getDungeonEventConfigs(params: { dungeonId?: string; eventType?: number; keyword?: string } = {}) {
  const qs = new URLSearchParams()
  if (params.dungeonId) qs.set('dungeonId', params.dungeonId)
  if (params.eventType != null) qs.set('eventType', String(params.eventType))
  if (params.keyword) qs.set('keyword', params.keyword)
  const query = qs.toString() ? `?${qs.toString()}` : ''
  return request<AdminDungeonEventConfigListItem[]>(`/api/admin/dungeon-instances/events${query}`)
}

export function getDungeonEventConfigDetail(eventId: string) {
  return request<AdminDungeonEventConfigDetail>(`/api/admin/dungeon-instances/events/${encodeURIComponent(eventId)}`)
}

export function saveDungeonEventConfig(payload: AdminDungeonEventConfigDetail) {
  return request<AdminDungeonEventConfigDetail>('/api/admin/dungeon-instances/events', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function deleteDungeonEventConfig(eventId: string) {
  return request<void>(`/api/admin/dungeon-instances/events/${encodeURIComponent(eventId)}`, {
    method: 'DELETE'
  })
}

export function toggleDungeonEventConfig(eventId: string, enabled: boolean) {
  return request<void>(`/api/admin/dungeon-instances/events/${encodeURIComponent(eventId)}/toggle?enabled=${enabled}`, {
    method: 'PATCH'
  })
}

// ---- 事件组 ----
export function getDungeonEventGroups(keyword = '') {
  const query = keyword ? `?keyword=${encodeURIComponent(keyword)}` : ''
  return request<AdminDungeonEventGroupListItem[]>(`/api/admin/dungeon-instances/groups${query}`)
}

export function getDungeonEventGroupDetail(groupId: string) {
  return request<AdminDungeonEventGroupDetail>(`/api/admin/dungeon-instances/groups/${encodeURIComponent(groupId)}`)
}

export function saveDungeonEventGroup(payload: AdminDungeonEventGroupDetail) {
  return request<AdminDungeonEventGroupDetail>('/api/admin/dungeon-instances/groups', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function deleteDungeonEventGroup(groupId: string) {
  return request<void>(`/api/admin/dungeon-instances/groups/${encodeURIComponent(groupId)}`, {
    method: 'DELETE'
  })
}

// ---- 运行时 ----
export function reloadDungeonInstanceRuntime() {
  return request<void>('/api/admin/dungeon-instances/reload-runtime', { method: 'POST' })
}
