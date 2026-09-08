# 种子数据总清单

> 更新时间：2026-09-01
>
> 本文记录当前项目中所有“会初始化数据库”或“作为初始化数据来源”的代码。数据内容仍以现有 C# 实现为准，本文不复制整份数据，避免出现第二份事实来源。

## 1. 结论

当前种子数据有四类来源：

1. `XXX.Infrastructure/SeedData/SeedDataService.cs`：后端主要数据库种子入口。
2. `XXX.Infrastructure/SeedData/BalanceCatalog.cs`、`DungeonInstanceSeedData.cs`、`CollectionLotterySeedData.cs`：后端专用数据目录，由 `SeedDataService` 调用。
3. `XXX.Infrastructure/SeedData/SectSeedData.cs`：宗门数据，在数据库建表/兼容迁移完成后由 `DbContext.InitDatabase()` 直接调用。
4. `XXX.Core` 中的 legacy/default catalog，以及前端 `XXXV/xiuxian-game/src/data/mockData.js`：作为种子来源或开发模拟数据，不是独立的数据库启动入口。

项目当前没有实际被 `SeedDataService` 读取的 JSON 种子文件；`SeedDataService` 中的通用 JSON 加载方法属于遗留兼容代码。

## 2. 启动执行顺序

```text
Program.cs
  └─ DbContext.InitDatabase()
       ├─ 创建/兼容 SQLite 表结构
       └─ SectSeedData.SeedAll(db)
  └─ ISeedDataService.InitializeAsync()
       ├─ 检查核心表是否缺失或数量不足
       ├─ BalanceCatalogSyncService.SyncAsync()
       ├─ 按顺序执行 SeedDataService 的各领域种子
       └─ 开发环境按配置创建默认管理员
  └─ RuntimeTemplateLoader.LoadAsync()
       └─ 从数据库加载模板到运行时缓存
  └─ 各规则 runtime service 加载数据库配置
  └─ ShopService / RankingService 加载缓存
```

对应代码入口：

- [Program.cs](XXX.WebApi/Program.cs:458)
- [DbContext.cs](XXX.Infrastructure/Data/DbContext.cs:113)
- [SeedDataService.cs](XXX.Infrastructure/SeedData/SeedDataService.cs:107)
- [RuntimeTemplateLoader.cs](XXX.Infrastructure/SeedData/RuntimeTemplateLoader.cs:31)

## 3. 后端数据库种子清单

### 3.1 模板与基础平衡数据

| 数据域 | 数据表 | 来源/入口 | 内置版本或标识 | 说明 |
| --- | --- | --- | --- | --- |
| 道具 | `ItemTemplates` | `BalanceCatalog.BuildItemTemplates()` → `BalanceCatalogSyncService` | `balance-playable-v4-20260331`；模板版本 `item-template-built-in-v1-20260403` | 含普通道具、材料、种子、丹药、宝石相关道具等 |
| 装备 | `EquipmentTemplates` | `BalanceCatalog.BuildEquipmentTemplates()` → `BalanceCatalogSyncService` | `equipment-template-built-in-v1-20260403` | 装备模板及基础属性 |
| 怪物 | `MonsterTemplates` | `BalanceCatalog.BuildMonsterTemplates()` → `BalanceCatalogSyncService` | `monster-template-built-in-v1-20260403` | 等级、战斗风格、元素池、技能和掉落 |
| 地图 | `MapTemplates` | `BalanceCatalog.BuildMapTemplates()` → `BalanceCatalogSyncService` | `map-template-built-in-v1-20260403` | 普通地图、刷怪规则和地图链路 |
| 灵宠 | `PetTemplates` | `BalanceCatalog.BuildPetTemplates()` → `BalanceCatalogSyncService` | `pet-template-built-in-v1-20260403` | 灵宠模板、技能和成长 |
| 副本目录 | `DungeonTemplates` | `BalanceCatalog.BuildDungeonTemplates()` → `BalanceCatalogSyncService` | `dungeon-template-built-in-v1-20260403` | 副本基础目录 |
| 技能 | `SkillTemplates` | `SkillData.InitializeSkills()` → `TemplateDataMapper` | `skill-template-built-in-v1-20260403` | legacy 内置技能转数据库模板 |
| Buff | `BuffTemplates` | `BuffDataTemplates.InitializeBuffTemplates()` → `TemplateDataMapper` | `buff-template-built-in-v1-20260403` | legacy 内置 Buff 转数据库模板 |
| 道具扩展 | `ItemChestConfigs`、`ItemChestRewardEntries` | `SeedItemExtensionConfigsAsync()` | 与道具内置版本同周期 | 宝箱配置与宝箱奖励 |
| 道具扩展 | `ItemSkillBookConfigs` | `SeedItemExtensionConfigsAsync()` | 与技能模板关联 | 技能书与技能模板映射 |
| 道具扩展 | `ItemPetEggConfigs` | `SeedItemExtensionConfigsAsync()` | 与灵宠模板关联 | 宠物蛋与灵宠模板映射 |
| 道具扩展 | `ItemPillConfigs` | `SeedItemExtensionConfigsAsync()` | 与丹药配置关联 | 丹药效果参数 |

