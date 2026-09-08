<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">锻造规则</p>
        <h2 class="section-title section-title--small">锻造规则</h2>
        <p class="section-note">维护锻造师等级经验曲线和锻造职业通用规则。</p>
      </div>

      <div class="filter-grid">
        <label class="field">
          <span>当前通用规则</span>
          <input :value="ruleConfig?.configId || ''" type="text" placeholder="未加载" disabled />
        </label>
        <label class="field">
          <span>等级规则数</span>
          <input :value="String(levelRules.length)" type="text" placeholder="0" disabled />
        </label>
        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadData">刷新</button>
          <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="openCreateLevelModal">新建等级规则</button>
        </div>
      </div>
    </section>

    <section class="dashboard-section-grid">
      <section class="panel-box table-panel">
        <div class="section-header-row">
          <div class="section-header-copy">
            <p class="section-kicker">通用规则</p>
            <h3 class="section-title section-title--small">通用规则</h3>
          </div>
          <span class="selected-pill">{{ ruleConfig?.isBuiltIn ? '系统内置' : '人工配置' }}</span>
        </div>

        <div v-if="ruleConfig" class="detail-overview detail-overview--compact">
          <article class="detail-overview__item"><span>每超 1 级加成</span><strong>+{{ ruleConfig.successBonusPerOverLevel }}%</strong></article>
          <article class="detail-overview__item"><span>最大成功率加成</span><strong>+{{ ruleConfig.maxSuccessBonus }}%</strong></article>
          <article class="detail-overview__item"><span>成功经验基础值</span><strong>{{ ruleConfig.successExpBase }}</strong></article>
          <article class="detail-overview__item"><span>成功经验等级系数</span><strong>{{ ruleConfig.successExpPerRequiredLevel }}</strong></article>
          <article class="detail-overview__item"><span>失败经验基础值</span><strong>{{ ruleConfig.failureExpBase }}</strong></article>
          <article class="detail-overview__item"><span>失败经验等级系数</span><strong>{{ ruleConfig.failureExpPerRequiredLevel }}</strong></article>
          <article class="detail-overview__item"><span>内置版本</span><strong>{{ ruleConfig.builtInVersion || '无' }}</strong></article>
        </div>

        <div class="filter-actions filter-actions--inline">
          <button class="primary-button" type="button" :disabled="!canManageConfigs || !ruleConfig" @click="ruleEditorOpen = true">编辑通用规则</button>
        </div>
      </section>

      <section class="panel-box table-panel">
        <div class="section-header-row">
          <div class="section-header-copy">
            <p class="section-kicker">等级曲线</p>
            <h3 class="section-title section-title--small">等级经验曲线</h3>
          </div>
          <span class="selected-pill">共 {{ levelRules.length }} 条</span>
        </div>

        <div class="table-wrap">
          <table class="data-table">
            <thead>
              <tr>
                <th>等级</th>
                <th>下一级经验</th>
                <th>来源</th>
                <th>状态</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="rule in levelRules" :key="rule.level" @click="openEditLevelModal(rule.level)" :class="{ 'is-active': selectedLevelRule?.level === rule.level }">
                <td>Lv.{{ rule.level }}</td>
                <td>{{ rule.nextLevelExp }}</td>
                <td>{{ rule.isBuiltIn ? `内置 · ${rule.builtInVersion || '未标记版本'}` : '人工配置' }}</td>
                <td>{{ rule.isEnabled ? '启用' : '停用' }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </section>


    <AdminModal v-model="ruleEditorOpen" kicker="锻造通用规则" title="编辑锻造职业通用规则" description="维护锻造职业的成功率加成和经验收益规则。">
      <fieldset v-if="ruleConfig" class="form-fieldset" :disabled="!canManageConfigs">
        <div class="detail-overview detail-overview--compact">
          <article class="detail-overview__item"><span>来源</span><strong>{{ ruleConfig.isBuiltIn ? '系统内置' : '人工配置' }}</strong></article>
          <article class="detail-overview__item"><span>种子键</span><strong>{{ ruleConfig.seedKey || '无' }}</strong></article>
          <article class="detail-overview__item"><span>内置版本</span><strong>{{ ruleConfig.builtInVersion || '无' }}</strong></article>
          <article class="detail-overview__item"><span>最后更新</span><strong>{{ ruleConfig.lastUpdateTime || '未记录' }}</strong></article>
        </div>

        <div class="editor-grid editor-grid--three">
          <label class="field"><span>每超 1 级加成</span><input v-model.number="ruleConfig.successBonusPerOverLevel" type="number" min="0" /></label>
          <label class="field"><span>最大成功率加成</span><input v-model.number="ruleConfig.maxSuccessBonus" type="number" min="0" /></label>
          <label class="field"><span>成功经验基础值</span><input v-model.number="ruleConfig.successExpBase" type="number" min="0" /></label>
          <label class="field"><span>成功经验等级系数</span><input v-model.number="ruleConfig.successExpPerRequiredLevel" type="number" min="0" /></label>
          <label class="field"><span>失败经验基础值</span><input v-model.number="ruleConfig.failureExpBase" type="number" min="0" /></label>
          <label class="field"><span>失败经验等级系数</span><input v-model.number="ruleConfig.failureExpPerRequiredLevel" type="number" min="0" /></label>
          <label class="field checkbox-field"><input v-model="ruleConfig.isEnabled" type="checkbox" /><span>启用规则</span></label>
        </div>
      </fieldset>

      <template #footer>
        <button class="secondary-button" type="button" @click="ruleEditorOpen = false">取消</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs || !ruleConfig" @click="saveRule">保存通用规则</button>
      </template>
    </AdminModal>

    <AdminModal v-model="levelEditorOpen" kicker="锻造等级规则" :title="selectedLevelRule ? `编辑等级 Lv.${selectedLevelRule.level}` : '新建等级规则'" description="维护锻造师等级经验曲线。">
      <fieldset v-if="selectedLevelRule" class="form-fieldset" :disabled="!canManageConfigs">
        <div class="detail-overview detail-overview--compact">
          <article class="detail-overview__item"><span>来源</span><strong>{{ selectedLevelRule.isBuiltIn ? '系统内置' : '人工配置' }}</strong></article>
          <article class="detail-overview__item"><span>种子键</span><strong>{{ selectedLevelRule.seedKey || '无' }}</strong></article>
          <article class="detail-overview__item"><span>内置版本</span><strong>{{ selectedLevelRule.builtInVersion || '无' }}</strong></article>
          <article class="detail-overview__item"><span>最后更新</span><strong>{{ selectedLevelRule.lastUpdateTime || '未记录' }}</strong></article>
        </div>

        <div class="editor-grid editor-grid--three">
          <label class="field"><span>等级</span><input v-model.number="selectedLevelRule.level" type="number" min="1" max="50" /></label>
          <label class="field"><span>下一级经验</span><input v-model.number="selectedLevelRule.nextLevelExp" type="number" min="1" /></label>
          <label class="field checkbox-field"><input v-model="selectedLevelRule.isEnabled" type="checkbox" /><span>启用规则</span></label>
        </div>
      </fieldset>

      <template #footer>
        <button class="secondary-button" type="button" @click="levelEditorOpen = false">取消</button>
        <button class="danger-button" type="button" :disabled="!canManageConfigs || !selectedLevelRule?.level" @click="removeLevelRule">删除</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs || !selectedLevelRule" @click="saveLevelRule">保存等级规则</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import {
  deleteAdminForgeProfessionLevelRule,
  getAdminForgeProfessionLevelRule,
  getAdminForgeProfessionLevelRules,
  getAdminForgeProfessionRule,
  saveAdminForgeProfessionLevelRule,
  saveAdminForgeProfessionRule
} from '@/services/forge-runtime'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import type { AdminForgeProfessionLevelRule, AdminForgeProfessionRule } from '@/types/admin'
import toast from '@/utils/toast'

