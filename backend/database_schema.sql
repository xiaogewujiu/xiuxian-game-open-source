-- SQLite database schema only.
-- Includes human-readable SQL comments sourced from C# entity documentation and safe fallbacks.
-- No INSERT, UPDATE, DELETE, or seed data is included.
-- SQLite has no native COMMENT ON syntax; comments are preserved as -- documentation lines.
-- Generated: 2026-09-01

PRAGMA foreign_keys = OFF;

-- ==================== TABLES ====================
-- 表：achievement_config
-- 成就配置实体 存储成就的配置信息，支持数据库动态配置
CREATE TABLE achievement_config (
-- 字段：AchievementId — 成就唯一标识符
    AchievementId TEXT PRIMARY KEY,
-- 字段：SeedKey — 内置种子键。
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 是否为系统内置成就。
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置版本号。
    BuiltInVersion TEXT NULL,
-- 字段：AchievementName — 成就名称
    AchievementName TEXT NOT NULL,
-- 字段：AchievementType — 成就类型：0-等级，1-战斗，2-装备，3-收集，4-社交
    AchievementType INTEGER NOT NULL DEFAULT 0,
-- 字段：Difficulty — 难度等级：1-简单，2-普通，3-困难，4-极难
    Difficulty INTEGER NOT NULL DEFAULT 0,
-- 字段：Description — 成就描述
    Description TEXT NOT NULL,
-- 字段：Category — 成就分类名称
    Category TEXT NOT NULL,
-- 字段：Points — 完成成就获得的点数
    Points INTEGER NOT NULL DEFAULT 0,
-- 字段：IsHidden — 是否为隐藏成就
    IsHidden INTEGER NOT NULL DEFAULT 0,
-- 字段：PreAchievementIds — 前置成就ID列表，逗号分隔
    PreAchievementIds TEXT NULL,
-- 字段：RewardGold — 奖励金币数量
    RewardGold INTEGER NOT NULL DEFAULT 0,
-- 字段：RewardSpiritStone — 奖励灵石数量
    RewardSpiritStone INTEGER NOT NULL DEFAULT 0,
-- 字段：RewardExp — 奖励经验值
    RewardExp INTEGER NOT NULL DEFAULT 0,
-- 字段：RewardTitle — 奖励的称号名称
    RewardTitle TEXT NULL,
-- 字段：RewardItemsJson — 奖励道具JSON。
    RewardItemsJson TEXT NULL,
-- 字段：RewardEquipmentIds — 奖励装备ID列表，逗号分隔
    RewardEquipmentIds TEXT NULL,
-- 字段：RequirementType — 要求类型：0-等级，1-击杀，2-收集，3-胜利次数等
    RequirementType INTEGER NOT NULL DEFAULT 0,
-- 字段：RequirementTargetValue — 要求达到的目标值
    RequirementTargetValue INTEGER NOT NULL DEFAULT 0,
-- 字段：RequirementDescription — 要求的描述文本
    RequirementDescription TEXT NULL,
-- 字段：RequirementsJson — 多条件JSON。
    RequirementsJson TEXT NULL,
-- 字段：SortOrder — 排序顺序，数值越小越靠前
    SortOrder INTEGER NOT NULL DEFAULT 0,
-- 字段：IsEnabled — 是否启用该成就
    IsEnabled INTEGER NOT NULL DEFAULT 1,
-- 字段：LastUpdateTime — 最后更新时间。
    LastUpdateTime TEXT NOT NULL
);
-- 表：achievement_metric_counter
-- 成就统计计数器实体。
CREATE TABLE achievement_metric_counter (
-- 字段：Id — 标识字段
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
-- 字段：PlayerId — 标识字段
    PlayerId TEXT NOT NULL,
-- 字段：RequirementType — 类型或状态
    RequirementType INTEGER NOT NULL DEFAULT 0,
-- 字段：TargetId — 标识字段
    TargetId TEXT NULL,
-- 字段：CurrentValue — current value 字段
    CurrentValue INTEGER NOT NULL DEFAULT 0,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL
);
-- 表：achievement_progress
-- 成就进度实体 记录玩家的成就完成进度
CREATE TABLE achievement_progress (
-- 字段：GID — 记录唯一标识符
    GID TEXT PRIMARY KEY,
-- 字段：PlayerId — 玩家ID
    PlayerId TEXT NOT NULL,
-- 字段：AchievementId — 成就ID
    AchievementId TEXT NOT NULL,
-- 字段：CurrentProgress — 当前进度值
    CurrentProgress INTEGER NOT NULL DEFAULT 0,
-- 字段：TargetProgress — 目标进度值
    TargetProgress INTEGER NOT NULL DEFAULT 0,
-- 字段：RequirementProgressJson — 多条件进度JSON。
    RequirementProgressJson TEXT NULL,
-- 字段：Status — 成就状态：0-未开始，1-进行中，2-已完成，3-已领取
    Status INTEGER NOT NULL DEFAULT 0,
-- 字段：AcceptTime — 开始追踪成就的时间
    AcceptTime TEXT NULL,
-- 字段：CompleteTime — 完成成就的时间
    CompleteTime TEXT NULL,
-- 字段：ClaimTime — 领取奖励的时间
    ClaimTime TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL
);
-- 表：AdminAuditLogs
-- 管理后台审计日志实体。 用于记录后台接口写操作，并保存请求、响应以及资源前后快照。
CREATE TABLE AdminAuditLogs (
-- 字段：LogId — 日志编号。
    LogId TEXT PRIMARY KEY,
-- 字段：OperatorId — 操作人编号。
    OperatorId TEXT NULL,
-- 字段：OperatorName — 操作人名称。
    OperatorName TEXT NULL,
-- 字段：OperatorRole — 操作人角色。
    OperatorRole TEXT NULL,
-- 字段：HttpMethod — HTTP 方法。
    HttpMethod TEXT NOT NULL,
-- 字段：Path — 请求路径。
    Path TEXT NOT NULL,
-- 字段：ResourceKey — 资源键。 例如 maps、monsters、players。
    ResourceKey TEXT NULL,
-- 字段：TargetId — 目标编号。
    TargetId TEXT NULL,
-- 字段：RequestJson — 请求体快照。 会对密码、令牌等敏感字段做脱敏。
    RequestJson TEXT NULL,
-- 字段：ResponseJson — 响应体快照。 会对密码、令牌等敏感字段做脱敏。
    ResponseJson TEXT NULL,
-- 字段：BeforeJson — 操作前资源快照。
    BeforeJson TEXT NULL,
-- 字段：AfterJson — 操作后资源快照。
    AfterJson TEXT NULL,
-- 字段：DiffJson — 前后差异快照。 当前记录变更字段路径以及前后值，便于快速审计。
    DiffJson TEXT NULL,
-- 字段：StatusCode — 响应状态码。
    StatusCode INTEGER NOT NULL DEFAULT 200,
-- 字段：Success — 是否成功。
    Success INTEGER NOT NULL DEFAULT 1,
-- 字段：ErrorMessage — 错误信息。
    ErrorMessage TEXT NULL,
-- 字段：IpAddress — 客户端 IP。
    IpAddress TEXT NULL,
    CreateTime TEXT NOT NULL
);
-- 表：AdminUsers
-- 管理员账号实体。 与玩家账号分离，专门用于管理后台登录与权限控制。
CREATE TABLE AdminUsers (
-- 字段：AdminId — 管理员唯一编号。
    AdminId TEXT PRIMARY KEY,
-- 字段：Account — 登录账号。
    Account TEXT NOT NULL,
-- 字段：DisplayName — 后台显示名。
    DisplayName TEXT NOT NULL,
-- 字段：PasswordHash — 密码哈希。
    PasswordHash TEXT NOT NULL,
-- 字段：Role — 管理员角色。
    Role TEXT NOT NULL,
-- 字段：IsActive — 是否启用。
    IsActive INTEGER NOT NULL DEFAULT 1,
-- 字段：IsDeleted — 是否软删除。
    IsDeleted INTEGER NOT NULL DEFAULT 0,
    CreateTime TEXT NOT NULL,
-- 字段：LastLoginTime — 最后登录时间。
    LastLoginTime TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间。
    LastUpdateTime TEXT NOT NULL
);
-- 表：AlchemyProfessionLevelConfigs
-- 炼丹职业等级经验配置。
CREATE TABLE AlchemyProfessionLevelConfigs (
-- 字段：Level — 等级或层级
    Level INTEGER PRIMARY KEY,
-- 字段：NextLevelExp — next level exp 字段
    NextLevelExp INTEGER NOT NULL DEFAULT 80,
-- 字段：IsEnabled — 状态标记
    IsEnabled INTEGER NOT NULL DEFAULT 1,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL
);
-- 表：AlchemyProfessionRuleConfigs
-- 炼丹职业通用规则配置。
CREATE TABLE AlchemyProfessionRuleConfigs (
-- 字段：ConfigId — 标识字段
    ConfigId TEXT PRIMARY KEY,
-- 字段：SuccessBonusPerOverLevel — 等级或层级
    SuccessBonusPerOverLevel INTEGER NOT NULL DEFAULT 1,
-- 字段：MaxSuccessBonus — max success bonus 字段
    MaxSuccessBonus INTEGER NOT NULL DEFAULT 15,
-- 字段：SuccessExpBase — success exp base 字段
    SuccessExpBase INTEGER NOT NULL DEFAULT 12,
-- 字段：SuccessExpPerRequiredLevel — 等级或层级
    SuccessExpPerRequiredLevel INTEGER NOT NULL DEFAULT 4,
-- 字段：FailureExpBase — failure exp base 字段
    FailureExpBase INTEGER NOT NULL DEFAULT 6,
-- 字段：FailureExpPerRequiredLevel — 等级或层级
    FailureExpPerRequiredLevel INTEGER NOT NULL DEFAULT 2,
-- 字段：IsEnabled — 状态标记
    IsEnabled INTEGER NOT NULL DEFAULT 1,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL
);
-- 表：AlchemyRecipes
-- 丹方实体 对应数据库丹方表
CREATE TABLE AlchemyRecipes (
-- 字段：RecipeId — 丹方ID
    RecipeId TEXT PRIMARY KEY,
-- 字段：PillTemplateId — 丹药模板ID
    PillTemplateId TEXT NOT NULL,
-- 字段：Name — 丹方名称
    Name TEXT NOT NULL,
-- 字段：Description — 丹方描述
    Description TEXT NULL,
-- 字段：RequiredFurnaceLevel — 所需炼丹炉等级
    RequiredFurnaceLevel INTEGER NOT NULL DEFAULT 1,
-- 字段：BaseSuccessRate — 基础成功率（百分比）
    BaseSuccessRate INTEGER NOT NULL DEFAULT 50,
-- 字段：BaseCraftTime — 基础炼制时间（秒）
    BaseCraftTime INTEGER NOT NULL DEFAULT 300,
-- 字段：MaterialsJson — 所需材料JSON
    MaterialsJson TEXT NULL,
-- 字段：UnlockCondition — 解锁条件
    UnlockCondition TEXT NULL,
-- 字段：IsDefaultLearned — 是否默认已学习
    IsDefaultLearned INTEGER NOT NULL DEFAULT 0,
-- 字段：SeedKey — 内置种子键。
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 是否为内置配方。
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置版本号。
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间。
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
);
-- 表：AlchemySystems
-- 炼丹系统数据实体 对应数据库炼丹系统表
CREATE TABLE AlchemySystems (
-- 字段：Id — 主键ID
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
-- 字段：PlayerId — 玩家ID
    PlayerId TEXT NOT NULL,
-- 字段：FurnaceLevel — 炼丹炉等级
    FurnaceLevel INTEGER NOT NULL DEFAULT 1,
-- 字段：AlchemistLevel — 炼丹师等级
    AlchemistLevel INTEGER NOT NULL DEFAULT 1,
-- 字段：AlchemistExp — 炼丹师总经验
    AlchemistExp INTEGER NOT NULL DEFAULT 0,
-- 字段：Proficiency — 炼丹熟练度
    Proficiency INTEGER NOT NULL DEFAULT 0,
-- 字段：LearnedRecipesJson — 已学习丹方ID列表（JSON格式）
    LearnedRecipesJson TEXT NULL,
-- 字段：SuccessRateBonus — 成功率加成（百分比）
    SuccessRateBonus INTEGER NOT NULL DEFAULT 0,
-- 字段：CraftTimeReduction — 炼制时间减少（百分比）
    CraftTimeReduction INTEGER NOT NULL DEFAULT 0,
-- 字段：YieldBonus — 产量加成（百分比）
    YieldBonus INTEGER NOT NULL DEFAULT 0,
-- 字段：TodayCraftCount — 今日炼制次数
    TodayCraftCount INTEGER NOT NULL DEFAULT 0,
-- 字段：DailyResetTime — 今日统计重置时间
    DailyResetTime TEXT NOT NULL,
-- 字段：TotalCraftCount — 累计炼制次数
    TotalCraftCount INTEGER NOT NULL DEFAULT 0,
-- 字段：SuccessCraftCount — 成功炼制次数
    SuccessCraftCount INTEGER NOT NULL DEFAULT 0,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL,
-- 字段：ActiveRecipeId — 当前正在炼制的丹方ID
    ActiveRecipeId TEXT NULL,
-- 字段：ActiveCraftStartedAt — 当前炼制开始时间
    ActiveCraftStartedAt TEXT NULL,
-- 字段：ActiveCraftCompleteAt — 当前炼制完成时间
    ActiveCraftCompleteAt TEXT NULL,
-- 字段：PendingResultJson — 当前炼制待领取结果（JSON）
    PendingResultJson TEXT NULL
);
-- 表：ArenaBattleLogs
-- 竞技场战斗日志实体。
CREATE TABLE ArenaBattleLogs (
-- 字段：Id — 标识字段
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
-- 字段：AttackerId — 挑战方GID。
    AttackerId TEXT NOT NULL,
-- 字段：DefenderId — 被挑战方GID。
    DefenderId TEXT NOT NULL,
-- 字段：AttackerName — 挑战方名称。
    AttackerName TEXT NOT NULL,
-- 字段：DefenderName — 被挑战方名称。
    DefenderName TEXT NOT NULL,
-- 字段：AttackerPointsBefore — 挑战前积分。
    AttackerPointsBefore INTEGER NOT NULL DEFAULT 0,
-- 字段：DefenderPointsBefore — 被挑战前积分。
    DefenderPointsBefore INTEGER NOT NULL DEFAULT 0,
-- 字段：AttackerPointsAfter — 挑战后积分。
    AttackerPointsAfter INTEGER NOT NULL DEFAULT 0,
-- 字段：DefenderPointsAfter — 被挑战后积分。
    DefenderPointsAfter INTEGER NOT NULL DEFAULT 0,
-- 字段：WinnerId — 胜者GID。
    WinnerId TEXT NOT NULL,
-- 字段：BattleLogJson — 战斗回放日志（JSON）。
    BattleLogJson TEXT NULL,
-- 字段：SeasonNumber — 赛季编号。
    SeasonNumber INTEGER NOT NULL DEFAULT 1,
    CreatedAt TEXT NOT NULL
);
-- 表：ArenaPlayers
-- 竞技场玩家实体。
CREATE TABLE ArenaPlayers (
-- 字段：Id — 标识字段
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
-- 字段：PlayerId — 玩家GID。
    PlayerId TEXT NOT NULL,
-- 字段：Points — 竞技场积分。
    Points INTEGER NOT NULL DEFAULT 1000,
-- 字段：Rank — 当前排名。
    Rank INTEGER NOT NULL DEFAULT 0,
-- 字段：Wins — 本赛季胜场。
    Wins INTEGER NOT NULL DEFAULT 0,
-- 字段：Losses — 本赛季负场。
    Losses INTEGER NOT NULL DEFAULT 0,
-- 字段：WinStreak — 当前连胜数。
    WinStreak INTEGER NOT NULL DEFAULT 0,
-- 字段：BestRank — 历史最高排名。
    BestRank INTEGER NOT NULL DEFAULT 0,
-- 字段：DailyBattlesUsed — 今日已用挑战次数。
    DailyBattlesUsed INTEGER NOT NULL DEFAULT 0,
-- 字段：DailyBattlesPurchased — 今日已购买次数。
    DailyBattlesPurchased INTEGER NOT NULL DEFAULT 0,
-- 字段：LastBattleAt — 最后一次战斗时间。
    LastBattleAt TEXT NULL,
-- 字段：SeasonNumber — 所属赛季编号。
    SeasonNumber INTEGER NOT NULL DEFAULT 1,
    CreatedAt TEXT NOT NULL,
-- 字段：UpdatedAt — 更新时间。
    UpdatedAt TEXT NOT NULL
, BannedUntil TEXT NULL, BanReason TEXT NULL);
-- 表：AttributePointConfigs
-- 属性点配置。 一行代表“某属性在某等级区间”的配置快照。
CREATE TABLE AttributePointConfigs (
-- 字段：ConfigId — 标识字段
    ConfigId TEXT PRIMARY KEY,
-- 字段：Key — key 字段
    Key TEXT NOT NULL,
-- 字段：Name — 名称
    Name TEXT NOT NULL,
-- 字段：Profession — profession 字段
    Profession TEXT NOT NULL DEFAULT 'warrior',
-- 字段：AttributeType — 类型或状态
    AttributeType INTEGER NOT NULL,
-- 字段：RatioPerPoint — ratio per point 字段
    RatioPerPoint REAL NOT NULL DEFAULT 1,
    BonusPerPoint INTEGER NOT NULL DEFAULT 1,
    PointsPerBonus INTEGER NOT NULL DEFAULT 1,
-- 字段：LevelStart — level start 字段
    LevelStart INTEGER NOT NULL,
-- 字段：LevelEnd — level end 字段
    LevelEnd INTEGER NOT NULL,
-- 字段：PointsGained — points gained 字段
    PointsGained INTEGER NOT NULL DEFAULT 0,
-- 字段：SortOrder — 排序或优先级
    SortOrder INTEGER NOT NULL DEFAULT 0,
-- 字段：IsEnabled — 状态标记
    IsEnabled INTEGER NOT NULL DEFAULT 1,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL
);
-- 表：BattleElementRelationConfigs
-- 元素克制矩阵规则配置。
CREATE TABLE BattleElementRelationConfigs (
-- 字段：GID — 标识字段
    GID TEXT PRIMARY KEY,
-- 字段：AttackerElement — attacker element 字段
    AttackerElement INTEGER NOT NULL,
-- 字段：DefenderElement — defender element 字段
    DefenderElement INTEGER NOT NULL,
-- 字段：Modifier — modifier 字段
    Modifier REAL NOT NULL DEFAULT 0,
-- 字段：SortOrder — 排序或优先级
    SortOrder INTEGER NOT NULL DEFAULT 0,
-- 字段：IsEnabled — 状态标记
    IsEnabled INTEGER NOT NULL DEFAULT 1,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL
);
-- 表：BuffTemplates
-- Buff 模板落库实体。
CREATE TABLE BuffTemplates (
-- 字段：BuffId — 标识字段
    BuffId TEXT PRIMARY KEY,
-- 字段：BuffCatalog — Buff 目录。legacy 为历史 Buff，current 为当前版本 Buff。
    BuffCatalog TEXT NOT NULL DEFAULT 'legacy',
-- 字段：Name — 名称
    Name TEXT NOT NULL,
-- 字段：Description — 描述信息
    Description TEXT NOT NULL,
-- 字段：Duration — duration 字段
    Duration INTEGER NOT NULL DEFAULT 0,
-- 字段：MaxStack — max stack 字段
    MaxStack INTEGER NOT NULL DEFAULT 1,
-- 字段：StackRule — stack rule 字段
    StackRule INTEGER NOT NULL DEFAULT 0,
-- 字段：EffectsJson — JSON 序列化配置或扩展数据
    EffectsJson TEXT NULL,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
);
-- 表：ChatMessages
-- 中文注释： 聊天消息持久化实体。 当前项目原本只有聊天 DTO，没有真实数据库表，导致世界聊天刷新后消息全部消失。 这里单独落一张最小可用表，只存频道、发送者、内容和时间， 先把“玩家发言可落库、切页后还能看到历史记录”这条真实链路跑通。
CREATE TABLE ChatMessages (
-- 字段：MessageId — 消息唯一 ID。
    MessageId TEXT PRIMARY KEY,
-- 字段：ChannelType — 频道类型。 world / sect / system
    ChannelType TEXT NOT NULL,
-- 字段：SenderId — 发送者玩家 ID。
    SenderId TEXT NOT NULL,
-- 字段：SenderName — 发送者名称。
    SenderName TEXT NOT NULL,
-- 字段：Content — 消息正文。
    Content TEXT NOT NULL,
-- 字段：SendTime — 发送时间。
    SendTime TEXT NOT NULL
);
-- 表：CheckInRewardConfigs
-- 连续签到奖励配置实体。
CREATE TABLE CheckInRewardConfigs (
-- 字段：ContinuousDay — 连续签到天数。
    ContinuousDay INTEGER PRIMARY KEY,
-- 字段：SeedKey — 内置种子键。
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 是否为系统内置配置。
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本。
    BuiltInVersion TEXT NULL,
-- 字段：ConfigVersion — 当前奖励配置版本号。
    ConfigVersion TEXT NOT NULL,
-- 字段：IsMilestone — 是否为里程碑奖励。
    IsMilestone INTEGER NOT NULL DEFAULT 0,
-- 字段：RewardJson — 奖励内容的 JSON 序列化结果。
    RewardJson TEXT NOT NULL,
-- 字段：Description — 奖励说明。
    Description TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间。
    LastUpdateTime TEXT NOT NULL
);
-- 表：CropTemplates
-- 作物模板实体 对应数据库作物模板表
CREATE TABLE CropTemplates (
-- 字段：TemplateId — 模板ID
    TemplateId TEXT PRIMARY KEY,
-- 字段：Name — 作物名称
    Name TEXT NOT NULL,
-- 字段：Description — 作物描述
    Description TEXT NULL,
-- 字段：Type — 作物类型
    Type INTEGER NOT NULL DEFAULT 1,
-- 字段：GrowthCycle — 生长周期（秒）
    GrowthCycle INTEGER NOT NULL DEFAULT 3600,
-- 字段：Yield — 产量
    Yield INTEGER NOT NULL DEFAULT 1,
-- 字段：SeedId — 种子ID
    SeedId TEXT NOT NULL,
-- 字段：SeedAmount — 种子数量
    SeedAmount INTEGER NOT NULL DEFAULT 1,
-- 字段：OutputItemId — 产出物品ID
    OutputItemId TEXT NOT NULL,
-- 字段：OutputAmount — 产出数量
    OutputAmount INTEGER NOT NULL DEFAULT 1,
-- 字段：MinQuality — 最小品质
    MinQuality INTEGER NOT NULL DEFAULT 1,
-- 字段：MaxQuality — 最大品质
    MaxQuality INTEGER NOT NULL DEFAULT 1,
-- 字段：UnlockLevel — 解锁等级
    UnlockLevel INTEGER NOT NULL DEFAULT 1,
-- 字段：SeedKey — 内置种子键。
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 是否为内置模板。
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置版本号。
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间。
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
);
-- 表：dungeon_daily_record
-- 玩家副本每日挑战记录 用于统计某玩家在某副本当天已挑战次数
CREATE TABLE dungeon_daily_record (
-- 字段：GID — 记录唯一标识
    GID TEXT PRIMARY KEY,
-- 字段：PlayerId — 玩家ID
    PlayerId TEXT NOT NULL,
-- 字段：DungeonId — 副本ID
    DungeonId TEXT NOT NULL,
-- 字段：ChallengeCount — 当天已挑战次数
    ChallengeCount INTEGER NOT NULL DEFAULT 0,
-- 字段：RecordDate — 记录日期（按天）
    RecordDate TEXT NOT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL
);
-- 表：DungeonEventConfigs
-- 秘境事件配置。 每条记录代表一个具体的秘境随机事件，可通过后台自由配置。
CREATE TABLE DungeonEventConfigs (
-- 字段：Id — 标识字段
    Id TEXT PRIMARY KEY,
-- 字段：Name — 名称
    Name TEXT NOT NULL,
-- 字段：Description — 描述信息
    Description TEXT NOT NULL DEFAULT '',
-- 字段：DungeonId — 关联的秘境模板 ID。"*" 表示所有秘境通用。
    DungeonId TEXT NOT NULL DEFAULT '*',
-- 字段：EventType — 事件大类（DungeonEventType 枚举值）。
    EventType INTEGER NOT NULL,
-- 字段：Weight — 在该类型事件池中的抽取权重。
    Weight INTEGER NOT NULL DEFAULT 10,
-- 字段：Enabled — 是否启用。
    Enabled INTEGER NOT NULL DEFAULT 1,
-- 字段：EventDataJson — 事件参数 JSON（怪物 ID、恢复百分比、buff 效果等）。
    EventDataJson TEXT NULL,
-- 字段：DeathKeep — 死亡时是否保留收益。仅收益类事件有效。
    DeathKeep INTEGER NULL,
-- 字段：SeedKey — 种子数据标识。
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 是否为内置种子数据。
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置数据版本。
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间。
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
);
-- 表：DungeonEventGroups
-- 秘境事件组。
CREATE TABLE DungeonEventGroups (
-- 字段：Id — 标识字段
    Id TEXT PRIMARY KEY,
-- 字段：Name — 名称
    Name TEXT NOT NULL,
-- 字段：Description — 描述信息
    Description TEXT NULL,
-- 字段：GroupItemsJson — 组内事件列表 JSON（EventGroupItem[]）。
    GroupItemsJson TEXT NULL,
-- 字段：SeedKey — 种子数据标识。
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 是否为内置种子数据。
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置数据版本。
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间。
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
);
-- 表：DungeonInstanceDailyRecords
-- 秘境每日进入记录。
CREATE TABLE DungeonInstanceDailyRecords (
-- 字段：Id — 标识字段
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
-- 字段：PlayerId — 标识字段
    PlayerId TEXT NOT NULL,
-- 字段：DungeonId — 秘境模板 ID。
    DungeonId TEXT NOT NULL,
-- 字段：EnterDate — 进入日期（自然日）。
    EnterDate TEXT NOT NULL,
-- 字段：EnterCount — 当日已进入次数。
    EnterCount INTEGER NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL DEFAULT '2000-01-01 00:00:00',
-- 字段：UpdatedAt — 时间或日期字段
    UpdatedAt TEXT NOT NULL DEFAULT '2000-01-01 00:00:00',
-- 字段：IsDeleted — 状态标记
    IsDeleted INTEGER NOT NULL DEFAULT 0
);
-- 表：DungeonInstances
-- 秘境运行实例。
CREATE TABLE DungeonInstances (
-- 字段：Id — 标识字段
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
-- 字段：PlayerId — 标识字段
    PlayerId TEXT NOT NULL,
-- 字段：DungeonId — 关联的秘境模板 ID。
    DungeonId TEXT NOT NULL,
-- 字段：Status — 实例状态。
    Status INTEGER NOT NULL DEFAULT 0,
-- 字段：EnterTime — 进入时间。
    EnterTime TEXT NOT NULL,
-- 字段：LastTickTime — 上次 tick 时间。
    LastTickTime TEXT NULL,
-- 字段：NextTickTime — 下次 tick 时间。
    NextTickTime TEXT NULL,
-- 字段：SettleTime — 结算时间。
    SettleTime TEXT NULL,
-- 字段：SettleReason — 结算原因。
    SettleReason INTEGER NULL,
-- 字段：SnapshotJson — 角色快照 JSON。
    SnapshotJson TEXT NULL,
-- 字段：CurrentHp — 当前生命值。
    CurrentHp INTEGER NOT NULL DEFAULT 0,
-- 字段：CurrentMp — 当前法力值。
    CurrentMp INTEGER NOT NULL DEFAULT 0,
-- 字段：MaxHp — 最大生命值。
    MaxHp INTEGER NOT NULL DEFAULT 0,
-- 字段：MaxMp — 最大法力值。
    MaxMp INTEGER NOT NULL DEFAULT 0,
-- 字段：ActiveBuffsJson — 秘境内临时 buff 列表 JSON。
    ActiveBuffsJson TEXT NULL,
-- 字段：ExploreLogJson — 探索日志 JSON。
    ExploreLogJson TEXT NULL,
-- 字段：PartyId — 关联的秘境临时队伍 ID。null 表示未组队。
    PartyId TEXT NULL,
-- 字段：IsPartyLeader — 是否为秘境临时队伍队长。
    IsPartyLeader INTEGER NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL DEFAULT '2000-01-01 00:00:00',
-- 字段：UpdatedAt — 时间或日期字段
    UpdatedAt TEXT NOT NULL DEFAULT '2000-01-01 00:00:00',
-- 字段：IsDeleted — 状态标记
    IsDeleted INTEGER NOT NULL DEFAULT 0
);
-- 表：DungeonInstanceTemplates
-- 秘境实例配置模板。
CREATE TABLE DungeonInstanceTemplates (
-- 字段：Id — 标识字段
    Id TEXT PRIMARY KEY,
-- 字段：Name — 名称
    Name TEXT NOT NULL,
-- 字段：Description — 描述信息
    Description TEXT NOT NULL DEFAULT '',
-- 字段：Enabled — 是否启用。
    Enabled INTEGER NOT NULL DEFAULT 1,
-- 字段：RecommendedLevel — 推荐等级。
    RecommendedLevel INTEGER NOT NULL DEFAULT 1,
-- 字段：DailyEnterLimit — 每日进入次数限制。
    DailyEnterLimit INTEGER NOT NULL DEFAULT 1,
-- 字段：TickIntervalSeconds — tick 间隔秒数。
    TickIntervalSeconds INTEGER NOT NULL DEFAULT 30,
-- 字段：OpenScheduleJson — 开放时间配置 JSON。
    OpenScheduleJson TEXT NULL,
-- 字段：EntryCostsJson — 进入消耗配置 JSON。
    EntryCostsJson TEXT NULL,
-- 字段：EventGroupId — 关联的事件组 ID。
    EventGroupId TEXT NULL,
-- 字段：AutoMedicineConfigJson — 自动用药配置 JSON。
    AutoMedicineConfigJson TEXT NULL,
-- 字段：EncounterConfigJson — 玩家偶遇配置 JSON（好感度概率阈值 + PvP 掠夺规则）。
    EncounterConfigJson TEXT NULL,
-- 字段：SeedKey — 种子数据标识。
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 是否为内置种子数据。
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置数据版本。
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间。
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
);
-- 表：DungeonParties
-- 秘境临时队伍。
CREATE TABLE DungeonParties (
-- 字段：PartyId — 队伍 ID。
    PartyId TEXT PRIMARY KEY,
-- 字段：DungeonId — 所在秘境模板 ID。
    DungeonId TEXT NOT NULL,
-- 字段：LeaderPlayerId — 队长玩家 ID。
    LeaderPlayerId TEXT NOT NULL,
-- 字段：MemberJson — 成员列表 JSON。
    MemberJson TEXT NOT NULL DEFAULT '[]',
    CreateTime TEXT NOT NULL,
-- 字段：LastUpdateTime — 最后更新时间。
    LastUpdateTime TEXT NOT NULL
);
-- 表：DungeonRewardPools
-- 秘境临时收益池。
CREATE TABLE DungeonRewardPools (
-- 字段：Id — 标识字段
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
-- 字段：PlayerId — 标识字段
    PlayerId TEXT NOT NULL,
-- 字段：InstanceId — 关联的秘境实例 ID。
    InstanceId TEXT NOT NULL,
-- 字段：RewardType — 奖励类型（Gold/Exp/Item/Equipment/Collection）。
    RewardType TEXT NOT NULL,
-- 字段：RewardId — 奖励 ID（道具 ID / 装备模板 ID / 空）。
    RewardId TEXT NULL,
-- 字段：Quantity — 数量。
    Quantity INTEGER NOT NULL DEFAULT 0,
-- 字段：Source — 来源（Battle/Resource/Treasure/Adventure/PlayerEncounter）。
    Source TEXT NOT NULL,
-- 字段：DeathKeep — 死亡时是否保留。
    DeathKeep INTEGER NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL DEFAULT '2000-01-01 00:00:00',
-- 字段：UpdatedAt — 时间或日期字段
    UpdatedAt TEXT NOT NULL DEFAULT '2000-01-01 00:00:00',
-- 字段：IsDeleted — 状态标记
    IsDeleted INTEGER NOT NULL DEFAULT 0
);
-- 表：DungeonTemplates
-- 副本模板实体。
CREATE TABLE DungeonTemplates (
-- 字段：DungeonId — 标识字段
    DungeonId TEXT PRIMARY KEY,
-- 字段：Name — 名称
    Name TEXT NOT NULL,
-- 字段：Description — 描述信息
    Description TEXT NOT NULL,
-- 字段：RecommendedLevel — 等级或层级
    RecommendedLevel INTEGER NOT NULL DEFAULT 1,
-- 字段：DailyLimit — daily limit 字段
    DailyLimit INTEGER NOT NULL DEFAULT 1,
-- 字段：NormalMapId — 标识字段
    NormalMapId TEXT NOT NULL,
-- 字段：FubenMapId — 标识字段
    FubenMapId TEXT NOT NULL,
-- 字段：RequiredTeamSize — required team size 字段
    RequiredTeamSize INTEGER NOT NULL DEFAULT 1,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
);
-- 表：EquipmentInstances
-- 装备实例实体
CREATE TABLE EquipmentInstances (
-- 字段：InstanceId — 标识字段
    InstanceId TEXT PRIMARY KEY,
-- 字段：PlayerId — 标识字段
    PlayerId TEXT NOT NULL,
-- 字段：TemplateId — 标识字段
    TemplateId TEXT NOT NULL,
-- 字段：Name — 名称
    Name TEXT NOT NULL,
-- 字段：Slot — slot 字段
    Slot INTEGER NOT NULL,
-- 字段：Quality — quality 字段
    Quality INTEGER NOT NULL DEFAULT 1,
-- 字段：EnhanceLevel — 等级或层级
    EnhanceLevel INTEGER NOT NULL DEFAULT 0,
-- 字段：IsEquipped — 状态标记
    IsEquipped INTEGER NOT NULL DEFAULT 0,
-- 字段：IsBound — 状态标记
    IsBound INTEGER NOT NULL DEFAULT 0,
-- 字段：BasePhysicalAttack — base physical attack 字段
    BasePhysicalAttack INTEGER NOT NULL DEFAULT 0,
-- 字段：BaseMagicAttack — base magic attack 字段
    BaseMagicAttack INTEGER NOT NULL DEFAULT 0,
-- 字段：BasePhysicalDefense — base physical defense 字段
    BasePhysicalDefense INTEGER NOT NULL DEFAULT 0,
-- 字段：BaseMagicDefense — base magic defense 字段
    BaseMagicDefense INTEGER NOT NULL DEFAULT 0,
-- 字段：BaseHP — base hp 字段
    BaseHP INTEGER NOT NULL DEFAULT 0,
-- 字段：BaseMP — base mp 字段
    BaseMP INTEGER NOT NULL DEFAULT 0,
-- 字段：BonusStatsJson — JSON 序列化配置或扩展数据
    BonusStatsJson TEXT NULL,
-- 字段：AcquiredTime — 时间或日期字段
    AcquiredTime TEXT NOT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL
, RerollStatsJson TEXT NULL, RerollCandidateJson TEXT NULL, RerollCount INTEGER NOT NULL DEFAULT 0, RerollCandidateCreatedAt TEXT NULL, GemSlotsJson TEXT NULL);
-- 表：EquipmentRerollAttributeValueConfigs
-- 装备洗练属性值配置 —— 按 (属性类型, 品阶) 定义数值区间。
CREATE TABLE EquipmentRerollAttributeValueConfigs (
-- 字段：GID — 标识字段
    GID INTEGER PRIMARY KEY AUTOINCREMENT,
-- 字段：AttributeType — 类型或状态
    AttributeType INTEGER NOT NULL,
-- 字段：Tier — tier 字段
    Tier INTEGER NOT NULL,
-- 字段：MinValue — min value 字段
    MinValue TEXT NOT NULL DEFAULT '0',
-- 字段：MaxValue — max value 字段
    MaxValue TEXT NOT NULL DEFAULT '0',
-- 字段：IsPercentage — 状态标记
    IsPercentage INTEGER NOT NULL DEFAULT 0,
-- 字段：SortOrder — 排序或优先级
    SortOrder INTEGER NOT NULL DEFAULT 0,
-- 字段：IsEnabled — 状态标记
    IsEnabled INTEGER NOT NULL DEFAULT 1,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
);
-- 表：EquipmentRerollSlotPoolConfigs
-- 装备洗练槽位词条池配置。
CREATE TABLE EquipmentRerollSlotPoolConfigs (
-- 字段：GID — 标识字段
    GID INTEGER PRIMARY KEY AUTOINCREMENT,
-- 字段：Slot — slot 字段
    Slot INTEGER NOT NULL,
-- 字段：AttributeType — 类型或状态
    AttributeType INTEGER NOT NULL,
-- 字段：Tier — tier 字段
    Tier INTEGER NOT NULL DEFAULT 1,
-- 字段：MaxDuplicateCount — 数量字段
    MaxDuplicateCount INTEGER NOT NULL DEFAULT 1,
-- 字段：SortOrder — 排序或优先级
    SortOrder INTEGER NOT NULL DEFAULT 0,
-- 字段：IsEnabled — 状态标记
    IsEnabled INTEGER NOT NULL DEFAULT 1,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
);
-- 表：EquipmentRerollSystemConfigs
-- 装备洗练系统配置。
CREATE TABLE EquipmentRerollSystemConfigs (
-- 字段：ConfigId — 标识字段
    ConfigId TEXT PRIMARY KEY,
-- 字段：IsEnabled — 状态标记
    IsEnabled INTEGER NOT NULL DEFAULT 1,
-- 字段：RerollStoneItemId — 标识字段
    RerollStoneItemId TEXT NOT NULL DEFAULT 'reroll_stone',
-- 字段：BaseStoneCost — 消耗或价格
    BaseStoneCost INTEGER NOT NULL DEFAULT 1,
-- 字段：ExtraStoneCostPerLockedLine — extra stone cost per locked line 字段
    ExtraStoneCostPerLockedLine INTEGER NOT NULL DEFAULT 1,
-- 字段：MaxLockedLineCount — 数量字段
    MaxLockedLineCount INTEGER NOT NULL DEFAULT 3,
-- 字段：BaseGoldCost — 消耗或价格
    BaseGoldCost INTEGER NOT NULL DEFAULT 300,
-- 字段：GoldCostPerEquipmentLevel — 等级或层级
    GoldCostPerEquipmentLevel INTEGER NOT NULL DEFAULT 25,
-- 字段：QualityGoldMultipliersJson — JSON 序列化配置或扩展数据
    QualityGoldMultipliersJson TEXT NULL,
-- 字段：RerollCountGoldGrowthPercent — 比例或概率
    RerollCountGoldGrowthPercent INTEGER NOT NULL DEFAULT 3,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
);

