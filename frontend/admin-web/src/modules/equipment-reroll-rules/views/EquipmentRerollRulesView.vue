<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">装备洗练规则</p>
        <h2 class="section-title section-title--small">装备洗练规则</h2>
        <p class="section-note">管理洗练系统配置、品阶、词条池与属性值区间。</p>
      </div>

      <div class="filter-grid">
        <label class="field">
          <span>洗练石道具</span>
          <input :value="systemConfig?.rerollStoneItemId || ''" type="text" placeholder="未加载" disabled />
        </label>
        <label class="field">
          <span>词条池条数</span>
          <input :value="String(poolConfigs.length)" type="text" placeholder="0" disabled />
        </label>
        <label class="field">
          <span>属性值配置条数</span>
          <input :value="String(attrValueConfigs.length)" type="text" placeholder="0" disabled />
        </label>
        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadAll" :disabled="loading">刷新</button>
        </div>
      </div>
    </section>

    <section class="dashboard-section-grid">
      <!-- 系统配置 -->
      <section class="panel-box table-panel">
        <div class="section-header-row">
          <div class="section-header-copy">
            <p class="section-kicker">系统配置</p>
            <h3 class="section-title section-title--small">系统配置</h3>
          </div>
          <span class="selected-pill">{{ systemConfig?.isBuiltIn ? '系统内置' : '人工配置' }}</span>
        </div>

        <div v-if="systemConfig" class="detail-overview detail-overview--compact">
          <article class="detail-overview__item"><span>状态</span><strong :class="systemConfig.isEnabled ? 'status-on' : 'status-off'">{{ systemConfig.isEnabled ? '已开启' : '已关闭' }}</strong></article>
          <article class="detail-overview__item"><span>最大锁定条数</span><strong>{{ systemConfig.maxLockedLineCount }}</strong></article>
        </div>
        <div v-else class="empty-state">暂无系统配置</div>

        <div class="filter-actions filter-actions--inline">
          <button class="primary-button" type="button" :disabled="!canManageConfigs || !systemConfig" @click="openSystemConfigEdit">编辑系统配置</button>
        </div>
      </section>

      <!-- 品阶配置 -->
      <section class="panel-box table-panel">
        <div class="section-header-row">
          <div class="section-header-copy">
            <p class="section-kicker">品阶配置</p>
            <h3 class="section-title section-title--small">品阶配置</h3>
          </div>
          <span class="selected-pill">共 {{ tierConfigs.length }} 条</span>
        </div>

        <div class="table-wrap">
          <table class="data-table">
            <thead>
              <tr>
                <th>品阶</th>
                <th>名称</th>
                <th>颜色</th>
                <th>权重</th>
                <th>操作</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="tier in tierConfigs" :key="tier.gid" @click="openTierEdit(tier)">
                <td>{{ tier.tier }}</td>
                <td><span :style="{ color: tier.color }">{{ tier.name }}</span></td>
                <td><span class="color-dot" :style="{ background: tier.color }"></span> {{ tier.color }}</td>
                <td>{{ tier.weight }}</td>
                <td>
                  <button class="danger-button" type="button" @click.stop="removeTier(tier)" :disabled="!canManageConfigs">删除</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <div class="filter-actions filter-actions--inline">
          <button class="primary-button" type="button" :disabled="!canManageConfigs || loading" @click="openTierCreate">新增品阶</button>
        </div>
      </section>
    </section>

    <!-- 强化消耗规则 -->
    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">强化消耗规则</p>
          <h3 class="section-title section-title--small">按装备等级配置强化</h3>
        </div>
        <span class="selected-pill">共 {{ enhanceRules.length }} 条</span>
      </div>
      <div class="table-wrap">
        <table class="data-table">
          <thead><tr><th>装备等级</th><th>材料</th><th>数量</th><th>金币</th><th>成功率</th><th>属性成长</th><th>操作</th></tr></thead>
          <tbody>
            <tr v-for="rule in enhanceRules" :key="rule.gid" @click="openEnhanceEdit(rule)">
              <td>{{ rule.minEquipmentLevel }} - {{ rule.maxEquipmentLevel }}</td>
              <td>{{ rule.materialItemId }}</td>
              <td>{{ rule.materialCount }}</td>
              <td>{{ rule.goldCost }}</td>
              <td>{{ rule.successRate }}%</td>
              <td>{{ rule.attributeGrowthPercent }}%</td>
              <td><button class="danger-button" type="button" @click.stop="removeEnhance(rule)" :disabled="!canManageConfigs">删除</button></td>
            </tr>
          </tbody>
        </table>
      </div>
      <div class="filter-actions filter-actions--inline">
        <button class="primary-button" type="button" :disabled="!canManageConfigs || loading" @click="openEnhanceCreate">新增强化规则</button>
      </div>
    </section>

    <!-- 洗炼消耗规则 -->
    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">洗炼消耗规则</p>
          <h3 class="section-title section-title--small">按装备等级配置洗炼</h3>
        </div>
        <span class="selected-pill">共 {{ rerollCostRules.length }} 条</span>
      </div>
      <div class="table-wrap">
        <table class="data-table">
          <thead><tr><th>装备等级</th><th>材料</th><th>基础数量</th><th>金币</th><th>锁定额外材料</th><th>操作</th></tr></thead>
          <tbody>
            <tr v-for="rule in rerollCostRules" :key="rule.gid" @click="openRerollCostEdit(rule)">
              <td>{{ rule.minEquipmentLevel }} - {{ rule.maxEquipmentLevel }}</td>
              <td>{{ rule.materialItemId }}</td>
              <td>{{ rule.materialCount }}</td>
              <td>{{ rule.goldCost }}</td>
              <td>{{ rule.extraMaterialPerLockedLine }}</td>
              <td><button class="danger-button" type="button" @click.stop="removeRerollCost(rule)" :disabled="!canManageConfigs">删除</button></td>
            </tr>
          </tbody>
        </table>
      </div>
      <div class="filter-actions filter-actions--inline">
        <button class="primary-button" type="button" :disabled="!canManageConfigs || loading" @click="openRerollCostCreate">新增洗炼规则</button>
      </div>
    </section>

    <AdminModal v-model="enhanceEditorOpen" kicker="强化规则" :title="enhanceEditor?.gid ? '编辑强化规则' : '新增强化规则'" description="等级区间不可与其他启用规则重叠。">
      <fieldset v-if="enhanceEditor" class="form-fieldset" :disabled="!canManageConfigs">
        <div class="editor-grid editor-grid--three">
          <label class="field"><span>最低装备等级</span><input v-model.number="enhanceEditor.minEquipmentLevel" type="number" min="1" /></label>
          <label class="field"><span>最高装备等级</span><input v-model.number="enhanceEditor.maxEquipmentLevel" type="number" min="1" /></label>
          <label class="field"><span>材料道具 ID</span><input v-model="enhanceEditor.materialItemId" type="text" /></label>
          <label class="field"><span>材料数量</span><input v-model.number="enhanceEditor.materialCount" type="number" min="1" /></label>
          <label class="field"><span>金币消耗</span><input v-model.number="enhanceEditor.goldCost" type="number" min="0" /></label>
          <label class="field"><span>成功率%</span><input v-model.number="enhanceEditor.successRate" type="number" min="0" max="100" /></label>
          <label class="field"><span>属性成长%</span><input v-model.number="enhanceEditor.attributeGrowthPercent" type="number" min="0" max="100" /></label>
          <label class="field"><span>强化上限</span><input v-model.number="enhanceEditor.maxEnhanceLevel" type="number" min="1" /></label>
          <label class="field checkbox-field"><input v-model="enhanceEditor.isEnabled" type="checkbox" /><span>启用</span></label>
        </div>
      </fieldset>
      <template #footer><button class="secondary-button" type="button" @click="enhanceEditorOpen = false">取消</button><button class="primary-button" type="button" :disabled="!canManageConfigs || saving" @click="saveEnhance">保存</button></template>
    </AdminModal>

    <AdminModal v-model="rerollCostEditorOpen" kicker="洗炼消耗规则" :title="rerollCostEditor?.gid ? '编辑洗炼规则' : '新增洗炼规则'" description="等级区间不可与其他启用规则重叠。">
      <fieldset v-if="rerollCostEditor" class="form-fieldset" :disabled="!canManageConfigs">
        <div class="editor-grid editor-grid--three">
          <label class="field"><span>最低装备等级</span><input v-model.number="rerollCostEditor.minEquipmentLevel" type="number" min="1" /></label>
          <label class="field"><span>最高装备等级</span><input v-model.number="rerollCostEditor.maxEquipmentLevel" type="number" min="1" /></label>
          <label class="field"><span>材料道具 ID</span><input v-model="rerollCostEditor.materialItemId" type="text" /></label>
          <label class="field"><span>基础材料数量</span><input v-model.number="rerollCostEditor.materialCount" type="number" min="1" /></label>
          <label class="field"><span>基础金币消耗</span><input v-model.number="rerollCostEditor.goldCost" type="number" min="0" /></label>
          <label class="field"><span>锁定额外材料</span><input v-model.number="rerollCostEditor.extraMaterialPerLockedLine" type="number" min="0" /></label>
          <label class="field"><span>锁定额外金币</span><input v-model.number="rerollCostEditor.extraGoldPerLockedLine" type="number" min="0" /></label>
          <label class="field checkbox-field"><input v-model="rerollCostEditor.isEnabled" type="checkbox" /><span>启用</span></label>
        </div>
      </fieldset>
      <template #footer><button class="secondary-button" type="button" @click="rerollCostEditorOpen = false">取消</button><button class="primary-button" type="button" :disabled="!canManageConfigs || saving" @click="saveRerollCost">保存</button></template>
    </AdminModal>

    <!-- 属性值配置 -->
    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">属性值配置</p>
          <h3 class="section-title section-title--small">属性值配置</h3>
        </div>
        <div class="section-header-actions">
          <label class="field">
            <span>属性筛选</span>
            <select v-model="filterAttrType">
              <option value="">全部属性</option>
              <option v-for="a in attributeOptions" :key="a.value" :value="a.value">{{ a.label }}</option>
            </select>
          </label>
          <label class="field">
            <span>品阶筛选</span>
            <select v-model="filterTier">
              <option value="">全部品阶</option>
              <option v-for="t in tierConfigs" :key="t.tier" :value="t.tier">{{ t.name }}</option>
            </select>
          </label>
          <span class="selected-pill">共 {{ filteredAttrValueConfigs.length }} 条</span>
        </div>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>属性类型</th>
              <th>品阶</th>
              <th>最小值</th>
              <th>最大值</th>
              <th>百分比</th>
              <th>操作</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="av in filteredAttrValueConfigs" :key="av.gid" @click="openAttrValueEdit(av)">
              <td>{{ getAttributeName(av.attributeType) }}</td>
              <td><span :style="{ color: getTierColor(av.tier) }">{{ getTierName(av.tier) }}</span></td>
              <td>{{ av.minValue }}</td>
              <td>{{ av.maxValue }}</td>
              <td>{{ av.isPercentage ? '是' : '否' }}</td>
              <td>
                <button class="danger-button" type="button" @click.stop="removeAttrValue(av)" :disabled="!canManageConfigs">删除</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div class="filter-actions filter-actions--inline">
        <button class="primary-button" type="button" :disabled="!canManageConfigs || loading" @click="openAttrValueCreate">新增属性值配置</button>
      </div>
    </section>

    <!-- 词条池配置 -->
    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">槽位词条池</p>
          <h3 class="section-title section-title--small">槽位词条池</h3>
        </div>
        <div class="section-header-actions">
          <label class="field">
            <span>槽位筛选</span>
            <select v-model="filterSlot">
              <option value="">全部槽位</option>
              <option v-for="s in slotOptions" :key="s.value" :value="s.value">{{ s.label }}</option>
            </select>
          </label>
          <span class="selected-pill">共 {{ filteredPoolConfigs.length }} 条</span>
        </div>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>槽位</th>
              <th>属性类型</th>
              <th>品阶</th>
              <th>操作</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="pool in filteredPoolConfigs" :key="pool.gid" @click="openPoolEdit(pool)">
              <td>{{ getSlotName(pool.slot) }}</td>
              <td>{{ getAttributeName(pool.attributeType) }}</td>
              <td><span :style="{ color: getTierColor(pool.tier) }">{{ getTierName(pool.tier) }}</span></td>
              <td>
                <button class="danger-button" type="button" @click.stop="removePool(pool)" :disabled="!canManageConfigs">删除</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div class="filter-actions filter-actions--inline">
        <button class="primary-button" type="button" :disabled="!canManageConfigs || loading" @click="openPoolCreate">新增词条池</button>
      </div>
    </section>

    <!-- 系统配置编辑弹窗 -->
    <AdminModal v-model="systemEditorOpen" kicker="系统配置" title="编辑系统配置" description="维护洗练系统的核心参数。">
      <fieldset v-if="systemEditor" class="form-fieldset" :disabled="!canManageConfigs">
        <div class="editor-grid editor-grid--three">
          <label class="field checkbox-field"><input v-model="systemEditor.isEnabled" type="checkbox" /><span>开启洗练</span></label>
          <label class="field"><span>最大锁定条数</span><input v-model.number="systemEditor.maxLockedLineCount" type="number" min="0" max="4" /></label>
          <label class="field"><span>说明</span><input value="材料和金币请在下方等级区间规则中配置" type="text" disabled /></label>
        </div>
      </fieldset>

      <template #footer>
        <button class="secondary-button" type="button" @click="systemEditorOpen = false">取消</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs || saving" @click="saveSystemConfig">保存</button>
      </template>
    </AdminModal>

    <!-- 品阶编辑弹窗 -->
    <AdminModal v-model="tierEditorOpen" kicker="品阶配置" :title="tierEditor?.gid ? '编辑品阶' : '新增品阶'" description="维护洗练品阶名称、颜色与权重。">
      <fieldset v-if="tierEditor" class="form-fieldset" :disabled="!canManageConfigs">
        <div v-if="tierEditor.gid" class="detail-overview detail-overview--compact">
          <article class="detail-overview__item"><span>来源</span><strong>{{ tierEditor.isBuiltIn ? '系统内置' : '人工配置' }}</strong></article>
          <article class="detail-overview__item"><span>种子键</span><strong>{{ tierEditor.seedKey || '无' }}</strong></article>
          <article class="detail-overview__item"><span>最后更新</span><strong>{{ tierEditor.lastUpdateTime || '未记录' }}</strong></article>
        </div>

        <div class="editor-grid editor-grid--three">
          <label class="field"><span>品阶数字</span><input v-model.number="tierEditor.tier" type="number" min="1" /></label>
          <label class="field"><span>名称</span><input v-model="tierEditor.name" type="text" /></label>
          <label class="field"><span>颜色</span><input v-model="tierEditor.color" type="color" /></label>
          <label class="field"><span>权重</span><input v-model.number="tierEditor.weight" type="number" min="1" /></label>
          <label class="field checkbox-field"><input v-model="tierEditor.isEnabled" type="checkbox" /><span>启用</span></label>
        </div>
      </fieldset>

      <template #footer>
        <button class="secondary-button" type="button" @click="tierEditorOpen = false">取消</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs || saving" @click="saveTier">保存</button>
      </template>
    </AdminModal>

    <!-- 属性值配置编辑弹窗 -->
    <AdminModal v-model="attrValueEditorOpen" kicker="属性值配置" :title="attrValueEditor?.gid ? '编辑属性值配置' : '新增属性值配置'" description="按属性类型和品阶配置数值区间。">
      <fieldset v-if="attrValueEditor" class="form-fieldset" :disabled="!canManageConfigs">
        <div v-if="attrValueEditor.gid" class="detail-overview detail-overview--compact">
          <article class="detail-overview__item"><span>来源</span><strong>{{ attrValueEditor.isBuiltIn ? '系统内置' : '人工配置' }}</strong></article>
          <article class="detail-overview__item"><span>种子键</span><strong>{{ attrValueEditor.seedKey || '无' }}</strong></article>
          <article class="detail-overview__item"><span>最后更新</span><strong>{{ attrValueEditor.lastUpdateTime || '未记录' }}</strong></article>
        </div>

        <div class="editor-grid editor-grid--three">
          <label class="field"><span>属性类型</span>
            <select v-model.number="attrValueEditor.attributeType">
              <option v-for="a in attributeOptions" :key="a.value" :value="a.value">{{ a.label }}</option>
            </select>
          </label>
          <label class="field"><span>品阶</span>
            <select v-model.number="attrValueEditor.tier">
              <option v-for="t in tierConfigs" :key="t.tier" :value="t.tier">{{ t.name }}</option>
            </select>
          </label>
          <label class="field"><span>最小值</span><input v-model="attrValueEditor.minValue" type="text" /></label>
          <label class="field"><span>最大值</span><input v-model="attrValueEditor.maxValue" type="text" /></label>
          <label class="field checkbox-field"><input v-model="attrValueEditor.isPercentage" type="checkbox" /><span>百分比属性</span></label>
          <label class="field checkbox-field"><input v-model="attrValueEditor.isEnabled" type="checkbox" /><span>启用</span></label>
        </div>
      </fieldset>

      <template #footer>
        <button class="secondary-button" type="button" @click="attrValueEditorOpen = false">取消</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs || saving" @click="saveAttrValue">保存</button>
      </template>
    </AdminModal>

    <!-- 词条池编辑弹窗 -->
    <AdminModal v-model="poolEditorOpen" kicker="词条池配置" :title="poolEditor?.gid ? '编辑词条池' : '新增词条池'" description="配置装备部位可出现的属性类型与品阶组合。">
      <fieldset v-if="poolEditor" class="form-fieldset" :disabled="!canManageConfigs">
        <div v-if="poolEditor.gid" class="detail-overview detail-overview--compact">
          <article class="detail-overview__item"><span>来源</span><strong>{{ poolEditor.isBuiltIn ? '系统内置' : '人工配置' }}</strong></article>
          <article class="detail-overview__item"><span>种子键</span><strong>{{ poolEditor.seedKey || '无' }}</strong></article>
          <article class="detail-overview__item"><span>最后更新</span><strong>{{ poolEditor.lastUpdateTime || '未记录' }}</strong></article>
        </div>

        <div class="editor-grid editor-grid--three">
          <label class="field"><span>槽位</span>
            <select v-model.number="poolEditor.slot">
              <option v-for="s in slotOptions" :key="s.value" :value="s.value">{{ s.label }}</option>
            </select>
          </label>
          <label class="field"><span>属性类型</span>
            <select v-model.number="poolEditor.attributeType">
              <option v-for="a in attributeOptions" :key="a.value" :value="a.value">{{ a.label }}</option>
            </select>
          </label>
          <label class="field"><span>品阶</span>
            <select v-model.number="poolEditor.tier">
              <option v-for="t in tierConfigs" :key="t.tier" :value="t.tier">{{ t.name }}</option>
            </select>
          </label>
          <label class="field checkbox-field"><input v-model="poolEditor.isEnabled" type="checkbox" /><span>启用</span></label>
        </div>
      </fieldset>

      <template #footer>
        <button class="secondary-button" type="button" @click="poolEditorOpen = false">取消</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs || saving" @click="savePool">保存</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, computed } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import {
  getEquipmentRerollSystemConfig,
  saveEquipmentRerollSystemConfig,
  getEquipmentEnhanceRules,
  saveEquipmentEnhanceRule,
  deleteEquipmentEnhanceRule,
  getEquipmentRerollCostRules,
  saveEquipmentRerollCostRule,
  deleteEquipmentRerollCostRule,
  getEquipmentRerollSlotPoolConfigs,
  saveEquipmentRerollSlotPoolConfig,
  deleteEquipmentRerollSlotPoolConfig,
  getEquipmentRerollTierConfigs,
  saveEquipmentRerollTierConfig,
  deleteEquipmentRerollTierConfig,
  getEquipmentRerollAttributeValueConfigs,
  saveEquipmentRerollAttributeValueConfig,
  deleteEquipmentRerollAttributeValueConfig
} from '@/services/equipment-reroll-rules'
import type {
  AdminEquipmentRerollSystemConfig,
  AdminEquipmentEnhanceRule,
  AdminEquipmentRerollCostRule,
  AdminEquipmentRerollSlotPoolConfig,
  AdminEquipmentRerollTierConfig,
  AdminEquipmentRerollAttributeValueConfig
} from '@/types/admin'
import toast from '@/utils/toast'

