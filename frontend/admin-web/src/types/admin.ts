export interface AdminCurrentUser {
  adminId: string
  account: string
  displayName: string
  role: string
  permissions: string[]
}

export interface AdminLoginResponse {
  accessToken: string
  refreshToken: string
  tokenType: string
  expiresIn: number
  currentUser: AdminCurrentUser
}

export interface AdminDashboardSummary {
  playerCount: number
  adminCount: number
  mapTemplateCount: number
  monsterTemplateCount: number
  itemTemplateCount: number
  equipmentTemplateCount: number
}

export interface AdminRuntimeConfigDomainStatus {
  domain: string
  name: string
  group: string
  description: string
  refreshSupported: boolean
  currentVersion?: string | null
  lastAppliedAt?: string | null
  lastAppliedBy?: string | null
  lastRefreshStatus: string
  lastRefreshMessage?: string | null
  refreshCount: number
  createTime?: string | null
  lastUpdateTime?: string | null
}

export interface AdminRuntimeConfigRefreshResult {
  domain: string
  name: string
  success: boolean
  message: string
  status: AdminRuntimeConfigDomainStatus
}

export interface IdAmountEntry {
  id: string
  amount: number
}

export interface QuestObjectiveEntry {
  objectiveType: number
  targetId: string
  targetCount: number
  targetValue: number
  description: string
  activityType: number
}

export interface AlchemyMaterialEntry {
  itemId: string
  itemName: string
  amount: number
  isReplaceable: boolean
  alternativeItemIds: string[]
}

export interface ForgeMaterialEntry {
  itemId: string
  name: string
  icon: string
  count: number
}
export interface RewardGrantEntry {
  type: string
  count: number
  itemId?: string | null
  
  description?: string | null
}
export interface AchievementRequirementEntry {
  requirementType: number
  targetId: string
  targetValue: number
  description: string
}
export interface MapMonsterSpawnRule {
  monsterTemplateId: string
  weight: number
  maxCount: number
}

export interface AdminMapListItem {
  mapId: string
  name: string
  level: number
  nextMapId?: string | null
  monsterCountMin: number
  monsterCountMax: number
  spawnRuleCount: number
  isBuiltIn: boolean
  builtInVersion?: string | null
}

export interface AdminMapDetail {
  mapId: string
  name: string
  level: number
  description: string
  nextMapId?: string | null
  carrying: boolean
  monsterCountMin: number
  monsterCountMax: number
  spawnRules: MapMonsterSpawnRule[]
  isBuiltIn: boolean
  seedKey?: string | null
  builtInVersion?: string | null
  lastUpdateTime?: string | null
  chainValid: boolean
  chainError?: string | null
  chainStages: AdminDungeonMapStage[]
  referencingDungeonIds?: string[]
}

export interface AdminDungeonMapStage {
  stageIndex: number
  mapId: string
  mapName: string
  nextMapId?: string | null
}

export interface BaseAttributesRange {
  minType1?: number | null
  maxType1?: number | null
  minType2?: number | null
  maxType2?: number | null
  minType3?: number | null
  maxType3?: number | null
  minType4?: number | null
  maxType4?: number | null
  minType5?: number | null
  maxType5?: number | null
  minType6?: number | null
  maxType6?: number | null
  minType7?: number | null
  maxType7?: number | null
  minType8?: number | null
  maxType8?: number | null
  minType9?: number | null
  maxType9?: number | null
  minType10?: number | null
  maxType10?: number | null
  minType11?: number | null
  maxType11?: number | null
  minType12?: number | null
  maxType12?: number | null
  minType13?: number | null
  maxType13?: number | null
  minType14?: number | null
  maxType14?: number | null
  minType15?: number | null
  maxType15?: number | null
  element?: number | null
}

export interface DropItem {
  itemId: string
  rate: number
}

export interface DropEquipment {
  equipmentId: string
  rate: number
}

export interface DropCollection {
  seriesId: string
  collectionType: number
  rate: number
}

export interface AdminMonsterListItem {
  monsterId: string
  name: string
  level: number
  skillCount: number
  passiveCount: number
  isBuiltIn: boolean
  builtInVersion?: string | null
}

export interface AdminMonsterDetail {
  monsterId: string
  name: string
  level: number
  expRewardMin: number
  expRewardMax: number
  goldRewardMin: number
  goldRewardMax: number
  skillIds: string[]
  passiveIds: string[]
  elementPool: number[]
  itemDrops: DropItem[]
  equipmentDrops: DropEquipment[]
  collectionDrops: DropCollection[]
  isBuiltIn: boolean
  seedKey?: string | null
  builtInVersion?: string | null
  lastUpdateTime?: string | null
  attributes: BaseAttributesRange
}

export interface AdminWorldBossTemplateListItem {
  bossId: string
  name: string
  monsterTemplateId: string
  isEnabled: boolean
  weight: number
  durationMinutes: number
  sortOrder: number
  lastUpdateTime?: string | null
}

export interface AdminWorldBossTemplateDetail {
  bossId: string
  name: string
  monsterTemplateId: string
  portraitPath?: string | null
  isEnabled: boolean
  weight: number
  durationMinutes: number
  noticeText?: string | null
  participationMinDamage: number
  participationRewardExp: number
  participationRewardGold: number
  participationRewardSpiritStone: number
  rank1RewardExp: number
  rank1RewardGold: number
  rank1RewardSpiritStone: number
  rank2RewardExp: number
  rank2RewardGold: number
  rank2RewardSpiritStone: number
  rank3RewardExp: number
  rank3RewardGold: number
  rank3RewardSpiritStone: number
  sortOrder: number
  lastUpdateTime?: string | null
}

export interface AdminWorldBossSchedule {
  scheduleId: string
  spawnTimeText: string
  timeZoneId: string
  selectionMode: number
  isEnabled: boolean
  lastUpdateTime?: string | null
}

export interface AdminWorldBossRankingEntry {
  rank: number
  playerId: string
  playerName: string
  totalDamage: number
  isSelf: boolean
}

export interface AdminWorldBossLogEntry {
  seq: number
  timestampUtc: string
  actionType: string
  content: string
}

export interface AdminWorldBossRuntimeCurrent {
  hasActiveBoss: boolean
  hasPendingReward: boolean
  pendingRewardInstanceId?: string | null
  instance?: {
    instanceId: string
    bossId: string
    bossName: string
    portraitPath?: string | null
    state: string
    spawnedAtUtc: string
    endAtUtc: string
    remainingSeconds: number
    participantCount: number
  } | null
  boss?: {
    fighterId: string
    name: string
    portraitPath?: string | null
    currentHp: number
    maxHp: number
    currentMp: number
    maxMp: number
  } | null
}

export interface AdminWorldBossRuntime {
  hasActiveInstance: boolean
  current?: AdminWorldBossRuntimeCurrent | null
  rankingTop10: AdminWorldBossRankingEntry[]
  recentLogs: AdminWorldBossLogEntry[]
}

