<template>
  <XiuXianModal
    :model-value="modelValue"
    title="宗门"
    :width="960"
    @update:model-value="$emit('update:modelValue', $event)"
  >
    <div class="sect-modal">
      <!-- 顶部标签页导航 -->
      <div class="sect-tabs">
        <button
          v-for="tab in tabs"
          :key="tab.key"
          class="tab-btn"
          :class="{ active: activeTab === tab.key, disabled: tab.key !== 'overview' && !playerHasGuild }"
          :disabled="tab.key !== 'overview' && !playerHasGuild"
          @click="tab.key === 'overview' || playerHasGuild ? (activeTab = tab.key) : null"
        >
          <span class="tab-icon"><AssetIcon :source="tab.icon" size="14" /></span>
          <span class="tab-name">{{ tab.label }}</span>
        </button>
      </div>

      <!-- 状态消息 -->
      <div v-if="statusMessage" class="status-message">{{ statusMessage }}</div>

      <!-- Tab 1: 宗门总览 -->
      <div v-if="activeTab === 'overview'" class="tab-content">
        <!-- 未加入宗门：显示可选宗门列表 -->
        <template v-if="!playerHasGuild">
          <div class="tab-header">
            <div class="tab-header-title">选择宗门</div>
            <button class="refresh-btn" :disabled="isLoading" @click="loadSectTemplates">
              {{ isLoading ? '刷新中...' : '刷新' }}
            </button>
          </div>

          <div v-if="selectedSect" class="sect-detail-card">
            <div class="sect-detail-header">
              <span class="sect-detail-icon"><AssetIcon :source="selectedSect.icon" size="25" /></span>
              <div>
                <div class="sect-detail-name">{{ selectedSect.name }}</div>
                <div class="sect-detail-meta">{{ selectedSect.memberCount }} 人</div>
              </div>
            </div>
            <div class="sect-detail-desc">{{ selectedSect.description }}</div>

            <div v-if="selectedSect.heartSutras && selectedSect.heartSutras.length > 0" class="sect-sutra-preview">
              <div class="sect-sutra-preview-title">宗门心法</div>
              <div v-for="hs in selectedSect.heartSutras" :key="hs.sutraId" class="sect-sutra-preview-item">
                <span class="sect-sutra-preview-name">{{ hs.name }}</span>
                <span class="sect-sutra-preview-layers">{{ hs.maxLayer }}层</span>
                <div class="sect-sutra-preview-bonuses">
                  <span v-for="bonus in (hs.previewBonuses || [])" :key="bonus.attr" class="bonus-tag">
                    {{ formatAttrName(bonus.attr) }}+{{ bonus.isPercent ? (bonus.val * 100).toFixed(0) + '%' : bonus.val }}
                  </span>
                </div>
              </div>
            </div>

            <div class="sect-detail-actions">
              <button class="secondary-btn" @click="selectedSect = null">返回列表</button>
              <button class="primary-btn" :disabled="isSubmitting" @click="joinSect(selectedSect.sectId)">
                {{ isSubmitting ? '处理中...' : '加入宗门' }}
              </button>
            </div>
          </div>

          <div v-else class="sect-grid">
            <div v-if="sectTemplates.length === 0 && !isLoading" class="empty-state">
              <div class="empty-icon"><AssetIcon :source="ICON.misc_castle" size="25" /></div>
              <div class="empty-text">暂无可选宗门。</div>
            </div>
            <button
              v-for="sect in sectTemplates"
              :key="sect.sectId"
              class="sect-card"
              @click="selectSectForDetail(sect)"
            >
              <div class="sect-card-icon"><AssetIcon :source="sect.icon" size="25" /></div>
              <div class="sect-card-name">{{ sect.name }}</div>
              <div class="sect-card-desc">{{ sect.descriptionSnippet }}</div>
              <div class="sect-card-meta">{{ sect.memberCount }} 人</div>
            </button>
          </div>
        </template>

        <!-- 已加入宗门：显示宗门信息 -->
        <template v-else>
          <div class="tab-header">
            <div class="tab-header-title">{{ sectInfo.name || '我的宗门' }}</div>
            <button class="refresh-btn" :disabled="isLoading" @click="loadSectInfo">
              {{ isLoading ? '刷新中...' : '刷新' }}
            </button>
          </div>

          <div class="sect-overview">
            <div class="sect-overview-card">
              <div class="sect-overview-row">
                <div class="sect-overview-name"><AssetIcon :source="sectInfo.icon" size="18" /> {{ sectInfo.name }}</div>
                <span class="guild-level">Lv.{{ sectInfo.level }}</span>
              </div>
              <div class="sect-overview-meta">
                <span>宗主 {{ sectInfo.leaderName }}</span>
                <span>{{ sectInfo.memberCount }}/{{ sectInfo.maxMembers }} 人</span>
              </div>
              <div v-if="sectInfo.announcement" class="sect-overview-announcement">
                公告：{{ sectInfo.announcement }}
              </div>
              <div v-if="sectInfo.description" class="sect-overview-desc">
                {{ sectInfo.description }}
              </div>
            </div>

            <div class="sect-overview-stats">
              <div class="stat-card">
                <span class="stat-label">宗门贡献</span>
                <strong class="stat-value">{{ formatNum(playerContribution) }}</strong>
              </div>
              <div class="stat-card">
                <span class="stat-label">宗门等级</span>
                <strong class="stat-value">Lv.{{ sectInfo.level }}</strong>
              </div>
              <div class="stat-card">
                <span class="stat-label">成员数量</span>
                <strong class="stat-value">{{ sectInfo.memberCount }}</strong>
              </div>
            </div>
          </div>
        </template>
      </div>

      <!-- Tab 2: 心法 -->
      <div v-if="activeTab === 'sutras'" class="tab-content">
        <div class="tab-header">
          <div class="tab-header-title">心法列表</div>
          <button class="refresh-btn" :disabled="isLoading" @click="loadSutras">
            {{ isLoading ? '刷新中...' : '刷新' }}
          </button>
        </div>

        <div v-if="sutras.length === 0" class="empty-state">
          <div class="empty-icon"><AssetIcon :source="ICON.misc_book_open" size="25" /></div>
          <div class="empty-text">{{ playerHasGuild ? '暂无可用心法。' : '请先加入宗门。' }}</div>
        </div>

        <div v-else class="sutra-list">
          <div v-for="sutra in sutras" :key="sutra.sutraId" class="sutra-card">
            <div class="sutra-header" @click="toggleSutraExpand(sutra.sutraId)">
              <div class="sutra-info">
                <span class="sutra-icon"><AssetIcon :source="sutra.icon" size="24" /></span>
                <div>
                  <div class="sutra-name">{{ sutra.name }}</div>
                  <div class="sutra-desc">{{ sutra.description }}</div>
                </div>
              </div>
              <div class="sutra-progress-badge">
                {{ sutra.currentLayer }}/{{ sutra.maxLayer }} 层
              </div>
            </div>

            <div v-if="expandedSutras.has(sutra.sutraId)" class="sutra-layers">
              <div
                v-for="layer in sutra.layers"
                :key="layer.layerNumber"
                class="sutra-layer"
                :class="{
                  current: layer.layerNumber === sutra.currentLayer,
                  locked: layer.layerNumber > sutra.currentLayer + 1,
                  available: layer.layerNumber === sutra.currentLayer + 1
                }"
              >
                <div class="layer-header">
                  <span class="layer-number">第{{ layer.layerNumber }}层</span>
                  <span class="layer-name">{{ layer.name }}</span>
                  <span v-if="layer.layerNumber === sutra.currentLayer" class="layer-status current">当前</span>
                  <span v-else-if="layer.layerNumber <= sutra.currentLayer" class="layer-status learned">已学</span>
                  <span v-else class="layer-status locked">未解锁</span>
                </div>

                <div class="layer-bonuses">
                  <span v-for="bonus in layer.bonuses" :key="bonus.name" class="layer-bonus">
                    {{ formatAttrName(bonus.name) }} +{{ bonus.isPercentage ? (bonus.value * 100).toFixed(0) + '%' : bonus.value }}
                  </span>
                </div>

                <div v-if="layer.skillUnlock" class="layer-unlock">
                  <span class="unlock-icon"><AssetIcon :source="ICON.misc_fire" size="12" /></span>
                  <span>解锁技能：{{ layer.skillUnlock }}</span>
                </div>

                <div v-if="layer.buffUnlock" class="layer-unlock">
                  <span class="unlock-icon"><AssetIcon :source="ICON.misc_sparkles" size="12" /></span>
                  <span>解锁增益：{{ layer.buffUnlock }}</span>
                </div>

                <button
                  v-if="layer.layerNumber === sutra.currentLayer + 1"
                  class="primary-btn sm"
                  :disabled="isSubmitting"
                  @click="upgradeSutra(sutra.sutraId)"
                >
                  {{ isSubmitting ? '修炼中...' : `修炼 (贡献${layer.contributionCost} + ${layer.goldCost}金)` }}
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Tab 3: 捐献 -->
      <div v-if="activeTab === 'donation'" class="tab-content">
        <div class="tab-header">
          <div class="tab-header-title">每日捐献</div>
          <button class="refresh-btn" :disabled="isLoading" @click="loadDonationStatus">
            {{ isLoading ? '刷新中...' : '刷新' }}
          </button>
        </div>

        <div class="donation-panel">
          <div class="donation-balances">
            <div class="donation-balance-card">
              <span class="donation-balance-icon"><AssetIcon :source="ICON.misc_coin" size="24" /></span>
              <div class="donation-balance-info">
                <span class="donation-balance-label">当前金币</span>
                <span class="donation-balance-value">{{ formatNum(playerGold) }}</span>
              </div>
            </div>
            <div class="donation-balance-card">
              <span class="donation-balance-icon"><AssetIcon :source="ICON.misc_castle" size="24" /></span>
              <div class="donation-balance-info">
                <span class="donation-balance-label">当前贡献</span>
                <span class="donation-balance-value">{{ formatNum(playerContribution) }}</span>
              </div>
            </div>
          </div>

          <div v-if="donationStatus.hasDonated" class="donation-done">
            <div class="donation-done-icon"><AssetIcon :source="ICON.misc_checkmark" size="25" /></div>
            <div class="donation-done-text">今日已捐献</div>
            <div class="donation-reward-info">
              获得贡献 +{{ donationStatus.contributionReward }}
            </div>
          </div>

          <div v-else class="donation-form">
            <div class="donation-amounts">
              <button
                v-for="amount in donationAmounts"
                :key="amount"
                class="donation-amount-btn"
                :class="{ active: donationGold === amount, 'donation-amount-btn--premium': amount >= 10000 }"
                :disabled="playerGold < amount"
                @click="donationGold = amount"
              >
                <span class="amount-icon"><AssetIcon :source="ICON.misc_coin" size="16" /></span>
                <span class="amount-value">{{ formatNum(amount) }}</span>
                <span class="amount-reward">贡献 +{{ amount / 100 }}</span>
              </button>
            </div>
            <div class="donation-tip">每捐献 1,000 金币可获得 10 贡献</div>
            <button
              class="primary-btn donation-submit-btn"
              :disabled="isSubmitting || donationGold <= 0 || playerGold < donationGold"
              @click="donateSect"
            >
              {{ isSubmitting ? '捐献中...' : `捐献 ${formatNum(donationGold)} 金币` }}
            </button>
          </div>
        </div>
      </div>

      <!-- Tab 4: 宗门任务 -->
      <div v-if="activeTab === 'tasks'" class="tab-content">
        <div class="tab-header">
          <div class="tab-header-title">宗门任务</div>
          <button class="refresh-btn" :disabled="isLoading" @click="loadSectTasks">
            {{ isLoading ? '刷新中...' : '刷新' }}
          </button>
        </div>

        <div v-if="sectTasks.length === 0" class="empty-state">
          <div class="empty-icon"><AssetIcon :source="ICON.misc_scroll" size="25" /></div>
          <div class="empty-text">暂无可用的宗门任务。</div>
        </div>

        <div v-else class="task-list">
          <div v-for="task in sectTasks" :key="task.questId" class="task-card">
            <div class="task-header">
              <div class="task-info">
                <span class="task-icon"><AssetIcon :source="task.icon" size="24" /></span>
                <div>
                  <div class="task-name">{{ task.name }}</div>
                  <div class="task-type">{{ task.typeText }}</div>
                </div>
              </div>
              <span class="status-badge" :class="task.statusClass">{{ task.statusText }}</span>
            </div>

            <div class="task-desc">{{ task.description }}</div>

            <div class="progress-block">
              <div class="progress-row">
                <span>进度</span>
                <span>{{ task.currentProgress }}/{{ task.targetProgress }}</span>
              </div>
              <div class="progress-track">
                <span class="progress-fill" :style="{ width: task.progressPercent + '%' }"></span>
              </div>
            </div>

            <div class="task-rewards">
              <span class="reward-tag"><AssetIcon :source="ICON.misc_castle" size="14" /> 贡献 +{{ task.contributionReward }}</span>
              <span v-if="task.goldReward > 0" class="reward-tag"><AssetIcon :source="ICON.misc_coin" size="14" /> 金币 +{{ task.goldReward }}</span>
            </div>

            <div class="task-actions">
              <button
                v-if="task.rewardClaimed"
                class="primary-btn sm"
                disabled
              >
                已领取
              </button>
              <button
                v-else-if="task.isCompleted"
                class="primary-btn sm"
                :disabled="isSubmitting"
                @click="submitSectTask(task.questId)"
              >
                {{ isSubmitting ? '提交中...' : '领取奖励' }}
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Tab 5: 弟子列表 -->
      <div v-if="activeTab === 'disciples'" class="tab-content">
        <div class="tab-header">
          <div class="tab-header-title">弟子列表</div>
          <button class="refresh-btn" :disabled="isLoading" @click="loadDisciples">
            {{ isLoading ? '刷新中...' : '刷新' }}
          </button>
        </div>

        <div v-if="disciples.length === 0" class="empty-state">
          <div class="empty-icon"><AssetIcon :source="ICON.misc_group" size="25" /></div>
          <div class="empty-text">暂无弟子信息。</div>
        </div>

        <div v-else class="disciple-table">
          <div class="disciple-table-header">
            <span class="col-name">名字</span>
            <span class="col-level">等级</span>
            <span class="col-element">灵根</span>
            <span class="col-profession">职业</span>
            <span class="col-contribution">贡献</span>
            <span class="col-winrate">胜率</span>
            <span class="col-action">操作</span>
          </div>
          <div v-for="disciple in disciples" :key="disciple.playerId" class="disciple-row">
            <span class="col-name">{{ disciple.name }}</span>
            <span class="col-level">Lv.{{ disciple.level }}</span>
            <span class="col-element">
              <span class="element-badge" :class="disciple.element">{{ disciple.elementName }}</span>
            </span>
            <span class="col-profession">{{ disciple.professionName }}</span>
            <span class="col-contribution">{{ formatNum(disciple.contribution) }}</span>
            <span class="col-winrate">{{ disciple.winRate }}%</span>
            <span class="col-action">
              <button
                v-if="!disciple.isSelf"
                class="action-btn"
                :disabled="isSubmitting"
                @click="startSpar(disciple.playerId)"
              >
                切磋
              </button>
              <span v-else class="self-tag">自己</span>
            </span>
          </div>
        </div>

        <!-- 切磋结果 -->
        <div v-if="sparResult" class="spar-result">
          <div class="spar-result-header">
            <span>切磋结果</span>
            <button class="close-btn" @click="sparResult = null">×</button>
          </div>
          <div class="spar-result-body">
            <div class="spar-vs">
              <span class="spar-player">{{ sparResult.attackerName }}</span>
              <span class="spar-vs-text">VS</span>
              <span class="spar-player">{{ sparResult.defenderName }}</span>
            </div>
            <div class="spar-outcome" :class="{ win: sparResult.isWin }">
              {{ sparResult.isWin ? '胜利' : '失败' }}
            </div>
            <div class="spar-rounds">共 {{ sparResult.totalRounds }} 回合</div>
            <div v-if="sparResult.log" class="spar-log">{{ sparResult.log }}</div>
          </div>
        </div>
      </div>

      <!-- Tab 6: 宗门大比 -->
      <div v-if="activeTab === 'tournament'" class="tab-content">
        <div class="tab-header">
          <div class="tab-header-title">宗门大比</div>
          <button class="refresh-btn" :disabled="isLoading" @click="loadSectTournament">
            {{ isLoading ? '刷新中...' : '刷新' }}
          </button>
        </div>

        <div v-if="!tournament" class="empty-state">
          <div class="empty-icon"><AssetIcon :source="ICON.misc_trophy" size="25" /></div>
          <div class="empty-text">暂无进行中的宗门大比。</div>
        </div>

        <div v-else class="tournament-panel">
          <div class="tournament-status-bar">
            <span class="tournament-state" :class="tournament.state">{{ tournament.stateText }}</span>
            <span v-if="tournament.state === 'upcoming'" class="tournament-countdown">
              距离开始：{{ tournament.countdownText }}
            </span>
            <span v-if="tournament.state === 'active'" class="tournament-round">
              第 {{ tournament.currentRound }} 轮
            </span>
          </div>

          <div v-if="tournament.state === 'settled' && tournament.hasReward" class="tournament-reward-bar">
            <button class="primary-btn" :disabled="isSubmitting" @click="claimTournamentReward">
              {{ isSubmitting ? '领取中...' : '领取奖励' }}
            </button>
          </div>

          <div class="bracket-view">
            <div v-for="round in tournamentMatches" :key="round.roundNumber" class="bracket-round">
              <div class="round-title">第{{ round.roundNumber }}轮</div>
              <div v-for="match in round.matches" :key="match.matchId" class="bracket-match">
                <div class="match-player" :class="{ winner: match.winnerId === match.player1Id }">
                  <span class="match-name">{{ match.player1Name }}</span>
                  <span v-if="match.player1Score !== undefined" class="match-score">{{ match.player1Score }}</span>
                </div>
                <div class="match-vs">VS</div>
                <div class="match-player" :class="{ winner: match.winnerId === match.player2Id }">
                  <span class="match-name">{{ match.player2Name }}</span>
                  <span v-if="match.player2Score !== undefined" class="match-score">{{ match.player2Score }}</span>
                </div>
              </div>
            </div>
            <div v-if="tournamentMatches.length === 0" class="empty-state">
              <div class="empty-text">暂无对战数据。</div>
            </div>
          </div>
        </div>
      </div>

      <!-- Tab 7: 天骄赛 -->
      <div v-if="activeTab === 'genius'" class="tab-content">
        <div class="tab-header">
          <div class="tab-header-title">天骄赛</div>
          <button class="refresh-btn" :disabled="isLoading" @click="loadGeniusTournament">
            {{ isLoading ? '刷新中...' : '刷新' }}
          </button>
        </div>

        <div v-if="!geniusTournament" class="empty-state">
          <div class="empty-icon"><AssetIcon :source="ICON.misc_star" size="25" /></div>
          <div class="empty-text">暂无进行中的天骄赛。</div>
        </div>

        <div v-else class="tournament-panel">
          <div class="tournament-status-bar">
            <span class="tournament-state" :class="geniusTournament.state">{{ geniusTournament.stateText }}</span>
            <span v-if="geniusTournament.state === 'upcoming'" class="tournament-countdown">
              距离开始：{{ geniusTournament.countdownText }}
            </span>
            <span v-if="geniusTournament.state === 'active'" class="tournament-round">
              第 {{ geniusTournament.currentRound }} 轮
            </span>
          </div>

          <div v-if="geniusTournament.state === 'settled' && geniusTournament.hasReward" class="tournament-reward-bar">
            <button class="primary-btn" :disabled="isSubmitting" @click="claimGeniusReward">
              {{ isSubmitting ? '领取中...' : '领取奖励' }}
            </button>
          </div>

          <div class="bracket-view">
            <div v-for="round in geniusMatches" :key="round.roundNumber" class="bracket-round">
              <div class="round-title">第{{ round.roundNumber }}轮</div>
              <div v-for="match in round.matches" :key="match.matchId" class="bracket-match">
                <div class="match-player" :class="{ winner: match.winnerId === match.player1Id }">
                  <span class="match-sect">{{ match.player1SectName }}</span>
                  <span class="match-name">{{ match.player1Name }}</span>
                  <span v-if="match.player1Score !== undefined" class="match-score">{{ match.player1Score }}</span>
                </div>
                <div class="match-vs">VS</div>
                <div class="match-player" :class="{ winner: match.winnerId === match.player2Id }">
                  <span class="match-sect">{{ match.player2SectName }}</span>
                  <span class="match-name">{{ match.player2Name }}</span>
                  <span v-if="match.player2Score !== undefined" class="match-score">{{ match.player2Score }}</span>
                </div>
              </div>
            </div>
            <div v-if="geniusMatches.length === 0" class="empty-state">
              <div class="empty-text">暂无对战数据。</div>
            </div>
          </div>
        </div>
      </div>

      <!-- Tab 8: 宗门Boss -->
      <div v-if="activeTab === 'boss'" class="tab-content">
        <div class="tab-header">
          <div class="tab-header-title">宗门Boss</div>
          <div class="tab-header-actions">
            <button class="refresh-btn" :disabled="isLoading" @click="loadSectBoss">
              {{ isLoading ? '刷新中...' : '刷新' }}
            </button>
            <button
              v-if="bossStatus.hasPendingReward"
              class="reward-btn"
              :disabled="isSubmitting"
              @click="claimSectBossReward"
            >
              {{ isSubmitting ? '领取中...' : '领取奖励' }}
            </button>
          </div>
        </div>

        <div v-if="!bossStatus.hasActiveBoss" class="empty-state">
          <div class="empty-icon"><AssetIcon :source="ICON.creature_demon" size="25" /></div>
          <div class="empty-text">当前没有开启中的宗门Boss。</div>
        </div>

        <div v-else class="boss-panel">
          <div class="boss-layout">
            <!-- Boss 信息 -->
            <div class="boss-card">
              <div class="boss-portrait-row">
                <div class="boss-portrait">
                  <span class="boss-portrait-icon"><AssetIcon :source="ICON.creature_demon" size="25" /></span>
                </div>
                <div class="boss-info">
                  <div class="boss-name">{{ bossStatus.bossName }}</div>
                  <div class="boss-state">{{ bossStatus.state }}</div>
                </div>
              </div>

              <div class="boss-hp-bar">
                <div class="boss-hp-head">
                  <span>气血</span>
                  <strong>{{ formatNum(bossStatus.currentHp) }}/{{ formatNum(bossStatus.maxHp) }}</strong>
                </div>
                <div class="boss-track">
                  <div class="boss-fill hp" :style="{ width: bossHpPercent + '%' }"></div>
                </div>
              </div>
            </div>

            <!-- 玩家状态 -->
            <div class="boss-card">
              <div class="boss-player-header">
                <div>
                  <div class="boss-section-title">我的战斗状态</div>
                  <div class="boss-player-name">{{ bossStatus.playerName || '未参战' }}</div>
                </div>
                <button
                  v-if="!bossStatus.isJoined"
                  class="primary-btn sm"
                  :disabled="isSubmitting"
                  @click="joinSectBoss"
                >
                  {{ isSubmitting ? '加入中...' : '参战' }}
                </button>
              </div>

              <div class="boss-player-stats">
                <div class="boss-stat-item">
                  <span class="boss-stat-label">总伤害</span>
                  <strong>{{ formatNum(bossStatus.myDamage) }}</strong>
                </div>
                <div class="boss-stat-item">
                  <span class="boss-stat-label">排名</span>
                  <strong>{{ bossStatus.myRank || '-' }}</strong>
                </div>
              </div>

              <div v-if="bossStatus.isJoined" class="boss-skill-strip">
                <button
                  class="boss-skill-btn"
                  :disabled="bossActionDisabled"
                  @click="bossAction('normal')"
                >
                  <span class="skill-icon"><AssetIcon :source="ICON.misc_sword_crossed" size="18" /></span>
                  <span class="skill-name">普通攻击</span>
                </button>
                <button
                  v-for="skill in bossSkills"
                  :key="skill.skillId"
                  class="boss-skill-btn"
                  :class="{ disabled: !skill.canUse }"
                  :disabled="bossActionDisabled || !skill.canUse"
                  @click="bossAction('skill', skill.skillId)"
                >
                  <span class="skill-icon"><AssetIcon :source="skill.icon" size="18" /></span>
                  <span class="skill-name">{{ skill.name }}</span>
                  <span class="skill-cd">CD {{ skill.currentCooldown }}/{{ skill.cooldown }}</span>
                </button>
              </div>
            </div>
          </div>

          <!-- 排行 -->
          <div v-if="bossRanking.length > 0" class="boss-ranking">
            <div class="boss-section-title">伤害排行</div>
            <div class="boss-rank-list">
              <div
                v-for="item in bossRanking"
                :key="item.playerId"
                class="boss-rank-item"
                :class="{ me: item.isSelf }"
              >
                <span class="rank-no">{{ item.rank }}</span>
                <span class="rank-name">{{ item.playerName }}</span>
                <span class="rank-damage">{{ formatNum(item.totalDamage) }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Tab 9: 宗门商店 -->
      <div v-if="activeTab === 'shop'" class="tab-content">
        <div class="tab-header">
          <div class="tab-header-title">宗门商店</div>
          <div class="tab-header-actions">
            <span class="contribution-display"><AssetIcon :source="ICON.misc_castle" size="14" /> 贡献 {{ formatNum(playerContribution) }}</span>
            <button class="refresh-btn" :disabled="isLoading" @click="loadSectShop">
              {{ isLoading ? '刷新中...' : '刷新' }}
            </button>
          </div>
        </div>

        <div v-if="shopItems.length === 0" class="empty-state">
          <div class="empty-icon"><AssetIcon :source="ICON.nav_shop" size="25" /></div>
          <div class="empty-text">商店暂无商品。</div>
        </div>

        <div v-else class="shop-grid">
          <div v-for="item in shopItems" :key="item.itemId" class="shop-item">
            <div class="shop-item-icon"><AssetIcon :source="item.icon" size="25" /></div>
            <div class="shop-item-info">
              <div class="shop-item-name" :class="item.quality">{{ item.name }}</div>
              <div class="shop-item-desc">{{ item.description }}</div>
              <div class="shop-item-stock">库存: {{ item.stock }}</div>
            </div>
            <div class="shop-item-price">
              <span class="price-icon"><AssetIcon :source="ICON.misc_castle" size="14" /></span>
              <span class="price-value">{{ formatNum(item.contributionCost) }}</span>
            </div>
            <button
              class="buy-btn"
              :disabled="item.stock <= 0 || playerContribution < item.contributionCost || isSubmitting"
              @click="purchaseSectItem(item.itemId)"
            >
              {{ isSubmitting ? '购买中...' : '购买' }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </XiuXianModal>
</template>

<script setup>
import { computed, reactive, ref, watch } from 'vue'
import XiuXianModal from '../common/XiuXianModal.vue'
import AssetIcon from '../common/AssetIcon.vue'
import { ICON } from '../../icons'
import { apiClient } from '../../lib/apiClient'
import { useGameStore } from '../../state/gameStore'

/*
 * 中文注释：
 * 宗门弹窗取代原 GuildModal，采用内部标签页实现九个功能模块。
 * 每个标签页的数据只在首次切换到该页时加载（懒加载），
 * 减少弹窗打开时的并发请求数量。
 */

const props = defineProps({
  modelValue: Boolean
})

const emit = defineEmits(['update:modelValue'])

const gameStore = useGameStore()

// 通用状态
const activeTab = ref('overview')
const isLoading = ref(false)
const isSubmitting = ref(false)
const statusMessage = ref('')
const loadedTabs = reactive({
  overview: false,
  sutras: false,
  donation: false,
  tasks: false,
  disciples: false,
  tournament: false,
  genius: false,
  boss: false,
  shop: false
})

// 标签页配置
const tabs = [
  { key: 'overview', label: '宗门总览', icon: ICON.misc_castle },
  { key: 'sutras', label: '心法', icon: ICON.misc_book_open },
  { key: 'donation', label: '捐献', icon: ICON.misc_money_bag },
  { key: 'tasks', label: '宗门任务', icon: ICON.misc_scroll },
  { key: 'disciples', label: '弟子列表', icon: ICON.misc_group },
  { key: 'tournament', label: '宗门大比', icon: ICON.misc_trophy },
  { key: 'genius', label: '天骄赛', icon: ICON.misc_star },
  { key: 'boss', label: '宗门Boss', icon: ICON.creature_demon },
  { key: 'shop', label: '宗门商店', icon: ICON.nav_shop }
]

// 玩家相关计算属性
const player = computed(() => gameStore.state.player || {})
const playerHasGuild = computed(() => {
  return sectTemplates.value.some((s) => s.isJoined)
})
const playerGold = computed(() => Number(player.value?.gold ?? player.value?.Gold ?? 0) || 0)
const playerContribution = computed(() => {
  return Number(player.value?.guildContribution ?? player.value?.GuildContribution ?? 0) || 0
})

// ==================== Tab 1: 宗门总览 ====================
const sectTemplates = ref([])
const selectedSect = ref(null)
const joinedSectId = ref('')
const sectInfo = reactive({
  sectId: '',
  name: '',
  icon: ICON.misc_castle,
  level: 1,
  leaderName: '',
  memberCount: 0,
  maxMembers: 0,
  announcement: '',
  description: ''
})

function normalizeSectTemplate(raw) {
  return {
    sectId: raw?.SectId ?? raw?.sectId ?? raw?.Id ?? raw?.id ?? '',
    name: raw?.Name ?? raw?.name ?? '未命名宗门',
    icon: raw?.Icon || raw?.icon || ICON.misc_castle,
    description: raw?.Description ?? raw?.description ?? '暂无描述',
    descriptionSnippet: String(raw?.Description ?? raw?.description ?? '暂无描述').slice(0, 60),
    memberCount: Number(raw?.MemberCount ?? raw?.memberCount ?? 0) || 0,
    isJoined: Boolean(raw?.IsJoined ?? raw?.isJoined ?? false)
  }
}

async function selectSectForDetail(sect) {
  selectedSect.value = { ...sect }
  try {
    const detail = await apiClient.getSectDetail(sect.sectId)
    if (detail?.heartSutras) {
      selectedSect.value.heartSutras = detail.heartSutras.map(hs => {
        const layers = hs.layers || hs.Layers || []
        // 取第一层的加成作为预览
        const firstLayer = layers[0]
        const previewBonuses = firstLayer
          ? (firstLayer.bonuses || firstLayer.Bonuses || []).map(b => ({
              attr: b.attributeName || b.AttributeName || '',
              val: b.value || b.Value || 0,
              isPercent: b.isPercentage || b.IsPercentage || false
            }))
          : []
        return {
          sutraId: hs.sutraId || hs.SutraId || '',
          name: hs.name || hs.Name || '未知心法',
          maxLayer: hs.maxLayer || hs.MaxLayer || layers.length,
          previewBonuses
        }
      })
    }
  } catch {
    // 加载详情失败不影响基本显示
  }
}

function normalizeSectInfo(raw) {
  return {
    sectId: raw?.SectId ?? raw?.sectId ?? raw?.GuildId ?? raw?.guildId ?? '',
    name: raw?.Name ?? raw?.name ?? raw?.GuildName ?? raw?.guildName ?? '未命名宗门',
    icon: raw?.Icon || raw?.icon || ICON.misc_castle,
    level: Number(raw?.Level ?? raw?.level ?? raw?.GuildLevel ?? raw?.guildLevel ?? 1) || 1,
    leaderName: raw?.LeaderName ?? raw?.leaderName ?? raw?.MasterName ?? raw?.masterName ?? '未知宗主',
    memberCount: Number(raw?.MemberCount ?? raw?.memberCount ?? 0) || 0,
    maxMembers: Number(raw?.MaxMembers ?? raw?.maxMembers ?? 0) || 0,
    announcement: raw?.Announcement ?? raw?.announcement ?? '',
    description: raw?.Description ?? raw?.description ?? ''
  }
}

async function loadSectTemplates() {
  isLoading.value = true
  try {
    const result = await apiClient.getSectTemplates()
    sectTemplates.value = Array.isArray(result) ? result.map(normalizeSectTemplate) : []
    statusMessage.value = ''
  } catch (error) {
    statusMessage.value = error.message || '加载宗门列表失败。'
  } finally {
    isLoading.value = false
  }
}

async function loadSectInfo() {
  isLoading.value = true
  try {
    await gameStore.refreshPlayerSnapshot({ includeRankings: false })
    const templates = await apiClient.getSectTemplates()
    const joinedSect = Array.isArray(templates)
      ? templates.find((t) => t?.IsJoined ?? t?.isJoined ?? false)
      : null
    if (joinedSect) {
      const sectId = joinedSect.SectId ?? joinedSect.sectId ?? ''
      joinedSectId.value = sectId
      const detail = await apiClient.getSectDetail(sectId)
      Object.assign(sectInfo, normalizeSectInfo(detail || joinedSect))
    }
    statusMessage.value = ''
  } catch (error) {
    statusMessage.value = error.message || '加载宗门信息失败。'
  } finally {
    isLoading.value = false
  }
}

async function joinSect(sectId) {
  if (!sectId || isSubmitting.value) return
  isSubmitting.value = true
  try {
    const result = await apiClient.joinSect(sectId)
    statusMessage.value = result?.message || result?.Message || '已加入宗门。'
    selectedSect.value = null
    await gameStore.refreshPlayerSnapshot({ includeRankings: false })
    loadedTabs.overview = false
    await loadOverviewTab()
  } catch (error) {
    statusMessage.value = error.message || '加入宗门失败。'
  } finally {
    isSubmitting.value = false
  }
}

async function loadOverviewTab() {
  if (loadedTabs.overview) return
  // Always load templates first so playerHasGuild can be computed
  await loadSectTemplates()
  if (playerHasGuild.value) {
    await loadSectInfo()
  }
  loadedTabs.overview = true
}

// ==================== Tab 2: 心法 ====================
const sutras = ref([])
const expandedSutras = reactive(new Set())

const ATTR_NAME_MAP = {
  Type1: '生命', Type2: '法力', Type3: '物攻', Type4: '法攻',
  Type5: '物防', Type6: '法防', Type7: '速度', Type8: '命中',
  Type9: '暴击', Type10: '暴伤', Type11: '连击', Type12: '反击',
  Type13: '破甲', Type14: '增伤', Type15: '闪避'
}

function formatAttrName(attrName) {
  if (!attrName) return ''
  return ATTR_NAME_MAP[attrName] || attrName
}

function normalizeSutra(raw, playerProgress) {
  const layers = Array.isArray(raw?.layers ?? raw?.Layers)
    ? (raw?.layers ?? raw?.Layers).map((layer) => ({
      layerNumber: Number(layer?.layer ?? layer?.Layer ?? layer?.layerNumber ?? 0) || 0,
      name: layer?.name ?? layer?.Name ?? '',
      bonuses: Array.isArray(layer?.bonuses ?? layer?.Bonuses)
        ? (layer?.bonuses ?? layer?.Bonuses).map((b) => ({
          name: b?.attributeName ?? b?.AttributeName ?? '',
          value: b?.value ?? b?.Value ?? 0,
          isPercentage: b?.isPercentage ?? b?.IsPercentage ?? false
        }))
        : [],
      unlockSkillId: layer?.unlockSkillId ?? layer?.UnlockSkillId ?? null,
      unlockBuffId: layer?.unlockBuffId ?? layer?.UnlockBuffId ?? null,
      skillUnlock: (layer?.unlockSkillName ?? layer?.UnlockSkillName)
        || (layer?.unlockSkillId ?? layer?.UnlockSkillId ? `技能#${layer?.unlockSkillId ?? layer?.UnlockSkillId}` : null),
      buffUnlock: (layer?.unlockBuffName ?? layer?.UnlockBuffName)
        || (layer?.unlockBuffId ?? layer?.UnlockBuffId ? `增益#${layer?.unlockBuffId ?? layer?.UnlockBuffId}` : null),
      contributionCost: Number(layer?.contributionCost ?? layer?.ContributionCost ?? 0) || 0,
      goldCost: Number(layer?.goldCost ?? layer?.GoldCost ?? 0) || 0
    }))
    : []

  return {
    sutraId: raw?.sutraId ?? raw?.SutraId ?? raw?.id ?? raw?.Id ?? '',
    name: raw?.name ?? raw?.Name ?? '未知心法',
    icon: raw?.icon || raw?.Icon || ICON.misc_book_open,
    description: raw?.description ?? raw?.Description ?? '',
    currentLayer: playerProgress?.currentLayer ?? 0,
    maxLayer: Number(raw?.maxLayer ?? raw?.MaxLayer ?? layers.length) || layers.length,
    layers
  }
}

function toggleSutraExpand(sutraId) {
  if (expandedSutras.has(sutraId)) {
    expandedSutras.delete(sutraId)
  } else {
    expandedSutras.add(sutraId)
  }
}

async function loadSutras() {
  if (!playerHasGuild.value || !joinedSectId.value) return
  isLoading.value = true
  try {
    // 从宗门详情获取所有可修心法
    const detail = await apiClient.getSectDetail(joinedSectId.value)
    const heartSutras = detail?.heartSutras ?? []
    // 获取玩家心法进度
    const playerSutras = await apiClient.getSectSutras()
    const progressMap = {}
    if (Array.isArray(playerSutras)) {
      playerSutras.forEach(ps => { progressMap[ps.sutraId] = ps })
    }
    sutras.value = heartSutras.map(raw => normalizeSutra(raw, progressMap[raw.sutraId ?? raw.SutraId]))
    statusMessage.value = ''
  } catch (error) {
    statusMessage.value = error.message || '加载心法数据失败。'
  } finally {
    isLoading.value = false
  }
}

async function upgradeSutra(sutraId) {
  if (!sutraId || isSubmitting.value) return
  isSubmitting.value = true
  try {
    const result = await apiClient.upgradeSutra(sutraId)
    statusMessage.value = result?.message || result?.Message || '心法修炼成功。'
    await gameStore.refreshPlayerSnapshot({ includeRankings: false })
    await loadSutras()
  } catch (error) {
    statusMessage.value = error.message || '心法修炼失败。'
  } finally {
    isSubmitting.value = false
  }
}

// ==================== Tab 3: 捐献 ====================
const donationStatus = reactive({
  hasDonated: false,
  canDonate: true,
  contributionReward: 0
})
const donationGold = ref(1000)
const donationAmounts = [1000, 5000, 10000, 50000]

async function loadDonationStatus() {
  isLoading.value = true
  try {
    const result = await apiClient.getDonationStatus()
    donationStatus.hasDonated = Boolean(result?.alreadyDonatedToday ?? result?.AlreadyDonatedToday ?? false)
    donationStatus.canDonate = Boolean(result?.canDonate ?? result?.CanDonate ?? true)
    donationStatus.contributionReward = Number(result?.contributionReward ?? result?.ContributionReward ?? 0) || 0
    statusMessage.value = ''
  } catch (error) {
    statusMessage.value = error.message || '加载捐献状态失败。'
  } finally {
    isLoading.value = false
  }
}

async function donateSect() {
  if (donationGold.value <= 0 || isSubmitting.value) return
  isSubmitting.value = true
  try {
    const result = await apiClient.donateSect(donationGold.value)
    statusMessage.value = result?.message || result?.Message || '捐献成功。'
    await gameStore.refreshPlayerSnapshot({ includeRankings: false })
    await loadDonationStatus()
  } catch (error) {
    statusMessage.value = error.message || '捐献失败。'
  } finally {
    isSubmitting.value = false
  }
}

// ==================== Tab 4: 宗门任务 ====================
const sectTasks = ref([])

function normalizeSectTask(raw) {
  const currentProgress = Number(raw?.CurrentProgress ?? raw?.currentProgress ?? 0) || 0
  const targetProgress = Math.max(1, Number(raw?.TargetProgress ?? raw?.targetProgress ?? 1))
  const progressPercent = Math.min(100, Math.max(0, Math.round(currentProgress * 100 / targetProgress)))
  const isCompleted = Boolean(raw?.IsCompleted ?? raw?.isCompleted ?? currentProgress >= targetProgress)
  const rewardClaimed = Boolean(raw?.RewardClaimed ?? raw?.rewardClaimed ?? false)

  let statusText = '进行中'
  let statusClass = 'info'
  if (rewardClaimed) {
    statusText = '已领取'
    statusClass = 'muted'
  } else if (isCompleted) {
    statusText = '可领取'
    statusClass = 'success'
  }

  return {
    questId: raw?.QuestId ?? raw?.questId ?? raw?.TaskId ?? raw?.taskId ?? '',
    name: raw?.Name ?? raw?.name ?? raw?.TaskName ?? raw?.taskName ?? '未命名任务',
    icon: raw?.Icon || raw?.icon || ICON.misc_scroll,
    description: raw?.Description ?? raw?.description ?? '',
    typeText: raw?.TypeText ?? raw?.typeText ?? '日常',
    currentProgress,
    targetProgress,
    progressPercent,
    isCompleted,
    rewardClaimed,
    statusText,
    statusClass,
    contributionReward: Number(raw?.ContributionReward ?? raw?.contributionReward ?? 0) || 0,
    goldReward: Number(raw?.GoldReward ?? raw?.goldReward ?? 0) || 0
  }
}

async function loadSectTasks() {
  isLoading.value = true
  try {
    const result = await apiClient.getSectTasks()
    sectTasks.value = Array.isArray(result) ? result.map(normalizeSectTask) : []
    statusMessage.value = ''
  } catch (error) {
    statusMessage.value = error.message || '加载宗门任务失败。'
  } finally {
    isLoading.value = false
  }
}

async function submitSectTask(questId) {
  if (!questId || isSubmitting.value) return
  isSubmitting.value = true
  try {
    const result = await apiClient.submitQuest(questId)
    statusMessage.value = result?.message || result?.Message || '任务奖励已领取。'
    await gameStore.refreshPlayerSnapshot({ includeRankings: false })
    await loadSectTasks()
  } catch (error) {
    statusMessage.value = error.message || '提交任务失败。'
  } finally {
    isSubmitting.value = false
  }
}

// ==================== Tab 5: 弟子列表 ====================
const disciples = ref([])
const sparResult = ref(null)

function normalizeDisciple(raw) {
  const winCount = Number(raw?.WinBattles ?? raw?.winBattles ?? 0) || 0
  const totalBattles = Number(raw?.TotalBattles ?? raw?.totalBattles ?? 0) || 0
  const winRate = totalBattles > 0 ? Math.round(winCount * 100 / totalBattles) : 0

  return {
    playerId: raw?.PlayerId ?? raw?.playerId ?? raw?.Id ?? raw?.id ?? '',
    name: raw?.PlayerName ?? raw?.playerName ?? '未知弟子',
    level: Number(raw?.PlayerLevel ?? raw?.playerLevel ?? 1) || 1,
    element: (raw?.Element ?? raw?.element ?? 'none').toLowerCase(),
    elementName: raw?.ElementName ?? raw?.elementName ?? '-',
    professionName: raw?.ProfessionName ?? raw?.professionName ?? '-',
    contribution: Number(raw?.Contribution ?? raw?.contribution ?? 0) || 0,
    winRate,
    isSelf: Boolean(raw?.IsSelf ?? raw?.isSelf)
  }
}

function normalizeSparResult(raw) {
  return {
    attackerName: raw?.AttackerName ?? raw?.attackerName ?? '',
    defenderName: raw?.DefenderName ?? raw?.defenderName ?? '',
    isWin: Boolean(raw?.IsWin ?? raw?.isWin),
    totalRounds: Number(raw?.TotalRounds ?? raw?.totalRounds ?? 0) || 0,
    log: raw?.Log ?? raw?.log ?? raw?.Summary ?? raw?.summary ?? ''
  }
}

async function loadDisciples() {
  isLoading.value = true
  try {
    const result = await apiClient.getSectDisciples()
    disciples.value = Array.isArray(result) ? result.map(normalizeDisciple) : []
    statusMessage.value = ''
  } catch (error) {
    statusMessage.value = error.message || '加载弟子列表失败。'
  } finally {
    isLoading.value = false
  }
}

async function startSpar(playerId) {
  if (!playerId || isSubmitting.value) return
  isSubmitting.value = true
  try {
    const result = await apiClient.sparWithDisciple(playerId)
    sparResult.value = normalizeSparResult(result)
    statusMessage.value = result?.isWin ?? result?.IsWin ? '切磋胜利！' : '切磋失败。'
    await loadDisciples()
  } catch (error) {
    statusMessage.value = error.message || '切磋失败。'
  } finally {
    isSubmitting.value = false
  }
}

// ==================== Tab 6: 宗门大比 ====================
const tournament = ref(null)
const tournamentMatches = ref([])

function normalizeTournament(raw) {
  if (!raw) return null
  const state = (raw?.State ?? raw?.state ?? 'unknown').toLowerCase()
  const stateTextMap = { upcoming: '即将开始', active: '进行中', settled: '已结算', unknown: '准备中', pending: '准备中', idle: '准备中' }

  return {
    tournamentId: raw?.TournamentId ?? raw?.tournamentId ?? raw?.Id ?? raw?.id ?? '',
    state,
    stateText: stateTextMap[state] || '准备中',
    currentRound: Number(raw?.CurrentRound ?? raw?.currentRound ?? 0) || 0,
    countdownText: raw?.CountdownText ?? raw?.countdownText ?? '',
    hasReward: Boolean(raw?.HasReward ?? raw?.hasReward)
  }
}

function normalizeMatchRound(raw) {
  return {
    roundNumber: Number(raw?.RoundNumber ?? raw?.roundNumber ?? 0) || 0,
    matches: Array.isArray(raw?.Matches ?? raw?.matches)
      ? (raw?.Matches ?? raw?.matches).map((m) => ({
        matchId: m?.MatchId ?? m?.matchId ?? '',
        player1Id: m?.Player1Id ?? m?.player1Id ?? '',
        player1Name: m?.Player1Name ?? m?.player1Name ?? '待定',
        player1Score: m?.Player1Score ?? m?.player1Score,
        player2Id: m?.Player2Id ?? m?.player2Id ?? '',
        player2Name: m?.Player2Name ?? m?.player2Name ?? '待定',
        player2Score: m?.Player2Score ?? m?.player2Score,
        winnerId: m?.WinnerId ?? m?.winnerId ?? ''
      }))
      : []
  }
}

async function loadSectTournament() {
  isLoading.value = true
  try {
    const result = await apiClient.getSectTournament()
    tournament.value = normalizeTournament(result)
    if (tournament.value?.tournamentId) {
      const matches = await apiClient.getTournamentMatches(tournament.value.tournamentId)
      tournamentMatches.value = Array.isArray(matches) ? matches.map(normalizeMatchRound) : []
    } else {
      tournamentMatches.value = []
    }
    statusMessage.value = ''
  } catch (error) {
    statusMessage.value = error.message || '加载宗门大比数据失败。'
  } finally {
    isLoading.value = false
  }
}

async function claimTournamentReward() {
  if (!tournament.value?.tournamentId || isSubmitting.value) return
  isSubmitting.value = true
  try {
    const result = await apiClient.claimTournamentReward(tournament.value.tournamentId)
    statusMessage.value = result?.message || result?.Message || '奖励领取成功。'
    await gameStore.refreshPlayerSnapshot({ includeRankings: false })
    await loadSectTournament()
  } catch (error) {
    statusMessage.value = error.message || '领取奖励失败。'
  } finally {
    isSubmitting.value = false
  }
}

// ==================== Tab 7: 天骄赛 ====================
const geniusTournament = ref(null)
const geniusMatches = ref([])

async function loadGeniusTournament() {
  isLoading.value = true
  try {
    const result = await apiClient.getGeniusTournament()
    geniusTournament.value = normalizeTournament(result)
    if (geniusTournament.value?.tournamentId) {
      const matches = await apiClient.getGeniusMatches(geniusTournament.value.tournamentId)
      geniusMatches.value = Array.isArray(matches) ? matches.map((raw) => {
        const round = normalizeMatchRound(raw)
        // 天骄赛额外显示宗门名
        round.matches = round.matches.map((m) => ({
          ...m,
          player1SectName: raw?.Matches?.find?.((rm) => (rm?.MatchId ?? rm?.matchId) === m.matchId)?.Player1SectName
            ?? raw?.matches?.find?.((rm) => (rm?.MatchId ?? rm?.matchId) === m.matchId)?.player1SectName ?? '',
          player2SectName: raw?.Matches?.find?.((rm) => (rm?.MatchId ?? rm?.matchId) === m.matchId)?.Player2SectName
            ?? raw?.matches?.find?.((rm) => (rm?.MatchId ?? rm?.matchId) === m.matchId)?.player2SectName ?? ''
        }))
        return round
      }) : []
    } else {
      geniusMatches.value = []
    }
    statusMessage.value = ''
  } catch (error) {
    statusMessage.value = error.message || '加载天骄赛数据失败。'
  } finally {
    isLoading.value = false
  }
}

async function claimGeniusReward() {
  if (!geniusTournament.value?.tournamentId || isSubmitting.value) return
  isSubmitting.value = true
  try {
    const result = await apiClient.claimGeniusReward(geniusTournament.value.tournamentId)
    statusMessage.value = result?.message || result?.Message || '奖励领取成功。'
    await gameStore.refreshPlayerSnapshot({ includeRankings: false })
    await loadGeniusTournament()
  } catch (error) {
    statusMessage.value = error.message || '领取奖励失败。'
  } finally {
    isSubmitting.value = false
  }
}

// ==================== Tab 8: 宗门Boss ====================
const bossStatus = reactive({
  hasActiveBoss: false,
  hasPendingReward: false,
  bossName: '',
  state: '',
  currentHp: 0,
  maxHp: 0,
  isJoined: false,
  playerName: '',
  myDamage: 0,
  myRank: 0,
  canAct: false,
  secondsToReady: 0,
  isDead: false,
  secondsToRevive: 0
})
const bossRanking = ref([])
const bossSkills = ref([])

const bossHpPercent = computed(() => {
  if (!bossStatus.maxHp) return 0
  return Math.max(0, Math.min(100, Math.round(bossStatus.currentHp * 100 / bossStatus.maxHp)))
})

const bossActionDisabled = computed(() => {
  return isSubmitting.value || !bossStatus.isJoined || !bossStatus.canAct || bossStatus.isDead
})

function assignBossStatus(payload) {
  bossStatus.hasActiveBoss = Boolean(payload?.hasActiveBoss ?? payload?.HasActiveBoss)
  bossStatus.hasPendingReward = Boolean(payload?.hasPendingReward ?? payload?.HasPendingReward)
  bossStatus.bossName = payload?.bossName ?? payload?.BossName ?? payload?.instance?.bossName ?? payload?.instance?.BossName ?? ''
  bossStatus.state = payload?.state ?? payload?.State ?? payload?.instance?.state ?? payload?.instance?.State ?? ''
  bossStatus.currentHp = Number(payload?.currentHp ?? payload?.CurrentHp ?? payload?.boss?.currentHp ?? payload?.boss?.CurrentHp ?? 0) || 0
  bossStatus.maxHp = Number(payload?.maxHp ?? payload?.MaxHp ?? payload?.boss?.maxHp ?? payload?.boss?.MaxHp ?? 0) || 0
  bossStatus.isJoined = Boolean(payload?.self?.isJoined ?? payload?.self?.IsJoined ?? payload?.IsJoined ?? payload?.isJoined)
  bossStatus.playerName = payload?.self?.playerName ?? payload?.self?.PlayerName ?? payload?.playerName ?? payload?.PlayerName ?? ''
  bossStatus.myDamage = Number(payload?.self?.totalDamage ?? payload?.self?.TotalDamage ?? payload?.myDamage ?? payload?.MyDamage ?? 0) || 0
  bossStatus.myRank = Number(payload?.self?.rank ?? payload?.self?.Rank ?? payload?.myRank ?? payload?.MyRank ?? 0) || 0
  bossStatus.canAct = Boolean(payload?.self?.canAct ?? payload?.self?.CanAct)
  bossStatus.secondsToReady = Number(payload?.self?.secondsToReady ?? payload?.self?.SecondsToReady ?? 0) || 0
  bossStatus.isDead = Boolean(payload?.self?.isDead ?? payload?.self?.IsDead)
  bossStatus.secondsToRevive = Number(payload?.self?.secondsToRevive ?? payload?.self?.SecondsToRevive ?? 0) || 0

  const skills = payload?.self?.skills ?? payload?.self?.Skills ?? payload?.skills ?? []
  bossSkills.value = Array.isArray(skills) ? skills.map((s) => ({
    skillId: s?.SkillId ?? s?.skillId ?? '',
    name: s?.Name ?? s?.name ?? '',
    icon: s?.Icon || s?.icon || ICON.misc_fire,
    canUse: Boolean(s?.CanUse ?? s?.canUse ?? true),
    currentCooldown: Number(s?.CurrentCooldown ?? s?.currentCooldown ?? 0) || 0,
    cooldown: Number(s?.Cooldown ?? s?.cooldown ?? 0) || 0
  })) : []
}

async function loadSectBoss() {
  isLoading.value = true
  try {
    const result = await apiClient.getSectBossStatus()
    assignBossStatus(result || {})
    bossRanking.value = Array.isArray(result?.ranking ?? result?.Ranking)
      ? (result?.ranking ?? result?.Ranking).map((r, index) => ({
        playerId: r?.PlayerId ?? r?.playerId ?? '',
        playerName: r?.PlayerName ?? r?.playerName ?? '',
        totalDamage: Number(r?.TotalDamage ?? r?.totalDamage ?? 0) || 0,
        rank: Number(r?.Rank ?? r?.rank ?? index + 1) || index + 1,
        isSelf: Boolean(r?.IsSelf ?? r?.isSelf)
      }))
      : []
    statusMessage.value = ''
  } catch (error) {
    statusMessage.value = error.message || '加载宗门Boss数据失败。'
  } finally {
    isLoading.value = false
  }
}

async function joinSectBoss() {
  if (isSubmitting.value) return
  isSubmitting.value = true
  try {
    const result = await apiClient.attackSectBoss()
    assignBossStatus(result || {})
    await loadSectBoss()
    statusMessage.value = '已参战。'
  } catch (error) {
    statusMessage.value = error.message || '参战失败。'
  } finally {
    isSubmitting.value = false
  }
}

async function bossAction(actionType, skillId) {
  if (isSubmitting.value) return
  isSubmitting.value = true
  try {
    await apiClient.attackSectBoss()
    await loadSectBoss()
  } catch (error) {
    statusMessage.value = error.message || '行动失败。'
  } finally {
    isSubmitting.value = false
  }
}

async function claimSectBossReward() {
  if (isSubmitting.value) return
  isSubmitting.value = true
  try {
    const result = await apiClient.claimSectBossReward()
    statusMessage.value = result?.message || result?.Message || '奖励领取成功。'
    await gameStore.refreshPlayerSnapshot({ includeRankings: false })
    await loadSectBoss()
  } catch (error) {
    statusMessage.value = error.message || '领取奖励失败。'
  } finally {
    isSubmitting.value = false
  }
}

// ==================== Tab 9: 宗门商店 ====================
const shopItems = ref([])

function normalizeShopItem(raw) {
  return {
    itemId: raw?.ItemId ?? raw?.itemId ?? raw?.Id ?? raw?.id ?? '',
    name: raw?.Name ?? raw?.name ?? '未知物品',
    icon: raw?.Icon || raw?.icon || ICON.misc_gift,
    description: raw?.Description ?? raw?.description ?? '暂无描述',
    quality: (raw?.QualityName ?? raw?.qualityName ?? raw?.Quality ?? raw?.quality ?? 'common').toLowerCase(),
    stock: Number(raw?.Stock ?? raw?.stock ?? 0) || 0,
    contributionCost: Number(raw?.ContributionCost ?? raw?.contributionCost ?? raw?.Price ?? raw?.price ?? 0) || 0
  }
}

async function loadSectShop() {
  isLoading.value = true
  try {
    const result = await apiClient.getSectShopItems()
    shopItems.value = Array.isArray(result) ? result.map(normalizeShopItem) : []
    statusMessage.value = ''
  } catch (error) {
    statusMessage.value = error.message || '加载宗门商店失败。'
  } finally {
    isLoading.value = false
  }
}

async function purchaseSectItem(itemId) {
  if (!itemId || isSubmitting.value) return
  isSubmitting.value = true
  try {
    const result = await apiClient.purchaseSectItem(itemId)
    statusMessage.value = result?.message || result?.Message || '购买成功。'
    await gameStore.refreshPlayerSnapshot({ includeRankings: false })
    await loadSectShop()
  } catch (error) {
    statusMessage.value = error.message || '购买失败。'
  } finally {
    isSubmitting.value = false
  }
}

// ==================== 通用工具 ====================
function formatNum(num) {
  const value = Number(num) || 0
  const abs = Math.abs(value)
  if (abs >= 100000000) return `${(value / 100000000).toFixed(2)}亿`
  if (abs >= 10000) return `${(value / 10000).toFixed(2)}万`
  return value.toLocaleString('zh-CN')
}

// ==================== 标签页懒加载 ====================
const tabLoaders = {
  overview: loadOverviewTab,
  sutras: loadSutras,
  donation: loadDonationStatus,
  tasks: loadSectTasks,
  disciples: loadDisciples,
  tournament: loadSectTournament,
  genius: loadGeniusTournament,
  boss: loadSectBoss,
  shop: loadSectShop
}

watch(activeTab, async (tab) => {
  if (loadedTabs[tab]) return
  const loader = tabLoaders[tab]
  if (loader) {
    await loader()
    loadedTabs[tab] = true
  }
})

// 弹窗打开时重置状态并加载默认标签
watch(() => props.modelValue, async (visible) => {
  if (!visible) return
  statusMessage.value = ''
  selectedSect.value = null
  sparResult.value = null
  activeTab.value = 'overview'

  // 重置加载标记，确保每次打开都拿最新数据
  Object.keys(loadedTabs).forEach((key) => {
    loadedTabs[key] = false
  })

  await loadOverviewTab()
  loadedTabs.overview = true
})
</script>

<style scoped>
.sect-modal {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
  color: var(--text-primary);
}

/* 标签页导航 */
.sect-tabs {
  display: flex;
  gap: var(--spacing-xs);
  flex-wrap: wrap;
  border-bottom: 1px solid var(--border-color);
  padding-bottom: var(--spacing-sm);
}

.tab-btn {
  display: inline-flex;
  align-items: center;
  gap: var(--spacing-xs);
  padding: var(--spacing-sm) var(--spacing-md);
  background: transparent;
  border: 1px solid var(--border-color);
  border-radius: var(--radius-sm);
  color: var(--text-secondary);
  cursor: pointer;
  transition: all 0.25s ease;
  font-family: var(--font-primary);
  font-size: var(--font-size-sm);
}

.tab-btn:hover,
.tab-btn.active {
  color: var(--text-primary);
  background: var(--accent-soft-bg);
  border-color: var(--accent-text);
  box-shadow: var(--shadow-sm);
}

.tab-btn.disabled {
  opacity: 0.4;
  cursor: not-allowed;
  pointer-events: none;
}

.tab-icon {
  font-size: 14px;
}

/* 标签页内容 */
.tab-content {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
  min-height: 400px;
  max-height: 560px;
  overflow-y: auto;
}

.tab-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--spacing-sm);
}