const { canManageConfigs } = useAdminPermissions()
const loading = ref(false)
const saving = ref(false)

const systemConfig = ref<AdminEquipmentRerollSystemConfig | null>(null)
const enhanceRules = ref<AdminEquipmentEnhanceRule[]>([])
const rerollCostRules = ref<AdminEquipmentRerollCostRule[]>([])
const poolConfigs = ref<AdminEquipmentRerollSlotPoolConfig[]>([])
const tierConfigs = ref<AdminEquipmentRerollTierConfig[]>([])
const attrValueConfigs = ref<AdminEquipmentRerollAttributeValueConfig[]>([])
const filterSlot = ref('')
const filterAttrType = ref('')
const filterTier = ref('')

const slotOptions = [
  { value: 0, label: '武器' },
  { value: 1, label: '头盔' },
  { value: 2, label: '护甲' },
  { value: 3, label: '裤子' },
  { value: 4, label: '鞋子' },
  { value: 5, label: '项链' },
  { value: 6, label: '戒指' }
]

const attributeOptions = [
  { value: 1, label: '最大血量' },
  { value: 2, label: '最大蓝量' },
  { value: 3, label: '物理攻击' },
  { value: 4, label: '法术攻击' },
  { value: 5, label: '物理防御' },
  { value: 6, label: '法术防御' },
  { value: 7, label: '速度' },
  { value: 8, label: '命中率' },
  { value: 9, label: '闪避率' },
  { value: 10, label: '暴击率' },
  { value: 11, label: '暴击伤害' },
  { value: 12, label: '连击率' },
  { value: 13, label: '反击率' },
  { value: 14, label: '破甲率' },
  { value: 15, label: '额外伤害' }
]

