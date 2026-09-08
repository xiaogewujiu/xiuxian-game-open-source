import { reactive, readonly } from 'vue'
import { apiClient, setAccessToken, setUnauthorizedHandler } from '../lib/apiClient'

const ACCESS_TOKEN_KEY = 'xiuxian-access-token'
const REFRESH_TOKEN_KEY = 'xiuxian-refresh-token'
const DEFAULT_RANKING_IDS = ['ranking_level', 'ranking_wealth', 'ranking_achievement', 'ranking_arena', 'ranking_tower']

/**
 * 中文注释：
 * 这里没有额外引入 Pinia，而是直接用 Vue 自带的 reactive 做一个轻量级全局状态仓库。
 * 这样可以减少依赖，保持当前项目“本地开发即可跑通”的目标，
 * 同时也足够支撑登录、战斗、背包、商店、排行这些真实交互。
 */
const state = reactive({
  accessToken: localStorage.getItem(ACCESS_TOKEN_KEY) || '',
  refreshToken: localStorage.getItem(REFRESH_TOKEN_KEY) || '',
  player: null,
  maps: [],
  dungeons: [],
  parties: [],
  currentParty: null,
  offlineBattleStatus: null,
  inventory: [],
  equipments: [],
  equippedItems: [],
  shops: [],
  rankings: {},
  loaded: {
    player: false,
    maps: false,
    dungeons: false,
    parties: false,
    currentParty: false,
    inventory: false,
    equipments: false,
    equippedItems: false,
    shops: false,
    rankings: {}
  },
  lastBattleResult: null,
  loading: false,
  authLoading: false,
  actionLoading: false,
  lastMessage: '',
  lastError: '',
  favorabilityList: [],
  favorabilityFromOthers: [],
  favorabilityLog: [],
  favorabilityLevels: [],
  favorabilityLoaded: false
})

setAccessToken(state.accessToken)

// 响应 apiClient 发出的会话变化事件，确保 store 与 localStorage 始终一致。
function syncTokenStateFromStorage() {
  state.accessToken = localStorage.getItem(ACCESS_TOKEN_KEY) || ''
  state.refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY) || ''
  setAccessToken(state.accessToken)
}

if (typeof window !== 'undefined') {
  window.addEventListener('xiuxian-session-changed', syncTokenStateFromStorage)
}

function setMessage(message = '') {
  state.lastMessage = message
  if (message) {
    state.lastError = ''
  }
}

// 统一写入错误提示。
function setError(message = '') {
  state.lastError = message
  if (message) {
    // 中文注释：
    // 一旦进入错误态，就清掉上一条成功提示，避免页面同时出现“成功”和“失败”两种互相矛盾的提示。
    state.lastMessage = ''
  }
}

function clearFeedback() {
  state.lastMessage = ''
  state.lastError = ''
}

// 把任意异常统一归一成 Error，方便调用方稳定读取 message。
function normalizeActionError(error, fallbackMessage) {
  if (error instanceof Error) {
    if (!error.message) {
      error.message = fallbackMessage
    }

    return error
  }

  return new Error(fallbackMessage)
}

function extractBattleCooldown(source) {
  const cooldownSeconds = Number(source?.battleCooldownSeconds ?? source?.BattleCooldownSeconds ?? 0) || 0
  const cooldownUntilUtc = source?.battleCooldownUntilUtc ?? source?.BattleCooldownUntilUtc ?? ''

  return {
    cooldownSeconds,
    cooldownUntilUtc
  }
}

// 把战斗冷却信息同步回当前玩家快照，避免战斗失败后界面还是旧的冷却状态。
function syncBattleCooldownToPlayer(source) {
  if (!state.player) {
    return
  }

  const { cooldownSeconds, cooldownUntilUtc } = extractBattleCooldown(source)
  state.player = {
    ...state.player,
    battleCooldownSeconds: cooldownSeconds,
    BattleCooldownSeconds: cooldownSeconds,
    battleCooldownUntilUtc: cooldownUntilUtc,
    BattleCooldownUntilUtc: cooldownUntilUtc
  }
}

