<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">道具</p>
        <h2 class="section-title section-title--small">道具管理</h2>
        <p class="section-note">按道具类型维护使用行为，道具箱、技能书、丹药、种子和宠物蛋都在这里配置。</p>
      </div>

      <div class="filter-grid">
        <label class="field">
          <span>关键字</span>
          <input v-model.trim="keyword" type="text" placeholder="搜索道具编号或名称" />
        </label>
        <label class="field">
          <span>当前选中</span>
          <input :value="selectedItem?.itemId || ''" type="text" placeholder="未选择" disabled />
        </label>
        <label class="field">
          <span>道具类型</span>
          <select v-model="typeFilter">
            <option value="">全部</option>
            <option v-for="option in ITEM_TYPE_OPTIONS" :key="option.value" :value="option.value">{{ option.label }}</option>
          </select>
        </label>

        <div class="filter-actions">
          <button class="secondary-button" type="button" @click="loadAll">查询</button>
          <button class="primary-button" type="button" @click="openCreateModal">新建道具</button>
        </div>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">道具列表</p>
          <h3 class="section-title section-title--small">道具列表</h3>
        </div>
        <span class="selected-pill">共 {{ filteredItems.length }} 条</span>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>图标</th>
              <th>道具编号</th>
              <th>名称</th>
              <th>类型</th>
              <th>使用等级</th>
              <th>来源</th>
              <th>品质</th>
              <th>可交易</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="item in filteredItems"
              :key="item.itemId"
              :class="{ 'is-active': selectedItem?.itemId === item.itemId }"
              @click="openEditModal(item.itemId)"
            >
              <td>
                <img v-if="item.iconPath" :src="buildAdminApiUrl(item.iconPath)" class="list-icon" alt="" />
                <span v-else class="list-icon-placeholder">-</span>
              </td>
              <td>{{ item.itemId }}</td>
              <td>{{ item.name }}</td>
              <td>{{ getItemTypeLabel(item.type) }}</td>
              <td>Lv.{{ item.useLevel }}</td>
              <td>{{ item.isBuiltIn ? '内置' : '人工' }}</td>
              <td>{{ item.quality }}</td>
              <td>
                <input type="checkbox" :checked="item.isTradeable" @click.stop @change="toggleItemTradeable(item, $event)" />
              </td>
            </tr>
          </tbody>
        </table>
      </div>

    </section>

    <AdminModal
      v-model="editorOpen"
      kicker="道具编辑"
      :title="selectedItem?.itemId ? `编辑道具 ${selectedItem.itemId}` : '新建道具'"
      description="基础信息和类型行为统一在一个弹窗里维护。"
      size="wide"
    >
      <div v-if="selectedItem" class="section-stack">
        <div class="detail-overview">
          <article class="detail-overview__item"><span>来源</span><strong>{{ selectedItem.isBuiltIn ? '内置' : '人工维护' }}</strong></article>
          <article class="detail-overview__item"><span>种子键</span><strong>{{ selectedItem.seedKey || '-' }}</strong></article>
          <article class="detail-overview__item"><span>内置版本</span><strong>{{ selectedItem.builtInVersion || '-' }}</strong></article>
          <article class="detail-overview__item"><span>最后更新</span><strong>{{ formatDateTime(selectedItem.lastUpdateTime) }}</strong></article>
        </div>
        <div class="editor-grid">
          <label class="field"><span>道具编号</span><input v-model.trim="selectedItem.itemId" type="text" /></label>
          <label class="field"><span>道具名称</span><input v-model.trim="selectedItem.name" type="text" /></label>
          <label class="field"><span>使用等级</span><input v-model.number="selectedItem.useLevel" type="number" min="0" /></label>
          <label class="field"><span>类型</span><select v-model.number="selectedItem.type"><option v-for="option in ITEM_TYPE_OPTIONS" :key="option.value" :value="option.value">{{ option.label }}</option></select></label>
          <label class="field"><span>最大堆叠</span><input v-model.number="selectedItem.maxStack" type="number" min="1" /></label>
          <label class="field"><span>品质</span><input v-model.number="selectedItem.quality" type="number" min="1" /></label>
          <label class="field checkbox-field"><input v-model="selectedItem.isTradeable" type="checkbox" /><span>可交易</span></label>
        </div>

        <AdminImageField
          v-model="selectedItem.iconPath"
          label="道具图片"
          upload-category="items"
          placeholder="未上传时保持为空"
        />

        <section v-if="selectedItem.type === 10 && selectedItem.chestConfig" class="panel-box section-stack">
          <div class="section-header-copy">
            <p class="section-kicker">道具箱</p>
            <h3 class="section-title section-title--small">道具箱配置</h3>
          </div>
          <div class="editor-grid">
            <label class="field">
              <span>开启模式</span>
              <select v-model.number="selectedItem.chestConfig.openMode">
                <option v-for="option in ITEM_CHEST_OPEN_MODE_OPTIONS" :key="option.value" :value="option.value">{{ option.label }}</option>
              </select>
            </label>
            <label class="field">
              <span>每次抽取次数</span>
              <input v-model.number="selectedItem.chestConfig.rollCount" type="number" min="1" />
            </label>
          </div>

          <div class="section-stack">
              <div class="section-header-row">
                <div class="section-header-copy">
                <p class="section-kicker">奖励池</p>
                <h4 class="section-title section-title--small">奖励池</h4>
                </div>
              <button class="secondary-button" type="button" @click="addChestReward">新增奖励</button>
            </div>

            <div
              v-for="(reward, index) in selectedItem.chestConfig.rewards"
              :key="`reward-${index}`"
              class="panel-box section-stack"
            >
              <div class="editor-grid editor-grid--three">
                <label class="field">
                  <span>奖励类型</span>
                  <select v-model.number="reward.rewardType">
                    <option v-for="option in ITEM_CHEST_REWARD_TYPE_OPTIONS" :key="option.value" :value="option.value">{{ option.label }}</option>
                  </select>
                </label>

                <label v-if="reward.rewardType === 3" class="field">
                  <span>道具</span>
                  <select v-model="reward.targetId">
                    <option value="">请选择道具</option>
                    <option v-for="option in itemOptions" :key="option.value" :value="option.value">{{ option.label }}</option>
                  </select>
                </label>

                <label v-else-if="reward.rewardType === 4" class="field">
                  <span>装备</span>
                  <select v-model="reward.targetId">
                    <option value="">请选择装备</option>
                    <option v-for="option in equipmentOptions" :key="option.value" :value="option.value">{{ option.label }}</option>
                  </select>
                </label>

                <label v-else class="field">
                  <span>目标</span>
                  <input value="无需目标编号" type="text" disabled />
                </label>

                <label class="field"><span>最小数量</span><input v-model.number="reward.minCount" type="number" min="1" /></label>
                <label class="field"><span>最大数量</span><input v-model.number="reward.maxCount" type="number" min="1" /></label>
                <label class="field"><span>权重</span><input v-model.number="reward.weight" type="number" min="1" /></label>
              </div>

              <div class="editor-grid">
                
                <label class="field">
                  <span>备注</span>
                  <input v-model.trim="reward.description" type="text" placeholder="留空时自动生成说明" />
                </label>
              </div>

              <div class="editor-actions">
                <button class="danger-button" type="button" @click="removeChestReward(index)">删除该奖励</button>
              </div>
            </div>
          </div>
        </section>

        <section v-if="selectedItem.type === 11 && selectedItem.skillBookConfig" class="panel-box section-stack">
          <div class="section-header-copy">
            <p class="section-kicker">技能书</p>
            <h3 class="section-title section-title--small">技能书配置</h3>
          </div>
          <label class="field">
            <span>关联技能</span>
            <select v-model.number="selectedItem.skillBookConfig.skillId">
              <option value="0">请选择技能</option>
              <option v-for="option in skillOptions" :key="option.value" :value="option.value">{{ option.label }}</option>
            </select>
          </label>
        </section>

        <section v-if="selectedItem.type === 14 && selectedItem.petEggConfig" class="panel-box section-stack">
          <div class="section-header-copy">
            <p class="section-kicker">宠物蛋</p>
            <h3 class="section-title section-title--small">宠物蛋配置</h3>
          </div>
          <label class="field">
            <span>灵宠模板</span>
            <select v-model="selectedItem.petEggConfig.petTemplateId">
              <option value="">请选择灵宠模板</option>
              <option v-for="option in petOptions" :key="option.value" :value="option.value">{{ option.label }}</option>
            </select>
          </label>
        </section>

        <section v-if="selectedItem.type === 12 && selectedItem.pillConfig" class="panel-box section-stack">
          <div class="section-header-copy">
            <p class="section-kicker">丹药</p>
            <h3 class="section-title section-title--small">丹药配置</h3>
          </div>
          <div class="editor-grid">
            <label class="field">
              <span>效果类型</span>
              <select v-model.number="selectedItem.pillConfig.effectType">
                <option v-for="option in ITEM_PILL_EFFECT_OPTIONS" :key="option.value" :value="option.value">{{ option.label }}</option>
              </select>
            </label>

            <label v-if="selectedItem.pillConfig.effectType === 1" class="field">
              <span>突破加成百分比</span>
              <input v-model.number="selectedItem.pillConfig.breakthroughBonusPercent" type="number" min="0" />
            </label>

            <label v-if="selectedItem.pillConfig.effectType === 2" class="field">
              <span>增加经验</span>
              <input v-model.number="selectedItem.pillConfig.expGain" type="number" min="0" />
            </label>

            <template v-if="selectedItem.pillConfig.effectType === 3">
              <label class="field">
                <span>属性类型</span>
                <select v-model="selectedItem.pillConfig.attributeType">
                  <option value="">请选择属性</option>
                  <option v-for="option in ITEM_PILL_ATTRIBUTE_OPTIONS" :key="option.value" :value="option.value">{{ option.label }}</option>
                </select>
              </label>
              <label class="field">
                <span>属性数值</span>
                <input v-model.number="selectedItem.pillConfig.attributeValue" type="number" min="0" step="0.01" />
              </label>
            </template>

            <template v-if="selectedItem.pillConfig.effectType === 4">
              <label class="field">
                <span>恢复HP百分比</span>
                <input v-model.number="selectedItem.pillConfig.healHpPercent" type="number" min="0" max="100" />
                <small class="field-hint">按最大HP的百分比恢复，0=不恢复</small>
              </label>
              <label class="field">
                <span>恢复MP百分比</span>
                <input v-model.number="selectedItem.pillConfig.healMpPercent" type="number" min="0" max="100" />
                <small class="field-hint">按最大MP的百分比恢复，0=不恢复</small>
              </label>
            </template>

            <!-- 时效分钟：仅增加经验、恢复HP/MP类型不显示 -->
            <label v-if="selectedItem.pillConfig.effectType !== 2 && selectedItem.pillConfig.effectType !== 4" class="field">
              <span>时效分钟</span>
              <input
                v-model.number="selectedItem.pillConfig.durationMinutes"
                type="number"
                min="0"
                placeholder="0 = 永久生效"
              />
              <small v-if="selectedItem.pillConfig.durationMinutes > 0" class="field-hint">
                使用后持续 {{ selectedItem.pillConfig.durationMinutes }} 分钟
              </small>
              <small v-else class="field-hint">0 = 永久生效</small>
            </label>

            <!-- 使用次数：所有效果类型都显示 -->
            <label class="field">
              <span>使用次数</span>
              <input
                v-model.number="selectedItem.pillConfig.maxUsageCount"
                type="number"
                min="0"
                placeholder="0 = 不限制"
              />
              <small v-if="selectedItem.pillConfig.maxUsageCount > 0" class="field-hint">
                每个玩家最多使用 {{ selectedItem.pillConfig.maxUsageCount }} 次
              </small>
              <small v-else class="field-hint">0 = 不限制使用次数</small>
            </label>
          </div>
        </section>

        <section v-if="selectedItem.type === 17 && selectedItem.recipeUnlockConfig" class="panel-box section-stack">
          <div class="section-header-copy">
            <p class="section-kicker">配方卷轴</p>
            <h3 class="section-title section-title--small">配方解锁配置</h3>
          </div>
          <div class="editor-grid">
            <label class="field">
              <span>配方类型</span>
              <select v-model="selectedItem.recipeUnlockConfig.recipeType">
                <option value="">请选择</option>
                <option value="Alchemy">炼丹</option>
                <option value="Forge">锻造</option>
              </select>
            </label>
            <label class="field">
              <span>配方编号</span>
              <input v-model.trim="selectedItem.recipeUnlockConfig.recipeId" type="text" placeholder="例如 alchemy_baseline_001" />
            </label>
          </div>
          <small class="field-hint">保存时会校验配方存在且一条配方只能绑定一张有效卷轴。</small>
        </section>

        <section v-if="selectedItem.type === 15 && selectedItem.favorabilityGiftConfig" class="panel-box section-stack">
          <div class="section-header-copy">
            <p class="section-kicker">好感礼物</p>
            <h3 class="section-title section-title--small">好感礼物配置</h3>
          </div>
          <div class="editor-grid">
            <label class="field">
              <span>好感度值</span>
              <input v-model.number="selectedItem.favorabilityGiftConfig.favorabilityValue" type="number" placeholder="正数增加，负数减少" />
              <small class="field-hint">赠送后增减的好感度数值，正数增加，负数减少</small>
            </label>
            <label class="field">
              <span>每日限制</span>
              <input v-model.number="selectedItem.favorabilityGiftConfig.dailyLimit" type="number" min="0" />
              <small class="field-hint">每个玩家每天对该道具的赠送次数上限</small>
            </label>
          </div>
          <label class="field checkbox-field">
            <input v-model="selectedItem.favorabilityGiftConfig.canGift" type="checkbox" />
            <span>可赠送</span>
          </label>
        </section>

        <label class="field">
          <span>描述</span>
          <textarea v-model="selectedItem.description" rows="5"></textarea>
        </label>
      </div>

      <template #footer>
        <button class="secondary-button" type="button" @click="editorOpen = false">取消</button>
        <button class="danger-button" type="button" :disabled="!selectedItem?.itemId" @click="removeItem">删除</button>
        <button class="primary-button" type="button" @click="saveItem">保存道具</button>
      </template>
    </AdminModal>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import AdminModal from '@/components/AdminModal.vue'