-- 表：EquipmentEnhanceRuleConfigs
-- 按装备需求等级区间配置强化材料、金币、成功率和属性成长。
CREATE TABLE EquipmentEnhanceRuleConfigs (
    GID INTEGER PRIMARY KEY AUTOINCREMENT,
    MinEquipmentLevel INTEGER NOT NULL DEFAULT 1,
    MaxEquipmentLevel INTEGER NOT NULL DEFAULT 30,
    MaterialItemId TEXT NOT NULL,
    MaterialCount INTEGER NOT NULL DEFAULT 1,
    GoldCost INTEGER NOT NULL DEFAULT 0,
    SuccessRate INTEGER NOT NULL DEFAULT 100,
    AttributeGrowthPercent INTEGER NOT NULL DEFAULT 10,
    MaxEnhanceLevel INTEGER NOT NULL DEFAULT 15,
    SortOrder INTEGER NOT NULL DEFAULT 0,
    IsEnabled INTEGER NOT NULL DEFAULT 1,
    SeedKey TEXT NULL,
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
    BuiltInVersion TEXT NULL,
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
);

-- 表：EquipmentDecomposeRuleConfigs
CREATE TABLE EquipmentDecomposeRuleConfigs (
    GID INTEGER PRIMARY KEY AUTOINCREMENT,
    Quality INTEGER NOT NULL DEFAULT 1,
    MaterialItemId TEXT NOT NULL,
    MinQuantity INTEGER NOT NULL DEFAULT 1,
    MaxQuantity INTEGER NOT NULL DEFAULT 1,
    SortOrder INTEGER NOT NULL DEFAULT 0,
    IsEnabled INTEGER NOT NULL DEFAULT 1,
    SeedKey TEXT NULL,
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
    BuiltInVersion TEXT NULL,
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
);

-- 表：EquipmentRerollCostRuleConfigs
-- 按装备需求等级区间配置洗炼材料、金币及锁定词条追加消耗。
CREATE TABLE EquipmentRerollCostRuleConfigs (
    GID INTEGER PRIMARY KEY AUTOINCREMENT,
    MinEquipmentLevel INTEGER NOT NULL DEFAULT 1,
    MaxEquipmentLevel INTEGER NOT NULL DEFAULT 30,
    MaterialItemId TEXT NOT NULL,
    MaterialCount INTEGER NOT NULL DEFAULT 1,
    GoldCost INTEGER NOT NULL DEFAULT 0,
    ExtraMaterialPerLockedLine INTEGER NOT NULL DEFAULT 0,
    ExtraGoldPerLockedLine INTEGER NOT NULL DEFAULT 0,
    SortOrder INTEGER NOT NULL DEFAULT 0,
    IsEnabled INTEGER NOT NULL DEFAULT 1,
    SeedKey TEXT NULL,
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
    BuiltInVersion TEXT NULL,
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
);
-- 表：EquipmentRerollTierConfigs
-- 装备洗练品阶配置。
CREATE TABLE EquipmentRerollTierConfigs (
-- 字段：GID — 标识字段
    GID INTEGER PRIMARY KEY AUTOINCREMENT,
-- 字段：Tier — tier 字段
    Tier INTEGER NOT NULL,
-- 字段：Name — 名称
    Name TEXT NOT NULL,
-- 字段：Color — color 字段
    Color TEXT NOT NULL DEFAULT '#9ca3af',
-- 字段：Weight — weight 字段
    Weight INTEGER NOT NULL DEFAULT 10,
-- 字段：ValueMultiplier — value multiplier 字段
    ValueMultiplier TEXT NOT NULL DEFAULT '1.00',
-- 字段：SortOrder — 排序或优先级
    SortOrder INTEGER NOT NULL DEFAULT 0,
-- 字段：IsEnabled — 状态标记
    IsEnabled INTEGER NOT NULL DEFAULT 1,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
);
-- 表：EquipmentTemplates
-- 装备模板落库实体
CREATE TABLE EquipmentTemplates (
-- 字段：EquipmentId — 标识字段
    EquipmentId INTEGER PRIMARY KEY,
-- 字段：Name — 名称
    Name TEXT NOT NULL,
-- 字段：Level — 等级或层级
    Level INTEGER NOT NULL DEFAULT 1,
-- 字段：Quality — quality 字段
    Quality INTEGER NOT NULL DEFAULT 1,
-- 字段：Slot — slot 字段
    Slot INTEGER NOT NULL DEFAULT 0,
-- 字段：CombatStyle — combat style 字段
    CombatStyle INTEGER NOT NULL DEFAULT 0,
-- 字段：WeaponCategory — weapon category 字段
    WeaponCategory INTEGER NOT NULL DEFAULT 0,
-- 字段：Description — 描述信息
    Description TEXT NOT NULL,
-- 字段：IconPath — icon path 字段
    IconPath TEXT NULL,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00',
-- 字段：MinType1 — min type1 字段
    MinType1 INTEGER NULL,
-- 字段：MaxType1 — max type1 字段
    MaxType1 INTEGER NULL,
-- 字段：MinType2 — min type2 字段
    MinType2 INTEGER NULL,
-- 字段：MaxType2 — max type2 字段
    MaxType2 INTEGER NULL,
-- 字段：MinType3 — min type3 字段
    MinType3 INTEGER NULL,
-- 字段：MaxType3 — max type3 字段
    MaxType3 INTEGER NULL,
-- 字段：MinType4 — min type4 字段
    MinType4 INTEGER NULL,
-- 字段：MaxType4 — max type4 字段
    MaxType4 INTEGER NULL,
-- 字段：MinType5 — min type5 字段
    MinType5 INTEGER NULL,
-- 字段：MaxType5 — max type5 字段
    MaxType5 INTEGER NULL,
-- 字段：MinType6 — min type6 字段
    MinType6 INTEGER NULL,
-- 字段：MaxType6 — max type6 字段
    MaxType6 INTEGER NULL,
-- 字段：MinType7 — min type7 字段
    MinType7 INTEGER NULL,
-- 字段：MaxType7 — max type7 字段
    MaxType7 INTEGER NULL,
-- 字段：MinType8 — min type8 字段
    MinType8 INTEGER NULL,
-- 字段：MaxType8 — max type8 字段
    MaxType8 INTEGER NULL,
-- 字段：MinType9 — min type9 字段
    MinType9 INTEGER NULL,
-- 字段：MaxType9 — max type9 字段
    MaxType9 INTEGER NULL,
-- 字段：MinType10 — min type10 字段
    MinType10 INTEGER NULL,
-- 字段：MaxType10 — max type10 字段
    MaxType10 INTEGER NULL,
-- 字段：MinType11 — min type11 字段
    MinType11 INTEGER NULL,
-- 字段：MaxType11 — max type11 字段
    MaxType11 INTEGER NULL,
-- 字段：MinType12 — min type12 字段
    MinType12 INTEGER NULL,
-- 字段：MaxType12 — max type12 字段
    MaxType12 INTEGER NULL,
-- 字段：MinType13 — min type13 字段
    MinType13 INTEGER NULL,
-- 字段：MaxType13 — max type13 字段
    MaxType13 INTEGER NULL,
-- 字段：MinType14 — min type14 字段
    MinType14 INTEGER NULL,
-- 字段：MaxType14 — max type14 字段
    MaxType14 INTEGER NULL,
-- 字段：MinType15 — min type15 字段
    MinType15 INTEGER NULL,
-- 字段：MaxType15 — max type15 字段
    MaxType15 INTEGER NULL,
-- 字段：Element — element 字段
    Element INTEGER NULL,
-- 字段：ElementPoolJson — JSON 序列化配置或扩展数据
    ElementPoolJson TEXT NULL
, IsTradeable INTEGER NOT NULL DEFAULT 1);
-- 表：FavorabilityAchievement
-- 好感度成就领取记录实体
CREATE TABLE FavorabilityAchievement (
-- 字段：Id — 标识字段
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
-- 字段：PlayerId — 标识字段
                    PlayerId TEXT NOT NULL,
-- 字段：AchievementId — 成就ID
                    AchievementId TEXT NOT NULL,
-- 字段：ClaimedAt — 领取时间
                    ClaimedAt DATETIME NOT NULL,
-- 字段：IsDeleted — 状态标记
                    IsDeleted INTEGER NOT NULL DEFAULT 0,
                    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
-- 字段：UpdatedAt — 时间或日期字段
                    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
                );
