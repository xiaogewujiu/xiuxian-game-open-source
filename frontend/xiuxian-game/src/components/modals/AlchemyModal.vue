<template>
  <XiuXianModal :model-value="modelValue" title="炼丹系统" :width="1180" @update:model-value="$emit('update:modelValue', $event)">
    <div class="alchemy-modal">
      <aside class="alchemy-sidebar">
        <section class="alchemy-card summary-card">
          <div class="card-kicker">丹道修为</div>
          <div class="summary-top">
            <div class="level-chip">Lv.{{ alchemyInfo.alchemistLevel }}</div>
            <div class="summary-cap">上限 Lv.{{ alchemyInfo.professionLevelCap }}</div>
          </div>
          <div class="progress-block">
            <div class="progress-label">炼丹经验</div>
            <div class="progress-bar">
              <div class="progress-fill" :style="{ width: alchemyProgressPercent + '%' }"></div>
            </div>
            <div class="progress-value">{{ alchemyInfo.currentLevelExp }}/{{ alchemyInfo.nextLevelExp }}</div>
          </div>
        </section>

        <section class="alchemy-card library-card">
          <div class="panel-head">
            <div>
              <div class="card-kicker">丹方卷册</div>
              <h3 class="section-title">丹方目录</h3>
            </div>
            <div class="panel-tag">{{ recipes.length }} 张</div>
          </div>
          <div class="recipe-list">
            <button
              v-for="recipe in recipes"
              :key="recipe.recipeId"
              type="button"
              class="recipe-item"
              :class="{
                selected: selectedRecipe?.recipeId === recipe.recipeId,
                locked: !recipe.isLearned
              }"
              @click="selectRecipe(recipe.recipeId)"
            >
              <div class="recipe-icon">
                <AssetIcon :source="recipe.iconPath || recipe.icon || recipeIcon(recipe)" :fallback="recipeIcon(recipe)" :alt="recipe.name" size="25" />
              </div>
              <div class="recipe-copy">
                <div class="recipe-header">
                  <span class="recipe-name" :class="recipeQuality(recipe)">{{ recipe.name }}</span>
                  <span class="recipe-state">{{ recipe.isLearned ? '已解锁' : '未解锁' }}</span>
                </div>
                <div class="recipe-desc">{{ recipe.description || '暂无描述' }}</div>
                <div class="recipe-meta">
                  <span>等级 {{ recipe.requiredLevel }}</span>
                  <span>成功率 {{ recipe.actualSuccessRate }}%</span>
                </div>
              </div>
            </button>
          </div>
        </section>
      </aside>

      <section class="alchemy-main">
        <template v-if="selectedRecipe">
          <section class="alchemy-card hero-card">
            <div class="hero-visual" :class="recipeQuality(selectedRecipe)">
              <span class="hero-icon"><AssetIcon :source="selectedRecipe.iconPath || selectedRecipe.icon || recipeIcon(selectedRecipe)" :fallback="recipeIcon(selectedRecipe)" :alt="selectedRecipe.name" size="48" /></span>
            </div>
            <div class="hero-copy">
              <div class="card-kicker">当前丹方</div>
              <div class="hero-title-row">
                <h3 class="hero-title" :class="recipeQuality(selectedRecipe)">{{ selectedRecipe.name }}</h3>
                <span class="hero-state">{{ selectedRecipe.isLearned ? '可炼制' : '未解锁' }}</span>
              </div>
              <p class="hero-desc">{{ selectedRecipe.description || '暂无描述' }}</p>
              <div class="hero-metrics">
                <span class="metric-pill">丹方 Lv.{{ selectedRecipe.requiredLevel }}</span>
                <span class="metric-pill">成功率 {{ actualSuccessRate }}%</span>
                <span class="metric-pill">耗时 {{ Math.ceil((selectedRecipe.actualCraftTimeSeconds || 0) / 60) }} 分钟</span>
              </div>
            </div>
            <div class="hero-actions">
              <div class="hero-note">
                <div v-if="!selectedRecipe.isLearned">
                  使用 {{ selectedRecipe.unlockItemName || '对应配方卷轴' }} 解锁
                </div>
                <div v-if="selectedRecipe.unavailableReason">{{ selectedRecipe.unavailableReason }}</div>
                <div v-if="alchemyInfo.isCrafting && !alchemyInfo.canCollect">剩余时间 {{ craftRemainingText }}</div>
                <div v-if="alchemyInfo.canCollect">炼丹已完成，可领取结果。</div>
              </div>
              <div v-if="!selectedRecipe.isLearned" class="unlock-hint">
                请在背包中使用对应配方卷轴
              </div>
              <button
                v-else
                class="action-btn craft-btn"
                :disabled="!canCraft || isSubmitting"
                @click="startRefine"
              >
                <span v-if="isSubmitting">炼丹中...</span>
                <span v-else-if="alchemyInfo.isCrafting">炼丹进行中</span>
                <span v-else>开始炼丹</span>
              </button>
              <button
                v-if="alchemyInfo.canCollect"
                class="action-btn learn-btn"
                :disabled="isSubmitting"
                @click="collectRefine"
              >
                <span v-if="isSubmitting">领取中...</span>
                <span v-else>领取丹药</span>
              </button>
            </div>
          </section>

          <div v-if="statusMessage" class="status-banner">{{ statusMessage }}</div>

          <div class="alchemy-grid">
            <section class="alchemy-card furnace-card">
              <div class="panel-head">
                <div>
                  <div class="card-kicker">丹火炉心</div>
                  <h3 class="section-title">炼制台</h3>
                </div>
                <div class="panel-tag">即时炼制</div>
              </div>
              <div class="furnace-stage" :class="{ refining: isSubmitting }">
                <div class="furnace-shell"><AssetIcon :source="ICON.nav_alchemy" size="80" /></div>
                <div v-if="isSubmitting" class="flames">
                  <span><AssetIcon :source="ICON.element_fire" size="26" /></span>
                  <span><AssetIcon :source="ICON.element_fire" size="26" /></span>
                  <span><AssetIcon :source="ICON.element_fire" size="26" /></span>
                </div>
                <div v-else class="furnace-preview">
                  <AssetIcon :source="selectedRecipe.iconPath || selectedRecipe.icon || recipeIcon(selectedRecipe)" :fallback="recipeIcon(selectedRecipe)" :alt="selectedRecipe.name" size="40" />
                </div>
              </div>
              <div class="success-panel">
                <div class="progress-label">本次成功率</div>
                <div class="progress-bar">
                  <div class="success-fill" :style="{ width: `${actualSuccessRate}%` }"></div>
                </div>
                <div class="success-value">{{ actualSuccessRate }}%</div>
              </div>
            </section>

            <section class="alchemy-card materials-card">
              <div class="panel-head">
                <div>
                  <div class="card-kicker">炉前备料</div>
                  <h3 class="section-title">所需材料</h3>
                </div>
                <div class="panel-tag">即时校验</div>
              </div>
              <div class="material-list">
                <div
                  v-for="mat in selectedRecipe.materials"
                  :key="mat.itemId"
                  class="material-row"
                  :class="{ enough: hasEnoughMaterial(mat) }"
                >
                  <div class="material-leading">
                    <span class="material-icon"><AssetIcon :source="materialIcon(mat.itemId, mat.icon)" size="25" /></span>
                    <div class="material-copy">
                      <div class="material-name">{{ mat.itemName }}</div>
                      <div class="material-hint">{{ hasEnoughMaterial(mat) ? '材料充足' : '材料不足' }}</div>
                    </div>
                  </div>
                  <div class="material-count">{{ getMaterialCount(mat.itemId) }}/{{ mat.amount }}</div>
                </div>
              </div>
            </section>

            <section class="alchemy-card inventory-card">
              <div class="panel-head">
                <div>
                  <div class="card-kicker">背包摘录</div>
                  <h3 class="section-title">药材库存</h3>
                </div>
                <div class="panel-tag">相关材料</div>
              </div>
              <div class="inventory-list">
                <div v-for="mat in trackedMaterials" :key="mat.itemId" class="inventory-row">
                  <span class="inventory-icon"><AssetIcon :source="materialIcon(mat.itemId, mat.icon)" size="25" /></span>
                  <span class="inventory-name">{{ mat.name }}</span>
                  <span class="inventory-count">x{{ getMaterialCount(mat.itemId) }}</span>
                </div>
              </div>
            </section>
          </div>
        </template>

        <section v-else class="alchemy-card empty-card">
          <div class="empty-orb"><AssetIcon :source="ICON.nav_alchemy" size="46" /></div>
          <div class="empty-text">当前没有已解锁丹方，请在背包中使用配方卷轴。</div>
        </section>
      </section>
    </div>

    <Transition name="fade">
      <div v-if="refineResult.show" class="result-overlay">
        <div class="result-card">
          <div class="result-title" :class="refineResult.success ? 'success' : 'fail'">
            {{ refineResult.success ? '炼丹成功' : '炼丹失败' }}
          </div>
          <div v-if="refineResult.success" class="result-body">
            <div class="result-icon">
              <AssetIcon :source="selectedRecipe?.iconPath || selectedRecipe?.icon || recipeIcon(selectedRecipe || {})" :fallback="recipeIcon(selectedRecipe || {})" :alt="selectedRecipe?.name || '丹药'" size="48" />
            </div>
            <div class="result-name" :class="recipeQuality(selectedRecipe || {})">{{ refineResult.name }}</div>
            <div class="result-message">{{ refineResult.message }}</div>
            <div class="result-count">获得 x{{ refineResult.count }}</div>
          </div>
          <div v-else class="result-body fail">
            <div class="result-icon"><AssetIcon :source="ICON.element_wind" size="48" /></div>
            <div class="result-message">{{ refineResult.message }}</div>
          </div>
          <button class="confirm-btn" @click="closeResult">确定</button>
        </div>
      </div>
    </Transition>
  </XiuXianModal>
