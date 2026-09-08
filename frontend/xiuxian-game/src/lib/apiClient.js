/**
 * 中文注释：
 * 这是新的前端 API 封装入口。
 * 之所以新增这个文件，而不是继续改旧的 `src/services/api.js`，
 * 是因为当前沙箱里的补丁工具对 `src/services` 目录存在稳定异常，
 * 会反复触发 `Failed to apply patch`。
 * 把后续真实联调所依赖的 API 能力迁到这个可正常维护的目录后，
 * 后续改动就不会再被同一个工具问题阻塞。
 */

const LOCAL_DEV_API_BASE_URL = 'http://127.0.0.1:5247'
const LOCAL_DEV_HOSTS = new Set(['localhost', '127.0.0.1'])
const LOCAL_DEV_PORTS = new Set(['3000', '3001', '3002', '3003', '3004', '5173', '5175', '5176', '5177', '5178'])
const ACCESS_TOKEN_KEY = 'xiuxian-access-token'
const REFRESH_TOKEN_KEY = 'xiuxian-refresh-token'

// 本地开发默认直连后端；部署成同源站点时则回退为相对路径。
function resolveApiBaseUrl() {
  const configuredBaseUrl = (import.meta.env.VITE_API_BASE_URL || '').trim().replace(/\/$/, '')
  if (configuredBaseUrl) {
    return configuredBaseUrl
  }

  if (typeof window === 'undefined') {
    return ''
  }

  const { hostname, port } = window.location

  // 中文注释：
  // 当前联调明确要求前端直接访问后端，不再通过 Vite 代理中转。
  // 因此本地开发端口下默认直连 127.0.0.1:5247；若以后部署成同源站点，再自然回退为相对路径。
  if (LOCAL_DEV_HOSTS.has(hostname) && LOCAL_DEV_PORTS.has(port)) {
    return LOCAL_DEV_API_BASE_URL
  }

  return ''
}

const API_BASE_URL = resolveApiBaseUrl()

let accessToken = localStorage.getItem(ACCESS_TOKEN_KEY) || ''
let refreshPromise = null
let onUnauthorized = null

// 给相对路径补上 API 根地址。
function buildUrl(path) {
  if (/^https?:\/\//i.test(path)) {
    return path
  }

  if (!API_BASE_URL) {
    return path
  }

  return `${API_BASE_URL}${path}`
}

function shouldAttemptRefresh(path) {
  return !path.startsWith('/api/auth/login') && !path.startsWith('/api/auth/register') && !path.startsWith('/api/auth/refresh')
}

// 把后端错误统一包装成 Error，并附带原始 payload，方便页面继续读取详细信息。
function createApiError(payload, fallbackMessage) {
  const error = new Error(payload?.message || fallbackMessage)
  error.payload = payload || null
  error.data = payload?.data ?? null
  error.code = payload?.code
  return error
}

function getRefreshToken() {
  return localStorage.getItem(REFRESH_TOKEN_KEY) || ''
}

// 用自定义事件通知全局状态仓库：登录态已经变化。
function notifySessionChanged() {
  if (typeof window !== 'undefined') {
    window.dispatchEvent(new CustomEvent('xiuxian-session-changed'))
  }
}

function persistTokenPair(nextAccessToken, nextRefreshToken) {
  accessToken = nextAccessToken || ''
  localStorage.setItem(ACCESS_TOKEN_KEY, accessToken)
  localStorage.setItem(REFRESH_TOKEN_KEY, nextRefreshToken || '')
  notifySessionChanged()
}

function clearPersistedTokens() {
  accessToken = ''
  localStorage.removeItem(ACCESS_TOKEN_KEY)
  localStorage.removeItem(REFRESH_TOKEN_KEY)
  notifySessionChanged()
}

