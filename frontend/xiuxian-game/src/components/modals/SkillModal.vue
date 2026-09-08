<template>
  <XiuXianModal :model-value="modelValue" @update:model-value="$emit('update:modelValue', $event)" title="技能" :width="660">
    <div class="skill-modal">
      <div class="notice-card">
        {{ notice }}
      </div>

      <div class="section-card">
        <div class="section-header">
          <span class="section-icon"><AssetIcon :source="ICON.stat_attack" size="25" /></span>
          <span class="section-title">已装备技能</span>
          <span class="slot-count">{{ equippedSkills.length }}/6</span>
        </div>

        <div class="skills-grid equipped">
          <div
            v-for="skill in equippedSkills"
            :key="skill.id"
            class="skill-slot equipped"
            :class="skill.qualityClass"
          >
            <button
              class="slot-action-btn"
              :disabled="actionLoading"
              @click.stop="unequipSkill(skill)"
            >
              {{ operatingSkillId === skill.id ? '处理中' : '卸下' }}
            </button>
            <XiuXianTooltip
              :title="skill.name"
              :titleClass="skill.qualityClass"
              position="top"
              :offset="12"
              :max-width="340"
            >
              <template #content>
                <div class="skill-detail-tooltip">
                  <div class="tooltip-stats skill-params">
                    <div>类型: {{ skill.damageType }}</div>
                    <div>目标: {{ skill.targetType }}</div>
                    <div>范围: {{ skill.rangeText }}</div>
                    <div>消耗: {{ skill.cost }}</div>
                    <div>冷却: {{ skill.cooldown }}</div>
                    <div v-if="skill.hitCount > 0">段数: {{ skill.hitCount }} 段</div>
                    <div v-if="skill.damageMultiplierText">倍率: {{ skill.damageMultiplierText }}</div>
                    <div v-if="skill.triggerChanceText">触发: {{ skill.triggerChanceText }}</div>
                    <div>状态: {{ skill.ownershipText }}</div>
                  </div>
                  <div class="tooltip-desc">{{ skill.description }}</div>
                  <div v-if="skill.buffDetails.length > 0" class="skill-buff-list">
                    <div v-for="buff in skill.buffDetails" :key="`equipped-${buff.id}`" class="skill-buff-item">
                      <div class="skill-buff-name">{{ buff.name }} · {{ buff.duration }}回合</div>
                      <div class="skill-buff-desc">{{ buff.description }}</div>
                    </div>
                  </div>
                </div>
              </template>

              <div class="skill-slot-trigger">
                <div class="skill-icon"><AssetIcon :source="skill.icon" size="24" /></div>
                <div class="skill-name">{{ skill.name }}</div>
              </div>
            </XiuXianTooltip>
          </div>

          <div v-for="n in Math.max(0, 6 - equippedSkills.length)" :key="'empty-'+n" class="skill-slot empty">
            <span class="empty-icon">+</span>
          </div>
        </div>
      </div>

      <div class="section-card">
        <div class="section-header">
          <span class="section-icon"><AssetIcon :source="ICON.misc_scroll" size="25" /></span>
          <span class="section-title">已掌握技能</span>
          <span class="library-count">{{ ownedSkillLibrary.length }} 项</span>
          <button class="library-btn" type="button" @click="showSkillCodex = true">
            技能图鉴
          </button>
        </div>

        <div v-if="ownedSkillLibrary.length > 0" class="skills-list">
          <XiuXianTooltip
            v-for="skill in ownedSkillLibrary"
            :key="skill.id"
            :title="skill.name"
            :titleClass="skill.qualityClass"
            position="right"
            :offset="12"
            :max-width="360"
          >
            <template #content>
              <div class="skill-detail-tooltip">
                <div class="tooltip-stats skill-params">
                  <div>类型: {{ skill.damageType }}</div>
                  <div>目标: {{ skill.targetType }}</div>
                  <div>范围: {{ skill.rangeText }}</div>
                  <div>消耗: {{ skill.cost }}</div>
                  <div>冷却: {{ skill.cooldown }}</div>
                  <div v-if="skill.hitCount > 0">段数: {{ skill.hitCount }} 段</div>
                  <div v-if="skill.damageMultiplierText">倍率: {{ skill.damageMultiplierText }}</div>
                  <div v-if="skill.triggerChanceText">触发: {{ skill.triggerChanceText }}</div>
                  <div>状态: {{ skill.ownershipText }}</div>
                </div>
                <div class="tooltip-desc">{{ skill.description }}</div>
                <div v-if="skill.buffDetails.length > 0" class="skill-buff-list">
                  <div v-for="buff in skill.buffDetails" :key="buff.id" class="skill-buff-item">
                    <div class="skill-buff-name">{{ buff.name }} · {{ buff.duration }}回合</div>
                    <div class="skill-buff-desc">{{ buff.description }}</div>
                  </div>
                </div>
              </div>
            </template>

            <div
              class="skill-item"
              :class="skill.qualityClass"
            >
              <div class="skill-icon-frame">
                <div class="skill-icon"><AssetIcon :source="skill.icon" size="24" /></div>
              </div>
              <div class="skill-info">
                <div class="library-skill-name">{{ skill.name }}</div>
                <div class="skill-type">{{ skill.damageType }} · {{ skill.targetType }} · {{ skill.rangeText }}</div>
                <div class="skill-type">
                  {{ skill.cost }} · {{ skill.cooldown }}
                  <span v-if="skill.hitCount > 0"> · {{ skill.hitCount }}段</span>
                  <span v-if="skill.damageMultiplierText"> · {{ skill.damageMultiplierText }}</span>
                  <span v-if="skill.triggerChanceText"> · {{ skill.triggerChanceText }}</span>
                </div>
                <div class="skill-description-preview">{{ skill.description }}</div>
                <div v-if="skill.buffSummary" class="skill-buff-summary">附带 {{ skill.buffSummary }}</div>
              </div>
              <div class="skill-side">
              <span class="skill-state-badge" :class="{ equipped: skill.isEquipped, locked: !skill.canOperate && !skill.isEquipped }">
                {{ skill.ownershipText }}
              </span>
              <button
                v-if="skill.hasNextSkill"
                class="upgrade-btn"
                type="button"
                :disabled="actionLoading || !skill.canUpgrade"
                @click.stop="openUpgradeModal(skill)"
              >
                升级
              </button>
              <button
                  class="equip-btn"
                  :disabled="actionLoading || !skill.canOperate || skill.isEquipped"
                  @click="equipSkill(skill)"
                >
                  {{ getActionLabel(skill) }}
                </button>
              </div>
            </div>
          </XiuXianTooltip>
        </div>

        <div v-else class="skill-empty-state">
          当前角色还没有掌握可用技能。
        </div>
      </div>
    </div>
  </XiuXianModal>

  <XiuXianModal
    :model-value="showSkillCodex"
    title="技能图鉴"
    :width="720"
    @update:model-value="showSkillCodex = $event"
  >
    <template #title>
      <AssetIcon :source="ICON.item_book" size="22" />
      <span>技能图鉴</span>
    </template>
    <div class="skill-codex-modal">
      <div class="codex-header">
        <div class="codex-title-group">
          <div class="codex-title">完整技能库</div>
          <div class="codex-subtitle">这里展示当前版本接入的全部技能模板，仅用于查阅。</div>
        </div>
        <span class="library-count">{{ codexSkills.length }} 项</span>
      </div>

      <div class="skills-list codex-list">
        <XiuXianTooltip
          v-for="skill in codexSkills"
          :key="`codex-${skill.id}`"
          :title="skill.name"
          :titleClass="skill.qualityClass"
          position="right"
          :offset="12"
          :max-width="360"
        >
          <template #content>
            <div class="skill-detail-tooltip">
              <div class="tooltip-stats skill-params">
                <div>类型: {{ skill.damageType }}</div>
                <div>目标: {{ skill.targetType }}</div>
                <div>范围: {{ skill.rangeText }}</div>
                <div>消耗: {{ skill.cost }}</div>
                <div>冷却: {{ skill.cooldown }}</div>
                <div v-if="skill.hitCount > 0">段数: {{ skill.hitCount }} 段</div>
                <div v-if="skill.damageMultiplierText">倍率: {{ skill.damageMultiplierText }}</div>
                <div v-if="skill.triggerChanceText">触发: {{ skill.triggerChanceText }}</div>
                <div>状态: {{ skill.ownershipText }}</div>
              </div>
              <div class="tooltip-desc">{{ skill.description }}</div>
              <div v-if="skill.buffDetails.length > 0" class="skill-buff-list">
                <div v-for="buff in skill.buffDetails" :key="`codex-${buff.id}`" class="skill-buff-item">
                  <div class="skill-buff-name">{{ buff.name }} · {{ buff.duration }}回合</div>
                  <div class="skill-buff-desc">{{ buff.description }}</div>
                </div>
              </div>
            </div>
          </template>

          <div class="skill-item codex-item" :class="skill.qualityClass">
            <div class="skill-icon-frame">
              <div class="skill-icon"><AssetIcon :source="skill.icon" size="25" /></div>
            </div>
            <div class="skill-info">
              <div class="library-skill-name">{{ skill.name }}</div>
              <div class="skill-type">{{ skill.damageType }} · {{ skill.targetType }} · {{ skill.rangeText }}</div>
              <div class="skill-type">
                {{ skill.cost }} · {{ skill.cooldown }}
                <span v-if="skill.hitCount > 0"> · {{ skill.hitCount }}段</span>
                <span v-if="skill.damageMultiplierText"> · {{ skill.damageMultiplierText }}</span>
                <span v-if="skill.triggerChanceText"> · {{ skill.triggerChanceText }}</span>
              </div>
              <div class="skill-description-preview">{{ skill.description }}</div>
              <div v-if="skill.buffSummary" class="skill-buff-summary">附带 {{ skill.buffSummary }}</div>
            </div>
            <div class="skill-side codex-side">
              <span class="skill-state-badge" :class="{ equipped: skill.isEquipped, locked: !skill.isOwned }">
                {{ skill.ownershipText }}
              </span>
            </div>
          </div>
        </XiuXianTooltip>
      </div>
    </div>
  </XiuXianModal>

  <XiuXianModal
    :model-value="Boolean(upgradeSkill)"
    title="技能升级确认"
    :width="520"
    @update:model-value="upgradeSkill = null"
  >
    <div v-if="upgradeSkill" class="skill-upgrade-confirm">
      <div class="skill-upgrade-heading">技能升级对比</div>
      <div class="skill-upgrade-desc">确认后将消耗配置的升级条件，并直接替换当前技能。</div>
      <div v-if="upgradeSkill.nextSkill" class="skill-upgrade-tooltip-compare">
        <div class="skill-upgrade-tooltip-card current">
          <div class="skill-upgrade-tooltip-title" :class="upgradeSkill.qualityClass">{{ upgradeSkill.name }}</div>
          <div class="skill-upgrade-tooltip-stats">
            <div class="skill-upgrade-tooltip-stat"><span>类型</span><strong>{{ upgradeSkill.damageType }}</strong></div>
            <div class="skill-upgrade-tooltip-stat"><span>目标</span><strong>{{ upgradeSkill.targetType }}</strong></div>
            <div class="skill-upgrade-tooltip-stat"><span>范围</span><strong>{{ upgradeSkill.rangeText }}</strong></div>
            <div class="skill-upgrade-tooltip-stat"><span>消耗</span><strong>{{ upgradeSkill.cost }}</strong></div>
            <div class="skill-upgrade-tooltip-stat"><span>冷却</span><strong>{{ upgradeSkill.cooldown }}</strong></div>
            <div v-if="upgradeSkill.hitCount > 0" class="skill-upgrade-tooltip-stat"><span>段数</span><strong>{{ upgradeSkill.hitCount }} 段</strong></div>
            <div v-if="upgradeSkill.damageMultiplierText" class="skill-upgrade-tooltip-stat"><span>倍率</span><strong>{{ upgradeSkill.damageMultiplierText }}</strong></div>
            <div v-if="upgradeSkill.triggerChanceText" class="skill-upgrade-tooltip-stat"><span>触发</span><strong>{{ upgradeSkill.triggerChanceText }}</strong></div>
          </div>
          <div class="skill-upgrade-tooltip-description">{{ upgradeSkill.description }}</div>
          <div v-if="upgradeSkill.buffDetails.length > 0" class="skill-upgrade-tooltip-buffs">
            <div v-for="buff in upgradeSkill.buffDetails" :key="`upgrade-current-${buff.id}`" class="skill-upgrade-tooltip-buff">
              <div class="skill-upgrade-tooltip-buff-name">{{ buff.name }} · {{ buff.duration }}回合</div>
              <div class="skill-upgrade-tooltip-buff-desc">{{ buff.description }}</div>
            </div>
          </div>
          <div v-else class="skill-upgrade-tooltip-empty">无附带 Buff</div>
        </div>
        <div class="skill-upgrade-tooltip-arrow">→</div>
        <div class="skill-upgrade-tooltip-card next">
          <div class="skill-upgrade-tooltip-title" :class="upgradeSkill.nextSkill.qualityClass">{{ upgradeSkill.nextSkill.name }}</div>
          <div class="skill-upgrade-tooltip-stats">
            <div class="skill-upgrade-tooltip-stat"><span>类型</span><strong>{{ upgradeSkill.nextSkill.damageType }}</strong></div>
            <div class="skill-upgrade-tooltip-stat"><span>目标</span><strong>{{ upgradeSkill.nextSkill.targetType }}</strong></div>
            <div class="skill-upgrade-tooltip-stat"><span>范围</span><strong>{{ upgradeSkill.nextSkill.rangeText }}</strong></div>
            <div class="skill-upgrade-tooltip-stat"><span>消耗</span><strong>{{ upgradeSkill.nextSkill.cost }}</strong></div>
            <div class="skill-upgrade-tooltip-stat"><span>冷却</span><strong>{{ upgradeSkill.nextSkill.cooldown }}</strong></div>
            <div v-if="upgradeSkill.nextSkill.hitCount > 0" class="skill-upgrade-tooltip-stat"><span>段数</span><strong>{{ upgradeSkill.nextSkill.hitCount }} 段</strong></div>
            <div v-if="upgradeSkill.nextSkill.damageMultiplierText" class="skill-upgrade-tooltip-stat"><span>倍率</span><strong>{{ upgradeSkill.nextSkill.damageMultiplierText }}</strong></div>
            <div v-if="upgradeSkill.nextSkill.triggerChanceText" class="skill-upgrade-tooltip-stat"><span>触发</span><strong>{{ upgradeSkill.nextSkill.triggerChanceText }}</strong></div>
          </div>
          <div class="skill-upgrade-tooltip-description">{{ upgradeSkill.nextSkill.description }}</div>
          <div v-if="upgradeSkill.nextSkill.buffDetails.length > 0" class="skill-upgrade-tooltip-buffs">
            <div v-for="buff in upgradeSkill.nextSkill.buffDetails" :key="`upgrade-next-${buff.id}`" class="skill-upgrade-tooltip-buff">
              <div class="skill-upgrade-tooltip-buff-name">{{ buff.name }} · {{ buff.duration }}回合</div>
              <div class="skill-upgrade-tooltip-buff-desc">{{ buff.description }}</div>
            </div>
          </div>
          <div v-else class="skill-upgrade-tooltip-empty">无附带 Buff</div>
        </div>
      </div>
      <div v-else class="skill-upgrade-empty">未找到下一级技能的完整详情。</div>
      <div v-if="upgradeSkill.upgradeConditions?.length" class="skill-upgrade-conditions">
        <div class="skill-upgrade-conditions-title">升级所需</div>
        <div class="skill-upgrade-material-list">
          <span
            v-for="condition in upgradeSkill.upgradeConditions"
            :key="`${condition.type}-${condition.targetName}`"
            class="skill-upgrade-material"
            :class="{ lacking: !condition.isMet }"
          >
            <span class="skill-upgrade-material-name">{{ condition.targetName }}</span>
            <span class="skill-upgrade-material-amount">
              <strong>{{ condition.currentAmount }}</strong> / {{ condition.amount }}
            </span>
          </span>
        </div>
      </div>
      <div class="skill-upgrade-actions">
        <button type="button" class="btn btn-gold" @click="upgradeSkill = null">取消</button>
        <button type="button" class="btn btn-jade" :disabled="actionLoading" @click="confirmUpgrade">确认升级</button>
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
import { apiClient } from '../../lib/apiClient'
import toast from '@/utils/toast'