export interface AdminItemListItem {
  itemId: string
  name: string
  useLevel: number
  type: number
  quality: number
  iconPath?: string | null
  isBuiltIn: boolean
  builtInVersion?: string | null
  isTradeable?: boolean
}

export interface AdminItemDetail {
  itemId: string
  name: string
  useLevel: number
  type: number
  description: string
  maxStack: number
  quality: number
  iconPath?: string | null
  isBuiltIn: boolean
  seedKey?: string | null
  builtInVersion?: string | null
  lastUpdateTime?: string | null
  isTradeable?: boolean
  chestConfig?: AdminItemChestConfig | null
  skillBookConfig?: AdminItemSkillBookConfig | null
  petEggConfig?: AdminItemPetEggConfig | null
  pillConfig?: AdminItemPillConfig | null
  recipeUnlockConfig?: AdminItemRecipeUnlockConfig | null
  favorabilityGiftConfig?: AdminItemFavorabilityGiftConfig | null
}

export interface AdminItemChestConfig {
  openMode: number
  rollCount: number
  rewards: AdminItemChestReward[]
}

export interface AdminItemChestReward {
  rewardType: number
  targetId?: string | null
  minCount: number
  maxCount: number
  weight: number
  
  description?: string | null
}

export interface AdminItemSkillBookConfig {
  skillId: number
}

export interface AdminItemRecipeUnlockConfig {
  recipeType: string
  recipeId: string
}

export interface AdminItemPetEggConfig {
  petTemplateId: string
}

export interface AdminItemPillConfig {
  effectType: number
  breakthroughBonusPercent: number
  expGain: number
  attributeType?: string | null
  attributeValue: number
  durationMinutes: number
  maxUsageCount: number
  healHpPercent?: number
  healMpPercent?: number
}

export interface AdminItemFavorabilityGiftConfig {
  favorabilityValue: number
  dailyLimit: number
  canGift: boolean
}

// ===== 好感度管理 =====
export interface AdminFavorabilityRelation {
  id: string
  playerId: string
  playerName: string
  targetPlayerId: string
  targetPlayerName: string
  value: number
  lastGiftTime: string | null
  createdTime: string
}

export interface AdminFavorabilityGiftLog {
  id: string
  playerId: string
  playerName: string
  targetPlayerId: string
  targetPlayerName: string
  itemId: string
  itemName: string
  favorabilityChange: number
  giftTime: string
}

export interface AdminFavorabilityStats {
  totalRelations: number
  totalGiftsToday: number
  totalGiftsAll: number
  averageFavorability: number
}

export interface AdminFavorabilityLevelConfig {
  id: string
  level: number
  minValue: number
  maxValue: number
  color: string
  sortOrder: number
  name: string
  description: string
  rewardJson: string | null
  isBuiltIn: boolean
  lastUpdateTime: string | null
}

export interface AdminFavorabilityLeaderboard {
  rank: number
  playerId: string
  playerName: string
  avatarPath?: string | null
  totalFavorability: number
  relationCount: number
}

export interface AdminDeepFriendship {
  rank: number
  player1Id: string
  player1Name: string
  player2Id: string
  player2Name: string
  combinedValue: number
  levelName: string
  levelColor: string
}

export interface AdminDungeonListItem {
  dungeonId: string
  name: string
  recommendedLevel: number
  dailyLimit: number
  requiredTeamSize: number
  isBuiltIn: boolean
  builtInVersion?: string | null
}

export interface AdminDungeonDetail extends AdminDungeonListItem {
  description: string
  fubenMapId: string
  seedKey?: string | null
  lastUpdateTime?: string | null
  chainValid?: boolean
  chainError?: string | null
  chainStages?: AdminDungeonMapStage[]
  referencingDungeonIds?: string[]
}

export interface AdminDungeonMapStage {
  stageIndex: number
  mapId: string
  mapName: string
  nextMapId?: string | null
}

export interface AdminEquipmentListItem {
  equipmentId: number
  name: string
  level: number
  quality: number
  slot: number
  iconPath?: string | null
  isBuiltIn: boolean
  builtInVersion?: string | null
  isTradeable?: boolean
}

export interface AdminEquipmentDetail {
  equipmentId: number
  name: string
  level: number
  quality: number
  slot: number
  combatStyle: number
  weaponCategory: number
  description: string
  iconPath?: string | null
  isBuiltIn: boolean
  seedKey?: string | null
  builtInVersion?: string | null
  lastUpdateTime?: string | null
  isTradeable?: boolean
  attributes: BaseAttributesRange
}

export interface AssetUploadResult {
  relativePath: string
  url: string
  fileName: string
}

export interface CollectionImageUploadResult {
  originalPath: string
  thumbPath: string
  fileName: string
}

export interface AdminPlayerListItem {
  playerId: string
  name: string
  account: string
  level: number
  profession: string
  professionName: string
  gold: number
  battleMode: string
  isOfflineBattling: boolean
  offlineBattleMapName?: string | null
  alchemistLevel: number
  blacksmithLevel: number
  arrayLevel: number
  spiritFieldLevel: number
}

export interface AdminPlayerDetail {
  playerId: string
  name: string
  account: string
  currentTitle?: string | null
  level: number
  profession: string
  professionName: string
  exp: number
  xExp: number
  gold: number
  honor: number
  guildContribution: number
  spiritStone: number
  isBanned: boolean
  banReason?: string | null
  banExpiresAt?: string | null
  battleCooldownUntilUtc?: string | null
  battleMode: string
  isOfflineBattling: boolean
  offlineBattleMapId?: string | null
  offlineBattleMapName?: string | null
  offlineBattleStartedAtUtc?: string | null
  offlineBattleLastTickAtUtc?: string | null
  offlineBattleTotalBattles: number
  offlineBattleWinBattles: number
  offlineBattleExpGained: number
  offlineBattleGoldGained: number
  arrayLevel: number
  metalLevel: number
  woodLevel: number
  waterLevel: number
  fireLevel: number
  earthLevel: number
  professionLevelCap: number
  alchemistLevel: number
  alchemistExp: number
  isAlchemyCrafting: boolean
  activeAlchemyRecipeId?: string | null
  activeAlchemyCompleteAt?: string | null
  canCollectAlchemy: boolean
  blacksmithLevel: number
  blacksmithExp: number
  isForging: boolean
  activeForgeRecipeId?: string | null
  activeForgeCompleteAt?: string | null
  canCollectForge: boolean
  spiritFieldLevel: number
  spiritFieldUnlockedPlots: number
  spiritFieldMaxPlots: number
  spiritFieldGlobalYieldBonus: number
}

export interface BattleDropSummary {
  itemId: string
  name: string
  quantity: number
  quality: number
}

export interface AdminOfflineBattleListItem {
  playerId: string
  name: string
  account: string
  mapId: string
  mapName: string
  startedAtUtc?: string | null
  lastTickAtUtc?: string | null
  battleCooldownUntilUtc?: string | null
  totalBattles: number
  winBattles: number
  expGained: number
  goldGained: number
}

