<template>
  <div class="encounter-editor">
    <div class="editor-grid">
      <label class="field">
        <span>好感度组队阈值</span>
        <input v-model.number="local.favorabilityTeamThreshold" type="number" @input="emitChange" />
      </label>
      <label class="field">
        <span>默认组队概率 (%)</span>
        <input v-model.number="local.defaultTeamRate" type="number" min="0" max="100" @input="emitChange" />
      </label>
      <label class="field">
        <span>对手 PvP 额外攻击加成 (%)</span>
        <input v-model.number="local.rivalPvpBonusAttack" type="number" min="0" @input="emitChange" />
      </label>
    </div>

    <h4 style="margin: 12px 0 8px; font-size: 14px;">PvP 掠夺配置</h4>
    <div class="editor-grid">
      <label class="field checkbox-field">
        <input type="checkbox" v-model="local.pvpEnabled" @change="emitChange" />
        <span>启用 PvP 掠夺</span>
      </label>
      <template v-if="local.pvpEnabled">
        <label class="field">
          <span>金币掠夺比例 (%)</span>
          <input v-model.number="local.currencyPlunderRate" type="number" min="0" max="100" @input="emitChange" />
        </label>
        <label class="field">
          <span>装备掠夺概率 (%)</span>
          <input v-model.number="local.equipmentPlunderRate" type="number" min="0" max="100" @input="emitChange" />
        </label>
        <label class="field">
          <span>道具掠夺概率 (%)</span>
          <input v-model.number="local.itemPlunderRate" type="number" min="0" max="100" @input="emitChange" />
        </label>
        <label class="field">
          <span>装备掠夺数量</span>
          <input v-model.trim="local.equipmentPlunderCount" type="text" placeholder="1-2" @input="emitChange" />
        </label>
        <label class="field">
          <span>道具掠夺数量</span>
          <input v-model.trim="local.itemPlunderCount" type="text" placeholder="1-2" @input="emitChange" />
        </label>
      </template>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'

interface EncounterConfig {
  favorabilityTeamThreshold: number
  defaultTeamRate: number
  rivalPvpBonusAttack: number
  pvpEnabled: boolean
  currencyPlunderRate: number
  equipmentPlunderRate: number
  itemPlunderRate: number
  equipmentPlunderCount: string
  itemPlunderCount: string
}

const props = defineProps<{
  modelValue: string | null | undefined
}>()

const emit = defineEmits<{ 'update:modelValue': [string | null] }>()

const local = ref<EncounterConfig>({
  favorabilityTeamThreshold: 100,
  defaultTeamRate: 20,
  rivalPvpBonusAttack: 10,
  pvpEnabled: false,
  currencyPlunderRate: 30,
  equipmentPlunderRate: 40,
  itemPlunderRate: 40,
  equipmentPlunderCount: '1-2',
  itemPlunderCount: '1-2'
})

watch(() => props.modelValue, (value) => {
  if (!value) {
    local.value = { favorabilityTeamThreshold: 100, defaultTeamRate: 20, rivalPvpBonusAttack: 10, pvpEnabled: false, currencyPlunderRate: 30, equipmentPlunderRate: 40, itemPlunderRate: 40, equipmentPlunderCount: '1-2', itemPlunderCount: '1-2' }
    return
  }
  try {
    const parsed = JSON.parse(value)
    local.value = {
      favorabilityTeamThreshold: parsed.favorabilityTeamThreshold ?? 100,
      defaultTeamRate: parsed.defaultTeamRate ?? 20,
      rivalPvpBonusAttack: parsed.rivalPvpBonusAttack ?? 10,
      pvpEnabled: !!parsed.pvpPlunderConfig,
      currencyPlunderRate: parsed.pvpPlunderConfig?.currencyPlunderRate ?? 30,
      equipmentPlunderRate: parsed.pvpPlunderConfig?.equipmentPlunderRate ?? 40,
      itemPlunderRate: parsed.pvpPlunderConfig?.itemPlunderRate ?? 40,
      equipmentPlunderCount: parsed.pvpPlunderConfig?.equipmentPlunderCount ?? '1-2',
      itemPlunderCount: parsed.pvpPlunderConfig?.itemPlunderCount ?? '1-2'
    }
  } catch {
    local.value = { favorabilityTeamThreshold: 100, defaultTeamRate: 20, rivalPvpBonusAttack: 10, pvpEnabled: false, currencyPlunderRate: 30, equipmentPlunderRate: 40, itemPlunderRate: 40, equipmentPlunderCount: '1-2', itemPlunderCount: '1-2' }
  }
}, { immediate: true })

function emitChange() {
  const result: any = {
    favorabilityTeamThreshold: local.value.favorabilityTeamThreshold,
    defaultTeamRate: local.value.defaultTeamRate,
    rivalPvpBonusAttack: local.value.rivalPvpBonusAttack
  }
  if (local.value.pvpEnabled) {
    result.pvpPlunderConfig = {
      currencyTypes: ['Gold', 'SpiritStone'],
      currencyPlunderRate: local.value.currencyPlunderRate,
      equipmentPlunderCount: local.value.equipmentPlunderCount || '1-2',
      itemPlunderCount: local.value.itemPlunderCount || '1-2',
      equipmentPlunderRate: local.value.equipmentPlunderRate,
      itemPlunderRate: local.value.itemPlunderRate
    }
  }
  emit('update:modelValue', JSON.stringify(result))
}
</script>

<style scoped>
.encounter-editor {
  display: flex;
  flex-direction: column;
  gap: 12px;
}
.editor-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
}
</style>