export default {
  name: 'SkillModal',

  components: {
    XiuXianModal,
    XiuXianTooltip,
    AssetIcon
  },

  props: {
    modelValue: {
      type: Boolean,
      default: false
    }
  },

  emits: ['update:modelValue'],

  setup(props) {
    // 技能弹窗的主状态：
    // equippedSkills 是已装备技能栏，skillLibrary 是完整技能库，ownedSkillLibrary 是当前角色已掌握子集。
    const notice = ref('技能信息加载中...')
    const equippedSkills = ref([])
    const skillLibrary = ref([])
    const showSkillCodex = ref(false)
    const actionLoading = ref(false)
    const operatingSkillId = ref(null)
    const ownedSkillLibrary = computed(() => skillLibrary.value.filter((skill) => skill.isOwned))
    // 技能图鉴只展示每条技能链的 Lv.1，避免同一技能的各等级重复占用图鉴列表。
    const codexSkills = computed(() => skillLibrary.value.filter((skill) => skill.skillLevel === 1))

    // 根据伤害类型给技能分配一个直观图标。
    function getSkillIcon(skill) {
      if (skill.damageType === '治疗') return ICON.item_heart_green
      if (skill.damageType === '增益/减益') return ICON.skill_buff
      if (skill.damageType === '法术') return ICON.element_fire
      if (skill.damageType === '真实') return ICON.skill_lightning
      if (skill.damageType === '复活') return ICON.skill_revive
      return ICON.skill_dagger
    }

    // 用一套稳定的前端品质 class，给不同类型技能提供颜色层级。
    function getQualityClass(skill) {
      if (skill.damageType === '真实') return 'legendary'
      if (skill.damageType === '法术') return 'rare'
      if (skill.damageType === '增益/减益') return 'epic'
      if (skill.damageType === '治疗') return 'uncommon'
      return 'common'
    }

    function normalizeSkill(skill, includeNextSkill = true) {
      // 中文注释：
      // 这里把后端 DTO 统一折成当前弹窗使用的展示字段，
      // 顺手兼容 PascalCase / camelCase 两套返回命名，避免前后端改 DTO 时弹窗直接失效。
      const buffs = skill.buffs || skill.Buffs || []
      const buffSummary = buffs
        .map((buff) => `${buff.name || buff.Name}(${buff.duration || buff.Duration}回合)`)
        .join('、')
      const damageMultiplier = Number(skill.damageMultiplier ?? skill.DamageMultiplier ?? 0) || 0
      const triggerChance = Number(skill.triggerChance ?? skill.TriggerChance ?? 0) || 0
      const hitCount = Number(skill.hitCount ?? skill.HitCount ?? 0) || 0
      const skillLevel = Number(skill.skillLevel ?? skill.SkillLevel ?? 1)
      const normalizedBuffs = buffs.map((buff) => ({
        id: buff.buffId ?? buff.BuffId ?? buff.name ?? buff.Name ?? '',
        name: buff.name || buff.Name || '未知 Buff',
        description: buff.description || buff.Description || '暂无描述',
        duration: Number(buff.duration || buff.Duration || 0) || 0
      }))
      const normalizedNextSkill = skill.nextSkill || skill.NextSkill || null
      const nextSkillId = skill.nextSkillId ?? skill.NextSkillId ?? null

      return {
        id: skill.skillId ?? skill.SkillId,
        nextSkillId: skill.nextSkillId ?? skill.NextSkillId ?? null,
        skillLevel,
        hasNextSkill: Boolean(skill.hasNextSkill ?? skill.HasNextSkill),
        canUpgrade: Boolean(skill.canUpgrade ?? skill.CanUpgrade),
        nextSkill: includeNextSkill && normalizedNextSkill
          ? normalizeSkill(normalizedNextSkill, false)
          : null,
        name: `${skill.name ?? skill.Name} Lv.${skillLevel}`,
        icon: getSkillIcon(skill),
        qualityClass: getQualityClass(skill),
        damageType: skill.damageType ?? skill.DamageType,
        targetType: skill.targetType ?? skill.TargetType,
        rangeText: skill.rangeText ?? skill.RangeText,
        cost: `${skill.manaCost ?? skill.ManaCost}法力`,
        cooldown: `${skill.cooldown ?? skill.Cooldown}回合`,
        description: skill.description ?? skill.Description,
        hitCount,
        damageMultiplier,
        damageMultiplierText: damageMultiplier > 0 ? `${Math.round(damageMultiplier * 100)}%` : '',
        triggerChance,
        triggerChanceText: triggerChance > 0
          ? `${Math.round((triggerChance <= 1 ? triggerChance * 100 : triggerChance) * 10) / 10}%`
          : '',
        ownershipText: skill.ownershipText ?? skill.OwnershipText,
        isEquipped: Boolean(skill.isEquipped ?? skill.IsEquipped),
        isOwned: Boolean(skill.isOwned ?? skill.IsOwned),
        canOperate: Boolean(skill.canOperate ?? skill.CanOperate),
        upgradeConditions: (skill.upgradeConditions || skill.UpgradeConditions || []).map((condition) => ({
          type: condition.type ?? condition.Type ?? '',
          targetName: condition.targetName ?? condition.TargetName ?? '未知材料',
          amount: Number(condition.amount ?? condition.Amount ?? 0),
          currentAmount: Number(condition.currentAmount ?? condition.CurrentAmount ?? 0),
          isMet: Boolean(condition.isMet ?? condition.IsMet),
          statusText: condition.statusText ?? condition.StatusText ?? ''
        })),
        buffSummary,
        buffDetails: normalizedBuffs
      }
    }

    function applyOverview(overview) {
      // 中文注释：
      // 技能弹窗始终以同一份总览响应刷新：
      // 无论是首次打开、携带还是卸下，前端都直接吃最新总览，
      // 这样可以避免本地自己维护“已携带/未携带”双列表时出现状态漂移。
      notice.value = overview?.notice || overview?.Notice || '当前版本按角色真实技能展示。'
      equippedSkills.value = (overview?.equippedSkills || overview?.EquippedSkills || []).map(normalizeSkill)
      const normalizedLibrary = (overview?.librarySkills || overview?.LibrarySkills || []).map(normalizeSkill)
      const skillMap = new Map(normalizedLibrary.map((skill) => [String(skill.id), skill]))

      // 中文注释：
      // 下一级技能通常既存在于 NextSkill，也存在于技能库列表中。
      // 如果某个后端版本只返回了 NextSkillId 没有展开 NextSkill，
      // 则从同一份技能库中补齐完整详情，避免升级对比弹窗出现空白。
      normalizedLibrary.forEach((skill) => {
        const nextSkill = skill.nextSkillId == null ? null : skillMap.get(String(skill.nextSkillId))
        if (nextSkill) {
          skill.nextSkill = nextSkill
        }
      })
      skillLibrary.value = normalizedLibrary
    }

    async function loadSkillOverview() {
      // 重新拉取当前角色的技能总览。
      try {
        const overview = await apiClient.getSkillOverview()
        applyOverview(overview)
      } catch (error) {
        notice.value = error.message || '加载技能信息失败。'
        equippedSkills.value = []
        skillLibrary.value = []
      }
    }

    const upgradeSkill = ref(null)

    function openUpgradeModal(skill) {
      upgradeSkill.value = skill
    }

    async function confirmUpgrade() {
      if (!upgradeSkill.value) return
      actionLoading.value = true
      operatingSkillId.value = upgradeSkill.value.id
      try {
        const result = await apiClient.upgradeSkill(upgradeSkill.value.id)
        applyOverview(result.overview || result.Overview)
        toast.success('技能升级成功。')
        upgradeSkill.value = null
      } catch (error) {
        toast.error(error.message || '技能升级失败。')
      } finally {
        actionLoading.value = false
        operatingSkillId.value = null
      }
    }

    // 装备技能。
    async function equipSkill(skill) {
      if (!skill?.canOperate || skill?.isEquipped) {
        return
      }

      actionLoading.value = true
      operatingSkillId.value = skill.id

      try {
        const overview = await apiClient.equipSkill(skill.id)
        applyOverview(overview)
      } catch (error) {
        toast.error(error.message || '携带技能失败。')
      } finally {
        actionLoading.value = false
        operatingSkillId.value = null
      }
    }

    // 卸下技能。
    async function unequipSkill(skill) {
      if (!skill?.isEquipped) {
        return
      }

      actionLoading.value = true
      operatingSkillId.value = skill.id

      try {
        const overview = await apiClient.unequipSkill(skill.id)
        applyOverview(overview)
      } catch (error) {
        toast.error(error.message || '卸下技能失败。')
      } finally {
        actionLoading.value = false
        operatingSkillId.value = null
      }
    }

    // 技能按钮文案统一在这里收口。
    function getActionLabel(skill) {
      if (operatingSkillId.value === skill.id) return '处理中'
      if (skill.isEquipped) return '已携带'
      if (skill.canOperate) return '携带'
      return skill.ownershipText
    }

    watch(() => props.modelValue, async (visible) => {
      if (!visible) {
        showSkillCodex.value = false
        return
      }

      // 中文注释：
      // 技能弹窗按打开时懒加载，不在首页首屏预取，
      // 这样未打开技能系统的情况下不会额外占用一次技能总览接口。
      await loadSkillOverview()
    }, { immediate: true })

    return {
      ICON,
      notice,
      equippedSkills,
      skillLibrary,
      codexSkills,
      ownedSkillLibrary,
      showSkillCodex,
      actionLoading,
      operatingSkillId,
      upgradeSkill,
      openUpgradeModal,
      confirmUpgrade,
      equipSkill,
      unequipSkill,
      getActionLabel
    }
  }
}
</script>

