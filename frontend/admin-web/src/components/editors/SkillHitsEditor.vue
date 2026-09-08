<template>
  <div class="array-editor">
    <div class="array-editor__header">
      <span>技能段列表</span>
      <button class="secondary-button" type="button" @click="addItem">新增技能段</button>
    </div>

    <div v-if="draft.length === 0" class="empty-tip">当前没有技能段配置。</div>

    <div v-else class="array-editor__list">
      <div v-for="(item, index) in draft" :key="index" class="array-editor__row array-editor__row--three">
        <label class="field"><span>倍率</span><input v-model.number="item.damageMultiplier" type="number" step="0.01" @input="emitChange" /></label>
        <label class="field"><span>基础伤害</span><input v-model.number="item.baseDamage" type="number" min="0" @input="emitChange" /></label>
        <label class="field">
          <span>伤害类型</span>
          <select v-model.number="item.hitDamageType" @change="emitChange">
            <option :value="-1">跟随主技能类型</option>
            <option v-for="option in DAMAGE_TYPE_OPTIONS" :key="option.value" :value="option.value">{{ option.label }}</option>
          </select>
        </label>
        <label class="field field--full"><span>描述</span><div class="inline-action"><input v-model.trim="item.description" type="text" @input="emitChange" /><button class="danger-button danger-button--small" type="button" @click="removeItem(index)">删除</button></div></label>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { DAMAGE_TYPE_OPTIONS } from '@/constants/game-options'
import type { SkillHit } from '@/types/admin'

const props = defineProps<{ modelValue: SkillHit[] }>()
const emit = defineEmits<{ 'update:modelValue': [SkillHit[]] }>()

const draft = ref<Array<SkillHit & { hitDamageType: number }>>([])
watch(() => props.modelValue, (value) => {
  draft.value = (value || []).map((item) => ({ ...item, hitDamageType: item.hitDamageType ?? -1 }))
}, { immediate: true, deep: true })

function emitChange() {
  emit('update:modelValue', draft.value.map((item) => ({
    damageMultiplier: Number(item.damageMultiplier) || 0,
    baseDamage: Math.max(0, Number(item.baseDamage) || 0),
    hitDamageType: item.hitDamageType < 0 ? null : Number(item.hitDamageType),
    description: item.description.trim()
  })))
}
function addItem() { draft.value.push({ damageMultiplier: 1, baseDamage: 0, hitDamageType: -1, description: '' }); emitChange() }
function removeItem(index: number) { draft.value.splice(index, 1); emitChange() }
</script>
