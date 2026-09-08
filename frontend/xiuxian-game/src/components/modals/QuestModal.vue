<template>
  <XiuXianModal
    :model-value="modelValue"
    title="任务簿"
    :width="960"
    @update:model-value="$emit('update:modelValue', $event)"
  >
    <div class="quest-modal">
      <div class="quest-toolbar">
        <div class="summary-grid">
          <div class="summary-card">
            <span class="summary-label">可接取</span>
            <strong class="summary-value">{{ availableQuests.length }}</strong>
          </div>
          <div class="summary-card">
            <span class="summary-label">进行中</span>
            <strong class="summary-value">{{ activeQuests.length }}</strong>
          </div>
          <div class="summary-card">
            <span class="summary-label">待提交</span>
            <strong class="summary-value">{{ completedQuests.length }}</strong>
          </div>
        </div>

        <button class="refresh-btn" :disabled="isLoading" @click="loadQuestData">
          {{ isLoading ? '刷新中...' : '刷新任务' }}
        </button>
      </div>

      <div class="tab-row">
        <button
          v-for="tab in tabs"
          :key="tab.key"
          class="tab-btn"
          :class="{ active: currentTab === tab.key }"
          @click="currentTab = tab.key"
        >
          <span class="tab-icon"><AssetIcon :source="tab.icon" size="20" /></span>
          <span>{{ tab.label }}</span>
        </button>
      </div>

      <div class="quest-layout">
        <section class="list-panel">
          <div v-if="currentList.length === 0" class="empty-state">
            <div class="empty-icon"><AssetIcon :source="ICON.misc_scroll" size="25" /></div>
            <div class="empty-text">{{ emptyText }}</div>
          </div>

          <button
            v-for="quest in currentList"
            :key="quest.questId"
            class="quest-item"
            :class="{ selected: selectedQuest?.questId === quest.questId }"
            @click="selectedQuestId = quest.questId"
          >
            <div class="quest-item-header">
              <div class="quest-main">
                <span class="quest-icon"><AssetIcon :source="quest.icon" size="20" /></span>
                <div class="quest-headline">
                  <div class="quest-name">{{ quest.name }}</div>
                  <div class="quest-subtitle">{{ quest.typeText }} · 推荐 Lv.{{ quest.requiredLevel }}</div>
                </div>
              </div>
              <span class="status-badge" :class="quest.statusClass">{{ quest.statusText }}</span>
            </div>

            <div class="quest-description">{{ quest.description }}</div>

            <div class="progress-block">
              <div class="progress-row">
                <span>总进度</span>
                <span>{{ formatProgress(quest.currentProgress, quest.targetProgress) }}</span>
              </div>
              <div class="progress-track">
                <span class="progress-fill" :style="{ width: `${quest.progressPercent}%` }" />
              </div>
            </div>

            <div class="quest-meta">
              <span>目标 {{ quest.objectives.length }} 项</span>
              <span>奖励 {{ quest.rewards.length }} 项</span>
            </div>
          </button>
        </section>

        <section class="detail-panel">
          <div v-if="!selectedQuest" class="detail-empty">
            <div class="empty-icon"><AssetIcon :source="ICON.ui_compass" size="25" /></div>
            <div class="empty-text">选择左侧任务查看详情。</div>
          </div>

          <template v-else>
            <div class="detail-header">
              <div>
                <div class="detail-title-row">
                  <span class="detail-icon"><AssetIcon :source="selectedQuest.icon" size="20" /></span>
                  <h3 class="detail-title">{{ selectedQuest.name }}</h3>
                </div>
                <div class="detail-subtitle">
                  {{ selectedQuest.typeText }} · 推荐 Lv.{{ selectedQuest.requiredLevel }}
                </div>
              </div>
              <span class="status-badge" :class="selectedQuest.statusClass">{{ selectedQuest.statusText }}</span>
            </div>

            <div class="detail-description">{{ selectedQuest.description }}</div>

            <div class="panel-card overall-card">
              <div class="panel-title">任务总进度</div>
              <div class="progress-row overall-row">
                <span>{{ formatProgress(selectedQuest.currentProgress, selectedQuest.targetProgress) }}</span>
                <span>{{ selectedQuest.progressPercent.toFixed(0) }}%</span>
              </div>
              <div class="progress-track large">
                <span class="progress-fill" :style="{ width: `${selectedQuest.progressPercent}%` }" />
              </div>
            </div>

            <div class="detail-grid">
              <div class="panel-card">
                <div class="panel-title">任务目标</div>
                <div class="objective-list">
                  <div
                    v-for="objective in selectedQuest.objectives"
                    :key="`${selectedQuest.questId}-${objective.index}`"
                    class="objective-item"
                    :class="{ completed: objective.isCompleted }"
                  >
                    <span class="objective-marker"><AssetIcon :source="objective.icon" size="16" /></span>
                    <div class="objective-content">
                      <div class="objective-header">
                        <div class="objective-name">{{ objective.description }}</div>
                        <span class="objective-state">{{ objective.isCompleted ? '已完成' : '进行中' }}</span>
                      </div>
                      <div class="objective-meta">{{ objective.typeText }}</div>
                      <div class="progress-row">
                        <span>目标进度</span>
                        <span>{{ formatProgress(objective.currentProgress, objective.targetProgress) }}</span>
                      </div>
                      <div class="progress-track">
                        <span class="progress-fill" :style="{ width: `${objective.progressPercent}%` }" />
                      </div>
                    </div>
                  </div>
                </div>
                <div v-if="selectedQuest.listType === 'active'" class="hint-text">
                  任务精确进度已接入后端实时概览，界面显示的是当前数据库中的真实值。
                </div>
              </div>

              <div class="panel-card">
                <div class="panel-title">奖励内容</div>
                <div class="reward-list">
                  <div
                    v-for="reward in selectedQuest.rewards"
                    :key="`${selectedQuest.questId}-${reward.key}`"
                    class="reward-item"
                  >
                    <span class="reward-icon"><AssetIcon :source="reward.icon" size="16" /></span>
                    <div class="reward-content">
                      <div class="reward-name">{{ reward.label }}</div>
                      <div class="reward-value">{{ reward.value }}</div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <div class="meta-strip">
              <span v-if="selectedQuest.acceptTime">接取于 {{ formatDateTime(selectedQuest.acceptTime) }}</span>
              <span v-if="selectedQuest.completeTime">完成于 {{ formatDateTime(selectedQuest.completeTime) }}</span>
              <span v-if="selectedQuest.submitTime">提交于 {{ formatDateTime(selectedQuest.submitTime) }}</span>
              <span v-if="selectedQuest.timeLimit > 0">限时 {{ selectedQuest.timeLimit }} 秒</span>
              <span v-if="selectedQuest.autoSubmit">完成后自动提交</span>
            </div>

            <div class="action-row">
              <button
                v-if="selectedQuest.listType === 'available'"
                class="primary-btn"
                :disabled="isSubmitting"
                @click="acceptSelectedQuest"
              >
                {{ isSubmitting ? '处理中...' : '接取任务' }}
              </button>

              <button
                v-if="selectedQuest.listType === 'completed'"
                class="primary-btn"
                :disabled="isSubmitting"
                @click="submitSelectedQuest"
              >
                {{ isSubmitting ? '处理中...' : '提交任务' }}
              </button>

              <button
                v-if="selectedQuest.listType === 'active'"
                class="danger-btn"
                :disabled="isSubmitting || selectedQuest.typeValue === 0"
                @click="abandonSelectedQuest"
              >
                {{ selectedQuest.typeValue === 0 ? '主线不可放弃' : (isSubmitting ? '处理中...' : '放弃任务') }}
              </button>
            </div>
          </template>
        </section>
      </div>

      <div v-if="statusMessage" class="status-message">{{ statusMessage }}</div>
    </div>
  </XiuXianModal>