export interface AdminOfflineBattleSummary {
  playerId: string
  name: string
  account: string
  mapId: string
  mapName: string
  startedAtUtc?: string | null
  stoppedAtUtc?: string | null
  durationSeconds: number
  totalBattles: number
  winBattles: number
  expGained: number
  goldGained: number
  itemDrops: BattleDropSummary[]
  equipmentDrops: BattleDropSummary[]
}

export interface AdminFiveElementListItem {
  playerId: string
  name: string
  account: string
  arrayLevel: number
  metalLevel: number
  woodLevel: number
  waterLevel: number
  fireLevel: number
  earthLevel: number
  professionLevelCap: number
  spiritFieldYieldBonusPercent: number
  battleExpBonusPercent: number
}

export interface AdminFiveElementDetail {
  playerId: string
  name: string
  account: string
  arrayLevel: number
  metalLevel: number
  woodLevel: number
  waterLevel: number
  fireLevel: number
  earthLevel: number
  metalExp: number
  woodExp: number
  waterExp: number
  fireExp: number
  earthExp: number
  professionLevelCap: number
  spiritFieldYieldBonusPercent: number
  battleExpBonusPercent: number
  activeCombinations: string[]
  maxElementLevel: number
  elementBonuses: FiveElementBonus[]
}

export interface FiveElementBonus {
  elementType: string
  elementName: string
  level: number
  maxLevel: number
  attributeType: string
  attributeName: string
  currentBonus: number
  bonusPerLevel: number
}

export interface AdminFiveElementLevelRuleListItem {
  arrayLevel: number
  upgradeGoldCost: number
  upgradeSpiritStoneCost: number
  spiritFieldYieldBonusPercent: number
  battleExpBonusPercent: number
  professionLevelCap: number
  isEnabled: boolean
  isBuiltIn: boolean
  builtInVersion?: string | null
}

export interface AdminFiveElementLevelRuleDetail {
  arrayLevel: number
  upgradeGoldCost: number
  upgradeSpiritStoneCost: number
  upgradeMaterialsJson: string
  spiritFieldYieldBonusPercent: number
  battleExpBonusPercent: number
  professionLevelCap: number
  isEnabled: boolean
  isBuiltIn: boolean
  seedKey?: string | null
  builtInVersion?: string | null
  lastUpdateTime?: string | null
}

export interface AdminFiveElementBranchRuleRangeListItem {
  gid: string
  elementType: string
  minLevel: number
  maxLevel: number
  attributeType: string
  bonusPerLevel: number
  goldCost: number
  spiritStoneCost: number
  isEnabled: boolean
  isBuiltIn: boolean
  builtInVersion?: string | null
}

export interface AdminFiveElementBranchRuleRangeDetail {
  gid: string
  elementType: string
  minLevel: number
  maxLevel: number
  attributeType: string
  bonusPerLevel: number
  goldCost: number
  spiritStoneCost: number
  materialsJson: string
  sortOrder: number
  isEnabled: boolean
  isBuiltIn: boolean
  seedKey?: string | null
  builtInVersion?: string | null
  lastUpdateTime?: string | null
}

export interface AdminFiveElementBranchRuleListItem {
  gid: string
  elementType: string
  targetLevel: number
  goldCost: number
  spiritStoneCost: number
  isEnabled: boolean
  isBuiltIn: boolean
  builtInVersion?: string | null
}

export interface AdminFiveElementBranchRuleDetail {
  gid: string
  elementType: string
  targetLevel: number
  goldCost: number
  spiritStoneCost: number
  materialsJson: string
  sortOrder: number
  isEnabled: boolean
  isBuiltIn: boolean
  seedKey?: string | null
  builtInVersion?: string | null
  lastUpdateTime?: string | null
}

export interface AdminSpiritFieldListItem {
  playerId: string
  name: string
  account: string
  fieldLevel: number
  unlockedPlots: number
  maxPlots: number
  globalYieldBonus: number
  globalGrowthSpeedBonus: number
  totalPlantCount: number
  totalHarvestCount: number
}

export interface AdminSpiritFieldPlotDetail {
  id: number
  plotNumber: number
  level: number
  status: string
  cropTemplateId?: string | null
  cropName?: string | null
  plantTime?: string | null
  expectedHarvestTime?: string | null
  speedUpCount: number
  yieldBonusPercent: number
}

export interface AdminSpiritFieldDetail {
  playerId: string
  name: string
  account: string
  fieldLevel: number
  unlockedPlots: number
  maxPlots: number
  globalYieldBonus: number
  globalGrowthSpeedBonus: number
  todayPlantCount: number
  todayHarvestCount: number
  totalPlantCount: number
  totalHarvestCount: number
  plots: AdminSpiritFieldPlotDetail[]
}

export interface AdminSpiritFieldSystemRule {
  configId: string
  defaultFieldLevel: number
  defaultUnlockedPlots: number
  defaultMaxPlots: number
  defaultInventoryCapacity: number
  plotUpgradeGoldPerLevel: number
  plotUpgradeSpiritStonePerLevel: number
  plotUpgradeYieldBonusPerLevel: number
  isEnabled: boolean
  isBuiltIn: boolean
  seedKey?: string | null
  builtInVersion?: string | null
  lastUpdateTime?: string | null
}

export interface AdminSpiritFieldSpeedUpItemRule {
  itemId: string
  speedUpSeconds: number
  sortOrder: number
  isEnabled: boolean
  isBuiltIn: boolean
  seedKey?: string | null
  builtInVersion?: string | null
  lastUpdateTime?: string | null
}

export interface AdminElementRelationEntry {
  gid: string
  attackerElement: number
  defenderElement: number
  modifier: number
  sortOrder: number
  isEnabled: boolean
  isBuiltIn: boolean
  seedKey?: string | null
  builtInVersion?: string | null
  lastUpdateTime?: string | null
}

export interface AdminAttributePointDefinition {
  profession: string
  key: string
  name: string
  attributeType: number
  bonusPerPoint: number
  pointsPerBonus: number
}

export interface AdminAttributePointLevelRange {
  levelStart: number
  levelEnd: number
  pointsGained: number
}

export interface AdminAttributePointConfigBundle {
  isBuiltIn: boolean
  builtInVersion?: string | null
  lastUpdateTime?: string | null
  attributes: AdminAttributePointDefinition[]
  levelRanges: AdminAttributePointLevelRange[]
}

export interface AdminPlayerLevelConfigListItem {
  level: number
  requiredExp: number
  baseHp: number
  baseMp: number
  basePhysicalAttack: number
  baseMagicAttack: number
  basePhysicalDefense: number
  baseMagicDefense: number
  baseSpeed: number
  isBuiltIn: boolean
  builtInVersion?: string | null
}

export interface AdminPlayerLevelConfigDetail {
  level: number
  requiredExp: number
  baseHp: number
  baseMp: number
  basePhysicalAttack: number
  baseMagicAttack: number
  basePhysicalDefense: number
  baseMagicDefense: number
  baseSpeed: number
  isBuiltIn: boolean
  seedKey?: string | null
  builtInVersion?: string | null
  lastUpdateTime?: string | null
}

