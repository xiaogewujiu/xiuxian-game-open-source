<template>
  <!-- 修仙问道 - 游戏主页面 (战斗布局) -->
  <div class="main-page">
    <!-- Floating toggle button when sidebar is collapsed -->
    <button
      v-if="isSidebarCollapsed"
      class="floating-sidebar-btn"
      title="展开导航栏"
      @click="isSidebarCollapsed = false"
    >
      <span>☰</span>
    </button>

    <div class="content-wrapper">
      <!-- 现代左侧导航栏 -->
      <AppSidebar
        :class="{ collapsed: isSidebarCollapsed }"
        :connection-state="chatState.connectionState"
        :connection-text="chatState.connectionText"
        :online-count="chatState.onlineCount"
        :active-modals="modals"
        @open-modal="openModal"
        @toggle-sidebar="isSidebarCollapsed = !isSidebarCollapsed"
      />

      <!-- 主体内容区 -->
      <div class="main-content">
        <!-- 主体战斗区域 -->
        <div class="main-body">
      <!-- 左侧：人物信息 + 属性 -->
      <div class="left-panel">
        <!-- 人物信息区 -->
        <div class="player-info-section">
          <div class="player-avatar">
            <AssetIcon :source="player.avatarImagePath || player.avatar" :fallback="player.avatar" :alt="player.name" />
          </div>
          <div class="player-name-with-root">
            <span class="player-name">{{ player.name }}</span>
            <span v-if="player.currentTitle" class="player-title-chip">「{{ player.currentTitle }}」</span>
            <div
              ref="rootBadgeRef"
              class="spirit-root-badge"
              :class="playerSpiritRoot.type"
              @mouseenter="handleRootMouseEnter"
              @mouseleave="showRootTooltip = false"
            >
              <span class="root-badge-text">{{ playerSpiritRoot.name }}</span>
            </div>
          </div>

          <!-- 灵根 Tooltip - 使用 Teleport 传送到 body 避免遮挡 -->
          <Teleport to="body">
            <div
              v-if="showRootTooltip"
              class="root-tooltip-teleport"
              :style="{ left: tooltipPosition.x + 'px', top: tooltipPosition.y + 'px' }"
            >
              <div class="tooltip-title">
                <span class="root-icon">{{ playerSpiritRoot.icon }}</span>
                {{ playerSpiritRoot.name }}灵根
              </div>
              <div class="tooltip-subtitle">克制关系</div>
              <div class="counter-list">
                <div
                  v-for="counter in playerSpiritRoot.counters"
                  :key="counter.type"
                  class="counter-item"
                  :class="{ advantage: counter.value > 0, disadvantage: counter.value < 0 }"
                >
                  <span class="counter-icon">{{ counter.icon }}</span>
                  <span class="counter-name">{{ counter.name }}</span>
                  <span class="counter-value">{{ counter.value > 0 ? '+' : '' }}{{ counter.value }}%</span>
                </div>
              </div>
              <div class="tooltip-desc">{{ playerSpiritRoot.description }}</div>
            </div>
          </Teleport>
          <!-- 境界突破 -->
          <div class="breakthrough-info">
            <div class="breakthrough-row">
              <span class="breakthrough-label">当前境界</span>
              <span class="breakthrough-name">{{ player.currentRealmText }}</span>
            </div>
            <div class="breakthrough-row">
              <span class="breakthrough-label">下一境界</span>
              <span class="breakthrough-name">{{ player.nextRealmText }}</span>
            </div>
            <div v-if="player.breakthrough.isBreakthroughPoint" class="breakthrough-extra">
              <div class="breakthrough-meta">
                <span>成功率 {{ player.breakthrough.breakthroughSuccessRate }}%</span>
                <span>失败扣除 {{ player.breakthrough.breakthroughExpLossPercent }}% 当前经验</span>
              </div>
              <div v-if="player.breakthrough.requiredMaterials.length > 0" class="breakthrough-materials">
                <span
                  v-for="material in player.breakthrough.requiredMaterials"
                  :key="material.itemId"
                  class="breakthrough-material"
                  :class="{ lacking: !material.isEnough }"
                >
                  {{ material.name }} {{ material.ownedCount }}/{{ material.count }}
                </span>
              </div>
            </div>
          </div>
          <div class="realm-progress-panel">
            <div class="realm-progress-header">
              <span class="realm-progress-title">等级进度</span>
              <span class="realm-progress-values">{{ formatNumber(player.realmExp) }} / {{ formatNumber(player.maxRealmExp) }}</span>
            </div>
            <div class="realm-progress-track">
              <div class="realm-progress-fill" :style="{ width: player.expPercent + '%' }"></div>
            </div>
            <div class="realm-progress-meta">
              <span>当前经验 {{ formatNumber(player.realmExp) }}</span>
              <span>升级需求 {{ formatNumber(player.maxRealmExp) }}</span>
              <span>还差 {{ formatNumber(remainingLevelExp) }}</span>
            </div>
          </div>
          <!-- 血量/蓝量 -->
          <div class="resource-bars">
            <div class="resource-row hp">
              <div class="resource-row-head">
                <span class="resource-label">气血</span>
                <span class="resource-value">{{ formatNumber(player.hp) }}</span>
              </div>
              <div class="hp-bar">
                <div class="bar-fill" :style="{ width: player.hpPercent + '%' }"></div>
              </div>
            </div>
            <div class="resource-row mp">
              <div class="resource-row-head">
                <span class="resource-label">灵力</span>
                <span class="resource-value">{{ formatNumber(player.mp) }}</span>
              </div>
              <div class="mp-bar">
                <div class="bar-fill" :style="{ width: player.mpPercent + '%' }"></div>
              </div>
            </div>
          </div>

          <div class="cultivation-controls breakthrough-only">
            <div class="breakthrough-btn-wrap">
              <button class="breakthrough-btn" :disabled="!canBreakthrough" @click="attemptBreakthrough">
                <AssetIcon :source="ICON.misc_lightning" size="25" /> 突破
              </button>
              <div class="breakthrough-btn-tooltip">
                <div class="breakthrough-btn-tip-line">成功率 {{ player.breakthrough.breakthroughSuccessRate }}%</div>
                <div v-if="player.breakthrough.requiredMaterials.length > 0" class="breakthrough-btn-tip-title">需要材料</div>
                <div
                  v-for="material in player.breakthrough.requiredMaterials"
                  :key="`btn-${material.itemId}`"
                  class="breakthrough-btn-tip-line"
                  :class="{ lacking: !material.isEnough }"
                >
                  {{ material.name }} {{ material.ownedCount }}/{{ material.count }}
                </div>
                <div v-if="player.breakthrough.requiredMaterials.length === 0" class="breakthrough-btn-tip-line">
                  当前无需额外材料
                </div>
              </div>
            </div>
            <button class="pill-effect-btn" @click="showPillEffectModal = true">
              丹药效果
            </button>
          </div>
        </div>

        <!-- 装备栏 -->
        <div ref="equipmentSectionRef" class="equipment-section">
          <div class="section-title"><AssetIcon :source="ICON.misc_shield" size="25" /> 装备栏</div>
          <div class="equipment-grid">
            <div
              v-for="(slot, index) in equipmentSlots"
              :key="slot.type"
              :ref="el => { if(el) slotRefs[index] = el }"
              class="equipment-slot"
              :class="{ 'has-item': slot.item, [slot.item?.quality]: slot.item }"
              @mouseenter="handleSlotHover(slot, index)"
              @mouseleave="hoveredSlot = null"
            >
              <div v-if="slot.item?.isLocked" class="slot-bound-badge" title="已锁定"><AssetIcon :source="ICON.ui_lock" size="14" /></div>
              <div class="slot-icon">
                <AssetIcon :source="slot.item ? slot.item.icon : slot.icon" :fallback="slot.item ? ICON.misc_shield : slot.icon" :alt="slot.item ? slot.item.name : slot.name" size="40" />
              </div>
              <div class="slot-name">{{ slot.item ? slot.item.name : slot.name }}<span v-if="slot.item?.enhance > 0" class="slot-enhance">+{{ slot.item.enhance }}</span></div>
              <button
                v-if="slot.item"
                class="unequip-btn"
                @click.stop="unequipItem(slot.type)"
                title="卸下装备"
              >
                ✕
              </button>
            </div>
          </div>
          <!-- 装备 Tooltip - 使用 Teleport 传送到 body -->
          <Teleport to="body">
            <div
              v-if="hoveredSlot && hoveredSlot.item"
              class="equipment-tooltip-teleport"
              :style="{ left: slotTooltipPosition.x + 'px', top: slotTooltipPosition.y + 'px' }"
            >
              <!-- 装备名称及品质 -->
              <div class="tooltip-name" :class="hoveredSlot.item.quality">{{ hoveredSlot.item.name }}</div>
              <div class="tooltip-type">{{ hoveredSlot.item.typeText }}</div>

              <!-- 1. 基础属性 -->
              <div v-if="hoveredSlot.item.stats?.length" class="tooltip-section">
                <div class="tooltip-section-title">📊 基础属性</div>
                <div class="tooltip-stats-list">
                  <div v-for="(stat, idx) in hoveredSlot.item.stats" :key="idx" class="tooltip-stat-row">
                    <span class="stat-lbl">{{ stat.label || stat.name }}</span>
                    <span class="stat-val">{{ stat.displayValue || ((String(stat.value).startsWith('-') || String(stat.value).startsWith('+')) ? stat.value : `+${stat.value}`) }}</span>
                  </div>
                </div>
              </div>

              <!-- 2. 洗练属性 -->
              <div v-if="hoveredSlot.item.hasRerollStats && hoveredSlot.item.rerollStats?.length" class="tooltip-section reroll">
                <div class="tooltip-section-title">⚡ 洗练词条</div>
                <div class="tooltip-reroll-list">
                  <div
                    v-for="stat in hoveredSlot.item.rerollStats"
                    :key="'reroll-' + stat.index"
                    class="tooltip-reroll-row"
                    :style="{ borderLeftColor: stat.tierColor }"
                  >
                    <span class="reroll-desc-text" :style="{ color: stat.tierColor }">
                      {{ stat.description }}
                    </span>
                    <span class="reroll-tier-tag" :style="{ color: stat.tierColor }">
                      [{{ stat.tierName }}]
                    </span>
                    <span class="reroll-val-text">
                      {{ stat.isPercentage ? stat.value + '%' : '+' + stat.value }}
                    </span>
                  </div>
                </div>
              </div>

              <!-- 3. 宝石镶嵌 -->
              <div v-if="hoveredSlot.item.gemSlots?.some(s => s.gemId)" class="tooltip-section gems">
                <div class="tooltip-section-title">💎 宝石镶嵌</div>
                <div class="tooltip-gems-list">
                  <div
                    v-for="slot in hoveredSlot.item.gemSlots.filter(s => s.gemId)"
                    :key="'gem-' + slot.slotIndex"
                    class="tooltip-gem-row"
                  >
                    <span class="gem-name">💎 {{ slot.gemName || slot.gemId }}</span>
                    <span class="gem-bonus">{{ slot.bonusText }}</span>
                  </div>
                </div>
              </div>

              <!-- 描述 -->
              <div class="tooltip-desc-box">{{ hoveredSlot.item.description }}</div>
            </div>
          </Teleport>
        </div>
      </div>

      <!-- 右侧：战斗区域 -->
      <div class="battle-area">
        <!-- 战斗工具栏 -->
        <div class="battle-toolbar">
          <div class="battle-status">
            <span class="map-name"><AssetIcon :source="ICON.ui_location" size="20" /> {{ selectedDungeon.name || battleResult.mapName || '未选择地图' }}</span>
            <span class="round-info" v-if="battleResult.totalRounds > 0">共 {{ battleResult.totalRounds }} 回合</span>
            <span class="result-badge" :class="{ victory: battleResult.isVictory, defeat: !battleResult.isVictory }" v-if="battleResult.totalRounds > 0">
              {{ battleResult.isVictory ? '胜利' : '失败' }}
            </span>
            <span class="offline-info" v-if="isOfflineBattling">
              离线挂机中 · {{ offlineBattleStatus.mapName || selectedDungeon.name || '未知地图' }} · {{ offlineBattleStatus.totalBattles }} 场
            </span>
          </div>
          <div class="battle-toolbar-actions">
            <button class="battle-action-btn map" :disabled="isStarting || autoBattleEnabled || isOfflineBattling" @click="openMapSelector">
              <AssetIcon :source="ICON.misc_map" size="25" /> 选择地图
            </button>
            <button class="battle-action-btn start" :disabled="battleStartDisabled" @click="startBattle">
              <AssetIcon :source="ICON.stat_attack" size="25" /> {{ battleStartLabel }}
            </button>
            <button
              class="battle-action-btn offline"
              :class="{ stop: isOfflineBattling }"
              :disabled="isStarting"
              @click="toggleOfflineBattle"
            >
              {{ isOfflineBattling ? '停止离线' : '开始离线' }}
            </button>
            <button class="battle-action-btn map" :disabled="isOfflineBattling" @click="modals.worldBoss = true">
              世界Boss
            </button>
            <button class="battle-action-btn map" @click="modals.dungeonInstance = true">
              秘境
            </button>
          </div>
        </div>

        <!-- 顶部战斗信息区域：敌方、友方、战斗日志 -->
        <div class="battle-columns">
          <!-- 敌人区域 -->
          <div class="enemy-section">
            <div class="section-header">
              <span class="section-title"><AssetIcon :source="ICON.creature_demon" size="25" /> 敌方</span>
              <span class="unit-count">{{ battleUnits.enemies.filter(e => e.hp > 0).length }} / {{ battleUnits.enemies.length }}</span>
            </div>
            <div class="units-row">
              <div
                v-for="enemy in battleUnits.enemies"
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
              <div v-if="battleUnits.enemies.length === 0" class="no-units">暂无敌方数据</div>
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
              <span class="section-title"><AssetIcon :source="ICON.misc_shield" size="25" /> 友方</span>
              <span class="unit-count">{{ battleUnits.allies.filter(a => a.hp > 0).length }} / {{ battleUnits.allies.length }}</span>
            </div>
            <div class="units-row">
              <div
                v-for="ally in battleUnits.allies"
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
              <div v-if="battleUnits.allies.length === 0" class="no-units">暂无友方数据</div>
            </div>
          </div>

          <!-- 战斗日志 -->
          <div class="battle-log-section">
            <div class="log-header">
              <span class="section-title"><AssetIcon :source="ICON.misc_scroll" size="25" /> 战斗日志</span>
              <button class="clear-btn" @click="clearLog">清空</button>
            </div>
            <div ref="battleLogContainer" class="log-content">
              <div v-for="(log, index) in battleLogs" :key="index" class="log-entry" :class="log.type">
                <span class="log-time">[{{ log.time }}]</span>
                <span class="log-text" v-html="log.html || log.message"></span>
              </div>
              <div v-if="battleLogs.length === 0" class="empty-log">
                请选择地图后点击“开始战斗”。
              </div>
            </div>
          </div>
        </div>

        <!-- 底部功能区域：五行聚灵阵与世界聊天 -->
        <div class="chat-array-container" :style="{ '--bottom-panel-height': bottomPanelHeight + 'px' }">
          <FiveElementsArray class="five-elements-array-section" />

          <!-- 世界聊天 -->
          <div class="chat-section">
            <div class="chat-tabs">
              <button
                v-for="tab in chatTabs"
                :key="tab.key"
                class="tab-btn"
                :class="{ active: chatState.currentTab === tab.key }"
                @click="switchChatTab(tab.key)"
              >
                <span class="tab-icon"><AssetIcon :source="tab.icon" size="20" /></span>
                <span class="tab-name">{{ tab.name }}</span>
                <span v-if="chatUnread[tab.key] > 0" class="tab-unread-badge">{{ chatUnread[tab.key] }}</span>
              </button>
            </div>
            <div ref="chatMessagesContainer" class="chat-messages">
              <div
                v-for="msg in chatState.messages[chatState.currentTab]"
                :key="msg.messageId"
                class="chat-message"
                :class="{ system: msg.tab === 'system', self: msg.isSelf }"
              >
                <!-- Title: badge (text) or icon (image) -->
                <span v-if="msg.senderTitle" class="msg-title-wrapper">
                  <img v-if="msg.senderTitleIcon" :src="resolveTitleIcon(msg.senderTitleIcon)" class="msg-title-img" />
                  <span v-else class="msg-title-badge" :class="'rarity-' + (msg.senderTitleRarity || '').toLowerCase()">
                    {{ msg.senderTitle }}
                  </span>
                </span>

                <!-- Sender Name -->
                <span v-if="msg.sender" class="msg-sender">{{ msg.sender }}:</span>

                <!-- Content -->
                <span class="msg-content">{{ msg.content }}</span>

                <!-- Timestamp -->
                <span class="msg-time">{{ msg.time }}</span>
              </div>
              <div v-if="chatState.messages[chatState.currentTab].length === 0" class="empty-hint">暂无消息</div>
            </div>
            <div class="chat-input-area">
              <input
                v-model="chatState.input"
                type="text"
                class="chat-input"
                :placeholder="chatInputPlaceholder"
                :disabled="chatState.currentTab === 'system'"
                @keyup.enter="sendChatMessage"
                maxlength="200"
              />
              <button class="send-btn" @click="sendChatMessage" :disabled="chatState.currentTab === 'system' || !chatState.input.trim()">发送</button>
            </div>
          </div>
        </div>
      </div>
    </div>

      </div>
    </div>

    <!-- Modal 弹窗区域 -->
    <CheckInModal v-model="modals.checkIn" />
    <QuestModal v-model="modals.quest" />
    <AchievementModal v-model="modals.achievement" />
    <SectModal v-model="modals.sect" />
    <SpiritFieldModal v-model="modals.spiritField" />
    <RankingsModal v-model="modals.rankings" />
    <TeamModal v-model="modals.team" @party-battle="handlePartyBattle" />
    <RedeemModal v-model="modals.redeem" />
    <PetModal v-model="modals.pet" />
    <ShopModal v-model="modals.shop" />
    <ForgeModal v-model="modals.forge" />
    <AlchemyModal v-model="modals.alchemy" />
    <ChangelogModal v-model="modals.changelog" />
    <MapSelectModal v-model="modals.mapSelect" @enter-map="enterMap" />
    <InventoryModal v-model="modals.inventory" @open-gift-favorability="handleOpenGiftFromInventory" @open-gem-synth="handleOpenGemSynth" @open-forge="handleOpenForge" />
    <SkillModal v-model="modals.skill" />
    <WorldBossModal v-model="modals.worldBoss" />
    <LotteryModal v-model="modals.lottery" />
    <FavorabilityModal v-model="modals.favorability" @open-gift="handleOpenGift" />
    <FeedbackModal v-model="modals.feedback" />
    <GiftFavorabilityModal v-model="modals.giftFavorability" :target-player-id="giftTarget.playerId" :target-player-name="giftTarget.playerName" />
    <DungeonInstanceModal v-model="modals.dungeonInstance" />
    <MailModal v-model="modals.mail" />
    <TitleModal v-model="modals.title" />
    <ArenaModal v-model="modals.arena" :player-name="player.name || '我'" />
    <TowerModal v-model="modals.tower" />
    <GemSynthModal v-model="modals.gemSynth" :gem-id="gemSynthGemId" />
    <EquipmentForgeModal v-model="modals.equipmentForge" :equipment="forgeEquipment" />
    <MarketModal v-model="modals.market" />
    <PillEffectModal v-model="showPillEffectModal" />
    <HelpModal v-model="modals.help" />
    <SettingsModal v-model="modals.settings" />
    <AttributesModal
      v-model="modals.attributes"
      :available-points="availablePoints"
      :attributes="attributes"
      :combat-stats="combatStats"
      :player-spirit-root="playerSpiritRoot"
      :player="player"
      :is-adjusting-attribute-point="isAdjustingAttributePoint"
      @increase-attribute="increaseAttribute"
      @decrease-attribute="decreaseAttribute"
    />
  </div>
