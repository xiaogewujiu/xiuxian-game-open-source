<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">灵田</p>
        <h2 class="section-title section-title--small">灵田系统监控</h2>
        <p class="section-note">查看灵田等级、已解锁地块和当前种植状态，支持后台修正地块等级和产量加成。</p>
      </div>

      <div class="filter-grid">
        <label class="field">
          <span>关键字</span>
          <input v-model.trim="keyword" type="text" placeholder="搜索玩家编号、账号或名称" />
        </label>
        <label class="field">
          <span>当前玩家</span>
          <input :value="selectedDetail?.playerId || ''" type="text" placeholder="未选择" disabled />
        </label>
        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadItems">查询</button>
        </div>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">灵田列表</p>
          <h3 class="section-title section-title--small">灵田列表</h3>
        </div>
        <span class="selected-pill">共 {{ items.length }} 条</span>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>玩家</th>
              <th>账号</th>
              <th>灵田等级</th>
              <th>地块</th>
              <th>产量加成</th>
              <th>生长加成</th>
              <th>累计种植</th>
              <th>累计收获</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="item in items"
              :key="item.playerId"
              :class="{ 'is-active': selectedDetail?.playerId === item.playerId }"
              @click="openDetail(item.playerId)"
            >
              <td>{{ item.name }}</td>
              <td>{{ item.account }}</td>
              <td>Lv.{{ item.fieldLevel }}</td>
              <td>{{ item.unlockedPlots }}/{{ item.maxPlots }}</td>
              <td>+{{ item.globalYieldBonus }}%</td>
              <td>+{{ item.globalGrowthSpeedBonus }}%</td>
              <td>{{ item.totalPlantCount }}</td>
              <td>{{ item.totalHarvestCount }}</td>
            </tr>
          </tbody>
        </table>
      </div>

    </section>

    <AdminModal
      v-model="editorOpen"
      kicker="灵田"
      :title="selectedDetail ? `编辑 ${selectedDetail.name} 的灵田` : '灵田详情'"
      description="系统字段可修正，地块当前种植状态只读，等级和额外产量可手工调整。"
      size="xwide"
    >
      <div v-if="selectedDetail" class="section-stack section-stack--spaced">
        <div class="detail-overview detail-overview--compact">
          <article class="detail-overview__item"><span>玩家</span><strong>{{ selectedDetail.name }}</strong></article>
          <article class="detail-overview__item"><span>账号</span><strong>{{ selectedDetail.account }}</strong></article>
          <article class="detail-overview__item"><span>聚灵阵加成</span><strong>+{{ selectedDetail.globalYieldBonus }}%</strong></article>
          <article class="detail-overview__item"><span>今日种植/收获</span><strong>{{ selectedDetail.todayPlantCount }} / {{ selectedDetail.todayHarvestCount }}</strong></article>
        </div>

        <fieldset class="form-fieldset" :disabled="!canWritePlayers">
          <div class="editor-grid editor-grid--three">
            <label class="field"><span>灵田等级</span><input v-model.number="selectedDetail.fieldLevel" type="number" min="1" /></label>
            <label class="field"><span>已解锁地块</span><input v-model.number="selectedDetail.unlockedPlots" type="number" min="1" /></label>
            <label class="field"><span>最大地块数</span><input v-model.number="selectedDetail.maxPlots" type="number" min="1" /></label>
            <label class="field"><span>生长速度加成</span><input v-model.number="selectedDetail.globalGrowthSpeedBonus" type="number" min="0" /></label>
            <label class="field"><span>累计种植</span><input :value="selectedDetail.totalPlantCount" type="number" disabled /></label>
            <label class="field"><span>累计收获</span><input :value="selectedDetail.totalHarvestCount" type="number" disabled /></label>
          </div>
        </fieldset>

        <section class="panel-box panel-box--embedded">
          <div class="section-header-row">
            <div class="section-header-copy">
              <p class="section-kicker">地块</p>
              <h3 class="section-title section-title--small">地块详情</h3>
            </div>
            <span class="selected-pill">共 {{ selectedDetail.plots.length }} 块</span>
          </div>

          <div class="table-wrap">
            <table class="data-table">
              <thead>
                <tr>
                  <th>地块</th>
                  <th>状态</th>
                  <th>作物</th>
                  <th>等级</th>
                  <th>额外产量</th>
                  <th>成熟时间</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="plot in selectedDetail.plots" :key="plot.plotNumber">
                  <td>{{ plot.plotNumber }}</td>
                  <td>{{ getStatusLabel(plot.status) }}</td>
                  <td>{{ plot.cropName || plot.cropTemplateId || '无' }}</td>
                  <td><input v-model.number="plot.level" type="number" min="1" :disabled="!canWritePlayers" /></td>
                  <td><input v-model.number="plot.yieldBonusPercent" type="number" min="0" :disabled="!canWritePlayers" /></td>
                  <td>{{ formatDateTime(plot.expectedHarvestTime) }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </section>
      </div>

      <template #footer>
        <button class="secondary-button" type="button" @click="editorOpen = false">取消</button>
        <button class="primary-button" type="button" :disabled="!canWritePlayers" @click="saveDetail">保存灵田数据</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import { getAdminSpiritFieldDetail, getAdminSpiritFields, saveAdminSpiritField } from '@/services/spirit-fields'
import type { AdminSpiritFieldDetail, AdminSpiritFieldListItem } from '@/types/admin'
import toast from '@/utils/toast'

// 灵田运行态页聚焦玩家实例数据修正，不处理系统规则模板。
const { canWritePlayers } = useAdminPermissions()
const keyword = ref('')
const items = ref<AdminSpiritFieldListItem[]>([])
const selectedDetail = ref<AdminSpiritFieldDetail | null>(null)
const editorOpen = ref(false)

// 统一格式化后台时间字段。
const STATUS_LABEL_MAP: Record<string, string> = { Empty: '空闲', Planting: '种植中', Growing: '生长中', Harvestable: '可收获', Harvested: '已收获', Locked: '未解锁' }

function getStatusLabel(status: string) {
  return STATUS_LABEL_MAP[status] || status
}

function formatDateTime(value?: string | null) {
  if (!value) return '未设置'
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? String(value) : date.toLocaleString()
}

// 拉取灵田运行态列表。
async function loadItems() {
  items.value = await getAdminSpiritFields(keyword.value)
}

// 打开指定玩家的灵田详情。
async function openDetail(playerId: string) {
  selectedDetail.value = await getAdminSpiritFieldDetail(playerId)
  editorOpen.value = true
}

// 保存灵田运行态。
async function saveDetail() {
  if (!canWritePlayers.value || !selectedDetail.value) return

  try {
    selectedDetail.value = await saveAdminSpiritField(selectedDetail.value)
    toast.success('灵田系统数据保存成功。')
    editorOpen.value = false
    await loadItems()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '灵田系统数据保存失败。')
  }
}

onMounted(loadItems)
</script>