export interface AdminRealmLevelConfigListItem {
  level: number
  realmName: string
  alias: string
  realmOrder: number
  layer: number
  requiredExp: number
  attributeBonusPercent: number
  isBreakthroughPoint: boolean
  isBuiltIn: boolean
  builtInVersion?: string | null
}

export interface AdminRealmBreakthroughMaterial {
  itemId: string
  count: number
}

export interface AdminRealmLevelConfigDetail {
  level: number
  realmName: string
  alias: string
  realmOrder: number
  layer: number
  requiredExp: number
  attributeBonusPercent: number
  isBreakthroughPoint: boolean
  breakthroughSuccessRate: number
  breakthroughExpLossPercent: number
  description?: string | null
  isBuiltIn: boolean
  seedKey?: string | null
  builtInVersion?: string | null
  lastUpdateTime?: string | null
  breakthroughMaterials: AdminRealmBreakthroughMaterial[]
}

export interface AdminAuditLogListItem {
  logId: string
  operatorName?: string | null
  operatorRole?: string | null
  httpMethod: string
  path: string
  resourceKey?: string | null
  targetId?: string | null
  success: boolean
  statusCode: number
  errorMessage?: string | null
  createTime: string
}

export interface AdminAuditLogDetail extends AdminAuditLogListItem {
  operatorId?: string | null
  ipAddress?: string | null
  requestJson?: string | null
  responseJson?: string | null
  beforeJson?: string | null
  afterJson?: string | null
  diffJson?: string | null
}

export interface AdminPetListItem {
  templateId: string
  name: string
  type: number
  initialQualityMin: number
  initialQualityMax: number
  isBuiltIn: boolean
  builtInVersion?: string | null
}

export interface AdminPetDetail {
  templateId: string
  name: string
  description?: string | null
  type: number
  initialQualityMin: number
  initialQualityMax: number
  maxQuality: number
  growthRateMin: number
  growthRateMax: number
  initialSkillCount: number
  attributes: BaseAttributesRange
  skillIds: string[]
  obtainMethod?: string | null
  isTradable: boolean
  isBuiltIn: boolean
  seedKey?: string | null
  builtInVersion?: string | null
  lastUpdateTime?: string | null
}

export interface AdminCropListItem {
  templateId: string
  name: string
  type: number
  unlockLevel: number
  isBuiltIn: boolean
  builtInVersion?: string | null
}

export interface AdminCropDetail {
  templateId: string
  name: string
  description?: string | null
  type: number
  growthCycle: number
  yield: number
  seedId: string
  seedAmount: number
  outputItemId: string
  outputAmount: number
  minQuality: number
  maxQuality: number
  unlockLevel: number
  isBuiltIn: boolean
  seedKey?: string | null
  builtInVersion?: string | null
  lastUpdateTime?: string | null
}

export interface AdminCheckInRewardConfig {
  continuousDay: number
  isBuiltIn: boolean
  seedKey?: string | null
  builtInVersion?: string | null
  configVersion: string
  isMilestone: boolean
  rewardJson: string
  description?: string | null
  lastUpdateTime?: string | null
}

export interface AdminRedeemCodeConfig {
  code: string
  isBuiltIn: boolean
  seedKey?: string | null
  builtInVersion?: string | null
  configVersion: string
  isEnabled: boolean
  description: string
  rewardJson: string
  lastUpdateTime?: string | null
}

export interface AdminStarterPackageListItem {
  packageId: string
  name: string
  isEnabled: boolean
  autoGrantOnRegister: boolean
  sortOrder: number
  itemGrantCount: number
  skillGrantCount: number
  configVersion?: string | null
  isBuiltIn: boolean
}

export interface AdminStarterPackageItemGrant {
  gid?: string | null
  itemId: string
  quantity: number
  
  sortOrder: number
}

export interface AdminStarterPackageSkillGrant {
  gid?: string | null
  skillId: number
  sortOrder: number
}

export interface AdminStarterPackageDetail {
  packageId: string
  name: string
  description: string
  isEnabled: boolean
  autoGrantOnRegister: boolean
  sortOrder: number
  configVersion?: string | null
  seedKey?: string | null
  isBuiltIn: boolean
  builtInVersion?: string | null
  remark?: string | null
  createdBy?: string | null
  updatedBy?: string | null
  createTime?: string | null
  lastUpdateTime?: string | null
  itemGrants: AdminStarterPackageItemGrant[]
  skillGrants: AdminStarterPackageSkillGrant[]
}

export interface AdminAlchemySystemListItem {
  playerId: string
  name: string
  account: string
  furnaceLevel: number
  alchemistLevel: number
  professionLevelCap: number
  proficiency: number
  isCrafting: boolean
  activeRecipeId?: string | null
  activeRecipeName?: string | null
  activeCraftCompleteAt?: string | null
  canCollect: boolean
  totalCraftCount: number
}

export interface AdminAlchemySystemDetail {
  playerId: string
  name: string
  account: string
  furnaceLevel: number
  alchemistLevel: number
  alchemistExp: number
  professionLevelCap: number
  proficiency: number
  successRateBonus: number
  craftTimeReduction: number
  yieldBonus: number
  todayCraftCount: number
  totalCraftCount: number
  successCraftCount: number
  isCrafting: boolean
  activeRecipeId?: string | null
  activeRecipeName?: string | null
  activeCraftStartedAt?: string | null
  activeCraftCompleteAt?: string | null
  canCollect: boolean
}

export interface AdminAlchemyProfessionLevelRule {
  level: number
  nextLevelExp: number
  isEnabled: boolean
  isBuiltIn: boolean
  builtInVersion?: string | null
  seedKey?: string | null
  lastUpdateTime?: string | null
}

export interface AdminAlchemyProfessionRule {
  configId: string
  successBonusPerOverLevel: number
  maxSuccessBonus: number
  successExpBase: number
  successExpPerRequiredLevel: number
  failureExpBase: number
  failureExpPerRequiredLevel: number
  isEnabled: boolean
  isBuiltIn: boolean
  builtInVersion?: string | null
  seedKey?: string | null
  lastUpdateTime?: string | null
}

export interface AdminAlchemyRecipeListItem {
  recipeId: string
  name: string
  pillTemplateId: string
  requiredFurnaceLevel: number
  isBuiltIn: boolean
  builtInVersion?: string | null
}

export interface AdminAlchemyRecipeDetail {
  recipeId: string
  pillTemplateId: string
  name: string
  description?: string | null
  requiredFurnaceLevel: number
  baseSuccessRate: number
  baseCraftTime: number
  materialsJson: string
  unlockCondition?: string | null
  isDefaultLearned: boolean
  isBuiltIn: boolean
  seedKey?: string | null
  builtInVersion?: string | null
  lastUpdateTime?: string | null
}

