<template>
  <div class="array-editor">
    <div class="array-editor__header">
      <span>开放时间配置</span>
      <button class="secondary-button" type="button" @click="addSchedule">新增时段</button>
    </div>

    <div v-if="draft.length === 0" class="empty-tip">当前没有开放时间配置（表示全天开放）。</div>

    <div v-else class="array-editor__list">
      <div v-for="(row, index) in draft" :key="index" class="schedule-row">
        <label class="field">
          <span>类型</span>
          <select v-model="row.type" @change="emitChange">
            <option value="daily">每天</option>
            <option value="weekly">每周</option>
          </select>
        </label>
        <label v-if="row.type === 'weekly'" class="field">
          <span>星期</span>
          <div class="weekday-checkboxes">
            <label v-for="d in WEEKDAY_OPTIONS" :key="d.value" class="weekday-cb">
              <input type="checkbox" :checked="(row.days || []).includes(d.value)" @change="toggleDay(index, d.value, $event)" /><span>{{ d.label }}</span>
            </label>
          </div>
        </label>
        <label class="field">
          <span>开始时间</span>
          <input v-model="row.startTime" type="time" @input="emitChange" />
        </label>
        <label class="field">
          <span>结束时间</span>
          <input v-model="row.endTime" type="time" @input="emitChange" />
        </label>
        <div class="field">
          <span>操作</span>
          <button class="danger-button danger-button--small" type="button" @click="removeSchedule(index)">删除</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'

interface ScheduleEntry {
  type: 'daily' | 'weekly'
  days?: number[]
  startTime: string
  endTime: string
}

const WEEKDAY_OPTIONS = [
  { value: 0, label: '日' },
  { value: 1, label: '一' },
  { value: 2, label: '二' },
  { value: 3, label: '三' },
  { value: 4, label: '四' },
  { value: 5, label: '五' },
  { value: 6, label: '六' }
]

const props = defineProps<{
  modelValue: string | null | undefined
}>()

const emit = defineEmits<{ 'update:modelValue': [string | null] }>()
const draft = ref<ScheduleEntry[]>([])

watch(() => props.modelValue, (value) => {
  if (!value) { draft.value = []; return }
  try {
    const parsed = JSON.parse(value)
    draft.value = (parsed.schedules || []).map((s: any) => ({
      type: s.type || 'daily',
      days: s.days || s.dayOfWeek != null ? (s.days || [s.dayOfWeek]) : undefined,
      startTime: s.startTime || '00:00',
      endTime: s.endTime || '23:59'
    }))
  } catch { draft.value = [] }
}, { immediate: true })

function emitChange() {
  if (draft.value.length === 0) {
    emit('update:modelValue', null)
    return
  }
  const schedules = draft.value.map(row => {
    const entry: any = { type: row.type, startTime: row.startTime, endTime: row.endTime }
    if (row.type === 'weekly' && row.days && row.days.length > 0) {
      entry.days = row.days
    }
    return entry
  })
  emit('update:modelValue', JSON.stringify({ schedules }))
}

function addSchedule() {
  draft.value.push({ type: 'daily', startTime: '12:00', endTime: '14:00' })
  emitChange()
}

function removeSchedule(index: number) {
  draft.value.splice(index, 1)
  emitChange()
}

function toggleDay(index: number, day: number, event: Event) {
  const checked = (event.target as HTMLInputElement).checked
  if (!draft.value[index].days) draft.value[index].days = []
  if (checked) {
    if (!draft.value[index].days!.includes(day)) draft.value[index].days!.push(day)
  } else {
    draft.value[index].days = draft.value[index].days!.filter(d => d !== day)
  }
  emitChange()
}
</script>

<style scoped>
.schedule-row {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  align-items: flex-end;
  padding: 10px 0;
  border-bottom: 1px solid var(--border-color, #eee);
}
.weekday-checkboxes {
  display: flex;
  gap: 6px;
  flex-wrap: wrap;
}
.weekday-cb {
  display: flex;
  align-items: center;
  gap: 2px;
  font-size: 13px;
  cursor: pointer;
}
.weekday-cb input { margin: 0; }
</style>