.tab-header-title {
  color: var(--highlight-text);
  font-weight: 600;
  font-size: var(--font-size-lg);
}

.tab-header-actions {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
}

/* 通用按钮 */
.refresh-btn,
.primary-btn,
.secondary-btn,
.danger-btn,
.action-btn,
.reward-btn,
.buy-btn {
  border: 1px solid transparent;
  border-radius: var(--radius-sm);
  cursor: pointer;
  transition: all 0.25s ease;
  font-family: var(--font-primary);
  padding: var(--spacing-sm) var(--spacing-md);
}

.refresh-btn {
  background: var(--button-neutral-bg);
  border-color: var(--button-neutral-border);
  color: var(--button-neutral-text);
}

.refresh-btn:hover:not(:disabled) {
  background: var(--button-neutral-hover-bg);
  color: var(--button-neutral-hover-text);
  border-color: var(--button-outline-hover-border);
}

.primary-btn {
  background: linear-gradient(135deg, var(--button-primary-start), var(--button-primary-end));
  color: var(--button-primary-text);
}

.primary-btn:hover:not(:disabled) {
  background: linear-gradient(135deg, var(--button-primary-hover-start), var(--button-primary-hover-end));
  color: var(--button-primary-hover-text);
  box-shadow: var(--shadow-sm);
}

