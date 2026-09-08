using System.Text.Json;
using XXX.Application.DTOs;
using XXX.Battle;
using XXX.Entity;

namespace XXX.Application.Services
{
    public partial class WorldBossService
    {
        /// <summary>
        /// 执行玩家主动点击发起的一次手动出手。
        /// 该路径会严格校验技能可用性，并把本次行动产生的日志和状态回写到运行时快照中。
        /// </summary>
        /// <param name="runtime">当前运行时快照。</param>
        /// <param name="playerId">出手玩家编号。</param>
        /// <param name="request">前端提交的手动出手请求。</param>
        /// <param name="nowUtc">当前 UTC 时间。</param>
        /// <returns>本次手动出手结果。</returns>
        private WorldBossActionResultDto ExecuteManualActionLocked(
            WorldBossRuntimeSnapshot runtime,
            string playerId,
            WorldBossActionRequestDto request,
            DateTime nowUtc)
        {
            var participant = runtime.Participants.First(item => string.Equals(item.PlayerId, playerId, StringComparison.OrdinalIgnoreCase));
            var roundLog = NewRoundLog();
            var context = CreateContext(runtime.CombatState);
            var actor = context.PlayerSide.FirstOrDefault(item => string.Equals(item.Id, playerId, StringComparison.OrdinalIgnoreCase));
            if (actor == null)
            {
                return FailAction("当前角色未在Boss战状态中。");
            }

            ProcessTurnStart(actor, context, roundLog);
            if (actor.CurrentHp <= 0)
            {
                runtime.CombatState = CaptureCombatState(context, runtime.CombatState);
                MarkDefeatedParticipantsLockedAsync(runtime, nowUtc).GetAwaiter().GetResult();
                AppendRoundLog(runtime, roundLog, "round-start");
                return FailAction("角色已在回合开始时倒下。");
            }

            var actionSuccess = ExecutePlayerRequestedAction(context, actor, request, roundLog);
            if (!actionSuccess)
            {
                return FailAction("当前操作无法执行，可能是技能冷却、灵力不足或状态限制。");
            }

            FinalizeTurn(actor);
            participant.LastActionAtUtc = nowUtc;
            participant.ReadyAtUtc = nowUtc.AddSeconds(PlayerActionCooldownSeconds);
            participant.DeadlineAtUtc = participant.ReadyAtUtc.AddSeconds(PlayerManualWindowSeconds);
            participant.LastUpdateTime = DateTime.Now;

            runtime.CombatState = CaptureCombatState(context, runtime.CombatState);
            ApplyActionStats(runtime, context.Result.FighterSkillStats);
            MarkDefeatedParticipantsLockedAsync(runtime, nowUtc).GetAwaiter().GetResult();
            AppendRoundLog(runtime, roundLog, NormalizeActionType(request.ActionType));

            var self = BuildParticipantStatus(runtime, playerId);
            var boss = BuildBossDto(runtime.CombatState.Boss, runtime.Instance.PortraitPath);
            var latestLogs = runtime.PendingLogs.Select(MapLog).ToList();

            return new WorldBossActionResultDto
            {
                Success = true,
                Message = "出手成功。",
                Self = self,
                Boss = boss,
                Logs = latestLogs
            };
        }

        /// <summary>
        /// 执行一次战斗单位回合。
        /// 既可用于玩家自动战斗，也可用于 Boss 自动出手。
        /// </summary>
        /// <param name="runtime">当前运行时快照。</param>
        /// <param name="actorId">出手战斗单位编号。</param>
        /// <param name="sequentialAuto">自动战斗时是否按装备顺序依次尝试技能。</param>
        /// <param name="manualRequest">手动请求；为空时按自动战斗规则执行。</param>
        /// <param name="nowUtc">当前 UTC 时间。</param>
        private void ExecuteActorTurn(
            WorldBossRuntimeSnapshot runtime,
            string actorId,
            bool sequentialAuto,
            WorldBossActionRequestDto? manualRequest,
            DateTime nowUtc)
        {
            var roundLog = NewRoundLog();
            var context = CreateContext(runtime.CombatState);
            var actor = context.PlayerSide.Concat(context.EnemySide)
                .FirstOrDefault(item => string.Equals(item.Id, actorId, StringComparison.OrdinalIgnoreCase));
            if (actor == null || actor.CurrentHp <= 0)
            {
                return;
            }

            ProcessTurnStart(actor, context, roundLog);
            if (actor.CurrentHp <= 0)
            {
                runtime.CombatState = CaptureCombatState(context, runtime.CombatState);
                MarkDefeatedParticipantsLockedAsync(runtime, nowUtc).GetAwaiter().GetResult();
                AppendRoundLog(runtime, roundLog, "round-start");
                return;
            }

            var actionSuccess = manualRequest == null
                ? ExecuteAutoAction(context, actor, roundLog, sequentialAuto)
                : ExecutePlayerRequestedAction(context, actor, manualRequest, roundLog);
            if (!actionSuccess && manualRequest == null)
            {
                ExecuteFallbackNormalAttack(context, actor, roundLog);
            }

            FinalizeTurn(actor);
            runtime.CombatState = CaptureCombatState(context, runtime.CombatState);
            ApplyActionStats(runtime, context.Result.FighterSkillStats);
            MarkDefeatedParticipantsLockedAsync(runtime, nowUtc).GetAwaiter().GetResult();
            AppendRoundLog(runtime, roundLog, manualRequest == null ? "auto" : NormalizeActionType(manualRequest.ActionType));

            var participant = runtime.Participants.FirstOrDefault(item => string.Equals(item.PlayerId, actorId, StringComparison.OrdinalIgnoreCase));
            if (participant != null)
            {
                participant.LastActionAtUtc = nowUtc;
                participant.ReadyAtUtc = nowUtc.AddSeconds(PlayerActionCooldownSeconds);
                participant.DeadlineAtUtc = participant.ReadyAtUtc.AddSeconds(PlayerManualWindowSeconds);
                participant.LastUpdateTime = DateTime.Now;
            }
        }

