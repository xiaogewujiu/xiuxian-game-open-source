import type { AdminSkillDetail, AdminSkillListItem, AdminSkillNextLevel, AdminBuffDetail, AdminBuffListItem, SkillUpgradeCondition } from '@/types/admin'
import { request } from './http'

export function getAdminSkills(keyword = '', catalog = 'current') {
  const params = new URLSearchParams()
  if (keyword) params.set('keyword', keyword)
  if (catalog) params.set('catalog', catalog)
  const query = params.toString() ? `?${params.toString()}` : ''
  return request<AdminSkillListItem[]>(`/api/admin/skills${query}`)
}

export function getAdminSkillDetail(skillId: number) {
  return request<AdminSkillDetail>(`/api/admin/skills/${skillId}`)
}

export function saveAdminSkill(payload: AdminSkillDetail) {
  return request<AdminSkillDetail>('/api/admin/skills', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function deleteAdminSkill(skillId: number) {
  return request<void>(`/api/admin/skills/${skillId}`, {
    method: 'DELETE'
  })
}

export function getAdminSkillNextLevels(skillId: number) {
  return request<AdminSkillNextLevel[]>(`/api/admin/skills/${skillId}/next-levels`)
}

export function copyAdminSkillNext(skillId: number) {
  return request<AdminSkillDetail>(`/api/admin/skills/${skillId}/next/copy`, {
    method: 'POST'
  })
}

export function linkAdminSkillNext(skillId: number, nextSkillId: number, upgradeConditions: SkillUpgradeCondition[]) {
  return request<AdminSkillDetail>(`/api/admin/skills/${skillId}/next/link`, {
    method: 'POST',
    body: JSON.stringify({ nextSkillId, upgradeConditions })
  })
}

export function updateAdminSkillUpgradeConditions(skillId: number, conditions: SkillUpgradeCondition[]) {
  return request<AdminSkillDetail>(`/api/admin/skills/${skillId}/upgrade-conditions`, {
    method: 'PUT',
    body: JSON.stringify(conditions)
  })
}

export function deleteAdminSkillNextChain(skillId: number) {
  return request<void>(`/api/admin/skills/${skillId}/next-chain`, {
    method: 'DELETE'
  })
}

export function getAdminBuffs(keyword = '') {
  const query = keyword ? `?keyword=${encodeURIComponent(keyword)}` : ''
  return request<AdminBuffListItem[]>(`/api/admin/buffs${query}`)
}

export function getAdminBuffDetail(buffId: string) {
  return request<AdminBuffDetail>(`/api/admin/buffs/${encodeURIComponent(buffId)}`)
}

export function saveAdminBuff(payload: AdminBuffDetail) {
  return request<AdminBuffDetail>('/api/admin/buffs', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function deleteAdminBuff(buffId: string) {
  return request<void>(`/api/admin/buffs/${encodeURIComponent(buffId)}`, {
    method: 'DELETE'
  })
}