export interface AdminForgeRecipeListItem {
  recipeId: string
  name: string
  templateId: string
  level: number
  isBuiltIn: boolean
  builtInVersion?: string | null
}

export interface AdminForgeRecipeDetail {
  recipeId: string
  templateId: string
  name: string
  description: string
  slotName: string
  quality: number
  level: number
  icon: string
  costGold: number
  successRate: number
  materialsJson: string
  isEnabled: boolean
  isBuiltIn: boolean
  seedKey?: string | null
  builtInVersion?: string | null
  lastUpdateTime?: string | null
}

export interface AdminForgeSystemListItem {
  playerId: string
  name: string
  account: string
  blacksmithLevel: number
  professionLevelCap: number
  isForging: boolean
  activeRecipeId?: string | null
  activeRecipeName?: string | null
  activeForgeCompleteAt?: string | null
  canCollect: boolean
  totalForgeCount: number
}

export interface AdminForgeSystemDetail {
  playerId: string
  name: string
  account: string
  blacksmithLevel: number
  blacksmithExp: number
  professionLevelCap: number
  todayForgeCount: number
  totalForgeCount: number
  successForgeCount: number
  isForging: boolean
  activeRecipeId?: string | null
  activeRecipeName?: string | null
  activeForgeStartedAt?: string | null
  activeForgeCompleteAt?: string | null
  canCollect: boolean
}

export interface AdminForgeProfessionLevelRule {
  level: number
  nextLevelExp: number
  isEnabled: boolean
  isBuiltIn: boolean
  builtInVersion?: string | null
  seedKey?: string | null
  lastUpdateTime?: string | null
}

export interface AdminForgeProfessionRule {
  configId: string
  successBonusPerOverLevel: number
  maxSuccessBonus: number
  successExpBase: number
  successExpPerRequiredLevel: number
  failureExpBase: number
  failureExpPerRequiredLevel: number
  isEnabled: boolean
  isBuiltIn: boolean
  builtInVersion?: string | null
  seedKey?: string | null
  lastUpdateTime?: string | null
}

export interface AdminEquipmentRerollSystemConfig {
  configId: string
  isEnabled: boolean
  rerollStoneItemId: string
  baseStoneCost: number
  extraStoneCostPerLockedLine: number
  maxLockedLineCount: number
  baseGoldCost: number
  goldCostPerEquipmentLevel: number
  qualityGoldMultipliersJson?: string | null
  rerollCountGoldGrowthPercent: number
  isBuiltIn: boolean
  builtInVersion?: string | null
  seedKey?: string | null
  lastUpdateTime?: string | null
}

export interface AdminEquipmentEnhanceRule {
  gid: number
  minEquipmentLevel: number
  maxEquipmentLevel: number
  materialItemId: string
  materialCount: number
  goldCost: number
  successRate: number
  attributeGrowthPercent: number
  maxEnhanceLevel: number
  sortOrder: number
  isEnabled: boolean
  isBuiltIn: boolean
  builtInVersion?: string | null
  seedKey?: string | null
  lastUpdateTime?: string | null
}

export interface AdminEquipmentDecomposeRule {
  gid: number
  quality: number
  materialItemId: string
  minQuantity: number
  maxQuantity: number
  sortOrder: number
  isEnabled: boolean
  isBuiltIn: boolean
  builtInVersion?: string | null
  seedKey?: string | null
  lastUpdateTime?: string | null
}

export interface AdminEquipmentRerollCostRule {
  gid: number
  minEquipmentLevel: number
  maxEquipmentLevel: number
  materialItemId: string
  materialCount: number
  goldCost: number
  extraMaterialPerLockedLine: number
  extraGoldPerLockedLine: number
  sortOrder: number
  isEnabled: boolean
  isBuiltIn: boolean
  builtInVersion?: string | null
  seedKey?: string | null
  lastUpdateTime?: string | null
}

export interface AdminEquipmentRerollSlotPoolConfig {
  gid: number
  slot: number
  attributeType: number
  tier: number
  maxDuplicateCount: number
  sortOrder: number
  isEnabled: boolean
  isBuiltIn: boolean
  builtInVersion?: string | null
  seedKey?: string | null
  lastUpdateTime?: string | null
}

export interface AdminEquipmentRerollAttributeValueConfig {
  gid: number
  attributeType: number
  tier: number
  minValue: string
  maxValue: string
  isPercentage: boolean
  sortOrder: number
  isEnabled: boolean
  isBuiltIn: boolean
  builtInVersion?: string | null
  seedKey?: string | null
  lastUpdateTime?: string | null
}

export interface AdminEquipmentRerollTierConfig {
  gid: number
  tier: number
  name: string
  color: string
  weight: number
  valueMultiplier: string
  sortOrder: number
  isEnabled: boolean
  isBuiltIn: boolean
  builtInVersion?: string | null
  seedKey?: string | null
  lastUpdateTime?: string | null
}

export interface AdminQuestListItem {
  questId: string
  questName: string
  questType: number
  resetCycle: number
  requiredLevel: number
  isEnabled: boolean
  isBuiltIn: boolean
  builtInVersion?: string | null
}

export interface AdminQuestDetail {
  questId: string
  questName: string
  questType: number
  resetCycle: number
  description: string
  requiredLevel: number
  preQuestIds?: string | null
  autoAccept: boolean
  autoSubmit: boolean
  timeLimit: number
  rewardExp: number
  rewardGold: number
  rewardSpiritStone: number
  rewardGuildContribution: number
  rewardItemsJson?: string | null
  rewardEquipmentIds?: string | null
  objectivesJson?: string | null
  sortOrder: number
  isEnabled: boolean
  isBuiltIn: boolean
  seedKey?: string | null
  builtInVersion?: string | null
  lastUpdateTime?: string | null
}

export interface AdminAchievementListItem {
  achievementId: string
  achievementName: string
  achievementType: number
  difficulty: number
  isEnabled: boolean
  isBuiltIn: boolean
  builtInVersion?: string | null
}

export interface AdminAchievementDetail {
  achievementId: string
  achievementName: string
  achievementType: number
  difficulty: number
  description: string
  category: string
  points: number
  isHidden: boolean
  preAchievementIds?: string | null
  rewardGold: number
  rewardSpiritStone: number
  rewardExp: number
  rewardTitle?: string | null
  rewardItemsJson?: string | null
  rewardEquipmentIds?: string | null
  requirementType: number
  requirementTargetValue: number
  requirementDescription?: string | null
  requirementsJson?: string | null
  sortOrder: number
  isEnabled: boolean
  isBuiltIn: boolean
  seedKey?: string | null
  builtInVersion?: string | null
  lastUpdateTime?: string | null
}

export interface AdminShopConfigListItem {
  shopId: string
  shopName: string
  shopType: number
  isOpen: boolean
  isEnabled: boolean
  isBuiltIn: boolean
  builtInVersion?: string | null
}

