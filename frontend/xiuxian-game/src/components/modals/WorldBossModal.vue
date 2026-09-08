<template>
  <XiuXianModal :model-value="modelValue" title="世界 Boss" :width="1160" @update:model-value="$emit('update:modelValue', $event)">
    <div class="wb-modal">
      <div class="wb-topbar">
        <div class="wb-topbar-copy">
          <div class="wb-kicker">世界Boss</div>
          <div class="wb-title-row">
            <h2 class="wb-title">{{ current.instance?.bossName || '当前无世界 Boss' }}</h2>
            <span class="wb-state" :class="{ active: current.hasActiveBoss }">
              {{ current.instance?.state || '空闲' }}
            </span>
          </div>
          <div class="wb-meta">
            <span>剩余 {{ current.instance?.remainingSeconds || 0 }} 秒</span>
            <span>参战 {{ current.instance?.participantCount || 0 }} 人</span>
            <span v-if="current.hasPendingReward">有待领取奖励</span>
          </div>
        </div>

        <div class="wb-actions">
          <button class="wb-btn" :disabled="loading" @click="refreshAll">刷新</button>
          <button
            v-if="current.hasPendingReward"
            class="wb-btn wb-btn--reward"
            :disabled="loading"
            @click="claimReward"
          >
            领取奖励
          </button>
        </div>
      </div>

      <div v-if="!current.hasActiveBoss" class="wb-empty">
        <div class="wb-empty-title">当前没有开启中的世界 Boss</div>
        <div class="wb-empty-text">请先在管理端生成 Boss，随后这里会自动显示战斗信息、日志和排行。</div>
      </div>

      <div v-else class="wb-layout">
        <section class="wb-main">
          <div class="wb-card wb-boss-card">
            <div class="wb-boss-grid">
              <div class="wb-portrait-panel">
                <div
                  class="wb-portrait-frame"
                  @mouseenter="showBossTooltip = true"
                  @mouseleave="showBossTooltip = false"
                >
                  <AssetIcon
                    :source="current.boss?.portraitPath || current.instance?.portraitPath || ''"
                    :fallback="ICON.creature_demon"
                    :alt="current.boss?.name || '世界Boss'"
                    class="wb-portrait"
                  />

                  <div v-if="showBossTooltip && current.boss" class="wb-tooltip">
                    <div class="wb-tooltip-title">{{ current.boss.name }}</div>
                    <div class="wb-tooltip-grid">
                      <div>物攻：{{ current.boss.attributes?.physicalAttack || 0 }}</div>
                      <div>法攻：{{ current.boss.attributes?.magicAttack || 0 }}</div>
                      <div>物防：{{ current.boss.attributes?.physicalDefense || 0 }}</div>
                      <div>法防：{{ current.boss.attributes?.magicDefense || 0 }}</div>
                      <div>速度：{{ current.boss.attributes?.speed || 0 }}</div>
                      <div>元素：{{ current.boss.attributes?.elementText || '-' }}</div>
                      <div>命中：{{ toPercent(current.boss.attributes?.hitRate) }}</div>
                      <div>闪避：{{ toPercent(current.boss.attributes?.dodgeRate) }}</div>
                      <div>暴击：{{ toPercent(current.boss.attributes?.critRate) }}</div>
                      <div>暴伤：{{ toPercent(current.boss.attributes?.critDamage) }}</div>
                      <div>连击：{{ toPercent(current.boss.attributes?.comboRate) }}</div>
                      <div>反击：{{ toPercent(current.boss.attributes?.counterRate) }}</div>
                    </div>
                  </div>
                </div>
              </div>

              <div class="wb-boss-info">
                <div class="wb-section-title">Boss 状态</div>

                <div class="wb-bar-card hp">
                  <div class="wb-bar-head">
                    <span>气血</span>
                    <strong>{{ displayBossCurrentHp }}/{{ current.boss?.maxHp || 0 }}</strong>
                  </div>
                  <div class="wb-track">
                    <div class="wb-fill hp" :style="{ width: bossHpPercent + '%' }"></div>
                  </div>
                </div>

                <div class="wb-bar-card mp">
                  <div class="wb-bar-head">
                    <span>灵力</span>
                    <strong>{{ displayBossCurrentMp }}/{{ current.boss?.maxMp || 0 }}</strong>
                  </div>
                  <div class="wb-track">
                    <div class="wb-fill mp" :style="{ width: bossMpPercent + '%' }"></div>
                  </div>
                </div>

                <div class="wb-mini-stats">
                  <div class="wb-mini-stat"><span class="wb-inline-label">物攻</span><strong>{{ current.boss?.attributes?.physicalAttack || 0 }}</strong></div>
                  <div class="wb-mini-stat"><span class="wb-inline-label">法攻</span><strong>{{ current.boss?.attributes?.magicAttack || 0 }}</strong></div>
                  <div class="wb-mini-stat"><span class="wb-inline-label">物防</span><strong>{{ current.boss?.attributes?.physicalDefense || 0 }}</strong></div>
                  <div class="wb-mini-stat"><span class="wb-inline-label">法防</span><strong>{{ current.boss?.attributes?.magicDefense || 0 }}</strong></div>
                  <div class="wb-mini-stat"><span class="wb-inline-label">速度</span><strong>{{ current.boss?.attributes?.speed || 0 }}</strong></div>
                  <div class="wb-mini-stat"><span class="wb-inline-label">元素</span><strong>{{ current.boss?.attributes?.elementText || '-' }}</strong></div>
                </div>

              </div>
            </div>
          </div>

          <div class="wb-card wb-player-card">
            <div class="wb-player-head">
              <div>
                <div class="wb-section-title">我的战斗状态</div>
                <div class="wb-player-name">{{ current.self?.playerName || '未参战' }}</div>
              </div>

              <button
                class="wb-btn"
                :class="{ 'wb-btn--auto': current.self?.isAuto }"
                :disabled="loading || !current.self?.isJoined"
                @click="toggleAuto"
              >
                {{ current.self?.isAuto ? '关闭自动' : '开启自动' }}
              </button>
            </div>

            <div class="wb-player-summary">
              <div class="wb-summary-item">
                <span class="wb-inline-label">总伤害</span>
                <strong>{{ current.self?.totalDamage || 0 }}</strong>
              </div>
              <div class="wb-summary-item">
                <span class="wb-inline-label">当前排名</span>
                <strong>{{ current.self?.rank || '-' }}</strong>
              </div>
              <div class="wb-summary-item">
                <span class="wb-inline-label">行动状态</span>
                <strong v-if="current.self?.isDead">复活 {{ current.self?.secondsToRevive || 0 }} 秒</strong>
                <strong v-else-if="!current.self?.canAct">冷却 {{ current.self?.secondsToReady || 0 }} 秒</strong>
                <strong v-else>窗口 {{ current.self?.secondsToDeadline || 0 }} 秒</strong>
              </div>
            </div>

            <div class="wb-self-bars">
              <div class="wb-bar-card hp">
                <div class="wb-bar-head">
                  <span>自己气血</span>
                  <strong>{{ displaySelfCurrentHp }}/{{ current.self?.maxHp || 0 }}</strong>
                </div>
                <div class="wb-track">
                  <div class="wb-fill hp" :style="{ width: selfHpPercent + '%' }"></div>
                </div>
              </div>

              <div class="wb-bar-card mp">
                <div class="wb-bar-head">
                  <span>自己灵力</span>
                  <strong>{{ displaySelfCurrentMp }}/{{ current.self?.maxMp || 0 }}</strong>
                </div>
                <div class="wb-track">
                  <div class="wb-fill mp" :style="{ width: selfMpPercent + '%' }"></div>
                </div>
              </div>
            </div>

            <div class="wb-skill-strip">
              <button class="wb-skill-slot wb-skill-slot--normal" :disabled="actionDisabled" @click="normalAttack">
                <span class="wb-slot-corner">普攻</span>
                <span class="wb-skill-icon"><AssetIcon :source="ICON.stat_attack" size="25" /></span>
                <span class="wb-skill-name">普通攻击</span>
                <span class="wb-skill-meta">默认</span>
              </button>

              <button
                v-for="(skill, index) in displaySkillSlots"
                :key="skill ? skill.skillId : `empty-${index}`"
                class="wb-skill-slot"
                :class="{ disabled: !skill || !skill.canUse, empty: !skill }"
                :disabled="actionDisabled || !skill || !skill.canUse"
                @click="skill && castSkill(skill.skillId)"
              >
                <span class="wb-slot-corner">技能{{ index + 1 }}</span>
                <template v-if="skill">
                  <span class="wb-skill-icon">{{ skill.icon }}</span>
                  <span class="wb-skill-name">{{ skill.name }}</span>
                  <span class="wb-skill-meta">CD {{ skill.currentCooldown }}/{{ skill.cooldown }}</span>
                </template>
                <template v-else>
                  <span class="wb-skill-icon wb-skill-icon--empty">+</span>
                  <span class="wb-skill-name">空位</span>
                  <span class="wb-skill-meta">未装备</span>
                </template>
              </button>
            </div>
          </div>

          <div class="wb-card wb-log-card">
            <div class="wb-card-head">
              <span>战斗日志</span>
              <span>{{ logs.length }} 条</span>
            </div>
            <div class="wb-log-list">
              <div v-for="log in logs" :key="`${log.seq}-${log.timestampUtc}`" class="wb-log-item" :class="log.actionType">
                <span class="wb-log-time">{{ formatTime(log.timestampUtc) }}</span>
                <span class="wb-log-text">{{ log.content }}</span>
              </div>
              <div v-if="logs.length === 0" class="wb-log-empty">暂无战斗日志</div>
            </div>
          </div>
        </section>

        <aside class="wb-side">
          <div class="wb-card wb-rank-card">
            <div class="wb-card-head">
              <span>伤害榜前十</span>
            </div>
            <div class="wb-rank-list">
              <div v-for="item in ranking" :key="item.playerId" class="wb-rank-item" :class="{ me: item.isSelf }">
                <span class="wb-rank-no">{{ item.rank }}</span>
                <div class="wb-rank-copy">
                  <span class="wb-rank-name">{{ item.playerName }}</span>
                  <span class="wb-rank-mark">{{ item.isSelf ? '你' : '参战者' }}</span>
                </div>
                <span class="wb-rank-damage">{{ item.totalDamage }}</span>
              </div>
              <div v-if="ranking.length === 0" class="wb-log-empty">暂无排行</div>
            </div>
          </div>
        </aside>
      </div>

      <p v-if="errorMessage" class="wb-error">{{ errorMessage }}</p>
    </div>
  </XiuXianModal>