</template>

<script>
import { computed, ref, watch } from 'vue'
import XiuXianModal from '../common/XiuXianModal.vue'
import AssetIcon from '../common/AssetIcon.vue'
import { apiClient } from '../../lib/apiClient'
import { useGameStore } from '../../state/gameStore'
import { ICON } from '../../icons'

/*
 * 中文注释：
 * 任务弹窗统一走后端 overview 接口。
 * 可接、进行中、待提交，以及每条 objective 的精确进度都来自数据库当前值。
 */
const QUEST_TYPE_META = {
  0: { text: '主线', icon: ICON.misc_fire },
  1: { text: '支线', icon: ICON.misc_map },
  2: { text: '日常', icon: ICON.ui_sun },
  3: { text: '周常', icon: ICON.misc_calendar },
  4: { text: '成就', icon: ICON.misc_trophy }
}

const QUEST_STATUS_META = {
  0: { text: '未接取', className: 'neutral' },
  1: { text: '进行中', className: 'info' },
  2: { text: '待提交', className: 'success' },
  3: { text: '已完成', className: 'neutral' },
  4: { text: '已放弃', className: 'danger' },
  5: { text: '已失败', className: 'danger' }
}

const OBJECTIVE_TYPE_META = {
  0: { text: '击杀', icon: ICON.misc_sword_crossed },
  1: { text: '收集', icon: ICON.misc_backpack },
  2: { text: '对话', icon: ICON.misc_book_open },
  3: { text: '副本', icon: ICON.misc_castle },
  4: { text: '等级', icon: ICON.misc_growth },
  5: { text: '强化', icon: ICON.ui_gear },
  6: { text: '战斗事件', icon: ICON.misc_fire },
  7: { text: '地图战斗', icon: ICON.misc_map },
  8: { text: '地图胜利', icon: ICON.misc_trophy },
  9: { text: '炼丹领取', icon: ICON.nav_alchemy },
  10: { text: '锻造领取', icon: ICON.nav_forge },
  11: { text: '炼丹成功', icon: ICON.nav_alchemy },
  12: { text: '锻造成功', icon: ICON.misc_dagger },
  13: { text: '强化次数', icon: ICON.misc_sparkles },
  14: { text: '强化达标', icon: ICON.misc_shield },
  15: { text: '种植', icon: ICON.misc_seedling },
  16: { text: '收获', icon: ICON.misc_seedling }
}