.primary-btn.sm {
  padding: var(--spacing-xs) var(--spacing-md);
  font-size: var(--font-size-sm);
}

.secondary-btn {
  background: var(--button-neutral-bg);
  border-color: var(--button-neutral-border);
  color: var(--button-neutral-text);
}

.reward-btn {
  background: oklch(0.65 0.12 80 / 0.12);
  border-color: oklch(0.65 0.12 80 / 0.25);
  color: var(--highlight-text);
}

.action-btn {
  background: var(--button-info-start);
  border-color: oklch(0.6 0.12 250 / 0.25);
  color: var(--accent-text);
  font-size: var(--font-size-sm);
  padding: var(--spacing-xs) var(--spacing-sm);
}

.action-btn:hover:not(:disabled) {
  background: oklch(0.6 0.12 250 / 0.2);
  color: var(--text-primary);
}

.refresh-btn:disabled,
.primary-btn:disabled,
.secondary-btn:disabled,
.action-btn:disabled,
.reward-btn:disabled,
.buy-btn:disabled {
  opacity: 0.55;
  cursor: not-allowed;
}

.close-btn {
  background: transparent;
  border: none;
  color: var(--text-secondary);
  cursor: pointer;
  font-size: 18px;
  padding: 0 4px;
}

.close-btn:hover {
  color: var(--text-primary);
}

