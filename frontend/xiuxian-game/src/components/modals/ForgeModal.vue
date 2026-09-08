<template>
  <XiuXianModal :model-value="modelValue" title="锻造系统" :width="1180" @update:model-value="$emit('update:modelValue', $event)">
    <div class="forge-modal">
      <aside class="forge-sidebar">
        <section class="forge-card summary-card">
          <div class="card-kicker">锻造总览</div>
          <div class="summary-top">
            <div class="level-chip">Lv.{{ forgeInfo.blacksmithLevel }}</div>
            <div class="summary-cap">上限 Lv.{{ forgeInfo.professionLevelCap }}</div>
          </div>
          <div class="progress-block">
            <div class="progress-label">锻造经验</div>
            <div class="progress-bar">
              <div class="progress-fill" :style="{ width: forgeProgressPercent + '%' }"></div>
            </div>
            <div class="progress-value">{{ forgeInfo.currentLevelExp }}/{{ forgeInfo.nextLevelExp }}</div>
          </div>
        </section>

        <section class="forge-card library-card">
          <div class="panel-head">
            <div>
              <div class="card-kicker">图纸列表</div>
              <h3 class="section-title">锻造图纸</h3>
            </div>
            <div class="panel-tag">{{ blueprints.length }} 张</div>
          </div>
          <div class="blueprint-list">
            <button
              v-for="bp in blueprints"
              :key="bp.id"
              type="button"
              class="blueprint-item"
              :class="{ selected: selectedBlueprint?.id === bp.id }"
              @click="selectBlueprint(bp)"
            >
              <div class="blueprint-icon">
                <AssetIcon :source="bp.icon" :fallback="ICON.nav_forge" :alt="bp.name" size="25" />
              </div>
              <div class="blueprint-copy">
                <div class="blueprint-header">
                  <span class="blueprint-name" :class="bp.quality">{{ bp.name }}</span>
                  <span class="blueprint-state">{{ bp.canForge ? '可打造' : '受限' }}</span>
                </div>
                <div class="blueprint-desc">{{ bp.description || '暂无描述' }}</div>
                <div class="blueprint-meta">
                  <span>等级 {{ bp.level }}</span>
                  <span>成功率 {{ bp.actualSuccessRate }}%</span>
                </div>
              </div>
            </button>
          </div>
        </section>
      </aside>

      <section class="forge-main">
        <template v-if="selectedBlueprint">
          <section class="forge-card hero-card">
            <div class="hero-visual">
              <span class="hero-icon"><AssetIcon :source="selectedBlueprint.icon" :fallback="ICON.nav_forge" :alt="selectedBlueprint.name" size="48" /></span>
            </div>
            <div class="hero-copy">
              <div class="card-kicker">当前图纸</div>
              <div class="hero-title-row">
                <h3 class="hero-title" :class="selectedBlueprint.quality">{{ selectedBlueprint.name }}</h3>
                <span class="hero-state">{{ selectedBlueprint.canForge ? '可锻造' : '条件不足' }}</span>
              </div>
              <p class="hero-desc">{{ selectedBlueprint.description || '暂无描述' }}</p>
              <div class="hero-metrics">
                <span class="metric-pill">图纸等级 Lv.{{ selectedBlueprint.level }}</span>
                <span class="metric-pill">成功率 {{ selectedBlueprint.actualSuccessRate }}%</span>
                <span class="metric-pill">{{ selectedBlueprint.slotName }}</span>
                <span class="metric-pill">耗时 {{ Math.ceil((selectedBlueprint.craftTimeSeconds || 0) / 60) }} 分钟</span>
              </div>
            </div>
            <div class="hero-actions">
              <div class="hero-note">
                <div>金币消耗 {{ selectedBlueprint.costGold }}</div>
                <div v-if="selectedBlueprint.unavailableReason">{{ selectedBlueprint.unavailableReason }}</div>
                <div v-if="forgeInfo.isForging && !forgeInfo.canCollect">剩余时间 {{ forgeRemainingText }}</div>
                <div v-if="forgeInfo.canCollect">锻造已完成，可领取结果。</div>
              </div>
              <button
                class="action-btn forge-btn"
                :disabled="!canForge || isForging"
                @click="startForge"
              >
                <span v-if="isForging">锻造中...</span>
                <span v-else-if="forgeInfo.isForging">锻造进行中</span>
                <span v-else>开始锻造</span>
              </button>
              <button
                v-if="forgeInfo.canCollect"
                class="action-btn forge-btn"
                :disabled="isForging"
                @click="collectForge"
              >
                <span v-if="isForging">领取中...</span>
                <span v-else>领取装备</span>
              </button>
            </div>
          </section>

          <div class="forge-grid">
            <section class="forge-card platform-card">
              <div class="panel-head">
                <div>
                  <div class="card-kicker">锻造台</div>
                  <h3 class="section-title">打造台</h3>
                </div>
                <div class="panel-tag">成功率 {{ selectedBlueprint.actualSuccessRate }}%</div>
              </div>
              <div class="forge-stage" :class="{ forging: isForging }">
                <div class="forge-shell"><AssetIcon :source="ICON.nav_forge" size="80" /></div>
                <div v-if="isForging" class="forge-effects">
                  <span class="spark"><AssetIcon :source="ICON.currency_exp" size="20" /></span>
                  <span class="spark"><AssetIcon :source="ICON.currency_exp" size="20" /></span>
                  <span class="spark"><AssetIcon :source="ICON.currency_exp" size="20" /></span>
                  <span class="hammer"><AssetIcon :source="ICON.nav_forge" size="35" /></span>
                </div>
                <div v-else class="forge-preview">
                  <AssetIcon :source="selectedBlueprint.icon" :fallback="ICON.nav_forge" :alt="selectedBlueprint.name" size="40" />
                </div>
              </div>
              <div class="success-panel">
                <div class="progress-label">本次成功率</div>
                <div class="progress-bar">
                  <div class="success-fill" :style="{ width: `${selectedBlueprint.actualSuccessRate}%` }"></div>
                </div>
                <div class="success-value">{{ selectedBlueprint.actualSuccessRate }}%</div>
              </div>
            </section>

            <section class="forge-card materials-card">
              <div class="panel-head">
                <div>
                  <div class="card-kicker">所需资源</div>
                  <h3 class="section-title">打造材料</h3>
                </div>
                <div class="panel-tag">即时校验</div>
              </div>
              <div class="material-list">
                <div class="material-row" :class="{ enough: playerGold >= selectedBlueprint.costGold }">
                  <div class="material-leading">
                    <span class="material-icon"><AssetIcon :source="ICON.currency_gold" size="25" /></span>
                    <div class="material-copy">
                      <div class="material-name">金币</div>
                      <div class="material-hint">{{ playerGold >= selectedBlueprint.costGold ? '资源充足' : '金币不足' }}</div>
                    </div>
                  </div>
                  <div class="material-count">{{ playerGold }}/{{ selectedBlueprint.costGold }}</div>
                </div>
                <div
                  v-for="mat in selectedBlueprint.materials"
                  :key="mat.itemId || mat.name"
                  class="material-row"
                  :class="{ enough: hasEnoughMaterial(mat) }"
                >
                  <div class="material-leading">
                    <span class="material-icon">
                      <AssetIcon :source="mat.icon" :fallback="ICON.item_chest" :alt="mat.name" size="25" />
                    </span>
                    <div class="material-copy">
                      <div class="material-name">{{ mat.name }}</div>
                      <div class="material-hint">{{ hasEnoughMaterial(mat) ? '材料充足' : '材料不足' }}</div>
                    </div>
                  </div>
                  <div class="material-count">{{ getMaterialCount(mat.itemId) }}/{{ mat.count }}</div>
                </div>
              </div>
            </section>

            <section class="forge-card inventory-card">
              <div class="panel-head">
                <div>
                  <div class="card-kicker">相关库存</div>
                  <h3 class="section-title">背包材料</h3>
                </div>
                <div class="panel-tag">图纸相关</div>
              </div>
              <div class="inventory-list">
                <div v-for="mat in myMaterials" :key="mat.itemId || mat.name" class="inventory-row">
                  <span class="inventory-icon">
                    <AssetIcon :source="mat.icon" :fallback="ICON.item_chest" :alt="mat.name" size="25" />
                  </span>
                  <span class="inventory-name">{{ mat.name }}</span>
                  <span class="inventory-count">x{{ mat.count }}</span>
                </div>
              </div>
            </section>
          </div>
        </template>

        <section v-else class="forge-card empty-card">
          <div class="empty-orb"><AssetIcon :source="ICON.nav_forge" size="46" /></div>
          <div class="empty-text">当前没有可用图纸。</div>
        </section>
      </section>
    </div>

    <Transition name="fade">
      <div v-if="forgeResult.show" class="result-overlay">
        <div class="result-card">
          <div class="result-title">{{ forgeResult.success ? '锻造成功' : '锻造失败' }}</div>
          <div v-if="forgeResult.item" class="result-body">
            <div class="result-icon">
              <AssetIcon :source="forgeResult.item.icon" :fallback="ICON.nav_forge" :alt="forgeResult.item.name" size="48" />
            </div>
            <div class="result-name" :class="forgeResult.item.quality">{{ forgeResult.item.name }}</div>
            <div class="result-stats">
              <div v-for="stat in forgeResult.item.stats" :key="stat.name">{{ stat.name }}: +{{ stat.value }}</div>
            </div>
          </div>
          <div v-else class="result-body fail">
            <div class="result-icon"><AssetIcon :source="ICON.nav_forge" size="48" /></div>
            <div class="result-message">{{ forgeResult.message }}</div>
          </div>
          <button class="confirm-btn" @click="closeResult">确定</button>
        </div>
      </div>
    </Transition>
  </XiuXianModal>
