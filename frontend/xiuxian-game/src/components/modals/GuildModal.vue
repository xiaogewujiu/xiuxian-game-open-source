<template>
  <XiuXianModal
    :model-value="modelValue"
    title="宗门"
    :width="880"
    @update:model-value="$emit('update:modelValue', $event)"
  >
    <div class="guild-modal">
      <div class="toolbar-row">
        <div class="toolbar-copy">
          宗门决定你的归属，也会影响宗门聊天与后续宗门玩法入口。
        </div>
        <button class="refresh-btn" :disabled="isLoading" @click="loadGuildData">
          {{ isLoading ? '刷新中...' : '刷新宗门' }}
        </button>
      </div>

      <div class="guild-layout">
        <section class="side-panel">
          <div class="panel-card current-panel">
            <div class="panel-title">我的宗门</div>

            <template v-if="currentGuild">
              <div class="current-guild-card">
                <div class="guild-name-row">
                  <div class="guild-name">{{ currentGuild.name }}</div>
                  <span class="guild-level">Lv.{{ currentGuild.level }}</span>
                </div>
                <div class="guild-meta">
                  <span>宗主 {{ currentGuild.leaderName }}</span>
                  <span>{{ currentGuild.memberCount }}/{{ currentGuild.maxMembers }} 人</span>
                </div>
                <div class="guild-player-meta">
                  <div>宗门贡献 {{ guildContribution }}</div>
                  <div>宗门编号 {{ currentGuild.guildId }}</div>
                </div>
                <button class="danger-btn" :disabled="isSubmitting" @click="leaveCurrentGuild">
                  {{ isSubmitting ? '处理中...' : '退出宗门' }}
                </button>
              </div>
            </template>

            <template v-else-if="isInGuild">
              <div class="empty-card">
                <div class="empty-icon"><AssetIcon :source="ICON.misc_castle" size="25" /></div>
                <div class="empty-text">已检测到宗门归属，正在等待宗门名册同步。</div>
              </div>
            </template>

            <template v-else>
              <div class="empty-card">
                <div class="empty-icon"><AssetIcon :source="ICON.misc_lotus" size="25" /></div>
                <div class="empty-text">你当前尚未加入任何宗门。</div>
              </div>
            </template>
          </div>

          <div class="panel-card create-panel">
            <div class="panel-title">创建宗门</div>
            <div class="input-group">
              <label>宗门名号</label>
              <input
                v-model.trim="guildName"
                type="text"
                maxlength="12"
                placeholder="输入你的宗门名号"
                :disabled="isInGuild || isSubmitting"
              />
            </div>
            <button
              class="primary-btn"
              :disabled="isInGuild || isSubmitting || guildName.length < 2"
              @click="createGuild"
            >
              {{ isSubmitting ? '处理中...' : '创建宗门' }}
            </button>
            <div class="hint-text">
              已加入宗门后不可重复创建；若你是宗主，离宗会受到后端规则限制。
            </div>
          </div>
        </section>

        <section class="list-panel">
          <div class="list-header">
            <div class="panel-title">宗门名册</div>
            <div class="list-count">共 {{ guilds.length }} 个宗门</div>
          </div>

          <div v-if="guilds.length === 0" class="empty-list">
            <div class="empty-icon"><AssetIcon :source="ICON.ui_scroll_list" size="25" /></div>
            <div class="empty-text">当前还没有任何宗门。</div>
          </div>

          <div v-else class="guild-list">
            <div v-for="guild in guilds" :key="guild.guildId" class="guild-card">
              <div class="guild-card-top">
                <div>
                  <div class="guild-card-name">{{ guild.name }}</div>
                  <div class="guild-card-meta">
                    宗主 {{ guild.leaderName }} · 等级 Lv.{{ guild.level }}
                  </div>
                </div>
                <span class="member-badge">{{ guild.memberCount }}/{{ guild.maxMembers }}</span>
              </div>

              <div class="guild-card-footer">
                <div class="guild-id">编号 {{ guild.guildId }}</div>
                <button
                  class="join-btn"
                  :disabled="isSubmitting || isInGuild || guild.guildId === currentGuildId"
                  @click="joinGuild(guild.guildId)"
                >
                  {{ guild.guildId === currentGuildId ? '已加入' : (isInGuild ? '已有宗门' : '申请加入') }}
                </button>
              </div>
            </div>
          </div>
        </section>
      </div>

      <div v-if="statusMessage" class="status-message">{{ statusMessage }}</div>
    </div>
  </XiuXianModal>
