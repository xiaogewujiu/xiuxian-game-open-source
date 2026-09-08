<template>
  <XiuXianModal
    :model-value="modelValue"
    title="成就谱"
    :width="960"
    @update:model-value="$emit('update:modelValue', $event)"
  >
    <div class="achievement-modal">
      <div class="stats-grid">
        <div class="stat-card">
          <span class="stat-label">总成就</span>
          <strong class="stat-value">{{ stats.totalAchievements }}</strong>
        </div>
        <div class="stat-card">
          <span class="stat-label">已领取</span>
          <strong class="stat-value">{{ stats.completedCount }}</strong>
        </div>
        <div class="stat-card">
          <span class="stat-label">完成率</span>
          <strong class="stat-value">{{ stats.completionPercent.toFixed(1) }}%</strong>
        </div>
        <div class="stat-card">
          <span class="stat-label">成就点</span>
          <strong class="stat-value">{{ stats.totalPoints }}</strong>
        </div>
      </div>

      <div class="toolbar-row">
        <div class="tab-row">
          <button
            v-for="tab in tabs"
            :key="tab.key"
            class="tab-btn"
            :class="{ active: currentTab === tab.key }"
            @click="currentTab = tab.key"
          >
            <span class="tab-icon"><AssetIcon :source="tab.icon" size="25" /></span>
            <span>{{ tab.label }}</span>
          </button>
        </div>

        <button class="refresh-btn" :disabled="isLoading" @click="loadAchievementData">
          {{ isLoading ? '刷新中...' : '刷新成就' }}
        </button>
      </div>

      <div class="achievement-layout">
        <section class="list-panel">
          <div v-if="currentList.length === 0" class="empty-state">
            <div class="empty-icon"><AssetIcon :source="ICON.misc_trophy" size="25" /></div>
            <div class="empty-text">当前筛选下暂无成就。</div>
          </div>

          <button
            v-for="achievement in currentList"
            :key="achievement.achievementId"
            class="achievement-item"
            :class="{ selected: selectedAchievement?.achievementId === achievement.achievementId }"
            @click="selectedAchievementId = achievement.achievementId"
          >
            <div class="achievement-header">
              <div class="achievement-main">
                <span class="achievement-icon"><AssetIcon :source="achievement.icon" size="25" /></span>
                <div class="achievement-headline">
                  <div class="achievement-name">{{ achievement.name }}</div>
                  <div class="achievement-meta">{{ achievement.category }} · {{ achievement.difficultyText }}</div>
                </div>
              </div>
              <span class="status-badge" :class="achievement.statusClass">{{ achievement.statusText }}</span>
            </div>

            <div class="achievement-description">{{ achievement.description }}</div>

            <div class="progress-block">
              <div class="progress-row">
                <span>总进度</span>
                <span>{{ formatProgress(achievement.currentProgress, achievement.targetProgress) }}</span>
              </div>
              <div class="progress-track">
                <span class="progress-fill" :style="{ width: `${achievement.progressPercent}%` }" />
              </div>
            </div>

            <div class="achievement-footer">
              <span>成就点 {{ achievement.points }}</span>
              <span>{{ achievement.typeText }}</span>
            </div>
          </button>
        </section>

        <section class="detail-panel">
          <div v-if="!selectedAchievement" class="detail-empty">
            <div class="empty-icon"><AssetIcon :source="ICON.misc_scroll" size="25" /></div>
            <div class="empty-text">选择左侧成就查看详情。</div>
          </div>

          <template v-else>
            <div class="detail-header">
              <div>
                <div class="title-row">
                  <span class="detail-icon"><AssetIcon :source="selectedAchievement.icon" size="25" /></span>
                  <h3 class="detail-title">{{ selectedAchievement.name }}</h3>
                </div>
                <div class="detail-subtitle">
                  {{ selectedAchievement.typeText }} · {{ selectedAchievement.difficultyText }} · 成就点 {{ selectedAchievement.points }}
                </div>
              </div>
              <span class="status-badge" :class="selectedAchievement.statusClass">{{ selectedAchievement.statusText }}</span>
            </div>

            <div class="detail-description">{{ selectedAchievement.description }}</div>

            <div class="panel-card overall-card">
              <div class="panel-title">成就总进度</div>
              <div class="progress-row overall-row">
                <span>{{ formatProgress(selectedAchievement.currentProgress, selectedAchievement.targetProgress) }}</span>
                <span>{{ selectedAchievement.progressPercent.toFixed(0) }}%</span>
              </div>
              <div class="progress-track large">
                <span class="progress-fill" :style="{ width: `${selectedAchievement.progressPercent}%` }" />
              </div>
            </div>

            <div class="detail-grid">
              <div class="panel-card">
                <div class="panel-title">达成条件</div>
                <div class="requirement-list">
                  <div
                    v-for="requirement in selectedAchievement.requirements"
                    :key="`${selectedAchievement.achievementId}-${requirement.index}`"
                    class="requirement-item"
                    :class="{ completed: requirement.isCompleted }"
                  >
                    <span class="requirement-icon"><AssetIcon :source="requirement.icon" size="25" /></span>
                    <div class="requirement-content">
                      <div class="requirement-header">
                        <div class="requirement-name">{{ requirement.description }}</div>
                        <span class="requirement-state">{{ requirement.isCompleted ? '已完成' : '进行中' }}</span>
                      </div>
                      <div class="requirement-meta">{{ requirement.typeText }}</div>
                      <div class="progress-row">
                        <span>{{ formatProgress(requirement.currentProgress, requirement.targetProgress) }}</span>
                        <span>{{ requirement.progressPercent.toFixed(0) }}%</span>
                      </div>
                      <div class="progress-track">
                        <span class="progress-fill" :style="{ width: `${requirement.progressPercent}%` }" />
                      </div>
                    </div>
                  </div>
                </div>
                <div v-if="selectedAchievement.statusKey === 'in_progress'" class="hint-text">
                  当前数值已直接来自后端成就概览，不再是前端根据状态推断的伪进度。
                </div>
              </div>

              <div class="panel-card">
                <div class="panel-title">奖励内容</div>
                <div class="reward-list">
                  <div
                    v-for="reward in selectedAchievement.rewards"
                    :key="`${selectedAchievement.achievementId}-${reward.key}`"
                    class="reward-item"
                  >
                    <span class="reward-icon"><AssetIcon :source="reward.icon" size="25" /></span>
                    <div class="reward-content">
                      <div class="reward-name">{{ reward.label }}</div>
                      <div class="reward-value">{{ reward.value }}</div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <div class="meta-strip">
              <span v-if="selectedAchievement.completeTime">完成于 {{ formatDateTime(selectedAchievement.completeTime) }}</span>
              <span v-if="selectedAchievement.claimTime">领取于 {{ formatDateTime(selectedAchievement.claimTime) }}</span>
              <span v-if="selectedAchievement.isHidden">隐世成就</span>
            </div>

            <div class="action-row">
              <button
                class="primary-btn"
                :disabled="selectedAchievement.statusKey !== 'claimable' || isSubmitting"
                @click="claimSelectedAchievement"
              >
                {{ isSubmitting ? '处理中...' : (selectedAchievement.statusKey === 'claimable' ? '领取奖励' : '暂不可领取') }}
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
import { computed, reactive, ref, watch } from 'vue'
import XiuXianModal from '../common/XiuXianModal.vue'
import AssetIcon from '../common/AssetIcon.vue'
import { ICON } from '../../icons'
import { apiClient } from '../../lib/apiClient'
import { useGameStore } from '../../state/gameStore'

