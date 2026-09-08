<template>
  <XiuXianModal :model-value="modelValue" title="战斗回放" :width="780" @update:model-value="$emit('update:modelValue', $event)">
    <div class="replay-wrapper">
      <!-- 战斗列 - 和主页完全一致 -->
      <div class="battle-columns">
        <!-- 敌人区域 -->
        <div class="enemy-section">
          <div class="section-header">
            <span class="section-title">敌方</span>
            <span class="unit-count">{{ enemies.filter(e => e.hp > 0).length }} / {{ enemies.length }}</span>
          </div>
          <div class="units-row">
            <div
              v-for="enemy in enemies"
              :key="enemy.id"
              class="battle-unit enemy"
              :class="{ dead: enemy.hp <= 0 }"
              @mouseenter="handleEnemyHover(enemy, $event)"
              @mouseleave="hideEnemyTooltip"
            >
              <div class="unit-name">{{ enemy.name }}</div>
              <div class="unit-hp-bar">
                <div class="hp-fill" :style="{ width: (enemy.maxHp > 0 ? (enemy.hp / enemy.maxHp * 100) : 0) + '%' }"></div>
                <span class="hp-text">{{ enemy.hp }}/{{ enemy.maxHp }}</span>
              </div>
              <div class="unit-mp-bar" v-if="enemy.maxMp > 0">
                <div class="mp-fill" :style="{ width: (enemy.maxMp > 0 ? (enemy.mp / enemy.maxMp * 100) : 0) + '%' }"></div>
                <span class="mp-text">{{ enemy.mp }}/{{ enemy.maxMp }}</span>
              </div>
            </div>
            <div v-if="enemies.length === 0" class="no-units">暂无数据</div>
          </div>
          <Teleport to="body">
            <div
              v-if="hoveredEnemyTooltip"
              class="battle-unit-tooltip-teleport enemy"
              :style="{ left: enemyTooltipPosition.x + 'px', top: enemyTooltipPosition.y + 'px' }"
            >
              <div class="tooltip-title-row">
                <div class="tooltip-name">{{ hoveredEnemyTooltip.name }}</div>
                <div v-if="hoveredEnemyTooltip.elementName" class="tooltip-tag">{{ hoveredEnemyTooltip.elementName }}</div>
              </div>
              <div class="tooltip-stats">
                <div v-for="stat in hoveredEnemyTooltip.baseStats" :key="stat.label" class="tooltip-stat-row">
                  <span>{{ stat.label }}</span>
                  <span>{{ stat.value }}</span>
                </div>
              </div>
              <div v-if="hoveredEnemyTooltip.advancedStats.length > 0" class="tooltip-section-label">高级属性</div>
              <div v-if="hoveredEnemyTooltip.advancedStats.length > 0" class="tooltip-stats advanced">
                <div v-for="stat in hoveredEnemyTooltip.advancedStats" :key="stat.label" class="tooltip-stat-row">
                  <span>{{ stat.label }}</span>
                  <span>{{ stat.value }}</span>
                </div>
              </div>
            </div>
          </Teleport>
        </div>

        <!-- 友方区域 -->
        <div class="ally-section">
          <div class="section-header">
            <span class="section-title">友方</span>
            <span class="unit-count">{{ allies.filter(a => a.hp > 0).length }} / {{ allies.length }}</span>
          </div>
          <div class="units-row">
            <div
              v-for="ally in allies"
              :key="ally.id"
              class="battle-unit ally"
              :class="{ dead: ally.hp <= 0 }"
            >
              <div class="unit-name">{{ ally.name }}</div>
              <div class="unit-hp-bar">
                <div class="hp-fill" :style="{ width: (ally.maxHp > 0 ? (ally.hp / ally.maxHp * 100) : 0) + '%' }"></div>
                <span class="hp-text">{{ ally.hp }}/{{ ally.maxHp }}</span>
              </div>
              <div class="unit-mp-bar" v-if="ally.maxMp > 0">
                <div class="mp-fill" :style="{ width: (ally.maxMp > 0 ? (ally.mp / ally.maxMp * 100) : 0) + '%' }"></div>
                <span class="mp-text">{{ ally.mp }}/{{ ally.maxMp }}</span>
              </div>
            </div>
            <div v-if="allies.length === 0" class="no-units">暂无数据</div>
          </div>
        </div>

        <!-- 战斗日志 -->
        <div class="battle-log-section">
          <div class="log-header">
            <span class="section-title">战斗日志</span>
            <div class="control-btns">
              <button v-if="!isPlaying && !isDone" class="clear-btn" @click="startReplay">播放</button>
              <button v-if="isPlaying" class="clear-btn" @click="pauseReplay">暂停</button>
              <button v-if="isDone" class="clear-btn" @click="restartReplay">重播</button>
              <select v-model="speed" class="speed-select" :disabled="isPlaying">
                <option :value="600">0.5x</option>
                <option :value="320">1x</option>
                <option :value="150">2x</option>
                <option :value="60">5x</option>
              </select>
            </div>
          </div>
          <div ref="logContainer" class="log-content">
            <div v-if="displayedLogs.length === 0" class="empty-log">点击播放开始回放</div>
            <div
              v-for="(log, idx) in displayedLogs"
              :key="idx"
              class="log-entry"
              :class="log.type"
            >
              <span v-if="log.roundLabel" class="round-separator">{{ log.roundLabel }}</span>
              <span v-else class="log-text">{{ log.text }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </XiuXianModal>
</template>

<script>
import { ref, reactive, watch, computed, nextTick } from 'vue'
import XiuXianModal from '../common/XiuXianModal.vue'

// === 以下函数完全复制主页 MainView.vue ===

const readBattleValue = (source, ...keys) => {
  for (const key of keys) {
    const value = source?.[key]
    if (value !== undefined && value !== null) return value
  }
  return undefined
}

const toNumber = (v) => {
  const n = Number(v)
  return Number.isFinite(n) ? n : 0
}

const getEntryType = (type) => {
  const typeMap = {
    1: 'skill', 2: 'attack', 3: 'damage',
    6: 'warning', 8: 'heal', 9: 'heal', 10: 'heal',
    11: 'buff', 14: 'warning', 15: 'warning', 16: 'warning',
    17: 'warning', 18: 'warning', 19: 'warning', 20: 'warning',
    21: 'counter', 22: 'crit', 23: 'damage', 24: 'warning', 25: 'warning', 26: 'warning',
    27: 'fail', 29: 'crit', 30: 'success', 31: 'success', 34: 'warning'
  }
  return typeMap[type] || 'normal'
}

const getEntryDescription = (entry) => {
  const description = readBattleValue(entry, 'Description', 'description')
  if (description) return description

  const casterName = readBattleValue(entry, 'CasterName', 'casterName') || '未知单位'
  const targetName = readBattleValue(entry, 'TargetName', 'targetName') || '未知目标'
  const value = readBattleValue(entry, 'Value', 'value') || 0
  const skillName = readBattleValue(entry, 'SkillName', 'skillName') || '技能'
  const buffName = readBattleValue(entry, 'BuffName', 'buffName') || '增益效果'

  const typeDesc = {
    1: `${casterName} 使用技能 ${skillName}`,
    2: `${casterName} 对 ${targetName} 发动普通攻击`,
    3: `${casterName} 对 ${targetName} 造成 ${value} 点伤害`,
    6: `${targetName} 闪避了 ${casterName} 的攻击`,
    8: `${targetName} 恢复 ${value} 点生命值`,
    9: `${targetName} 被复活并恢复 ${value} 点生命值`,
    10: `${targetName} 通过吸血恢复 ${value} 点生命值`,
    11: `${targetName} 获得 ${buffName}`,
    15: `${targetName} 被驱散效果`,
    16: `${targetName} 被净化效果`,
    21: `${casterName} 对 ${targetName} 发动反击`,
    22: `${casterName} 对 ${targetName} 触发连击`,
    23: `${casterName} 对 ${targetName} 造成反伤`,
    24: `${targetName} 的护盾吸收了伤害`,
    25: `${targetName} 触发了庇护分担`,
    27: `${targetName} 倒下了`,
    29: `${casterName} 对 ${targetName} 触发了斩杀`
  }
  return typeDesc[readBattleValue(entry, 'Type', 'type')] || '未知战斗动作'
}

const resolveBattleLogType = (line) => {
  const text = String(line || '')
  if (!text) return 'normal'
  if (text.includes('暴击')) return 'crit'
  if (text.includes('释放') || text.includes('技能')) return 'skill'
  if (text.includes('恢复') || text.includes('治疗')) return 'heal'
  if (text.includes('胜利') || text.includes('通关')) return 'success'
  if (text.includes('失败') || text.includes('阵亡') || text.includes('战败')) return 'fail'
  if (text.includes('闪避')) return 'warning'
  if (text.includes('反伤') || text.includes('反击')) return 'counter'
  if (text.includes('击杀') || text.includes('倒下')) return 'fail'
  if (text.includes('获得') || text.includes('增益') || text.includes('buff')) return 'buff'
  if (text.includes('伤害') || text.includes('攻击')) return 'damage'
  return 'normal'
}

function normalizeFighterStates(raw) {
  if (!raw) return []
  if (Array.isArray(raw)) return raw.filter(Boolean)
  return []
}

function createUnitFromState(state, defaultIsPlayerSide) {
  const stateIsPlayerSide = readBattleValue(state, 'IsPlayerSide', 'isPlayerSide')
  const isPlayerSide = stateIsPlayerSide === undefined ? defaultIsPlayerSide : !!stateIsPlayerSide
  const fighterName = readBattleValue(state, 'FighterName', 'fighterName') || '未知单位'
  const fighterId = readBattleValue(state, 'FighterId', 'fighterId') || fighterName

  return {
    id: fighterId,
    name: fighterName,
    hp: toNumber(readBattleValue(state, 'CurrentHp', 'currentHp')),
    maxHp: toNumber(readBattleValue(state, 'MaxHp', 'maxHp')),
    mp: toNumber(readBattleValue(state, 'CurrentMp', 'currentMp')),
    maxMp: toNumber(readBattleValue(state, 'MaxMp', 'maxMp')),
    stats: state
  }
}

// === 组件 ===

export default {
  name: 'BattleReplayModal',
  components: { XiuXianModal },
  props: {
    modelValue: { type: Boolean, default: false },
    battleLogJson: { type: String, default: '' },
    attackerName: { type: String, default: '挑战方' },
    defenderName: { type: String, default: '防守方' },
    isWin: { type: Boolean, default: null }
  },
  emits: ['update:modelValue'],
  setup(props) {
    const rounds = ref([])
    const allies = ref([])
    const enemies = ref([])
    const displayedLogs = ref([])
    const isPlaying = ref(false)
    const isDone = ref(false)
    const speed = ref(320)
    const logContainer = ref(null)
    const hoveredEnemyTooltip = ref(null)
    const enemyTooltipPosition = reactive({ x: 0, y: 0 })
    let timer = null
    let queueIndex = 0
    let flatQueue = []

    // === 和主页完全一致的悬浮窗逻辑 ===

    const isMeaningfulBattleStat = (value) => {
      const text = String(value ?? '').trim()
      if (!text) return false
      const numeric = Number(text.replace('%', ''))
      return Number.isFinite(numeric) && Math.abs(numeric) > 0.0001
    }

    const buildEnemyTooltipData = (enemy) => {
      const state = enemy?.stats || {}
      const elementName = readBattleValue(state, 'ElementName', 'elementName') || ''
      const baseStats = [
        { label: '气血', value: `${enemy.hp}/${enemy.maxHp}` },
        ...(enemy.maxMp > 0 ? [{ label: '灵力', value: `${enemy.mp}/${enemy.maxMp}` }] : []),
        { label: '物攻', value: readBattleValue(state, 'PhysicalAttack', 'physicalAttack') || 0 },
        { label: '法攻', value: readBattleValue(state, 'MagicAttack', 'magicAttack') || 0 },
        { label: '物防', value: readBattleValue(state, 'PhysicalDefense', 'physicalDefense') || 0 },
        { label: '法防', value: readBattleValue(state, 'MagicDefense', 'magicDefense') || 0 },
        { label: '速度', value: readBattleValue(state, 'Speed', 'speed') || 0 }
      ]
      const advancedStats = [
        { label: '命中', value: readBattleValue(state, 'HitRate', 'hitRate') || '' },
        { label: '闪避', value: readBattleValue(state, 'DodgeRate', 'dodgeRate') || '' },
        { label: '暴击', value: readBattleValue(state, 'CritRate', 'critRate') || '' },
        { label: '暴伤', value: readBattleValue(state, 'CritDamage', 'critDamage') || '' },
        { label: '连击', value: readBattleValue(state, 'ComboRate', 'comboRate') || '' },
        { label: '反击', value: readBattleValue(state, 'CounterRate', 'counterRate') || '' },
        { label: '破甲', value: readBattleValue(state, 'ArmorBreak', 'armorBreak') || '' },
        { label: '附伤', value: readBattleValue(state, 'ExtraDamage', 'extraDamage') || '' }
      ].filter((stat) => isMeaningfulBattleStat(stat.value))
      return { name: enemy?.name || '未知敌人', elementName, baseStats, advancedStats }
    }

    const handleEnemyHover = (enemy, event) => {
      hoveredEnemyTooltip.value = buildEnemyTooltipData(enemy)
      const rect = event.currentTarget.getBoundingClientRect()
      const tooltipWidth = 240
      const tooltipHeight = 260
      const preferredX = rect.right + 10
      const fallbackX = rect.left - tooltipWidth - 10
      enemyTooltipPosition.x = preferredX + tooltipWidth > window.innerWidth
        ? Math.max(12, fallbackX)
        : preferredX
      enemyTooltipPosition.y = Math.min(
        Math.max(12, rect.top),
        Math.max(12, window.innerHeight - tooltipHeight - 12)
      )
    }

    const hideEnemyTooltip = () => {
      hoveredEnemyTooltip.value = null
    }

    // === 和主页完全一致的单位更新逻辑 ===

    function findUnitIndex(list, fighterId, fighterName) {
      return list.findIndex((unit) => {
        if (fighterId && unit.id === fighterId) return true
        return fighterName && unit.name === fighterName
      })
    }

    function replaceBattleUnitsByStates(fighterStates) {
      const normalizedStates = normalizeFighterStates(fighterStates)
      if (normalizedStates.length === 0) return

      const nextAllies = []
      const nextEnemies = []

      normalizedStates.forEach((state) => {
        const fighterId = readBattleValue(state, 'FighterId', 'fighterId')
        const fighterName = readBattleValue(state, 'FighterName', 'fighterName')
        const stateIsPlayerSide = readBattleValue(state, 'IsPlayerSide', 'isPlayerSide')
        const isPlayerSide = !!stateIsPlayerSide

        const unit = createUnitFromState({ ...state, IsPlayerSide: isPlayerSide }, isPlayerSide)

        if (isPlayerSide) nextAllies.push(unit)
        else nextEnemies.push(unit)
      })

      allies.value.splice(0, allies.value.length, ...nextAllies)
      enemies.value.splice(0, enemies.value.length, ...nextEnemies)
    }

    function upsertUnitFromState(state) {
      if (!state) return

      const fighterId = readBattleValue(state, 'FighterId', 'fighterId') || ''
      const fighterName = readBattleValue(state, 'FighterName', 'fighterName') || ''
      const allyIndex = findUnitIndex(allies.value, fighterId, fighterName)
      const enemyIndex = findUnitIndex(enemies.value, fighterId, fighterName)

      let isPlayerSide = readBattleValue(state, 'IsPlayerSide', 'isPlayerSide')
      if (isPlayerSide === undefined) {
        if (allyIndex >= 0) isPlayerSide = true
        else if (enemyIndex >= 0) isPlayerSide = false
      }

      const targetList = isPlayerSide ? allies.value : enemies.value
      const targetIndex = findUnitIndex(targetList, fighterId, fighterName)

      const hp = toNumber(readBattleValue(state, 'CurrentHp', 'currentHp'))
      const maxHp = toNumber(readBattleValue(state, 'MaxHp', 'maxHp'))
      const mp = toNumber(readBattleValue(state, 'CurrentMp', 'currentMp'))
      const maxMp = toNumber(readBattleValue(state, 'MaxMp', 'maxMp'))

      if (targetIndex >= 0) {
        targetList[targetIndex].hp = hp
        targetList[targetIndex].maxHp = maxHp
        targetList[targetIndex].mp = mp
        targetList[targetIndex].maxMp = maxMp
      } else {
        const unit = createUnitFromState(state, isPlayerSide)
        targetList.push(unit)
      }
    }

    // === 和主页完全一致的日志播放逻辑 ===

    function playEntry(entry) {
      const message = getEntryDescription(entry)
      const type = getEntryType(readBattleValue(entry, 'Type', 'type'))
      const shouldHide = String(message || '').includes('属性克制')
      if (!shouldHide) {
        addLog({ text: message, type })
      }

      const entryStates = normalizeFighterStates(entry?.FighterStates ?? entry?.fighterStates)
      if (entryStates.length > 0) {
        entryStates.forEach((state) => upsertUnitFromState(state))
        return
      }

      const targetCurrentHp = readBattleValue(entry, 'TargetCurrentHp', 'targetCurrentHp')
      const targetName = readBattleValue(entry, 'TargetName', 'targetName')
      if (targetCurrentHp !== undefined && targetName) {
        const enemy = enemies.value.find((item) => item.name === targetName)
        if (enemy) enemy.hp = Math.max(0, toNumber(targetCurrentHp))
        const ally = allies.value.find((item) => item.name === targetName)
        if (ally) ally.hp = Math.max(0, toNumber(targetCurrentHp))
      }
    }

    function scrollToBottom() {
      nextTick(() => { if (logContainer.value) logContainer.value.scrollTop = logContainer.value.scrollHeight })
    }

    function addLog(log) {
      displayedLogs.value.push(log)
      if (displayedLogs.value.length > 200) displayedLogs.value.shift()
      scrollToBottom()
    }

    function buildQueue() {
      const queue = []
      for (const round of rounds.value) {
        const roundNum = toNumber(readBattleValue(round, 'RoundNumber', 'roundNumber'))
        const roundStates = normalizeFighterStates(readBattleValue(round, 'FighterStates', 'fighterStates'))
        if (roundNum > 0) {
          queue.push({ kind: 'round', roundNum, fighterStates: roundStates })
        }
        for (const entry of (round.Entries || [])) {
          queue.push({ kind: 'entry', entry, roundNum })
        }
      }
      return queue
    }

    function playNext() {
      if (queueIndex >= flatQueue.length) {
        stopTimer()
        isPlaying.value = false
        isDone.value = true
        addLog({ text: `战斗结束：${props.isWin ? '胜利' : '失败'}`, type: props.isWin ? 'success' : 'fail' })
        return
      }

      const item = flatQueue[queueIndex++]

      if (item.kind === 'round') {
        addLog({ roundLabel: `━━ 第 ${item.roundNum} 回合 ━━`, type: 'separator' })
        if (Array.isArray(item.fighterStates) && item.fighterStates.length > 0) {
          replaceBattleUnitsByStates(item.fighterStates)
        }
        return
      }

      playEntry(item.entry)
    }

    function startReplay() {
      if (flatQueue.length === 0) return
      isPlaying.value = true
      isDone.value = false
      timer = setInterval(playNext, speed.value)
    }

    function pauseReplay() { stopTimer(); isPlaying.value = false }

    function restartReplay() {
      stopTimer()
      isPlaying.value = false
      isDone.value = false
      displayedLogs.value = []
      queueIndex = 0
      initUnits()
      startReplay()
    }

    function stopTimer() { if (timer) { clearInterval(timer); timer = null } }

    function initUnits() {
      if (rounds.value.length === 0) return
      const states = normalizeFighterStates(readBattleValue(rounds.value[0], 'FighterStates', 'fighterStates'))
      replaceBattleUnitsByStates(states)
    }

    function parseLog() {
      try {
        const data = JSON.parse(props.battleLogJson)
        rounds.value = Array.isArray(data) ? data : []
      } catch { rounds.value = [] }
      flatQueue = buildQueue()
      displayedLogs.value = []
      queueIndex = 0
      isDone.value = false
      isPlaying.value = false
      initUnits()
    }

    watch(() => props.modelValue, (val) => { if (val) parseLog(); else { stopTimer(); isPlaying.value = false } })
    watch(() => props.battleLogJson, () => { if (props.modelValue) parseLog() })

    return {
      allies, enemies, displayedLogs, isPlaying, isDone, speed,
      logContainer, startReplay, pauseReplay, restartReplay,
      hoveredEnemyTooltip, enemyTooltipPosition, handleEnemyHover, hideEnemyTooltip
    }
  }
}
</script>

<style scoped>
/* === 以下样式完全复制主页 MainView.vue === */

.replay-wrapper {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}

.battle-columns {
  display: flex;
  flex-direction: row;
  gap: var(--spacing-md);
  overflow: hidden;
}

/* 敌人区域 */
.enemy-section {
  flex: 0 0 180px;
  height: 350px;
  display: flex;
  flex-direction: column;
  background: var(--xiuxian-bg-panel);
  border: 1px solid var(--border-color);
  border-top: 3px solid var(--hp-color);
  border-radius: var(--radius-lg);
  padding: var(--spacing-md);
  overflow-y: auto;
}

.enemy-section .section-header {
  flex-shrink: 0;
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--spacing-md);
}

