<template>
  <!-- 修仙风格进度条组件 -->
  <div class="progress-wrapper" :class="{ vertical }">
    <!-- 标签区域 -->
    <div v-if="showLabel || showValue" class="progress-label">
      <span v-if="showLabel" class="label-text">{{ label }}</span>
      <span v-if="showValue" class="value-text">{{ current }} / {{ max }}</span>
    </div>

    <!-- 进度条 -->
    <div class="progress-track" :class="type">
      <div class="progress-fill"
           :class="[type, { animated }]"
           :style="fillStyle"
      >
        <!-- 灵气粒子效果 -->
        <div v-if="spiritEffect" class="spirit-particles">
          <div v-for="n in 3" :key="n" class="particle" :style="{ animationDelay: n * 0.3 + 's' }"></div>
        </div>
      </div>

      <!-- 分段标记 -->
      <div v-if="segments" class="progress-segments">
        <div v-for="n in segments - 1" :key="n" class="segment-mark" :style="{ left: (n / segments * 100) + '%' }"></div>
      </div>
    </div>

    <!-- 百分比显示 -->
    <div v-if="showPercent" class="progress-percent">
      {{ percent }}%
    </div>
  </div>
</template>

<script>
import { computed } from 'vue'

/**
 * XiuXianProgress - 修仙风格进度条组件
 *
 * 功能：
 * - 多种类型（hp/mp/exp/修为）
 * - 灵气粒子特效
 * - 分段标记
 * - 垂直/水平方向
 *
 * Props:
 * - current: 当前值
 * - max: 最大值
 * - type: 类型 (hp/mp/exp/cultivation/custom)
 * - label: 标签文字
 * - showLabel: 是否显示标签
 * - showValue: 是否显示数值
 * - showPercent: 是否显示百分比
 * - height: 高度
 * - vertical: 是否垂直
 * - animated: 是否启用动画
 * - spiritEffect: 是否启用灵气特效
 * - segments: 分段数量
 */
export default {
  name: 'XiuXianProgress',

  props: {
    current: {
      type: Number,
      default: 0
    },
    max: {
      type: Number,
      default: 100
    },
    type: {
      type: String,
      default: 'custom',
      validator: (val) => ['hp', 'mp', 'stamina', 'exp', 'cultivation', 'custom'].includes(val)
    },
    label: {
      type: String,
      default: ''
    },
    showLabel: {
      type: Boolean,
      default: true
    },
    showValue: {
      type: Boolean,
      default: false
    },
    showPercent: {
      type: Boolean,
      default: false
    },
    height: {
      type: Number,
      default: 16
    },
    vertical: {
      type: Boolean,
      default: false
    },
    animated: {
      type: Boolean,
      default: true
    },
    spiritEffect: {
      type: Boolean,
      default: true
    },
    segments: {
      type: Number,
      default: 0
    }
  },

  setup(props) {
    // 计算百分比
    const percent = computed(() => {
      const p = (props.current / props.max) * 100
      return Math.min(Math.max(p, 0), 100).toFixed(1)
    })

    // 填充样式
    const fillStyle = computed(() => {
      if (props.vertical) {
        return {
          height: percent.value + '%',
          width: '100%'
        }
      }
      return {
        width: percent.value + '%',
        height: '100%'
      }
    })

    return {
      percent,
      fillStyle
    }
  }
}
</script>

<style scoped>
.progress-wrapper {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-xs);
}

.progress-wrapper.vertical {
  flex-direction: row;
  align-items: stretch;
  height: 100%;
}

/* 标签区域 */
.progress-label {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: var(--font-size-base);
}

.label-text {
  color: var(--text-secondary);
}

.value-text {
  color: var(--text-primary);
  font-family: var(--font-mono);
}

/* 进度条轨道 */
.progress-track {
  position: relative;
  flex: 1;
  background: var(--xiuxian-bg-secondary);
  border-radius: var(--radius-md);
  overflow: hidden;
  border: 1px solid var(--border-color);
}

.progress-track.exp {
  background: var(--progress-track-exp-bg);
}

.progress-wrapper:not(.vertical) .progress-track {
  height: v-bind(height + 'px');
}

.progress-wrapper.vertical .progress-track {
  width: v-bind(height + 'px');
}

/* 进度条填充 */
.progress-fill {
  position: relative;
  border-radius: var(--radius-md);
  transition: all 0.15s ease;
  overflow: hidden;
}

/* 不同类型颜色 */
.progress-fill.hp {
  background: var(--hp-color);
}

.progress-fill.mp {
  background: var(--mp-color);
}

.progress-fill.stamina {
  background: var(--stamina-color);
}

.progress-fill.exp {
  background: var(--exp-color);
}

.progress-fill.cultivation {
  background: var(--button-primary-start);
}

.progress-fill.custom {
  background: var(--button-primary-start);
}

/* 灵气粒子效果 */
.spirit-particles {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  overflow: hidden;
  pointer-events: none;
}

.particle {
  position: absolute;
  width: 4px;
  height: 4px;
  background: var(--spirit-light);
  border-radius: 50%;
  animation: float-up 2s ease-in-out infinite;
  opacity: 0;
}

.particle:nth-child(1) { left: 20%; }
.particle:nth-child(2) { left: 50%; }
.particle:nth-child(3) { left: 80%; }

@keyframes float-up {
  0% {
    bottom: 0;
    opacity: 0;
    transform: scale(0);
  }
  20% {
    opacity: 1;
    transform: scale(1);
  }
  80% {
    opacity: 0.6;
  }
  100% {
    bottom: 100%;
    opacity: 0;
    transform: scale(0.5);
  }
}

/* 分段标记 */
.progress-segments {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  pointer-events: none;
}

.segment-mark {
  position: absolute;
  top: 0;
  bottom: 0;
  width: 2px;
  background: var(--border-color);
}

/* 百分比显示 */
.progress-percent {
  font-size: var(--font-size-base);
  color: var(--text-muted);
  font-family: var(--font-mono);
  text-align: center;
}
</style>
