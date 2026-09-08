using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SqlSugar;
using XXX.Entity;
using XXX.Infrastructure.SeedData;

namespace XXX.Infrastructure.Data
{
    /// <summary>
    /// 数据库上下文。
    /// </summary>
    public class DbContext
    {
        private readonly ISqlSugarClient _db;
        private readonly ILogger<DbContext>? _logger;
        private readonly DbType _dbType;
        private readonly string _connectionString;

        /// <summary>
        /// 获取 SqlSugar 客户端。
        /// </summary>
        public ISqlSugarClient Db => _db;

        /// <summary>
        /// 获取当前数据库类型。
        /// </summary>
        public DbType DatabaseType => _dbType;

        /// <summary>
        /// 使用配置对象初始化数据库上下文。
        /// </summary>
        /// <param name="configuration">应用配置。</param>
        /// <param name="logger">日志记录器。</param>
        public DbContext(IConfiguration configuration, ILogger<DbContext>? logger = null)
            : this(
                configuration.GetConnectionString("DefaultConnection")
                    ?? throw new ArgumentNullException("DefaultConnection"),
                configuration["DatabaseProvider"],
                logger)
        {
        }

        /// <summary>
        /// 使用连接串直接初始化数据库上下文。
        /// </summary>
        /// <param name="connectionString">数据库连接串。</param>
        /// <param name="databaseProvider">显式指定的数据库类型。</param>
        /// <param name="logger">日志记录器。</param>
        public DbContext(string connectionString, string? databaseProvider = null, ILogger<DbContext>? logger = null)
        {
            _logger = logger;
            _dbType = ResolveDbType(connectionString, databaseProvider);
            _connectionString = _dbType == DbType.Sqlite && !connectionString.Contains("Foreign Keys=", StringComparison.OrdinalIgnoreCase)
                ? $"{connectionString};Foreign Keys=True;Default Timeout=5"
                : connectionString;

            if (_dbType == DbType.Sqlite)
            {
                EnsureSqliteDirectory(_connectionString);
            }

            // 中文注释：
            // 这里不能继续使用普通 SqlSugarClient + Singleton DbContext 的组合。
            // 原因是前端首页会并发请求商店、多个排行榜、玩家信息等接口，
            // 如果整个应用共享同一个非线程安全的 SqlSugarClient，
            // 在 SQLite 下很容易出现 DataReader 串读、字段绑定错位、连接提前关闭等随机异常。
            // 改成 SqlSugarScope 后，框架会为并发异步调用隔离实际连接上下文，
            // 这样既能保留当前 DbContext 的注入方式，也能避免并发联调时接口互相干扰。
            _db = new SqlSugarScope(new ConnectionConfig
            {
                ConnectionString = _connectionString,
                DbType = _dbType,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute,
                MoreSettings = new ConnMoreSettings
                {
                    SqlServerCodeFirstNvarchar = true
                }
            });

            _db.Aop.OnLogExecuting = (sql, pars) =>
            {
                _logger?.LogDebug("[SQL] {Sql}", sql);
            };

            _db.Aop.OnError = exp =>
            {
                _logger?.LogError(exp, "SQL 执行失败：{Message}", exp.Message);
            };

            if (_dbType == DbType.Sqlite)
            {
                ConfigureSqlitePragmas();
            }

            RegisterSoftDeleteFilters();
        }

        /// <summary>
        /// 配置 SQLite 并发和外键约束参数，确保每个应用数据库连接具备一致行为。
        /// </summary>
        private void ConfigureSqlitePragmas()
        {
            _db.Ado.ExecuteCommand("PRAGMA journal_mode = WAL;");
            _db.Ado.ExecuteCommand("PRAGMA synchronous = NORMAL;");
            _db.Ado.ExecuteCommand("PRAGMA busy_timeout = 5000;");
            _db.Ado.ExecuteCommand("PRAGMA foreign_keys = ON;");
        }

        /// <summary>
        /// 初始化数据库。
        /// </summary>
        public void InitDatabase()
        {
            if (_dbType == DbType.Sqlite)
            {
                InitSqliteCoreTables();
                return;
            }

            InitCodeFirstTables();
        }

        /// <summary>
        /// 开启事务。
        /// </summary>
        public void BeginTransaction()
        {
            _db.Ado.BeginTran();
        }

        /// <summary>
        /// 提交事务。
        /// </summary>
        public void CommitTransaction()
        {
            _db.Ado.CommitTran();
        }

        /// <summary>
        /// 回滚事务。
        /// </summary>
        public void RollbackTransaction()
        {
            _db.Ado.RollbackTran();
        }

