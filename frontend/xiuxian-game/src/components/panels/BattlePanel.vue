<template>
  <!-- 战斗面板 -->
  <div class="battle-panel">
    <!-- 战斗场景 -->
    <div class="battle-scene" :style="sceneStyle">
      <!-- 地图信息 -->
      <div class="map-info">
        <span class="map-name"><AssetIcon :source="ICON.ui_location" size="20" /> {{ currentMap.name }}</span>
        <span class="map-difficulty" :class="currentMap.difficulty">{{ currentMap.difficultyText }}</span>
      </div>

      <!-- 战斗单位区域 -->
      <div class="battle-field">
        <!-- 敌方单位（左侧） -->
        <div class="enemy-side">
          <div
            v-for="enemy in enemies"
            :key="enemy.id"
            class="battle-unit enemy"
            :class="{ dead: enemy.hp <= 0, active: currentTurn === 'enemy' && enemy.hp > 0 }"
          >
            <div class="unit-avatar"><AssetIcon :source="enemy.avatar" size="40" /></div>
            <div class="unit-name">{{ enemy.name }}</div>

            <div class="unit-bars">
              <div class="hp-bar">
                <div class="bar-fill" :style="{ width: (enemy.hp / enemy.maxHp * 100) + '%' }"></div>
                <span class="bar-text">{{ enemy.hp }}/{{ enemy.maxHp }}</span>
              </div>

              <div v-if="enemy.maxMp > 0" class="mp-bar">
                <div class="bar-fill" :style="{ width: (enemy.mp / enemy.maxMp * 100) + '%' }"></div>
                <span class="bar-text">{{ enemy.mp }}/{{ enemy.maxMp }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- 战斗VS标识 -->
        <div class="vs-indicator">
          <div class="vs-circle">VS</div>
        </div>

        <!-- 友方单位（右侧） -->
        <div class="ally-side">
          <div
            v-for="ally in allies"
            :key="ally.id"
            class="battle-unit ally"
            :class="{ dead: ally.hp <= 0, active: currentTurn === 'ally' && ally.hp > 0 }"
          >
            <div class="unit-avatar"><AssetIcon :source="ally.avatar" size="40" /></div>
            <div class="unit-name">{{ ally.name }}</div>

            <div class="unit-bars">
              <div class="hp-bar">
                <div class="bar-fill" :style="{ width: (ally.hp / ally.maxHp * 100) + '%' }"></div>
                <span class="bar-text">{{ ally.hp }}/{{ ally.maxHp }}</span>
              </div>

              <div class="mp-bar">
                <div class="bar-fill" :style="{ width: (ally.mp / ally.maxMp * 100) + '%' }"></div>
                <span class="bar-text">{{ ally.mp }}/{{ ally.maxMp }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 浮动战斗文字 -->
      <div class="floating-texts">
        <TransitionGroup name="float">
          <div
            v-for="text in floatingTexts"
            :key="text.id"
            class="floating-text"
            :class="text.type"
            :style="{ left: text.x + '%', top: text.y + '%' }"
          >
            {{ text.content }}
          </div>
        </TransitionGroup>
      </div>
    </div>

    <!-- 战斗控制区 -->
    <div class="battle-controls">
      <!-- 战斗日志 -->
      <div class="battle-log">
        <div class="log-header">
          <span><AssetIcon :source="ICON.misc_scroll" size="25" /> 战斗日志</span>
          <button class="clear-btn" @click="clearLog">清空</button>
        </div>

        <div ref="logContainer" class="log-content">
          <div
            v-for="(log, index) in battleLogs"
            :key="index"
            class="log-entry"
            :class="log.type"
          >
            <span class="log-time">[{{ log.time }}]</span>
            <span class="log-text">{{ log.message }}</span>
          </div>
        </div>
      </div>

      <!-- 战斗操作 -->
      <div class="battle-actions">
        <div class="action-buttons">
          <button
            class="action-btn attack"
            :disabled="!canAct"
            @click="doAction('attack')"
          >
            <AssetIcon :source="ICON.stat_attack" size="25" /> 普通攻击
          </button>

          <button
            class="action-btn skill"
            :disabled="!canAct"
            @click="doAction('skill')"
          >
            <AssetIcon :source="ICON.misc_fire" size="25" /> 释放技能
          </button>

          <button
            class="action-btn item"
            :disabled="!canAct"
            @click="doAction('item')"
          >
            <AssetIcon :source="ICON.item_potion" size="25" /> 使用物品
          </button>

          <button
            class="action-btn escape"
            :disabled="!canAct"
            @click="doAction('escape')"
          >
            <AssetIcon :source="ICON.stat_speed_up" size="25" /> 逃跑
          </button>
        </div>

        <div class="auto-control">
          <button class="auto-btn" :class="{ active: isAuto }" @click="toggleAuto">
            {{ isAuto ? '⏸️ 停止自动' : '▶️ 自动战斗' }}
          </button>

          <button class="map-btn" @click="openMapSelect">
            <AssetIcon :source="ICON.misc_map" size="25" /> 选择地图
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { reactive, ref, computed, nextTick, onMounted, onUnmounted, inject } from 'vue'
import { ICON } from '../../icons'
import AssetIcon from '../common/AssetIcon.vue'

/**
 * BattlePanel - 战斗面板
 *
 * 功能：
 * - 显示战斗场景（地图背景）
 * - 左侧敌方单位，右侧友方单位
 * - 显示血量条、蓝量条
 * - 战斗日志滚动显示
 * - 战斗操作按钮（攻击、技能、物品、逃跑）
 * - 自动战斗功能
 */
export default {
  name: 'BattlePanel',
  components: { AssetIcon },

  setup() {
    const openModal = inject('openModal')

    // 当前地图
    const currentMap = reactive({
      name: '青云山脉',
      difficulty: 'normal',
      difficultyText: '普通',
      bgImage: 'linear-gradient(180deg, #1a2332 0%, #0d1117 100%)'
    })

    // 场景样式
    const sceneStyle = computed(() => ({
      background: currentMap.bgImage
    }))

    // 敌方单位
    const enemies = reactive([
      { id: 1, name: '妖狼', avatar: ICON.creature_wolf, hp: 800, maxHp: 800, mp: 0, maxMp: 0 },
      { id: 2, name: '妖狼王', avatar: ICON.creature_wolf, hp: 1500, maxHp: 1500, mp: 200, maxMp: 200 }
    ])

    // 友方单位
    const allies = reactive([
      { id: 1, name: '青云道人', avatar: ICON.misc_wizard, hp: 2500, maxHp: 2500, mp: 500, maxMp: 500 }
    ])

    // 当前回合
    const currentTurn = ref('ally')
    const canAct = computed(() => currentTurn.value === 'ally')
    const isAuto = ref(false)

    // 战斗日志
    const battleLogs = reactive([])
    const logContainer = ref(null)

    // 浮动文字
    const floatingTexts = reactive([])

    // 添加日志
    const addLog = (message, type = 'normal') => {
      const now = new Date()
      const time = now.toLocaleTimeString('zh-CN', { hour12: false })
      battleLogs.push({ time, message, type })

      // 限制日志数量
      if (battleLogs.length > 100) {
        battleLogs.shift()
      }

      // 自动滚动到底部
      nextTick(() => {
        if (logContainer.value) {
          logContainer.value.scrollTop = logContainer.value.scrollHeight
        }
      })
    }

    // 清空日志
    const clearLog = () => {
      battleLogs.length = 0
    }

    // 添加浮动文字
    const addFloatingText = (content, x, y, type = 'damage') => {
      const id = Date.now() + Math.random()
      floatingTexts.push({ id, content, x, y, type })

      setTimeout(() => {
        const index = floatingTexts.findIndex(t => t.id === id)
        if (index > -1) {
          floatingTexts.splice(index, 1)
        }
      }, 1500)
    }

    // 执行动作
    const doAction = (action) => {
      if (!canAct.value) return

      switch (action) {
        case 'attack':
          performAttack()
          break
        case 'skill':
          performSkill()
          break
        case 'item':
          useItem()
          break
        case 'escape':
          attemptEscape()
          break
      }
    }

    // 普通攻击
    const performAttack = () => {
      const target = enemies.find(e => e.hp > 0)
      if (!target) return

      const damage = Math.floor(Math.random() * 100) + 150
      target.hp = Math.max(0, target.hp - damage)

      // 暴击判断
      const isCrit = Math.random() < 0.2
      const finalDamage = isCrit ? Math.floor(damage * 1.5) : damage

      addLog(`你对 ${target.name} 造成 ${finalDamage} 点${isCrit ? '暴击' : ''}伤害！`, isCrit ? 'crit' : 'attack')
      addFloatingText(`-${finalDamage}`, 25, 30 + Math.random() * 20, isCrit ? 'crit' : 'damage')

      // 切换到敌方回合
      switchTurn()
    }

    // 释放技能
    const performSkill = () => {
      const ally = allies[0]
      if (ally.mp < 50) {
        addLog('法力不足！', 'warning')
        return
      }

      ally.mp -= 50
      const target = enemies.find(e => e.hp > 0)
      if (!target) return

      const damage = Math.floor(Math.random() * 150) + 300
      target.hp = Math.max(0, target.hp - damage)

      addLog(`你施展「火球术」，对 ${target.name} 造成 ${damage} 点伤害！`, 'skill')
      addFloatingText(`-${damage}`, 25, 30 + Math.random() * 20, 'skill')

      switchTurn()
    }

    // 使用物品
    const useItem = () => {
      const ally = allies[0]
      const heal = 300
      ally.hp = Math.min(ally.maxHp, ally.hp + heal)

      addLog(`你使用了止血丹，恢复 ${heal} 点生命值`, 'heal')
      addFloatingText(`+${heal}`, 75, 30 + Math.random() * 20, 'heal')

      switchTurn()
    }

    // 尝试逃跑
    const attemptEscape = () => {
      const success = Math.random() > 0.5
      if (success) {
        addLog('你成功逃离了战斗！', 'success')
        // 重置战斗
        resetBattle()
      } else {
        addLog('逃跑失败！', 'warning')
        switchTurn()
      }
    }

    // 敌方回合
    const enemyTurn = () => {
      const aliveEnemies = enemies.filter(e => e.hp > 0)
      if (aliveEnemies.length === 0) {
        addLog('战斗胜利！', 'success')
        resetBattle()
        return
      }

      aliveEnemies.forEach((enemy, index) => {
        setTimeout(() => {
          const ally = allies[0]
          const damage = Math.floor(Math.random() * 50) + 80
          ally.hp = Math.max(0, ally.hp - damage)

          addLog(`${enemy.name} 对你造成 ${damage} 点伤害！`, 'enemy')
          addFloatingText(`-${damage}`, 75, 30 + Math.random() * 20, 'damage')

          if (ally.hp <= 0) {
            addLog('你倒下了...', 'fail')
            resetBattle()
          }

          if (index === aliveEnemies.length - 1) {
            currentTurn.value = 'ally'
          }
        }, (index + 1) * 800)
      })
    }

    // 切换回合
    const switchTurn = () => {
      currentTurn.value = 'enemy'
      setTimeout(() => {
        enemyTurn()
      }, 500)
    }

    // 重置战斗
    const resetBattle = () => {
      setTimeout(() => {
        enemies.forEach(e => {
          e.hp = e.maxHp
          e.mp = e.maxMp
        })
        allies.forEach(a => {
          a.hp = a.maxHp
          a.mp = a.maxMp
        })
        currentTurn.value = 'ally'
      }, 2000)
    }

    // 自动战斗
    let autoInterval = null
    const toggleAuto = () => {
      isAuto.value = !isAuto.value

      if (isAuto.value) {
        autoInterval = setInterval(() => {
          if (canAct.value) {
            doAction('attack')
          }
        }, 2000)
      } else {
        clearInterval(autoInterval)
      }
    }

    // 打开地图选择
    const openMapSelect = () => {
      openModal('mapSelect')
    }

    onMounted(() => {
      addLog('战斗开始！')
    })

    onUnmounted(() => {
      if (autoInterval) {
        clearInterval(autoInterval)
      }
    })

    return {
      ICON,
      currentMap,
      sceneStyle,
      enemies,
      allies,
      currentTurn,
      canAct,
      isAuto,
      battleLogs,
      logContainer,
      floatingTexts,
      addLog,
      clearLog,
      doAction,
      toggleAuto,
      openMapSelect
    }
  }
}
</script>

<style scoped>
.battle-panel {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
  height: 100%;
}

/* 战斗场景 */
.battle-scene {
  flex: 1;
  border-radius: var(--radius-lg);
  border: 1px solid var(--border-color);
  position: relative;
  overflow: hidden;
  min-height: 350px;
}

.map-info {
  position: absolute;
  top: var(--spacing-md);
  left: var(--spacing-md);
  display: flex;
  gap: var(--spacing-sm);
  align-items: center;
  background: var(--bg-overlay-dark);
  padding: var(--spacing-sm) var(--spacing-md);
  border-radius: var(--radius-md);
}

.map-name {
  font-size: 14px;
  color: var(--text-primary);
}

.map-difficulty {
  padding: 2px 8px;
  border-radius: var(--radius-sm);
  font-size: var(--font-size-sm);
}

.map-difficulty.easy { background: var(--status-success-bg); color: var(--status-success-text); }
.map-difficulty.normal { background: var(--status-warning-bg); color: var(--status-warning-text); }
.map-difficulty.hard { background: var(--status-danger-bg); color: var(--status-danger-text); }

/* 战斗区域 */
.battle-field {
  display: flex;
  justify-content: space-between;
  align-items: center;
  height: 100%;
  padding: 0 var(--spacing-xl);
}

.enemy-side,
.ally-side {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-lg);
}

.vs-indicator {
  position: absolute;
  left: 50%;
  top: 50%;
  transform: translate(-50%, -50%);
}

.vs-circle {
  width: 60px;
  height: 60px;
  background: var(--button-primary-start);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 20px;
  font-weight: 700;
  color: var(--text-on-dark);
  box-shadow: var(--shadow-sm);
}

/* 战斗单位 */
.battle-unit {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--spacing-sm);
  padding: var(--spacing-md);
  background: var(--xiuxian-bg-panel);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  transition: all 0.15s ease;
  min-width: 120px;
}