<style scoped>
.upgrade-btn {
  padding: 4px 12px;
  background: #b7791f;
  border: none;
  border-radius: var(--radius-sm);
  color: white;
  cursor: pointer;
}

.skill-upgrade-confirm {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}

.skill-upgrade-heading {
  font-size: 18px;
  font-weight: 600;
}

.skill-upgrade-compare {
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto minmax(0, 1fr);
  align-items: stretch;
  gap: var(--spacing-sm);
}

.skill-upgrade-card {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
  min-width: 0;
  padding: var(--spacing-md);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  background: var(--xiuxian-bg-secondary);
}

.skill-upgrade-card.current {
  border-color: var(--border-color);
}

.skill-upgrade-card.next {
  border-color: var(--highlight-text);
  background: linear-gradient(180deg, var(--highlight-soft-bg), var(--xiuxian-bg-secondary));
}

.skill-upgrade-card-title {
  color: var(--text-muted);
  font-size: var(--font-size-sm);
  font-weight: 600;
}

.skill-upgrade-card.next .skill-upgrade-card-title {
  color: var(--highlight-text);
}

.skill-upgrade-card-name {
  color: var(--text-primary);
  font-size: var(--font-size-md);
  font-weight: 700;
}

.skill-upgrade-params {
  display: flex;
  flex-wrap: wrap;
  gap: 4px 10px;
  color: var(--text-secondary);
  font-size: var(--font-size-sm);
  line-height: 1.5;
}

