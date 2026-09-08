<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">通天塔</p>
        <h2 class="section-title section-title--small">通天塔管理</h2>
        <p class="section-note">管理通天塔楼层配置、查看玩家进度。</p>
      </div>

      <div class="detail-overview" v-if="overview">
        <article class="detail-overview__item"><span>参与人数</span><strong>{{ overview.totalPlayers }}</strong></article>
        <article class="detail-overview__item"><span>今日挑战</span><strong>{{ overview.todayChallenges }}</strong></article>
        <article class="detail-overview__item"><span>平均最高层</span><strong>{{ overview.averageHighestFloor }}</strong></article>
      </div>

      <div class="tab-bar">
        <button
          v-for="tab in TAB_OPTIONS"
          :key="tab.value"
          class="tab-button"
          :class="{ 'is-active': activeTab === tab.value }"
          type="button"
          @click="activeTab = tab.value"
        >{{ tab.label }}</button>
      </div>
    </section>

    <!-- Tab 1: 楼层配置 -->
    <template v-if="activeTab === 'floors'">
      <section class="panel-box filter-panel">
        <div class="section-header-copy">
          <p class="section-kicker">楼层配置</p>
          <h3 class="section-title section-title--small">楼层配置管理</h3>
        </div>
        <div class="filter-actions">
          <button class="primary-button" type="button" @click="openBatchGenerate">批量生成</button>
        </div>
      </section>

      <section class="panel-box table-panel">
        <div class="section-header-row">
          <div class="section-header-copy">
            <p class="section-kicker">楼层列表</p>
            <h3 class="section-title section-title--small">楼层配置列表</h3>
          </div>
          <span class="selected-pill">共 {{ floorConfigs.length }} 层</span>
        </div>
        <div class="table-wrap">
          <table class="data-table">
            <thead>
              <tr>
                <th>层数</th>
                <th>怪物数</th>
                <th>属性倍率</th>
                <th>金币奖励</th>
                <th>经验奖励</th>
                <th>里程碑</th>
                <th>操作</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="f in floorConfigs" :key="f.id">
                <td>第{{ f.floor }}层</td>
                <td>{{ f.monsterCount }}</td>
                <td>{{ f.statMultiplier.toFixed(2) }}</td>
                <td>{{ f.rewardGold }}</td>
                <td>{{ f.rewardExp }}</td>
                <td>{{ f.milestoneRewardJson ? '是' : '-' }}</td>
                <td>
                  <button class="secondary-button secondary-button--sm" type="button" @click="openEditFloor(f)">编辑</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </template>

    <!-- Tab 2: 玩家进度 -->
    <template v-if="activeTab === 'players'">
      <section class="panel-box filter-panel">
        <div class="section-header-copy">
          <p class="section-kicker">玩家查询</p>
          <h3 class="section-title section-title--small">玩家进度</h3>
        </div>
        <div class="filter-grid">
          <label class="field">
            <span>关键字</span>
            <input v-model.trim="playerKeyword" type="text" placeholder="搜索玩家ID或名称" />
          </label>
          <div class="filter-actions">
            <button class="secondary-button" type="button" @click="loadPlayers">查询</button>
          </div>
        </div>
      </section>

      <section class="panel-box table-panel">
        <div class="section-header-row">
          <div class="section-header-copy">
            <p class="section-kicker">玩家列表</p>
            <h3 class="section-title section-title--small">玩家通天塔进度</h3>
          </div>
          <span class="selected-pill">共 {{ players.length }} 人</span>
        </div>
        <div class="table-wrap">
          <table class="data-table">
            <thead>
              <tr>
                <th>玩家</th>
                <th>最高层</th>
                <th>当前层</th>
                <th>今日失败</th>
                <th>最后挑战</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="p in players" :key="p.id">
                <td>{{ p.playerName }} ({{ p.playerId }})</td>
                <td>第{{ p.highestFloor }}层</td>
                <td>第{{ p.currentFloor }}层</td>
                <td>{{ p.dailyAttemptsUsed }}</td>
                <td>{{ formatDateTime(p.lastAttemptAt) }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </template>

    <!-- Tab 3: 层数分布 -->
    <template v-if="activeTab === 'distribution'">
      <section class="panel-box filter-panel">
        <div class="section-header-copy">
          <p class="section-kicker">数据分析</p>
          <h3 class="section-title section-title--small">玩家层数分布</h3>
          <p class="section-note">显示各最高通关层的玩家数量分布。</p>
        </div>
        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadDistribution">刷新</button>
        </div>
      </section>

      <section class="panel-box table-panel">
        <div class="section-header-row">
          <div class="section-header-copy">
            <p class="section-kicker">分布图</p>
            <h3 class="section-title section-title--small">层数分布柱状图</h3>
          </div>
          <span class="selected-pill">共 {{ distribution.length }} 个层段</span>
        </div>
        <div class="distribution-chart" v-if="distribution.length > 0">
          <div class="chart-bar-row" v-for="d in distribution" :key="d.floor">
            <span class="chart-label">第{{ d.floor }}层</span>
            <div class="chart-bar-track">
              <div
                class="chart-bar-fill"
                :style="{ width: (d.playerCount / maxDistribution * 100) + '%' }"
              ></div>
            </div>
            <span class="chart-value">{{ d.playerCount }}人</span>
          </div>
        </div>
        <div v-else class="empty-state">暂无数据</div>
      </section>
    </template>

    <!-- 编辑楼层弹窗 -->
    <AdminModal
      v-model="editModalOpen"
      kicker="楼层配置"
      :title="editingFloor ? `编辑第${editingFloor.floor}层` : '编辑楼层'"
      description="修改楼层的怪物、倍率和奖励配置。"
    >
      <div v-if="editingFloor" class="editor-grid editor-grid--three">
        <label class="field"><span>怪物数量</span><input v-model.number="editingFloor.monsterCount" type="number" min="1" /></label>
        <label class="field"><span>属性倍率</span><input v-model.number="editingFloor.statMultiplier" type="number" step="0.01" min="0.1" /></label>
        <label class="field"><span>金币奖励</span><input v-model.number="editingFloor.rewardGold" type="number" min="0" /></label>
        <label class="field"><span>经验奖励</span><input v-model.number="editingFloor.rewardExp" type="number" min="0" /></label>
      </div>
      <div class="section-header-copy" style="margin-top: 0.5rem;">
        <h4 class="section-title section-title--small">怪物选择</h4>
      </div>
      <div class="monster-select-grid">
        <label v-for="m in monsterOptions" :key="m.monsterId" class="checkbox-label">
          <input type="checkbox" :value="m.monsterId" v-model="editSelectedMonsterIds" />
          <span>{{ m.name }} (Lv.{{ m.level }})</span>
        </label>
      </div>
      <div class="section-header-copy" style="margin-top: 0.5rem;">
        <h4 class="section-title section-title--small">里程碑奖励（每10层）</h4>
      </div>
      <div class="editor-grid editor-grid--three">
        <label class="field"><span>金币</span><input v-model.number="editMilestone.gold" type="number" min="0" /></label>
        <label class="field"><span>灵石</span><input v-model.number="editMilestone.spiritStone" type="number" min="0" /></label>
      </div>
      <template #footer>
        <button class="secondary-button" type="button" @click="editModalOpen = false">取消</button>
        <button class="primary-button" type="button" @click="saveFloor">保存</button>
      </template>
    </AdminModal>

    <!-- 批量生成弹窗 -->
    <AdminModal
      v-model="batchModalOpen"
      kicker="批量生成"
      title="批量生成楼层配置"
      description="按规则批量生成100层配置。已存在的楼层会被跳过。"
    >
      <div class="section-header-copy" style="margin-bottom: 0.5rem;">
        <h4 class="section-title section-title--small">怪物选择</h4>
      </div>
      <div class="monster-select-grid">
        <label v-for="m in monsterOptions" :key="m.monsterId" class="checkbox-label">
          <input type="checkbox" :value="m.monsterId" v-model="batchSelectedMonsterIds" />
          <span>{{ m.name }} (Lv.{{ m.level }})</span>
        </label>
      </div>
      <div class="editor-grid editor-grid--three" style="margin-top: 0.5rem;">
        <label class="field"><span>每层怪物数</span><input v-model.number="batchForm.monsterCountPerFloor" type="number" min="1" /></label>
        <label class="field"><span>基础倍率</span><input v-model.number="batchForm.baseMultiplier" type="number" step="0.01" /></label>
        <label class="field"><span>每层倍率增长</span><input v-model.number="batchForm.multiplierGrowth" type="number" step="0.01" /></label>
        <label class="field"><span>基础金币奖励</span><input v-model.number="batchForm.baseRewardGold" type="number" /></label>
        <label class="field"><span>基础经验奖励</span><input v-model.number="batchForm.baseRewardExp" type="number" /></label>
      </div>
      <template #footer>
        <button class="secondary-button" type="button" @click="batchModalOpen = false">取消</button>
        <button class="primary-button" type="button" @click="doBatchGenerate">生成</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, reactive, computed } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import toast from '@/utils/toast'
