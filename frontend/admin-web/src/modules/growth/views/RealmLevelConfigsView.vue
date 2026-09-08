<template>
  <div class="dashboard-grid">
    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">境界配置</p>
          <h2 class="section-title section-title--small">境界突破配置</h2>
          <p class="section-note">维护等级对应境界、突破成功率、经验损失和突破材料。保存后会刷新成长域运行时配置。</p>
        </div>
        <span class="selected-pill">共 {{ rows.length }} 级</span>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>等级</th>
              <th>境界</th>
              <th>别名</th>
              <th>升级经验</th>
              <th>属性加成</th>
              <th>突破</th>
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
              <td>{{ item.realmName }}</td>
              <td>{{ item.alias }}</td>
              <td>{{ item.requiredExp }}</td>
              <td>{{ item.attributeBonusPercent }}%</td>
              <td>{{ item.isBreakthroughPoint ? '是' : '否' }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>

    <AdminModal
      v-model="detailOpen"
      kicker="境界详情"
      :title="selectedDetail ? `第 ${selectedDetail.level} 级境界配置` : '境界突破详情'"
      description="点击表格行后在弹窗内查看和编辑该等级的境界与突破规则。"
      size="xwide"
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
            <span>大境界</span>
            <input v-model.trim="selectedDetail.realmName" type="text" :disabled="!canManageConfigs || loading" />
          </label>
          <label class="field">
            <span>显示别名</span>
            <input v-model.trim="selectedDetail.alias" type="text" :disabled="!canManageConfigs || loading" />
          </label>
          <label class="field">
            <span>境界排序</span>
            <input v-model.number="selectedDetail.realmOrder" type="number" min="1" :disabled="!canManageConfigs || loading" />
          </label>
          <label class="field">
            <span>层数</span>
            <input v-model.number="selectedDetail.layer" type="number" min="1" :disabled="!canManageConfigs || loading" />
          </label>
          <label class="field">
            <span>升级经验</span>
            <input v-model.number="selectedDetail.requiredExp" type="number" min="0" :disabled="!canManageConfigs || loading" />
          </label>
          <label class="field">
            <span>属性加成%</span>
            <input v-model.number="selectedDetail.attributeBonusPercent" type="number" min="0" :disabled="!canManageConfigs || loading" />
          </label>
          <label class="field checkbox-field">
            <input v-model="selectedDetail.isBreakthroughPoint" type="checkbox" :disabled="!canManageConfigs || loading || selectedDetail.level >= 100" />
            <span>是否突破关口</span>
          </label>
          <label class="field">
            <span>突破成功率%</span>
            <input v-model.number="selectedDetail.breakthroughSuccessRate" type="number" min="1" max="100" :disabled="!canManageConfigs || loading || !selectedDetail.isBreakthroughPoint" />
          </label>
          <label class="field">
            <span>失败经验损失%</span>
            <input v-model.number="selectedDetail.breakthroughExpLossPercent" type="number" min="0" max="100" :disabled="!canManageConfigs || loading || !selectedDetail.isBreakthroughPoint" />
          </label>
          <label class="field field--full">
            <span>说明</span>
            <textarea v-model.trim="selectedDetail.description" rows="3" :disabled="!canManageConfigs || loading" />
          </label>
        </div>

        <div class="section-header-row">
          <div class="section-header-copy">
            <p class="section-kicker">材料</p>
            <h3 class="section-title section-title--small">突破材料</h3>
          </div>
          <button class="secondary-button" type="button" :disabled="!canManageConfigs || loading || !selectedDetail.isBreakthroughPoint" @click="addMaterial">新增材料</button>
        </div>

        <div class="table-wrap">
          <table class="data-table">
            <thead>
              <tr>
                <th>道具ID</th>
                <th>数量</th>
                <th>操作</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(material, index) in selectedDetail.breakthroughMaterials" :key="`${material.itemId}-${index}`">
                <td>
                  <select v-model="material.itemId" :disabled="!canManageConfigs || loading || !selectedDetail.isBreakthroughPoint">
                    <option value="">请选择</option>
                    <option v-for="item in itemOptions" :key="item.itemId" :value="item.itemId">{{ item.name }} ({{ item.itemId }})</option>
                  </select>
                </td>
                <td>
                  <input v-model.number="material.count" type="number" min="1" :disabled="!canManageConfigs || loading || !selectedDetail.isBreakthroughPoint" />
                </td>
                <td>
                  <button class="secondary-button" type="button" :disabled="!canManageConfigs || loading || !selectedDetail.isBreakthroughPoint" @click="removeMaterial(index)">删除</button>
                </td>
              </tr>
            </tbody>
          </table>
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
import { getAdminItems } from '@/services/items'
import {
  getAdminRealmLevelConfigDetail,
  getAdminRealmLevelConfigs,
  saveAdminRealmLevelConfig
} from '@/services/realm-level-configs'
import type {
  AdminItemListItem,
  AdminRealmLevelConfigDetail,
  AdminRealmLevelConfigListItem
} from '@/types/admin'
import toast from '@/utils/toast'

// 境界页维护等级对应的境界名称、突破概率和突破材料。
const { canManageConfigs } = useAdminPermissions()
const loading = ref(false)
const rows = ref<AdminRealmLevelConfigListItem[]>([])
const itemOptions = ref<AdminItemListItem[]>([])
const selectedLevel = ref<number | null>(null)
const selectedDetail = ref<AdminRealmLevelConfigDetail | null>(null)
const detailOpen = ref(false)

// 统一格式化后台时间字段。
function formatDateTime(value?: string | null) {
  if (!value) return '-'
  return value.replace('T', ' ').slice(0, 19)
}

// 拉取境界配置列表。
async function loadList() {
  rows.value = await getAdminRealmLevelConfigs()
}

// 拉取单个等级的境界配置详情。
async function loadDetail(level: number) {
  selectedDetail.value = await getAdminRealmLevelConfigDetail(level)
}

// 打开境界详情弹窗。
async function openDetailModal(level: number) {
  try {
    loading.value = true
    selectedLevel.value = level
    await loadDetail(level)
    detailOpen.value = true
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '境界配置读取失败。')
  } finally {
    loading.value = false
  }
}

