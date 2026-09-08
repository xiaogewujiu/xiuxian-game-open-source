<template>
  <div class="medicine-editor">
    <label class="field checkbox-field">
      <input type="checkbox" v-model="local.enabled" @change="emitChange" />
      <span>启用自动用药</span>
    </label>

    <template v-if="local.enabled">
      <div class="editor-grid">
        <label class="field">
          <span>HP 阈值 (%)</span>
          <input v-model.number="local.hpThresholdPercent" type="number" min="1" max="100" @input="emitChange" />
        </label>
        <label class="field">
          <span>MP 阈值 (%)</span>
          <input v-model.number="local.mpThresholdPercent" type="number" min="1" max="100" @input="emitChange" />
        </label>
      </div>

      <div class="array-editor">
        <div class="array-editor__header">
          <span>药品列表</span>
          <button class="secondary-button" type="button" @click="addPill">新增药品</button>
        </div>
        <div v-if="local.pills.length === 0" class="empty-tip">未配置药品。</div>
        <div v-else class="array-editor__list">
          <div v-for="(pill, index) in local.pills" :key="index" class="array-editor__row">
            <label class="field">
              <span>道具</span>
              <select v-model="pill.itemId" @change="emitChange">
                <option value="">请选择药品</option>
                <option v-for="opt in itemOptions" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
              </select>
            </label>
            <label class="field">
              <span>恢复HP</span>
              <input v-model.number="pill.healAmount" type="number" min="0" @input="emitChange" />
            </label>
            <label class="field">
              <span>恢复MP</span>
              <input v-model.number="pill.healMpAmount" type="number" min="0" @input="emitChange" />
            </label>
            <label class="field">
              <span>操作</span>
              <button class="danger-button danger-button--small" type="button" @click="removePill(index)">删除</button>
            </label>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'

interface PillEntry {
  itemId: string
  healAmount: number
  healMpAmount: number
}

interface MedicineConfig {
  enabled: boolean
  hpThresholdPercent: number
  mpThresholdPercent: number
  pills: PillEntry[]
}

const props = defineProps<{
  modelValue: string | null | undefined
  itemOptions: { value: string; label: string }[]
}>()

const emit = defineEmits<{ 'update:modelValue': [string | null] }>()

const local = ref<MedicineConfig>({
  enabled: false,
  hpThresholdPercent: 50,
  mpThresholdPercent: 30,
  pills: []
})

watch(() => props.modelValue, (value) => {
  if (!value) {
    local.value = { enabled: false, hpThresholdPercent: 50, mpThresholdPercent: 30, pills: [] }
    return
  }
  try {
    const parsed = JSON.parse(value)
    // 兼容旧格式 pillTypes
    if (parsed.pillTypes && !parsed.pills) {
      local.value = {
        enabled: parsed.enabled ?? true,
        hpThresholdPercent: parsed.hpThresholdPercent ?? parsed.hpThreshold ?? 50,
        mpThresholdPercent: parsed.mpThresholdPercent ?? parsed.mpThreshold ?? 30,
        pills: []
      }
    } else {
      local.value = {
        enabled: parsed.enabled ?? true,
        hpThresholdPercent: parsed.hpThresholdPercent ?? parsed.hpThreshold ?? 50,
        mpThresholdPercent: parsed.mpThresholdPercent ?? parsed.mpThreshold ?? 30,
        pills: (parsed.pills || []).map((p: any) => ({
          itemId: p.itemId || '',
          healAmount: p.healAmount || 0,
          healMpAmount: p.healMpAmount || 0
        }))
      }
    }
  } catch {
    local.value = { enabled: false, hpThresholdPercent: 50, mpThresholdPercent: 30, pills: [] }
  }
}, { immediate: true })

function emitChange() {
  if (!local.value.enabled) {
    emit('update:modelValue', null)
    return
  }
  emit('update:modelValue', JSON.stringify({
    enabled: true,
    hpThresholdPercent: local.value.hpThresholdPercent,
    mpThresholdPercent: local.value.mpThresholdPercent,
    pills: local.value.pills.filter(p => p.itemId)
  }))
}

function addPill() {
  local.value.pills.push({ itemId: '', healAmount: 0, healMpAmount: 0 })
  emitChange()
}

function removePill(index: number) {
  local.value.pills.splice(index, 1)
  emitChange()
}
</script>

<style scoped>
.medicine-editor {
  display: flex;
  flex-direction: column;
  gap: 12px;
}
.editor-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
}
.array-editor__row {
  display: grid;
  grid-template-columns: 1fr 100px 100px 80px;
  gap: 8px;
  align-items: end;
}
</style>
