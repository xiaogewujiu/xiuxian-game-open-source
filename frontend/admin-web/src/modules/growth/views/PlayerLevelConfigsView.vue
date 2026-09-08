<template>
  <div class="dashboard-grid">
    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">等级成长</p>
          <h2 class="section-title section-title--small">等级成长配置</h2>
          <p class="section-note">维护升级经验和基础属性成长。名称展示继续走现有境界配置，不在这里维护。</p>
        </div>
        <span class="selected-pill">共 {{ rows.length }} 级</span>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>等级</th>
              <th>升级经验</th>
              <th>血量</th>
              <th>蓝量</th>
              <th>物攻</th>
              <th>法攻</th>
              <th>物防</th>
              <th>法防</th>
              <th>速度</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="item in rows"
              :key="item.level"
              :class="{ 'is-selected-row': selectedLevel === item.level }"
              @click="openDetailModal(item.level)"
            >
              <td>{{ item.level }}</td>
              <td>{{ item.requiredExp }}</td>
              <td>{{ item.baseHp }}</td>
              <td>{{ item.baseMp }}</td>
              <td>{{ item.basePhysicalAttack }}</td>
              <td>{{ item.baseMagicAttack }}</td>
              <td>{{ item.basePhysicalDefense }}</td>
              <td>{{ item.baseMagicDefense }}</td>
              <td>{{ item.baseSpeed }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>

    <AdminModal
      v-model="detailOpen"
      kicker="等级详情"
      :title="selectedDetail ? `第 ${selectedDetail.level} 级成长数值` : '等级成长详情'"
      description="点击表格行后在弹窗内查看和编辑当前等级的成长配置。"
      size="wide"
    >
      <fieldset v-if="selectedDetail" class="form-fieldset" :disabled="loading">
        <div class="detail-overview detail-overview--compact">
          <article class="detail-overview__item"><span>来源</span><strong>{{ selectedDetail.isBuiltIn ? '内置' : '人工维护' }}</strong></article>
          <article class="detail-overview__item"><span>种子键</span><strong>{{ selectedDetail.seedKey || '-' }}</strong></article>
          <article class="detail-overview__item"><span>内置版本</span><strong>{{ selectedDetail.builtInVersion || '-' }}</strong></article>
          <article class="detail-overview__item"><span>最后更新</span><strong>{{ formatDateTime(selectedDetail.lastUpdateTime) }}</strong></article>
        </div>

        <div class="editor-grid editor-grid--three">
          <label class="field">
            <span>升级经验</span>
            <input v-model.number="selectedDetail.requiredExp" type="number" min="0" :disabled="!canManageConfigs || loading || selectedDetail.level >= 100" />
          </label>
          <label class="field">
            <span>基础血量</span>
            <input v-model.number="selectedDetail.baseHp" type="number" min="1" :disabled="!canManageConfigs || loading" />
          </label>
          <label class="field">
            <span>基础蓝量</span>
            <input v-model.number="selectedDetail.baseMp" type="number" min="0" :disabled="!canManageConfigs || loading" />
          </label>
          <label class="field">
            <span>基础物攻</span>
            <input v-model.number="selectedDetail.basePhysicalAttack" type="number" min="0" :disabled="!canManageConfigs || loading" />
          </label>
          <label class="field">
            <span>基础法攻</span>
            <input v-model.number="selectedDetail.baseMagicAttack" type="number" min="0" :disabled="!canManageConfigs || loading" />
          </label>
          <label class="field">
            <span>基础物防</span>
            <input v-model.number="selectedDetail.basePhysicalDefense" type="number" min="0" :disabled="!canManageConfigs || loading" />
          </label>
          <label class="field">
            <span>基础法防</span>
            <input v-model.number="selectedDetail.baseMagicDefense" type="number" min="0" :disabled="!canManageConfigs || loading" />
          </label>
          <label class="field">
            <span>基础速度</span>
            <input v-model.number="selectedDetail.baseSpeed" type="number" min="0" :disabled="!canManageConfigs || loading" />
          </label>
        </div>

      </fieldset>

      <template #footer>
        <button class="secondary-button" type="button" @click="detailOpen = false">关闭</button>
        <button class="secondary-button" type="button" :disabled="loading || selectedLevel === null" @click="reloadSelected">重新加载</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs || loading || !selectedDetail" @click="saveSelected">保存配置</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import {
  getAdminPlayerLevelConfigDetail,
  getAdminPlayerLevelConfigs,
  saveAdminPlayerLevelConfig
} from '@/services/player-level-configs'
import type {
  AdminPlayerLevelConfigDetail,
  AdminPlayerLevelConfigListItem
} from '@/types/admin'
import toast from '@/utils/toast'

// 等级成长页只维护“升级经验 + 基础属性”。
const { canManageConfigs } = useAdminPermissions()
const loading = ref(false)
const rows = ref<AdminPlayerLevelConfigListItem[]>([])
const selectedLevel = ref<number | null>(null)
const selectedDetail = ref<AdminPlayerLevelConfigDetail | null>(null)
const detailOpen = ref(false)

// 统一格式化后台时间字段。
function formatDateTime(value?: string | null) {
  if (!value) return '-'
  return value.replace('T', ' ').slice(0, 19)
}

// 拉取等级成长列表。
async function loadList() {
  rows.value = await getAdminPlayerLevelConfigs()
}

// 拉取单个等级详情。
async function loadDetail(level: number) {
  selectedDetail.value = await getAdminPlayerLevelConfigDetail(level)
}

// 打开等级详情弹窗。
async function openDetailModal(level: number) {
  try {
    loading.value = true
    selectedLevel.value = level
    await loadDetail(level)
    detailOpen.value = true
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '等级成长配置读取失败。')
  } finally {
    loading.value = false
  }
}

// 重新加载当前选中的等级详情。
async function reloadSelected() {
  if (selectedLevel.value === null) return
  try {
    loading.value = true
    await loadDetail(selectedLevel.value)
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '等级成长配置读取失败。')
  } finally {
    loading.value = false
  }
}

// 保存当前等级成长配置。
async function saveSelected() {
  if (!selectedDetail.value || !canManageConfigs.value) return

  try {
    loading.value = true
    const payload: AdminPlayerLevelConfigDetail = {
      ...selectedDetail.value,
      requiredExp: Number(selectedDetail.value.requiredExp) || 0,
      baseHp: Number(selectedDetail.value.baseHp) || 0,
      baseMp: Number(selectedDetail.value.baseMp) || 0,
      basePhysicalAttack: Number(selectedDetail.value.basePhysicalAttack) || 0,
      baseMagicAttack: Number(selectedDetail.value.baseMagicAttack) || 0,
      basePhysicalDefense: Number(selectedDetail.value.basePhysicalDefense) || 0,
      baseMagicDefense: Number(selectedDetail.value.baseMagicDefense) || 0,
      baseSpeed: Number(selectedDetail.value.baseSpeed) || 0
    }
    const saved = await saveAdminPlayerLevelConfig(payload)
    selectedDetail.value = saved
    await loadList()
    detailOpen.value = false
    toast.success('等级成长配置保存成功。')
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '等级成长配置保存失败。')
  } finally {
    loading.value = false
  }
}

onMounted(async () => {
  try {
    await loadList()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '等级成长配置读取失败。')
  }
})
</script>
