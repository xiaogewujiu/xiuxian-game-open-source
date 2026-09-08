<template>
  <!-- 商店系统 Modal -->
  <XiuXianModal :model-value="modelValue" title="修仙商店" :width="700" @update:model-value="$emit('update:modelValue', $event)">
    <div class="shop-modal">
      <!-- 商店分类 -->
      <div class="shop-tabs">
        <button
          v-for="tab in shopTabs"
          :key="tab.key"
          class="tab-btn"
          :class="{ active: currentTab === tab.key }"
          @click="currentTab = tab.key"
        >
          <span class="tab-icon"><AssetIcon :source="tab.icon" size="14" /></span>
          <span class="tab-name">{{ tab.name }}</span>
        </button>
      </div>

      <!-- 商品列表 -->
      <div class="shop-content">
        <div class="items-grid">
          <XiuXianTooltip
            v-for="item in currentItems"
            :key="item.id"
            :title="item.name"
            :titleClass="item.quality"
            position="bottom"
            :offset="8"
            :max-width="360"
            :stats="item.tooltipStats"
            :description="item.description"
            trigger="click"
          >
            <div class="shop-item" :class="item.quality">
              <div class="item-icon">
                <AssetIcon :source="item.icon" :fallback="ICON.misc_gift" :alt="item.name" size="25" />
              </div>
              <div class="item-info">
                <div class="item-name">{{ item.name }}</div>
                <div v-if="currentTab === 'equipment'" class="item-level">Lv.{{ item.requiredLevel }}</div>
                <div class="item-stock">库存: {{ item.stock }}</div>
              </div>
              <div class="item-price">
                <span class="price-icon"><AssetIcon :source="item.currency === 'spirit' ? ICON.currency_spirit : ICON.currency_gold" size="25" /></span>
                <span class="price-value">{{ formatNumber(item.price) }}</span>
              </div>
              <button
                class="buy-btn"
                :disabled="item.stock <= 0"
                @click.stop="buyItem(item)"
              >
                购买
              </button>
            </div>
          </XiuXianTooltip>
        </div>
      </div>

      <!-- 我的货币 -->
      <div class="my-currency">
        <div class="currency-item">
          <span class="currency-icon"><AssetIcon :source="ICON.currency_spirit" size="20" /></span>
          <span class="currency-name">灵石</span>
          <span class="currency-value">{{ formatNumber(mySpirit) }}</span>
        </div>
        <div class="currency-item">
          <span class="currency-icon"><AssetIcon :source="ICON.currency_gold" size="20" /></span>
          <span class="currency-name">金币</span>
          <span class="currency-value">{{ formatNumber(myGold) }}</span>
        </div>
      </div>
    </div>
  </XiuXianModal>
</template>

<script>
import { computed, ref, watch } from 'vue'
import XiuXianModal from '../common/XiuXianModal.vue'
import XiuXianTooltip from '../common/XiuXianTooltip.vue'
import AssetIcon from '../common/AssetIcon.vue'
import { ICON } from '../../icons'
import { useGameStore } from '../../state/gameStore'
import { buildPlayerView, buildShopItemsByTab, formatCompactNumber } from '../../services/gameDisplay'
import toast from '@/utils/toast'

/**
 * ShopModal - 商店系统弹窗
 *
 * 这里保留原有标签页布局，
 * 只是把商品来源改为真实商店接口返回的数据。
 */
export default {
  name: 'ShopModal',

  components: {
    XiuXianModal,
    XiuXianTooltip,
    AssetIcon
  },

  props: {
    modelValue: Boolean
  },

  emits: ['update:modelValue'],

  setup(props) {
    const gameStore = useGameStore()

    // 当前商店页签。
    const currentTab = ref('equipment')

    // 商店分类只是前端视图分组；
    // 实际商品还是来自后端统一商店接口，再在 gameDisplay 里按标签页切开。
    const shopTabs = [
      { key: 'equipment', name: '装备', icon: ICON.slot_weapon },
      { key: 'consumables', name: '消耗品', icon: ICON.item_potion },
      { key: 'materials', name: '材料', icon: ICON.currency_spirit },
      { key: 'skillbooks', name: '功法', icon: ICON.item_scroll },
      { key: 'gems', name: '宝石', icon: ICON.currency_spirit }
    ]

    const player = computed(() => buildPlayerView(gameStore.state.player))
    const groupedItems = computed(() => buildShopItemsByTab(gameStore.state.shops))
    const currentItems = computed(() => groupedItems.value[currentTab.value] || [])

    const mySpirit = computed(() => player.value.spiritStone)
    const myGold = computed(() => player.value.gold)

    // 商店只在真正打开弹窗时才懒加载，避免首页初始化阶段提前打接口。
    watch(() => props.modelValue, async (visible) => {
      if (!visible) {
        return
      }

      try {
        // 中文注释：
        // 商店商品列表只有在用户实际打开商店时才需要展示，
        // 因此这里改为弹窗打开时懒加载，避免首页初始化阶段就请求商店接口。
        await gameStore.loadShops(true)
      } catch (error) {
        toast.error(error.message || '加载商店数据失败。')
      }
    }, { immediate: true })

    // 购买前先做一层前端快速校验，减少明显无效请求。
    const buyItem = async (item) => {
      if (item.stock <= 0) {
        toast.warning('该商品已售罄。')
        return
      }

      if (item.remainingDailyLimit === 0) {
        toast.warning('今日限购已达上限，明天再来。')
        return
      }

      if (player.value.level < item.requiredLevel) {
        toast.warning(`等级不足，需要达到 ${item.requiredLevel} 级。`)
        return
      }

      const isSpiritCurrency = item.currency.includes('spirit')
      const currentMoney = isSpiritCurrency ? mySpirit.value : myGold.value
      if (currentMoney < item.price) {
        toast.warning(isSpiritCurrency ? '灵石不足。' : '金币不足。')
        return
      }

      try {
        const result = await gameStore.buyItem(item.shopId, item.itemId, 1)
        toast.success(result?.message || result?.Message || `购买成功：${item.name}`)
      } catch (error) {
        toast.error(error.message || '购买失败')
      }
    }

    return {
      ICON,
      currentTab,
      shopTabs,
      currentItems,
      mySpirit,
      myGold,
      buyItem,
      formatNumber: formatCompactNumber
    }
  }
}
</script>

