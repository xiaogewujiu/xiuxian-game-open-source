<template>
  <div class="toast-item" :class="['toast-' + type, { 'toast-leaving': leaving }]">
    <span class="toast-icon">{{ icon }}</span>
    <span class="toast-message">{{ message }}</span>
    <button class="toast-close" @click="close">&times;</button>
  </div>
</template>

<script>
import { ref, onMounted, onUnmounted, computed } from 'vue'

export default {
  name: 'ToastItem',
  props: {
    message: { type: String, default: '' },
    type: { type: String, default: 'info' },
    duration: { type: Number, default: 3000 },
    onClose: { type: Function, default: null }
  },
  setup(props) {
    const leaving = ref(false)
    let timer = null

    const icon = computed(() => {
      const icons = { success: '✓', error: '✗', warning: '⚠', info: 'ℹ' }
      return icons[props.type] || icons.info
    })

    const close = () => {
      leaving.value = true
      setTimeout(() => {
        props.onClose?.()
      }, 300)
    }

    onMounted(() => {
      if (props.duration > 0) {
        timer = setTimeout(close, props.duration)
      }
    })

    onUnmounted(() => {
      if (timer) clearTimeout(timer)
    })

    return { leaving, icon, close }
  }
}
</script>
