<template>
  <!-- 修仙风格 Modal 弹窗组件 -->
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="modelValue" class="modal-overlay" @click.self="handleOverlayClick">
        <div class="modal-container" :style="{ width: width + 'px' }">
          <!-- 弹窗头部 -->
          <div class="modal-header">
            <div class="modal-title">
              <span class="rune-icon" v-if="showIcon"></span>
              <slot name="title">{{ title }}</slot>
            </div>
            <button v-if="showClose" class="modal-close" @click="close">
              <span>×</span>
            </button>
          </div>

          <!-- 弹窗内容 -->
          <div class="modal-body">
            <slot></slot>
          </div>

          <!-- 弹窗底部 -->
          <div v-if="$slots.footer" class="modal-footer">
            <slot name="footer"></slot>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script>
/**
 * XiuXianModal - 修仙风格 Modal 弹窗组件
 *
 * 功能：
 * - 统一的修仙风格弹窗外观
 * - 支持自定义宽度
 * - 点击遮罩层关闭（可配置）
 * - 显示/隐藏动画
 *
 * Props:
 * - modelValue: 控制显示/隐藏
 * - title: 弹窗标题
 * - width: 弹窗宽度（默认 500）
 * - showClose: 是否显示关闭按钮（默认 true）
 * - showIcon: 是否显示符文图标（默认 true）
 * - closeOnOverlay: 点击遮罩层是否关闭（默认 true）
 *
 * Events:
 * - update:modelValue: 更新显示状态
 */
export default {
  name: 'XiuXianModal',

  props: {
    modelValue: {
      type: Boolean,
      default: false
    },
    title: {
      type: String,
      default: '提示'
    },
    width: {
      type: Number,
      default: 500
    },
    showClose: {
      type: Boolean,
      default: true
    },
    showIcon: {
      type: Boolean,
      default: true
    },
    closeOnOverlay: {
      type: Boolean,
      default: true
    }
  },

  emits: ['update:modelValue', 'close'],

  setup(props, { emit }) {
    // 关闭弹窗
    const close = () => {
      emit('update:modelValue', false)
      emit('close')
    }

    // 点击遮罩层
    const handleOverlayClick = () => {
      if (props.closeOnOverlay) {
        close()
      }
    }

    return {
      close,
      handleOverlayClick
    }
  }
}
</script>

<style scoped>
/* 遮罩层 */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: var(--overlay-bg);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  backdrop-filter: blur(3px);
}

/* 弹窗容器 */
.modal-container {
  background: var(--xiuxian-bg-panel);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-lg);
  box-shadow: var(--modal-shadow);
  position: relative;
  max-height: 90vh;
  display: flex;
  flex-direction: column;
}

/* 弹窗头部 */
.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--spacing-md) var(--spacing-lg);
  border-bottom: 1px solid var(--border-color);
  background: var(--modal-header-bg);
}

.modal-title {
  font-size: 18px;
  font-weight: 600;
  color: var(--text-primary);
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  letter-spacing: 2px;
}

/* 关闭按钮 */
.modal-close {
  width: 28px;
  height: 28px;
  background: var(--control-bg);
  border: 1px solid var(--control-border);
  border-radius: var(--radius-md);
  color: var(--text-secondary);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 20px;
  transition: all 0.15s ease;
}

.modal-close:hover {
  border-color: var(--border-color);
  color: var(--accent-text);
  background: var(--control-bg-hover);
}

/* 弹窗内容 */
.modal-body {
  padding: var(--spacing-lg);
  overflow-y: auto;
  flex: 1;
}

/* 弹窗底部 */
.modal-footer {
  padding: var(--spacing-md) var(--spacing-lg);
  border-top: 1px solid var(--border-color);
  display: flex;
  justify-content: flex-end;
  gap: var(--spacing-md);
}

/* 过渡动画 */
.modal-enter-active,
.modal-leave-active {
  transition: all 0.15s ease;
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}

.modal-enter-from .modal-container,
.modal-leave-to .modal-container {
  transform: scale(0.9) translateY(-20px);
  opacity: 0;
}
</style>