</template>

<script>
import { computed, nextTick, onMounted, onUnmounted, provide, reactive, ref, watch } from 'vue'
import { useRouter } from 'vue-router'

import AppSidebar from '../components/layout/AppSidebar.vue'
import FiveElementsArray from '../components/FiveElementsArray.vue'
import CheckInModal from '../components/modals/CheckInModal.vue'
import QuestModal from '../components/modals/QuestModal.vue'
import AchievementModal from '../components/modals/AchievementModal.vue'
import SectModal from '../components/modals/SectModal.vue'
import SpiritFieldModal from '../components/modals/SpiritFieldModal.vue'
import RankingsModal from '../components/modals/RankingsModal.vue'
import TeamModal from '../components/modals/TeamModal.vue'
import RedeemModal from '../components/modals/RedeemModal.vue'
import PetModal from '../components/modals/PetModal.vue'
import ShopModal from '../components/modals/ShopModal.vue'
import ForgeModal from '../components/modals/ForgeModal.vue'
import AlchemyModal from '../components/modals/AlchemyModal.vue'
import ChangelogModal from '../components/modals/ChangelogModal.vue'
import MapSelectModal from '../components/modals/MapSelectModal.vue'
import InventoryModal from '../components/modals/InventoryModal.vue'
import SkillModal from '../components/modals/SkillModal.vue'
import WorldBossModal from '../components/modals/WorldBossModal.vue'
import DungeonInstanceModal from '../components/modals/DungeonInstanceModal.vue'
import LotteryModal from '../components/modals/LotteryModal.vue'
import PillEffectModal from '../components/modals/PillEffectModal.vue'
import FavorabilityModal from '../components/modals/FavorabilityModal.vue'
import MailModal from '../components/modals/MailModal.vue'
import TitleModal from '../components/modals/TitleModal.vue'
import ArenaModal from '../components/modals/ArenaModal.vue'
import TowerModal from '../components/modals/TowerModal.vue'
import GemSynthModal from '../components/modals/GemSynthModal.vue'
import EquipmentForgeModal from '../components/modals/EquipmentForgeModal.vue'
import MarketModal from '../components/modals/MarketModal.vue'
import FeedbackModal from '../components/modals/FeedbackModal.vue'
import GiftFavorabilityModal from '../components/modals/GiftFavorabilityModal.vue'
import AttributesModal from '../components/modals/AttributesModal.vue'
import HelpModal from '../components/modals/HelpModal.vue'
import SettingsModal from '../components/modals/SettingsModal.vue'
import AssetIcon from '../components/common/AssetIcon.vue'
import { ICON } from '../icons'
import { useGameStore } from '../state/gameStore'
import { apiClient, buildApiUrl } from '../lib/apiClient'
import { getChatHubState, restartChatHub, sendChatHubMessage, startChatHub, stopChatHub, subscribeChatHub } from '../lib/chatHub'
import { startPartyHub, stopPartyHub, subscribePartyHub, syncPartyHubGroups } from '../lib/partyHub'
import { buildEquipmentSlots, buildPlayerView, formatCompactNumber, resolveBackendSlotValue } from '../services/gameDisplay'
import toast from '@/utils/toast'

/**
 * MainView - 游戏主页面（战斗布局）
 *
 * 这里保留原有的三栏布局和弹窗结构，
 * 重点改造的是页面底层数据流：
 * - 玩家信息改为真实后端玩家数据
 * - 装备栏改为真实已穿戴装备
 * - 副本挑战改为真实战斗接口
 * - 弹窗数据改为统一从全局游戏仓库读取
 */
