<template>
  <XiuXianModal :model-value="modelValue" title="游戏设置" :width="560" @update:model-value="$emit('update:modelValue', $event)">
    <div class="settings-modal">
      <section class="settings-card">
        <div class="settings-card-title">装备自动出售</div>
        <p class="settings-card-desc">普通地图、离线战斗、副本和秘境结算获得的装备，会按照以下条件自动清理。</p>
        <label class="settings-field">
          <span>最低保留装备等级</span>
          <select v-model.number="form.minEquipmentLevel">
            <option :value="0">不限制</option>
            <option v-for="level in levelOptions" :key="level" :value="level">Lv.{{ level }} 及以上</option>
          </select>
        </label>
        <label class="settings-field">
          <span>最低保留装备品质</span>
          <select v-model.number="form.minEquipmentQuality">
            <option :value="0">不限制</option>
            <option v-for="quality in qualityOptions" :key="quality.value" :value="quality.value">{{ quality.label }}及以上</option>
          </select>
        </label>
        <div class="settings-note">
          <div>规则说明</div>
          <p>装备等级和品质都达到设定值时保留；未配置的条件不参与限制。</p>
          <p>两个条件都选择“不限制”时，不会自动出售任何装备。</p>
          <p>已穿戴或已绑定装备不会被自动出售。</p>
        </div>
      </section>
      <div v-if="message" class="settings-message" :class="{ error: isError }">{{ message }}</div>
      <div class="settings-actions">
        <button class="secondary-btn" type="button" @click="loadSettings" :disabled="loading">恢复当前设置</button>
        <button class="primary-btn" type="button" @click="saveSettings" :disabled="saving">{{ saving ? '保存中...' : '保存设置' }}</button>
      </div>
    </div>
  </XiuXianModal>
</template>

<script>
import { reactive, ref, watch } from 'vue'
import XiuXianModal from '../common/XiuXianModal.vue'
import { apiClient } from '../../lib/apiClient'

const qualityOptions = [
  { value: 1, label: '普通' }, { value: 2, label: '优秀' }, { value: 3, label: '精良' },
  { value: 4, label: '史诗' }, { value: 5, label: '传说' }
]

export default {
  name: 'SettingsModal',
  components: { XiuXianModal },
  props: { modelValue: Boolean },
  emits: ['update:modelValue'],
  setup(props) {
    const form = reactive({ minEquipmentLevel: 0, minEquipmentQuality: 0 })
    const levelOptions = [1, 5, 6, ...Array.from({ length: 19 }, (_, index) => (index + 1) * 5)].filter((level, index, levels) => levels.indexOf(level) === index)
    const loading = ref(false)
    const saving = ref(false)
    const message = ref('')
    const isError = ref(false)
    const applySettings = (settings) => {
      form.minEquipmentLevel = Number(settings?.minEquipmentLevel ?? settings?.MinEquipmentLevel ?? 0) || 0
      form.minEquipmentQuality = Number(settings?.minEquipmentQuality ?? settings?.MinEquipmentQuality ?? 0) || 0
    }
    const loadSettings = async () => {
      loading.value = true; message.value = ''; isError.value = false
      try { applySettings(await apiClient.getEquipmentAutoSellSettings()); message.value = '已恢复当前设置。' }
      catch (error) { isError.value = true; message.value = error.message || '读取设置失败。' }
      finally { loading.value = false }
    }
    const saveSettings = async () => {
      saving.value = true; message.value = ''; isError.value = false
      try {
        const result = await apiClient.updateEquipmentAutoSellSettings({ minEquipmentLevel: form.minEquipmentLevel, minEquipmentQuality: form.minEquipmentQuality })
        applySettings(result); message.value = '自动出售设置已保存。'
      } catch (error) { isError.value = true; message.value = error.message || '保存设置失败。' }
      finally { saving.value = false }
    }
    watch(() => props.modelValue, (visible) => { if (visible) loadSettings() })
    return { form, levelOptions, qualityOptions, loading, saving, message, isError, loadSettings, saveSettings }
  }
}
</script>

<style scoped>
.settings-modal { display: flex; flex-direction: column; gap: var(--spacing-md); color: var(--text-primary); }
.settings-card { padding: var(--spacing-lg); border: 1px solid var(--border-color); border-radius: var(--radius-md); background: rgba(255,255,255,.025); }
.settings-card-title { color: var(--highlight-text); font-size: 17px; font-weight: 600; }
.settings-card-desc { margin: 6px 0 18px; color: var(--text-secondary); line-height: 1.6; }
.settings-field { display: grid; grid-template-columns: 170px 1fr; align-items: center; gap: var(--spacing-md); margin-top: var(--spacing-md); }
.settings-field select { width: 100%; }
.settings-note { margin-top: 18px; padding: 12px; border-left: 3px solid var(--accent-text); background: var(--accent-soft-bg); color: var(--text-secondary); line-height: 1.65; }
.settings-note div { color: var(--text-primary); font-weight: 600; }
.settings-note p { margin: 3px 0 0; }
.settings-message { padding: 9px 12px; border: 1px solid var(--control-border); border-radius: var(--radius-sm); background: var(--accent-soft-bg); }
.settings-message.error { color: var(--status-danger-text); }
.settings-actions { display: flex; justify-content: flex-end; gap: var(--spacing-sm); }
.primary-btn, .secondary-btn { padding: 8px 16px; border-radius: var(--radius-sm); cursor: pointer; font-family: inherit; }
.primary-btn { border: 1px solid transparent; background: var(--button-primary-start); color: var(--button-primary-text); }
.secondary-btn { border: 1px solid var(--control-border); background: var(--control-bg); color: var(--control-text); }
.primary-btn:disabled, .secondary-btn:disabled { opacity: .55; cursor: not-allowed; }
@media (max-width: 560px) { .settings-field { grid-template-columns: 1fr; gap: 5px; } .settings-actions { flex-direction: column-reverse; } .primary-btn, .secondary-btn { width: 100%; } }
</style>
