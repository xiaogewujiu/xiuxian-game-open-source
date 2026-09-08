<template>
  <div class="sutra-layer-editor">
    <div class="sutra-layer-editor__header">
      <span>层数配置（{{ layers.length }} 层）</span>
      <div class="sutra-layer-editor__actions">
        <button class="secondary-button" type="button" @click="autoFill">按最大层数生成</button>
        <button class="secondary-button" type="button" @click="addLayer">手动新增</button>
      </div>
    </div>

    <div v-if="layers.length === 0" class="empty-tip">暂无层数配置。点击"按最大层数生成"快速创建。</div>

    <div v-else class="layer-list">
      <div v-for="(layer, li) in layers" :key="li" class="layer-card" :class="{ 'layer-card--open': openLayers.has(li) }">
        <div class="layer-card__head" @click="toggle(li)">
          <span class="layer-card__index">第{{ layer.layer }}层</span>
          <span class="layer-card__name">{{ layer.name || '未命名' }}</span>
          <span class="layer-card__cost">
            贡献 {{ layer.contributionCost }} / 金币 {{ layer.goldCost }}
          </span>
          <span class="layer-card__bonus-count">{{ layer.bonuses.length }} 项加成</span>
          <span class="layer-card__arrow">{{ openLayers.has(li) ? '▲' : '▼' }}</span>
          <button class="danger-button danger-button--small" type="button" @click.stop="removeLayer(li)">删除</button>
        </div>

        <div v-if="openLayers.has(li)" class="layer-card__body">
          <div class="editor-grid editor-grid--three">
            <label class="field"><span>层名</span><input v-model.trim="layer.name" type="text" /></label>
            <label class="field"><span>贡献消耗</span><input v-model.number="layer.contributionCost" type="number" min="0" /></label>
            <label class="field"><span>金币消耗</span><input v-model.number="layer.goldCost" type="number" min="0" /></label>
          </div>
          <div class="editor-grid editor-grid--two">
            <label class="field"><span>解锁技能</span>
              <select v-model="layer.unlockSkillId">
                <option :value="null">无</option>
                <option v-for="s in skills" :key="s.skillId" :value="String(s.skillId)">{{ s.name }} ({{ s.skillId }})</option>
              </select>
            </label>
            <label class="field"><span>解锁Buff</span>
              <select v-model="layer.unlockBuffId">
                <option :value="null">无</option>
                <option v-for="b in buffs" :key="b.buffId" :value="b.buffId">{{ b.name }} ({{ b.buffId }})</option>
              </select>
            </label>
          </div>

          <div class="bonus-section">
            <div class="bonus-section__header">
              <span>属性加成</span>
              <button class="secondary-button secondary-button--small" type="button" @click="addBonus(li)">新增加成</button>
            </div>
            <div v-if="layer.bonuses.length === 0" class="empty-tip empty-tip--sm">暂无加成</div>
            <table v-else class="bonus-table">
              <thead>
                <tr>
                  <th>属性</th>
                  <th>数值</th>
                  <th>百分比</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="(bonus, bi) in layer.bonuses" :key="bi">
                  <td>
                    <select v-model="bonus.attributeName" class="bonus-select">
                      <option v-for="attr in attributeOptions" :key="attr.value" :value="attr.value">{{ attr.label }}</option>
                    </select>
                  </td>
                  <td><input v-model.number="bonus.value" type="number" class="bonus-input" /></td>
                  <td><input v-model="bonus.isPercentage" type="checkbox" /></td>
                  <td><button class="danger-button danger-button--small" type="button" @click="removeBonus(li, bi)">删除</button></td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'

interface SutraAttributeBonus {
  attributeName: string
  value: number
  isPercentage: boolean
}

interface SutraLayerConfig {
  layer: number
  name: string
  contributionCost: number
  goldCost: number
  bonuses: SutraAttributeBonus[]
  unlockSkillId: string | null
  unlockBuffId: string | null
}

interface SkillOption { skillId: number; name: string }
interface BuffOption { buffId: string; name: string }

const props = defineProps<{
  modelValue: string
  maxLayer: number
  skills?: SkillOption[]
  buffs?: BuffOption[]
}>()
const emit = defineEmits<{ 'update:modelValue': [string] }>()

const layers = ref<SutraLayerConfig[]>([])
const openLayers = ref(new Set<number>())

const attributeOptions = [
  { value: 'Type1', label: '生命 (Type1)' },
  { value: 'Type2', label: '法力 (Type2)' },
  { value: 'Type3', label: '物攻 (Type3)' },
  { value: 'Type4', label: '法攻 (Type4)' },
  { value: 'Type5', label: '物防 (Type5)' },
  { value: 'Type6', label: '法防 (Type6)' },
  { value: 'Type7', label: '速度 (Type7)' },
  { value: 'Type8', label: '命中 (Type8)' },
  { value: 'Type9', label: '暴击 (Type9)' },
  { value: 'Type10', label: '暴伤 (Type10)' },
  { value: 'Type11', label: '连击 (Type11)' },
  { value: 'Type12', label: '反击 (Type12)' },
  { value: 'Type13', label: '破甲 (Type13)' },
  { value: 'Type14', label: '增伤 (Type14)' },
  { value: 'Type15', label: '闪避 (Type15)' },
]