</template>

<script>
import { computed, onUnmounted, reactive, ref, watch } from 'vue'
import XiuXianModal from '../common/XiuXianModal.vue'
import AssetIcon from '../common/AssetIcon.vue'
import { ICON } from '../../icons'
import { apiClient } from '../../lib/apiClient'
import { useGameStore } from '../../state/gameStore'

export default {
  name: 'AlchemyModal',

  components: {
    XiuXianModal,
    AssetIcon
  },

  props: {
    modelValue: Boolean
  },

  emits: ['update:modelValue'],

  setup(props) {
    const gameStore = useGameStore()
    // 炼丹弹窗核心状态：
    // recipes 是丹方目录，alchemyInfo 是当前丹道总览，refineResult 是炼丹结果弹层。
    const recipes = ref([])
    const selectedRecipeId = ref('')
    const isSubmitting = ref(false)
    const statusMessage = ref('')
    const alchemyInfo = reactive({
      furnaceLevel: 1,
      alchemistLevel: 1,
      currentLevelExp: 0,
      nextLevelExp: 0,
      professionLevelCap: 1,
      proficiency: 0,
      successRateBonus: 0,
      isCrafting: false,
      activeRecipeId: '',
      activeCraftStartedAt: null,
      activeCraftCompleteAt: null,
      canCollect: false
    })
    const nowTs = ref(Date.now())
    let countdownTimer = null

    const refineResult = reactive({
      show: false,
      success: false,
      name: '',
      message: '',
      count: 0
    })

    // 当前选中的丹方。
    const selectedRecipe = computed(() => recipes.value.find((recipe) => recipe.recipeId === selectedRecipeId.value) || null)
    const inventoryItems = computed(() => gameStore.state.inventory || [])
    const playerGold = computed(() => gameStore.state.player?.gold || 0)
    const alchemyProgressPercent = computed(() => {
      const nextExp = Math.max(1, alchemyInfo.nextLevelExp || 1)
      return Math.min(100, Math.floor((alchemyInfo.currentLevelExp || 0) / nextExp * 100))
    })

    const actualSuccessRate = computed(() => {
      if (!selectedRecipe.value) {
        return 0
      }

      return selectedRecipe.value.actualSuccessRate || selectedRecipe.value.baseSuccessRate || 0
    })
    const craftRemainingText = computed(() => {
      if (!alchemyInfo.activeCraftCompleteAt) {
        return ''
      }

      const end = new Date(alchemyInfo.activeCraftCompleteAt).getTime()
      const remainingSeconds = Math.max(0, Math.ceil((end - nowTs.value) / 1000))
      const minutes = Math.floor(remainingSeconds / 60)
      const seconds = remainingSeconds % 60
      return `${minutes}分${seconds.toString().padStart(2, '0')}秒`
    })

    const trackedMaterials = computed(() => {
      const materialMap = new Map()
      recipes.value.forEach((recipe) => {
        ;(recipe.materials || []).forEach((material) => {
          if (!materialMap.has(material.itemId)) {
            materialMap.set(material.itemId, {
              itemId: material.itemId,
              name: material.itemName,
              icon: material.icon || ''
            })
          }
        })
      })

      return Array.from(materialMap.values())
    })

    const canCraft = computed(() => {
      if (!selectedRecipe.value || !selectedRecipe.value.isLearned) {
        return false
      }

      if (alchemyInfo.isCrafting) {
        return false
      }

      return Boolean(selectedRecipe.value.canCraft) &&
        selectedRecipe.value.materials.every((material) => hasEnoughMaterial(material))
    })

    // 把后端炼丹配方 DTO 统一折成当前弹窗使用的字段结构。
    function normalizeRecipe(recipe) {
      return {
        recipeId: recipe.recipeId || recipe.RecipeId,
        pillTemplateId: recipe.pillTemplateId || recipe.PillTemplateId,
        name: recipe.name || recipe.Name,
        iconPath: recipe.iconPath || recipe.IconPath || '',
        description: recipe.description || recipe.Description || '',
        requiredLevel: recipe.requiredLevel || recipe.RequiredLevel || recipe.requiredFurnaceLevel || recipe.RequiredFurnaceLevel || 1,
        requiredFurnaceLevel: recipe.requiredFurnaceLevel || recipe.RequiredFurnaceLevel || 1,
        baseSuccessRate: recipe.baseSuccessRate || recipe.BaseSuccessRate || 0,
        baseCraftTime: recipe.baseCraftTime || recipe.BaseCraftTime || 0,
        actualCraftTimeSeconds: recipe.actualCraftTimeSeconds || recipe.ActualCraftTimeSeconds || recipe.baseCraftTime || recipe.BaseCraftTime || 0,
        actualSuccessRate: recipe.actualSuccessRate || recipe.ActualSuccessRate || recipe.baseSuccessRate || recipe.BaseSuccessRate || 0,
        successRateBonus: recipe.successRateBonus || recipe.SuccessRateBonus || 0,
        canCraft: Boolean(recipe.canCraft ?? recipe.CanCraft),
        unavailableReason: recipe.unavailableReason || recipe.UnavailableReason || '',
        isLearned: Boolean(recipe.isLearned ?? recipe.IsLearned),
        unlockItemId: recipe.unlockItemId || recipe.UnlockItemId || '',
        unlockItemName: recipe.unlockItemName || recipe.UnlockItemName || '',
        materials: Array.isArray(recipe.materials || recipe.Materials)
          ? (recipe.materials || recipe.Materials).map((material) => ({
            itemId: material.itemId || material.ItemId,
            itemName: material.itemName || material.ItemName,
            amount: material.amount || material.Amount || 0,
            icon: material.icon || material.Icon || ''
          }))
          : []
      }
    }

    // 根据丹药模板编号推导前端展示图标。
    function recipeIcon(recipe) {
      const pillTemplateId = recipe?.pillTemplateId || ''
      if (pillTemplateId.includes('mp')) return ICON.stat_mp
      if (pillTemplateId.includes('large')) return ICON.currency_spirit
      return ICON.item_potion
    }

    // 根据丹药模板编号推导前端品质 class。
    function recipeQuality(recipe) {
      const pillTemplateId = recipe?.pillTemplateId || ''
      if (pillTemplateId.includes('large')) return 'epic'
      if (pillTemplateId.includes('mp')) return 'rare'
      return 'common'
    }

    // 当前版本里把常见药材先映射成固定图标。
    function materialIcon(itemId, icon) {
      if (icon) return icon
      return {
        alchemy_herb: ICON.item_seed,
        spirit_water: ICON.stat_mp,
        spirit_dust: ICON.currency_exp,
        beast_core: ICON.slot_treasure
      }[itemId] || ICON.item_chest
    }

    // 选择当前丹方。
    function selectRecipe(recipeId) {
      if (isSubmitting.value) {
        return
      }

      selectedRecipeId.value = recipeId
      statusMessage.value = ''
    }

    // 读取指定材料当前持有数量。
    function getMaterialCount(itemId) {
      return inventoryItems.value
        .filter((item) => item.itemId === itemId)
        .reduce((sum, item) => sum + (item.quantity || 0), 0)
    }

    // 当前材料是否足够。
    function hasEnoughMaterial(material) {
      return getMaterialCount(material.itemId) >= material.amount
    }

    // 读取炼丹总览、丹方列表和背包材料。
    async function loadAlchemyData() {
      statusMessage.value = ''

      try {
        const [info, recipeList] = await Promise.all([
          apiClient.getAlchemyInfo(),
          apiClient.getAlchemyRecipes(),
          gameStore.loadInventory(true),
          gameStore.loadPlayer(true)
        ])

        alchemyInfo.furnaceLevel = info?.furnaceLevel ?? info?.FurnaceLevel ?? 1
        alchemyInfo.alchemistLevel = info?.alchemistLevel ?? info?.AlchemistLevel ?? 1
        alchemyInfo.currentLevelExp = info?.currentLevelExp ?? info?.CurrentLevelExp ?? 0
        alchemyInfo.nextLevelExp = info?.nextLevelExp ?? info?.NextLevelExp ?? 0
        alchemyInfo.professionLevelCap = info?.professionLevelCap ?? info?.ProfessionLevelCap ?? alchemyInfo.alchemistLevel
        alchemyInfo.proficiency = info?.proficiency ?? info?.Proficiency ?? 0
        alchemyInfo.successRateBonus = info?.successRateBonus ?? info?.SuccessRateBonus ?? 0
        alchemyInfo.isCrafting = Boolean(info?.isCrafting ?? info?.IsCrafting)
        alchemyInfo.activeRecipeId = info?.activeRecipeId ?? info?.ActiveRecipeId ?? ''
        alchemyInfo.activeCraftStartedAt = info?.activeCraftStartedAt ?? info?.ActiveCraftStartedAt ?? null
        alchemyInfo.activeCraftCompleteAt = info?.activeCraftCompleteAt ?? info?.ActiveCraftCompleteAt ?? null
        alchemyInfo.canCollect = Boolean(info?.canCollect ?? info?.CanCollect)
        recipes.value = (Array.isArray(recipeList) ? recipeList : []).map(normalizeRecipe)

        if (!selectedRecipeId.value || !recipes.value.find((recipe) => recipe.recipeId === selectedRecipeId.value)) {
          selectedRecipeId.value = recipes.value.find((recipe) => recipe.isLearned)?.recipeId || recipes.value[0]?.recipeId || ''
        }
      } catch (error) {
        recipes.value = []
        selectedRecipeId.value = ''
        statusMessage.value = error.message || '加载炼丹数据失败。'
      }
    }

    async function refreshAfterAlchemyMutation(message) {
      await Promise.allSettled([
        loadAlchemyData(),
        gameStore.loadInventory(true),
        gameStore.loadPlayer(true)
      ])

      if (message) {
        statusMessage.value = message
      }
    }

    // 开始炼丹。
    async function startRefine() {
      if (!selectedRecipe.value || !canCraft.value || isSubmitting.value) {
        return
      }

      isSubmitting.value = true

      try {
        const result = await apiClient.craftAlchemy(selectedRecipe.value.recipeId, 1)
        const requiresCollection = Boolean(result?.requiresCollection ?? result?.RequiresCollection)
        if (requiresCollection) {
          await refreshAfterAlchemyMutation(result?.message || result?.Message || '开始炼丹。')
          return
        }

        refineResult.success = Boolean(result?.success ?? result?.Success)
        refineResult.name = result?.pillName || result?.PillName || selectedRecipe.value.name
        refineResult.message = result?.message || result?.Message || (refineResult.success ? '炼丹成功。' : '炼丹失败。')
        refineResult.count = result?.pillQuantity || result?.PillQuantity || 0
        refineResult.show = true
        await refreshAfterAlchemyMutation(refineResult.message)
      } catch (error) {
        statusMessage.value = error.message || '炼丹失败。'
      } finally {
        isSubmitting.value = false
      }
    }

    // 领取炼丹结果。
    async function collectRefine() {
      if (isSubmitting.value || !alchemyInfo.canCollect) {
        return
      }

      isSubmitting.value = true
      try {
        const result = await apiClient.collectAlchemy()
        refineResult.success = Boolean(result?.success ?? result?.Success)
        refineResult.name = result?.pillName || result?.PillName || selectedRecipe.value?.name || ''
        refineResult.message = result?.message || result?.Message || (refineResult.success ? '炼丹完成。' : '炼丹失败。')
        refineResult.count = result?.pillQuantity || result?.PillQuantity || 0
        refineResult.show = true
        await refreshAfterAlchemyMutation(refineResult.message)
      } catch (error) {
        statusMessage.value = error.message || '领取炼丹结果失败。'
      } finally {
        isSubmitting.value = false
      }
    }

    function syncCountdownTimer() {
      if (countdownTimer) {
        clearInterval(countdownTimer)
        countdownTimer = null
      }

      if (!alchemyInfo.isCrafting) {
        return
      }

      countdownTimer = window.setInterval(() => {
        nowTs.value = Date.now()
      }, 1000)
    }

    function closeResult() {
      refineResult.show = false
    }

    watch(() => props.modelValue, async (visible) => {
      if (!visible) {
        if (countdownTimer) {
          clearInterval(countdownTimer)
          countdownTimer = null
        }
        return
      }

      await loadAlchemyData()
    }, { immediate: true })

    watch(() => [alchemyInfo.isCrafting, alchemyInfo.activeCraftCompleteAt], () => {
      syncCountdownTimer()
    })

    onUnmounted(() => {
      if (countdownTimer) {
        clearInterval(countdownTimer)
      }
    })

    return {
      recipes,
      selectedRecipe,
      trackedMaterials,
      alchemyInfo,
      alchemyProgressPercent,
      isSubmitting,
      refineResult,
      statusMessage,
      playerGold,
      actualSuccessRate,
      craftRemainingText,
      canCraft,
      ICON,
      recipeIcon,
      recipeQuality,
      materialIcon,
      selectRecipe,
      getMaterialCount,
      hasEnoughMaterial,
      startRefine,
      collectRefine,
      closeResult
    }
  }
}
</script>