// 后端离线挂机状态同时存在大小写两种字段风格，这里统一规范成前端内部结构。
function normalizeOfflineBattleStatus(source) {
  return {
    isOfflineBattling: Boolean(source?.isOfflineBattling ?? source?.IsOfflineBattling),
    mapId: source?.mapId ?? source?.MapId ?? '',
    mapName: source?.mapName ?? source?.MapName ?? '',
    startedAtUtc: source?.startedAtUtc ?? source?.StartedAtUtc ?? '',
    lastTickAtUtc: source?.lastTickAtUtc ?? source?.LastTickAtUtc ?? '',
    totalBattles: Number(source?.totalBattles ?? source?.TotalBattles ?? 0) || 0,
    winBattles: Number(source?.winBattles ?? source?.WinBattles ?? 0) || 0,
    totalRounds: Number(source?.totalRounds ?? source?.TotalRounds ?? 0) || 0
  }
}

// 已加载标记只在当前会话内生效。
// 清空它可以强制下次重新拉取对应模块数据。
function resetLoadedState() {
  state.loaded.player = false
  state.loaded.maps = false
  state.loaded.dungeons = false
  state.loaded.parties = false
  state.loaded.currentParty = false
  state.loaded.inventory = false
  state.loaded.equipments = false
  state.loaded.equippedItems = false
  state.loaded.shops = false
  state.loaded.rankings = {}
}

// 本地彻底清空玩家会话与已加载数据。
function clearSession(message = '') {
  state.accessToken = ''
  state.refreshToken = ''
  state.player = null
  state.maps = []
  state.dungeons = []
  state.parties = []
  state.currentParty = null
  state.offlineBattleStatus = null
  state.inventory = []
  state.equipments = []
  state.equippedItems = []
  state.shops = []
  state.rankings = {}
  state.lastBattleResult = null
  state.favorabilityList = []
  state.favorabilityFromOthers = []
  state.favorabilityLog = []
  state.favorabilityLevels = []
  state.favorabilityLoaded = false
  resetLoadedState()

  localStorage.removeItem(ACCESS_TOKEN_KEY)
  localStorage.removeItem(REFRESH_TOKEN_KEY)
  setAccessToken('')

  if (message) {
    setMessage(message)
  }
}

// 登录或注册成功后，把令牌和玩家快照写入全局状态。
function persistSession(loginData) {
  state.accessToken = loginData?.accessToken || loginData?.AccessToken || ''
  state.refreshToken = loginData?.refreshToken || loginData?.RefreshToken || ''
  state.player = loginData?.player || loginData?.Player || null
  state.loaded.player = Boolean(state.player)

  localStorage.setItem(ACCESS_TOKEN_KEY, state.accessToken)
  localStorage.setItem(REFRESH_TOKEN_KEY, state.refreshToken)
  setAccessToken(state.accessToken)
}

// 中文注释：
// 路由守卫只能拦截“还没进页面”的情况。
// 如果用户已经在游戏内，后端再返回 401，就需要在这里统一做会话清理和回跳，
// 避免各个弹窗自己处理一遍未授权，最终出现状态残留或重复弹错。
setUnauthorizedHandler(() => {
  clearSession('登录状态已失效，请重新登录。')
  if (window.location.pathname !== '/') {
    window.location.replace('/')
  }
})

async function loadPlayer(force = false) {
  if (state.loaded.player && !force) {
    return state.player
  }

  state.player = await apiClient.getCurrentPlayer()
  state.loaded.player = true
  return state.player
}

async function loadMaps(force = false) {
  if (state.loaded.maps && !force) {
    return state.maps
  }

  state.maps = await apiClient.getMaps()
  state.loaded.maps = true
  return state.maps
}

