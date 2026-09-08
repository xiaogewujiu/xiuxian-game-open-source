<template>
  <!-- 签到系统 Modal -->
  <XiuXianModal :model-value="modelValue" title="每日签到" :width="500" @update:model-value="$emit('update:modelValue', $event)">
    <div class="checkin-modal">
      <!-- 连续签到信息 -->
      <div class="streak-info">
        <div class="streak-count">
          <span class="streak-number">{{ consecutiveDays }}</span>
          <span class="streak-label">天</span>
        </div>
        <div class="streak-text">连续签到</div>
      </div>

      <!-- 月历视图 -->
      <div class="calendar">
        <div class="calendar-header">
          <button class="nav-btn" @click="prevMonth">‹</button>
          <span class="current-month">{{ currentYear }}年 {{ currentMonth }}月</span>
          <button class="nav-btn" @click="nextMonth">›</button>
        </div>

        <div class="weekdays">
          <span v-for="day in weekDays" :key="day">{{ day }}</span>
        </div>

        <div class="days-grid">
          <div
            v-for="(day, index) in calendarDays"
            :key="index"
            class="day-cell"
            :class="{
              'other-month': !day.isCurrentMonth,
              'today': day.isToday,
              'checked': day.isChecked,
              'future': day.isFuture
            }"
          >
            <span class="day-number">{{ day.date }}</span>
            <span v-if="day.isChecked" class="check-mark">✓</span>
            <span v-if="day.reward" class="reward-icon">
              <AssetIcon :source="day.reward" size="14" />
            </span>
          </div>
        </div>
      </div>

      <!-- 今日签到奖励 -->
      <div class="today-reward">
        <div class="reward-title">今日签到奖励</div>
        <div class="reward-items">
          <div v-for="(reward, index) in todayRewards" :key="index" class="reward-item">
            <span class="reward-icon"><AssetIcon :source="reward.icon" size="20" /></span>
            <span class="reward-name">{{ reward.name }}</span>
            <span class="reward-count">x{{ reward.count }}</span>
          </div>
        </div>
      </div>

      <!-- 签到按钮 -->
      <div class="checkin-action">
        <button
          class="checkin-btn"
          :class="{ checked: hasCheckedToday }"
          :disabled="hasCheckedToday || isLoading || !isCurrentMonthView"
          @click="doCheckIn"
        >
          {{ isLoading ? '处理中...' : (!isCurrentMonthView ? '历史记录' : (hasCheckedToday ? '今日已签到' : '立即签到')) }}
        </button>
        <div v-if="statusMessage" class="status-message">{{ statusMessage }}</div>
      </div>

      <!-- 累计奖励展示 -->
      <div class="cumulative-rewards">
        <div class="cumulative-title">累计签到奖励</div>
        <div class="reward-progress">
          <div
            v-for="milestone in cumulativeRewards"
            :key="milestone.days"
            class="milestone"
            :class="{ achieved: consecutiveDays >= milestone.days }"
          >
            <div class="milestone-day">{{ milestone.days }}天</div>
            <div class="milestone-reward">{{ milestone.reward }}</div>
            <div v-if="consecutiveDays >= milestone.days" class="achieved-mark">✓</div>
          </div>
        </div>
      </div>
    </div>
  </XiuXianModal>
</template>

<script>
import { ref, computed, watch } from 'vue'
import XiuXianModal from '../common/XiuXianModal.vue'
import { apiClient } from '../../lib/apiClient'
import { useGameStore } from '../../state/gameStore'
import { ICON } from '../../icons'
import AssetIcon from '../common/AssetIcon.vue'

/**
 * CheckInModal - 签到系统弹窗
 *
 * 功能：
 * - 月历视图显示签到记录
 * - 今日高亮显示
 * - 签到按钮
 * - 连续签到奖励展示
 */
