<template>
  <Teleport to="body">
    <div v-if="modelValue" class="admin-modal" role="dialog" aria-modal="true" @keydown.esc="emitClose">
      <button class="admin-modal__backdrop" type="button" aria-label="关闭弹窗" @click="emitClose"></button>

      <section class="admin-modal__card" :class="cardClass">
        <header class="admin-modal__header">
          <div class="admin-modal__heading">
            <p v-if="kicker" class="section-kicker">{{ kicker }}</p>
            <h3 class="admin-modal__title">{{ title }}</h3>
            <p v-if="description" class="section-note">{{ description }}</p>
          </div>

          <button class="header-icon-button" type="button" aria-label="关闭弹窗" @click="emitClose">
            ×
          </button>
        </header>

        <div class="admin-modal__body">
          <slot />
        </div>

        <footer v-if="$slots.footer" class="admin-modal__footer">
          <slot name="footer" />
        </footer>
      </section>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const props = withDefaults(defineProps<{
  modelValue: boolean
  title: string
  kicker?: string
  description?: string
  size?: 'default' | 'wide' | 'xwide'
}>(), {
  kicker: '',
  description: '',
  size: 'default'
})

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
}>()

const cardClass = computed(() => ({
  'admin-modal__card--wide': props.size === 'wide',
  'admin-modal__card--xwide': props.size === 'xwide'
}))

function emitClose() {
  emit('update:modelValue', false)
}
</script>
