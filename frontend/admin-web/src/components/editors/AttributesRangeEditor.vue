<template>
  <div class="array-editor">
    <div class="array-editor__header array-editor__header--stack">
      <div class="array-editor__heading">
        <span>属性</span>
        <span>区间</span>
      </div>
      <p class="array-editor__note">{{ noteText }}</p>
    </div>

    <div class="attribute-grid">
      <div v-for="field in fields" :key="field.key" class="attribute-grid__row">
        <div class="attribute-grid__meta">
          <strong>{{ field.label }}</strong>
          <span>{{ field.hint }}</span>
        </div>
        <input v-model.number="draft[field.minKey]" type="number" :placeholder="minPlaceholder(field)" @input="emitChange" />
        <input v-model.number="draft[field.maxKey]" type="number" :placeholder="maxPlaceholder(field)" @input="emitChange" />
      </div>
    </div>

    <label class="field">
      <span>{{ elementLabel }}</span>
      <select v-model.number="draft.element" @change="emitChange">
        <option :value="0">{{ elementOptions[0] }}</option>
        <option :value="1">{{ elementOptions[1] }}</option>
        <option :value="2">{{ elementOptions[2] }}</option>
        <option :value="3">{{ elementOptions[3] }}</option>
        <option :value="4">{{ elementOptions[4] }}</option>
        <option :value="5">{{ elementOptions[5] }}</option>
        <option :value="6">{{ elementOptions[6] }}</option>
        <option :value="7">{{ elementOptions[7] }}</option>
        <option :value="8">{{ elementOptions[8] }}</option>
      </select>
    </label>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import type { BaseAttributesRange } from '@/types/admin'

const props = defineProps<{ modelValue: BaseAttributesRange }>()
const emit = defineEmits<{ 'update:modelValue': [BaseAttributesRange] }>()

const titleText = '\u5c5e\u6027\u533a\u95f4'
const noteText = '\u57fa\u7840\u5c5e\u6027\u76f4\u63a5\u586b\u6574\u6570\u3002\u9ad8\u7ea7\u5c5e\u6027\u5728\u540e\u53f0\u6309 0-100 \u7684\u767e\u5206\u6bd4\u53e3\u5f84\u586b\u5199\uff1a\u547d\u4e2d\u3001\u95ea\u907f\u3001\u66b4\u51fb\u3001\u8fde\u51fb\u3001\u53cd\u51fb\u3001\u7834\u7532\u3001\u9644\u52a0\u4f24\u5bb3\u586b 10 \u8868\u793a 10%\uff1b\u66b4\u51fb\u4f24\u5bb3\u586b 150 \u8868\u793a 150%\uff0c\u8fd0\u884c\u65f6\u4f1a\u8f6c\u4e3a 1.5 \u500d\u3002'
const elementLabel = '\u9ed8\u8ba4\u5143\u7d20'
const elementOptions = ['\u65e0', '\u91d1', '\u6728', '\u6c34', '\u706b', '\u571f', '\u98ce', '\u51b0', '\u96f7']
const minPrefix = '\u6700\u5c0f'
const maxPrefix = '\u6700\u5927'

const fields = [
  { key: 'type1', label: '\u751f\u547d', hint: '\u56fa\u5b9a\u503c\uff0c\u76f4\u63a5\u586b\u6574\u6570', minKey: 'minType1', maxKey: 'maxType1' },
  { key: 'type2', label: '\u6cd5\u529b', hint: '\u56fa\u5b9a\u503c\uff0c\u76f4\u63a5\u586b\u6574\u6570', minKey: 'minType2', maxKey: 'maxType2' },
  { key: 'type3', label: '\u7269\u653b', hint: '\u56fa\u5b9a\u503c\uff0c\u76f4\u63a5\u586b\u6574\u6570', minKey: 'minType3', maxKey: 'maxType3' },
  { key: 'type4', label: '\u6cd5\u653b', hint: '\u56fa\u5b9a\u503c\uff0c\u76f4\u63a5\u586b\u6574\u6570', minKey: 'minType4', maxKey: 'maxType4' },
  { key: 'type5', label: '\u7269\u9632', hint: '\u56fa\u5b9a\u503c\uff0c\u76f4\u63a5\u586b\u6574\u6570', minKey: 'minType5', maxKey: 'maxType5' },
  { key: 'type6', label: '\u6cd5\u9632', hint: '\u56fa\u5b9a\u503c\uff0c\u76f4\u63a5\u586b\u6574\u6570', minKey: 'minType6', maxKey: 'maxType6' },
  { key: 'type7', label: '\u901f\u5ea6', hint: '\u56fa\u5b9a\u503c\uff0c\u76f4\u63a5\u586b\u6574\u6570', minKey: 'minType7', maxKey: 'maxType7' },
  { key: 'type8', label: '\u547d\u4e2d\u7387', hint: '\u586b 90 \u8868\u793a 90%\uff0c\u8fd0\u884c\u65f6\u4e3a 0.9', minKey: 'minType8', maxKey: 'maxType8' },
  { key: 'type9', label: '\u95ea\u907f\u7387', hint: '\u586b 5 \u8868\u793a 5%\uff0c\u8fd0\u884c\u65f6\u4e3a 0.05', minKey: 'minType9', maxKey: 'maxType9' },
  { key: 'type10', label: '\u66b4\u51fb\u7387', hint: '\u586b 12 \u8868\u793a 12%\uff0c\u8fd0\u884c\u65f6\u4e3a 0.12', minKey: 'minType10', maxKey: 'maxType10' },
  { key: 'type11', label: '\u66b4\u51fb\u4f24\u5bb3', hint: '\u586b 150 \u8868\u793a 150%\uff0c\u8fd0\u884c\u65f6\u4e3a 1.5 \u500d', minKey: 'minType11', maxKey: 'maxType11' },
  { key: 'type12', label: '\u8fde\u51fb\u7387', hint: '\u586b 8 \u8868\u793a 8%\uff0c\u8fd0\u884c\u65f6\u4e3a 0.08', minKey: 'minType12', maxKey: 'maxType12' },
  { key: 'type13', label: '\u53cd\u51fb\u7387', hint: '\u586b 8 \u8868\u793a 8%\uff0c\u8fd0\u884c\u65f6\u4e3a 0.08', minKey: 'minType13', maxKey: 'maxType13' },
  { key: 'type14', label: '\u7834\u7532\u7387', hint: '\u586b 10 \u8868\u793a 10%\uff0c\u8fd0\u884c\u65f6\u4e3a 0.1', minKey: 'minType14', maxKey: 'maxType14' },
  { key: 'type15', label: '\u9644\u52a0\u4f24\u5bb3', hint: '\u586b 10 \u8868\u793a +10%\uff0c\u8fd0\u884c\u65f6\u4e3a 0.1', minKey: 'minType15', maxKey: 'maxType15' }
] as const

const draft = ref<BaseAttributesRange>({})

watch(() => props.modelValue, (value) => {
  draft.value = { ...(value || {}) }
}, { immediate: true, deep: true })

function minPlaceholder(field: { label: string }) {
  return `${minPrefix}${field.label}`
}

function maxPlaceholder(field: { label: string }) {
  return `${maxPrefix}${field.label}`
}

function emitChange() {
  emit('update:modelValue', { ...draft.value })
}
</script>