const REWARD_META = {
  Exp: { icon: ICON.misc_sparkles, label: '修为', prefix: '+' },
  Gold: { icon: ICON.misc_coin, label: '金币', prefix: '+' },
  SpiritStone: { icon: ICON.misc_diamond, label: '灵石', prefix: '+' },
  Honor: { icon: ICON.misc_medal, label: '荣誉', prefix: '+' },
  GuildContribution: { icon: ICON.misc_castle, label: '宗门贡献', prefix: '+' },
  Item: { icon: ICON.misc_gift, label: '物品', prefix: 'x' },
  Equipment: { icon: ICON.misc_dagger, label: '装备', prefix: 'x' }
}

function toNumber(value, fallback = 0) {
  const numericValue = Number(value)
  return Number.isFinite(numericValue) ? numericValue : fallback
}

function clampPercent(value) {
  return Math.max(0, Math.min(100, Number(value) || 0))
}

function fallbackQuestStatus(listType) {
  if (listType === 'completed') return 2
  if (listType === 'active') return 1
  return 0
}

function buildRewardEntries(rewardSource) {
  if (Array.isArray(rewardSource) && rewardSource.length > 0) {
    return rewardSource.map((reward, index) => {
      const rewardType = reward?.RewardType ?? reward?.rewardType ?? 'Item'
      const meta = REWARD_META[rewardType] || REWARD_META.Item
      const quantity = toNumber(reward?.Quantity ?? reward?.quantity ?? reward?.Amount ?? reward?.amount, 0)
      const label = reward?.Name ?? reward?.name ?? reward?.ItemId ?? reward?.itemId ?? meta.label
      const value = rewardType === 'Equipment'
        ? 'x1'
        : `${meta.prefix}${quantity}`

      return {
        key: `${rewardType}-${reward?.ItemId ?? reward?.itemId ?? reward?.EquipmentId ?? reward?.equipmentId ?? index}`,
        icon: meta.icon,
        label,
        value
      }
    })
  }

  const reward = rewardSource || {}
  const entries = []

  const addEntry = (key, type, amount, label = null) => {
    const numericAmount = toNumber(amount, 0)
    if (numericAmount <= 0) {
      return
    }

    const meta = REWARD_META[type] || REWARD_META.Item
    entries.push({
      key,
      icon: meta.icon,
      label: label || meta.label,
      value: `${meta.prefix}${numericAmount}`
    })
  }

  addEntry('exp', 'Exp', reward.Exp ?? reward.exp)
  addEntry('gold', 'Gold', reward.Gold ?? reward.gold)
  addEntry('spiritStone', 'SpiritStone', reward.SpiritStone ?? reward.spiritStone)
  addEntry('honor', 'Honor', reward.Honor ?? reward.honor)
  addEntry('guildContribution', 'GuildContribution', reward.GuildContribution ?? reward.guildContribution)

  const items = reward.Items || reward.items || {}
  Object.entries(items).forEach(([itemId, quantity]) => {
    addEntry(`item-${itemId}`, 'Item', quantity, itemId)
  })

  const equipmentIds = reward.EquipmentIds || reward.equipmentIds || []
  equipmentIds.forEach((equipmentId) => {
    entries.push({
      key: `equipment-${equipmentId}`,
      icon: REWARD_META.Equipment.icon,
      label: `装备 ${equipmentId}`,
      value: 'x1'
    })
  })

  if (entries.length === 0) {
    entries.push({
      key: 'none',
      icon: '🪶',
      label: '暂无奖励',
      value: '无'
    })
  }

  return entries
}