        /// <summary>
        /// 执行自动战斗逻辑。
        /// </summary>
        /// <param name="context">当前战斗上下文。</param>
        /// <param name="actor">当前出手者。</param>
        /// <param name="roundLog">本回合日志。</param>
        /// <param name="sequentialAuto">是否按装备顺序尝试技能。</param>
        /// <returns>本次自动出手是否成功执行。</returns>
        private bool ExecuteAutoAction(BattleContext context, BattleFighter actor, RoundLog roundLog, bool sequentialAuto)
        {
            return ExecuteActionInternal(context, actor, roundLog, null, sequentialAuto);
        }

        /// <summary>
        /// 执行玩家指定动作的手动战斗逻辑。
        /// </summary>
        /// <param name="context">当前战斗上下文。</param>
        /// <param name="actor">当前出手玩家。</param>
        /// <param name="request">玩家指定的动作请求。</param>
        /// <param name="roundLog">本回合日志。</param>
        /// <returns>动作是否成功执行。</returns>
        private bool ExecutePlayerRequestedAction(BattleContext context, BattleFighter actor, WorldBossActionRequestDto request, RoundLog roundLog)
        {
            return ExecuteActionInternal(context, actor, roundLog, request, true);
        }

        /// <summary>
        /// 统一处理战斗单位的一次行动决策。
        /// 该方法会处理魅惑、嘲讽、沉默、眩晕、技能尝试和普通攻击兜底等规则。
        /// </summary>
        /// <param name="context">当前战斗上下文。</param>
        /// <param name="fighter">当前出手战斗单位。</param>
        /// <param name="roundLog">本回合日志。</param>
        /// <param name="request">玩家手动请求；为空时走自动逻辑。</param>
        /// <param name="sequentialAuto">自动战斗时是否按顺序释放技能。</param>
        /// <returns>本次行动是否成功执行。</returns>
        private bool ExecuteActionInternal(
            BattleContext context,
            BattleFighter fighter,
            RoundLog roundLog,
            WorldBossActionRequestDto? request,
            bool sequentialAuto)
        {
            var enemies = fighter.IsPlayerSide
                ? context.EnemySide.Where(item => item.CurrentHp > 0).ToList()
                : context.PlayerSide.Where(item => item.CurrentHp > 0).ToList();
            var allies = fighter.IsPlayerSide ? context.PlayerSide.ToList() : context.EnemySide.ToList();
            var aliveAllies = allies.Where(item => item.CurrentHp > 0).ToList();
            if (enemies.Count == 0)
            {
                return false;
            }

            if (BuffProcessor.IsCharmed(fighter))
            {
                new BattleLogBuilder(roundLog, context, BattleLogType.Charm)
                    .Target(fighter)
                    .Description($"{fighter.Name} 受到[魅惑]影响，转而攻击友方单位。")
                    .Build();
                enemies = aliveAllies.Where(item => item != fighter).ToList();
                if (enemies.Count == 0)
                {
                    return false;
                }
            }

            var tauntTarget = BattleHelper.GetTauntTarget(fighter);
            if (tauntTarget != null && tauntTarget.CurrentHp > 0)
            {
                new BattleLogBuilder(roundLog, context, BattleLogType.BuffApply)
                    .Caster(fighter)
                    .Target(tauntTarget)
                    .Description($"{fighter.Name} 受到[嘲讽]影响，被迫攻击 {tauntTarget.Name}。")
                    .Build();
                NormalAttackExecutor.ExecuteNormalAttack(fighter, tauntTarget, context, roundLog);
                return true;
            }

            if (fighter.IsMirror)
            {
                return ExecuteFallbackNormalAttack(context, fighter, roundLog);
            }

            if (BattleHelper.IsStunned(fighter))
            {
                new BattleLogBuilder(roundLog, context, BattleLogType.Stun)
                    .Target(fighter)
                    .Description($"{fighter.Name} 受到[眩晕]影响，无法行动。")
                    .Build();
                return false;
            }

            if (request != null && NormalizeActionType(request.ActionType) == "skill")
            {
                return TryUseSpecifiedSkill(context, fighter, enemies, allies, roundLog, request.SkillId);
            }

            if (request == null)
            {
                var usedSkill = SkillExecutor.TryUseSkill(
                    context,
                    fighter,
                    enemies,
                    allies,
                    roundLog,
                    sequentialAuto ? SkillReleaseMode.Sequential : SkillReleaseMode.Smart);
                if (usedSkill)
                {
                    return true;
                }
            }

            return ExecuteFallbackNormalAttack(context, fighter, roundLog);
        }

