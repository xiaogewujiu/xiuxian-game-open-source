<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">好感度</p>
        <h2 class="section-title section-title--small">好感度管理</h2>
        <p class="section-note">管理玩家间好感度关系、赠送记录、等级配置和排行榜。</p>
      </div>

      <div class="detail-overview" v-if="stats">
        <article class="detail-overview__item"><span>关系总数</span><strong>{{ stats.totalRelations }}</strong></article>
        <article class="detail-overview__item"><span>累计赠送</span><strong>{{ stats.totalGiftsAll }}</strong></article>
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

    <!-- Tab 1: 好感度关系 -->
    <template v-if="activeTab === 'relations'">
      <section class="panel-box filter-panel">
        <div class="section-header-copy">
          <p class="section-kicker">关系查询</p>
          <h3 class="section-title section-title--small">好感度关系</h3>
        </div>
        <div class="filter-grid">
          <label class="field">
            <span>关键字</span>
            <input v-model.trim="relationKeyword" type="text" placeholder="搜索玩家编号或名称" />
          </label>
          <div class="filter-actions">
            <button class="secondary-button" type="button" @click="loadRelations(1)">查询</button>
          </div>
        </div>
      </section>

      <section class="panel-box table-panel">
        <div class="section-header-row">
          <div class="section-header-copy">
            <p class="section-kicker">关系列表</p>
            <h3 class="section-title section-title--small">好感度关系列表</h3>
          </div>
          <span class="selected-pill">共 {{ relationTotal }} 条</span>
        </div>
        <div class="table-wrap">
          <table class="data-table">
            <thead>
              <tr>
                <th>玩家</th>
                <th>目标玩家</th>
                <th>好感度</th>
                <th>最后赠送</th>
                <th>建立时间</th>
                <th>操作</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="rel in relations" :key="rel.id">
                <td>{{ rel.playerName }} ({{ rel.playerId }})</td>
                <td>{{ rel.targetPlayerName }} ({{ rel.targetPlayerId }})</td>
                <td>{{ rel.value }}</td>
                <td>{{ formatDateTime(rel.lastGiftTime) }}</td>
                <td>{{ formatDateTime(rel.createdTime) }}</td>
                <td>
                  <button class="secondary-button secondary-button--sm" type="button" @click="openEditRelation(rel)">编辑</button>
                  <button class="danger-button danger-button--sm" type="button" @click="removeRelation(rel)">删除</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
        <div class="pagination-bar" v-if="relationTotalPages > 1">
          <button class="secondary-button" type="button" :disabled="relationPage <= 1" @click="loadRelations(relationPage - 1)">上一页</button>
          <span>第 {{ relationPage }} / {{ relationTotalPages }} 页</span>
          <button class="secondary-button" type="button" :disabled="relationPage >= relationTotalPages" @click="loadRelations(relationPage + 1)">下一页</button>
        </div>
      </section>
    </template>

    <!-- Tab 2: 赠送记录 -->
    <template v-if="activeTab === 'gift-log'">
      <section class="panel-box filter-panel">
        <div class="section-header-copy">
          <p class="section-kicker">记录查询</p>
          <h3 class="section-title section-title--small">赠送记录</h3>
        </div>
        <div class="filter-grid">
          <label class="field">
            <span>赠送方玩家ID</span>
            <input v-model.trim="giftLogPlayerId" type="text" placeholder="输入玩家ID" />
          </label>
          <label class="field">
            <span>接收方玩家ID</span>
            <input v-model.trim="giftLogTargetPlayerId" type="text" placeholder="输入目标玩家ID" />
          </label>
          <div class="filter-actions">
            <button class="secondary-button" type="button" @click="loadGiftLog(1)">查询</button>
          </div>
        </div>
      </section>

      <section class="panel-box table-panel">
        <div class="section-header-row">
          <div class="section-header-copy">
            <p class="section-kicker">赠送记录</p>
            <h3 class="section-title section-title--small">赠送记录列表</h3>
          </div>
          <span class="selected-pill">共 {{ giftLogTotal }} 条</span>
        </div>
        <div class="table-wrap">
          <table class="data-table">
            <thead>
              <tr>
                <th>赠送方</th>
                <th>接收方</th>
                <th>道具</th>
                <th>好感度变化</th>
                <th>赠送时间</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="log in giftLogs" :key="log.id">
                <td>{{ log.playerName }} ({{ log.playerId }})</td>
                <td>{{ log.targetPlayerName }} ({{ log.targetPlayerId }})</td>
                <td>{{ log.itemName }} ({{ log.itemId }})</td>
                <td>{{ log.favorabilityChange > 0 ? '+' : '' }}{{ log.favorabilityChange }}</td>
                <td>{{ formatDateTime(log.giftTime) }}</td>
              </tr>
            </tbody>
          </table>
        </div>
        <div class="pagination-bar" v-if="giftLogTotalPages > 1">
          <button class="secondary-button" type="button" :disabled="giftLogPage <= 1" @click="loadGiftLog(giftLogPage - 1)">上一页</button>
          <span>第 {{ giftLogPage }} / {{ giftLogTotalPages }} 页</span>
          <button class="secondary-button" type="button" :disabled="giftLogPage >= giftLogTotalPages" @click="loadGiftLog(giftLogPage + 1)">下一页</button>
        </div>
      </section>
    </template>

    <!-- Tab 3: 等级配置 -->
    <template v-if="activeTab === 'levels'">
      <section class="panel-box filter-panel">
        <div class="section-header-copy">
          <p class="section-kicker">等级配置</p>
          <h3 class="section-title section-title--small">好感度等级配置</h3>
        </div>
        <div class="filter-actions">
          <button class="primary-button" type="button" @click="openCreateLevel">新建等级</button>
        </div>
      </section>

      <section class="panel-box table-panel">
        <div class="section-header-row">
          <div class="section-header-copy">
            <p class="section-kicker">等级列表</p>
            <h3 class="section-title section-title--small">等级配置列表</h3>
          </div>
          <span class="selected-pill">共 {{ levels.length }} 条</span>
        </div>
        <div class="table-wrap">
          <table class="data-table">
            <thead>
              <tr>
                <th>等级</th>
                <th>名称</th>
                <th>好感度区间</th>
                <th>颜色</th>
                <th>排序</th>
                <th>来源</th>
                <th>操作</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="lv in levels" :key="lv.id">
                <td>Lv.{{ lv.level }}</td>
                <td>{{ lv.name }}</td>
                <td>{{ lv.minValue }} ~ {{ lv.maxValue }}</td>
                <td><span :style="{ color: lv.color }">{{ lv.color }}</span></td>
                <td>{{ lv.sortOrder }}</td>
                <td>{{ lv.isBuiltIn ? '内置' : '人工' }}</td>
                <td>
                  <button class="secondary-button secondary-button--sm" type="button" @click="openEditLevel(lv)">编辑</button>
                  <button class="danger-button danger-button--sm" type="button" @click="removeLevel(lv)">删除</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </template>

    <!-- Tab 4: 好感度排行榜 -->
    <template v-if="activeTab === 'leaderboard'">
      <section class="panel-box table-panel">
        <div class="section-header-row">
          <div class="section-header-copy">
            <p class="section-kicker">排行榜</p>
            <h3 class="section-title section-title--small">好感度排行榜（被好感最多）</h3>
          </div>
        </div>
        <div class="table-wrap">
          <table class="data-table">
            <thead>
              <tr>
                <th>排名</th>
                <th>玩家</th>
                <th>收到好感度总计</th>
                <th>关系数</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="entry in leaderboard" :key="entry.playerId">
                <td>{{ entry.rank }}</td>
                <td>{{ entry.playerName }} ({{ entry.playerId }})</td>
                <td>{{ entry.totalFavorability }}</td>
                <td>{{ entry.relationCount }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </template>

    <!-- Tab 5: 深度友谊 -->
    <template v-if="activeTab === 'deep-friendship'">
      <section class="panel-box table-panel">
        <div class="section-header-row">
          <div class="section-header-copy">
            <p class="section-kicker">深度友谊</p>
            <h3 class="section-title section-title--small">双向好感度排行</h3>
          </div>
        </div>
        <div class="table-wrap">
          <table class="data-table">
            <thead>
              <tr>
                <th>排名</th>
                <th>玩家A</th>
                <th>玩家B</th>
                <th>双向好感度合计</th>
                <th>等级</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="entry in deepFriendships" :key="`${entry.player1Id}-${entry.player2Id}`">
                <td>{{ entry.rank }}</td>
                <td>{{ entry.player1Name }} ({{ entry.player1Id }})</td>
                <td>{{ entry.player2Name }} ({{ entry.player2Id }})</td>
                <td>{{ entry.combinedValue }}</td>
                <td><span :style="{ color: entry.levelColor }">{{ entry.levelName }}</span></td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </template>

    <!-- 编辑好感度弹窗 -->
    <AdminModal
      v-model="editRelationOpen"
      kicker="好感度编辑"
      :title="editingRelation ? `编辑关系 ${editingRelation.playerName} → ${editingRelation.targetPlayerName}` : '编辑好感度'"
      description="修改玩家间的好感度数值。"
    >
      <div v-if="editingRelation" class="editor-grid">
        <label class="field">
          <span>当前好感度</span>
          <input :value="String(editingRelation.value)" type="text" disabled />
        </label>
        <label class="field">
          <span>新好感度值</span>
          <input v-model.number="editRelationNewValue" type="number" />
        </label>
      </div>
      <template #footer>
        <button class="secondary-button" type="button" @click="editRelationOpen = false">取消</button>
        <button class="primary-button" type="button" @click="saveRelation">保存</button>
      </template>
    </AdminModal>

    <!-- 新建/编辑等级弹窗 -->
    <AdminModal
      v-model="levelModalOpen"
      kicker="等级配置"
      :title="editingLevelId ? '编辑好感度等级' : '新建好感度等级'"
      description="配置好感度等级所需数值、名称、颜色和说明。"
    >
      <div v-if="levelForm" class="editor-grid editor-grid--three">
        <label class="field"><span>等级</span><input v-model.number="levelForm.level" type="number" min="1" /></label>
        <label class="field"><span>名称</span><input v-model.trim="levelForm.name" type="text" /></label>
        <label class="field"><span>最低好感度</span><input v-model.number="levelForm.minValue" type="number" /></label>
        <label class="field"><span>最高好感度</span><input v-model.number="levelForm.maxValue" type="number" /></label>
        <label class="field"><span>颜色</span><input v-model.trim="levelForm.color" type="text" placeholder="#4CAF50" /></label>
        <label class="field"><span>排序</span><input v-model.number="levelForm.sortOrder" type="number" min="0" /></label>
        <label class="field"><span>说明</span><input v-model.trim="levelForm.description" type="text" /></label>
      </div>
      <template #footer>
        <button class="secondary-button" type="button" @click="levelModalOpen = false">取消</button>
        <button class="primary-button" type="button" @click="saveLevel">保存</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import toast from '@/utils/toast'
import {
  getFavorabilityRelations,
  updateFavorabilityRelation,
  deleteFavorabilityRelation,
  getFavorabilityGiftLog,
  getFavorabilityStats,
  getFavorabilityLevels,
  saveFavorabilityLevel,
  deleteFavorabilityLevel,
  getFavorabilityLeaderboard,
  getDeepFriendshipLeaderboard
} from '@/services/favorability'
import type {
  AdminFavorabilityRelation,
  AdminFavorabilityGiftLog,
  AdminFavorabilityLevelConfig,
  AdminFavorabilityStats,
  AdminFavorabilityLeaderboard,
  AdminDeepFriendship
} from '@/types/admin'

const TAB_OPTIONS = [
  { value: 'relations', label: '好感度关系' },
  { value: 'gift-log', label: '赠送记录' },
  { value: 'levels', label: '等级配置' },
  { value: 'leaderboard', label: '排行榜' },
  { value: 'deep-friendship', label: '深度友谊' }
] as const

type TabValue = (typeof TAB_OPTIONS)[number]['value']

const activeTab = ref<TabValue>('relations')
const stats = ref<AdminFavorabilityStats | null>(null)

// ===== 好感度关系 =====
const relationKeyword = ref('')
const relations = ref<AdminFavorabilityRelation[]>([])
const relationTotal = ref(0)
const relationPage = ref(1)
const relationPageSize = 20
const relationTotalPages = computed(() => Math.ceil(relationTotal.value / relationPageSize))

const editRelationOpen = ref(false)
const editingRelation = ref<AdminFavorabilityRelation | null>(null)
const editRelationNewValue = ref(0)

// ===== 赠送记录 =====
const giftLogPlayerId = ref('')
const giftLogTargetPlayerId = ref('')
const giftLogs = ref<AdminFavorabilityGiftLog[]>([])
const giftLogTotal = ref(0)
const giftLogPage = ref(1)
const giftLogPageSize = 20
const giftLogTotalPages = computed(() => Math.ceil(giftLogTotal.value / giftLogPageSize))

// ===== 等级配置 =====
const levels = ref<AdminFavorabilityLevelConfig[]>([])
const levelModalOpen = ref(false)
const editingLevelId = ref<number | null>(null)
const levelForm = ref<Partial<AdminFavorabilityLevelConfig> | null>(null)

// ===== 排行榜 =====
const leaderboard = ref<AdminFavorabilityLeaderboard[]>([])
const deepFriendships = ref<AdminDeepFriendship[]>([])

function formatDateTime(value?: string | null) {
  if (!value) return '-'
  return value.replace('T', ' ').slice(0, 19)
}

async function loadStats() {
  stats.value = await getFavorabilityStats()
}

async function loadRelations(page: number) {
  relationPage.value = page
  const result = await getFavorabilityRelations(relationKeyword.value, relationPage.value, relationPageSize)
  relations.value = result.items
  relationTotal.value = result.total
}

async function loadGiftLog(page: number) {
  giftLogPage.value = page
  const result = await getFavorabilityGiftLog(giftLogPlayerId.value, giftLogTargetPlayerId.value, giftLogPage.value, giftLogPageSize)
  giftLogs.value = result.items
  giftLogTotal.value = result.total
}

async function loadLevels() {
  levels.value = await getFavorabilityLevels()
}

async function loadLeaderboard() {
  leaderboard.value = await getFavorabilityLeaderboard(20)
}

async function loadDeepFriendship() {
  deepFriendships.value = await getDeepFriendshipLeaderboard(10)
}

function openEditRelation(rel: AdminFavorabilityRelation) {
  editingRelation.value = { ...rel }
  editRelationNewValue.value = rel.value
  editRelationOpen.value = true
}

async function saveRelation() {
  if (!editingRelation.value) return
  try {
    await updateFavorabilityRelation(editingRelation.value.id, editRelationNewValue.value)
    toast.success('好感度更新成功。')
    editRelationOpen.value = false
    await Promise.all([loadRelations(relationPage.value), loadStats()])
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '好感度更新失败。')
  }
}