</template>

<script>
import { computed, onBeforeUnmount, reactive, ref, watch } from 'vue'
import XiuXianModal from '../common/XiuXianModal.vue'
import AssetIcon from '../common/AssetIcon.vue'
import { apiClient } from '../../lib/apiClient'
import { ICON } from '../../icons'

export default {
  name: 'WorldBossModal',
  components: {
    XiuXianModal,
    AssetIcon
  },
  props: {
    modelValue: Boolean
  },
  emits: ['update:modelValue'],
  setup(props) {
    // 世界 Boss 弹窗核心状态。
    // current 保存当前实例、Boss 和玩家自身战斗状态，
    // ranking/logs 分别对应右侧排行和底部日志区。
    const current = reactive({
      hasActiveBoss: false,
      hasPendingReward: false,
      pendingRewardInstanceId: '',
      instance: null,
      boss: null,
      self: null
    })
    const ranking = ref([])
    const logs = ref([])
    const loading = ref(false)
    const errorMessage = ref('')
    const showBossTooltip = ref(false)
    let refreshTimer = null

    // 血蓝条百分比统一在前端通过当前值 / 最大值计算，
    // 这样可以避免模板调整后还要额外维护展示字段。
    const bossHpPercent = computed(() => {
      const value = current.boss?.maxHp ? (current.boss.currentHp / current.boss.maxHp) * 100 : 0
      return Math.max(0, Math.min(100, value))
    })
    const bossMpPercent = computed(() => {
      const value = current.boss?.maxMp ? (current.boss.currentMp / current.boss.maxMp) * 100 : 0
      return Math.max(0, Math.min(100, value))
    })
    const selfHpPercent = computed(() => {
      const value = current.self?.maxHp ? (current.self.currentHp / current.self.maxHp) * 100 : 0
      return Math.max(0, Math.min(100, value))
    })
    const selfMpPercent = computed(() => {
      const value = current.self?.maxMp ? (current.self.currentMp / current.self.maxMp) * 100 : 0
      return Math.max(0, Math.min(100, value))
    })
    const displayBossCurrentHp = computed(() => Math.max(0, Number(current.boss?.currentHp || 0)))
    const displayBossCurrentMp = computed(() => Math.max(0, Number(current.boss?.currentMp || 0)))
    const displaySelfCurrentHp = computed(() => Math.max(0, Number(current.self?.currentHp || 0)))
    const displaySelfCurrentMp = computed(() => Math.max(0, Number(current.self?.currentMp || 0)))
    const displaySkillSlots = computed(() => {
      const result = Array.isArray(current.self?.skills) ? [...current.self.skills] : []
      const normalized = result.slice(0, 6)
      while (normalized.length < 6) {
        normalized.push(null)
      }
      return normalized
    })
    const actionDisabled = computed(() => {
      return loading.value || !current.self?.isJoined || !current.self?.canAct || current.self?.isDead
    })

    // 用接口返回的当前状态整包覆盖本地响应式对象。
    // 统一从这里写入，避免页面各处直接改结构导致状态不一致。
    const assignCurrent = (payload) => {
      current.hasActiveBoss = Boolean(payload?.hasActiveBoss)
      current.hasPendingReward = Boolean(payload?.hasPendingReward)
      current.pendingRewardInstanceId = payload?.pendingRewardInstanceId || ''
      current.instance = payload?.instance || null
      current.boss = payload?.boss || null
      current.self = payload?.self || null
    }

    // 停止自动刷新计时器。
    const stopTimer = () => {
      if (refreshTimer) {
        clearInterval(refreshTimer)
        refreshTimer = null
      }
    }

    // 世界 Boss 面板打开后每秒自动刷新一次。
    // 因为行动冷却和手动窗口都是秒级变化，刷新频率必须比普通静态页面更高。
    const startTimer = () => {
      stopTimer()
      refreshTimer = window.setInterval(() => {
        refreshAll(false)
      }, 1000)
    }

    // 拉取当前 Boss、排行和日志三组数据。
    const refreshAll = async (showLoading = true) => {
      try {
        if (showLoading) loading.value = true
        const [currentResult, rankingResult, logResult] = await Promise.all([
          apiClient.getWorldBossCurrent(),
          apiClient.getWorldBossRanking(10),
          apiClient.getWorldBossLogs(100)
        ])
        assignCurrent(currentResult || {})
        ranking.value = rankingResult || []
        logs.value = logResult || []
        errorMessage.value = ''
      } catch (error) {
        errorMessage.value = error.message || '世界Boss数据加载失败。'
      } finally {
        loading.value = false
      }
    }

    // 玩家打开世界 Boss 弹窗时，先通过 join 接口确保自己处于参战状态。
    const openWorldBoss = async () => {
      try {
        loading.value = true
        const result = await apiClient.joinWorldBoss()
        assignCurrent(result || {})
        await refreshAll(false)
        startTimer()
      } catch (error) {
        errorMessage.value = error.message || '进入世界Boss失败。'
      } finally {
        loading.value = false
      }
    }

    // 发起一次普通攻击。
    const normalAttack = async () => {
      try {
        loading.value = true
        await apiClient.worldBossAction('normal')
        await refreshAll(false)
      } catch (error) {
        errorMessage.value = error.message || '普攻失败。'
      } finally {
        loading.value = false
      }
    }

    // 发起一次指定技能释放。
    const castSkill = async (skillId) => {
      try {
        loading.value = true
        await apiClient.worldBossAction('skill', skillId)
        await refreshAll(false)
      } catch (error) {
        errorMessage.value = error.message || '技能释放失败。'
      } finally {
        loading.value = false
      }
    }

    // 切换自动战斗开关。
    const toggleAuto = async () => {
      try {
        loading.value = true
        await apiClient.setWorldBossAuto(!current.self?.isAuto)
        await refreshAll(false)
      } catch (error) {
        errorMessage.value = error.message || '自动状态切换失败。'
      } finally {
        loading.value = false
      }
    }

    // 领取当前实例的结算奖励。
    const claimReward = async () => {
      try {
        loading.value = true
        await apiClient.claimWorldBossReward()
        await refreshAll(false)
      } catch (error) {
        errorMessage.value = error.message || '奖励领取失败。'
      } finally {
        loading.value = false
      }
    }

    // 把后端 UTC/ISO 时间裁成前端日志展示需要的时分秒。
    const formatTime = (value) => {
      if (!value) return '--:--:--'
      return String(value).replace('T', ' ').slice(11, 19)
    }

    // 把小数比例统一格式化成百分比文案。
    const toPercent = (value) => {
      const number = Number(value) || 0
      return `${(number * 100).toFixed(1).replace(/\.0$/, '')}%`
    }

    // 弹窗打开时立即加入战场并启动自动刷新；关闭时停止刷新。
    watch(() => props.modelValue, async (visible) => {
      if (visible) {
        await openWorldBoss()
      } else {
        stopTimer()
      }
    }, { immediate: true })

    // 组件销毁时一定要清掉轮询计时器，避免弹窗关闭后还在持续请求接口。
    onBeforeUnmount(() => {
      stopTimer()
    })

    return {
      ICON,
      current,
      ranking,
      logs,
      loading,
      errorMessage,
      showBossTooltip,
      bossHpPercent,
      bossMpPercent,
      selfHpPercent,
      selfMpPercent,
      displayBossCurrentHp,
      displayBossCurrentMp,
      displaySelfCurrentHp,
      displaySelfCurrentMp,
      displaySkillSlots,
      actionDisabled,
      refreshAll,
      normalAttack,
      castSkill,
      toggleAuto,
      claimReward,
      formatTime,
      toPercent
    }
  }
}
</script>

