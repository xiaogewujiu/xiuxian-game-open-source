using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using XXX.Achievement;
using XXX.Application.DTOs;
using XXX.Application.Events;
using XXX.Application.Interfaces;
using XXX.Battle;
using XXX.Dungeon;
using XXX.Entity;
using XXX.Infrastructure.Repositories;
using XXX.Player;
using XXX.Quest;

namespace XXX.Application.Services
{
    /// <summary>
    /// 战斗服务实现
    /// </summary>
    public class BattleService : IBattleService
    {
        private const int BaseBattleCooldownSeconds = 5;
        private const int OfflineBattleWorkerBatchSize = 1;
        private static readonly JsonSerializerOptions OfflineBattleJsonOptions = new(JsonSerializerDefaults.Web);

        // 中文注释：
        // GameData 里的怪物、地图、掉落模版都是静态模版数据，只需要初始化一次。
        // 如果每次请求都重复初始化，轻则浪费性能，重则可能把运行时状态和静态配置搅在一起。

        // 中文注释：
        // 副本次数的扣减必须保证"读取次数 -> 判断上限 -> 写回次数"这一整段是串行的，
        // 否则并发点击挑战按钮时，可能出现同一天超额挑战的问题。
        private static readonly SemaphoreSlim DungeonChallengeLock = new(1, 1);
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> OfflineBattlePlayerLocks = new(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> PartyBattleLocks = new(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<string, BattleResultDto> PartyBattleResults = new(StringComparer.OrdinalIgnoreCase);


        private readonly IRepository<UserEntity> _userRepository;
        private readonly IPlayerAttributeService _playerAttributeService;
        private readonly IInventoryService _inventoryService;
        private readonly IRepository<InventoryItemEntity> _inventoryItemRepository;
        private readonly IRepository<EquipmentInstanceEntity> _equipmentRepository;
        private readonly IEquipmentService _equipmentService;
        private readonly IRepository<PetInstanceEntity> _petRepository;
        private readonly IRepository<FiveElementArrayEntity> _fiveElementRepository;
        private readonly IPetService _petService;
        private readonly IRepository<DungeonDailyRecordEntity> _dungeonDailyRecordRepository;
        private readonly IRepository<PartyEntity> _partyRepository;
        private readonly IRepository<PartyMemberEntity> _partyMemberRepository;
        private readonly IRepository<PartyBattleRecordEntity> _partyBattleRecordRepository;
        private readonly IQuestService _questService;
        private readonly IAchievementService _achievementService;
        private readonly ICollectionService _collectionService;
        private readonly IFavorabilityService _favorabilityService;
        private readonly IBattleProgressQueue _battleProgressQueue;
        private readonly ILogger<BattleService> _logger;

        /// <summary>
        /// 初始化战斗服务。
        /// </summary>
        /// <param name="userRepository">玩家仓储。</param>
        /// <param name="playerAttributeService">玩家属性服务。</param>
        /// <param name="inventoryService">背包服务。</param>
        /// <param name="inventoryItemRepository">背包物品仓储。</param>
        /// <param name="equipmentRepository">装备仓储。</param>
        /// <param name="petRepository">灵宠仓储。</param>
        /// <param name="fiveElementRepository">五行阵仓储。</param>
        /// <param name="petService">灵宠服务。</param>
        /// <param name="dungeonDailyRecordRepository">副本日常记录仓储。</param>
        /// <param name="partyRepository">临时队伍仓储。</param>
        /// <param name="partyMemberRepository">临时队伍成员仓储。</param>
        /// <param name="partyBattleRecordRepository">组队副本摘要仓储。</param>
        /// <param name="questService">任务服务。</param>
        /// <param name="achievementService">成就服务。</param>
        /// <param name="collectionService">图鉴服务。</param>
        /// <param name="favorabilityService">好感度服务。</param>
        /// <param name="logger">日志记录器。</param>
        public BattleService(
            IRepository<UserEntity> userRepository,
            IPlayerAttributeService playerAttributeService,
            IInventoryService inventoryService,
            IRepository<InventoryItemEntity> inventoryItemRepository,
            IRepository<EquipmentInstanceEntity> equipmentRepository,
            IEquipmentService equipmentService,
            IRepository<PetInstanceEntity> petRepository,
            IRepository<FiveElementArrayEntity> fiveElementRepository,
            IPetService petService,
            IRepository<DungeonDailyRecordEntity> dungeonDailyRecordRepository,
            IRepository<PartyEntity> partyRepository,
            IRepository<PartyMemberEntity> partyMemberRepository,
            IRepository<PartyBattleRecordEntity> partyBattleRecordRepository,
            IQuestService questService,
            IAchievementService achievementService,
            ICollectionService collectionService,
            IFavorabilityService favorabilityService,
            IBattleProgressQueue battleProgressQueue,
            ILogger<BattleService> logger)
        {
            _userRepository = userRepository;
            _playerAttributeService = playerAttributeService;
            _inventoryService = inventoryService;
            _inventoryItemRepository = inventoryItemRepository;
            _equipmentRepository = equipmentRepository;
            _equipmentService = equipmentService;
            _petRepository = petRepository;
            _fiveElementRepository = fiveElementRepository;
            _petService = petService;
            _dungeonDailyRecordRepository = dungeonDailyRecordRepository;
            _partyRepository = partyRepository;
            _partyMemberRepository = partyMemberRepository;
            _partyBattleRecordRepository = partyBattleRecordRepository;
            _questService = questService;
            _achievementService = achievementService;
            _collectionService = collectionService;
            _favorabilityService = favorabilityService;
            _battleProgressQueue = battleProgressQueue;
            _logger = logger;
        }

        /// <summary>
        /// 开始战斗（统一入口）
        /// 1. 校验请求与玩家
        /// 2. 根据 BattleType 分流到 PVE/PVP
        /// 3. 返回统一战斗结果 DTO
        /// </summary>
        public async Task<BattleResultDto> StartBattleAsync(string playerId, BattleRequestDto request)
        {
            // 防御式校验：请求为空时直接返回失败，避免后续空引用。
            if (request == null)
            {
                return new BattleResultDto
                {
                    IsWin = false,
                    BattleLog = ["Invalid battle request."]
                };
            }

            var player = await _userRepository.GetByIdAsync(playerId);
            if (player == null)
            {
                return new BattleResultDto
                {
                    IsWin = false,
                    BattleLog = ["Player not found."]
                };
            }

            var offlineRejectedResult = GetOfflineBattleRejectedResult(player);
            if (offlineRejectedResult != null)
            {
                return offlineRejectedResult;
            }

            var cooldownRejectedResult = GetBattleCooldownRejectedResult(player);
            if (cooldownRejectedResult != null)
            {
                return cooldownRejectedResult;
            }

            if (!string.IsNullOrEmpty(player.ActiveDungeonInstanceId))
            {
                return new BattleResultDto
                {
                    IsWin = false,
                    BattleLog = ["秘境探索中，无法进行普通战斗。请先退出秘境。"]
                };
            }

            EnsureGameDataInitialized();

            // 统一入口分流：同一 API 根据战斗类型走不同核心流程。
            var battleType = request.BattleType?.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(battleType))
            {
                battleType = "PVE";
            }

            if (battleType == "PVP")
            {
                return await StartPvpBattleAsync(player, request);
            }

            if (battleType != "PVE")
            {
                return new BattleResultDto
                {
                    IsWin = false,
                    BattleLog = [$"Unsupported battle type: {request.BattleType}"]
                };
            }

            return await StartPveBattleAsync(player, request);
        }

        /// <summary>
        /// 获取普通地图列表
        /// </summary>
        /// <remarks>
        /// 方法作用：返回主界面常规刷怪使用的普通地图。
        /// 关键逻辑：普通地图与副本不是同一套规则，这里只按地图等级开放，不计算每日挑战次数。
        /// </remarks>
        public async Task<List<BattleMapDto>> GetAvailableMapsAsync(string playerId)
        {
            var player = await _userRepository.GetByIdAsync(playerId);
            var level = player?.Level ?? 1;

            EnsureGameDataInitialized();

            return global::XXX.GameData.Maps.Values
                .Where(IsNormalBattleMap)
                .OrderBy(map => map.Level)
                .Select(map => new BattleMapDto
                {
                    MapId = map.MapGId,
                    Name = map.Name,
                    Description = map.Description,
                    RecommendedLevel = map.Level,
                    MonsterCountMin = map.MonsterCount.min,
                    MonsterCountMax = map.MonsterCount.max,
                    IsAvailable = level >= map.Level,
                    UnavailableReason = BuildBattleMapUnavailableReason(level, map.Level),
                    Monsters = BuildMapMonsterPreviews(map),
                    Drops = BuildMapDropPreviews(map)
                })
                .ToList();
        }

        /// <summary>
        /// 根据地图刷怪规则生成真实的敌情预览。
        /// </summary>
        private static List<BattleMapMonsterPreviewDto> BuildMapMonsterPreviews(Map map)
        {
            return (map.SpawnRules ?? [])
                .Where(rule => rule != null && !string.IsNullOrWhiteSpace(rule.MonsterTemplateId))
                .Select(rule =>
                {
                    global::XXX.GameData.MonsterTemplates.TryGetValue(rule.MonsterTemplateId, out var monster);
                    var level = int.TryParse(monster?.Level, out var parsedLevel) ? parsedLevel : 0;
                    return new BattleMapMonsterPreviewDto
                    {
                        MonsterId = rule.MonsterTemplateId,
                        Name = string.IsNullOrWhiteSpace(monster?.Name) ? rule.MonsterTemplateId : monster.Name,
                        Level = level,
                        Weight = Math.Max(1, rule.Weight),
                        MaxCount = Math.Max(1, rule.MaxCount)
                    };
                })
                .ToList();
        }

        /// <summary>
        /// 汇总地图关联怪物的具体道具和装备掉落，并按模板编号去重。
        /// </summary>
        private static List<BattleDropDto> BuildMapDropPreviews(Map map)
        {
            var itemDrops = new Dictionary<string, BattleDropDto>(StringComparer.OrdinalIgnoreCase);
            var equipmentDrops = new Dictionary<string, BattleDropDto>(StringComparer.OrdinalIgnoreCase);

            foreach (var rule in map.SpawnRules ?? [])
            {
                if (rule == null || !global::XXX.GameData.MonsterTemplates.TryGetValue(rule.MonsterTemplateId, out var monster))
                {
                    continue;
                }

                foreach (var drop in monster.ItemDrops ?? [])
                {
                    if (string.IsNullOrWhiteSpace(drop.ItemId) || itemDrops.ContainsKey(drop.ItemId))
                    {
                        continue;
                    }

                    if (global::XXX.GameData.Items.TryGetValue(drop.ItemId, out var item))
                    {
                        itemDrops[drop.ItemId] = new BattleDropDto
                        {
                            ItemId = drop.ItemId,
                            Name = string.IsNullOrWhiteSpace(item.Name) ? drop.ItemId : item.Name,
                            Quality = item.Quality,
                            IconPath = item.IconPath
                        };
                    }
                }

                foreach (var drop in monster.EquipmentDrops ?? [])
                {
                    if (!int.TryParse(drop.EquipmentId, out var equipmentId) || equipmentDrops.ContainsKey(drop.EquipmentId))
                    {
                        continue;
                    }

                    if (global::XXX.GameData.EquipmentTemplates.TryGetValue(equipmentId, out var equipment))
                    {
                        equipmentDrops[drop.EquipmentId] = new BattleDropDto
                        {
                            ItemId = $"equipment_{equipmentId}",
                            Name = string.IsNullOrWhiteSpace(equipment.Name) ? drop.EquipmentId : equipment.Name,
                            Quality = (int)equipment.Quality,
                            IconPath = equipment.IconPath
                        };
                    }
                }
            }

            return itemDrops.Values.Concat(equipmentDrops.Values).ToList();
        }

        /// <summary>
        /// 开始单人普通地图离线挂机。
        /// </summary>
        public async Task<OfflineBattleStatusDto> StartOfflineBattleAsync(string playerId, StartOfflineBattleRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.MapId))
            {
                throw new InvalidOperationException("请选择要离线挂机的普通地图。");
            }

            EnsureGameDataInitialized();

            var playerLock = GetOfflineBattleLock(playerId);
            await playerLock.WaitAsync();
            try
            {
                var player = await _userRepository.GetByIdAsync(playerId)
                    ?? throw new KeyNotFoundException("玩家不存在。");

                if (player.BattleMode == BattleMode.OfflineAuto)
                {
                    throw new InvalidOperationException("当前账号已处于离线挂机中，请先停止后再重新开始。");
                }

                if (!string.IsNullOrEmpty(player.ActiveDungeonInstanceId))
                {
                    throw new InvalidOperationException("秘境探索中，无法开始离线挂机。请先退出秘境。");
                }

                var mapId = request.MapId.Trim();
                if (!global::XXX.GameData.Maps.TryGetValue(mapId, out var map) || !IsNormalBattleMap(map))
                {
                    throw new InvalidOperationException("离线挂机仅支持普通地图。");
                }

                var unavailableReason = BuildBattleMapUnavailableReason(player.Level, map.Level);
                if (!string.IsNullOrWhiteSpace(unavailableReason))
                {
                    throw new InvalidOperationException(unavailableReason);
                }

                var nowUtc = DateTime.UtcNow;
                if (!player.BattleCooldownUntilUtc.HasValue || player.BattleCooldownUntilUtc.Value < nowUtc)
                {
                    player.BattleCooldownUntilUtc = nowUtc;
                }

                var summary = new OfflineBattleSummaryDto
                {
                    MapId = mapId,
                    MapName = map.Name,
                    StartedAtUtc = nowUtc,
                    EndAtUtc = nowUtc.AddHours(48)
                };

                player.BattleMode = BattleMode.OfflineAuto;
                player.OfflineBattleMapId = mapId;
                player.OfflineBattleStartedAtUtc = nowUtc;
                player.OfflineBattleLastTickAtUtc = nowUtc;
                player.OfflineBattleEndAtUtc = summary.EndAtUtc;
                player.OfflineBattleSummaryJson = SerializeOfflineBattleSummary(summary);
                player.LastUpdateTime = DateTime.Now;
                await _userRepository.UpdateAsync(player);

                return BuildOfflineBattleStatus(player, summary);
            }
            finally
            {
                playerLock.Release();
            }
        }

        /// <summary>
        /// 停止当前离线挂机并返回汇总。
        /// </summary>
        public async Task<OfflineBattleSummaryDto> StopOfflineBattleAsync(string playerId)
        {
            var playerLock = GetOfflineBattleLock(playerId);
            await playerLock.WaitAsync();
            try
            {
                var player = await _userRepository.GetByIdAsync(playerId)
                    ?? throw new KeyNotFoundException("玩家不存在。");

                if (player.BattleMode != BattleMode.OfflineAuto)
                {
                    throw new InvalidOperationException("当前账号未处于离线挂机中。");
                }

                await AdvanceOfflineBattleSessionLockedAsync(player, DateTime.UtcNow, 1, CancellationToken.None);
                var latestPlayer = await _userRepository.GetByIdAsync(playerId)
                    ?? throw new KeyNotFoundException("玩家不存在。");
                var summary = DeserializeOfflineBattleSummary(latestPlayer);
                var stoppedAtUtc = DateTime.UtcNow;
                var response = BuildOfflineBattleSummary(latestPlayer, summary, stoppedAtUtc);

                ClearOfflineBattleSession(latestPlayer);
                await _userRepository.UpdateAsync(latestPlayer);
                return response;
            }
            finally
            {
                playerLock.Release();
            }
        }