.battle-unit.enemy {
  background: var(--xiuxian-bg-panel);
  border-color: var(--border-color);
}

.battle-unit.ally {
  background: var(--xiuxian-bg-panel);
  border-color: var(--border-color);
}

.battle-unit.active {
  border-color: var(--highlight-text);
  box-shadow: var(--shadow-sm);
}

.battle-unit.dead {
  opacity: 0.4;
  filter: grayscale(100%);
}

.unit-avatar {
  font-size: 40px;
}

.unit-name {
  font-size: var(--font-size-base);
  color: var(--text-primary);
  font-weight: 500;
}

.unit-bars {
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.hp-bar,
.mp-bar {
  position: relative;
  width: 90px;
  height: 10px;
  background: var(--bg-overlay-dark);
  border-radius: 5px;
  overflow: hidden;
  border: 1px solid rgba(0, 0, 0, 0.15);
}

.hp-bar .bar-fill {
  height: 100%;
  background: var(--hp-color);
  border-radius: 5px;
  transition: width 0.15s ease;
}

.mp-bar .bar-fill {
  height: 100%;
  background: var(--mp-color);
  border-radius: 5px;
  transition: width 0.15s ease;
}

.bar-text {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  font-size: 8px;
  color: white;
  font-family: var(--font-mono);
  font-weight: 600;
  text-shadow: 0 1px 2px rgba(0, 0, 0, 0.8);
  white-space: nowrap;
}

/* 浮动文字 */
.floating-texts {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  pointer-events: none;
}

.floating-text {
  position: absolute;
  font-size: 20px;
  font-weight: 700;
  animation: float-up 1.5s ease-out forwards;
  font-family: var(--font-mono);
}

.floating-text.damage {
  color: var(--hp-color);
  text-shadow: 0 0 10px rgba(231, 76, 60, 0.5);
}

.floating-text.crit {
  color: var(--status-warning-text);
  font-size: 28px;
  text-shadow: 0 0 15px rgba(255, 152, 0, 0.6);
}

.floating-text.skill {
  color: var(--exp-color);
  text-shadow: 0 0 10px rgba(155, 89, 182, 0.5);
}

.floating-text.heal {
  color: var(--quality-uncommon);
  text-shadow: 0 0 10px rgba(76, 175, 80, 0.5);
}

@keyframes float-up {
  0% {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
  100% {
    opacity: 0;
    transform: translateY(-50px) scale(1.2);
  }
}

.float-enter-active,
.float-leave-active {
  transition: all 0.15s ease;
}

.float-enter-from,
.float-leave-to {
  opacity: 0;
}

/* 战斗控制区 */
.battle-controls {
  display: flex;
  gap: var(--spacing-md);
  height: 200px;
}

/* 战斗日志 */
.battle-log {
  flex: 1;
  background: var(--xiuxian-bg-panel);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.log-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--spacing-sm) var(--spacing-md);
  background: rgba(124, 58, 237, 0.1);
  border-bottom: 1px solid var(--border-color);
  font-size: var(--font-size-base);
  font-weight: 500;
  color: var(--accent-text);
}

.clear-btn {
  font-size: var(--font-size-sm);
  padding: 2px 8px;
  background: transparent;
  border: 1px solid var(--border-color);
  border-radius: var(--radius-sm);
  color: var(--text-muted);
  cursor: pointer;
}

.clear-btn:hover {
  border-color: var(--border-color);
  color: var(--accent-text);
}

.log-content {
  flex: 1;
  padding: var(--spacing-sm);
  overflow-y: auto;
  font-size: var(--font-size-base);
  line-height: 1.6;
}

.log-entry {
  padding: 2px 0;
}

.log-time {
  color: var(--text-muted);
  margin-right: var(--spacing-xs);
}

.log-entry.attack .log-text { color: var(--text-primary); }
.log-entry.crit .log-text { color: var(--status-warning-text); font-weight: 600; }
.log-entry.skill .log-text { color: var(--exp-color); }
.log-entry.heal .log-text { color: var(--quality-uncommon); }
.log-entry.enemy .log-text { color: var(--hp-color); }
.log-entry.success .log-text { color: var(--quality-uncommon); font-weight: 600; }
.log-entry.fail .log-text { color: var(--hp-color); }
.log-entry.warning .log-text { color: var(--status-warning-text); }

/* 战斗操作 */
.battle-actions {
  width: 250px;
  background: var(--xiuxian-bg-panel);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  padding: var(--spacing-md);
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}

.action-buttons {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--spacing-sm);
}

