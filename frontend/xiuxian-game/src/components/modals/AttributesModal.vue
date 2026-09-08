<template>
  <XiuXianModal :model-value="modelValue" title="人物属性与加点" :width="780" @update:model-value="$emit('update:modelValue', $event)">
    <div class="attributes-modal-content">
      
      <!-- 左侧：修仙根基与加点 -->
      <div class="modal-left-column">
        <!-- 修士根基 -->
        <div class="modal-card-custom root-card">
          <div class="card-title-custom">
            <AssetIcon :source="ICON.misc_yinyang" size="18" />
            <span>修士根基</span>
          </div>
          
          <div class="cultivator-profile">
            <div class="profile-avatar">
              <span>修</span>
            </div>
            <div class="profile-info">
              <div class="profile-name">{{ player.name || '无名修士' }}</div>
              <div class="profile-row">
                <span class="profile-badge realm">{{ player.realmName || '练气期一层' }}</span>
                <span class="profile-badge profession">{{ player.professionName || '修士' }}</span>
              </div>
            </div>
          </div>

          <div class="spirit-root-info">
            <div class="root-meta">
              <span class="root-icon-emoji">{{ playerSpiritRoot.icon }}</span>
              <span class="root-name">{{ playerSpiritRoot.name }}灵根</span>
            </div>
            <p class="root-desc">{{ playerSpiritRoot.description }}</p>
          </div>

          <div class="counter-section">
            <div class="counter-title">⚡ 五行相克法则</div>
            <div class="counter-grid">
              <div
                v-for="counter in playerSpiritRoot.counters"
                :key="counter.type"
                class="counter-badge-item"
                :class="{ advantage: counter.value > 0, disadvantage: counter.value < 0 }"
              >
                <span class="counter-badge-icon">{{ counter.icon }}</span>
                <span class="counter-badge-name">{{ counter.name }}</span>
                <span class="counter-badge-val">{{ counter.value > 0 ? '+' : '' }}{{ counter.value }}%</span>
              </div>
            </div>
          </div>
        </div>

        <!-- 战斗属性 -->
        <div class="modal-card-custom combat-stats-card">
          <div class="card-title-custom">
            <AssetIcon :source="ICON.misc_shield" size="18" />
            <span>道法属性</span>
          </div>

          <div class="stats-category">
            <div class="stats-grid-2x2">
              <div class="stat-detail-box">
                <span class="stat-label">血量</span>
                <span class="stat-value-text">{{ player.hp }}</span>
              </div>
              <div class="stat-detail-box">
                <span class="stat-label">蓝量</span>
                <span class="stat-value-text">{{ player.mp }}</span>
              </div>
              <div class="stat-detail-box">
                <span class="stat-label">物理攻击</span>
                <span class="stat-value-text">{{ combatStats.phyAtk }}</span>
              </div>
              <div class="stat-detail-box">
                <span class="stat-label">法术攻击</span>
                <span class="stat-value-text">{{ combatStats.magAtk }}</span>
              </div>
              <div class="stat-detail-box">
                <span class="stat-label">物理防御</span>
                <span class="stat-value-text">{{ combatStats.phyDef }}</span>
              </div>
              <div class="stat-detail-box">
                <span class="stat-label">法术防御</span>
                <span class="stat-value-text">{{ combatStats.magDef }}</span>
              </div>
            </div>
            <div class="stat-detail-box speed-box">
              <span class="stat-label">身法速度</span>
              <span class="stat-value-text highlight">{{ combatStats.speed }}</span>
            </div>
          </div>

          <div class="stats-category">
            <div class="stats-grid-2x2">
              <div class="stat-detail-box mini">
                <span class="stat-label">命中率</span>
                <span class="stat-value-text percent">{{ combatStats.hitRate }}%</span>
              </div>
              <div class="stat-detail-box mini">
                <span class="stat-label">闪避率</span>
                <span class="stat-value-text percent">{{ combatStats.dodgeRate }}%</span>
              </div>
              <div class="stat-detail-box mini">
                <span class="stat-label">暴击率</span>
                <span class="stat-value-text percent">{{ combatStats.critRate }}%</span>
              </div>
              <div class="stat-detail-box mini">
                <span class="stat-label">暴击伤害</span>
                <span class="stat-value-text percent">{{ combatStats.critDmg }}%</span>
              </div>
              <div class="stat-detail-box mini">
                <span class="stat-label">连击率</span>
                <span class="stat-value-text percent">{{ combatStats.comboRate }}%</span>
              </div>
              <div class="stat-detail-box mini">
                <span class="stat-label">反击率</span>
                <span class="stat-value-text percent">{{ combatStats.counterRate }}%</span>
              </div>
              <div class="stat-detail-box mini">
                <span class="stat-label">物理破甲</span>
                <span class="stat-value-text percent">{{ combatStats.armorBreak }}%</span>
              </div>
              <div class="stat-detail-box mini">
                <span class="stat-label">附带伤害</span>
                <span class="stat-value-text percent">{{ combatStats.bonusDmg }}%</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 右侧：资产与加点 -->
      <div class="modal-right-column">
        <!-- 修仙资产 -->
        <div class="modal-card-custom treasury-card">
          <div class="card-title-custom">
            <AssetIcon :source="ICON.currency_spirit" size="18" />
            <span>乾坤宝库</span>
          </div>
          <div class="treasury-grid">
            <div class="treasury-item spirit-stones">
              <div class="item-header">
                <span class="item-icon-circle"><AssetIcon :source="ICON.currency_spirit" size="16" /></span>
                <span class="item-name">灵石</span>
              </div>
              <span class="item-val">{{ formatCompact(player.spiritStone) }}</span>
            </div>
            <div class="treasury-item gold">
              <div class="item-header">
                <span class="item-icon-circle"><AssetIcon :source="ICON.currency_gold" size="16" /></span>
                <span class="item-name">金币</span>
              </div>
              <span class="item-val">{{ formatCompact(player.gold) }}</span>
            </div>
            <div class="treasury-item exp">
              <div class="item-header">
                <span class="item-icon-circle"><AssetIcon :source="ICON.currency_exp" size="16" /></span>
                <span class="item-name">修为经验</span>
              </div>
              <span class="item-val">{{ formatCompact(player.realmExp) }}</span>
            </div>
            <div class="treasury-item honor">
              <div class="item-header">
                <span class="item-icon-circle"><AssetIcon :source="ICON.currency_honor" size="16" /></span>
                <span class="item-name">荣誉值</span>
              </div>
              <span class="item-val">{{ formatCompact(player.honor) }}</span>
            </div>
          </div>
        </div>

        <!-- 属性加点 -->
        <div class="modal-card-custom point-allocation">
          <div class="card-title-custom">
            <AssetIcon :source="ICON.stat_attack" size="18" />
            <span>潜能加点</span>
          </div>
          
          <div class="points-pool-dashboard">
            <div class="pool-left">
              <span class="pool-icon">✨</span>
              <span class="pool-label">可用潜能属性点</span>
            </div>
            <span class="pool-val" :class="{ 'has-points': availablePoints > 0 }">{{ availablePoints }}</span>
          </div>

          <div class="attr-alloc-list">
            <div v-for="attr in attributes" :key="attr.type" class="attr-alloc-card">
              <div class="attr-alloc-meta">
                <span class="attr-alloc-name">{{ attr.name }}</span>
                <span class="attr-alloc-bonus">{{ formatBonus(attr.bonusPerPoint, attr.pointsPerBonus) }}</span>
              </div>
              <div class="attr-alloc-control">
                <button
                  class="attr-action-btn minus"
                  :disabled="attr.invested <= 0 || isAdjustingAttributePoint"
                  @click="$emit('decrease-attribute', attr.type)"
                >
                  -
                </button>
                <span class="attr-action-val">{{ attr.invested }}<span class="unit">点</span></span>
                <button
                  class="attr-action-btn plus"
                  :disabled="availablePoints <= 0 || isAdjustingAttributePoint"
                  @click="$emit('increase-attribute', attr.type)"
                >
                  +
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </XiuXianModal>
</template>