/* 状态消息 */
.status-message {
  padding: var(--spacing-sm) var(--spacing-md);
  background: rgba(124, 58, 237, 0.12);
  border: 1px solid rgba(124, 58, 237, 0.28);
  border-radius: var(--radius-sm);
  color: var(--text-primary);
  font-size: var(--font-size-sm);
}

/* 空状态 */
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: var(--spacing-sm);
  color: var(--text-secondary);
  min-height: 200px;
}

.empty-icon {
  font-size: 30px;
}

/* ==================== Tab 1: 宗门总览 ==================== */
.sect-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: var(--spacing-md);
}

.sect-card {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--spacing-sm);
  padding: var(--spacing-lg);
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  cursor: pointer;
  transition: all 0.25s ease;
  text-align: center;
  color: inherit;
}

.sect-card:hover {
  border-color: var(--accent-text);
  background: rgba(124, 58, 237, 0.08);
  transform: translateY(-2px);
}

.sect-card-icon {
  font-size: 36px;
}

.sect-card-name {
  font-weight: 600;
  color: var(--text-primary);
  font-size: var(--font-size-md);
}

.sect-card-desc {
  color: var(--text-secondary);
  font-size: var(--font-size-sm);
  line-height: 1.4;
}

.sect-card-meta {
  color: var(--text-muted);
  font-size: var(--font-size-sm);
}

