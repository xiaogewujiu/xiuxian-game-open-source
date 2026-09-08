<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">世界Boss</p>
        <h2 class="section-title section-title--small">世界 Boss</h2>
        <p class="section-note">维护 Boss 模板、每日刷新时间，并查看当前运行中的 Boss 状态。</p>
      </div>

      <div class="filter-grid">
        <label class="field">
          <span>当前模板</span>
          <input :value="selectedTemplate?.bossId || ''" type="text" placeholder="未选择" disabled />
        </label>
        <label class="field">
          <span>关联怪物</span>
          <input :value="selectedTemplate?.monsterTemplateId || ''" type="text" placeholder="未选择" disabled />
        </label>
        <label class="field">
          <span>运行状态</span>
          <input :value="runtime.current?.hasActiveBoss ? '运行中' : '空闲'" type="text" disabled />
        </label>
        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadAll">刷新</button>
          <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="openCreateModal">新建模板</button>
        </div>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">首领模板</p>
          <h3 class="section-title section-title--small">模板列表</h3>
        </div>
        <span class="selected-pill">共 {{ templates.length }} 条</span>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>编号</th>
              <th>名称</th>
              <th>怪物模板</th>
              <th>权重</th>
              <th>持续</th>
              <th>状态</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="item in templates"
              :key="item.bossId"
              :class="{ 'is-active': selectedTemplate?.bossId === item.bossId }"
              @click="openEditModal(item.bossId)"
            >
              <td>{{ item.bossId }}</td>
              <td>{{ item.name }}</td>
              <td>{{ item.monsterTemplateId }}</td>
              <td>{{ item.weight }}</td>
              <td>{{ item.durationMinutes }} 分钟</td>
              <td>{{ item.isEnabled ? '启用' : '停用' }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">每日排期</p>
          <h3 class="section-title section-title--small">每日排期</h3>
        </div>
      </div>

      <div class="editor-grid editor-grid--three">
        <label class="field"><span>刷新时间</span><input v-model.trim="schedule.spawnTimeText" type="text" placeholder="11:00" /></label>
        <label class="field"><span>时区</span><input v-model.trim="schedule.timeZoneId" type="text" /></label>
        <label class="field"><span>选择模式</span>
          <select v-model.number="schedule.selectionMode">
            <option :value="0">随机</option>
            <option :value="1">轮换</option>
          </select>
        </label>
      </div>
      <div class="filter-actions">
        <button class="secondary-button" type="button" @click="loadSchedule">重载排期</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="saveScheduleConfig">保存排期</button>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">运行时</p>
          <h3 class="section-title section-title--small">运行时</h3>
        </div>
      </div>

      <div class="detail-overview">
        <article class="detail-overview__item"><span>当前Boss</span><strong>{{ runtime.current?.instance?.bossName || '-' }}</strong></article>
        <article class="detail-overview__item"><span>实例状态</span><strong>{{ runtime.current?.instance?.state || '-' }}</strong></article>
        <article class="detail-overview__item"><span>剩余时间</span><strong>{{ runtime.current?.instance?.remainingSeconds || 0 }} 秒</strong></article>
        <article class="detail-overview__item"><span>参与人数</span><strong>{{ runtime.current?.instance?.participantCount || 0 }}</strong></article>
        <article class="detail-overview__item"><span>Boss 血量</span><strong>{{ runtime.current?.boss ? `${runtime.current.boss.currentHp}/${runtime.current.boss.maxHp}` : '-' }}</strong></article>
      </div>

      <div class="filter-actions">
        <button class="secondary-button" type="button" @click="loadRuntime">刷新运行时</button>
        <button class="secondary-button" type="button" :disabled="!canManageConfigs" @click="spawnRandomBoss">随机生成</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs || !selectedTemplate?.bossId" @click="spawnSelectedBoss">生成当前模板</button>
        <button class="danger-button" type="button" :disabled="!canManageConfigs || !runtime.current?.hasActiveBoss" @click="closeCurrentBoss">关闭当前Boss</button>
      </div>

      <div class="runtime-columns">
        <div class="runtime-column">
          <h4 class="subheading">伤害前十</h4>
          <div class="table-wrap">
            <table class="data-table">
              <thead>
                <tr>
                  <th>排名</th>
                  <th>玩家</th>
                  <th>伤害</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="item in runtime.rankingTop10" :key="item.playerId">
                  <td>{{ item.rank }}</td>
                  <td>{{ item.playerName }}</td>
                  <td>{{ item.totalDamage }}</td>
                </tr>
                <tr v-if="runtime.rankingTop10.length === 0">
                  <td colspan="3">暂无排行数据</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <div class="runtime-column">
          <h4 class="subheading">最近日志</h4>
          <div class="runtime-log-list">
            <div v-for="log in runtime.recentLogs" :key="`${log.seq}-${log.timestampUtc}`" class="runtime-log-item">
              <span class="runtime-log-time">{{ formatDateTime(log.timestampUtc) }}</span>
              <span class="runtime-log-text">{{ log.content }}</span>
            </div>
            <div v-if="runtime.recentLogs.length === 0" class="runtime-log-empty">暂无日志</div>
          </div>
        </div>
      </div>

    </section>

    <AdminModal
      v-model="editorOpen"
      kicker="首领编辑"
      :title="selectedTemplate?.bossId ? `编辑模板 ${selectedTemplate.bossId}` : '新建世界Boss模板'"
      description="绑定怪物模板、配置开放时长和参与/排行奖励。"
      size="xwide"
    >
      <fieldset v-if="selectedTemplate" class="form-fieldset" :disabled="!canManageConfigs">
        <div class="editor-grid editor-grid--three">
          <label class="field"><span>Boss编号</span><input v-model.trim="selectedTemplate.bossId" type="text" /></label>
          <label class="field"><span>Boss名称</span><input v-model.trim="selectedTemplate.name" type="text" /></label>
          <label class="field"><span>怪物模板</span>
            <select v-model="selectedTemplate.monsterTemplateId">
              <option value="">请选择</option>
              <option v-for="monster in monsters" :key="monster.monsterId" :value="monster.monsterId">
                {{ monster.name }} ({{ monster.monsterId }})
              </option>
            </select>
          </label>
          <label class="field"><span>权重</span><input v-model.number="selectedTemplate.weight" type="number" min="1" /></label>
          <label class="field"><span>持续分钟</span><input v-model.number="selectedTemplate.durationMinutes" type="number" min="1" /></label>
          <label class="field"><span>排序</span><input v-model.number="selectedTemplate.sortOrder" type="number" min="0" /></label>
          <label class="field field--checkbox"><span>启用</span><input v-model="selectedTemplate.isEnabled" type="checkbox" /></label>
          <label class="field field--full"><span>公告文案</span><textarea v-model.trim="selectedTemplate.noticeText" rows="3"></textarea></label>
        </div>

        <AdminImageField
          v-model="selectedTemplate.portraitPath"
          label="Boss画像"
          upload-category="world-boss"
          placeholder="未上传时保持为空"
        />

        <div class="section-split">
          <div>
            <h4 class="subheading">参与奖励</h4>
            <div class="editor-grid editor-grid--three">
              <label class="field"><span>最低伤害</span><input v-model.number="selectedTemplate.participationMinDamage" type="number" min="0" /></label>
              <label class="field"><span>经验</span><input v-model.number="selectedTemplate.participationRewardExp" type="number" min="0" /></label>
              <label class="field"><span>金币</span><input v-model.number="selectedTemplate.participationRewardGold" type="number" min="0" /></label>
              <label class="field"><span>灵石</span><input v-model.number="selectedTemplate.participationRewardSpiritStone" type="number" min="0" /></label>
            </div>
          </div>

          <div>
            <h4 class="subheading">排行奖励</h4>
            <div class="editor-grid editor-grid--three">
              <label class="field"><span>第1名经验</span><input v-model.number="selectedTemplate.rank1RewardExp" type="number" min="0" /></label>
              <label class="field"><span>第1名金币</span><input v-model.number="selectedTemplate.rank1RewardGold" type="number" min="0" /></label>
              <label class="field"><span>第1名灵石</span><input v-model.number="selectedTemplate.rank1RewardSpiritStone" type="number" min="0" /></label>
              <label class="field"><span>第2名经验</span><input v-model.number="selectedTemplate.rank2RewardExp" type="number" min="0" /></label>
              <label class="field"><span>第2名金币</span><input v-model.number="selectedTemplate.rank2RewardGold" type="number" min="0" /></label>
              <label class="field"><span>第2名灵石</span><input v-model.number="selectedTemplate.rank2RewardSpiritStone" type="number" min="0" /></label>
              <label class="field"><span>第3名经验</span><input v-model.number="selectedTemplate.rank3RewardExp" type="number" min="0" /></label>
              <label class="field"><span>第3名金币</span><input v-model.number="selectedTemplate.rank3RewardGold" type="number" min="0" /></label>
              <label class="field"><span>第3名灵石</span><input v-model.number="selectedTemplate.rank3RewardSpiritStone" type="number" min="0" /></label>
            </div>
          </div>
        </div>
      </fieldset>

      <template #footer>
        <button class="secondary-button" type="button" @click="editorOpen = false">取消</button>
        <button class="danger-button" type="button" :disabled="!canManageConfigs || !selectedTemplate?.bossId" @click="removeTemplate">删除</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="saveTemplate">保存模板</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import AdminImageField from '@/components/AdminImageField.vue'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import type {
  AdminMonsterListItem,
  AdminWorldBossRuntime,
  AdminWorldBossSchedule,
  AdminWorldBossTemplateDetail,
  AdminWorldBossTemplateListItem
} from '@/types/admin'
import { getAdminMonsters } from '@/services/monsters'
import {
  closeAdminWorldBoss,
  deleteAdminWorldBossTemplate,
  getAdminWorldBossRuntime,
  getAdminWorldBossSchedule,
  getAdminWorldBossTemplate,
  getAdminWorldBossTemplates,
  saveAdminWorldBossSchedule,
  saveAdminWorldBossTemplate,
  spawnAdminWorldBoss
} from '@/services/world-boss'
import toast from '@/utils/toast'

// 当前登录管理员的权限集合。
// 世界 Boss 页面上的保存、删除、生成等危险操作都会依赖它来控制按钮状态。
const { canManageConfigs } = useAdminPermissions()

// 页面主状态：
// templates 管模板列表，selectedTemplate 管当前编辑项，schedule 管每日排期，
// runtime 管当前世界 Boss 运行时概览，message/errorMessage 用于顶部反馈提示。
const templates = ref<AdminWorldBossTemplateListItem[]>([])
const monsters = ref<AdminMonsterListItem[]>([])
const selectedTemplate = ref<AdminWorldBossTemplateDetail | null>(null)
const schedule = ref<AdminWorldBossSchedule>({
  scheduleId: 'default',
  spawnTimeText: '11:00',
  timeZoneId: 'China Standard Time',
  selectionMode: 0,
  isEnabled: true
})
const runtime = ref<AdminWorldBossRuntime>({
  hasActiveInstance: false,
  current: null,
  rankingTop10: [],
  recentLogs: []
})
const editorOpen = ref(false)

// 统一格式化后台返回的时间字符串，方便列表和运行时面板复用。
function formatDateTime(value?: string | null) {
  if (!value) return '-'
  return value.replace('T', ' ').slice(0, 19)
}

// 新建模板时使用的默认对象。
// 这里把后台必填字段都初始化好，避免第一次打开编辑器就是大量空值。
function createEmptyTemplate(): AdminWorldBossTemplateDetail {
  return {
    bossId: '',
    name: '',
    monsterTemplateId: '',
    portraitPath: '',
    isEnabled: true,
    weight: 1,
    durationMinutes: 60,
    noticeText: '',
    participationMinDamage: 1,
    participationRewardExp: 0,
    participationRewardGold: 0,
    participationRewardSpiritStone: 0,
    rank1RewardExp: 0,
    rank1RewardGold: 0,
    rank1RewardSpiritStone: 0,
    rank2RewardExp: 0,
    rank2RewardGold: 0,
    rank2RewardSpiritStone: 0,
    rank3RewardExp: 0,
    rank3RewardGold: 0,
    rank3RewardSpiritStone: 0,
    sortOrder: 0
  }
}

// 打开“新建模板”弹窗。
function openCreateModal() {
  selectedTemplate.value = createEmptyTemplate()
  editorOpen.value = true
}

// 打开“编辑模板”弹窗，并先加载模板详情。
async function openEditModal(bossId: string) {
  selectedTemplate.value = await getAdminWorldBossTemplate(bossId)
  editorOpen.value = true
}

// 拉取模板列表。
async function loadTemplates() {
  templates.value = await getAdminWorldBossTemplates()
}

// 拉取每日排期配置。
async function loadSchedule() {
  schedule.value = await getAdminWorldBossSchedule()
}

// 拉取当前世界 Boss 运行时概览。
async function loadRuntime() {
  runtime.value = await getAdminWorldBossRuntime()
}

// 拉取怪物模板选项，供 Boss 模板编辑器下拉框使用。
async function loadMonsterOptions() {
  monsters.value = await getAdminMonsters()
}

// 页面首次进入或手动刷新时并行加载全部数据。
async function loadAll() {
  await Promise.all([loadTemplates(), loadSchedule(), loadRuntime(), loadMonsterOptions()])
}

// 保存当前编辑中的 Boss 模板。
async function saveTemplate() {
  if (!selectedTemplate.value) return

  try {
    selectedTemplate.value = await saveAdminWorldBossTemplate(selectedTemplate.value)
    editorOpen.value = false
    toast.success('世界Boss模板保存成功。')
    await loadTemplates()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '保存失败。')
  }
}