/*
 * 中文注释：
 * 成就弹窗完全依赖后端 overview/progress 结果。
 * 左侧列表和右侧详情共享同一份归一化数据，避免前端再额外推断成就状态。
 */
const ACHIEVEMENT_TYPE_META = {
  0: { text: '境界', icon: ICON.misc_growth },
  1: { text: '战斗', icon: ICON.misc_sword_crossed },
  2: { text: '装备', icon: ICON.misc_dagger },
  3: { text: '收集', icon: ICON.misc_backpack },
  4: { text: '社交', icon: ICON.misc_group },
  5: { text: '活动', icon: ICON.ui_lantern },
  6: { text: '秘闻', icon: ICON.misc_yinyang }
}

const DIFFICULTY_META = {
  10: { text: '入门', className: 'difficulty-easy' },
  25: { text: '寻常', className: 'difficulty-normal' },
  50: { text: '艰深', className: 'difficulty-hard' },
  100: { text: '天命', className: 'difficulty-extreme' }
}

const STATUS_META = {
  claimable: { text: '可领取', className: 'success' },
  claimed: { text: '已领取', className: 'neutral' },
  in_progress: { text: '进行中', className: 'info' },
  not_started: { text: '未开始', className: 'neutral' }
}

const REQUIREMENT_META = {
  0: { text: '等级', icon: ICON.misc_growth },
  1: { text: '击杀', icon: ICON.misc_sword_crossed },
  2: { text: '胜场', icon: ICON.misc_medal },
  3: { text: '强化', icon: ICON.ui_gear },
  4: { text: '装备收集', icon: ICON.misc_dagger },
  5: { text: '道具收集', icon: ICON.misc_backpack },
  6: { text: '金币', icon: ICON.misc_coin },
  7: { text: '灵石消耗', icon: ICON.misc_diamond },
  8: { text: '登录', icon: ICON.misc_scroll },
  9: { text: '连续登录', icon: ICON.misc_calendar },
  10: { text: '任务', icon: ICON.ui_target },
  11: { text: '副本', icon: ICON.misc_castle },
  12: { text: '无伤战斗', icon: ICON.misc_shield },
  13: { text: '连击', icon: ICON.misc_sparkles },
  14: { text: '总伤害', icon: ICON.misc_fire },
  15: { text: '总修为', icon: ICON.misc_fire },
  16: { text: '承伤', icon: ICON.misc_brick },
  17: { text: '炼丹成功', icon: ICON.nav_alchemy },
  18: { text: '炼丹失败', icon: ICON.misc_crystal_ball },
  19: { text: '锻造成功', icon: ICON.nav_forge },
  20: { text: '锻造失败', icon: ICON.misc_brick },
  21: { text: '强化成功', icon: ICON.misc_sparkles },
  22: { text: '强化失败', icon: ICON.misc_crystal_ball },
  23: { text: '种植', icon: ICON.misc_seedling },
  24: { text: '收获', icon: ICON.misc_seedling },
  25: { text: '副本胜利', icon: ICON.misc_castle },
  26: { text: '野怪击杀', icon: ICON.creature_demon },
  27: { text: '技能使用', icon: ICON.item_book },
  28: { text: '蓝量消耗', icon: ICON.misc_water_drop },
  29: { text: '暴击', icon: ICON.stat_critical },
  30: { text: '死亡', icon: ICON.creature_demon },
  31: { text: '闪避', icon: ICON.stat_dodge },
  32: { text: '技能伤害', icon: ICON.misc_fire }
}

