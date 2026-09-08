<template>
  <XiuXianModal :model-value="modelValue" @update:model-value="$emit('update:modelValue', $event)" title="图鉴抽奖" :width="640">
    <div class="lottery-modal">
      <!-- Tab 切换 -->
      <div class="lottery-tabs">
        <button v-for="tab in tabs" :key="tab.key" class="lottery-tab-btn" :class="{ active: activeTab === tab.key }" @click="switchTab(tab.key)">
          <span class="tab-icon"><AssetIcon :source="tab.icon" size="20" /></span>
          <span class="tab-label">{{ tab.label }}</span>
        </button>
      </div>

      <div v-if="loading" class="lottery-loading">
        <span class="loading-spinner"></span>
        <span>加载中...</span>
      </div>

      <div v-else-if="currentPool" class="lottery-content">
        <!-- 池子信息 -->
        <div class="pool-header">
          <div class="pool-name">{{ currentPool.name }}</div>
          <div class="pool-cost">
            <span class="cost-label">消耗</span>
            <span class="cost-amount">{{ currentPool.costAmount }}</span>
            <span class="cost-type">{{ costLabel }}</span>
          </div>
          <div class="pool-limits">
            <span v-if="currentPool.dailyRemaining >= 0" class="limit-item">今日剩余 {{ currentPool.dailyRemaining }} 次</span>
            <span v-if="currentPool.totalRemaining >= 0" class="limit-item">总剩余 {{ currentPool.totalRemaining }} 次</span>
          </div>
        </div>

        <!-- 转盘区域 -->
        <div class="wheel-section">
          <div v-if="spinning && !showResult" class="draw-loading">
            <span class="draw-loading-spinner"></span>
            <span class="draw-loading-text">抽取中...</span>
          </div>
          <div class="wheel-container">
            <!-- 指针 -->
            <div class="wheel-pointer">&#9660;</div>
            <!-- 转盘 -->
            <div class="wheel-outer">
              <div class="wheel-disc" :style="{ transform: `rotate(${wheelRotation}deg)` }" @transitionend="onWheelStop">
                <!-- 扇区由 conic-gradient + emoji 标记点实现 -->
                <div v-for="(seg, i) in segments" :key="i" class="wheel-marker" :style="markerStyle(i)">
                  <span class="marker-emoji"><AssetIcon :source="seg.icon" size="20" /></span>
                </div>
              </div>
              <!-- 中心按钮 -->
              <button class="wheel-center-btn" :disabled="!canDraw || spinning" @click="doSingleDraw">
                {{ spinning ? '...' : '抽奖' }}
              </button>
            </div>
          </div>
        </div>

        <!-- 操作按钮 -->
        <div class="draw-actions">
          <button class="action-btn single" :disabled="!canDraw || spinning" @click="doSingleDraw">
            <span class="btn-label">单抽</span>
            <span class="btn-cost">{{ currentPool.costAmount }} {{ costLabel }}</span>
          </button>
          <button v-if="currentPool.supportTen" class="action-btn ten" :disabled="!canDrawTen || spinning" @click="doTenDraw">
            <span class="btn-label">十连抽</span>
            <span class="btn-cost">{{ currentPool.costAmount * 10 }} {{ costLabel }}</span>
          </button>
        </div>
      </div>

      <div v-else class="lottery-empty">暂无可用的抽奖池</div>

      <p v-if="error" class="lottery-error">{{ error }}</p>
    </div>

    <!-- 结果弹窗 -->
    <Transition name="fade">
      <div v-if="showResult" class="result-overlay" @click.self="closeResult">
        <div class="result-card">
          <!-- 单抽结果 -->
          <template v-if="resultMode === 'single'">
            <div class="result-title">抽奖结果</div>
            <div class="result-body">
              <div v-for="(item, idx) in resultRewards" :key="idx" class="reward-line" :class="rewardClass(item)">
                <img v-if="item.rewardType === 'image_collection' && item.thumbUrl" :src="buildApiUrl(item.thumbUrl)" class="reward-thumb" />
                <div class="reward-info">
                  <div class="reward-main">
                    <span class="reward-name" :class="rewardClass(item)">{{ item.name }}</span>
                    <span v-if="item.count > 1" class="reward-count">x{{ item.count }}</span>
                  </div>
                  <div v-if="item.rewardType === 'thanks'" class="reward-sub thanks-text">运气欠佳，下次再来</div>
                  <div v-else-if="item.isNew" class="reward-sub new-text">首次获得！</div>
                  <div v-else-if="item.ownedCount > 1 && !['gold','spiritStone','item'].includes(item.rewardType)" class="reward-sub repeat-text">重复获得，数量 +1</div>
                </div>
              </div>
            </div>
          </template>
          <!-- 十连结果 -->
          <template v-else>
            <div class="result-title">十连结果</div>
            <div class="result-stats">
              <span class="stat-new">新获得 {{ newCount }} 个</span>
            </div>
            <div class="result-body ten-grid">
              <div v-for="(item, idx) in resultRewards" :key="idx" class="reward-cell" :class="rewardClass(item)">
                <div class="cell-icon">{{ rewardIcon(item) }}</div>
                <div class="cell-name">{{ item.name }}</div>
                <span v-if="item.count > 1" class="cell-count">x{{ item.count }}</span>
                <span v-if="item.isNew" class="cell-new">NEW</span>
              </div>
            </div>
          </template>
          <button class="result-close-btn" @click="closeResult">确定</button>
        </div>
      </div>
    </Transition>
  </XiuXianModal>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import XiuXianModal from '../common/XiuXianModal.vue'