export default {
  name: 'CheckInModal',

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
    const consecutiveDays = ref(0)
    const totalDays = ref(0)
    const hasCheckedToday = ref(false)
    const checkedDateKeys = ref(new Set())
    const isLoading = ref(false)
    const isCurrentMonthView = ref(true)
    const statusMessage = ref('')

    // 日历相关
    const currentDate = ref(new Date())
    const currentYear = computed(() => currentDate.value.getFullYear())
    const currentMonth = computed(() => currentDate.value.getMonth() + 1)

    const weekDays = ['日', '一', '二', '三', '四', '五', '六']

    // 生成月历数据
    const calendarDays = computed(() => {
      const year = currentDate.value.getFullYear()
      const month = currentDate.value.getMonth()
      const today = new Date()

      const firstDay = new Date(year, month, 1)
      const lastDay = new Date(year, month + 1, 0)
      const prevLastDay = new Date(year, month, 0)

      const days = []
      const startPadding = firstDay.getDay()

      // 上月日期
      for (let i = startPadding - 1; i >= 0; i--) {
        const date = new Date(year, month - 1, prevLastDay.getDate() - i)
        days.push({
          date: prevLastDay.getDate() - i,
          isCurrentMonth: false,
          isToday: false,
          isChecked: checkedDateKeys.value.has(formatDateKey(date)),
          isFuture: false,
          reward: null
        })
      }

      // 当月日期
      for (let i = 1; i <= lastDay.getDate(); i++) {
        const currentCellDate = new Date(year, month, i)
        const dateKey = formatDateKey(currentCellDate)
        const isToday = year === today.getFullYear() &&
          month === today.getMonth() &&
          i === today.getDate()
        const isFuture = new Date(year, month, i) > today

        days.push({
          date: i,
          isCurrentMonth: true,
          isToday,
          isChecked: checkedDateKeys.value.has(dateKey),
          isFuture,
          reward: isToday && !hasCheckedToday.value ? ICON.misc_gift : null
        })
      }

      return days
    })

    const todayRewards = ref([])
    const cumulativeRewards = ref([])

    function formatDateKey(date) {
      const year = date.getFullYear()
      const month = String(date.getMonth() + 1).padStart(2, '0')
      const day = String(date.getDate()).padStart(2, '0')
      return `${year}-${month}-${day}`
    }

    function getRewardIcon(type) {
      return {
        gold: ICON.misc_coin,
        exp: ICON.misc_sparkles,
        spiritStone: ICON.misc_diamond,
        item: ICON.misc_gift
      }[type] || ICON.misc_gift
    }

    function normalizeReward(reward) {
      return {
        icon: getRewardIcon(reward.type),
        name: reward.name,
        count: reward.count
      }
    }

    function applyStatus(status) {
      const now = new Date()

      consecutiveDays.value = status?.continuousDays || 0
      totalDays.value = status?.totalDays || 0
      hasCheckedToday.value = Boolean(status?.hasCheckedToday)
      isCurrentMonthView.value = status?.year === now.getFullYear() && status?.month === now.getMonth() + 1
      checkedDateKeys.value = new Set(status?.checkedDates || [])
      todayRewards.value = isCurrentMonthView.value
        ? (status?.todayRewards || []).map(normalizeReward)
        : []
      cumulativeRewards.value = (status?.milestones || []).map((milestone) => ({
        days: milestone.days,
        reward: milestone.rewardSummary,
        achieved: Boolean(milestone.isReached)
      }))
      statusMessage.value = isCurrentMonthView.value
        ? (status?.hintMessage || '')
        : '当前查看的是历史月份记录，仅用于查看当月签到情况。'
    }

    async function loadStatus() {
      isLoading.value = true

      try {
        // 中文注释：
        // 签到面板只在真正打开时请求接口，切换月份时也只刷新当前面板所需的月份数据。
        // 这样既满足首页懒加载要求，也能保证月历展示的每个勾选日期都来自真实数据库记录。
        const status = await apiClient.getCheckInStatus(currentYear.value, currentMonth.value)
        applyStatus(status)
      } catch (error) {
        statusMessage.value = error.message || '加载签到信息失败。'
      } finally {
        isLoading.value = false
      }
    }

    // 切换月份
    const prevMonth = async () => {
      currentDate.value = new Date(currentDate.value.getFullYear(), currentDate.value.getMonth() - 1, 1)
      await loadStatus()
    }

    const nextMonth = async () => {
      currentDate.value = new Date(currentDate.value.getFullYear(), currentDate.value.getMonth() + 1, 1)
      await loadStatus()
    }

    // 执行签到
    const doCheckIn = async () => {
      if (hasCheckedToday.value || isLoading.value || !isCurrentMonthView.value) return

      isLoading.value = true

      try {
        const result = await apiClient.claimCheckIn()
        applyStatus(result?.status)

        // 中文注释：
        // 签到奖励既可能发到人物货币，也可能发到背包物品。
        // 因此这里把玩家信息和背包都刷新一次，避免用户刚签到完还看到旧数值。
        await Promise.allSettled([
          gameStore.loadPlayer(true),
          gameStore.loadInventory(true)
        ])

        const rewardText = (result?.rewards || [])
          .map((reward) => `${reward.name} x${reward.count}`)
          .join('、')

        statusMessage.value = rewardText
          ? `${result?.message || '签到成功。'} 获得：${rewardText}`
          : (result?.message || '签到成功。')
      } catch (error) {
        statusMessage.value = error.message || '签到失败。'
      } finally {
        isLoading.value = false
      }
    }

    watch(() => props.modelValue, async (visible) => {
      if (!visible) {
        return
      }

      currentDate.value = new Date()
      await loadStatus()
    }, { immediate: true })

    return {
      ICON,
      consecutiveDays,
      totalDays,
      hasCheckedToday,
      isLoading,
      isCurrentMonthView,
      statusMessage,
      currentYear,
      currentMonth,
      weekDays,
      calendarDays,
      todayRewards,
      cumulativeRewards,
      prevMonth,
      nextMonth,
      doCheckIn
    }
  }
}
</script>

