<template>
  <img
    v-if="shouldRenderImage"
    :src="resolvedImageUrl"
    :alt="alt"
    class="asset-image"
    :style="sizeStyle"
    @error="handleImageError"
  >
  <img
    v-else-if="shouldRenderFallbackImage"
    :src="resolvedFallbackImageUrl"
    :alt="alt"
    class="asset-image"
    :style="sizeStyle"
    @error="handleFallbackImageError"
  >
  <span v-else class="asset-text" :style="sizeStyle">{{ fallbackText }}</span>
</template>

<script setup>
import { computed, ref, watch } from 'vue'
import { buildApiUrl } from '../../lib/apiClient'

const props = defineProps({
  source: {
    type: String,
    default: ''
  },
  fallback: {
    type: String,
    default: '🎒'
  },
  alt: {
    type: String,
    default: ''
  },
  size: {
    type: [String, Number],
    default: null
  }
})

const sizeStyle = computed(() => {
  if (!props.size) return {}
  const raw = String(props.size)
  if (/^\d+$/.test(raw)) {
    const boosted = Math.round(Number(raw) * 1.2)
    return { width: `${boosted}px`, height: `${boosted}px`, 'flex-shrink': '0' }
  }
  return { width: raw, height: raw, 'flex-shrink': '0' }
})

const imageFailed = ref(false)
const fallbackImageFailed = ref(false)

watch(() => props.source, () => {
  imageFailed.value = false
  fallbackImageFailed.value = false
})

watch(() => props.fallback, () => {
  fallbackImageFailed.value = false
})

function isImageSource(value) {
  const normalized = String(value || '').trim()
  if (!normalized) return false
  const pathOnly = normalized.split('?')[0]
  return /^https?:\/\//i.test(normalized) ||
    normalized.startsWith('/uploads/') ||
    normalized.startsWith('uploads/') ||
    normalized.startsWith('data:image/') ||
    normalized.startsWith('/src/') ||
    normalized.startsWith('/assets/') ||
    /\.(png|jpe?g|webp|gif|svg)$/i.test(pathOnly)
}

const shouldRenderImage = computed(() => {
  const value = String(props.source || '').trim()
  if (!value || imageFailed.value) {
    return false
  }
  return isImageSource(value)
})

const resolvedImageUrl = computed(() => {
  const value = String(props.source || '').trim()
  if (!value) {
    return ''
  }

  // 绝对 URL 或 data URI 直接使用
  if (/^https?:\/\//i.test(value) || value.startsWith('data:image/')) {
    return value
  }

  // Vite 构建资源（/src/...、/assets/...）直接使用，不拼后端地址
  if (value.startsWith('/src/') || value.startsWith('/assets/')) {
    return value
  }

  // 上传文件等后端资源走 buildApiUrl
  return buildApiUrl(value.startsWith('/') ? value : `/${value}`)
})

const shouldRenderFallbackImage = computed(() => {
  if (fallbackImageFailed.value || shouldRenderImage.value) {
    return false
  }

  return isImageSource(props.fallback)
})

const resolvedFallbackImageUrl = computed(() => {
  const value = String(props.fallback || '').trim()
  if (!value) return ''
  if (/^https?:\/\//i.test(value) || value.startsWith('data:image/')) {
    return value
  }
  if (value.startsWith('/src/') || value.startsWith('/assets/')) {
    return value
  }
  return buildApiUrl(value.startsWith('/') ? value : `/${value}`)
})

const fallbackText = computed(() => {
  const value = String(props.source || '').trim()
  const fallback = String(props.fallback || '').trim()

  // 图片回退地址加载失败时不能把 /assets/... 或上传图片路径当成文字显示。
  // 此时交给空文本占位，避免装备栏出现图片路径。
  if (isImageSource(fallback) && fallbackImageFailed.value) {
    return ''
  }

  if (!value || shouldRenderImage.value || shouldRenderFallbackImage.value || imageFailed.value) {
    return props.fallback
  }

  return value
})

function handleImageError() {
  imageFailed.value = true
}

function handleFallbackImageError() {
  fallbackImageFailed.value = true
}
</script>

<style scoped>
.asset-image {
  object-fit: contain;
  display: inline-block;
  vertical-align: middle;
}

.asset-text {
  display: flex;
  align-items: center;
  justify-content: center;
}
</style>
