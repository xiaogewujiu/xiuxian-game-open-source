<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">邮件</p>
        <h2 class="section-title section-title--small">邮件管理</h2>
        <p class="section-note">查看已发送邮件、发送新邮件（单人/全服），支持配置附件。</p>
      </div>

      <div class="filter-grid">
        <label class="field">
          <span>关键字</span>
          <input v-model.trim="keyword" type="text" placeholder="搜索标题/收件人" />
        </label>
        <label class="field">
          <span>邮件类型</span>
          <select v-model="globalFilter">
            <option value="">全部</option>
            <option value="true">全服邮件</option>
            <option value="false">个人邮件</option>
          </select>
        </label>

        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadMails">查询</button>
          <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="openSendModal">发送邮件</button>
        </div>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">邮件列表</p>
          <h3 class="section-title section-title--small">邮件列表</h3>
        </div>
        <span class="selected-pill">共 {{ mails.length }} 条</span>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>标题</th>
              <th>发件人</th>
              <th>类型</th>
              <th>附件</th>
              <th>发送时间</th>
              <th>过期时间</th>
              <th>操作</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="mail in mails" :key="mail.id">
              <td>{{ mail.id }}</td>
              <td>{{ mail.title }}</td>
              <td>{{ mail.senderName }}</td>
              <td>{{ mail.isGlobal ? '全服' : '个人' }}</td>
              <td>{{ mail.hasAttachments ? '有' : '无' }}</td>
              <td>{{ formatTime(mail.createdAt) }}</td>
              <td>{{ formatTime(mail.expireAt) }}</td>
              <td>
                <button class="secondary-button" type="button" @click="viewDetail(mail.id)">查看</button>
                <button v-if="mail.isGlobal" class="warning-button" type="button" :disabled="!canManageConfigs" @click="recallMail(mail.id)">撤回</button>
                <button class="danger-button" type="button" :disabled="!canManageConfigs" @click="removeMail(mail.id)">删除</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>

    <!-- 发送邮件弹窗 -->
    <AdminModal
      v-model="sendModalOpen"
      kicker="发送邮件"
      title="发送邮件"
      description="配置收件人、标题正文和附件内容。"
      size="xwide"
    >
      <fieldset class="form-fieldset" :disabled="!canManageConfigs">
        <div class="editor-grid editor-grid--three">
          <label class="field checkbox-field">
            <input v-model="sendForm.isGlobal" type="checkbox" />
            <span>全服邮件</span>
          </label>
          <label class="field" v-if="!sendForm.isGlobal">
            <span>收件人</span>
            <select v-model="sendForm.recipientId">
              <option value="">请选择玩家</option>
              <option v-for="p in playerOptions" :key="p.playerId" :value="p.playerId">{{ p.name }} (Lv.{{ p.level }})</option>
            </select>
          </label>
          <label class="field">
            <span>发件人名称</span>
            <input v-model.trim="sendForm.senderName" type="text" placeholder="GM" />
          </label>
        </div>

        <label class="field">
          <span>邮件标题</span>
          <input v-model.trim="sendForm.title" type="text" placeholder="请输入邮件标题" />
        </label>

        <label class="field">
          <span>邮件正文</span>
          <textarea v-model="sendForm.content" rows="4" placeholder="请输入邮件正文"></textarea>
        </label>

        <div class="section-header-copy" style="margin-top: 1rem;">
          <h4 class="section-title section-title--small">附件配置</h4>
        </div>

        <div class="editor-grid editor-grid--three">
          <label class="field">
            <span>金币</span>
            <input v-model.number="attachments.gold" type="number" min="0" />
          </label>
          <label class="field">
            <span>灵石</span>
            <input v-model.number="attachments.spiritStone" type="number" min="0" />
          </label>
        </div>

        <div class="section-header-copy" style="margin-top: 0.5rem;">
          <h4 class="section-title section-title--small">道具附件</h4>
          <button class="secondary-button" type="button" @click="addItemAttachment">添加道具</button>
        </div>
        <div v-for="(item, index) in attachments.items" :key="index" class="editor-grid editor-grid--three" style="margin-bottom: 0.5rem;">
          <label class="field">
            <span>道具</span>
            <select v-model="item.itemId">
              <option value="">请选择道具</option>
              <option v-for="it in itemOptions" :key="it.itemId" :value="it.itemId">{{ it.name }} ({{ it.itemId }})</option>
            </select>
          </label>
          <label class="field">
            <span>数量</span>
            <input v-model.number="item.quantity" type="number" min="1" />
          </label>
          <div class="field">
            <button class="danger-button" type="button" @click="removeItemAttachment(index)">移除</button>
          </div>
        </div>

        <div class="section-header-copy" style="margin-top: 0.5rem;">
          <h4 class="section-title section-title--small">装备附件</h4>
          <button class="secondary-button" type="button" @click="addEquipmentAttachment">添加装备</button>
        </div>
        <div v-for="(equip, index) in attachments.equipment" :key="index" class="editor-grid editor-grid--three" style="margin-bottom: 0.5rem;">
          <label class="field">
            <span>装备模板</span>
            <select v-model="equip.templateId">
              <option value="">请选择装备</option>
              <option v-for="eq in equipmentOptions" :key="eq.equipmentId" :value="String(eq.equipmentId)">{{ eq.name }} (Lv.{{ eq.level }})</option>
            </select>
          </label>
          <label class="field">
            <span>品质</span>
            <input v-model.number="equip.quality" type="number" min="1" max="5" />
          </label>
          <div class="field">
            <button class="danger-button" type="button" @click="removeEquipmentAttachment(index)">移除</button>
          </div>
        </div>

        <div class="section-header-copy" style="margin-top: 0.5rem;">
          <h4 class="section-title section-title--small">称号附件</h4>
          <button class="secondary-button" type="button" @click="addTitleAttachment">添加称号</button>
        </div>
        <div v-for="(title, index) in attachments.titles" :key="index" class="editor-grid editor-grid--three" style="margin-bottom: 0.5rem;">
          <label class="field">
            <span>称号</span>
            <select v-model="title.titleId">
              <option value="">请选择称号</option>
              <option v-for="tp in titleOptions" :key="tp.titleId" :value="tp.titleId">{{ tp.name }} ({{ tp.titleId }})</option>
            </select>
          </label>
          <div class="field">
            <button class="danger-button" type="button" @click="removeTitleAttachment(index)">移除</button>
          </div>
        </div>
      </fieldset>

      <template #footer>
        <button class="secondary-button" type="button" @click="sendModalOpen = false">取消</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="sendMail">发送</button>
      </template>
    </AdminModal>

    <!-- 邮件详情弹窗 -->
    <AdminModal
      v-model="detailModalOpen"
      kicker="邮件详情"
      :title="`邮件 #${selectedMail?.id ?? ''}`"
      description="查看邮件完整内容。"
      size="wide"
    >
      <div v-if="selectedMail" class="detail-overview detail-overview--compact">
        <article class="detail-overview__item"><span>发件人</span><strong>{{ selectedMail.senderName }}（{{ selectedMail.senderType }}）</strong></article>
        <article class="detail-overview__item"><span>类型</span><strong>{{ selectedMail.isGlobal ? '全服邮件' : '个人邮件' }}</strong></article>
        <article class="detail-overview__item"><span>收件人</span><strong>{{ selectedMail.recipientId || '全服' }}</strong></article>
        <article class="detail-overview__item"><span>发送时间</span><strong>{{ formatTime(selectedMail.createdAt) }}</strong></article>
        <article class="detail-overview__item"><span>过期时间</span><strong>{{ formatTime(selectedMail.expireAt) }}</strong></article>
      </div>

      <div v-if="selectedMail" style="margin-top: 1rem;">
        <label class="field"><span>标题</span><input :value="selectedMail.title" type="text" disabled /></label>
        <label class="field"><span>正文</span><textarea :value="selectedMail.content" rows="6" disabled></textarea></label>
        <label class="field" v-if="selectedMail.attachmentsJson">
          <span>附件JSON</span>
          <textarea :value="selectedMail.attachmentsJson" rows="4" disabled></textarea>
        </label>
      </div>

      <template #footer>
        <button class="secondary-button" type="button" @click="detailModalOpen = false">关闭</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import toast from '@/utils/toast'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import { deleteAdminMail, getAdminMailDetail, getAdminMails, sendAdminMail, recallGlobalMail } from '@/services/mail'