async function removeRelation(rel: AdminFavorabilityRelation) {
  if (!window.confirm(`确认删除 ${rel.playerName} 与 ${rel.targetPlayerName} 之间的好感度关系吗？`)) return
  try {
    await deleteFavorabilityRelation(rel.id)
    toast.success('好感度关系删除成功。')
    await Promise.all([loadRelations(relationPage.value), loadStats()])
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '删除失败。')
  }
}

function openCreateLevel() {
  editingLevelId.value = null
  levelForm.value = { level: 1, minValue: 0, maxValue: 99, name: '', color: '#CCCCCC', sortOrder: 0, description: '' }
  levelModalOpen.value = true
}

function openEditLevel(lv: AdminFavorabilityLevelConfig) {
  editingLevelId.value = Number(lv.id)
  levelForm.value = { ...lv }
  levelModalOpen.value = true
}

async function saveLevel() {
  if (!levelForm.value) return
  try {
    await saveFavorabilityLevel(levelForm.value)
    toast.success('等级配置保存成功。')
    levelModalOpen.value = false
    await loadLevels()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '保存失败。')
  }
}

async function removeLevel(lv: AdminFavorabilityLevelConfig) {
  if (!window.confirm(`确认删除等级 Lv.${lv.level}（${lv.name}）吗？`)) return
  try {
    await deleteFavorabilityLevel(lv.id)
    toast.success('等级配置删除成功。')
    await loadLevels()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '删除失败。')
  }
}

onMounted(async () => {
  await Promise.all([loadStats(), loadRelations(1)])
})

watch(activeTab, (tab) => {
  if (tab === 'levels' && levels.value.length === 0) loadLevels()
  if (tab === 'gift-log' && giftLogs.value.length === 0) loadGiftLog(1)
  if (tab === 'leaderboard' && leaderboard.value.length === 0) loadLeaderboard()
  if (tab === 'deep-friendship' && deepFriendships.value.length === 0) loadDeepFriendship()
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

.pagination-bar {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 1rem;
  padding: 1rem 0;
}

.secondary-button--sm,
.danger-button--sm {
  padding: 0.25rem 0.5rem;
  font-size: 0.75rem;
}
</style>