export interface AdminShopConfigDetail {
  shopId: string
  shopName: string
  shopType: number
  description?: string | null
  requiredLevel: number
  requiredVipLevel: number
  discount: number
  isOpen: boolean
  autoRefresh: boolean
  refreshIntervalHours: number
  icon?: string | null
  sortOrder: number
  isEnabled: boolean
  isBuiltIn: boolean
  seedKey?: string | null
  builtInVersion?: string | null
  lastUpdateTime?: string | null
}

export interface AdminShopItemListItem {
  gid: string
  shopId: string
  shopName: string
  itemId: string
  name: string
  itemType: number
  currentPrice: number
  isEnabled: boolean
  isBuiltIn: boolean
  builtInVersion?: string | null
}

export interface AdminShopItemDetail {
  gid: string
  shopId: string
  itemId: string
  itemType: number
  basePrice: number
  currentPrice: number
  stock: number
  initialStock: number
  dailyLimit: number
  sortOrder: number
  isEnabled: boolean
  isBuiltIn: boolean
  seedKey?: string | null
  builtInVersion?: string | null
  lastUpdateTime?: string | null
}

export interface AdminRankingConfigListItem {
  rankingId: string
  rankingName: string
  rankingType: number
  isEnabled: boolean
  isBuiltIn: boolean
  builtInVersion?: string | null
}

export interface AdminRankingConfigDetail {
  rankingId: string
  rankingName: string
  rankingType: number
  description?: string | null
  maxSize: number
  updateInterval: number
  seasonEnabled: boolean
  seasonDuration: number
  currentSeason: number
  seasonStartTime?: string | null
  sortOrder: number
  isEnabled: boolean
  isBuiltIn: boolean
  seedKey?: string | null
  builtInVersion?: string | null
  lastUpdateTime?: string | null
}

export interface AdminRankingRewardListItem {
  gid: string
  rankingId: string
  rewardTitle: string
  minRank: number
  maxRank: number
  isBuiltIn: boolean
  builtInVersion?: string | null
}

export interface AdminRankingRewardDetail {
  gid: string
  rankingId: string
  minRank: number
  maxRank: number
  rewardTitle: string
  gold: number
  spiritStone: number
  title?: string | null
  isBuiltIn: boolean
  seedKey?: string | null
  builtInVersion?: string | null
  lastUpdateTime?: string | null
}

export interface AdminGrantCurrencyRequest {
  resourceType: string
  amount: number
  reason: string
}

export interface AdminGrantItemRequest {
  itemId: string
  quantity: number
  
  reason: string
}

export interface AdminUserListItem {
  adminId: string
  account: string
  displayName: string
  role: string
  isActive: boolean
}

export interface AdminUserDetail {
  adminId: string
  account: string
  displayName: string
  role: string
  isActive: boolean
  lastLoginTime?: string | null
}

export interface AdminCreateUserRequest {
  account: string
  displayName: string
  role: string
  password: string
}

export interface AdminUpdateUserRequest {
  displayName: string
  role: string
  isActive: boolean
}

export interface AdminResetPasswordRequest {
  newPassword: string
}


export interface AdminBanPlayerRequest {
  reason: string
  hours: number
}

export interface SkillHit {
  damageMultiplier: number
  baseDamage: number
  hitDamageType?: number | null
  description: string
}

export interface BuffEffect {
  effectType: number
  value: number
  isPercentage: boolean
  targetCamp: number
  targetCount: number
  isRandomTarget: boolean
  triggerChance: number
  targetSelectionMode: number
  targetSelfOnly: boolean
  effectDuration: number
}

export interface AdminSkillListItem {
  skillId: number
  skillCatalog?: string
  name: string
  targetType: number
  cooldown: number
  skillLevel: number
  nextSkillId?: number | null
  allowedProfessionText: string
  isBuiltIn: boolean
  builtInVersion?: string | null
}

export interface AdminSkillNextLevel {
  previousSkillId: number
  skillId: number
  skillLevel: number
  name: string
  damageType: number
  manaCost: number
  cooldown: number
  upgradeConditionSummary: string
  upgradeConditions: SkillUpgradeCondition[]
}

export interface SkillUpgradeCondition {
  type: number
  amount: number
  itemId?: string | null
}

export interface AdminSkillDetail {
  skillId: number
  skillCatalog?: string
  name: string
  description: string
  nextSkillId?: number | null
  skillLevel: number
  previousSkillId?: number | null
  nextSkillName?: string | null
  upgradeConditions: SkillUpgradeCondition[]
  nextSkill?: AdminSkillDetail | null
  targetType: number
  manaCost: number
  cooldown: number
  damageType: number
  hitCount: number
  rangeType: number
  damageMultiplier: number
  triggerChance: number
  hits: SkillHit[]
  buffIds: string[]
  allowedProfessions: string[]
  isBuiltIn: boolean
  seedKey?: string | null
  builtInVersion?: string | null
  lastUpdateTime?: string | null
}

export interface AdminBuffListItem {
  buffId: string
  name: string
  duration: number
  maxStack: number
  isBuiltIn: boolean
  builtInVersion?: string | null
}

export interface AdminBuffDetail {
  buffId: string
  name: string
  description: string
  duration: number
  maxStack: number
  stackRule: number
  effects: BuffEffect[]
  isBuiltIn: boolean
  seedKey?: string | null
  builtInVersion?: string | null
  lastUpdateTime?: string | null
}

// ─── Sect System Types ──────────────────────────────────────────

export interface AdminSectTemplateListItem {
  sectId: string
  name: string
  isEnabled: boolean
  sortOrder: number
  memberCount: number
}

export interface AdminSectTemplateDetail {
  sectId: string
  name: string
  description: string
  icon: string
  portraitPath: string
  heartSutraIdsJson: string
  isEnabled: boolean
  sortOrder: number
}

export interface AdminHeartSutraListItem {
  sutraId: string
  name: string
  sectId: string
  sectName: string
  maxLayer: number
}

export interface AdminHeartSutraDetail {
  sutraId: string
  name: string
  description: string
  sectId: string
  maxLayer: number
  layersJson: string
  sortOrder: number
}

export interface AdminSectBossTemplateListItem {
  bossId: string
  name: string
  monsterTemplateId: string
  isEnabled: boolean
}

export interface AdminSectBossTemplateDetail {
  bossId: string
  name: string
  monsterTemplateId: string
  portraitPath: string
  isEnabled: boolean
  durationMinutes: number
  participationRewardContribution: number
  rank1RewardContribution: number
  rank2RewardContribution: number
  rank3RewardContribution: number
  sortOrder: number
}

export interface AdminSectTournamentSchedule {
  scheduleId: string
  spawnTimeText: string
  timeZoneId: string
  isEnabled: boolean
  durationMinutes: number
}

export interface AdminSectShopItemListItem {
  gid: string
  shopId: string
  itemId: string
  itemType: number
  contributionCost: number
}

export interface AdminSectShopItemDetail {
  gid: string
  shopId: string
  itemId: string
  itemType: number
  contributionCost: number
  stock: number
  dailyLimit: number
  sortOrder: number
}