统一快照文件：[BalanceCatalog.cs](XXX.Infrastructure/SeedData/BalanceCatalog.cs:28)。

### 3.2 成长、任务和奖励

| 数据域 | 数据表 | 来源/入口 | 版本 |
| --- | --- | --- | --- |
| 玩家等级 | `PlayerLevelConfigs` | `LevelConfig.GetLegacyEntries()` → `SeedPlayerLevelConfigsAsync()` | `player-level-config-v1-20260403` |
| 境界等级 | `RealmLevelConfigs` | `RealmLevelCatalog.GetLegacyEntries()` → `SeedRealmLevelConfigsAsync()` | `realm-level-config-v1-20260403` |
| 属性点 | `AttributePointConfigs` | `BuildAttributePointConfigEntries()` → `SeedAttributePointConfigsAsync()` | `attribute-point-config-v1-20260403` |
| 玩家初始资源 | `PlayerInitialResourceConfigs` | `SeedPlayerInitialResourceConfigAsync()` | `player-initial-resource-config-v1-20260403` |
| 主线/日常/周常任务 | `quest_config` | `QuestSeedCatalog.BuildDefaultQuestEntities()` → `SeedQuestsAsync()` | `quest-built-in-v1-20260403` |
| 成就 | `achievement_config` | `AchievementSeedCatalog.BuildDefaultAchievementEntities()` → `SeedAchievementsAsync()` | `achievement-built-in-v1-20260403` |
| 连续签到 | `CheckInRewardConfigs` | `ActivityRewardCatalog.BuildCheckInRewards()` → `SeedCheckInRewardConfigsAsync()` | `checkin-balance-v2-20260331` |
| 兑换码 | `RedeemCodeConfigs` | `ActivityRewardCatalog.BuildRedeemCodes()` → `SeedRedeemCodeConfigsAsync()` | `redeem-balance-v1-20260323` |
| 新手礼包 | `StarterPackageConfigs`、`StarterPackageGrantItems`、`StarterPackageGrantSkills` | `BuildDefaultStarterPackage*()` → `SeedStarterPackagesAsync()` | `starter-package-v1-20260402` |

任务和成就会先从数据库读取地图、怪物、技能和副本上下文，再生成默认配置，因此它们依赖模板种子先执行。

### 3.3 商店、排行榜和规则配置