<style scoped>
.wb-modal {
  display: flex;
  flex-direction: column;
  gap: 10px;
  max-height: min(72vh, 720px);
  min-height: 0;
}

.wb-topbar {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 12px;
  padding-bottom: 6px;
  border-bottom: 1px solid var(--border-color);
  flex-shrink: 0;
}

.wb-kicker {
  font-size: 10px;
  letter-spacing: 0.18em;
  text-transform: uppercase;
  color: var(--highlight-text);
}

.wb-title-row {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
  margin-top: 4px;
}

.wb-title {
  margin: 0;
  font-size: 21px;
  color: var(--highlight-text);
  line-height: 1.05;
}

.wb-state {
  padding: 3px 8px;
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.06);
  border: 1px solid var(--border-color);
  color: var(--text-secondary);
  font-size: 11px;
  line-height: 1.1;
}

.wb-state.active {
  color: var(--status-success-text);
  border-color: rgba(76, 175, 80, 0.35);
  background: rgba(76, 175, 80, 0.14);
}

.wb-meta {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
  margin-top: 4px;
  color: var(--text-secondary);
  font-size: 11px;
  line-height: 1.1;
}

.wb-actions {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.wb-layout {
  display: grid;
  grid-template-columns: minmax(0, 1fr) 220px;
  gap: 12px;
  min-height: 0;
  flex: 1;
}

.wb-main {
  display: flex;
  flex-direction: column;
  gap: 12px;
  min-height: 0;
}

.wb-side {
  min-width: 0;
  min-height: 0;
  align-self: start;
}

.wb-card {
  background: var(--xiuxian-bg-panel);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-lg);
  padding: 8px;
  min-height: 0;
  overflow: hidden;
}