const REWARD_META = {
  Title: { icon: ICON.misc_medal, label: '称号' },
  Exp: { icon: ICON.misc_sparkles, label: '修为', prefix: '+' },
  Gold: { icon: ICON.misc_coin, label: '金币', prefix: '+' },
  SpiritStone: { icon: ICON.misc_diamond, label: '灵石', prefix: '+' },
  Honor: { icon: ICON.misc_medal, label: '荣誉', prefix: '+' },
  GuildContribution: { icon: ICON.misc_castle, label: '宗门贡献', prefix: '+' },
  Item: { icon: ICON.misc_gift, label: '物品', prefix: 'x' },
  Equipment: { icon: ICON.misc_dagger, label: '装备', prefix: 'x' },
  Buff: { icon: ICON.misc_water_drop, label: '特效', prefix: '' }
}

function toNumber(value, fallback = 0) {
  const numericValue = Number(value)
  return Number.isFinite(numericValue) ? numericValue : fallback
}

function clampPercent(value) {
  return Math.max(0, Math.min(100, Number(value) || 0))
}

function resolveAchievementStatus(rawAchievement, currentProgress) {
  const statusValue = toNumber(rawAchievement?.StatusValue ?? rawAchievement?.statusValue, 0)
  const isRewardClaimed = Boolean(rawAchievement?.IsRewardClaimed ?? rawAchievement?.isRewardClaimed)
  const isCompleted = Boolean(rawAchievement?.IsCompleted ?? rawAchievement?.isCompleted)

  if (isRewardClaimed || statusValue === 3) {
    return 'claimed'
  }

  if (isCompleted || statusValue === 2) {
    return 'claimable'
  }

  if (statusValue === 1 || currentProgress > 0) {
    return 'in_progress'
  }

  return 'not_started'
}