<style scoped>
.alchemy-modal {
  display: flex;
  gap: var(--spacing-lg);
  min-height: 560px;
}

.alchemy-sidebar {
  width: 280px;
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}

.alchemy-main {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
  min-width: 0;
}

.alchemy-card {
  background: var(--xiuxian-bg-secondary);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  padding: var(--spacing-md);
}

.card-kicker {
  font-size: 12px;
  color: var(--highlight-text);
  margin-bottom: 6px;
}

.section-title {
  margin: 0;
  font-size: var(--font-size-base);
  font-weight: 600;
  color: var(--accent-text);
}

.panel-head {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: var(--spacing-sm);
  margin-bottom: var(--spacing-md);
}

.panel-tag {
  color: var(--text-muted);
  font-size: var(--font-size-xs);
}

.summary-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--spacing-sm);
  margin-bottom: var(--spacing-md);
}

.level-chip {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 90px;
  padding: 10px 14px;
  border-radius: var(--radius-md);
  background: rgba(216, 94, 43, 0.3);
  border: 1px solid rgba(216, 94, 43, 0.5);
  color: var(--highlight-text);
  font-size: 18px;
  font-weight: 700;
}

.summary-cap {
  color: var(--text-secondary);
  font-size: var(--font-size-sm);
}

.progress-block {
  display: flex;
  flex-direction: column;
  gap: 6px;
  margin-bottom: var(--spacing-md);
}