// 删除当前编辑中的 Boss 模板。
async function removeTemplate() {
  if (!selectedTemplate.value?.bossId) return
  if (!window.confirm(`确认删除模板 ${selectedTemplate.value.bossId} 吗？`)) return

  try {
    await deleteAdminWorldBossTemplate(selectedTemplate.value.bossId)
    editorOpen.value = false
    selectedTemplate.value = null
    toast.success('世界Boss模板删除成功。')
    await loadTemplates()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '删除失败。')
  }
}

// 保存每日排期配置。
async function saveScheduleConfig() {
  try {
    schedule.value = await saveAdminWorldBossSchedule(schedule.value)
    toast.success('世界Boss排期保存成功。')
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '排期保存失败。')
  }
}

// 按后台服务默认选取逻辑立即生成一个世界 Boss。
async function spawnRandomBoss() {
  try {
    await spawnAdminWorldBoss()
    toast.success('世界Boss已生成。')
    await loadRuntime()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '生成失败。')
  }
}

// 使用当前选中的模板立即生成世界 Boss。
async function spawnSelectedBoss() {
  if (!selectedTemplate.value?.bossId) return

  try {
    await spawnAdminWorldBoss(selectedTemplate.value.bossId)
    toast.success('指定世界Boss已生成。')
    await loadRuntime()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '生成失败。')
  }
}