function normalizeObjective(objective, index) {
  // 中文注释：
  // 任务目标在不同接口里字段层级略有差异，
  // 这里统一折成列表和详情都能复用的结构。
  const typeValue = toNumber(objective?.ObjectiveTypeValue ?? objective?.objectiveTypeValue ?? objective?.ObjectiveType ?? objective?.objectiveType, 0)
  const meta = OBJECTIVE_TYPE_META[typeValue] || OBJECTIVE_TYPE_META[0]
  const currentProgress = toNumber(objective?.CurrentProgress ?? objective?.currentProgress, 0)
  const targetProgress = Math.max(1, toNumber(
    objective?.TargetProgress ?? objective?.targetProgress ?? objective?.TargetCount ?? objective?.targetCount,
    1
  ))
  const progressPercent = clampPercent(
    objective?.ProgressPercent ?? objective?.progressPercent ?? (currentProgress * 100 / targetProgress)
  )

  return {
    index: toNumber(objective?.Index ?? objective?.index, index),
    typeValue,
    typeText: meta.text,
    icon: objective?.Icon || objective?.icon || meta.icon,
    targetId: objective?.TargetId ?? objective?.targetId ?? '',
    currentProgress,
    targetProgress,
    progressPercent,
    isCompleted: Boolean(objective?.IsCompleted ?? objective?.isCompleted ?? currentProgress >= targetProgress),
    description: objective?.Description ?? objective?.description ?? '完成对应目标'
  }
}