export interface AdminSectBlessingListItem {
  blessingId: string
  name: string
  requiredGuildLevel: number
  buffId: string
}

export interface AdminSectBlessingDetail {
  blessingId: string
  name: string
  description: string
  requiredGuildLevel: number
  buffId: string
  sortOrder: number
}

export interface AdminGuildListItem {
  guildId: string
  name: string
  sectName: string
  level: number
  memberCount: number
  leaderName: string
  totalDonation: number
}

export interface AdminGuildDetail {
  guildId: string
  name: string
  sectTemplateId: string
  sectName: string
  level: number
  exp: number
  memberCount: number
  maxMembers: number
  funds: number
  leaderId: string
  leaderName: string
  totalDonation: number
  announcement: string
  createTime: string
}

// ===== 文字图鉴系列 =====
export interface AdminTextCollectionSeriesListItem {
  seriesId: string
  name: string
  description: string
  icon: string
  isEnabled: boolean
  isBuiltIn: boolean
  builtInVersion: string | null
}

export interface AdminTextCollectionSeriesDetail {
  seriesId: string
  name: string
  description: string
  icon: string
  sortOrder: number
  isEnabled: boolean
  isBuiltIn: boolean
  seedKey: string | null
  builtInVersion: string | null
  lastUpdateTime: string | null
}

// ===== 文字图鉴项 =====
export interface AdminTextCollectionItemListItem {
  itemId: string
  seriesId: string
  character: string
  slotIndex: number
  isEnabled: boolean
  isBuiltIn: boolean
  builtInVersion: string | null
}

export interface AdminTextCollectionItemDetail {
  itemId: string
  seriesId: string
  character: string
  slotIndex: number
  sortOrder: number
  isEnabled: boolean
  isBuiltIn: boolean
  seedKey: string | null
  builtInVersion: string | null
  lastUpdateTime: string | null
}

// ===== 文字图鉴属性加成 =====
export interface AdminTextCollectionBonusListItem {
  bonusId: string
  seriesId: string
  attrType: string
  attrValue: number
  valueType: number
  isBuiltIn: boolean
  builtInVersion: string | null
}

export interface AdminTextCollectionBonusDetail {
  bonusId: string
  seriesId: string
  attrType: string
  attrValue: number
  valueType: number
  sortOrder: number
  isBuiltIn: boolean
  seedKey: string | null
  builtInVersion: string | null
  lastUpdateTime: string | null
}

// ===== 图片图鉴系列 =====
export interface AdminImageCollectionSeriesListItem {
  seriesId: string
  name: string
  description: string
  icon: string
  isEnabled: boolean
  isBuiltIn: boolean
  builtInVersion: string | null
}

export interface AdminImageCollectionSeriesDetail {
  seriesId: string
  name: string
  description: string
  icon: string
  sortOrder: number
  isEnabled: boolean
  isBuiltIn: boolean
  seedKey: string | null
  builtInVersion: string | null
  lastUpdateTime: string | null
}

// ===== 图片图鉴项 =====
export interface AdminImageCollectionItemListItem {
  itemId: string
  seriesId: string
  imageName: string
  thumbUrl: string
  originalUrl: string
  isEnabled: boolean
  isBuiltIn: boolean
  builtInVersion: string | null
}

export interface AdminImageCollectionItemDetail {
  itemId: string
  seriesId: string
  imageName: string
  thumbUrl: string
  originalUrl: string
  sortOrder: number
  isEnabled: boolean
  isBuiltIn: boolean
  seedKey: string | null
  builtInVersion: string | null
  lastUpdateTime: string | null
}

// ===== 图片图鉴属性加成 =====
export interface AdminImageCollectionBonusListItem {
  bonusId: string
  seriesId: string
  attrType: string
  attrValue: number
  valueType: number
  isBuiltIn: boolean
  builtInVersion: string | null
}

export interface AdminImageCollectionBonusDetail {
  bonusId: string
  seriesId: string
  attrType: string
  attrValue: number
  valueType: number
  sortOrder: number
  isBuiltIn: boolean
  seedKey: string | null
  builtInVersion: string | null
  lastUpdateTime: string | null
}

// ===== 抽奖池 =====
export interface AdminLotteryPoolListItem {
  poolId: string
  name: string
  lotteryType: number
  isEnabled: boolean
  isBuiltIn: boolean
  builtInVersion: string | null
}

export interface AdminLotteryPoolDetail {
  poolId: string
  name: string
  lotteryType: number
  costType: number
  costItemId: string | null
  costAmount: number
  isEnabled: boolean
  startTime: string | null
  endTime: string | null
  supportSingle: boolean
  supportTen: boolean
  dailyLimit: number
  totalLimit: number
  sortOrder: number
  isBuiltIn: boolean
  seedKey: string | null
  builtInVersion: string | null
  lastUpdateTime: string | null
}

// ===== 抽奖奖项 =====
export interface AdminLotteryPrizeListItem {
  prizeId: string
  poolId: string
  rewardType: number
  rewardTargetId: string | null
  rewardAmount: number
  probability: number
  isEnabled: boolean
  isBuiltIn: boolean
  builtInVersion: string | null
}

export interface AdminLotteryPrizeDetail {
  prizeId: string
  poolId: string
  rewardType: number
  rewardTargetId: string | null
  rewardAmount: number
  probability: number
  sortOrder: number
  isEnabled: boolean
  isBuiltIn: boolean
  seedKey: string | null
  builtInVersion: string | null
  lastUpdateTime: string | null
}

// ===== 抽奖日志 =====
export interface AdminLotteryLogListItem {
  logId: string
  playerId: string
  poolId: string
  lotteryType: number
  costType: number
  costAmount: number
  rewardType: number
  rewardName: string
  rewardAmount: number
  isThanks: boolean
  lotteryTime: string
}

export interface AdminPagedResult<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
}

// ===== 秘境实例模板 =====
export interface AdminDungeonInstanceTemplateListItem {
  id: string
  name: string
  enabled: boolean
  recommendedLevel: number
  dailyEnterLimit: number
  tickIntervalSeconds: number
  isBuiltIn: boolean
  builtInVersion?: string | null
  eventGroupId?: string | null
}

export interface AdminDungeonInstanceTemplateDetail {
  id: string
  name: string
  description: string
  enabled: boolean
  recommendedLevel: number
  dailyEnterLimit: number
  tickIntervalSeconds: number
  openScheduleJson?: string | null
  entryCostsJson?: string | null
  eventGroupId?: string | null
  autoMedicineConfigJson?: string | null
  encounterConfigJson?: string | null
  isBuiltIn: boolean
  seedKey?: string | null
  builtInVersion?: string | null
  lastUpdateTime?: string | null
}

// ===== 秘境事件配置 =====
export interface AdminDungeonEventConfigListItem {
  id: string
  name: string
  dungeonId: string
  eventType: number
  weight: number
  enabled: boolean
  deathKeep?: boolean | null
  isBuiltIn: boolean
  builtInVersion?: string | null
}