const filteredPoolConfigs = computed(() => {
  let list = poolConfigs.value
  if (filterSlot.value !== '') list = list.filter(p => p.slot === Number(filterSlot.value))
  return list
})

const filteredAttrValueConfigs = computed(() => {
  let list = attrValueConfigs.value
  if (filterAttrType.value !== '') list = list.filter(p => p.attributeType === Number(filterAttrType.value))
  if (filterTier.value !== '') list = list.filter(p => p.tier === Number(filterTier.value))
  return list
})

function getSlotName(slot: number) { return slotOptions.find(s => s.value === slot)?.label || `槽位${slot}` }
function getAttributeName(at: number) { return attributeOptions.find(a => a.value === at)?.label || `Type${at}` }
function getTierName(tier: number) { return tierConfigs.value.find(t => t.tier === tier)?.name || `品阶${tier}` }
function getTierColor(tier: number) { return tierConfigs.value.find(t => t.tier === tier)?.color || '#9ca3af' }

// System config editor
const systemEditorOpen = ref(false)
const systemEditor = ref<AdminEquipmentRerollSystemConfig | null>(null)
const enhanceEditorOpen = ref(false)
const enhanceEditor = ref<AdminEquipmentEnhanceRule | null>(null)
const rerollCostEditorOpen = ref(false)
const rerollCostEditor = ref<AdminEquipmentRerollCostRule | null>(null)

