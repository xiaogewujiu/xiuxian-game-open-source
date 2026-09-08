<template>
  <Teleport to="body">
    <Transition name="confirm-fade">
      <div v-if="visible" class="cd-overlay" @click.self="onCancel">
        <div class="cd-card">
          <div class="cd-header">
            <span class="cd-icon">{{ icon }}</span>
            <span class="cd-title">{{ title }}</span>
          </div>
          <div class="cd-body">{{ message }}</div>
          <div class="cd-footer">
            <button v-if="cancelText" class="cd-btn cd-btn--cancel" @click="onCancel">{{ cancelText }}</button>
            <button class="cd-btn cd-btn--confirm" :class="`cd-btn--${type}`" @click="onConfirm">{{ confirmText }}</button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script>
import { ref, onMounted } from 'vue'

export default {
  name: 'ConfirmDialog',
  props: {
    title: { type: String, default: '确认' },
    message: { type: String, default: '' },
    type: { type: String, default: 'warning' },
    confirmText: { type: String, default: '确认' },
    cancelText: { type: String, default: '取消' },
    icon: { type: String, default: '⚠' }
  },
  emits: ['confirm', 'cancel'],
  setup(props, { emit }) {
    const visible = ref(false)

    onMounted(() => { visible.value = true })

    function onConfirm() { visible.value = false; setTimeout(() => emit('confirm'), 200) }
    function onCancel() { visible.value = false; setTimeout(() => emit('cancel'), 200) }

    return { visible, onConfirm, onCancel }
  }
}
</script>

<style scoped>
.cd-overlay {
  position: fixed; inset: 0; z-index: 10000;
  background: rgba(0,0,0,0.55);
  display: flex; align-items: center; justify-content: center;
}
.cd-card {
  background: var(--card-bg, #1e1e2e);
  border: 1px solid var(--border, #333);
  border-radius: 12px;
  padding: 24px 28px;
  min-width: 320px; max-width: 420px;
  box-shadow: 0 8px 32px rgba(0,0,0,0.4);
}
.cd-header {
  display: flex; align-items: center; gap: 10px;
  margin-bottom: 14px; font-size: 16px; font-weight: 600;
  color: var(--foreground, #e0e0e0);
}
.cd-icon { font-size: 22px; }
.cd-body {
  color: var(--muted-foreground, #aaa);
  font-size: 14px; line-height: 1.6; margin-bottom: 20px;
  white-space: pre-wrap;
}
.cd-footer { display: flex; justify-content: flex-end; gap: 10px; }
.cd-btn {
  padding: 8px 20px; border-radius: 8px; border: none;
  font-size: 14px; cursor: pointer; transition: all 0.15s;
}
.cd-btn--cancel {
  background: var(--muted, #2a2a3a);
  color: var(--muted-foreground, #aaa);
}
.cd-btn--cancel:hover { background: var(--muted-hover, #3a3a4a); }
.cd-btn--confirm { color: #fff; }
.cd-btn--warning { background: #e5a100; }
.cd-btn--warning:hover { background: #cc8f00; }
.cd-btn--danger { background: #dc3545; }
.cd-btn--danger:hover { background: #c82333; }
.cd-btn--info { background: #0d6efd; }
.cd-btn--info:hover { background: #0b5ed7; }

.confirm-fade-enter-active,
.confirm-fade-leave-active { transition: opacity 0.2s ease; }
.confirm-fade-enter-from,
.confirm-fade-leave-to { opacity: 0; }
.confirm-fade-enter-active .cd-card {
  animation: cd-slide 0.25s ease;
}
@keyframes cd-slide {
  from { transform: translateY(16px) scale(0.96); opacity: 0; }
  to { transform: translateY(0) scale(1); opacity: 1; }
}
</style>