        /// <summary>
        /// 在无法释放技能时执行普通攻击兜底逻辑。
        /// 若出手者被缴械，则本次行动会直接失败。
        /// </summary>
        /// <param name="context">当前战斗上下文。</param>
        /// <param name="fighter">当前出手战斗单位。</param>
        /// <param name="roundLog">本回合日志。</param>
        /// <returns>普通攻击是否成功执行。</returns>
        private bool ExecuteFallbackNormalAttack(BattleContext context, BattleFighter fighter, RoundLog roundLog)
        {
            if (BattleHelper.IsDisarmed(fighter))
            {
                new BattleLogBuilder(roundLog, context, BattleLogType.Disarm)
                    .Target(fighter)
                    .Description($"{fighter.Name} 受到[缴械]影响，无法使用普通攻击。")
                    .Build();
                return false;
            }

            var enemies = fighter.IsPlayerSide
                ? context.EnemySide.Where(item => item.CurrentHp > 0).ToList()
                : context.PlayerSide.Where(item => item.CurrentHp > 0).ToList();
            var target = TargetSelector.SelectAttackTarget(enemies);
            if (target == null)
            {
                return false;
            }

            NormalAttackExecutor.ExecuteNormalAttack(fighter, target, context, roundLog);
            return true;
        }

        /// <summary>
        /// 尝试释放玩家指定的某个技能。
        /// 会依次校验技能存在、是否已装备、沉默、冷却、蓝量和触发成功率。
        /// </summary>
        /// <param name="context">当前战斗上下文。</param>
        /// <param name="fighter">当前出手战斗单位。</param>
        /// <param name="enemies">敌方存活目标列表。</param>
        /// <param name="allies">己方目标列表。</param>
        /// <param name="roundLog">本回合日志。</param>
        /// <param name="skillId">前端指定的技能编号。</param>
        /// <returns>技能是否成功释放。</returns>
        private bool TryUseSpecifiedSkill(
            BattleContext context,
            BattleFighter fighter,
            List<BattleFighter> enemies,
            List<BattleFighter> allies,
            RoundLog roundLog,
            int? skillId)
        {
            if (!skillId.HasValue || !SkillData.Skills.TryGetValue(skillId.Value, out var skill))
            {
                return false;
            }

            if (!fighter.SkillIds.Contains(skillId.Value.ToString(), StringComparer.Ordinal))
            {
                return false;
            }

            if (BattleHelper.IsSilenced(fighter))
            {
                new BattleLogBuilder(roundLog, context, BattleLogType.Silence)
                    .Target(fighter)
                    .Description($"{fighter.Name} 受到[沉默]影响，无法使用技能。")
                    .Build();
                return false;
            }

            if (fighter.SkillCooldowns.TryGetValue(skill.Id, out var cooldown) && cooldown > 0)
            {
                return false;
            }

            if (fighter.CurrentMp < skill.ManaCost)
            {
                return false;
            }

            if (Random.Shared.NextDouble() > skill.TriggerChance)
            {
                new BattleLogBuilder(roundLog, context, BattleLogType.SkillUse)
                    .Caster(fighter)
                    .Skill(skill)
                    .Description($"{fighter.Name} 试图施放[{skill.Name}]，但未能成功引动。")
                    .Build();
                return false;
            }

            SkillExecutor.UseSkill(context, fighter, skill, enemies, allies, roundLog);
            return true;
        }

        /// <summary>
        /// 执行回合开始时的通用处理。
        /// 主要负责重置回合内计数并结算回合开始触发的 Buff。
        /// </summary>
        /// <param name="fighter">当前出手战斗单位。</param>
        /// <param name="context">当前战斗上下文。</param>
        /// <param name="roundLog">本回合日志。</param>
        private void ProcessTurnStart(BattleFighter fighter, BattleContext context, RoundLog roundLog)
        {
            fighter.ComboCountThisRound = 0;
            fighter.CounterCountPerAttacker.Clear();
            BuffProcessor.ProcessBuffsAtRoundStart(fighter, context, roundLog);
        }