import { apiClient, buildApiUrl } from '../../lib/apiClient'
import { useGameStore } from '../../state/gameStore'
import { ICON } from '../../icons'
import AssetIcon from '../common/AssetIcon.vue'

const gameStore = useGameStore()

const props = defineProps({ modelValue: { type: Boolean, default: false } })
const emit = defineEmits(['update:modelValue'])

const tabs = [
  { key: 0, label: '文字图鉴', icon: ICON.misc_book_open },
  { key: 1, label: '图片图鉴', icon: ICON.misc_image_frame },
  { key: 2, label: '幸运抽奖', icon: ICON.misc_lottery }
]

// 转盘6扇区（展示用）
const segments = [
  { icon: ICON.misc_book_open, label: '文字图鉴', color: '#a855f7' },
  { icon: ICON.misc_image_frame, label: '图片图鉴', color: '#3b82f6' },
  { icon: ICON.misc_money_bag, label: '金币', color: '#f59e0b' },
  { icon: ICON.misc_diamond, label: '灵石', color: '#06b6d4' },
  { icon: ICON.misc_gift, label: '道具', color: '#22c55e' },
  { icon: ICON.misc_leaf, label: '谢谢惠顾', color: '#6b7280' }
]

const activeTab = ref(0)
const pools = ref([])
const loading = ref(false)
const drawing = ref(false)
const spinning = ref(false)
const wheelRotation = ref(0)
const error = ref('')
const showResult = ref(false)
const resultMode = ref('single')
const resultRewards = ref([])

const currentPool = computed(() => pools.value.find(p => p.lotteryType === activeTab.value))
const costLabel = computed(() => {
  if (!currentPool.value) return ''
  return currentPool.value.costType === 0 ? '金币' : currentPool.value.costType === 1 ? '灵石' : '道具'
})
const canDraw = computed(() => currentPool.value?.canDraw && currentPool.value.supportSingle && !spinning.value)
const canDrawTen = computed(() => currentPool.value?.canDraw && currentPool.value.supportTen && !spinning.value)
const newCount = computed(() => resultRewards.value.filter(r => r.isNew).length)

function switchTab(key) {
  if (spinning.value) return
  activeTab.value = key
}

// conic-gradient 生成
const wheelGradient = computed(() => {
  const n = segments.length
  const angle = 360 / n
  const stops = segments.flatMap((seg, i) => {
    const start = i * angle
    const end = (i + 1) * angle
    // 每个扇区两种深浅交替
    const light = i % 2 === 0 ? 0.85 : 0.65
    return [
      `${seg.color} ${start}deg`,
      `${adjustBrightness(seg.color, light)} ${end}deg`
    ]
  })
  return `conic-gradient(from 0deg, ${stops.join(', ')})`
})

function adjustBrightness(hex, factor) {
  const r = parseInt(hex.slice(1, 3), 16)
  const g = parseInt(hex.slice(3, 5), 16)
  const b = parseInt(hex.slice(5, 7), 16)
  return `rgb(${Math.round(r * factor)}, ${Math.round(g * factor)}, ${Math.round(b * factor)})`
}

// emoji 标记点位置（扇区中心半径处）
function markerStyle(index) {
  const n = segments.length
  const angle = (360 / n) * index + 360 / n / 2
  const rad = (angle - 90) * Math.PI / 180
  const r = 90 // 距中心半径 px
  const x = 140 + r * Math.cos(rad) - 16
  const y = 140 + r * Math.sin(rad) - 16
  return { left: x + 'px', top: y + 'px' }
}

