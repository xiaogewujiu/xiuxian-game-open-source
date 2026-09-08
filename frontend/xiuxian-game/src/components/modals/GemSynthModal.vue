<template>
  <XiuXianModal :model-value="modelValue" title="宝石合成" :width="420" @update:model-value="$emit('update:modelValue', $event)">
    <div class="gem-synth-modal">
      <div v-if="!recipe" class="empty-state">暂无可合成的宝石</div>
      <template v-else>
        <!-- 合成配方 -->
        <div class="synth-recipe">
          <div class="recipe-source">
            <div class="recipe-gem" :class="recipe.sourceQuality">
              <AssetIcon :source="ICON.misc_diamond" size="28" />
              <span class="recipe-gem-name">{{ recipe.sourceName }}</span>
              <span class="recipe-gem-level">Lv.{{ recipe.sourceLevel }}</span>
            </div>
            <span class="recipe-qty">×{{ recipe.requiredCount }}</span>
          </div>

          <div class="recipe-arrow">→</div>

          <div class="recipe-target">
            <div class="recipe-gem" :class="recipe.targetQuality">
              <AssetIcon :source="ICON.misc_diamond" size="28" />
              <span class="recipe-gem-name">{{ recipe.targetName }}</span>
              <span class="recipe-gem-level">Lv.{{ recipe.targetLevel }}</span>
            </div>
          </div>
        </div>

        <!-- 属性预览 -->
        <div class="synth-bonus">
          <div class="bonus-row">
            <span class="bonus-label">源宝石属性:</span>
            <span class="bonus-value">{{ recipe.sourceAttr }} +{{ recipe.sourceBonusDisplay }}</span>
          </div>
          <div class="bonus-row target">
            <span class="bonus-label">合成后属性:</span>
            <span class="bonus-value">{{ recipe.targetAttr }} +{{ recipe.targetBonusDisplay }}</span>
          </div>
        </div>

        <!-- 拥有数量 -->
        <div class="synth-owned">
          <span>拥有:</span>
          <span :class="{ enough: recipe.ownedCount >= recipe.requiredCount, lacking: recipe.ownedCount < recipe.requiredCount }">
            {{ recipe.ownedCount }} / {{ recipe.requiredCount }}
          </span>
        </div>

        <!-- 成功率 -->
        <div class="synth-rate" :class="rateClass">
          <span class="rate-label">合成成功率:</span>
          <span class="rate-value">{{ recipe.successRate }}%</span>
          <span v-if="recipe.successRate < 100" class="rate-warning">失败将损失材料</span>
        </div>

        <!-- 操作按钮 -->
        <div class="synth-actions">
          <button
            class="synth-btn"
            :disabled="isLoading || recipe.ownedCount < recipe.requiredCount"
            @click="handleSynthesize"
          >
            {{ isLoading ? '合成中...' : '合成' }}
          </button>
        </div>
      </template>
    </div>
  </XiuXianModal>
</template>

<script>
import { ref, watch, computed } from 'vue'
import XiuXianModal from '../common/XiuXianModal.vue'
import AssetIcon from '../common/AssetIcon.vue'
import { ICON } from '../../icons'
import { apiClient } from '../../lib/apiClient.js'
import { useGameStore } from '../../state/gameStore'
import toast from '@/utils/toast'

