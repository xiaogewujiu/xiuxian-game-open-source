<template>
  <XiuXianModal :model-value="modelValue" title="邮件" :width="500" @update:model-value="$emit('update:modelValue', $event)">
    <div class="mail-modal">
      <!-- 操作栏 -->
      <div class="mail-actions">
        <span class="mail-count">未读: {{ unreadCount }}</span>
        <button class="action-btn" :disabled="isLoading" @click="markAllRead">全部已读</button>
        <button class="action-btn claim-btn" :disabled="isLoading" @click="claimAll">一键领取</button>
      </div>

      <!-- 邮件列表 -->
      <div v-if="!selectedMail" class="mail-list">
        <div v-if="mails.length === 0" class="empty-state">暂无邮件</div>
        <div
          v-for="mail in mails"
          :key="mail.id"
          class="mail-item"
          :class="{ unread: !mail.isRead }"
          @click="openMail(mail)"
        >
          <div class="mail-item-header">
            <span class="mail-dot" :class="{ active: !mail.isRead }"></span>
            <span class="mail-title">{{ mail.title }}</span>
            <span v-if="mail.hasAttachments && !mail.isClaimed" class="mail-badge">附件</span>
            <span v-if="mail.hasAttachments && mail.isClaimed" class="mail-badge claimed">已领取</span>
          </div>
          <div class="mail-item-footer">
            <span class="mail-sender">{{ mail.senderName }}</span>
            <span class="mail-time">{{ formatTime(mail.createdAt) }}</span>
          </div>
        </div>

        <!-- 分页 -->
        <div v-if="totalPages > 1" class="mail-pagination">
          <button class="page-btn" :disabled="currentPage <= 1 || isLoading" @click="loadPage(currentPage - 1)">上一页</button>
          <span class="page-info">{{ currentPage }}/{{ totalPages }}</span>
          <button class="page-btn" :disabled="currentPage >= totalPages || isLoading" @click="loadPage(currentPage + 1)">下一页</button>
        </div>
      </div>

      <!-- 邮件详情 -->
      <div v-else class="mail-detail">
        <button class="back-btn" @click="selectedMail = null">返回列表</button>

        <div class="mail-detail-header">
          <h3 class="mail-detail-title">{{ selectedMail.title }}</h3>
          <div class="mail-detail-meta">
            <span>发件人: {{ selectedMail.senderName }}</span>
            <span>{{ formatTime(selectedMail.createdAt) }}</span>
          </div>
        </div>

        <div class="mail-detail-content">{{ selectedMail.content }}</div>

        <!-- 附件 -->
        <div v-if="parsedAttachments" class="mail-attachments">
          <div class="attachments-title">附件</div>
          <div class="attachment-list">
            <div v-if="parsedAttachments.gold > 0" class="attachment-item">
              <AssetIcon :source="ICON.currency_gold" size="18" />
              <span>金币 x{{ parsedAttachments.gold }}</span>
            </div>
            <div v-if="parsedAttachments.spiritStone > 0" class="attachment-item">
              <AssetIcon :source="ICON.currency_spirit" size="18" />
              <span>灵石 x{{ parsedAttachments.spiritStone }}</span>
            </div>
            <div v-for="(item, index) in (parsedAttachments.items || [])" :key="'item-' + index" class="attachment-item">
              <AssetIcon :source="ICON.item_chest" size="18" />
              <span>{{ item.name || item.itemId }} x{{ item.quantity }}</span>
            </div>
            <div v-for="(equip, index) in (parsedAttachments.equipment || [])" :key="'equip-' + index" class="attachment-item">
              <AssetIcon :source="ICON.slot_armor" size="18" />
              <span>{{ equip.name || '装备' }}</span>
            </div>
            <div v-for="(title, index) in (parsedAttachments.titles || [])" :key="'title-' + index" class="attachment-item">
              <AssetIcon :source="ICON.nav_achievement" size="18" />
              <span>{{ title.name || title.titleId }}</span>
            </div>
          </div>

          <button
            v-if="!selectedMail.isClaimed"
            class="claim-attach-btn"
            :disabled="isLoading"
            @click="claimAttachments"
          >
            {{ isLoading ? '领取中...' : '领取附件' }}
          </button>
          <div v-else class="claimed-status">附件已领取</div>
        </div>

        <button class="delete-btn" :disabled="isLoading" @click="deleteMail">删除邮件</button>
      </div>

      <!-- 提示消息 -->
      <Transition name="fade">
        <div v-if="toastMsg" class="toast-msg" :class="toastType">{{ toastMsg }}</div>
      </Transition>
    </div>
  </XiuXianModal>