.enemy-section .section-title {
  font-size: var(--font-size-sm);
  font-weight: 600;
  color: var(--hp-color);
  background: rgba(244, 63, 94, 0.08);
  padding: 3px 8px;
  border-radius: var(--radius-sm);
  border: 1px solid rgba(244, 63, 94, 0.15);
}

/* 友方区域 */
.ally-section {
  flex: 0 0 180px;
  height: 350px;
  display: flex;
  flex-direction: column;
  background: var(--xiuxian-bg-panel);
  border: 1px solid var(--border-color);
  border-top: 3px solid var(--quality-uncommon);
  border-radius: var(--radius-lg);
  padding: var(--spacing-md);
  overflow-y: auto;
}

.ally-section .section-header {
  flex-shrink: 0;
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--spacing-md);
}

.ally-section .section-title {
  font-size: var(--font-size-sm);
  font-weight: 600;
  color: var(--quality-uncommon);
  background: rgba(34, 197, 94, 0.08);
  padding: 3px 8px;
  border-radius: var(--radius-sm);
  border: 1px solid rgba(34, 197, 94, 0.15);
}

.unit-count {
  font-size: var(--font-size-sm);
  color: var(--text-muted);
  background: var(--bg-overlay-light);
  padding: 2px 8px;
  border-radius: var(--radius-sm);
}

