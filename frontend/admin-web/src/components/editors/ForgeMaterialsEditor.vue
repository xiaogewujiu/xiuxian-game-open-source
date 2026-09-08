<template>
  <div class="array-editor">
    <div class="array-editor__header">
      <span>锻造材料</span>
      <button class="secondary-button" type="button" @click="addItem">新增材料</button>
    </div>

    <div v-if="draft.length === 0" class="empty-tip">当前没有材料配置。</div>

    <div v-else class="array-editor__list">
      <div v-for="(item, index) in draft" :key="`${item.itemId}-${index}`" class="array-editor__row array-editor__row--three">
        <label class="field"><span>材料道具</span><select v-if="options.length > 0" v-model="item.itemId" @change="applyOptionMeta(item); emitChange()"><option value="">请选择</option><option v-for="option in options" :key="option.value" :value="option.value">{{ option.label }}</option></select><input v-else v-model.trim="item.itemId" type="text" @input="emitChange" /></label>
        <label class="field"><span>材料名称</span><input v-model.trim="item.name" type="text" @input="emitChange" /></label>
        <label class="field"><span>图标</span><input v-model.trim="item.icon" type="text" @input="emitChange" /></label>
        <label class="field"><span>数量</span><input v-model.number="item.count" type="number" min="1" @input="emitChange" /></label>
        <label class="field"><span>操作</span><button class="danger-button danger-button--small" type="button" @click="removeItem(index)">删除</button></label>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import type { ForgeMaterialEntry } from '@/types/admin'

type OptionItem = { value: string; label: string; name?: string }
const props = withDefaults(defineProps<{ modelValue: ForgeMaterialEntry[]; options?: OptionItem[] }>(), { options: () => [] })
const emit = defineEmits<{ 'update:modelValue': [ForgeMaterialEntry[]] }>()
const draft = ref<ForgeMaterialEntry[]>([])

watch(() => props.modelValue, (value) => { draft.value = (value || []).map((item) => ({ ...item })) }, { immediate: true, deep: true })

function applyOptionMeta(item: ForgeMaterialEntry) {
  const option = props.options.find((entry) => entry.value === item.itemId)
  if (option && !item.name) {
    item.name = option.name || option.label
  }
}

function emitChange() {
  emit('update:modelValue', draft.value.map((item) => ({ itemId: item.itemId.trim(), name: item.name.trim(), icon: item.icon.trim(), count: Math.max(1, Number(item.count) || 1) })).filter((item) => item.itemId))
}

function addItem() {
  draft.value.push({ itemId: '', name: '', icon: '', count: 1 })
  emitChange()
}

function removeItem(index: number) {
  draft.value.splice(index, 1)
  emitChange()
}
</script>
