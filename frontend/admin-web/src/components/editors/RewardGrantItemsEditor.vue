<template>
  <div class="array-editor">
    <div class="array-editor__header">
      <span>{{ title }}</span>
      <button class="secondary-button" type="button" @click="addItem">新增奖励</button>
    </div>

    <div v-if="draft.length === 0" class="empty-tip">当前没有奖励配置。</div>

    <div v-else class="array-editor__list">
      <div v-for="(item, index) in draft" :key="`${item.type}-${item.itemId}-${index}`" class="array-editor__row array-editor__row--three">
        <label class="field"><span>奖励类型</span><select v-model="item.type" @change="emitChange"><option value="gold">金币</option><option value="exp">经验</option><option value="spiritStone">灵石</option><option value="honor">荣誉</option><option value="guildContribution">公会贡献</option><option value="item">道具</option></select></label>
        <label class="field"><span>数量</span><input v-model.number="item.count" type="number" min="1" @input="emitChange" /></label>
        <label class="field"><span>道具编号</span><select v-if="item.type === 'item' && itemOptions.length > 0" v-model="item.itemId" @change="emitChange"><option value="">请选择</option><option v-for="option in itemOptions" :key="option.value" :value="option.value">{{ option.label }}</option></select><input v-else v-model.trim="item.itemId" type="text" placeholder="仅 type=item 时填写" @input="emitChange" /></label>
        
        <label class="field field--full"><span>说明</span><div class="inline-action"><input v-model.trim="item.description" type="text" @input="emitChange" /><button class="danger-button danger-button--small" type="button" @click="removeItem(index)">删除</button></div></label>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import type { RewardGrantEntry } from '@/types/admin'

type OptionItem = { value: string; label: string }
const props = withDefaults(defineProps<{ modelValue: RewardGrantEntry[]; title?: string; itemOptions?: OptionItem[] }>(), { title: '奖励配置', itemOptions: () => [] })
const emit = defineEmits<{ 'update:modelValue': [RewardGrantEntry[]] }>()
const draft = ref<RewardGrantEntry[]>([])

watch(() => props.modelValue, (value) => { draft.value = (value || []).map((item) => ({ ...item })) }, { immediate: true, deep: true })

function emitChange() {
  emit('update:modelValue', draft.value.map((item) => ({ type: item.type.trim(), count: Math.max(1, Number(item.count) || 1), itemId: item.itemId?.trim() || null, description: item.description?.trim() || null })).filter((item) => item.type))
}

function addItem() {
  draft.value.push({ type: 'gold', count: 1, itemId: null, description: null })
  emitChange()
}

function removeItem(index: number) {
  draft.value.splice(index, 1)
  emitChange()
}
</script>