import AdminImageField from '@/components/AdminImageField.vue'
import { buildAdminApiUrl } from '@/services/http'
import {
  ITEM_CHEST_OPEN_MODE_OPTIONS,
  ITEM_CHEST_REWARD_TYPE_OPTIONS,
  ITEM_PILL_ATTRIBUTE_OPTIONS,
  ITEM_PILL_EFFECT_OPTIONS,
  ITEM_TYPE_OPTIONS
} from '@/constants/game-options'
import type {
  AdminEquipmentListItem,
  AdminItemChestReward,
  AdminItemDetail,
  AdminItemListItem,
  AdminPetListItem,
  AdminSkillListItem
} from '@/types/admin'
import { deleteAdminItem, getAdminItemDetail, getAdminItems, saveAdminItem } from '@/services/items'
import { getAdminSkills } from '@/services/skills-buffs'
import { getAdminPets } from '@/services/pets'
import { getAdminEquipments } from '@/services/equipments'
import toast from '@/utils/toast'

// 道具页把“列表摘要”和“当前编辑详情”分开维护，
// 避免列表切换时直接污染还没保存的编辑对象。
const items = ref<AdminItemListItem[]>([])
const keyword = ref('')
const typeFilter = ref<number | ''>('')
const selectedItem = ref<AdminItemDetail | null>(null)
const editorOpen = ref(false)