        /// <summary>
        /// 在事务中执行带返回值的操作。
        /// </summary>
        public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> action)
        {
            try
            {
                BeginTransaction();
                var result = await action();
                CommitTransaction();
                return result;
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
        }

        /// <summary>
        /// 在事务中执行不带返回值的操作。
        /// </summary>
        public async Task ExecuteInTransactionAsync(Func<Task> action)
        {
            try
            {
                BeginTransaction();
                await action();
                CommitTransaction();
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
        }

        /// <summary>
        /// 识别数据库类型，开发环境默认走 SQLite。
        /// </summary>
        private static DbType ResolveDbType(string connectionString, string? databaseProvider)
        {
            if (!string.IsNullOrWhiteSpace(databaseProvider))
            {
                if (databaseProvider.Equals("sqlite", StringComparison.OrdinalIgnoreCase))
                {
                    return DbType.Sqlite;
                }

                if (databaseProvider.Equals("mysql", StringComparison.OrdinalIgnoreCase))
                {
                    return DbType.MySql;
                }
            }

            if (connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase) ||
                connectionString.Contains(".db", StringComparison.OrdinalIgnoreCase) ||
                connectionString.Contains(".sqlite", StringComparison.OrdinalIgnoreCase))
            {
                return DbType.Sqlite;
            }

            return DbType.MySql;
        }

        /// <summary>
        /// SQLite 文件数据库需要提前确保目录存在。
        /// </summary>
        private static void EnsureSqliteDirectory(string connectionString)
        {
            var dataSourcePart = connectionString
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault(part => part.TrimStart().StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(dataSourcePart))
            {
                return;
            }

            var filePath = dataSourcePart.Split('=', 2)[1].Trim();
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return;
            }

            var fullPath = Path.GetFullPath(filePath);
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        /// <summary>
        /// 为继承软删除基类的实体添加过滤器。
        /// </summary>
        private void RegisterSoftDeleteFilters()
        {
            var coreAssembly = typeof(UserEntity).Assembly;
            var baseEntityTypes = coreAssembly.GetTypes()
                .Where(t => typeof(BaseEntity).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                .ToList();

            foreach (var entityType in baseEntityTypes)
            {
                var filterMethod = typeof(DbContext)
                    .GetMethod(nameof(AddSoftDeleteFilter), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                    ?.MakeGenericMethod(entityType);

                filterMethod?.Invoke(null, new object[] { _db });
            }
        }

        private static void AddSoftDeleteFilter<T>(ISqlSugarClient db) where T : BaseEntity
        {
            db.QueryFilter.AddTableFilter<T>(entity => entity.IsDeleted == false);
        }

        /// <summary>
        /// 非 SQLite 环境继续使用现有 CodeFirst。
        /// </summary>
        private void InitCodeFirstTables()
        {
            var coreAssembly = typeof(UserEntity).Assembly;
            var entityTypes = coreAssembly.GetTypes()
                .Where(t => t.Namespace != null && t.Namespace.StartsWith("XXX.Entity") && t.IsClass && !t.IsAbstract)
                .ToList();

            foreach (var type in entityTypes)
            {
                try
                {
                    _db.CodeFirst.InitTables(type);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "创建数据表失败：{EntityName}", type.Name);
                }
            }
        }

        /// <summary>
        /// SQLite 开发环境手工建核心表，避免现有大批实体的方言差异阻塞联调。
        /// </summary>
        private void InitSqliteCoreTables()
        {
            var commands = new[]
            {
                """
                CREATE TABLE IF NOT EXISTS Users (
                    GID TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Profession TEXT NOT NULL DEFAULT 'warrior',
                    CurrentTitle TEXT NULL,
                    AvatarImagePath TEXT NULL,
                    Account TEXT NOT NULL,
                    PasswordHash TEXT NOT NULL,
                    Level INTEGER NOT NULL DEFAULT 1,
                    Exp INTEGER NOT NULL DEFAULT 0,
                    XExp INTEGER NOT NULL DEFAULT 100,
                    Exp2 INTEGER NOT NULL DEFAULT 0,
                    XExp2 INTEGER NOT NULL DEFAULT 0,
                    SkillIdsJson TEXT NULL,
                    OwnedSkillIdsJson TEXT NULL,
                    PetId TEXT NULL,
                    GuildId TEXT NULL,
                    PassiveIdsJson TEXT NULL,
                    AttributesJson TEXT NULL,
                    BreakthroughBonusPercent INTEGER NOT NULL DEFAULT 0,
                    Gold INTEGER NOT NULL DEFAULT 1000,
                    EquipmentInventoryCapacity INTEGER NOT NULL DEFAULT 100,
                    ItemInventoryCapacity INTEGER NOT NULL DEFAULT 100,
                    Honor INTEGER NOT NULL DEFAULT 0,
                    GuildContribution INTEGER NOT NULL DEFAULT 0,
                    SpiritStone INTEGER NOT NULL DEFAULT 0,
                    CreateTime TEXT NOT NULL,
                    LastLoginTime TEXT NOT NULL,
                    LastUpdateTime TEXT NOT NULL,
                    BattleCooldownUntilUtc TEXT NULL,
                    BattleMode INTEGER NOT NULL DEFAULT 0,
                    OfflineBattleMapId TEXT NULL,
                    OfflineBattleStartedAtUtc TEXT NULL,
                    OfflineBattleLastTickAtUtc TEXT NULL,
                    OfflineBattleEndAtUtc TEXT NULL,
                    OfflineBattleSummaryJson TEXT NULL,
                    ActiveDungeonInstanceId TEXT NULL,
                    EquipmentAutoSellMinLevel INTEGER NOT NULL DEFAULT 0,
                    EquipmentAutoSellMinQuality INTEGER NOT NULL DEFAULT 0,
                    IsBanned INTEGER NOT NULL DEFAULT 0,
                    BanReason TEXT NULL,
                    BanExpiresAt TEXT NULL,
                    TotalBattles INTEGER NOT NULL DEFAULT 0,
                    WinBattles INTEGER NOT NULL DEFAULT 0,
                    TotalKills INTEGER NOT NULL DEFAULT 0,
                    MaxCombo INTEGER NOT NULL DEFAULT 0,
                    TotalGoldEarned INTEGER NOT NULL DEFAULT 1000,
                    TotalGoldSpent INTEGER NOT NULL DEFAULT 0,
                    TotalExpEarned INTEGER NOT NULL DEFAULT 0,
                    Type1 INTEGER NOT NULL DEFAULT 0,
                    Type2 INTEGER NOT NULL DEFAULT 0,
                    Type3 INTEGER NOT NULL DEFAULT 0,
                    Type4 INTEGER NOT NULL DEFAULT 0,
                    Type5 INTEGER NOT NULL DEFAULT 0,
                    Type6 INTEGER NOT NULL DEFAULT 0,
                    Type7 INTEGER NOT NULL DEFAULT 0,
                    Type8 REAL NOT NULL DEFAULT 0,
                    Type9 REAL NOT NULL DEFAULT 0,
                    Type10 REAL NOT NULL DEFAULT 0,
                    Type11 REAL NOT NULL DEFAULT 1.5,
                    Type12 REAL NOT NULL DEFAULT 0,
                    Type13 REAL NOT NULL DEFAULT 0,
                    Type14 REAL NOT NULL DEFAULT 0,
                    Type15 REAL NOT NULL DEFAULT 0,
                    Element INTEGER NOT NULL DEFAULT 0,
                    IsDeleted INTEGER NOT NULL DEFAULT 0
                );
                """,
                "CREATE UNIQUE INDEX IF NOT EXISTS idx_users_account ON Users(Account);",
                "CREATE INDEX IF NOT EXISTS idx_users_name ON Users(Name);",
                """
                CREATE TABLE IF NOT EXISTS PlayerAttributeAllocations (
                    AllocationId TEXT PRIMARY KEY,
                    PlayerId TEXT NOT NULL,
                    AttributeKey TEXT NOT NULL,
                    AllocatedPoints INTEGER NOT NULL DEFAULT 0,
                    CreateTime TEXT NOT NULL,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                "CREATE UNIQUE INDEX IF NOT EXISTS idx_player_attribute_allocation_unique ON PlayerAttributeAllocations(PlayerId, AttributeKey);",
                "CREATE INDEX IF NOT EXISTS idx_player_attribute_allocation_player ON PlayerAttributeAllocations(PlayerId);",
                """
                CREATE TABLE IF NOT EXISTS AdminUsers (
                    AdminId TEXT PRIMARY KEY,
                    Account TEXT NOT NULL,
                    DisplayName TEXT NOT NULL,
                    PasswordHash TEXT NOT NULL,
                    Role TEXT NOT NULL,
                    IsActive INTEGER NOT NULL DEFAULT 1,
                    IsDeleted INTEGER NOT NULL DEFAULT 0,
                    CreateTime TEXT NOT NULL,
                    LastLoginTime TEXT NULL,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                "CREATE UNIQUE INDEX IF NOT EXISTS idx_admin_account ON AdminUsers(Account);",
                """
                CREATE TABLE IF NOT EXISTS AdminAuditLogs (
                    LogId TEXT PRIMARY KEY,
                    OperatorId TEXT NULL,
                    OperatorName TEXT NULL,
                    OperatorRole TEXT NULL,
                    HttpMethod TEXT NOT NULL,
                    Path TEXT NOT NULL,
                    ResourceKey TEXT NULL,
                    TargetId TEXT NULL,
                    RequestJson TEXT NULL,
                    ResponseJson TEXT NULL,
                    BeforeJson TEXT NULL,
                    AfterJson TEXT NULL,
                    DiffJson TEXT NULL,
                    StatusCode INTEGER NOT NULL DEFAULT 200,
                    Success INTEGER NOT NULL DEFAULT 1,
                    ErrorMessage TEXT NULL,
                    IpAddress TEXT NULL,
                    CreateTime TEXT NOT NULL
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_admin_audit_create_time ON AdminAuditLogs(CreateTime);",
                """
                CREATE TABLE IF NOT EXISTS SystemConfigVersions (
                    ConfigDomain TEXT PRIMARY KEY,
                    CurrentVersion TEXT NULL,
                    LastAppliedAt TEXT NULL,
                    LastAppliedBy TEXT NULL,
                    LastRefreshStatus TEXT NOT NULL DEFAULT '未执行',
                    LastRefreshMessage TEXT NULL,
                    RefreshCount INTEGER NOT NULL DEFAULT 0,
                    CreateTime TEXT NOT NULL,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_system_config_versions_update_time ON SystemConfigVersions(LastUpdateTime);",
                """
                CREATE TABLE IF NOT EXISTS InventoryItems (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PlayerId TEXT NOT NULL,
                    ItemId TEXT NOT NULL,
                    Quantity INTEGER NOT NULL DEFAULT 1,
                    IsBound INTEGER NOT NULL DEFAULT 0,
                    AcquiredTime TEXT NOT NULL,
                    ExpireTime TEXT NULL
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_inventory_player ON InventoryItems(PlayerId);",
                """
                CREATE TABLE IF NOT EXISTS RealmLevelConfigs (
                    Level INTEGER PRIMARY KEY,
                    RealmName TEXT NOT NULL,
                    Alias TEXT NOT NULL,
                    RealmOrder INTEGER NOT NULL DEFAULT 1,
                    Layer INTEGER NOT NULL DEFAULT 1,
                    RequiredExp INTEGER NOT NULL DEFAULT 100,
                    AttributeBonusPercent INTEGER NOT NULL DEFAULT 0,
                    IsBreakthroughPoint INTEGER NOT NULL DEFAULT 0,
                    BreakthroughSuccessRate INTEGER NOT NULL DEFAULT 100,
                    BreakthroughExpLossPercent INTEGER NOT NULL DEFAULT 0,
                    BreakthroughMaterialsJson TEXT NULL,
                    Description TEXT NULL,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS PlayerLevelConfigs (
                    Level INTEGER PRIMARY KEY,
                    RequiredExp INTEGER NOT NULL DEFAULT 0,
                    BaseHp INTEGER NOT NULL DEFAULT 1,
                    BaseMp INTEGER NOT NULL DEFAULT 0,
                    BasePhysicalAttack INTEGER NOT NULL DEFAULT 0,
                    BaseMagicAttack INTEGER NOT NULL DEFAULT 0,
                    BasePhysicalDefense INTEGER NOT NULL DEFAULT 0,
                    BaseMagicDefense INTEGER NOT NULL DEFAULT 0,
                    BaseSpeed INTEGER NOT NULL DEFAULT 0,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS AttributePointConfigs (
                    ConfigId TEXT PRIMARY KEY,
                    Key TEXT NOT NULL,
                    Name TEXT NOT NULL,
                    Profession TEXT NOT NULL DEFAULT 'warrior',
                    AttributeType INTEGER NOT NULL,
                    RatioPerPoint REAL NOT NULL DEFAULT 1,
                    BonusPerPoint INTEGER NOT NULL DEFAULT 1,
                    PointsPerBonus INTEGER NOT NULL DEFAULT 1,
                    LevelStart INTEGER NOT NULL,
                    LevelEnd INTEGER NOT NULL,
                    PointsGained INTEGER NOT NULL DEFAULT 0,
                    SortOrder INTEGER NOT NULL DEFAULT 0,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_attribute_point_configs_key ON AttributePointConfigs(Key);",
                "CREATE INDEX IF NOT EXISTS idx_attribute_point_configs_level_range ON AttributePointConfigs(LevelStart, LevelEnd);",
                """
                CREATE TABLE IF NOT EXISTS FiveElementArrays (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PlayerId TEXT NOT NULL,
                    ArrayLevel INTEGER NOT NULL DEFAULT 1,
                    MetalLevel INTEGER NOT NULL DEFAULT 1,
                    WoodLevel INTEGER NOT NULL DEFAULT 1,
                    WaterLevel INTEGER NOT NULL DEFAULT 1,
                    FireLevel INTEGER NOT NULL DEFAULT 1,
                    EarthLevel INTEGER NOT NULL DEFAULT 1,
                    MetalExp INTEGER NOT NULL DEFAULT 0,
                    WoodExp INTEGER NOT NULL DEFAULT 0,
                    WaterExp INTEGER NOT NULL DEFAULT 0,
                    FireExp INTEGER NOT NULL DEFAULT 0,
                    EarthExp INTEGER NOT NULL DEFAULT 0,
                    TotalSpiritPower INTEGER NOT NULL DEFAULT 0,
                    SpiritPowerRate INTEGER NOT NULL DEFAULT 20,
                    LastCollectTime TEXT NOT NULL,
                    TodayCollectCount INTEGER NOT NULL DEFAULT 0,
                    DailyResetTime TEXT NOT NULL,
                    TotalCollectedSpiritPower INTEGER NOT NULL DEFAULT 0,
                    ActiveCombinationsJson TEXT NULL,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                "CREATE UNIQUE INDEX IF NOT EXISTS idx_five_element_player ON FiveElementArrays(PlayerId);",
                """
                CREATE TABLE IF NOT EXISTS FiveElementLevelConfigs (
                    ArrayLevel INTEGER PRIMARY KEY,
                    UpgradeGoldCost INTEGER NOT NULL DEFAULT 0,
                    UpgradeSpiritStoneCost INTEGER NOT NULL DEFAULT 0,
                    UpgradeMaterialsJson TEXT NULL,
                    SpiritFieldYieldBonusPercent INTEGER NOT NULL DEFAULT 0,
                    BattleExpBonusPercent INTEGER NOT NULL DEFAULT 0,
                    ProfessionLevelCap INTEGER NOT NULL DEFAULT 1,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS FiveElementBranchUpgradeConfigs (
                    GID TEXT PRIMARY KEY,
                    ElementType TEXT NOT NULL,
                    TargetLevel INTEGER NOT NULL,
                    GoldCost INTEGER NOT NULL DEFAULT 0,
                    SpiritStoneCost INTEGER NOT NULL DEFAULT 0,
                    MaterialsJson TEXT NULL,
                    SortOrder INTEGER NOT NULL DEFAULT 0,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                "CREATE UNIQUE INDEX IF NOT EXISTS idx_five_element_branch_unique ON FiveElementBranchUpgradeConfigs(ElementType, TargetLevel);",
                "CREATE TABLE IF NOT EXISTS FiveElementBranchRuleRanges (GID TEXT PRIMARY KEY, ElementType TEXT NOT NULL, MinLevel INTEGER NOT NULL, MaxLevel INTEGER NOT NULL, AttributeType TEXT NOT NULL, BonusPerLevel INTEGER NOT NULL DEFAULT 0, GoldCost INTEGER NOT NULL DEFAULT 0, SpiritStoneCost INTEGER NOT NULL DEFAULT 0, MaterialsJson TEXT NULL, SortOrder INTEGER NOT NULL DEFAULT 0, IsEnabled INTEGER NOT NULL DEFAULT 1, SeedKey TEXT NULL, IsBuiltIn INTEGER NOT NULL DEFAULT 0, BuiltInVersion TEXT NULL, LastUpdateTime TEXT NOT NULL);",
                "CREATE UNIQUE INDEX IF NOT EXISTS idx_five_element_range_unique ON FiveElementBranchRuleRanges(ElementType, MinLevel, MaxLevel);",
                """
                CREATE TABLE IF NOT EXISTS EquipmentInstances (
                    InstanceId TEXT PRIMARY KEY,
                    PlayerId TEXT NOT NULL,
                    TemplateId TEXT NOT NULL,
                    Name TEXT NOT NULL,
                    Slot INTEGER NOT NULL,
                    Quality INTEGER NOT NULL DEFAULT 1,
                    EnhanceLevel INTEGER NOT NULL DEFAULT 0,
                    IsEquipped INTEGER NOT NULL DEFAULT 0,
                    IsBound INTEGER NOT NULL DEFAULT 0,
                    BasePhysicalAttack INTEGER NOT NULL DEFAULT 0,
                    BaseMagicAttack INTEGER NOT NULL DEFAULT 0,
                    BasePhysicalDefense INTEGER NOT NULL DEFAULT 0,
                    BaseMagicDefense INTEGER NOT NULL DEFAULT 0,
                    BaseHP INTEGER NOT NULL DEFAULT 0,
                    BaseMP INTEGER NOT NULL DEFAULT 0,
                    BonusStatsJson TEXT NULL,
                    AcquiredTime TEXT NOT NULL,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_equipment_player ON EquipmentInstances(PlayerId);",
                // 中文注释：
                // 玩家属性重算时会统一查询当前出战灵宠，用来把灵宠提供的生命、攻击、防御加成叠加回人物面板。
                // 之前 SQLite 开发库只创建了用户、背包、装备这些最小表，却漏掉了灵宠实例表。
                // 结果就是“穿戴装备 -> 提交事务成功 -> 开始重算人物属性 -> 查询 PetInstances 时报 no such table”，
                // 最终前端看到 500，但数据库里的装备状态其实已经写成功了。
                // 这里把灵宠模板表和灵宠实例表一并补齐，避免后续灵宠功能接入时再次出现同类缺表问题。
                """
                CREATE TABLE IF NOT EXISTS PetTemplates (
                    TemplateId TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Description TEXT NULL,
                    Type INTEGER NOT NULL DEFAULT 0,
                    InitialQualityMin INTEGER NOT NULL DEFAULT 1,
                    InitialQualityMax INTEGER NOT NULL DEFAULT 1,
                    MaxQuality INTEGER NOT NULL DEFAULT 5,
                    GrowthRateMin REAL NOT NULL DEFAULT 1.0,
                    GrowthRateMax REAL NOT NULL DEFAULT 1.0,
                    InitialSkillCount INTEGER NOT NULL DEFAULT 1,
                    Element INTEGER NOT NULL DEFAULT 0,
                    MinType1 INTEGER NULL,
                    MaxType1 INTEGER NULL,
                    MinType2 INTEGER NULL,
                    MaxType2 INTEGER NULL,
                    MinType3 INTEGER NULL,
                    MaxType3 INTEGER NULL,
                    MinType4 INTEGER NULL,
                    MaxType4 INTEGER NULL,
                    MinType5 INTEGER NULL,
                    MaxType5 INTEGER NULL,
                    MinType6 INTEGER NULL,
                    MaxType6 INTEGER NULL,
                    MinType7 INTEGER NULL,
                    MaxType7 INTEGER NULL,
                    MinType8 INTEGER NULL,
                    MaxType8 INTEGER NULL,
                    MinType9 INTEGER NULL,
                    MaxType9 INTEGER NULL,
                    MinType10 INTEGER NULL,
                    MaxType10 INTEGER NULL,
                    MinType11 INTEGER NULL,
                    MaxType11 INTEGER NULL,
                    MinType12 INTEGER NULL,
                    MaxType12 INTEGER NULL,
                    MinType13 INTEGER NULL,
                    MaxType13 INTEGER NULL,
                    MinType14 INTEGER NULL,
                    MaxType14 INTEGER NULL,
                    MinType15 INTEGER NULL,
                    MaxType15 INTEGER NULL,
                    SkillIdsJson TEXT NULL,
                    ObtainMethod TEXT NULL,
                    IsTradable INTEGER NOT NULL DEFAULT 1,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS PetInstances (
                    InstanceId TEXT PRIMARY KEY,
                    PlayerId TEXT NOT NULL,
                    TemplateId TEXT NOT NULL,
                    Name TEXT NOT NULL,
                    Level INTEGER NOT NULL DEFAULT 1,
                    Exp INTEGER NOT NULL DEFAULT 0,
                    XExp INTEGER NOT NULL DEFAULT 100,
                    Quality INTEGER NOT NULL DEFAULT 1,
                    GrowthRate REAL NOT NULL DEFAULT 1.0,
                    Type1 INTEGER NOT NULL DEFAULT 100,
                    Type2 INTEGER NOT NULL DEFAULT 50,
                    Type3 INTEGER NOT NULL DEFAULT 10,
                    Type4 INTEGER NOT NULL DEFAULT 5,
                    Type5 INTEGER NOT NULL DEFAULT 5,
                    Type6 INTEGER NOT NULL DEFAULT 3,
                    Type7 INTEGER NOT NULL DEFAULT 10,
                    Type8 REAL NOT NULL DEFAULT 0.9,
                    Type9 REAL NOT NULL DEFAULT 0,
                    Type10 REAL NOT NULL DEFAULT 0,
                    Type11 REAL NOT NULL DEFAULT 1.5,
                    Type12 REAL NOT NULL DEFAULT 0,
                    Type13 REAL NOT NULL DEFAULT 0,
                    Type14 REAL NOT NULL DEFAULT 0,
                    Type15 REAL NOT NULL DEFAULT 0,
                    Element INTEGER NOT NULL DEFAULT 0,
                    Loyalty INTEGER NOT NULL DEFAULT 100,
                    IsActive INTEGER NOT NULL DEFAULT 0,
                    IsBound INTEGER NOT NULL DEFAULT 1,
                    SkillIdsJson TEXT NULL,
                    AcquiredTime TEXT NOT NULL,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_pet_instances_player ON PetInstances(PlayerId);",
                """
                CREATE TABLE IF NOT EXISTS AlchemyRecipes (
                    RecipeId TEXT PRIMARY KEY,
                    PillTemplateId TEXT NOT NULL,
                    Name TEXT NOT NULL,
                    Description TEXT NULL,
                    RequiredFurnaceLevel INTEGER NOT NULL DEFAULT 1,
                    BaseSuccessRate INTEGER NOT NULL DEFAULT 50,
                    BaseCraftTime INTEGER NOT NULL DEFAULT 300,
                    MaterialsJson TEXT NULL,
                    UnlockCondition TEXT NULL,
                    IsDefaultLearned INTEGER NOT NULL DEFAULT 0,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_alchemy_recipe_furnace_level ON AlchemyRecipes(RequiredFurnaceLevel);",
                """
                CREATE TABLE IF NOT EXISTS AlchemySystems (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PlayerId TEXT NOT NULL,
                    FurnaceLevel INTEGER NOT NULL DEFAULT 1,
                    AlchemistLevel INTEGER NOT NULL DEFAULT 1,
                    AlchemistExp INTEGER NOT NULL DEFAULT 0,
                    Proficiency INTEGER NOT NULL DEFAULT 0,
                    LearnedRecipesJson TEXT NULL,
                    SuccessRateBonus INTEGER NOT NULL DEFAULT 0,
                    CraftTimeReduction INTEGER NOT NULL DEFAULT 0,
                    YieldBonus INTEGER NOT NULL DEFAULT 0,
                    TodayCraftCount INTEGER NOT NULL DEFAULT 0,
                    DailyResetTime TEXT NOT NULL,
                    TotalCraftCount INTEGER NOT NULL DEFAULT 0,
                    SuccessCraftCount INTEGER NOT NULL DEFAULT 0,
                    LastUpdateTime TEXT NOT NULL,
                    ActiveRecipeId TEXT NULL,
                    ActiveCraftStartedAt TEXT NULL,
                    ActiveCraftCompleteAt TEXT NULL,
                    PendingResultJson TEXT NULL
                );
                """,
                "CREATE UNIQUE INDEX IF NOT EXISTS idx_alchemy_system_player ON AlchemySystems(PlayerId);",
                """
                CREATE TABLE IF NOT EXISTS AlchemyProfessionLevelConfigs (
                    Level INTEGER PRIMARY KEY,
                    NextLevelExp INTEGER NOT NULL DEFAULT 80,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS AlchemyProfessionRuleConfigs (
                    ConfigId TEXT PRIMARY KEY,
                    SuccessBonusPerOverLevel INTEGER NOT NULL DEFAULT 1,
                    MaxSuccessBonus INTEGER NOT NULL DEFAULT 15,
                    SuccessExpBase INTEGER NOT NULL DEFAULT 12,
                    SuccessExpPerRequiredLevel INTEGER NOT NULL DEFAULT 4,
                    FailureExpBase INTEGER NOT NULL DEFAULT 6,
                    FailureExpPerRequiredLevel INTEGER NOT NULL DEFAULT 2,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS refresh_token (
                    GID TEXT PRIMARY KEY,
                    UserId TEXT NOT NULL,
                    Token TEXT NOT NULL,
                    ExpiresAt TEXT NOT NULL,
                    CreateTime TEXT NOT NULL,
                    IsRevoked INTEGER NOT NULL DEFAULT 0,
                    RevokedAt TEXT NULL,
                    DeviceInfo TEXT NULL,
                    IpAddress TEXT NULL
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_refresh_token_user ON refresh_token(UserId);",
                """
                CREATE TABLE IF NOT EXISTS token_blacklist (
                    GID TEXT PRIMARY KEY,
                    AccessToken TEXT NOT NULL,
                    UserId TEXT NOT NULL,
                    ExpiresAt TEXT NOT NULL,
                    CreateTime TEXT NOT NULL,
                    Reason TEXT NULL
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS dungeon_daily_record (
                    GID TEXT PRIMARY KEY,
                    PlayerId TEXT NOT NULL,
                    DungeonId TEXT NOT NULL,
                    ChallengeCount INTEGER NOT NULL DEFAULT 0,
                    RecordDate TEXT NOT NULL,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                "CREATE UNIQUE INDEX IF NOT EXISTS idx_dungeon_daily_unique ON dungeon_daily_record(PlayerId, DungeonId, RecordDate);",
                """
                CREATE TABLE IF NOT EXISTS shop_daily_record (
                    GID TEXT PRIMARY KEY,
                    PlayerId TEXT NOT NULL,
                    ShopId TEXT NOT NULL,
                    ItemId TEXT NOT NULL,
                    PurchasedCount INTEGER NOT NULL DEFAULT 0,
                    RecordDate TEXT NOT NULL,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                "CREATE UNIQUE INDEX IF NOT EXISTS idx_shop_daily_unique ON shop_daily_record(PlayerId, ItemId, RecordDate);",
                """
                CREATE TABLE IF NOT EXISTS shop_config (
                    ShopId TEXT PRIMARY KEY,
                    ShopName TEXT NOT NULL,
                    ShopType INTEGER NOT NULL DEFAULT 0,
                    Description TEXT NULL,
                    RequiredLevel INTEGER NOT NULL DEFAULT 1,
                    RequiredVipLevel INTEGER NOT NULL DEFAULT 0,
                    Discount REAL NOT NULL DEFAULT 1.0,
                    IsOpen INTEGER NOT NULL DEFAULT 1,
                    AutoRefresh INTEGER NOT NULL DEFAULT 0,
                    RefreshIntervalHours INTEGER NOT NULL DEFAULT 24,
                    Icon TEXT NULL,
                    SortOrder INTEGER NOT NULL DEFAULT 0,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS shop_item (
                    GID TEXT PRIMARY KEY,
                    ShopId TEXT NOT NULL,
                    ItemId TEXT NOT NULL,
                    ItemType INTEGER NOT NULL DEFAULT 0,
                    BasePrice INTEGER NOT NULL DEFAULT 0,
                    CurrentPrice INTEGER NOT NULL DEFAULT 0,
                    Stock INTEGER NOT NULL DEFAULT -1,
                    InitialStock INTEGER NOT NULL DEFAULT -1,
                    DailyLimit INTEGER NOT NULL DEFAULT -1,
                    SortOrder INTEGER NOT NULL DEFAULT 0,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_shop_item_shop ON shop_item(ShopId);",
                """
                CREATE TABLE IF NOT EXISTS ranking_config (
                    RankingId TEXT PRIMARY KEY,
                    RankingName TEXT NOT NULL,
                    RankingType INTEGER NOT NULL DEFAULT 0,
                    Description TEXT NULL,
                    MaxSize INTEGER NOT NULL DEFAULT 100,
                    UpdateInterval INTEGER NOT NULL DEFAULT 60,
                    SeasonEnabled INTEGER NOT NULL DEFAULT 0,
                    SeasonDuration INTEGER NOT NULL DEFAULT 30,
                    CurrentSeason INTEGER NOT NULL DEFAULT 1,
                    SeasonStartTime TEXT NULL,
                    SortOrder INTEGER NOT NULL DEFAULT 0,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS ranking_entry (
                    GID TEXT PRIMARY KEY,
                    RankingId TEXT NOT NULL,
                    PlayerId TEXT NOT NULL,
                    PlayerName TEXT NOT NULL,
                    PlayerLevel INTEGER NOT NULL DEFAULT 1,
                    Score INTEGER NOT NULL DEFAULT 0,
                    Rank INTEGER NOT NULL DEFAULT 0,
                    LastRank INTEGER NOT NULL DEFAULT 0,
                    Season INTEGER NOT NULL DEFAULT 1,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                "CREATE UNIQUE INDEX IF NOT EXISTS idx_ranking_entry_unique ON ranking_entry(RankingId, PlayerId, Season);",
                """
                CREATE TABLE IF NOT EXISTS ranking_reward (
                    GID TEXT PRIMARY KEY,
                    RankingId TEXT NOT NULL,
                    MinRank INTEGER NOT NULL DEFAULT 1,
                    MaxRank INTEGER NOT NULL DEFAULT 1,
                    RewardTitle TEXT NOT NULL,
                    Gold INTEGER NOT NULL DEFAULT 0,
                    SpiritStone INTEGER NOT NULL DEFAULT 0,
                    Title TEXT NULL,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS ranking_history (
                    GID TEXT PRIMARY KEY,
                    RankingId TEXT NOT NULL,
                    SnapshotType INTEGER NOT NULL DEFAULT 0,
                    Season INTEGER NOT NULL DEFAULT 1,
                    Description TEXT NULL,
                    SnapshotData TEXT NULL,
                    CreateTime TEXT NOT NULL
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS ranking_outbox (
                    EventId TEXT PRIMARY KEY,
                    PlayerId TEXT NOT NULL,
                    RankingIdsJson TEXT NOT NULL DEFAULT '[]',
                    Reason TEXT NOT NULL DEFAULT '',
                    OccurredAtUtc TEXT NOT NULL,
                    Status TEXT NOT NULL DEFAULT 'Pending',
                    RetryCount INTEGER NOT NULL DEFAULT 0,
                    NextAttemptAtUtc TEXT NOT NULL,
                    LastError TEXT NULL,
                    CreatedAtUtc TEXT NOT NULL,
                    ProcessedAtUtc TEXT NULL,
                    ProcessingStartedAtUtc TEXT NULL
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_ranking_outbox_status_attempt ON ranking_outbox(Status, NextAttemptAtUtc);",
                "CREATE INDEX IF NOT EXISTS idx_ranking_outbox_player_status_attempt ON ranking_outbox(PlayerId, Status, NextAttemptAtUtc);",
                """
                CREATE TABLE IF NOT EXISTS PlayerCheckInStates (
                    PlayerId TEXT PRIMARY KEY,
                    ContinuousDays INTEGER NOT NULL DEFAULT 0,
                    TotalDays INTEGER NOT NULL DEFAULT 0,
                    LastCheckInDate TEXT NULL,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS PlayerCheckInRecords (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PlayerId TEXT NOT NULL,
                    CheckInDate TEXT NOT NULL,
                    RewardSnapshotJson TEXT NULL,
                    CreateTime TEXT NOT NULL
                );
                """,
                "CREATE UNIQUE INDEX IF NOT EXISTS idx_checkin_player_date ON PlayerCheckInRecords(PlayerId, CheckInDate);",
                "CREATE INDEX IF NOT EXISTS idx_checkin_player_month ON PlayerCheckInRecords(PlayerId, CheckInDate);",
                """
                CREATE TABLE IF NOT EXISTS CheckInRewardConfigs (
                    ContinuousDay INTEGER PRIMARY KEY,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    ConfigVersion TEXT NOT NULL,
                    IsMilestone INTEGER NOT NULL DEFAULT 0,
                    RewardJson TEXT NOT NULL,
                    Description TEXT NULL,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS RedeemCodeConfigs (
                    Code TEXT PRIMARY KEY,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    ConfigVersion TEXT NOT NULL,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    Description TEXT NOT NULL,
                    RewardJson TEXT NOT NULL,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS RedeemCodeUsages (
                    UsageId TEXT PRIMARY KEY,
                    PlayerId TEXT NOT NULL,
                    Code TEXT NOT NULL,
                    RewardSnapshotJson TEXT NULL,
                    RedeemedAt TEXT NOT NULL
                );
                """,
                "CREATE UNIQUE INDEX IF NOT EXISTS idx_redeem_player_code ON RedeemCodeUsages(PlayerId, Code);",
                """
                CREATE TABLE IF NOT EXISTS BattleElementRelationConfigs (
                    GID TEXT PRIMARY KEY,
                    AttackerElement INTEGER NOT NULL,
                    DefenderElement INTEGER NOT NULL,
                    Modifier REAL NOT NULL DEFAULT 0,
                    SortOrder INTEGER NOT NULL DEFAULT 0,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                "CREATE UNIQUE INDEX IF NOT EXISTS idx_element_relation_attacker_defender ON BattleElementRelationConfigs(AttackerElement, DefenderElement);",
                """
                CREATE TABLE IF NOT EXISTS StarterPackageConfigs (
                    PackageId TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Description TEXT NULL,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    AutoGrantOnRegister INTEGER NOT NULL DEFAULT 0,
                    SortOrder INTEGER NOT NULL DEFAULT 0,
                    ConfigVersion TEXT NULL,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    Remark TEXT NULL,
                    CreatedBy TEXT NULL,
                    UpdatedBy TEXT NULL,
                    CreateTime TEXT NOT NULL,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_starter_package_sort ON StarterPackageConfigs(SortOrder);",
                """
                CREATE TABLE IF NOT EXISTS StarterPackageGrantItems (
                    GID TEXT PRIMARY KEY,
                    PackageId TEXT NOT NULL,
                    ItemId TEXT NOT NULL,
                    Quantity INTEGER NOT NULL DEFAULT 1,
                    IsBound INTEGER NOT NULL DEFAULT 1,
                    SortOrder INTEGER NOT NULL DEFAULT 0
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_starter_package_item_package ON StarterPackageGrantItems(PackageId);",
                """
                CREATE TABLE IF NOT EXISTS StarterPackageGrantSkills (
                    GID TEXT PRIMARY KEY,
                    PackageId TEXT NOT NULL,
                    SkillId INTEGER NOT NULL,
                    SortOrder INTEGER NOT NULL DEFAULT 0
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_starter_package_skill_package ON StarterPackageGrantSkills(PackageId);",
                """
                CREATE TABLE IF NOT EXISTS ItemTemplates (
                    ItemId TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    UseLevel INTEGER NOT NULL DEFAULT 1,
                    Type INTEGER NOT NULL DEFAULT 0,
                    Description TEXT NOT NULL,
                    MaxStack INTEGER NOT NULL DEFAULT 1,
                    Quality INTEGER NOT NULL DEFAULT 1,
                    IconPath TEXT NULL,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00',
                    ChestConfigJson TEXT NULL,
                    SkillBookConfigJson TEXT NULL,
                    PetEggConfigJson TEXT NULL,
                    PillConfigJson TEXT NULL
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_item_template_type ON ItemTemplates(Type);",
                """
                CREATE TABLE IF NOT EXISTS ItemChestConfigs (
                    ItemId TEXT PRIMARY KEY,
                    OpenMode INTEGER NOT NULL DEFAULT 1,
                    RollCount INTEGER NOT NULL DEFAULT 1,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS ItemChestRewardEntries (
                    GID TEXT PRIMARY KEY,
                    ItemId TEXT NOT NULL,
                    RewardType INTEGER NOT NULL DEFAULT 3,
                    TargetId TEXT NULL,
                    MinCount INTEGER NOT NULL DEFAULT 1,
                    MaxCount INTEGER NOT NULL DEFAULT 1,
                    Weight INTEGER NOT NULL DEFAULT 100,
                    IsBound INTEGER NOT NULL DEFAULT 0,
                    Description TEXT NULL,
                    SortOrder INTEGER NOT NULL DEFAULT 0
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_item_chest_rewards_item ON ItemChestRewardEntries(ItemId, SortOrder);",
                """
                CREATE TABLE IF NOT EXISTS ItemSkillBookConfigs (
                    ItemId TEXT PRIMARY KEY,
                    SkillId INTEGER NOT NULL DEFAULT 0,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS ItemRecipeUnlockConfigs (
                    ItemId TEXT PRIMARY KEY,
                    RecipeType TEXT NOT NULL,
                    RecipeId TEXT NOT NULL,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_item_recipe_unlock_recipe ON ItemRecipeUnlockConfigs(RecipeType, RecipeId);",
                """
                CREATE TABLE IF NOT EXISTS ItemPetEggConfigs (
                    ItemId TEXT PRIMARY KEY,
                    PetTemplateId TEXT NOT NULL,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS ItemPillConfigs (
                    ItemId TEXT PRIMARY KEY,
                    EffectType INTEGER NOT NULL DEFAULT 2,
                    BreakthroughBonusPercent INTEGER NOT NULL DEFAULT 0,
                    ExpGain INTEGER NOT NULL DEFAULT 0,
                    AttributeType TEXT NULL,
                    AttributeValue REAL NOT NULL DEFAULT 0,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
                """,
                // PlayerPillEffects
                @"CREATE TABLE IF NOT EXISTS PlayerPillEffects (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PlayerId TEXT NOT NULL,
                    ItemId TEXT NOT NULL,
                    EffectType INTEGER NOT NULL DEFAULT 0,
                    BonusType TEXT NULL,
                    BonusValue REAL NOT NULL DEFAULT 0,
                    UsageCount INTEGER NOT NULL DEFAULT 0,
                    IsTemporary INTEGER NOT NULL DEFAULT 0,
                    ExpiresAt TEXT NULL,
                    CreatedAt TEXT NOT NULL,
                    LastUpdateTime TEXT NOT NULL
                )",
                @"CREATE INDEX IF NOT EXISTS idx_pill_effects_player_item ON PlayerPillEffects (PlayerId, ItemId)",
                @"CREATE INDEX IF NOT EXISTS idx_pill_effects_expires ON PlayerPillEffects (ExpiresAt)",
                """
                CREATE TABLE IF NOT EXISTS EquipmentTemplates (
                    EquipmentId INTEGER PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Level INTEGER NOT NULL DEFAULT 1,
                    Quality INTEGER NOT NULL DEFAULT 1,
                    Slot INTEGER NOT NULL DEFAULT 0,
                    CombatStyle INTEGER NOT NULL DEFAULT 0,
                    WeaponCategory INTEGER NOT NULL DEFAULT 0,
                    Description TEXT NOT NULL,
                    IconPath TEXT NULL,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00',
                    MinType1 INTEGER NULL,
                    MaxType1 INTEGER NULL,
                    MinType2 INTEGER NULL,
                    MaxType2 INTEGER NULL,
                    MinType3 INTEGER NULL,
                    MaxType3 INTEGER NULL,
                    MinType4 INTEGER NULL,
                    MaxType4 INTEGER NULL,
                    MinType5 INTEGER NULL,
                    MaxType5 INTEGER NULL,
                    MinType6 INTEGER NULL,
                    MaxType6 INTEGER NULL,
                    MinType7 INTEGER NULL,
                    MaxType7 INTEGER NULL,
                    MinType8 INTEGER NULL,
                    MaxType8 INTEGER NULL,
                    MinType9 INTEGER NULL,
                    MaxType9 INTEGER NULL,
                    MinType10 INTEGER NULL,
                    MaxType10 INTEGER NULL,
                    MinType11 INTEGER NULL,
                    MaxType11 INTEGER NULL,
                    MinType12 INTEGER NULL,
                    MaxType12 INTEGER NULL,
                    MinType13 INTEGER NULL,
                    MaxType13 INTEGER NULL,
                    MinType14 INTEGER NULL,
                    MaxType14 INTEGER NULL,
                    MinType15 INTEGER NULL,
                    MaxType15 INTEGER NULL,
                    Element INTEGER NULL,
                    ElementPoolJson TEXT NULL
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_equipment_template_level ON EquipmentTemplates(Level);",
                """
                CREATE TABLE IF NOT EXISTS MonsterTemplates (
                    MonsterId TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Level INTEGER NOT NULL DEFAULT 1,
                    ExpRewardMin INTEGER NOT NULL DEFAULT 0,
                    ExpRewardMax INTEGER NOT NULL DEFAULT 0,
                    GoldRewardMin INTEGER NOT NULL DEFAULT 0,
                    GoldRewardMax INTEGER NOT NULL DEFAULT 0,
                    SkillIdsJson TEXT NULL,
                    PassiveIdsJson TEXT NULL,
                    ItemDropsJson TEXT NULL,
                    EquipmentDropsJson TEXT NULL,
                    MinType1 INTEGER NULL,
                    MaxType1 INTEGER NULL,
                    MinType2 INTEGER NULL,
                    MaxType2 INTEGER NULL,
                    MinType3 INTEGER NULL,
                    MaxType3 INTEGER NULL,
                    MinType4 INTEGER NULL,
                    MaxType4 INTEGER NULL,
                    MinType5 INTEGER NULL,
                    MaxType5 INTEGER NULL,
                    MinType6 INTEGER NULL,
                    MaxType6 INTEGER NULL,
                    MinType7 INTEGER NULL,
                    MaxType7 INTEGER NULL,
                    MinType8 INTEGER NULL,
                    MaxType8 INTEGER NULL,
                    MinType9 INTEGER NULL,
                    MaxType9 INTEGER NULL,
                    MinType10 INTEGER NULL,
                    MaxType10 INTEGER NULL,
                    MinType11 INTEGER NULL,
                    MaxType11 INTEGER NULL,
                    MinType12 INTEGER NULL,
                    MaxType12 INTEGER NULL,
                    MinType13 INTEGER NULL,
                    MaxType13 INTEGER NULL,
                    MinType14 INTEGER NULL,
                    MaxType14 INTEGER NULL,
                    MinType15 INTEGER NULL,
                    MaxType15 INTEGER NULL,
                    Element INTEGER NULL,
                    ElementPoolJson TEXT NULL,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_monster_template_level ON MonsterTemplates(Level);",
                """
                CREATE TABLE IF NOT EXISTS MapTemplates (
                    MapId TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Level INTEGER NOT NULL DEFAULT 1,
                    Description TEXT NOT NULL,
                    NextMapId TEXT NULL,
                    Carrying INTEGER NOT NULL DEFAULT 0,
                    MonsterCountMin INTEGER NOT NULL DEFAULT 1,
                    MonsterCountMax INTEGER NOT NULL DEFAULT 1,
                    SpawnRulesJson TEXT NULL,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_map_template_level ON MapTemplates(Level);",
                """
                CREATE TABLE IF NOT EXISTS SkillTemplates (
                    SkillId INTEGER PRIMARY KEY,
                    SkillCatalog TEXT NOT NULL DEFAULT 'legacy',
                    SkillLevel INTEGER NOT NULL DEFAULT 1,
                    PreviousSkillId INTEGER NULL,
                    NextSkillId INTEGER NULL,
                    UpgradeConditionsJson TEXT NULL,
                    Name TEXT NOT NULL,
                    Description TEXT NOT NULL,
                    TargetType INTEGER NOT NULL DEFAULT 1,
                    ManaCost INTEGER NOT NULL DEFAULT 0,
                    Cooldown INTEGER NOT NULL DEFAULT 0,
                    DamageType INTEGER NOT NULL DEFAULT 0,
                    HitCount INTEGER NOT NULL DEFAULT 1,
                    RangeType INTEGER NOT NULL DEFAULT 1,
                    DamageMultiplier REAL NOT NULL DEFAULT 0,
                    TriggerChance REAL NOT NULL DEFAULT 1,
                    HitsJson TEXT NULL,
                    BuffIdsJson TEXT NULL,
                    AllowedProfessionsJson TEXT NULL,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS BuffTemplates (
                    BuffId TEXT PRIMARY KEY,
                    BuffCatalog TEXT NOT NULL DEFAULT 'legacy',
                    Name TEXT NOT NULL,
                    Description TEXT NOT NULL,
                    Duration INTEGER NOT NULL DEFAULT 0,
                    MaxStack INTEGER NOT NULL DEFAULT 1,
                    StackRule INTEGER NOT NULL DEFAULT 0,
                    EffectsJson TEXT NULL,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS ForgeRecipes (
                    RecipeId TEXT PRIMARY KEY,
                    TemplateId TEXT NOT NULL,
                    Name TEXT NOT NULL,
                    Description TEXT NOT NULL,
                    SlotName TEXT NOT NULL,
                    Quality INTEGER NOT NULL DEFAULT 1,
                    Level INTEGER NOT NULL DEFAULT 1,
                    Icon TEXT NOT NULL,
                    CostGold INTEGER NOT NULL DEFAULT 0,
                    SuccessRate INTEGER NOT NULL DEFAULT 100,
                    MaterialsJson TEXT NOT NULL,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_forge_recipe_level ON ForgeRecipes(Level);",
                """
                CREATE TABLE IF NOT EXISTS ForgeSystems (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PlayerId TEXT NOT NULL,
                    BlacksmithLevel INTEGER NOT NULL DEFAULT 1,
                    BlacksmithExp INTEGER NOT NULL DEFAULT 0,
                    TodayForgeCount INTEGER NOT NULL DEFAULT 0,
                    DailyResetTime TEXT NOT NULL,
                    TotalForgeCount INTEGER NOT NULL DEFAULT 0,
                    SuccessForgeCount INTEGER NOT NULL DEFAULT 0,
                    LastUpdateTime TEXT NOT NULL,
                    ActiveRecipeId TEXT NULL,
                    ActiveForgeStartedAt TEXT NULL,
                    ActiveForgeCompleteAt TEXT NULL,
                    LearnedRecipesJson TEXT NULL,
                    PendingResultJson TEXT NULL
                );
                """,
                "CREATE UNIQUE INDEX IF NOT EXISTS idx_forge_system_player ON ForgeSystems(PlayerId);",
                """
                CREATE TABLE IF NOT EXISTS ForgeProfessionLevelConfigs (
                    Level INTEGER PRIMARY KEY,
                    NextLevelExp INTEGER NOT NULL DEFAULT 80,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS ForgeProfessionRuleConfigs (
                    ConfigId TEXT PRIMARY KEY,
                    SuccessBonusPerOverLevel INTEGER NOT NULL DEFAULT 1,
                    MaxSuccessBonus INTEGER NOT NULL DEFAULT 15,
                    SuccessExpBase INTEGER NOT NULL DEFAULT 12,
                    SuccessExpPerRequiredLevel INTEGER NOT NULL DEFAULT 4,
                    FailureExpBase INTEGER NOT NULL DEFAULT 6,
                    FailureExpPerRequiredLevel INTEGER NOT NULL DEFAULT 2,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS DungeonTemplates (
                    DungeonId TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Description TEXT NOT NULL,
                    RecommendedLevel INTEGER NOT NULL DEFAULT 1,
                    DailyLimit INTEGER NOT NULL DEFAULT 1,
                    NormalMapId TEXT NOT NULL,
                    FubenMapId TEXT NOT NULL,
                    RequiredTeamSize INTEGER NOT NULL DEFAULT 1,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS Parties (
                    PartyId TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    LeaderPlayerId TEXT NOT NULL,
                    TargetDungeonId TEXT NOT NULL,
                    TargetDungeonName TEXT NOT NULL,
                    MinLevel INTEGER NOT NULL DEFAULT 1,
                    MaxMembers INTEGER NOT NULL DEFAULT 1,
                    IsRecruiting INTEGER NOT NULL DEFAULT 1,
                    CreateTime TEXT NOT NULL,
                    LastUpdateTime TEXT NOT NULL,
                    ExpiresAt TEXT NOT NULL
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_party_target_dungeon ON Parties(TargetDungeonId);",
                """
                CREATE TABLE IF NOT EXISTS PartyMembers (
                    MembershipId TEXT PRIMARY KEY,
                    PartyId TEXT NOT NULL,
                    PlayerId TEXT NOT NULL,
                    IsLeader INTEGER NOT NULL DEFAULT 0,
                    JoinTime TEXT NOT NULL
                );
                """,
                "CREATE UNIQUE INDEX IF NOT EXISTS idx_party_member_player ON PartyMembers(PlayerId);",
                "CREATE INDEX IF NOT EXISTS idx_party_member_party ON PartyMembers(PartyId);",
                """
                CREATE TABLE IF NOT EXISTS PartyBattleRecords (
                    BattleId TEXT PRIMARY KEY,
                    PartyId TEXT NOT NULL,
                    DungeonId TEXT NOT NULL,
                    InitiatorPlayerId TEXT NOT NULL,
                    MemberIdsJson TEXT NOT NULL DEFAULT '[]',
                    RequestId TEXT NOT NULL,
                    StartedAtUtc TEXT NOT NULL,
                    CompletedAtUtc TEXT NULL,
                    IsVictory INTEGER NOT NULL DEFAULT 0,
                    StageCount INTEGER NOT NULL DEFAULT 0,
                    TotalRounds INTEGER NOT NULL DEFAULT 0,
                    RewardSummaryJson TEXT NOT NULL DEFAULT '{}',
                    SettlementStatus TEXT NOT NULL DEFAULT 'Started',
                    FullReplayJson TEXT NULL
                );
                """,
                "CREATE UNIQUE INDEX IF NOT EXISTS idx_party_battle_request ON PartyBattleRecords(PartyId, RequestId);",
                "CREATE INDEX IF NOT EXISTS idx_party_battle_party ON PartyBattleRecords(PartyId, StartedAtUtc);",
                """
                CREATE TABLE IF NOT EXISTS WorldBossTemplates (
                    BossId TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    MonsterTemplateId TEXT NOT NULL,
                    PortraitPath TEXT NULL,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    Weight INTEGER NOT NULL DEFAULT 1,
                    DurationMinutes INTEGER NOT NULL DEFAULT 60,
                    NoticeText TEXT NULL,
                    ParticipationMinDamage INTEGER NOT NULL DEFAULT 1,
                    ParticipationRewardExp INTEGER NOT NULL DEFAULT 0,
                    ParticipationRewardGold INTEGER NOT NULL DEFAULT 0,
                    ParticipationRewardSpiritStone INTEGER NOT NULL DEFAULT 0,
                    Rank1RewardExp INTEGER NOT NULL DEFAULT 0,
                    Rank1RewardGold INTEGER NOT NULL DEFAULT 0,
                    Rank1RewardSpiritStone INTEGER NOT NULL DEFAULT 0,
                    Rank2RewardExp INTEGER NOT NULL DEFAULT 0,
                    Rank2RewardGold INTEGER NOT NULL DEFAULT 0,
                    Rank2RewardSpiritStone INTEGER NOT NULL DEFAULT 0,
                    Rank3RewardExp INTEGER NOT NULL DEFAULT 0,
                    Rank3RewardGold INTEGER NOT NULL DEFAULT 0,
                    Rank3RewardSpiritStone INTEGER NOT NULL DEFAULT 0,
                    SortOrder INTEGER NOT NULL DEFAULT 0,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS WorldBossSchedules (
                    ScheduleId TEXT PRIMARY KEY,
                    SpawnTimeText TEXT NOT NULL,
                    TimeZoneId TEXT NOT NULL,
                    SelectionMode INTEGER NOT NULL DEFAULT 0,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS WorldBossInstances (
                    InstanceId TEXT PRIMARY KEY,
                    BossId TEXT NOT NULL,
                    BossName TEXT NOT NULL,
                    MonsterTemplateId TEXT NOT NULL,
                    PortraitPath TEXT NULL,
                    NoticeText TEXT NULL,
                    State INTEGER NOT NULL DEFAULT 1,
                    SpawnedAtUtc TEXT NOT NULL,
                    EndAtUtc TEXT NOT NULL,
                    SettledAtUtc TEXT NULL,
                    CurrentHp INTEGER NOT NULL DEFAULT 0,
                    MaxHp INTEGER NOT NULL DEFAULT 0,
                    CurrentMp INTEGER NOT NULL DEFAULT 0,
                    MaxMp INTEGER NOT NULL DEFAULT 0,
                    CombatStateJson TEXT NOT NULL,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_world_boss_instances_state ON WorldBossInstances(State);",
                """
                CREATE TABLE IF NOT EXISTS WorldBossParticipants (
                    ParticipantId TEXT PRIMARY KEY,
                    InstanceId TEXT NOT NULL,
                    PlayerId TEXT NOT NULL,
                    PlayerName TEXT NOT NULL,
                    IsAuto INTEGER NOT NULL DEFAULT 0,
                    TotalDamage INTEGER NOT NULL DEFAULT 0,
                    TotalHeal INTEGER NOT NULL DEFAULT 0,
                    DeathCount INTEGER NOT NULL DEFAULT 0,
                    JoinAtUtc TEXT NOT NULL,
                    LastActionAtUtc TEXT NULL,
                    ReadyAtUtc TEXT NOT NULL,
                    DeadlineAtUtc TEXT NOT NULL,
                    ReviveAtUtc TEXT NULL,
                    RewardClaimed INTEGER NOT NULL DEFAULT 0,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                "CREATE UNIQUE INDEX IF NOT EXISTS idx_world_boss_participant_instance_player ON WorldBossParticipants(InstanceId, PlayerId);",
                "CREATE INDEX IF NOT EXISTS idx_world_boss_participant_instance ON WorldBossParticipants(InstanceId);",
                """
                CREATE TABLE IF NOT EXISTS WorldBossRewardRecords (
                    RecordId TEXT PRIMARY KEY,
                    InstanceId TEXT NOT NULL,
                    PlayerId TEXT NOT NULL,
                    PlayerName TEXT NOT NULL,
                    Rank INTEGER NOT NULL DEFAULT 0,
                    Damage INTEGER NOT NULL DEFAULT 0,
                    RewardExp INTEGER NOT NULL DEFAULT 0,
                    RewardGold INTEGER NOT NULL DEFAULT 0,
                    RewardSpiritStone INTEGER NOT NULL DEFAULT 0,
                    IsClaimed INTEGER NOT NULL DEFAULT 0,
                    ClaimedAtUtc TEXT NULL,
                    CreateTime TEXT NOT NULL
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_world_boss_reward_player ON WorldBossRewardRecords(PlayerId, IsClaimed);",
                """
                CREATE TABLE IF NOT EXISTS WorldBossLogs (
                    LogId TEXT PRIMARY KEY,
                    InstanceId TEXT NOT NULL,
                    Seq INTEGER NOT NULL DEFAULT 0,
                    TimestampUtc TEXT NOT NULL,
                    ActorId TEXT NULL,
                    ActorName TEXT NULL,
                    TargetId TEXT NULL,
                    TargetName TEXT NULL,
                    ActionType TEXT NOT NULL,
                    Content TEXT NOT NULL
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_world_boss_logs_instance_seq ON WorldBossLogs(InstanceId, Seq);",
                """
                CREATE TABLE IF NOT EXISTS achievement_progress (
                    GID TEXT PRIMARY KEY,
                    PlayerId TEXT NOT NULL,
                    AchievementId TEXT NOT NULL,
                    CurrentProgress INTEGER NOT NULL DEFAULT 0,
                    TargetProgress INTEGER NOT NULL DEFAULT 0,
                    RequirementProgressJson TEXT NULL,
                    Status INTEGER NOT NULL DEFAULT 0,
                    AcceptTime TEXT NULL,
                    CompleteTime TEXT NULL,
                    ClaimTime TEXT NULL,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                "CREATE UNIQUE INDEX IF NOT EXISTS idx_achievement_progress_player_achievement ON achievement_progress(PlayerId, AchievementId);",
                """
                CREATE TABLE IF NOT EXISTS achievement_config (
                    AchievementId TEXT PRIMARY KEY,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    AchievementName TEXT NOT NULL,
                    AchievementType INTEGER NOT NULL DEFAULT 0,
                    Difficulty INTEGER NOT NULL DEFAULT 0,
                    Description TEXT NOT NULL,
                    Category TEXT NOT NULL,
                    Points INTEGER NOT NULL DEFAULT 0,
                    IsHidden INTEGER NOT NULL DEFAULT 0,
                    PreAchievementIds TEXT NULL,
                    RewardGold INTEGER NOT NULL DEFAULT 0,
                    RewardSpiritStone INTEGER NOT NULL DEFAULT 0,
                    RewardExp INTEGER NOT NULL DEFAULT 0,
                    RewardTitle TEXT NULL,
                    RewardItemsJson TEXT NULL,
                    RewardEquipmentIds TEXT NULL,
                    RequirementType INTEGER NOT NULL DEFAULT 0,
                    RequirementTargetValue INTEGER NOT NULL DEFAULT 0,
                    RequirementDescription TEXT NULL,
                    RequirementsJson TEXT NULL,
                    SortOrder INTEGER NOT NULL DEFAULT 0,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS achievement_metric_counter (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PlayerId TEXT NOT NULL,
                    RequirementType INTEGER NOT NULL DEFAULT 0,
                    TargetId TEXT NULL,
                    CurrentValue INTEGER NOT NULL DEFAULT 0,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                "CREATE UNIQUE INDEX IF NOT EXISTS idx_achievement_metric_counter_player_requirement_target ON achievement_metric_counter(PlayerId, RequirementType, TargetId);",
                """
                CREATE TABLE IF NOT EXISTS quest_progress (
                    GID TEXT PRIMARY KEY,
                    PlayerId TEXT NOT NULL,
                    QuestId TEXT NOT NULL,
                    CurrentStage INTEGER NOT NULL DEFAULT 0,
                    ObjectiveProgressJson TEXT NULL,
                    Status INTEGER NOT NULL DEFAULT 0,
                    AcceptTime TEXT NULL,
                    CompleteTime TEXT NULL,
                    SubmitTime TEXT NULL,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                "CREATE UNIQUE INDEX IF NOT EXISTS idx_quest_progress_player_quest ON quest_progress(PlayerId, QuestId);",
                """
                CREATE TABLE IF NOT EXISTS quest_config (
                    QuestId TEXT PRIMARY KEY,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    QuestName TEXT NOT NULL,
                    QuestType INTEGER NOT NULL DEFAULT 0,
                    ResetCycle INTEGER NOT NULL DEFAULT 0,
                    Description TEXT NOT NULL,
                    RequiredLevel INTEGER NOT NULL DEFAULT 1,
                    PreQuestIds TEXT NULL,
                    AutoAccept INTEGER NOT NULL DEFAULT 0,
                    AutoSubmit INTEGER NOT NULL DEFAULT 0,
                    TimeLimit INTEGER NOT NULL DEFAULT 0,
                    RewardExp INTEGER NOT NULL DEFAULT 0,
                    RewardGold INTEGER NOT NULL DEFAULT 0,
                    RewardSpiritStone INTEGER NOT NULL DEFAULT 0,
                    RewardItemsJson TEXT NULL,
                    RewardEquipmentIds TEXT NULL,
                    ObjectivesJson TEXT NULL,
                    SortOrder INTEGER NOT NULL DEFAULT 0,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS quest_completed_record (
                    GID TEXT PRIMARY KEY,
                    PlayerId TEXT NOT NULL,
                    QuestId TEXT NOT NULL,
                    CompleteTime TEXT NOT NULL,
                    SubmitTime TEXT NULL
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_quest_completed_player ON quest_completed_record(PlayerId);",
                """
                CREATE TABLE IF NOT EXISTS guild (
                    GID TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Announcement TEXT NULL,
                    LeaderId TEXT NOT NULL,
                    LeaderName TEXT NOT NULL,
                    Level INTEGER NOT NULL DEFAULT 1,
                    Exp INTEGER NOT NULL DEFAULT 0,
                    MemberCount INTEGER NOT NULL DEFAULT 1,
                    MaxMembers INTEGER NOT NULL DEFAULT 20,
                    Funds INTEGER NOT NULL DEFAULT 0,
                    RequiredLevel INTEGER NOT NULL DEFAULT 1,
                    AutoJoin INTEGER NOT NULL DEFAULT 0,
                    Icon TEXT NULL,
                    CreateTime TEXT NOT NULL,
                    LastUpdateTime TEXT NOT NULL,
                    IsDeleted INTEGER NOT NULL DEFAULT 0
                );
                """,
                "CREATE UNIQUE INDEX IF NOT EXISTS idx_guild_name ON guild(Name);",
                """
                CREATE TABLE IF NOT EXISTS guild_member (
                    GID TEXT PRIMARY KEY,
                    GuildId TEXT NOT NULL,
                    PlayerId TEXT NOT NULL,
                    PlayerName TEXT NOT NULL,
                    PlayerLevel INTEGER NOT NULL DEFAULT 1,
                    Position INTEGER NOT NULL DEFAULT 0,
                    Contribution INTEGER NOT NULL DEFAULT 0,
                    TotalContribution INTEGER NOT NULL DEFAULT 0,
                    JoinTime TEXT NOT NULL,
                    LastActiveTime TEXT NOT NULL
                );
                """,
                "CREATE UNIQUE INDEX IF NOT EXISTS idx_guild_member_player ON guild_member(PlayerId);",
                "CREATE INDEX IF NOT EXISTS idx_guild_member_guild ON guild_member(GuildId);",
                """
                CREATE TABLE IF NOT EXISTS CropTemplates (
                    TemplateId TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Description TEXT NULL,
                    Type INTEGER NOT NULL DEFAULT 1,
                    GrowthCycle INTEGER NOT NULL DEFAULT 3600,
                    Yield INTEGER NOT NULL DEFAULT 1,
                    SeedId TEXT NOT NULL,
                    SeedAmount INTEGER NOT NULL DEFAULT 1,
                    OutputItemId TEXT NOT NULL,
                    OutputAmount INTEGER NOT NULL DEFAULT 1,
                    MinQuality INTEGER NOT NULL DEFAULT 1,
                    MaxQuality INTEGER NOT NULL DEFAULT 1,
                    UnlockLevel INTEGER NOT NULL DEFAULT 1,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_crop_templates_unlock_level ON CropTemplates(UnlockLevel);",
                """
                CREATE TABLE IF NOT EXISTS SpiritFieldPlots (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PlayerId TEXT NOT NULL,
                    PlotNumber INTEGER NOT NULL DEFAULT 1,
                    Level INTEGER NOT NULL DEFAULT 1,
                    Status INTEGER NOT NULL DEFAULT 0,
                    CropTemplateId TEXT NULL,
                    PlantTime TEXT NULL,
                    ExpectedHarvestTime TEXT NULL,
                    ActualHarvestTime TEXT NULL,
                    SpeedUpCount INTEGER NOT NULL DEFAULT 0,
                    SpeedUpDuration INTEGER NOT NULL DEFAULT 0,
                    YieldBonusPercent INTEGER NOT NULL DEFAULT 0,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                "CREATE UNIQUE INDEX IF NOT EXISTS idx_spirit_field_plot_player_plot ON SpiritFieldPlots(PlayerId, PlotNumber);",
                """
                CREATE TABLE IF NOT EXISTS SpiritFieldSystems (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PlayerId TEXT NOT NULL,
                    FieldLevel INTEGER NOT NULL DEFAULT 1,
                    UnlockedPlots INTEGER NOT NULL DEFAULT 1,
                    MaxPlots INTEGER NOT NULL DEFAULT 9,
                    GlobalYieldBonus INTEGER NOT NULL DEFAULT 0,
                    GlobalGrowthSpeedBonus INTEGER NOT NULL DEFAULT 0,
                    TodayPlantCount INTEGER NOT NULL DEFAULT 0,
                    TodayHarvestCount INTEGER NOT NULL DEFAULT 0,
                    DailyResetTime TEXT NOT NULL,
                    TotalPlantCount INTEGER NOT NULL DEFAULT 0,
                    TotalHarvestCount INTEGER NOT NULL DEFAULT 0,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                "CREATE UNIQUE INDEX IF NOT EXISTS idx_spirit_field_system_player ON SpiritFieldSystems(PlayerId);",
                """
                CREATE TABLE IF NOT EXISTS SpiritFieldSystemConfigs (
                    ConfigId TEXT PRIMARY KEY,
                    DefaultFieldLevel INTEGER NOT NULL DEFAULT 1,
                    DefaultUnlockedPlots INTEGER NOT NULL DEFAULT 3,
                    DefaultMaxPlots INTEGER NOT NULL DEFAULT 9,
                    DefaultInventoryCapacity INTEGER NOT NULL DEFAULT 100,
                    PlotUpgradeGoldPerLevel INTEGER NOT NULL DEFAULT 1000,
                    PlotUpgradeSpiritStonePerLevel INTEGER NOT NULL DEFAULT 10,
                    PlotUpgradeYieldBonusPerLevel INTEGER NOT NULL DEFAULT 5,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS SpiritFieldSpeedUpItemConfigs (
                    ItemId TEXT PRIMARY KEY,
                    SpeedUpSeconds INTEGER NOT NULL DEFAULT 3600,
                    SortOrder INTEGER NOT NULL DEFAULT 0,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS PlayerInitialResourceConfigs (
                    ConfigId TEXT PRIMARY KEY,
                    StartLevel INTEGER NOT NULL DEFAULT 1,
                    StartExp INTEGER NOT NULL DEFAULT 0,
                    StartGold INTEGER NOT NULL DEFAULT 1000,
                    StartSpiritStone INTEGER NOT NULL DEFAULT 0,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS ChatMessages (
                    MessageId TEXT PRIMARY KEY,
                    ChannelType TEXT NOT NULL,
                    SenderId TEXT NOT NULL,
                    SenderName TEXT NOT NULL,
                    Content TEXT NOT NULL,
                    SendTime TEXT NOT NULL
                );
                """
                ,
                "CREATE INDEX IF NOT EXISTS idx_chat_channel_time ON ChatMessages(ChannelType, SendTime);",
                """
                CREATE TABLE IF NOT EXISTS sect_templates (
                    SectId TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Description TEXT NULL,
                    Icon TEXT NULL,
                    PortraitPath TEXT NULL,
                    HeartSutraIdsJson TEXT NULL,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    SortOrder INTEGER NOT NULL DEFAULT 0,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS heart_sutra_templates (
                    SutraId TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Description TEXT NULL,
                    SectId TEXT NOT NULL,
                    MaxLayer INTEGER NOT NULL DEFAULT 10,
                    LayersJson TEXT NOT NULL DEFAULT '[]',
                    SortOrder INTEGER NOT NULL DEFAULT 0,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_heart_sutra_sect ON heart_sutra_templates(SectId);",
                """
                CREATE TABLE IF NOT EXISTS player_heart_sutras (
                    GID TEXT PRIMARY KEY,
                    PlayerId TEXT NOT NULL,
                    SutraId TEXT NOT NULL,
                    CurrentLayer INTEGER NOT NULL DEFAULT 0,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                "CREATE UNIQUE INDEX IF NOT EXISTS idx_player_heart_sutra_unique ON player_heart_sutras(PlayerId, SutraId);",
                """
                CREATE TABLE IF NOT EXISTS sect_donation_records (
                    GID TEXT PRIMARY KEY,
                    PlayerId TEXT NOT NULL,
                    GuildId TEXT NOT NULL,
                    GoldDonated INTEGER NOT NULL DEFAULT 0,
                    ContributionEarned INTEGER NOT NULL DEFAULT 0,
                    DonateTime TEXT NOT NULL,
                    DonateDate TEXT NOT NULL
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_sect_donation_player_date ON sect_donation_records(PlayerId, DonateDate);",
                """
                CREATE TABLE IF NOT EXISTS sect_tournaments (
                    TournamentId TEXT PRIMARY KEY,
                    GuildId TEXT NOT NULL,
                    StartTime TEXT NOT NULL,
                    EndTime TEXT NOT NULL,
                    State INTEGER NOT NULL DEFAULT 0,
                    ResultsJson TEXT NULL,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_sect_tournament_guild ON sect_tournaments(GuildId);",
                """
                CREATE TABLE IF NOT EXISTS sect_tournament_matches (
                    MatchId TEXT PRIMARY KEY,
                    TournamentId TEXT NOT NULL,
                    Player1Id TEXT NOT NULL,
                    Player1Name TEXT NOT NULL,
                    Player2Id TEXT NOT NULL,
                    Player2Name TEXT NOT NULL,
                    Round INTEGER NOT NULL DEFAULT 0,
                    WinnerId TEXT NULL,
                    BattleLogJson TEXT NULL,
                    MatchTime TEXT NOT NULL
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_sect_tournament_match_tournament ON sect_tournament_matches(TournamentId);",
                """
                CREATE TABLE IF NOT EXISTS genius_tournaments (
                    TournamentId TEXT PRIMARY KEY,
                    Season INTEGER NOT NULL DEFAULT 1,
                    StartTime TEXT NOT NULL,
                    EndTime TEXT NOT NULL,
                    State INTEGER NOT NULL DEFAULT 0,
                    ResultsJson TEXT NULL,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS genius_tournament_matches (
                    MatchId TEXT PRIMARY KEY,
                    TournamentId TEXT NOT NULL,
                    Player1Id TEXT NOT NULL,
                    Player1Name TEXT NOT NULL,
                    Player1SectName TEXT NOT NULL,
                    Player2Id TEXT NOT NULL,
                    Player2Name TEXT NOT NULL,
                    Player2SectName TEXT NOT NULL,
                    Round INTEGER NOT NULL DEFAULT 0,
                    WinnerId TEXT NULL,
                    BattleLogJson TEXT NULL,
                    MatchTime TEXT NOT NULL
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_genius_tournament_match_tournament ON genius_tournament_matches(TournamentId);",
                """
                CREATE TABLE IF NOT EXISTS sect_tournament_schedules (
                    ScheduleId TEXT PRIMARY KEY,
                    SpawnTimeText TEXT NOT NULL DEFAULT '20:00',
                    TimeZoneId TEXT NOT NULL DEFAULT 'China Standard Time',
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    DurationMinutes INTEGER NOT NULL DEFAULT 60,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS sect_boss_templates (
                    BossId TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    MonsterTemplateId TEXT NOT NULL,
                    PortraitPath TEXT NULL,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    DurationMinutes INTEGER NOT NULL DEFAULT 30,
                    ParticipationRewardContribution INTEGER NOT NULL DEFAULT 0,
                    Rank1RewardContribution INTEGER NOT NULL DEFAULT 0,
                    Rank2RewardContribution INTEGER NOT NULL DEFAULT 0,
                    Rank3RewardContribution INTEGER NOT NULL DEFAULT 0,
                    SortOrder INTEGER NOT NULL DEFAULT 0,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS sect_boss_instances (
                    InstanceId TEXT PRIMARY KEY,
                    GuildId TEXT NOT NULL,
                    BossId TEXT NOT NULL,
                    BossName TEXT NOT NULL,
                    MonsterTemplateId TEXT NOT NULL,
                    State INTEGER NOT NULL DEFAULT 1,
                    SpawnedAtUtc TEXT NOT NULL,
                    EndAtUtc TEXT NOT NULL,
                    CurrentHp INTEGER NOT NULL DEFAULT 0,
                    MaxHp INTEGER NOT NULL DEFAULT 0,
                    CombatStateJson TEXT NOT NULL DEFAULT '{}',
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_sect_boss_instance_guild ON sect_boss_instances(GuildId);",
                """
                CREATE TABLE IF NOT EXISTS sect_shop_configs (
                    ShopId TEXT PRIMARY KEY,
                    ShopName TEXT NOT NULL,
                    Description TEXT NULL,
                    IsOpen INTEGER NOT NULL DEFAULT 1,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS sect_shop_items (
                    GID TEXT PRIMARY KEY,
                    ShopId TEXT NOT NULL,
                    ItemId TEXT NOT NULL,
                    ItemType INTEGER NOT NULL DEFAULT 0,
                    ContributionCost INTEGER NOT NULL DEFAULT 0,
                    Stock INTEGER NOT NULL DEFAULT -1,
                    DailyLimit INTEGER NOT NULL DEFAULT -1,
                    SortOrder INTEGER NOT NULL DEFAULT 0,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_sect_shop_item_shop ON sect_shop_items(ShopId);",
                """
                CREATE TABLE IF NOT EXISTS sect_blessing_configs (
                    BlessingId TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Description TEXT NULL,
                    RequiredGuildLevel INTEGER NOT NULL DEFAULT 1,
                    BuffId TEXT NOT NULL,
                    SortOrder INTEGER NOT NULL DEFAULT 0,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                // 图鉴抽奖系统表
                """
                CREATE TABLE IF NOT EXISTS text_collection_series (
                    SeriesId TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Description TEXT NULL,
                    Icon TEXT NULL,
                    SortOrder INTEGER NOT NULL DEFAULT 0,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS text_collection_item (
                    ItemId TEXT PRIMARY KEY,
                    SeriesId TEXT NOT NULL,
                    Character TEXT NOT NULL,
                    SlotIndex INTEGER NOT NULL DEFAULT 0,
                    SortOrder INTEGER NOT NULL DEFAULT 0,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_text_collection_item_series ON text_collection_item(SeriesId);",
                """
                CREATE TABLE IF NOT EXISTS text_collection_bonus (
                    BonusId TEXT PRIMARY KEY,
                    SeriesId TEXT NOT NULL,
                    AttrType TEXT NOT NULL,
                    AttrValue REAL NOT NULL DEFAULT 0,
                    ValueType INTEGER NOT NULL DEFAULT 0,
                    SortOrder INTEGER NOT NULL DEFAULT 0,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_text_collection_bonus_series ON text_collection_bonus(SeriesId);",
                """
                CREATE TABLE IF NOT EXISTS image_collection_series (
                    SeriesId TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Description TEXT NULL,
                    Icon TEXT NULL,
                    SortOrder INTEGER NOT NULL DEFAULT 0,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS image_collection_item (
                    ItemId TEXT PRIMARY KEY,
                    SeriesId TEXT NOT NULL,
                    ImageName TEXT NOT NULL,
                    ThumbUrl TEXT NULL,
                    OriginalUrl TEXT NULL,
                    SortOrder INTEGER NOT NULL DEFAULT 0,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_image_collection_item_series ON image_collection_item(SeriesId);",
                """
                CREATE TABLE IF NOT EXISTS image_collection_bonus (
                    BonusId TEXT PRIMARY KEY,
                    SeriesId TEXT NOT NULL,
                    AttrType TEXT NOT NULL,
                    AttrValue REAL NOT NULL DEFAULT 0,
                    ValueType INTEGER NOT NULL DEFAULT 0,
                    SortOrder INTEGER NOT NULL DEFAULT 0,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_image_collection_bonus_series ON image_collection_bonus(SeriesId);",
                """
                CREATE TABLE IF NOT EXISTS lottery_pool (
                    PoolId TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    LotteryType INTEGER NOT NULL DEFAULT 0,
                    CostType INTEGER NOT NULL DEFAULT 0,
                    CostItemId TEXT NULL,
                    CostAmount INTEGER NOT NULL DEFAULT 0,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    StartTime TEXT NULL,
                    EndTime TEXT NULL,
                    SupportSingle INTEGER NOT NULL DEFAULT 1,
                    SupportTen INTEGER NOT NULL DEFAULT 0,
                    DailyLimit INTEGER NOT NULL DEFAULT -1,
                    TotalLimit INTEGER NOT NULL DEFAULT -1,
                    SortOrder INTEGER NOT NULL DEFAULT 0,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS lottery_prize (
                    PrizeId TEXT PRIMARY KEY,
                    PoolId TEXT NOT NULL,
                    RewardType INTEGER NOT NULL DEFAULT 0,
                    RewardTargetId TEXT NULL,
                    RewardAmount INTEGER NOT NULL DEFAULT 0,
                    Probability INTEGER NOT NULL DEFAULT 0,
                    SortOrder INTEGER NOT NULL DEFAULT 0,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_lottery_prize_pool ON lottery_prize(PoolId);",
                """
                CREATE TABLE IF NOT EXISTS player_collection (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PlayerId TEXT NOT NULL,
                    CollectionType INTEGER NOT NULL DEFAULT 0,
                    SeriesId TEXT NOT NULL,
                    ItemId TEXT NOT NULL,
                    OwnedCount INTEGER NOT NULL DEFAULT 0,
                    FirstGetTime TEXT NOT NULL,
                    LastGetTime TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL,
                    IsDeleted INTEGER NOT NULL DEFAULT 0
                );
                """,
                "CREATE UNIQUE INDEX IF NOT EXISTS idx_player_collection_unique ON player_collection(PlayerId, CollectionType, SeriesId, ItemId);",
                "CREATE INDEX IF NOT EXISTS idx_player_collection_player ON player_collection(PlayerId);",
                """
                CREATE TABLE IF NOT EXISTS lottery_log (
                    LogId TEXT PRIMARY KEY,
                    PlayerId TEXT NOT NULL,
                    PoolId TEXT NOT NULL,
                    LotteryType INTEGER NOT NULL DEFAULT 0,
                    CostType INTEGER NOT NULL DEFAULT 0,
                    CostItemId TEXT NULL,
                    CostAmount INTEGER NOT NULL DEFAULT 0,
                    PrizeId TEXT NOT NULL,
                    RewardType INTEGER NOT NULL DEFAULT 0,
                    RewardTargetId TEXT NULL,
                    RewardName TEXT NOT NULL,
                    RewardAmount INTEGER NOT NULL DEFAULT 0,
                    IsThanks INTEGER NOT NULL DEFAULT 0,
                    RandomValue INTEGER NOT NULL DEFAULT 0,
                    LotteryTime TEXT NOT NULL,
                    PlayerIp TEXT NULL
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_lottery_log_player_time ON lottery_log(PlayerId, LotteryTime);",
                "CREATE INDEX IF NOT EXISTS idx_lottery_log_pool ON lottery_log(PoolId);",
                """
                CREATE TABLE IF NOT EXISTS EquipmentRerollSystemConfigs (
                    ConfigId TEXT PRIMARY KEY,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    RerollStoneItemId TEXT NOT NULL DEFAULT 'reroll_stone',
                    BaseStoneCost INTEGER NOT NULL DEFAULT 1,
                    ExtraStoneCostPerLockedLine INTEGER NOT NULL DEFAULT 1,
                    MaxLockedLineCount INTEGER NOT NULL DEFAULT 3,
                    BaseGoldCost INTEGER NOT NULL DEFAULT 300,
                    GoldCostPerEquipmentLevel INTEGER NOT NULL DEFAULT 25,
                    QualityGoldMultipliersJson TEXT NULL,
                    RerollCountGoldGrowthPercent INTEGER NOT NULL DEFAULT 3,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS EquipmentEnhanceRuleConfigs (
                    GID INTEGER PRIMARY KEY AUTOINCREMENT,
                    MinEquipmentLevel INTEGER NOT NULL DEFAULT 1,
                    MaxEquipmentLevel INTEGER NOT NULL DEFAULT 10,
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
                """,
                """
                CREATE TABLE IF NOT EXISTS EquipmentRerollCostRuleConfigs (
                    GID INTEGER PRIMARY KEY AUTOINCREMENT,
                    MinEquipmentLevel INTEGER NOT NULL DEFAULT 1,
                    MaxEquipmentLevel INTEGER NOT NULL DEFAULT 10,
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
                """,
                """
                CREATE TABLE IF NOT EXISTS EquipmentDecomposeRuleConfigs (
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
                """,
                // 洗练词条池表重构：移除 MinValue/MaxValue/IsPercentage/Weight，新增 Tier
                """
                CREATE TABLE IF NOT EXISTS EquipmentRerollSlotPoolConfigs (
                    GID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Slot INTEGER NOT NULL,
                    AttributeType INTEGER NOT NULL,
                    Tier INTEGER NOT NULL DEFAULT 1,
                    MaxDuplicateCount INTEGER NOT NULL DEFAULT 1,
                    SortOrder INTEGER NOT NULL DEFAULT 0,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS EquipmentRerollAttributeValueConfigs (
                    GID INTEGER PRIMARY KEY AUTOINCREMENT,
                    AttributeType INTEGER NOT NULL,
                    Tier INTEGER NOT NULL,
                    MinValue TEXT NOT NULL DEFAULT '0',
                    MaxValue TEXT NOT NULL DEFAULT '0',
                    IsPercentage INTEGER NOT NULL DEFAULT 0,
                    SortOrder INTEGER NOT NULL DEFAULT 0,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS EquipmentRerollTierConfigs (
                    GID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Tier INTEGER NOT NULL,
                    Name TEXT NOT NULL,
                    Color TEXT NOT NULL DEFAULT '#9ca3af',
                    Weight INTEGER NOT NULL DEFAULT 10,
                    ValueMultiplier TEXT NOT NULL DEFAULT '1.00',
                    SortOrder INTEGER NOT NULL DEFAULT 0,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
                """,
                // PlayerFavorability
                @"CREATE TABLE IF NOT EXISTS PlayerFavorability (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PlayerId TEXT NOT NULL,
                    TargetPlayerId TEXT NOT NULL,
                    Value INTEGER NOT NULL DEFAULT 0,
                    TodayGiftJson TEXT,
                    TodayPartyBattleJson TEXT,
                    LastGiftTime DATETIME,
                    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    IsDeleted INTEGER NOT NULL DEFAULT 0
                )",
                @"CREATE UNIQUE INDEX IF NOT EXISTS idx_player_favorability_pair ON PlayerFavorability(PlayerId, TargetPlayerId)",
                @"CREATE INDEX IF NOT EXISTS idx_player_favorability_player ON PlayerFavorability(PlayerId)",
                @"CREATE INDEX IF NOT EXISTS idx_player_favorability_target ON PlayerFavorability(TargetPlayerId)",
                // FavorabilityGiftLog
                @"CREATE TABLE IF NOT EXISTS FavorabilityGiftLog (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PlayerId TEXT NOT NULL,
                    TargetPlayerId TEXT NOT NULL,
                    ItemId TEXT,
                    ItemName TEXT NOT NULL,
                    FavorabilityChange INTEGER NOT NULL,
                    GiftTime DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    IsDeleted INTEGER NOT NULL DEFAULT 0
                )",
                @"CREATE INDEX IF NOT EXISTS idx_favorability_gift_log_player ON FavorabilityGiftLog(PlayerId)",
                @"CREATE INDEX IF NOT EXISTS idx_favorability_gift_log_target ON FavorabilityGiftLog(TargetPlayerId)",
                // FavorabilityLevelConfig
                @"CREATE TABLE IF NOT EXISTS FavorabilityLevelConfig (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Level INTEGER NOT NULL,
                    Name TEXT NOT NULL,
                    MinValue INTEGER NOT NULL,
                    MaxValue INTEGER NOT NULL,
                    Color TEXT NOT NULL DEFAULT '#CCCCCC',
                    SortOrder INTEGER NOT NULL DEFAULT 0,
                    RewardJson TEXT NULL,
                    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
                )",
                // FavorabilityAchievement
                @"CREATE TABLE IF NOT EXISTS FavorabilityAchievement (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PlayerId TEXT NOT NULL,
                    AchievementId TEXT NOT NULL,
                    ClaimedAt DATETIME NOT NULL,
                    IsDeleted INTEGER NOT NULL DEFAULT 0,
                    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
                )",
                @"CREATE INDEX IF NOT EXISTS idx_fav_achievement_player ON FavorabilityAchievement(PlayerId)",
                @"CREATE UNIQUE INDEX IF NOT EXISTS idx_fav_achievement_unique ON FavorabilityAchievement(PlayerId, AchievementId)",

                // 秘境实例模板
                """
                CREATE TABLE IF NOT EXISTS DungeonInstanceTemplates (
                    Id TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Description TEXT NOT NULL DEFAULT '',
                    Enabled INTEGER NOT NULL DEFAULT 1,
                    RecommendedLevel INTEGER NOT NULL DEFAULT 1,
                    DailyEnterLimit INTEGER NOT NULL DEFAULT 1,
                    TickIntervalSeconds INTEGER NOT NULL DEFAULT 30,
                    OpenScheduleJson TEXT NULL,
                    EntryCostsJson TEXT NULL,
                    EventGroupId TEXT NULL,
                    AutoMedicineConfigJson TEXT NULL,
                    EncounterConfigJson TEXT NULL,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
                """,

                // 秘境运行实例
                """
                CREATE TABLE IF NOT EXISTS DungeonInstances (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PlayerId TEXT NOT NULL,
                    DungeonId TEXT NOT NULL,
                    Status INTEGER NOT NULL DEFAULT 0,
                    EnterTime TEXT NOT NULL,
                    LastTickTime TEXT NULL,
                    NextTickTime TEXT NULL,
                    SettleTime TEXT NULL,
                    SettleReason INTEGER NULL,
                    SnapshotJson TEXT NULL,
                    CurrentHp INTEGER NOT NULL DEFAULT 0,
                    CurrentMp INTEGER NOT NULL DEFAULT 0,
                    MaxHp INTEGER NOT NULL DEFAULT 0,
                    MaxMp INTEGER NOT NULL DEFAULT 0,
                    ActiveBuffsJson TEXT NULL,
                    ExploreLogJson TEXT NULL,
                    PartyId TEXT NULL,
                    IsPartyLeader INTEGER NOT NULL DEFAULT 0,
                    CreatedAt TEXT NOT NULL DEFAULT '2000-01-01 00:00:00',
                    UpdatedAt TEXT NOT NULL DEFAULT '2000-01-01 00:00:00',
                    IsDeleted INTEGER NOT NULL DEFAULT 0
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_dungeon_instance_player ON DungeonInstances(PlayerId);",
                "CREATE INDEX IF NOT EXISTS idx_dungeon_instance_dungeon ON DungeonInstances(DungeonId);",
                "CREATE INDEX IF NOT EXISTS idx_dungeon_instance_status ON DungeonInstances(Status);",

                // 秘境每日记录
                """
                CREATE TABLE IF NOT EXISTS DungeonInstanceDailyRecords (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PlayerId TEXT NOT NULL,
                    DungeonId TEXT NOT NULL,
                    EnterDate TEXT NOT NULL,
                    EnterCount INTEGER NOT NULL DEFAULT 0,
                    CreatedAt TEXT NOT NULL DEFAULT '2000-01-01 00:00:00',
                    UpdatedAt TEXT NOT NULL DEFAULT '2000-01-01 00:00:00',
                    IsDeleted INTEGER NOT NULL DEFAULT 0
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_dungeon_daily_player ON DungeonInstanceDailyRecords(PlayerId);",
                "CREATE UNIQUE INDEX IF NOT EXISTS idx_dungeon_daily_unique ON DungeonInstanceDailyRecords(PlayerId, DungeonId, EnterDate);",

                // 秘境收益池
                """
                CREATE TABLE IF NOT EXISTS DungeonRewardPools (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PlayerId TEXT NOT NULL,
                    InstanceId TEXT NOT NULL,
                    RewardType TEXT NOT NULL,
                    RewardId TEXT NULL,
                    Quantity INTEGER NOT NULL DEFAULT 0,
                    Source TEXT NOT NULL,
                    DeathKeep INTEGER NOT NULL DEFAULT 0,
                    CreatedAt TEXT NOT NULL DEFAULT '2000-01-01 00:00:00',
                    UpdatedAt TEXT NOT NULL DEFAULT '2000-01-01 00:00:00',
                    IsDeleted INTEGER NOT NULL DEFAULT 0
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_dungeon_reward_player ON DungeonRewardPools(PlayerId);",
                "CREATE INDEX IF NOT EXISTS idx_dungeon_reward_instance ON DungeonRewardPools(InstanceId);",

                // 秘境事件配置
                """
                CREATE TABLE IF NOT EXISTS DungeonEventConfigs (
                    Id TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Description TEXT NOT NULL DEFAULT '',
                    DungeonId TEXT NOT NULL DEFAULT '*',
                    EventType INTEGER NOT NULL,
                    Weight INTEGER NOT NULL DEFAULT 10,
                    Enabled INTEGER NOT NULL DEFAULT 1,
                    EventDataJson TEXT NULL,
                    DeathKeep INTEGER NULL,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_dungeon_event_dungeon ON DungeonEventConfigs(DungeonId);",
                "CREATE INDEX IF NOT EXISTS idx_dungeon_event_type ON DungeonEventConfigs(EventType);",

                // 秘境事件组
                """
                CREATE TABLE IF NOT EXISTS DungeonEventGroups (
                    Id TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Description TEXT NULL,
                    GroupItemsJson TEXT NULL,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
                """,

                // 秘境临时队伍
                """
                CREATE TABLE IF NOT EXISTS DungeonParties (
                    PartyId TEXT PRIMARY KEY,
                    DungeonId TEXT NOT NULL,
                    LeaderPlayerId TEXT NOT NULL,
                    MemberJson TEXT NOT NULL DEFAULT '[]',
                    CreateTime TEXT NOT NULL,
                    LastUpdateTime TEXT NOT NULL
                );
                """,
                // 建议反馈系统
                """
                CREATE TABLE IF NOT EXISTS PlayerFeedbacks (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PlayerId TEXT NOT NULL,
                    Type TEXT NOT NULL DEFAULT 'Suggestion',
                    Title TEXT NOT NULL,
                    Content TEXT NOT NULL,
                    Status TEXT NOT NULL DEFAULT 'Pending',
                    AdminReply TEXT NULL,
                    InternalNote TEXT NULL,
                    HandledByAdminId TEXT NULL,
                    HandledAt TEXT NULL,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL,
                    IsDeleted INTEGER NOT NULL DEFAULT 0
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_feedback_player ON PlayerFeedbacks(PlayerId, CreatedAt);",
                "CREATE INDEX IF NOT EXISTS idx_feedback_status ON PlayerFeedbacks(Status, CreatedAt);",
                """
                CREATE TABLE IF NOT EXISTS PlayerFeedbackAttachments (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    FeedbackId INTEGER NOT NULL,
                    RelativePath TEXT NOT NULL,
                    OriginalFileName TEXT NULL,
                    ContentType TEXT NOT NULL,
                    FileSize INTEGER NOT NULL,
                    SortOrder INTEGER NOT NULL DEFAULT 0,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL,
                    IsDeleted INTEGER NOT NULL DEFAULT 0
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_feedback_attachment ON PlayerFeedbackAttachments(FeedbackId, SortOrder);",
                """
                CREATE TABLE IF NOT EXISTS PlayerFeedbackStatusHistories (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    FeedbackId INTEGER NOT NULL,
                    FromStatus TEXT NULL,
                    ToStatus TEXT NOT NULL,
                    AdminId TEXT NOT NULL,
                    ReplySnapshot TEXT NULL,
                    NoteSnapshot TEXT NULL,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL,
                    IsDeleted INTEGER NOT NULL DEFAULT 0
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_feedback_history ON PlayerFeedbackStatusHistories(FeedbackId, CreatedAt);",
                // 邮件系统
                """
                CREATE TABLE IF NOT EXISTS MailMessages (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    RecipientId TEXT NULL,
                    SenderType TEXT NOT NULL DEFAULT 'System',
                    SenderName TEXT NOT NULL DEFAULT '系统',
                    Title TEXT NOT NULL,
                    Content TEXT NOT NULL DEFAULT '',
                    AttachmentsJson TEXT NULL,
                    IsRead INTEGER NOT NULL DEFAULT 0,
                    IsClaimed INTEGER NOT NULL DEFAULT 0,
                    IsGlobal INTEGER NOT NULL DEFAULT 0,
                    CreatedAt TEXT NOT NULL,
                    ExpireAt TEXT NOT NULL
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_mail_messages_recipient ON MailMessages(RecipientId);",
                "CREATE INDEX IF NOT EXISTS idx_mail_messages_global ON MailMessages(IsGlobal, ExpireAt);",
                """
                CREATE TABLE IF NOT EXISTS MailGlobalClaimRecords (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    MailId INTEGER NOT NULL,
                    PlayerId TEXT NOT NULL,
                    ClaimedAt TEXT NOT NULL
                );
                """,
                "CREATE UNIQUE INDEX IF NOT EXISTS idx_mail_global_claim_unique ON MailGlobalClaimRecords(MailId, PlayerId);",
                // 称号系统
                """
                CREATE TABLE IF NOT EXISTS TitleTemplates (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TitleId TEXT NOT NULL,
                    Name TEXT NOT NULL,
                    Description TEXT NULL,
                    Source TEXT NULL,
                    Rarity TEXT NOT NULL DEFAULT 'common',
                    IconPath TEXT NULL,
                    ImagePath TEXT NULL,
                    IsVisible INTEGER NOT NULL DEFAULT 1,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
                """,
                "CREATE UNIQUE INDEX IF NOT EXISTS idx_title_templates_title_id ON TitleTemplates(TitleId);",
                """
                CREATE TABLE IF NOT EXISTS PlayerTitles (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PlayerId TEXT NOT NULL,
                    TitleId TEXT NOT NULL,
                    IsEquipped INTEGER NOT NULL DEFAULT 0,
                    UnlockedAt TEXT NOT NULL
                );
                """,
                "CREATE UNIQUE INDEX IF NOT EXISTS idx_player_titles_unique ON PlayerTitles(PlayerId, TitleId);",
                "CREATE INDEX IF NOT EXISTS idx_player_titles_player ON PlayerTitles(PlayerId);",
                // PVP竞技场
                """
                CREATE TABLE IF NOT EXISTS ArenaPlayers (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PlayerId TEXT NOT NULL,
                    Points INTEGER NOT NULL DEFAULT 1000,
                    Rank INTEGER NOT NULL DEFAULT 0,
                    Wins INTEGER NOT NULL DEFAULT 0,
                    Losses INTEGER NOT NULL DEFAULT 0,
                    WinStreak INTEGER NOT NULL DEFAULT 0,
                    BestRank INTEGER NOT NULL DEFAULT 0,
                    DailyBattlesUsed INTEGER NOT NULL DEFAULT 0,
                    DailyBattlesPurchased INTEGER NOT NULL DEFAULT 0,
                    LastBattleAt TEXT NULL,
                    SeasonNumber INTEGER NOT NULL DEFAULT 1,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL,
                    BannedUntil TEXT NULL,
                    BanReason TEXT NULL
                );
                """,
                "CREATE UNIQUE INDEX IF NOT EXISTS idx_arena_players_player ON ArenaPlayers(PlayerId);",
                "CREATE INDEX IF NOT EXISTS idx_arena_players_rank ON ArenaPlayers(Rank);",
                """
                CREATE TABLE IF NOT EXISTS ArenaBattleLogs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    AttackerId TEXT NOT NULL,
                    DefenderId TEXT NOT NULL,
                    AttackerName TEXT NOT NULL,
                    DefenderName TEXT NOT NULL,
                    AttackerPointsBefore INTEGER NOT NULL DEFAULT 0,
                    DefenderPointsBefore INTEGER NOT NULL DEFAULT 0,
                    AttackerPointsAfter INTEGER NOT NULL DEFAULT 0,
                    DefenderPointsAfter INTEGER NOT NULL DEFAULT 0,
                    WinnerId TEXT NOT NULL,
                    BattleLogJson TEXT NULL,
                    SeasonNumber INTEGER NOT NULL DEFAULT 1,
                    CreatedAt TEXT NOT NULL
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_arena_battle_logs_attacker ON ArenaBattleLogs(AttackerId);",
                "CREATE INDEX IF NOT EXISTS idx_arena_battle_logs_defender ON ArenaBattleLogs(DefenderId);",
                // 通天塔
                """
                CREATE TABLE IF NOT EXISTS TowerProgress (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PlayerId TEXT NOT NULL,
                    HighestFloor INTEGER NOT NULL DEFAULT 0,
                    CurrentFloor INTEGER NOT NULL DEFAULT 1,
                    DailyAttemptsUsed INTEGER NOT NULL DEFAULT 0,
                    DailyAttemptsPurchased INTEGER NOT NULL DEFAULT 0,
                    BestClearTimeMs INTEGER NULL,
                    LastAttemptAt TEXT NULL,
                    SeasonNumber INTEGER NOT NULL DEFAULT 1,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL
                );
                """,
                "CREATE UNIQUE INDEX IF NOT EXISTS idx_tower_progress_player ON TowerProgress(PlayerId);",
                """
                CREATE TABLE IF NOT EXISTS TowerFloorConfigs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Floor INTEGER NOT NULL,
                    MonsterTemplateIdsJson TEXT NOT NULL DEFAULT '[]',
                    MonsterCount INTEGER NOT NULL DEFAULT 1,
                    StatMultiplier REAL NOT NULL DEFAULT 1.0,
                    RewardGold INTEGER NOT NULL DEFAULT 0,
                    RewardExp INTEGER NOT NULL DEFAULT 0,
                    MilestoneRewardJson TEXT NULL,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
                """,
                "CREATE UNIQUE INDEX IF NOT EXISTS idx_tower_floor_configs_floor ON TowerFloorConfigs(Floor);",
                """
                CREATE TABLE IF NOT EXISTS TowerBattleLogs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PlayerId TEXT NOT NULL,
                    Floor INTEGER NOT NULL,
                    IsWin INTEGER NOT NULL DEFAULT 0,
                    BattleLogJson TEXT NULL,
                    CreatedAt TEXT NOT NULL
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_tower_battle_logs_player ON TowerBattleLogs(PlayerId);",
                // 宝石系统
                """
                CREATE TABLE IF NOT EXISTS GemTemplates (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    GemId TEXT NOT NULL,
                    Name TEXT NOT NULL,
                    Level INTEGER NOT NULL DEFAULT 1,
                    AttributeType TEXT NOT NULL,
                    BonusValue INTEGER NOT NULL DEFAULT 0,
                    BonusMode TEXT NOT NULL DEFAULT 'Flat',
                    IconPath TEXT NULL,
                    Quality INTEGER NOT NULL DEFAULT 1,
                    SynthCount INTEGER NOT NULL DEFAULT 3,
                    SynthFromGemId TEXT NULL,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
                """,
                "CREATE UNIQUE INDEX IF NOT EXISTS idx_gem_templates_gem_id ON GemTemplates(GemId);",
                // 寄售行
                """
                CREATE TABLE IF NOT EXISTS MarketListings (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SellerId TEXT NOT NULL,
                    SellerName TEXT NOT NULL,
                    ItemType TEXT NOT NULL,
                    ItemId TEXT NULL,
                    EquipmentInstanceId TEXT NULL,
                    Quantity INTEGER NOT NULL DEFAULT 1,
                    Price INTEGER NOT NULL DEFAULT 0,
                    CurrencyType TEXT NOT NULL DEFAULT 'Gold',
                    Status TEXT NOT NULL DEFAULT 'Listed',
                    IsBound INTEGER NOT NULL DEFAULT 0,
                    BuyerId TEXT NULL,
                    SoldAt TEXT NULL,
                    CreatedAt TEXT NOT NULL,
                    ExpireAt TEXT NOT NULL
                );
                """,
                "CREATE INDEX IF NOT EXISTS idx_market_listings_seller ON MarketListings(SellerId);",
                "CREATE INDEX IF NOT EXISTS idx_market_listings_status ON MarketListings(Status);",
                "CREATE INDEX IF NOT EXISTS idx_market_listings_expire ON MarketListings(ExpireAt);",
                """
                CREATE TABLE IF NOT EXISTS MarketConfigs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ConfigKey TEXT NOT NULL,
                    ConfigValue TEXT NOT NULL
                );
                """,
                "CREATE UNIQUE INDEX IF NOT EXISTS idx_market_configs_key ON MarketConfigs(ConfigKey);",
            };

            foreach (var command in commands)
            {
                _db.Ado.ExecuteCommand(command);
            }

            EnsureSqliteCoreSchemaCompatibility();
            // 纯净版：宗门种子由基准数据库提供，项目启动不再自动写入种子数据。
            // SectSeedData.SeedAll(_db);
            _logger?.LogInformation("SQLite 核心开发库初始化完成：{ConnectionString}", _connectionString);
        }

        /// <summary>
        /// 中文注释：
        /// SQLite 开发库会长期复用同一份 db 文件，单靠 CREATE TABLE IF NOT EXISTS
        /// 无法把新列补到已有表结构里，所以这里集中做一次“向后兼容补列”。
        /// </summary>
        private void EnsureSqliteCoreSchemaCompatibility()
        {
            // 洗练词条池表迁移：旧表有 Weight/MinValue/MaxValue/IsPercentage 列，需要重建
            MigrateSlotPoolConfigsIfNeeded();

            EnsureSqliteColumnExists("SkillTemplates", "SkillLevel", "ALTER TABLE SkillTemplates ADD COLUMN SkillLevel INTEGER NOT NULL DEFAULT 1;");
            EnsureSqliteColumnExists("SkillTemplates", "SkillCatalog", "ALTER TABLE SkillTemplates ADD COLUMN SkillCatalog TEXT NOT NULL DEFAULT 'legacy';");
            EnsureSqliteColumnExists("SkillTemplates", "PreviousSkillId", "ALTER TABLE SkillTemplates ADD COLUMN PreviousSkillId INTEGER NULL;");
            EnsureSqliteColumnExists("SkillTemplates", "NextSkillId", "ALTER TABLE SkillTemplates ADD COLUMN NextSkillId INTEGER NULL;");
            EnsureSqliteColumnExists("SkillTemplates", "UpgradeConditionsJson", "ALTER TABLE SkillTemplates ADD COLUMN UpgradeConditionsJson TEXT NULL;");
            EnsureSqliteColumnExists("Users", "CurrentTitle", "ALTER TABLE Users ADD COLUMN CurrentTitle TEXT NULL;");
            EnsureSqliteColumnExists("Users", "Profession", "ALTER TABLE Users ADD COLUMN Profession TEXT NOT NULL DEFAULT 'warrior';");
            EnsureSqliteColumnExists("Users", "AvatarImagePath", "ALTER TABLE Users ADD COLUMN AvatarImagePath TEXT NULL;");
            EnsureSqliteColumnExists("Users", "OwnedSkillIdsJson", "ALTER TABLE Users ADD COLUMN OwnedSkillIdsJson TEXT NULL;");
            EnsureSqliteColumnExists("Users", "IsBanned", "ALTER TABLE Users ADD COLUMN IsBanned INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("Users", "BanReason", "ALTER TABLE Users ADD COLUMN BanReason TEXT NULL;");
            EnsureSqliteColumnExists("Users", "BanExpiresAt", "ALTER TABLE Users ADD COLUMN BanExpiresAt TEXT NULL;");
            EnsureSqliteColumnExists("EquipmentTemplates", "CombatStyle", "ALTER TABLE EquipmentTemplates ADD COLUMN CombatStyle INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("EquipmentTemplates", "WeaponCategory", "ALTER TABLE EquipmentTemplates ADD COLUMN WeaponCategory INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("EquipmentTemplates", "IconPath", "ALTER TABLE EquipmentTemplates ADD COLUMN IconPath TEXT NULL;");
            EnsureSqliteColumnExists("EquipmentTemplates", "SeedKey", "ALTER TABLE EquipmentTemplates ADD COLUMN SeedKey TEXT NULL;");
            EnsureSqliteColumnExists("EquipmentTemplates", "IsBuiltIn", "ALTER TABLE EquipmentTemplates ADD COLUMN IsBuiltIn INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("EquipmentTemplates", "BuiltInVersion", "ALTER TABLE EquipmentTemplates ADD COLUMN BuiltInVersion TEXT NULL;");
            EnsureSqliteColumnExists("EquipmentTemplates", "LastUpdateTime", "ALTER TABLE EquipmentTemplates ADD COLUMN LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00';");
            EnsureSqliteColumnExists("ItemTemplates", "IconPath", "ALTER TABLE ItemTemplates ADD COLUMN IconPath TEXT NULL;");
            EnsureSqliteColumnExists("ItemTemplates", "ChestConfigJson", "ALTER TABLE ItemTemplates ADD COLUMN ChestConfigJson TEXT NULL;");
            EnsureSqliteColumnExists("ItemTemplates", "SkillBookConfigJson", "ALTER TABLE ItemTemplates ADD COLUMN SkillBookConfigJson TEXT NULL;");
            EnsureSqliteColumnExists("ItemTemplates", "PetEggConfigJson", "ALTER TABLE ItemTemplates ADD COLUMN PetEggConfigJson TEXT NULL;");
            EnsureSqliteColumnExists("ItemTemplates", "PillConfigJson", "ALTER TABLE ItemTemplates ADD COLUMN PillConfigJson TEXT NULL;");
            EnsureSqliteColumnExists("ItemTemplates", "FavorabilityGiftConfigJson", "ALTER TABLE ItemTemplates ADD COLUMN FavorabilityGiftConfigJson TEXT NULL;");
            EnsureSqliteColumnExists("MapTemplates", "SeedKey", "ALTER TABLE MapTemplates ADD COLUMN SeedKey TEXT NULL;");
            EnsureSqliteColumnExists("MapTemplates", "IsBuiltIn", "ALTER TABLE MapTemplates ADD COLUMN IsBuiltIn INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("MapTemplates", "BuiltInVersion", "ALTER TABLE MapTemplates ADD COLUMN BuiltInVersion TEXT NULL;");
            EnsureSqliteColumnExists("MapTemplates", "LastUpdateTime", "ALTER TABLE MapTemplates ADD COLUMN LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00';");
            EnsureSqliteColumnExists("EquipmentInstances", "BasePhysicalAttack", "ALTER TABLE EquipmentInstances ADD COLUMN BasePhysicalAttack INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("EquipmentInstances", "BaseMagicAttack", "ALTER TABLE EquipmentInstances ADD COLUMN BaseMagicAttack INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("EquipmentInstances", "BasePhysicalDefense", "ALTER TABLE EquipmentInstances ADD COLUMN BasePhysicalDefense INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("EquipmentInstances", "BaseMagicDefense", "ALTER TABLE EquipmentInstances ADD COLUMN BaseMagicDefense INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("EquipmentInstances", "RerollStatsJson", "ALTER TABLE EquipmentInstances ADD COLUMN RerollStatsJson TEXT NULL;");
            EnsureSqliteColumnExists("EquipmentInstances", "RerollCandidateJson", "ALTER TABLE EquipmentInstances ADD COLUMN RerollCandidateJson TEXT NULL;");
            EnsureSqliteColumnExists("EquipmentInstances", "RerollCount", "ALTER TABLE EquipmentInstances ADD COLUMN RerollCount INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("EquipmentInstances", "RerollCandidateCreatedAt", "ALTER TABLE EquipmentInstances ADD COLUMN RerollCandidateCreatedAt TEXT NULL;");
            EnsureSqliteColumnExists("EquipmentInstances", "GemSlotsJson", "ALTER TABLE EquipmentInstances ADD COLUMN GemSlotsJson TEXT NULL;");
            EnsureSqliteColumnExists("ItemTemplates", "IsTradeable", "ALTER TABLE ItemTemplates ADD COLUMN IsTradeable INTEGER NOT NULL DEFAULT 1;");
            EnsureSqliteColumnExists("EquipmentTemplates", "IsTradeable", "ALTER TABLE EquipmentTemplates ADD COLUMN IsTradeable INTEGER NOT NULL DEFAULT 1;");
            EnsureSqliteColumnExists("MailGlobalClaimRecords", "IsRead", "ALTER TABLE MailGlobalClaimRecords ADD COLUMN IsRead INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("MailGlobalClaimRecords", "HasClaimed", "ALTER TABLE MailGlobalClaimRecords ADD COLUMN HasClaimed INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("TitleTemplates", "SourceId", "ALTER TABLE TitleTemplates ADD COLUMN SourceId TEXT NULL;");
            EnsureSqliteColumnExists("TitleTemplates", "CreatedAt", "ALTER TABLE TitleTemplates ADD COLUMN CreatedAt TEXT NOT NULL DEFAULT '';");
            EnsureSqliteColumnExists("Users", "BattleCooldownUntilUtc", "ALTER TABLE Users ADD COLUMN BattleCooldownUntilUtc TEXT NULL;");
            EnsureSqliteColumnExists("Users", "BattleMode", "ALTER TABLE Users ADD COLUMN BattleMode INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("Users", "OfflineBattleMapId", "ALTER TABLE Users ADD COLUMN OfflineBattleMapId TEXT NULL;");
            EnsureSqliteColumnExists("Users", "OfflineBattleStartedAtUtc", "ALTER TABLE Users ADD COLUMN OfflineBattleStartedAtUtc TEXT NULL;");
            EnsureSqliteColumnExists("Users", "OfflineBattleLastTickAtUtc", "ALTER TABLE Users ADD COLUMN OfflineBattleLastTickAtUtc TEXT NULL;");
            EnsureSqliteColumnExists("Users", "OfflineBattleEndAtUtc", "ALTER TABLE Users ADD COLUMN OfflineBattleEndAtUtc TEXT NULL;");
            EnsureSqliteColumnExists("Users", "OfflineBattleSummaryJson", "ALTER TABLE Users ADD COLUMN OfflineBattleSummaryJson TEXT NULL;");
            EnsureSqliteColumnExists("Users", "ActiveDungeonInstanceId", "ALTER TABLE Users ADD COLUMN ActiveDungeonInstanceId TEXT NULL;");
            EnsureSqliteColumnExists("Users", "EquipmentAutoSellMinLevel", "ALTER TABLE Users ADD COLUMN EquipmentAutoSellMinLevel INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("Users", "EquipmentAutoSellMinQuality", "ALTER TABLE Users ADD COLUMN EquipmentAutoSellMinQuality INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("Users", "EquipmentInventoryCapacity", "ALTER TABLE Users ADD COLUMN EquipmentInventoryCapacity INTEGER NOT NULL DEFAULT 100;");
            EnsureSqliteColumnExists("Users", "ItemInventoryCapacity", "ALTER TABLE Users ADD COLUMN ItemInventoryCapacity INTEGER NOT NULL DEFAULT 100;");

            // 秘境模板：事件组字段替代事件类型权重JSON
            EnsureSqliteColumnExists("DungeonInstanceTemplates", "EventGroupId", "ALTER TABLE DungeonInstanceTemplates ADD COLUMN EventGroupId TEXT NULL;");

            // 秘境实例表补列
            EnsureSqliteColumnExists("DungeonInstances", "CreatedAt", "ALTER TABLE DungeonInstances ADD COLUMN CreatedAt TEXT NOT NULL DEFAULT '2000-01-01 00:00:00';");
            EnsureSqliteColumnExists("DungeonInstances", "UpdatedAt", "ALTER TABLE DungeonInstances ADD COLUMN UpdatedAt TEXT NOT NULL DEFAULT '2000-01-01 00:00:00';");
            EnsureSqliteColumnExists("DungeonInstances", "IsDeleted", "ALTER TABLE DungeonInstances ADD COLUMN IsDeleted INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("DungeonInstanceDailyRecords", "CreatedAt", "ALTER TABLE DungeonInstanceDailyRecords ADD COLUMN CreatedAt TEXT NOT NULL DEFAULT '2000-01-01 00:00:00';");
            EnsureSqliteColumnExists("DungeonInstanceDailyRecords", "UpdatedAt", "ALTER TABLE DungeonInstanceDailyRecords ADD COLUMN UpdatedAt TEXT NOT NULL DEFAULT '2000-01-01 00:00:00';");
            EnsureSqliteColumnExists("DungeonInstanceDailyRecords", "IsDeleted", "ALTER TABLE DungeonInstanceDailyRecords ADD COLUMN IsDeleted INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("DungeonRewardPools", "CreatedAt", "ALTER TABLE DungeonRewardPools ADD COLUMN CreatedAt TEXT NOT NULL DEFAULT '2000-01-01 00:00:00';");
            EnsureSqliteColumnExists("DungeonRewardPools", "UpdatedAt", "ALTER TABLE DungeonRewardPools ADD COLUMN UpdatedAt TEXT NOT NULL DEFAULT '2000-01-01 00:00:00';");
            EnsureSqliteColumnExists("DungeonRewardPools", "IsDeleted", "ALTER TABLE DungeonRewardPools ADD COLUMN IsDeleted INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("Users", "BreakthroughBonusPercent", "ALTER TABLE Users ADD COLUMN BreakthroughBonusPercent INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("Users", "PillBonusAttributesJson", "ALTER TABLE Users ADD COLUMN PillBonusAttributesJson TEXT NULL;");
            EnsureSqliteColumnExists("AlchemySystems", "AlchemistLevel", "ALTER TABLE AlchemySystems ADD COLUMN AlchemistLevel INTEGER NOT NULL DEFAULT 1;");
            EnsureSqliteColumnExists("AlchemySystems", "AlchemistExp", "ALTER TABLE AlchemySystems ADD COLUMN AlchemistExp INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("AlchemySystems", "ActiveRecipeId", "ALTER TABLE AlchemySystems ADD COLUMN ActiveRecipeId TEXT NULL;");
            EnsureSqliteColumnExists("AlchemySystems", "ActiveCraftStartedAt", "ALTER TABLE AlchemySystems ADD COLUMN ActiveCraftStartedAt TEXT NULL;");
            EnsureSqliteColumnExists("AlchemySystems", "ActiveCraftCompleteAt", "ALTER TABLE AlchemySystems ADD COLUMN ActiveCraftCompleteAt TEXT NULL;");
            EnsureSqliteColumnExists("AlchemySystems", "PendingResultJson", "ALTER TABLE AlchemySystems ADD COLUMN PendingResultJson TEXT NULL;");
            EnsureSqliteColumnExists("AlchemyProfessionLevelConfigs", "SeedKey", "ALTER TABLE AlchemyProfessionLevelConfigs ADD COLUMN SeedKey TEXT NULL;");
            EnsureSqliteColumnExists("AlchemyProfessionLevelConfigs", "IsBuiltIn", "ALTER TABLE AlchemyProfessionLevelConfigs ADD COLUMN IsBuiltIn INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("AlchemyProfessionLevelConfigs", "BuiltInVersion", "ALTER TABLE AlchemyProfessionLevelConfigs ADD COLUMN BuiltInVersion TEXT NULL;");
            EnsureSqliteColumnExists("AlchemyProfessionLevelConfigs", "LastUpdateTime", "ALTER TABLE AlchemyProfessionLevelConfigs ADD COLUMN LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00';");
            EnsureSqliteColumnExists("AlchemyProfessionRuleConfigs", "SeedKey", "ALTER TABLE AlchemyProfessionRuleConfigs ADD COLUMN SeedKey TEXT NULL;");
            EnsureSqliteColumnExists("AlchemyProfessionRuleConfigs", "IsBuiltIn", "ALTER TABLE AlchemyProfessionRuleConfigs ADD COLUMN IsBuiltIn INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("AlchemyProfessionRuleConfigs", "BuiltInVersion", "ALTER TABLE AlchemyProfessionRuleConfigs ADD COLUMN BuiltInVersion TEXT NULL;");
            EnsureSqliteColumnExists("AlchemyProfessionRuleConfigs", "LastUpdateTime", "ALTER TABLE AlchemyProfessionRuleConfigs ADD COLUMN LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00';");
            EnsureSqliteColumnExists("ForgeSystems", "ActiveRecipeId", "ALTER TABLE ForgeSystems ADD COLUMN ActiveRecipeId TEXT NULL;");
            EnsureSqliteColumnExists("ForgeSystems", "ActiveForgeStartedAt", "ALTER TABLE ForgeSystems ADD COLUMN ActiveForgeStartedAt TEXT NULL;");
            EnsureSqliteColumnExists("ForgeSystems", "ActiveForgeCompleteAt", "ALTER TABLE ForgeSystems ADD COLUMN ActiveForgeCompleteAt TEXT NULL;");
            EnsureSqliteColumnExists("ForgeSystems", "LearnedRecipesJson", "ALTER TABLE ForgeSystems ADD COLUMN LearnedRecipesJson TEXT NULL;");
            EnsureSqliteColumnExists("ForgeSystems", "PendingResultJson", "ALTER TABLE ForgeSystems ADD COLUMN PendingResultJson TEXT NULL;");
            EnsureSqliteColumnExists("ForgeProfessionLevelConfigs", "SeedKey", "ALTER TABLE ForgeProfessionLevelConfigs ADD COLUMN SeedKey TEXT NULL;");
            EnsureSqliteColumnExists("ForgeProfessionLevelConfigs", "IsBuiltIn", "ALTER TABLE ForgeProfessionLevelConfigs ADD COLUMN IsBuiltIn INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("ForgeProfessionLevelConfigs", "BuiltInVersion", "ALTER TABLE ForgeProfessionLevelConfigs ADD COLUMN BuiltInVersion TEXT NULL;");
            EnsureSqliteColumnExists("ForgeProfessionLevelConfigs", "LastUpdateTime", "ALTER TABLE ForgeProfessionLevelConfigs ADD COLUMN LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00';");
            EnsureSqliteColumnExists("ForgeProfessionRuleConfigs", "SeedKey", "ALTER TABLE ForgeProfessionRuleConfigs ADD COLUMN SeedKey TEXT NULL;");
            EnsureSqliteColumnExists("ForgeProfessionRuleConfigs", "IsBuiltIn", "ALTER TABLE ForgeProfessionRuleConfigs ADD COLUMN IsBuiltIn INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("ForgeProfessionRuleConfigs", "BuiltInVersion", "ALTER TABLE ForgeProfessionRuleConfigs ADD COLUMN BuiltInVersion TEXT NULL;");
            EnsureSqliteColumnExists("ForgeProfessionRuleConfigs", "LastUpdateTime", "ALTER TABLE ForgeProfessionRuleConfigs ADD COLUMN LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00';");
            EnsureSqliteColumnExists("quest_config", "ResetCycle", "ALTER TABLE quest_config ADD COLUMN ResetCycle INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("MonsterTemplates", "ElementPoolJson", "ALTER TABLE MonsterTemplates ADD COLUMN ElementPoolJson TEXT NULL;");
            EnsureSqliteColumnExists("MonsterTemplates", "CollectionDropsJson", "ALTER TABLE MonsterTemplates ADD COLUMN CollectionDropsJson TEXT NULL;");
            EnsureSqliteColumnExists("MonsterTemplates", "SeedKey", "ALTER TABLE MonsterTemplates ADD COLUMN SeedKey TEXT NULL;");
            EnsureSqliteColumnExists("MonsterTemplates", "IsBuiltIn", "ALTER TABLE MonsterTemplates ADD COLUMN IsBuiltIn INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("MonsterTemplates", "BuiltInVersion", "ALTER TABLE MonsterTemplates ADD COLUMN BuiltInVersion TEXT NULL;");
            EnsureSqliteColumnExists("MonsterTemplates", "LastUpdateTime", "ALTER TABLE MonsterTemplates ADD COLUMN LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00';");
            EnsureSqliteColumnExists("MapTemplates", "SpawnRulesJson", "ALTER TABLE MapTemplates ADD COLUMN SpawnRulesJson TEXT NULL;");
            TryDropSqliteColumn("MapTemplates", "MonsterTemplateIdsJson");
            EnsureSqliteColumnExists("PetTemplates", "InitialQualityMin", "ALTER TABLE PetTemplates ADD COLUMN InitialQualityMin INTEGER NOT NULL DEFAULT 1;");
            EnsureSqliteColumnExists("PetTemplates", "InitialQualityMax", "ALTER TABLE PetTemplates ADD COLUMN InitialQualityMax INTEGER NOT NULL DEFAULT 1;");
            EnsureSqliteColumnExists("PetTemplates", "GrowthRateMin", "ALTER TABLE PetTemplates ADD COLUMN GrowthRateMin REAL NOT NULL DEFAULT 1.0;");
            EnsureSqliteColumnExists("PetTemplates", "GrowthRateMax", "ALTER TABLE PetTemplates ADD COLUMN GrowthRateMax REAL NOT NULL DEFAULT 1.0;");
            EnsureSqliteColumnExists("PetTemplates", "InitialSkillCount", "ALTER TABLE PetTemplates ADD COLUMN InitialSkillCount INTEGER NOT NULL DEFAULT 1;");
            EnsureSqliteColumnExists("PetTemplates", "Element", "ALTER TABLE PetTemplates ADD COLUMN Element INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("PetTemplates", "MinType1", "ALTER TABLE PetTemplates ADD COLUMN MinType1 INTEGER NULL;");
            EnsureSqliteColumnExists("PetTemplates", "MaxType1", "ALTER TABLE PetTemplates ADD COLUMN MaxType1 INTEGER NULL;");
            EnsureSqliteColumnExists("PetTemplates", "MinType2", "ALTER TABLE PetTemplates ADD COLUMN MinType2 INTEGER NULL;");
            EnsureSqliteColumnExists("PetTemplates", "MaxType2", "ALTER TABLE PetTemplates ADD COLUMN MaxType2 INTEGER NULL;");
            EnsureSqliteColumnExists("PetTemplates", "MinType3", "ALTER TABLE PetTemplates ADD COLUMN MinType3 INTEGER NULL;");
            EnsureSqliteColumnExists("PetTemplates", "MaxType3", "ALTER TABLE PetTemplates ADD COLUMN MaxType3 INTEGER NULL;");
            EnsureSqliteColumnExists("PetTemplates", "MinType4", "ALTER TABLE PetTemplates ADD COLUMN MinType4 INTEGER NULL;");
            EnsureSqliteColumnExists("PetTemplates", "MaxType4", "ALTER TABLE PetTemplates ADD COLUMN MaxType4 INTEGER NULL;");
            EnsureSqliteColumnExists("PetTemplates", "MinType5", "ALTER TABLE PetTemplates ADD COLUMN MinType5 INTEGER NULL;");
            EnsureSqliteColumnExists("PetTemplates", "MaxType5", "ALTER TABLE PetTemplates ADD COLUMN MaxType5 INTEGER NULL;");
            EnsureSqliteColumnExists("PetTemplates", "MinType6", "ALTER TABLE PetTemplates ADD COLUMN MinType6 INTEGER NULL;");
            EnsureSqliteColumnExists("PetTemplates", "MaxType6", "ALTER TABLE PetTemplates ADD COLUMN MaxType6 INTEGER NULL;");
            EnsureSqliteColumnExists("PetTemplates", "MinType7", "ALTER TABLE PetTemplates ADD COLUMN MinType7 INTEGER NULL;");
            EnsureSqliteColumnExists("PetTemplates", "MaxType7", "ALTER TABLE PetTemplates ADD COLUMN MaxType7 INTEGER NULL;");
            EnsureSqliteColumnExists("PetTemplates", "MinType8", "ALTER TABLE PetTemplates ADD COLUMN MinType8 INTEGER NULL;");
            EnsureSqliteColumnExists("PetTemplates", "MaxType8", "ALTER TABLE PetTemplates ADD COLUMN MaxType8 INTEGER NULL;");
            EnsureSqliteColumnExists("PetTemplates", "MinType9", "ALTER TABLE PetTemplates ADD COLUMN MinType9 INTEGER NULL;");
            EnsureSqliteColumnExists("PetTemplates", "MaxType9", "ALTER TABLE PetTemplates ADD COLUMN MaxType9 INTEGER NULL;");
            EnsureSqliteColumnExists("PetTemplates", "MinType10", "ALTER TABLE PetTemplates ADD COLUMN MinType10 INTEGER NULL;");
            EnsureSqliteColumnExists("PetTemplates", "MaxType10", "ALTER TABLE PetTemplates ADD COLUMN MaxType10 INTEGER NULL;");
            EnsureSqliteColumnExists("PetTemplates", "MinType11", "ALTER TABLE PetTemplates ADD COLUMN MinType11 INTEGER NULL;");
            EnsureSqliteColumnExists("PetTemplates", "MaxType11", "ALTER TABLE PetTemplates ADD COLUMN MaxType11 INTEGER NULL;");
            EnsureSqliteColumnExists("PetTemplates", "MinType12", "ALTER TABLE PetTemplates ADD COLUMN MinType12 INTEGER NULL;");
            EnsureSqliteColumnExists("PetTemplates", "MaxType12", "ALTER TABLE PetTemplates ADD COLUMN MaxType12 INTEGER NULL;");
            EnsureSqliteColumnExists("PetTemplates", "MinType13", "ALTER TABLE PetTemplates ADD COLUMN MinType13 INTEGER NULL;");
            EnsureSqliteColumnExists("PetTemplates", "MaxType13", "ALTER TABLE PetTemplates ADD COLUMN MaxType13 INTEGER NULL;");
            EnsureSqliteColumnExists("PetTemplates", "MinType14", "ALTER TABLE PetTemplates ADD COLUMN MinType14 INTEGER NULL;");
            EnsureSqliteColumnExists("PetTemplates", "MaxType14", "ALTER TABLE PetTemplates ADD COLUMN MaxType14 INTEGER NULL;");
            EnsureSqliteColumnExists("PetTemplates", "MinType15", "ALTER TABLE PetTemplates ADD COLUMN MinType15 INTEGER NULL;");
            EnsureSqliteColumnExists("PetTemplates", "MaxType15", "ALTER TABLE PetTemplates ADD COLUMN MaxType15 INTEGER NULL;");
            EnsureSqliteColumnExists("PetTemplates", "SeedKey", "ALTER TABLE PetTemplates ADD COLUMN SeedKey TEXT NULL;");
            EnsureSqliteColumnExists("PetTemplates", "IsBuiltIn", "ALTER TABLE PetTemplates ADD COLUMN IsBuiltIn INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("PetTemplates", "BuiltInVersion", "ALTER TABLE PetTemplates ADD COLUMN BuiltInVersion TEXT NULL;");
            EnsureSqliteColumnExists("PetTemplates", "LastUpdateTime", "ALTER TABLE PetTemplates ADD COLUMN LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00';");
            EnsureSqliteColumnExists("DungeonTemplates", "SeedKey", "ALTER TABLE DungeonTemplates ADD COLUMN SeedKey TEXT NULL;");
            EnsureSqliteColumnExists("DungeonTemplates", "IsBuiltIn", "ALTER TABLE DungeonTemplates ADD COLUMN IsBuiltIn INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("DungeonTemplates", "BuiltInVersion", "ALTER TABLE DungeonTemplates ADD COLUMN BuiltInVersion TEXT NULL;");
            EnsureSqliteColumnExists("DungeonTemplates", "LastUpdateTime", "ALTER TABLE DungeonTemplates ADD COLUMN LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00';");
            EnsureSqliteColumnExists("CropTemplates", "SeedKey", "ALTER TABLE CropTemplates ADD COLUMN SeedKey TEXT NULL;");
            EnsureSqliteColumnExists("CropTemplates", "IsBuiltIn", "ALTER TABLE CropTemplates ADD COLUMN IsBuiltIn INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("CropTemplates", "BuiltInVersion", "ALTER TABLE CropTemplates ADD COLUMN BuiltInVersion TEXT NULL;");
            EnsureSqliteColumnExists("CropTemplates", "LastUpdateTime", "ALTER TABLE CropTemplates ADD COLUMN LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00';");
            EnsureSqliteColumnExists("SkillTemplates", "SeedKey", "ALTER TABLE SkillTemplates ADD COLUMN SeedKey TEXT NULL;");
            EnsureSqliteColumnExists("SkillTemplates", "AllowedProfessionsJson", "ALTER TABLE SkillTemplates ADD COLUMN AllowedProfessionsJson TEXT NULL;");
            EnsureSqliteColumnExists("SkillTemplates", "IsBuiltIn", "ALTER TABLE SkillTemplates ADD COLUMN IsBuiltIn INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("SkillTemplates", "BuiltInVersion", "ALTER TABLE SkillTemplates ADD COLUMN BuiltInVersion TEXT NULL;");
            EnsureSqliteColumnExists("SkillTemplates", "LastUpdateTime", "ALTER TABLE SkillTemplates ADD COLUMN LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00';");
            EnsureSqliteColumnExists("BuffTemplates", "SeedKey", "ALTER TABLE BuffTemplates ADD COLUMN SeedKey TEXT NULL;");
            EnsureSqliteColumnExists("BuffTemplates", "BuffCatalog", "ALTER TABLE BuffTemplates ADD COLUMN BuffCatalog TEXT NOT NULL DEFAULT 'legacy';");
            EnsureSqliteColumnExists("BuffTemplates", "IsBuiltIn", "ALTER TABLE BuffTemplates ADD COLUMN IsBuiltIn INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("BuffTemplates", "BuiltInVersion", "ALTER TABLE BuffTemplates ADD COLUMN BuiltInVersion TEXT NULL;");
            EnsureSqliteColumnExists("BuffTemplates", "LastUpdateTime", "ALTER TABLE BuffTemplates ADD COLUMN LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00';");
            EnsureSqliteColumnExists("AlchemyRecipes", "SeedKey", "ALTER TABLE AlchemyRecipes ADD COLUMN SeedKey TEXT NULL;");
            EnsureSqliteColumnExists("AlchemyRecipes", "IsBuiltIn", "ALTER TABLE AlchemyRecipes ADD COLUMN IsBuiltIn INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("AlchemyRecipes", "BuiltInVersion", "ALTER TABLE AlchemyRecipes ADD COLUMN BuiltInVersion TEXT NULL;");
            EnsureSqliteColumnExists("AlchemyRecipes", "LastUpdateTime", "ALTER TABLE AlchemyRecipes ADD COLUMN LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00';");
            EnsureSqliteColumnExists("ForgeRecipes", "SeedKey", "ALTER TABLE ForgeRecipes ADD COLUMN SeedKey TEXT NULL;");
            EnsureSqliteColumnExists("ForgeRecipes", "IsBuiltIn", "ALTER TABLE ForgeRecipes ADD COLUMN IsBuiltIn INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("ForgeRecipes", "BuiltInVersion", "ALTER TABLE ForgeRecipes ADD COLUMN BuiltInVersion TEXT NULL;");
            EnsureSqliteColumnExists("ForgeRecipes", "LastUpdateTime", "ALTER TABLE ForgeRecipes ADD COLUMN LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00';");
            EnsureSqliteColumnExists("PetInstances", "GrowthRate", "ALTER TABLE PetInstances ADD COLUMN GrowthRate REAL NOT NULL DEFAULT 1.0;");
            EnsureSqliteColumnExists("PetInstances", "Type1", "ALTER TABLE PetInstances ADD COLUMN Type1 INTEGER NOT NULL DEFAULT 100;");
            EnsureSqliteColumnExists("PetInstances", "Type2", "ALTER TABLE PetInstances ADD COLUMN Type2 INTEGER NOT NULL DEFAULT 50;");
            EnsureSqliteColumnExists("PetInstances", "Type3", "ALTER TABLE PetInstances ADD COLUMN Type3 INTEGER NOT NULL DEFAULT 10;");
            EnsureSqliteColumnExists("PetInstances", "Type4", "ALTER TABLE PetInstances ADD COLUMN Type4 INTEGER NOT NULL DEFAULT 5;");
            EnsureSqliteColumnExists("PetInstances", "Type5", "ALTER TABLE PetInstances ADD COLUMN Type5 INTEGER NOT NULL DEFAULT 5;");
            EnsureSqliteColumnExists("PetInstances", "Type6", "ALTER TABLE PetInstances ADD COLUMN Type6 INTEGER NOT NULL DEFAULT 3;");
            EnsureSqliteColumnExists("PetInstances", "Type7", "ALTER TABLE PetInstances ADD COLUMN Type7 INTEGER NOT NULL DEFAULT 10;");
            EnsureSqliteColumnExists("PetInstances", "Type8", "ALTER TABLE PetInstances ADD COLUMN Type8 REAL NOT NULL DEFAULT 0.9;");
            EnsureSqliteColumnExists("PetInstances", "Type9", "ALTER TABLE PetInstances ADD COLUMN Type9 REAL NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("PetInstances", "Type10", "ALTER TABLE PetInstances ADD COLUMN Type10 REAL NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("PetInstances", "Type11", "ALTER TABLE PetInstances ADD COLUMN Type11 REAL NOT NULL DEFAULT 1.5;");
            EnsureSqliteColumnExists("PetInstances", "Type12", "ALTER TABLE PetInstances ADD COLUMN Type12 REAL NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("PetInstances", "Type13", "ALTER TABLE PetInstances ADD COLUMN Type13 REAL NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("PetInstances", "Type14", "ALTER TABLE PetInstances ADD COLUMN Type14 REAL NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("PetInstances", "Type15", "ALTER TABLE PetInstances ADD COLUMN Type15 REAL NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("PetInstances", "Element", "ALTER TABLE PetInstances ADD COLUMN Element INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("AdminAuditLogs", "ResponseJson", "ALTER TABLE AdminAuditLogs ADD COLUMN ResponseJson TEXT NULL;");
            EnsureSqliteColumnExists("AdminAuditLogs", "BeforeJson", "ALTER TABLE AdminAuditLogs ADD COLUMN BeforeJson TEXT NULL;");
            EnsureSqliteColumnExists("AdminAuditLogs", "AfterJson", "ALTER TABLE AdminAuditLogs ADD COLUMN AfterJson TEXT NULL;");
            EnsureSqliteColumnExists("AdminAuditLogs", "DiffJson", "ALTER TABLE AdminAuditLogs ADD COLUMN DiffJson TEXT NULL;");
            EnsureSqliteColumnExists("achievement_progress", "RequirementProgressJson", "ALTER TABLE achievement_progress ADD COLUMN RequirementProgressJson TEXT NULL;");
            EnsureSqliteColumnExists("achievement_config", "RewardItemsJson", "ALTER TABLE achievement_config ADD COLUMN RewardItemsJson TEXT NULL;");
            EnsureSqliteColumnExists("achievement_config", "RequirementsJson", "ALTER TABLE achievement_config ADD COLUMN RequirementsJson TEXT NULL;");
            EnsureSqliteColumnExists("achievement_config", "SeedKey", "ALTER TABLE achievement_config ADD COLUMN SeedKey TEXT NULL;");
            EnsureSqliteColumnExists("achievement_config", "IsBuiltIn", "ALTER TABLE achievement_config ADD COLUMN IsBuiltIn INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("achievement_config", "BuiltInVersion", "ALTER TABLE achievement_config ADD COLUMN BuiltInVersion TEXT NULL;");
            EnsureSqliteColumnExists("achievement_config", "LastUpdateTime", "ALTER TABLE achievement_config ADD COLUMN LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00';");
            EnsureSqliteColumnExists("quest_config", "SeedKey", "ALTER TABLE quest_config ADD COLUMN SeedKey TEXT NULL;");
            EnsureSqliteColumnExists("quest_config", "IsBuiltIn", "ALTER TABLE quest_config ADD COLUMN IsBuiltIn INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("quest_config", "BuiltInVersion", "ALTER TABLE quest_config ADD COLUMN BuiltInVersion TEXT NULL;");
            EnsureSqliteColumnExists("quest_config", "LastUpdateTime", "ALTER TABLE quest_config ADD COLUMN LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00';");
            EnsureSqliteColumnExists("PlayerLevelConfigs", "SeedKey", "ALTER TABLE PlayerLevelConfigs ADD COLUMN SeedKey TEXT NULL;");
            EnsureSqliteColumnExists("PlayerLevelConfigs", "IsBuiltIn", "ALTER TABLE PlayerLevelConfigs ADD COLUMN IsBuiltIn INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("PlayerLevelConfigs", "BuiltInVersion", "ALTER TABLE PlayerLevelConfigs ADD COLUMN BuiltInVersion TEXT NULL;");
            EnsureSqliteColumnExists("PlayerLevelConfigs", "LastUpdateTime", "ALTER TABLE PlayerLevelConfigs ADD COLUMN LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00';");
            EnsureSqliteColumnExists("ItemTemplates", "SeedKey", "ALTER TABLE ItemTemplates ADD COLUMN SeedKey TEXT NULL;");
            EnsureSqliteColumnExists("ItemTemplates", "IsBuiltIn", "ALTER TABLE ItemTemplates ADD COLUMN IsBuiltIn INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("ItemTemplates", "BuiltInVersion", "ALTER TABLE ItemTemplates ADD COLUMN BuiltInVersion TEXT NULL;");
            EnsureSqliteColumnExists("ItemTemplates", "LastUpdateTime", "ALTER TABLE ItemTemplates ADD COLUMN LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00';");
            EnsureSqliteColumnExists("RealmLevelConfigs", "RequiredExp", "ALTER TABLE RealmLevelConfigs ADD COLUMN RequiredExp INTEGER NOT NULL DEFAULT 100;");
            EnsureSqliteColumnExists("RealmLevelConfigs", "SeedKey", "ALTER TABLE RealmLevelConfigs ADD COLUMN SeedKey TEXT NULL;");
            EnsureSqliteColumnExists("RealmLevelConfigs", "IsBuiltIn", "ALTER TABLE RealmLevelConfigs ADD COLUMN IsBuiltIn INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("RealmLevelConfigs", "BuiltInVersion", "ALTER TABLE RealmLevelConfigs ADD COLUMN BuiltInVersion TEXT NULL;");
            EnsureSqliteColumnExists("RealmLevelConfigs", "LastUpdateTime", "ALTER TABLE RealmLevelConfigs ADD COLUMN LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00';");
            EnsureSqliteColumnExists("shop_config", "SeedKey", "ALTER TABLE shop_config ADD COLUMN SeedKey TEXT NULL;");
            EnsureSqliteColumnExists("shop_config", "IsBuiltIn", "ALTER TABLE shop_config ADD COLUMN IsBuiltIn INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("shop_config", "BuiltInVersion", "ALTER TABLE shop_config ADD COLUMN BuiltInVersion TEXT NULL;");
            EnsureSqliteColumnExists("shop_config", "LastUpdateTime", "ALTER TABLE shop_config ADD COLUMN LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00';");
            EnsureSqliteColumnExists("shop_item", "SeedKey", "ALTER TABLE shop_item ADD COLUMN SeedKey TEXT NULL;");
            EnsureSqliteColumnExists("shop_item", "IsBuiltIn", "ALTER TABLE shop_item ADD COLUMN IsBuiltIn INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("shop_item", "BuiltInVersion", "ALTER TABLE shop_item ADD COLUMN BuiltInVersion TEXT NULL;");
            EnsureSqliteColumnExists("shop_item", "LastUpdateTime", "ALTER TABLE shop_item ADD COLUMN LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00';");
            EnsureSqliteColumnExists("ranking_config", "SeedKey", "ALTER TABLE ranking_config ADD COLUMN SeedKey TEXT NULL;");
            EnsureSqliteColumnExists("ranking_config", "IsBuiltIn", "ALTER TABLE ranking_config ADD COLUMN IsBuiltIn INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("ranking_config", "BuiltInVersion", "ALTER TABLE ranking_config ADD COLUMN BuiltInVersion TEXT NULL;");
            EnsureSqliteColumnExists("ranking_config", "LastUpdateTime", "ALTER TABLE ranking_config ADD COLUMN LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00';");
            EnsureSqliteColumnExists("ranking_reward", "SeedKey", "ALTER TABLE ranking_reward ADD COLUMN SeedKey TEXT NULL;");
            EnsureSqliteColumnExists("ranking_reward", "IsBuiltIn", "ALTER TABLE ranking_reward ADD COLUMN IsBuiltIn INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("ranking_reward", "BuiltInVersion", "ALTER TABLE ranking_reward ADD COLUMN BuiltInVersion TEXT NULL;");
            EnsureSqliteColumnExists("ranking_reward", "LastUpdateTime", "ALTER TABLE ranking_reward ADD COLUMN LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00';");
            EnsureSqliteColumnExists("AttributePointConfigs", "SeedKey", "ALTER TABLE AttributePointConfigs ADD COLUMN SeedKey TEXT NULL;");
            EnsureSqliteColumnExists("AttributePointConfigs", "Profession", "ALTER TABLE AttributePointConfigs ADD COLUMN Profession TEXT NOT NULL DEFAULT 'warrior';");
            EnsureSqliteColumnExists("AttributePointConfigs", "BonusPerPoint", "ALTER TABLE AttributePointConfigs ADD COLUMN BonusPerPoint INTEGER NOT NULL DEFAULT 1;");
            EnsureSqliteColumnExists("AttributePointConfigs", "PointsPerBonus", "ALTER TABLE AttributePointConfigs ADD COLUMN PointsPerBonus INTEGER NOT NULL DEFAULT 1;");
            EnsureSqliteColumnExists("AttributePointConfigs", "IsBuiltIn", "ALTER TABLE AttributePointConfigs ADD COLUMN IsBuiltIn INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("AttributePointConfigs", "BuiltInVersion", "ALTER TABLE AttributePointConfigs ADD COLUMN BuiltInVersion TEXT NULL;");
            EnsureSqliteColumnExists("AttributePointConfigs", "LastUpdateTime", "ALTER TABLE AttributePointConfigs ADD COLUMN LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00';");
            EnsureSqliteColumnExists("CheckInRewardConfigs", "SeedKey", "ALTER TABLE CheckInRewardConfigs ADD COLUMN SeedKey TEXT NULL;");
            EnsureSqliteColumnExists("CheckInRewardConfigs", "IsBuiltIn", "ALTER TABLE CheckInRewardConfigs ADD COLUMN IsBuiltIn INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("CheckInRewardConfigs", "BuiltInVersion", "ALTER TABLE CheckInRewardConfigs ADD COLUMN BuiltInVersion TEXT NULL;");
            EnsureSqliteColumnExists("CheckInRewardConfigs", "LastUpdateTime", "ALTER TABLE CheckInRewardConfigs ADD COLUMN LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00';");
            EnsureSqliteColumnExists("RedeemCodeConfigs", "SeedKey", "ALTER TABLE RedeemCodeConfigs ADD COLUMN SeedKey TEXT NULL;");
            EnsureSqliteColumnExists("RedeemCodeConfigs", "IsBuiltIn", "ALTER TABLE RedeemCodeConfigs ADD COLUMN IsBuiltIn INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("RedeemCodeConfigs", "BuiltInVersion", "ALTER TABLE RedeemCodeConfigs ADD COLUMN BuiltInVersion TEXT NULL;");
            EnsureSqliteColumnExists("RedeemCodeConfigs", "LastUpdateTime", "ALTER TABLE RedeemCodeConfigs ADD COLUMN LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00';");
            EnsureSqliteColumnExists("BattleElementRelationConfigs", "SeedKey", "ALTER TABLE BattleElementRelationConfigs ADD COLUMN SeedKey TEXT NULL;");
            EnsureSqliteColumnExists("BattleElementRelationConfigs", "IsBuiltIn", "ALTER TABLE BattleElementRelationConfigs ADD COLUMN IsBuiltIn INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("BattleElementRelationConfigs", "BuiltInVersion", "ALTER TABLE BattleElementRelationConfigs ADD COLUMN BuiltInVersion TEXT NULL;");
            EnsureSqliteColumnExists("BattleElementRelationConfigs", "LastUpdateTime", "ALTER TABLE BattleElementRelationConfigs ADD COLUMN LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00';");
            EnsureSqliteColumnExists("SpiritFieldSystemConfigs", "SeedKey", "ALTER TABLE SpiritFieldSystemConfigs ADD COLUMN SeedKey TEXT NULL;");
            EnsureSqliteColumnExists("SpiritFieldSystemConfigs", "IsBuiltIn", "ALTER TABLE SpiritFieldSystemConfigs ADD COLUMN IsBuiltIn INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("SpiritFieldSystemConfigs", "BuiltInVersion", "ALTER TABLE SpiritFieldSystemConfigs ADD COLUMN BuiltInVersion TEXT NULL;");
            EnsureSqliteColumnExists("SpiritFieldSystemConfigs", "LastUpdateTime", "ALTER TABLE SpiritFieldSystemConfigs ADD COLUMN LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00';");
            EnsureSqliteColumnExists("SpiritFieldSpeedUpItemConfigs", "SeedKey", "ALTER TABLE SpiritFieldSpeedUpItemConfigs ADD COLUMN SeedKey TEXT NULL;");
            EnsureSqliteColumnExists("SpiritFieldSpeedUpItemConfigs", "IsBuiltIn", "ALTER TABLE SpiritFieldSpeedUpItemConfigs ADD COLUMN IsBuiltIn INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("SpiritFieldSpeedUpItemConfigs", "BuiltInVersion", "ALTER TABLE SpiritFieldSpeedUpItemConfigs ADD COLUMN BuiltInVersion TEXT NULL;");
            EnsureSqliteColumnExists("SpiritFieldSpeedUpItemConfigs", "LastUpdateTime", "ALTER TABLE SpiritFieldSpeedUpItemConfigs ADD COLUMN LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00';");
            EnsureSqliteColumnExists("PlayerInitialResourceConfigs", "StartLevel", "ALTER TABLE PlayerInitialResourceConfigs ADD COLUMN StartLevel INTEGER NOT NULL DEFAULT 1;");
            EnsureSqliteColumnExists("PlayerInitialResourceConfigs", "StartExp", "ALTER TABLE PlayerInitialResourceConfigs ADD COLUMN StartExp INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("guild", "SectTemplateId", "ALTER TABLE guild ADD COLUMN SectTemplateId TEXT NULL;");
            EnsureSqliteColumnExists("guild", "TotalDonation", "ALTER TABLE guild ADD COLUMN TotalDonation INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("ItemPillConfigs", "DurationMinutes", "ALTER TABLE ItemPillConfigs ADD COLUMN DurationMinutes INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("ItemPillConfigs", "MaxUsageCount", "ALTER TABLE ItemPillConfigs ADD COLUMN MaxUsageCount INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("ItemPillConfigs", "HealHpPercent", "ALTER TABLE ItemPillConfigs ADD COLUMN HealHpPercent INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("ItemPillConfigs", "HealMpPercent", "ALTER TABLE ItemPillConfigs ADD COLUMN HealMpPercent INTEGER NOT NULL DEFAULT 0;");
            EnsureSqliteColumnExists("GemTemplates", "SynthSuccessRate", "ALTER TABLE GemTemplates ADD COLUMN SynthSuccessRate INTEGER NOT NULL DEFAULT 100;");
            EnsureSqliteColumnExists("PlayerFavorability", "TodayPartyBattleJson", "ALTER TABLE PlayerFavorability ADD COLUMN TodayPartyBattleJson TEXT NULL;");
            EnsureSqliteColumnExists("FavorabilityLevelConfig", "RewardJson", "ALTER TABLE FavorabilityLevelConfig ADD COLUMN RewardJson TEXT NULL;");
            BackfillEquipmentSplitStats();
            NormalizeInventoryStacks();
            TryDropSqliteColumn("EquipmentInstances", "BaseAttack");
            TryDropSqliteColumn("EquipmentInstances", "BaseDefense");
            TryDropSqliteColumn("EquipmentTemplates", "GearArchetype");
            TryDropSqliteColumn("EquipmentRerollSlotPoolConfigs", "CombatStyle");
            TryDropSqliteColumn("EquipmentRerollSlotPoolConfigs", "GearArchetype");
            BackfillPetTemplateGenerationColumns();
            BackfillPetInstanceCombatColumns();
            TryDropSqliteColumn("PetTemplates", "InitialQuality");
            TryDropSqliteColumn("PetTemplates", "BaseAttack");
            TryDropSqliteColumn("PetTemplates", "BaseDefense");
            TryDropSqliteColumn("PetTemplates", "BaseHP");
            TryDropSqliteColumn("PetTemplates", "BaseMP");
            TryDropSqliteColumn("PetTemplates", "BaseSpeed");
            TryDropSqliteColumn("PetTemplates", "GrowthRate");
            TryDropSqliteColumn("PetInstances", "Attack");
            TryDropSqliteColumn("PetInstances", "Defense");
            TryDropSqliteColumn("PetInstances", "HP");
            TryDropSqliteColumn("PetInstances", "MP");
            TryDropSqliteColumn("PetInstances", "Speed");

            // 竞技场禁赛字段
            EnsureSqliteColumnExists("ArenaPlayers", "BannedUntil", "ALTER TABLE ArenaPlayers ADD COLUMN BannedUntil TEXT NULL;");
            EnsureSqliteColumnExists("ArenaPlayers", "BanReason", "ALTER TABLE ArenaPlayers ADD COLUMN BanReason TEXT NULL;");
        }

        private void BackfillEquipmentSplitStats()
        {
            if (!SqliteColumnExists("EquipmentInstances", "BaseAttack") || !SqliteColumnExists("EquipmentInstances", "BaseDefense"))
            {
                return;
            }

            _db.Ado.ExecuteCommand(
                """
                UPDATE EquipmentInstances
                SET
                    BasePhysicalAttack = CASE
                        WHEN BasePhysicalAttack = 0 AND BaseMagicAttack = 0 AND BaseAttack > 0 AND EXISTS (
                            SELECT 1
                            FROM EquipmentTemplates t
                            WHERE CAST(t.EquipmentId AS TEXT) = EquipmentInstances.TemplateId
                              AND t.CombatStyle <> 2
                        ) THEN BaseAttack
                        ELSE BasePhysicalAttack
                    END,
                    BaseMagicAttack = CASE
                        WHEN BasePhysicalAttack = 0 AND BaseMagicAttack = 0 AND BaseAttack > 0 AND EXISTS (
                            SELECT 1
                            FROM EquipmentTemplates t
                            WHERE CAST(t.EquipmentId AS TEXT) = EquipmentInstances.TemplateId
                              AND t.CombatStyle = 2
                        ) THEN BaseAttack
                        ELSE BaseMagicAttack
                    END,
                    BasePhysicalDefense = CASE
                        WHEN BasePhysicalDefense = 0 AND BaseMagicDefense = 0 AND BaseDefense > 0 AND EXISTS (
                            SELECT 1
                            FROM EquipmentTemplates t
                            WHERE CAST(t.EquipmentId AS TEXT) = EquipmentInstances.TemplateId
                              AND t.GearArchetype <> 2
                        ) THEN BaseDefense
                        ELSE BasePhysicalDefense
                    END,
                    BaseMagicDefense = CASE
                        WHEN BasePhysicalDefense = 0 AND BaseMagicDefense = 0 AND BaseDefense > 0 AND EXISTS (
                            SELECT 1
                            FROM EquipmentTemplates t
                            WHERE CAST(t.EquipmentId AS TEXT) = EquipmentInstances.TemplateId
                              AND t.GearArchetype = 2
                        ) THEN BaseDefense
                        ELSE BaseMagicDefense
                    END
                WHERE BaseAttack > 0 OR BaseDefense > 0;
                """);
        }

        /// <summary>
        /// 中文注释：
        /// 旧版灵宠模板是“固定一只宠物”的数据结构，只有 BaseAttack / BaseHP 这类固定值。
        /// 新版改成“模板定义生成范围”，所以这里把旧列一次性回填到 MinType/MaxType、品质范围和成长范围，
        /// 避免老库在切换新逻辑后生成出全默认值的宠物。
        /// </summary>
        private void BackfillPetTemplateGenerationColumns()
        {
            if (!SqliteColumnExists("PetTemplates", "BaseAttack") || !SqliteColumnExists("PetTemplates", "InitialQuality"))
            {
                return;
            }

            _db.Ado.ExecuteCommand(
                """
                UPDATE PetTemplates
                SET
                    InitialQualityMin = CASE WHEN InitialQualityMin <= 0 THEN InitialQuality ELSE InitialQualityMin END,
                    InitialQualityMax = CASE WHEN InitialQualityMax <= 0 THEN InitialQuality ELSE InitialQualityMax END,
                    GrowthRateMin = CASE WHEN GrowthRateMin <= 0 THEN GrowthRate ELSE GrowthRateMin END,
                    GrowthRateMax = CASE WHEN GrowthRateMax <= 0 THEN GrowthRate ELSE GrowthRateMax END,
                    InitialSkillCount = CASE
                        WHEN InitialSkillCount <= 0 AND SkillIdsJson IS NOT NULL AND SkillIdsJson <> '' AND SkillIdsJson <> '[]' THEN 1
                        WHEN InitialSkillCount <= 0 THEN 0
                        ELSE InitialSkillCount
                    END,
                    MinType1 = CASE WHEN MinType1 IS NULL THEN BaseHP ELSE MinType1 END,
                    MaxType1 = CASE WHEN MaxType1 IS NULL THEN BaseHP ELSE MaxType1 END,
                    MinType2 = CASE WHEN MinType2 IS NULL THEN BaseMP ELSE MinType2 END,
                    MaxType2 = CASE WHEN MaxType2 IS NULL THEN BaseMP ELSE MaxType2 END,
                    MinType3 = CASE WHEN MinType3 IS NULL THEN BaseAttack ELSE MinType3 END,
                    MaxType3 = CASE WHEN MaxType3 IS NULL THEN BaseAttack ELSE MaxType3 END,
                    MinType4 = CASE WHEN MinType4 IS NULL THEN MAX(1, CAST(BaseAttack * 0.6 AS INTEGER)) ELSE MinType4 END,
                    MaxType4 = CASE WHEN MaxType4 IS NULL THEN MAX(1, CAST(BaseAttack * 0.75 AS INTEGER)) ELSE MaxType4 END,
                    MinType5 = CASE WHEN MinType5 IS NULL THEN BaseDefense ELSE MinType5 END,
                    MaxType5 = CASE WHEN MaxType5 IS NULL THEN BaseDefense ELSE MaxType5 END,
                    MinType6 = CASE WHEN MinType6 IS NULL THEN MAX(1, CAST(BaseDefense * 0.6 AS INTEGER)) ELSE MinType6 END,
                    MaxType6 = CASE WHEN MaxType6 IS NULL THEN MAX(1, CAST(BaseDefense * 0.75 AS INTEGER)) ELSE MaxType6 END,
                    MinType7 = CASE WHEN MinType7 IS NULL THEN BaseSpeed ELSE MinType7 END,
                    MaxType7 = CASE WHEN MaxType7 IS NULL THEN BaseSpeed ELSE MaxType7 END,
                    MinType8 = CASE WHEN MinType8 IS NULL THEN 90 ELSE MinType8 END,
                    MaxType8 = CASE WHEN MaxType8 IS NULL THEN 90 ELSE MaxType8 END,
                    MinType9 = CASE WHEN MinType9 IS NULL THEN 2 ELSE MinType9 END,
                    MaxType9 = CASE WHEN MaxType9 IS NULL THEN 4 ELSE MaxType9 END,
                    MinType10 = CASE WHEN MinType10 IS NULL THEN 2 ELSE MinType10 END,
                    MaxType10 = CASE WHEN MaxType10 IS NULL THEN 5 ELSE MaxType10 END,
                    MinType11 = CASE WHEN MinType11 IS NULL THEN 150 ELSE MinType11 END,
                    MaxType11 = CASE WHEN MaxType11 IS NULL THEN 150 ELSE MaxType11 END,
                    MinType12 = CASE WHEN MinType12 IS NULL THEN 0 ELSE MinType12 END,
                    MaxType12 = CASE WHEN MaxType12 IS NULL THEN 2 ELSE MaxType12 END,
                    MinType13 = CASE WHEN MinType13 IS NULL THEN 0 ELSE MinType13 END,
                    MaxType13 = CASE WHEN MaxType13 IS NULL THEN 2 ELSE MaxType13 END,
                    MinType14 = CASE WHEN MinType14 IS NULL THEN 0 ELSE MinType14 END,
                    MaxType14 = CASE WHEN MaxType14 IS NULL THEN 2 ELSE MaxType14 END,
                    MinType15 = CASE WHEN MinType15 IS NULL THEN 0 ELSE MinType15 END,
                    MaxType15 = CASE WHEN MaxType15 IS NULL THEN 3 ELSE MaxType15 END
                WHERE
                    InitialQualityMin <= 0 OR InitialQualityMax <= 0 OR
                    GrowthRateMin <= 0 OR GrowthRateMax <= 0 OR
                    MinType1 IS NULL OR MaxType1 IS NULL OR
                    MinType3 IS NULL OR MaxType3 IS NULL;
                """);
        }

        /// <summary>
        /// 中文注释：
        /// 旧版灵宠实例只保存 HP / Attack / Defense / Speed 这组固定字段。
        /// 新版战斗和成长统一读取 Type1-Type15，所以这里把老实例一次性回填到新列，避免出战时读到全 0。
        /// </summary>
        private void BackfillPetInstanceCombatColumns()
        {
            if (!SqliteColumnExists("PetInstances", "Attack"))
            {
                return;
            }

            _db.Ado.ExecuteCommand(
                """
                UPDATE PetInstances
                SET
                    GrowthRate = CASE WHEN GrowthRate <= 0 THEN 1.0 ELSE GrowthRate END,
                    Type1 = CASE WHEN Type1 <= 0 THEN HP ELSE Type1 END,
                    Type2 = CASE WHEN Type2 <= 0 THEN MP ELSE Type2 END,
                    Type3 = CASE WHEN Type3 <= 0 THEN Attack ELSE Type3 END,
                    Type4 = CASE WHEN Type4 <= 0 THEN MAX(1, CAST(Attack * 0.5 AS INTEGER)) ELSE Type4 END,
                    Type5 = CASE WHEN Type5 <= 0 THEN Defense ELSE Type5 END,
                    Type6 = CASE WHEN Type6 <= 0 THEN MAX(1, CAST(Defense * 0.5 AS INTEGER)) ELSE Type6 END,
                    Type7 = CASE WHEN Type7 <= 0 THEN Speed ELSE Type7 END,
                    Type8 = CASE WHEN Type8 <= 0 THEN 0.9 ELSE Type8 END,
                    Type9 = CASE WHEN Type9 < 0 THEN 0 ELSE Type9 END,
                    Type10 = CASE WHEN Type10 < 0 THEN 0 ELSE Type10 END,
                    Type11 = CASE WHEN Type11 <= 0 THEN 1.5 ELSE Type11 END,
                    Type12 = CASE WHEN Type12 < 0 THEN 0 ELSE Type12 END,
                    Type13 = CASE WHEN Type13 < 0 THEN 0 ELSE Type13 END,
                    Type14 = CASE WHEN Type14 < 0 THEN 0 ELSE Type14 END,
                    Type15 = CASE WHEN Type15 < 0 THEN 0 ELSE Type15 END,
                    Element = CASE WHEN Element IS NULL THEN 0 ELSE Element END
                WHERE
                    Type1 <= 0 OR Type2 <= 0 OR Type3 <= 0 OR Type5 <= 0 OR Type7 <= 0 OR
                    Type8 <= 0 OR Type11 <= 0;
                """);
        }

        private void TryDropSqliteColumn(string tableName, string columnName)
        {
            if (!SqliteColumnExists(tableName, columnName))
            {
                return;
            }

            try
            {
                _db.Ado.ExecuteCommand($"ALTER TABLE {tableName} DROP COLUMN {columnName};");
                _logger?.LogInformation("SQLite table {TableName} dropped legacy column {ColumnName}", tableName, columnName);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "SQLite table {TableName} failed to drop legacy column {ColumnName}; compatibility column will be kept.", tableName, columnName);
            }
        }

        private void MigrateSlotPoolConfigsIfNeeded()
        {
            // 如果旧表存在 Weight 列（旧 schema），需要重建为新 schema
            if (!SqliteColumnExists("EquipmentRerollSlotPoolConfigs", "Weight"))
            {
                return;
            }

            _logger?.LogInformation("检测到旧版 EquipmentRerollSlotPoolConfigs 表结构，正在迁移...");

            // 备份旧数据中仍然有效的列
            _db.Ado.ExecuteCommand(@"
                CREATE TABLE IF NOT EXISTS EquipmentRerollSlotPoolConfigs_backup AS
                SELECT GID, Slot, AttributeType, 1 AS Tier,
                       MaxDuplicateCount, SortOrder, IsEnabled, SeedKey, IsBuiltIn, BuiltInVersion, LastUpdateTime
                FROM EquipmentRerollSlotPoolConfigs;
            ");
            _db.Ado.ExecuteCommand("DROP TABLE EquipmentRerollSlotPoolConfigs;");
            _db.Ado.ExecuteCommand(@"
                CREATE TABLE EquipmentRerollSlotPoolConfigs (
                    GID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Slot INTEGER NOT NULL,
                    AttributeType INTEGER NOT NULL,
                    Tier INTEGER NOT NULL DEFAULT 1,
                    MaxDuplicateCount INTEGER NOT NULL DEFAULT 1,
                    SortOrder INTEGER NOT NULL DEFAULT 0,
                    IsEnabled INTEGER NOT NULL DEFAULT 1,
                    SeedKey TEXT NULL,
                    IsBuiltIn INTEGER NOT NULL DEFAULT 0,
                    BuiltInVersion TEXT NULL,
                    LastUpdateTime TEXT NOT NULL DEFAULT '2000-01-01 00:00:00'
                );
            ");
            _db.Ado.ExecuteCommand(@"
                INSERT INTO EquipmentRerollSlotPoolConfigs
                SELECT * FROM EquipmentRerollSlotPoolConfigs_backup;
            ");
            _db.Ado.ExecuteCommand("DROP TABLE EquipmentRerollSlotPoolConfigs_backup;");

            _logger?.LogInformation("EquipmentRerollSlotPoolConfigs 表迁移完成");
        }

        private void NormalizeInventoryStacks()
        {
            _db.Ado.ExecuteCommand("""
                DROP TABLE IF EXISTS temp.InventoryItemsStackMigration;
                CREATE TEMP TABLE InventoryItemsStackMigration AS
                SELECT
                    MIN(Id) AS KeeperId,
                    PlayerId,
                    ItemId,
                    SUM(Quantity) AS Quantity,
                    MAX(IsBound) AS IsLocked,
                    MIN(AcquiredTime) AS AcquiredTime,
                    MAX(ExpireTime) AS ExpireTime
                FROM InventoryItems
                GROUP BY PlayerId, ItemId;

                UPDATE InventoryItems
                SET
                    Quantity = (
                        SELECT Quantity
                        FROM InventoryItemsStackMigration m
                        WHERE m.KeeperId = InventoryItems.Id
                    ),
                    IsBound = (
                        SELECT IsLocked
                        FROM InventoryItemsStackMigration m
                        WHERE m.KeeperId = InventoryItems.Id
                    ),
                    AcquiredTime = (
                        SELECT AcquiredTime
                        FROM InventoryItemsStackMigration m
                        WHERE m.KeeperId = InventoryItems.Id
                    ),
                    ExpireTime = (
                        SELECT ExpireTime
                        FROM InventoryItemsStackMigration m
                        WHERE m.KeeperId = InventoryItems.Id
                    )
                WHERE Id IN (SELECT KeeperId FROM InventoryItemsStackMigration);

                DELETE FROM InventoryItems
                WHERE Id NOT IN (SELECT KeeperId FROM InventoryItemsStackMigration);

                DROP TABLE temp.InventoryItemsStackMigration;
                CREATE UNIQUE INDEX IF NOT EXISTS idx_inventory_player_item
                    ON InventoryItems(PlayerId, ItemId);
                """);
        }
        private void EnsureSqliteColumnExists(string tableName, string columnName, string alterSql)
        {
            if (SqliteColumnExists(tableName, columnName))
            {
                return;
            }

            _db.Ado.ExecuteCommand(alterSql);
            _logger?.LogInformation("SQLite 表 {TableName} 已补齐列 {ColumnName}", tableName, columnName);
        }

        private bool SqliteColumnExists(string tableName, string columnName)
        {
            var safeTableName = tableName.Replace("'", "''");
            var safeColumnName = columnName.Replace("'", "''");
            var columnRows = _db.Ado.GetDataTable(
                $"SELECT name FROM pragma_table_info('{safeTableName}') WHERE name = '{safeColumnName}';");

            return columnRows.Rows.Count > 0;
        }
    }
}

