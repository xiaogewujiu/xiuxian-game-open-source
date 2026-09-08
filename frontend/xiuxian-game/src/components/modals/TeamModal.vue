<template>
  <XiuXianModal
    :model-value="modelValue"
    title="组队系统"
    :width="720"
    @update:model-value="$emit('update:modelValue', $event)"
  >
    <div class="team-modal">
      <div class="team-toolbar">
        <div class="status-text">
          {{ statusMessage || '临时队伍只用于组队副本，不与工会系统共用数据。' }}
        </div>
        <button class="refresh-btn" :disabled="isLoading" @click="reloadTeamData">
          {{ isLoading ? '刷新中...' : '刷新列表' }}
        </button>
      </div>

      <div v-if="myTeam" class="team-layout">
        <section class="panel-card team-panel">
          <div class="panel-header">
            <div>
              <h3 class="panel-title">{{ myTeam.name }}</h3>
              <div class="panel-subtitle">
              副本将在主页选择 · {{ myTeam.currentMembers }}/{{ myTeam.maxMembers }} 人
              </div>
            </div>
            <span class="status-badge" :class="{ active: myTeam.isRecruiting }">
              {{ myTeam.statusText }}
            </span>
          </div>

          <div class="team-meta">
            <div class="meta-item">
              <span>队长</span>
              <strong>{{ myTeam.leaderName }}</strong>
            </div>
            <div class="meta-item">
              <span>最低等级</span>
              <strong>Lv.{{ myTeam.minLevel }}</strong>
            </div>
            <div class="meta-item">
              <span>到期时间</span>
              <strong>{{ formatDateTime(myTeam.expiresAt) }}</strong>
            </div>
          </div>

          <div v-if="myTeam.unavailableReason" class="hint-card warning">
            {{ myTeam.unavailableReason }}
          </div>

          <div class="member-list">
            <div
              v-for="member in myTeam.members"
              :key="member.playerId"
              class="member-card"
              :class="{ leader: member.isLeader, self: member.isSelf }"
            >
              <div class="member-avatar">{{ member.spiritRootIcon }}</div>
              <div class="member-main">
                <div class="member-name">
                  {{ member.name }}
                  <span v-if="member.isLeader" class="member-tag">队长</span>
                  <span v-if="member.isSelf" class="member-tag self">你</span>
                </div>
                <div class="member-detail">
                  {{ member.realmText }} · Lv.{{ member.level }} · {{ member.spiritRootName }}灵根
                </div>
              </div>
            </div>

            <div
              v-for="slotIndex in emptySlots"
              :key="`empty-${slotIndex}`"
              class="member-card empty"
            >
              <div class="empty-slot">+</div>
              <div class="member-main">
                <div class="member-name">等待队友加入</div>
                <div class="member-detail">需要满员后才能在主页选择副本并发起挑战</div>
              </div>
            </div>
          </div>

          <div class="action-row">
            <button
              class="secondary-btn"
              :disabled="!myTeam.isLeader || isLoading"
              @click="toggleRecruit"
            >
              {{ myTeam.isRecruiting ? '关闭招募' : '开启招募' }}
            </button>
            <button class="secondary-btn danger" :disabled="isLoading" @click="leaveTeam">
              {{ myTeam.isLeader ? '退出并移交/解散' : '退出队伍' }}
            </button>
            <button
              v-if="myTeam.isLeader"
              class="secondary-btn danger"
              :disabled="isLoading"
              @click="dismissTeam"
            >
              解散队伍
            </button>
          </div>
        </section>
      </div>

      <div v-else class="team-layout">
        <section class="panel-card create-panel">
          <div class="panel-header">
            <div>
              <h3 class="panel-title">创建临时队伍</h3>
              <div class="panel-subtitle">只能为支持组队的副本创建队伍</div>
            </div>
          </div>

          <div class="form-group">
            <label>队伍名称</label>
            <input v-model.trim="newTeam.name" type="text" maxlength="20" placeholder="例如：金丹试炼三人组" />
          </div>

          <div class="form-group">
            <label>队伍人数</label>
            <select v-model.number="newTeam.teamSize">
              <option :value="1">单人队伍（1人）</option>
              <option :value="2">双人队伍（2人）</option>
              <option :value="3">三人队伍（3人）</option>
            </select>
          </div>

          <div class="hint-card">
            副本不在创建队伍时绑定，请回到主页通过“选择地图”确定本场副本。
          </div>

          <button
            class="primary-btn full-width"
            :disabled="isLoading || !canCreateTeam"
            @click="createTeam"
          >
            {{ isLoading ? '处理中...' : '创建队伍' }}
          </button>
        </section>

        <section class="panel-card list-panel">
          <div class="panel-header">
            <div>
              <h3 class="panel-title">可加入队伍</h3>
              <div class="panel-subtitle">这里只展示真实数据库中的临时队伍</div>
            </div>
          </div>

          <div v-if="partyList.length === 0" class="empty-list">
            当前暂无可加入的队伍。
          </div>

          <div v-else class="party-list">
            <div v-for="party in partyList" :key="party.partyId" class="party-card">
              <div class="party-top">
                <div>
                  <div class="party-name">{{ party.name }}</div>
                  <div class="party-target">{{ party.targetDungeonName || `固定${party.maxMembers}人队伍，副本在主页选择` }}</div>
                </div>
                <span class="status-badge" :class="{ active: party.isRecruiting }">
                  {{ party.statusText }}
                </span>
              </div>

              <div class="party-info">
                <span>队长：{{ party.leaderName }}</span>
                <span>人数：{{ party.currentMembers }}/{{ party.maxMembers }}</span>
                <span>要求：Lv.{{ party.minLevel }}</span>
              </div>

              <div v-if="party.unavailableReason" class="hint-card warning compact">
                {{ party.unavailableReason }}
              </div>

              <button
                class="secondary-btn full-width"
                :disabled="!party.canJoin || isLoading"
                @click="joinTeam(party)"
              >
                {{ party.canJoin ? '加入队伍' : '暂不可加入' }}
              </button>
            </div>
          </div>
        </section>
      </div>
    </div>
  </XiuXianModal>
