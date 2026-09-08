<template>
  <!-- 修仙风格 Tooltip 悬停提示组件 -->
  <div class="tooltip-wrapper"
       ref="wrapperEl"
       @mouseenter="handleMouseEnter"
       @mouseleave="handleMouseLeave"
       @mousemove="updatePosition"
       @click="handleClick">
    <!-- 触发元素插槽 -->
    <slot></slot>

    <!-- Tooltip 内容 - 使用 Teleport 挂载到 body -->
    <Teleport to="body">
      <Transition name="tooltip">
        <div v-show="visible"
             ref="tooltipEl"
             class="tooltip-content"
             :class="position"
             :style="tooltipStyle">
          <!-- 标题 -->
          <div v-if="title" class="tooltip-title" :class="titleClass">
            {{ title }}
          </div>

          <!-- 自定义内容 -->
          <div v-if="$slots.content" class="tooltip-body">
            <slot name="content"></slot>
          </div>

          <!-- 纯文本内容 -->
          <div v-else-if="content" class="tooltip-body">
            {{ content }}
          </div>

          <!-- 属性列表 -->
          <div v-if="stats && stats.length" class="tooltip-stats">
            <div v-for="(stat, index) in stats" :key="index" class="stat-line">
              <span class="stat-name">{{ stat.name }}:</span>
              <span class="stat-value" :class="stat.type || ''">{{ stat.value }}</span>
            </div>
          </div>

          <!-- 描述文字 -->
          <div v-if="description" class="tooltip-description">
            {{ description }}
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<script>
import { ref, computed, onMounted, onBeforeUnmount } from 'vue'

/**
 * XiuXianTooltip - 修仙风格 Tooltip 组件
 *
 * 功能：
 * - 鼠标悬停显示详细信息
 * - 支持自定义位置
 * - 支持属性列表展示
 * - 支持品质颜色
 *
 * Props:
 * - title: 标题
 * - titleClass: 标题样式类（用于品质颜色）
 * - content: 纯文本内容
 * - stats: 属性列表 [{ name, value, type }]
 * - description: 描述文字
 * - position: 位置 ('top' | 'bottom' | 'left' | 'right')
 * - offset: 偏移距离
 */
export default {
  name: 'XiuXianTooltip',

  props: {
    title: {
      type: String,
      default: ''
    },
    titleClass: {
      type: String,
      default: ''
    },
    content: {
      type: String,
      default: ''
    },
    stats: {
      type: Array,
      default: () => []
    },
    description: {
      type: String,
      default: ''
    },
    position: {
      type: String,
      default: 'top',
      validator: (val) => ['top', 'bottom', 'left', 'right'].includes(val)
    },
    offset: {
      type: Number,
      default: 10
    },
    maxWidth: {
      type: Number,
      default: 300
    },
    trigger: {
      type: String,
      default: 'hover',
      validator: (val) => ['hover', 'click'].includes(val)
    }
  },

  setup(props) {
    const visible = ref(false)
    const tooltipEl = ref(null)
    const wrapperEl = ref(null)
    const posX = ref(0)
    const posY = ref(0)

    const syncPositionFromWrapper = () => {
      if (!wrapperEl.value) {
        return
      }

      const rect = wrapperEl.value.getBoundingClientRect()
      posX.value = rect.left + rect.width / 2
      posY.value = rect.top + rect.height / 2
    }

    // Tooltip 样式 - 使用 fixed 定位
    const tooltipStyle = computed(() => {
      const style = {
        position: 'fixed',
        maxWidth: props.maxWidth + 'px',
        zIndex: 9999
      }

      // 根据位置设置定位
      switch (props.position) {
        case 'top':
          style.left = posX.value + 'px'
          style.top = (posY.value - props.offset) + 'px'
          style.transform = 'translate(-50%, -100%)'
          break
        case 'bottom':
          style.left = posX.value + 'px'
          style.top = (posY.value + props.offset) + 'px'
          style.transform = 'translateX(-50%)'
          break
        case 'left':
          style.left = (posX.value - props.offset) + 'px'
          style.top = posY.value + 'px'
          style.transform = 'translate(-100%, -50%)'
          break
        case 'right':
          style.left = (posX.value + props.offset) + 'px'
          style.top = posY.value + 'px'
          style.transform = 'translateY(-50%)'
          break
      }

      return style
    })

    // 显示 Tooltip
    const show = () => {
      syncPositionFromWrapper()
      visible.value = true
    }

    // 隐藏 Tooltip
    const hide = () => {
      visible.value = false
    }

    // 更新位置
    const updatePosition = (e) => {
      syncPositionFromWrapper()
    }

    const handleMouseEnter = () => {
      if (props.trigger === 'hover') {
        show()
      }
    }

    const handleMouseLeave = () => {
      if (props.trigger === 'hover') {
        hide()
      }
    }

    const handleClick = () => {
      if (props.trigger !== 'click') {
        return
      }

      if (visible.value) {
        hide()
      } else {
        show()
      }
    }

    const handleDocumentPointerDown = (event) => {
      if (props.trigger !== 'click' || !visible.value) {
        return
      }

      const target = event.target instanceof Element ? event.target : null
      if (!target || wrapperEl.value?.contains(target) || tooltipEl.value?.contains(target)) {
        return
      }

      hide()
    }

    const handleDocumentKeydown = (event) => {
      if (props.trigger === 'click' && event.key === 'Escape') {
        hide()
      }
    }

    onMounted(() => {
      document.addEventListener('pointerdown', handleDocumentPointerDown, true)
      document.addEventListener('keydown', handleDocumentKeydown)
    })

    onBeforeUnmount(() => {
      document.removeEventListener('pointerdown', handleDocumentPointerDown, true)
      document.removeEventListener('keydown', handleDocumentKeydown)
    })

    return {
      visible,
      tooltipEl,
      wrapperEl,
      tooltipStyle,
      show,
      hide,
      updatePosition,
      handleMouseEnter,
      handleMouseLeave,
      handleClick
    }
  }
}
</script>