        /// <summary>
        /// 执行回合结束时的通用清理。
        /// 这里会结算 Buff 持续回合并推进技能冷却。
        /// </summary>
        /// <param name="fighter">刚完成行动的战斗单位。</param>
        private static void FinalizeTurn(BattleFighter fighter)
        {
            foreach (var buff in fighter.ActiveBuffs.ToList())
            {
                if (buff.RemainingDuration == -1)
                {
                    continue;
                }

                if (buff.IsNewlyApplied)
                {
                    buff.IsNewlyApplied = false;
                    continue;
                }

                buff.RemainingDuration--;
                if (buff.RemainingDuration <= 0)
                {
                    fighter.ActiveBuffs.Remove(buff);
                }
            }

            var cooldownModifier = BuffProcessor.GetCooldownModifier(fighter);
            foreach (var skillId in fighter.SkillCooldowns.Keys.ToList())
            {
                if (fighter.SkillCooldowns[skillId] <= 0 || fighter.SkillsUsedThisRound.Contains(skillId))
                {
                    continue;
                }

                var reduction = (int)Math.Ceiling(cooldownModifier) + 1;
                if (reduction >= 0)
                {
                    fighter.SkillCooldowns[skillId] = Math.Max(0, fighter.SkillCooldowns[skillId] - reduction);
                }
                else
                {
                    fighter.SkillCooldowns[skillId]--;
                }
            }

            fighter.SkillsUsedThisRound.Clear();
        }

