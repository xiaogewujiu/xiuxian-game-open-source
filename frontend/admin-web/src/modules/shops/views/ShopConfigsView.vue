<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">商店</p>
        <h2 class="section-title section-title--small">商店运营配置</h2>
        <p class="section-note">商店配置和商品配置统一使用表格列表，新增和编辑都通过弹窗完成。</p>
      </div>

      <div class="filter-grid">
        <label class="field">
          <span>关键字</span>
          <input v-model.trim="keyword" type="text" placeholder="搜索商店编号或名称" />
        </label>
        <label class="field">
          <span>当前商店</span>
          <input :value="selectedConfig?.shopId || ''" type="text" placeholder="未选择" disabled />
        </label>
        <label class="field">
          <span>商品数量</span>
          <input :value="selectedConfig ? String(items.length) : ''" type="text" placeholder="0" disabled />
        </label>

        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadConfigs">查询</button>
          <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="openCreateConfigModal">新建商店</button>
        </div>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">商店列表</p>
          <h3 class="section-title section-title--small">商店列表</h3>
        </div>
        <span class="selected-pill">共 {{ configs.length }} 条</span>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>商店编号</th>
              <th>商店名称</th>
              <th>商店类型</th>
              <th>来源</th>
              <th>开放状态</th>
              <th>配置状态</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="config in configs"
              :key="config.shopId"
              :class="{ 'is-active': selectedConfig?.shopId === config.shopId }"
              @click="openEditConfigModal(config.shopId)"
            >
              <td>{{ config.shopId }}</td>
              <td>{{ config.shopName }}</td>
              <td>{{ getShopTypeLabel(config.shopType) }}</td>
              <td>{{ config.isBuiltIn ? '内置' : '人工' }}</td>
              <td>{{ config.isOpen ? '开放' : '关闭' }}</td>
              <td>{{ config.isEnabled ? '启用' : '停用' }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">商品列表</p>
          <h3 class="section-title section-title--small">商店商品</h3>
        </div>
        <div class="detail-actions detail-actions--inline">
          <span class="selected-pill">共 {{ items.length }} 条</span>
          <button class="primary-button" type="button" :disabled="!canManageConfigs || !selectedConfig" @click="openCreateItemModal">新建商品</button>
        </div>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>商店名称</th>
              <th>商品名称</th>
              <th>商品类型</th>
              <th>来源</th>
              <th>当前价格</th>
              <th>状态</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="item in items"
              :key="item.gid"
              :class="{ 'is-active': selectedItem?.gid === item.gid }"
              @click="openEditItemModal(item.gid)"
            >
              <td>{{ item.shopName || item.shopId }}</td>
              <td>{{ item.name || item.itemId }}</td>
              <td>{{ item.itemType === 1 ? '装备' : '道具' }}</td>
              <td>{{ item.isBuiltIn ? '内置' : '人工' }}</td>
              <td>{{ item.currentPrice }}</td>
              <td>{{ item.isEnabled ? '启用' : '停用' }}</td>
            </tr>
          </tbody>
        </table>
      </div>

    </section>

    <AdminModal
      v-model="configEditorOpen"
      kicker="商店配置"
      :title="selectedConfig?.shopId ? `编辑商店 ${selectedConfig.shopId}` : '新建商店'"
      description="维护商店开放条件、折扣、刷新周期和基础展示信息。"
      size="xwide"
    >
      <fieldset v-if="selectedConfig" class="form-fieldset" :disabled="!canManageConfigs">
        <div class="detail-overview">
          <article class="detail-overview__item"><span>来源</span><strong>{{ selectedConfig.isBuiltIn ? '内置' : '人工维护' }}</strong></article>
          <article class="detail-overview__item"><span>种子键</span><strong>{{ selectedConfig.seedKey || '-' }}</strong></article>
          <article class="detail-overview__item"><span>内置版本</span><strong>{{ selectedConfig.builtInVersion || '-' }}</strong></article>
          <article class="detail-overview__item"><span>最后更新</span><strong>{{ formatDateTime(selectedConfig.lastUpdateTime) }}</strong></article>
        </div>
        <div class="editor-grid editor-grid--three">
          <label class="field"><span>商店编号</span><input v-model.trim="selectedConfig.shopId" type="text" /></label>
          <label class="field"><span>商店名称</span><input v-model.trim="selectedConfig.shopName" type="text" /></label>
          <label class="field"><span>商店类型</span><select v-model.number="selectedConfig.shopType"><option v-for="option in SHOP_TYPE_OPTIONS" :key="option.value" :value="option.value">{{ option.label }}</option></select></label>
          <label class="field"><span>需要等级</span><input v-model.number="selectedConfig.requiredLevel" type="number" min="1" /></label>
          <label class="field"><span>需要VIP</span><input v-model.number="selectedConfig.requiredVipLevel" type="number" min="0" /></label>
          <label class="field"><span>折扣</span><input v-model.number="selectedConfig.discount" type="number" min="0.01" max="1" step="0.01" /></label>
          <label class="field"><span>刷新间隔</span><input v-model.number="selectedConfig.refreshIntervalHours" type="number" min="0" /></label>
          <label class="field"><span>图标</span><input v-model.trim="selectedConfig.icon" type="text" /></label>
          <label class="field"><span>排序值</span><input v-model.number="selectedConfig.sortOrder" type="number" /></label>
        </div>
        <div class="editor-grid">
          <label class="field checkbox-field"><input v-model="selectedConfig.isOpen" type="checkbox" /><span>开放商店</span></label>
          <label class="field checkbox-field"><input v-model="selectedConfig.autoRefresh" type="checkbox" /><span>自动刷新</span></label>
          <label class="field checkbox-field"><input v-model="selectedConfig.isEnabled" type="checkbox" /><span>启用配置</span></label>
        </div>
        <label class="field"><span>描述</span><textarea v-model="selectedConfig.description" rows="3"></textarea></label>
      </fieldset>

      <template #footer>
        <button class="secondary-button" type="button" @click="configEditorOpen = false">取消</button>
        <button class="danger-button" type="button" :disabled="!canManageConfigs || !selectedConfig?.shopId" @click="removeConfig">删除商店</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="saveConfig">保存商店</button>
      </template>
    </AdminModal>

    <AdminModal
      v-model="itemEditorOpen"
      kicker="商店商品"
      :title="selectedItem?.gid ? `编辑商品 ${selectedItem.gid}` : '新建商品'"
      description="维护商品类型、价格、库存和限购规则。"
      size="xwide"
    >
      <fieldset v-if="selectedItem" class="form-fieldset" :disabled="!canManageConfigs">
        <div class="detail-overview">
          <article class="detail-overview__item"><span>来源</span><strong>{{ selectedItem.isBuiltIn ? '内置' : '人工维护' }}</strong></article>
          <article class="detail-overview__item"><span>种子键</span><strong>{{ selectedItem.seedKey || '-' }}</strong></article>
          <article class="detail-overview__item"><span>内置版本</span><strong>{{ selectedItem.builtInVersion || '-' }}</strong></article>
          <article class="detail-overview__item"><span>最后更新</span><strong>{{ formatDateTime(selectedItem.lastUpdateTime) }}</strong></article>
        </div>
        <div class="editor-grid editor-grid--three">
          <label class="field"><span>记录编号</span><input v-model.trim="selectedItem.gid" type="text" /></label>
          <label class="field"><span>商店编号</span><select v-model="selectedItem.shopId"><option v-for="config in configs" :key="config.shopId" :value="config.shopId">{{ config.shopName }} ({{ config.shopId }})</option></select></label>
          <label class="field"><span>商品类型</span><select v-model.number="selectedItem.itemType"><option :value="0">道具</option><option :value="1">装备</option></select></label>
          <label class="field"><span>商品编号</span><select v-model="selectedItem.itemId"><option value="">请选择</option><option v-for="option in currentMerchandiseOptions" :key="option.value" :value="option.value">{{ option.label }}</option></select></label>
          <label class="field"><span>基础价格</span><input v-model.number="selectedItem.basePrice" type="number" min="0" /></label>
          <label class="field"><span>当前价格</span><input v-model.number="selectedItem.currentPrice" type="number" min="0" /></label>
          <label class="field"><span>库存</span><input v-model.number="selectedItem.stock" type="number" /></label>
          <label class="field"><span>初始库存</span><input v-model.number="selectedItem.initialStock" type="number" /></label>
          <label class="field"><span>每日限购</span><input v-model.number="selectedItem.dailyLimit" type="number" /></label>
          <label class="field"><span>排序值</span><input v-model.number="selectedItem.sortOrder" type="number" /></label>
        </div>
        <label class="field checkbox-field"><input v-model="selectedItem.isEnabled" type="checkbox" /><span>启用商品</span></label>
      </fieldset>

      <template #footer>
        <button class="secondary-button" type="button" @click="itemEditorOpen = false">取消</button>
        <button class="danger-button" type="button" :disabled="!canManageConfigs || !selectedItem?.gid" @click="removeItem">删除商品</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="saveItem">保存商品</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import { SHOP_TYPE_OPTIONS } from '@/constants/game-options'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import { getAdminEquipments } from '@/services/equipments'
import { getAdminItems } from '@/services/items'
import { deleteAdminShopConfig, deleteAdminShopItem, getAdminShopConfigDetail, getAdminShopConfigs, getAdminShopItemDetail, getAdminShopItems, saveAdminShopConfig, saveAdminShopItem } from '@/services/shops'
import type { AdminEquipmentListItem, AdminItemListItem, AdminShopConfigDetail, AdminShopConfigListItem, AdminShopItemDetail, AdminShopItemListItem } from '@/types/admin'
import toast from '@/utils/toast'

// 页面主状态：
// configs 是商店列表，items 是当前商店下的商品列表，
// itemTemplates / equipmentTemplates 用于商品编辑器的下拉选项。
const { canManageConfigs } = useAdminPermissions()
const keyword = ref('')
const configs = ref<AdminShopConfigListItem[]>([])
const items = ref<AdminShopItemListItem[]>([])
const itemTemplates = ref<AdminItemListItem[]>([])
const equipmentTemplates = ref<AdminEquipmentListItem[]>([])
const selectedConfig = ref<AdminShopConfigDetail | null>(null)
const selectedItem = ref<AdminShopItemDetail | null>(null)
const configEditorOpen = ref(false)
const itemEditorOpen = ref(false)

// 统一格式化后台时间字段。
function formatDateTime(value?: string | null) {
  if (!value) return '-'
  return value.replace('T', ' ').slice(0, 19)
}

// 根据商品类型切换“可选商品编号”的下拉数据源。
const currentMerchandiseOptions = computed(() => {
  if (selectedItem.value?.itemType === 1) {
    return equipmentTemplates.value.map((equipment) => ({
      value: String(equipment.equipmentId),
      label: `${equipment.name} (${equipment.equipmentId})`
    }))
  }

  return itemTemplates.value.map((item) => ({
    value: item.itemId,
    label: `${item.name} (${item.itemId})`
  }))
})

// 商店类型值转中文标签。
function getShopTypeLabel(value: number) {
  return SHOP_TYPE_OPTIONS.find((option) => option.value === value)?.label || `类型 ${value}`
}

// 预加载道具和装备模板，供商品编辑器选择。
async function loadLookupOptions() {
  const [nextItems, nextEquipments] = await Promise.all([
    getAdminItems(),
    getAdminEquipments()
  ])
  itemTemplates.value = nextItems
  equipmentTemplates.value = nextEquipments
}

// 新建商店时的默认对象。
function createEmptyConfig(): AdminShopConfigDetail {
  return {
    shopId: '', shopName: '', shopType: 0, description: '', requiredLevel: 1, requiredVipLevel: 0,
    discount: 1, isOpen: true, autoRefresh: false, refreshIntervalHours: 24, icon: '', sortOrder: 0, isEnabled: true,
    isBuiltIn: false, seedKey: null, builtInVersion: null, lastUpdateTime: null
  }
}

// 新建商品时的默认对象。
function createEmptyItem(shopId = ''): AdminShopItemDetail {
  return {
    gid: '', shopId, itemId: '', itemType: 0, basePrice: 0, currentPrice: 0,
    stock: -1, initialStock: -1, dailyLimit: -1, sortOrder: 0, isEnabled: true,
    isBuiltIn: false, seedKey: null, builtInVersion: null, lastUpdateTime: null
  }
}

// 打开新建商店弹窗。
function openCreateConfigModal() {
  if (!canManageConfigs.value) return
  selectedConfig.value = createEmptyConfig()
  configEditorOpen.value = true
}

// 打开新建商品弹窗。
function openCreateItemModal() {
  if (!canManageConfigs.value || !selectedConfig.value) return
  selectedItem.value = createEmptyItem(selectedConfig.value.shopId || '')
  itemEditorOpen.value = true
}

// 校验商店配置的基础合法性。
function validateConfig() {
  if (!selectedConfig.value) return '当前没有可保存的商店配置。'
  if (!selectedConfig.value.shopId.trim()) return '商店编号不能为空。'
  if (!selectedConfig.value.shopName.trim()) return '商店名称不能为空。'
  if (selectedConfig.value.discount <= 0 || selectedConfig.value.discount > 1) return '商店折扣必须在 0 到 1 之间。'
  if (selectedConfig.value.autoRefresh && selectedConfig.value.refreshIntervalHours <= 0) return '启用自动刷新时，刷新间隔必须大于 0 小时。'
  return ''
}

// 校验商品配置的基础合法性。
function validateItem() {
  if (!selectedItem.value) return '当前没有可保存的商品配置。'
  if (!selectedItem.value.shopId.trim()) return '商店编号不能为空。'
  if (!selectedItem.value.itemId.trim()) return '商品编号不能为空。'
  if (selectedItem.value.itemType < 0 || selectedItem.value.itemType > 1) return '商品类型仅支持 0=道具、1=装备。'
  if (selectedItem.value.basePrice < 0 || selectedItem.value.currentPrice < 0) return '商品价格不能小于 0。'
  if (selectedItem.value.stock < -1 || selectedItem.value.initialStock < -1 || selectedItem.value.dailyLimit < -1) return '库存、初始库存和每日限购只能填写 -1 或大于等于 0 的数值。'
  return ''
}

// 拉取商店列表。
async function loadConfigs() {
  configs.value = await getAdminShopConfigs(keyword.value)
}

// 打开商店详情弹窗，并同步读取商品列表。
async function openEditConfigModal(shopId: string) {
  selectedConfig.value = await getAdminShopConfigDetail(shopId)
  items.value = await getAdminShopItems(shopId)
  selectedItem.value = null
  configEditorOpen.value = true
}

// 保存商店配置。
async function saveConfig() {
  if (!canManageConfigs.value) return
  const validationMessage = validateConfig()
  if (validationMessage) {
    toast.error(validationMessage)
    return
  }

  try {
    selectedConfig.value = await saveAdminShopConfig(selectedConfig.value!)
    toast.success('商店配置保存成功。')
    configEditorOpen.value = false
    await loadConfigs()
    if (selectedConfig.value?.shopId) {
      items.value = await getAdminShopItems(selectedConfig.value.shopId)
    }
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '商店配置保存失败。')
  }
}