function rewardTypeToSegment(rewardType) {
  const map = { text_collection: 0, image_collection: 1, gold: 2, spiritStone: 3, item: 4, thanks: 5 }
  return map[rewardType] ?? 5
}

function calcTargetAngle(segmentIndex) {
  const segAngle = 360 / segments.length
  const target = segmentIndex * segAngle + segAngle / 2
  const extraSpins = (5 + Math.floor(Math.random() * 3)) * 360
  return extraSpins + (360 - target)
}

async function doSingleDraw() {
  if (!currentPool.value || spinning.value || drawing.value) return
  spinning.value = true
  drawing.value = true
  error.value = ''
  try {
    const result = await apiClient.lotteryDraw(currentPool.value.poolId, 1)
    const rewards = result.rewards || []
    const firstReward = rewards[0]
    if (firstReward) {
      const segIdx = rewardTypeToSegment(firstReward.rewardType)
      wheelRotation.value += calcTargetAngle(segIdx)
    }
    await new Promise(resolve => setTimeout(resolve, 4200))
    resultMode.value = 'single'
    resultRewards.value = rewards
    showResult.value = true
    await loadPools()
    await gameStore.loadPlayer(true)
  } catch (e) {
    error.value = e.message || '抽奖失败'
    spinning.value = false
  } finally {
    drawing.value = false
  }
}

async function doTenDraw() {
  if (!currentPool.value || spinning.value || drawing.value) return
  spinning.value = true
  drawing.value = true
  error.value = ''
  try {
    const result = await apiClient.lotteryDraw(currentPool.value.poolId, 10)
    await new Promise(resolve => setTimeout(resolve, 2000))
    resultMode.value = 'ten'
    resultRewards.value = result.rewards || []
    showResult.value = true
    await loadPools()
    await gameStore.loadPlayer(true)
  } catch (e) {
    error.value = e.message || '抽奖失败'
  } finally {
    spinning.value = false
    drawing.value = false
  }
}

function onWheelStop() {
  spinning.value = false
}

function closeResult() {
  showResult.value = false
  resultRewards.value = []
}

function rewardClass(item) {
  if (item.rewardType === 'thanks') return 'thanks'
  if (item.isNew) return 'new'
  return 'repeat'
}

function rewardIcon(item) {
  const map = { text_collection: ICON.misc_book_open, image_collection: ICON.misc_image_frame, gold: ICON.misc_money_bag, spiritStone: ICON.misc_diamond, item: ICON.misc_gift, thanks: ICON.misc_leaf }
  return map[item.rewardType] || ICON.misc_gift
}

async function loadPools() {
  loading.value = true
  error.value = ''
  try {
    pools.value = await apiClient.getLotteryPools()
  } catch (e) {
    error.value = e.message || '加载抽奖池失败'
  } finally {
    loading.value = false
  }
}

watch(() => props.modelValue, (val) => {
  if (val) {
    showResult.value = false
    resultRewards.value = []
    error.value = ''
    wheelRotation.value = 0
    loadPools()
  }
})
</script>

<style scoped>
.lottery-modal { display: flex; flex-direction: column; gap: var(--spacing-md); }

/* Tab */
.lottery-tabs {
  display: flex; gap: var(--spacing-xs);
  border-bottom: 2px solid var(--border-color);
  padding-bottom: var(--spacing-sm);
}
.lottery-tab-btn {
  display: flex; align-items: center; gap: 6px;
  padding: 8px 16px;
  background: var(--bg-overlay-light); color: var(--text-secondary);
  border: 1px solid var(--border-color); border-radius: var(--radius-md);
  cursor: pointer; transition: all 0.2s ease; font-size: var(--font-size-sm);
}
.lottery-tab-btn:hover:not(.active) { background: var(--bg-overlay-medium); color: var(--text-primary); }
.lottery-tab-btn.active { background: var(--accent-soft-bg); border-color: var(--accent-text); color: var(--accent-text); }
.tab-icon { font-size: 16px; }
.tab-label { font-weight: 500; }

/* 池子信息 */
.pool-header {
  background: var(--bg-overlay-medium); border: 1px solid var(--border-color);
  border-radius: var(--radius-md); padding: var(--spacing-md); text-align: center;
}
.pool-name { font-size: 16px; font-weight: 600; color: var(--highlight-text); margin-bottom: 6px; }
.pool-cost { display: flex; align-items: center; justify-content: center; gap: 6px; font-size: var(--font-size-sm); }
.cost-label { color: var(--text-muted); }
.cost-amount { color: var(--quality-legendary); font-weight: 700; font-size: 16px; font-family: var(--font-mono); }
.cost-type { color: var(--text-secondary); }
.pool-limits { display: flex; justify-content: center; gap: var(--spacing-md); margin-top: 6px; }
.limit-item { font-size: var(--font-size-xs); color: var(--text-muted); }

