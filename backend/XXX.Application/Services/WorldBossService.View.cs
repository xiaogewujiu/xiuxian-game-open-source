using XXX.Application.DTOs;
using XXX.Battle;
using XXX.Entity;

namespace XXX.Application.Services
{
    public partial class WorldBossService
    {
        /// <summary>
        /// 组装玩家端世界 Boss 当前面板 DTO。
        /// </summary>
        /// <param name="runtime">当前运行时快照。</param>
        /// <param name="playerId">当前查看面板的玩家编号。</param>
        /// <param name="pendingReward">玩家尚未领取的奖励记录。</param>
        /// <returns>玩家端世界 Boss 当前状态 DTO。</returns>
        private WorldBossCurrentDto BuildCurrentDto(
            WorldBossRuntimeSnapshot runtime,
            string playerId,
            WorldBossRewardRecordEntity? pendingReward)
        {
            return new WorldBossCurrentDto
            {
                HasActiveBoss = true,
                HasPendingReward = pendingReward != null,
                PendingRewardInstanceId = pendingReward?.InstanceId,
                Instance = new WorldBossInstanceDto
                {
                    InstanceId = runtime.Instance.InstanceId,
                    BossId = runtime.Instance.BossId,
                    BossName = runtime.Instance.BossName,
                    PortraitPath = runtime.Instance.PortraitPath,
                    State = GetInstanceStateText(runtime.Instance.State),
                    SpawnedAtUtc = runtime.Instance.SpawnedAtUtc,
                    EndAtUtc = runtime.Instance.EndAtUtc,
                    RemainingSeconds = Math.Max(0, (int)Math.Ceiling((runtime.Instance.EndAtUtc - DateTime.UtcNow).TotalSeconds)),
                    ParticipantCount = runtime.Participants.Count
                },
                Boss = BuildBossDto(runtime.CombatState.Boss, runtime.Instance.PortraitPath),
                Self = string.IsNullOrWhiteSpace(playerId) ? null : BuildParticipantStatus(runtime, playerId)
            };
        }

        /// <summary>
        /// 把 Boss 战斗状态转换为前端展示 DTO。
        /// </summary>
        /// <param name="state">Boss 当前战斗状态。</param>
        /// <param name="portraitPath">Boss 画像路径。</param>
        /// <returns>Boss 展示 DTO。</returns>
        private WorldBossBossDto BuildBossDto(WorldBossFighterState state, string? portraitPath)
        {
            return new WorldBossBossDto
            {
                FighterId = state.Id,
                Name = state.Name,
                PortraitPath = portraitPath,
                CurrentHp = state.CurrentHp,
                MaxHp = state.MaxHp,
                CurrentMp = state.CurrentMp,
                MaxMp = state.MaxMp,
                Attributes = BuildAttributes(state),
                Skills = BuildSkillDtos(state)
            };
        }

        /// <summary>
        /// 构建指定玩家在当前世界 Boss 中的个人战斗状态。
        /// </summary>
        /// <param name="runtime">当前运行时快照。</param>
        /// <param name="playerId">玩家编号。</param>
        /// <returns>玩家状态 DTO；未参战时返回空。</returns>
        private WorldBossParticipantStatusDto? BuildParticipantStatus(WorldBossRuntimeSnapshot runtime, string playerId)
        {
            var participant = runtime.Participants.FirstOrDefault(item => string.Equals(item.PlayerId, playerId, StringComparison.OrdinalIgnoreCase));
            var state = FindPlayerState(runtime.CombatState, playerId);
            if (participant == null || state == null)
            {
                return null;
            }

            var ranking = runtime.Participants
                .OrderByDescending(item => item.TotalDamage)
                .ThenBy(item => item.JoinAtUtc)
                .ToList();
            var rank = ranking.FindIndex(item => string.Equals(item.PlayerId, playerId, StringComparison.OrdinalIgnoreCase)) + 1;
            var nowUtc = DateTime.UtcNow;
            return new WorldBossParticipantStatusDto
            {
                PlayerId = participant.PlayerId,
                PlayerName = participant.PlayerName,
                IsJoined = true,
                IsAuto = participant.IsAuto,
                CanAct = !participant.ReviveAtUtc.HasValue &&
                    participant.ReadyAtUtc <= nowUtc &&
                    participant.DeadlineAtUtc >= nowUtc,
                IsDead = participant.ReviveAtUtc.HasValue || state.CurrentHp <= 0,
                CurrentHp = state.CurrentHp,
                MaxHp = state.MaxHp,
                CurrentMp = state.CurrentMp,
                MaxMp = state.MaxMp,
                ReadyAtUtc = participant.ReadyAtUtc,
                DeadlineAtUtc = participant.DeadlineAtUtc,
                ReviveAtUtc = participant.ReviveAtUtc,
                SecondsToReady = Math.Max(0, (int)Math.Ceiling((participant.ReadyAtUtc - nowUtc).TotalSeconds)),
                SecondsToDeadline = Math.Max(0, (int)Math.Ceiling((participant.DeadlineAtUtc - nowUtc).TotalSeconds)),
                SecondsToRevive = participant.ReviveAtUtc.HasValue
                    ? Math.Max(0, (int)Math.Ceiling((participant.ReviveAtUtc.Value - nowUtc).TotalSeconds))
                    : 0,
                TotalDamage = participant.TotalDamage,
                Rank = Math.Max(0, rank),
                Skills = BuildSkillDtos(state)
            };
        }