export default {
  name: 'MainView',

  components: {
    AppSidebar,
    FiveElementsArray,
    CheckInModal,
    QuestModal,
    AchievementModal,
    SectModal,
    SpiritFieldModal,
    RankingsModal,
    TeamModal,
    RedeemModal,
    PetModal,
    ShopModal,
    ForgeModal,
    AlchemyModal,
    ChangelogModal,
    MapSelectModal,
    InventoryModal,
    SkillModal,
    WorldBossModal,
    DungeonInstanceModal,
    LotteryModal,
    PillEffectModal,
    FavorabilityModal,
    GiftFavorabilityModal,
    MailModal,
    TitleModal,
    ArenaModal,
    TowerModal,
    GemSynthModal,
    EquipmentForgeModal,
    MarketModal,
    FeedbackModal,
    AttributesModal,
    HelpModal,
    SettingsModal,
    AssetIcon
  },

  setup() {
    const router = useRouter()
    const gameStore = useGameStore()

    const isSidebarCollapsed = ref(true)
    const equipmentSectionRef = ref(null)
    const bottomPanelHeight = ref(350)
    let equipmentResizeObserver = null

    const syncBottomPanelHeight = () => {
      const equipmentHeight = equipmentSectionRef.value?.getBoundingClientRect().height
      if (equipmentHeight && equipmentHeight > 0) {
        bottomPanelHeight.value = Math.round(equipmentHeight)
      }
    }

    const toggleSidebar = () => {
      isSidebarCollapsed.value = !isSidebarCollapsed.value
    }

    const toggleTheme = () => {
      const themes = ['dark', 'light', 'elegant']
      const currentTheme = document.documentElement.getAttribute('data-theme') || 'light'
      const currentIndex = themes.indexOf(currentTheme)
      const nextTheme = themes[(currentIndex + 1) % themes.length]
      document.documentElement.setAttribute('data-theme', nextTheme)
      localStorage.setItem('xiuxian-theme', nextTheme)
    }

    const logout = async () => {
      await gameStore.logout('已退出当前账号。')
      router.replace('/')
    }

    // 所有弹窗统一集中在一个响应式对象里管理，
    // 底部快捷栏和页面内按钮都只需要改对应布尔值即可。
    const modals = reactive({
      checkIn: false,
      quest: false,
      achievement: false,
      sect: false,
      spiritField: false,
      rankings: false,
      team: false,
      redeem: false,
      pet: false,
      shop: false,
      forge: false,
      alchemy: false,
      changelog: false,
      mapSelect: false,
      inventory: false,
      skill: false,
      worldBoss: false,
      lottery: false,
      favorability: false,
      giftFavorability: false,
      dungeonInstance: false,
      mail: false,
      title: false,
      arena: false,
      tower: false,
      gemSynth: false,
      equipmentForge: false,
      market: false,
      feedback: false,
      attributes: false,
      help: false,
      settings: false
    })

    const giftTarget = reactive({ playerId: '', playerName: '' })

    function handleOpenGift(item) {
      giftTarget.playerId = item.playerId
      giftTarget.playerName = item.playerName
      modals.giftFavorability = true
    }

    function handleOpenGiftFromInventory() {
      giftTarget.playerId = ''
      giftTarget.playerName = ''
      modals.giftFavorability = true
    }

    const showPillEffectModal = ref(false)
    const gemSynthGemId = ref('')

    function handleOpenGemSynth(gemId) {
      gemSynthGemId.value = gemId || ''
      modals.gemSynth = true
    }

    const forgeEquipment = ref(null)

    function handleOpenForge(equipment) {
      forgeEquipment.value = equipment
      modals.equipmentForge = true
    }

    // 玩家面板使用一个“可变视图模型”承接后端人物数据，
    // 这样模板里就不需要频繁判断大小写字段或空值。
    const player = reactive(buildPlayerView(null))

    const formatNumber = (num) => formatCompactNumber(num)
    const formatAttributeRatio = (value) => {
      if (value === null || value === undefined || value === '') {
        return '-'
      }
      const numericValue = Number(value) || 0
      return Number.isInteger(numericValue) ? numericValue : numericValue.toFixed(2).replace(/\.?0+$/, '')
    }
    const canBreakthrough = computed(() => Boolean(player.breakthrough?.canBreakthrough))
    const remainingLevelExp = computed(() => {
      const requiredExp = Number(player.maxRealmExp) || 0
      const currentExp = Number(player.realmExp) || 0
      return Math.max(0, requiredExp - currentExp)
    })

    // 突破按钮统一走这里：
    // 先拦截不满足条件的情况，再调用全局仓库里的真实突破接口。
    const attemptBreakthrough = async () => {
      if (!player.breakthrough?.canBreakthrough) {
        addLog(player.breakthrough?.requirementText || '当前尚未满足突破条件。', 'warning')
        return
      }

      try {
        const result = await gameStore.breakthrough()
        const breakthrough = result?.breakthrough || result?.Breakthrough || {}
        const succeeded = Boolean(result?.succeeded ?? result?.Succeeded)

        if (succeeded) {
          addLog(result?.message || result?.Message || `突破成功，已踏入 ${breakthrough.currentAlias || breakthrough.CurrentAlias || '新境界'}。`, 'success')
        } else {
          addLog(result?.message || result?.Message || '突破失败。', 'warning')
        }
      } catch (error) {
        toast.error(error.message || '突破失败。')
      }
    }

    // 中间战斗属性面板直接消费这个结构，
    // 每次刷新玩家数据后再整体同步一次，避免模板里重复做换算。
    const combatStats = reactive({
      phyAtk: 0,
      magAtk: 0,
      phyDef: 0,
      magDef: 0,
      speed: 0,
      hitRate: 0,
      dodgeRate: 0,
      critRate: 0,
      critDmg: 0,
      comboRate: 0,
      counterRate: 0,
      armorBreak: 0,
      bonusDmg: 0
    })

    // 灵根展示与 tooltip 状态单独拆出来，
    // 这样左侧人物信息区的鼠标交互不会污染主 player 结构。
    const playerSpiritRoot = reactive({
      type: 'none',
      name: '无',
      icon: '○',
      description: '尚未觉醒灵根。',
      counters: []
    })

    const showRootTooltip = ref(false)
    const rootBadgeRef = ref(null)
    const tooltipPosition = reactive({ x: 0, y: 0 })

    // 鼠标进入灵根徽章时，动态计算 tooltip 在页面中的锚点位置。
    const handleRootMouseEnter = () => {
      if (rootBadgeRef.value) {
        const rect = rootBadgeRef.value.getBoundingClientRect()
        tooltipPosition.x = rect.right + 8
        tooltipPosition.y = rect.top
      }
      showRootTooltip.value = true
    }

    // 把后端 SpiritRoot DTO 统一映射成当前页面的灵根展示结构。
    const applyPlayerSpiritRoot = (sourcePlayer) => {
      const spiritRoot = sourcePlayer?.spiritRoot || sourcePlayer?.SpiritRoot || {}
      playerSpiritRoot.type = spiritRoot.type || spiritRoot.Type || 'none'
      playerSpiritRoot.name = spiritRoot.name || spiritRoot.Name || '无'
      playerSpiritRoot.icon = spiritRoot.icon || spiritRoot.Icon || '○'
      playerSpiritRoot.description = spiritRoot.description || spiritRoot.Description || '暂无灵根描述。'
      playerSpiritRoot.counters = Array.isArray(spiritRoot.counters || spiritRoot.Counters)
        ? [...(spiritRoot.counters || spiritRoot.Counters)]
        : []
    }

    // 属性点区域使用固定槽位列表承接后端配置，
    // 这样职业切换后仍能保持同一套展示结构，只改名称和整数收益。
    const availablePoints = ref(0)
    const isAdjustingAttributePoint = ref(false)
    const attributes = reactive([
      { type: 'type1', name: '血量', current: 0, base: 0, invested: 0, bonusValue: 0, bonusPerPoint: null, pointsPerBonus: 1 },
      { type: 'type2', name: '蓝量', current: 0, base: 0, invested: 0, bonusValue: 0, bonusPerPoint: null, pointsPerBonus: 1 },
      { type: 'type3', name: '物攻', current: 0, base: 0, invested: 0, bonusValue: 0, bonusPerPoint: null, pointsPerBonus: 1 },
      { type: 'type4', name: '法攻', current: 0, base: 0, invested: 0, bonusValue: 0, bonusPerPoint: null, pointsPerBonus: 1 },
      { type: 'type5', name: '物防', current: 0, base: 0, invested: 0, bonusValue: 0, bonusPerPoint: null, pointsPerBonus: 1 },
      { type: 'type6', name: '法防', current: 0, base: 0, invested: 0, bonusValue: 0, bonusPerPoint: null, pointsPerBonus: 1 },
      { type: 'type7', name: '速度', current: 0, base: 0, invested: 0, bonusValue: 0, bonusPerPoint: null, pointsPerBonus: 1 }
    ])

    const getPlayerBaseAttributeValue = (latestPlayer, type) => {
      const level = Math.max(1, Number(latestPlayer.level) || 1)

      switch (type) {
        case 'type1':
          return 500 + level * 50
        case 'type2':
          return 200 + level * 20
        case 'type3':
          return 50 + level * 8
        case 'type4':
          return 50 + level * 8
        case 'type5':
          return 30 + level * 5
        case 'type6':
          return 30 + level * 5
        case 'type7':
          return 20 + level * 2
        default:
          return 0
      }
    }

    const getPlayerCurrentAttributeValue = (latestPlayer, type) => {
      switch (type) {
        case 'type1':
          return latestPlayer.maxHp
        case 'type2':
          return latestPlayer.maxMp
        case 'type3':
          return latestPlayer.attack
        case 'type4':
          return latestPlayer.magicAttack
        case 'type5':
          return latestPlayer.defense
        case 'type6':
          return latestPlayer.magicDefense
        case 'type7':
          return latestPlayer.speed
        default:
          return 0
      }
    }

    const syncAttributePointState = (sourcePlayer, latestPlayer) => {
      const overview = sourcePlayer?.attributePoints || sourcePlayer?.AttributePoints || latestPlayer?.attributePoints || {}
      const overviewItems = Array.isArray(overview.attributes || overview.Attributes)
        ? (overview.attributes || overview.Attributes)
        : []

      availablePoints.value = Number(overview.availablePoints ?? overview.AvailablePoints ?? 0) || 0

      attributes.forEach((attr) => {
        const matched = overviewItems.find((item) => {
          const key = (item.key || item.Key || '').toLowerCase()
          return key === attr.type
        })

        const baseValue = getPlayerBaseAttributeValue(latestPlayer, attr.type)
        const currentValue = getPlayerCurrentAttributeValue(latestPlayer, attr.type)
        attr.base = baseValue
        attr.current = currentValue
        attr.name = matched?.name ?? matched?.Name ?? attr.name
        attr.invested = Number(matched?.allocatedPoints ?? matched?.AllocatedPoints ?? 0) || 0
        attr.bonusValue = Number(matched?.bonusValue ?? matched?.BonusValue ?? 0) || 0
        const nextBonus = matched?.bonusPerPoint ?? matched?.BonusPerPoint
        attr.bonusPerPoint = nextBonus === null || nextBonus === undefined || nextBonus === ''
          ? null
          : Number(nextBonus)
        attr.pointsPerBonus = Math.max(1, Number(matched?.pointsPerBonus ?? matched?.PointsPerBonus ?? 1) || 1)
      })
    }

    const increaseAttribute = async (type) => {
      if (availablePoints.value <= 0 || isAdjustingAttributePoint.value) return

      const attr = attributes.find((item) => item.type === type)
      if (!attr) return

      try {
        isAdjustingAttributePoint.value = true
        await gameStore.allocateAttributePoint(type)
        addLog(`${attr.name} 加点成功。`, 'success')
      } catch (error) {
        addLog(error.message || '属性加点失败。', 'fail')
        toast.error(error.message || '属性加点失败。')
      } finally {
        isAdjustingAttributePoint.value = false
      }
    }

    const decreaseAttribute = async (type) => {
      if (isAdjustingAttributePoint.value) return

      const attr = attributes.find((item) => item.type === type)
      if (!attr || attr.invested <= 0) return

      try {
        isAdjustingAttributePoint.value = true
        await gameStore.refundAttributePoint(type)
        addLog(`${attr.name} 已返还 1 点。`, 'warning')
      } catch (error) {
        addLog(error.message || '返还属性点失败。', 'fail')
        toast.error(error.message || '返还属性点失败。')
      } finally {
        isAdjustingAttributePoint.value = false
      }
    }

    const hoveredSlot = ref(null)
    const slotRefs = ref([])
    const slotTooltipPosition = reactive({ x: 0, y: 0 })
    const equipmentSlots = reactive(buildEquipmentSlots([]))

    const handleSlotHover = (slot, index) => {
      hoveredSlot.value = slot
      const element = slotRefs.value[index]
      if (!element) return
      const rect = element.getBoundingClientRect()
      slotTooltipPosition.x = rect.right + 8
      slotTooltipPosition.y = rect.top
    }

    const applyEquipmentSlots = (equippedItems) => {
      const latestSlots = buildEquipmentSlots(equippedItems)
      equipmentSlots.splice(0, equipmentSlots.length, ...latestSlots)
    }

    const unequipItem = async (slotType) => {
      const slot = equipmentSlots.find((item) => item.type === slotType)
      if (!slot?.item) {
        return
      }

      try {
        await gameStore.unequipItem(resolveBackendSlotValue(slotType))
        addLog(`已卸下 ${slot.item.name}`, 'warning')
      } catch (error) {
        addLog(error.message || '卸下装备失败。', 'fail')
        toast.error(error.message || '卸下装备失败')
      }
    }

    const battleLogs = reactive([])
    const battleLogContainer = ref(null)
    const isReplaying = ref(false)
    const isStarting = ref(false)
    const autoBattleEnabled = ref(false)
    let autoBattleTimer = null
    const battleCooldownRemaining = ref(0)
    const battleCooldownUntilUtc = ref('')
    let battleCooldownTimer = null
    const currentRound = ref(0)
    const replayInterval = ref(null)
    const replayTick = ref(null)
    const replaySpeed = ref(320)
    const normalizeOfflineBattleStatus = (source) => ({
      isOfflineBattling: Boolean(source?.isOfflineBattling ?? source?.IsOfflineBattling),
      mapId: source?.mapId ?? source?.MapId ?? '',
      mapName: source?.mapName ?? source?.MapName ?? '',
      startedAtUtc: source?.startedAtUtc ?? source?.StartedAtUtc ?? '',
      lastTickAtUtc: source?.lastTickAtUtc ?? source?.LastTickAtUtc ?? '',
      totalBattles: Number(source?.totalBattles ?? source?.TotalBattles ?? 0) || 0,
      winBattles: Number(source?.winBattles ?? source?.WinBattles ?? 0) || 0,
      totalRounds: Number(source?.totalRounds ?? source?.TotalRounds ?? 0) || 0
    })
    const offlineBattleStatus = computed(() => normalizeOfflineBattleStatus(gameStore.state.offlineBattleStatus))
    const isOfflineBattling = computed(() => offlineBattleStatus.value.isOfflineBattling)
    const battleStartDisabled = computed(() => {
      if (isOfflineBattling.value) {
        return true
      }

      return autoBattleEnabled.value ? false : (isStarting.value || battleCooldownRemaining.value > 0)
    })
    const battleStartLabel = computed(() => {
      if (isOfflineBattling.value) {
        return '离线挂机中'
      }

      if (autoBattleEnabled.value) {
        if (isStarting.value) {
          return '循环战斗中...'
        }

        if (battleCooldownRemaining.value > 0) {
          return `停止循环 (${battleCooldownRemaining.value}s)`
        }

        return '停止循环'
      }

      if (isStarting.value) {
        return '战斗中...'
      }

      if (battleCooldownRemaining.value > 0) {
        return `冷却中 ${battleCooldownRemaining.value}s`
      }

      return '开始战斗'
    })
    const selectedDungeon = reactive({
      id: '',
      name: '',
      raw: null,
      canChallenge: false,
      unavailableReason: ''
    })

    const battleUnits = reactive({
      enemies: [],
      allies: []
    })
    const hoveredEnemyTooltip = ref(null)
    const enemyTooltipPosition = reactive({ x: 0, y: 0 })

    const battleResult = reactive({
      mapName: '',
      isVictory: false,
      totalRounds: 0,
      stages: [],
      fighterStats: {}
    })
    const processedPartyBattleIds = new Set()

    const chatState = reactive({
      currentTab: 'world',
      input: '',
      onlineCount: 0,
      realtimeReady: false,
      connectionState: 'offline',
      connectionText: '未连接',
      messages: {
        world: [],
        sect: [],
        system: []
      }
    })
    const loadedChatTabs = reactive({
      world: false,
      sect: false,
      system: false
    })
    const chatUnread = reactive({
      world: 0,
      sect: 0,
      system: 0
    })
    const chatMessageIds = new Set()
    const chatMessagesContainer = ref(null)
    let unsubscribeChatHub = null
    let unsubscribePartyHub = null

    const chatTabs = [
      { key: 'world', name: '世界聊天', icon: ICON.misc_globe },
      { key: 'sect', name: '宗门聊天', icon: ICON.nav_sect },
      { key: 'system', name: '系统公告', icon: ICON.misc_megaphone }
    ]
    const chatInputPlaceholder = computed(() => {
      if (chatState.currentTab === 'system') {
        return '系统公告频道只读'
      }

      if (chatState.currentTab === 'sect' && !player.guildId) {
        return '加入宗门后才能在此发言'
      }

      return '输入消息...'
    })

    const scrollChatToBottom = (force = false) => {
      nextTick(() => {
        const container = chatMessagesContainer.value
        if (!container) {
          return
        }

        const remaining = container.scrollHeight - container.scrollTop - container.clientHeight
        if (force || remaining < 80) {
          container.scrollTop = container.scrollHeight
        }
      })
    }

    const normalizeChatTab = (channelType) => {
      const normalized = String(channelType || '').toLowerCase()
      if (normalized === 'sect' || normalized === 'guild') return 'sect'
      if (normalized === 'system' || normalized === 'notice') return 'system'
      return 'world'
    }

    const RARITY_LABELS = {
      Common: '普通',
      Rare: '稀有',
      Epic: '史诗',
      Legendary: '传说'
    }

    const resolveTitleIcon = (path) => {
      if (!path) return ''
      if (/^https?:\/\//i.test(path) || path.startsWith('data:image/')) return path
      return buildApiUrl(path.startsWith('/') ? path : `/${path}`)
    }

    const normalizeChatMessage = (message) => {
      const tab = normalizeChatTab(message.channelType || message.ChannelType)
      const sendTime = message.sendTime || message.SendTime || Date.now()
      const sender = message.senderName || message.SenderName || (tab === 'system' ? '系统' : '')
      const content = message.content || message.Content || ''
      const messageId = message.messageId || message.MessageId || `${tab}-${sender}-${sendTime}-${content}`
      const senderTitle = message.senderTitle || message.SenderTitle || null
      const senderTitleRarity = message.senderTitleRarity || message.SenderTitleRarity || null
      const senderTitleIcon = message.senderTitleIcon || message.SenderTitleIcon || null

      return {
        messageId,
        tab,
        time: new Date(sendTime).toLocaleTimeString('zh-CN', { hour12: false }),
        sender,
        content,
        isSelf: (message.senderName || message.SenderName || '') === player.name,
        senderTitle,
        senderTitleRarity,
        senderTitleIcon,
        senderTitleLabel: senderTitle ? RARITY_LABELS[senderTitleRarity] || senderTitleRarity : null
      }
    }

    const appendChatMessage = (rawMessage) => {
      const message = normalizeChatMessage(rawMessage)
      if (chatMessageIds.has(message.messageId)) {
        return
      }

      chatMessageIds.add(message.messageId)
      chatState.messages[message.tab].push(message)
      if (chatState.messages[message.tab].length > 80) {
        chatState.messages[message.tab].shift()
      }

      if (message.tab === chatState.currentTab) {
        scrollChatToBottom()
        return
      }

      chatUnread[message.tab] += 1
    }

    const connectChatHub = async () => {
      if (chatState.realtimeReady) {
        return
      }

      try {
        await startChatHub()
      } catch (error) {
        chatState.connectionState = 'offline'
        chatState.connectionText = '实时未连接'
        addLog(error.message || '聊天实时连接失败，将继续使用普通接口。', 'warning')
      }
    }

    const loadChatHistory = async (tab, force = false) => {
      if (!force && loadedChatTabs[tab]) {
        return
      }

      try {
        const messages = await apiClient.getChatHistory(tab, 50)
        chatState.messages[tab] = (messages || []).map((message) => {
          const normalized = normalizeChatMessage(message)
          chatMessageIds.add(normalized.messageId)
          return normalized
        })
        loadedChatTabs[tab] = true
        if (tab === chatState.currentTab) {
          scrollChatToBottom(true)
        }
      } catch (error) {
        addLog(error.message || '加载聊天记录失败。', 'fail')
      }
    }

    const switchChatTab = async (tab) => {
      if (tab === 'sect' && !player.guildId) {
        addLog('加入公会后才能访问宗门频道。', 'warning')
        return
      }

      chatState.currentTab = tab
      chatUnread[tab] = 0
      await loadChatHistory(tab)
      scrollChatToBottom(true)
    }

    const sendChatMessage = async () => {
      const content = chatState.input.trim()
      if (!content) return
      if (chatState.currentTab === 'system') {
        addLog('系统公告频道只读，不能主动发送消息。', 'warning')
        return
      }

      try {
        let message = null
        if (chatState.realtimeReady) {
          message = await sendChatHubMessage(chatState.currentTab, content)
        } else {
          message = await apiClient.sendChatMessage({
            channelType: chatState.currentTab,
            content
          })
        }

        appendChatMessage(message || {
          channelType: chatState.currentTab,
          senderName: player.name,
          content
        })
        chatState.input = ''
        scrollChatToBottom(true)
      } catch (error) {
        addLog(error.message || '发送聊天消息失败。', 'fail')
      }
    }

    const escapeHtml = (str) => {
      return String(str).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;')
    }

    const formatLogHtml = (message) => {
      if (!message) return ''
      let text = escapeHtml(message)

      // 1. 技能名/Buff名 [XX] → 金色
      text = text.replace(/\[([^\]]+)\]/g, '<span class="log-hl-skill">[$1]</span>')

      // 2. 伤害数值：造成 N 点伤害 / 造成 N（暴击） 点伤害 / 造成总计 x、y（暴击）、z 点伤害
      text = text.replace(/(造成(?:总计\s*)?)([\d、（）\u4e00-\u9fff\s]+?)(\s*点(?:伤害|(?:暴击)?伤害))/g, (_, pre, num, suf) => {
        return pre + '<span class="log-hl-damage">' + num.trim() + '</span>' + suf
      })
      text = text.replace(/(追加造成\s*)([\d（）\u4e00-\u9fff]+?)(\s*点)/g, '$1<span class="log-hl-damage">$2</span>$3')
      text = text.replace(/(损失了?\s*)(\d+)(\s*点)/g, '$1<span class="log-hl-damage">$2</span>$3')
      text = text.replace(/(吸收了\s*)(\d+)(\s*点)/g, '$1<span class="log-hl-damage">$2</span>$3')
      text = text.replace(/(分担了\s*)(\d+)(\s*点)/g, '$1<span class="log-hl-damage">$2</span>$3')
      text = text.replace(/(反弹了\s*)(\d+)(\s*点)/g, '$1<span class="log-hl-damage">$2</span>$3')

      // 3. 治疗数值：恢复 N 点
      text = text.replace(/(恢复了?\s*)(\d+)(\s*点)/g, '$1<span class="log-hl-heal">$2</span>$3')

      // 4. 剩余气血/血量数值
      text = text.replace(/(剩余[气血量]+\s*)(\d+)/g, '$1<span class="log-hl-damage">$2</span>')

      // 5. 角色名高亮 — 收集当前所有已知角色名
      const allyNames = battleUnits.allies.map(u => u.name).filter(Boolean)
      const enemyNames = battleUnits.enemies.map(u => u.name).filter(Boolean)
      // 按名字长度降序排列，避免短名字先匹配导致长名字被截断
      const allNames = [
        ...allyNames.map(n => ({ name: n, cls: 'log-hl-ally' })),
        ...enemyNames.map(n => ({ name: n, cls: 'log-hl-enemy' }))
      ].sort((a, b) => b.name.length - a.name.length)

      // 用占位符替换，避免嵌套替换破坏 HTML 标签
      const placeholders = []
      for (const { name, cls } of allNames) {
        const escaped = escapeHtml(name).replace(/[.*+?^${}()|[\]\\]/g, '\\$&')
        text = text.replace(new RegExp(escaped, 'g'), () => {
          const idx = placeholders.length
          placeholders.push(`<span class="${cls}">${escapeHtml(name)}</span>`)
          return `\x00PH${idx}\x00`
        })
      }
      // 还原占位符
      text = text.replace(/\x00PH(\d+)\x00/g, (_, idx) => placeholders[Number(idx)])

      return text
    }

    const addLog = (message, type = 'normal') => {
      const now = new Date()
      const time = now.toLocaleTimeString('zh-CN', { hour12: false })
      battleLogs.push({ time, message, type, html: formatLogHtml(message) })

      if (battleLogs.length > 100) {
        battleLogs.shift()
      }

      nextTick(() => {
        if (battleLogContainer.value) {
          battleLogContainer.value.scrollTop = battleLogContainer.value.scrollHeight
        }
      })
    }

    const sanitizeStoreFeedback = (message, fallback = '') => {
      const text = String(message || '').trim()
      if (!text) {
        return fallback
      }

      // 中文注释：
      // 之前有一轮错误写入把部分中文提示直接写成了连续问号。
      // 这里在主战斗页做一层只读兜底，不去篡改后端数据本身，
      // 只是在把全局提示写入战斗日志前先拦掉这些坏字符串，避免再次污染日志区域。
      if (/^\?+$/.test(text) || text.includes('????')) {
        return fallback
      }

      return text
    }

    const clearLog = () => {
      battleLogs.length = 0
    }

    const clearAutoBattleTimer = () => {
      if (autoBattleTimer) {
        clearTimeout(autoBattleTimer)
        autoBattleTimer = null
      }
    }

    const stopAutoBattle = (message = '') => {
      autoBattleEnabled.value = false
      clearAutoBattleTimer()
      if (message) {
        addLog(message, 'warning')
      }
    }

    const scheduleAutoBattleTick = (delay = 400) => {
      clearAutoBattleTimer()
      if (!autoBattleEnabled.value) {
        return
      }

      autoBattleTimer = window.setTimeout(async () => {
        if (!autoBattleEnabled.value) {
          return
        }

        if (isOfflineBattling.value) {
          stopAutoBattle('当前账号已切换到离线挂机。')
          return
        }

        if (!selectedDungeon.id || !selectedDungeon.canChallenge) {
          stopAutoBattle(selectedDungeon.unavailableReason || '当前地图无法继续循环战斗。')
          return
        }

        if (isStarting.value || isReplaying.value || battleCooldownRemaining.value > 0) {
          scheduleAutoBattleTick(500)
          return
        }

        await runBattleLoopOnce()
      }, delay)
    }

    const stopBattleCooldownTimer = () => {
      if (battleCooldownTimer) {
        clearInterval(battleCooldownTimer)
        battleCooldownTimer = null
      }
    }

    const refreshBattleCooldownRemaining = () => {
      if (!battleCooldownUntilUtc.value) {
        battleCooldownRemaining.value = 0
        stopBattleCooldownTimer()
        return
      }

      const targetTime = Date.parse(battleCooldownUntilUtc.value)
      if (!Number.isFinite(targetTime)) {
        battleCooldownRemaining.value = 0
        battleCooldownUntilUtc.value = ''
        stopBattleCooldownTimer()
        return
      }

      const remaining = Math.max(0, Math.ceil((targetTime - Date.now()) / 1000))
      battleCooldownRemaining.value = remaining

      if (remaining <= 0) {
        battleCooldownUntilUtc.value = ''
        stopBattleCooldownTimer()
      }
    }

    const startBattleCooldownTimer = () => {
      stopBattleCooldownTimer()
      refreshBattleCooldownRemaining()
      if (battleCooldownRemaining.value > 0) {
        battleCooldownTimer = window.setInterval(refreshBattleCooldownRemaining, 1000)
      }
    }

    const applyBattleCooldown = (source) => {
      const cooldownUntil = source?.battleCooldownUntilUtc ?? source?.BattleCooldownUntilUtc ?? ''
      const cooldownSeconds = Number(source?.battleCooldownSeconds ?? source?.BattleCooldownSeconds ?? 0) || 0

      if (cooldownSeconds > 0) {
        battleCooldownUntilUtc.value = new Date(Date.now() + cooldownSeconds * 1000).toISOString()
        battleCooldownRemaining.value = cooldownSeconds
        startBattleCooldownTimer()
        return
      }

      if (cooldownUntil) {
        battleCooldownUntilUtc.value = cooldownUntil
        startBattleCooldownTimer()
        return
      }

      if (battleCooldownRemaining.value > 0) {
        return
      }

      battleCooldownUntilUtc.value = ''
      battleCooldownRemaining.value = 0
      stopBattleCooldownTimer()
    }

    const stopReplayTimer = () => {
      if (replayInterval.value) {
        clearInterval(replayInterval.value)
        replayInterval.value = null
      }
      replayTick.value = null
    }

    const resetBattleUnits = () => {
      battleUnits.allies.splice(0, battleUnits.allies.length)
      battleUnits.enemies.splice(0, battleUnits.enemies.length)
    }

    const normalizeFighterStates = (fighterStates) => {
      if (Array.isArray(fighterStates)) {
        return fighterStates.filter(Boolean)
      }

      if (fighterStates && typeof fighterStates === 'object') {
        return Object.values(fighterStates).filter(Boolean)
      }

      return []
    }

    const toNumber = (value, fallback = 0) => {
      const parsed = Number(value)
      return Number.isFinite(parsed) ? parsed : fallback
    }

    const readBattleValue = (source, ...keys) => {
      for (const key of keys) {
        const value = source?.[key]
        if (value !== undefined && value !== null) {
          return value
        }
      }

      return undefined
    }

    const findUnitIndex = (units, fighterId, fighterName) => {
      return units.findIndex((unit) => {
        if (fighterId && unit.id === fighterId) return true
        return fighterName && unit.name === fighterName
      })
    }

    const createUnitFromState = (state, defaultIsPlayerSide = false) => {
      const stateIsPlayerSide = readBattleValue(state, 'IsPlayerSide', 'isPlayerSide')
      const isPlayerSide = stateIsPlayerSide === undefined ? defaultIsPlayerSide : !!stateIsPlayerSide
      const fighterName = readBattleValue(state, 'FighterName', 'fighterName') || '未知单位'
      const fighterId = readBattleValue(state, 'FighterId', 'fighterId') || fighterName

      return {
        id: fighterId,
        name: fighterName,
        avatar: isPlayerSide ? ICON.misc_wizard : ICON.creature_demon,
        hp: toNumber(readBattleValue(state, 'CurrentHp', 'currentHp')),
        maxHp: toNumber(readBattleValue(state, 'MaxHp', 'maxHp')),
        mp: toNumber(readBattleValue(state, 'CurrentMp', 'currentMp')),
        maxMp: toNumber(readBattleValue(state, 'MaxMp', 'maxMp')),
        stats: state
      }
    }

    const isMeaningfulBattleStat = (value) => {
      const text = String(value ?? '').trim()
      if (!text) {
        return false
      }

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

      return {
        name: enemy?.name || '未知敌人',
        elementName,
        baseStats,
        advancedStats
      }
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

    const replaceBattleUnitsByStates = (fighterStates) => {
      const normalizedStates = normalizeFighterStates(fighterStates)
      if (normalizedStates.length === 0) {
        return
      }

      const nextAllies = []
      const nextEnemies = []

      normalizedStates.forEach((state) => {
        // 中文注释：
        // 后端正常情况下会明确给出 IsPlayerSide，但为了防止历史数据或异常日志缺失该字段，
        // 这里额外根据当前玩家自身信息做一次兜底推断，避免把我方单位误放进敌方区域。
        const fighterId = readBattleValue(state, 'FighterId', 'fighterId')
        const fighterName = readBattleValue(state, 'FighterName', 'fighterName')
        const stateIsPlayerSide = readBattleValue(state, 'IsPlayerSide', 'isPlayerSide')
        const inferredIsPlayerSide = stateIsPlayerSide === undefined
          ? fighterId === player.id || fighterName === player.name
          : !!stateIsPlayerSide

        const unit = createUnitFromState({
          ...state,
          IsPlayerSide: inferredIsPlayerSide
        }, inferredIsPlayerSide)

        if (inferredIsPlayerSide) {
          nextAllies.push(unit)
        } else {
          nextEnemies.push(unit)
        }
      })

      battleUnits.allies.splice(0, battleUnits.allies.length, ...nextAllies)
      battleUnits.enemies.splice(0, battleUnits.enemies.length, ...nextEnemies)
    }

    const upsertUnitFromState = (state) => {
      if (!state) return

      const fighterId = readBattleValue(state, 'FighterId', 'fighterId') || ''
      const fighterName = readBattleValue(state, 'FighterName', 'fighterName') || ''
      const allyIndex = findUnitIndex(battleUnits.allies, fighterId, fighterName)
      const enemyIndex = findUnitIndex(battleUnits.enemies, fighterId, fighterName)

      let isPlayerSide = readBattleValue(state, 'IsPlayerSide', 'isPlayerSide')
      if (isPlayerSide === undefined) {
        if (allyIndex >= 0) {
          isPlayerSide = true
        } else if (enemyIndex >= 0) {
          isPlayerSide = false
        }
      }

      const targetList = isPlayerSide ? battleUnits.allies : battleUnits.enemies
      const otherList = isPlayerSide ? battleUnits.enemies : battleUnits.allies
      const targetIndex = findUnitIndex(targetList, fighterId, fighterName)
      const otherIndex = findUnitIndex(otherList, fighterId, fighterName)

      let targetUnit = null
      if (targetIndex >= 0) {
        targetUnit = targetList[targetIndex]
      } else if (otherIndex >= 0) {
        targetUnit = otherList.splice(otherIndex, 1)[0]
        targetList.push(targetUnit)
      }

      if (!targetUnit) {
        targetList.push(createUnitFromState(state, !!isPlayerSide))
        return
      }

      targetUnit.id = fighterId || targetUnit.id
      targetUnit.name = fighterName || targetUnit.name
      targetUnit.avatar = isPlayerSide ? ICON.misc_wizard : ICON.creature_demon
      targetUnit.hp = toNumber(readBattleValue(state, 'CurrentHp', 'currentHp'), targetUnit.hp)
      targetUnit.maxHp = toNumber(readBattleValue(state, 'MaxHp', 'maxHp'), targetUnit.maxHp)
      targetUnit.mp = toNumber(readBattleValue(state, 'CurrentMp', 'currentMp'), targetUnit.mp)
      targetUnit.maxMp = toNumber(readBattleValue(state, 'MaxMp', 'maxMp'), targetUnit.maxMp)
      targetUnit.stats = state
    }

    const initializeBattleUnitsFromStage = (stage) => {
      resetBattleUnits()

      const rounds = stage?.roundLogs || []
      const initialRound = rounds.find((round) => Number(round.RoundNumber ?? round.roundNumber ?? 0) === 0) || rounds[0]
      const initialStates = normalizeFighterStates(initialRound?.FighterStates ?? initialRound?.fighterStates)

      if (initialStates.length > 0) {
        replaceBattleUnitsByStates(initialStates)
        return
      }

      battleUnits.allies.push({
        id: player.id || 'self',
        name: player.name || '未知修士',
        avatar: player.avatar,
        hp: player.hp,
        maxHp: player.maxHp,
        mp: player.mp,
        maxMp: player.maxMp
      })
    }

    const normalizeBattleStages = (result, mapName = '') => {
      const rawStages = result?.Stages || result?.stages || []
      if (Array.isArray(rawStages) && rawStages.length > 0) {
        return rawStages.map((stage, index) => ({
          stageIndex: Number(stage?.StageIndex ?? stage?.stageIndex ?? index + 1) || index + 1,
          mapName: stage?.MapName || stage?.mapName || mapName || result?.MapName || result?.mapName || `第 ${index + 1} 层`,
          isVictory: Boolean(stage?.IsVictory ?? stage?.isVictory ?? stage?.IsWin ?? stage?.isWin),
          totalRounds: Number(stage?.TotalRounds ?? stage?.totalRounds ?? stage?.Rounds ?? stage?.rounds ?? 0) || 0,
          roundLogs: stage?.RoundLogs || stage?.roundLogs || []
        }))
      }

      return [{
        stageIndex: 1,
        mapName: mapName || result?.MapName || result?.mapName || '未知地图',
        isVictory: Boolean(result?.IsVictory ?? result?.isVictory ?? result?.IsWin ?? result?.isWin),
        totalRounds: Number(result?.TotalRounds ?? result?.totalRounds ?? result?.Rounds ?? result?.rounds ?? 0) || 0,
        roundLogs: result?.RoundLogs || result?.roundLogs || []
      }]
    }

    const resolveBattleLogType = (line) => {
      const text = String(line || '')

      if (!text) {
        return 'normal'
      }

      if (text.includes('暴击')) {
        return 'crit'
      }

      if (text.includes('释放') || text.includes('技能')) {
        return 'skill'
      }

      if (text.includes('恢复') || text.includes('治疗')) {
        return 'heal'
      }

      if (text.includes('胜利') || text.includes('通关')) {
        return 'success'
      }

      if (text.includes('失败') || text.includes('阵亡') || text.includes('战败')) {
        return 'fail'
      }

      if (text.includes('回合') || text.includes('奖励')) {
        return 'warning'
      }

      if (text.includes('敌') || text.includes('妖')) {
        return 'enemy'
      }

      if (text.includes('攻击')) {
        return 'attack'
      }

      return 'normal'
    }

    const getEntryType = (type) => {
      const typeMap = {
        1: 'skill',
        2: 'attack',
        3: 'damage',
        6: 'warning',
        8: 'heal',
        9: 'heal',
        10: 'heal',
        11: 'buff',
        14: 'warning',
        15: 'warning',
        16: 'warning',
        17: 'warning',
        18: 'warning',
        19: 'warning',
        20: 'warning',
        21: 'counter',
        22: 'crit',
        23: 'damage',
        24: 'warning',
        25: 'warning',
        26: 'warning',
        27: 'fail',
        29: 'crit',
        30: 'success',
        31: 'success',
        34: 'warning'
      }
      return typeMap[type] || 'normal'
    }

    const getEntryDescription = (entry) => {
      const description = readBattleValue(entry, 'Description', 'description')
      if (description) {
        return description
      }

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

    const playEntry = (entry) => {
      const type = getEntryType(readBattleValue(entry, 'Type', 'type'))
      const message = getEntryDescription(entry)
      const shouldHideBattleEntry = String(message || '').includes('属性克制')
      if (!shouldHideBattleEntry) {
        addLog(message, type)
      }

      const entryStates = normalizeFighterStates(entry?.FighterStates ?? entry?.fighterStates)
      if (entryStates.length > 0) {
        entryStates.forEach((state) => {
          upsertUnitFromState(state)
        })
        return
      }

      const targetCurrentHp = readBattleValue(entry, 'TargetCurrentHp', 'targetCurrentHp')
      const targetName = readBattleValue(entry, 'TargetName', 'targetName')
      if (targetCurrentHp !== undefined && targetName) {
        const enemy = battleUnits.enemies.find((item) => item.name === targetName)
        if (enemy) {
          enemy.hp = Math.max(0, targetCurrentHp)
        }

        const ally = battleUnits.allies.find((item) => item.name === targetName)
        if (ally) {
          ally.hp = Math.max(0, targetCurrentHp)
        }
      }
    }

    const applyBattleResult = (result, mapName = '') => {
      const stages = normalizeBattleStages(result, mapName)
      const finalMapName = stages[0]?.mapName || mapName || selectedDungeon.name || battleResult.mapName || '未知地图'
      const totalRounds = Number(result?.TotalRounds ?? result?.totalRounds ?? result?.Rounds ?? result?.rounds ?? 0)
        || stages.reduce((sum, stage) => sum + Number(stage.totalRounds || 0), 0)

      battleResult.mapName = finalMapName
      battleResult.isVictory = Boolean(result?.IsVictory ?? result?.isVictory ?? result?.IsWin ?? result?.isWin)
      battleResult.totalRounds = totalRounds
      battleResult.stages = stages
      battleResult.fighterStats = {
        expGained: Number(result?.ExpGained ?? result?.expGained ?? 0) || 0,
        goldGained: Number(result?.GoldGained ?? result?.goldGained ?? 0) || 0,
        drops: result?.Drops || result?.drops || []
      }

      applyBattleCooldown(result)

      initializeBattleUnitsFromStage(stages[0])
    }

    const getCurrentPlayerId = () => {
      return player.id || gameStore.state.player?.id || gameStore.state.player?.Id || ''
    }

    const rememberPartyBattleId = (battleId) => {
      if (!battleId) {
        return false
      }

      if (processedPartyBattleIds.has(battleId)) {
        return true
      }

      processedPartyBattleIds.add(battleId)
      if (processedPartyBattleIds.size > 20) {
        const oldest = processedPartyBattleIds.values().next().value
        if (oldest) {
          processedPartyBattleIds.delete(oldest)
        }
      }

      return false
    }

    const buildBattlePlaybackQueue = () => {
      const queue = []

      battleResult.stages.forEach((stage, index) => {
        const stageNumber = Number(stage.stageIndex ?? index + 1) || index + 1
        const rounds = stage.roundLogs || []
        const initialRound = rounds.find((round) => Number(round.RoundNumber ?? round.roundNumber ?? 0) === 0) || rounds[0]
        const initialStates = normalizeFighterStates(initialRound?.FighterStates ?? initialRound?.fighterStates)

        queue.push({
          kind: 'stage-start',
          stageNumber,
          fighterStates: initialStates
        })

        rounds.forEach((round) => {
          const roundNumber = Number(round.RoundNumber ?? round.roundNumber ?? 0) || 0
          const roundStates = normalizeFighterStates(round?.FighterStates ?? round?.fighterStates)
          if (roundNumber > 0) {
            queue.push({
              kind: 'round-start',
              stageNumber,
              roundNumber,
              fighterStates: roundStates
            })
          }

          const entries = [...(round.Entries || round.entries || [])]
            .sort((left, right) => toNumber(left?.Timestamp ?? left?.timestamp) - toNumber(right?.Timestamp ?? right?.timestamp))
          entries.forEach((entry) => {
            queue.push({
              kind: 'entry',
              stageNumber,
              roundNumber,
              entry
            })
          })
        })

        queue.push({
          kind: 'stage-end',
          stageNumber,
          isVictory: stage.isVictory
        })
      })

      return queue
    }

    const startBattleLogPlayback = () => {
      stopReplayTimer()
      isReplaying.value = true
      currentRound.value = 0
      clearLog()

      addLog(`战斗开始：${battleResult.mapName}`, 'success')
      addLog(`总回合数：${battleResult.totalRounds || 0} 回合`, 'warning')
      addLog('', 'normal')

      const queue = buildBattlePlaybackQueue()
      const hasEntry = queue.some((item) => item.kind === 'entry')
      if (!hasEntry) {
        addLog('本次战斗没有可回放的日志条目。', 'warning')
        isReplaying.value = false
        return
      }

      let queueIndex = 0
      let lastStage = -1
      let lastRound = -1
      const showStageHeader = battleResult.stages.length > 1

      const playNextEntry = () => {
        if (queueIndex >= queue.length) {
          stopReplayTimer()
          isReplaying.value = false
          addLog('', 'normal')
          addLog(`战斗结束：${battleResult.isVictory ? '胜利' : '失败'}`, battleResult.isVictory ? 'success' : 'fail')
          addLog(`共进行 ${battleResult.totalRounds || currentRound.value || 0} 回合`, 'warning')

            const expGained = Number(battleResult.fighterStats.expGained || 0)
            const goldGained = Number(battleResult.fighterStats.goldGained || 0)
            const drops = Array.isArray(battleResult.fighterStats.drops) ? battleResult.fighterStats.drops : []
            if (expGained > 0 || goldGained > 0 || drops.length > 0) {
              const dropSummary = drops
                .map((drop) => {
                  const dropName = drop?.name || drop?.Name || drop?.itemId || drop?.ItemId || '未知掉落'
                  const quantity = Number(drop?.quantity ?? drop?.Quantity ?? 1) || 1
                  return quantity > 1 ? `${dropName} x${quantity}` : dropName
                })
                .join('、')
              const dropText = dropSummary ? `，掉落 ${dropSummary}` : ''
              addLog(`战斗奖励：${formatNumber(expGained)} 经验，${formatNumber(goldGained)} 金币${dropText}`, 'warning')
            }
            if (autoBattleEnabled.value) {
              scheduleAutoBattleTick(300)
            }
            return
        }

        const item = queue[queueIndex]
        queueIndex += 1

        if (item.kind === 'stage-start') {
          if (showStageHeader) {
            addLog(`━━ 第 ${item.stageNumber} 层：${battleResult.stages[item.stageNumber - 1]?.mapName || battleResult.mapName} ━━`, 'success')
          }

          if (Array.isArray(item.fighterStates) && item.fighterStates.length > 0) {
            replaceBattleUnitsByStates(item.fighterStates)
          }

          lastStage = item.stageNumber
          lastRound = -1
          return
        }

        if (item.kind === 'round-start') {
          addLog(`━━ 第 ${item.roundNumber} 回合 ━━`, 'success')
          currentRound.value = item.roundNumber
          lastRound = item.roundNumber

          // 中文注释：
          // 回合开始时优先用 RoundLog.FighterStates 整体覆盖一遍敌我单位。
          // 这样可以完全对齐老 main 的显示思路：日志往前走一条，角色面板就跟随最新快照，
          // 不会因为某条 Entry 只携带局部状态而让另一侧面板停留在旧血量。
          if (Array.isArray(item.fighterStates) && item.fighterStates.length > 0) {
            replaceBattleUnitsByStates(item.fighterStates)
          }
          return
        }

        if (item.kind === 'stage-end') {
          if (showStageHeader) {
            addLog(`第 ${item.stageNumber} 层${item.isVictory ? '通关' : '失败'}`, item.isVictory ? 'success' : 'fail')
          }
          return
        }

        if (item.stageNumber !== lastStage) {
          if (showStageHeader) {
            addLog(`━━ 第 ${item.stageNumber} 层：${battleResult.stages[item.stageNumber - 1]?.mapName || battleResult.mapName} ━━`, 'success')
          }
          lastStage = item.stageNumber
          lastRound = -1
        }

        if (item.roundNumber > 0 && item.roundNumber !== lastRound) {
          addLog(`━━ 第 ${item.roundNumber} 回合 ━━`, 'success')
          currentRound.value = item.roundNumber
          lastRound = item.roundNumber
        }

        playEntry(item.entry)
      }

      replayTick.value = playNextEntry
      playNextEntry()
      replayInterval.value = setInterval(playNextEntry, replaySpeed.value)
    }

    const openMapSelector = () => {
      modals.mapSelect = true
    }

    const formatOfflineDrops = (drops = []) => {
      if (!Array.isArray(drops) || drops.length === 0) {
        return '无'
      }

      return drops
        .map((drop) => `${drop?.name || drop?.Name || drop?.itemId || drop?.ItemId || '未知掉落'} x${Number(drop?.quantity ?? drop?.Quantity ?? 0) || 0}`)
        .join('，')
    }

    const formatOfflineBattleSummary = (summary = {}) => {
      const totalBattles = Number(summary?.totalBattles ?? summary?.TotalBattles ?? 0) || 0
      const winBattles = Number(summary?.winBattles ?? summary?.WinBattles ?? 0) || 0
      const winRate = Number(summary?.winRate ?? summary?.WinRate ?? 0) || 0
      const durationSeconds = Number(summary?.durationSeconds ?? summary?.DurationSeconds ?? 0) || 0
      const summaryMapName = summary?.mapName ?? summary?.MapName ?? ''
      const mapName = summaryMapName || selectedDungeon.name || '未知地图'
      const totalRounds = Number(summary?.totalRounds ?? summary?.TotalRounds ?? 0) || 0
      const expGained = Number(summary?.expGained ?? summary?.ExpGained ?? 0) || 0
      const goldGained = Number(summary?.goldGained ?? summary?.GoldGained ?? 0) || 0
      const itemDrops = summary?.itemDrops ?? summary?.ItemDrops ?? []
      const equipmentDrops = summary?.equipmentDrops ?? summary?.EquipmentDrops ?? []

      return [
        `挂机地图：${mapName}`,
        `挂机时长：${durationSeconds} 秒`,
        `总场次：${totalBattles}`,
        `胜场 / 胜率：${winBattles} / ${winRate}%`,
        `总回合：${totalRounds}`,
        `获得经验：${expGained}`,
        `获得金币：${goldGained}`,
        `道具掉落：${formatOfflineDrops(itemDrops)}`,
        `装备掉落：${formatOfflineDrops(equipmentDrops)}`
      ].join('\n')
    }

    const appendOfflineBattleSummaryLogs = (summary = {}) => {
      const totalBattles = Number(summary?.totalBattles ?? summary?.TotalBattles ?? 0) || 0
      const winBattles = Number(summary?.winBattles ?? summary?.WinBattles ?? 0) || 0
      const winRate = Number(summary?.winRate ?? summary?.WinRate ?? 0) || 0
      const durationSeconds = Number(summary?.durationSeconds ?? summary?.DurationSeconds ?? 0) || 0
      const expGained = Number(summary?.expGained ?? summary?.ExpGained ?? 0) || 0
      const goldGained = Number(summary?.goldGained ?? summary?.GoldGained ?? 0) || 0
      const itemDrops = summary?.itemDrops ?? summary?.ItemDrops ?? []
      const equipmentDrops = summary?.equipmentDrops ?? summary?.EquipmentDrops ?? []
      const summaryMapName = summary?.mapName ?? summary?.MapName ?? ''
      const mapName = summaryMapName || selectedDungeon.name || '未知地图'

      addLog(`离线挂机已停止：${mapName}`, 'warning')
      addLog(`挂机时长：${durationSeconds} 秒`, 'warning')
      addLog(`挂机统计：${totalBattles} 场，胜场 ${winBattles}，胜率 ${winRate}%。`, 'success')
      addLog(`挂机收益：经验 ${expGained}，金币 ${goldGained}。`, 'success')
      addLog(`道具掉落：${formatOfflineDrops(itemDrops)}`, 'success')
      addLog(`装备掉落：${formatOfflineDrops(equipmentDrops)}`, 'success')

      if (totalBattles > 0 && winBattles <= 0) {
        addLog('本次离线挂机未取得胜利场次。', 'warning')
      }
    }

    const runBattleLoopOnce = async () => {
      isStarting.value = true
      try {
        const partyId = gameStore.state.currentParty?.partyId || gameStore.state.currentParty?.PartyId
        const dungeonId = selectedDungeon.raw?.dungeonId || selectedDungeon.raw?.DungeonId
        const result = partyId && dungeonId
          ? await gameStore.challengePartyDungeon(dungeonId, partyId)
          : await gameStore.startMapBattle(selectedDungeon.id)
        applyBattleResult(result, selectedDungeon.name)
        isStarting.value = false
        await nextTick()
        startBattleLogPlayback()
      } catch (error) {
        const cooldownSeconds = Number(error?.data?.battleCooldownSeconds ?? error?.data?.BattleCooldownSeconds ?? 0) || 0
        if (error?.data) {
          applyBattleCooldown(error.data)
        }

        if (cooldownSeconds > 0 && !autoBattleEnabled.value) {
          addLog(error.message || `战斗冷却中，还需等待 ${cooldownSeconds} 秒。`, 'warning')
        } else {
          if (autoBattleEnabled.value) {
            stopAutoBattle()
          }
          addLog(error.message || '开始战斗失败。', 'fail')
          toast.error(error.message || '开始战斗失败。')
        }
      } finally {
        if (isStarting.value) {
          isStarting.value = false
        }

        if (autoBattleEnabled.value && !isReplaying.value && !isOfflineBattling.value) {
          scheduleAutoBattleTick(500)
        }
      }
    }

    const startBattle = async () => {
      if (isOfflineBattling.value) {
        addLog('当前账号正在离线挂机中，请先停止离线挂机。', 'warning')
        return
      }

      if (autoBattleEnabled.value) {
        stopAutoBattle('已停止循环战斗。')
        return
      }

      if (isStarting.value) return

      if (!selectedDungeon.id) {
        addLog('请先选择地图。', 'warning')
        modals.mapSelect = true
        return
      }

      if (!selectedDungeon.canChallenge) {
        addLog(selectedDungeon.unavailableReason || '当前地图暂时不可挑战。', 'warning')
        return
      }

      const party = gameStore.state.currentParty
      const partyId = party?.partyId || party?.PartyId
      const isPartyDungeon = Boolean(partyId && (selectedDungeon.raw?.dungeonId || selectedDungeon.raw?.DungeonId))
      const isLeader = Boolean(party?.isLeader ?? party?.IsLeader)
      if (isPartyDungeon && !isLeader) {
        addLog('只有队长可以开始组队副本。', 'warning')
        return
      }

      autoBattleEnabled.value = true
      addLog(`开始循环挑战 ${selectedDungeon.name}`, 'success')

      if (battleCooldownRemaining.value > 0) {
        scheduleAutoBattleTick(500)
        return
      }

      await runBattleLoopOnce()
    }

    let offlinePollingTimer = null

    const startOfflinePolling = () => {
      stopOfflinePolling()
      offlinePollingTimer = window.setInterval(async () => {
        if (!isOfflineBattling.value) {
          stopOfflinePolling()
          return
        }
        await gameStore.loadOfflineBattleStatus()
      }, 60000)
    }

    const stopOfflinePolling = () => {
      if (offlinePollingTimer) {
        window.clearInterval(offlinePollingTimer)
        offlinePollingTimer = null
      }
    }

    const toggleOfflineBattle = async () => {
      if (isOfflineBattling.value) {
        await stopOfflineBattle()
      } else {
        await startOfflineBattle()
      }
    }

    const startOfflineBattle = async () => {
      if (isStarting.value) {
        return
      }

      if (!selectedDungeon.id) {
        addLog('请先选择地图后再开始离线挂机。', 'warning')
        modals.mapSelect = true
        return
      }

      if (!selectedDungeon.canChallenge) {
        addLog(selectedDungeon.unavailableReason || '当前地图暂时不可挂机。', 'warning')
        return
      }

      stopReplayTimer()
      if (autoBattleEnabled.value) {
        stopAutoBattle('已停止循环战斗，准备切换到离线挂机。')
      }

      try {
        const status = await gameStore.startOfflineBattle(selectedDungeon.id)
        addLog(`已开始离线挂机：${status?.mapName || status?.MapName || selectedDungeon.name}`, 'success')
        addLog('页面内普通战斗已禁用，战斗推进改由后端托管。', 'warning')
        startOfflinePolling()
      } catch (error) {
        addLog(error.message || '开始离线挂机失败。', 'fail')
        toast.error(error.message || '开始离线挂机失败。')
      }
    }

    const stopOfflineBattle = async () => {
      if (!isOfflineBattling.value) {
        return
      }

      try {
        const summary = await gameStore.stopOfflineBattle()
        stopOfflinePolling()
        appendOfflineBattleSummaryLogs(summary)
      } catch (error) {
        addLog(error.message || '停止离线挂机失败。', 'fail')
        toast.error(error.message || '停止离线挂机失败。')
      }
    }

    const handlePartyBattle = ({ result, dungeonName, startedByPlayerId } = {}) => {
      if (!result) {
        return
      }

      const battleId = result?.battleId || result?.BattleId || ''
      if (rememberPartyBattleId(battleId)) {
        return
      }

      stopReplayTimer()
      clearLog()

      if (dungeonName) {
        // 中文注释：组队副本战斗结果同步后保留当前副本选择，
        // 这样循环战斗的下一次调度仍能取得 dungeonId 和原始副本数据。
        selectedDungeon.name = dungeonName
        battleResult.mapName = dungeonName
      }

      applyBattleResult(result, dungeonName || '')
      startBattleLogPlayback()

      const eventStarter = startedByPlayerId || result?.startedByPlayerId || result?.StartedByPlayerId || ''
      if (eventStarter && eventStarter !== getCurrentPlayerId()) {
        addLog(`队伍副本结果已同步：${dungeonName || result?.mapName || result?.MapName || '未知副本'}`, 'warning')
      }
    }

    const connectPartyBattleHub = async () => {
      try {
        await startPartyHub()
        await syncPartyHubGroups()
      } catch (error) {
        addLog(error?.message || '队伍战斗实时同步连接失败。', 'warning')
      }
    }

    const syncPlayerState = (sourcePlayer) => {
      const latestPlayer = buildPlayerView(sourcePlayer)
      Object.assign(player, latestPlayer)
      applyBattleCooldown(latestPlayer)
      applyPlayerSpiritRoot(sourcePlayer)
      syncAttributePointState(sourcePlayer, latestPlayer)

      const convertRate = (value, fallback = 0) => {
        const numericValue = Number(value)
        if (!Number.isFinite(numericValue)) {
          return fallback
        }

        if (numericValue <= 2.5) {
          return Math.round(numericValue * 100)
        }

        return Math.round(numericValue)
      }

      combatStats.phyAtk = latestPlayer.attack
      combatStats.magAtk = latestPlayer.magicAttack
      combatStats.phyDef = latestPlayer.defense
      combatStats.magDef = latestPlayer.magicDefense
      combatStats.speed = latestPlayer.speed
      combatStats.hitRate = convertRate(latestPlayer.hitRate, 0)
      combatStats.dodgeRate = convertRate(latestPlayer.dodgeRate, 0)
      combatStats.critRate = convertRate(latestPlayer.critRate, 0)
      combatStats.critDmg = convertRate(latestPlayer.critDamage, 150)
      combatStats.comboRate = convertRate(latestPlayer.comboRate, 0)
      combatStats.counterRate = convertRate(latestPlayer.counterRate, 0)
      combatStats.armorBreak = convertRate(latestPlayer.armorBreak, 0)
      combatStats.bonusDmg = convertRate(latestPlayer.bonusDamage, 0)
    }

    const changeView = (view) => {
      if (view === 'mapSelect') {
        modals.mapSelect = true
      }
    }

    const openModal = (modalName) => {
      if (modalName === 'help') {
        modals.help = true
        localStorage.setItem('xiuxian-help-seen-v1', '1')
        return
      }
      if (modalName === 'settings') {
        modals.settings = true
        return
      }
      if (modalName === 'theme') {
        toggleTheme()
        return
      }
      if (modalName === 'logout') {
        logout()
        return
      }
      if (Object.prototype.hasOwnProperty.call(modals, modalName)) {
        modals[modalName] = true
      }
    }

    const enterMap = (mapData) => {
      const partyId = gameStore.state.currentParty?.partyId || gameStore.state.currentParty?.PartyId
      const dungeonId = mapData?.dungeonId || mapData?.raw?.DungeonId || mapData?.raw?.dungeonId
      if (partyId && !dungeonId) {
        toast.error('当前队伍必须选择副本地图。')
        return
      }
      if (partyId && dungeonId) {
        selectedDungeon.raw = mapData
        selectedDungeon.id = dungeonId
        selectedDungeon.name = mapData?.name || mapData?.raw?.Name || '未知副本'
        selectedDungeon.canChallenge = mapData?.canChallenge ?? true
        selectedDungeon.unavailableReason = mapData?.unavailableReason || ''
        battleResult.mapName = selectedDungeon.name
        addLog(`已选择组队副本：${selectedDungeon.name}，点击“开始战斗”后将由队长发起。`, 'success')
        return
      }

      const mapId = mapData?.mapId || mapData?.id || mapData?.raw?.MapId || mapData?.raw?.mapId
      if (!mapId) {
        toast.error('地图数据异常，无法进入该地图。')
        return
      }
      selectedDungeon.id = mapId
      selectedDungeon.name = mapData?.name || mapData?.raw?.Name || '未知地图'
      selectedDungeon.raw = mapData || null
      selectedDungeon.canChallenge = mapData?.canChallenge ?? true
      selectedDungeon.unavailableReason = mapData?.unavailableReason || ''
      battleResult.mapName = selectedDungeon.name
      addLog(`已选择地图：${selectedDungeon.name}，点击“开始战斗”后将自动开始。`, 'success')
    }

    provide('openModal', openModal)

    watch(() => gameStore.state.player, (value) => {
      syncPlayerState(value)
    }, { immediate: true, deep: true })

    watch(() => gameStore.state.equippedItems, (value) => {
      applyEquipmentSlots(value || [])
    }, { immediate: true, deep: true })

    watch(() => gameStore.state.lastMessage, (value) => {
      const message = sanitizeStoreFeedback(value)
      if (message) {
        addLog(message, 'success')
      }
    })

    watch(() => gameStore.state.lastError, (value) => {
      const message = sanitizeStoreFeedback(value)
      if (message) {
        addLog(message, 'fail')
      }
    })

    watch(() => gameStore.state.lastBattleResult, (value) => {
      if (value) {
        applyBattleResult(value, selectedDungeon.name)
      }
    }, { deep: true })

    watch(isOfflineBattling, (value) => {
      if (!value) {
        stopOfflinePolling()
        return
      }

      if (autoBattleEnabled.value) {
        stopAutoBattle('当前账号已切换到离线挂机。')
      }
      startOfflinePolling()
    })

    watch(
      () => gameStore.state.currentParty?.partyId || gameStore.state.currentParty?.PartyId || '',
      async () => {
        if (!gameStore.state.accessToken) {
          return
        }

        await connectPartyBattleHub()
      }
    )

    watch(() => player.guildId, async (guildId, previousGuildId) => {
      if (guildId === previousGuildId) {
        return
      }

      if (chatState.currentTab === 'sect' && !guildId) {
        chatState.currentTab = 'world'
      }

      if (!chatState.realtimeReady && getChatHubState() === 'Disconnected') {
        return
      }

      try {
        await restartChatHub()
        if (guildId) {
          addLog('宗门聊天频道已同步到当前宗门。', 'success')
        }
      } catch (error) {
        addLog(error.message || '宗门聊天实时连接刷新失败。', 'warning')
      }
    })

    onMounted(async () => {
      const savedTheme = localStorage.getItem('xiuxian-theme')
      if (savedTheme) {
        document.documentElement.setAttribute('data-theme', savedTheme)
      }

      if (!gameStore.state.accessToken) {
        router.replace('/')
        return
      }

      await nextTick()
      syncBottomPanelHeight()
      if (equipmentSectionRef.value) {
        equipmentResizeObserver = new ResizeObserver(syncBottomPanelHeight)
        equipmentResizeObserver.observe(equipmentSectionRef.value)
      }

      try {
        await gameStore.bootstrapGameData()
        await gameStore.loadCurrentParty(true)
        const initialOfflineStatus = await gameStore.loadOfflineBattleStatus()
        addLog('游戏数据加载完成。', 'success')
        if (!localStorage.getItem('xiuxian-help-seen-v1')) {
          modals.help = true
          localStorage.setItem('xiuxian-help-seen-v1', '1')
        }
        if (initialOfflineStatus?.isOfflineBattling) {
          selectedDungeon.id = initialOfflineStatus.mapId || ''
          selectedDungeon.name = initialOfflineStatus.mapName || '未知地图'
          selectedDungeon.raw = null
          selectedDungeon.canChallenge = true
          selectedDungeon.unavailableReason = ''
          addLog(`已恢复离线挂机状态：${selectedDungeon.name}`, 'warning')
          startOfflinePolling()
        } else {
          addLog('请选择地图后点击“开始战斗”。', 'warning')
        }
        await loadChatHistory('world')
        unsubscribeChatHub = subscribeChatHub({
          onMessage: (message) => {
            appendChatMessage(message)
          },
          onOnlineCount: (count) => {
            chatState.onlineCount = Number(count) || 0
          },
          onForceDisconnect: (message) => {
            chatState.realtimeReady = false
            chatState.connectionState = 'offline'
            chatState.connectionText = '连接已断开'
            addLog(message || '聊天连接已被服务端关闭。', 'warning')
          },
          onStateChange: (state, error) => {
            chatState.realtimeReady = state === 'connected'
            if (state === 'reconnecting') {
              chatState.connectionState = 'reconnecting'
              chatState.connectionText = '重连中'
              addLog('聊天实时连接重连中。', 'warning')
            } else if (state === 'connected') {
              chatState.connectionState = 'connected'
              chatState.connectionText = '实时已连接'
              addLog('聊天实时连接已建立。', 'success')
              scrollChatToBottom()
            } else if (state === 'error') {
              chatState.connectionState = 'offline'
              chatState.connectionText = '实时未连接'
              addLog(error?.message || '聊天实时连接失败，将继续使用普通接口。', 'warning')
            } else if (state === 'closed') {
              chatState.connectionState = 'offline'
              chatState.connectionText = '连接已关闭'
            }
          }
        })
        await connectChatHub()
        unsubscribePartyHub = subscribePartyHub({
          onPartyBattleResolved: (payload) => {
            const result = payload?.result || payload?.Result || null
            const dungeonName = payload?.dungeonName || payload?.DungeonName || result?.mapName || result?.MapName || ''
            const startedByPlayerId = payload?.startedByPlayerId || payload?.StartedByPlayerId || result?.startedByPlayerId || result?.StartedByPlayerId || ''
            handlePartyBattle({ result, dungeonName, startedByPlayerId })
          },
          onStateChange: (state, error) => {
            if (state === 'error') {
              addLog(error?.message || '队伍战斗实时同步连接失败。', 'warning')
            }
          }
        })
        await connectPartyBattleHub()
      } catch (error) {
        toast.error(error.message || '加载游戏数据失败')
        router.replace('/')
        return
      }

    })

    onUnmounted(() => {
      equipmentResizeObserver?.disconnect()
      stopReplayTimer()
      stopBattleCooldownTimer()
      clearAutoBattleTimer()
      stopOfflinePolling()
      unsubscribeChatHub?.()
      unsubscribePartyHub?.()
      stopChatHub().catch(() => {})
      stopPartyHub().catch(() => {})
    })

    return {
      ICON,
      modals,
      giftTarget,
      showPillEffectModal,
      gemSynthGemId,
      handleOpenGemSynth,
      forgeEquipment,
      handleOpenForge,
      player,
      canBreakthrough,
      remainingLevelExp,
      formatNumber,
      formatAttributeRatio,
      attemptBreakthrough,
      combatStats,
      showRootTooltip,
      rootBadgeRef,
      tooltipPosition,
      handleRootMouseEnter,
      playerSpiritRoot,
      availablePoints,
      isAdjustingAttributePoint,
      attributes,
      increaseAttribute,
      decreaseAttribute,
      hoveredSlot,
      slotRefs,
      slotTooltipPosition,
      handleSlotHover,
      equipmentSlots,
      unequipItem,
      battleLogs,
      battleLogContainer,
      clearLog,
      changeView,
      openModal,
      openMapSelector,
      enterMap,
      startBattle,
      toggleOfflineBattle,
      handlePartyBattle,
      selectedDungeon,
      offlineBattleStatus,
      isOfflineBattling,
      battleUnits,
      hoveredEnemyTooltip,
      enemyTooltipPosition,
      handleEnemyHover,
      hideEnemyTooltip,
      battleResult,
      isStarting,
      autoBattleEnabled,
      battleStartDisabled,
      battleStartLabel,
      currentRound,
      chatState,
      chatUnread,
      chatMessagesContainer,
      chatTabs,
      chatInputPlaceholder,
      switchChatTab,
      sendChatMessage,
      resolveTitleIcon,
      isSidebarCollapsed,
      equipmentSectionRef,
      bottomPanelHeight,
      toggleSidebar
    }
  }
}
</script>

<style scoped>
.main-page {
  width: 100%;
  height: 100vh;
  display: flex;
  flex-direction: column;
  background: var(--xiuxian-bg-primary);
  overflow: hidden;
}

.content-wrapper {
  flex: 1;
  display: flex;
  min-height: 0;
  overflow: hidden;
  position: relative;
}

.floating-sidebar-btn {
  position: fixed;
  top: 16px;
  left: 16px;
  width: 36px;
  height: 36px;
  background: var(--control-bg);
  border: 1px solid var(--control-border);
  border-radius: 50%;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 16px;
  color: var(--text-secondary);
  box-shadow: var(--shadow-md);
  z-index: 98;
  transition: all 0.2s ease;
  backdrop-filter: blur(8px);
}

.floating-sidebar-btn:hover {
  border-color: var(--border-color);
  color: var(--accent-text);
  background: var(--control-bg-hover);
  transform: scale(1.05);
}

.main-content {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-width: 0;
  min-height: 0;
  overflow: hidden;
}

/* 主体区域 - 双栏布局 */
.main-body {
  flex: 1;
  display: grid;
  grid-template-columns: 260px 1fr;
  overflow: hidden;
  gap: var(--spacing-md);
  padding: var(--spacing-md);
}

/* 左侧面板 - 人物信息 + 装备 */
.left-panel {
  width: 260px;
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
  flex-shrink: 0;
  overflow-y: auto;
}

/* 人物信息区 */
.player-info-section {
  background: var(--xiuxian-bg-panel);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-lg);
  padding: 12px;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
}

.player-avatar {
  position: relative;
  width: 72px;
  height: 72px;
  background: var(--bg-overlay-light);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 36px;
  border: 1px solid var(--border-color);
  box-shadow: var(--shadow-sm);
  transition: all 0.2s ease;
  margin-bottom: 4px;

  img,
  :deep(img) {
    width: 100%;
    height: 100%;
    border-radius: 50%;
    object-fit: cover;
  }
}

.player-name {
  font-size: 16px;
  font-weight: 600;
  color: var(--text-primary);
}

/* 玩家名字和灵根徽章 */
.player-name-with-root {
  display: flex;
  align-items: center;
  gap: var(--spacing-xs);
  margin-top: 2px;

  .player-name {
    font-size: 16px;
    font-weight: 600;
    color: var(--text-primary);
  }

  .player-title-chip {
    max-width: 110px;
    padding: 2px 8px;
    border: 1px solid var(--border-color);
    border-radius: 999px;
    background: var(--accent-soft-bg);
    color: var(--text-primary);
    font-size: 11px;
    font-weight: 600;
    line-height: 1.2;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  .spirit-root-badge {
    position: relative;
    width: 20px;
    height: 20px;
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: 4px;
    font-size: 12px;
    font-weight: 700;
    cursor: pointer;
    transition: all 0.2s ease;
    border: 1px solid;

    /* 各灵根颜色 */
    &.metal {
      background: rgba(192, 192, 192, 0.2);
      border-color: #C0C0C0;
      color: #C0C0C0;
    }

    &.wood {
      background: rgba(34, 139, 34, 0.2);
      border-color: #228B22;
      color: #228B22;
    }

    &.water {
      background: rgba(30, 144, 255, 0.2);
      border-color: #1E90FF;
      color: #1E90FF;
    }

    &.fire {
      background: rgba(255, 69, 0, 0.2);
      border-color: #FF4500;
      color: #FF4500;
    }

    &.earth {
      background: rgba(210, 105, 30, 0.2);
      border-color: #D2691E;
      color: #D2691E;
    }

    &.wind {
      background: rgba(32, 178, 170, 0.2);
      border-color: #20B2AA;
      color: #20B2AA;
    }

    &.ice {
      background: rgba(135, 206, 235, 0.2);
      border-color: #87CEEB;
      color: #87CEEB;
    }

    &.thunder {
      background: rgba(147, 112, 219, 0.2);
      border-color: #9370DB;
      color: #9370DB;
    }

    &:hover {
      transform: scale(1.1);
    }

    /* Tooltip 已使用 Teleport 传送到 body */
  }
}

.player-realm {
  .realm-badge {
    padding: 4px 12px;
    background: var(--accent-soft-bg);
    border-radius: var(--radius-sm);
    font-size: var(--font-size-base);
    font-weight: 500;
    color: var(--accent-text);
  }
}

.player-body {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 2px;
  .body-label {
    font-size: var(--font-size-sm);
    color: var(--text-muted);
  }
  .body-value {
    font-size: var(--font-size-base);
    color: var(--accent-text);
  }
}

/* 修炼进度条 */
.cultivation-progress {
  width: 100%;
  .progress-label {
    display: flex;
    justify-content: space-between;
    font-size: var(--font-size-sm);
    color: var(--text-secondary);
    margin-bottom: 4px;
  }
  .progress-bar {
    height: 8px;
    background: var(--bg-overlay-dark);
    border-radius: 4px;
    overflow: hidden;
    .progress-fill {
      height: 100%;
      background: var(--text-secondary);
      border-radius: 4px;
      transition: width 0.3s ease;
    }
  }
}

/* 资源条 */
.resource-bars {
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 6px;
  margin: 6px 0;
}

.resource-row {
  display: flex;
  flex-direction: column;
  gap: 3px;
}

.resource-row-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
}

.resource-label {
  color: var(--text-secondary);
  font-size: var(--font-size-xs);
}

.resource-value {
  color: var(--text-primary);
  font-size: var(--font-size-xs);
  font-family: var(--font-mono);
}

.hp-bar, .mp-bar {
  position: relative;
  height: 10px;
  background: var(--bg-overlay-medium);
  border-radius: 5px;
  overflow: hidden;
}

.hp-bar .bar-fill {
  height: 100%;
  background: var(--hp-color);
  border-radius: 5px;
  transition: width 0.3s ease;
}

.mp-bar .bar-fill {
  height: 100%;
  background: var(--mp-color);
  border-radius: 5px;
  transition: width 0.3s ease;
}

/* 修炼区域 */
.cultivation-section {
  width: 100%;
  margin-top: var(--spacing-sm);
  padding-top: var(--spacing-sm);
  border-top: 1px solid var(--border-color);
}

.cultivation-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--spacing-xs);
}