/* 转盘 */
.wheel-section { display: flex; justify-content: center; padding: var(--spacing-md) 0; position: relative; }

.draw-loading {
  position: absolute; inset: 0; z-index: 20;
  display: flex; flex-direction: column; align-items: center; justify-content: center; gap: var(--spacing-sm);
  background: var(--overlay-bg, rgba(0,0,0,0.6)); border-radius: var(--radius-lg);
}
.draw-loading-spinner {
  width: 36px; height: 36px;
  border: 3px solid var(--border-color); border-top-color: var(--accent-text);
  border-radius: 50%; animation: spin 0.8s linear infinite;
}
.draw-loading-text { color: var(--highlight-text); font-size: 15px; font-weight: 600; }
@keyframes spin { to { transform: rotate(360deg); } }

.wheel-container { position: relative; width: 280px; height: 296px; }

.wheel-pointer {
  position: absolute; top: 0; left: 50%; transform: translateX(-50%);
  font-size: 24px; color: var(--highlight-text-strong); z-index: 10;
  filter: drop-shadow(0 2px 4px rgba(0,0,0,0.5));
}

.wheel-outer {
  position: absolute; top: 16px; left: 0;
  width: 280px; height: 280px;
}

.wheel-disc {
  width: 100%; height: 100%; border-radius: 50%;
  position: relative; overflow: hidden;
  border: 3px solid var(--highlight-text);
  box-shadow: 0 0 20px rgba(0,0,0,0.3), inset 0 0 15px rgba(0,0,0,0.2);
  transition: transform 4s cubic-bezier(0.17, 0.67, 0.12, 0.99);
  background: v-bind(wheelGradient);
}

.wheel-marker {
  position: absolute; width: 32px; height: 32px;
  display: flex; align-items: center; justify-content: center;
  pointer-events: none;
}
.marker-emoji {
  font-size: 22px;
  filter: drop-shadow(0 1px 2px rgba(0,0,0,0.4));
}

