<template>
  <div class="array-editor">
    <div class="array-editor__header">
      <span>{{ title }}</span>
      <div class="inline-action inline-action--compact">
        <select v-model="selectedValue">
          <option value="">请选择</option>
          <option v-for="option in availableOptions" :key="option.value" :value="option.value">{{ option.label }}</option>
        </select>
        <button class="secondary-button" type="button" @click="addItem">添加</button>
      </div>
    </div>

    <div v-if="draft.length === 0" class="empty-tip">当前没有已选项。</div>

    <div v-else class="array-editor__list">
      <div v-for="item in draft" :key="item" class="array-editor__row array-editor__row--single">
        <div class="selected-pill">{{ getOptionLabel(item) }}</div>
        <button class="danger-button danger-button--small" type="button" @click="removeItem(item)">删除</button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'

type OptionItem = {
  value: string
  label: string
}

const props = defineProps<{
  modelValue: string[]
  title: string
  options: OptionItem[]
}>()

const emit = defineEmits<{ 'update:modelValue': [string[]] }>()

const draft = ref<string[]>([])
const selectedValue = ref('')

watch(() => props.modelValue, (value) => {
  draft.value = [...(value || [])]
}, { immediate: true, deep: true })

const availableOptions = computed(() => props.options.filter((option) => !draft.value.includes(option.value)))

function emitChange() {
  emit('update:modelValue', draft.value.filter(Boolean))
}

function addItem() {
  if (!selectedValue.value || draft.value.includes(selectedValue.value)) {
    return
  }

  draft.value.push(selectedValue.value)
  selectedValue.value = ''
  emitChange()
}

function removeItem(item: string) {
  draft.value = draft.value.filter((value) => value !== item)
  emitChange()
}

function getOptionLabel(value: string) {
  return props.options.find((option) => option.value === value)?.label || value
}
</script>
