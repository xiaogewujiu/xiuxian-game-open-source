<template>
  <div class="array-editor">
    <div class="array-editor__header">
      <span>进入消耗列表</span>
      <button class="secondary-button" type="button" @click="addCost">新增</button>
    </div>

    <div v-if="draft.length === 0" class="empty-tip">当前没有配置项。</div>

    <div v-else class="array-editor__list">
      <div v-for="(item, index) in draft" :key="index" class="array-editor__row">
        <label class="field">
          <span>消耗类型</span>
          <select v-model="item.type" @change="onTypeChange(index)">
            <option v-for="opt in COST_TYPE_OPTIONS" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
          </select>
        </label>
        <label v-if="item.type === 'Item'" class="field">
          <span>道具</span>
          <select v-model="item.itemId" @change="emitChange">
            <option value="">请选择道具</option>
            <option v-for="opt in itemOptions" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
          </select>
        </label>
        <label class="field">
          <span>数量</span>
          <input v-model.number="item.quantity" type="number" min="1" @input="emitChange" />
        </label>
        <label class="field">
          <span>操作</span>
          <button class="danger-button danger-button--small" type="button" @click="removeCost(index)">删除</button>
        </label>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'

interface CostEntry {
  type: string
  itemId: string
  quantity: number
}

const COST_TYPE_OPTIONS = [
  { value: 'Gold', label: '金币' },
  { value: 'SpiritStone', label: '灵石' },
  { value: 'Item', label: '道具' }
]

const props = defineProps<{
  modelValue: string | null | undefined
  itemOptions: { value: string; label: string }[]
}>()

const emit = defineEmits<{ 'update:modelValue': [string | null] }>()
const draft = ref<CostEntry[]>([])

watch(() => props.modelValue, (value) => {
  if (!value) { draft.value = []; return }
  try {
    const parsed = JSON.parse(value)
    draft.value = (parsed || []).map((item: any) => ({
      type: item.type || 'Gold',
      itemId: item.id || '',
      quantity: item.quantity || item.amount || 0
    }))
  } catch { draft.value = [] }
}, { immediate: true })

function onTypeChange(index: number) {
  if (draft.value[index].type !== 'Item') {
    draft.value[index].itemId = ''
  }
  emitChange()
}

function emitChange() {
  const costs = draft.value
    .filter(c => c.quantity > 0)
    .map(c => {
      if (c.type === 'Gold' || c.type === 'SpiritStone') {
        return { type: c.type, quantity: c.quantity }
      }
      return { type: 'Item', id: c.itemId, quantity: c.quantity }
    })
    .filter(c => c.type !== 'Item' || (c as any).id)
  emit('update:modelValue', costs.length > 0 ? JSON.stringify(costs) : null)
}

function addCost() {
  draft.value.push({ type: 'Gold', itemId: '', quantity: 1 })
  emitChange()
}

function removeCost(index: number) {
  draft.value.splice(index, 1)
  emitChange()
}
</script>

<style scoped>
.array-editor__row {
  display: grid;
  grid-template-columns: 1fr 1fr 100px 80px;
  gap: 8px;
  align-items: end;
}
</style>
