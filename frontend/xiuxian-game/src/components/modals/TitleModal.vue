<template>
  <XiuXianModal :model-value="modelValue" title="称号" :width="480" @update:model-value="$emit('update:modelValue', $event)">
    <div class="title-modal">
      <!-- 当前佩戴 -->
      <div class="current-title" v-if="overview.currentTitle">
        <div class="current-label">当前佩戴</div>
        <div class="title-card equipped">
          <div class="title-icon">
            <AssetIcon v-if="overview.currentTitle.iconPath" :source="overview.currentTitle.iconPath" size="40" />
            <div v-else class="title-icon-placeholder" :class="'rarity-' + overview.currentTitle.rarity.toLowerCase()">{{ overview.currentTitle.name[0] }}</div>
          </div>
          <div class="title-info">
            <span class="title-name" :class="'rarity-' + overview.currentTitle.rarity.toLowerCase()">{{ overview.currentTitle.name }}</span>
            <span class="title-rarity">{{ getRarityLabel(overview.currentTitle.rarity) }}</span>
          </div>
          <button class="action-btn unequip-btn" :disabled="isLoading" @click="handleUnequip">取下</button>
        </div>
      </div>
      <div class="current-title" v-else>
        <div class="current-label">当前佩戴</div>
        <div class="empty-state">未佩戴任何称号</div>
      </div>

      <!-- 称号列表 -->
      <div class="title-list-section">
        <div class="list-label">已拥有 ({{ overview.titles.length }})</div>
        <div v-if="overview.titles.length === 0" class="empty-state">暂无称号</div>
        <div
          v-for="title in overview.titles"
          :key="title.titleId"
          class="title-card"
          :class="{ equipped: title.isEquipped }"
        >
          <div class="title-icon">
            <AssetIcon v-if="title.iconPath" :source="title.iconPath" size="40" />
            <div v-else class="title-icon-placeholder" :class="'rarity-' + title.rarity.toLowerCase()">{{ title.name[0] }}</div>
          </div>
          <div class="title-info">
            <span class="title-name" :class="'rarity-' + title.rarity.toLowerCase()">{{ title.name }}</span>
            <span class="title-rarity">{{ getRarityLabel(title.rarity) }}</span>
            <span v-if="title.description" class="title-desc">{{ title.description }}</span>
          </div>
          <button
            v-if="!title.isEquipped"
            class="action-btn equip-btn"
            :disabled="isLoading"
            @click="handleEquip(title.titleId)"
          >佩戴</button>
          <span v-else class="equipped-tag">佩戴中</span>
        </div>
      </div>
    </div>
  </XiuXianModal>
</template>

<script>
import { ref, reactive, watch } from 'vue'
import XiuXianModal from '../common/XiuXianModal.vue'
import AssetIcon from '../common/AssetIcon.vue'
import { apiClient } from '../../lib/apiClient.js'

export default {
  name: 'TitleModal',
  components: { XiuXianModal, AssetIcon },
  props: {
    modelValue: { type: Boolean, default: false }
  },
  emits: ['update:modelValue'],
  setup(props) {
    const isLoading = ref(false)
    const overview = reactive({
      currentTitle: null,
      titles: []
    })

    const RARITY_LABELS = {
      Common: '普通',
      Rare: '稀有',
      Epic: '史诗',
      Legendary: '传说'
    }

    function getRarityLabel(rarity) {
      return RARITY_LABELS[rarity] || rarity
    }

    async function loadOverview() {
      isLoading.value = true
      try {
        const data = await apiClient.getTitleOverview()
        overview.currentTitle = data.currentTitle || null
        overview.titles = data.titles || []
      } catch (e) {
        console.error('加载称号失败', e)
      } finally {
        isLoading.value = false
      }
    }

    async function handleEquip(titleId) {
      isLoading.value = true
      try {
        await apiClient.equipTitle(titleId)
        await loadOverview()
      } catch (e) {
        console.error('佩戴称号失败', e)
      } finally {
        isLoading.value = false
      }
    }

    async function handleUnequip() {
      isLoading.value = true
      try {
        await apiClient.unequipTitle()
        await loadOverview()
      } catch (e) {
        console.error('取消佩戴失败', e)
      } finally {
        isLoading.value = false
      }
    }

    watch(() => props.modelValue, (val) => {
      if (val) loadOverview()
    })

    return { isLoading, overview, handleEquip, handleUnequip, getRarityLabel }
  }
}
</script>

<style scoped>
.title-modal {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}

.current-title {
  padding-bottom: var(--spacing-md);
  border-bottom: 1px solid var(--border-color);
}

.current-label,
.list-label {
  font-size: var(--font-size-xs);
  color: var(--text-muted);
  margin-bottom: var(--spacing-sm);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.title-card {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  padding: var(--spacing-sm) var(--spacing-md);
  border-radius: var(--radius-md);
  background: var(--xiuxian-bg-secondary);
  margin-bottom: var(--spacing-xs);
  transition: all 0.2s;
}

.title-card:hover {
  background: var(--xiuxian-bg-hover, rgba(255, 255, 255, 0.05));
}

.title-card.equipped {
  background: rgba(167, 139, 250, 0.1);
  border: 1px solid rgba(167, 139, 250, 0.3);
}

.title-icon {
  flex-shrink: 0;
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.title-icon-placeholder {
  width: 40px;
  height: 40px;
  border-radius: var(--radius-md);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 18px;
  font-weight: 700;
  background: var(--xiuxian-bg-primary);
  border: 2px solid var(--border-color);
}

.title-icon-placeholder.rarity-common { border-color: #64748b; color: #64748b; }
.title-icon-placeholder.rarity-rare { border-color: #3b82f6; color: #3b82f6; }
.title-icon-placeholder.rarity-epic { border-color: #a855f7; color: #a855f7; }
.title-icon-placeholder.rarity-legendary { border-color: #f59e0b; color: #f59e0b; }

.title-info {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 2px;
  min-width: 0;
}

.title-name {
  font-weight: 600;
  font-size: var(--font-size-base);
}

.title-rarity {
  font-size: var(--font-size-xs);
  color: var(--text-muted);
}

.title-desc {
  font-size: var(--font-size-xs);
  color: var(--text-secondary);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.rarity-common { color: #94a3b8; }
.rarity-rare { color: #3b82f6; }
.rarity-epic { color: #a855f7; }
.rarity-legendary { color: #f59e0b; }

.equipped-tag {
  font-size: var(--font-size-xs);
  color: #a78bfa;
  font-weight: 600;
  padding: 2px 8px;
  background: rgba(167, 139, 250, 0.1);
  border-radius: var(--radius-sm);
}

.action-btn {
  padding: var(--spacing-xs) var(--spacing-md);
  border-radius: var(--radius-sm);
  border: 1px solid var(--border-color);
  background: transparent;
  cursor: pointer;
  font-size: var(--font-size-sm);
  transition: all 0.2s;
  flex-shrink: 0;
}

.action-btn:hover:not(:disabled) {
  background: var(--xiuxian-bg-hover, rgba(255, 255, 255, 0.05));
}

.action-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.equip-btn {
  border-color: #a78bfa;
  color: #a78bfa;
}

.equip-btn:hover:not(:disabled) {
  background: rgba(167, 139, 250, 0.1);
}

.unequip-btn {
  border-color: #ef4444;
  color: #ef4444;
}

.unequip-btn:hover:not(:disabled) {
  background: rgba(239, 68, 68, 0.1);
}

.empty-state {
  text-align: center;
  padding: var(--spacing-lg);
  color: var(--text-muted);
  font-size: var(--font-size-sm);
}
</style>
