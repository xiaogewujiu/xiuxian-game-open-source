<template>
  <XiuXianModal :model-value="modelValue" title="建议反馈" :width="680" @update:model-value="$emit('update:modelValue', $event)">
    <div class="feedback-modal">
      <div class="feedback-tabs">
        <button :class="{ active: tab === 'submit' }" type="button" @click="tab = 'submit'; detail = null; error = ''">提交反馈</button>
        <button :class="{ active: tab === 'mine' }" type="button" @click="tab = 'mine'; detail = null; loadList()">我的反馈</button>
      </div>

      <form v-if="tab === 'submit'" class="feedback-form" @submit.prevent="submit">
        <label>反馈类型<select v-model="form.type"><option value="Suggestion">建议</option><option value="Bug">Bug</option><option value="Gameplay">玩法问题</option><option value="Account">账号问题</option><option value="Other">其他</option></select></label>
        <label>标题<input v-model.trim="form.title" maxlength="80" placeholder="请输入反馈标题" /></label>
        <label>内容<textarea v-model.trim="form.content" maxlength="2000" rows="7" placeholder="请详细描述你的建议或遇到的问题"></textarea></label>
        <div class="upload-row"><span>图片附件（最多 5 张）</span><input type="file" accept="image/png,image/jpeg,image/webp,image/gif" multiple @change="uploadFiles" /></div>
        <div v-if="attachments.length" class="attachment-list"><div v-for="(item, index) in attachments" :key="item.relativePath" class="attachment-item"><button type="button" class="attachment-image-button" @click="previewImage = buildApiUrl(item.relativePath)"><img :src="buildApiUrl(item.relativePath)" /></button><button type="button" class="attachment-remove" @click="removeAttachment(index)">删除</button></div></div>
        <p v-if="error" class="error">{{ error }}</p>
        <button class="submit-button" :disabled="loading" type="submit">{{ loading ? '提交中...' : '提交反馈' }}</button>
      </form>

      <div v-else-if="tab === 'mine' && !detail" class="feedback-list">
        <p v-if="listLoading" class="empty">正在加载反馈...</p>
        <p v-else-if="!list.length" class="empty">暂无反馈记录</p>
        <button v-for="item in list" :key="item.id" class="feedback-item" type="button" @click="openDetail(item.id)">
          <span class="feedback-item-main"><strong>#{{ item.id }}</strong><span>{{ item.title }}</span></span>
          <b class="status-badge" :class="statusClass(item.status)">{{ statusText[item.status] || item.status }}</b>
          <small>{{ formatTime(item.createdAt) }}</small>
        </button>
      </div>

      <div v-else-if="tab === 'mine' && detail" class="feedback-detail">
        <button class="back" type="button" @click="detail = null">← 返回我的反馈</button>
        <div class="detail-header">
          <div>
            <p class="detail-kicker">反馈详情 · #{{ detail.id }}</p>
            <h3>{{ detail.title }}</h3>
          </div>
          <span class="status-badge" :class="statusClass(detail.status)">{{ statusText[detail.status] || detail.status }}</span>
        </div>
        <div class="detail-meta"><span>{{ typeText[detail.type] || detail.type }}</span><span>提交于 {{ formatTime(detail.createdAt) }}</span></div>
        <section class="detail-card">
          <div class="detail-card-title">反馈内容</div>
          <p class="content">{{ detail.content }}</p>
        </section>
        <section v-if="detail.attachments?.length" class="detail-card">
          <div class="detail-card-title">图片附件（{{ detail.attachments.length }}）</div>
          <div class="detail-attachments"><button v-for="item in detail.attachments" :key="item.id" type="button" class="detail-image-button" @click="previewImage = buildApiUrl(item.relativePath)"><img :src="buildApiUrl(item.relativePath)" :alt="item.originalFileName || '反馈图片'" /></button></div>
        </section>
        <section class="reply" :class="{ 'reply-empty': !detail.adminReply }">
          <strong>管理员回复</strong>
          <p>{{ detail.adminReply || '暂未回复，请耐心等待处理。' }}</p>
        </section>
      </div>
      <div v-if="previewImage" class="image-lightbox" @click.self="previewImage = ''"><button type="button" class="lightbox-close" @click="previewImage = ''">×</button><img :src="previewImage" alt="反馈图片大图" /></div>
    </div>
  </XiuXianModal>