function buildRewardEntries(rewardSource) {
  if (Array.isArray(rewardSource) && rewardSource.length > 0) {
    return rewardSource.map((reward, index) => {
      const rewardType = reward?.RewardType ?? reward?.rewardType ?? 'Item'
      const meta = REWARD_META[rewardType] || REWARD_META.Item
      const amount = toNumber(reward?.Amount ?? reward?.amount, 0)
      const itemId = reward?.ItemId ?? reward?.itemId ?? ''
      const equipmentId = reward?.EquipmentId ?? reward?.equipmentId ?? ''

      let label = reward?.Name ?? reward?.name ?? meta.label
      let value = `${meta.prefix || ''}${amount}`

      if (rewardType === 'Title') {
        label = meta.label
        value = itemId || reward?.Name || '未知称号'
      } else if (rewardType === 'Item') {
        label = itemId || label
      } else if (rewardType === 'Equipment') {
        label = reward?.Name ?? `装备 ${equipmentId}`
        value = 'x1'
      } else if (rewardType === 'Buff') {
        value = itemId || reward?.Name || 'Buff'
      }

      return {
        key: `${rewardType}-${itemId || equipmentId || index}`,
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
      value: `${meta.prefix || ''}${numericAmount}`
    })
  }

  if (reward.Title || reward.title) {
    entries.push({
      key: 'title',
      icon: REWARD_META.Title.icon,
      label: REWARD_META.Title.label,
      value: reward.Title || reward.title
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

  const buffs = reward.Buffs || reward.buffs || []
  buffs.forEach((buffId) => {
    entries.push({
      key: `buff-${buffId}`,
      icon: REWARD_META.Buff.icon,
      label: REWARD_META.Buff.label,
      value: buffId
    })
  })

  if (entries.length === 0) {
    entries.push({
      key: 'none',
      icon: ICON.misc_feather,
      label: '暂无奖励',
      value: '无'
    })
  }

  return entries
}

function normalizeRequirement(requirement, index) {
  // 中文注释：
  // requirement 同时兼容 overview/progress 两种返回结构，
  // 统一拍平成详情面板直接可消费的字段。
  const typeValue = toNumber(
    requirement?.RequirementTypeValue ?? requirement?.requirementTypeValue ?? requirement?.RequirementType ?? requirement?.requirementType,
    0
  )
  const meta = REQUIREMENT_META[typeValue] || REQUIREMENT_META[0]
  const currentProgress = toNumber(requirement?.CurrentProgress ?? requirement?.currentProgress, 0)
  const targetProgress = Math.max(1, toNumber(requirement?.TargetProgress ?? requirement?.targetProgress ?? requirement?.TargetValue ?? requirement?.targetValue, 1))
  const progressPercent = clampPercent(
    requirement?.ProgressPercent ?? requirement?.progressPercent ?? (currentProgress * 100 / targetProgress)
  )

  return {
    index: toNumber(requirement?.Index ?? requirement?.index, index),
    icon: requirement?.Icon || requirement?.icon || meta.icon,
    typeText: meta.text,
    currentProgress,
    targetProgress,
    progressPercent,
    isCompleted: Boolean(requirement?.IsCompleted ?? requirement?.isCompleted ?? currentProgress >= targetProgress),
    description: requirement?.Description ?? requirement?.description ?? '完成对应成就条件'
  }
}

function normalizeAchievement(rawAchievement) {
  // 中文注释：
  // 单条成就会把条件、奖励、状态文本和总进度一次性整理好，
  // 列表区和详情区都复用这份结构，避免双份映射带来显示漂移。
  const requirements = Array.isArray(rawAchievement?.Requirements || rawAchievement?.requirements)
    ? (rawAchievement?.Requirements || rawAchievement?.requirements).map((requirement, index) => normalizeRequirement(requirement, index))
    : []

  const requirementTargetTotal = requirements.reduce((sum, requirement) => sum + requirement.targetProgress, 0)
  const requirementCurrentTotal = requirements.reduce((sum, requirement) => sum + Math.min(requirement.currentProgress, requirement.targetProgress), 0)
  const currentProgress = toNumber(rawAchievement?.CurrentProgress ?? rawAchievement?.currentProgress, requirementCurrentTotal)
  const targetProgress = Math.max(toNumber(rawAchievement?.TargetProgress ?? rawAchievement?.targetProgress, requirementTargetTotal), requirementTargetTotal)
  const progressPercent = clampPercent(targetProgress > 0 ? currentProgress * 100 / targetProgress : 0)
  const typeValue = toNumber(rawAchievement?.TypeValue ?? rawAchievement?.typeValue ?? rawAchievement?.AchievementType ?? rawAchievement?.achievementType, 0)
  const difficultyValue = toNumber(rawAchievement?.DifficultyValue ?? rawAchievement?.difficultyValue ?? rawAchievement?.Difficulty ?? rawAchievement?.difficulty, 25)
  const typeMeta = ACHIEVEMENT_TYPE_META[typeValue] || ACHIEVEMENT_TYPE_META[1]
  const difficultyMeta = DIFFICULTY_META[difficultyValue] || DIFFICULTY_META[25]
  const statusKey = resolveAchievementStatus(rawAchievement, currentProgress)
  const statusMeta = STATUS_META[statusKey]
  const isHidden = Boolean(rawAchievement?.IsHidden ?? rawAchievement?.isHidden)
  const shouldMask = isHidden && statusKey === 'not_started'

  return {
    achievementId: rawAchievement?.AchievementId ?? rawAchievement?.achievementId ?? '',
    name: shouldMask ? '未显化成就' : (rawAchievement?.Name ?? rawAchievement?.name ?? rawAchievement?.AchievementName ?? rawAchievement?.achievementName ?? '未命名成就'),
    description: shouldMask ? '机缘未至，暂不可窥见全貌。' : (rawAchievement?.Description ?? rawAchievement?.description ?? '暂无成就描述。'),
    icon: shouldMask ? '✦' : (rawAchievement?.Icon || rawAchievement?.icon || typeMeta.icon),
    typeText: typeMeta.text,
    difficultyText: difficultyMeta.text,
    difficultyClass: difficultyMeta.className,
    category: shouldMask ? '隐世' : (rawAchievement?.Category ?? rawAchievement?.category ?? '未分类'),
    points: toNumber(rawAchievement?.Points ?? rawAchievement?.points, 0),
    currentProgress,
    targetProgress,
    progressPercent,
    isHidden,
    statusKey,
    statusText: statusMeta.text,
    statusClass: statusMeta.className,
    completeTime: rawAchievement?.CompleteTime ?? rawAchievement?.completeTime ?? null,
    claimTime: rawAchievement?.ClaimTime ?? rawAchievement?.claimTime ?? null,
    sortOrder: toNumber(rawAchievement?.SortOrder ?? rawAchievement?.sortOrder, 0),
    requirements,
    rewards: buildRewardEntries(rawAchievement?.Rewards ?? rawAchievement?.rewards)
  }
}

export default {
  name: 'AchievementModal',

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
      { key: 'claimable', label: '可领取', icon: ICON.misc_gift },
      { key: 'progress', label: '进行中', icon: ICON.misc_fire },
      { key: 'all', label: '全部', icon: ICON.misc_books },
      { key: 'claimed', label: '已领取', icon: ICON.misc_checkmark }
    ]

    const currentTab = ref('claimable')
    const achievements = ref([])
    const selectedAchievementId = ref('')
    const isLoading = ref(false)
    const isSubmitting = ref(false)
    const statusMessage = ref('')

    const stats = reactive({
      totalAchievements: 0,
      completedCount: 0,
      inProgressCount: 0,
      completionPercent: 0,
      totalPoints: 0
    })

    const currentList = computed(() => {
      if (currentTab.value === 'claimable') {
        return achievements.value.filter((achievement) => achievement.statusKey === 'claimable')
      }

      if (currentTab.value === 'progress') {
        return achievements.value.filter((achievement) => achievement.statusKey === 'in_progress')
      }

      if (currentTab.value === 'claimed') {
        return achievements.value.filter((achievement) => achievement.statusKey === 'claimed')
      }

      return achievements.value
    })

    const selectedAchievement = computed(() => {
      return currentList.value.find((achievement) => achievement.achievementId === selectedAchievementId.value) || null
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

    async function loadAchievementData() {
      isLoading.value = true

      try {
        // 中文注释：
        // 成就页需要把 overview 和 stats 合并使用：
        // overview 负责列表与详情，stats 负责顶部总览数字，缺一项都会让页面信息不完整。
        const [overview, achievementStats] = await Promise.all([
          apiClient.getAchievementOverview(),
          apiClient.getAchievementStats()
        ])

        achievements.value = (Array.isArray(overview) ? overview : [])
          .map((achievement) => normalizeAchievement(achievement))
          .sort((left, right) => {
            if (left.statusKey === 'claimable' && right.statusKey !== 'claimable') return -1
            if (left.statusKey !== 'claimable' && right.statusKey === 'claimable') return 1
            if (left.sortOrder !== right.sortOrder) return left.sortOrder - right.sortOrder
            return right.points - left.points
          })

        stats.totalAchievements = toNumber(achievementStats?.TotalAchievements ?? achievementStats?.totalAchievements, achievements.value.length)
        stats.completedCount = toNumber(achievementStats?.CompletedCount ?? achievementStats?.completedCount, 0)
        stats.inProgressCount = toNumber(achievementStats?.InProgressCount ?? achievementStats?.inProgressCount, 0)
        stats.completionPercent = Number(achievementStats?.CompletionPercent ?? achievementStats?.completionPercent ?? 0) || 0
        stats.totalPoints = toNumber(achievementStats?.TotalPoints ?? achievementStats?.totalPoints, 0)
        statusMessage.value = ''
      } catch (error) {
        statusMessage.value = error.message || '加载成就数据失败。'
      } finally {
        isLoading.value = false
      }
    }

    async function claimSelectedAchievement() {
      if (!selectedAchievement.value || selectedAchievement.value.statusKey !== 'claimable' || isSubmitting.value) {
        return
      }

      isSubmitting.value = true

      try {
        // 中文注释：
        // 领取成就奖励后，玩家资源、背包、装备战力和排行榜都可能变化，
        // 所以这里先刷新玩家快照，再重新拉成就数据，保证两个面板同步更新。
        const result = await apiClient.claimAchievementReward(selectedAchievement.value.achievementId)
        statusMessage.value = result?.message || result?.Message || '成就奖励已领取。'
        await gameStore.refreshPlayerSnapshot({
          includeInventory: true,
          includeEquipment: true,
          includeRankings: true
        })
        await loadAchievementData()
      } catch (error) {
        statusMessage.value = error.message || '领取成就奖励失败。'
      } finally {
        isSubmitting.value = false
      }
    }

    watch(currentList, (list) => {
      // 中文注释：
      // 切换标签或重新加载后，如果旧选中项不在当前列表里，
      // 自动落到第一项，避免右侧详情面板指向已失效的数据。
      if (list.some((achievement) => achievement.achievementId === selectedAchievementId.value)) {
        return
      }

      selectedAchievementId.value = list[0]?.achievementId || ''
    }, { immediate: true })

    watch(() => props.modelValue, async (visible) => {
      if (!visible) {
        return
      }

      // 中文注释：
      // 成就弹窗按打开时懒加载，避免首页阶段提前请求成就概览和统计接口。
      await loadAchievementData()
    })

    return {
      ICON,
      tabs,
      currentTab,
      achievements,
      selectedAchievementId,
      selectedAchievement,
      currentList,
      stats,
      isLoading,
      isSubmitting,
      statusMessage,
      formatDateTime,
      formatProgress,
      loadAchievementData,
      claimSelectedAchievement
    }
  }
}
</script>

<style scoped>
.achievement-modal {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
  color: var(--text-primary);
}

.stats-grid,
.detail-grid {
  display: grid;
  gap: var(--spacing-sm);
}

.stats-grid {
  grid-template-columns: repeat(4, minmax(0, 1fr));
}

.stat-card,
.list-panel,
.detail-panel,
.achievement-item,
.panel-card {
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
}

.stat-card {
  padding: var(--spacing-sm) var(--spacing-md);
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.stat-label,
.achievement-meta,
.achievement-description,
.achievement-footer,
.detail-subtitle,
.reward-value,
.requirement-meta,
.meta-strip,
.hint-text,
.progress-row {
  color: var(--text-secondary);
}

.stat-value {
  color: var(--highlight-text);
}

.toolbar-row {
  display: flex;
  justify-content: space-between;
  gap: var(--spacing-md);
  align-items: center;
}

.tab-row {
  display: flex;
  flex-wrap: wrap;
  gap: var(--spacing-sm);
}

.tab-btn,
.refresh-btn,
.primary-btn,
.achievement-item {
  transition: all 0.25s ease;
}

.tab-btn {
  display: inline-flex;
  align-items: center;
  gap: var(--spacing-xs);
  padding: var(--spacing-sm) var(--spacing-md);
  background: transparent;
  border: 1px solid var(--border-color);
  border-radius: 999px;
  color: var(--text-secondary);
  cursor: pointer;
}

.tab-btn:hover,
.tab-btn.active {
  background: var(--accent-soft-bg);
  border-color: var(--accent-text);
  color: var(--text-primary);
  box-shadow: var(--shadow-sm);
}

.refresh-btn,
.primary-btn {
  padding: var(--spacing-sm) var(--spacing-md);
  border-radius: var(--radius-sm);
  border: 1px solid transparent;
  cursor: pointer;
  font-family: var(--font-primary);
}

.refresh-btn {
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
.primary-btn:disabled {
  opacity: 0.55;
  cursor: not-allowed;
}

.achievement-layout {
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

.achievement-item {
  text-align: left;
  color: inherit;
  cursor: pointer;
  padding: var(--spacing-md);
}

.achievement-item:hover,
.achievement-item.selected {
  border-color: var(--accent-text);
  background: var(--accent-soft-bg);
  transform: translateY(-1px);
}

.achievement-header,
.detail-header,
.achievement-main,
.title-row,
.reward-item,
.requirement-item,
.reward-content,
.requirement-content,
.requirement-header {
  display: flex;
}

.achievement-header,
.detail-header,
.requirement-header {
  justify-content: space-between;
  align-items: flex-start;
  gap: var(--spacing-sm);
}

.achievement-main,
.title-row,
.reward-item,
.requirement-item {
  gap: var(--spacing-sm);
}

.achievement-icon,
.detail-icon {
  font-size: 24px;
}

.achievement-headline {
  min-width: 0;
}

.achievement-name,
.detail-title,
.requirement-name,
.reward-name {
  color: var(--text-primary);
  margin: 0;
}

.achievement-description {
  margin-top: var(--spacing-sm);
  line-height: 1.6;
}

.achievement-footer {
  margin-top: var(--spacing-sm);
  display: flex;
  gap: var(--spacing-md);
  font-size: var(--font-size-sm);
}

.status-badge,
.requirement-state {
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
  color: var(--text-primary);
  line-height: 1.8;
}

.detail-grid {
  grid-template-columns: repeat(2, minmax(0, 1fr));
}

.panel-card {
  padding: var(--spacing-md);
}

.panel-title {
  margin-bottom: var(--spacing-md);
  color: var(--highlight-text);
  font-weight: 600;
}

.requirement-list,
.reward-list {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
}

.requirement-item,
.reward-item {
  align-items: flex-start;
  padding: var(--spacing-sm);
  background: rgba(255, 255, 255, 0.03);
  border-radius: var(--radius-sm);
}

.requirement-item.completed {
  border: 1px solid var(--control-border);
  background: var(--accent-soft-bg);
}

.reward-content,
.requirement-content {
  flex: 1;
  flex-direction: column;
  gap: 6px;
  min-width: 0;
}

.requirement-state {
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
  justify-content: flex-end;
}

.primary-btn {
  min-width: 136px;
  background: var(--button-primary-start);
  color: var(--button-primary-text);
}

.primary-btn:hover:not(:disabled) {
  background: var(--button-primary-start);
  opacity: 0.85;
  color: var(--button-primary-hover-text);
  box-shadow: var(--shadow-sm);
}

.empty-state,
.detail-empty {
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
  .stats-grid,
  .achievement-layout,
  .detail-grid,
  .toolbar-row {
    grid-template-columns: 1fr;
    display: grid;
  }

  .action-row {
    justify-content: stretch;
  }

  .primary-btn {
    width: 100%;
  }
}
</style>
