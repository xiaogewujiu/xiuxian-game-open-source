<template>
  <XiuXianModal :model-value="modelValue" title="宠物系统" :width="700" @update:model-value="$emit('update:modelValue', $event)">
    <div class="pet-modal">
      <div class="pet-list">
        <div
          v-for="pet in pets"
          :key="pet.instanceId"
          class="pet-card"
          :class="{ selected: selectedPet?.instanceId === pet.instanceId, active: pet.isActive }"
          @click="selectPet(pet.instanceId)"
        >
          <div class="pet-avatar">{{ pet.avatar }}</div>
          <div class="pet-name">{{ pet.name }}</div>
          <div class="pet-level">Lv.{{ pet.level }}</div>
          <div v-if="pet.isActive" class="active-badge">出战</div>
        </div>
      </div>

      <div v-if="isLoading" class="status-panel">
        <div class="hint-icon">⏳</div>
        <p>宠物数据加载中...</p>
      </div>

      <div v-else-if="selectedPet" class="pet-details">
        <div class="details-header">
          <div class="details-avatar">{{ selectedPet.avatar }}</div>
          <div class="details-info">
            <div class="details-name">
              {{ selectedPet.name }}
              <span class="quality-badge" :class="qualityClass(selectedPet.quality)">{{ selectedPet.qualityText }}</span>
            </div>
            <div class="details-type">{{ selectedPet.typeText }} | {{ selectedPet.elementText || '无属性' }} | {{ selectedPet.growthText }}成长</div>
            <div class="details-desc">{{ selectedPet.description }}</div>
          </div>
        </div>

        <div class="pet-level-info">
          <div class="level-row">
            <span>等级: Lv.{{ selectedPet.level }}</span>
            <span>经验: {{ selectedPet.exp }}/{{ selectedPet.xExp }}</span>
          </div>
          <div class="exp-bar">
            <div class="exp-fill" :style="{ width: `${expPercent}%` }"></div>
          </div>
        </div>

        <div class="resource-card">
          <div class="resource-row">
            <span>灵兽口粮</span>
            <span :class="{ insufficient: petFoodCount <= 0 }">{{ petFoodCount }}</span>
          </div>
          <div class="resource-row">
            <span>进化石</span>
            <span :class="{ insufficient: evolutionStoneCount < evolveCost }">{{ evolutionStoneCount }}/{{ evolveCost }}</span>
          </div>
          <div class="resource-row">
            <span>忠诚度</span>
            <span>{{ selectedPet.loyalty }}/100</span>
          </div>
        </div>

        <div class="pet-stats">
          <div class="stats-title">宠物属性</div>
          <div class="stats-grid">
            <div class="stat-item">
              <span class="stat-name">攻击</span>
              <span class="stat-value">{{ selectedPet.attack }}</span>
            </div>
            <div class="stat-item">
              <span class="stat-name">防御</span>
              <span class="stat-value">{{ selectedPet.defense }}</span>
            </div>
            <div class="stat-item">
              <span class="stat-name">生命</span>
              <span class="stat-value">{{ selectedPet.hp }}</span>
            </div>
            <div class="stat-item">
              <span class="stat-name">法力</span>
              <span class="stat-value">{{ selectedPet.mp }}</span>
            </div>
            <div class="stat-item">
              <span class="stat-name">速度</span>
              <span class="stat-value">{{ selectedPet.speed }}</span>
            </div>
            <div class="stat-item">
              <span class="stat-name">品质</span>
              <span class="stat-value">{{ selectedPet.qualityText }} / {{ selectedPet.maxQuality }}</span>
            </div>
            <div class="stat-item">
              <span class="stat-name">命中</span>
              <span class="stat-value">{{ formatPercent(selectedPet.hitRate) }}</span>
            </div>
            <div class="stat-item">
              <span class="stat-name">闪避</span>
              <span class="stat-value">{{ formatPercent(selectedPet.dodgeRate) }}</span>
            </div>
            <div class="stat-item">
              <span class="stat-name">暴击</span>
              <span class="stat-value">{{ formatPercent(selectedPet.critRate) }}</span>
            </div>
            <div class="stat-item">
              <span class="stat-name">暴伤</span>
              <span class="stat-value">{{ formatPercent(selectedPet.critDamage, true) }}</span>
            </div>
            <div class="stat-item">
              <span class="stat-name">连击</span>
              <span class="stat-value">{{ formatPercent(selectedPet.comboRate) }}</span>
            </div>
            <div class="stat-item">
              <span class="stat-name">反击</span>
              <span class="stat-value">{{ formatPercent(selectedPet.counterRate) }}</span>
            </div>
            <div class="stat-item">
              <span class="stat-name">破甲</span>
              <span class="stat-value">{{ formatPercent(selectedPet.armorBreak) }}</span>
            </div>
            <div class="stat-item">
              <span class="stat-name">附伤</span>
              <span class="stat-value">{{ formatPercent(selectedPet.bonusDamage) }}</span>
            </div>
          </div>
        </div>

        <div class="pet-skills">
          <div class="skills-title">宠物技能</div>
          <div class="skills-list">
            <div v-for="skill in selectedPet.skills" :key="skill.skillId" class="skill-item">
              <span class="skill-icon">{{ skill.icon }}</span>
              <div class="skill-info">
                <div class="skill-name">{{ skill.name }}</div>
                <div class="skill-desc">{{ skill.description }}</div>
              </div>
            </div>
            <div v-if="!selectedPet.skills.length" class="empty-hint">该灵宠当前没有技能配置。</div>
          </div>
        </div>

        <div class="pet-actions">
          <button class="action-btn" :class="{ active: selectedPet.isActive }" :disabled="isSubmitting" @click="toggleActive">
            {{ isSubmitting ? '处理中...' : (selectedPet.isActive ? '召回' : '出战') }}
          </button>
          <button class="action-btn upgrade" :disabled="isSubmitting || evolutionStoneCount < evolveCost || selectedPet.quality >= selectedPet.maxQuality" @click="evolvePet">
            {{ selectedPet.quality >= selectedPet.maxQuality ? '已满阶' : '进化' }}
          </button>
          <button class="action-btn feed" :disabled="isSubmitting || petFoodCount <= 0 || selectedPet.loyalty >= 100" @click="feedPet">
            喂养
          </button>
          <button class="action-btn release" :disabled="isSubmitting || selectedPet.isActive" @click="releasePet">
            放生
          </button>
        </div>
      </div>

      <div v-else class="status-panel">
        <div class="hint-icon"><AssetIcon :source="ICON.misc_dragon" size="25" /></div>
        <p>{{ statusMessage || '当前还没有灵宠，请先在背包中使用宠物蛋。' }}</p>
      </div>
    </div>

    <div v-if="statusMessage && selectedPet" class="status-message">{{ statusMessage }}</div>
  </XiuXianModal>