async function loadInventory(force = false) {
  if (state.loaded.inventory && !force) {
    return state.inventory
  }

  state.inventory = await apiClient.getInventory()
  state.loaded.inventory = true
  return state.inventory
}

async function loadEquipment(force = false) {
  if (state.loaded.equipments && !force) {
    return state.equipments
  }

  state.equipments = await apiClient.getEquipment()
  state.loaded.equipments = true
  return state.equipments
}

async function loadEquippedItems(force = false) {
  if (state.loaded.equippedItems && !force) {
    return state.equippedItems
  }

  state.equippedItems = await apiClient.getEquipped()
  state.loaded.equippedItems = true
  return state.equippedItems
}

async function loadDungeons(force = false) {
  if (state.loaded.dungeons && !force) {
    return state.dungeons
  }

  state.dungeons = await apiClient.getDungeons()
  state.loaded.dungeons = true
  return state.dungeons
}

// 统一把“当前是否已加入临时队伍”转换成稳定结构。
function normalizeCurrentParty(party) {
  const normalizedParty = party || null
  const partyId = normalizedParty?.partyId || normalizedParty?.PartyId || ''
  return partyId ? normalizedParty : null
}

async function loadParties(force = false) {
  if (state.loaded.parties && !force) {
    return state.parties
  }

  state.parties = await apiClient.getParties()
  state.loaded.parties = true
  return state.parties
}

async function loadCurrentParty(force = false) {
  if (state.loaded.currentParty && !force) {
    return state.currentParty
  }

  state.currentParty = normalizeCurrentParty(await apiClient.getCurrentParty())
  state.loaded.currentParty = true
  return state.currentParty
}

async function loadShops(force = false) {
  if (state.loaded.shops && !force) {
    return state.shops
  }

  state.shops = await apiClient.getShops()
  state.loaded.shops = true
  return state.shops
}

// 单个榜单按需加载，并以 rankingId 为缓存键。
async function loadRanking(rankingId = 'ranking_level', topCount = 10, force = false) {
  if (state.loaded.rankings[rankingId] && !force) {
    return state.rankings[rankingId] || []
  }

  const ranking = await apiClient.getRanking(rankingId, topCount)
  state.rankings = {
    ...state.rankings,
    [rankingId]: ranking
  }
  state.loaded.rankings = {
    ...state.loaded.rankings,
    [rankingId]: true
  }
  return ranking
}

async function loadDefaultRankings(force = false) {
  const results = await Promise.allSettled(
    DEFAULT_RANKING_IDS.map((rankingId) => loadRanking(rankingId, 10, force))
  )

  // 中文注释：
  // 排行榜在当前项目里不是阻塞主流程的关键数据，
  // 所以这里采用“尽量加载”的策略，某一类榜单失败时不影响登录后的主界面打开。
  return results
}

// 按当前页面依赖关系拼出“懒刷新”任务列表。
function buildLazyRefreshTasks(options = {}) {
  const refreshTasks = []

  if (options.includePlayer !== false) {
    refreshTasks.push(loadPlayer(true))
  }

  if (options.includeEquippedItems && state.loaded.equippedItems) {
    refreshTasks.push(loadEquippedItems(true))
  }

  if (options.includeInventory && state.loaded.inventory) {
    refreshTasks.push(loadInventory(true))
  }

  if (options.includeEquipments && state.loaded.equipments) {
    refreshTasks.push(loadEquipment(true))
  }

  if (options.includeMaps && state.loaded.maps) {
    refreshTasks.push(loadMaps(true))
  }

  if (options.includeDungeons && state.loaded.dungeons) {
    refreshTasks.push(loadDungeons(true))
  }

  if (options.includeParties && state.loaded.parties) {
    refreshTasks.push(loadParties(true))
  }

  if (options.includeCurrentParty && state.loaded.currentParty) {
    refreshTasks.push(loadCurrentParty(true))
  }

  if (options.includeShops && state.loaded.shops) {
    refreshTasks.push(loadShops(true))
  }

  if (options.includeRankings) {
    Object.keys(state.loaded.rankings).forEach((rankingId) => {
      if (state.loaded.rankings[rankingId]) {
        refreshTasks.push(loadRanking(rankingId, 10, true))
      }
    })
  }

  return refreshTasks
}
/**
 * 中文注释：
 * 主游戏页首屏只加载“页面立即可见”的核心数据：
 * 1. 玩家信息：左侧人物信息、资源条、属性值需要立即展示。
 * 2. 已穿戴装备：中间装备栏是首页固定内容，必须首屏可见。
 *
 * 地图、背包、商店、排行榜这类弹窗数据改为点击后再加载，
 * 这样可以减少首页无意义接口请求，避免一进游戏就把所有弹窗接口全部打完。
 */
