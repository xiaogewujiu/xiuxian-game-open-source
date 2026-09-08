<template>
  <!-- 技能与装备展示面板 -->
  <div class="skill-equip-panel">
    <!-- 已装备技能 -->
    <div class="section-card">
      <div class="section-header">
        <span class="section-icon"><AssetIcon :source="ICON.stat_attack" size="25" /></span>
        <span class="section-title">已装备技能</span>
        <span class="slot-count">{{ equippedSkills.length }}/6</span>
      </div>

      <div class="skills-grid equipped">
        <XiuXianTooltip
          v-for="skill in equippedSkills"
          :key="skill.id"
          :title="skill.name"
          :titleClass="skill.quality"
          :stats="[
            { name: '消耗', value: skill.cost },
            { name: '冷却', value: skill.cooldown },
            { name: '伤害', value: skill.damage }
          ]"
          :description="skill.description"
        >
          <div class="skill-slot equipped" :class="skill.quality">
            <div class="skill-icon"><AssetIcon :source="skill.icon" size="25" /></div>
            <div class="skill-name">{{ skill.name }}</div>
          </div>
        </XiuXianTooltip>

        <!-- 空槽位 -->
        <div v-for="n in 6 - equippedSkills.length" :key="'empty-'+n" class="skill-slot empty">
          <span class="empty-icon">+</span>
        </div>
      </div>
    </div>

    <!-- 技能列表 -->
    <div class="section-card">
      <div class="section-header">
        <span class="section-icon"><AssetIcon :source="ICON.misc_scroll" size="25" /></span>
        <span class="section-title">技能库</span>
      </div>

      <div class="skills-list">
        <XiuXianTooltip
          v-for="skill in skillLibrary"
          :key="skill.id"
          :title="skill.name"
          :titleClass="skill.quality"
          :stats="[
            { name: '消耗', value: skill.cost },
            { name: '冷却', value: skill.cooldown },
            { name: '伤害', value: skill.damage }
          ]"
          :description="skill.description"
        >
          <div class="skill-item" :class="skill.quality">
            <div class="skill-icon"><AssetIcon :source="skill.icon" size="28" /></div>
            <div class="skill-info">
              <div class="skill-name">{{ skill.name }}</div>
              <div class="skill-type">{{ skill.type }}</div>
            </div>
            <button class="equip-btn" @click="equipSkill(skill)">装备</button>
          </div>
        </XiuXianTooltip>
      </div>
    </div>

    <!-- 装备展示 -->
    <div class="section-card">
      <div class="section-header">
        <span class="section-icon"><AssetIcon :source="ICON.misc_shield" size="25" /></span>
        <span class="section-title">当前装备</span>
      </div>

      <div class="equipment-display">
        <!-- 装备栏位 -->
        <div class="equip-slots">
          <XiuXianTooltip
            v-for="slot in equipmentSlots"
            :key="slot.position"
            :title="slot.equipment ? slot.equipment.name : slot.name"
            :titleClass="slot.equipment?.quality"
            :stats="slot.equipment ? [
              { name: '等级', value: slot.equipment.level },
              ...slot.equipment.stats.map(s => ({ name: s.name, value: s.value, type: s.value > 0 ? 'positive' : 'negative' }))
            ] : []"
            :description="slot.equipment?.description"
          >
            <div
              class="equip-slot"
              :class="[slot.position, slot.equipment?.quality || 'empty']"
            >
              <div v-if="slot.equipment" class="equip-content">
                <div class="equip-icon"><AssetIcon :source="slot.equipment.icon" size="28" /></div>
                <div class="equip-level">+{{ slot.equipment.enhance }}</div>
              </div>
              <div v-else class="equip-empty">
                <span class="slot-icon"><AssetIcon :source="slot.icon" size="20" /></span>
                <span class="slot-name">{{ slot.name }}</span>
              </div>
            </div>
          </XiuXianTooltip>
        </div>

        <!-- 装备属性汇总 -->
        <div class="equip-summary">
          <div class="summary-title">装备加成</div>
          <div class="summary-stats">
            <div v-for="stat in equipBonusStats" :key="stat.name" class="summary-stat">
              <span class="stat-name">{{ stat.name }}</span>
              <span class="stat-value" :class="{ positive: stat.value > 0 }">+{{ stat.value }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { reactive } from 'vue'
import XiuXianTooltip from '../common/XiuXianTooltip.vue'
import AssetIcon from '../common/AssetIcon.vue'
import { ICON } from '../../icons'
import toast from '@/utils/toast'

/**
 * SkillEquipPanel - 技能与装备展示面板
 *
 * 功能：
 * - 已装备技能列表（6个槽位）
 * - 技能库列表（可装备）
 * - 当前装备展示（鼠标悬停显示详细属性）
 */
export default {
  name: 'SkillEquipPanel',

  components: {
    XiuXianTooltip,
    AssetIcon
  },

  setup() {
    // 已装备技能
    const equippedSkills = reactive([
      { id: 1, name: '火球术', icon: ICON.misc_fire, quality: 'rare', type: '法术', cost: '50法力', cooldown: '3秒', damage: '320', description: '凝聚火元素，发射一颗火球攻击敌人' },
      { id: 2, name: '御剑术', icon: ICON.misc_dagger, quality: 'epic', type: '物理', cost: '30法力', cooldown: '2秒', damage: '280', description: '以气御剑，远程攻击敌人' },
      { id: 3, name: '护盾术', icon: ICON.misc_shield, quality: 'uncommon', type: '防御', cost: '40法力', cooldown: '10秒', damage: '-', description: '召唤灵气护盾，吸收伤害' },
      { id: 4, name: '雷击术', icon: ICON.misc_lightning, quality: 'legendary', type: '法术', cost: '80法力', cooldown: '5秒', damage: '550', description: '召唤天雷轰击敌人' }
    ])

    // 技能库
    const skillLibrary = reactive([
      { id: 5, name: '冰冻术', icon: ICON.element_ice, quality: 'rare', type: '控制', cost: '60法力', cooldown: '4秒', damage: '200', description: '冰冻敌人，使其无法行动' },
      { id: 6, name: '治愈术', icon: ICON.item_heart_green, quality: 'uncommon', type: '治疗', cost: '50法力', cooldown: '8秒', damage: '-', description: '恢复生命值' },
      { id: 7, name: '风刃术', icon: ICON.element_wind, quality: 'common', type: '物理', cost: '25法力', cooldown: '1.5秒', damage: '150', description: '发射风刃切割敌人' },
      { id: 8, name: '石化术', icon: ICON.element_mountain, quality: 'epic', type: '控制', cost: '70法力', cooldown: '6秒', damage: '-', description: '将敌人石化' },
      { id: 9, name: '火焰风暴', icon: ICON.element_lava, quality: 'mythic', type: '法术', cost: '120法力', cooldown: '10秒', damage: '800', description: '召唤火焰风暴，对范围内敌人造成伤害' }
    ])

    // 装备槽位
    const equipmentSlots = reactive([
      { position: 'weapon', name: '武器', icon: ICON.misc_sword_crossed, equipment: { name: '青锋剑', icon: ICON.misc_dagger, quality: 'rare', level: 45, enhance: 8, description: '青云门制式飞剑，锋利无比', stats: [{ name: '攻击', value: 320 }, { name: '暴击', value: 15 }] } },
      { position: 'helmet', name: '头盔', icon: ICON.slot_helmet, equipment: { name: '灵风头巾', icon: ICON.ui_mask, quality: 'uncommon', level: 40, enhance: 5, description: '以灵蚕丝织成，轻便透气', stats: [{ name: '防御', value: 120 }, { name: '法力', value: 50 }] } },
      { position: 'armor', name: '衣服', icon: ICON.slot_armor, equipment: { name: '青云道袍', icon: ICON.slot_armor, quality: 'rare', level: 45, enhance: 6, description: '青云门弟子服装，带有防御法阵', stats: [{ name: '防御', value: 280 }, { name: '生命', value: 200 }] } },
      { position: 'pants', name: '裤子', icon: ICON.slot_pants, equipment: null },
      { position: 'shoes', name: '鞋子', icon: ICON.slot_boots, equipment: { name: '疾风靴', icon: ICON.slot_boots, quality: 'epic', level: 42, enhance: 4, description: '穿上后身轻如燕', stats: [{ name: '速度', value: 25 }, { name: '闪避', value: 10 }] } },
      { position: 'necklace', name: '项链', icon: ICON.slot_necklace, equipment: null },
      { position: 'ring1', name: '戒指', icon: ICON.slot_ring, equipment: { name: '储物戒', icon: ICON.slot_ring, quality: 'legendary', level: 50, enhance: 0, description: '内含一方小天地，可储存物品', stats: [{ name: '背包', value: 100 }] } },
      { position: 'treasure', name: '法宝', icon: ICON.slot_treasure, equipment: null }
    ])

    // 装备加成统计
    const equipBonusStats = reactive([
      { name: '攻击', value: 320 },
      { name: '防御', value: 400 },
      { name: '生命', value: 200 },
      { name: '法力', value: 50 },
      { name: '暴击', value: 15 },
      { name: '闪避', value: 10 },
      { name: '速度', value: 25 }
    ])

    // 装备技能
    const equipSkill = (skill) => {
      if (equippedSkills.length >= 6) {
        toast.warning('技能栏已满！')
        return
      }
      equippedSkills.push({ ...skill })
    }

    return {
      ICON,
      equippedSkills,
      skillLibrary,
      equipmentSlots,
      equipBonusStats,
      equipSkill
    }
  }
}
</script>

<style scoped>
.skill-equip-panel {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
  padding: var(--spacing-md);
}

.section-card {
  background: var(--xiuxian-bg-panel);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  padding: var(--spacing-md);
}

.section-header {
  display: flex;
  align-items: center;
  gap: var(--spacing-xs);
  margin-bottom: var(--spacing-md);
  padding-bottom: var(--spacing-sm);
  border-bottom: 1px solid var(--border-color);
}

.section-icon {
  font-size: 18px;
}

.section-title {
  flex: 1;
  font-size: 14px;
  font-weight: 600;
  color: var(--accent-text);
}

.slot-count {
  font-size: var(--font-size-base);
  color: var(--text-muted);
  font-family: var(--font-mono);
}

/* 技能槽位 */
.skills-grid {
  display: grid;
  grid-template-columns: repeat(6, 1fr);
  gap: var(--spacing-sm);
}

.skill-slot {
  aspect-ratio: 1;
  border-radius: var(--radius-sm);
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 4px;
  cursor: pointer;
  transition: all 0.15s ease;
  position: relative;
}

.skill-slot.equipped {
  border: 2px solid;
  background: rgba(124, 58, 237, 0.1);
}

.skill-slot.empty {
  border: 2px dashed var(--border-color);
  background: var(--xiuxian-bg-secondary);
}

.skill-slot.empty:hover {
  border-color: var(--accent-text);
}

.skill-slot.common { border-color: var(--quality-common); }
.skill-slot.uncommon { border-color: var(--quality-uncommon); }
.skill-slot.rare { border-color: var(--quality-rare); }
.skill-slot.epic { border-color: var(--quality-epic); }
.skill-slot.legendary { border-color: var(--quality-legendary); }
.skill-slot.mythic { border-color: var(--quality-mythic); }

.skill-slot .skill-icon {
  font-size: 24px;
}

.skill-slot .skill-name {
  font-size: var(--font-size-xs);
  color: var(--text-secondary);
  text-align: center;
}

.empty-icon {
  font-size: 24px;
  color: var(--text-muted);
}

/* 技能列表 */
.skills-list {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: var(--spacing-sm);
}

.skill-item {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  padding: var(--spacing-sm);
  background: var(--xiuxian-bg-secondary);
  border-radius: var(--radius-sm);
  border: 1px solid transparent;
  cursor: pointer;
  transition: all 0.15s ease;
}

.skill-item:hover {
  border-color: var(--border-color);
}

.skill-item .skill-icon {
  font-size: 28px;
}

.skill-item .skill-info {
  flex: 1;
}

.skill-item .skill-name {
  font-size: var(--font-size-base);
  font-weight: 500;
  color: var(--text-primary);
}

.skill-item .skill-type {
  font-size: var(--font-size-sm);
  color: var(--text-muted);
}

.skill-item.common { border-color: var(--quality-common); }
.skill-item.uncommon { border-color: var(--quality-uncommon); }
.skill-item.rare { border-color: var(--quality-rare); }
.skill-item.epic { border-color: var(--quality-epic); }
.skill-item.legendary { border-color: var(--quality-legendary); }
.skill-item.mythic { border-color: var(--quality-mythic); }

.equip-btn {
  padding: 4px 12px;
  background: var(--button-primary-start);
  border: none;
  border-radius: var(--radius-sm);
  color: white;
  font-size: var(--font-size-base);
  cursor: pointer;
  transition: all 0.15s ease;
}

.equip-btn:hover {
  background: var(--button-primary-start);
  opacity: 0.85;
}

/* 装备展示 */
.equipment-display {
  display: flex;
  gap: var(--spacing-lg);
}

.equip-slots {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: var(--spacing-sm);
  flex: 1;
}

.equip-slot {
  aspect-ratio: 1;
  border-radius: var(--radius-sm);
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.15s ease;
  position: relative;
}

.equip-slot.empty {
  border: 2px dashed var(--border-color);
  background: var(--xiuxian-bg-secondary);
}

.equip-slot:not(.empty) {
  border: 2px solid;
  background: rgba(124, 58, 237, 0.1);
}

.equip-slot.empty:hover {
  border-color: var(--accent-text);
}

.equip-slot.common { border-color: var(--quality-common); }
.equip-slot.uncommon { border-color: var(--quality-uncommon); }
.equip-slot.rare { border-color: var(--quality-rare); }
.equip-slot.epic { border-color: var(--quality-epic); }
.equip-slot.legendary { border-color: var(--quality-legendary); }
.equip-slot.mythic { border-color: var(--quality-mythic); }

.equip-content {
  position: relative;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.equip-icon {
  font-size: 28px;
}

.equip-level {
  position: absolute;
  bottom: -5px;
  right: -10px;
  background: var(--highlight-text);
  color: var(--text-on-dark);
  font-size: var(--font-size-xs);
  font-weight: 600;
  padding: 1px 4px;
  border-radius: 3px;
}

.equip-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
}

.equip-empty .slot-icon {
  font-size: 20px;
  opacity: 0.5;
}

.equip-empty .slot-name {
  font-size: var(--font-size-xs);
  color: var(--text-muted);
}

/* 装备属性汇总 */
.equip-summary {
  width: 200px;
  background: rgba(124, 58, 237, 0.1);
  border-radius: var(--radius-sm);
  padding: var(--spacing-md);
}

.summary-title {
  font-size: var(--font-size-base);
  font-weight: 600;
  color: var(--accent-text);
  margin-bottom: var(--spacing-md);
  padding-bottom: var(--spacing-sm);
  border-bottom: 1px solid var(--border-color);
}

.summary-stats {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
}

.summary-stat {
  display: flex;
  justify-content: space-between;
  font-size: var(--font-size-base);
}

.summary-stat .stat-name {
  color: var(--text-secondary);
}

.summary-stat .stat-value {
  color: var(--text-primary);
  font-family: var(--font-mono);
  font-weight: 500;
}

.summary-stat .stat-value.positive {
  color: var(--quality-uncommon);
}
</style>
