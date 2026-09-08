<script setup>
import { ref, watch, onMounted, onUnmounted, computed } from 'vue'
import { apiClient } from '@/lib/apiClient'

const props = defineProps({ modelValue: { type: Boolean, default: false } })
const emit = defineEmits(['update:modelValue', 'close'])

const permanentEffects = ref([])
const temporaryEffects = ref([])
const loading = ref(true)
const now = ref(Date.now())

let timer = null

async function loadPillEffects() {
  loading.value = true
  try {
    const data = await apiClient.getPillEffects()
    permanentEffects.value = data.permanentEffects ?? []
    temporaryEffects.value = data.temporaryEffects ?? []
  } catch (e) {
    console.error('加载丹药效果失败:', e)
  } finally {
    loading.value = false
  }
}

function close() {
  emit('update:modelValue', false)
  emit('close')
}

function formatCountdown(expiresAt) {
  const diff = new Date(expiresAt).getTime() - now.value
  if (diff <= 0) return '已过期'
  const minutes = Math.floor(diff / 60000)
  const seconds = Math.floor((diff % 60000) / 1000)
  return `${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`
}

const activeTemporaryEffects = computed(() => {
  return temporaryEffects.value.filter(e => new Date(e.expiresAt).getTime() > now.value)
})

watch(() => props.modelValue, async (visible) => {
  if (visible) {
    await loadPillEffects()
  }
})

onMounted(() => {
  timer = setInterval(() => { now.value = Date.now() }, 1000)
})

onUnmounted(() => {
  if (timer) clearInterval(timer)
})
</script>

<template>
  <div v-if="modelValue" class="modal-overlay" @click.self="close">
    <div class="pill-effect-modal">
      <div class="modal-header">
        <h3>丹药效果</h3>
        <button class="close-btn" @click="close">&times;</button>
      </div>

      <div v-if="loading" class="modal-body loading-state">加载中...</div>

      <div v-else class="modal-body">
        <div class="effects-columns">
          <div class="effects-column">
            <h4 class="column-title">永久生效</h4>
            <div v-if="permanentEffects.length === 0" class="empty-hint">暂无</div>
            <div v-else class="effect-list">
              <div v-for="effect in permanentEffects" :key="'p-' + effect.itemId" class="effect-row">
                <span class="effect-name">{{ effect.name }}</span>
                <span class="effect-type">{{ effect.bonusType }}</span>
                <span class="effect-value">+{{ effect.cumulativeValue }}</span>
                <span class="effect-count">x{{ effect.usageCount }}</span>
              </div>
            </div>
          </div>

          <div class="effects-column">
            <h4 class="column-title">限时生效</h4>
            <div v-if="activeTemporaryEffects.length === 0" class="empty-hint">暂无</div>
            <div v-else class="effect-list">
              <div v-for="effect in activeTemporaryEffects" :key="'t-' + effect.itemId" class="effect-row temporary">
                <span class="effect-name">{{ effect.name }}</span>
                <span class="effect-type">{{ effect.bonusType }}</span>
                <span class="effect-value">+{{ effect.currentValue }}</span>
                <span class="countdown">{{ formatCountdown(effect.expiresAt) }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.6);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}
.pill-effect-modal {
  background: var(--xiuxian-bg-panel);
  border: 1px solid var(--border-color);
  border-radius: 12px;
  width: 90%;
  max-width: 700px;
  max-height: 80vh;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}
.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 20px;
  border-bottom: 1px solid var(--border-color);
}
.modal-header h3 { margin: 0; color: var(--text-primary); font-size: 18px; }
.close-btn { background: none; border: none; color: var(--text-muted); font-size: 24px; cursor: pointer; }
.close-btn:hover { color: var(--text-primary); }
.modal-body { padding: 16px 20px; overflow-y: auto; }
.loading-state { text-align: center; color: var(--text-muted); padding: 40px 0; }
.effects-columns { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }
.column-title { margin: 0 0 8px; color: var(--text-secondary); font-size: 13px; font-weight: 500; }
.empty-hint { color: var(--text-muted); font-size: 12px; padding: 4px 0; }
.effect-list { display: flex; flex-direction: column; gap: 4px; }
.effect-row {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 6px 10px;
  background: var(--bg-overlay-light);
  border: 1px solid rgba(255, 255, 255, 0.06);
  border-radius: var(--radius-sm);
  font-size: 13px;
}
.effect-row.temporary { border-color: rgba(100, 200, 255, 0.15); }
.effect-name { color: var(--text-primary); font-weight: 500; white-space: nowrap; }
.effect-type { color: var(--text-muted); font-size: 12px; white-space: nowrap; }
.effect-value { color: var(--quality-uncommon); font-weight: 600; white-space: nowrap; margin-left: auto; }
.effect-count { color: var(--text-muted); font-size: 12px; white-space: nowrap; }
.countdown { color: #60a5fa; font-family: var(--font-mono); font-size: 13px; white-space: nowrap; margin-left: auto; }
</style>