.cultivation-label {
  font-size: var(--font-size-base);
  color: var(--accent-text);
}

.cultivation-status {
  font-size: var(--font-size-xs);
  padding: 2px 6px;
  border-radius: var(--radius-sm);
  background: var(--bg-overlay-light);
  color: var(--text-muted);
}

.cultivation-status.active {
  background: var(--status-success-bg);
  color: var(--status-success-text);
}

.cultivation-progress-bar {
  display: flex;
  align-items: center;
  gap: var(--spacing-xs);
  margin-bottom: var(--spacing-xs);
}

.progress-track {
  flex: 1;
  height: 10px;
  background: var(--bg-overlay-dark);
  border-radius: 5px;
  overflow: hidden;
}

.progress-thumb {
  height: 100%;
  background: var(--text-secondary);
  border-radius: 5px;
  transition: width 0.1s linear;
}

.progress-percent {
  width: 35px;
  text-align: right;
  font-size: var(--font-size-xs);
  color: var(--text-muted);
  font-family: var(--font-mono);
}

.exp-gain {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: var(--font-size-sm);
  margin-bottom: var(--spacing-sm);
}

.exp-label {
  color: var(--text-secondary);
}

.exp-value {
  color: var(--text-primary);
  font-weight: 600;
  font-family: var(--font-mono);
}