function openEnhanceCreate() {
  enhanceEditor.value = {
    gid: 0, minEquipmentLevel: 1, maxEquipmentLevel: 10, materialItemId: 'itm_enhance_s01',
    materialCount: 1, goldCost: 100, successRate: 100, attributeGrowthPercent: 10,
    maxEnhanceLevel: 15, sortOrder: 0, isEnabled: true, isBuiltIn: false
  }
  enhanceEditorOpen.value = true
}

function openEnhanceEdit(rule: AdminEquipmentEnhanceRule) {
  enhanceEditor.value = { ...rule }
  enhanceEditorOpen.value = true
}

async function saveEnhance() {
  if (!enhanceEditor.value) return
  saving.value = true
  try {
    await saveEquipmentEnhanceRule(enhanceEditor.value)
    toast.success('强化规则保存成功')
    enhanceEditorOpen.value = false
    await loadEnhanceRules()
  } catch (e: any) { toast.error(e.message || '保存失败') } finally { saving.value = false }
}

async function removeEnhance(rule: AdminEquipmentEnhanceRule) {
  if (!window.confirm(`确定删除装备等级 ${rule.minEquipmentLevel}-${rule.maxEquipmentLevel} 的强化规则？`)) return
  try {
    await deleteEquipmentEnhanceRule(rule.gid)
    toast.success('删除成功')
    await loadEnhanceRules()
  } catch (e: any) { toast.error(e.message || '删除失败') }
}