<script>
import XiuXianModal from '../common/XiuXianModal.vue'
import AssetIcon from '../common/AssetIcon.vue'
import { ICON } from '../../icons'
import { formatCompactNumber } from '../../services/gameDisplay'

export default {
  name: 'AttributesModal',

  components: {
    XiuXianModal,
    AssetIcon
  },

  props: {
    modelValue: Boolean,
    availablePoints: { type: Number, default: 0 },
    attributes: { type: Array, required: true },
    combatStats: { type: Object, required: true },
    playerSpiritRoot: { type: Object, required: true },
    player: { type: Object, required: true },
    isAdjustingAttributePoint: { type: Boolean, default: false }
  },

  emits: ['update:modelValue', 'increase-attribute', 'decrease-attribute'],

  setup() {
    const formatBonus = (value, pointsPerBonus = 1) => {
      if (value === null || value === undefined) return '暂无配置'
      const bonusText = value > 0 ? `+${value}` : `${value}`
      return pointsPerBonus > 1 ? `每${pointsPerBonus}点 ${bonusText}` : `每点 ${bonusText}`
    }

    return {
      ICON,
      formatBonus,
      formatCompact: formatCompactNumber
    }
  }
}
</script>

<style scoped>
/* 强制消除父组件弹窗体的滚动条 */
:deep(.modal-body) {
  overflow-y: hidden !important;
  padding: 12px !important;
}

