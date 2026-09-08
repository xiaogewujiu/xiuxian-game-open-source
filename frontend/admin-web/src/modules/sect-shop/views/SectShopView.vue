<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">宗门商店</p>
        <h2 class="section-title section-title--small">宗门贡献商店</h2>
        <p class="section-note">管理宗门贡献商店商品与价格。点击列表行打开编辑弹窗。</p>
      </div>

      <div class="filter-grid">
        <label class="field">
          <span>商店编号</span>
          <input v-model.trim="filterShopId" type="text" placeholder="按商店编号筛选" />
        </label>
        <label class="field">
          <span>当前商品</span>
          <input :value="selected?.gid || ''" type="text" placeholder="未选择" disabled />
        </label>

        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadList">查询</button>
          <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="openCreateModal">新建商品</button>
        </div>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">商品列表</p>
          <h3 class="section-title section-title--small">宗门商店商品</h3>
        </div>
        <span class="selected-pill">共 {{ list.length }} 条</span>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>记录编号</th>
              <th>商店编号</th>
              <th>商品编号</th>
              <th>商品类型</th>
              <th>贡献价</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="item in list"
              :key="item.gid"
              :class="{ 'is-active': selected?.gid === item.gid }"
              @click="openEditModal(item.gid)"
            >
              <td>{{ item.gid }}</td>
              <td>{{ item.shopId }}</td>
              <td>{{ item.itemId }}</td>
              <td>{{ item.itemType === 1 ? '装备' : '道具' }}</td>
              <td>{{ item.contributionCost }}</td>
            </tr>
          </tbody>
        </table>
      </div>

    </section>

    <AdminModal
      v-model="editorOpen"
      kicker="宗门商品编辑"
      :title="selected?.gid ? `编辑商品 ${selected.gid}` : '新建宗门商品'"
      description="维护商品类型、贡献价格、库存和限购规则。"
      size="xwide"
    >
      <fieldset v-if="selected" class="form-fieldset" :disabled="!canManageConfigs">
        <div class="editor-grid editor-grid--three">
          <label class="field"><span>记录编号</span><input v-model.trim="selected.gid" type="text" /></label>
          <label class="field"><span>商店编号</span><input v-model.trim="selected.shopId" type="text" /></label>
          <label class="field"><span>商品</span>
            <select v-model="selected.itemId">
              <option value="">请选择商品</option>
              <option v-for="it in items" :key="it.itemId" :value="it.itemId">{{ it.name }} ({{ it.itemId }})</option>
            </select>
          </label>
          <label class="field"><span>商品类型</span>
            <select v-model.number="selected.itemType">
              <option :value="0">道具</option>
              <option :value="1">装备</option>
            </select>
          </label>
          <label class="field"><span>贡献价格</span><input v-model.number="selected.contributionCost" type="number" min="0" /></label>
          <label class="field"><span>库存</span><input v-model.number="selected.stock" type="number" /></label>
          <label class="field"><span>每日限购</span><input v-model.number="selected.dailyLimit" type="number" /></label>
          <label class="field"><span>排序值</span><input v-model.number="selected.sortOrder" type="number" /></label>
        </div>
      </fieldset>

      <template #footer>
        <button class="secondary-button" type="button" @click="editorOpen = false">取消</button>
        <button class="danger-button" type="button" :disabled="!canManageConfigs || !selected?.gid" @click="remove">删除商品</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="save">保存商品</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import { getSectShopItems, getSectShopItem, saveSectShopItem, deleteSectShopItem } from '@/services/sect'
import { getAdminItems } from '@/services/items'
import type { AdminSectShopItemDetail, AdminSectShopItemListItem, AdminItemListItem } from '@/types/admin'
import toast from '@/utils/toast'

const { canManageConfigs } = useAdminPermissions()
const filterShopId = ref('')
const items = ref<AdminItemListItem[]>([])
const list = ref<AdminSectShopItemListItem[]>([])
const selected = ref<AdminSectShopItemDetail | null>(null)
const editorOpen = ref(false)

function createEmpty(): AdminSectShopItemDetail {
  return {
    gid: '', shopId: '', itemId: '', itemType: 0,
    contributionCost: 0, stock: -1, dailyLimit: -1, sortOrder: 0
  }
}

function openCreateModal() {
  if (!canManageConfigs.value) return
  selected.value = createEmpty()
  editorOpen.value = true
}

async function openEditModal(gid: string) {
  selected.value = await getSectShopItem(gid)
  editorOpen.value = true
}

async function loadList() {
  list.value = await getSectShopItems(filterShopId.value)
}

async function save() {
  if (!canManageConfigs.value || !selected.value) return
  if (!selected.value.shopId.trim()) { toast.error('商店编号不能为空。'); return }
  if (!selected.value.itemId.trim()) { toast.error('商品编号不能为空。'); return }

  try {
    selected.value = await saveSectShopItem(selected.value)
    toast.success('宗门商品保存成功。')
    editorOpen.value = false
    await loadList()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '保存失败。')
  }
}

async function remove() {
  if (!canManageConfigs.value || !selected.value?.gid) return
  if (!window.confirm(`确认删除商品 ${selected.value.itemId} 吗？`)) return

  try {
    await deleteSectShopItem(selected.value.gid)
    selected.value = null
    toast.success('宗门商品删除成功。')
    editorOpen.value = false
    await loadList()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '删除失败。')
  }
}

watch(filterShopId, () => { loadList() })

onMounted(async () => {
  await Promise.all([loadList(), (async () => { items.value = await getAdminItems() })()])
})
</script>