.units-row {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
  overflow-y: auto;
  flex: 1;
  min-height: 0;
}

/* 战斗单位 */
.battle-unit {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--spacing-xs);
  padding: var(--spacing-sm) var(--spacing-md);
  background: var(--xiuxian-bg-panel);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  min-width: 80px;
}

.battle-unit.enemy {
  background: var(--xiuxian-bg-panel);
  border-color: var(--border-color);
}

.battle-unit.ally {
  background: var(--xiuxian-bg-panel);
  border-color: var(--border-color);
}

.battle-unit.dead {
  opacity: 0.4;
  filter: grayscale(100%);
}

.unit-name {
  font-size: var(--font-size-base);
  color: var(--text-primary);
}

.battle-unit .unit-hp-bar {
  position: relative;
  width: 90px;
  height: 16px;
  background: var(--bg-overlay-dark);
  border-radius: 8px;
  overflow: hidden;
  border: 1px solid rgba(0, 0, 0, 0.15);
}

.battle-unit .hp-fill {
  height: 100%;
  background: var(--hp-color);
  border-radius: 8px;
  transition: width 0.3s ease;
}

.battle-unit .hp-text {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  font-size: 11px;
  color: white;
  font-family: var(--font-mono);
  font-weight: 600;
  text-shadow: 0 1px 2px rgba(0, 0, 0, 0.8);
  white-space: nowrap;
}

