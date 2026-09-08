<template>
  <div class="dashboard-grid">
    <section class="panel-box filter-panel">
      <div class="section-header-copy">
        <p class="section-kicker">元素克制</p>
        <h2 class="section-title section-title--small">元素克制矩阵</h2>
        <p class="section-note">维护八元素克制关系，保存后会直接刷新运行时战斗计算。</p>
      </div>

      <div class="filter-actions">
        <button class="secondary-button" type="button" @click="loadEntries">刷新</button>
        <button class="primary-button" type="button" :disabled="!canManageConfigs || loading" @click="saveMatrix">保存矩阵</button>
      </div>
    </section>

    <section class="panel-box table-panel">
      <div class="section-header-row">
        <div class="section-header-copy">
          <p class="section-kicker">克制矩阵</p>
          <h3 class="section-title section-title--small">克制矩阵</h3>
        </div>
        <span class="selected-pill">共 {{ matrixEntries.length }} 条</span>
      </div>

      <div class="table-wrap">
        <table class="data-table element-matrix-table">
          <thead>
            <tr>
              <th>攻击\防御</th>
              <th v-for="element in elementOptions" :key="element.value">{{ element.label }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="attacker in elementOptions" :key="attacker.value">
              <td>{{ attacker.label }}</td>
              <td v-for="defender in elementOptions" :key="`${attacker.value}-${defender.value}`">
                <input
                  v-model.number="matrixMap[`${attacker.value}-${defender.value}`].modifier"
                  type="number"
                  step="0.01"
                  :disabled="!canManageConfigs"
                />
              </td>
            </tr>
          </tbody>
        </table>
      </div>

    </section>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { useAdminPermissions } from '@/composables/useAdminPermissions'
import { getAdminElementRelations, saveAdminElementRelations } from '@/services/element-rules'
import type { AdminElementRelationEntry } from '@/types/admin'
import toast from '@/utils/toast'

// 页面主状态：
// entries 是后端原始矩阵条目，matrixMap 是前端编辑时按“攻击-防御”索引的网格结构。
const { canManageConfigs } = useAdminPermissions()
const loading = ref(false)
const entries = ref<AdminElementRelationEntry[]>([])

const elementOptions = [
  { value: 1, label: '金' },
  { value: 2, label: '木' },
  { value: 3, label: '水' },
  { value: 4, label: '火' },
  { value: 5, label: '土' },
  { value: 6, label: '风' },
  { value: 7, label: '冰' },
  { value: 8, label: '雷' }
]

const matrixMap = reactive<Record<string, AdminElementRelationEntry>>({})

const matrixEntries = computed(() =>
  Object.values(matrixMap).sort((a, b) => a.sortOrder - b.sortOrder)
)

// 构造一条默认矩阵规则。
function buildDefaultEntry(attacker: number, defender: number, sortOrder: number): AdminElementRelationEntry {
  return {
    gid: `${attacker}_${defender}`,
    attackerElement: attacker,
    defenderElement: defender,
    modifier: 0,
    sortOrder,
    isEnabled: true,
    isBuiltIn: false,
    seedKey: null,
    builtInVersion: null,
    lastUpdateTime: null
  }
}

// 把后端条目折叠成前端可直接编辑的八元素矩阵。
function hydrateMatrix(items: AdminElementRelationEntry[]) {
  const entryMap = new Map(items.map((item) => [`${item.attackerElement}-${item.defenderElement}`, item]))

  for (const key of Object.keys(matrixMap)) {
    delete matrixMap[key]
  }

  let sortOrder = 1
  for (const attacker of elementOptions) {
    for (const defender of elementOptions) {
      const key = `${attacker.value}-${defender.value}`
      const existing = entryMap.get(key)
      matrixMap[key] = existing
        ? { ...existing }
        : buildDefaultEntry(attacker.value, defender.value, sortOrder)
      sortOrder++
    }
  }
}

// 拉取元素克制矩阵。
async function loadEntries() {
  try {
    loading.value = true
    entries.value = await getAdminElementRelations()
    hydrateMatrix(entries.value)
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '元素克制矩阵加载失败。')
  } finally {
    loading.value = false
  }
}

// 保存整个元素克制矩阵。
async function saveMatrix() {
  if (!canManageConfigs.value) return

  try {
    loading.value = true
    const payload = matrixEntries.value.map((item) => ({
      ...item,
      modifier: Number(item.modifier) || 0
    }))
    entries.value = await saveAdminElementRelations(payload)
    hydrateMatrix(entries.value)
    toast.success('元素克制矩阵保存成功。')
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '元素克制矩阵保存失败。')
  } finally {
    loading.value = false
  }
}

hydrateMatrix([])
onMounted(loadEntries)
</script>
