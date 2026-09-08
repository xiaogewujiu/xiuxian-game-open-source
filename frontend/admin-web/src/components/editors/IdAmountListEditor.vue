<template>
  <div class="array-editor">
    <div class="array-editor__header">
      <span>{{ title }}</span>
      <button class="secondary-button" type="button" @click="addItem">新增</button>
    </div>

    <div v-if="draft.length === 0" class="empty-tip">当前没有配置项。</div>

    <div v-else class="array-editor__list">
      <div v-for="(item, index) in draft" :key="`${item.id}-${index}`" class="array-editor__row array-editor__row--three">
        <label class="field">
          <span>{{ idLabel }}</span>
          <select v-if="options.length > 0" v-model="item.id" @change="emitChange">
            <option value="">请选择</option>
            <option v-for="option in options" :key="option.value" :value="option.value">{{ option.label }}</option>
          </select>
          <input v-else v-model.trim="item.id" type="text" @input="emitChange" />
        </label>
        <label class="field">
          <span>{{ amountLabel }}</span>
          <input v-model.number="item.amount" type="number" min="1" @input="emitChange" />
        </label>
        <label class="field">
          <span>操作</span>
          <button class="danger-button danger-button--small" type="button" @click="removeItem(index)">删除</button>
        </label>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import type { IdAmountEntry } from '@/types/admin'

type OptionItem = {
  value: string
  label: string
}

const props = withDefaults(defineProps<{
  modelValue: IdAmountEntry[]
  title: string
  idLabel?: string
  amountLabel?: string
  options?: OptionItem[]
}>(), {
  idLabel: '编号',
  amountLabel: '数量',
  options: () => []
})

const emit = defineEmits<{ 'update:modelValue': [IdAmountEntry[]] }>()
const draft = ref<IdAmountEntry[]>([])

watch(() => props.modelValue, (value) => {
  draft.value = (value || []).map((item) => ({ ...item }))
}, { immediate: true, deep: true })

function emitChange() {
  emit('update:modelValue', draft.value
    .map((item) => ({ id: item.id.trim(), amount: Number(item.amount) || 0 }))
    .filter((item) => item.id && item.amount > 0))
}

function addItem() {
  draft.value.push({ id: '', amount: 1 })
}

function removeItem(index: number) {
  draft.value.splice(index, 1)
  emitChange()
}
</script>