</template>

<script>
import { computed, ref, watch } from 'vue'
import XiuXianModal from '../common/XiuXianModal.vue'
import { apiClient } from '../../lib/apiClient'
import { useGameStore } from '../../state/gameStore'
import { ICON } from '../../icons'
import AssetIcon from '../common/AssetIcon.vue'

/*
 * 中文注释：
 * 宗门弹窗把“我的宗门”“创建宗门”“加入宗门名册”放在同一页里，
 * 避免玩家在多个子弹窗之间来回切换。
 */
function toNumber(value, fallback = 0) {
  const numericValue = Number(value)
  return Number.isFinite(numericValue) ? numericValue : fallback
}

function normalizeGuild(guild) {
  // 中文注释：
  // 后端当前仍有 PascalCase / camelCase 混用的字段命名，
  // 这里先统一归一化，再交给模板渲染。
  return {
    guildId: guild?.GuildId ?? guild?.guildId ?? '',
    name: guild?.Name ?? guild?.name ?? '未命名宗门',
    level: toNumber(guild?.Level ?? guild?.level, 1),
    memberCount: toNumber(guild?.MemberCount ?? guild?.memberCount, 0),
    maxMembers: toNumber(guild?.MaxMembers ?? guild?.maxMembers, 0),
    leaderName: guild?.LeaderName ?? guild?.leaderName ?? '未知宗主'
  }
}

export default {
  name: 'GuildModal',

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
    const guilds = ref([])
    const guildName = ref('')
    const isLoading = ref(false)
    const isSubmitting = ref(false)
    const statusMessage = ref('')

    const player = computed(() => gameStore.state.player || {})
    const currentGuildId = computed(() => player.value?.guildId ?? player.value?.GuildId ?? '')
    const guildContribution = computed(() => {
      return toNumber(player.value?.guildContribution ?? player.value?.GuildContribution, 0)
    })
    const isInGuild = computed(() => Boolean(currentGuildId.value))
    const currentGuild = computed(() => {
      return guilds.value.find((guild) => guild.guildId === currentGuildId.value) || null
    })

    async function loadGuildData() {
      isLoading.value = true

      try {
        // 中文注释：
        // 宗门 UI 依赖玩家当前 guildId 和贡献值，所以刷新名册前顺手刷新一次玩家快照。
        await gameStore.refreshPlayerSnapshot({ includeRankings: false })
        const guildList = await apiClient.getGuilds()
        guilds.value = Array.isArray(guildList)
          ? guildList.map(normalizeGuild)
          : []
        statusMessage.value = ''
      } catch (error) {
        statusMessage.value = error.message || '加载宗门数据失败。'
      } finally {
        isLoading.value = false
      }
    }

    async function createGuild() {
      if (guildName.value.length < 2 || isSubmitting.value || isInGuild.value) {
        return
      }

      isSubmitting.value = true

      try {
        const result = await apiClient.createGuild(guildName.value)
        statusMessage.value = result?.message || result?.Message || `宗门 ${guildName.value} 已建立。`
        guildName.value = ''
        await gameStore.refreshPlayerSnapshot({ includeRankings: false })
        await loadGuildData()
      } catch (error) {
        statusMessage.value = error.message || '创建宗门失败。'
      } finally {
        isSubmitting.value = false
      }
    }

    async function joinGuild(guildId) {
      if (!guildId || isSubmitting.value || isInGuild.value) {
        return
      }

      isSubmitting.value = true

      try {
        const result = await apiClient.joinGuild(guildId)
        statusMessage.value = result?.message || result?.Message || '已加入宗门。'
        await gameStore.refreshPlayerSnapshot({ includeRankings: false })
        await loadGuildData()
      } catch (error) {
        statusMessage.value = error.message || '加入宗门失败。'
      } finally {
        isSubmitting.value = false
      }
    }

    async function leaveCurrentGuild() {
      if (!isInGuild.value || isSubmitting.value) {
        return
      }

      isSubmitting.value = true

      try {
        const result = await apiClient.leaveGuild()
        statusMessage.value = result?.message || result?.Message || '已退出宗门。'
        await gameStore.refreshPlayerSnapshot({ includeRankings: false })
        await loadGuildData()
      } catch (error) {
        statusMessage.value = error.message || '退出宗门失败。'
      } finally {
        isSubmitting.value = false
      }
    }

    watch(() => props.modelValue, async (visible) => {
      if (!visible) {
        return
      }

      await loadGuildData()
    })

    return {
      ICON,
      guilds,
      guildName,
      isLoading,
      isSubmitting,
      statusMessage,
      currentGuildId,
      currentGuild,
      guildContribution,
      isInGuild,
      loadGuildData,
      createGuild,
      joinGuild,
      leaveCurrentGuild
    }
  }
}
</script>