.sect-detail-card {
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  padding: var(--spacing-lg);
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}

.sect-detail-header {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
}

.sect-detail-icon {
  font-size: 40px;
}

.sect-detail-name {
  font-size: var(--font-size-lg);
  font-weight: 600;
  color: var(--highlight-text);
}

.sect-detail-meta {
  color: var(--text-secondary);
  font-size: var(--font-size-sm);
}

.sect-detail-desc {
  color: var(--text-primary);
  line-height: 1.6;
  background: rgba(0, 0, 0, 0.16);
  border-radius: var(--radius-md);
  padding: var(--spacing-md);
}

.sect-sutra-preview {
  background: rgba(0, 0, 0, 0.12);
  border-radius: var(--radius-md);
  padding: var(--spacing-md);
}

.sect-sutra-preview-title {
  font-size: 14px;
  font-weight: 600;
  color: var(--accent-primary);
  margin-bottom: 8px;
}

.sect-sutra-preview-item {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 8px;
  padding: 6px 0;
  border-bottom: 1px solid rgba(255, 255, 255, 0.06);
}

.sect-sutra-preview-item:last-child {
  border-bottom: none;
}

.sect-sutra-preview-name {
  font-weight: 500;
  color: var(--text-primary);
  min-width: 80px;
}

.sect-sutra-preview-layers {
  font-size: 12px;
  color: var(--text-muted);
}