.progress-label {
  color: var(--text-muted);
  font-size: var(--font-size-xs);
}

.progress-bar {
  height: 8px;
  background: var(--xiuxian-bg-primary);
  border-radius: 999px;
  overflow: hidden;
}

.progress-fill,
.success-fill {
  height: 100%;
  border-radius: inherit;
}

.progress-fill {
  background: linear-gradient(90deg, #d94c31, #ffb454);
}

.progress-value {
  color: var(--text-secondary);
  font-size: var(--font-size-xs);
  font-family: var(--font-mono);
}

.summary-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: var(--spacing-sm);
}

.summary-item {
  padding: var(--spacing-sm);
  background: var(--xiuxian-bg-primary);
  border-radius: var(--radius-sm);
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.summary-item.wide {
  grid-column: 1 / -1;
}

.summary-label {
  color: var(--text-secondary);
  font-size: var(--font-size-xs);
}

.summary-value {
  color: var(--highlight-text);
  font-size: 18px;
  font-weight: 700;
}

.summary-value.gold {
  color: var(--highlight-text);
}

.library-card {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-height: 0;
}

.recipe-list {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
  overflow-y: auto;
}

.recipe-item {
  width: 100%;
  display: flex;
  gap: var(--spacing-sm);
  padding: var(--spacing-sm);
  background: var(--xiuxian-bg-primary);
  border: 1px solid transparent;
  border-radius: var(--radius-sm);
  cursor: pointer;
  text-align: left;
  transition: all 0.25s ease;
}

.recipe-item:hover {
  border-color: var(--border-color);
}

.recipe-item.selected {
  border-color: var(--accent-text);
  background: rgba(124, 58, 237, 0.10);
}

.recipe-item.locked {
  opacity: 0.8;
}

.recipe-icon {
  width: 46px;
  height: 46px;
  border-radius: var(--radius-sm);
  background: rgba(255, 255, 255, 0.06);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 24px;
  flex-shrink: 0;
}

.recipe-copy {
  flex: 1;
  min-width: 0;
}

.recipe-header {
  display: flex;
  justify-content: space-between;
  gap: var(--spacing-xs);
  align-items: flex-start;
  margin-bottom: 4px;
}

.recipe-name {
  font-size: 15px;
  font-weight: 600;
}

.recipe-name.common { color: var(--quality-common); }
.recipe-name.rare { color: var(--quality-rare); }
.recipe-name.epic { color: var(--quality-epic); }

.recipe-state {
  padding: 2px 8px;
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.06);
  color: var(--text-muted);
  font-size: 10px;
  flex-shrink: 0;
}