</template>

<script>
import { computed, reactive, ref, watch } from 'vue'
import { confirm as showConfirm } from '@/utils/confirm'
import XiuXianModal from '../common/XiuXianModal.vue'
import { useGameStore } from '../../state/gameStore'
import toast from '@/utils/toast'

/*
 * 中文注释：
 * 这里的队伍是专门服务组队副本的临时队伍，不和宗门系统共用状态。
 * 因此前端会单独处理创建、加入、招募和发起副本挑战。
 */
function toNumber(value, fallback = 0) {
  const numericValue = Number(value)
  return Number.isFinite(numericValue) ? numericValue : fallback
}

function normalizeDungeon(dungeon) {
  // 中文注释：
  // 只保留创建队伍真正需要的副本字段，避免把原始 DTO 整包透传到模板层。
  return {
    dungeonId: dungeon?.dungeonId || dungeon?.DungeonId || '',
    name: dungeon?.name || dungeon?.Name || '未知副本',
    description: dungeon?.description || dungeon?.Description || '暂无副本说明。',
    recommendedLevel: toNumber(dungeon?.recommendedLevel ?? dungeon?.RecommendedLevel ?? 1, 1),
    requiredTeamSize: toNumber(dungeon?.requiredTeamSize ?? dungeon?.RequiredTeamSize ?? 1, 1),
    supportsParty: Boolean(dungeon?.supportsParty ?? dungeon?.SupportsParty),
    unavailableReason: dungeon?.unavailableReason || dungeon?.UnavailableReason || ''
  }
}

function normalizePartyMember(member) {
  const realmName = member?.realmName || member?.RealmName || '练气'
  const realmLayer = toNumber(member?.realmLayer ?? member?.RealmLayer ?? 1, 1)

  return {
    playerId: member?.playerId || member?.PlayerId || '',
    name: member?.name || member?.Name || '未知修士',
    level: toNumber(member?.level ?? member?.Level ?? 1, 1),
    realmText: `${realmName}${realmLayer}层`,
    isLeader: Boolean(member?.isLeader ?? member?.IsLeader),
    isSelf: Boolean(member?.isSelf ?? member?.IsSelf),
    spiritRootName: member?.spiritRootName || member?.SpiritRootName || '无',
    spiritRootIcon: member?.spiritRootIcon || member?.SpiritRootIcon || '○'
  }
}