// 后端已支持按类型筛选，filteredItems 直接引用 items。
const filteredItems = items

const skills = ref<AdminSkillListItem[]>([])
const pets = ref<AdminPetListItem[]>([])
const equipments = ref<AdminEquipmentListItem[]>([])

// 统一格式化后台时间字段。
function formatDateTime(value?: string | null) {
  if (!value) return '-'
  return value.replace('T', ' ').slice(0, 19)
}

// 新建道具时使用的默认对象。
// 不同类型的扩展配置会在后续 watch 里按类型自动补齐。
function createEmptyItem(): AdminItemDetail {
  return {
    itemId: '',
    name: '',
    useLevel: 0,
    type: 0,
    description: '',
    maxStack: 1,
    quality: 1,
    iconPath: '',
    isBuiltIn: false,
    seedKey: null,
    builtInVersion: null,
    lastUpdateTime: null,
    chestConfig: null,
    skillBookConfig: null,
    petEggConfig: null,
    pillConfig: null,
    recipeUnlockConfig: null,
    favorabilityGiftConfig: null
  }
}

function createDefaultChestReward(): AdminItemChestReward {
  return {
    rewardType: 3,
    targetId: '',
    minCount: 1,
    maxCount: 1,
    weight: 100,
    description: ''
  }
}

