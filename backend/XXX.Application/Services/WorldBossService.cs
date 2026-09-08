using System.Text.Json;
using Microsoft.Extensions.Logging;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Dungeon;
using XXX.Entity;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    /// <summary>
    /// 世界 Boss 运行时服务。
    /// </summary>
    public partial class WorldBossService : IWorldBossService
    {
        /// <summary>
        /// 玩家每次出手后的固定冷却秒数。
        /// 当前玩法中每 5 秒才能再次行动一次。
        /// </summary>
        private const int PlayerActionCooldownSeconds = 5;

        /// <summary>
        /// 玩家单次手动操作窗口秒数。
        /// 超过这个窗口未操作，就会自动切换到自动战斗。
        /// </summary>
        private const int PlayerManualWindowSeconds = 5;

        /// <summary>
        /// Boss 自动出手的固定间隔秒数。
        /// </summary>
        private const int BossActionIntervalSeconds = 5;

        /// <summary>
        /// 玩家死亡后的复活等待秒数。
        /// </summary>
        private const int PlayerReviveSeconds = 15;

        /// <summary>
        /// 世界 Boss 全局运行时锁。
        /// 同一时刻只允许一个线程推进或改写当前实例，避免并发把战斗态写乱。
        /// </summary>
        private static readonly SemaphoreSlim RuntimeLock = new(1, 1);

        /// <summary>
        /// 世界 Boss 战斗态 JSON 序列化配置。
        /// 运行时快照会落到数据库实例表中，因此这里统一固定序列化选项。
        /// </summary>
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        /// <summary>
        /// 世界 Boss 模板仓储。
        /// </summary>
        private readonly IRepository<WorldBossTemplateEntity> _templateRepository;

        /// <summary>
        /// 世界 Boss 排期仓储。
        /// </summary>
        private readonly IRepository<WorldBossScheduleEntity> _scheduleRepository;

        /// <summary>
        /// 世界 Boss 实例仓储。
        /// </summary>
        private readonly IRepository<WorldBossInstanceEntity> _instanceRepository;

        /// <summary>
        /// 世界 Boss 参战者仓储。
        /// </summary>
        private readonly IRepository<WorldBossParticipantEntity> _participantRepository;

        /// <summary>
        /// 世界 Boss 奖励记录仓储。
        /// </summary>
        private readonly IRepository<WorldBossRewardRecordEntity> _rewardRepository;

        /// <summary>
        /// 世界 Boss 战斗日志仓储。
        /// </summary>
        private readonly IRepository<WorldBossLogEntity> _logRepository;

        /// <summary>
        /// 玩家仓储。
        /// 奖励发放和参战快照都会读取玩家数据。
        /// </summary>
        private readonly IRepository<UserEntity> _userRepository;

        /// <summary>
        /// 玩家属性服务。
        /// 玩家进入世界 Boss 时会先重算属性，奖励发放时也会复用其加经验/金币能力。
        /// </summary>
        private readonly IPlayerAttributeService _playerAttributeService;

        /// <summary>
        /// 世界 Boss 运行日志记录器。
        /// </summary>
        private readonly ILogger<WorldBossService> _logger;

        /// <summary>
        /// 初始化世界 Boss 运行时服务。
        /// </summary>
        /// <param name="templateRepository">世界 Boss 模板仓储。</param>
        /// <param name="scheduleRepository">世界 Boss 排期仓储。</param>
        /// <param name="instanceRepository">世界 Boss 实例仓储。</param>
        /// <param name="participantRepository">世界 Boss 参战者仓储。</param>
        /// <param name="rewardRepository">世界 Boss 奖励记录仓储。</param>
        /// <param name="logRepository">世界 Boss 战斗日志仓储。</param>
        /// <param name="userRepository">玩家仓储。</param>
        /// <param name="playerAttributeService">玩家属性服务。</param>
        /// <param name="logger">日志记录器。</param>
        public WorldBossService(
            IRepository<WorldBossTemplateEntity> templateRepository,
            IRepository<WorldBossScheduleEntity> scheduleRepository,
            IRepository<WorldBossInstanceEntity> instanceRepository,
            IRepository<WorldBossParticipantEntity> participantRepository,
            IRepository<WorldBossRewardRecordEntity> rewardRepository,
            IRepository<WorldBossLogEntity> logRepository,
            IRepository<UserEntity> userRepository,
            IPlayerAttributeService playerAttributeService,
            ILogger<WorldBossService> logger)
        {
            _templateRepository = templateRepository;
            _scheduleRepository = scheduleRepository;
            _instanceRepository = instanceRepository;
            _participantRepository = participantRepository;
            _rewardRepository = rewardRepository;
            _logRepository = logRepository;
            _userRepository = userRepository;
            _playerAttributeService = playerAttributeService;
            _logger = logger;
        }

        /// <summary>
        /// 推进世界 Boss 的一次运行时 Tick。
        /// 负责尝试按排期生成 Boss、推进玩家和 Boss 出手、处理复活以及最终结算。
        /// </summary>
        /// <param name="cancellationToken">后台任务取消令牌。</param>
        public async Task ProcessTickAsync(CancellationToken cancellationToken = default)
        {
            await RuntimeLock.WaitAsync(cancellationToken);
            try
            {
                EnsureGameDataInitialized();

                var nowUtc = DateTime.UtcNow;
                var runtime = await LoadActiveRuntimeLockedAsync();
                if (runtime == null)
                {
                    await TrySpawnScheduledLockedAsync(nowUtc);
                    return;
                }

                await ProcessRevivesLockedAsync(runtime, nowUtc);
                await ProcessParticipantActionsLockedAsync(runtime, nowUtc);
                if (runtime.CombatState.Boss.CurrentHp <= 0)
                {
                    await SettleRuntimeLockedAsync(runtime, "worldboss-defeated");
                    return;
                }

                await ProcessBossActionLockedAsync(runtime, nowUtc);
                if (runtime.CombatState.Boss.CurrentHp <= 0)
                {
                    await SettleRuntimeLockedAsync(runtime, "worldboss-defeated");
                    return;
                }

                if (runtime.Instance.EndAtUtc <= nowUtc)
                {
                    await SettleRuntimeLockedAsync(runtime, "worldboss-expired");
                    return;
                }

                await SaveRuntimeLockedAsync(runtime);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "世界Boss运行时 Tick 执行失败。");
            }
            finally
            {
                RuntimeLock.Release();
            }
        }

        /// <summary>
        /// 获取世界 Boss 当前面板状态。
        /// 玩家端打开弹窗时首先调用这个接口。
        /// </summary>
        /// <param name="playerId">当前查看面板的玩家编号。</param>
        /// <returns>当前 Boss 状态、玩家自身状态和待领奖励信息。</returns>
        public async Task<WorldBossCurrentDto> GetCurrentAsync(string playerId)
        {
            await RuntimeLock.WaitAsync();
            try
            {
                var runtime = await LoadActiveRuntimeLockedAsync();
                var pendingReward = string.IsNullOrWhiteSpace(playerId)
                    ? null
                    : await _rewardRepository.Db.Queryable<WorldBossRewardRecordEntity>()
                        .Where(record => record.PlayerId == playerId && !record.IsClaimed)
                        .OrderByDescending(record => record.CreateTime)
                        .FirstAsync();

                if (runtime == null)
                {
                    return new WorldBossCurrentDto
                    {
                        HasActiveBoss = false,
                        HasPendingReward = pendingReward != null,
                        PendingRewardInstanceId = pendingReward?.InstanceId
                    };
                }

                return BuildCurrentDto(runtime, playerId, pendingReward);
            }
            finally
            {
                RuntimeLock.Release();
            }
        }

        /// <summary>
        /// 让玩家加入当前世界 Boss 战场。
        /// 首次加入时会锁定玩家实时属性快照，后续本场战斗都使用这份快照。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <returns>加入后的当前世界 Boss 状态。</returns>
        public async Task<WorldBossCurrentDto> JoinAsync(string playerId)
        {
            await RuntimeLock.WaitAsync();
            try
            {
                EnsureGameDataInitialized();

                var runtime = await LoadActiveRuntimeLockedAsync();
                if (runtime == null)
                {
                    return new WorldBossCurrentDto
                    {
                        HasActiveBoss = false
                    };
                }

                var participant = runtime.Participants.FirstOrDefault(item => string.Equals(item.PlayerId, playerId, StringComparison.OrdinalIgnoreCase));
                if (participant == null)
                {
                    await _playerAttributeService.RecalculatePlayerAttributesAsync(playerId);
                    var player = await _userRepository.GetByIdAsync(playerId)
                        ?? throw new KeyNotFoundException("玩家不存在。");

                    if (player.BattleMode == BattleMode.OfflineAuto)
                    {
                        throw new InvalidOperationException("离线挂机中无法进入世界Boss。");
                    }

                    participant = await CreateParticipantLockedAsync(runtime, player);
                    runtime.Participants.Add(participant);
                    runtime.CombatState.Players.Add(CreatePlayerState(player, participant.ReadyAtUtc));

                    var roundLog = NewRoundLog();
                    var context = CreateContext(runtime.CombatState);
                    var fighter = context.PlayerSide.First(item => string.Equals(item.Id, player.GID, StringComparison.OrdinalIgnoreCase));
                    ApplyPassiveBuffsForFighter(fighter, context, roundLog);
                    runtime.CombatState = CaptureCombatState(context, runtime.CombatState);
                    AppendRoundLog(runtime, roundLog, "buff");
                    AppendLog(runtime, "join", $"{player.Name} 加入了世界Boss战场。", player.GID, player.Name);
                    await SaveRuntimeLockedAsync(runtime);
                }

                return BuildCurrentDto(runtime, playerId, null);
            }
            finally
            {
                RuntimeLock.Release();
            }
        }

        /// <summary>
        /// 获取世界 Boss 伤害排行榜。
        /// </summary>
        /// <param name="playerId">当前查看排行的玩家编号。</param>
        /// <param name="top">返回的排行数量上限。</param>
        /// <returns>按伤害倒序排列的排行数据。</returns>
        public async Task<List<WorldBossRankingEntryDto>> GetRankingAsync(string playerId, int top = 10)
        {
            await RuntimeLock.WaitAsync();
            try
            {
                var runtime = await LoadActiveRuntimeLockedAsync();
                if (runtime == null)
                {
                    return [];
                }

                return BuildRanking(runtime.Participants, playerId, Math.Max(1, top));
            }
            finally
            {
                RuntimeLock.Release();
            }
        }

        /// <summary>
        /// 获取世界 Boss 战斗日志。
        /// 日志按最新记录优先返回，方便前端倒序展示。
        /// </summary>
        /// <param name="playerId">当前查看日志的玩家编号。</param>
        /// <param name="count">返回的日志条数上限。</param>
        /// <returns>日志列表。</returns>
        public async Task<List<WorldBossLogDto>> GetLogsAsync(string playerId, int count = 100)
        {
            await RuntimeLock.WaitAsync();
            try
            {
                var runtime = await LoadActiveRuntimeLockedAsync();
                if (runtime == null)
                {
                    return [];
                }

                var logs = await _logRepository.Db.Queryable<WorldBossLogEntity>()
                    .Where(log => log.InstanceId == runtime.Instance.InstanceId)
                    .OrderByDescending(log => log.Seq)
                    .Take(Math.Max(1, count))
                    .ToListAsync();

                return logs.Select(MapLog).ToList();
            }
            finally
            {
                RuntimeLock.Release();
            }
        }

        /// <summary>
        /// 执行玩家一次手动出手。
        /// 仅在手动窗口期内允许，超时后会自动切换为自动战斗。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="request">本次出手请求。</param>
        /// <returns>出手结果和最新状态。</returns>
        public async Task<WorldBossActionResultDto> ExecuteActionAsync(string playerId, WorldBossActionRequestDto request)
        {
            await RuntimeLock.WaitAsync();
            try
            {
                EnsureGameDataInitialized();
                var runtime = await LoadActiveRuntimeLockedAsync();
                if (runtime == null)
                {
                    return FailAction("当前没有开启中的世界Boss。");
                }

                var participant = runtime.Participants.FirstOrDefault(item => string.Equals(item.PlayerId, playerId, StringComparison.OrdinalIgnoreCase));
                if (participant == null)
                {
                    return FailAction("请先进入世界Boss弹窗后再出手。");
                }

                var nowUtc = DateTime.UtcNow;
                if (participant.ReviveAtUtc.HasValue && participant.ReviveAtUtc.Value > nowUtc)
                {
                    return FailAction($"角色尚未复活，还需等待 {Math.Max(1, (int)Math.Ceiling((participant.ReviveAtUtc.Value - nowUtc).TotalSeconds))} 秒。");
                }

                if (participant.ReadyAtUtc > nowUtc)
                {
                    return FailAction($"行动冷却中，还需等待 {Math.Max(1, (int)Math.Ceiling((participant.ReadyAtUtc - nowUtc).TotalSeconds))} 秒。");
                }

                if (participant.DeadlineAtUtc < nowUtc)
                {
                    participant.IsAuto = true;
                    participant.LastUpdateTime = DateTime.Now;
                    await _participantRepository.UpdateAsync(participant);
                    return FailAction("本次手动窗口已过，已自动切换为自动战斗。");
                }

                var actionResult = ExecuteManualActionLocked(runtime, playerId, request, nowUtc);
                if (actionResult.Success)
                {
                    await SaveRuntimeLockedAsync(runtime);
                }

                return actionResult;
            }
            finally
            {
                RuntimeLock.Release();
            }
        }

        /// <summary>
        /// 切换玩家的自动战斗状态。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="enabled">是否开启自动战斗。</param>
        /// <returns>切换后的玩家状态。</returns>
        public async Task<WorldBossParticipantStatusDto> ToggleAutoAsync(string playerId, bool enabled)
        {
            await RuntimeLock.WaitAsync();
            try
            {
                var runtime = await LoadActiveRuntimeLockedAsync();
                if (runtime == null)
                {
                    throw new InvalidOperationException("当前没有开启中的世界Boss。");
                }

                var participant = runtime.Participants.FirstOrDefault(item => string.Equals(item.PlayerId, playerId, StringComparison.OrdinalIgnoreCase));
                if (participant == null)
                {
                    throw new InvalidOperationException("请先进入世界Boss战场。");
                }

                participant.IsAuto = enabled;
                participant.LastUpdateTime = DateTime.Now;
                await _participantRepository.UpdateAsync(participant);
                return BuildParticipantStatus(runtime, playerId)
                    ?? throw new InvalidOperationException("世界Boss角色状态不存在。");
            }
            finally
            {
                RuntimeLock.Release();
            }
        }

        /// <summary>
        /// 立即生成一个世界 Boss。
        /// 后台“随机生成”或“生成当前模板”时会调用这个入口。
        /// </summary>
        /// <param name="bossId">指定的 Boss 模板编号；为空时按排期规则选取。</param>
        /// <returns>生成后的当前世界 Boss 状态。</returns>
        public async Task<WorldBossCurrentDto> SpawnNowAsync(string? bossId = null)
        {
            await RuntimeLock.WaitAsync();
            try
            {
                EnsureGameDataInitialized();

                var existing = await LoadActiveRuntimeLockedAsync();
                if (existing != null)
                {
                    return BuildCurrentDto(existing, string.Empty, null);
                }

                var template = !string.IsNullOrWhiteSpace(bossId)
                    ? await _templateRepository.GetByIdAsync(bossId.Trim())
                    : await SelectBossTemplateLockedAsync(null);
                if (template == null || !template.IsEnabled)
                {
                    throw new InvalidOperationException("没有可生成的世界Boss模板。");
                }

                var runtime = await CreateRuntimeFromTemplateLockedAsync(template, DateTime.UtcNow);
                await SaveRuntimeLockedAsync(runtime);
                return BuildCurrentDto(runtime, string.Empty, null);
            }
            finally
            {
                RuntimeLock.Release();
            }
        }

        /// <summary>
        /// 关闭当前运行中的世界 Boss。
        /// 实际上会走一次完整结算，把奖励记录落库后再切换实例状态。
        /// </summary>
        /// <param name="reason">关闭原因。</param>
        /// <returns>存在可关闭实例时返回真。</returns>
        public async Task<bool> CloseCurrentAsync(string? reason = null)
        {
            await RuntimeLock.WaitAsync();
            try
            {
                var runtime = await LoadActiveRuntimeLockedAsync();
                if (runtime == null)
                {
                    return false;
                }

                await SettleRuntimeLockedAsync(runtime, string.IsNullOrWhiteSpace(reason) ? "worldboss-closed" : reason.Trim());
                return true;
            }
            finally
            {
                RuntimeLock.Release();
            }
        }

        /// <summary>
        /// 领取玩家未领取的世界 Boss 奖励。
        /// 奖励会立刻写回玩家经验、金币和灵石。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <returns>奖励领取结果；没有待领奖励时返回空。</returns>
        public async Task<WorldBossRewardClaimDto?> ClaimRewardAsync(string playerId)
        {
            await RuntimeLock.WaitAsync();
            try
            {
                var reward = await _rewardRepository.Db.Queryable<WorldBossRewardRecordEntity>()
                    .Where(record => record.PlayerId == playerId && !record.IsClaimed)
                    .OrderByDescending(record => record.CreateTime)
                    .FirstAsync();
                if (reward == null)
                {
                    return null;
                }

                if (reward.RewardExp > 0)
                {
                    await _playerAttributeService.AddExpAsync(playerId, reward.RewardExp);
                }

                if (reward.RewardGold > 0)
                {
                    await _playerAttributeService.AddGoldAsync(playerId, reward.RewardGold, "世界Boss奖励");
                }

                if (reward.RewardSpiritStone > 0)
                {
                    var user = await _userRepository.GetByIdAsync(playerId)
                        ?? throw new KeyNotFoundException("玩家不存在。");
                    user.SpiritStone += reward.RewardSpiritStone;
                    user.LastUpdateTime = DateTime.Now;
                    await _userRepository.UpdateAsync(user);
                }

                reward.IsClaimed = true;
                reward.ClaimedAtUtc = DateTime.UtcNow;
                await _rewardRepository.UpdateAsync(reward);

                var participant = await _participantRepository.Db.Queryable<WorldBossParticipantEntity>()
                    .Where(item => item.InstanceId == reward.InstanceId && item.PlayerId == playerId)
                    .FirstAsync();
                if (participant != null)
                {
                    participant.RewardClaimed = true;
                    participant.LastUpdateTime = DateTime.Now;
                    await _participantRepository.UpdateAsync(participant);
                }

                return new WorldBossRewardClaimDto
                {
                    Success = true,
                    Message = "奖励领取成功。",
                    InstanceId = reward.InstanceId,
                    Rank = reward.Rank,
                    Damage = reward.Damage,
                    RewardExp = reward.RewardExp,
                    RewardGold = reward.RewardGold,
                    RewardSpiritStone = reward.RewardSpiritStone
                };
            }
            finally
            {
                RuntimeLock.Release();
            }
        }

        /// <summary>
        /// 校验世界 Boss 运行时依赖的数据缓存是否已经从数据库完成加载。
        /// 当前战斗逻辑依赖怪物模板、技能模板、Buff 模板和副本目录的运行时缓存，任何一项缺失都会导致出手异常。
        /// </summary>
        private static void EnsureGameDataInitialized()
        {
            if (global::XXX.GameData.MonsterTemplates.Count > 0 &&
                global::XXX.GameData.Items.Count > 0 &&
                global::XXX.SkillData.HasLoadedRuntimeTemplates &&
                global::XXX.BuffDataTemplates.HasLoadedRuntimeTemplates &&
                DungeonCatalog.HasLoadedRuntimeEntries)
            {
                return;
            }

            throw new InvalidOperationException("运行时模板缓存尚未从数据库加载完成。");
        }

        /// <summary>
        /// 归一化前端传入的动作类型。
        /// 当前只区分技能和普通攻击，其他值一律按普通攻击处理。
        /// </summary>
        /// <param name="actionType">前端提交的动作类型文本。</param>
        /// <returns>标准化后的动作类型标识。</returns>
        private static string NormalizeActionType(string? actionType)
        {
            return string.Equals(actionType?.Trim(), "skill", StringComparison.OrdinalIgnoreCase)
                ? "skill"
                : "normal";
        }

        /// <summary>
        /// 构造一次失败的出手结果。
        /// </summary>
        /// <param name="message">失败原因。</param>
        /// <returns>统一格式的失败结果 DTO。</returns>
        private static WorldBossActionResultDto FailAction(string message)
        {
            return new WorldBossActionResultDto
            {
                Success = false,
                Message = message,
                Logs = []
            };
        }
    }
}
