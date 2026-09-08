<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">聚灵阵</p>
        <h2 class="section-title section-title--small">聚灵阵监控</h2>
        <p class="section-note">查看玩家聚灵阵等级、五行分布与当前加成，必要时可直接修正数据。</p>
      </div>

      <div class="filter-grid">
        <label class="field">
          <span>关键字</span>
          <input v-model.trim="keyword" type="text" placeholder="搜索玩家编号、账号或名称" />
        </label>
        <label class="field">
          <span>当前玩家</span>
          <input :value="selectedDetail?.playerId || ''" type="text" placeholder="未选择" disabled />
        </label>
        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadItems">查询</button>
        </div>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">聚灵阵列表</p>
          <h3 class="section-title section-title--small">玩家聚灵阵</h3>
        </div>
        <span class="selected-pill">共 {{ items.length }} 条</span>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>玩家</th>
              <th>账号</th>
              <th>阵等级</th>
              <th>五行</th>
              <th>职业上限</th>
              <th>灵田加成</th>
              <th>战斗经验</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="item in items"
              :key="item.playerId"
              :class="{ 'is-active': selectedDetail?.playerId === item.playerId }"
              @click="openDetail(item.playerId)"
            >
              <td>{{ item.name }}</td>
              <td>{{ item.account }}</td>
              <td>Lv.{{ item.arrayLevel }}</td>
              <td>金{{ item.metalLevel }} 木{{ item.woodLevel }} 水{{ item.waterLevel }} 火{{ item.fireLevel }} 土{{ item.earthLevel }}</td>
              <td>Lv.{{ item.professionLevelCap }}</td>
              <td>+{{ item.spiritFieldYieldBonusPercent }}%</td>
              <td>+{{ item.battleExpBonusPercent }}%</td>
            </tr>
          </tbody>
        </table>
      </div>

    </section>

    <AdminModal
      v-model="editorOpen"
      kicker="聚灵阵"
      :title="selectedDetail ? `编辑 ${selectedDetail.name} 的聚灵阵` : '聚灵阵详情'"
      description="等级和经验可直接修正，加成和职业上限按当前阵等级自动推导。"
      size="xwide"
    >
      <div v-if="selectedDetail" class="section-stack section-stack--spaced">
        <div class="detail-overview detail-overview--compact">
          <!-- 显示后端真实的五行等级上限与属性加成；玩家详情保存后接口会重新返回归一化后的结果。 -->
          <article class="detail-overview__item"><span>玩家</span><strong>{{ selectedDetail.name }}</strong></article>
          <article class="detail-overview__item"><span>账号</span><strong>{{ selectedDetail.account }}</strong></article>
          <article class="detail-overview__item"><span>职业上限</span><strong>Lv.{{ selectedDetail.professionLevelCap }}</strong></article>
          <article class="detail-overview__item"><span>灵田加成</span><strong>+{{ selectedDetail.spiritFieldYieldBonusPercent }}%</strong></article>
          <article class="detail-overview__item"><span>战斗经验</span><strong>+{{ selectedDetail.battleExpBonusPercent }}%</strong></article>
          <article class="detail-overview__item"><span>五行等级上限</span><strong>Lv.{{ selectedDetail.maxElementLevel }}</strong></article>
        </div>

          <div v-if="hasElementLevelTruncation" class="form-warning">
            当前聚灵阵等级只能支持五行最高 Lv.{{ maxElementLevel }}，保存后超出等级会被后端截断。
          </div>

          <div class="five-element-bonus-list">
            <div v-for="bonus in selectedDetail.elementBonuses" :key="bonus.elementType" class="five-element-bonus-item">
              {{ bonus.elementName }}：Lv.{{ bonus.level }} / Lv.{{ bonus.maxLevel }}，{{ bonus.attributeName }} +{{ bonus.currentBonus }}（每级 +{{ bonus.bonusPerLevel }}）
            </div>
          </div>

        <fieldset class="form-fieldset" :disabled="!canWritePlayers">
          <div class="editor-grid editor-grid--three">
            <label class="field"><span>聚灵阵等级</span><input v-model.number="selectedDetail.arrayLevel" type="number" min="1" max="50" /></label>
            <label class="field"><span>金等级</span><input v-model.number="selectedDetail.metalLevel" type="number" min="1" :max="maxElementLevel" /></label>
            <label class="field"><span>木等级</span><input v-model.number="selectedDetail.woodLevel" type="number" min="1" :max="maxElementLevel" /></label>
            <label class="field"><span>水等级</span><input v-model.number="selectedDetail.waterLevel" type="number" min="1" :max="maxElementLevel" /></label>
            <label class="field"><span>火等级</span><input v-model.number="selectedDetail.fireLevel" type="number" min="1" :max="maxElementLevel" /></label>
            <label class="field"><span>土等级</span><input v-model.number="selectedDetail.earthLevel" type="number" min="1" :max="maxElementLevel" /></label>
            <label class="field"><span>金经验</span><input v-model.number="selectedDetail.metalExp" type="number" min="0" /></label>
            <label class="field"><span>木经验</span><input v-model.number="selectedDetail.woodExp" type="number" min="0" /></label>
            <label class="field"><span>水经验</span><input v-model.number="selectedDetail.waterExp" type="number" min="0" /></label>
            <label class="field"><span>火经验</span><input v-model.number="selectedDetail.fireExp" type="number" min="0" /></label>
            <label class="field"><span>土经验</span><input v-model.number="selectedDetail.earthExp" type="number" min="0" /></label>
          </div>
          <label class="field"><span>激活组合</span><textarea v-model="activeCombinationsText" rows="3" placeholder="每行一个组合名称"></textarea></label>
        </fieldset>
      </div>

      <template #footer>
        <button class="secondary-button" type="button" @click="editorOpen = false">取消</button>
        <button class="primary-button" type="button" :disabled="!canWritePlayers" @click="saveDetail">保存聚灵阵</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import { getAdminFiveElementDetail, getAdminFiveElements, saveAdminFiveElement } from '@/services/five-elements'