.cultivation-controls {
  display: flex;
  gap: var(--spacing-xs);
  width: 100%;
  margin-top: 2px;
}

.breakthrough-btn-wrap {
  position: relative;
  flex: 1;
}

.cultivate-btn, .breakthrough-btn {
  flex: 1;
  padding: 5px 8px;
  border: none;
  border-radius: var(--radius-sm);
  font-size: var(--font-size-xs);
  cursor: pointer;
  transition: all 0.15s ease;
}

.cultivate-btn.start {
  background: var(--button-primary-start);
  color: var(--button-primary-text);
  border: 1px solid var(--button-primary-start);
}

.cultivate-btn.start:hover {
  background: var(--button-primary-hover-start);
  color: var(--button-primary-hover-text);
  border-color: var(--button-primary-hover-start);
}

.cultivate-btn.pause {
  background: var(--button-secondary-start);
  color: var(--button-secondary-text);
  border: 1px solid var(--border-color);
}

.cultivate-btn.pause:hover {
  background: var(--button-secondary-hover-start);
  color: var(--button-secondary-hover-text);
}

.breakthrough-btn {
  background: var(--button-primary-start);
  color: var(--button-primary-text);
}

.cultivate-btn:hover,
.breakthrough-btn:hover:not(:disabled) {
}