        /// <summary>
        /// 标记并处理所有刚刚被击倒的玩家。
        /// 玩家进入死亡状态后会写入复活时间，并追加一条死亡日志。
        /// </summary>
        /// <param name="runtime">当前运行时快照。</param>
        /// <param name="nowUtc">当前 UTC 时间。</param>
        private Task MarkDefeatedParticipantsLockedAsync(WorldBossRuntimeSnapshot runtime, DateTime nowUtc)
        {
            foreach (var participant in runtime.Participants)
            {
                if (participant.ReviveAtUtc.HasValue)
                {
                    continue;
                }

                var playerState = FindPlayerState(runtime.CombatState, participant.PlayerId);
                if (playerState == null || playerState.CurrentHp > 0)
                {
                    continue;
                }

                participant.ReviveAtUtc = nowUtc.AddSeconds(PlayerReviveSeconds);
                participant.DeathCount += 1;
                participant.LastUpdateTime = DateTime.Now;
                AppendLog(runtime, "dead", $"{participant.PlayerName} 倒下了，将在 {PlayerReviveSeconds} 秒后重新归来。", participant.PlayerId, participant.PlayerName);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// 把战斗引擎统计出来的伤害/治疗结果累计到参战记录上。
        /// </summary>
        /// <param name="runtime">当前运行时快照。</param>
        /// <param name="fighterStats">战斗引擎输出的单位统计表。</param>
        private void ApplyActionStats(WorldBossRuntimeSnapshot runtime, Dictionary<string, FighterSkillStats> fighterStats)
        {
            if (fighterStats == null || fighterStats.Count == 0)
            {
                return;
            }

            foreach (var participant in runtime.Participants)
            {
                if (!fighterStats.TryGetValue(participant.PlayerId, out var stats))
                {
                    continue;
                }

                participant.TotalDamage += stats.TotalDamageDealt;
                participant.TotalHeal += stats.TotalHealingDone;
                participant.LastUpdateTime = DateTime.Now;
            }
        }

        /// <summary>
        /// 根据 Boss 模板和怪物模板创建 Boss 初始战斗状态。
        /// 怪物模板中的属性范围会在这里随机一次，生成本场 Boss 的具体数值。
        /// </summary>
        /// <param name="template">Boss 模板。</param>
        /// <param name="nextActionAtUtc">Boss 下一次允许出手的 UTC 时间。</param>
        /// <returns>Boss 初始战斗状态。</returns>
        private static WorldBossFighterState CreateBossState(WorldBossTemplateEntity template, DateTime nextActionAtUtc)
        {
            if (!global::XXX.GameData.MonsterTemplates.TryGetValue(template.MonsterTemplateId, out var monsterTemplate))
            {
                throw new InvalidOperationException($"怪物模板不存在：{template.MonsterTemplateId}");
            }

            var element = monsterTemplate.ElementPool.Count > 0
                ? monsterTemplate.ElementPool[Random.Shared.Next(0, monsterTemplate.ElementPool.Count)]
                : monsterTemplate.Element ?? Element.None;

            var maxHp = RandomRange(monsterTemplate.MinType1, monsterTemplate.MaxType1);
            var maxMp = RandomRange(monsterTemplate.MinType2, monsterTemplate.MaxType2);

            return new WorldBossFighterState
            {
                Id = $"worldboss_{Guid.NewGuid():N}",
                Name = template.Name,
                NextActionAtUtc = nextActionAtUtc,
                IsPlayerSide = false,
                FighterType = FighterType.Monster,
                MonsterTemplateId = monsterTemplate.GID,
                CurrentHp = maxHp,
                MaxHp = maxHp,
                CurrentMp = maxMp,
                MaxMp = maxMp,
                PhysicalAttack = RandomRange(monsterTemplate.MinType3, monsterTemplate.MaxType3),
                MagicAttack = RandomRange(monsterTemplate.MinType4, monsterTemplate.MaxType4),
                PhysicalDefense = RandomRange(monsterTemplate.MinType5, monsterTemplate.MaxType5),
                MagicDefense = RandomRange(monsterTemplate.MinType6, monsterTemplate.MaxType6),
                Speed = RandomRange(monsterTemplate.MinType7, monsterTemplate.MaxType7),
                HitRate = RandomRange(monsterTemplate.MinType8, monsterTemplate.MaxType8) / 100f,
                DodgeRate = RandomRange(monsterTemplate.MinType9, monsterTemplate.MaxType9) / 100f,
                CritRate = RandomRange(monsterTemplate.MinType10, monsterTemplate.MaxType10) / 100f,
                CritDamage = RandomRange(monsterTemplate.MinType11, monsterTemplate.MaxType11) / 100f,
                ComboRate = RandomRange(monsterTemplate.MinType12, monsterTemplate.MaxType12) / 100f,
                CounterRate = RandomRange(monsterTemplate.MinType13, monsterTemplate.MaxType13) / 100f,
                ArmorBreak = RandomRange(monsterTemplate.MinType14, monsterTemplate.MaxType14) / 100f,
                ExtraDamage = RandomRange(monsterTemplate.MinType15, monsterTemplate.MaxType15) / 100f,
                Element = element,
                SkillIds = monsterTemplate.SkillIds.ToList(),
                PassiveIds = monsterTemplate.PassiveIds.ToList(),
                SkillCooldowns = [],
                ActiveBuffs = [],
                Mirrors = []
            };
        }

        /// <summary>
        /// 根据玩家实体创建进入世界 Boss 时的战斗状态快照。
        /// 这里会把玩家当前重算后的最终属性直接固化下来。
        /// </summary>
        /// <param name="user">玩家实体。</param>
        /// <param name="readyAtUtc">玩家下一次允许出手的 UTC 时间。</param>
        /// <returns>玩家初始战斗状态。</returns>
        private static WorldBossFighterState CreatePlayerState(UserEntity user, DateTime readyAtUtc)
        {
            return new WorldBossFighterState
            {
                Id = user.GID,
                Name = user.Name,
                NextActionAtUtc = readyAtUtc,
                IsPlayerSide = true,
                FighterType = FighterType.Player,
                CurrentHp = user.Type1,
                MaxHp = user.Type1,
                CurrentMp = user.Type2,
                MaxMp = user.Type2,
                PhysicalAttack = user.Type3,
                MagicAttack = user.Type4,
                PhysicalDefense = user.Type5,
                MagicDefense = user.Type6,
                Speed = user.Type7,
                HitRate = user.Type8,
                DodgeRate = user.Type9,
                CritRate = user.Type10,
                CritDamage = user.Type11,
                ComboRate = user.Type12,
                CounterRate = user.Type13,
                ArmorBreak = user.Type14,
                ExtraDamage = user.Type15,
                Element = user.Element,
                SkillIds = user.SkillIds.ToList(),
                PassiveIds = user.PassiveIds.ToList(),
                SkillCooldowns = [],
                ActiveBuffs = [],
                Mirrors = []
            };
        }

        /// <summary>
        /// 在怪物模板的最小值和最大值之间随机一个整数。
        /// </summary>
        /// <param name="min">最小值。</param>
        /// <param name="max">最大值。</param>
        /// <returns>随机后的整数值。</returns>
        private static int RandomRange(int? min, int? max)
        {
            var minValue = min ?? 0;
            var maxValue = max ?? minValue;
            if (maxValue < minValue)
            {
                maxValue = minValue;
            }

            return Random.Shared.Next(minValue, maxValue + 1);
        }

        /// <summary>
        /// 解析后台配置的时区标识。
        /// 配置非法时回退到服务器本地时区。
        /// </summary>
        /// <param name="timeZoneId">后台保存的时区标识。</param>
        /// <returns>可用的时区对象。</returns>
        private static TimeZoneInfo ResolveTimeZone(string? timeZoneId)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(timeZoneId))
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId.Trim());
                }
            }
            catch
            {
            }

            return TimeZoneInfo.Local;
        }

        /// <summary>
        /// 将世界 Boss 战斗态序列化为 JSON。
        /// </summary>
        /// <param name="state">待序列化的战斗态。</param>
        /// <returns>序列化后的 JSON 文本。</returns>
        private static string SerializeCombatState(WorldBossCombatState state)
            => JsonSerializer.Serialize(state, JsonOptions);

        /// <summary>
        /// 从数据库中的 JSON 文本还原世界 Boss 战斗态。
        /// 数据为空或反序列化失败时会回退到空战斗态。
        /// </summary>
        /// <param name="json">数据库保存的战斗态 JSON。</param>
        /// <returns>反序列化后的战斗态对象。</returns>
        private static WorldBossCombatState DeserializeCombatState(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new WorldBossCombatState();
            }

            try
            {
                return JsonSerializer.Deserialize<WorldBossCombatState>(json, JsonOptions) ?? new WorldBossCombatState();
            }
            catch
            {
                return new WorldBossCombatState();
            }
        }

        /// <summary>
        /// 根据世界 Boss 战斗态创建战斗引擎上下文。
        /// 这里会把 Boss、玩家、镜像和 Buff 都恢复成战斗引擎可直接运行的对象。
        /// </summary>
        /// <param name="combatState">持久化的世界 Boss 战斗态。</param>
        /// <returns>可直接交给战斗引擎使用的上下文对象。</returns>
        private static BattleContext CreateContext(WorldBossCombatState combatState)
        {
            var context = new BattleContext
            {
                Map = new Map
                {
                    MapGId = "worldboss",
                    Name = "世界Boss",
                    MonsterCount = (1, 1)
                },
                EnableElementAdvantage = true
            };

            var fighterMap = new Dictionary<string, BattleFighter>(StringComparer.OrdinalIgnoreCase);
            var playerFighters = combatState.Players.Select(state => ToBattleFighter(state, fighterMap)).ToList();
            var bossFighter = ToBattleFighter(combatState.Boss, fighterMap);

            foreach (var playerState in combatState.Players)
            {
                ResolveFighterLinks(playerState, fighterMap);
            }

            ResolveFighterLinks(combatState.Boss, fighterMap);
            context.PlayerSide.AddRange(playerFighters);
            context.EnemySide.Add(bossFighter);
            return context;
        }

        /// <summary>
        /// 把世界 Boss 战斗态中的单个单位转换为战斗引擎对象。
        /// 镜像分身会递归一起转换。
        /// </summary>
        /// <param name="state">战斗态单位快照。</param>
        /// <param name="fighterMap">战斗单位映射表，用于后续恢复 Buff 来源和镜像关系。</param>
        /// <returns>战斗引擎中的战斗单位对象。</returns>
        private static BattleFighter ToBattleFighter(WorldBossFighterState state, IDictionary<string, BattleFighter> fighterMap)
        {
            var fighter = new BattleFighter
            {
                Id = state.Id,
                Name = state.Name,
                MonsterTempID = state.MonsterTemplateId,
                IsPlayerSide = state.IsPlayerSide,
                FighterType = state.FighterType,
                CurrentHp = state.CurrentHp,
                MaxHp = state.MaxHp,
                CurrentMp = state.CurrentMp,
                MaxMp = state.MaxMp,
                PhysicalAttack = state.PhysicalAttack,
                MagicAttack = state.MagicAttack,
                PhysicalDefense = state.PhysicalDefense,
                MagicDefense = state.MagicDefense,
                Speed = state.Speed,
                HitRate = state.HitRate,
                DodgeRate = state.DodgeRate,
                CritRate = state.CritRate,
                CritDamage = state.CritDamage,
                ComboRate = state.ComboRate,
                CounterRate = state.CounterRate,
                ArmorBreak = state.ArmorBreak,
                ExtraDamage = state.ExtraDamage,
                SkillIds = state.SkillIds.ToList(),
                PassiveIds = state.PassiveIds.ToList(),
                SkillCooldowns = state.SkillCooldowns.ToDictionary(item => item.Key, item => item.Value),
                Element = state.Element,
                IsMirror = state.IsMirror,
                MirrorDuration = state.MirrorDuration,
                MirrorAttackRatio = state.MirrorAttackRatio,
                MirrorHpRatio = state.MirrorHpRatio,
                ActiveBuffs = []
            };

            fighterMap[fighter.Id] = fighter;
            foreach (var mirrorState in state.Mirrors)
            {
                var mirror = ToBattleFighter(mirrorState, fighterMap);
                fighter.Mirrors.Add(mirror);
            }

            return fighter;
        }

        /// <summary>
        /// 恢复战斗单位之间的引用关系。
        /// 包括 Buff 来源、镜像归属等不能直接靠简单字段还原的关系。
        /// </summary>
        /// <param name="state">战斗态单位快照。</param>
        /// <param name="fighterMap">已构建好的战斗单位映射表。</param>
        private static void ResolveFighterLinks(WorldBossFighterState state, IDictionary<string, BattleFighter> fighterMap)
        {
            if (!fighterMap.TryGetValue(state.Id, out var fighter))
            {
                return;
            }

            fighter.ActiveBuffs = state.ActiveBuffs
                .Where(buff => !string.IsNullOrWhiteSpace(buff.BuffId) && BuffDataTemplates.BuffTemplates.ContainsKey(buff.BuffId))
                .Select(buff => new ActiveBuff
                {
                    Template = BuffDataTemplates.BuffTemplates[buff.BuffId],
                    RemainingDuration = buff.RemainingDuration,
                    CurrentStack = buff.CurrentStack,
                    Source = !string.IsNullOrWhiteSpace(buff.SourceId) && fighterMap.TryGetValue(buff.SourceId, out var source)
                        ? source
                        : null,
                    IsNewlyApplied = buff.IsNewlyApplied,
                    ShieldValue = buff.ShieldValue
                })
                .ToList();

            if (state.IsMirror && !string.IsNullOrWhiteSpace(state.MirrorOwnerId) && fighterMap.TryGetValue(state.MirrorOwnerId, out var owner))
            {
                fighter.MirrorOwner = owner;
            }

            foreach (var mirrorState in state.Mirrors)
            {
                ResolveFighterLinks(mirrorState, fighterMap);
            }
        }

        /// <summary>
        /// 从战斗引擎上下文重新捕获一份可持久化的世界 Boss 战斗态。
        /// </summary>
        /// <param name="context">战斗引擎上下文。</param>
        /// <param name="previousState">上一份持久化战斗态，用于沿用无法从引擎反推的字段。</param>
        /// <returns>最新的世界 Boss 战斗态快照。</returns>
        private static WorldBossCombatState CaptureCombatState(BattleContext context, WorldBossCombatState? previousState)
        {
            var previousMap = new Dictionary<string, WorldBossFighterState>(StringComparer.OrdinalIgnoreCase);
            if (previousState != null)
            {
                CollectStateMap(previousState.Boss, previousMap);
                foreach (var playerState in previousState.Players)
                {
                    CollectStateMap(playerState, previousMap);
                }
            }

            return new WorldBossCombatState
            {
                Boss = CaptureFighterState(context.EnemySide.First(), previousMap),
                Players = context.PlayerSide
                    .Where(item => !item.IsMirror)
                    .Select(item => CaptureFighterState(item, previousMap))
                    .ToList()
            };
        }

        /// <summary>
        /// 收集一份战斗态树形结构中的所有单位快照，方便按编号回查旧状态。
        /// </summary>
        /// <param name="state">当前遍历到的战斗单位状态。</param>
        /// <param name="map">状态索引表。</param>
        private static void CollectStateMap(WorldBossFighterState state, IDictionary<string, WorldBossFighterState> map)
        {
            map[state.Id] = state;
            foreach (var mirror in state.Mirrors)
            {
                CollectStateMap(mirror, map);
            }
        }

        /// <summary>
        /// 把战斗引擎中的单个单位重新落成世界 Boss 持久化快照。
        /// </summary>
        /// <param name="fighter">战斗引擎中的战斗单位。</param>
        /// <param name="previousMap">上一份快照索引表。</param>
        /// <returns>新的战斗单位快照。</returns>
        private static WorldBossFighterState CaptureFighterState(BattleFighter fighter, IDictionary<string, WorldBossFighterState> previousMap)
        {
            previousMap.TryGetValue(fighter.Id, out var previousState);
            return new WorldBossFighterState
            {
                Id = fighter.Id,
                Name = fighter.Name,
                NextActionAtUtc = previousState?.NextActionAtUtc ?? DateTime.UtcNow,
                IsPlayerSide = fighter.IsPlayerSide,
                FighterType = fighter.FighterType,
                MonsterTemplateId = fighter.MonsterTempID,
                CurrentHp = fighter.CurrentHp,
                MaxHp = fighter.MaxHp,
                CurrentMp = fighter.CurrentMp,
                MaxMp = fighter.MaxMp,
                PhysicalAttack = fighter.PhysicalAttack,
                MagicAttack = fighter.MagicAttack,
                PhysicalDefense = fighter.PhysicalDefense,
                MagicDefense = fighter.MagicDefense,
                Speed = fighter.Speed,
                HitRate = fighter.HitRate,
                DodgeRate = fighter.DodgeRate,
                CritRate = fighter.CritRate,
                CritDamage = fighter.CritDamage,
                ComboRate = fighter.ComboRate,
                CounterRate = fighter.CounterRate,
                ArmorBreak = fighter.ArmorBreak,
                ExtraDamage = fighter.ExtraDamage,
                Element = fighter.Element,
                SkillIds = fighter.SkillIds.ToList(),
                PassiveIds = fighter.PassiveIds.ToList(),
                SkillCooldowns = fighter.SkillCooldowns.ToDictionary(item => item.Key, item => item.Value),
                ActiveBuffs = fighter.ActiveBuffs.Select(buff => new WorldBossBuffState
                {
                    BuffId = buff.Template.Gid,
                    RemainingDuration = buff.RemainingDuration,
                    CurrentStack = buff.CurrentStack,
                    SourceId = buff.Source?.Id,
                    IsNewlyApplied = buff.IsNewlyApplied,
                    ShieldValue = buff.ShieldValue
                }).ToList(),
                IsMirror = fighter.IsMirror,
                MirrorOwnerId = fighter.MirrorOwner?.Id,
                MirrorDuration = fighter.MirrorDuration,
                MirrorAttackRatio = fighter.MirrorAttackRatio,
                MirrorHpRatio = fighter.MirrorHpRatio,
                Mirrors = fighter.Mirrors.Select(mirror => CaptureFighterState(mirror, previousMap)).ToList()
            };
        }

        /// <summary>
        /// 从当前战斗态中查找指定玩家的状态快照。
        /// </summary>
        /// <param name="combatState">当前世界 Boss 战斗态。</param>
        /// <param name="playerId">玩家编号。</param>
        /// <returns>玩家状态快照；未参战时返回空。</returns>
        private static WorldBossFighterState? FindPlayerState(WorldBossCombatState combatState, string playerId)
        {
            return combatState.Players.FirstOrDefault(item => string.Equals(item.Id, playerId, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 给战斗单位挂载进入战斗时应立即生效的被动 Buff。
        /// 目前玩家和 Boss 在创建战斗态后都会走这一步。
        /// </summary>
        /// <param name="fighter">需要挂载被动的战斗单位。</param>
        /// <param name="context">当前战斗上下文。</param>
        /// <param name="log">用于记录被动施加过程的回合日志。</param>
        private static void ApplyPassiveBuffsForFighter(BattleFighter fighter, BattleContext context, RoundLog log)
        {
            if (fighter.IsMirror || fighter.PassiveIds == null || fighter.PassiveIds.Count == 0)
            {
                return;
            }

            foreach (var passiveId in fighter.PassiveIds)
            {
                if (!BuffDataTemplates.BuffTemplates.TryGetValue(passiveId, out var template))
                {
                    continue;
                }

                var targets = GetPassiveTargets(fighter, template, context);
                BuffProcessor.ApplyBuffWithTargetSelection(fighter, null, template, targets, context, log);
            }
        }

        /// <summary>
        /// 根据被动 Buff 的首个效果推导其目标集合。
        /// 世界 Boss 当前只需要支持自施放、敌方全体和友方全体这几种常见情况。
        /// </summary>
        /// <param name="caster">被动效果施加者。</param>
        /// <param name="template">Buff 模板。</param>
        /// <param name="context">当前战斗上下文。</param>
        /// <returns>本次被动效果的目标列表。</returns>
        private static List<BattleFighter> GetPassiveTargets(BattleFighter caster, BuffTemplate template, BattleContext context)
        {
            if (template.Effects == null || template.Effects.Count == 0)
            {
                return [];
            }

            var firstEffect = template.Effects[0];
            if (firstEffect.TargetSelfOnly)
            {
                return [caster];
            }

            if (firstEffect.TargetCamp == 1)
            {
                return caster.IsPlayerSide
                    ? context.EnemySide.Where(item => item.CurrentHp > 0).ToList()
                    : context.PlayerSide.Where(item => item.CurrentHp > 0).ToList();
            }

            if (firstEffect.TargetCamp == 2)
            {
                return caster.IsPlayerSide
                    ? context.PlayerSide.Where(item => item.CurrentHp > 0).ToList()
                    : context.EnemySide.Where(item => item.CurrentHp > 0).ToList();
            }

            return [];
        }

        /// <summary>
        /// 新建一份世界 Boss 回合日志容器。
        /// 当前世界 Boss 前端不展示回合号，所以统一固定为 1，只复用其动作集合。
        /// </summary>
        /// <returns>新的回合日志对象。</returns>
        private static RoundLog NewRoundLog()
        {
            return new RoundLog
            {
                RoundNumber = 1
            };
        }

        /// <summary>
        /// 把本次回合日志中的动作文本逐条追加到世界 Boss 日志缓冲区。
        /// </summary>
        /// <param name="runtime">当前运行时快照。</param>
        /// <param name="roundLog">战斗引擎生成的回合日志。</param>
        /// <param name="actionType">归档到世界 Boss 日志时使用的动作类型。</param>
        private static void AppendRoundLog(WorldBossRuntimeSnapshot runtime, RoundLog roundLog, string actionType)
        {
            foreach (var action in roundLog.Actions.Where(action => !string.IsNullOrWhiteSpace(action)))
            {
                AppendLog(runtime, actionType, action, null, null);
            }
        }

        /// <summary>
        /// 追加一条世界 Boss 运行时日志到待落库缓冲区。
        /// </summary>
        /// <param name="runtime">当前运行时快照。</param>
        /// <param name="actionType">动作类型标识。</param>
        /// <param name="content">日志正文。</param>
        /// <param name="actorId">动作发起者编号。</param>
        /// <param name="actorName">动作发起者名称。</param>
        /// <param name="targetId">动作目标编号。</param>
        /// <param name="targetName">动作目标名称。</param>
        private static void AppendLog(
            WorldBossRuntimeSnapshot runtime,
            string actionType,
            string content,
            string? actorId,
            string? actorName,
            string? targetId = null,
            string? targetName = null)
        {
            runtime.PendingLogs.Add(new WorldBossLogEntity
            {
                LogId = Guid.NewGuid().ToString("N"),
                InstanceId = runtime.Instance.InstanceId,
                Seq = runtime.NextLogSeq++,
                TimestampUtc = DateTime.UtcNow,
                ActorId = actorId,
                ActorName = actorName,
                TargetId = targetId,
                TargetName = targetName,
                ActionType = actionType,
                Content = content
            });
        }
    }
}
