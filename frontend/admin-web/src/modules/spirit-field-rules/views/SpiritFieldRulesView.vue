<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">灵田规则</p>
        <h2 class="section-title section-title--small">灵田规则</h2>
        <p class="section-note">维护灵田默认地块、地块升级收益与催熟道具规则。</p>
      </div>

      <div class="filter-grid">
        <label class="field">
          <span>当前系统</span>
          <input :value="systemRule?.configId || ''" type="text" placeholder="未加载" disabled />
        </label>
        <label class="field">
          <span>催熟道具数</span>
          <input :value="String(speedUpRules.length)" type="text" placeholder="0" disabled />
        </label>
        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadData">刷新</button>
          <button class="primary-button" type="button" :disabled="!canManageConfigs" @click="openCreateSpeedUpModal">新建催熟规则</button>
        </div>
      </div>
    </section>

    <section class="dashboard-section-grid">
      <section class="panel-box table-panel">
        <div class="section-header-row">
          <div class="section-header-copy">
            <p class="section-kicker">系统规则</p>
            <h3 class="section-title section-title--small">系统规则</h3>
          </div>
          <span class="selected-pill">{{ systemRule?.isBuiltIn ? '系统内置' : '人工配置' }}</span>
        </div>

        <div v-if="systemRule" class="detail-overview detail-overview--compact">
          <article class="detail-overview__item"><span>默认灵田等级</span><strong>{{ systemRule.defaultFieldLevel }}</strong></article>
          <article class="detail-overview__item"><span>默认解锁地块</span><strong>{{ systemRule.defaultUnlockedPlots }}</strong></article>
          <article class="detail-overview__item"><span>最大地块数</span><strong>{{ systemRule.defaultMaxPlots }}</strong></article>
          <article class="detail-overview__item"><span>默认背包容量</span><strong>{{ systemRule.defaultInventoryCapacity }}</strong></article>
          <article class="detail-overview__item"><span>升级金币系数</span><strong>{{ systemRule.plotUpgradeGoldPerLevel }}</strong></article>
          <article class="detail-overview__item"><span>升级灵石系数</span><strong>{{ systemRule.plotUpgradeSpiritStonePerLevel }}</strong></article>
          <article class="detail-overview__item"><span>地块产量加成</span><strong>+{{ systemRule.plotUpgradeYieldBonusPerLevel }}%</strong></article>
          <article class="detail-overview__item"><span>内置版本</span><strong>{{ systemRule.builtInVersion || '无' }}</strong></article>
        </div>

        <div class="filter-actions filter-actions--inline">
          <button class="primary-button" type="button" :disabled="!canManageConfigs || !systemRule" @click="systemEditorOpen = true">编辑系统规则</button>
        </div>
      </section>

      <section class="panel-box table-panel">
        <div class="section-header-row">
          <div class="section-header-copy">
            <p class="section-kicker">催熟规则</p>
            <h3 class="section-title section-title--small">催熟道具规则</h3>
          </div>
          <span class="selected-pill">共 {{ speedUpRules.length }} 条</span>
        </div>

        <div class="table-wrap">
          <table class="data-table">
            <thead>
              <tr>
                <th>道具</th>
                <th>缩时秒数</th>
                <th>排序</th>
                <th>来源</th>
                <th>状态</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="rule in speedUpRules" :key="rule.itemId" @click="openEditSpeedUpModal(rule.itemId)" :class="{ 'is-active': selectedSpeedUpRule?.itemId === rule.itemId }">
                <td>{{ itemNameMap[rule.itemId] || rule.itemId }}</td>
                <td>{{ rule.speedUpSeconds }}</td>
                <td>{{ rule.sortOrder }}</td>
                <td>{{ rule.isBuiltIn ? `内置 · ${rule.builtInVersion || '未标记版本'}` : '人工配置' }}</td>
                <td>{{ rule.isEnabled ? '启用' : '停用' }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </section>


    <AdminModal v-model="systemEditorOpen" kicker="灵田系统规则" title="编辑灵田系统规则" description="维护灵田默认地块、背包容量和地块升级收益。">
      <fieldset v-if="systemRule" class="form-fieldset" :disabled="!canManageConfigs">
        <div class="detail-overview detail-overview--compact">
          <article class="detail-overview__item"><span>来源</span><strong>{{ systemRule.isBuiltIn ? '系统内置' : '人工配置' }}</strong></article>
          <article class="detail-overview__item"><span>种子键</span><strong>{{ systemRule.seedKey || '无' }}</strong></article>
          <article class="detail-overview__item"><span>内置版本</span><strong>{{ systemRule.builtInVersion || '无' }}</strong></article>
          <article class="detail-overview__item"><span>最后更新</span><strong>{{ systemRule.lastUpdateTime || '未记录' }}</strong></article>
        </div>

        <div class="editor-grid editor-grid--three">
          <label class="field"><span>默认灵田等级</span><input v-model.number="systemRule.defaultFieldLevel" type="number" min="1" /></label>
          <label class="field"><span>默认解锁地块</span><input v-model.number="systemRule.defaultUnlockedPlots" type="number" min="1" /></label>
          <label class="field"><span>最大地块数</span><input v-model.number="systemRule.defaultMaxPlots" type="number" min="1" /></label>
          <label class="field"><span>默认背包容量</span><input v-model.number="systemRule.defaultInventoryCapacity" type="number" min="1" /></label>
          <label class="field"><span>升级金币系数</span><input v-model.number="systemRule.plotUpgradeGoldPerLevel" type="number" min="0" /></label>
          <label class="field"><span>升级灵石系数</span><input v-model.number="systemRule.plotUpgradeSpiritStonePerLevel" type="number" min="0" /></label>
          <label class="field"><span>地块产量加成</span><input v-model.number="systemRule.plotUpgradeYieldBonusPerLevel" type="number" min="0" /></label>
          <label class="field checkbox-field"><input v-model="systemRule.isEnabled" type="checkbox" /><span>启用规则</span></label>
        </div>
      </fieldset>

      <template #footer>
        <button class="secondary-button" type="button" @click="systemEditorOpen = false">取消</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs || !systemRule" @click="saveSystemRule">保存系统规则</button>
      </template>
    </AdminModal>

    <AdminModal v-model="speedUpEditorOpen" kicker="催熟规则" :title="selectedSpeedUpRule?.itemId ? `编辑催熟规则 ${selectedSpeedUpRule.itemId}` : '新建催熟规则'" description="维护允许催熟的道具和每个道具缩短的时间。">
      <fieldset v-if="selectedSpeedUpRule" class="form-fieldset" :disabled="!canManageConfigs">
        <div class="detail-overview detail-overview--compact">
          <article class="detail-overview__item"><span>来源</span><strong>{{ selectedSpeedUpRule.isBuiltIn ? '系统内置' : '人工配置' }}</strong></article>
          <article class="detail-overview__item"><span>种子键</span><strong>{{ selectedSpeedUpRule.seedKey || '无' }}</strong></article>
          <article class="detail-overview__item"><span>内置版本</span><strong>{{ selectedSpeedUpRule.builtInVersion || '无' }}</strong></article>
          <article class="detail-overview__item"><span>最后更新</span><strong>{{ selectedSpeedUpRule.lastUpdateTime || '未记录' }}</strong></article>
        </div>

        <div class="editor-grid editor-grid--three">
          <label class="field"><span>道具编号</span><input v-model.trim="selectedSpeedUpRule.itemId" type="text" /></label>
          <label class="field"><span>缩时秒数</span><input v-model.number="selectedSpeedUpRule.speedUpSeconds" type="number" min="1" /></label>
          <label class="field"><span>排序</span><input v-model.number="selectedSpeedUpRule.sortOrder" type="number" min="0" /></label>
          <label class="field checkbox-field"><input v-model="selectedSpeedUpRule.isEnabled" type="checkbox" /><span>启用规则</span></label>
        </div>
      </fieldset>

      <template #footer>
        <button class="secondary-button" type="button" @click="speedUpEditorOpen = false">取消</button>
        <button class="danger-button" type="button" :disabled="!canManageConfigs || !selectedSpeedUpRule?.itemId" @click="removeSpeedUpRule">删除</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs || !selectedSpeedUpRule" @click="saveSpeedUpRule">保存催熟规则</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import {
  deleteAdminSpiritFieldSpeedUpRule,
  getAdminSpiritFieldSpeedUpRule,
  getAdminSpiritFieldSpeedUpRules,
  getAdminSpiritFieldSystemRule,
  saveAdminSpiritFieldSpeedUpRule,
  saveAdminSpiritFieldSystemRule
} from '@/services/spirit-fields'
import { getAdminItems } from '@/services/items'
import type { AdminSpiritFieldSpeedUpItemRule, AdminSpiritFieldSystemRule } from '@/types/admin'
import toast from '@/utils/toast'