.breakthrough-btn:not(:disabled) {
  background: var(--primary-color) !important;
  color: var(--text-on-dark) !important;
  font-weight: 600;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.08);
}

.breakthrough-btn:hover:not(:disabled) {
  background: var(--jade-light) !important;
  box-shadow: 0 6px 14px rgba(0, 0, 0, 0.12);
}

.breakthrough-btn:disabled {
  opacity: 0.35;
  cursor: not-allowed;
}

.pill-effect-btn {
  flex-shrink: 0;
  padding: 6px 14px;
  border: 1px solid var(--border-color);
  border-radius: 6px;
  background: var(--bg-overlay-light);
  color: var(--text-secondary);
  cursor: pointer;
  font-size: 13px;
  transition: all 0.2s;
  white-space: nowrap;
}
.pill-effect-btn:hover {
  background: var(--bg-overlay);
  color: var(--text-primary);
}

.breakthrough-btn-tooltip {
  position: absolute;
  left: 50%;
  bottom: calc(100% + 10px);
  transform: translateX(-50%);
  min-width: 220px;
  padding: 10px 12px;
  border-radius: var(--radius-md);
  background: var(--xiuxian-bg-secondary);
  border: 1px solid var(--border-color);
  box-shadow: var(--shadow-md);
  opacity: 0;
  pointer-events: none;
  transition: opacity 0.2s ease, transform 0.2s ease;
  z-index: 20;
}

.breakthrough-btn-tooltip::after {
  content: '';
  position: absolute;
  left: 50%;
  top: 100%;
  transform: translateX(-50%);
  border-width: 6px;
  border-style: solid;
  border-color: var(--border-color) transparent transparent transparent;
}

.breakthrough-btn-wrap:hover .breakthrough-btn-tooltip {
  opacity: 1;
  transform: translateX(-50%) translateY(-2px);
}

.breakthrough-btn-tip-title {
  margin-top: 6px;
  margin-bottom: 4px;
  color: var(--text-primary);
  font-size: var(--font-size-xs);
  font-weight: 600;
}

.breakthrough-btn-tip-line {
  color: var(--text-secondary);
  font-size: var(--font-size-xs);
  line-height: 1.5;
  white-space: nowrap;
}

.breakthrough-btn-tip-line.lacking {
  color: var(--hp-color);
}

/* 突破信息区域 */
.breakthrough-info {
  width: 100%;
  margin: 4px 0;
  padding: 8px 10px;
  background: var(--bg-overlay-medium);
  border-radius: var(--radius-md);
  border: 1px solid var(--border-color);
}

.breakthrough-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 2px 0;
  font-size: var(--font-size-sm);
  gap: 10px;
}

.breakthrough-label {
  color: var(--text-muted);
  font-size: var(--font-size-xs);
}

.breakthrough-name {
  color: var(--text-primary);
  font-weight: 500;
  text-align: right;
  font-size: var(--font-size-sm);
}

.breakthrough-extra {
  margin-top: 6px;
  padding-top: 6px;
  border-top: 1px dashed var(--border-color);
}

.breakthrough-meta {
  display: flex;
  justify-content: space-between;
  gap: 8px;
  color: var(--text-muted);
  font-size: var(--font-size-xs);
  font-family: var(--font-mono);
}

.breakthrough-materials {
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
  margin-top: 6px;
}