</template>

<script>
import { ref, computed, watch } from 'vue'
import XiuXianModal from '../common/XiuXianModal.vue'
import AssetIcon from '../common/AssetIcon.vue'
import { apiClient } from '../../lib/apiClient'
import { useGameStore } from '../../state/gameStore'
import { ICON } from '../../icons'

export default {
  name: 'MailModal',

  components: { XiuXianModal, AssetIcon },

  props: {
    modelValue: Boolean
  },

  emits: ['update:modelValue'],

  setup(props) {
    const gameStore = useGameStore()
    const mails = ref([])
    const selectedMail = ref(null)
    const unreadCount = ref(0)
    const currentPage = ref(1)
    const totalPages = ref(1)
    const isLoading = ref(false)
    const toastMsg = ref('')
    const toastType = ref('success')

    function showToast(msg, type = 'success') {
      toastMsg.value = msg
      toastType.value = type
      setTimeout(() => { toastMsg.value = '' }, 2000)
    }

    function formatTime(value) {
      if (!value) return ''
      return new Date(value).toLocaleString('zh-CN')
    }

    const parsedAttachments = computed(() => {
      const raw = selectedMail.value?.resolvedAttachmentsJson || selectedMail.value?.attachmentsJson
      if (!raw) return null
      try {
        return JSON.parse(raw)
      } catch {
        return null
      }
    })

    async function loadMails(page = 1) {
      isLoading.value = true
      try {
        const result = await apiClient.getMails(page, 20)
        mails.value = result.items || []
        unreadCount.value = result.unreadCount || 0
        currentPage.value = result.page || 1
        totalPages.value = Math.ceil((result.total || 0) / (result.pageSize || 20))
      } catch (e) {
        showToast(e.message || '加载邮件失败', 'error')
      } finally {
        isLoading.value = false
      }
    }

    async function loadPage(page) {
      await loadMails(page)
    }

    async function openMail(mail) {
      isLoading.value = true
      try {
        selectedMail.value = await apiClient.getMailDetail(mail.id)
        // 标记已读
        if (!mail.isRead) {
          await apiClient.markMailAsRead(mail.id)
          mail.isRead = true
          unreadCount.value = Math.max(0, unreadCount.value - 1)
        }
      } catch (e) {
        showToast(e.message || '加载邮件详情失败', 'error')
      } finally {
        isLoading.value = false
      }
    }

    async function markAllRead() {
      isLoading.value = true
      try {
        await apiClient.markAllMailsAsRead()
        mails.value.forEach(m => { m.isRead = true })
        unreadCount.value = 0
        showToast('已全部标记为已读')
      } catch (e) {
        showToast(e.message || '操作失败', 'error')
      } finally {
        isLoading.value = false
      }
    }

    async function claimAttachments() {
      if (!selectedMail.value) return
      isLoading.value = true
      try {
        const result = await apiClient.claimMailAttachments(selectedMail.value.id)
        selectedMail.value.isClaimed = true
        // 更新列表中的状态
        const listItem = mails.value.find(m => m.id === selectedMail.value.id)
        if (listItem) listItem.isClaimed = true
        showToast(result.message || '附件领取成功')
        await gameStore.loadPlayer(true)
      } catch (e) {
        const msg = e.message || '领取失败'
        // 如果是已领取的错误，同步更新本地状态
        if (msg.includes('已领取')) {
          selectedMail.value.isClaimed = true
          const listItem = mails.value.find(m => m.id === selectedMail.value.id)
          if (listItem) listItem.isClaimed = true
        }
        showToast(msg, 'error')
      } finally {
        isLoading.value = false
      }
    }

    async function claimAll() {
      isLoading.value = true
      try {
        const result = await apiClient.claimAllMailAttachments()
        showToast(`成功领取 ${result.claimedCount || 0} 封邮件附件`)
        await loadMails(currentPage.value)
        await gameStore.loadPlayer(true)
      } catch (e) {
        showToast(e.message || '领取失败', 'error')
      } finally {
        isLoading.value = false
      }
    }

    async function deleteMail() {
      if (!selectedMail.value) return
      isLoading.value = true
      try {
        await apiClient.deleteMail(selectedMail.value.id)
        showToast('邮件已删除')
        selectedMail.value = null
        await loadMails(currentPage.value)
      } catch (e) {
        showToast(e.message || '删除失败', 'error')
      } finally {
        isLoading.value = false
      }
    }

    watch(() => props.modelValue, (val) => {
      if (val) {
        selectedMail.value = null
        loadMails()
      }
    })

    return {
      ICON,
      mails,
      selectedMail,
      unreadCount,
      currentPage,
      totalPages,
      isLoading,
      toastMsg,
      toastType,
      parsedAttachments,
      formatTime,
      loadPage,
      openMail,
      markAllRead,
      claimAttachments,
      claimAll,
      deleteMail
    }
  }
}
</script>