function openRerollCostCreate() {
  rerollCostEditor.value = {
    gid: 0, minEquipmentLevel: 1, maxEquipmentLevel: 10, materialItemId: 'itm_reroll_s01',
    materialCount: 1, goldCost: 150, extraMaterialPerLockedLine: 1, extraGoldPerLockedLine: 0,
    sortOrder: 0, isEnabled: true, isBuiltIn: false
  }
  rerollCostEditorOpen.value = true
}

function openRerollCostEdit(rule: AdminEquipmentRerollCostRule) {
  rerollCostEditor.value = { ...rule }
  rerollCostEditorOpen.value = true
}

async function saveRerollCost() {
  if (!rerollCostEditor.value) return
  saving.value = true
  try {
    await saveEquipmentRerollCostRule(rerollCostEditor.value)
    toast.success('洗炼消耗规则保存成功')
    rerollCostEditorOpen.value = false
    await loadRerollCostRules()
  } catch (e: any) { toast.error(e.message || '保存失败') } finally { saving.value = false }
}

async function removeRerollCost(rule: AdminEquipmentRerollCostRule) {
  if (!window.confirm(`确定删除装备等级 ${rule.minEquipmentLevel}-${rule.maxEquipmentLevel} 的洗炼规则？`)) return
  try {
    await deleteEquipmentRerollCostRule(rule.gid)
    toast.success('删除成功')
    await loadRerollCostRules()
  } catch (e: any) { toast.error(e.message || '删除失败') }
}