import type { AdminFiveElementDetail, AdminFiveElementListItem } from '@/types/admin'
import toast from '@/utils/toast'

// 聚灵阵运行态页只维护玩家实例数据，不涉及规则模板。
const { canWritePlayers } = useAdminPermissions()
const keyword = ref('')
const items = ref<AdminFiveElementListItem[]>([])
const selectedDetail = ref<AdminFiveElementDetail | null>(null)
const activeCombinationsText = ref('')
const editorOpen = ref(false)
// 管理端输入框的动态上限，跟随当前聚灵阵等级实时变化。
const maxElementLevel = computed(() => Math.min(500, Math.max(1, Number(selectedDetail.value?.arrayLevel || 1)) * 10))
// 聚灵阵降级后，检测现有五行等级是否会被后端截断，保存前明确提示管理员。
const hasElementLevelTruncation = computed(() => {
  if (!selectedDetail.value) return false
  return [selectedDetail.value.metalLevel, selectedDetail.value.woodLevel, selectedDetail.value.waterLevel, selectedDetail.value.fireLevel, selectedDetail.value.earthLevel]
    .some((level) => Number(level) > maxElementLevel.value)
})


// 拉取聚灵阵运行态列表。
async function loadItems() {
  items.value = await getAdminFiveElements(keyword.value)
}

// 打开指定玩家的聚灵阵详情。
async function openDetail(playerId: string) {
  selectedDetail.value = await getAdminFiveElementDetail(playerId)
  activeCombinationsText.value = (selectedDetail.value.activeCombinations || []).join('\n')
  editorOpen.value = true
}

// 保存聚灵阵运行态。
async function saveDetail() {
  if (!canWritePlayers.value || !selectedDetail.value) return
  selectedDetail.value.activeCombinations = activeCombinationsText.value
    .split('\n')
    .map((item) => item.trim())
    .filter(Boolean)

  if (hasElementLevelTruncation.value && !window.confirm(`当前聚灵阵等级的五行上限为 Lv.${maxElementLevel.value}，超出等级保存后会被后端截断，确认继续吗？`)) return

  try {
    selectedDetail.value = await saveAdminFiveElement(selectedDetail.value)
    activeCombinationsText.value = (selectedDetail.value.activeCombinations || []).join('\n')
    toast.success('聚灵阵数据保存成功。')
    editorOpen.value = false
    await loadItems()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '聚灵阵数据保存失败。')
  }
}

onMounted(loadItems)
</script>