.battle-unit .unit-mp-bar {
  position: relative;
  width: 90px;
  height: 12px;
  background: var(--bg-overlay-dark);
  border-radius: 6px;
  overflow: hidden;
  margin-top: 4px;
  border: 1px solid rgba(0, 0, 0, 0.15);
}

.battle-unit .mp-fill {
  height: 100%;
  background: var(--mp-color);
  border-radius: 6px;
  transition: width 0.3s ease;
}

.battle-unit .mp-text {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  font-size: 10px;
  color: white;
  font-family: var(--font-mono);
  font-weight: 600;
  text-shadow: 0 1px 2px rgba(0, 0, 0, 0.8);
  white-space: nowrap;
}

.no-units {
  text-align: center;
  color: var(--text-muted);
  padding: var(--spacing-md);
  font-style: italic;
}

/* 战斗日志区域 */
.battle-log-section {
  flex: 1;
  height: 350px;
  min-width: 300px;
  background: var(--xiuxian-bg-panel);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-lg);
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.battle-log-section .log-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--spacing-sm) var(--spacing-md);
  background: var(--bg-overlay-light);
  border-bottom: 1px solid var(--border-color);
}

.battle-log-section .log-header .section-title {
  font-size: var(--font-size-sm);
  font-weight: 500;
  color: var(--accent-text);
}