.skill-upgrade-description {
  padding-top: var(--spacing-sm);
  border-top: 1px dashed var(--border-color);
  color: var(--text-secondary);
  font-size: var(--font-size-sm);
  line-height: 1.6;
  overflow-wrap: anywhere;
}

.skill-upgrade-empty-buff,
.skill-upgrade-empty {
  color: var(--text-muted);
  font-size: var(--font-size-sm);
}

.skill-upgrade-arrow {
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--highlight-text);
  font-size: 24px;
  font-weight: 700;
}

.skill-upgrade-tooltip-compare {
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto minmax(0, 1fr);
  align-items: stretch;
  gap: var(--spacing-md);
}

.skill-upgrade-tooltip-card {
  min-width: 0;
  padding: var(--spacing-md);
  background: var(--xiuxian-bg-panel);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-lg);
}

.skill-upgrade-tooltip-card.next {
  border-color: var(--highlight-text);
  box-shadow: 0 0 0 1px var(--highlight-soft-bg), var(--shadow-lg);
}

.skill-upgrade-tooltip-title {
  margin-bottom: var(--spacing-sm);
  padding-bottom: var(--spacing-sm);
  border-bottom: 1px solid var(--border-color);
  font-size: 16px;
  font-weight: 600;
}

.skill-upgrade-tooltip-stats {
  display: grid;
  gap: 2px;
  margin: var(--spacing-sm) 0;
  padding: var(--spacing-sm) 0;
  border-top: 1px solid var(--border-color);
  border-bottom: 1px solid var(--border-color);
}