.sect-sutra-preview-bonuses {
  display: flex;
  gap: 6px;
  flex-wrap: wrap;
}

.bonus-tag {
  font-size: 11px;
  padding: 2px 6px;
  border-radius: 4px;
  background: rgba(76, 175, 80, 0.15);
  color: #81c784;
}

.sect-detail-actions {
  display: flex;
  gap: var(--spacing-sm);
  justify-content: flex-end;
}

.sect-overview {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}

.sect-overview-card {
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  padding: var(--spacing-lg);
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
}

.sect-overview-row {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
}

.sect-overview-name {
  font-size: var(--font-size-lg);
  font-weight: 600;
  color: var(--highlight-text);
}

.guild-level {
  padding: 4px 10px;
  border-radius: 999px;
  background: var(--highlight-soft-bg);
  color: var(--highlight-text);
  font-size: var(--font-size-sm);
}

.sect-overview-meta {
  display: flex;
  gap: var(--spacing-md);
  color: var(--text-secondary);
  font-size: var(--font-size-sm);
}

.sect-overview-announcement {
  padding: var(--spacing-sm) var(--spacing-md);
  background: rgba(215, 180, 92, 0.08);
  border: 1px solid rgba(215, 180, 92, 0.2);
  border-radius: var(--radius-sm);
  color: var(--highlight-text);
  font-size: var(--font-size-sm);
}