.control-btns {
  display: flex;
  gap: 0.375rem;
  align-items: center;
}

.clear-btn {
  font-size: var(--font-size-sm);
  padding: 2px 8px;
  background: var(--button-neutral-bg);
  border: 1px solid var(--button-neutral-border);
  border-radius: var(--radius-sm);
  color: var(--button-neutral-text);
  cursor: pointer;
}

.clear-btn:hover {
  background: var(--button-outline-hover-bg);
  border-color: var(--button-outline-hover-border);
  color: var(--button-outline-hover-text);
}

.speed-select {
  font-size: var(--font-size-sm);
  padding: 2px 4px;
  background: var(--button-neutral-bg);
  border: 1px solid var(--button-neutral-border);
  border-radius: var(--radius-sm);
  color: var(--button-neutral-text);
  cursor: pointer;
}

.log-content {
  flex: 1;
  padding: var(--spacing-sm);
  overflow-y: auto;
  font-size: var(--font-size-base);
  line-height: 1.6;
  scrollbar-width: thin;
  scrollbar-color: var(--border-color) transparent;
}

.log-content::-webkit-scrollbar {
  width: 6px;
}

.log-content::-webkit-scrollbar-track {
  background: transparent;
}

.log-content::-webkit-scrollbar-thumb {
  background: var(--border-color);
  border-radius: 3px;
}

