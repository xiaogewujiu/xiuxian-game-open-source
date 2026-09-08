using System.Globalization;
using XXX.Application.DTOs;
using XXX.Entity;

namespace XXX.Application.Services
{
    public partial class WorldBossService
    {
        /// <summary>
        /// 为刚加入战场的玩家创建参战记录。
        /// 这里仅负责持久化参战元信息，真正的战斗属性快照在调用方同时写入运行时战斗态。
        /// </summary>
        /// <param name="runtime">当前世界 Boss 运行时快照。</param>
        /// <param name="player">准备加入战斗的玩家实体。</param>
        /// <returns>新建的参战者记录。</returns>
        private async Task<WorldBossParticipantEntity> CreateParticipantLockedAsync(WorldBossRuntimeSnapshot runtime, UserEntity player)
        {
            var participant = new WorldBossParticipantEntity
            {
                ParticipantId = Guid.NewGuid().ToString("N"),
                InstanceId = runtime.Instance.InstanceId,
                PlayerId = player.GID,
                PlayerName = player.Name,
                IsAuto = false,
                TotalDamage = 0,
                TotalHeal = 0,
                DeathCount = 0,
                JoinAtUtc = DateTime.UtcNow,
                ReadyAtUtc = DateTime.UtcNow,
                DeadlineAtUtc = DateTime.UtcNow.AddSeconds(PlayerManualWindowSeconds),
                LastUpdateTime = DateTime.Now
            };

            await _participantRepository.AddAsync(participant);
            return participant;
        }

        /// <summary>
        /// 按每日排期尝试生成世界 Boss。
        /// 仅当今天尚未刷出实例且当前时间已到达排期时间时才会真正生成。
        /// </summary>
        /// <param name="nowUtc">当前 UTC 时间。</param>
        private async Task TrySpawnScheduledLockedAsync(DateTime nowUtc)
        {
            var schedule = await GetScheduleLockedAsync();
            if (!schedule.IsEnabled)
            {
                return;
            }

            var timezone = ResolveTimeZone(schedule.TimeZoneId);
            var localNow = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, timezone);
            if (!TimeSpan.TryParseExact(schedule.SpawnTimeText, "hh\\:mm", CultureInfo.InvariantCulture, out var spawnTime))
            {
                return;
            }

            if (localNow.TimeOfDay < spawnTime)
            {
                return;
            }

            var localDate = localNow.Date;
            var dayStartUtc = TimeZoneInfo.ConvertTimeToUtc(localDate, timezone);
            var dayEndUtc = TimeZoneInfo.ConvertTimeToUtc(localDate.AddDays(1), timezone);
            var alreadySpawned = await _instanceRepository.Db.Queryable<WorldBossInstanceEntity>()
                .Where(instance => instance.SpawnedAtUtc >= dayStartUtc && instance.SpawnedAtUtc < dayEndUtc)
                .AnyAsync();
            if (alreadySpawned)
            {
                return;
            }

            var template = await SelectBossTemplateLockedAsync(schedule);
            if (template == null)
            {
                return;
            }

            var runtime = await CreateRuntimeFromTemplateLockedAsync(template, nowUtc);
            await SaveRuntimeLockedAsync(runtime);
        }

        /// <summary>
        /// 根据 Boss 模板创建新的运行时实例。
        /// 这里会把模板配置、Boss 战斗初始属性以及第一批日志一并准备好。
        /// </summary>
        /// <param name="template">即将生成的 Boss 模板。</param>
        /// <param name="nowUtc">生成时刻的 UTC 时间。</param>
        /// <returns>完整的运行时快照。</returns>
        private async Task<WorldBossRuntimeSnapshot> CreateRuntimeFromTemplateLockedAsync(WorldBossTemplateEntity template, DateTime nowUtc)
        {
            var bossState = CreateBossState(template, nowUtc.AddSeconds(BossActionIntervalSeconds));
            var runtime = new WorldBossRuntimeSnapshot
            {
                Instance = new WorldBossInstanceEntity
                {
                    InstanceId = Guid.NewGuid().ToString("N"),
                    BossId = template.BossId,
                    BossName = template.Name,
                    MonsterTemplateId = template.MonsterTemplateId,
                    PortraitPath = template.PortraitPath,
                    NoticeText = template.NoticeText,
                    State = WorldBossInstanceState.Active,
                    SpawnedAtUtc = nowUtc,
                    EndAtUtc = nowUtc.AddMinutes(Math.Max(1, template.DurationMinutes)),
                    CurrentHp = bossState.CurrentHp,
                    MaxHp = bossState.MaxHp,
                    CurrentMp = bossState.CurrentMp,
                    MaxMp = bossState.MaxMp,
                    LastUpdateTime = DateTime.Now
                },
                CombatState = new WorldBossCombatState
                {
                    Boss = bossState,
                    Players = []
                },
                Participants = [],
                PendingLogs = [],
                NextLogSeq = 1
            };

            var roundLog = NewRoundLog();
            var context = CreateContext(runtime.CombatState);
            var boss = context.EnemySide.First();
            ApplyPassiveBuffsForFighter(boss, context, roundLog);
            runtime.CombatState = CaptureCombatState(context, runtime.CombatState);
            runtime.CombatState.Boss.NextActionAtUtc = bossState.NextActionAtUtc;

            AppendRoundLog(runtime, roundLog, "buff");
            AppendLog(runtime, "spawn", string.IsNullOrWhiteSpace(template.NoticeText)
                ? $"世界Boss【{template.Name}】已降临。"
                : template.NoticeText!,
                runtime.CombatState.Boss.Id,
                template.Name);

            runtime.Instance.CombatStateJson = SerializeCombatState(runtime.CombatState);
            await _instanceRepository.AddAsync(runtime.Instance);
            return runtime;
        }