watch(() => props.modelValue, (val) => {
  try {
    const parsed = JSON.parse(val || '[]')
    layers.value = Array.isArray(parsed) ? parsed.map(normalizeLayer) : []
  } catch {
    layers.value = []
  }
}, { immediate: true })

function normalizeLayer(raw: any): SutraLayerConfig {
  return {
    layer: raw.Layer ?? raw.layer ?? 0,
    name: raw.Name ?? raw.name ?? '',
    contributionCost: raw.ContributionCost ?? raw.contributionCost ?? 0,
    goldCost: raw.GoldCost ?? raw.goldCost ?? 0,
    bonuses: Array.isArray(raw.Bonuses ?? raw.bonuses)
      ? (raw.Bonuses ?? raw.bonuses).map((b: any) => ({
          attributeName: b.AttributeName ?? b.attributeName ?? '',
          value: b.Value ?? b.value ?? 0,
          isPercentage: b.IsPercentage ?? b.isPercentage ?? false,
        }))
      : [],
    unlockSkillId: raw.UnlockSkillId ?? raw.unlockSkillId ?? null,
    unlockBuffId: raw.UnlockBuffId ?? raw.unlockBuffId ?? null,
  }
}

function emitChange() {
  emit('update:modelValue', JSON.stringify(layers.value))
}

function toggle(index: number) {
  if (openLayers.value.has(index)) {
    openLayers.value.delete(index)
  } else {
    openLayers.value.add(index)
  }
}

function autoFill() {
  const count = props.maxLayer || 10
  layers.value = Array.from({ length: count }, (_, i) => ({
    layer: i + 1,
    name: `第${i + 1}层`,
    contributionCost: (i + 1) * 100,
    goldCost: (i + 1) * 500,
    bonuses: [],
    unlockSkillId: null,
    unlockBuffId: null,
  }))
  openLayers.value.clear()
  emitChange()
}

function addLayer() {
  const next = layers.value.length + 1
  layers.value.push({
    layer: next,
    name: `第${next}层`,
    contributionCost: 0,
    goldCost: 0,
    bonuses: [],
    unlockSkillId: null,
    unlockBuffId: null,
  })
  openLayers.value.add(layers.value.length - 1)
  emitChange()
}

function removeLayer(index: number) {
  layers.value.splice(index, 1)
  openLayers.value.delete(index)
  emitChange()
}

function addBonus(layerIndex: number) {
  layers.value[layerIndex].bonuses.push({
    attributeName: 'Type1',
    value: 0,
    isPercentage: false,
  })
  emitChange()
}

function removeBonus(layerIndex: number, bonusIndex: number) {
  layers.value[layerIndex].bonuses.splice(bonusIndex, 1)
  emitChange()
}
</script>

<style scoped>
.sutra-layer-editor__header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
  font-size: 14px;
  font-weight: 600;
  color: var(--text-primary, #1d2939);
}

.sutra-layer-editor__actions {
  display: flex;
  gap: 8px;
}

.layer-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.layer-card {
  border: 1px solid var(--line-soft, #e4e7ec);
  border-radius: 8px;
  overflow: hidden;
  background: var(--bg-panel, #ffffff);
}

.layer-card__head {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 10px 14px;
  cursor: pointer;
  user-select: none;
  background: var(--bg-panel-soft, #fafbfc);
  font-size: 13px;
}

.layer-card__head:hover {
  background: var(--bg-panel-muted, #f1f5f9);
}

.layer-card__index {
  font-weight: 700;
  color: var(--accent-primary, #1677ff);
  min-width: 52px;
}

.layer-card__name {
  color: var(--text-primary, #1d2939);
  font-weight: 500;
  min-width: 80px;
}

.layer-card__cost {
  color: var(--text-secondary, #667085);
  font-size: 12px;
  flex: 1;
}

.layer-card__bonus-count {
  color: var(--text-muted, #98a2b3);
  font-size: 12px;
  min-width: 70px;
  text-align: right;
}

.layer-card__arrow {
  color: var(--text-muted, #98a2b3);
  font-size: 11px;
  margin-left: 4px;
}

.layer-card__body {
  padding: 14px;
  display: flex;
  flex-direction: column;
  gap: 12px;
  border-top: 1px solid var(--line-soft, #e4e7ec);
}

.bonus-section__header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 13px;
  font-weight: 600;
  color: var(--text-secondary, #667085);
  margin-bottom: 8px;
}

.bonus-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 13px;
}

.bonus-table th {
  text-align: left;
  padding: 6px 8px;
  color: var(--text-muted, #98a2b3);
  font-weight: 500;
  border-bottom: 1px solid var(--line-soft, #e4e7ec);
}

.bonus-table td {
  padding: 4px 8px;
  border-bottom: 1px solid var(--line-soft, #e4e7ec);
}

.bonus-select {
  width: 100%;
  padding: 4px 6px;
  font-size: 12px;
}

.bonus-input {
  width: 100px;
  padding: 4px 6px;
  font-size: 12px;
}

.secondary-button--small {
  padding: 3px 10px;
  font-size: 12px;
}

.empty-tip--sm {
  font-size: 12px;
  padding: 6px 0;
}
</style>