// 页面主状态：
// ruleConfig 是锻造职业通用规则，levelRules 是等级经验曲线，selectedLevelRule 是当前编辑行。
const { canManageConfigs } = useAdminPermissions()
const levelRules = ref<AdminForgeProfessionLevelRule[]>([])
const ruleConfig = ref<AdminForgeProfessionRule | null>(null)
const selectedLevelRule = ref<AdminForgeProfessionLevelRule | null>(null)
const ruleEditorOpen = ref(false)
const levelEditorOpen = ref(false)

// 并行加载通用规则和等级规则。
async function loadData() {
  const [nextRule, nextLevels] = await Promise.all([
    getAdminForgeProfessionRule(),
    getAdminForgeProfessionLevelRules()
  ])
  ruleConfig.value = nextRule
  levelRules.value = nextLevels
}

// 保存锻造职业通用规则。
async function saveRule() {
  if (!canManageConfigs.value || !ruleConfig.value) return
  try {
    ruleConfig.value = await saveAdminForgeProfessionRule(ruleConfig.value)
    toast.success('锻造职业通用规则保存成功。')
    ruleEditorOpen.value = false
    await loadData()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '锻造职业通用规则保存失败。')
  }
}

// 打开等级规则编辑弹窗。
async function openEditLevelModal(level: number) {
  selectedLevelRule.value = await getAdminForgeProfessionLevelRule(level)
  levelEditorOpen.value = true
}

// 打开新建等级规则弹窗。
function openCreateLevelModal() {
  selectedLevelRule.value = {
    level: 1,
    nextLevelExp: 80,
    isEnabled: true,
    isBuiltIn: false,
    builtInVersion: null,
    seedKey: null,
    lastUpdateTime: null
  }
  levelEditorOpen.value = true
}

// 保存锻造职业等级规则。
async function saveLevelRule() {
  if (!canManageConfigs.value || !selectedLevelRule.value) return
  try {
    selectedLevelRule.value = await saveAdminForgeProfessionLevelRule(selectedLevelRule.value)
    toast.success('锻造职业等级规则保存成功。')
    levelEditorOpen.value = false
    await loadData()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '锻造职业等级规则保存失败。')
  }
}

// 删除锻造职业等级规则。
async function removeLevelRule() {
  if (!canManageConfigs.value || !selectedLevelRule.value?.level) return
  if (!window.confirm(`确认删除锻造职业等级 Lv.${selectedLevelRule.value.level} 规则吗？`)) return
  try {
    await deleteAdminForgeProfessionLevelRule(selectedLevelRule.value.level)
    toast.success('锻造职业等级规则删除成功。')
    levelEditorOpen.value = false
    await loadData()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '锻造职业等级规则删除失败。')
  }
}

onMounted(loadData)
</script>