.skill-upgrade-tooltip-stat {
  display: flex;
  justify-content: space-between;
  gap: var(--spacing-sm);
  font-size: var(--font-size-sm);
}

.skill-upgrade-tooltip-stat span {
  color: var(--text-muted);
}

.skill-upgrade-tooltip-stat strong {
  color: var(--text-primary);
  font-weight: 500;
  text-align: right;
}

.skill-upgrade-tooltip-description {
  margin-top: var(--spacing-sm);
  padding-top: var(--spacing-sm);
  border-top: 1px dashed var(--border-color);
  color: var(--text-muted);
  font-size: var(--font-size-sm);
  line-height: 1.5;
}

.skill-upgrade-tooltip-buffs {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-xs);
  margin-top: var(--spacing-sm);
  padding-top: var(--spacing-sm);
  border-top: 1px dashed var(--border-color);
}

.skill-upgrade-tooltip-buff-name {
  color: var(--highlight-text);
  font-size: var(--font-size-sm);
}

.skill-upgrade-tooltip-buff-desc,
.skill-upgrade-tooltip-empty,
.skill-upgrade-empty {
  color: var(--text-muted);
  font-size: var(--font-size-sm);
  line-height: 1.5;
}

.skill-upgrade-tooltip-arrow {
  display: flex;
  align-items: center;
  color: var(--highlight-text);
  font-size: 24px;
  font-weight: 700;
}

