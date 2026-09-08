<template>
  <div class="array-editor">
    <div class="array-editor__header">
      <span>炼丹材料</span>
      <button class="secondary-button" type="button" @click="addItem">新增材料</button>
    </div>

    <div v-if="draft.length === 0" class="empty-tip">当前没有材料配置。</div>

    <div v-else class="array-editor__list">
      <div v-for="(item, index) in draft" :key="`${item.itemId}-${index}`" class="array-editor__row array-editor__row--three">
        <label class="field"><span>材料道具</span><select v-if="options.length > 0" v-model="item.itemId" @change="applyOptionMeta(item); emitChange()"><option value="">请选择</option><option v-for="option in options" :key="option.value" :value="option.value">{{ option.label }}</option></select><input v-else v-model.trim="item.itemId" type="text" @input="emitChange" /></label>
        <label class="field"><span>材料名称</span><input v-model.trim="item.itemName" type="text" @input="emitChange" /></label>
        <label class="field"><span>数量</span><input v-model.number="item.amount" type="number" min="1" @input="emitChange" /></label>
        <label class="field checkbox-field"><input v-model="item.isReplaceable" type="checkbox" @change="emitChange" /><span>允许替代</span></label>
        <label class="field field--full"><span>替代材料编号</span><div class="inline-action"><input v-model.trim="item.alternativeItemIdsText" type="text" placeholder="多个编号用英文逗号分隔" @input="emitChange" /><button class="danger-button danger-button--small" type="button" @click="removeItem(index)">删除</button></div></label>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import type { AlchemyMaterialEntry } from '@/types/admin'

type OptionItem = { value: string; label: string; name?: string }
type DraftAlchemyMaterial = AlchemyMaterialEntry & { alternativeItemIdsText: string }

const props = withDefaults(defineProps<{ modelValue: AlchemyMaterialEntry[]; options?: OptionItem[] }>(), { options: () => [] })
const emit = defineEmits<{ 'update:modelValue': [AlchemyMaterialEntry[]] }>()
const draft = ref<DraftAlchemyMaterial[]>([])

watch(() => props.modelValue, (value) => {
  draft.value = (value || []).map((item) => ({ ...item, alternativeItemIdsText: (item.alternativeItemIds || []).join(', ') }))
}, { immediate: true, deep: true })

function applyOptionMeta(item: DraftAlchemyMaterial) {
  const option = props.options.find((entry) => entry.value === item.itemId)
  if (option && !item.itemName) {
    item.itemName = option.name || option.label
  }
}

function normalize(item: DraftAlchemyMaterial): AlchemyMaterialEntry {
  return {
    itemId: item.itemId.trim(),
    itemName: item.itemName.trim(),
    amount: Math.max(1, Number(item.amount) || 1),
    isReplaceable: Boolean(item.isReplaceable),
    alternativeItemIds: item.alternativeItemIdsText.split(',').map((entry) => entry.trim()).filter(Boolean)
  }
}

function emitChange() {
  emit('update:modelValue', draft.value.map(normalize).filter((item) => item.itemId))
}

function addItem() {
  draft.value.push({ itemId: '', itemName: '', amount: 1, isReplaceable: false, alternativeItemIds: [], alternativeItemIdsText: '' })
  emitChange()
}

function removeItem(index: number) {
  draft.value.splice(index, 1)
  emitChange()
}
</script>