import { getTowerOverview, getTowerPlayers, getTowerFloorConfigs, updateTowerFloorConfig, batchGenerateTowerFloors, getTowerFloorDistribution } from '@/services/tower'
import { getAdminMonsters } from '@/services/monsters'
import type { AdminTowerOverview, AdminTowerPlayer, AdminTowerFloorConfig, AdminMonsterListItem, TowerFloorDistribution } from '@/types/admin'

const TAB_OPTIONS = [
  { value: 'floors', label: '楼层配置' },
  { value: 'players', label: '玩家进度' },
  { value: 'distribution', label: '层数分布' }
] as const

type TabValue = (typeof TAB_OPTIONS)[number]['value']

const activeTab = ref<TabValue>('floors')
const overview = ref<AdminTowerOverview | null>(null)

// ===== 楼层配置 =====
const floorConfigs = ref<AdminTowerFloorConfig[]>([])
const editModalOpen = ref(false)
const editingFloor = ref<AdminTowerFloorConfig | null>(null)
const monsterOptions = ref<AdminMonsterListItem[]>([])
const editSelectedMonsterIds = ref<string[]>([])
const editMilestone = reactive({ gold: 0, spiritStone: 0 })

// ===== 玩家进度 =====
const playerKeyword = ref('')
const players = ref<AdminTowerPlayer[]>([])

