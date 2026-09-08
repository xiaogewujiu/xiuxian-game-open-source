<template>
  <div class="array-editor">
    <div class="array-editor__header">
      <span>{{ title }}</span>
      <button class="secondary-button" type="button" @click="addRow">新增</button>
    </div>

    <div v-if="draft.length === 0" class="empty-tip">当前没有掉落配置。</div>

    <div v-else class="array-editor__list">
      <div v-for="(row, index) in draft" :key="`${index}`" class="array-editor__row array-editor__row--three">
        <label class="field">
          <span>{{ idLabel }}</span>
          <select v-if="options.length > 0" v-model="draft[index][idKey]" @change="emitChange">
            <option value="">请选择</option>
            <option v-for="option in options" :key="option.value" :value="option.value">{{ option.label }}</option>
          </select>
          <input v-else v-model.trim="draft[index][idKey]" type="text" @input="emitChange" />
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
import type { DropEquipment, DropItem } from '@/types/admin'

type DropRow = {
  itemId?: string
  equipmentId?: string
  rate: number
}

type OptionItem = {
  value: string
  label: string
}

const props = withDefaults(defineProps<{
  modelValue: Array<DropItem | DropEquipment>
  idKey: 'itemId' | 'equipmentId'
  idLabel: string
  title: string
  options?: OptionItem[]
}>(), {
  options: () => []
})
const emit = defineEmits<{ 'update:modelValue': [Array<DropItem | DropEquipment>] }>()

const draft = ref<DropRow[]>([])

watch(() => props.modelValue, (value) => {
  draft.value = (value || []).map((item) => ({ ...item }))
}, { immediate: true, deep: true })

function emitChange() {
  emit('update:modelValue', draft.value
    .map((row) => ({ ...row, rate: Number(row.rate) || 0 })) as Array<DropItem | DropEquipment>)
}

function addRow() {
  if (props.idKey === 'itemId') {
    draft.value.push({ itemId: '', rate: 0 })
  } else {
    draft.value.push({ equipmentId: '', rate: 0 })
  }
  emitChange()
}

function removeRow(index: number) {
  draft.value.splice(index, 1)
  emitChange()
}
</script>