-- 表：FavorabilityGiftLog
-- 好感度赠送日志实体 记录玩家赠送礼物的历史流水，保留物品名称快照
CREATE TABLE FavorabilityGiftLog (
-- 字段：Id — 标识字段
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
-- 字段：PlayerId — 标识字段
                    PlayerId TEXT NOT NULL,
-- 字段：TargetPlayerId — 目标玩家ID
                    TargetPlayerId TEXT NOT NULL,
-- 字段：ItemId — 赠送物品ID（组队战斗来源时为 null）
                    ItemId TEXT,
-- 字段：ItemName — 赠送物品名称（历史快照，即使物品改名也能保留记录）
                    ItemName TEXT NOT NULL,
-- 字段：FavorabilityChange — 好感度变化值
                    FavorabilityChange INTEGER NOT NULL,
-- 字段：GiftTime — 赠送时间
                    GiftTime DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
-- 字段：UpdatedAt — 时间或日期字段
                    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
-- 字段：IsDeleted — 状态标记
                    IsDeleted INTEGER NOT NULL DEFAULT 0
                );
-- 表：FavorabilityLevelConfig
-- 好感度等级配置实体 定义好感度各等级的名称、数值区间、颜色和排序
CREATE TABLE FavorabilityLevelConfig (
-- 字段：Id — 配置ID
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
-- 字段：Level — 等级
                    Level INTEGER NOT NULL,
-- 字段：Name — 等级名称
                    Name TEXT NOT NULL,
-- 字段：MinValue — 最低好感度值
                    MinValue INTEGER NOT NULL,
-- 字段：MaxValue — 最高好感度值
                    MaxValue INTEGER NOT NULL,
-- 字段：Color — 等级颜色（十六进制）
                    Color TEXT NOT NULL DEFAULT '#CCCCCC',
-- 字段：SortOrder — 排序权重
                    SortOrder INTEGER NOT NULL DEFAULT 0,
-- 字段：RewardJson — 等级奖励JSON
                    RewardJson TEXT NULL,
                    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
-- 字段：UpdatedAt — 更新时间
                    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
                );
