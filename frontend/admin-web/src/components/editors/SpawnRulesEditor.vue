<template>
  <div class="array-editor">
    <div class="array-editor__header">
      <span>刷怪规则</span>
      <button class="secondary-button" type="button" @click="addRule">新增规则</button>
    </div>

    <div v-if="draft.length === 0" class="empty-tip">当前没有刷怪规则。</div>

    <div v-else class="array-editor__list">
      <div v-for="(rule, index) in draft" :key="`${rule.monsterTemplateId}-${index}`" class="array-editor__row array-editor__row--three">
        <label class="field">
          <span>怪物模板</span>
          <select v-if="options.length > 0" v-model="rule.monsterTemplateId" @change="emitChange">
            <option value="">请选择</option>
            <option v-for="option in options" :key="option.value" :value="option.value">{{ option.label }}</option>
          </select>
          <input v-else v-model.trim="rule.monsterTemplateId" type="text" @input="emitChange" />
        </label>
        <label class="field">
          <span>权重</span>
          <input v-model.number="rule.weight" type="number" min="1" @input="emitChange" />
        </label>
        <label class="field">
          <span>最大数量</span>
          <div class="inline-action">
            <input v-model.number="rule.maxCount" type="number" min="1" @input="emitChange" />
            <button class="danger-button danger-button--small" type="button" @click="removeRule(index)">删除</button>
          </div>
        </label>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import type { MapMonsterSpawnRule } from '@/types/admin'

type OptionItem = {
  value: string
  label: string
}

const props = withDefaults(defineProps<{ modelValue: MapMonsterSpawnRule[]; options?: OptionItem[] }>(), {
  options: () => []
})
const emit = defineEmits<{ 'update:modelValue': [MapMonsterSpawnRule[]] }>()

const draft = ref<MapMonsterSpawnRule[]>([])

watch(() => props.modelValue, (value) => {
  draft.value = (value || []).map((rule) => ({ ...rule }))
}, { immediate: true, deep: true })

function emitChange() {
  emit('update:modelValue', draft.value.map((rule) => ({
    monsterTemplateId: rule.monsterTemplateId,
    weight: Number(rule.weight) || 1,
    maxCount: Number(rule.maxCount) || 1
  })))
}

function addRule() {
  draft.value.push({ monsterTemplateId: '', weight: 100, maxCount: 1 })
  emitChange()
}

function removeRule(index: number) {
  draft.value.splice(index, 1)
  emitChange()
}
</script>
