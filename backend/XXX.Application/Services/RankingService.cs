using Microsoft.Extensions.Logging;
using SqlSugar;
using System.Collections.Concurrent;
using System.Text.Json;
using XXX.Application.Interfaces;
using XXX.Entity;
using CacheService = XXX.Infrastructure.Cache.ICacheService;
using XXX.Infrastructure.Data;
using XXX.Ranking;

namespace XXX.Application.Services
{
    /// <summary>
    /// 排行榜服务实现类
    /// 提供排行榜配置管理、排名查询、分数更新、赛季管理等排行榜系统功能的实现
    /// </summary>
    /// <remarks>
    /// 主要职责：
    /// - 加载和管理排行榜配置
    /// - 处理分数更新和排名计算
    /// - 管理赛季周期
    /// - 创建历史快照
    /// </remarks>
    public class RankingService : IRankingService
    {
        /// <summary>
        /// 数据库客户端，用于数据持久化操作
        /// </summary>
        private readonly ISqlSugarClient _db;

        /// <summary>
        /// 日志记录器
        /// </summary>
        private readonly ILogger<RankingService> _logger;

        /// <summary>
        /// 排行榜结果缓存服务。
        /// </summary>
        private readonly CacheService _cacheService;

        /// <summary>
        /// 每个排行榜独立的构建锁，避免同一榜单缓存同时失效时重复查库。
        /// </summary>
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> RankingBuildLocks = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 排行榜结果缓存的固定版本号，调整结果结构或规则时递增。
        /// </summary>
        private const string RankingCacheVersion = "v1";

        /// <summary>
        /// 排行榜结果缓存的绝对过期时间（分钟）。
        /// </summary>
        private const int RankingCacheExpirationMinutes = 10;

        /// <summary>
        /// 排行榜配置缓存，键为排行榜ID
        /// </summary>
        private static readonly Dictionary<string, RankingConfig> SharedRankingConfigs = [];
        private static readonly System.Threading.SemaphoreSlim InitializationLock = new(1, 1);
        private readonly Dictionary<string, RankingConfig> _rankingConfigs = SharedRankingConfigs;

        private static bool _isInitialized;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dbContext">数据库上下文</param>
        /// <param name="cacheService">排行榜结果缓存服务。</param>
        /// <param name="logger">日志记录器</param>
        public RankingService(DbContext dbContext, CacheService cacheService, ILogger<RankingService> logger)
        {
            _db = dbContext.Db;
            _cacheService = cacheService;
            _logger = logger;
        }

        /// <summary>
        /// 初始化排行榜配置缓存。
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_isInitialized && _rankingConfigs.Count > 0)
            {
                return;
            }