function openSystemConfigEdit() {
  if (!systemConfig.value) return
  systemEditor.value = { ...systemConfig.value }
  systemEditorOpen.value = true
}

async function saveSystemConfig() {
  if (!systemEditor.value) return
  saving.value = true
  try {
    await saveEquipmentRerollSystemConfig(systemEditor.value)
    toast.success('系统配置保存成功')
    systemEditorOpen.value = false
    await loadSystemConfig()
  } catch (e: any) {
    toast.error(e.message || '保存失败')
  } finally {
    saving.value = false
  }
}

// Pool editor
const poolEditorOpen = ref(false)
const poolEditor = ref<AdminEquipmentRerollSlotPoolConfig | null>(null)

function openPoolCreate() {
  poolEditor.value = { gid: 0, slot: 0, attributeType: 1, tier: 1, maxDuplicateCount: 1, sortOrder: 0, isEnabled: true, isBuiltIn: false, builtInVersion: null, seedKey: null, lastUpdateTime: null }
  poolEditorOpen.value = true
}

function openPoolEdit(pool: AdminEquipmentRerollSlotPoolConfig) {
  poolEditor.value = { ...pool }
  poolEditorOpen.value = true
}

async function savePool() {
  if (!poolEditor.value) return
  saving.value = true
  try {
    await saveEquipmentRerollSlotPoolConfig(poolEditor.value)
    toast.success('词条池配置保存成功')
    poolEditorOpen.value = false
    await loadPoolConfigs()
  } catch (e: any) {
    toast.error(e.message || '保存失败')
  } finally {
    saving.value = false
  }
}