.sect-overview-desc {
  color: var(--text-secondary);
  line-height: 1.6;
}

.sect-overview-stats {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: var(--spacing-sm);
}

.stat-card {
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  padding: var(--spacing-md);
  display: flex;
  flex-direction: column;
  gap: 2px;
  text-align: center;
}

.stat-label {
  color: var(--text-secondary);
  font-size: var(--font-size-sm);
}

.stat-value {
  color: var(--highlight-text);
  font-size: var(--font-size-lg);
}

/* ==================== Tab 2: 心法 ==================== */
.sutra-list {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}

.sutra-card {
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  overflow: hidden;
}

.sutra-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--spacing-md);
  cursor: pointer;
  transition: background 0.2s ease;
}

.sutra-header:hover {
  background: rgba(255, 255, 255, 0.03);
}

.sutra-info {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
}

.sutra-icon {
  font-size: 24px;
}

.sutra-name {
  font-weight: 600;
  color: var(--text-primary);
}

.sutra-desc {
  color: var(--text-secondary);
  font-size: var(--font-size-sm);
}

.sutra-progress-badge {
  padding: 4px 10px;
  border-radius: 999px;
  background: var(--highlight-soft-bg);
  color: var(--highlight-text);
  font-size: var(--font-size-sm);
  white-space: nowrap;
}

.sutra-layers {
  border-top: 1px solid var(--border-color);
  padding: var(--spacing-sm);
  display: flex;
  flex-direction: column;
  gap: var(--spacing-xs);
}

.sutra-layer {
  padding: var(--spacing-sm) var(--spacing-md);
  border-radius: var(--radius-sm);
  background: rgba(255, 255, 255, 0.02);
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: var(--spacing-sm);
}

.sutra-layer.current {
  background: rgba(124, 58, 237, 0.1);
  border: 1px solid rgba(124, 58, 237, 0.28);
}

.sutra-layer.locked {
  opacity: 0.5;
}

.sutra-layer.available {
  border: 1px solid rgba(215, 180, 92, 0.2);
}

.layer-header {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  min-width: 200px;
}

.layer-number {
  color: var(--text-muted);
  font-size: var(--font-size-sm);
  min-width: 50px;
}

.layer-name {
  color: var(--text-primary);
  font-weight: 500;
}

.layer-status {
  padding: 2px 8px;
  border-radius: 999px;
  font-size: var(--font-size-xs);
}

.layer-status.current {
  background: rgba(124, 58, 237, 0.2);
  color: var(--accent-text);
}

.layer-status.learned {
  background: rgba(76, 175, 80, 0.15);
  color: var(--status-success-text);
}

.layer-status.locked {
  background: rgba(255, 255, 255, 0.06);
  color: var(--text-muted);
}

.layer-bonuses {
  display: flex;
  gap: var(--spacing-sm);
  flex-wrap: wrap;
}

.layer-bonus {
  padding: 2px 8px;
  background: rgba(255, 255, 255, 0.04);
  border-radius: var(--radius-sm);
  font-size: var(--font-size-xs);
  color: var(--text-secondary);
}

.layer-unlock {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: var(--font-size-xs);
  color: var(--highlight-text);
}

.unlock-icon {
  font-size: 12px;
}

/* ==================== Tab 3: 捐献 ==================== */
.donation-panel {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-lg);
}

.donation-balances {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: var(--spacing-sm);
}

.donation-balance-card {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  padding: var(--spacing-md);
}

.donation-balance-icon {
  font-size: 24px;
}

.donation-balance-info {
  display: flex;
  flex-direction: column;
}

.donation-balance-label {
  color: var(--text-muted);
  font-size: var(--font-size-xs);
}

.donation-balance-value {
  color: var(--highlight-text);
  font-weight: 600;
  font-size: var(--font-size-lg);
  font-family: var(--font-mono);
}

.donation-done {
  text-align: center;
  padding: var(--spacing-lg);
  background: rgba(76, 175, 80, 0.08);
  border: 1px solid rgba(76, 175, 80, 0.2);
  border-radius: var(--radius-md);
}

.donation-done-icon {
  font-size: 36px;
}

.donation-done-text {
  color: var(--status-success-text);
  font-weight: 600;
  margin-top: var(--spacing-sm);
}

.donation-reward-info {
  color: var(--text-secondary);
  font-size: var(--font-size-sm);
  margin-top: var(--spacing-xs);
}

.donation-form {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}

.donation-amounts {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: var(--spacing-sm);
}

.donation-amount-btn {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 2px;
  padding: var(--spacing-md);
  background: var(--bg-overlay-light);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  color: var(--text-secondary);
  cursor: pointer;
  transition: all 0.25s ease;
  font-family: var(--font-primary);
}

.donation-amount-btn:hover:not(:disabled) {
  border-color: var(--accent-text);
  color: var(--text-primary);
}

.donation-amount-btn.active {
  border-color: var(--accent-text);
  background: var(--accent-soft-bg);
  color: var(--accent-text);
}

.donation-amount-btn--premium .amount-reward {
  color: var(--highlight-warn, #f5a623);
}

.donation-amount-btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.amount-icon {
  font-size: 16px;
}

.amount-value {
  font-weight: 600;
}

.amount-reward {
  font-size: var(--font-size-xs);
  color: var(--text-muted);
}

.donation-tip {
  text-align: center;
  font-size: var(--font-size-xs);
  color: var(--text-muted);
}

.donation-submit-btn {
  margin-top: var(--spacing-xs);
}

/* ==================== Tab 4: 宗门任务 ==================== */
.task-list {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}

.task-card {
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  padding: var(--spacing-md);
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
}

.task-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: var(--spacing-sm);
}

.task-info {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
}

.task-icon {
  font-size: 24px;
}

.task-name {
  font-weight: 600;
  color: var(--text-primary);
}

.task-type {
  color: var(--text-secondary);
  font-size: var(--font-size-sm);
}

.task-desc {
  color: var(--text-secondary);
  font-size: var(--font-size-sm);
  line-height: 1.4;
}

.progress-block {
  margin-top: 2px;
}

.progress-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--spacing-sm);
  font-size: var(--font-size-sm);
  color: var(--text-secondary);
  margin-bottom: 6px;
}

.progress-track {
  width: 100%;
  height: 8px;
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.08);
  overflow: hidden;
}

.progress-fill {
  display: block;
  height: 100%;
  border-radius: inherit;
  background: linear-gradient(90deg, rgba(124, 58, 237, 0.78), rgba(227, 179, 92, 0.92));
  box-shadow: 0 0 12px rgba(124, 58, 237, 0.28);
  transition: width 0.3s ease;
}

.task-rewards {
  display: flex;
  gap: var(--spacing-sm);
  flex-wrap: wrap;
}

.reward-tag {
  padding: 2px 8px;
  background: rgba(215, 180, 92, 0.08);
  border-radius: var(--radius-sm);
  font-size: var(--font-size-xs);
  color: var(--highlight-text);
}

.task-actions {
  display: flex;
  justify-content: flex-end;
}

.status-badge {
  display: inline-flex;
  align-items: center;
  padding: 4px 10px;
  border-radius: 999px;
  font-size: var(--font-size-sm);
  white-space: nowrap;
}

.status-badge.neutral {
  background: rgba(255, 255, 255, 0.08);
  color: var(--text-secondary);
}

.status-badge.info {
  background: var(--status-info-bg);
  color: var(--status-info-text);
}

.status-badge.success {
  background: var(--status-success-bg);
  color: var(--status-success-text);
}

.status-badge.muted {
  background: rgba(255, 255, 255, 0.05);
  color: var(--text-muted);
}

