import { apiClient } from '../lib/apiClient'
import { useGameStore as useLegacyGameStore } from '../stores/game'

/**
 * 中文注释：
 * 这是对旧全局状态仓库的包装层。
 * 旧仓库文件位于 `src/stores`，当前补丁工具无法稳定修改那个目录，
 * 所以这里采用“保留旧能力 + 新能力从包装层扩展”的方式继续推进开发。
 * 这样既不会破坏现有登录、战斗、商店等流程，也能把新增真实接口逐步接进来。
 */
export function useGameStore() {
  const legacyStore = useLegacyGameStore()

  async function refreshPlayerSnapshot({
    includeInventory = false,
    includeEquipment = false,
    includeParty = false,
    includeShops = false,
    includeRankings = true
  } = {}) {
    const {
      state,
      loadPlayer,
      loadInventory,
      loadEquipment,
      loadEquippedItems,
      loadCurrentParty,
      loadShops,
      loadRanking
    } = legacyStore

    // 中文注释：
    // 这是所有“局部系统操作后补刷新”的统一入口。
    // 调用方通过参数声明自己依赖哪些模块，避免每次都把全部接口重刷一遍。
    await Promise.allSettled([
      loadPlayer(true),
      includeInventory ? loadInventory(true) : Promise.resolve(),
      includeEquipment ? loadEquipment(true) : Promise.resolve(),
      includeEquipment ? loadEquippedItems(true) : Promise.resolve(),
      includeParty ? loadCurrentParty(true) : Promise.resolve(),
      includeShops ? loadShops(true) : Promise.resolve(),
      ...(includeRankings && state.loaded?.rankings
        ? Object.keys(state.loaded.rankings)
          .filter((rankingId) => state.loaded.rankings[rankingId])
          .map((rankingId) => loadRanking(rankingId, 10, true))
        : [])
    ])
  }

  async function refreshAfterEquipmentMutation({ includeInventory = true } = {}) {
    // 中文注释：
    // 装备变化只影响人物面板和装备数据；战力榜已移除，不再刷新排行榜。
    await refreshPlayerSnapshot({
      includeInventory,
      includeEquipment: true,
      includeRankings: false
    })
  }

  async function enhanceEquipment(equipmentId, materials = []) {
    try {
      const result = await apiClient.enhanceEquipment(equipmentId, materials)
      await refreshAfterEquipmentMutation({ includeInventory: true })
      return result
    } catch (error) {
      throw new Error(error.message || '强化装备失败。')
    }
  }

  async function bindEquipment(equipmentId) {
    try {
      const result = await apiClient.bindEquipment(equipmentId)
      await refreshAfterEquipmentMutation({ includeInventory: false })
      return result
    } catch (error) {
      throw new Error(error.message || '锁定装备失败。')
    }
  }

  async function unlockEquipment(equipmentId) {
    try {
      const result = await apiClient.unlockEquipment(equipmentId)
      await refreshAfterEquipmentMutation({ includeInventory: false })
      return result
    } catch (error) {
      throw new Error(error.message || '解锁装备失败。')
    }
  }

  async function lockItem(itemId) {
    try {
      const result = await apiClient.lockItem(itemId)
      await refreshAfterEquipmentMutation({ includeInventory: true })
      return result
    } catch (error) {
      throw new Error(error.message || '锁定道具失败。')
    }
  }

  async function unlockItem(itemId) {
    try {
      const result = await apiClient.unlockItem(itemId)
      await refreshAfterEquipmentMutation({ includeInventory: true })
      return result
    } catch (error) {
      throw new Error(error.message || '解锁道具失败。')
    }
  }

  async function donateEquipment(equipmentId) {
    try {
      const result = await apiClient.donateEquipment(equipmentId)
      await refreshAfterEquipmentMutation({ includeInventory: false })
      return result
    } catch (error) {
      throw new Error(error.message || '捐献装备失败。')
    }
  }

  async function getEquipmentRerollPreview(equipmentId) {
    try {
      return await apiClient.getEquipmentRerollPreview(equipmentId)
    } catch (error) {
      throw new Error(error.message || '获取洗练预览失败。')
    }
  }

  async function rollEquipmentReroll(equipmentId, lockedIndices = []) {
    try {
      const result = await apiClient.rollEquipmentReroll(equipmentId, lockedIndices)
      await refreshAfterEquipmentMutation({ includeInventory: true })
      return result
    } catch (error) {
      throw new Error(error.message || '洗练失败。')
    }
  }

  async function acceptEquipmentReroll(equipmentId) {
    try {
      const result = await apiClient.acceptEquipmentReroll(equipmentId)
      await refreshAfterEquipmentMutation({ includeInventory: true })
      return result
    } catch (error) {
      throw new Error(error.message || '接受洗练结果失败。')
    }
  }

  async function discardEquipmentReroll(equipmentId) {
    try {
      const result = await apiClient.discardEquipmentReroll(equipmentId)
      await refreshAfterEquipmentMutation({ includeInventory: true })
      return result
    } catch (error) {
      throw new Error(error.message || '丢弃洗练结果失败。')
    }
  }

  async function discardItem(itemId, quantity = 1) {
    const { state, loadInventory, loadPlayer } = legacyStore

    try {
      const result = await apiClient.discardItem(itemId, quantity)

      // 中文注释：
      // 丢弃物品只会影响背包和玩家资源面板，因此这里不触发装备、队伍等无关模块刷新。
      await Promise.allSettled([
        state.loaded?.inventory ? loadInventory(true) : Promise.resolve(),
        state.loaded?.player ? loadPlayer(true) : Promise.resolve()
      ])

      return result
    } catch (error) {
      throw new Error(error.message || '丢弃物品失败。')
    }
  }

  async function discardItemsBatch(items = []) {
    try {
      const result = await apiClient.discardItemsBatch(items)
      await refreshPlayerSnapshot({ includeInventory: true, includeRankings: false })
      return result
    } catch (error) {
      throw new Error(error.message || '批量丢弃道具失败。')
    }
  }

  async function sellEquipmentsBatch(equipmentIds = []) {
    try {
      const result = await apiClient.sellEquipmentsBatch(equipmentIds)
      await refreshPlayerSnapshot({ includeEquipment: true, includeInventory: true, includeRankings: false })
      return result
    } catch (error) {
      throw new Error(error.message || '批量出售装备失败。')
    }
  }

  async function decomposeEquipments(equipmentIds = []) {
    try {
      const result = await apiClient.decomposeEquipments(equipmentIds)
      await refreshPlayerSnapshot({ includeEquipment: true, includeInventory: true, includeRankings: false })
      return result
    } catch (error) {
      throw new Error(error.message || '装备分解失败。')
    }
  }

  async function sellInventoryItemsBatch(items = []) {
    try {
      const result = await apiClient.sellInventoryItemsBatch(items)
      await refreshPlayerSnapshot({ includeInventory: true, includeRankings: false })
      return result
    } catch (error) {
      throw new Error(error.message || '批量出售道具失败。')
    }
  }

  async function allocateAttributePoint(attributeKey) {
    const { state, loadPlayer } = legacyStore

    try {
      const result = await apiClient.allocateAttributePoint(attributeKey)
      if (state.loaded?.player) {
        await loadPlayer(true)
      }
      return result
    } catch (error) {
      throw new Error(error.message || '属性加点失败。')
    }
  }

  async function refundAttributePoint(attributeKey) {
    const { state, loadPlayer } = legacyStore

    try {
      const result = await apiClient.refundAttributePoint(attributeKey)
      if (state.loaded?.player) {
        await loadPlayer(true)
      }
      return result
    } catch (error) {
      throw new Error(error.message || '返还属性点失败。')
    }
  }

  async function getForgeOverview() {
    try {
      return await apiClient.getForgeOverview()
    } catch (error) {
      throw new Error(error.message || '加载锻造数据失败。')
    }
  }

  async function forgeEquipment(recipeId) {
    try {
      const result = await apiClient.forgeEquipment(recipeId)
      await refreshAfterEquipmentMutation({ includeInventory: true })
      return result
    } catch (error) {
      throw new Error(error.message || '锻造失败。')
    }
  }

  async function collectForgeResult() {
    try {
      const result = await apiClient.collectForgeResult()
      await refreshAfterEquipmentMutation({ includeInventory: true })
      return result
    } catch (error) {
      throw new Error(error.message || '领取锻造结果失败。')
    }
  }

  // ---- 秘境实例 ----
  async function getAvailableDungeonInstances() {
    try {
      return await apiClient.getAvailableDungeonInstances()
    } catch (error) {
      throw new Error(error.message || '加载秘境列表失败。')
    }
  }

  async function enterDungeonInstance(dungeonId) {
    try {
      return await apiClient.enterDungeonInstance(dungeonId)
    } catch (error) {
      throw new Error(error.message || '进入秘境失败。')
    }
  }

  async function getDungeonInstanceStatus() {
    try {
      return await apiClient.getDungeonInstanceStatus()
    } catch (error) {
      throw new Error(error.message || '获取秘境状态失败。')
    }
  }

  async function quitDungeonInstance() {
    try {
      const result = await apiClient.quitDungeonInstance()
      await refreshPlayerSnapshot({ includeInventory: true, includeEquipment: true, includeRankings: true })
      return result
    } catch (error) {
      throw new Error(error.message || '退出秘境失败。')
    }
  }

  async function getDungeonInstanceHistory(limit = 20) {
    try {
      return await apiClient.getDungeonInstanceHistory(limit)
    } catch (error) {
      throw new Error(error.message || '获取秘境历史失败。')
    }
  }

  return {
    ...legacyStore,
    enhanceEquipment,
    bindEquipment,
    unlockEquipment,
    lockItem,
    unlockItem,
    donateEquipment,
    getEquipmentRerollPreview,
    rollEquipmentReroll,
    acceptEquipmentReroll,
    discardEquipmentReroll,
    discardItem,
    discardItemsBatch,
    sellEquipmentsBatch,
    decomposeEquipments,
    sellInventoryItemsBatch,
    allocateAttributePoint,
    refundAttributePoint,
    getForgeOverview,
    forgeEquipment,
    collectForgeResult,
    refreshPlayerSnapshot,
    getAvailableDungeonInstances,
    enterDungeonInstance,
    getDungeonInstanceStatus,
    quitDungeonInstance,
    getDungeonInstanceHistory
  }
}