-- 表：FiveElementArrays
-- 五行聚灵阵数据实体 对应数据库五行聚灵阵表
CREATE TABLE FiveElementArrays (
-- 字段：Id — 主键ID
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
-- 字段：PlayerId — 玩家ID
    PlayerId TEXT NOT NULL,
-- 字段：ArrayLevel — 聚灵阵等级
    ArrayLevel INTEGER NOT NULL DEFAULT 1,
-- 字段：MetalLevel — 金元素等级
    MetalLevel INTEGER NOT NULL DEFAULT 1,
-- 字段：WoodLevel — 木元素等级
    WoodLevel INTEGER NOT NULL DEFAULT 1,
-- 字段：WaterLevel — 水元素等级
    WaterLevel INTEGER NOT NULL DEFAULT 1,
-- 字段：FireLevel — 火元素等级
    FireLevel INTEGER NOT NULL DEFAULT 1,
-- 字段：EarthLevel — 土元素等级
    EarthLevel INTEGER NOT NULL DEFAULT 1,
-- 字段：MetalExp — 金元素经验
    MetalExp INTEGER NOT NULL DEFAULT 0,
-- 字段：WoodExp — 木元素经验
    WoodExp INTEGER NOT NULL DEFAULT 0,
-- 字段：WaterExp — 水元素经验
    WaterExp INTEGER NOT NULL DEFAULT 0,
-- 字段：FireExp — 火元素经验
    FireExp INTEGER NOT NULL DEFAULT 0,
-- 字段：EarthExp — 土元素经验
    EarthExp INTEGER NOT NULL DEFAULT 0,
-- 字段：TotalSpiritPower — 总灵气值
    TotalSpiritPower INTEGER NOT NULL DEFAULT 0,
-- 字段：SpiritPowerRate — 灵气产出速率（每小时）
    SpiritPowerRate INTEGER NOT NULL DEFAULT 20,
-- 字段：LastCollectTime — 最后收集灵气时间
    LastCollectTime TEXT NOT NULL,
-- 字段：TodayCollectCount — 今日收集次数
    TodayCollectCount INTEGER NOT NULL DEFAULT 0,
-- 字段：DailyResetTime — 今日统计重置时间
    DailyResetTime TEXT NOT NULL,
-- 字段：TotalCollectedSpiritPower — 累计收集灵气
    TotalCollectedSpiritPower INTEGER NOT NULL DEFAULT 0,
-- 字段：ActiveCombinationsJson — 已激活的五行组合（JSON格式）
    ActiveCombinationsJson TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL
);
-- 表：FiveElementBranchRuleRanges
-- 五行分支等级区间规则。 同一元素在区间内复用属性映射、每级加成和升级消耗，减少规则维护量。
CREATE TABLE FiveElementBranchRuleRanges (GID TEXT PRIMARY KEY, ElementType TEXT NOT NULL, MinLevel INTEGER NOT NULL, MaxLevel INTEGER NOT NULL, AttributeType TEXT NOT NULL, BonusPerLevel INTEGER NOT NULL DEFAULT 0, GoldCost INTEGER NOT NULL DEFAULT 0, SpiritStoneCost INTEGER NOT NULL DEFAULT 0, MaterialsJson TEXT NULL, SortOrder INTEGER NOT NULL DEFAULT 0, IsEnabled INTEGER NOT NULL DEFAULT 1, SeedKey TEXT NULL, IsBuiltIn INTEGER NOT NULL DEFAULT 0, BuiltInVersion TEXT NULL, LastUpdateTime TEXT NOT NULL);
-- 表：FiveElementBranchUpgradeConfigs
-- 五行分支升级规则配置。
CREATE TABLE FiveElementBranchUpgradeConfigs (
-- 字段：GID — 标识字段
    GID TEXT PRIMARY KEY,
-- 字段：ElementType — 类型或状态
    ElementType TEXT NOT NULL,
-- 字段：TargetLevel — 等级或层级
    TargetLevel INTEGER NOT NULL,
-- 字段：GoldCost — 消耗或价格
    GoldCost INTEGER NOT NULL DEFAULT 0,
-- 字段：SpiritStoneCost — 消耗或价格
    SpiritStoneCost INTEGER NOT NULL DEFAULT 0,
-- 字段：MaterialsJson — JSON 序列化配置或扩展数据
    MaterialsJson TEXT NULL,
-- 字段：SortOrder — 排序或优先级
    SortOrder INTEGER NOT NULL DEFAULT 0,
-- 字段：IsEnabled — 状态标记
    IsEnabled INTEGER NOT NULL DEFAULT 1,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL
);
-- 表：FiveElementLevelConfigs
-- 聚灵阵主等级规则配置。
CREATE TABLE FiveElementLevelConfigs (
-- 字段：ArrayLevel — 等级或层级
    ArrayLevel INTEGER PRIMARY KEY,
-- 字段：UpgradeGoldCost — 消耗或价格
    UpgradeGoldCost INTEGER NOT NULL DEFAULT 0,
-- 字段：UpgradeSpiritStoneCost — 消耗或价格
    UpgradeSpiritStoneCost INTEGER NOT NULL DEFAULT 0,
-- 字段：UpgradeMaterialsJson — JSON 序列化配置或扩展数据
    UpgradeMaterialsJson TEXT NULL,
-- 字段：SpiritFieldYieldBonusPercent — 比例或概率
    SpiritFieldYieldBonusPercent INTEGER NOT NULL DEFAULT 0,
-- 字段：BattleExpBonusPercent — 比例或概率
    BattleExpBonusPercent INTEGER NOT NULL DEFAULT 0,
-- 字段：ProfessionLevelCap — profession level cap 字段
    ProfessionLevelCap INTEGER NOT NULL DEFAULT 1,
-- 字段：IsEnabled — 状态标记
    IsEnabled INTEGER NOT NULL DEFAULT 1,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL
);
-- 表：ForgeProfessionLevelConfigs
-- 锻造职业等级经验配置。
CREATE TABLE ForgeProfessionLevelConfigs (
-- 字段：Level — 等级或层级
    Level INTEGER PRIMARY KEY,
-- 字段：NextLevelExp — next level exp 字段
    NextLevelExp INTEGER NOT NULL DEFAULT 80,
-- 字段：IsEnabled — 状态标记
    IsEnabled INTEGER NOT NULL DEFAULT 1,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL
);
-- 表：ForgeProfessionRuleConfigs
-- 锻造职业通用规则配置。
CREATE TABLE ForgeProfessionRuleConfigs (
-- 字段：ConfigId — 标识字段
    ConfigId TEXT PRIMARY KEY,
-- 字段：SuccessBonusPerOverLevel — 等级或层级
    SuccessBonusPerOverLevel INTEGER NOT NULL DEFAULT 1,
-- 字段：MaxSuccessBonus — max success bonus 字段
    MaxSuccessBonus INTEGER NOT NULL DEFAULT 15,
-- 字段：SuccessExpBase — success exp base 字段
    SuccessExpBase INTEGER NOT NULL DEFAULT 12,
-- 字段：SuccessExpPerRequiredLevel — 等级或层级
    SuccessExpPerRequiredLevel INTEGER NOT NULL DEFAULT 4,
-- 字段：FailureExpBase — failure exp base 字段
    FailureExpBase INTEGER NOT NULL DEFAULT 6,
-- 字段：FailureExpPerRequiredLevel — 等级或层级
    FailureExpPerRequiredLevel INTEGER NOT NULL DEFAULT 2,
-- 字段：IsEnabled — 状态标记
    IsEnabled INTEGER NOT NULL DEFAULT 1,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL
);
-- 表：ForgeRecipes
-- 锻造配方实体。
CREATE TABLE ForgeRecipes (
-- 字段：RecipeId — 标识字段
    RecipeId TEXT PRIMARY KEY,
-- 字段：TemplateId — 标识字段
    TemplateId TEXT NOT NULL,
-- 字段：Name — 名称
    Name TEXT NOT NULL,
-- 字段：Description — 描述信息
    Description TEXT NOT NULL,
-- 字段：SlotName — 名称
    SlotName TEXT NOT NULL,
-- 字段：Quality — quality 字段
    Quality INTEGER NOT NULL DEFAULT 1,
-- 字段：Level — 等级或层级
    Level INTEGER NOT NULL DEFAULT 1,
-- 字段：Icon — icon 字段
    Icon TEXT NOT NULL,
-- 字段：CostGold — cost gold 字段
    CostGold INTEGER NOT NULL DEFAULT 0,
-- 字段：SuccessRate — 比例或概率
    SuccessRate INTEGER NOT NULL DEFAULT 100,
-- 字段：MaterialsJson — JSON 序列化配置或扩展数据
    MaterialsJson TEXT NOT NULL,
-- 字段：IsEnabled — 状态标记
    IsEnabled INTEGER NOT NULL DEFAULT 1,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
);
-- 表：ForgeSystems
-- 锻造系统数据实体。
CREATE TABLE ForgeSystems (
-- 字段：Id — 标识字段
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
-- 字段：PlayerId — 标识字段
    PlayerId TEXT NOT NULL,
-- 字段：BlacksmithLevel — 等级或层级
    BlacksmithLevel INTEGER NOT NULL DEFAULT 1,
-- 字段：BlacksmithExp — blacksmith exp 字段
    BlacksmithExp INTEGER NOT NULL DEFAULT 0,
-- 字段：TodayForgeCount — 数量字段
    TodayForgeCount INTEGER NOT NULL DEFAULT 0,
-- 字段：DailyResetTime — 时间或日期字段
    DailyResetTime TEXT NOT NULL,
-- 字段：TotalForgeCount — 数量字段
    TotalForgeCount INTEGER NOT NULL DEFAULT 0,
-- 字段：SuccessForgeCount — 数量字段
    SuccessForgeCount INTEGER NOT NULL DEFAULT 0,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL,
-- 字段：ActiveRecipeId — 标识字段
    ActiveRecipeId TEXT NULL,
-- 字段：ActiveForgeStartedAt — 时间或日期字段
    ActiveForgeStartedAt TEXT NULL,
-- 字段：ActiveForgeCompleteAt — 时间或日期字段
    ActiveForgeCompleteAt TEXT NULL,
-- 字段：PendingResultJson — JSON 序列化配置或扩展数据
    PendingResultJson TEXT NULL
);
-- 表：GemTemplates
-- 宝石模板实体
CREATE TABLE GemTemplates (
-- 字段：Id — 标识字段
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
-- 字段：GemId — 标识字段
    GemId TEXT NOT NULL,
-- 字段：Name — 名称
    Name TEXT NOT NULL,
-- 字段：Level — 等级或层级
    Level INTEGER NOT NULL DEFAULT 1,
-- 字段：AttributeType — 类型或状态
    AttributeType TEXT NOT NULL,
-- 字段：BonusValue — bonus value 字段
    BonusValue INTEGER NOT NULL DEFAULT 0,
-- 字段：BonusMode — bonus mode 字段
    BonusMode TEXT NOT NULL DEFAULT 'Flat',
-- 字段：IconPath — icon path 字段
    IconPath TEXT NULL,
-- 字段：Quality — quality 字段
    Quality INTEGER NOT NULL DEFAULT 1,
-- 字段：SynthCount — 数量字段
    SynthCount INTEGER NOT NULL DEFAULT 3,
-- 字段：SynthFromGemId — 标识字段
    SynthFromGemId TEXT NULL,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
, SynthSuccessRate INTEGER NOT NULL DEFAULT 100);
-- 表：genius_tournament_matches
-- 天骄赛对战记录实体
CREATE TABLE genius_tournament_matches (
-- 字段：MatchId — 标识字段
    MatchId TEXT PRIMARY KEY,
-- 字段：TournamentId — 标识字段
    TournamentId TEXT NOT NULL,
-- 字段：Player1Id — 标识字段
    Player1Id TEXT NOT NULL,
-- 字段：Player1Name — 名称
    Player1Name TEXT NOT NULL,
-- 字段：Player1SectName — 名称
    Player1SectName TEXT NOT NULL,
-- 字段：Player2Id — 标识字段
    Player2Id TEXT NOT NULL,
-- 字段：Player2Name — 名称
    Player2Name TEXT NOT NULL,
-- 字段：Player2SectName — 名称
    Player2SectName TEXT NOT NULL,
-- 字段：Round — round 字段
    Round INTEGER NOT NULL DEFAULT 0,
-- 字段：WinnerId — 标识字段
    WinnerId TEXT NULL,
-- 字段：BattleLogJson — JSON 序列化配置或扩展数据
    BattleLogJson TEXT NULL,
-- 字段：MatchTime — 时间或日期字段
    MatchTime TEXT NOT NULL
);
-- 表：genius_tournaments
-- 天骄赛实体
CREATE TABLE genius_tournaments (
-- 字段：TournamentId — 标识字段
    TournamentId TEXT PRIMARY KEY,
-- 字段：Season — 等级或层级
    Season INTEGER NOT NULL DEFAULT 1,
-- 字段：StartTime — 时间或日期字段
    StartTime TEXT NOT NULL,
-- 字段：EndTime — 时间或日期字段
    EndTime TEXT NOT NULL,
-- 字段：State — 类型或状态
    State INTEGER NOT NULL DEFAULT 0,
-- 字段：ResultsJson — JSON 序列化配置或扩展数据
    ResultsJson TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL
);
-- 表：guild
-- 公会实体
CREATE TABLE guild (
-- 字段：GID — 公会ID
    GID TEXT PRIMARY KEY,
-- 字段：Name — 公会名称
    Name TEXT NOT NULL,
-- 字段：Announcement — 公会公告
    Announcement TEXT NULL,
-- 字段：LeaderId — 会长ID
    LeaderId TEXT NOT NULL,
-- 字段：LeaderName — 会长名称
    LeaderName TEXT NOT NULL,
-- 字段：Level — 公会等级
    Level INTEGER NOT NULL DEFAULT 1,
-- 字段：Exp — 公会经验
    Exp INTEGER NOT NULL DEFAULT 0,
-- 字段：MemberCount — 成员数量
    MemberCount INTEGER NOT NULL DEFAULT 1,
-- 字段：MaxMembers — 最大成员数
    MaxMembers INTEGER NOT NULL DEFAULT 20,
-- 字段：Funds — 公会资金
    Funds INTEGER NOT NULL DEFAULT 0,
-- 字段：RequiredLevel — 加入条件：最低等级
    RequiredLevel INTEGER NOT NULL DEFAULT 1,
-- 字段：AutoJoin — 是否允许自由加入
    AutoJoin INTEGER NOT NULL DEFAULT 0,
-- 字段：Icon — 公会图标
    Icon TEXT NULL,
    CreateTime TEXT NOT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL,
-- 字段：IsDeleted — 是否已删除
    IsDeleted INTEGER NOT NULL DEFAULT 0
, SectTemplateId TEXT NULL, TotalDonation INTEGER NOT NULL DEFAULT 0);
-- 表：guild_member
-- 公会成员实体
CREATE TABLE guild_member (
-- 字段：GID — 记录ID
    GID TEXT PRIMARY KEY,
-- 字段：GuildId — 公会ID
    GuildId TEXT NOT NULL,
-- 字段：PlayerId — 成员ID
    PlayerId TEXT NOT NULL,
-- 字段：PlayerName — 成员名称
    PlayerName TEXT NOT NULL,
-- 字段：PlayerLevel — 成员等级
    PlayerLevel INTEGER NOT NULL DEFAULT 1,
-- 字段：Position — 公会职位
    Position INTEGER NOT NULL DEFAULT 0,
-- 字段：Contribution — 公会贡献值
    Contribution INTEGER NOT NULL DEFAULT 0,
-- 字段：TotalContribution — 总贡献值
    TotalContribution INTEGER NOT NULL DEFAULT 0,
-- 字段：JoinTime — 加入时间
    JoinTime TEXT NOT NULL,
-- 字段：LastActiveTime — 最后活跃时间
    LastActiveTime TEXT NOT NULL
);
-- 表：heart_sutra_templates
-- 心法模板实体
CREATE TABLE heart_sutra_templates (
-- 字段：SutraId — 标识字段
    SutraId TEXT PRIMARY KEY,
-- 字段：Name — 名称
    Name TEXT NOT NULL,
-- 字段：Description — 描述信息
    Description TEXT NULL,
-- 字段：SectId — 标识字段
    SectId TEXT NOT NULL,
-- 字段：MaxLayer — max layer 字段
    MaxLayer INTEGER NOT NULL DEFAULT 10,
-- 字段：LayersJson — JSON 序列化配置或扩展数据
    LayersJson TEXT NOT NULL DEFAULT '[]',
-- 字段：SortOrder — 排序或优先级
    SortOrder INTEGER NOT NULL DEFAULT 0,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL
);
-- 表：image_collection_bonus
-- 图片图鉴属性加成实体
CREATE TABLE image_collection_bonus (
-- 字段：BonusId — 标识字段
    BonusId TEXT PRIMARY KEY,
-- 字段：SeriesId — 标识字段
    SeriesId TEXT NOT NULL,
-- 字段：AttrType — 类型或状态
    AttrType TEXT NOT NULL,
-- 字段：AttrValue — attr value 字段
    AttrValue REAL NOT NULL DEFAULT 0,
-- 字段：ValueType — 属性值类型：0-固定值，1-百分比
    ValueType INTEGER NOT NULL DEFAULT 0,
-- 字段：SortOrder — 排序或优先级
    SortOrder INTEGER NOT NULL DEFAULT 0,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
);
-- 表：image_collection_item
-- 图片图鉴项实体
CREATE TABLE image_collection_item (
-- 字段：ItemId — 标识字段
    ItemId TEXT PRIMARY KEY,
-- 字段：SeriesId — 标识字段
    SeriesId TEXT NOT NULL,
-- 字段：ImageName — 名称
    ImageName TEXT NOT NULL,
-- 字段：ThumbUrl — thumb url 字段
    ThumbUrl TEXT NULL,
-- 字段：OriginalUrl — original url 字段
    OriginalUrl TEXT NULL,
-- 字段：SortOrder — 排序或优先级
    SortOrder INTEGER NOT NULL DEFAULT 0,
-- 字段：IsEnabled — 状态标记
    IsEnabled INTEGER NOT NULL DEFAULT 1,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
);
-- 表：image_collection_series
-- 图片图鉴系列实体
CREATE TABLE image_collection_series (
-- 字段：SeriesId — 标识字段
    SeriesId TEXT PRIMARY KEY,
-- 字段：Name — 名称
    Name TEXT NOT NULL,
-- 字段：Description — 描述信息
    Description TEXT NULL,
-- 字段：Icon — icon 字段
    Icon TEXT NULL,
-- 字段：SortOrder — 排序或优先级
    SortOrder INTEGER NOT NULL DEFAULT 0,
-- 字段：IsEnabled — 状态标记
    IsEnabled INTEGER NOT NULL DEFAULT 1,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
);
-- 表：InventoryItems
-- 背包物品实体
CREATE TABLE InventoryItems (
-- 字段：Id — 标识字段
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
-- 字段：PlayerId — 标识字段
    PlayerId TEXT NOT NULL,
-- 字段：ItemId — 标识字段
    ItemId TEXT NOT NULL,
-- 字段：Quantity — 数量字段
    Quantity INTEGER NOT NULL DEFAULT 1,
-- 字段：IsBound — 状态标记
    IsBound INTEGER NOT NULL DEFAULT 0,
-- 字段：AcquiredTime — 时间或日期字段
    AcquiredTime TEXT NOT NULL,
-- 字段：ExpireTime — 时间或日期字段
    ExpireTime TEXT NULL
);
-- 表：ItemChestConfigs
-- 宝箱扩展配置。
CREATE TABLE ItemChestConfigs (
-- 字段：ItemId — 标识字段
    ItemId TEXT PRIMARY KEY,
-- 字段：OpenMode — open mode 字段
    OpenMode INTEGER NOT NULL DEFAULT 1,
-- 字段：RollCount — 数量字段
    RollCount INTEGER NOT NULL DEFAULT 1,
-- 字段：IsEnabled — 状态标记
    IsEnabled INTEGER NOT NULL DEFAULT 1,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
);
-- 表：ItemChestRewardEntries
-- 宝箱奖励条目扩展配置。
CREATE TABLE ItemChestRewardEntries (
-- 字段：GID — 标识字段
    GID TEXT PRIMARY KEY,
-- 字段：ItemId — 标识字段
    ItemId TEXT NOT NULL,
-- 字段：RewardType — 类型或状态
    RewardType INTEGER NOT NULL DEFAULT 3,
-- 字段：TargetId — 标识字段
    TargetId TEXT NULL,
-- 字段：MinCount — 数量字段
    MinCount INTEGER NOT NULL DEFAULT 1,
-- 字段：MaxCount — 数量字段
    MaxCount INTEGER NOT NULL DEFAULT 1,
-- 字段：Weight — weight 字段
    Weight INTEGER NOT NULL DEFAULT 100,
-- 字段：IsBound — 状态标记
    IsBound INTEGER NOT NULL DEFAULT 0,
-- 字段：Description — 描述信息
    Description TEXT NULL,
-- 字段：SortOrder — 排序或优先级
    SortOrder INTEGER NOT NULL DEFAULT 0
);
-- 表：ItemPetEggConfigs
-- 宠物蛋扩展配置。
CREATE TABLE ItemPetEggConfigs (
-- 字段：ItemId — 标识字段
    ItemId TEXT PRIMARY KEY,
-- 字段：PetTemplateId — 标识字段
    PetTemplateId TEXT NOT NULL,
-- 字段：IsEnabled — 状态标记
    IsEnabled INTEGER NOT NULL DEFAULT 1,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
);
-- 表：ItemPillConfigs
-- 丹药扩展配置。
CREATE TABLE ItemPillConfigs (
-- 字段：ItemId — 标识字段
    ItemId TEXT PRIMARY KEY,
-- 字段：EffectType — 类型或状态
    EffectType INTEGER NOT NULL DEFAULT 2,
-- 字段：BreakthroughBonusPercent — 比例或概率
    BreakthroughBonusPercent INTEGER NOT NULL DEFAULT 0,
-- 字段：ExpGain — exp gain 字段
    ExpGain INTEGER NOT NULL DEFAULT 0,
-- 字段：AttributeType — 类型或状态
    AttributeType TEXT NULL,
-- 字段：AttributeValue — attribute value 字段
    AttributeValue REAL NOT NULL DEFAULT 0,
-- 字段：IsEnabled — 状态标记
    IsEnabled INTEGER NOT NULL DEFAULT 1,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
, DurationMinutes INTEGER NOT NULL DEFAULT 0, MaxUsageCount INTEGER NOT NULL DEFAULT 0, HealHpPercent INTEGER NOT NULL DEFAULT 0, HealMpPercent INTEGER NOT NULL DEFAULT 0);
-- 表：ItemSkillBookConfigs
-- 技能书扩展配置。
CREATE TABLE ItemSkillBookConfigs (
-- 字段：ItemId — 标识字段
    ItemId TEXT PRIMARY KEY,
-- 字段：SkillId — 标识字段
    SkillId INTEGER NOT NULL DEFAULT 0,
-- 字段：IsEnabled — 状态标记
    IsEnabled INTEGER NOT NULL DEFAULT 1,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
);
-- 表：ItemTemplates
-- 物品模板落库实体。
CREATE TABLE ItemTemplates (
-- 字段：ItemId — 标识字段
    ItemId TEXT PRIMARY KEY,
-- 字段：Name — 名称
    Name TEXT NOT NULL,
-- 字段：UseLevel — 等级或层级
    UseLevel INTEGER NOT NULL DEFAULT 1,
-- 字段：Type — 类型或状态
    Type INTEGER NOT NULL DEFAULT 0,
-- 字段：Description — 描述信息
    Description TEXT NOT NULL,
-- 字段：MaxStack — max stack 字段
    MaxStack INTEGER NOT NULL DEFAULT 1,
-- 字段：Quality — quality 字段
    Quality INTEGER NOT NULL DEFAULT 1,
-- 字段：IconPath — icon path 字段
    IconPath TEXT NULL,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00',
-- 字段：ChestConfigJson — JSON 序列化配置或扩展数据
    ChestConfigJson TEXT NULL,
-- 字段：SkillBookConfigJson — JSON 序列化配置或扩展数据
    SkillBookConfigJson TEXT NULL,
-- 字段：PetEggConfigJson — JSON 序列化配置或扩展数据
    PetEggConfigJson TEXT NULL,
-- 字段：PillConfigJson — JSON 序列化配置或扩展数据
    PillConfigJson TEXT NULL
, FavorabilityGiftConfigJson TEXT NULL, IsTradeable INTEGER NOT NULL DEFAULT 1);
-- 表：lottery_log
-- 抽奖日志实体
CREATE TABLE lottery_log (
-- 字段：LogId — 标识字段
    LogId TEXT PRIMARY KEY,
-- 字段：PlayerId — 标识字段
    PlayerId TEXT NOT NULL,
-- 字段：PoolId — 标识字段
    PoolId TEXT NOT NULL,
-- 字段：LotteryType — 类型或状态
    LotteryType INTEGER NOT NULL DEFAULT 0,
-- 字段：CostType — 类型或状态
    CostType INTEGER NOT NULL DEFAULT 0,
-- 字段：CostItemId — 标识字段
    CostItemId TEXT NULL,
-- 字段：CostAmount — 数量字段
    CostAmount INTEGER NOT NULL DEFAULT 0,
-- 字段：PrizeId — 标识字段
    PrizeId TEXT NOT NULL,
-- 字段：RewardType — 类型或状态
    RewardType INTEGER NOT NULL DEFAULT 0,
-- 字段：RewardTargetId — 标识字段
    RewardTargetId TEXT NULL,
-- 字段：RewardName — 名称
    RewardName TEXT NOT NULL,
-- 字段：RewardAmount — 数量字段
    RewardAmount INTEGER NOT NULL DEFAULT 0,
-- 字段：IsThanks — 状态标记
    IsThanks INTEGER NOT NULL DEFAULT 0,
-- 字段：RandomValue — random value 字段
    RandomValue INTEGER NOT NULL DEFAULT 0,
-- 字段：LotteryTime — 时间或日期字段
    LotteryTime TEXT NOT NULL,
-- 字段：PlayerIp — player ip 字段
    PlayerIp TEXT NULL
);
-- 表：lottery_pool
-- 抽奖池配置实体
CREATE TABLE lottery_pool (
-- 字段：PoolId — 标识字段
    PoolId TEXT PRIMARY KEY,
-- 字段：Name — 名称
    Name TEXT NOT NULL,
-- 字段：LotteryType — 抽奖类型：0-文字图鉴抽奖，1-图片图鉴抽奖，2-幸运抽奖
    LotteryType INTEGER NOT NULL DEFAULT 0,
-- 字段：CostType — 消耗类型：0-金币，1-灵石，2-道具
    CostType INTEGER NOT NULL DEFAULT 0,
-- 字段：CostItemId — 标识字段
    CostItemId TEXT NULL,
-- 字段：CostAmount — 数量字段
    CostAmount INTEGER NOT NULL DEFAULT 0,
-- 字段：IsEnabled — 状态标记
    IsEnabled INTEGER NOT NULL DEFAULT 1,
-- 字段：StartTime — 时间或日期字段
    StartTime TEXT NULL,
-- 字段：EndTime — 时间或日期字段
    EndTime TEXT NULL,
-- 字段：SupportSingle — support single 字段
    SupportSingle INTEGER NOT NULL DEFAULT 1,
-- 字段：SupportTen — support ten 字段
    SupportTen INTEGER NOT NULL DEFAULT 0,
-- 字段：DailyLimit — 每日抽奖次数限制，-1表示无限制
    DailyLimit INTEGER NOT NULL DEFAULT -1,
-- 字段：TotalLimit — 总抽奖次数限制，-1表示无限制
    TotalLimit INTEGER NOT NULL DEFAULT -1,
-- 字段：SortOrder — 排序或优先级
    SortOrder INTEGER NOT NULL DEFAULT 0,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
);
-- 表：lottery_prize
-- 抽奖奖项配置实体
CREATE TABLE lottery_prize (
-- 字段：PrizeId — 标识字段
    PrizeId TEXT PRIMARY KEY,
-- 字段：PoolId — 标识字段
    PoolId TEXT NOT NULL,
-- 字段：RewardType — 奖励类型：0-金币，1-灵石，2-道具，3-文字图鉴，4-图片图鉴，5-谢谢惠顾
    RewardType INTEGER NOT NULL DEFAULT 0,
-- 字段：RewardTargetId — 标识字段
    RewardTargetId TEXT NULL,
-- 字段：RewardAmount — 数量字段
    RewardAmount INTEGER NOT NULL DEFAULT 0,
-- 字段：Probability — 概率，万分比，10000=100%
    Probability INTEGER NOT NULL DEFAULT 0,
-- 字段：SortOrder — 排序或优先级
    SortOrder INTEGER NOT NULL DEFAULT 0,
-- 字段：IsEnabled — 状态标记
    IsEnabled INTEGER NOT NULL DEFAULT 1,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
);
-- 表：MailGlobalClaimRecords
-- 全服邮件领取记录实体。
CREATE TABLE MailGlobalClaimRecords (
-- 字段：Id — 主键。
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
-- 字段：MailId — 全服邮件ID。
    MailId INTEGER NOT NULL,
-- 字段：PlayerId — 玩家GID。
    PlayerId TEXT NOT NULL,
-- 字段：ClaimedAt — 领取时间。
    ClaimedAt TEXT NOT NULL
, IsRead INTEGER NOT NULL DEFAULT 0, HasClaimed INTEGER NOT NULL DEFAULT 0);
-- 表：MailMessages
-- 邮件消息实体。
CREATE TABLE MailMessages (
-- 字段：Id — 主键。
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
-- 字段：RecipientId — 收件人GID（全服邮件时为空）。
    RecipientId TEXT NULL,
-- 字段：SenderType — 发件人类型（System/GM）。
    SenderType TEXT NOT NULL DEFAULT 'System',
-- 字段：SenderName — 发件人名称。
    SenderName TEXT NOT NULL DEFAULT '系统',
-- 字段：Title — 邮件标题。
    Title TEXT NOT NULL,
-- 字段：Content — 邮件正文。
    Content TEXT NOT NULL DEFAULT '',
-- 字段：AttachmentsJson — 附件JSON。
    AttachmentsJson TEXT NULL,
-- 字段：IsRead — 是否已读。
    IsRead INTEGER NOT NULL DEFAULT 0,
-- 字段：IsClaimed — 附件是否已领取。
    IsClaimed INTEGER NOT NULL DEFAULT 0,
-- 字段：IsGlobal — 是否全服邮件。
    IsGlobal INTEGER NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL,
-- 字段：ExpireAt — 过期时间。
    ExpireAt TEXT NOT NULL
);
-- 表：MapTemplates
-- 地图模板落库实体。
CREATE TABLE MapTemplates (
-- 字段：MapId — 标识字段
    MapId TEXT PRIMARY KEY,
-- 字段：Name — 名称
    Name TEXT NOT NULL,
-- 字段：Level — 等级或层级
    Level INTEGER NOT NULL DEFAULT 1,
-- 字段：Description — 描述信息
    Description TEXT NOT NULL,
-- 字段：NextMapId — 标识字段
    NextMapId TEXT NULL,
-- 字段：Carrying — carrying 字段
    Carrying INTEGER NOT NULL DEFAULT 0,
-- 字段：MonsterCountMin — monster count min 字段
    MonsterCountMin INTEGER NOT NULL DEFAULT 1,
-- 字段：MonsterCountMax — monster count max 字段
    MonsterCountMax INTEGER NOT NULL DEFAULT 1,
-- 字段：SpawnRulesJson — JSON 序列化配置或扩展数据
    SpawnRulesJson TEXT NULL,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
);
-- 表：MarketConfigs
-- 寄售行配置实体
CREATE TABLE MarketConfigs (
-- 字段：Id — 标识字段
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
-- 字段：ConfigKey — config key 字段
    ConfigKey TEXT NOT NULL,
-- 字段：ConfigValue — config value 字段
    ConfigValue TEXT NOT NULL
);
-- 表：MarketListings
-- 寄售行商品实体
CREATE TABLE MarketListings (
-- 字段：Id — 标识字段
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
-- 字段：SellerId — 标识字段
    SellerId TEXT NOT NULL,
-- 字段：SellerName — 名称
    SellerName TEXT NOT NULL,
-- 字段：ItemType — 类型或状态
    ItemType TEXT NOT NULL,
-- 字段：ItemId — 标识字段
    ItemId TEXT NULL,
-- 字段：EquipmentInstanceId — 标识字段
    EquipmentInstanceId TEXT NULL,
-- 字段：Quantity — 数量字段
    Quantity INTEGER NOT NULL DEFAULT 1,
-- 字段：Price — 消耗或价格
    Price INTEGER NOT NULL DEFAULT 0,
-- 字段：CurrencyType — 类型或状态
    CurrencyType TEXT NOT NULL DEFAULT 'Gold',
-- 字段：Status — 类型或状态
    Status TEXT NOT NULL DEFAULT 'Listed',
-- 字段：IsBound — 状态标记
    IsBound INTEGER NOT NULL DEFAULT 0,
-- 字段：BuyerId — 标识字段
    BuyerId TEXT NULL,
-- 字段：SoldAt — 时间或日期字段
    SoldAt TEXT NULL,
    CreatedAt TEXT NOT NULL,
-- 字段：ExpireAt — 时间或日期字段
    ExpireAt TEXT NOT NULL
);
-- 表：MonsterTemplates
-- 怪物模板落库实体。
CREATE TABLE MonsterTemplates (
-- 字段：MonsterId — 标识字段
    MonsterId TEXT PRIMARY KEY,
-- 字段：Name — 名称
    Name TEXT NOT NULL,
-- 字段：Level — 等级或层级
    Level INTEGER NOT NULL DEFAULT 1,
-- 字段：ExpRewardMin — exp reward min 字段
    ExpRewardMin INTEGER NOT NULL DEFAULT 0,
-- 字段：ExpRewardMax — exp reward max 字段
    ExpRewardMax INTEGER NOT NULL DEFAULT 0,
-- 字段：GoldRewardMin — gold reward min 字段
    GoldRewardMin INTEGER NOT NULL DEFAULT 0,
-- 字段：GoldRewardMax — gold reward max 字段
    GoldRewardMax INTEGER NOT NULL DEFAULT 0,
-- 字段：SkillIdsJson — JSON 序列化配置或扩展数据
    SkillIdsJson TEXT NULL,
-- 字段：PassiveIdsJson — JSON 序列化配置或扩展数据
    PassiveIdsJson TEXT NULL,
-- 字段：ItemDropsJson — JSON 序列化配置或扩展数据
    ItemDropsJson TEXT NULL,
-- 字段：EquipmentDropsJson — JSON 序列化配置或扩展数据
    EquipmentDropsJson TEXT NULL,
-- 字段：MinType1 — min type1 字段
    MinType1 INTEGER NULL,
-- 字段：MaxType1 — max type1 字段
    MaxType1 INTEGER NULL,
-- 字段：MinType2 — min type2 字段
    MinType2 INTEGER NULL,
-- 字段：MaxType2 — max type2 字段
    MaxType2 INTEGER NULL,
-- 字段：MinType3 — min type3 字段
    MinType3 INTEGER NULL,
-- 字段：MaxType3 — max type3 字段
    MaxType3 INTEGER NULL,
-- 字段：MinType4 — min type4 字段
    MinType4 INTEGER NULL,
-- 字段：MaxType4 — max type4 字段
    MaxType4 INTEGER NULL,
-- 字段：MinType5 — min type5 字段
    MinType5 INTEGER NULL,
-- 字段：MaxType5 — max type5 字段
    MaxType5 INTEGER NULL,
-- 字段：MinType6 — min type6 字段
    MinType6 INTEGER NULL,
-- 字段：MaxType6 — max type6 字段
    MaxType6 INTEGER NULL,
-- 字段：MinType7 — min type7 字段
    MinType7 INTEGER NULL,
-- 字段：MaxType7 — max type7 字段
    MaxType7 INTEGER NULL,
-- 字段：MinType8 — min type8 字段
    MinType8 INTEGER NULL,
-- 字段：MaxType8 — max type8 字段
    MaxType8 INTEGER NULL,
-- 字段：MinType9 — min type9 字段
    MinType9 INTEGER NULL,
-- 字段：MaxType9 — max type9 字段
    MaxType9 INTEGER NULL,
-- 字段：MinType10 — min type10 字段
    MinType10 INTEGER NULL,
-- 字段：MaxType10 — max type10 字段
    MaxType10 INTEGER NULL,
-- 字段：MinType11 — min type11 字段
    MinType11 INTEGER NULL,
-- 字段：MaxType11 — max type11 字段
    MaxType11 INTEGER NULL,
-- 字段：MinType12 — min type12 字段
    MinType12 INTEGER NULL,
-- 字段：MaxType12 — max type12 字段
    MaxType12 INTEGER NULL,
-- 字段：MinType13 — min type13 字段
    MinType13 INTEGER NULL,
-- 字段：MaxType13 — max type13 字段
    MaxType13 INTEGER NULL,
-- 字段：MinType14 — min type14 字段
    MinType14 INTEGER NULL,
-- 字段：MaxType14 — max type14 字段
    MaxType14 INTEGER NULL,
-- 字段：MinType15 — min type15 字段
    MinType15 INTEGER NULL,
-- 字段：MaxType15 — max type15 字段
    MaxType15 INTEGER NULL,
-- 字段：Element — element 字段
    Element INTEGER NULL,
-- 字段：ElementPoolJson — JSON 序列化配置或扩展数据
    ElementPoolJson TEXT NULL,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
, CollectionDropsJson TEXT NULL);
-- 表：Parties
-- 临时副本队伍实体。
CREATE TABLE Parties (
-- 字段：PartyId — 标识字段
    PartyId TEXT PRIMARY KEY,
-- 字段：Name — 名称
    Name TEXT NOT NULL,
-- 字段：LeaderPlayerId — 标识字段
    LeaderPlayerId TEXT NOT NULL,
-- 字段：TargetDungeonId — 标识字段
    TargetDungeonId TEXT NOT NULL,
-- 字段：TargetDungeonName — 名称
    TargetDungeonName TEXT NOT NULL,
-- 字段：MinLevel — 等级或层级
    MinLevel INTEGER NOT NULL DEFAULT 1,
-- 字段：MaxMembers — max members 字段
    MaxMembers INTEGER NOT NULL DEFAULT 1,
-- 字段：IsRecruiting — 状态标记
    IsRecruiting INTEGER NOT NULL DEFAULT 1,
    CreateTime TEXT NOT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL,
-- 字段：ExpiresAt — 时间或日期字段
    ExpiresAt TEXT NOT NULL
);
-- 表：PartyBattleRecords
-- 组队副本主战斗摘要记录。 用于持久化一次队伍挑战的幂等状态、成员资格和结算摘要，防止 HTTP 重试重复扣次或发奖。
CREATE TABLE PartyBattleRecords (
-- 字段：BattleId — 战斗唯一编号。
    BattleId TEXT PRIMARY KEY,
-- 字段：PartyId — 临时队伍编号。
    PartyId TEXT NOT NULL,
-- 字段：DungeonId — 副本编号。
    DungeonId TEXT NOT NULL,
-- 字段：InitiatorPlayerId — 发起挑战的玩家编号。
    InitiatorPlayerId TEXT NOT NULL,
-- 字段：MemberIdsJson — 本场成员编号 JSON。
    MemberIdsJson TEXT NOT NULL DEFAULT '[]',
-- 字段：RequestId — 请求幂等编号。
    RequestId TEXT NOT NULL,
-- 字段：StartedAtUtc — 开始时间（UTC）。
    StartedAtUtc TEXT NOT NULL,
-- 字段：CompletedAtUtc — 完成时间（UTC）。
    CompletedAtUtc TEXT NULL,
-- 字段：IsVictory — 是否胜利。
    IsVictory INTEGER NOT NULL DEFAULT 0,
-- 字段：StageCount — 完成的副本层数。
    StageCount INTEGER NOT NULL DEFAULT 0,
-- 字段：TotalRounds — 总回合数。
    TotalRounds INTEGER NOT NULL DEFAULT 0,
-- 字段：RewardSummaryJson — 成员资格、次数和奖励摘要 JSON。
    RewardSummaryJson TEXT NOT NULL DEFAULT '{}',
-- 字段：SettlementStatus — 结算状态：Started、Completed、Failed、Settling。
    SettlementStatus TEXT NOT NULL DEFAULT 'Started',
-- 字段：FullReplayJson — 完整回放 JSON；当前默认保留，后续清理任务可按完成时间删除。
    FullReplayJson TEXT NULL
);
-- 表：PartyMembers
-- 临时副本队伍成员实体。
CREATE TABLE PartyMembers (
-- 字段：MembershipId — 标识字段
    MembershipId TEXT PRIMARY KEY,
-- 字段：PartyId — 标识字段
    PartyId TEXT NOT NULL,
-- 字段：PlayerId — 标识字段
    PlayerId TEXT NOT NULL,
-- 字段：IsLeader — 状态标记
    IsLeader INTEGER NOT NULL DEFAULT 0,
-- 字段：JoinTime — 时间或日期字段
    JoinTime TEXT NOT NULL
);
-- 表：PetInstances
-- 灵宠实例实体 对应数据库灵宠实例表
CREATE TABLE PetInstances (
-- 字段：InstanceId — 实例ID
    InstanceId TEXT PRIMARY KEY,
-- 字段：PlayerId — 玩家ID
    PlayerId TEXT NOT NULL,
-- 字段：TemplateId — 模板ID
    TemplateId TEXT NOT NULL,
-- 字段：Name — 灵宠名称
    Name TEXT NOT NULL,
-- 字段：Level — 当前等级
    Level INTEGER NOT NULL DEFAULT 1,
-- 字段：Exp — 当前经验
    Exp INTEGER NOT NULL DEFAULT 0,
-- 字段：XExp — 升级所需经验
    XExp INTEGER NOT NULL DEFAULT 100,
-- 字段：Quality — 当前品质
    Quality INTEGER NOT NULL DEFAULT 1,
-- 字段：GrowthRate — 当前成长率。
    GrowthRate REAL NOT NULL DEFAULT 1.0,
-- 字段：Type1 — 生命值。
    Type1 INTEGER NOT NULL DEFAULT 100,
-- 字段：Type2 — 法力值。
    Type2 INTEGER NOT NULL DEFAULT 50,
-- 字段：Type3 — 物理攻击。
    Type3 INTEGER NOT NULL DEFAULT 10,
-- 字段：Type4 — 法术攻击。
    Type4 INTEGER NOT NULL DEFAULT 5,
-- 字段：Type5 — 物理防御。
    Type5 INTEGER NOT NULL DEFAULT 5,
-- 字段：Type6 — 法术防御。
    Type6 INTEGER NOT NULL DEFAULT 3,
-- 字段：Type7 — 速度。
    Type7 INTEGER NOT NULL DEFAULT 10,
-- 字段：Type8 — 命中率。
    Type8 REAL NOT NULL DEFAULT 0.9,
-- 字段：Type9 — 闪避率。
    Type9 REAL NOT NULL DEFAULT 0,
-- 字段：Type10 — 暴击率。
    Type10 REAL NOT NULL DEFAULT 0,
-- 字段：Type11 — 暴击伤害倍率。
    Type11 REAL NOT NULL DEFAULT 1.5,
-- 字段：Type12 — 连击率。
    Type12 REAL NOT NULL DEFAULT 0,
-- 字段：Type13 — 反击率。
    Type13 REAL NOT NULL DEFAULT 0,
-- 字段：Type14 — 破甲率。
    Type14 REAL NOT NULL DEFAULT 0,
-- 字段：Type15 — 额外伤害。
    Type15 REAL NOT NULL DEFAULT 0,
-- 字段：Element — 元素。
    Element INTEGER NOT NULL DEFAULT 0,
-- 字段：Loyalty — 忠诚度（0-100）
    Loyalty INTEGER NOT NULL DEFAULT 100,
-- 字段：IsActive — 是否出战
    IsActive INTEGER NOT NULL DEFAULT 0,
-- 字段：IsBound — 是否绑定
    IsBound INTEGER NOT NULL DEFAULT 1,
-- 字段：SkillIdsJson — 技能ID列表（JSON格式）
    SkillIdsJson TEXT NULL,
-- 字段：AcquiredTime — 获得时间
    AcquiredTime TEXT NOT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL
);
-- 表：PetTemplates
-- 灵宠模板实体 对应数据库灵宠模板表
CREATE TABLE PetTemplates (
-- 字段：TemplateId — 模板ID
    TemplateId TEXT PRIMARY KEY,
-- 字段：Name — 灵宠名称
    Name TEXT NOT NULL,
-- 字段：Description — 灵宠描述
    Description TEXT NULL,
-- 字段：Type — 灵宠类型
    Type INTEGER NOT NULL DEFAULT 0,
-- 字段：InitialQualityMin — 初始品质下限。
    InitialQualityMin INTEGER NOT NULL DEFAULT 1,
-- 字段：InitialQualityMax — 初始品质上限。
    InitialQualityMax INTEGER NOT NULL DEFAULT 1,
-- 字段：MaxQuality — 最高品质
    MaxQuality INTEGER NOT NULL DEFAULT 5,
-- 字段：GrowthRateMin — 成长率下限。
    GrowthRateMin REAL NOT NULL DEFAULT 1.0,
-- 字段：GrowthRateMax — 成长率上限。
    GrowthRateMax REAL NOT NULL DEFAULT 1.0,
-- 字段：InitialSkillCount — 初始技能数量。
    InitialSkillCount INTEGER NOT NULL DEFAULT 1,
-- 字段：Element — 元素。
    Element INTEGER NOT NULL DEFAULT 0,
-- 字段：MinType1 — 生命下限。
    MinType1 INTEGER NULL,
-- 字段：MaxType1 — 生命上限。
    MaxType1 INTEGER NULL,
-- 字段：MinType2 — 法力下限。
    MinType2 INTEGER NULL,
-- 字段：MaxType2 — 法力上限。
    MaxType2 INTEGER NULL,
-- 字段：MinType3 — 物攻下限。
    MinType3 INTEGER NULL,
-- 字段：MaxType3 — 物攻上限。
    MaxType3 INTEGER NULL,
-- 字段：MinType4 — 法攻下限。
    MinType4 INTEGER NULL,
-- 字段：MaxType4 — 法攻上限。
    MaxType4 INTEGER NULL,
-- 字段：MinType5 — 物防下限。
    MinType5 INTEGER NULL,
-- 字段：MaxType5 — 物防上限。
    MaxType5 INTEGER NULL,
-- 字段：MinType6 — 法防下限。
    MinType6 INTEGER NULL,
-- 字段：MaxType6 — 法防上限。
    MaxType6 INTEGER NULL,
-- 字段：MinType7 — 速度下限。
    MinType7 INTEGER NULL,
-- 字段：MaxType7 — 速度上限。
    MaxType7 INTEGER NULL,
-- 字段：MinType8 — 命中率下限，按 0-100 存模板值。
    MinType8 INTEGER NULL,
-- 字段：MaxType8 — 命中率上限，按 0-100 存模板值。
    MaxType8 INTEGER NULL,
-- 字段：MinType9 — 闪避率下限，按 0-100 存模板值。
    MinType9 INTEGER NULL,
-- 字段：MaxType9 — 闪避率上限，按 0-100 存模板值。
    MaxType9 INTEGER NULL,
-- 字段：MinType10 — 暴击率下限，按 0-100 存模板值。
    MinType10 INTEGER NULL,
-- 字段：MaxType10 — 暴击率上限，按 0-100 存模板值。
    MaxType10 INTEGER NULL,
-- 字段：MinType11 — 暴击伤害下限，按 0-100 存模板值。
    MinType11 INTEGER NULL,
-- 字段：MaxType11 — 暴击伤害上限，按 0-100 存模板值。
    MaxType11 INTEGER NULL,
-- 字段：MinType12 — 连击率下限，按 0-100 存模板值。
    MinType12 INTEGER NULL,
-- 字段：MaxType12 — 连击率上限，按 0-100 存模板值。
    MaxType12 INTEGER NULL,
-- 字段：MinType13 — 反击率下限，按 0-100 存模板值。
    MinType13 INTEGER NULL,
-- 字段：MaxType13 — 反击率上限，按 0-100 存模板值。
    MaxType13 INTEGER NULL,
-- 字段：MinType14 — 破甲率下限，按 0-100 存模板值。
    MinType14 INTEGER NULL,
-- 字段：MaxType14 — 破甲率上限，按 0-100 存模板值。
    MaxType14 INTEGER NULL,
-- 字段：MinType15 — 额外伤害下限，按 0-100 存模板值。
    MinType15 INTEGER NULL,
-- 字段：MaxType15 — 额外伤害上限，按 0-100 存模板值。
    MaxType15 INTEGER NULL,
-- 字段：SkillIdsJson — 技能ID列表（JSON格式）
    SkillIdsJson TEXT NULL,
-- 字段：ObtainMethod — 获取方式
    ObtainMethod TEXT NULL,
-- 字段：IsTradable — 是否可交易
    IsTradable INTEGER NOT NULL DEFAULT 1,
-- 字段：SeedKey — 内置种子键。
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 是否为内置模板。
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置版本号。
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间。
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
);
-- 表：player_collection
-- 玩家图鉴拥有记录实体
CREATE TABLE player_collection (
-- 字段：Id — 标识字段
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
-- 字段：PlayerId — 标识字段
    PlayerId TEXT NOT NULL,
-- 字段：CollectionType — 图鉴类型：0-文字图鉴，1-图片图鉴
    CollectionType INTEGER NOT NULL DEFAULT 0,
-- 字段：SeriesId — 标识字段
    SeriesId TEXT NOT NULL,
-- 字段：ItemId — 标识字段
    ItemId TEXT NOT NULL,
-- 字段：OwnedCount — 数量字段
    OwnedCount INTEGER NOT NULL DEFAULT 0,
-- 字段：FirstGetTime — 时间或日期字段
    FirstGetTime TEXT NOT NULL,
-- 字段：LastGetTime — 时间或日期字段
    LastGetTime TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
-- 字段：UpdatedAt — 时间或日期字段
    UpdatedAt TEXT NOT NULL,
-- 字段：IsDeleted — 状态标记
    IsDeleted INTEGER NOT NULL DEFAULT 0
);
-- 表：player_heart_sutras
-- 玩家心法进度实体
CREATE TABLE player_heart_sutras (
-- 字段：GID — 标识字段
    GID TEXT PRIMARY KEY,
-- 字段：PlayerId — 标识字段
    PlayerId TEXT NOT NULL,
-- 字段：SutraId — 标识字段
    SutraId TEXT NOT NULL,
-- 字段：CurrentLayer — current layer 字段
    CurrentLayer INTEGER NOT NULL DEFAULT 0,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL
);
-- 表：PlayerAttributeAllocations
-- 玩家属性点分配实体。 单条记录只表示一种属性当前已投入的整数点数。
CREATE TABLE PlayerAttributeAllocations (
-- 字段：AllocationId — 分配记录主键。
    AllocationId TEXT PRIMARY KEY,
-- 字段：PlayerId — 玩家ID。
    PlayerId TEXT NOT NULL,
-- 字段：AttributeKey — 属性键，例如 type1 / type3。
    AttributeKey TEXT NOT NULL,
-- 字段：AllocatedPoints — 当前已经投入的点数。
    AllocatedPoints INTEGER NOT NULL DEFAULT 0,
    CreateTime TEXT NOT NULL,
-- 字段：LastUpdateTime — 最后更新时间。
    LastUpdateTime TEXT NOT NULL
);
-- 表：PlayerCheckInRecords
-- 中文注释： 玩家每日签到明细表。 这张表按“玩家 + 日期”记录每一次实际签到结果， 主要用于两类场景： 1. 给前端月历面板提供真实勾选数据； 2. 用唯一索引硬性防止同一天重复签到。
CREATE TABLE PlayerCheckInRecords (
-- 字段：Id — 标识字段
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
-- 字段：PlayerId — 标识字段
    PlayerId TEXT NOT NULL,
-- 字段：CheckInDate — 时间或日期字段
    CheckInDate TEXT NOT NULL,
-- 字段：RewardSnapshotJson — 本次签到奖励快照。 使用 JSON 保存，是为了让后续排查“玩家那天到底领了什么”时有据可查。
    RewardSnapshotJson TEXT NULL,
    CreateTime TEXT NOT NULL
);
-- 表：PlayerCheckInStates
-- 中文注释： 玩家签到总状态表。 这张表负责保存“连续签到天数、总签到天数、上次签到时间”这类聚合数据， 目的是避免前端每次打开签到弹窗时都临时扫全量签到记录再自己计算连续天数。
CREATE TABLE PlayerCheckInStates (
-- 字段：PlayerId — 玩家 ID。 一名玩家只保留一条签到总状态，因此直接用 PlayerId 作为主键。
    PlayerId TEXT PRIMARY KEY,
-- 字段：ContinuousDays — 当前连续签到天数。
    ContinuousDays INTEGER NOT NULL DEFAULT 0,
-- 字段：TotalDays — 历史总签到天数。
    TotalDays INTEGER NOT NULL DEFAULT 0,
-- 字段：LastCheckInDate — 上次签到日期。
    LastCheckInDate TEXT NULL,
-- 字段：LastUpdateTime — 最近一次状态更新时间。
    LastUpdateTime TEXT NOT NULL
);
-- 表：PlayerFavorability
-- 玩家好感度实体 记录玩家之间的好感度数值及今日赠送信息
CREATE TABLE PlayerFavorability (
-- 字段：Id — 标识字段
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
-- 字段：PlayerId — 标识字段
                    PlayerId TEXT NOT NULL,
-- 字段：TargetPlayerId — 目标玩家ID
                    TargetPlayerId TEXT NOT NULL,
-- 字段：Value — 好感度数值
                    Value INTEGER NOT NULL DEFAULT 0,
-- 字段：TodayGiftJson — 今日赠送记录JSON
                    TodayGiftJson TEXT,
-- 字段：TodayPartyBattleJson — 今日组队战斗好感度记录JSON，格式：{"2026-06-15":true}
                    TodayPartyBattleJson TEXT,
-- 字段：LastGiftTime — 最后赠送时间
                    LastGiftTime DATETIME,
                    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
-- 字段：UpdatedAt — 时间或日期字段
                    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
-- 字段：IsDeleted — 状态标记
                    IsDeleted INTEGER NOT NULL DEFAULT 0
                );
