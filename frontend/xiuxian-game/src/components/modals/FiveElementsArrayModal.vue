<template>
  <XiuXianModal
    :model-value="modelValue"
    title="五行聚灵阵"
    :width="750"
    @update:model-value="$emit('update:modelValue', $event)"
  >
    <div class="five-elements-array-modal">
      <FiveElementsArray in-modal />
    </div>
  </XiuXianModal>
</template>

<script>
import XiuXianModal from '../common/XiuXianModal.vue'
import FiveElementsArray from '../FiveElementsArray.vue'

/**
 * FiveElementsArrayModal - 五行聚灵阵弹窗
 */
export default {
  name: 'FiveElementsArrayModal',

  components: {
    XiuXianModal,
    FiveElementsArray
  },

  props: {
    modelValue: {
      type: Boolean,
      required: true
    }
  },

  emits: ['update:modelValue']
}
</script>

<style scoped>
.five-elements-array-modal {
  min-height: 380px;
  display: flex;
  flex-direction: column;
}
</style>