</template>

<script>
import { computed, ref, watch } from 'vue'
import XiuXianModal from '../common/XiuXianModal.vue'
import { apiClient } from '../../lib/apiClient'
import { confirm as showConfirm } from '@/utils/confirm'
import { useGameStore } from '../../state/gameStore'
import { ICON } from '../../icons'
import AssetIcon from '../common/AssetIcon.vue'

/*
 * 中文注释：
 * 宠物弹窗现在只维护“我已经拥有的灵宠”这一份状态。
 * 新灵宠统一通过背包使用宠物蛋获得，这里只负责查看与培养。
 */
export default {
  name: 'PetModal',

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
    const pets = ref([])
    const selectedPetId = ref('')
    const isLoading = ref(false)
    const isSubmitting = ref(false)
    const statusMessage = ref('')

    const inventoryItems = computed(() => gameStore.state.inventory || [])
    const selectedPet = computed(() => pets.value.find((pet) => pet.instanceId === selectedPetId.value) || null)
    const petFoodCount = computed(() => getItemCount('pet_food_basic'))
    const evolutionStoneCount = computed(() => getItemCount('evolution_stone'))

    const evolveCost = computed(() => {
      if (!selectedPet.value) {
        return 0
      }

      return Math.max(1, selectedPet.value.quality * 5)
    })

    const expPercent = computed(() => {
      if (!selectedPet.value || !selectedPet.value.xExp) {
        return 0
      }

      return Math.min(100, Math.round((selectedPet.value.exp / selectedPet.value.xExp) * 100))
    })

    function getItemCount(itemId) {
      return inventoryItems.value
        .filter((item) => item.itemId === itemId)
        .reduce((sum, item) => sum + (item.quantity || 0), 0)
    }

    function qualityClass(quality) {
      if (quality >= 5) return 'legendary'
      if (quality === 4) return 'epic'
      if (quality === 3) return 'rare'
      if (quality === 2) return 'uncommon'
      return 'common'
    }

    function formatPercent(value, isMultiplier = false) {
      if (value == null || Number.isNaN(Number(value))) {
        return '-'
      }

      return `${(Number(value) * 100).toFixed(1)}%${isMultiplier ? ' 倍率' : ''}`
    }

    function selectPet(petId) {
      selectedPetId.value = petId
      statusMessage.value = ''
    }

    async function refreshPetData() {
      const [petList] = await Promise.all([
        apiClient.getPets(),
        gameStore.loadInventory(true)
      ])

      pets.value = Array.isArray(petList) ? petList : []
      if (!selectedPetId.value || !pets.value.find((pet) => pet.instanceId === selectedPetId.value)) {
        selectedPetId.value = pets.value[0]?.instanceId || ''
      }
    }

    async function loadPetModalData() {
      isLoading.value = true
      statusMessage.value = ''

      try {
        await refreshPetData()
      } catch (error) {
        pets.value = []
        selectedPetId.value = ''
        statusMessage.value = error.message || '加载宠物数据失败。'
      } finally {
        isLoading.value = false
      }
    }

    async function refreshAfterMutation(message) {
      await Promise.allSettled([
        refreshPetData(),
        gameStore.loadPlayer(true)
      ])

      if (message) {
        statusMessage.value = message
      }
    }

    async function toggleActive() {
      if (!selectedPet.value || isSubmitting.value) {
        return
      }

      isSubmitting.value = true

      try {
        if (selectedPet.value.isActive) {
          await apiClient.clearActivePet()
          await refreshAfterMutation('已召回当前出战灵宠。')
        } else {
          await apiClient.setActivePet(selectedPet.value.instanceId)
          await refreshAfterMutation(`已设置 ${selectedPet.value.name} 出战。`)
        }
      } catch (error) {
        statusMessage.value = error.message || '切换出战状态失败。'
      } finally {
        isSubmitting.value = false
      }
    }

    async function feedPet() {
      if (!selectedPet.value || petFoodCount.value <= 0 || isSubmitting.value) {
        return
      }

      isSubmitting.value = true

      try {
        await apiClient.feedPet(selectedPet.value.instanceId, 'pet_food_basic', 1)
        await refreshAfterMutation(`${selectedPet.value.name} 喂养成功，忠诚度已提升。`)
      } catch (error) {
        statusMessage.value = error.message || '喂养失败。'
      } finally {
        isSubmitting.value = false
      }
    }

    async function evolvePet() {
      if (!selectedPet.value || isSubmitting.value || evolutionStoneCount.value < evolveCost.value) {
        return
      }

      isSubmitting.value = true

      try {
        await apiClient.evolvePet(selectedPet.value.instanceId)
        await refreshAfterMutation(`${selectedPet.value.name} 进化成功。`)
      } catch (error) {
        statusMessage.value = error.message || '进化失败。'
      } finally {
        isSubmitting.value = false
      }
    }

    async function releasePet() {
      if (!selectedPet.value || selectedPet.value.isActive || isSubmitting.value) {
        return
      }

      const targetName = selectedPet.value.name
      if (!await showConfirm(`确定要放生 ${targetName} 吗？放生后无法恢复。`, { type: 'danger', icon: '🐾' })) {
        return
      }

      isSubmitting.value = true

      try {
        await apiClient.releasePet(selectedPet.value.instanceId)
        await refreshAfterMutation(`已放生灵宠：${targetName}。`)
      } catch (error) {
        statusMessage.value = error.message || '放生失败。'
      } finally {
        isSubmitting.value = false
      }
    }

    watch(() => props.modelValue, async (visible) => {
      if (!visible) {
        return
      }

      await loadPetModalData()
    }, { immediate: true })

    return {
      ICON,
      pets,
      selectedPet,
      petFoodCount,
      evolutionStoneCount,
      evolveCost,
      expPercent,
      isLoading,
      isSubmitting,
      statusMessage,
      qualityClass,
      formatPercent,
      selectPet,
      toggleActive,
      feedPet,
      evolvePet,
      releasePet
    }
  }
}
</script>