const { canManageConfigs } = useAdminPermissions()
const systemRule = ref<AdminSpiritFieldSystemRule | null>(null)
const speedUpRules = ref<AdminSpiritFieldSpeedUpItemRule[]>([])
const itemTemplates = ref<Array<{ itemId: string; name: string }>>([])
const itemNameMap = computed(() => Object.fromEntries(itemTemplates.value.map((item) => [item.itemId, item.name])))
const selectedSpeedUpRule = ref<AdminSpiritFieldSpeedUpItemRule | null>(null)
const systemEditorOpen = ref(false)
const speedUpEditorOpen = ref(false)

async function loadData() {
  const [nextSystemRule, nextSpeedUpRules, items] = await Promise.all([
    getAdminSpiritFieldSystemRule(),
    getAdminSpiritFieldSpeedUpRules(),
    getAdminItems()
  ])
  systemRule.value = nextSystemRule
  speedUpRules.value = nextSpeedUpRules
  itemTemplates.value = items
}

async function saveSystemRule() {
  if (!canManageConfigs.value || !systemRule.value) return
  try {
    systemRule.value = await saveAdminSpiritFieldSystemRule(systemRule.value)
    toast.success('灵田系统规则保存成功。')
    systemEditorOpen.value = false
    await loadData()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '灵田系统规则保存失败。')
  }
}