function ensureItemTypeConfig(item: AdminItemDetail | null) {
  if (!item) return

  if (item.type === 10) {
    item.chestConfig ??= { openMode: 1, rollCount: 1, rewards: [createDefaultChestReward()] }
  }

  if (item.type === 11) {
    item.skillBookConfig ??= { skillId: 0 }
  }

  if (item.type === 12) {
    item.pillConfig ??= {
      effectType: 2,
      breakthroughBonusPercent: 0,
      expGain: 0,
      attributeType: '',
      attributeValue: 0,
      durationMinutes: 0,
      maxUsageCount: 0
    }
  }

  if (item.type === 14) {
    item.petEggConfig ??= { petTemplateId: '' }
  }

  if (item.type === 15) {
    item.favorabilityGiftConfig ??= { favorabilityValue: 1, dailyLimit: 10, canGift: true }
  }

  if (item.type === 17) {
    item.recipeUnlockConfig ??= { recipeType: '', recipeId: '' }
  }
}

watch(
  () => selectedItem.value?.type,
  () => ensureItemTypeConfig(selectedItem.value)
)

const itemOptions = computed(() =>
  items.value.map((item) => ({
    value: item.itemId,
    label: `${item.itemId} · ${item.name}`
  }))
)

const skillOptions = computed(() =>
  skills.value.map((skill) => ({
    value: skill.skillId,
    label: `${skill.skillId} · ${skill.name}`
  }))
)