<style scoped>
.pet-modal {
  display: flex;
  gap: var(--spacing-lg);
  min-height: 430px;
}

.pet-list {
  width: 160px;
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
  overflow-y: auto;
  max-height: 430px;
  padding-right: var(--spacing-sm);
}

.pet-card {
  padding: var(--spacing-md);
  background: var(--xiuxian-bg-secondary);
  border-radius: var(--radius-md);
  text-align: center;
  cursor: pointer;
  transition: all 0.15s ease;
  border: 2px solid transparent;
  position: relative;
}

.pet-card:hover {
  border-color: var(--accent-text);
}

.pet-card.selected {
  border-color: var(--highlight-text);
  background: rgba(212, 168, 83, 0.12);
}

.pet-card.active {
  border-color: var(--quality-uncommon);
}

.pet-avatar {
  font-size: 40px;
  margin-bottom: var(--spacing-xs);
}

.pet-name {
  font-size: var(--font-size-base);
  font-weight: 500;
  color: var(--text-primary);
  margin-bottom: 2px;
}

.pet-level {
  font-size: var(--font-size-sm);
  color: var(--accent-text);
}

.active-badge {
  position: absolute;
  top: 4px;
  right: 4px;
  padding: 2px 6px;
  background: var(--quality-uncommon);
  border-radius: var(--radius-sm);
  font-size: var(--font-size-xs);
  color: var(--text-on-dark);
}