export interface AdminDungeonEventConfigDetail {
  id: string
  name: string
  description: string
  dungeonId: string
  eventType: number
  weight: number
  enabled: boolean
  eventDataJson?: string | null
  deathKeep?: boolean | null
  isBuiltIn: boolean
  seedKey?: string | null
  builtInVersion?: string | null
  lastUpdateTime?: string | null
}

// ===== 秘境事件组 =====
export interface AdminDungeonEventGroupListItem {
  id: string
  name: string
  eventCount: number
  isBuiltIn: boolean
  builtInVersion?: string | null
}

export interface AdminDungeonEventGroupDetail {
  id: string
  name: string
  description: string
  groupItemsJson?: string | null
  isBuiltIn: boolean
  seedKey?: string | null
  builtInVersion?: string | null
  lastUpdateTime?: string | null
}

export interface EventGroupItem {
  eventId: string
  weight: number
}

// ===== 邮件系统 =====

export interface AdminMailListItem {
  id: number
  recipientId?: string | null
  senderType: string
  senderName: string
  title: string
  isGlobal: boolean
  hasAttachments: boolean
  createdAt: string
  expireAt: string
}

export interface AdminMailDetail {
  id: number
  recipientId?: string | null
  senderType: string
  senderName: string
  title: string
  content: string
  attachmentsJson?: string | null
  isGlobal: boolean
  createdAt: string
  expireAt: string
}

export interface AdminSendMailPayload {
  recipientId?: string | null
  isGlobal: boolean
  senderName: string
  title: string
  content: string
  attachmentsJson?: string | null
}

export interface MailAttachments {
  gold: number
  spiritStone: number
  items?: Array<{ itemId: string; quantity: number }>
  equipment?: Array<{ templateId: string; quality: number }>
  titles?: Array<{ titleId: string }>
}

// ===== 称号系统 =====

export interface AdminTitleListItem {
  id: number
  titleId: string
  name: string
  source: string
  rarity: string
  isVisible: boolean
}

export interface AdminTitleDetail {
  id: number
  titleId: string
  name: string
  description?: string | null
  source: string
  sourceId?: string | null
  rarity: string
  iconPath?: string | null
  imagePath?: string | null
  isVisible: boolean
  createdAt: string
}

export interface AdminGrantTitlePayload {
  playerId: string
  titleId: string
}

// ===== PVP竞技场 =====

export interface AdminArenaOverview {
  seasonNumber: number
  totalPlayers: number
  todayBattles: number
  activePlayers: number
  averagePoints: number
}

export interface AdminArenaPlayer {
  id: number
  playerId: string
  playerName: string
  points: number
  rank: number
  wins: number
  losses: number
  winStreak: number
  bestRank: number
  lastBattleAt?: string | null
  bannedUntil?: string | null
  banReason?: string | null
}

export interface AdminArenaBattleLog {
  id: number
  attackerId: string
  attackerName: string
  defenderId: string
  defenderName: string
  attackerPointsBefore: number
  defenderPointsBefore: number
  attackerPointsAfter: number
  defenderPointsAfter: number
  winnerId: string
  battleLogJson?: string | null
  seasonNumber: number
  createdAt: string
}

export interface AdminArenaSeason {
  seasonNumber: number
  seasonEnabled: boolean
  seasonDuration: number
  seasonStartTime: string
  seasonEndTime: string
  daysRemaining: number
  totalPlayers: number
  rewards: AdminArenaSeasonReward[]
}

export interface AdminArenaSeasonReward {
  gid: string
  minRank: number
  maxRank: number
  rewardTitle: string
  gold: number
  spiritStone: number
  title?: string | null
}

export interface AdminSettleSeasonResult {
  seasonNumber: number
  rewardedPlayers: number
  messages: string[]
}

// ===== 通天塔 =====

export interface AdminTowerOverview {
  totalPlayers: number
  todayChallenges: number
  averageHighestFloor: number
}

export interface AdminTowerPlayer {
  id: number
  playerId: string
  playerName: string
  highestFloor: number
  currentFloor: number
  dailyAttemptsUsed: number
  lastAttemptAt?: string | null
}

export interface AdminTowerFloorConfig {
  id: number
  floor: number
  monsterTemplateIdsJson: string
  monsterCount: number
  statMultiplier: number
  rewardGold: number
  rewardExp: number
  milestoneRewardJson?: string | null
}

export interface TowerFloorDistribution {
  floor: number
  playerCount: number
}

export interface AdminGemListItem {
  id: number
  gemId: string
  name: string
  level: number
  attributeType: string
  bonusValue: number
  bonusMode: string
  quality: number
  synthCount: number
  synthFromGemId?: string | null
  isBuiltIn: boolean
  synthSuccessRate: number
}

export interface AdminGemDetail {
  id: number
  gemId: string
  name: string
  level: number
  attributeType: string
  bonusValue: number
  bonusMode: string
  iconPath?: string | null
  quality: number
  synthCount: number
  synthFromGemId?: string | null
  isBuiltIn: boolean
  synthSuccessRate: number
}

export interface AdminGemSavePayload {
  gemId: string
  name: string
  level: number
  attributeType: string
  bonusValue: number
  bonusMode: string
  iconPath?: string | null
  quality: number
  synthCount: number
  synthFromGemId?: string | null
  synthSuccessRate: number
}

export interface AdminGemBatchGeneratePayload {
  attributeType: string
  baseBonusValue: number
  bonusValueGrowth: number
  bonusMode: string
  maxLevel: number
  namePrefix: string
  synthSuccessRate: number
}

export interface FeedbackListItem {
  id: number
  playerId: string
  playerName: string
  type: string
  title: string
  attachmentCount: number
  status: string
  createdAt: string
  handledByAdminId?: string | null
  handledAt?: string | null
}
export interface FeedbackAttachment { id: number; relativePath: string; originalFileName?: string | null; contentType: string; fileSize: number; sortOrder: number }
export interface FeedbackStatusHistory { fromStatus?: string | null; toStatus: string; adminId: string; replySnapshot?: string | null; noteSnapshot?: string | null; createdAt: string }
export interface FeedbackDetail extends FeedbackListItem { content: string; adminReply?: string | null; internalNote?: string | null; attachments: FeedbackAttachment[]; statusHistory: FeedbackStatusHistory[] }
export interface ProcessFeedbackPayload { status: string; adminReply?: string; internalNote?: string }
export interface AdminMarketConfig {
  feePercent: number
  maxListingsPerPlayer: number
  listingDurationDays: number
  allowedCurrencies: string
  minPrice: number
}

export interface AdminMarketListing {
  id: number
  sellerId: string
  sellerName: string
  itemType: string
  itemName: string
  quantity: number
  price: number
  currencyType: string
  status: string
  createdAt: string
  expireAt: string
}

export interface AdminMarketTransaction {
  id: number
  sellerName: string
  buyerId?: string | null
  itemType: string
  itemName: string
  quantity: number
  price: number
  currencyType: string
  fee: number
  soldAt?: string | null
}