async function openEditSpeedUpModal(itemId: string) {
  selectedSpeedUpRule.value = await getAdminSpiritFieldSpeedUpRule(itemId)
  speedUpEditorOpen.value = true
}

function openCreateSpeedUpModal() {
  selectedSpeedUpRule.value = {
    itemId: '',
    speedUpSeconds: 3600,
    sortOrder: 0,
    isEnabled: true,
    isBuiltIn: false,
    seedKey: null,
    builtInVersion: null,
    lastUpdateTime: null
  }
  speedUpEditorOpen.value = true
}

async function saveSpeedUpRule() {
  if (!canManageConfigs.value || !selectedSpeedUpRule.value) return
  try {
    selectedSpeedUpRule.value = await saveAdminSpiritFieldSpeedUpRule(selectedSpeedUpRule.value)
    toast.success('灵田催熟规则保存成功。')
    speedUpEditorOpen.value = false
    await loadData()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '灵田催熟规则保存失败。')
  }
}

async function removeSpeedUpRule() {
  if (!canManageConfigs.value || !selectedSpeedUpRule.value?.itemId) return
  if (!window.confirm(`确认删除催熟规则 ${selectedSpeedUpRule.value.itemId} 吗？`)) return
  try {
    await deleteAdminSpiritFieldSpeedUpRule(selectedSpeedUpRule.value.itemId)
    toast.success('灵田催熟规则删除成功。')
    speedUpEditorOpen.value = false
    await loadData()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '灵田催熟规则删除失败。')
  }
}

onMounted(loadData)
</script>