        /// <summary>
        /// 获取当前离线挂机状态。
        /// </summary>
        public async Task<OfflineBattleStatusDto> GetOfflineBattleStatusAsync(string playerId)
        {
            var player = await _userRepository.GetByIdAsync(playerId)
                ?? throw new KeyNotFoundException("玩家不存在。");

            return BuildOfflineBattleStatus(player);
        }

        /// <summary>
        /// 推进所有离线挂机账号。
        /// </summary>
        public async Task ProcessOfflineBattleTicksAsync(CancellationToken cancellationToken = default)
        {
            EnsureGameDataInitialized();

            var players = await _userRepository.Db.Queryable<UserEntity>()
                .Where(user => !user.IsDeleted && user.BattleMode == BattleMode.OfflineAuto)
                .ToListAsync();
            if (players.Count == 0)
            {
                return;
            }

            var settledAtUtc = DateTime.UtcNow;
            foreach (var player in players)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var playerLock = GetOfflineBattleLock(player.GID);
                var lockTaken = await playerLock.WaitAsync(0, cancellationToken);
                if (!lockTaken)
                {
                    continue;
                }

                try
                {
                    var latestPlayer = await _userRepository.GetByIdAsync(player.GID);
                    if (latestPlayer == null || latestPlayer.IsDeleted || latestPlayer.BattleMode != BattleMode.OfflineAuto)
                    {
                        continue;
                    }

                    await AdvanceOfflineBattleSessionLockedAsync(latestPlayer, settledAtUtc, OfflineBattleWorkerBatchSize, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to process offline battle tick for player {PlayerId}", player.GID);
                }
                finally
                {
                    playerLock.Release();
                }
            }
        }

        /// <summary>
        /// 执行 PVE 战斗流程
        /// 1. 解析地图并调用核心 BattleSystem
        /// 2. 胜利后结算经验/金币/掉落
        /// 3. 回写玩家战斗统计
        /// </summary>
        private async Task<BattleResultDto> StartPveBattleAsync(UserEntity player, BattleRequestDto request)
        {
            var stopwatch = Stopwatch.StartNew();
            var mapId = ResolveMapId(player, request);
            if (string.IsNullOrWhiteSpace(mapId))
            {
                return new BattleResultDto
                {
                    IsWin = false,
                    BattleLog = ["普通战斗只能进入普通地图，副本请使用专用挑战入口。"]
                };
            }

            var execution = await ExecuteSinglePveBattleAsync(player, mapId);
            var result = execution.Result!;
            _logger.LogInformation(
                "StartPveBattleAsync timing. PlayerId={PlayerId}, MapId={MapId}, TotalMs={TotalMs}",
                player.GID,
                mapId,
                stopwatch.ElapsedMilliseconds);
            return result;
        }