.log-entry {
  padding: 4px 8px;
  border-left: 2px solid transparent;
  border-radius: var(--radius-sm);
  transition: background 0.2s ease, border-color 0.2s ease;
}

.log-entry .log-text {
  color: var(--text-primary);
}

.log-entry.normal {
  border-left-color: rgba(90, 74, 58, 0.18);
}

.log-entry.attack {
  background: rgba(123, 86, 50, 0.05);
  border-left-color: var(--text-secondary);
}

.log-entry.damage {
  background: rgba(168, 90, 90, 0.06);
  border-left-color: var(--log-damage);
}

.log-entry.skill {
  background: rgba(123, 86, 50, 0.06);
  border-left-color: var(--log-skill);
}

.log-entry.heal {
  background: rgba(74, 122, 74, 0.06);
  border-left-color: var(--log-heal);
}

.log-entry.counter {
  background: rgba(79, 102, 89, 0.06);
  border-left-color: var(--text-secondary);
}

.log-entry.crit {
  background: rgba(167, 122, 72, 0.08);
  border-left-color: var(--highlight-text);
}

.log-entry.success {
  background: rgba(106, 138, 90, 0.08);
  border-left-color: var(--quality-uncommon);
}

.log-entry.success .log-text {
  font-weight: 600;
}

