<template>
  <!-- 人物属性面板 -->
  <div class="stats-panel">
    <!-- 属性总览卡片 -->
    <div class="stats-overview">
      <div class="combat-power">
        <div class="power-label"><AssetIcon :source="ICON.stat_attack" size="25" /> 战斗力</div>
        <div class="power-value">{{ formatNumber(combatPower) }}</div>
      </div>

      <div class="power-details">
        <div v-for="stat in powerStats" :key="stat.name" class="power-stat">
          <span class="stat-name">{{ stat.name }}</span>
          <span class="stat-value">{{ formatNumber(stat.value) }}</span>
        </div>
      </div>
    </div>

    <!-- 基础属性 -->
    <div class="stats-section">
      <div class="section-title">
        <span class="icon"><AssetIcon :source="ICON.misc_scroll" size="25" /></span>
        <span>基础属性</span>
      </div>

      <div class="stats-grid">
        <div v-for="stat in basicStats" :key="stat.name" class="stat-card">
          <div class="stat-icon"><AssetIcon :source="stat.icon" size="25" /></div>
          <div class="stat-info">
            <div class="stat-name">{{ stat.name }}</div>
            <div class="stat-value" :class="{ positive: stat.addValue > 0 }">
              {{ formatNumber(stat.value) }}
              <span v-if="stat.addValue > 0" class="add-value">+{{ formatNumber(stat.addValue) }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 攻击属性 -->
    <div class="stats-section">
      <div class="section-title">
        <span class="icon"><AssetIcon :source="ICON.stat_attack" size="25" /></span>
        <span>攻击属性</span>
      </div>

      <div class="stats-list">
        <div v-for="stat in attackStats" :key="stat.name" class="stat-row">
          <div class="row-left">
            <span class="row-icon"><AssetIcon :source="stat.icon" size="25" /></span>
            <span class="row-name">{{ stat.name }}</span>
          </div>
          <div class="row-value">
            <span class="base">{{ formatNumber(stat.base) }}</span>
            <span v-if="stat.bonus > 0" class="bonus">+{{ formatNumber(stat.bonus) }}</span>
          </div>
        </div>
      </div>
    </div>

    <!-- 防御属性 -->
    <div class="stats-section">
      <div class="section-title">
        <span class="icon"><AssetIcon :source="ICON.stat_defense" size="25" /></span>
        <span>防御属性</span>
      </div>

      <div class="stats-list">
        <div v-for="stat in defenseStats" :key="stat.name" class="stat-row">
          <div class="row-left">
            <span class="row-icon"><AssetIcon :source="stat.icon" size="25" /></span>
            <span class="row-name">{{ stat.name }}</span>
          </div>
          <div class="row-value">
            <span class="base">{{ formatNumber(stat.value) }}</span>
            <span v-if="stat.percent" class="percent">({{ stat.percent }}%)</span>
          </div>
        </div>
      </div>
    </div>

    <!-- 特殊属性 -->
    <div class="stats-section">
      <div class="section-title">
        <span class="icon"><AssetIcon :source="ICON.misc_sparkles" size="25" /></span>
        <span>特殊属性</span>
      </div>

      <div class="special-stats">
        <div v-for="stat in specialStats" :key="stat.name" class="special-card">
          <div class="special-icon"><AssetIcon :source="stat.icon" size="25" /></div>
          <div class="special-name">{{ stat.name }}</div>
          <div class="special-value">{{ stat.value }}%</div>
          <div class="special-bar">
            <div class="bar-fill" :style="{ width: stat.value + '%' }"></div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { reactive, computed } from 'vue'
import { ICON } from '../../icons'
import AssetIcon from '../common/AssetIcon.vue'

/**
 * StatsPanel - 人物属性面板
 *
 * 显示内容：
 * - 基础属性（生命、法力、攻击、防御等）
 * - 攻击属性（物攻、法攻、命中、破甲等）
 * - 防御属性（物防、法防、闪避等）
 * - 特殊属性（暴击、反击、连击等）
 */
export default {
  name: 'StatsPanel',
  components: { AssetIcon },

  setup() {
    // 战斗力
    const combatPower = 125680

    // 战力统计
    const powerStats = [
      { name: '攻击', value: 8540 },
      { name: '防御', value: 6230 },
      { name: '生命', value: 25800 },
      { name: '法力', value: 5200 }
    ]

    // 基础属性
    const basicStats = reactive([
      { name: '生命值', icon: ICON.stat_hp, value: 2580, addValue: 450, maxValue: 2580 },
      { name: '法力值', icon: ICON.stat_mp, value: 520, addValue: 80, maxValue: 520 },
      { name: '物理攻击', icon: ICON.stat_attack, value: 854, addValue: 120 },
      { name: '法术攻击', icon: ICON.stat_magic_attack, value: 620, addValue: 80 },
      { name: '物理防御', icon: ICON.stat_defense, value: 623, addValue: 95 },
      { name: '法术防御', icon: ICON.misc_shield, value: 580, addValue: 75 }
    ])

    // 攻击属性
    const attackStats = reactive([
      { name: '物理攻击', icon: ICON.stat_attack, base: 854, bonus: 120 },
      { name: '法术攻击', icon: ICON.stat_magic_attack, base: 620, bonus: 80 },
      { name: '命中', icon: ICON.ui_target, base: 95, bonus: 5 },
      { name: '破甲', icon: ICON.misc_fire, base: 150, bonus: 30 },
      { name: '暴击率', icon: ICON.stat_critical, base: 15, bonus: 5 },
      { name: '暴击伤害', icon: ICON.creature_demon, base: 150, bonus: 20 }
    ])

    // 防御属性
    const defenseStats = reactive([
      { name: '物理防御', icon: ICON.stat_defense, value: 623, percent: 25 },
      { name: '法术防御', icon: ICON.misc_shield, value: 580, percent: 22 },
      { name: '闪避', icon: ICON.stat_dodge, value: 12, percent: 12 },
      { name: '格挡', icon: '⛨', value: 8, percent: 8 },
      { name: '韧性', icon: ICON.stat_link, value: 20, percent: 20 }
    ])

    // 特殊属性
    const specialStats = reactive([
      { name: '反击率', icon: ICON.stat_combo, value: 15 },
      { name: '连击率', icon: ICON.stat_combo, value: 10 },
      { name: '吸血率', icon: ICON.stat_bleed, value: 5 },
      { name: '闪避率', icon: ICON.stat_dodge, value: 12 },
      { name: '命中率', icon: ICON.ui_target, value: 95 },
      { name: '暴击率', icon: ICON.misc_fire, value: 20 }
    ])

    // 格式化数字
    const formatNumber = (num) => {
      if (num >= 10000) {
        return (num / 10000).toFixed(2) + '万'
      }
      return num.toLocaleString()
    }

    return {
      ICON,
      combatPower,
      powerStats,
      basicStats,
      attackStats,
      defenseStats,
      specialStats,
      formatNumber
    }
  }
}
</script>

<style scoped>
.stats-panel {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
  padding: var(--spacing-md);
}

/* 属性总览 */
.stats-overview {
  background: linear-gradient(135deg, oklch(from var(--primary-color) l c h / 0.08), oklch(from var(--gold-primary) l c h / 0.04));
  border: 1px solid oklch(from var(--primary-color) l c h / 0.15);
  border-radius: var(--radius-lg);
  padding: var(--spacing-lg);
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.combat-power {
  text-align: left;
}

.power-label {
  font-size: 11px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 1px;
  color: var(--text-secondary);
  margin-bottom: var(--spacing-xs);
  display: flex;
  align-items: center;
  gap: 4px;
}

.power-value {
  font-size: 32px;
  font-weight: 800;
  color: var(--primary-color);
  text-shadow: 0 0 16px oklch(from var(--primary-color) l c h / 0.2);
  font-family: var(--font-mono);
}

.power-details {
  display: flex;
  gap: var(--spacing-lg);
}

.power-stat {
  text-align: right;
  min-width: 60px;
}

.power-stat .stat-name {
  display: block;
  font-size: 11px;
  color: var(--text-muted);
  margin-bottom: 2px;
}

.power-stat .stat-value {
  font-size: 14px;
  font-weight: 700;
  color: var(--text-primary);
  font-family: var(--font-mono);
}

/* 属性区域 */
.stats-section {
  background: var(--xiuxian-bg-panel);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-lg);
  padding: var(--spacing-md);
}

.section-title {
  display: flex;
  align-items: center;
  gap: var(--spacing-xs);
  font-size: 13px;
  font-weight: 700;
  color: var(--text-primary);
  margin-bottom: var(--spacing-md);
  padding-bottom: var(--spacing-sm);
  border-bottom: 1px solid var(--border-color);
}

.section-title .icon {
  display: flex;
  align-items: center;
}

/* 基础属性网格 */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: var(--spacing-sm);
}

