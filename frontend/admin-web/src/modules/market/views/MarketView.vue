<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">寄售行</p>
        <h2 class="section-title section-title--small">寄售行管理</h2>
        <p class="section-note">管理寄售行配置、浏览商品、查看交易日志。</p>
      </div>

      <div class="tab-bar">
        <button
          v-for="tab in TAB_OPTIONS"
          :key="tab.value"
          class="tab-button"
          :class="{ 'is-active': activeTab === tab.value }"
          type="button"
          @click="activeTab = tab.value"
        >{{ tab.label }}</button>
      </div>
    </section>

    <!-- Tab 1: 配置 -->
    <template v-if="activeTab === 'config'">
      <section class="panel-box filter-panel">
        <div class="section-header-copy">
          <p class="section-kicker">寄售行配置</p>
          <h3 class="section-title section-title--small">基础参数</h3>
        </div>
        <div class="filter-actions">
          <button class="primary-button" type="button" @click="handleSaveConfig">保存配置</button>
        </div>
      </section>

      <section class="panel-box table-panel">
        <div v-if="config" class="editor-grid editor-grid--three">
          <label class="field"><span>手续费(%)</span><input v-model.number="config.feePercent" type="number" min="0" max="100" /></label>
          <label class="field"><span>每人最大上架数</span><input v-model.number="config.maxListingsPerPlayer" type="number" min="1" /></label>
          <label class="field"><span>上架有效期(天)</span><input v-model.number="config.listingDurationDays" type="number" min="1" /></label>
          <label class="field"><span>允许的货币类型</span></label>
          <div class="currency-checkboxes">
            <label class="checkbox-label">
              <input type="checkbox" value="Gold" :checked="config.allowedCurrencies.includes('Gold')" @change="toggleCurrency('Gold')" />
              <span>金币</span>
            </label>
            <label class="checkbox-label">
              <input type="checkbox" value="SpiritStone" :checked="config.allowedCurrencies.includes('SpiritStone')" @change="toggleCurrency('SpiritStone')" />
              <span>灵石</span>
            </label>
          </div>
          <label class="field"><span>最低价格</span><input v-model.number="config.minPrice" type="number" min="0" /></label>
        </div>
      </section>
    </template>

    <!-- Tab 2: 商品浏览 -->
    <template v-if="activeTab === 'listings'">
      <section class="panel-box filter-panel">
        <div class="section-header-copy">
          <p class="section-kicker">商品浏览</p>
          <h3 class="section-title section-title--small">寄售行商品</h3>
        </div>
        <div class="filter-grid">
          <label class="field">
            <span>状态</span>
            <select v-model="listingsFilter.status">
              <option value="">全部</option>
              <option value="Listed">在售</option>
              <option value="Sold">已售</option>
              <option value="Cancelled">已下架</option>
            </select>
          </label>
          <label class="field">
            <span>关键字</span>
            <input v-model.trim="listingsFilter.keyword" type="text" placeholder="搜索卖家或物品" />
          </label>
          <div class="filter-actions">
            <button class="secondary-button" type="button" @click="loadListings">查询</button>
          </div>
        </div>
      </section>

      <section class="panel-box table-panel">
        <div class="section-header-row">
          <div class="section-header-copy">
            <p class="section-kicker">商品列表</p>
            <h3 class="section-title section-title--small">寄售行商品</h3>
          </div>
          <span class="selected-pill">共 {{ listings.length }} 件</span>
        </div>
        <div class="table-wrap">
          <table class="data-table">
            <thead>
              <tr>
                <th>ID</th>
                <th>卖家</th>
                <th>物品</th>
                <th>类型</th>
                <th>数量</th>
                <th>价格</th>
                <th>货币</th>
                <th>状态</th>
                <th>上架时间</th>
                <th>操作</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="l in listings" :key="l.id">
                <td>{{ l.id }}</td>
                <td>{{ l.sellerName }}</td>
                <td>{{ l.itemName }}</td>
                <td>{{ l.itemType === 'Equipment' ? '装备' : '道具' }}</td>
                <td>{{ l.quantity }}</td>
                <td>{{ l.price }}</td>
                <td>{{ l.currencyType === 'Gold' ? '金币' : '灵石' }}</td>
                <td>{{ l.status === 'Listed' ? '在售' : l.status === 'Sold' ? '已售' : '已下架' }}</td>
                <td>{{ l.createdAt }}</td>
                <td>
                  <button v-if="l.status === 'Listed'" class="secondary-button secondary-button--sm" type="button" @click="handleForceCancel(l)">强制下架</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </template>

    <!-- Tab 3: 交易日志 -->
    <template v-if="activeTab === 'transactions'">
      <section class="panel-box filter-panel">
        <div class="section-header-copy">
          <p class="section-kicker">交易日志</p>
          <h3 class="section-title section-title--small">成交记录</h3>
        </div>
        <div class="filter-grid">
          <label class="field">
            <span>关键字</span>
            <input v-model.trim="txFilter.keyword" type="text" placeholder="搜索卖家或买家" />
          </label>
          <div class="filter-actions">
            <button class="secondary-button" type="button" @click="loadTransactions">查询</button>
          </div>
        </div>
      </section>

      <section class="panel-box table-panel">
        <div class="section-header-row">
          <div class="section-header-copy">
            <p class="section-kicker">成交记录</p>
            <h3 class="section-title section-title--small">交易日志</h3>
          </div>
          <span class="selected-pill">共 {{ transactions.length }} 条</span>
        </div>
        <div class="table-wrap">
          <table class="data-table">
            <thead>
              <tr>
                <th>ID</th>
                <th>卖家</th>
                <th>买家</th>
                <th>物品</th>
                <th>数量</th>
                <th>成交价</th>
                <th>货币</th>
                <th>手续费</th>
                <th>成交时间</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="tx in transactions" :key="tx.id">
                <td>{{ tx.id }}</td>
                <td>{{ tx.sellerName }}</td>
                <td>{{ tx.buyerId || '-' }}</td>
                <td>{{ tx.itemName }}</td>
                <td>{{ tx.quantity }}</td>
                <td>{{ tx.price }}</td>
                <td>{{ tx.currencyType === 'Gold' ? '金币' : '灵石' }}</td>
                <td>{{ tx.fee }}</td>
                <td>{{ tx.soldAt || '-' }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </template>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, reactive } from 'vue'