</template>

<script>
import { ref, reactive } from 'vue'
import XiuXianModal from '../common/XiuXianModal.vue'
import { apiClient, buildApiUrl } from '../../lib/apiClient'

export default {
  name: 'FeedbackModal',
  components: { XiuXianModal },
  props: { modelValue: { type: Boolean, default: false } },
  emits: ['update:modelValue'],
  setup() {
    const tab = ref('submit'); const loading = ref(false); const listLoading = ref(false); const error = ref(''); const list = ref([]); const detail = ref(null); const attachments = ref([]); const previewImage = ref('')
    const form = reactive({ type: 'Suggestion', title: '', content: '' })
    const statusText = { Pending: '待处理', Processing: '处理中', Resolved: '已解决', Rejected: '不予处理', Closed: '已关闭' }
    const typeText = { Suggestion: '建议', Bug: 'Bug', Gameplay: '玩法问题', Account: '账号问题', Other: '其他' }
    async function loadList() { listLoading.value = true; error.value = ''; try { list.value = await apiClient.getFeedbackList() } catch (e) { error.value = e.message } finally { listLoading.value = false } }
    async function openDetail(id) { error.value = ''; try { detail.value = await apiClient.getFeedbackDetail(id) } catch (e) { error.value = e.message } }
    async function uploadFiles(event) { const files = Array.from(event.target.files || []); if (attachments.value.length + files.length > 5) { error.value = '最多上传 5 张图片'; return } for (const file of files) { try { attachments.value.push(await apiClient.uploadFeedbackAttachment(file)) } catch (e) { error.value = e.message } } event.target.value = '' }
    async function removeAttachment(index) { const item = attachments.value[index]; attachments.value.splice(index, 1); try { await apiClient.deleteFeedbackAttachment(item.relativePath) } catch (e) { /* 删除临时文件失败不阻断表单操作 */ } }
    async function submit() { error.value = ''; if (form.title.length < 2 || !form.content || form.content.length < 10) { error.value = '标题至少 2 个字符，内容至少 10 个字符'; return } loading.value = true; try { await apiClient.createFeedback({ ...form, attachments: attachments.value.map(x => ({ relativePath: x.relativePath, originalFileName: x.originalFileName, contentType: x.contentType, fileSize: x.fileSize })) }); form.type = 'Suggestion'; form.title = ''; form.content = ''; attachments.value = []; detail.value = null; previewImage.value = ''; error.value = ''; tab.value = 'mine'; await loadList() } catch (e) { error.value = e.message } finally { loading.value = false } }
    function formatTime(value) { return value ? new Date(value).toLocaleString() : '' }
    function statusClass(status) { return `status-${String(status || '').toLowerCase()}` }
    return { tab, loading, listLoading, error, list, detail, attachments, previewImage, form, statusText, typeText, loadList, openDetail, uploadFiles, removeAttachment, submit, formatTime, statusClass, buildApiUrl }
  }
}
</script>

<style scoped>
.feedback-tabs {
  display: flex;
  gap: var(--spacing-sm);
  margin-bottom: var(--spacing-lg);
  border-bottom: 1px solid var(--border-color);
}

.feedback-tabs button {
  padding: var(--spacing-sm) var(--spacing-lg);
  border: 0;
  border-bottom: 2px solid transparent;
  background: transparent;
  color: var(--text-secondary);
  cursor: pointer;
  transition: color 0.15s ease, border-color 0.15s ease;
}