function normalizeQuest(rawQuest, listType = 'available') {
  // 中文注释：
  // 单条任务在进入组件时先把目标、奖励、状态、总进度全部整理好，
  // 模板层后续只读取一种字段口径。
  const typeValue = toNumber(rawQuest?.TypeValue ?? rawQuest?.typeValue ?? rawQuest?.QuestType ?? rawQuest?.questType, 0)
  const typeMeta = QUEST_TYPE_META[typeValue] || QUEST_TYPE_META[1]
  const statusValue = toNumber(rawQuest?.StatusValue ?? rawQuest?.statusValue, fallbackQuestStatus(listType))
  const statusMeta = QUEST_STATUS_META[statusValue] || QUEST_STATUS_META[0]
  const objectives = Array.isArray(rawQuest?.Objectives || rawQuest?.objectives)
    ? (rawQuest?.Objectives || rawQuest?.objectives).map((objective, index) => normalizeObjective(objective, index))
    : []

  const objectiveTargetTotal = objectives.reduce((sum, objective) => sum + objective.targetProgress, 0)
  const objectiveCurrentTotal = objectives.reduce((sum, objective) => sum + Math.min(objective.currentProgress, objective.targetProgress), 0)
  const targetProgress = Math.max(
    toNumber(rawQuest?.TargetProgress ?? rawQuest?.targetProgress, objectiveTargetTotal),
    objectiveTargetTotal
  )
  const currentProgress = toNumber(rawQuest?.CurrentProgress ?? rawQuest?.currentProgress, objectiveCurrentTotal)
  const progressPercent = clampPercent(
    rawQuest?.ProgressPercent ?? rawQuest?.progressPercent ?? (targetProgress > 0 ? currentProgress * 100 / targetProgress : 0)
  )

  let displayStatusText = statusMeta.text
  if (listType === 'available') displayStatusText = '可接取'
  if (listType === 'completed') displayStatusText = '待提交'

  return {
    questId: rawQuest?.QuestId ?? rawQuest?.questId ?? '',
    name: rawQuest?.Name ?? rawQuest?.name ?? rawQuest?.QuestName ?? rawQuest?.questName ?? '未命名任务',
    description: rawQuest?.Description ?? rawQuest?.description ?? '暂无任务描述。',
    typeValue,
    typeText: typeMeta.text,
    icon: rawQuest?.Icon || rawQuest?.icon || typeMeta.icon,
    requiredLevel: toNumber(rawQuest?.RequiredLevel ?? rawQuest?.requiredLevel, 1),
    statusValue,
    statusText: displayStatusText,
    statusClass: listType === 'completed' ? 'success' : statusMeta.className,
    currentProgress,
    targetProgress,
    progressPercent,
    sortOrder: toNumber(rawQuest?.SortOrder ?? rawQuest?.sortOrder, 0),
    autoSubmit: Boolean(rawQuest?.AutoSubmit ?? rawQuest?.autoSubmit),
    timeLimit: toNumber(rawQuest?.TimeLimit ?? rawQuest?.timeLimit, 0),
    objectives,
    rewards: buildRewardEntries(rawQuest?.Rewards ?? rawQuest?.rewards),
    acceptTime: rawQuest?.AcceptTime ?? rawQuest?.acceptTime ?? null,
    completeTime: rawQuest?.CompleteTime ?? rawQuest?.completeTime ?? null,
    submitTime: rawQuest?.SubmitTime ?? rawQuest?.submitTime ?? null,
    listType
  }
}