        /// <summary>
        /// 处理所有已到复活时间的参战玩家。
        /// 玩家复活时会重置血蓝、清空技能冷却和 Buff，并给一个短暂准备时间避免瞬间再次出手。
        /// </summary>
        /// <param name="runtime">当前运行时快照。</param>
        /// <param name="nowUtc">当前 UTC 时间。</param>
        private Task ProcessRevivesLockedAsync(WorldBossRuntimeSnapshot runtime, DateTime nowUtc)
        {
            var reviveList = runtime.Participants
                .Where(item => item.ReviveAtUtc.HasValue && item.ReviveAtUtc.Value <= nowUtc)
                .ToList();
            if (reviveList.Count == 0)
            {
                return Task.CompletedTask;
            }

            foreach (var participant in reviveList)
            {
                var state = FindPlayerState(runtime.CombatState, participant.PlayerId);
                if (state == null)
                {
                    continue;
                }

                state.CurrentHp = state.MaxHp;
                state.CurrentMp = state.MaxMp;
                state.SkillCooldowns = [];
                state.ActiveBuffs = [];
                state.Mirrors = [];
                participant.ReviveAtUtc = null;
                participant.ReadyAtUtc = nowUtc.AddSeconds(3);
                participant.DeadlineAtUtc = participant.ReadyAtUtc.AddSeconds(PlayerManualWindowSeconds);
                participant.LastUpdateTime = DateTime.Now;
                AppendLog(runtime, "revive", $"{participant.PlayerName} 在世界Boss战中复活归来。", participant.PlayerId, participant.PlayerName);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// 推进所有参战玩家的自动出手逻辑。
        /// 手动窗口超时的玩家会先被切换成自动战斗，再按行动时间顺序依次出手。
        /// </summary>
        /// <param name="runtime">当前运行时快照。</param>
        /// <param name="nowUtc">当前 UTC 时间。</param>
        private Task ProcessParticipantActionsLockedAsync(WorldBossRuntimeSnapshot runtime, DateTime nowUtc)
        {
            foreach (var participant in runtime.Participants
                .OrderBy(item => item.ReadyAtUtc)
                .ThenBy(item => item.JoinAtUtc)
                .ToList())
            {
                if (runtime.CombatState.Boss.CurrentHp <= 0)
                {
                    return Task.CompletedTask;
                }

                if (participant.ReviveAtUtc.HasValue && participant.ReviveAtUtc.Value > nowUtc)
                {
                    continue;
                }

                if (!participant.IsAuto && participant.DeadlineAtUtc <= nowUtc)
                {
                    participant.IsAuto = true;
                    participant.LastUpdateTime = DateTime.Now;
                    AppendLog(runtime, "auto", $"{participant.PlayerName} 超时未操作，已切换为自动战斗。", participant.PlayerId, participant.PlayerName);
                }

                if (!participant.IsAuto || participant.ReadyAtUtc > nowUtc)
                {
                    continue;
                }

                ExecuteActorTurn(runtime, participant.PlayerId, true, null, nowUtc);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// 推进 Boss 的自动出手。
        /// Boss 只要还有存活玩家目标并且到达出手时间，就会执行一轮自动操作。
        /// </summary>
        /// <param name="runtime">当前运行时快照。</param>
        /// <param name="nowUtc">当前 UTC 时间。</param>
        private Task ProcessBossActionLockedAsync(WorldBossRuntimeSnapshot runtime, DateTime nowUtc)
        {
            if (runtime.CombatState.Boss.CurrentHp <= 0 || runtime.CombatState.Boss.NextActionAtUtc > nowUtc)
            {
                return Task.CompletedTask;
            }

            var alivePlayers = runtime.Participants
                .Where(item => !item.ReviveAtUtc.HasValue)
                .Select(item => FindPlayerState(runtime.CombatState, item.PlayerId))
                .Where(item => item != null && item.CurrentHp > 0)
                .ToList();
            if (alivePlayers.Count == 0)
            {
                runtime.CombatState.Boss.NextActionAtUtc = nowUtc.AddSeconds(BossActionIntervalSeconds);
                return Task.CompletedTask;
            }

            ExecuteActorTurn(runtime, runtime.CombatState.Boss.Id, true, null, nowUtc);
            runtime.CombatState.Boss.NextActionAtUtc = nowUtc.AddSeconds(BossActionIntervalSeconds);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 结算当前世界 Boss 实例。
        /// 该方法会写入结算日志、按排名生成奖励记录并把实例状态切换为最终状态。
        /// </summary>
        /// <param name="runtime">待结算的运行时快照。</param>
        /// <param name="reason">结算原因。</param>
        private async Task SettleRuntimeLockedAsync(WorldBossRuntimeSnapshot runtime, string reason)
        {
            AppendLog(runtime, "settle", BuildSettlementMessage(runtime, reason), runtime.CombatState.Boss.Id, runtime.CombatState.Boss.Name);
            await FlushPendingLogsLockedAsync(runtime);

            var template = await _templateRepository.GetByIdAsync(runtime.Instance.BossId);
            var ranking = runtime.Participants
                .OrderByDescending(item => item.TotalDamage)
                .ThenBy(item => item.JoinAtUtc)
                .ToList();

            for (var index = 0; index < ranking.Count; index++)
            {
                var participant = ranking[index];
                var exists = await _rewardRepository.Db.Queryable<WorldBossRewardRecordEntity>()
                    .Where(item => item.InstanceId == runtime.Instance.InstanceId && item.PlayerId == participant.PlayerId)
                    .AnyAsync();
                if (exists)
                {
                    continue;
                }

                var rewardRecord = BuildRewardRecord(runtime.Instance, template, participant, index + 1);
                if (rewardRecord != null)
                {
                    await _rewardRepository.AddAsync(rewardRecord);
                }
            }

            runtime.Instance.State = reason switch
            {
                "worldboss-defeated" => WorldBossInstanceState.Settled,
                "worldboss-expired" => WorldBossInstanceState.Expired,
                _ => WorldBossInstanceState.Closed
            };
            runtime.Instance.SettledAtUtc = DateTime.UtcNow;
            runtime.Instance.LastUpdateTime = DateTime.Now;
            runtime.Instance.CombatStateJson = SerializeCombatState(runtime.CombatState);
            runtime.Instance.CurrentHp = runtime.CombatState.Boss.CurrentHp;
            runtime.Instance.MaxHp = runtime.CombatState.Boss.MaxHp;
            runtime.Instance.CurrentMp = runtime.CombatState.Boss.CurrentMp;
            runtime.Instance.MaxMp = runtime.CombatState.Boss.MaxMp;
            await _instanceRepository.UpdateAsync(runtime.Instance);

            foreach (var participant in runtime.Participants)
            {
                participant.LastUpdateTime = DateTime.Now;
                await _participantRepository.UpdateAsync(participant);
            }
        }

        /// <summary>
        /// 将当前运行时快照完整写回数据库。
        /// 包括实例血蓝、战斗态 JSON、参战者状态和待刷新的日志。
        /// </summary>
        /// <param name="runtime">需要持久化的运行时快照。</param>
        private async Task SaveRuntimeLockedAsync(WorldBossRuntimeSnapshot runtime)
        {
            runtime.Instance.CurrentHp = runtime.CombatState.Boss.CurrentHp;
            runtime.Instance.MaxHp = runtime.CombatState.Boss.MaxHp;
            runtime.Instance.CurrentMp = runtime.CombatState.Boss.CurrentMp;
            runtime.Instance.MaxMp = runtime.CombatState.Boss.MaxMp;
            runtime.Instance.CombatStateJson = SerializeCombatState(runtime.CombatState);
            runtime.Instance.LastUpdateTime = DateTime.Now;
            await _instanceRepository.UpdateAsync(runtime.Instance);

            foreach (var participant in runtime.Participants)
            {
                participant.LastUpdateTime = DateTime.Now;
                await _participantRepository.UpdateAsync(participant);
            }

            await FlushPendingLogsLockedAsync(runtime);
        }

        /// <summary>
        /// 把尚未落库的日志批量写入数据库。
        /// </summary>
        /// <param name="runtime">当前运行时快照。</param>
        private async Task FlushPendingLogsLockedAsync(WorldBossRuntimeSnapshot runtime)
        {
            if (runtime.PendingLogs.Count == 0)
            {
                return;
            }

            foreach (var log in runtime.PendingLogs)
            {
                await _logRepository.AddAsync(log);
            }

            runtime.PendingLogs.Clear();
        }

        /// <summary>
        /// 读取当前激活中的世界 Boss 运行时快照。
        /// 如果没有处于 <see cref="WorldBossInstanceState.Active"/> 的实例，则返回空。
        /// </summary>
        /// <returns>当前激活实例的完整运行时快照。</returns>
        private async Task<WorldBossRuntimeSnapshot?> LoadActiveRuntimeLockedAsync()
        {
            var instance = await _instanceRepository.Db.Queryable<WorldBossInstanceEntity>()
                .Where(item => item.State == WorldBossInstanceState.Active)
                .OrderByDescending(item => item.SpawnedAtUtc)
                .FirstAsync();
            if (instance == null)
            {
                return null;
            }

            var participants = await _participantRepository.Db.Queryable<WorldBossParticipantEntity>()
                .Where(item => item.InstanceId == instance.InstanceId)
                .OrderBy(item => item.JoinAtUtc)
                .ToListAsync();
            var latestLog = await _logRepository.Db.Queryable<WorldBossLogEntity>()
                .Where(item => item.InstanceId == instance.InstanceId)
                .OrderByDescending(item => item.Seq)
                .FirstAsync();

            return new WorldBossRuntimeSnapshot
            {
                Instance = instance,
                CombatState = DeserializeCombatState(instance.CombatStateJson),
                Participants = participants,
                NextLogSeq = latestLog?.Seq + 1 ?? 1
            };
        }

        /// <summary>
        /// 获取默认排期配置。
        /// 数据库为空时会自动补一条默认排期，避免后台页空白。
        /// </summary>
        /// <returns>世界 Boss 排期实体。</returns>
        private async Task<WorldBossScheduleEntity> GetScheduleLockedAsync()
        {
            var schedule = await _scheduleRepository.GetByIdAsync("default");
            if (schedule != null)
            {
                return schedule;
            }

            schedule = new WorldBossScheduleEntity();
            await _scheduleRepository.AddAsync(schedule);
            return schedule;
        }

        /// <summary>
        /// 按当前排期策略选取一个可用的 Boss 模板。
        /// 顺序模式会沿用上次生成记录继续轮换，随机模式则按权重抽取。
        /// </summary>
        /// <param name="schedule">当前排期配置；为空时默认走随机逻辑。</param>
        /// <returns>命中的 Boss 模板；没有启用模板时返回空。</returns>
        private async Task<WorldBossTemplateEntity?> SelectBossTemplateLockedAsync(WorldBossScheduleEntity? schedule)
        {
            var enabledTemplates = await _templateRepository.Db.Queryable<WorldBossTemplateEntity>()
                .Where(item => item.IsEnabled)
                .OrderBy(item => item.SortOrder)
                .OrderBy(item => item.BossId)
                .ToListAsync();
            if (enabledTemplates.Count == 0)
            {
                return null;
            }

            if (schedule?.SelectionMode == WorldBossSelectionMode.Sequential)
            {
                var lastInstance = await _instanceRepository.Db.Queryable<WorldBossInstanceEntity>()
                    .OrderByDescending(item => item.SpawnedAtUtc)
                    .FirstAsync();
                if (lastInstance == null)
                {
                    return enabledTemplates[0];
                }

                var index = enabledTemplates.FindIndex(item => string.Equals(item.BossId, lastInstance.BossId, StringComparison.OrdinalIgnoreCase));
                return index < 0
                    ? enabledTemplates[0]
                    : enabledTemplates[(index + 1) % enabledTemplates.Count];
            }

            var totalWeight = enabledTemplates.Sum(item => Math.Max(1, item.Weight));
            var roll = Random.Shared.Next(totalWeight);
            var current = 0;
            foreach (var template in enabledTemplates)
            {
                current += Math.Max(1, template.Weight);
                if (roll < current)
                {
                    return template;
                }
            }

            return enabledTemplates[^1];
        }

        /// <summary>
        /// 按参与门槛和排名奖励规则生成奖励记录。
        /// 没有任何奖励内容时返回空，避免插入无意义的空记录。
        /// </summary>
        /// <param name="instance">已结算的世界 Boss 实例。</param>
        /// <param name="template">本场使用的 Boss 模板。</param>
        /// <param name="participant">参与结算的玩家记录。</param>
        /// <param name="rank">玩家最终排名。</param>
        /// <returns>奖励记录；本次没有可发奖励时返回空。</returns>
        private static WorldBossRewardRecordEntity? BuildRewardRecord(
            WorldBossInstanceEntity instance,
            WorldBossTemplateEntity? template,
            WorldBossParticipantEntity participant,
            int rank)
        {
            if (template == null)
            {
                return null;
            }

            var rewardExp = 0L;
            var rewardGold = 0L;
            var rewardSpiritStone = 0L;

            if (participant.TotalDamage >= template.ParticipationMinDamage)
            {
                rewardExp += template.ParticipationRewardExp;
                rewardGold += template.ParticipationRewardGold;
                rewardSpiritStone += template.ParticipationRewardSpiritStone;
            }

            if (rank == 1)
            {
                rewardExp += template.Rank1RewardExp;
                rewardGold += template.Rank1RewardGold;
                rewardSpiritStone += template.Rank1RewardSpiritStone;
            }
            else if (rank == 2)
            {
                rewardExp += template.Rank2RewardExp;
                rewardGold += template.Rank2RewardGold;
                rewardSpiritStone += template.Rank2RewardSpiritStone;
            }
            else if (rank == 3)
            {
                rewardExp += template.Rank3RewardExp;
                rewardGold += template.Rank3RewardGold;
                rewardSpiritStone += template.Rank3RewardSpiritStone;
            }

            if (rewardExp <= 0 && rewardGold <= 0 && rewardSpiritStone <= 0)
            {
                return null;
            }

            return new WorldBossRewardRecordEntity
            {
                RecordId = Guid.NewGuid().ToString("N"),
                InstanceId = instance.InstanceId,
                PlayerId = participant.PlayerId,
                PlayerName = participant.PlayerName,
                Rank = rank,
                Damage = participant.TotalDamage,
                RewardExp = rewardExp,
                RewardGold = rewardGold,
                RewardSpiritStone = rewardSpiritStone,
                IsClaimed = false,
                CreateTime = DateTime.UtcNow
            };
        }

        /// <summary>
        /// 根据结算原因生成日志文案。
        /// </summary>
        /// <param name="runtime">当前运行时快照。</param>
        /// <param name="reason">结算原因。</param>
        /// <returns>用于写入结算日志的中文文案。</returns>
        private static string BuildSettlementMessage(WorldBossRuntimeSnapshot runtime, string reason)
        {
            return reason switch
            {
                "worldboss-defeated" => $"世界Boss【{runtime.Instance.BossName}】已被击败。",
                "worldboss-expired" => $"世界Boss【{runtime.Instance.BossName}】活动时间结束。",
                _ => $"世界Boss【{runtime.Instance.BossName}】已被后台关闭。"
            };
        }

        /// <summary>
        /// 世界 Boss 运行时的内存快照对象。
        /// 只在服务内部使用，用于把实例表、参战表、日志缓冲区和战斗态统一组织起来。
        /// </summary>
        private sealed class WorldBossRuntimeSnapshot
        {
            /// <summary>
            /// 当前世界 Boss 实例实体。
            /// </summary>
            public WorldBossInstanceEntity Instance { get; set; } = new();

            /// <summary>
            /// 当前实例的完整战斗态快照。
            /// </summary>
            public WorldBossCombatState CombatState { get; set; } = new();

            /// <summary>
            /// 当前所有参战者的数据库实体集合。
            /// </summary>
            public List<WorldBossParticipantEntity> Participants { get; set; } = [];

            /// <summary>
            /// 尚未落库的日志缓冲区。
            /// 保存运行时过程中新产生的日志，统一在保存阶段刷入数据库。
            /// </summary>
            public List<WorldBossLogEntity> PendingLogs { get; set; } = [];

            /// <summary>
            /// 下一个将要写入日志的递增序号。
            /// </summary>
            public long NextLogSeq { get; set; } = 1;
        }
    }
}