.feedback-tabs button:hover,
.feedback-tabs .active {
  color: var(--accent-text);
  border-bottom-color: var(--accent-text);
}

.feedback-form {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}

.feedback-form label {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-xs);
  color: var(--text-primary);
  font-size: var(--font-size-sm);
}

.feedback-form input,
.feedback-form select,
.feedback-form textarea {
  width: 100%;
  box-sizing: border-box;
  padding: var(--spacing-sm) var(--spacing-md);
  border: 1px solid var(--control-border);
  border-radius: var(--radius-sm);
  background: var(--control-bg);
  color: var(--control-text);
  font: inherit;
  transition: border-color 0.15s ease, box-shadow 0.15s ease;
}

.feedback-form input::placeholder,
.feedback-form textarea::placeholder {
  color: var(--control-placeholder);
}

.feedback-form input:focus,
.feedback-form select:focus,
.feedback-form textarea:focus {
  outline: none;
  border-color: var(--accent-text);
  box-shadow: 0 0 0 3px var(--accent-soft-bg);
}

.upload-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--spacing-md);
  color: var(--text-primary);
  font-size: var(--font-size-sm);
}

.upload-row input {
  max-width: 260px;
  color: var(--text-secondary);
}

.attachment-list {
  display: flex;
  flex-wrap: wrap;
  gap: var(--spacing-sm);
}

.attachment-item {
  position: relative;
  overflow: hidden;
  width: 90px;
  height: 70px;
  border: 1px solid var(--border-color);
  border-radius: var(--radius-sm);
  background: var(--bg-overlay-light);
}

.attachment-list img {
  display: block;
  width: 90px;
  height: 70px;
  object-fit: cover;
}

.attachment-image-button {
  display: block;
  padding: 0;
  border: 0;
  background: transparent;
  cursor: zoom-in;
}

.attachment-remove {
  position: absolute;
  right: var(--spacing-xs);
  bottom: var(--spacing-xs);
  padding: 2px 5px;
  border: 1px solid var(--button-danger-start);
  border-radius: var(--radius-sm);
  background: var(--button-danger-start);
  color: var(--button-danger-text);
  cursor: pointer;
  font-size: var(--font-size-xs);
}

.attachment-item button {
  position: absolute;
  top: var(--spacing-xs);
  right: var(--spacing-xs);
  padding: 2px 5px;
  border: 1px solid var(--button-danger-start);
  border-radius: var(--radius-sm);
  background: var(--button-danger-start);
  color: var(--button-danger-text);
  cursor: pointer;
  font-size: var(--font-size-xs);
}

.attachment-item button:not(.attachment-remove) {
  position: static;
  padding: 0;
  border: 0;
  background: transparent;
  color: inherit;
  font-size: inherit;
}

.submit-button {
  padding: var(--spacing-sm) var(--spacing-md);
  border: 1px solid var(--button-primary-start);
  border-radius: var(--radius-sm);
  background: linear-gradient(135deg, var(--button-primary-start), var(--button-primary-end));
  color: var(--button-primary-text);
  cursor: pointer;
  font: inherit;
  transition: filter 0.15s ease, opacity 0.15s ease;
}

.submit-button:hover:not(:disabled) {
  background: linear-gradient(135deg, var(--button-primary-hover-start), var(--button-primary-hover-end));
  color: var(--button-primary-hover-text);
}

.submit-button:disabled {
  cursor: not-allowed;
  opacity: 0.55;
}

.error {
  margin: 0;
  color: var(--status-danger-text);
  font-size: var(--font-size-sm);
}

.empty {
  margin: 0;
  color: var(--text-secondary);
}

.feedback-item {
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto auto;
  gap: var(--spacing-md);
  width: 100%;
  margin-bottom: var(--spacing-sm);
  padding: var(--spacing-md);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-sm);
  background: var(--bg-overlay-light);
  color: var(--text-primary);
  text-align: left;
  cursor: pointer;
  transition: border-color 0.15s ease, background 0.15s ease;
}