.attributes-modal-content {
  display: flex;
  gap: 10px;
  padding: 4px;
}

.modal-left-column,
.modal-right-column {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 10px;
}

/* 自定义卡片样式 */
.modal-card-custom {
  background: var(--xiuxian-bg-panel);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-lg);
  padding: 12px;
  box-shadow: var(--shadow-sm);
}

.card-title-custom {
  font-size: var(--font-size-sm);
  font-weight: 700;
  color: var(--text-primary);
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  border-bottom: 1px solid var(--border-color);
  padding-bottom: 6px;
  margin-bottom: 8px;
}

/* 修士信息卡片 */
.cultivator-profile {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px 10px;
  background: var(--bg-overlay-light);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  margin-bottom: 8px;
}

.profile-avatar {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: var(--accent-soft-bg);
  border: 1px solid var(--primary-color);
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  color: var(--primary-color);
  font-size: 13px;
}

.profile-info {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.profile-name {
  font-size: var(--font-size-base);
  font-weight: 700;
  color: var(--text-primary);
  line-height: 1.1;
}

.profile-row {
  display: flex;
  gap: 4px;
}

.profile-badge {
  font-size: 9px;
  padding: 1px 4px;
  border-radius: 3px;
  font-weight: 600;
  line-height: 1.2;
}

.profile-badge.realm {
  background: var(--status-warning-bg);
  color: var(--status-warning-text);
  border: 1px solid rgba(234, 179, 8, 0.2);
}

.profile-badge.profession {
  background: var(--status-info-bg);
  color: var(--status-info-text);
  border: 1px solid rgba(37, 99, 235, 0.15);
}

/* 灵根与克制样式 */
.spirit-root-info {
  padding: 8px 10px;
  border-left: 3px solid var(--primary-color);
  background: var(--bg-overlay-light);
  border-radius: 0 var(--radius-md) var(--radius-md) 0;
  margin-bottom: 8px;
}

.root-meta {
  display: flex;
  align-items: center;
  gap: 6px;
  font-weight: 700;
  font-size: 12px;
  color: var(--text-primary);
}

.root-icon-emoji {
  font-size: 14px;
}

.root-desc {
  font-size: 11px;
  color: var(--text-secondary);
  line-height: 1.4;
  margin-top: 2px;
}

.counter-section {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.counter-title {
  font-size: 10px;
  font-weight: 600;
  color: var(--text-muted);
  text-transform: uppercase;
}

.counter-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 6px;
}

.counter-badge-item {
  display: flex;
  align-items: center;
  gap: var(--spacing-xs);
  padding: 4px 8px;
  border-radius: var(--radius-md);
  font-size: 11px;
  background: var(--bg-overlay-light);
  border: 1px solid var(--border-color);
}

.counter-badge-item.advantage {
  color: var(--status-success-text);
  background: var(--status-success-bg);
  border-color: rgba(34, 197, 94, 0.2);
}

.counter-badge-item.disadvantage {
  color: var(--hp-color);
  background: var(--status-danger-bg);
  border-color: rgba(244, 63, 94, 0.2);
}

.counter-badge-icon {
  font-size: 12px;
}

.counter-badge-name {
  flex: 1;
}

.counter-badge-val {
  font-family: var(--font-mono);
  font-weight: 600;
}

/* 属性加点样式 */
.points-pool-dashboard {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 12px;
  background: var(--bg-overlay-medium);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  margin-bottom: 8px;
}

.pool-left {
  display: flex;
  align-items: center;
  gap: 6px;
}

.pool-icon {
  font-size: 13px;
}

.pool-label {
  font-size: 11px;
  font-weight: 600;
  color: var(--text-secondary);
}

.pool-val {
  font-size: 15px;
  font-weight: 700;
  color: var(--text-muted);
  font-family: var(--font-mono);
}

.pool-val.has-points {
  color: var(--primary-color);
  animation: pulse 2s infinite;
}

@keyframes pulse {
  0% { transform: scale(1); }
  50% { transform: scale(1.05); }
  100% { transform: scale(1); }
}

.attr-alloc-list {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.attr-alloc-card {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 6px 10px;
  background: var(--bg-overlay-light);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  transition: all 0.15s ease;
}

.attr-alloc-card:hover {
  border-color: var(--text-muted);
  background: var(--bg-overlay-medium);
}

.attr-alloc-meta {
  display: flex;
  flex-direction: column;
  gap: 1px;
}

.attr-alloc-name {
  font-size: 12px;
  font-weight: 700;
  color: var(--text-primary);
}

.attr-alloc-bonus {
  font-size: 10px;
  color: var(--text-muted);
}

.attr-alloc-control {
  display: flex;
  align-items: center;
  gap: 6px;
}

.attr-action-btn {
  width: 24px;
  height: 24px;
  border-radius: 4px;
  border: 1px solid var(--control-border);
  background: var(--control-bg);
  color: var(--control-text);
  font-weight: 700;
  font-size: 12px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.15s ease;
}

.attr-action-btn:hover:not(:disabled) {
  background: var(--control-bg-hover);
  border-color: var(--text-secondary);
}

.attr-action-btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.attr-action-val {
  font-family: var(--font-mono);
  font-weight: 700;
  font-size: 12px;
  color: var(--text-primary);
  min-width: 44px;
  text-align: center;
}

.attr-action-val .unit {
  font-size: 9px;
  font-weight: 500;
  color: var(--text-muted);
  margin-left: 2px;
}

/* 战斗属性面板 */
.stats-category {
  margin-bottom: 10px;
}

.stats-category:last-child {
  margin-bottom: 0;
}

.category-header {
  font-size: 10px;
  font-weight: 700;
  color: var(--text-muted);
  margin-bottom: 6px;
  text-transform: uppercase;
}

.stats-grid-2x2 {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 6px;
}

.stat-detail-box {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 6px 10px;
  background: var(--bg-overlay-light);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  font-size: 12px;
  transition: all 0.15s ease;
}

.stat-detail-box:hover {
  background: var(--bg-overlay-medium);
  border-color: var(--text-muted);
}

.stat-detail-box.mini {
  padding: 5px 8px;
  font-size: 11px;
}

.stat-label {
  color: var(--text-secondary);
}

.stat-value-text {
  font-family: var(--font-mono);
  font-weight: 600;
  color: var(--text-primary);
}

.stat-value-text.highlight {
  color: var(--primary-color);
  font-weight: 700;
}

.speed-box {
  margin-top: 6px;
  width: 100%;
}

/* 资源资产卡片样式 */
.treasury-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 8px;
}

.treasury-item {
  display: flex;
  flex-direction: column;
  gap: 4px;
  padding: 8px 10px;
  background: var(--bg-overlay-light);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  transition: all 0.15s ease;
}

.treasury-item:hover {
  border-color: var(--text-muted);
  background: var(--bg-overlay-medium);
}

.item-header {
  display: flex;
  align-items: center;
  gap: 6px;
}

.item-icon-circle {
  width: 20px;
  height: 20px;
  border-radius: 50%;
  background: var(--bg-overlay-medium);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--primary-color);
}

.item-name {
  font-size: 10px;
  color: var(--text-muted);
  font-weight: 600;
}

.item-val {
  font-family: var(--font-mono);
  font-weight: 700;
  font-size: 13px;
  color: var(--text-primary);
}
</style>
