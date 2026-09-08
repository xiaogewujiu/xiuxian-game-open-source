<template>
  <aside class="left-sidebar">
    <div class="sidebar-logo">
      <AssetIcon :source="ICON.misc_yinyang" size="30" class="logo-icon" />
    </div>

    <nav class="sidebar-nav">
      <button
        v-for="cat in categories"
        :key="cat.key"
        class="sidebar-btn"
        :class="{ active: modelValue === cat.key }"
        @click="selectCategory(cat.key)"
        :title="cat.label"
      >
        <span class="btn-icon"><AssetIcon :source="cat.icon" size="24" /></span>
        <span class="btn-label">{{ cat.label }}</span>
      </button>
    </nav>
  </aside>
</template>

<script>
import { ICON } from '../../icons'
import AssetIcon from '../common/AssetIcon.vue'

/**
 * LeftSidebar - 左侧快捷类别栏
 */
export default {
  name: 'LeftSidebar',

  components: {
    AssetIcon
  },

  props: {
    modelValue: {
      type: String,
      required: true
    }
  },

  emits: ['update:modelValue'],

  setup(props, { emit }) {
    const categories = [
      { key: 'cultivation', label: '修仙', icon: ICON.misc_yinyang },
      { key: 'adventure', label: '历练', icon: ICON.misc_swords },
      { key: 'market', label: '集市', icon: ICON.misc_merchant },
      { key: 'welfare', label: '福利', icon: ICON.misc_gift },
      { key: 'system', label: '系统', icon: ICON.ui_gear }
    ]

    const selectCategory = (key) => {
      emit('update:modelValue', key)
    }

    return {
      ICON,
      categories,
      selectCategory
    }
  }
}
</script>

<style scoped>
.left-sidebar {
  width: 68px;
  background: var(--xiuxian-bg-secondary);
  border-right: 1px solid var(--border-color);
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: var(--spacing-md) 0;
  gap: var(--spacing-lg);
  flex-shrink: 0;
  height: 100%;
}

.sidebar-logo {
  width: 44px;
  height: 44px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--bg-overlay-light);
  border-radius: var(--radius-md);
  margin-bottom: var(--spacing-sm);
}

.logo-icon {
  animation: logo-spin 15s linear infinite;
}

@keyframes logo-spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

.sidebar-nav {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
  width: 100%;
  align-items: center;
}

.sidebar-btn {
  width: 52px;
  height: 52px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 3px;
  background: transparent;
  border: 1px solid transparent;
  border-radius: var(--radius-md);
  color: var(--text-secondary);
  cursor: pointer;
  transition: all 0.15s ease;
}

.sidebar-btn:hover {
  background: var(--control-bg-hover);
  color: var(--text-primary);
}

.sidebar-btn.active {
  background: var(--accent-soft-bg);
  border-color: var(--border-color);
  color: var(--primary-color);
}

.btn-icon {
  display: flex;
  align-items: center;
  justify-content: center;
}

.btn-label {
  font-size: 10px;
  font-weight: 600;
}
</style>