.breakthrough-material {
  padding: 2px 6px;
  border-radius: 999px;
  background: var(--bg-overlay-light);
  color: var(--accent-text);
  font-size: var(--font-size-xs);
  border: 1px solid var(--border-color);
}

.breakthrough-material.lacking {
  color: var(--hp-color);
  border-color: var(--hp-color);
}

.breakthrough-value {
  color: var(--accent-text);
  font-family: var(--font-mono);
  font-size: var(--font-size-sm);
  text-align: right;
}

.realm-progress-panel {
  width: 100%;
  margin-bottom: 4px;
  padding: 8px 10px;
  background: var(--xiuxian-bg-secondary);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  box-shadow: inset 0 1px 0 var(--bg-overlay-light);
}

.realm-progress-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  margin-bottom: 6px;
}

.realm-progress-title {
  color: var(--accent-text);
  font-size: var(--font-size-xs);
  font-weight: 600;
}

.realm-progress-values {
  color: var(--accent-text);
  font-size: var(--font-size-xs);
  font-family: var(--font-mono);
}

.realm-progress-track {
  height: 8px;
  background: var(--bg-overlay-medium);
  border-radius: 999px;
  overflow: hidden;
}

.realm-progress-fill {
  height: 100%;
  background: var(--exp-color);
  border-radius: 999px;
  transition: width 0.3s ease;
}

.realm-progress-meta {
  display: flex;
  justify-content: space-between;
  gap: 8px;
  margin-top: 6px;
  color: var(--text-secondary);
  font-size: var(--font-size-xs);
  font-family: var(--font-mono);
}

.exp-pool-row {
  margin-top: 6px;
  padding-top: 6px;
  border-top: 1px dashed var(--border-color);
}

.exp-pool-row .breakthrough-name {
  color: var(--text-secondary);
}

/* 属性区域 */
.stats-section {
  background: var(--xiuxian-bg-panel);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-lg);
  padding: var(--spacing-md);
  overflow-y: auto;

  .section-title {
    font-size: var(--font-size-base);
    font-weight: 600;
    color: var(--text-primary);
    margin-bottom: var(--spacing-sm);
    padding-bottom: var(--spacing-xs);
    border-bottom: 1px solid var(--border-color);
  }

  /* 属性分组 */
  .stats-group {
    margin-bottom: var(--spacing-sm);

    &:last-child {
      margin-bottom: 0;
    }

    .group-label {
      font-size: var(--font-size-xs);
      color: var(--text-muted);
      margin-bottom: 4px;
      padding-left: 4px;
    }
  }

  .stats-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 4px;

    &.percent-stats {
      grid-template-columns: 1fr 1fr;

      .stat-item {
        padding: 4px 6px;
      }
    }

    .stat-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 5px 8px;
      background: var(--bg-overlay-light);
      border-radius: var(--radius-sm);

      &.full-width {
        grid-column: 1 / -1;
      }

      .stat-label {
        font-size: 11px;
        color: var(--text-muted);
      }

      .stat-value {
        font-size: 12px;
        font-weight: 600;
        color: var(--accent-text);
        font-family: var(--font-mono);

        &.highlight {
          color: var(--highlight-text);
        }
      }
    }
  }
}



/* 属性加点区域 */
.attribute-section {
  flex: 1;
  background: var(--xiuxian-bg-panel);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-lg);
  padding: var(--spacing-md);
  overflow-y: auto;
  overflow-x: hidden;

  .section-title {
    font-size: var(--font-size-base);
    font-weight: 600;
    color: var(--text-primary);
    margin-bottom: var(--spacing-md);
    padding-bottom: var(--spacing-sm);
    border-bottom: 1px solid var(--border-color);
  }

  .available-points {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: var(--spacing-sm);
    background: var(--bg-overlay-medium);
    border-radius: var(--radius-md);
    margin-bottom: var(--spacing-md);

    .points-label {
      font-size: var(--font-size-sm);
      color: var(--text-secondary);
    }

    .points-value {
      font-size: 18px;
      font-weight: 700;
      color: var(--text-primary);
      font-family: var(--font-mono);
    }
  }

  .attribute-list {
    display: flex;
    flex-direction: column;
    gap: 2px;
  }

  .attribute-item {
    padding: 6px 8px;
    background:
      linear-gradient(135deg, rgba(255, 255, 255, 0.03), transparent 55%),
      var(--bg-overlay-light);
    border-radius: var(--radius-md);
    border: 1px solid rgba(255, 255, 255, 0.05);
    box-shadow: inset 0 1px 0 rgba(255, 255, 255, 0.04);

    .attr-row {
      display: flex;
      flex-direction: column;
      align-items: stretch;
      gap: 3px;
      width: 100%;
    }

    .attr-meta {
      display: flex;
      align-items: center;
      justify-content: flex-start;

      .attr-name {
        font-size: 13px;
        font-weight: 600;
        color: var(--text-primary);
        line-height: 1.3;
        white-space: normal;
        overflow: visible;
        text-overflow: clip;
      }
    }

    .attr-controls {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 6px;
      padding: 1px 4px;
      border-radius: 999px;
      background: rgba(0, 0, 0, 0.07);
      border: 1px solid rgba(255, 255, 255, 0.04);
      width: fit-content;
      max-width: 100%;
      margin-left: auto;

      .attr-btn {
        width: 34px;
        height: 34px;
        border: none;
        border-radius: 50%;
        font-size: 18px;
        font-weight: 700;
        cursor: pointer;
        transition: all 0.2s ease;
        display: flex;
        align-items: center;
        justify-content: center;
        box-shadow: 0 4px 10px rgba(0, 0, 0, 0.16);

        &.minus {
          background: linear-gradient(135deg, #c0392b, #e74c3c);
          color: white;
        }

        &.plus {
          background: linear-gradient(135deg, #27ae60, #2ecc71);
          color: white;
        }

        &:disabled {
          opacity: 0.4;
          cursor: not-allowed;
        }

        &:not(:disabled):hover {
          transform: translateY(-1px) scale(1.04);
        }
      }

      .attr-invested {
        font-size: 13px;
        color: var(--text-primary);
        font-family: var(--font-mono);
        min-width: 54px;
        text-align: center;
        white-space: nowrap;
        padding: 5px 8px;
        border-radius: 999px;
        background: var(--accent-soft-bg);
        border: 1px solid var(--border-color);
      }
    }

  }
}

/* 装备栏区域 */
.equipment-section {
  flex: 1.5;
  background: var(--xiuxian-bg-panel);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-lg);
  padding: var(--spacing-md);
  overflow-y: auto;
  min-height: 0;

  .section-title {
    font-size: var(--font-size-base);
    font-weight: 600;
    color: var(--text-primary);
    margin-bottom: var(--spacing-md);
    padding-bottom: var(--spacing-sm);
    border-bottom: 1px solid var(--border-color);
  }

  .equipment-grid {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: var(--spacing-sm);
  }

  .equipment-slot {
    position: relative;
    display: flex;
    flex-direction: column;
    align-items: center;
    padding: var(--spacing-sm);
    background: var(--bg-overlay-light);
    border-radius: var(--radius-md);
    border: 2px dashed var(--border-color);
    cursor: pointer;
    transition: all 0.2s ease;
    min-height: 70px;

    &:hover {
      background: var(--bg-overlay-medium);
      border-color: var(--text-muted);
    }

    &.has-item {
      border-style: solid;
      border-color: var(--border-color);

      &:hover {
        border-color: var(--text-secondary);
      }
    }

    &.common { 
      border-color: var(--quality-common);
      background: var(--xiuxian-bg-panel);
    }
    &.uncommon { 
      border-color: var(--quality-uncommon); 
      background: var(--xiuxian-bg-panel);
      box-shadow: 0 2px 6px rgba(22, 163, 74, 0.06);
    }
    &.rare { 
      border-color: var(--quality-rare); 
      background: var(--xiuxian-bg-panel);
      box-shadow: 0 2px 8px rgba(37, 99, 235, 0.06);
    }
    &.epic { 
      border-color: var(--quality-epic); 
      background: var(--xiuxian-bg-panel);
      box-shadow: 0 2px 10px rgba(147, 51, 234, 0.08);
    }
    &.legendary { 
      border-color: var(--quality-legendary); 
      background: var(--xiuxian-bg-panel);
      box-shadow: 0 2px 12px rgba(234, 88, 12, 0.1);
    }
    &.mythic { 
      border-color: var(--quality-mythic); 
      background: var(--xiuxian-bg-panel);
      box-shadow: 0 2px 16px rgba(220, 38, 38, 0.12);
    }

    .slot-icon {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 40px;
      height: 40px;
      margin-bottom: 2px;
    }

    .slot-bound-badge {
      position: absolute;
      top: 4px;
      left: 4px;
      font-size: 12px;
      line-height: 1;
      color: var(--text-primary);
      text-shadow: 0 1px 2px rgba(15, 23, 42, 0.75);
    }

    .slot-name {
      font-size: 13px;
      color: var(--text-secondary);
      text-align: center;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
      max-width: 100%;
      line-height: 1.2;
    }

    .slot-enhance {
      color: #4fc3f7;
      font-size: 12px;
      margin-left: 2px;
    }

    .unequip-btn {
      position: absolute;
      top: 2px;
      right: 2px;
      width: 16px;
      height: 16px;
      border: none;
      border-radius: 50%;
      background: var(--status-danger-bg);
      color: var(--status-danger-text);
      font-size: 10px;
      cursor: pointer;
      opacity: 0;
      transition: opacity 0.2s ease;
      display: flex;
      align-items: center;
      justify-content: center;
      line-height: 1;
    }

    &:hover .unequip-btn {
      opacity: 1;
    }

    /* Tooltip */
    .equipment-tooltip {
      position: absolute;
      left: 100%;
      top: 50%;
      transform: translateY(-50%);
      margin-left: 8px;
      width: 200px;
      padding: var(--spacing-md);
      background: var(--xiuxian-bg-panel);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-md);
      box-shadow: 0 4px 20px rgba(0, 0, 0, 0.5);
      z-index: 1000;
      pointer-events: none;

      &::before {
        content: '';
        position: absolute;
        left: -6px;
        top: 50%;
        transform: translateY(-50%);
        border-style: solid;
        border-width: 6px 6px 6px 0;
        border-color: transparent var(--border-color) transparent transparent;
      }

      .tooltip-name {
        font-size: var(--font-size-base);
        font-weight: 600;
        margin-bottom: 4px;

        &.common { color: var(--quality-common); }
        &.uncommon { color: var(--quality-uncommon); }
        &.rare { color: var(--quality-rare); }
        &.epic { color: var(--quality-epic); }
        &.legendary { color: var(--quality-legendary); }
        &.mythic { color: var(--quality-mythic); }
      }

      .tooltip-type {
        font-size: var(--font-size-xs);
        color: var(--text-muted);
        margin-bottom: var(--spacing-sm);
        padding-bottom: var(--spacing-xs);
        border-bottom: 1px solid var(--border-color);
      }

      .tooltip-stats {
        display: flex;
        flex-direction: column;
        gap: 2px;
        margin-bottom: var(--spacing-sm);

        .tooltip-stat {
          font-size: var(--font-size-sm);
          color: var(--accent-text);
        }
      }

      .tooltip-desc {
        font-size: var(--font-size-xs);
        color: var(--text-secondary);
        font-style: italic;
        line-height: 1.4;
      }
    }
  }
}

/* 右侧战斗区域 */
.battle-area {
  flex: 1;
  min-height: 0;
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
  min-width: 0;
  overflow: hidden;
}

/* 三列横向布局容器 */
.battle-columns {
  display: flex;
  flex: 1;
  min-height: 0;
  flex-direction: row;
  gap: var(--spacing-md);
  overflow: hidden;
}

/* 战斗工具栏 */
.battle-toolbar {
  background: var(--xiuxian-bg-panel);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-lg);
  padding: var(--spacing-md);
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: var(--spacing-md);
  flex-wrap: wrap;
}

.battle-status {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  flex-wrap: wrap;
}

.battle-status .map-name {
  font-size: var(--font-size-base);
  font-weight: 600;
  color: var(--text-primary);
}

.battle-status .round-info {
  font-size: var(--font-size-sm);
  color: var(--text-secondary);
  background: var(--bg-overlay-light);
  padding: 2px 8px;
  border-radius: var(--radius-sm);
}

.battle-status .offline-info {
  font-size: var(--font-size-sm);
  color: var(--status-warning-text);
  background: var(--status-warning-bg);
  padding: 2px 8px;
  border-radius: var(--radius-sm);
}

.result-badge {
  padding: 2px 8px;
  border-radius: var(--radius-sm);
  font-size: var(--font-size-sm);
  font-weight: 600;
}

.result-badge.victory {
  background: var(--status-success-bg);
  color: var(--status-success-text);
}

.result-badge.defeat {
  background: var(--status-danger-bg);
  color: var(--status-danger-text);
}

.battle-toolbar-actions {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  flex-wrap: wrap;
}

.battle-action-btn {
  padding: 8px 14px;
  border: 1px solid var(--button-neutral-border);
  border-radius: var(--radius-sm);
  background: var(--button-neutral-bg);
  color: var(--button-neutral-text);
  font-size: var(--font-size-sm);
  cursor: pointer;
  transition: all 0.2s ease;
}

.battle-action-btn:hover:not(:disabled) {
  border-color: var(--button-outline-hover-border);
  background: var(--button-outline-hover-bg);
  color: var(--button-outline-hover-text);
}

.battle-action-btn.start {
  background: var(--button-primary-start);
  border-color: var(--text-secondary);
  color: var(--button-primary-text);
}

.battle-action-btn.start:hover:not(:disabled) {
  background: var(--button-primary-hover-start);
  color: var(--button-primary-hover-text);
}

.battle-action-btn.offline {
  background: var(--button-neutral-bg);
  border-color: var(--button-neutral-border);
  color: var(--text-primary);
}

.battle-action-btn.offline:hover:not(:disabled) {
  background: var(--button-neutral-hover-bg);
}

.battle-action-btn.offline.stop {
  background: var(--button-neutral-bg);
  border-color: var(--hp-color);
  color: var(--hp-color);
}

.battle-action-btn.offline.stop:hover:not(:disabled) {
  background: var(--button-neutral-hover-bg);
  border-color: var(--hp-color);
  color: var(--hp-color);
}

.battle-action-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* 敌人区域 */
.enemy-section {
  order: 2;
  flex: 0 0 200px;
  height: 100%;
  min-height: 0;
  display: flex;
  flex-direction: column;
  background: var(--xiuxian-bg-panel);
  border: 1px solid var(--border-color);
  border-top: 3px solid var(--hp-color);
  border-radius: var(--radius-lg);
  padding: var(--spacing-md);
  overflow-y: auto;

  .section-header {
    flex-shrink: 0;
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: var(--spacing-md);

    .section-title {
      font-size: var(--font-size-sm);
      font-weight: 600;
      color: var(--hp-color);
      background: rgba(244, 63, 94, 0.08);
      padding: 3px 8px;
      border-radius: var(--radius-sm);
      border: 1px solid rgba(244, 63, 94, 0.15);
    }

    .map-tag {
      font-size: var(--font-size-base);
      color: var(--text-muted);
      background: var(--bg-overlay-light);
      padding: 2px 8px;
      border-radius: var(--radius-sm);
    }
  }
}

/* 友方区域 */
.ally-section {
  order: 1;
  flex: 0 0 200px;
  height: 100%;
  min-height: 0;
  display: flex;
  flex-direction: column;
  background: var(--xiuxian-bg-panel);
  border: 1px solid var(--border-color);
  border-top: 3px solid var(--quality-uncommon);
  border-radius: var(--radius-lg);
  padding: var(--spacing-md);
  overflow-y: auto;

  .section-header {
    flex-shrink: 0;
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: var(--spacing-md);

    .section-title {
      font-size: var(--font-size-sm);
      font-weight: 600;
      color: var(--quality-uncommon);
      background: rgba(34, 197, 94, 0.08);
      padding: 3px 8px;
      border-radius: var(--radius-sm);
      border: 1px solid rgba(34, 197, 94, 0.15);
    }
  }
}

/* 单位行 */
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

  &.enemy {
    background: var(--xiuxian-bg-panel);
    border-color: var(--border-color);
    &.elite {
      border-color: var(--status-warning-text);
      box-shadow: 0 0 10px var(--status-warning-bg);
    }
  }

  &.ally {
    background: var(--xiuxian-bg-panel);
    border-color: var(--border-color);
    &.active {
      border-color: var(--text-secondary);
    }
  }

  .unit-avatar {
    font-size: 32px;
  }

  .unit-name {
    font-size: var(--font-size-base);
    color: var(--text-primary);
  }
}

