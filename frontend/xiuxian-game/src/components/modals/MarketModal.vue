<template>
  <XiuXianModal :model-value="modelValue" title="寄售行" :width="560" @update:model-value="$emit('update:modelValue', $event)">
    <div class="market-modal">
      <!-- 视图切换 -->
      <div class="market-tabs">
        <button class="tab-btn" :class="{ active: currentView === 'browse' }" @click="currentView = 'browse'">浏览商品</button>
        <button class="tab-btn" :class="{ active: currentView === 'sell' }" @click="currentView = 'sell'">上架商品</button>
        <button class="tab-btn" :class="{ active: currentView === 'my' }" @click="currentView = 'my'">我的上架</button>
        <button class="tab-btn" :class="{ active: currentView === 'history' }" @click="currentView = 'history'">交易记录</button>
      </div>

      <!-- 筛选栏 -->
      <div v-if="currentView === 'browse'" class="filter-row">
        <select v-model="browseFilter.itemType" @change="loadBrowse">
          <option value="">全部类型</option>
          <option value="Item">道具</option>
          <option value="Equipment">装备</option>
        </select>
        <select v-model="browseFilter.currencyType" @change="loadBrowse">
          <option value="">全部货币</option>
          <option value="Gold">金币</option>
          <option value="SpiritStone">灵石</option>
        </select>
        <select v-model="browseFilter.sortBy" @change="loadBrowse">
          <option value="time">时间</option>
          <option value="price">价格</option>
        </select>
      </div>

      <!-- 上架商品 -->
      <div v-if="currentView === 'sell'" class="market-list sell-panel">
        <div class="sell-form">
          <div class="sell-field">
            <label>物品类型</label>
            <div class="sell-type-btns">
              <button class="type-btn" :class="{ active: sellForm.itemType === 'Item' }" @click="sellForm.itemType = 'Item'; sellForm.selectedId = ''; loadSellItems()">道具</button>
              <button class="type-btn" :class="{ active: sellForm.itemType === 'Equipment' }" @click="sellForm.itemType = 'Equipment'; sellForm.selectedId = ''; loadSellItems()">装备</button>
            </div>
          </div>

          <div class="sell-field">
            <label>选择物品</label>
            <select v-model="sellForm.selectedId" class="sell-select">
              <option value="">-- 请选择 --</option>
              <option v-for="item in sellItems" :key="item.id || item.instanceId" :value="item.id || item.instanceId">
                {{ item.name }}{{ item.quantity > 1 ? ' x' + item.quantity : '' }}
              </option>
            </select>
          </div>

          <div class="sell-field" v-if="sellForm.itemType === 'Item'">
            <label>数量</label>
            <input v-model.number="sellForm.quantity" type="number" min="1" :max="selectedSellItem?.quantity || 1" class="sell-input" />
          </div>

          <div class="sell-field">
            <label>单价</label>
            <input v-model.number="sellForm.price" type="number" min="1" class="sell-input" placeholder="输入售价" />
          </div>

          <div class="sell-field">
            <label>货币类型</label>
            <div class="sell-type-btns">
              <button class="type-btn" :class="{ active: sellForm.currencyType === 'Gold' }" @click="sellForm.currencyType = 'Gold'">金币</button>
              <button class="type-btn" :class="{ active: sellForm.currencyType === 'SpiritStone' }" @click="sellForm.currencyType = 'SpiritStone'">灵石</button>
            </div>
          </div>

          <div class="sell-preview" v-if="sellForm.price > 0">
            <div class="preview-row">
              <span>售价总额</span>
              <span class="preview-val">{{ sellForm.price * (sellForm.itemType === 'Item' ? sellForm.quantity : 1) }} {{ sellForm.currencyType === 'Gold' ? '金币' : '灵石' }}</span>
            </div>
            <div class="preview-row">
              <span>手续费 (5%)</span>
              <span class="preview-fee">-{{ Math.floor(sellForm.price * (sellForm.itemType === 'Item' ? sellForm.quantity : 1) * 0.05) }}</span>
            </div>
            <div class="preview-row preview-total">
              <span>预计收入</span>
              <span class="preview-val">{{ Math.floor(sellForm.price * (sellForm.itemType === 'Item' ? sellForm.quantity : 1) * 0.95) }} {{ sellForm.currencyType === 'Gold' ? '金币' : '灵石' }}</span>
            </div>
          </div>

          <button
            class="sell-confirm-btn"
            :disabled="!canSell || isLoading"
            @click="handleSell"
          >确认上架</button>
        </div>
      </div>

      <!-- 浏览商品 -->
      <div v-if="currentView === 'browse'" class="market-list">
        <div v-if="listings.length === 0" class="empty-state">暂无商品</div>
        <div
          v-for="item in listings"
          :key="item.id"
          class="market-card"
        >
          <div class="market-card-info">
            <span class="market-item-name">{{ item.itemName }}</span>
            <span v-if="item.quantity > 1" class="market-qty">x{{ item.quantity }}</span>
            <span class="market-seller">{{ item.sellerName }}</span>
          </div>
          <div class="market-card-price">
            <span class="market-price">{{ item.price }} {{ item.currencyType === 'Gold' ? '金币' : '灵石' }}</span>
            <button class="action-btn buy-btn" :disabled="isLoading" @click="handleBuy(item)">购买</button>
          </div>
          <div class="market-card-expire">剩余: {{ formatExpire(item.expireAt) }}</div>
        </div>
        <div class="pagination">
          <button class="action-btn" :disabled="browsePage <= 1 || isLoading" @click="browsePage--; loadBrowse()">上一页</button>
          <span>第{{ browsePage }}页</span>
          <button class="action-btn" :disabled="listings.length < 20 || isLoading" @click="browsePage++; loadBrowse()">下一页</button>
        </div>
      </div>

      <!-- 我的上架 -->
      <div v-if="currentView === 'my'" class="market-list">
        <div v-if="myListings.length === 0" class="empty-state">暂无上架商品</div>
        <div
          v-for="item in myListings"
          :key="item.id"
          class="market-card"
          :class="{ cancelled: item.status !== 'Listed' }"
        >
          <div class="market-card-info">
            <span class="market-item-name">{{ item.itemName }}</span>
            <span v-if="item.quantity > 1" class="market-qty">x{{ item.quantity }}</span>
            <span class="market-status">{{ item.status === 'Listed' ? '在售' : item.status === 'Sold' ? '已售' : '已下架' }}</span>
          </div>
          <div class="market-card-price">
            <span class="market-price">{{ item.price }} {{ item.currencyType === 'Gold' ? '金币' : '灵石' }}</span>
            <button
              v-if="item.status === 'Listed'"
              class="action-btn cancel-btn"
              :disabled="isLoading"
              @click="handleCancel(item)"
            >下架</button>
          </div>
        </div>
      </div>

      <!-- 交易记录 -->
      <div v-if="currentView === 'history'" class="market-list">
        <div v-if="history.length === 0" class="empty-state">暂无交易记录</div>
        <div
          v-for="tx in history"
          :key="tx.id"
          class="market-card"
        >
          <div class="market-card-info">
            <span class="market-item-name">{{ tx.itemName }}</span>
            <span v-if="tx.quantity > 1" class="market-qty">x{{ tx.quantity }}</span>
            <span class="market-role">{{ tx.isSeller ? '卖出' : '买入' }}</span>
          </div>
          <div class="market-card-price">
            <span class="market-price">{{ tx.price }} {{ tx.currencyType === 'Gold' ? '金币' : '灵石' }}</span>
            <span v-if="tx.fee > 0" class="market-fee">手续费: {{ tx.fee }}</span>
          </div>
          <div class="market-card-time">{{ tx.soldAt || '' }}</div>
        </div>
      </div>
    </div>
  </XiuXianModal>