.wb-boss-card,
.wb-player-card {
  flex: 0 0 auto;
}

.wb-empty {
  padding: 40px 16px;
  text-align: center;
  background: var(--xiuxian-bg-panel);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-lg);
}

.wb-empty-title {
  font-size: 22px;
  font-weight: 700;
  color: var(--highlight-text);
}

.wb-empty-text {
  margin-top: 8px;
  color: var(--text-secondary);
}

.wb-boss-grid {
  display: grid;
  grid-template-columns: 82px minmax(0, 1fr);
  gap: 4px;
  align-items: start;
}

.wb-portrait-panel {
  display: flex;
  align-items: flex-start;
  justify-content: flex-start;
}

.wb-portrait-frame {
  position: relative;
  width: 68px;
  height: 68px;
}

.wb-portrait {
  width: 100%;
  height: 100%;
  border-radius: 20px;
  overflow: hidden;
  border: 1px solid rgba(255, 255, 255, 0.08);
  background: rgba(255, 255, 255, 0.04);
}

.wb-tooltip {
  position: absolute;
  left: calc(100% + 12px);
  top: 0;
  z-index: 20;
  width: 300px;
  padding: 12px;
  border-radius: 16px;
  background: rgba(9, 13, 21, 0.96);
  border: 1px solid rgba(255, 255, 255, 0.1);
  box-shadow: 0 14px 40px rgba(0, 0, 0, 0.3);
}