import toast from '@/utils/toast'
import { getMarketConfig, updateMarketConfig, getMarketListings, forceCancelListing, getMarketTransactions } from '@/services/market'
import type { AdminMarketConfig, AdminMarketListing, AdminMarketTransaction } from '@/types/admin'

const TAB_OPTIONS = [
  { value: 'config', label: '寄售行配置' },
  { value: 'listings', label: '商品浏览' },
  { value: 'transactions', label: '交易日志' }
] as const

type TabValue = (typeof TAB_OPTIONS)[number]['value']

const activeTab = ref<TabValue>('config')
const config = ref<AdminMarketConfig | null>(null)
const listings = ref<AdminMarketListing[]>([])
const transactions = ref<AdminMarketTransaction[]>([])

const listingsFilter = reactive({ status: '', keyword: '' })
const txFilter = reactive({ keyword: '' })

async function loadConfig() {
  config.value = await getMarketConfig()
}

function toggleCurrency(currency: string) {
  if (!config.value) return
  const list = config.value.allowedCurrencies.split(',').filter(Boolean)
  const idx = list.indexOf(currency)
  if (idx >= 0) {
    list.splice(idx, 1)
  } else {
    list.push(currency)
  }
  config.value.allowedCurrencies = list.join(',')
}

async function handleSaveConfig() {
  if (!config.value) return
  try {
    await updateMarketConfig(config.value)
    toast.success('配置已保存。')
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '保存失败。')
  }
}

async function loadListings() {
  listings.value = await getMarketListings(listingsFilter.status || undefined, listingsFilter.keyword || undefined)
}

async function handleForceCancel(l: AdminMarketListing) {
  if (!confirm(`确定强制下架 "${l.itemName}" (卖家: ${l.sellerName})？`)) return
  try {
    await forceCancelListing(l.id)
    toast.success('已强制下架。')
    await loadListings()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '操作失败。')
  }
}

async function loadTransactions() {
  transactions.value = await getMarketTransactions(txFilter.keyword || undefined)
}

onMounted(loadConfig)
</script>

<style scoped>
.tab-bar {
  display: flex;
  gap: 0.5rem;
  margin-top: 1rem;
  flex-wrap: wrap;
}

.tab-button {
  padding: 0.5rem 1rem;
  border: 1px solid var(--border-color, #e2e8f0);
  border-radius: 6px;
  background: transparent;
  cursor: pointer;
  font-size: 0.875rem;
  color: var(--text-secondary, #64748b);
  transition: all 0.15s ease;
}

.tab-button.is-active {
  background: var(--primary-color, #3b82f6);
  color: #fff;
  border-color: var(--primary-color, #3b82f6);
}

.tab-button:hover:not(.is-active) {
  background: var(--bg-hover, #f1f5f9);
}

.secondary-button--sm {
  padding: 0.25rem 0.5rem;
  font-size: 0.75rem;
}

.currency-checkboxes {
  display: flex;
  gap: 1rem;
  align-items: center;
  padding: 0.5rem 0;
}

.checkbox-label {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  font-size: 0.85rem;
  cursor: pointer;
}

.checkbox-label input[type="checkbox"] {
  width: auto;
}
</style>