        /// <summary>
        /// 执行 PVP 战斗流程
        /// 1. 校验目标玩家（必填、非自己、存在）
        /// 2. 调用核心玩家对战
        /// 3. 禁用 PVE 资源奖励并回写双方战绩
        /// </summary>
        private async Task<BattleResultDto> StartPvpBattleAsync(UserEntity player, BattleRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.TargetId))
            {
                return new BattleResultDto
                {
                    IsWin = false,
                    BattleLog = ["PVP target player id is required."]
                };
            }

            if (string.Equals(player.GID, request.TargetId, StringComparison.OrdinalIgnoreCase))
            {
                return new BattleResultDto
                {
                    IsWin = false,
                    BattleLog = ["Cannot challenge yourself."]
                };
            }

            var enemyPlayer = await _userRepository.GetByIdAsync(request.TargetId);
            if (enemyPlayer == null)
            {
                return new BattleResultDto
                {
                    IsWin = false,
                    BattleLog = ["Target player not found."]
                };
            }

            var targetOfflineRejectedResult = GetOfflineBattleRejectedResult(enemyPlayer, $"{enemyPlayer.Name}正在离线挂机中");
            if (targetOfflineRejectedResult != null)
            {
                return targetOfflineRejectedResult;
            }

            var targetCooldownRejectedResult = GetBattleCooldownRejectedResult(enemyPlayer, $"{enemyPlayer.Name}处于战斗冷却中");
            if (targetCooldownRejectedResult != null)
            {
                return targetCooldownRejectedResult;
            }

            await _playerAttributeService.ApplyAllBonusesToEntityAsync(player.GID, player);
            var activePets = await HydrateActivePetsForBattleAsync(new[] { player, enemyPlayer });
            var coreResult = BattleSystem.StartBattle(
                [player],
                [enemyPlayer],
                activePets: activePets);

            var result = MapToBattleResultDto(coreResult);
            var challengerKillCount = GetPlayerKillCount(coreResult, player.GID);
            var opponentKillCount = GetPlayerKillCount(coreResult, enemyPlayer.GID);
            result.ExpGained = 0;
            result.GoldGained = 0;
            result.Drops = [];
            result.BattleLog.Add("PVP mode: reward settlement disabled.");

            // 双方都计入一场战斗，胜方增加一场胜利。
            await UpdatePvpBattleStatsAsync(player.GID, enemyPlayer.GID, result.IsWin, challengerKillCount, opponentKillCount);
            await SyncBattleQuestsAsync(player.GID, new[] { coreResult }, result.IsWin, null, null);
            await SyncBattleQuestsAsync(enemyPlayer.GID, new[] { coreResult }, !result.IsWin, null, null);
            await SyncBattleAchievementsAsync(player.GID, new[] { coreResult }, result.IsWin);
            await SyncBattleAchievementsAsync(enemyPlayer.GID, new[] { coreResult }, !result.IsWin);
            await ApplyBattleCooldownAsync([player, enemyPlayer], result.TotalRounds);
            PopulateBattleCooldown(result, player, result.TotalRounds);
            return result;
        }

        /// <summary>
        /// 获取可挑战副本列表
        /// 返回推荐等级、每日奖励上限、当日已获得奖励次数与可进入状态
        /// </summary>
        public async Task<List<DungeonDto>> GetAvailableDungeonsAsync(string playerId)
        {
            var player = await _userRepository.GetByIdAsync(playerId);
            var level = player?.Level ?? 1;
            var today = DateTime.Today;
            var currentPartyMembership = await _partyMemberRepository.GetFirstAsync(member => member.PlayerId == playerId);
            var currentParty = currentPartyMembership == null
                ? null
                : await _partyRepository.GetByIdAsync(currentPartyMembership.PartyId);
            var allowedTeamSize = currentParty?.MaxMembers;
            if (!allowedTeamSize.HasValue)
            {
                return [];
            }

            var dungeons = new List<DungeonDto>();

            // 列表由"静态副本配置 + 动态当日次数"共同组成。
            foreach (var definition in DungeonCatalog.Entries.Where(entry => entry.RequiredTeamSize == allowedTeamSize.Value))
            {
                var todayCount = await GetTodayDungeonChallengeCountAsync(playerId, definition.DungeonId, today);
                var remainingCount = Math.Max(0, definition.DailyLimit - todayCount);
                var isAvailable = level >= definition.RecommendedLevel &&
                                  definition.RequiredTeamSize == allowedTeamSize.Value;
                dungeons.Add(new DungeonDto
                {
                    DungeonId = definition.DungeonId,
                    Name = definition.Name,
                    Description = definition.Description,
                    RecommendedLevel = definition.RecommendedLevel,
                    DailyLimit = definition.DailyLimit,
                    TodayCount = todayCount,
                    RemainingCount = remainingCount,
                    IsAvailable = isAvailable,
                    UnavailableReason = BuildDungeonUnavailableReason(level, definition, todayCount),
                    RequiredTeamSize = definition.RequiredTeamSize,
                    SupportsParty = definition.SupportsParty
                });
            }

            return dungeons;
        }

        /// <summary>
        /// 挑战副本
        /// 1. 校验玩家、等级与副本存在性
        /// 2. 按每名成员的奖励次数决定是否扣减奖励次数
        /// 3. 执行核心副本战斗并聚合多层结果
        /// 4. 胜利结算奖励并回写战绩
        /// </summary>
        public async Task<BattleResultDto> ChallengeDungeonAsync(string playerId, string dungeonId)
        {
            var player = await _userRepository.GetByIdAsync(playerId);
            if (player == null)
            {
                return new BattleResultDto
                {
                    IsWin = false,
                    BattleLog = ["Player not found."]
                };
            }

            var offlineRejectedResult = GetOfflineBattleRejectedResult(player);
            if (offlineRejectedResult != null)
            {
                return offlineRejectedResult;
            }

            var definition = GetDungeonDefinition(dungeonId);
            if (definition == null)
            {
                return new BattleResultDto
                {
                    IsWin = false,
                    BattleLog = [$"Dungeon not found: {dungeonId}"]
                };
            }

            var cooldownRejectedResult = GetBattleCooldownRejectedResult(player);
            if (cooldownRejectedResult != null)
            {
                return cooldownRejectedResult;
            }

            if (player.Level < definition.RecommendedLevel)
            {
                return new BattleResultDto
                {
                    IsWin = false,
                    BattleLog = [$"Level {definition.RecommendedLevel} required."]
                };
            }

            if (definition.RequiredTeamSize > 1)
            {
                return new BattleResultDto
                {
                    IsWin = false,
                    BattleLog = [$"该副本需要 {definition.RequiredTeamSize} 人临时队伍挑战。"]
                };
            }

            return await ChallengeDungeonInternalAsync([player], definition, player.GID);
        }

        /// <summary>
        /// 使用临时队伍挑战副本。
        /// </summary>
        /// <param name="playerId">发起挑战的队长编号。</param>
        /// <param name="partyId">临时队伍编号。</param>
        /// <param name="dungeonId">副本编号。</param>
        /// <param name="requestId">客户端请求幂等编号；为空时服务端生成。</param>
        /// <returns>组队副本挑战结果。</returns>
        public async Task<BattleResultDto> ChallengeDungeonWithPartyAsync(string playerId, string partyId, string dungeonId, string? requestId = null)
        {
            var partyLock = PartyBattleLocks.GetOrAdd(partyId, _ => new SemaphoreSlim(1, 1));
            if (!await partyLock.WaitAsync(0))
            {
                return new BattleResultDto
                {
                    IsWin = false,
                    Success = false,
                    BattleLog = ["该队伍正在进行副本挑战，请等待当前挑战完成。"]
                };
            }

            try
            {
                var normalizedRequestId = string.IsNullOrWhiteSpace(requestId) ? Guid.NewGuid().ToString("N") : requestId.Trim();
                var existingRecord = await _partyBattleRecordRepository.Db.Queryable<PartyBattleRecordEntity>()
                    .Where(record => record.PartyId == partyId && record.RequestId == normalizedRequestId)
                    .FirstAsync();
                if (existingRecord != null)
                {
                    if (!string.IsNullOrWhiteSpace(existingRecord.FullReplayJson))
                    {
                        var replay = JsonSerializer.Deserialize<BattleResultDto>(existingRecord.FullReplayJson);
                        if (replay != null) return replay;
                    }

                    return new BattleResultDto
                    {
                        IsWin = false,
                        Success = false,
                        BattleId = existingRecord.BattleId,
                        PartyId = partyId,
                        StartedByPlayerId = existingRecord.InitiatorPlayerId,
                        BattleLog = ["该请求的组队副本挑战正在结算中，请稍后查询 BattleId。"]
                    };
                }

                var result = await ChallengeDungeonWithPartyLockedAsync(playerId, partyId, dungeonId, normalizedRequestId);
                if (result.Success && string.IsNullOrWhiteSpace(result.BattleId))
                {
                    result.BattleId = Guid.NewGuid().ToString("N");
                }
                return result;
            }
            finally
            {
                partyLock.Release();
            }
        }

        /// <summary>
        /// 在队伍锁内执行组队副本挑战，避免同一 PartyId 并发执行两场真实战斗。
        /// </summary>
        private async Task<BattleResultDto> ChallengeDungeonWithPartyLockedAsync(string playerId, string partyId, string dungeonId, string requestId)
        {
            var definition = GetDungeonDefinition(dungeonId);
            if (definition == null)
            {
                return new BattleResultDto
                {
                    IsWin = false,
                    BattleLog = [$"Dungeon not found: {dungeonId}"]
                };
            }

            if (definition.RequiredTeamSize < 1 || definition.RequiredTeamSize > 3)
            {
                return new BattleResultDto
                {
                    IsWin = false,
                    BattleLog = ["副本人数配置非法，仅支持 1、2、3 人。"]
                };
            }

            var party = await _partyRepository.GetByIdAsync(partyId);
            if (party == null)
            {
                return new BattleResultDto
                {
                    IsWin = false,
                    BattleLog = ["队伍不存在，或目标副本与当前挑战不一致。"]
                };
            }

            if (party.MaxMembers != definition.RequiredTeamSize)
            {
                return new BattleResultDto
                {
                    IsWin = false,
                    BattleLog = [$"队伍人数 {party.MaxMembers} 与副本要求 {definition.RequiredTeamSize} 人不匹配。"]
                };
            }

            if (!string.Equals(party.LeaderPlayerId, playerId, StringComparison.OrdinalIgnoreCase))
            {
                return new BattleResultDto
                {
                    IsWin = false,
                    BattleLog = ["只有队长可以发起组队副本挑战。"]
                };
            }

            var members = await _partyMemberRepository.Db.Queryable<PartyMemberEntity>()
                .Where(m => m.PartyId == partyId)
                .OrderBy(m => m.JoinTime)
                .ToListAsync();
            if (members.Count != definition.RequiredTeamSize)
            {
                return new BattleResultDto
                {
                    IsWin = false,
                    BattleLog = [$"该副本需要完整的 {definition.RequiredTeamSize} 人队伍后才能挑战。"]
                };
            }

            var memberIds = members.Select(m => m.PlayerId).Distinct().ToList();
            var players = await _userRepository.Db.Queryable<UserEntity>()
                .Where(u => memberIds.Contains(u.GID) && !u.IsDeleted)
                .ToListAsync();
            if (players.Count != definition.RequiredTeamSize)
            {
                return new BattleResultDto
                {
                    IsWin = false,
                    BattleLog = ["队伍成员数据异常，请重新组队后再试。"]
                };
            }

            var teamOfflineRejectedResult = GetTeamOfflineBattleRejectedResult(players);
            if (teamOfflineRejectedResult != null)
            {
                return teamOfflineRejectedResult;
            }

            var teamCooldownRejectedResult = GetTeamBattleCooldownRejectedResult(players);
            if (teamCooldownRejectedResult != null)
            {
                return teamCooldownRejectedResult;
            }

            var underLevelPlayer = players.FirstOrDefault(user => user.Level < definition.RecommendedLevel);
            if (underLevelPlayer != null)
            {
                return new BattleResultDto
                {
                    IsWin = false,
                    BattleLog = [$"{underLevelPlayer.Name} 尚未达到 Lv.{definition.RecommendedLevel}，无法进入该副本。"]
                };
            }

            var today = DateTime.Today;
            var eligibility = new List<PartyRewardEligibility>();
            foreach (var member in members)
            {
                var attemptsBefore = await GetTodayDungeonChallengeCountAsync(member.PlayerId, definition.DungeonId, today);
                eligibility.Add(new PartyRewardEligibility
                {
                    PlayerId = member.PlayerId,
                    RewardEligible = attemptsBefore < definition.DailyLimit,
                    AttemptsBefore = attemptsBefore,
                    PartyId = partyId,
                    DungeonId = definition.DungeonId
                });
            }
            var rewardEligiblePlayerIds = eligibility
                .Where(item => item.RewardEligible)
                .Select(item => item.PlayerId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var result = await ChallengeDungeonInternalAsync(players, definition, playerId, rewardEligiblePlayerIds, partyId, requestId, members.Select(member => member.PlayerId).ToList(), eligibility);
            result.PartyId = partyId;
            result.Message = $"队长已发起 {definition.Name} 组队挑战，队伍人数 {players.Count}/{definition.RequiredTeamSize}。";
            result.StartedByPlayerId = playerId;
            if (string.IsNullOrWhiteSpace(result.BattleId))
            {
                result.BattleId = Guid.NewGuid().ToString("N");
            }
            return result;
        }

        private async Task<BattleResultDto> ChallengeDungeonInternalAsync(
            List<UserEntity> players,
            DungeonCatalogEntry definition,
            string initiatorPlayerId,
            IReadOnlyCollection<string>? rewardEligiblePlayerIds = null,
            string? partyId = null,
            string? requestId = null,
            IReadOnlyCollection<string>? memberIds = null,
            IReadOnlyCollection<PartyRewardEligibility>? eligibility = null)
        {
            EnsureGameDataInitialized();

            var mapId = ResolveDungeonFubenMapId(definition.DungeonId);
            var chainValidation = ValidateDungeonMapChain(definition.DungeonId, mapId);
            if (!chainValidation.IsValid)
            {
                return new BattleResultDto
                {
                    IsWin = false,
                    Success = false,
                    BattleLog = [chainValidation.ErrorMessage]
                };
            }

            var playerIds = players.Select(player => player.GID).Distinct().ToList();
            var partyBattleId = Guid.NewGuid().ToString("N");
            PartyBattleRecordEntity? partyRecord = null;
            if (!string.IsNullOrWhiteSpace(partyId) && !string.IsNullOrWhiteSpace(requestId))
            {
                partyRecord = new PartyBattleRecordEntity
                {
                    BattleId = partyBattleId,
                    PartyId = partyId,
                    DungeonId = definition.DungeonId,
                    InitiatorPlayerId = initiatorPlayerId,
                    MemberIdsJson = JsonSerializer.Serialize(memberIds ?? playerIds),
                    RequestId = requestId,
                    StartedAtUtc = DateTime.UtcNow,
                    SettlementStatus = "Started"
                };
                await _partyBattleRecordRepository.AddAsync(partyRecord);
            }
            var today = DateTime.Today;
            var isPartyChallenge = partyRecord != null;
            var eligibleIds = rewardEligiblePlayerIds == null
                ? new List<string>()
                : playerIds.Where(rewardEligiblePlayerIds.Contains).ToList();
            if (!isPartyChallenge)
            {
                foreach (var playerId in playerIds)
                {
                    var todayCount = await GetTodayDungeonChallengeCountAsync(playerId, definition.DungeonId, today);
                    if (todayCount < definition.DailyLimit)
                    {
                        eligibleIds.Add(playerId);
                    }
                }

                if (eligibleIds.Count > 0)
                {
                    var consumed = await TryConsumeDungeonChallengeAsync(eligibleIds, definition.DungeonId, definition.DailyLimit, today);
                    if (!consumed)
                    {
                        var counts = await Task.WhenAll(eligibleIds.Select(playerId => GetTodayDungeonChallengeCountAsync(playerId, definition.DungeonId, today)));
                        var blockedCount = counts.DefaultIfEmpty(0).Max();
                        return new BattleResultDto
                        {
                            IsWin = false,
                            BattleLog = [$"今日奖励次数已用尽（{blockedCount}/{definition.DailyLimit}），本次可参战但不获得副本奖励。"]
                        };
                    }
                }
            }

            foreach (var p in players)
            {
                await _playerAttributeService.ApplyAllBonusesToEntityAsync(p.GID, p);
            }
            var activePets = await HydrateActivePetsForBattleAsync(players);
            var stageResults = BattleSystem.StartBattleFuben(players, mapId, activePets: activePets);
            if (stageResults == null || stageResults.Count == 0)
            {
                var notStartedResult = new BattleResultDto
                {
                    IsWin = false,
                    BattleLog = ["Dungeon battle failed to start."]
                };
                if (partyRecord != null)
                {
                    partyRecord.CompletedAtUtc = DateTime.UtcNow;
                    partyRecord.SettlementStatus = "Failed";
                    partyRecord.FullReplayJson = JsonSerializer.Serialize(notStartedResult);
                    await _partyBattleRecordRepository.UpdateAsync(partyRecord);
                }
                return notStartedResult;
            }

            if (isPartyChallenge)
            {
                var consumed = eligibleIds.Count == 0 || await TryConsumeDungeonChallengeAsync(eligibleIds, definition.DungeonId, definition.DailyLimit, today);
                if (!consumed)
                {
                    var counts = await Task.WhenAll(eligibleIds.Select(playerId => GetTodayDungeonChallengeCountAsync(playerId, definition.DungeonId, today)));
                    var blockedCount = counts.DefaultIfEmpty(0).Max();
                    var blockedResult = new BattleResultDto
                    {
                        IsWin = false,
                        BattleLog = [$"今日奖励次数已用尽（{blockedCount}/{definition.DailyLimit}），本次可参战但不获得副本奖励。"]
                    };
                    if (partyRecord != null)
                    {
                        partyRecord.CompletedAtUtc = DateTime.UtcNow;
                        partyRecord.SettlementStatus = "Failed";
                        partyRecord.FullReplayJson = JsonSerializer.Serialize(blockedResult);
                        await _partyBattleRecordRepository.UpdateAsync(partyRecord);
                    }
                    return blockedResult;
                }

                // 中文注释：次数扣减完成后冻结成员资格快照，供结算记录审计。
                if (eligibility != null)
                {
                    foreach (var item in eligibility)
                    {
                        item.BattleId = partyBattleId;
                        item.AttemptConsumed = item.RewardEligible;
                    }
                }
            }

            var aggregateResult = AggregateDungeonResult(stageResults);
            var initiatorBattleExpBonusPercent = await GetArrayBattleExpBonusPercentAsync(initiatorPlayerId);
            var displayDungeonExp = aggregateResult.ExpGained > 0
                ? FiveElementProgressionRules.ApplyPercentageBonus(aggregateResult.ExpGained, initiatorBattleExpBonusPercent)
                : 0;
            var killCounts = BuildPlayerKillCounts(stageResults, playerIds);
            var stageDtos = stageResults
                .Where(stageResult => stageResult != null)
                .Select((stageResult, index) => MapToBattleStageDto(stageResult, index + 1))
                .ToList();

            var result = new BattleResultDto
            {
                IsWin = aggregateResult.IsVictory,
                IsVictory = aggregateResult.IsVictory,
                // 中文注释：
                // 多层副本的总标题应使用副本目录名，而不是第一层地图名。
                // 各层真实地图名已经在 Stages 里保留，这样前端顶部标题仍显示"副本名"，
                // 但回放区依旧能按"一层 / 二层 / 三层"逐层展示。
                MapName = definition.Name,
                Rounds = aggregateResult.TotalRounds,
                TotalRounds = aggregateResult.TotalRounds,
                ExpGained = displayDungeonExp,
                GoldGained = aggregateResult.GoldGained,
                Drops = eligibleIds.Count > 0 ? MapDrops(aggregateResult) : [],
                BattleLog = FlattenDungeonBattleLog(stageResults),
                RoundLogs = stageDtos.FirstOrDefault()?.RoundLogs ?? [],
                FighterSkillStats = stageDtos.FirstOrDefault()?.FighterSkillStats ?? [],
                Stages = stageDtos
            };

            if (result.BattleLog.Count == 0)
            {
                result.BattleLog.Add(result.IsWin ? "Dungeon finished: victory." : "Dungeon finished: defeat.");
            }

            if (!result.IsWin)
            {
                result.ExpGained = 0;
                result.GoldGained = 0;
                result.Drops = [];
                result.BattleLog.Add("Dungeon failed: rewards not granted.");
            }

            if (result.IsWin)
            {
                foreach (var player in players.Where(player => eligibleIds.Contains(player.GID)))
                {
                    var memberReward = CreateIndependentPartyReward(player.GID, aggregateResult);
                    await AwardDungeonRewardsAsync(player.GID, memberReward, result.BattleLog, definition, players.Count);
                }
            }

            await UpdateBattleStatsAsync(playerIds, result.IsWin, killCounts);
            foreach (var playerId in playerIds)
            {
                await SyncBattleQuestsAsync(playerId, stageResults, result.IsWin, definition.DungeonId, mapId);
                await SyncBattleAchievementsAsync(playerId, stageResults, result.IsWin, definition.DungeonId);
            }
            await ApplyBattleCooldownAsync(players, result.TotalRounds);
            try
            {
                await _favorabilityService.AwardPartyBattleFavorabilityAsync(playerIds);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to award party battle favorability for dungeon {DungeonId}", definition.DungeonId);
            }
            var initiator = players.FirstOrDefault(player => string.Equals(player.GID, initiatorPlayerId, StringComparison.OrdinalIgnoreCase));
            if (initiator != null)
            {
                PopulateBattleCooldown(result, initiator, result.TotalRounds);
            }
            if (partyRecord != null)
            {
                result.BattleId = partyBattleId;
            }
            if (partyRecord != null)
            {
                partyRecord.CompletedAtUtc = DateTime.UtcNow;
                partyRecord.IsVictory = result.IsWin;
                partyRecord.StageCount = result.Stages.Count;
                partyRecord.TotalRounds = result.TotalRounds;
                partyRecord.SettlementStatus = result.IsWin ? "Completed" : "Failed";
                partyRecord.RewardSummaryJson = JsonSerializer.Serialize(new { Members = eligibility ?? [], Drops = result.Drops });
                partyRecord.FullReplayJson = JsonSerializer.Serialize(result);
                await _partyBattleRecordRepository.UpdateAsync(partyRecord);
            }
            _logger.LogInformation(
                "Dungeon {DungeonId} challenge finished by initiator {PlayerId} with {MemberCount} members, victory: {IsWin}, BattleId={BattleId}",
                definition.DungeonId,
                initiatorPlayerId,
                players.Count,
                result.IsWin,
                result.BattleId);

            return result;
        }

        /// <summary>
        /// 按 BattleId 查询组队副本摘要；当前版本先读取持久化记录结构。
        /// </summary>
        public async Task<PartyBattleRecordDto?> GetPartyBattleRecordAsync(string battleId)
        {
            if (string.IsNullOrWhiteSpace(battleId)) return null;
            var record = await _partyBattleRecordRepository.GetByIdAsync(battleId.Trim());
            return record == null ? null : new PartyBattleRecordDto
            {
                BattleId = record.BattleId, PartyId = record.PartyId, DungeonId = record.DungeonId,
                InitiatorPlayerId = record.InitiatorPlayerId, MemberIdsJson = record.MemberIdsJson,
                RequestId = record.RequestId, StartedAtUtc = record.StartedAtUtc,
                CompletedAtUtc = record.CompletedAtUtc, IsVictory = record.IsVictory,
                StageCount = record.StageCount, TotalRounds = record.TotalRounds,
                SettlementStatus = record.SettlementStatus, RewardSummaryJson = record.RewardSummaryJson
            };
        }

        private static SemaphoreSlim GetOfflineBattleLock(string playerId)
        {
            return OfflineBattlePlayerLocks.GetOrAdd(playerId, _ => new SemaphoreSlim(1, 1));
        }

        private async Task<int> AdvanceOfflineBattleSessionLockedAsync(UserEntity player, DateTime settledAtUtc, int maxBattles, CancellationToken cancellationToken)
        {
            if (player.BattleMode != BattleMode.OfflineAuto)
            {
                return 0;
            }

            var mapId = player.OfflineBattleMapId?.Trim();
            if (string.IsNullOrWhiteSpace(mapId) ||
                !global::XXX.GameData.Maps.TryGetValue(mapId, out var map) ||
                !IsNormalBattleMap(map))
            {
                ClearOfflineBattleSession(player);
                await _userRepository.UpdateAsync(player);
                return 0;
            }

            var latestPlayer = player;
            var summary = DeserializeOfflineBattleSummary(latestPlayer);
            summary.MapId = mapId;
            summary.MapName = map.Name;
            summary.StartedAtUtc ??= latestPlayer.OfflineBattleStartedAtUtc;

            var endAtUtc = latestPlayer.OfflineBattleEndAtUtc
                ?? summary.EndAtUtc
                ?? latestPlayer.OfflineBattleStartedAtUtc?.AddHours(48);
            if (endAtUtc.HasValue)
            {
                summary.EndAtUtc = endAtUtc;
            }
            var effectiveSettledAtUtc = endAtUtc.HasValue
                ? (settledAtUtc < endAtUtc.Value ? settledAtUtc : endAtUtc.Value)
                : settledAtUtc;

            var processed = 0;
            while (latestPlayer.BattleMode == BattleMode.OfflineAuto &&
                   IsOfflineBattleDue(latestPlayer, effectiveSettledAtUtc) &&
                   processed < maxBattles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var battleStartedAtUtc = GetNextOfflineBattleStartUtc(latestPlayer);
                var execution = await ExecuteSinglePveBattleAsync(latestPlayer, mapId, battleStartedAtUtc, includeBattleResult: false);
                MergeOfflineBattleSummary(summary, execution.CoreResult);

                latestPlayer.OfflineBattleLastTickAtUtc = battleStartedAtUtc;
                latestPlayer.OfflineBattleSummaryJson = SerializeOfflineBattleSummary(summary);
                latestPlayer.LastUpdateTime = DateTime.Now;
                await _userRepository.Db.Updateable<UserEntity>()
                    .SetColumns(user => user.OfflineBattleLastTickAtUtc == latestPlayer.OfflineBattleLastTickAtUtc)
                    .SetColumns(user => user.OfflineBattleSummaryJson == latestPlayer.OfflineBattleSummaryJson)
                    .SetColumns(user => user.LastUpdateTime == latestPlayer.LastUpdateTime)
                    .Where(user => user.GID == latestPlayer.GID && !user.IsDeleted)
                    .ExecuteCommandAsync();

                processed++;
                latestPlayer = await _userRepository.GetByIdAsync(latestPlayer.GID) ?? latestPlayer;
            }

            if (endAtUtc.HasValue && endAtUtc.Value <= settledAtUtc && latestPlayer.BattleMode == BattleMode.OfflineAuto)
            {
                ClearOfflineBattleSession(latestPlayer);
                await _userRepository.UpdateAsync(latestPlayer);
            }

            return processed;
        }

        private async Task<PveBattleExecutionResult> ExecuteSinglePveBattleAsync(
            UserEntity player,
            string mapId,
            DateTime? cooldownBaseUtc = null,
            bool includeBattleResult = true)
        {
            // 中文注释：
            // 性能基线阶段计时：普通 PVE 战斗请求（includeBattleResult=true）记录每个阶段的耗时，
            // 用于定位 3~7 秒瓶颈分别耗在哪些环节。离线挂机等后台路径不记录，避免日志刷屏。
            // BattleId 在普通 PVE 请求的 Controller 层不会赋值，这里生成请求级 trace id 方便按日志对账。
            var battleTraceId = Guid.NewGuid().ToString("N")[..12];
            var stageTimings = new List<(string Stage, long ElapsedMs)>();
            var stageStopwatch = Stopwatch.StartNew();
            var battleLeveledUp = false;
            void RecordStage(string stage)
            {
                stageTimings.Add((stage, stageStopwatch.ElapsedMilliseconds));
                stageStopwatch.Restart();
            }

            await _playerAttributeService.ApplyAllBonusesToEntityAsync(player.GID, player);
            RecordStage("ApplyAllBonuses");

            var activePets = await HydrateActivePetsForBattleAsync(new[] { player });
            RecordStage("HydratePets");

            var coreResult = BattleSystem.StartBattle([player], mapId, activePets: activePets);
            RecordStage("BattleSystem");

            var arrayBattleExpBonusPercent = await ApplyArrayBattleExpBonusAsync(player.GID, coreResult);
            RecordStage("ExpBonus");

            var result = includeBattleResult ? MapToBattleResultDto(coreResult) : null;
            var killCount = GetPlayerKillCount(coreResult, player.GID);
            var battleLog = result?.BattleLog ?? [];

            if (arrayBattleExpBonusPercent > 0 && coreResult.ExpGained > 0 && result != null)
            {
                battleLog.Add($"聚灵阵加持：战斗经验 +{arrayBattleExpBonusPercent}%");
            }

            if (coreResult.IsVictory)
            {
                // 中文注释：
                // 战斗胜利路径：经验、金币、道具掉落、装备掉落、战斗统计、冷却
                // 统一进入核心结算事务（ApplyCoreSettlementAsync）一次性提交，
                // 避免原来的逐项独立写入造成多次 SQLite 往返和部分奖励不一致。
                // 宠物经验（GrantPetBattleExpAsync）与图鉴掉落（SyncCollectionDropsAsync）
                // 依赖各自独立服务逻辑，保留在事务外调用，行为与原实现一致。
                var settlement = new BattleSettlementModel
                {
                    PlayerId = player.GID,
                    ExpGained = coreResult.ExpGained,
                    GoldGained = coreResult.GoldGained,
                    IsVictory = true,
                    KillCount = killCount,
                    ItemDrops = coreResult.DroppedItems
                        .Where(item => item != null && !string.IsNullOrWhiteSpace(item.ItemId))
                        .GroupBy(item => item.ItemId)
                        .Select(group => new ItemDropGroup
                        {
                            ItemId = group.Key,
                            Quantity = group.Count(),
                        })
                        .ToList(),
                    EquipmentDrops = coreResult.DroppedEquipments
                        .Where(equipment => equipment != null)
                        .ToList(),
                    CollectionDrops = coreResult.DroppedCollections
                        .Select(item => new CollectionDropGroup
                        {
                            SeriesId = item.SeriesId,
                            CollectionType = item.CollectionType
                        })
                        .ToList(),
                    TotalRounds = coreResult.TotalRounds,
                    CooldownBaseUtc = cooldownBaseUtc
                };
                battleLeveledUp = await ApplyCoreSettlementAsync(player, settlement, battleLog);
                RecordStage("CoreSettlement");
                if (battleLeveledUp)
                {
                    _logger.LogInformation(
                        "Battle level up detected. PlayerId={PlayerId}, NewLevel={NewLevel}",
                        player.GID,
                        player.Level);
                }

                await GrantPetBattleExpAsync(player.GID, coreResult.ExpGained);
                RecordStage("PetExp");

                await SyncCollectionDropsAsync(player.GID, coreResult.DroppedCollections, battleLog);
                RecordStage("CollectionDrops");
            }
            else
            {
                // 中文注释：
                // 战斗失败路径：不发放任何奖励，但战斗统计（总场次）与冷却仍然需要更新，
                // 与原实现（UpdateBattleStatsAsync + ApplyBattleCooldownAsync）行为一致。
                var settlement = new BattleSettlementModel
                {
                    PlayerId = player.GID,
                    ExpGained = 0,
                    GoldGained = 0,
                    IsVictory = false,
                    KillCount = killCount,
                    ItemDrops = [],
                    EquipmentDrops = [],
                    CollectionDrops = [],
                    TotalRounds = coreResult.TotalRounds,
                    CooldownBaseUtc = cooldownBaseUtc
                };
                await ApplyCoreSettlementAsync(player, settlement, battleLog);
                RecordStage("CoreSettlement");
            }

            var playerBattleStats = AggregatePlayerBattleStats(new[] { coreResult }, player.GID);
            await _battleProgressQueue.EnqueueAsync(new BattleProgressEvent
            {
                PlayerId = player.GID,
                MapId = mapId,
                IsVictory = coreResult.IsVictory,
                TotalDamageDealt = playerBattleStats.TotalDamageDealt,
                TotalDamageTaken = playerBattleStats.TotalDamageTaken,
                Kills = playerBattleStats.Kills,
                CritCount = playerBattleStats.CritCount,
                DodgeCount = playerBattleStats.DodgeCount,
                SkillUseCount = playerBattleStats.SkillUseCount,
                TotalMpConsumed = playerBattleStats.TotalMpConsumed,
                KilledMonsterTemplateIds = playerBattleStats.KilledMonsterTemplateIds.ToList(),
                SkillUsageBySkill = new Dictionary<int, int>(playerBattleStats.SkillUsageBySkill),
                SkillDamageBySkill = new Dictionary<int, int>(playerBattleStats.SkillDamageBySkill),
                OccurredAtUtc = DateTime.UtcNow
            });
            RecordStage("BattleProgressQueued");

            // 中文注释：
            // 属性重算和排行榜事件是两个独立维度：升级只决定是否需要重算属性，
            // 经验/金币变化则决定是否刷新等级榜/财富榜，不能用 if/else 让升级路径漏掉排行榜事件。
            if (coreResult.IsVictory && battleLeveledUp)
            {
                await _playerAttributeService.RecalculatePlayerAttributesAsync(player.GID, syncLevelDrivenProgress: true);
                RecordStage("RecalculateAttributes");
            }
            else
            {
                RecordStage("RecalculateAttributesSkipped");
            }

            // 中文注释：查询时排行榜架构不再由战斗结算发布排行榜事件，
            // 经验和金币只写入真实业务表，由排行榜查询缓存过期后重新读取。
            RecordStage("RankingQueryOnDemand");

            if (result != null)
            {
                PopulateBattleCooldown(result, player, coreResult.TotalRounds);
            }

            if (includeBattleResult)
            {
                // 中文注释：
                // 阶段计时汇总日志：每个阶段独立耗时(ms)，带 BattleId/PlayerId/MapId 便于按请求对账。
                var stageSummary = string.Join(", ", stageTimings.Select(s => $"{s.Stage}={s.ElapsedMs}"));
                _logger.LogInformation(
                    "ExecuteSinglePveBattleAsync stages. BattleId={BattleId}, PlayerId={PlayerId}, MapId={MapId}, TotalMs={TotalMs}, Stages=[{Stages}]",
                    battleTraceId, player.GID, mapId, stageTimings.Sum(s => s.ElapsedMs), stageSummary);
            }

            return new PveBattleExecutionResult(coreResult, result);
        }

        /// <summary>
        /// 战斗核心结算事务。
        /// </summary>
        /// <remarks>
        /// 中文注释：
        /// 把经验、金币、战斗统计、冷却、道具掉落、装备掉落放进同一个 SQLite 事务，
        /// 一次性提交。任何一步失败都会整体回滚，杜绝“经验发了但道具丢了”这类部分奖励。
        /// 玩家实体在事务内重新读取最新数据后再修改，避免用战斗前缓存的旧实体覆盖并发写入。
        /// 注意：宠物经验与图鉴掉落依赖独立服务，调用方（ExecuteSinglePveBattleAsync）在事务外处理。
        /// </remarks>
        /// <param name="player">战斗前加载的玩家实体（仅用于回填冷却时间到响应）。</param>
        /// <param name="settlement">本次战斗的核心结算数据。</param>
        /// <param name="battleLog">战斗日志（用于记录跳过项）。</param>
        /// <returns>是否发生升级。</returns>
        private async Task<bool> ApplyCoreSettlementAsync(UserEntity player, BattleSettlementModel settlement, List<string> battleLog)
        {
            var db = _userRepository.Db;
            var leveledUp = false;
            try
            {
                db.Ado.BeginTran();

                // 中文注释：
                // 事务内重新读取玩家最新数据。原实现 AddExpAsync 也是内部重新 GetByIdAsync，
                // 这里保持一致，避免把 ApplyAllBonuses 在内存里改过的临时属性直接整实体写回。
                var latestPlayer = await _userRepository.GetByIdAsync(settlement.PlayerId);
                if (latestPlayer == null || latestPlayer.IsDeleted)
                {
                    db.Ado.RollbackTran();
                    return false;
                }

                // 1. 经验与升级：复用 PlayerManager.AddExp 的升级逻辑（支持连续升级、突破阻挡）。
                //    原 AddExpAsync 在升级后会刷新 XExp，这里同样刷新。
                if (settlement.ExpGained > 0)
                {
                    var levelUpResult = PlayerManager.AddExp(latestPlayer, settlement.ExpGained);
                    if (levelUpResult.Success)
                    {
                        latestPlayer.XExp = PlayerManager.GetRequiredExp(latestPlayer);
                        leveledUp = levelUpResult.LeveledUp;
                    }
                }

                // 2. 金币与累计获得金币：与原 AddGoldAsync 的 SetColumns 行为一致。
                if (settlement.GoldGained > 0)
                {
                    latestPlayer.Gold += settlement.GoldGained;
                    latestPlayer.TotalGoldEarned += settlement.GoldGained;
                }

                // 3. 战斗统计：总场次 +1，胜利场次 +1（仅胜利），累计击杀 +killCount。
                latestPlayer.TotalBattles += 1;
                if (settlement.IsVictory)
                {
                    latestPlayer.WinBattles += 1;
                }

                if (settlement.KillCount > 0)
                {
                    latestPlayer.TotalKills += settlement.KillCount;
                }

                // 4. 战斗冷却：与原 ApplyBattleCooldownAsync 的计算规则一致。
                var cooldownUntilUtc = (settlement.CooldownBaseUtc ?? DateTime.UtcNow)
                    .AddSeconds(GetBattleCooldownDurationSeconds(settlement.TotalRounds));
                latestPlayer.BattleCooldownUntilUtc = cooldownUntilUtc;
                latestPlayer.LastUpdateTime = DateTime.Now;

                await _userRepository.UpdateAsync(latestPlayer);

                // 回填冷却到传入实体，供 PopulateBattleCooldown 写进响应。
                if (player != null)
                {
                    player.BattleCooldownUntilUtc = cooldownUntilUtc;
                    player.LastUpdateTime = latestPlayer.LastUpdateTime;
                }

                // 5. 道具掉落批量写入：按 ItemId 聚合后一次查询、内存合并、批量增改。
                if (settlement.ItemDrops.Count > 0)
                {
                    await SyncItemDropsBulkAsync(settlement.PlayerId, settlement.ItemDrops, battleLog);
                }

                // 6. 装备掉落批量写入：所有装备实例一次性插入。
                if (settlement.EquipmentDrops.Count > 0)
                {
                    await SyncEquipmentDropsBulkAsync(settlement.PlayerId, settlement.EquipmentDrops, battleLog);
                }

                db.Ado.CommitTran();
                return leveledUp;
            }
            catch
            {
                db.Ado.RollbackTran();
                throw;
            }
        }

        /// <summary>
        /// 批量写入道具掉落。
        /// </summary>
        /// <remarks>
        /// 中文注释：
        /// 原实现（SyncItemDropsAsync + InventoryService.AddItemAsync）对每个掉落道具组
        /// 分别执行一次玩家查询、一次现有道具查询、一次插入或更新，SQL 往返次数多。
        /// 这里改为：一次查询该玩家这些 ItemId 的现有记录，内存合并数量，
        /// 已存在的记录批量更新，不存在的记录批量插入。背包容量限制（100 种）与原实现一致。
        /// 道具入包后保留原 CollectItem 任务事件通知，保证任务进度不丢。
        /// </remarks>
        private async Task SyncItemDropsBulkAsync(string playerId, List<ItemDropGroup> drops, List<string> battleLog)
        {
            foreach (var drop in drops)
            {
                if (string.IsNullOrWhiteSpace(drop.ItemId) || drop.Quantity <= 0)
                {
                    continue;
                }

                try
                {
                    await InventoryItemGrantHelper.AddOrMergeAsync(
                        _inventoryItemRepository.Db,
                        playerId,
                        drop.ItemId,
                        drop.Quantity,
                        "背包已满，战斗掉落未发放。");

                    await _questService.RecordObjectiveEventAsync(playerId, new XXX.Quest.QuestObjectiveEvent
                    {
                        ObjectiveType = XXX.Quest.ObjectiveType.CollectItem,
                        TargetId = drop.ItemId,
                        Delta = drop.Quantity
                    });
                }
                catch (Exception ex)
                {
                    battleLog.Add($"Drop delivery skipped: {drop.ItemId} x{drop.Quantity} (背包已满)");
                    _logger.LogWarning(ex, "Failed to deliver battle item drop for player {PlayerId}, item {ItemId}", playerId, drop.ItemId);
                }
            }
        }
        /// <summary>
        /// 批量写入装备掉落。
        /// </summary>
        /// <remarks>
        /// 中文注释：
        /// 原实现（SyncEquipmentDropsAsync）逐件 AddAsync。这里改为先把所有掉落装备
        /// 映射为 EquipmentInstanceEntity（保留实例唯一标识、模板 ID、品质、绑定状态等字段，
        /// 每件装备实例独立，不按模板去重），再一次性批量插入。
        /// 映射阶段单件失败仅记录日志跳过，不影响其它装备；批量插入失败由上层事务整体回滚。
        /// </remarks>
        private async Task SyncEquipmentDropsBulkAsync(string playerId, List<EquipmentInstance> droppedEquipments, List<string> battleLog)
        {
            var entities = new List<EquipmentInstanceEntity>();
            foreach (var equipment in droppedEquipments.Where(e => e != null))
            {
                try
                {
                    var template = equipment.Template;
                    var templateId = template?.EquipmentId ?? equipment.EquipmentId;
                    if (templateId <= 0)
                    {
                        battleLog.Add("Equipment delivery skipped: invalid template id.");
                        continue;
                    }

                    entities.Add(EquipmentBalanceHelper.CreateEntity(playerId, equipment, false));
                }
                catch
                {
                    var equipmentName = string.IsNullOrWhiteSpace(equipment?.Name)
                        ? $"equipment_{equipment?.EquipmentId ?? 0}"
                        : equipment.Name;
                    battleLog.Add($"Equipment delivery skipped: {equipmentName}");
                }
            }

            if (entities.Count > 0)
            {
                var usedEquipmentSlots = await _equipmentRepository.Db.Queryable<EquipmentInstanceEntity>()
                    .Where(entity => entity.PlayerId == playerId && !entity.IsEquipped)
                    .CountAsync();
                var equipmentCapacity = await InventoryCapacityRules.GetEquipmentCapacityAsync(_equipmentRepository.Db, playerId);
                var availableSlots = Math.Max(0, equipmentCapacity - usedEquipmentSlots);
                if (availableSlots < entities.Count)
                {
                    battleLog.Add($"Equipment delivery skipped: 装备背包空间不足，最多还能放入 {availableSlots} 件装备。");
                    entities = entities.Take(availableSlots).ToList();
                }

                if (entities.Count == 0)
                {
                    return;
                }

                await _equipmentRepository.AddRangeAsync(entities);
                var autoSell = await _equipmentService.AutoSellUnqualifiedEquipmentAsync(
                    playerId,
                    entities.Select(entity => entity.InstanceId),
                    reason: "战斗结算自动出售",
                    joinCurrentTransaction: true);
                if (autoSell.SoldCount > 0)
                {
                    battleLog.Add($"自动出售 {autoSell.SoldCount} 件不符合保留条件的装备，获得 {autoSell.GoldGained} 金币。");
                }
            }
        }

        private async Task<int> ApplyArrayBattleExpBonusAsync(string playerId, BattleResult coreResult)
        {
            if (coreResult == null || coreResult.ExpGained <= 0)
            {
                return 0;
            }

            var bonusPercent = await GetArrayBattleExpBonusPercentAsync(playerId);
            if (bonusPercent <= 0)
            {
                return 0;
            }

            coreResult.ExpGained = FiveElementProgressionRules.ApplyPercentageBonus(coreResult.ExpGained, bonusPercent);
            return bonusPercent;
        }

        private async Task<int> GetArrayBattleExpBonusPercentAsync(string playerId)
        {
            var array = await _fiveElementRepository.GetFirstAsync(entity => entity.PlayerId == playerId);
            var arrayLevel = array == null
                ? 1
                : Math.Clamp(array.ArrayLevel <= 0 ? 1 : array.ArrayLevel, 1, 50);
            return FiveElementProgressionRules.GetBattleExpBonusPercent(arrayLevel);
        }

        private static bool IsOfflineBattleDue(UserEntity player, DateTime settledAtUtc)
        {
            return GetNextOfflineBattleStartUtc(player) <= settledAtUtc;
        }

        private static DateTime GetNextOfflineBattleStartUtc(UserEntity player)
        {
            return player.BattleCooldownUntilUtc
                ?? player.OfflineBattleLastTickAtUtc
                ?? player.OfflineBattleStartedAtUtc
                ?? DateTime.UtcNow;
        }

        private static void ClearOfflineBattleSession(UserEntity player)
        {
            player.BattleMode = BattleMode.Normal;
            player.OfflineBattleMapId = null;
            player.OfflineBattleStartedAtUtc = null;
            player.OfflineBattleLastTickAtUtc = null;
            player.OfflineBattleEndAtUtc = null;
            player.OfflineBattleSummaryJson = null;
            player.LastUpdateTime = DateTime.Now;
        }

        private static OfflineBattleStatusDto BuildOfflineBattleStatus(UserEntity player, OfflineBattleSummaryDto? existingSummary = null)
        {
            if (player.BattleMode != BattleMode.OfflineAuto)
            {
                return new OfflineBattleStatusDto();
            }

            var summary = existingSummary ?? DeserializeOfflineBattleSummary(player);
            var mapId = string.IsNullOrWhiteSpace(player.OfflineBattleMapId) ? summary.MapId : player.OfflineBattleMapId;
            return new OfflineBattleStatusDto
            {
                IsOfflineBattling = true,
                MapId = mapId ?? string.Empty,
                MapName = string.IsNullOrWhiteSpace(summary.MapName)
                    ? ResolveBattleMapName(mapId)
                    : summary.MapName,
                StartedAtUtc = player.OfflineBattleStartedAtUtc ?? summary.StartedAtUtc,
                LastTickAtUtc = player.OfflineBattleLastTickAtUtc,
                EndAtUtc = player.OfflineBattleEndAtUtc
                    ?? summary.EndAtUtc
                    ?? player.OfflineBattleStartedAtUtc?.AddHours(48),
                TotalBattles = summary.TotalBattles,
                WinBattles = summary.WinBattles,
                TotalRounds = summary.TotalRounds
            };
        }

        private static OfflineBattleSummaryDto BuildOfflineBattleSummary(UserEntity player, OfflineBattleSummaryDto summary, DateTime stoppedAtUtc)
        {
            var startedAtUtc = player.OfflineBattleStartedAtUtc ?? summary.StartedAtUtc ?? stoppedAtUtc;
            return new OfflineBattleSummaryDto
            {
                MapId = string.IsNullOrWhiteSpace(player.OfflineBattleMapId) ? summary.MapId : player.OfflineBattleMapId ?? string.Empty,
                MapName = string.IsNullOrWhiteSpace(summary.MapName)
                    ? ResolveBattleMapName(player.OfflineBattleMapId ?? summary.MapId)
                    : summary.MapName,
                StartedAtUtc = startedAtUtc,
                StoppedAtUtc = stoppedAtUtc,
                DurationSeconds = Math.Max(0, (int)Math.Round((stoppedAtUtc - startedAtUtc).TotalSeconds)),
                TotalBattles = summary.TotalBattles,
                WinBattles = summary.WinBattles,
                TotalRounds = summary.TotalRounds,
                ExpGained = summary.ExpGained,
                GoldGained = summary.GoldGained,
                ItemDrops = summary.ItemDrops.ToList(),
                EquipmentDrops = summary.EquipmentDrops.ToList()
            };
        }

        private static string ResolveBattleMapName(string? mapId)
        {
            if (!string.IsNullOrWhiteSpace(mapId) &&
                global::XXX.GameData.Maps.TryGetValue(mapId, out var map) &&
                map != null)
            {
                return map.Name ?? map.MapGId ?? string.Empty;
            }

            return string.Empty;
        }

        private static OfflineBattleSummaryDto DeserializeOfflineBattleSummary(UserEntity player)
        {
            if (!string.IsNullOrWhiteSpace(player.OfflineBattleSummaryJson))
            {
                try
                {
                    var summary = JsonSerializer.Deserialize<OfflineBattleSummaryDto>(player.OfflineBattleSummaryJson, OfflineBattleJsonOptions);
                    if (summary != null)
                    {
                        summary.MapId = string.IsNullOrWhiteSpace(summary.MapId) ? player.OfflineBattleMapId ?? string.Empty : summary.MapId;
                        summary.MapName = string.IsNullOrWhiteSpace(summary.MapName)
                            ? ResolveBattleMapName(player.OfflineBattleMapId)
                            : summary.MapName;
                        summary.StartedAtUtc ??= player.OfflineBattleStartedAtUtc;
                        return summary;
                    }
                }
                catch
                {
                    // 中文注释：
                    // 老数据或异常 JSON 不应阻塞玩家停止离线挂机；
                    // 回退到"从玩家表主字段恢复一个空汇总"即可。
                }
            }

            return new OfflineBattleSummaryDto
            {
                MapId = player.OfflineBattleMapId ?? string.Empty,
                MapName = ResolveBattleMapName(player.OfflineBattleMapId),
                StartedAtUtc = player.OfflineBattleStartedAtUtc
            };
        }

        private static string SerializeOfflineBattleSummary(OfflineBattleSummaryDto summary)
        {
            return JsonSerializer.Serialize(summary, OfflineBattleJsonOptions);
        }

        private static void MergeOfflineBattleSummary(OfflineBattleSummaryDto summary, BattleResult coreResult)
        {
            summary.TotalBattles += 1;
            if (coreResult.IsVictory)
            {
                summary.WinBattles += 1;
                summary.ExpGained += coreResult.ExpGained;
                summary.GoldGained += coreResult.GoldGained;
                MergeDropList(summary.ItemDrops, MapItemDrops(coreResult));
                MergeDropList(summary.EquipmentDrops, MapEquipmentDrops(coreResult));
                MergeDropList(summary.CollectionDrops, MapCollectionDrops(coreResult));
            }

            summary.TotalRounds += coreResult.TotalRounds;
        }

        private static void MergeDropList(List<BattleDropDto> target, IEnumerable<BattleDropDto> additions)
        {
            foreach (var addition in additions.Where(item => item != null))
            {
                var existing = target.FirstOrDefault(item => string.Equals(item.ItemId, addition.ItemId, StringComparison.OrdinalIgnoreCase));
                if (existing == null)
                {
                    target.Add(new BattleDropDto
                    {
                        ItemId = addition.ItemId,
                        Name = addition.Name,
                        Quantity = addition.Quantity,
                        Quality = addition.Quality
                    });
                    continue;
                }

                existing.Quantity += addition.Quantity;
                if (string.IsNullOrWhiteSpace(existing.Name))
                {
                    existing.Name = addition.Name;
                }

                if (existing.Quality <= 0)
                {
                    existing.Quality = addition.Quality;
                }
            }
        }

        private BattleResultDto? GetBattleCooldownRejectedResult(UserEntity player, string? customPrefix = null)
        {
            var remainingSeconds = GetBattleCooldownRemainingSeconds(player);
            if (remainingSeconds <= 0)
            {
                return null;
            }

            var prefix = string.IsNullOrWhiteSpace(customPrefix) ? "战斗冷却中" : customPrefix;
            return new BattleResultDto
            {
                Success = false,
                IsWin = false,
                IsVictory = false,
                Message = $"{prefix}，还需等待 {remainingSeconds} 秒。",
                BattleCooldownSeconds = remainingSeconds,
                BattleCooldownUntilUtc = player.BattleCooldownUntilUtc,
                BattleLog =
                [
                    $"{prefix}，还需等待 {remainingSeconds} 秒。"
                ]
            };
        }

        private BattleResultDto? GetOfflineBattleRejectedResult(UserEntity player, string? customPrefix = null)
        {
            if (player.BattleMode != BattleMode.OfflineAuto)
            {
                return null;
            }

            var prefix = string.IsNullOrWhiteSpace(customPrefix)
                ? "当前账号正在离线挂机中，请先停止离线挂机"
                : customPrefix;
            return new BattleResultDto
            {
                Success = false,
                IsWin = false,
                IsVictory = false,
                Message = $"{prefix}。",
                BattleLog =
                [
                    $"{prefix}。"
                ]
            };
        }

        private BattleResultDto? GetTeamOfflineBattleRejectedResult(IEnumerable<UserEntity> players)
        {
            var blockedPlayer = players.FirstOrDefault(player => player.BattleMode == BattleMode.OfflineAuto);
            if (blockedPlayer == null)
            {
                return null;
            }

            return new BattleResultDto
            {
                Success = false,
                IsWin = false,
                IsVictory = false,
                Message = $"队伍成员 {blockedPlayer.Name} 正在离线挂机中，请先停止离线挂机。",
                BattleLog =
                [
                    $"队伍成员 {blockedPlayer.Name} 正在离线挂机中，请先停止离线挂机。"
                ]
            };
        }

        private BattleResultDto? GetTeamBattleCooldownRejectedResult(IEnumerable<UserEntity> players)
        {
            var blockedPlayer = players
                .Select(player => new
                {
                    Player = player,
                    RemainingSeconds = GetBattleCooldownRemainingSeconds(player)
                })
                .Where(item => item.RemainingSeconds > 0)
                .OrderByDescending(item => item.RemainingSeconds)
                .FirstOrDefault();

            if (blockedPlayer == null)
            {
                return null;
            }

            return new BattleResultDto
            {
                Success = false,
                IsWin = false,
                IsVictory = false,
                Message = $"队伍成员 {blockedPlayer.Player.Name} 处于战斗冷却中，还需等待 {blockedPlayer.RemainingSeconds} 秒。",
                BattleCooldownSeconds = blockedPlayer.RemainingSeconds,
                BattleCooldownUntilUtc = blockedPlayer.Player.BattleCooldownUntilUtc,
                BattleLog =
                [
                    $"队伍成员 {blockedPlayer.Player.Name} 处于战斗冷却中，还需等待 {blockedPlayer.RemainingSeconds} 秒。"
                ]
            };
        }

        private static int GetBattleCooldownDurationSeconds(int totalRounds)
        {
            return BaseBattleCooldownSeconds + Math.Max(0, totalRounds);
        }

        private static int GetBattleCooldownRemainingSeconds(UserEntity player)
        {
            if (!player.BattleCooldownUntilUtc.HasValue)
            {
                return 0;
            }

            var remaining = player.BattleCooldownUntilUtc.Value - DateTime.UtcNow;
            if (remaining <= TimeSpan.Zero)
            {
                return 0;
            }

            return (int)Math.Ceiling(remaining.TotalSeconds);
        }

        private async Task ApplyBattleCooldownAsync(IEnumerable<UserEntity> players, int totalRounds, DateTime? cooldownBaseUtc = null)
        {
            var cooldownUntilUtc = (cooldownBaseUtc ?? DateTime.UtcNow).AddSeconds(GetBattleCooldownDurationSeconds(totalRounds));
            foreach (var player in players.Where(player => player != null))
            {
                player.BattleCooldownUntilUtc = cooldownUntilUtc;
                player.LastUpdateTime = DateTime.Now;
                await _userRepository.Db.Updateable<UserEntity>()
                    .SetColumns(user => user.BattleCooldownUntilUtc == cooldownUntilUtc)
                    .SetColumns(user => user.LastUpdateTime == player.LastUpdateTime)
                    .Where(user => user.GID == player.GID && !user.IsDeleted)
                    .ExecuteCommandAsync();
            }
        }

        private static void PopulateBattleCooldown(BattleResultDto result, UserEntity player, int totalRounds)
        {
            result.BattleCooldownSeconds = GetBattleCooldownDurationSeconds(totalRounds);
            result.BattleCooldownUntilUtc = player.BattleCooldownUntilUtc;
        }

        /// <summary>
        /// 确保战斗模板数据仅初始化一次
        /// </summary>
        private async Task<IReadOnlyDictionary<string, PetEntity>> HydrateActivePetsForBattleAsync(IEnumerable<UserEntity> players)
        {
            var activePetIds = players
                .Select(player => player.PetId)
                .Where(petId => !string.IsNullOrWhiteSpace(petId))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (activePetIds.Count == 0)
            {
                return new Dictionary<string, PetEntity>(StringComparer.OrdinalIgnoreCase);
            }

            var petInstances = await _petRepository.GetListAsync(pet => activePetIds.Contains(pet.InstanceId));
            var pets = new Dictionary<string, PetEntity>(StringComparer.OrdinalIgnoreCase);
            foreach (var pet in petInstances)
            {
                var petEntity = new PetEntity
                {
                    GID = pet.InstanceId,
                    Name = pet.Name,
                    Level = pet.Level.ToString(),
                    Type1 = pet.Type1,
                    Type2 = pet.Type2,
                    Type3 = pet.Type3,
                    Type4 = pet.Type4,
                    Type5 = pet.Type5,
                    Type6 = pet.Type6,
                    Type7 = pet.Type7,
                    Type8 = pet.Type8,
                    Type9 = pet.Type9,
                    Type10 = pet.Type10,
                    Type11 = pet.Type11,
                    Type12 = pet.Type12,
                    Type13 = pet.Type13,
                    Type14 = pet.Type14,
                    Type15 = pet.Type15,
                    Element = pet.Element
                };

                petEntity.SetSkillIds(pet.SkillIds ?? []);
                pets[pet.InstanceId] = petEntity;
            }

            return pets;
        }

        /// <summary>
        /// 中文注释：
        /// 宠物现在是独立成长线，战斗后只给当前出战宠物发经验，不再给人物面板叠加属性。
        /// 这里把换算比例固定在战斗奖励经验的 20%，同时保底 5 点，避免前期战斗宠物完全不成长。
        /// </summary>
        private async Task GrantPetBattleExpAsync(string playerId, long expGained)
        {
            if (expGained <= 0)
            {
                return;
            }

            var petExp = Math.Max(5, (int)Math.Round(expGained * 0.2d, MidpointRounding.AwayFromZero));
            await _petService.GrantBattleExpAsync(playerId, petExp);
        }

        private static void EnsureGameDataInitialized()
        {
            if (global::XXX.GameData.MonsterTemplates.Count > 0 &&
                global::XXX.GameData.EquipmentTemplates.Count > 0 &&
                global::XXX.GameData.Maps.Count > 0 &&
                global::XXX.GameData.Items.Count > 0 &&
                global::XXX.SkillData.HasLoadedRuntimeTemplates &&
                global::XXX.BuffDataTemplates.HasLoadedRuntimeTemplates &&
                DungeonCatalog.HasLoadedRuntimeEntries)
            {
                return;
            }

            throw new InvalidOperationException("Runtime template cache has not been loaded from database.");
        }

        /// <summary>
        /// 解析 PVE 战斗地图
        /// 优先级：请求 MapId > 按玩家等级推荐 > 默认 map_001
        /// </summary>
        private static string? ResolveMapId(UserEntity player, BattleRequestDto request)
        {
            if (!string.IsNullOrWhiteSpace(request.MapId) &&
                global::XXX.GameData.Maps.TryGetValue(request.MapId, out var requestedMap))
            {
                // 中文注释：
                // 普通战斗接口只接受普通地图。
                // 若客户端把某个副本层地图直接塞到这里，应交给上层按"非法入口"处理，
                // 避免绕过副本次数与组队校验。
                return IsNormalBattleMap(requestedMap) ? request.MapId : null;
            }

            var mapIdByLevel = global::XXX.GameData.Maps.Values
                .Where(IsNormalBattleMap)
                .Where(map => map.Level <= player.Level)
                .OrderByDescending(map => map.Level)
                .Select(map => map.MapGId)
                .FirstOrDefault();

            return string.IsNullOrWhiteSpace(mapIdByLevel) ? "map_001" : mapIdByLevel;
        }

        /// <summary>
        /// 判断是否为普通地图
        /// </summary>
        private static bool IsNormalBattleMap(Map map)
        {
            return map != null &&
                   !string.IsNullOrWhiteSpace(map.MapGId) &&
                   map.MapGId.StartsWith("map_", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 解析副本链起始地图（优先 fuben_xxx）
        /// 统一从副本配置读取起点，避免硬编码分散导致映射错误
        /// </summary>
        private static string? ResolveDungeonFubenMapId(string? dungeonId)
        {
            var definition = GetDungeonDefinition(dungeonId);
            if (definition != null &&
                !string.IsNullOrWhiteSpace(definition.FubenMapId) &&
                global::XXX.GameData.Maps.ContainsKey(definition.FubenMapId))
            {
                return definition.FubenMapId;
            }

            // 中文注释：
            // 副本入口必须显式命中 FubenMapId。
            // 若这里悄悄回退到 NormalMapId，会把配置错误伪装成"普通地图单层战斗"，
            // 最终出现副本次数被扣了、但实际没走多层副本链的脏状态。
            return null;
        }

        /// <summary>
        /// 校验副本地图链，防止空起点、断链、循环、普通地图混入和超过最大层数。
        /// </summary>
        private static (bool IsValid, string ErrorMessage) ValidateDungeonMapChain(string dungeonId, string? startMapId)
        {
            const int maxStages = 20;
            if (string.IsNullOrWhiteSpace(startMapId) || !global::XXX.GameData.Maps.TryGetValue(startMapId, out Map? current))
            {
                return (false, $"副本 {dungeonId} 的起点地图不存在。");
            }

            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var stage = 0;
            while (current != null)
            {
                stage++;
                if (stage > maxStages)
                {
                    return (false, $"副本 {dungeonId} 地图链超过最大层数 {maxStages}。");
                }

                if (!current.MapGId.StartsWith("fuben_", StringComparison.OrdinalIgnoreCase))
                {
                    return (false, $"副本 {dungeonId} 地图链包含普通地图 {current.MapGId}。");
                }

                if (!visited.Add(current.MapGId))
                {
                    return (false, $"副本 {dungeonId} 地图链存在循环：{current.MapGId}。");
                }

                if (string.IsNullOrWhiteSpace(current.NextMapId))
                {
                    return (true, string.Empty);
                }

                var nextMapId = current.NextMapId!;
                if (!global::XXX.GameData.Maps.TryGetValue(nextMapId, out current))
                {
                    return (false, $"副本 {dungeonId} 地图链断裂，下一张地图不存在：{nextMapId}。");
                }
            }

            return (false, $"副本 {dungeonId} 地图链配置无效。");
        }
        /// <summary>
        /// 获取副本静态定义。
        /// </summary>
        private static DungeonCatalogEntry? GetDungeonDefinition(string? dungeonId)
        {
            if (string.IsNullOrWhiteSpace(dungeonId))
            {
                return null;
            }

            return DungeonCatalog.Get(dungeonId);
        }

        /// <summary>
        /// 生成副本不可挑战原因
        /// </summary>
        /// <remarks>
        /// 方法作用：把"等级不足"和"奖励次数用尽"这两类最常见的状态统一收口成可直接给前端展示的文案。
        /// 关键逻辑：奖励次数用尽不再阻止进入副本，只有等级不足会让副本显示为不可用。
        /// </remarks>
        private static string? BuildDungeonUnavailableReason(int playerLevel, DungeonCatalogEntry definition, int todayCount)
        {
            if (playerLevel < definition.RecommendedLevel)
            {
                return $"需要达到 Lv.{definition.RecommendedLevel} 后才能挑战。";
            }

            if (todayCount >= definition.DailyLimit)
            {
                return $"今日奖励次数已用尽（{todayCount}/{definition.DailyLimit}），仍可参战但本次不获得副本奖励。";
            }

            if (definition.RequiredTeamSize > 1)
            {
                return $"该副本需要 {definition.RequiredTeamSize} 人临时队伍挑战。";
            }

            return null;
        }

        /// <summary>
        /// 生成普通地图不可进入原因
        /// </summary>
        private static string? BuildBattleMapUnavailableReason(int playerLevel, int mapLevel)
        {
            if (playerLevel < mapLevel)
            {
                return $"需要达到 Lv.{mapLevel} 后才能前往该地图。";
            }

            return null;
        }

        /// <summary>
        /// 获取玩家在指定副本的当日挑战次数
        /// </summary>
        private async Task<int> GetTodayDungeonChallengeCountAsync(string playerId, string dungeonId, DateTime recordDate)
        {
            var record = (await _dungeonDailyRecordRepository.Db.Queryable<DungeonDailyRecordEntity>()
                .Where(r => r.PlayerId == playerId && r.DungeonId == dungeonId)
                .ToListAsync())
                .Where(r => r.RecordDate.Date == recordDate.Date)
                .OrderByDescending(r => r.LastUpdateTime)
                .FirstOrDefault();

            return record?.ChallengeCount ?? 0;
        }

        /// <summary>
        /// 尝试扣减一次副本挑战次数
        /// 返回 true 表示扣减成功，false 表示已达上限
        /// </summary>
        private async Task<bool> TryConsumeDungeonChallengeAsync(string playerId, string dungeonId, int dailyLimit, DateTime recordDate)
        {
            return await TryConsumeDungeonChallengeAsync([playerId], dungeonId, dailyLimit, recordDate);
        }

        /// <summary>
        /// 中文注释：
        /// 组队副本必须保证所有成员"要么一起扣次数成功，要么一起失败"。
        /// 这里统一使用一个事务批量检查并写回每日次数，避免出现部分成员已扣、部分成员未扣的脏状态。
        /// </summary>
        private async Task<bool> TryConsumeDungeonChallengeAsync(IEnumerable<string> playerIds, string dungeonId, int dailyLimit, DateTime recordDate)
        {
            if (dailyLimit <= 0)
            {
                return true;
            }

            var distinctPlayerIds = playerIds
                .Where(playerId => !string.IsNullOrWhiteSpace(playerId))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (distinctPlayerIds.Count == 0)
            {
                return false;
            }

            // 进程锁 + 数据库事务：保证"读取-判断-写入"原子性。
            await DungeonChallengeLock.WaitAsync();
            try
            {
                var db = _dungeonDailyRecordRepository.Db;
                try
                {
                    db.Ado.BeginTran();
                    var records = (await db.Queryable<DungeonDailyRecordEntity>()
                        .Where(r => distinctPlayerIds.Contains(r.PlayerId) && r.DungeonId == dungeonId)
                        .ToListAsync())
                        .Where(r => r.RecordDate.Date == recordDate.Date)
                        .GroupBy(r => r.PlayerId, StringComparer.OrdinalIgnoreCase)
                        .Select(group =>
                        {
                            var ordered = group.OrderByDescending(r => r.LastUpdateTime).ToList();
                            var primary = ordered[0];
                            // 中文注释：历史版本可能因日期带不带时间产生重复记录；事务内合并为一条，避免次数查询和扣减分叉。
                            foreach (var duplicate in ordered.Skip(1))
                            {
                                primary.ChallengeCount += duplicate.ChallengeCount;
                                db.Deleteable<DungeonDailyRecordEntity>()
                                    .Where(r => r.GID == duplicate.GID)
                                    .ExecuteCommand();
                            }
                            return primary;
                        })
                        .ToList();

                    foreach (var playerId in distinctPlayerIds)
                    {
                        var record = records.FirstOrDefault(r => string.Equals(r.PlayerId, playerId, StringComparison.OrdinalIgnoreCase));
                        if (record == null)
                        {
                            continue;
                        }

                        if (record.ChallengeCount >= dailyLimit)
                        {
                            db.Ado.RollbackTran();
                            return false;
                        }
                    }

                    foreach (var playerId in distinctPlayerIds)
                    {
                        var record = records.FirstOrDefault(r => string.Equals(r.PlayerId, playerId, StringComparison.OrdinalIgnoreCase));
                        if (record == null)
                        {
                            record = new DungeonDailyRecordEntity
                            {
                                GID = Guid.NewGuid().ToString(),
                                PlayerId = playerId,
                                DungeonId = dungeonId,
                                ChallengeCount = 1,
                                RecordDate = recordDate,
                                LastUpdateTime = DateTime.Now
                            };

                            await db.Insertable(record).ExecuteCommandAsync();
                            continue;
                        }

                        record.ChallengeCount += 1;
                        record.LastUpdateTime = DateTime.Now;
                        await db.Updateable(record).ExecuteCommandAsync();
                    }

                    db.Ado.CommitTran();
                    return true;
                }
                catch
                {
                    db.Ado.RollbackTran();
                    throw;
                }
            }
            finally
            {
                DungeonChallengeLock.Release();
            }
        }

        /// <summary>
        /// 回写单玩家战斗统计
        /// TotalBattles 每场 +1；胜利时 WinBattles +1；击杀数按战斗结果累计
        /// </summary>
        /// <remarks>
        /// 方法作用：在不读取整行玩家数据的情况下，原子增加战斗统计字段。
        /// 关键逻辑：使用数据库增量更新，只改 TotalBattles/WinBattles/TotalKills/LastUpdateTime，避免覆盖前序奖励结算写入。
        /// </remarks>
        private async Task UpdateBattleStatsAsync(string playerId, bool isWin, int killCount)
        {
            var now = DateTime.Now;
            await _userRepository.Db.Updateable<UserEntity>()
                .SetColumns(u => u.TotalBattles == u.TotalBattles + 1)
                .SetColumnsIF(isWin, u => u.WinBattles == u.WinBattles + 1)
                .SetColumnsIF(killCount > 0, u => u.TotalKills == u.TotalKills + killCount)
                .SetColumns(u => u.LastUpdateTime == now)
                .Where(u => u.GID == playerId && !u.IsDeleted)
                .ExecuteCommandAsync();
        }

        private async Task UpdateBattleStatsAsync(IEnumerable<string> playerIds, bool isWin, IReadOnlyDictionary<string, int>? killCounts = null)
        {
            foreach (var playerId in playerIds
                .Where(playerId => !string.IsNullOrWhiteSpace(playerId))
                .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var currentKillCount = 0;
                if (killCounts != null && killCounts.TryGetValue(playerId, out var syncedKillCount))
                {
                    currentKillCount = syncedKillCount;
                }

                await UpdateBattleStatsAsync(playerId, isWin, currentKillCount);
            }
        }

        /// <summary>
        /// 回写 PVP 双方战斗统计
        /// 双方总场次 +1，胜方胜场 +1
        /// </summary>
        /// <remarks>
        /// 方法作用：同时更新挑战方与被挑战方的 PVP 战绩，保证双方数据一致。
        /// 关键逻辑：在同一事务中分别做两次原子增量更新，任一更新失败即整体回滚，避免出现单边写入。
        /// </remarks>
        private async Task UpdatePvpBattleStatsAsync(string challengerId, string opponentId, bool challengerWin, int challengerKillCount, int opponentKillCount)
        {
            var db = _userRepository.Db;
            var now = DateTime.Now;
            try
            {
                // 关键逻辑：PVP 双方战绩写回必须同事务提交，防止只更新一方造成脏数据。
                db.Ado.BeginTran();

                var challengerRows = await db.Updateable<UserEntity>()
                    .SetColumns(u => u.TotalBattles == u.TotalBattles + 1)
                    .SetColumnsIF(challengerWin, u => u.WinBattles == u.WinBattles + 1)
                    .SetColumnsIF(challengerKillCount > 0, u => u.TotalKills == u.TotalKills + challengerKillCount)
                    .SetColumns(u => u.LastUpdateTime == now)
                    .Where(u => u.GID == challengerId && !u.IsDeleted)
                    .ExecuteCommandAsync();

                if (challengerRows == 0)
                {
                    db.Ado.RollbackTran();
                    return;
                }

                var opponentRows = await db.Updateable<UserEntity>()
                    .SetColumns(u => u.TotalBattles == u.TotalBattles + 1)
                    .SetColumnsIF(!challengerWin, u => u.WinBattles == u.WinBattles + 1)
                    .SetColumnsIF(opponentKillCount > 0, u => u.TotalKills == u.TotalKills + opponentKillCount)
                    .SetColumns(u => u.LastUpdateTime == now)
                    .Where(u => u.GID == opponentId && !u.IsDeleted)
                    .ExecuteCommandAsync();

                if (opponentRows == 0)
                {
                    db.Ado.RollbackTran();
                    return;
                }

                db.Ado.CommitTran();
            }
            catch
            {
                db.Ado.RollbackTran();
                throw;
            }

        }

        /// <summary>
        /// 处理战斗任务/成就进度汇总事件。
        /// </summary>
        /// <remarks>
        /// 中文注释：
        /// 后台 Worker 将事件中的统计恢复为最小 BattleResult，复用现有任务和成就同步逻辑，
        /// 保证普通战斗从请求线程移出后，进度计算规则仍与原实现一致。
        /// </remarks>
        public async Task ProcessBattleProgressEventAsync(BattleProgressEvent progressEvent)
        {
            if (progressEvent == null || string.IsNullOrWhiteSpace(progressEvent.PlayerId))
            {
                return;
            }

            var battleResult = new BattleResult
            {
                IsVictory = progressEvent.IsVictory,
                RoundLogs = [],
                FighterSkillStats = new Dictionary<string, FighterSkillStats>
                {
                    [progressEvent.PlayerId] = new FighterSkillStats
                    {
                        GID = progressEvent.PlayerId,
                        TotalDamageDealt = progressEvent.TotalDamageDealt,
                        TotalDamageTaken = progressEvent.TotalDamageTaken,
                        Kills = progressEvent.Kills,
                        CritCount = progressEvent.CritCount,
                        DodgeCount = progressEvent.DodgeCount,
                        TotalMpConsumed = progressEvent.TotalMpConsumed,
                        SkillUsageCount = new Dictionary<int, int>(progressEvent.SkillUsageBySkill),
                        SkillDamageDealt = new Dictionary<int, int>(progressEvent.SkillDamageBySkill),
                        KilledMonsterTemplateIds = progressEvent.KilledMonsterTemplateIds.ToList()
                    }
                }
            };

            await SyncBattleQuestsAsync(
                progressEvent.PlayerId,
                new[] { battleResult },
                progressEvent.IsVictory,
                progressEvent.DungeonId,
                progressEvent.MapId);
            await SyncBattleAchievementsAsync(
                progressEvent.PlayerId,
                new[] { battleResult },
                progressEvent.IsVictory,
                progressEvent.DungeonId);
        }

        private async Task SyncBattleQuestsAsync(string playerId, IEnumerable<BattleResult> battleResults, bool isWin, string? dungeonId = null, string? mapId = null)
        {
            try
            {
                var player = await _userRepository.GetByIdAsync(playerId);
                if (player == null || player.IsDeleted)
                {
                    return;
                }

                var battleResultList = battleResults?
                    .Where(result => result != null)
                    .ToList() ?? [];
                if (battleResultList.Count == 0)
                {
                    return;
                }

                var playerStats = AggregatePlayerBattleStats(battleResultList, playerId);

                foreach (var defeatedMonsterId in playerStats.KilledMonsterTemplateIds)
                {
                    await _questService.RecordObjectiveEventAsync(playerId, new QuestObjectiveEvent
                    {
                        ObjectiveType = ObjectiveType.KillMonster,
                        TargetId = defeatedMonsterId,
                        Delta = 1
                    });
                }

                if (!string.IsNullOrWhiteSpace(mapId))
                {
                    await _questService.RecordObjectiveEventAsync(playerId, new QuestObjectiveEvent
                    {
                        ObjectiveType = ObjectiveType.MapBattleCount,
                        TargetId = mapId,
                        Delta = 1
                    });

                    if (isWin)
                    {
                        await _questService.RecordObjectiveEventAsync(playerId, new QuestObjectiveEvent
                        {
                            ObjectiveType = ObjectiveType.MapWinCount,
                            TargetId = mapId,
                            Delta = 1
                        });
                    }
                }

                if (isWin && !string.IsNullOrWhiteSpace(dungeonId))
                {
                    await _questService.RecordObjectiveEventAsync(playerId, new QuestObjectiveEvent
                    {
                        ObjectiveType = ObjectiveType.CompleteDungeon,
                        TargetId = dungeonId,
                        Delta = 1
                    });
                }

                if (playerStats.TotalDamageDealt > 0)
                {
                    await _questService.RecordObjectiveEventAsync(playerId, new QuestObjectiveEvent
                    {
                        ObjectiveType = ObjectiveType.BattleActivity,
                        ActivityType = BattleActivityType.DealDamage,
                        Delta = playerStats.TotalDamageDealt
                    });
                }

                if (playerStats.TotalDamageTaken > 0)
                {
                    await _questService.RecordObjectiveEventAsync(playerId, new QuestObjectiveEvent
                    {
                        ObjectiveType = ObjectiveType.BattleActivity,
                        ActivityType = BattleActivityType.TakeDamage,
                        Delta = playerStats.TotalDamageTaken
                    });
                }

                if (playerStats.Kills > 0)
                {
                    await _questService.RecordObjectiveEventAsync(playerId, new QuestObjectiveEvent
                    {
                        ObjectiveType = ObjectiveType.BattleActivity,
                        ActivityType = BattleActivityType.KillCount,
                        Delta = playerStats.Kills
                    });
                }

                if (isWin)
                {
                    await _questService.RecordObjectiveEventAsync(playerId, new QuestObjectiveEvent
                    {
                        ObjectiveType = ObjectiveType.BattleActivity,
                        ActivityType = BattleActivityType.WinCount,
                        Delta = 1
                    });

                    // 更新宗门任务：战斗胜利次数
                    await _questService.RecordObjectiveEventAsync(playerId, new QuestObjectiveEvent
                    {
                        ObjectiveType = ObjectiveType.WinBattle,
                        Delta = 1
                    });
                }

                if (playerStats.SkillUseCount > 0)
                {
                    await _questService.RecordObjectiveEventAsync(playerId, new QuestObjectiveEvent
                    {
                        ObjectiveType = ObjectiveType.BattleActivity,
                        ActivityType = BattleActivityType.UseSkill,
                        Delta = playerStats.SkillUseCount
                    });
                }

                if (playerStats.CritCount > 0)
                {
                    await _questService.RecordObjectiveEventAsync(playerId, new QuestObjectiveEvent
                    {
                        ObjectiveType = ObjectiveType.BattleActivity,
                        ActivityType = BattleActivityType.CriticalHit,
                        Delta = playerStats.CritCount
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to sync battle quests for player {PlayerId}", playerId);
            }
        }

        private async Task SyncBattleAchievementsAsync(string playerId, IEnumerable<BattleResult> battleResults, bool isWin, string? dungeonId = null)
        {
            try
            {
                var player = await _userRepository.GetByIdAsync(playerId);
                if (player == null || player.IsDeleted)
                {
                    return;
                }

                var battleResultList = battleResults?
                    .Where(result => result != null)
                    .ToList() ?? [];
                if (battleResultList.Count == 0)
                {
                    return;
                }

                var playerStats = AggregatePlayerBattleStats(battleResultList, playerId);

                await _achievementService.SyncRequirementStateAsync(player.GID, AchievementRequirementType.TotalWins);
                await _achievementService.SyncRequirementStateAsync(player.GID, AchievementRequirementType.TotalKills);

                if (playerStats.TotalDamageDealt > 0)
                {
                    await _achievementService.RecordRequirementEventAsync(player.GID, new AchievementRequirementEvent
                    {
                        RequirementType = AchievementRequirementType.TotalDamageDealt,
                        Delta = playerStats.TotalDamageDealt
                    });
                }

                if (playerStats.TotalDamageTaken > 0)
                {
                    await _achievementService.RecordRequirementEventAsync(player.GID, new AchievementRequirementEvent
                    {
                        RequirementType = AchievementRequirementType.TotalDamageTaken,
                        Delta = playerStats.TotalDamageTaken
                    });
                }

                if (playerStats.TotalMpConsumed > 0)
                {
                    await _achievementService.RecordRequirementEventAsync(player.GID, new AchievementRequirementEvent
                    {
                        RequirementType = AchievementRequirementType.TotalMpConsumed,
                        Delta = playerStats.TotalMpConsumed
                    });
                }

                if (playerStats.CritCount > 0)
                {
                    await _achievementService.RecordRequirementEventAsync(player.GID, new AchievementRequirementEvent
                    {
                        RequirementType = AchievementRequirementType.CriticalHitCount,
                        Delta = playerStats.CritCount
                    });
                }

                if (playerStats.DeathCount > 0)
                {
                    await _achievementService.RecordRequirementEventAsync(player.GID, new AchievementRequirementEvent
                    {
                        RequirementType = AchievementRequirementType.DeathCount,
                        Delta = playerStats.DeathCount
                    });
                }

                if (playerStats.DodgeCount > 0)
                {
                    await _achievementService.RecordRequirementEventAsync(player.GID, new AchievementRequirementEvent
                    {
                        RequirementType = AchievementRequirementType.DodgeCount,
                        Delta = playerStats.DodgeCount
                    });
                }

                foreach (var monsterId in playerStats.KilledMonsterTemplateIds)
                {
                    await _achievementService.RecordRequirementEventAsync(player.GID, new AchievementRequirementEvent
                    {
                        RequirementType = AchievementRequirementType.MonsterKillCount,
                        TargetId = monsterId,
                        Delta = 1
                    });
                }

                foreach (var skillUsage in playerStats.SkillUsageBySkill)
                {
                    await _achievementService.RecordRequirementEventAsync(player.GID, new AchievementRequirementEvent
                    {
                        RequirementType = AchievementRequirementType.SkillUseCount,
                        TargetId = skillUsage.Key.ToString(),
                        Delta = skillUsage.Value
                    });
                }

                foreach (var skillDamage in playerStats.SkillDamageBySkill)
                {
                    await _achievementService.RecordRequirementEventAsync(player.GID, new AchievementRequirementEvent
                    {
                        RequirementType = AchievementRequirementType.SkillDamageDealt,
                        TargetId = skillDamage.Key.ToString(),
                        Delta = skillDamage.Value
                    });
                }

                if (isWin && !string.IsNullOrWhiteSpace(dungeonId))
                {
                    await _achievementService.RecordRequirementEventAsync(player.GID, new AchievementRequirementEvent
                    {
                        RequirementType = AchievementRequirementType.DungeonWinCount,
                        TargetId = dungeonId,
                        Delta = 1
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to sync battle achievements for player {PlayerId}", playerId);
            }
        }

        private static int GetObjectiveProgressDelta(
            QuestObjective objective,
            PlayerBattleStats playerStats,
            IReadOnlyCollection<string> defeatedMonsterNames,
            bool isWin,
            string? dungeonId)
        {
            return objective.ObjectiveType switch
            {
                ObjectiveType.BattleActivity => GetBattleActivityDelta(objective, playerStats, isWin),
                ObjectiveType.KillMonster => defeatedMonsterNames.Count(name => MonsterNameMatchesObjective(name, objective)),
                ObjectiveType.CompleteDungeon => isWin &&
                    !string.IsNullOrWhiteSpace(dungeonId) &&
                    string.Equals(objective.TargetId, dungeonId, StringComparison.OrdinalIgnoreCase)
                        ? 1
                        : 0,
                _ => 0
            };
        }

        private static int GetBattleActivityDelta(QuestObjective objective, PlayerBattleStats playerStats, bool isWin)
        {
            return objective.ActivityType switch
            {
                BattleActivityType.DealDamage => playerStats.TotalDamageDealt,
                BattleActivityType.TakeDamage => playerStats.TotalDamageTaken,
                BattleActivityType.KillCount => playerStats.Kills,
                BattleActivityType.WinCount => isWin ? 1 : 0,
                BattleActivityType.UseSkill => playerStats.SkillUseCount,
                BattleActivityType.CriticalHit => playerStats.CritCount,
                _ => 0
            };
        }

        private static PlayerBattleStats AggregatePlayerBattleStats(IEnumerable<BattleResult> battleResults, string playerId)
        {
            var aggregate = new PlayerBattleStats();

            foreach (var stats in battleResults
                .Where(result => result?.FighterSkillStats != null)
                .SelectMany(result => result.FighterSkillStats.Values)
                .Where(stats =>
                    stats != null &&
                    string.Equals(stats.GID, playerId, StringComparison.OrdinalIgnoreCase)))
            {
                aggregate.TotalDamageDealt += stats.TotalDamageDealt;
                aggregate.TotalDamageTaken += stats.TotalDamageTaken;
                aggregate.Kills += stats.Kills;
                aggregate.DeathCount += stats.Deaths;
                aggregate.DodgeCount += stats.DodgeCount;
                aggregate.CritCount += stats.CritCount;
                aggregate.TotalMpConsumed += stats.TotalMpConsumed;
                aggregate.SkillUseCount += stats.SkillUsageCount?.Values.Sum() ?? 0;
                foreach (var skillUsage in stats.SkillUsageCount ?? [])
                {
                    aggregate.SkillUsageBySkill[skillUsage.Key] = aggregate.SkillUsageBySkill.TryGetValue(skillUsage.Key, out var existingUsage)
                        ? existingUsage + skillUsage.Value
                        : skillUsage.Value;
                }
                foreach (var skillDamage in stats.SkillDamageDealt ?? [])
                {
                    aggregate.SkillDamageBySkill[skillDamage.Key] = aggregate.SkillDamageBySkill.TryGetValue(skillDamage.Key, out var existingDamage)
                        ? existingDamage + skillDamage.Value
                        : skillDamage.Value;
                }
                if (stats.KilledMonsterTemplateIds != null)
                {
                    aggregate.KilledMonsterTemplateIds.AddRange(
                        stats.KilledMonsterTemplateIds
                            .Where(id => !string.IsNullOrWhiteSpace(id))
                            .Select(id => id.Trim()));
                }
            }

            return aggregate;
        }

        private static IReadOnlyCollection<string> CollectDefeatedMonsterNames(IEnumerable<BattleResult> battleResults)
        {
            if (battleResults == null)
            {
                return Array.Empty<string>();
            }

            var defeatedMonsterNames = new List<string>();

            foreach (var battleResult in battleResults.Where(result => result?.RoundLogs != null))
            {
                var defeatedMonsterNamesInBattle = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var entry in battleResult.RoundLogs
                    .Where(roundLog => roundLog?.Entries != null)
                    .SelectMany(roundLog => roundLog.Entries)
                    .Where(IsDefeatedMonsterEntry))
                {
                    var targetName = entry.TargetName.Trim();
                    if (!defeatedMonsterNamesInBattle.Add(targetName))
                    {
                        continue;
                    }

                    defeatedMonsterNames.Add(targetName);
                }
            }

            return defeatedMonsterNames;
        }

        private static bool IsDefeatedMonsterEntry(BattleLogEntry entry)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.TargetName))
            {
                return false;
            }

            if (entry.Type is BattleLogType.Death or BattleLogType.Kill or BattleLogType.Execute)
            {
                return true;
            }

            if (entry.Type is BattleLogType.DamageDealt or BattleLogType.DamageTaken or BattleLogType.CritDamage or BattleLogType.ReflectDamage)
            {
                if (entry.TargetCurrentHp.HasValue && entry.TargetCurrentHp.Value <= 0)
                {
                    return true;
                }
            }

            if (string.IsNullOrWhiteSpace(entry.Description))
            {
                return false;
            }

            return entry.Description.Contains("剩余血量 0", StringComparison.OrdinalIgnoreCase) ||
                entry.Description.Contains("remaining hp = 0", StringComparison.OrdinalIgnoreCase) ||
                entry.Description.Contains("remaining hp 0", StringComparison.OrdinalIgnoreCase) ||
                entry.Description.Contains("死亡", StringComparison.OrdinalIgnoreCase) ||
                entry.Description.Contains("斩杀", StringComparison.OrdinalIgnoreCase);
        }

        private static bool MonsterNameMatchesObjective(string defeatedMonsterName, QuestObjective objective)
        {
            if (string.IsNullOrWhiteSpace(defeatedMonsterName))
            {
                return false;
            }

            foreach (var alias in BuildMonsterObjectiveAliases(objective))
            {
                if (defeatedMonsterName.Contains(alias, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static IEnumerable<string> BuildMonsterObjectiveAliases(QuestObjective objective)
        {
            var aliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(objective.TargetId))
            {
                aliases.Add(objective.TargetId.Trim());

                if (global::XXX.GameData.MonsterTemplates.TryGetValue(objective.TargetId.Trim(), out var monsterTemplate) &&
                    !string.IsNullOrWhiteSpace(monsterTemplate.Name))
                {
                    aliases.Add(monsterTemplate.Name.Trim());
                }
            }

            if (!string.IsNullOrWhiteSpace(objective.Description))
            {
                var normalizedDescription = objective.Description
                    .Replace("击败", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Replace("击杀", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Replace("只", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Trim();

                if (!string.IsNullOrWhiteSpace(normalizedDescription))
                {
                    aliases.Add(normalizedDescription);
                }
            }

            return aliases;
        }

        private static IReadOnlyDictionary<string, int> BuildPlayerKillCounts(IEnumerable<BattleResult> stageResults, IEnumerable<string> playerIds)
        {
            var killCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var playerId in playerIds
                .Where(playerId => !string.IsNullOrWhiteSpace(playerId))
                .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                killCounts[playerId] = GetPlayerKillCount(stageResults, playerId);
            }

            return killCounts;
        }

        private static int GetPlayerKillCount(IEnumerable<BattleResult> battleResults, string playerId)
        {
            if (battleResults == null)
            {
                return 0;
            }

            return battleResults
                .Where(battleResult => battleResult != null)
                .Sum(battleResult => GetPlayerKillCount(battleResult, playerId));
        }

        private static int GetPlayerKillCount(BattleResult battleResult, string playerId)
        {
            if (battleResult?.FighterSkillStats == null || string.IsNullOrWhiteSpace(playerId))
            {
                return 0;
            }

            return battleResult.FighterSkillStats.Values
                .Where(stats => stats != null && string.Equals(stats.GID, playerId, StringComparison.OrdinalIgnoreCase))
                .Sum(stats => Math.Max(0, stats.Kills));
        }

        /// <summary>
        /// 核心战斗结果映射为 API DTO
        /// </summary>
        private static BattleResultDto MapToBattleResultDto(BattleResult coreResult)
        {
            var battleLog = FlattenBattleLog(coreResult.RoundLogs);
            if (battleLog.Count == 0)
            {
                battleLog.Add(coreResult.IsVictory ? "Battle finished: victory." : "Battle finished: defeat.");
            }

            return new BattleResultDto
            {
                IsWin = coreResult.IsVictory,
                IsVictory = coreResult.IsVictory,
                MapName = coreResult.MapName ?? string.Empty,
                Rounds = coreResult.TotalRounds,
                TotalRounds = coreResult.TotalRounds,
                ExpGained = coreResult.ExpGained,
                GoldGained = coreResult.GoldGained,
                Drops = MapDrops(coreResult),
                BattleLog = battleLog,
                RoundLogs = coreResult.RoundLogs?.ToList() ?? [],
                FighterSkillStats = coreResult.FighterSkillStats?.ToDictionary(kv => kv.Key, kv => kv.Value) ?? [],
                Stages =
                [
                    MapToBattleStageDto(coreResult, 1)
                ]
            };
        }

        /// <summary>
        /// 映射单层战斗结果
        /// </summary>
        /// <remarks>
        /// 中文注释：
        /// 这里不再把多层副本压扁成纯字符串，而是把每层自己的回合日志和状态快照独立保留下来。
        /// 这样前端就可以像老 main 一样，按"层 -> 回合 -> 条目 -> 角色状态"逐步回放，而不是只能硬拆文本。
        /// </remarks>
        private static BattleStageDto MapToBattleStageDto(BattleResult coreResult, int stageIndex)
        {
            return new BattleStageDto
            {
                StageIndex = stageIndex,
                MapName = coreResult?.MapName ?? string.Empty,
                IsVictory = coreResult?.IsVictory ?? false,
                TotalRounds = coreResult?.TotalRounds ?? 0,
                RoundLogs = coreResult?.RoundLogs?.ToList() ?? [],
                FighterSkillStats = coreResult?.FighterSkillStats?.ToDictionary(kv => kv.Key, kv => kv.Value)
                    ?? []
            };
        }

        /// <summary>
        /// 展平单场战斗日志（按回合排序）
        /// </summary>
        private static List<string> FlattenBattleLog(IEnumerable<RoundLog> roundLogs)
        {
            var logs = new List<string>();
            if (roundLogs == null)
            {
                return logs;
            }

            foreach (var round in roundLogs.OrderBy(log => log.RoundNumber))
            {
                foreach (var action in round.Actions.Where(action => !string.IsNullOrWhiteSpace(action)))
                {
                    logs.Add($"R{round.RoundNumber}: {action}");
                }
            }

            return logs;
        }

        /// <summary>
        /// 展平副本多层日志（按层、按回合）
        /// 某层失败后停止继续拼接后续层
        /// </summary>
        private static List<string> FlattenDungeonBattleLog(IEnumerable<BattleResult> stageResults)
        {
            var logs = new List<string>();
            if (stageResults == null)
            {
                return logs;
            }

            var stageIndex = 0;
            foreach (var stageResult in stageResults)
            {
                if (stageResult == null)
                {
                    continue;
                }

                stageIndex++;
                logs.Add($"S{stageIndex}: {(stageResult.IsVictory ? "Victory" : "Defeat")}");

                foreach (var round in stageResult.RoundLogs.OrderBy(log => log.RoundNumber))
                {
                    foreach (var action in round.Actions.Where(action => !string.IsNullOrWhiteSpace(action)))
                    {
                        logs.Add($"S{stageIndex}-R{round.RoundNumber}: {action}");
                    }
                }

                if (!stageResult.IsVictory)
                {
                    break;
                }
            }

            return logs;
        }

        /// <summary>
        /// 聚合副本多层结果为单结果
        /// 统计规则：回合/经验/金币/掉落累加，任一层失败则整体失败
        /// </summary>
        private static BattleResult AggregateDungeonResult(IEnumerable<BattleResult> stageResults)
        {
            var aggregate = new BattleResult { IsVictory = true };
            var hasAnyResult = false;

            foreach (var stageResult in stageResults)
            {
                if (stageResult == null)
                {
                    continue;
                }

                hasAnyResult = true;

                if (string.IsNullOrWhiteSpace(aggregate.MapName))
                {
                    aggregate.MapName = stageResult.MapName;
                }

                aggregate.TotalRounds += stageResult.TotalRounds;
                aggregate.ExpGained += stageResult.ExpGained;
                aggregate.GoldGained += stageResult.GoldGained;
                aggregate.DroppedItems.AddRange(stageResult.DroppedItems);
                aggregate.DroppedEquipments.AddRange(stageResult.DroppedEquipments);
                aggregate.DroppedCollections.AddRange(stageResult.DroppedCollections);
                foreach (var stats in stageResult.FighterSkillStats.Values)
                {
                    if (!aggregate.FighterSkillStats.TryGetValue(stats.GID, out var aggregateStats))
                    {
                        aggregateStats = new FighterSkillStats { GID = stats.GID, Name = stats.Name };
                        aggregate.FighterSkillStats[stats.GID] = aggregateStats;
                    }
                    aggregateStats.Kills += stats.Kills;
                    aggregateStats.KilledMonsterTemplateIds.AddRange(stats.KilledMonsterTemplateIds);
                }

                if (!stageResult.IsVictory)
                {
                    aggregate.IsVictory = false;
                    break;
                }
            }

            if (!hasAnyResult)
            {
                aggregate.IsVictory = false;
            }

            return aggregate;
        }

        /// <summary>
        /// 掉落映射：按 ItemId/EquipmentId 聚合数量，转换为 API 掉落格式
        /// </summary>
        private static List<BattleDropDto> MapDrops(BattleResult coreResult)
        {
            var drops = new List<BattleDropDto>();
            drops.AddRange(MapItemDrops(coreResult));
            drops.AddRange(MapEquipmentDrops(coreResult));
            drops.AddRange(MapCollectionDrops(coreResult));
            return drops;
        }

        private static List<BattleDropDto> MapItemDrops(BattleResult coreResult)
        {
            var drops = new List<BattleDropDto>();
            var groupedItemDrops = coreResult.DroppedItems
                .Where(item => item != null && !string.IsNullOrWhiteSpace(item.ItemId))
                .GroupBy(item => item.ItemId);

            foreach (var group in groupedItemDrops)
            {
                var sample = group.First();
                drops.Add(new BattleDropDto
                {
                    ItemId = sample.ItemId,
                    Name = string.IsNullOrWhiteSpace(sample.Name) ? sample.ItemId : sample.Name,
                    Quantity = group.Count(),
                    Quality = sample.Quality
                });
            }

            return drops;
        }

        private static List<BattleDropDto> MapEquipmentDrops(BattleResult coreResult)
        {
            var drops = new List<BattleDropDto>();
            var groupedEquipmentDrops = coreResult.DroppedEquipments
                .Where(equipment => equipment != null && equipment.EquipmentId > 0)
                .GroupBy(equipment => equipment.EquipmentId);

            foreach (var group in groupedEquipmentDrops)
            {
                var sample = group.First();
                var quality = (int)(sample.Template?.Quality ?? EquipmentQuality.Common);
                drops.Add(new BattleDropDto
                {
                    ItemId = $"equipment_{sample.EquipmentId}",
                    Name = string.IsNullOrWhiteSpace(sample.Name) ? $"Equipment {sample.EquipmentId}" : sample.Name,
                    Quantity = group.Count(),
                    Quality = quality
                });
            }

            return drops;
        }

        private static List<BattleDropDto> MapCollectionDrops(BattleResult coreResult)
        {
            var drops = new List<BattleDropDto>();
            if (coreResult.DroppedCollections == null) return drops;

            var grouped = coreResult.DroppedCollections
                .GroupBy(c => $"{c.SeriesId}_{c.CollectionType}");

            foreach (var group in grouped)
            {
                var sample = group.First();
                var typeName = sample.CollectionType == 0 ? "文字图鉴" : "图片图鉴";
                drops.Add(new BattleDropDto
                {
                    ItemId = $"collection_{sample.SeriesId}",
                    Name = $"{typeName}:{sample.SeriesId}",
                    Quantity = group.Count(),
                    Quality = 0
                });
            }

            return drops;
        }

        /// <summary>
        /// 为单个有资格成员创建独立奖励快照。
        /// </summary>
        private static BattleResult CreateIndependentPartyReward(string playerId, BattleResult aggregateResult)
        {
            var reward = new BattleResult
            {
                IsVictory = aggregateResult.IsVictory,
                ExpGained = aggregateResult.ExpGained,
                GoldGained = aggregateResult.GoldGained,
                MapName = aggregateResult.MapName,
                TotalRounds = aggregateResult.TotalRounds,
                DroppedItems = aggregateResult.DroppedItems.ToList(),
                DroppedCollections = aggregateResult.DroppedCollections.ToList()
            };

            var monsterTemplateIds = aggregateResult.FighterSkillStats.Values
                .SelectMany(stats => stats.KilledMonsterTemplateIds)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToList();
            if (monsterTemplateIds.Count > 0)
            {
                var independent = BattleRewards.CalculateRewardsForMonsterTemplates(monsterTemplateIds);
                reward.DroppedItems = independent.DroppedItems;
                reward.DroppedEquipments = independent.DroppedEquipments;
                reward.DroppedCollections = independent.DroppedCollections;
                reward.ExpGained = independent.ExpGained;
                reward.GoldGained = independent.GoldGained;
            }
            else
            {
                reward.DroppedEquipments = aggregateResult.DroppedEquipments
                    .Select(equipment => equipment == null ? null : BattleRewards.CreateEquipmentFromTemplate(equipment.EquipmentId))
                    .Where(equipment => equipment != null)
                    .Cast<EquipmentInstance>()
                    .ToList();
            }

            return reward;
        }

        private async Task AwardDungeonRewardsAsync(string playerId, BattleResult aggregateResult, List<string> battleLog, DungeonCatalogEntry definition, int memberCount)
        {
            var battleExpBonusPercent = await GetArrayBattleExpBonusPercentAsync(playerId);
            var rewardExp = aggregateResult.ExpGained > 0
                ? FiveElementProgressionRules.ApplyPercentageBonus(aggregateResult.ExpGained, battleExpBonusPercent)
                : 0;

            if (rewardExp > 0)
            {
                await _playerAttributeService.AddExpAsync(playerId, rewardExp, recalculateAttributes: false);
            }

            if (aggregateResult.GoldGained > 0)
            {
                await _playerAttributeService.AddGoldAsync(playerId, aggregateResult.GoldGained, "Dungeon reward", syncRankings: false);
            }

            await GrantPetBattleExpAsync(playerId, rewardExp);
            await SyncItemDropsAsync(playerId, aggregateResult.DroppedItems, battleLog);
            await SyncEquipmentDropsAsync(playerId, aggregateResult.DroppedEquipments, battleLog);
            await SyncCollectionDropsAsync(playerId, aggregateResult.DroppedCollections, battleLog);
            // 中文注释：
            // 副本奖励可能同时发放经验和金币，统一在奖励处理完成后重算一次属性与排行榜。
            if (rewardExp > 0 || aggregateResult.GoldGained > 0)
            {
                await _playerAttributeService.RecalculatePlayerAttributesAsync(playerId, syncLevelDrivenProgress: true);
            }
            if (battleExpBonusPercent > 0 && rewardExp > 0)
            {
                battleLog.Add($"聚灵阵加持：副本经验 +{battleExpBonusPercent}%");
            }
            battleLog.Add($"玩家 {playerId} 已领取 {definition.Name} 通关奖励（队伍人数 {memberCount}）。");
        }

        /// <summary>
        /// 同步道具掉落到背包
        /// 若单个道具入包失败，仅记录日志，不中断整场结算
        /// </summary>
        private async Task SyncItemDropsAsync(string playerId, IEnumerable<ItemTable> droppedItems, List<string> battleLog)
        {
            if (droppedItems == null)
            {
                return;
            }

            var groupedDrops = droppedItems
                .Where(item => item != null && !string.IsNullOrWhiteSpace(item.ItemId))
                .GroupBy(item => item.ItemId);

            foreach (var dropGroup in groupedDrops)
            {
                try
                {
                    await _inventoryService.AddItemAsync(playerId, new AddItemRequestDto
                    {
                        ItemId = dropGroup.Key,
                        Quantity = dropGroup.Count(),
                        Source = "Battle"
                    });
                }
                catch
                {
                    battleLog.Add($"Drop delivery skipped: {dropGroup.Key} x{dropGroup.Count()}");
                }
            }
        }

        /// <summary>
        /// 同步装备掉落到装备实例表
        /// 1. 将核心战斗掉落的 EquipmentInstance 映射为 EquipmentInstanceEntity
        /// 2. 逐条写入数据库，保证单件失败不影响其它奖励结算
        /// </summary>
        private async Task SyncEquipmentDropsAsync(string playerId, IEnumerable<EquipmentInstance> droppedEquipments, List<string> battleLog)
        {
            if (droppedEquipments == null)
            {
                return;
            }

            foreach (var equipment in droppedEquipments.Where(e => e != null))
            {
                try
                {
                    var template = equipment.Template;
                    var templateId = template?.EquipmentId ?? equipment.EquipmentId;
                    if (templateId <= 0)
                    {
                        battleLog.Add("Equipment delivery skipped: invalid template id.");
                        continue;
                    }

                    var entity = EquipmentBalanceHelper.CreateEntity(playerId, equipment, false);
                    if (!await InventoryCapacityRules.HasEquipmentSlotsAsync(_equipmentRepository.Db, playerId))
                    {
                        battleLog.Add($"Equipment delivery skipped: {entity.Name} (装备背包已满)");
                        continue;
                    }

                    await _equipmentRepository.AddAsync(entity);
                    var autoSell = await _equipmentService.AutoSellUnqualifiedEquipmentAsync(
                        playerId,
                        new[] { entity.InstanceId },
                        reason: "副本结算自动出售");
                    if (autoSell.SoldCount > 0)
                    {
                        battleLog.Add($"自动出售装备 {entity.Name}，获得 {autoSell.GoldGained} 金币。");
                    }
                }
                catch
                {
                    var equipmentName = string.IsNullOrWhiteSpace(equipment?.Name)
                        ? $"equipment_{equipment?.EquipmentId ?? 0}"
                        : equipment.Name;
                    battleLog.Add($"Equipment delivery skipped: {equipmentName}");
                }
            }
        }

        /// <summary>
        /// 同步图鉴掉落到玩家图鉴
        /// </summary>
        private async Task SyncCollectionDropsAsync(string playerId, IEnumerable<(string SeriesId, int CollectionType)> droppedCollections, List<string> battleLog)
        {
            if (droppedCollections == null) return;

            foreach (var (seriesId, collectionType) in droppedCollections)
            {
                try
                {
                    await _collectionService.GrantRandomCollectionItemAsync(playerId, seriesId, collectionType);
                    var typeName = collectionType == 0 ? "文字图鉴" : "图片图鉴";
                    battleLog.Add($"获得{typeName}：{seriesId}");
                }
                catch
                {
                    battleLog.Add($"Collection delivery skipped: {seriesId}");
                }
            }
        }

        private sealed class PlayerBattleStats
        {
            public int TotalDamageDealt { get; set; }

            public int TotalDamageTaken { get; set; }

            public int Kills { get; set; }

            public int DeathCount { get; set; }

            public int DodgeCount { get; set; }

            public int CritCount { get; set; }

            public int SkillUseCount { get; set; }

            public int TotalMpConsumed { get; set; }

            public Dictionary<int, int> SkillUsageBySkill { get; set; } = [];

            public Dictionary<int, int> SkillDamageBySkill { get; set; } = [];

            public List<string> KilledMonsterTemplateIds { get; set; } = [];
        }

        private sealed class PveBattleExecutionResult
        {
            public PveBattleExecutionResult(BattleResult coreResult, BattleResultDto? result)
            {
                CoreResult = coreResult;
                Result = result;
            }

            public BattleResult CoreResult { get; }

            public BattleResultDto? Result { get; }
        }

    }
}