</template>

<script>
import { ref, reactive, computed, watch } from 'vue'
import XiuXianModal from '../common/XiuXianModal.vue'
import { apiClient } from '../../lib/apiClient.js'

export default {
  name: 'MarketModal',
  components: { XiuXianModal },
  props: {
    modelValue: { type: Boolean, default: false }
  },
  emits: ['update:modelValue'],
  setup(props) {
    const isLoading = ref(false)
    const currentView = ref('browse')
    const listings = ref([])
    const myListings = ref([])
    const history = ref([])
    const browsePage = ref(1)

    const browseFilter = reactive({
      itemType: '',
      currencyType: '',
      sortBy: 'time',
      sortOrder: 'Desc'
    })

    // 上架表单
    const sellForm = reactive({
      itemType: 'Item',
      selectedId: '',
      quantity: 1,
      price: 0,
      currencyType: 'Gold'
    })
    const sellItems = ref([])

    const selectedSellItem = computed(() => {
      if (!sellForm.selectedId) return null
      return sellItems.value.find(i => (i.id || i.instanceId) === sellForm.selectedId) || null
    })

    const canSell = computed(() => {
      if (!sellForm.selectedId || sellForm.price <= 0) return false
      if (sellForm.itemType === 'Item' && sellForm.quantity <= 0) return false
      return true
    })

    async function loadSellItems() {
      try {
        if (sellForm.itemType === 'Item') {
          const result = await apiClient.getInventory()
          sellItems.value = (result.items || result || []).filter(i => (i.quantity || 0) > 0)
        } else {
          const result = await apiClient.getEquipment()
          sellItems.value = (result.items || result || []).filter(e => !e.isEquipped)
        }
      } catch (e) {
        console.error('加载物品列表失败', e)
        sellItems.value = []
      }
    }

    async function handleSell() {
      if (!canSell.value) return
      isLoading.value = true
      try {
        const itemId = sellForm.itemType === 'Item' ? sellForm.selectedId : null
        const equipId = sellForm.itemType === 'Equipment' ? sellForm.selectedId : null
        await apiClient.listMarketItem(
          sellForm.itemType,
          itemId,
          equipId,
          sellForm.itemType === 'Item' ? sellForm.quantity : 1,
          sellForm.price,
          sellForm.currencyType
        )
        sellForm.selectedId = ''
        sellForm.quantity = 1
        sellForm.price = 0
        sellItems.value = []
        alert('上架成功！')
      } catch (e) {
        console.error('上架失败', e)
        alert('上架失败: ' + (e.message || '未知错误'))
      } finally {
        isLoading.value = false
      }
    }

    async function loadBrowse() {
      isLoading.value = true
      try {
        listings.value = await apiClient.browseMarket(
          browseFilter.itemType || null,
          browseFilter.currencyType || null,
          null,
          browseFilter.sortBy,
          browseFilter.sortOrder,
          browsePage.value,
          20
        )
      } catch (e) {
        console.error('加载商品失败', e)
      } finally {
        isLoading.value = false
      }
    }

    async function loadMyListings() {
      isLoading.value = true
      try {
        myListings.value = await apiClient.getMyMarketListings()
      } catch (e) {
        console.error('加载我的上架失败', e)
      } finally {
        isLoading.value = false
      }
    }

    async function loadHistory() {
      isLoading.value = true
      try {
        history.value = await apiClient.getMarketHistory(1, 50)
      } catch (e) {
        console.error('加载交易记录失败', e)
      } finally {
        isLoading.value = false
      }
    }

    async function handleBuy(item) {
      if (!confirm(`确定购买 ${item.itemName}？价格: ${item.price} ${item.currencyType === 'Gold' ? '金币' : '灵石'}`)) return
      isLoading.value = true
      try {
        await apiClient.buyMarketItem(item.id)
        await loadBrowse()
      } catch (e) {
        console.error('购买失败', e)
      } finally {
        isLoading.value = false
      }
    }

    async function handleCancel(item) {
      if (!confirm(`确定下架 ${item.itemName}？`)) return
      isLoading.value = true
      try {
        await apiClient.cancelMarketListing(item.id)
        await loadMyListings()
      } catch (e) {
        console.error('下架失败', e)
      } finally {
        isLoading.value = false
      }
    }

    function formatExpire(expireAt) {
      if (!expireAt) return ''
      const diff = new Date(expireAt) - new Date()
      if (diff <= 0) return '已过期'
      const days = Math.floor(diff / 86400000)
      const hours = Math.floor((diff % 86400000) / 3600000)
      if (days > 0) return `${days}天${hours}小时`
      return `${hours}小时`
    }

    watch(() => props.modelValue, (val) => {
      if (val) {
        currentView.value = 'browse'
        browsePage.value = 1
        loadBrowse()
      }
    })

    watch(currentView, (view) => {
      if (view === 'browse') loadBrowse()
      else if (view === 'sell') loadSellItems()
      else if (view === 'my') loadMyListings()
      else if (view === 'history') loadHistory()
    })

    return {
      isLoading, currentView, listings, myListings, history, browsePage, browseFilter,
      sellForm, sellItems, selectedSellItem, canSell, loadSellItems, handleSell,
      loadBrowse, handleBuy, handleCancel, formatExpire
    }
  }
}
</script>