<style scoped>
.checkin-modal {
  padding: var(--spacing-md);
}

/* 连续签到信息 */
.streak-info {
  text-align: center;
  margin-bottom: var(--spacing-lg);
  padding: var(--spacing-lg);
  background: linear-gradient(135deg, rgba(212, 168, 83, 0.2), rgba(124, 58, 237, 0.2));
  border-radius: var(--radius-md);
  border: 1px solid var(--border-color);
}

.streak-count {
  display: flex;
  align-items: baseline;
  justify-content: center;
  gap: 4px;
}

.streak-number {
  font-size: 48px;
  font-weight: 700;
  color: var(--highlight-text);
}

.streak-label {
  font-size: 18px;
  color: var(--text-secondary);
}

.streak-text {
  font-size: 14px;
  color: var(--accent-text);
  margin-top: var(--spacing-xs);
}

/* 日历 */
.calendar {
  background: var(--xiuxian-bg-secondary);
  border-radius: var(--radius-md);
  padding: var(--spacing-md);
  margin-bottom: var(--spacing-lg);
}

.calendar-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--spacing-md);
}

.nav-btn {
  width: 32px;
  height: 32px;
  background: transparent;
  border: 1px solid var(--border-color);
  border-radius: var(--radius-sm);
  color: var(--text-secondary);
  cursor: pointer;
  font-size: 18px;
  transition: all 0.15s ease;
}

.nav-btn:hover {
  background: var(--button-outline-hover-bg);
  border-color: var(--button-outline-hover-border);
  color: var(--button-outline-hover-text);
}

.current-month {
  font-size: 16px;
  font-weight: 600;
  color: var(--text-primary);
}

.weekdays {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  gap: 4px;
  margin-bottom: var(--spacing-sm);
}

.weekdays span {
  text-align: center;
  font-size: var(--font-size-base);
  color: var(--text-muted);
  padding: var(--spacing-xs);
}