</template>

<script>
import { ref, reactive, computed, watch, onUnmounted } from 'vue'
import XiuXianModal from '../common/XiuXianModal.vue'
import AssetIcon from '../common/AssetIcon.vue'
import { ICON } from '../../icons'
import { useGameStore } from '../../state/gameStore'
import { buildEquipmentCard, getItemIconByType, getQualityClass, resolveDisplayIcon } from '../../services/gameDisplay'
import toast from '@/utils/toast'

export default {
  name: 'ForgeModal',

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
    // 左侧图纸列表、右侧详情和顶部总览都依赖这几份核心状态。
    const blueprints = ref([])
    const selectedBlueprint = ref(null)
    const myMaterials = ref([])
    const isForging = ref(false)
    const playerGold = computed(() => gameStore.state.player?.gold || 0)
    const forgeInfo = reactive({
      blacksmithLevel: 1,
      currentLevelExp: 0,
      nextLevelExp: 0,
      professionLevelCap: 1,
      isForging: false,
      activeRecipeId: '',
      activeForgeStartedAt: null,
      activeForgeCompleteAt: null,
      canCollect: false
    })
    const forgeProgressPercent = computed(() => {
      const nextExp = Math.max(1, forgeInfo.nextLevelExp || 1)
      return Math.min(100, Math.floor((forgeInfo.currentLevelExp || 0) / nextExp * 100))
    })
    const nowTs = ref(Date.now())
    let countdownTimer = null
    const forgeRemainingText = computed(() => {
      if (!forgeInfo.activeForgeCompleteAt) {
        return ''
      }

      const end = new Date(forgeInfo.activeForgeCompleteAt).getTime()
      const remainingSeconds = Math.max(0, Math.ceil((end - nowTs.value) / 1000))
      const minutes = Math.floor(remainingSeconds / 60)
      const seconds = remainingSeconds % 60
      return `${minutes}分${seconds.toString().padStart(2, '0')}秒`
    })

    const forgeResult = reactive({
      show: false,
      success: false,
      item: null,
      message: ''
    })

    // 读取锻造总览时，一次把图纸、职业等级、进行中的任务和可领取状态全部同步到前端。
    const loadForgeOverview = async () => {
      const [overview] = await Promise.all([
        gameStore.getForgeOverview(),
        gameStore.loadPlayer(true)
      ])

      forgeInfo.blacksmithLevel = overview?.blacksmithLevel || overview?.BlacksmithLevel || 1
      forgeInfo.currentLevelExp = overview?.currentLevelExp || overview?.CurrentLevelExp || 0
      forgeInfo.nextLevelExp = overview?.nextLevelExp || overview?.NextLevelExp || 0
      forgeInfo.professionLevelCap = overview?.professionLevelCap || overview?.ProfessionLevelCap || forgeInfo.blacksmithLevel
      forgeInfo.isForging = Boolean(overview?.isForging ?? overview?.IsForging)
      forgeInfo.activeRecipeId = overview?.activeRecipeId ?? overview?.ActiveRecipeId ?? ''
      forgeInfo.activeForgeStartedAt = overview?.activeForgeStartedAt ?? overview?.ActiveForgeStartedAt ?? null
      forgeInfo.activeForgeCompleteAt = overview?.activeForgeCompleteAt ?? overview?.ActiveForgeCompleteAt ?? null
      forgeInfo.canCollect = Boolean(overview?.canCollect ?? overview?.CanCollect)

      blueprints.value = Array.isArray(overview?.recipes || overview?.Recipes)
        ? (overview.recipes || overview.Recipes).map((recipe) => ({
          id: recipe.recipeId || recipe.RecipeId,
          recipeId: recipe.recipeId || recipe.RecipeId,
          templateId: recipe.templateId || recipe.TemplateId,
          name: recipe.name || recipe.Name,
          icon: resolveDisplayIcon(recipe.icon || recipe.Icon, ICON.nav_forge),
          quality: getQualityClass(recipe.quality || recipe.Quality || 1),
          description: recipe.description || recipe.Description || '暂无描述',
          slotName: recipe.slotName || recipe.SlotName || '装备',
          level: recipe.level || recipe.Level || 1,
          costGold: recipe.costGold || recipe.CostGold || 0,
          craftTimeSeconds: recipe.craftTimeSeconds || recipe.CraftTimeSeconds || 0,
          actualSuccessRate: recipe.actualSuccessRate || recipe.ActualSuccessRate || recipe.successRate || recipe.SuccessRate || 100,
          canForge: Boolean(recipe.canForge ?? recipe.CanForge),
          unavailableReason: recipe.unavailableReason || recipe.UnavailableReason || '',
          materials: Array.isArray(recipe.materials || recipe.Materials)
            ? (recipe.materials || recipe.Materials).map((material) => ({
              itemId: material.itemId || material.ItemId,
              name: material.name || material.Name,
              icon: resolveDisplayIcon(
                material.icon || material.Icon,
                getItemIconByType('material', material.name || material.Name)
              ),
              count: material.count || material.Count || 0
            }))
            : []
        }))
        : []

      myMaterials.value = Array.isArray(overview?.materials || overview?.Materials)
        ? (overview.materials || overview.Materials).map((material) => ({
          id: material.id || material.Id,
          itemId: material.itemId || material.ItemId,
          name: material.name || material.Name,
          icon: resolveDisplayIcon(
            material.icon || material.Icon,
            getItemIconByType(material.itemType || material.ItemType, material.name || material.Name)
          ),
          count: material.quantity || material.Quantity || 0
        }))
        : []

      // 如果当前已有选中图纸，则优先保持选中；
      // 否则默认选第一张图纸，保证右侧详情区总是有内容。
      if (!selectedBlueprint.value) {
        selectedBlueprint.value = blueprints.value[0] || null
      } else {
        const latestSelected = blueprints.value.find((item) => item.id === selectedBlueprint.value?.id)
        selectedBlueprint.value = latestSelected || blueprints.value[0] || null
      }
    }

    watch(() => props.modelValue, async (visible) => {
      if (!visible) {
        return
      }

      try {
        await loadForgeOverview()
      } catch (error) {
        toast.error(error.message || '加载锻造数据失败。')
      }
    }, { immediate: true })

    // 选择当前图纸。
    const selectBlueprint = (bp) => {
      if (isForging.value) return
      selectedBlueprint.value = bp
    }

    // 读取指定材料当前持有数量。
    const getMaterialCount = (itemId) => {
      const mat = myMaterials.value.find((item) => item.itemId === itemId)
      return mat ? mat.count : 0
    }

    // 当前材料是否足够。
    const hasEnoughMaterial = (mat) => {
      return getMaterialCount(mat.itemId) >= mat.count
    }

    const canForge = computed(() => {
      if (!selectedBlueprint.value) return false
      if (forgeInfo.isForging) return false
      return Boolean(selectedBlueprint.value.canForge) &&
        playerGold.value >= (selectedBlueprint.value.costGold || 0) &&
        selectedBlueprint.value.materials.every((mat) => hasEnoughMaterial(mat))
    })

    // 开始锻造。
    const startForge = async () => {
      if (!canForge.value || isForging.value || !selectedBlueprint.value?.recipeId) return

      isForging.value = true

      try {
        const result = await gameStore.forgeEquipment(selectedBlueprint.value.recipeId)
        const requiresCollection = Boolean(result?.requiresCollection ?? result?.RequiresCollection)
        if (requiresCollection) {
          await loadForgeOverview()
          return
        }

        const equipment = result?.equipment || result?.Equipment || null

        forgeResult.success = Boolean(result?.success ?? result?.Success ?? true)
        forgeResult.item = equipment ? buildEquipmentCard(equipment) : null
        forgeResult.message = result?.message || result?.Message || (forgeResult.success ? '锻造成功。' : '锻造失败。')
        forgeResult.show = true

        await loadForgeOverview()
      } catch (error) {
        toast.error(error.message || '锻造失败')
      } finally {
        isForging.value = false
      }
    }

    // 领取锻造结果。
    const collectForge = async () => {
      if (isForging.value || !forgeInfo.canCollect) return

      isForging.value = true

      try {
        const result = await gameStore.collectForgeResult()
        const equipment = result?.equipment || result?.Equipment || null
        forgeResult.success = Boolean(result?.success ?? result?.Success ?? true)
        forgeResult.item = equipment ? buildEquipmentCard(equipment) : null
        forgeResult.message = result?.message || result?.Message || (forgeResult.success ? '锻造完成。' : '锻造失败。')
        forgeResult.show = true
        await loadForgeOverview()
      } catch (error) {
        toast.error(error.message || '领取锻造结果失败')
      } finally {
        isForging.value = false
      }
    }

    function syncCountdownTimer() {
      if (countdownTimer) {
        clearInterval(countdownTimer)
        countdownTimer = null
      }

      if (!forgeInfo.isForging) {
        return
      }

      countdownTimer = window.setInterval(() => {
        nowTs.value = Date.now()
      }, 1000)
    }

    const closeResult = () => {
      forgeResult.show = false
    }

    watch(() => [forgeInfo.isForging, forgeInfo.activeForgeCompleteAt], () => {
      syncCountdownTimer()
    })

    onUnmounted(() => {
      if (countdownTimer) {
        clearInterval(countdownTimer)
      }
    })

    return {
      ICON,
      blueprints,
      selectedBlueprint,
      myMaterials,
      isForging,
      forgeResult,
      forgeInfo,
      forgeProgressPercent,
      forgeRemainingText,
      playerGold,
      canForge,
      selectBlueprint,
      getMaterialCount,
      hasEnoughMaterial,
      startForge,
      collectForge,
      closeResult
    }
  }
}
</script>

