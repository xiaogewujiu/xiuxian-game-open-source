<template>
  <div class="array-editor">
    <div class="array-editor__header">
      <span>{{ title }}</span>
      <button class="secondary-button" type="button" @click="addRow">新增</button>
    </div>

    <div v-if="draft.length === 0" class="empty-tip">当前没有图鉴掉落配置。</div>

    <div v-else class="array-editor__list">
      <div v-for="(row, index) in draft" :key="index" class="array-editor__row array-editor__row--three">
        <label class="field">
          <span>图鉴系列</span>
          <select v-model="draft[index].seriesId" @change="onSeriesChange(index)">
            <option value="">请选择</option>
            <option v-for="option in options" :key="option.value" :value="option.value">
              {{ option.label }}
            </option>
          </select>
        </label>
        <label class="field">
          <span>类型</span>
          <select v-model.number="draft[index].collectionType" @change="emitChange">
            <option :value="0">文字图鉴</option>
            <option :value="1">图片图鉴</option>
          </select>
        </label>
        <label class="field">
          <span>概率(万分比)</span>
          <input v-model.number="draft[index].rate" type="number" min="0" max="10000" step="100" @input="emitChange" />
        </label>
        <div class="field">
          <span>操作</span>
          <button class="danger-button danger-button--small" type="button" @click="removeRow(index)">删除</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import type { DropCollection } from '@/types/admin'

type OptionItem = {
  value: string
  label: string
  collectionType: number
}

const props = withDefaults(defineProps<{
  modelValue: DropCollection[]
  title: string
  options?: OptionItem[]
}>(), {
  options: () => []
})
const emit = defineEmits<{ 'update:modelValue': [DropCollection[]] }>()

const draft = ref<DropCollection[]>([])

watch(() => props.modelValue, (value) => {
  draft.value = (value || []).map((item) => ({ ...item }))
}, { immediate: true, deep: true })

function emitChange() {
  emit('update:modelValue', draft.value
    .map((row) => ({ ...row, rate: Number(row.rate) || 0 })))
}

function onSeriesChange(index: number) {
  const selected = props.options.find(o => o.value === draft.value[index].seriesId)
  if (selected) {
    draft.value[index].collectionType = selected.collectionType
  }
  emitChange()
}

function addRow() {
  draft.value.push({ seriesId: '', collectionType: 0, rate: 0 })
  emitChange()
}

function removeRow(index: number) {
  draft.value.splice(index, 1)
  emitChange()
}
</script>