.action-btn {
  padding: var(--spacing-md);
  border: none;
  border-radius: var(--radius-sm);
  font-size: var(--font-size-base);
  cursor: pointer;
  transition: all 0.15s ease;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
}

.action-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.action-btn.attack {
  background: oklch(0.6 0.2 25 / 0.15);
  border: 1px solid oklch(0.6 0.2 25 / 0.3);
  color: #f87171;
}

.action-btn.skill {
  background: oklch(0.55 0.18 300 / 0.15);
  border: 1px solid oklch(0.55 0.18 300 / 0.3);
  color: #c084fc;
}

.action-btn.item {
  background: oklch(0.6 0.15 155 / 0.15);
  border: 1px solid oklch(0.6 0.15 155 / 0.3);
  color: #4ade80;
}

.action-btn.escape {
  background: var(--bg-overlay-light);
  border: 1px solid var(--border-color);
  color: var(--text-secondary);
}

.action-btn:not(:disabled):hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-md);
}

.auto-control {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
}

.auto-btn,
.map-btn {
  padding: var(--spacing-sm);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-sm);
  background: var(--xiuxian-bg-secondary);
  color: var(--text-primary);
  font-size: var(--font-size-base);
  cursor: pointer;
  transition: all 0.15s ease;
}

.auto-btn:hover,
.map-btn:hover {
  border-color: var(--border-color);
  background: rgba(124, 58, 237, 0.1);
}

.auto-btn.active {
  background: var(--status-success-bg);
  border-color: var(--quality-uncommon);
  color: var(--quality-uncommon);
}
</style>