// 重新加载当前选中的境界详情。
async function reloadSelected() {
  if (selectedLevel.value === null) return
  try {
    loading.value = true
    await loadDetail(selectedLevel.value)
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '境界配置读取失败。')
  } finally {
    loading.value = false
  }
}

// 追加一条突破材料。
function addMaterial() {
  if (!selectedDetail.value) return
  selectedDetail.value.breakthroughMaterials.push({
    itemId: '',
    count: 1
  })
}

// 删除一条突破材料。
function removeMaterial(index: number) {
  selectedDetail.value?.breakthroughMaterials.splice(index, 1)
}

// 保存境界配置。
async function saveSelected() {
  if (!selectedDetail.value || !canManageConfigs.value) return

  try {
    loading.value = true
    const payload: AdminRealmLevelConfigDetail = {
      ...selectedDetail.value,
      realmName: selectedDetail.value.realmName.trim(),
      alias: selectedDetail.value.alias.trim(),
      description: selectedDetail.value.description?.trim() || null,
      breakthroughMaterials: (selectedDetail.value.isBreakthroughPoint ? selectedDetail.value.breakthroughMaterials : [])
        .map((item) => ({
          itemId: item.itemId.trim(),
          count: Number(item.count) || 0
        }))
    }
    const saved = await saveAdminRealmLevelConfig(payload)
    selectedDetail.value = saved
    await loadList()
    detailOpen.value = false
    toast.success('境界配置保存成功。')
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '境界配置保存失败。')
  } finally {
    loading.value = false
  }
}

onMounted(async () => {
  try {
    itemOptions.value = await getAdminItems()
    await loadList()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '境界配置读取失败。')
  }
})
</script>
