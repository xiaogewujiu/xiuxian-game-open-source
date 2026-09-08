<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">宗门监控</p>
        <h2 class="section-title section-title--small">玩家宗门管理</h2>
        <p class="section-note">查看和管理玩家宗门。点击列表行查看详情。</p>
      </div>

      <div class="filter-grid">
        <label class="field">
          <span>关键字</span>
          <input v-model.trim="keyword" type="text" placeholder="搜索宗门名称或盟主" />
        </label>
        <label class="field">
          <span>当前宗门</span>
          <input :value="selected?.guildId || ''" type="text" placeholder="未选择" disabled />
        </label>

        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadList">刷新</button>
        </div>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">宗门列表</p>
          <h3 class="section-title section-title--small">玩家宗门列表</h3>
        </div>
        <span class="selected-pill">共 {{ filteredList.length }} 条</span>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>宗门编号</th>
              <th>宗门名称</th>
              <th>所属模板</th>
              <th>等级</th>
              <th>人数</th>
              <th>盟主</th>
              <th>总捐献</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="item in filteredList"
              :key="item.guildId"
              :class="{ 'is-active': selected?.guildId === item.guildId }"
              @click="openDetail(item.guildId)"
            >
              <td>{{ item.guildId }}</td>
              <td>{{ item.name }}</td>
              <td>{{ item.sectName }}</td>
              <td>{{ item.level }}</td>
              <td>{{ item.memberCount }}</td>
              <td>{{ item.leaderName }}</td>
              <td>{{ item.totalDonation }}</td>
            </tr>
          </tbody>
        </table>
      </div>

    </section>

    <AdminModal
      v-model="detailOpen"
      kicker="宗门详情"
      :title="selected ? `宗门 ${selected.name}` : '宗门详情'"
      description="查看玩家宗门的详细信息（只读）。"
      size="xwide"
    >
      <fieldset v-if="selected" class="form-fieldset">
        <div class="detail-overview">
          <article class="detail-overview__item"><span>宗门编号</span><strong>{{ selected.guildId }}</strong></article>
          <article class="detail-overview__item"><span>宗门名称</span><strong>{{ selected.name }}</strong></article>
          <article class="detail-overview__item"><span>所属模板</span><strong>{{ selected.sectName }} ({{ selected.sectTemplateId }})</strong></article>
          <article class="detail-overview__item"><span>等级</span><strong>{{ selected.level }}</strong></article>
          <article class="detail-overview__item"><span>经验</span><strong>{{ selected.exp }}</strong></article>
          <article class="detail-overview__item"><span>人数</span><strong>{{ selected.memberCount }} / {{ selected.maxMembers }}</strong></article>
          <article class="detail-overview__item"><span>资金</span><strong>{{ selected.funds }}</strong></article>
          <article class="detail-overview__item"><span>盟主</span><strong>{{ selected.leaderName }} ({{ selected.leaderId }})</strong></article>
          <article class="detail-overview__item"><span>总捐献</span><strong>{{ selected.totalDonation }}</strong></article>
          <article class="detail-overview__item"><span>创建时间</span><strong>{{ formatDateTime(selected.createTime) }}</strong></article>
        </div>
        <label class="field"><span>公告</span><textarea :value="selected.announcement" rows="4" readonly></textarea></label>
      </fieldset>

      <template #footer>
        <button class="secondary-button" type="button" @click="detailOpen = false">关闭</button>
        <button class="danger-button" type="button" :disabled="!canManageConfigs || !selected?.guildId" @click="remove">删除宗门</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import { getGuilds, getGuild, deleteGuild } from '@/services/sect'
import type { AdminGuildDetail, AdminGuildListItem } from '@/types/admin'
import toast from '@/utils/toast'

const { canManageConfigs } = useAdminPermissions()
const keyword = ref('')
const list = ref<AdminGuildListItem[]>([])
const selected = ref<AdminGuildDetail | null>(null)
const detailOpen = ref(false)

const filteredList = computed(() => {
  if (!keyword.value) return list.value
  const kw = keyword.value.toLowerCase()
  return list.value.filter((item) =>
    item.name.toLowerCase().includes(kw) ||
    item.leaderName.toLowerCase().includes(kw) ||
    item.sectName.toLowerCase().includes(kw)
  )
})

function formatDateTime(value?: string | null) {
  if (!value) return '-'
  return value.replace('T', ' ').slice(0, 19)
}

async function openDetail(guildId: string) {
  selected.value = await getGuild(guildId)
  detailOpen.value = true
}

async function loadList() {
  list.value = await getGuilds()
}

async function remove() {
  if (!canManageConfigs.value || !selected.value?.guildId) return
  if (!window.confirm(`确认删除宗门 ${selected.value.name} 吗？此操作不可恢复。`)) return

  try {
    await deleteGuild(selected.value.guildId)
    selected.value = null
    toast.success('宗门删除成功。')
    detailOpen.value = false
    await loadList()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '删除失败。')
  }
}

onMounted(loadList)
</script>