<style scoped>
.guild-modal {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}

.toolbar-row,
.guild-card-top,
.guild-card-footer,
.guild-name-row,
.list-header {
  display: flex;
  justify-content: space-between;
  gap: var(--spacing-sm);
}

.toolbar-row {
  align-items: center;
}

.toolbar-copy,
.hint-text,
.guild-card-meta,
.guild-id,
.guild-meta,
.guild-player-meta,
.list-count {
  color: var(--text-secondary);
}

.refresh-btn,
.primary-btn,
.danger-btn,
.join-btn {
  border: 1px solid transparent;
  border-radius: var(--radius-sm);
  cursor: pointer;
  transition: all 0.25s ease;
  font-family: var(--font-primary);
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
.danger-btn:disabled,
.join-btn:disabled {
  opacity: 0.55;
  cursor: not-allowed;
}

.guild-layout {
  display: grid;
  grid-template-columns: 300px minmax(0, 1fr);
  gap: var(--spacing-md);
}

.side-panel,
.guild-list {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}

.panel-card,
.guild-card {
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
}

.panel-card {
  padding: var(--spacing-md);
}

.panel-title {
  color: var(--highlight-text);
  margin-bottom: var(--spacing-md);
  font-weight: 600;
}

.current-guild-card,
.empty-card {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
}

.guild-name {
  color: var(--text-primary);
  font-size: var(--font-size-lg);
  font-weight: 600;
}

.guild-level,
.member-badge {
  align-self: flex-start;
  padding: 4px 10px;
  border-radius: 999px;
  background: var(--highlight-soft-bg);
  color: var(--highlight-text);
}

.guild-meta,
.guild-player-meta {
  display: flex;
  flex-direction: column;
  gap: 4px;
  font-size: var(--font-size-sm);
}

.input-group {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
  margin-bottom: var(--spacing-md);
}

.input-group label {
  color: var(--text-secondary);
}

.primary-btn,
.danger-btn,
.join-btn {
  padding: var(--spacing-sm) var(--spacing-md);
}

.primary-btn {
  width: 100%;
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
}

.list-panel {
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  padding: var(--spacing-md);
}

.guild-list {
  max-height: 520px;
  overflow-y: auto;
}

.guild-card {
  padding: var(--spacing-md);
}

.guild-card-name {
  color: var(--text-primary);
  font-size: var(--font-size-md);
  font-weight: 600;
}

.guild-card-footer {
  align-items: center;
  margin-top: var(--spacing-sm);
}

.join-btn {
  background: var(--button-info-start);
  border-color: oklch(0.6 0.12 250 / 0.25);
  color: var(--accent-text);
}

.join-btn:hover:not(:disabled) {
  background: oklch(0.6 0.12 250 / 0.2);
  color: var(--text-primary);
}

.empty-card,
.empty-list {
  min-height: 160px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: var(--spacing-sm);
  color: var(--text-secondary);
}

.empty-icon {
  font-size: 28px;
}

.status-message {
  padding: var(--spacing-sm) var(--spacing-md);
  background: rgba(124, 58, 237, 0.12);
  border: 1px solid rgba(124, 58, 237, 0.28);
  border-radius: var(--radius-sm);
  color: var(--text-primary);
}

@media (max-width: 900px) {
  .toolbar-row,
  .guild-layout {
    grid-template-columns: 1fr;
    display: grid;
  }
}
</style>