async function bootstrapGameData() {
  if (!state.accessToken) {
    return
  }

  state.loading = true
  setError('')

  try {
    await Promise.all([
      loadPlayer(true),
      loadEquippedItems(true)
    ])
  } catch (error) {
    setError(error.message || '加载游戏数据失败。')
    throw error
  } finally {
    state.loading = false
  }
}

async function login(form) {
  state.authLoading = true
  setError('')

  try {
    const data = await apiClient.login(form)
    persistSession(data)
    setMessage('登录成功，正在进入游戏。')
    return data
  } catch (error) {
    setError(error.message || '登录失败。')
    throw error
  } finally {
    state.authLoading = false
  }
}

async function register(form) {
  state.authLoading = true
  setError('')

  try {
    const data = await apiClient.register(form)
    persistSession(data)
    setMessage('注册成功，角色已创建。')
    return data
  } catch (error) {
    setError(error.message || '注册失败。')
    throw error
  } finally {
    state.authLoading = false
  }
}

async function startMapBattle(mapId) {
  state.actionLoading = true
  setError('')

  try {
    const result = await apiClient.startBattle({
      battleType: 'PVE',
      mapId
    })
    state.lastBattleResult = result
    syncBattleCooldownToPlayer(result)
    setMessage(result?.isWin ?? result?.IsWin ? '战斗胜利。' : '战斗失败。')

    // 中文注释：
    // 普通战斗结束后只刷新玩家快照，保证经验、资源和冷却状态及时同步。
    // 背包、装备、地图和排行榜属于按需加载的数据，分别在对应弹窗打开时刷新，
    // 避免每场战斗结束后重复请求一批当前不可见的接口。
    await loadPlayer(true)

    return result
  } catch (error) {
    const { cooldownSeconds } = extractBattleCooldown(error?.data)
    syncBattleCooldownToPlayer(error?.data)
    if (cooldownSeconds <= 0) {
      setError(error.message || '开始战斗失败。')
    }
    throw normalizeActionError(error, '开始战斗失败。')
  } finally {
    state.actionLoading = false
  }
}

async function loadOfflineBattleStatus() {
  if (!state.accessToken) {
    state.offlineBattleStatus = null
    return null
  }

  const result = await apiClient.getOfflineBattleStatus()
  state.offlineBattleStatus = normalizeOfflineBattleStatus(result)
  return state.offlineBattleStatus
}

async function startOfflineBattle(mapId) {
  state.actionLoading = true
  setError('')

  try {
    const result = await apiClient.startOfflineBattle(mapId)
    state.offlineBattleStatus = normalizeOfflineBattleStatus(result)
    setMessage('已开始离线挂机。')
    return result
  } catch (error) {
    setError(error.message || '开始离线挂机失败。')
    throw normalizeActionError(error, '开始离线挂机失败。')
  } finally {
    state.actionLoading = false
  }
}

async function stopOfflineBattle() {
  state.actionLoading = true
  setError('')

  try {
    const result = await apiClient.stopOfflineBattle()
    state.offlineBattleStatus = null

    // 中文注释：
    // 停止离线挂机后只同步玩家快照；背包和装备在对应弹窗打开时再强制刷新，
    // 地图、队伍、排行榜和副本数据不属于停止操作的即时依赖，避免产生无关请求。
    await loadPlayer(true)

    setMessage('已停止离线挂机。')
    return result
  } catch (error) {
    setError(error.message || '停止离线挂机失败。')
    throw normalizeActionError(error, '停止离线挂机失败。')
  } finally {
    state.actionLoading = false
  }
}