        /// <summary>
        /// 将战斗单位的数值快照转换为前端属性展示 DTO。
        /// </summary>
        /// <param name="state">战斗单位状态。</param>
        /// <returns>属性展示 DTO。</returns>
        private static WorldBossCombatAttributesDto BuildAttributes(WorldBossFighterState state)
        {
            return new WorldBossCombatAttributesDto
            {
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
                Element = (int)state.Element,
                ElementText = state.Element.ToString()
            };
        }

        /// <summary>
        /// 把战斗单位携带的技能列表转换成前端技能栏数据。
        /// </summary>
        /// <param name="state">战斗单位状态。</param>
        /// <returns>技能展示 DTO 列表。</returns>
        private static List<WorldBossBattleSkillDto> BuildSkillDtos(WorldBossFighterState state)
        {
            var result = new List<WorldBossBattleSkillDto>();
            foreach (var skillIdText in state.SkillIds)
            {
                if (!int.TryParse(skillIdText, out var skillId) || !SkillData.Skills.TryGetValue(skillId, out var skill))
                {
                    continue;
                }

                state.SkillCooldowns.TryGetValue(skillId, out var currentCooldown);
                result.Add(new WorldBossBattleSkillDto
                {
                    SkillId = skill.Id,
                    Name = skill.Name,
                    Description = skill.Description,
                    ManaCost = skill.ManaCost,
                    Cooldown = skill.Cooldown,
                    CurrentCooldown = Math.Max(0, currentCooldown),
                    Icon = GetSkillIcon(skill.DamageType),
                    CanUse = currentCooldown <= 0 && state.CurrentMp >= skill.ManaCost
                });
            }

            return result;
        }

        /// <summary>
        /// 根据技能伤害类型返回前端展示图标。
        /// </summary>
        /// <param name="damageType">技能伤害类型。</param>
        /// <returns>技能图标文本。</returns>
        private static string GetSkillIcon(DamageType damageType)
        {
            return damageType switch
            {
                DamageType.Physical => "⚔️",
                DamageType.Magic => "🔮",
                DamageType.Heal => "💚",
                DamageType.Buff => "✨",
                DamageType.Revive => "🌿",
                DamageType.True => "💥",
                _ => "✦"
            };
        }

        /// <summary>
        /// 构建世界 Boss 伤害排行榜数据。
        /// </summary>
        /// <param name="participants">当前参战者集合。</param>
        /// <param name="playerId">当前查看排行的玩家编号。</param>
        /// <param name="top">返回的排行数量上限。</param>
        /// <returns>排行榜 DTO 列表。</returns>
        private static List<WorldBossRankingEntryDto> BuildRanking(IEnumerable<WorldBossParticipantEntity> participants, string playerId, int top)
        {
            return participants
                .OrderByDescending(item => item.TotalDamage)
                .ThenBy(item => item.JoinAtUtc)
                .Take(top)
                .Select((item, index) => new WorldBossRankingEntryDto
                {
                    Rank = index + 1,
                    PlayerId = item.PlayerId,
                    PlayerName = item.PlayerName,
                    TotalDamage = item.TotalDamage,
                    IsSelf = !string.IsNullOrWhiteSpace(playerId) && string.Equals(item.PlayerId, playerId, StringComparison.OrdinalIgnoreCase)
                })
                .ToList();
        }

        /// <summary>
        /// 将日志实体转换为前端日志 DTO。
        /// </summary>
        /// <param name="entity">日志实体。</param>
        /// <returns>日志 DTO。</returns>
        private static WorldBossLogDto MapLog(WorldBossLogEntity entity)
        {
            return new WorldBossLogDto
            {
                Seq = entity.Seq,
                TimestampUtc = entity.TimestampUtc,
                ActionType = entity.ActionType,
                Content = entity.Content
            };
        }

        /// <summary>
        /// 将世界 Boss 实例状态转换为前端展示的中文文本。
        /// </summary>
        /// <param name="state">实例状态枚举。</param>
        /// <returns>中文状态文案。</returns>
        private static string GetInstanceStateText(WorldBossInstanceState state)
        {
            return state switch
            {
                WorldBossInstanceState.Pending => "待开始",
                WorldBossInstanceState.Active => "进行中",
                WorldBossInstanceState.Settled => "已结算",
                WorldBossInstanceState.Closed => "已关闭",
                WorldBossInstanceState.Expired => "已结束",
                _ => "未知"
            };
        }
    }
}