.recipe-desc {
  color: var(--text-secondary);
  font-size: var(--font-size-xs);
  line-height: 1.5;
  margin-bottom: 4px;
}

.recipe-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  color: var(--text-muted);
  font-size: 11px;
}

.hero-card {
  display: flex;
  gap: var(--spacing-md);
  align-items: center;
}

.hero-visual {
  width: 96px;
  height: 96px;
  border-radius: var(--radius-md);
  background: rgba(255, 255, 255, 0.04);
  border: 1px solid var(--border-color);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.hero-icon {
  font-size: 48px;
}

.hero-copy {
  flex: 1;
  min-width: 0;
}

.hero-title-row {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  margin-bottom: var(--spacing-sm);
}

.hero-title {
  margin: 0;
  font-size: 24px;
  font-weight: 700;
}

.hero-title.common { color: var(--quality-common); }
.hero-title.rare { color: var(--quality-rare); }
.hero-title.epic { color: var(--quality-epic); }

.hero-state {
  padding: 3px 10px;
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.06);
  color: var(--text-secondary);
  font-size: 12px;
}

.hero-desc {
  margin: 0 0 var(--spacing-sm);
  color: var(--text-secondary);
  line-height: 1.6;
}

.hero-metrics {
  display: flex;
  flex-wrap: wrap;
  gap: var(--spacing-xs);
}