// 统一解析后端响应体。
async function parsePayload(response) {
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

async function refreshAccessToken() {
  const currentAccessToken = getAccessToken()
  const currentRefreshToken = getRefreshToken()
  if (!currentAccessToken || !currentRefreshToken) {
    throw new Error('当前没有可用的刷新令牌。')
  }

  const response = await fetch(buildUrl('/api/auth/refresh'), {
    method: 'POST',
    headers: {
      'Accept': 'application/json',
      'Content-Type': 'application/json; charset=UTF-8'
    },
    body: JSON.stringify({
      accessToken: currentAccessToken,
      refreshToken: currentRefreshToken
    })
  })

  const payload = await parsePayload(response)
  if (!response.ok || payload?.success === false) {
    clearPersistedTokens()
    throw new Error(payload?.message || `令牌刷新失败（${response.status}）`)
  }

  const tokenPair = payload?.data ?? payload
  persistTokenPair(tokenPair.accessToken || tokenPair.AccessToken || '', tokenPair.refreshToken || tokenPair.RefreshToken || '')
  return getAccessToken()
}

export function buildApiUrl(path) {
  return buildUrl(path)
}

export function setAccessToken(token) {
  accessToken = token || ''
}

export function getAccessToken() {
  return accessToken || localStorage.getItem(ACCESS_TOKEN_KEY) || ''
}

export function setUnauthorizedHandler(handler) {
  onUnauthorized = handler
}

// 游戏端统一请求入口。
// 负责自动带令牌、处理 401 自动刷新，并把 ApiResponse 解包成业务数据。
async function request(path, options = {}, allowRetry = true) {
  const headers = new Headers(options.headers || {})

  // 中文注释：
  // 所有带请求体的接口统一按 UTF-8 JSON 发出，尽量降低中文角色名、聊天内容出现编码异常的概率。
  if (!headers.has('Content-Type') && options.body && !(options.body instanceof FormData)) {
    headers.set('Content-Type', 'application/json; charset=UTF-8')
  }

  headers.set('Accept', 'application/json')

  const currentAccessToken = getAccessToken()
  if (currentAccessToken) {
    headers.set('Authorization', `Bearer ${currentAccessToken}`)
  }

  const response = await fetch(buildUrl(path), {
    ...options,
    headers
  })

  const payload = await parsePayload(response)

  if (response.status === 401 && allowRetry && shouldAttemptRefresh(path) && getRefreshToken()) {
    try {
      refreshPromise ??= refreshAccessToken().finally(() => {
        refreshPromise = null
      })
      await refreshPromise
      return request(path, options, false)
    } catch {
      if (typeof onUnauthorized === 'function') {
        onUnauthorized()
      }

      throw new Error(payload?.message || '登录状态已失效，请重新登录。')
    }
  }

  if (response.status === 401) {
    if (typeof onUnauthorized === 'function') {
      onUnauthorized()
    }

    throw createApiError(payload, '登录状态已失效，请重新登录。')
  }

  if (!response.ok) {
    throw createApiError(payload, `请求失败（${response.status}）`)
  }

  if (payload && payload.success === false) {
    throw createApiError(payload, '业务请求失败。')
  }

  return payload?.data ?? payload
}
export const apiClient = {
  login(data) {
    return request('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify(data)
    })
  },
  register(data) {
    return request('/api/auth/register', {
      method: 'POST',
      body: JSON.stringify(data)
    })
  },
  logout() {
    return request('/api/auth/logout', {
      method: 'POST',
      body: JSON.stringify({
        accessToken: getAccessToken(),
        refreshToken: getRefreshToken()
      })
    })
  },
  getFeedbackList() {
    return request('/api/feedback')
  },
  getFeedbackDetail(feedbackId) {
    return request(`/api/feedback/${feedbackId}`)
  },
  uploadFeedbackAttachment(file) {
    const formData = new FormData()
    formData.append('file', file)
    return request('/api/feedback/attachments', {
      method: 'POST',
      body: formData
    })
  },
  deleteFeedbackAttachment(relativePath) {
    return request('/api/feedback/attachments', {
      method: 'DELETE',
      body: JSON.stringify({ relativePath })
    })
  },
  createFeedback(data) {
    return request('/api/feedback', {
      method: 'POST',
      body: JSON.stringify(data)
    })
  },
  getMaps() {
    return request('/api/battle/maps')
  },
  getCurrentPlayer() {
    return request('/api/player/me')
  },
  getEquipmentAutoSellSettings() {
    return request('/api/player/me/equipment-auto-sell')
  },
  updateEquipmentAutoSellSettings(data) {
    return request('/api/player/me/equipment-auto-sell', {
      method: 'PUT',
      body: JSON.stringify(data)
    })
  },
  uploadPlayerAvatar(file) {
    const formData = new FormData()
    formData.append('file', file)
    return request('/api/player/me/avatar', {
      method: 'POST',
      body: formData
    })
  },
  breakthrough() {
    return request('/api/player/me/breakthrough', {
      method: 'POST'
    })
  },
  getPillEffects() {
    return request('/api/player/me/pill-effects')
  },
  getAttributePoints() {
    return request('/api/player/me/attribute-points')
  },
  getAvailableQuests() {
    return request('/api/quest/available')
  },
  getAvailableQuestOverview() {
    return request('/api/quest/available-overview')
  },
  getActiveQuests() {
    return request('/api/quest/active')
  },
  getActiveQuestOverview() {
    return request('/api/quest/active-overview')
  },
  getCompletedQuests() {
    return request('/api/quest/completed')
  },
  getCompletedQuestOverview() {
    return request('/api/quest/completed-overview')
  },
  getQuestDetail(questId) {
    return request(`/api/quest/${encodeURIComponent(questId)}`)
  },
  acceptQuest(questId) {
    return request('/api/quest/accept', {
      method: 'POST',
      body: JSON.stringify({ questId })
    })
  },
  submitQuest(questId) {
    return request('/api/quest/submit', {
      method: 'POST',
      body: JSON.stringify({ questId })
    })
  },
  abandonQuest(questId) {
    return request(`/api/quest/${encodeURIComponent(questId)}`, {
      method: 'DELETE'
    })
  },
  allocateAttributePoint(attributeKey) {
    return request('/api/player/me/attribute-points', {
      method: 'POST',
      body: JSON.stringify({ attributeKey })
    })
  },
  refundAttributePoint(attributeKey) {
    return request('/api/player/me/attribute-points', {
      method: 'DELETE',
      body: JSON.stringify({ attributeKey })
    })
  },
  startBattle(data) {
    return request('/api/battle', {
      method: 'POST',
      body: JSON.stringify(data)
    })
  },
  startOfflineBattle(mapId) {
    return request('/api/battle/offline/start', {
      method: 'POST',
      body: JSON.stringify({ mapId })
    })
  },
  stopOfflineBattle() {
    return request('/api/battle/offline/stop', {
      method: 'POST'
    })
  },
  getOfflineBattleStatus() {
    return request('/api/battle/offline/status')
  },
  getDungeons() {
    return request('/api/battle/dungeons')
  },
  challengeDungeon(dungeonId) {
    return request(`/api/battle/dungeon/${dungeonId}`, {
      method: 'POST'
    })
  },
  challengePartyDungeon(dungeonId, partyId, requestId = null) {
    return request(`/api/battle/dungeon/${encodeURIComponent(dungeonId)}/party/${encodeURIComponent(partyId)}`, {
      method: 'POST',
      body: JSON.stringify(requestId ? { requestId } : {})
    })
  },
  getPartyBattleRecord(battleId) {
    return request(`/api/battle/party-record/${encodeURIComponent(battleId)}`)
  },
  // ---- 秘境实例 ----
  getAvailableDungeonInstances() {
    return request('/api/dungeoninstance/available')
  },
  enterDungeonInstance(dungeonId) {
    return request('/api/dungeoninstance/enter', {
      method: 'POST',
      body: JSON.stringify({ dungeonId })
    })
  },
  getDungeonInstanceStatus() {
    return request('/api/dungeoninstance/status')
  },
  quitDungeonInstance() {
    return request('/api/dungeoninstance/quit', { method: 'POST' })
  },
  getDungeonInstanceHistory(limit = 20) {
    return request(`/api/dungeoninstance/history?limit=${limit}`)
  },
  getWorldBossCurrent() {
    return request('/api/world-boss/current')
  },
  joinWorldBoss() {
    return request('/api/world-boss/join', {
      method: 'POST'
    })
  },
  getWorldBossRanking(top = 10) {
    return request(`/api/world-boss/ranking?top=${top}`)
  },
  getWorldBossLogs(count = 100) {
    return request(`/api/world-boss/logs?count=${count}`)
  },
  worldBossAction(actionType = 'normal', skillId = null) {
    return request('/api/world-boss/action', {
      method: 'POST',
      body: JSON.stringify({ actionType, skillId })
    })
  },
  setWorldBossAuto(enabled) {
    return request('/api/world-boss/auto', {
      method: 'POST',
      body: JSON.stringify({ enabled })
    })
  },
  claimWorldBossReward() {
    return request('/api/world-boss/claim', {
      method: 'POST'
    })
  },
  getParties() {
    return request('/api/team')
  },
  getGuilds() {
    return request('/api/guild')
  },
  createGuild(name) {
    return request('/api/guild', {
      method: 'POST',
      body: JSON.stringify({ name })
    })
  },
  joinGuild(guildId) {
    return request(`/api/guild/join/${encodeURIComponent(guildId)}`, {
      method: 'POST'
    })
  },
  leaveGuild() {
    return request('/api/guild/leave', {
      method: 'POST'
    })
  },
  getCurrentParty() {
    return request('/api/team/me')
  },
  createParty(data) {
    return request('/api/team', {
      method: 'POST',
      body: JSON.stringify(data)
    })
  },
  joinParty(partyId) {
    return request(`/api/team/${encodeURIComponent(partyId)}/join`, {
      method: 'POST'
    })
  },
  leaveCurrentParty() {
    return request('/api/team/me/leave', {
      method: 'POST'
    })
  },
  togglePartyRecruiting(partyId) {
    return request(`/api/team/${encodeURIComponent(partyId)}/recruiting`, {
      method: 'POST'
    })
  },
  dismissParty(partyId) {
    return request(`/api/team/${encodeURIComponent(partyId)}`, {
      method: 'DELETE'
    })
  },
  getInventory() {
    return request('/api/inventory')
  },
  useItem(itemId, quantity = 1) {
    return request('/api/inventory/use', {
      method: 'POST',
      body: JSON.stringify({ itemId, quantity })
    })
  },
  decomposeSkillBook(itemId, quantity = 1) {
    return request('/api/inventory/skill-books/decompose', {
      method: 'POST',
      body: JSON.stringify({ itemId, quantity })
    })
  },
  discardItem(itemId, quantity = 1) {
    return request('/api/inventory/discard', {
      method: 'POST',
      body: JSON.stringify({ itemId, quantity })
    })
  },
  discardItemsBatch(items = []) {
    return request('/api/inventory/discard-batch', {
      method: 'POST',
      body: JSON.stringify({ items })
    })
  },
  getEquipment() {
    return request('/api/equipment')
  },
  getEquipped() {
    return request('/api/equipment/equipped')
  },
  getEquipmentCompare(equipmentId) {
    return request(`/api/equipment/compare/${encodeURIComponent(equipmentId)}`)
  },
  equipItem(equipmentId) {
    return request('/api/equipment/equip', {
      method: 'POST',
      body: JSON.stringify({ equipmentId })
    })
  },
  unequipItem(slot) {
    return request('/api/equipment/unequip', {
      method: 'POST',
      body: JSON.stringify({ slot })
    })
  },
  sellEquipment(equipmentId) {
    return request(`/api/equipment/sell/${equipmentId}`, {
      method: 'POST'
    })
  },
  sellEquipmentsBatch(equipmentIds = []) {
    return request('/api/equipment/sell-batch', {
      method: 'POST',
      body: JSON.stringify({ equipmentIds })
    })
  },
  decomposeEquipments(equipmentIds = []) {
    return request('/api/equipment/decompose', {
      method: 'POST',
      body: JSON.stringify({ equipmentIds })
    })
  },
  enhanceEquipment(equipmentId, materials = []) {
    return request('/api/equipment/enhance', {
      method: 'POST',
      body: JSON.stringify({ equipmentId, materials })
    })
  },
  bindEquipment(equipmentId) {
    return request('/api/equipment/bind', {
      method: 'POST',
      body: JSON.stringify({ equipmentId })
    })
  },
  unlockEquipment(equipmentId) {
    return request('/api/equipment/unlock', {
      method: 'POST',
      body: JSON.stringify({ equipmentId })
    })
  },
  lockItem(itemId) {
    return request('/api/inventory/lock', {
      method: 'POST',
      body: JSON.stringify({ itemId })
    })
  },
  unlockItem(itemId) {
    return request('/api/inventory/unlock', {
      method: 'POST',
      body: JSON.stringify({ itemId })
    })
  },
  donateEquipment(equipmentId) {
    return request(`/api/equipment/donate/${equipmentId}`, {
      method: 'POST'
    })
  },
  getEquipmentRerollPreview(equipmentId) {
    return request(`/api/equipment/reroll/preview/${equipmentId}`)
  },
  rollEquipmentReroll(equipmentId, lockedIndices = []) {
    return request('/api/equipment/reroll/roll', {
      method: 'POST',
      body: JSON.stringify({ equipmentId, lockedIndices })
    })
  },
  acceptEquipmentReroll(equipmentId) {
    return request('/api/equipment/reroll/accept', {
      method: 'POST',
      body: JSON.stringify({ equipmentId })
    })
  },
  discardEquipmentReroll(equipmentId) {
    return request('/api/equipment/reroll/discard', {
      method: 'POST',
      body: JSON.stringify({ equipmentId })
    })
  },
  getForgeOverview() {
    return request('/api/forge')
  },
  forgeEquipment(recipeId) {
    return request('/api/forge', {
      method: 'POST',
      body: JSON.stringify({ recipeId })
    })
  },
  collectForgeResult() {
    return request('/api/forge/collect', {
      method: 'POST'
    })
  },
  getShops() {
    return request('/api/shop')
  },
  buyItem(shopId, itemId, count = 1) {
    return request('/api/shop/buy', {
      method: 'POST',
      body: JSON.stringify({ shopId, itemId, count })
    })
  },
  sellInventoryItem(itemId, count = 1) {
    return request('/api/shop/sell', {
      method: 'POST',
      body: JSON.stringify({ itemId, count })
    })
  },
  sellInventoryItemsBatch(items = []) {
    return request('/api/shop/sell-batch', {
      method: 'POST',
      body: JSON.stringify({ items })
    })
  },
  getRanking(rankingId = 'ranking_level', topCount = 10) {
    return request(`/api/ranking/${rankingId}?topCount=${topCount}`)
  },
  // 中文注释：
  // 技能总览接口只负责把“角色当前真实技能”和“后端技能模板库”一起返回，
  // 前端技能弹窗据此展示描述、Buff、耗蓝和冷却，不再依赖本地写死的数据。
  getSkillOverview() {
    return request('/api/skill/me')
  },
  upgradeSkill(skillId) {
    return request('/api/skill/upgrade', {
      method: 'POST',
      body: JSON.stringify({ skillId })
    })
  },
  equipSkill(skillId) {
    return request('/api/skill/equip', {
      method: 'POST',
      body: JSON.stringify({ skillId })
    })
  },
  unequipSkill(skillId) {
    return request('/api/skill/unequip', {
      method: 'POST',
      body: JSON.stringify({ skillId })
    })
  },
  // 中文注释：
  // 灵宠相关接口全部走真实数据库链路：
  // - 已拥有灵宠来自 PetInstances
  // - 获取灵宠改为通过背包使用宠物蛋
  // - 出战、召回、喂养、进化都会直接改后端数据
  getPets() {
    return request('/api/pet')
  },
  setActivePet(petId) {
    return request(`/api/pet/active?petId=${encodeURIComponent(petId)}`, {
      method: 'POST'
    })
  },
  clearActivePet() {
    return request('/api/pet/active', {
      method: 'DELETE'
    })
  },
  feedPet(petId, foodItemId, quantity = 1) {
    return request('/api/pet/feed', {
      method: 'POST',
      body: JSON.stringify({ petId, foodItemId, quantity })
    })
  },
  evolvePet(petId) {
    return request('/api/pet/evolve', {
      method: 'POST',
      body: JSON.stringify({ petId })
    })
  },
  releasePet(petId) {
    return request(`/api/pet/${encodeURIComponent(petId)}`, {
      method: 'DELETE'
    })
  },
  // 中文注释：
  // 炼丹弹窗只在真正打开时拉取炼丹系统状态、丹方列表和结果，
  // 避免继续使用旧的前端随机数和本地药材数组。
  getAlchemyInfo() {
    return request('/api/alchemy')
  },
  getAlchemyRecipes() {
    return request('/api/alchemy/recipes/available')
  },
  getLearnedAlchemyRecipes() {
    return request('/api/alchemy/recipes/learned')
  },
  learnAlchemyRecipe(recipeId) {
    return request(`/api/alchemy/recipes/${encodeURIComponent(recipeId)}/learn`, {
      method: 'POST'
    })
  },
  craftAlchemy(recipeId, quantity = 1) {
    return request('/api/alchemy/craft', {
      method: 'POST',
      body: JSON.stringify({ recipeId, quantity })
    })
  },
  collectAlchemy() {
    return request('/api/alchemy/collect', {
      method: 'POST'
    })
  },
  // 中文注释：
  // 签到状态接口支持按年月查询，方便签到弹窗切换月份时仍然查看真实月历记录。
  getCheckInStatus(year, month) {
    const params = new URLSearchParams()
    if (year) {
      params.set('year', year)
    }
    if (month) {
      params.set('month', month)
    }

    const query = params.toString()
    return request(query ? `/api/checkin?${query}` : '/api/checkin')
  },
  // 中文注释：
  // 执行签到时不需要额外参数，后端会按当前登录角色和当天日期完成校验与发奖。
  claimCheckIn() {
    return request('/api/checkin/claim', {
      method: 'POST'
    })
  },
  // 中文注释：
  // 兑换码统一走后端校验和落库，前端只负责传递用户输入，不再维护本地真假码列表。
  redeemCode(code) {
    return request('/api/redeem/claim', {
      method: 'POST',
      body: JSON.stringify({ code })
    })
  },
  getAchievements() {
    return request('/api/achievement')
  },
  getAchievementOverview() {
    return request('/api/achievement/overview')
  },
  getAchievementProgress() {
    return request('/api/achievement/progress')
  },
  getAchievementStats() {
    return request('/api/achievement/stats')
  },
  claimAchievementReward(achievementId) {
    return request(`/api/achievement/${encodeURIComponent(achievementId)}/claim`, {
      method: 'POST'
    })
  },
  getChatHistory(channelType, count = 50) {
    return request(`/api/chat/history/${channelType}?count=${count}`)
  },
  sendChatMessage(data) {
    return request('/api/chat/send', {
      method: 'POST',
      body: JSON.stringify(data)
    })
  },
  getFiveElementInfo() {
    return request('/api/fiveelement')
  },
  upgradeFiveElementArray() {
    return request('/api/fiveelement/array/upgrade', {
      method: 'POST'
    })
  },
  upgradeFiveElement(elementType) {
    return request('/api/fiveelement/upgrade', {
      method: 'POST',
      body: JSON.stringify({ elementType })
    })
  },
  collectFiveElementSpiritPower() {
    return request('/api/fiveelement/collect', {
      method: 'POST'
    })
  },
  getSpiritFieldInfo() {
    return request('/api/spiritfield')
  },
  getSpiritFieldCrops() {
    return request('/api/spiritfield/crops')
  },
  plantSpiritFieldCrop(plotNumber, cropTemplateId) {
    return request('/api/spiritfield/plant', {
      method: 'POST',
      body: JSON.stringify({ plotNumber, cropTemplateId })
    })
  },
  harvestSpiritFieldCrop(plotNumber) {
    return request('/api/spiritfield/harvest', {
      method: 'POST',
      body: JSON.stringify({ plotNumber })
    })
  },
  speedUpSpiritFieldCrop(plotNumber, itemId) {
    return request('/api/spiritfield/speedup', {
      method: 'POST',
      body: JSON.stringify({ plotNumber, itemId })
    })
  },
  upgradeSpiritFieldPlot(plotNumber) {
    return request(`/api/spiritfield/upgrade/${encodeURIComponent(plotNumber)}`, {
      method: 'POST'
    })
  },
  // 中文注释：
  // 宗门系统相关接口，覆盖宗门总览、心法、捐献、任务、弟子、大比、天骄赛、Boss、商店。
  getSectTemplates() {
    return request('/api/sect')
  },
  getSectDetail(sectId) {
    return request(`/api/sect/${encodeURIComponent(sectId)}`)
  },
  joinSect(sectId) {
    return request(`/api/sect/join/${encodeURIComponent(sectId)}`, {
      method: 'POST'
    })
  },
  getSectSutras() {
    return request('/api/sect/sutras')
  },
  upgradeSutra(sutraId) {
    return request(`/api/sect/sutras/${encodeURIComponent(sutraId)}/upgrade`, {
      method: 'POST'
    })
  },
  donateSect(goldAmount) {
    return request('/api/sect/donation', {
      method: 'POST',
      body: JSON.stringify({ goldAmount })
    })
  },
  getDonationStatus() {
    return request('/api/sect/donation/status')
  },
  getSectTasks() {
    return request('/api/sect/tasks')
  },
  getSectDisciples() {
    return request('/api/sect/disciples')
  },
  sparWithDisciple(playerId) {
    return request(`/api/sect/spar/${encodeURIComponent(playerId)}`, {
      method: 'POST'
    })
  },
  getSectTournament() {
    return request('/api/sect/tournament/status')
  },
  getTournamentMatches(tournamentId) {
    return request(`/api/sect/tournament/${encodeURIComponent(tournamentId)}/matches`)
  },
  claimTournamentReward(tournamentId) {
    return request(`/api/sect/tournament/${encodeURIComponent(tournamentId)}/claim`, {
      method: 'POST'
    })
  },
  getGeniusTournament() {
    return request('/api/sect/genius-tournament/status')
  },
  getGeniusMatches(tournamentId) {
    return request(`/api/sect/genius-tournament/${encodeURIComponent(tournamentId)}/matches`)
  },
  claimGeniusReward(tournamentId) {
    return request(`/api/sect/genius-tournament/${encodeURIComponent(tournamentId)}/claim`, {
      method: 'POST'
    })
  },
  getSectBossStatus() {
    return request('/api/sect/boss/status')
  },
  attackSectBoss() {
    return request('/api/sect/boss/attack', {
      method: 'POST'
    })
  },
  claimSectBossReward() {
    return request('/api/sect/boss/claim', {
      method: 'POST'
    })
  },
  getSectShopItems() {
    return request('/api/sect/shop')
  },
  purchaseSectItem(shopItemId) {
    return request(`/api/sect/shop/${encodeURIComponent(shopItemId)}/purchase`, {
      method: 'POST'
    })
  },
  getSectBlessings() {
    return request('/api/sect/blessings')
  },

  // ===== 图鉴抽奖系统 =====
  getLotteryPools() {
    return request('/api/lottery/list')
  },
  lotteryDraw(poolId, count) {
    return request('/api/lottery/draw', {
      method: 'POST',
      body: JSON.stringify({ poolId, count })
    })
  },
  getCollectionConfig() {
    return request('/api/collection/config')
  },
  getPlayerCollection() {
    return request('/api/collection/player')
  },
  getPlayerBonuses() {
    return request('/api/collection/bonus')
  },

  // ===== 好感度系统 =====
  getFavorabilityList(direction = 'ToOthers') {
    return request(`/api/favorability/list?direction=${direction}`)
  },
  getFavorabilityLog(targetPlayerId = null) {
    let url = '/api/favorability/log?pageIndex=1&pageSize=50'
    if (targetPlayerId) url += `&targetPlayerId=${targetPlayerId}`
    return request(url)
  },
  getFavorabilityLevels() {
    return request('/api/favorability/levels')
  },
  giftFavorabilityItem(targetPlayerId, itemId) {
    return request('/api/favorability/gift', {
      method: 'POST',
      body: JSON.stringify({ targetPlayerId, itemId })
    })
  },

  // ===== 邮件系统 =====
  getMails(page = 1, pageSize = 20) {
    return request(`/api/mail?page=${page}&pageSize=${pageSize}`)
  },
  getMailDetail(mailId) {
    return request(`/api/mail/${mailId}`)
  },
  markMailAsRead(mailId) {
    return request(`/api/mail/${mailId}/read`, {
      method: 'POST'
    })
  },
  claimMailAttachments(mailId) {
    return request(`/api/mail/${mailId}/claim`, {
      method: 'POST'
    })
  },
  claimAllMailAttachments() {
    return request('/api/mail/claim-all', {
      method: 'POST'
    })
  },
  deleteMail(mailId) {
    return request(`/api/mail/${mailId}`, {
      method: 'DELETE'
    })
  },
  markAllMailsAsRead() {
    return request('/api/mail/read-all', {
      method: 'POST'
    })
  },

  // ===== 称号系统 =====
  getTitleOverview() {
    return request('/api/title')
  },
  equipTitle(titleId) {
    return request('/api/title/equip', {
      method: 'POST',
      body: JSON.stringify({ titleId })
    })
  },
  unequipTitle() {
    return request('/api/title/unequip', {
      method: 'POST'
    })
  },

  // ===== PVP竞技场 =====
  getArenaMe() {
    return request('/api/arena/me')
  },
  getArenaOpponents() {
    return request('/api/arena/opponents')
  },
  arenaChallenge(defenderId) {
    return request('/api/arena/challenge', {
      method: 'POST',
      body: JSON.stringify({ defenderId })
    })
  },
  getArenaHistory(page = 1, pageSize = 20) {
    return request(`/api/arena/history?page=${page}&pageSize=${pageSize}`)
  },
  buyArenaAttempts() {
    return request('/api/arena/buy-attempts', {
      method: 'POST'
    })
  },
  getArenaSeason() {
    return request('/api/arena/season')
  },

  // ===== 通天塔 =====
  getTowerMe() {
    return request('/api/tower/me')
  },
  getTowerFloors() {
    return request('/api/tower/floors')
  },
  towerChallenge() {
    return request('/api/tower/challenge', {
      method: 'POST'
    })
  },
  getTowerHistory(take = 20) {
    return request(`/api/tower/history?take=${take}`)
  },
  buyTowerAttempts() {
    return request('/api/tower/buy-attempts', {
      method: 'POST'
    })
  },
  getTowerLeaderboard(count = 50) {
    return request(`/api/tower/leaderboard?count=${count}`)
  },

  // ===== 宝石系统 =====
  getGemTemplates() {
    return request('/api/gem/templates')
  },
  getGemInventory() {
    return request('/api/gem/inventory')
  },
  socketGem(equipmentInstanceId, slotIndex, inventoryItemId) {
    return request('/api/gem/socket', {
      method: 'POST',
      body: JSON.stringify({ equipmentInstanceId, slotIndex, inventoryItemId })
    })
  },
  unsocketGem(equipmentInstanceId, slotIndex) {
    return request('/api/gem/unsocket', {
      method: 'POST',
      body: JSON.stringify({ equipmentInstanceId, slotIndex })
    })
  },
  synthesizeGem(gemId, inventoryItemId) {
    return request('/api/gem/synthesize', {
      method: 'POST',
      body: JSON.stringify({ gemId, inventoryItemId })
    })
  },

  // ===== 寄售行 =====
  browseMarket(itemType, currencyType, keyword, sortBy, sortOrder, page, pageSize) {
    const params = new URLSearchParams()
    if (itemType) params.set('itemType', itemType)
    if (currencyType) params.set('currencyType', currencyType)
    if (keyword) params.set('keyword', keyword)
    if (sortBy) params.set('sortBy', sortBy)
    if (sortOrder) params.set('sortOrder', sortOrder)
    params.set('page', String(page || 1))
    params.set('pageSize', String(pageSize || 20))
    return request(`/api/market/browse?${params}`)
  },
  getMyMarketListings() {
    return request('/api/market/me')
  },
  listMarketItem(itemType, inventoryItemId, equipmentInstanceId, quantity, price, currencyType) {
    return request('/api/market/list', {
      method: 'POST',
      body: JSON.stringify({ itemType, inventoryItemId, equipmentInstanceId, quantity, price, currencyType })
    })
  },
  cancelMarketListing(listingId) {
    return request('/api/market/cancel', {
      method: 'POST',
      body: JSON.stringify(listingId)
    })
  },
  buyMarketItem(listingId) {
    return request('/api/market/buy', {
      method: 'POST',
      body: JSON.stringify(listingId)
    })
  },
  getMarketHistory(page, pageSize) {
    const params = new URLSearchParams()
    params.set('page', String(page || 1))
    params.set('pageSize', String(pageSize || 20))
    return request(`/api/market/history?${params}`)
  }
}