.feedback-item-main {
  display: flex;
  min-width: 0;
  gap: var(--spacing-sm);
  align-items: center;
}

.feedback-item-main > span:last-child {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.status-badge {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 58px;
  padding: 3px 8px;
  border: 1px solid currentColor;
  border-radius: 999px;
  font-size: var(--font-size-xs);
  font-weight: 600;
  white-space: nowrap;
}

.status-pending { color: var(--status-warning-text); background: var(--status-warning-bg); }
.status-processing { color: var(--status-info-text); background: var(--status-info-bg); }
.status-resolved { color: var(--status-success-text); background: var(--status-success-bg); }
.status-rejected,
.status-closed { color: var(--text-secondary); background: var(--bg-overlay-light); }

.feedback-item:hover {
  border-color: var(--accent-text);
  background: var(--bg-overlay-medium);
}

.feedback-item b {
  color: var(--accent-text);
  font-weight: 600;
}

.feedback-item small {
  color: var(--text-secondary);
}

.feedback-detail .back {
  padding: 0;
  border: 0;
  background: transparent;
  color: var(--accent-text);
  cursor: pointer;
}

.detail-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: var(--spacing-md);
  margin-top: var(--spacing-md);
}

.detail-kicker {
  margin: 0 0 var(--spacing-xs);
  color: var(--text-secondary);
  font-size: var(--font-size-xs);
}

.detail-header h3 {
  margin: 0;
}

.detail-meta {
  display: flex;
  gap: var(--spacing-md);
  margin: var(--spacing-sm) 0 var(--spacing-lg);
  font-size: var(--font-size-sm);
}

.detail-card {
  margin-bottom: var(--spacing-md);
  padding: var(--spacing-md);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  background: var(--bg-overlay-light);
}

.detail-card-title {
  margin-bottom: var(--spacing-sm);
  color: var(--text-secondary);
  font-size: var(--font-size-sm);
  font-weight: 600;
}

.detail-attachments {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(120px, 1fr));
  gap: var(--spacing-sm);
}

.detail-image-button {
  padding: 0;
  border: 0;
  border-radius: var(--radius-sm);
  background: transparent;
  cursor: zoom-in;
}

.detail-image-button img {
  width: 100%;
  height: 100px;
  border: 1px solid var(--border-color);
  border-radius: var(--radius-sm);
  object-fit: cover;
}

.feedback-detail h3 {
  color: var(--text-primary);
}

.detail-meta {
  color: var(--text-secondary);
}

.content {
  color: var(--text-primary);
  white-space: pre-wrap;
  line-height: 1.7;
}

.reply {
  padding: var(--spacing-md);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-sm);
  background: var(--accent-soft-bg);
  color: var(--text-primary);
}

.reply strong {
  color: var(--accent-text);
}

.reply-empty {
  color: var(--text-secondary);
}

.image-lightbox {
  position: fixed;
  inset: 0;
  z-index: 1200;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: var(--spacing-xl);
  background: var(--overlay-bg);
  cursor: zoom-out;
}

.image-lightbox img {
  max-width: min(92vw, 1200px);
  max-height: 86vh;
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-lg);
  object-fit: contain;
}

.lightbox-close {
  position: absolute;
  top: var(--spacing-lg);
  right: var(--spacing-lg);
  width: 34px;
  height: 34px;
  border: 1px solid var(--border-color);
  border-radius: 50%;
  background: var(--control-bg);
  color: var(--control-text);
  cursor: pointer;
  font-size: 24px;
  line-height: 1;
}

@media (max-width: 640px) {
  .upload-row,
  .feedback-item {
    align-items: flex-start;
  }

  .feedback-item {
    grid-template-columns: 1fr;
  }

  .detail-header {
    flex-direction: column;
  }

  .upload-row input {
    max-width: none;
    width: 100%;
  }
}
</style>