.metric-pill {
  padding: 4px 10px;
  border-radius: 999px;
  background: var(--xiuxian-bg-primary);
  color: var(--text-secondary);
  font-size: 12px;
}

.hero-actions {
  width: 220px;
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
  flex-shrink: 0;
}

.hero-note {
  color: var(--text-secondary);
  font-size: var(--font-size-xs);
  line-height: 1.6;
}

.action-btn {
  width: 100%;
  padding: 12px;
  border: none;
  border-radius: var(--radius-sm);
  color: #fff;
  font-size: 15px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.25s ease;
}

.craft-btn {
  background: linear-gradient(135deg, #d04b2d, #ff8145);
}

.learn-btn {
  background: linear-gradient(135deg, #a26a12, #d8a347);
}

.action-btn:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 10px 20px rgba(0, 0, 0, 0.16);
}

.action-btn:disabled {
  background: var(--xiuxian-bg-primary);
  color: var(--text-muted);
  cursor: not-allowed;
}

.status-banner {
  padding: var(--spacing-sm) var(--spacing-md);
  background: var(--accent-soft-bg);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-sm);
  color: var(--text-secondary);
}

.alchemy-grid {
  display: grid;
  grid-template-columns: 260px minmax(0, 1fr) 240px;
  gap: var(--spacing-md);
}