export default {
  name: 'GemSynthModal',
  components: { XiuXianModal, AssetIcon },
  props: {
    modelValue: { type: Boolean, default: false },
    gemId: { type: String, default: '' }
  },
  emits: ['update:modelValue'],
  setup(props) {
    const gameStore = useGameStore()
    const isLoading = ref(false)
    const recipe = ref(null)

    const ATTR_MAP = {
      Type1: '生命', Type2: '法力', Type3: '物攻', Type4: '法攻',
      Type5: '物防', Type6: '法防', Type7: '速度', Type8: '命中',
      Type9: '闪避', Type10: '暴击', Type11: '暴伤', Type12: '连击',
      Type13: '反击', Type14: '破甲', Type15: '增伤'
    }

    const QUALITY_MAP = { 1: 'common', 2: 'uncommon', 3: 'rare', 4: 'epic', 5: 'legendary' }

    const rateClass = computed(() => {
      const rate = recipe.value?.successRate ?? 100
      if (rate >= 80) return 'rate-high'
      if (rate >= 50) return 'rate-medium'
      return 'rate-low'
    })

    function formatBonus(value, mode) {
      return mode === 'Percent' ? value + '%' : String(value)
    }

    async function loadData() {
      if (!props.gemId) return
      isLoading.value = true
      try {
        const [templates, inventory] = await Promise.all([
          apiClient.getGemTemplates(),
          apiClient.getGemInventory()
        ])

        // 找到点击的宝石对应的模板（兼容大小写字段名）
        const clickedGem = templates.find(t => (t.gemId || t.GemId) === props.gemId)
        if (!clickedGem) { recipe.value = null; return }

        const clickedGemId = clickedGem.gemId || clickedGem.GemId

        // 找到该宝石可以合成的目标（即以它为源的高一级宝石）
        const target = templates.find(t => (t.synthFromGemId || t.SynthFromGemId) === clickedGemId)
        if (!target) { recipe.value = null; return }

        // 统计源宝石的拥有数量
        const owned = inventory
          .filter(g => (g.gemId || g.GemId) === clickedGemId)
          .reduce((sum, g) => sum + (g.quantity ?? g.Quantity ?? 0), 0)

        recipe.value = {
          sourceName: clickedGem.name || clickedGem.Name,
          sourceLevel: clickedGem.level ?? clickedGem.Level,
          sourceQuality: QUALITY_MAP[clickedGem.quality ?? clickedGem.Quality] || 'common',
          sourceAttr: ATTR_MAP[clickedGem.attributeType || clickedGem.AttributeType] || (clickedGem.attributeType || clickedGem.AttributeType),
          sourceBonusDisplay: formatBonus(clickedGem.bonusValue ?? clickedGem.BonusValue, clickedGem.bonusMode || clickedGem.BonusMode),
          targetName: target.name || target.Name,
          targetLevel: target.level ?? target.Level,
          targetQuality: QUALITY_MAP[target.quality ?? target.Quality] || 'common',
          targetAttr: ATTR_MAP[target.attributeType || target.AttributeType] || (target.attributeType || target.AttributeType),
          targetBonusDisplay: formatBonus(target.bonusValue ?? target.BonusValue, target.bonusMode || target.BonusMode),
          requiredCount: target.synthCount ?? target.SynthCount,
          ownedCount: owned,
          sourceGemId: clickedGemId,
          targetGemId: target.gemId || target.GemId,
          successRate: target.synthSuccessRate ?? target.SynthSuccessRate ?? 100
        }
      } catch (e) {
        toast.error(e.message || '加载宝石数据失败')
      } finally {
        isLoading.value = false
      }
    }

    async function handleSynthesize() {
      if (!recipe.value || recipe.value.ownedCount < recipe.value.requiredCount) return

      isLoading.value = true
      try {
        // 获取源宝石，只需传一个物品ID，后端自动按数量扣减
        const inventory = await apiClient.getGemInventory()
        const sourceItem = inventory
          .find(g => (g.gemId || g.GemId) === recipe.value.sourceGemId)

        if (!sourceItem) {
          toast.error('宝石数量不足')
          return
        }

        const itemId = sourceItem.inventoryItemId ?? sourceItem.InventoryItemId
        await apiClient.synthesizeGem(recipe.value.targetGemId, itemId)
        toast.success('合成成功')
        await Promise.all([loadData(), gameStore.loadInventory(true)])
      } catch (e) {
        toast.error(e.message || '合成失败')
      } finally {
        isLoading.value = false
      }
    }

    watch(() => props.modelValue, (val) => {
      if (val) loadData()
    })

    return { ICON, isLoading, recipe, handleSynthesize, rateClass }
  }
}
</script>