async function removePool(pool: AdminEquipmentRerollSlotPoolConfig) {
  if (!window.confirm(`确定删除该词条池配置？`)) return
  try {
    await deleteEquipmentRerollSlotPoolConfig(pool.gid)
    toast.success('删除成功')
    await loadPoolConfigs()
  } catch (e: any) {
    toast.error(e.message || '删除失败')
  }
}

// Tier editor
const tierEditorOpen = ref(false)
const tierEditor = ref<AdminEquipmentRerollTierConfig | null>(null)

function openTierCreate() {
  tierEditor.value = { gid: 0, tier: 1, name: '', color: '#9ca3af', weight: 10, valueMultiplier: '1.00', sortOrder: 0, isEnabled: true, isBuiltIn: false, builtInVersion: null, seedKey: null, lastUpdateTime: null }
  tierEditorOpen.value = true
}

function openTierEdit(tier: AdminEquipmentRerollTierConfig) {
  tierEditor.value = { ...tier }
  tierEditorOpen.value = true
}

async function saveTier() {
  if (!tierEditor.value) return
  saving.value = true
  try {
    await saveEquipmentRerollTierConfig(tierEditor.value)
    toast.success('品阶配置保存成功')
    tierEditorOpen.value = false
    await loadTierConfigs()
  } catch (e: any) {
    toast.error(e.message || '保存失败')
  } finally {
    saving.value = false
  }
}