<style scoped>
.forge-modal {
  display: flex;
  gap: var(--spacing-lg);
  min-height: 560px;
}

.forge-sidebar {
  width: 280px;
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
  min-height: 0;
}

.forge-main {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
  min-width: 0;
}

.forge-card {
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
  white-space: nowrap;
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
  background: rgba(175, 131, 49, 0.35);
  border: 1px solid rgba(255, 196, 94, 0.5);
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
  background: linear-gradient(90deg, #be8f3d, #f4c464);
}

.progress-value {
  color: var(--text-secondary);
  font-size: var(--font-size-xs);
  font-family: var(--font-mono);
}

.summary-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: var(--spacing-sm);
}

.summary-item {
  display: flex;
  flex-direction: column;
  gap: 4px;
  padding: var(--spacing-sm);
  background: var(--xiuxian-bg-primary);
  border-radius: var(--radius-sm);
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
  overflow: hidden;
}

.blueprint-list {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
  overflow-y: auto;
  min-height: 0;
  max-height: 360px;
  padding-right: 4px;
}

.blueprint-item {
  width: 100%;
  display: flex;
  gap: var(--spacing-sm);
  align-items: center;
  padding: var(--spacing-sm);
  background: var(--xiuxian-bg-primary);
  border: 1px solid transparent;
  border-radius: var(--radius-sm);
  cursor: pointer;
  text-align: left;
  transition: all 0.25s ease;
}