<style scoped>
.mail-modal {
  padding: 0.5rem;
  min-height: 300px;
  position: relative;
}

.mail-actions {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 0.75rem;
  padding-bottom: 0.5rem;
  border-bottom: 1px solid rgba(255,255,255,0.1);
}

.mail-count {
  font-size: 0.85rem;
  color: #f59e0b;
  margin-right: auto;
}

.action-btn {
  background: rgba(255,255,255,0.08);
  border: 1px solid rgba(255,255,255,0.15);
  color: #e2e8f0;
  padding: 0.3rem 0.6rem;
  border-radius: 6px;
  font-size: 0.8rem;
  cursor: pointer;
  transition: all 0.2s;
}

.action-btn:hover:not(:disabled) {
  background: rgba(255,255,255,0.15);
}

.action-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.claim-btn {
  background: rgba(34,197,94,0.2);
  border-color: rgba(34,197,94,0.3);
  color: #22c55e;
}

.claim-btn:hover:not(:disabled) {
  background: rgba(34,197,94,0.3);
}

.mail-list {
  max-height: 400px;
  overflow-y: auto;
}

.empty-state {
  text-align: center;
  color: #94a3b8;
  padding: 2rem;
  font-size: 0.9rem;
}

.mail-item {
  padding: 0.6rem;
  border-radius: 8px;
  cursor: pointer;
  transition: background 0.2s;
  margin-bottom: 0.25rem;
}

.mail-item:hover {
  background: rgba(255,255,255,0.06);
}

.mail-item.unread {
  background: rgba(59,130,246,0.08);
}

.mail-item-header {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.mail-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: transparent;
  flex-shrink: 0;
}

.mail-dot.active {
  background: #3b82f6;
}

