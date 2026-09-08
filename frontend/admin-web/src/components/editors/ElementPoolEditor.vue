<template>
  <div class="array-editor">
    <div class="array-editor__header">
      <span>元素池</span>
      <button class="secondary-button" type="button" @click="addItem">新增元素</button>
    </div>

    <div v-if="draft.length === 0" class="empty-tip">当前没有元素配置。</div>

    <div v-else class="array-editor__list">
      <div v-for="(item, index) in draft" :key="`${item}-${index}`" class="array-editor__row array-editor__row--single">
        <select v-model.number="draft[index]" @change="emitChange">
          <option v-for="option in options" :key="option.value" :value="option.value">{{ option.label }}</option>
        </select>
        <button class="danger-button danger-button--small" type="button" @click="removeItem(index)">删除</button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'

const props = defineProps<{ modelValue: number[] }>()
const emit = defineEmits<{ 'update:modelValue': [number[]] }>()

const options = [
  { value: 1, label: '金' },
  { value: 2, label: '木' },
  { value: 3, label: '水' },
  { value: 4, label: '火' },
  { value: 5, label: '土' },
  { value: 6, label: '风' },
  { value: 7, label: '冰' },
  { value: 8, label: '雷' }
]

const draft = ref<number[]>([])

watch(() => props.modelValue, (value) => {
  draft.value = [...(value || [])]
}, { immediate: true, deep: true })

function emitChange() {
  emit('update:modelValue', draft.value.filter((item) => Number.isFinite(item) && item > 0))
}

function addItem() {
  draft.value.push(1)
  emitChange()
}

function removeItem(index: number) {
  draft.value.splice(index, 1)
  emitChange()
}
</script>