.blueprint-item:hover {
  border-color: var(--border-color);
}

.blueprint-item.selected {
  border-color: var(--highlight-text);
  background: rgba(212, 168, 83, 0.10);
}

.blueprint-icon {
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

.blueprint-copy {
  flex: 1;
  min-width: 0;
}

.blueprint-header {
  display: flex;
  justify-content: space-between;
  gap: var(--spacing-xs);
  align-items: flex-start;
  margin-bottom: 4px;
}

.blueprint-name {
  font-size: 15px;
  font-weight: 600;
}

.blueprint-name.uncommon { color: var(--quality-uncommon); }
.blueprint-name.rare { color: var(--quality-rare); }
.blueprint-name.epic { color: var(--quality-epic); }
.blueprint-name.legendary { color: var(--quality-legendary); }

.blueprint-state {
  padding: 2px 8px;
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.06);
  color: var(--text-muted);
  font-size: 10px;
  flex-shrink: 0;
}

.blueprint-desc {
  color: var(--text-secondary);
  font-size: var(--font-size-xs);
  line-height: 1.5;
  margin-bottom: 4px;
}

.blueprint-meta {
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

.hero-title.uncommon { color: var(--quality-uncommon); }
.hero-title.rare { color: var(--quality-rare); }
.hero-title.epic { color: var(--quality-epic); }
.hero-title.legendary { color: var(--quality-legendary); }

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

.forge-btn {
  background: var(--button-primary-start);
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

.forge-grid {
  display: grid;
  grid-template-columns: 280px minmax(0, 1fr) 260px;
  gap: var(--spacing-md);
  align-items: start;
}

.platform-card {
  grid-column: 1;
}

.materials-card {
  grid-column: 2;
}

.inventory-card {
  grid-column: 3;
}

.forge-stage {
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

.forge-stage.forging {
  box-shadow: 0 0 24px rgba(216, 165, 73, 0.15);
}

.forge-shell {
  font-size: 82px;
  opacity: 0.92;
}

.forge-preview {
  position: absolute;
  top: 24px;
  font-size: 34px;
  animation: float 2.2s ease-in-out infinite;
}

.forge-effects {
  position: absolute;
  inset: 0;
}

.spark {
  position: absolute;
  font-size: 20px;
  animation: sparkle 0.5s ease-in-out infinite;
}

.spark:nth-child(1) { top: 20%; left: 20%; }
.spark:nth-child(2) { top: 30%; right: 20%; animation-delay: 0.2s; }
.spark:nth-child(3) { bottom: 30%; left: 30%; animation-delay: 0.4s; }

.hammer {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  font-size: 40px;
  animation: hammer 0.5s ease-in-out infinite;
}

.success-panel {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.success-fill {
  background: linear-gradient(90deg, #9c7224, #e2b25b, #5ebf67);
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
  min-height: 0;
  max-height: 320px;
  overflow-y: auto;
  padding-right: 4px;
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
  border: 2px solid var(--highlight-text);
  border-radius: var(--radius-lg);
  padding: var(--spacing-xl);
  text-align: center;
  box-shadow: var(--shadow-sm);
}

.result-title {
  font-size: 22px;
  font-weight: 700;
  margin-bottom: var(--spacing-lg);
  color: var(--highlight-text);
}

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

.result-name.uncommon { color: var(--quality-uncommon); }
.result-name.rare { color: var(--quality-rare); }
.result-name.epic { color: var(--quality-epic); }
.result-name.legendary { color: var(--quality-legendary); }

.result-message,
.result-stats {
  color: var(--text-secondary);
  line-height: 1.6;
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

@keyframes sparkle {
  0%, 100% { opacity: 0; transform: scale(0); }
  50% { opacity: 1; transform: scale(1); }
}

@keyframes hammer {
  0%, 100% { transform: translate(-50%, -50%) rotate(-30deg); }
  50% { transform: translate(-50%, -50%) rotate(0deg); }
}

@media (max-width: 1080px) {
  .forge-modal {
    flex-direction: column;
  }

  .forge-sidebar {
    width: 100%;
  }

  .forge-grid {
    grid-template-columns: 1fr;
  }

  .platform-card,
  .inventory-card,
  .materials-card {
    grid-column: auto;
  }

  .hero-card {
    flex-direction: column;
    align-items: flex-start;
  }

  .hero-actions {
    width: 100%;
  }
}
</style>