.wb-tooltip-title {
  font-size: 16px;
  font-weight: 700;
  color: var(--highlight-text);
  margin-bottom: 10px;
}

.wb-tooltip-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 6px 12px;
  color: var(--text-secondary);
  font-size: 13px;
}

.wb-boss-info,
.wb-player-card {
  display: flex;
  flex-direction: column;
  gap: 4px;
  min-height: 0;
}

.wb-section-title {
  font-size: 11px;
  font-weight: 700;
  color: var(--highlight-text);
  line-height: 1.2;
  margin-bottom: 1px;
}

.wb-bar-card {
  padding: 5px 7px;
  border-radius: 9px;
  background: rgba(255, 255, 255, 0.03);
}

.wb-bar-head {
  display: flex;
  justify-content: space-between;
  gap: 10px;
  margin-bottom: 2px;
  color: var(--text-secondary);
  font-size: 10px;
}

.wb-bar-head strong {
  color: var(--text-primary);
  font-size: 11px;
}

.wb-track {
  height: 8px;
  border-radius: 999px;
  overflow: hidden;
  background: rgba(255, 255, 255, 0.08);
}

.wb-fill {
  height: 100%;
  border-radius: 999px;
}

.wb-fill.hp {
  background: linear-gradient(90deg, #b84a4a, #e07b58);
}

.wb-fill.mp {
  background: linear-gradient(90deg, #245ca6, #4f93f4);
}

.wb-mini-stat,
.wb-summary-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 6px;
  padding: 6px 7px;
  border-radius: 10px;
  background: rgba(255, 255, 255, 0.03);
}

.wb-mini-stats {
  display: grid;
  grid-template-columns: repeat(6, minmax(0, 1fr));
  gap: 3px;
}

.wb-mini-stat span,
.wb-summary-item span {
  color: var(--text-secondary);
  font-size: 9px;
}

.wb-mini-stat strong,
.wb-summary-item strong {
  color: var(--text-primary);
  font-size: 11px;
  line-height: 1.1;
}

.wb-inline-label {
  white-space: nowrap;
}

.wb-player-head {
  display: flex;
  justify-content: space-between;
  gap: 10px;
  align-items: flex-start;
}

.wb-player-name {
  font-size: 15px;
  font-weight: 700;
  color: var(--highlight-text);
}

.wb-player-summary {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 4px;
}

.wb-self-bars {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 4px;
}

.wb-skill-strip {
  display: grid;
  grid-template-columns: repeat(7, minmax(0, 1fr));
  gap: 6px;
}

.wb-skill-slot {
  position: relative;
  display: flex;
  flex-direction: column;
  gap: 2px;
  min-height: 58px;
  padding: 14px 5px 5px;
  border-radius: 10px;
  border: 1px solid rgba(255, 255, 255, 0.08);
  background: rgba(255, 255, 255, 0.03);
  color: var(--text-primary);
  text-align: center;
  cursor: pointer;
  transition: all 0.2s ease;
}

.wb-skill-slot:hover:not(:disabled) {
  border-color: var(--accent-text);
  box-shadow: 0 0 0 1px rgba(124, 58, 237, 0.18);
}

.wb-skill-slot--normal {
  background: rgba(215, 180, 92, 0.08);
}

.wb-skill-slot.disabled,
.wb-skill-slot:disabled {
  cursor: not-allowed;
  border-color: rgba(255, 255, 255, 0.06);
  background: rgba(255, 255, 255, 0.025);
}

.wb-skill-slot.empty {
  border-style: dashed;
  color: var(--text-muted);
}

.wb-slot-corner {
  position: absolute;
  top: 4px;
  left: 6px;
  font-size: 9px;
  color: var(--text-muted);
}

.wb-skill-icon {
  font-size: 13px;
}

.wb-skill-name {
  font-size: 11px;
  font-weight: 700;
  line-height: 1.15;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.wb-skill-meta {
  color: var(--text-secondary);
  font-size: 9px;
  line-height: 1.1;
}

.wb-skill-icon--empty {
  color: var(--text-muted);
}

.wb-card-head {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
  color: var(--highlight-text);
  font-weight: 700;
  flex-shrink: 0;
}

.wb-log-list,
.wb-rank-list {
  max-height: none;
  overflow: auto;
  display: flex;
  flex-direction: column;
  gap: 6px;
  min-height: 0;
  flex: 1;
}

.wb-log-item {
  display: grid;
  grid-template-columns: 64px minmax(0, 1fr);
  gap: 8px;
  padding: 7px 9px;
  border-radius: 12px;
  background: rgba(255, 255, 255, 0.03);
  border-left: 3px solid rgba(255, 255, 255, 0.08);
}

.wb-log-item.auto { border-left-color: var(--status-warning-text); }
.wb-log-item.skill { border-left-color: #6fa8ff; }
.wb-log-item.normal { border-left-color: var(--highlight-text); }
.wb-log-item.dead { border-left-color: var(--status-danger-text); }
.wb-log-item.revive { border-left-color: var(--status-success-text); }
.wb-log-item.settle { border-left-color: var(--accent-text); }

.wb-log-time {
  color: var(--text-muted);
  font-family: var(--font-mono);
  font-size: 11px;
}

.wb-log-text {
  color: var(--text-primary);
  line-height: 1.45;
  font-size: 12px;
}

.wb-log-empty,
.wb-error {
  text-align: center;
  color: var(--text-secondary);
  padding: 24px 0;
}

.wb-rank-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 9px;
  border-radius: 12px;
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid transparent;
}

.wb-rank-item.me {
  border-color: rgba(215, 180, 92, 0.35);
  background: rgba(215, 180, 92, 0.08);
}

.wb-rank-no {
  width: 22px;
  text-align: center;
  color: var(--highlight-text);
  font-weight: 700;
  font-size: 13px;
}

.wb-rank-copy {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.wb-rank-name {
  color: var(--text-primary);
  font-weight: 600;
  font-size: 13px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.wb-rank-mark {
  color: var(--text-secondary);
  font-size: 11px;
}

.wb-rank-damage {
  color: var(--text-primary);
  font-family: var(--font-mono);
  font-size: 11px;
}

.wb-btn {
  padding: 7px 11px;
  border-radius: 999px;
  border: 1px solid var(--border-color);
  background: var(--bg-overlay-light);
  color: var(--text-primary);
  cursor: pointer;
  transition: all 0.2s ease;
  font-size: 12px;
  line-height: 1.1;
}

.wb-btn:hover:not(:disabled) {
  border-color: var(--accent-text);
  color: var(--accent-text);
}

.wb-btn--reward {
  background: oklch(0.65 0.12 80 / 0.12);
  border-color: oklch(0.65 0.12 80 / 0.25);
}

.wb-btn--auto {
  background: var(--button-info-start);
  border-color: oklch(0.6 0.12 250 / 0.25);
}

.wb-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.wb-log-card,
.wb-rank-card {
  display: flex;
  flex-direction: column;
  min-height: 0;
}

.wb-log-card {
  min-height: 200px;
  flex: 1 1 auto;
}

.wb-rank-card {
  width: 100%;
  max-height: 220px;
}

@media (max-width: 1080px) {
  .wb-layout {
    grid-template-columns: 1fr;
  }

  .wb-boss-grid {
    grid-template-columns: 1fr;
  }

  .wb-portrait-panel {
    justify-content: flex-start;
  }

  .wb-modal {
    max-height: min(78vh, 760px);
  }
}

@media (max-width: 760px) {
  .wb-topbar,
  .wb-player-head {
    flex-direction: column;
    align-items: flex-start;
  }

  .wb-tooltip {
    left: 0;
    top: calc(100% + 12px);
    width: min(320px, calc(100vw - 88px));
  }

  .wb-tooltip-grid,
  .wb-mini-stats,
  .wb-player-summary,
  .wb-skill-strip {
    grid-template-columns: 1fr;
  }

  .wb-self-bars {
    grid-template-columns: 1fr;
  }

  .wb-modal {
    max-height: none;
  }

  .wb-log-list,
  .wb-rank-list {
    max-height: 260px;
  }
}
</style>