import { getAdminPlayers } from '@/services/players'
import { getAdminItems } from '@/services/items'
import { getAdminEquipments } from '@/services/equipments'
import { getAdminTitles } from '@/services/title'
import type { AdminMailDetail, AdminMailListItem, AdminSendMailPayload, MailAttachments, AdminPlayerListItem, AdminItemListItem, AdminEquipmentListItem, AdminTitleListItem } from '@/types/admin'

const { canManageConfigs } = useAdminPermissions()
const mails = ref<AdminMailListItem[]>([])
const keyword = ref('')
const globalFilter = ref('')
const playerOptions = ref<AdminPlayerListItem[]>([])
const itemOptions = ref<AdminItemListItem[]>([])
const equipmentOptions = ref<AdminEquipmentListItem[]>([])
const titleOptions = ref<AdminTitleListItem[]>([])
const sendModalOpen = ref(false)
const detailModalOpen = ref(false)
const selectedMail = ref<AdminMailDetail | null>(null)

const sendForm = reactive<AdminSendMailPayload>({
  recipientId: null,
  isGlobal: false,
  senderName: 'GM',
  title: '',
  content: '',
  attachmentsJson: null
})

const attachments = reactive<MailAttachments>({
  gold: 0,
  spiritStone: 0,
  items: [],
  equipment: [],
  titles: []
})