export default {
  name: 'QuestModal',

  components: {
    XiuXianModal,
    AssetIcon
  },

  props: {
    modelValue: Boolean
  },

  emits: ['update:modelValue'],

  setup(props) {
    const gameStore = useGameStore()
    const tabs = [
      { key: 'available', label: '可接取', icon: ICON.misc_map },
      { key: 'active', label: '进行中', icon: ICON.misc_sword_crossed },
      { key: 'completed', label: '待提交', icon: ICON.ui_lantern }
    ]

    const currentTab = ref('available')
    const availableQuests = ref([])
    const activeQuests = ref([])
    const completedQuests = ref([])
    const selectedQuestId = ref('')
    const isLoading = ref(false)
    const isSubmitting = ref(false)
    const statusMessage = ref('')

    const currentList = computed(() => {
      if (currentTab.value === 'active') return activeQuests.value
      if (currentTab.value === 'completed') return completedQuests.value
      return availableQuests.value
    })

    const selectedQuest = computed(() => {
      return currentList.value.find((quest) => quest.questId === selectedQuestId.value) || null
    })

    const emptyText = computed(() => {
      if (currentTab.value === 'active') return '当前没有进行中的任务。'
      if (currentTab.value === 'completed') return '当前没有待提交的任务。'
      return '当前没有可接取的任务。'
    })

    function formatDateTime(value) {
      if (!value) {
        return '未知时间'
      }

      const date = new Date(value)
      if (Number.isNaN(date.getTime())) {
        return String(value)
      }

      return date.toLocaleString('zh-CN', { hour12: false })
    }

    function formatProgress(current, target) {
      const safeTarget = Math.max(0, toNumber(target, 0))
      return `${toNumber(current, 0)} / ${safeTarget}`
    }

    async function loadQuestData() {
      isLoading.value = true

      try {
        const [available, active, completed] = await Promise.all([
          apiClient.getAvailableQuestOverview(),
          apiClient.getActiveQuestOverview(),
          apiClient.getCompletedQuestOverview()
        ])

        availableQuests.value = (Array.isArray(available) ? available : [])
          .map((quest) => normalizeQuest(quest, 'available'))
          .filter((quest) => quest.typeValue !== 5)
          .sort((left, right) => left.sortOrder - right.sortOrder)

        activeQuests.value = (Array.isArray(active) ? active : [])
          .map((quest) => normalizeQuest(quest, 'active'))
          .filter((quest) => quest.typeValue !== 5)
          .sort((left, right) => left.sortOrder - right.sortOrder)

        completedQuests.value = (Array.isArray(completed) ? completed : [])
          .map((quest) => normalizeQuest(quest, 'completed'))
          .filter((quest) => quest.typeValue !== 5)
          .sort((left, right) => left.sortOrder - right.sortOrder)

        statusMessage.value = ''
      } catch (error) {
        statusMessage.value = error.message || '加载任务数据失败。'
      } finally {
        isLoading.value = false
      }
    }

    async function refreshAfterQuestMutation({ includeInventory = false, includeEquipment = false } = {}) {
      // 中文注释：
      // 任务操作本身不直接修改本地玩家状态，
      // 统一走包装层刷新，确保奖励入包、装备发放、排行榜变化都能同步回首页。
      await gameStore.refreshPlayerSnapshot({
        includeInventory,
        includeEquipment,
        includeRankings: true
      })
    }

    async function acceptSelectedQuest() {
      if (!selectedQuest.value || isSubmitting.value) {
        return
      }

      isSubmitting.value = true

      try {
        // 中文注释：
        // 接取成功后切到“进行中”标签，让玩家立刻看到刚接的任务进度，而不是仍停留在可接列表。
        const result = await apiClient.acceptQuest(selectedQuest.value.questId)
        statusMessage.value = result?.message || result?.Message || '任务已接取。'
        await refreshAfterQuestMutation()
        await loadQuestData()
        currentTab.value = 'active'
      } catch (error) {
        statusMessage.value = error.message || '接取任务失败。'
      } finally {
        isSubmitting.value = false
      }
    }

    async function submitSelectedQuest() {
      if (!selectedQuest.value || isSubmitting.value) {
        return
      }

      isSubmitting.value = true

      try {
        // 中文注释：
        // 提交任务可能发放道具和装备，所以这里显式要求补刷背包与装备模块。
        const result = await apiClient.submitQuest(selectedQuest.value.questId)
        statusMessage.value = result?.message || result?.Message || '任务奖励已领取。'
        await refreshAfterQuestMutation({
          includeInventory: true,
          includeEquipment: true
        })
        await loadQuestData()
        currentTab.value = 'available'
      } catch (error) {
        statusMessage.value = error.message || '提交任务失败。'
      } finally {
        isSubmitting.value = false
      }
    }

    async function abandonSelectedQuest() {
      if (!selectedQuest.value || isSubmitting.value) {
        return
      }

      isSubmitting.value = true

      try {
        const result = await apiClient.abandonQuest(selectedQuest.value.questId)
        statusMessage.value = result?.message || result?.Message || '任务已放弃。'
        await refreshAfterQuestMutation()
        await loadQuestData()
        currentTab.value = 'available'
      } catch (error) {
        statusMessage.value = error.message || '放弃任务失败。'
      } finally {
        isSubmitting.value = false
      }
    }

    watch(currentList, (list) => {
      // 中文注释：
      // 当前标签列表变化后，如果旧选中任务已经不在新列表中，
      // 自动切到第一条，避免右侧详情引用空对象。
      if (list.some((quest) => quest.questId === selectedQuestId.value)) {
        return
      }

      selectedQuestId.value = list[0]?.questId || ''
    }, { immediate: true })

    watch(() => props.modelValue, async (visible) => {
      if (!visible) {
        return
      }

      // 中文注释：
      // 任务弹窗按打开时懒加载，减少首页阶段的无关接口请求。
      await loadQuestData()
    })

    return {
      ICON,
      tabs,
      currentTab,
      availableQuests,
      activeQuests,
      completedQuests,
      currentList,
      selectedQuestId,
      selectedQuest,
      emptyText,
      isLoading,
      isSubmitting,
      statusMessage,
      formatDateTime,
      formatProgress,
      loadQuestData,
      acceptSelectedQuest,
      submitSelectedQuest,
      abandonSelectedQuest
    }
  }
}
</script>