.days-grid {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  gap: 4px;
}

.day-cell {
  aspect-ratio: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  background: var(--xiuxian-bg-primary);
  border-radius: var(--radius-sm);
  position: relative;
  cursor: pointer;
  transition: all 0.2s ease;
}

.day-cell:hover:not(.future) {
  background: rgba(124, 58, 237, 0.2);
}

.day-cell.other-month {
  opacity: 0.3;
}

.day-cell.today {
  border: 2px solid var(--highlight-text);
  box-shadow: var(--shadow-sm);
}

.day-cell.checked {
  background: rgba(76, 175, 80, 0.2);
}

.day-cell.future {
  opacity: 0.3;
  cursor: default;
}

.day-number {
  font-size: var(--font-size-base);
  color: var(--text-primary);
}

.check-mark {
  position: absolute;
  bottom: 2px;
  font-size: var(--font-size-xs);
  color: var(--quality-uncommon);
}

.reward-icon {
  position: absolute;
  top: 2px;
  font-size: var(--font-size-xs);
}

/* 今日奖励 */
.today-reward {
  background: rgba(212, 168, 83, 0.1);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  padding: var(--spacing-md);
  margin-bottom: var(--spacing-md);
}

.reward-title {
  font-size: var(--font-size-base);
  font-weight: 600;
  color: var(--highlight-text);
  margin-bottom: var(--spacing-sm);
}

.reward-items {
  display: flex;
  gap: var(--spacing-md);
}

.reward-item {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: var(--spacing-sm);
  background: var(--xiuxian-bg-primary);
  border-radius: var(--radius-sm);
}

.reward-item .reward-icon {
  font-size: 18px;
}

.reward-item .reward-name {
  font-size: var(--font-size-base);
  color: var(--text-primary);
}

.reward-item .reward-count {
  font-size: var(--font-size-base);
  color: var(--highlight-text);
  font-weight: 600;
}

/* 签到按钮 */
.checkin-action {
  margin-bottom: var(--spacing-lg);
}

.status-message {
  margin-top: var(--spacing-sm);
  font-size: var(--font-size-base);
  color: var(--text-secondary);
  text-align: center;
  line-height: 1.5;
}

.checkin-btn {
  width: 100%;
  padding: var(--spacing-md);
  background: var(--button-primary-start);
  border: none;
  border-radius: var(--radius-sm);
  color: var(--text-on-dark);
  font-size: var(--font-size-md);
  font-weight: 600;
  cursor: pointer;
  transition: all 0.15s ease;
}

.checkin-btn:hover:not(:disabled) {
  box-shadow: var(--shadow-sm);
  transform: translateY(-2px);
}

.checkin-btn.checked {
  background: var(--xiuxian-bg-primary);
  color: var(--text-muted);
  cursor: default;
}

.checkin-btn:disabled {
  cursor: not-allowed;
}

/* 累计奖励 */
.cumulative-rewards {
  border-top: 1px solid var(--border-color);
  padding-top: var(--spacing-md);
}

.cumulative-title {
  font-size: var(--font-size-base);
  font-weight: 600;
  color: var(--accent-text);
  margin-bottom: var(--spacing-md);
}

.reward-progress {
  display: flex;
  justify-content: space-around;
}

.milestone {
  text-align: center;
  padding: var(--spacing-md);
  background: var(--xiuxian-bg-secondary);
  border-radius: var(--radius-md);
  min-width: 80px;
  position: relative;
  border: 2px solid transparent;
}

.milestone.achieved {
  border-color: var(--highlight-text);
  background: rgba(212, 168, 83, 0.1);
}

.milestone-day {
  font-size: 16px;
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 4px;
}

.milestone-reward {
  font-size: var(--font-size-base);
  color: var(--accent-text);
}

.achieved-mark {
  position: absolute;
  top: -8px;
  right: -8px;
  width: 20px;
  height: 20px;
  background: #4caf50;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: var(--font-size-base);
  color: white;
}
</style>