function formatTime(value: string) {
  if (!value) return '-'
  return new Date(value).toLocaleString('zh-CN')
}

function addItemAttachment() {
  if (!attachments.items) attachments.items = []
  attachments.items.push({ itemId: '', quantity: 1 })
}

function removeItemAttachment(index: number) {
  attachments.items?.splice(index, 1)
}

function addEquipmentAttachment() {
  if (!attachments.equipment) attachments.equipment = []
  attachments.equipment.push({ templateId: '', quality: 1 })
}

function removeEquipmentAttachment(index: number) {
  attachments.equipment?.splice(index, 1)
}

function addTitleAttachment() {
  if (!attachments.titles) attachments.titles = []
  attachments.titles.push({ titleId: '' })
}

function removeTitleAttachment(index: number) {
  attachments.titles?.splice(index, 1)
}

function buildAttachmentsJson(): string | null {
  const hasGold = attachments.gold > 0
  const hasSpiritStone = attachments.spiritStone > 0
  const hasItems = attachments.items && attachments.items.some(i => i.itemId && i.quantity > 0)
  const hasEquipment = attachments.equipment && attachments.equipment.some(e => e.templateId)
  const hasTitles = attachments.titles && attachments.titles.some(t => t.titleId)

  if (!hasGold && !hasSpiritStone && !hasItems && !hasEquipment && !hasTitles) return null

  const payload: MailAttachments = {
    gold: attachments.gold || 0,
    spiritStone: attachments.spiritStone || 0
  }

  if (hasItems) {
    payload.items = attachments.items!.filter(i => i.itemId && i.quantity > 0)
  }

  if (hasEquipment) {
    payload.equipment = attachments.equipment!.filter(e => e.templateId)
  }

  if (hasTitles) {
    payload.titles = attachments.titles!.filter(t => t.titleId)
  }

  return JSON.stringify(payload)
}

function resetSendForm() {
  sendForm.recipientId = null
  sendForm.isGlobal = false
  sendForm.senderName = 'GM'
  sendForm.title = ''
  sendForm.content = ''
  sendForm.attachmentsJson = null
  attachments.gold = 0
  attachments.spiritStone = 0
  attachments.items = []
  attachments.equipment = []
  attachments.titles = []
}

function openSendModal() {
  if (!canManageConfigs.value) return
  resetSendForm()
  sendModalOpen.value = true
}

async function loadMails() {
  const isGlobal = globalFilter.value === '' ? null : globalFilter.value === 'true'
  mails.value = await getAdminMails(keyword.value, isGlobal)
}

async function viewDetail(mailId: number) {
  selectedMail.value = await getAdminMailDetail(mailId)
  detailModalOpen.value = true
}

async function sendMail() {
  if (!canManageConfigs.value) return
  if (!sendForm.title) {
    toast.error('邮件标题不能为空。')
    return
  }
  if (!sendForm.content) {
    toast.error('邮件正文不能为空。')
    return
  }
  if (!sendForm.isGlobal && !sendForm.recipientId) {
    toast.error('非全服邮件必须指定收件人。')
    return
  }

  try {
    sendForm.attachmentsJson = buildAttachmentsJson()
    await sendAdminMail({ ...sendForm })
    toast.success('邮件发送成功。')
    sendModalOpen.value = false
    await loadMails()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '邮件发送失败。')
  }
}

async function removeMail(mailId: number) {
  if (!canManageConfigs.value) return
  if (!window.confirm(`确认删除邮件 #${mailId} 吗？`)) return
  try {
    await deleteAdminMail(mailId)
    toast.success('邮件已删除。')
    await loadMails()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '邮件删除失败。')
  }
}

async function recallMail(mailId: number) {
  if (!canManageConfigs.value) return
  if (!window.confirm(`确认撤回全服邮件 #${mailId} 吗？撤回后所有玩家的领取记录将被清除。`)) return
  try {
    await recallGlobalMail(mailId)
    toast.success('全服邮件已撤回。')
    await loadMails()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '邮件撤回失败。')
  }
}

onMounted(async () => {
  await loadMails()
  const [players, items, equipments, titles] = await Promise.all([
    getAdminPlayers(),
    getAdminItems(),
    getAdminEquipments(),
    getAdminTitles()
  ])
  playerOptions.value = players
  itemOptions.value = items
  equipmentOptions.value = equipments
  titleOptions.value = titles
})
</script>