.stat-card {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  padding: var(--spacing-sm) var(--spacing-md);
  background: var(--bg-overlay-medium);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
}

.stat-card:hover {
  background: var(--bg-overlay-dark);
  border-color: oklch(from var(--primary-color) l c h / 0.3);
  transform: translateY(-1px);
}

.stat-icon {
  display: flex;
  align-items: center;
}

.stat-info {
  flex: 1;
  min-width: 0;
}

.stat-info .stat-name {
  font-size: 11px;
  color: var(--text-secondary);
  display: block;
}

.stat-info .stat-value {
  font-size: 14px;
  font-weight: 700;
  color: var(--text-primary);
  font-family: var(--font-mono);
  display: flex;
  align-items: baseline;
  flex-wrap: wrap;
}

.stat-info .stat-value.positive {
  color: var(--text-primary);
}

.add-value {
  font-size: 11px;
  font-weight: 600;
  color: var(--quality-uncommon);
  margin-left: 2px;
}

/* 属性列表 */
.stats-list {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: var(--spacing-xs);
}

@media (max-width: 600px) {
  .stats-list {
    grid-template-columns: 1fr;
  }
}

.stat-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--spacing-sm) var(--spacing-md);
  background: var(--bg-overlay-light);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  transition: all 0.2s ease;
}

