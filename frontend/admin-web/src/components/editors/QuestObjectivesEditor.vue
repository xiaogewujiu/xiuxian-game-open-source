<template>
  <div class="array-editor">
    <div class="array-editor__header">
      <span>任务目标</span>
      <button class="secondary-button" type="button" @click="addItem">新增目标</button>
    </div>

    <div v-if="draft.length === 0" class="empty-tip">当前没有任务目标。</div>

    <div v-else class="array-editor__list">
      <div v-for="(item, index) in draft" :key="`${item.targetId}-${index}`" class="array-editor__row array-editor__row--three">
        <label class="field">
          <span>目标类型</span>
          <select v-model.number="item.objectiveType" @change="emitChange">
            <option v-for="option in QUEST_OBJECTIVE_TYPE_OPTIONS" :key="option.value" :value="option.value">{{ option.label }}</option>
          </select>
        </label>
        <label v-if="requiresTargetId(item.objectiveType) && getTargetOptions(item.objectiveType).length > 0" class="field">
          <span>{{ getTargetIdLabel(item.objectiveType) }}</span>
          <select v-model="item.targetId" @change="emitChange">
            <option value="">请选择</option>
            <option v-for="option in getTargetOptions(item.objectiveType)" :key="option.value" :value="option.value">{{ option.label }}</option>
          </select>
        </label>
        <label v-else-if="requiresTargetId(item.objectiveType)" class="field"><span>{{ getTargetIdLabel(item.objectiveType) }}</span><input v-model.trim="item.targetId" type="text" @input="emitChange" /></label>
        <label v-else class="field"><span>目标对象</span><input value="无需配置" type="text" disabled /></label>
        <label class="field"><span>目标数量</span><input v-model.number="item.targetCount" type="number" min="1" @input="emitChange" /></label>
        <label class="field"><span>{{ getTargetValueLabel(item.objectiveType) }}</span><input v-model.number="item.targetValue" type="number" min="0" :disabled="!requiresTargetValue(item.objectiveType)" @input="emitChange" /></label>
        <label class="field">
          <span>战斗活动类型</span>
          <select v-model.number="item.activityType" :disabled="item.objectiveType !== 6" @change="emitChange">
            <option v-for="option in BATTLE_ACTIVITY_TYPE_OPTIONS" :key="option.value" :value="option.value">{{ option.label }}</option>
          </select>
        </label>
        <label class="field field--full"><span>描述</span><div class="inline-action"><input v-model.trim="item.description" type="text" @input="emitChange" /><button class="danger-button danger-button--small" type="button" @click="removeItem(index)">删除</button></div></label>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { BATTLE_ACTIVITY_TYPE_OPTIONS, QUEST_OBJECTIVE_TYPE_OPTIONS } from '@/constants/game-options'
import type { QuestObjectiveEntry } from '@/types/admin'

type OptionItem = {
  value: string
  label: string
}

const props = withDefaults(defineProps<{
  modelValue: QuestObjectiveEntry[]
  monsterOptions?: OptionItem[]
  itemOptions?: OptionItem[]
  mapOptions?: OptionItem[]
  dungeonOptions?: OptionItem[]
  cropOptions?: OptionItem[]
  alchemyRecipeOptions?: OptionItem[]
  forgeRecipeOptions?: OptionItem[]
}>(), {
  monsterOptions: () => [],
  itemOptions: () => [],
  mapOptions: () => [],
  dungeonOptions: () => [],
  cropOptions: () => [],
  alchemyRecipeOptions: () => [],
  forgeRecipeOptions: () => []
})
const emit = defineEmits<{ 'update:modelValue': [QuestObjectiveEntry[]] }>()

const draft = ref<QuestObjectiveEntry[]>([])

watch(() => props.modelValue, (value) => {
  draft.value = (value || []).map((item) => ({ ...item }))
}, { immediate: true, deep: true })

function requiresTargetId(objectiveType: number) {
  return ![4, 5, 6, 9, 10, 13, 14].includes(Number(objectiveType))
}

function requiresTargetValue(objectiveType: number) {
  return [4, 5, 14].includes(Number(objectiveType))
}

function getTargetIdLabel(objectiveType: number) {
  switch (Number(objectiveType)) {
    case 0: return '怪物'
    case 1: return '道具'
    case 2: return 'NPC编号'
    case 3: return '副本'
    case 7:
    case 8: return '地图'
    case 11: return '丹方'
    case 12: return '图纸'
    case 15:
    case 16: return '作物'
    default: return '目标编号'
  }
}

function getTargetValueLabel(objectiveType: number) {
  switch (Number(objectiveType)) {
    case 4: return '目标等级'
    case 5:
    case 14: return '强化等级'
    default: return '条件值'
  }
}

function getTargetOptions(objectiveType: number) {
  switch (Number(objectiveType)) {
    case 0: return props.monsterOptions
    case 1: return props.itemOptions
    case 3: return props.dungeonOptions
    case 7:
    case 8: return props.mapOptions
    case 11: return props.alchemyRecipeOptions
    case 12: return props.forgeRecipeOptions
    case 15:
    case 16: return props.cropOptions
    default: return []
  }
}

function emitChange() {
  emit('update:modelValue', draft.value
    .map((item) => ({
      objectiveType: Number(item.objectiveType) || 0,
      targetId: item.targetId.trim(),
      targetCount: Math.max(1, Number(item.targetCount) || 1),
      targetValue: Math.max(0, Number(item.targetValue) || 0),
      description: item.description.trim(),
      activityType: Number(item.objectiveType) === 6 ? (Number(item.activityType) || 0) : 0
    })))
}

function addItem() {
  draft.value.push({
    objectiveType: 0,
    targetId: '',
    targetCount: 1,
    targetValue: 0,
    description: '',
    activityType: 0
  })
}

function removeItem(index: number) {
  draft.value.splice(index, 1)
  emitChange()
}
</script>