const petOptions = computed(() =>
  pets.value.map((pet) => ({
    value: pet.templateId,
    label: `${pet.templateId} · ${pet.name}`
  }))
)

const equipmentOptions = computed(() =>
  equipments.value.map((equipment) => ({
    value: String(equipment.equipmentId),
    label: `${equipment.equipmentId} · ${equipment.name}`
  }))
)

function getItemTypeLabel(value: number) {
  return ITEM_TYPE_OPTIONS.find((option) => option.value === value)?.label || `类型 ${value}`
}

function openCreateModal() {
  selectedItem.value = createEmptyItem()
  ensureItemTypeConfig(selectedItem.value)
  editorOpen.value = true
}

async function loadAll() {
  const filterType = typeFilter.value === '' ? null : Number(typeFilter.value)
  const [itemList, skillList, petList, equipmentList] = await Promise.all([
    getAdminItems(keyword.value, filterType),
    getAdminSkills(),
    getAdminPets(),
    getAdminEquipments()
  ])

  items.value = itemList
  skills.value = skillList
  pets.value = petList
  equipments.value = equipmentList
}

async function openEditModal(itemId: string) {
  selectedItem.value = await getAdminItemDetail(itemId)
  ensureItemTypeConfig(selectedItem.value)
  editorOpen.value = true
}

function addChestReward() {
  selectedItem.value?.chestConfig?.rewards.push(createDefaultChestReward())
}

function removeChestReward(index: number) {
  selectedItem.value?.chestConfig?.rewards.splice(index, 1)
}

async function toggleItemTradeable(item: AdminItemListItem, event: Event) {
  const checked = (event.target as HTMLInputElement).checked
  try {
    const detail = await getAdminItemDetail(item.itemId)
    detail.isTradeable = checked
    await saveAdminItem(detail)
    item.isTradeable = checked
    toast.success(`道具 ${item.name} 可交易状态已更新。`)
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '更新失败。')
    ;(event.target as HTMLInputElement).checked = !checked
  }
}

async function saveItem() {
  if (!selectedItem.value) return

  try {
    selectedItem.value = await saveAdminItem(selectedItem.value)
    ensureItemTypeConfig(selectedItem.value)
    toast.success('道具模板保存成功。')
    editorOpen.value = false
    await loadAll()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '道具模板保存失败。')
  }
}

async function removeItem() {
  if (!selectedItem.value?.itemId) return

  try {
    await deleteAdminItem(selectedItem.value.itemId)
    toast.success('道具模板删除成功。')
    selectedItem.value = null
    editorOpen.value = false
    await loadAll()
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '道具模板删除失败。')
  }
}

onMounted(loadAll)
</script>
