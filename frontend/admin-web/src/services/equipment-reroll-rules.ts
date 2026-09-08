import type {
  AdminEquipmentEnhanceRule,
  AdminEquipmentRerollCostRule,
  AdminEquipmentRerollSystemConfig,
  AdminEquipmentRerollSlotPoolConfig,
  AdminEquipmentRerollTierConfig,
  AdminEquipmentRerollAttributeValueConfig
} from '@/types/admin'
import { request } from './http'

export function getEquipmentEnhanceRules() {
  return request<AdminEquipmentEnhanceRule[]>('/api/admin/equipment-reroll-rules/enhance-costs')
}

export function saveEquipmentEnhanceRule(payload: AdminEquipmentEnhanceRule) {
  return request<void>('/api/admin/equipment-reroll-rules/enhance-costs', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function deleteEquipmentEnhanceRule(gid: number) {
  return request<void>(`/api/admin/equipment-reroll-rules/enhance-costs/${gid}`, { method: 'DELETE' })
}

export function getEquipmentRerollCostRules() {
  return request<AdminEquipmentRerollCostRule[]>('/api/admin/equipment-reroll-rules/reroll-costs')
}

export function saveEquipmentRerollCostRule(payload: AdminEquipmentRerollCostRule) {
  return request<void>('/api/admin/equipment-reroll-rules/reroll-costs', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function deleteEquipmentRerollCostRule(gid: number) {
  return request<void>(`/api/admin/equipment-reroll-rules/reroll-costs/${gid}`, { method: 'DELETE' })
}

export function getEquipmentRerollSystemConfig() {
  return request<AdminEquipmentRerollSystemConfig>('/api/admin/equipment-reroll-rules/system')
}

export function saveEquipmentRerollSystemConfig(payload: AdminEquipmentRerollSystemConfig) {
  return request<AdminEquipmentRerollSystemConfig>('/api/admin/equipment-reroll-rules/system', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function getEquipmentRerollSlotPoolConfigs() {
  return request<AdminEquipmentRerollSlotPoolConfig[]>('/api/admin/equipment-reroll-rules/pools')
}

export function saveEquipmentRerollSlotPoolConfig(payload: AdminEquipmentRerollSlotPoolConfig) {
  return request<AdminEquipmentRerollSlotPoolConfig>('/api/admin/equipment-reroll-rules/pools', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function deleteEquipmentRerollSlotPoolConfig(gid: number) {
  return request<void>(`/api/admin/equipment-reroll-rules/pools/${gid}`, {
    method: 'DELETE'
  })
}

export function getEquipmentRerollTierConfigs() {
  return request<AdminEquipmentRerollTierConfig[]>('/api/admin/equipment-reroll-rules/tiers')
}

export function saveEquipmentRerollTierConfig(payload: AdminEquipmentRerollTierConfig) {
  return request<AdminEquipmentRerollTierConfig>('/api/admin/equipment-reroll-rules/tiers', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function deleteEquipmentRerollTierConfig(gid: number) {
  return request<void>(`/api/admin/equipment-reroll-rules/tiers/${gid}`, {
    method: 'DELETE'
  })
}

export function getEquipmentRerollAttributeValueConfigs() {
  return request<AdminEquipmentRerollAttributeValueConfig[]>('/api/admin/equipment-reroll-rules/attribute-values')
}

export function saveEquipmentRerollAttributeValueConfig(payload: AdminEquipmentRerollAttributeValueConfig) {
  return request<AdminEquipmentRerollAttributeValueConfig>('/api/admin/equipment-reroll-rules/attribute-values', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function deleteEquipmentRerollAttributeValueConfig(gid: number) {
  return request<void>(`/api/admin/equipment-reroll-rules/attribute-values/${gid}`, {
    method: 'DELETE'
  })
}