<style scoped>
.shop-modal {
  padding: var(--spacing-md);
}

/* 商店分类 */
.shop-tabs {
  display: flex;
  gap: var(--spacing-xs);
  margin-bottom: var(--spacing-lg);
}

.tab-btn {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--spacing-xs);
  padding: var(--spacing-sm) var(--spacing-md);
  background: var(--xiuxian-bg-secondary);
  border: 1px solid transparent;
  border-radius: var(--radius-sm);
  cursor: pointer;
  transition: all 0.15s ease;
  color: var(--text-secondary);
}

.tab-btn:hover {
  border-color: var(--border-color);
  background: var(--button-neutral-hover-bg);
  color: var(--button-neutral-hover-text);
}

.tab-btn.active {
  background: var(--accent-soft-bg);
  border-color: var(--accent-text);
  color: var(--accent-text);
}

.tab-icon {
  font-size: 16px;
}

.tab-name {
  font-size: var(--font-size-base);
}

/* 商品网格 */
.items-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: var(--spacing-md);
  max-height: 350px;
  overflow-y: auto;
  padding-right: var(--spacing-sm);
}

.shop-item {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  padding: var(--spacing-md);
  background: var(--xiuxian-bg-secondary);
  border-radius: var(--radius-md);
  border: 1px solid transparent;
  transition: all 0.15s ease;
}

.shop-item:hover {
  border-color: var(--border-color);
}

.shop-item.common { border-color: var(--quality-common); }
.shop-item.uncommon { border-color: var(--quality-uncommon); }
.shop-item.rare { border-color: var(--quality-rare); }
.shop-item.epic { border-color: var(--quality-epic); }
.shop-item.legendary { border-color: var(--quality-legendary); }

.item-icon {
  width: 42px;
  height: 42px;
  font-size: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
}

.item-info {
  flex: 1;
}

.item-name {
  font-size: var(--font-size-base);
  font-weight: 500;
  color: var(--text-primary);
  margin-bottom: 2px;
}

.item-level {
  font-size: var(--font-size-sm);
  color: var(--highlight-text);
  margin-bottom: 2px;
  font-family: var(--font-mono);
}

.item-stock {
  font-size: var(--font-size-sm);
  color: var(--text-muted);
}

.item-price {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 4px 8px;
  background: rgba(212, 168, 83, 0.1);
  border-radius: var(--radius-sm);
}

.price-icon {
  font-size: 14px;
}

.price-value {
  font-size: var(--font-size-base);
  color: var(--highlight-text);
  font-weight: 600;
  font-family: var(--font-mono);
}

.buy-btn {
  padding: var(--spacing-sm) var(--spacing-md);
  background: var(--button-primary-start);
  border: none;
  border-radius: var(--radius-sm);
  color: var(--button-primary-text);
  font-size: var(--font-size-base);
  cursor: pointer;
  transition: all 0.15s ease;
}

.buy-btn:hover:not(:disabled) {
  background: var(--button-primary-start);
  opacity: 0.85;
  color: var(--button-primary-hover-text);
  box-shadow: var(--shadow-sm);
}

.buy-btn:disabled {
  background: var(--xiuxian-bg-primary);
  color: var(--text-muted);
  cursor: not-allowed;
}

/* 我的货币 */
.my-currency {
  display: flex;
  gap: var(--spacing-lg);
  margin-top: var(--spacing-lg);
  padding-top: var(--spacing-md);
  border-top: 1px solid var(--border-color);
}

.currency-item {
  flex: 1;
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  padding: var(--spacing-md);
  background: var(--xiuxian-bg-secondary);
  border-radius: var(--radius-md);
}

.currency-icon {
  font-size: 20px;
}

.currency-name {
  font-size: var(--font-size-base);
  color: var(--text-secondary);
}

.currency-value {
  flex: 1;
  text-align: right;
  font-size: var(--font-size-base);
  font-weight: 600;
  color: var(--highlight-text);
  font-family: var(--font-mono);
}
</style>