.mail-title {
  font-size: 0.9rem;
  color: var(--text-primary);
  flex: 1;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.mail-badge {
  font-size: 0.7rem;
  padding: 0.1rem 0.4rem;
  border-radius: 4px;
  background: rgba(245,158,11,0.2);
  color: #f59e0b;
  flex-shrink: 0;
}

.mail-badge.claimed {
  background: rgba(34,197,94,0.2);
  color: #22c55e;
}

.mail-item-footer {
  display: flex;
  justify-content: space-between;
  margin-top: 0.3rem;
  font-size: 0.75rem;
  color: var(--text-secondary);
}

.mail-pagination {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 0.75rem;
  margin-top: 0.75rem;
  padding-top: 0.5rem;
  border-top: 1px solid rgba(255,255,255,0.1);
}

.page-btn {
  background: rgba(255,255,255,0.08);
  border: 1px solid rgba(255,255,255,0.15);
  color: #e2e8f0;
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  font-size: 0.8rem;
  cursor: pointer;
}

.page-btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.page-info {
  font-size: 0.8rem;
  color: #94a3b8;
}

/* 邮件详情 */
.mail-detail {
  max-height: 400px;
  overflow-y: auto;
}

.back-btn {
  background: none;
  border: none;
  color: #60a5fa;
  font-size: 0.85rem;
  cursor: pointer;
  padding: 0;
  margin-bottom: 0.75rem;
}

.back-btn:hover {
  color: #93bbfd;
}

.mail-detail-header {
  margin-bottom: 0.75rem;
}

.mail-detail-title {
  font-size: 1.05rem;
  color: var(--text-primary);
  margin: 0 0 0.4rem;
}

.mail-detail-meta {
  display: flex;
  justify-content: space-between;
  font-size: 0.78rem;
  color: var(--text-secondary);
}

.mail-detail-content {
  font-size: 0.88rem;
  color: var(--text-primary);
  line-height: 1.6;
  padding: 0.75rem;
  background: rgba(255,255,255,0.03);
  border-radius: 8px;
  margin-bottom: 0.75rem;
  white-space: pre-wrap;
}

.mail-attachments {
  background: rgba(34,197,94,0.06);
  border: 1px solid rgba(34,197,94,0.15);
  border-radius: 8px;
  padding: 0.75rem;
  margin-bottom: 0.75rem;
}

.attachments-title {
  font-size: 0.85rem;
  color: #22c55e;
  font-weight: 600;
  margin-bottom: 0.5rem;
}

.attachment-list {
  display: flex;
  flex-direction: column;
  gap: 0.3rem;
}

.attachment-item {
  font-size: 0.85rem;
  color: var(--text-primary);
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.25rem 0;
}

.attachment-icon {
  font-size: 0.9rem;
}

.claim-attach-btn {
  width: 100%;
  margin-top: 0.5rem;
  padding: 0.5rem;
  background: var(--button-primary-start);
  border: none;
  color: var(--button-primary-text);
  border-radius: 8px;
  font-size: 0.9rem;
  cursor: pointer;
  transition: all 0.2s;
}

.claim-attach-btn:hover:not(:disabled) {
  background: var(--button-primary-hover-start);
  color: var(--button-primary-hover-text);
}

.claim-attach-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.claimed-status {
  margin-top: 0.5rem;
  text-align: center;
  font-size: 0.82rem;
  color: #22c55e;
}

.delete-btn {
  width: 100%;
  padding: 0.4rem;
  background: rgba(239,68,68,0.15);
  border: 1px solid rgba(239,68,68,0.3);
  color: #ef4444;
  border-radius: 6px;
  font-size: 0.82rem;
  cursor: pointer;
  transition: background 0.2s;
}

.delete-btn:hover:not(:disabled) {
  background: rgba(239,68,68,0.25);
}

.delete-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* Toast */
.toast-msg {
  position: absolute;
  bottom: 0.5rem;
  left: 50%;
  transform: translateX(-50%);
  padding: 0.4rem 1rem;
  border-radius: 6px;
  font-size: 0.82rem;
  z-index: 10;
  white-space: nowrap;
}

.toast-msg.success {
  background: rgba(34,197,94,0.9);
  color: white;
}

.toast-msg.error {
  background: rgba(239,68,68,0.9);
  color: white;
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