function normalizePartySummary(party) {
  // 中文注释：
  // 队伍列表和我的队伍详情先共用一层摘要结构，
  // 这样列表卡片、顶部状态条和后续详情扩展都能复用同一套字段命名。
  return {
    partyId: party?.partyId || party?.PartyId || '',
    name: party?.name || party?.Name || '未命名队伍',
    leaderName: party?.leaderName || party?.LeaderName || '未知队长',
    targetDungeonId: party?.targetDungeonId || party?.TargetDungeonId || '',
    targetDungeonName: party?.targetDungeonName || party?.TargetDungeonName || '未知副本',
    minLevel: toNumber(party?.minLevel ?? party?.MinLevel ?? 1, 1),
    maxMembers: toNumber(party?.maxMembers ?? party?.MaxMembers ?? 1, 1),
    currentMembers: toNumber(party?.currentMembers ?? party?.CurrentMembers ?? 0, 0),
    isRecruiting: Boolean(party?.isRecruiting ?? party?.IsRecruiting),
    canJoin: Boolean(party?.canJoin ?? party?.CanJoin),
    canChallenge: Boolean(party?.canChallenge ?? party?.CanChallenge),
    statusText: party?.statusText || party?.StatusText || '未知状态',
    unavailableReason: party?.unavailableReason || party?.UnavailableReason || '',
    expiresAt: party?.expiresAt || party?.ExpiresAt || ''
  }
}

function normalizePartyDetail(party) {
  const base = normalizePartySummary(party)
  if (!base.partyId) {
    return null
  }

  const members = Array.isArray(party?.members || party?.Members)
    ? (party?.members || party?.Members).map(normalizePartyMember)
    : []

  return {
    ...base,
    isLeader: Boolean(party?.isLeader ?? party?.IsLeader),
    isMember: Boolean(party?.isMember ?? party?.IsMember),
    members
  }
}