.furnace-card,
.materials-card,
.inventory-card {
  display: flex;
  flex-direction: column;
}

.furnace-stage {
  position: relative;
  min-height: 240px;
  background: var(--xiuxian-bg-primary);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: var(--spacing-md);
  overflow: hidden;
}

.furnace-stage.refining {
  box-shadow: 0 0 24px rgba(255, 126, 70, 0.15);
}

.furnace-shell {
  font-size: 82px;
  opacity: 0.9;
}

.furnace-preview {
  position: absolute;
  top: 24px;
  font-size: 34px;
  animation: float 2.2s ease-in-out infinite;
}

.flames {
  position: absolute;
  bottom: 30px;
  display: flex;
  gap: 8px;
  font-size: 26px;
}

.flames span {
  animation: fire 0.35s ease-in-out infinite alternate;
}

.flames span:nth-child(2) { animation-delay: 0.1s; }
.flames span:nth-child(3) { animation-delay: 0.2s; }

.success-panel {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.success-fill {
  background: linear-gradient(90deg, #d94c31, #ffb454, #5ebf67);
}

.success-value {
  color: var(--highlight-text);
  font-size: 20px;
  font-weight: 700;
}

.material-list,
.inventory-list {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
}

.material-row,
.inventory-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--spacing-sm);
  padding: var(--spacing-sm);
  border-radius: var(--radius-sm);
  background: var(--xiuxian-bg-primary);
  border: 1px solid transparent;
}