.skill-upgrade-conditions {
  padding: var(--spacing-sm) var(--spacing-md);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  background: var(--bg-overlay-light);
}

.skill-upgrade-conditions-title {
  display: inline-block;
  margin-right: var(--spacing-md);
  color: var(--text-primary);
  font-size: var(--font-size-sm);
  font-weight: 600;
}

.skill-upgrade-material-list {
  display: inline-flex;
  align-items: center;
  gap: var(--spacing-lg);
  vertical-align: middle;
}

.skill-upgrade-material {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  white-space: nowrap;
}

.skill-upgrade-material-name {
  color: var(--text-primary);
  font-size: var(--font-size-sm);
}

.skill-upgrade-material-amount {
  flex: 0 0 auto;
  color: var(--text-muted);
  font-family: var(--font-mono);
  font-size: var(--font-size-sm);
  white-space: nowrap;
}

.skill-upgrade-material-amount strong {
  color: var(--status-success-text);
  font-weight: 600;
}

.skill-upgrade-material.lacking .skill-upgrade-material-amount strong {
  color: var(--status-danger-text);
}

.skill-upgrade-actions {
  display: flex;
  justify-content: flex-end;
  gap: var(--spacing-sm);
}
.skill-modal {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
  max-height: 60vh;
  overflow-y: auto;
}