<style scoped>
.tooltip-wrapper {
  position: relative;
  display: inline-block;
}

.tooltip-content {
  background: var(--xiuxian-bg-panel);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  padding: var(--spacing-md);
  box-shadow: var(--shadow-lg);
  pointer-events: none;
  min-width: 200px;
  backdrop-filter: blur(6px);
}

/* 箭头 */
.tooltip-content::before {
  content: '';
  position: absolute;
  width: 0;
  height: 0;
  border: 6px solid transparent;
}

.tooltip-content.top::before {
  bottom: -13px;
  left: 50%;
  transform: translateX(-50%);
  border-top-color: var(--border-color);
}

.tooltip-content.bottom::before {
  top: -13px;
  left: 50%;
  transform: translateX(-50%);
  border-bottom-color: var(--border-color);
}

.tooltip-content.left::before {
  right: -13px;
  top: 50%;
  transform: translateY(-50%);
  border-left-color: var(--border-color);
}

.tooltip-content.right::before {
  left: -13px;
  top: 50%;
  transform: translateY(-50%);
  border-right-color: var(--border-color);
}

.tooltip-title {
  font-size: 16px;
  font-weight: 600;
  margin-bottom: var(--spacing-sm);
  padding-bottom: var(--spacing-sm);
  border-bottom: 1px solid var(--border-color);
}

.tooltip-body {
  font-size: var(--font-size-base);
  color: var(--text-secondary);
  line-height: 1.6;
}

.tooltip-stats {
  margin: var(--spacing-sm) 0;
  padding: var(--spacing-sm) 0;
  border-top: 1px solid var(--border-color);
  border-bottom: 1px solid var(--border-color);
}

.stat-line {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 2px 0;
  font-size: var(--font-size-base);
}

.stat-name {
  color: var(--text-muted);
}

.stat-value {
  color: var(--text-primary);
  font-weight: 500;
}

.stat-value.positive {
  color: var(--quality-uncommon);
}

.stat-value.negative {
  color: var(--hp-color);
}

.tooltip-description {
  margin-top: var(--spacing-sm);
  padding-top: var(--spacing-sm);
  font-size: var(--font-size-base);
  color: var(--text-muted);
  font-style: italic;
  line-height: 1.5;
  border-top: 1px dashed var(--border-color);
}

/* 过渡动画 */
.tooltip-enter-active,
.tooltip-leave-active {
  transition: opacity 0.15s ease;
}

.tooltip-enter-from,
.tooltip-leave-to {
  opacity: 0;
}
</style>