async function challengeDungeon(dungeonId) {
  state.actionLoading = true
  setError('')

  try {
    const result = await apiClient.challengeDungeon(dungeonId)
    state.lastBattleResult = result
    syncBattleCooldownToPlayer(result)
    setMessage(result?.isWin ?? result?.IsWin ? '战斗胜利。' : '战斗失败。')

    // 中文注释：
    // 副本结算后只刷新玩家快照；副本、背包、装备和排行榜数据由对应弹窗按需刷新。
    await loadPlayer(true)

    return result
  } catch (error) {
    const { cooldownSeconds } = extractBattleCooldown(error?.data)
    syncBattleCooldownToPlayer(error?.data)
    if (cooldownSeconds <= 0) {
      setError(error.message || '副本挑战失败。')
    }
    throw normalizeActionError(error, '副本挑战失败。')
  } finally {
    state.actionLoading = false
  }
}

async function breakthrough() {
  state.actionLoading = true
  setError('')

  try {
    const result = await apiClient.breakthrough()
    state.player = result?.player || result?.Player || state.player
    state.loaded.player = Boolean(state.player)
    setMessage(result?.message || result?.Message || '突破成功。')

    Promise.allSettled(buildLazyRefreshTasks({
      includeDungeons: true,
      includeParties: true,
      includeCurrentParty: true,
      includeRankings: true
    })).then((results) => {
      const hasRefreshFailure = results.some((item) => item.status === 'rejected')
      if (hasRefreshFailure) {
        setError('突破后的部分数据刷新失败，请稍后手动刷新。')
      }
    })

    return result
  } catch (error) {
    setError(error.message || '突破失败。')
    throw error
  } finally {
    state.actionLoading = false
  }
}

async function createParty(request) {
  state.actionLoading = true
  setError('')

  try {
    const result = await apiClient.createParty(request)
    state.currentParty = normalizeCurrentParty(result)
    state.loaded.currentParty = true
    setMessage(`队伍已创建：${state.currentParty?.Name || state.currentParty?.name || request?.name || '未命名队伍'}。`)

    await Promise.allSettled([
      loadParties(true),
      loadDungeons(true)
    ])

    return result
  } catch (error) {
    setError(error.message || '创建队伍失败。')
    throw error
  } finally {
    state.actionLoading = false
  }
}

async function joinParty(partyId) {
  state.actionLoading = true
  setError('')

  try {
    const result = await apiClient.joinParty(partyId)
    state.currentParty = normalizeCurrentParty(result)
    state.loaded.currentParty = true
    setMessage(`已加入队伍：${state.currentParty?.Name || state.currentParty?.name || partyId}。`)

    await Promise.allSettled([
      loadParties(true),
      loadDungeons(true)
    ])

    return result
  } catch (error) {
    setError(error.message || '加入队伍失败。')
    throw error
  } finally {
    state.actionLoading = false
  }
}

async function leaveCurrentParty() {
  state.actionLoading = true
  setError('')

  try {
    const result = await apiClient.leaveCurrentParty()
    state.currentParty = null
    state.loaded.currentParty = true
    setMessage(result?.message || result?.Message || '已退出当前队伍。')

    await Promise.allSettled([
      loadParties(true),
      loadDungeons(true)
    ])

    return result
  } catch (error) {
    setError(error.message || '退出队伍失败。')
    throw error
  } finally {
    state.actionLoading = false
  }
}