async function removeTier(tier: AdminEquipmentRerollTierConfig) {
  if (!window.confirm(`确定删除品阶"${tier.name}"？`)) return
  try {
    await deleteEquipmentRerollTierConfig(tier.gid)
    toast.success('删除成功')
    await loadTierConfigs()
  } catch (e: any) {
    toast.error(e.message || '删除失败')
  }
}

// AttrValue editor
const attrValueEditorOpen = ref(false)
const attrValueEditor = ref<AdminEquipmentRerollAttributeValueConfig | null>(null)

function openAttrValueCreate() {
  attrValueEditor.value = { gid: 0, attributeType: 1, tier: 1, minValue: '0', maxValue: '0', isPercentage: false, sortOrder: 0, isEnabled: true, isBuiltIn: false, builtInVersion: null, seedKey: null, lastUpdateTime: null }
  attrValueEditorOpen.value = true
}

function openAttrValueEdit(av: AdminEquipmentRerollAttributeValueConfig) {
  attrValueEditor.value = { ...av }
  attrValueEditorOpen.value = true
}

async function saveAttrValue() {
  if (!attrValueEditor.value) return
  saving.value = true
  try {
    await saveEquipmentRerollAttributeValueConfig(attrValueEditor.value)
    toast.success('属性值配置保存成功')
    attrValueEditorOpen.value = false
    await loadAttrValueConfigs()
  } catch (e: any) {
    toast.error(e.message || '保存失败')
  } finally {
    saving.value = false
  }
}

async function removeAttrValue(av: AdminEquipmentRerollAttributeValueConfig) {
  if (!window.confirm(`确定删除该属性值配置？`)) return
  try {
    await deleteEquipmentRerollAttributeValueConfig(av.gid)
    toast.success('删除成功')
    await loadAttrValueConfigs()
  } catch (e: any) {
    toast.error(e.message || '删除失败')
  }
}

// Data loading
async function loadSystemConfig() {
  try { systemConfig.value = await getEquipmentRerollSystemConfig() } catch { systemConfig.value = null }
}

async function loadEnhanceRules() {
  try { enhanceRules.value = await getEquipmentEnhanceRules() } catch { enhanceRules.value = [] }
}

async function loadRerollCostRules() {
  try { rerollCostRules.value = await getEquipmentRerollCostRules() } catch { rerollCostRules.value = [] }
}

async function loadPoolConfigs() {
  try { poolConfigs.value = await getEquipmentRerollSlotPoolConfigs() } catch { poolConfigs.value = [] }
}

async function loadTierConfigs() {
  try { tierConfigs.value = await getEquipmentRerollTierConfigs() } catch { tierConfigs.value = [] }
}

async function loadAttrValueConfigs() {
  try { attrValueConfigs.value = await getEquipmentRerollAttributeValueConfigs() } catch { attrValueConfigs.value = [] }
}

async function loadAll() {
  loading.value = true
  try {
    await Promise.all([loadSystemConfig(), loadEnhanceRules(), loadRerollCostRules(), loadPoolConfigs(), loadTierConfigs(), loadAttrValueConfigs()])
  } finally {
    loading.value = false
  }
}

onMounted(loadAll)
</script>

<style scoped>
.status-on { color: #22c55e; font-weight: 600; }
.status-off { color: #ef4444; font-weight: 600; }
.color-dot { display: inline-block; width: 12px; height: 12px; border-radius: 50%; vertical-align: middle; margin-right: 4px; }
.empty-state { color: var(--text-muted, #666); font-size: 0.85rem; padding: 16px 0; text-align: center; }
.section-header-actions { display: flex; gap: 8px; align-items: center; }
</style>