| 数据域 | 数据表 | 来源/入口 | 版本 |
| --- | --- | --- | --- |
| 商店 | `shop_config`、`shop_item` | `BuildDefaultShopConfigs()`、`BuildExpectedDefaultShopItems()` → `SeedShopsAsync()` | `shop-built-in-v1-20260403` |
| 排行榜 | `ranking_config`、`ranking_reward` | `BuildDefaultRankingSeeds()`、`BuildDefaultRankingRewards()` → `SeedRankingsAsync()` | `ranking-built-in-v1-20260403` |
| 五行聚灵阵 | `FiveElementLevelConfigs`、`FiveElementBranchUpgradeConfigs`、`FiveElementBranchRuleRanges` | `SeedFiveElementRulesAsync()`、`SeedFiveElementBranchRuleRangesAsync()` | `five-element-rule-v1-20260403` |
| 灵田规则 | `SpiritFieldSystemConfigs`、`SpiritFieldSpeedUpItemConfigs` | `SeedSpiritFieldRulesAsync()` | `spirit-field-rule-v1-20260403` |
| 灵田作物 | `CropTemplates` | `BuildSpiritFieldCropTemplates()` → `SeedSpiritFieldAsync()` | `crop-template-built-in-v1-20260403` |
| 炼丹职业 | `AlchemyProfessionLevelConfigs`、`AlchemyProfessionRuleConfigs` | `SeedAlchemyProfessionRulesAsync()` | `alchemy-profession-rule-v1-20260403` |
| 炼丹配方 | `AlchemyRecipes` | `SeedAlchemyRecipesAsync()` | `alchemy-recipe-built-in-v1-20260403` |
| 锻造职业 | `ForgeProfessionLevelConfigs`、`ForgeProfessionRuleConfigs` | `SeedForgeProfessionRulesAsync()` | `forge-profession-rule-v1-20260403` |
| 锻造配方 | `ForgeRecipes` | `BuildBalancedForgeRecipes()` → `SeedForgeRecipesAsync()` | `forge-recipe-built-in-v1-20260403` |
| 元素克制 | `BattleElementRelationConfigs` | `BuildElementRelationRules()` → `SeedElementRelationRulesAsync()` | `element-relation-rule-v2-20260612` |
| 装备洗练 | `EquipmentRerollSystemConfigs`、`EquipmentRerollSlotPoolConfigs`、`EquipmentRerollTierConfigs`、`EquipmentRerollAttributeValueConfigs` | `SeedEquipmentRerollRulesAsync()` | `equipment-reroll-rule-v1-20260609` |

### 3.4 副本实例、收藏和抽奖

| 数据域 | 数据表 | 来源/入口 | 版本 |
| --- | --- | --- | --- |
| 副本实例模板 | `DungeonInstanceTemplates` | `DungeonInstanceSeedData.BuildTemplates()` → `SeedDungeonInstancesAsync()` | `dungeon-instance-template-built-in-v1-20260616` |
| 副本事件 | `DungeonEventConfigs` | `DungeonInstanceSeedData.BuildEventConfigs()` → `SeedDungeonInstancesAsync()` | `dungeon-event-config-built-in-v1-20260616` |
| 副本事件组 | `DungeonEventGroups` | `DungeonInstanceSeedData.BuildEventGroups()` → `SeedDungeonInstancesAsync()` | `dungeon-event-group-built-in-v1-20260616` |
| 文字收藏 | `TextCollectionSeries`、`TextCollectionItems`、`TextCollectionBonuses` | `CollectionLotterySeedData` → `SeedCollectionsAndLotteryAsync()` | `collection-lottery-v1-20260608` |
| 图片收藏 | `ImageCollectionSeries`、`ImageCollectionItems`、`ImageCollectionBonuses` | `CollectionLotterySeedData` → `SeedCollectionsAndLotteryAsync()` | `collection-lottery-v1-20260608` |
| 抽奖池 | `LotteryPools` | `CollectionLotterySeedData.BuildLotteryPools()` | `collection-lottery-v1-20260608` |
| 抽奖奖品 | `LotteryPrizes` | `CollectionLotterySeedData.BuildLotteryPrizes()` | `collection-lottery-v1-20260608` |

专用数据文件：

- [DungeonInstanceSeedData.cs](XXX.Infrastructure/SeedData/DungeonInstanceSeedData.cs)
- [CollectionLotterySeedData.cs](XXX.Infrastructure/SeedData/CollectionLotterySeedData.cs)

### 3.5 宗门数据

宗门数据不经过 `ISeedDataService`，而是在 [DbContext.cs](XXX.Infrastructure/Data/DbContext.cs:2628) 中直接调用 [SectSeedData.cs](XXX.Infrastructure/SeedData/SectSeedData.cs:14)。

| 数据域 | 数据表 | 入口 | 备注 |
| --- | --- | --- | --- |
| 宗门模板 | `sect_templates` | `SeedSectTemplates()` | 10 个宗门基础信息 |
| 心法模板 | `heart_sutra_templates` | `SeedHeartSutraTemplates()` | 10 套心法，每套 10 层 |
| 宗门宝库 | `sect_shop_configs` | `SeedSectShopConfig()` | 默认宗门商店 |
| 宗门赛程 | `sect_tournament_schedules` | `SeedSectTournamentSchedule()` | 默认 20:00、中国标准时间、60 分钟 |
| 宗门祝福 | `sect_blessing_configs` | `SeedSectBlessingConfigs()` | 6 个祝福配置 |
| 宗门任务 | `quest_config` | `SeedSectQuestConfigs()` | 6 个宗门日/周常任务，使用 `built-in:sect-quest:*` |

