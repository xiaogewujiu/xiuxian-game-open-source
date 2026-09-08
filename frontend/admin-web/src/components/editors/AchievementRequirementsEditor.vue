<template>
  <div class="array-editor">
    <div class="array-editor__header">
      <span>成就条件</span>
      <button class="secondary-button" type="button" @click="addItem">新增条件</button>
    </div>

    <div v-if="draft.length === 0" class="empty-tip">当前没有成就条件。</div>

    <div v-else class="array-editor__list">
      <div v-for="(item, index) in draft" :key="`${item.requirementType}-${item.targetId}-${index}`" class="array-editor__row array-editor__row--three">
        <label class="field">
          <span>条件类型</span>
          <select v-model.number="item.requirementType" @change="emitChange">
            <option v-for="option in ACHIEVEMENT_REQUIREMENT_TYPE_OPTIONS" :key="option.value" :value="option.value">{{ option.label }}</option>
          </select>
        </label>
        <label v-if="requiresTargetId(item.requirementType) && getTargetOptions(item.requirementType).length > 0" class="field">
          <span>{{ getTargetLabel(item.requirementType) }}</span>
          <select v-model="item.targetId" @change="emitChange">
            <option value="">请选择</option>
            <option v-for="option in getTargetOptions(item.requirementType)" :key="option.value" :value="option.value">{{ option.label }}</option>
          </select>
        </label>
        <label v-else-if="requiresTargetId(item.requirementType)" class="field"><span>{{ getTargetLabel(item.requirementType) }}</span><input v-model.trim="item.targetId" type="text" @input="emitChange" /></label>
        <label v-else class="field"><span>目标对象</span><input value="无需配置" type="text" disabled /></label>
        <label class="field"><span>目标值</span><input v-model.number="item.targetValue" type="number" min="0" @input="emitChange" /></label>
        <label class="field field--full"><span>描述</span><div class="inline-action"><input v-model.trim="item.description" type="text" @input="emitChange" /><button class="danger-button danger-button--small" type="button" @click="removeItem(index)">删除</button></div></label>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { ACHIEVEMENT_REQUIREMENT_TYPE_OPTIONS } from '@/constants/game-options'
import type { AchievementRequirementEntry } from '@/types/admin'

type OptionItem = {
  value: string
  label: string
}

const props = withDefaults(defineProps<{
  modelValue: AchievementRequirementEntry[]
  monsterOptions?: OptionItem[]
  itemOptions?: OptionItem[]
  dungeonOptions?: OptionItem[]
  cropOptions?: OptionItem[]
  skillOptions?: OptionItem[]
}>(), {
  monsterOptions: () => [],
  itemOptions: () => [],
  dungeonOptions: () => [],
  cropOptions: () => [],
  skillOptions: () => []
})

const emit = defineEmits<{ 'update:modelValue': [AchievementRequirementEntry[]] }>()
const draft = ref<AchievementRequirementEntry[]>([])

watch(() => props.modelValue, (value) => {
  draft.value = (value || []).map((item) => ({ ...item }))
}, { immediate: true, deep: true })

function requiresTargetId(requirementType: number) {
  return [5, 11, 25, 26, 27, 23, 24, 32].includes(Number(requirementType))
}

function getTargetLabel(requirementType: number) {
  switch (Number(requirementType)) {
    case 5: return '道具'
    case 11:
    case 25: return '副本'
    case 23:
    case 24: return '作物'
    case 26: return '怪物'
    case 27:
    case 32: return '技能'
    default: return '目标对象'
  }
}

function getTargetOptions(requirementType: number) {
  switch (Number(requirementType)) {
    case 5: return props.itemOptions
    case 11:
    case 25: return props.dungeonOptions
    case 23:
    case 24: return props.cropOptions
    case 26: return props.monsterOptions
    case 27:
    case 32: return props.skillOptions
    default: return []
  }
}

function emitChange() {
  emit('update:modelValue', draft.value.map((item) => ({
    requirementType: Number(item.requirementType) || 0,
    targetId: String(item.targetId || '').trim(),
    targetValue: Math.max(0, Number(item.targetValue) || 0),
    description: String(item.description || '').trim()
  })))
}

function addItem() {
  draft.value.push({
    requirementType: 14,
    targetId: '',
    targetValue: 1,
    description: ''
  })
}

function removeItem(index: number) {
  draft.value.splice(index, 1)
  emitChange()
}
</script>