-- 表：PlayerFeedbackAttachments
-- 建议反馈图片附件实体。
CREATE TABLE PlayerFeedbackAttachments (
-- 字段：Id — 标识字段
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
-- 字段：FeedbackId — 所属反馈编号。
    FeedbackId INTEGER NOT NULL,
-- 字段：RelativePath — 图片相对路径。
    RelativePath TEXT NOT NULL,
-- 字段：OriginalFileName — 原始文件名。
    OriginalFileName TEXT NULL,
-- 字段：ContentType — 文件 MIME 类型。
    ContentType TEXT NOT NULL,
-- 字段：FileSize — 文件大小，单位为字节。
    FileSize INTEGER NOT NULL,
-- 字段：SortOrder — 图片展示顺序。
    SortOrder INTEGER NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL,
-- 字段：UpdatedAt — 时间或日期字段
    UpdatedAt TEXT NOT NULL,
-- 字段：IsDeleted — 状态标记
    IsDeleted INTEGER NOT NULL DEFAULT 0
);
-- 表：PlayerFeedbacks
-- 玩家建议反馈实体。 保存玩家提交的反馈正文、处理状态以及管理员处理结果。
CREATE TABLE PlayerFeedbacks (
-- 字段：Id — 标识字段
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
-- 字段：PlayerId — 提交反馈的玩家编号。
    PlayerId TEXT NOT NULL,
-- 字段：Type — 反馈类型：Suggestion、Bug、Gameplay、Account 或 Other。
    Type TEXT NOT NULL DEFAULT 'Suggestion',
-- 字段：Title — 反馈标题。
    Title TEXT NOT NULL,
-- 字段：Content — 反馈正文。
    Content TEXT NOT NULL,
-- 字段：Status — 当前处理状态：Pending、Processing、Resolved、Rejected 或 Closed。
    Status TEXT NOT NULL DEFAULT 'Pending',
-- 字段：AdminReply — 玩家可见的管理员回复。
    AdminReply TEXT NULL,
-- 字段：InternalNote — 仅管理员可见的内部备注。
    InternalNote TEXT NULL,
-- 字段：HandledByAdminId — 最后处理反馈的管理员编号。
    HandledByAdminId TEXT NULL,
-- 字段：HandledAt — 最后处理时间。
    HandledAt TEXT NULL,
    CreatedAt TEXT NOT NULL,
-- 字段：UpdatedAt — 时间或日期字段
    UpdatedAt TEXT NOT NULL,
-- 字段：IsDeleted — 状态标记
    IsDeleted INTEGER NOT NULL DEFAULT 0
);
-- 表：PlayerFeedbackStatusHistories
-- 建议反馈状态历史实体。
CREATE TABLE PlayerFeedbackStatusHistories (
-- 字段：Id — 标识字段
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
-- 字段：FeedbackId — 所属反馈编号。
    FeedbackId INTEGER NOT NULL,
-- 字段：FromStatus — 变更前状态。
    FromStatus TEXT NULL,
-- 字段：ToStatus — 变更后状态。
    ToStatus TEXT NOT NULL,
-- 字段：AdminId — 执行操作的管理员编号。
    AdminId TEXT NOT NULL,
-- 字段：ReplySnapshot — 操作时保存的玩家可见回复快照。
    ReplySnapshot TEXT NULL,
-- 字段：NoteSnapshot — 操作时保存的内部备注快照。
    NoteSnapshot TEXT NULL,
    CreatedAt TEXT NOT NULL,
-- 字段：UpdatedAt — 时间或日期字段
    UpdatedAt TEXT NOT NULL,
-- 字段：IsDeleted — 状态标记
    IsDeleted INTEGER NOT NULL DEFAULT 0
);
-- 表：PlayerInitialResourceConfigs
-- 玩家初始资源配置。
CREATE TABLE PlayerInitialResourceConfigs (
-- 字段：ConfigId — 标识字段
    ConfigId TEXT PRIMARY KEY,
-- 字段：StartLevel — 等级或层级
    StartLevel INTEGER NOT NULL DEFAULT 1,
-- 字段：StartExp — start exp 字段
    StartExp INTEGER NOT NULL DEFAULT 0,
-- 字段：StartGold — start gold 字段
    StartGold INTEGER NOT NULL DEFAULT 1000,
-- 字段：StartSpiritStone — start spirit stone 字段
    StartSpiritStone INTEGER NOT NULL DEFAULT 0,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL
);
-- 表：PlayerLevelConfigs
-- 玩家等级成长配置。
CREATE TABLE PlayerLevelConfigs (
-- 字段：Level — 等级或层级
    Level INTEGER PRIMARY KEY,
-- 字段：RequiredExp — required exp 字段
    RequiredExp INTEGER NOT NULL DEFAULT 0,
-- 字段：BaseHp — base hp 字段
    BaseHp INTEGER NOT NULL DEFAULT 1,
-- 字段：BaseMp — base mp 字段
    BaseMp INTEGER NOT NULL DEFAULT 0,
-- 字段：BasePhysicalAttack — base physical attack 字段
    BasePhysicalAttack INTEGER NOT NULL DEFAULT 0,
-- 字段：BaseMagicAttack — base magic attack 字段
    BaseMagicAttack INTEGER NOT NULL DEFAULT 0,
-- 字段：BasePhysicalDefense — base physical defense 字段
    BasePhysicalDefense INTEGER NOT NULL DEFAULT 0,
-- 字段：BaseMagicDefense — base magic defense 字段
    BaseMagicDefense INTEGER NOT NULL DEFAULT 0,
-- 字段：BaseSpeed — base speed 字段
    BaseSpeed INTEGER NOT NULL DEFAULT 0,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
);
-- 表：PlayerPillEffects
-- 玩家丹药效果记录。
CREATE TABLE PlayerPillEffects (
-- 字段：Id — 标识字段
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
-- 字段：PlayerId — 标识字段
                    PlayerId TEXT NOT NULL,
-- 字段：ItemId — 标识字段
                    ItemId TEXT NOT NULL,
-- 字段：EffectType — 丹药效果类型，对应 ItemPillEffectType 枚举值。
                    EffectType INTEGER NOT NULL DEFAULT 0,
-- 字段：BonusType — 加成类型名称，如"物攻"、"突破概率"等，用于展示。
                    BonusType TEXT NULL,
-- 字段：BonusValue — 单次加成数值。 限时突破概率丹药多次使用时此值会累加；限时属性丹药不累加。
                    BonusValue REAL NOT NULL DEFAULT 0,
-- 字段：UsageCount — 已使用次数。
                    UsageCount INTEGER NOT NULL DEFAULT 0,
-- 字段：IsTemporary — 是否为限时效果。
                    IsTemporary INTEGER NOT NULL DEFAULT 0,
-- 字段：ExpiresAt — 过期时间。限时效果非 null，永久效果为 null。
                    ExpiresAt TEXT NULL,
                    CreatedAt TEXT NOT NULL,
-- 字段：LastUpdateTime — 最后更新时间
                    LastUpdateTime TEXT NOT NULL
                );