<style scoped>
.market-modal {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.market-tabs {
  display: flex;
  gap: 0.5rem;
}

.tab-btn {
  flex: 1;
  padding: 0.5rem;
  border: 1px solid var(--border-color, #e2e8f0);
  border-radius: 6px;
  background: transparent;
  cursor: pointer;
  font-size: 0.8rem;
  color: var(--text-secondary, #64748b);
  transition: all 0.15s;
}

.tab-btn.active {
  background: var(--accent-soft-bg);
  color: var(--text-primary);
  border-color: var(--text-secondary);
}

.filter-row {
  display: flex;
  gap: 0.5rem;
}

.filter-row select {
  flex: 1;
  padding: 0.375rem;
  border: 1px solid var(--border-color, #e2e8f0);
  border-radius: 4px;
  font-size: 0.75rem;
  background: var(--bg-primary, #fff);
}

.market-card {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  padding: 0.5rem 0.75rem;
  border-radius: 6px;
  background: var(--bg-secondary, #f8fafc);
  margin-bottom: 0.375rem;
  gap: 0.25rem;
}

.market-card.cancelled { opacity: 0.5; }

.market-card-info {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.market-item-name {
  font-weight: 600;
  font-size: 0.85rem;
}

.market-qty {
  font-size: 0.7rem;
  color: var(--text-muted, #94a3b8);
}

.market-seller, .market-role, .market-status {
  font-size: 0.7rem;
  color: var(--text-muted, #94a3b8);
}

.market-role { color: var(--primary-color, #3b82f6); }

.market-card-price {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.market-price {
  font-weight: 700;
  font-size: 0.85rem;
  color: #f59e0b;
}

.market-fee {
  font-size: 0.7rem;
  color: var(--text-muted, #94a3b8);
}

.market-card-expire, .market-card-time {
  width: 100%;
  font-size: 0.7rem;
  color: var(--text-muted, #94a3b8);
}

.action-btn {
  padding: 0.25rem 0.75rem;
  border-radius: 4px;
  border: 1px solid var(--border-color, #e2e8f0);
  background: var(--bg-primary, #fff);
  cursor: pointer;
  font-size: 0.75rem;
  transition: all 0.15s;
}

.action-btn:hover:not(:disabled) {
  background: var(--bg-hover, #f1f5f9);
}

.action-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.buy-btn {
  border-color: var(--status-success-text);
  color: var(--status-success-text);
  background: var(--status-success-bg);
}

.buy-btn:hover:not(:disabled) {
  background: var(--status-success-text);
  color: var(--xiuxian-bg-panel);
}

.cancel-btn {
  border-color: var(--status-danger-text);
  color: var(--status-danger-text);
  background: var(--status-danger-bg);
}

.cancel-btn:hover:not(:disabled) {
  background: var(--status-danger-text);
  color: var(--text-on-dark);
}

.pagination {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 1rem;
  margin-top: 0.5rem;
  font-size: 0.8rem;
}

.empty-state {
  text-align: center;
  padding: 1.5rem;
  color: var(--text-muted, #94a3b8);
  font-size: 0.875rem;
}

/* ===== 上架表单 ===== */
.sell-panel {
  max-height: 400px;
  overflow-y: auto;
}

.sell-form {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.sell-field {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.sell-field label {
  font-size: 0.75rem;
  font-weight: 600;
  color: var(--text-secondary, #64748b);
}

.sell-type-btns {
  display: flex;
  gap: 0.375rem;
}

.type-btn {
  flex: 1;
  padding: 0.375rem;
  border: 1px solid var(--border-color, #e2e8f0);
  border-radius: 4px;
  background: transparent;
  cursor: pointer;
  font-size: 0.8rem;
  color: var(--text-secondary, #64748b);
  transition: all 0.15s;
}

.type-btn.active {
  background: var(--accent-soft-bg);
  color: var(--text-primary);
  border-color: var(--text-secondary);
}

.sell-select, .sell-input {
  padding: 0.375rem 0.5rem;
  border: 1px solid var(--border-color, #e2e8f0);
  border-radius: 4px;
  font-size: 0.8rem;
  background: var(--bg-primary, #fff);
  color: var(--text-primary, #1e293b);
}

.sell-preview {
  padding: 0.625rem;
  background: var(--bg-secondary, #f8fafc);
  border-radius: 6px;
  border: 1px solid var(--border-color, #e2e8f0);
}

.preview-row {
  display: flex;
  justify-content: space-between;
  font-size: 0.8rem;
  padding: 0.15rem 0;
  color: var(--text-secondary, #64748b);
}

.preview-val {
  font-weight: 600;
  color: var(--text-primary, #1e293b);
}

.preview-fee {
  color: #ef4444;
  font-weight: 600;
}

.preview-total {
  border-top: 1px solid var(--border-color, #e2e8f0);
  margin-top: 0.25rem;
  padding-top: 0.375rem;
}

.preview-total .preview-val {
  color: #22c55e;
  font-weight: 700;
}

.sell-confirm-btn {
  padding: 0.625rem;
  border: none;
  border-radius: 6px;
  background: var(--button-primary-start);
  color: var(--button-primary-text);
  font-size: 0.9rem;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.2s;
}

.sell-confirm-btn:hover:not(:disabled) {
  transform: translateY(-1px);
  background: var(--button-primary-hover-start);
  color: var(--button-primary-hover-text);
  box-shadow: var(--shadow-sm);
}

.sell-confirm-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
</style>