async function togglePartyRecruiting(partyId) {
  state.actionLoading = true
  setError('')

  try {
    const result = await apiClient.togglePartyRecruiting(partyId)
    state.currentParty = normalizeCurrentParty(result)
    state.loaded.currentParty = true

    const isRecruiting = Boolean(result?.isRecruiting ?? result?.IsRecruiting)
    setMessage(isRecruiting ? '队伍已开启招募。' : '队伍已关闭招募。')

    await Promise.allSettled([
      loadParties(true),
      loadDungeons(true)
    ])

    return result
  } catch (error) {
    setError(error.message || '切换招募状态失败。')
    throw error
  } finally {
    state.actionLoading = false
  }
}

async function dismissParty(partyId) {
  state.actionLoading = true
  setError('')

  try {
    const result = await apiClient.dismissParty(partyId)
    state.currentParty = null
    state.loaded.currentParty = true
    setMessage(result?.message || result?.Message || '队伍已解散。')

    await Promise.allSettled([
      loadParties(true),
      loadDungeons(true)
    ])

    return result
  } catch (error) {
    setError(error.message || '解散队伍失败。')
    throw error
  } finally {
    state.actionLoading = false
  }
}

async function challengePartyDungeon(dungeonId, partyId) {
  state.actionLoading = true
  setError('')

  try {
    const result = await apiClient.challengePartyDungeon(dungeonId, partyId)
    state.lastBattleResult = result
    syncBattleCooldownToPlayer(result)
    setMessage(result?.isWin ?? result?.IsWin ? '组队副本挑战胜利。' : '组队副本挑战失败。')

    // 中文注释：
    // 组队副本结算后只刷新玩家快照，避免同时重刷背包、装备、地图、队伍和排行榜。
    // 这些界面数据在用户重新打开对应弹窗时再读取最新值。
    await loadPlayer(true)

    return result
  } catch (error) {
    const { cooldownSeconds } = extractBattleCooldown(error?.data)
    syncBattleCooldownToPlayer(error?.data)
    if (cooldownSeconds <= 0) {
      setError(error.message || '组队副本挑战失败。')
    }
    throw normalizeActionError(error, '组队副本挑战失败。')
  } finally {
    state.actionLoading = false
  }
}

async function buyItem(shopId, itemId, count = 1) {
  state.actionLoading = true
  setError('')

  try {
    const result = await apiClient.buyItem(shopId, itemId, count)
    setMessage(result?.message || result?.Message || '购买成功。')

    await Promise.all(buildLazyRefreshTasks({
      includeInventory: true,
      includeEquipments: true,
      includeShops: true
    }))

    return result
  } catch (error) {
    setError(error.message || '购买失败。')
    throw error
  } finally {
    state.actionLoading = false
  }
}

async function useItem(itemId, quantity = 1) {
  state.actionLoading = true
  setError('')

  try {
    const result = await apiClient.useItem(itemId, quantity)
    setMessage(result?.message || result?.Message || '道具使用完成。')

    await Promise.all([
      loadPlayer(true),
      loadInventory(true)
    ])

    return result
  } catch (error) {
    setError(error.message || '使用道具失败。')
    throw error
  } finally {
    state.actionLoading = false
  }
}

async function decomposeSkillBook(itemId, quantity = 1) {
  state.actionLoading = true
  setError('')

  try {
    const result = await apiClient.decomposeSkillBook(itemId, quantity)
    setMessage(result?.message || result?.Message || '技能书分解完成。')
    await Promise.all([loadPlayer(true), loadInventory(true)])
    return result
  } catch (error) {
    setError(error.message || '技能书分解失败。')
    throw error
  } finally {
    state.actionLoading = false
  }
}

async function equipItem(equipmentId) {
  state.actionLoading = true
  setError('')

  try {
    await apiClient.equipItem(equipmentId)
    setMessage('装备已穿戴。')

    await Promise.all(buildLazyRefreshTasks({
      includeEquipments: true,
      includeEquippedItems: true
    }))
  } catch (error) {
    setError(error.message || '穿戴装备失败。')
    throw error
  } finally {
    state.actionLoading = false
  }
}