.material-row.enough {
  border-color: rgba(76, 175, 80, 0.28);
}

.material-leading {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  min-width: 0;
}

.material-icon,
.inventory-icon {
  width: 34px;
  height: 34px;
  border-radius: var(--radius-sm);
  background: rgba(255, 255, 255, 0.06);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 18px;
  flex-shrink: 0;
}

.material-copy {
  min-width: 0;
}

.material-name,
.inventory-name {
  color: var(--text-primary);
  font-size: 14px;
  font-weight: 600;
}

.material-hint {
  color: var(--text-muted);
  font-size: 11px;
  margin-top: 2px;
}

.material-count,
.inventory-count {
  color: var(--highlight-text);
  font-family: var(--font-mono);
  white-space: nowrap;
  flex-shrink: 0;
}

.empty-card {
  min-height: 420px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: var(--spacing-md);
}

.empty-orb {
  width: 92px;
  height: 92px;
  border-radius: var(--radius-lg);
  background: var(--xiuxian-bg-primary);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 46px;
}

.empty-text {
  color: var(--text-secondary);
}

.result-overlay {
  position: fixed;
  inset: 0;
  background: var(--overlay-bg);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 2000;
}

.result-card {
  min-width: 320px;
  background: var(--xiuxian-bg-panel);
  border: 2px solid var(--accent-text);
  border-radius: var(--radius-lg);
  padding: var(--spacing-xl);
  text-align: center;
  box-shadow: var(--shadow-sm);
}

.result-title {
  font-size: 22px;
  font-weight: 700;
  margin-bottom: var(--spacing-lg);
}

.result-title.success { color: var(--quality-uncommon); }
.result-title.fail { color: var(--hp-color); }

.result-body {
  margin-bottom: var(--spacing-lg);
}

.result-body.fail {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
}

.result-icon {
  font-size: 64px;
  margin-bottom: var(--spacing-md);
}

.result-name {
  font-size: 18px;
  font-weight: 700;
  margin-bottom: var(--spacing-sm);
}

.result-name.common { color: var(--quality-common); }
.result-name.rare { color: var(--quality-rare); }
.result-name.epic { color: var(--quality-epic); }

.result-message {
  color: var(--text-secondary);
}

.result-count {
  margin-top: 8px;
  color: var(--highlight-text);
  font-weight: 700;
}

.confirm-btn {
  min-width: 132px;
  padding: 10px 22px;
  border: none;
  border-radius: var(--radius-sm);
  background: linear-gradient(135deg, var(--button-primary-start), var(--button-primary-end));
  color: var(--button-primary-text);
  font-size: 14px;
  font-weight: 700;
  cursor: pointer;
}

.confirm-btn:hover {
  box-shadow: var(--shadow-sm);
}

.fade-enter-active,
.fade-leave-active {
  transition: all 0.25s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

@keyframes float {
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(-8px); }
}

@keyframes fire {
  from { transform: scaleY(1) scaleX(1); }
  to { transform: scaleY(1.2) scaleX(0.92); }
}

@media (max-width: 1080px) {
  .alchemy-modal {
    flex-direction: column;
  }

  .alchemy-sidebar {
    width: 100%;
  }

  .alchemy-grid {
    grid-template-columns: 1fr;
  }

  .hero-card {
    flex-direction: column;
    align-items: flex-start;
  }

  .hero-actions {
    width: 100%;
  }
}

@media (max-width: 720px) {
  .summary-grid {
    grid-template-columns: 1fr;
  }

  .summary-item.wide {
    grid-column: auto;
  }
}
</style>
