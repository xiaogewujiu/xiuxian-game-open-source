<template>
  <div class="array-editor">
    <div class="array-editor__header">
      <span>Buff效果列表</span>
      <button class="secondary-button" type="button" @click="addItem">新增效果</button>
    </div>

    <div v-if="draft.length === 0" class="empty-tip">当前没有 Buff 效果。</div>

    <div v-else class="array-editor__list">
      <div v-for="(item, index) in draft" :key="index" class="array-editor__row array-editor__row--three">
        <label class="field">
          <span>效果类型</span>
          <select v-model.number="item.effectType" @change="emitChange">
            <option v-for="option in BUFF_EFFECT_TYPE_OPTIONS" :key="option.value" :value="option.value">{{ option.label }}</option>
          </select>
        </label>
        <label class="field"><span>数值</span><input v-model.number="item.value" type="number" step="0.01" @input="emitChange" /></label>
        <label class="field checkbox-field"><input v-model="item.isPercentage" type="checkbox" @change="emitChange" /><span>百分比</span></label>
        <label class="field">
          <span>目标阵营</span>
          <select v-model.number="item.targetCamp" @change="emitChange">
            <option v-for="option in BUFF_TARGET_CAMP_OPTIONS" :key="option.value" :value="option.value">{{ option.label }}</option>
          </select>
        </label>
        <label class="field"><span>目标数量</span><input v-model.number="item.targetCount" type="number" min="0" @input="emitChange" /></label>
        <label class="field checkbox-field"><input v-model="item.isRandomTarget" type="checkbox" @change="emitChange" /><span>随机目标</span></label>
        <label class="field"><span>触发概率</span><input v-model.number="item.triggerChance" type="number" min="0" step="0.01" @input="emitChange" /></label>
        <label class="field">
          <span>目标选择模式</span>
          <select v-model.number="item.targetSelectionMode" @change="emitChange">
            <option v-for="option in BUFF_TARGET_SELECTION_MODE_OPTIONS" :key="option.value" :value="option.value">{{ option.label }}</option>
          </select>
        </label>
        <label class="field checkbox-field"><input v-model="item.targetSelfOnly" type="checkbox" @change="emitChange" /><span>仅施法者自己</span></label>
        <label class="field"><span>效果时长</span><div class="inline-action"><input v-model.number="item.effectDuration" type="number" @input="emitChange" /><button class="danger-button danger-button--small" type="button" @click="removeItem(index)">删除</button></div></label>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { BUFF_EFFECT_TYPE_OPTIONS, BUFF_TARGET_CAMP_OPTIONS, BUFF_TARGET_SELECTION_MODE_OPTIONS } from '@/constants/game-options'
import type { BuffEffect } from '@/types/admin'

const props = defineProps<{ modelValue: BuffEffect[] }>()
const emit = defineEmits<{ 'update:modelValue': [BuffEffect[]] }>()

const draft = ref<BuffEffect[]>([])
watch(() => props.modelValue, (value) => { draft.value = (value || []).map((item) => ({ ...item })) }, { immediate: true, deep: true })

function emitChange() { emit('update:modelValue', draft.value.map((item) => ({ ...item }))) }
function addItem() {
  draft.value.push({ effectType: 1, value: 0, isPercentage: false, targetCamp: 1, targetCount: 0, isRandomTarget: true, triggerChance: 1, targetSelectionMode: 1, targetSelfOnly: false, effectDuration: -1 })
  emitChange()
}
function removeItem(index: number) { draft.value.splice(index, 1); emitChange() }
</script>