export default {
  name: 'TeamModal',

  components: {
    XiuXianModal
  },

  props: {
    modelValue: Boolean
  },

  emits: ['update:modelValue', 'party-battle'],

  setup(props, { emit }) {
    const gameStore = useGameStore()
    const isLoading = ref(false)
    const statusMessage = ref('')
    const newTeam = reactive({
      name: '',
      teamSize: 1
    })

    const partyDungeons = computed(() => {
      return (gameStore.state.dungeons || [])
        .map(normalizeDungeon)
        .filter((item) => item.requiredTeamSize >= 1 && item.requiredTeamSize <= 3)
    })

    const selectedDungeon = computed(() => {
      return partyDungeons.value.find((item) => item.dungeonId === newTeam.dungeonId) || null
    })

    const myTeam = computed(() => normalizePartyDetail(gameStore.state.currentParty))
    const partyList = computed(() => {
      return (gameStore.state.parties || [])
        .map(normalizePartySummary)
        .filter((party) => Boolean(party.partyId))
    })

    const canCreateTeam = computed(() => {
      return Boolean(newTeam.name.trim() && [1, 2, 3].includes(Number(newTeam.teamSize)))
    })

    const emptySlots = computed(() => {
      if (!myTeam.value) {
        return 0
      }

      return Math.max(0, myTeam.value.maxMembers - myTeam.value.members.length)
    })

    const formatDateTime = (value) => {
      if (!value) {
        return '未知'
      }

      const date = new Date(value)
      if (Number.isNaN(date.getTime())) {
        return String(value)
      }

      return date.toLocaleString('zh-CN', {
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit'
      })
    }

    const syncDefaultDungeon = () => {
      return
    }

    const reloadTeamData = async () => {
      isLoading.value = true
      statusMessage.value = ''

      try {
        // 中文注释：
        // 队伍页依赖“副本列表 + 招募队伍 + 我的当前队伍”三份数据。
        // 这里统一并行刷新，避免它们分别更新时让页面出现短暂错位。
        await Promise.all([
          gameStore.loadDungeons(true),
          gameStore.loadParties(true),
          gameStore.loadCurrentParty(true)
        ])
        syncDefaultDungeon()
      } catch (error) {
        statusMessage.value = error.message || '加载队伍数据失败。'
      } finally {
        isLoading.value = false
      }
    }

    watch(() => props.modelValue, async (visible) => {
      if (!visible) {
        return
      }

      await reloadTeamData()
    }, { immediate: true })

    watch(partyDungeons, () => {
      syncDefaultDungeon()
    }, { immediate: true })

    const createTeam = async () => {
      if (!canCreateTeam.value) {
        statusMessage.value = '请先填写队伍名称并选择合法的队伍人数。'
        return
      }

      try {
        // 中文注释：
        // 创建成功后不直接手工拼本地队伍对象，而是回到统一 reload 流程。
        // 这样队伍成员数、招募状态、我的队伍视图都会和后端最新结果保持一致。
        const result = await gameStore.createParty({
          name: newTeam.name.trim(),
          teamSize: Number(newTeam.teamSize)
        })

        statusMessage.value = `已创建队伍：${result?.name || result?.Name || newTeam.name.trim()}`
        newTeam.name = ''
        await reloadTeamData()
      } catch (error) {
        statusMessage.value = error.message || '创建队伍失败。'
        toast.error(statusMessage.value)
      }
    }

    const joinTeam = async (party) => {
      try {
        await gameStore.joinParty(party.partyId)
        statusMessage.value = `已加入队伍：${party.name}`
        await reloadTeamData()
      } catch (error) {
        statusMessage.value = error.message || '加入队伍失败。'
        toast.error(statusMessage.value)
      }
    }

    const leaveTeam = async () => {
      if (!await showConfirm('确定要退出当前队伍吗？')) {
        return
      }

      try {
        await gameStore.leaveCurrentParty()
        statusMessage.value = '已退出当前队伍。'
        await reloadTeamData()
      } catch (error) {
        statusMessage.value = error.message || '退出队伍失败。'
        toast.error(statusMessage.value)
      }
    }

    const toggleRecruit = async () => {
      if (!myTeam.value?.partyId) {
        return
      }

      try {
        const result = await gameStore.togglePartyRecruiting(myTeam.value.partyId)
        const latest = normalizePartyDetail(result)
        statusMessage.value = latest?.isRecruiting ? '队伍已开启招募。' : '队伍已关闭招募。'
        await reloadTeamData()
      } catch (error) {
        statusMessage.value = error.message || '切换招募状态失败。'
        toast.error(statusMessage.value)
      }
    }

    const dismissTeam = async () => {
      if (!myTeam.value?.partyId) {
        return
      }

      if (!await showConfirm('确定要解散当前队伍吗？', { type: 'danger' })) {
        return
      }

      try {
        await gameStore.dismissParty(myTeam.value.partyId)
        statusMessage.value = '队伍已解散。'
        await reloadTeamData()
      } catch (error) {
        statusMessage.value = error.message || '解散队伍失败。'
        toast.error(statusMessage.value)
      }
    }

    const startActivity = async () => {
      statusMessage.value = '副本挑战请回到主页选择地图后开始。'
    }

    return {
      isLoading,
      statusMessage,
      newTeam,
      partyDungeons,
      selectedDungeon,
      myTeam,
      partyList,
      canCreateTeam,
      emptySlots,
      formatDateTime,
      reloadTeamData,
      createTeam,
      joinTeam,
      leaveTeam,
      toggleRecruit,
      dismissTeam,
      startActivity
    }
  }
}
</script>

<style scoped>
.team-modal {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
  min-height: 480px;
}

.team-toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--spacing-md);
  padding: var(--spacing-sm) var(--spacing-md);
  background: var(--xiuxian-bg-secondary);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
}

.status-text {
  color: var(--text-secondary);
  line-height: 1.6;
}

.refresh-btn,
.primary-btn,
.secondary-btn {
  border: none;
  border-radius: var(--radius-sm);
  cursor: pointer;
  transition: all 0.2s ease;
}

.refresh-btn {
  padding: var(--spacing-sm) var(--spacing-md);
  background: rgba(124, 58, 237, 0.18);
  color: var(--accent-text);
}

.refresh-btn:disabled,
.primary-btn:disabled,
.secondary-btn:disabled {
  cursor: not-allowed;
  opacity: 0.55;
}

.team-layout {
  display: grid;
  grid-template-columns: 1fr;
  gap: var(--spacing-md);
}

.panel-card {
  background: var(--xiuxian-bg-secondary);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  padding: var(--spacing-md);
}