.pet-details,
.status-panel {
  flex: 1;
  background: var(--xiuxian-bg-secondary);
  border-radius: var(--radius-md);
  padding: var(--spacing-lg);
}

.status-panel {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: var(--text-muted);
}

.hint-icon {
  font-size: 64px;
  margin-bottom: var(--spacing-md);
  opacity: 0.6;
}

.details-header {
  display: flex;
  gap: var(--spacing-md);
  margin-bottom: var(--spacing-lg);
}

.details-avatar {
  font-size: 64px;
}

.details-info {
  flex: 1;
}

.details-name {
  font-size: 18px;
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: var(--spacing-xs);
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
}

.details-type {
  font-size: var(--font-size-base);
  color: var(--text-muted);
  margin-bottom: var(--spacing-xs);
}

.details-desc {
  font-size: var(--font-size-sm);
  color: var(--text-secondary);
  line-height: 1.6;
}

.quality-badge {
  font-size: var(--font-size-xs);
  padding: 2px 8px;
  border-radius: var(--radius-sm);
}

.quality-badge.common { background: rgba(176, 176, 176, 0.2); color: var(--quality-common); }
.quality-badge.uncommon { background: rgba(76, 175, 80, 0.2); color: var(--quality-uncommon); }
.quality-badge.rare { background: rgba(33, 150, 243, 0.2); color: var(--quality-rare); }
.quality-badge.epic { background: rgba(156, 39, 176, 0.2); color: var(--quality-epic); }
.quality-badge.legendary { background: rgba(255, 152, 0, 0.2); color: var(--quality-legendary); }