// 手动关闭当前运行中的世界 Boss。
async function closeCurrentBoss() {
  if (!window.confirm('确认关闭当前世界Boss吗？')) return

  try {
    await closeAdminWorldBoss()
    toast.success('当前世界Boss已关闭。')
    await loadRuntime()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '关闭失败。')
  }
}

// 页面挂载后立即加载一次完整数据。
onMounted(loadAll)
</script>

<style scoped>
.runtime-columns {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
}

.runtime-column {
  min-width: 0;
}

.runtime-log-list {
  max-height: 360px;
  overflow: auto;
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: 14px;
  padding: 12px;
  background: rgba(5, 10, 18, 0.3);
}

.runtime-log-item {
  display: flex;
  gap: 12px;
  padding: 8px 0;
  border-bottom: 1px solid rgba(255, 255, 255, 0.06);
}

.runtime-log-item:last-child {
  border-bottom: none;
}

.runtime-log-time {
  color: var(--text-tertiary);
  white-space: nowrap;
}

.runtime-log-text {
  color: var(--text-primary);
}

.runtime-log-empty {
  color: var(--text-tertiary);
  text-align: center;
  padding: 28px 0;
}

.subheading {
  margin: 0 0 12px;
  font-size: 14px;
  color: var(--text-secondary);
}

.section-split {
  display: grid;
  grid-template-columns: 1fr;
  gap: 20px;
}

@media (max-width: 1100px) {
  .runtime-columns {
    grid-template-columns: 1fr;
  }
}
</style>