<style scoped>
.quest-modal {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
  color: var(--text-primary);
}

.quest-toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--spacing-md);
}

.summary-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: var(--spacing-sm);
  flex: 1;
}

.summary-card,
.panel-card,
.quest-item,
.detail-panel,
.list-panel {
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
}

.summary-card {
  padding: var(--spacing-sm) var(--spacing-md);
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.summary-label,
.quest-subtitle,
.quest-description,
.quest-meta,
.detail-subtitle,
.objective-meta,
.reward-value,
.meta-strip,
.hint-text,
.progress-row {
  color: var(--text-secondary);
}

.summary-value {
  color: var(--highlight-text);
  font-size: var(--font-size-lg);
}

.refresh-btn,
.primary-btn,
.danger-btn,
.tab-btn {
  border-radius: var(--radius-sm);
  transition: all 0.25s ease;
  font-family: var(--font-primary);
}

.refresh-btn,
.primary-btn,
.danger-btn {
  border: 1px solid transparent;
  cursor: pointer;
}

.refresh-btn {
  padding: var(--spacing-sm) var(--spacing-md);
  background: var(--button-neutral-bg);
  border-color: var(--button-neutral-border);
  color: var(--button-neutral-text);
}

.refresh-btn:hover:not(:disabled) {
  background: var(--button-neutral-hover-bg);
  color: var(--button-neutral-hover-text);
  border-color: var(--button-outline-hover-border);
}

.refresh-btn:disabled,
.primary-btn:disabled,
.danger-btn:disabled {
  opacity: 0.55;
  cursor: not-allowed;
}

.tab-row {
  display: flex;
  gap: var(--spacing-sm);
  flex-wrap: wrap;
}

.tab-btn {
  display: inline-flex;
  align-items: center;
  gap: var(--spacing-xs);
  padding: var(--spacing-sm) var(--spacing-md);
  background: transparent;
  border: 1px solid var(--border-color);
  color: var(--text-secondary);
  cursor: pointer;
}

.tab-btn:hover,
.tab-btn.active {
  color: var(--text-primary);
  background: var(--accent-soft-bg);
  border-color: var(--accent-text);
  box-shadow: var(--shadow-sm);
}

.quest-layout {
  display: grid;
  grid-template-columns: 320px minmax(0, 1fr);
  gap: var(--spacing-md);
  height: min(480px, 65vh);
  min-height: 0;
  overflow: hidden;
}

.list-panel {
  padding: var(--spacing-sm);
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
  overflow-y: auto;
}

.quest-item {
  text-align: left;
  padding: var(--spacing-md);
  color: inherit;
  cursor: pointer;
}

.quest-item:hover,
.quest-item.selected {
  border-color: var(--accent-text);
  background: var(--accent-soft-bg);
  transform: translateY(-1px);
}

.quest-item-header,
.detail-header,
.quest-main,
.detail-title-row,
.reward-item,
.objective-item,
.objective-content,
.reward-content,
.objective-header {
  display: flex;
}

.quest-item-header,
.detail-header,
.objective-header {
  align-items: flex-start;
  justify-content: space-between;
  gap: var(--spacing-sm);
}

.quest-main,
.detail-title-row,
.reward-item,
.objective-item {
  gap: var(--spacing-sm);
}

.quest-icon,
.detail-icon {
  font-size: 24px;
}

.quest-headline {
  min-width: 0;
}

.quest-name,
.detail-title,
.objective-name,
.reward-name {
  color: var(--text-primary);
  margin: 0;
}

.quest-description {
  margin-top: var(--spacing-sm);
  line-height: 1.6;
}

.quest-meta {
  margin-top: var(--spacing-sm);
  display: flex;
  gap: var(--spacing-md);
  font-size: var(--font-size-sm);
}

.status-badge,
.objective-state {
  display: inline-flex;
  align-items: center;
  padding: 4px 10px;
  border-radius: 999px;
  font-size: var(--font-size-sm);
  white-space: nowrap;
}

.status-badge.neutral {
  background: rgba(255, 255, 255, 0.08);
  color: var(--text-secondary);
}

.status-badge.info {
  background: var(--status-info-bg);
  color: var(--status-info-text);
}

.status-badge.success {
  background: var(--status-success-bg);
  color: var(--status-success-text);
}

.status-badge.danger {
  background: var(--status-danger-bg);
  color: var(--status-danger-text);
}

.detail-panel {
  padding: var(--spacing-lg);
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
  overflow-y: auto;
  min-height: 0;
}

.detail-description {
  padding: var(--spacing-md);
  background: var(--highlight-soft-bg);
  border-radius: var(--radius-md);
  line-height: 1.8;
  color: var(--text-primary);
}

.detail-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: var(--spacing-md);
}