.panel-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--spacing-md);
  margin-bottom: var(--spacing-md);
}

.panel-title {
  margin: 0;
  font-size: var(--font-size-md);
  color: var(--highlight-text);
}

.panel-subtitle {
  margin-top: 4px;
  color: var(--text-muted);
  line-height: 1.5;
}

.status-badge {
  padding: 4px 10px;
  border-radius: var(--radius-sm);
  background: rgba(214, 90, 90, 0.16);
  color: var(--status-danger-text);
  white-space: nowrap;
}

.status-badge.active {
  background: rgba(124, 58, 237, 0.18);
  color: var(--accent-text);
}

.team-meta {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: var(--spacing-sm);
  margin-bottom: var(--spacing-md);
}

.meta-item {
  display: flex;
  flex-direction: column;
  gap: 6px;
  padding: var(--spacing-sm);
  background: rgba(255, 255, 255, 0.03);
  border-radius: var(--radius-sm);
  color: var(--text-muted);
}

.meta-item strong {
  color: var(--text-primary);
}

.hint-card {
  display: flex;
  flex-direction: column;
  gap: 6px;
  padding: var(--spacing-sm) var(--spacing-md);
  margin-bottom: var(--spacing-md);
  background: rgba(124, 58, 237, 0.12);
  border-radius: var(--radius-sm);
  color: var(--text-secondary);
}

.hint-card.warning {
  background: rgba(214, 90, 90, 0.12);
  color: var(--status-danger-text);
}

.hint-card.compact {
  margin-bottom: var(--spacing-sm);
}

.member-list,
.party-list {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
}

.member-card,
.party-card {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  padding: var(--spacing-md);
  background: var(--xiuxian-bg-panel);
  border-radius: var(--radius-sm);
  border: 1px solid transparent;
}

.member-card.leader,
.party-card:hover {
  border-color: rgba(212, 168, 83, 0.35);
}

.member-card.self {
  border-color: rgba(124, 58, 237, 0.35);
}

.member-card.empty {
  opacity: 0.72;
}

.member-avatar,
.empty-slot {
  width: 42px;
  height: 42px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(255, 255, 255, 0.04);
  font-size: 20px;
}

.member-main {
  flex: 1;
  min-width: 0;
}

.member-name,
.party-name {
  color: var(--text-primary);
  font-weight: 600;
}

.member-detail,
.party-target,
.party-info {
  color: var(--text-muted);
  line-height: 1.5;
}

.member-tag {
  margin-left: 8px;
  padding: 2px 6px;
  border-radius: 999px;
  background: rgba(212, 168, 83, 0.16);
  color: var(--highlight-text);
  font-size: var(--font-size-xs);
}

.member-tag.self {
  background: rgba(124, 58, 237, 0.16);
  color: var(--accent-text);
}

.party-card {
  flex-direction: column;
  align-items: stretch;
}

.party-top,
.party-info,
.action-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--spacing-sm);
}

.party-info {
  flex-wrap: wrap;
  justify-content: flex-start;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-bottom: var(--spacing-md);
}

.form-group label {
  color: var(--text-secondary);
}

.form-group input,
.form-group select {
  width: 100%;
  padding: var(--spacing-sm) var(--spacing-md);
  background: var(--xiuxian-bg-panel);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-sm);
  color: var(--text-primary);
}

.primary-btn,
.secondary-btn {
  padding: var(--spacing-sm) var(--spacing-md);
}

.primary-btn {
  background: var(--button-primary-start);
  color: var(--text-on-dark);
}

.secondary-btn {
  background: rgba(255, 255, 255, 0.06);
  color: var(--text-primary);
}

.secondary-btn.danger {
  background: rgba(214, 90, 90, 0.16);
  color: var(--status-danger-text);
}

.full-width {
  width: 100%;
}

.empty-list {
  padding: var(--spacing-lg);
  text-align: center;
  color: var(--text-muted);
  background: var(--xiuxian-bg-panel);
  border-radius: var(--radius-sm);
}

@media (max-width: 768px) {
  .team-toolbar,
  .panel-header,
  .action-row,
  .party-top {
    flex-direction: column;
    align-items: stretch;
  }

  .team-meta {
    grid-template-columns: 1fr;
  }
}
</style>
