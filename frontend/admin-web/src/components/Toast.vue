<template>
  <div class="toast-item" :class="['toast-' + type, { 'toast-leaving': leaving }]">
    <span class="toast-icon">{{ icon }}</span>
    <span class="toast-message">{{ message }}</span>
    <button class="toast-close" @click="close">&times;</button>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed } from 'vue'

const props = defineProps<{
  message: string
  type?: 'info' | 'success' | 'warning' | 'error'
  duration?: number
  onClose?: () => void
}>()

const leaving = ref(false)
let timer: ReturnType<typeof setTimeout> | null = null

const icon = computed(() => {
  const icons: Record<string, string> = { success: '✓', error: '✗', warning: '⚠', info: 'ℹ' }
  return icons[props.type || 'info'] || icons.info
})

const close = () => {
  leaving.value = true
  setTimeout(() => {
    props.onClose?.()
  }, 300)
}

onMounted(() => {
  if ((props.duration ?? 3000) > 0) {
    timer = setTimeout(close, props.duration ?? 3000)
  }
})

onUnmounted(() => {
  if (timer) clearTimeout(timer)
})
</script>
