<template>
  <div class="array-editor">
    <div class="array-editor__header">
      <span>{{ title }}</span>
      <button class="secondary-button" type="button" @click="addItem">新增</button>
    </div>

    <div v-if="draft.length === 0" class="empty-tip">当前没有数据。</div>

    <div v-else class="array-editor__list">
      <div v-for="(item, index) in draft" :key="`${item}-${index}`" class="array-editor__row array-editor__row--single">
        <select v-if="options && options.length" v-model="draft[index]" @change="emitChange">
          <option value="">请选择</option>
          <option v-for="opt in options" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
        </select>
        <input v-else v-model.trim="draft[index]" type="text" @input="emitChange" />
        <button class="danger-button danger-button--small" type="button" @click="removeItem(index)">删除</button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'

export interface StringListOption {
  value: string
  label: string
}

const props = defineProps<{
  modelValue: string[]
  title: string
  options?: StringListOption[]
}>()
const emit = defineEmits<{ 'update:modelValue': [string[]] }>()

const draft = ref<string[]>([])

watch(() => props.modelValue, (value) => {
  draft.value = [...(value || [])]
}, { immediate: true, deep: true })

function emitChange() {
  emit('update:modelValue', draft.value.map((item) => item.trim()).filter(Boolean))
}

function addItem() {
  draft.value.push('')
  emitChange()
}

function removeItem(index: number) {
  draft.value.splice(index, 1)
  emitChange()
}
</script>