.wheel-center-btn {
  position: absolute; top: 50%; left: 50%;
  transform: translate(-50%, -50%);
  width: 64px; height: 64px; border-radius: 50%;
  background: linear-gradient(135deg, var(--button-primary-start, #2d1f5e), var(--button-primary-end, #7c3aed));
  color: var(--button-primary-text, #fff);
  border: 3px solid var(--highlight-text);
  font-size: 16px; font-weight: 700; cursor: pointer; z-index: 5;
  box-shadow: 0 2px 10px rgba(0,0,0,0.3); transition: all 0.2s ease;
}
.wheel-center-btn:hover:not(:disabled) {
  transform: translate(-50%, -50%) scale(1.05);
  box-shadow: 0 4px 16px rgba(124, 58, 237, 0.4);
}
.wheel-center-btn:disabled { opacity: 0.5; cursor: not-allowed; }

/* 操作按钮 */
.draw-actions { display: flex; gap: var(--spacing-md); justify-content: center; }
.action-btn {
  display: flex; flex-direction: column; align-items: center; gap: 4px;
  padding: 10px 28px; border: 1px solid var(--border-color); border-radius: var(--radius-md);
  cursor: pointer; transition: all 0.2s ease; font-size: var(--font-size-sm);
}
.action-btn.single {
  background: linear-gradient(135deg, var(--button-primary-start, #2d1f5e), var(--button-primary-end, #7c3aed));
  color: var(--button-primary-text, #fff); border-color: var(--accent-text);
}
.action-btn.ten {
  background: linear-gradient(135deg, #7c3aed, #a855f7);
  color: #fff; border-color: var(--quality-epic);
}
.action-btn:hover:not(:disabled) { transform: translateY(-2px); box-shadow: var(--shadow-sm); }
.action-btn:disabled { opacity: 0.4; cursor: not-allowed; }
.btn-label { font-weight: 600; }
.btn-cost { font-size: var(--font-size-xs); opacity: 0.8; }

/* 加载 / 空 / 错误 */
.lottery-content,
.lottery-loading,
.lottery-empty {
  height: min(510px, 65vh);
  min-height: 0;
  box-sizing: border-box;
}

.lottery-content {
  overflow-y: auto;
}

.lottery-loading { display: flex; align-items: center; justify-content: center; gap: var(--spacing-sm); padding: var(--spacing-xl) 0; color: var(--text-secondary); }
.loading-spinner {
  width: 18px; height: 18px;
  border: 2px solid var(--border-color); border-top-color: var(--accent-text);
  border-radius: 50%; animation: spin 0.8s linear infinite;
}
.lottery-empty { display: flex; align-items: center; justify-content: center; text-align: center; color: var(--text-muted); padding: var(--spacing-xl) 0; }
.lottery-error { color: var(--hp-color); text-align: center; font-size: var(--font-size-sm); }

/* 结果弹窗 */
.result-overlay {
  position: fixed; inset: 0; background: var(--overlay-bg);
  display: flex; align-items: center; justify-content: center; z-index: 2000;
}
.result-card {
  min-width: 340px; max-width: 480px; max-height: 80vh; overflow-y: auto;
  background: var(--xiuxian-bg-panel); border: 2px solid var(--highlight-text);
  border-radius: var(--radius-lg); padding: var(--spacing-xl); text-align: center;
  box-shadow: var(--modal-shadow);
}
.result-title { font-size: 18px; font-weight: 700; color: var(--highlight-text); margin-bottom: var(--spacing-md); }
.result-stats { margin-bottom: var(--spacing-md); }
.stat-new { font-size: var(--font-size-sm); color: var(--quality-uncommon); }
.result-body { display: flex; flex-direction: column; gap: var(--spacing-sm); margin-bottom: var(--spacing-lg); }

.reward-line {
  display: flex; align-items: center; gap: var(--spacing-md);
  padding: var(--spacing-sm) var(--spacing-md);
  background: var(--bg-overlay-medium); border-radius: var(--radius-md);
  border: 1px solid var(--border-color); text-align: left;
}
.reward-line.thanks { opacity: 0.6; border-color: var(--text-muted); }
.reward-line.new { border-color: var(--quality-uncommon); background: rgba(34, 197, 94, 0.08); }
.reward-thumb {
  width: 48px; height: 48px; border-radius: var(--radius-sm);
  object-fit: cover; border: 1px solid var(--border-color); flex-shrink: 0;
}
.reward-info { flex: 1; }
.reward-main { display: flex; align-items: center; gap: 6px; }
.reward-name { font-weight: 600; font-size: var(--font-size-sm); }
.reward-name.thanks { color: var(--text-muted); }
.reward-name.new { color: var(--quality-uncommon); }
.reward-name.repeat { color: var(--text-primary); }
.reward-count { font-size: var(--font-size-xs); color: var(--text-secondary); font-family: var(--font-mono); }
.reward-sub { font-size: var(--font-size-xs); margin-top: 2px; }
.thanks-text { color: var(--text-muted); }
.new-text { color: var(--quality-uncommon); }
.repeat-text { color: var(--text-secondary); }

.ten-grid { display: grid; grid-template-columns: repeat(5, 1fr); gap: var(--spacing-xs); }
.reward-cell {
  display: flex; flex-direction: column; align-items: center; gap: 2px;
  padding: var(--spacing-xs); background: var(--bg-overlay-medium);
  border: 1px solid var(--border-color); border-radius: var(--radius-sm); position: relative;
}
.reward-cell.thanks { opacity: 0.5; }
.reward-cell.new { border-color: var(--quality-uncommon); }
.cell-icon { font-size: 20px; }
.cell-name { font-size: 10px; color: var(--text-secondary); max-width: 60px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.cell-count { font-size: 9px; color: var(--text-muted); font-family: var(--font-mono); }
.cell-new {
  position: absolute; top: 2px; right: 2px;
  font-size: 8px; background: var(--quality-uncommon); color: #000;
  padding: 1px 3px; border-radius: 2px; font-weight: 700;
}

.result-close-btn {
  padding: 10px 40px;
  background: linear-gradient(135deg, var(--button-primary-start, #2d1f5e), var(--button-primary-end, #7c3aed));
  color: var(--button-primary-text, #fff); border: none; border-radius: var(--radius-md);
  font-size: var(--font-size-sm); font-weight: 600; cursor: pointer; transition: all 0.15s ease;
}
.result-close-btn:hover { transform: translateY(-1px); box-shadow: var(--shadow-sm); }

.fade-enter-active, .fade-leave-active { transition: all 0.25s ease; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
.fade-enter-from .result-card, .fade-leave-to .result-card { transform: scale(0.9); }
</style>