.log-entry.fail {
  background: rgba(168, 90, 90, 0.08);
  border-left-color: var(--hp-color);
}

.log-entry.fail .log-text {
  font-weight: 600;
}

.log-entry.warning {
  background: var(--bg-overlay-light);
  border-left-color: var(--text-muted);
}

.log-entry.buff {
  background: rgba(74, 95, 142, 0.06);
  border-left-color: var(--quality-rare);
}

.log-entry.separator {
  padding: 6px 8px;
  border-left: 2px solid var(--quality-uncommon);
  background: rgba(106, 138, 90, 0.06);
  margin: 4px 0;
}

.round-separator {
  font-weight: 600;
  color: var(--quality-uncommon);
  font-size: var(--font-size-sm);
}

.empty-log {
  text-align: center;
  color: var(--text-muted);
  padding: var(--spacing-xl);
  font-style: italic;
}

/* 悬浮窗 - 完全复制主页 */
.battle-unit-tooltip-teleport {
  position: fixed;
  width: 240px;
  padding: 12px;
  background: var(--xiuxian-bg-panel);
  border: 1px solid var(--border-color);
  border-radius: 8px;
  box-shadow: var(--shadow-lg);
  z-index: 99999;
  text-align: left;
  pointer-events: none;
}

.battle-unit-tooltip-teleport .tooltip-title-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  margin-bottom: 8px;
  padding-bottom: 8px;
  border-bottom: 1px solid var(--border-color);
}

.battle-unit-tooltip-teleport .tooltip-name {
  font-size: 16px;
  font-weight: 700;
  color: var(--highlight-text);
}

.battle-unit-tooltip-teleport .tooltip-tag {
  padding: 2px 8px;
  border-radius: 999px;
  background: var(--accent-soft-bg);
  border: 1px solid var(--border-color);
  color: var(--accent-text);
  font-size: 12px;
}

.battle-unit-tooltip-teleport .tooltip-section-label {
  margin-top: 8px;
  margin-bottom: 6px;
  font-size: 12px;
  color: var(--text-muted);
}

.battle-unit-tooltip-teleport .tooltip-stats {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.battle-unit-tooltip-teleport .tooltip-stat-row {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  font-size: 13px;
}

.battle-unit-tooltip-teleport .tooltip-stat-row span:first-child {
  color: var(--text-secondary);
}

.battle-unit-tooltip-teleport .tooltip-stat-row span:last-child {
  color: var(--text-primary);
  font-family: var(--font-mono);
}
</style>