宗门种子使用原生 SQL 的 `INSERT OR IGNORE`/`INSERT OR REPLACE`，其幂等策略与 `SeedDataService` 的“按内置身份更新”策略不同，后续适合统一到同一个种子服务中。

## 4. 前端模拟数据

文件：[mockData.js](../XXXV/xiuxian-game/src/data/mockData.js)

该文件是前端开发 mock，不会写入后端 SQLite，也不会被 `SeedDataService` 读取。当前导出数据包括：

| 导出 | 内容 |
| --- | --- |
| `playerData` | 玩家、境界、资源、属性和战斗力 |
| `skillsData` | 已装备技能和技能库 |
| `equipmentData` | 装备 |
| `mapsData` | 地图 |
| `monstersData` | 怪物 |
| `shopItemsData` | 商店商品 |
| `petsData` | 灵宠 |
| `rankingsData` | 排行榜 |
| `recipesData` | 配方 |
| `blueprintsData` | 图纸 |
| `changelogsData` | 更新日志 |
| `redeemCodesData` | 兑换码 |
| `realmsData` | 境界 |
| `bodyRealmsData` | 肉身境界 |
| `elementsData` | 元素 |

README 已明确将其定位为接口响应格式参考/模拟数据，不应与后端数据库种子混用。

## 5. 当前种子策略

- 启动时先做 bootstrap 表检查；`SeedStartup:SkipBootstrapCheck=true` 时完全跳过种子初始化。
- 数据库已存在完整 bootstrap 数据时，仍会执行 `BalanceCatalogSyncService` 的缺失模板补齐，以及默认开启的排行榜同步。
- 大多数内置配置通过 `SeedKey`、`IsBuiltIn` 和 `BuiltInVersion` 标识，可在不覆盖后台自定义数据的前提下更新内置项。
- `SyncEquipmentRerollRules`、`SyncElementRelationRules` 默认关闭，可通过 `appsettings.json` 控制每次启动是否同步。
- 默认管理员只在 `Development` 环境、管理员表为空且 `Admin:Enabled=true` 时创建。
- 运行时服务应从数据库缓存读取模板和规则；`SkillData`、`BuffDataTemplates` 等旧内存目录只在种子转换阶段短暂作为默认来源。

## 6. 整理发现与后续建议

### 已确认的遗留点

1. `SeedDataService.LoadSeedDataAsync()` 和 `SeedDataAsync()` 当前没有调用方，且项目中没有被其读取的 JSON 种子文件。
2. `SkillData`、`BuffDataTemplates`、`LevelConfig`、`RealmLevelCatalog`、`DungeonCatalog` 仍保留 legacy/default 数据；其中部分是数据库种子的来源，部分是运行时 fallback 目录，职责容易混淆。
3. 宗门种子绕过 `ISeedDataService`，直接由 `DbContext.InitDatabase()` 执行，且没有统一使用 `BuiltInVersion` 字段。
4. `SeedDataService` 仍然是一个超过 4,000 行的领域混合类，包含成长、活动、商店、规则、配方、副本和管理员等多个职责。
5. `appsettings.json` 中包含默认管理员口令；即使只用于开发，也建议改为环境变量或 user-secrets，避免把可用口令提交到仓库。

### 建议的下一步重构顺序

1. 先删除或隔离未使用的 JSON 加载器，避免新增“看似可用、实际不生效”的 JSON 文件。
2. 把 `SeedDataService` 按领域拆成 `GrowthSeedService`、`TemplateSeedService`、`ContentSeedService`、`RuleSeedService`、`DungeonSeedService` 和 `AdminSeedService`，由一个轻量 orchestrator 保持当前执行顺序。
3. 将 `SectSeedData` 纳入统一 orchestrator，并为宗门表补齐 `SeedKey`/`BuiltInVersion` 的一致标识。
4. 让前端 mock 从后端导出的契约样例生成或校验，减少 mock 与真实模板数据逐渐偏离。