-- 表：PlayerTitles
-- 玩家称号实体。
CREATE TABLE PlayerTitles (
-- 字段：Id — 主键。
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
-- 字段：PlayerId — 玩家GID。
    PlayerId TEXT NOT NULL,
-- 字段：TitleId — 称号ID。
    TitleId TEXT NOT NULL,
-- 字段：IsEquipped — 是否佩戴中。
    IsEquipped INTEGER NOT NULL DEFAULT 0,
-- 字段：UnlockedAt — 解锁时间。
    UnlockedAt TEXT NOT NULL
);
-- 表：quest_completed_record
-- 任务完成记录实体 记录玩家已完成的任务历史
CREATE TABLE quest_completed_record (
-- 字段：GID — 记录唯一标识符
    GID TEXT PRIMARY KEY,
-- 字段：PlayerId — 玩家ID
    PlayerId TEXT NOT NULL,
-- 字段：QuestId — 任务ID
    QuestId TEXT NOT NULL,
-- 字段：CompleteTime — 完成任务的时间
    CompleteTime TEXT NOT NULL,
-- 字段：SubmitTime — 提交任务的时间
    SubmitTime TEXT NULL
);
-- 表：quest_config
-- 任务配置实体 存储任务的配置信息，支持数据库动态配置
CREATE TABLE quest_config (
-- 字段：QuestId — 任务唯一标识符
    QuestId TEXT PRIMARY KEY,
-- 字段：SeedKey — 内置种子键。
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 是否为系统内置任务。
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置版本号。
    BuiltInVersion TEXT NULL,
-- 字段：QuestName — 任务名称
    QuestName TEXT NOT NULL,
-- 字段：QuestType — 任务类型：0-主线，1-支线，2-日常，3-周常，4-活动
    QuestType INTEGER NOT NULL DEFAULT 0,
-- 字段：ResetCycle — 任务重置周期：0-一次性，1-每日，2-每周
    ResetCycle INTEGER NOT NULL DEFAULT 0,
-- 字段：Description — 任务描述
    Description TEXT NOT NULL,
-- 字段：RequiredLevel — 接取任务所需的最低等级
    RequiredLevel INTEGER NOT NULL DEFAULT 1,
-- 字段：PreQuestIds — 前置任务ID列表，逗号分隔
    PreQuestIds TEXT NULL,
-- 字段：AutoAccept — 是否自动接取
    AutoAccept INTEGER NOT NULL DEFAULT 0,
-- 字段：AutoSubmit — 是否自动提交
    AutoSubmit INTEGER NOT NULL DEFAULT 0,
-- 字段：TimeLimit — 任务时间限制（秒），0表示无限制
    TimeLimit INTEGER NOT NULL DEFAULT 0,
-- 字段：RewardExp — 奖励经验值
    RewardExp INTEGER NOT NULL DEFAULT 0,
-- 字段：RewardGold — 奖励金币数量
    RewardGold INTEGER NOT NULL DEFAULT 0,
-- 字段：RewardSpiritStone — 奖励灵石数量
    RewardSpiritStone INTEGER NOT NULL DEFAULT 0,
-- 字段：RewardItemsJson — 奖励道具的JSON序列化数据
    RewardItemsJson TEXT NULL,
-- 字段：RewardEquipmentIds — 奖励装备ID列表，逗号分隔
    RewardEquipmentIds TEXT NULL,
-- 字段：ObjectivesJson — 任务目标的JSON序列化数据
    ObjectivesJson TEXT NULL,
-- 字段：SortOrder — 排序顺序，数值越小越靠前
    SortOrder INTEGER NOT NULL DEFAULT 0,
-- 字段：IsEnabled — 是否启用该任务
    IsEnabled INTEGER NOT NULL DEFAULT 1,
-- 字段：LastUpdateTime — 最后更新时间。
    LastUpdateTime TEXT NOT NULL
);
-- 表：quest_progress
-- 任务进度实体 记录玩家的任务完成进度
CREATE TABLE quest_progress (
-- 字段：GID — 记录唯一标识符
    GID TEXT PRIMARY KEY,
-- 字段：PlayerId — 玩家ID
    PlayerId TEXT NOT NULL,
-- 字段：QuestId — 任务ID
    QuestId TEXT NOT NULL,
-- 字段：CurrentStage — 当前任务阶段
    CurrentStage INTEGER NOT NULL DEFAULT 0,
-- 字段：ObjectiveProgressJson — 各目标进度的JSON序列化数据
    ObjectiveProgressJson TEXT NULL,
-- 字段：Status — 任务状态：0-未接取，1-进行中，2-已完成，3-已提交
    Status INTEGER NOT NULL DEFAULT 0,
-- 字段：AcceptTime — 接取任务的时间
    AcceptTime TEXT NULL,
-- 字段：CompleteTime — 完成任务的时间
    CompleteTime TEXT NULL,
-- 字段：SubmitTime — 提交任务的时间
    SubmitTime TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL
);
-- 表：ranking_config
-- 排行榜配置实体 存储排行榜的基本配置信息
CREATE TABLE ranking_config (
-- 字段：RankingId — 排行榜唯一标识符
    RankingId TEXT PRIMARY KEY,
-- 字段：RankingName — 排行榜名称
    RankingName TEXT NOT NULL,
-- 字段：RankingType — 排行榜类型：0-等级，1-战力，2-竞技场，3-成就，4-财富
    RankingType INTEGER NOT NULL DEFAULT 0,
-- 字段：Description — 排行榜描述
    Description TEXT NULL,
-- 字段：MaxSize — 排行榜最大容量
    MaxSize INTEGER NOT NULL DEFAULT 100,
-- 字段：UpdateInterval — 自动更新间隔（分钟）
    UpdateInterval INTEGER NOT NULL DEFAULT 60,
-- 字段：SeasonEnabled — 是否启用赛季机制
    SeasonEnabled INTEGER NOT NULL DEFAULT 0,
-- 字段：SeasonDuration — 赛季时长（天）
    SeasonDuration INTEGER NOT NULL DEFAULT 30,
-- 字段：CurrentSeason — 当前赛季编号
    CurrentSeason INTEGER NOT NULL DEFAULT 1,
-- 字段：SeasonStartTime — 当前赛季开始时间
    SeasonStartTime TEXT NULL,
-- 字段：SortOrder — 排序顺序，数值越小越靠前
    SortOrder INTEGER NOT NULL DEFAULT 0,
-- 字段：IsEnabled — 是否启用该排行榜
    IsEnabled INTEGER NOT NULL DEFAULT 1,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
);
-- 表：ranking_entry
-- 排行榜条目实体 记录排行榜中的玩家排名数据
CREATE TABLE ranking_entry (
-- 字段：GID — 记录唯一标识符
    GID TEXT PRIMARY KEY,
-- 字段：RankingId — 所属排行榜ID
    RankingId TEXT NOT NULL,
-- 字段：PlayerId — 玩家ID
    PlayerId TEXT NOT NULL,
-- 字段：PlayerName — 玩家名称
    PlayerName TEXT NOT NULL,
-- 字段：PlayerLevel — 玩家等级
    PlayerLevel INTEGER NOT NULL DEFAULT 1,
-- 字段：Score — 排行分数
    Score INTEGER NOT NULL DEFAULT 0,
-- 字段：Rank — 当前排名
    Rank INTEGER NOT NULL DEFAULT 0,
-- 字段：LastRank — 上次排名，用于显示排名变化
    LastRank INTEGER NOT NULL DEFAULT 0,
-- 字段：Season — 当前赛季编号
    Season INTEGER NOT NULL DEFAULT 1,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL
);
-- 表：ranking_history
-- 排行榜历史实体 存储排行榜的历史快照数据
CREATE TABLE ranking_history (
-- 字段：GID — 历史记录唯一标识符
    GID TEXT PRIMARY KEY,
-- 字段：RankingId — 所属排行榜ID
    RankingId TEXT NOT NULL,
-- 字段：SnapshotType — 快照类型：0-手动，1-每日，2-每周，3-赛季结束
    SnapshotType INTEGER NOT NULL DEFAULT 0,
-- 字段：Season — 快照所属赛季
    Season INTEGER NOT NULL DEFAULT 1,
-- 字段：Description — 快照描述
    Description TEXT NULL,
-- 字段：SnapshotData — 快照数据的JSON序列化
    SnapshotData TEXT NULL,
    CreateTime TEXT NOT NULL
);
-- 表：ranking_outbox
-- 数据表：ranking outbox（系统数据库表）
CREATE TABLE ranking_outbox (
-- 字段：EventId — 标识字段
    EventId TEXT PRIMARY KEY,
-- 字段：PlayerId — 标识字段
    PlayerId TEXT NOT NULL,
-- 字段：RankingIdsJson — JSON 序列化配置或扩展数据
    RankingIdsJson TEXT NOT NULL DEFAULT '[]',
-- 字段：Reason — 描述信息
    Reason TEXT NOT NULL DEFAULT '',
-- 字段：OccurredAtUtc — occurred at utc 字段
    OccurredAtUtc TEXT NOT NULL,
-- 字段：Status — 类型或状态
    Status TEXT NOT NULL DEFAULT 'Pending',
-- 字段：RetryCount — 数量字段
    RetryCount INTEGER NOT NULL DEFAULT 0,
-- 字段：NextAttemptAtUtc — next attempt at utc 字段
    NextAttemptAtUtc TEXT NOT NULL,
-- 字段：LastError — last error 字段
    LastError TEXT NULL,
    CreatedAtUtc TEXT NOT NULL,
-- 字段：ProcessedAtUtc — processed at utc 字段
    ProcessedAtUtc TEXT NULL,
-- 字段：ProcessingStartedAtUtc — processing started at utc 字段
    ProcessingStartedAtUtc TEXT NULL
);
-- 表：ranking_reward
-- 排行榜奖励实体 存储排行榜的排名奖励配置
CREATE TABLE ranking_reward (
-- 字段：GID — 奖励记录唯一标识符
    GID TEXT PRIMARY KEY,
-- 字段：RankingId — 所属排行榜ID
    RankingId TEXT NOT NULL,
-- 字段：MinRank — 奖励适用的最小排名
    MinRank INTEGER NOT NULL DEFAULT 1,
-- 字段：MaxRank — 奖励适用的最大排名
    MaxRank INTEGER NOT NULL DEFAULT 1,
-- 字段：RewardTitle — 奖励标题/名称
    RewardTitle TEXT NOT NULL,
-- 字段：Gold — 奖励金币数量
    Gold INTEGER NOT NULL DEFAULT 0,
-- 字段：SpiritStone — 奖励灵石数量
    SpiritStone INTEGER NOT NULL DEFAULT 0,
-- 字段：Title — 奖励的称号
    Title TEXT NULL,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
);
-- 表：RealmLevelConfigs
-- 等级到境界映射配置。 一行代表一个玩家等级，对应当前境界展示、属性加成与突破规则。
CREATE TABLE RealmLevelConfigs (
-- 字段：Level — 对应玩家等级，直接作为主键。
    Level INTEGER PRIMARY KEY,
-- 字段：RealmName — 大境界名称，例如练气、筑基。
    RealmName TEXT NOT NULL,
-- 字段：Alias — 前端直接显示的别名，例如练气一层。
    Alias TEXT NOT NULL,
-- 字段：RealmOrder — 大境界排序。
    RealmOrder INTEGER NOT NULL DEFAULT 1,
-- 字段：Layer — 当前境界中的层数。
    Layer INTEGER NOT NULL DEFAULT 1,
-- 字段：RequiredExp — 当前等级升到下一级所需经验。
    RequiredExp INTEGER NOT NULL DEFAULT 100,
-- 字段：AttributeBonusPercent — 当前等级对应的总属性加成百分比。
    AttributeBonusPercent INTEGER NOT NULL DEFAULT 0,
-- 字段：IsBreakthroughPoint — 当前等级是否为突破关口。
    IsBreakthroughPoint INTEGER NOT NULL DEFAULT 0,
-- 字段：BreakthroughSuccessRate — 突破成功率，百分比整数。
    BreakthroughSuccessRate INTEGER NOT NULL DEFAULT 100,
-- 字段：BreakthroughExpLossPercent — 突破失败时扣除的当前等级经验百分比。
    BreakthroughExpLossPercent INTEGER NOT NULL DEFAULT 0,
-- 字段：BreakthroughMaterialsJson — 突破材料 JSON。
    BreakthroughMaterialsJson TEXT NULL,
-- 字段：Description — 说明文案。
    Description TEXT NULL,
-- 字段：SeedKey — 内置稳定键。
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 是否为内置配置。
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置版本号。
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间。
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
);
-- 表：RedeemCodeConfigs
-- 兑换码奖励配置实体。
CREATE TABLE RedeemCodeConfigs (
-- 字段：Code — 兑换码文本。
    Code TEXT PRIMARY KEY,
-- 字段：SeedKey — 内置种子键。
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 是否为系统内置配置。
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本。
    BuiltInVersion TEXT NULL,
-- 字段：ConfigVersion — 当前配置版本号。
    ConfigVersion TEXT NOT NULL,
-- 字段：IsEnabled — 是否启用该兑换码。
    IsEnabled INTEGER NOT NULL DEFAULT 1,
-- 字段：Description — 兑换码说明。
    Description TEXT NOT NULL,
-- 字段：RewardJson — 奖励内容的 JSON 序列化结果。
    RewardJson TEXT NOT NULL,
-- 字段：LastUpdateTime — 最后更新时间。
    LastUpdateTime TEXT NOT NULL
);
-- 表：RedeemCodeUsages
-- 中文注释： 兑换码使用记录表。 当前项目里的兑换码配置先放在后端代码中维护，但“谁兑换过、什么时候兑换、兑换到了什么”必须真实落库， 否则页面刷新后无法判断玩家是否已经领过，也无法阻止重复兑换。
CREATE TABLE RedeemCodeUsages (
-- 字段：UsageId — 标识字段
    UsageId TEXT PRIMARY KEY,
-- 字段：PlayerId — 标识字段
    PlayerId TEXT NOT NULL,
-- 字段：Code — code 字段
    Code TEXT NOT NULL,
-- 字段：RewardSnapshotJson — JSON 序列化配置或扩展数据
    RewardSnapshotJson TEXT NULL,
-- 字段：RedeemedAt — 时间或日期字段
    RedeemedAt TEXT NOT NULL
);
-- 表：refresh_token
-- 刷新令牌实体 用于存储JWT刷新令牌，支持令牌持久化和撤销管理
CREATE TABLE refresh_token (
-- 字段：GID — 令牌唯一标识符
    GID TEXT PRIMARY KEY,
-- 字段：UserId — 关联的用户ID
    UserId TEXT NOT NULL,
-- 字段：Token — 刷新令牌值
    Token TEXT NOT NULL,
-- 字段：ExpiresAt — 令牌过期时间
    ExpiresAt TEXT NOT NULL,
    CreateTime TEXT NOT NULL,
-- 字段：IsRevoked — 是否已被撤销
    IsRevoked INTEGER NOT NULL DEFAULT 0,
-- 字段：RevokedAt — 撤销时间
    RevokedAt TEXT NULL,
-- 字段：DeviceInfo — 创建令牌时的设备信息
    DeviceInfo TEXT NULL,
-- 字段：IpAddress — 创建令牌时的IP地址
    IpAddress TEXT NULL
);
-- 表：sect_blessing_configs
-- 宗门福利配置实体
CREATE TABLE sect_blessing_configs (
-- 字段：BlessingId — 标识字段
    BlessingId TEXT PRIMARY KEY,
-- 字段：Name — 名称
    Name TEXT NOT NULL,
-- 字段：Description — 描述信息
    Description TEXT NULL,
-- 字段：RequiredGuildLevel — 等级或层级
    RequiredGuildLevel INTEGER NOT NULL DEFAULT 1,
-- 字段：BuffId — 标识字段
    BuffId TEXT NOT NULL,
-- 字段：SortOrder — 排序或优先级
    SortOrder INTEGER NOT NULL DEFAULT 0,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL
);
-- 表：sect_boss_instances
-- 宗门Boss实例实体
CREATE TABLE sect_boss_instances (
-- 字段：InstanceId — 标识字段
    InstanceId TEXT PRIMARY KEY,
-- 字段：GuildId — 标识字段
    GuildId TEXT NOT NULL,
-- 字段：BossId — 标识字段
    BossId TEXT NOT NULL,
-- 字段：BossName — 名称
    BossName TEXT NOT NULL,
-- 字段：MonsterTemplateId — 标识字段
    MonsterTemplateId TEXT NOT NULL,
-- 字段：State — 类型或状态
    State INTEGER NOT NULL DEFAULT 1,
-- 字段：SpawnedAtUtc — spawned at utc 字段
    SpawnedAtUtc TEXT NOT NULL,
-- 字段：EndAtUtc — end at utc 字段
    EndAtUtc TEXT NOT NULL,
-- 字段：CurrentHp — current hp 字段
    CurrentHp INTEGER NOT NULL DEFAULT 0,
-- 字段：MaxHp — max hp 字段
    MaxHp INTEGER NOT NULL DEFAULT 0,
-- 字段：CombatStateJson — JSON 序列化配置或扩展数据
    CombatStateJson TEXT NOT NULL DEFAULT '{}',
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL
);
-- 表：sect_boss_templates
-- 宗门Boss模板实体
CREATE TABLE sect_boss_templates (
-- 字段：BossId — 标识字段
    BossId TEXT PRIMARY KEY,
-- 字段：Name — 名称
    Name TEXT NOT NULL,
-- 字段：MonsterTemplateId — 标识字段
    MonsterTemplateId TEXT NOT NULL,
-- 字段：PortraitPath — portrait path 字段
    PortraitPath TEXT NULL,
-- 字段：IsEnabled — 状态标记
    IsEnabled INTEGER NOT NULL DEFAULT 1,
-- 字段：DurationMinutes — duration minutes 字段
    DurationMinutes INTEGER NOT NULL DEFAULT 30,
-- 字段：ParticipationRewardContribution — participation reward contribution 字段
    ParticipationRewardContribution INTEGER NOT NULL DEFAULT 0,
-- 字段：Rank1RewardContribution — rank1 reward contribution 字段
    Rank1RewardContribution INTEGER NOT NULL DEFAULT 0,
-- 字段：Rank2RewardContribution — rank2 reward contribution 字段
    Rank2RewardContribution INTEGER NOT NULL DEFAULT 0,
-- 字段：Rank3RewardContribution — rank3 reward contribution 字段
    Rank3RewardContribution INTEGER NOT NULL DEFAULT 0,
-- 字段：SortOrder — 排序或优先级
    SortOrder INTEGER NOT NULL DEFAULT 0,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL
);
-- 表：sect_donation_records
-- 宗门捐献记录实体
CREATE TABLE sect_donation_records (
-- 字段：GID — 标识字段
    GID TEXT PRIMARY KEY,
-- 字段：PlayerId — 标识字段
    PlayerId TEXT NOT NULL,
-- 字段：GuildId — 标识字段
    GuildId TEXT NOT NULL,
-- 字段：GoldDonated — gold donated 字段
    GoldDonated INTEGER NOT NULL DEFAULT 0,
-- 字段：ContributionEarned — contribution earned 字段
    ContributionEarned INTEGER NOT NULL DEFAULT 0,
-- 字段：DonateTime — 时间或日期字段
    DonateTime TEXT NOT NULL,
-- 字段：DonateDate — 时间或日期字段
    DonateDate TEXT NOT NULL
);
-- 表：sect_shop_configs
-- 宗门商店配置实体
CREATE TABLE sect_shop_configs (
-- 字段：ShopId — 标识字段
    ShopId TEXT PRIMARY KEY,
-- 字段：ShopName — 名称
    ShopName TEXT NOT NULL,
-- 字段：Description — 描述信息
    Description TEXT NULL,
-- 字段：IsOpen — 状态标记
    IsOpen INTEGER NOT NULL DEFAULT 1,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL
);
-- 表：sect_shop_items
-- 宗门商店商品实体
CREATE TABLE sect_shop_items (
-- 字段：GID — 标识字段
    GID TEXT PRIMARY KEY,
-- 字段：ShopId — 标识字段
    ShopId TEXT NOT NULL,
-- 字段：ItemId — 标识字段
    ItemId TEXT NOT NULL,
-- 字段：ItemType — 类型或状态
    ItemType INTEGER NOT NULL DEFAULT 0,
-- 字段：ContributionCost — 消耗或价格
    ContributionCost INTEGER NOT NULL DEFAULT 0,
-- 字段：Stock — stock 字段
    Stock INTEGER NOT NULL DEFAULT -1,
-- 字段：DailyLimit — daily limit 字段
    DailyLimit INTEGER NOT NULL DEFAULT -1,
-- 字段：SortOrder — 排序或优先级
    SortOrder INTEGER NOT NULL DEFAULT 0,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL
);
-- 表：sect_templates
-- 宗门模板实体（预设宗门）
CREATE TABLE sect_templates (
-- 字段：SectId — 标识字段
    SectId TEXT PRIMARY KEY,
-- 字段：Name — 名称
    Name TEXT NOT NULL,
-- 字段：Description — 描述信息
    Description TEXT NULL,
-- 字段：Icon — icon 字段
    Icon TEXT NULL,
-- 字段：PortraitPath — portrait path 字段
    PortraitPath TEXT NULL,
-- 字段：HeartSutraIdsJson — JSON 序列化配置或扩展数据
    HeartSutraIdsJson TEXT NULL,
-- 字段：IsEnabled — 状态标记
    IsEnabled INTEGER NOT NULL DEFAULT 1,
-- 字段：SortOrder — 排序或优先级
    SortOrder INTEGER NOT NULL DEFAULT 0,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL
);
-- 表：sect_tournament_matches
-- 宗门大比对战记录实体
CREATE TABLE sect_tournament_matches (
-- 字段：MatchId — 标识字段
    MatchId TEXT PRIMARY KEY,
-- 字段：TournamentId — 标识字段
    TournamentId TEXT NOT NULL,
-- 字段：Player1Id — 标识字段
    Player1Id TEXT NOT NULL,
-- 字段：Player1Name — 名称
    Player1Name TEXT NOT NULL,
-- 字段：Player2Id — 标识字段
    Player2Id TEXT NOT NULL,
-- 字段：Player2Name — 名称
    Player2Name TEXT NOT NULL,
-- 字段：Round — round 字段
    Round INTEGER NOT NULL DEFAULT 0,
-- 字段：WinnerId — 标识字段
    WinnerId TEXT NULL,
-- 字段：BattleLogJson — JSON 序列化配置或扩展数据
    BattleLogJson TEXT NULL,
-- 字段：MatchTime — 时间或日期字段
    MatchTime TEXT NOT NULL
);
-- 表：sect_tournament_schedules
-- 宗门大比排期配置实体
CREATE TABLE sect_tournament_schedules (
-- 字段：ScheduleId — 标识字段
    ScheduleId TEXT PRIMARY KEY,
-- 字段：SpawnTimeText — spawn time text 字段
    SpawnTimeText TEXT NOT NULL DEFAULT '20:00',
-- 字段：TimeZoneId — 标识字段
    TimeZoneId TEXT NOT NULL DEFAULT 'China Standard Time',
-- 字段：IsEnabled — 状态标记
    IsEnabled INTEGER NOT NULL DEFAULT 1,
-- 字段：DurationMinutes — duration minutes 字段
    DurationMinutes INTEGER NOT NULL DEFAULT 60,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL
);
-- 表：sect_tournaments
-- 宗门大比实体
CREATE TABLE sect_tournaments (
-- 字段：TournamentId — 标识字段
    TournamentId TEXT PRIMARY KEY,
-- 字段：GuildId — 标识字段
    GuildId TEXT NOT NULL,
-- 字段：StartTime — 时间或日期字段
    StartTime TEXT NOT NULL,
-- 字段：EndTime — 时间或日期字段
    EndTime TEXT NOT NULL,
-- 字段：State — 类型或状态
    State INTEGER NOT NULL DEFAULT 0,
-- 字段：ResultsJson — JSON 序列化配置或扩展数据
    ResultsJson TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL
);
-- 表：shop_config
-- 商店配置实体 存储商店的基本配置信息
CREATE TABLE shop_config (
-- 字段：ShopId — 商店唯一标识符
    ShopId TEXT PRIMARY KEY,
-- 字段：ShopName — 商店名称
    ShopName TEXT NOT NULL,
-- 字段：ShopType — 商店类型：0-普通商店，1-铁匠铺，2-神秘商店，3-VIP商店
    ShopType INTEGER NOT NULL DEFAULT 0,
-- 字段：Description — 商店描述
    Description TEXT NULL,
-- 字段：RequiredLevel — 进入商店所需的最低等级
    RequiredLevel INTEGER NOT NULL DEFAULT 1,
-- 字段：RequiredVipLevel — 进入商店所需的VIP等级
    RequiredVipLevel INTEGER NOT NULL DEFAULT 0,
-- 字段：Discount — 商店折扣系数，1.0表示无折扣
    Discount REAL NOT NULL DEFAULT 1.0,
-- 字段：IsOpen — 商店是否开放
    IsOpen INTEGER NOT NULL DEFAULT 1,
-- 字段：AutoRefresh — 是否自动刷新商品
    AutoRefresh INTEGER NOT NULL DEFAULT 0,
-- 字段：RefreshIntervalHours — 自动刷新间隔（小时）
    RefreshIntervalHours INTEGER NOT NULL DEFAULT 24,
-- 字段：Icon — 商店图标资源路径
    Icon TEXT NULL,
-- 字段：SortOrder — 排序顺序，数值越小越靠前
    SortOrder INTEGER NOT NULL DEFAULT 0,
-- 字段：IsEnabled — 是否启用该商店
    IsEnabled INTEGER NOT NULL DEFAULT 1,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
);
-- 表：shop_daily_record
-- 商店每日购买记录实体 记录玩家每日的商品购买数量，用于限购功能
CREATE TABLE shop_daily_record (
-- 字段：GID — 记录唯一标识符
    GID TEXT PRIMARY KEY,
-- 字段：PlayerId — 玩家ID
    PlayerId TEXT NOT NULL,
-- 字段：ShopId — 商店ID
    ShopId TEXT NOT NULL,
-- 字段：ItemId — 商品ID
    ItemId TEXT NOT NULL,
-- 字段：PurchasedCount — 今日已购买数量
    PurchasedCount INTEGER NOT NULL DEFAULT 0,
-- 字段：RecordDate — 记录日期，用于每日重置
    RecordDate TEXT NOT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL
);
-- 表：shop_item
-- 商品实体 存储商店中销售的商品信息
CREATE TABLE shop_item (
-- 字段：GID — 商品记录唯一标识符
    GID TEXT PRIMARY KEY,
-- 字段：ShopId — 所属商店ID
    ShopId TEXT NOT NULL,
-- 字段：ItemId — 商品ID（对应道具或装备模板ID）
    ItemId TEXT NOT NULL,
-- 字段：ItemType — 商品类型：0-道具，1-装备
    ItemType INTEGER NOT NULL DEFAULT 0,
-- 字段：BasePrice — 基础价格
    BasePrice INTEGER NOT NULL DEFAULT 0,
-- 字段：CurrentPrice — 当前价格（可能因折扣而变化）
    CurrentPrice INTEGER NOT NULL DEFAULT 0,
-- 字段：Stock — 库存数量，-1表示无限库存
    Stock INTEGER NOT NULL DEFAULT -1,
-- 字段：InitialStock — 初始库存数量
    InitialStock INTEGER NOT NULL DEFAULT -1,
-- 字段：DailyLimit — 每日限购数量，-1表示无限
    DailyLimit INTEGER NOT NULL DEFAULT -1,
-- 字段：SortOrder — 排序顺序，数值越小越靠前
    SortOrder INTEGER NOT NULL DEFAULT 0,
-- 字段：IsEnabled — 是否启用该商品
    IsEnabled INTEGER NOT NULL DEFAULT 1,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
);
-- 表：SkillTemplates
-- 技能模板落库实体。
CREATE TABLE SkillTemplates (
-- 字段：SkillId — 标识字段
    SkillId INTEGER PRIMARY KEY,
-- 字段：SkillCatalog — 技能目录。legacy 为历史技能，current 为当前版本技能。
    SkillCatalog TEXT NOT NULL DEFAULT 'legacy',
-- 字段：Name — 当前技能到下一级的升级条件列表，非持久化访问器。
    Name TEXT NOT NULL,
-- 字段：Description — 描述信息
    Description TEXT NOT NULL,
-- 字段：TargetType — 类型或状态
    TargetType INTEGER NOT NULL DEFAULT 1,
-- 字段：ManaCost — 消耗或价格
    ManaCost INTEGER NOT NULL DEFAULT 0,
-- 字段：Cooldown — cooldown 字段
    Cooldown INTEGER NOT NULL DEFAULT 0,
-- 字段：DamageType — 类型或状态
    DamageType INTEGER NOT NULL DEFAULT 0,
-- 字段：HitCount — 数量字段
    HitCount INTEGER NOT NULL DEFAULT 1,
-- 字段：RangeType — 类型或状态
    RangeType INTEGER NOT NULL DEFAULT 1,
-- 字段：DamageMultiplier — damage multiplier 字段
    DamageMultiplier REAL NOT NULL DEFAULT 0,
-- 字段：TriggerChance — trigger chance 字段
    TriggerChance REAL NOT NULL DEFAULT 1,
-- 字段：HitsJson — JSON 序列化配置或扩展数据
    HitsJson TEXT NULL,
-- 字段：BuffIdsJson — JSON 序列化配置或扩展数据
    BuffIdsJson TEXT NULL,
-- 字段：AllowedProfessionsJson — JSON 序列化配置或扩展数据
    AllowedProfessionsJson TEXT NULL,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
, SkillLevel INTEGER NOT NULL DEFAULT 1, PreviousSkillId INTEGER NULL, NextSkillId INTEGER NULL, UpgradeConditionsJson TEXT NULL);
-- 表：SpiritFieldPlots
-- 灵田地块实体 对应数据库灵田地块表
CREATE TABLE SpiritFieldPlots (
-- 字段：Id — 主键ID
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
-- 字段：PlayerId — 玩家ID
    PlayerId TEXT NOT NULL,
-- 字段：PlotNumber — 地块编号（1-9）
    PlotNumber INTEGER NOT NULL DEFAULT 1,
-- 字段：Level — 地块等级
    Level INTEGER NOT NULL DEFAULT 1,
-- 字段：Status — 地块状态
    Status INTEGER NOT NULL DEFAULT 0,
-- 字段：CropTemplateId — 种植的作物模板ID
    CropTemplateId TEXT NULL,
-- 字段：PlantTime — 种植时间
    PlantTime TEXT NULL,
-- 字段：ExpectedHarvestTime — 预计成熟时间
    ExpectedHarvestTime TEXT NULL,
-- 字段：ActualHarvestTime — 实际收获时间
    ActualHarvestTime TEXT NULL,
-- 字段：SpeedUpCount — 加速次数
    SpeedUpCount INTEGER NOT NULL DEFAULT 0,
-- 字段：SpeedUpDuration — 加速总时间（秒）
    SpeedUpDuration INTEGER NOT NULL DEFAULT 0,
-- 字段：YieldBonusPercent — 产量加成百分比
    YieldBonusPercent INTEGER NOT NULL DEFAULT 0,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL
);
-- 表：SpiritFieldSpeedUpItemConfigs
-- 灵田催熟道具规则配置。
CREATE TABLE SpiritFieldSpeedUpItemConfigs (
-- 字段：ItemId — 标识字段
    ItemId TEXT PRIMARY KEY,
-- 字段：SpeedUpSeconds — speed up seconds 字段
    SpeedUpSeconds INTEGER NOT NULL DEFAULT 3600,
-- 字段：SortOrder — 排序或优先级
    SortOrder INTEGER NOT NULL DEFAULT 0,
-- 字段：IsEnabled — 状态标记
    IsEnabled INTEGER NOT NULL DEFAULT 1,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL
);
-- 表：SpiritFieldSystemConfigs
-- 灵田系统规则配置。
CREATE TABLE SpiritFieldSystemConfigs (
-- 字段：ConfigId — 标识字段
    ConfigId TEXT PRIMARY KEY,
-- 字段：DefaultFieldLevel — 等级或层级
    DefaultFieldLevel INTEGER NOT NULL DEFAULT 1,
-- 字段：DefaultUnlockedPlots — default unlocked plots 字段
    DefaultUnlockedPlots INTEGER NOT NULL DEFAULT 3,
-- 字段：DefaultMaxPlots — default max plots 字段
    DefaultMaxPlots INTEGER NOT NULL DEFAULT 9,
-- 字段：DefaultInventoryCapacity — default inventory capacity 字段
    DefaultInventoryCapacity INTEGER NOT NULL DEFAULT 100,
-- 字段：PlotUpgradeGoldPerLevel — 等级或层级
    PlotUpgradeGoldPerLevel INTEGER NOT NULL DEFAULT 1000,
-- 字段：PlotUpgradeSpiritStonePerLevel — 等级或层级
    PlotUpgradeSpiritStonePerLevel INTEGER NOT NULL DEFAULT 10,
-- 字段：PlotUpgradeYieldBonusPerLevel — 等级或层级
    PlotUpgradeYieldBonusPerLevel INTEGER NOT NULL DEFAULT 5,
-- 字段：IsEnabled — 状态标记
    IsEnabled INTEGER NOT NULL DEFAULT 1,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL
);
-- 表：SpiritFieldSystems
-- 灵田系统数据实体 对应数据库灵田系统表
CREATE TABLE SpiritFieldSystems (
-- 字段：Id — 主键ID
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
-- 字段：PlayerId — 玩家ID
    PlayerId TEXT NOT NULL,
-- 字段：FieldLevel — 灵田等级
    FieldLevel INTEGER NOT NULL DEFAULT 1,
-- 字段：UnlockedPlots — 已解锁地块数量
    UnlockedPlots INTEGER NOT NULL DEFAULT 1,
-- 字段：MaxPlots — 最大地块数量
    MaxPlots INTEGER NOT NULL DEFAULT 9,
-- 字段：GlobalYieldBonus — 全局产量加成百分比
    GlobalYieldBonus INTEGER NOT NULL DEFAULT 0,
-- 字段：GlobalGrowthSpeedBonus — 全局生长速度加成百分比
    GlobalGrowthSpeedBonus INTEGER NOT NULL DEFAULT 0,
-- 字段：TodayPlantCount — 今日已种植次数
    TodayPlantCount INTEGER NOT NULL DEFAULT 0,
-- 字段：TodayHarvestCount — 今日已收获次数
    TodayHarvestCount INTEGER NOT NULL DEFAULT 0,
-- 字段：DailyResetTime — 今日统计重置时间
    DailyResetTime TEXT NOT NULL,
-- 字段：TotalPlantCount — 累计种植次数
    TotalPlantCount INTEGER NOT NULL DEFAULT 0,
-- 字段：TotalHarvestCount — 累计收获次数
    TotalHarvestCount INTEGER NOT NULL DEFAULT 0,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL
);
-- 表：StarterPackageConfigs
-- 新手礼包配置实体。
CREATE TABLE StarterPackageConfigs (
-- 字段：PackageId — 标识字段
    PackageId TEXT PRIMARY KEY,
-- 字段：Name — 名称
    Name TEXT NOT NULL,
-- 字段：Description — 描述信息
    Description TEXT NULL,
-- 字段：IsEnabled — 状态标记
    IsEnabled INTEGER NOT NULL DEFAULT 1,
-- 字段：AutoGrantOnRegister — 状态标记
    AutoGrantOnRegister INTEGER NOT NULL DEFAULT 0,
-- 字段：SortOrder — 排序或优先级
    SortOrder INTEGER NOT NULL DEFAULT 0,
-- 字段：ConfigVersion — 内置配置版本
    ConfigVersion TEXT NULL,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：Remark — 描述信息
    Remark TEXT NULL,
    CreatedBy TEXT NULL,
-- 字段：UpdatedBy — updated by 字段
    UpdatedBy TEXT NULL,
    CreateTime TEXT NOT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL
);
-- 表：StarterPackageGrantItems
-- 新手礼包道具发放项。
CREATE TABLE StarterPackageGrantItems (
-- 字段：GID — 标识字段
    GID TEXT PRIMARY KEY,
-- 字段：PackageId — 标识字段
    PackageId TEXT NOT NULL,
-- 字段：ItemId — 标识字段
    ItemId TEXT NOT NULL,
-- 字段：Quantity — 数量字段
    Quantity INTEGER NOT NULL DEFAULT 1,
-- 字段：IsBound — 状态标记
    IsBound INTEGER NOT NULL DEFAULT 1,
-- 字段：SortOrder — 排序或优先级
    SortOrder INTEGER NOT NULL DEFAULT 0
);
-- 表：StarterPackageGrantSkills
-- 新手礼包技能发放项。
CREATE TABLE StarterPackageGrantSkills (
-- 字段：GID — 标识字段
    GID TEXT PRIMARY KEY,
-- 字段：PackageId — 标识字段
    PackageId TEXT NOT NULL,
-- 字段：SkillId — 标识字段
    SkillId INTEGER NOT NULL,
-- 字段：SortOrder — 排序或优先级
    SortOrder INTEGER NOT NULL DEFAULT 0
);
-- 表：SystemConfigVersions
-- 运行时配置域版本与刷新状态。
CREATE TABLE SystemConfigVersions (
-- 字段：ConfigDomain — 配置域唯一键。
    ConfigDomain TEXT PRIMARY KEY,
-- 字段：CurrentVersion — 当前版本号。
    CurrentVersion TEXT NULL,
-- 字段：LastAppliedAt — 最近一次应用时间。
    LastAppliedAt TEXT NULL,
-- 字段：LastAppliedBy — 最近一次应用人。
    LastAppliedBy TEXT NULL,
-- 字段：LastRefreshStatus — 最近一次刷新状态。
    LastRefreshStatus TEXT NOT NULL DEFAULT '未执行',
-- 字段：LastRefreshMessage — 最近一次刷新消息。
    LastRefreshMessage TEXT NULL,
-- 字段：RefreshCount — 累计刷新次数。
    RefreshCount INTEGER NOT NULL DEFAULT 0,
    CreateTime TEXT NOT NULL,
-- 字段：LastUpdateTime — 最后更新时间。
    LastUpdateTime TEXT NOT NULL
);
-- 表：text_collection_bonus
-- 文字图鉴属性加成实体
CREATE TABLE text_collection_bonus (
-- 字段：BonusId — 标识字段
    BonusId TEXT PRIMARY KEY,
-- 字段：SeriesId — 标识字段
    SeriesId TEXT NOT NULL,
-- 字段：AttrType — 类型或状态
    AttrType TEXT NOT NULL,
-- 字段：AttrValue — attr value 字段
    AttrValue REAL NOT NULL DEFAULT 0,
-- 字段：ValueType — 属性值类型：0-固定值，1-百分比
    ValueType INTEGER NOT NULL DEFAULT 0,
-- 字段：SortOrder — 排序或优先级
    SortOrder INTEGER NOT NULL DEFAULT 0,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
);
-- 表：text_collection_item
-- 文字图鉴项实体
CREATE TABLE text_collection_item (
-- 字段：ItemId — 标识字段
    ItemId TEXT PRIMARY KEY,
-- 字段：SeriesId — 标识字段
    SeriesId TEXT NOT NULL,
-- 字段：Character — character 字段
    Character TEXT NOT NULL,
-- 字段：SlotIndex — slot index 字段
    SlotIndex INTEGER NOT NULL DEFAULT 0,
-- 字段：SortOrder — 排序或优先级
    SortOrder INTEGER NOT NULL DEFAULT 0,
-- 字段：IsEnabled — 状态标记
    IsEnabled INTEGER NOT NULL DEFAULT 1,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
);
-- 表：text_collection_series
-- 文字图鉴系列实体
CREATE TABLE text_collection_series (
-- 字段：SeriesId — 标识字段
    SeriesId TEXT PRIMARY KEY,
-- 字段：Name — 名称
    Name TEXT NOT NULL,
-- 字段：Description — 描述信息
    Description TEXT NULL,
-- 字段：Icon — icon 字段
    Icon TEXT NULL,
-- 字段：SortOrder — 排序或优先级
    SortOrder INTEGER NOT NULL DEFAULT 0,
-- 字段：IsEnabled — 状态标记
    IsEnabled INTEGER NOT NULL DEFAULT 1,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
);
-- 表：TitleTemplates
-- 称号模板实体。
CREATE TABLE TitleTemplates (
-- 字段：Id — 主键。
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
-- 字段：TitleId — 称号唯一标识。
    TitleId TEXT NOT NULL,
-- 字段：Name — 称号名称。
    Name TEXT NOT NULL,
-- 字段：Description — 描述。
    Description TEXT NULL,
-- 字段：Source — 来源类型（Achievement/Arena/Tower/Admin）。
    Source TEXT NULL,
-- 字段：Rarity — 稀有度（Common/Rare/Epic/Legendary）。
    Rarity TEXT NOT NULL DEFAULT 'common',
-- 字段：IconPath — 小图标路径。
    IconPath TEXT NULL,
-- 字段：ImagePath — 称号图片路径（徽章/边框图）。
    ImagePath TEXT NULL,
-- 字段：IsVisible — 是否在称号列表中可见。
    IsVisible INTEGER NOT NULL DEFAULT 1,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
, SourceId TEXT NULL, CreatedAt TEXT NOT NULL DEFAULT '');
-- 表：token_blacklist
-- 令牌黑名单实体 用于存储已失效的访问令牌，支持登出后令牌失效
CREATE TABLE token_blacklist (
-- 字段：GID — 记录唯一标识符
    GID TEXT PRIMARY KEY,
-- 字段：AccessToken — 被加入黑名单的访问令牌
    AccessToken TEXT NOT NULL,
-- 字段：UserId — 关联的用户ID
    UserId TEXT NOT NULL,
-- 字段：ExpiresAt — 令牌过期时间，过期后自动清理
    ExpiresAt TEXT NOT NULL,
    CreateTime TEXT NOT NULL,
-- 字段：Reason — 加入黑名单的原因
    Reason TEXT NULL
);
-- 表：TowerBattleLogs
-- 通天塔战斗日志实体。
CREATE TABLE TowerBattleLogs (
-- 字段：Id — 标识字段
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
-- 字段：PlayerId — 标识字段
    PlayerId TEXT NOT NULL,
-- 字段：Floor — 等级或层级
    Floor INTEGER NOT NULL,
-- 字段：IsWin — 状态标记
    IsWin INTEGER NOT NULL DEFAULT 0,
-- 字段：BattleLogJson — JSON 序列化配置或扩展数据
    BattleLogJson TEXT NULL,
    CreatedAt TEXT NOT NULL
);
-- 表：TowerFloorConfigs
-- 通天塔楼层配置实体。
CREATE TABLE TowerFloorConfigs (
-- 字段：Id — 标识字段
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
-- 字段：Floor — 等级或层级
    Floor INTEGER NOT NULL,
-- 字段：MonsterTemplateIdsJson — JSON 序列化配置或扩展数据
    MonsterTemplateIdsJson TEXT NOT NULL DEFAULT '[]',
-- 字段：MonsterCount — 数量字段
    MonsterCount INTEGER NOT NULL DEFAULT 1,
-- 字段：StatMultiplier — stat multiplier 字段
    StatMultiplier REAL NOT NULL DEFAULT 1.0,
-- 字段：RewardGold — reward gold 字段
    RewardGold INTEGER NOT NULL DEFAULT 0,
-- 字段：RewardExp — reward exp 字段
    RewardExp INTEGER NOT NULL DEFAULT 0,
-- 字段：MilestoneRewardJson — JSON 序列化配置或扩展数据
    MilestoneRewardJson TEXT NULL,
-- 字段：SeedKey — 内置种子唯一标识
    SeedKey TEXT NULL,
-- 字段：IsBuiltIn — 状态标记
    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
-- 字段：BuiltInVersion — 内置配置版本
    BuiltInVersion TEXT NULL,
-- 字段：LastUpdateTime — 最后更新时间
    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
);
-- 表：TowerProgress
-- 通天塔进度实体。
CREATE TABLE TowerProgress (
-- 字段：Id — 标识字段
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
-- 字段：PlayerId — 标识字段
    PlayerId TEXT NOT NULL,
-- 字段：HighestFloor — highest floor 字段
    HighestFloor INTEGER NOT NULL DEFAULT 0,
-- 字段：CurrentFloor — current floor 字段
    CurrentFloor INTEGER NOT NULL DEFAULT 1,
-- 字段：DailyAttemptsUsed — daily attempts used 字段
    DailyAttemptsUsed INTEGER NOT NULL DEFAULT 0,
-- 字段：DailyAttemptsPurchased — daily attempts purchased 字段
    DailyAttemptsPurchased INTEGER NOT NULL DEFAULT 0,
-- 字段：BestClearTimeMs — best clear time ms 字段
    BestClearTimeMs INTEGER NULL,
-- 字段：LastAttemptAt — 时间或日期字段
    LastAttemptAt TEXT NULL,
-- 字段：SeasonNumber — season number 字段
    SeasonNumber INTEGER NOT NULL DEFAULT 1,
    CreatedAt TEXT NOT NULL,
-- 字段：UpdatedAt — 时间或日期字段
    UpdatedAt TEXT NOT NULL
);
-- 表：Users
-- 玩家实体。 对应数据库中的 Users 表。
CREATE TABLE Users (
-- 字段：GID — 主键，玩家唯一编号。
    GID TEXT PRIMARY KEY,
-- 字段：Name — 道号，也就是玩家展示名称。
    Name TEXT NOT NULL,
-- 字段：Profession — 玩家战斗职业。 支持：warrior / mage / body。
    Profession TEXT NOT NULL DEFAULT 'warrior',
-- 字段：CurrentTitle — 当前佩戴的玩家称号。 主要由成就奖励、排行奖励等展示型系统写入。
    CurrentTitle TEXT NULL,
-- 字段：AvatarImagePath — 玩家头像图片路径。 未上传时保持为空，前端继续回退到默认头像图标。
    AvatarImagePath TEXT NULL,
-- 字段：Account — 登录账号。
    Account TEXT NOT NULL,
-- 字段：PasswordHash — 密码哈希。
    PasswordHash TEXT NOT NULL,
-- 字段：Level — 等级。
    Level INTEGER NOT NULL DEFAULT 1,
-- 字段：Exp — 当前经验。
    Exp INTEGER NOT NULL DEFAULT 0,
-- 字段：XExp — 升级所需经验。
    XExp INTEGER NOT NULL DEFAULT 100,
-- 字段：Exp2 — 第二经验槽当前值。 当前主要作为旧成长链兼容字段保留。
    Exp2 INTEGER NOT NULL DEFAULT 0,
-- 字段：XExp2 — 第二经验槽所需值。
    XExp2 INTEGER NOT NULL DEFAULT 0,
-- 字段：SkillIdsJson — 已装备技能 ID 列表，使用 JSON 持久化。
    SkillIdsJson TEXT NULL,
-- 字段：OwnedSkillIdsJson — 已掌握技能 ID 列表，使用 JSON 持久化。 `SkillIds` 只表示当前装备技能；这里单独保留已掌握集合，避免卸下技能后丢失记录。
    OwnedSkillIdsJson TEXT NULL,
-- 字段：PetId — 当前出战灵宠实例 ID。
    PetId TEXT NULL,
-- 字段：GuildId — 公会 ID。
    GuildId TEXT NULL,
-- 字段：PassiveIdsJson — 被动 Buff ID 列表，使用 JSON 持久化。
    PassiveIdsJson TEXT NULL,
-- 字段：AttributesJson — 额外属性列表，使用 JSON 持久化。 当前主要保留给非属性点类的额外成长数据；属性点分配已迁到独立表。
    AttributesJson TEXT NULL,
-- 字段：BreakthroughBonusPercent — 下次突破额外成功率加成。 主要由突破类丹药提供，成功或失败后都会清空。
    BreakthroughBonusPercent INTEGER NOT NULL DEFAULT 0,
-- 字段：Gold — 金币，基础货币。
    Gold INTEGER NOT NULL DEFAULT 1000,
    EquipmentInventoryCapacity INTEGER NOT NULL DEFAULT 100,
    ItemInventoryCapacity INTEGER NOT NULL DEFAULT 100,
-- 字段：Honor — 荣誉点，主要用于 PVP 相关兑换。
    Honor INTEGER NOT NULL DEFAULT 0,
-- 字段：GuildContribution — 公会贡献。
    GuildContribution INTEGER NOT NULL DEFAULT 0,
-- 字段：SpiritStone — 灵石，修仙体系货币。
    SpiritStone INTEGER NOT NULL DEFAULT 0,
    CreateTime TEXT NOT NULL,
-- 字段：LastLoginTime — 最后登录时间。
    LastLoginTime TEXT NOT NULL,
-- 字段：LastUpdateTime — 最后更新时间。
    LastUpdateTime TEXT NOT NULL,
-- 字段：BattleCooldownUntilUtc — 战斗冷却截止时间（UTC）。 为空表示当前没有战斗冷却。
    BattleCooldownUntilUtc TEXT NULL,
-- 字段：BattleMode — 当前账号战斗模式。
    BattleMode INTEGER NOT NULL DEFAULT 0,
-- 字段：OfflineBattleMapId — 当前离线挂机地图编号。
    OfflineBattleMapId TEXT NULL,
-- 字段：OfflineBattleStartedAtUtc — 当前离线挂机开始时间（UTC）。
    OfflineBattleStartedAtUtc TEXT NULL,
-- 字段：OfflineBattleLastTickAtUtc — 当前离线挂机最后一次推进时间（UTC）。
    OfflineBattleLastTickAtUtc TEXT NULL,
-- 字段：OfflineBattleSummaryJson — 当前离线挂机会话的累计汇总 JSON。
    OfflineBattleSummaryJson TEXT NULL,
-- 字段：ActiveDungeonInstanceId — 当前活跃的秘境实例 ID。null 表示不在秘境中。
    ActiveDungeonInstanceId TEXT NULL,
-- 字段：IsBanned — 是否已被封禁。
    IsBanned INTEGER NOT NULL DEFAULT 0,
-- 字段：BanReason — 封禁原因。
    BanReason TEXT NULL,
-- 字段：BanExpiresAt — 封禁截止时间。 为空表示长期封禁。
    BanExpiresAt TEXT NULL,
-- 字段：TotalBattles — 总战斗场次。
    TotalBattles INTEGER NOT NULL DEFAULT 0,
-- 字段：WinBattles — 胜利场次。
    WinBattles INTEGER NOT NULL DEFAULT 0,
-- 字段：TotalKills — 总击杀数。
    TotalKills INTEGER NOT NULL DEFAULT 0,
-- 字段：MaxCombo — 最大连击数。
    MaxCombo INTEGER NOT NULL DEFAULT 0,
-- 字段：TotalGoldEarned — 累计获得金币。
    TotalGoldEarned INTEGER NOT NULL DEFAULT 1000,
-- 字段：TotalGoldSpent — 累计消耗金币。
    TotalGoldSpent INTEGER NOT NULL DEFAULT 0,
-- 字段：TotalExpEarned — 累计获得经验。
    TotalExpEarned INTEGER NOT NULL DEFAULT 0,
-- 字段：Type1 — type1 字段
    Type1 INTEGER NOT NULL DEFAULT 0,
-- 字段：Type2 — type2 字段
    Type2 INTEGER NOT NULL DEFAULT 0,
-- 字段：Type3 — type3 字段
    Type3 INTEGER NOT NULL DEFAULT 0,
-- 字段：Type4 — type4 字段
    Type4 INTEGER NOT NULL DEFAULT 0,
-- 字段：Type5 — type5 字段
    Type5 INTEGER NOT NULL DEFAULT 0,
-- 字段：Type6 — type6 字段
    Type6 INTEGER NOT NULL DEFAULT 0,
-- 字段：Type7 — type7 字段
    Type7 INTEGER NOT NULL DEFAULT 0,
-- 字段：Type8 — type8 字段
    Type8 REAL NOT NULL DEFAULT 0,
-- 字段：Type9 — type9 字段
    Type9 REAL NOT NULL DEFAULT 0,
-- 字段：Type10 — type10 字段
    Type10 REAL NOT NULL DEFAULT 0,
-- 字段：Type11 — type11 字段
    Type11 REAL NOT NULL DEFAULT 1.5,
-- 字段：Type12 — type12 字段
    Type12 REAL NOT NULL DEFAULT 0,
-- 字段：Type13 — type13 字段
    Type13 REAL NOT NULL DEFAULT 0,
-- 字段：Type14 — type14 字段
    Type14 REAL NOT NULL DEFAULT 0,
-- 字段：Type15 — type15 字段
    Type15 REAL NOT NULL DEFAULT 0,
-- 字段：Element — element 字段
    Element INTEGER NOT NULL DEFAULT 0,
-- 字段：IsDeleted — 是否删除，使用软删除标记。
    IsDeleted INTEGER NOT NULL DEFAULT 0
, PillBonusAttributesJson TEXT NULL);
-- 表：WorldBossInstances
-- 世界 Boss 活动实例。
CREATE TABLE WorldBossInstances (
-- 字段：InstanceId — 世界 Boss 实例唯一编号。 每次刷新或后台手动生成都会创建新实例。
    InstanceId TEXT PRIMARY KEY,
-- 字段：BossId — 本次实例使用的 Boss 模板编号。
    BossId TEXT NOT NULL,
-- 字段：BossName — 本次实例的 Boss 展示名称。 生成时会把模板名称固化下来，避免后续编辑模板影响历史实例。
    BossName TEXT NOT NULL,
-- 字段：MonsterTemplateId — 本次实例绑定的怪物模板编号。
    MonsterTemplateId TEXT NOT NULL,
-- 字段：PortraitPath — 本次实例使用的 Boss 画像路径快照。
    PortraitPath TEXT NULL,
-- 字段：NoticeText — 本次实例的公告文案快照。
    NoticeText TEXT NULL,
-- 字段：State — 当前实例状态。
    State INTEGER NOT NULL DEFAULT 1,
-- 字段：SpawnedAtUtc — 实例实际生成时间，使用 UTC 记录。
    SpawnedAtUtc TEXT NOT NULL,
-- 字段：EndAtUtc — 实例预定结束时间，使用 UTC 记录。
    EndAtUtc TEXT NOT NULL,
-- 字段：SettledAtUtc — 实例结算完成时间，使用 UTC 记录。
    SettledAtUtc TEXT NULL,
-- 字段：CurrentHp — Boss 当前气血值。 运行时每次保存快照都会同步到该字段。
    CurrentHp INTEGER NOT NULL DEFAULT 0,
-- 字段：MaxHp — Boss 最大气血值。
    MaxHp INTEGER NOT NULL DEFAULT 0,
-- 字段：CurrentMp — Boss 当前灵力值。
    CurrentMp INTEGER NOT NULL DEFAULT 0,
-- 字段：MaxMp — Boss 最大灵力值。
    MaxMp INTEGER NOT NULL DEFAULT 0,
-- 字段：CombatStateJson — 世界 Boss 完整战斗态的 JSON 快照。 包含 Boss、参战玩家、镜像和 Buff 等运行时信息。
    CombatStateJson TEXT NOT NULL,
-- 字段：LastUpdateTime — 实例最后一次持久化时间。
    LastUpdateTime TEXT NOT NULL
);
-- 表：WorldBossLogs
-- 世界 Boss 战斗日志。
CREATE TABLE WorldBossLogs (
-- 字段：LogId — 战斗日志唯一编号。
    LogId TEXT PRIMARY KEY,
-- 字段：InstanceId — 所属世界 Boss 实例编号。
    InstanceId TEXT NOT NULL,
-- 字段：Seq — 实例内递增的日志序号。 前端会按该值倒序展示日志。
    Seq INTEGER NOT NULL DEFAULT 0,
-- 字段：TimestampUtc — 日志产生时间，使用 UTC 记录。
    TimestampUtc TEXT NOT NULL,
-- 字段：ActorId — 动作发起者编号。 系统日志可能为空。
    ActorId TEXT NULL,
-- 字段：ActorName — 动作发起者名称。
    ActorName TEXT NULL,
-- 字段：TargetId — 动作目标编号。
    TargetId TEXT NULL,
-- 字段：TargetName — 动作目标名称。
    TargetName TEXT NULL,
-- 字段：ActionType — 日志动作类型。 当前会使用 spawn、auto、skill、normal 等短标识。
    ActionType TEXT NOT NULL,
-- 字段：Content — 给前端直接展示的日志正文。
    Content TEXT NOT NULL
);
-- 表：WorldBossParticipants
-- 世界 Boss 参战者记录。
CREATE TABLE WorldBossParticipants (
-- 字段：ParticipantId — 参战记录唯一编号。
    ParticipantId TEXT PRIMARY KEY,
-- 字段：InstanceId — 所属世界 Boss 实例编号。
    InstanceId TEXT NOT NULL,
-- 字段：PlayerId — 参战玩家编号。
    PlayerId TEXT NOT NULL,
-- 字段：PlayerName — 玩家加入战斗时的展示名称快照。
    PlayerName TEXT NOT NULL,
-- 字段：IsAuto — 当前是否处于自动战斗模式。 手动窗口超时后也会被系统强制改成真。
    IsAuto INTEGER NOT NULL DEFAULT 0,
-- 字段：TotalDamage — 本场累计造成的总伤害。 排名和参与奖励都会基于这个值计算。
    TotalDamage INTEGER NOT NULL DEFAULT 0,
-- 字段：TotalHeal — 本场累计治疗量。 当前主要用于记录表现，尚未进入奖励结算规则。
    TotalHeal INTEGER NOT NULL DEFAULT 0,
-- 字段：DeathCount — 本场累计死亡次数。
    DeathCount INTEGER NOT NULL DEFAULT 0,
-- 字段：JoinAtUtc — 进入世界 Boss 战场的时间，使用 UTC 记录。 相同伤害时会用这个时间作为排序兜底。
    JoinAtUtc TEXT NOT NULL,
-- 字段：LastActionAtUtc — 最近一次实际出手时间，使用 UTC 记录。
    LastActionAtUtc TEXT NULL,
-- 字段：ReadyAtUtc — 下次允许出手的时间，使用 UTC 记录。 当前规则里每次出手后都会进入 5 秒冷却。
    ReadyAtUtc TEXT NOT NULL,
-- 字段：DeadlineAtUtc — 当前手动操作窗口截止时间，使用 UTC 记录。 玩家在此之前不操作就会被切换到自动战斗。
    DeadlineAtUtc TEXT NOT NULL,
-- 字段：ReviveAtUtc — 复活完成时间，使用 UTC 记录。 为空表示当前未处于死亡等待阶段。
    ReviveAtUtc TEXT NULL,
-- 字段：RewardClaimed — 当前实例的结算奖励是否已被玩家领取。
    RewardClaimed INTEGER NOT NULL DEFAULT 0,
-- 字段：LastUpdateTime — 参战记录最后一次更新时间。
    LastUpdateTime TEXT NOT NULL
);
-- 表：WorldBossRewardRecords
-- 世界 Boss 奖励结算记录。
CREATE TABLE WorldBossRewardRecords (
-- 字段：RecordId — 奖励记录唯一编号。
    RecordId TEXT PRIMARY KEY,
-- 字段：InstanceId — 所属世界 Boss 实例编号。
    InstanceId TEXT NOT NULL,
-- 字段：PlayerId — 奖励归属玩家编号。
    PlayerId TEXT NOT NULL,
-- 字段：PlayerName — 结算时玩家名称快照。
    PlayerName TEXT NOT NULL,
-- 字段：Rank — 结算排名。
    Rank INTEGER NOT NULL DEFAULT 0,
-- 字段：Damage — 结算时记录下来的总伤害。
    Damage INTEGER NOT NULL DEFAULT 0,
-- 字段：RewardExp — 本条奖励中包含的经验值数量。
    RewardExp INTEGER NOT NULL DEFAULT 0,
-- 字段：RewardGold — 本条奖励中包含的金币数量。
    RewardGold INTEGER NOT NULL DEFAULT 0,
-- 字段：RewardSpiritStone — 本条奖励中包含的灵石数量。
    RewardSpiritStone INTEGER NOT NULL DEFAULT 0,
-- 字段：IsClaimed — 该奖励是否已领取。
    IsClaimed INTEGER NOT NULL DEFAULT 0,
-- 字段：ClaimedAtUtc — 奖励领取时间，使用 UTC 记录。
    ClaimedAtUtc TEXT NULL,
    CreateTime TEXT NOT NULL
);
-- 表：WorldBossSchedules
-- 世界 Boss 每日排期。 当前首版只需要单条默认排期，但实体保留可扩展能力。
CREATE TABLE WorldBossSchedules (
-- 字段：ScheduleId — 排期记录主键。 当前系统只使用默认值 default 这一条记录。
    ScheduleId TEXT PRIMARY KEY,
-- 字段：SpawnTimeText — 每日触发刷新时间，格式固定为 HH:mm。
    SpawnTimeText TEXT NOT NULL,
-- 字段：TimeZoneId — 排期使用的时区标识。 由后台配置页面写入，用于把 UTC 时间换算成业务时间。
    TimeZoneId TEXT NOT NULL,
-- 字段：SelectionMode — 每日刷新时采用的 Boss 选择模式。
    SelectionMode INTEGER NOT NULL DEFAULT 0,
-- 字段：IsEnabled — 是否启用该排期。 关闭后后台任务不会自动生成世界 Boss。
    IsEnabled INTEGER NOT NULL DEFAULT 1,
-- 字段：LastUpdateTime — 排期最后一次编辑时间。
    LastUpdateTime TEXT NOT NULL
);
-- 表：WorldBossTemplates
-- 世界 Boss 模板。 复用怪物模板的属性与技能，这里只放活动专属配置。
CREATE TABLE WorldBossTemplates (
-- 字段：BossId — Boss 模板唯一编号。 后台新增、编辑和手动生成都会以它作为主键。
    BossId TEXT PRIMARY KEY,
-- 字段：Name — Boss 展示名称。 前后端界面、公告和日志都会直接显示这个名称。
    Name TEXT NOT NULL,
-- 字段：MonsterTemplateId — 关联的怪物模板编号。 世界 Boss 的基础属性、技能和被动都复用该怪物模板的数据。
    MonsterTemplateId TEXT NOT NULL,
-- 字段：PortraitPath — Boss 画像资源路径。 为空时前端会回退到默认图标。
    PortraitPath TEXT NULL,
-- 字段：IsEnabled — 是否允许该模板参与排期或手动生成。
    IsEnabled INTEGER NOT NULL DEFAULT 1,
-- 字段：Weight — 随机选取模式下的抽取权重。 权重越高，被随机刷新的概率越大。
    Weight INTEGER NOT NULL DEFAULT 1,
-- 字段：DurationMinutes — 单次世界 Boss 持续时长，单位为分钟。
    DurationMinutes INTEGER NOT NULL DEFAULT 60,
-- 字段：NoticeText — Boss 生成时推送到玩家日志区的公告文案。 留空时会使用系统默认公告。
    NoticeText TEXT NULL,
-- 字段：ParticipationMinDamage — 参与奖励的最低伤害门槛。 玩家总伤害达到该值后才能领取基础参与奖励。
    ParticipationMinDamage INTEGER NOT NULL DEFAULT 1,
-- 字段：ParticipationRewardExp — 参与奖励中的经验值数量。
    ParticipationRewardExp INTEGER NOT NULL DEFAULT 0,
-- 字段：ParticipationRewardGold — 参与奖励中的金币数量。
    ParticipationRewardGold INTEGER NOT NULL DEFAULT 0,
-- 字段：ParticipationRewardSpiritStone — 参与奖励中的灵石数量。
    ParticipationRewardSpiritStone INTEGER NOT NULL DEFAULT 0,
-- 字段：Rank1RewardExp — 第 1 名额外奖励的经验值数量。
    Rank1RewardExp INTEGER NOT NULL DEFAULT 0,
-- 字段：Rank1RewardGold — 第 1 名额外奖励的金币数量。
    Rank1RewardGold INTEGER NOT NULL DEFAULT 0,
-- 字段：Rank1RewardSpiritStone — 第 1 名额外奖励的灵石数量。
    Rank1RewardSpiritStone INTEGER NOT NULL DEFAULT 0,
-- 字段：Rank2RewardExp — 第 2 名额外奖励的经验值数量。
    Rank2RewardExp INTEGER NOT NULL DEFAULT 0,
-- 字段：Rank2RewardGold — 第 2 名额外奖励的金币数量。
    Rank2RewardGold INTEGER NOT NULL DEFAULT 0,
-- 字段：Rank2RewardSpiritStone — 第 2 名额外奖励的灵石数量。
    Rank2RewardSpiritStone INTEGER NOT NULL DEFAULT 0,
-- 字段：Rank3RewardExp — 第 3 名额外奖励的经验值数量。
    Rank3RewardExp INTEGER NOT NULL DEFAULT 0,
-- 字段：Rank3RewardGold — 第 3 名额外奖励的金币数量。
    Rank3RewardGold INTEGER NOT NULL DEFAULT 0,
-- 字段：Rank3RewardSpiritStone — 第 3 名额外奖励的灵石数量。
    Rank3RewardSpiritStone INTEGER NOT NULL DEFAULT 0,
-- 字段：SortOrder — 后台模板列表的排序值。
    SortOrder INTEGER NOT NULL DEFAULT 0,
-- 字段：LastUpdateTime — 模板最后一次编辑时间。
    LastUpdateTime TEXT NOT NULL
);