            await ReloadCacheAsync();
        }

        /// <summary>
        /// 重新从数据库装载排行榜配置缓存。
        /// </summary>
        public async Task ReloadCacheAsync()
        {
            await InitializationLock.WaitAsync();
            try
            {
                var totalConfigCount = await _db.Queryable<RankingConfigEntity>().CountAsync();
                if (totalConfigCount == 0)
                {
                    _logger.LogError(
                        "Ranking configs are missing. Runtime ranking seeding has been disabled; database configs are required.");
                    throw new InvalidOperationException(
                        "Ranking configs are missing. Please run startup seed sync before using rankings.");
                }

                var configEntities = await _db.Queryable<RankingConfigEntity>()
                    .Where(c => c.IsEnabled)
                    .ToListAsync();

                _rankingConfigs.Clear();
                foreach (var entity in configEntities)
                {
                    var config = MapToConfig(entity);
                    _rankingConfigs[config.RankingId] = config;
                }

                _isInitialized = true;

                _logger.LogInformation("Ranking system cache reloaded with {Count} rankings", _rankingConfigs.Count);
            }
            finally
            {
                InitializationLock.Release();
            }
        }

        /// <summary>
        /// 获取所有排行榜配置。
        /// </summary>
        /// <returns>按展示顺序排序后的排行榜配置列表。</returns>
        /// <summary>
        /// 获取所有排行榜配置。
        /// </summary>
        /// <returns>按展示顺序排序后的排行榜配置列表。</returns>
        public async Task<List<RankingConfig>> GetAllRankingsAsync()
        {
            await EnsureInitializedAsync();
            return _rankingConfigs.Values.OrderBy(r => r.SortOrder).ToList();
        }

        /// <summary>
        /// 获取指定排行榜配置。
        /// </summary>
        /// <param name="rankingId">排行榜编号。</param>
        /// <returns>命中的排行榜配置；不存在时返回空。</returns>
        public async Task<RankingConfig?> GetRankingConfigAsync(string rankingId)
        {
            await EnsureInitializedAsync();
            return _rankingConfigs.TryGetValue(rankingId, out var config) ? config : null;
        }

        /// <summary>
        /// 获取排行榜当前条目。
        /// </summary>
        /// <param name="rankingId">排行榜编号。</param>
        /// <returns>按分数降序返回的排行榜条目。</returns>
        public async Task<List<RankingEntry>> GetRankingAsync(string rankingId)
        {
            await EnsureInitializedAsync();
            var config = await GetRankingConfigAsync(rankingId);
            if (config == null)
            {
                throw new KeyNotFoundException($"排行榜不存在或未启用：{rankingId}");
            }

            var seasonPart = config.SeasonEnabled && rankingId.Equals("ranking_arena", StringComparison.OrdinalIgnoreCase)
                ? $":season:{config.CurrentSeason}"
                : string.Empty;
            var cacheKey = $"ranking:list:{rankingId}:{RankingCacheVersion}{seasonPart}";
            var cached = await _cacheService.GetAsync<List<RankingEntry>>(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("Ranking cache hit. RankingId={RankingId}, CacheKey={CacheKey}, Count={Count}", rankingId, cacheKey, cached.Count);
                return cached;
            }

            var buildLock = RankingBuildLocks.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));
            await buildLock.WaitAsync();
            try
            {
                cached = await _cacheService.GetAsync<List<RankingEntry>>(cacheKey);
                if (cached != null)
                {
                    _logger.LogInformation("Ranking cache hit after lock. RankingId={RankingId}, CacheKey={CacheKey}, Count={Count}", rankingId, cacheKey, cached.Count);
                    return cached;
                }

                _logger.LogInformation("Ranking cache miss; building from business tables. RankingId={RankingId}, CacheKey={CacheKey}", rankingId, cacheKey);
                var ranking = await BuildRankingFromBusinessTablesAsync(rankingId, config);
                await _cacheService.SetAsync(cacheKey, ranking, RankingCacheExpirationMinutes);
                _logger.LogInformation("Ranking cache populated. RankingId={RankingId}, CacheKey={CacheKey}, Count={Count}, AbsoluteExpirationMinutes={ExpirationMinutes}", rankingId, cacheKey, ranking.Count, RankingCacheExpirationMinutes);
                return ranking;
            }
            finally
            {
                buildLock.Release();
            }
        }

        /// <summary>
        /// 从真实业务表批量构建指定排行榜，不读取或写入 ranking_entry。
        /// </summary>
        private async Task<List<RankingEntry>> BuildRankingFromBusinessTablesAsync(string rankingId, RankingConfig config)
        {
            var users = await _db.Queryable<UserEntity>()
                .Where(user => !user.IsDeleted)
                .ToListAsync();

            var entries = new List<RankingEntry>();
            if (rankingId.Equals("ranking_level", StringComparison.OrdinalIgnoreCase))
            {
                entries = users
                    .OrderByDescending(user => user.Level)
                    .ThenByDescending(user => user.Exp)
                    .ThenBy(user => user.GID, StringComparer.Ordinal)
                    .Select((user, index) => CreateBusinessEntry(user.GID, user.Name, user.Level, user.Level * 1_000_000L + user.Exp, index + 1, user.CurrentTitle, user.LastUpdateTime))
                    .Take(config.MaxSize)
                    .ToList();
            }
            else if (rankingId.Equals("ranking_wealth", StringComparison.OrdinalIgnoreCase))
            {
                entries = users
                    .Select(user => new { User = user, Score = user.Gold + user.SpiritStone * 10L })
                    .OrderByDescending(item => item.Score)
                    .ThenBy(item => item.User.GID, StringComparer.Ordinal)
                    .Select((item, index) => CreateBusinessEntry(item.User.GID, item.User.Name, item.User.Level, item.Score, index + 1, item.User.CurrentTitle, item.User.LastUpdateTime))
                    .Take(config.MaxSize)
                    .ToList();
            }
            else if (rankingId.Equals("ranking_achievement", StringComparison.OrdinalIgnoreCase))
            {
                // 成就进度、配置和玩家各批量读取一次，禁止按玩家逐个查询。
                var progress = await _db.Queryable<AchievementProgressEntity>()
                    .Where(item => item.Status >= 2)
                    .ToListAsync();
                var configs = await _db.Queryable<AchievementConfigEntity>().ToListAsync();
                var points = configs.ToDictionary(item => item.AchievementId, item => (long)item.Points);
                var scoreByPlayer = progress
                    .GroupBy(item => item.PlayerId)
                    .ToDictionary(group => group.Key, group => group.Sum(item => points.TryGetValue(item.AchievementId, out var point) ? point : 0L));
                entries = users
                    .Where(user => scoreByPlayer.ContainsKey(user.GID))
                    .Select(user => new { User = user, Score = scoreByPlayer[user.GID] })
                    .OrderByDescending(item => item.Score)
                    .ThenBy(item => item.User.GID, StringComparer.Ordinal)
                    .Select((item, index) => CreateBusinessEntry(item.User.GID, item.User.Name, item.User.Level, item.Score, index + 1, item.User.CurrentTitle, item.User.LastUpdateTime))
                    .Take(config.MaxSize)
                    .ToList();
            }
            else if (rankingId.Equals("ranking_arena", StringComparison.OrdinalIgnoreCase))
            {
                var arenaQuery = _db.Queryable<ArenaPlayerEntity>();
                if (config.SeasonEnabled)
                {
                    arenaQuery = arenaQuery.Where(item => item.SeasonNumber == config.CurrentSeason);
                }
                var arena = await arenaQuery.ToListAsync();
                var userMap = users.ToDictionary(user => user.GID, StringComparer.Ordinal);
                entries = arena
                    .Where(item => userMap.ContainsKey(item.PlayerId))
                    .Select(item => new { Arena = item, User = userMap[item.PlayerId] })
                    .OrderByDescending(item => item.Arena.Points)
                    .ThenBy(item => item.Arena.PlayerId, StringComparer.Ordinal)
                    .Select((item, index) => CreateBusinessEntry(item.Arena.PlayerId, item.User.Name, item.User.Level, item.Arena.Points, index + 1, item.User.CurrentTitle, item.Arena.UpdatedAt))
                    .Take(config.MaxSize)
                    .ToList();
            }
            else if (rankingId.Equals("ranking_tower", StringComparison.OrdinalIgnoreCase))
            {
                var tower = await _db.Queryable<TowerProgressEntity>().ToListAsync();
                var userMap = users.ToDictionary(user => user.GID, StringComparer.Ordinal);
                entries = tower
                    .Where(item => userMap.ContainsKey(item.PlayerId))
                    .Select(item => new { Tower = item, User = userMap[item.PlayerId], HasTime = item.BestClearTimeMs.HasValue && item.BestClearTimeMs.Value > 0 })
                    .OrderByDescending(item => item.Tower.HighestFloor)
                    .ThenByDescending(item => item.HasTime)
                    .ThenBy(item => item.HasTime ? item.Tower.BestClearTimeMs!.Value : long.MaxValue)
                    .ThenBy(item => item.Tower.PlayerId, StringComparer.Ordinal)
                    .Select((item, index) => CreateBusinessEntry(item.Tower.PlayerId, item.User.Name, item.User.Level,
                        item.Tower.HighestFloor * 1_000_000L + (item.HasTime ? 1_000_000L - Math.Min(item.Tower.BestClearTimeMs!.Value, 999_999L) : 0L),
                        index + 1, item.User.CurrentTitle, item.Tower.UpdatedAt))
                    .Take(config.MaxSize)
                    .ToList();
            }
            else
            {
                throw new InvalidOperationException($"不支持的排行榜：{rankingId}");
            }

            foreach (var entry in entries)
            {
                // 第一版不恢复旧榜单趋势，LastRank 明确等于当前查询排名。
                entry.LastRank = entry.Rank;
            }
            return entries;
        }

        /// <summary>
        /// 创建已经补充展示字段并完成当前排名计算的排行榜条目。
        /// </summary>
        private static RankingEntry CreateBusinessEntry(string playerId, string playerName, int level, long score, int rank, string? title, DateTime lastUpdateTime)
        {
            return new RankingEntry
            {
                PlayerId = playerId,
                PlayerName = playerName,
                Level = level,
                Score = score,
                Rank = rank,
                LastRank = rank,
                CurrentTitle = title,
                LastUpdateTime = lastUpdateTime
            };
        }

        /// <summary>
        /// 获取排行榜前 N 名。
        /// </summary>
        /// <param name="rankingId">排行榜编号。</param>
        /// <param name="count">返回数量。</param>
        /// <returns>前若干名排行榜条目。</returns>
        public async Task<List<RankingEntry>> GetTopNAsync(string rankingId, int count)
        {
            if (count is < 1 or > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "排行榜数量必须在 1 到 100 之间。");
            }

            var ranking = await GetRankingAsync(rankingId);
            return ranking.Take(count).ToList();
        }

        /// <summary>
        /// 获取玩家在指定排行榜中的名次。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="rankingId">排行榜编号。</param>
        /// <returns>命中的名次；未上榜时返回空。</returns>
        public async Task<int?> GetPlayerRankAsync(string playerId, string rankingId)
        {
            var ranking = await GetRankingAsync(rankingId);
            var index = ranking.FindIndex(entry => entry.PlayerId == playerId);
            return index >= 0 ? index + 1 : null;
        }



        /// <summary>
        /// 检查赛季制排行榜是否需要开启新赛季。
        /// </summary>
        public async Task CheckSeasonsAsync()
        {
            await EnsureInitializedAsync();
            foreach (var config in _rankingConfigs.Values)
            {
                if (config.NeedsNewSeason())
                {
                    await CreateSnapshotAsync(config.RankingId, SnapshotType.SeasonEnd, $"赛季{config.CurrentSeason}结束");

                    config.StartNewSeason();

                    await _db.Updateable<RankingConfigEntity>()
                        .SetColumns(c => c.CurrentSeason == config.CurrentSeason)
                        .SetColumns(c => c.SeasonStartTime == config.SeasonStartTime)
                        .Where(c => c.RankingId == config.RankingId)
                        .ExecuteCommandAsync();
                }
            }
        }

        /// <summary>
        /// 获取排行榜统计摘要。
        /// </summary>
        /// <param name="rankingId">排行榜编号。</param>
        /// <returns>当前条目数、赛季信息和刷新时间等统计结果。</returns>
        public async Task<RankingStats> GetRankingStatsAsync(string rankingId)
        {
            await EnsureInitializedAsync();
            var config = await GetRankingConfigAsync(rankingId);
            var ranking = await GetRankingAsync(rankingId);

            return new RankingStats
            {
                RankingId = rankingId,
                RankingName = config?.RankingName ?? string.Empty,
                TotalEntries = ranking.Count,
                MaxSize = config?.MaxSize ?? 0,
                LastRefreshTime = DateTime.Now,
                SeasonEnabled = config?.SeasonEnabled ?? false,
                CurrentSeason = config?.CurrentSeason ?? 1,
                SeasonDaysRemaining = config?.GetSeasonDaysRemaining() ?? 0
            };
        }

        /// <summary>
        /// 为排行榜创建历史快照。
        /// </summary>
        /// <param name="rankingId">排行榜编号。</param>
        /// <param name="type">快照类型。</param>
        /// <param name="description">快照描述。</param>
        /// <returns>生成的快照编号；没有可保存数据时返回空。</returns>
        public async Task<string?> CreateSnapshotAsync(string rankingId, SnapshotType type, string description)
        {
            await EnsureInitializedAsync();
            var config = await GetRankingConfigAsync(rankingId);
            if (config == null) return null;

            var ranking = await GetRankingAsync(rankingId);
            if (ranking.Count == 0) return null;

            var snapshotData = JsonSerializer.Serialize(ranking);

            var historyEntity = new RankingHistoryEntity
            {
                GID = Guid.NewGuid().ToString("N"),
                RankingId = rankingId,
                SnapshotType = (int)type,
                Season = config.CurrentSeason,
                Description = description,
                SnapshotData = snapshotData,
                CreateTime = DateTime.Now
            };

            await _db.Insertable(historyEntity).ExecuteCommandAsync();

            _logger.LogInformation("Created ranking snapshot for {RankingId}, type: {Type}", rankingId, type);

            return historyEntity.GID;
        }

        /// <summary>
        /// 中文注释：
        /// 排行榜服务和商店服务一样，也是 Scoped 生命周期。
        /// 如果只依赖启动阶段那一次初始化，请求链路里拿到的新实例就没有任何排行榜配置缓存，
        /// 进一步会导致“注册成功但排行榜没有写入、查询接口总是空数组”的问题。
        /// 因此这里统一做按需初始化，保证查询和同步分数时都能先拿到有效配置。
        /// </summary>
        private async Task EnsureInitializedAsync()
        {
            if (_isInitialized && _rankingConfigs.Count > 0)
            {
                return;
            }

            await InitializeAsync();
        }


        private RankingConfig MapToConfig(RankingConfigEntity entity)
        {
            return new RankingConfig
            {
                RankingId = entity.RankingId,
                RankingName = entity.RankingName,
                RankingType = (RankingType)entity.RankingType,
                Description = entity.Description ?? "",
                MaxSize = entity.MaxSize,
                UpdateInterval = entity.UpdateInterval,
                SeasonEnabled = entity.SeasonEnabled,
                SeasonDuration = entity.SeasonDuration,
                CurrentSeason = entity.CurrentSeason,
                SeasonStartTime = entity.SeasonStartTime ?? DateTime.Now,
                SortOrder = entity.SortOrder
            };
        }


    }
}