// ===== 批量生成 =====
const batchModalOpen = ref(false)
const batchSelectedMonsterIds = ref<string[]>([])
const batchForm = reactive({
  monsterTemplateIdsJson: '[]',
  monsterCountPerFloor: 2,
  baseMultiplier: 1.0,
  multiplierGrowth: 0.05,
  baseRewardGold: 100,
  baseRewardExp: 50
})

// ===== 层数分布 =====
const distribution = ref<TowerFloorDistribution[]>([])
const maxDistribution = computed(() => Math.max(1, ...distribution.value.map(d => d.playerCount)))

function formatDateTime(value?: string | null) {
  if (!value) return '-'
  return value.replace('T', ' ').slice(0, 19)
}

async function loadOverview() {
  overview.value = await getTowerOverview()
}

async function loadFloorConfigs() {
  floorConfigs.value = await getTowerFloorConfigs()
}

async function loadPlayers() {
  players.value = await getTowerPlayers(playerKeyword.value)
}

function openEditFloor(floor: AdminTowerFloorConfig) {
  editingFloor.value = { ...floor }
  try {
    editSelectedMonsterIds.value = JSON.parse(floor.monsterTemplateIdsJson || '[]')
  } catch { editSelectedMonsterIds.value = [] }
  try {
    const mr = floor.milestoneRewardJson ? JSON.parse(floor.milestoneRewardJson) : {}
    editMilestone.gold = mr.gold || 0
    editMilestone.spiritStone = mr.spiritStone || 0
  } catch { editMilestone.gold = 0; editMilestone.spiritStone = 0 }
  editModalOpen.value = true
}