<style scoped>
.gem-synth-modal {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
  padding: var(--spacing-sm);
}

.empty-state {
  text-align: center;
  padding: var(--spacing-lg);
  color: var(--text-muted);
  font-size: var(--font-size-base);
}

.synth-recipe {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--spacing-md);
  padding: var(--spacing-md);
  background: var(--xiuxian-bg-secondary);
  border-radius: var(--radius-md);
}

.recipe-source,
.recipe-target {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
}

.recipe-gem {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
  padding: var(--spacing-md);
  border-radius: var(--radius-md);
  border: 2px solid var(--border-color);
  background: var(--xiuxian-bg-primary);
  min-width: 80px;
}

.recipe-gem.common { border-color: var(--quality-common); }
.recipe-gem.uncommon { border-color: var(--quality-uncommon); }
.recipe-gem.rare { border-color: var(--quality-rare); }
.recipe-gem.epic { border-color: var(--quality-epic); }
.recipe-gem.legendary { border-color: var(--quality-legendary); }

.recipe-gem-name {
  font-size: var(--font-size-sm);
  font-weight: 600;
  color: var(--text-primary);
}

.recipe-gem-level {
  font-size: var(--font-size-xs);
  color: var(--text-muted);
  font-family: var(--font-mono);
}

.recipe-qty {
  font-size: var(--font-size-lg);
  font-weight: 700;
  color: var(--highlight-text);
}

.recipe-arrow {
  font-size: 24px;
  font-weight: 700;
  color: var(--accent-text);
}

.synth-bonus {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-xs);
  padding: var(--spacing-sm);
  background: var(--xiuxian-bg-secondary);
  border-radius: var(--radius-md);
}

.bonus-row {
  display: flex;
  justify-content: space-between;
  font-size: var(--font-size-sm);
}

.bonus-label {
  color: var(--text-secondary);
}

.bonus-value {
  color: var(--highlight-text);
  font-weight: 600;
}

.bonus-row.target .bonus-value {
  color: var(--quality-uncommon);
}

.synth-owned {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--spacing-sm);
  font-size: var(--font-size-base);
  color: var(--text-secondary);
}

.synth-owned .enough {
  color: var(--quality-uncommon);
  font-weight: 700;
}

.synth-owned .lacking {
  color: var(--status-danger-text);
  font-weight: 700;
}

.synth-rate {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--spacing-sm);
  padding: var(--spacing-sm);
  border-radius: var(--radius-md);
  font-size: var(--font-size-sm);
}

.synth-rate.rate-high {
  background: rgba(34, 197, 94, 0.1);
  border: 1px solid rgba(34, 197, 94, 0.3);
}

.synth-rate.rate-medium {
  background: rgba(234, 179, 8, 0.1);
  border: 1px solid rgba(234, 179, 8, 0.3);
}

.synth-rate.rate-low {
  background: rgba(239, 68, 68, 0.1);
  border: 1px solid rgba(239, 68, 68, 0.3);
}

.rate-label {
  color: var(--text-secondary);
}

.rate-value {
  font-weight: 700;
  font-family: var(--font-mono);
}

.rate-high .rate-value { color: #22c55e; }
.rate-medium .rate-value { color: #eab308; }
.rate-low .rate-value { color: #ef4444; }

.rate-warning {
  font-size: var(--font-size-xs);
  color: #ef4444;
  font-weight: 500;
}

.synth-actions {
  display: flex;
  justify-content: center;
}

.synth-btn {
  padding: var(--spacing-sm) var(--spacing-xl);
  background: linear-gradient(135deg, var(--button-primary-start), var(--button-primary-end));
  border: none;
  border-radius: var(--radius-md);
  color: var(--button-primary-text);
  font-size: var(--font-size-base);
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
}

.synth-btn:hover:not(:disabled) {
  opacity: 0.9;
  box-shadow: var(--shadow-sm);
}

.synth-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
</style>