/* ==================== Tab 5: 弟子列表 ==================== */
.disciple-table {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.disciple-table-header {
  display: grid;
  grid-template-columns: 1.2fr 0.6fr 0.8fr 0.6fr 0.8fr 0.6fr 0.8fr;
  gap: var(--spacing-xs);
  padding: var(--spacing-sm) var(--spacing-md);
  background: rgba(255, 255, 255, 0.04);
  border-radius: var(--radius-sm);
  font-size: var(--font-size-sm);
  color: var(--text-muted);
  font-weight: 600;
}

.disciple-row {
  display: grid;
  grid-template-columns: 1.2fr 0.6fr 0.8fr 0.6fr 0.8fr 0.6fr 0.8fr;
  gap: var(--spacing-xs);
  padding: var(--spacing-sm) var(--spacing-md);
  background: rgba(255, 255, 255, 0.02);
  border-radius: var(--radius-sm);
  align-items: center;
  font-size: var(--font-size-sm);
}

.disciple-row:hover {
  background: rgba(255, 255, 255, 0.04);
}

.col-name {
  font-weight: 500;
  color: var(--text-primary);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.col-level,
.col-profession,
.col-contribution,
.col-winrate {
  color: var(--text-secondary);
  font-family: var(--font-mono);
}

.element-badge {
  padding: 2px 6px;
  border-radius: var(--radius-sm);
  font-size: var(--font-size-xs);
  color: var(--text-primary);
}

.element-badge.metal { background: rgba(192, 192, 192, 0.2); }
.element-badge.wood { background: rgba(34, 139, 34, 0.2); }
.element-badge.water { background: rgba(30, 144, 255, 0.2); }
.element-badge.fire { background: rgba(255, 69, 0, 0.2); }
.element-badge.earth { background: rgba(210, 105, 30, 0.2); }
.element-badge.wind { background: rgba(32, 178, 170, 0.2); }
.element-badge.ice { background: rgba(135, 206, 235, 0.2); }
.element-badge.thunder { background: rgba(147, 112, 219, 0.2); }

.self-tag {
  color: var(--text-muted);
  font-size: var(--font-size-xs);
}

.spar-result {
  margin-top: var(--spacing-md);
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  overflow: hidden;
}

.spar-result-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--spacing-sm) var(--spacing-md);
  background: rgba(255, 255, 255, 0.04);
  color: var(--highlight-text);
  font-weight: 600;
}

.spar-result-body {
  padding: var(--spacing-md);
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
  align-items: center;
}

.spar-vs {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
}

.spar-player {
  font-weight: 600;
  color: var(--text-primary);
}

.spar-vs-text {
  color: var(--highlight-text);
  font-weight: 700;
}

.spar-outcome {
  font-size: var(--font-size-lg);
  font-weight: 700;
  color: var(--status-danger-text);
}

.spar-outcome.win {
  color: var(--status-success-text);
}

.spar-rounds {
  color: var(--text-secondary);
  font-size: var(--font-size-sm);
}

.spar-log {
  color: var(--text-secondary);
  font-size: var(--font-size-sm);
  line-height: 1.4;
  max-width: 100%;
  word-break: break-all;
}

/* ==================== Tab 6 & 7: 大比/天骄赛 ==================== */
.tournament-panel {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}

.tournament-status-bar {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  padding: var(--spacing-sm) var(--spacing-md);
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
}

.tournament-state {
  padding: 4px 10px;
  border-radius: 999px;
  font-size: var(--font-size-sm);
  font-weight: 600;
}

.tournament-state.upcoming {
  background: rgba(33, 150, 243, 0.15);
  color: #64b5f6;
}

.tournament-state.active {
  background: rgba(76, 175, 80, 0.15);
  color: var(--status-success-text);
}

.tournament-state.settled {
  background: rgba(255, 255, 255, 0.08);
  color: var(--text-secondary);
}

.tournament-countdown,
.tournament-round {
  color: var(--text-secondary);
  font-size: var(--font-size-sm);
}

.tournament-reward-bar {
  display: flex;
  justify-content: center;
}

.bracket-view {
  display: flex;
  gap: var(--spacing-lg);
  overflow-x: auto;
  padding-bottom: var(--spacing-sm);
}

.bracket-round {
  min-width: 200px;
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
}

.round-title {
  color: var(--highlight-text);
  font-weight: 600;
  font-size: var(--font-size-sm);
  text-align: center;
  padding-bottom: var(--spacing-xs);
  border-bottom: 1px solid var(--border-color);
}

.bracket-match {
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-sm);
  overflow: hidden;
}

.match-player {
  display: flex;
  align-items: center;
  gap: var(--spacing-xs);
  padding: var(--spacing-xs) var(--spacing-sm);
  font-size: var(--font-size-sm);
  color: var(--text-secondary);
}

.match-player.winner {
  background: rgba(76, 175, 80, 0.08);
  color: var(--status-success-text);
  font-weight: 600;
}

.match-sect {
  font-size: var(--font-size-xs);
  color: var(--text-muted);
  max-width: 60px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.match-name {
  flex: 1;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.match-score {
  font-family: var(--font-mono);
  font-weight: 600;
  min-width: 20px;
  text-align: center;
}

.match-vs {
  text-align: center;
  font-size: var(--font-size-xs);
  color: var(--text-muted);
  padding: 2px 0;
  background: rgba(255, 255, 255, 0.02);
}

/* ==================== Tab 8: 宗门Boss ==================== */
.boss-panel {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}

.boss-layout {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--spacing-md);
}

.boss-card {
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  padding: var(--spacing-md);
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
}

.boss-portrait-row {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
}

.boss-portrait {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  background: rgba(255, 255, 255, 0.06);
  display: flex;
  align-items: center;
  justify-content: center;
  border: 1px solid rgba(255, 255, 255, 0.08);
}

.boss-portrait-icon {
  font-size: 28px;
}

.boss-name {
  font-weight: 700;
  color: var(--highlight-text);
  font-size: var(--font-size-md);
}

.boss-state {
  color: var(--status-success-text);
  font-size: var(--font-size-sm);
}

.boss-hp-bar {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.boss-hp-head {
  display: flex;
  justify-content: space-between;
  color: var(--text-secondary);
  font-size: var(--font-size-sm);
}

.boss-hp-head strong {
  color: var(--text-primary);
}

.boss-track {
  height: 10px;
  border-radius: 999px;
  overflow: hidden;
  background: rgba(255, 255, 255, 0.08);
}

.boss-fill {
  height: 100%;
  border-radius: 999px;
  transition: width 0.3s ease;
}

.boss-fill.hp {
  background: linear-gradient(90deg, #b84a4a, #e07b58);
}

.boss-player-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: var(--spacing-sm);
}

.boss-section-title {
  font-size: var(--font-size-xs);
  font-weight: 700;
  color: var(--highlight-text);
}

.boss-player-name {
  font-size: var(--font-size-md);
  font-weight: 700;
  color: var(--highlight-text);
}

.boss-player-stats {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--spacing-xs);
}

.boss-stat-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--spacing-xs) var(--spacing-sm);
  background: rgba(255, 255, 255, 0.03);
  border-radius: var(--radius-sm);
}

.boss-stat-label {
  color: var(--text-secondary);
  font-size: var(--font-size-xs);
}

.boss-stat-item strong {
  color: var(--text-primary);
  font-size: var(--font-size-sm);
}

.boss-skill-strip {
  display: flex;
  gap: var(--spacing-xs);
  flex-wrap: wrap;
}

.boss-skill-btn {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 2px;
  min-width: 64px;
  padding: var(--spacing-sm);
  background: var(--bg-overlay-light);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-sm);
  color: var(--text-primary);
  cursor: pointer;
  transition: all 0.2s ease;
  font-family: var(--font-primary);
}

.boss-skill-btn:hover:not(:disabled) {
  border-color: var(--accent-text);
}

.boss-skill-btn.disabled,
.boss-skill-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.skill-icon {
  font-size: 18px;
}

.skill-name {
  font-size: var(--font-size-xs);
  font-weight: 600;
}

.skill-cd {
  font-size: 10px;
  color: var(--text-muted);
}

.boss-ranking {
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  padding: var(--spacing-md);
}

.boss-rank-list {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-xs);
  margin-top: var(--spacing-sm);
  max-height: 200px;
  overflow-y: auto;
}

.boss-rank-item {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  padding: var(--spacing-xs) var(--spacing-sm);
  border-radius: var(--radius-sm);
  background: rgba(255, 255, 255, 0.02);
}

.boss-rank-item.me {
  background: rgba(215, 180, 92, 0.08);
  border: 1px solid rgba(215, 180, 92, 0.2);
}

.rank-no {
  width: 24px;
  text-align: center;
  color: var(--highlight-text);
  font-weight: 700;
  font-size: var(--font-size-sm);
}

.rank-name {
  flex: 1;
  color: var(--text-primary);
  font-size: var(--font-size-sm);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.rank-damage {
  color: var(--text-primary);
  font-family: var(--font-mono);
  font-size: var(--font-size-sm);
}

/* ==================== Tab 9: 宗门商店 ==================== */
.contribution-display {
  color: var(--highlight-text);
  font-size: var(--font-size-sm);
  font-weight: 600;
}

.shop-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: var(--spacing-md);
}

.shop-item {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  padding: var(--spacing-md);
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  transition: all 0.15s ease;
}

.shop-item:hover {
  border-color: var(--accent-text);
}

.shop-item-icon {
  font-size: 28px;
  width: 40px;
  text-align: center;
  flex-shrink: 0;
}

.shop-item-info {
  flex: 1;
  min-width: 0;
}

.shop-item-name {
  font-weight: 500;
  color: var(--text-primary);
  font-size: var(--font-size-base);
}

.shop-item-name.rare { color: var(--quality-rare); }
.shop-item-name.epic { color: var(--quality-epic); }
.shop-item-name.legendary { color: var(--quality-legendary); }

.shop-item-desc {
  color: var(--text-secondary);
  font-size: var(--font-size-xs);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.shop-item-stock {
  color: var(--text-muted);
  font-size: var(--font-size-xs);
}

.shop-item-price {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 4px 8px;
  background: rgba(212, 168, 83, 0.1);
  border-radius: var(--radius-sm);
  flex-shrink: 0;
}

.price-icon {
  font-size: 14px;
}

.price-value {
  font-size: var(--font-size-base);
  color: var(--highlight-text);
  font-weight: 600;
  font-family: var(--font-mono);
}

.buy-btn {
  padding: var(--spacing-sm) var(--spacing-md);
  background: linear-gradient(135deg, var(--button-primary-start), var(--button-primary-end));
  color: var(--button-primary-text);
  font-size: var(--font-size-sm);
  flex-shrink: 0;
}

.buy-btn:hover:not(:disabled) {
  background: linear-gradient(135deg, var(--button-primary-hover-start), var(--button-primary-hover-end));
  color: var(--button-primary-hover-text);
  box-shadow: var(--shadow-sm);
}

@media (max-width: 900px) {
  .sect-grid {
    grid-template-columns: repeat(2, 1fr);
  }

  .sect-overview-stats,
  .donation-amounts {
    grid-template-columns: 1fr 1fr;
  }

  .boss-layout {
    grid-template-columns: 1fr;
  }

  .shop-grid {
    grid-template-columns: 1fr;
  }

  .disciple-table-header,
  .disciple-row {
    grid-template-columns: 1fr 0.5fr 0.7fr 0.5fr 0.7fr 0.5fr 0.7fr;
  }

  .bracket-view {
    flex-direction: column;
  }

  .bracket-round {
    min-width: auto;
  }
}
</style>
