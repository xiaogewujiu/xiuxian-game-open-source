<template>
  <div class="array-editor">
    <div class="array-editor__header">
      <span>事件组配置</span>
      <button class="secondary-button" type="button" @click="addRow">新增事件</button>
    </div>

    <div v-if="draft.length === 0" class="empty-tip">当前没有事件配置。</div>

    <div v-else class="array-editor__list">
      <div v-for="(row, index) in draft" :key="`${row.eventId}-${index}`" class="array-editor__row array-editor__row--three">
        <label class="field">
          <span>事件</span>
          <select v-model="draft[index].eventId" @change="emitChange">
            <option value="">请选择事件</option>
            <option v-for="opt in eventOptions" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
          </select>
        </label>
        <label class="field">
          <span>权重(1-10000)</span>
          <input v-model.number="draft[index].weight" type="number" min="1" max="10000" @input="emitChange" />
        </label>
        <div class="field">
          <span>操作</span>
          <button class="danger-button danger-button--small" type="button" @click="removeRow(index)">删除</button>
        </div>
      </div>
    </div>

    <div v-if="draft.length > 0" class="array-editor__summary">
      共 {{ draft.length }} 个事件，权重总和：{{ totalWeight }}
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import type { EventGroupItem } from '@/types/admin'

type OptionItem = {
  value: string
  label: string
}

const props = withDefaults(defineProps<{
  modelValue: EventGroupItem[]
  eventOptions?: OptionItem[]
}>(), {
  eventOptions: () => []
})

const emit = defineEmits<{ 'update:modelValue': [EventGroupItem[]] }>()
const draft = ref<EventGroupItem[]>([])

const totalWeight = computed(() => draft.value.reduce((sum, item) => sum + (item.weight || 0), 0))

watch(() => props.modelValue, (value) => {
  draft.value = (value || []).map((item) => ({ ...item }))
}, { immediate: true, deep: true })

function emitChange() {
  emit('update:modelValue', draft.value
    .map((row) => ({ eventId: row.eventId.trim(), weight: Number(row.weight) || 0 }))
    .filter((row) => row.eventId && row.weight > 0))
}

function addRow() {
  draft.value.push({ eventId: '', weight: 100 })
  emitChange()
}

function removeRow(index: number) {
  draft.value.splice(index, 1)
  emitChange()
}
</script>

<style scoped>
.array-editor__summary {
  margin-top: 8px;
  padding: 8px 12px;
  background: var(--surface-secondary, #f5f5f5);
  border-radius: 6px;
  font-size: 13px;
  color: var(--text-secondary);
}
</style>