/* 战斗日志区域 */
.battle-log-section {
  order: 3;
  flex: 1;
  min-height: 0;
  height: 100%;
  min-width: 300px;
  background: var(--xiuxian-bg-panel);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-lg);
  display: flex;
  flex-direction: column;
  overflow: hidden;

  .log-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: var(--spacing-sm) var(--spacing-md);
    background: var(--bg-overlay-light);
    border-bottom: 1px solid var(--border-color);

    .section-title {
      font-size: var(--font-size-sm);
      font-weight: 500;
      color: var(--accent-text);
    }

    .clear-btn {
      font-size: var(--font-size-sm);
      padding: 2px 8px;
      background: var(--button-neutral-bg);
      border: 1px solid var(--button-neutral-border);
      border-radius: var(--radius-sm);
      color: var(--button-neutral-text);
      cursor: pointer;

      &:hover {
        background: var(--button-outline-hover-bg);
        border-color: var(--button-outline-hover-border);
        color: var(--button-outline-hover-text);
      }
    }
  }

  .log-content {
    flex: 1;
    padding: var(--spacing-sm);
    overflow-y: auto;
    font-size: var(--font-size-base);
    line-height: 1.6;
    scrollbar-width: thin;
    scrollbar-color: var(--border-color) transparent;

    &::-webkit-scrollbar {
      width: 6px;
    }

    &::-webkit-scrollbar-track {
      background: transparent;
    }

    &::-webkit-scrollbar-thumb {
      background: var(--border-color);
      border-radius: 3px;
    }

    &::-webkit-scrollbar-thumb:hover {
      background: var(--border-color);
    }
  }
}

.log-entry {
  padding: 4px 8px;
  border-left: 2px solid transparent;
  border-radius: var(--radius-sm);
  transition: background 0.2s ease, border-color 0.2s ease;

  .log-time {
    color: var(--text-muted);
    margin-right: var(--spacing-xs);
    font-family: var(--font-mono);
  }

  .log-text {
    color: var(--text-primary);
  }

  &.normal {
    border-left-color: var(--border-color);
  }

  &.attack {
    border-left-color: var(--text-muted);
  }

  &.attack .log-text {
    color: var(--text-primary);
  }

  &.damage {
    background: rgba(239, 68, 68, 0.03);
    border-left-color: var(--log-damage);
  }

  &.damage .log-text {
    color: var(--text-primary);
  }

  &.enemy {
    border-left-color: var(--log-enemy);
  }

  &.enemy .log-text {
    color: var(--text-primary);
  }

  &.skill {
    border-left-color: var(--log-skill);
  }

  &.skill .log-text {
    color: var(--text-primary);
  }

  &.heal {
    background: rgba(34, 197, 94, 0.03);
    border-left-color: var(--log-heal);
  }

  &.heal .log-text {
    color: var(--text-primary);
  }

  &.counter {
    border-left-color: var(--text-muted);
  }

  &.counter .log-text {
    color: var(--text-primary);
  }

  &.crit {
    background: rgba(245, 158, 11, 0.03);
    border-left-color: var(--log-skill);
  }

  &.crit .log-text {
    color: var(--text-primary);
  }

  &.success {
    background: rgba(34, 197, 94, 0.04);
    border-left-color: var(--status-success-text);
  }

  &.success .log-text {
    color: var(--status-success-text);
    font-weight: 600;
  }

  &.fail {
    background: rgba(239, 68, 68, 0.04);
    border-left-color: var(--hp-color);
  }

  &.fail .log-text {
    color: var(--status-danger-text);
    font-weight: 600;
  }

  &.warning {
    background: var(--bg-overlay-light);
    border-left-color: var(--status-warning-text);
  }

  &.warning .log-text {
    color: var(--text-primary);
  }
}

/* 日志富文本高亮 */
.log-entry .log-text :deep(.log-hl-ally) { color: var(--log-ally); font-weight: 600; }
.log-entry .log-text :deep(.log-hl-enemy) { color: var(--log-enemy); font-weight: 600; }
.log-entry .log-text :deep(.log-hl-skill) { color: var(--log-skill); font-weight: 600; }
.log-entry .log-text :deep(.log-hl-damage) { color: var(--log-damage); font-weight: 700; }
.log-entry .log-text :deep(.log-hl-heal) { color: var(--log-heal); font-weight: 700; }
.log-entry .log-text :deep(.log-hl-hp) { color: var(--text-muted); }

.empty-log {
  text-align: center;
  color: var(--text-muted);
  padding: var(--spacing-xl);
  font-style: italic;
}

/* 聊天窗口和聚灵阵容器 */
.chat-array-container {
  display: flex;
  flex: 0 0 var(--bottom-panel-height, 350px);
  height: var(--bottom-panel-height, 350px);
  gap: var(--spacing-md);
  min-height: 0;
  overflow: hidden;
}

.five-elements-array-section {
  flex: 0 1 clamp(380px, 34vw, 460px);
  width: clamp(380px, 34vw, 460px);
  min-width: 380px;
}

/* 聊天窗口 */
.chat-section {
  flex: 1;
  background: var(--xiuxian-bg-panel);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-lg);
  display: flex;
  flex-direction: column;
  min-height: 0;
  overflow: hidden;
}



.chat-tabs {
  display: flex;
  background: var(--bg-overlay-medium);
  border-bottom: 1px solid var(--border-color);
}

.chat-meta-bar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: var(--spacing-sm);
  padding: 6px 12px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.05);
  color: var(--text-secondary);
  font-size: var(--font-size-xs);
}

.chat-status-chip {
  display: inline-flex;
  align-items: center;
  padding: 4px 10px;
  border-radius: 999px;
  border: 1px solid var(--border-color);
  background: rgba(255, 255, 255, 0.04);
}

.chat-status-chip.connected {
  color: #8be0a8;
  border-color: rgba(97, 212, 122, 0.45);
}

.chat-status-chip.reconnecting {
  color: #f1d58b;
  border-color: rgba(241, 213, 139, 0.45);
}

.chat-status-chip.offline {
  color: var(--text-muted);
}

.chat-online-count {
  white-space: nowrap;
}

.tab-btn {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--spacing-xs);
  padding: var(--spacing-sm);
  background: transparent;
  border: none;
  border-bottom: 2px solid transparent;
  cursor: pointer;
  transition: all 0.15s ease;
}

.tab-unread-badge {
  min-width: 18px;
  height: 18px;
  padding: 0 5px;
  border-radius: 999px;
  background: rgba(220, 84, 84, 0.9);
  color: #fff;
  font-size: 11px;
  font-weight: 700;
  line-height: 18px;
}

.tab-btn:hover {
  background: var(--button-neutral-hover-bg);
  color: var(--button-neutral-hover-text);
}

.tab-btn.active {
  background: var(--accent-soft-bg);
  border-bottom-color: var(--text-secondary);
}

.tab-btn.active .tab-name {
  color: var(--accent-text);
}

.tab-icon {
  font-size: 16px;
}

.tab-name {
  font-size: var(--font-size-sm);
  color: var(--text-secondary);
}

.chat-messages {
  flex: 1;
  padding: 2px var(--spacing-md) var(--spacing-md);
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.chat-messages::-webkit-scrollbar {
  width: 6px;
}

.chat-messages::-webkit-scrollbar-track {
  background: transparent;
}

.chat-messages::-webkit-scrollbar-thumb {
  background: var(--border-color);
  border-radius: 3px;
}

.chat-messages::-webkit-scrollbar-thumb:hover {
  background: var(--border-color);
}

.chat-message {
  font-size: var(--font-size-sm);
  line-height: 1.6;
  padding: 6px 12px;
  border-radius: var(--radius-md);
  display: flex;
  align-items: flex-start;
  gap: 8px;
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
  margin-bottom: 3px;
  position: relative;
  flex-shrink: 0;
}

.chat-message:hover {
  background: var(--bg-overlay-medium);
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.03);
}

.chat-message.self {
  background: var(--bg-overlay-medium);
  padding-left: 12px;
  border-radius: var(--radius-md);
  position: relative;
  overflow: hidden;
}

.chat-message.self::before {
  content: '';
  position: absolute;
  left: 0;
  top: 0;
  bottom: 0;
  width: 3px;
  background: linear-gradient(180deg, var(--text-primary), var(--text-muted));
  opacity: 0.7;
}

.chat-message.self:hover {
  background: var(--bg-overlay-dark);
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.05);
}

.chat-message.self .msg-sender {
  color: var(--text-primary);
}

.chat-message.system {
  background: rgba(234, 179, 8, 0.04);
  padding-left: 12px;
  border-radius: var(--radius-md);
  position: relative;
  overflow: hidden;
}

.chat-message.system::before {
  content: '';
  position: absolute;
  left: 0;
  top: 0;
  bottom: 0;
  width: 3px;
  background: linear-gradient(180deg, rgba(234, 179, 8, 0.85), rgba(251, 191, 36, 0.45));
}

.chat-message.system:hover {
  background: rgba(234, 179, 8, 0.08);
  box-shadow: 0 2px 6px rgba(234, 179, 8, 0.08);
}

.chat-message.system .msg-sender,
.chat-message.system .msg-content {
  color: #d97706;
  font-weight: 500;
}

[data-theme="dark"] .chat-message.system .msg-sender,
[data-theme="dark"] .chat-message.system .msg-content {
  color: var(--gold-light);
}

[data-theme="elegant"] .chat-message.system .msg-sender,
[data-theme="elegant"] .chat-message.system .msg-content {
  color: var(--gold-dark);
}

.msg-time {
  color: var(--text-muted);
  font-size: 11px;
  margin-left: auto;
  align-self: flex-start;
  margin-top: 3px;
  white-space: nowrap;
  opacity: 0.65;
}

.msg-sender {
  color: var(--text-primary);
  font-weight: 600;
  margin-right: 4px;
  white-space: nowrap;
  flex-shrink: 0;
  align-self: flex-start;
}

.msg-title-wrapper {
  display: inline-flex;
  align-items: center;
  flex-shrink: 0;
  white-space: nowrap;
  align-self: flex-start;
  margin-top: 1px;
}

.msg-title-badge {
  font-size: 10px;
  font-weight: 700;
  padding: 1px 5px;
  border-radius: var(--radius-sm);
  line-height: 1.2;
  text-transform: uppercase;
}

.msg-title-badge.rarity-common {
  background: rgba(148, 163, 184, 0.1);
  color: #94a3b8;
  border: 1px solid rgba(148, 163, 184, 0.15);
}

.msg-title-badge.rarity-rare {
  background: rgba(59, 130, 246, 0.1);
  color: #3b82f6;
  border: 1px solid rgba(59, 130, 246, 0.15);
}

.msg-title-badge.rarity-epic {
  background: rgba(168, 85, 247, 0.1);
  color: #a855f7;
  border: 1px solid rgba(168, 85, 247, 0.15);
}

.msg-title-badge.rarity-legendary {
  background: rgba(245, 158, 11, 0.1);
  color: #f59e0b;
  border: 1px solid rgba(245, 158, 11, 0.15);
  box-shadow: 0 0 6px rgba(245, 158, 11, 0.1);
}

.msg-title-img {
  height: 26px;
  width: auto;
  vertical-align: middle;
  object-fit: contain;
  display: inline-block;
  border-radius: var(--radius-sm);
}

.msg-content {
  color: var(--text-secondary);
  flex: 1;
  min-width: 0;
  word-break: break-all;
}

.empty-hint {
  text-align: center;
  color: var(--text-muted);
  padding: var(--spacing-xl);
  font-style: italic;
}

.chat-input-area {
  display: flex;
  gap: var(--spacing-sm);
  padding: var(--spacing-sm);
  background: var(--bg-overlay-light);
  border-top: 1px solid var(--border-color);
}

.chat-input {
  flex: 1;
  padding: var(--spacing-sm);
  background: var(--xiuxian-bg-primary);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-sm);
  color: var(--text-primary);
  font-size: var(--font-size-sm);
  outline: none;
  transition: border-color 0.15s ease;
}

.chat-input:focus {
  border-color: var(--text-secondary);
}

.send-btn {
  padding: var(--spacing-sm) var(--spacing-md);
  background: var(--button-primary-start);
  border: none;
  border-radius: var(--radius-sm);
  color: var(--button-primary-text);
  font-size: var(--font-size-sm);
  font-weight: 500;
  cursor: pointer;
  transition: all 0.15s ease;
}

.send-btn:hover:not(:disabled) {
  background: var(--button-primary-hover-start);
  color: var(--button-primary-hover-text);
  transform: translateY(-1px);
}

.send-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* 单位数量显示 */
.unit-count {
  font-size: var(--font-size-sm);
  color: var(--text-muted);
  background: var(--bg-overlay-light);
  padding: 2px 8px;
  border-radius: var(--radius-sm);
}

/* 战斗单位扩展样式 */
.battle-unit.dead {
  opacity: 0.4;
  filter: grayscale(100%);
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

/* 响应式调整 */
@media (max-width: 760px) {
  .main-body {
    grid-template-columns: 1fr;
    grid-template-rows: auto auto;
    overflow-y: auto;
  }

  .left-panel {
    width: 100%;
    overflow-y: visible;
  }

  .battle-area {
    overflow-y: visible;
  }

  .chat-array-container {
    flex: none;
    height: auto;
    overflow: visible;
  }

  .five-elements-array-section {
    flex-basis: 380px;
    width: 100%;
    min-width: 0;
  }
}

@media (max-width: 640px) {
  .chat-array-container {
    flex-direction: column;
  }

  .five-elements-array-section {
    flex-basis: auto;
    height: 350px;
  }

  .chat-section {
    min-height: 350px;
  }

  .realm-progress-header,
  .realm-progress-meta,
  .resource-row-head,
  .breakthrough-row {
    flex-direction: column;
    align-items: flex-start;
  }

  .resource-row {
    grid-template-columns: 1fr;
    grid-template-areas:
      'head'
      'track'
      'percent';
  }

  .resource-percent,
  .breakthrough-name,
  .breakthrough-value {
    text-align: left;
  }

  .attribute-section .attribute-item .attr-meta {
    align-items: flex-start;
    justify-content: flex-start;
  }

  .attribute-section .attribute-item .attr-controls {
    margin-left: 0;
  }
}

/* 全局 Tooltip 样式 - Teleport 到 body */
.root-tooltip-teleport {
  position: fixed;
  width: 180px;
  padding: 12px;
  background: var(--xiuxian-bg-panel);
  border: 1px solid var(--border-color);
  border-radius: 8px;
  box-shadow: var(--shadow-lg);
  z-index: 99999;
  text-align: left;
  pointer-events: none;

  .tooltip-title {
    display: flex;
    align-items: center;
    gap: 6px;
    font-size: 16px;
    font-weight: 600;
    color: var(--text-primary);
    margin-bottom: 6px;
    padding-bottom: 6px;
    border-bottom: 1px solid var(--border-color);

    .root-icon {
      font-size: 18px;
    }
  }

  .tooltip-subtitle {
    font-size: 12px;
    color: var(--text-muted);
    margin-bottom: 6px;
  }

  .counter-list {
    display: flex;
    flex-direction: column;
    gap: 2px;
    margin-bottom: 10px;
  }

  .counter-item {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 3px 6px;
    border-radius: 4px;
    font-size: 14px;

    .counter-icon {
      margin-right: 4px;
    }

    .counter-name {
      flex: 1;
      color: var(--text-secondary);
    }

    .counter-value {
      font-weight: 600;
      font-family: var(--font-mono);
    }

    &.advantage {
      background: rgba(46, 125, 50, 0.2);

      .counter-value {
        color: var(--status-success-text);
      }
    }

    &.disadvantage {
      background: rgba(198, 40, 40, 0.2);

      .counter-value {
        color: var(--status-danger-text);
      }
    }
  }

  .tooltip-desc {
    font-size: 12px;
    color: var(--text-secondary);
    font-style: italic;
    line-height: 1.4;
    padding-top: 6px;
    border-top: 1px dashed var(--border-color);
  }
}

/* 装备 Tooltip - Teleport 到 body */
.equipment-tooltip-teleport {
  position: fixed;
  width: 200px;
  padding: 12px;
  background: var(--xiuxian-bg-panel);
  border: 1px solid var(--border-color);
  border-radius: 8px;
  box-shadow: var(--shadow-lg);
  z-index: 99999;
  text-align: left;
  pointer-events: none;

  .tooltip-name {
    font-size: 16px;
    font-weight: 600;
    margin-bottom: 4px;
    padding-bottom: 4px;
    border-bottom: 1px solid var(--border-color);

    &.common { color: var(--quality-common); }
    &.uncommon { color: var(--quality-uncommon); }
    &.rare { color: var(--quality-rare); }
    &.epic { color: var(--quality-epic); }
    &.legendary { color: var(--quality-legendary); }
    &.mythic { color: var(--quality-mythic); }
  }

  .tooltip-type {
    font-size: 13px;
    color: var(--text-muted);
    margin-bottom: 8px;
  }

  .tooltip-stats {
    display: flex;
    flex-direction: column;
    gap: 2px;
    margin-bottom: 10px;

    .tooltip-stat {
      font-size: 14px;
      color: var(--accent-text);
    }
  }

  .tooltip-gem-slots {
    display: flex;
    flex-wrap: wrap;
    gap: 4px;
    margin-bottom: 8px;
    padding-bottom: 8px;
    border-bottom: 1px dashed var(--border-color);
  }

  .tooltip-gem-tag {
    display: inline-flex;
    align-items: center;
    gap: 3px;
    padding: 2px 8px;
    font-size: 12px;
    color: #a78bfa;
    background: rgba(167, 139, 250, 0.1);
    border: 1px solid rgba(167, 139, 250, 0.2);
    border-radius: 4px;

    &::before {
      content: '◆';
      font-size: 8px;
    }
  }

  .tooltip-desc {
    font-size: 12px;
    color: var(--text-secondary);
    font-style: italic;
    line-height: 1.4;
    padding-top: 8px;
    border-top: 1px dashed var(--border-color);
  }
}

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
