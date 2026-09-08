import type {
  AdminSectTemplateListItem,
  AdminSectTemplateDetail,
  AdminHeartSutraListItem,
  AdminHeartSutraDetail,
  AdminSectBossTemplateListItem,
  AdminSectBossTemplateDetail,
  AdminSectTournamentSchedule,
  AdminSectShopItemListItem,
  AdminSectShopItemDetail,
  AdminSectBlessingListItem,
  AdminSectBlessingDetail,
  AdminGuildListItem,
  AdminGuildDetail
} from '@/types/admin'
import { request } from './http'

// ─── Sect Templates ──────────────────────────────────────────────

export function getSectTemplates() {
  return request<AdminSectTemplateListItem[]>('/api/admin/sect/templates')
}

export function getSectTemplate(id: string) {
  return request<AdminSectTemplateDetail>(`/api/admin/sect/templates/${encodeURIComponent(id)}`)
}

export function saveSectTemplate(payload: AdminSectTemplateDetail) {
  return request<AdminSectTemplateDetail>('/api/admin/sect/templates', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function deleteSectTemplate(id: string) {
  return request<void>(`/api/admin/sect/templates/${encodeURIComponent(id)}`, {
    method: 'DELETE'
  })
}

// ─── Heart Sutras ────────────────────────────────────────────────

export function getHeartSutras(sectId = '') {
  const query = sectId ? `?sectId=${encodeURIComponent(sectId)}` : ''
  return request<AdminHeartSutraListItem[]>(`/api/admin/sect/heart-sutras${query}`)
}

export function getHeartSutra(id: string) {
  return request<AdminHeartSutraDetail>(`/api/admin/sect/heart-sutras/${encodeURIComponent(id)}`)
}

export function saveHeartSutra(payload: AdminHeartSutraDetail) {
  return request<AdminHeartSutraDetail>('/api/admin/sect/heart-sutras', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function deleteHeartSutra(id: string) {
  return request<void>(`/api/admin/sect/heart-sutras/${encodeURIComponent(id)}`, {
    method: 'DELETE'
  })
}

// ─── Sect Boss Templates ────────────────────────────────────────

export function getSectBossTemplates() {
  return request<AdminSectBossTemplateListItem[]>('/api/admin/sect/boss-templates')
}

export function getSectBossTemplate(id: string) {
  return request<AdminSectBossTemplateDetail>(`/api/admin/sect/boss-templates/${encodeURIComponent(id)}`)
}

export function saveSectBossTemplate(payload: AdminSectBossTemplateDetail) {
  return request<AdminSectBossTemplateDetail>('/api/admin/sect/boss-templates', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function deleteSectBossTemplate(id: string) {
  return request<void>(`/api/admin/sect/boss-templates/${encodeURIComponent(id)}`, {
    method: 'DELETE'
  })
}

// ─── Tournament Schedule ────────────────────────────────────────

export function getTournamentSchedule() {
  return request<AdminSectTournamentSchedule>('/api/admin/sect/tournament-schedule')
}

export function saveTournamentSchedule(payload: AdminSectTournamentSchedule) {
  return request<AdminSectTournamentSchedule>('/api/admin/sect/tournament-schedule', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

// ─── Sect Shop Items ────────────────────────────────────────────

export function getSectShopItems(shopId = '') {
  const query = shopId ? `?shopId=${encodeURIComponent(shopId)}` : ''
  return request<AdminSectShopItemListItem[]>(`/api/admin/sect/shop-items${query}`)
}

export function getSectShopItem(id: string) {
  return request<AdminSectShopItemDetail>(`/api/admin/sect/shop-items/${encodeURIComponent(id)}`)
}

export function saveSectShopItem(payload: AdminSectShopItemDetail) {
  return request<AdminSectShopItemDetail>('/api/admin/sect/shop-items', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function deleteSectShopItem(id: string) {
  return request<void>(`/api/admin/sect/shop-items/${encodeURIComponent(id)}`, {
    method: 'DELETE'
  })
}

// ─── Sect Blessings ─────────────────────────────────────────────

export function getSectBlessings() {
  return request<AdminSectBlessingListItem[]>('/api/admin/sect/blessings')
}

export function getSectBlessing(id: string) {
  return request<AdminSectBlessingDetail>(`/api/admin/sect/blessings/${encodeURIComponent(id)}`)
}

export function saveSectBlessing(payload: AdminSectBlessingDetail) {
  return request<AdminSectBlessingDetail>('/api/admin/sect/blessings', {
    method: 'POST',
    body: JSON.stringify(payload)
  })
}

export function deleteSectBlessing(id: string) {
  return request<void>(`/api/admin/sect/blessings/${encodeURIComponent(id)}`, {
    method: 'DELETE'
  })
}

// ─── Guild Management ───────────────────────────────────────────

export function getGuilds() {
  return request<AdminGuildListItem[]>('/api/admin/sect/guilds')
}

export function getGuild(id: string) {
  return request<AdminGuildDetail>(`/api/admin/sect/guilds/${encodeURIComponent(id)}`)
}

export function deleteGuild(id: string) {
  return request<void>(`/api/admin/sect/guilds/${encodeURIComponent(id)}`, {
    method: 'DELETE'
  })
}