-- ==================== INDEXES ====================
CREATE UNIQUE INDEX idx_achievement_metric_counter_player_requirement_target ON achievement_metric_counter(PlayerId, RequirementType, TargetId);
CREATE UNIQUE INDEX idx_achievement_progress_player_achievement ON achievement_progress(PlayerId, AchievementId);
CREATE UNIQUE INDEX idx_admin_account ON AdminUsers(Account);
CREATE INDEX idx_admin_audit_create_time ON AdminAuditLogs(CreateTime);
CREATE INDEX idx_alchemy_recipe_furnace_level ON AlchemyRecipes(RequiredFurnaceLevel);
CREATE UNIQUE INDEX idx_alchemy_system_player ON AlchemySystems(PlayerId);
CREATE INDEX idx_arena_battle_logs_attacker ON ArenaBattleLogs(AttackerId);
CREATE INDEX idx_arena_battle_logs_defender ON ArenaBattleLogs(DefenderId);
CREATE UNIQUE INDEX idx_arena_players_player ON ArenaPlayers(PlayerId);
CREATE INDEX idx_arena_players_rank ON ArenaPlayers(Rank);
CREATE INDEX idx_attribute_point_configs_key ON AttributePointConfigs(Key);
CREATE INDEX idx_attribute_point_configs_level_range ON AttributePointConfigs(LevelStart, LevelEnd);
CREATE INDEX idx_chat_channel_time ON ChatMessages(ChannelType, SendTime);
CREATE UNIQUE INDEX idx_checkin_player_date ON PlayerCheckInRecords(PlayerId, CheckInDate);
CREATE INDEX idx_checkin_player_month ON PlayerCheckInRecords(PlayerId, CheckInDate);
CREATE INDEX idx_crop_templates_unlock_level ON CropTemplates(UnlockLevel);
CREATE INDEX idx_dungeon_daily_player ON DungeonInstanceDailyRecords(PlayerId);
CREATE UNIQUE INDEX idx_dungeon_daily_unique ON dungeon_daily_record(PlayerId, DungeonId, RecordDate);
CREATE INDEX idx_dungeon_event_dungeon ON DungeonEventConfigs(DungeonId);
CREATE INDEX idx_dungeon_event_type ON DungeonEventConfigs(EventType);
CREATE INDEX idx_dungeon_instance_dungeon ON DungeonInstances(DungeonId);
CREATE INDEX idx_dungeon_instance_player ON DungeonInstances(PlayerId);
CREATE INDEX idx_dungeon_instance_status ON DungeonInstances(Status);
CREATE INDEX idx_dungeon_reward_instance ON DungeonRewardPools(InstanceId);
CREATE INDEX idx_dungeon_reward_player ON DungeonRewardPools(PlayerId);
CREATE UNIQUE INDEX idx_element_relation_attacker_defender ON BattleElementRelationConfigs(AttackerElement, DefenderElement);
CREATE INDEX idx_equipment_player ON EquipmentInstances(PlayerId);
CREATE INDEX idx_equipment_template_level ON EquipmentTemplates(Level);
CREATE INDEX idx_fav_achievement_player ON FavorabilityAchievement(PlayerId);
CREATE UNIQUE INDEX idx_fav_achievement_unique ON FavorabilityAchievement(PlayerId, AchievementId);
CREATE INDEX idx_favorability_gift_log_player ON FavorabilityGiftLog(PlayerId);
CREATE INDEX idx_favorability_gift_log_target ON FavorabilityGiftLog(TargetPlayerId);
CREATE INDEX idx_feedback_attachment ON PlayerFeedbackAttachments(FeedbackId, SortOrder);
CREATE INDEX idx_feedback_history ON PlayerFeedbackStatusHistories(FeedbackId, CreatedAt);
CREATE INDEX idx_feedback_player ON PlayerFeedbacks(PlayerId, CreatedAt);
CREATE INDEX idx_feedback_status ON PlayerFeedbacks(Status, CreatedAt);
CREATE UNIQUE INDEX idx_five_element_branch_unique ON FiveElementBranchUpgradeConfigs(ElementType, TargetLevel);
CREATE UNIQUE INDEX idx_five_element_player ON FiveElementArrays(PlayerId);
CREATE UNIQUE INDEX idx_five_element_range_unique ON FiveElementBranchRuleRanges(ElementType, MinLevel, MaxLevel);
CREATE INDEX idx_forge_recipe_level ON ForgeRecipes(Level);
CREATE UNIQUE INDEX idx_forge_system_player ON ForgeSystems(PlayerId);
CREATE UNIQUE INDEX idx_gem_templates_gem_id ON GemTemplates(GemId);
CREATE INDEX idx_genius_tournament_match_tournament ON genius_tournament_matches(TournamentId);
CREATE INDEX idx_guild_member_guild ON guild_member(GuildId);
CREATE UNIQUE INDEX idx_guild_member_player ON guild_member(PlayerId);
CREATE UNIQUE INDEX idx_guild_name ON guild(Name);
CREATE INDEX idx_heart_sutra_sect ON heart_sutra_templates(SectId);
CREATE INDEX idx_image_collection_bonus_series ON image_collection_bonus(SeriesId);
CREATE INDEX idx_image_collection_item_series ON image_collection_item(SeriesId);
CREATE INDEX idx_inventory_player ON InventoryItems(PlayerId);
CREATE INDEX idx_item_chest_rewards_item ON ItemChestRewardEntries(ItemId, SortOrder);
CREATE INDEX idx_item_template_type ON ItemTemplates(Type);
CREATE INDEX idx_lottery_log_player_time ON lottery_log(PlayerId, LotteryTime);
CREATE INDEX idx_lottery_log_pool ON lottery_log(PoolId);
CREATE INDEX idx_lottery_prize_pool ON lottery_prize(PoolId);
CREATE UNIQUE INDEX idx_mail_global_claim_unique ON MailGlobalClaimRecords(MailId, PlayerId);
CREATE INDEX idx_mail_messages_global ON MailMessages(IsGlobal, ExpireAt);
CREATE INDEX idx_mail_messages_recipient ON MailMessages(RecipientId);
CREATE INDEX idx_map_template_level ON MapTemplates(Level);
CREATE UNIQUE INDEX idx_market_configs_key ON MarketConfigs(ConfigKey);
CREATE INDEX idx_market_listings_expire ON MarketListings(ExpireAt);
CREATE INDEX idx_market_listings_seller ON MarketListings(SellerId);
CREATE INDEX idx_market_listings_status ON MarketListings(Status);
CREATE INDEX idx_monster_template_level ON MonsterTemplates(Level);
CREATE INDEX idx_party_battle_party ON PartyBattleRecords(PartyId, StartedAtUtc);
CREATE UNIQUE INDEX idx_party_battle_request ON PartyBattleRecords(PartyId, RequestId);
CREATE INDEX idx_party_member_party ON PartyMembers(PartyId);
CREATE UNIQUE INDEX idx_party_member_player ON PartyMembers(PlayerId);
CREATE INDEX idx_party_target_dungeon ON Parties(TargetDungeonId);
CREATE INDEX idx_pet_instances_player ON PetInstances(PlayerId);
CREATE INDEX idx_pill_effects_expires ON PlayerPillEffects (ExpiresAt);
CREATE INDEX idx_pill_effects_player_item ON PlayerPillEffects (PlayerId, ItemId);
CREATE INDEX idx_player_attribute_allocation_player ON PlayerAttributeAllocations(PlayerId);
CREATE UNIQUE INDEX idx_player_attribute_allocation_unique ON PlayerAttributeAllocations(PlayerId, AttributeKey);
CREATE INDEX idx_player_collection_player ON player_collection(PlayerId);
CREATE UNIQUE INDEX idx_player_collection_unique ON player_collection(PlayerId, CollectionType, SeriesId, ItemId);
CREATE UNIQUE INDEX idx_player_favorability_pair ON PlayerFavorability(PlayerId, TargetPlayerId);
CREATE INDEX idx_player_favorability_player ON PlayerFavorability(PlayerId);
CREATE INDEX idx_player_favorability_target ON PlayerFavorability(TargetPlayerId);
CREATE UNIQUE INDEX idx_player_heart_sutra_unique ON player_heart_sutras(PlayerId, SutraId);
CREATE INDEX idx_player_titles_player ON PlayerTitles(PlayerId);
CREATE UNIQUE INDEX idx_player_titles_unique ON PlayerTitles(PlayerId, TitleId);
CREATE INDEX idx_quest_completed_player ON quest_completed_record(PlayerId);
CREATE UNIQUE INDEX idx_quest_progress_player_quest ON quest_progress(PlayerId, QuestId);
CREATE UNIQUE INDEX idx_ranking_entry_unique ON ranking_entry(RankingId, PlayerId, Season);
CREATE INDEX idx_ranking_outbox_player_status_attempt ON ranking_outbox(PlayerId, Status, NextAttemptAtUtc);
CREATE INDEX idx_ranking_outbox_status_attempt ON ranking_outbox(Status, NextAttemptAtUtc);
CREATE UNIQUE INDEX idx_redeem_player_code ON RedeemCodeUsages(PlayerId, Code);
CREATE INDEX idx_refresh_token_user ON refresh_token(UserId);
CREATE INDEX idx_sect_boss_instance_guild ON sect_boss_instances(GuildId);
CREATE INDEX idx_sect_donation_player_date ON sect_donation_records(PlayerId, DonateDate);
CREATE INDEX idx_sect_shop_item_shop ON sect_shop_items(ShopId);
CREATE INDEX idx_sect_tournament_guild ON sect_tournaments(GuildId);
CREATE INDEX idx_sect_tournament_match_tournament ON sect_tournament_matches(TournamentId);
CREATE UNIQUE INDEX idx_shop_daily_unique ON shop_daily_record(PlayerId, ItemId, RecordDate);
CREATE INDEX idx_shop_item_shop ON shop_item(ShopId);
CREATE UNIQUE INDEX idx_spirit_field_plot_player_plot ON SpiritFieldPlots(PlayerId, PlotNumber);
CREATE UNIQUE INDEX idx_spirit_field_system_player ON SpiritFieldSystems(PlayerId);
CREATE INDEX idx_starter_package_item_package ON StarterPackageGrantItems(PackageId);
CREATE INDEX idx_starter_package_skill_package ON StarterPackageGrantSkills(PackageId);
CREATE INDEX idx_starter_package_sort ON StarterPackageConfigs(SortOrder);
CREATE INDEX idx_system_config_versions_update_time ON SystemConfigVersions(LastUpdateTime);
CREATE INDEX idx_text_collection_bonus_series ON text_collection_bonus(SeriesId);
CREATE INDEX idx_text_collection_item_series ON text_collection_item(SeriesId);
CREATE UNIQUE INDEX idx_title_templates_title_id ON TitleTemplates(TitleId);
CREATE INDEX idx_tower_battle_logs_player ON TowerBattleLogs(PlayerId);
CREATE UNIQUE INDEX idx_tower_floor_configs_floor ON TowerFloorConfigs(Floor);
CREATE UNIQUE INDEX idx_tower_progress_player ON TowerProgress(PlayerId);
CREATE UNIQUE INDEX idx_users_account ON Users(Account);
CREATE INDEX idx_users_name ON Users(Name);
CREATE INDEX idx_world_boss_instances_state ON WorldBossInstances(State);
CREATE INDEX idx_world_boss_logs_instance_seq ON WorldBossLogs(InstanceId, Seq);
CREATE INDEX idx_world_boss_participant_instance ON WorldBossParticipants(InstanceId);
CREATE UNIQUE INDEX idx_world_boss_participant_instance_player ON WorldBossParticipants(InstanceId, PlayerId);
CREATE INDEX idx_world_boss_reward_player ON WorldBossRewardRecords(PlayerId, IsClaimed);

PRAGMA foreign_keys = ON;
