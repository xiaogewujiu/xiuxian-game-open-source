<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">成长配置</p>
        <h2 class="section-title section-title--small">属性点配置</h2>
        <p class="section-note">维护属性点整数收益和等级区间给点规则，保存后会直接刷新成长域运行时配置。</p>
      </div>

      <div class="filter-actions">
        <button class="secondary-button" type="button" @click="loadConfig">刷新</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs || loading" @click="saveConfig">保存配置</button>
      </div>

      <div v-if="config" class="detail-overview">
        <article class="detail-overview__item"><span>来源</span><strong>{{ config.isBuiltIn ? '内置' : '人工维护' }}</strong></article>
        <article class="detail-overview__item"><span>内置版本</span><strong>{{ config.builtInVersion || '-' }}</strong></article>
        <article class="detail-overview__item"><span>最后更新</span><strong>{{ formatDateTime(config.lastUpdateTime) }}</strong></article>
      </div>
    </section>

    <section class="dashboard-section-grid">
      <section v-for="profession in professionOptions" :key="profession.value" class="panel-box table-panel">
        <div class="section-header-row">
          <div class="section-header-copy">
            <p class="section-kicker">整数收益</p>
            <h3 class="section-title section-title--small">{{ profession.label }}属性收益</h3>
          </div>
          <span class="selected-pill">共 {{ getProfessionRows(profession.value).length }} 项</span>
        </div>

        <div class="table-wrap">
          <table class="data-table">
            <thead>
              <tr>
                <th>属性键</th>
                <th>名称</th>
                <th>底层属性</th>
                <th>触发收益</th>
                <th>触发点数</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="row in getProfessionRows(profession.value)" :key="`${profession.value}-${row.key}`">
                <td>{{ row.key }}</td>
                <td>
                  <input v-model.trim="row.name" type="text" :disabled="!canManageConfigs" />
                </td>
                <td>{{ getAttributeTypeLabel(row.attributeType) }}</td>
                <td>
                  <input v-model.number="row.bonusPerPoint" type="number" step="1" min="0" :disabled="!canManageConfigs" />
                </td>
                <td>
                  <input v-model.number="row.pointsPerBonus" type="number" step="1" min="1" :disabled="!canManageConfigs" />
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">给点规则</p>
          <h3 class="section-title section-title--small">等级区间给点</h3>
        </div>
        <div class="filter-actions">
          <span class="selected-pill">共 {{ levelRangeRows.length }} 段</span>
          <button class="secondary-button" type="button" :disabled="!canManageConfigs" @click="addLevelRange">新增区间</button>
        </div>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>开始等级</th>
              <th>结束等级</th>
              <th>每级给点</th>
              <th>操作</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(row, index) in levelRangeRows" :key="`${row.levelStart}-${row.levelEnd}-${index}`">
              <td>
                <input v-model.number="row.levelStart" type="number" min="2" max="100" :disabled="!canManageConfigs" />
              </td>
              <td>
                <input v-model.number="row.levelEnd" type="number" min="2" max="100" :disabled="!canManageConfigs" />
              </td>
              <td>
                <input v-model.number="row.pointsGained" type="number" min="0" max="99" :disabled="!canManageConfigs" />
              </td>
              <td>
                <button class="secondary-button" type="button" :disabled="!canManageConfigs || levelRangeRows.length <= 1" @click="removeLevelRange(index)">删除</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

    </section>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import { getAdminAttributePointConfig, saveAdminAttributePointConfig } from '@/services/attribute-point-configs'
import type {
  AdminAttributePointConfigBundle,
  AdminAttributePointDefinition,
  AdminAttributePointLevelRange
} from '@/types/admin'
import toast from '@/utils/toast'

// 属性点配置页把“整数收益表”和“等级区间给点”放在同一页统一维护。
const { canManageConfigs } = useAdminPermissions()
const loading = ref(false)
const config = ref<AdminAttributePointConfigBundle | null>(null)
const attributeRows = ref<AdminAttributePointDefinition[]>([])
const levelRangeRows = ref<AdminAttributePointLevelRange[]>([])

const professionOptions = [
  { value: 'warrior', label: '战' },
  { value: 'mage', label: '法' },
  { value: 'body', label: '体' }
]

const attributeTypeLabelMap: Record<number, string> = {
  1: 'Type1 / 血量',
  2: 'Type2 / 蓝量',
  3: 'Type3 / 物攻',
  4: 'Type4 / 法攻',
  5: 'Type5 / 物防',
  6: 'Type6 / 法防',
  7: 'Type7 / 速度'
}

// 底层属性编号转中文标签。
function getAttributeTypeLabel(attributeType: number) {
  return attributeTypeLabelMap[attributeType] || `Type${attributeType}`
}

// 统一格式化后台时间字段。
function formatDateTime(value?: string | null) {
  if (!value) return '-'
  return value.replace('T', ' ').slice(0, 19)
}

// 按职业筛选倍率行。
function getProfessionRows(profession: string) {
  return attributeRows.value.filter((item) => item.profession === profession)
}

// 把后端返回的配置同步到当前可编辑状态。
function hydrate(data: AdminAttributePointConfigBundle) {
  config.value = data
  attributeRows.value = data.attributes
    .map((item) => ({ ...item }))
    .sort((a, b) => {
      const professionOrder = professionOptions.findIndex((item) => item.value === a.profession) - professionOptions.findIndex((item) => item.value === b.profession)
      if (professionOrder !== 0) {
        return professionOrder
      }

      return a.attributeType - b.attributeType
    })
  levelRangeRows.value = data.levelRanges
    .map((item) => ({ ...item }))
    .sort((a, b) => a.levelStart - b.levelStart || a.levelEnd - b.levelEnd)
}

// 拉取属性点配置总览。
async function loadConfig() {
  try {
    const data = await getAdminAttributePointConfig()
    hydrate(data)
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '属性点配置读取失败。')
  }
}

// 追加一个新的等级区间规则。
function addLevelRange() {
  const last = levelRangeRows.value[levelRangeRows.value.length - 1]
  const nextStart = last ? Math.min(100, last.levelEnd + 1) : 2
  levelRangeRows.value.push({
    levelStart: nextStart,
    levelEnd: nextStart,
    pointsGained: last?.pointsGained ?? 4
  })
}

// 删除指定等级区间规则。
function removeLevelRange(index: number) {
  levelRangeRows.value.splice(index, 1)
}

// 保存属性点配置。
async function saveConfig() {
  if (!canManageConfigs.value || !config.value) return

  try {
    loading.value = true
    const payload: AdminAttributePointConfigBundle = {
      ...config.value,
      attributes: attributeRows.value.map((item) => ({
        ...item,
        profession: item.profession,
        bonusPerPoint: Math.max(0, Math.trunc(Number(item.bonusPerPoint) || 0)),
        pointsPerBonus: Math.max(1, Math.trunc(Number(item.pointsPerBonus) || 1))
      })),
      levelRanges: levelRangeRows.value
        .map((item) => ({
          ...item,
          levelStart: Number(item.levelStart) || 0,
          levelEnd: Number(item.levelEnd) || 0,
          pointsGained: Number(item.pointsGained) || 0
        }))
        .sort((a, b) => a.levelStart - b.levelStart || a.levelEnd - b.levelEnd)
    }
    const data = await saveAdminAttributePointConfig(payload)
    hydrate(data)
    toast.success('属性点配置保存成功。')
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '属性点配置保存失败。')
  } finally {
    loading.value = false
  }
}

onMounted(loadConfig)
</script>