async function removeConfig() {
  if (!canManageConfigs.value || !selectedConfig.value?.shopId) return
  if (!window.confirm(`确认删除商店 ${selectedConfig.value.shopId} 吗？`)) return

  try {
    await deleteAdminShopConfig(selectedConfig.value.shopId)
    selectedConfig.value = null
    items.value = []
    toast.success('商店配置删除成功。')
    configEditorOpen.value = false
    await loadConfigs()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '商店配置删除失败。')
  }
}

async function openEditItemModal(gid: string) {
  selectedItem.value = await getAdminShopItemDetail(gid)
  itemEditorOpen.value = true
}

async function saveItem() {
  if (!canManageConfigs.value) return
  const validationMessage = validateItem()
  if (validationMessage) {
    toast.error(validationMessage)
    return
  }

  try {
    selectedItem.value = await saveAdminShopItem(selectedItem.value!)
    toast.success('商店商品保存成功。')
    itemEditorOpen.value = false
    items.value = await getAdminShopItems(selectedItem.value.shopId)
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '商店商品保存失败。')
  }
}

async function removeItem() {
  if (!canManageConfigs.value || !selectedItem.value?.gid) return
  if (!window.confirm(`确认删除商品 ${selectedItem.value.itemId} 吗？`)) return

  try {
    const shopId = selectedItem.value.shopId
    await deleteAdminShopItem(selectedItem.value.gid)
    selectedItem.value = null
    toast.success('商店商品删除成功。')
    itemEditorOpen.value = false
    items.value = await getAdminShopItems(shopId)
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '商店商品删除失败。')
  }
}

onMounted(async () => {
  await Promise.all([loadLookupOptions(), loadConfigs()])
})
</script>