.stat-row:hover {
  background: var(--bg-overlay-medium);
  border-color: oklch(from var(--primary-color) l c h / 0.2);
}

.row-left {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
}

.row-icon {
  display: flex;
  align-items: center;
}

.row-name {
  font-size: 12px;
  color: var(--text-secondary);
}

.row-value {
  font-family: var(--font-mono);
  font-size: 13px;
  display: flex;
  align-items: baseline;
}

.row-value .base {
  font-weight: 700;
  color: var(--text-primary);
}

.row-value .bonus {
  font-size: 11px;
  font-weight: 600;
  color: var(--quality-uncommon);
  margin-left: 4px;
}

.row-value .percent {
  font-size: 11px;
  color: var(--text-muted);
  margin-left: 4px;
}

/* 特殊属性 */
.special-stats {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: var(--spacing-sm);
}

.special-card {
  text-align: center;
  padding: var(--spacing-md) var(--spacing-sm);
  background: var(--bg-overlay-light);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
}

.special-card:hover {
  border-color: oklch(from var(--primary-color) l c h / 0.2);
  background: var(--bg-overlay-medium);
  transform: translateY(-1px);
}

.special-icon {
  display: flex;
  justify-content: center;
  margin-bottom: var(--spacing-xs);
}

.special-name {
  font-size: 11px;
  color: var(--text-secondary);
  margin-bottom: 2px;
}

.special-value {
  font-size: 16px;
  font-weight: 700;
  color: var(--text-primary);
  font-family: var(--font-mono);
  margin-bottom: var(--spacing-sm);
}

.special-bar {
  height: 3px;
  background: var(--bg-overlay-dark);
  border-radius: 99px;
  overflow: hidden;
}

.special-bar .bar-fill {
  height: 100%;
  background: var(--primary-color);
  border-radius: 99px;
  transition: width 0.5s ease;
}
</style>