async function saveFloor() {
  if (!editingFloor.value) return
  try {
    editingFloor.value.monsterTemplateIdsJson = JSON.stringify(editSelectedMonsterIds.value)
    const hasMilestone = editMilestone.gold > 0 || editMilestone.spiritStone > 0
    editingFloor.value.milestoneRewardJson = hasMilestone
      ? JSON.stringify({ gold: editMilestone.gold, spiritStone: editMilestone.spiritStone })
      : null
    await updateTowerFloorConfig(editingFloor.value.id, editingFloor.value)
    toast.success('楼层配置已更新。')
    editModalOpen.value = false
    await loadFloorConfigs()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '保存失败。')
  }
}

function openBatchGenerate() {
  batchModalOpen.value = true
}

async function doBatchGenerate() {
  try {
    batchForm.monsterTemplateIdsJson = JSON.stringify(batchSelectedMonsterIds.value)
    const count = await batchGenerateTowerFloors(batchForm)
    toast.success(`生成了 ${count} 层配置。`)
    batchModalOpen.value = false
    await loadFloorConfigs()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '生成失败。')
  }
}

async function loadDistribution() {
  distribution.value = await getTowerFloorDistribution()
}

onMounted(async () => {
  await Promise.all([loadOverview(), loadFloorConfigs(), loadDistribution()])
  monsterOptions.value = await getAdminMonsters()
})
</script>

<style scoped>
.tab-bar {
  display: flex;
  gap: 0.5rem;
  margin-top: 1rem;
  flex-wrap: wrap;
}

.tab-button {
  padding: 0.5rem 1rem;
  border: 1px solid var(--border-color, #e2e8f0);
  border-radius: 6px;
  background: transparent;
  cursor: pointer;
  font-size: 0.875rem;
  color: var(--text-secondary, #64748b);
  transition: all 0.15s ease;
}

.tab-button.is-active {
  background: var(--primary-color, #3b82f6);
  color: #fff;
  border-color: var(--primary-color, #3b82f6);
}

.tab-button:hover:not(.is-active) {
  background: var(--bg-hover, #f1f5f9);
}

.secondary-button--sm {
  padding: 0.25rem 0.5rem;
  font-size: 0.75rem;
}

.monster-select-grid {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  max-height: 200px;
  overflow-y: auto;
  padding: 0.5rem;
  border: 1px solid var(--border-color, #e2e8f0);
  border-radius: 6px;
}

.checkbox-label {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  font-size: 0.85rem;
  cursor: pointer;
}

.checkbox-label input[type="checkbox"] {
  width: auto;
}

.distribution-chart {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  padding: 0.5rem 0;
}

.chart-bar-row {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.chart-label {
  min-width: 60px;
  font-size: 0.8rem;
  text-align: right;
  color: var(--text-secondary, #64748b);
}

.chart-bar-track {
  flex: 1;
  height: 18px;
  background: var(--bg-hover, #f1f5f9);
  border-radius: 4px;
  overflow: hidden;
}

.chart-bar-fill {
  height: 100%;
  background: var(--primary-color, #3b82f6);
  border-radius: 4px;
  min-width: 2px;
  transition: width 0.3s ease;
}

.chart-value {
  min-width: 50px;
  font-size: 0.8rem;
  color: var(--text-secondary, #64748b);
}

.empty-state {
  text-align: center;
  padding: 2rem;
  color: var(--text-secondary, #94a3b8);
  font-size: 0.9rem;
}
</style>