.panel-card {
  padding: var(--spacing-md);
}

.panel-title {
  color: var(--highlight-text);
  margin-bottom: var(--spacing-md);
  font-weight: 600;
}

.objective-list,
.reward-list {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
}

.objective-item,
.reward-item {
  align-items: flex-start;
  padding: var(--spacing-sm);
  background: rgba(255, 255, 255, 0.03);
  border-radius: var(--radius-sm);
}

.objective-item.completed {
  border: 1px solid var(--control-border);
  background: var(--accent-soft-bg);
}

.objective-content,
.reward-content {
  flex: 1;
  flex-direction: column;
  gap: 6px;
  min-width: 0;
}

.objective-state {
  background: rgba(255, 255, 255, 0.08);
  color: var(--text-secondary);
}

.progress-block {
  margin-top: var(--spacing-sm);
}

.progress-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--spacing-sm);
  font-size: var(--font-size-sm);
}

.progress-track {
  width: 100%;
  height: 8px;
  margin-top: 8px;
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.08);
  overflow: hidden;
}

.progress-track.large {
  height: 10px;
}

.progress-fill {
  display: block;
  height: 100%;
  border-radius: inherit;
  background: linear-gradient(90deg, rgba(124, 58, 237, 0.78), rgba(227, 179, 92, 0.92));
  box-shadow: 0 0 12px rgba(124, 58, 237, 0.28);
}

.overall-card {
  background: var(--highlight-soft-bg);
}

.overall-row {
  font-size: var(--font-size-base);
}

.meta-strip {
  display: flex;
  gap: var(--spacing-md);
  flex-wrap: wrap;
  padding-top: var(--spacing-sm);
  border-top: 1px solid rgba(255, 255, 255, 0.06);
  font-size: var(--font-size-sm);
}

.action-row {
  display: flex;
  gap: var(--spacing-sm);
  justify-content: flex-end;
}

.primary-btn,
.danger-btn {
  min-width: 128px;
  padding: var(--spacing-sm) var(--spacing-lg);
}

.primary-btn {
  background: var(--button-primary-start);
  color: var(--button-primary-text);
}

.primary-btn:hover:not(:disabled) {
  background: var(--button-primary-start);
  opacity: 0.85;
  color: var(--button-primary-hover-text);
  box-shadow: var(--shadow-sm);
}

.danger-btn {
  background: linear-gradient(135deg, var(--button-danger-start), var(--button-danger-end));
  color: var(--button-danger-text);
}

.danger-btn:hover:not(:disabled) {
  background: linear-gradient(135deg, var(--button-danger-hover-start), var(--button-danger-hover-end));
  color: var(--button-danger-hover-text);
}

.detail-empty,
.empty-state {
  min-height: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: var(--spacing-sm);
  color: var(--text-secondary);
}

.empty-icon {
  font-size: 30px;
}

.status-message {
  padding: var(--spacing-sm) var(--spacing-md);
  background: var(--accent-soft-bg);
  border: 1px solid var(--control-border);
  border-radius: var(--radius-sm);
  color: var(--text-primary);
}

@media (max-width: 900px) {
  .quest-toolbar,
  .quest-layout,
  .detail-grid {
    grid-template-columns: 1fr;
    display: grid;
  }

  .quest-toolbar {
    gap: var(--spacing-sm);
  }

  .summary-grid {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }

  .action-row {
    justify-content: stretch;
  }

  .primary-btn,
  .danger-btn {
    flex: 1;
  }
}
</style>