.notice-card {
  padding: var(--spacing-md);
  background: var(--accent-soft-bg);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  color: var(--text-secondary);
  line-height: 1.6;
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

.library-count {
  padding: 2px 10px;
  border-radius: 999px;
  border: 1px solid var(--border-color);
  background: var(--bg-overlay-light);
  color: var(--text-secondary);
  font-size: var(--font-size-sm);
  font-family: var(--font-mono);
}

.library-btn {
  padding: 6px 12px;
  border: 1px solid var(--border-color);
  border-radius: var(--radius-sm);
  background: var(--button-neutral-bg);
  color: var(--highlight-text);
  font-size: var(--font-size-sm);
  cursor: pointer;
  transition: all 0.2s ease;
}

.library-btn:hover {
  background: var(--highlight-soft-bg);
  color: var(--highlight-text-strong);
}

.skills-grid {
  display: grid;
  grid-template-columns: repeat(6, 1fr);
  gap: var(--spacing-sm);
}

.skills-grid :deep(.tooltip-wrapper) {
  display: flex;
  flex: 1;
  min-width: 0;
  height: 100%;
}

.skill-slot-trigger {
  flex: 1;
  min-width: 0;
  height: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 4px;
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

.slot-action-btn {
  position: absolute;
  top: 4px;
  right: 4px;
  padding: 2px 6px;
  border: 1px solid var(--border-color);
  border-radius: 999px;
  background: var(--control-bg);
  color: var(--highlight-text);
  font-size: 10px;
  cursor: pointer;
  opacity: 0;
  transition: opacity 0.2s ease, transform 0.2s ease, background 0.2s ease, color 0.2s ease;
  backdrop-filter: blur(6px);
}

.slot-action-btn:disabled {
  cursor: wait;
  opacity: 1;
}

.skill-slot:hover .slot-action-btn {
  opacity: 1;
}

.slot-action-btn:hover:not(:disabled) {
  transform: translateY(-1px);
  background: var(--highlight-soft-bg);
  color: var(--highlight-text-strong);
}

.skill-icon {
  font-size: 24px;
}

.skill-name {
  font-size: 10px;
  color: var(--text-primary);
  text-align: center;
  line-height: 1.2;
  max-width: 100%;
}

.empty-icon {
  font-size: 24px;
  color: var(--text-muted);
  opacity: 0.6;
}

.tooltip-stats {
  display: grid;
  gap: 2px;
  margin-bottom: var(--spacing-xs);
  color: var(--text-secondary);
  font-size: var(--font-size-sm);
}

.tooltip-desc {
  color: var(--text-muted);
  font-size: var(--font-size-sm);
  line-height: 1.5;
}

.skills-list {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
}

.skills-list :deep(.tooltip-wrapper) {
  display: block;
  width: 100%;
}

.skill-item {
  position: relative;
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  padding: 12px 14px;
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  background: var(--xiuxian-bg-secondary);
  transition: border-color 0.2s ease, background 0.2s ease, box-shadow 0.2s ease;
}

.skill-item:hover {
  border-color: var(--accent-text);
  background:
    linear-gradient(180deg, var(--accent-soft-bg), transparent 75%),
    var(--xiuxian-bg-secondary);
  box-shadow: var(--shadow-sm);
}

.skill-empty-state {
  padding: var(--spacing-lg);
  border: 1px dashed var(--border-color);
  border-radius: var(--radius-md);
  background: var(--bg-overlay-light);
  color: var(--text-muted);
  text-align: center;
}

.skill-item.common { border-color: var(--quality-common); }
.skill-item.uncommon { border-color: var(--quality-uncommon); }
.skill-item.rare { border-color: var(--quality-rare); }
.skill-item.epic { border-color: var(--quality-epic); }
.skill-item.legendary { border-color: var(--quality-legendary); }

.skill-icon-frame {
  width: 52px;
  height: 52px;
  flex-shrink: 0;
  display: grid;
  place-items: center;
  border-radius: 14px;
  border: 1px solid var(--control-border);
  background: var(--control-bg);
}

.skill-info {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.library-skill-name {
  font-size: var(--font-size-md);
  font-weight: 700;
  line-height: 1.4;
  color: var(--text-primary);
}

.skill-description-preview {
  color: var(--text-secondary);
  font-size: var(--font-size-sm);
  line-height: 1.55;
  display: -webkit-box;
  -webkit-line-clamp: 1;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.skill-buff-summary {
  color: var(--highlight-text);
  font-size: var(--font-size-xs);
  line-height: 1.45;
  display: -webkit-box;
  -webkit-line-clamp: 1;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.skill-side {
  flex-shrink: 0;
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 10px;
}

.codex-side {
  justify-content: flex-start;
}

.skill-state-badge {
  padding: 2px 8px;
  border-radius: 999px;
  border: 1px solid var(--border-color);
  background: var(--bg-overlay-light);
  color: var(--text-secondary);
  font-size: var(--font-size-xs);
}

.skill-state-badge.equipped {
  border-color: var(--highlight-text);
  background: var(--highlight-soft-bg);
  color: var(--highlight-text);
}

.skill-state-badge.locked {
  border-color: var(--status-danger-text);
  background: var(--status-danger-bg);
  color: var(--status-danger-text);
}

.skill-codex-modal {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
  max-height: 68vh;
  overflow-y: auto;
}

.codex-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: var(--spacing-md);
  padding: var(--spacing-md);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  background: var(--bg-overlay-light);
}

.codex-title-group {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.codex-title {
  font-size: var(--font-size-md);
  font-weight: 700;
  color: var(--text-primary);
}

.codex-subtitle {
  font-size: var(--font-size-sm);
  line-height: 1.6;
  color: var(--text-muted);
}

.codex-item {
  align-items: flex-start;
}

.skill-detail-tooltip {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
}

.skill-params {
  margin-bottom: 0;
}

.skill-buff-list {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-xs);
  padding-top: var(--spacing-sm);
  border-top: 1px dashed var(--border-color);
}

.skill-buff-item {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.skill-buff-name {
  font-size: var(--font-size-sm);
  color: var(--highlight-text);
}

.skill-buff-desc {
  font-size: var(--font-size-sm);
  line-height: 1.5;
  color: var(--text-muted);
}

.equip-btn {
  min-width: 82px;
  padding: 8px 14px;
  border-radius: var(--radius-sm);
  border: 1px solid var(--border-color);
  background: var(--button-neutral-bg);
  color: var(--accent-text);
  cursor: pointer;
  transition: all 0.2s ease;
}

.equip-btn:hover:not(:disabled) {
  background: var(--button-outline-hover-bg);
  border-color: var(--button-outline-hover-border);
  color: var(--button-outline-hover-text);
}

.equip-btn:disabled {
  opacity: 0.55;
  cursor: not-allowed;
}

@media (max-width: 720px) {
  .skill-item {
    align-items: flex-start;
  }

  .skill-side {
    width: 100%;
    flex-direction: row;
    justify-content: space-between;
    align-items: center;
  }

  .codex-header {
    flex-direction: column;
  }

}
</style>