async function unequipItem(slot) {
  state.actionLoading = true
  setError('')

  try {
    await apiClient.unequipItem(slot)
    setMessage('装备已卸下。')

    await Promise.all(buildLazyRefreshTasks({
      includeEquipments: true,
      includeEquippedItems: true
    }))
  } catch (error) {
    setError(error.message || '卸下装备失败。')
    throw error
  } finally {
    state.actionLoading = false
  }
}

async function sellEquipment(equipmentId) {
  state.actionLoading = true
  setError('')

  try {
    const result = await apiClient.sellEquipment(equipmentId)
    setMessage('装备出售成功。')

    await Promise.all(buildLazyRefreshTasks({
      includeEquipments: true,
      includeEquippedItems: true
    }))

    return result
  } catch (error) {
    setError(error.message || '出售装备失败。')
    throw error
  } finally {
    state.actionLoading = false
  }
}

async function sellInventoryItem(itemId, count = 1) {
  state.actionLoading = true
  setError('')

  try {
    const result = await apiClient.sellInventoryItem(itemId, count)
    setMessage(result?.message || result?.Message || '道具出售成功。')

    await Promise.all([
      loadPlayer(true),
      loadInventory(true),
      ...(state.loaded.shops ? [loadShops(true)] : [])
    ])

    return result
  } catch (error) {
    setError(error.message || '出售道具失败。')
    throw error
  } finally {
    state.actionLoading = false
  }
}

async function logout(message = '已退出当前账号。') {
  try {
    if (state.accessToken) {
      await apiClient.logout()
    }
  } catch (error) {
    // 中文注释：
    // 登出接口失败时不阻塞前端本地会话清理，
    // 否则用户会遇到“明明点了退出却还卡在当前账号”的体验问题。
  }

  clearSession(message)
}

// ==================== 好感度 ====================

async function loadFavorabilityList(direction = 'ToOthers') {
  try {
    const data = await apiClient.getFavorabilityList(direction)
    if (direction === 'ToOthers') {
      state.favorabilityList = data || []
    } else {
      state.favorabilityFromOthers = data || []
    }
  } catch (e) {
    console.error('加载好感度列表失败', e)
  }
}

async function loadFavorabilityLog(targetPlayerId = null) {
  try {
    const data = await apiClient.getFavorabilityLog(targetPlayerId)
    state.favorabilityLog = data || []
  } catch (e) {
    console.error('加载赠送记录失败', e)
  }
}

async function loadFavorabilityLevels() {
  try {
    const data = await apiClient.getFavorabilityLevels()
    state.favorabilityLevels = data || []
  } catch (e) {
    console.error('加载好感度等级失败', e)
  }
}

async function giftFavorabilityItem(targetPlayerId, itemId) {
  try {
    const data = await apiClient.giftFavorabilityItem(targetPlayerId, itemId)
    setMessage('赠送成功')
    return data
  } catch (e) {
    setError(normalizeActionError(e, '赠送失败'))
    return null
  }
}

export function useGameStore() {
  return {
    state: readonly(state),
    clearFeedback,
    login,
    register,
    logout,
    bootstrapGameData,
    loadMaps,
    startMapBattle,
    loadOfflineBattleStatus,
    startOfflineBattle,
    stopOfflineBattle,
    challengeDungeon,
    breakthrough,
    loadParties,
    loadCurrentParty,
    createParty,
    joinParty,
    leaveCurrentParty,
    togglePartyRecruiting,
    dismissParty,
    challengePartyDungeon,
    buyItem,
    useItem,
    decomposeSkillBook,
    equipItem,
    unequipItem,
    sellEquipment,
    sellInventoryItem,
    loadPlayer,
    loadInventory,
    loadEquipment,
    loadEquippedItems,
    loadDungeons,
    loadShops,
    loadRanking,
    loadDefaultRankings,
    loadFavorabilityList,
    loadFavorabilityLog,
    loadFavorabilityLevels,
    giftFavorabilityItem
  }
}