.pet-level-info,
.resource-card {
  background: rgba(124, 58, 237, 0.1);
  padding: var(--spacing-md);
  border-radius: var(--radius-md);
  margin-bottom: var(--spacing-lg);
}

.level-row,
.resource-row {
  display: flex;
  justify-content: space-between;
  font-size: var(--font-size-base);
  color: var(--text-secondary);
}

.resource-row + .resource-row {
  margin-top: var(--spacing-xs);
}

.insufficient {
  color: var(--hp-color);
}

.exp-bar {
  height: 8px;
  background: var(--xiuxian-bg-primary);
  border-radius: 4px;
  overflow: hidden;
  margin-top: var(--spacing-xs);
}

.exp-fill {
  height: 100%;
  background: var(--button-primary-start);
  border-radius: 4px;
  transition: width 0.3s ease;
}

.pet-stats,
.pet-skills {
  margin-bottom: var(--spacing-lg);
}

.stats-title,
.skills-title {
  font-size: var(--font-size-base);
  font-weight: 600;
  color: var(--accent-text);
  margin-bottom: var(--spacing-md);
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: var(--spacing-sm);
}

.stat-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: var(--spacing-sm);
  background: var(--xiuxian-bg-primary);
  border-radius: var(--radius-sm);
}

.stat-name {
  font-size: var(--font-size-sm);
  color: var(--text-muted);
  margin-bottom: 2px;
}

.stat-value {
  font-size: 14px;
  font-weight: 600;
  color: var(--text-primary);
  font-family: var(--font-mono);
}

.skills-list {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
}

.skill-item {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  padding: var(--spacing-sm);
  background: var(--xiuxian-bg-primary);
  border-radius: var(--radius-sm);
  border: 1px solid transparent;
}

.skill-icon {
  font-size: 24px;
}

.skill-info {
  flex: 1;
}

.skill-name {
  font-size: var(--font-size-base);
  font-weight: 500;
  color: var(--text-primary);
}

.skill-desc,
.empty-hint {
  font-size: var(--font-size-sm);
  color: var(--text-muted);
  line-height: 1.5;
}

.pet-actions {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: var(--spacing-sm);
}

.action-btn {
  padding: var(--spacing-md);
  background: var(--xiuxian-bg-primary);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-sm);
  color: var(--text-primary);
  font-size: var(--font-size-base);
  cursor: pointer;
  transition: all 0.15s ease;
}

.action-btn:hover:not(:disabled) {
  border-color: var(--accent-text);
}

.action-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.action-btn.active {
  background: var(--status-danger-bg);
  border-color: var(--hp-color);
  color: var(--hp-color);
}

.action-btn.upgrade {
  background: var(--status-warning-bg);
  border-color: var(--status-warning-text);
  color: var(--status-warning-text);
}

.action-btn.feed {
  background: var(--status-success-bg);
  border-color: var(--quality-uncommon);
  color: var(--quality-uncommon);
}

.action-btn.release {
  background: var(--button-danger-start);
  border-color: var(--status-danger-text);
  color: #fff;
  font-weight: 600;
  text-shadow: 0 1px 2px rgba(0, 0, 0, 0.45);
}

.action-btn.release:hover:not(:disabled) {
  background: var(--button-danger-hover-start, #b91c1c);
  border-color: var(--status-danger-text);
  color: #fff;
}

.action-btn.release:disabled {
  color: rgba(255, 255, 255, 0.72);
}

.status-message {
  margin-top: var(--spacing-md);
  padding: var(--spacing-sm) var(--spacing-md);
  background: var(--accent-soft-bg);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-sm);
  color: var(--text-secondary);
  line-height: 1.6;
}

@media (max-width: 900px) {
  .pet-modal {
    flex-direction: column;
  }

  .pet-list {
    width: 100%;
    max-height: none;
    flex-direction: row;
    overflow-x: auto;
    overflow-y: hidden;
  }

  .pet-card {
    min-width: 120px;
  }

  .stats-grid,
  .pet-actions {
    grid-template-columns: repeat(2, 1fr);
  }
}
</style>
