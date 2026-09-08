<template>
  <div class="field image-field">
    <span>{{ label }}</span>
    <div class="image-field-body">
      <div v-if="previewUrl" class="image-preview" :style="{ width: previewWidth + 'px', height: previewHeight + 'px' }">
        <img :src="previewUrl" :alt="label" />
      </div>
      <div v-else class="image-placeholder" :style="{ width: previewWidth + 'px', height: previewHeight + 'px' }">暂无图片</div>

      <div class="image-actions">
        <input
          :value="modelValue || ''"
          type="text"
          :placeholder="placeholder"
          @input="handleTextInput"
        />
        <div class="button-row">
          <button class="secondary-button" type="button" :disabled="uploading" @click="triggerPick">
            {{ uploading ? '上传中...' : '上传图片' }}
          </button>
          <button class="ai-button" type="button" :disabled="generating" @click="toggleAiPanel">
            {{ generating ? '生成中...' : 'AI 生成' }}
          </button>
          <template v-if="showAiPanel">
            <input
              v-model="aiPrompt"
              type="text"
              class="ai-prompt-input"
              placeholder="输入图标描述，如：一把红色的仙剑"
              @keyup.enter="handleGenerate"
            />
            <button
              class="ai-generate-btn"
              type="button"
              :disabled="generating || !aiPrompt.trim()"
              @click="handleGenerate"
            >
              {{ generating ? '...' : '生成' }}
            </button>
          </template>
          <button class="secondary-button" type="button" :disabled="!modelValue" @click="clearValue">
            清空
          </button>
        </div>
        <div v-if="generating" class="ai-status">
          <span class="ai-spinner"></span>
          正在生成图标，请稍候...
        </div>
        <input ref="fileInputRef" type="file" accept="image/*" hidden @change="handleChange" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { buildAdminApiUrl } from '@/services/http'
import { uploadAdminAsset, generateIcon } from '@/services/assets'
import toast from '@/utils/toast'

const props = withDefaults(defineProps<{
  modelValue?: string | null
  label: string
  uploadCategory: string
  placeholder?: string
  previewWidth?: number
  previewHeight?: number
}>(), {
  previewWidth: 120,
  previewHeight: 120
})

const emit = defineEmits<{
  (e: 'update:modelValue', value: string): void
}>()

const fileInputRef = ref<HTMLInputElement | null>(null)
const uploading = ref(false)
const generating = ref(false)
const showAiPanel = ref(false)
const aiPrompt = ref('')

const previewUrl = computed(() => {
  const raw = (props.modelValue || '').trim()
  if (!raw) {
    return ''
  }

  return /^https?:\/\//i.test(raw) ? raw : buildAdminApiUrl(raw)
})

function triggerPick() {
  fileInputRef.value?.click()
}

function handleTextInput(event: Event) {
  const target = event.target as HTMLInputElement | null
  emit('update:modelValue', target?.value || '')
}

function clearValue() {
  emit('update:modelValue', '')
}

function toggleAiPanel() {
  showAiPanel.value = !showAiPanel.value
}

async function handleGenerate() {
  const prompt = aiPrompt.value.trim()
  if (!prompt) return

  generating.value = true
  try {
    const result = await generateIcon(prompt, props.uploadCategory, props.modelValue)
    emit('update:modelValue', result.relativePath)
    toast.success('图标生成成功！')
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '图标生成失败。')
  } finally {
    generating.value = false
  }
}

async function handleChange(event: Event) {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]
  target.value = ''
  if (!file) {
    return
  }

  uploading.value = true
  try {
    const result = await uploadAdminAsset(props.uploadCategory, file)
    emit('update:modelValue', result.relativePath)
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '上传失败。')
  } finally {
    uploading.value = false
  }
}
</script>

<style scoped>
.image-field-body {
  display: grid;
  grid-template-columns: auto minmax(0, 1fr);
  gap: 12px;
  align-items: start;
}

.image-preview,
.image-placeholder {
  border-radius: 10px;
  border: 1px solid var(--border-color);
  background: #f8fafc;
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
  color: #64748b;
  font-size: 12px;
}

.image-preview img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.image-actions {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.button-row {
  display: flex;
  gap: 8px;
  align-items: center;
  flex-wrap: wrap;
}

.ai-button {
  background: linear-gradient(135deg, #8b5cf6, #6366f1);
  color: white;
  border: none;
  padding: 6px 14px;
  border-radius: 6px;
  cursor: pointer;
  font-size: 13px;
  font-weight: 500;
  transition: opacity 0.2s;
}

.ai-button:hover:not(:disabled) {
  opacity: 0.85;
}

.ai-button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.ai-prompt-input {
  flex: 1;
  min-width: 120px;
  padding: 6px 10px;
  border: 1px solid #cbd5e1;
  border-radius: 6px;
  font-size: 13px;
  outline: none;
  transition: border-color 0.2s;
}

.ai-prompt-input:focus {
  border-color: #8b5cf6;
}

.ai-generate-btn {
  background: #8b5cf6;
  color: white;
  border: none;
  padding: 6px 14px;
  border-radius: 6px;
  cursor: pointer;
  font-size: 13px;
  font-weight: 600;
  white-space: nowrap;
  transition: background 0.2s;
}

.ai-generate-btn:hover:not(:disabled) {
  background: #7c3aed;
}

.ai-generate-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.ai-status {
  display: flex;
  align-items: center;
  gap: 8px;
  color: #6366f1;
  font-size: 13px;
}

.ai-spinner {
  width: 14px;
  height: 14px;
  border: 2px solid #e2e8f0;
  border-top-color: #6366f1;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

@media (max-width: 900px) {
  .image-field-body {
    grid-template-columns: 1fr;
  }
}
</style>
